using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.ScheduleItem)]
	[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMSchdItem : IBqlTable
	{
	    private string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}] Priority = {SchPriority}, Const = {ConstDate?.ToShortDateString()}, Start = {StartDate?.ToShortDateString()}, End = {EndDate?.ToShortDateString()}";

        #region Keys

        public class PK : PrimaryKeyOf<AMSchdItem>.By<orderType, prodOrdID, schdID>
        {
            public static AMSchdItem Find(PXGraph graph, string orderType, string prodOrdID, int? schdID) 
                => FindBy(graph, orderType, prodOrdID, schdID);
            public static AMSchdItem FindDirty(PXGraph graph, string orderType, string prodOrdID, int? schdID)
                => PXSelect<AMSchdItem,
                    Where<orderType, Equal<Required<orderType>>,
                        And<prodOrdID, Equal<Required<prodOrdID>>,
                        And<schdID, Equal<Required<schdID>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, schdID);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMSchdItem>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMSchdItem>.By<orderType, prodOrdID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMSchdItem>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMSchdItem>.By<siteID> { }
        }

        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.orderType))]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
		#region ProdOrdID
		public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

		protected string _ProdOrdID;
        [ProductionNbr(IsKey = true, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.prodOrdID))]
        [PXParent(typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMSchdItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMSchdItem.prodOrdID>>>>>))]
        //Require validate value equals false as not all schd items are orders in prod item. (MRP Plan for example)
        //  We want the selector to support the hyper-link on scheduling pages and/or GIs
        [ProductionOrderSelector(typeof(AMSchdItem.orderType), true, ValidateValue = false)]
		public virtual string ProdOrdID
		{
			get
			{
				return this._ProdOrdID;
			}
			set
			{
				this._ProdOrdID = value;
			}
		}
        #endregion
        #region IsPlan
        /// <summary>
        /// Indicates if the record is plan order (most likely out of MRP planning)
        /// Plan orders should not have an Actual order in AMProdItem as of 6.0 release
        /// </summary>
        public abstract class isPlan : PX.Data.BQL.BqlBool.Field<isPlan> { }

        protected bool? _IsPlan;
        /// <summary>
        /// Indicates if the record is plan order (most likely out of MRP planning)
        /// Plan orders should not have an Actual order in AMProdItem as of 6.0 release
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Plan", Enabled = false)]
        public virtual bool? IsPlan
        {
            get
            {
                return this._IsPlan;
            }
            set
            {
                this._IsPlan = value;
            }
        }
        #endregion
        #region IsMRP
        /// <summary>
        /// When MRP Reruns it will reset the IsPlan records for this field back to false.
        /// Then rerun and if the same order is picked back up to reschedule it will contain a true value
        /// </summary>
        public abstract class isMRP : PX.Data.BQL.BqlBool.Field<isMRP> { }

        protected bool? _IsMRP;
        /// <summary>
        /// When MRP Reruns it will reset the IsPlan records for this field back to false.
        /// Then rerun and if the same order is picked back up to reschedule it will contain a true value
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is MRP", Enabled = false)]
        public virtual bool? IsMRP
        {
            get
            {
                return this._IsMRP;
            }
            set
            {
                this._IsMRP = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem(Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXDefault]
        [Site(Enabled = false)]
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
		#region SchdID
		public abstract class schdID : PX.Data.BQL.BqlInt.Field<schdID> { }

		protected int? _SchdID;
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Schedule ID", Enabled = false, Visible = false)]
		public virtual int? SchdID
		{
			get
			{
				return this._SchdID;
			}
			set
			{
				this._SchdID = value;
			}
		}
		#endregion
		#region ConstDate
		public abstract class constDate : PX.Data.BQL.BqlDateTime.Field<constDate> { }

		protected DateTime? _ConstDate;
	    [PXDBDate]
	    [PXDefault(typeof(AccessInfo.businessDate))]
	    [PXUIField(DisplayName = "Constraint", Enabled = false)]
        public virtual DateTime? ConstDate
		{
			get
			{
				return this._ConstDate;
			}
			set
			{
				this._ConstDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		protected DateTime? _EndDate;
		[PXDBDate]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "End Date", Enabled = false)]
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
		#region QtyComplete
        /// <summary>
        /// Order Base Unit Qty Complete
        /// </summary>
		public abstract class qtyComplete : PX.Data.BQL.BqlDecimal.Field<qtyComplete> { }

		protected decimal? _QtyComplete;
        /// <summary>
        /// Order Base Unit Qty Complete
        /// </summary>
        [PXDBQuantity]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Qty Complete", Enabled = false)]
		public virtual decimal? QtyComplete
		{
			get
			{
				return this._QtyComplete;
			}
			set
			{
				this._QtyComplete = value;
			}
		}
		#endregion
		#region QtyScrapped
        /// <summary>
        /// Order Base Unit Qty Scrapped
        /// </summary>
		public abstract class qtyScrapped : PX.Data.BQL.BqlDecimal.Field<qtyScrapped> { }

		protected decimal? _QtyScrapped;
        /// <summary>
        /// Order Base Unit Qty Scrapped
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty Scrapped", Enabled = false)]
		public virtual decimal? QtyScrapped
		{
			get
			{
				return this._QtyScrapped;
			}
			set
			{
				this._QtyScrapped = value;
			}
		}
		#endregion
		#region QtytoProd
        /// <summary>
        /// Order Base Unit Qty to Produce
        /// </summary>
		public abstract class qtytoProd : PX.Data.BQL.BqlDecimal.Field<qtytoProd> { }

		protected decimal? _QtytoProd;
        /// <summary>
        /// Order Base Unit Qty to Produce
        /// </summary>
		[PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty to Produce", Enabled = false)]
		public virtual decimal? QtytoProd
		{
			get
			{
				return this._QtytoProd;
			}
			set
			{
				this._QtytoProd = value;
			}
		}
        #endregion
	    #region QtyRemaining

	    public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

	    protected Decimal? _QtyRemaining;
	    [PXQuantity()]
	    [PXFormula(typeof(Switch<Case<Where<Current<AMPSetup.inclScrap>, Equal<True>>, SubNotLessThanZero<AMSchdItem.qtytoProd, Add<AMSchdItem.qtyComplete, AMSchdItem.qtyScrapped>>>,
	        SubNotLessThanZero<AMSchdItem.qtytoProd, AMSchdItem.qtyComplete>>))]
	    [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
	    [PXUIField(DisplayName = "Qty Remaining", Enabled = false)]
	    public virtual Decimal? QtyRemaining
	    {
	        get
	        {
	            return _QtyRemaining;
	        }
	        set
	        {
	            _QtyRemaining = value;
	        }
	    }
	    #endregion
	    #region SchedulingMethod
	    public abstract class schedulingMethod : PX.Data.BQL.BqlString.Field<schedulingMethod> { }

	    protected string _SchedulingMethod;
	    [PXDBString(1, IsFixed = true)]
	    [PXDefault(ScheduleMethod.StartOn)]
	    [PXUIField(DisplayName = "Scheduling Method")]
	    [ScheduleMethod.List]
	    public virtual string SchedulingMethod
	    {
	        get
	        {
	            return this._SchedulingMethod;
	        }
	        set
	        {
	            this._SchedulingMethod = value;
	        }
	    }
	    #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		protected DateTime? _StartDate;
		[PXDBDate]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
        #region SchPriority
        public abstract class schPriority : PX.Data.BQL.BqlShort.Field<schPriority> { }

        protected Int16? _SchPriority;
        [PXDBShort(MinValue = 1, MaxValue = 10)]
        [PXDefault((short)5)]
        [PXUIField(DisplayName = "Dispatch Priority", Enabled = false)]
        public virtual Int16? SchPriority
        {
            get
            {
                return this._SchPriority;
            }
            set
            {
                this._SchPriority = value;
            }
        }
        #endregion
        #region FirmSchedule
        public abstract class firmSchedule : PX.Data.BQL.BqlBool.Field<firmSchedule> { }

        protected bool? _FirmSchedule;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Firm Schedule", Enabled = false, Visible = false)]
        public virtual bool? FirmSchedule
        {
            get
            {
                return this._FirmSchedule;
            }
            set
            {
                this._FirmSchedule = value;
            }
        }
        #endregion
        #region MRPPlanID
        public abstract class mRPPlanID : PX.Data.BQL.BqlLong.Field<mRPPlanID> { }

        protected Int64? _MRPPlanID;
        [PXDBLong]
        public virtual Int64? MRPPlanID
        {
            get
            {
                return this._MRPPlanID;
            }
            set
            {
                this._MRPPlanID = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
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
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _RefNoteID;
        [PXDBGuid]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected byte[] _tstamp;
		[PXDBTimestamp()]
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
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
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
        [PXDBCreatedByScreenID()]
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
        [PXDBCreatedDateTime()]
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
        [PXDBLastModifiedByID()]
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
        [PXDBLastModifiedByScreenID()]
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
        [PXDBLastModifiedDateTime()]
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