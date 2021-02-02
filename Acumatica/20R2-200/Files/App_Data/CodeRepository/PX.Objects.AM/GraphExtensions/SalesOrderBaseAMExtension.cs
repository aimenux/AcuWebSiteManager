using System;
using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    using AM;
    using AM.Attributes;

    /// <summary>
    /// Reusable business object for MFG and Sales Orders
    /// </summary>
    public abstract class SalesOrderBaseAMExtension<TGraph, TPrimary, TConfigWhere> :
            PXGraphExtension<TGraph>
            where TGraph : PXGraph
            where TPrimary : class, IBqlTable, new()
            where TConfigWhere : IBqlWhere, new()
    {
        /// <summary>
        /// Linked to quick estimate
        /// </summary>
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<
        	AMEstimateItem, 
        	Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateReference.estimateID>>,
        		And<AMEstimateItem.revisionID, Equal<Current<AMEstimateReference.revisionID>>>>> SelectedEstimateRecord;

        /// <summary>
        /// Used for Add estimate panel
        /// </summary>
        public PXFilter<AMOrderEstimateItemFilter> OrderEstimateItemFilter;

        [PXHidden]
        [PXCopyPasteHiddenView]
        public ConfigurationSelect<TConfigWhere> ItemConfiguration;
        public PXSetupOptional<AMEstimateSetup> EstimateSetup;
        public PXSetupOptional<AMPSetup> ProductionSetup;
        public PXSetupOptional<AMConfiguratorSetup> ConfiguratorSetup;

        public override void Initialize()
        {
            base.Initialize();

            Base.RowDeleted.AddHandler<TPrimary>(PrimaryRowDeleted);
        }
        
        protected abstract bool ContainsEstimates { get; }
        public abstract bool OrderAllowsEdit { get; }

        /// <summary>
        /// Indicates if estimates allowed allows for orders (Internal usage)
        /// </summary>
        internal bool AllowEstimatesInt => PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() &&
                                           Features.EstimatingEnabled() &&
                                           !string.IsNullOrWhiteSpace(EstimateSetup.Current?.EstimateNumberingID);

        /// <summary>
        /// Indicates if configurations are allowed for orders (Internal usage)
        /// </summary>
        internal bool AllowConfigurationsInt => PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() &&
                                                Features.ProductConfiguratorEnabled() &&
                                                !string.IsNullOrWhiteSpace(ConfiguratorSetup?.Current?.ConfigNumberingID);

        protected virtual void OrderLineRowSelected<TRow>(PXCache sender, TRow row, IMfgConfigOrderLineExtension rowExtension, AMConfigurationResults configResult, bool cfgAllowed) 
            where TRow : class, IBqlTable, new()
        {
            if (rowExtension == null)
            {
                return;
            }

            PXUIFieldAttribute.SetVisible(sender, row, "aMConfigKeyID", cfgAllowed);
            if (!cfgAllowed)
            {
                return;
            }

            var configurationCompleted = configResult?.Completed == true;

            if (rowExtension.IsConfigurable.GetValueOrDefault())
            {
                PXUIFieldAttribute.SetEnabled(sender, row, "aMConfigKeyID", !configurationCompleted);

                // Discount
                PXUIFieldAttribute.SetEnabled(sender, row, "discPct", ConfiguratorSetup.Current?.EnableDiscount == true || !configurationCompleted);
                PXUIFieldAttribute.SetEnabled(sender, row, "curyDiscAmt", ConfiguratorSetup.Current?.EnableDiscount == true || !configurationCompleted);
                PXUIFieldAttribute.SetEnabled(sender, row, "discountID", ConfiguratorSetup.Current?.EnableDiscount == true || !configurationCompleted);
                PXUIFieldAttribute.SetEnabled(sender, row, "manualDisc", ConfiguratorSetup.Current?.EnableDiscount == true || !configurationCompleted);

                // Price
                PXUIFieldAttribute.SetEnabled(sender, row, "curyUnitPrice", ConfiguratorSetup.Current?.EnablePrice == true || !configurationCompleted);
                PXUIFieldAttribute.SetEnabled(sender, row, "manualPrice", ConfiguratorSetup.Current?.EnablePrice == true || !configurationCompleted);
                PXUIFieldAttribute.SetEnabled(sender, row, "curyLineAmt", ConfiguratorSetup.Current?.EnablePrice == true || !configurationCompleted);

                // Warehouse
                PXUIFieldAttribute.SetEnabled(sender, row, "siteID", ConfiguratorSetup.Current?.EnableWarehouse == true && Common.Cache.GetEnabled(sender, row, "siteID").GetValueOrDefault());


                // Sub-item
                PXUIFieldAttribute.SetEnabled(sender, row, "subItemID", ConfiguratorSetup.Current?.EnableSubItem == true || !configurationCompleted);

                // Disable fields which could change configuration
                PXUIFieldAttribute.SetEnabled(sender, row, "inventoryID", !configurationCompleted);
                return;
            }

            PXUIFieldAttribute.SetEnabled(sender, row, "aMConfigKeyID", false);

            if (rowExtension.AMIsSupplemental.GetValueOrDefault())
            {
                PXUIFieldAttribute.SetEnabled(sender, row, "inventoryID", false);

                // Discount
                PXUIFieldAttribute.SetEnabled(sender, row, "discPct", ConfiguratorSetup.Current?.EnableDiscount == true);
                PXUIFieldAttribute.SetEnabled(sender, row, "curyDiscAmt", ConfiguratorSetup.Current?.EnableDiscount == true);
                PXUIFieldAttribute.SetEnabled(sender, row, "discountID", ConfiguratorSetup.Current?.EnableDiscount == true);
                PXUIFieldAttribute.SetEnabled(sender, row, "manualDisc", ConfiguratorSetup.Current?.EnableDiscount == true);


                // Price
                PXUIFieldAttribute.SetEnabled(sender, row, "curyUnitPrice", ConfiguratorSetup.Current?.EnablePrice == true);

                PXUIFieldAttribute.SetEnabled(sender, row, "curyLineAmt", ConfiguratorSetup.Current?.EnablePrice == true);
            }
        }

        /// <summary>
        /// Update the reference dac values with the estimate item values and return the updated reference (if update able)
        /// </summary>
        /// <param name="estimateReference">estimate reference row to sync</param>
        /// <param name="estimateItem">Estimate item related to the revision on the reference</param>
        /// <returns>Updated reference row if able to update/sync from estimate item values</returns>
        protected virtual AMEstimateReference SyncEstimateReference(AMEstimateReference estimateReference, AMEstimateItem estimateItem)
        {
            if (estimateReference == null
                || estimateItem == null
                || !EstimateStatus.IsEditable(estimateItem.EstimateStatus)
                || !estimateItem.EstimateID.EqualsWithTrim(estimateReference.EstimateID)
                || !estimateItem.RevisionID.EqualsWithTrim(estimateReference.RevisionID))
            {
                return estimateReference;
            }

            estimateReference.OrderQty = estimateItem.OrderQty.GetValueOrDefault();
            estimateReference.CuryUnitPrice = estimateItem.CuryUnitPrice.GetValueOrDefault();

            return estimateReference;
        }

        internal int? WarehouseAsID(object siteCD)
        {
            if (siteCD is string)
            {
                INSite inSite = PXSelect<INSite, Where<INSite.siteCD, Equal<Required<INSite.siteCD>>>>.SelectWindowed(Base, 0, 1, siteCD);
                return inSite?.SiteID;
            }

            return siteCD as int?;
        }

        internal virtual void AMEstimateItemRowSelectedInt(PXCache cache, PXRowSelectedEventArgs e, bool allowEstimateEdit)
        {
            var amEstimateItem = (AMEstimateItem)e.Row;
            if (amEstimateItem == null)
            {
                return;
            }

            SelectedEstimateRecord.AllowUpdate = AllowEstimatesInt && EstimateStatus.IsEditable(amEstimateItem.EstimateStatus) && amEstimateItem.IsPrimary.GetValueOrDefault() && allowEstimateEdit;

            PXUIFieldAttribute.SetEnabled<AMEstimateItem.estimateID>(cache, amEstimateItem, false);
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.revisionID>(cache, amEstimateItem, false);
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.inventoryCD>(cache, amEstimateItem, false);
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.subItemID>(cache, amEstimateItem, false);

            if (!SelectedEstimateRecord.AllowUpdate)
            {
                PXUIFieldAttribute.SetEnabled(cache, amEstimateItem, false);
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMEstimateItem.fixedLaborCost>(cache, amEstimateItem, amEstimateItem.FixedLaborOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.variableLaborCost>(cache, amEstimateItem, amEstimateItem.VariableLaborOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.machineCost>(cache, amEstimateItem, amEstimateItem.MachineOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.materialCost>(cache, amEstimateItem, amEstimateItem.MaterialOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.toolCost>(cache, amEstimateItem, amEstimateItem.ToolOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.fixedOverheadCost>(cache, amEstimateItem, amEstimateItem.FixedOverheadOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.variableOverheadCost>(cache, amEstimateItem, amEstimateItem.VariableOverheadOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.curyUnitPrice>(cache, amEstimateItem, amEstimateItem.PriceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.subcontractCost>(cache, amEstimateItem, amEstimateItem.SubcontractOverride.GetValueOrDefault());
        }

        public PXAction<TPrimary> AddEstimate;
#pragma warning disable PX1092 // Action handlers must be decorated with the PXUIField attribute and the PXButton attribute or its successors
        protected abstract IEnumerable addEstimate(PXAdapter adapter);
#pragma warning restore PX1092

        public PXAction<TPrimary> quickEstimate;
        [PXUIField(DisplayName = Messages.QuickEstimate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable QuickEstimate(PXAdapter adapter)
        {
            if (string.IsNullOrWhiteSpace(SelectedEstimateRecord.Current?.EstimateID))
            {
                SelectedEstimateRecord.Current = SelectedEstimateRecord.Select();
            }

            if (string.IsNullOrWhiteSpace(SelectedEstimateRecord.Current?.EstimateID))
            {
                SelectedEstimateRecord.Cache.Clear();
                SelectedEstimateRecord.Cache.ClearQueryCache();
                SelectedEstimateRecord.Current = null;
                return adapter.Get();
            }

            if (SelectedEstimateRecord.AskExt() == WebDialogResult.OK)
            {
                var estimateGraph = EstimateMaint.Construct(SelectedEstimateRecord.Current.EstimateID, SelectedEstimateRecord.Current.RevisionID.TrimIfNotNullEmpty());
                var filterUpdated = PXCache<AMEstimateItem>.CreateCopy(SelectedEstimateRecord.Current);
                if (filterUpdated != null)
                {
                    estimateGraph.Documents.Update(filterUpdated);
                    //clear change from base graph to avoid double update
                    SelectedEstimateRecord.Cache.Clear();
                }

                if (estimateGraph.IsDirty)
                {
                    var estGraphHelper = new EstimateGraphHelper(estimateGraph);
                    if (!estGraphHelper.PersistEstimateReference(Base, EstimateReferenceOrderAction.Update))
                    {
                        // When not updated with base graph we still need to persist the estimate graph as there could be non cost/qty changes
                        estimateGraph.PersistBase();
                    }
                    //press cancel only for "refresh"
                    Base.Actions.PressCancel();
                }
            }

            SelectedEstimateRecord.Cache.Clear();
            SelectedEstimateRecord.Cache.ClearQueryCache();
            SelectedEstimateRecord.Current = null;

            return adapter.Get();
        }

        public PXAction<TPrimary> removeEstimate;
        [PXUIField(DisplayName = Messages.RemoveEstimate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable RemoveEstimate(PXAdapter adapter)
        {
            if (string.IsNullOrWhiteSpace(SelectedEstimateRecord.Current?.EstimateID))
            {
                SelectedEstimateRecord.Current = SelectedEstimateRecord.Select();
            }

            if (string.IsNullOrWhiteSpace(SelectedEstimateRecord.Current?.EstimateID))
            {
                SelectedEstimateRecord.Cache.Clear();
                SelectedEstimateRecord.Cache.ClearQueryCache();
                SelectedEstimateRecord.Current = null;
                return adapter.Get();
            }

            var userMessage = Messages.GetLocal(Messages.RemoveEstimateConfirmation,
                SelectedEstimateRecord.Current.EstimateID, SelectedEstimateRecord.Current.RevisionID);

            WebDialogResult response = SelectedEstimateRecord.Ask(
                        Messages.RemoveEstimate,
                        $"{userMessage} {Messages.GetLocal(Messages.ContinueWithEstimateRemoval)}?",
                        MessageButtons.YesNo);
            if (response == WebDialogResult.Yes)
            {
                var estimateGraph = EstimateMaint.Construct(SelectedEstimateRecord.Current.EstimateID, SelectedEstimateRecord.Current.RevisionID);
                if (estimateGraph?.Documents?.Current == null)
                {
                    SelectedEstimateRecord.Cache.Clear();
                    SelectedEstimateRecord.Cache.ClearQueryCache();
                    SelectedEstimateRecord.Current = null;
                    return adapter.Get();
                }

                estimateGraph.EstimateReferenceRecord.Current = estimateGraph.EstimateReferenceRecord.Current ?? estimateGraph.EstimateReferenceRecord.Select();
                var estimateItem = estimateGraph.Documents.Current;
                if (!EstimateGraphHelper.IsEstimateReferencedToQuote(estimateGraph, estimateItem, true))
                {
                    estimateItem.QuoteSource = EstimateSource.Estimate;
                    estimateItem.IsLockedByQuote = false;
                    estimateGraph.Documents.Current = estimateGraph.Documents.Update(estimateItem);
                }

                RemoveEstimateFromQuote(estimateGraph);
            }
            SelectedEstimateRecord.Cache.Clear();
            SelectedEstimateRecord.Cache.ClearQueryCache();
            SelectedEstimateRecord.Current = null;
            return adapter.Get();
        }

        public PXAction<TPrimary> ConfigureEntry;
#pragma warning disable PX1092 // Action handlers must be decorated with the PXUIField attribute and the PXButton attribute or its successors
        protected abstract void configureEntry();
#pragma warning restore PX1092

        protected abstract AMEstimateHistory MakeQuoteEstimateHistory(AMEstimateItem amEstimateItem, bool estimateCreated);
        protected abstract void RemoveEstimateFromQuote(EstimateMaint estimateGraph);

        protected abstract AMEstimateReference SetEstimateReference(AMEstimateReference estimateReference);

        protected virtual AMEstimateReference SetEstimateReferenceInt(AMEstimateReference estimateReference, AMEstimateItem estimateItem, string revisionId)
        {
            if (estimateReference == null)
            {
                throw new ArgumentNullException(nameof(estimateReference));
            }

            if (estimateItem == null)
            {
                throw new ArgumentNullException(nameof(estimateItem));
            }

            if (estimateItem.QuoteSource == EstimateSource.Estimate)
            {
                //reset the tax linenbr for a process later which will get the next line number for the order the estimate is being added to
                estimateReference.TaxLineNbr = null;
            }

            estimateReference.RevisionID = revisionId;
            estimateReference.OrderQty = estimateItem.OrderQty;
            estimateReference.CuryUnitPrice = estimateItem.CuryUnitPrice;

            return estimateReference;
        }

        /// <summary>
        /// Add (link) estimate to a quote order. This could be a new or existing estimate.
        /// </summary>
        /// <param name="estimateItemFilter"></param>
        /// <param name="primaryRow">Primary order row</param>
        /// <returns>Estimate graph with cached changes</returns>
        protected virtual EstimateMaint AddEstimateToQuote(AMOrderEstimateItemFilter estimateItemFilter, TPrimary primaryRow)
        {
            if (estimateItemFilter == null)
            {
                throw new ArgumentNullException(nameof(estimateItemFilter));
            }

            if (estimateItemFilter.AddExisting.GetValueOrDefault())
            {
                var existingEstimateGraph = EstimateMaint.Construct(estimateItemFilter.CurrentEstimate, estimateItemFilter.RevisionID);

                if (existingEstimateGraph?.Documents?.Current == null)
                {
                    throw new PXException($"Unable to find estimate {estimateItemFilter.CurrentEstimate} revision {estimateItemFilter.RevisionID}");
                }

                var estimateItem = existingEstimateGraph.Documents.Current;
                var estimateRef = existingEstimateGraph.EstimateReferenceRecord.Current ??
                                  existingEstimateGraph.EstimateReferenceRecord.Select();

                var wasEstimateSource = estimateItem.QuoteSource == EstimateSource.Estimate;
                estimateItem.QuoteSource = EstimateSource.GetEstimateQuoteSource(Base);
                estimateItem.IsLockedByQuote = Base is PX.Objects.CR.QuoteMaint;

                if (!estimateItem.IsLockedByQuote.GetValueOrDefault() || wasEstimateSource)
                {
                    estimateItem.PrimaryRevisionID = estimateItem.RevisionID;
                }

                estimateItem = existingEstimateGraph.Documents.Update(estimateItem);
                existingEstimateGraph.EstimateReferenceRecord.Update(SetEstimateReference(SetEstimateReferenceInt(estimateRef, estimateItem, estimateItemFilter.RevisionID)));

                InsertEstimateHistory(existingEstimateGraph, estimateItem, MakeQuoteEstimateHistory(estimateItem, false));

                return existingEstimateGraph;
            }

            var estimateGraph = PXGraph.CreateInstance<EstimateMaint>();
            var amEstimateItem = PXCache<AMEstimateItem>.CreateCopy(estimateItemFilter);
            amEstimateItem.IsNonInventory = true;
            // must trim the Revision ID to allow estimate to function properly
            amEstimateItem.RevisionID = estimateItemFilter.RevisionID.TrimIfNotNullEmpty();

            InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>
                >>.Select(estimateGraph, estimateItemFilter.InventoryCD);

            string taxCategory = null;
            if (inventoryItem != null)
            {
                amEstimateItem.InventoryID = inventoryItem.InventoryID;
                amEstimateItem.IsNonInventory = false;
                amEstimateItem.ItemDesc = inventoryItem.Descr;
                amEstimateItem.UOM = inventoryItem.BaseUnit;
                amEstimateItem.ItemClassID = inventoryItem.ItemClassID;
                taxCategory = inventoryItem.TaxCategoryID;
            }

            var primaryCuryId = (string)Base.Caches<TPrimary>().GetValue(primaryRow, "CuryID");
            var primaryCuryInfoId = (long?)Base.Caches<TPrimary>().GetValue(primaryRow, "CuryInfoID");
            if (primaryCuryId != null && primaryCuryInfoId != null)
            {
                amEstimateItem.CuryID = primaryCuryId;
                amEstimateItem.CuryInfoID = primaryCuryInfoId;
            }

            amEstimateItem = estimateGraph.Documents.Insert(amEstimateItem);
            amEstimateItem.QuoteSource = EstimateSource.GetEstimateQuoteSource(Base);
            amEstimateItem.IsLockedByQuote = Base is PX.Objects.CR.QuoteMaint;
            amEstimateItem = estimateGraph.Documents.Update(amEstimateItem);

            InsertEstimateHistory(estimateGraph, amEstimateItem, MakeQuoteEstimateHistory(amEstimateItem, true));

            var amEstimateReference = SetEstimateReference(SetEstimateReferenceInt(estimateGraph.EstimateReferenceRecord.Current ?? estimateGraph.EstimateReferenceRecord.Insert(new AMEstimateReference()), amEstimateItem, estimateItemFilter.RevisionID));
            amEstimateReference.TaxCategoryID = taxCategory;
            if (string.IsNullOrWhiteSpace(amEstimateReference.TaxCategoryID))
            {
                AMEstimateClass estimateClass = PXSelect<AMEstimateClass, Where<AMEstimateClass.estimateClassID,
                    Equal<Required<AMEstimateClass.estimateClassID>>>>.Select(estimateGraph,
                    estimateItemFilter.EstimateClassID);

                if (estimateClass != null)
                {
                    amEstimateReference.TaxCategoryID = estimateClass.TaxCategoryID;
                }
            }

            estimateGraph.EstimateReferenceRecord.Update(amEstimateReference);

            return estimateGraph;
        }

        protected virtual AMEstimateHistory InsertEstimateHistory(EstimateMaint estimateMaint, AMEstimateItem estimateItem, AMEstimateHistory estimateHistory)
        {
            if (estimateItem?.RevisionID == null || estimateHistory?.Description == null)
            {
                return null;
            }

            var currentEstItem = estimateMaint.Documents.Current;
            if (currentEstItem == null || currentEstItem.EstimateID.EqualsWithTrim(estimateItem.EstimateID))
            {
                estimateMaint.Documents.Current = estimateItem;
            }

            return estimateMaint.EstimateHistoryRecords.Insert(estimateHistory);
        }

        /// <summary>
        /// Does the given parameters require the configuration to be rebuild/changed?
        /// </summary>
        /// <param name="inventoryID">Inventory ID to verify</param>
        /// <param name="siteID">Warehouse ID to verify</param>
        /// <param name="configuration">current related configuration</param>
        /// <returns>True if a change of configuration is required</returns>
        internal bool ConfigurationChangeRequired(int? inventoryID, int? siteID, AMConfigurationResults configuration)
        {
            string configurationID;
            bool isSuccessful = ItemConfiguration.TryGetDefaultConfigurationID(inventoryID, siteID, out configurationID);
            return ConfigurationChangeRequired(isSuccessful ? configurationID : configuration.ConfigurationID, configuration) && AllowConfigurationsInt;
        }

        /// <summary>
        /// Does the given parameters require the configuration to be rebuild/changed?
        /// </summary>
        internal bool ConfigurationChangeRequired(string configurationID, AMConfigurationResults configuration)
        {
            return configuration == null || !configuration.ConfigurationID.EqualsWithTrim(configurationID);
        }

        protected virtual Numbering GetEstimateNumbering()
        {
            return EstimateSetup?.Current?.EstimateNumberingID == null
                ? null
                : (Numbering)Base.Caches<Numbering>()
                      .Locate(new Numbering { NumberingID = EstimateSetup.Current.EstimateNumberingID }) ??
                  PXSelect<Numbering,
                      Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>
                  >.Select(Base, EstimateSetup.Current.EstimateNumberingID);
        }

        protected virtual void AMOrderEstimateItemFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var isUserNumbering = GetEstimateNumbering()?.UserNumbering == true;
            var newEstimate = ((AMOrderEstimateItemFilter) e.Row)?.AddExisting != true;
            var isStockItem = ((AMOrderEstimateItemFilter)e.Row)?.IsNonInventory != true;
            PXUIFieldAttribute.SetEnabled<AMOrderEstimateItemFilter.estimateID>(cache, e.Row, !newEstimate || isUserNumbering);
            PXUIFieldAttribute.SetEnabled<AMOrderEstimateItemFilter.inventoryCD>(cache, e.Row, newEstimate);
            PXUIFieldAttribute.SetEnabled<AMOrderEstimateItemFilter.itemDesc>(cache, e.Row, newEstimate);
            PXUIFieldAttribute.SetEnabled<AMOrderEstimateItemFilter.uOM>(cache, e.Row, newEstimate && !isStockItem);
            PXUIFieldAttribute.SetEnabled<AMOrderEstimateItemFilter.itemClassID>(cache, e.Row, newEstimate && !isStockItem);
            PXUIFieldAttribute.SetEnabled<AMOrderEstimateItemFilter.siteID>(cache, e.Row, newEstimate);
            PXUIFieldAttribute.SetEnabled<AMOrderEstimateItemFilter.estimateClassID>(cache, e.Row, newEstimate);
        }

        protected virtual void AMOrderEstimateItemFilter_AddExisting_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMOrderEstimateItemFilter)e.Row;
            cache.SetDefaultExt<AMOrderEstimateItemFilter.estimateID>(row);
            if (row == null)
            {
                return;
            }

            row.RevisionID = row.AddExisting.GetValueOrDefault() ? null : EstimateSetup.Current.DefaultRevisionID;
            row.CurrentEstimate = null;
            row.InventoryCD = null;
            row.InventoryID = null;
            row.IsNonInventory = true;
            row.ItemClassID = null;
            row.UOM = null;
            row.ItemDesc = null;
            row.SiteID = null;
            row.EstimateClassID = EstimateSetup.Current.DefaultEstimateClassID;

            if (row.AddExisting.GetValueOrDefault())
            {
                return;
            }

            AMEstimateClass estimateClass = PXSelect<AMEstimateClass, Where<AMEstimateClass.estimateClassID,
                Equal<Required<AMEstimateClass.estimateClassID>>>>.Select(Base, row.EstimateClassID);

            if (estimateClass == null)
            {
                return;
            }
            row.ItemClassID = estimateClass.ItemClassID;

            INItemClass itemClass = PXSelect<INItemClass, Where<INItemClass.itemClassID,
                Equal<Required<INItemClass.itemClassID>>>>.Select(Base, row.ItemClassID);

            if (itemClass == null)
            {
                return;
            }
            row.UOM = itemClass.BaseUnit;
        }

        protected virtual void AMOrderEstimateItemFilter_EstimateID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var row = (AMOrderEstimateItemFilter)e.Row;
            if (row == null)
            {
                return;
            }

            e.NewValue = row.AddExisting.GetValueOrDefault() ? null : GetEstimateNumbering()?.NewSymbol;
        }

        protected virtual void AMOrderEstimateItemFilter_EstimateID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var row = (AMOrderEstimateItemFilter)e.Row;
            if (row == null 
                || row.AddExisting.GetValueOrDefault())
            {
                return;
            }

            var newSymbol = GetEstimateNumbering()?.NewSymbol;
            if (!string.IsNullOrWhiteSpace(newSymbol) && newSymbol.Equals(e.NewValue))
            {
                e.Cancel = true;
            }
        }

        protected virtual void AMOrderEstimateItemFilter_EstimateID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetOrderEstimateItemFilterFromEstimate((AMOrderEstimateItemFilter)e.Row);
        }

        protected virtual void AMOrderEstimateItemFilter_RevisionID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetOrderEstimateItemFilterFromEstimate((AMOrderEstimateItemFilter) e.Row);
        }

        protected virtual void AMOrderEstimateItemFilter_RevisionID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var row = (AMOrderEstimateItemFilter)e.Row;
            if (row == null || row.AddExisting.GetValueOrDefault())
            {
                return;
            }

            e.Cancel = true;
        }

        protected virtual void SetOrderEstimateItemFilterFromEstimate(AMOrderEstimateItemFilter row)
        {
            if (string.IsNullOrWhiteSpace(row?.EstimateID)
                || !row.AddExisting.GetValueOrDefault())
            {
                return;
            }

            row.CurrentEstimate = row.EstimateID;

            if (string.IsNullOrWhiteSpace(row.RevisionID))
            {
                return;
            }

            AMEstimateItem estimateItem = PXSelect<AMEstimateItem,
                Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                    And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>>>
            >.Select(Base, row.EstimateID, row.RevisionID);

            if (estimateItem == null)
            {
                row.CurrentEstimate = null;
                row.InventoryCD = null;
                row.ItemDesc = null;
                row.InventoryID = null;
                row.IsNonInventory = true;
                row.EstimateClassID = null;
                row.ItemClassID = null;
                row.UOM = null;
                row.SiteID = null;
                return;
            }

            row.InventoryCD = estimateItem.InventoryCD;
            row.InventoryID = estimateItem.InventoryID;
            row.IsNonInventory = estimateItem.IsNonInventory;
            row.ItemDesc = estimateItem.ItemDesc;
            row.UOM = estimateItem.UOM;
            row.ItemClassID = estimateItem.ItemClassID;
            row.EstimateClassID = estimateItem.EstimateClassID;
            row.SiteID = estimateItem.SiteID;
        }

        protected virtual void AMOrderEstimateItemFilter_RevisionID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var row = (AMOrderEstimateItemFilter)e.Row;
            if (string.IsNullOrWhiteSpace(row?.EstimateID) || row.EstimateID.EqualsWithTrim(GetEstimateNumbering()?.NewSymbol))
            {
                e.NewValue = EstimateSetup.Current?.DefaultRevisionID;
                return;
            }

            var primaryRevision = (AMEstimateItem)PXSelect<AMEstimateItem,
                Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                And<AMEstimateItem.revisionID, Equal<AMEstimateItem.primaryRevisionID>>>>.Select(Base, row.EstimateID);

            e.NewValue = primaryRevision?.RevisionID;
        }

        protected virtual void AMOrderEstimateItemFilter_EstimateClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMOrderEstimateItemFilter)e.Row;
            if (row?.IsNonInventory == null || row.IsNonInventory == false)
            {
                return;
            }

            AMEstimateClass estimateClass = PXSelect<AMEstimateClass, Where<AMEstimateClass.estimateClassID,
                Equal<Required<AMEstimateClass.estimateClassID>>>>.Select(Base, row.EstimateClassID);

            if (estimateClass == null)
            {
                return;
            }
            row.ItemClassID = estimateClass.ItemClassID;
        }

        protected virtual void AMOrderEstimateItemFilter_ItemClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMOrderEstimateItemFilter)e.Row;
            if (row == null)
            {
                return;
            }

            INItemClass itemClass = PXSelect<INItemClass, Where<INItemClass.itemClassID,
                Equal<Required<INItemClass.itemClassID>>>>.Select(Base, row.ItemClassID);

            if (itemClass == null)
            {
                return;
            }
            row.UOM = itemClass.BaseUnit;
        }

        protected abstract void PrimaryRowDeleted(PXCache cache, PXRowDeletedEventArgs e);

        public virtual void RemoveEstimateReference(PXResultset<AMEstimateReference> estimates, string eventDescription)
        {
            foreach (PXResult<AMEstimateReference, AMEstimateItem> result in estimates)
            {
                var estReference = (AMEstimateReference)result;
                var estItem = (AMEstimateItem)result;

                if (estReference == null || string.IsNullOrWhiteSpace(estReference.EstimateID) || string.IsNullOrWhiteSpace(estReference.RevisionID)
                || estItem == null || string.IsNullOrWhiteSpace(estItem.EstimateID) || string.IsNullOrWhiteSpace(estItem.RevisionID))
                {
                    continue;
                }

                // Check for other revsions of the estimate on the Opportunity
                var existingRevision = PXSelect<AMEstimateReference,
                    Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateReference.estimateID>>,
                        And<AMEstimateReference.revisionID, NotEqual<Required<AMEstimateReference.revisionID>>,
                        And<AMEstimateReference.opportunityID, IsNotNull
                        >>>>.Select(Base, estReference.EstimateID, estReference.RevisionID);

                RemoveEstimateReference(estReference, estItem, eventDescription, existingRevision == null ? false : true);
            }
        }

        public virtual void RemoveEstimateReference(AMEstimateReference estReference, AMEstimateItem estItem, string description, bool existingReference)
        {
            Common.Cache.AddCacheView<AMEstimateHistory>(Base);
            Common.Cache.AddCacheView<AMEstimateItem>(Base);
            Common.Cache.AddCacheView<AMEstimateReference>(Base);
            
            if (estReference == null || string.IsNullOrWhiteSpace(estReference.EstimateID) || string.IsNullOrWhiteSpace(estReference.RevisionID)
                || estItem == null || string.IsNullOrWhiteSpace(estItem.EstimateID) || string.IsNullOrWhiteSpace(estItem.RevisionID))
            {
                return;
            }

            if (!existingReference)
            {
                // Update the Estimate Item record 
                estItem.QuoteSource = EstimateSource.Estimate;
                estItem.IsLockedByQuote = false;
                Base.Caches<AMEstimateItem>().Update(estItem);
            }

            //Update the Estimate Reference record
            EstimateGraphHelper.ClearQuoteReferenceFields(ref estReference);
            estReference.TaxLineNbr = null;
            Base.Caches<AMEstimateReference>().Update(estReference);

            Base.Caches<AMEstimateHistory>().Insert(new AMEstimateHistory
            {
                EstimateID = estItem.EstimateID.TrimIfNotNullEmpty(),
                RevisionID = estItem.RevisionID.TrimIfNotNullEmpty(),
                Description = description
            });
        }
    }
}