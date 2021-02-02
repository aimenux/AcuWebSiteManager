using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.EP.GraphExtensions
{
    public class EmployeeActivitiesEntryExt : PXGraphExtension<EmployeeActivitiesEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<EmployeeActivitiesEntry.PMTimeActivityFilter.projectID>))]
        protected virtual void PMTimeActivityFilter_ProjectTaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<EPActivityApprove.projectID>))]
        protected virtual void EPActivityApprove_ProjectTaskID_CacheAttached(PXCache cache)
        {
        }
    }
}