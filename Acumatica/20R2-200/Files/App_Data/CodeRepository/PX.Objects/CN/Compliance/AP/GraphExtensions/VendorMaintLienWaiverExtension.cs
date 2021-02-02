using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.AP.CacheExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.GraphExtensions
{
    public class VendorMaintLienWaiverExtension : PXGraphExtension<VendorMaint>
    {
        public PXSetup<LienWaiverSetup> LienWaiverSetup;

        public virtual void _(Events.FieldUpdated<Vendor.vendorClassID> args)
        {
            if (args.Row is Vendor vendor)
            {
                var vendorExtension = PXCache<Vendor>.GetExtension<VendorExtension>(vendor);
                args.Cache.RaiseFieldDefaulting<VendorExtension.shouldGenerateLienWaivers>(
                    vendor, out var shouldGenerateLienWaivers);
                vendorExtension.ShouldGenerateLienWaivers = shouldGenerateLienWaivers as bool?;
            }
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
    }
}