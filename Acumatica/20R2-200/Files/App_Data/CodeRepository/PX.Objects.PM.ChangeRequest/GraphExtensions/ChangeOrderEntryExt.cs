using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using System.Collections;
using System.Diagnostics;

namespace PX.Objects.PM.ChangeRequest.GraphExtensions
{
	public class ChangeOrderEntryExt : PXGraphExtension<PX.Objects.PM.ChangeOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.changeRequest>();
		}

		//Removed PXParent to block cascade delete
		[PXDBString(PMChangeRequest.refNbr.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		protected virtual void PMChangeRequestLine_RefNbr_CacheAttached(PXCache sender) { }

		//Removed PXParent to block cascade delete
		[PXDBString(PMChangeRequest.refNbr.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		protected virtual void PMChangeRequestMarkup_RefNbr_CacheAttached(PXCache sender) { }

		public PXSelect<PMChangeRequest, Where<PMChangeRequest.changeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>,
			Or<PMChangeRequest.costChangeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>>>> ChangeRequests;

		public PXSelect<PMChangeRequest> AvailableChangeRequests;
		public IEnumerable availableChangeRequests()
		{
			PXSelectBase<PMChangeRequest> select;

			if (Base.Document.Current != null && !string.IsNullOrEmpty(Base.Document.Current.ClassID) && Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled != true)
			{
				select = new PXSelect<PMChangeRequest, Where<PMChangeRequest.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeRequest.approved, Equal<True>,
				And<PMChangeRequest.costChangeOrderNbr, IsNull>>>>(this.Base);
			}
			else
			{
				select = new PXSelect<PMChangeRequest, Where<PMChangeRequest.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeRequest.approved, Equal<True>,
				And<PMChangeRequest.changeOrderNbr, IsNull>>>>(this.Base);
			}

			return select.Select();
		}

		public PXSelectJoin<PMChangeRequestLine, InnerJoin<PMChangeRequest, On<PMChangeRequest.refNbr, Equal<PMChangeRequestLine.refNbr>>>> ChangeRequestCostDetails;
	
		public IEnumerable changeRequestCostDetails()
		{
			List<string> ids = new List<string>();
			foreach (PMChangeRequest changeRequest in ChangeRequests.Select())
			{
				ids.Add(changeRequest.RefNbr);
			}

			var select = new PXSelect<PMChangeRequestLine, Where<PMChangeRequestLine.refNbr, In<Required<PMChangeRequestLine.refNbr>>,
				And<PMChangeRequestLine.costTaskID, Equal<Current<PMChangeOrderCostBudget.projectTaskID>>,
				And<PMChangeRequestLine.costAccountGroupID, Equal<Current<PMChangeOrderCostBudget.accountGroupID>>,
				And2<Where<PMChangeRequestLine.costCodeID, Equal<Current<PMChangeOrderCostBudget.costCodeID>>,
					Or<Where<PMChangeRequestLine.costCodeID, IsNull, And<Current<PMChangeOrderCostBudget.costCodeID>, Equal<Required<PMChangeRequestLine.costCodeID>>>>>>,
				And<Where<PMChangeRequestLine.inventoryID, Equal<Current<PMChangeOrderCostBudget.inventoryID>>,
					Or<Where<Current<PMChangeOrderCostBudget.inventoryID>, Equal<Required<PMChangeRequestLine.inventoryID>>>>>>>>>>>(Base);

			return select.Select(ids.ToArray(), CostCodeAttribute.GetDefaultCostCode(), PMInventorySelectorAttribute.EmptyInventoryID);
		}

		public PXSelectJoin<PMChangeRequestLine, InnerJoin<PMChangeRequest, On<PMChangeRequest.refNbr, Equal<PMChangeRequestLine.refNbr>>>> ChangeRequestRevenueDetails;

		public IEnumerable changeRequestRevenueDetails()
		{
			List<string> ids = new List<string>();
			foreach(PMChangeRequest changeRequest in ChangeRequests.Select())
			{
				ids.Add(changeRequest.RefNbr);
			}

			var select = new PXSelect<PMChangeRequestLine, Where<PMChangeRequestLine.refNbr, In<Required<PMChangeRequestLine.refNbr>>,
				And<PMChangeRequestLine.revenueTaskID, Equal<Current<PMChangeOrderRevenueBudget.projectTaskID>>,
				And<PMChangeRequestLine.revenueAccountGroupID, Equal<Current<PMChangeOrderRevenueBudget.accountGroupID>>,
				And2<Where<PMChangeRequestLine.revenueCodeID, Equal<Current<PMChangeOrderRevenueBudget.costCodeID>>,
					Or<Where<PMChangeRequestLine.revenueCodeID, IsNull, And<Current<PMChangeOrderRevenueBudget.costCodeID>, Equal<Required<PMChangeRequestLine.revenueCodeID>>>>>>,
				And<Where<PMChangeRequestLine.revenueInventoryID, Equal<Current<PMChangeOrderRevenueBudget.inventoryID>>,
					Or<Where<Current<PMChangeOrderRevenueBudget.inventoryID>, Equal<Required<PMChangeRequestLine.revenueInventoryID>>>>>>>>>>>(Base);

			return select.Select(ids.ToArray(), CostCodeAttribute.GetDefaultCostCode(), PMInventorySelectorAttribute.EmptyInventoryID);
		}

		public PXSelectJoin<PMChangeRequestMarkup, InnerJoin<PMChangeRequest, On<PMChangeRequest.refNbr, Equal<PMChangeRequestMarkup.refNbr>>>> ChangeRequestMarkupDetails;

		public IEnumerable changeRequestMarkupDetails()
		{
			List<string> ids = new List<string>();
			foreach (PMChangeRequest changeRequest in ChangeRequests.Select())
			{
				ids.Add(changeRequest.RefNbr);
			}

			var select = new PXSelect<PMChangeRequestMarkup,
			Where<PMChangeRequestMarkup.refNbr, In<Required<PMChangeRequestMarkup.refNbr>>,
			And<PMChangeRequestMarkup.taskID, Equal<Current<PMChangeOrderRevenueBudget.projectTaskID>>,
			And<PMChangeRequestMarkup.accountGroupID, Equal<Current<PMChangeOrderRevenueBudget.accountGroupID>>,
			And2<Where<PMChangeRequestMarkup.costCodeID, Equal<Current<PMChangeOrderRevenueBudget.costCodeID>>,
				Or<Where<PMChangeRequestMarkup.costCodeID, IsNull, And<Current<PMChangeOrderRevenueBudget.costCodeID>, Equal<Required<PMChangeRequestMarkup.costCodeID>>>>>>,
			And<Where<PMChangeRequestMarkup.inventoryID, Equal<Current<PMChangeOrderRevenueBudget.inventoryID>>,
				Or<Where<PMChangeRequestMarkup.inventoryID, IsNull, And<Current<PMChangeOrderRevenueBudget.inventoryID>, Equal<Required<PMChangeRequestMarkup.inventoryID>>>>>>>>>>>>(Base);

			return select.Select(ids.ToArray(), CostCodeAttribute.GetDefaultCostCode(), PMInventorySelectorAttribute.EmptyInventoryID);
		}

		public PXAction<PMChangeOrder> addChangeRequests;
		[PXUIField(DisplayName = "Select Change Requests")]
		[PXButton(VisibleOnDataSource = false)]
		public IEnumerable AddChangeRequests(PXAdapter adapter)
		{
			if (AvailableChangeRequests.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedChangeRequests();
			}

			return adapter.Get();
		}

				
		public PXAction<PMChangeOrder> appendSelectedChangeRequests;
		[PXUIField(DisplayName = "Add Change Requests")]
		[PXButton(VisibleOnDataSource = false)]
		public IEnumerable AppendSelectedChangeRequests(PXAdapter adapter)
		{
			AddSelectedChangeRequests();

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewChangeRequest;
		[PXUIField(DisplayName = Messages.ViewChangeRequest, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnDataSource = false)]
		public IEnumerable ViewChangeRequest(PXAdapter adapter)
		{
			if (ChangeRequests.Current != null)
			{
				ChangeRequestEntry target = PXGraph.CreateInstance<ChangeRequestEntry>();
				target.Document.Current = ChangeRequests.Current;

				throw new PXRedirectRequiredException(target, true, Messages.ViewChangeRequest) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewChangeRequestCostDetails;
		[PXUIField(DisplayName = "View Change Request Details")]
		[PXButton(VisibleOnDataSource = false)]
		public IEnumerable ViewChangeRequestCostDetails(PXAdapter adapter)
		{
			ChangeRequestCostDetails.View.AskExt(true);

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewChangeRequestRevenueDetails;
		[PXUIField(DisplayName = "View Change Request Details")]
		[PXButton(VisibleOnDataSource = false)]
		public IEnumerable ViewChangeRequestRevenueDetails(PXAdapter adapter)
		{
			ChangeRequestRevenueDetails.View.AskExt(true);

			return adapter.Get();
		}

		public virtual void AddSelectedChangeRequests()
		{
			foreach (PMChangeRequest cr in AvailableChangeRequests.Cache.Updated)
			{
				if (cr.Selected != true) continue;

				AddChangeRequest(cr);
			}
		}

		internal virtual void AddChangeRequest(PMChangeRequest cr)
		{
			VerifyRequestCanBeAdded(cr.RefNbr);

			var select = new PXSelect<PMChangeRequestLine, Where<PMChangeRequestLine.refNbr, Equal<Required<PMChangeRequestLine.refNbr>>>>(Base);
			
			foreach(PMChangeRequestLine line in select.Select(cr.RefNbr))
			{
				if (string.IsNullOrEmpty(cr.CostChangeOrderNbr))
				{
					PMChangeOrderCostBudget cost = ExtractCostBudget(line);
					if (cost != null)
					{
						Base.AddToDraftBucket(cost, -1);
						AddToCostBudget(cost);
					}

					if (line.IsCommitment == true)
					{
						AddCommitmentLine(line);
					}
				}

				if (Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled == true)
				{
					PMChangeOrderRevenueBudget revenue = ExtractRevenueBudget(line);
					if (revenue != null)
					{
						Base.AddToDraftBucket(revenue, -1);
						AddToRevenueBudget(revenue);
					}
				}
							
			}

			PMChangeOrder doc = Base.Document.Current;
			doc.ChangeRequestCostTotal = doc.ChangeRequestCostTotal.GetValueOrDefault() + cr.CostTotal.GetValueOrDefault();
			doc.ChangeRequestLineTotal = doc.ChangeRequestLineTotal.GetValueOrDefault() + cr.LineTotal.GetValueOrDefault();
			doc.ChangeRequestMarkupTotal = doc.ChangeRequestMarkupTotal.GetValueOrDefault() + cr.MarkupTotal.GetValueOrDefault();
			doc.ChangeRequestPriceTotal = doc.ChangeRequestPriceTotal.GetValueOrDefault() + cr.PriceTotal.GetValueOrDefault();
			Base.Document.Current.DelayDays = Base.Document.Current.DelayDays.GetValueOrDefault() + cr.DelayDays.GetValueOrDefault();
			Base.Document.Update(Base.Document.Current);

			if (Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled == true)
			{
				var selectMarkup = new PXSelect<PMChangeRequestMarkup, Where<PMChangeRequestMarkup.refNbr, Equal<Required<PMChangeRequestMarkup.refNbr>>>>(Base);
				foreach (PMChangeRequestMarkup line in selectMarkup.Select(cr.RefNbr))
				{
					PMChangeOrderRevenueBudget revenue = ExtractRevenueBudget(line);
					if (revenue != null)
					{
						Base.AddToDraftBucket(revenue, -1);
						AddToRevenueBudget(revenue);
					}
				}
				cr.Status = ChangeRequestStatus.Closed;
				cr.Released = true;
				cr.ChangeOrderNbr = Base.Document.Current.RefNbr;
			}
			
			if (string.IsNullOrEmpty(cr.CostChangeOrderNbr) && Base.ChangeOrderClass.Current.IsCostBudgetEnabled == true)
			{
				cr.CostChangeOrderNbr = Base.Document.Current.RefNbr;
			}
			
			ChangeRequests.Update(cr);
		}

		protected virtual PMChangeOrderCostBudget AddToCostBudget(PMChangeOrderCostBudget cost)
		{
			return AddToCostBudget(cost, 1);
		}
		protected virtual PMChangeOrderCostBudget AddToCostBudget(PMChangeOrderCostBudget cost, int mult = 1)
		{
			PMChangeOrderCostBudget existing = GetCostBudget(BudgetKeyTuple.Create(cost));
			if (existing != null)
			{
				var rollupQty = Base.BalanceCalculator.CalculateRollupQty<PMChangeOrderBudget>(cost, existing);

				existing.Qty += mult * rollupQty;
				existing.ChangeRequestQty += mult * rollupQty;
				existing.Amount += mult * cost.Amount.GetValueOrDefault();
				existing.ChangeRequestAmount += mult * cost.Amount.GetValueOrDefault();
				existing.IsDisabled = true;

				return Base.CostBudget.Update(existing);
			}
			else
			{
				PMChangeOrderCostBudget added = Base.CostBudget.Insert(cost);
				if (added != null)
				{
					added.ChangeRequestQty += mult * cost.Qty.GetValueOrDefault();
					added.ChangeRequestAmount += mult * cost.Amount.GetValueOrDefault();
					added.IsDisabled = true;

					return Base.CostBudget.Update(added);
				}
			}

			return null;
		}

		protected virtual PMChangeOrderCostBudget GetCostBudget(BudgetKeyTuple key)
		{
			var select = new PXSelect<PMChangeOrderCostBudget,
				Where<PMChangeOrderCostBudget.refNbr, Equal<Current<PMChangeOrderCostBudget.refNbr>>,
				And<PMChangeOrderCostBudget.projectID, Equal<Required<PMChangeOrderCostBudget.projectID>>,
				And<PMChangeOrderCostBudget.projectTaskID, Equal<Required<PMChangeOrderCostBudget.projectTaskID>>,
				And<PMChangeOrderCostBudget.accountGroupID, Equal<Required<PMChangeOrderCostBudget.accountGroupID>>,
				And<PMChangeOrderCostBudget.costCodeID, Equal<Required<PMChangeOrderCostBudget.costCodeID>>,
				And<PMChangeOrderCostBudget.inventoryID, Equal<Required<PMChangeOrderCostBudget.inventoryID>>>>>>>>>(Base);

			return select.SelectSingle(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, key.CostCodeID, key.InventoryID);
		}

		protected virtual PMChangeOrderCostBudget ExtractCostBudget(PMChangeRequestLine line)
		{
			if (line.CostTaskID == null || line.CostAccountGroupID == null)
				return null;

			PMAccountGroup accountGroup = PMAccountGroup.PK.Find(Base, line.CostAccountGroupID);
			if (accountGroup == null)
				return null;
			
			PMBudget filter = new PMBudget();
			filter.ProjectID = line.ProjectID;
			filter.ProjectTaskID = line.CostTaskID;
			filter.AccountGroupID = line.CostAccountGroupID;
			filter.CostCodeID = line.CostCodeID;
			filter.InventoryID = line.InventoryID;

			bool isExisting;
			BudgetService budgetService = new BudgetService(Base);
			Lite.PMBudget target = budgetService.SelectProjectBalance(filter, accountGroup, Base.Project.Current, out isExisting);

			PMChangeOrderCostBudget budget = new PMChangeOrderCostBudget();
			budget.RefNbr = Base.Document.Current.RefNbr;
			budget.ProjectID = target.ProjectID;
			budget.ProjectTaskID = target.TaskID;
			budget.AccountGroupID = target.AccountGroupID;
			budget.CostCodeID = target.CostCodeID;
			budget.InventoryID = target.InventoryID;
			budget.UOM = line.UOM;
			budget.Qty = line.Qty;
			budget.Amount = line.ExtCost;

			return budget;
		}

		protected virtual PMChangeOrderRevenueBudget AddToRevenueBudget(PMChangeOrderRevenueBudget revenue)
		{
			return AddToRevenueBudget(revenue, 1);
		}
		protected virtual PMChangeOrderRevenueBudget AddToRevenueBudget(PMChangeOrderRevenueBudget revenue, int mult = 1)
		{
			PMChangeOrderRevenueBudget existing = GetRevenueBudget(BudgetKeyTuple.Create(revenue));
			if (existing != null)
			{
				var rollupQty = Base.BalanceCalculator.CalculateRollupQty<PMChangeOrderBudget>(revenue, existing);

				existing.Qty += mult * rollupQty;
				existing.ChangeRequestQty += mult * rollupQty;
				existing.Amount += mult * revenue.Amount.GetValueOrDefault();
				existing.ChangeRequestAmount += mult * revenue.Amount.GetValueOrDefault();
				existing.IsDisabled = true;

				return Base.RevenueBudget.Update(existing);
			}
			else
			{
				PMChangeOrderRevenueBudget added = Base.RevenueBudget.Insert(revenue);
				if (added != null)
				{
					added.ChangeRequestQty += mult * revenue.Qty.GetValueOrDefault();
					added.ChangeRequestAmount += mult * revenue.Amount.GetValueOrDefault();
					added.IsDisabled = true;

					return Base.RevenueBudget.Update(added);
				}
				
			}

			return null;
		}

		protected virtual PMChangeOrderRevenueBudget GetRevenueBudget(BudgetKeyTuple key)
		{
			var select = new PXSelect<PMChangeOrderRevenueBudget,
				Where<PMChangeOrderRevenueBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
				And<PMChangeOrderRevenueBudget.projectID, Equal<Required<PMChangeOrderRevenueBudget.projectID>>,
				And<PMChangeOrderRevenueBudget.projectTaskID, Equal<Required<PMChangeOrderRevenueBudget.projectTaskID>>,
				And<PMChangeOrderRevenueBudget.accountGroupID, Equal<Required<PMChangeOrderRevenueBudget.accountGroupID>>,
				And<PMChangeOrderRevenueBudget.costCodeID, Equal<Required<PMChangeOrderRevenueBudget.costCodeID>>,
				And<PMChangeOrderRevenueBudget.inventoryID, Equal<Required<PMChangeOrderRevenueBudget.inventoryID>>>>>>>>>(Base);

			return select.SelectSingle(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, key.CostCodeID, key.InventoryID);
		}

		protected virtual PMChangeOrderRevenueBudget ExtractRevenueBudget(PMChangeRequestLine line)
		{
			if (line.RevenueTaskID == null || line.RevenueAccountGroupID == null)
				return null;

			PMAccountGroup accountGroup = PMAccountGroup.PK.Find(Base, line.RevenueAccountGroupID);
			if (accountGroup == null)
				return null;

			PMBudget filter = new PMBudget();
			filter.ProjectID = line.ProjectID;
			filter.ProjectTaskID = line.RevenueTaskID;
			filter.AccountGroupID = line.RevenueAccountGroupID;
			filter.CostCodeID = line.RevenueCodeID;
			filter.InventoryID = line.InventoryID;

			bool isExisting;
			BudgetService budgetService = new BudgetService(Base);
			Lite.PMBudget target = budgetService.SelectProjectBalance(filter, accountGroup, Base.Project.Current, out isExisting);

			PMChangeOrderRevenueBudget budget = new PMChangeOrderRevenueBudget();
			budget.RefNbr = Base.Document.Current.RefNbr;
			budget.ProjectID = target.ProjectID;
			budget.ProjectTaskID = target.TaskID;
			budget.AccountGroupID = target.AccountGroupID;
			budget.CostCodeID = target.CostCodeID;
			budget.InventoryID = target.InventoryID;
			budget.UOM = line.UOM;
			budget.Qty = line.Qty;
			budget.Amount = line.LineAmount;

			return budget;
		}

		protected virtual PMChangeOrderRevenueBudget ExtractRevenueBudget(PMChangeRequestMarkup line)
		{
			if (line.TaskID == null || line.AccountGroupID == null)
				return null;

			PMAccountGroup accountGroup = PMAccountGroup.PK.Find(Base, line.AccountGroupID);
			if (accountGroup == null)
				return null;

			PMBudget filter = new PMBudget();
			filter.ProjectID = Base.Document.Current.ProjectID;
			filter.ProjectTaskID = line.TaskID;
			filter.AccountGroupID = line.AccountGroupID;
			filter.CostCodeID = line.CostCodeID;
			filter.InventoryID = line.InventoryID;

			bool isExisting;
			BudgetService budgetService = new BudgetService(Base);
			Lite.PMBudget target = budgetService.SelectProjectBalance(filter, accountGroup, Base.Project.Current, out isExisting);

			PMChangeOrderRevenueBudget budget = new PMChangeOrderRevenueBudget();
			budget.RefNbr = Base.Document.Current.RefNbr;
			budget.ProjectID = target.ProjectID;
			budget.ProjectTaskID = target.TaskID;
			budget.AccountGroupID = target.AccountGroupID;
			budget.CostCodeID = target.CostCodeID;
			budget.InventoryID = target.InventoryID;
			budget.Amount = line.MarkupAmount;

			return budget;
		}

		protected virtual void AddCommitmentLine(PMChangeRequestLine line)
		{
			PMChangeOrderLine detail = Base.Details.Insert();
			detail.TaskID = line.CostTaskID;
			detail.CostCodeID = line.CostCodeID;
			detail.InventoryID = line.InventoryID;
			detail.Description = line.Description;
			detail.UOM = line.UOM;
			detail.Qty = line.Qty;
			detail.UnitCost = line.UnitCost;
			detail.Amount = line.ExtCost;
			detail.AmountInProjectCury = line.ExtCost;
			detail.CuryID = Base.Project.Current.CuryID;
			detail.VendorID = line.VendorID;

			detail = Base.Details.Update(detail);
			Base.Details.Cache.SetValue<PMChangeOrderLine.changeRequestRefNbr>(detail, line.RefNbr);
			Base.Details.Cache.SetValue<PMChangeOrderLine.changeRequestLineNbr>(detail, line.LineNbr);
		}

		protected virtual void _(Events.RowDeleting<PMChangeRequest> e)
		{
			if (e.Row.Released == true && Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled != true)
			{
				throw new PXException(Messages.CannotDeleteLinkedCR, e.Row.ChangeOrderNbr);
			}
		}

		protected virtual void _(Events.RowDeleted<PMChangeRequest> e)
		{
			e.Cache.SetStatus(e.Row, PXEntryStatus.Updated);

			var select = new PXSelect<PMChangeRequestLine, Where<PMChangeRequestLine.refNbr, Equal<Required<PMChangeRequestLine.refNbr>>>>(Base);

			Dictionary<BudgetKeyTuple, PMChangeOrderCostBudget> costBudgets = new Dictionary<BudgetKeyTuple, PMChangeOrderCostBudget>();
			Dictionary<BudgetKeyTuple, PMChangeOrderRevenueBudget> revenueBudgets = new Dictionary<BudgetKeyTuple, PMChangeOrderRevenueBudget>();
			foreach (PMChangeRequestLine line in select.Select(e.Row.RefNbr))
			{
				if (e.Row.CostChangeOrderNbr == Base.Document.Current.RefNbr)
				{
					PMChangeOrderCostBudget costFromLine = ExtractCostBudget(line);
					if (costFromLine != null)
					{
					PMChangeOrderCostBudget cost = AddToCostBudget(costFromLine, -1);
					if (cost != null)
					{
						Base.AddToDraftBucket(costFromLine);

						var costKey = BudgetKeyTuple.Create(cost);
						if (!costBudgets.ContainsKey(costKey))
						{
							costBudgets.Add(costKey, cost);
						}
					}
				}
				}

				if (Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled == true)
				{
					PMChangeOrderRevenueBudget revenueFromLine = ExtractRevenueBudget(line);
					if (revenueFromLine != null)
					{
					PMChangeOrderRevenueBudget revenue = AddToRevenueBudget(ExtractRevenueBudget(line), -1);
					if (revenue != null)
					{
						Base.AddToDraftBucket(revenueFromLine);

						var revenueKey = BudgetKeyTuple.Create(revenue);
						if (!revenueBudgets.ContainsKey(revenueKey))
						{
							revenueBudgets.Add(revenueKey, revenue);
						}
					}
				}
			}
			}

			if (e.Row.CostChangeOrderNbr == Base.Document.Current.RefNbr)
			{
				e.Row.CostChangeOrderNbr = null;
			}

			Base.Document.Current.DelayDays = Math.Max(0, Base.Document.Current.DelayDays.GetValueOrDefault() - e.Row.DelayDays.GetValueOrDefault());
			

			if (Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled == true)
			{
				var selectMarkup = new PXSelect<PMChangeRequestMarkup, Where<PMChangeRequestMarkup.refNbr, Equal<Required<PMChangeRequestMarkup.refNbr>>>>(Base);
				foreach (PMChangeRequestMarkup line in selectMarkup.Select(e.Row.RefNbr))
				{
					PMChangeOrderRevenueBudget revenue = ExtractRevenueBudget(line);
					if (revenue != null)
					{
						AddToRevenueBudget(revenue, -1);
						Base.AddToDraftBucket(revenue);

						var revenueKey = BudgetKeyTuple.Create(revenue);
						if (!revenueBudgets.ContainsKey(revenueKey))
						{
							revenueBudgets.Add(revenueKey, revenue);
						}
					}
				}

				e.Row.ChangeOrderNbr = null;
				e.Row.Status = ChangeRequestStatus.Open;
				e.Row.Released = false;
			}
			
			var selectLines = new PXSelect<PMChangeOrderLine,
				Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
				And<PMChangeOrderLine.changeRequestRefNbr, Equal<Required<PMChangeOrderLine.changeRequestRefNbr>>>>>(Base);

			foreach(PMChangeOrderLine line in selectLines.Select(e.Row.RefNbr))
			{
				Base.Details.SetValueExt<PMChangeOrderLine.changeRequestRefNbr>(line, null);
				Base.Details.Delete(line);
			}

			foreach (PMChangeOrderCostBudget budget in costBudgets.Values)
			{
				if (!IsReferencedByOtherChangeRequest(budget, e.Row.RefNbr))
				{
					Base.CostBudget.SetValueExt<PMChangeOrderCostBudget.isDisabled>(budget, false);
				}
			}

			foreach (PMChangeOrderRevenueBudget budget in revenueBudgets.Values)
			{
				if (!IsReferencedByOtherChangeRequest(budget, e.Row.RefNbr))
				{
					Base.RevenueBudget.SetValueExt<PMChangeOrderRevenueBudget.isDisabled>(budget, false);
				}
			}

			DeleteClearedBudgets();

			PMChangeOrder extDoc = Base.Document.Current;
			extDoc.ChangeRequestCostTotal = extDoc.ChangeRequestCostTotal.GetValueOrDefault() - e.Row.CostTotal.GetValueOrDefault();
			extDoc.ChangeRequestLineTotal = extDoc.ChangeRequestLineTotal.GetValueOrDefault() - e.Row.LineTotal.GetValueOrDefault();
			extDoc.ChangeRequestMarkupTotal = extDoc.ChangeRequestMarkupTotal.GetValueOrDefault() - e.Row.MarkupTotal.GetValueOrDefault();
			extDoc.ChangeRequestPriceTotal = extDoc.ChangeRequestPriceTotal.GetValueOrDefault() - e.Row.PriceTotal.GetValueOrDefault();

			Base.Document.Update(Base.Document.Current);
		}
				
		protected virtual void DeleteClearedBudgets()
		{
			foreach (PMChangeOrderCostBudget budget in Base.CostBudget.Cache.Dirty)
			{
				if (budget.Qty == 0 && budget.Amount == 0 &&
					(decimal?)Base.CostBudget.Cache.GetValue<PMChangeOrderCostBudget.changeRequestAmount>(budget) == 0 &&
					(decimal?)Base.CostBudget.Cache.GetValue<PMChangeOrderCostBudget.changeRequestQty>(budget) == 0)
				{
					Base.CostBudget.Delete(budget);
				}
			}

			if (Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled == true)
			{
				foreach (PMChangeOrderRevenueBudget budget in Base.RevenueBudget.Cache.Dirty)
				{
					if (budget.Qty == 0 && budget.Amount == 0 &&
						(decimal?)Base.RevenueBudget.Cache.GetValue<PMChangeOrderRevenueBudget.changeRequestAmount>(budget) == 0 &&
						(decimal?)Base.RevenueBudget.Cache.GetValue<PMChangeOrderRevenueBudget.changeRequestQty>(budget) == 0)
					{
						Base.RevenueBudget.Delete(budget);
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeRequest, PMChangeRequest.selected> e)
		{
			VerifyRequestCanBeAdded(e.Row.RefNbr);
		}

		protected virtual void _(Events.RowSelected<PMChangeOrder> e)
		{
			if (!string.IsNullOrEmpty(e.Row.ClassID))
			{
				addChangeRequests.SetVisible(Base.ChangeOrderClass.Current != null && Base.ChangeOrderClass.Current.IsAdvance == true);
				e.Row.IsChangeRequestVisible = Base.ChangeOrderClass.Current.IsAdvance == true;
			}

			if(e.Row != null)
			{
				viewChangeRequestCostDetails.SetVisible(e.Row.IsChangeRequestVisible == true);
				viewChangeRequestRevenueDetails.SetVisible(e.Row.IsChangeRequestVisible == true);

				PXUIFieldAttribute.SetVisible<PMChangeOrder.changeRequestCostTotal>(e.Cache, e.Row, e.Row.IsChangeRequestVisible == true );
				PXUIFieldAttribute.SetVisible<PMChangeOrder.changeRequestLineTotal>(e.Cache, e.Row, e.Row.IsChangeRequestVisible == true && Base.ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true);
				PXUIFieldAttribute.SetVisible<PMChangeOrder.changeRequestMarkupTotal>(e.Cache, e.Row, e.Row.IsChangeRequestVisible == true && Base.ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true);
				PXUIFieldAttribute.SetVisible<PMChangeOrder.changeRequestPriceTotal>(e.Cache, e.Row, e.Row.IsChangeRequestVisible == true && Base.ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true);

				PXUIFieldAttribute.SetVisible<PMChangeOrderLine.changeRequestRefNbr>(Base.Details.Cache, null, e.Row.IsChangeRequestVisible == true);
			}

		}

		protected virtual void _(Events.RowSelected<PMChangeOrderCostBudget> e)
		{
			if (e.Row != null)
			{
				bool isEnabled = true != e.Row.IsDisabled;
				PXUIFieldAttribute.SetEnabled<PMChangeOrderCostBudget.projectTaskID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderCostBudget.accountGroupID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderCostBudget.costCodeID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderCostBudget.inventoryID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderCostBudget.uOM>(e.Cache, e.Row, isEnabled);
			}
		}

		protected virtual void _(Events.RowSelected<PMChangeOrderRevenueBudget> e)
		{
			if (e.Row != null)
			{
				bool isEnabled = true != e.Row.IsDisabled;
				PXUIFieldAttribute.SetEnabled<PMChangeOrderRevenueBudget.projectTaskID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderRevenueBudget.accountGroupID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderRevenueBudget.costCodeID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderRevenueBudget.inventoryID>(e.Cache, e.Row, isEnabled);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderRevenueBudget.uOM>(e.Cache, e.Row, isEnabled);
			}
		}


		protected virtual void _(Events.RowDeleting<PMChangeOrderCostBudget> e)
		{
			bool isEnabled = true != e.Row.IsDisabled;
			if (!isEnabled)
			{
				throw new PXException(Messages.ReferencedByCR);
			}
		}

		protected virtual void _(Events.RowDeleting<PMChangeOrderLine> e)
		{
			if (!string.IsNullOrEmpty(e.Row.ChangeRequestRefNbr))
			{
				throw new PXException(Messages.ReferencedByCR);
			}
		}


		protected virtual void _(Events.RowDeleting<PMChangeOrder> e)
		{
			if (ChangeRequests.Select().Count > 0)
			{
				throw new PXException(Messages.RemoveRequests);
			}
		}


		private string NewRefNbr = null;
		protected virtual void _(Events.RowPersisting<PMChangeOrder> e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				NewRefNbr = e.Row.RefNbr;
			}
		}
		protected virtual void _(Events.RowPersisted<PMChangeOrder> e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				foreach (PMChangeRequest request in ChangeRequests.Cache.Updated)
				{
					if (request.ChangeOrderNbr == NewRefNbr)
					{
						request.ChangeOrderNbr = e.Row.RefNbr;
					}

					if (request.CostChangeOrderNbr == NewRefNbr)
					{
						request.CostChangeOrderNbr = e.Row.RefNbr;
					}
				}
			}
		}

		

		protected virtual bool IsReferencedByOtherChangeRequest(PMChangeOrderCostBudget row, string crRefNbr)
		{
			if (Base.ChangeOrderClass.Current != null && Base.ChangeOrderClass.Current.IsAdvance == true)
			{
				var select = new PXSelectJoin<PMChangeRequestLine,
					InnerJoin<PMChangeRequest, On<PMChangeRequestLine.refNbr, Equal<PMChangeRequest.refNbr>>>,
					Where<PMChangeRequest.changeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>, Or<PMChangeRequest.costChangeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>>>>(Base);

				foreach (PMChangeRequestLine line in select.Select())
				{
					if (line.RefNbr == crRefNbr) continue;

					PMChangeOrderCostBudget budget = ExtractCostBudget(line);
					if (budget != null && BudgetKeyTuple.Create(row).Equals(BudgetKeyTuple.Create(budget)))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IsReferencedByOtherChangeRequest(PMChangeOrderRevenueBudget row, string crRefNbr)
		{
			if (Base.ChangeOrderClass.Current != null && Base.ChangeOrderClass.Current.IsAdvance == true)
			{
				var rowKey = BudgetKeyTuple.Create(row);

				var select = new PXSelectJoin<PMChangeRequestLine,
						InnerJoin<PMChangeRequest, On<PMChangeRequestLine.refNbr, Equal<PMChangeRequest.refNbr>>>,
						Where<PMChangeRequest.changeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>, Or<PMChangeRequest.costChangeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>>>>(Base);

				foreach (PMChangeRequestLine line in select.Select())
				{
					if (line.RefNbr == crRefNbr) continue;

					PMChangeOrderRevenueBudget budget = ExtractRevenueBudget(line);
					if (budget != null && rowKey.Equals(BudgetKeyTuple.Create(budget)))
					{
						return true;
					}
				}

				var selectMarkup = new PXSelectJoin<PMChangeRequestMarkup,
					InnerJoin<PMChangeRequest, On<PMChangeRequestMarkup.refNbr, Equal<PMChangeRequest.refNbr>>>,
					Where<PMChangeRequest.changeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>, Or<PMChangeRequest.costChangeOrderNbr, Equal<Current<PMChangeOrder.refNbr>>>>>(Base);

				foreach (PMChangeRequestMarkup line in selectMarkup.Select())
				{
					if (line.RefNbr == crRefNbr) continue;

					PMChangeOrderRevenueBudget budget = ExtractRevenueBudget(line);
					if (budget != null && rowKey.Equals(BudgetKeyTuple.Create(budget)))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool VerifyRequestCanBeAdded(string crRefNbr)
		{
			if (Base.ChangeOrderClass.Current != null && Base.ChangeOrderClass.Current.IsRevenueBudgetEnabled == true)
			{
				var select = new PXSelect<PMChangeRequestLine, Where<PMChangeRequestLine.refNbr, Equal<Required<PMChangeRequestLine.refNbr>>>>(Base);
				
				foreach (PMChangeRequestLine line in select.Select(crRefNbr))
				{
					if (Base.Project.Current.BudgetLevel == BudgetLevels.Task)
					{
						if (line.RevenueTaskID == null || line.RevenueAccountGroupID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_Task);
						}
					}
					else if (Base.Project.Current.BudgetLevel == BudgetLevels.Item)
					{
						if (line.RevenueTaskID == null || line.RevenueAccountGroupID == null || line.RevenueInventoryID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_TaskItem);
						}
					}
					else if (Base.Project.Current.BudgetLevel == BudgetLevels.CostCode)
					{
						if (line.RevenueTaskID == null || line.RevenueAccountGroupID == null || line.RevenueCodeID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_TaskCostCode);
						}
					}
					else if (Base.Project.Current.BudgetLevel == BudgetLevels.Detail)
					{
						if (line.RevenueTaskID == null || line.RevenueAccountGroupID == null || line.RevenueInventoryID == null || line.RevenueCodeID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_TaskCostCode);
						}
					}
				}

				var selectMarkup = new PXSelect<PMChangeRequestMarkup, Where<PMChangeRequestMarkup.refNbr, Equal<Required<PMChangeRequestMarkup.refNbr>>>>(Base);
				foreach (PMChangeRequestMarkup line in selectMarkup.Select(crRefNbr))
				{
					if (Base.Project.Current.BudgetLevel == BudgetLevels.Task)
					{
						if (line.TaskID == null || line.AccountGroupID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_Task);
						}
					}
					else if (Base.Project.Current.BudgetLevel == BudgetLevels.Item)
					{
						if (line.TaskID == null || line.AccountGroupID == null || line.InventoryID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_TaskItem);
						}
					}
					else if (Base.Project.Current.BudgetLevel == BudgetLevels.CostCode)
					{
						if (line.TaskID == null || line.AccountGroupID == null || line.CostCodeID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_TaskCostCode);
						}
					}
					else if (Base.Project.Current.BudgetLevel == BudgetLevels.Detail)
					{
						if (line.TaskID == null || line.AccountGroupID == null || line.InventoryID == null || line.CostCodeID == null)
						{
							throw new PXSetPropertyException(Messages.ChangeRequestCannotBeAdded_TaskCostCode);
						}
					}
				}
			}

			return true;
		}
	}
}
