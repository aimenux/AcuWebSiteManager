using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.RQ
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "RQ Error";
		public const string ItemIsNotInRequestClassList = "Item is not in list of Request Class {0} items.";
		public const string DontHaveAppoveRights = "You don't have access rights to aprove document";
		public const string TransferLinesNotExsist = "There are not selected lines to transfer.";
		public const string InsuffQty_LineQtyUpdated = IN.Messages.InsuffQty_LineQtyUpdated;
		public const string LineBiddingNotComplete = "Bidding Qty cannot be lower than Order Qty";
		public const string VendorIsNull = "Choose vendor for requisition.";
		public const string VendorLocationIsNull = "Choose vendor location for requisition.";
        public const string UnableDeleteRequestLine = "Item has been used in a requisition. It can't be deleted.";
		public const string UnableDeleteReqClass = "Request class is used in request items. It can't be deleted.";
		public const string MergeLinesInventoryID = "Line is excluded from merge, inventory is empty";
		public const string MergeLinesNoSource = "Line cannot be merged with others, lines separated by line source, inventory, UOM and expense account";
		public const string BiddingEmpty = "Bidding result is empty.";
		public const string VendorNotInBidding = "Vendor didn't take part in bidding.";
		public const string OrderQtyLessMinQty = "Order qty less than minimal qty specified by the vendor";
		public const string OrderQtyMoreQuoteQty = "Order qty more than quote qty specified by the vendor";
		public const string OrderQtyInsuff = "Insufficient quantity available. Order quantity was changed to match.";
		public const string ExpenseAccDefaultByDepartment = "Unable to default expense account by department for customer request class.";
		public const string SubAccDefaultByDepartment = "Unable to combine expense subaccount by department for customer request class.";
		public const string ExpenseAccDefaultByItem = "Unable to default expense account by purchase item when inventory item is hidden.";
		public const string SubAccDefaultByItem = "Unable to combine expense subaccount by purchase item when inventory item is hidden.";
        public const string OverbudgetWarning = "The request amount exceeds the budget amount.";
		public const string CheckBudgetWarning = "Check for budget exceed in request item.";
		public const string DeleteSetupClass = "Request class is used in setup. It can't be deleted.";
		public const string ShouldBeDefined = "Should be defined before order creation.";
		public const string UnableToCreateOrders = "Unable to create orders, not all required fields are defined.";
		public const string UnableToCreateSOOrders = "Unable to create SO order, require location and lot/serial information should be off.";
		public const string UnableToCreateSOOrderOrderTypeInactive = "Unable to create Sales Order. Order Type '{0}' is inactive.";
		public const string ItemListShouldBeDefined = "Item list should be defined when restriction is set.";
		public const string RequisitionCreationConfirmation = "Some of processed request items have different data (Customers, Location etc). Continue to create requisition without customer specified?";
		public const string SelectedReqClassRestriction = "Some of items in request are restricted in selected request class";
		public const string AskConfirmation = AP.Messages.AskConfirmation;
		public const string RequisitionCreated = "Requisition '{0}' created.";
		public const string CustomerUpdateConfirmation = "There are requests from another customer or from emplyees in requisition. Continue to update customer for full requisition?";
		public const string ItemReqClassRestriction = "Inventory item '{0}' is restricted for selected request class";
		public const string PurchaseOrderCreated = "Purchase Order '{0}' created.";
		public const string DropShipRequisition = "Drop ship order type allowed only for customer requisition.";
		public const string RequestBudgetCuryIDValidation = "Denominated budget account  currency is different from Request currency";
		public const string RequisitionVendorCuryIDValidation = "Selected vendor is not supported operation in currency '{0}' do you want to update requisition currency by vendor default?";
        public const string RequisitionVendorCuryRateIDValidation = "Selected vendor does not support operations in the currency rate '{0}'. Do you want to update the requisition currency rate with the vendor's default rate?";
				public const string ChooseVendorError = "Unable to process operation, some lines doesn't contain valid quotation information.";
		public const string SubcontractOrdersAreNotSupported = "Subcontract orders are not supported.";
		#endregion 

		#region Translatable Strings used in the code		
		public const string Draft = "Draft";
		public const string Request = "Request";
		public const string Quotation = "Quotation";
		public const string BiddingBtn = "Bidding";
		public const string MergeLines = "Merge Lines";
		public const string AddRequest = "Add Requested Items";
		public const string RequestDetails = "Request Details";
		public const string ViewRequest = "View Request";
		public const string ViewOrder = "View Order";
		public const string Assign = "Assign";
		public const string CreateOrders = "Create Orders";
		public const string CreateQuotation = "Create Quote";
		public const string PurchaseDetails = "Purchase Details";
		public const string ChooseVendor = "Choose Vendor";
		public const string VendorResponse = "Vendor Response";
		public const string VendorInfo = "Vendor Info";
		public const string BiddingComplete = "Complete Bidding";
		public const string UpdateResult = "Update Result";
		public const string ClearResult = "Clear Result";
		public const string SearchableTitleRequest = "Request: {0} - {2}";
		public const string SearchableTitleRequisition = "Requisition: {0} - {2}";
		#endregion

		#region Graph and Cache Names
        public const string RQSetupMaint = "Requisition Preferences";								
		public const string RQRequest = "Request";
		public const string RQRequisition = "Requisition";
        public const string RQRequestLine = "Request Line";
        public const string RQBudget = "Request Budget Line";
		public const string Approval = "Approval";
		public const string RQBiddingVendor = "Bidding Vendor";
		public const string RQRequisitionLine = "Requisition Line";
		#endregion

		#region View Names
        #endregion

		#region Order Line Type	
		public const string Document = "Document";
		public const string Items = "Items";
		public const string Annual = "Annual";
		public const string YTDValues = "YTD Values";
		public const string PTDValues = "PTD Values";
		#endregion

		#region Combo Values
		public const string MaskItem = "Inventory Item";
		public const string MaskRequester = "Requester";
		public const string MaskClass = "Request Class";
		public const string MaskDepartment = "Department";

		public const string Hold = "On Hold";
		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string Issued = "Issued";
		public const string Canceled = "Canceled";
		public const string Bidding = "Pending Bidding";
		public const string PendingQuotation = "Pending Quote";
		public const string PendingApproval = "Pending Approval";
		public const string Rejected = "Rejected";
		public const string Released = "Released";
		public const string PartiallyIssued = "Partially Issued";
		public const string PartiallyReceived = "Partially Received";
		public const string Received = "Received";
		public const string None = "None";
		public const string Warning = "Warning";
		public const string Error = "Error";
		public const string Requester = "Requester";
		public const string RequesterName = "Requester Name";
		public const string Split = "Split";
		public const string Transfer = "Transfer";
		public const string Department = "Department";
		public const string PurchaseItem = "Purchase Item";
		public const string RequestClass = "Request Class";
		public const string Ordered = "Ordered";
		public const string Requested = "Requested";
		#endregion
	}
}
