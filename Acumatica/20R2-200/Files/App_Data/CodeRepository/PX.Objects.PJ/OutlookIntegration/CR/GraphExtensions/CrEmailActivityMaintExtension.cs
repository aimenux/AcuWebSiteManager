using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.OutlookIntegration.CR.GraphExtensions
{
    public class CrEmailActivityMaintExtension : PXGraphExtension<CREmailActivityMaint>
    {
        public SelectFrom<PMTimeActivity>.View TimeActivities;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [ProjectTask(typeof(PMTimeActivity.projectID), "TA", DisplayName = "Project Task", AllowNull = true)]
        public void _(Events.CacheAttached<PMTimeActivity.projectTaskID> args)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [CostCode(null, typeof(PMTimeActivity.projectTaskID), "E",
            ReleasedField = typeof(PMTimeActivity.released), AllowNullValue = true)]
        public void _(Events.CacheAttached<PMTimeActivity.costCodeID> args)
        {
        }
    }
}