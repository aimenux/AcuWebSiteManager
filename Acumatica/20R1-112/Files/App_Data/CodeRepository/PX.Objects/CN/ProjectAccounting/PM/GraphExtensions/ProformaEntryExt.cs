using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class ProformaEntryExt : PXGraphExtension<ProformaEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
			ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
		[PXFormula(typeof(Validate<PMProformaLine.costCodeID, PMProformaLine.description>))]
		protected virtual void PMProformaTransactLine_TaskID_CacheAttached(PXCache cache)
		{
		}
	}
}