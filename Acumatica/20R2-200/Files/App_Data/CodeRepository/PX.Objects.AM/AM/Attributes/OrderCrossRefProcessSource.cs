using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class OrderCrossRefProcessSource
    {
        public const int ProductionMaint = 1;
        public const int MRP = 2;
        public const int SalesOrder = 3;
        public const int CriticalMaterial = 4;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string ProductionMaint => Messages.GetLocal(Messages.ProductionMaint);
            public static string MRP => Messages.GetLocal(Messages.MRP);
            public static string SalesOrder => Messages.GetLocal(Messages.SalesOrder);
            public static string CriticalMaterial => Messages.GetLocal(Messages.CriticalMaterial);
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(new int[]
			{
			    ProductionMaint, 
                MRP, 
                SalesOrder,
                CriticalMaterial
			},
                new string[]
			{
                Messages.ProductionMaint,
                Messages.MRP,
                Messages.SalesOrder,
                Messages.CriticalMaterial
			})
            {
            }
        }
    }
}