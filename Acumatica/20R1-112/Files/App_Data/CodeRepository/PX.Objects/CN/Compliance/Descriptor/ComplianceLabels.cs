using PX.Common;

namespace PX.Objects.CN.Compliance.Descriptor
{
    [PXLocalizable]
    public class ComplianceLabels
    {
        [PXLocalizable]
        public static class Subcontract
        {
            public const string SubcontractNumber = "Subcontract Nbr.";
            public const string SubcontractTotal = "Subcontract Total";
            public const string VendorReference = "Vendor Ref.";
            public const string Date = "Date";
            public const string Status = "Status";
            public const string Vendor = "Vendor";
            public const string VendorName = "Vendor Name";
            public const string Location = "Location";
            public const string Currency = "Currency";
        }

        [PXLocalizable]
        public static class LienWaiverSetup
        {
            public const string AutomaticallyGenerateLienWaivers = "Automatically Generate Lien Waivers";
            public const string GenerateLienWaiversOn = "Generate Lien Waivers on";
            public const string ThroughDate = "Through Date";
            public const string FinalLienWaiverAmount = "Final Lien Waiver Amount";
        }
    }
}