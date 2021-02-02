using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing cache extension on Opportunity Maint. - "Create Sales Order" process/panel for <see cref="OpportunityMaint.CreateSalesOrderFilter"/>
    /// </summary>
    [Serializable]
    public sealed class CRCreateSalesOrderFilterExt : PXCacheExtension<OpportunityMaint.CreateSalesOrderFilter>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
        }

        #region AMIncludeEstimate
        public abstract class aMIncludeEstimate : PX.Data.BQL.BqlBool.Field<aMIncludeEstimate> { }
        /// <summary>
        /// Indicates if the estimates should be included in the create sales order process.
        /// When checked the current opp estimates will covert to sales order detail lines if valid.
        /// </summary>
        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Convert Estimates")]
        public bool? AMIncludeEstimate { get; set; }
        #endregion
        #region AMCopyConfigurations
        public abstract class aMCopyConfigurations : PX.Data.BQL.BqlBool.Field<aMCopyConfigurations> { }

        /// <summary>
        /// Indicates if the configurations should be included in the create sales order process.
        /// When checked the current opp configurations will copy to sales order.
        /// </summary>
        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Copy Configurations")]
        public bool? AMCopyConfigurations { get; set; }
        #endregion
    }
}