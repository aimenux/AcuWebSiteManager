using System;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// PXProjection for <see cref="InventoryItem"/> only including the Manufacturing related fields
    /// Replacement for old InventoryItemExt standalone table updates.
    /// </summary>
    [Serializable]
    [PXProjection(typeof(Select<InventoryItem>), Persistent = true)]
    [PXHidden]
    public class InventoryItemMfgOnly : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;

        /// <summary>
        /// Database identity.
        /// The unique identifier of the Inventory Item.
        /// </summary>
        [PXDBInt(IsKey = true, BqlField = typeof(InventoryItem.inventoryID))]
        [PXDefault]
        [PXUIField(DisplayName = "Inventory ID", Visible = false, Enabled = false)]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region AMBOMID
        /// <summary>
        /// Default BOM ID
        /// </summary>
        public abstract class aMBOMID : PX.Data.BQL.BqlString.Field<aMBOMID> { }
        /// <summary>
        /// Default BOM ID
        /// </summary>
        [BomID(DisplayName = "Default BOM ID", BqlField = typeof(InventoryItemExt.aMBOMID))]
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
        [BomID(DisplayName = "Planning BOM ID", BqlField = typeof(InventoryItemExt.aMPlanningBOMID))]
        public string AMPlanningBOMID { get; set; }
        #endregion
        #region AMLotSize
        public abstract class aMLotSize : PX.Data.BQL.BqlDecimal.Field<aMLotSize> { }
        [PXDBQuantity(BqlField = typeof(InventoryItemExt.aMLotSize))]
        [PXUIField(DisplayName = "Lot Size")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMLotSize { get; set; }
        #endregion
        #region AMMaxOrdQty
        public abstract class aMMaxOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMaxOrdQty> { }
        [PXDBQuantity(BqlField = typeof(InventoryItemExt.aMMaxOrdQty))]
        [PXUIField(DisplayName = "Max Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMaxOrdQty { get; set; }
        #endregion
        #region AMMinOrdQty
        public abstract class aMMinOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMinOrdQty> { }
        [PXDBQuantity(BqlField = typeof(InventoryItemExt.aMMinOrdQty))]
        [PXUIField(DisplayName = "Min Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMinOrdQty { get; set; }
        #endregion
        #region AMLowLevel
        public abstract class aMLowLevel : PX.Data.BQL.BqlInt.Field<aMLowLevel> { }
        /// <summary>
        /// Non UI field - keeps items lowest bom level value used in calculations
        /// </summary>
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMLowLevel))]
        [PXUIField(DisplayName = "Low Level", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMLowLevel { get; set; }
        #endregion
        #region AMMRPItem
        public abstract class aMMRPItem : PX.Data.BQL.BqlBool.Field<aMMRPItem> { }
        [PXDBBool(BqlField = typeof(InventoryItemExt.aMMRPItem))]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "MRP Item")]
        public Boolean? AMMRPItem { get; set; }
        #endregion
        #region AMMFGLeadTime
        public abstract class aMMFGLeadTime : PX.Data.BQL.BqlInt.Field<aMMFGLeadTime> { }
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMMFGLeadTime))]
        [PXUIField(DisplayName = "MFG Lead Time")]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMMFGLeadTime { get; set; }
        #endregion
        #region AMGroupWindow
        public abstract class aMGroupWindow : PX.Data.BQL.BqlInt.Field<aMGroupWindow> { }
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMGroupWindow))]
        [PXUIField(DisplayName = "Group Planning")]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMGroupWindow { get; set; }
        #endregion
        #region AMWIPAcctID
        public abstract class aMWIPAcctID : PX.Data.BQL.BqlInt.Field<aMWIPAcctID> { }
        protected Int32? _AMWIPAcctID;
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMWIPAcctID))]
        [PXUIField(DisplayName = "Work in Process Account", Visibility = PXUIVisibility.Visible, FieldClass = "ACCOUNT")]
        public virtual Int32? AMWIPAcctID
        {
            get
            {
                return this._AMWIPAcctID;
            }
            set
            {
                this._AMWIPAcctID = value;
            }
        }
        #endregion
        #region AMWIPSubID
        public abstract class aMWIPSubID : PX.Data.BQL.BqlInt.Field<aMWIPSubID> { }
        protected Int32? _AMWIPSubID;
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMWIPSubID))]
        [PXUIField(DisplayName = "Work In Process Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = "SUBACCOUNT")]
        public virtual Int32? AMWIPSubID
        {
            get
            {
                return this._AMWIPSubID;
            }
            set
            {
                this._AMWIPSubID = value;
            }
        }
        #endregion
        #region AMWIPVarianceAcctID
        public abstract class aMWIPVarianceAcctID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceAcctID> { }
        protected Int32? _AMWIPVarianceAcctID;
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMWIPVarianceAcctID))]
        [PXUIField(DisplayName = "WIP Variance Account", Visibility = PXUIVisibility.Visible, FieldClass = "ACCOUNT")]
        public virtual Int32? AMWIPVarianceAcctID
        {
            get
            {
                return this._AMWIPVarianceAcctID;
            }
            set
            {
                this._AMWIPVarianceAcctID = value;
            }
        }
        #endregion
        #region AMWIPVarianceSubID
        public abstract class aMWIPVarianceSubID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceSubID> { }
        protected Int32? _AMWIPVarianceSubID;
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMWIPVarianceSubID))]
        [PXUIField(DisplayName = "WIP Variance Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = "SUBACCOUNT")]
        public virtual Int32? AMWIPVarianceSubID
        {
            get
            {
                return this._AMWIPVarianceSubID;
            }
            set
            {
                this._AMWIPVarianceSubID = value;
            }
        }
        #endregion
        #region AMConfigurationID

        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }
        /// <summary>
        /// Item configuration ID. If null, this item isn't configurable.
        /// </summary>
        [PXDBString(15, IsUnicode = true, BqlField = typeof(InventoryItemExt.aMConfigurationID))]
        [PXUIField(DisplayName = "Configuration ID")]
        public string AMConfigurationID { get; set; }

        #endregion
        #region AMReplenishmentSourceOverride
        public abstract class aMReplenishmentSourceOverride : PX.Data.BQL.BqlBool.Field<aMReplenishmentSourceOverride> { }
        protected Boolean? _AMReplenishmentSourceOverride;
        [PXDBBool(BqlField = typeof(InventoryItemExt.aMReplenishmentSourceOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AMReplenishmentSourceOverride
        {
            get
            {
                return this._AMReplenishmentSourceOverride;
            }
            set
            {
                this._AMReplenishmentSourceOverride = value;
            }
        }
        #endregion
        #region AMReplenishmentSource

        public abstract class aMReplenishmentSource : PX.Data.BQL.BqlString.Field<aMReplenishmentSource> { }
        protected string _AMReplenishmentSource;
        [PXDBString(1, IsFixed = true, BqlField = typeof(InventoryItemExt.aMReplenishmentSource))]
        [PXUIField(DisplayName = "Source")]
        [PXDefault(INReplenishmentSource.Purchased)]
        [INReplenishmentSource.List]
        public virtual string AMReplenishmentSource
        {
            get
            {
                return this._AMReplenishmentSource;
            }
            set
            {
                this._AMReplenishmentSource = value;
            }
        }
        #endregion
        #region AMSafetyStockOverride
        public abstract class aMSafetyStockOverride : PX.Data.BQL.BqlBool.Field<aMSafetyStockOverride> { }
        protected Boolean? _AMSafetyStockOverride;
        [PXDBBool(BqlField = typeof(InventoryItemExt.aMSafetyStockOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AMSafetyStockOverride
        {
            get
            {
                return this._AMSafetyStockOverride;
            }
            set
            {
                this._AMSafetyStockOverride = value;
            }
        }
        #endregion
        #region AMSafetyStock
        public abstract class aMSafetyStock : PX.Data.BQL.BqlDecimal.Field<aMSafetyStock> { }
        protected Decimal? _AMSafetyStock;
        [PXDBQuantity(BqlField = typeof(InventoryItemExt.aMSafetyStock))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Safety Stock")]
        public virtual Decimal? AMSafetyStock
        {
            get
            {
                return this._AMSafetyStock;
            }
            set
            {
                this._AMSafetyStock = value;
            }
        }
        #endregion
        #region AMMinQtyOverride
        public abstract class aMMinQtyOverride : PX.Data.BQL.BqlBool.Field<aMMinQtyOverride> { }
        protected Boolean? _AMMinQtyOverride;
        [PXDBBool(BqlField = typeof(InventoryItemExt.aMMinQtyOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AMMinQtyOverride
        {
            get
            {
                return this._AMMinQtyOverride;
            }
            set
            {
                this._AMMinQtyOverride = value;
            }
        }
        #endregion
        #region AMMinQty
        public abstract class aMMinQty : PX.Data.BQL.BqlDecimal.Field<aMMinQty> { }
        protected Decimal? _AMMinQty;
        [PXDBQuantity(BqlField = typeof(InventoryItemExt.aMMinQty))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Reorder Point")]
        public virtual Decimal? AMMinQty
        {
            get
            {
                return this._AMMinQty;
            }
            set
            {
                this._AMMinQty = value;
            }
        }
        #endregion
        #region AMQtyRoundUp
        public abstract class aMQtyRoundUp : PX.Data.BQL.BqlBool.Field<aMQtyRoundUp> { }
        [PXDBBool(BqlField = typeof(InventoryItemExt.aMQtyRoundUp))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Quantity Round Up")]
        public Boolean? AMQtyRoundUp { get; set; }
        #endregion
        #region AMScrapSiteID
        public abstract class aMScrapSiteID : PX.Data.BQL.BqlInt.Field<aMScrapSiteID> { }
        protected Int32? _AMScrapSiteID;
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMScrapSiteID))]
        [PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
        public virtual Int32? AMScrapSiteID
        {
            get
            {
                return this._AMScrapSiteID;
            }
            set
            {
                this._AMScrapSiteID = value;
            }
        }
        #endregion
        #region AMScrapLocationID
        public abstract class aMScrapLocationID : PX.Data.BQL.BqlInt.Field<aMScrapLocationID> { }
        protected Int32? _AMScrapLocationID;
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMScrapLocationID))]
        [PXUIField(DisplayName = "Scrap Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
        public virtual Int32? AMScrapLocationID
        {
            get
            {
                return this._AMScrapLocationID;
            }
            set
            {
                this._AMScrapLocationID = value;
            }
        }
        #endregion
        #region AMMakeToOrderItem
        public abstract class aMMakeToOrderItem : PX.Data.BQL.BqlBool.Field<aMMakeToOrderItem> { }
        [PXDBBool(BqlField = typeof(InventoryItemExt.aMMakeToOrderItem))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Make to Order Item")]
        public Boolean? AMMakeToOrderItem { get; set; }
        #endregion
        #region AMDefaultMarkFor
        public abstract class aMDefaultMarkFor : PX.Data.BQL.BqlInt.Field<aMDefaultMarkFor> { }
        [PXDBInt(BqlField = typeof(InventoryItemExt.aMDefaultMarkFor))]
        [PXDefault(MaterialDefaultMarkFor.NoDefault, PersistingCheck = PXPersistingCheck.Nothing)]
        [MaterialDefaultMarkFor.StockItemList]
        [PXUIField(DisplayName = "Material Default Mark For")]
        public int? AMDefaultMarkFor { get; set; }
        #endregion
        #region AMCheckSchdMatlAvailability
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        public abstract class aMCheckSchdMatlAvailability : PX.Data.BQL.BqlBool.Field<aMCheckSchdMatlAvailability> { }

        protected Boolean? _AMCheckSchdMatlAvailability;
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        [PXDBBool(BqlField = typeof(InventoryItemExt.aMCheckSchdMatlAvailability))]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Check for Material Availability")]
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

        public static implicit operator InventoryItemMfgOnly(CacheExtensions.InventoryItemExt extension)
        {
            return new InventoryItemMfgOnly
            {
                AMBOMID = extension.AMBOMID,
                AMPlanningBOMID = extension.AMPlanningBOMID,
                AMLotSize = extension.AMLotSize,
                AMMaxOrdQty = extension.AMMaxOrdQty,
                AMMinOrdQty = extension.AMMinOrdQty,
                AMLowLevel = extension.AMLowLevel,
                AMMRPItem = extension.AMMRPItem,
                AMMFGLeadTime = extension.AMMFGLeadTime,
                AMGroupWindow = extension.AMGroupWindow,
                AMWIPAcctID = extension.AMWIPAcctID,
                AMWIPSubID = extension.AMWIPSubID,
                AMWIPVarianceAcctID = extension.AMWIPVarianceAcctID,
                AMWIPVarianceSubID = extension.AMWIPVarianceSubID,
                AMConfigurationID = extension.AMConfigurationID,
                AMReplenishmentSourceOverride = extension.AMReplenishmentSourceOverride,
                AMReplenishmentSource = extension.AMReplenishmentSource,
                AMSafetyStockOverride = extension.AMSafetyStockOverride,
                AMSafetyStock = extension.AMSafetyStock,
                AMMinQtyOverride = extension.AMMinQtyOverride,
                AMMinQty = extension.AMMinQty,
                AMQtyRoundUp = extension.AMQtyRoundUp,
                AMScrapSiteID = extension.AMScrapSiteID,
                AMScrapLocationID = extension.AMScrapLocationID,
                AMMakeToOrderItem = extension.AMMakeToOrderItem,
                AMDefaultMarkFor = extension.AMDefaultMarkFor,
                AMCheckSchdMatlAvailability = extension.AMCheckSchdMatlAvailability
            };
        }
    }
}
