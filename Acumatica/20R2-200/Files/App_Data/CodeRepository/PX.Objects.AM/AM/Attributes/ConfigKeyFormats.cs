using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Configuration Key Formats
    /// </summary>
    public class ConfigKeyFormats
    {
        public const string NoKey = "X";
        public const string Formula = "F";
        public const string NumberSequence = "N";

        public static class Desc
        {
            public static string NoKey => Messages.GetLocal(Messages.NoKey);
            public static string Formula => Messages.GetLocal(Messages.Formula);
            public static string NumberSequence => Messages.GetLocal(Messages.NumberSequence);
        }

        //BQL constants declaration
        public class noKey : PX.Data.BQL.BqlString.Constant<noKey>
        {
            public noKey() : base(NoKey) { }
        }
        public class formula : PX.Data.BQL.BqlString.Constant<formula>
        {
            public formula() : base(Formula) { }
        }
        public class numberSequence : PX.Data.BQL.BqlString.Constant<numberSequence>
        {
            public numberSequence() : base(NumberSequence) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { NoKey, Formula, NumberSequence },
                    new string[] { Messages.NoKey, Messages.Formula, Messages.NumberSequence }) { }
        }
    }
}