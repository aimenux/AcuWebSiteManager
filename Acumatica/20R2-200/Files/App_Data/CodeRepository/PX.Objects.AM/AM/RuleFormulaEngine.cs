using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM
{
    internal class AMRuleFormulaEngine
    {
        #region Members

        public RuleCacheContainer Container;

        #endregion

        #region CTOR

        protected AMRuleFormulaEngine(PXGraph graph)
        {
            Container = new RuleCacheContainer(graph);
        }

        protected AMRuleFormulaEngine(PXGraph graph, AMConfigResultsOption option, bool? included) : this(graph)
        {
            Container.UpdateOptionIncluded(option, included);
        }

        protected AMRuleFormulaEngine(PXGraph graph, AMConfigResultsAttribute attribute, string value) : this(graph)
        {
            Container.UpdateAttributeValue(attribute, value);
        }

        #endregion

        #region Run Processes

        public static RuleReturn Run(PXGraph graph)
        {
            return Run(new AMRuleFormulaEngine(graph));
        }
        public static RuleReturn Run(PXGraph graph, AMConfigResultsOption option, bool? included)
        {
            return Run(new AMRuleFormulaEngine(graph, option, included));
        }
        public static RuleReturn Run(PXGraph graph, AMConfigResultsAttribute attribute, string value)
        {
            return Run(new AMRuleFormulaEngine(graph, attribute, value));
        }

        private static RuleReturn Run(AMRuleFormulaEngine engine)
        {
            try
            {
                RunFormulaProcess(engine.Container);
                RunRulesProcess(engine.Container);
                return new RuleReturn(engine.Container);
            }
            catch (AMRuleException rex)
            {
                return new RuleReturn(rex.Message);
            }
        }

        #endregion

        #region Rules Processing

        private static void RunRulesProcess(RuleCacheContainer container, int cntr = 0)
        {
            if (cntr > 25)
                throw new AMRuleException(Messages.RuleRecursive);

            foreach (var rule in container.Rules[RuleType.Require])
            {
                container.GetRuleTargetOption(rule)?.SetRequired(rule.Validate(container));
            }

            foreach (var rule in container.Rules[RuleType.Exclude])
            {
                container.GetRuleTargetOption(rule)?.SetExcluded(rule.Validate(container));
            }

            foreach (var rule in container.Rules[RuleType.Include])
            {
                container.GetRuleTargetOption(rule)?.SetIncluded(rule.Validate(container), rule.Result.IsSoftRule == true);
            }

            foreach (var rule in container.Rules[RuleType.Validate])
            {
                container.GetRuleSourceValidate(rule)?.SetRuleValid(rule.Validate(container));
            }

            if (container.Update())
                RunRulesProcess(container, ++cntr);
        }

        #endregion

        #region Formulas

        // FormulaFields
        // 1. AMConfigFeature
        //   1. Min Selection
        //   2. Max Selection
        //   3. Min Qty
        //   4. Max Qty
        //   5. Lot Qty
        // 2. AMConfigurationOption
        //   1. Qty Required
        //   2. Min Qty
        //   3. Max Qty
        //   4. Lot Qty
        //   5. Scrap Factor
        //   6. Price Factor

        private static void RunFormulaProcess(RuleCacheContainer container)
        {
            RunFormulaAttributeProcess(container);

            var ds = container.FormulaDS;

            foreach (var feature in container.Features.Values)
            {
                feature.Result.MinSelection = AMFormulaInterpreter.GetFormulaIntValue(feature.Config.MinSelection, ds);
                feature.Result.MaxSelection = AMFormulaInterpreter.GetFormulaIntValue(feature.Config.MaxSelection, ds);
                feature.Result.MinQty = AMFormulaInterpreter.GetFormulaDecimalValue(feature.Config.MinQty, ds);
                feature.Result.MaxQty = AMFormulaInterpreter.GetFormulaDecimalValue(feature.Config.MaxQty, ds);
                feature.Result.LotQty = AMFormulaInterpreter.GetFormulaDecimalValue(feature.Config.LotQty, ds);
            }

            foreach (var featureOption in container.Options.Values)
            {
                foreach (var option in featureOption.Values)
                {
                    option.Result.QtyRequired = AMFormulaInterpreter.GetFormulaDecimalValue(option.Config.QtyRequired, ds) ?? (decimal?)0M;

                    if (AMFormulaInterpreter.IsFormula(option.Config.QtyRequired) 
                        || !option.Result.Included.GetValueOrDefault())
                    {
#if DEBUG
                        if(option.Result.Qty.GetValueOrDefault(-99) != UomHelper.QuantityRound(option.Result.QtyRequired.GetValueOrDefault()))
                        {
                            AMDebug.TraceWriteMethodName($"Changing Qty from {option.Result.Qty.GetValueOrDefault(-99)} to {option.Result.QtyRequired.GetValueOrDefault()} [Feature {option.Result.FeatureLineNbr.GetValueOrDefault()}, Option {option.Result.OptionLineNbr.GetValueOrDefault()}]");
                        }
#endif
                        // Some cases the "Qty" field never gets set with a value.
                        // The issue occurs as only some (not all rows) fire the field updated event on QtyRequired even though the rows contain an updated value. (ref bug 1082)
                        option.Result.Qty = option.Result.QtyRequired.GetValueOrDefault();
                    }

                    option.Result.MinQty = AMFormulaInterpreter.GetFormulaDecimalValue(option.Config.MinQty, ds);
                    option.Result.MaxQty = AMFormulaInterpreter.GetFormulaDecimalValue(option.Config.MaxQty, ds);
                    option.Result.LotQty = AMFormulaInterpreter.GetFormulaDecimalValue(option.Config.LotQty, ds);
                    option.Result.ScrapFactor = AMFormulaInterpreter.GetFormulaDecimalValue(option.Config.ScrapFactor, ds) ?? 0m;
                    option.Result.PriceFactor = AMFormulaInterpreter.GetFormulaDecimalValue(option.Config.PriceFactor, ds) ?? 1m;
                }
            }
        }

        private static void RunFormulaAttributeProcess(RuleCacheContainer container, int cntr = 0)
        {
            if (cntr > 25)
                throw new AMRuleException(Messages.RuleAttributeFormula);

            foreach (var attribute in container.Attributes.Values)
                if (attribute.Config.IsFormula == true)
                    attribute.SetValue(AMFormulaInterpreter.GetFormulaStringValue(attribute.Config.Value, container.FormulaDS));

            if (container.UpdateAttributes())
                RunFormulaAttributeProcess(container, ++cntr);
        }

        #endregion
    }

    internal interface IRuleProcessor
    {
        void AddRulesHandlers();
        void RemoveRulesHandlers();
    }

    internal interface IRuleConditionValidator
    {
        bool ValidateOption(AMConfigResultsOption option, out string message);
        bool ValidateAttribute(AMConfigResultsAttribute attribute, out string message);
        bool ValidateFeature(AMConfigResultsFeature feature, out string message);
    }

    internal class AMRuleFormulaScope : IDisposable
    {
        private IRuleProcessor _Processor;
        private bool IsNested { get; }

        public AMRuleFormulaScope(IRuleProcessor processor)
        {
            _Processor = processor;
            IsNested = PXContext.GetSlot<AMRuleFormulaScope>() != null;
            if (!IsNested)
            {
                _Processor.RemoveRulesHandlers();
                PXContext.SetSlot<AMRuleFormulaScope>(this);
            }
        }
        public void Dispose()
        {
            if (!IsNested)
            {
                _Processor.AddRulesHandlers();
                PXContext.SetSlot<AMRuleFormulaScope>(null);
            }

        }
    }

    [Serializable]
    internal class AMRuleException : PXException
    {
        public AMRuleException(string message) : base(message) { }
    }

    internal class RuleSummary
    {
        public bool IsTestMode { get; }
        public bool IsError { get; }
        private string _ErrorMessage;

        public PXGraph Graph { get; }
        public List<AMConfigResultsRule> Rules { get; }
        public List<RuleDescription> ActiveRules { get; private set; }
        public List<RuleDescription> UnactiveRules { get; private set; }

        #region CTOR

        private RuleSummary(PXGraph graph, bool isTestMode)
        {
            IsTestMode = isTestMode;
            Graph = graph;
            ActiveRules = new List<RuleDescription>();
            UnactiveRules = new List<RuleDescription>();
        }

        private RuleSummary(PXGraph graph, AMConfigResultsOption option, IRuleConditionValidator validator, bool isTestMode) : this(graph, isTestMode)
        {
            Rules = GetOptionRules(graph, option).ToList();
            this.IsError = !validator.ValidateOption(option, out this._ErrorMessage);
        }

        private RuleSummary(PXGraph graph, AMConfigResultsAttribute attribute, IRuleConditionValidator validator, bool isTestMode) : this(graph, isTestMode)
        {
            Rules = GetAttributeRules(graph, attribute).ToList();
            this.IsError = !validator.ValidateAttribute(attribute, out this._ErrorMessage);
        }

        #endregion

        #region Get

        private static string GetRuleDescription(RuleSummary ruleSummary)
        {
            return ruleSummary.GetRuleDescription();
        }

        public static string GetRuleDescription(PXGraph graph, AMConfigResultsOption option, IRuleConditionValidator validator, bool isTestMode)
        {
            return GetRuleDescription(new RuleSummary(graph, option, validator, isTestMode));
        }

        public static string GetRuleDescription(PXGraph graph, AMConfigResultsAttribute attribute, IRuleConditionValidator validator, bool isTestMode)
        {
            return GetRuleDescription(new RuleSummary(graph, attribute, validator, isTestMode));
        }

        private static string GetFullDescription(RuleSummary ruleSummary)
        {
            return ruleSummary.GetDescription();
        }

        public static string GetFullDescription(PXGraph graph, AMConfigResultsOption option, IRuleConditionValidator validator, bool isTestMode)
        {
            return GetFullDescription(new RuleSummary(graph, option, validator, isTestMode));
        }

        public static string GetFullDescription(PXGraph graph, AMConfigResultsAttribute attribute, IRuleConditionValidator validator, bool isTestMode)
        {
            return GetFullDescription(new RuleSummary(graph, attribute, validator, isTestMode));
        }

        private static PXSetPropertyException GetException(RuleSummary summary)
        {
            var message = summary.GetDescription();

            if (string.IsNullOrEmpty(message))
            {
                return null;
            }
            else
            {
                return new PXSetPropertyException(message, summary.IsError
                                                    ? PXErrorLevel.RowWarning 
                                                    : PXErrorLevel.RowInfo);
            }
        }

        public static PXSetPropertyException GetException(PXGraph graph, AMConfigResultsOption option, IRuleConditionValidator validator, bool isTestMode)
        {
            return GetException(new RuleSummary(graph, option, validator, isTestMode));
        }

        public static PXSetPropertyException GetException(PXGraph graph, AMConfigResultsAttribute attribute, IRuleConditionValidator validator, bool isTestMode)
        {
            return GetException(new RuleSummary(graph, attribute, validator, isTestMode));
        }

        #endregion

        #region Create Descriptions

        private void BuildRuleDescriptions()
        {
            ActiveRules = new List<RuleDescription>();
            UnactiveRules = new List<RuleDescription>();

            foreach (var rule in Rules)
            {
                var ruleDescription = RuleDescription.Create(this.Graph, rule);

                if (ruleDescription.IsValid)
                    ActiveRules.Add(ruleDescription);
                else
                    UnactiveRules.Add(ruleDescription);
            }
        }

        public string GetDescription()
        {
            var description = string.Empty;

            AggregateMessages(ref description, GetErrorDescription());
            AggregateMessages(ref description, GetRuleDescription());

            return description;
        }

        public string GetErrorDescription()
        {
            if(!string.IsNullOrEmpty(_ErrorMessage))
            {
                return string.Format("{1}: {2} {0}", System.Environment.NewLine, Messages.GetLocal(Messages.Error), _ErrorMessage);
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetRuleDescription()
        {
            BuildRuleDescriptions();

            var description = string.Empty;
            if (IsTestMode)
            {
                AggregateMessages(ref description, GetRuleListDescription(Messages.GetLocal(Messages.ActiveRules), this.ActiveRules));
                AggregateMessages(ref description, GetRuleListDescription(Messages.GetLocal(Messages.UnactiveRules), this.UnactiveRules));
            }
            else
            {
                description = GetRuleListDescription(Messages.GetLocal(Messages.Rules), this.ActiveRules);
            }

            return description;
        }

        private string GetRuleListDescription(string title, List<RuleDescription> rules)
        {
            var ret = string.Empty;
            if (rules.Any())
            {
                ret += string.Format("{0}: {1}", title, System.Environment.NewLine);
                foreach (var ruleDescription in rules)
                {
                    ret += string.Format("{0}  • {1}: {2} {0}", System.Environment.NewLine, ruleDescription.GetRuleTypeDescription(), ruleDescription.ToString());
                }
            }

            return ret;
        }

        public void AggregateMessages(ref string message, string newMessage)
        {
            if (string.IsNullOrEmpty(message))
                message += newMessage;
            else
                message += string.Format("{0} {1}", System.Environment.NewLine, newMessage);
        }

        #endregion

        #region Data

        private static IEnumerable<AMConfigResultsRule> GetOptionRules(PXGraph graph, AMConfigResultsOption option)
        {
            foreach (AMConfigResultsRule rule in PXSelect<AMConfigResultsRule,
                                                    Where<AMConfigResultsRule.ruleTarget,
                                                        Equal<RuleTargetSource.option>,
                                                    And<AMConfigResultsRule.configResultsID,
                                                        Equal<Required<AMConfigResultsRule.configResultsID>>, 
                                                    And<AMConfigResultsRule.targetLineNbr,
                                                        Equal<Required<AMConfigResultsRule.targetLineNbr>>,
                                                    And<AMConfigResultsRule.targetSubLineNbr,
                                                        Equal<Required<AMConfigResultsRule.targetSubLineNbr>>>>>>>
                                                    .Select(graph, option.ConfigResultsID, option.FeatureLineNbr, option.OptionLineNbr))
            {
                yield return rule;
            }
        }

        private static IEnumerable<AMConfigResultsRule> GetAttributeRules(PXGraph graph, AMConfigResultsAttribute attribute)
        {
            foreach (AMConfigResultsRule rule in PXSelect<AMConfigResultsRule,
                                                    Where<AMConfigResultsRule.ruleTarget,
                                                        Equal<RuleTargetSource.attribute>,
                                                    And<AMConfigResultsRule.configResultsID,
                                                        Equal<Required<AMConfigResultsRule.configResultsID>>,
                                                    And<AMConfigResultsRule.targetLineNbr,
                                                        Equal<Required<AMConfigResultsRule.targetLineNbr>>>>>>
                                                    .Select(graph, attribute.ConfigResultsID, attribute.AttributeLineNbr))
            {
                yield return rule;
            }
        }

        #endregion
    }

    internal class RuleDescription
    {
        public PXGraph Graph { get; }
        public bool IsValid { get; }
        public AMConfigResultsRule Rule { get; }

        private RuleDescription(PXGraph graph, AMConfigResultsRule rule)
        {
            Graph = graph;
            IsValid = rule.RuleValid == true;
            Rule = rule;
        }

        private string BuildDescription()
        {
            switch (this.Rule.RuleSource)
            {
                case RuleTargetSource.Attribute:
                    return BuildAttributeDescription();
                case RuleTargetSource.Feature:
                    return BuildFeatureDescription();
                default:
                    return string.Empty;
            }
        }

        private string BuildAttributeDescription()
        {
            var attribute = (AMConfigurationAttribute)PXSelect<AMConfigurationAttribute,
                                            Where<AMConfigurationAttribute.configurationID,
                                                Equal<Required<AMConfigurationAttribute.configurationID>>,
                                            And<AMConfigurationAttribute.revision,
                                                Equal<Required<AMConfigurationAttribute.revision>>,
                                            And<AMConfigurationAttribute.lineNbr,
                                                Equal<Required<AMConfigurationAttribute.lineNbr>>>>>>
                                            .Select(this.Graph, this.Rule.ConfigurationID, this.Rule.Revision, this.Rule.RuleSourceLineNbr);

            var condition = this.Rule.Condition.TrimIfNotNull();
            if (RuleFormulaConditions.RequiresValue2(condition))
            {
                return Messages.GetLocal(Messages.RuleAttributeValue2, 
                                    this.Rule.CalcValue, 
                                    attribute.Label,
                                    RuleFormulaConditions.Desc.Get(condition),
                                    this.Rule.CalcValue1,
                                    this.Rule.CalcValue2);
            }

            if (RuleFormulaConditions.RequiresValue1(condition))
            {
                return Messages.GetLocal(Messages.RuleAttributeValue1,
                    this.Rule.CalcValue,
                    attribute.Label,
                    RuleFormulaConditions.Desc.Get(condition),
                    this.Rule.CalcValue1);
            }

            return Messages.GetLocal(Messages.RuleAttributeNoValue,
                this.Rule.CalcValue,
                attribute.Label,
                RuleFormulaConditions.Desc.Get(condition));
        }

        private string BuildFeatureDescription()
        {
            var feature = (AMConfigurationFeature)PXSelect<AMConfigurationFeature,
                                            Where<AMConfigurationFeature.configurationID,
                                                Equal<Required<AMConfigurationFeature.configurationID>>,
                                            And<AMConfigurationFeature.revision,
                                                Equal<Required<AMConfigurationFeature.revision>>,
                                            And<AMConfigurationFeature.lineNbr,
                                                Equal<Required<AMConfigurationFeature.lineNbr>>>>>>
                                            .Select(this.Graph, this.Rule.ConfigurationID, this.Rule.Revision, this.Rule.RuleSourceLineNbr);

            int optionLineNbr;
            if (int.TryParse(this.Rule.CalcValue, out optionLineNbr))
            {
                var option = (AMConfigurationOption)PXSelect<AMConfigurationOption,
                                                        Where<AMConfigurationOption.configurationID,
                                                            Equal<Required<AMConfigurationOption.configurationID>>,
                                                        And<AMConfigurationOption.revision,
                                                            Equal<Required<AMConfigurationOption.revision>>,
                                                        And<AMConfigurationOption.configFeatureLineNbr,
                                                            Equal<Required<AMConfigurationOption.configFeatureLineNbr>>,
                                                        And<AMConfigurationOption.lineNbr,
                                                            Equal<Required<AMConfigurationOption.lineNbr>>>>>>>
                                                        .Select(this.Graph, this.Rule.ConfigurationID, this.Rule.Revision, this.Rule.RuleSourceLineNbr, optionLineNbr);

                if(this.Rule.RuleValid == true)
                {
                    return Messages.GetLocal(Messages.RuleFeatureOptionIncluded, option.Label, feature.Label);
                }
                else
                {
                    return Messages.GetLocal(Messages.RuleFeatureOptionNotIncluded, option.Label, feature.Label);
                }
            }
            else
            {
                if (this.Rule.RuleValid == true)
                {
                    return Messages.GetLocal(Messages.RuleFeatureAnyOptionIncluded, feature.Label);
                }
                else
                {
                    return Messages.GetLocal(Messages.RuleFeatureAnyOptionNotIncluded, feature.Label);
                }
            }
        }

        public static RuleDescription Create(PXGraph graph, AMConfigResultsRule rule)
        {
            return new RuleDescription(graph, rule);
        }

        public override string ToString()
        {
            return BuildDescription();
        }

        public string GetRuleTypeDescription()
        {
            return RuleTypes.Desc.Get(this.Rule.RuleType);
        }
    }

    internal class RuleReturn
    {
        public bool IsValid { get; }

        public string Message { get; }

        public IEnumerable<AMConfigResultsFeature> FeaturesToUpdate { get; }
        public IEnumerable<AMConfigResultsOption> OptionsToUpdate { get; }
        public IEnumerable<AMConfigResultsAttribute> AttributesToUpdate { get; }
        public IEnumerable<AMConfigResultsRule> RulesToUpdate { get; }


        public RuleReturn(string message)
        {
            IsValid = false;
            Message = message;
        }

        public RuleReturn(RuleCacheContainer cacheContainer)
        {
            IsValid = true;
            FeaturesToUpdate = cacheContainer.GetFeaturesToUpdate();
            OptionsToUpdate = cacheContainer.GetOptionsToUpdate();
            AttributesToUpdate = cacheContainer.GetAttributesToUpdate();
            RulesToUpdate = cacheContainer.GetRulesToUpdate();
        }
    }

    internal class RuleCacheContainer
    {
        public RuleCacheDictionary<int?, RuleFeatureCache> Features;
        public RuleCacheDictionary<int?, int?, RuleOptionCache> Options;
        public RuleCacheDictionary<int?, RuleAttributeCache> Attributes;

        public RuleDictionary Rules;

        public Dictionary<string, object> FormulaDS
        {
            get
            {
                // Attribute variables are optional... only want those with variables
                return Attributes.Where(pair => !string.IsNullOrWhiteSpace(pair.Value.Config.Variable))
                    .ToDictionary(pair => pair.Value.Config.Variable, pair => AMFormulaInterpreter.SanitizeFormulaValue(pair.Value.Result.Value));
            }
        }

        #region CTOR / Initializer 

        public RuleCacheContainer(PXGraph graph)
        {
            Features = new RuleCacheDictionary<int?, RuleFeatureCache>();
            Options = new RuleCacheDictionary<int?, int?, RuleOptionCache>();
            Attributes = new RuleCacheDictionary<int?, RuleAttributeCache>();
            Rules = new RuleDictionary();

#if DEBUG
            var sbDebug = new System.Text.StringBuilder();
            sbDebug.AppendLine("RuleCacheContainer process times:");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
#endif
                SetFeaturesDictionary(graph);
#if DEBUG
                var lastElapsed = sw.Elapsed;
                sbDebug.AppendLine(PXTraceHelper.CreateTimespanMessage(lastElapsed, "SetFeaturesDictionary: AMConfigResultsFeature + AMConfigurationFeature").Indent(1));
#endif
                SetOptionsDictionary(graph);
#if DEBUG
                sbDebug.AppendLine(PXTraceHelper.CreateTimespanMessage(sw.Elapsed - lastElapsed, "SetOptionsDictionary: AMConfigResultsOption + AMConfigurationOption").Indent(1));
                lastElapsed = sw.Elapsed;
#endif
                SetAttributesDictionary(graph);
#if DEBUG
                sbDebug.AppendLine(PXTraceHelper.CreateTimespanMessage(sw.Elapsed - lastElapsed, "SetAttributesDictionary: AMConfigResultsAttribute + AMConfigurationAttribute").Indent(1));
                lastElapsed = sw.Elapsed;
#endif
                SetRulesDictionary(graph);
#if DEBUG
                sbDebug.AppendLine(PXTraceHelper.CreateTimespanMessage(sw.Elapsed - lastElapsed, "SetRulesDictionary: AMConfigurationResults + AMConfigResultsRule").Indent(1));
            }
            finally
            {
                sw.Stop();
                sbDebug.AppendLine(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "Total").Indent(1));
                PXTrace.WriteInformation(sbDebug.ToString());
                AMDebug.TraceWriteMethodName(sbDebug.ToString());
            }
#endif
        }

        /// <summary>
        /// Setting Dictionaries for <see cref="AMConfigResultsFeature"/> and <see cref="AMConfigurationFeature"/>
        /// Manually merge the Config vs Results records as the old query would call sub query to each config record.
        /// </summary>
        private void SetFeaturesDictionary(PXGraph graph)
        {
            var configFeatures = new Dictionary<string, AMConfigurationFeature>();
            foreach (AMConfigurationFeature configFeature in PXSelect<
                    AMConfigurationFeature,
                    Where<AMConfigurationFeature.configurationID, Equal<Current<AMConfigurationResults.configurationID>>,
                        And<AMConfigurationFeature.revision, Equal<Current<AMConfigurationResults.revision>>>>>
                .Select(graph))
            {
                if (configFeature?.ConfigurationID == null)
                {
                    continue;
                }
                var key = string.Join(":", configFeature.ConfigurationID, configFeature.Revision, configFeature.LineNbr);
                configFeatures[key] = configFeature.CreateCopy();
            }

            foreach (AMConfigResultsFeature resultFeature in
                PXSelect<AMConfigResultsFeature,
                    Where<AMConfigResultsFeature.configResultsID,
                        Equal<Current<AMConfigurationResults.configResultsID>>>>.Select(graph))
            {
                if (resultFeature?.FeatureLineNbr == null)
                {
                    continue;
                }

                var key = string.Join(":", resultFeature.ConfigurationID, resultFeature.Revision, resultFeature.FeatureLineNbr);
                if (!configFeatures.TryGetValue(key, out var configFeature) || configFeature?.LineNbr == null)
                {
                    continue;
                }

                Features[resultFeature.FeatureLineNbr] = new RuleFeatureCache(resultFeature.CreateCopy(), configFeature);
            }
        }

        /// <summary>
        /// Setting Dictionaries for <see cref="AMConfigResultsRule"/> and <see cref="AMConfigurationRule"/>
        /// Manually merge the Config vs Results records as the old query would call sub query to each config record.
        /// </summary>
        private void SetRulesDictionary(PXGraph graph)
        {
            var configRules = new Dictionary<string, AMConfigurationRule>();
            foreach (AMConfigurationRule configRule in PXSelect<
                    AMConfigurationRule,
                    Where<AMConfigurationRule.configurationID, Equal<Current<AMConfigurationResults.configurationID>>,
                        And<AMConfigurationRule.revision, Equal<Current<AMConfigurationResults.revision>>>>>
                .Select(graph))
            {
                if (configRule?.ConfigurationID == null)
                {
                    continue;
                }
                var key = string.Join(":", configRule.ConfigurationID, configRule.Revision, configRule.RuleSource, configRule.SourceLineNbr, configRule.LineNbr);
                configRules[key] = configRule;
            }

            foreach (AMConfigResultsRule resultRule in
                PXSelect<AMConfigResultsRule,
                    Where<AMConfigResultsRule.configResultsID,
                        Equal<Current<AMConfigurationResults.configResultsID>>>>.Select(graph))
            {
                if (resultRule?.RuleLineNbr == null)
                {
                    continue;
                }

                var key = string.Join(":", resultRule.ConfigurationID, resultRule.Revision, resultRule.RuleSource, resultRule.RuleSourceLineNbr, resultRule.RuleLineNbr);
                if (!configRules.TryGetValue(key, out var configRule) || configRule?.LineNbr == null)
                {
                    continue;
                }

                Rules[RuleTypes.Get(resultRule.RuleType)].Add(new RuleExecutionCache(resultRule, configRule));
            }
        }

        /// <summary>
        /// Setting Dictionaries for <see cref="AMConfigResultsAttribute"/> and <see cref="AMConfigurationAttribute"/>
        /// Manually merge the Config vs Results records as the old query would call sub query to each config record.
        /// </summary>
        private void SetAttributesDictionary(PXGraph graph)
        {
            var configAttributes = new Dictionary<string, AMConfigurationAttribute>();
            foreach (AMConfigurationAttribute configAttribute in PXSelect<
                    AMConfigurationAttribute,
                    Where<AMConfigurationAttribute.configurationID, Equal<Current<AMConfigurationResults.configurationID>>,
                        And<AMConfigurationAttribute.revision, Equal<Current<AMConfigurationResults.revision>>>>>
                .Select(graph))
            {
                if (configAttribute?.ConfigurationID == null)
                {
                    continue;
                }
                var key = string.Join(":", configAttribute.ConfigurationID, configAttribute.Revision, configAttribute.LineNbr);
                configAttributes[key] = configAttribute.CreateCopy();
            }

            foreach (AMConfigResultsAttribute resultAttribute in
                PXSelect<AMConfigResultsAttribute,
                    Where<AMConfigResultsAttribute.configResultsID,
                        Equal<Current<AMConfigurationResults.configResultsID>>>>.Select(graph))
            {
                if (resultAttribute?.AttributeLineNbr == null)
                {
                    continue;
                }

                var key = string.Join(":", resultAttribute.ConfigurationID, resultAttribute.Revision, resultAttribute.AttributeLineNbr);
                if (!configAttributes.TryGetValue(key, out var configAttribute) || configAttribute?.LineNbr == null)
                {
                    continue;
                }

                Attributes[resultAttribute.AttributeLineNbr] = new RuleAttributeCache(resultAttribute.CreateCopy(), configAttribute);
            }
        }

        /// <summary>
        /// Setting Dictionaries for <see cref="AMConfigResultsOption"/> and <see cref="AMConfigurationOption"/>
        /// Manually merge the Config vs Results records as the old query would call sub query to each config record.
        /// </summary>
        private void SetOptionsDictionary(PXGraph graph)
        {
            var configOptions = new Dictionary<string, AMConfigurationOption>();
            foreach (AMConfigurationOption configOption in PXSelect<
                    AMConfigurationOption,
                    Where<AMConfigurationOption.configurationID, Equal<Current<AMConfigurationResults.configurationID>>,
                        And<AMConfigurationOption.revision, Equal<Current<AMConfigurationResults.revision>>>>>
                .Select(graph))
            {
                if (configOption?.ConfigurationID == null)
                {
                    continue;
                }
                var key = string.Join(":", configOption.ConfigurationID, configOption.Revision, configOption.ConfigFeatureLineNbr, configOption.LineNbr);
                configOptions[key] = configOption.CreateCopy();
            }

            foreach (AMConfigResultsOption resultOption in
                PXSelect<AMConfigResultsOption,
                    Where<AMConfigResultsOption.configResultsID,
                        Equal<Current<AMConfigurationResults.configResultsID>>>>.Select(graph))
            {
                if (resultOption?.OptionLineNbr == null)
                {
                    continue;
                }

                var key = string.Join(":", resultOption.ConfigurationID, resultOption.Revision, resultOption.FeatureLineNbr, resultOption.OptionLineNbr);
                if (!configOptions.TryGetValue(key, out var configOption) || configOption?.LineNbr == null)
                {
                    continue;
                }

                Options[resultOption.FeatureLineNbr][resultOption.OptionLineNbr] = new RuleOptionCache(resultOption.CreateCopy(), configOption);
            }
        }

        public void UpdateAttributeValue(AMConfigResultsAttribute attribute, string value)
        {
            var copy = PXCache<AMConfigResultsAttribute>.CreateCopy(attribute);
            copy.Value = value;
            if (copy.AttributeLineNbr == null)
            {
                return;
            }
            try
            {
                this.Attributes[copy.AttributeLineNbr].InitResult(copy, true);
            }
            catch (KeyNotFoundException knfe)
            {
                AMDebug.TraceException(knfe);
            }
        }

        public void UpdateOptionIncluded(AMConfigResultsOption option, bool? included)
        {
            var copy = PXCache<AMConfigResultsOption>.CreateCopy(option);
            copy.Included = included;
            if (Options.TryGetValue(copy.FeatureLineNbr, copy.OptionLineNbr, out var cacheReturn))
            {
                cacheReturn.InitResult(copy, true);
            }
        }

        #endregion

        #region Target/Source/Value Getters

        public RuleOptionCache GetRuleTargetOption(RuleExecutionCache rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (rule.Result == null)
            {
                throw new ArgumentNullException(nameof(rule.Result));
            }

            if (rule.Result.RuleTarget != RuleTargetSource.Option)
            {
                throw new PXException(Messages.RuleTargetnotAnOption);
            }

            if (Options.TryGetValue(rule.Result.TargetLineNbr, rule.Result.TargetSubLineNbr, out var cacheReturn))
            {
                return cacheReturn;
            }

            PXTrace.WriteWarning($"[KeyNotFound-RuleIgnored] Unable to find target option '{rule.Result.TargetLineNbr},{rule.Result.TargetSubLineNbr}' for configuration {rule.Result.ConfigurationID} revision {rule.Result.Revision}.");
            return null;
        }

        public RuleCache GetRuleSourceValidate(RuleExecutionCache rule)
        {
            switch(rule.Result.RuleTarget)
            {
                case RuleTargetSource.Attribute:
                    return Attributes[rule.Result.RuleSourceLineNbr];
                case RuleTargetSource.Feature:
                    return Features[rule.Result.RuleSourceLineNbr];
                default:
                    throw new PXException();
            }
        }

        public string GetAttributeSourceValue(RuleExecutionCache rule)
        {
            return Attributes[rule.Result.RuleSourceLineNbr].Result.Value;
        }

        public IEnumerable<int?> GetFeatureIncludedOptions(RuleExecutionCache rule)
        {
            foreach (var option in Options[rule.Result.RuleSourceLineNbr].Values)
            {
                if (option.Result.Included == true)
                {
                    yield return option.Result.OptionLineNbr;
                }
            }
        }

        #endregion

        #region Update

        public bool Update()
        {
            return UpdateFeatures() || UpdateOptions() || UpdateAttributes() || UpdateRules();
        }

        public bool UpdateFeatures()
        {
            var reRunRules = false;
            foreach (var feature in Features.Values)
                reRunRules |= feature.Update();
            return reRunRules;
        }

        public bool UpdateOptions()
        {
            var reRunRules = false;
            foreach (var featureOption in Options.Values)
                foreach (var option in featureOption.Values)
                    reRunRules |= option.Update();
            return reRunRules;
        }

        public bool UpdateAttributes()
        {
            var reRunRules = false;
            foreach (var attribute in Attributes.Values)
                reRunRules |= attribute.Update();
            return reRunRules;
        }

        public bool UpdateRules()
        {
            var reRunRules = false;
            foreach (var ruleType in Rules.Values)
                foreach (var rule in ruleType)
                    reRunRules |= rule.Update();
            return reRunRules;
        }

        public IEnumerable<AMConfigResultsFeature> GetFeaturesToUpdate()
        {
            foreach (var feature in this.Features.Values)
            {
                if (feature.IsDirty)
                {
                    yield return feature.Result;
                }
            }
        }

        public IEnumerable<AMConfigResultsOption> GetOptionsToUpdate()
        {
            foreach (var featureOption in this.Options.Values)
            {
                foreach (var option in featureOption.Values)
                {
                    if (option.IsDirty)
                    {
                        yield return option.Result;
                    }
                }
            }
        }

        public IEnumerable<AMConfigResultsAttribute> GetAttributesToUpdate()
        {
            foreach (var option in this.Attributes.Values)
            {
                if (option.IsDirty)
                {
                    yield return option.Result;
                }
            }
        }

        public IEnumerable<AMConfigResultsRule> GetRulesToUpdate()
        {
            foreach (var ruleType in this.Rules.Values)
            {
                foreach (var rule in ruleType)
                {
                    if (rule.IsDirty)
                    {
                        yield return rule.Result;
                    }
                }
            }
        }

        #endregion
    }

    #region Rule Cache Container

    internal abstract class RuleCache
    {
        public bool IsValid { get; private set; }

        public RuleCache()
        {
            InitializeRuleProcess();
        }

        public virtual void InitializeRuleProcess()
        {
            IsValid = true;
        }

        public virtual bool SetRuleValid(bool isValid)
        {
            if (!isValid)
                this.IsValid = false;
            return this.IsValid;
        }
    }

    internal abstract class RuleCache<TResult, TConfig> : RuleCache
        where TResult : class, IBqlTable, IRuleValid, new()
        where TConfig : class, IBqlTable, new()
    {
        public bool IsMainItem { get; private set; }

        public TResult OriginalResult { get; private set; }

        public TResult Result { get; set; }
        public TConfig Config { get; }
        
        public bool IsDirty
        {
            get
            {
                return IsUpdated();
            }
        }

        public RuleCache(TResult result, TConfig config)
        {
            InitResult(result, false);
            this.Config = config;
        }

        public abstract bool IsUpdated(TResult original, TResult current);
        //Return either we need to rerun the rules or not
        protected abstract bool UpdateResult();

        public bool IsUpdated()
        {
            return IsUpdated(OriginalResult, Result);
        }

        public bool Update()
        {
            var reRunRules = UpdateResult();
            InitializeRuleProcess();
            return reRunRules;
        }

        public virtual void InitResult(TResult result, bool isMainItem)
        {
            this.IsMainItem = isMainItem;

            this.OriginalResult = PXCache<TResult>.CreateCopy(result);
            this.Result = result;
        }
    }

    internal class RuleOptionCache : RuleCache<AMConfigResultsOption, AMConfigurationOption>
    {
        public bool? ForceInclude { get; private set; }

        public bool IsRequired { get; private set; }

        public bool IsExcluded { get; private set; }

        public bool IsHardIncluded { get; private set; }
        public bool IsSoftIncluded { get; private set; }

        public RuleOptionCache(AMConfigResultsOption result, AMConfigurationOption config) : base(result, config) { }

        public override bool IsUpdated(AMConfigResultsOption original, AMConfigResultsOption current)
        {
            return original.QtyRequired != current.QtyRequired ||
                   original.MinQty != current.MinQty ||
                   original.MaxQty != current.MaxQty ||
                   original.LotQty != current.LotQty ||
                   original.ScrapFactor != current.ScrapFactor ||
                   original.PriceFactor != current.PriceFactor ||
                   original.Required != current.Required ||
                   original.ManualInclude != current.ManualInclude ||
                   original.Included != current.Included ||
                   original.Available != current.Available ||
                   original.RuleValid != current.RuleValid;
        }

        protected override bool UpdateResult()
        {
            var isUserInclude = Result.ManualInclude == true || Result.FixedInclude == true;
            var isRequired = IsRequired;
            var isIncluded = this.ForceInclude.HasValue 
                                              ? this.ForceInclude.Value
                                              : isUserInclude || IsHardIncluded || !IsExcluded && IsSoftIncluded;
            var isExcluded = IsExcluded;
            var isValid = !((isExcluded && isIncluded) || (isRequired && isExcluded));
            var includedHasChanges = Result.Included != isIncluded;

            Result.Required = IsRequired;
            Result.Included = isIncluded;
            Result.Available = !isExcluded;
            Result.RuleValid = isValid;

            return includedHasChanges;
        }

        public override void InitializeRuleProcess()
        {
            base.InitializeRuleProcess();

            IsRequired = false;

            IsExcluded = false;

            IsHardIncluded = false;
            IsSoftIncluded = false;
        }

        public bool SetRequired(bool isRequired)
        {
            if (isRequired)
                this.IsRequired = true;
            return this.IsRequired;
        }

        public bool SetExcluded(bool isExcluded)
        {
            if (isExcluded)
                this.IsExcluded = true;
            return this.IsExcluded;
        }

        public bool SetIncluded(bool isIncluded, bool isSoftRule)
        {
            if (isSoftRule)
            {
                if (isIncluded)
                    this.IsSoftIncluded = true;
                return this.IsSoftIncluded;
            }
            else
            {
                if (isIncluded)
                    this.IsHardIncluded = true;
                return this.IsHardIncluded;
            }
        }

        public override void InitResult(AMConfigResultsOption result, bool isMainItem)
        {
            base.InitResult(result, isMainItem);

            if (isMainItem)
                this.ForceInclude = result.Included;
            else
                this.ForceInclude = null;
        }
    }

    internal class RuleFeatureCache : RuleCache<AMConfigResultsFeature, AMConfigurationFeature>
    {
        public RuleFeatureCache(AMConfigResultsFeature result, AMConfigurationFeature config) : base(result, config) { }

        public override bool IsUpdated(AMConfigResultsFeature original, AMConfigResultsFeature current)
        {
            return original.MinSelection != current.MinSelection ||
                   original.MaxSelection != current.MaxSelection ||
                   original.MinQty != current.MinQty ||
                   original.MaxQty != current.MaxQty ||
                   original.LotQty != current.LotQty ||
                   original.RuleValid != current.RuleValid;
        }

        protected override bool UpdateResult()
        {
            Result.RuleValid = base.IsValid;
            return false;
        }
    }

    internal class RuleAttributeCache : RuleCache<AMConfigResultsAttribute, AMConfigurationAttribute>
    {
        public bool IsSetValue { get; private set; }

        public string Value { get; private set; }

        public RuleAttributeCache(AMConfigResultsAttribute result, AMConfigurationAttribute config) : base(result, config) { }

        public override bool IsUpdated(AMConfigResultsAttribute original, AMConfigResultsAttribute current)
        {
            return original.Value != current.Value ||
                   original.RuleValid != current.RuleValid;
        }

        public string SetValue(string value)
        {
            this.IsSetValue = true;
            this.Value = value;

            return this.Value;
        }

        protected override bool UpdateResult()
        {
            var valueHasChanged = this.IsSetValue && this.Value != Result.Value;

            if (valueHasChanged)
                Result.Value = this.Value;

            Result.RuleValid = base.IsValid;
            return valueHasChanged;
        }

        public override void InitializeRuleProcess()
        {
            base.InitializeRuleProcess();

            this.IsSetValue = false;
            this.Value = string.Empty;
        }
    }

    internal class RuleExecutionCache : RuleCache<AMConfigResultsRule, AMConfigurationRule>
    {
        public string CalcValue { get; private set; }
        public string CalcValue1 { get; private set; }
        public string CalcValue2 { get; private set; }

        public RuleExecutionCache(AMConfigResultsRule result, AMConfigurationRule config) : base(result, config) { }

        public override bool IsUpdated(AMConfigResultsRule original, AMConfigResultsRule current)
        {
            return original.CalcValue != current.CalcValue ||
                   original.CalcValue1 != current.CalcValue1 ||
                   original.CalcValue2 != current.CalcValue2 ||
                   original.RuleValid != current.RuleValid;
        }

        protected override bool UpdateResult()
        {
            Result.CalcValue = this.CalcValue;
            Result.CalcValue1 = this.CalcValue1;
            Result.CalcValue2 = this.CalcValue2;
            Result.RuleValid = base.IsValid;
            return false;
        }

        public override void InitializeRuleProcess()
        {
            base.InitializeRuleProcess();

            CalcValue = string.Empty;
            CalcValue1 = string.Empty;
            CalcValue2 = string.Empty;
        }

        public bool Validate(RuleCacheContainer container)
        {
            var isValid = false;
            if (this.Result.RuleSource == RuleTargetSource.Feature)
            {
                int option;
                if (int.TryParse(this.Config.Value1, out option))
                {
                    foreach (var includedOption in container.GetFeatureIncludedOptions(this))
                    {
                        isValid |= option == includedOption;
                    }

                    this.CalcValue = this.Config.Value1;
                }
                else
                {
                    //Any Source is selected
                    if (this.Result.RuleSourceLineNbr == this.Result.TargetLineNbr)
                        isValid = true; //Self Referencing
                    else
                        isValid = container.GetFeatureIncludedOptions(this).Any();
                }
            }
            else
            {
                this.CalcValue = container.GetAttributeSourceValue(this);
                this.CalcValue1 = AMFormulaInterpreter.GetFormulaStringValue(this.Config.Value1, container.FormulaDS);
                this.CalcValue2 = AMFormulaInterpreter.GetFormulaStringValue(this.Config.Value2, container.FormulaDS);

                isValid = AMFormulaInterpreter.ValidateCondition(this.Config.Condition,
                                                                 this.Config.Value1,
                                                                 this.Config.Value2,
                                                                 container.FormulaDS,
                                                                 this.CalcValue);
            }

            this.SetRuleValid(isValid);

            return isValid;
        }

        public override bool SetRuleValid(bool isValid)
        {
            if (this.Result.RuleType == RuleTypes.Validate)
                isValid = !isValid;
            return base.SetRuleValid(isValid);
        }
    }

    #endregion

    #region Rule Dictionaries

    internal class RuleCacheDictionary<TKey, TCache> : Dictionary<TKey, TCache>
        where TCache : RuleCache
    {
    }

    internal class RuleCacheDictionary<TKey1, TKey2, TCache> : Dictionary<TKey1, RuleCacheDictionary<TKey2, TCache>>
        where TCache : RuleCache
    {
        public new RuleCacheDictionary<TKey2, TCache> this[TKey1 key]
        {
            get
            {
                RuleCacheDictionary<TKey2, TCache> dict;
                if (!this.TryGetValue(key, out dict))
                {
                    dict = new RuleCacheDictionary<TKey2, TCache>();
                    this.Add(key, dict);
                }
                return dict;
            }
            set
            {
                base[key] = value;
            }
        }

        public bool TryGetValue(TKey1 key1, TKey2 key2, out TCache outValue)
        {
            outValue = null;
            if (key1 == null || key2 == null)
            {
                return false;
            }
            var dic1 = this[key1];
            return dic1 != null && dic1.TryGetValue(key2, out outValue);
        }

        //public bool TryGetValue(TKey1 key1, TKey2 key2, out TCache outValue)
        //{
        //    outValue = null;
        //    if (this.TryGetValue(key1, out var key1Value))
        //    {
        //        return key1Value != null && key1Value.TryGetValue(key2, out outValue);
        //    }

        //    return false;
        //}
    }

    internal class RuleDictionary : Dictionary<RuleType, List<RuleExecutionCache>>
    {
        public RuleDictionary()
        {
            this.Add(RuleType.Exclude, new List<RuleExecutionCache>());
            this.Add(RuleType.Include, new List<RuleExecutionCache>());
            this.Add(RuleType.Require, new List<RuleExecutionCache>());
            this.Add(RuleType.Validate, new List<RuleExecutionCache>());
        }
    }

    #endregion
}
