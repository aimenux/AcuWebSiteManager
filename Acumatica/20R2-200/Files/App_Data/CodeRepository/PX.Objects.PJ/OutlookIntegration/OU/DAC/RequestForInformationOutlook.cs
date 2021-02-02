using System;
using PX.Objects.PJ.OutlookIntegration.OU.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.TM;

// TODO : common fields for RequestForInformationOutlook and RequestForInformation should be moved to a base class
namespace PX.Objects.PJ.OutlookIntegration.OU.DAC
{
    [Serializable]
    [PXHidden]
    public class RequestForInformationOutlook : IBqlTable
    {
        [PXDefault(typeof(OUMessage.subject), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = RequestForInformationLabels.Summary)]
        public virtual string Summary
        {
            get;
            set;
        }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString(10, InputMask = ">aaaaaaaaaa", IsUnicode = true)]
        [ResetOutlookResponseDueDate(typeof(dueResponseDate))]
        [PXUIField(DisplayName = RequestForInformationLabels.ClassId)]
        [PXSelector(typeof(Search<ProjectManagementClass.projectManagementClassId,
                Where<ProjectManagementClass.useForRequestForInformation, Equal<True>>>),
            typeof(ProjectManagementClass.projectManagementClassId),
            DescriptionField = typeof(ProjectManagementClass.description))]
        [ClassPriorityDefaulting(nameof(PriorityId))]
        public virtual string ClassId
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Incoming")]
        public virtual bool? Incoming
        {
            get;
            set;
        }

        [PXString(1, IsFixed = true)]
        [RequestForInformationStatus]
        [PXDefault(RequestForInformationStatusAttribute.NewStatus, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = RequestForInformationLabels.Status, Required = true)]
        public virtual string Status
        {
            get;
            set;
        }

        [PXInt]
        [ProjectManagementPrioritySelector(typeof(classId))]
        [PXUIField(DisplayName = "Priority")]
        public virtual int? PriorityId
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

        [PXInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<Contact.contactID,
                Where<Contact.contactType,
                    In3<ContactTypesAttribute.person, ContactTypesAttribute.employee>,
                    And<Contact.isActive, Equal<True>>>>),
            SubstituteKey = typeof(Contact.displayName))]
        [PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>),
            RequestForInformationMessages.OnlyActiveContactsAreAllowed)]
        [PXUIField(DisplayName = RequestForInformationLabels.ContactId, Required = true)]
        public virtual int? ContactId
        {
            get;
            set;
        }

        [Owner(IsDBField = false)]
        [PXDefault(typeof(AccessInfo.contactID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? OwnerId
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Answer Date")]
        public virtual DateTime? DueResponseDate
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Schedule Impact (days)")]
        public virtual bool? IsScheduleImpact
        {
            get;
            set;
        }

        [PXInt]
        [PXUIField(DisplayName = "Schedule Impact (days)")]
        public virtual int? ScheduleImpact
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Cost Impact")]
        public virtual bool? IsCostImpact
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = "Cost Impact")]
        public virtual decimal? CostImpact
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Design Change")]
        public virtual bool? DesignChange
        {
            get;
            set;
        }

        public abstract class summary : BqlString.Field<summary>
        {
        }

        public abstract class classId : BqlString.Field<classId>
        {
        }

        public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class incoming : BqlBool.Field<incoming>
        {
        }

        public abstract class status : BqlString.Field<status>
        {
        }

        public abstract class contactId : BqlInt.Field<contactId>
        {
        }

        public abstract class ownerId : BqlInt.Field<ownerId>
        {
        }

        public abstract class dueResponseDate : BqlDateTime.Field<dueResponseDate>
        {
        }

        public abstract class isCostImpact : BqlBool.Field<isCostImpact>
        {
        }

        public abstract class costImpact : BqlDecimal.Field<costImpact>
        {
        }

        public abstract class isScheduleImpact : BqlBool.Field<isScheduleImpact>
        {
        }

        public abstract class scheduleImpact : BqlInt.Field<scheduleImpact>
        {
        }

        public abstract class designChange : BqlBool.Field<designChange>
        {
        }
    }
}