using PX.Data;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    internal static class UnitCostHelper
    {
        public class UnitCostPair
        {
            public decimal? unitCost;
            public decimal? curyUnitCost;

            public UnitCostPair(decimal? unitCost, decimal? curyUnitCost)
            {
                this.unitCost = unitCost;
                this.curyUnitCost = curyUnitCost;
            }
        }

        public static UnitCostPair CalculateCuryUnitCost<unitCostField, inventoryIDField, uomField>(PXCache cache, object row, bool raiseUnitCostDefaulting, decimal? unitCost)
            where unitCostField : IBqlField
            where inventoryIDField : IBqlField
            where uomField : IBqlField
        {
            decimal curyUnitCost = 0m;

            if (raiseUnitCostDefaulting == true)
            {
                object unitCostObj;
                cache.RaiseFieldDefaulting<unitCostField>(row, out unitCostObj);
                unitCost = (decimal?)unitCostObj;
            }

            if (unitCost != null && unitCost != 0m)
            {
                decimal valueConvertedToBase = INUnitAttribute.ConvertToBase<inventoryIDField, uomField>(cache, row, unitCost.Value, INPrecision.NOROUND);

                IPXCurrencyHelper currencyHelper = cache.Graph.FindImplementation<IPXCurrencyHelper>();

                if (currencyHelper != null)
                {
                    currencyHelper.CuryConvCury(unitCost.Value, out valueConvertedToBase);
                }
                else
                {
                    CM.PXDBCurrencyAttribute.CuryConvCury(cache, row, valueConvertedToBase, out valueConvertedToBase, true);
                }

                curyUnitCost = Math.Round(valueConvertedToBase, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
            }

            return new UnitCostPair(curyUnitCost, unitCost);
        }
    }
}
