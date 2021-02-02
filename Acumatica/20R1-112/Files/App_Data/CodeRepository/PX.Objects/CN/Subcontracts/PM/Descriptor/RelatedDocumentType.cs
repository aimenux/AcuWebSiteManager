using PX.Data;

namespace PX.Objects.CN.Subcontracts.PM.Descriptor
{
    public class RelatedDocumentType
    {
        public const string AllCommitmentsType = "AllCommitments";
        public const string PurchaseOrderType = "POOrder";
        public const string SubcontractType = "Subcontract";

        public const string AllCommitmentsLabel = "All Commitments";
        public const string PurchaseOrderLabel = "Purchase Order";
        public const string SubcontractLabel = "Subcontract";

        private static readonly string[] RelatedDocumentTypes =
        {
            AllCommitmentsType,
            PurchaseOrderType,
            SubcontractType
        };

        private static readonly string[] RelatedDocumentLabels =
        {
            AllCommitmentsLabel,
            PurchaseOrderLabel,
            SubcontractLabel
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(RelatedDocumentTypes, RelatedDocumentLabels)
            {
            }
        }
    }
}
