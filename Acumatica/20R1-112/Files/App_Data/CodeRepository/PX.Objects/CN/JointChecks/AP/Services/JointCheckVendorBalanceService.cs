using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services
{
    public class JointCheckVendorBalanceService
    {
        public void UpdateVendorBalanceDisplayName(APRegister invoice, PXCache cache)
        {
            var isRetainageGroup = invoice.IsRetainageDocument.GetValueOrDefault() ||
                invoice.RetainageApply.GetValueOrDefault();
            var displayName = isRetainageGroup
                ? JointCheckLabels.MaxAvailableAmount
                : JointCheckLabels.VendorBalance;
            var fieldAttributes = cache.GetAttributesOfType<PXUIFieldAttribute>(
                invoice, typeof(APInvoiceJCExt.vendorBalance).Name);
            fieldAttributes.Where(attribute => attribute != null).ForEach(x => x.DisplayName = displayName);
        }
    }
}