using System;
using PX.Objects.AM.Standalone;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Reusable business object for MFG and Opportunities
    /// </summary>
    public abstract class OpportunityBaseAMExtension<TGraph, TPrimary> :
        SalesOrderBaseAMExtension<TGraph, TPrimary, 
            Where<AMConfigurationResults.opportunityQuoteID,
            Equal<Current<CROpportunityProducts.quoteID>>,
                And<AMConfigurationResults.opportunityLineNbr,
                    Equal<Current<CROpportunityProducts.lineNbr>>>>>
        where TGraph : PXGraph
        where TPrimary : class, IBqlTable, new()
    {
        /// <summary>
        /// Defines the default mapping of the <see cref="OpportunityDocument" /> mapped cache extension to a DAC.
        /// </summary>
        protected class OpportunityDocumentMapping : IBqlMapping 
        {
            protected Type _table;
            public Type Table => _table;

            public Type Extension => typeof(OpportunityDocument);

            /// <summary>Creates the default mapping of the <see cref="OpportunityDocument" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC. to MAP</param>
            public OpportunityDocumentMapping(Type table)
            {
                _table = table;
            }

            public Type QuoteID = typeof(OpportunityDocument.quoteID);
        }

        /// <summary>Returns the mapping of the <see cref="OpportunityDocument" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you  can either return the default mapping or override the default
        /// mapping in this method.</remarks>
        protected abstract OpportunityDocumentMapping GetOpportunityDocumentMapping();
        
        /// <summary>
        /// Linked to estimates tab --> grid
        /// </summary>
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<
            AMEstimateReference, 
            InnerJoin<AMEstimateItem, 
                On<AMEstimateReference.estimateID, Equal<AMEstimateItem.estimateID>,
                And<AMEstimateReference.revisionID, Equal<AMEstimateItem.revisionID>>>>,
            Where<AMEstimateReference.opportunityQuoteID, Equal<Current<OpportunityDocument.quoteID>>>> OpportunityEstimateRecords;

        public override void Initialize()  
        {
            base.Initialize();
            OpportunityEstimateRecords.AllowDelete = false;
            OpportunityEstimateRecords.AllowInsert = false;
            OpportunityEstimateRecords.AllowUpdate = false;
            OpportunityEstimateRecords.AllowSelect = AllowEstimates;
            SelectedEstimateRecord.AllowDelete = false;

            //always enable the link command buttons...
            ViewEstimate.SetEnabled(true);

            ConfigureEntry.SetEnabled(AllowConfigurationsInt);

            PXUIFieldAttribute.SetVisible<CROpportunityProductsExt.isConfigurable>(Base.Caches<CROpportunityProducts>(), null, AllowConfigurations);

            //return PXAccess.FeatureInstalled<FeaturesSet.manufacturingEstimating>();
        }

        /// <summary>
        /// Base opportunity related graph view name for the opportunity products
        /// </summary>
        public virtual string ProductsViewName => "Products";

        /// <summary>
        /// Does the current opportunity contain estimates?
        /// </summary>
        protected override bool ContainsEstimates
        {
            get
            {
                if (!OpportunityEstimateRecords.Cache.Cached.Any_() || OpportunityEstimateRecords.Cache.Current == null)
                {
                    var results = OpportunityEstimateRecords.Select();
                    if (OpportunityEstimateRecords.Cache.Current == null && (results?.Count ?? 0) > 0)
                    {
                        OpportunityEstimateRecords.Cache.Current = results[0];
                    }
                }
                return OpportunityEstimateRecords.Cache.Cached.Any_() || OpportunityEstimateRecords.Cache.Current != null;
            }
        }

#if DEBUG
        //TODO 2018R1/2017R2 - find out where the CRTax stuff went (AMEstimateReference_TaxCategoryID_CacheAttached)
        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //[AMCRTax(typeof(CROpportunity), typeof(CROpportunityTax), typeof(CRTaxTran), TaxCalc = PX.Objects.TX.TaxCalc.ManualLineCalc)]
        //protected virtual void AMEstimateReference_TaxCategoryID_CacheAttached(PXCache sender)
        //{
        //} 
#endif

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXParent(typeof(Select<AMEstimateReference,
            Where<AMEstimateReference.opportunityQuoteID, Equal<Current<CROpportunityTax.quoteID>>,
                And<AMEstimateReference.taxLineNbr, Equal<Current<CROpportunityTax.lineNbr>>>>>))]
        protected virtual void CROpportunityTax_LineNbr_CacheAttached(PXCache sender)
        {
        }

