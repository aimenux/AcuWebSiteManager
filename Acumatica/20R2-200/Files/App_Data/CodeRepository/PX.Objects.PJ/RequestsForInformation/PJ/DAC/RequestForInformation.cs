using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.CN.Common.Descriptor.Attributes;
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

namespace PX.Objects.PJ.RequestsForInformation.PJ.DAC
{
    [Serializable]
    [PXPrimaryGraph(typeof(RequestForInformationMaint))]
    [PXCacheName(CacheNames.RequestForInformation)]
    [PXEMailSource]
    public class RequestForInformation : ProjectManagementImpact, IAssign, IPXSelectable, IBqlTable, IProjectManagementDocumentBase
    {
        [PXDBIdentity]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible,
            DisplayName = RequestForInformationLabels.RequestForInformationNumberId)]
        [CascadeDelete(typeof(RequestForInformationDrawingLog), typeof(RequestForInformationDrawingLog.requestForInformationId))]
        public virtual int? RequestForInformationId
        {
            get;
            set;
        }

        [PXDefault]
        [PXFieldDescription]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = RequestForInformationLabels.RequestForInformationNumberId,
            Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [AutoNumber(typeof(ProjectManagementSetup.requestForInformationNumberingId), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(Search2<requestForInformationCd,
                LeftJoin<Contact, On<Contact.contactID, Equal<contactID>>>>),
            typeof(requestForInformationCd),
            typeof(projectId),
            typeof(projectTaskId),
            typeof(businessAccountId),
            typeof(Contact.displayName),
            typeof(classId),
            typeof(summary),
            typeof(status),
            typeof(reason),
            typeof(ownerID),
            Filterable = true,
            Headers = new[]
            {
                RequestForInformationLabels.RequestForInformationNumberId,
                RequestForInformationLabels.ProjectId,
                "Project Task",
                RequestForInformationLabels.BusinessAccount,
                RequestForInformationLabels.ContactId,
                RequestForInformationLabels.ClassId,
                RequestForInformationLabels.Summary,
                RequestForInformationLabels.Status,
                RequestForInformationLabels.Reason,
                RequestForInformationLabels.Owner
            })]
        public virtual string RequestForInformationCd
        {
            get;
            set;
        }

        [PXDefault]
        [PXFieldDescription]
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = RequestForInformationLabels.Summary, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Summary
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Link to Incoming RFI")]
        [PXSelector(typeof(Search<requestForInformationId>), SubstituteKey = typeof(requestForInformationCd))]
        public virtual int? IncomingRequestForInformationId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "RFI Number.")]
        public virtual string RequestForInformationNumber
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = RequestForInformationLabels.ClassId)]
        [ResetResponseDueDate(typeof(dueResponseDate))]
        [PXSelector(typeof(Search<ProjectManagementClass.projectManagementClassId,
                Where<ProjectManagementClass.useForRequestForInformation, Equal<True>>>),
            typeof(ProjectManagementClass.projectManagementClassId),
            DescriptionField = typeof(ProjectManagementClass.description))]
        [ClassChangeConfirmation(
            Message = RequestForInformationMessages.ClassChangeWillRemoveAttributes,
            ClassIdField = typeof(classId),
            ViewName = nameof(RequestForInformation))]
        [ClassPriorityDefaulting(nameof(PriorityId))]
        public virtual string ClassId
        {
            get;
            set;
        }

        [PXDefault]
        [Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = RequestForInformationLabels.ProjectId)]
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
        [PXCompanyTreeSelector]
        [PXMassUpdatableField]
        [PXUIField(DisplayName = "Workgroup")]
        public virtual int? WorkgroupID
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBText(IsUnicode = true)]
        [PXUIField(DisplayName = RequestForInformationLabels.RequestDetails)]
        public virtual string RequestDetails
        {
            get;
            set;
        }

        [PXDBText(IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = RequestForInformationLabels.RequestAnswer)]
        public virtual string RequestAnswer
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, DisplayMask = "d")]
        [PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
        public virtual DateTime? LastModifiedRequestAnswer
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

        [PXDBString(1, IsFixed = true)]
        [RequestForInformationStatus]
        [PXDefault(RequestForInformationStatusAttribute.NewStatus)]
        [PXUIField(DisplayName = RequestForInformationLabels.Status, Required = true)]
        public virtual string Status
        {
            get;
            set;
        }

        [PXDBString(1, IsFixed = true)]
        [RequestForInformationStatus]
        [PXDefault(RequestForInformationStatusAttribute.NewStatus)]
        [PXUIField(Visible = false, DisplayName = "Major Status")]
        public virtual string MajorStatus
        {
            get;
            set;
        }

        [BAccount(DisplayName = RequestForInformationLabels.BusinessAccount, Filterable = true)]
        [PXRestrictor(typeof(Where<BAccount.type, In3<BAccountType.vendorType,
                    BAccountType.customerType, BAccountType.combinedType>>),
            RequestForInformationMessages.BusinessAccountRestrictionType, typeof(BAccount.type))]
        [PXRestrictor(typeof(Where<BAccount.status, NotEqual<BAccount.status.inactive>>),
            RequestForInformationMessages.BusinessAccountRestrictionStatus)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? BusinessAccountId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = RequestForInformationLabels.ContactId, Visibility = PXUIVisibility.Visible,
            Required = true)]
        [DependsOnField(typeof(businessAccountId), ShouldDisable = false)]
        [PXSelector(typeof(Search<Contact.contactID,
                Where<Contact.contactType,
                    In3<ContactTypesAttribute.person, ContactTypesAttribute.employee>,
                    And2<Where<Contact.bAccountID, Equal<Current<businessAccountId>>,
                            Or<Current<businessAccountId>, IsNull>>,
                    And<Contact.isActive, Equal<True>>>>>),
            DescriptionField = typeof(Contact.displayName),
            Filterable = true,
            DirtyRead = true)]
        [PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>),
            RequestForInformationMessages.OnlyActiveContactsAreAllowed)]
        public virtual int? ContactId
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Design Change", Visibility = PXUIVisibility.Visible)]
        public virtual bool? DesignChange
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Incoming", Visibility = PXUIVisibility.Visible)]
        public virtual bool? Incoming
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Documentation Link")]
        public virtual string DocumentationLink
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Spec. section")]
        public virtual string SpecSection
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Creation Date")]
        public DateTime? CreationDate
        {
            get;
            set;
        }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By", Required = true)]
        public override Guid? CreatedById
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

        [LinkRefNote(typeof(PMChangeRequest.refNbr))]
        [PXUIField(DisplayName = "Converted To", Enabled = false)]
        public virtual Guid? ConvertedTo
        {
            get;
            set;
        }

        [LinkRefNote(typeof(ProjectIssue.projectIssueCd))]
        [PXUIField(DisplayName = "Converted From", Enabled = false)]
        public virtual Guid? ConvertedFrom
        {
            get;
            set;
        }

        [PXDBString(50)]
        [RequestForInformationReason]
        [PXDefault(RequestForInformationReasonAttribute.Unassigned)]
        [CRDropDownAutoValue(typeof(status))]
        [PXUIField(DisplayName = RequestForInformationLabels.Reason)]
        public virtual string Reason
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Answer Due Date")]
        public virtual DateTime? DueResponseDate
        {
            get;
            set;
        }

        [PXNote(ShowInReferenceSelector = true,
            Selector = typeof(Search<requestForInformationCd>),
            FieldList = new[]
            {
                typeof(requestForInformationCd),
                typeof(summary),
                typeof(status)
            })]
        [RequestForInformationSearchable]
        public override Guid? NoteID
        {
            get;
            set;
        }

        [PXString]
        [PXFormula(typeof(IIf<Where<isScheduleImpact, Equal<True>>,
            RequestForInformationMessages.NotificationTemplate.yesWithComma,
            RequestForInformationMessages.NotificationTemplate.no>))]
        public string IsScheduleImpactFormatted
        {
            get;
            set;
        }

        [PXString]
        [PXFormula(typeof(IIf<Where<isCostImpact, Equal<True>>,
            RequestForInformationMessages.NotificationTemplate.yesWithComma,
            RequestForInformationMessages.NotificationTemplate.no>))]
        public string IsCostImpactFormatted
        {
            get;
            set;
        }

        [PXString]
        [PXFormula(typeof(IIf<Where<designChange, Equal<True>>,
            RequestForInformationMessages.NotificationTemplate.yes,
            RequestForInformationMessages.NotificationTemplate.no>))]
        public string DesignChangeFormatted
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

        public abstract class businessAccountId : BqlInt.Field<businessAccountId>
        {
        }

        public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

        public abstract class convertedTo : BqlGuid.Field<convertedTo>
        {
        }

        public abstract class dueResponseDate : BqlDateTime.Field<dueResponseDate>
        {
        }

        public abstract class convertedFrom : BqlGuid.Field<convertedFrom>
        {
        }

        public abstract class reason : BqlString.Field<reason>
        {
        }

        public abstract class specSection : BqlString.Field<specSection>
        {
        }

        public abstract class incomingRequestForInformationId : BqlInt.Field<incomingRequestForInformationId>
        {
        }

        public abstract class documentationLink : BqlString.Field<documentationLink>
        {
        }

        public abstract class requestAnswer : BqlString.Field<requestAnswer>
        {
        }

        public abstract class designChange : BqlBool.Field<designChange>
        {
        }

        public abstract class contactID : BqlInt.Field<contactID>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class incoming : BqlBool.Field<incoming>
        {
        }

        public abstract class status : BqlString.Field<status>
        {
        }

        public abstract class majorStatus : BqlString.Field<majorStatus>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class attributes : BqlAttributes.Field<attributes>
        {
        }

        public abstract class requestDetails : BqlString.Field<requestDetails>
        {
        }

        public abstract class lastModifiedRequestAnswer : BqlDateTime.Field<lastModifiedRequestAnswer>
        {
        }

        public abstract class summary : BqlString.Field<summary>
        {
        }

        public abstract class requestForInformationId : BqlInt.Field<requestForInformationId>
        {
        }

        public abstract class requestForInformationCd : BqlString.Field<requestForInformationCd>
        {
        }

        public abstract class classId : BqlString.Field<classId>
        {
        }

        public abstract class requestForInformationNumber : BqlString.Field<requestForInformationNumber>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class ownerID : BqlInt.Field<ownerID>
        {
        }

        public abstract class workgroupID : BqlInt.Field<workgroupID>
        {
        }

        public abstract class creationDate : BqlDateTime.Field<creationDate>
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
                : base(typeof(RequestForInformation).FullName)
            {
            }
        }
    }
}