using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    [Serializable]
    [PXPrimaryGraph(typeof(ProjectManagementClassMaint))]
    [PXCacheName(CacheNames.ProjectManagementClassPriority)]
    public class ProjectManagementClassPriority : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public int? PriorityId
        {
            get;
            set;
        }

        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Search<ProjectManagementClass.projectManagementClassId,
            Where<ProjectManagementClass.projectManagementClassId,
                Equal<Current<ProjectManagementClass.projectManagementClassId>>>>))]
        public string ClassId
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public bool? IsActive
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Priority Name",
            Visibility = PXUIVisibility.SelectorVisible)]
        public string PriorityName
        {
            get;
            set;
        }

        [PXDBInt(MinValue = 1)]
        [PXUIField(DisplayName = "Sort Order")]
        public int? SortOrder
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Default")]
        public bool? IsDefault
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        public bool? IsSystemPriority
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        public bool? IsHighestPriority
        {
            get;
            set;
        }

        public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

        public abstract class classId : BqlString.Field<classId>
        {
        }

        public abstract class isActive : BqlBool.Field<isActive>
        {
        }

        public abstract class priorityName : BqlString.Field<priorityName>
        {
        }

        public abstract class sortOrder : BqlInt.Field<sortOrder>
        {
        }

        public abstract class isDefault : BqlBool.Field<isDefault>
        {
        }

        public abstract class isSystemPriority : BqlBool.Field<isSystemPriority>
        {
        }

        public abstract class isHighestPriority : BqlBool.Field<isHighestPriority>
        {
        }
    }
}