#if DEBUG
        //TODO 2018R1/2017R2 - find out where the CRTax stuff went (CROpportunityProducts_TaxCategoryID_CacheAttached)
        ////Using a DAC extension does not work
        //[PXDBString(10, IsUnicode = true)]
        //[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        //[PXSelector(typeof(PX.Objects.TX.TaxCategory.taxCategoryID), DescriptionField = typeof(PX.Objects.TX.TaxCategory.descr))]
        //[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
        //    Where<InventoryItem.inventoryID, Equal<Current<CROpportunityProducts.inventoryID>>>>),
        //    PersistingCheck = PXPersistingCheck.Nothing)]
        //[CRTaxAMExtension(typeof(CROpportunity), typeof(CROpportunityTax), typeof(CRTaxTran), TaxCalc = PX.Objects.TX.TaxCalc.ManualLineCalc)]
        //protected virtual void CROpportunityProducts_TaxCategoryID_CacheAttached(PXCache sender)
        //{
        //} 
#endif

        protected virtual void CROpportunityProducts_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            var isManualPrice = row.ManualPrice.GetValueOrDefault();
            var isConfiguredItem = row.GetExtension<CROpportunityProductsExt>()?.IsConfigurable == true;

            del?.Invoke(sender, e);

            if (!isConfiguredItem || isManualPrice)
            {
                return;
            }

            var priceChanged = !sender.ObjectsEqual<CROpportunityProducts.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<CROpportunityProducts.curyExtPrice>(e.Row, e.OldRow);

            if (sender.ObjectsEqual<CROpportunityProducts.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<CROpportunityProducts.uOM>(e.Row, e.OldRow)
                && sender.ObjectsEqual<CROpportunityProducts.quantity>(e.Row, e.OldRow)
                && sender.ObjectsEqual<CROpportunityProducts.isFree>(e.Row, e.OldRow)
                && sender.ObjectsEqual<CROpportunityProducts.uOM>(e.Row, e.OldRow)
                && sender.ObjectsEqual<CROpportunityProducts.siteID>(e.Row, e.OldRow)
                && priceChanged && isManualPrice != row.ManualPrice.GetValueOrDefault())
            {
                // Keep value for configured lines...
                row.ManualPrice = isManualPrice;
            }
        }

        /// <summary>
        /// Similar implementation to "DAC_RowSelected" event naming but as a single parameter with the row already of the correct type (no need to cast). Method name must be "_".
        /// Is strong typed so it allow to avoid additional casting for row and detect renames of DAC fields and classes when events declared.
        /// Unable to determine if we can receive the base delegate for chained extensions (Ex: PXRowSelected del). Internal Acumatica ticket AC-101189 asks the same unanswered question --> Allowed starting 2019R2
        /// </summary>
        /// <param name="e">contains both the row already as type OpportunityDocument and includes the cache</param>
        protected virtual void _(Events.RowSelected<OpportunityDocument> e)
        {
            if (e?.Row == null)
            {
                return;
            }

            var isSavedOrder = e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted;

            AddEstimate.SetEnabled(AllowEstimates && isSavedOrder && OrderAllowsEdit);
            quickEstimate.SetEnabled(AllowEstimates && isSavedOrder);
            removeEstimate.SetEnabled(AllowEstimates && isSavedOrder && OrderAllowsEdit);

            ConfigureEntry.SetEnabled(AllowConfigurations && isSavedOrder);
            ConfigureEntry.SetVisible(AllowConfigurations);

            //temp check for feature enabled until case 110612 is resolved. Until then the total could be zero when working with Quotes
            var isQuoteFeature = PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.salesQuotes>();

            PXUIFieldAttribute.SetVisible(e.Cache, e.Row, "aMCuryEstimateTotal", AllowEstimates && !isQuoteFeature);
        }

        public virtual void CROpportunityProducts_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            OrderLineRowSelected(sender, row, row.GetExtension<CROpportunityProductsExt>(), (AMConfigurationResults)ItemConfiguration.Select(), AllowConfigurations);
        }


        protected virtual void CROpportunityProducts_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }
            var rowExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(row);
            if (rowExt == null)
            {
                return;
            }

            rowExt.AMParentLineNbr = row.LineNbr;
        }


        public virtual void CROpportunityProducts_RowDeleting(PXCache sender, PXRowDeletingEventArgs e, PXRowDeleting del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(row);

            if (AllowConfigurations
                && rowExt != null
                && rowExt.AMIsSupplemental.GetValueOrDefault()
                && rowExt.AMParentLineNbr != row.LineNbr)
            {
                e.Cancel = true;

                // Get the Parent Product Line
                CROpportunityProducts parentRow = PXSelect<
                    CROpportunityProducts,
                    Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunityProducts.quoteID>>,
                        And<CROpportunityProducts.lineNbr, Equal<Required<CROpportunityProducts.lineNbr>>>>>
                    .Select(Base, row.QuoteID, rowExt.AMParentLineNbr);

                var currentInventoryID = sender.GetStateExt<CROpportunityProducts.inventoryID>((object)row);
                var parentInventoryID = sender.GetStateExt<CROpportunityProducts.inventoryID>((object)parentRow);

                throw new PXException(Messages.GetLocal(Messages.CannotDeleteSupplementalOpportunityLine), currentInventoryID, parentInventoryID);
            }
        }

        public virtual void CROpportunityProducts_RowDeleted(PXCache sender, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(row);
            if (rowExt == null)
            {
                return;
            }

            foreach (CROpportunityProducts supplementalLine in PXSelect<
                CROpportunityProducts,
                Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunityProducts.quoteID>>,
                    And<CROpportunityProductsExt.aMParentLineNbr, Equal<Required<CROpportunityProductsExt.aMParentLineNbr>>>>>
                .Select(Base, row.QuoteID, rowExt.AMParentLineNbr))
            {
                var newExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(supplementalLine);
                newExt.AMParentLineNbr = supplementalLine.LineNbr;
                Base.Caches<CROpportunityProducts>().Delete(Base.Caches<CROpportunityProducts>().Update(supplementalLine));
            }
        }

        // In this graph, Acumatica uses the CuryUnitPrice field defaulting to set the price of the line. Changes to other fields simply call the cache.SetDefaultExt on this field.
        protected virtual void CROpportunityProducts_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting del)
        {
            if (CanUpdateConfigPrice((CROpportunityProducts)e.Row))
            {
                //Get config price
                e.NewValue = AMConfigurationPriceAttribute.GetPriceExt<AMConfigurationResults.displayPrice>(
                    ItemConfiguration.Cache, ItemConfiguration.Select(), ConfigCuryType.Document).GetValueOrDefault();
                e.Cancel = true;
                return;
            }

            del?.Invoke(sender, e);
        }

        protected static bool CanUpdateConfigPrice(CROpportunityProducts row)
        {
            if (row == null)
            {
                return false;
            }

            var rowExt = row.GetExtension<CROpportunityProductsExt>();
            return rowExt != null && rowExt.IsConfigurable.GetValueOrDefault() && row.ManualPrice != true &&
                   row.InventoryID != null && row.IsFree != true;
        }

        public virtual void CROpportunityProducts_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(row);
            if (rowExt == null)
            {
                return;
            }

            if (rowExt.IsConfigurable == true && AllowConfigurations
                && Base.Views[ProductsViewName].Ask(Messages.DeletingExistingConfiguration, MessageButtons.YesNo) == WebDialogResult.No)
            {
                e.NewValue = row.InventoryID;
                e.Cancel = true;
            }
        }

        public virtual void CROpportunityProducts_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(row);
            if (rowExt == null)
            {
                return;
            }

            if (AllowConfigurations)
            {
                UpdateConfigurationResult(row, rowExt, ItemConfiguration.Select());
            }
        }

        public virtual void CROpportunityProducts_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(row);
            if (rowExt == null)
            {
                return;
            }

            if (rowExt.IsConfigurable == true && AllowConfigurations
                && sender.GetStatus(e.Row) != PXEntryStatus.Inserted
                && ConfigurationChangeRequired(row.InventoryID, WarehouseAsID(e.NewValue), ItemConfiguration.Select())
                && Base.Views[ProductsViewName].Ask(Messages.DeletingExistingConfiguration, MessageButtons.YesNo) == WebDialogResult.No)
            {
                e.NewValue = row.SiteID;
                e.Cancel = true;
            }
        }

        public virtual void CROpportunityProducts_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            var row = (CROpportunityProducts)e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(row);
            if (rowExt == null)
            {
                return;
            }

            //If UOM defaulting wasn't done already, skip since this will be done later by another event handler
            if (row.UOM != null && AllowConfigurations)
            {
                UpdateConfigurationResult(row, rowExt, ItemConfiguration.Select());
            }
        }

        /// <summary>
        /// Update a given configuration to match the given product line information.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowExt"></param>s
        /// <param name="configuration"></param>
        private void UpdateConfigurationResult(CROpportunityProducts row, CROpportunityProductsExt rowExt, AMConfigurationResults configuration)
        {
            if (configuration != null &&
                (row.QuoteID != configuration.OpportunityQuoteID ||
                 row.LineNbr != configuration.OpportunityLineNbr))
            {
                return;
            }

            var isSuccessful = ItemConfiguration.TryGetDefaultConfigurationID(row.InventoryID, row.SiteID, out var configurationID);
            rowExt.AMConfigurationID = isSuccessful ? configurationID : null;

            var newConfigRequired = ConfigurationChangeRequired(rowExt.AMConfigurationID, configuration);
            if (!newConfigRequired)
            {
                return;
            }

            var isCurrentConfig = configuration != null;
            if (isCurrentConfig)
            {
                isCurrentConfig = ItemConfiguration.Delete(configuration) == null;
            }

            if (rowExt.IsConfigurable.GetValueOrDefault() && !isCurrentConfig)
            {
                InsertConfigurationResult(row, rowExt.AMConfigurationID);
            }
        }

        internal AMConfigurationResults InsertConfigurationResult(CROpportunityProducts row, string configurationID)
        {
            var configurationResult = new AMConfigurationResults()
            {
                ConfigurationID = configurationID,
                InventoryID = row.InventoryID,
                OpportunityQuoteID = row.QuoteID,
                OpportunityLineNbr = row.LineNbr,
                UOM = row.UOM,
                CuryInfoID = row.CuryInfoID
            };

            return ItemConfiguration.Insert(configurationResult);
        }

        public static void RefreshConfiguredOpportunityProduct(OpportunityMaint graph, CROpportunityProducts crOpportunityProducts, AMConfigurationResults configResults, bool calledFromOpportunity)
        {
            if (graph.Opportunity.Current == null)
            {
                return;
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            var graphExt = graph.GetExtension<OpportunityMaintAMExtension>();
            graphExt.ItemConfiguration.RemoveProductLineHandlers();

            graph.Products.Current = crOpportunityProducts;

            if (!crOpportunityProducts.ManualPrice.GetValueOrDefault())
            {
                var configUnitPrice = AMConfigurationPriceAttribute.GetPriceExt<AMConfigurationResults.displayPrice>(
                    graphExt.ItemConfiguration.Cache, graphExt.ItemConfiguration.Select(), ConfigCuryType.Document).GetValueOrDefault();
                graph.Products.Cache.SetValueExt<CROpportunityProducts.curyUnitPrice>(crOpportunityProducts, configUnitPrice);
            }

            if (!string.IsNullOrWhiteSpace(configResults.TranDescription))
            {
                graph.Products.Cache.SetValueExt<CROpportunityProducts.descr>(crOpportunityProducts, configResults.TranDescription);
            }
            graph.Products.Cache.SetValueExt<CROpportunityProductsExt.aMConfigKeyID>(crOpportunityProducts, configResults.KeyID);

            if (!calledFromOpportunity)
            {
                graph.Products.Update(crOpportunityProducts);
            }
        }

        public static void RefreshConfiguredQuoteProduct(QuoteMaint graph, CROpportunityProducts crOpportunityProducts, AMConfigurationResults configResults, bool calledFromQuote)
        {
            if (graph.Quote.Current == null)
            {
                return;
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            var graphExt = graph.GetExtension<QuoteMaintAMExtension>();
            graphExt.ItemConfiguration.RemoveProductLineHandlers();

            graph.Products.Current = crOpportunityProducts;

            if (!crOpportunityProducts.ManualPrice.GetValueOrDefault())
            {
                var configUnitPrice = AMConfigurationPriceAttribute.GetPriceExt<AMConfigurationResults.displayPrice>(
                    graphExt.ItemConfiguration.Cache, graphExt.ItemConfiguration.Select(), ConfigCuryType.Document).GetValueOrDefault();
                graph.Products.Cache.SetValueExt<CROpportunityProducts.curyUnitPrice>(crOpportunityProducts, configUnitPrice);
            }

            if (!string.IsNullOrWhiteSpace(configResults.TranDescription))
            {
                graph.Products.Cache.SetValueExt<CROpportunityProducts.descr>(crOpportunityProducts, configResults.TranDescription);
            }
            graph.Products.Cache.SetValueExt<CROpportunityProductsExt.aMConfigKeyID>(crOpportunityProducts, configResults.KeyID);

            if (!calledFromQuote)
            {
                graph.Products.Update(crOpportunityProducts);
            }
        }

        internal CRSetupExt CRSetupExtension
        {
            get
            {
                var crSetup = (CRSetup)Base.Caches<CRSetup>().Current ?? (CRSetup)PXSelect<CRSetup>.Select(Base);
                return crSetup?.GetExtension<CRSetupExt>();
            }
        }

        /// <summary>
        /// Indicates if order allows for estimates
        /// </summary>
        public bool AllowEstimates
        {
            get
            {
                var crSetupExt = CRSetupExtension;
                return AllowEstimatesInt && crSetupExt != null && crSetupExt.AMEstimateEntry.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Indicates if configurations are allowed
        /// </summary>
        public bool AllowConfigurations
        {
            get
            {
                var crSetupExt = CRSetupExtension;
                return AllowConfigurationsInt && crSetupExt != null && crSetupExt.AMConfigurationEntry.GetValueOrDefault();
            }
        }

        public virtual void AMEstimateItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            AMEstimateItemRowSelectedInt(cache, e, OrderAllowsEdit);
        }

        protected override AMEstimateReference SetEstimateReference(AMEstimateReference estimateReference)
        {
            if (estimateReference == null)
            {
                throw new ArgumentNullException(nameof(estimateReference));
            }

            var order = (CROpportunity)Base?.Caches<CROpportunity>()?.Current;
            if (order == null)
            {
                return estimateReference;
            }

            estimateReference.BAccountID = order.BAccountID;
            estimateReference.BranchID = order.BranchID;
            estimateReference.OpportunityID = order.OpportunityID;
            estimateReference.OpportunityQuoteID = order.QuoteNoteID;
            estimateReference.CuryInfoID = order.CuryInfoID;

            return estimateReference;
        }


        protected override AMEstimateHistory MakeQuoteEstimateHistory(AMEstimateItem amEstimateItem, bool estimateCreated)
        {
            var currentOrder = GetCROpportunity();
            if (currentOrder == null)
            {
                return null;
            }

            if (estimateCreated)
            {
                return new AMEstimateHistory
                {
                    RevisionID = amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                    Description = Messages.GetLocal(Messages.EstimateCreatedFromOpportunity,
                        amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                        currentOrder.OpportunityID)
                };
            }

            return new AMEstimateHistory
            {
                EstimateID = amEstimateItem.EstimateID.TrimIfNotNullEmpty(),
                RevisionID = amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                Description = Messages.GetLocal(Messages.EstimateAddedToOpportunity,
                    amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                    currentOrder.OpportunityID)
            };
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel, Tooltip = "Launch configuration entry")]
        [PXUIField(DisplayName = Messages.Configure, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        protected override void configureEntry()
        {
            var opportunityProducts = (CROpportunityProducts)Base?.Caches<CROpportunityProducts>()?.Current;
            var opportunityProductsExt = opportunityProducts?.GetExtension<CROpportunityProductsExt>();
            if (opportunityProducts == null || opportunityProductsExt == null)
            {
                return;
            }

            if (!AllowConfigurations || !opportunityProductsExt.IsConfigurable.GetValueOrDefault())
            {
                throw new PXException(Messages.NotConfigurableItem);
            }

            if (Base.IsDirty)
            {
                Base.Actions.PressSave();
            }

            AMConfigurationResults configuration = ItemConfiguration.Select();
            if (configuration != null)
            {
                var graph = PXGraph.CreateInstance<ConfigurationEntry>();
                graph.Results.Current = graph.Results.Search<AMConfigurationResults.configResultsID>(configuration.ConfigResultsID);
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Popup);
            }
        }

        /// <summary>
        /// Hyper-link on EstimateID to open the estimate page
        /// </summary>
        public PXAction<TPrimary> ViewEstimate;
        [PXButton]
        [PXUIField(DisplayName = "View Estimate", Visible = false)]
        protected virtual void viewEstimate()
        {
            if (OpportunityEstimateRecords?.Current?.EstimateID == null)
            {
                return;
            }

            EstimateMaint.Redirect(OpportunityEstimateRecords.Current.EstimateID, OpportunityEstimateRecords.Current.RevisionID);
        }


        /// <summary>
        /// Pre-check for existing estimates if allowed to be converted to sales details
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual bool CurrentEstimatesValidForSalesDetails(out PXException exception)
        {
            exception = null;
            var sb = new System.Text.StringBuilder();

            foreach (PXResult<AMEstimateReference, AMEstimateItem> result in OpportunityEstimateRecords.Select())
            {
                var estReference = (AMEstimateReference)result;
                var estItem = (AMEstimateItem)result;

                if (string.IsNullOrWhiteSpace(estReference?.RevisionID) || string.IsNullOrWhiteSpace(estItem?.RevisionID) || !estReference.RevisionID.EqualsWithTrim(estItem.RevisionID))
                {
                    continue;
                }
                var estMsg = string.Empty;
                if (estItem.IsNonInventory.GetValueOrDefault())
                {
                    estMsg = Messages.GetLocal(Messages.EstimateItemMustBeStockItem);
                }

                if (estItem.SiteID.GetValueOrDefault() == 0)
                {
                    estMsg = string.Concat(estMsg.PadRightSpace(), Messages.GetLocal(Messages.WarehouseMustBeSpecified));
                }

                if (EstimateContainsNonInventoryMaterial(estItem))
                {
                    estMsg = string.Concat(estMsg.PadRightSpace(), Messages.GetLocal(Messages.EstimateMaterialMustBeItems));
                }

                if (!string.IsNullOrWhiteSpace(estMsg))
                {
                    sb.AppendLine(Messages.GetLocal(Messages.UnableToConvertEstimateDueTo,
                        estItem.EstimateID.TrimIfNotNullEmpty(),
                        estItem.RevisionID.TrimIfNotNullEmpty(),
                        estMsg));
                }
            }

            if (sb.Length > 0)
            {
                exception = new PXException(sb.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the given estimate contains material indicated as non inventory
        /// </summary>
        /// <param name="estimateItem"></param>
        /// <returns>True if non inventory material exists</returns>
        protected virtual bool EstimateContainsNonInventoryMaterial(AMEstimateItem estimateItem)
        {
            return (AMEstimateMatl)PXSelect<AMEstimateMatl,
                Where<AMEstimateMatl.estimateID, Equal<Required<AMEstimateOper.estimateID>>,
                    And<AMEstimateMatl.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                    And<AMEstimateMatl.isNonInventory, Equal<True>>>>
                    >.SelectWindowed(Base, 0, 1, estimateItem.EstimateID, estimateItem.RevisionID) != null;
        }

        protected virtual void AddEstimatesToSalesOrder(SOOrderEntry soOrderEntry)
        {
            if (soOrderEntry == null)
            {
                throw new PXArgumentException(nameof(soOrderEntry));
            }

            foreach (PXResult<AMEstimateReference, AMEstimateItem> result in OpportunityEstimateRecords.Select())
            {
                var estReference = (AMEstimateReference)result;
                var estItem = (AMEstimateItem)result;

                if (string.IsNullOrWhiteSpace(estReference?.RevisionID) || string.IsNullOrWhiteSpace(estItem?.RevisionID) || !estReference.RevisionID.EqualsWithTrim(estItem.RevisionID))
                {
                    continue;
                }

                SOOrderEntryAMExtension.AddEstimateAsSalesLine(soOrderEntry, estItem);
            }
        }

        public virtual void CopyConfigurationsToNewQuote(Guid? quoteId, QuoteMaint graph)
        {
            if (quoteId == null)
            {
                throw new ArgumentNullException(nameof(quoteId));
            }

            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            foreach (CROpportunityProducts product in graph.Products.Select())
            {
                var productExt = product.GetExtension<CROpportunityProductsExt>();
                if (string.IsNullOrWhiteSpace(productExt?.AMConfigurationID))
                {
                    continue;
                }

                AMConfigurationResults fromConfigResult = PXSelect<AMConfigurationResults,
                        Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>,
                            And<AMConfigurationResults.opportunityLineNbr, Equal<Required<AMConfigurationResults.opportunityLineNbr>>>>>
                    .Select(graph, quoteId, product.LineNbr);

                if (fromConfigResult == null)
                {
                    continue;
                }

                CopyConfiguration(product, InsertConfigurationResult(product, productExt.AMConfigurationID), fromConfigResult, graph);
            }
        }

        public virtual void CopyConfiguration(CROpportunityProducts toProduct, AMConfigurationResults toConfigResult,
            AMConfigurationResults fromConfigResult, QuoteMaint graph)
        {
            if (toProduct == null)
            {
                throw new PXArgumentException(nameof(toProduct));
            }

            if (toConfigResult == null || fromConfigResult == null)
            {
                return;
            }

            ConfigSupplementalItemsHelper.RemoveProductSupplementalLineItems(graph, toConfigResult);
            ConfigurationCopyEngine.UpdateConfigurationFromConfiguration(graph, toConfigResult, fromConfigResult);

            //Try to "Finish" the configuration...
            var locatedConfig = ItemConfiguration.Locate(toConfigResult);
            if (locatedConfig == null)
            {
                return;
            }

            if (ItemConfiguration.IsDocumentValid(out var errorMessage))
            {
                locatedConfig.Completed = true;
                ItemConfiguration.Update(locatedConfig);

                ConfigurationSelect.UpdateQuoteWithConfiguredLineChanges(graph, toProduct, locatedConfig);

                return;
            }

            if (!graph.IsImport && !graph.IsContractBasedAPI)
            {
                graph.Products.Cache.RaiseExceptionHandling<CROpportunityProducts.inventoryID>(toProduct,
                    toProduct.InventoryID,
                    new PXSetPropertyException(Messages.ConfigurationNeedsAttention, PXErrorLevel.Warning,
                        errorMessage));
            }
        }

        public virtual void CopyEstimatesToQuote(Guid? quoteId, QuoteMaint graph)
        {
            EstimateCopy copyGraph = null;
            var persistQuoteMaint = false;
            var firstQuote = false;
            var currentQuote = graph?.Quote?.Current;

            if (currentQuote == null)
            {
                return;
            }

            CRQuote existingQuote = PXSelect<CRQuote,
                Where<CRQuote.opportunityID, Equal<Required<CRQuote.opportunityID>>,
                    And<CRQuote.quoteID, NotEqual<Required<CRQuote.quoteID>>
                >>>.Select(graph, graph.Quote.Current.OpportunityID, quoteId);

            if (existingQuote == null)
            {
                firstQuote = true;
            }

            foreach (PXResult<AMEstimateReference, AMEstimateItem> result in PXSelectJoin<
                AMEstimateReference,
                InnerJoin<AMEstimateItem, 
                    On<AMEstimateReference.estimateID, Equal<AMEstimateItem.estimateID>,
                    And<AMEstimateReference.revisionID, Equal<AMEstimateItem.revisionID>>>>,
                Where<AMEstimateReference.opportunityQuoteID, Equal<Required<AMEstimateReference.opportunityQuoteID>>
                >>.Select(graph, quoteId))
            {
                var estimateReference = (AMEstimateReference) result;
                var estimateItem = (AMEstimateItem) result;

                if (estimateReference == null || estimateItem == null)
                {
                    continue;
                }

                // Since First Quote, No Need to Copy Estimate, just set the Reference QuoteNbr and IsLockedByQuote = true
                if (firstQuote)
                {
                    estimateItem.IsLockedByQuote = true;
                    estimateReference.OpportunityQuoteID = currentQuote.QuoteID;
                    estimateReference.QuoteNbr = currentQuote.QuoteNbr;
                    graph.Caches<AMEstimateItem>().Update(estimateItem);
                    graph.Caches<AMEstimateReference>().Update(estimateReference);
                    persistQuoteMaint = true;
                    continue;
                }

                if (copyGraph == null)
                {
                    copyGraph = PXGraph.CreateInstance<EstimateCopy>();
                }

                copyGraph.Clear();

                AMEstimateReference maxRevision = PXSelect<AMEstimateReference,
                    Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateReference.estimateID>>>,
                    OrderBy<Desc<AMEstimateReference.revisionID>>
                >.SelectWindowed(Base, 0, 1, estimateItem.EstimateID);

                if (maxRevision == null)
                {
                    continue;
                }
                var nextRev = AutoNumberHelper.NextNumber(maxRevision.RevisionID);

                var estItem = copyGraph.CopyAsNewEstimateItem(estimateItem, nextRev, estimateItem.EstimateID, true);

                copyGraph.CreateNewRevision(estimateItem, estItem);
                var newReference = copyGraph.EstimateReferenceRecord.Current;

                if (newReference != null)
                {
                    newReference.OpportunityID = currentQuote.OpportunityID;
                    newReference.OpportunityQuoteID = currentQuote.QuoteID;
                    newReference.QuoteNbr = currentQuote.QuoteNbr;
                    copyGraph.EstimateReferenceRecord.Update(newReference);
                }

                //We want to keep the old references so lets remove that change...
                var locatedCurrentEstRef = (AMEstimateReference)copyGraph.EstimateReferenceRecord.Cache.Locate(estimateReference);
                if (locatedCurrentEstRef != null && locatedCurrentEstRef.QuoteNbr == null && estimateReference.QuoteNbr != null)
                {
                    locatedCurrentEstRef.OpportunityID = estimateReference.OpportunityID;
                    locatedCurrentEstRef.OpportunityQuoteID = estimateReference.OpportunityQuoteID;
                    locatedCurrentEstRef.QuoteNbr = estimateReference.QuoteNbr;
                    copyGraph.EstimateReferenceRecord.Update(locatedCurrentEstRef);
                }

                copyGraph.Actions.PressSave();
            }

            if (persistQuoteMaint)
            {
                graph.Persist();
            }
        }

        /// <summary>
        /// Set the given opportunity/quote related estimates to be the primary estimates. Remove non primary estimates in case they do not exist on the new primary quote
        /// </summary>
        protected virtual void ChangeEstimatePrimary<QuoteIdField>(PXCache cache)
            where QuoteIdField : IBqlField
        {
            var currentRow = cache.Current;
            if (currentRow == null || !AllowEstimates)
            {
                return;
            }

            var origRow = cache.GetOriginal(currentRow);
            if (origRow == null)
            {
                return;
            }

            ChangeEstimatePrimary((Guid?) cache.GetValue<QuoteIdField>(currentRow),
                (Guid?) cache.GetValue<QuoteIdField>(origRow));
        }

        internal void ChangeEstimatePrimary(Guid? primaryQuoteId, Guid? oldQuoteId)
        {
            if (primaryQuoteId == null || oldQuoteId == null || primaryQuoteId.Equals(oldQuoteId))
            {
                return;
            }

            //Set as primary...
            ChangeEstimatePrimary(primaryQuoteId.GetValueOrDefault());
        }

        private void ChangeEstimatePrimary(Guid quoteId)
        {
            if (!AllowEstimates)
            {
                return;
            }

            Common.Cache.AddCacheView<AMEstimatePrimary>(Base);

            var quoteStatus = ((CRQuote) PXSelect<
                    CRQuote, 
                    Where<CRQuote.quoteID, Equal<Required<CRQuote.quoteID>>>>
                    .Select(Base, quoteId))?.Status;

            foreach (PXResult<AMEstimateReference, AMEstimatePrimary> result in GetEstimateReferences(quoteId))
            {
                var estRef = (AMEstimateReference)result;
                var estPrimary = Base.Caches<AMEstimatePrimary>().LocateElseCopy<AMEstimatePrimary>(result);

                if (estRef?.RevisionID == null || estPrimary?.PrimaryRevisionID == null
                                               || estPrimary.PrimaryRevisionID.EqualsWithTrim(estRef.RevisionID))
                {
                    continue;
                }

                estPrimary.PrimaryRevisionID = estRef.RevisionID;
                if (TryConvertQuoteStatusToEstimateStatus(result, estPrimary, quoteStatus, out var newStatus))
                {
                    estPrimary.EstimateStatus = newStatus;
                }

                Base.Caches<AMEstimatePrimary>().Update(estPrimary);
            }
        }

        /// <summary>
        /// Sync quote status to estimate status
        /// </summary>
        protected virtual void ChangeEstimateStatus<QuoteIdField, StatusField>(PXCache cache)
            where QuoteIdField : IBqlField
            where StatusField : IBqlField
        {
            var currentRow = cache.Current;
            if (currentRow == null)
            {
                return;
            }

            var origRow = cache.GetOriginal(currentRow);
            if (origRow == null || cache.GetValue<StatusField>(origRow) == null || cache.ObjectsEqual<StatusField>(origRow, currentRow))
            {
                return;
            }

            Common.Cache.AddCacheView<AMEstimatePrimary>(Base);

            foreach (PXResult<AMEstimateReference, AMEstimatePrimary> result in GetEstimateReferences(cache.GetValue<QuoteIdField>(currentRow)))
            {
                var estPrimary = Base.Caches<AMEstimatePrimary>().LocateElseCopy<AMEstimatePrimary>(result);

                if (!TryConvertQuoteStatusToEstimateStatus(result, estPrimary, (string) cache.GetValue<StatusField>(currentRow), out var newStatus))
                {
                    continue;
                }

                estPrimary.EstimateStatus = newStatus;
                Base.Caches<AMEstimatePrimary>().Update(estPrimary);
            }
        }

        protected virtual PXResultset<AMEstimateReference> GetEstimateReferences(object quoteId)
        {
            return PXSelectJoin<
                AMEstimateReference,
                InnerJoin<AMEstimatePrimary,
                    On<AMEstimatePrimary.estimateID, Equal<AMEstimateReference.estimateID>>>,
                Where<AMEstimateReference.opportunityQuoteID, Equal<Required<AMEstimateReference.opportunityQuoteID>>>>
                .Select(Base, quoteId);
        }

        protected virtual bool TryConvertQuoteStatusToEstimateStatus(AMEstimateReference estRef, AMEstimatePrimary estPrimary, string quoteStatus, out int? estimateStatus)
        {
            estimateStatus = null;
            if (estRef?.RevisionID == null || estPrimary?.PrimaryRevisionID == null || !estPrimary.PrimaryRevisionID.EqualsWithTrim(estRef.RevisionID))
            {
                return false;
            }

            estimateStatus = ConvertQuoteStatusToEstimateStatus(quoteStatus, estPrimary.EstimateStatus);
            return estimateStatus != null && estPrimary.EstimateStatus != estimateStatus;
        }

        protected virtual int? ConvertQuoteStatusToEstimateStatus(string quoteStatus, int? currentEstimateStatus)
        {
            if (quoteStatus == null || EstimateStatus.IsFinished(currentEstimateStatus) || currentEstimateStatus == EstimateStatus.Completed)
            {
                return currentEstimateStatus ?? EstimateStatus.NewStatus;
            }

            switch (quoteStatus)
            {
                case CRQuoteStatusAttribute.Draft:
                    return EstimateStatus.NewStatus;
                case CRQuoteStatusAttribute.Sent:
                    return EstimateStatus.Sent;
                case CRQuoteStatusAttribute.PendingApproval:
                    return EstimateStatus.PendingApproval;
                case CRQuoteStatusAttribute.Approved:
                    return EstimateStatus.Approved;
                case CRQuoteStatusAttribute.Rejected:
                    return EstimateStatus.InProcess;
            }

            return currentEstimateStatus ?? EstimateStatus.NewStatus;
        }

        protected virtual CROpportunity GetCROpportunity()
        {
            var viewName = Base is QuoteMaint ?
                "CurrentOpportunity" :
                "Opportunity";

            PXView view = null;
            if (Base?.Views?.TryGetValue(viewName, out view) != true)
            {
                return null;
            }

            return (CROpportunity)view.Cache?.Current ?? (CROpportunity)view.SelectSingle();
        }
    }
}
