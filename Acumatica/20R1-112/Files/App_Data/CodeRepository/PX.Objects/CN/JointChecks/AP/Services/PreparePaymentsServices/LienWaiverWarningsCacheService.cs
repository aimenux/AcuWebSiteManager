using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.CN.JointChecks.AP.Services.PreparePaymentsServices
{
    public class LienWaiverWarningsCacheService
    {
        private const string Key = "LienWaiver";

        private List<APAdjust> Caches
        {
            get;
        } = new List<APAdjust>();

        public static void Add(APAdjust adjustment)
        {
            PXDatabase.GetSlot<LienWaiverWarningsCacheService>(Key).Caches.Add(adjustment);
        }

        public static bool ShouldShowWarning(APAdjust adjustment)
        {
            return PXDatabase.GetSlot<LienWaiverWarningsCacheService>(Key).Caches.Contains(adjustment);
        }
    }
}
