using PX.Common;

namespace PX.Objects.CN.Subcontracts.PO.Descriptor
{
    [PXLocalizable(Prefix)]
    public static class Messages
    {
        public const string InvalidAssignmentMap = "Invalid assignment map";
        public const string OnlyPurchaseOrdersAreAllowedMessage = "Only Purchase Orders are allowed.";
        public const string NoteFilesFieldName = "NoteFiles";

        private const string Prefix = "PO Error";

        public static class PoSetup
        {
            public const string SubcontractNumberingName = "SUBCONTR";
            public const string SubcontractNumberingId = "Subcontract Numbering Sequence";
            public const string RequireSubcontractControlTotal = "Validate Total on Entry";
            public const string SubcontractRequireApproval = "Require Approval";
        }
    }
}