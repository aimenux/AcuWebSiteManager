namespace PX.Objects.AR
{
    using System;
    using PX.Data;

    public class ARSPCommissionPeriodStatus
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Prepared, Open, Closed },
                new string[] { Messages.PeriodPrepared, Messages.PeriodOpen, Messages.PeriodClosed })
            {; }
        }

        public const string Prepared = "P";
        public const string Open = "N";
        public const string Closed = "C";

        public class prepared : PX.Data.BQL.BqlString.Constant<prepared>
		{
            public prepared() : base(Prepared) {; }
        }

        public class open : PX.Data.BQL.BqlString.Constant<open>
		{
            public open() : base(Open) {; }
        }

        public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
            public closed() : base(Closed) {; }
        }
    }

    /// <summary>
    /// Represents a period of a <see cref="ARSPCommissionYear">commission year</see>,
    /// for which salesperson commissions are calculated by the Calculate Commissions 
    /// (AR505500) process. The number of periods in a commission is regulated by the
    /// <see cref="ARSetup.SPCommnPeriodType">commission period type</see>, which is specified
    /// on the Accounts Receivable Preferences (AR101000) form. The entities of this type are
    /// created during the Calculate Commissions (AR505500) process, which corresponds
    /// to the <see cref="ARSPCommissionProcess"/> graph. Commission periods are closed
    /// on the Close Commission Period (AR506500) form, which corresponds to the
    /// <see cref="ARSPCommissionReview"/> graph.
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.ARSPCommissionPeriod)]
    public partial class ARSPCommissionPeriod : IBqlTable
    {
        #region CommnPeriodID
        public abstract class commnPeriodID : PX.Data.BQL.BqlString.Field<commnPeriodID> { }
        protected String _CommnPeriodID;
        [PXDefault()]
        [GL.FinPeriodID(IsKey = true)]
        [PXUIField(DisplayName = "Commission Period", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(ARSPCommissionPeriod.commnPeriodID))]
        public virtual String CommnPeriodID
        {
            get
            {
                return this._CommnPeriodID;
            }
            set
            {
                this._CommnPeriodID = value;
            }
        }
        #endregion
        #region Year
        public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
        protected String _Year;
        [PXDBString(4, IsFixed = true)]
        [PXDefault()]
        public virtual String Year
        {
            get
            {
                return this._Year;
            }
            set
            {
                this._Year = value;
            }
        }
        #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        protected DateTime? _StartDate;
        [PXDBDate()]
        [PXDefault()]
        public virtual DateTime? StartDate
        {
            get
            {
                return this._StartDate;
            }
            set
            {
                this._StartDate = value;
            }
        }
        #endregion
        #region StartDateUI
        public abstract class startDateUI : PX.Data.BQL.BqlDateTime.Field<startDateUI> { }

        [PXDate]

        [PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? StartDateUI
        {
            [PXDependsOnFields(typeof(startDate), typeof(endDate))]
            get
            {
                return (_StartDate != null && _EndDate != null && _StartDate == _EndDate) ? _StartDate.Value.AddDays(-1) : _StartDate;
            }
            set
            {
                _StartDate = (value != null && _EndDate != null && value == EndDateUI) ? value.Value.AddDays(1) : value;
            }
        }
        #endregion
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
        protected DateTime? _EndDate;
        [PXDBDate()]
        [PXDefault()]
        //[PXUIField(DisplayName = "To", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        protected String _Status;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ARSPCommissionPeriodStatus.Open)]
        [ARSPCommissionPeriodStatus.List()]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual String Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region Filed
        public abstract class filed : PX.Data.BQL.BqlBool.Field<filed> { }
        protected Boolean? _Filed;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Filed
        {
            get
            {
                return this._Filed;
            }
            set
            {
                this._Filed = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region EndDateUI
        public abstract class endDateUI : PX.Data.BQL.BqlDateTime.Field<endDateUI> { }

        [PXDate()]
        [PXUIField(DisplayName = "To", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? EndDateUI
        {
            [PXDependsOnFields(typeof(endDate))]
            get
            {
                return _EndDate?.AddDays(-1);
            }
            set
            {
                _EndDate = value?.AddDays(1);
            }
        }
        #endregion
    }
}