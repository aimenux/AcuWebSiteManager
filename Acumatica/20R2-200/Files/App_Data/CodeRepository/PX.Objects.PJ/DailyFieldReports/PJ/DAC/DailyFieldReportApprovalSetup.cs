using System;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Approval Setup")]
    public class DailyFieldReportApprovalSetup : ProjectManagementSetup, IAssignedMap
    {
        [PXDBInt(BqlField = typeof(dailyFieldReportApprovalMapId))]
        public virtual int? AssignmentMapID
        {
            get;
            set;
        }

        [PXDBInt(BqlField = typeof(pendingApprovalNotification))]
        public virtual int? AssignmentNotificationID
        {
            get;
            set;
        }

        [PXBool]
        public virtual bool? IsActive => PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();

        [PXNote]
        public override Guid? NoteID
        {
            get;
            set;
        }

        public abstract class assignmentMapID : BqlInt.Field<assignmentMapID>
        {
        }

        public abstract class assignmentNotificationID : BqlInt.Field<assignmentNotificationID>
        {
        }

        public abstract class isActive : BqlBool.Field<isActive>
        {
        }

        public abstract class noteId : BqlGuid.Field<noteId>
        {
        }
    }
}