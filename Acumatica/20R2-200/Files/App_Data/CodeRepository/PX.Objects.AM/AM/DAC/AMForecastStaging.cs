using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Forecast Staging")]
    public class AMForecastStaging : IBqlTable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected", Enabled = true)]
        public virtual bool? Selected
        {
            get { return _Selected; }
            set { _Selected = value; }
        }

        #endregion
        #region BeginDate
        public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }

        protected DateTime? _BeginDate;
        [PXDBDate(IsKey = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Begin Date", Required = true)]
        public virtual DateTime? BeginDate
        {
            get
            {
                return this._BeginDate;
            }
            set
            {
                this._BeginDate = value;
            }
        }
        #endregion
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

        protected DateTime? _EndDate;
        [PXDBDate(IsKey = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "End Date", Required = true)]
        public virtual DateTime? EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem(IsKey = true, Required = true)]
        [PXDefault]
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(IsKey = true)]
        public virtual Int32? SubItemID
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
        #region ForecastQty
        public abstract class forecastQty : PX.Data.BQL.BqlDecimal.Field<forecastQty> { }

        protected Decimal? _ForecastQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal,"0.000000")]
        [PXUIField(DisplayName = "Quantity")]
        public virtual Decimal? ForecastQty
        {
            get
            {
                return this._ForecastQty;
            }
            set
            {
                this._ForecastQty = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<AMForecastStaging.inventoryID>>>>))]
        [INUnit(typeof(AMForecastStaging.inventoryID), Required = true)]
        public virtual String UOM
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(IsKey = true, Required = true)]
        [PXDefault(typeof(Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current<AMForecastStaging.inventoryID>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
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
        #region ChangeUnits
        public abstract class changeUnits : PX.Data.BQL.BqlDecimal.Field<changeUnits> { }

        protected Decimal? _ChangeUnits;
        [PXQuantity]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Change in Units", Enabled = false)]
        [PXFormula(typeof(Sub<AMForecastStaging.forecastQty, AMForecastStaging.lastYearSalesQty>))]
        public virtual Decimal? ChangeUnits
        {
            get
            {
                return this._ChangeUnits;
            }
            set
            {
                this._ChangeUnits = value;
            }
        }
        #endregion
        #region PercentChange
        public abstract class percentChange : PX.Data.BQL.BqlDecimal.Field<percentChange> { }

        protected Decimal? _PercentChange;
        [PXDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Percent Change", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMForecastStaging.lastYearSalesQty, IsNull, Or<AMForecastStaging.lastYearSalesQty, Equal<decimal0>>>, decimal0>,
            Div<AMForecastStaging.changeUnits, AMForecastStaging.lastYearSalesQty>>))]
        public virtual Decimal? PercentChange
        {
            get
            {
                return this._PercentChange;
            }
            set
            {
                this._PercentChange = value;
            }
        }
        #endregion
        #region Seasonality
        public abstract class seasonality : PX.Data.BQL.BqlString.Field<seasonality> { }

        protected String _Seasonality;
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Seasonality", Visible = false, Enabled = false)]
        public virtual String Seasonality
        {
            get
            {
                return this._Seasonality;
            }
            set
            {
                this._Seasonality = value;
            }
        }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        protected Int32? _CustomerID;
        [Customer]
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
        #region Dependent
        public abstract class dependent : PX.Data.BQL.BqlBool.Field<dependent> { }

        protected Boolean? _Dependent;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Dependent")]
        public virtual Boolean? Dependent
        {
            get
            {
                return this._Dependent;
            }
            set
            {
                this._Dependent = value;
            }
        }
        #endregion
        #region UserID
        public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

        protected Guid? _UserID;
        [PXDBGuid(IsKey = true)]
        [PXDefault(typeof(AccessInfo.userID))]
        public virtual Guid? UserID
        {
            get
            {
                return this._UserID;
            }
            set
            {
                this._UserID = value;
            }
        }
        #endregion
        #region LastYearBaseQty
        public abstract class lastYearBaseQty : PX.Data.BQL.BqlDecimal.Field<lastYearBaseQty> { }

        protected Decimal? _LastYearBaseQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.000000")]
        [PXUIField(DisplayName = "Last Year Base", Visible = false, Enabled = false)]
        public virtual Decimal? LastYearBaseQty
        {
            get
            {
                return this._LastYearBaseQty;
            }
            set
            {
                this._LastYearBaseQty = value;
            }
        }
        #endregion
        #region LastYearSalesQty
        public abstract class lastYearSalesQty : PX.Data.BQL.BqlDecimal.Field<lastYearSalesQty> { }

        protected Decimal? _LastYearSalesQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.000000")]
        [PXUIField(DisplayName = "Last Year", Visible = false, Enabled = false)]
        public virtual Decimal? LastYearSalesQty
        {
            get
            {
                return this._LastYearSalesQty;
            }
            set
            {
                this._LastYearSalesQty = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp]
        public virtual Byte[] tstamp
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
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID
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
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID
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
    }
}
