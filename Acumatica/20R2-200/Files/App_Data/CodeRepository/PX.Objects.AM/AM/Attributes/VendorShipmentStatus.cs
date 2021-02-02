using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class VendorShipmentStatus
    {
        public const string Open = "N";
        public const string Hold = "H";
        public const string Completed = "C";
        public const string Cancelled = "L";
        public const string Confirmed = "F";

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(Open, PX.Objects.SO.Messages.Open),
                    Pair(Hold, PX.Objects.SO.Messages.Hold),
                    Pair(Completed, PX.Objects.SO.Messages.Completed),
                    Pair(Cancelled, PX.Objects.SO.Messages.Cancelled),
                    Pair(Confirmed, PX.Objects.SO.Messages.Confirmed)
                })
            { }
        }

        public class open : PX.Data.BQL.BqlString.Constant<open>
        {
            public open() : base(Open) {; }
        }

        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) {; }
        }

        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(Completed) {; }
        }

        public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
        {
            public cancelled() : base(Cancelled) {; }
        }

        public class confirmed : PX.Data.BQL.BqlString.Constant<confirmed>
        {
            public confirmed() : base(Confirmed) {; }
        }
    }
}