using PX.Common;

namespace PX.Objects.CN.Subcontracts.AP.Descriptor
{
    [PXLocalizable(Prefix)]
    public class Messages
    {
        public const string AddSubcontracts = "Add Subcontracts";
        public const string AddSubcontract = "Add Subcontract";
        public const string AddSubcontractLine = "Add Subcontract Line";
        public const string ViewSubcontract = "View Subcontract";
        public const string ViewPoOrder = "View PO Order";
        public const string SubcontractViewName = "Subcontract";

        public const string FailedToAddSubcontractLinesError =
            "SC Error: Failed to add one or more lines from the Subcontract. Please check the Trace for details.";

        public const string AutoApplyRetainageCheckBox = "The Apply Retainage check box is selected automatically " +
            "because you have added one or more lines with a retainage from the purchase order or subcontract.";

        private const string Prefix = "AP Error";

        public static class LinkLineFilterMode
        {
            public const string PurchaseOrderOrSubcontract = "Purchase Order / Subcontract";
        }

        public static class Subcontract
        {
            public const string SubcontractNumber = "Subcontract Nbr.";
            public const string SubcontractTotal = "Subcontract Total";
            public const string Project = "Project";
            public const string SubcontractLine = "Subcontract Line";
            public const string SubcontractDate = "Subcontract Date";
        }

        public static class FieldClass
        {
            public const string Distribution = "DISTR";
        }
    }
}
