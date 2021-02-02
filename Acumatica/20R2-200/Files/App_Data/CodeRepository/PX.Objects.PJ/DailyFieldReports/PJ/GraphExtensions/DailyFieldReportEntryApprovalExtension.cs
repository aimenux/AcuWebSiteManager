using System.Collections;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;
using Messages = PX.Objects.PM.Messages;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryApprovalExtension : PXGraphExtension<DailyFieldReportEntry>
    {
        [PXViewName(PX.Objects.EP.Messages.Approval)]
        public EPApprovalAutomation<DailyFieldReport, DailyFieldReport.approved, DailyFieldReport.rejected,
            DailyFieldReport.hold, DailyFieldReportApprovalSetup> Approval;

        public SelectFrom<DailyFieldReportApprovalHistory>
            .LeftJoin<ApproverEmployee>
                .On<ApproverEmployee.defContactID.IsEqual<DailyFieldReportApprovalHistory.ownerId>>
            .LeftJoin<ApprovedByEmployee>
                .On<ApprovedByEmployee.userID.IsEqual<DailyFieldReportApprovalHistory.approvedById>>
            .Where<DailyFieldReportApprovalHistory.dailyFieldReportNoteId
                .IsEqual<DailyFieldReport.noteID.FromCurrent>>.View ApprovalHistory;

        public PXAction<DailyFieldReport> ActionsFolder;
        public PXAction<DailyFieldReport> Approve;
        public PXAction<DailyFieldReport> Reject;

        public override void Initialize()
        {
            var isApprovalFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();
            ActionsFolder.AddMenuAction(Approve);
            ActionsFolder.AddMenuAction(Reject);
            ActionsFolder.SetVisible(isApprovalFeatureEnabled);
            Approval.View.AllowSelect = isApprovalFeatureEnabled;
            ApprovalHistory.View.AllowSelect = isApprovalFeatureEnabled;
        }

        [PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder, MenuAutoOpen = true)]
        public void actionsFolder()
        {
        }

        [PXUIField(DisplayName = Messages.Approve, Visible = false, MapEnableRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false)]
        public IEnumerable approve(PXAdapter adapter)
        {
            return ChangeDailyFieldReportStatus<DailyFieldReport.approved>(adapter, DailyFieldReportStatus.Completed);
        }

        [PXUIField(DisplayName = Messages.Reject, Visible = false, MapEnableRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false)]
        public IEnumerable reject(PXAdapter adapter)
        {
            return ChangeDailyFieldReportStatus<DailyFieldReport.rejected>(adapter, DailyFieldReportStatus.Rejected);
        }

        public virtual void _(Events.RowUpdating<DailyFieldReport> args)
        {
            var dailyFieldReport = args.Row;
            var updatedDailyFieldReport = args.NewRow;
            if (dailyFieldReport.Hold != updatedDailyFieldReport.Hold)
            {
                if (updatedDailyFieldReport.Hold == true)
                {
                    SetStatusesToDefault(updatedDailyFieldReport);
                    DeletePendingApprovals();
                }
                else
                {
                    SetNextStatus(updatedDailyFieldReport);
                }
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReport> args)
        {
            var isStatusPendingApproval = args.Row.Status.Trim() == DailyFieldReportStatus.PendingApproval;
            Approve.SetEnabled(isStatusPendingApproval);
            Reject.SetEnabled(isStatusPendingApproval);
        }

        public virtual void _(Events.RowInserted<EPApproval> args)
        {
            ApprovalHistory.Insert();
        }

        public virtual void _(Events.RowUpdated<EPApproval> args)
        {
            var approval = args.Row;
            var approvalHistory = GetApprovalHistory(approval);
            approvalHistory.ApprovedById = PXAccess.GetUserID(approval.ApprovedByID);
            approvalHistory.ApproveDate = approval.ApproveDate;
            approvalHistory.Status = approval.Status;
            ApprovalHistory.Update(approvalHistory);
        }

        private void DeletePendingApprovals()
        {
            var pendingApprovals = ApprovalHistory.Select<DailyFieldReportApprovalHistory>()
                .Where(a => a.Status == EPApprovalStatus.Pending);
            ApprovalHistory.Cache.DeleteAll(pendingApprovals);
        }

        private void SetNextStatus(DailyFieldReport updatedDailyFieldReport)
        {
            updatedDailyFieldReport.Status = IsApprovalConfigured()
                ? DailyFieldReportStatus.PendingApproval
                : DailyFieldReportStatus.Completed;
        }

        private static void SetStatusesToDefault(DailyFieldReport updatedDailyFieldReport)
        {
            updatedDailyFieldReport.Status = DailyFieldReportStatus.Hold;
            updatedDailyFieldReport.Approved = false;
            updatedDailyFieldReport.Rejected = false;
        }

        private bool IsApprovalConfigured()
        {
            var isApprovalFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();
            var projectManagementSetup = Base.ProjectManagementSetup.Current;
            return isApprovalFeatureEnabled && projectManagementSetup.DailyFieldReportApprovalMapId.HasValue;
        }

        private IEnumerable ChangeDailyFieldReportStatus<TStatusField>(PXAdapter adapter, string nextStatus)
            where TStatusField : IBqlField
        {
            var dailyFieldReports = adapter.Get<DailyFieldReport>().ToArray();
            return dailyFieldReports.Select(dfr =>
                ChangeDailyFieldReportStatus<TStatusField>(dfr, nextStatus));
        }

        private DailyFieldReport ChangeDailyFieldReportStatus<TStatusField>(DailyFieldReport dailyFieldReport,
            string nextStatus)
            where TStatusField : IBqlField
        {
            Base.DailyFieldReport.Cache.SetValue<TStatusField>(dailyFieldReport, true);
            dailyFieldReport.Status = nextStatus;
            Base.DailyFieldReport.Update(dailyFieldReport);
            Base.Save.Press();
            return dailyFieldReport;
        }

        private DailyFieldReportApprovalHistory GetApprovalHistory(EPApproval approval)
        {
            return ApprovalHistory
                .Search<DailyFieldReportApprovalHistory.dailyFieldReportApprovalHistoryId>(approval.ApprovalID);
        }
    }
}