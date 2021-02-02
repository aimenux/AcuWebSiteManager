using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.SM;
using PX.Data;
using PX.TM;
using OwnedFilter = PX.Objects.CR.OwnedFilter;


namespace PX.Objects.EP
{
	public class EmployeeActivitiesEntry : PXGraph<EmployeeActivitiesEntry>
	{
		#region Selects
		[PXHidden]
		public PXSelect<ContractEx> dummyContract;
		[PXHidden]
		public PXSelect<CRActivity> dummy;
		public PXFilter<PMTimeActivityFilter> Filter;

		public PXSelectJoin<
				EPActivityApprove,
				LeftJoin<CRActivityLink,
					On<CRActivityLink.noteID, Equal<EPActivityApprove.refNoteID>>,
				LeftJoin<EPEarningType,
					On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>,
				LeftJoin<CRCase,
					On<CRCase.noteID, Equal<CRActivityLink.refNoteID>>,
				LeftJoin<ContractEx,
					On<CRCase.contractID, Equal<ContractEx.contractID>>>>>>,
				Where<EPActivityApprove.ownerID, Equal<Current<PMTimeActivityFilter.ownerID>>,
					And<EPActivityApprove.trackTime, Equal<True>>>, 
				OrderBy<Asc<EPActivityApprove.date>>>
			Activity;

		public PXSetupOptional<EPSetup> EPsetingst;
		[PXHidden]
		public PXFilter<EP.CRActivityMaint.EPTempData> TempData;

		public PXSelect<CRActivity, 
			Where<CRActivity.noteID, Equal<Current<EPActivityApprove.refNoteID>>,
				And<CRActivity.isLocked, Equal<False>>>> Parent;
		#endregion

