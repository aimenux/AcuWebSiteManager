using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	[PXCacheName(PX.Objects.PM.Messages.Budget)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMBudgetRecord : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlString.Field<recordID>
		{
		}
		[PXDBString(IsKey = true)]
		[PXDefault]
		public virtual string RecordID
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected Int32? _ProjectID;
		[PXDBInt()]
		public virtual Int32? ProjectID
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
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		public int? TaskID => ProjectTaskID;		
		[ProjectTask(typeof(PMBudgetRecord.projectID))]
		public virtual Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		protected Int32? _CostCodeID;
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		[CostCode(null, typeof(projectTaskID), null, typeof(accountGroupID), true, Filterable = false, SkipVerification = true)]
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
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}
		protected Int32? _AccountGroupID;
		[AccountGroupAttribute()]
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
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
		[PMInventorySelector]
		[PXParent(typeof(Select<
			InventoryItem,
			Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>))]
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

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}
		protected string _Type;
		[PXDBString(1)]
		[PXDefault]
		[PMAccountType.List]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[CurrencyInfo(typeof(PMProject.curyInfoID))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}
		protected String _Description;
		[PXDBString(Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
		{
		}
		protected Decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Quantity", Enabled = false)]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		protected String _UOM;
		[PXDefault(typeof(Search<
			InventoryItem.baseUnit,
			Where<InventoryItem.inventoryID, Equal<Current<PMBudget.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(inventoryID))]
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
		#region CuryUnitRate
		public abstract class curyUnitRate : PX.Data.BQL.BqlDecimal.Field<curyUnitRate> { }
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(PMBudget.curyInfoID), typeof(PMBudget.rate))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Rate", Visible=false)]
		public virtual decimal? CuryUnitRate
		{
			get;
			set;
		}
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		[PXDBPriceCost]
		public virtual decimal? Rate
		{
			get;
			set;
		}
		#endregion
		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.amount))]
		[PXFormula(typeof(Mult<qty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount", Visible=false)]
		public virtual Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount in Base Currency", Visible=false)]
		public virtual Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region RevisedQty
		public abstract class revisedQty : PX.Data.BQL.BqlDecimal.Field<revisedQty>
		{
		}
		protected Decimal? _RevisedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Quantity", Visible=false)]
		public virtual Decimal? RevisedQty
		{
			get
			{
				return this._RevisedQty;
			}
			set
			{
				this._RevisedQty = value;
			}
		}
		#endregion
		#region CuryRevisedAmount
		public abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.revisedAmount))]
		[PXFormula(typeof(Mult<revisedQty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.RevisedBudgetedAmount, Visible=false)]
		public virtual Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region RevisedAmount
		public abstract class revisedAmount : PX.Data.BQL.BqlDecimal.Field<revisedAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount in Base Currency", Visible=false)]
		public virtual Decimal? RevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryInvoicedAmount
		public abstract class curyInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyInvoicedAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.invoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.DraftInvoiceAmount, Enabled = false)]
		public virtual Decimal? CuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region InvoicedAmount
		public abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount in Base Currency", Enabled = false)]
		public virtual Decimal? InvoicedAmount
		{
			get;
			set;
		}
		#endregion


		#region ActualQty
		public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty>
		{
		}
		protected Decimal? _ActualQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false)]
		public virtual Decimal? ActualQty
		{
			get
			{
				return this._ActualQty;
			}
			set
			{
				this._ActualQty = value;
			}
		}
		#endregion
		#region CuryActualAmount
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.baseActualAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.ActualAmount, Enabled = false)]
		public virtual decimal? CuryActualAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualAmount
		public abstract class actualAmount : PX.Data.BQL.BqlDecimal.Field<actualAmount> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Hist. Actual Amount in Base Currency", Enabled = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual decimal? ActualAmount
		{
			get;
			set;
		}
		#endregion
		#region BaseActualAmount
		public abstract class baseActualAmount : PX.Data.BQL.BqlDecimal.Field<baseActualAmount> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Actual Amount in Base Currency", Enabled = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual decimal? BaseActualAmount
		{
			get;
			set;
		}
		#endregion
		#region DraftChangeOrderQty
		public abstract class draftChangeOrderQty : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderQty>
		{
		}

		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? DraftChangeOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryDraftChangeOrderAmount
		public abstract class curyDraftChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyDraftChangeOrderAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.draftChangeOrderAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryDraftChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region DraftChangeOrderAmount
		public abstract class draftChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? DraftChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderQty
		public abstract class changeOrderQty : PX.Data.BQL.BqlDecimal.Field<changeOrderQty>
		{
		}
		protected Decimal? _ChangeOrderQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? ChangeOrderQty
		{
			get
			{
				return this._ChangeOrderQty;
			}
			set
			{
				this._ChangeOrderQty = value;
			}
		}
		#endregion
		#region CuryChangeOrderAmount
		public abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.changeOrderAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderAmount
		public abstract class changeOrderAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? ChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedQty
		public abstract class committedQty : PX.Data.BQL.BqlDecimal.Field<committedQty>
		{
		}
		protected Decimal? _CommittedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Quantity", Enabled = false)]
		public virtual Decimal? CommittedQty
		{
			get
			{
				return this._CommittedQty;
			}
			set
			{
				this._CommittedQty = value;
			}
		}
		#endregion
		#region CuryCommittedAmount
		public abstract class curyCommittedAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Amount", Enabled = false)]
		public virtual Decimal? CuryCommittedAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedAmount
		public abstract class committedAmount : PX.Data.BQL.BqlDecimal.Field<committedAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Amount in Base Currency", Enabled = false)]
		public virtual Decimal? CommittedAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedCOQty
		public abstract class committedCOQty : PX.Data.BQL.BqlDecimal.Field<committedCOQty>
		{
		}
		[PXQuantity]

		[PXUIField(DisplayName = "Committed CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOQty
		{
			[PXDependsOnFields(typeof(committedQty), typeof(committedOrigQty))]
			get
			{
				return this.CommittedQty.GetValueOrDefault() - this.CommittedOrigQty.GetValueOrDefault();
			}
		}
		#endregion
		#region CuryCommittedCOAmount
		public abstract class curyCommittedCOAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedCOAmount>
		{
		}
		[PXCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedCOAmount))]
		[PXUIField(DisplayName = "Committed CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryCommittedCOAmount
		{
			[PXDependsOnFields(typeof(curyCommittedAmount), typeof(curyCommittedOrigAmount))]
			get
			{
				return this.CuryCommittedAmount.GetValueOrDefault() - this.CuryCommittedOrigAmount.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedCOAmount
		public abstract class committedCOAmount : PX.Data.BQL.BqlDecimal.Field<committedCOAmount>
		{
		}
		[PXBaseCury]
		[PXUIField(DisplayName = "Committed CO Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOAmount
		{
			[PXDependsOnFields(typeof(committedAmount), typeof(committedOrigAmount))]
			get
			{
				return this.CommittedAmount.GetValueOrDefault() - this.CommittedOrigAmount.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedOrigQty
		public abstract class committedOrigQty : PX.Data.BQL.BqlDecimal.Field<committedOrigQty>
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Committed Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedOrigQty
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedOrigAmount
		public abstract class curyCommittedOrigAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOrigAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedOrigAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Committed Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryCommittedOrigAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedOrigAmount
		public abstract class committedOrigAmount : PX.Data.BQL.BqlDecimal.Field<committedOrigAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Committed Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedOrigAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedOpenQty
		public abstract class committedOpenQty : PX.Data.BQL.BqlDecimal.Field<committedOpenQty>
		{
		}
		protected Decimal? _CommittedOpenQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Quantity", Enabled = false)]
		public virtual Decimal? CommittedOpenQty
		{
			get
			{
				return this._CommittedOpenQty;
			}
			set
			{
				this._CommittedOpenQty = value;
			}
		}
		#endregion
		#region CuryCommittedOpenAmount
		public abstract class curyCommittedOpenAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOpenAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedOpenAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Amount", Enabled = false)]
		public virtual Decimal? CuryCommittedOpenAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedOpenAmount
		public abstract class committedOpenAmount : PX.Data.BQL.BqlDecimal.Field<committedOpenAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Amount in Base Currency", Enabled = false)]
		public virtual Decimal? CommittedOpenAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedReceivedQty
		public abstract class committedReceivedQty : PX.Data.BQL.BqlDecimal.Field<committedReceivedQty>
		{
		}
		protected Decimal? _CommittedReceivedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Received Quantity", Enabled = false)]
		public virtual Decimal? CommittedReceivedQty
		{
			get
			{
				return this._CommittedReceivedQty;
			}
			set
			{
				this._CommittedReceivedQty = value;
			}
		}
		#endregion
		#region CommittedInvoicedQty
		public abstract class committedInvoicedQty : PX.Data.BQL.BqlDecimal.Field<committedInvoicedQty>
		{
		}
		protected Decimal? _CommittedInvoicedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Quantity", Enabled = false)]
		public virtual Decimal? CommittedInvoicedQty
		{
			get
			{
				return this._CommittedInvoicedQty;
			}
			set
			{
				this._CommittedInvoicedQty = value;
			}
		}
		#endregion
		#region CuryCommittedInvoicedAmount
		public abstract class curyCommittedInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedInvoicedAmount>
		{
		}
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedInvoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Amount", Enabled = false)]
		public virtual Decimal? CuryCommittedInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedInvoicedAmount
		public abstract class committedInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<committedInvoicedAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Amount in Base Currency", Enabled = false)]
		public virtual Decimal? CommittedInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryActualPlusOpenCommittedAmount
		public abstract class curyActualPlusOpenCommittedAmount : PX.Data.BQL.BqlDecimal.Field<curyActualPlusOpenCommittedAmount>
		{
		}

		[PXCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.actualPlusOpenCommittedAmount))]
		[PXUIField(DisplayName = "Actual + Open Committed Amount", Enabled = false)]
		public virtual Decimal? CuryActualPlusOpenCommittedAmount
		{
			[PXDependsOnFields(typeof(curyActualAmount), typeof(curyCommittedOpenAmount))]
			get
			{
				return this.CuryActualAmount + this.CuryCommittedOpenAmount;
			}
		}
		#endregion
		#region ActualPlusOpenCommittedAmount
		public abstract class actualPlusOpenCommittedAmount : PX.Data.BQL.BqlDecimal.Field<actualPlusOpenCommittedAmount>
		{
		}

		[PXBaseCury]
		[PXUIField(DisplayName = "Actual + Open Committed Amount in Base Currency", Enabled = false)]
		public virtual Decimal? ActualPlusOpenCommittedAmount
		{
			[PXDependsOnFields(typeof(actualAmount), typeof(committedOpenAmount))]
			get
			{
				return this.ActualAmount + this.CommittedOpenAmount;
			}
		}
		#endregion
		#region CuryVarianceAmount
		public abstract class curyVarianceAmount : PX.Data.BQL.BqlDecimal.Field<curyVarianceAmount>
		{
		}

		[PXCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.varianceAmount))]
		[PXUIField(DisplayName = "Variance Amount", Enabled = false)]
		public virtual Decimal? CuryVarianceAmount
		{
			[PXDependsOnFields(typeof(curyRevisedAmount), typeof(curyActualPlusOpenCommittedAmount))]
			get
			{
				return this.CuryRevisedAmount - this.CuryActualPlusOpenCommittedAmount;
			}
		}
		#endregion
		#region VarianceAmount
		public abstract class varianceAmount : PX.Data.BQL.BqlDecimal.Field<varianceAmount>
		{
		}

		[PXBaseCury]
		[PXUIField(DisplayName = "Variance Amount in Base Currency", Enabled = false)]
		public virtual Decimal? VarianceAmount
		{
			[PXDependsOnFields(typeof(revisedAmount), typeof(actualPlusOpenCommittedAmount))]
			get
			{
				return this.RevisedAmount - this.ActualPlusOpenCommittedAmount;
			}
		}
		#endregion
	}
}
