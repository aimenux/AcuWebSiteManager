using PX.Common;

namespace PX.Objects.CN.Subcontracts.SC.Descriptor
{
    [PXLocalizable(Prefix)]
    public static class Messages
    {
        public const string InvalidInventoryItemMessage = "Item should not require receipt.";
        public const string ViewSubcontract = "View Subcontract";
        public const string SubcontractsPreferencesScreenName = "Subcontracts Preferences";
        public const string SubcontractClassId = "SUBCONTRACTS";
        public const string AttributeDeleteWarningHeader = "Warning";
        public const string SubcontractLineLinkedToSalesOrderLine = "Subcontract line linked to Sales Order Line";

        public const string SubcontractStartDateChangeConfirmation = "Changing of the subcontract 'Start date' will " +
            "reset 'Start Date' dates for all it's details to their default values. Continue?";

        public const string SubcontractDateChangeConfirmation = "Changing of the subcontract date will reset the " +
            "'Requested' and 'Start Date' dates for all order lines to new values. Do you want to continue?";

        public const string ProjectCommitmentsLocked = "To be able to create original Subcontract commitments for " +
            "this project, perform the Unlock Commitments action for the project on the Projects (PM301000) form.";

        public const string CanNotDeleteWithChangeOrderBehavior = "The subcontract cannot be removed: change order " +
            "workflow has been enabled for the document because it contains lines related to projects with change " +
            "order workflow enabled.";

        public const string SubcontractHasReceiptsAndCannotBeDeleted = "The subcontract cannot be deleted because " +
            "some quantity of items for this subcontract have been received.";

        public const string SubcontractHasBillsReleasedAndCannotBeDeleted = "The subcontract cannot be deleted " +
            "because there is at least one AP bill has been released for this subcontract.";

        public const string SubcontractHasBillsGeneratedAndCannotBeDeleted = "The subcontract cannot be deleted " +
            "because one or multiple AP bills have been generated for this order. To proceed, delete AP bills first.";

        public const string ItemIsNotPresentedInTheProjectBudget =
            "The item is not associated with the cost budget of the specified project.";

        private const string Prefix = "SC Error";

        public static class Subcontract
        {
            public const string CacheName = "Subcontract";
            public const string SubcontractNumber = "Subcontract Nbr.";
            public const string StartDate = "Start Date";
            public const string SubcontractTotal = "Subcontract Total";
            public const string Project = "Project";
            public const string ReceivedQty = "Received Qty.";
            public const string Vendor = "Vendor";
            public const string Location = "Location";
            public const string Date = "Date";
            public const string Status = "Status";
            public const string Currency = "Currency";
            public const string VendorReference = "Vendor Ref.";
            public const string LineTotal = "Line Total";
            public const string SalesOrderType = "Sales Order Type";
            public const string SalesOrderNumber = "Sales Order Nbr.";
            public const string Description = "Description";
            public const string Owner = "Owner";
        }

        public static class SubcontractLineInventoryItemAttribute
        {
            public const string LineItemNotPurchased = "Item cannot be Purchased";
            public const string LineItemReserved = "Item reserved for Project Module to represent N/A item.";
        }
    }

    [PXLocalizable]
    public static class InfoMessages
    {
        public const string SubcontractWillBeDeleted = "The current subcontract record will be deleted.";
    }
}