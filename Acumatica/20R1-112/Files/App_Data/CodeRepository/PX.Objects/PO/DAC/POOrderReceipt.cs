using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Attributes;

namespace PX.Objects.PO
{
	[System.SerializableAttribute()]
	public partial class POOrderReceipt : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POOrderReceipt>.By<receiptNbr, pOType, pONbr>
		{
			public static POOrderReceipt Find(PXGraph graph, string receiptNbr, string pOType, string pONbr) => FindBy(graph, receiptNbr, pOType, pONbr);
		}
		public static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POOrderReceipt>.By<receiptNbr> { }
			public class Order : POOrder.PK.ForeignKeyOf<POOrderReceipt>.By<pOType, pONbr> { }
		}
		#endregion
		
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		protected String _ReceiptType;
		[PXDBString(2, IsFixed = true, InputMask = "")]
		[PXDBDefault(typeof(POReceipt.receiptType))]
		[POReceiptType.List()]
		public virtual String ReceiptType
		{
			get
			{
				return this._ReceiptType;
			}
			set
			{
				this._ReceiptType = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(POReceipt.receiptNbr))]
		[PXDefault()]
		[PXParent(typeof(Select<POReceipt, Where<POReceipt.receiptType, Equal<Current<POOrderReceipt.receiptType>>, And<POReceipt.receiptNbr, Equal<Current<POOrderReceipt.receiptNbr>>>>>))]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		protected String _POType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = false)]
		public virtual String POType
		{
			get
			{
				return this._POType;
			}
			set
			{
				this._POType = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
		protected String _PONbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[PO.RefNbr(typeof(Search<POOrder.orderNbr>), Filterable = true)]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region ReceiptNoteID
		public abstract class receiptNoteID : PX.Data.BQL.BqlGuid.Field<receiptNoteID> { }
		protected Guid? _ReceiptNoteID;
		[PXDBGuid(IsImmutable = true)]
		[PXDefault(typeof(POReceipt.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? ReceiptNoteID
		{
			get
			{
				return this._ReceiptNoteID;
			}
			set
			{
				this._ReceiptNoteID = value;
			}
		}
		#endregion
		#region OrderNoteID
		public abstract class orderNoteID : PX.Data.BQL.BqlGuid.Field<orderNoteID> { }
		protected Guid? _OrderNoteID;
		[CopiedNoteID(typeof(POOrder))]
		public virtual Guid? OrderNoteID
		{
			get
			{
				return this._OrderNoteID;
			}
			set
			{
				this._OrderNoteID = value;
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
	}

	[PXProjection(typeof(Select3<POOrderReceipt,
								LeftJoin<POOrder,
									 On<POOrderReceipt.pOType, Equal<POOrder.orderType>,
									And<POOrderReceipt.pONbr, Equal<POOrder.orderNbr>>>>,
								OrderBy<Asc<POOrderReceipt.pONbr>>>), Persistent = false)]
	[SerializableAttribute()]
	[PXCacheName(Messages.POReceiptPOOrder)]
	public partial class POOrderReceiptLink : POOrderReceipt
	{
		#region Keys
		public new class PK : PrimaryKeyOf<POOrderReceiptLink>.By<receiptNbr, pOType, pONbr>
		{
			public static POOrderReceiptLink Find(PXGraph graph, string receiptNbr, string pOType, string pONbr) => FindBy(graph, receiptNbr, pOType, pONbr);
		}
		public new static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POOrderReceiptLink>.By<receiptNbr> { }
			public class Order : POOrder.PK.ForeignKeyOf<POOrderReceiptLink>.By<pOType, pONbr> { }
		}
		#endregion

		#region ReceiptType
		public new abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		#endregion
		#region OrderType
		public new abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		#endregion
		#region OrderNbr
		public new abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true, BqlField = typeof(POOrder.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POOrderStatus.List()]
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
		#region ReceiptNoteID
		public new abstract class receiptNoteID : PX.Data.BQL.BqlGuid.Field<receiptNoteID> { }
		#endregion
		#region OrderNoteID
		public new abstract class orderNoteID : PX.Data.BQL.BqlGuid.Field<orderNoteID> { }
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POOrder.taxZoneID))]
		[PXUIField(DisplayName = "Vendor Tax Zone", Visibility = PXUIVisibility.Visible)]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(POOrder.taxCalcMode))]
		[TX.TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual String TaxCalcMode { get; set; }
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POOrder.termsID))]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		public virtual string TermsID { get; set; }
		#endregion
		#region PayToVendorID
		public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }
		[POOrderPayToVendor(CacheGlobal = true, Filterable = true, BqlField = typeof(POOrder.payToVendorID))]
		public virtual int? PayToVendorID { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrder.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(POOrder.curyInfoID))]
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
		#region UnbilledOrderQty
		public abstract class unbilledOrderQty : PX.Data.BQL.BqlDecimal.Field<unbilledOrderQty> { }
		[PXDBQuantity(BqlField = typeof(POOrder.unbilledOrderQty))]
		[PXUIField(DisplayName = "Unbilled Quantity", Enabled = false)]
		public virtual decimal? UnbilledOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledOrderTotal
		public abstract class curyUnbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledOrderTotal> { }
		[PXDBCurrency(typeof(POOrderReceiptLink.curyInfoID), typeof(POOrderReceiptLink.unbilledOrderTotal), BqlField = typeof(POOrder.curyUnbilledOrderTotal))]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		public virtual decimal? CuryUnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderTotal
		public abstract class unbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<unbilledOrderTotal> { }
		[PXDBDecimal(4, BqlField = typeof(POOrder.unbilledOrderTotal))]
		public virtual decimal? UnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}
