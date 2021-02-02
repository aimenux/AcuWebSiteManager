using PX.Data;
using PX.Objects.CN.Mobile.EP.Descriptor.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;

namespace PX.Objects.CN.Mobile.EP.GraphExtensions.TimeCardMaintExt
{
    public class TimeCardMaintExt : PXGraphExtension<TimeCardMaint>
    {
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}


		[MobileProjectCostCode(typeof(TimeCardMaint.EPTimeCardSummaryWithInfo.projectTaskID), AccountType.Expense)]
        protected virtual void EPTimeCardSummaryWithInfo_CostCodeID_CacheAttached(PXCache cache)
        {
        }

        [MobileProjectCostCode(typeof(TimeCardMaint.EPTimecardDetail.projectTaskID), AccountType.Expense)]
        protected virtual void EPTimeCardDetail_CostCodeID_CacheAttached(PXCache cache)
        {
        }

        [MobileProjectCostCode(typeof(EPTimeCardItem.taskID), AccountType.Expense)]
        protected virtual void EPTimeCardItem_CostCodeID_CacheAttached(PXCache cache)
        {
        }
    }
}