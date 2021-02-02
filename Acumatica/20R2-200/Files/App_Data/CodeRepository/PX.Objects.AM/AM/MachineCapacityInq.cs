using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Machine Capacity Inquiry
    /// </summary>
    [PX.Objects.GL.TableAndChartDashboardType]
    public class MachineCapacityInq : CapacityInqBase<MachineCapacityInq.MachineCapacityFilter, MachineCapacityInq.MachineCapacityDetail>
    {
        protected override IEnumerable<MachineCapacityInq.MachineCapacityDetail> GetDetail()
        {
            PXSelectBase<MachineCapacityInq.MachineCapacityDetail> cmd = new PXSelectGroupBy<MachineCapacityInq.MachineCapacityDetail,
                Aggregate<GroupBy<MachineCapacityInq.MachineCapacityDetail.machID,
                    GroupBy<MachineCapacityInq.MachineCapacityDetail.schdDate,
                    Sum<MachineCapacityInq.MachineCapacityDetail.workTime,
                    Sum<MachineCapacityInq.MachineCapacityDetail.totalBlocks,
                    Sum<MachineCapacityInq.MachineCapacityDetail.planBlocks,
                    Sum<MachineCapacityInq.MachineCapacityDetail.schdTime,
                    Sum<MachineCapacityInq.MachineCapacityDetail.schdEfficiencyTime,
                    Sum<MachineCapacityInq.MachineCapacityDetail.schdBlocks,
                    Sum<MachineCapacityInq.MachineCapacityDetail.availableBlocks,
                    Max<MachineCapacityInq.MachineCapacityDetail.endTime,
                    Min<MachineCapacityInq.MachineCapacityDetail.startTime>>>>>>>>>>>>>(this);

            var filter = CapacityFilter.Current;

            if (filter?.FromDate == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(filter.MachID))
            {
                cmd.WhereAnd<Where<MachineCapacityInq.MachineCapacityDetail.machID, Equal<Current<MachineCapacityFilter.machID>>>>();
            }

            if (filter.FromDate != null && filter.ToDate == null)
            {
                cmd.WhereAnd<Where<MachineCapacityInq.MachineCapacityDetail.schdDate, GreaterEqual<Current<MachineCapacityFilter.fromDate>>>>();
            }

            if (filter.FromDate != null && filter.ToDate != null)
            {
                cmd.WhereAnd<Where<MachineCapacityInq.MachineCapacityDetail.schdDate, Between<Current<MachineCapacityFilter.fromDate>, Current<MachineCapacityFilter.toDate>>>>();
            }

            return cmd.Select().ToFirstTable();
        }

        [Serializable]
        [PXCacheName("Machine Capacity Filter")]
        public class MachineCapacityFilter : CapacityFilterBase
        {
            #region MachID
            public abstract class machID : PX.Data.BQL.BqlString.Field<machID> { }

            [PXString(30, IsUnicode = true)]
            [PXUIField(DisplayName = "Machine ID")]
            [PXSelector(typeof(Search<AMMach.machID>))]
            public virtual string MachID
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
        }

        [Serializable]
        [PXCacheName("Machine Capacity Detail")]
        public class MachineCapacityDetail : AMMachSchd, ICapacityUtilization
        {
            public new abstract class machID : PX.Data.BQL.BqlString.Field<machID> { }

            public new abstract class schdDate : PX.Data.BQL.BqlDateTime.Field<schdDate> { }

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