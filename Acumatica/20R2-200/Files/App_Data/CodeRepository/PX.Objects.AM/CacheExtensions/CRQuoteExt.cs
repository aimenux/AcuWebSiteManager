﻿using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// MFG extension to CRQuote PXProjection to use CROpportunityRevisionExt
    /// (non table extension)
    /// </summary>
    [Serializable]
    public sealed class CRQuoteExt : PXCacheExtension<CRQuote>
    {
        /// <summary>
        /// Determines if extension is active
        /// </summary>
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingEstimating>();
        }

        #region AMEstimateQty (CROpportunityRevisionExt.aMEstimateQty)
        /// <summary>
        /// Pointer to CROpportunityRevisionExt.aMEstimateQty
        /// </summary>
        public abstract class aMEstimateQty : PX.Data.IBqlField { }

        /// <summary>
        /// Pointer to CROpportunityRevisionExt.AMEstimateQty
        /// </summary>
        [PXDBQuantity(BqlField = typeof(CROpportunityRevisionExt.aMEstimateQty))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Qty", Enabled = false)]
        public Decimal? AMEstimateQty { get; set; }
        #endregion
        #region AMCuryEstimateTotal (CROpportunityRevisionExt.aMCuryEstimateTotal)
        /// <summary>
        /// Pointer to CROpportunityRevisionExt.aMCuryEstimateTotal
        /// </summary>
        public abstract class aMCuryEstimateTotal : PX.Data.IBqlField { }

        /// <summary>
        /// Pointer to CROpportunityRevisionExt.AMCuryEstimateTotal
        /// </summary>
        [PXDBCurrency(typeof(CRQuote.curyInfoID), typeof(CRQuoteExt.aMEstimateTotal), BqlField = typeof(CROpportunityRevisionExt.aMCuryEstimateTotal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Amount", Enabled = false)]
        public Decimal? AMCuryEstimateTotal { get; set; }
        #endregion
        #region AMEstimateTotal (CROpportunityRevisionExt.aMEstimateTotal)
        /// <summary>
        /// Pointer to CROpportunityRevisionExt.aMEstimateTotal
        /// </summary>
        public abstract class aMEstimateTotal : PX.Data.IBqlField { }

        /// <summary>
        /// Pointer to CROpportunityRevisionExt.AMEstimateTotal
        /// </summary>
        [PXDBBaseCury(BqlField = typeof(CROpportunityRevisionExt.aMEstimateTotal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Total", Enabled = false)]
        public Decimal? AMEstimateTotal { get; set; }
        #endregion
    }
}