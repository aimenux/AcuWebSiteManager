using PX.Data;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class TemplateGlobalTaskMaintExt : PXGraphExtension<TemplateGlobalTaskMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected virtual void _(Events.RowInserting<PMTask> args)
        {
            var task = args.Row;
            if (task == null)
            {
                return;
            }
            task.Type = ProjectTaskType.CostRevenue;
        }
    }
}