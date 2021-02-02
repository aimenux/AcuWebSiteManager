using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Currency Helper class
    /// </summary>
    public static class CurrencyHelper
    {
        private const short DefaultCuryDecPlaces = 2;

        /// <summary>
        /// Get the current base currency decimal places
        /// </summary>
        public static int GetBaseCuryDecimalPlaces(PXGraph graph)
        {
            if (graph == null)
            {
                return DefaultCuryDecPlaces;
            }

            //Multiple calls on the same graph will not hit the DB each time - the query is cached and the row is reused
            var currency = (Currency)PXSelectJoin<
                Currency, 
                InnerJoin<PX.Objects.GL.Company, 
                    On<PX.Objects.GL.Company.baseCuryID, Equal<Currency.curyID>>>>
                .Select(graph);

            return currency?.DecimalPlaces ?? DefaultCuryDecPlaces;
        }

        public static decimal? ConvertFromToCury(PXGraph graph, long? toCuryInfoID, long? fromCuryInfoID, decimal? fromAount)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
            {
                return fromAount;
            }

            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (toCuryInfoID == null)
            {
                throw new PXArgumentException(nameof(toCuryInfoID));
            }

            if (fromCuryInfoID == null)
            {
                throw new PXArgumentException(nameof(fromCuryInfoID));
            }
            
            CurrencyInfo toCurrencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(graph, toCuryInfoID);
            CurrencyInfo fromCurrencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(graph, fromCuryInfoID);
            DateTime startDate = toCurrencyInfo.CuryEffDate ?? graph.Accessinfo.BusinessDate.GetValueOrDefault();
            return ConvertFromToCury(graph, toCurrencyInfo, fromCurrencyInfo, fromAount, startDate);
        }

        public static decimal? ConvertFromToCury(PXGraph graph, CurrencyInfo toCurrencyInfo, CurrencyInfo fromCurrencyInfo, decimal? fromAount)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
            {
                return fromAount;
            }

            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (fromCurrencyInfo == null)
            {
                throw new PXArgumentException(nameof(fromCurrencyInfo));
            }

            if (toCurrencyInfo == null)
            {
                throw new PXArgumentException(nameof(toCurrencyInfo));
            }

            DateTime startDate = toCurrencyInfo.CuryEffDate ?? graph.Accessinfo.BusinessDate.GetValueOrDefault();
            return ConvertFromToCury(graph, toCurrencyInfo, fromCurrencyInfo, fromAount, startDate);
        }

        public static decimal? ConvertFromToCury(PXGraph graph, CurrencyInfo toCurrencyInfo, CurrencyInfo fromCurrencyInfo, decimal? fromAmount, DateTime curyDateTime)
        {
            if (fromCurrencyInfo == null)
            {
                throw new PXArgumentException(nameof(fromCurrencyInfo));
            }

            return ConvertFromToCury(graph, toCurrencyInfo, fromCurrencyInfo.CuryID, fromAmount, curyDateTime);
        }

        public static decimal? ConvertFromToCury(PXGraph graph, CurrencyInfo toCurrencyInfo, string fromCuryID, decimal? fromAmount, DateTime curyDateTime)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
            {
                return fromAmount;
            }

            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (string.IsNullOrWhiteSpace(fromCuryID))
            {
                throw new PXArgumentException(nameof(fromCuryID));
            }

            if (toCurrencyInfo == null)
            {
                throw new PXArgumentException(nameof(toCurrencyInfo));
            }

            if (fromCuryID.EqualsWithTrim(toCurrencyInfo.CuryID))
            {
                return fromAmount;
            }

            Currency toCurrency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(graph, toCurrencyInfo.CuryID);

            var orderCurrencyRate = FindCuryRate(graph, toCurrencyInfo.CuryID, fromCuryID, toCurrencyInfo.CuryRateTypeID, curyDateTime);

            if (orderCurrencyRate == null)
            {
                throw new Exception(nameof(orderCurrencyRate));
            }

            return CuryConvCury(fromAmount, orderCurrencyRate.CuryRate.GetValueOrDefault(), orderCurrencyRate.CuryMultDiv, toCurrency == null ? DefaultCuryDecPlaces : toCurrency.DecimalPlaces.GetValueOrDefault(DefaultCuryDecPlaces));
        }

        public static decimal? ConvertFromBaseCury(PXGraph graph, CurrencyInfo toCurrencyInfo, decimal? baseAmount)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
            {
                return baseAmount;
            }

            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (toCurrencyInfo == null)
            {
                throw new PXArgumentException(nameof(toCurrencyInfo));
            }

            DateTime startDate = toCurrencyInfo.CuryEffDate ?? graph.Accessinfo.BusinessDate.GetValueOrDefault();
            return ConvertFromToCury(graph, toCurrencyInfo, toCurrencyInfo.BaseCuryID, baseAmount, startDate);
        }

        public static decimal? CuryConvCury(decimal? baseAmt, decimal curyRate, string curyMultDiv, int curyPrecision)
        {
            if (curyMultDiv == MultDiv.Divide && baseAmt != null)
            {
                return Math.Round(baseAmt.GetValueOrDefault() * curyRate, curyPrecision, MidpointRounding.AwayFromZero);
            }

            if (curyRate != 0m && baseAmt != null)
            {
                return Math.Round(baseAmt.GetValueOrDefault() / curyRate, curyPrecision, MidpointRounding.AwayFromZero);
            }

            return baseAmt;
        }

        public static CurrencyRate FindCuryRate(PXGraph graph, string fromCuryID, string toCuryID, string aCuryRateType, DateTime aDate)
        {
            CurrencyRate curyRate = PXSelectReadonly<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
                                And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
                                And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
                                And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
                                OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(graph, fromCuryID, toCuryID, aCuryRateType, aDate);

            return curyRate ?? PXSelectReadonly<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
                                And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
                                And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
                                And<CurrencyRate.curyEffDate, Greater<Required<CurrencyRate.curyEffDate>>>>>>,
                                OrderBy<Asc<CurrencyRate.curyEffDate>>>.Select(graph, fromCuryID, toCuryID, aCuryRateType, aDate);
        }
    }
}