		public EmployeeActivitiesEntry()
		{
			//NO CRM Mode
			PXUIFieldAttribute.SetVisible<EPActivityApprove.contractID>(Activity.Cache, null, !PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
			PXUIFieldAttribute.SetVisible<ContractEx.contractCD>(dummyContract.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.customerModule>());

			if (!PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.timeSpent).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.timeBillable).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.isBillable).Name, false);

				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.regularOvertime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.regularTime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.regularTotal>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.billableOvertime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.billableTime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.billableTotal>(Filter.Cache, null, false);
			}
			bool contractInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>();
			if (!contractInstalled)
			{
				PXUIFieldAttribute.SetVisible<Contract.contractCD>(Caches[typeof(Contract)], null, false);
			}
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.projectTaskID).Name, false);
				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.projectTaskID>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMTimeActivityFilter.includeReject>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityApprove.approvalStatus>(Activity.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityApprove.approverID>(Activity.Cache, null, false);
			}

			
			EPActivityType activityType = (EPActivityType) PXSelect<EPActivityType>.Search<EPActivityType.type>(this, EPsetingst.Current.DefaultActivityType);
			if (activityType == null || activityType.RequireTimeByDefault != true)
			{
				throw new PXSetupNotEnteredException(Messages.SetupNotEntered, typeof(EPActivityType), PXMessages.LocalizeNoPrefix(Messages.ActivityType));
			}
			this.FieldUpdating.AddHandler(typeof(EPActivityApprove), "Date_Date", StartDateFieldUpdating);

			EPEmployee employeeByUserID = PXSelect<EPEmployee, Where<EP.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (employeeByUserID == null && !ProxyIsActive)
			{
				if (IsExport || IsImport)
					throw new PXException(Messages.MustBeEmployee);
				else
					Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", Messages.MustBeEmployee, "error"));
			}
        }

		#region Actions

		public PXSave<PMTimeActivityFilter> Save;
		public PXCancel<PMTimeActivityFilter> Cancel;
		public PXAction<PMTimeActivityFilter> View;
		[PXUIField(DisplayName = Messages.View)]
		[PXButton()]
		public virtual IEnumerable view(PXAdapter adapter)
		{
			var row = Activity.Current;
			if (row != null)
			{
				PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<PMTimeActivityFilter> viewCase;
		[PXUIField(Visible = false)]
		[PXButton()]
		public virtual IEnumerable ViewCase(PXAdapter adapter)
		{
			var row = Activity.Current;

			var apRow = (EPActivityApprove)row;

			if (row != null && row.RefNoteID != null)
			{
				CRCase caseRow = PXSelectJoin<CRCase,
					InnerJoin<CRActivityLink,
						On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
					Where<CRActivityLink.noteID, Equal<Required<EPActivityApprove.refNoteID>>>>.Select(this, apRow.RefNoteID);

				if (caseRow != null)
					PXRedirectHelper.TryRedirect(this, caseRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<PMTimeActivityFilter> viewContract;
		[PXUIField(Visible = false)]
		[PXButton()]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			var row = Activity.Current;

			var apRow = (EPActivityApprove)row;

			if (apRow != null)
			{
				Contract contractRow = null;
				
				contractRow = PXSelectJoin<Contract,
					InnerJoin<CRCase,
						On<CRCase.contractID, Equal<Contract.contractID>>,
					InnerJoin<CRActivityLink,
						On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>>,
					Where<CRActivityLink.noteID, Equal<Required<EPActivityApprove.refNoteID>>>>.Select(this, apRow.RefNoteID);
				
				if (contractRow != null)
					PXRedirectHelper.TryRedirect(this, contractRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		#endregion

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
		public virtual EmployeeCostEngine CreateEmployeeCostEngine()
		{
			return new EmployeeCostEngine(this);
		}

		#region Event handlers

		protected virtual IEnumerable activity()
		{

			List<object> args = new List<object>();
			PMTimeActivityFilter filterRow = Filter.Current;
			if (filterRow == null)
				return null;

			BqlCommand cmd;
			cmd = BqlCommand.CreateInstance(typeof(Select2<
				EPActivityApprove,
				LeftJoin<EPEarningType, 
					On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>,
				LeftJoin<CRActivityLink, 
					On<CRActivityLink.noteID, Equal<EPActivityApprove.refNoteID>>,
				LeftJoin<CRCase, 
					On<CRCase.noteID, Equal<CRActivityLink.refNoteID>>,
				LeftJoin<ContractEx, 
					On<CRCase.contractID, Equal<ContractEx.contractID>>>>>>,
				Where
					<EPActivityApprove.ownerID, Equal<Current<PMTimeActivityFilter.ownerID>>,
					And<EPActivityApprove.trackTime, Equal<True>,
                    And<EPActivityApprove.isCorrected, Equal<False>>>>,
				OrderBy<Desc<EPActivityApprove.date>>>));


			if (filterRow.ProjectID != null)
				cmd = cmd.WhereAnd<Where<EPActivityApprove.projectID, Equal<Current<PMTimeActivityFilter.projectID>>>>();

			if (filterRow.ProjectTaskID != null)
				cmd = cmd.WhereAnd<Where<EPActivityApprove.projectTaskID, Equal<Current<PMTimeActivityFilter.projectTaskID>>>>();

			if (filterRow.FromWeek != null || filterRow.TillWeek != null)
			{
				List<Type> cmdList = new List<Type>();

				if (filterRow.IncludeReject == true)
				{
					cmdList.Add(typeof(Where<,,>));
					cmdList.Add(typeof(EPActivityApprove.approvalStatus));
					cmdList.Add(typeof(Equal<CR.ActivityStatusListAttribute.rejected>));
					cmdList.Add(typeof(Or<>));
				}

				if (filterRow.FromWeek != null)
				{
					if (filterRow.TillWeek != null)
						cmdList.Add(typeof(Where<,,>));
					else
						cmdList.Add(typeof(Where<,>));
					cmdList.Add(typeof(EPActivityApprove.weekID));
					cmdList.Add(typeof(GreaterEqual<Required<PMTimeActivityFilter.fromWeek>>));
					args.Add(filterRow.FromWeek);
					if (filterRow.TillWeek != null)
						cmdList.Add(typeof(And<>));
				}

				if (filterRow.TillWeek != null)
				{
					cmdList.Add(typeof(Where<EPActivityApprove.weekID, LessEqual<Required<PMTimeActivityFilter.tillWeek>>>));
					args.Add(filterRow.TillWeek);
				}

				cmd = cmd.WhereAnd(BqlCommand.Compose(cmdList.ToArray()));
			}

			PXView view = new PXView(this, false, cmd);
			return view.SelectMultiBound(new object[] { Filter.Current }, args.ToArray());
		}

		[PXUIEnabled(typeof(EPActivityApprove.isBillable))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPActivityApprove_TimeBillable_CacheAttached(PXCache sender) { }

		[PXDefault(typeof(Search<PMProject.contractID, Where<PMProject.nonProject, Equal<True>>>))]
		[EPProject(typeof(EPActivityApprove.ownerID), FieldClass = ProjectAttribute.DimensionName, BqlField = typeof(PMTimeActivity.projectID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void EPActivityApprove_ProjectID_CacheAttached(PXCache sender) { }

		protected virtual void PMTimeActivityFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXDBDateAndTimeAttribute.SetTimeVisible<EPActivityApprove.date>(Activity.Cache, null, EPsetingst.Current.RequireTimes == true);
			
			PMTimeActivityFilter row = (PMTimeActivityFilter)e.Row;
			if (row != null)
			{
				row.BillableTime = 0;
				row.BillableOvertime = 0;
				row.BillableTotal = 0;
				row.RegularTime = 0;
				row.RegularOvertime = 0;
				row.RegularTotal = 0;
				foreach (PXResult<EPActivityApprove, EPEarningType> item in Activity.Select())
				{
					EPActivityApprove rowActivity = (EPActivityApprove)item;
					EPEarningType rowEarningType = (EPEarningType)item;

					if (rowEarningType.IsOvertime == true)
					{
						row.RegularOvertime += rowActivity.TimeSpent.GetValueOrDefault(0);
						row.BillableOvertime += rowActivity.TimeBillable.GetValueOrDefault(0);
					}
					else
					{
						row.RegularTime += rowActivity.TimeSpent.GetValueOrDefault(0);
						row.BillableTime += rowActivity.TimeBillable.GetValueOrDefault(0);
					}

					row.BillableTotal = row.BillableTime + row.BillableOvertime;
					row.RegularTotal = row.RegularTime + row.RegularOvertime;
				}
			}
		}

		protected virtual void PMTimeActivityFilter_FromWeek_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			try
			{
				e.NewValue = PXWeekSelector2Attribute.GetWeekID(this, Accessinfo.BusinessDate.GetValueOrDefault(DateTime.Now));
			}
			catch (PXException exception)
			{
				sender.RaiseExceptionHandling<PMTimeActivityFilter.fromWeek>(e.Row, null, exception);
			}

		}

		protected virtual void PMTimeActivityFilter_TillWeek_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			try
			{
				e.NewValue = PXWeekSelector2Attribute.GetWeekID(this, Accessinfo.BusinessDate.GetValueOrDefault(DateTime.Now));
			}
			catch (PXException exception)
			{
				sender.RaiseExceptionHandling<PMTimeActivityFilter.fromWeek>(e.Row, null, exception);
			}
		}

		protected virtual void PMTimeActivityFilter_FromWeek_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTimeActivityFilter row = (PMTimeActivityFilter) e.Row;
			if (row != null && e.ExternalCall && row.FromWeek > row.TillWeek)
				row.TillWeek = row.FromWeek;
		}

		protected virtual void PMTimeActivityFilter_TillWeek_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTimeActivityFilter row = (PMTimeActivityFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromWeek > row.TillWeek)
				row.FromWeek = row.TillWeek;
		}

		protected virtual void PMTimeActivityFilter_ProjectID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMTimeActivityFilter row = (PMTimeActivityFilter)e.Row;

			if (row == null) return;
			
			PMProject proj = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);

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
					ex.ErrorValue = proj.ContractCD;
					throw ex;
				}
				if (proj.Status == Contract.status.Expired)
				{
					var ex = new PXSetPropertyException(PM.Messages.ProjectIsSuspended);
					ex.ErrorValue = proj.ContractCD;
					throw ex;
				}

			}
			
		}

		protected virtual void PMTimeActivityFilter_ProjectTaskID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMTimeActivityFilter row = (PMTimeActivityFilter)e.Row;

			if (row == null) return;

			if (e.NewValue != null && e.NewValue is int)
			{
				PMTask task = PXSelect<PMTask>.Search<PMTask.taskID>(sender.Graph, e.NewValue);
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

		protected virtual void PMTimeActivityFilter_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (Activity.Cache.IsDirty && Filter.View.Ask(ActionsMessages.ConfirmationMsg, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				e.Cancel = true;
			}
			else
			{
				Activity.Cache.Clear();
			}

		}

		protected virtual void StartDateFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			PMTimeActivityFilter rowFilter = Filter.Current;
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (rowFilter == null || e.NewValue == null)
				return;

			DateTime? newValue = null;
			DateTime valFromString;
			if (e.NewValue is string &&
				DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out valFromString))
			{
				newValue = valFromString;
			}
			if (e.NewValue is DateTime)
				newValue = (DateTime)e.NewValue;

			if (newValue != null)
			{
				int weekId = PXWeekSelector2Attribute.GetWeekID(this, newValue.Value.Date);

				if (weekId < rowFilter.FromWeek || weekId > rowFilter.TillWeek)
				{
					sender.RaiseExceptionHandling<EPActivityApprove.date>(row, e.NewValue,
						new PXSetPropertyException(Messages.StartDateOutOfRange));
					e.Cancel = true;
				}
			}
		}

		protected virtual void EPActivityApprove_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			PMTimeActivityFilter rowFilter = (PMTimeActivityFilter)Filter.Current;

			if (row == null || rowFilter == null)
				return;

			if (row.Released == true)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				PXDBDateAndTimeAttribute.SetTimeEnabled<EPActivityApprove.date>(sender, row, false);
				PXDBDateAndTimeAttribute.SetDateEnabled<EPActivityApprove.date>(sender, row, false);
			}
			else if (row.ApprovalStatus == CR.ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.timeBillable>(sender, row);
				PMProject project = (PMProject) PXSelectorAttribute.Select<EPActivityApprove.projectID>(sender, row);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.projectID>(sender, row, rowFilter.ProjectID == null);
				if (project != null)
					PXUIFieldAttribute.SetEnabled<EPActivityApprove.projectTaskID>(sender, row,
					                                                               rowFilter.ProjectTaskID == null &&
					                                                               project.BaseType ==
																				   CT.CTPRType.Project);
				else
					PXUIFieldAttribute.SetEnabled<EPActivityApprove.projectTaskID>(sender, row, rowFilter.ProjectTaskID == null);
				PXDBDateAndTimeAttribute.SetTimeEnabled<EPActivityApprove.date>(sender, row, true);
				PXDBDateAndTimeAttribute.SetDateEnabled<EPActivityApprove.date>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.approvalStatus>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.approverID>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.timeCardCD>(sender, row, false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				PXDBDateAndTimeAttribute.SetTimeEnabled<EPActivityApprove.date>(sender, row, false);
				PXDBDateAndTimeAttribute.SetDateEnabled<EPActivityApprove.date>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.hold>(sender, row, true);
			}

			PXUIFieldAttribute.SetEnabled<EPActivityApprove.approvalStatus>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivityApprove.employeeRate>(sender, row, false);

			if (row.ApprovalStatus == CR.ActivityStatusListAttribute.Rejected)
				sender.RaiseExceptionHandling<EPActivityApprove.hold>(row, null, new PXSetPropertyException(Messages.Rejected, PXErrorLevel.RowWarning));
		}

		protected virtual void EPActivityApprove_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;

			if (row.ApprovalStatus == CR.ActivityStatusListAttribute.Approved || row.Released == true)
			{
				Filter.View.Ask((PXMessages.LocalizeFormatNoPrefix(Messages.ActivityIs, sender.GetValueExt<EPActivityApprove.approvalStatus>(row))), MessageButtons.OK);
				e.Cancel = true;
			}
			else if (row.TimeCardCD != null)
			{
				Filter.View.Ask(Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
				e.Cancel = true;
			}
		}

        protected virtual void EPActivityApprove_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            EPActivityApprove row = (EPActivityApprove)e.Row;
            if (row == null)
                return;

            if (this.IsMobile)
            {
                this.Persist();
            }
        }

        protected virtual void EPActivityApprove_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;
			if (row.TimeCardCD != null)
			{
				Filter.View.Ask(Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowUpdated<EPActivityApprove> e)
		{
			if (e.Row.Date.GetValueOrDefault() != e.OldRow.Date.GetValueOrDefault()
					|| e.Row.EarningTypeID != e.OldRow.EarningTypeID
					|| e.Row.ProjectID.GetValueOrDefault() != e.OldRow.ProjectID.GetValueOrDefault()
					|| e.Row.ProjectTaskID.GetValueOrDefault() != e.OldRow.ProjectTaskID.GetValueOrDefault()
					|| e.Row.CostCodeID != e.OldRow.CostCodeID
					|| e.Row.UnionID != e.OldRow.UnionID
					|| e.Row.LabourItemID != e.OldRow.LabourItemID
					|| e.Row.CertifiedJob.GetValueOrDefault() != e.OldRow.CertifiedJob.GetValueOrDefault()
					|| e.Row.OwnerID.GetValueOrDefault() != e.OldRow.OwnerID.GetValueOrDefault())
			{
				EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, e.Row.OwnerID);
				if (employee != null)
				{
					var cost = CostEngine.CalculateEmployeeCost(null, e.Row.EarningTypeID, e.Row.LabourItemID, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.CertifiedJob, e.Row.UnionID, employee.BAccountID, e.Row.Date.Value);
					e.Row.EmployeeRate = cost?.Rate;
				}
			}

			UpdateReportedInTimeZoneIDIfNeeded(e.Cache, e.Row, e.OldRow.Date, e.Row.Date);
		}

		protected virtual void _(Events.RowInserted<EPActivityApprove> e)
		{
			UpdateReportedInTimeZoneIDIfNeeded(e.Cache, e.Row, null, e.Row.Date);
		}

		protected virtual void EPActivityApprove_ProjectID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null) return;

			if (e.NewValue != null && e.NewValue is int)
			{
				PMProject proj = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
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
						ex.ErrorValue = proj.ContractCD;
						throw ex;
					}
					if (proj.Status == Contract.status.Expired)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsSuspended);
						ex.ErrorValue = proj.ContractCD;
						throw ex;
					}
				}
			}
		}

		protected virtual void EPActivityApprove_ProjectTaskID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null) return;

			if (e.NewValue != null && e.NewValue is int)
			{
				PMTask task = PXSelect<PMTask>.Search<PMTask.taskID>(sender.Graph, e.NewValue);
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
		
		protected virtual void EPActivityApprove_EarningTypeID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null || EPsetingst.Current == null)
				return;

			EPEmployee rowEmploye = PXSelect<EPEmployee>.Search<EPEmployee.userID>(this, Filter.Current.OwnerID);
			if (rowEmploye == null || row.Date == null)
				return;

			if (CalendarHelper.IsWorkDay(this, rowEmploye.CalendarID, (DateTime)row.Date))
			{
				e.NewValue = EPsetingst.Current.RegularHoursType;
			}
			else
			{
				e.NewValue = EPsetingst.Current.HolidaysType;
			}
		}

		protected virtual void EPActivityApprove_ProjectID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;

			if (Filter.Current.ProjectID != null)
			{
				e.NewValue = Filter.Current.ProjectID;
				e.Cancel = true;
			}
			else
			{
				EPEarningType earningType = PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.EarningTypeID);
				if (earningType != null && earningType.ProjectID != null && 
					PXSelectorAttribute.Select(cache, e.Row, cache.GetField(typeof(EPActivityApprove.projectID)), earningType.ProjectID) != null //available in selector
					)
				{
					e.NewValue = earningType.ProjectID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void EPActivityApprove_ProjectTaskID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			
			if (row == null)
				return;

			if (Filter.Current.ProjectTaskID != null)
			{
				e.NewValue = Filter.Current.ProjectTaskID;
				e.Cancel = true;
				return;
			}

			if (row.ParentTaskNoteID != null)
			{
				EPActivityApprove rowParentTask = PXSelect<EPActivityApprove>.Search<EPActivityApprove.noteID>(this, row.ParentTaskNoteID);
				if (rowParentTask != null && rowParentTask.ProjectID == row.ProjectID)
				{
					e.NewValue = rowParentTask.ProjectTaskID;
					e.Cancel = true;
				}
			}

			EPEarningType earningRow = (EPEarningType)PXSelectorAttribute.Select<EPActivityApprove.earningTypeID>(cache, row);
			if (e.NewValue == null && earningRow != null && earningRow.ProjectID == row.ProjectID)
			{
				PMTask defTask = PXSelectorAttribute.Select(cache, e.Row, cache.GetField(typeof(EPTimeCardSummary.projectTaskID)), earningRow.TaskID) as PMTask;
				if (defTask != null && defTask.VisibleInTA == true)
				{
					e.NewValue = earningRow.TaskID;
					e.Cancel = true;
				}
			}

			
		}

		protected virtual void EPActivityApprove_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;

			if (row.ApprovalStatus == ActivityStatusListAttribute.Approved)
				cache.RaiseExceptionHandling<EPActivityApprove.hold>(row, null, new PXSetPropertyException(Messages.Approved, PXErrorLevel.RowWarning));
			if (row.RefNoteID != null)
			{
				CRActivity parent = (CRActivity) Parent.View.SelectSingleBound(new[] {row});
				if (parent != null)
				{
					parent.UIStatus = row.Hold == true ? ActivityStatusListAttribute.Open : ActivityStatusListAttribute.Completed;
					Parent.Update(parent);
				}
			}

		}

		protected virtual void EPActivityApprove_Hold_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row != null)
				e.ReturnValue = row.ApprovalStatus == ActivityStatusListAttribute.Open;
		}

		protected virtual int? GetTimeBillable(EPActivityApprove row, int? OldTimeSpent)
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

		protected virtual void EPActivityApprove_IsBillable_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;
			else
				row.TimeBillable = GetTimeBillable(row, null);
		}

		protected virtual void EPActivityApprove_TimeSpent_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;
			else
				row.TimeBillable = GetTimeBillable(row, (int?)e.OldValue);
		}

		protected virtual void EPActivityApprove_Type_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove) e.Row;
			if (row == null)
			{
				e.NewValue = null;
			}
			else
			{
				e.NewValue = EPsetingst.Current.DefaultActivityType;
				e.Cancel = true;
			}
		}

		protected virtual void EPActivityApprove_Date_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if(this.IsImport)
			{
				return;
			}

			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
			{
				e.NewValue = null;
			}
			else
			{
				e.NewValue = CRActivityMaint.GetNextActivityStartDate<EPActivityApprove>(this, Activity.Select(), row, Filter.Current.FromWeek, Filter.Current.TillWeek, TempData.Cache, typeof(EP.CRActivityMaint.EPTempData.lastEnteredDate));
			}
		}

		protected virtual void EPActivityApprove_Date_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;

			if (row == null)
				return;

			if (row.Date != null)
				TempData.Cache.SetValue<CRActivityMaint.EPTempData.lastEnteredDate>(TempData.Current, row.Date);

			UpdateReportedInTimeZoneIDIfNeeded(cache, row, (DateTime?) e.OldValue, row.Date);
		}

		protected virtual void _(Events.FieldUpdated<EPActivityApprove, EPActivityApprove.costCodeID> e)
		{
			e.Cache.SetDefaultExt<EPActivityApprove.workCodeID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<EPActivityApprove, EPActivityApprove.projectID> e)
		{
			e.Cache.SetDefaultExt<EPActivityApprove.unionID>(e.Row);
			e.Cache.SetDefaultExt<EPActivityApprove.certifiedJob>(e.Row);
			e.Cache.SetDefaultExt<EPActivityApprove.labourItemID>(e.Row);
			e.Cache.SetDefaultExt<EPActivityApprove.employeeRate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<EPActivityApprove, EPActivityApprove.hold> e)
		{
			e.Cache.SetDefaultExt<EPActivityApprove.approverID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<EPActivityApprove, EPActivityApprove.earningTypeID> e)
		{
			if (e.Row.ProjectID == null || ProjectDefaultAttribute.IsNonProject(e.Row.ProjectID))
				e.Cache.SetDefaultExt<EPActivityApprove.projectID>(e.Row);
		}

		#endregion


		#region DAC Overrides

		#region ProjectTaskID
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<EPActivityApprove.projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[EPTimecardProjectTask(typeof(EPActivityApprove.projectID), GL.BatchModule.TA, DisplayName = "Project Task", BqlField = typeof(PMTimeActivity.projectTaskID))]
		[PXFormula(typeof(Default<EPActivityApprove.parentTaskNoteID, EPActivityApprove.earningTypeID>))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void EPActivityApprove_ProjectTaskID_CacheAttached(PXCache cache) { }
		#endregion
		
		#region OwnerID
		[PXDefault(typeof(PMTimeActivityFilter.ownerID))]
		[PXDBGuid(BqlField = typeof(PMTimeActivity.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void EPActivityApprove_OwnerID_CacheAttached(PXCache cache) { }
		#endregion

		#region Subject
		[PXDBString(255, InputMask = "", IsUnicode = true, BqlField = typeof(PMTimeActivity.summary))]
		[PXDefault]
		[PXFormula(typeof(
			Switch<
				Case<Where<Current<EPActivityApprove.summary>, IsNotNull>, Current<EPActivityApprove.summary>, 
				Case<Where<EPActivityApprove.parentTaskNoteID, IsNotNull>, Selector<EPActivityApprove.parentTaskNoteID, EPActivityApprove.summary>, 
				Case<Where<EPActivityApprove.projectTaskID, IsNotNull>, Selector<EPActivityApprove.projectTaskID, PMTask.description>>>>>
			))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void EPActivityApprove_Summary_CacheAttached(PXCache cache) { }
		#endregion

		#region ParentTaskNoteID
		[PXSelector(
			typeof(Search<CRActivity.noteID,
				Where<CRActivity.noteID, NotEqual<Current<EPActivityApprove.noteID>>,
					And<CRActivity.ownerID, Equal<Current<EPActivityApprove.ownerID>>,
						And<CRActivity.classID, NotEqual<CRActivityClass.events>,
							And<Where<CRActivity.classID, Equal<CRActivityClass.task>, Or<CRActivity.classID, Equal<CRActivityClass.events>>>>>>>,
				OrderBy<Desc<CRActivity.createdDateTime>>>),
			typeof(CRActivity.subject),
			typeof(CRActivity.uistatus),
			DescriptionField = typeof(CRActivity.subject)
			)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPActivityApprove_ParentTaskNoteID_CacheAttached(PXCache cache) { }
		#endregion

		#region TimeCardCD
		[PXUIField(DisplayName = "Time Card Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void EPActivityApprove_TimeCardCD_CacheAttached(PXCache cache) { }
		#endregion
		
		#region Contract_ContractCD
		[PXDimensionSelector(ContractAttribute.DimensionName,
			typeof(Search2<Contract.contractCD, InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>, LeftJoin<AR.Customer, On<AR.Customer.bAccountID, Equal<Contract.customerID>>>>
			, Where<ContractExEx.baseType, Equal<CTPRType.contract>>>),
			typeof(Contract.contractCD),
			typeof(Contract.contractCD), typeof(Contract.customerID), typeof(AR.Customer.acctName), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), DescriptionField = typeof(Contract.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFieldDescription]
		public virtual void ContractEx_ContractCD_CacheAttached(PXCache cache) { }
		#endregion

		#endregion

		#region Filter
		[Serializable]
		public class PMTimeActivityFilter : OwnedFilter
		{
			private int? _fromWeek;

			#region OwnerID
			public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
			[PXDBGuid]
			[PXUIField(DisplayName = "Employee")]
			[PXDefault(typeof(AccessInfo.userID))]
			[PXSubordinateAndWingmenOwnerSelector]
			public override Guid? OwnerID { set; get; }
			#endregion

			#region FromWeek
			public abstract class fromWeek : PX.Data.BQL.BqlInt.Field<fromWeek> { }

			[PXDBInt]
			[PXWeekSelector2(DescriptionField = typeof (EPWeekRaw.shortDescription))]
			[PXUIField(DisplayName = "From Week", Visibility = PXUIVisibility.Visible)]
			public virtual int? FromWeek
			{
				set { _fromWeek = value; }
				get { return _fromWeek; }
			}

			#endregion

			#region TillWeek
			public abstract class tillWeek : PX.Data.BQL.BqlInt.Field<tillWeek> { }
			[PXDBInt]
			[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
			[PXUIField(DisplayName = "Until Week", Visibility = PXUIVisibility.Visible)]
			public virtual int? TillWeek { set; get; }
			#endregion

			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			[EPProject(typeof(ownerID), DisplayName = PM.Messages.Project)]
			public virtual Int32? ProjectID { get; set; }
			#endregion

			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

			[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[EPTimecardProjectTask(typeof(projectID), GL.BatchModule.TA, DisplayName = "Project Task", AllowNull=true)]
			public virtual Int32? ProjectTaskID { set; get; }
			#endregion

			#region IncludeReject
			public abstract class includeReject : PX.Data.BQL.BqlBool.Field<includeReject> { }
			[PXBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Include All Rejected")]
			public virtual bool? IncludeReject { set; get; }
			#endregion

			#region Total
			#region Regular
			public abstract class regularTime : PX.Data.BQL.BqlInt.Field<regularTime> { }
			[PXInt]
			[PXUIField(DisplayName = "Time Spent", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? RegularTime { set; get; }
			#endregion
			#region RegularOvertime
			public abstract class regularOvertime : PX.Data.BQL.BqlInt.Field<regularOvertime> { }
			[PXInt]
			[PXUIField(DisplayName = "Regular Overtime", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? RegularOvertime { set; get; }
			#endregion
			#region RegularTotal
			public abstract class regularTotal : PX.Data.BQL.BqlInt.Field<regularTotal> { }
			[PXInt]
			[PXUIField(DisplayName = "Regular Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? RegularTotal { set; get; }
			#endregion

			#region BillableTime
			public abstract class billableTime : PX.Data.BQL.BqlInt.Field<billableTime> { }
			[PXInt]
			[PXUIField(DisplayName = "Billable", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableTime { set; get; }
			#endregion
			#region BillableOvertime
			public abstract class billableOvertime : PX.Data.BQL.BqlInt.Field<billableOvertime> { }
			[PXInt]
			[PXUIField(DisplayName = "Billable Overtime", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableOvertime { set; get; }
			#endregion
			#region BillableTotal
			public abstract class billableTotal : PX.Data.BQL.BqlInt.Field<billableTotal> { }
			[PXInt]
			[PXUIField(DisplayName = "Billable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableTotal { set; get; }
			#endregion
			#endregion
		}
		#endregion

		public static void UpdateReportedInTimeZoneIDIfNeeded(PXCache cache, PMTimeActivity row, DateTime? oldValue, DateTime? newValue)
		{
			if (oldValue != newValue)
			{
				string timeZoneID = newValue != null ? LocaleInfo.GetTimeZone()?.Id : null;

				cache.SetValueExt<PMTimeActivity.reportedInTimeZoneID>(row, timeZoneID);
			}
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

	}

}