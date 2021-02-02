using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.AM
{
    public class ConfigurationMaint : PXRevisionableGraph<ConfigurationMaint, AMConfiguration, AMConfiguration.configurationID, AMConfiguration.revision>
    {
        #region Views

        public PXSelect<AMConfiguration,
                    Where<AMConfiguration.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfiguration.revision,
                        Equal<Current<AMConfiguration.revision>>>>> SelectedConfiguration;

        [PXImport(typeof(AMConfiguration))]
        public PXSelect<AMConfigurationFeature,
                    Where<AMConfigurationFeature.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfigurationFeature.revision,
                        Equal<Current<AMConfiguration.revision>>>>,
                    OrderBy<Asc<AMConfigurationFeature.sortOrder>>> ConfigurationFeatures;

        [PXImport(typeof(AMConfiguration))]
        public PXSelect<AMConfigurationOption,
                    Where<AMConfigurationOption.configurationID,
                        Equal<Current<AMConfigurationFeature.configurationID>>,
                    And<AMConfigurationOption.revision,
                        Equal<Current<AMConfigurationFeature.revision>>,
                    And<AMConfigurationOption.configFeatureLineNbr,
                        Equal<Current<AMConfigurationFeature.lineNbr>>>>>,
                    OrderBy<Asc<AMConfigurationOption.configurationID,
                            Asc<AMConfigurationOption.revision,
                            Asc<AMConfigurationOption.configFeatureLineNbr,
                            Asc<AMConfigurationOption.sortOrder,
                            Asc<AMConfigurationOption.lineNbr>>>>>>> FeatureOptions;

        [PXImport(typeof(AMConfiguration))]
        public PXSelect<AMConfigurationFeatureRule,
                    Where<AMConfigurationFeatureRule.configurationID,
                        Equal<Current<AMConfigurationFeature.configurationID>>,
                    And<AMConfigurationFeatureRule.revision,
                        Equal<Current<AMConfigurationFeature.revision>>,
                    And<AMConfigurationFeatureRule.sourceLineNbr,
                        Equal<Current<AMConfigurationFeature.lineNbr>>>>>> FeatureRules;

        [PXImport(typeof(AMConfiguration))]
        public PXSelect<AMConfigurationAttribute,
                    Where<AMConfigurationAttribute.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfigurationAttribute.revision,
                        Equal<Current<AMConfiguration.revision>>>>,
                    OrderBy<Asc<AMConfigurationAttribute.configurationID,
                            Asc<AMConfigurationAttribute.revision,
                            Asc<AMConfigurationAttribute.sortOrder,
                            Asc<AMConfigurationAttribute.lineNbr>>>>>> ConfigurationAttributes;

        [PXImport(typeof(AMConfiguration))]
        public PXSelect<AMConfigurationAttributeRule,
                    Where<AMConfigurationAttributeRule.configurationID,
                        Equal<Current<AMConfigurationAttribute.configurationID>>,
                    And<AMConfigurationAttributeRule.revision,
                        Equal<Current<AMConfigurationAttribute.revision>>,
                    And<AMConfigurationAttributeRule.sourceLineNbr,
                        Equal<Current<AMConfigurationAttribute.lineNbr>>>>>> AttributeRules;
        
        public PXSetup<AMConfiguratorSetup> ConfiguratorSetup;
        public PXSetup<AMPSetup> ProductionSetup;

        [PXHidden]
        public PXSelectReadonly<AMConfigurationResults,
                    Where<AMConfigurationResults.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfigurationResults.revision,
                        Equal<Current<AMConfiguration.revision>>>>> RelatedResults;

        [PXHidden]
        public PXSelectReadonly<AMConfiguration,
                    Where<AMConfiguration.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfiguration.revision,
                        NotEqual<Current<AMConfiguration.revision>>,
                    And<AMConfiguration.status,
                        Equal<ConfigRevisionStatus.pending>>>>> OtherPendingConfiguration;

        [PXHidden]
        public PXSelectReadonly<AMConfiguration,
                    Where<AMConfiguration.configurationID,
                        Equal<Current<AMConfiguration.configurationID>>,
                    And<AMConfiguration.revision,
                        NotEqual<Current<AMConfiguration.revision>>,
                    And<AMConfiguration.status,
                        Equal<ConfigRevisionStatus.active>>>>> OtherActiveConfiguration;

        // For Configuration ID updates
        [PXHidden]
        public PXSelect<InventoryItem, 
            Where<InventoryItem.inventoryID, Equal<Current<AMConfiguration.inventoryID>>>> ConfiguredInventoryItem;

        // For Configuration ID updates
        [PXHidden]
        public PXSelectJoin<INItemSite, 
            LeftJoin<AMBomItem, 
                On<INItemSite.inventoryID, Equal<AMBomItem.inventoryID>, 
                    And<INItemSite.siteID, Equal<AMBomItem.siteID>>>>,
            Where<INItemSite.inventoryID, Equal<Current<AMConfiguration.inventoryID>>,
                And<AMBomItem.bOMID, Equal<Current<AMConfiguration.bOMID>>,
                And<AMBomItem.revisionID, Equal<Current<AMConfiguration.bOMRevisionID>>>>>> ConfiguredItemSite;

        [PXHidden]
        public PXFilter<ConfigurationIDLevels> ConfigurationIDUpdateFilter;

        #endregion

        public ConfigurationMaint()
        {
            var setup = ConfiguratorSetup.Current;

            var prodSetup = ProductionSetup.Current;
            AMPSetup.CheckSetup(prodSetup);
            
            ActionDropMenu.AddMenuAction(TestConfiguration);
            ActionDropMenu.AddMenuAction(SetAsDefaultForItem);
            ActionDropMenu.AddMenuAction(CopyConfiguration);
            ActionDropMenu.AddMenuAction(SetActiveConfiguration);
        }

        public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);

            //Required to get PXSelect on AMBomItem to work - GetConfigBom() for example
            this.Caches.AddCacheMapping(typeof(AMBomItem), typeof(AMBomItem));
        }

        //Hide copy paste buttons
        public override bool CanClipboardCopyPaste()
        {
            return false;
        }

        //We get field name cannot be empty but no indication to which DAC, so we add this for improved error reporting
        public override int Persist(Type cacheType, PXDBOperation operation)
        {
            try
            {
                return base.Persist(cacheType, operation);
            }
            catch (Exception e)
            {
                PXTrace.WriteError($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#if DEBUG
                AMDebug.TraceWriteMethodName($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#endif
                throw;
            }
        }

        #region Auxiliary methods
        // This function is used to get the Attributes' variable names in the Formula editor.
        public string[] GetAllAttributes()
        {
            return GetAttributeVariables();
        }

        public string[] GetAllButCurrentAttributes()
        {
            if (this.ConfigurationAttributes.Current == null)
                return GetAttributeVariables();
            else
                return GetAttributeVariables(this.ConfigurationAttributes.Current.Variable);
        }

        private string[] GetAttributeVariables(params string[] removeVal)
        {
            return this.ConfigurationAttributes.Select()
                                               .Select(attr => ((AMConfigurationAttribute)attr).Variable)
                                               .Where(var => !string.IsNullOrEmpty(var) && !removeVal.Contains(var))
                                               .Select(var => string.Format("[{0}]", var))
                                               .OrderBy(var => var)
                                               .ToArray();
        }
        #endregion

        #region Revision
        public override bool CanCreateNewRevision(ConfigurationMaint fromGraph, ConfigurationMaint toGraph, string keyValue, string revisionValue, out string error)
        {
#if DEBUG
            AMDebug.TraceWriteMethodName($"key '{keyValue}' rev '{revisionValue}'");
#endif
            error = string.Empty;
            if (fromGraph.Documents.Current.Status != ConfigRevisionStatus.Pending)
            {
                return true;
            }
            else
            {
                error = Messages.GetLocal(Messages.StatCantCreateNextRevision, ConfigRevisionStatus.Desc.Pending);
                return false;
            }
        }

        //Lets get the first operation number from the BOM for legacy data for records existing without OperNbr
        protected virtual AMBomOper GetFirstOperation(AMConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration?.BOMID) || string.IsNullOrWhiteSpace(configuration?.BOMRevisionID))
            {
                return null;
            }

            return PXSelect<
                AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>>
                .SelectWindowed(this, 0, 1, configuration.BOMID, configuration.BOMRevisionID);
        }

        public override AMConfiguration CreateNewRevision(PXCache cache, ConfigurationMaint toGraph, string keyValue, string revisionValue)
        {
            cache.Graph.FieldVerifying.AddHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);
            toGraph.FieldVerifying.AddHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);
            try
            {
                return base.CreateNewRevision(cache, toGraph, keyValue, revisionValue);
            }
            finally
            {
                cache.Graph.FieldVerifying.RemoveHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);
                toGraph.FieldVerifying.RemoveHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);
            }
        }

        protected static AMBomItem GetConfigBom(PXGraph graph, AMConfiguration configuration)
        {
            if (graph == null)
            {
                return null;
            }

            AMBomItem amBomItem = PXSelect<AMBomItem,
                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                    And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>
                    >>>.Select(graph, configuration?.BOMID, configuration?.BOMRevisionID);

            if (amBomItem == null)
            {
                throw new PXException($"{Messages.GetLocal(Messages.InvalidBOM)}: {configuration?.BOMID}; {configuration?.BOMRevisionID}");
            }

            return amBomItem;
        }

        public override void CopyRevision(ConfigurationMaint fromGraph, ConfigurationMaint toGraph, string keyValue, string revisionValue)
        {
            if (fromGraph.Documents.Current == null || toGraph.Documents.Current == null)
            {
                return;
            }

            var amBomItem = GetConfigBom(toGraph, toGraph.Documents.Current);

            if (amBomItem?.Status != AMBomStatus.Active)
            {
                var toCopy = PXCache<AMConfiguration>.CreateCopy(toGraph.Documents.Current);

                toGraph.Documents.Cache.SetDefaultExt<AMConfiguration.bOMRevisionID>(toGraph.Documents.Current);

                if (!string.IsNullOrWhiteSpace(toCopy.BOMRevisionID) &&
                    string.IsNullOrWhiteSpace(toGraph.Documents.Current.BOMRevisionID))
                {
                    // Keep the old value as there is no active revision
                    toGraph.Documents.Cache.SetValue<AMConfiguration.bOMRevisionID>(toGraph.Documents.Current, toCopy.BOMRevisionID);
                    toGraph.Documents.Cache.SetValue<AMConfiguration.inventoryID>(toGraph.Documents.Current, toCopy.InventoryID);

                    toGraph.Documents.Cache.RaiseExceptionHandling<AMConfiguration.bOMRevisionID>(toGraph.Documents.Current, toGraph.Documents.Current.BOMRevisionID,
                        new PXSetPropertyException(Messages.GetLocal(Messages.BomRevisionIsNotActive, toGraph.Documents.Current.BOMID, toGraph.Documents.Current.BOMRevisionID), PXErrorLevel.Warning));
                }
            }

            toGraph.Documents.Cache.SetDefaultExt<AMConfiguration.status>(toGraph.Documents.Current);
            // Set the NoteID to a new Guid to prevent another process has added error
            toGraph.Documents.Current.NoteID = Guid.NewGuid();

            PXNoteAttribute.CopyNoteAndFiles(fromGraph.Documents.Cache, fromGraph.Documents.Current, toGraph.Documents.Cache, toGraph.Documents.Current);

            if (SkipAutoCreateNewRevision())
            {
                return;
            }

            toGraph.FieldUpdated.RemoveHandler<AMConfigurationFeature.featureID>(toGraph.AMConfigurationFeature_FeatureID_FieldUpdated);
            AMBomOper firstOper = null;

            var featureRulesToCopy = new List<AMConfigurationFeatureRule>();
            toGraph.IsCopyPasteContext = true;
            foreach (AMConfigurationFeature feature in fromGraph.ConfigurationFeatures.Select())
            {
                var copyFeature = fromGraph.ConfigurationFeatures.Cache.CreateCopy(feature) as AMConfigurationFeature;
                copyFeature.ConfigurationID = keyValue;
                copyFeature.Revision = revisionValue;

                toGraph.ConfigurationFeatures.Current = toGraph.ConfigurationFeatures.Insert(copyFeature);
                fromGraph.ConfigurationFeatures.Current = feature;
            
                foreach (AMConfigurationOption option in fromGraph.FeatureOptions.Select())
                {
                    var copyOption = fromGraph.FeatureOptions.Cache.CreateCopy(option) as AMConfigurationOption;
                    copyOption.ConfigurationID = keyValue;
                    copyOption.Revision = revisionValue;
                    copyOption = toGraph.FeatureOptions.Insert(copyOption);

                    //Older version allowed for empty OperNbr when it should have been required...
                    if (copyOption.InventoryID != null && copyOption.OperationID == null)
                    {
                        if (firstOper == null && toGraph.Documents.Current != null)
                        {
                            firstOper = GetFirstOperation(toGraph.Documents.Current);
                        }
                        if (firstOper?.OperationID != null)
                        {
                            copyOption.OperationID = firstOper.OperationID;
                            toGraph.FeatureOptions.Update(copyOption);
                        }
                    }
                }

                foreach (AMConfigurationFeatureRule featureRule in fromGraph.FeatureRules.Select())
                {
                    var copyRule = fromGraph.FeatureRules.Cache.CreateCopy(featureRule) as AMConfigurationFeatureRule;
                    copyRule.ConfigurationID = keyValue;
                    copyRule.Revision = revisionValue;

                    featureRulesToCopy.Add(copyRule);
                }
            }
            toGraph.IsCopyPasteContext = false;

            //Rules refer to other features that might not yet be copied 
            //so we wait that all the rules are copied before inserting them
            foreach (var featureRuleToCopy in featureRulesToCopy)
            {
                toGraph.FeatureRules.Insert(featureRuleToCopy);
            }

            toGraph.FieldUpdated.AddHandler<AMConfigurationFeature.featureID>(toGraph.AMConfigurationFeature_FeatureID_FieldUpdated);

            foreach (AMConfigurationAttribute attribute in fromGraph.ConfigurationAttributes.Select())
            {
                var copyAttribute = fromGraph.ConfigurationAttributes.Cache.CreateCopy(attribute) as AMConfigurationAttribute;
                copyAttribute.ConfigurationID = keyValue;
                copyAttribute.Revision = revisionValue;

                toGraph.ConfigurationAttributes.Current = toGraph.ConfigurationAttributes.Insert(copyAttribute);
                fromGraph.ConfigurationAttributes.Current = attribute;

                foreach (AMConfigurationAttributeRule attributeRule in fromGraph.AttributeRules.Select())
                {
                    var copyRule = fromGraph.AttributeRules.Cache.CreateCopy(attributeRule) as AMConfigurationAttributeRule;
                    copyRule.ConfigurationID = keyValue;
                    copyRule.Revision = revisionValue;

                    toGraph.AttributeRules.Insert(copyRule);
                }
            }
        }
        #endregion

        #region Event Handlers

        #region AMConfiguration Handlers

        protected virtual void AMConfiguration_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMConfiguration)e.Row;
            if (row == null)
            {
                return;
            }

            // Key Tab
            var enableFormula = false;
            var enableNumSequence = false;
            switch (row.KeyFormat)
            {
                case ConfigKeyFormats.Formula:
                    enableFormula = true;
                    break;
                case ConfigKeyFormats.NumberSequence:
                    enableNumSequence = true;
                    break;
                    // Other cases need no special handling, fields stay hidden.
            }

            PXUIFieldAttribute.SetVisible<AMConfiguration.keyEquation>(sender, row, enableFormula);
            PXUIFieldAttribute.SetRequired<AMConfiguration.keyEquation>(sender, enableFormula);
            PXDefaultAttribute.SetPersistingCheck<AMConfiguration.keyEquation>(sender, row, enableFormula ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            PXUIFieldAttribute.SetVisible<AMConfiguration.keyNumberingID>(sender, row, enableNumSequence);
            PXUIFieldAttribute.SetEnabled<AMConfiguration.keyNumberingID>(sender, row, enableNumSequence);
            PXUIFieldAttribute.SetRequired<AMConfiguration.keyNumberingID>(sender, enableNumSequence);
            PXDefaultAttribute.SetPersistingCheck<AMConfiguration.keyNumberingID>(sender, row, enableNumSequence ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            bool isPersisted = Documents.Cache.GetStatus(row) != PXEntryStatus.Inserted;

            // Price Tab
            PXUIFieldAttribute.SetEnabled<AMConfiguration.priceRollup>(sender, row, ConfiguratorSetup.Current.AllowRollupOverride == true);
            PXUIFieldAttribute.SetEnabled<AMConfiguration.priceCalc>(sender, row, ConfiguratorSetup.Current.AllowCalculateOverride == true);
            
            Documents.AllowDelete =

            ConfigurationFeatures.AllowInsert =
            ConfigurationFeatures.AllowUpdate =
            ConfigurationFeatures.AllowDelete =

            ConfigurationAttributes.AllowInsert =
            ConfigurationAttributes.AllowUpdate =
            ConfigurationAttributes.AllowDelete =

            AttributeRules.AllowInsert =
            AttributeRules.AllowUpdate =
            AttributeRules.AllowDelete =

            FeatureOptions.AllowInsert =
            FeatureOptions.AllowUpdate =
            FeatureOptions.AllowDelete =

            SelectedConfiguration.AllowUpdate =

            FeatureRules.AllowInsert =
            FeatureRules.AllowUpdate =
            FeatureRules.AllowDelete = row.Status == ConfigRevisionStatus.Pending && isPersisted;

            //Actions
            Delete.SetEnabled(row.Status == ConfigRevisionStatus.Pending);
            TestConfiguration.SetEnabled(isPersisted);
            CopyConfiguration.SetEnabled(isPersisted);
            SetAsDefaultForItem.SetEnabled(isPersisted);
            SetActiveConfiguration.SetEnabled(isPersisted && row.Status == ConfigRevisionStatus.Pending);

            PXUIFieldAttribute.SetEnabled<AMConfiguration.bOMID>(sender, row, row.Status == ConfigRevisionStatus.Pending && !isPersisted);
        }

        protected virtual void AMConfiguration_Status_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (e.NewValue.ToString() == ConfigRevisionStatus.Pending && RelatedResults.SelectSingle() != null)
            {
                throw new PXSetPropertyKeepPreviousException(Messages.CantChangeActiveRevisionStatusHasResult);
            }
            else if (e.NewValue.ToString() != ConfigRevisionStatus.Inactive && (OtherPendingConfiguration.SelectSingle() != null || OtherActiveConfiguration.SelectSingle() != null))
            {
                throw new PXSetPropertyKeepPreviousException(Messages.CantChangeActiveRevisionStatusHasPendingOrActive);
            }
        }

        protected virtual void AMConfiguration_Status_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMConfiguration)e.Row;
            if (row?.InventoryID == null)
            {
                return;
            }

            var cm = new ConfigurationIDManager(sender.Graph);
            if (row.Status == ConfigRevisionStatus.Active && row.InventoryID != null && !string.IsNullOrWhiteSpace(row.BOMID))
            {
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
                cm.SetID(row);
#pragma warning restore PX1043
                return;
            }

            var oldValueString = Convert.ToString(e.OldValue);
            if (!string.IsNullOrWhiteSpace(oldValueString) 
                && oldValueString == ConfigRevisionStatus.Active)
            {
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
                cm.RemoveID(row);
#pragma warning restore PX1043
            }
        }

        protected virtual void AMConfiguration_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMConfiguration)e.Row;
            if (row == null)
            {
                return;
            }

            if (row.Status != ConfigRevisionStatus.Pending)
            {
                e.Cancel = true;
                throw new PXInvalidOperationException(Messages.CantDeleteConfigurationUnlessPending);
            }
        }

        protected virtual void AMConfiguration_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {

            var row = (AMConfiguration)e.Row;
            if (row == null || e.TranStatus != PXTranStatus.Open)
            {
                return;
            }

#pragma warning disable PX1073 // Exceptions cannot be thrown in the RowPersisted event handler
            if (e.Operation == PXDBOperation.Delete && !ConfigurationContainsOtherRevs(row))
            {
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
                var cm = new ConfigurationIDManager(this) {PersistChanges = true};
                cm.RemoveID(row);
#pragma warning restore PX1043
            }
#pragma warning restore PX1073
        }

        #endregion AMConfiguration Handlers

        #region AMConfigurationFeature Handlers

        protected virtual void AMConfigurationFeature_SortOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMConfigurationFeature)e.Row;
            if (row == null) return;

            e.NewValue = row.LineNbr;
        }

        protected virtual void AMConfigurationFeature_FeatureID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMConfigurationFeature)e.Row;
            if (row == null || IsImport)
            {
                return;
            }

            var item = PXSelectorAttribute.Select<AMFeature.featureID>(sender, row) as AMFeature;
            if (string.IsNullOrWhiteSpace(row.Label))
            {
                sender.SetValueExt<AMConfigurationFeature.label>(row, item?.FeatureID);
                sender.SetValueExt<AMConfigurationFeature.descr>(row, item?.Descr);
                sender.SetValueExt<AMConfigurationFeature.printResults>(row, item?.PrintResults);
            }

            //Adds configured AMFeatureOption as default values
            if (!FeatureOptions.Select().Any())
            {
                foreach (AMFeatureOption featureOption in PXSelect<AMFeatureOption,
                                                            Where<AMFeatureOption.featureID,
                                                                Equal<Required<AMFeatureOption.featureID>>>>.Select(this, row.FeatureID))
                {
                    var insertedOption = FeatureOptions.Insert(new AMConfigurationOption
                    {
                        Descr = featureOption.Descr,
                        InventoryID = featureOption.InventoryID,
                        Label = featureOption.Label,
                        SubItemID = featureOption.SubItemID,
                        UOM = featureOption.UOM
                    });

                    insertedOption.FixedInclude = featureOption.FixedInclude;
                    insertedOption.LotQty = featureOption.LotQty;
                    insertedOption.MaterialType = featureOption.MaterialType;
                    insertedOption.SubcontractSource = featureOption.SubcontractSource;
                    insertedOption.MaxQty = featureOption.MaxQty;
                    insertedOption.MinQty = featureOption.MinQty;
                    insertedOption.PhantomRouting = featureOption.PhantomRouting;
                    insertedOption.PriceFactor = featureOption.PriceFactor;
                    insertedOption.QtyEnabled = featureOption.QtyEnabled;
                    insertedOption.QtyRequired = featureOption.QtyRequired;
                    insertedOption.ResultsCopy = featureOption.ResultsCopy;
                    insertedOption.ScrapFactor = featureOption.ScrapFactor;
                    insertedOption.BFlush = featureOption.BFlush;
                    insertedOption.QtyRoundUp = featureOption.QtyRoundUp;
                    insertedOption.BatchSize = featureOption.BatchSize;
                    insertedOption.PrintResults = featureOption.PrintResults;

                    if (insertedOption.FixedInclude.GetValueOrDefault() &&
                        insertedOption.InventoryID != null && 
                        ConfigOptionFixedIncludeAttribute.TryGetQtyRequiredAsFormulaValue(FeatureOptions.Cache, insertedOption.QtyRequired, out var formulaValue))
                    {
                        // Make sure fixed include options have the correct formula value for qty required
                        insertedOption.QtyRequired = formulaValue;
                    }

                    FeatureOptions.Update(insertedOption);
                }
            }

            var configAttributeLabels = new HashSet<string>();
            foreach (AMConfigurationAttribute result in ConfigurationAttributes.Select())
            {
                if (result.Label == null)
                {
                    continue;
                }

                configAttributeLabels.Add(result.Label.Trim());
            }

            //Adds configured AMFeatureOption as default values
            foreach (AMFeatureAttribute currentOption in PXSelect<AMFeatureAttribute,
                                                        Where<AMFeatureAttribute.featureID,
                                                            Equal<Required<AMFeatureAttribute.featureID>>>>.Select(this, row.FeatureID))
            {
                if (currentOption.Label != null && configAttributeLabels.Add(currentOption.Label.Trim()))
                {
                    var insertedOption = new AMConfigurationAttribute
                    {
                        AttributeID = currentOption.AttributeID,
                        ConfigurationID = row.ConfigurationID,
                        Descr = currentOption.Descr,
                        Enabled = currentOption.Enabled,
                        Label = currentOption.Label,
                        Required = currentOption.Required,
                        Revision = row.Revision,
                        Value = currentOption.Value,
                        Variable = currentOption.Variable,
                        Visible = currentOption.Visible,
                    };

                    ConfigurationAttributes.Insert(insertedOption);
                }
            }
        }

        protected virtual void AMConfigurationFeature_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMConfigurationFeature)e.Row;
            if (row == null)
            {
                return;
            }

            if (Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            var relatedRulesCount = 0;
            if (ConfigurationFeatures.View.Answer == WebDialogResult.None)
            {
                //Run only once before asking the question...
                var relatedRules = GetReferencedRules(row);
                if (relatedRules == null || relatedRules.Count == 0)
                {
                    return;
                }

                relatedRulesCount = relatedRules.Count;
                PXTrace.WriteInformation(RulesToUserMessage(row, relatedRules));
            }

            if (FeatureOptionReferencedOnRules(ConfigurationFeatures.View, Messages.GetLocal(Messages.DeletingFeatureIsReferencedOnRules,
                    row.Label, relatedRulesCount)) != WebDialogResult.Yes)
            {
                e.Cancel = true;
                throw new PXException(Messages.GetLocal(Messages.UnableToDeleteFeatureWithRules, row.Label.TrimIfNotNullEmpty()));
            }
        }

        protected virtual void AMConfigurationFeature_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            var row = (AMConfigurationFeature)e.Row;
            if (row == null)
            {
                return;
            }

            if (Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            var relatedRules = GetReferencedRules(row);
            if (relatedRules == null || relatedRules.Count == 0)
            {
                return;
            }

            DeleteRules(relatedRules);
        }

        #endregion

        #region AMConfigurationOption Handlers

        protected virtual void AMConfigurationOption_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMConfigurationOption)e.Row;
            if (row == null)
            {
                return;
            }

            var item = PXSelectorAttribute.Select<AMFeatureOption.inventoryID>(sender, row) as InventoryItem;
            if (item == null)
            {
                row.UOM = null;
                row.ScrapFactor = null;
                sender.SetDefaultExt<AMConfigurationOption.bFlush>(row);
                sender.SetDefaultExt<AMConfigurationOption.phantomRouting>(row);
                sender.SetDefaultExt<AMConfigurationOption.materialType>(row);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(row.Label))
            {
                row.Label = item.InventoryCD;
            }

            row.Descr = item.Descr;
            row.UOM = item.BaseUnit;
        }

        protected virtual void AMConfigurationOption_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMConfigurationOption) e.Row;
            if(row == null)
            {
                return;
            }

            if (Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            var feature = FindFeature(row.ConfigurationID, row.Revision, row.ConfigFeatureLineNbr);
            if (feature != null && ConfigurationFeatures.Cache.IsRowDeleted(feature))
            {
                return;
            }

            var relatedRulesCount = 0;
            if (ConfigurationFeatures.View.Answer == WebDialogResult.None)
            {
                //Run only once before asking the question...
                var relatedRules = GetReferencedRules(row);
                if (relatedRules == null || relatedRules.Count == 0)
                {
                    return;
                }

                relatedRulesCount = relatedRules.Count;
                PXTrace.WriteInformation(RulesToUserMessage(feature, row, relatedRules));
            }

            if (FeatureOptionReferencedOnRules(FeatureOptions.View, Messages.GetLocal(Messages.DeletingFeatureOptionIsReferencedOnRules,
                    feature?.Label.TrimIfNotNullEmpty(), row.Label.TrimIfNotNullEmpty(), relatedRulesCount)) != WebDialogResult.Yes)
            {
                e.Cancel = true;
                throw new PXException(Messages.GetLocal(Messages.UnableToDeleteFeatureOptionWithRules, feature?.Label.TrimIfNotNullEmpty(), row.Label.TrimIfNotNullEmpty()));
            }
        }

        protected virtual void AMConfigurationOption_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            var row = (AMConfigurationOption)e.Row;
            if (row == null)
            {
                return;
            }

            if (Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            var feature = FindFeature(row.ConfigurationID, row.Revision, row.ConfigFeatureLineNbr);
            if (feature != null && ConfigurationFeatures.Cache.IsRowDeleted(feature))
            {
                return;
            }

            var relatedRules = GetReferencedRules(row);
            if (relatedRules == null || relatedRules.Count == 0)
            {
                return;
            }

            DeleteRules(relatedRules);
        }

        protected virtual void AMConfigurationOption_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMConfigurationOption)e.Row;
            if (row == null)
            {
                return;
            }

            if (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update)
            {
                return;
            }

            if (row.InventoryID != null && row.OperationID == null && row.MaterialType != AMMaterialType.Supplemental)
            {
                sender.RaiseExceptionHandling<AMConfigurationOption.operationID>(
                    row,
                    row.OperationID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
                        PXUIFieldAttribute.GetDisplayName<AMConfigurationOption.operationID>(sender),
                    PXErrorLevel.Error));
                e.Cancel = true;
            }

            AMFeature feature = PXSelect<AMFeature,
                Where<AMFeature.featureID, Equal<Required<AMFeature.featureID>>
                >>.Select(this, ConfigurationFeatures.Current.FeatureID);

            if (feature == null)
            {
                return;
            }

            if (row.InventoryID == null && !feature.AllowNonInventoryOptions.GetValueOrDefault())
            {
                sender.RaiseExceptionHandling<AMConfigurationOption.inventoryID>(
                    row,
                    row.InventoryID,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FeatureDoesntAllowNonInventory),
                        feature.FeatureID.TrimIfNotNullEmpty(),
                    PXErrorLevel.Error));
                e.Cancel = true;
            }

            // Require SUBITEMID when the item is a stock item
            if (InventoryHelper.SubItemFeatureEnabled && row.InventoryID != null && row.SubItemID == null)
            {
                var inventoryItem = (InventoryItem)PXSelectorAttribute.Select<AMConfigurationOption.inventoryID>(sender, row);
                if (inventoryItem?.StkItem != true)
                {
                    return;
                }

                sender.RaiseExceptionHandling<AMConfigurationOption.subItemID>(
                        row,
                        row.SubItemID,
                        new PXSetPropertyException(Messages.SubItemIDRequiredForStockItem, PXErrorLevel.Error));
                e.Cancel = true;
            }
        }

        protected virtual void AMConfigurationOption_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMConfigurationOption) e.Row;
            if (row == null
                || !sender.AllowUpdate)
            {
                return;
            }

            bool isInventoryRow = row.InventoryID != null;
            bool isSupplemental = row.MaterialType == AMMaterialType.Supplemental;

            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.operationID>(sender, row, isInventoryRow && !isSupplemental);
            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.bFlush>(sender, row, isInventoryRow && !isSupplemental);
            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.scrapFactor>(sender, row, isInventoryRow && !isSupplemental);
            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.phantomRouting>(sender, row, isInventoryRow && !isSupplemental);
            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.materialType>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.siteID>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.locationID>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMConfigurationOption.priceFactor>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMFeatureOption.subcontractSource>(sender, row, isInventoryRow);
        }

        #endregion

        #region AMConfigurationRule Handlers

        protected virtual void AMConfigurationAttributeRule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMConfigurationAttributeRule)e.Row;
            if (row == null)
            {
                return;
            }

            AMEmptySelectorValueAttribute.OverrideDefaultValue<AMConfigurationAttributeRule.targetOptionLineNbr>(sender, row, row.RuleType != RuleTypes.Validate);

            PXUIFieldAttribute.SetEnabled<AMConfigurationAttributeRule.targetFeatureLineNbr>(sender, row, row.RuleType != RuleTypes.Validate);
            PXUIFieldAttribute.SetEnabled<AMConfigurationAttributeRule.targetOptionLineNbr>(sender, row, row.RuleType != RuleTypes.Validate);
            PXDefaultAttribute.SetPersistingCheck<AMConfigurationAttributeRule.targetFeatureLineNbr>(sender, row, row.RuleType != RuleTypes.Validate
                ? PXPersistingCheck.NullOrBlank
                : PXPersistingCheck.Nothing);

            var condition = row.Condition.TrimIfNotNull();
            var requiresField1 = RuleFormulaConditions.RequiresValue1(condition);
            var requiresField2 = RuleFormulaConditions.RequiresValue2(condition);
            PXUIFieldAttribute.SetEnabled<AMConfigurationAttributeRule.value1>(sender, row, requiresField1);
            PXDefaultAttribute.SetPersistingCheck<AMConfigurationAttributeRule.value1>(sender, row, requiresField1
                ? PXPersistingCheck.NullOrBlank
                : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<AMConfigurationAttributeRule.value2>(sender, row, requiresField2);
            PXDefaultAttribute.SetPersistingCheck<AMConfigurationAttributeRule.value2>(sender, row, requiresField2
                ? PXPersistingCheck.NullOrBlank
                : PXPersistingCheck.Nothing);
        }

        #endregion

        #region AMConfigurationAttribute Handlers
        protected virtual void AMConfigurationAttribute_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMConfigurationAttribute)e.Row;
            if(row != null)
            {
                PXUIFieldAttribute.SetEnabled<AMConfigurationAttribute.enabled>(sender, row, row.IsFormula != true);
            }
        }

        protected virtual void AMConfigurationAttribute_AttributeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMConfigurationAttribute)e.Row;
            if (row.IsFormula == true)
            {
                sender.SetValue<AMConfigurationAttribute.enabled>(row, false);
            }
            else
            {
                sender.SetValue<AMConfigurationAttribute.enabled>(row, true);

                var item = PXSelectorAttribute.Select<AMConfigurationAttribute.attributeID>(sender, row) as CSAttribute;
                if (item != null && string.IsNullOrWhiteSpace(row.Label))
                {
                    sender.SetValue<AMConfigurationAttribute.label>(row, item.AttributeID);
                }
                if (item != null && string.IsNullOrWhiteSpace(row.Descr))
                {
                    sender.SetValue<AMConfigurationAttribute.descr>(row, item.Description);
                }
            }
        }

        protected virtual void AMConfigurationAttribute_Variable_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (AMConfigurationAttribute)e.Row;
            if (string.IsNullOrWhiteSpace(row?.Variable))
            {
                return;
            }

            var variableInUseMessages = GetAttributeVariableInUseMessages(row);
            if (variableInUseMessages == null || variableInUseMessages.Count <= 0)
            {
                return;
            }

            e.Cancel = true;
            e.NewValue = row.Variable;

            var exMsg = Messages.GetLocal(Messages.UnableToChangeVariableInUse, row.Label, row.Variable, e.NewValue);
            var infoMsg = $"{exMsg}:{System.Environment.NewLine}{variableInUseMessages.ToAppendedNewLineString()}";
            PXTrace.WriteInformation(infoMsg);
