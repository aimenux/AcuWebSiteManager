using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class PhantomRoutingOptions
    {
        /// <summary>
        /// Include phantom routing before parent routing
        /// </summary>
        public const int Before = 1;

        /// <summary>
        /// Include phantom routing after parent routing
        /// </summary>
        public const int After = 2;

        /// <summary>
        /// Exclude phantom routing
        /// </summary>
        public const int Exclude = 3;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Before => Messages.GetLocal(Messages.Before);
            public static string After => Messages.GetLocal(Messages.After);
            public static string Exclude => Messages.GetLocal(Messages.Exclude);
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(new int[]{Before, After, Exclude}
                , new string[]{ Messages.Before, Messages.After, Messages.Exclude}) { }
        }

        /// <summary>
        /// Include phantom routing before parent routing
        /// </summary>
        public class before : PX.Data.BQL.BqlInt.Constant<before>
        {
            public before() : base(Before) { ;}
        }

        /// <summary>
        /// Include phantom routing after parent routing
        /// </summary>
        public class after : PX.Data.BQL.BqlInt.Constant<after>
        {
            public after() : base(After) { ;}
        }

        /// <summary>
        /// Exclude phantom routing
        /// </summary>
        public class exclude : PX.Data.BQL.BqlInt.Constant<exclude>
        {
            public exclude() : base(Exclude) { ;}
        }
    }
}