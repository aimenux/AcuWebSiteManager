using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common.Parser;

namespace PX.Objects.AM.Attributes
{
    public class AMFormulaInterpreter : ExpressionParser
    {
        #region Implementation 

        private Dictionary<string, object> _DataSource;

        AMFormulaInterpreter(Dictionary<string, object> dataSource, string text)
            : base(text)
        {
            _DataSource = dataSource;
        }

        protected override ExpressionContext CreateContext()
        {
            return new AMPCExpressionContext(_DataSource);
        }

        protected override NameNode CreateNameNode(ExpressionNode node, string tokenString)
        {
            return new NameNode(node, tokenString, Context);
        }

        protected override void ValidateName(NameNode node, string tokenString)
        {
            throw new NotImplementedException();
        }
        protected override FunctionNode CreateFunctionNode(ExpressionNode node, string name)
        {
            var n = node as NameNode;
            return base.CreateFunctionNode(node, name);
        }
        protected override bool IsAggregate(string nodeName)
        {
            return false;
        }

        #endregion

        #region Parsers

        public static string GetFormulaStringValue(string formula, Dictionary<string, object> ds)
        {
            if (string.IsNullOrEmpty(formula))
                return string.Empty;

            var result = AMFormulaInterpreter.Evaluate(ds, formula);

            if (result != null)
                return result.ToString();

            return string.Empty;
        }

        public static bool? GetFormulaBoolValue(string formula, Dictionary<string, object> ds)
        {
            if (string.IsNullOrEmpty(formula))
                return null;

            var result = AMFormulaInterpreter.Evaluate(ds, formula);

            bool ret;
            if (result is bool)
                return (bool)result;
            else if (bool.TryParse(result?.ToString(), out ret))
                return ret;
            else
                return null;
        }

        public static int? GetFormulaIntValue(string formula, Dictionary<string, object> ds)
        {
            if (string.IsNullOrEmpty(formula))
                return null;

            var result = AMFormulaInterpreter.Evaluate(ds, formula);

            int ret;
            if (result is int)
                return (int)result;
            else if (int.TryParse(result?.ToString(), out ret))
                return ret;
            else
                return null;
        }

        public static decimal? GetFormulaDecimalValue(string formula, Dictionary<string, object> ds)
        {
            if (string.IsNullOrEmpty(formula))
                return null;

            var result = AMFormulaInterpreter.Evaluate(ds, formula);

            decimal ret;
            if (result is decimal)
                return (decimal)result;
            else if (decimal.TryParse(result?.ToString(), out ret))
                return ret;
            else
                return null;
        }

        #endregion

        #region Conditions

        //If the condition is in this list, we try to cast the result of the formula as bool.
        private static readonly string[] FormulaConditions =
        {
            RuleFormulaConditions.Equal,
            RuleFormulaConditions.NotEqual,
            RuleFormulaConditions.Greater,
            RuleFormulaConditions.GreaterEqual,
            RuleFormulaConditions.Less,
            RuleFormulaConditions.LessEqual,
            RuleFormulaConditions.Between,
            RuleFormulaConditions.Null,
            RuleFormulaConditions.NotNull,
            RuleFormulaConditions.Custom,
            RuleFormulaConditions.Even,
            RuleFormulaConditions.Odd
        };

        //If the condition is in this list, we want to calculate the formula on the value and do the validation manually.
        private static readonly string[] FormulaValue =
        {
            RuleFormulaConditions.Contains,
            RuleFormulaConditions.NotContains,
            RuleFormulaConditions.StartWith,
            RuleFormulaConditions.EndsWith
        };


        public enum ConditionType
        {
            FormulaConditions,
            FormulaValue
        }

        public static ConditionType GetConditionType(string condition)
        {
            if (FormulaValue.Contains(condition))
                return ConditionType.FormulaValue;
            else
                return ConditionType.FormulaConditions;
        }

        private const string OrdinalFieldVar = "ORDINAL_FIELD";
        public static bool ValidateCondition(string condition, string value1, string value2, Dictionary<string, object> ds, object ordValue)
        {
            if (ordValue == null) return false;

            var conditionType = GetConditionType(condition);

            if (ds == null)
                ds = new Dictionary<string, object>();

            ds.Add(OrdinalFieldVar, ordValue);

            if (conditionType == ConditionType.FormulaConditions)
                return CalculateConditionFormulaCondition(condition, value1, value2, ds);
            else
                return CalculateConditionFormulaValue(condition, value1, value2, ds);
        }

        private static bool CalculateConditionFormulaCondition(string condition, string value1, string value2, Dictionary<string, object> ds)
        {
            var formula = GetConditionFormulaCondition(condition, value1, value2);
            return GetFormulaBoolValue(formula, ds) ?? false;
        }

