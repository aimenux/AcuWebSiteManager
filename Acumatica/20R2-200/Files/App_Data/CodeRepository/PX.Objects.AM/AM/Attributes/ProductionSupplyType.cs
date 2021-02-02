using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Prodction Supply Types 
    /// </summary>
    public class ProductionSupplyType
    {
        /// <summary>
        /// Inventory
        /// </summary>
        public const int Inventory = 1;
        /// <summary>
        /// Production
        /// </summary>
        public const int Production = 2;
        /// <summary>
        /// SalesOrder
        /// </summary>
        public const int SalesOrder = 3;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// Inventory
            /// </summary>
            public static string Inventory => Messages.GetLocal(Messages.Inventory);

            /// <summary>
            /// Production
            /// </summary>
            public static string Production => Messages.GetLocal(Messages.Production);

            /// <summary>
            /// SalesOrder
            /// </summary>
            public static string SalesOrder => Messages.GetLocal(Messages.SalesOrder);
        }

        /// <summary>
        /// Production Supply Type Inventory
        /// </summary>
        public class inventory : PX.Data.BQL.BqlInt.Constant<inventory>
        {
            public inventory() : base(Inventory) { }
        }
        /// <summary>
        /// Production Supply Type Production
        /// </summary>
        public class production : PX.Data.BQL.BqlInt.Constant<inventory>
        {
            public production() : base(Production) { }
        }

        /// <summary>
        /// Production Supply Type Sales Order
        /// </summary>
        public class salesOrder : PX.Data.BQL.BqlInt.Constant<salesOrder>
        {
            public salesOrder() : base(SalesOrder) { }
        }

        /// <summary>
        /// List for Production Supply Types
        /// </summary>
        public class SupplyTypeListAttribute : PXIntListAttribute
        {
            public SupplyTypeListAttribute()
                : base(
                new int[] { Inventory, Production, SalesOrder },
                new string[] { Messages.Inventory, Messages.Production, Messages.SalesOrder })
            { }
        }
    }
}