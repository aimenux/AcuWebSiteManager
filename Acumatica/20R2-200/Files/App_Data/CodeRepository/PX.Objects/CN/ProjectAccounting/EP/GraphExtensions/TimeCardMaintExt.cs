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
    public class TimeCardMaintExt : PXGraphExtension<TimeCardMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<TimeCardMaint.EPTimeCardSummaryWithInfo.projectID>))]
        protected virtual void _(Events.CacheAttached<TimeCardMaint.EPTimeCardSummaryWithInfo.projectTaskID> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<TimeCardMaint.EPTimecardDetail.projectID>))]
        protected virtual void _(Events.CacheAttached<TimeCardMaint.EPTimecardDetail.projectTaskID> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<EPTimeCardItem.projectID>))]
        protected virtual void _(Events.CacheAttached<EPTimeCardItem.taskID> e)
        {
        }
    }
}
