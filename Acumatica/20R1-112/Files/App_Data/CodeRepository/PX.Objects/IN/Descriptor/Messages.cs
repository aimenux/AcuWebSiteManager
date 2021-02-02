using System;
using PX.Common;

namespace PX.Objects.IN
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages
		public const string Prefix = "IN Error";
		public const string InventoryItemIsInStatus = "The inventory item is {0}.";
		public const string InventoryItemIsNotAStock = "The inventory item is not a stock item.";
		public const string InventoryItemIsAStock = "The inventory item is a stock item.";
		public const string InventoryItemIsNotaKit = "The inventory item is not a kit.";
		public const string InventoryItemIsATemplate = "The inventory item is a template item.";
		public const string InventoryItemIsNotATemplate = "The inventory item is not a template item.";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		public const string DocumentOutOfBalance = "Document is out of balance.";
		public const string TransitSiteIsNotAvailable = "The warehouse cannot be selected; it is used for transit.";
		public const string Document_OnHold_CannotRelease = "Document is On Hold and cannot be released.";
		public const string Inventory_Negative = "Inventory quantity for {0} in warehouse '{1} {2}' will go negative.";
		public const string Inventory_Negative2 = "Inventory quantity will go negative.";
		public const string Transfered_Item_Receipted = "Inventory item '{0}' already receipted in Receipt '{1}'";
		public const string SubItemSeg_Missing_ConsolidatedVal = "Subitem Segmented Key missing one or more Consolidated values.";
		public const string SubItemIsDisabled = "The {0} value of the {1} segment of the {2} inventory item is inactive. Activate the value on the Stock Items (IN202500) form.";
		public const string TranType_Invalid = "Invalid Transaction Type.";
		public const string InternalError = "Internal Error: {0}.";
		public const string NotPrimaryLocation = "Selected item is not allowed in this location.";
		public const string LocationReceiptsInvalid = "Selected Location is not valid for receipts.";
		public const string LocationSalesInvalid = "Selected Location is not valid for sales.";
		public const string LocationTransfersInvalid = "Selected Location is not valid for transfers.";
		public const string Location = "Location";
		public const string StandardCostNoCostOnlyAdjust = "Cost only adjustments are not allowed for Standard Cost items.";
		public const string StatusCheck_QtyNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity available will go negative.";
		public const string StatusCheck_QtyNegativeSPC = "Adjustment cannot be released because the adjustment quantity exceeds the on-hand quantity of the '{0} {1}' item with the {2} lot or serial number.";
		public const string StatusCheck_QtyNegativeFifo = "Adjustment cannot be released because the adjustment quantity exceeds the on-hand quantity of the '{0} {1}' item received by the {2} receipt.";
		public const string StatusCheck_QtyNegativeFifoExactCost = "The document cannot be released because the return quantity exceeds the on-hand quantity of the {0} {1} item received by the {2} receipt.";
		public const string StatusCheck_QtyNegative1 = "Updating item '{0} {1}' in location '{2}' of warehouse '{3}' quantity available will go negative.";
		public const string StatusCheck_QtyNegative2 = "Updating item '{0} {1}' in warehouse '{2}' in cost layer '{3}' quantity  will go negative.";
		public const string StatusCheck_QtyAvailNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity available for shipment will go negative.";
		public const string StatusCheck_QtyLocationNegative = "Updating data for item '{0} {1}' on warehouse '{2} {3}' will result in negative available quantity.";
		public const string StatusCheck_QtyLotSerialNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' lot/serial number '{4}' quantity available will go negative.";
		public const string StatusCheck_QtyOnHandNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity on hand will go negative.";
		public const string StatusCheck_QtyActualNegative = "The document cannot be released, because the available for issue quantity will become negative for the '{0} {1}' inventory item. The item is located in the '{2}' warehouse.";
		public const string StatusCheck_QtyLotSerialActualNegative = "The document cannot be released, because the available for issue quantity will become negative for the '{0}' inventory item. The item has the '{2}' lot/serial number and is located in the '{1}' warehouse.";
		public const string StatusCheck_QtyTransitOnHandNegative = "The document cannot be released. The quantity in transit for the '{0} {1}' item will become negative. To proceed, adjust the quantity of the item in the document.";
		public const string StatusCheck_QtyLocationOnHandNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' quantity on hand will go negative.";
		public const string StatusCheck_QtyLotSerialOnHandNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' lot/serial number '{4}' quantity on hand will go negative.";
		public const string StatusCheck_QtyTransitLotSerialOnHandNegative = "The document cannot be released. The quantity in transit for the '{0} {1}' item with the '{2}' lot/serial number will become negative. To proceed, adjust the quantity of the item in the document.";
		public const string StatusCheck_QtySerialNotSingle = "Updating the quantity of the item '{0}' with serial number '{1}' will result in a quantity on hand for this serial number greater than 1.";
		public const string StatusCheck_QtyCostImblance = "Updating item '{0} {1}' in warehouse '{2}' caused cost to quantity imbalance.";
		public const string EmptyAutoIncValue = "Auto-Incremental value is not set in {0}.";
		public const string LotSerTrackExpirationInvalid = "Only classes with enabled Track Expiration can use Expiration Issue Method.";
		public const string LotSerAssignCannotBeChanged = "The assignment method for the posting class cannot be changed because this method is assigned to the '{0}' item.";
		public const string LotSerIssueMethodCannotBeChangedShipment = "The issue method cannot be changed because the {0} shipment contains the {1} item with the serial number of this class. Process the shipment with the current settings or delete the shipment, and then change the settings";
		public const string LotSerAutoNextNbrCannotBeChangedShipment = "The value of the 'Auto-Generate Next Number' checkbox cannot be changed because the {0} shipment contains the {1} item with the serial number of this class. Process the shipment with the current settings or delete the shipment, and then change the settings";
		public const string LotSerClass = "Lot/Serial Class";
		public const string InventoryItem = "Inventory Item";
		public const string EmptyKitNotAllowed = "Selected kit cannot be added. The kit has no components specified.";
		public const string StartDateMustBeLessOrEqualToTheEndDate = "Start date must be less or equal to the end date.";
		public const string ValMethodCannotBeChanged = "Valuation method cannot be changed from '{0}' to '{1}' while stock is not zero.";
		public const string ValMethodCannotBeChangedTransit = "The valuation method cannot be changed from {0} to {1} because the item is in transit.";
		public const string ValMethodChanged = "Valuation method can be changed but the history will not be updated.";
		public const string ThisItemClassCanNotBeDeletedBecauseItIsUsedInInventorySetup = "This Item Class can not be deleted because it is used in Inventory Setup.";
		public const string ThisItemClassCanNotBeDeletedBecauseItIsUsedInInventoryItem = "This Item Class cannot be deleted because it is used for inventory item: {0}.";
		public const string StkItemValueCanNotBeChangedBecauseItIsUsedInInventoryItem = "The value of the Stock Item check box cannot be changed because the item class is assigned to the {0} item. Select another item class for this item first.";
		public const string ChildStkItemValueCanNotBeChangedBecauseItIsUsedInInventoryItem = "The value of the Stock Item check box cannot be changed for the {0} child item class because the item class is assigned to the {1} item. Select another item class for this item first.";
		public const string ThisItemClassCanNotBeDeletedBecauseItIsUsedInWarehouseLocation = "This Item Class can not be deleted because it is used in Warehouse/Location: {0}/{1}.";
		public const string TotalPctShouldBe100 = "Total % should be 100%";
		public const string ThisValueShouldBeBetweenP0AndP1 = "This value should be between {0} and {1}";
		public const string PercentageValueShouldBeBetween0And100 = "Percentage value should be between 0 and 100";
		public const string SpecificOnlyNumbered = "Specific valuated items should be lot or serial numbered during receipt.";
		public const string InsuffQty_LineQtyUpdated = "Insufficient quantity available. Line quantity was changed to match.";
		public const string SerialItem_LineQtyUpdated = "Invalid quantity specified for serial item. Line quantity was changed to match.";
		public const string SerialItemAdjustment_LineQtyUpdated = "Serialized item adjustment can be made for zero or one '{0}' items. Line quantity was changed to match.";
		public const string SerialItemAdjustment_UOMUpdated = "Serialized item adjustment can be made for zero or one '{0}' items. UOM was changed to match.";
		public const string SiteLocationOverride = "Update default location for all items on this site by selected location?";

		public const string Availability_Field = "Availability";
		public const string Availability_Info = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}";
		public const string Availability_ActualInfo = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}, Available for Issue {4} {0}";
		public const string PIDBInconsistency = "Inconsistent DB data: The system cannot find the pair record (lock) in one of the following tables: INPIStatusItem, INPIStatusLoc.";
		public const string PICollision = "The system cannot run the PI because it has intersecting entities with PI {0}, which is in progress in warehouse {1}. See Trace for details.";
		public const string PICollisionDetails = "The system cannot run the current PI in warehouse {1} because the PI has intersecting entities with PI {0}, which is in progress. Intersecting inventory items: {2}; intersecting locations: {3}.";
		public const string PIAllInventoryCollisionDetails = "The system cannot run the current PI in warehouse {1} because the PI has intersecting entities with PI {0}, which is in progress. Intersecting locations: {2}.";
		public const string PIAllLocationsCollisionDetails = "The system cannot run the current PI in warehouse {1} because the PI has intersecting entities with PI {0}, which is in progress. Intersecting inventory items: {2}.";
		public const string PIFullCollisionDetails = "The system cannot run the current PI in the {1} warehouse because the full PI {0} is in progress.";
		public const string PICountInProgressDuringRelease = "Physical count in progress for {0} in warehouse '{1} {2}'";
		public const string InventoryShouldBeUsedInCurrentPI = "Combination of selected Inventory Item and Warehouse Location is not allowed for this Physical Count.";
		public const string ThisCombinationIsUsedAlready = "This Combination Is Used Already in Line Nbr. {0}";
		public const string ThisSerialNumberIsUsedAlready = "This  Serial Number Is Used Already in Line Nbr. {0}";
		public const string ThisSerialNumberIsUsedInItem = "This Serial Number Is Used Already for the item";
		public const string PINotEnoughQtyInWarehouse = "Unable to create adjustment for line '{0}'. Insufficient Qty. On Hand for item '{1} {2}' in warehouse '{3}'.";
		public const string PINotEnoughQtyOnLocation = "Unable to create adjustment for line '{0}'. Insufficient Qty. On Hand for item '{1} {2}' in warehouse '{3} {4}'.";
		public const string ConfirmationXRefUpdate = "Substitute previous cross references information?";
		public const string AlternatieIDNotUnique = "Value '{0}' for Alternate ID is already used for another inventory item.";
		public const string FractionalUnitConversion = "Fractional unit conversions not supported for serial numbered items";
		public const string SiteUsageDeleted = "Unable to delete warehouse, item '{0}' has non-zero Quantity On Hand.";
		public const string ItemLotSerClassVerifying = "Lot/serial class cannot be changed when its tracking method is not compatible with the previous class and the item is in use.";
		public const string SerialNumberAlreadyIssued = "Serial Number '{1}' for item '{0}' already issued.";
		public const string SerialNumberAlreadyIssuedIn = "Serial Number '{1}' for item '{0}' already issued in '{2}'.";
		public const string SerialNumberAlreadyReceived = "Serial Number '{1}' for item '{0}' is already received.";
		public const string SerialNumberAlreadyReceivedIn = "Serial Number '{1}' for item '{0}' is already received in '{2}'.";
		public const string SerialNumberDuplicated = "Duplicate serial number '{1}' for item '{0}' is found in document.";
		public const string NumericLotSerSegmentNotExists = "'{0}' segment must be defined for lot/serial class.";
		public const string NumericLotSerSegmentMultiple = "Multiple '{0}' segments defined for lot/serial class.";
		public const string SumOfAllComponentsMustBeHundred = "Total Percentage for Components must be 100. Please correct the percentage split for the components.";
		public const string SumOfAllComponentsMustBeLessHundredWithResiduals = "Total Percentage for Components must be less than 100 when there is a component with 'Residual' allocation method. Please correct the percentage split for the components.";
		public const string OnlyOneResidualComponentAllowed = "There must be only one component with 'Residual' allocation method for an item.";
		public const string ItemClassChangeWarning = "Please confirm if you want to update current Item settings with the Inventory Class defaults. Original settings will be preserved otherwise.";
		public const string ItemClassAndInventoryItemStkItemShouldBeSameSingleItem = "Inventory item {0} has not been moved to the {1} item class because moved item and the target item class should both be configured either as stock or as non-stock entities.";
		public const string ItemClassAndInventoryItemStkItemShouldBeSameManyItems = "Inventory items have not been moved to the {0} item class because all moved items and the target item class should both be configured either as stock or as non-stock entities. See trace for details.";
		public const string CouldNotBeMovedToItemClassItemsList = "Inventory items that cannot be moved to the {0} item class:";
		public const string DifferentItemsCouldNotBeMovedToItemClass = "You have selected both stock and non-stock items. They could not be moved to one item class.";
		public const string MissingUnitConversion = "Unit conversion is missing.";
		public const string MissingUnitConversionVerbose = "Unit conversion {0} is missing.";
		public const string DfltQtyShouldBeBetweenMinAndMaxQty = "Component Qty should be between Min. and Max. Qty.";
		public const string KitMayNotIncludeItselfAsComponentPart = "Kit May Not Include Itself As Component Part";
		public const string IssuesAreNotAllowedFromThisLocationContinue = "Issues are not allowed from this Location. Continue ?";
		public const string NonStockKitAssemblyNotAllowed = "Non-Stock Kit Assembly is not allowed.";
		public const string LSCannotAutoNumberItem = "Cannot generate the next lot/serial number for item {0}.";
		public const string LocationCostedWarning = "There is non zero Quantity on Hand for this item on selected Warehouse Location. You can only change Cost Separately option when the Qty on Hand is equal to zero";
		public const string LocationCostedSetWarning = "Last Inventory cost on warehouse will not be updated if the item has been received on this Warehouse Location.";
		public const string PeriofNbrCanNotBeGreaterThenInSetup = "Period Number can not be greater then Turnover Periods per Year on the InSetup.";
		public const string PossibleValuesAre = "Possible Values are: 1,2,3,4,6,12.";
		public const string TemplateItemExists = "This ID is already used for another template item. Specify another ID.";
		public const string NonStockItemExists = "This ID is already used for another Non-Stock Item.";
		public const string StockItemExists = "This ID is already used for another Stock Item.";
		public const string QtyOnHandExists = "There is non zero Quantity on Hand for this item. You can only change Cost when the Qty on Hand is equal to zero";
		public const string PILineDeleted = "Unable to delete line, just manually added line can be deleted.";
		public const string PIEmpty = "Cannot generate the physical inventory count. List of details is empty.";
		public const string PIPhysicalQty = "Serial-numbered items should have physical quantity only 1 or 0.";
		public const string BinLotSerialNotAssigned = "One or more lines have unassigned Location and/or Lot/Serial Number";
		public const string BinLotSerialNotAssignedWithItemCode = "One or more lines for item '{0}' have unassigned Location and/or Lot/Serial Number";
		public const string AdjstmentCreated = "Adjustment '{0}' created.";
		public const string AdjustmentsCreated = "The following adjustments have been created: {0}.";
		public const string SingleRevisionForNS = "Non-Stock kit can contain only one revision.";
		public const string RestictedSubItem = "Subitem status restricts using it for selected site.";
		public const string CantGetPrimaryView = "Can't get the primary view type for the graph {0}";
		public const string UnknownSegmentType = "Unknown segment type";
		public const string TooShortNum = "Lot/Serial Number must be {0} characters long";
		public const string UnableNavigateDocument = "Unable to navigate on document.";
		public const string ReplenihmentPlanDeleted = "Processing of replenishment with 0 quantity will delete previous plan.";
        public const string ReplenihmentSourceIsNotSelected = "No replenishment source has been specified for the item. Specify a replenishment source.";
		public const string ReceiptAddedForPO = "Item {0}{1} receipted {2} {3} for Purchase Order {4}";
		public const string PILineUpdated = "Item {0}{1} updated physical quantity {2} {3} line {4}.";
		public const string ConversionNotFound = "Unit Conversion is not setup on 'Units Of Measure' screen. Please setup Unit Conversion FROM {0} TO {1}.";
		public const string BoxesRequired = "At least one box must be specified in the Boxes grid for the given packaging option.";
		public const string PeriodHasINDocsFromPO_LCToBeCreated = "There one or more pending IN Adjustment originating from the existing Landed Cost transaction in PO module which belong to this period. They have to be created and released before the period may be closed in IN. Please, check the screen 'Process Landed Cost'(PO.50.60.00)";
		public const string PeriodHasINDocsFromAP_LCToBeCreated = "There one or more pending IN Adjustment originating from the existing Landed Cost transaction in AP module which belong to this period. They have to be created and released before the period may be closed in IN. Please, check the screen 'Process Landed Cost'(AP.50.65.00)";
		public const string ReplenishmentSourceSiteMustBeDifferentFromCurrenSite = "Replenishment Source Warehouse must be different from current Warehouse";
		public const string InactiveWarehouse = "Warehouse '{0}' is inactive";
		public const string InactiveLocation = "Location '{0}' is inactive";
		public const string SubitemDeleteError = "You cannot delete Subitem because it is already in use.";
		public const string CantDeactivateSite = "Can't deactivate warehouse. It has unreleased transactions.";
		public const string PeriodsOverlap = "Periods overlap.";
		public const string ItemCannotPurchase = "Item cannot be purchased";
		public const string ItemCannotSale = "Item cannot be sold";
		public const string ValueIsRequiredForAutoPackage = "Value is required for Auto packaging to work correctly.";
		public const string MaxWeightIsNotDefined = "Box Max. Weight must be defined for Auto Packaging to work correctly.";
		public const string MaxVolumeIsNotDefined = "Box Max. Volume must be defined for Auto Packaging to work correctly.";
		public const string ItemDontFitInTheBox = "The item can't fit the given Box.";
		public const string NonStockKitInKit = "It is not allowed to add non-stock kits as components to a stock kit or to a  non-stock kit.";
		public const string UOMRequiredForAccount = "{0} may not be empty for Account '{1}'";
		public const string CollumnIsMandatory = "Incorrect head in the file. Column \"{0}\" is mandatory";
		public const string ImportHasError = "Import has some error. The list of incorrect records is recorded in the Trace.";
		public const string RowError = "Row number {0}. Error message \"{1}\"";
		public const string ItemIsUsed = "'{0}' item is used in '{1}' and cannot be deleted.";
		public const string ItemHasPurchaseOrders = "The '{0}' stock item is used in the Purchase Orders (PO301000) screen, Order Type: '{1}' Order Number: '{2}'";
		public const string ItemClassIsStock = "The class you have selected can not be assigned to a non-stock item, because the Stock Item check box is selected for this class on the Item Classes (IN201000) form. Select another item class which is designated to group non-stock items.";
		public const string ItemClassIsNonStock = "The class you have selected can not be assigned to a stock item, because the Stock Item check box is cleared for this class on the Item Classes(IN201000) form.Select another item class which is designated to group stock items.";
		public const string ItemHasSalesOrders = "The '{0}' stock item is used in the Sales Orders (SO301000) screen, Order Type: '{1}' Order Number: '{2}'";
		public const string ItemHasKitSpecifications = "The '{0}' stock item is used in the Kit Specifications (IN209500) screen";
		public const string ItemHasProjectTransactions = "The '{0}' stock item is used in the Project Transactions(PM304000) screen, Reference Nbr. '{1}'";
		public const string ItemHasPhysicalInventoryReview = "The '{0}' stock item is used in the Physical Inventory Review (IN305000) screen, Reference Nbr. '{1}'";
		public const string ItemHasAPBill = "The '{0}' stock item is used in the Bills And Adjustments (AP301000) screen, Reference Nbr. '{1}'";
		public const string ItemHasRequisition = "The '{0}' stock item is used in the Requisitions(RQ302000) screen, Document Nbr. '{1}'";
		public const string ItemHasRequest = "The '{0}' stock item is used in the Requests (RQ301000) screen, Document Nbr. '{1}'";
		public const string ItemWasDeleted = "The item was deleted";
		public const string ItemHasStockRemainder = "There is a non-zero quantity of the '{0}' item at the '{1}' warehouse.";
		public const string DiscountAccountIsNotSetupLocation = "Discount Account is not set up. See Location \"{0}\" for Customer \"{1}\" ";
		public const string DiscountAccountIsNotSetupCustomer = "Discount Account is not set up. See Customer \"{0}\" ";
		public const string WarehouseNotAllowed = "Selected Warehouse is not allowed in {0} transfer";
		public const string ProjectUsedInPO = "Project cannot be changed. Atleast one Unrelased PO Receipt exists for the given Project.";
		public const string TaskUsedInPO = "Project Task cannot be changed. Atleast one Unrelased PO Receipt exists for the given Project Task.";
		public const string ProjectUsedInSO = "Project cannot be changed. Atleast one Unrelased SO Shipment exists for the given Project.";
		public const string TaskUsedInSO = "Project Task cannot be changed. Atleast one Unrelased SO Shipment exists for the given Project Task.";
		public const string ProjectUsedInIN = "Project cannot be changed. Available Quantity on this location is not zero.";
		public const string TaskUsedInIN = "Project Task cannot be changed. Available Quantity on this location is not zero.";
		public const string LocationIsMappedToAnotherTask = "The Project Task specified for the given Location do not match the selected Project Task.";
		public const string MixedProjectsInSplits = "Splits cannot mix locations with different Project/Tasks. Please enter them as seperate lines.";
		public const string RequireSingleLocation = "When posting to Project Location must be the same for all splits.";
		public const string StandardCostItemOnProjectLocation = "Location '{0}' is associated with Project and contains non-zero 'Quantity On-Hand' and cannot be correctly revaluated. To revaluate 'Standard-Cost' item first move all remaining items to a Non-Project location.";
		public const string AlternateIDDoesNotCorrelateWithCurrentSegmentRules = "The specified alternate ID does not comply with the INVENTORY segmented key settings. It might be not possible to use this alternate ID directly in entry forms.";
		public const string TransferLineIsCorrupted = "The warehouse in the document differs from the warehouse in the line. Remove the line and add it again to update the warehouse.";
		public const string TransferDocumentIsCorrupted = "The document is corrupted because the warehouse in the document differs from the warehouse in the {0} line. Remove the line and add it again to update the warehouse.";

		public const string BaseUnitNotSmallest = "The base unit is not the smallest unit of measure available for this item. Ensure that the quantity precision configured in the system is large enough. See the Quantity Decimal Places setting on the Branches form.";
		public const string BaseUnitCouldNotBeChanged = "Base UOM cannot be changed for the item in use.";
		public const string FromUnitCouldNotBeEqualBaseUnit = "The entered unit is the base unit and cannot be used to convert from. Enter a different unit.";
		public const string NotDecimalBaseUnit = "The {0} base UOM is not divisible for the {1} item. Check conversion rules.";
		public const string NotDecimalSalesUnit = "The {0} sales UOM is not divisible for the {1} item.";
		public const string NotDecimalPurchaseUnit = "The {0} purchase UOM is not divisible for the {1} item.";
		public const string DecimalBaseUnitCouldNotUnchecked = "The {0} UOM cannot be changed to not divisible because the quantity of the item allocated for {1} is fractional.";
		public const string LocationIsNotActive = "Location is not Active.";
		public const string ZeroQtyWhenNonZeroCost = "Quantity cannot be zero when Ext. Cost is nonzero.";
		public const string ProjectWildcardLocationIsUsedIn = "Project wildcard (without Task) is already setup for the Warehouse '{0}' Location '{1}'.";
		public const string DoesNotMatchWithAlternateType = "The alternate type for '{0}' is '{1}', which does not match the selected alternate type.";
		public const string FailedToProcessComponent = "Failed to process Component '{0}' when processing kit '{1}'. {2}";
		public const string MultipleAggregateChecksEncountred = "The '{0}' segment of the '{1}' segmented key has more than one value with the Aggregation check box selected  on the Segment Values (CS203000) form.";
		public const string LocationInUseInItemWarehouseDetails = "Location '{0}' is selected as default location in Item Warehouse Details for Item '{1}' and cannot be deleted.";
		public const string LocationInUseInPIType = "Location '{0}' is added to Physical Inventory Type '{1}' and cannot be deleted.";
		public const string InvalidPlan = "A transaction is missing allocation details. Please, delete current document and create a new one.";
		public const string BinLotSerialEntryDisabled = "The Allocations dialog box cannot be opened, because managing allocations is not allowed for the selected item.";
		public const string CannotAddNonStockKit = "A non-stock kit cannot be added to a cash transaction.";
		public const string NotPossibleDeleteINAvailScheme = "This availability calculation rule cannot be deleted because it is assigned to at least one item class.";
		public const string EnteredItemClassIsNotStock = "The entered item class is not a stock item class.";
		public const string EnteredItemClassIsNotNonStock = "The entered item class is not a non-stock item class.";
		public const string ManyAltIDsForSingleInventoryID = "The specified alternate ID is assigned to multiple inventory items. Please select the appropriate inventory ID in the row.";
		public const string AltIDIsNotDefinedAndWillBeAddedOnRelease = "The specified alternate ID has not been defined for the selected inventory item on the Cross-Reference tab of the Stock Items (IN202500) or Non-Stock Items (IN202000) form. Upon release of the worksheet, the system assigns this alternate ID to the inventory item.";
		public const string AltIDIsNotDefinedAndWillNotBeAddedOnRelease = "The specified alternate ID is already defined for another inventory item and thus cannot be assigned to the inventory ID selected in this row.";
		public const string NoSpecifiedAltID = "The specified alternate ID cannot be found in the system.";
		public const string UOMAssignedToAltIDIsNotDefined = "The specified unit of measure is not defined for this inventory item.";
		public const string UnpostedDocsExist = "There are documents pending posting of inventory transactions to the closed period. Review the Unposted IN report (IN656500) for details.";
		public const string WrongUnitConversion = "The changes cannot be saved because the conversion factor for converting unit '{0}' to unit '{0}' differs from 1.";
        public const string INTranCostOverReceipted = "The document has not been released because the cost layer of the '{0} {1}' item was not updated. Try to release the document again.";
		public const string InactiveKitRevision = "Revision '{0}' is inactive";
		public const string LocationWithProjectLowestPickPriority = "There is a location without a project association with the same or lower pick priority. Consider specifying lower pick priority for the current location to ensure correct selection of a location for sales orders unrelated to projects.";
	    public const string ReplenishmentSourceSiteRequiredInTransfer = "Replenishment Warehouse cannot be empty.";
		public const string InactiveSegmentValues = "At least one value in each segment that requires validation should be selected on the SUBITEMS tab.";
		public const string ReasonCodeDoesNotMatch = "The usage type of the reason code does not match the document type.";
		public const string CannotReleaseAllocationsMissing = "The system cannot release the document because allocation of the {0} item in the {1} warehouse is not found. Reallocate the item in the document line #{2}.";
		public const string PITypeEarlyInventoryUnfreezeWarning = "Unfreezing stock when a PI process is not completed may cause discrepancy in cost or quantity of stock items and inability to release PI adjustments.";
		public const string PIGenerationEarlyInventoryUnfreezeWarning = "The Unfreeze Stock When Counting Is Finished check box is selected on the Physical Inventory Types (IN208900) form. This may cause discrepancy in cost or quantity of stock items and inability to release PI adjustments.";
		public const string BaseCompanyUomIsNotDefined = "Default values for weight UOM and volume UOM are not specified on the Companies (CS101500) form.";
		public const string TransferIsCorrupted = "The database record that corresponds to the {0} transfer is corrupted. Please contact your Acumatica support provider.";
		public const string WrongInventoryItemToUnitValue = "The {0} value specified in the To Unit box differs from the {2} base unit specified for the {1} item. To resolve the issue, please contact your Acumatica support provider.";
		public const string BaseConversionNotFound = "The conversion rule of the {0} unit of measure to the {1} unit of measure is not found for the {2} item. To resolve the issue, please contact your Acumatica support provider.";
		public const string WrongItemClassToUnitValue = "The {0} value specified in the To Unit box differs from the {2} base unit specified for the {1} item class. To resolve the issue, please contact your Acumatica support provider.";
		public const string TransferShouldBeProcessedThroughPO = "The {0} transfer receipt must be processed by using the Purchase Receipts (PO302000) form.";
		public const string KitSpecificationExists = "The check box cannot be cleared because a kit specification exists for this item.";
		#endregion

		#region Translatable Strings used in the code
		public const string LS = "Lot/Serial";
		public const string Multiple = "<MULTIPLE>";
		public const string Unassigned = "<UNASSIGNED>";
		public const string ExceptLocationNotAvailable = "[*]  Except Location Not Available";
		public const string ExceptExpiredNotAvailable = "[**] Except Expired and  Loc. Not Available";
		public const string EstimatedCosts = "[*]  Estimated Costs";
		public const string CustomerID = "Customer ID";
		public const string Customer = "Customer";
		public const string CustomerName = "Customer Name";
		public const string Contact = "Contact";
		public const string ReceiptType = "Receipt Type";
		public const string ReceiptNbr = "Receipt Nbr.";
		public const string ExpireDateLessThanStartDate = "Expire Date must be greater than Start Date";
		public const string ProductionVarianceTranDesc = "Production Variance";
		public const string SeasonalSettingsAreOverlaped = "Seasonal settings are not defined correctly (overlap detected)";
		public const string AttemptToComparePeriodsOfDifferentType = "Period of different types can not be compared";
		public const string ThisTypeOfForecastModelIsNotImplemetedYet = "The model type {0} is not implemented yet";
		public const string InternalErrorSequenceIsNotSortedCorrectly = "InternalError: Sequence's  sorting order is wrong or it's not sorted";

		public const string OverrideInventoryAcctSub = "Override Inventory Account/Sub.";
		public const string OverrideInventoryAcct = "Override Inventory Account.";
		public const string SearchableTitleKit = "Kit: {0} {1}";
		public const string NoDefaultTermSpecified = "For items with no Default Term, the system cannot calculate Term End Date.";

		public const string UnknownDocumentType = "The Document Type is unknown.";
		public const string NotEnteredLineDataError = "Line data should be entered.";
		public const string UnknownPiTagSortOrder = "Unknown PI Tag # sort order";

		public const string Confirmation = "Confirmation";
		public const string ConfirmItemClassApplyToChildren = "The settings of this item class will be assigned to its child item classes, which might override the custom settings. Please confirm your action.";
		public const string ConfirmItemClassDeleteKeepChildren = "The item class that you want to delete has child item classes. Would you like to keep the child item classes? (If you keep the child item classes, they will become children of the item class at the level immediately above the deleted class.)";
		public const string DuplicateItemClassID = "The {0} item class ID already exists. Specify another item class ID.";
		public const string CopyingSettingsFailed = "Copying settings from the selected item class has completed with errors; some settings have not been copied. Try to select the item class again and save the changes.";
		public const string FinancialPeriodClosedInIN = "The {0} financial period of the {1} company is closed in Inventory.";
		public const string TypeMustImplementInterface = "The specified type {0} must implement the {1} interface.";

		public const string NewKey = "<NEW>";

		#endregion

		#region Graph Names
		public const string INUnitMaint = "Inventory Unit Maintenance";
		public const string INItemClassMaint = "Item Class Maintenance";
		public const string NonStockItemMaint = "Non-Stock Items Maintenance";
		public const string InventoryItemMaint = "Inventory Items Maintenance";
		public const string INItemSiteMaint = "Inventory Item Warehouse Detail";
		public const string INSiteMaint = "Warehouse Maintenance";
		public const string INReceiptEntry = "Receipt Entry";
		public const string INIssueEntry = "Issue Entry";
		public const string INAdjustmentEntry = "Adjustment Entry";
		public const string INTransferEntry = "Transfer Entry";
		public const string INDocumentRelease = "Release IN Documents";
		public const string InventorySummaryEnq = "Inventory Summary Enquiry";
		public const string INPostClassMaint = "Posting Class Maintenance";
		public const string INLotSerClassMaint = "Lot/Serial Class Maintenance";
		public const string INSetup = "Inventory Preferences";
		public const string INSetupMaint = "IN Setup";
		public const string InventoryAllocDetEnq = "Inventory Allocation Detail Inquiry";
		public const string InventoryTranDetEnq = "Inventory Transaction Detail Inquiry";
		public const string InventoryTranSumEnq = "Inventory Transaction Summary Inquiry";
		public const string InventoryTranHistEnq = "Inventory Transaction History Inquiry";
		public const string InventoryTranByAcctEnq = "Inventory Transaction By Account Inquiry";
		public const string InventoryLotSerInq = "Inventory Lot/Serial Inquiry";
		public const string INABCCodeMaint = "ABC Code Maintenance";
		public const string INMovementClassMaint = "Movement Class Maintenance";
		public const string INPICycleMaint = "PI Cycle Maintenance";
		public const string INPriceClassMaint = "Inventory Price Class Maintenance";
		public const string PIGenerator = "PI Tags Generator";
		public const string INPIEntry = "Physical Inventory Entry";
		public const string INPIReview = "Physical Inventory Review";
		public const string INUpdateABCAssignment = "Update ABC Assignments";
		public const string INUpdateMCAssignment = "Update Movement Class Assignments";
		public const string INReplenishmentClassMaint = "Replenishment Class";
		public const string INAccess = "Warehouse Access";
		public const string INAccessItem = "Inventory Item Access";
		public const string INAccessDetail = "Warehouse Access Detail";
		public const string INAccessDetailItem = "Inventory Item Access Detail";
		public const string INKitSpecMaint = "Kit Specification Maintenance";
		public const string KitAssemblyEntry = "Kit Assembly Entry";
		public const string KitSubstitutionIsRestricted = "Manual Component substitution is not allowed by the Kit specification.";
		public const string KitQtyVarianceIsRestricted = "Quantity is dictated by the Kit specification and cannot be changed manualy for the given component.";
		public const string KitQtyOutOfBounds = "Quantity is out of bounds. Specification dictates that it should be within [{0}-{1}] {2}.";
		public const string KitQtyNotEvenDistributed = "Quantity of Components is not valid. Quantity must be such that it can be uniformly distributed among the kits produced.";
		public const string KitItemMustBeUniqueAccrosSubItems = "Component Item must be unique for the given Kit accross Component ID and Subitem combinations.";
		public const string KitItemMustBeUnique = "Component Item must be unique for the given Kit.";
		public const string INReplenishmentCreate = "Inventory Replenishment Create";
		public const string EquipmentMaint = "Equipment Maintenance";
		public const string CategoryMaint = "Category Maintenance";
		public const string UsingKitAsItsComponent = "Non-stock kit can't using as its own component";
		public const string SNComponentInSNKit = "Serial-numbered components are allowed only in serial-numbered kits";
		public const string WhenUsedComponentInKit = "Components with the 'When-Used' assignment method and 'User-Enterable' issue method are not allowed in non-stock kits";
		public const string SerialNumberedComponentMustBeInBaseUnitOnly = "You can add serial tracked components with only a base UOM ('{0}') to the kit specification.";
		#endregion

		#region Cache Names
		public const string Warehouse = "Warehouse";
		public const string WarehouseBuilding = "Warehouse Building";
		public const string ItemClass = "Item Class";
		public const string INItemClassRep = "Item Class Replenishment";
		public const string Warranty = "Warranty";
		public const string Equipment = "Equipment";
		public const string Register = "Receipt";
		public const string INSite = "Warehouse";
		public const string ItemWarehouseSettings = "Item/Warehouse Settings";
		public const string PostingClass = "Posting Class";
		public const string KitSpecification = "Kit Specification";
		public const string ReplenishmentPolicy = "Replenishment Policy";
		public const string INSubItem = "IN Sub Item";
		public const string INItemClassSubItemSegment = "SubItem Segment of Item Class";
		public const string INItemSiteReplenishment = "SubItem Replenishment Info";
		public const string InventoryUnitConversions = "Inventory Unit Conversions";
		public const string DeferredRevenueComponents = "Deferred Revenue Components";
		public const string ItemCostStatistics = "Item Cost Statistics";
		public const string ItemReplenishmentSettings = "Item Replenishment Settings";
		public const string SubitemReplenishmentSettings = "Subitem Replenishment Settings";
		public const string INReplenishmentClass = "Replenishment Class";
		public const string INReplenishmentOrder = "Replenishment Order";
		public const string INReplenishmentLine = "Replenishment Line";
		public const string INReplenishmentSeason = "Replenishment Seasonality";
		public const string XReferences = "Cross-Reference";
		public const string INComponentTran = "IN Component";
		public const string INOverheadTran = "IN Overhead";
		public const string INTran = "IN Transaction";
		public const string INComponentTranSplit = "IN Component Split";
		public const string INKitTranSplit = "IN Kit Split";
		public const string INTranSplit = "IN Transaction Split";
		public const string INKit = "IN Kit";
		public const string INKitSpecNonStkDet = "Non-Stock Component of Kit Specification";
		public const string INKitSpecStkDet = "Stock Component of Kit Specification";
		public const string INKitSerialPart = "Kit Serial";
		public const string INLocationStatus = "IN Location Status";
		public const string INLotSerialStatus = "IN Lot/Serial Status";
		public const string INItemLotSerial = "Lot/Serial by Item";
		public const string INSiteLotSerial = "Lot/Serial by Warehouse";
		public const string INLotSerSegment = "Lot/Serial Segment";
		public const string INSiteStatus = "IN Site Status";
		public const string INItemSiteHistByPeriod = "IN Item Site History By Period";
		public const string INItemSiteHistDay = "IN Item Site History Day";
		public const string INItemSiteHistByDay = "IN Item Site History By Day";
		public const string INItemSiteHistByLastDayInPeriod = "IN Item Site History By Last Day In Period";
		public const string INTranDetail = "IN Transaction Detail";
		public const string INCostStatusSummary = "IN Cost Status Summary";
		public const string INLocation = "IN Location";
		public const string INTranCost = "IN Transaction Cost";
		public const string INCostStatus = "IN Cost Status";
		public const string INItemCostHistByPeriod = "IN Item Cost History By Period";
		public const string INPIDetail = "IN Physical count Detail";
		public const string INPIClass = "Physical Inventory Type";
		public const string INPIClassItem = "Physical Inventory Type by Item";
		public const string INPIClassLocation = "Physical Inventory Type by Location";
		public const string INPICycle = "Physical Inventory Cycle";
		public const string INPIStatus = "Physical Inventory Status";
		public const string INItemSiteHist = "IN Item Site History";
		public const string INItemCostHist = "IN Item Cost History";
		public const string INItemSalesHist = "Item Sales History";
		public const string INCategory = "Item Sales Category";
		public const string INItemCategory = "Item Sales Category by Item";
		public const string INSiteStatusSummary = "IN Warehouse Status";
		public const string INAvailabilityScheme = "Availability Calculation Rule";
		public const string INSubItemSegmentValue = "IN Subitem Segment Value";
		public const string INPriceClass = "IN Item Price Class";
		public const string INItemPlan = "IN Item Plan";
		public const string INItemPlanType = "IN Item Plan Type";
		public const string INItemStats = "IN Item Statistics";
		public const string INMovementClass = "IN Movement Class";
		public const string INABCCode = "IN ABC Code";
		public const string INTote = "IN Tote";
		public const string INCart = "IN Cart";
		public const string INCartSplit = "IN Cart Split";
		public const string INStoragePlace = "IN Storage Place";
		public const string INStoragePlaceSplit = "IN Storage Place Split";
		public const string INStoragePlaceStatus = "IN Storage Place Status";
		public const string INStoragePlaceStatusExpanded = "IN Storage Place Detailed Status";
		public const string INScanSetup = "IN Scan Setup";
		public const string INScanUserSetup = "IN Scan User Setup";
		public const string StockItemAutoIncrementalValue = "Auto-Incremental Value of a Stock Item";
		public const string LotSerClassAutoIncrementalValue = "Auto-Incremental Value of a Lot/Serial Class";
		public const string GS1UOMSetup = "GS1 Unit Setup";
		#endregion

		#region Combo Values

		public const string ModulePI = "PI";

		#region Inventory Mask Codes
		public const string MaskItem = "Inventory Item";
		public const string MaskSite = "Warehouse";
		public const string MaskClass = "Posting Class";
		public const string MaskReasonCode = "Reason Code";
		public const string MaskVendor = "Vendor";
		#endregion

		#region Item Types
		public const string NonStockItem = "Non-Stock Item";
		public const string LaborItem = "Labor";
		public const string ServiceItem = "Service";
		public const string ChargeItem = "Charge";
		public const string ExpenseItem = "Expense";

		public const string FinishedGood = "Finished Good";
		public const string Component = "Component Part";
		public const string SubAssembly = "Subassembly";
		#endregion

		#region Valuation Methods
		public const string Standard = "Standard";
		public const string Average = "Average";
		public const string FIFO = "FIFO";
		public const string Specific = "Specific";
		#endregion

		#region Lot Serial Assignment
		public const string WhenReceived = "When Received";
		public const string WhenUsed = "When Used";
		#endregion

		#region Lot Serial Tracking
		public const string NotNumbered = "Not Tracked";
		public const string LotNumbered = "Track Lot Numbers";
		public const string SerialNumbered = "Track Serial Numbers";
		#endregion

		#region Lot Serial Issue Method
		public const string LIFO = "LIFO";
		public const string Sequential = "Sequential";
		public const string Expiration = "Expiration";
		public const string UserEnterable = "User-Enterable";
		#endregion

		#region Lot Serial Segment Type
		public const string NumericVal = "Auto-Incremental Value";
		public const string FixedConst = "Constant";
		public const string DayConst = "Day";
		public const string MonthConst = "Month";
		public const string MonthLongConst = "Month Long";
		public const string YearConst = "Year";
		public const string YearLongConst = "Year Long";
		public const string DateConst = "Custom Date Format";
		#endregion

		#region Transaction / Journal Types
		public const string Assembly = "Assembly";
		public const string Receipt = "Receipt";
		public const string Issue = "Issue";
		public const string Return = "Return";
		public const string Invoice = "Invoice";
		public const string DebitMemo = "Debit Memo";
		public const string CreditMemo = "Credit Memo";
		public const string Transfer = "Transfer";
		public const string Adjustment = "Adjustment";
		public const string Undefined = "Not Used in Inventory";
		public const string StandardCostAdjustment = "Standard Cost Adjustment";
		public const string NegativeCostAdjustment = "Negative Cost Adjustment";
		public const string ReceiptCostAdjustment = "Receipt Cost Adjustment";
		public const string NoUpdate = "No Update";
		public const string Production = "Production";
		public const string Change = "Change";
		public const string Disassembly = "Disassembly";
		public const string DropShip = "Drop-Shipment";

		#endregion

		#region Transfer Types
		public const string OneStep = "1-Step";
		public const string TwoStep = "2-Step";
		#endregion

		#region Item Status
		public const string Active = "Active";
		public const string NoSales = "No Sales";
		public const string NoPurchases = "No Purchases";
		public const string NoRequest = "No Request";
		public const string Inactive = "Inactive";
		public const string ToDelete = "Marked for Deletion";
		public const string Template = "Template";
		#endregion

		#region Qty Allocation Doc Type
		public const string qadSOOrder = "SO Order";
        public const string qadFSServiceOrder = "FS Order";
        #endregion

        #region Document Status
        public const string Hold = "On Hold";
		public const string Balanced = "Balanced";
		public const string Released = "Released";

		// some additional statuses for PIHeader
		public const string Counting = "Counting In Progress";
		public const string DataEntering = "Data Entering";
		public const string InReview = "In Review";
		public const string Completed = "Completed";
		public const string Cancelled = "Canceled";

		// some additional statuses for PIDetail
		public const string NotEntered = "Not Entered";
		public const string Entered = "Entered";
		public const string Voided = "Voided";
		public const string Skipped = "Skipped";

		// LineType for PIDetail		
		//public const string Normal = "Normal"; // [Normal] is used from layer type
		public const string Blank = "Blank";
		public const string UserEntered = "UserEntered";

		// some additional statuses for PICountStatus
		// public const string InProgress = "In Progress"; // defined above
		public const string Available = "Available";
		public const string InProgress = "In Progress";
		public const string NotAvailable = "Not Available";

		#endregion

		#region Primary Item Validation
		public const string PrimaryNothing = "No Validation";
		public const string PrimaryItemError = "Primary Item Error";
		public const string PrimaryItemClassError = "Primary Item Class Error";
		public const string PrimaryItemWarning = "Primary Item Warning";
		public const string PrimaryItemClassWarning = "Primary Item Class Warning";
		#endregion

		#region Location Validation Types
		public const string LocValidate = "Do Not Allow On-the-Fly Entry";
		public const string LocNoValidate = "Allow On-the-Fly Entry";
		public const string LocWarn = "Warn But Allow On-the-Fly Entry";
		#endregion

		#region Alternate Types
		public const string CPN = "Customer Part Number";
		public const string VPN = "Vendor Part Number";
		public const string Global = "Global";
		public const string Barcode = "Barcode";
		public const string Substitute = "Substitute";
		public const string Obsolete = "Obsolete";
		#endregion

		#region Layer Types
		public const string Normal = "Normal";
		public const string Oversold = "Oversold";
        public const string Unmanaged = "Unmanaged";
        #endregion

        #region Physical Inventory Types
        public const string ByInventory = "By Inventory";
		public const string ByLocation = "By Location";
		#endregion

		#region PrimaryItemValidationType

		public const string Warning = "Warning";
		public const string Error = "Error";

		#endregion


		#region INPriceOption

		public const string Percentage = "Percentage";
		public const string FixedAmt = "Fixed Amount";
		public const string Residual = "Residual";


		#endregion

		#region INReplenishmentType
		public const string None = "None";
		public const string MinMax = "Min./Max.";
		public const string FixedReorder = "Fixed Reorder Qty";
		#endregion

		#region INReplenishmentSource
		public const string Purchased = "Purchase";
		public const string Manufactured = "Manufacturing";
		public const string PurchaseToOrder = "Purchase to Order";
		//public const string TransferToOrder = "Transfer to Order";
		public const string DropShipToOrder = "Drop-Ship";
		#endregion

		#region Cost Source
		public const string AverageCost = "Average";
		public const string LastCost = "Last";
		#endregion

		#region PackageOption
		public const string Weight = "By Weight";
		public const string Quantity = "By Quantity";
		public const string WeightAndVolume = "By Weight & Volume";
		public const string Manual = "Manual";
		#endregion
		
		#region CostBasisOption
		public const string StandardCost = "Standard Cost";
		public const string PriceMarkupPercent = "Markup %";
		public const string PercentOfSalesPrice = "Percentage of Sales Price";
		public const string UndefinedCostBasis = "Undefined";
		#endregion

		#region Demand Period Type
		public const string Month = "Month";
		public const string Week = "Week";
		public const string Day = "Day";
		public const string Quarter = "Quarter";

		#endregion

		#region DemandForecastModelType
		public const string DFM_None = "None";
		public const string DFM_MovingAverage = "Moving Average";

		#endregion

		#region Demand Calculation
		public const string ItemClassSettings = "Item Class Settings";
		public const string HardDemand = "Hard Demand Only";
		#endregion
		#region CompletePOLine
		public const string ByAmount = "By Amount";
		public const string ByQuantity = "By Quantity";
		#endregion

		#endregion

		#region Custom Actions
		public const string Release = PM.Messages.Release;
		public const string ReleaseAll = PM.Messages.ReleaseAll;
		public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string ViewInventoryTranDet = "Inventory Transaction Details";
		public const string INEditDetails = "Inventory Edit Details";
		public const string INRegisterDetails = "Inventory Register Detailed";
		public const string INItemLabels = "Inventory Item Labels";
		public const string INLocationLabels = "Location Labels";
		public const string ViewDocument = "View Document";
		public const string GeneratePI = "Generate PI";
		public const string FinishCounting = "Finish Counting";
		public const string CancelPI = "Cancel PI";
		public const string CompletePI = "Complete PI";
		public const string InventorySummary = "Summary";
		public const string InventoryAllocDet = "Allocation Details";
		public const string InventoryTranSum = "Transaction Summmary";
		public const string InventoryTranHist = "Transaction History";
		public const string InventoryTranDet = "Transaction Details";
		public const string SetNotEnteredToZero = "Set Not Entered To Zero";
		public const string SetNotEnteredToSkipped = "Set Not Entered To Skipped";
		public const string UpdateCost = "Update Actual Cost";
		public const string BinLotSerial = "Allocations";
		public const string Generate = "Generate";
		public const string Add = "Add";
		public const string AddNewLine = "Add New Line";
		public const string ViewRestrictionGroup = "Group Details";
		public const string ApplyRestrictionSettings = "Apply Restriction Settings to All Inventory Items";
		public const string Calculate = "Calculate";
		public const string Clear = "Clear";
		public const string ttipRefresh = "Refresh";
		public const string ApplyToChildren = "Apply to Children";
		public const string ttipCutSelectedRecords = "Cut Selected Records";
		public const string ttipPasteRecords = "Paste Records";

		#endregion

		#region PI Generation Sort Order Combos
		public const string ByLocationID = "By Location";
		public const string ByInventoryID = "By Inventory ID";
		public const string BySubItem = "By Subitem";
		public const string ByLotSerial = "By Lot/Serial Number";
		public const string ByInventoryDescription = "By Inventory Description";
		#endregion

		#region PI Generation Methods
		public const string FullPhysicalInventory = "Full Physical Inventory";
		public const string ByCycleCountFrequency = "By Cycle Count Frequency";
		public const string ByMovementClassCountFrequency = "By Movement Class Count Frequency";
		public const string ByABCClassCountFrequency = "By ABC Code Count Frequency";
		public const string ByCycleID = "By Cycle";
		public const string LastCountDate = "Last Count On Or Before";
		public const string ByPreviousPIID = "By Previous Physical Count";
		public const string ByItemClassID = "By Item Class";
		public const string ListOfItems = "List Of Items";
		public const string RandomlySelectedItems = "Random Items (up to)";
		public const string ItemsHavingNegativeBookQty = "Items Having Negative Book Qty.";
		public const string ByMovementClass = "By Movement Class";
		public const string ByABCClass = "By ABC Code";
		public const string ByCycle = "By Cycle";

		#endregion

		public const string InTransit = "In-Transit";
		public const string InTransitLine = "Transfer Line";
		public const string InTransitS = "In-Transit [*]";
		public const string InTransit2S = "In-Transit [**]";
		public const string SOBooked = "SO Booked";
		public const string SOBookedS = "SO Booked [*]";
		public const string SOBooked2S = "SO Booked [**]";
		public const string SOAllocated = "SO Allocated";
		public const string SOAllocatedS = "SO Allocated [*]";
		public const string SOAllocated2S = "SO Allocated [**]";
		public const string SOShipped = "SO Shipped";
		public const string SOShippedS = "SO Shipped [*]";
		public const string SOShipped2S = "SO Shipped [**]";
		public const string INIssues = "IN Issues";
		public const string INIssuesS = "IN Issues [*]";
		public const string INIssues2S = "IN Issues [**]";
		public const string INReceipts = "IN Receipts";
		public const string INReceiptsS = "IN Receipts [*]";
		public const string Expired = "Expired";
		public const string ExpiredS = "Expired [*]";
		public const string ExceptExpiredS = "[*] Except Expired";
		public const string ExceptExpired2S = "[**] Except Expired and  Loc. Not Available";
		public const string TotalLocation = "Total:";

		public const string RegisterCart = "Receipt Cart";
		public const string RegisterCartLine = "Receipt Cart Line";
		public const string BuildingID = "Building ID";
		
		#region Matrix
		public const string AdditionalAttributesDAC = "Additional Attributes";
		public const string INMatrixGenerationRuleDAC = "Matrix Generation Rule";
		public const string IDGenerationRuleDAC = "ID Generation Rule";
		public const string DescriptionGenerationRuleDAC = "Description Generation Rule";
		public const string EntityHeaderDAC = "Entity Header";
		public const string EntityMatrixDAC = "Entity Matrix";
		public const string InventoryItemWithAttributeValuesDAC = "Inventory Item with Attribute Values";
		public const string TemplateAttributesDAC = "Template Attributes";
		public const string AttributeDescriptionGroupDAC = "Attribute Description Group";
		public const string AttributeDescriptionItemDAC = "Attribute Description Item";
		public const string LinesWithSameInventoryHaveDifferentUOM = "Specify the same UOM in all lines with the selected inventory item before using inventory matrix.";
		public const string ItIsNotAllowedToChangeStkItemFlagIfChildExists = "You cannot change the value of the Stock Item check box if the template item has at least one matrix item. Remove all matrix items of the template item first.";
		public const string ItIsNotAllowedToChangeMainFieldsIfChildExists = "You cannot change values of the Item Class, Base Unit, Sales Unit, Purchase Unit, and Sales Categories boxes if the template item has at least one matrix item. Remove all matrix items of the template item first.";
		public const string InventoryIDExists = "The item with the same inventory ID already exists. Change segment settings of the inventory ID.";
		public const string InventoryIDDuplicates = "The inventory ID is duplicated. Change segment settings of the inventory ID.";
		public const string SelectRow = "Select Row";
		public const string SelectColumn = "Select Column";
		public const string TotalQty = "Total Qty.";
		public const string StkItemSettingMustCoincide = "The item class specified for the stock item must be the same as in the template item.";
		public const string CantChangeAttributeCategoryForMatrixItem = "The value in the Category column cannot be changed if at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string CantChangeAttributeCategoryForMatrixTemplate = "The attribute category cannot be changed because this attribute is specified as the default column or row attribute for the following templates on the Template Items (IN203000) form: {0}. Select another attribute as the default column or row attribute for the templates first.";
		public const string CantChangeAttributeIsActiveFlagForMatrixItem = "The value of the Active check box for the variant attribute cannot be changed if at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string CantChangeAttributeIsActiveFlagForMatrixTemplate = "The Active check box cannot be cleared for this attribute because it is specified as the default column or row attribute for the following templates on the Template Items (IN203000) form: {0}. Select another attribute as the default column or row attribute for the templates first.";
		public const string CantDeleteVariantAttributeForMatrixItem = "The {0} attribute cannot be deleted because it is a variant attribute and at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string CantDeleteVariantAttributeForMatrixTemplate = "The {0} attribute cannot be deleted because it is specified as the default column or row attribute for the following templates on the Template Items (IN203000) form: {1}. Select another attribute as the default column or row attribute for all the templates first.";
		public const string CantAddVariantAttributeForMatrixItem = "The {0} variant attribute cannot be added if at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string AttributeIsInactive = "The {0} attribute is inactive. Specify an active attribute.";
		public const string SampleInventoryID = "Inventory ID Example: {0}";
		public const string SampleInventoryDescription = "Description Example: {0}";
		#endregion
	}
}
