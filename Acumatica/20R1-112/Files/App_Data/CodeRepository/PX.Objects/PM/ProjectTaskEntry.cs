using System;
using System.Collections.Generic;
using PX.Data;
using System.Collections;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.CT;

namespace PX.Objects.PM
{
	public class ProjectTaskEntry : PXGraph<ProjectTaskEntry, PMTask>
	{
		#region DAC Attributes Override

		#region PMTask
		
		[Project(typeof(Where<PMProject.nonProject, NotEqual<True>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>), DisplayName = "Project ID", IsKey = true)]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXDefault]
		protected virtual void PMTask_ProjectID_CacheAttached(PXCache sender) { }


		[PXDimensionSelector(ProjectTaskAttribute.DimensionName,
			typeof(Search<PMTask.taskCD, Where<PMTask.projectID, Equal<Current<PMTask.projectID>>>>),
			typeof(PMTask.taskCD),
			typeof(PMTask.taskCD), typeof(PMTask.locationID), typeof(PMTask.description), typeof(PMTask.status), DescriptionField = typeof(PMTask.description))]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void PMTask_TaskCD_CacheAttached(PXCache sender) { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault(ProjectTaskStatus.Planned)]
		[PXUIField(DisplayName = "Status", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void PMTask_Status_CacheAttached(PXCache sender) { }
		
		#endregion
		
		#region CRCampaign

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.AccountedCampaign, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void CRCampaign_CampaignID_CacheAttached(PXCache sender) { }

		#endregion

		#region PMBudget
				
		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<PMTask, Where<PMTask.taskID, Equal<Current<PMBudget.projectTaskID>>>>))]
		protected virtual void PMBudget_ProjectTaskID_CacheAttached(PXCache sender) { }

		#endregion

		#endregion

		#region Views/Selects

		public PXSelectJoin<PMTask, 
			LeftJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, 
			Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>> Task;

		[PXViewName(Messages.ProjectTask)]
		public PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMTask.taskID>>>> TaskProperties;
				
		public PXSelect<PMRecurringItem,
            Where<PMRecurringItem.projectID, Equal<Current<PMTask.projectID>>,
            And<PMRecurringItem.taskID, Equal<Current<PMTask.taskID>>>>> BillingItems;

		public PXSelect<PMBudget, Where<PMBudget.projectTaskID, Equal<Current<PMTask.taskID>>>> TaskBudgets;
       							
		[PXViewName(Messages.TaskAnswers)]
		public CRAttributeList<PMTask> Answers;

		[PXFilterable]
		[PXViewName(Messages.Activities)]
		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<PMTask.customerID>>>>))]
		public ProjectTaskActivities Activities;

       	public PXSetup<PMSetup> Setup;
		public PXSetup<Company> CompanySetup;
		[PXViewName(Messages.Project)]
		public PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>> Project;

		[PXReadOnlyView]
		public PXSelect<CRCampaign, Where<CRCampaign.projectID, Equal<Current<PMTask.projectID>>, And<CRCampaign.projectTaskID, Equal<Current<PMTask.taskID>>>>> TaskCampaign;

		#endregion


		public ProjectTaskEntry()
		{
			if (Setup.Current == null)
			{
				throw new PXException(Messages.SetupNotConfigured);
			}
						
			Activities.GetNewEmailAddress =
					() =>
						{
							PMProject current = Project.Select();
							if (current != null)
							{
								Contact customerContact = PXSelectJoin<Contact, InnerJoin<BAccount, On<BAccount.defContactID, Equal<Contact.contactID>>>, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(this, current.CustomerID);

								if (customerContact != null && !string.IsNullOrWhiteSpace(customerContact.EMail))
									return PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(customerContact.EMail, customerContact.DisplayName);
							}
							return String.Empty;
						};
		}

		#region Event Handlers

		protected virtual void PMTask_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<PMTask.visibleInGL>(sender, row, Setup.Current.VisibleInGL == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInAP>(sender, row, Setup.Current.VisibleInAP == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInAR>(sender, row, Setup.Current.VisibleInAR == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInSO>(sender, row, Setup.Current.VisibleInSO == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInPO>(sender, row, Setup.Current.VisibleInPO == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInTA>(sender, row, Setup.Current.VisibleInTA == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInEA>(sender, row, Setup.Current.VisibleInEA == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInIN>(sender, row, Setup.Current.VisibleInIN == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInCA>(sender, row, Setup.Current.VisibleInCA == true);

			PMProject project = Project.Select();
			if (project == null) return;

			string status = GetStatusFromFlags(row);
			bool projectEditable = !(project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Cancelled);
			PXUIFieldAttribute.SetEnabled<PMTask.status>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.description>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.rateTableID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.allocationID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.billingID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.billingOption>(sender, row, status == ProjectTaskStatus.Planned);
			PXUIFieldAttribute.SetEnabled<PMTask.completedPercent>(sender, row, row.CompletedPctMethod == PMCompletedPctMethod.Manual && status != ProjectTaskStatus.Planned && projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.taxCategoryID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.approverID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.startDate>(sender, row, (status == ProjectTaskStatus.Planned || status == ProjectTaskStatus.Active) && projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.endDate>(sender, row, status != ProjectTaskStatus.Completed && projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.plannedStartDate>(sender, row, status == ProjectTaskStatus.Planned);
			PXUIFieldAttribute.SetEnabled<PMTask.plannedEndDate>(sender, row, status == ProjectTaskStatus.Planned);
			PXUIFieldAttribute.SetEnabled<PMTask.isDefault>(sender, row, projectEditable);
		}

		protected virtual void PMTask_CompletedPercent_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null && row.CompletedPctMethod != PMCompletedPctMethod.Manual)
			{
				PXSelectBase<PMCostBudget> select = new PXSelectGroupBy<PMCostBudget,
						   Where<PMCostBudget.projectID, Equal<Required<PMTask.projectID>>,
						   And<PMCostBudget.projectTaskID, Equal<Required<PMTask.taskID>>,
						   And<PMCostBudget.isProduction, Equal<True>>>>,
						   Aggregate<
						   GroupBy<PMCostBudget.accountGroupID,
						   GroupBy<PMCostBudget.inventoryID,
						   GroupBy<PMCostBudget.uOM,
						   Sum<PMCostBudget.amount,
						   Sum<PMCostBudget.qty,
						   Sum<PMCostBudget.curyRevisedAmount,
						   Sum<PMCostBudget.revisedQty,
						   Sum<PMCostBudget.actualAmount,
						   Sum<PMCostBudget.actualQty>>>>>>>>>>>(this);

				PXResultset<PMCostBudget> ps = select.Select(row.ProjectID, row.TaskID);


				if (ps != null)
				{
					double percentSum = 0;
					Int32 recordCount = 0;
					decimal actualAmount = 0;
					decimal revisedAmount = 0;
					foreach (PMCostBudget item in ps)
					{

						if (row.CompletedPctMethod == PMCompletedPctMethod.ByQuantity && item.RevisedQty > 0)
						{
							recordCount ++;
							percentSum += Convert.ToDouble(100 * item.ActualQty / item.RevisedQty);
						}
						else if (row.CompletedPctMethod == PMCompletedPctMethod.ByAmount)
						{
							recordCount++;
							actualAmount += item.CuryActualAmount.GetValueOrDefault(0);
							revisedAmount += item.CuryRevisedAmount.GetValueOrDefault(0);
						}
					}
					if (row.CompletedPctMethod == PMCompletedPctMethod.ByAmount)
						e.ReturnValue = revisedAmount == 0 ? 0 : Convert.ToDecimal(100 * actualAmount / revisedAmount);
					else
						e.ReturnValue = Convert.ToDecimal(percentSum) == 0 ? 0 : Convert.ToDecimal(percentSum / recordCount);
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnValue, typeof(decimal?), false, false, 0, 2, 0, 0, nameof(PMTask.completedPercent), null, null, null, PXErrorLevel.Undefined, false, true, true, PXUIVisibility.Visible, null, null, null);
				}
			}
		}

		protected virtual void PMTask_IsActive_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null && e.NewValue != null && ((bool)e.NewValue) == true)
			{
				PMProject project = Project.Select();
				if (project != null)
				{
					if (project.IsActive == false)
					{
						sender.RaiseExceptionHandling<PMTask.status>(e.Row, e.NewValue, new PXSetPropertyException(Warnings.ProjectIsNotActive, PXErrorLevel.Warning));
					}
				}
			}
		}
		
		protected virtual void PMTask_Status_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row == null) return;

			SetFlagsFromStatus(row, row.Status);
		}

		protected virtual void _(Events.FieldUpdated<PMTask, PMTask.isDefault> e)
		{
			if (e.Row.IsDefault == true)
			{
				var select = new PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>>>(this);
				foreach (PMTask task in select.Select(e.Row.ProjectID))
				{
					if (task.IsDefault == true && task.TaskID != e.Row.TaskID)
					{
						Task.Cache.SetValue<PMTask.isDefault>(task, false);
						Task.Cache.SmartSetStatus(task, PXEntryStatus.Updated);
					}
				}
			}
		}


		protected virtual void PMTask_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row == null)
				return;

			if (row.IsActive == true && row.IsCancelled == false)
			{
				throw new PXException(Messages.OnlyPlannedCanbeDeleted);
			}

			//validate that all child records can be deleted:

			PMTran tran = PXSelect<PMTran, Where<PMTran.projectID, Equal<Required<PMTask.projectID>>, And<PMTran.taskID, Equal<Required<PMTask.taskID>>>>>.SelectWindowed(this, 0, 1, row.ProjectID, row.TaskID);
			if ( tran != null )
			{
				throw new PXException(Messages.HasTranData);
			}

			PMTimeActivity activity = PXSelect<PMTimeActivity, Where<PMTimeActivity.projectID, Equal<Required<PMTask.projectID>>, And<PMTimeActivity.projectTaskID, Equal<Required<PMTask.taskID>>>>>.SelectWindowed(this, 0, 1, row.ProjectID, row.TaskID);
			if (activity != null)
			{
				throw new PXException(Messages.HasActivityData);
			}

            EP.EPTimeCardItem timeCardItem = PXSelect<EP.EPTimeCardItem, Where<EP.EPTimeCardItem.projectID, Equal<Required<PMTask.projectID>>, And<EP.EPTimeCardItem.taskID, Equal<Required<PMTask.taskID>>>>>.SelectWindowed(this, 0, 1, row.ProjectID, row.TaskID);
            if (timeCardItem != null)
            {
                throw new PXException(Messages.HasTimeCardItemData);
            }
		}

		
		
		protected virtual void PMTask_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(this);
			if (row != null && project != null)
			{
				row.CustomerID = project.CustomerID;
				row.BillingID = project.BillingID;
				row.AllocationID = project.AllocationID;
				row.DefaultAccountID = project.DefaultAccountID;
				row.DefaultSubID = project.DefaultSubID;
			}
		}

		protected virtual void PMTask_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			PMTask oldRow = e.OldRow as PMTask;
			if (row == null || oldRow == null) return;

			if (row.IsActive == true && oldRow.IsActive != true)
			{
				ActivateTask(row);
			}

			if (row.IsCompleted == true && oldRow.IsCompleted != true)
			{
				CompleteTask(row);
			}
		}

		protected virtual void _(Events.RowPersisting<PMTask> e)
		{
			if (e.Operation != PXDBOperation.Delete && e.Row.Status == ProjectTaskStatus.Active && e.Row.IsActive != true)
				throw new PXException(Messages.TaskCannotBeSaved, e.Row.TaskCD);
		}

		protected virtual void PMTask_Status_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			
			string status = GetStatusFromFlags(row);
			List<KeyValuePair<string, string>> list = GetValidStatusTargets(row, status);

			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();

			foreach(var pair in list)
			{
				allowedValues.Add(pair.Key);
				allowedLabels.Add(Messages.GetLocal(pair.Value));
			}
			
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, nameof(PMTask.Status), false, 1, null, allowedValues.ToArray(), allowedLabels.ToArray(), true, allowedValues[0]);
		}
	

        protected virtual void PMTask_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            PMTask row = e.Row as PMTask;
            if (row != null)
            {
                sender.SetDefaultExt<PMTask.visibleInAP>(row);
                sender.SetDefaultExt<PMTask.visibleInAR>(row);
                sender.SetDefaultExt<PMTask.visibleInCA>(row);
                sender.SetDefaultExt<PMTask.visibleInCR>(row);				
				sender.SetDefaultExt<PMTask.visibleInTA>(row);
				sender.SetDefaultExt<PMTask.visibleInEA>(row);
				sender.SetDefaultExt<PMTask.visibleInGL>(row);
                sender.SetDefaultExt<PMTask.visibleInIN>(row);
                sender.SetDefaultExt<PMTask.visibleInPO>(row);
                sender.SetDefaultExt<PMTask.visibleInSO>(row);
                sender.SetDefaultExt<PMTask.customerID>(row);
                sender.SetDefaultExt<PMTask.locationID>(row);
                sender.SetDefaultExt<PMTask.rateTableID>(row);
            }
        }
		protected virtual void _(Events.FieldUpdated<PMRecurringItem, PMRecurringItem.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMRecurringItem.description>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.amount>(e.Row);
		}


		protected virtual void _(Events.FieldDefaulting<PMRecurringItem, PMRecurringItem.amount> e)
		{
			if (e.Row == null) return;
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.Row.InventoryID);
			if (item != null)
			{
				e.NewValue = item.BasePrice;
			}
		}

		protected virtual void _(Events.RowSelected<PMRecurringItem> e)
		{
			if (e.Row != null && Task.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<PMRecurringItem.included>(e.Cache, e.Row, Task.Current.IsActive != true);
				PXUIFieldAttribute.SetEnabled<PMRecurringItem.accountID>(e.Cache, e.Row, e.Row.AccountSource != PMAccountSource.None);
				PXUIFieldAttribute.SetEnabled<PMRecurringItem.subID>(e.Cache, e.Row, e.Row.AccountSource != PMAccountSource.None);
				PXUIFieldAttribute.SetEnabled<PMRecurringItem.subMask>(e.Cache, e.Row, e.Row.AccountSource != PMAccountSource.None);
            }
        }

		#endregion

		public virtual void SetFlagsFromStatus(PMTask task, string status)
		{
			if (string.IsNullOrEmpty(status))
				throw new ArgumentNullException();

			switch (status)
			{
				case ProjectTaskStatus.Planned:
					task.IsActive = false;
					task.IsCompleted = false;
					task.IsCancelled = false;
					break;
				case ProjectTaskStatus.Active:
					task.IsActive = true;
					task.IsCompleted = false;
					task.IsCancelled = false;
					break;
				case ProjectTaskStatus.Canceled:
					task.IsActive = false;
					task.IsCompleted = false;
					task.IsCancelled = true;
					break;
				case ProjectTaskStatus.Completed:
					task.IsActive = true;
					task.IsCompleted = true;
					task.IsCancelled = false;
					break;

				default:
					PXTrace.WriteError("Unknown status: " + status);
					break;
			}
		}

		public virtual string GetStatusFromFlags(PMTask task)
		{
			if (task == null)
				return ProjectTaskStatus.Planned;

			if (task.IsCancelled == true)
				return ProjectTaskStatus.Canceled;

			if (task.IsCompleted == true)
				return ProjectTaskStatus.Completed;

			if (task.IsActive == true)
				return ProjectTaskStatus.Active;

			return ProjectTaskStatus.Planned;
		}

		public virtual void SetFieldStateByStatus(PMTask task, string status)
		{
			
		}

		public virtual List<KeyValuePair<string, string>> GetValidStatusTargets(PMTask task, string status)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

			if (task == null || string.IsNullOrEmpty(status))
			{
				list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Planned, Messages.InPlanning));
				list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Active, Messages.Active));
				list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Completed, Messages.Completed));
				list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Canceled, Messages.Canceled));
			}
			else
			{
				if (status == ProjectTaskStatus.Canceled)
				{
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Canceled, Messages.Canceled));
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Active, Messages.Active));
				}
				else if (status == ProjectTaskStatus.Completed)
				{
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Completed, Messages.Completed));
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Active, Messages.Active));
				}
				else if (status == ProjectTaskStatus.Active)
				{
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Planned, Messages.InPlanning));
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Active, Messages.Active));
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Completed, Messages.Completed));
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Canceled, Messages.Canceled));
				}
				else if (status == ProjectTaskStatus.Planned)
				{
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Planned, Messages.InPlanning));
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Active, Messages.Active));
					list.Add(new KeyValuePair<string, string>(ProjectTaskStatus.Canceled, Messages.Canceled));
				}
			}

			return list;
		}
								
		public virtual void ActivateTask(PMTask task)
		{
			if (task.StartDate == null)
				task.StartDate = Accessinfo.BusinessDate;
		}
				
		public virtual void CompleteTask(PMTask task)
		{
			task.EndDate = Accessinfo.BusinessDate;
			task.CompletedPercent = Math.Max(100, task.CompletedPercent.GetValueOrDefault());
		}
		
	}
}
