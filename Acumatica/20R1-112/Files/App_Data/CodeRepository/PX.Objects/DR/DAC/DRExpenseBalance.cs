namespace PX.Objects.DR
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CM;
	using GL.FinPeriods;

	[Serializable]
	[PXAccumulator(
		new Type[] {
                typeof(DRExpenseBalance.endBalance),
				typeof(DRExpenseBalance.endProjected),
				typeof(DRExpenseBalance.endBalance),
				typeof(DRExpenseBalance.endProjected),

				typeof(DRExpenseBalance.tranEndBalance),
				typeof(DRExpenseBalance.tranEndProjected),
				typeof(DRExpenseBalance.tranEndBalance),
				typeof(DRExpenseBalance.tranEndProjected)
            },
			new Type[] {
                typeof(DRExpenseBalance.begBalance),
				typeof(DRExpenseBalance.begProjected),
				typeof(DRExpenseBalance.endBalance),
				typeof(DRExpenseBalance.endProjected),

				typeof(DRExpenseBalance.tranBegBalance),
				typeof(DRExpenseBalance.tranBegProjected),
				typeof(DRExpenseBalance.tranEndBalance),
				typeof(DRExpenseBalance.tranEndProjected)
            }
		)]
	[PXCacheName(Messages.DRExpenseBalance)]
	public partial class DRExpenseBalance : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(IsKey = true)]
		public virtual int? BranchID { get; set; }
		#endregion
		#region AcctID
		public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }
		protected Int32? _AcctID;
		[Account(IsKey=true, DisplayName = "Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description))]
		public virtual Int32? AcctID
		{
			get
			{
				return this._AcctID;
			}
			set
			{
				this._AcctID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(DRExpenseBalance.acctID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description), IsKey = true)]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region ComponentID
		public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
		protected Int32? _ComponentID;

		[PXDBInt(IsKey=true)]
		[PXDefault()]
		public virtual Int32? ComponentID
		{
			get
			{
				return this._ComponentID;
			}
			set
			{
				this._ComponentID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDefault(0)]
		[PXDBInt(IsKey=true)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodID(IsKey = true)]
		[PXUIField(DisplayName = "FinPeriod", Enabled = false)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion

		#region BegBalance
		public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
		protected Decimal? _BegBalance;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Begin Balance")]
		public virtual Decimal? BegBalance
		{
			get
			{
				return this._BegBalance;
			}
			set
			{
				this._BegBalance = value;
			}
		}
		#endregion
		#region BegProjected
		public abstract class begProjected : PX.Data.BQL.BqlDecimal.Field<begProjected> { }
		protected Decimal? _BegProjected;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Begin Projected")]
		public virtual Decimal? BegProjected
		{
			get
			{
				return this._BegProjected;
			}
			set
			{
				this._BegProjected = value;
			}
		}
		#endregion
		#region PTDDeferred
		public abstract class pTDDeferred : PX.Data.BQL.BqlDecimal.Field<pTDDeferred> { }
		protected Decimal? _PTDDeferred;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deferred Amount")]
		public virtual Decimal? PTDDeferred
		{
			get
			{
				return this._PTDDeferred;
			}
			set
			{
				this._PTDDeferred = value;
			}
		}
		#endregion
		#region PTDRecognized
		public abstract class pTDRecognized : PX.Data.BQL.BqlDecimal.Field<pTDRecognized> { }
		protected Decimal? _PTDRecognized;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Recognized Amount")]
		public virtual Decimal? PTDRecognized
		{
			get
			{
				return this._PTDRecognized;
			}
			set
			{
				this._PTDRecognized = value;
			}
		}
		#endregion
		#region PTDRecognizedSamePeriod
		public abstract class pTDRecognizedSamePeriod : PX.Data.BQL.BqlDecimal.Field<pTDRecognizedSamePeriod> { }
		protected Decimal? _PTDRecognizedSamePeriod;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Recognized Amount in Same Period")]
		public virtual Decimal? PTDRecognizedSamePeriod
		{
			get
			{
				return this._PTDRecognizedSamePeriod;
			}
			set
			{
				this._PTDRecognizedSamePeriod = value;
			}
		}
		#endregion
		#region PTDProjected
		public abstract class pTDProjected : PX.Data.BQL.BqlDecimal.Field<pTDProjected> { }
		protected Decimal? _PTDProjected;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Amount")]
		public virtual Decimal? PTDProjected
		{
			get
			{
				return this._PTDProjected;
			}
			set
			{
				this._PTDProjected = value;
			}
		}
		#endregion
		#region EndBalance
		public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
		protected Decimal? _EndBalance;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "End Balance")]
		public virtual Decimal? EndBalance
		{
			get
			{
				return this._EndBalance;
			}
			set
			{
				this._EndBalance = value;
			}
		}
		#endregion
		#region EndProjected
		public abstract class endProjected : PX.Data.BQL.BqlDecimal.Field<endProjected> { }
		protected Decimal? _EndProjected;
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "End Projected")]
		public virtual Decimal? EndProjected
		{
			get
			{
				return this._EndProjected;
			}
			set
			{
				this._EndProjected = value;
			}
		}
		#endregion
		
		#region TranBegBalance
		public abstract class tranBegBalance : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Begin Balance")]
		public virtual decimal? TranBegBalance { get; set; }
		#endregion
		#region TranBegProjected
		public abstract class tranBegProjected : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Begin Projected")]
		public virtual decimal? TranBegProjected { get; set; }
		#endregion
		#region TranPTDDeferred
		public abstract class tranPTDDeferred : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deferred Amount")]
		public virtual decimal? TranPTDDeferred { get; set; }
		#endregion
		#region TranPTDRecognized
		public abstract class tranPTDRecognized : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Recognized Amount")]
		public virtual decimal? TranPTDRecognized { get; set; }
		#endregion
		#region TranPTDRecognizedSamePeriod
		public abstract class tranPTDRecognizedSamePeriod : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Recognized Amount in Same Period")]
		public virtual decimal? TranPTDRecognizedSamePeriod { get; set; }
		#endregion
		#region TranPTDProjected
		public abstract class tranPTDProjected : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Amount")]
		public virtual decimal? TranPTDProjected { get; set; }
		#endregion
		#region TranEndBalance
		public abstract class tranEndBalance : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "End Balance")]
		public virtual decimal? TranEndBalance { get; set; }
		#endregion
		#region TranEndProjected
		public abstract class tranEndProjected : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "End Projected")]
		public virtual decimal? TranEndProjected { get; set; }
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
	}

	[PXProjection(typeof(Select5<
		DRExpenseBalance,
		InnerJoin<MasterFinPeriod, 
			On<MasterFinPeriod.finPeriodID, GreaterEqual<DRExpenseBalance.finPeriodID>>>,
		Aggregate<
			GroupBy<DRExpenseBalance.branchID,
			GroupBy<DRExpenseBalance.acctID,
      GroupBy<DRExpenseBalance.subID,
      GroupBy<DRExpenseBalance.componentID,
      GroupBy<DRExpenseBalance.vendorID,
      GroupBy<DRExpenseBalance.projectID,
      Max<DRExpenseBalance.finPeriodID,
			GroupBy<MasterFinPeriod.finPeriodID
		>>>>>>>>>>))]
    [Serializable]
	[PXCacheName(Messages.DRExpenseBalanceByPeriod)]
	public partial class DRExpenseBalanceByPeriod : PX.Data.IBqlTable
    {
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(IsKey = true, BqlField = typeof(DRExpenseBalance.branchID))]
		public virtual int? BranchID { get; set; }
		#endregion
        #region AcctID
        public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }
        protected Int32? _AcctID;
        [Account(IsKey = true, BqlField = typeof(DRExpenseBalance.acctID), DisplayName = "Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description))]
        public virtual Int32? AcctID
        {
            get
            {
                return this._AcctID;
            }
            set
            {
                this._AcctID = value;
            }
        }
        #endregion
        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        protected Int32? _SubID;
        [SubAccount(typeof(DRExpenseBalance.acctID), BqlField = typeof(DRExpenseBalance.subID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description), IsKey = true)]
        public virtual Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
        protected Int32? _ComponentID;

        [PXDBInt(IsKey = true, BqlField = typeof(DRExpenseBalance.componentID))]
        [PXDefault()]
        public virtual Int32? ComponentID
        {
            get
            {
                return this._ComponentID;
            }
            set
            {
                this._ComponentID = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [PXDBInt(IsKey = true, BqlField=typeof(DRExpenseBalance.vendorID))]
        [PXDefault()]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        protected Int32? _ProjectID;
        [PXDBInt(IsKey = true, BqlField = typeof(DRExpenseBalance.projectID))]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion

        #region LastActivityPeriod
        public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
        protected String _LastActivityPeriod;
        [FinPeriodID(BqlField = typeof(DRExpenseBalance.finPeriodID))]
        public virtual String LastActivityPeriod
        {
            get
            {
                return this._LastActivityPeriod;
            }
            set
            {
                this._LastActivityPeriod = value;
            }
        }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        protected String _FinPeriodID;
		[FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
        public virtual String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
    }
	public class DRExpenseBalance2 : DRExpenseBalance
	{
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region AcctID
		public new abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		#endregion
		#region ComponentID
		public new abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
		#endregion
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		#endregion
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region BegBalance
		public new abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
		#endregion
		#region BegProjected
		public new abstract class begProjected : PX.Data.BQL.BqlDecimal.Field<begProjected> { }
		#endregion
		#region PTDDeferred
		public new abstract class pTDDeferred : PX.Data.BQL.BqlDecimal.Field<pTDDeferred> { }
		#endregion
		#region PTDRecognized
		public new abstract class pTDRecognized : PX.Data.BQL.BqlDecimal.Field<pTDRecognized> { }
		#endregion
		#region PTDRecognizedSamePeriod
		public new abstract class pTDRecognizedSamePeriod : PX.Data.BQL.BqlDecimal.Field<pTDRecognizedSamePeriod> { }
		#endregion
		#region PTDProjected
		public new abstract class pTDProjected : PX.Data.BQL.BqlDecimal.Field<pTDProjected> { }
		#endregion
		#region EndBalance
		public new abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
		#endregion
		#region EndProjected
		public new abstract class endProjected : PX.Data.BQL.BqlDecimal.Field<endProjected> { }
		#endregion
		#region TranBegBalance
		public new abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
		#endregion
		#region TranBegProjected
		public new abstract class tranBegProjected : PX.Data.BQL.BqlDecimal.Field<tranBegProjected> { }
		#endregion
		#region TranPTDDeferred
		public new abstract class tranPTDDeferred : PX.Data.BQL.BqlDecimal.Field<tranPTDDeferred> { }
		#endregion
		#region TranPTDRecognized
		public new abstract class tranPTDRecognized : PX.Data.BQL.BqlDecimal.Field<tranPTDRecognized> { }
		#endregion
		#region TranPTDRecognizedSamePeriod
		public new abstract class tranPTDRecognizedSamePeriod : PX.Data.BQL.BqlDecimal.Field<tranPTDRecognizedSamePeriod> { }
		#endregion
		#region TranPTDProjected
		public new abstract class tranPTDProjected : PX.Data.BQL.BqlDecimal.Field<tranPTDProjected> { }
		#endregion
		#region TranEndBalance
		public new abstract class tranEndBalance : PX.Data.BQL.BqlDecimal.Field<tranEndBalance> { }
		#endregion
		#region TranEndProjected
		public new abstract class tranEndProjected : PX.Data.BQL.BqlDecimal.Field<tranEndProjected> { }
		#endregion
		#region tstamp
		public new abstract class tstamp : IBqlField { }
		#endregion
	}

}
