using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{

    public class ConfigurationSelect<TWhere> : ConfigurationSelect
        where TWhere : IBqlWhere, new()
    {
        public ConfigurationSelect(PXGraph graph) : base(graph)
        {
            base.View.WhereNew(typeof(TWhere));
        }
    }

    public class ConfigurationSelect : PXSelect<AMConfigurationResults>, IRuleProcessor, IRuleConditionValidator
    {
        public bool IsConfiguratorActive => Features.ProductConfiguratorEnabled();

        /// <summary>
        /// Are any of the configuration caches dirty
        /// </summary>
        public bool IsDirty =>
            Cache.IsDirty || 
            ResultAttributes.Cache.IsDirty ||
            ResultFeatures.Cache.IsDirty ||
            ResultOptions.Cache.IsDirty ||
            ResultRules.Cache.IsDirty;

        /// <summary>
        /// Is the current cache of configurations being used in a copy configuration process
        /// </summary>
        public bool IsCopyMode => _isCopyMode;

        public void SetCopyMode(bool isCopyMode)
        {
            SetCopyMode(isCopyMode, false);
        }

        public void SetCopyMode(bool isCopyMode, bool addHandlers)
        {
            bool changing = _isCopyMode != isCopyMode;

            if (!isCopyMode && addHandlers)
            {
                AddRulesHandlers();
            }

            if (!changing)
            {
                return;
            }

            if (isCopyMode)
            {
                RemoveRulesHandlers();
            }
            AMDebug.TraceWriteMethodName($"Changing value from {_isCopyMode} to {isCopyMode}");
            _isCopyMode = isCopyMode;
        }

        /// <summary>
        /// Is the current cache of configurations being used in a copy configuration process
        /// </summary>
        private bool _isCopyMode;

        private bool _handlersRemoved;
        
        #region DataViews

        public AMInViewSelect<AMConfigResultsAttribute,
                    Where<AMConfigResultsAttribute.configResultsID,
                        Equal<Optional<AMConfigurationResults.configResultsID>>>> ResultAttributes;

        public AMInViewSelect<AMConfigResultsFeature,
                    Where<AMConfigResultsFeature.configResultsID,
                        Equal<Optional<AMConfigurationResults.configResultsID>>>> ResultFeatures;

        public AMInViewSelect<AMConfigResultsOption,
                    Where<AMConfigResultsOption.configResultsID,
                        Equal<Optional<AMConfigurationResults.configResultsID>>,
                    And<AMConfigResultsOption.featureLineNbr,
                        Equal<Optional<AMConfigResultsFeature.featureLineNbr>>>>> ResultOptions;

        public AMInViewSelect<AMConfigResultsRule,
                    Where<AMConfigResultsRule.configResultsID,
                        Equal<Optional<AMConfigurationResults.configResultsID>>>> ResultRules;

        public AMInViewSelectReadOnly<AMConfiguration,
                   Where<AMConfiguration.configurationID,
                       Equal<Current<AMConfigurationResults.configurationID>>,
                   And<AMConfiguration.revision,
                       Equal<Current<AMConfigurationResults.revision>>>>> Configuration;

        public AMInViewSelectReadOnly<AMConfigurationAttribute,
                    Where<AMConfigurationAttribute.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfigurationAttribute.revision,
                        Equal<Current<AMConfiguration.revision>>>>> ConfigAttributes;

        public AMInViewSelectReadOnly<AMConfigurationFeature,
                    Where<AMConfigurationFeature.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfigurationFeature.revision,
                        Equal<Current<AMConfiguration.revision>>>>> ConfigFeatures;

        public AMInViewSelectReadOnly<AMConfigurationOption,
                    Where<AMConfigurationOption.configurationID,
                        Equal<Current<AMConfigurationFeature.configurationID>>,
                    And<AMConfigurationOption.revision,
                        Equal<Current<AMConfigurationFeature.revision>>,
                    And<AMConfigurationOption.configFeatureLineNbr,
                        Equal<Current<AMConfigurationFeature.lineNbr>>>>>> ConfigOptions;

        public AMInViewSelectReadOnly<AMConfigurationRule,
                   Where<AMConfigurationRule.configurationID,
                       Equal<Current<AMConfigurationResults.configurationID>>,
                   And<AMConfigurationRule.revision,
                       Equal<Current<AMConfigurationResults.revision>>>>> ConfigRules;

        public AMInViewSelectReadOnly<AMConfigurationAttribute,
                    Where<AMConfigurationAttribute.configurationID,
                        Equal<Optional<AMConfigResultsAttribute.configurationID>>,
                    And<AMConfigurationAttribute.revision,
                        Equal<Optional<AMConfigResultsAttribute.revision>>,
                    And<AMConfigurationAttribute.lineNbr,
                        Equal<Optional<AMConfigResultsAttribute.attributeLineNbr>>>>>> CurrentConfigAttributes;

        public AMInViewSelectReadOnly<AMConfigurationFeature,
                    Where<AMConfigurationFeature.configurationID,
                        Equal<Optional<AMConfigResultsFeature.configurationID>>,
                    And<AMConfigurationFeature.revision,
                        Equal<Optional<AMConfigResultsFeature.revision>>,
                    And<AMConfigurationFeature.lineNbr,
                        Equal<Optional<AMConfigResultsFeature.featureLineNbr>>>>>> CurrentConfigFeatures;

        public AMInViewSelectReadOnly<AMConfigurationOption,
                    Where<AMConfigurationOption.configurationID,
                        Equal<Optional<AMConfigResultsOption.configurationID>>,
                    And<AMConfigurationOption.revision,
                        Equal<Optional<AMConfigResultsOption.revision>>,
                    And<AMConfigurationOption.configFeatureLineNbr,
                        Equal<Optional<AMConfigResultsOption.featureLineNbr>>,
                    And<AMConfigurationOption.lineNbr,
                        Equal<Optional<AMConfigResultsOption.optionLineNbr>>>>>>> CurrentConfigOptions;

        public AMInViewSelect<CurrencyInfo,
                    Where<CurrencyInfo.curyInfoID,
                        Equal<Current<AMConfigurationResults.curyInfoID>>>> currencyinfo;

        public AMInViewSelectReadOnly<SOLine,
                    Where<SOLine.orderType,
                        Equal<Current<AMConfigurationResults.ordTypeRef>>,
                    And<SOLine.orderNbr,
                        Equal<Current<AMConfigurationResults.ordNbrRef>>,
                    And<SOLine.lineNbr,
                        Equal<Current<AMConfigurationResults.ordLineRef>>>>>> SOLineRef;

        public AMInViewSelectReadOnly<CROpportunityProducts,
                    Where<CROpportunityProducts.quoteID,
                        Equal<Current<AMConfigurationResults.opportunityQuoteID>>,
                    And<CROpportunityProducts.lineNbr,
                        Equal<Current<AMConfigurationResults.opportunityLineNbr>>>>> OpportunityProductRef;
        #endregion

        #region CTOR

        public ConfigurationSelect(PXGraph graph)
            : base(graph)
        {
            graph.Initialized += OnGraphInitialized;
        }

        private void OnGraphInitialized(PXGraph graph)
        {
            this.InitializeViews(graph);
            if (Features.ProductConfiguratorEnabled())
            {
                RegisterHandlers(graph);
            }
        }

        private void RegisterHandlers(PXGraph graph)
        {
            graph.RowInserting.AddHandler<AMConfigurationResults>(AMConfigurationResults_RowInserting);
            graph.RowInserted.AddHandler<AMConfigurationResults>(AMConfigurationResults_RowInserted);
            graph.RowUpdated.AddHandler<AMConfigurationResults>(AMConfigurationResults_RowUpdated);
            graph.FieldUpdated.AddHandler<AMConfigurationResults.completed>(AMConfigurationResults_Completed_FieldUpdated);
            graph.FieldDefaulting.AddHandler<AMConfigurationResults.revision>(AMConfigurationResults_Revision_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMConfigurationResults.curyFixedPriceTotal>(AMConfigurationResults_CuryFixedPriceTotal_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMConfigurationResults.curyBOMPriceTotal>(AMConfigurationResults_CuryBOMPriceTotal_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMConfigResultsOption.qty>(AMConfigResultsOption_Qty_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMConfigResultsOption.unitPrice>(AMConfigResultsOption_UnitPrice_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMConfigResultsOption.curyUnitPrice>(AMConfigResultsOption_CuryUnitPrice_FieldDefaulting);
            graph.FieldVerifying.AddHandler<AMConfigResultsOption.included>(AMConfigResultsOption_Included_FieldVerifying);
            graph.FieldUpdated.AddHandler<AMConfigResultsOption.included>(AMConfigResultsOption_Included_FieldUpdated);
            graph.FieldVerifying.AddHandler<AMConfigResultsAttribute.value>(AMConfigResultsAttribute_Value_FieldVerifying);
            graph.FieldUpdated.AddHandler<AMConfigResultsAttribute.value>(AMConfigResultsAttribute_Value_FieldUpdated);

            if (graph is SOOrderEntry)
            {
                graph.FieldUpdated.AddHandler<SOOrder.completed>(SOOrder_Completed_FieldUpdated);
                graph.FieldUpdated.AddHandler<SOOrder.cancelled>(SOOrder_Cancelled_FieldUpdated);
                graph.FieldUpdating.AddHandler<SOLineExt.aMConfigKeyID>(SOLine_AMConfigKeyID_FieldUpdating);
                graph.FieldUpdated.AddHandler<SOLineExt.aMConfigKeyID>(SOLine_AMConfigKeyID_FieldUpdated);
                graph.RowUpdated.AddHandler<SOLine>(SOLine_RowUpdated);
            }

            if (graph is OpportunityMaint)
            {
                graph.RowUpdated.AddHandler<CROpportunity>(CROpportunity_RowUpdated);
                graph.FieldUpdating.AddHandler<CROpportunityProductsExt.aMConfigKeyID>(CROpportunityProducts_AMConfigKeyID_FieldUpdating);
                graph.FieldUpdated.AddHandler<CROpportunityProductsExt.aMConfigKeyID>(CROpportunityProducts_AMConfigKeyID_FieldUpdated);
                graph.RowUpdated.AddHandler<CROpportunityProducts>(CROpportunityProducts_RowUpdated);
            }

            if (graph is QuoteMaint)
            {
                graph.RowUpdated.AddHandler<CRQuote>(CRQuote_RowUpdated);
                graph.FieldUpdating.AddHandler<CROpportunityProductsExt.aMConfigKeyID>(CROpportunityProducts_AMConfigKeyID_FieldUpdating);
                graph.FieldUpdated.AddHandler<CROpportunityProductsExt.aMConfigKeyID>(CROpportunityProducts_AMConfigKeyID_FieldUpdated);
                graph.RowUpdated.AddHandler<CROpportunityProducts>(CROpportunityProducts_RowUpdated);
            }

            if (graph is ProdMaint)
            {
                graph.FieldUpdating.AddHandler<AMConfigurationResults.keyID>(AMConfigurationResults_KeyID_FieldUpdating);
                graph.FieldUpdated.AddHandler<AMConfigurationResults.keyID>(AMConfigurationResults_KeyID_FieldUpdated);
                graph.RowUpdated.AddHandler<AMProdItem>(AMProdItem_RowUpdated);
            }
        }

        #endregion

        /// <summary>
        /// Persist only the configuration data
        /// </summary>
        public virtual void ConfigPersistInsertUpdate()
        {
            using (var ts = new PXTransactionScope())
            {
                Cache.Persist(PXDBOperation.Insert);
                Cache.Persist(PXDBOperation.Update);

                ResultAttributes.Cache.Persist(PXDBOperation.Insert);
                ResultAttributes.Cache.Persist(PXDBOperation.Update);

                ResultFeatures.Cache.Persist(PXDBOperation.Insert);
                ResultFeatures.Cache.Persist(PXDBOperation.Update);

                ResultOptions.Cache.Persist(PXDBOperation.Insert);
                ResultOptions.Cache.Persist(PXDBOperation.Update);

                ResultRules.Cache.Persist(PXDBOperation.Insert);
                ResultRules.Cache.Persist(PXDBOperation.Update);

                ts.Complete(_Graph);
            }

            Cache.Persisted(false);
            ResultAttributes.Cache.Persisted(false);
            ResultFeatures.Cache.Persisted(false);
            ResultOptions.Cache.Persisted(false);
            ResultRules.Cache.Persisted(false);
        }

        public override AMConfigurationResults Insert()
        {
            if (!Features.ProductConfiguratorEnabled())
            {
                return null;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName();
#endif
            return base.Insert();
        }

        public override AMConfigurationResults Insert(AMConfigurationResults item)
        {
            if (!Features.ProductConfiguratorEnabled())
            {
                return null;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"{item.ConfigurationID} - {item.ConfigResultsID}");
#endif
            return base.Insert(item);
        }

        public override AMConfigurationResults Update(AMConfigurationResults item)
        {
            if (!Features.ProductConfiguratorEnabled())
            {
                return null;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"{item.ConfigurationID} - {item.ConfigResultsID}");
#endif
            return base.Update(item);
        }

        internal bool IsDocumentValid(out string errorMessage)
        {
            bool isValid = true;
            var errorList = new List<string>();

            foreach (AMConfigResultsFeature feature in ResultFeatures.Select())
            {
                isValid &= IsFeatureOptionValid(feature, out errorMessage);
                AggregateErrorMessage(errorMessage, errorList);
            }

            foreach (AMConfigResultsAttribute attribute in ResultAttributes.Select())
            {
                isValid &= ValidateAttribute(attribute, out errorMessage);
                AggregateErrorMessage(errorMessage, errorList);
            }

            errorMessage = string.Join(", ", errorList);
            return isValid;
        }

        internal bool IsFeatureOptionValid(AMConfigResultsFeature feature, out string errorMessage, bool aggregateOptionErrorMessages = true)
        {
            var isValid = true;

            var errorList = new List<string>();

            isValid &= ValidateFeature(feature, out errorMessage);
            AggregateErrorMessage(errorMessage, errorList);

            var options = ResultOptions
                .Select(Current.ConfigResultsID, feature.FeatureLineNbr)
                .Select(opt => (AMConfigResultsOption)opt)
                .ToList();

            var totalSelected = options.Where(opt => opt.Included == true).Count();

            if (feature.MinSelection.HasValue && feature.MinSelection > totalSelected)
            {
                errorMessage = Messages.GetLocal(Messages.SelMustBeGreaterThanMinSel, totalSelected, feature.MinSelection);
                isValid = false;
            }
            else if (feature.MaxSelection.HasValue && feature.MaxSelection < totalSelected)
            {
                errorMessage = Messages.GetLocal(Messages.SelMustBeSmallerThanMaxSel, totalSelected, feature.MaxSelection);
                isValid = false;
            }

            AggregateErrorMessage(errorMessage, errorList);

            foreach (var option in options)
            {
                isValid &= ValidateOption(option, out errorMessage);

                if (aggregateOptionErrorMessages)
                    AggregateErrorMessage(errorMessage, errorList);
            }

            errorMessage = String.Join(" / ", errorList);
            return isValid;
        }

        internal void AggregateErrorMessage(string errorMessage, List<string> errorList)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                errorList.Add(errorMessage);
                errorMessage = string.Empty;
            }
        }

        internal virtual void UpdateConfigurationFromKey(PXGraph graph, AMConfigurationResults currentConfigResult, string configKeyFrom)
        {
            if (currentConfigResult == null)
            {
                return;
            }

            var fromConfigResult = ConfigurationCopyEngine.GetConfigResultByKey(graph, configKeyFrom, currentConfigResult.ConfigurationID, currentConfigResult.ConfigResultsID);
            if (fromConfigResult == null)
            {
                return;
            }

#if DEBUG
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Copy Configurations From / To");
            sb.AppendLine($"ConfigResultsID: {fromConfigResult.ConfigResultsID} / {currentConfigResult.ConfigResultsID}");
            sb.AppendLine($"ConfigurationID: {fromConfigResult.ConfigurationID} / {currentConfigResult.ConfigurationID}");
            sb.AppendLine($"Revision:        {fromConfigResult.Revision} / {currentConfigResult.Revision}");
            sb.AppendLine($"KeyID:           {fromConfigResult.KeyID} / {currentConfigResult.KeyID}");
            sb.AppendLine($"KeyDescription:  {fromConfigResult.KeyDescription} / {currentConfigResult.KeyDescription}");
            AMDebug.TraceWriteMethodName(sb.ToString());
#endif

            ConfigurationCopyEngine.UpdateConfigurationFromConfiguration(graph, currentConfigResult, fromConfigResult, true, false);

            try
            {
                SetCopyMode(true);

                ConfigurationCopyEngine.UpdateConfigurationFromConfiguration(graph, graph.Caches<AMConfigurationResults>().LocateElse(currentConfigResult), fromConfigResult, false, true);
            }
            finally
            {
                SetCopyMode(false, true);
            }
        }

        #region Event Handlers

        protected virtual void AMConfigurationResults_KeyID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (AMConfigurationResults)e.Row;
            if (row == null || string.IsNullOrWhiteSpace(row.ConfigurationID))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(row.KeyID) && !sender.Graph.IsImport && !sender.Graph.IsContractBasedAPI && 
                Ask(Messages.ConfirmConfigKeyChange,
                    Messages.ConfirmConfigKeyChangeContinue,
                    MessageButtons.YesNo) != WebDialogResult.Yes)
            {
                e.Cancel = true;
                e.NewValue = row.KeyID;
                return;
            }
        }

        protected virtual void AMConfigurationResults_KeyID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMConfigurationResults)e.Row;
            if (row == null)
            {
                return;
            }

            RemoveProdMaintHandlers(sender.Graph);
            UpdateConfigurationFromKey(sender.Graph, row, row.KeyID);

            //Try to "Finish" the configuration...
            var locatedConfig = sender.Graph.Caches[typeof(AMConfigurationResults)].Locate(row) as AMConfigurationResults;
            if (locatedConfig != null)
            {
                string errorMessage;
                bool documentValid = IsDocumentValid(out errorMessage);

                if (documentValid)
                {
                    locatedConfig.Completed = true;
                    Update(locatedConfig);
                    return;
                }

                if (!sender.Graph.IsImport && !sender.Graph.IsContractBasedAPI)
                {
                    sender.RaiseExceptionHandling<AMConfigurationResults.keyID>(row, row.KeyID,
                        new PXSetPropertyException(Messages.ConfigurationNeedsAttention, PXErrorLevel.Warning, errorMessage));
                }
            }
        }

        protected virtual void SOLine_AMConfigKeyID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (!typeof(SOOrderEntry).IsAssignableFrom(sender.Graph.GetType()))
            {
                return;
            }

            var row = (SOLine)e.Row;
            if (row?.InventoryID == null || string.IsNullOrWhiteSpace((string)e.NewValue) || sender.Graph.IsImport || sender.Graph.IsContractBasedAPI)
            {
                return;
            }
            var rowExt = row.GetExtension<CROpportunityProductsExt>();
            if (string.IsNullOrWhiteSpace(rowExt?.AMConfigKeyID) || string.IsNullOrWhiteSpace(rowExt?.AMConfigurationID))
            {
                return;
            }

            if (((SOOrderEntry)sender.Graph).Transactions.Ask(
                    Messages.ConfirmConfigKeyChange,
                    Messages.ConfirmConfigKeyChangeContinue,
                    MessageButtons.YesNo) != WebDialogResult.Yes)
            {
                e.Cancel = true;
                e.NewValue = rowExt.AMConfigKeyID;
            }
        }

        protected virtual void SOLine_AMConfigKeyID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (SOLine) e.Row;
            if (row == null)
            {
                return;
            }
            var rowExt = row.GetExtension<SOLineExt>();
            if (rowExt == null)
            {
                return;
            }

            AMConfigurationResults currentConfig = PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                        And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
                >.Select(sender.Graph, row.OrderType, row.OrderNbr, row.LineNbr);

            if (currentConfig == null)
            {
                return;
            }

            // Set of current required for rules processing to correctly process during change of values on attributes and options.
            this.Current = currentConfig;

            if (currentConfig.KeyID == rowExt.AMConfigKeyID)
            {
                return;
            }

            if (sender.Graph is SOOrderEntry)
            {
                ConfigSupplementalItemsHelper.RemoveSOSupplementalLineItems((SOOrderEntry) sender.Graph, currentConfig);
            }

            UpdateConfigurationFromKey(sender.Graph, currentConfig, rowExt.AMConfigKeyID);

            //Try to "Finish" the configuration...
            var locatedConfig = sender.Graph.Caches[typeof(AMConfigurationResults)].Locate(currentConfig) as AMConfigurationResults;
            if (locatedConfig != null)
            {
                string errorMessage;
                bool documentValid = IsDocumentValid(out errorMessage);

                if (documentValid)
                {
                    locatedConfig.Completed = true;
                    sender.Graph.Caches[typeof(AMConfigurationResults)].Update(locatedConfig);

                    UpdateSalesOrderWithConfiguredLineChanges((SOOrderEntry)_Graph, row, locatedConfig, true, true);

                    return;
                }

                if (!sender.Graph.IsImport && !sender.Graph.IsContractBasedAPI)
                {
                    sender.RaiseExceptionHandling<SOLineExt.aMConfigKeyID>(row, rowExt.AMConfigKeyID,
                        new PXSetPropertyException(Messages.ConfigurationNeedsAttention, PXErrorLevel.Warning, errorMessage));
                }
            }
        }

        // Sync changes from SOLine to AMConfiguraitonResults
        protected virtual void SOLine_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            var row = (SOLine) e.Row;
            var rowExt = row?.GetExtension<SOLineExt>();
            if (rowExt == null || string.IsNullOrWhiteSpace(rowExt.AMConfigurationID))
            {
                return;
            }

            if (cache.ObjectsEqual<SOLine.orderQty, 
                SOLine.uOM, 
                SOLine.customerID, 
                SOLine.siteID>(e.OldRow, e.Row))
            {
                return;
            }

            AMConfigurationResults configResults = PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                    And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
            >.Select(cache.Graph, row.OrderType, row.OrderNbr, row.LineNbr);

            if (configResults == null || configResults.IsOpportunityReferenced.GetValueOrDefault())
            {
                return;
            }

            configResults.Qty = row.OrderQty.GetValueOrDefault();
            configResults.UOM = row.UOM;
            configResults.CustomerID = row.CustomerID;
            configResults.SiteID = row.SiteID;

            if (cache.Graph is SOOrderEntry && ((SOOrderEntry)cache.Graph).Document?.Current != null )
            {
                configResults.CustomerLocationID = ((SOOrderEntry) cache.Graph).Document?.Current.CustomerLocationID;
            }

            Update(configResults);
        }

        // Sync changes from CROpportunityProducts to AMConfigurationResults
        protected virtual void CROpportunityProducts_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            var row = (CROpportunityProducts)e.Row;
            var rowExt = row?.GetExtension<CROpportunityProductsExt>();
            if (rowExt == null || string.IsNullOrWhiteSpace(rowExt.AMConfigurationID))
            {
                return;
            }

            if (cache.ObjectsEqual<CROpportunityProducts.quantity, 
                CROpportunityProducts.uOM,
                CROpportunityProducts.customerID, 
                CROpportunityProducts.siteID>(e.OldRow, e.Row))
            {
                return;
            }

            AMConfigurationResults configResults = PXSelect<
                AMConfigurationResults,
                Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>,
                    And<AMConfigurationResults.opportunityLineNbr, Equal<Required<AMConfigurationResults.opportunityLineNbr>>>>>
                .Select(cache.Graph, row.QuoteID, row.LineNbr);

            if (configResults == null || configResults.IsSalesReferenced.GetValueOrDefault())
            {
                return;
            }

            configResults.Qty = row.Quantity.GetValueOrDefault();
            configResults.UOM = row.UOM;
            configResults.CustomerID = row.CustomerID;
            configResults.SiteID = row.SiteID;

            if (cache.Graph is OpportunityMaint && ((OpportunityMaint)cache.Graph).Opportunity?.Current != null)
            {
                configResults.CustomerLocationID = ((OpportunityMaint)cache.Graph).Opportunity?.Current.LocationID;
            }

            if (cache.Graph is QuoteMaint && ((QuoteMaint)cache.Graph).Quote?.Current != null)
            {
                configResults.CustomerLocationID = ((QuoteMaint)cache.Graph).Quote?.Current.LocationID;
            }

            Update(configResults);
        }

        // Sync changes from AMProdItem to AMConfiguraitonResults
        protected virtual void AMProdItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            var row = (AMProdItem)e.Row;
            if (row == null)
            {
                return;
            }

            if (cache.ObjectsEqual<AMProdItem.qtytoProd,
                AMProdItem.uOM,
                AMProdItem.customerID,
                AMProdItem.siteID>(e.OldRow, e.Row))
            {
                return;
            }

            AMConfigurationResults configResults = PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.prodOrderType, Equal<Required<AMConfigurationResults.prodOrderType>>,
                    And<AMConfigurationResults.prodOrderNbr, Equal<Required<AMConfigurationResults.prodOrderNbr>>>>
            >.Select(cache.Graph, row.OrderType, row.ProdOrdID);

            if (configResults == null || 
                configResults.IsOpportunityReferenced.GetValueOrDefault() || 
                configResults.IsSalesReferenced.GetValueOrDefault())
            {
                return;
            }

            configResults.Qty = row.QtytoProd.GetValueOrDefault();
            configResults.UOM = row.UOM;
            configResults.CustomerID = row.CustomerID;
            configResults.SiteID = row.SiteID;

            Update(configResults);
        }

        protected virtual void CROpportunityProducts_AMConfigKeyID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (!typeof(OpportunityMaint).IsAssignableFrom(sender.Graph.GetType()))
            {
                return;
            }

            var row = (CROpportunityProducts)e.Row;
            if (row?.InventoryID == null || string.IsNullOrWhiteSpace((string)e.NewValue) || sender.Graph.IsImport || sender.Graph.IsContractBasedAPI)
            {
                return;
            }
            var rowExt = row.GetExtension<CROpportunityProductsExt>();
            if (string.IsNullOrWhiteSpace(rowExt?.AMConfigKeyID) || string.IsNullOrWhiteSpace(rowExt?.AMConfigurationID))
            {
                return;
            }

            if (((OpportunityMaint)sender.Graph).Opportunity.Ask(
                     Messages.ConfirmConfigKeyChange,
                     Messages.ConfirmConfigKeyChangeContinue,
                     MessageButtons.YesNo) != WebDialogResult.Yes)
            {
                e.Cancel = true;
                e.NewValue = rowExt?.AMConfigKeyID;
            }
        }

        protected virtual void CROpportunityProducts_AMConfigKeyID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (CROpportunityProducts)e.Row;
            var rowExt = row?.GetExtension<CROpportunityProductsExt>();
            if (rowExt == null)
            {
                return;
            }

            AMConfigurationResults currentConfig = PXSelect<
                AMConfigurationResults,
                Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>,
                    And<AMConfigurationResults.opportunityLineNbr, Equal<Required<AMConfigurationResults.opportunityLineNbr>>>>>
                .Select(sender.Graph, row.QuoteID, row.LineNbr);

            if (currentConfig == null)
            {
                return;
            }

            // Set of current required for rules processing to correctly process during change of values on attributes and options.
            this.Current = currentConfig;

            if (currentConfig.KeyID == rowExt.AMConfigKeyID)
            {
                return;
            }

            ConfigSupplementalItemsHelper.RemoveProductSupplementalLineItems(sender.Graph, currentConfig);

            UpdateConfigurationFromKey(sender.Graph, currentConfig, rowExt.AMConfigKeyID);

            //Try to "Finish" the configuration...
            var locatedConfig = sender.Graph.Caches[typeof(AMConfigurationResults)].Locate(currentConfig) as AMConfigurationResults;
            if (locatedConfig != null)
            {
                string errorMessage;
                bool documentValid = IsDocumentValid(out errorMessage);

                if (documentValid)
                {
                    locatedConfig.Completed = true;
                    sender.Graph.Caches[typeof(AMConfigurationResults)].Update(locatedConfig);

                    if (_Graph is OpportunityMaint)
                    {
                        UpdateOpportunityWithConfiguredLineChanges((OpportunityMaint)_Graph, row, locatedConfig, true);
                        return;
                    }

                    if (_Graph is QuoteMaint)
                    {
                        UpdateQuoteWithConfiguredLineChanges((QuoteMaint)_Graph, row, locatedConfig, true);
                    }

                    return;
                }

                sender.RaiseExceptionHandling<CROpportunityProductsExt.aMConfigKeyID>(row, rowExt.AMConfigKeyID,
                    new PXSetPropertyException(Messages.ConfigurationNeedsAttention, PXErrorLevel.Warning, errorMessage));
            }
        }

        public virtual void CROpportunity_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            SetConfigurationStatus(sender, (CROpportunity)e.Row);
        }

        public virtual void CRQuote_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            SetConfigurationStatus(sender, (CRQuote)e.Row);
        }

        public virtual void SOOrder_Completed_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetConfigurationStatus(cache, (SOOrder)e.Row);
        }

        public virtual void SOOrder_Cancelled_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetConfigurationStatus(cache, (SOOrder)e.Row);
        }

        protected virtual void SetConfigurationStatus(PXCache cache, SOOrder order)
        {
            if(cache == null || order == null)
            {
                return;
            }

            foreach (AMConfigurationResults configResult in PXSelectReadonly<AMConfigurationResults,
                    Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>>>
                    >.Select(cache.Graph, order.OrderType, order.OrderNbr))
            {
                SetConfigurationStatus(cache, order, configResult);
            }
        }

        protected virtual void SetConfigurationStatus(PXCache cache, SOOrder order, AMConfigurationResults configurationResults)
        {
            if (cache == null || order == null || configurationResults == null)
            {
                return;
            }

            bool isClosed = order.Completed.GetValueOrDefault() || order.Cancelled.GetValueOrDefault();
            if (configurationResults.Closed.GetValueOrDefault() != isClosed)
            {
                configurationResults.Closed = isClosed;
                this.Update(configurationResults);
            }
        }

        protected virtual void SetConfigurationStatus(PXCache cache, CROpportunity order)
        {
            if (cache == null || order == null)
            {
                return;
            }

            if (PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.salesQuotes>())
            {
                foreach (PXResult<AMConfigurationResults, CRQuote> result in PXSelectJoin<AMConfigurationResults,
                    LeftJoin<CRQuote, On<AMConfigurationResults.opportunityQuoteID, Equal<CRQuote.quoteID>>>,
                    Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>>>.Select(cache.Graph, order.QuoteNoteID))
                {
                    SetConfigurationStatus(cache, order, (CRQuote)result, (AMConfigurationResults)result);
                }
            }
            else
            {
                foreach (AMConfigurationResults configResult in PXSelectReadonly<
                    AMConfigurationResults,
                    Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>>>
                    .Select(cache.Graph, order.QuoteNoteID))
                {
                    SetConfigurationStatus(cache, order, configResult);
                }
            }
        }

        protected virtual void SetConfigurationStatus(PXCache cache, CROpportunity order, CRQuote quote, AMConfigurationResults configurationResults)
        {
            if (cache == null || configurationResults == null)
            {
                return;
            }

            var isClosed = (order != null && order.IsActive != true) || (quote != null && quote.IsDisabled == true);
            if (configurationResults.Closed.GetValueOrDefault() != isClosed)
            {
                configurationResults.Closed = isClosed;
                this.Update(configurationResults);
            }
        }

        protected virtual void SetConfigurationStatus(PXCache cache, CROpportunity order, AMConfigurationResults configurationResults)
        {
            if (cache == null || order == null || configurationResults == null)
            {
                return;
            }

            var isClosed = order.IsActive != true;
            if (configurationResults.Closed.GetValueOrDefault() != isClosed)
            {
                configurationResults.Closed = isClosed;
                this.Update(configurationResults);
            }
        }

        protected virtual void SetConfigurationStatus(PXCache cache, CRQuote quote)
        {
            if (cache == null || quote == null)
            {
                return;
            }

            foreach (AMConfigurationResults configResult in PXSelectReadonly<
                AMConfigurationResults,
                Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>>>
                .Select(cache.Graph, quote.QuoteID))
            {
                SetConfigurationStatus(cache, quote, configResult);
            }
        }

        protected virtual void SetConfigurationStatus(PXCache cache, CRQuote quote, AMConfigurationResults configurationResults)
        {
            if (cache == null || quote == null || configurationResults == null)
            {
                return;
            }

            bool isClosed = quote.IsDisabled.GetValueOrDefault();
            if (configurationResults.Closed.GetValueOrDefault() != isClosed)
            {
                configurationResults.Closed = isClosed;
                this.Update(configurationResults);
            }
        }

        protected virtual void AMConfigurationResults_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            var row = (AMConfigurationResults)e.Row;
            var prodItem = _Graph.GetCacheCurrent<AMProdItem>();
            var soOrder = _Graph.GetCacheCurrent<SOOrder>();
            var soLine = _Graph.GetCacheCurrent<SOLine>();
            var crOpportunity = _Graph.GetCacheCurrent<CROpportunity>();
            var crQuote = _Graph.GetCacheCurrent<CRQuote>();
            var crOpportunityProduct = _Graph.GetCacheCurrent<CROpportunityProducts>();
            var configuration = this.Configuration.Current;

            if (prodItem == null && (soOrder == null || soLine == null) && (crOpportunity == null || crOpportunityProduct == null))
            {
                e.Cancel = true;
                return;
            }

            if (soOrder?.Current != null && soLine?.Current != null)
            {
                // InventoryID and these other fields get set in SOOrderEntryAMExtension.InsertConfigurationResult. Really no need to set here but just in case.
                //  The problem with updating here is in Import scenarios as the soLine.Current is not actually the current line... it is the previous inserted line.
                if (row.InventoryID == null)
                {
                    row.InventoryID = soLine.Current.InventoryID;
                    row.UOM = soLine.Current.UOM;
                    row.Qty = soLine.Current.Qty;
                    row.SiteID = soLine.Current.SiteID;
                }

                //For pricing
                row.CustomerID = soOrder.Current.CustomerID;
                row.CustomerLocationID = soOrder.Current.CustomerLocationID;

                row.DocDate = soOrder.Current.OrderDate;

                if (row.Revision == null)
                {
                    throw new PXException(Messages.NotActiveRevision);
                }
                return;
            }

            if (prodItem?.Current != null)
            {
                row.InventoryID = prodItem.Current.InventoryID;
                row.UOM = prodItem.Current.UOM;
                row.Qty = prodItem.Current.QtytoProd;
                row.SiteID = prodItem.Current.SiteID;

                //For pricing
                row.CustomerID = prodItem.Current.CustomerID;
                cache.SetDefaultExt<AMConfigurationResults.customerLocationID>(row);
                row.DocDate = prodItem.Current.ProdDate;

                if (row.Revision == null)
                {
                    throw new PXException(Messages.NotActiveRevision);
                }
                return;
            }

            if (crOpportunity?.Current != null)
            {
                row.Qty = 0m;
                //For pricing
                row.CustomerID = crOpportunity.Current.BAccountID;
                row.CustomerLocationID = crOpportunity.Current.LocationID;

                if (crOpportunityProduct?.Current != null)
                {
                    row.InventoryID = crOpportunityProduct.Current.InventoryID;
                    row.UOM = crOpportunityProduct.Current.UOM;
                    row.Qty = crOpportunityProduct.Current.Qty;
                    row.SiteID = crOpportunityProduct.Current.SiteID;
                }

                if (row.Revision == null)
                {
                    throw new PXException(Messages.NotActiveRevision);
                }
                return;
            }

            if (crQuote?.Current != null)
            {
                row.Qty = 0m;
                //For pricing
                row.CustomerID = crQuote.Current.BAccountID;
                row.CustomerLocationID = crQuote.Current.LocationID;

                if (crOpportunityProduct?.Current != null)
                {
                    row.InventoryID = crOpportunityProduct.Current.InventoryID;
                    row.UOM = crOpportunityProduct.Current.UOM;
                    row.Qty = crOpportunityProduct.Current.Qty;
                    row.SiteID = crOpportunityProduct.Current.SiteID;
                }

                if (row.Revision == null)
                {
                    throw new PXException(Messages.NotActiveRevision);
                }
                return;
            }

            if (configuration != null)
            {
                row.ConfigurationID = configuration.ConfigurationID;
                row.Revision = configuration.Revision;
                row.InventoryID = configuration.InventoryID;
            }
            
            row.IsConfigurationTesting = true;
        }

        protected virtual void AMConfigurationResults_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (!IsCopyMode)
            {
                CreateResultsTemplate(cache, (AMConfigurationResults)e.Row);
            }
        }

        protected virtual void AMConfigurationResults_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            var row = (AMConfigurationResults) e.Row;
            if (row == null || !Features.ProductConfiguratorEnabled() ||
                cache.ObjectsEqual<AMConfigurationResults.customerID, AMConfigurationResults.customerLocationID, AMConfigurationResults.docDate>(e.OldRow, row))
            {
                return;
            }

            UpdateConfigResultPrice(row);
        }

        public static void UpdateSalesOrderWithConfiguredLineChanges(PXGraph graph, SOLine soLine, AMConfigurationResults configResults)
        {
            UpdateSalesOrderWithConfiguredLineChanges(graph, soLine, configResults, true);
        }

        internal static void UpdateSalesOrderWithConfiguredLineChanges(PXGraph graph, SOLine soLine, AMConfigurationResults configResults, bool refreshView)
        {
            if (soLine == null)
            {
                return;
            }

            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            if (graph is SOOrderEntry)
            {
                UpdateSalesOrderWithConfiguredLineChanges((SOOrderEntry)graph, soLine, configResults, refreshView, true);
                return;
            }

            var soOrderEntryGraph = PXGraph.CreateInstance<SOOrderEntry>();
            soOrderEntryGraph.Document.Current = soOrderEntryGraph.Document.Search<SOOrder.orderNbr>(soLine.OrderNbr, soLine.OrderType);
            UpdateSalesOrderWithConfiguredLineChanges(soOrderEntryGraph, soLine, configResults, refreshView, false);

            if (soOrderEntryGraph.IsDirty)
            {
                soOrderEntryGraph.RecalculateExternalTaxesSync = true;
                soOrderEntryGraph.Actions.PressSave();
            }
        }

        internal static void UpdateSalesOrderWithConfiguredLineChanges(SOOrderEntry soOrderEntryGraph, SOLine soLine, AMConfigurationResults configResults, bool refreshView, bool callFromSoGraph)
        {
            UpdateSalesOrderWithConfiguredLineChanges(soOrderEntryGraph, soLine, configResults,
                ConfigSupplementalItemsHelper.GetSupplementalOptions(soOrderEntryGraph, configResults), refreshView,
                callFromSoGraph);
        }

        internal static void UpdateSalesOrderWithConfiguredLineChanges(SOOrderEntry soOrderEntryGraph, SOLine soLine, AMConfigurationResults configResults, PXResultset<AMConfigResultsOption> configResultOptions, bool refreshView, bool callFromSoGraph)
        {
            if (soLine == null)
            {
                return;
            }

            if (configResults?.ConfigurationID == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            SOOrderEntryAMExtension.RefreshConfiguredSalesLine(soOrderEntryGraph, soLine, configResults, callFromSoGraph);

            ConfigSupplementalItemsHelper.AddSupplementalLineItems(soLine, soOrderEntryGraph, configResultOptions);

            if (callFromSoGraph && refreshView && ConfigSupplementalItemsHelper.ContainsNewSupplementalLines(soOrderEntryGraph))
            {
                // Makes sure new lines show up in UI before user clicks save
                soOrderEntryGraph.Transactions.View.RequestRefresh();
            }
        }

        protected virtual void UpdateOpportunityWithConfiguredLineChanges(PXGraph graph, CROpportunityProducts product, AMConfigurationResults configResults)
        {
            if (product == null)
            {
                return;
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            if (graph is OpportunityMaint)
            {
                UpdateOpportunityWithConfiguredLineChanges((OpportunityMaint)graph, product, configResults, true);
                return;
            }

            var opportunityGraph = PXGraph.CreateInstance<OpportunityMaint>();
            opportunityGraph.Opportunity.Current = opportunityGraph.Opportunity.Search<CROpportunity.quoteNoteID>(configResults.OpportunityQuoteID);
            UpdateOpportunityWithConfiguredLineChanges(opportunityGraph, product, configResults, false);

            if (opportunityGraph.IsDirty)
            {
                // Using persist over Actions.PressSave because that call will fail when using External Tax Providers
                opportunityGraph.Persist();
            }
        }

        protected virtual void UpdateOpportunityWithConfiguredLineChanges(OpportunityMaint opportunityGraph, CROpportunityProducts product, AMConfigurationResults configResults, bool callFromOppGraph)
        {
            UpdateOpportunityWithConfiguredLineChangesInt(opportunityGraph, product, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(opportunityGraph, configResults), callFromOppGraph);
        }
        
        internal static void UpdateOpportunityWithConfiguredLineChangesInt(OpportunityMaint opportunityGraph, CROpportunityProducts product, AMConfigurationResults configResults, PXResultset<AMConfigResultsOption> configResultOptions, bool callFromOppGraph)
        {
            if (product == null)
            {
                return;
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            OpportunityMaintAMExtension.RefreshConfiguredOpportunityProduct(opportunityGraph, product, configResults, callFromOppGraph);

            ConfigSupplementalItemsHelper.AddSupplementalProductLineItems(product, opportunityGraph, configResultOptions);

            if (callFromOppGraph && ConfigSupplementalItemsHelper.ContainsNewSupplementalLines(opportunityGraph))
            {
                // Makes sure new lines show up in UI before user clicks save
                opportunityGraph.GetExtension<OpportunityMaintAMExtension>()?.Products.View.RequestRefresh();
            }
        }

        public static void UpdateQuoteWithConfiguredLineChanges(PXGraph graph, CROpportunityProducts product, AMConfigurationResults configResults)
        {
            if (product == null)
            {
                return;
            }

            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            if (graph is QuoteMaint)
            {
                UpdateQuoteWithConfiguredLineChanges((QuoteMaint)graph, product, configResults, true);
                return;
            }

            var quoteGraph = PXGraph.CreateInstance<QuoteMaint>();
            CRQuote quote = PXSelect<CRQuote, Where<CRQuote.quoteID, Equal<Required<CRQuote.quoteID>>>>.Select(quoteGraph, configResults.OpportunityQuoteID);
            if (quote?.QuoteID == null)
            {
                PXTrace.WriteError(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(CRQuote)));
                return;
            }

            quoteGraph.Quote.Current = quote;
            UpdateQuoteWithConfiguredLineChanges(quoteGraph, product, configResults, false);

            if (quoteGraph != null && quoteGraph.IsDirty)
            {
                // Using persist over Actions.PressSave because that call will fail when using External Tax Providers
                quoteGraph.Persist();
            }
        }
        
        public static void UpdateQuoteWithConfiguredLineChanges(QuoteMaint quoteGraph, CROpportunityProducts product, AMConfigurationResults configResults, bool callFromQuoteGraph)
        {
            UpdateQuoteWithConfiguredLineChanges(quoteGraph, product, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(quoteGraph, configResults), callFromQuoteGraph);
        }

        internal static void UpdateQuoteWithConfiguredLineChanges(QuoteMaint quoteGraph, CROpportunityProducts product, AMConfigurationResults configResults, PXResultset<AMConfigResultsOption> configResultOptions, bool callFromQuoteGraph)
        {
            if (product == null)
            {
                return;
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }


            QuoteMaintAMExtension.RefreshConfiguredQuoteProduct(quoteGraph, product, configResults, callFromQuoteGraph);

            ConfigSupplementalItemsHelper.AddSupplementalProductLineItems(product, quoteGraph, configResultOptions);

            if (callFromQuoteGraph && ConfigSupplementalItemsHelper.ContainsNewSupplementalLines(quoteGraph))
            {
                // Makes sure new lines show up in UI before user clicks save
                quoteGraph.GetExtension<QuoteMaintAMExtension>()?.Products.View.RequestRefresh();
            }
        }

        protected virtual void AMConfigurationResults_Revision_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var row = (AMConfigurationResults)e.Row;
            if (row != null)
            {
                var activeRevision = (AMConfiguration)PXSelect<AMConfiguration,
                                                        Where<AMConfiguration.configurationID,
                                                            Equal<Required<AMConfiguration.configurationID>>,
                                                        And<AMConfiguration.status,
                                                            Equal<ConfigRevisionStatus.active>>>>.Select(cache.Graph, row.ConfigurationID);
                e.NewValue = activeRevision?.Revision;
            }
        }

        protected virtual void AMConfigurationResults_CuryFixedPriceTotal_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMConfigurationResults)e.Row;
            if (row?.InventoryID == null || row.CuryInfoID == null)
            {
                e.NewValue = 0m;
                return;
            }
            CurrencyInfo ci = null;

            //3111 for some reason when the currency is edited on an opportunity the curyinfo record is stored in the extensions.currencyinfo cache 
            //and not the regular one. Check for inserted extensions.currencyinfo before looking up record in the DB
            var ciExt = (PX.Objects.CM.Extensions.CurrencyInfo)_Graph.Caches<PX.Objects.CM.Extensions.CurrencyInfo>().Current;
            if (ciExt != null)
            {
                ci = ciExt.GetCM();
            }

            if (ci == null)
            {
                ci = currencyinfo.Select();
            }

            if (ci == null)
            {
                return;
            }

            e.NewValue = CalculateSalesPrice(_Graph, sender, GetCustomerPriceClass(), row.CustomerID, row.InventoryID, row.SiteID, ci, row.UOM, row.Qty, row.DocDate.GetValueOrDefault(Common.Dates.Today), 0m).GetValueOrDefault();
        }

        protected virtual void AMConfigurationResults_Completed_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMConfigurationResults) e.Row;
            if (row == null)
            {
                return;
            }

            if (!row.Completed.GetValueOrDefault())
            {
                return;
            }

            ConfigKeyFormulaEngine.ProcessKeys(sender, row, row.IsConfigurationTesting.GetValueOrDefault());
            if (row.IsConfigurationTesting.GetValueOrDefault())
            {
                TraceConfigurationValues(sender, (AMConfigurationResults)sender.Locate(row) ?? row);
            }
        }

        /// <summary>
        /// Print selective configuration data to Trace window. Example of use: running configuration entry in test mode
        /// </summary>
        /// <param name="configResults"></param>
        protected virtual void TraceConfigurationValues(PXCache sender, AMConfigurationResults configResults)
        {
            if (string.IsNullOrWhiteSpace(configResults?.ConfigurationID))
            {
                return;
            }

            var sb = new System.Text.StringBuilder().AppendLine($"Test Configuration '{configResults.ConfigurationID.TrimIfNotNullEmpty()}' '{configResults.Revision.TrimIfNotNullEmpty()}' Data:");

            //Keys
            sb.AppendLine($"{PXUIFieldAttribute.GetDisplayName<AMConfigurationResults.keyID>(sender)} = \"{configResults.KeyID.TrimIfNotNullEmpty()}\"");
            sb.AppendLine($"{PXUIFieldAttribute.GetDisplayName<AMConfigurationResults.keyDescription>(sender)} = \"{configResults.KeyDescription.TrimIfNotNullEmpty()}\"");
            sb.AppendLine($"{PXUIFieldAttribute.GetDisplayName<AMConfigurationResults.tranDescription>(sender)} = \"{configResults.TranDescription.TrimIfNotNullEmpty()}\"");

            PXTrace.WriteInformation(sb.ToString());
        }

        protected virtual List<AMBomMatl> GetConfigurationBomMaterial(AMConfigurationResults configurationResults)
        {
            if (configurationResults == null)
            {
                return null;
            }

            return GetConfigurationBomMaterial(_Graph, 
                configurationResults.ConfigurationID, 
                configurationResults.Revision);
        }

        /// <summary>
        /// Get a given configurations set of BOM material
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="configurationiID">Configuration ID</param>
        /// <param name="revision">Configuration revision</param>
        /// <returns></returns>
        public static List<AMBomMatl> GetConfigurationBomMaterial(PXGraph graph, string configurationiID, string revision)
        {
            return PXSelectJoin<
                AMBomMatl,
                InnerJoin<AMBomOper, 
                    On<AMBomMatl.bOMID, Equal<AMBomOper.bOMID>,
                    And<AMBomMatl.revisionID, Equal<AMBomOper.revisionID>>>,
                InnerJoin<AMConfiguration,
                    On<AMConfiguration.bOMID, Equal<AMBomMatl.bOMID>,
                    And<AMConfiguration.bOMRevisionID, Equal<AMBomMatl.revisionID>>>>>,
                Where2<
                    Where<AMBomMatl.expDate, IsNull,
                        Or<AMBomMatl.expDate, GreaterEqual<Today>>>,
                    And<AMConfiguration.configurationID, Equal<Required<AMConfiguration.configurationID>>,
                    And<AMConfiguration.revision, Equal<Required<AMConfiguration.revision>>>>>,
                OrderBy<
                    Asc<AMBomOper.operationCD, 
                    Asc<AMBomMatl.sortOrder, 
                    Asc<AMBomMatl.lineID>>>>>
                .Select(graph, configurationiID, revision)
                .FirstTableItems
                .ToList();
        }

        protected virtual void AMConfigurationResults_CuryBOMPriceTotal_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMConfigurationResults) e.Row;
            if (row == null)
            {
                e.NewValue = 0m;
                return;
            }

            CurrencyInfo curInfo = currencyinfo.Select();
            if (curInfo == null)
            {
                return;
            }

            var total = 0m;
            foreach (var bomMatl in GetConfigurationBomMaterial(row))
            {
                if (bomMatl.InventoryID == null)
                {
                    continue;
                }

                InventoryItem item = PXSelect<
                        InventoryItem,
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .SelectWindowed(_Graph, 0, 1, bomMatl.InventoryID);
                try
                {
                    var itemExt = item?.GetExtension<InventoryItemExt>();

                    var qtyRequired = bomMatl.QtyReq.GetValueOrDefault() * (1 + bomMatl.ScrapFactor.GetValueOrDefault()) *
                            (bomMatl.BatchSize.GetValueOrDefault() == 0m ? 1m
                            : 1m / bomMatl.BatchSize.GetValueOrDefault());

                    qtyRequired = itemExt?.AMQtyRoundUp == true ? Math.Ceiling(qtyRequired) : qtyRequired;

                    var unitPrice = CalculateSalesPrice(_Graph, sender, GetCustomerPriceClass(), row.CustomerID, bomMatl.InventoryID, bomMatl.SiteID ?? row.SiteID, curInfo, bomMatl.UOM, qtyRequired, row.DocDate.GetValueOrDefault(Common.Dates.Today), 0m).GetValueOrDefault();
#if DEBUG
                    if (item != null)
                    {
                        AMDebug.TraceWriteMethodName($"{bomMatl.BOMID};{bomMatl.RevisionID};({bomMatl.OperationID});{bomMatl.LineID};{item.InventoryCD.TrimIfNotNullEmpty()} - Unit Price = {unitPrice} [Total = {unitPrice * qtyRequired}]; DocDate = {row.DocDate}");
                    }
#endif
                    total += unitPrice * qtyRequired;
                }
                catch (Exception exception)
                {
                    PXException priceException = null;
                    if (!string.IsNullOrWhiteSpace(bomMatl?.BOMID))
                    {
                        var oper = (AMBomOper)PXSelect<AMBomOper,
                            Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                                And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
                                    And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>
                                >>>.SelectWindowed(_Graph, 0, 1, bomMatl.BOMID, bomMatl.RevisionID, bomMatl.OperationID);

                        priceException = new PXException(exception,
                            Messages.ErrorCalculatingConfigPriceFromBomMatl,
                            bomMatl.BOMID,
                            bomMatl.RevisionID,
                            oper?.OperationCD,
                            bomMatl.LineID,
                            item == null ? string.Empty : $" {PXUIFieldAttribute.GetDisplayName<InventoryItem.inventoryID>(_Graph.Caches<InventoryItem>())} {item.InventoryCD.TrimIfNotNullEmpty()}");
                    }

                    if (priceException != null && exception is NullReferenceException)
                    {
                        throw priceException;
                    }

                    PXTraceHelper.PxTraceException(priceException);
                    throw;
                }
            }

            e.NewValue = total;
        }      

        protected virtual void AMConfigResultsOption_Qty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMConfigResultsOption)e.Row;
            if (row.QtyRequired.HasValue)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"Changing Qty from {row.Qty.GetValueOrDefault(-99)} to {row.QtyRequired.GetValueOrDefault()} [Feature {row.FeatureLineNbr.GetValueOrDefault()}, Option {row.OptionLineNbr.GetValueOrDefault()}]");
