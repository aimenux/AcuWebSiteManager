using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Approval History")]
    public class DailyFieldReportApprovalHistory : BaseCache, IBqlTable
    {
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(EPApproval.approvalID))]
        [PXUIField(DisplayName = "Approval History ID")]
        public int? DailyFieldReportApprovalHistoryId
        {
            get;
            set;
        }

        [PXRefNote]
        [PXDefault(typeof(EPApproval.refNoteID.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "References Nbr.")]
        public Guid? DailyFieldReportNoteId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault(typeof(EPApproval.assignmentMapID.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(SearchFor<EPAssignmentMap.assignmentMapID>
                .Where<EPAssignmentMap.assignmentMapID.IsEqual<EPApproval.assignmentMapID.FromCurrent>>),
            SubstituteKey = typeof(EPAssignmentMap.name), ValidateValue = false)]
        [PXUIField(DisplayName = "Map")]
        public int? AssignmentMapId
        {
            get;
            set;
        }

        [PXDBGuid]
        [PXUIField(DisplayName = "Map Step")]
        [PXDefault(typeof(EPApproval.stepID.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(SearchFor<EPRule.ruleID>
                .Where<EPRule.ruleID.IsEqual<EPApproval.stepID.FromCurrent>>),
            SubstituteKey = typeof(EPRule.name), ValidateValue = false)]
        public Guid? StepId
        {
            get;
            set;
        }

        [PXDBGuid]
        [PXUIField(DisplayName = "Map Rule")]
        [PXDefault(typeof(EPApproval.ruleID.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(SearchFor<EPRule.ruleID>
                .Where<EPRule.ruleID.IsEqual<EPApproval.ruleID.FromCurrent>>),
            SubstituteKey = typeof(EPRule.name), ValidateValue = false)]
        public Guid? RuleId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Workgroup")]
        [PXDefault(typeof(EPApproval.workgroupID.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<EPApproval.workgroupID>))]
        public int? WorkgroupId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Assigned Approver ID")]
        [PXDefault(typeof(EPApproval.ownerID.FromCurrent))]
        public int? OwnerId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault(typeof(Current<EPApproval.documentOwnerID>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Owner")]
        public int? DocumentOwnerId
        {
            get;
            set;
        }

        [PXDBGuid]
        [PXDefault(typeof(EPApproval.approvedByID.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approver ID")]
        public Guid? ApprovedById
        {
            get;
            set;
        }

        [PXDBDate]
        [PXDefault(typeof(EPApproval.approveDate.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approval Date")]
        public DateTime? ApproveDate
        {
            get;
            set;
        }

        [PXNote]
        public override Guid? NoteID
        {
            get;
            set;
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(EPApproval.status.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [EPApprovalStatus.List]
        [PXUIField(DisplayName = "Status")]
        public string Status
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(typeof(EPApproval.reason.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Reason")]
        public string Reason
        {
            get;
            set;
        }

        [PXDBCreatedDateTime(UseTimeZone = true)]
        [PXDefault(typeof(EPApproval.createdDateTime.FromCurrent), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Assignment Date")]
        public override DateTime? CreatedDateTime
        {
            get;
            set;
        }

        public abstract class dailyFieldReportApprovalHistoryId : BqlInt.Field<dailyFieldReportApprovalHistoryId>
        {
        }

        public abstract class dailyFieldReportNoteId : BqlGuid.Field<dailyFieldReportNoteId>
        {
        }

        public abstract class assignmentMapId : BqlInt.Field<assignmentMapId>
        {
        }

        public abstract class stepId : BqlGuid.Field<stepId>
        {
        }

        public abstract class ruleId : BqlGuid.Field<ruleId>
        {
        }

        public abstract class workgroupId : BqlInt.Field<workgroupId>
        {
        }

        public abstract class ownerId : BqlInt.Field<ownerId>
        {
        }

        public abstract class documentOwnerId : BqlInt.Field<documentOwnerId>
        {
        }

        public abstract class approvedById : BqlGuid.Field<approvedById>
        {
        }

        public abstract class approveDate : BqlDateTime.Field<approveDate>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class status : BqlString.Field<status>
        {
        }

        public abstract class reason : BqlString.Field<reason>
        {
        }

        public abstract class tstamp : BqlByteArray.Field<tstamp>
        {
        }

        public abstract class createdByID : BqlGuid.Field<createdByID>
        {
        }

        public abstract class createdByScreenID : BqlString.Field<createdByScreenID>
        {
        }

        public abstract class createdDateTime : BqlDateTime.Field<createdDateTime>
        {
        }

        public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID>
        {
        }

        public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID>
        {
        }

        public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime>
        {
        }
    }
}