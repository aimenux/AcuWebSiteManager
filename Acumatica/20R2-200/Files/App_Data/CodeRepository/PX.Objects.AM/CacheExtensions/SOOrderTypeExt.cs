using System;
using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing Cache Extension for <see cref="PX.Objects.SO.SOOrderType"/>
    /// </summary>
    [Serializable]
    public sealed class SOOrderTypeExt : PXCacheExtension<SOOrderType>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturing>();
        }

        #region AMProductionOrderEntry
        public abstract class aMProductionOrderEntry : PX.Data.BQL.BqlBool.Field<aMProductionOrderEntry> { }

        /// <summary>
        /// Indicates if the Order Type when selected on a sales order
        /// allows the user to click the ACtions > Production Orders button to create 
        /// production orders for the given sales order number
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Production Orders - Approved")]
        public bool? AMProductionOrderEntry { get; set; }
        #endregion
        #region AMProductionOrderEntryOnHold
        public abstract class aMProductionOrderEntryOnHold : PX.Data.BQL.BqlBool.Field<aMProductionOrderEntryOnHold> { }

        /// <summary>
        /// Indicates for those order types that allow production if the status of the order
        /// must be only approved (true) or any status (false)
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Production Orders - Hold")]
        public bool? AMProductionOrderEntryOnHold { get; set; }
        #endregion
        #region AMConfigurationEntry
        public abstract class aMConfigurationEntry : PX.Data.BQL.BqlString.Field<aMConfigurationEntry> { }
        /// <summary>
        /// Indicates if the Order Type allows the user to launch the configuration entry page.
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Configuration Entry")]
        public bool? AMConfigurationEntry { get; set; }
        #endregion
        #region AMEstimateEntry
        public abstract class aMEstimateEntry : PX.Data.BQL.BqlBool.Field<aMEstimateEntry> { }

        /// <summary>
        /// Indicates if the Order Type allows the user to enter estimates on a sales order.
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Estimating")]
        public bool? AMEstimateEntry { get; set; }
        #endregion
        #region AMEnableWarehouseLinkedProduction
        public abstract class aMEnableWarehouseLinkedProduction : PX.Data.BQL.BqlBool.Field<aMEnableWarehouseLinkedProduction> { }

        /// <summary>
        /// Indicates if Warehouse should be enabled with Linked Production 
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Enable Warehouse On Line With Linked Production")]
        public bool? AMEnableWarehouseLinkedProduction { get; set; }
        #endregion
        #region AMMTOOrder
        public abstract class aMMTOOrder : PX.Data.BQL.BqlBool.Field<aMMTOOrder> { }

        /// <summary>
        /// Indicates if the Order Type when selected on a sales order
        /// allows the user to click the ACtions > Production Orders button to create 
        /// production orders for the given sales order number
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "MTO Order")]
        public bool? AMMTOOrder { get; set; }
        #endregion
    }
}