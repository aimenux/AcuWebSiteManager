using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Data;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.RQ
{

	//Add fields
	[System.SerializableAttribute()]
    [PXHidden]
	public partial class RQRequestLineBudget : RQRequestLine
	{
		#region Keys
		public new class PK : PrimaryKeyOf<RQRequestLineBudget>.By<orderNbr, lineNbr>
		{
			public static RQRequestLineBudget Find(PXGraph graph, string orderNbr, int? lineNbr) => FindBy(graph, orderNbr, lineNbr);
		}
		public new static class FK
		{
			public class Request : RQRequest.PK.ForeignKeyOf<RQRequestLineBudget>.By<orderNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<RQRequestLineBudget>.By<inventoryID> { }
		}
		#endregion

		#region OrderNbr
		public new abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		#endregion
		#region EstExtCost
		public new abstract class estExtCost : PX.Data.BQL.BqlDecimal.Field<estExtCost> { }
		#endregion
		#region CuryEstExtCost
		public new abstract class curyEstExtCost : PX.Data.BQL.BqlDecimal.Field<curyEstExtCost> { }
		#endregion
		#region ExpenseAcctID
		public new abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		#endregion
		#region ExpenseSubID
		public new abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		#endregion
		#region AprovedAmt
		public abstract class aprovedAmt : PX.Data.BQL.BqlDecimal.Field<aprovedAmt> { }
		protected Decimal? _AprovedAmt;
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolTrue>>, RQRequestLineBudget.estExtCost>, decimal0>), typeof(decimal))]
		public virtual Decimal? AprovedAmt
		{
			get
			{
				return this._AprovedAmt;
			}
			set
			{
				this._AprovedAmt = value;
			}
		}
		#endregion
		#region UnaprovedAmt
		public abstract class unaprovedAmt : PX.Data.BQL.BqlDecimal.Field<unaprovedAmt> { }
		protected Decimal? _UnaprovedAmt;
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolFalse>>, RQRequestLineBudget.estExtCost>, decimal0>), typeof(decimal))]
		public virtual Decimal? UnaprovedAmt
		{
			get
			{
				return this._UnaprovedAmt;
			}
			set
			{
				this._UnaprovedAmt = value;
			}
		}
		#endregion
		#region CuryAprovedAmt
		public abstract class curyAprovedAmt : PX.Data.BQL.BqlDecimal.Field<curyAprovedAmt> { }
		protected Decimal? _CuryAprovedAmt;
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolTrue>>, RQRequestLineBudget.curyEstExtCost>, decimal0>), typeof(decimal))]
		public virtual Decimal? CuryAprovedAmt
		{
			get
			{
				return this._CuryAprovedAmt;
			}
			set
			{
				this._CuryAprovedAmt = value;
			}
		}
		#endregion
		#region CuryUnaprovedAmt
		public abstract class curyUnaprovedAmt : PX.Data.BQL.BqlDecimal.Field<curyUnaprovedAmt> { }
		protected Decimal? _CuryUnaprovedAmt;
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolFalse>>, RQRequestLineBudget.curyEstExtCost>, decimal0>), typeof(decimal))]
		public virtual Decimal? CuryUnaprovedAmt
		{
			get
			{
				return this._CuryUnaprovedAmt;
			}
			set
			{
				this._CuryUnaprovedAmt = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select2<RQRequestLineBudget,
		InnerJoin<RQRequest, On<RQRequest.orderNbr, Equal<RQRequestLineBudget.orderNbr>>>>))]
    [PXCacheName(Messages.RQBudget)]
    [Serializable]
	public partial class RQBudget : IBqlTable
	{
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(RQRequest.curyID))]
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
		[PXDBLong(BqlField = typeof(RQRequest.curyInfoID))]
		//[CurrencyInfo(ModuleCode = "PO")]
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
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;
		[Account(SuppressCurrencyValidation = true, DisplayName = "Account", Visibility = PXUIVisibility.Visible, Filterable = false, DescriptionField = typeof(Account.description), IsKey = true, BqlField = typeof(RQRequestLineBudget.expenseAcctID))]
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

		[SubAccount(typeof(RQBudget.expenseAcctID), DisplayName = "Sub.", Visibility = PXUIVisibility.Visible, Filterable = true, DescriptionField = typeof(Sub.description), IsKey = true, BqlField = typeof(RQRequestLineBudget.expenseSubID))]
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
		#region DocRequestAmt
		public abstract class docRequestAmt : PX.Data.BQL.BqlDecimal.Field<docRequestAmt> { }
		protected Decimal? _DocRequestAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDecimal()]
		[PXUIField(DisplayName = "Document Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? DocRequestAmt
		{
			get
			{
				return this._DocRequestAmt;
			}
			set
			{
				this._DocRequestAmt = value;
			}
		}
		#endregion
		#region CuryDocRequestAmt
		public abstract class curyDocRequestAmt : PX.Data.BQL.BqlDecimal.Field<curyDocRequestAmt> { }
		protected Decimal? _CuryDocRequestAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDecimal()]
		public virtual Decimal? CuryDocRequestAmt
		{
			get
			{
				return this._CuryDocRequestAmt;
			}
			set
			{
				this._CuryDocRequestAmt = value;
			}
		}
		#endregion		
		#region RequestAmt
		public abstract class requestAmt : PX.Data.BQL.BqlDecimal.Field<requestAmt> { }
		protected Decimal? _RequestAmt;		
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(BqlField = typeof(RQRequestLineBudget.estExtCost))]
		[PXUIField(DisplayName = "Request Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? RequestAmt
		{
			get
			{
				return this._RequestAmt;
			}
			set
			{
				this._RequestAmt = value;
			}
		}
		#endregion		
		#region CuryRequestAmt
		public abstract class curyRequestAmt : PX.Data.BQL.BqlDecimal.Field<curyRequestAmt> { }
		protected Decimal? _CuryRequestAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(BqlField = typeof(RQRequestLineBudget.curyEstExtCost))]
		public virtual Decimal? CuryRequestAmt
		{
			get
			{
				return this._CuryRequestAmt;
			}
			set
			{
				this._CuryRequestAmt = value;
			}
		}
		#endregion		
		#region AprovedAmt
		public abstract class aprovedAmt : PX.Data.BQL.BqlDecimal.Field<aprovedAmt> { }
		protected Decimal? _AprovedAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]		
		[PXBaseCury]
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolTrue>>, RQRequestLineBudget.estExtCost>, decimal0>), typeof(decimal))]
		[PXUIField(DisplayName = "Approved Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? AprovedAmt
		{
			get
			{
				return this._AprovedAmt;
			}
			set
			{
				this._AprovedAmt = value;
			}
		}
		#endregion
		#region CuryAprovedAmt
		public abstract class curyAprovedAmt : PX.Data.BQL.BqlDecimal.Field<curyAprovedAmt> { }
		protected Decimal? _CuryAprovedAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXBaseCury]
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolTrue>>, RQRequestLineBudget.curyEstExtCost>, decimal0>), typeof(decimal))]
		public virtual Decimal? CuryAprovedAmt
		{
			get
			{
				return this._CuryAprovedAmt;
			}
			set
			{
				this._CuryAprovedAmt = value;
			}
		}
		#endregion
		#region UnaprovedAmt
		public abstract class unaprovedAmt : PX.Data.BQL.BqlDecimal.Field<unaprovedAmt> { }
		protected Decimal? _UnaprovedAmt;
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolFalse>>, RQRequestLineBudget.estExtCost>, decimal0>), typeof(decimal))]
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unapproved Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? UnaprovedAmt
		{
			get
			{
				return this._UnaprovedAmt;
			}
			set
			{
				this._UnaprovedAmt = value;
			}
		}
		#endregion		
		#region CuryUnaprovedAmt
		public abstract class curyUnaprovedAmt : PX.Data.BQL.BqlDecimal.Field<curyUnaprovedAmt> { }
		protected Decimal? _CuryUnaprovedAmt;
		[PXDBCalced(typeof(Switch<Case<Where<RQRequest.approved, Equal<boolFalse>>, RQRequestLineBudget.curyEstExtCost>, decimal0>), typeof(decimal))]
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		public virtual Decimal? CuryUnaprovedAmt
		{
			get
			{
				return this._CuryUnaprovedAmt;
			}
			set
			{
				this._CuryUnaprovedAmt = value;
			}
		}
		#endregion		
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlField = typeof(RQRequest.orderNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]		
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

		[PXDBDate(BqlField = typeof(RQRequest.orderDate))]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[PXDBString(BqlField=typeof(RQRequest.finPeriodID))]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion		
		#region BudgetAmt
		public abstract class budgetAmt : PX.Data.BQL.BqlDecimal.Field<budgetAmt> { }
		protected Decimal? _BudgetAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXBaseCury]
		[PXUIField(DisplayName = "Budget Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? BudgetAmt
		{
			get
			{
				return this._BudgetAmt;
			}
			set
			{
				this._BudgetAmt = value;
			}
		}
		#endregion
		#region UsageAmt
		public abstract class usageAmt : PX.Data.BQL.BqlDecimal.Field<usageAmt> { }
		protected Decimal? _UsageAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXBaseCury]
		[PXUIField(DisplayName = "Amount Spent", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? UsageAmt
		{
			get
			{
				return this._UsageAmt;
			}
			set
			{
				this._UsageAmt = value;
			}
		}
		#endregion	
	}	
}
