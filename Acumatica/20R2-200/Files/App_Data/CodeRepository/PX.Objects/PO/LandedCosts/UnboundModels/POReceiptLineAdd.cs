using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;

namespace PX.Objects.PO.LandedCosts
{
	//Read-only class for selector
	[PXProjection(typeof(Select2<POReceiptLine,
		InnerJoin<POReceipt, On<POReceipt.receiptType, Equal<POReceiptLine.receiptType>, And<POReceipt.receiptNbr, Equal<POReceiptLine.receiptNbr>>>,
		LeftJoin<POLine, On<POLine.orderType, Equal<POReceiptLine.pOType>, 
			And<POLine.orderNbr, Equal<POReceiptLine.pONbr>, 
			And<POLine.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>>>>),
		Persistent = false)]
	[PXCacheName("Purchase Receipt Line")]
	[Serializable]
	public partial class POReceiptLineAdd : IBqlTable, ISortOrder
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(BqlField = typeof(POReceiptLine.branchID))]
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
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		[PXUIField(DisplayName = "Receipt Nbr.")]
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
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		protected String _ReceiptType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.receiptType))]
		[PXUIField(DisplayName = "Receipt Type")]
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

		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField = typeof(POReceipt.invoiceNbr))]
		[PXUIField(DisplayName = "Vendor Ref.")]
		public virtual String InvoiceNbr
		{
			get;
			set;
		}
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion

		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXDBInt(BqlField = typeof(POReceiptLine.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
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
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.lineType))]
		[POLineType.List()]
		[PXUIField(DisplayName = "Line Type")]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(POReceiptLine.inventoryID))]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(BqlField = typeof(POReceiptLine.vendorID), Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }

		[PXDBDate(BqlField = typeof(POReceiptLine.receiptDate))]
		public virtual DateTime? ReceiptDate
		{
			get;
			set;
		}
		#endregion
		#region ReceiptLastModifiedDateTime
		public abstract class receiptLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<receiptLastModifiedDateTime> { }

		[PXDBDate(BqlField = typeof(POReceipt.lastModifiedDateTime))]
		public virtual DateTime? ReceiptLastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(typeof(inventoryID), BqlField = typeof(POReceiptLine.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.SiteAvail(typeof(inventoryID), typeof(subItemID), BqlField = typeof(POReceiptLine.siteID))]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(inventoryID), BqlField = typeof(POReceiptLine.uOM))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region ReceiptQty
		public abstract class receiptQty : PX.Data.BQL.BqlDecimal.Field<receiptQty> { }

		protected Decimal? _ReceiptQty;
		[PXDBQuantity(typeof(uOM), typeof(baseReceiptQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.receiptQty))]
		[PXUIField(DisplayName = "Receipt Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReceiptQty
		{
			get
			{
				return this._ReceiptQty;
			}
			set
			{
				this._ReceiptQty = value;
			}
		}

		#endregion
		#region BaseReceiptQty
		public abstract class baseReceiptQty : PX.Data.BQL.BqlDecimal.Field<baseReceiptQty> { }

		protected Decimal? _BaseReceiptQty;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseReceiptQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseReceiptQty
		{
			get
			{
				return this._BaseReceiptQty;
			}
			set
			{
				this._BaseReceiptQty = value;
			}
		}

		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseReceiptQty;
			}
			set
			{
				this._BaseReceiptQty = value;
			}
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(POReceiptLine.curyInfoID))]
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
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;

		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.unitCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion

		#region TranCostFinal
		public abstract class tranCostFinal : PX.Data.BQL.BqlDecimal.Field<tranCostFinal> { }
		[PXDBBaseCury(BqlField = typeof(POReceiptLine.tranCostFinal))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Final IN Ext. Cost", Enabled = false)]
		public virtual decimal? TranCostFinal
		{
			get;
			set;
		}
		#endregion

		#region UnitWeight
		public abstract class unitWeight : PX.Data.BQL.BqlDecimal.Field<unitWeight> { }
		protected Decimal? _UnitWeight;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.unitWeight))]
		[PXUIField(DisplayName = "Unit Weight")]
		public virtual Decimal? UnitWeight
		{
			get;
			set;
		}
		#endregion
		#region UnitVolume
		public abstract class unitVolume : PX.Data.BQL.BqlDecimal.Field<unitVolume> { }
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.unitVolume))]
		[PXUIField(DisplayName = "Unit Volume")]
		public virtual Decimal? UnitVolume
		{
			get;
			set;
		}
		#endregion

		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;

		[PXDBInt(BqlField = typeof(POReceiptLine.expenseAcctID))]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;

		[PXDBInt(BqlField = typeof(POReceiptLine.expenseSubID))]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region POAccrualAcctID
		public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID> { }
		protected Int32? _POAccrualAcctID;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualAcctID))]
		public virtual Int32? POAccrualAcctID
		{
			get
			{
				return this._POAccrualAcctID;
			}
			set
			{
				this._POAccrualAcctID = value;
			}
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID> { }
		protected Int32? _POAccrualSubID;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualSubID))]
		public virtual Int32? POAccrualSubID
		{
			get
			{
				return this._POAccrualSubID;
			}
			set
			{
				this._POAccrualSubID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POReceiptLine.tranDesc))]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.Visible)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected int? _ProjectID;
		[PXDBInt(BqlField = typeof(POReceiptLine.projectID))]
		public virtual int? ProjectID
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
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected int? _TaskID;
		[PXDBInt(BqlField = typeof(POReceiptLine.taskID))]
		public virtual int? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(BqlField = typeof(POReceiptLine.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion


		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		protected String _POType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.pOType))]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
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
		[PXDBString(15, IsUnicode = true, BqlField = typeof(POReceiptLine.pONbr))]
		[PXUIField(DisplayName = "Order Nbr.")]
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
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
		protected Int32? _POLineNbr;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOLineNbr))]
		[PXUIField(DisplayName = "PO Line Nbr.")]
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion
	}
}
