using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.GL;
using Location = PX.Objects.CR.Location;

namespace PX.Objects.AM
{
    // +
    // + Index created for projections: IX_AMConfigurationResults_AMConfigurationKeysProjection
    // +

    /// <summary>
    /// Projection to display Configuration Keys of most recent completed Keys (no duplicate KeyIDs displayed)
    /// </summary>
    [Serializable]
    [PXCacheName("Configuration Keys")]
    [PXProjection(typeof(Select2<AMConfigurationResults,
        InnerJoin<AMConfigurationKeysAggregate, 
            On<AMConfigurationResults.configResultsID, Equal<AMConfigurationKeysAggregate.configResultsID>>>>))]
    public class AMConfigurationKeys : IBqlTable
    {
        #region ConfigResultsID
        public abstract class configResultsID : PX.Data.BQL.BqlInt.Field<configResultsID> { }

        protected int? _ConfigResultsID;
        [PXDBInt(IsKey = true, BqlField = typeof(AMConfigurationResults.configResultsID))]
        [PXUIField(DisplayName = "Config Results ID", Visible = false, Enabled = false)]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected int? _InventoryID;
        [Inventory(Enabled = false, BqlField = typeof(AMConfigurationResults.inventoryID))]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(Enabled = false, BqlField = typeof(AMConfigurationResults.siteID))]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region ConfigurationID
        public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

