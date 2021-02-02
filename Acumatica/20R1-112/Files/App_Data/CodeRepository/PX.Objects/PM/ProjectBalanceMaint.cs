using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.AR;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	[Serializable]
	public class ProjectBalanceMaint : PXGraph<ProjectBalanceMaint>, PXImportAttribute.IPXPrepareItems
	{

		#region DAC Attributes Override

		[PXDefault]
		[Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>), IsKey=true)]
		protected virtual void PMBudget_ProjectID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<PMBudget.projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[ProjectTask(typeof(PMBudget.projectID), IsKey = true, AlwaysEnabled =true)]
		protected virtual void PMBudget_ProjectTaskID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault]
		[AccountGroup(IsKey = true)]
		protected virtual void PMBudget_AccountGroupID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXBool]
		[PXDefault(false)]
		protected virtual void PMCostCode_IsProjectOverride_CacheAttached(PXCache sender)
		{
		}

		#endregion

		public ProjectBalanceMaint()
		{
			SetDefaultColumnVisibility();
		}

		[PXImport(typeof(PMBudget))]
		[PXViewName(Messages.Budget)]
		[PXFilterable]
		public PXSelectJoin<PMBudget, 
			InnerJoin<PMTask, On<PMBudget.projectID, Equal<PMTask.projectID>, And<PMBudget.projectTaskID, Equal<PMTask.taskID>>>,
			InnerJoin<PMProject, On<PMBudget.projectID, Equal<PMProject.contractID>>,
			InnerJoin<PMAccountGroup, On<PMBudget.accountGroupID, Equal<PMAccountGroup.groupID>>>>>,
			Where<PMProject.nonProject, Equal<False>,
			And<PMProject.baseType, Equal<CT.CTPRType.project>,
			And<Match<Current<AccessInfo.userName>>>>>,
			OrderBy<Asc<PMProject.contractCD, Asc<PMTask.taskCD, Asc<PMAccountGroup.groupCD>>>>> Items;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMCostCode> dummyCostCode;

		public PXSavePerRow<PMBudget> Save;
		public PXCancel<PMBudget> Cancel;
		public PXSetup<PMSetup> Setup;



		public PXAction<PMBudget> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewProject(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Items.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<PMBudget> viewTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewTask(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				ProjectTaskEntry graph = PXGraph.CreateInstance<ProjectTaskEntry>();
				graph.Task.Current = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, Items.Current.ProjectID, Items.Current.ProjectTaskID);

				throw new PXPopupRedirectException(graph, Messages.ProjectTaskEntry + " - " + Messages.ViewTask, true);
			}
			return adapter.Get();
		}

		public PXAction<PMBudget> viewTransactions;
		[PXUIField(DisplayName = Messages.ViewTransactions, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewTransactions(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				TransactionInquiry graph = PXGraph.CreateInstance<TransactionInquiry>();
				graph.Filter.Current.ProjectID = Items.Current.ProjectID;
				graph.Filter.Current.ProjectTaskID = Items.Current.ProjectTaskID;
				graph.Filter.Current.AccountGroupID = Items.Current.AccountGroupID;
				graph.Filter.Current.InventoryID = Items.Current.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : Items.Current.InventoryID;

				throw new PXPopupRedirectException(graph, Messages.ViewTransactions, true);
			}
			return adapter.Get();
		}

		public PXAction<PMBudget> viewCommitments;
		[PXUIField(DisplayName = Messages.ViewCommitments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewCommitments(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				CommitmentInquiry graph = PXGraph.CreateInstance<CommitmentInquiry>();
				graph.Filter.Current.AccountGroupID = Items.Current.AccountGroupID;
				graph.Filter.Current.ProjectID = Items.Current.ProjectID;
				graph.Filter.Current.ProjectTaskID = Items.Current.ProjectTaskID;
				graph.Filter.Current.InventoryID = Items.Current.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : Items.Current.InventoryID;


				throw new PXPopupRedirectException(graph, Messages.CommitmentEntry + " - " + Messages.ViewCommitments, true);
			}
			return adapter.Get();
		}


		protected virtual void _(Events.RowSelected<PMBudget> e)
		{
			if (e.Row != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, e.Row.ProjectID);

				PXUIFieldAttribute.SetEnabled<PMCostBudget.curyUnitRate>(e.Cache, null, project?.BudgetFinalized != true);
				PXUIFieldAttribute.SetEnabled<PMCostBudget.qty>(e.Cache, null, project?.BudgetFinalized != true);
				PXUIFieldAttribute.SetEnabled<PMCostBudget.curyAmount>(e.Cache, null, project?.BudgetFinalized != true);

				PXUIFieldAttribute.SetEnabled<PMCostBudget.revisedQty>(e.Cache, null, project?.ChangeOrderWorkflow != true);
				PXUIFieldAttribute.SetEnabled<PMCostBudget.curyRevisedAmount> (e.Cache, null, project?.ChangeOrderWorkflow != true);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMCostBudget.curyAmount> e)
		{
			if (e.Row != null)
			{
				PMBudget row = (PMBudget)e.Row;
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID);

				if (project?.BudgetFinalized == true)
				{
					row.CuryAmount = e.OldValue as decimal?;
				}
			}
		}

		protected virtual void PMBudget_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMBudget row = e.Row as PMBudget;
			if (row == null || string.IsNullOrEmpty(row.UOM)) return;

			var select = new PXSelect<PMTran, Where<PMTran.projectID, Equal<Current<PMBudget.projectID>>,
				And<PMTran.taskID, Equal<Current<PMBudget.projectTaskID>>,
				And<PMTran.costCodeID, Equal<Current<PMBudget.costCodeID>>,
				And<PMTran.inventoryID, Equal<Current<PMBudget.inventoryID>>,
				And2<Where<PMTran.accountGroupID, Equal<Current<PMBudget.accountGroupID>>, Or<PMTran.offsetAccountGroupID, Equal<Current<PMBudget.accountGroupID>>>>,
				And<PMTran.released, Equal<True>,
				And<PMTran.uOM, NotEqual<Required<PMTran.uOM>>>>>>>>>>(this);

			string uom = (string) e.NewValue;
			if (!string.IsNullOrEmpty(uom))
			{
				PMTran tranInOtherUOM = select.SelectWindowed(0, 1, uom);

				if (tranInOtherUOM != null)
				{
					var ex = new PXSetPropertyException(Messages.OtherUomUsedInTransaction);
					ex.ErrorValue = uom;
					throw ex;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.accountGroupID> e)
		{
			e.Cache.SetDefaultExt<PMBudget.type>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.curyAmount> e)
		{
			if (e.Row != null)
			{
				e.Row.CuryRevisedAmount = e.Row.CuryAmount;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.qty> e)
		{
			if (e.Row != null)
			{
				e.Row.RevisedQty = e.Row.Qty;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.type> e)
		{
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, e.Row.AccountGroupID);
			if (ag != null)
			{
				if (ag.Type == PMAccountType.OffBalance)
					e.NewValue = ag.IsExpense == true ? GL.AccountType.Expense : ag.Type;
				else
					e.NewValue = ag.Type;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.costCodeID> e)
		{
			if (e.Row == null) return;

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, e.Row.ProjectID);
			if (project != null)
			{
				if (project.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.inventoryID> e)
		{
			if (e.Row == null) return;

			e.NewValue = PM.PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.description> e)
		{
			if (e.Row == null) return;

			if (CostCodeAttribute.UseCostCode())
			{
				if (e.Row.CostCodeID != null && e.Row.CostCodeID != CostCodeAttribute.GetDefaultCostCode())
				{
					PMCostCode costCode = PXSelectorAttribute.Select<PMBudget.costCodeID>(e.Cache, e.Row) as PMCostCode;
					if (costCode != null)
					{
						e.NewValue = costCode.Description;
					}
				}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.inventoryID> e)
		{
			if (!CostCodeAttribute.UseCostCode())
			{
				//Current record may be in process of importing from excel. In this case all we have is pending values for description, Uom, Rate
				string pendingDescription = null;
				string pendingUom = null;

				object pendingDescriptionObj = Items.Cache.GetValuePending<PMBudget.description>(e.Row);
				object pendingUomObj = Items.Cache.GetValuePending<PMBudget.uOM>(e.Row);

				if (pendingDescriptionObj != null && pendingDescriptionObj != PXCache.NotSetValue)
					pendingDescription = (string)pendingDescriptionObj;

				if (pendingUomObj != null && pendingUomObj != PXCache.NotSetValue)
					pendingUom = (string)pendingUomObj;

				object pendingRate = Items.Cache.GetValuePending<PMBudget.curyUnitRate>(e.Row);

				if (string.IsNullOrEmpty(pendingDescription))
					e.Cache.SetDefaultExt<PMBudget.description>(e.Row);

				if (string.IsNullOrEmpty(pendingUom))
					e.Cache.SetDefaultExt<PMBudget.uOM>(e.Row);

				if (pendingRate == null)
					e.Cache.SetDefaultExt<PMBudget.curyUnitRate>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMCostBudget.costCodeID> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				//Current record may be in process of importing from excel. In this case all we have is pending values for description, Uom, Rate
				string pendingDescription = null;
				string pendingUom = null;

				object pendingDescriptionObj = Items.Cache.GetValuePending<PMBudget.description>(e.Row);
				object pendingUomObj = Items.Cache.GetValuePending<PMBudget.uOM>(e.Row);

				if (pendingDescriptionObj != null && pendingDescriptionObj != PXCache.NotSetValue)
					pendingDescription = (string)pendingDescriptionObj;

				if (pendingUomObj != null && pendingUomObj != PXCache.NotSetValue)
					pendingUom = (string)pendingUomObj;

				object pendingRate = Items.Cache.GetValuePending<PMBudget.curyUnitRate>(e.Row);

				if (string.IsNullOrEmpty(pendingDescription))
					e.Cache.SetDefaultExt<PMBudget.description>(e.Row);

				if (string.IsNullOrEmpty(pendingUom))
					e.Cache.SetDefaultExt<PMBudget.uOM>(e.Row);

				if (pendingRate == null)
					e.Cache.SetDefaultExt<PMBudget.curyUnitRate>(e.Row);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.curyUnitRate> e)
		{
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, e.Row.AccountGroupID);
			if (ag != null)
			{
				if (ag.IsExpense == true)
				{
					if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
					{
						InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMCostBudget.inventoryID>(e.Cache, e.Row);
						e.NewValue = item?.StdCost;
					}
				}
				else
				{
					if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
					{
						string customerPriceClass = ARPriceClass.EmptyPriceClass;

						PMTask projectTask = (PMTask)PXSelectorAttribute.Select<PMRevenueBudget.projectTaskID>(e.Cache, e.Row);
						CR.Location c = (CR.Location)PXSelectorAttribute.Select<PMTask.locationID>(e.Cache, projectTask);
						if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
							customerPriceClass = c.CPriceClassID;

						CM.CurrencyInfo dummy = new CM.CurrencyInfo();
						dummy.CuryID = Accessinfo.BaseCuryID;
						dummy.BaseCuryID = Accessinfo.BaseCuryID;
						dummy.CuryRate = 1;

						e.NewValue = ARSalesPriceMaint.CalculateSalesPrice(Caches[typeof(PMTran)], customerPriceClass, projectTask.CustomerID, e.Row.InventoryID, dummy, e.Row.Qty, e.Row.UOM, Accessinfo.BusinessDate.Value, true);
					}
				}
			}

		}

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (!CostCodeAttribute.UseCostCode())
			{
				PMCostCode defaultCostCode = PXSelect<PMCostCode, Where<PMCostCode.isDefault, Equal<True>>>.Select(this);
				if (defaultCostCode != null)
				{
					keys[nameof(PMBudget.CostCodeID)] = defaultCostCode.CostCodeCD;
					values[nameof(PMBudget.CostCodeID)] = defaultCostCode.CostCodeCD;
				}
			}

			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { } 
		#endregion

		public virtual void SetDefaultColumnVisibility()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.costCodes>())
			{
				PXUIFieldAttribute.SetVisible<PMBudget.inventoryID>(Items.Cache, null, false);
			}

			bool commitmentTracking = Setup.Current.CostCommitmentTracking == true || Setup.Current.RevenueCommitmentTracking == true;
			viewCommitments.SetVisible(commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedAmount>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedQty>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedInvoicedAmount>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedInvoicedQty>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedOpenAmount>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedOpenQty>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedReceivedQty>(Items.Cache, null, commitmentTracking);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}
	}
}
