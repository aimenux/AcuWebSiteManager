using PX.Data;
using PX.Objects.IN;
using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM.CacheExtensions
{
    [PXCopyPasteHiddenFields(
        typeof(InventoryItemExt.aMBOMID), 
        typeof(InventoryItemExt.aMPlanningBOMID), 
        typeof(InventoryItemExt.aMConfigurationID))]
    [Serializable]
    public sealed class InventoryItemExt : PXCacheExtension<InventoryItem>
    {
        // Developer note: new fields added here should also be added to InventoryItemMfgOnly

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
                        Where<AMBomItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
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
                        Where<AMBomItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
                            OrderBy<Asc<AMBomItem.bOMID>>>)
            , typeof(AMBomItem.bOMID)
            , typeof(AMBomItem.siteID)
            , typeof(AMBomItem.revisionID)
            , DescriptionField = typeof(AMBomItem.descr))]
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>>), Messages.BomIsNotActive, typeof(AMBomItem.bOMID))]
        public string AMPlanningBOMID { get; set; }
        #endregion
        #region AMLotSize
        public abstract class aMLotSize : PX.Data.BQL.BqlDecimal.Field<aMLotSize> { }
        [PXDBQuantity]
        [PXUIField(DisplayName = "Lot Size")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMLotSize { get; set; }
        #endregion
        #region AMMaxOrdQty
        public abstract class aMMaxOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMaxOrdQty> { }
        [PXDBQuantity]
        [PXUIField(DisplayName="Max Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMaxOrdQty { get; set; }
        #endregion
        #region AMMinOrdQty
        public abstract class aMMinOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMinOrdQty> { }
        [PXDBQuantity]
        [PXUIField(DisplayName="Min Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMinOrdQty { get; set; }
        #endregion
        #region AMLowLevel
        public abstract class aMLowLevel : PX.Data.BQL.BqlInt.Field<aMLowLevel> { }
        /// <summary>
        /// Non UI field - keeps items lowest bom level value used in calculations
        /// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "Low Level", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMLowLevel { get; set; }
        #endregion
        #region AMMRPItem
        public abstract class aMMRPItem : PX.Data.BQL.BqlBool.Field<aMMRPItem> { }
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "MRP Item", FieldClass = Features.MRPFIELDCLASS)]
        public Boolean? AMMRPItem { get; set; }
        #endregion
        #region AMMFGLeadTime
        public abstract class aMMFGLeadTime : PX.Data.BQL.BqlInt.Field<aMMFGLeadTime> { }
        [PXDBInt]
        [PXUIField(DisplayName = "MFG Lead Time")]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMMFGLeadTime { get; set; }
        #endregion
        #region AMGroupWindow
        public abstract class aMGroupWindow : PX.Data.BQL.BqlInt.Field<aMGroupWindow> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Group Planning", FieldClass = Features.MRPFIELDCLASS)]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMGroupWindow { get; set; }
        #endregion
        #region AMWIPAcctID
        public abstract class aMWIPAcctID : PX.Data.BQL.BqlInt.Field<aMWIPAcctID> { }

        [Account(DisplayName = "Work In Process Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPAcctID>.IsRelatedTo<Account.accountID>))]
        public Int32? AMWIPAcctID { get; set; }
        #endregion
        #region AMWIPSubID
        public abstract class aMWIPSubID : PX.Data.BQL.BqlInt.Field<aMWIPSubID> { }

        [SubAccount(typeof(InventoryItemExt.aMWIPAcctID), DisplayName = "Work In Process Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPSubID>.IsRelatedTo<Sub.subID>))]
        public Int32? AMWIPSubID { get; set; }
        #endregion
        #region AMWIPVarianceAcctID
        public abstract class aMWIPVarianceAcctID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceAcctID> { }

        [Account(DisplayName = "WIP Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPVarianceAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPVarianceAcctID>.IsRelatedTo<Account.accountID>))]
        public Int32? AMWIPVarianceAcctID { get; set; }
        #endregion
        #region AMWIPVarianceSubID
        public abstract class aMWIPVarianceSubID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceSubID> { }

        [SubAccount(typeof(InventoryItemExt.aMWIPVarianceAcctID), DisplayName = "WIP Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPVarianceSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPVarianceSubID>.IsRelatedTo<Sub.subID>))]
        public Int32? AMWIPVarianceSubID { get; set; }
        #endregion
        #region AMConfigurationID

        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }
        /// <summary>
        /// Item configuration ID. If null, this item isn't configurable.
        /// </summary>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Configuration ID", FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        [PXSelector(typeof(Search<AMConfiguration.configurationID, 
            Where<AMConfiguration.status, 
                Equal<ConfigRevisionStatus.active>, 
            And<AMConfiguration.inventoryID,
                Equal<Current<InventoryItem.inventoryID>>>>>),
            DescriptionField = typeof(AMConfiguration.descr))]
        public string AMConfigurationID { get; set; }

        #endregion
        #region AMReplenishmentSourceOverride
        public abstract class aMReplenishmentSourceOverride : PX.Data.BQL.BqlBool.Field<aMReplenishmentSourceOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMReplenishmentSourceOverride { get; set; }
        #endregion
        #region AMReplenishmentSource

        public abstract class aMReplenishmentSource : PX.Data.BQL.BqlString.Field<aMReplenishmentSource> { }
        protected string _AMReplenishmentSource;

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Source")]
        [PXDefault(INReplenishmentSource.Purchased, PersistingCheck = PXPersistingCheck.Nothing)]
        [INReplenishmentSource.List]
        public string AMReplenishmentSource
        {
            get => this._AMReplenishmentSource ?? INReplenishmentSource.Purchased;
            set => this._AMReplenishmentSource = value;
        }
        #endregion
        #region AMSafetyStockOverride
        public abstract class aMSafetyStockOverride : PX.Data.BQL.BqlBool.Field<aMSafetyStockOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMSafetyStockOverride { get; set; }
        #endregion
        #region AMSafetyStock
        public abstract class aMSafetyStock : PX.Data.BQL.BqlDecimal.Field<aMSafetyStock> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Safety Stock")]
        public Decimal? AMSafetyStock { get; set; }
        #endregion
        #region AMMinQtyOverride
        public abstract class aMMinQtyOverride : PX.Data.BQL.BqlBool.Field<aMMinQtyOverride> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMinQtyOverride { get; set; }
        #endregion
        #region AMMinQty
        public abstract class aMMinQty : PX.Data.BQL.BqlDecimal.Field<aMMinQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Reorder Point")]
        public Decimal? AMMinQty { get; set; }
        #endregion
        #region AMQtyRoundUp
        public abstract class aMQtyRoundUp : PX.Data.BQL.BqlBool.Field<aMQtyRoundUp> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Quantity Round Up")]
        public Boolean? AMQtyRoundUp { get; set; }
        #endregion
        #region AMScrapSiteID
        public abstract class aMScrapSiteID : PX.Data.BQL.BqlInt.Field<aMScrapSiteID> { }

        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site(DisplayName = "Scrap Warehouse")]
        public Int32? AMScrapSiteID { get; set; }
        #endregion
        #region AMScrapLocationID
        public abstract class aMScrapLocationID : PX.Data.BQL.BqlInt.Field<aMScrapLocationID> { }

        [Location(typeof(InventoryItemExt.aMScrapSiteID), DisplayName = "Scrap Location")]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>),
            PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        public Int32? AMScrapLocationID { get; set; }
        #endregion
        #region AMMakeToOrderItem
        public abstract class aMMakeToOrderItem : PX.Data.BQL.BqlBool.Field<aMMakeToOrderItem> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Make to Order Item")]
        public Boolean? AMMakeToOrderItem { get; set; }
        #endregion
        #region AMDefaultMarkFor
        public abstract class aMDefaultMarkFor : PX.Data.BQL.BqlInt.Field<aMDefaultMarkFor> { }
        [PXDBInt]
        [PXDefault(MaterialDefaultMarkFor.NoDefault, PersistingCheck = PXPersistingCheck.Nothing)]
        [MaterialDefaultMarkFor.StockItemList]
        [PXUIField(DisplayName = "Dflt Mark For")]
        public int? AMDefaultMarkFor { get; set; }
        #endregion
        #region AMCheckSchdMatlAvailability
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        public abstract class aMCheckSchdMatlAvailability : PX.Data.BQL.BqlBool.Field<aMCheckSchdMatlAvailability> { }

        private Boolean? _AMCheckSchdMatlAvailability;
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Check for Material Availability", FieldClass = Features.ADVANCEDPLANNINGFIELDCLASS)]
        public Boolean? AMCheckSchdMatlAvailability
        {
            get
            {
                return this._AMCheckSchdMatlAvailability;
            }
            set
            {
                this._AMCheckSchdMatlAvailability = value ?? true;
            }
        }
        #endregion
    }
}