#if DEBUG
            AMDebug.TraceWriteMethodName(infoMsg);
#endif
            var additionalInfo = variableInUseMessages.Count == 1
                ? variableInUseMessages[0]
                : Messages.GetLocal(Messages.SeeTraceWindow);
            throw new PXException($"{exMsg} {additionalInfo}");
        }

        protected virtual void AMConfigurationAttribute_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMConfigurationAttribute)e.Row;
            if (string.IsNullOrWhiteSpace(row?.Variable))
            {
                return;
            }

            if (Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            var variableInUseMessages = GetAttributeVariableInUseMessages(row);
            if (variableInUseMessages == null || variableInUseMessages.Count <= 0)
            {
                return;
            }

            e.Cancel = true;

            var exMsg = Messages.GetLocal(Messages.UnableToDeleteAttributeWithVariableInUse, row.Label, row.Variable);
            var infoMsg = $"{exMsg}:{System.Environment.NewLine}{variableInUseMessages.ToAppendedNewLineString()}";
            PXTrace.WriteInformation(infoMsg);
#if DEBUG
            AMDebug.TraceWriteMethodName(infoMsg);
#endif
            var additionalInfo = variableInUseMessages.Count == 1
                ? variableInUseMessages[0]
                : Messages.GetLocal(Messages.SeeTraceWindow);
            throw new PXException($"{exMsg} {additionalInfo}");
        }

        #endregion

        #endregion Event Handlers

        public PXAction<AMConfiguration> ActionDropMenu;
        [PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable actionDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMConfiguration> TestConfiguration;
        [PXButton]
        [PXUIField(DisplayName = "Test Configuration", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
        protected virtual void testConfiguration()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var graph = InitTestConfigurationEntry(SelectedConfiguration.Current);
                if (graph?.Results?.Current == null)
                {
                    return;
                }
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            }
            finally
            {   
                sw.Stop();
                var msg1 = Messages.GetLocal(Messages.LoadingConfigurationForTesting,
                    SelectedConfiguration.Current.ConfigurationID, SelectedConfiguration.Current.Revision);
                var msg2 = $"{msg1} {PXTraceHelper.RuntimeMessage(sw.Elapsed)}";
                PXTrace.WriteInformation(msg2);
#if DEBUG
                AMDebug.TraceWriteMethodName(msg2);
#endif
            }
        }

        protected virtual ConfigurationEntry InitTestConfigurationEntry(AMConfiguration configuration)
        {
            var graph = CreateInstance<ConfigurationEntry>();

            AMBomItem bom = PXSelect<AMBomItem, Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>>>.SelectWindowed(this, 0, 1, configuration?.BOMID);
            var item = InventoryItem.PK.Find(this, configuration.InventoryID);

            graph.Results.Configuration.Current = configuration;
            graph.IsInitConfiguration = true;
            graph.Results.Current = graph.Results.Insert(new AMConfigurationResults
            {
                Qty = 1.0m,
                SiteID = bom?.SiteID,
                UOM = item?.BaseUnit
            });
            if (graph.Results.Current == null)
            {
                return null;
            }
            graph.Results.Cache.SetDefaultExt<AMConfigurationResults.uOM>(graph.Results.Current);
            graph.IsInitConfiguration = false;
            graph.ConfigFilter.Current.ShowCanTestPersist = true;
            
            return graph;
        }

        public PXAction<AMConfiguration> CopyConfiguration;
        [PXButton]
        [PXUIField(DisplayName = "Copy Configuration", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Insert)]
        protected virtual void copyConfiguration()
        {
            if (Documents.Current == null)
            {
                return;
            }
            
            var toGraph = CreateInstance<ConfigurationMaint>();
            var newConfigDocument = toGraph.Documents.Insert() as AMConfiguration;

            FieldVerifying.AddHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);
            toGraph.FieldVerifying.AddHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);

            var configurationCopy = Documents.Cache.CreateCopy(Documents.Current) as AMConfiguration;
            if (configurationCopy == null)
            {
                return;
            }

            configurationCopy.NoteID = newConfigDocument.NoteID;
            configurationCopy.ConfigurationID = newConfigDocument.ConfigurationID;
            configurationCopy.Revision = newConfigDocument.Revision;
            configurationCopy = toGraph.Documents.Update(configurationCopy);

            toGraph.Documents.Cache.SetStatus(configurationCopy, PXEntryStatus.Updated);
            CopyRevision(this, toGraph, configurationCopy.ConfigurationID, configurationCopy.Revision);
            toGraph.Documents.Cache.SetStatus(configurationCopy, PXEntryStatus.Inserted);

            FieldVerifying.RemoveHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);
            toGraph.FieldVerifying.RemoveHandler<AMConfiguration.bOMRevisionID>(CancelBomRevFieldVerifying);

            throw new PXRedirectRequiredException(toGraph, string.Empty);
        }

        public PXAction<AMConfiguration> SetAsDefaultForItem;
        [PXButton]
        [PXUIField(DisplayName = "Set as default for Item", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
        protected virtual void setAsDefaultForItem()
        {
            if (Documents.Current == null)
            {
                return;
            }

            var idManager = new ConfigurationIDManager(this) {PersistChanges = true};

            if (Documents.Cache.GetStatus(Documents.Current) == PXEntryStatus.Inserted
                || (Documents.Current.Status != ConfigRevisionStatus.Active && !ConfigurationContainsActiveRev()))
            {
                throw new PXInvalidOperationException(Messages.GetLocal(Messages.SavedAndActiveConfigNecessary));
            }

            if (ConfigurationIDUpdateFilter.AskExt() == WebDialogResult.OK)
            {
                idManager.UpdateID(Documents.Current,
                    ConfigurationIDUpdateFilter.Current.Item.GetValueOrDefault(),
                    ConfigurationIDUpdateFilter.Current.Warehouse.GetValueOrDefault());
            }

            ConfigurationIDUpdateFilter.Cache.Clear();
        }

        public PXAction<AMConfiguration> SetActiveConfiguration;
        [PXButton]
        [PXUIField(DisplayName = "Set Active", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
        protected virtual void setActiveConfiguration()
        {
            Actions.PressSave();

            var config = Documents.Current;
            if (config?.ConfigurationID == null || config.Status != ConfigRevisionStatus.Pending)
            {
                return;
            }

            var activeConfig = (AMConfiguration) PXSelect<AMConfiguration,
                Where<AMConfiguration.configurationID, Equal<Required<AMConfigurationResults.configurationID>>,
                    And<AMConfiguration.status, Equal<ConfigRevisionStatus.active>>>>.Select(this, config.ConfigurationID);

            var sb = new System.Text.StringBuilder();
            if (activeConfig?.ConfigurationID != null)
            {
                activeConfig.Status = ConfigRevisionStatus.Inactive;

                sb.AppendLine(Messages.GetLocal(Messages.ConfigurationStatusChangedTo, activeConfig.ConfigurationID,
                    activeConfig.Revision, ConfigRevisionStatus.GetDescription(activeConfig.Status)));

                Documents.Update(activeConfig);
            }

            try
            {
                this.FieldVerifying.RemoveHandler<AMConfiguration.status>(AMConfiguration_Status_FieldVerifying);

                config.Status = ConfigRevisionStatus.Active;

                sb.AppendLine(Messages.GetLocal(Messages.ConfigurationStatusChangedTo, config.ConfigurationID,
                    config.Revision, ConfigRevisionStatus.GetDescription(config.Status)));

                Documents.Update(config);

                Actions.PressSave();

                if (sb.Length > 0)
                {
                    PXTrace.WriteInformation(sb.ToString());
                }
            }
            finally
            {
                this.FieldVerifying.AddHandler<AMConfiguration.status>(AMConfiguration_Status_FieldVerifying);
            }
        }

        /// <summary>
        /// Check if the given configuration contains a rev that is active
        /// </summary>
        public virtual bool ConfigurationContainsActiveRev()
        {
            if (Documents.Current == null)
            {
                return false;
            }

            return ConfigurationContainsActiveRev(this, Documents.Current.ConfigurationID);
        }

        /// <summary>
        /// Check if the given configuration contains a rev that is active
        /// </summary>
        public virtual bool ConfigurationContainsActiveRev(string configuraitonID)
        {
            return ConfigurationContainsActiveRev(this, configuraitonID);
        }

        /// <summary>
        /// Check if the given configuration contains a rev that is active
        /// </summary>
        public static bool ConfigurationContainsActiveRev(PXGraph graph, string configuraitonID)
        {
            if (graph == null)
            {
                throw new PXArgumentException("graph");
            }

            if (string.IsNullOrWhiteSpace(configuraitonID))
            {
                throw new PXArgumentException("configuraitonID");
            }

            return (AMConfiguration)PXSelect<AMConfiguration,
                Where<AMConfiguration.configurationID, Equal<Required<AMConfiguration.configurationID>>,
                    And<AMConfiguration.status, Equal<ConfigRevisionStatus.active>>>
                        >.Select(graph, configuraitonID) != null;
        }

        /// <summary>
        /// Check if the given configuration contains other revisions
        /// </summary>
        public virtual bool ConfigurationContainsOtherRevs()
        {
            return ConfigurationContainsOtherRevs(Documents.Current);
        }

        /// <summary>
        /// Check if the given configuration contains other revisions
        /// </summary>
        public virtual bool ConfigurationContainsOtherRevs(AMConfiguration configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            return ConfigurationContainsOtherRevs(configuration.ConfigurationID, configuration.Revision);
        }

        /// <summary>
        /// Check if the given configuration contains other revisions
        /// </summary>
        public virtual bool ConfigurationContainsOtherRevs(string configuraitonID, string excludingRevision)
        {
            if (string.IsNullOrWhiteSpace(configuraitonID))
            {
                throw new PXArgumentException(nameof(configuraitonID));
            }

            if (string.IsNullOrWhiteSpace(excludingRevision))
            {
                throw new PXArgumentException(nameof(excludingRevision));
            }
            
            return (AMConfiguration)PXSelect<AMConfiguration,
                Where<AMConfiguration.configurationID, Equal<Required<AMConfiguration.configurationID>>,
                    And<AMConfiguration.revision, NotEqual<Required<AMConfiguration.revision>>>>
                        >.SelectWindowed(this, 0, 1, configuraitonID, excludingRevision) != null;
        }

        #region Rule Checks

        protected virtual WebDialogResult FeatureOptionReferencedOnRules(PXView view, string question)
        {
            if (IsImport || IsContractBasedAPI)
            {
                return WebDialogResult.Yes;
            }

            return IsImport || IsContractBasedAPI ? WebDialogResult.Yes : view.Ask(question, MessageButtons.YesNo);
        }

        protected virtual List<AMConfigurationRule> GetReferencedRules(AMConfigurationOption configOption)
        {
            return configOption == null ? null :
                PXSelect<AMConfigurationRule,
                        Where<AMConfigurationRule.configurationID, Equal<Current<AMConfigurationOption.configurationID>>,
                            And<AMConfigurationRule.revision, Equal<Current<AMConfigurationOption.revision>>,
                                And<AMConfigurationRule.targetFeatureLineNbr, Equal<Current<AMConfigurationOption.configFeatureLineNbr>>,
                                    And<AMConfigurationRule.targetOptionLineNbr, Equal<Current<AMConfigurationOption.lineNbr>>>>>>>
                    .SelectMultiBound(this, new object[] { configOption }).ToFirstTableList();
        }

        protected virtual List<AMConfigurationRule> GetReferencedRules(AMConfigurationFeature configFeature)
        {
            return configFeature == null ? null :
                PXSelect<AMConfigurationRule,
                        Where<AMConfigurationRule.configurationID, Equal<Current<AMConfigurationFeature.configurationID>>,
                            And<AMConfigurationRule.revision, Equal<Current<AMConfigurationFeature.revision>>,
                                And<AMConfigurationRule.targetFeatureLineNbr, Equal<Current<AMConfigurationFeature.lineNbr>>>>>>
                    .SelectMultiBound(this, new object[] { configFeature }).ToFirstTableList();
        }

        protected virtual string RulesToUserMessage(AMConfigurationFeature feature, List<AMConfigurationRule> rules)
        {
            return RulesToUserMessage(feature, null, rules);
        }

        protected virtual string RulesToUserMessage(AMConfigurationFeature feature, AMConfigurationOption option, List<AMConfigurationRule> rules)
        {
            var sb = new StringBuilder();
            foreach (var rule in rules)
            {
                var msg = RuleToUserMessage(feature?.Label, option?.Label, rule);
                if (string.IsNullOrWhiteSpace(msg))
                {
                    continue;
                }
                sb.AppendLine(msg);
            }
            return sb.ToString();
        }

        private string _LocalAllMsg;
        private string _LocalRuleExistsOnAttributeMsg;
        private string _LocalRuleExistsOnFeature;
        protected string LocalAllMsg => string.IsNullOrWhiteSpace(_LocalAllMsg) ? _LocalAllMsg = Messages.GetLocal(Messages.All) : _LocalAllMsg;
        protected string LocalRuleExistsOnAttributeMsg => string.IsNullOrWhiteSpace(_LocalRuleExistsOnAttributeMsg) ? _LocalRuleExistsOnAttributeMsg = Messages.GetLocal(Messages.RuleExistsOnAttribute) : _LocalRuleExistsOnAttributeMsg;
        protected string LocalRuleExistsOnFeature => string.IsNullOrWhiteSpace(_LocalRuleExistsOnFeature) ? _LocalRuleExistsOnFeature = Messages.GetLocal(Messages.RuleExistsOnFeature) : _LocalRuleExistsOnFeature;

        protected virtual string RuleToUserMessage(string featureLabel, string optionLabel, AMConfigurationRule rule)
        {
            if (rule?.ConfigurationID == null)
            {
                return null;
            }

            var optionLabelMsg = optionLabel.TrimIfNotNullEmpty();
            if (string.IsNullOrWhiteSpace(optionLabelMsg) && rule.TargetOptionLineNbr != null)
            {
                optionLabelMsg = FindOptionLinkedToRule(rule)?.Label;
            }
            optionLabelMsg = string.IsNullOrWhiteSpace(optionLabelMsg) ? $"[{LocalAllMsg}]" : optionLabelMsg.TrimIfNotNullEmpty();
            
            if (rule.RuleSource == RuleTargetSource.Attribute)
            {
                var linkedAttribute = FindAttributeLinkedToRule(rule);
                return string.Format(LocalRuleExistsOnAttributeMsg, linkedAttribute?.Label.TrimIfNotNullEmpty(), RuleTypes.Get(rule.RuleType), featureLabel.TrimIfNotNullEmpty(), optionLabelMsg);
            }

            var linkedFeature = FindFeatureLinkedToRule(rule);
            return string.Format(LocalRuleExistsOnFeature, linkedFeature?.Label.TrimIfNotNullEmpty(), RuleTypes.Get(rule.RuleType), featureLabel.TrimIfNotNullEmpty(), optionLabelMsg);
        }

        protected virtual AMConfigurationAttribute FindAttributeLinkedToRule(AMConfigurationRule rule)
        {
            if (rule == null)
            {
                return null;
            }

            var located = ConfigurationAttributes.Locate(new AMConfigurationAttribute
            {
                ConfigurationID = rule.ConfigurationID,
                Revision = rule.Revision,
                LineNbr = rule.SourceLineNbr
            });

            return located ?? PXSelect<AMConfigurationAttribute,
                           Where<AMConfigurationAttribute.configurationID, Equal<Current<AMConfigurationRule.configurationID>>,
                               And<AMConfigurationAttribute.revision, Equal<Current<AMConfigurationRule.revision>>,
                                   And<AMConfigurationAttribute.lineNbr, Equal<Current<AMConfigurationRule.sourceLineNbr>>>>>>
                       .SelectSingleBound(this, new object[] { rule });
        }

        protected virtual AMConfigurationFeature FindFeatureLinkedToRule(AMConfigurationRule rule)
        {
            return FindFeature(rule.ConfigurationID, rule.Revision, rule.SourceLineNbr);
        }

        protected virtual AMConfigurationFeature FindFeature(string configurationId, string revision, int? lineNbr)
        {
            var located = ConfigurationFeatures.Locate(new AMConfigurationFeature
            {
                ConfigurationID = configurationId,
                Revision = revision,
                LineNbr = lineNbr
            });

            return located ?? PXSelect<AMConfigurationFeature,
                           Where<AMConfigurationFeature.configurationID, Equal<Required<AMConfigurationFeature.configurationID>>,
                               And<AMConfigurationFeature.revision, Equal<Required<AMConfigurationFeature.revision>>,
                                   And<AMConfigurationFeature.lineNbr, Equal<Required<AMConfigurationFeature.lineNbr>>>>>>
                       .Select(this, configurationId, revision, lineNbr);
        }

        protected virtual AMConfigurationOption FindOptionLinkedToRule(AMConfigurationRule rule)
        {
            return FindOption(rule?.ConfigurationID, rule?.Revision, rule?.TargetFeatureLineNbr, rule?.TargetOptionLineNbr);
        }

        protected virtual AMConfigurationOption FindOption(string configurationId, string revision, int? featureLineNbr, int? optionLineNbr)
        {
            var located = FeatureOptions.Locate(new AMConfigurationOption
            {
                ConfigurationID = configurationId,
                Revision = revision,
                ConfigFeatureLineNbr = featureLineNbr,
                LineNbr = optionLineNbr
            });

            return located ?? PXSelect<AMConfigurationOption,
                           Where<AMConfigurationOption.configurationID, Equal<Required<AMConfigurationOption.configurationID>>,
                               And<AMConfigurationOption.revision, Equal<Required<AMConfigurationOption.revision>>,
                                   And<AMConfigurationOption.configFeatureLineNbr, Equal<Required<AMConfigurationOption.configFeatureLineNbr>>,
                                       And<AMConfigurationOption.lineNbr, Equal<Required<AMConfigurationOption.lineNbr>>>>>>>
                       .Select(this, configurationId, revision, featureLineNbr, optionLineNbr);
        }

        protected virtual AMConfigurationAttributeRule FindAsAttributeRule(AMConfigurationRule rule)
        {
            if (rule == null)
            {
                return null;
            }

            var located = AttributeRules.Locate(new AMConfigurationAttributeRule
            {
                ConfigurationID = rule.ConfigurationID,
                Revision = rule.Revision,
                RuleSource = rule.RuleSource,
                SourceLineNbr = rule.SourceLineNbr,
                LineNbr = rule.LineNbr
            });

            return located ?? PXSelect<AMConfigurationAttributeRule,
                           Where<AMConfigurationAttributeRule.configurationID, Equal<Required<AMConfigurationAttribute.configurationID>>,
                               And<AMConfigurationAttributeRule.revision, Equal<Required<AMConfigurationAttribute.revision>>,
                                   And<AMConfigurationAttributeRule.sourceLineNbr, Equal<Required<AMConfigurationAttribute.lineNbr>>,
                                       And<AMConfigurationAttributeRule.lineNbr, Equal<Required<AMConfigurationAttributeRule.lineNbr>>>>>>>
                       .Select(this, rule.ConfigurationID, rule.Revision, rule.SourceLineNbr, rule.LineNbr);
        }

        protected virtual AMConfigurationFeatureRule FindAsFeatureRule(AMConfigurationRule rule)
        {
            if (rule == null)
            {
                return null;
            }

            var located = FeatureRules.Locate(new AMConfigurationFeatureRule
            {
                ConfigurationID = rule.ConfigurationID,
                Revision = rule.Revision,
                RuleSource = rule.RuleSource,
                SourceLineNbr = rule.SourceLineNbr,
                LineNbr = rule.LineNbr
            });

            return located ?? PXSelect<AMConfigurationFeatureRule,
                           Where<AMConfigurationFeatureRule.configurationID, Equal<Required<AMConfigurationFeatureRule.configurationID>>,
                               And<AMConfigurationFeatureRule.revision, Equal<Required<AMConfigurationFeatureRule.revision>>,
                                   And<AMConfigurationFeatureRule.sourceLineNbr, Equal<Required<AMConfigurationFeatureRule.lineNbr>>,
                                       And<AMConfigurationFeatureRule.lineNbr, Equal<Required<AMConfigurationFeatureRule.lineNbr>>>>>>>
                       .Select(this, rule.ConfigurationID, rule.Revision, rule.SourceLineNbr, rule.LineNbr);
        }

        private void DeleteRules(List<AMConfigurationRule> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                return;
            }

            foreach (var rule in rules)
            {
                if (rule?.RuleSource == null || this.Caches<AMConfigurationRule>().IsRowDeleted(rule))
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"Rule keys: {rule.GetRowKeyValues(this)}");
#endif

                if (rule.RuleSource == RuleTargetSource.Attribute)
                {
                    var foundAttributeRule = FindAsAttributeRule(rule);
                    if (foundAttributeRule != null && !AttributeRules.Cache.IsRowDeleted(foundAttributeRule))
                    {
                        AttributeRules.Delete(foundAttributeRule);
                        continue;
                    }
                }

                if (rule.RuleSource == RuleTargetSource.Feature)
                {
                    var foundFeatureRule = FindAsFeatureRule(rule);
                    if (foundFeatureRule != null && !FeatureRules.Cache.IsRowDeleted(foundFeatureRule))
                    {
                        FeatureRules.Delete(foundFeatureRule);
                        continue;
                    }
                }
            }
        }

        #endregion

        #region Attribute Variable Checks

        //Areas with Formulas:
        //  + AMConfiguration.KeyDescription, TranDescription, KeyEquation
        //  + AMConfigurationAttribute.Value
        //  + AMConfigurationAttributeRule.Value1, Value2
        //  + AMConfigurationFeature.MinSelection, MaxSelection, MinQty, MaxQty, LotQty
        //  + AMConfigurationOption.QtyRequired, MinQty, MaxQty, LotQty, ScrapFactor

        /// <summary>
        /// Main call for check of all attribute variables in use checks.
        /// </summary>
        /// <returns>List of user messages referencing entities using the given variable</returns>
        protected virtual List<string> GetAttributeVariableInUseMessages(AMConfigurationAttribute attribute)
        {
            if (attribute?.ConfigurationID == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            return GetAttributeVariableInUseMessages(attribute.ConfigurationID, attribute.Revision, attribute.Variable);
        }

        /// <summary>
        /// Main call for check of all attribute variables in use checks.
        /// </summary>
        /// <returns>List of user messages referencing entities using the given variable</returns>
        protected virtual List<string> GetAttributeVariableInUseMessages(string configurationId, string revision, string variable)
        {
            if (string.IsNullOrWhiteSpace(configurationId))
            {
                throw new ArgumentNullException(nameof(configurationId));
            }

            if (string.IsNullOrWhiteSpace(revision))
            {
                throw new ArgumentNullException(nameof(revision));
            }

            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(variable))
            {
                return list;
            }

            var config = Documents.Current;
            if (config?.ConfigurationID == null || config.ConfigurationID != configurationId ||
                config.Revision != revision)
            {
                return list;
            }

            // Covers formula fields:   AMConfiguration.KeyDescription, TranDescription, KeyEquation
            if (IsRowFieldsUsingVariable(Documents.Cache, config, variable, 
                new[] {nameof(AMConfiguration.keyDescription),
                    nameof(AMConfiguration.tranDescription),
                    nameof(AMConfiguration.keyEquation) }, out var configFieldList))
            {
                list.Add(BuildVariableInUseUserMessage(Documents.Cache, config, variable, configFieldList, null));
            }

            // Covers formula fields:   AMConfigurationAttribute.Value
            var otherAttributeMessages = GetAttributesWithAttributeVariable(configurationId, revision, variable);
            if (otherAttributeMessages != null)
            {
                foreach (var otherAttributeMsg in otherAttributeMessages)
                {
                    list.Add(otherAttributeMsg);
                }
            }

            // Covers formula fields:   AMConfigurationAttributeRule.Value1, Value2
            var attributeRuleMessages = GetAttributesRulesWithAttributeVariable(configurationId, revision, variable);
            if (attributeRuleMessages != null)
            {
                foreach (var attributeRuleMsg in attributeRuleMessages)
                {
                    list.Add(attributeRuleMsg);
                }
            }

            // Covers formula fields:   AMConfigurationFeature.MinSelection, MaxSelection, MinQty, MaxQty, LotQty
            //                          AMConfigurationOption.QtyRequired, MinQty, MaxQty, LotQty, ScrapFactor
            var featureOptionMessages = GetFeaturesOptionsWithAttributeVariable(configurationId, revision, variable);
            if (featureOptionMessages != null)
            {
                foreach (var featureOptionMsg in featureOptionMessages)
                {
                    list.Add(featureOptionMsg);
                }
            }

            return list;
        }

        protected virtual string BuildVariableInUseUserMessage<TDac>(PXCache rowCache, TDac row, string variable, string fieldNamesContainingVariable, string rowKeyUserInfo)
            where TDac : class, IBqlTable, new()
        {
            var containsMsg = string.IsNullOrWhiteSpace(rowKeyUserInfo) 
                ? Common.Cache.GetCacheName(typeof(TDac)) 
                : $"{Common.Cache.GetCacheName(typeof(TDac))} ({rowKeyUserInfo})";
            return Messages.GetLocal(Messages.ConfigContainsVariable, containsMsg, variable, fieldNamesContainingVariable);
        }

        protected virtual bool IsRowFieldsUsingVariable<TDac>(PXCache rowCache, TDac row, string variable, string[] fieldNames, out string fieldDisplayNames)
            where TDac : class, IBqlTable, new()
        {
            fieldDisplayNames = null;
            if (fieldNames == null)
            {
                return false;
            }

            var fieldDisplayNameList = new List<string>();
            foreach (var fieldName in fieldNames)
            {
                if (AMFormulaInterpreter.FormulaContainsVariable(rowCache.GetValue(row, fieldName) as string, variable))
                {
                    fieldDisplayNameList.Add(PXUIFieldAttribute.GetDisplayName(rowCache, fieldName));
                }
            }

            if (fieldDisplayNameList.Count > 0)
            {
                fieldDisplayNames = string.Join(", ", fieldDisplayNameList);
            }

            return fieldDisplayNameList.Count > 0;
        }

        private bool IsConfigRevVariableEmpty(string configurationId, string revision, string variable)
        {
            return string.IsNullOrWhiteSpace(configurationId) || 
                   string.IsNullOrWhiteSpace(revision) ||
                   string.IsNullOrWhiteSpace(variable);
        }

        /// <summary>
        /// Find all features and/or options using formulas relating to variables returning user messages of row/values
        /// </summary>
        protected virtual List<string> GetFeaturesOptionsWithAttributeVariable(string configurationId, string revision, string variable)
        {
            var list = new List<string>();
            if (IsConfigRevVariableEmpty(configurationId, revision, variable))
            {
                return list;
            }

            var featureHash = new HashSet<int>();

            foreach (PXResult<AMConfigurationFeature, AMConfigurationOption> result in PXSelectJoin<AMConfigurationFeature,
                    LeftJoin<AMConfigurationOption,
                        On<AMConfigurationOption.configurationID, Equal<AMConfigurationFeature.configurationID>,
                            And<AMConfigurationOption.revision, Equal<AMConfigurationFeature.revision>,
                                And<AMConfigurationOption.configFeatureLineNbr, Equal<AMConfigurationFeature.lineNbr>>>>>,
                    Where<AMConfigurationOption.configurationID, Equal<Required<AMConfigurationFeature.configurationID>>,
                        And<AMConfigurationOption.revision, Equal<Required<AMConfigurationFeature.revision>>>>>
                .Select(this, configurationId, revision))
            {
                var feature = ConfigurationFeatures.Cache.LocateElse((AMConfigurationFeature)result);
                if (feature?.LineNbr == null)
                {
                    continue;
                }

                //Hash used to process once per feature...
                if (featureHash.Add(feature.LineNbr.GetValueOrDefault()) && 
                    IsRowFieldsUsingVariable(ConfigurationFeatures.Cache, feature, variable,
                        new[] 
                        {
                            nameof(AMConfigurationFeature.minSelection),
                            nameof(AMConfigurationFeature.maxSelection),
                            nameof(AMConfigurationFeature.minQty),
                            nameof(AMConfigurationFeature.maxQty),
                            nameof(AMConfigurationFeature.lotQty)
                        }, out var featureFieldList))
                {
                    list.Add(BuildVariableInUseUserMessage(ConfigurationFeatures.Cache, feature, variable, featureFieldList, feature.Label.TrimIfNotNullEmpty()));
                }

                //Option could be null for 
                var option = FeatureOptions.Cache.LocateElse((AMConfigurationOption)result);
                if (option?.Revision == null)
                {
                    continue;
                }

                //Process for each option...
                if (IsRowFieldsUsingVariable(FeatureOptions.Cache, option, variable,
                    new[]
                    {
                        nameof(AMConfigurationOption.qtyRequired),
                        nameof(AMConfigurationOption.scrapFactor),
                        nameof(AMConfigurationOption.minQty),
                        nameof(AMConfigurationOption.maxQty),
                        nameof(AMConfigurationOption.lotQty)
                    }, out var optionFieldList))
                {
                    var onFeature = Messages.GetLocal(Messages.OnFeature, option.Label.TrimIfNotNullEmpty(), feature.Label.TrimIfNotNullEmpty());
                    list.Add(BuildVariableInUseUserMessage(FeatureOptions.Cache, option, variable, optionFieldList, onFeature));
                }
            }

            return list;
        }

        /// <summary>
        /// Find all attributes using formulas relating to variables returning user messages of row/values
        /// </summary>
        protected virtual List<string> GetAttributesWithAttributeVariable(string configurationId, string revision, string variable)
        {
            var list = new List<string>();
            if (IsConfigRevVariableEmpty(configurationId, revision, variable))
            {
                return list;
            }

            string valueDisplayName = null;

            foreach (AMConfigurationAttribute otherAtt in PXSelect<AMConfigurationAttribute,
                Where<AMConfigurationAttribute.configurationID, Equal<Required<AMConfiguration.configurationID>>,
                    And<AMConfigurationAttribute.revision, Equal<Required<AMConfiguration.revision>>>>>
                .Select(this, configurationId, revision))
            {
                var otherAttribute = ConfigurationAttributes.Cache.LocateElse(otherAtt);
                // Null attributeid is a formula attribute which is the only way you are going to get a variable on an configuration attribute
                if (otherAttribute?.AttributeID == null || otherAttribute.Variable.EqualsWithTrim(variable))
                {
                    continue;
                }

                if (!AMFormulaInterpreter.FormulaContainsVariable(otherAttribute.Value, variable))
                {
                    continue;
                }

                if (valueDisplayName == null)
                {
                    valueDisplayName = PXUIFieldAttribute.GetDisplayName(ConfigurationAttributes.Cache, nameof(AMConfigurationAttribute.value));
                }

                list.Add(BuildVariableInUseUserMessage(ConfigurationAttributes.Cache, otherAttribute, variable, valueDisplayName, otherAttribute.Label.TrimIfNotNullEmpty()));
            }
            return list;
        }

        //  + AMConfigurationAttributeRule.Value1, Value2
        /// <summary>
        /// Find all attributes rules using formulas relating to variables returning user messages of row/values
        /// </summary>
        protected virtual List<string> GetAttributesRulesWithAttributeVariable(string configurationId, string revision, string variable)
        {
            var list = new List<string>();
            if (IsConfigRevVariableEmpty(configurationId, revision, variable))
            {
                return list;
            }

            foreach (PXResult<AMConfigurationAttributeRule, AMConfigurationAttribute> result in PXSelectJoin<AMConfigurationAttributeRule,
                    InnerJoin<AMConfigurationAttribute, 
                        On<AMConfigurationAttribute.configurationID, Equal<AMConfigurationAttributeRule.configurationID>,
                            And<AMConfigurationAttribute.revision, Equal<AMConfigurationAttributeRule.revision>,
                            And<AMConfigurationAttribute.lineNbr, Equal< AMConfigurationAttributeRule.sourceLineNbr>>>>>,
                    Where<AMConfigurationAttributeRule.configurationID, Equal<Required<AMConfigurationAttributeRule.configurationID>>,
                        And<AMConfigurationAttributeRule.revision, Equal<Required<AMConfigurationAttributeRule.revision>>>>>
                .Select(this, configurationId, revision))
            {
                var attributeRule = AttributeRules.Cache.LocateElse((AMConfigurationAttributeRule) result);
                var attribute = ConfigurationAttributes.Cache.LocateElse((AMConfigurationAttribute) result);

                // Null attributeid is a formula attribute which is the only way you are going to get a variable on an configuration attribute
                if (attributeRule?.Value1 == null || attribute?.Label == null)
                {
                    continue;
                }

                if (IsRowFieldsUsingVariable(AttributeRules.Cache, attributeRule, variable,
                    new[]
                    {
                        nameof(AMConfigurationAttributeRule.value1),
                        nameof(AMConfigurationAttributeRule.value2)
                    }, out var ruleFieldList))
                {
                    var ruleOnAttribute = Messages.GetLocal(Messages.RuleOnAttribute, RuleTypes.Get(attributeRule.RuleType), attribute.Label.TrimIfNotNullEmpty());
                    list.Add(BuildVariableInUseUserMessage(AttributeRules.Cache, attributeRule, variable, ruleFieldList, ruleOnAttribute));
                }
            }
            return list;
        }

        #endregion


        protected virtual void CancelBomRevFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void _(Events.FieldUpdated<AMConfigurationOption, AMConfigurationOption.materialType> e)
        {
            if (e.Row?.MaterialType == null)
            {
                return;
            }

            if (e.Row.MaterialType == AMMaterialType.Regular)
            {
                e.Cache.SetValueExt<AMConfigurationOption.subcontractSource>(e.Row, AMSubcontractSource.None);
                return;
            }

            e.Cache.SetValueExt<AMConfigurationOption.subcontractSource>(e.Row, AMSubcontractSource.Purchase);
        }

        [Serializable]
        [PXCacheName("Configuration Levels")]
        public class ConfigurationIDLevels : IBqlTable
        {
            #region Item
            public abstract class item : PX.Data.BQL.BqlBool.Field<item> { }
            protected bool? _Item;
            [PXBool]
            [PXUnboundDefault(true)]
            [PXUIField(DisplayName = "Item")]
            public virtual bool? Item
            {
                get
                {
                    return this._Item;
                }
                set
                {
                    this._Item = value;
                }
            }
            #endregion
            #region Warehouse
            public abstract class warehouse : PX.Data.BQL.BqlBool.Field<warehouse> { }
            protected bool? _Warehouse;
            [PXBool]
            [PXUnboundDefault(true)]
            [PXUIField(DisplayName = "Warehouse")]
            public virtual bool? Warehouse
            {
                get
                {
                    return this._Warehouse;
                }
                set
                {
                    this._Warehouse = value;
                }
            }
            #endregion
        }
    }
}