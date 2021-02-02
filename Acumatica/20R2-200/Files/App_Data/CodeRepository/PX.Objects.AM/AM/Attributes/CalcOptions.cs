using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Configuration Calculate Options
    /// </summary>
    public class CalcOptions
    {
        public const string OnCompletion = "C";
        public const string AfterSelection = "S";

        public static class Desc
        {
            public static string OnCompletion => Messages.GetLocal(Messages.OnCompletion);
            public static string AfterSelection => Messages.GetLocal(Messages.AfterSelection);
        }

        //BQL constants declaration
        public class onCompletion : PX.Data.BQL.BqlString.Constant<onCompletion>
        {
            public onCompletion() : base(OnCompletion) { ;}
        }
        public class afterSelection : PX.Data.BQL.BqlString.Constant<afterSelection>
        {
            public afterSelection() : base(AfterSelection) { ;}
        }
        
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { OnCompletion, AfterSelection },
                    new string[] { Messages.OnCompletion, Messages.AfterSelection }){ }
        }
    }
}