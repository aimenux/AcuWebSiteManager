using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public static class ExtensionHelper
    {
        public static bool IsMultyCurrencyEnabled
        {
            get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
        }

        public static CurrencyInfo SelectCurrencyInfo(PXSelectBase<CurrencyInfo> currencyInfoView, long? curyInfoID)
        {
            if (curyInfoID == null)
            {
                return null;
            }

            var result = (CurrencyInfo)currencyInfoView.Cache.Current;
            return result != null && curyInfoID == result.CuryInfoID ? result : currencyInfoView.SelectSingle();
        }

        public static CurrencyInfo GetCurrencyInfo(PXGraph graph, long? curyInfoID)
        {
            if (curyInfoID == null)
            {
                return null;
            }

            return PXSelect<CurrencyInfo,
                   Where<
                       CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
                   .Select(graph, curyInfoID);
        }
    }
}
