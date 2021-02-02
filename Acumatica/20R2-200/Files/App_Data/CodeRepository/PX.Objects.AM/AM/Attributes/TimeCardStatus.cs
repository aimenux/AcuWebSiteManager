using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class TimeCardStatus
    {
        public const int Unprocessed = 0;
        public const int Processed = 1;
        public const int Skipped = 2;

        public class Desc
        {
            public static string Unprocessed => Messages.GetLocal(Messages.Unprocessed);
            public static string Processed => Messages.GetLocal(Messages.Processed);
            public static string Skipped => Messages.GetLocal(Messages.Skipped);
        }

        public class unprocessed : PX.Data.BQL.BqlInt.Constant<unprocessed>
        {
            public unprocessed() : base(Unprocessed) { }
        }

        public class processed : PX.Data.BQL.BqlInt.Constant<processed>
        {
            public processed() : base(Processed) { }
        }

        public class skipped : PX.Data.BQL.BqlInt.Constant<skipped>
        {
            public skipped() : base(Skipped) { }
        }


        /// <summary>
        /// UI list for production time card status
        /// </summary>
        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { Unprocessed, Processed, Skipped },
                new string[] { Messages.Unprocessed, Messages.Processed, Messages.Skipped})
            { }
        }
    }
}