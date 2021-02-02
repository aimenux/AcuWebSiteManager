using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.AR.GraphExtensions
{
    public class ArInvoiceEntryExt : PXGraphExtension<ARInvoiceEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() && !SiteMapExtension.IsInvoicesScreenId();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.cost>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        protected virtual void ARTran_TaskID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.RowUpdated<ARTran> args)
        {
            if (args.Row is ARTran line)
            {
                object taskId = line.TaskID;
                try
                {
                    args.Cache.RaiseFieldVerifying<ARTran.taskID>(line, ref taskId);
                }
                catch (PXSetPropertyException)
                {
                    line.TaskID = null;
                }
            }
        }
    }
}