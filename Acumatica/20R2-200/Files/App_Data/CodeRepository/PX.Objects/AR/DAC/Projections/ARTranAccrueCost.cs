using System;
using System.Diagnostics;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.DR;
using PX.Objects.PM;
using PX.Objects.CR;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.GL.DAC.Abstract;


namespace PX.Objects.AR
{
	[PXProjection(typeof(Select<ARTran,
			Where<ARTran.accrueCost, Equal<True>>>), Persistent = false)]
	[Serializable]
	public partial class ARTranAccrueCost : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ARTranAccrueCost>.By<tranType, refNbr, lineNbr>
		{
			public static ARTranAccrueCost Find(PXGraph graph, string tranType, string refNbr, int? lineNbr) => FindBy(graph, tranType, refNbr, lineNbr);
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(BqlField = typeof(ARTran.branchID))]
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARTran.tranType))]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARTran.refNbr))]
		[PXParent(typeof(Select<ARRegister, Where<ARRegister.docType, Equal<Current<ARTranAccrueCost.tranType>>, And<ARRegister.refNbr, Equal<Current<ARTranAccrueCost.refNbr>>>>>))]
		[PXParent(typeof(Select<SO.SOInvoice, Where<SO.SOInvoice.docType, Equal<Current<ARTranAccrueCost.tranType>>, And<SO.SOInvoice.refNbr, Equal<Current<ARTranAccrueCost.refNbr>>>>>))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(ARTran.lineNbr))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[ARTranInventoryItem(Filterable = true, BqlField = typeof(ARTran.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region IsStockItem
		public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
		[PXBool]
		[PXFormula(typeof(Selector<ARTranAccrueCost.inventoryID, InventoryItem.stkItem>))]
		public virtual Boolean? IsStockItem
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(ARTranAccrueCost.inventoryID), BqlField = typeof(ARTran.uOM))]
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
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		[PXDBQuantity(BqlField = typeof(ARTran.qty))]
		public virtual decimal? Qty
		{
			get;
			set;
		}
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBQuantity(BqlField = typeof(ARTran.baseQty))]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost> { }
		protected Boolean? _AccrueCost;
		[PXDBBool(BqlField = typeof(ARTran.accrueCost))]
		public virtual Boolean? AccrueCost
		{
			get
			{
				return this._AccrueCost;
			}
			set
			{
				this._AccrueCost = value;
			}
		}
		#endregion
		#region CostBasis
		public abstract class costBasis : PX.Data.BQL.BqlString.Field<costBasis> { }
		protected String _CostBasis;
		[PXDBString(1, IsFixed = true, BqlField = typeof(ARTran.costBasis))]
		public virtual String CostBasis
		{
			get
			{
				return this._CostBasis;
			}
			set
			{
				this._CostBasis = value;
			}
		}
		#endregion
		#region CuryAccruedCost
		public abstract class curyAccruedCost : PX.Data.BQL.BqlDecimal.Field<curyAccruedCost> { }
		protected Decimal? _CuryAccruedCost;
		[PXDBDecimal(BqlField = typeof(ARTran.curyAccruedCost))]
		public virtual Decimal? CuryAccruedCost
		{
			get
			{
				return this._CuryAccruedCost;
			}
			set
			{
				this._CuryAccruedCost = value;
			}
		}
		#endregion
		#region AccruedCost
		public abstract class accruedCost : PX.Data.BQL.BqlDecimal.Field<accruedCost> { }
		protected Decimal? _AccruedCost;
		[PXDBDecimal(BqlField = typeof(ARTran.accruedCost))]
		public virtual Decimal? AccruedCost
		{
			get
			{
				return this._AccruedCost;
			}
			set
			{
				this._AccruedCost = value;
			}
		}
		#endregion
		#region ExpenseAccrualAccountID
		public abstract class expenseAccrualAccountID : PX.Data.BQL.BqlInt.Field<expenseAccrualAccountID> { }
		protected Int32? _ExpenseAccrualAccountID;
		[Account(typeof(ARTranAccrueCost.branchID), BqlField = typeof(ARTran.expenseAccrualAccountID))]
		public virtual Int32? ExpenseAccrualAccountID
		{
			get
			{
				return this._ExpenseAccrualAccountID;
			}
			set
			{
				this._ExpenseAccrualAccountID = value;
			}
		}
		#endregion
		#region ExpenseAccrualSubID
		public abstract class expenseAccrualSubID : PX.Data.BQL.BqlInt.Field<expenseAccrualSubID> { }
		protected Int32? _ExpenseAccrualSubID;
		[SubAccount(typeof(ARTranAccrueCost.expenseAccrualAccountID), typeof(ARTranAccrueCost.branchID), true, BqlField = typeof(ARTran.expenseAccrualSubID))]
		public virtual Int32? ExpenseAccrualSubID
		{
			get
			{
				return this._ExpenseAccrualSubID;
			}
			set
			{
				this._ExpenseAccrualSubID = value;
			}
		}
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.BQL.BqlInt.Field<expenseAccountID> { }
		protected Int32? _ExpenseAccountID;
		[Account(typeof(ARTranAccrueCost.branchID), BqlField = typeof(ARTran.expenseAccountID))]
		public virtual Int32? ExpenseAccountID
		{
			get
			{
				return this._ExpenseAccountID;
			}
			set
			{
				this._ExpenseAccountID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;
		[SubAccount(typeof(ARTranAccrueCost.expenseAccountID), typeof(ARTranAccrueCost.branchID), true, BqlField = typeof(ARTran.expenseSubID))]
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
		#endregion
	}
}
