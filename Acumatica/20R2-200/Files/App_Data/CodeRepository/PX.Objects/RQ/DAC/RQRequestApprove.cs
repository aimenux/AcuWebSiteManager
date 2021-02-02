using PX.SM;

namespace PX.Objects.RQ
{
	using System;
	using PX.Data;
	using PX.Objects.AP;
	using PX.Objects.CS;
	using PX.Objects.CR;
	using PX.Objects.CM;
	using PX.Objects.IN;
	using PX.Objects.AR;
	using PX.TM;
	using PX.Objects.EP;
	using PX.Objects.PO;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]	
    [PXHidden]
	public partial class RQRequestApprove : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<RQRequestApprove>.By<orderNbr, lineNbr>
		{
			public static RQRequestApprove Find(PXGraph graph, string orderNbr, int? lineNbr) => FindBy(graph, orderNbr, lineNbr);
		}
		public static class FK
		{
			public class RequestClass : RQRequestClass.PK.ForeignKeyOf<RQRequestApprove>.By<reqClassID> { }
		}
		#endregion

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
		#region ReqClassID
		public abstract class reqClassID : PX.Data.BQL.BqlString.Field<reqClassID> { }
		protected string _ReqClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(RQSetup.defaultReqClassID))]
		[PXUIField(DisplayName = "Request Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(RQRequestClass.reqClassID), DescriptionField = typeof(RQRequestClass.descr))]
		public virtual string ReqClassID
		{
			get
			{
				return this._ReqClassID;
			}
			set
			{
				this._ReqClassID = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]		
		[PXSelectorAttribute(
			typeof(RQRequest.orderNbr),
			typeof(RQRequest.orderNbr),
			typeof(RQRequest.orderDate),
			typeof(RQRequest.status),
			typeof(RQRequest.employeeID),
			typeof(RQRequest.departmentID),
			typeof(RQRequest.locationID),
			Filterable = true)]
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
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Priority
		public abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }
		protected Int32? _Priority;
		[PXDBInt]
		[PXUIField]
		[PXDefault(1)]
		[PXIntList(new int[] { 0, 1, 2 },
			new string[] { "Low", "Normal", "High" })]
		public virtual Int32? Priority
		{
			get
			{
				return this._Priority;
			}
			set
			{
				this._Priority = value;
			}
		}
		#endregion					
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion		
		#region CheckBudget
		public abstract class checkBudget : PX.Data.BQL.BqlBool.Field<checkBudget> { }
		protected Boolean? _CheckBudget;
		[PXDBBool()]
		[PXUIField(DisplayName = "Check Budget", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? CheckBudget
		{
			get
			{
				return this._CheckBudget;
			}
			set
			{
				this._CheckBudget = value;
			}
		}
		#endregion		

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt()]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[RQRequesterSelector(typeof(RQRequestApprove.reqClassID))]
		[PXUIField(DisplayName = "Requested By", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
		#region DepartmentID
		public abstract class departmentID : PX.Data.BQL.BqlString.Field<departmentID> { }
		protected String _DepartmentID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<EPEmployee.departmentID, Where<EPEmployee.bAccountID, Equal<Current<RQRequestApprove.employeeID>>>>))]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DepartmentID
		{
			get
			{
				return this._DepartmentID;
			}
			set
			{
				this._DepartmentID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected int? _LocationID;
		[PXDefault(typeof(Search<EPEmployee.defLocationID, Where<EPEmployee.bAccountID, Equal<Current<RQRequestApprove.employeeID>>>>))]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<RQRequestApprove.employeeID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		public int? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = GL.BatchModule.PO)]
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

		#region Purpose
		public abstract class purpose : PX.Data.BQL.BqlString.Field<purpose> { }
		protected String _Purpose;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Purpose")]
		public virtual String Purpose
		{
			get
			{
				return this._Purpose;
			}
			set
			{
				this._Purpose = value;
			}
		}
		#endregion	

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]		
		public virtual int? LineNbr
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
		[PXDBString(2, IsFixed = true)]
		[POLineType.DefaultList()]
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
		public abstract class inventoryID : PX.Data.BQL.BqlString.Field<inventoryID> { }
		protected string _InventoryID;
		[InventoryRaw(Filterable = true)]
		public virtual string InventoryID
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
		[SubItem(typeof(RQRequestApprove.inventoryID))]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<RQRequestApprove.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(RQRequestApprove.inventoryID), DisplayName = "UOM")]
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
		[PXDBQuantity(typeof(RQRequestApprove.uOM), typeof(RQRequestApprove.baseOrderQty), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Qty.", Visibility = PXUIVisibility.Visible)]
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
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CuryEstUnitCost
		public abstract class curyEstUnitCost : PX.Data.BQL.BqlDecimal.Field<curyEstUnitCost> { }
		protected Decimal? _CuryEstUnitCost;

		[PXDBDecimal]
		[PXUIField(DisplayName = "Est. Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryEstUnitCost
		{
			get
			{
				return this._CuryEstUnitCost;
			}
			set
			{
				this._CuryEstUnitCost = value;
			}
		}
		#endregion
		#region CuryEstExtCost
		public abstract class curyEstExtCost : PX.Data.BQL.BqlDecimal.Field<curyEstExtCost> { }
		protected Decimal? _CuryEstExtCost;
		[PXDBCurrency(typeof(RQRequestApprove.curyInfoID), typeof(RQRequestApprove.estExtCost))]
		[PXUIField(DisplayName = "Est. Ext. Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<RQRequestApprove.orderQty, RQRequestApprove.curyEstUnitCost>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryEstExtCost
		{
			get
			{
				return this._CuryEstExtCost;
			}
			set
			{
				this._CuryEstExtCost = value;
			}
		}
		#endregion
		#region EstExtCost
		public abstract class estExtCost : PX.Data.BQL.BqlDecimal.Field<estExtCost> { }
		protected Decimal? _EstExtCost;

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstExtCost
		{
			get
			{
				return this._EstExtCost;
			}
			set
			{
				this._EstExtCost = value;
			}
		}
		#endregion
		#region RequestedDate
		public abstract class requestedDate : PX.Data.BQL.BqlDateTime.Field<requestedDate> { }
		protected DateTime? _RequestedDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Required Date")]
		public virtual DateTime? RequestedDate
		{
			get
			{
				return this._RequestedDate;
			}
			set
			{
				this._RequestedDate = value;
			}
		}
		#endregion
		#region PromisedDate
		public abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }
		protected DateTime? _PromisedDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Promised Date")]
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
		#region Escalated
		public abstract class escalated : PX.Data.BQL.BqlBool.Field<escalated> { }
		protected bool? _Escalated;
		[PXBool]
		public virtual bool? Escalated
		{
			get
			{
				return this._Escalated;
			}
			set
			{
				this._Escalated = value;
			}
		}
		#endregion
	}
}
