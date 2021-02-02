using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class TimeEntry : PXGraph<TimeEntry, PMTimeActivity>
	{
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<PMTimeActivity.noteID>))]
		[PXUIField(DisplayName = "ID")]
		[PXDBGuid(true, IsKey = true)]
		protected virtual void PMTimeActivity_NoteID_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Track Time")]
		protected virtual void PMTimeActivity_TrackTime_CacheAttached(PXCache sender) { }
		
		[PXDBGuid]
		[PXUIField(DisplayName = "Employee")]
		[PXDefault(typeof(AccessInfo.userID))]
		[PXSubordinateAndWingmenOwnerSelector]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void PMTimeActivity_OwnerID_CacheAttached(PXCache cache) { }
				
		[PXSelector(
			typeof(Search<CRActivity.noteID,
				Where<CRActivity.noteID, NotEqual<Current<PMTimeActivity.noteID>>,
					And<CRActivity.ownerID, Equal<Current<PMTimeActivity.ownerID>>,
						And<CRActivity.classID, NotEqual<CRActivityClass.events>,
							And<Where<CRActivity.classID, Equal<CRActivityClass.task>, Or<CRActivity.classID, Equal<CRActivityClass.events>>>>>>>,
				OrderBy<Desc<CRActivity.createdDateTime>>>),
			typeof(CRActivity.subject),
			typeof(CRActivity.uistatus),
			DescriptionField = typeof(CRActivity.subject)
			)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_ParentTaskNoteID_CacheAttached(PXCache cache) { }

		[PXUIField(DisplayName = "Time Card Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void PMTimeActivity_TimeCardCD_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDefault(ActivityStatusListAttribute.Open)]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Approval Status")]
		public virtual void PMTimeActivity_ApprovalStatus_CacheAttached(PXCache cache) { }
				
		public PXSelect<PMTimeActivity> Items;
		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<PMTimeActivity, Where<PMTimeActivity.noteID, Equal<Current<PMTimeActivity.noteID>>>> ItemProperties;

		public PXSetup<EPSetup> Setup;

		public PXAction<PMTimeActivity> MarkAsCompleted;
		[PXUIField(DisplayName = EP.Messages.Complete)]
		[PXButton(Tooltip = EP.Messages.MarkAsCompletedTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75)] //Ctrl + K
		public virtual void markAsCompleted()
		{
			PMTimeActivity row = Items.Current;
			if (row == null) return;

			CompleteActivity(row);
		}

		public PXAction<PMTimeActivity> MarkAsOpen;
		[PXUIField(DisplayName = EP.Messages.Open)]
		[PXButton(Tooltip = EP.Messages.Open)]
		public virtual void markAsOpen()
		{
			PMTimeActivity row = Items.Current;
			if (row == null) return;

			OpenActivity(row);
		}

		protected EmployeeCostEngine costEngine;
		public EmployeeCostEngine CostEngine
		{
			get
			{
				if (costEngine == null)
				{
					costEngine = CreateEmployeeCostEngine();
				}

				return costEngine;
			}
		}

		protected virtual void _(Events.RowSelected<PMTimeActivity> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.Released == true)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.noteID>(e.Cache, e.Row, true);
			}
			else if (e.Row.ApprovalStatus == CR.ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(e.Cache, e.Row);
				PMProject project = (PMProject)PXSelectorAttribute.Select<PMTimeActivity.projectID>(e.Cache, e.Row);

				if (project != null)
					PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectTaskID>(e.Cache, e.Row, project.BaseType == PMProject.ProjectBaseType.Project);
				PXDBDateAndTimeAttribute.SetTimeEnabled<PMTimeActivity.date>(e.Cache, e.Row, true);
				PXDBDateAndTimeAttribute.SetDateEnabled<PMTimeActivity.date>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.approverID>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeCardCD>(e.Cache, e.Row, false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.noteID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.approvalStatus>(e.Cache, e.Row, true);
			}

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.employeeRate>(e.Cache, e.Row, false);
		}

		protected virtual void _(Events.RowDeleting<PMTimeActivity> e)
		{
			if (e.Row.ApprovalStatus == CR.ActivityStatusListAttribute.Approved || e.Row.Released == true)
			{
				Items.View.Ask((PXMessages.LocalizeFormatNoPrefix(EP.Messages.ActivityIs, e.Cache.GetValueExt<PMTimeActivity.approvalStatus>(e.Row))), MessageButtons.OK);
				e.Cancel = true;
			}
			else if (e.Row.TimeCardCD != null)
			{
				Items.View.Ask(EP.Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldSelecting<PMTimeActivity, PMTimeActivity.approvalStatus> e)
		{
			if (e.Row != null)
			{
				List<string> allowedValues = new List<string>();
				List<string> allowedLabels = new List<string>();

				if (e.Row.Released == true)
				{
					allowedValues.Add(ActivityStatusListAttribute.Released);
					allowedLabels.Add(Messages.GetLocal(EP.Messages.Released));
					
				}
				else
				{
					allowedValues.Add(ActivityStatusListAttribute.Open);
					allowedLabels.Add(Messages.GetLocal(EP.Messages.Open));
					allowedValues.Add(ActivityStatusListAttribute.Completed);
					allowedLabels.Add(Messages.GetLocal(EP.Messages.Completed));
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(PMTimeActivity.approvalStatus).Name, true, 1, null, allowedValues.ToArray(), allowedLabels.ToArray(), true, ActivityStatusListAttribute.Completed);
			}
		}

		protected virtual void PMTimeActivity_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;
			if (row == null)
				return;
			if (row.TimeCardCD != null)
			{
				Items.View.Ask(EP.Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
				e.Cancel = true;
			}
		}
				
		protected virtual void _(Events.FieldDefaulting<PMTimeActivity, PMTimeActivity.earningTypeID> e)
		{
			EPEmployee rowEmploye = PXSelect<EPEmployee>.Search<EPEmployee.userID>(this, e.Row.OwnerID);
			if (rowEmploye == null || e.Row.Date == null)
				return;

			if (CalendarHelper.IsWorkDay(this, rowEmploye.CalendarID, (DateTime)e.Row.Date))
			{
				e.NewValue = Setup.Current.RegularHoursType;
			}
			else
			{
				e.NewValue = Setup.Current.HolidaysType;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMTimeActivity, PMTimeActivity.projectID> e)
		{
			if (e.Row == null)
				return;

			EPEarningType earningType = PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Required<EPEarningType.typeCD>>>>.Select(this, e.Row.EarningTypeID);
			if (earningType != null && earningType.ProjectID != null && e.Row.ProjectID == null &&
				PXSelectorAttribute.Select(e.Cache, e.Row, e.Cache.GetField(typeof(PMTimeActivity.projectID)), earningType.ProjectID) != null //available in selector
				)
			{
				e.NewValue = earningType.ProjectID;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMTimeActivity, PMTimeActivity.projectTaskID> e)
		{
			if (e.Row == null)
				return;
						
			if (e.Row.ParentTaskNoteID != null)
			{
				EPActivityApprove rowParentTask = PXSelect<EPActivityApprove>.Search<EPActivityApprove.noteID>(this, e.Row.ParentTaskNoteID);
				if (rowParentTask != null && rowParentTask.ProjectID == e.Row.ProjectID)
				{
					e.NewValue = rowParentTask.ProjectTaskID;
					e.Cancel = true;
				}
			}

			EPEarningType earningRow = (EPEarningType)PXSelectorAttribute.Select<EPActivityApprove.earningTypeID>(e.Cache, e.Row);
			if (e.NewValue == null && earningRow != null && earningRow.ProjectID == e.Row.ProjectID)
			{
				PMTask defTask = PXSelectorAttribute.Select(e.Cache, e.Row, e.Cache.GetField(typeof(EPTimeCardSummary.projectTaskID)), earningRow.TaskID) as PMTask;
				if (defTask != null && defTask.VisibleInTA == true)
				{
					e.NewValue = earningRow.TaskID;
					e.Cancel = true;
				}
			}


		}

		protected virtual void _(Events.FieldVerifying<PMTimeActivity, PMTimeActivity.projectID> e)
		{
			if (e.NewValue != null && e.NewValue is int)
			{
				PMProject proj = PXSelect<PMProject>.Search<PMProject.contractID>(this, e.NewValue);
				if (proj != null)
				{
					if (proj.IsCompleted == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsCompleted);
						ex.ErrorValue = proj.ContractCD;
						throw ex;
					}
					if (proj.IsCancelled == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsCanceled);
						ex.ErrorValue = proj.ContractCD; ;
						throw ex;
					}
					if (proj.Status == CT.Contract.status.Expired)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsSuspended);
						ex.ErrorValue = proj.ContractCD; ;
						throw ex;
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMTimeActivity, PMTimeActivity.projectTaskID> e)
		{
			if (e.NewValue != null && e.NewValue is int)
			{
				PMTask task = PXSelect<PMTask>.Search<PMTask.taskID>(this, e.NewValue);
				if (task != null)
				{
					if (task.IsCompleted == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectTaskIsCompleted);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}
					if (task.IsCancelled == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectTaskIsCanceled);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.ownerID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.workCodeID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.projectID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.unionID>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.certifiedJob>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}
		
		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.isBillable> e)
		{
			e.Row.TimeBillable = GetTimeBillable(e.Row, null);
		}

		protected virtual void _(Events.RowUpdated<PMTimeActivity> e)
		{
			if (e.Row.Date.GetValueOrDefault() != e.OldRow.Date.GetValueOrDefault()
					|| e.Row.EarningTypeID != e.OldRow.EarningTypeID
					|| e.Row.ProjectID.GetValueOrDefault() != e.OldRow.ProjectID.GetValueOrDefault()
					|| e.Row.ProjectTaskID.GetValueOrDefault() != e.OldRow.ProjectTaskID.GetValueOrDefault()
					|| e.Row.CostCodeID != e.OldRow.CostCodeID
					|| e.Row.UnionID != e.OldRow.UnionID
					|| e.Row.LabourItemID != e.OldRow.LabourItemID
					|| e.Row.CertifiedJob.GetValueOrDefault() != e.OldRow.CertifiedJob.GetValueOrDefault()
					|| e.Row.OwnerID.GetValueOrDefault() != e.OldRow.OwnerID.GetValueOrDefault()
					)
			{
				RecalculateEmployeeCost(e.Row);
			}
		}

		protected virtual void _(Events.RowInserted<PMTimeActivity> e)
		{
			RecalculateEmployeeCost(e.Row);
		}

		public virtual void RecalculateEmployeeCost(PMTimeActivity row)
		{
			if (row != null && row.Date != null)
			{
				int? employeeID = null;
				if (row.OwnerID != null)
				{
					EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, row.OwnerID);

					if (employee != null)
						employeeID = employee.BAccountID;
				}
				var cost = CostEngine.CalculateEmployeeCost(null, row.EarningTypeID, row.LabourItemID, row.ProjectID, row.ProjectTaskID, row.CertifiedJob, row.UnionID, employeeID, row.Date.Value);
				row.EmployeeRate = cost?.Rate;
			}
		}

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}


		protected virtual int? GetTimeBillable(PMTimeActivity row, int? OldTimeSpent)
		{
			if (row.TimeCardCD == null && row.Billed != true)
			{
				if (row.IsBillable != true)
					return 0;
				else if ((OldTimeSpent ?? 0) == 0 || OldTimeSpent == row.TimeBillable)
					return row.TimeSpent;
				else
					return row.TimeBillable;
			}
			else
				return row.TimeBillable;
		}

		private void CompleteActivity(PMTimeActivity activity)
		{			
			if (activity != null)
			{
				activity.ApprovalStatus = ActivityStatusListAttribute.Completed;
				Items.Cache.Update(activity);
			}

			Actions.PressSave();
		}

		private void OpenActivity(PMTimeActivity activity)
		{
			if (activity != null)
			{
				activity.ApprovalStatus = ActivityStatusListAttribute.Open;
				Items.Cache.Update(activity);
			}

			Actions.PressSave();
		}

		public virtual EmployeeCostEngine CreateEmployeeCostEngine()
		{
			return new EmployeeCostEngine(this);
		}
	}
}
