namespace PX.Objects.SO
{
	using System;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.AR;
	using PX.Objects.CS;
	using PX.Objects.CR;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOSalesPerTran)]
	public partial class SOSalesPerTran : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOSalesPerTran>.By<orderType, orderNbr, salespersonID>
		{
			public static SOSalesPerTran Find(PXGraph graph, string orderType, string orderNbr, int? salespersonID)
				=> FindBy(graph, orderType, orderNbr, salespersonID);
		}
		public static class FK
		{
			public class Order : SOOrder.PK.ForeignKeyOf<SOSalesPerTran>.By<orderType, orderNbr> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOSalesPerTran>.By<orderType> { }
		}
		#endregion
		
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrder.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBLiteDefault(typeof(SOOrder.orderNbr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOSalesPerTran.orderType>>,
						 And<SOOrder.orderNbr, Equal<Current<SOSalesPerTran.orderNbr>>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region SalespersonID
		public abstract class salespersonID : PX.Data.BQL.BqlInt.Field<salespersonID> { }
		protected Int32? _SalespersonID;
		[SalesPerson(DirtyRead = true, Enabled = false, IsKey = true)]
		[PXForeignReference(typeof(Field<SOSalesPerTran.salespersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
		public virtual Int32? SalespersonID
		{
			get
			{
				return this._SalespersonID;
			}
			set
			{
				this._SalespersonID = value;
			}
		}
		#endregion
		#region RefCntr
		public abstract class refCntr : PX.Data.BQL.BqlInt.Field<refCntr> { }
		protected Int32? _RefCntr;
		[PXDBInt]
		[PXDefault(0)]
		public virtual Int32? RefCntr
		{
			get
			{
				return this._RefCntr;
			}
			set
			{
				this._RefCntr = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
			Search<CustSalesPeople.commisionPct, Where<CustSalesPeople.bAccountID, Equal<Current<SOOrder.customerID>>,
				And<CustSalesPeople.locationID, Equal<Current<SOOrder.customerLocationID>>,
				And<CustSalesPeople.salesPersonID, Equal<Current<SOSalesPerTran.salespersonID>>>>>>, 
			Search<SalesPerson.commnPct, Where<SalesPerson.salesPersonID, Equal<Current<SOSalesPerTran.salespersonID>>>>>))]
		[PXUIField(DisplayName = "Commission %")]
		public virtual Decimal? CommnPct
		{
			get
			{
				return this._CommnPct;
			}
			set
			{
				this._CommnPct = value;
			}
		}
		#endregion
		#region CuryCommnblAmt
		public abstract class curyCommnblAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnblAmt> { }
		protected Decimal? _CuryCommnblAmt;
		[PXDBCurrency(typeof(SOSalesPerTran.curyInfoID), typeof(SOSalesPerTran.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commissionable Amount", Enabled = false)]
		public virtual Decimal? CuryCommnblAmt
		{
			get
			{
				return this._CuryCommnblAmt;
			}
			set
			{
				this._CuryCommnblAmt = value;
			}
		}
		#endregion
		#region CommnblAmt
		public abstract class commnblAmt : PX.Data.BQL.BqlDecimal.Field<commnblAmt> { }
		protected Decimal? _CommnblAmt;
		[PXDBBaseCury()]
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
		#region CuryCommnAmt
		public abstract class curyCommnAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnAmt> { }
		protected Decimal? _CuryCommnAmt;
		[PXDBCurrency(typeof(SOSalesPerTran.curyInfoID), typeof(SOSalesPerTran.commnAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
		[PXFormula(typeof(Mult<SOSalesPerTran.curyCommnblAmt, Div<SOSalesPerTran.commnPct, decimal100>>))]
		public virtual Decimal? CuryCommnAmt
		{
			get
			{
				return this._CuryCommnAmt;
			}
			set
			{
				this._CuryCommnAmt = value;
			}
		}
		#endregion
		#region CommnAmt
		public abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
		protected Decimal? _CommnAmt;
		[PXDBBaseCury()]
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
