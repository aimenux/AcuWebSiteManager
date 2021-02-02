using System;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Work Center Capacity Inquiry
    /// </summary>
    [PX.Objects.GL.TableAndChartDashboardType]
    public class WorkCenterCapacityInq : CapacityInqBase<WorkCenterCapacityInq.WorkCenterCapacityFilter, WorkCenterCapacityInq.WorkCenterCapacityDetail>
    {
        protected override IEnumerable<WorkCenterCapacityInq.WorkCenterCapacityDetail> GetDetail()
        {
            PXSelectBase<WorkCenterCapacityInq.WorkCenterCapacityDetail> cmd = new PXSelectGroupBy<WorkCenterCapacityInq.WorkCenterCapacityDetail,
                Aggregate<GroupBy<WorkCenterCapacityInq.WorkCenterCapacityDetail.wcID,
                    GroupBy<WorkCenterCapacityInq.WorkCenterCapacityDetail.schdDate,
                    Sum<WorkCenterCapacityInq.WorkCenterCapacityDetail.workTime,
                    Sum<WorkCenterCapacityInq.WorkCenterCapacityDetail.totalBlocks,
                    Sum<WorkCenterCapacityInq.WorkCenterCapacityDetail.planBlocks,
                    Sum<WorkCenterCapacityInq.WorkCenterCapacityDetail.schdTime,
                    Sum<WorkCenterCapacityInq.WorkCenterCapacityDetail.schdEfficiencyTime,
                    Sum<WorkCenterCapacityInq.WorkCenterCapacityDetail.schdBlocks,
                    Sum<WorkCenterCapacityInq.WorkCenterCapacityDetail.availableBlocks,
                    Max<WorkCenterCapacityInq.WorkCenterCapacityDetail.endTime,
                    Min<WorkCenterCapacityInq.WorkCenterCapacityDetail.startTime>>>>>>>>>>>>>(this);

            var filter = CapacityFilter.Current;

            if (filter?.FromDate == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(filter.WcID))
            {
                cmd.WhereAnd<Where<WorkCenterCapacityInq.WorkCenterCapacityDetail.wcID, Equal<Current<WorkCenterCapacityFilter.wcID>>>>();
            }

            if (filter.FromDate != null && filter.ToDate == null)
            {
                cmd.WhereAnd<Where<WorkCenterCapacityInq.WorkCenterCapacityDetail.schdDate, GreaterEqual<Current<WorkCenterCapacityFilter.fromDate>>>>();
            }

            if (filter.FromDate != null && filter.ToDate != null)
            {
                cmd.WhereAnd<Where<WorkCenterCapacityInq.WorkCenterCapacityDetail.schdDate, Between<Current<WorkCenterCapacityFilter.fromDate>, Current<WorkCenterCapacityFilter.toDate>>>>();
            }

            return cmd.Select().ToFirstTable();
        }

        [Serializable]
        [PXCacheName("Work Center Capacity Filter")]
        public class WorkCenterCapacityFilter : CapacityFilterBase
        {
            #region WcID

            public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

            [WorkCenterIDField]
            [PXSelector(typeof(Search<AMWC.wcID>))]
            public virtual string WcID
            {
                get { return this._ResourceID; }
                set { this._ResourceID = value; }
            }

            #endregion
        }

        [Serializable]
        [PXCacheName("Work Center Capacity Detail")]
        public class WorkCenterCapacityDetail : AMWCSchd, ICapacityUtilization
        {
            public new abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

            public new abstract class schdDate : PX.Data.BQL.BqlDateTime.Field<schdDate> { }

            #region ShiftID
            public new abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }
            [PXDBString(4)]
            [PXUIField(DisplayName = "Shift", Enabled = false, Visible = false)]
            public override String ShiftID
            {
                get
                {
                    return this._ShiftID;
                }
                set
                {
                    this._ShiftID = value;
                }
            }
            #endregion
            #region PlanUtilizationPct

            public abstract class planUtilizationPct : PX.Data.BQL.BqlDecimal.Field<planUtilizationPct> { }

            protected decimal? _PlanUtilizationPct;
            [PXDecimal(2)]
            [PXUnboundDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Plan Utilization Pct", Enabled = false)]
            public virtual decimal? PlanUtilizationPct
            {
                get { return this._PlanUtilizationPct; }
                set { this._PlanUtilizationPct = value; }
            }
            #endregion
            #region SchdUtilizationPct

            public abstract class schdUtilizationPct : PX.Data.BQL.BqlDecimal.Field<schdUtilizationPct> { }

            protected decimal? _SchdUtilizationPct;
            [PXDecimal(2)]
            [PXUnboundDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Schd Utilization Pct", Enabled = false)]
            public virtual decimal? SchdUtilizationPct
            {
                get { return this._SchdUtilizationPct; }
                set { this._SchdUtilizationPct = value; }
            }
            #endregion
            #region FromDate
            /// <summary>
            /// Starting date range
            /// </summary>
            public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

            protected DateTime? _FromDate;
            /// <summary>
            /// Starting date range
            /// </summary>
            [PXDate]
            [PXUIField(DisplayName = "From Date", Visible = false)]
            public virtual DateTime? FromDate
            {
                get { return this._FromDate; }
                set { this._FromDate = value; }
            }

            #endregion
            #region ToDate
            /// <summary>
            /// Ending date range
            /// </summary>
            public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

            protected DateTime? _ToDate;
            /// <summary>
            /// Ending date range
            /// </summary>
            [PXDate]
            [PXUIField(DisplayName = "To Date", Visible = false)]
            public virtual DateTime? ToDate
            {
                get { return this._ToDate; }
                set { this._ToDate = value; }
            }

            #endregion
        }
    }
}