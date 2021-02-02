using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Attribute levels
    /// </summary>
    public class AMAttributeLevels
    {
        /// <summary>
        /// BOM
        /// </summary>
        public const int BOM = 1;
        /// <summary>
        /// Operation
        /// </summary>
        public const int Operation = 2;
        /// <summary>
        /// Prod
        /// </summary>
        public const int Order = 3;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// BOM
            /// </summary>
            public static string BOM => Messages.GetLocal(Messages.BOM);

            /// <summary>
            /// Operation
            /// </summary>
            public static string Operation => Messages.GetLocal(Messages.Operation);

            /// <summary>
            /// Prod Order
            /// </summary>
            public static string Order => Messages.GetLocal(Messages.Order);
        }


        /// <summary>
        /// Attribute Levels BOM
        /// </summary>
        public class bOM : PX.Data.BQL.BqlInt.Constant<bOM>
        {
            public bOM() : base(BOM) { }
        }
        /// <summary>
        /// Attribute Levels Operation
        /// </summary>
        public class operation : PX.Data.BQL.BqlInt.Constant<operation>
        {
            public operation() : base(Operation) { }
        }

        /// <summary>
        /// Attribute Levels Prod Order
        /// </summary>
        public class order : PX.Data.BQL.BqlInt.Constant<order>
        {
            public order() : base(Order) { }
        }

        /// <summary>
        /// List for BOM Attributes
        /// </summary>
        public class BomListAttribute : PXIntListAttribute
        {
            public BomListAttribute()
                : base(
                new int[] { BOM, Operation },
                new string[] { Messages.BOM, Messages.Operation })
            { }
        }

        /// <summary>
        /// List for Production Order Attributes
        /// </summary>
        public class ProdOrderListAttribute : PXIntListAttribute
        {
            public ProdOrderListAttribute()
                : base(
                new int[] { Order, Operation},
                new string[] { Messages.Order, Messages.Operation})
            { }
        }
    }
}