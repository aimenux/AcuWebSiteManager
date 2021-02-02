using PX.Common;
using PX.Data;
using System;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Localizable Messages
    /// </summary>
    [PXLocalizable(Messages.Prefix)]
    public static class Messages
    {
        #region Validation and Processing Messages

        public const string Prefix = "AM Error";
        public const string Release = "Release";
        public const string ReleaseAll = "Release All";
        public const string Plan = "Plan";
        public const string PlanAll = "Plan All";
        public const string Select = "Select";
        public const string SelectAll = "Select All";
        public const string SetupNotEntered = "Required configuration data is not entered. Check the settings in {0}.";
        public const string CheckSettings = "Check the settings in {0}.";
        public const string ArgumentInObjectNameInvalid = "Arguments in '{0}' are invalid.";
        public const string UnableToCopyMaterialFromToBomID = "Cannot copy the {0} material item from {1} {2} BOM to the {3} {4} BOM. {5}";
        public const string CannotDeleteRecWithQtyCost = "Cannot delete records with issued quantity or cost.";
        public const string CannotDeleteOperationWithTransaction = "Cannot delete Operation {0} because {1} Batch {2} exists.";
        public const string ProductionOrderCreatedWithoutBom = "The {0} {1} production order was created without BOM.";
        public const string BomMatlCircularRefAttempt = "Cannot add the BOM item as a material. This creates a circular reference.";
        public const string SubItemIDRequiredForStockItem = "A subitem ID is required for stock items.";
        public const string ProdStatusOnHold = "Production order is on hold.";
        public const string ProdConfigNotFinish = "Configuration is not finished for the {0} {1} production order.";
        public const string CannotCreateProductionOrderNonInventory = "Cannot create a production order for the {0} non-inventory estimate item.";
        public const string CouldNotFindItem = "The item cannot be found.";
        public const string CannotCreateProdOrderForNonStock = "Cannot create a production order for a non-stock item.";
        public const string WarehouseMustBeSpecified = "Warehouse must be specified.";
        public const string Journal = "Journal";
        public const string BOMID = "BOM ID";
        public const string Warning = "Warning";
        public const string Continue = "Continue";
        public const string ContinueWithEstimateRemoval = "Continue with Estimate Removal";
        public const string Confirm = "Confirm";
        public const string ConfirmProcess = "Confirm Process";
        public const string ConfirmCancelTitle = "Confirm Cancel";
        public const string ConfirmDeleteTitle = "Confirm Delete";
        public const string FieldCannotBeZero = "{0} cannot be zero.";
        public const string ConfirmReleasedTransactionsExist = "The transaction line contains related transactions that were released. Are you sure you want to delete the line? (The released transactions will remain as processed without the parent transaction.)";
        public const string ConfirmReleasedBatchExist = "The batch contains related transactions that are released. Are you sure you want to delete the batch? (The released transactions will remain as processed without the parent transaction.)";
        public const string ConfirmCancelProductionOrder = "Are you sure you want to cancel the production order?";
        public const string ConfirmCompleteMessage = "Are you sure you want to complete the production order?";
        public const string ProdStatusChangeInvalid = "The status of the production order cannot be changed from {0} to {1}.";
        public const string ProdStatusDeleteInvalid = "A production order with the {0} status cannot be deleted.";
        public const string ProdStatusHoldInvalid = "A production order with the {0} status cannot be placed on hold.";
        public const string ProdStatusInvalidForProcess = "The {0} {1} production order with the {2} status cannot be used in this process.";
        public const string NoDefaultBOM = "No active default BOM found.";
        public const string InvalidProductionNbr = "The {0} {1} production order is not found.";
        public const string InvalidProductionOperationNbr = "Operation ID ({2}) of Production Order {0} {1} is not found.";
        public const string ProcessComplete = "Process Complete";
        public const string ProcessError = "Process Error";
        public const string CreateOrdersError = "Error creating order";
        public const string CreateProductionOrders = "Production Orders Created";
        public const string MultiLevelRunAll = "Are you sure you want to run the report for all BOM IDs?";
        public const string NonStkNoExpAccrualAcct = "An Expense Accrual account for the {0} non-stock item was not found.";
        public const string MissingLaborCode = "Labor Code {0} is not found.";
        public const string ErrorCreatingGLInventoryEntry = "An error message occurred when creating an inventory GL transaction.";
        public const string ErrorInAMGLentry = "The following error message occurred during creation of a GL entry: {0}";
        public const string ErrorInCreateGLLine = "A GL entry for Line {2} of {0} Batch {1} cannot be created.";
        public const string RecordMissing = "The {0} record is missed.";
        public const string RecordMissingWithID = "The {0} record is missed for the {1} ID.";
        public const string InvalidPeriodToPost = "The period to post is invalid.";
        public const string BomExists = "The entered BOM ID {0} exists.";
        public const string BomRevisionExists = "Revision {1} of BOM {0} is currently in use.";
        public const string NoBomWarning = "No active BOM exists for the current combination of the inventory ID and warehouse ID.";
        public const string NoBomWarningSubItem = "No active BOM exists for the current combination of the inventory ID, warehouse ID, and subitem.";
        public const string InvalidRevisionWarning = "The revision is invalid for the current BOM.";
        public const string Create = "Create";
        public const string LSAMMTranLinesUnassigned = "One or more lines require a warehouse location, lot number, or serial number to be assigned.";
        public const string EntryGreaterEqualZero = "Enter the value that is greater than or equal to {0}.";
        public const string BomMatlDupItems = "A duplicate inventory item was found in Operation {0} of Revision {2} of BOM {1}.";
        public const string RemoveEstimateConfirmation = "Revision {1} of Estimate {0} will be removed from this order.";
        public const string UnableToDeleteEstimate = "The estimate cannot be deleted because it is referenced on {0} {1}.";
        public const string InvalidItemClass = "The selected item class ID is not valid for the stock item.";
        public const string MissingFixMfgCalendar = "The fixed manufacturing calendar is missing.";
        public const string UnableToUpdateField = "The {0} field cannot be updated. {1}";
        public const string WorkCenterMustHaveOneShift = "For the {0} work center, specify at least one shift.";
        public const string NegQtyGreaterThanOperQtyComplete = "The total reversing quantity of {0} {2} cannot be greater than the operation quantity complete of {1} {2} for the {3} {4} production order at Operation {5}.";
        public const string UnableToVerifyQtyforNegMove = "Cannot verify the original quantity moved for a negative transaction with the {0} {1} production order.";
        public const string NoBatchCreated = "No batch has been created.";
        public const string NoLaborCodeForShift = "The direct labor was not set up for this shift in the current operation.";
        public const string OverheadAccountRequiredIndirectLabor = "The overhead account and/or subaccount is not specified for the {0} indirect labor code.";
        public const string MachineUsedinActiveWorkCenter = "The record cannot be deleted because it is used by the {0} work center.";
        public const string InvalidWorkCalendar = "The {0} work calendar is invalid.";
        public const string InvalidBOM = "The BOM is invalid.";
        public const string OneDefaultSubItem = "You can specify only one default value for a subitem.";
        public const string LaborCodeNoDeleteUsedInWC = "The labor code cannot be deleted because it is used by the {0} work center for the {1} shift.  Select another labor code on the Work Centers (AM207000) form before deletion.";
        public const string UnableToSaveRecordForInventoryID = "The {0} record for the {1} inventory ID cannot be saved. {2}";
        public const string CreatedFromOrderTypeOrderNbr = "Created from {0} Order {1}";
        public const string CreatedFromParentProductionOrder = "Created from parent Production Order {0} {1}";
        public const string QuantityGreaterThanZero = "Specify the quantity greater than 0.";
        public const string MustSelectSeasonality = "A seasonality must be selected.";
        public const string MustChooseForecastDate = "A forecast date must be selected.";
        public const string MustBeGreaterThan = "{0} must be greater than {1}.";
        public const string MustBeGreaterThanOrEqualTo = "{0} must be greater than or equal to {1}.";
        public const string NotComplete = "The MRP regeneration process was not completed.";
        public const string Exception = "Exception";
        public const string ExceptionIn = "Exception in {0}";
        public const string MpsMaintPlanDateWarning = "The plan date is outside of the MPS fence.";
        public const string MrpSetupExceptionWindow = "The {0} should be less than or equal to the {1}.";
        public const string Runtime = "Runtime: {0}";
        public const string ForecastNumberSequenceSetupNotEntered = "The forecast numbering sequence is required to use MRP forecasts. Check the settings in {0}.";
        public const string ForecastNumberSequenceMustBeAutoNumbered = "The forecast numbering sequence with automatic numbering is required to process forecasts.";
        public const string Failed = "Failed";
        public const string ConfirmCalculateMessage = "Calculation will delete all unprocessed records. Do you want to continue?";
        public const string DeletedTransactionWithReference = "The line referenced to the released {3} {2} Transaction {4} has been deleted from {0} Batch {1}.";
        public const string ExceptionInUpdatingProductionCosts = "An exception occurred when updating production costs for the {0} {1} production order.";
        public const string StatusChangedTo = "The {0} status was changed to {1}.";
        public const string ProductionOrderStatusUpdatedTo = "The status of the {0} {1} production order was updated to {2}.";
        public const string ProductionOrderStatusChangeError = "The following error message occurred when changing the status of the {0} {1} production order from {2} to {3}: {4}.";
        public const string ItemIsNotStandardCost = "The valuation method for the inventory item is not Standard.";
        public const string CalendarHelperUnableToAddDays = "The following error occurred in production scheduling by using the {2} calendar ID: Unable to add {0} days to the {1} date.";
        public const string MissingCalendarID = "The calendar ID is required.";
        public const string ErrorInCopyBomProcess = "The following error occurred when the system tried to copy the BOM: {0}";
        public const string UnableToInsertDAC = "Data cannot be inserted in the {0} DAC.";
        public const string UnableToUpdateDAC = "Data cannot be updated in the {0} DAC.";
        public const string MaterialWizardBatchCreationError = "An error occurred during the batch creation by the Material Wizard. See the Trace for details.";
        public const string LSLotSerialStatus = "Item {0} {1}, Warehouse {2} {3}, Lot/Serial Number {4}, On Hand {6} {5}, Available {7} {5}";
        public const string LSLocationStatus = "Item {0} {1}, Warehouse {2} {3}, On Hand {5} {4}, Available {6} {4}";
        public const string LSSiteStatus = "Item {0} {1}, Warehouse {2}, On Hand {4} {3}, Available {5} {3}";
        public const string LSTranStatus = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}";
        public const string LowLevelUnableToUpdateItem = "The {0} item cannot be updated: {1}";
        public const string LowLevelMaxErrorsReceived = "Low level process: max number of errors reached. See the Trace for details.";
        public const string LowLevelMaxLevelReachedForItem = "Max low level has been reached for the {0} item and the {1} low level.";
        public const string LowLevelMaxLevelReached = "Calculated low level reached max low level of {0}. Review BOMs for potential circular reference.";
        public const string InvalidTranTypeForBuilding = "The inventory transaction cannot be generated because of an incorrect transaction type.";
        public const string CreatedNewINSubItemRecord = "The new INSubItem record has been created for the {0} CD.";
        public const string ProductionOrderUnreleasedTransactions = "The {0} {1} production order has unreleased transactions.";
        public const string MakeDefaultBomFailed = "The default BOM cannot be updated. See the Trace for details.";
        public const string MakePrimaryBomFailed = "The planning BOM cannot be updated. See the Trace for details.";
        public const string ReportParametersInvalid = "Report parameters are incorrect.";
        public const string ProcessIsCurrentlyRunning = "The process is currently running by the {0} user with the {1} current runtime.";
        public const string UpgradeToVersion = "Upgrade to version {0} {1}. {2}";
        public const string SeeTraceWindow = "See the Trace for details.";
        public const string ConfirmSOLinkedOrderDelete = "The order is linked to the sales order. Do you want to continue?";
        public const string ConfirmCancelProductionOrderWithSORef = "The order is linked to the sales order. Are you sure you want to cancel the production order?";
        public const string ShiftIDInUse = "The {0} shift cannot be removed because it is used in the {1} work center.";
        public const string OrderUpdatingInLongProcess = "Order is currently being processed. Wait for long running process to complete.";
        public const string RescheduleOrderException = "The {0} {1} order cannot be rescheduled.";
        public const string RecalculatePlanCostException = "Plan costs cannot be recalculated for the {0} {1} order.";
        public const string ByProductBackflushLS = "For lot or serial numbered by-products, the sequential issue method must be set up. The {0} by-product will not be backflushed.";
        public const string InventoryIDAlreadyExists = "The {0} inventory ID already exists.";
        public const string InventoryIDCouldNotBeCreated = "The {0} inventory ID cannot be created. {1}";
        public const string UnableToUpdateEstimateRevisionsUsingInventoryID = "The estimate revisions cannot be updated by using the {0} inventory ID.";
        public const string StartEndTimeMoreThan24Hours = "The period between start and end time is more than 24 hours.";
        public const string SchdDateDetailDateMismatch = "Dates in the date details do not match the requested date.";
        public const string CantChangeActiveRevisionStatusHasResult = "The status of the revision cannot be changed because at least one result has already been generated.";
        public const string CantChangeActiveRevisionStatusHasPendingOrActive = "The status of the revision cannot be changed because another pending or active revision exists.";
        public const string UnableToProcess = "Unable to process {0}";
        public const string StatCantCreateNextRevision = "The new revision cannot be created because the status of the last one is {0}.";
        public const string NotConfigurableItem = "This item is not configurable.";
        public const string ConfirmChangeSourceDeleteConfigResults = "Changing the source will delete the current configuration. Are you sure you want to proceed?";
        public const string ConfirmDeleteConfigResults = "Are you sure you want to delete the current configuration ?";
        public const string QtyCannotBeLessThanMinQty = "The quantity ({0}) cannot be less than the minimum quantity ({1}).";
        public const string QtyCannotBeGreaterThanMaxQty = "The quantity ({0}) cannot be greater than the maximum quantity ({1}).";
        public const string SelMustBeGreaterThanMinSel = "The selection ({0}) must be greater than the minimum selection ({1}).";
        public const string SelMustBeSmallerThanMaxSel = "The selection ({0}) must be smaller than the maximum selection ({1}).";
        public const string RequiredOptionNotIncluded = "A required option is not included.";
        public const string ProductionAlreadyPlanned = "The production has already been planned.";
        public const string ConfigurationRevisionPending = "The {0} configuration of the {1} revision is pending.";
        public const string CannotCreateBOMForNonStock = "A BOM cannot be created for a non-stock item.";
        public const string EstimateWarehouseRequiredForBOM = "Estimate must have a warehouse selected to create a BOM.";
        public const string CannotCreateBOMNonInventory = "A BOM cannot be created for the {0} non-inventory estimate item.";
        public const string UnableToCreateBomFromEstimateMaterial = "A BOM cannot be created from the {1} revision of the {0} estimate. The {2} material is not valid.";
        public const string UnableToCreateBomFromEstimateTool = "A BOM cannot be created from the {1} revision of the {0} estimate. The {2} tool is not valid.";
        public const string UnableToCreateBomFromEstimateOverhead = "A BOM cannot be created from the {1} revision of the {0} estimate. The {2} overhead is not valid.";
        public const string Tool_NotDeleted_OnEstimate = "The tool cannot be deleted because it is used in the {1} revision of the {0} estimate.";
        public const string Overhead_NotDeleted_OnEstimate = "The overhead cannot be deleted because it is used in the {1} revision of the {0} estimate.";
        public const string Overhead_NotDeleted_OnBOM = "The overhead cannot be deleted because it is used in the {0} BOM.";
        public const string Tool_NotDeleted_OnBOM = "The tool cannot be deleted because it is used in the {0} BOM.";
        public const string Tool_NotDeleted_OnProd = "The tool cannot be deleted because it is used in the {0} {1} production order.";
        public const string Overhead_NotDeleted_OnProd = "The overhead cannot be deleted because it is used in the {0} {1} production order.";
        public const string EstimateReferenceMissingSalesOrderNbrOrType = "The estimate cannot be added to the sales order because the sales order number or type is missed.";
        public const string EstimateReferenceMissingOpportunityID = "The estimate cannot be added to the opportunity because the opportunity ID is missed.";
        public const string EstimateReferenceSalesOrderNotFound = "The estimate cannot be added to the sales order because the sales order is not found.";
        public const string EstimateReferenceOpportunityNotFound = "The estimate cannot be added to the opportunity because the opportunity is not found.";
        public const string CannotAddEstimateSalesOrderClosedCancelled = "The estimate cannot be added to the sales order because the sales order cannot be modified.";
        public const string CannotAddorEditEstimateOpportunityClosed = "The estimate cannot be added to the opportunity because the opportunity cannot be modified.";
        public const string CannotAddorEditEstimateOpportunityQuoteClosed = "The estimate cannot be added to the opportunity quote or edited because the quote cannot be modified.";
        public const string EstimateReferenceMissing = "The estimate reference is missed.";
        public const string EstimateCannotBeNonStock = "The {0} item is a non-stock item. Select a stock item.";
        public const string EstimateItemClassMustBeStockItem = "The {0} item class is used for non-stock items. Select an item class for stock items.";
        public const string Required = "Required";
        public const string CantDeleteConfigurationUnlessPending = "The configuration cannot be deleted because its status is not Pending.";
        public const string PrintMaterial = "Print Material";
        public const string PrintMaterialConfirmation = "Do you want to print the material on the report?";
        public const string NeedsUpgrade = "To complete the upgrade process, check settings in {0}.";
        public const string ProductionOrderConfigurationUncompleted = "The {0} item cannot be sent to production because its configuration is not completed.";
        public const string ConfigurationUsedForItemSite = "The configuration cannot be deleted because it is the default configuration for the {0} item in the {1} warehouse.";
        public const string ProductionOrderIDIsAlreadyUsed = "The {0} production number is used for the {1} order type. Select another number.";
        public const string OrderTypeInUseCannotBeDeleted = "The {0} order type cannot be deleted because it is in use.";
        public const string NotActiveRevision = "The item does not have an active configuration revision assigned.";
        public const string DeletingExistingConfiguration = "Changing this box value will delete the assigned configuration. Do you want to continue?";
        public const string BomOperNbrOnConfiguration = "Operation {1} of Revision {5} of BOM {0} is used in Feature {4} of Revision {3} of Configuration {2}.";
        public const string OnlyOneProdMatlCanBeSelectedUserNumbering = "Only one record can be selected. User numbering {0} is manual.";
        public const string TransferIDIsAlreadyUsed = "The {0} transfer is already used in the {1} order type.";
        public const string PurchaseOrderIDIsAlreadyUsed = "The {0} purchase order is already used in the {1} order type.";
        public const string CannotMakeOrderTypeInactiveOpenOrders = "The {0} order type cannot be made inactive. Open orders of this type exist in the system.";
        public const string OrderTypeIsReferenced = "The {0} order type is configured as {1} in {2}.";
        //public const string SetActiveBeforeItemDefault = "Set active to use as default for item.";
        public const string IncorrectOrderTypeFunction = "The function in the order type is incorrect.";
        public const string NumberingMissingExceptionProduction = "A numbering sequence for the {0} order type is not configured.";
        public const string TransferCannotContainMultipleWarehouses = "Cannot plan Transfer for multiple warehouses. All records selected must be from the same Warehouse";
        public const string ProductionOrderIDIsAlreadyUsedOrderCreate = "The {0} production order number is already used for the {1} order type.";
        public const string CreatedPurchaseOrder = "Purchase Order Created";
        public const string CreatedTransferOrder = "Transfer Order Created";
        public const string UnableToConvertEstimatesForOpportunity = "Estimates for the {0} opportunity cannot be converted.";
        public const string EstimateItemMustBeStockItem = "Estimate item must be a stock item.";
        public const string EstimateMaterialMustBeItems = "An estimate material must be a stock or non-stock item.";
        public const string UnableToConvertEstimateDueTo = "The {1} revision of the {0} estimate cannot be converted due to the following error message: {2}";
        public const string LocationAssemblyInvalid = "The selected location cannot be used for assembly.";
        public const string UnableToUpdateSalesOrderFromProductionOrder = "The {2} line of the {0} {1} sales order cannot be updated from the {3} {4} production order: {5}";
        public const string FieldCannotBeChangedLinkedToSO = "{0} cannot be changed. The {1} {2} production order is linked to the {3} {4} sales order.";
        public const string CannotDeleteSupplementalSalesLine = "The {0} sales line cannot be deleted because it is a supplemental item for {1}.";
        public const string CannotDeleteSupplementalOpportunityLine = "The {0} product cannot be deleted it is a supplemental item for {1}.";
        public const string TranLineRequiresAttribute = "Line {2} of {0} Transaction {1} requires Attribute {3}";
        public const string ConfirmOperationDeleteWhenAttributesExist = "The operation contains attributes. Do you want to delete the operation?";
        public const string CannotDeleteSourceAttributes = "The {0} source attributes cannot be deleted.";
        public const string UnderIssuedMaterial = "The {0} material is under issued in the {4} line of the {3} operation of the {1} {2} order.The current issued quantity is {6} {5}. The required quantity is {7} {5}";
        public const string ProductionComplete = "Production {0} {1} {2} {3} is completed.";
        public const string TransactionQtyOverCompleteRemaining = "Transaction quantity of {1} {0} will over complete the remaining quantity of {2} {0} in the {3} {4} production order.";
        public const string EstimateClass_NotDeleted = "The estimate class cannot be deleted because it is used in {0} {1}.";
        public const string EstimateInventoryCDNotValidDimension = "The {0} estimate inventory does not comply with the settings of the INVENTORY segmented key.";
        public const string ConfiguraitonIncompletePortal = "The {0} item for {1} {2} has an incomplete configuration.";
        public const string ConfiguraitonIncompleteSitePortal = "The {0} item for {1} {2} at warehouse {3} has an incomplete configuration.";
        public const string PasteLine = "Paste Line";
        public const string ResetLines = "Reset Lines";
        public const string FeatureInUseCannotDelete = "The feature cannot be deleted because it is used in the {1} revision of the {0} configuration.";
        public const string FeatureDoesntAllowNonInventory = "You cannot use non-inventory options for the {0} feature.";
        public const string OverheadIDCannotBeUpdated = "The overhead ID cannot be changed.";
        public const string OrderHasProductReferences = "Linked Orders by Product:";
        public const string OrderHasParentReferences = "Linked Orders by Parent:";
        public const string ManualNumberingKeyNumberingDisabled = "A numbering sequence with manual numbering cannot be used for a configuration ID.";
        public const string MaterialQuantityOverIssue = "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued in the {6} line for the {5} item of the {4} operation in the {3} order.";
        public const string MaterialQuantityOverIssueShortMsg = "Material Quantity {1} {0} to be issued is greater than the quantity remaining to be issued {2} {0}.";
        public const string MaterialQuantityOverIssueWithUnreleasedQty = "The material quantity of {1} {0} to be  issued is greater than the remaining quantity of {2} {0} to be issued(includes the unreleased batch quantity of { 7} {0}) in the {6} line for the {5} item of the {4} operation in the {3} order.";
        public const string BackFlushMaterialShortage = "Backflush material shortage of {1} {0} was found for the {2} item in the {6} line of the {5} operation in the {3} {4} order.";
        public const string ProductionOrderNotCreated = "A production order cannot be created for the {0} item in the {1} operation.";
        public const string DocumentOnHoldCannotRelease = PX.Objects.IN.Messages.Document_OnHold_CannotRelease;
        public const string ScrapWarehouseLocationCannotBeNull = "The scrap warehouse or location in the {0} {1} production order is empty.";
        public const string NegQtyGreaterThanTotQtyComplete = "The total reversing quantity of {0} {3} cannot be greater than the difference of the operation quantity complete of {1} {3} and the total order quantity complete of {2} {3} for the {6} operation of the {4} {5} production order.Reversing entry must be performed at the last operation.";
        public const string MPSTypeReferencedSetup = "The {0} MPS type ID was specified in MRP preferences as the default type.";
        public const string MPSTypeReferencedActiveMPS = "The {0} MPS type ID is used in an active MPS for the {1} inventory ID.";
        public const string OrderAttributeNotFound = "The {0} transaction attribute was not found in the {1} {2} production order.";
        public const string DisassemblyNumberingRequired = "A disassembly numbering sequence must be specified before creating a disassembly batch.";
        public const string DisassemblyRefLineNbrInvalid = "Disassembly ref line nbr {0} invalid";
        public const string UnableToReleaseRelatedTransactions = "The {0} reference line number of the disassembly is invalid.";
        public const string JournalEntryIsReleased = "The {0} reference line number of the disassembly is invalid.";
        public const string TransactionRequiresAttributes = "The transaction requires attributes.";
        public const string InvalidValuesOnOneOrMoreBOMS = "One or more BOMs contain invalid values. See the Trace for details.";
        public const string InvalidInventoryIDOnBOM = "The inventory ID is invalid for the {0} BOM.";
        public const string InvalidUOMForMaterialonBOM = "The {0} UOM is invalid for the {1} item used in the {2} revision of {3} BOM.";
        public const string OrderTypeIsRequiredForProductionOrderCreation = "An order type is required to create a production order.";
        public const string DeletingFeatureIsReferencedOnRules = "The {0} feature is used in the {1} rules that will be deleted together with the feature. Do you want to continue? (Click No to cancel deletion and view the rules in the Trace.)";
        public const string DeletingFeatureOptionIsReferencedOnRules = "The {0} option of the {1} feature is used in the {2} rules that will be deleted together with the option. Do you want to continue? (Click No to cancel deletion and view the rules in the Trace.)";
        public const string UnableToDeleteFeatureWithRules = "The {0} feature cannot be deleted because it is used in rules.";
        public const string UnableToDeleteFeatureOptionWithRules = "The {0} option of the {1} feature cannot be deleted because it is used in rules.";
        public const string UnableToDeleteAttributeWithVariableInUse = "Unable to delete attribute {0} while variable {1} is referenced in formulas.";
        public const string UnableToChangeVariableInUse = "The {0} attribute cannot be deleted because the {1} variable is used in formulas.";
        public const string ItemNotOnProductionOrder = "The {0} inventory ID does not exist in the {1} {2} production order.";
        public const string ManualNumberingNotAllowedForProcess = "The {0} order type cannot be used in this process because the type uses manual numbering.";
        public const string ChangingBlockSizeMsg = "Changing the block size will result in a change to the current schedule records when saved. This process could take a long time to complete and should be avoided when production orders are being created and transactions are being processed. Do you want to continue?";
        public const string MaterialExpiredOnBom = "The material item is expired in the {0} revision of {1} BOM.";
        public const string BomRevisionIsNotActive = "The {0} revision of the {1} BOM is inactive.";
        public const string NoActiveRevisionForBom = "No active revisions for the {0} BOM were found.";
        public const string BomRevisionIsArchived = "The {0} revision of the {1} BOM is archived.";
        public const string MrpFixedLeadTimeRequiresProdPreferencesCalendar = "Using fixed lead times in MRP requires the fixed calendar to be configured in production preferences.";
        public const string UnableToProcessRecord = "The record cannot be processed.";
        public const string PeriodHasUnreleasedProductionDocs = "Unreleased production documents exist for the {0} financial period.";
        public const string IncorrectDocTypeForProcess = "The {0} document type is incorrect of the process.";
        public const string OrderNotFound = "The order was not found.";
        public const string CannotAddToClosedOrder = "The estimate cannot be added to the order";
        public const string UnableToInsertMaterialToProductionOrder = "The {0} material item cannot be added to the {1} operation of the {1} {2} production order.";
        public const string WorkCenterNotActive = "The work center is inactive.";
        public const string ErrorInsertingProductionOperation = "The following error message occurred during inserting the Production Order Operation {1} from Operation '{4}' '{5}' of  {0} '{2}' '{3}' with Work Center {6}: {7}";
        public const string CalculateSalesPriceForItemError = "The following error message occurred during calculation of a sales price for the {0} item: {1}";
        public const string CalculateSalesPriceError = "The following error message occurred during calculation of a sales price: {0}";
        public const string NoWorkDaysForCalendarWorkCenter = "A work date for the {0} work center cannot be found.";
        public const string UnableToDelete = "{0} cannot be deleted.";
        public const string UnableToCreateWorkDaysForWC = "Work days for some work centers from {0} to {1} cannot be created.";
        public const string NoActiveBomForPhantomMaterial = "No active BOM revision was found for the {0} phantom material in the {4} line of the {3} operation of the {1}:{2} BOM at the {5} level.";
        public const string TimeReportingModuleNotEnabled = "The Time Reporting on Activity feature must be enabled on the Enable/Disable Features (CS100000) form to use this process.";
        public const string ECRRequired = "A new revision must be created by using an engineering change request.";
        public const string UnableToUpdateAUDefinition = "The {0} automation definition cannot be updated.";
        public const string AUDefinitionUpdated = "The {0} automation definition has been updated..";
        public const string ProductionOrderNotEditable = "The {0} {1} production order cannot be edited.";
        public const string ChangingOperationCDQtyCompleteLessThan = "The operation ID cannot be inserted or changed. The quantity complete of {2} {0} in the {1} operation is less than the quantity complete {4} {0} in the {3} operation.";
        public const string ChangingOperationCDQtyCompleteGreaterThan = "The operation ID cannot be inserted or changed.The quantity complete of {2} {0} in the {1} operation is greater than the quantity complete {4} {0} in the {3} operation.";
        public const string ProductionOrderOperationUnreleasedTransactions = "The {2} operation of the {0} {1} production order has related transactions that are not released.{3}";
        public const string UnableToConvertToOrderNonInventory = "The {0} non-inventory estimate item cannot be converted in the {2} revision of the {1} estimate. Only stock items can be converted.";
        public const string EstimateNumberingSequenceIncorrect = "The required configuration for the estimate numbering sequence is incorrect. Check settings in {0}.";
        public const string EstimateCopyRequiresAutoNumber = "The estimate with the {0} ID cannot be copied because the numbering sequence for the ID is set to be specified manually. Only estimates with auto increased IDs can be copied.";
        public const string ErrorCalculatingConfigPriceFromBomMatl = "The configuration price in the {3}{4} material line of the {2} operation in the {0} {1} BOM cannot be calculated.";
        public const string GLBatchDebitLineMissing = "A debit line for {1} {2} {3} in the {0} GL batch is missed";
        public const string GLBatchCreditLineMissing = "A credit line for {1} {2} {3} in the {0} GL batch is missed";        
        public const string DuplicateSerialNumber = "The {0} item with the {1} serial number in the {2} {3} production order has already been issued.";
        public const string DuplicateSerialNumberInDocument = "The {0} inventory item has the duplicate {1} serial number in the {2} production order.";
        public const string UnableToGetFirstTransaction = "The first transaction cannot be retrieved.";
        public const string InvalidOperationOnTransaction = "The {0} line of the {1} transaction contains an incorrect operation ID.";
        public const string WarehouseIsCurrentWorkCenterWarehouse = "Cannot use current Work Center Warehouse";
        public const string InvalidWorkCenterSubstitute = "The current warehouse in the work center cannot be used.";
        public const string ChangingWarehouseWillDeleteSubstitue = "Changing the warehouse will cause deletion of a work center substitute for the same warehouse. Do you want to continue?";
        public const string ComponentBOMRevisionNotActive = "The {1} revision of the {0} comp BOM for the {2} inventory ID is inactive.";
        public const string BomIsNotActive = "The {0} BOM is inactive.";
        public const string RecursiveBomFound = "The recursive {0} {1} BOM was found.";
        public const string CreateLinkedOrdersRequiresAutoNumber = "The linked orders for the {1} numbering ID of the {0} order type cannot be created because the numbering sequence is not auto increased.";
        public const string MaxAutoLinkedOrders = "Max {0} auto create production order levels reached.";
        public const string ValueChangedFromEstimateValue = "The {0} value has been changed from the {1} estimate value.";
        public const string CannotCreateProductionOrderFromEstimateNonInventory = "A production order cannot be created from the {1} revision of the {0} estimate for the {2} non-inventory estimate material in the {3} operation.";
        public const string WorkCenterOperationMismatch = "The {0} work center does not match the {1} operation work center.";
        public const string LotSerialNotIssuedToOrder = "The material cannot be returned because the item with the {0} lot or serial number has not been issued for the {1} {2} production order.";
        public const string ByproductLotSerialNotReceivedWithOrder = "The by-product material cannot be returned because the item with the {0} lot or serial number has not been received for the {1} {2} production order.";
        public const string StartDateBeforeToday = "The start date is earlier than the today’s date.";
        public const string UnableToUpdateEstimateInventory = "The {0} {1} estimate cannot be updated in the {3} revision of the {2} estimate because {0} cannot be empty.";
        public const string SalesLineIsLInkedToProduction = "{0} cannot be changed because the sales line is linked to the {1} {2} production order.";
        public const string BothValuesMustBePosOrNeg = "{0} and {1} must both be either positive or negative.";
        public const string ErrorProcessingProdMaterial = "The following error message occurred during processing of the {0} production material in the {4} line of the {3} operation of the {1} {2} order: {5}";
        public const string CannotDeleteProdEvent = "The production event cannot be deleted.";
        public const string UnableToProcessForecastBucketsForItem = "The following error message occurred when processing forecast buckets for the {0} inventory ID: {1}";
        public const string SubcontractSourceNotValidForMaterialType = "The {0} subcontract source cannot be used for the {1} material type.}";
        public const string VendorShipmentNumberingRequired = "A numbering sequence must be specified for vendor shipments before creating.";
        public const string NoMaterialOnOperationForThisProcess = "The material is not specified for the operation in this process.";
        public const string NoVendorSelectedOrPreferredVendor = "Select a vendor to proceed.";
        public const string UnableToReleaseMaterialForShipment = "The {0} material batch for the {1} vendor shipment cannot be released. See the Trace for details.";
        public const string ReleasedMaterialBatchForShipment = "The {0} material batch for the {1} vendor shipment has been released.";
        public const string ShipLineCannotDeleteIsReleased = "The {1} line in the {0} shipment cannot be deleted because the line has been released.";
        public const string EmployeeNotProduction = "The employee is not a production employee.";
        public const string EmployeeAlreadyExists = "The employee has an existing clock entry record.";
        public const string EmployeeNotCurrentUser = "The clock entry cannot be updated because the selected employee is not assigned to your username. Select the employee assigned to your username or sign in with the username assigned to the selected employee.";
        public const string UnableToCreateRelatedTransaction = "The related transaction cannot be created.";

        #region Formula/Rule 
        public const string Rules = "Rules";
        public const string NotRespectedRules = "Rules are not respected";
        public const string ConflictingRules = "Conflicting Rules";
        public const string RuleRecursive = "Recursive rules";
        public const string RuleAttributeFormula = "Recursive Attribute Formulas";
        public const string ActiveRules = "Active Rules";
        public const string UnactiveRules = "Inactive Rules";

        public const string RuleFeatureOptionIncluded = "The {0} option from the {1} feature is included.";
        public const string RuleFeatureOptionNotIncluded = "The {0} option from the {1} feature is not included.";
        public const string RuleFeatureAnyOptionIncluded = "An option from the {0} feature is included.";
        public const string RuleFeatureAnyOptionNotIncluded = "No option from the {0} feature is included.";

        public const string RuleAttributeNoValue = "Value {0} from Attribute {1} {2}";
        public const string RuleAttributeValue1 = "Value {0} from Attribute {1} {2} {3}";
        public const string RuleAttributeValue2 = "Value {0} from Attribute {1} {2} {3}, and {4}";

        public const string RuleExistsOnFeature = "The {0} feature contains a rule ({1}) for the {3} option of the {2} target feature.";
        public const string RuleExistsOnAttribute = "The {0} attribute contains a rule ({1}) for the {3} option of the {2} target feature.";
        #endregion
        #endregion

        #region Translatable Strings used in the code

        public const string Error = "Error";
        public const string CreatingProductionOrders = "Creating production orders";
        public const string UnknownDocType = "Unknown Document Type";
        public const string UnknownTranType = "Unknown Transaction Type";
        public const string ProductionStatusFromTo = "The status has been changed from {0} to {1}.";
        public const string Transaction = "Transaction";
        public const string CreatedByDocTypeBatchNbr = " Created by Document Type {0}, Batch {1}";
        public const string DeletedOperationEvent = "The {0} {1} operation in the {2} work center has been deleted.";
        public const string EstimateCreated = "The {0} estimate revision has been created.";
        public const string EstimateCreatedPrimary = "The {0} estimate revision has been created as the primary revision.";
        public const string EstimateRevisionUpdated = "The {0} estimate revision has been updated.";
        public const string EstimateCreatedProdOrder = "The {1} {0} production order has been created from the {2} revision.";
        public const string CreatedProductionOrderFromMrp = "The {0} {1} production order has been created from MRP.";
        public const string EstimateCreatedOpportunity = "The {0} production order has been created from the {1} opportunity.";
        public const string EstimateStatusChangedFromTo = "The estimate status has been changed from {0} to {1}.";
        public const string EstimatePrimaryRevChangedFromTo = "The estimate primary revision has been changed from {0} to {1}.";
        public const string EstimateLevelModified = "The estimate level of the {0} estimate revision has been modified.";
        public const string EstimateOperationLevelModified = "The operation level of the {0} estimate revision has been modified.";
        public const string EstimateDetailLevelModified = "The detail level of the {0} estimate revision has been modified.";
        public const string EstimateFieldRequiredForOrder = "The {0} field must be added to the order.";
        public const string EstimateArchived = "The {0} estimate revision has been archived.";
        public const string EstimateRevisionDeleted = "The {0} estimate revision has been deleted.";
        public const string EstimateAddedToSalesOrder = "The {0} estimate revision has been added to the {1} {2} sales order.";
        public const string EstimateCreatedFromSalesOrder = "The {0} estimate revision has been created for the {1} {2} sales order.";
        public const string EstimateCreatedFromOpportunity = "The {0} estimate revision has been created for the {1} opportunity.";
        public const string EstimateRemovedFromSalesOrder = "The estimate revision has been removed from the {0} {1} sales order.";
        public const string EstimateRemovedFromOpportunity = "The estimate revision has been created for the {0} opportunity.";
        public const string EstimateAddedToOpportunity = "The {0} estimate revision has been added to the {1} opportunity.";
        public const string MRPEngineStarting = "MRP Engine Starting";
        public const string MRPSubitemsEnabled = "The Inventory Subitems feature has been enabled.";
        public const string MRPEngineMaxLowLevel = "Low level codes have been set. The max level is {0}.";
        public const string MRPEngineReuseScheduleNumbersDisabled = "Reuse schedule numbers have been disabled.";
        public const string MRPEngineBomLookupByDateDisabled = "BOM lookup by date has been disabled.";
        public const string MRPEngineMaxLowLevelProcessSkipped = "No changes in low levels have been found. The max level is {0}.";
        public const string MRPEngineProcessFirstPass = "Processing First Pass";
        public const string MRPEngineFirstPassCompleted = "First Pass Completed";
        public const string MRPEngineCheckinExceptions = "Checking for Exceptions";
        public const string MRPEngineCheckinExceptionsComplete = "Checking for exception complete";
        public const string MRPEngineCreateMRPPlan = "Creating the MRP plan";
        public const string MRPEngineProcessingLevel = "Processing Items at Level {0}";
        public const string MRPEngineProcessingLevelAdjustComplete = "Level {0}: Adjustments Complete";
        public const string MRPEngineProcessingLevelBlowdownComplete = "Level {0}: Blowdown complete. {1} records have been processed.";
        public const string MRPEngineUnableToFindItemInMRPCache = "The {0} inventory ID cannot be found in MRP item cache (the {1} first pass record ID).";
        public const string MRPEngineUnableToScheduleFPRecordID = "The MRP plan order cannot be scheduled for the {0} first pass detail record ID.";
        public const string ProductionEventOrderCompleted = "The order has been completed by {0} batch {1}. {2}";
        public const string MRPErrorCachingItemWithWarehouse = "The {0} inventory ID cannot be cached at the {1} warehouse.";
        public const string MRPErrorCachingItemWithoutWarehouse = "The {0} inventory ID cannot be cached without the warehouse detail.";
        public const string MRPEngineUnableToSchedule = "The MRP plan order cannot be scheduled for the {0} item by using the {2} revision of the {1} BOM for the {3} date and quantity of {4}.";
        public const string EstimateReferenceStatus = "The estimate reference status is {0}.";
        public const string Starting = "Starting";
        public const string EstimateDetailsUpdatedFromEstimate = "The estimate details have been updated from the {1} revision of the {0} estimate.";
        public const string EstimateDetailsUpdatedFromProductionOrder = "The estimate details have been updated from the {0} production order (the order type is {1}).";
        public const string EstimateDetailsUpdatedFromBOM = "The estimate details have been updated from the {1} revision of the {0} BOM.";
        public const string EstimateCreatedBOM = "The {0} {1} BOM has been created from the {2} revision";
        public const string EstimateCreatedBOM2 = "The BOM has been created from the {0} revision.";
        public const string MFGLicense = "Manufacturing License";
        public const string CreatedOrder = "The {0} order has been created.";
        public const string UpdateSalesLineWarehouse = "Do you want to update the warehouse in the {2} line of the {0} {1} sales order?";
        public const string BucketInvalid = "The bucket is inactive.";
        public const string DaysHoursMinutesCompact = "### d 00:00";
        public const string ShortHoursMinutesCompact = "00:00";
        public const string ConfirmConfigKeyChange = "Confirm change of the configuration key.";
        public const string ConfirmConfigKeyChangeContinue = "Changing configuration key will replace existing configuration results with the data from the selected configuration key. Do you want to continue?";
        public const string ConfigurationNeedsAttention = "Configuration is not finished and requires attention: {0}";
        public const string ConfirmLicenseApplyToAllCompanies = "The {0} companies will be updated with the current licensing information.";
        public const string InCompany = " in the {0} company";
        public const string CheckingForMfgVersionUpdates = "Checking for Manufacturing Version {0} updates {1}";
        public const string NoUpdatesForMfgVersionUpdates = "No updates found for Manufacturing version {0}{1}";
        public const string UpgradeError = "Upgrade error: {0}";
        public const string EstimateSearchableTitleDocument = "Estimate {0} - {1}: {2}";
        public const string BOMSearchableTitleDocument = "BOM {0} - {1}";
        public const string ProductionSearchableTitleDocument = "Production Order {0} {1}";
        public const string ConfigContainsVariable = "{0} contains the {1} attribute variable in the following fields: {2}.";
        public const string OnFeature = "{0} on feature {1}";
        public const string RuleOnAttribute = "{0} rule on attribute {1}";
        public const string ScheduleMaterialShortage = "The {0} material in the {1} {2} order for the {3} operation and {4} schedule date cannot be supplied on any date.";
        public const string ScheduleMaterialShortUntilDate = "Material {0} on order {1} {2} operation {3} for schedule date {4} has no available supply until {5}.";
        public const string NotProjectAccounts = "The Inventory account and WIP account do not have an account group specified.";
        public const string BatchIsReleased = "{0} batch {1} has been released and cannot be modified.";
        public const string ErrorProcessingMrpDetailFpSupply = "An error occurred in MRP during processing the First Pass Detail record.";
        public const string ErrorAdjustingFirstPassSupplyQty = "An error occurred when adjusting first pass supply quantity.";
        public const string UnableToCalcGracePeriod = "The grace period date cannot be calculated from {0} and the {1} grace period.";
        public const string MRPErrorProcessingDetailFP = "MRP Error processing First Pass Detail";
        public const string IncorrectConfiguration = "The configuration is incorrect.";
        public const string InsertErrorRowSkipped = "An error occurred when inserting {0}. Insertion has been skipped.";
        public const string MachineIsInactive = "The {0} machine is inactive.";
        public const string CleanupScheduleHistory = "Cleanup Schedule History";
        public const string EnableDisableFeatures = "Enable/Disable Features";
        public const string FieldValueChanged = "{0} has been changed from {1} to {2}.";
        public const string NewRowInserted = "{0} {1} has been inserted.";
        public const string FileContentEmpty = "Contents of the {0} file are empty.";
        public const string MFGLicenseCopying = "The manufacturing license has been copied to the {0} company.";
        public const string CreatedProductionOrderForSalesOrder = "The {0} {1} production order has been created for the {2} {3} sales order.";
        public const string CreatedProductionOrderForProductionOrder = "The {0} {1} production order has been created for the {2} {3} production order.";
        public const string ErrorSchedulingOrderDates = "An error occurred when scheduling order dates.";
        public const string MaxLevelsReached = "The max level has been reached.";
        public const string ConfigurationStatusChangedTo = "The status of the {1} revision of the {0} configuration has been changed to {2}.";
        public const string INSetupNotFound = "Inventory preferences were not found.";
        public const string UnableToCreateProductionOrders = "Production orders cannot be created";
        public const string UnableToCreateTransfer = "A transfer cannot be created.";
        public const string UnableToSaveCahngesWithSO = "Changes for the sales order cannot be saved: {0}";
        public const string UnableToSaveCahngesWithOpp = "Changes for the opportunity cannot be saved: {0";
        public const string UnableToSaveCahngesWithOppQuote = "Changes for the opportunity quote cannot be saved: {0}";
        public const string UnableToFindBomRev = "The {0} revision of the {0} BOM cannot be found.";
        public const string UnableToGetAccountDefault = "The {0} AM account cannot be set as default.";
        public const string RuleTargetnotAnOption = "The rule target is not an option.";
        public const string NumberingSequenceMustUseAutoNumbering = "The numbering sequence for {0} must use autonumbering.";
        public const string UnableToGetNextNumber = "The next number of the {0} numbering ID cannot be generated.";
        public const string UnableToSetLL = "An error occurred when setting low levels.";
        public const string RecordIDEqual = "Record ID {0}";
        public const string ErrorProcessingMultLevelWhereUsedInqMatl = "The following error message occurred during processing multiple levels of BOM's in the Where Used inquiry using Material '{0}' from BOM '{1}' at Level '{2}': {3}";
        public const string ProductionOrderMaterialTransfer = "The material of the {0} {1} production order has been transferred.";
        public const string EstimateRevisionCreated = "The {0} estimate revision has been created.";
        public const string LoadingConfigurationForTesting = "The {0} - {1} configuration has been loaded for testing.";
        public const string ApplyRuleFormula = "A rule formula has been applied to the {0} - {1} configuration for the {2} result.";
        public const string OrderRescheduled = "The order with the {0} status has been rescheduled.";
        public const string UnableToInsertProdMatlFromBom = "The {0} production material item from the {3} line of the {2} operation of the {1} BOM cannot be inserted.";
        public const string UnableToInsertProdMatlFromCfg = "The {0} production material label from the {4} line of the {3} feature of the {1} {2} configuration cannot be inserted.";
        public const string Cleanup = "Cleanup";
        public const string DeletingUnreleasedINTransaction = "Unreleased Inventory {0} Transaction {1} related to {2} Transaction {3} has been deleted.";
        public const string MrpIsRunningWaitFinished = "MRP is running. {0}. Wait until the process is finished.";
        public const string ForOperation = "{0} for Operation {1}";
        public const string ClockIn = "Clock In";
        public const string ClockOut = "Clock Out";
        public const string CorrectingHistoryLineCountersForEstimate = "Correcting history line counters for estimate {0}";
        public const string CorrectingHistoryLineCountersForProduction = "Line counters in the production event for the {0} {1} production order have been corrected.";
        public const string ProcessedRecordCountMessage = "A total of {0} records have been processed.";
        public const string ErrorSavingTransaction = "The following error message occurred when saving the {0} transaction: {1}";
        public const string SavingResults = "Saving results";
        public const string SavingDacName = "Saving {0}";

        #endregion

        #region Graph Names

        public const string BOMMaint = "BOM Maintenance";
        public const string BOMSetup = "BOM Preferences";
        public const string MrpSetup = "MRP Preferences";
        public const string ProductionSetup = "Production Preferences";
        public const string MRPDetailInquiry = "MRP Detail Inquiry";
        public const string EstimateSetup = "Estimate Preferences";
        public const string ConfiguratorSetup = "Configurator Preferences";
        public const string AMOrderTypes = "AM Order Types";
        public const string AMECRSetupApproval = "ECR Setup Approval";
        public const string AMECOSetupApproval = "ECO Setup Approval";

        #endregion

        #region Cache Names

        public const string ProductionItem = "Production Item";
        public const string ProductionItemSplit = "Production Item Split";
        public const string ProductionOper = "Production Operation";
        public const string ProductionEvnt = "Production Event";
        public const string ProductionMatl = "Production Material";
        public const string ProductionMatlSplit = "Production Material Split";
        public const string ProductionOvhd = "Production Overhead";
        public const string ProductionStep = "Production Step";
        public const string ProductionTool = "Production Tool";
        public const string ProductionTotals = "Production Totals";
        public const string BOMItem = "BOM Item";
        public const string BOMOper = "BOM Operation";
        public const string BOMEvnt = "BOM Event";
        public const string BOMMatl = "BOM Material";
        public const string BOMOvhd = "BOM Overhead";
        public const string BOMStep = "BOM Step";
        public const string BOMTool = "BOM Tool";
        public const string BOMRef = "BOM Reference Designator";
        public const string MRPFirstPassDetail = "MRP First Pass Detail";
        public const string MRPExceptions = "MRP Exceptions";
        public const string MRPAudit = "MRP Audit";
        public const string AMOrder = "AM Order";
        public const string WorkCenter = "Work Center";
        public const string ScheduleItem = "Schedule Item";
        public const string ScheduleOper = "Schedule Operation";
        public const string Machine = "Machine";
        public const string Shift = "Shift";
        public const string AMTransactionLine = "AM Transaction";
        public const string AMTransactionSplit = "AM Transaction Split";
        public const string EstimateItem = "Estimate Item";
        public const string EstimateClass = "Estimate Class";
        public const string EstimateHistory = "Estimate History";
        public const string EstimateReference = "Estimate Reference";
        public const string EstimateMaterial = "Estimate Material";
        public const string EstimateOperations = "Estimate Operations";
        public const string EstimateOverhead = "Estimate Overhead";
        public const string EstimateTool = "Estimate Tool";
        public const string SalesEstimate = "Sales Estimate";
        public const string AMLicensing = "MFG Licensing";
        public const string AMLicensingFilter = "MFG Licensing Filter";
        public const string CopyEstimateFromFilter = "Copy Estimate from Filter";
        public const string WcOverheads = "Work Center Overheads";
        public const string WcMachines = "Work Center Machines";
        public const string BOMAttributes = "BOM Attributes";
        public const string ProductionAttributes = "Production Attributes";
        public const string AMMTranAttribute = "Transaction Attributes";
        public const string ToolMst = "Tools";
        public const string ToolSchdDetail = "Tool Schedule Detail";
        public const string MRPBuckets = "MRP Buckets";
        public const string MRPBucketDetail = "MRP Bucket Detail";
        public const string MRPBucketInq = "MRP Bucket Inquiry";
        public const string MRPBucketDetailInq = "MRP Bucket Detail Inquiry";
        public const string MRPInventory = "MRP Inventory";
        public const string OrderTypeAttributes = "Order Type Attributes";
        public const string AMDisassembleBatch = "AM Disassemble";
        public const string AMDisassembleTran = "AM Disassemble Transaction";
        public const string AMDisassembleBatchSplit = "AM Disassemble Batch Split";
        public const string AMDisassembleTranSplit = "AM Disassemble Transaction Split";
        public const string AMDisassembleTranAttribute = "AM Disassemble Transaction Attribute";
        public const string AMDisassembleLaborTran = "AM Disassemble Labor Transaction";
        public const string ConfigurationAttribute = "Configuration Attribute";
        public const string ConfigurationFeature = "Configuration Feature";
        public const string ConfigurationOption = "Configuration Option";
        public const string ConfigurationRule = "Configuration Rule";
        public const string ConfigurationAttributeRule = "Configuration Attribute Rule";
        public const string ConfigurationFeatureRule = "Configuration Feature Rule";
        public const string ConfigurationResult = "Configuration Result";
        public const string ConfigurationResultAttribute = "Configuration Attribute Result";
        public const string ConfigurationResultFeature = "Configuration Feature Result";
        public const string ConfigurationResultOption = "Configuration Option Result";
        public const string ConfigurationResultRule = "Configuration Rule Result";
        public const string MRPDetailPlan = "MRP Detail Plan";
        public const string EstimateItemFilter = "Estimate Item Filter";
        public const string ActiveBOMS = "Active BOMs";
        public const string ECRItem = "ECR Item";
        public const string ECOItem = "ECO Item";
        public const string WorkCenterSubstitute = "Work Center Substitute";
        public const string VendorShipment = "Vendor Shipment";
        public const string VendorShipmentLine = "Vendor Shipment Line";
        public const string VendorShipmentLineSplit = "Vendor Shipment Line Split";
        public const string VendorShipmentAddress = "Vendor Shipment Address";
        public const string VendorShipmentContact = "Vendor Shipment Contact";
        public const string EstimateStep = "Estimate Step";
        public const string AMItemPlan = "AM Item Plan";
        public const string OpportunityDocument = "Opportunity Document";
        public const string CreateECOFilter = "Create ECO Filter";
        public const string CreateInventoryFilter = "Create Inventory Filter";
        public const string NonInventoryFilter = "Non-Inventory Filter";
        public const string ProductionOrdersCreateFilter = "Create Production Orders Filter";
        public const string AMFixedDemand = "AM Fixed Demand";
        public const string OrderCrossRef = "Order Cross Reference  ";
        public const string MatlWizardFilter = "Material Wizard Filter";
        public const string PlanProductionMatl = "Plan Production Material";
        public const string UnreleasedMatlAllocationsFilter = "Unreleased Material Allocations Filter";
        public const string ForecastSettings = "Forecast Settings";
        public const string MRPDetailInventoryFilter = "MRP Detail Inventory Filter";
        public const string BucketFilter = "Bucket Filter";
        public const string AMScanSetup = "AM Scan Setup";
        public const string AMScanUserSetup = "AM Scan User Setup";
        public const string ClockLine = "Clock Transaction";
        public const string ClockLineSplit = "Clock Transaction Split";
        public const string ClockItem = "Clock Employee";

        #endregion

        #region View Names

        public const string BOMCost = "BOM Cost";
        public const string Operations = "Operations";
        public const string Tools = "Tools";
        public const string Overhead = "Overhead";
        public const string Steps = "Steps";

        #endregion

        #region Combo Values

        //AMDocType
        public const string DocTypeMove = "Move";
        public const string DocTypeLabor = "Labor";
        public const string DocTypeMaterial = "Material";
        public const string DocTypeWipAdjust = "WIP Adjustment";
        public const string DocTypeProdCost = "Cost";
        public const string Disassembly = "Disassembly";
        public const string DocTypeClock = "Clock Time";

        //AMLaborType
        public const string Direct = "Direct";
        public const string Indirect = "Indirect";

        //AMMaterialType
        public const string Regular = "Regular";
        public const string Phantom = "Phantom";
        public const string Supplemental = "Supplemental";
        public const string Subcontract = "Subcontract";

        //AMAttributeLevels
        public const string Operation = "Operation";
        public const string Order = "Order";

        //AMAttributeSource
        public const string Production = "Production";
        public const string ProductionRef = "Production Reference";

        //AMTranType
        public const string TranTypeWIPadjustment = "WIP Adjustment";
        public const string TranTypeWIPvariance = "WIP Variance";
        public const string TranTypeVarOvhd = "Variable Overhead";
        public const string TranTypeFixOvhd = "Fixed Overhead";
        public const string TranTypeTool = "Tool";
        public const string TranTypeBFLabor = "Backflush Labor";
        public const string TranTypeLabor = "Labor";
        public const string TranTypeIndirectLabor = "Indirect Labor";
        public const string TranTypeMachine = "Machine";
        public const string TranTypeReturn = "Return";
        public const string TranTypeIssue = PX.Objects.IN.Messages.Issue;
        public const string TranTypeReceipt = PX.Objects.IN.Messages.Receipt;
        public const string ProdEntry_Receipt = "Production Receipt";
        public const string ProdEntry_Issue = "Production Issue";
        public const string ProdEntry_Return = "Production Return";
        public const string ProdGLEntry_WIPAdjustment = "Production WIP Adjustment";
        public const string ProdGLEntry_LaborEntry = "Production Labor";
        public const string ProdGLEntry_IndirectLaborEntry = "Indirect Labor";
        public const string ProdGLEntry_LaborBackflush = "Production Backflush Labor";
        public const string ProdGLEntry_MachineCosts = "Production Machine Cost";
        public const string ProdGLEntry_ToolCosts = "Production Tool Cost";
        public const string ProdEntry_ReturnMatl = "Production Material Return";
        public const string ProdEntry_ReturnFg = "Production Finished Good Return";
        public const string ProdGLEntry_FixOverheadCosts = "Production Fixed Overhead Cost";
        public const string ProdGLEntry_VarOverheadCosts = "Production Variable Overhead Cost";
        public const string ProdGLEntry_ProdTranGeneric = "Production Transaction";
        public const string ProdGLEntry_ProdTranNonStockMatl = "Production Non-Stock Material Transaction";
        public const string ProdGLEntry_ProdMatlTran = "Production Material Transaction";
        public const string ProductionTransaction = "Production {0} Transaction";
        public const string ProdGLEntry_WIPVariance = "Production WIP Variance";
        public const string ProdGLEntry_NonStockItem = "Production Non-Stock Item";
        public const string ProdEntry_Adjustment = "Production Adjustment";
        public const string ScrapWriteOff = "Scrap Write-Off";
        public const string ScrapQuarantine = "Scrap Quarantine";
        public const string ProdEntry_ScrapWriteOff = "Production Scrap Write-Off";
        public const string ProdEntry_ScrapQuarantine = "Production Scrap Quarantine";
        public const string ScrapTransaction = "Production Scrap";
        public const string OperationWipComplete = "Operation MFG to Inventory";
        public const string ProdEntry_OperationWipComplete = "Production Operation MFG to Inventory";
        public const string ProdEntry_Disassembly = "Production Disassembly";
        public const string Correction = "Correction";

        //AMShipType
        public const string Shipment = "Shipment";
        public const string Return = "Return";
        public const string WIP = "WIP";

        //DocStatus
        public const string Balanced = PX.Objects.IN.Messages.Balanced;
        public const string Hold = PX.Objects.IN.Messages.Hold;
        public const string Released = PX.Objects.IN.Messages.Released;

        //ForecastInterval
        public const string OneTime = "One Time";
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Yearly = "Yearly";

        //LaborRateType
        public const string Employee = "Employee";
        public const string Standard = "Standard";

        //MaterialPOReceiptStatus
        public const string UnRelPo_Released = Released;
        public const string UnRelPo_UnReleased = "Unreleased";
        public const string UnRelPo_Deleted = "Deleted";

        //OrderCrossRefProcessSource
        public const string MRP = "MRP";
        public const string CriticalMaterial = "Critical Material";
        public const string ProductionMaint = "Production Maintenance";
        public const string SalesOrder = "Sales Order";

        //OverheadType
        public const string FixedType = "Fixed";
        public const string VarLaborHrs = "Variable by Labor Hours";
        public const string VarLaborCost = "Variable by Labor Cost";
        public const string VarMatlCost = "Variable by Material Cost";
        public const string VarMachHrs = "Variable by Machine Hours";
        public const string VarQtyComp = "Variable by Quantity Completed";
        public const string VarQtyTot = "Variable by Total Quantity";

        //PhantomRoutingOptions
        public const string Before = "Before";
        public const string After = "After";
        public const string Exclude = "Exclude";

        //ProductionEventType
        public const string Info = "Information";
        public const string Comment = "Comment";
        public const string Created = "Created";
        // (repeat) Released
        public const string OnHold = "On Hold";
        public const string HoldRemoved = "Hold Removed";
        public const string Closed = "Closed";
        public const string Canceled = "Canceled";
        public const string ResetToPlan = "Reset to Plan";
        public const string OrderEdit = "Order Edit";
        public const string OperationChange = "Operation Change";
        public const string ReportPrinted = "Report Printed";

        //ProductionOrderStatus
        public const string Planned = "Planned";
        // (repeat) Released
        public const string InProcess = "In Process";
        // (repeat) Hold
        // (repeat) Canceled
        public const string Completed = "Completed";
        // (repeat) Closed
        public const string Deleted = "Deleted";

        //ScheduleMethod
        public const string FinishOn = "Finish On";
        public const string StartOn = "Start On";
        public const string UserDates = "User Dates";

        //SetupMessages
        public const string Setting_Allow = "Allow";
        public const string Setting_Warn = "Warn";
        public const string Setting_NotAllow = "Do Not Allow";

        //ShiftDiffType
        public const string AmountDiff = "Amount";
        public const string RateDiff = "Rate";

        //TimeUnits
        public const string Days = "Days";
        public const string Hours = "Hours";

        //XRefType
        public const string PurchasedXRef = PX.Objects.IN.Messages.Purchased;
        public const string ManufactureXRef = PX.Objects.IN.Messages.Manufactured;
        public const string TransferXRef = PX.Objects.IN.Messages.Transfer;

        //MRPExceptionType
        public const string MRPExceptionTypeDefer = "Defer";
        //********************************************************************************************************************
        //**  "Delete" Causing issues for standard Acumatica drop down on Access Rights by Role page. 
        //**  Was asked to remove due to localization limitation.
        //**  Ref Acumatica case # 040332 (Task # 595 )
        //**    public const string MRPExceptionTypeDelete = "Delete"; 
        //********************************************************************************************************************
        public const string MRPExceptionTypeExpedite = "Expedite";
        public const string MRPExceptionTypeLate = "Late Order";
        public const string MRPExceptionTypeTransfer = "Transfer Available";
        public const string MRPExceptionTypeOrderOnHold = "Order on Hold";

        //MRPPlanningType
        public const string Unknown = "Unknown";
        // (repeat) SalesOrder
        public const string PurchaseOrder = PX.Objects.PO.Messages.PurchaseOrder;
        public const string Forecast = "Forecast";
        public const string ProductionOrder = "Production Order";
        public const string OpportunityRef = "Opportunity ";
        public const string SalesOrderRef = "Sales Order ";
        public const string ProductionMaterial = "Production Material";
        public const string StockAdjustment = "Stock Adjustment";
        public const string SafetyStock = "Safety Stock";
        public const string MPS = "MPS";
        public const string MRPPlan = "MRP Plan";
        public const string MrpRequirement = "MRP Requirement";
        public const string TransferDemand = "Transfer Demand";
        public const string TransferSupply = "Transfer Supply";
        public const string AssemblyDemand = "Assembly Demand";
        public const string AssemblySupply = "Assembly Supply";
        public const string InventoryDemand = "Inventory Demand";
        public const string InventorySupply = "Inventory Supply";
        public const string FieldService = "Field Service";
        public const string VendorShipments = "Vendor Shipments";

        //MRPSDFlag
        public const string Supply = "Supply";
        public const string Demand = "Demand";

        //OnHoldStatus
        public const string OnHoldStatusNotOnHold = "Not on Hold";
        public const string OnHoldStatusOnHoldInclude = "On Hold Exclude";
        public const string OnHoldStatusOnHoldExclude = "On Hold Include";
        public const string OnHoldStatusInvalidItemStatus = "Invalid Item Status";

        //GenerateForecastType
        // (repeat) Regular
        public const string Seasonality = "Seasonality";

        //MRPStockingMethod
        public const string StockingMethodSafetyStock = "Safety Stock";
        public const string StockingMethodReorderPoint = "Reorder Point";

        //AMRPAuditTable.MsgType
        public const string Start = "Started";
        public const string End = "Ended";

        //CostMethod
        public const string Estimated = "Estimated";

        //EstimateRevisionStatus
        public const string Active = "Active";
        public const string Archived = "Archived";

        //NonInventoryLevel.NonInventoryLevel
        public const string Estimate = "Estimate";
        public const string Material = "Material";

        // Transaction Prod Types
        public const string Labor = "Labor";
        public const string Tool = "Tool";

        //DateRangeInq
        public const string Daily = "Daily";
        public const string BiWeekly = "Biweekly";

        //DayOfWeekAttribute
        public const string All = "All";

        //ProductionDetailSource
        public const string NoSource = "No Source";
        public const string BOM = "BOM";
        public const string Configuration = "Configuration";

        //EstimateStatus
        public const string NewStatus = "New";

        //SOCopyParamFilterAMExtension.EstimateAction
        public const string NoAction = "No Action";
        public const string Copy = "Copy";
        public const string Convert = "Convert";

        //CalcOptions
        public const string OnCompletion = "On Completion";
        public const string AfterSelection = "After Selection";

        //ConfigKeyFormats
        public const string NoKey = "No Keys";
        public const string Formula = "Formula";
        public const string NumberSequence = "Numbering Sequence";
        public const string SubItem = "Sub item";

        //ConfigRevisionStatus
        public const string Inactive = "Inactive";
        public const string Pending = "Pending";

        //RuleTypes
        public const string Include = "Include";
        public const string Require = "Require";
        public const string Validate = "Validate";

        //Rule Sources/Target
        public const string Attribute = "Attribute";
        public const string Feature = "Feature";
        public const string Option = "Option";

        //SelectorDefault value
        public const string SelectorAny = "<ANY>";
        public const string SelectorAll = "<ALL>";
        public const string SelectorFormula = "<FORMULA>";

        //Rule Formula Conditions
        public const string Equal = "Equals";
        public const string NotEqual = "Does Not Equal";
        public const string Greater = "Is Greater Than";
        public const string GreaterEqual = "Is Greater Than or Equal To";
        public const string Less = "Is Less Than";
        public const string LessEqual = "Is Less Than or Equal To";
        public const string Between = "Is Between";
        public const string Contains = "Contains";
        public const string NotContains = "Does Not Contain";
        public const string StartWith = "Starts With";
        public const string EndsWith = "Ends With";
        public const string Null = "Is Null";
        public const string NotNull = "Is Not Null";
        public const string Custom = "Custom Condition";
        public const string Even = "Is Even";
        public const string Odd = "Is Odd";

        //RollupOptions
        public const string Parent = "Parent";
        public const string ChildrenAll = "Children All";
        public const string ChildrenCFG = "Children CFG";
        public const string ParentChildren = "Parent/Children";

        //AMLicenseStatus
        public const string Trial = "Trial";
        public const string Unlicensed = "Unlicensed";
        public const string Licensed = "Licensed";
        public const string Bypassed = "Bypassed";

        //OrderTypeFunction
        // (repeat) Regular
        public const string Planning = "Planning";
        public const string Disassemble = "Disassemble";

        //OrderSourceTypes
        public const string None = "None";

        //AMPlanConstants
        public const string ProductionDemand = "Production Demand";
        public const string ProductionAllocated = "Production Allocated";
        public const string ProductionSupply = "Production Supply";

        // Scrap Source for Production Scrap Source
        public const string Item = "Item";
        public const string Warehouse = "Warehouse";
        public const string OrderType = "Order Type";

        // Scrap Action for Production Scrap Source
        public const string WriteOff = "Write-Off";
        public const string Quarantine = "Quarantine";

        // POLineType
        public const string GoodsForManufacturing = "Goods for MFG";
        public const string NonStockForManufacturing = "Non-Stock for MFG";

        // Production Supply Types
        public const string Inventory = "Inventory";
        
        //AMMTran TimeCard Status
        public const string Unprocessed = "Unprocessed";
        public const string Processed = "Processed";
        public const string Skipped = "Skipped";

        //MaterialDefaultMarkFor
        public const string NoDefault = "No Default";
        
        //ECR Row Status
        public const string Unchanged = "Unchanged";
        public const string Updated = "Updated";
        public const string Inserted = "Inserted";

        //BOM Compare
        public const string BOMCompare = "BOM Compare";

        //BOMCompareInq.IDTypes
        public const string ECR = "ECR";
        public const string ECO = "ECO";

        // AMSubcontractSource
        // (repeat) None
        // (repeat) Purchase;
        public const string DropShip = "Drop Ship";
        public const string VendorSupplied = "Vendor Supplied";
        public const string ShipToVendor = "Ship to Vendor";

        //  VendorShipmentEntryActionsAttribute
        public const string ConfirmShipment = "Confirm Shipment";

        #endregion

        #region Custom Actions

        public const string ProductionOrders = "Production Orders";
        public const string BOMSummary = "BOM Summary";
        public const string MultiLevel = "Multilevel";
        public const string ProductionDetail = "Production Detail";
        public const string Wizard = "Wizard";
        public const string CopyBom = "Copy BOM";
        public const string MakeDefaultBom = "Make Default BOM";
        public const string MakePlanningBom = "Make Planning BOM";
        public const string BOMCostSummary = "BOM Cost Summary";
        public const string RevisionFilter = "Revision Filter";
        public const string PlanOrder = "Plan Order";
        public const string ReleaseOrdAction = "Release Order";
        public const string CancelOrdAction = "Cancel Order";
        public const string CloseOrder = "Close Order";
        public const string CalculatePlanCost = "Calculate Plan Cost";
        public const string CompleteOrder = "Complete Order";
        public const string RunReport = "Run Report";
        public const string RollCosts = "Roll Costs";
        public const string UpdatePending = "Update Pending";
        public const string Calculate = "Calculate";
        public const string Actions = "Actions";
        public const string Inquiries = "Inquiries";
        public const string Reports = "Reports";
        public const string ViewOrigBatch = "View Original Batch";
        public const string Upgrade = "Upgrade";
        public const string CopyFrom = "Copy From";
        public const string Add2Order = "Add to Order";
        public const string CreateInventory = "Create Inventory";
        public const string AddHistory = "Add Comment";
        public const string NewRevision = "New Revision";
        public const string ViewReference = "View Reference";
        public const string CreateBOM = "Create BOM";
        public const string CreateProdOrder = "Create Production Order";
        public const string UpdatePendingStdCost = "Update Pending Standard Cost";
        public const string WcCalendarInquiry = "Calendar"; //might change the label in the future...
        public const string Schedule = "Schedule";
        public const string Capacity = "Capacity";
        public const string Add = "Add";
        public const string QuickEstimate = "Quick Estimate";
        public const string RemoveEstimate = "Remove";
        public const string DeleteEstimate = "Delete Estimate";
        public const string Purchase = "Purchase";
        public const string Manufacture = "Manufacture";
        public const string Dispatch = "Dispatch";
        public const string Finish = "Finish";
        public const string Unfinish = "Unfinish";
        public const string ShowAll = "Show All";
        public const string SelectOptions = "Select Options";
        public const string Configure = "Configure";
        public const string DeleteConfig = "Delete Configuration";
        public const string CloseTesting = "Close Testing";
        public const string SaveAndClose = "Save & Close";
        public const string Summary = "Summary";
        public const string Quote = "Quote";
        public const string SavedAndActiveConfigNecessary = "Configuration needs to be saved and contain an active revision.";
        public const string ReleaseMaterial = "Release Material";
        public const string Attributes = "Attributes";
        public const string AutoCreateLinkedOrders = "Auto Create Linked Orders";
        public const string ApplyToAllCompanies = "Apply to All Companies";
        public const string MassUpdate = "Mass Update";
        public const string CopyLine = "Copy Line";
        public const string CreateMove = "Create Move";
        public const string ArchiveBom = "Archive BOM";
        public const string Engineering = "Engineering";
        public const string Costed = "Costed";
        public const string CreatePurchaseOrder = "Create Purchase Order";
        public const string CreatePurchaseOrdersInq = "Create Purchase Orders";
        public const string CreateProductionOrdersInq = "Create Production Orders";

        #endregion

        #region Reports

        public const string MultiLevelBOM = "Multilevel BOM Engineering";
        public const string MultiLevelBOMCosted = "Multilevel BOM Costed";

        #endregion

        #region Modules
        public const string ModuleBillOfMaterial = "Bill of Material";
        public const string ModuleProduction = "Production Management";
        public const string ModuleMRP = "Material Requirements Planning";
        public const string ModuleAPS = "Advanced Planning and Scheduling";
        public const string ModuleEstimating = "Estimating";
        public const string ModuleConfigurator = "Product Configurator";
        public const string ModuleECC = "Engineering Change Control";
        public const string ModuleDataCollection = "MFG Data Collection";
        public const string TimeReportingModule = "Time Reporting Module";

        #endregion

        /// <summary>
        /// Localize the message
        /// </summary>
        /// <param name="msg">Message to localize</param>
        /// <returns>Localized message</returns>
        public static string GetLocal(string msg)
        {
            return GetLocal(msg, typeof(Messages));
        }

        /// <summary>
        /// Localize the message with provided format arguments
        /// </summary>
        /// <param name="msg">Message to localize</param>
        /// <param name="formatArgs">Arguments for string.Format</param>
        /// <returns>Localized message</returns>
        public static string GetLocal(string msg, params object[] formatArgs)
        {
            return string.Format(GetLocal(msg, typeof(Messages)), formatArgs);
        }

        /// <summary>
        /// Localize the message
        /// </summary>
        public static string GetLocal(string msg, Type msgType)
        {
            return PXLocalizer.Localize(msg, msgType.ToString());
        }
    }
}
