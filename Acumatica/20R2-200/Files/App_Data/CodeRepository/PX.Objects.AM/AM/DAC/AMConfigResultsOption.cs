using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationResultOption)]
    public class AMConfigResultsOption : IBqlTable, IRuleValid
    {
        internal string DebuggerDisplay => $"[{ConfigResultsID}][{ConfigurationID}:{Revision}:{FeatureLineNbr}:{OptionLineNbr}] Included={Included}; Qty={Qty}";

        #region ConfigResultsID
        public abstract class configResultsID : PX.Data.BQL.BqlInt.Field<configResultsID> { }

		protected int? _ConfigResultsID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Config Results ID", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMConfigurationResults.configResultsID))]
        [PXParent(typeof(Select<AMConfigurationResults, Where<AMConfigurationResults.configResultsID, Equal<Current<configResultsID>>>>))]
        public virtual int? ConfigResultsID
		{
			get
			{
				return this._ConfigResultsID;
			}
			set
			{
				this._ConfigResultsID = value;
			}
		}
        #endregion
        #region ConfigurationID
        public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

        protected string _ConfigurationID;
        [PXDBString(15, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.configurationID))]
        [PXUIField(DisplayName = "Configuration ID", Visible = false, Enabled = false)]
        public virtual string ConfigurationID
        {
            get
            {
                return this._ConfigurationID;
            }
            set
            {
                this._ConfigurationID = value;
            }
        }
        #endregion
        #region Revision
        public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }

        protected string _Revision;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.revision))]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual string Revision
        {
            get
            {
                return this._Revision;
            }
            set
            {
                this._Revision = value;
            }
        }
        #endregion
        #region FeatureLineNbr
        public abstract class featureLineNbr : PX.Data.BQL.BqlInt.Field<featureLineNbr> { }

		protected int? _FeatureLineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Feature Line Nbr", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMConfigResultsFeature.featureLineNbr))]
        [PXParent(typeof(Select<AMConfigResultsFeature, 
                            Where<AMConfigResultsFeature.configResultsID, 
                                Equal<Current<configResultsID>>, 
                            And<AMConfigResultsFeature.featureLineNbr,
                                Equal<Current<featureLineNbr>>>>>))]
        public virtual int? FeatureLineNbr
		{
			get
			{
				return this._FeatureLineNbr;
			}
			set
			{
				this._FeatureLineNbr = value;
			}
		}
		#endregion
		#region OptionLineNbr
		public abstract class optionLineNbr : PX.Data.BQL.BqlInt.Field<optionLineNbr> { }

		protected int? _OptionLineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Option Line Nbr", Visible = false, Enabled = false)]
		public virtual int? OptionLineNbr
		{
			get
			{
				return this._OptionLineNbr;
			}
			set
			{
				this._OptionLineNbr = value;
			}
		}
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected int? _InventoryID;
        [AnyInventory(Enabled = false)]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual int? InventoryID
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected int? _SubItemID;
        [SubItem(typeof(inventoryID), Enabled = false)]
        public virtual int? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected string _UOM;
        [PXDBString(6, IsUnicode = true)]
        [PXUIField(DisplayName = "UOM", Enabled = false)]
        public virtual string UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		protected decimal? _Qty;
        [PXDBQuantity]
		[PXUIField(DisplayName = "Qty")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
        #endregion
        #region ActualQty
        public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty> { }

        protected decimal? _ActualQty;
        [PXQuantity]
        [PXUnboundFormula(typeof(Switch<Case<Where<AMConfigResultsOption.included, 
                                                Equal<boolTrue>>,
                                            AMConfigResultsOption.qty>, 
                                            decimal0>),
                          typeof(SumCalc<AMConfigResultsFeature.totalQty>))]
        public virtual decimal? ActualQty
        {
            get
            {
                return this._ActualQty;
            }
            set
            {
                this._ActualQty = value;
            }
        }
        #endregion
        #region pricing
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        protected Int64? _CuryInfoID;
        [PXDBLong]
        [CurrencyInfo(typeof(AMConfigurationResults.curyInfoID))]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
        #region CuryUnitPrice
        public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        protected Decimal? _CuryUnitPrice;
        [PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(curyInfoID), typeof(unitPrice))]
        [PXUIField(DisplayName = "Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<qty, included>))]
        public virtual Decimal? CuryUnitPrice
        {
            get
            {
                return this._CuryUnitPrice;
            }
            set
            {
                this._CuryUnitPrice = value;
            }
        }
        #endregion
        #region UnitPrice
        public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        protected Decimal? _UnitPrice;
        [PXDBPriceCost]
        public virtual Decimal? UnitPrice
        {
            get
            {
                return this._UnitPrice;
            }
            set
            {
                this._UnitPrice = value;
            }
        }
        #endregion
        #region CuryExtPrice
        public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }

        protected Decimal? _CuryExtPrice;
        [PXDBCurrency(typeof(AMConfigResultsOption.curyInfoID), typeof(AMConfigResultsOption.extPrice))]
        [PXUIField(DisplayName = "Cfg. Ext. Price")]
        [PXFormula(typeof(IsNull<Mult<Mult<AMConfigResultsOption.qty, AMConfigResultsOption.curyUnitPrice>, AMConfigResultsOption.priceFactor>, decimal0>), null)]
        // Non supplemental roll-up...
        [PXUnboundFormula(typeof(Switch<Case<Where<AMConfigResultsOption.materialType, NotEqual<AMMaterialType.supplemental>>,
                    IsNull<Mult<Mult<AMConfigResultsOption.qty, AMConfigResultsOption.curyUnitPrice>, AMConfigResultsOption.priceFactor>, decimal0>>,
                decimal0>),
            typeof(SumCalc<AMConfigurationResults.curyOptionPriceTotal>))]
        // Supplemental roll-up...
        [PXUnboundFormula(typeof(Switch<Case<Where<AMConfigResultsOption.materialType, Equal<AMMaterialType.supplemental>>,
                        IsNull<Mult<AMConfigResultsOption.qty, AMConfigResultsOption.curyUnitPrice>, decimal0>>,
                    decimal0>),
                typeof(SumCalc<AMConfigurationResults.curySupplementalPriceTotal>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryExtPrice
        {
            get
            {
                return this._CuryExtPrice;
            }
            set
            {
                this._CuryExtPrice = value;
            }
        }
        #endregion
        #region ExtPrice
        public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }

        protected Decimal? _ExtPrice;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Cfg Ext Price")]
        public virtual Decimal? ExtPrice
        {
            get
            {
                return this._ExtPrice;
            }
            set
            {
                this._ExtPrice = value;
            }
        }
        #endregion
        #endregion
        #region Available
        public abstract class available : PX.Data.BQL.BqlBool.Field<available> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Available", Enabled = false)]
        public virtual bool? Available { get; set; }
        #endregion

        #region Included
        public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Included")]
        public virtual bool? Included { get; set; }
        #endregion

        #region FixedInclude
        public abstract class fixedInclude : PX.Data.BQL.BqlBool.Field<fixedInclude> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Fixed Include", Enabled = false)]
        public virtual bool? FixedInclude { get; set; }
        #endregion

        #region ManualInclude
        public abstract class manualInclude : PX.Data.BQL.BqlBool.Field<manualInclude> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Include", Enabled = false, Visible = false)]
        public virtual bool? ManualInclude { get; set; }
        #endregion

        #region IsRemovable
        public abstract class isRemovable : PX.Data.BQL.BqlBool.Field<isRemovable> { }

        [PXBool]
        [PXUIField(DisplayName = "Is Removable", Visible = false, Enabled = false)]
        [PXDependsOnFields(typeof(manualInclude), typeof(fixedInclude))]
        public virtual bool? IsRemovable
        {
            get
            {
                return ManualInclude == true && FixedInclude != true;
            }
        }
        #endregion

        #region Required
        public abstract class required : PX.Data.BQL.BqlBool.Field<required> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Required", Enabled = false)]
        public virtual bool? Required { get; set; }
        #endregion

        #region RuleValid
        public abstract class ruleValid : PX.Data.BQL.BqlBool.Field<ruleValid> { }

        [PXDBBool]
        [PXDefault(true)]
        public virtual bool? RuleValid { get; set; }
        #endregion

        #region System Fields
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected byte[] _tstamp;
        [PXDBTimestamp]
        public virtual byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #endregion

        #region Formula Calculated Fields
        #region QtyRequired
        public abstract class qtyRequired : PX.Data.BQL.BqlDecimal.Field<qtyRequired> { }

        protected decimal? _QtyRequired;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Qty Required", Enabled = false)]
        public virtual decimal? QtyRequired
        {
            get
            {
                return this._QtyRequired;
            }
            set
            {
                this._QtyRequired = value;
            }
        }
        #endregion
        #region MinQty
        public abstract class minQty : PX.Data.BQL.BqlDecimal.Field<minQty> { }

        protected decimal? _MinQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Min Qty", Enabled = false)]
        public virtual decimal? MinQty
        {
            get
            {
                return this._MinQty;
            }
            set
            {
                this._MinQty = value;
            }
        }
        #endregion
        #region MaxQty
        public abstract class maxQty : PX.Data.BQL.BqlDecimal.Field<maxQty> { }

        protected decimal? _MaxQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Max Qty", Enabled = false)]
        public virtual decimal? MaxQty
        {
            get
            {
                return this._MaxQty;
            }
            set
            {
                this._MaxQty = value;
            }
        }
        #endregion
        #region LotQty
        public abstract class lotQty : PX.Data.BQL.BqlDecimal.Field<lotQty> { }

        protected decimal? _LotQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Lot Qty", Enabled = false)]
        public virtual decimal? LotQty
        {
            get
            {
                return this._LotQty;
            }
            set
            {
                this._LotQty = value;
            }
        }
        #endregion
        #region ScrapFactor
        public abstract class scrapFactor : PX.Data.BQL.BqlDecimal.Field<scrapFactor> { }

        protected decimal? _ScrapFactor;
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Scrap Factor", Enabled = false)]
        public virtual decimal? ScrapFactor
        {
            get
            {
                return this._ScrapFactor;
            }
            set
            {
                this._ScrapFactor = value;
            }
        }
        #endregion
        #region PriceFactor
        public abstract class priceFactor : PX.Data.BQL.BqlDecimal.Field<priceFactor> { }

        protected decimal? _PriceFactor;
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Price Factor", Enabled = false)]
        public virtual decimal? PriceFactor
        {
            get
            {
                return this._PriceFactor;
            }
            set
            {
                this._PriceFactor = value;
            }
        }
        #endregion
        #endregion

        #region MaterialType
        public abstract class materialType : PX.Data.BQL.BqlInt.Field<materialType> { }

        protected int? _MaterialType;
        [PXDBInt]
        [PXDefault(AMMaterialType.Regular)]
        [PXUIField(DisplayName = "Material Type")]
        [AMMaterialType.ConfigList]
        public virtual int? MaterialType
        {
            get
            {
                return this._MaterialType;
            }
            set
            {
                this._MaterialType = value;
            }
        }
        #endregion

        #region Unbound Fields

        public abstract class minLotMaxQty : IBqlField { }
        [PXUIField(DisplayName = "Min/Lot/Max Qty", Enabled = false)]
        [CombineInfo(typeof(AMConfigResultsOption.minQty), typeof(AMConfigResultsOption.lotQty), typeof(AMConfigResultsOption.maxQty))]
        public virtual string MinLotMaxQty { get; set; }

        public abstract class selected : PX.Data.IBqlField
        {
        }
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }

        
        #endregion
    }
}