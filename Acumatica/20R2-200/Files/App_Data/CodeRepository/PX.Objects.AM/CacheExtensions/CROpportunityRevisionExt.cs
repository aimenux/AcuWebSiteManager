using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR.Standalone;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    [PXCopyPasteHiddenFields(
        typeof(CROpportunityRevisionExt.aMEstimateQty),
        typeof(CROpportunityRevisionExt.aMEstimateTotal),
        typeof(CROpportunityRevisionExt.aMCuryEstimateTotal))]
    [Serializable]
    public sealed class CROpportunityRevisionExt : PXCacheExtension<CROpportunityRevision>
    {
        public static bool IsActive()
        {
            // features in this extension only related to estimating module
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingEstimating>();
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

        [PXDBCurrency(typeof(CROpportunityRevision.curyInfoID), typeof(CROpportunityRevisionExt.aMEstimateTotal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Amount", Enabled = false)]
        public Decimal? AMCuryEstimateTotal { get; set; }
        #endregion
        #region AMEstimateTotal
        public abstract class aMEstimateTotal : PX.Data.BQL.BqlDecimal.Field<aMEstimateTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Total", Enabled = false)]
        public Decimal? AMEstimateTotal { get; set; }
        #endregion
    }
}