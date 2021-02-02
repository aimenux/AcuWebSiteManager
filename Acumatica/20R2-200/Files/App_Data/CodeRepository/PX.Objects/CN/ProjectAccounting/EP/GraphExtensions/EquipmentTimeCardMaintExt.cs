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
    public class EquipmentTimeCardMaintExt : PXGraphExtension<EquipmentTimeCardMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<EPEquipmentSummary.projectID>))]
        protected virtual void _(Events.CacheAttached<EPEquipmentSummary.projectTaskID> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        [PXFormula(typeof(Validate<EPEquipmentDetail.projectID>))]
        protected virtual void _(Events.CacheAttached<EPEquipmentDetail.projectTaskID> e)
        {
        }
    }
}