#endif
                e.NewValue = row.QtyRequired;
            }
        }

        protected virtual void AMConfigResultsOption_UnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (((AMConfigResultsOption)e.Row).InventoryID == null)
            {
                e.NewValue = 0m;
            }
        }

        protected virtual void AMConfigResultsOption_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = GetCuryUnitPriceDefaultValue(sender, (AMConfigResultsOption)e.Row, Current);
        }

        protected virtual decimal GetCuryUnitPriceDefaultValue(PXCache cache, AMConfigResultsOption configResultOption, AMConfigurationResults configResult)
        {
            if (configResultOption == null || configResult == null || configResultOption?.InventoryID == null ||
                configResultOption.UOM == null || !configResultOption.Included.GetValueOrDefault())
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"[{configResultOption?.ConfigResultsID}][{configResultOption?.ConfigurationID}:{configResultOption?.Revision}:{configResultOption?.FeatureLineNbr}:{configResultOption?.OptionLineNbr}] CuryUnitPrice = 0.00 (old CuryUnitPrice = {configResultOption?.CuryUnitPrice})");
#endif
                return 0m;
            }

            CurrencyInfo curInfo = currencyinfo.Select();
            if (curInfo == null)
            {
                return 0m;
            }

            var curyUnitPrice = CalculateSalesPrice(_Graph, cache, GetCustomerPriceClass(), configResult.CustomerID, configResultOption.InventoryID, configResult.SiteID, curInfo, configResultOption.UOM, configResultOption.Qty, configResult.DocDate.GetValueOrDefault(Common.Dates.Today), 0m).GetValueOrDefault();
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{configResultOption.ConfigResultsID}][{configResultOption.ConfigurationID}:{configResultOption.Revision}:{configResultOption.FeatureLineNbr}:{configResultOption.OptionLineNbr}] CuryUnitPrice = {curyUnitPrice} (old CuryUnitPrice = {configResultOption.CuryUnitPrice}); DocDate = {configResult.DocDate}");
#endif
            return curyUnitPrice;
        }

        protected virtual void UpdateConfigResultPrice(AMConfigurationResults configResult)
        {
            ResetConfigResultsOptionUnitPrice(configResult);
            Cache.SetDefaultExt<AMConfigurationResults.curyBOMPriceTotal>(Cache.LocateElse(configResult));
        }

        protected virtual void ResetConfigResultsOptionUnitPrice(AMConfigurationResults configResult)
        {
            foreach (AMConfigResultsOption configResultOption in PXSelect<AMConfigResultsOption,
            Where<AMConfigResultsOption.configResultsID, Equal<Required<AMConfigurationResults.configResultsID>>>>.Select(_Graph, configResult?.ConfigResultsID))
            {
                var newUnitPrice = GetCuryUnitPriceDefaultValue(ResultOptions.Cache, configResultOption, configResult);
                if (newUnitPrice == configResultOption.CuryUnitPrice.GetValueOrDefault())
                {
                    continue;
                }

                configResultOption.CuryUnitPrice = newUnitPrice;
                ResultOptions.Update(configResultOption);
            }
        }

        public virtual string GetCustomerPriceClass()
        {
            Location c = PXSelect<Location,
                Where<Location.bAccountID,
                    Equal<Current<AMConfigurationResults.customerID>>,
                    And<Location.locationID,
                        Equal<Optional<AMConfigurationResults.customerLocationID>>>>>.Select(_Graph);
            if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
            {
                return c.CPriceClassID;
            }

            return ARPriceClass.EmptyPriceClass;
        }

        protected static decimal? CalculateSalesPrice(
            PXGraph graph,
            PXCache cache,
            string customerPriceClass,
            int? customerID,
            int? inventoryID,
            CurrencyInfo currencyinfo,
            string UOM,
            decimal? quantity,
            DateTime date,
            decimal? currentUnitPrice)
        {
            return CalculateSalesPrice(graph, cache, customerPriceClass, customerID, inventoryID, currencyinfo, UOM, quantity, date, currentUnitPrice, true);
        }

        protected static decimal? CalculateSalesPrice(
            PXGraph graph,
            PXCache cache,
            string customerPriceClass,
            int? customerID,
            int? inventoryID,
            int? siteID,
            CurrencyInfo currencyinfo,
            string UOM,
            decimal? quantity,
            DateTime date,
            decimal? currentUnitPrice)
        {
            return CalculateSalesPrice(graph, cache, customerPriceClass, customerID, inventoryID, siteID, currencyinfo, UOM, quantity, date, currentUnitPrice, true);
        }

        protected static decimal? CalculateSalesPrice(
            PXGraph graph,
            PXCache cache,
            string customerPriceClass,
            int? customerID,
            int? inventoryID,
            CurrencyInfo currencyinfo,
            string UOM,
            decimal? quantity,
            DateTime date,
            decimal? currentUnitPrice,
            bool useBaseCall)
        {
            return CalculateSalesPrice(graph, cache, customerPriceClass, customerID, inventoryID, null, currencyinfo, UOM, quantity, date, currentUnitPrice, true);
        }

        protected static decimal? CalculateSalesPrice(
            PXGraph graph,
            PXCache cache,
            string customerPriceClass,
            int? customerID,
            int? inventoryID,
            int? siteID,
            CurrencyInfo currencyinfo,
            string UOM,
            decimal? quantity,
            DateTime date,
            decimal? currentUnitPrice,
            bool useBaseCall)
        {
            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (cache == null)
            {
                throw new PXArgumentException(nameof(cache));
            }

            if (currencyinfo == null)
            {
                throw new PXArgumentException(nameof(currencyinfo));
            }

            if (inventoryID == null)
            {
                return null;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(UOM))
                {
                    return CalculateSalesPrice(graph, cache, inventoryID, currencyinfo, UOM);
                }

                if (useBaseCall)
                {
                    return ARSalesPriceMaintAMExtension.BaseCalculateSalesPrice(
                        cache,
                        customerPriceClass,
                        customerID,
                        inventoryID,
                        siteID,
                        currencyinfo,
                        UOM,
                        quantity.GetValueOrDefault(),
                        date,
                        currentUnitPrice);
                }

                return ARSalesPriceMaint.CalculateSalesPrice(
                    cache,
                    customerPriceClass,
                    customerID,
                    inventoryID,
                    siteID,
                    currencyinfo,
                    UOM,
                    quantity.GetValueOrDefault(),
                    date,
                    currentUnitPrice);
            }
            catch (Exception exception)
            {
#if DEBUG
                AMDebug.TraceExceptionMethodName(exception);
#endif
                PXTrace.WriteError(exception);
                if (inventoryID.GetValueOrDefault() != 0)
                {
                    InventoryItem item = PXSelect<
                        InventoryItem, 
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .SelectWindowed(graph, 0, 1, inventoryID);

                    if (item != null)
                    {
                        throw new PXException(Messages.CalculateSalesPriceForItemError,
                            item?.InventoryCD.TrimIfNotNullEmpty(),
                            exception.Message);
                    }
                }

                throw new PXException(Messages.CalculateSalesPriceError, exception.Message);
            }
        }

        /// <summary>
        /// Calculate sales price without customer information
        /// </summary>
        /// <returns></returns>
        protected static decimal? CalculateSalesPrice(
            PXGraph graph,
            PXCache cache,
            int? inventoryID,
            CurrencyInfo currencyinfo,
            string uom)
        {
            return CalculateSalesPrice(graph, cache,
                PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(graph, inventoryID),
                currencyinfo,
                uom);
        }

        protected static decimal? CalculateSalesPrice(
            PXGraph graph,
            PXCache cache,
            InventoryItem inventoryItem,
            CurrencyInfo currencyinfo,
            string uom)
        {
            if (graph == null || cache == null || inventoryItem == null)
            {
                return null;
            }

            var returnPrice = inventoryItem.BasePrice.GetValueOrDefault();
            if (returnPrice == 0 || string.IsNullOrWhiteSpace(uom) || inventoryItem.BaseUnit.EqualsWithTrim(uom))
            {
                return GetCuryValueFromBase(graph, currencyinfo, returnPrice);
            }

            var uom2 = string.IsNullOrWhiteSpace(uom) ? inventoryItem.BaseUnit : uom;

            if (UomHelper.TryConvertFromBaseCost<InventoryItem.inventoryID>(cache, inventoryItem, uom2, returnPrice,
                out var uomPrice))
            {
                returnPrice = uomPrice.GetValueOrDefault();
            }

            return GetCuryValueFromBase(graph, currencyinfo, returnPrice);
        }

        protected static decimal? GetCuryValueFromBase(PXGraph graph, CurrencyInfo currencyinfo, decimal? price)
        {
            if (currencyinfo?.CuryID == null || currencyinfo.BaseCuryID == currencyinfo.CuryID)
            {
                return price;
            }

            return CurrencyHelper.ConvertFromBaseCury(graph, currencyinfo, price);
        }

        protected virtual void AMConfigResultsOption_Included_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMConfigResultsOption)e.Row;
            var newValue = (bool?)e.NewValue;
            if (row.Included != newValue)
            {
                string message;
#pragma warning disable PX1044 // Changing PXCache is prohibited in this event handler
#if DEBUG
                // PX1044 skip ok per Dmitry 12/2/2019
#endif
                if (!RunRuleFormula(row, newValue, out message))
#pragma warning restore PX1044
                {
                    e.NewValue = row.Included;
                    e.Cancel = true;
                    throw new PXSetPropertyException(message, PXErrorLevel.Error);
                }
            }
        }

        protected virtual void AMConfigResultsOption_Included_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMConfigResultsOption)e.Row;
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{row.ConfigResultsID}][{row.ConfigurationID}:{row.Revision}:{row.FeatureLineNbr}:{row.OptionLineNbr}] Included = {row.Included}");
#endif
            ApplyRuleFormula();
            sender.SetValue<AMConfigResultsOption.manualInclude>(row, row.Included);
        }

        protected virtual void AMConfigResultsAttribute_Value_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMConfigResultsAttribute)e.Row;
            var newValue = e.NewValue?.ToString();
