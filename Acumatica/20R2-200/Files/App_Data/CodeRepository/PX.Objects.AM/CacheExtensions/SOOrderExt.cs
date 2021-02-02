using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM.CacheExtensions
{
    [PXCopyPasteHiddenFields(
        typeof(SOOrderExt.aMEstimateQty),
        typeof(SOOrderExt.aMEstimateTotal),
        typeof(SOOrderExt.aMCuryEstimateTotal))]
    [Serializable]
    public sealed class SOOrderExt : PXCacheExtension<SOOrder>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturingProductConfigurator>() ||
                   PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturingEstimating>();
        }

        #region AMEstimateQty
        public abstract class aMEstimateQty : PX.Data.BQL.BqlDecimal.Field<aMEstimateQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Qty", Enabled = false)]
        public Decimal? AMEstimateQty { get; set; }
        #endregion
        #region AMCuryEstimateTotal
        public abstract class aMCuryEstimateTotal : PX.Data.BQL.BqlDecimal.Field<aMCuryEstimateTotal> { }

        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrderExt.aMEstimateTotal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Total", Enabled = false)]
        public Decimal? AMCuryEstimateTotal { get; set; }
        #endregion
        #region AMEstimateTotal
        public abstract class aMEstimateTotal : PX.Data.BQL.BqlDecimal.Field<aMEstimateTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Total", Enabled = false)]
        public Decimal? AMEstimateTotal { get; set; }
        #endregion
        #region AMUseConfigPrice
        /// <summary>
        /// If the order is being updated that triggers the update of all sales line prices, we need to know so we can override and get the config line prices
        /// </summary>
        public abstract class aMUseConfigPrice : PX.Data.BQL.BqlBool.Field<aMUseConfigPrice> { }

        /// <summary>
        /// If the order is being updated that triggers the update of all sales line prices, we need to know so we can override and get the config line prices
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Use Config Price", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public bool? AMUseConfigPrice { get; set; }
        #endregion
    }
}