using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.CN.JointChecks.AP.Services.PreparePaymentsServices
{
    public class PreparePaymentsAdjustmentsCache
    {
        private const string Key = "PreparePaymentsAdjustmentsCache";

        private List<APAdjust> Caches
        {
            get;
        } = new List<APAdjust>();

        public static void Add(IEnumerable<APAdjust> adjustments)
        {
            PXDatabase.GetSlot<PreparePaymentsAdjustmentsCache>(Key).Caches.AddRange(adjustments);
        }

        public static List<APAdjust> GetStoredAdjustments()
        {
            return PXDatabase.GetSlot<PreparePaymentsAdjustmentsCache>(Key).Caches;
        }

        public static void ClearStoredAdjustments()
        {
            PXDatabase.GetSlot<PreparePaymentsAdjustmentsCache>(Key).Caches.Clear();
        }
    }
}