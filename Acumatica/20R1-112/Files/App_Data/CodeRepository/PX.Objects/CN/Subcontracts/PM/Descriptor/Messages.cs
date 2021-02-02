using PX.Common;

namespace PX.Objects.CN.Subcontracts.PM.Descriptor
{
    [PXLocalizable(Prefix)]
    public static class Messages
    {
        public const string Subcontract = "Subcontract";

        private const string Prefix = "PM Error";

        public static class PmTask
        {
            public const string TaskTypeIsNotAvailable = "Task Type is not valid";
        }

        public static class PmCommitment
        {
            public const string RelatedDocumentType = "Related Document Type";
            public const string PurchaseOrderType = "POOrder";
            public const string SalesOrderType = "SOOrder";
            public const string SubcontractType = "Subcontract";
            public const string PurchaseOrderLabel = "Purchase Order";
            public const string SalesOrderLabel = "Sales Order";
            public const string SubcontractLabel = "Subcontract";
        }

        public static class ChangeOrders
        {
            public const string CommitmentNbr = "Commitment Nbr.";
            public const string CommitmentLineNbr = "Commitment Line Nbr.";
        }

        public static class PmChangeOrderLine
        {
            public const string CommitmentType = "Commitment Type";
        }

        public const string CreateSubcontract = "Create Subcontract";
	}
}