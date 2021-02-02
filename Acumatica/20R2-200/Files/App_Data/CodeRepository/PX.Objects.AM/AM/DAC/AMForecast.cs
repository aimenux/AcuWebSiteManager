using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("ForecastID = {ForecastID}, InventoryID = {InventoryID}")]
	[Serializable]
    [PXCacheName("Forecast")]
    [PXPrimaryGraph(typeof(Forecast))]
	public class AMForecast : IBqlTable, INotable
    {
        #region ForecastID (Key)
        public abstract class forecastID : PX.Data.BQL.BqlString.Field<forecastID> { }

        protected String _ForecastID;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Forecast ID", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        [MrpNumbering.Forecast]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual String ForecastID
        {
            get
            {
                return this._ForecastID;
            }
            set
            {
                this._ForecastID = value;
            }
        }
        #endregion
        #region ActiveFlg
        public abstract class activeFlg : PX.Data.BQL.BqlBool.Field<activeFlg> { }

		protected Boolean? _ActiveFlg;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? ActiveFlg
		{
			get
			{
				return this._ActiveFlg;
			}
			set
			{
				this._ActiveFlg = value;
			}
		}
        #endregion
        #region Interval
        public abstract class interval : PX.Data.BQL.BqlString.Field<interval> { }

        protected String _Interval;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ForecastInterval.Weekly)]
        [PXUIField(DisplayName = "Interval")]
        [ForecastInterval.List]
        public virtual String Interval
        {
            get
            {
                return this._Interval;
            }
            set
            {
                this._Interval = value;
            }
        }
        #endregion
        #region BeginDate
        public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }

		protected DateTime? _BeginDate;
		[PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Begin Date")]
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
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        protected Int32? _CustomerID;
        [Customer(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName))]
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
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		protected DateTime? _EndDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "End Date")]
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
        [StockItem]
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
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMForecast.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXFormula(typeof(Default<AMForecast.inventoryID>))]
        [SubItem(typeof(AMForecast.inventoryID))]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [AutoNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity]
        public virtual Decimal? BaseQty
        {
            get
            {
                return this._BaseQty;
            }
            set
            {
                this._BaseQty = value;
            }
        }
        #endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		protected Decimal? _Qty;
        [PXDBQuantity(typeof(AMForecast.uOM), typeof(AMForecast.baseQty), MinValue = 0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<AMForecast.inventoryID>>>>))]
        [PXFormula(typeof(Default<AMForecast.inventoryID>))]
        [INUnit(typeof(AMForecast.inventoryID))]
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
        [PXDefault(typeof(Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current<AMForecast.inventoryID>>>>))]
        [PXFormula(typeof(Default<AMForecast.inventoryID>))]
        [Site]
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
        #region Dependent
        public abstract class dependent : PX.Data.BQL.BqlBool.Field<dependent> { }

        protected Boolean? _Dependent;
		[PXDBBool]
		[PXDefault(false)]
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
