namespace PX.Objects.DR
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CM;

	[Serializable]
	[PXCacheName(Messages.DRExpenseProjection)]
	public partial class DRExpenseProjection : PX.Data.IBqlTable
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
		[SubAccount(typeof(DRExpenseProjection.acctID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description), IsKey = true)]
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
		[PXDBInt(IsKey = true)]
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

		#region TranPTDProjected
		public abstract class tranPTDProjected : IBqlField { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Amount")]
		public virtual decimal? TranPTDProjected { get; set; }
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

	[DRExpenseAccum]
	[Serializable]
	[PXHidden]
	public partial class DRExpenseProjectionAccum : DRExpenseProjection
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

		#region PTDProjected
		public new abstract class pTDProjected : PX.Data.BQL.BqlDecimal.Field<pTDProjected> { }
		#endregion
		#region PTDRecognized
		public new abstract class pTDRecognized : PX.Data.BQL.BqlDecimal.Field<pTDRecognized> { }
		#endregion
		#region PTDRecognizedSamePeriod
		public new abstract class pTDRecognizedSamePeriod : PX.Data.BQL.BqlDecimal.Field<pTDRecognizedSamePeriod> { }
		#endregion

		#region TranPTDProjected
		public new abstract class tranPTDProjected : IBqlField { }
		#endregion
		#region TranPTDRecognized
		public new abstract class tranPTDRecognized : IBqlField { }
		#endregion
		#region TranPTDRecognizedSamePeriod
		public new abstract class tranPTDRecognizedSamePeriod : IBqlField { }
		#endregion
	}
}
