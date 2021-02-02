using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.SO.DAC.Projections
{
	[PXProjection(typeof(Select2<ARTran,
	  LeftJoin<INTran, On<INTran.aRDocType, Equal<ARTran.tranType>, And<INTran.aRRefNbr, Equal<ARTran.refNbr>, And<INTran.aRLineNbr, Equal<ARTran.lineNbr>>>>>,
	  Where<ARTran.released, Equal<boolTrue>,
		And<Where2<Where<INTran.released, Equal<True>, And<INTran.qty, Greater<decimal0>,
		And<INTran.tranType, In3<INTranType.issue, INTranType.debitMemo, INTranType.invoice>>>>,
		Or<Where<INTran.released, IsNull, And<ARTran.lineType, In3<SOLineType.miscCharge, SOLineType.nonInventory>>>>>>>,
		OrderBy<Desc<ARTran.refNbr>>>),
		Persistent = false)]
	[PXCacheName(AR.Messages.ARTran)]
	[Serializable]
	public class ARTranForDirectInvoice : IBqlTable
	{
		#region Selected
		public abstract class selected : Data.BQL.BqlBool.Field<selected>
		{
		}
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : Data.BQL.BqlString.Field<tranType>
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARTran.tranType))]
		[PXUIField(DisplayName = "Doc. Type", Enabled = false)]
		[ARDocType.List]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : Data.BQL.BqlString.Field<refNbr>
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARTran.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		[PXDBInt(IsKey = true, BqlField = typeof(ARTran.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region LineType
		public abstract class lineType : Data.BQL.BqlString.Field<lineType>
		{
		}
		[PXDBString(2, IsFixed = true, BqlField = typeof(ARTran.lineType))]
		[PXUIField(DisplayName = "Line Type", Enabled = false)]
		public virtual string LineType
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public abstract class customerID : Data.BQL.BqlInt.Field<customerID>
		{
		}
		[Customer(Enabled = false, BqlField = typeof(ARTran.customerID))]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : Data.BQL.BqlDateTime.Field<tranDate>
		{
		}
		[PXDBDate(BqlField = typeof(ARTran.tranDate))]
		[PXUIField(DisplayName = "Doc. Date", Enabled = false)]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		[AnyInventory(Enabled = false, BqlField = typeof(ARTran.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : Data.BQL.BqlString.Field<uOM>
		{
		}
		[INUnit(typeof(ARTran.inventoryID), Enabled = false, BqlField = typeof(ARTran.uOM))]
		public virtual string UOM
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : Data.BQL.BqlDecimal.Field<qty>
		{
		}
		[PXDBDecimal(BqlField = typeof(ARTran.qty))]
		[PXUIField(DisplayName = "Qty", Enabled = false)]
		public virtual decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : Data.BQL.BqlDecimal.Field<curyUnitPrice>
		{
		}
		[PXDBDecimal(BqlField = typeof(ARTran.curyUnitPrice))]
		[PXUIField(DisplayName = "Unit Price", Enabled = false)]
		public virtual decimal? CuryUnitPrice
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : Data.BQL.BqlBool.Field<released>
		{
		}
		[PXDBBool(BqlField = typeof(ARTran.released))]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion

		#region SubItemID
		public abstract class subItemID : Data.BQL.BqlInt.Field<subItemID>
		{
		}
		[PXDBInt(BqlField = typeof(ARTran.subItemID))]
		public virtual int? SubItemID
		{
			get;
			set;
		}
		#endregion

		#region SiteID
		public abstract class siteID : Data.BQL.BqlInt.Field<siteID>
		{
		}
		[PXDBInt(BqlField = typeof(ARTran.siteID))]
		public virtual int? SiteID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : Data.BQL.BqlInt.Field<locationID>
		{
		}
		[PXDBInt(BqlField = typeof(ARTran.locationID))]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : Data.BQL.BqlString.Field<lotSerialNbr>
		{
		}
		[PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "", BqlField = typeof(ARTran.lotSerialNbr))]
		public virtual string LotSerialNbr
		{
			get;
			set;
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : Data.BQL.BqlDateTime.Field<expireDate>
		{
		}
		[PXDBDate(InputMask = "d", DisplayMask = "d", BqlField = typeof(ARTran.expireDate))]
		public virtual DateTime? ExpireDate
		{
			get;
			set;
		}
		#endregion
	}
}
