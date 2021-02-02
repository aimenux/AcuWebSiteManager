namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CM;
	using PX.Objects.CR;
	
	/// <summary>
	/// Represents aggregate historical information about commissions calculated 
	/// for a given salesperson in a given financial period, additionally broken 
	/// down by branch, customer, and customer location. The records of this type 
	/// are created by the Calculate Commissions process, which corresponds to the 
	/// <see cref="ARSPCommissionProcess"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARSPCommnHistory)]
	public partial class ARSPCommnHistory : PX.Data.IBqlTable
	{        
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[PXDefault()]
		[SalesPerson(DescriptionField = typeof(SalesPerson.descr),IsKey = true)]
		[PXForeignReference(typeof(Field<ARSPCommnHistory.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region CommnPeriod
		public abstract class commnPeriod : PX.Data.BQL.BqlString.Field<commnPeriod> { }
		protected String _CommnPeriod;
		[PXDefault()]
		[GL.FinPeriodID(IsKey=true)]
		[PXUIField(DisplayName = "Commission Period")]
		public virtual String CommnPeriod
		{
			get
			{
				return this._CommnPeriod;
			}
			set
			{
				this._CommnPeriod = value;
			}
		}
		#endregion        
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
        [GL.Branch(IsKey = true)]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<ARSPCommnHistory.customerID>.IsRelatedTo<Location.bAccountID>,
				Field<ARSPCommnHistory.customerLocationID>.IsRelatedTo<Location.locationID>
			>))]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region CommnAmt
		public abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
		protected Decimal? _CommnAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Commission Amount")]
		public virtual Decimal? CommnAmt
		{
			get
			{
				return this._CommnAmt;
			}
			set
			{
				this._CommnAmt = value;
			}
		}
		#endregion
		#region CommnblAmt
		public abstract class commnblAmt : PX.Data.BQL.BqlDecimal.Field<commnblAmt> { }
		protected Decimal? _CommnblAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Commissionable Amount")]
		public virtual Decimal? CommnblAmt
		{
			get
			{
				return this._CommnblAmt;
			}
			set
			{
				this._CommnblAmt = value;
			}
		}
		#endregion
		#region PRProcessedDate
		public abstract class pRProcessedDate : PX.Data.BQL.BqlDateTime.Field<pRProcessedDate> { }
		[PXDBDate]
		[PXUIField(DisplayName = "PR Processed Date", Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? PRProcessedDate { get; set; }
		#endregion
		
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Enabled=false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[CR.BAccountType.SalesPersonTypeListAttribute]
        [Obsolete(Common.Messages.ObsoletePayrollFieldToRemove)]
		public virtual String Type { get; set; }
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
}
