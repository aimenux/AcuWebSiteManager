using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// MRP Process Exception Types
    /// </summary>
    public class MRPExceptionType
    {
        /// <summary>
        /// Defer - Supply due prior to the promise date
        /// </summary>
        public const int Defer = 1;

        /// <summary>
        /// Delete - Supply not required for any demand
        /// </summary>
        public const int Delete = 2;

        /// <summary>
        /// Expedite - Supply due beyond the promise date 
        /// </summary>
        public const int Expedite = 3;

        /// <summary>
        /// Late - Supply item should have been received
        /// </summary>
        public const int Late = 4;

        /// <summary>
        /// Transfer Available - Supply item has inventory beyond site requirements
        /// </summary>
        public const int Transfer = 5;

        /// <summary>
        /// Order on Hold - The order is not included in the MRP Processing due to the order being on hold and MRP Setup option excluding the order type when on hold
        /// </summary>
        public const int OrderOnHold = 6;

        public class Desc
        {
            public static string Defer => AM.Messages.GetLocal(AM.Messages.MRPExceptionTypeDefer);
            public static string Delete => AM.Messages.GetLocal(PX.Objects.EP.Messages.Delete);
            public static string Expedite => AM.Messages.GetLocal(AM.Messages.MRPExceptionTypeExpedite);
            public static string Late => AM.Messages.GetLocal(AM.Messages.MRPExceptionTypeLate);
            public static string Transfer => AM.Messages.GetLocal(AM.Messages.MRPExceptionTypeTransfer);
            public static string OrderOnHold => AM.Messages.GetLocal(AM.Messages.MRPExceptionTypeOrderOnHold);
        }

        public class defer : PX.Data.BQL.BqlInt.Constant<defer>
        {
            public defer() : base(Defer) { }
        }
        public class delete : PX.Data.BQL.BqlInt.Constant<delete>
        {
            public delete() : base(Delete) { }
        }
        public class expedite : PX.Data.BQL.BqlInt.Constant<expedite>
        {
            public expedite() : base(Expedite) { }
        }
        public class late : PX.Data.BQL.BqlInt.Constant<late>
        {
            public late() : base(Late) { }
        }
        public class transfer : PX.Data.BQL.BqlInt.Constant<transfer>
        {
            public transfer() : base(Transfer) { }
        }
        public class orderOnHold : PX.Data.BQL.BqlInt.Constant<orderOnHold>
        {
            public orderOnHold() : base(OrderOnHold) { }
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                    new []
                    {
                        Defer,
                        Delete,
                        Expedite,
                        Late,
                        Transfer,
                        OrderOnHold
                    },
                    new []
                    {
                        Messages.MRPExceptionTypeDefer,
                        EP.Messages.Delete,
                        Messages.MRPExceptionTypeExpedite,
                        Messages.MRPExceptionTypeLate,
                        Messages.MRPExceptionTypeTransfer,
                        Messages.MRPExceptionTypeOrderOnHold
                    })
            {
            }
        }
    }
}