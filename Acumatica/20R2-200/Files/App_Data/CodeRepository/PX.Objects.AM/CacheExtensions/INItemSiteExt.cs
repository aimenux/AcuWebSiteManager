using PX.Data;
using PX.Objects.IN;
using System;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class INItemSiteExt : PXCacheExtension<INItemSite>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMBOMID
        /// <summary>
        /// Default BOM ID
        /// </summary>
        public abstract class aMBOMID : PX.Data.BQL.BqlString.Field<aMBOMID> { }
        /// <summary>
        /// Default BOM ID
        /// </summary>
        [BomID(DisplayName = "Default BOM ID")]
        [PXSelector(typeof(Search<AMBomItem.bOMID,
                Where<AMBomItem.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<AMBomItem.siteID, Equal<Current<INItemSite.siteID>>>>,
                OrderBy<Asc<AMBomItem.bOMID>>>)
            , typeof(AMBomItem.bOMID)
            , typeof(AMBomItem.siteID)
            , typeof(AMBomItem.revisionID)
            , DescriptionField = typeof(AMBomItem.descr))]
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>>), Messages.BomIsNotActive, typeof(AMBomItem.bOMID))]
        public string AMBOMID { get; set; }
        #endregion
        #region AMPlanningBOMID
        /// <summary>
        /// Planning BOM ID
        /// </summary>
        public abstract class aMPlanningBOMID : PX.Data.BQL.BqlString.Field<aMPlanningBOMID> { }
        /// <summary>
        /// Planning BOM ID
        /// </summary>
        [BomID(DisplayName = "Planning BOM ID", Visibility = PXUIVisibility.Undefined, FieldClass = Features.MRPFIELDCLASS)]
        [PXSelector(typeof(Search<AMBomItem.bOMID,
                Where<AMBomItem.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                    And<AMBomItem.siteID, Equal<Current<INItemSite.siteID>>>>,
                OrderBy<Asc<AMBomItem.bOMID>>>)
            , typeof(AMBomItem.bOMID)
            , typeof(AMBomItem.siteID)
            , typeof(AMBomItem.revisionID)
            , DescriptionField = typeof(AMBomItem.descr))]
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>>), Messages.BomIsNotActive, typeof(AMBomItem.bOMID))]
        public string AMPlanningBOMID { get; set; }
        #endregion
        #region AMLotSizeOverride
        public abstract class aMLotSizeOverride : PX.Data.BQL.BqlBool.Field<aMLotSizeOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMLotSizeOverride { get; set; }
        #endregion
        #region AMLotSize
        public abstract class aMLotSize : PX.Data.BQL.BqlDecimal.Field<aMLotSize> { }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Lot Size")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMLotSize { get; set; }
        #endregion
        #region AMMaxOrdQtyOverride
        public abstract class aMMaxOrdQtyOverride : PX.Data.BQL.BqlBool.Field<aMMaxOrdQtyOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMaxOrdQtyOverride { get; set; }
        #endregion
        #region AMMaxOrdQty
        public abstract class aMMaxOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMaxOrdQty> { }
        [PXDBQuantity]
        [PXUIField(DisplayName = "Max Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMaxOrdQty { get; set; }
        #endregion
        #region AMMinOrdQtyOverride
        public abstract class aMMinOrdQtyOverride : PX.Data.BQL.BqlBool.Field<aMMinOrdQtyOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMinOrdQtyOverride { get; set; }
        #endregion
        #region AMMinOrdQty
        public abstract class aMMinOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMinOrdQty> { }
        [PXDBQuantity]
        [PXUIField(DisplayName = "Min Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMinOrdQty { get; set; }
        #endregion
        #region AMMFGLeadTimeOverride
        public abstract class aMMFGLeadTimeOverride : PX.Data.BQL.BqlBool.Field<aMMFGLeadTimeOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMFGLeadTimeOverride { get; set; }
        #endregion
        #region AMMFGLeadTime
        public abstract class aMMFGLeadTime : PX.Data.BQL.BqlInt.Field<aMMFGLeadTime> { }
        [PXDBInt]
        [PXUIField(DisplayName = "MFG Lead Time")]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMMFGLeadTime { get; set; }
        #endregion
        #region AMGroupWindowOverride
        public abstract class aMGroupWindowOverride : PX.Data.BQL.BqlBool.Field<aMGroupWindowOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override", FieldClass = Features.MRPFIELDCLASS)]
        public Boolean? AMGroupWindowOverride { get; set; }
        #endregion
        #region AMGroupWindow
        public abstract class aMGroupWindow : PX.Data.BQL.BqlInt.Field<aMGroupWindow> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Group Planning", FieldClass = Features.MRPFIELDCLASS)]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMGroupWindow { get; set; }
        #endregion
        #region AMConfigurationID

        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }
        /// <summary>
        /// Item configuration ID. If null, this item will fallback to StockItem configuration ID
        /// </summary>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Configuration ID", FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        [PXSelector(typeof(Search2<AMConfiguration.configurationID,
                InnerJoin<AMBomItem,
                    On<AMBomItem.bOMID, Equal<AMConfiguration.bOMID>,
                        And<AMBomItem.revisionID, Equal<AMConfiguration.bOMRevisionID>>>>,
                Where<AMConfiguration.status, Equal<ConfigRevisionStatus.active>,
                    And<AMConfiguration.inventoryID, Equal<Current<INItemSite.inventoryID>>>>>),
            new[]
            {
                typeof(AMConfiguration.configurationID),
                typeof(AMConfiguration.revision),
                typeof(AMConfiguration.bOMID),
                typeof(AMBomItem.siteID)
            },
            DescriptionField = typeof(AMConfiguration.descr))]
        public string AMConfigurationID { get; set; }

        #endregion
        #region AMReplenishmentPolicyOverride  (Unbound)
        public abstract class aMReplenishmentPolicyOverride : PX.Data.BQL.BqlBool.Field<aMReplenishmentPolicyOverride> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMReplenishmentPolicyOverride { get; set; }
        #endregion
        #region AMReplenishmentSource (Unbound)

        public abstract class aMReplenishmentSource : PX.Data.BQL.BqlString.Field<aMReplenishmentSource> { }

        [PXString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Source")]
        [INReplenishmentSource.List]
        public string AMReplenishmentSource { get; set; }
        #endregion
        #region AMSafetyStockOverride (Unbound)
        public abstract class aMSafetyStockOverride : PX.Data.BQL.BqlBool.Field<aMSafetyStockOverride> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMSafetyStockOverride { get; set; }
        #endregion
        #region AMSafetyStock  (Unbound)
        public abstract class aMSafetyStock : PX.Data.BQL.BqlDecimal.Field<aMSafetyStock> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Safety Stock")]
        public Decimal? AMSafetyStock { get; set; }
        #endregion
        #region AMMinQtyOverride (Unbound)
        public abstract class aMMinQtyOverride : PX.Data.BQL.BqlBool.Field<aMMinQtyOverride> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMinQtyOverride { get; set; }
        #endregion
        #region AMMinQty  (Unbound)
        public abstract class aMMinQty : PX.Data.BQL.BqlDecimal.Field<aMMinQty> { }

        [PXQuantity]
        [PXUIField(DisplayName = "Reorder Point")]
        public Decimal? AMMinQty { get; set; }
        #endregion
        #region AMScrapOverride
        public abstract class aMScrapOverride : PX.Data.BQL.BqlBool.Field<aMScrapOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Scrap Override")]
        public Boolean? AMScrapOverride { get; set; }
        #endregion
        #region AMScrapSiteID
        public abstract class aMScrapSiteID : PX.Data.BQL.BqlInt.Field<aMScrapSiteID> { }

        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site(DisplayName = "Scrap Warehouse")]
        public Int32? AMScrapSiteID { get; set; }
        #endregion
        #region AMScrapLocationID
        public abstract class aMScrapLocationID : PX.Data.BQL.BqlInt.Field<aMScrapLocationID> { }

        [Location(typeof(INItemSiteExt.aMScrapSiteID))]
        [PXUIField(DisplayName = "Scrap Location")]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>),
            PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        public Int32? AMScrapLocationID { get; set; }
        #endregion
        #region AMReplenishmentSourceSiteID (Unbound)
        public abstract class aMReplenishmentSourceSiteID : PX.Data.BQL.BqlInt.Field<aMReplenishmentSourceSiteID> { }
        [PXInt]
        [PXUIField(DisplayName = "Replenishment Warehouse")]
        [PXSelector(typeof(INSite.siteID), SubstituteKey = typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
        public Int32? AMReplenishmentSourceSiteID { get; set; }
        #endregion
    }
}