        private static string GetConditionFormulaCondition(string condition, string value1, string value2)
        {
            switch (condition.TrimIfNotNull())
            {
                case RuleFormulaConditions.Custom: //Custom Condition
                    return value1;
                case RuleFormulaConditions.Odd: //Is Odd
                    return String.Format("=CInt({0})%2 <> 0", OrdinalFieldVar);
                case RuleFormulaConditions.Even: //Is Even
                    return String.Format("=CInt({0})%2 = 0", OrdinalFieldVar);
                case RuleFormulaConditions.NotNull: //Is Not Null
                    return String.Format("={0} <> Null", OrdinalFieldVar);
                case RuleFormulaConditions.Null: //Is Null
                    return String.Format("={0} = Null", OrdinalFieldVar);
                case RuleFormulaConditions.Between: //Is Between
                    return String.Format("={1} <= {0} And {0} <= {2}", OrdinalFieldVar,
                                                                       AMFormulaInterpreter.SanitizeFormula(value1),
                                                                       AMFormulaInterpreter.SanitizeFormula(value2));
                case RuleFormulaConditions.LessEqual: //Is Less Than or Equal To
                    return String.Format("={0} <= {1}", OrdinalFieldVar, AMFormulaInterpreter.SanitizeFormula(value1));
                case RuleFormulaConditions.Less: //Is Less Than
                    return String.Format("={0} < {1}", OrdinalFieldVar, AMFormulaInterpreter.SanitizeFormula(value1));
                case RuleFormulaConditions.GreaterEqual: //Is Greater Than or Equal To
                    return String.Format("={0} >= {1}", OrdinalFieldVar, AMFormulaInterpreter.SanitizeFormula(value1));
                case RuleFormulaConditions.Greater: //Is Greater Than
                    return String.Format("={0} > {1}", OrdinalFieldVar, AMFormulaInterpreter.SanitizeFormula(value1));
                case RuleFormulaConditions.NotEqual: //Does Not Equal
                    return String.Format("={0} <> {1}", OrdinalFieldVar, AMFormulaInterpreter.SanitizeFormula(value1));
                case RuleFormulaConditions.Equal:  //Equals
                default:
                    return String.Format("={0} = {1}", OrdinalFieldVar, AMFormulaInterpreter.SanitizeFormula(value1));
            }
        }

        private static bool CalculateConditionFormulaValue(string condition, string value1, string value2, Dictionary<string, object> ds)
        {
            var ordinalValue = GetFormulaStringValue(OrdinalFieldVar, ds);
            var compareValue = GetFormulaStringValue(value1, ds);

            switch (condition.TrimIfNotNull())
            {
                case RuleFormulaConditions.Contains: //Contains
                    return ordinalValue.Contains(compareValue);
                case RuleFormulaConditions.NotContains: //Does Not Contain
                    return !ordinalValue.Contains(compareValue);
                case RuleFormulaConditions.StartWith: //Starts With
                    return ordinalValue.StartsWith(compareValue);
                case RuleFormulaConditions.EndsWith: //Ends With
                    return ordinalValue.EndsWith(compareValue);
                default:
                    return false;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Is the given value a field formula
        /// </summary>
        /// <param name="value">formula field value</param>
        /// <returns>True when value is a formula</returns>
        public static bool IsFormula(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.StartsWith("=");
        }

        private static object Evaluate(Dictionary<string, object> dataSource, string formula)
        {
            formula = SanitizeFormula(formula);

            var expr = new AMFormulaInterpreter(dataSource, formula);
            try
            {
                var node = expr.Parse();
                return node.Eval(null);
            }
            catch (ParserException)
            {
                return null;
            }
            catch
            {
                throw;
            }
        }

        public static string SanitizeFormula(string formula)
        {
            if (IsFormula(formula))
            {
                formula = formula.Substring(1);
            }
            return formula;
        }


        //We work on the premise that more often than not, 
        //attribute values are going to be numbers 
        public static object SanitizeFormulaValue(string value)
        {
            //If no value is set. We return 0 decimal
            if (string.IsNullOrEmpty(value))
                return Decimal.Zero;

            //If it's possible to parse the value as decimal
            //we do it so that we can do math operation (1+2 = 3)
            //instead of text operation (1+2 = 12)
            decimal decValue;
            if (Decimal.TryParse(value, out decValue))
                return decValue;

            //If it's not a number, we will consider it's a text field.
            return value;
        }

        public class AMPCExpressionContext : ExpressionContext
        {
            public AMPCExpressionContext(Dictionary<string, object> dataSource)
            {
                foreach (var property in dataSource)
                    RegisterProperty(property.Key, property.Value);
            }
        }

        /// <summary>
        /// Does the variable exist within the formula?
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="variable"></param>
        /// <returns>True when found</returns>
        public static bool FormulaContainsVariable(string formula, string variable)
        {
            if (string.IsNullOrWhiteSpace(variable) || !IsFormula(formula))
            {
                return false;
            }

            var variablesInFormulaHash = ExtractFormulaVariables(formula);
            return variablesInFormulaHash != null && variablesInFormulaHash.Contains(variable);
        }

        protected static char VariableSeperatorStart = '[';
        protected static char VariableSeperatorEnd = ']';

        /// <summary>
        /// pull out the variables existing in the given formula
        /// </summary>
        public static HashSet<string> ExtractFormulaVariables(string formula)
        {
            var hash = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(formula))
            {
                return hash;
            }

            var variableSb = new System.Text.StringBuilder();
            var processVariable = false;
            foreach (var c in formula.ToCharArray())
            {
                if (c == VariableSeperatorStart)
                {
                    processVariable = true;
                    continue;
                }

                if (c == VariableSeperatorEnd)
                {
                    if (variableSb.Length > 0)
                    {
                        hash.Add(variableSb.ToString());
                    }

                    variableSb.Clear();
                    processVariable = false;
                    continue;
                }

                if (!processVariable)
                {
                    continue;
                }

                variableSb.Append(c);
            }

            return hash;
        }

        #endregion
    }
}
