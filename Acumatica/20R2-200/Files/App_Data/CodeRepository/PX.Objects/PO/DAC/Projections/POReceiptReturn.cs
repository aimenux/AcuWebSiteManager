using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PXProjection(typeof(Select<POReceipt>), Persistent = false)]
	[Serializable]
	[PXHidden]
	public partial class POReceiptReturn : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceipt.receiptNbr))]
		[POReceiptType.RefNbr(typeof(Search<POReceipt.receiptNbr>), Filterable = true)]
		[PXUIField(DisplayName = "Receipt Nbr.", Enabled = false)]
		public virtual string ReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceipt.receiptType))]
		[POReceiptType.List]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		public virtual string ReceiptType
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[VendorActive(Enabled = false, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, BqlField = typeof(POReceipt.vendorID))]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POReceipt.vendorID>>>), DescriptionField = typeof(Location.descr), Enabled = false, BqlField = typeof(POReceipt.vendorLocationID))]
		public virtual int? VendorLocationID
		{
			get;
			set;
		}
		#endregion
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
		[PXDBDate(BqlField = typeof(POReceipt.receiptDate))]
		[PXUIField(DisplayName = "Date", Enabled = false)]
		public virtual DateTime? ReceiptDate
		{
			get;
			set;
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		[PXDBQuantity(BqlField = typeof(POReceipt.orderQty))]
		[PXUIField(DisplayName = "Total Qty.", Enabled = false)]
		public virtual decimal? OrderQty
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool(BqlField = typeof(POReceipt.released))]
		[PXUIField(DisplayName = "Released")]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
	}
}
