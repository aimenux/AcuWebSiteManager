using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Configuration Rule Source
    /// </summary>
    public class RuleTargetSource
    {
        public const string Attribute = "A";
        public const string Feature = "F";
        public const string Option = "O";

        public static class Desc
        {
            public static string Attribute => Messages.GetLocal(Messages.Attribute);
            public static string Feature => Messages.GetLocal(Messages.Feature);
            public static string Option => Messages.GetLocal(Messages.Option);
        }

        //BQL constants declaration
        public class attribute : PX.Data.BQL.BqlString.Constant<attribute>
        {
            public attribute() : base(Attribute) { }
        }
        public class feature : PX.Data.BQL.BqlString.Constant<feature>
        {
            public feature() : base(Feature) { }
        }

        public class option : PX.Data.BQL.BqlString.Constant<option>
        {
            public option() : base(Option) { }
        }

        public class SourceListAttribute : PXStringListAttribute
        {
            public SourceListAttribute()
                : base(
                    new string[] { Attribute, Feature },
                    new string[] { Messages.Attribute, Messages.Feature }) { }
        }

        public class TargetListAttribute : PXStringListAttribute
        {
            public TargetListAttribute()
                : base(
                    new string[] { Attribute, Feature, Option },
                    new string[] { Messages.Attribute, Messages.Feature, Messages.Option })
            { }
        }
    }
}