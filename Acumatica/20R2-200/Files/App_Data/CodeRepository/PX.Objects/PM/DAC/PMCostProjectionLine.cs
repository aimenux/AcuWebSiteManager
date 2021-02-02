using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CN.ProjectAccounting;
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
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.CostProjection)]
	[PXPrimaryGraph(typeof(CostProjectionEntry))]
	[Serializable]
	public class PMCostProjectionLine : PX.Data.IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDefault(typeof(PMCostProjection.projectID))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID>
		{
		}
		[PXDBString(30, IsKey = true)]
		[PXDefault(typeof(PMCostProjection.revisionID))]
		[PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string RevisionID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXParent(typeof(Select<PMCostProjection, Where<PMCostProjection.projectID, Equal<Current<projectID>>, And<PMCostProjection.revisionID, Equal<Current<revisionID>>>>>))]
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMCostProjection.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion
		
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[ProjectTask(typeof(projectID),	AlwaysEnabled = true, DisplayName = "Cost Task", AllowNull = true)]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[AccountGroup(typeof(Where<PMAccountGroup.isExpense, Equal<True>>))]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public virtual Int32? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(null, typeof(taskID), PX.Objects.GL.AccountType.Expense, typeof(accountGroupID), SkipVerification = true, AllowNullValue = true)]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt()]
		[PXUIField(DisplayName = "Inventory ID")]
		[PMInventorySelector]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}
		[PXDBString(Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		[PXDBString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "UOM", Enabled = false)]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion
		#region BudgetedQuantity
		public abstract class budgetedQuantity : PX.Data.BQL.BqlDecimal.Field<budgetedQuantity> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalBudgetedQuantity>))]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted Quantity", Enabled = false)]
		public virtual Decimal? BudgetedQuantity
		{
			get;
			set;
		}
		#endregion
		#region BudgetedAmount
		public abstract class budgetedAmount : PX.Data.BQL.BqlDecimal.Field<budgetedAmount> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalBudgetedAmount>))]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted Cost", Enabled = false)]
		public virtual Decimal? BudgetedAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualQuantity
		public abstract class actualQuantity : PX.Data.BQL.BqlDecimal.Field<actualQuantity> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalActualQuantity>))]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false, Visible = false)]
		public virtual Decimal? ActualQuantity
		{
			get;
			set;
		}
		#endregion
		#region ActualAmount
		public abstract class actualAmount : PX.Data.BQL.BqlDecimal.Field<actualAmount> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalActualAmount>))]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Cost", Enabled = false, Visible = false)]
		public virtual Decimal? ActualAmount
		{
			get;
			set;
		}
		#endregion
		#region UnbilledQuantity
		public abstract class unbilledQuantity : PX.Data.BQL.BqlDecimal.Field<unbilledQuantity> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalUnbilledQuantity>))]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Quantity", Enabled = false, Visible = false)]
		public virtual Decimal? UnbilledQuantity
		{
			get;
			set;
		}
		#endregion
		#region UnbilledAmount
		public abstract class unbilledAmount : PX.Data.BQL.BqlDecimal.Field<unbilledAmount> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalUnbilledAmount>))]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Cost", Enabled = false, Visible = false)]
		public virtual Decimal? UnbilledAmount
		{
			get;
			set;
		}
		#endregion
		
		#region CompletedQuantity
		public abstract class completedQuantity : PX.Data.BQL.BqlDecimal.Field<completedQuantity> { }
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual + Committed Open Quantity", Enabled = false)]
		public virtual Decimal? CompletedQuantity
		{
			[PXDependsOnFields(typeof(actualQuantity), typeof(unbilledQuantity))]
			get { return ActualQuantity.GetValueOrDefault() + UnbilledQuantity.GetValueOrDefault(); }

		}
		#endregion
		#region CompletedAmount
		public abstract class completedAmount : PX.Data.BQL.BqlDecimal.Field<completedAmount> { }
		[PXBaseCury()]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual + Committed Open Cost", Enabled = false)]
		public virtual Decimal? CompletedAmount
		{
			[PXDependsOnFields(typeof(actualAmount), typeof(unbilledAmount))]
			get { return ActualAmount.GetValueOrDefault() + UnbilledAmount.GetValueOrDefault(); }
		}
		#endregion
		#region QuantityToComplete
		public abstract class quantityToComplete : PX.Data.BQL.BqlDecimal.Field<quantityToComplete> { }
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Quantity To Complete", Enabled = false)]
		public virtual Decimal? QuantityToComplete
		{
			[PXDependsOnFields(typeof(budgetedQuantity), typeof(completedQuantity))]
			get { return Math.Max(0, BudgetedQuantity.GetValueOrDefault() - CompletedQuantity.GetValueOrDefault()); }
		}
		#endregion
		#region AmountToComplete
		public abstract class amountToComplete : PX.Data.BQL.BqlDecimal.Field<amountToComplete> { }

		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cost To Complete", Enabled = false)]
		public virtual Decimal? AmountToComplete
		{
			[PXDependsOnFields(typeof(budgetedAmount), typeof(completedAmount))]
			get { return Math.Max(0, BudgetedAmount.GetValueOrDefault() - CompletedAmount.GetValueOrDefault()); }
		}
		#endregion

		#region Mode
		public abstract class mode : PX.Data.BQL.BqlString.Field<mode> { }
		[PXDBString(1, IsFixed = true)]
		[ProjectionMode.List()]
		[PXDefault(ProjectionMode.Auto)]
		[PXUIField(DisplayName = "Mode")]
		public virtual String Mode
		{
			get;
			set;
		}
		#endregion
		#region Quantity
		public abstract class quantity : PX.Data.BQL.BqlDecimal.Field<quantity> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalQuantity>))]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Quantity to Complete")]
		public virtual Decimal? Quantity
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalAmount>))]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost to Complete")]
		public virtual Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region ProjectedQuantity
		public abstract class projectedQuantity : PX.Data.BQL.BqlDecimal.Field<projectedQuantity> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalProjectedQuantity>))]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Quantity at Completion")]
		public virtual Decimal? ProjectedQuantity
		{
			get;
			set;
		}
		#endregion
		#region ProjectedAmount
		public abstract class projectedAmount : PX.Data.BQL.BqlDecimal.Field<projectedAmount> { }
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalProjectedAmount>))]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost at Completion")]
		public virtual Decimal? ProjectedAmount
		{
			get;
			set;
		}
		#endregion
		#region CompletedPct
		public abstract class completedPct : PX.Data.BQL.BqlDecimal.Field<completedPct> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Completed (%)")]
		public virtual Decimal? CompletedPct
		{
			get;
			set;
		}
		#endregion
				
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get; set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get; set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get; set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get; set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get; set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get; set;
		}
		#endregion
		#endregion
	}
}
