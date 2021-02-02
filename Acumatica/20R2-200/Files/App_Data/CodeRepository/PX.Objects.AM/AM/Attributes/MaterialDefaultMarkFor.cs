using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class MaterialDefaultMarkFor
    {
        public const int NoDefault = 0;
        public const int Purchase = 1;
        public const int Production = 2;
        
        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string NoDefault => Messages.GetLocal(Messages.NoDefault);
            public static string Purchase => Messages.GetLocal(Messages.Purchase);
            public static string Production => Messages.GetLocal(Messages.Production);
        }

        public static bool IsNoDefault(int? markForType)
        {
            return markForType == null || (markForType != Purchase && markForType != Production);
        }

        /// <summary>
        /// Return the functions user friendly description
        /// </summary>
        /// <param name="function">function ID</param>
        /// <returns>OrderTypeFunction.Desc value (same as the list drop down)</returns>
        public static string GetDescription(int? markForType)
        {
            if (markForType == null)
            {
                return Messages.Unknown;
            }

            return new StockItemListAttribute().ValueLabelDic[markForType.GetValueOrDefault()];
        }

        public class StockItemListAttribute : PXIntListAttribute
        {
            public StockItemListAttribute()
                : base(
                    new[] { 
                        NoDefault,
                        Purchase,
                        Production
                    },
                    new[] {
                        Messages.NoDefault,
                        Messages.Purchase,
                        Messages.Production,
                    })
            {
            }
        }

        public class NonStockItemListAttribute : PXIntListAttribute
        {
            public NonStockItemListAttribute()
                : base(
                    new[] { 
                        NoDefault,
                        Purchase
                    },

                    new[] {
                        Messages.NoDefault,
                        Messages.Purchase
                    })
            {
            }
        }

        public class noDefault : PX.Data.BQL.BqlInt.Constant<noDefault>
        {
            public noDefault() : base(NoDefault) {; }
        }

        public class purchase : PX.Data.BQL.BqlInt.Constant<purchase>
        {
            public purchase() : base(Purchase) {; }
        }

        public class production : PX.Data.BQL.BqlInt.Constant<production>
        {
            public production() : base(Production) {; }
        }
    }
}