        protected string _ConfigurationID;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMConfigurationResults.configurationID))]
        [PXDefault]
        [PXUIField(DisplayName = "Configuration ID", Enabled = false)]
        [PXSelector(
            typeof(Search<AMConfiguration.configurationID,
                Where<AMConfiguration.inventoryID,
                    Equal<Current<AMConfigurationResults.inventoryID>>>>),
            typeof(AMConfiguration.configurationID),
            typeof(AMConfiguration.descr))]
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

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC", BqlField = typeof(AMConfigurationResults.revision))]
        [PXUIField(DisplayName = "Conf. Revision", Visible = false, Enabled = false)]
        [PXDefault]
        [PXSelector(typeof(Search<AMConfiguration.revision, Where<AMConfiguration.configurationID, Equal<Optional<AMConfigurationResults.configurationID>>>>), typeof(AMConfiguration.revision), typeof(AMConfiguration.descr))]
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
        #region DocDate
        public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

        protected DateTime? _DocDate;

        /// <summary>
        /// The date of the document.
        /// </summary>
        /// <value>
        /// Defaults to the current <see cref="AccessInfo.BusinessDate">Business Date</see>.
        /// </value>
        [PXDBDate(BqlField = typeof(AMConfigurationResults.docDate))]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual DateTime? DocDate
        {
            get
            {
                return this._DocDate;
            }
            set
            {
                this._DocDate = value;
            }
        }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        protected Int32? _CustomerID;

        /// <summary>
        /// Identifier of the <see cref="Customer"/>, whom the document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="BAccount.BAccountID"/> field.
        /// </value>
        [Customer(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Enabled = false, 
            BqlField = typeof(AMConfigurationResults.customerID))]
        public virtual Int32? CustomerID
        {
            get
            {
                return this._CustomerID;
            }
            set
            {
                this._CustomerID = value;
            }
        }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        protected Int32? _CustomerLocationID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.CR.Location"/> of the Customer.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="BAccount.DefLocationID">Default Location</see> of the <see cref="CustomerID">Customer</see> if it is specified,
        /// or to the first found <see cref="PX.Objects.CR.Location"/>, associated with the Customer.
        /// Corresponds to the <see cref="PX.Objects.CR.Location.LocationID"/> field.
        /// </value>
        [LocationID(typeof(Where<Location.bAccountID, Equal<Optional<customerID>>,
            And<Location.isActive, Equal<True>,
                And<MatchWithBranch<Location.cBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, Enabled = false,
            BqlField = typeof(AMConfigurationResults.customerLocationID))]
        public virtual Int32? CustomerLocationID
        {
            get
            {
                return this._CustomerLocationID;
            }
            set
            {
                this._CustomerLocationID = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected string _UOM;
        [PXDBString(6, IsUnicode = true, BqlField = typeof(AMConfigurationResults.uOM))]
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
        [PXDBQuantity(BqlField = typeof(AMConfigurationResults.qty))]
        [PXUIField(DisplayName = "Qty")]
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
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

        protected String _CuryID;

        /// <summary>
        /// The code of the <see cref="System.Currency"/> of the document.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// Corresponds to the <see cref="System.Currency.CuryID"/> field.
        /// </value>
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(AMConfigurationResults.curyID))]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault(typeof(Search<Company.baseCuryID>))]
        [PXSelector(typeof(Currency.curyID))]
        public virtual String CuryID
        {
            get
            {
                return this._CuryID;
            }
            set
            {
                this._CuryID = value;
            }
        }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        protected Int64? _CuryInfoID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> object associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyInfoID"/> field.
        /// </value>
        [PXDBLong(BqlField = typeof(AMConfigurationResults.curyInfoID))]
        [CurrencyInfo(typeof(SOOrder.curyInfoID))]
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
        #region CuryOptionPriceTotal
        public abstract class curyOptionPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyOptionPriceTotal> { }

        protected Decimal? _CuryOptionPriceTotal;

        /// <summary>
        /// The total amount of the non supplemental <see cref="AMConfigResultsOption">lines</see> of the document.
        /// Given in the <see cref="CuryID">currency of the document</see>.
        /// </summary>
        [PXDBCurrency(typeof(curyInfoID), typeof(optionPriceTotal), BqlField = typeof(AMConfigurationResults.curyOptionPriceTotal))]
        [PXUIField(DisplayName = "Option price Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryOptionPriceTotal
        {
            get
            {
                return this._CuryOptionPriceTotal;
            }
            set
            {
                this._CuryOptionPriceTotal = value;
            }
        }
        #endregion
        #region OptionPriceTotal
        public abstract class optionPriceTotal : PX.Data.BQL.BqlDecimal.Field<optionPriceTotal> { }

        protected Decimal? _OptionPriceTotal;

        /// <summary>
        /// The total amount of the non supplemental <see cref="AMConfigResultsOption">lines</see> of the document.
        /// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </summary>
        [PXDBDecimal(4, BqlField = typeof(AMConfigurationResults.optionPriceTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? OptionPriceTotal
        {
            get
            {
                return this._OptionPriceTotal;
            }
            set
            {
                this._OptionPriceTotal = value;
            }
        }
        #endregion
        #region CurySupplementalPriceTotal
        public abstract class curySupplementalPriceTotal : PX.Data.BQL.BqlDecimal.Field<curySupplementalPriceTotal> { }

        protected Decimal? _CurySupplementalPriceTotal;

        /// <summary>
        /// The total amount of the supplemental <see cref="AMConfigResultsOption">lines</see> of the document.
        /// Given in the <see cref="CuryID">currency of the document</see>.
        /// </summary>
        [PXDBCurrency(typeof(curyInfoID), typeof(supplementalOptionPriceTotal), BqlField = typeof(AMConfigurationResults.curySupplementalPriceTotal))]
        [PXUIField(DisplayName = "Option price Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CurySupplementalPriceTotal
        {
            get
            {
                return this._CurySupplementalPriceTotal;
            }
            set
            {
                this._CurySupplementalPriceTotal = value;
            }
        }
        #endregion
        #region SupplementalPriceTotal
        public abstract class supplementalOptionPriceTotal : PX.Data.BQL.BqlDecimal.Field<supplementalOptionPriceTotal> { }

        protected Decimal? _SupplementalPriceTotal;

        /// <summary>
        /// The total amount of the supplemental <see cref="AMConfigResultsOption">lines</see> of the document.
        /// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </summary>
        [PXDBDecimal(4, BqlField = typeof(AMConfigurationResults.supplementalOptionPriceTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? SupplementalPriceTotal
        {
            get
            {
                return this._SupplementalPriceTotal;
            }
            set
            {
                this._SupplementalPriceTotal = value;
            }
        }
        #endregion
        #region CuryBOMPriceTotal
        public abstract class curyBOMPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyBOMPriceTotal> { }

        protected Decimal? _CuryBOMPriceTotal;

        /// <summary>
        /// The total amount of the <see cref="AMBomMatl">lines</see> of the related Bill of Material.
        /// Given in the <see cref="CuryID">currency of the document</see>.
        /// </summary>
        [PXDBCurrency(typeof(curyInfoID), typeof(bOMPriceTotal), BqlField = typeof(AMConfigurationResults.curyBOMPriceTotal))]
        [PXUIField(DisplayName = "BOM Price Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryBOMPriceTotal
        {
            get
            {
                return this._CuryBOMPriceTotal;
            }
            set
            {
                this._CuryBOMPriceTotal = value;
            }
        }
        #endregion
        #region BOMPriceTotal
        public abstract class bOMPriceTotal : PX.Data.BQL.BqlDecimal.Field<bOMPriceTotal> { }

        protected Decimal? _BOMPriceTotal;

        /// <summary>
        /// The total amount of the <see cref="AMBomMatl">lines</see> of the related Bill of Material.
        /// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </summary>
        [PXDBDecimal(4, BqlField = typeof(AMConfigurationResults.bOMPriceTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BOMPriceTotal
        {
            get
            {
                return this._BOMPriceTotal;
            }
            set
            {
                this._BOMPriceTotal = value;
            }
        }
        #endregion
        #region CuryFixedPriceTotal
        public abstract class curyFixedPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyFixedPriceTotal> { }

        protected Decimal? _CuryFixedPriceTotal;

        /// <summary>
        /// The total amount of the <see cref="AMConfigResultsFixed">lines</see> of the document.
        /// Given in the <see cref="CuryID">currency of the document</see>.
        /// </summary>
        [PXDBCurrency(typeof(curyInfoID), typeof(fixedPriceTotal), BqlField = typeof(AMConfigurationResults.curyFixedPriceTotal))]
        [PXUIField(DisplayName = "Fixed Price Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryFixedPriceTotal
        {
            get
            {
                return this._CuryFixedPriceTotal;
            }
            set
            {
                this._CuryFixedPriceTotal = value;
            }
        }
        #endregion
        #region FixedPriceTotal
        public abstract class fixedPriceTotal : PX.Data.BQL.BqlDecimal.Field<fixedPriceTotal> { }

        protected Decimal? _FixedPriceTotal;

        /// <summary>
        /// The total amount of the <see cref="AMConfigResultsFixed">lines</see> of the document.
        /// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </summary>
        [PXDBDecimal(4, BqlField = typeof(AMConfigurationResults.fixedPriceTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? FixedPriceTotal
        {
            get
            {
                return this._FixedPriceTotal;
            }
            set
            {
                this._FixedPriceTotal = value;
            }
        }
        #endregion
        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        protected bool? _Completed;
        [PXDBBool(BqlField = typeof(AMConfigurationResults.completed))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public virtual bool? Completed
        {
            get
            {
                return this._Completed;
            }
            set
            {
                this._Completed = value;
            }
        }
        #endregion
        #region Closed
        /// <summary>
        /// Indicates if the configuration is closed - no more edit.
        /// Most likely related to linked sales order/opportunity is closed/canceled
        /// </summary>
        public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }

        protected bool? _Closed;
        /// <summary>
        /// Indicates if the configuration is closed - no more edit.
        /// Most likely related to linked sales order/opportunity is closed/canceled
        /// </summary>
        [PXDBBool(BqlField = typeof(AMConfigurationResults.closed))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Closed", Enabled = false)]
        public virtual bool? Closed
        {
            get
            {
                return this._Closed;
            }
            set
            {
                this._Closed = value;
            }
        }
        #endregion
        #region ProdOrderType
        public abstract class prodOrderType : PX.Data.BQL.BqlString.Field<prodOrderType> { }

        protected String _ProdOrderType;
        [PXDBString(2, IsFixed = true, BqlField = typeof(AMConfigurationResults.prodOrderType))]
        [PXUIField(DisplayName = "Prod Order Type", Visible = false, Enabled = false)]
        public virtual String ProdOrderType
        {
            get
            {
                return this._ProdOrderType;
            }
            set
            {
                this._ProdOrderType = value;
            }
        }
        #endregion
        #region ProdOrderNbr
        public abstract class prodOrderNbr : PX.Data.BQL.BqlString.Field<prodOrderNbr> { }

        protected String _ProdOrderNbr;
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(AMConfigurationResults.prodOrderNbr))]
        [PXUIField(DisplayName = "Prod Order Nbr", Visible = false, Enabled = false)]
        public virtual String ProdOrderNbr
        {
            get
            {
                return this._ProdOrderNbr;
            }
            set
            {
                this._ProdOrderNbr = value;
            }
        }
        #endregion
        #region OrdLineRef
        public abstract class ordLineRef : PX.Data.BQL.BqlInt.Field<ordLineRef> { }

        protected int? _OrdLineRef;
        [PXDBInt(BqlField = typeof(AMConfigurationResults.ordLineRef))]
        [PXUIField(DisplayName = "SO Line Nbr.", Enabled = false)]
        public virtual int? OrdLineRef
        {
            get
            {
                return this._OrdLineRef;
            }
            set
            {
                this._OrdLineRef = value;
            }
        }
        #endregion
        #region OrdTypeRef
        public abstract class ordTypeRef : PX.Data.BQL.BqlString.Field<ordTypeRef> { }

        protected String _OrdTypeRef;
        [PXDBString(2, IsFixed = true, BqlField = typeof(AMConfigurationResults.ordTypeRef))]
        [PXUIField(DisplayName = "SO Order Type", Enabled = false)]
        public virtual String OrdTypeRef
        {
            get
            {
                return this._OrdTypeRef;
            }
            set
            {
                this._OrdTypeRef = value;
            }
        }
        #endregion
        #region OrdNbr
        public abstract class ordNbrRef : PX.Data.BQL.BqlString.Field<ordNbrRef> { }

        protected String _OrdNbrRef;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMConfigurationResults.ordNbrRef))]
        [PXUIField(DisplayName = "SO Order Nbr", Enabled = false)]
        [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<AMConfigurationResults.ordTypeRef>>>>))]
        public virtual String OrdNbrRef
        {
            get
            {
                return this._OrdNbrRef;
            }
            set
            {
                this._OrdNbrRef = value;
            }
        }
        #endregion
        #region OrderSource
        public abstract class orderSource : PX.Data.BQL.BqlInt.Field<orderSource> { }

        protected int? _OrderSource;
        [PXDBInt(BqlField = typeof(AMConfigurationResults.orderSource))]
        [PXDefault(Attributes.OrderSource.None)]
        [PXUIField(DisplayName = "Order Source", Enabled = false)]
        [OrderSource.List]
        public virtual int? OrderSource
        {
            get
            {
                return this._OrderSource;
            }
            set
            {
                this._OrderSource = value;
            }
        }
        #endregion
        #region OpportunityQuoteID
        public abstract class opportunityQuoteID : PX.Data.BQL.BqlGuid.Field<opportunityQuoteID> { }
        [PXDBGuid(BqlField = typeof(AMConfigurationResults.opportunityQuoteID))]
        [PXUIField(DisplayName = "Opportunity Quote ID", Enabled = false, Visible = false)]
        public virtual Guid? OpportunityQuoteID { get; set; }
        #endregion
        #region OpportunityLineNbr
        public abstract class opportunityLineNbr : PX.Data.BQL.BqlInt.Field<opportunityLineNbr> { }

        protected int? _OpportunityLineNbr;
        [PXDBInt(BqlField = typeof(AMConfigurationResults.opportunityLineNbr))]
        [PXUIField(DisplayName = "Opportunity Line Nbr", Enabled = false)]
        public virtual int? OpportunityLineNbr
        {
            get
            {
                return this._OpportunityLineNbr;
            }
            set
            {
                this._OpportunityLineNbr = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime(BqlField = typeof(AMConfigurationResults.createdDateTime))]
        [PXUIField(DisplayName = "Config. Date", Enabled = false)]
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
        [PXDBCreatedByScreenID(BqlField = typeof(AMConfigurationResults.createdByScreenID))]
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
        [PXDBCreatedByID(BqlField = typeof(AMConfigurationResults.createdByID))]
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
        #region KeyID
        /// <summary>
        /// Configuration key ID which represents the key used/generated from the results of a finished configuration
        /// </summary>
        public abstract class keyID : PX.Data.BQL.BqlString.Field<keyID> { }

        protected string _KeyID;
        /// <summary>
        /// Configuration key ID which represents the key used/generated from the results of a finished configuration
        /// </summary>
        [PXDBString(120, IsUnicode = true, BqlField = typeof(AMConfigurationResults.keyID))]
        [PXUIField(DisplayName = "Config. Key", Enabled = false)]
        public virtual string KeyID
        {
            get
            {
                return this._KeyID;
            }
            set
            {
                this._KeyID = value;
            }
        }
        #endregion
        #region KeyDescription
        public abstract class keyDescription : PX.Data.BQL.BqlString.Field<keyDescription> { }

        protected string _KeyDescription;
        [PXDBString(IsUnicode = true, BqlField = typeof(AMConfigurationResults.keyDescription))]
        [PXUIField(DisplayName = "Key Description", Enabled = false)]
        public virtual string KeyDescription
        {
            get
            {
                return this._KeyDescription;
            }
            set
            {
                this._KeyDescription = value;
            }
        }
        #endregion
        #region TranDescription
        public abstract class tranDescription : PX.Data.BQL.BqlString.Field<tranDescription> { }

        protected string _TranDescription;
        [PXDBString(256, IsUnicode = true, BqlField = typeof(AMConfigurationResults.tranDescription))]
        [PXUIField(DisplayName = "Tran Description", Enabled = false)]
        public virtual string TranDescription
        {
            get
            {
                return this._TranDescription;
            }
            set
            {
                this._TranDescription = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Aggregate of keys to remove duplicates. Use this is a sub-query on configResultID for full AMConfigurationResults records
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select4<AMConfigurationResults,
        Where<AMConfigurationResults.completed, Equal<True>,
            And<AMConfigurationResults.keyID, IsNotNull>>,
        Aggregate<
            GroupBy<AMConfigurationResults.configurationID,
                GroupBy<AMConfigurationResults.keyID>>>>))]
    public class AMConfigurationKeysAggregate : IBqlTable
    {
        #region ConfigResultsID
        public abstract class configResultsID : PX.Data.BQL.BqlInt.Field<configResultsID> { }

        protected int? _ConfigResultsID;
        [PXDBInt(IsKey = true, BqlField = typeof(AMConfigurationResults.configResultsID))]
        [PXUIField(DisplayName = "Config Results ID", Visible = false, Enabled = false)]
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
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMConfigurationResults.configurationID))]
        [PXUIField(DisplayName = "Configuration ID", Enabled = false)]
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
        #region KeyID
        public abstract class keyID : PX.Data.BQL.BqlString.Field<keyID> { }

        protected string _KeyID;
        [PXDBString(120, IsUnicode = true, BqlField = typeof(AMConfigurationResults.keyID))]
        [PXUIField(DisplayName = "Config. Key", Enabled = false)]
        public virtual string KeyID
        {
            get
            {
                return this._KeyID;
            }
            set
            {
                this._KeyID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime(BqlField = typeof(AMConfigurationResults.createdDateTime))]
        [PXUIField(DisplayName = "Config. Date", Enabled = false)]
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
    }
}