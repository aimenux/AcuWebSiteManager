using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.Common.CacheExtensions
{
    public sealed class PmTaskExt : PXCacheExtension<PMTask>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [ClearRelatedEntities(
            typeof(RequestForInformation.projectTaskId),
            typeof(ProjectIssue.projectTaskId),
            typeof(DrawingLog.projectTaskId))]
        public int? TaskID
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
    }
}