#pragma warning disable PX1044 // Changing PXCache is prohibited in this event handler
#if DEBUG
            // PX1044 skip ok per Dmitry 12/2/2019
#endif
            if (row.Value != newValue && !RunRuleFormula(row, newValue, out var message))
#pragma warning restore PX1044
            {
                e.NewValue = row.Value;
                e.Cancel = true;
                throw new PXSetPropertyException(message, PXErrorLevel.Error);
            }
        }

        protected virtual void AMConfigResultsAttribute_Value_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
#if DEBUG
            AMDebug.TraceWriteMethodName($"L={((AMConfigResultsAttribute)e.Row).AttributeLineNbr} ; V={((AMConfigResultsAttribute)e.Row).Value}");
#endif
            ApplyRuleFormula();
        }

        #endregion

        #region Rule & Formulas

        public void RemoveRulesHandlers()
        {
            if (!_handlersRemoved)
            {
                _handlersRemoved = true;
                _Graph.FieldVerifying.RemoveHandler<AMConfigResultsOption.included>(AMConfigResultsOption_Included_FieldVerifying);
                _Graph.FieldUpdated.RemoveHandler<AMConfigResultsOption.included>(AMConfigResultsOption_Included_FieldUpdated);

                _Graph.FieldVerifying.RemoveHandler<AMConfigResultsAttribute.value>(AMConfigResultsAttribute_Value_FieldVerifying);
                _Graph.FieldUpdated.RemoveHandler<AMConfigResultsAttribute.value>(AMConfigResultsAttribute_Value_FieldUpdated);
            }
        }

        public void AddRulesHandlers()
        {
            if (_handlersRemoved)
            {
                _handlersRemoved = false;
                _Graph.FieldVerifying.AddHandler<AMConfigResultsOption.included>(AMConfigResultsOption_Included_FieldVerifying);
                _Graph.FieldUpdated.AddHandler<AMConfigResultsOption.included>(AMConfigResultsOption_Included_FieldUpdated);

                _Graph.FieldVerifying.AddHandler<AMConfigResultsAttribute.value>(AMConfigResultsAttribute_Value_FieldVerifying);
                _Graph.FieldUpdated.AddHandler<AMConfigResultsAttribute.value>(AMConfigResultsAttribute_Value_FieldUpdated);
            }
        }

        public bool RunRuleFormula(out string message)
        {
            AMDebug.TraceWriteMethodName();
            if (_isCopyMode)
            {
                message = null;
                return true;
            }
            return RunRuleFormula(AMRuleFormulaEngine.Run(_Graph), false, out message);
        }

        public bool RunRuleFormula(AMConfigResultsAttribute row, string newValue, out string message)
        {
            AMDebug.TraceWriteMethodName();
            if (_isCopyMode)
            {
                message = null;
                return true;
            }
            return RunRuleFormula(AMRuleFormulaEngine.Run(_Graph, row, newValue), true, out message);
        }

        public bool RunRuleFormula(AMConfigResultsOption row, bool? newValue, out string message)
        {
            AMDebug.TraceWriteMethodName();
            if (_isCopyMode)
            {
                message = null;
                return true;
            }
            return RunRuleFormula(AMRuleFormulaEngine.Run(_Graph, row, newValue), true, out message);
        }

        private bool RunRuleFormula(RuleReturn ret, bool isTwoStep, out string message)
        {
            if (ret.IsValid)
            {
                message = string.Empty;

                if (isTwoStep)
                    PXContext.SetSlot(ret);
                else
                    ApplyRuleFormula(ret);

                return true;
            }
            message = ret.Message;
            return false;
        }

        public void ApplyRuleFormula()
        {
            ApplyRuleFormula(PXContext.GetSlot<RuleReturn>());
            PXContext.SetSlot<RuleReturn>(null);
        }

        private void ApplyRuleFormula(RuleReturn ret)
        {
            if (ret == null)
            {
                return;
            }

            using (new AMRuleFormulaScope(this))
            {
                // DisableReadItem is required to get the random trip to the database via SQL Select to stop occuring.
                // The trip to sql only occurs with the PXDBDefault to ConfigResultsID but we need that attribute
                ResultOptions.Cache.DisableReadItem = true;
                ResultFeatures.Cache.DisableReadItem = true;
                ResultAttributes.Cache.DisableReadItem = true;
                ResultRules.Cache.DisableReadItem = true;
                
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    foreach (var option in ret.OptionsToUpdate)
                    {
                        // As the collection is not receiving cache updates correctly... lets make sure those fields updated after the collection is loaded are correctly set from the current cache values...
                        var optionCopy = _Graph.Caches<AMConfigResultsOption>().LocateElseCopy(option);
                        option.CuryUnitPrice = optionCopy.CuryUnitPrice.GetValueOrDefault();

                        // The option being updated receives a Select to SQL for this same option
                        ResultOptions.Update(option);
                    }
                    foreach (var feature in ret.FeaturesToUpdate)
                    {
                        // The feature being updated receives a Select to SQL for this same feature
                        ResultFeatures.Update(feature);
                    }
                    foreach (var attribute in ret.AttributesToUpdate)
                    {
                        ResultAttributes.Update(attribute);
                    }
                    foreach (var rule in ret.RulesToUpdate)
                    {
                        // The rule being updated receives a Select to SQL for this same rule
                        ResultRules.Update(rule);
                    }
                }
                finally
                {
                    sw.Stop();
                    var msg1 = Messages.GetLocal(Messages.ApplyRuleFormula,
                        Current?.ConfigurationID, Current?.Revision, Current?.ConfigResultsID);
                    var msg2 = $"{msg1} {PXTraceHelper.RuntimeMessage(sw.Elapsed)}";
                    //We only need this viewable when viewing verbose messages
                    PXTrace.WriteVerbose(msg2);
#if DEBUG
                    AMDebug.TraceWriteMethodName(msg2);
#endif
                }
            }
        }

        #endregion

        #region Validation

        public bool ValidateOption(AMConfigResultsOption row, out string message)
        {
            if (row.RuleValid != true)
            {
                message = Messages.ConflictingRules;
                return false;
            }
            else if (row.Included == true && row.Required == true && row.Included != true)
            {
                message = Messages.RequiredOptionNotIncluded;
                return false;
            }
            else if (row.Included == true && row.MinQty.HasValue && row.MinQty > row.Qty)
            {
                message = Messages.GetLocal(Messages.QtyCannotBeLessThanMinQty, row.Qty, row.MinQty);
                return false;
            }

            else if (row.Included == true && row.MaxQty.HasValue && row.MaxQty < row.Qty)
            {
                message = Messages.GetLocal(Messages.QtyCannotBeGreaterThanMaxQty, row.Qty, row.MaxQty);
                return false;
            }
            else
            {
                message = string.Empty;
                return true;
            }
        }

        public bool ValidateAttribute(AMConfigResultsAttribute attribute, out string message)
        {
            if (attribute.RuleValid != true)
            {
                message = Messages.NotRespectedRules;
                return false;
            }
            else if (attribute.Required == true && string.IsNullOrEmpty(attribute.Value))
            {
                message = Messages.Require;
                return false;
            }
            else
            {
                message = string.Empty;
                return true;
            }
        }

        public bool ValidateFeature(AMConfigResultsFeature row, out string message)
        {
            if (row.MinQty.HasValue && row.MinQty > row.TotalQty)
            {
                message = Messages.GetLocal(Messages.QtyCannotBeLessThanMinQty, row.TotalQty, row.MinQty);
                return false;
            }
            else if (row.MaxQty.HasValue && row.MaxQty < row.TotalQty)
            {
                message = Messages.GetLocal(Messages.QtyCannotBeGreaterThanMaxQty, row.TotalQty, row.MaxQty);
                return false;
            }
            else
            {
                message = string.Empty;
                return true;
            }
        }

        #endregion

        #region Initialization

        protected void CreateResultsTemplate(PXCache sender, AMConfigurationResults resultConfiguration)
        {
            foreach (AMConfiguration config in Configuration.Select())
            {
                RemoveRulesHandlers();

                Configuration.Current = config;

                var item = (InventoryItem)PXSelect<InventoryItem,
                                                Where<InventoryItem.inventoryID,
                                                    Equal<Required<InventoryItem.inventoryID>>>>
                                                .Select(_Graph, config?.InventoryID);
                if (item == null)
                {
                    continue;
                }

                using (var scope = new AMRuleFormulaScope(this))
                {
                    foreach (AMConfigurationAttribute configAttribute in ConfigAttributes.Select())
                    {
                        ConfigAttributes.Current = configAttribute;

                        var resultAttribute = new AMConfigResultsAttribute
                        {
                            AttributeLineNbr = configAttribute.LineNbr,
                            AttributeID = configAttribute.AttributeID,
                            Enabled = configAttribute.Enabled,
                            Visible = configAttribute.Visible,
                            Required = configAttribute.Required,
                            Value = configAttribute.IsFormula == true
                                ? string.Empty
                                : configAttribute.Value,
                            Parent = item.InventoryCD
                        };
                        ResultAttributes.Insert(resultAttribute);
                    }
                        
                    foreach (AMConfigurationFeature configFeature in ConfigFeatures.Select())
                    {
                        ConfigFeatures.Current = configFeature;

                        ResultFeatures.Insert(new AMConfigResultsFeature { FeatureLineNbr = configFeature.LineNbr });

                        foreach (AMConfigurationOption configOption in ConfigOptions.Select())
                        {
                            ConfigOptions.Current = configOption;

                            var resultOption = new AMConfigResultsOption
                            {
                                FeatureLineNbr = configOption.ConfigFeatureLineNbr,
                                OptionLineNbr = configOption.LineNbr,
                                Included = configOption.FixedInclude,
                                FixedInclude = configOption.FixedInclude,
                                InventoryID = configOption.InventoryID,
                                SubItemID = configOption.SubItemID,
                                UOM = configOption.UOM,
                                MaterialType = configOption.MaterialType
                            };
                            ResultOptions.Insert(resultOption);
                        }
                    }

                    foreach (AMConfigurationRule configRule in ConfigRules.Select())
                    {
                        //The rule reference itself (Validation)
                        if (configRule.TargetFeatureLineNbr == null)
                        {
                            InsertResultRule(configRule, configRule.RuleSource, configRule.SourceLineNbr, 0, false);
                        }
                        else
                        {
                            //The rule references multiple options (soft rule)
                            if (configRule.TargetOptionLineNbr == null)
                            {
                                foreach (AMConfigResultsOption option in ResultOptions.Select(resultConfiguration.ConfigResultsID, configRule.TargetFeatureLineNbr))
                                {
                                    InsertResultRule(configRule, RuleTargetSource.Option, configRule.TargetFeatureLineNbr, option.OptionLineNbr, true);
                                }
                            }
                            //The rule references a single option (hard rule)
                            else
                            {
                                InsertResultRule(configRule, RuleTargetSource.Option, configRule.TargetFeatureLineNbr, configRule.TargetOptionLineNbr, false);
                            }
                        }
                    }

                    string formulaError;
                    if (!RunRuleFormula(out formulaError))
                    {
                        throw new PXException(formulaError);
                    }

                    RecalcPricing(sender, resultConfiguration);
                }
            }
        }

        public void RecalcPricing()
        {
            RecalcPricing(Cache, Current);
        }

        protected static void RecalcPricing(PXCache sender, AMConfigurationResults resultConfiguration)
        {
            sender.SetDefaultExt<AMConfigurationResults.fixedPriceTotal>(resultConfiguration);
            sender.SetDefaultExt<AMConfigurationResults.curyFixedPriceTotal>(resultConfiguration);

            sender.SetDefaultExt<AMConfigurationResults.bOMPriceTotal>(resultConfiguration);
            sender.SetDefaultExt<AMConfigurationResults.curyBOMPriceTotal>(resultConfiguration);
        }

        protected void InsertResultRule(AMConfigurationRule configRule, string ruleTarget, int? targetLineNbr, int? targetSubLineNbr, bool isSoftRule)
        {
            ResultRules.Insert(new AMConfigResultsRule
            {
                RuleTarget = ruleTarget,
                TargetLineNbr = targetLineNbr,
                TargetSubLineNbr = targetSubLineNbr,
                RuleSource = configRule.RuleSource,
                RuleSourceLineNbr = configRule.SourceLineNbr,
                RuleLineNbr = configRule.LineNbr,
                RuleType = configRule.RuleType,
                IsSoftRule = isSoftRule,
                Condition = configRule.Condition
            });
        }
        
        public void RemoveSOLineHandlers()
        {
            _Graph.FieldUpdating.RemoveHandler<SOLineExt.aMConfigKeyID>(SOLine_AMConfigKeyID_FieldUpdating);
            _Graph.FieldUpdated.RemoveHandler<SOLineExt.aMConfigKeyID>(SOLine_AMConfigKeyID_FieldUpdated);
        }

        public void AddSOLineHandlers()
        {
            _Graph.FieldUpdating.AddHandler<SOLineExt.aMConfigKeyID>(SOLine_AMConfigKeyID_FieldUpdating);
            _Graph.FieldUpdated.AddHandler<SOLineExt.aMConfigKeyID>(SOLine_AMConfigKeyID_FieldUpdated);
        }

        public void RemoveProductLineHandlers()
        {
            _Graph.FieldUpdating.RemoveHandler<CROpportunityProductsExt.aMConfigKeyID>(CROpportunityProducts_AMConfigKeyID_FieldUpdating);
            _Graph.FieldUpdated.RemoveHandler<CROpportunityProductsExt.aMConfigKeyID>(CROpportunityProducts_AMConfigKeyID_FieldUpdated);
        }

        public void RemoveProdMaintHandlers(PXGraph graph)
        {
            graph.FieldUpdating.RemoveHandler<AMConfigurationResults.keyID>(AMConfigurationResults_KeyID_FieldUpdating);
            graph.FieldUpdated.RemoveHandler<AMConfigurationResults.keyID>(AMConfigurationResults_KeyID_FieldUpdated);
        }

        #endregion


        /// <summary>
        /// Resolve which configuration ID to return as default by looking for IDs set in INItemSite and InventoryItem.
        /// </summary>
        /// <param name="inventoryID">InventoryItem item ID</param>
        /// <param name="siteID">INItemSite site ID</param>
        /// <param name="configurationID"> Configuration ID taken from : Item Warehouse if available,  otherwise returns Stock Item if available.  Null if not found.</param>
        /// <returns> True if a configurationID was found, otherwise false.</returns>
        public bool TryGetDefaultConfigurationID(int? inventoryID, int? siteID, out string configurationID)
        {
            return TryGetDefaultConfigurationID(this._Graph, inventoryID, siteID, out configurationID);
        }

        /// <summary>
        /// Resolve which configuration ID to return as default by looking for IDs set in INItemSite and InventoryItem.
        /// </summary>
        /// <param name="inventoryID">InventoryItem item ID</param>
        /// <param name="siteID">INItemSite site ID</param>
        /// <param name="configurationID"> Configuration ID taken from : Item Warehouse if available,  otherwise returns Stock Item if available.  Null if not found.</param>
        /// <returns> True if a configurationID was found, otherwise false.</returns>
        public static bool TryGetDefaultConfigurationID(PXGraph graph, int? inventoryID, int? siteID, out string configurationID)
        {
            configurationID = null;

            if (!Features.ProductConfiguratorEnabled() || inventoryID == null)
            {
                return false;
            }

            configurationID = ConfigurationIDManager.GetID(graph, inventoryID, siteID);

            return !string.IsNullOrEmpty(configurationID);
        }

        /// <summary>
        /// Return default configuration ID set on Stock Item.
        /// </summary>
        /// <param name="inventoryID">InventoryItem item ID</param>
        /// <param name="configurationID"> Configuration ID taken from Stock Item if available, otherwise null.</param>
        /// <returns> True if a configurationID was found, otherwise false.</returns>
        public bool TryGetDefaultConfigurationID(int? inventoryID, out string configurationID)
        {
            return TryGetDefaultConfigurationID(inventoryID, null, out configurationID);
        }

        /// <summary>
        /// Return default configuration ID set on Stock Item.
        /// </summary>
        /// <param name="inventoryID">InventoryItem item ID</param>
        /// <param name="configurationID"> Configuration ID taken from Stock Item if available, otherwise null.</param>
        /// <returns> True if a configurationID was found, otherwise false.</returns>
        public bool TryGetDefaultConfigurationID(PXGraph graph, int? inventoryID, out string configurationID)
        {
            return TryGetDefaultConfigurationID(graph, inventoryID, null, out configurationID);
        }

        /// <summary>
        /// Clear the cache configuration Results 
        /// </summary>
        public void ClearConfigurationCache()
        {
            Cache.Clear();
            ResultAttributes.Cache.Clear();
            ResultFeatures.Cache.Clear();
            ResultOptions.Cache.Clear();
            ResultRules.Cache.Clear();
        }
    }
}
