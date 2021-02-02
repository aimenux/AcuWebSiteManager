using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	[Serializable()]
	[PXProjection(typeof(Select2<POLine,
		InnerJoin<POOrder, On<POOrder.orderType, Equal<POLine.orderType>, 
			And<POOrder.orderNbr, Equal<POLine.orderNbr>, And<POOrder.approved, Equal<True>>>>,
		LeftJoin<PMCostCode, On<POLine.costCodeID, Equal<PMCostCode.costCodeID>>>>>))]
	public partial class POLinePM : PX.Data.IBqlTable
	{
		//POLine fields:
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POLine.orderType))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Type", Enabled = false)]
		[PO.POOrderType.List()]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POLine.orderNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<POLinePM.orderType>>>>), DescriptionField = typeof(POOrder.orderDesc))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POLine.lineNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Line Nbr.")]
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
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POLine.lineType))]
		[PO.POLineType.List()]
		[PXUIField(DisplayName = "Status", Enabled = false)]
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
		[Inventory(Filterable = true, BqlField = typeof(POLine.inventoryID), Enabled = false)]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(BqlField = typeof(POLine.subItemID), Enabled = false)]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[AP.Vendor(typeof(Search<BAccountR.bAccountID,
			Where<AP.Vendor.type, NotEqual<BAccountType.employeeType>>>),
			BqlField = typeof(POLine.vendorID), Enabled = false)]
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
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;
		[PXDBDate(BqlField = typeof(POLine.orderDate))]
		[PXUIField(DisplayName = "Order Date", Enabled = false)]
		public virtual DateTime? OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this._OrderDate = value;
			}
		}
		#endregion
		#region PromisedDate
		public abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }
		protected DateTime? _PromisedDate;
		[PXDBDate(BqlField = typeof(POLine.promisedDate))]
		[PXUIField(DisplayName = "Promised", Enabled = false, Visible = false)]
		public virtual DateTime? PromisedDate
		{
			get
			{
				return this._PromisedDate;
			}
			set
			{
				this._PromisedDate = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
		protected Boolean? _Cancelled;
		[PXDBBool(BqlField = typeof(POLine.cancelled))]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;
		[PXDBBool(BqlField = typeof(POLine.completed))]
		public virtual Boolean? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(BqlField = typeof(POLine.siteID))]
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
		[PXDBString(6, IsUnicode = true, BqlField = typeof(POLine.uOM))]
		[PXUIField(DisplayName = "UOM", Enabled = false)]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity(BqlField = typeof(POLine.orderQty))]
		[PXUIField(DisplayName = "Order Qty.", Enabled = false)]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		protected Decimal? _BaseOrderQty;
		[PXDBQuantity(BqlField = typeof(POLine.baseOrderQty))]
		public virtual Decimal? BaseOrderQty
		{
			get
			{
				return this._BaseOrderQty;
			}
			set
			{
				this._BaseOrderQty = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBQuantity(BqlField = typeof(POLine.openQty))]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region BaseOpenQty
		public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
		protected Decimal? _BaseOpenQty;
		[PXDBDecimal(6, BqlField = typeof(POLine.baseOpenQty))]
		public virtual Decimal? BaseOpenQty
		{
			get
			{
				return this._BaseOpenQty;
			}
			set
			{
				this._BaseOpenQty = value;
			}
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
		protected Decimal? _ReceivedQty;
		[PXUIField(DisplayName = "Qty. On Receipts")]
		[PXDBQuantity(BqlField = typeof(POLine.receivedQty))]
		public virtual Decimal? ReceivedQty
		{
			get
			{
				return this._ReceivedQty;
			}
			set
			{
				this._ReceivedQty = value;
			}
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }
		protected Decimal? _BaseReceivedQty;
		[PXDBDecimal(6, BqlField = typeof(POLine.baseReceivedQty))]
		public virtual Decimal? BaseReceivedQty
		{
			get
			{
				return this._BaseReceivedQty;
			}
			set
			{
				this._BaseReceivedQty = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }
		[PXDBPriceCost(BqlField = typeof(POLine.curyUnbilledAmt))]
		public virtual Decimal? CuryUnbilledAmt
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POLine.tranDesc))]
		[PXUIField(DisplayName = "Line Description", Enabled = false)]
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
		[ProjectBase(BqlField =typeof(POLine.projectID))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveOrInPlanningProjectTask(typeof(POLinePM.projectID), GL.BatchModule.PO, DisplayName = "Project Task", BqlField =typeof(POLine.taskID))]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(BqlField = typeof(POLine.costCodeID), SkipVerification = true)]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		
		[PXDBLong(BqlField = typeof(POLine.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitCost
		public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }
		[PXUIField(DisplayName = "Unit Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBPriceCost(BqlField = typeof(POLine.curyUnitCost))]
		public virtual Decimal? CuryUnitCost
		{
			get;
			set;
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

		[PXDBBaseCury(BqlField = typeof(POLine.unitCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get;
			set;
		}
		#endregion
		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
		[PXUIField(DisplayName = "Ext. Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(lineAmt), BaseCalc = false, BqlField = typeof(POLine.curyLineAmt))]
		public virtual Decimal? CuryLineAmt
		{
			get;
			set;
		}
		#endregion
		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }
		
		[PXDBBaseCury(BqlField = typeof(POLine.lineAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Cost in Base Currency")]
		public virtual Decimal? LineAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }
		[PXUIField(DisplayName = "Line Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(extCost), BaseCalc = false, BqlField = typeof(POLine.curyExtCost))]
		public virtual Decimal? CuryExtCost
		{
			get;
			set;
		}
		#endregion
		#region ExtCost
		public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

		[PXDBBaseCury(BqlField = typeof(POLine.extCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Amount in Base Currency")]
		public virtual Decimal? ExtCost
		{
			get;
			set;
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXUIField(DisplayName = "Alternate ID", Visible = false)]
		[PXDBString(50, IsUnicode = true, BqlField =typeof(POLine.alternateID))]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;
		[PXDBInt(BqlField = typeof(POLine.expenseAcctID))]
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
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(BqlField = typeof(POLine.unbilledQty))]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion

		//POOrder Fields:
		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
		protected String _VendorRefNbr;
		[PXDBString(40, BqlField = typeof(POOrder.vendorRefNbr))]
		[PXUIField(DisplayName = "Vendor Ref.", Enabled = false, Visible = false)]
		public virtual String VendorRefNbr
		{
			get
			{
				return this._VendorRefNbr;
			}
			set
			{
				this._VendorRefNbr = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrder.curyID))]
		[PXUIField(DisplayName = "Currency")]
		[PXDefault(typeof(Search<GL.Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
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

		//PMCostCode Fields:
		#region CostCodeCD
		public abstract class costCodeCD : PX.Data.BQL.BqlString.Field<costCodeCD> { }

		[PXDBString(IsUnicode = true, BqlField = typeof(PMCostCode.costCodeCD), InputMask = "")]
		[PXUIField(DisplayName = "Cost Code", FieldClass = CostCodeAttribute.COSTCODE)]
		public virtual String CostCodeCD
		{
			get;
			set;
		}
		#endregion

		//Calculated fields:
		#region CalcOpenQty
		public abstract class calcOpenQty : PX.Data.BQL.BqlDecimal.Field<calcOpenQty> { }
		[PXQuantity]
		[PXUIField(DisplayName = "Open Qty.", Enabled = false)]
		public virtual Decimal? CalcOpenQty
		{
			get
			{
				return Math.Min(this.OpenQty.GetValueOrDefault(), this.UnbilledQty.GetValueOrDefault());
			}
		}
		#endregion
		#region CalcCuryOpenAmt
		public abstract class calcCuryOpenAmt : PX.Data.BQL.BqlDecimal.Field<calcCuryOpenAmt> { }
		[PXPriceCost]
		[PXUIField(DisplayName = "Open Amount")]
		public virtual Decimal? CalcCuryOpenAmt
		{
			get
			{
				if (this.OrderQty.GetValueOrDefault() == 0 || this.CalcOpenQty.GetValueOrDefault() == 0) return this.CuryUnbilledAmt;
				Decimal unitCost = this.CuryLineAmt.GetValueOrDefault() / this.OrderQty.GetValueOrDefault();
				var result = Math.Min(this.CuryUnbilledAmt.GetValueOrDefault(), this.CalcOpenQty.GetValueOrDefault() * unitCost);
				return result;
			}
		}
		#endregion
	}

	[Serializable()]
	[PXProjection(typeof(Select<POOrder>))]
	public partial class POOrderPM : PX.Data.IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POOrder.orderType))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Type", Enabled = false)]
		[PO.POOrderType.List()]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POOrder.orderNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<POLinePM.orderType>>>>), DescriptionField = typeof(POOrder.orderDesc))]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[AP.Vendor(typeof(Search<BAccountR.bAccountID,
			Where<AP.Vendor.type, NotEqual<BAccountType.employeeType>>>),
			BqlField = typeof(POOrder.vendorID), Enabled = false)]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrder.curyID))]
		[PXUIField(DisplayName = "Currency")]
		[PXDefault(typeof(Search<GL.Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
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
	}
	
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXProjection(typeof(Select<PMBudget>), Persistent = false)]
	public class PMBudgetedCostCode : PX.Data.IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField =typeof(PMBudget.projectID))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
		public virtual Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.costCodeID))]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.accountGroupID))]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		
		[PXDBInt(IsKey = true, BqlField =typeof(PMBudget.inventoryID))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1, BqlField =typeof(PMBudget.type))]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(255, IsUnicode = true, BqlField = typeof(PMBudget.description))]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
	}

	

}
