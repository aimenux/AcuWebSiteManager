using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Base MFG Capacity Graph
    /// </summary>
    /// <typeparam name="TPrimary"></typeparam>
    /// <typeparam name="TDetail"></typeparam>
    public abstract class CapacityInqBase<TPrimary, TDetail> : PXGraph
        where TPrimary : CapacityFilterBase, IBqlTable, new()
        where TDetail : class, IBqlTable, ISchd, ICapacityUtilization, new()
    {
        public PXCancel<TPrimary> Cancel;
        public PXFilter<TPrimary> CapacityFilter;
        public PXSelect<TDetail> CapacityDetail;

        public CapacityInqBase()
        {
            CapacityDetail.AllowInsert =
                CapacityDetail.AllowDelete = false;

            FieldDefaulting.AddHandler(typeof(TPrimary), nameof(CapacityFilterBase.FromDate), FilterFromDateFieldDefaulting);
            FieldDefaulting.AddHandler(typeof(TPrimary), nameof(CapacityFilterBase.ToDate), FilterToDateFieldDefaulting);
        }

        protected virtual void FilterFromDateFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = sender.Graph.Accessinfo.BusinessDate;
        }

        protected virtual void FilterToDateFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = sender.Graph.Accessinfo.BusinessDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
        }

        public PXAction<TPrimary> ViewSchedule;
        [PXUIField(DisplayName = "View Schedule")]
        [PXButton]
        protected virtual void viewSchedule()
        {
            if (CapacityDetail?.Current?.ResourceID == null)
            {
                return;
            }

            RedirectToSchedule(CapacityDetail.Current.ResourceID, CapacityDetail.Current.FromDate, CapacityDetail.Current.ToDate);
        }

        protected virtual void RedirectToSchedule(string resourceId, DateTime? fromDate, DateTime? toDate)
        {
            var gi = new GIWorkCenterSchedule();
            gi.SetParameter(GIWorkCenterSchedule.Parameters.WorkCenter, resourceId);
            gi.SetParameter(GIWorkCenterSchedule.Parameters.DateFrom, fromDate, this);
            gi.SetParameter(GIWorkCenterSchedule.Parameters.DateTo, toDate, this);
            gi.CallGenericInquiry(PXBaseRedirectException.WindowMode.New);
        }

        protected abstract IEnumerable<TDetail> GetDetail();

        protected virtual IEnumerable capacityDetail()
        {
            var detail = GetDetail().ToList();

            TimeBucket timeBucket = null;
            SchdTimeBucket<TDetail> schdTimeBucket = null;
            for (var i = 0; i < detail.Count; i++)
            {
                var result = detail[i];

                if (result == null || string.IsNullOrWhiteSpace(result.ResourceID) || result.SchdDate == null)
                {
                    continue;
                }

                if (timeBucket == null)
                {
                    timeBucket = CalculateTimeBucket(result.SchdDate.GetValueOrDefault(), CapacityFilter.Current);
                }

                if (schdTimeBucket == null)
                {
                    schdTimeBucket = new SchdTimeBucket<TDetail>(result.ResourceID, timeBucket.StartDate, timeBucket.EndDate);
                }

                if (result.ResourceID.EqualsWithTrim(schdTimeBucket.SchdRecord.ResourceID)
                    && result.SchdDate.GetValueOrDefault().BetweenInclusive(schdTimeBucket.StartDate, schdTimeBucket.EndDate))
                {
                    schdTimeBucket.Add(result);
                }
                else
                {
                    yield return schdTimeBucket.SchdRecord;

                    timeBucket = CalculateTimeBucket(result.SchdDate.GetValueOrDefault(), CapacityFilter.Current);
                    schdTimeBucket = new SchdTimeBucket<TDetail>(result.ResourceID, timeBucket.StartDate, timeBucket.EndDate);
                    schdTimeBucket.Add(result);
                }

                //make sure we add the last row...
                if (i == detail.Count - 1)
                {
                    yield return schdTimeBucket.SchdRecord;
                }
            }
        }

        protected virtual TimeBucket CalculateTimeBucket(DateTime date, TPrimary capacityFilter)
        {
            if (capacityFilter == null)
            {
                throw new ArgumentNullException(nameof(capacityFilter));
            }

            var baseDate = capacityFilter.FromDate.GetValueOrDefault(Common.Dates.Today);
            var startDate = date;
            var endDate = date;
            switch (capacityFilter.CapacityRange)
            {
                case DateRangeInq.Weekly:
                    startDate = date.PreviousDateOf(baseDate.DayOfWeek);
                    endDate = EndDateAddDays(capacityFilter.CapacityRange, startDate);
                    break;
                case DateRangeInq.Biweekly:
                    int diff = (date.Date - baseDate.Date).Days;
                    startDate = date.AddDays(diff % 2 * -1).PreviousDateOf(baseDate.DayOfWeek);
                    endDate = EndDateAddDays(capacityFilter.CapacityRange, startDate);
                    break;
                case DateRangeInq.Monthly:
                    startDate = new DateTime(date.Year, date.Month, baseDate.Day);
                    endDate = EndDateAddDays(capacityFilter.CapacityRange, startDate);
                    break;
            }

            return new TimeBucket(startDate, endDate);
        }

        protected static DateTime EndDateAddDays(string capacityRange, DateTime date)
        {
            switch (capacityRange)
            {
                case DateRangeInq.Weekly:
                    return date.AddDays(6);
                case DateRangeInq.Biweekly:
                    return date.AddDays(13);
                case DateRangeInq.Monthly:
                    return date.AddMonths(1).AddDays(-1);
                default:
                    return date;
            }
        }

        protected class TimeBucket
        {
            public DateTime StartDate { get; private set; }
            public DateTime EndDate { get; private set; }

            public TimeBucket(DateTime startDate, DateTime endDate)
            {
                StartDate = startDate;
                EndDate = endDate;
            }
        }

        protected class SchdTimeBucket<TSchd> : TimeBucket
            where TSchd : class, IBqlTable, ISchd, ICapacityUtilization, new()
        {
            public TSchd SchdRecord { get; private set; }

            public SchdTimeBucket(string resourceId, DateTime startDate, DateTime endDate) :
                base(startDate, endDate)
            {
                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    throw new ArgumentNullException(nameof(resourceId));
                }

                SchdRecord = new TSchd
                {
                    ResourceID = resourceId,
                    SchdDate = startDate,
                    FromDate = startDate,
                    ToDate = endDate
                };
            }

            public void Add(ISchd s)
            {
                if (s == null
                    || string.IsNullOrWhiteSpace(s.ResourceID)
                    || !s.ResourceID.EqualsWithTrim(s.ResourceID)
                    || s.SchdDate == null
                    || !s.SchdDate.GetValueOrDefault().BetweenInclusive(StartDate, EndDate))
                {
                    return;
                }

                SchdRecord.WorkTime = SchdRecord.WorkTime.GetValueOrDefault() + s.WorkTime.GetValueOrDefault();
                SchdRecord.TotalBlocks = SchdRecord.TotalBlocks.GetValueOrDefault() + s.TotalBlocks.GetValueOrDefault();
                SchdRecord.PlanBlocks = SchdRecord.PlanBlocks.GetValueOrDefault() + s.PlanBlocks.GetValueOrDefault();
                SchdRecord.SchdTime = SchdRecord.SchdTime.GetValueOrDefault() + s.SchdTime.GetValueOrDefault();
                SchdRecord.AvailableBlocks = SchdRecord.AvailableBlocks.GetValueOrDefault() + s.AvailableBlocks.GetValueOrDefault();
                SchdRecord.SchdEfficiencyTime = SchdRecord.SchdEfficiencyTime.GetValueOrDefault() + s.SchdEfficiencyTime.GetValueOrDefault();
                SchdRecord.SchdBlocks = SchdRecord.SchdBlocks.GetValueOrDefault() + s.SchdBlocks.GetValueOrDefault();

                SchdRecord.PlanUtilizationPct = 0;
                SchdRecord.SchdUtilizationPct = 0;

                if (SchdRecord.TotalBlocks.GetValueOrDefault() > 0)
                {
                    SchdRecord.PlanUtilizationPct = 100 * SchdRecord.PlanBlocks.GetValueOrDefault() / (decimal)SchdRecord.TotalBlocks.GetValueOrDefault();
                    SchdRecord.SchdUtilizationPct = 100 * SchdRecord.SchdBlocks.GetValueOrDefault() / (decimal)SchdRecord.TotalBlocks.GetValueOrDefault();
                }
            }
        }
    }

    public interface ICapacityUtilization
    {
        decimal? PlanUtilizationPct { get; set; }
        decimal? SchdUtilizationPct { get; set; }
        DateTime? FromDate { get; set; }
        DateTime? ToDate { get; set; }
    }

    [Serializable]
    [PXCacheName("Capacity Filter")]
    public class CapacityFilterBase : IBqlTable
    {
        #region ResourceID
        public abstract class resourceID : PX.Data.BQL.BqlString.Field<resourceID> { }

        protected string _ResourceID;
        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Resource ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual string ResourceID
        {
            get
            {
                return this._ResourceID;
            }
            set
            {
                this._ResourceID = value;
            }
        }
        #endregion
        #region CapacityRange

        public abstract class capacityRange : PX.Data.BQL.BqlString.Field<capacityRange> { }

        protected String _CapacityRange;
        [PXString]
        [PXUnboundDefault(DateRangeInq.Weekly)]
        [PXUIField(DisplayName = "Range")]
        [DateRangeInq.List]
        public virtual String CapacityRange
        {
            get { return this._CapacityRange; }
            set { this._CapacityRange = value; }
        }

        #endregion
        #region FromDate

        public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

        protected DateTime? _FromDate;
        [PXDate]
        [PXUIField(DisplayName = "From Date")]
        public virtual DateTime? FromDate
        {
            get { return this._FromDate; }
            set { this._FromDate = value; }
        }

        #endregion
        #region ToDate

        public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

        protected DateTime? _ToDate;
        [PXDate]
        [PXUIField(DisplayName = "To Date")]
        public virtual DateTime? ToDate
        {
            get { return this._ToDate; }
            set { this._ToDate = value; }
        }

        #endregion
    }
}