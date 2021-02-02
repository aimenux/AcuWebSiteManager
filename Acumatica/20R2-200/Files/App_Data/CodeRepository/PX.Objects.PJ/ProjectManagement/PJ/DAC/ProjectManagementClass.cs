using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    [Serializable]
    [PXPrimaryGraph(typeof(ProjectManagementClassMaint))]
    [PXCacheName(CacheNames.ProjectManagementClass)]
    public class ProjectManagementClass : BaseCache, IBqlTable
    {
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Project Management Class ID",
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(projectManagementClassId),
            typeof(projectManagementClassId),
            typeof(description),
            typeof(useForProjectIssue),
            typeof(useForRequestForInformation),
            DescriptionField = typeof(description))]
        [CascadeDelete(typeof(ProjectManagementClassPriority), typeof(ProjectManagementClassPriority.classId))]
        public virtual string ProjectManagementClassId
        {
            get;
            set;
        }

        [PXDBLocalizableString(255, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Description",
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Description
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Internal")]
        public virtual bool? IsInternal
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Project Issues",
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? UseForProjectIssue
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Requests For Information",
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? UseForRequestForInformation
        {
            get;
            set;
        }

        [PXDBInt(MinValue = 0)]
        [PXDefault(typeof(IIf<useForRequestForInformation.IsEqual<True>,
            int5, Null>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(useForRequestForInformation.IsEqual<True>))]
        [PXFormula(typeof(Default<useForRequestForInformation>))]
        [PXUIField(DisplayName = "Answer Days Default")]
        public virtual int? RequestForInformationResponseTimeFrame
        {
            get;
            set;
        }

        [PXDBInt(MinValue = 0)]
        [PXDefault(typeof(IIf<useForProjectIssue.IsEqual<True>,
            int5, Null>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(useForProjectIssue.IsEqual<True>))]
        [PXFormula(typeof(Default<useForProjectIssue>))]
        [PXUIField(DisplayName = "Answer Days Default")]
        public virtual int? ProjectIssueResponseTimeFrame
        {
            get;
            set;
        }

        public abstract class requestForInformationResponseTimeFrame : BqlInt.Field<requestForInformationResponseTimeFrame>
        {
        }

        public abstract class projectIssueResponseTimeFrame : BqlInt.Field<projectIssueResponseTimeFrame>
        {
        }

        public abstract class isInternal : BqlBool.Field<isInternal>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class useForProjectIssue : BqlBool.Field<useForProjectIssue>
        {
        }

        public abstract class useForRequestForInformation : BqlBool.Field<useForRequestForInformation>
        {
        }

        public abstract class projectManagementClassId : BqlString.Field<projectManagementClassId>
        {
        }
    }
}