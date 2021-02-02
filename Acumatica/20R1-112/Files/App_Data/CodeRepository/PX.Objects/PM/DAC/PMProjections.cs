using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using System;

namespace PX.Objects.PM
{
	[PXCacheName(PO.Messages.POLineShort)]
	[Serializable]
	[PXProjection(typeof(Select2<POLine,
		InnerJoin<POOrder, On<POOrder.orderType, Equal<POLine.orderType>, 
			And<POOrder.orderNbr, Equal<POLine.orderNbr>, And<POOrder.approved, Equal<True>, And<POOrder.hold, Equal<False>>>>>,
		LeftJoin<PMCostCode, On<POLine.costCodeID, Equal<PMCostCode.costCodeID>>>>>))]
	public partial class POLinePM : IBqlTable
	{
		//POLine fields:
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXUnboundDefault(false)]
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

	[PXCacheName(PO.Messages.POOrder)]
	[Serializable()]
	[PXProjection(typeof(Select<POOrder>))]
	public partial class POOrderPM : IBqlTable
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

	[PXCacheName(Messages.Budget)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXProjection(typeof(Select<PMBudget>), Persistent = false)]
	public class PMBudgetedCostCode : IBqlTable
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
	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXProjection(typeof(Select4<PMCostBudget, Where<PMCostBudget.isProduction, Equal<True>,
						And<PMCostBudget.type, Equal<GL.AccountType.expense>>>,
						Aggregate<GroupBy<PMCostBudget.projectID,
						GroupBy<PMCostBudget.revenueTaskID,
						GroupBy<PMCostBudget.revenueInventoryID,
						Sum<PMCostBudget.curyRevisedAmount,
						Sum<PMCostBudget.curyActualAmount>>>>>>>), Persistent = false)]
	public class PMProductionBudget : PX.Data.IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region RevenueTaskID
		public abstract class revenueTaskID : PX.Data.BQL.BqlInt.Field<revenueTaskID>
		{
		}
		
		[PXDBInt(IsKey = true, BqlField = typeof(PMCostBudget.revenueTaskID))]
		public virtual Int32? RevenueTaskID
		{
			get;
			set;
		}
		#endregion
		#region RevenueInventoryID
		public abstract class revenueInventoryID : PX.Data.BQL.BqlInt.Field<revenueInventoryID>
		{
		}
		[PXDBInt(IsKey = true, BqlField = typeof(PMCostBudget.revenueInventoryID))]
		public virtual Int32? RevenueInventoryID
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		public abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}
		[PXDBBaseCury(BqlField = typeof(PMCostBudget.curyRevisedAmount))]
		public virtual Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryActualAmount
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }
		[PXDBBaseCury(BqlField = typeof(PMCostBudget.curyActualAmount))]
		public virtual decimal? CuryActualAmount
		{
			get;
			set;
		}
		#endregion
	}

	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXProjection(typeof(Select4<PMRevenueBudget, Where<PMRevenueBudget.type, Equal<GL.AccountType.income>>,
						Aggregate<GroupBy<PMRevenueBudget.projectID,
						GroupBy<PMRevenueBudget.projectTaskID,
						GroupBy<PMRevenueBudget.inventoryID,
						Sum<PMRevenueBudget.curyRevisedAmount,
						Sum<PMRevenueBudget.curyInvoicedAmount>>>>>>>), Persistent = false)]
	public class PMRevenueTotal : PX.Data.IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		[PXDBInt(IsKey = true, BqlField = typeof(PMRevenueBudget.projectTaskID))]
		public virtual Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		[PXDBInt(IsKey = true, BqlField = typeof(PMRevenueBudget.revenueInventoryID))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		public abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}
		[PXDBBaseCury(BqlField = typeof(PMRevenueBudget.curyRevisedAmount))]
		public virtual Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryInvoicedAmount
		public abstract class curyInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyInvoicedAmount> { }
		[PXDBBaseCury(BqlField = typeof(PMRevenueBudget.curyInvoicedAmount))]
		public virtual decimal? CuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryAmountToInvoiceProjected
		public abstract class curyAmountToInvoiceProjected : PX.Data.BQL.BqlDecimal.Field<curyAmountToInvoiceProjected> { }
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBaseCury]
		public virtual decimal? CuryAmountToInvoiceProjected
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// Aggregate Total Sum of Revenue(Income) budget lines of a Project.
	/// </summary>
	[PXCacheName(Messages.ContractTotal)]
	[PXProjection(typeof(Select4<PMBudget,
				Where<PMBudget.type, Equal<AccountType.income>>,
				Aggregate<GroupBy<PMBudget.projectID,
				Sum<PMBudget.curyRevisedAmount,
				Sum<PMBudget.curyAmount,
				Sum<PMBudget.curyInvoicedAmount,
				Sum<PMBudget.curyActualAmount,
				Sum<PMBudget.curyTotalRetainedAmount,
				Sum<PMBudget.curyAmountToInvoice>>>>>>>>>))]
	public class PMProjectRevenueTotal : PX.Data.IBqlTable
	{
		#region ProjectID
		/// <exclude/>
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		/// <summary>
		/// Project
		/// </summary>
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region CuryAmount
		/// <exclude/>
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}
		/// <summary>
		/// Contract (Revenue) Total
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Contract Total", Enabled = false)]
		public virtual Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		/// <exclude/>
		public abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}
		/// <summary>
		/// Revised Contract Total
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyRevisedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Contract Total", Enabled = false)]
		public virtual Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryInvoicedAmount
		/// <exclude/>
		public abstract class curyInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyInvoicedAmount>
		{
		}
		/// <summary>
		/// Total Draft Invoiced Amount for Project
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyInvoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount", Enabled = false)]
		public virtual Decimal? CuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryActualAmount
		/// <exclude/>
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }
		/// <summary>
		/// Total Actual Amount for Project
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyActualAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount", Enabled = false)]
		public virtual decimal? CuryActualAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryAmountToInvoice
		/// <exclude/>
		public abstract class curyAmountToInvoice : PX.Data.BQL.BqlDecimal.Field<curyAmountToInvoice>
		{
		}
		/// <summary>
		/// Total Amount to Invoice for Project
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyAmountToInvoice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount to Invoice")]
		public virtual Decimal? CuryAmountToInvoice
		{
			get;
			set;
		}
		#endregion

		#region CuryTotalRetainedAmount
		/// <exclude/>
		public abstract class curyTotalRetainedAmount : PX.Data.BQL.BqlDecimal.Field<curyTotalRetainedAmount>
		{
		}

		/// <summary>
		///  Total Retained Amount (including Draft) for Project
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyTotalRetainedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Retained Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryTotalRetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region ContractCompletedPct
		/// <exclude/>
		public abstract class contractCompletedPct : PX.Data.BQL.BqlDecimal.Field<contractCompletedPct> { }
		/// <summary>
		/// Contract Completed % (without Change Orders)
		/// </summary>
		[PXDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Completed (%)", Enabled = false)]
		public virtual decimal? ContractCompletedPct
		{
			[PXDependsOnFields(typeof(curyAmount), typeof(curyInvoicedAmount), typeof(PMBudget.curyActualAmount))]
			get
			{
				if (CuryAmount != 0)
					return ((CuryInvoicedAmount + CuryActualAmount) / CuryAmount) * 100;
				else
					return 0;
			}
		}
		#endregion
		#region ContractCompletedWithCOPct
		/// <exclude/>
		public abstract class contractCompletedWithCOPct : PX.Data.BQL.BqlDecimal.Field<contractCompletedWithCOPct> { }
		/// <summary>
		/// Contract Completed % (with Change Orders)
		/// </summary>
		[PXDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Completed (%)", Enabled = false)]
		public virtual decimal? ContractCompletedWithCOPct
		{
			[PXDependsOnFields(typeof(curyRevisedAmount), typeof(curyInvoicedAmount), typeof(PMBudget.curyActualAmount))]
			get
			{
				if (CuryRevisedAmount != 0)
					return ((CuryInvoicedAmount + CuryActualAmount) / CuryRevisedAmount) * 100;
				else
					return 0;
			}
		}
		#endregion
		
	}

	[PXHidden]
	[Serializable]
	[PXProjection(typeof(Select4<PMChangeOrder,
		Where<PMChangeOrder.refNbr, Less<Current<PMChangeOrderPrevioslyTotalAmount.refNbr>>,
			And<PMChangeOrder.released, Equal<True>,
			And<PMChangeOrder.reverseStatus, NotEqual<ChangeOrderReverseStatus.reversed>,
			And<PMChangeOrder.reverseStatus, NotEqual<ChangeOrderReverseStatus.reversal>>>>>,
		Aggregate<GroupBy<PMChangeOrder.projectID,
			Sum<PMChangeOrder.revenueTotal>>>>))]
	public class PMChangeOrderPrevioslyTotalAmount : IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(PMChangeOrder.refNbr.Length, IsUnicode = true, BqlField = typeof(PMChangeOrder.refNbr))]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrder.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region RevenueTotal
		public abstract class revenueTotal : PX.Data.BQL.BqlDecimal.Field<revenueTotal> { }
		[PXDBBaseCury(BqlField = typeof(PMChangeOrder.revenueTotal))]
		public virtual decimal? RevenueTotal
		{
			get; 
			set;
		}
		#endregion
	}
}
