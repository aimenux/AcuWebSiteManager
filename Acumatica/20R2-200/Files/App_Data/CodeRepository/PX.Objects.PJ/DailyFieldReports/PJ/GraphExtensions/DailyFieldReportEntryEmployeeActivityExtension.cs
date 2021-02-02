using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.TM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryEmployeeActivityExtension : PXGraphExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.EmployeeActivities)]
        [PXCopyPasteHiddenFields(typeof(EPActivityApprove.date), typeof(EPActivityApprove.hold),
            typeof(EPActivityApprove.trackTime), typeof(PMTimeActivity.createdByID),
            typeof(PMTimeActivity.lastModifiedDateTime), typeof(PMTimeActivity.refNoteID), typeof(Note.noteText))]
        public SelectFrom<EPActivityApprove>
            .LeftJoin<DailyFieldReportEmployeeActivity>
                .On<EPActivityApprove.noteID.IsEqual<DailyFieldReportEmployeeActivity.employeeActivityId>>
            .Where<DailyFieldReportEmployeeActivity.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View EmployeeActivities;

        public PXSetup<EPSetup> EmployeeSetup;

        [InjectDependency]
        public IEmployeeDataProvider EmployeeDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public IProjectTaskDataProvider ProjectTaskDataProvider
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>();
        }

        [EPProject(typeof(EPActivityApprove.ownerID))]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void EPActivityApprove_ProjectID_CacheAttached(PXCache cache)
        {
        }

        [PXNote(IsKey = true)]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void EPActivityApprove_NoteID_CacheAttached(PXCache cache)
        {
        }

        [PXRemoveBaseAttribute(typeof(PXSequentialSelfRefNoteAttribute))]
        [PXDBGuid(true)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_RefNoteID_CacheAttached(PXCache cache)
        {
        }

        [PXDefault]
        [EPTimecardProjectTask(typeof(EPActivityApprove.projectID),
            PX.Objects.GL.BatchModule.TA, DisplayName = "Project Task")]
        [PXRestrictor(typeof(Where<PMTask.type.IsNotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<EPActivityApprove.projectID>))]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void EPActivityApprove_ProjectTaskID_CacheAttached(PXCache cache)
        {
        }

        [PXUIEnabled(typeof(PMTimeActivity.isBillable))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_TimeBillable_CacheAttached(PXCache cache)
        {
        }

        [PXDefault]
        [PXUIField(DisplayName = "Employee", Required = true)]
        [SubordinateAndWingmenOwner]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void EPActivityApprove_OwnerID_CacheAttached(PXCache cache)
        {
        }

        [PXDefault]
        [PXFieldDescription]
        [PXDBString(255, InputMask = "", IsUnicode = true, BqlField = typeof(PMTimeActivity.summary))]
        [PXUIField(DisplayName = "Description",
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(
            Switch<Case<Where<Current<EPActivityApprove.summary>, IsNotNull>,
                Current<EPActivityApprove.summary>,
                Case<Where<EPActivityApprove.parentTaskNoteID.IsNotNull>,
                    Selector<EPActivityApprove.parentTaskNoteID, EPActivityApprove.summary>,
                    Case<Where<EPActivityApprove.projectTaskID.IsNotNull>,
                        Selector<EPActivityApprove.projectTaskID, PMTask.description>>>>>))]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void EPActivityApprove_Summary_CacheAttached(PXCache cache)
        {
        }

        [PXSelector(typeof(SelectFrom<CRActivity>.
                Where<CRActivity.noteID.IsNotEqual<EPActivityApprove.noteID.FromCurrent>
                    .And<CRActivity.ownerID.IsEqual<EPActivityApprove.ownerID.FromCurrent>>
                    .And<CRActivity.classID.IsNotEqual<CRActivityClass.events>>
                    .And<CRActivity.classID.IsEqual<CRActivityClass.task>
                        .Or<CRActivity.classID.IsEqual<CRActivityClass.events>>>>
                .OrderBy<CRActivity.createdDateTime.Desc>.SearchFor<CRActivity.noteID>),
            typeof(CRActivity.subject),
            typeof(CRActivity.uistatus),
            DescriptionField = typeof(CRActivity.subject))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_ParentTaskNoteID_CacheAttached(PXCache cache)
        {
        }

        [PXUIField(DisplayName = "Time Card Ref.",
            Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_TimeCardCD_CacheAttached(PXCache cache)
        {
        }

        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_IsBillable_CacheAttached(PXCache cache)
        {
        }

        [PXCustomizeBaseAttribute(typeof(CostCodeAttribute), Constants.AttributeProperties.Required, true)]
        public virtual void EPActivityApprove_CostCodeID_CacheAttached(PXCache cache)
        {
        }

        [PXUIField(DisplayName = "Last Modification Date", IsReadOnly = true)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_LastModifiedDateTime_CacheAttached(PXCache cache)
        {
        }

        [DefaultDateFromDailyFieldReport]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        public virtual void EPActivityApprove_Date_CacheAttached(PXCache cache)
        {
        }

        public virtual void _(Events.RowUpdating<EPActivityApprove> args)
        {
            var employeeActivity = args.NewRow;
            if (employeeActivity?.TimeCardCD != null)
            {
                args.Cancel = true;
                if (Base.IsMobile)
                {
                    args.Cache.RaiseException<EPActivityApprove.hold>(
                        employeeActivity, PX.Objects.EP.Messages.ActivityAssignedToTimeCard, false);
                }
                else
                {
                    EmployeeActivities.Ask(PX.Objects.EP.Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
                }
            }
        }

        public virtual void _(Events.RowDeleting<EPActivityApprove> args)
        {
            var employeeActivity = args.Row;
            if (employeeActivity.TimeCardCD != null)
            {
                EmployeeActivities.View.Ask(PX.Objects.EP.Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
                args.Cancel = true;
            }
        }

        public virtual void _(Events.RowSelected<EPActivityApprove> args)
        {
            var employeeActivity = args.Row;
            if (employeeActivity != null)
            {
                SetFieldsAvailability(employeeActivity, args.Cache);
                ValidateEmployeeActivityApprovalStatus(employeeActivity, args.Cache);
                if (Base.IsMobile)
                {
                    employeeActivity.LastModifiedDateTime = employeeActivity.LastModifiedDateTime.GetValueOrDefault().Date;
                }
            }
        }

        public virtual void _(Events.FieldVerifying<EPActivityApprove, EPActivityApprove.projectTaskID> args)
        {
            if (args.Row != null && args.NewValue is int projectTaskId)
            {
                var projectTask = ProjectTaskDataProvider.GetProjectTask(Base, projectTaskId);
                if (projectTask != null)
                {
                    VerifyProjectTask(projectTask);
                }
            }
        }

        public virtual void _(Events.FieldSelecting<EPActivityApprove, EPActivityApprove.hold> args)
        {
            var employeeActivity = args.Row;
            if (employeeActivity != null)
            {
                args.ReturnValue = employeeActivity.ApprovalStatus == ActivityStatusListAttribute.Open;
            }
        }

        public virtual void _(Events.FieldUpdated<EPActivityApprove, EPActivityApprove.hold> args)
        {
            args.Cache.SetDefaultExt<EPActivityApprove.approverID>(args.Row);
        }

        public virtual void _(Events.FieldUpdated<EPActivityApprove, PMTimeActivity.costCodeID> args)
        {
            args.Cache.SetDefaultExt<PMTimeActivity.workCodeID>(args.Row);
        }

        public virtual void _(Events.FieldUpdated<EPActivityApprove, PMTimeActivity.ownerID> args)
        {
            var employeeActivity = args.Row;
            if (employeeActivity?.OwnerID != null)
            {
                var employee = EmployeeDataProvider.GetEmployee(Base, employeeActivity.OwnerID);
                employeeActivity.EarningTypeID = CalendarHelper.IsWorkDay(
                    Base, employee?.CalendarID, employeeActivity.Date.GetValueOrDefault())
                    ? EmployeeSetup.Current?.RegularHoursType
                    : EmployeeSetup.Current?.HolidaysType;
            }
        }

        public virtual void _(Events.FieldUpdated<EPActivityApprove, PMTimeActivity.isBillable> args)
        {
            var employeeActivity = args.Row;
            if (employeeActivity != null)
            {
                employeeActivity.TimeBillable = GetTimeBillable(employeeActivity);
            }
        }

        public virtual void _(Events.FieldUpdated<EPActivityApprove, PMTimeActivity.timeSpent> args)
        {
            var employeeActivity = args.Row;
            if (employeeActivity != null)
            {
                employeeActivity.TimeBillable = GetTimeBillable(employeeActivity, (int?) args.OldValue);
            }
        }

        public virtual void _(Events.RowUpdated<EPActivityApprove> args)
        {
            var employeeActivity = args.Row;
            if (IsEmployeeRateUpdateNeeded(employeeActivity, args.OldRow))
            {
                var employee = EmployeeDataProvider.GetEmployee(Base, employeeActivity.OwnerID);
                var employeeCostEngine = new EmployeeCostEngine(Base);
                employeeActivity.EmployeeRate = employeeCostEngine.CalculateEmployeeCost(null,
                    employeeActivity.EarningTypeID, employeeActivity.LabourItemID, employeeActivity.ProjectID,
                    employeeActivity.ProjectTaskID, employeeActivity.CertifiedJob,
                    employeeActivity.UnionID, employee.BAccountID, employeeActivity.Date.GetValueOrDefault())?.Rate;
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReport> args)
        {
            var dailyFieldReport = args.Row;
            if (dailyFieldReport != null)
            {
                SetFieldsVisibility();
            }
        }

        private static void VerifyProjectTask(PMTask projectTaskTask)
        {
            if (projectTaskTask.IsCompleted == true)
            {
                RaiseException(projectTaskTask.TaskCD, PX.Objects.PM.Messages.ProjectTaskIsCompleted);
            }
            if (projectTaskTask.IsCancelled == true)
            {
                RaiseException(projectTaskTask.TaskCD, PX.Objects.PM.Messages.ProjectTaskIsCanceled);
            }
        }

        private static void RaiseException(string taskCd, string message)
        {
            throw new PXSetPropertyException(message)
            {
                ErrorValue = taskCd
            };
        }

        private static int? GetTimeBillable(PMTimeActivity employeeActivity, int? oldTimeSpent = 0)
        {
            if (employeeActivity.Billed == true)
            {
                return employeeActivity.TimeBillable;
            }
            if (employeeActivity.IsBillable != true)
            {
                return 0;
            }
            return oldTimeSpent == 0 || oldTimeSpent == employeeActivity.TimeBillable
                ? employeeActivity.TimeSpent
                : employeeActivity.TimeBillable;
        }

        private void SetFieldsVisibility()
        {
            SetCustomerModuleVisibility();
            SetProjectModuleVisibility();
            PXDBDateAndTimeAttribute.SetTimeVisible<EPActivityApprove.date>(EmployeeActivities.Cache,
                null, EmployeeSetup.Current.RequireTimes == true);
        }

        private void SetProjectModuleVisibility()
        {
            var isProjectModuleInstalled = PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
            PXUIFieldAttribute.SetVisible(EmployeeActivities.Cache,
                typeof(EPActivityApprove.projectTaskID).Name, isProjectModuleInstalled);
            PXUIFieldAttribute.SetVisible<EPActivityApprove.approvalStatus>(EmployeeActivities.Cache,
                null, isProjectModuleInstalled);
            PXUIFieldAttribute.SetVisible<EPActivityApprove.approverID>(EmployeeActivities.Cache,
                null, isProjectModuleInstalled);
        }

        private void SetCustomerModuleVisibility()
        {
            PXUIFieldAttribute.SetVisible<EPActivityApprove.contractID>(EmployeeActivities.Cache, null,
                !PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
        }

        private static void SetFieldsAvailability(PMTimeActivity employeeActivity, PXCache cache)
        {
            if (employeeActivity.Released == true)
            {
                PXUIFieldAttribute.SetEnabled(cache, employeeActivity, false);
            }
            else if (employeeActivity.ApprovalStatus == ActivityStatusListAttribute.Open)
            {
                PXUIFieldAttribute.SetEnabled(cache, employeeActivity, true);
                PXDBDateAndTimeAttribute.SetTimeEnabled<EPActivityApprove.date>(cache, employeeActivity, true);
                PXDBDateAndTimeAttribute.SetDateEnabled<EPActivityApprove.date>(cache, employeeActivity, true);
                PXUIFieldAttribute.SetEnabled<EPActivityApprove.approverID>(cache, employeeActivity, false);
            }
            else
            {
                PXUIFieldAttribute.SetEnabled(cache, employeeActivity, false);
                PXUIFieldAttribute.SetEnabled<EPActivityApprove.hold>(cache, employeeActivity, true);
            }
            PXUIFieldAttribute.SetEnabled<EPActivityApprove.approvalStatus>(cache, employeeActivity, false);
            PXUIFieldAttribute.SetEnabled<PMTimeActivity.employeeRate>(cache, employeeActivity, false);
            PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeCardCD>(cache, employeeActivity, false);
        }

        private static void ValidateEmployeeActivityApprovalStatus(PMTimeActivity employeeActivity, PXCache cache)
        {
            if (employeeActivity.ApprovalStatus == ActivityStatusListAttribute.Rejected)
            {
                cache.RaiseException<EPActivityApprove.hold>(
                    employeeActivity, PX.Objects.EP.Messages.Rejected, errorLevel: PXErrorLevel.RowWarning);
            }
        }

        private static bool IsEmployeeRateUpdateNeeded(PMTimeActivity employeeActivity,
            PMTimeActivity oldEmployeeActivity)
        {
            return employeeActivity.OwnerID != null &&
                (employeeActivity.Date != oldEmployeeActivity.Date ||
                    employeeActivity.EarningTypeID != oldEmployeeActivity.EarningTypeID ||
                    employeeActivity.ProjectID != oldEmployeeActivity.ProjectID ||
                    employeeActivity.ProjectTaskID != oldEmployeeActivity.ProjectTaskID ||
                    employeeActivity.CostCodeID != oldEmployeeActivity.CostCodeID ||
                    employeeActivity.UnionID != oldEmployeeActivity.UnionID ||
                    employeeActivity.LabourItemID != oldEmployeeActivity.LabourItemID ||
                    employeeActivity.CertifiedJob != oldEmployeeActivity.CertifiedJob ||
                    employeeActivity.OwnerID != oldEmployeeActivity.OwnerID);
        }
    }
}