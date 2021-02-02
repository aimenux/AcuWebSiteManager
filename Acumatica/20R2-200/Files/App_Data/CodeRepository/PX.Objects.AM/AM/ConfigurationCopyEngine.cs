using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AM
{
    public class ConfigurationCopyEngine
    {
        protected readonly PXGraph _graph;

        private ConfigurationCopyEngine(PXGraph graph)
        {
            _graph = graph ?? throw new PXArgumentException(nameof(graph));
        }

        public static void UpdateConfigurationFromKey(PXGraph graph, AMConfigurationResults currentConfigResult, string configKeyFrom)
        {
            if (graph == null || currentConfigResult == null || string.IsNullOrWhiteSpace(configKeyFrom))
            {
                return;
            }

            var fromConfiguration = GetConfigResultByKey(graph, configKeyFrom, currentConfigResult.ConfigurationID, currentConfigResult.ConfigResultsID);
            if (fromConfiguration == null)
            {
                return;
            }

            UpdateConfigurationFromConfiguration(graph, currentConfigResult, fromConfiguration);
        }

        public static void UpdateConfigurationFromConfiguration(PXGraph graph, AMConfigurationResults currentConfigResult, AMConfigurationResults fromConfigResult)
        {
            UpdateConfigurationFromConfiguration(graph, currentConfigResult, fromConfigResult, true, true);
        }

        // ASSUMES CURRENT CONFIGURATION RESULTS HAS ALL RECORDS ALREADY IN PLACE...
        // Allows for copy of configuration across revisions
        public static void UpdateConfigurationFromConfiguration(PXGraph graph, AMConfigurationResults currentConfigResult, AMConfigurationResults fromConfigResult, bool updateAttributes, bool updateOptions)
        {
            if (currentConfigResult == null || fromConfigResult == null)
            {
                return;
            }

            // We only need to sync the records the users would change such as attributes and options. All others are calculated or not updated by the user.
            var cce = new ConfigurationCopyEngine(graph);

            cce.UpdateConfigResult(currentConfigResult, fromConfigResult);

            // Set attributes first so copy can pickup qty enabled options where qty also has a formula from attributes
            if (updateAttributes)
            {
                cce.UpdateAttriburtes(currentConfigResult, fromConfigResult);
            }

            if (updateOptions)
            {
                cce.UpdateOptions(currentConfigResult, fromConfigResult, true);
            }
        }

        protected virtual void UpdateConfigResult(AMConfigurationResults toConfigResult, AMConfigurationResults fromConfigResult)
        {
            if (toConfigResult == null)
            {
                throw new PXArgumentException(nameof(toConfigResult));
            }
            if (fromConfigResult == null)
            {
                throw new PXArgumentException(nameof(fromConfigResult));
            }

            toConfigResult.KeyID = fromConfigResult.KeyID;
            toConfigResult.KeyDescription = fromConfigResult.KeyDescription;
            toConfigResult.TranDescription = fromConfigResult.TranDescription;
            _graph.Caches[typeof(AMConfigurationResults)].Update(toConfigResult);
        }

        protected virtual void UpdateAttriburtes(AMConfigurationResults toConfigResult, AMConfigurationResults fromConfigResult)
        {
            var fromAttributes = GetAttributes(fromConfigResult);
            foreach (PXResult<AMConfigResultsAttribute, AMConfigurationAttribute> result in PXSelectJoin<AMConfigResultsAttribute,
                InnerJoin<AMConfigurationAttribute,
                    On<AMConfigResultsAttribute.configurationID, Equal<AMConfigurationAttribute.configurationID>,
                        And<AMConfigResultsAttribute.revision, Equal<AMConfigurationAttribute.revision>,
                            And<AMConfigResultsAttribute.attributeLineNbr, Equal<AMConfigurationAttribute.lineNbr>>>>>,
                Where<AMConfigResultsAttribute.configResultsID, Equal<Required<AMConfigurationResults.configResultsID>>>>.Select(_graph, toConfigResult.ConfigResultsID))
            {
                var attribute = (AMConfigurationAttribute) result;
                var attributeResult = (AMConfigResultsAttribute) result;
                if (attribute == null || string.IsNullOrWhiteSpace(attribute.Label) ||
                    attributeResult == null || attributeResult.ConfigResultsID == null)
                {
                    continue;
                }

                PXResult<AMConfigResultsAttribute, AMConfigurationAttribute> fromAttributeResult = null;
                if (!fromAttributes.TryGetValue(attribute.Label, out fromAttributeResult))
                {
                    continue;
                }
                var attributeResultFrom = (AMConfigResultsAttribute)fromAttributeResult;
                if (attributeResultFrom == null || attributeResultFrom.ConfigResultsID == null)
                {
                    continue;
                }

                // UPDATES...

                attributeResult.Value = attributeResultFrom.Value;

                _graph.Caches[typeof(AMConfigResultsAttribute)].Update(attributeResult);
            }
        }

        protected virtual void UpdateOptions(AMConfigurationResults toConfigResult, AMConfigurationResults fromConfigResult)
        {
            UpdateOptions(toConfigResult, fromConfigResult, false);
        }

        protected virtual void UpdateOptions(AMConfigurationResults toConfigResult, AMConfigurationResults fromConfigResult, bool includeQtyEnabledOnly)
        {
            var fromOptions = GetOptions(fromConfigResult);
            foreach (PXResult<AMConfigResultsOption, AMConfigurationOption, AMConfigurationFeature> result in PXSelectJoin<AMConfigResultsOption,
                InnerJoin<AMConfigurationOption,
                    On<AMConfigResultsOption.configurationID, Equal<AMConfigurationOption.configurationID>,
                        And<AMConfigResultsOption.revision, Equal<AMConfigurationOption.revision>,
                            And<AMConfigResultsOption.featureLineNbr, Equal<AMConfigurationOption.configFeatureLineNbr>,
                                And<AMConfigResultsOption.optionLineNbr, Equal<AMConfigurationOption.lineNbr>>>>>,
                    InnerJoin<AMConfigurationFeature,
                        On<AMConfigResultsOption.configurationID, Equal<AMConfigurationFeature.configurationID>,
                            And<AMConfigResultsOption.revision, Equal<AMConfigurationFeature.revision>,
                                And<AMConfigResultsOption.featureLineNbr, Equal<AMConfigurationFeature.lineNbr>>>>>>,
                Where<AMConfigResultsOption.configResultsID, Equal<Required<AMConfigResultsOption.configResultsID>>>>.Select(_graph, toConfigResult.ConfigResultsID))
            {
                var optionResult = _graph.Caches[typeof(AMConfigResultsOption)].LocateElseCopy((AMConfigResultsOption) result);
                var configFeature = (AMConfigurationFeature) result;
                var configOption = (AMConfigurationOption) result;
                if (optionResult?.ConfigResultsID == null || string.IsNullOrWhiteSpace(configOption?.Label) ||
                    !configOption.ResultsCopy.GetValueOrDefault() || string.IsNullOrWhiteSpace(configFeature?.Label))
                {
                    continue;
                }

                PXResult<AMConfigResultsOption, AMConfigurationOption, AMConfigurationFeature> fromOptionResult = null;
                if (!fromOptions.TryGetValue(MergeLabelsForKey(configFeature, configOption), out fromOptionResult))
                {
                    continue;
                }
                var optionResultFrom = (AMConfigResultsOption)fromOptionResult;
                if (optionResultFrom?.ConfigResultsID == null)
                {
                    continue;
                }

                optionResult.Included = optionResultFrom.Included;
                optionResult.ManualInclude = optionResultFrom.ManualInclude;
                optionResult.Available = optionResultFrom.Available;
                optionResult.Required = optionResultFrom.Required;
                var optionResultUpdated = (AMConfigResultsOption)_graph.Caches[typeof(AMConfigResultsOption)].Update(optionResult);

                if (!includeQtyEnabledOnly || (configOption.QtyEnabled.GetValueOrDefault() && optionResultUpdated.Included.GetValueOrDefault()))
                {
                    // Potential need to exclude set of qty is due to attribute formulas calculating the qty when not qty enabled
                    optionResultUpdated.Qty = optionResultFrom.Qty;
                    _graph.Caches[typeof(AMConfigResultsOption)].Update(optionResultUpdated);
                }
            }
        }

        protected virtual Dictionary<string, PXResult<AMConfigResultsAttribute, AMConfigurationAttribute>> GetAttributes(AMConfigurationResults configResult)
        {
            var dic = new Dictionary<string, PXResult<AMConfigResultsAttribute, AMConfigurationAttribute>>();
            foreach (PXResult<AMConfigResultsAttribute, AMConfigurationAttribute> result in PXSelectJoin<AMConfigResultsAttribute,
                InnerJoin<AMConfigurationAttribute,
                    On<AMConfigResultsAttribute.configurationID, Equal<AMConfigurationAttribute.configurationID>,
                        And<AMConfigResultsAttribute.revision, Equal<AMConfigurationAttribute.revision>,
                            And<AMConfigResultsAttribute.attributeLineNbr, Equal<AMConfigurationAttribute.lineNbr>>>>>,
                Where<AMConfigResultsAttribute.configResultsID, Equal<Required<AMConfigurationResults.configResultsID>>>>.Select(_graph, configResult.ConfigResultsID))
            {
                var attribute = (AMConfigurationAttribute) result;
                if (attribute == null || string.IsNullOrWhiteSpace(attribute.Label))
                {
                    continue;
                }

                dic.Add(attribute.Label, result);
            }
            return dic;
        }

        protected virtual Dictionary<string, PXResult<AMConfigResultsOption, AMConfigurationOption, AMConfigurationFeature>> GetOptions(AMConfigurationResults configResult)
        {
            var dic = new Dictionary<string, PXResult<AMConfigResultsOption, AMConfigurationOption, AMConfigurationFeature>>();
            foreach (PXResult<AMConfigResultsOption, AMConfigurationOption, AMConfigurationFeature> result in PXSelectJoin<AMConfigResultsOption,
                InnerJoin<AMConfigurationOption,
                    On<AMConfigResultsOption.configurationID, Equal<AMConfigurationOption.configurationID>,
                        And<AMConfigResultsOption.revision, Equal<AMConfigurationOption.revision>,
                            And<AMConfigResultsOption.featureLineNbr, Equal<AMConfigurationOption.configFeatureLineNbr>,
                                And<AMConfigResultsOption.optionLineNbr, Equal<AMConfigurationOption.lineNbr>>>>>,
                InnerJoin<AMConfigurationFeature,
                        On<AMConfigResultsOption.configurationID, Equal<AMConfigurationFeature.configurationID>,
                            And<AMConfigResultsOption.revision, Equal<AMConfigurationFeature.revision>,
                                And<AMConfigResultsOption.featureLineNbr, Equal<AMConfigurationFeature.lineNbr>>>>>>,
                Where<AMConfigResultsOption.configResultsID, Equal<Required<AMConfigResultsOption.configResultsID>>>>.Select(_graph, configResult?.ConfigResultsID))
            {
                var configFeature = (AMConfigurationFeature)result;
                var configOption = (AMConfigurationOption)result;
                if (configOption == null || string.IsNullOrWhiteSpace(configOption.Label) ||
                    configFeature == null || string.IsNullOrWhiteSpace(configFeature.Label))
                {
                    continue;
                }

                dic.Add(MergeLabelsForKey(configFeature, configOption), result);
            }
            return dic;
        }

        protected static string MergeLabelsForKey(AMConfigurationFeature feature, AMConfigurationOption option)
        {
            if (option == null || feature == null)
            {
                return string.Empty;
            }
            return $"{feature.Label.TrimIfNotNullEmpty()}{feature.LineNbr}{option.Label.TrimIfNotNullEmpty()}";
        }

        /// <summary>
        /// Get the latest configuration result by a config key
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="configKey">Configuration KEYID to lookup</param>
        /// <param name="configurationID">ConfigurationID the ConfigKey is related to</param>
        /// <param name="excludingConfigResultsID">Excluding a specific config results ID (optional)</param>
        public static AMConfigurationResults GetConfigResultByKey(PXGraph graph, string configKey, string configurationID, int? excludingConfigResultsID)
        {
            if (string.IsNullOrWhiteSpace(configKey))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(configurationID))
            {
                throw new PXArgumentException(nameof(configurationID));
            }

            // Configurations not yet saved will be a negative number - no need to look those up
            if (excludingConfigResultsID.GetValueOrDefault() <= 0)
            {
                return PXSelect<AMConfigurationResults,
                    Where<AMConfigurationResults.keyID, Equal<Required<AMConfigurationResults.keyID>>,
                        And<AMConfigurationResults.configurationID, Equal<Required<AMConfigurationResults.configurationID>>,
                        And<AMConfigurationResults.completed, Equal<True>>>>,
                    OrderBy<Desc<AMConfigurationResults.createdDateTime>>>.SelectWindowed(graph, 0, 1, configKey, configurationID);
            }

            return PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.keyID, Equal<Required<AMConfigurationResults.keyID>>,
                    And<AMConfigurationResults.configurationID, Equal<Required<AMConfigurationResults.configurationID>>,
                    And<AMConfigurationResults.completed, Equal<True>,
                    And<AMConfigurationResults.configResultsID, NotEqual<Required<AMConfigurationResults.configResultsID>>>>>>,
                OrderBy<Desc<AMConfigurationResults.createdDateTime>>>.SelectWindowed(graph, 0, 1, configKey, configurationID, excludingConfigResultsID);
        }
    }
}