using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.Descriptor;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.Utilities;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CN.Common.Utilities;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.TM;
using Constants = PX.Objects.PJ.Common.Descriptor.Constants;

namespace PX.Objects.PJ.ProjectsIssue.PJ.DAC
{
    [Serializable]
    [PXEMailSource]
    [PXPrimaryGraph(typeof(ProjectIssueMaint))]
    [PXCacheName(CacheNames.ProjectIssue)]
    public class ProjectIssue : ProjectManagementImpact, IBqlTable, IAssign, IPXSelectable, IProjectManagementDocumentBase
    {
        [PXDBIdentity]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible,
            DisplayName = "Project Issue ID")]
        [CascadeDelete(typeof(ProjectIssueDrawingLog), typeof(ProjectIssueDrawingLog.projectIssueId))]
        public virtual int? ProjectIssueId
        {
            get;
            set;
        }

        [PXDefault]
        [PXFieldDescription]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Project Issue ID", Required = true,
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<projectIssueCd>),
            typeof(projectIssueCd),
            typeof(projectId),
            typeof(projectTaskId),
            typeof(classId),
            typeof(summary),
            typeof(status),
            typeof(ownerID),
            Filterable = true)]
        [AutoNumber(typeof(ProjectManagementSetup.projectIssueNumberingId), typeof(AccessInfo.businessDate))]
        public virtual string ProjectIssueCd
        {
            get;
            set;
        }

        [PXDefault]
        [PXFieldDescription]
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Summary
        {
            get;
            set;
        }

        [PXDefault]
        [Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = "Project")]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [ProjectTaskWithType(typeof(projectId), AlwaysEnabled = true, AllowNull = true,
            DisplayName = "Project Task")]
        [PXFormula(typeof(Validate<projectId>))]
        [ProjectTaskTypeValidation(
            ProjectTaskIdField = typeof(projectTaskId),
            Message = ProjectAccountingMessages.CostTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Revenue)]
        public virtual int? ProjectTaskId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Class ID")]
        [ResetResponseDueDate(typeof(dueDate))]
        [PXSelector(typeof(Search<ProjectManagementClass.projectManagementClassId,
                Where<ProjectManagementClass.useForProjectIssue, Equal<True>>>),
            typeof(ProjectManagementClass.projectManagementClassId),
            DescriptionField = typeof(ProjectManagementClass.description))]
        [ClassChangeConfirmation(
            Message = ProjectIssueMessages.WarningRemovingProjectIssueAttributes,
            ClassIdField = typeof(classId),
            ViewName = nameof(ProjectIssue))]
        [ClassPriorityDefaulting(nameof(PriorityId))]
        public virtual string ClassId
        {
            get;
            set;
        }

        [Owner(typeof(workgroupID))]
        [PXMassUpdatableField]
        [PXFormula(typeof(Default<workgroupID>))]
        [PXDefault(typeof(Coalesce<
            Search<EPCompanyTreeMember.contactID,
                Where<EPCompanyTreeMember.workGroupID, Equal<Current<workgroupID>>,
                    And<EPCompanyTreeMember.contactID, Equal<Current<AccessInfo.contactID>>>>>,
            Search<CREmployee.defContactID,
                Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
                    And<Current<workgroupID>, IsNull>>>>))]
        public virtual int? OwnerID
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Workgroup")]
        [PXCompanyTreeSelector]
        [PXMassUpdatableField]
        public virtual int? WorkgroupID
        {
            get;
            set;
        }

        [PXDBText(IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description
        {
            get;
            set;
        }

        [PXDBInt]
        [ProjectManagementPrioritySelector(typeof(classId))]
        [PXUIField(DisplayName = "Priority")]
        public virtual int? PriorityId
        {
            get;
            set;
        }

        [PXDBString(1, IsFixed = true)]
        [ProjectIssueStatus]
        [PXDefault(ProjectIssueStatusAttribute.Open)]
        [PXUIField(DisplayName = "Status", Required = true)]
        public virtual string Status
        {
            get;
            set;
        }

        [PXDBString(1, IsFixed = true)]
        [ProjectIssueStatus]
        [PXDefault(ProjectIssueStatusAttribute.Open)]
        public virtual string MajorStatus
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Due Date")]
        public virtual DateTime? DueDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Resolved On")]
        public virtual DateTime? ResolvedOn
        {
            get;
            set;
        }

        [PXDBDateAndTime(DisplayNameDate = "Created On")]
        [DefaultWorkingTimeStart]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Created On", Required = false)]
        public DateTime? CreationDate
        {
            get;
            set;
        }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By")]
        public override Guid? CreatedById
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Selected
        {
            get;
            set;
        }

        [LinkRefNote(typeof(RequestForInformation.requestForInformationCd), typeof(PMChangeRequest.refNbr))]
        [PXUIField(DisplayName = "Converted To")]
        public virtual Guid? ConvertedTo
        {
            get;
            set;
        }

        [PXNote(ShowInReferenceSelector = true,
            Selector = typeof(Search<projectIssueCd>),
            FieldList = new[]
            {
                typeof(projectIssueCd),
                typeof(summary),
                typeof(status)
            })]
        [ProjectIssueSearchable]
        public override Guid? NoteID
        {
            get;
            set;
        }

        [PXDBGuid(DatabaseFieldName = "RelatedEntityId")]
        public virtual Guid? RefNoteId
        {
            get;
            set;
        }

        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Related Entity", Enabled = false)]
        [PXFormula(typeof(RelatedEntityDescription<refNoteId>))]
        public virtual string RelatedEntityDescription
        {
            get;
            set;
        }

        [PXUIField(DisplayName = "Priority Icon", IsReadOnly = true)]
        [PXImage(HeaderImage = Constants.PriorityIconHeaderImage)]
        [PXFormula(typeof(PriorityIcon<priorityId>))]
        public virtual string PriorityIcon
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Project Issue Type")]
        [PXSelector(typeof(SearchFor<ProjectIssueType.projectIssueTypeId>),
            SubstituteKey = typeof(ProjectIssueType.typeName))]
        public virtual int? ProjectIssueTypeId
        {
            get;
            set;
        }

        [CRAttributesField(typeof(classId), typeof(noteID))]
        public virtual string[] Attributes
        {
            get;
            set;
        }

        [PXUIField(DisplayName = "Last Modification Date")]
        [PXDBLastModifiedDateTime]
        public override DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        public abstract class refNoteId : BqlGuid.Field<refNoteId>
        {
        }

        public abstract class summary : BqlString.Field<summary>
        {
        }

        public abstract class projectIssueCd : BqlString.Field<projectIssueCd>
        {
        }

        public abstract class projectIssueId : BqlInt.Field<projectIssueId>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class classId : BqlString.Field<classId>
        {
        }

        public abstract class attributes : BqlAttributes.Field<attributes>
        {
        }

        public abstract class dueDate : BqlDateTime.Field<dueDate>
        {
        }

        public abstract class resolvedOn : BqlDateTime.Field<resolvedOn>
        {
        }

        public abstract class ownerID : BqlInt.Field<ownerID>
        {
        }

        public abstract class workgroupID : BqlInt.Field<workgroupID>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

        public abstract class status : BqlString.Field<status>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class creationDate : BqlDateTime.Field<creationDate>
        {
        }

        public abstract class priorityIcon : BqlString.Field<priorityIcon>
        {
        }

        public abstract class convertedTo : BqlGuid.Field<convertedTo>
        {
        }

        public abstract class projectIssueTypeId : BqlInt.Field<projectIssueTypeId>
        {
        }

        public new abstract class isScheduleImpact : BqlBool.Field<isScheduleImpact>
        {
        }

        public new abstract class scheduleImpact : BqlInt.Field<scheduleImpact>
        {
        }

        public new abstract class isCostImpact : BqlBool.Field<isCostImpact>
        {
        }

        public new abstract class costImpact : BqlDecimal.Field<costImpact>
        {
        }

        public class typeName : BqlString.Constant<typeName>
        {
            public typeName()
                : base(typeof(ProjectIssue).FullName)
            {
            }
        }
    }
}