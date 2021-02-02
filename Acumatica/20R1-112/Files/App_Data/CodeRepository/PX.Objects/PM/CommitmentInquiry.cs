using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using PX.Objects.CS;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	[Serializable]
	public class CommitmentInquiry : PXGraph<CommitmentInquiry>
	{

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMCostCode_IsProjectOverride_CacheAttached(PXCache sender)
		{
		}

		public PXFilter<ProjectBalanceFilter> Filter;

		[PXFilterable]
		[PXViewName(Messages.Commitments)]
		public PXSelect<PMCommitment,
			Where2<Where<PMCommitment.projectID, Equal<Current<ProjectBalanceFilter.projectID>>, Or<Current<ProjectBalanceFilter.projectID>, IsNull>>,
			And2<Where<PMCommitment.accountGroupID, Equal<Current<ProjectBalanceFilter.accountGroupID>>, Or<Current<ProjectBalanceFilter.accountGroupID>, IsNull>>,
			And2<Where<PMCommitment.projectTaskID, Equal<Current<ProjectBalanceFilter.projectTaskID>>, Or<Current<ProjectBalanceFilter.projectTaskID>, IsNull>>,
			And2<Where<PMCommitment.costCodeID, Equal<Current<ProjectBalanceFilter.costCode>>, Or<Current<ProjectBalanceFilter.costCode>, IsNull>>,
			And<Where<PMCommitment.inventoryID, Equal<Current<ProjectBalanceFilter.inventoryID>>, Or<Current<ProjectBalanceFilter.inventoryID>, IsNull>>>>>>>> Items;

		public PXSelectGroupBy<PMCommitment,
			Where2<Where<PMCommitment.projectID, Equal<Current<ProjectBalanceFilter.projectID>>, Or<Current<ProjectBalanceFilter.projectID>, IsNull>>,
			And2<Where<PMCommitment.accountGroupID, Equal<Current<ProjectBalanceFilter.accountGroupID>>, Or<Current<ProjectBalanceFilter.accountGroupID>, IsNull>>,
			And2<Where<PMCommitment.projectTaskID, Equal<Current<ProjectBalanceFilter.projectTaskID>>, Or<Current<ProjectBalanceFilter.projectTaskID>, IsNull>>,
			And2<Where<PMCommitment.costCodeID, Equal<Current<ProjectBalanceFilter.costCode>>, Or<Current<ProjectBalanceFilter.costCode>, IsNull>>,
			And<Where<PMCommitment.inventoryID, Equal<Current<ProjectBalanceFilter.inventoryID>>, Or<Current<ProjectBalanceFilter.inventoryID>, IsNull>>>>>>>,
			Aggregate<Sum<PMCommitment.qty, Sum<PMCommitment.amount, Sum<PMCommitment.openQty, Sum<PMCommitment.openAmount, Sum<PMCommitment.receivedQty, Sum<PMCommitment.invoicedQty, Sum<PMCommitment.invoicedAmount>>>>>>>>> Totals;

		public PXCancel<ProjectBalanceFilter> Cancel;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMCostCode> dummyCostCode;

		public PXAction<ProjectBalanceFilter> createCommitment;
		[PXUIField(DisplayName = Messages.CreateCommitment)]
		[PXButton(Tooltip = Messages.CreateCommitment)]
		public virtual IEnumerable CreateCommitment(PXAdapter adapter)
		{
			ExternalCommitmentEntry graph = PXGraph.CreateInstance<ExternalCommitmentEntry>();
			throw new PXPopupRedirectException(graph, Messages.CommitmentEntry + " - " + Messages.CreateCommitment, true);
		}

		public PXAction<ProjectBalanceFilter> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProject(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Items.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<ProjectBalanceFilter> viewExternalCommitment;
		[PXUIField(DisplayName = Messages.ViewExternalCommitment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewExternalCommitment(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var graph = CreateInstance<ExternalCommitmentEntry>();
				graph.Commitments.Current = Items.Current;
				throw new PXRedirectRequiredException(graph, true, Messages.ViewExternalCommitment) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		
		#region Local Types

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class ProjectBalanceFilter : IBqlTable
		{
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			protected Int32? _ProjectID;
			[Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>))]
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

			#region AccountGroupID
			public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
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
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(ProjectBalanceFilter.projectID))]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[PXDBInt]
			[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
			[PMInventorySelector]
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
			#region CostCode
			public abstract class costCode : PX.Data.BQL.BqlInt.Field<costCode> { }
			[CostCode(Filterable = false, SkipVerification = true)]
			public virtual Int32? CostCode
			{
				get;
				set;
			}
			#endregion

			#region Qty
			public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
			protected Decimal? _Qty;
			[PXDecimal]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Revised Quantity")]
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
			#region Amount
			public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
			protected Decimal? _Amount;
			[PXBaseCury]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Revised Amount")]
			public virtual Decimal? Amount
			{
				get
				{
					return this._Amount;
				}
				set
				{
					this._Amount = value;
				}
			}
			#endregion
			#region OpenQty
			public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
			protected Decimal? _OpenQty;
			[PXDecimal]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Open Quantity")]
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
			#region OpenAmount
			public abstract class openAmount : PX.Data.BQL.BqlDecimal.Field<openAmount> { }
			protected Decimal? _OpenAmount;
			[PXBaseCury]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Open Amount")]
			public virtual Decimal? OpenAmount
			{
				get
				{
					return this._OpenAmount;
				}
				set
				{
					this._OpenAmount = value;
				}
			}
			#endregion
			#region ReceivedQty
			public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
			protected Decimal? _ReceivedQty;
			[PXDBQuantity]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Received Quantity")]
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
			#region InvoicedQty
			public abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }
			protected Decimal? _InvoicedQty;
			[PXDBQuantity]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Invoiced Quantity")]
			public virtual Decimal? InvoicedQty
			{
				get
				{
					return this._InvoicedQty;
				}
				set
				{
					this._InvoicedQty = value;
				}
			}
			#endregion
			#region InvoicedAmount
			public abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount> { }
			protected Decimal? _InvoicedAmount;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Invoiced Amount")]
			public virtual Decimal? InvoicedAmount
			{
				get
				{
					return this._InvoicedAmount;
				}
				set
				{
					this._InvoicedAmount = value;
				}
			}
			#endregion
		}

		#endregion
	}
}
