using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Configuration Rule Types
    /// </summary>
    public class RuleTypes
    {
        public const string Include = "I";
        public const string Exclude = "E";
        public const string Require = "R";
        public const string Validate = "V";

        public static class Desc
        {
            public static string Include => Messages.GetLocal(Messages.Include);
            public static string Exclude => Messages.GetLocal(Messages.Exclude);
            public static string Require => Messages.GetLocal(Messages.Require);
            public static string Validate => Messages.GetLocal(Messages.Validate);

            public static string Get(string type)
            {
                switch(type)
                {
                    case RuleTypes.Include:
                        return RuleTypes.Desc.Include;
                    case RuleTypes.Exclude:
                        return RuleTypes.Desc.Exclude;
                    case RuleTypes.Require:
                        return RuleTypes.Desc.Require;
                    case RuleTypes.Validate:
                        return RuleTypes.Desc.Validate;
                    default:
                        return string.Empty;
                }
            }

            public static string Get(RuleType type)
            {
                return RuleTypes.Desc.Get(RuleTypes.Get(type));
            }
        }

        //BQL constants declaration
        public class include : PX.Data.BQL.BqlString.Constant<include>
        {
            public include() : base(Include) { }
        }
        public class exclude : PX.Data.BQL.BqlString.Constant<exclude>
        {
            public exclude() : base(Exclude) { }
        }
        public class require : PX.Data.BQL.BqlString.Constant<require>
        {
            public require() : base(Require) { }
        }

        public class validate : PX.Data.BQL.BqlString.Constant<validate>
        {
            public validate() : base(Validate) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { Include, Exclude, Require, Validate },
                    new string[] { Messages.Include, Messages.Exclude, Messages.Require, Messages.Validate }) { }
        }

        public class ListNoValidateAttribute : PXStringListAttribute
        {
            public ListNoValidateAttribute()
                : base(
                    new string[] { Include, Exclude, Require },
                    new string[] { Messages.Include, Messages.Exclude, Messages.Require })
            { }
        }

        public static string Get(RuleType ruleType)
        {
            switch(ruleType)
            {
                case RuleType.Include:
                    return Include;
                case RuleType.Exclude:
                    return Exclude;
                case RuleType.Require:
                    return Require;
                case RuleType.Validate:
                    return Validate;
                default:
                    throw new PXArgumentException();
            }
        }

        public static RuleType Get(string ruleType)
        {
            switch (ruleType)
            {
                case Include:
                    return RuleType.Include;
                case Exclude:
                    return RuleType.Exclude;
                case Require:
                    return RuleType.Require;
                case Validate:
                    return RuleType.Validate;
                default:
                    throw new PXArgumentException();
            }
        }
    }

    public enum RuleType
    {
        Require,
        Exclude,
        Include,
        Validate
    }
}