using CommonServiceLocator;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.Objects.PM.ChangeRequest.GraphExtensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PM.ChangeRequest
{
	public class ChangeRequestEntry : PXGraph<ChangeRequestEntry, PMChangeRequest>
	{
		[InjectDependency]
		public IUnitRateService RateService { get; set; }

		public const string ChangeRequestReport = "PM643500";
		public const string ChangeRequestNotificationCD = "CHANGE REQUEST";

		#region DAC Overrides

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(PMChangeRequest.date), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(PMChangeRequest.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(PMChangeRequest.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMChangeRequest.costTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMChangeOrder.costTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region PMChangeRequestLine
		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<PMChangeRequestLine.inventoryID> e)
		{
		}

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<PMChangeRequestLine.revenueInventoryID> e)
		{
		}
		#endregion

		#endregion

		[PXViewName(Messages.ChangeRequest)]
		public PXSelect<PMChangeRequest> Document;
		public PXSelect<PMChangeRequest, Where<PMChangeRequest.refNbr, Equal<Current<PMChangeRequest.refNbr>>>> DocumentSettings;
		public PXSelect<PMChangeRequestMarkup, Where<PMChangeRequestMarkup.refNbr, Equal<Current<PMChangeRequest.refNbr>>>> Markups;
		[PXViewName(PX.Objects.PM.Messages.Project)]
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<PMChangeRequest.projectID.FromCurrent>> Project;
		public PXSetup<Company> Company;
		public PXSetup<PMSetup> Setup;

		[PXImport(typeof(PMChangeRequest))]
		[PXFilterable]
		public PXSelect<PMChangeRequestLine, Where<PMChangeRequestLine.refNbr, Equal<Current<PMChangeRequest.refNbr>>>> Details;

		[PXCopyPasteHiddenView]
		[PXViewName(PX.Objects.PM.Messages.Approval)]
		public EPApprovalList<PMChangeRequest, PMChangeRequest.approved, PMChangeRequest.rejected> Approval;

		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<PMChangeRequest.customerID>>>>))]
		[PMDefaultMailTo(typeof(Select2<Contact,
			InnerJoin<Customer, On<Customer.bAccountID, Equal<Contact.bAccountID>, And<Customer.defContactID, Equal<Contact.contactID>>>>,
			Where<Customer.bAccountID, Equal<Current<PMChangeOrder.customerID>>>>))]
		public PMActivityList<PMChangeRequest> Activity;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<PMBudgetAccum> Budget;

		[PXHidden]
		public PXSelect<PMForecastHistoryAccum> ForecastHistory;

		public ChangeRequestEntry()
		{
			InitalizeActionsMenu();
			InitializeReportsMenu();
		}

		protected ProjectBalance balanceCalculator;

		public virtual ProjectBalance BalanceCalculator
		{
			get
			{
				if (balanceCalculator == null)
				{
					balanceCalculator = new ProjectBalance(this);
				}

				return balanceCalculator;
			}
		}

		public PXAction<PMChangeRequest> approve;

		[PXUIField(DisplayName = "Approve")]
		public IEnumerable Approve(PXAdapter adapter)
		{
			List<PMChangeRequest> list = new List<PMChangeRequest>();

			foreach (PMChangeRequest item in adapter.Get())
			{
				list.Add(item);
			}

			Save.Press();

			foreach (PMChangeRequest item in list)
			{
				if (item.Approved == true)
					continue;

				if (Setup.Current.ChangeRequestApprovalMapID != null)
				{
					if (!Approval.Approve(item))
						throw new PXSetPropertyException(PX.Objects.Common.Messages.NotApprover);
					item.Approved = Approval.IsApproved(item);
					if (item.Approved == true)
						item.Status = ChangeOrderStatus.Open;
				}
				else
				{
					item.Approved = true;
					item.Status = ChangeOrderStatus.Open;
				}

				Document.Update(item);

				Save.Press();

				yield return item;
			}
		}

		public PXAction<PMChangeRequest> reject;

		[PXUIField(DisplayName = "Reject")]
		public IEnumerable Reject(PXAdapter adapter)
		{
			List<PMChangeRequest> list = new List<PMChangeRequest>();

			foreach (PMChangeRequest item in adapter.Get())
			{
				list.Add(item);
			}

			Save.Press();

			foreach (PMChangeRequest item in list)
			{
				if (item.Approved == true || item.Rejected == true)
					continue;

				if (Setup.Current.ChangeRequestApprovalMapID != null)
				{
					if (!Approval.Reject(item))
						throw new PXSetPropertyException(PX.Objects.Common.Messages.NotApprover);
					item.Rejected = true;
				}
				else
				{
					item.Rejected = true;
				}

				item.Status = ChangeOrderStatus.Rejected;
				Document.Update(item);

				Save.Press();

				yield return item;
			}
		}

		public PXAction<PMChangeRequest> createChangeOrder;
		[PXUIField(DisplayName = "Create Change Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable CreateChangeOrder(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.Hold != true && Document.Current.Released != true)
			{
				Save.Press();

				ChangeOrderEntry target = PXGraph.CreateInstance<ChangeOrderEntry>();
				target.Document.Insert();
				target.Document.SetValueExt<PMChangeOrder.projectID>(target.Document.Current, Project.Current.ContractID);
				if (target.ChangeOrderClass.Current == null || target.ChangeOrderClass.Current.IsAdvance != true)
				{
					throw new PXException(Messages.InvlaidClass);
				}
				ChangeOrderEntryExt ext = target.GetExtension<ChangeOrderEntryExt>();
				ext.AddChangeRequest(Document.Current);

				throw new PXRedirectRequiredException(target, false, Messages.ChangeRequest) { Mode = PXBaseRedirectException.WindowMode.Same };
			}
			return adapter.Get();
		}

		public PXAction<PMChangeRequest> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<PMChangeRequest> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<PMChangeRequest> crReport;
		[PXUIField(DisplayName = "Print Change Request", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable CRReport(PXAdapter adapter)
		{
			OpenReport(ChangeRequestReport, Document.Current);

			return adapter.Get();
		}

		public virtual void OpenReport(string reportID, PMChangeRequest doc)
		{
			if (doc != null && Document.Cache.GetStatus(doc) != PXEntryStatus.Inserted)
			{
				var utility = new CR.NotificationUtility(this);
				string specificReportID = utility.SearchReport(PMNotificationSource.Project, Project.Current, reportID, Project.Current.DefaultBranchID);

				var parameters = new Dictionary<string, string>();
				parameters["RefNbr"] = doc.RefNbr;
				throw new PXReportRequiredException(parameters, specificReportID, specificReportID);
			}
		}

		public PXAction<PMChangeRequest> send;
		[PXUIField(DisplayName = "Email Change Request", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Send(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				PXLongOperation.StartOperation(this, delegate () {
					SendReport(ChangeRequestNotificationCD, Document.Current);
				});
			}

			return adapter.Get();
		}

		public virtual void SendReport(string notificationCD, PMChangeRequest doc)
		{
			if (doc != null)
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>();
				mailParams["RefNbr"] = Document.Current.RefNbr;

				using (var ts = new PXTransactionScope())
				{
					Activity.SendNotification(PMNotificationSource.Project, notificationCD, Project.Current.DefaultBranchID, mailParams);
					this.Save.Press();

					ts.Complete();
				}
			}
		}

		public virtual void InitalizeActionsMenu()
		{
			action.MenuAutoOpen = true;
			action.AddMenuAction(approve);
			action.AddMenuAction(reject);
			action.AddMenuAction(send);
		}

		public virtual void InitializeReportsMenu()
		{
			report.MenuAutoOpen = true;
			report.AddMenuAction(crReport);
		}

		private IFinPeriodRepository finPeriodsRepo;
		public virtual IFinPeriodRepository FinPeriodRepository
		{
			get
			{
				if (finPeriodsRepo == null)
				{
					finPeriodsRepo = new FinPeriodRepository(this);
				}

				return finPeriodsRepo;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequestLine, PMChangeRequestLine.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMChangeRequestLine.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeRequestLine.unitCost>(e.Row);
			e.Cache.SetDefaultExt<PMChangeRequestLine.priceMarkupPct>(e.Row);

			if (!CostCodeAttribute.UseCostCode())
				e.Cache.SetDefaultExt<PMChangeRequestLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequestLine, PMChangeRequestLine.uOM> e)
		{
			e.Cache.SetDefaultExt<PMChangeRequestLine.unitCost>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequestLine, PMChangeRequestLine.costCodeID> e)
		{
			if (CostCodeAttribute.UseCostCode())
				e.Cache.SetDefaultExt<PMChangeRequestLine.description>(e.Row);
		}

		
		protected virtual void _(Events.FieldUpdated<PMChangeRequestLine, PMChangeRequestLine.costAccountGroupID> e)
		{
			e.Cache.SetDefaultExt<PMChangeRequestLine.lineMarkupPct>(e.Row);
			e.Cache.SetDefaultExt<PMChangeRequestLine.isCommitment>(e.Row);
			if (e.Row.RevenueAccountGroupID == null)
				e.Cache.SetDefaultExt<PMChangeRequestLine.revenueAccountGroupID>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeRequestLine, PMChangeRequestLine.unitCost> e)
		{
			if (e.Row == null) return;
			PMProject project = PMProject.PK.Find(this, e.Row.ProjectID);
			e.NewValue = RateService.CalculateUnitCost(e.Cache, e.Row.ProjectID, e.Row.CostTaskID, e.Row.InventoryID, e.Row.UOM, null, Document.Current.Date, project.CuryInfoID);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeRequestLine, PMChangeRequestLine.priceMarkupPct> e)
		{
			if (e.Row == null) return;

			InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMChangeRequestLine.inventoryID>(e.Cache, e.Row);

			if (item != null && item.MarkupPct != 0)
			{
				e.NewValue = item.MarkupPct;
			}
			else
			{
				e.NewValue =Setup.Current?.DefaultPriceMarkupPct;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeRequestLine, PMChangeRequestLine.revenueAccountGroupID> e)
		{
			if (e.Row == null) return;

			var select = new PXSelect<PMAccountGroup, Where<PMAccountGroup.type, Equal<PX.Objects.GL.AccountType.income>>>(this);

			var resultset = select.SelectWindowed(0, 2);

			if (resultset.Count == 1)
			{
				e.NewValue = ((PMAccountGroup)resultset).GroupID;
			}
			else
			{
				if (e.Row.CostAccountGroupID != null)
				{
					PMAccountGroup ag = PMAccountGroup.PK.Find(this, e.Row.CostAccountGroupID);
					if (ag != null && ag.RevenueAccountGroupID != null)
					{
						e.NewValue = ag.RevenueAccountGroupID;
					}
				}
			}
		}

		protected virtual void _(Events.RowInserted<PMChangeRequestLine> e)
		{
			if (Document.Current.Approved == true)
			{
				AddToDraftBucket(e.Row);
			}
		}

		protected virtual void _(Events.RowUpdated<PMChangeRequestLine> e)
		{
			if (Document.Current.Approved == true)
			{
				AddToDraftBucket(e.OldRow, -1);
				AddToDraftBucket(e.Row);
			}
		}

		protected virtual void _(Events.RowDeleted<PMChangeRequestLine> e)
		{
			if (Document.Current.Approved == true)
			{
				AddToDraftBucket(e.Row, -1);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequest, PMChangeRequest.projectID> e)
		{
			if (!IsCopyPasteContext)
			{
				foreach (PMChangeRequestMarkup markup in Markups.Select())
				{
					Markups.Delete(markup);
				}

				var select = new PXSelect<PMMarkup, Where<PMMarkup.projectID, Equal<Current<PMChangeRequest.projectID>>>>(this);

				foreach (PMMarkup setup in select.Select())
				{
					PMChangeRequestMarkup markup = new PMChangeRequestMarkup();
					markup.RefNbr = e.Row.RefNbr;
					markup.Type = setup.Type;
					markup.Description = setup.Description;
					markup.Value = setup.Value;
					markup.TaskID = setup.TaskID;
					markup.CostCodeID = setup.CostCodeID;
					markup.AccountGroupID = setup.AccountGroupID;
					markup.InventoryID = setup.InventoryID;

					Markups.Insert(markup);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequest, PMChangeRequest.lineTotal> e)
		{
			RecalculateMarkupAmount(e.Row, false);
		}

        protected virtual void _(Events.FieldVerifying<PMChangeRequest, PMChangeRequest.date> e)
        {
            if (e.NewValue != null)
            {
                Func<PXGraph, IFinPeriodRepository> factoryFunc = (Func<PXGraph, IFinPeriodRepository>)ServiceLocator.Current.GetService(typeof(Func<PXGraph, IFinPeriodRepository>));
                IFinPeriodRepository service = factoryFunc(this);

                if (service != null)
                {
                    try
                    {
                        var finperiod = service.GetFinPeriodByDate((DateTime?)e.NewValue, PXAccess.GetParentOrganizationID(Accessinfo.BranchID));

                        if (finperiod == null)
                        {
                            throw new PXSetPropertyException(Messages.InvalidDate, e.NewValue);
                        }
                    }
                    catch (PXException ex)
                    {
                        throw new PXSetPropertyException(ex, PXErrorLevel.Error, Messages.InvalidDate, e.NewValue);
                    }
                }
            }
        }

		protected virtual void _(Events.FieldUpdated<PMChangeRequestMarkup, PMChangeRequestMarkup.amount> e)
		{
			e.Cache.SetDefaultExt<PMChangeRequestMarkup.markupAmount>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequestMarkup, PMChangeRequestMarkup.type> e)
		{
			e.Cache.SetDefaultExt<PMChangeRequestMarkup.amount>(e.Row);
			e.Cache.SetDefaultExt<PMChangeRequestMarkup.markupAmount>(e.Row);

			if (e.Row.Type == PMMarkupLineType.Cumulative)
			{
				RecalculateMarkupAmount(Document.Current, true);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequestMarkup, PMChangeRequestMarkup.value> e)
		{
			e.Cache.SetDefaultExt<PMChangeRequestMarkup.amount>(e.Row);
			e.Cache.SetDefaultExt<PMChangeRequestMarkup.markupAmount>(e.Row);
		}

		protected virtual void _(Events.RowInserted<PMChangeRequestMarkup> e)
		{
			if (Document.Current.Approved == true)
			{
				AddToDraftBucket(e.Row);
			}
		}

		protected virtual void _(Events.RowUpdated<PMChangeRequestMarkup> e)
		{
			if (e.Row.Type == PMMarkupLineType.Percentage)
			{
				RecalculateMarkupAmount(Document.Current, true);
				Markups.View.RequestRefresh();
			}

			if (Document.Current.Approved == true)
			{
				AddToDraftBucket(e.OldRow, -1);
				AddToDraftBucket(e.Row);
			}
		}

		protected virtual void _(Events.RowDeleted<PMChangeRequestMarkup> e)
		{
			if (Document.Current.Approved == true)
			{
				AddToDraftBucket(e.Row, -1);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeRequestMarkup, PMChangeRequestMarkup.amount> e)
		{
			if (e.Row.Type != PMMarkupLineType.Cumulative && Document.Current != null)
			{
				e.NewValue = Document.Current.LineTotal;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeRequestMarkup, PMChangeRequestMarkup.markupAmount> e)
		{
			if (e.Row.Type == PMMarkupLineType.FlatFee)
			{
				e.NewValue = e.Row.Value;
			}
			else
			{
				e.NewValue = e.Row.Value.GetValueOrDefault() * 0.01m * e.Row.Amount.GetValueOrDefault();
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeRequestLine, PMChangeRequestLine.description> e)
		{
			if (e.Row == null) return;

			if (CostCodeAttribute.UseCostCode())
			{
				if (e.Row.CostCodeID != null && e.Row.CostCodeID != CostCodeAttribute.GetDefaultCostCode())
				{
					PMCostCode costCode = PXSelectorAttribute.Select<PMChangeRequestLine.costCodeID>(e.Cache, e.Row) as PMCostCode;
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
					InventoryItem item = PXSelectorAttribute.Select<PMChangeRequestLine.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMChangeRequest> e)
		{
			approve.SetEnabled(e.Row?.Status == ChangeOrderStatus.PendingApproval);
			reject.SetEnabled(e.Row?.Status == ChangeOrderStatus.PendingApproval);
			createChangeOrder.SetEnabled(e.Row.Hold != true && e.Row.Released != true &&e.Row.Approved == true);
			
			if (e.Row.ProjectID != null && Project.Current != null)
			{				
				PXUIFieldAttribute.SetVisible<PMChangeRequestMarkup.inventoryID>(Markups.Cache, null, Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail);
				
				bool visible = CostCodeAttribute.UseCostCode() && (Project.Current.BudgetLevel == BudgetLevels.CostCode || Project.Current.BudgetLevel == BudgetLevels.Detail);
				PXUIFieldAttribute.SetVisible<PMChangeRequestMarkup.costCodeID>(Markups.Cache, null, visible);
				PXUIFieldAttribute.SetVisible<PMChangeRequestLine.revenueCodeID>(Details.Cache, null, visible);
				PXUIFieldAttribute.SetVisible<PMChangeRequestLine.revenueInventoryID>(Details.Cache, null, Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail);
				
				PXUIVisibility costCodeIDVisibility = visible ? PXUIVisibility.Visible : PXUIVisibility.Invisible;
				PXUIFieldAttribute.SetVisibility<PMChangeRequestMarkup.costCodeID>(Markups.Cache, null, costCodeIDVisibility);
				PXUIFieldAttribute.SetVisibility<PMChangeRequestLine.revenueCodeID>(Details.Cache, null, costCodeIDVisibility);
			}

			PXUIFieldAttribute.SetVisible<PMChangeRequest.costChangeOrderNbr>(e.Cache, e.Row, PXGraph.ProxyIsActive || e.Row.CostChangeOrderNbr != e.Row.ChangeOrderNbr && !string.IsNullOrEmpty(e.Row.CostChangeOrderNbr));
						
			bool isEditable = e.Row.Hold == true;

			if (e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted)
			{
				//editable only if OnHold status is saved.
				bool? oldHold = (bool?)Document.Cache.GetValueOriginal<PMChangeRequest.hold>(e.Row);
				isEditable = isEditable && oldHold == true;
			}

			Document.Cache.AllowDelete = isEditable;						
			Details.Cache.AllowInsert = isEditable;
			Details.Cache.AllowUpdate = e.Row.Released != true;
			Details.Cache.AllowDelete = isEditable;
			Markups.Cache.AllowInsert = e.Row.Released != true;
			Markups.Cache.AllowUpdate = e.Row.Released != true;
			Markups.Cache.AllowDelete = e.Row.Released != true;

			PXUIFieldAttribute.SetEnabled<PMChangeRequest.projectID>(e.Cache, e.Row, isEditable && IsProjectEnabled());
			PXUIFieldAttribute.SetEnabled<PMChangeRequest.description>(e.Cache, e.Row, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequest.hold>(e.Cache, e.Row, string.IsNullOrEmpty(e.Row.ChangeOrderNbr) && string.IsNullOrEmpty(e.Row.CostChangeOrderNbr));
			PXUIFieldAttribute.SetEnabled<PMChangeRequest.extRefNbr>(e.Cache, e.Row, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequest.date>(e.Cache, e.Row, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequest.delayDays>(e.Cache, e.Row, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequest.text>(e.Cache, e.Row, isEditable);

			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.costAccountGroupID>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.costCodeID>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.costTaskID>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.description>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.extCost>(Details.Cache, null, isEditable);
			//PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.extPrice>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.inventoryID>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.isCommitment>(Details.Cache, null, isEditable);
			//PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.lineAmount>(Details.Cache, null, isEditable);
			//PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.lineMarkupPct>(Details.Cache, null, isEditable);
			//PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.priceMarkupPct>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.qty>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.subItemID>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.unitCost>(Details.Cache, null, isEditable);
			//PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.unitPrice>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.uOM>(Details.Cache, null, isEditable);
			PXUIFieldAttribute.SetEnabled<PMChangeRequestLine.vendorID>(Details.Cache, null, isEditable);

			//PXUIFieldAttribute.SetEnabled<PMChangeRequestMarkup.type>(Markups.Cache, null, isEditable);
			//PXUIFieldAttribute.SetEnabled<PMChangeRequestMarkup.description>(Markups.Cache, null, isEditable);
			//PXUIFieldAttribute.SetEnabled<PMChangeRequestMarkup.value>(Markups.Cache, null, isEditable);
		}

		protected virtual void _(Events.RowPersisting<PMChangeRequestLine> e)
		{
			if (e.Row != null && Document.Current != null && Document.Current.Hold != true && e.Operation != PXDBOperation.Delete)
			{
				if (e.Row.CostAccountGroupID == null)
				{
					e.Cache.RaiseExceptionHandling<PMChangeRequestLine.costAccountGroupID>(e.Row, null, new PXSetPropertyException<PMChangeRequestLine.costAccountGroupID>(Data.ErrorMessages.FieldIsEmpty, nameof(PMChangeRequestLine.CostAccountGroupID)));
					throw new PXRowPersistingException(nameof(PMChangeRequestLine.CostAccountGroupID), e.Row.CostAccountGroupID, ErrorMessages.FieldIsEmpty, nameof(PMChangeRequestLine.CostAccountGroupID));
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeRequest, PMChangeRequest.hold> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Hold == true)
				{
					Approval.Reset(e.Row);
					e.Cache.SetValue<PMChangeRequest.approved>(e.Row, false);
					e.Cache.SetValue<PMChangeRequest.rejected>(e.Row, false);
					e.Cache.SetValue<PMChangeRequest.status>(e.Row, ChangeRequestStatus.OnHold);
				}
				else
				{
					if (Setup.Current.ChangeRequestApprovalMapID != null)
					{
						Approval.Assign(e.Row, Setup.Current.ChangeRequestApprovalMapID, Setup.Current.ChangeRequestApprovalNotificationID);
						if (true == (bool?)e.Cache.GetValue<PMChangeOrder.approved>(e.Row))
						{
							e.Cache.SetValue<PMChangeRequest.status>(e.Row, ChangeRequestStatus.Open);
						}
						else
						{
							e.Cache.SetValue<PMChangeRequest.status>(e.Row, ChangeRequestStatus.PendingApproval);
						}
					}
					else
					{
						e.Cache.SetValue<PMChangeRequest.approved>(e.Row, true);
						e.Cache.SetValue<PMChangeRequest.status>(e.Row, ChangeOrderStatus.Open);
					}
				}
			}
		}

		public virtual EmployeeCostEngine CreateEmployeeCostEngine()
		{
			return new EmployeeCostEngine(this);
		}
				
		public virtual bool IsProjectEnabled()
		{
			if (Details.Cache.IsInsertedUpdatedDeleted)
				return false;
			
			if (Details.Select().Count > 0)
				return false;

			return true;
		}

		protected virtual void RecalculateMarkupAmount(PMChangeRequest row, bool onlyCumulative)
		{
			decimal markupTotal = 0;
			foreach (PMChangeRequestMarkup markup in Markups.Select())
			{
				PMChangeRequestMarkup copy = (PMChangeRequestMarkup)Markups.Cache.CreateCopy(markup);
				if (markup.Type == PMMarkupLineType.FlatFee && !onlyCumulative)
				{
					copy.Amount = row.LineTotal;
					Markups.Update(copy);
				}
				else if (markup.Type == PMMarkupLineType.Percentage)
				{
					if (!onlyCumulative)
					{
						copy.Amount = row.LineTotal;
						var updatedMarkup = Markups.Update(copy);
						markupTotal += updatedMarkup.MarkupAmount.GetValueOrDefault();
					}
					else
					{
						markupTotal += markup.MarkupAmount.GetValueOrDefault();
					}
					
				}
				if (markup.Type == PMMarkupLineType.Cumulative)
				{
					copy.Amount = markupTotal;
					Markups.Update(copy);
				}
			}
		}

		protected virtual PMBudgetAccum ExtractCostBudget(PMChangeRequestLine line)
		{
			if (line.CostTaskID == null || line.CostAccountGroupID == null)
				return null;

			PMAccountGroup accountGroup = PMAccountGroup.PK.Find(this, line.CostAccountGroupID);
			if (accountGroup == null)
				return null;

			Lite.PMBudget budget = new Lite.PMBudget();
			budget.ProjectID = line.ProjectID;
			budget.ProjectTaskID = line.CostTaskID;
			budget.AccountGroupID = line.CostAccountGroupID;
			budget.InventoryID = line.InventoryID;
			budget.CostCodeID = line.CostCodeID;

			bool isExisting;
			BudgetService budgetService = new BudgetService(this);
			budget = budgetService.SelectProjectBalance(budget, accountGroup, Project.Current, out isExisting);

			
			return FromLiteBudget(budget);
		}

		protected virtual PMBudgetAccum FromLiteBudget(Lite.PMBudget budget)
		{
			PMBudgetAccum target = new PMBudgetAccum();
			target.ProjectID = budget.ProjectID;
			target.ProjectTaskID = budget.ProjectTaskID;
			target.AccountGroupID = budget.AccountGroupID;
			target.InventoryID = budget.InventoryID;
			target.CostCodeID = budget.CostCodeID;
			target.Type = budget.Type;
			target.UOM = budget.UOM;
			target.IsProduction = budget.IsProduction;

			return target;
		}

		protected virtual PMBudgetAccum ExtractRevenueBudget(PMChangeRequestLine line)
		{			
			if (line.RevenueTaskID == null || line.RevenueAccountGroupID == null)
				return null;

			PMAccountGroup accountGroup = PMAccountGroup.PK.Find(this, line.RevenueAccountGroupID);
			if (accountGroup == null)
				return null;

			Lite.PMBudget budget = new Lite.PMBudget();
			budget.ProjectID = line.ProjectID;
			budget.ProjectTaskID = line.RevenueTaskID;
			budget.AccountGroupID = line.RevenueAccountGroupID;
			budget.Type = PX.Objects.GL.AccountType.Expense;
			budget.InventoryID = line.InventoryID;
			budget.CostCodeID = line.RevenueCodeID;

			bool isExisting;
			BudgetService budgetService = new BudgetService(this);
			budget = budgetService.SelectProjectBalance(budget, accountGroup, Project.Current, out isExisting);
			
			return FromLiteBudget(budget);
		}

		protected virtual PMBudgetAccum ExtractRevenueBudget(PMChangeRequestMarkup line)
		{
			if (line.TaskID == null || line.AccountGroupID == null)
				return null;

			PMAccountGroup accountGroup = PMAccountGroup.PK.Find(this, line.AccountGroupID);
			if (accountGroup == null)
				return null;

			Lite.PMBudget budget = new Lite.PMBudget();
			budget.ProjectID = Project.Current.ContractID;
			budget.ProjectTaskID = line.TaskID;
			budget.AccountGroupID = line.AccountGroupID;
			budget.InventoryID = line.InventoryID;
			budget.CostCodeID = line.CostCodeID;

			bool isExisting;
			BudgetService budgetService = new BudgetService(this);
			budget = budgetService.SelectProjectBalance(budget, accountGroup, Project.Current, out isExisting);

			return FromLiteBudget(budget);
		}

		protected virtual void OnDocumentApprovedChanged(PMChangeRequest row, bool isApproved)
		{
			foreach (PMChangeRequestMarkup markup in Markups.Select())
			{
				AddToDraftBucket(markup, isApproved ? 1 : -1);
			}

			foreach (PMChangeRequestLine line in Details.Select())
			{
				AddToDraftBucket(line, isApproved ? 1 : -1);
			}
		}
		
		public virtual void AddToDraftBucket(PMChangeRequestLine row, int mult = 1)
		{
			PMBudgetAccum cost = ExtractCostBudget(row);

			if (cost != null)
			{
				var rollupQty = BalanceCalculator.CalculateRollupQty(row, cost);
					
				cost = Budget.Insert(cost);				
				cost.CuryDraftChangeOrderAmount += row.ExtCost.GetValueOrDefault() * mult;
				cost.DraftChangeOrderQty += rollupQty * mult;
				
				if (Document.Current != null)
				{
					FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(Document.Current.Date, FinPeriod.organizationID.MasterValue);

					if (finPeriod != null)
					{
						PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
						forecast.ProjectID = cost.ProjectID;
						forecast.ProjectTaskID = cost.ProjectTaskID;
						forecast.AccountGroupID = cost.AccountGroupID;
						forecast.InventoryID = cost.InventoryID;
						forecast.CostCodeID = cost.CostCodeID;
						forecast.PeriodID = finPeriod.FinPeriodID;

						forecast = ForecastHistory.Insert(forecast);

						forecast.DraftChangeOrderQty += rollupQty * mult;
						forecast.CuryDraftChangeOrderAmount += mult * row.ExtCost.GetValueOrDefault();
					}
				}
			}

			PMBudgetAccum revenue = ExtractRevenueBudget(row);

			if (revenue != null)
			{
				var rollupQty = BalanceCalculator.CalculateRollupQty(row, revenue);

				revenue = Budget.Insert(revenue);
				revenue.CuryDraftChangeOrderAmount += row.LineAmount.GetValueOrDefault() * mult;
				revenue.DraftChangeOrderQty += rollupQty * mult;

				if (Document.Current != null)
				{
					FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(Document.Current.Date, FinPeriod.organizationID.MasterValue);

					if (finPeriod != null)
					{
						PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
						forecast.ProjectID = revenue.ProjectID;
						forecast.ProjectTaskID = revenue.ProjectTaskID;
						forecast.AccountGroupID = revenue.AccountGroupID;
						forecast.InventoryID = revenue.InventoryID;
						forecast.CostCodeID = revenue.CostCodeID;
						forecast.PeriodID = finPeriod.FinPeriodID;

						forecast = ForecastHistory.Insert(forecast);

						forecast.DraftChangeOrderQty += mult * rollupQty;
						forecast.CuryDraftChangeOrderAmount += mult * row.LineAmount.GetValueOrDefault();
					}
				}
			}
		}

		public virtual void AddToDraftBucket(PMChangeRequestMarkup row, int mult = 1)
		{			
			PMBudgetAccum revenue = ExtractRevenueBudget(row);

			if (revenue != null)
			{
				revenue = Budget.Insert(revenue);
				revenue.CuryDraftChangeOrderAmount += row.MarkupAmount.GetValueOrDefault() * mult;

				if (Document.Current != null)
				{
					FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(Document.Current.Date, FinPeriod.organizationID.MasterValue);

					if (finPeriod != null)
					{
						PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
						forecast.ProjectID = revenue.ProjectID;
						forecast.ProjectTaskID = revenue.ProjectTaskID;
						forecast.AccountGroupID = revenue.AccountGroupID;
						forecast.InventoryID = revenue.InventoryID;
						forecast.CostCodeID = revenue.CostCodeID;
						forecast.PeriodID = finPeriod.FinPeriodID;

						forecast = ForecastHistory.Insert(forecast);

						forecast.CuryDraftChangeOrderAmount += mult * row.MarkupAmount.GetValueOrDefault();
					}
				}
			}
		}

		public override void Persist()
		{
			if (Document.Current != null)
			{
				bool? oldApproved = (bool?)Document.Cache.GetValueOriginal<PMChangeRequest.approved>(Document.Current);
				if (oldApproved.GetValueOrDefault() != Document.Current.Approved.GetValueOrDefault())
				{
					//Persist can be called multiple times (due to validation errors on UI), make sure to clear any previous accumulations: 
					Budget.Cache.Clear();
					ForecastHistory.Cache.Clear();

					OnDocumentApprovedChanged(Document.Current, Document.Current.Approved == true);
				}
			}

			base.Persist();
		}
	}
}
