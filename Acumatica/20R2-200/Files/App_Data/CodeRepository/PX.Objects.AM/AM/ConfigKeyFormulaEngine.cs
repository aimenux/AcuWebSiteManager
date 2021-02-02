using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Engine for processing configuration keys and their related auto calculated/formula values
    /// </summary>
    internal class ConfigKeyFormulaEngine : AMRuleFormulaEngine
    {
        protected ConfigKeyFormulaEngine(PXGraph graph) : base(graph)
        {
        }

        public static void ProcessKeys(PXCache cache, bool testMode)
        {
            if (cache == null)
            {
                throw new PXArgumentException(nameof(cache));
            }

            AMConfigurationResults configResults = PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.configResultsID, Equal<Current<AMConfigurationResults.configResultsID>>>>.Select(cache.Graph);

            ProcessKeys(cache, configResults, testMode);
        }

        public static void ProcessKeys(PXCache cache, AMConfigurationResults configResults, bool testMode)
        {
            if (cache == null)
            {
                throw new PXArgumentException(nameof(cache));
            }

            if (configResults == null || configResults.ConfigResultsID == null)
            {
                return;
            }

            AMConfiguration configuration = PXSelect<AMConfiguration,
                Where<AMConfiguration.configurationID, Equal<Required<AMConfigurationResults.configurationID>>,
                    And<AMConfiguration.revision, Equal<Required<AMConfigurationResults.revision>>>>>.Select(cache.Graph, configResults.ConfigurationID, configResults.Revision);

            if (configuration == null || string.IsNullOrWhiteSpace(configuration.ConfigurationID))
            {
                return;
            }
            
            ConfigKeyFormulaEngine engine = null;

            var tranDescription = GetTranDescription(cache.Graph, configuration, ref engine);
            if (tranDescription != null && configResults.TranDescription == null
                || tranDescription != null && !tranDescription.Equals(configResults.TranDescription))
            {
                cache.SetValueExt<AMConfigurationResults.tranDescription>(configResults, tranDescription);
            }
            
            if (configuration.KeyFormat == ConfigKeyFormats.NoKey)
            {
                // Configuration not calculating key values... all done
                return;
            }

            var keyID = GetKeyId(cache.Graph, configResults, configuration, ref engine, testMode);
            if (keyID != null && configResults.KeyID == null
                || keyID != null && !keyID.Equals(configResults.KeyID))
            {
                cache.SetValueExt<AMConfigurationResults.keyID>(configResults, keyID);
            }

            if (engine == null)
            {
                engine = new ConfigKeyFormulaEngine(cache.Graph);
            }

            var keyDescription = AMFormulaInterpreter.GetFormulaStringValue(configuration.KeyDescription, engine.Container.FormulaDS);
            if (keyDescription != null && configResults.KeyDescription == null
                || keyDescription != null && !keyDescription.Equals(configResults.KeyDescription))
            {
                cache.SetValueExt<AMConfigurationResults.keyDescription>(configResults, keyDescription);
            }
        }

        protected static string GetKeyId(PXGraph graph, AMConfigurationResults configResults, AMConfiguration configuration, ref ConfigKeyFormulaEngine engine, bool testMode)
        {
            switch (configuration.KeyFormat)
            {
                case ConfigKeyFormats.NumberSequence:
                    return GetKeyIdByNumberSequence(graph, configResults, configuration, testMode);
                case ConfigKeyFormats.Formula:
                    return GetKeyIdByFormula(graph, configuration, ref engine);
            }

            return null;
        }

        protected static string GetKeyIdByNumberSequence(PXGraph graph, AMConfigurationResults configResults, AMConfiguration configuration, bool testMode)
        {
            //If request is coming from configuration entry we assume the configuration might have been changed so we will use a new key if the current key is already in use to preserve the original configuration.
            if (string.IsNullOrWhiteSpace(configResults.KeyID) || (graph is ConfigurationEntry && IsKeyIdInUse(graph, configResults)))
            {
                return testMode ? "TESTMODE" : AutoNumberAttribute.GetNextNumber(graph.Caches[typeof(AMConfiguration)], null, configuration.KeyNumberingID, graph.Accessinfo.BusinessDate);
            }
            return configResults.KeyID;
        }

        /// <summary>
        /// Determine if the given configuration key is in use by another finished configuration
        /// </summary>
        protected static bool IsKeyIdInUse(PXGraph graph, AMConfigurationResults configResults)
        {
            if (configResults == null || string.IsNullOrWhiteSpace(configResults.KeyID))
            {
                return false;
            }

            return (AMConfigurationResults)PXSelect<AMConfigurationResults,
                       Where<AMConfigurationResults.configResultsID, NotEqual<Required<AMConfigurationResults.configResultsID>>,
                           And<AMConfigurationResults.completed, Equal<True>,
                               And<AMConfigurationResults.keyID, Equal<Required<AMConfigurationResults.keyID>>>>>
                   >.SelectWindowed(graph, 0, 1, configResults.ConfigResultsID, configResults.KeyID) != null;
        }

        protected static string GetKeyIdByFormula(PXGraph graph, AMConfiguration configuration, ref ConfigKeyFormulaEngine engine)
        {
            if (engine == null)
            {
                engine = new ConfigKeyFormulaEngine(graph);
            }

            return AMFormulaInterpreter.GetFormulaStringValue(configuration.KeyEquation, engine.Container.FormulaDS);
        }

        protected static string GetTranDescription(PXGraph graph, AMConfiguration configuration, ref ConfigKeyFormulaEngine engine)
        {
            //Process Tran description here as the key description and key id are skipped if key format is set to no key
            if (string.IsNullOrWhiteSpace(configuration.TranDescription))
            {
                return null;
            }

            if (engine == null)
            {
                engine = new ConfigKeyFormulaEngine(graph);
            }

            return AMFormulaInterpreter.GetFormulaStringValue(configuration.TranDescription, engine.Container.FormulaDS);
        }

    }
}