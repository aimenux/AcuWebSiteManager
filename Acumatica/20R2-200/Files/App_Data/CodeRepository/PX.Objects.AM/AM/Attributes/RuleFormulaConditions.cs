using PX.Data;
using System.Linq;

namespace PX.Objects.AM.Attributes
{
    public class RuleFormulaConditions
    {
        public const string Equal = "E";
        public const string NotEqual = "NE";
        public const string Greater = "G";
        public const string GreaterEqual = "GE";
        public const string Less = "L";
        public const string LessEqual = "LE";
        public const string Between = "B";
        public const string Contains = "LI";
        public const string NotContains = "NL";
        public const string StartWith = "RL";
        public const string EndsWith = "LL";
        public const string Null = "NU";
        public const string NotNull = "NN";

        //Exclusive to AM (not in PXConditionlist)
        public const string Custom = "CC";
        public const string Even = "EV";
        public const string Odd = "OD";

        public static class Desc
        {
            public static string Equal => Messages.GetLocal(Messages.Equal);
            public static string NotEqual => Messages.GetLocal(Messages.NotEqual);
            public static string Greater => Messages.GetLocal(Messages.Greater);
            public static string GreaterEqual => Messages.GetLocal(Messages.GreaterEqual);
            public static string Less => Messages.GetLocal(Messages.Less);
            public static string LessEqual => Messages.GetLocal(Messages.LessEqual);
            public static string Between => Messages.GetLocal(Messages.Between);
            public static string Contains => Messages.GetLocal(Messages.Contains);
            public static string NotContains => Messages.GetLocal(Messages.NotContains);
            public static string StartWith => Messages.GetLocal(Messages.StartWith);
            public static string EndsWith => Messages.GetLocal(Messages.EndsWith);
            public static string Null => Messages.GetLocal(Messages.Null);
            public static string NotNull => Messages.GetLocal(Messages.NotNull);

            //Exclusive to AM (not in PXConditionlist)
            public static string Custom => Messages.GetLocal(Messages.Custom);
            public static string Even => Messages.GetLocal(Messages.Even);
            public static string Odd => Messages.GetLocal(Messages.Odd);

            public static string Get(string condition)
            {
                switch(condition)
                {
                    case RuleFormulaConditions.Equal:
                        return RuleFormulaConditions.Desc.Equal;
                    case RuleFormulaConditions.NotEqual:
                        return RuleFormulaConditions.Desc.NotEqual;
                    case RuleFormulaConditions.Greater:
                        return RuleFormulaConditions.Desc.Greater;
                    case RuleFormulaConditions.GreaterEqual:
                        return RuleFormulaConditions.Desc.GreaterEqual;
                    case RuleFormulaConditions.Less:
                        return RuleFormulaConditions.Desc.Less;
                    case RuleFormulaConditions.LessEqual:
                        return RuleFormulaConditions.Desc.LessEqual;
                    case RuleFormulaConditions.Between:
                        return RuleFormulaConditions.Desc.Between;
                    case RuleFormulaConditions.Contains:
                        return RuleFormulaConditions.Desc.Contains;
                    case RuleFormulaConditions.NotContains:
                        return RuleFormulaConditions.Desc.NotContains;
                    case RuleFormulaConditions.StartWith:
                        return RuleFormulaConditions.Desc.StartWith;
                    case RuleFormulaConditions.EndsWith:
                        return RuleFormulaConditions.Desc.EndsWith;
                    case RuleFormulaConditions.Null:
                        return RuleFormulaConditions.Desc.Null;
                    case RuleFormulaConditions.NotNull:
                        return RuleFormulaConditions.Desc.NotNull;
                    case RuleFormulaConditions.Custom:
                        return RuleFormulaConditions.Desc.Custom;
                    case RuleFormulaConditions.Even:
                        return RuleFormulaConditions.Desc.Even;
                    case RuleFormulaConditions.Odd:
                        return RuleFormulaConditions.Desc.Odd;
                    default:
                        return string.Empty;
                }
            }
        }
        
        private static readonly string[] RequiresValue1List = 
        {
            RuleFormulaConditions.Equal,
            RuleFormulaConditions.NotEqual,
            RuleFormulaConditions.Greater,
            RuleFormulaConditions.GreaterEqual,
            RuleFormulaConditions.Less,
            RuleFormulaConditions.LessEqual,
            RuleFormulaConditions.Between,
            RuleFormulaConditions.Contains,
            RuleFormulaConditions.NotContains,
            RuleFormulaConditions.StartWith,
            RuleFormulaConditions.EndsWith,
            RuleFormulaConditions.Custom
        };
        
        private static readonly string[] RequiresValue2List = 
        {
            RuleFormulaConditions.Between
        };

        public static bool RequiresValue1(string condition)
        {
            return RequiresValue1List.Contains(condition);
        }

        public static bool RequiresValue2(string condition)
        {
            return RequiresValue2List.Contains(condition);
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] 
                    {
                        RuleFormulaConditions.Custom,
                        RuleFormulaConditions.Equal,
                        RuleFormulaConditions.NotEqual,
                        RuleFormulaConditions.Greater,
                        RuleFormulaConditions.GreaterEqual,
                        RuleFormulaConditions.Less,
                        RuleFormulaConditions.LessEqual,
                        RuleFormulaConditions.Between,
                        RuleFormulaConditions.Contains,
                        RuleFormulaConditions.NotContains,
                        RuleFormulaConditions.StartWith,
                        RuleFormulaConditions.EndsWith,
                        RuleFormulaConditions.Null,
                        RuleFormulaConditions.NotNull,
                        RuleFormulaConditions.Even,
                        RuleFormulaConditions.Odd
                    },
                    new string[]
                    {
                        Messages.Custom,
                        Messages.Equal,
                        Messages.NotEqual,
                        Messages.Greater,
                        Messages.GreaterEqual,
                        Messages.Less,
                        Messages.LessEqual,
                        Messages.Between,
                        Messages.Contains,
                        Messages.NotContains,
                        Messages.StartWith,
                        Messages.EndsWith,
                        Messages.Null,
                        Messages.NotNull,
                        Messages.Even,
                        Messages.Odd
                    })
            { }
        }
    }
}