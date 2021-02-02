using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.PO
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "PO Error";
		public const string DocumentOutOfBalance = "Document is out of balance.";
        public const string AssignNotSetup = "Default Purchase Order Assignment Map is not entered in PO setup";
        public const string AssignNotSetup_Receipt = "Default Purchase Order Receipt Assignment Map is not entered in PO setup";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
        public const string POOrderLineHasReceiptsAndCannotBeDeleted = "The line cannot be deleted because some quantity of an item in this line have been received.";
        public const string POOrderHasReceiptsAndCannotBeDeleted = "The order cannot be deleted because some quantity of items for this purchase order have been received.";
        public const string POOrderHasBillsReleasedAndCannotBeDeleted = "The order cannot be deleted because there is at least one AP bill has been released for this order. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
        public const string POOrderLineHasBillsReleasedAndCannotBeDeleted = "The order line cannot be deleted because there is at least one AP bill has been released for this order line. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
        public const string POOrderHasBillsGeneratedAndCannotBeDeleted = "The order cannot be deleted because one or multiple AP bills have been generated for this order. To proceed, delete AP bills first. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
        public const string POOrderLineHasBillsGeneratedAndCannotBeDeleted = "The order line cannot be deleted because one or multiple AP bills have been generated for this order line. To proceed, delete AP bills first. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
		public const string ReceiptLineQtyGoNegative = "Receipt quantity will go negative source PO Order line";	
		public const string ReceiptLineQtyDoesNotMatchMinPOQuantityRules = "Receipt quantity is below then minimum quantity defined in PO Order for this item";
		public const string ReceiptLineQtyDoesNotMatchMaxPOQuantityRules = "Receipted quantity is above the maximum quantity defined in PO Order for this item";
		public const string LandedCostReceiptTotalCostIsZero = "This Landed Cost is allocated by Amount , but Receipt total Amount is equal to zero";
		public const string LandedCostReceiptTotalVolumeIsZero = "This Landed Cost is  allocated by Volume, but Receipt total Volume is equal to zero";
		public const string LandedCostReceiptTotalWeightIsZero = "This Landed Cost is allocated by Weight , but Receipt total Weight is equal to zero";
		public const string LandedCostReceiptTotalQuantityIsZero = "This Landed Cost is allocated by Quantity , but Receipt total Quantity is equal to zero";
		public const string LandedCostReceiptNoReceiptLines = "This Receipt does not have detail lines";
		public const string LandedCostUnknownAllocationType = "Unknown Landed Cost alloction type";
		public const string LandedCostProcessingForOneOrMorePOReceiptsFailed = "At least one of the Document selected has failed to process";
		public const string SourcePOOrderExpiresBeforeTheDateOfDocument = "Originating Blanket Order# {1} expires before the date of current Document - on {0:d}";
		public const string POBlanketOrderExpiresBeforeTheDateOfDocument = "Blanket Order# {1} expires before the date of currend Document- on {0:d}";
		public const string POBlanketOrderExpiresEarlierThanTheOrderDate = "The Expires On date is set to a date in the past.";
		public const string PostingClassIsNotDefinedForTheItemInReceiptRow = "Posting class is not defined for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}";
        public const string ShippingContactMayNotBeEmpty = "The document cannot be saved, shipping contact is not specified  for the warehouse selected  on the Shipping Instructions tab.";
        public const string ShippingAddressMayNotBeEmpty = "The document cannot be saved, shipping address is not specified  for the warehouse selected  on the Shipping Instructions tab.";
        public const string LCVarianceAccountCanNotBeFoundForItemInReceiptRow = "Landed Cost Variance Account can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
		public const string LCVarianceSubAccountCanNotBeFoundForItemInReceiptRow = "Landed Cost Variance Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
        public const string COGSAccountCanNotBeFoundForItemInReceiptRow = "GOGS Account can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
        public const string COGSSubAccountCanNotBeFoundForItemInReceiptRow = "COGS Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
		public const string POAccrualSubAccountCanNotBeFoundForItemInReceiptRow = "PO Accrual Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
		public const string LCInventoryItemInReceiptRowIsNotFound = "Inventory Item '{0}' used in PO Receipt# '{1}' line {2} is not found in the system";
		public const string UnknownLCAllocationMethod = "Unknown Landed Cost Allocation Method for Landed Cost Code '{0}'";
		public const string POReceiptLineDoesNotReferencePOOrder = "This line does not reference PO Order";
		public const string INReceiptMustBeReleasedBeforeLCProcessing = "IN Receipt# '{0}' created from PO Receipt# '{1}' must be released before the Landed Cost may be processed";
		public const string OrderLineQtyExceedsQuantityInBlanketOrder = "Order Quantity is above the  quantity {0} defined in the Blanket Order# '{1}' for this item";
		public const string QuantityReceivedForOrderLineExceedsOrdersQuatity = "Quantity received is already above the order's quantity for this line";
		public const string ReleaseOfOneOrMoreReceiptsHasFailed = "Release of one or more PO Receipts  has failed";
		public const string PurchaseOrderHasTypeDifferentFromOthersInPOReceipt = "Purchase Order {0} {1} cannot be added - it has a type different from other orders in this Receipt";
		public const string PurchaseOrderOnHoldWithReceipt = "The purchase order {0} {1} cannot be put on hold. It has unreleased receipt {2}.";
        public const string PurchaseOrderHasShipDestinationDifferentFromOthersInPOReceipt = "Purchase Order {0} {1} cannot be added because it has a different shipping destination than other orders in this receipt.";
		public const string SomeOrdersMayNotBeAddedTypeOrShippingDestIsDifferent = "Selected Purchase Orders cannot be added. Selected Orders must have same Currency, Type and Shipping Destinations.";
		public const string PurchaseOrderHasCurrencyDifferentFromPOReceipt = "Purchase Order {0} {1} has currency other then one of current Purchase Receipt";
        public const string LandedCostCannotBeDistributed = "Landed Cost is not applicable for selected transfers";
		
		public const string POReceiptFromOrderCreation_NoApplicableLinesFound = "There are no lines in this document that may be entered in PO Receipt Document";
		public const string APInvoicePOOrderCreation_NoApplicableLinesFound = "There are no lines in this document that may be entered in AP Bill Document directly";
		public const string ReceiptNumberBelongsToDocumentHavingDifferentType = "Document with number {0} already exists but it has a type {1}. Document Number must be unique for all the PO Receipts regardless of the type";
		public const string UnreasedAPDocumentExistsForPOReceipt = "There is one or more unreleased AP documents created for this document. They must be released before another AP Document may be created";
		public const string UnreleasedAPDocumentExistsForPOLines = "There is at least one unreleased AP document {0} prepared for this purchase receipt. To create new AP document, remove or release the unreleased AP document.";
	    public const string AllTheLinesOfPOReceiptAreAlreadyBilled = "AP documents are already created for all the lines of this document.";
		public const string UnitCostShouldBeNonZeroForStockItems = "Unit Cost should not be 0 for the stock items";
		public const string QuantityReducedToBlanketOpen = "Order Quantity reduced to Blanket Order: '{0}' Open Qty. for this item";
		public const string LCCodeUsesNonLCVendor = "This Code uses a vendor which has 'Landed Cost Vendor' set to 'off'. You should correct vendor or use another one";
		public const string POOrderHasUnreleaseReceiptsAndCantBeCancelled = "This Order can not be cancelled - there is one or more unreleased PO Receipts referencing it";
		public const string POLineHasUnreleaseReceiptsAndCantBeCompletedOrCancelled = "This line can not be completed or cancelled - there is one or more unreleased PO Receipts referencing it";
        public const string POSourceNotFound = "There are no lines in open purchase orders that match specified criteria.";
        public const string INSourceNotFound = "There are no unreceived lines in transfer orders that match specified criteria.";
		public const string BarCodeAddToItem = "Barcode will be added to inventory item.";
		public const string POOrderPromisedDateChangeConfirmation = "Changing of the purchase order 'Promised on' date will reset 'Promised' dates for all it's details to their default values. Continue?";
        public const string POOrderOrderDateChangeConfirmation = "Changing of the purchase order date will reset the 'Requested' and 'Promised' dates for all order lines to new values. Do you want to continue?";
		public const string POOrderPromisedDateChangedAutomatically = "The promised date has been changed from {0} to {1}.";
		public const string INDocumentFailedToReleaseDuringPOReceiptRelease = "IN Document has been created but failed to release with the following error: '{0}'. Please, validate a created IN Document";
		public const string INDocumentFailedToReleaseDuringPOReceiptReleaseInTransaction = "IN Document failed to release with the following error: '{0}'.";
        public const string APDocumentFailedToReleaseDuringPOReceiptRelease = "Release of AP document failed with the following error: '{0}'. Please validate the AP document";
        public const string LandedCostFailedToUpdateDuringPOReceiptRelease = "Update of landed cost document failed with the following error: '{0}'. Validate the landed cost document.";
		public const string APDocumentNotCreatedAllBilled = "AP document was not created because all lines have already been billed.";
        public const string POOrderQtyValidation = "Insufficient quantity requested. Line quantity was changed to match.";		
		public const string POLineQuantityMustBeGreaterThanZero = "Quantity must be greater than 0";
		public const string BinLotSerialNotAssigned = IN.Messages.BinLotSerialNotAssigned;
		public const string ReceiptAddedForPO = "Item {0}{1} receipted {2} {3} for Purchase Order {4}";
		public const string ReceiptAdded = "Item {0}{1} receipted {2} {3}";
		public const string Availability_Info = IN.Messages.Availability_Info;
		public const string VendorIsNotLandedCostVendor = "Vendor is not landed cost vendor.";
		public const string UnsupportedLineType = "Selected line type is not allowed for current inventory item.";
		public const string POVendorInventoryDuplicate = "Unable to propagate selected item for all locations there's another one.";
		public const string UnitCostMustBeNonNegativeForStockItem = "A value for the Unit Cost must not be negaive for Stock Items";
		public const string POOrderTotalAmountMustBeNonNegative = "'{0}' may not be negative";
        public const string LineCannotBeReceiptedTwicePO = "The item in this transfer line is already received according to the '{1}' line of the '{0}' PO receipt.";
        public const string LineCannotBeReceiptedTwiceIN = "The item in this transfer line is already received according to the '{1}' line of the '{0}' IN transaction.";
        public const string DiscountGreaterLineTotal = "Discount Total may not be greater than Line Total";
		[Obsolete]
		public const string PrepaymentNotUpdated = "Prepayment was not updated: ";

		public const string PPVAccountCanNotBeFoundForItemInReceiptRow = "Purchase Price Variance Account can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the Reason Code '{3}'";
		public const string PPVSubAccountCanNotBeFoundForItemInReceiptRow = "Purchase Price Variance Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the Reason Code '{3}', Inventory Item and Warehouse '{4}'";
		public const string PPVInventoryItemInReceiptRowIsNotFound = "Inventory Item '{0}' used in PO Receipt# '{1}' line {2} is not found in the system";
		public const string ThisEntityNotBeDeletedBecauseItIsUsedIn = "This '{0}' cannot be deleted because it is used in '{1}'.";
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2020R2)]
		public const string DeductibleVATNotAllowed = "Deductible VAT is not supported for stock items and non-stock items that require receipts";
		public const string ProjectTaskIsNotAssociatedWithAnyInLocation = "Given Project Task is not associated with any Warehouse Location and cannot be used with a Stock-Item. Either select another Project Task or map it to the warehouse location.";
		public const string ProjectTaskIsAssociatedWithAnotherWarehouse = "Given Project Task is associated with another Warehouse - '{0}'. Either change the warehouse or select another Project Task.";
		public const string LocationIsNotMappedToTaskWarning = "Given location is not associated with Project Task selected on Purchase Order line.";
		public const string LocationIsNotMappedToTaskError = "Given location is not associated with Project Task selected on Purchase Order line. Since current document explicitly requires approval and was approved Project Task cannot be changed on the Receipt.";
		public const string TaskDiffersWarning = "Given Task differs from the Project Task selected on Purchase Order line.";
		public const string TaskDiffersError = "Given Task differs from the Project Task selected on Purchase Order line. Since Purchase Order document explicitly requires approval and was approved Project Task cannot be changed on the Receipt.";
		public const string DefaultVendorDeactivated = "Default Vendor cannot be deactivated.";
		public const string DuplicateSerialNumbers = "One or more records with duplicate serial numbers found in the document.";
		public const string ContainsDuplicateSerialNumbers = "Contains duplicate serial numbers.";
		public const string POLineLinkedToSOLine = "Deletion of the purchase order line will unlink sales order '{0}' from this purchase order. Do you want to continue?";
		public const string ChangingInventoryForLinkedRecord = "This line is linked to a sales order line. To replace the item in this line, you have to replace it in the linked sales order first.";
		public const string CustomerCannotBeChangedForOrderWithActiveDemand = "A customer to whom the drop-ship order is shipped must match the customer specified in the linked sales order.";
		public const string POLineLinkedToSO = "The line cannot be reopened because it is linked to a sales order.";
		public const string ProjectIsNotActive = "Project is not Active.";
		public const string ProjectTaskIsNotActive = "Project Task is not Active.";
		public const string LineIsInvalid = "One or more records in the document is invalid.";
		public const string FailedToAddLine = "Failed to add one or more lines from the PO order. Please check the Trace for details.";
		public const string PayToVendorHasDifferentCury = "The currency '{1}' of the pay-to vendor '{0}' differs from currency '{2}' of the document.";
		public const string PossibleOverbillingAPNormalPO = "Changing this setting could lead to overbilling open service lines in the purchase orders for which you have already created AP bills. Please see the list of the affected purchase orders in the trace log.";
		public const string PossibleOverbillingAPDSPO = "Changing this setting could lead to overbilling open service lines in the drop-ship purchase orders for which you have already created AP bills. Please see the list of the affected drop-ship purchase orders in the trace log.";
		public const string PossibleOverbillingPRNormalPO = "Changing this setting could lead to overbilling open service lines in the purchase orders for which you have already created purchase receipts. Please see the list of the affected purchase orders in the trace log.";
		public const string PossibleOverbillingPRDSPO = "Changing this setting could lead to overbilling open service lines in the drop-ship purchase orders for which you have already created purchase receipts. Please see the list of the affected drop-ship purchase orders in the trace log.";
		public const string PossibleOverbillingTraceList = "List of open service lines that are partially billed or receipted (Top 1000):";
		public const string PossibleOverbillingTraceMessage = "Order Type: {0}, Order Nbr.: {1}, Line Nbr.: {2}, Inventory ID: {3}";
		public const string InvalidApplicationAllocationCombination = "The landed cost cannot be applied from an AP bill because the allocation method is set to None.";
		public const string InvalidAllocationMethod = "The landed cost cannot be applied from the AP bill because the allocation method is set to None.";
		public const string AutoAppliedRetainageFromOrder = "The Apply Retainage check box is selected automatically because you have added one or more lines with a retainage from the purchase order.";
		public const string LocationNotAssignedToProject = "The selected location is not assigned to the {0} project that is specified in the purchase order. Please specify another location.";
		public const string TransferUsedInUnreleasedReceipt = "The transfer order cannot be selected because it is used in the unreleased transfer receipt {0}";
		public const string CanNotDeleteWithChangeOrderBehavior = "The purchase order cannot be removed: change order workflow has been enabled for the document because it contains lines related to projects with change order workflow enabled.";
		public const string SORequestedDateEarlier = "The Requested On date in the corresponding sales order {0} is {1}.";
		public const string ManualDiscountsWillNotBeCopiedToAP = "Manual discounts will not be transferred to the AP bill.";
		public const string SelectLinesForProcessing = "Select lines for processing.";
		public const string ReturnedQtyMoreThanReceivedQty = "The specified Returned Qty. exceeds the Received Qty. specified in the original PO receipt.";
		public const string UnreleasedReturnExistsForPOReceipt = "There is at least one unreleased return {0} prepared for this purchase receipt. To create new purchase return, remove or release the unreleased purchase return.";
		public const string GoodsLineWithoutOrigLinkCantReturnByOrigCost = "Return by original receipt cost cannot be processed because there is at least one line not linked to original receipt (#{0}).";
		public const string OrigINReceiptMustBeReleased = "Original inventory receipt {0} must be released prior to return.";
		public const string ReceiptLineIsCompletelyReturned = "The line is already completely returned.";
		public const string AccrualAcctUsedInAPBill = "Cannot change accrual account because the line is used in the AP Document {0}, {1}.";
		public const string AccrualAcctUsedInPOReceipt = "Cannot change accrual account because the line is used in the PO Receipt {0}, {1}.";
		public const string INIssueMustReleasedPriorBilling = "IN Issue for the Purchase Return must be released prior to billing.";
		public const string SomeLinesSkippedBecauseUom = "A line from the PO receipt related to the same PO line as this line has not been added to the bill because the quantity of items is in different UOM.";
		public const string SomeLinesAggregated = "The quantity of items in this line has been summed with the quantity of items related to the same PO line from the added PO receipt or receipts.";
		public const string LineDelinkedFromBlanket = "This line is no longer linked to the {0} blanket PO because the inventory ID was changed.";

		public const string LandedCostsDetailsEmpty = "Landed Costs document details are empty.";
		public const string LandedCostsCantReleaseWoDetails = "Cannot release landed costs without document detail lines.";
		public const string LandedCostsDetailCannotDelete = "Landed costs with a linked AP bill cannot be deleted.";
		public const string LandedCostsFailCreateAdj = "IN Document has not been created with the following error: {0}";
		public const string LandedCostsFailCreateInvoice = "AP Document has not been created with the following error: {0}";
		public const string LandedCostsFailReleaseAdj = "IN Document failed to release with the following error: {0}";
		public const string LandedCostsFailReleaseInvoice = "AP Document failed to release with the following error: {0}";
		public const string LandedCostsManyErrors = "Landed costs release has many errors. Check Trace for details.";
		public const string LandedCostsCannotReleaseWithUnreleasedReceipts = "Cannot release landed costs with unreleased receipts.";
		public const string PostingTaxToInventoryThroughLandedCostsIsNotSupported = "Cannot save the landed cost document because the tax with the {0} tax ID has the Use Tax Expense Account check box cleared and thus cannot be applied to the document. For processing taxable landed costs, configure a separate tax ID with the Use Tax Expense Account check box selected.";

		public const string POLineOverbillingNotAllowed = "Cannot release the bill because the billed quantity of the related line in the {0} purchase order cannot exceed {1}. Correct the {2} line of the purchase order or remove the {3} line from the bill.";
		public const string PrepaidQtyCantExceedPOLine = "The prepaid quantity cannot exceed the quantity in the corresponding line of the purchase order.";
		public const string PrepaidAmtCantExceedPOLine = "The prepaid amount cannot exceed the amount in the corresponding line of the purchase order.";
		public const string PrepaidAmtMayExceedPOLine = "The {0} purchase order already has a partial prepayment. Make sure that the amount of the current prepayment does not exceed the purchase order amount.";
		public const string SiteWithoutDropShipLocation = "The selected warehouse has no drop-ship location. Please select another warehouse or define the drop-ship location for the currently selected warehouse.";
		public const string POHasNoItemsToReceive = "The purchase order {0} does not contain any items to be received.";
		public const string BillCannotReverseItHasNotReleasedINAdjustment = "Cannot reverse the bill because the {0} IN adjustment linked to the {1} bill is not released. Release the adjustment first to be able to reverse the bill.";
		public const string DetailSiteDiffersFromShipping = "The warehouse in the line differs from the warehouse on the Shipping Instructions tab. The items will be delivered to the {0} warehouse.";

		#endregion

		#region Translatable Strings used in the code

		public const string AskConfirmation = "Confirmation";
		public const string Warning = "Warning";
		public const string ReprintCaption = "Reprint";
		public const string PurchaseOrder = "Purchase Order";
		public const string Assign = CR.Messages.Assign;
		public const string Release = "Release";
		public const string AddPOOrder = "Add PO";
		public const string AddTransfer = "Add Transfer";
        public const string AddTransferLine = "Add Transfer Line";
		public const string AddBlanketOrder = "Add Blanket PO";
		public const string ViewPOOrder = "View PO";
		public const string AddPOOrderLine = "Add PO Line";
		public const string AddBlanketLine = "Add Blanket PO Line";
		public const string AddPOReceipt = "Add PO Receipt";
		public const string AddPOReceiptLine = "Add PO Receipt Line";
		public const string AddLine = "Add Line";
		public const string AddReceipt = "Add PR";
		public const string AddReceiptLine = "Add PR Line";
		public const string ViewINDocument = "View IN Document";
		public const string ViewAPDocument = "View AP Document";
		public const string ViewPODocument = "View PO Document";
		public const string NewVendor = "New Vendor";
		public const string ViewDemand = "View SO Demand";
		public const string EditVendor = "Edit Vendor";
		public const string NewItem = "New Item";
		public const string EditItem = "Edit Item";
		public const string ReportPOReceiptBillingDetails = "Purchase Receipt Billing Details";
		public const string ReportPOReceipAllocated = "Purchase Receipt Allocated and Backordered";
		public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string Document = "Document";
		public const string CreatePOReceipt = "Enter Receipt";
		public const string CreateAPInvoice= "Enter AP Bill";
		public const string CreateLandedCostDocument= "Enter Landed Costs";
		public const string CreateReturn = "Return";
		public const string POReceiptRedirection = "Switch to PO Receipt";
		public const string AllowComplete = "Allow Complete";
		public const string AllowOpen = "Allow Open";
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider;
		public const string ExternalTaxVendorNotFound = TX.Messages.ExternalTaxVendorNotFound;
		public const string FailedGetFromAddressSO = SO.Messages.FailedGetFromAddressSO;
		public const string FailedGetToAddressSO = SO.Messages.FailedGetToAddressSO;
		public const string POTypeNbrLineNbrMustBeFilled = "PO line number, PO type, and PO number must be filled in for the system to add the purchase order to the details.";
		public const string PurchaseOrderLineNotFound = "The purchase order line was not found.";
		public const string TransferOrderNotFound = "The transfer order was not found.";
		[Obsolete]
		public const string PrepaymentAlreadyExists = "The prepayment already exists.";
		public const string Availability_POOrder = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}, Purchase Orders {4} {0}";

		#endregion

		#region Graph and Cache Names
		public const string POSetupMaint = "Purchasing Preferences";
		public const string POOrderEntry = "Purchase Order Entry";
		public const string POPrintOrder = "Print Purchase Order";
		public const string POReceiptEntry = "PO Receipt Entry";
		public const string POVendorCatalogueMaint = "PO Vendor Catalogue Maintenance";
		public const string LandedCostCodeMaint = "Landed Cost Code Maintenance";

		public const string POOrder = "Purchase Order";
		public const string POReceipt = "Purchase Receipt";
		public const string POReceiptLine = "Purchase Receipt Line";		
		public const string POLine = "Purchase Order Line";
		public const string PORemitAddress = "Remit Address";
		public const string PORemitContact = "Remit Contact";
		public const string POShipAddress = "Ship Address";
		public const string POShipContact = "Ship Contact";
        public const string VendorContact = "Vendor Contact";
        public const string Approval = "Approval";
		public const string LandedCostCode = "Landed Cost Code";
		public const string VendorLocation = "Vendor Location";
        public const string InventoryItemVendorDetails = "Inventory Item Vendor Details";
        public const string PORemitAddressFull = "PO Remittance Address";
        public const string POShipAddressFull = "PO Shipping Address";
        public const string POAddress = "PO Address";
        public const string PORemitContactFull = "PO Remittance Contact";
        public const string POShipContactFull = "PO Shipping Contact";
        public const string POContact = "PO Contact";
        public const string POLineS = "PO Line to Add";
        public const string POLineShort = "PO Line";
		public const string POAccrualStatus = "PO Accrual Status";
		public const string POAccrualSplit = "PO Accrual Allocation";
		public const string POOrderPrepayment = "PO Prepayment";
		public const string POOrderPOReceipt = "Purchase Order to Purchase Receipt Link";
		public const string POReceiptPOOrder = "Purchase Receipt to Purchase Order Link";
		public const string POReceiptPOReturn = "Purchase Receipt to Purchase Return Link";
		public const string POOrderAPDoc = "Purchase Order to Accounts Payable Document Link";

        public const string POOrderDiscountDetail = "Purchase Order Discount Detail";
        public const string POReceiptDiscountDetail = "Purchase Receipt Discount Detail";
		public const string POSetupApproval = "PO Approval";
		public const string POReceiptLineSplit = "Purchase Receipt Line Split";
		public const string POTaxTran = "PO Tax";
		public const string POTax = "PO Tax Detail";
		public const string POReceiptTaxTran = "Purchase Receipt Tax";
		public const string POReceiptTax = "Purchase Receipt Tax Detail";
		public const string POReceiptAPDoc = "Purchase Receipt Billing History";

		public const string POLandedCostDoc = "Landed Costs Document";
		public const string POLandedCostDetail = "Landed Costs Detail";
		public const string POLandedCostReceipt = "Landed Costs Receipt";
		public const string POLandedCostReceiptLine = "Landed Costs Receipt Line";
		public const string POLandedCostTaxTran = "Landed Costs Tax";
		public const string POLandedCostTax = "Landed Costs Tax Detail";

		public const string POReceiptSplitToTransferSplitLink = "Receipt Line Split To Transfer Line Split Link";
		public const string POReceiptSplitToCartSplitLink = "Receipt Line Split To Cart Split Link";
		public const string POCartReceipt = "Receipt Cart";
		public const string POReceivePutAwaySetup = "Receive Put Away Setup";
		public const string POReceivePutAwayUserSetup = "Receive Put Away User Setup";
		#endregion

		#region Order Type
		public const string Blanket = "Blanket";
		public const string StandardBlanket = "Standard";
		public const string RegularOrder = "Normal";
		public const string DropShip = "Drop Ship";

		#endregion

		public const string POBehavior_Standard = "Standard";
		public const string POBehavior_ChangeOrder = "Change Order";

		#region Shipping Destination
        public const string ShipDestCompanyLocation = "Branch";
        public const string ShipDestCustomer = "Customer";
		public const string ShipDestVendor = "Vendor";
		public const string ShipDestSite = "Warehouse";
		#endregion

        #region Default Receipt Quantity
        public const string OpenQuantity = "Open Quantity";
        public const string ZeroQuantity = "Zero";
        #endregion

		#region Order Line Type
		public const string GoodsForInventory = "Goods for IN";
		public const string GoodsForSalesOrder = "Goods for SO";
        public const string GoodsForServiceOrder = "Goods for FS";
        public const string GoodsForReplenishment = "Goods for RP";
		public const string GoodsForDropShip = "Goods for Drop-Ship";
		public const string NonStockForDropShip = "Non-Stock for Drop-Ship";
		public const string NonStockForSalesOrder = "Non-Stock for SO";
        public const string NonStockForServiceOrder = "Non-Stock for FS";
        public const string NonStockItem = "Non-Stock";
		public const string Service = "Service";
		public const string Freight = "Freight";
		public const string MiscCharges = "Misc. Charges";
		public const string Description = "Description";

		#endregion

		#region Report Order Type
		public const string PrintBlanket = "Blanket";
		public const string PrintStandardBlanket = "Stadard";
		public const string PrintRegularOrder = "Normal";
		public const string PrintDropShip = "Drop Ship";
		#endregion

		#region Report Order Type
		public const string PrintInvoice = "BILL";
		public const string PrintCreditAdj = "CRADJ";
		public const string PrintDebitAdj = "DRADJ";
		public const string PrintCheck = "CHECK";
		public const string PrintPrepayment = "PREPAY";
		public const string PrintRefund = "REF";
		public const string PrintVoidCheck = "VOIDCK";
		#endregion

		#region Order Status
		public const string Hold = "On Hold";
		public const string Open = "Open";
		public const string Completed = "Completed";
		public const string Closed = "Closed";
		public const string Cancelled = "Canceled";
		public const string PendingPrint = "Pending Printing";
		public const string PendingEmail = "Pending Email";
		public const string Printed = "Printed";
		public const string Approved = "Approved";
		public const string Rejected = "Rejected";
		public const string PrintOrder = "Print PO Order";
		#endregion

		#region Receipt Status
		public const string Balanced = "Balanced";
		public const string Released = "Released";
		#endregion

		#region PO Receipt Quantity Action
		public const string Accept = "Accept";
		public const string AcceptButWarn = "Accept but Warn";
		public const string Reject = "Reject";
		#endregion

		#region PO Vendor Catalogue Actions
        public const string ShowVendorPrices = "Vendor Prices";
		#endregion

		#region AP Mask Codes
		public const string MaskItem = "Non-Stock Item";
		public const string MaskLocation = AP.Messages.MaskLocation;
		public const string MaskEmployee = "Employee";
        public const string MaskCompany = "Branch";
        #endregion

		#region LandedCostAllocationMethods
		public const string ByQuantity = "By Quantity";
		public const string ByCost = "By Cost";
		public const string ByWeight = "By Weight";
		public const string ByVolume = "By Volume";
		public const string None = "None";

		#endregion

		#region LandedCostApplicationMethod
		public const string FromAP = "From AP";
		public const string FromPO = "From PO";
		public const string FromBoth = "From Both";
		#endregion

		#region LandedCostType
		public const string FreightOriginCharges = "Freight & Misc. Origin Charges";
		public const string CustomDuties = "Customs Duties";
		public const string VATTaxes = "VAT Taxes";
		public const string MiscDestinationCharges = "Misc. Destination Charges";
		public const string Other = "Other";
		#endregion

		#region Landed Costs Action
		public const string EnterAPBill = "Enter AP Bill";
		public const string AddLandedCosts = "Add Landed Costs";
		public const string Actions = "Actions";
		#endregion

		#region POAccrualType

		public const string Receipt = "Receipt";
		public const string Order = "Order";

		#endregion

		#region PPV Allocation Mode
		public const string PPVInventory = "Inventory Account";
		public const string PPVAccount = "Purchase Price Variance Account";
		#endregion

		#region Vendor changed - validation messages
		public const string VendorChangedOnOrderWithRestrictedCurrency = "The selected vendor does not work with the {0} currency specified in the purchase order. Select another vendor.";
		public const string VendorChangedOnOrderWithRestrictedRateType = "The selected vendor does not work with the {0} currency rate type specified in the purchase order. Select another vendor.";
		public const string VendorChangedOnOrderWithPurchaseReceipt = "You cannot change a vendor for this purchase order because it has purchase receipts linked.";
		public const string VendorChangedOnOrderWithAPDocument = "You cannot change a vendor for this purchase order because it has AP documents linked.";
		public const string VendorChangedOnOrderWithLinesFromBlanketOrder = "You cannot change a vendor for this purchase order because one or more lines were created from a blanked purchase order.";
		public const string VendorChangedOnOrderLinkedWithSO = "You cannot change a vendor for this purchase order because it is linked with a sales order line that is split between multiple purchase orders.";
		#endregion

		#region Headings
		public const string InventoryItemDescr = "Item Description";
		public const string SiteDescr = "Warehouse Description";
		public const string VendorAcctName = "Vendor Name";
		public const string CustomerAcctName = "Customer Name";
		public const string CustomerLocationID = "Customer Location";
		public const string PlanTypeDescr = "Plan Type";
		public const string VendorPrice = "Vendor Price";
		public const string CustomerPrice = "Customer Price";
		public const string CustomerPriceUOM = "Customer UOM";
		public const string POLineOrderNbr = "PO Nbr.";
		#endregion

		public const string PurchaseOrderCreated = "Purchase Order '{0}' created.";
		public const string MissingVendorOrLocation = "Vendor and vendor location should be defined.";
		public const string SalesOrderRelatedToDropShipReceiptIsNotApproved = "The {0} {1} sales order related to this drop-ship receipt is not approved. Contact {2} for details.";
		public const string SalesOrderLineHasAlreadyBeenCompleted = "In the {0} {1} sales order related to this purchase receipt, the line with the {2} item has already been completed.";
		public const string SalesOrderLineHasAlreadyBeenCompletedContactForDetails = "In the {0} {1} sales order related to this purchase receipt, the line with the {2} item has already been completed. Contact {3} for details.";
		public const string StatusTotalBilled = "Total Billed Qty. {0}, Total Billed Amt. {1}";
		public const string StatusTotalBilledTotalPPV = "Total Billed Qty. {0}, Total Billed Amt. {1}, Total PPV Amt. {2}";
		public const string StatusTotalBilledTotalAccrued = "Total Billed Qty. {0}, Total Billed Amt. {1}, Total Accrued Qty. {2}, Total Accrued Amt. {3}, Total PPV Amt. {4}";
		public const string StatusTotalReceived = "Total Received Qty. {0}";
		public const string StatusTotalPrepayments = "Total Applied to Order Amount {0}, Total Balance Amount {1}";
		public const string POAdjust = "Purchase Order Adjust";
		public const string LoadOrders = "Load Orders";
		public const string CanNotRemoveReferenceToReleasedPOOrder = "You cannot remove the reference to the {0} purchase order because it is released.";
		public const string CanNotRemoveReferenceToPOOrderRelatedPrepayment = "You cannot remove the reference to the {0} purchase order because it is related to the {1} prepayment request.";
		public const string PrepaidTotalGreaterUnbilledOrderTotal = "The value of the Unbilled Prepayment Total box of the {0} purchase order cannot exceed its unbilled amount.";
		public const string TotalPrepaymentAmountGreaterDocumentAmount = "The value of the PO Applied Amount box cannot be greater than the value of the Payment Amount box.";
		public const string PrepayPOLineMoreThanAllowed = "Cannot release the prepayment because the sum of amounts in the released prepayments and in the current prepayment for the purchase order line ({0}, {1}, {2}) exceeds the order amount {3}.";
		public const string POOrderDocTypeOnPrepaymentCheck = "PO";
	}
}
