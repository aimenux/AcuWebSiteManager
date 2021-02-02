using System;
using PX.Objects.PJ.OutlookIntegration.OU.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.TM;

// TODO : common fields for ProjectIssueOutlook and ProjectIssue should be moved to a base class
namespace PX.Objects.PJ.OutlookIntegration.OU.DAC
{
    [Serializable]
    [PXHidden]
    public class ProjectIssueOutlook : IBqlTable
    {
        [PXDefault(typeof(OUMessage.subject), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Summary")]
        public virtual string Summary
        {
            get;
            set;
        }

        [PXDefault]
        [Project(typeof(Where<PMProject.nonProject, Equal<False>,
            And<PMProject.baseType, Equal<CTPRType.project>>>), DisplayName = "Project")]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString(10, InputMask = ">aaaaaaaaaa", IsUnicode = true)]
        [ResetOutlookResponseDueDate(typeof(dueDate))]
        [PXSelector(typeof(Search<ProjectManagementClass.projectManagementClassId,
                Where<ProjectManagementClass.useForProjectIssue, Equal<True>>>),
            typeof(ProjectManagementClass.projectManagementClassId),
            DescriptionField = typeof(ProjectManagementClass.description))]
        [ClassPriorityDefaulting(nameof(PriorityId))]
        [PXUIField(DisplayName = "Class ID")]
        public virtual string ClassId
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

        [Owner(IsDBField = false)]
        [PXDefault(typeof(AccessInfo.contactID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? OwnerID
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

        public abstract class summary : BqlString.Field<summary>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class classId : BqlString.Field<classId>
        {
        }

        public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

        public abstract class ownerId : BqlInt.Field<ownerId>
        {
        }

        public abstract class dueDate : BqlDateTime.Field<dueDate>
        {
        }
    }
}