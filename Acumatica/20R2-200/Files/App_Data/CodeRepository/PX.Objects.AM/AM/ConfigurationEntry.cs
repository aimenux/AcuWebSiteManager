using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Web.UI;
using System;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.AM
{
    public class ConfigurationEntry : PXGraph<ConfigurationEntry>
    {
        public PXSave<AMConfigurationResults> Save;
        public PXCancel<AMConfigurationResults> Cancel;
        public PXSaveClose<AMConfigurationResults> SaveClose;

        #region Views

        public ConfigurationSelect Results;

        public PXSelect<AMConfigurationResults,
            Where<AMConfigurationResults.configResultsID,
                Equal<Current<AMConfigurationResults.configResultsID>>>> CurrentResults;

        public PXSelectJoin<AMConfigResultsAttribute,
            InnerJoin<AMConfigurationAttribute,
                On<AMConfigResultsAttribute.configurationID,
                    Equal<AMConfigurationAttribute.configurationID>,
                    And<AMConfigResultsAttribute.revision,
                        Equal<AMConfigurationAttribute.revision>,
                        And<AMConfigResultsAttribute.attributeLineNbr,
                            Equal<AMConfigurationAttribute.lineNbr>>>>>,
            Where<AMConfigResultsAttribute.configResultsID,
                Equal<Current<AMConfigurationResults.configResultsID>>,
                And<AMConfigResultsAttribute.visible, Equal<True>>>,
            OrderBy<Asc<AMConfigResultsAttribute.configurationID,
                Asc<AMConfigResultsAttribute.revision,
                Asc<AMConfigurationAttribute.sortOrder,
                Asc<AMConfigResultsAttribute.attributeLineNbr>>>>>> Attributes;

        public PXSelect<AMConfigTreeNode,
            Where<AMConfigTreeNode.lineNbr,
                Equal<Argument<int?>>,
                And<AMConfigTreeNode.optionLineNbr,
                    Equal<Argument<int?>>>>,
            OrderBy<Asc<AMConfigTreeNode.sortOrder>>> Features;
        public virtual IEnumerable features([PXInt]int? lineNbr, [PXInt]int? optionLineNbr)
        {
            if (lineNbr == null)
            {
                // Get features related to current config.
                var results = PXSelectJoin<AMConfigResultsFeature,
                    InnerJoin<AMConfigurationFeature,
                        On<AMConfigResultsFeature.configurationID,
                            Equal<AMConfigurationFeature.configurationID>,
                            And<AMConfigResultsFeature.revision,
                                Equal<AMConfigurationFeature.revision>,
                                And<AMConfigResultsFeature.featureLineNbr,
                                    Equal<AMConfigurationFeature.lineNbr>>>>>,
                    Where<AMConfigurationFeature.visible,
                        Equal<True>,
                        And<AMConfigResultsFeature.configResultsID,
                            Equal<Current<AMConfigurationResults.configResultsID>>>>>.Select(this);

                // Set the icon based on completed status.
                foreach (PXResult<AMConfigResultsFeature, AMConfigurationFeature> feature in results)
                {
                    var resultFeature = (AMConfigResultsFeature)feature;
                    var configurationFeature = (AMConfigurationFeature)feature;

                    var item = new AMConfigTreeNode();
                    item.LineNbr = configurationFeature.LineNbr;
                    item.Label = configurationFeature.Label;
                    item.SortOrder = configurationFeature.SortOrder;

                    string errorMessage;
                    var valid = Results.IsFeatureOptionValid(resultFeature, out errorMessage, false);
                    if (valid)
                    {
                        item.ToolTip = string.Empty;
                        item.Icon = Sprite.Main.GetFullUrl(Sprite.Main.Success);
                    }
                    else
                    {
                        item.ToolTip = errorMessage;
                        item.Icon = Sprite.Main.GetFullUrl(Sprite.Main.Fail);
                    }

                    yield return item;
                }
            }
            else if (optionLineNbr == null)
            {
                // Feature LineNbr is set, but not option. This is a feature node. Get related Options.
                var options = OptionsTree.Select(lineNbr);

                var result = new PXResultset<AMConfigTreeNode>();
                foreach (PXResult<AMConfigResultsOption, AMConfigurationOption> option in options)
                {
                    var resultOption = (AMConfigResultsOption)option;
                    var configOption = (AMConfigurationOption)option;
                    AMConfigTreeNode item = new AMConfigTreeNode();
                    item.LineNbr = resultOption.FeatureLineNbr;
                    item.OptionLineNbr = resultOption.OptionLineNbr;
                    item.Label = configOption.Label;

                    string errorMessage;
                    var valid = Results.ValidateOption(resultOption, out errorMessage);
                    if (valid)
                    {
                        item.ToolTip = string.Empty;
                        item.Icon = Sprite.Control.GetFullUrl(Sprite.Control.Info);
                    }
                    else
                    {
                        item.ToolTip = errorMessage;
                        item.Icon = Sprite.Control.GetFullUrl(Sprite.Control.Error);
                    }

                    yield return item;
                }
            }
        }

        public PXSelect<AMConfigResultsFeature,
            Where<AMConfigResultsFeature.configResultsID,
                Equal<Current<AMConfigurationResults.configResultsID>>,
                And<AMConfigResultsFeature.featureLineNbr,
                    Equal<Argument<int?>>>>> CurrentFeature;
        public virtual IEnumerable currentFeature([PXInt]int? lineNbr)
        {
            return PXSelect<AMConfigResultsFeature,
                Where<AMConfigResultsFeature.configResultsID,
                    Equal<Current<AMConfigurationResults.configResultsID>>,
                    And<AMConfigResultsFeature.featureLineNbr,
                        Equal<Required<AMConfigResultsFeature.featureLineNbr>>>>>.Select(this, lineNbr);
        }

        public PXSelectJoin<AMConfigResultsOption,
            InnerJoin<AMConfigurationOption,
                On<AMConfigResultsOption.configurationID,
                    Equal<AMConfigurationOption.configurationID>,
                    And<AMConfigResultsOption.revision,
                        Equal<AMConfigurationOption.revision>,
                        And<AMConfigResultsOption.featureLineNbr,
                            Equal<AMConfigurationOption.configFeatureLineNbr>,
                            And<AMConfigResultsOption.optionLineNbr,
                                Equal<AMConfigurationOption.lineNbr>>>>>>,
            Where<AMConfigResultsOption.configResultsID,
                Equal<Current<AMConfigResultsFeature.configResultsID>>,
                And<AMConfigResultsOption.featureLineNbr,
                    Equal<Optional<AMConfigResultsFeature.featureLineNbr>>,
                    And<Where2<
                        Where<Current<SelectOptionsFilter.showAll>,
                            Equal<True>>,
                        Or<AMConfigResultsOption.available,
                            Equal<True>>>>>>,
            OrderBy<Asc<AMConfigResultsOption.configurationID,
                Asc<AMConfigResultsOption.revision,
                Asc<AMConfigResultsOption.featureLineNbr,
                Asc<AMConfigurationOption.sortOrder,
                Asc<AMConfigResultsOption.optionLineNbr>>>>>>> Options;

        public PXSelectJoin<AMConfigResultsOption,
            InnerJoin<AMConfigurationOption,
                On<AMConfigResultsOption.configurationID,
                    Equal<AMConfigurationOption.configurationID>,
                    And<AMConfigResultsOption.revision,
                        Equal<AMConfigurationOption.revision>,
                        And<AMConfigResultsOption.featureLineNbr,
                            Equal<AMConfigurationOption.configFeatureLineNbr>,
                            And<AMConfigResultsOption.optionLineNbr,
                                Equal<AMConfigurationOption.lineNbr>>>>>>,
            Where<AMConfigResultsOption.configResultsID,
                Equal<Current<AMConfigResultsFeature.configResultsID>>,
                And<AMConfigResultsOption.featureLineNbr,
                    Equal<Optional<AMConfigResultsFeature.featureLineNbr>>,
                    And<Where2<
                        Where<AMConfigResultsOption.included,
                            Equal<True>>,
                        Or<AMConfigResultsOption.ruleValid,
                            Equal<False>>>>>>> OptionsTree;

        public PXSelect<AMConfigResultsOption,
            Where<AMConfigResultsOption.configResultsID,
                Equal<Optional<AMConfigResultsOption.configResultsID>>,
                And<AMConfigResultsOption.featureLineNbr,
                    Equal<Optional<AMConfigResultsOption.featureLineNbr>>,
                    And<AMConfigResultsOption.optionLineNbr,
                        Equal<Optional<AMConfigResultsOption.optionLineNbr>>>>>> CurrentOption;

        public PXFilter<ConfigEntryFilter> ConfigFilter;

        #region Select Option SmartPanel

        public PXFilter<SelectOptionsFilter> OptionsSelectFilter;

        #endregion

        #region Multi-Currency

        public ToggleCurrency<AMConfigurationResults> CurrencyView;

        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<AMConfigurationResults.curyInfoID>>>> currencyinfo;

        #endregion

        #region External Refs


        public PXSelect<AMProdItem,
            Where<AMProdItem.orderType,
                Equal<Current<AMConfigurationResults.prodOrderType>>,
                And<AMProdItem.prodOrdID,
                    Equal<Current<AMConfigurationResults.prodOrderNbr>>>>> ProdItemRef;
        #endregion

        #endregion

        /// <summary>
        /// Is the processing initializing a configuration
        /// </summary>
        internal bool IsInitConfiguration;

        public ConfigurationEntry()
        {
            Results.AllowDelete =

                Options.AllowInsert =
                    Options.AllowDelete =

                        Attributes.AllowInsert =
                            Attributes.AllowDelete = false;
        }

        //Avoid prompt to save when in test mode
        public override bool IsDirty
        {
            get
            {
                if (Results?.Current?.IsConfigurationTesting == true)
                {
                    return false;
                }

                return base.IsDirty;
            }
        }

        #region Actions

        public PXAction<AMConfigurationResults> Finish;
        [PXUIField(DisplayName = Messages.Finish)]
        [PXButton(CommitChanges = true)]
        protected virtual IEnumerable finish(PXAdapter a)
        {
            AMConfigurationResults results = Results.Current;
            if (results == null
                || results.Closed.GetValueOrDefault())
            {
                return a.Get();
            }

            if (results.Completed != true)
            {
                string errorMessage;
                bool documentValid = Results.IsDocumentValid(out errorMessage);

                if (!documentValid)
                {
                    throw new PXException(errorMessage);
                }

                results.Completed = true;
            }
            else
            {
                // Remove Supplemental line items 
                ConfigSupplementalItemsHelper.RemoveSupplementalLineItems(this, results);

                results.Completed = false;
            }

            results = Results.Update(results);
            if (results != null && results.IsConfigurationTesting != true)
            {
                var retSave = Save.Press(a);

                if (ConfigFilter?.Current?.ShowCanTestPersist == true)
                {
                    // implemented this way to get over a refresh issue when saving a test configuration
                    return retSave;
                }

                AMProdItem prodItem = ProdItemRef.Select();
                if (prodItem != null && prodItem.StatusID != ProductionOrderStatus.Planned)
                {
                    throw new PXException(Messages.ProductionAlreadyPlanned);
                }
                if (prodItem != null)
                {
                    var prodGraph = CreateInstance<ProdMaint>();

                    prodItem.BuildProductionBom = true;
                    prodGraph.ProdMaintRecords.Current = prodGraph.ProdMaintRecords.Update(prodItem);
                    prodGraph.ItemConfiguration.Current = prodGraph.ItemConfiguration.Select();
                    // Need to pass in the current results so later the correct value for Completed is found
                    prodGraph.ItemConfiguration.Current = results;
                    prodGraph.Actions.PressSave();
                }

                return retSave;
            }

            return a.Get();
        }

        public PXAction<AMConfigurationResults> ShowAll;
        [PXUIField(DisplayName = "Show All")]
        [PXButton]
        protected virtual void showAll()
        {
            OptionsSelectFilter.Current.ShowAll = !OptionsSelectFilter.Current.ShowAll.GetValueOrDefault();
        }

        #endregion

        #region Handlers

        protected virtual void AMConfigurationResults_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMConfigurationResults)e.Row;
            if (row == null)
            {
                return;
            }

            AMProdItem prodItem = ProdItemRef.Select();
            var canFinish = prodItem == null || prodItem.StatusID == ProductionOrderStatus.Planned;

            // All header records not allowed for update/insert in UI
            PXUIFieldAttribute.SetEnabled(sender, row, false);

            var isTestConfig = row.IsConfigurationTesting.GetValueOrDefault() ||
                               ConfigFilter?.Current?.ShowCanTestPersist == true;
            if (isTestConfig)
            {
                PXUIFieldAttribute.SetEnabled<AMConfigurationResults.customerID>(sender, row, true);
                PXUIFieldAttribute.SetVisible<AMConfigurationResults.customerID>(sender, row, true);
                PXUIFieldAttribute.SetEnabled<AMConfigurationResults.isConfigurationTesting>(sender, row, true);
                PXUIFieldAttribute.SetVisible<AMConfigurationResults.isConfigurationTesting>(sender, row, true);
                PXUIFieldAttribute.SetEnabled<AMConfigurationResults.siteID>(sender, row, true);
                PXUIFieldAttribute.SetVisible<AMConfigurationResults.siteID>(sender, row, true);
            }

            PXUIFieldAttribute.SetVisible<AMConfigResultsOption.inventoryID>(Options.Cache, null, isTestConfig);
            PXUIFieldAttribute.SetVisible<AMConfigResultsOption.subItemID>(Options.Cache, null, isTestConfig);

            Options.AllowUpdate =
                Results.AllowUpdate =
                    Attributes.AllowUpdate = !row.Closed.GetValueOrDefault() && !row.Completed.GetValueOrDefault() && canFinish;

            Finish.SetEnabled(!row.Closed.GetValueOrDefault() && canFinish);
            Finish.SetCaption(row.Completed != true ? Messages.Finish : Messages.Unfinish);

            Save.SetEnabled(!row.IsConfigurationTesting.GetValueOrDefault());
            // There is no way to bring the configuration back when testing so disable
            Cancel.SetEnabled(!isTestConfig);
            SaveClose.SetCaption(!isTestConfig ? Messages.SaveAndClose : Messages.CloseTesting);
            ShowAll.SetCaption(OptionsSelectFilter.Current.ShowAll.GetValueOrDefault() ? PX.Objects.CA.Messages.HideTran : Messages.ShowAll);
        }

        protected virtual void AMConfigResultsAttribute_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMConfigResultsAttribute)e.Row;
            if (row == null || IsInitConfiguration)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMConfigResultsAttribute.value>(sender, row, row.Enabled == true);
            sender.RaiseExceptionHandling<AMConfigResultsAttribute.value>(row, row.Value, RuleSummary.GetException(this, row, this.Results, CurrentResults?.Current?.IsConfigurationTesting == true));
        }

        protected virtual void AMConfigResultsAttribute_Value_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Options.View.Clear();
            Attributes.View.RequestRefresh();
        }

        protected virtual void AMConfigResultsOption_Included_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            OptionsTree.View.Clear();
            Options.View.RequestRefresh();
        }

        protected virtual void SelectOptionsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (SelectOptionsFilter)e.Row;
            if (row == null) return;

            PXUIFieldAttribute.SetVisible<AMConfigResultsOption.available>(Options.Cache, null, row.ShowAll.GetValueOrDefault());
        }

        protected virtual void AMConfigResultsOption_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMConfigResultsOption)e.Row;
            if (row == null || IsInitConfiguration)
            {
                return;
            }

            // Only shown if Show All is checked
            PXUIFieldAttribute.SetEnabled<AMConfigResultsOption.selected>(sender, row, row.Available == true && row.Included != true);

            var option = (AMConfigurationOption)PXSelect<AMConfigurationOption,
                Where<AMConfigurationOption.configurationID,
                    Equal<Current<AMConfigResultsFeature.configurationID>>,
                    And<AMConfigurationOption.revision,
                        Equal<Current<AMConfigResultsFeature.revision>>,
                        And<AMConfigurationOption.configFeatureLineNbr,
                            Equal<Current<AMConfigResultsFeature.featureLineNbr>>,
                            And<AMConfigurationOption.lineNbr,
                                Equal<Required<AMConfigurationOption.lineNbr>>>>>>>.Select(this, row.OptionLineNbr);
            if(option != null)
            {
                PXUIFieldAttribute.SetEnabled<AMConfigResultsOption.qty>(sender, row, option.QtyEnabled == true);
            }

            PXUIFieldAttribute.SetEnabled<AMConfigResultsOption.included>(sender, row, !row.FixedInclude.GetValueOrDefault());

            //We are doing it in row selected instead of field verifying because the value of row.Qty could be calculated
            //and we want to warn the user if it's not respecting the rules without removing the qty. He could then adjust 
            //the attributes so it fits the rules. 
            sender.RaiseExceptionHandling<AMConfigResultsOption.qty>(row, row.Qty, RuleSummary.GetException(this, row, this.Results, CurrentResults.Current.IsConfigurationTesting == true));
        }

        protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CurrencyInfo info = e.Row as CurrencyInfo;
            if (info != null)
            {
                bool curyenabled = info.AllowUpdate(this.Results.Cache);
                Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<AMConfigurationResults.customerID>>>>.Select(this);
                if (customer != null && !(bool)customer.AllowOverrideRate)
                {
                    curyenabled = false;
                }

                PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
                PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
                PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
                PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
            }
        }

        public override void Persist()
        {
            var currentConfig = CurrentResults?.Current;
            if (currentConfig == null || currentConfig.IsConfigurationTesting == true)
            {
                return;
            }

            var status = CurrentResults.Cache.GetStatus(currentConfig);
            if (currentConfig.Completed.GetValueOrDefault() && (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated))
            {
                if (currentConfig?.IsSalesReferenced == true)
                {
                    var soGraph = UpdateSalesOrderWithConfiguredLineChanges(currentConfig);
                    if (soGraph != null && soGraph.IsDirty)
                    {
                        using (var ts = new PXTransactionScope())
                        {
                            soGraph.Actions.PressSave();
                            base.Persist();
                            ts.Complete();
                        }
                        return;
                    }
                }

                if (currentConfig?.IsOpportunityReferenced == true)
                {
                    var crGraph = UpdateOpportunityWithConfiguredLineChanges(currentConfig);
                    if (crGraph != null && crGraph.IsDirty)
                    {
                        using (var ts = new PXTransactionScope())
                        {
                            // Using persist over Actions.PressSave because that call will fail when using External Tax Providers
                            crGraph.Persist();
                            base.Persist();
                            ts.Complete();
                        }

                        return;
                    }
                }
            }

            base.Persist();
        }

        // Copy over from ConfigurationSelect - UpdateSalesOrderWithConfiguredLineChanges
        protected virtual SOOrderEntry UpdateSalesOrderWithConfiguredLineChanges(AMConfigurationResults configResults)
        {
            if (configResults == null)
            {
                return null;
            }

            var soLine = (SOLine)PXSelect<SOLine,
                Where<SOLine.orderType, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                    And<SOLine.orderNbr, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                        And<SOLine.lineNbr, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
            >.Select(this, configResults.OrdTypeRef, configResults.OrdNbrRef, configResults.OrdLineRef);

            if (soLine?.OrderNbr == null)
            {
                return null;
            }

            var soOrderEntryGraph = CreateInstance<SOOrderEntry>();
            soOrderEntryGraph.RecalculateExternalTaxesSync = true;
            soOrderEntryGraph.Document.Current = soOrderEntryGraph.Document.Search<SOOrder.orderNbr>(soLine.OrderNbr, soLine.OrderType);
            if (soOrderEntryGraph.Document?.Current == null)
            {
                return null;
            }

            //Need to set the config into cache for correct results when query later
            soOrderEntryGraph.Caches<AMConfigurationResults>().Update(PXCache<AMConfigurationResults>.CreateCopy(configResults));
            soOrderEntryGraph.Caches<AMConfigurationResults>().SetStatus(configResults, PXEntryStatus.Notchanged);

            ConfigurationSelect.UpdateSalesOrderWithConfiguredLineChanges(soOrderEntryGraph, soLine, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(this, configResults), false, false);

            return soOrderEntryGraph;
        }

        // Copy over from ConfigurationSelect - UpdateOpportunityWithConfiguredLineChanges
        protected virtual PXGraph UpdateOpportunityWithConfiguredLineChanges(AMConfigurationResults configResults)
        {
            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            var result = (PXResult<CROpportunityProducts, CRQuote>)PXSelectJoin<CROpportunityProducts,
                LeftJoin<CRQuote, On<CRQuote.quoteID, Equal<CROpportunityProducts.quoteID>>>,
                Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunityProducts.quoteID>>,
                    And<CROpportunityProducts.lineNbr, Equal<Required<CROpportunityProducts.lineNbr>>>>
            >.SelectWindowed(this, 0, 1, configResults.OpportunityQuoteID, configResults.OpportunityLineNbr);

            var product = (CROpportunityProducts) result;
            if (product?.QuoteID == null)
            {
                return null;
            }

            var quote = (CRQuote) result;
            if (string.IsNullOrWhiteSpace(quote?.QuoteNbr))
            {
                //return ConfigurationSelect.UpdateOpportunityWithConfiguredLineChangesInt(product, configResults);
                var opportunityGraph = PXGraph.CreateInstance<OpportunityMaint>();
                opportunityGraph.Opportunity.Current = opportunityGraph.Opportunity.Search<CROpportunity.quoteNoteID>(configResults.OpportunityQuoteID);

                //Need to set the config into cache for correct results when query later
                opportunityGraph.Caches<AMConfigurationResults>().Update(PXCache<AMConfigurationResults>.CreateCopy(configResults));
                opportunityGraph.Caches<AMConfigurationResults>().SetStatus(configResults, PXEntryStatus.Notchanged);

                ConfigurationSelect.UpdateOpportunityWithConfiguredLineChangesInt(opportunityGraph, product, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(this, configResults), false);
                return opportunityGraph;
            }

            if (quote.IsDisabled.GetValueOrDefault())
            {
                return null;
            }

            var quoteGraph = PXGraph.CreateInstance<QuoteMaint>();
            quoteGraph.Quote.Current = quote;

            //Need to set the config into cache for correct results when query later
            quoteGraph.Caches<AMConfigurationResults>().Update(PXCache<AMConfigurationResults>.CreateCopy(configResults));
            quoteGraph.Caches<AMConfigurationResults>().SetStatus(configResults, PXEntryStatus.Notchanged);

            ConfigurationSelect.UpdateQuoteWithConfiguredLineChanges(quoteGraph, product, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(this, configResults), false);

            return quoteGraph;
        }
        #endregion

        #region Unbound Dacs

        [Serializable]
        [PXHidden]
        public class SelectOptionsFilter : IBqlTable
        {
            #region ShowAll
            public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = Messages.ShowAll)]
            public virtual Boolean? ShowAll { get; set; }
            #endregion
        }


        [Serializable]
        [PXHidden]
        public class ConfigEntryFilter : IBqlTable
        {
            #region CanTestPersist
            public abstract class canTestPersist : PX.Data.BQL.BqlBool.Field<canTestPersist> { }
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = "Save Test Results")]
            public virtual Boolean? ShowCanTestPersist { get; set; }
            #endregion
        }
        #endregion

    }
}