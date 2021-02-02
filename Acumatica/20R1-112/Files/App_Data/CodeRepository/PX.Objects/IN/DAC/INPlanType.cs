using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INItemPlanType, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class INPlanType : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPlanType>.By<planType>
		{
			public static INPlanType Find(PXGraph graph, string planType) => FindBy(graph, planType);
		}

		#endregion
		#region planType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask=">aa")]
		[PXDefault()]
		[PXUIField(DisplayName="Plan Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region IsFixed
		public abstract class isFixed : PX.Data.BQL.BqlBool.Field<isFixed> { }
		protected Boolean? _IsFixed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName="Is Fixed", Enabled = false)]
		public virtual Boolean? IsFixed
		{
			get
			{
				return this._IsFixed;
			}
			set
			{
				this._IsFixed = value;
			}
		}
		#endregion
		#region IsSupply
		public abstract class isSupply : PX.Data.BQL.BqlBool.Field<isSupply> { }
		protected Boolean? _IsSupply;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Supply", Enabled = false)]
		public virtual Boolean? IsSupply
		{
			get
			{
				return this._IsSupply;
			}
			set
			{
				this._IsSupply = value;
			}
		}
		#endregion
		#region IsDemand
		public abstract class isDemand : PX.Data.BQL.BqlBool.Field<isDemand> { }
		protected Boolean? _IsDemand;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Demand", Enabled = false)]
		public virtual Boolean? IsDemand
		{
			get
			{
				return this._IsDemand;
			}
			set
			{
				this._IsDemand = value;
			}
		}
		#endregion
		#region IsForDate
		public abstract class isForDate : PX.Data.BQL.BqlBool.Field<isForDate> { }
		protected Boolean? _IsForDate;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Planned for Date", Enabled = false)]
		public virtual Boolean? IsForDate
		{
			get
			{
				return this._IsForDate;
			}
			set
			{
				this._IsForDate = value;
			}
		}
        #endregion

        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlShort.Field<inclQtyFSSrvOrdBooked> { }
        protected Int16? _InclQtyFSSrvOrdBooked;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "FS Booked", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual Int16? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region inclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlShort.Field<inclQtyFSSrvOrdAllocated> { }
        protected Int16? _InclQtyFSSrvOrdAllocated;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "FS Allocated", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual Int16? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion
        #region inclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlShort.Field<inclQtyFSSrvOrdPrepared> { }
        protected Int16? _InclQtyFSSrvOrdPrepared;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "FS Prepared", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual Int16? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion

        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlShort.Field<inclQtySOBackOrdered> { }
		protected Int16? _InclQtySOBackOrdered;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "SO Back Ordered", Enabled = false)]
		public virtual Int16? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOPrepared
		public abstract class inclQtySOPrepared : PX.Data.BQL.BqlShort.Field<inclQtySOPrepared> { }
		protected Int16? _InclQtySOPrepared;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "SO Prepared", Enabled = false)]
		public virtual Int16? InclQtySOPrepared
		{
			get
			{
				return this._InclQtySOPrepared;
			}
			set
			{
				this._InclQtySOPrepared = value;
			}
		}
		#endregion
		#region InclQtySOBooked
		public abstract class inclQtySOBooked : PX.Data.BQL.BqlShort.Field<inclQtySOBooked> { }
		protected Int16? _InclQtySOBooked;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "SO Booked", Enabled = false)]
		public virtual Int16? InclQtySOBooked
		{
			get
			{
				return this._InclQtySOBooked;
			}
			set
			{
				this._InclQtySOBooked = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.BQL.BqlShort.Field<inclQtySOShipped> { }
		protected Int16? _InclQtySOShipped;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "SO Shipped", Enabled = false)]
		public virtual Int16? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.BQL.BqlShort.Field<inclQtySOShipping> { }
		protected Int16? _InclQtySOShipping;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "SO Allocated", Enabled = false)]
		public virtual Int16? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region InclQtyInTransit
		public abstract class inclQtyInTransit : PX.Data.BQL.BqlShort.Field<inclQtyInTransit> { }
		protected Int16? _InclQtyInTransit;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "In-Transit", Enabled = false)]
		public virtual Int16? InclQtyInTransit
		{
			get
			{
				return this._InclQtyInTransit;
			}
			set
			{
				this._InclQtyInTransit = value;
			}
		}
		#endregion
        #region InclQtyInTransitToSO
        public abstract class inclQtyInTransitToSO : PX.Data.BQL.BqlShort.Field<inclQtyInTransitToSO> { }
        protected Int16? _InclQtyInTransitToSO;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "In-Transit to SO", Enabled = false)]
        public virtual Int16? InclQtyInTransitToSO
        {
            get
            {
                return this._InclQtyInTransitToSO;
            }
            set
            {
                this._InclQtyInTransitToSO = value;
            }
        }
        #endregion
		#region InclQtyPOReceipts
		public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlShort.Field<inclQtyPOReceipts> { }
		protected Int16? _InclQtyPOReceipts;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "PO Receipt", Enabled = false)]
		public virtual Int16? InclQtyPOReceipts
		{
			get
			{
				return this._InclQtyPOReceipts;
			}
			set
			{
				this._InclQtyPOReceipts = value;
			}
		}
		#endregion
		#region InclQtyPOPrepared
		public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlShort.Field<inclQtyPOPrepared> { }
		protected Int16? _InclQtyPOPrepared;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "PO Prepared", Enabled = false)]
		public virtual Int16? InclQtyPOPrepared
		{
			get
			{
				return this._InclQtyPOPrepared;
			}
			set
			{
				this._InclQtyPOPrepared = value;
			}
		}
		#endregion
		#region InclQtyPOOrders
		public abstract class inclQtyPOOrders : PX.Data.BQL.BqlShort.Field<inclQtyPOOrders> { }
		protected Int16? _InclQtyPOOrders;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "PO Order", Enabled = false)]
		public virtual Int16? InclQtyPOOrders
		{
			get
			{
				return this._InclQtyPOOrders;
			}
			set
			{
				this._InclQtyPOOrders = value;
			}
		}
		#endregion
		#region InclQtyINIssues
		public abstract class inclQtyINIssues : PX.Data.BQL.BqlShort.Field<inclQtyINIssues> { }
		protected Int16? _InclQtyINIssues;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "IN Issues", Enabled = false)]
		public virtual Int16? InclQtyINIssues
		{
			get
			{
				return this._InclQtyINIssues;
			}
			set
			{
				this._InclQtyINIssues = value;
			}
		}
		#endregion
		#region InclQtyINReceipts
		public abstract class inclQtyINReceipts : PX.Data.BQL.BqlShort.Field<inclQtyINReceipts> { }
		protected Int16? _InclQtyINReceipts;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "IN Receipts", Enabled = false)]
		public virtual Int16? InclQtyINReceipts
		{
			get
			{
				return this._InclQtyINReceipts;
			}
			set
			{
				this._InclQtyINReceipts = value;
			}
		}
		#endregion
		#region InclQtyINAssemblyDemand
		public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlShort.Field<inclQtyINAssemblyDemand> { }
		protected Int16? _InclQtyINAssemblyDemand;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "IN Assembly Demand", Enabled = false)]
		public virtual Int16? InclQtyINAssemblyDemand
		{
			get
			{
				return this._InclQtyINAssemblyDemand;
			}
			set
			{
				this._InclQtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region InclQtyINAssemblySupply
		public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlShort.Field<inclQtyINAssemblySupply> { }
		protected Int16? _InclQtyINAssemblySupply;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "IN Assembly Supply", Enabled = false)]
		public virtual Int16? InclQtyINAssemblySupply
		{
			get
			{
				return this._InclQtyINAssemblySupply;
			}
			set
			{
				this._InclQtyINAssemblySupply = value;
			}
        }
        #endregion
        #region InclQtyInTransitToProduction
        public abstract class inclQtyInTransitToProduction : PX.Data.BQL.BqlShort.Field<inclQtyInTransitToProduction> { }
        protected Int16? _InclQtyInTransitToProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity In Transit to Production.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "In Transit to Production", Enabled = false)]
        public virtual Int16? InclQtyInTransitToProduction
        {
            get
            {
                return this._InclQtyInTransitToProduction;
            }
            set
            {
                this._InclQtyInTransitToProduction = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlShort.Field<inclQtyProductionSupplyPrepared> { }
        protected Int16? _InclQtyProductionSupplyPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production Supply Prepared.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Production Supply Prepared", Enabled = false)]
        public virtual Int16? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlShort.Field<inclQtyProductionSupply> { }
        protected Int16? _InclQtyProductionSupply;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production Supply.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Production Supply", Enabled = false)]
        public virtual Int16? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyPOFixedProductionPrepared
        public abstract class inclQtyPOFixedProductionPrepared : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedProductionPrepared> { }
        protected Int16? _InclQtyPOFixedProductionPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Purchase for Prod. Prepared.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Purchase for Prod. Prepared", Enabled = false)]
        public virtual Int16? InclQtyPOFixedProductionPrepared
        {
            get
            {
                return this._InclQtyPOFixedProductionPrepared;
            }
            set
            {
                this._InclQtyPOFixedProductionPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOFixedProductionOrders
        public abstract class inclQtyPOFixedProductionOrders : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedProductionOrders> { }
        protected Int16? _InclQtyPOFixedProductionOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Purchase for Production.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Purchase for Production", Enabled = false)]
        public virtual Int16? InclQtyPOFixedProductionOrders
        {
            get
            {
                return this._InclQtyPOFixedProductionOrders;
            }
            set
            {
                this._InclQtyPOFixedProductionOrders = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlShort.Field<inclQtyProductionDemandPrepared> { }
        protected Int16? _InclQtyProductionDemandPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production Demand Prepared.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Production Demand Prepared", Enabled = false)]
        public virtual Int16? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlShort.Field<inclQtyProductionDemand> { }
        protected Int16? _InclQtyProductionDemand;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production Demand.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Production Demand", Enabled = false)]
        public virtual Int16? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlShort.Field<inclQtyProductionAllocated> { }
        protected Int16? _InclQtyProductionAllocated;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production Allocated.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Production Allocated", Enabled = false)]
        public virtual Int16? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtySOFixedProduction
        public abstract class inclQtySOFixedProduction : PX.Data.BQL.BqlShort.Field<inclQtySOFixedProduction> { }
        protected Int16? _InclQtySOFixedProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity SO to Production.  
        /// </summary>
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "SO to Production", Enabled = false)]
        public virtual Int16? InclQtySOFixedProduction
        {
            get
            {
                return this._InclQtySOFixedProduction;
            }
            set
            {
                this._InclQtySOFixedProduction = value;
            }
        }
        #endregion
        #region InclQtyProdFixedPurchase
        // M9
        public abstract class inclQtyProdFixedPurchase : PX.Data.BQL.BqlShort.Field<inclQtyProdFixedPurchase> { }
	    protected Int16? _InclQtyProdFixedPurchase;
	    /// <summary>
	    /// Production / Manufacturing 
	    /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production to Purchase.  
	    /// </summary>
	    [PXDBShort]
	    [PXDefault((short)0)]
	    [PXUIField(DisplayName = "Production to Purchase", Enabled = false)]
	    public virtual Int16? InclQtyProdFixedPurchase
	    {
	        get
	        {
	            return this._InclQtyProdFixedPurchase;
	        }
	        set
	        {
	            this._InclQtyProdFixedPurchase = value;
	        }
	    }
        #endregion
	    #region InclQtyProdFixedProduction
        // MA
	    public abstract class inclQtyProdFixedProduction : PX.Data.BQL.BqlShort.Field<inclQtyProdFixedProduction> { }
	    protected Int16? _InclQtyProdFixedProduction;
	    /// <summary>
	    /// Production / Manufacturing 
	    /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production to Production.  
	    /// </summary>
	    [PXDBShort]
	    [PXDefault((short)0)]
	    [PXUIField(DisplayName = "Production to Production", Enabled = false)]
	    public virtual Int16? InclQtyProdFixedProduction
	    {
	        get
	        {
	            return this._InclQtyProdFixedProduction;
	        }
	        set
	        {
	            this._InclQtyProdFixedProduction = value;
	        }
	    }
        #endregion
	    #region InclQtyProdFixedProdOrdersPrepared
	    // MB
	    public abstract class inclQtyProdFixedProdOrdersPrepared : PX.Data.BQL.BqlShort.Field<inclQtyProdFixedProdOrdersPrepared> { }
	    protected Int16? _InclQtyProdFixedProdOrdersPrepared;
	    /// <summary>
	    /// Production / Manufacturing 
	    /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production for Prod. Prepared.  
	    /// </summary>
	    [PXDBShort]
	    [PXDefault((short)0)]
	    [PXUIField(DisplayName = "Production for Prod. Prepared", Enabled = false)]
	    public virtual Int16? InclQtyProdFixedProdOrdersPrepared
	    {
	        get
	        {
	            return this._InclQtyProdFixedProdOrdersPrepared;
	        }
	        set
	        {
	            this._InclQtyProdFixedProdOrdersPrepared = value;
	        }
	    }
        #endregion
	    #region InclQtyProdFixedProdOrders
	    // MC
	    public abstract class inclQtyProdFixedProdOrders : PX.Data.BQL.BqlShort.Field<inclQtyProdFixedProdOrders> { }
	    protected Int16? _InclQtyProdFixedProdOrders;
	    /// <summary>
	    /// Production / Manufacturing 
	    /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production for Production.  
	    /// </summary>
	    [PXDBShort]
	    [PXDefault((short)0)]
	    [PXUIField(DisplayName = "Production for Production", Enabled = false)]
	    public virtual Int16? InclQtyProdFixedProdOrders
	    {
	        get
	        {
	            return this._InclQtyProdFixedProdOrders;
	        }
	        set
	        {
	            this._InclQtyProdFixedProdOrders = value;
	        }
	    }
        #endregion
	    #region InclQtyProdFixedSalesOrdersPrepared
	    // MD
	    public abstract class inclQtyProdFixedSalesOrdersPrepared : PX.Data.BQL.BqlShort.Field<inclQtyProdFixedSalesOrdersPrepared> { }
	    protected Int16? _InclQtyProdFixedSalesOrdersPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production for SO Prepared.  
        /// </summary>
        [PXDBShort]
	    [PXDefault((short)0)]
	    [PXUIField(DisplayName = "Production for SO Prepared", Enabled = false)]
	    public virtual Int16? InclQtyProdFixedSalesOrdersPrepared
	    {
	        get
	        {
	            return this._InclQtyProdFixedSalesOrdersPrepared;
	        }
	        set
	        {
	            this._InclQtyProdFixedSalesOrdersPrepared = value;
	        }
	    }
        #endregion
	    #region InclQtyProdFixedSalesOrders
	    // ME
	    public abstract class inclQtyProdFixedSalesOrders : PX.Data.BQL.BqlShort.Field<inclQtyProdFixedSalesOrders> { }
	    protected Int16? _InclQtyProdFixedSalesOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies (if set to <c>1</c>) that the plan type impacts the quantity Production for SO.  
        /// </summary>
        [PXDBShort]
	    [PXDefault((short)0)]
	    [PXUIField(DisplayName = "Production for SO", Enabled = false)]
	    public virtual Int16? InclQtyProdFixedSalesOrders
	    {
	        get
	        {
	            return this._InclQtyProdFixedSalesOrders;
	        }
	        set
	        {
	            this._InclQtyProdFixedSalesOrders = value;
	        }
	    }
	    #endregion
        #region InclQtyINReplaned
        public abstract class inclQtyINReplaned : PX.Data.BQL.BqlShort.Field<inclQtyINReplaned> { }
		protected Int16? _InclQtyINReplaned;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "IN Replaned", Enabled = false)]
		public virtual Int16? InclQtyINReplaned
		{
			get
			{
				return this._InclQtyINReplaned;
			}
			set
			{
				this._InclQtyINReplaned = value;
			}
		}
        #endregion

        #region InclQtyFixedFSSrvOrd
        public abstract class inclQtyFixedFSSrvOrd : PX.Data.BQL.BqlShort.Field<inclQtyFixedFSSrvOrd> { }
        protected short? _InclQtyFixedFSSrvOrd;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "FS to Purchase", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual short? InclQtyFixedFSSrvOrd
        {
            get
            {
                return this._InclQtyFixedFSSrvOrd;
            }
            set
            {
                this._InclQtyFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region InclQtyPOFixedFSSrvOrd
        public abstract class inclQtyPOFixedFSSrvOrd : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedFSSrvOrd> { }
        protected short? _InclQtyPOFixedFSSrvOrd;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Purchase for FS", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual short? InclQtyPOFixedFSSrvOrd
        {
            get
            {
                return this._InclQtyPOFixedFSSrvOrd;
            }
            set
            {
                this._InclQtyPOFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region InclQtyPOFixedFSSrvOrdPrepared
        public abstract class inclQtyPOFixedFSSrvOrdPrepared : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedFSSrvOrdPrepared> { }
        protected short? _InclQtyPOFixedFSSrvOrdPrepared;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Purchase for FS Prepared", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual short? InclQtyPOFixedFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyPOFixedFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyPOFixedFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOFixedFSSrvOrdReceipts
        public abstract class inclQtyPOFixedFSSrvOrdReceipts : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedFSSrvOrdReceipts> { }
        protected short? _InclQtyPOFixedFSSrvOrdReceipts;
        [PXDBShort()]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Receipts for FS", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual short? InclQtyPOFixedFSSrvOrdReceipts
        {
            get
            {
                return this._InclQtyPOFixedFSSrvOrdReceipts;
            }
            set
            {
                this._InclQtyPOFixedFSSrvOrdReceipts = value;
            }
        }
        #endregion

        #region InclQtySOFixed
        public abstract class inclQtySOFixed : PX.Data.BQL.BqlShort.Field<inclQtySOFixed> { }
		protected short? _InclQtySOFixed;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "SO to Purchase", Enabled = false)]
		public virtual short? InclQtySOFixed
		{
			get
			{
				return this._InclQtySOFixed;
			}
			set
			{
				this._InclQtySOFixed = value;
			}
		}
		#endregion
		#region InclQtyPOFixedOrders
		public abstract class inclQtyPOFixedOrders : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedOrders> { }
		protected short? _InclQtyPOFixedOrders;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Purchase for SO", Enabled = false)]
		public virtual short? InclQtyPOFixedOrders
		{
			get
			{
				return this._InclQtyPOFixedOrders;
			}
			set
			{
				this._InclQtyPOFixedOrders = value;
			}
		}
		#endregion
		#region InclQtyPOFixedPrepared
		public abstract class inclQtyPOFixedPrepared : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedPrepared> { }
		protected short? _InclQtyPOFixedPrepared;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Purchase for SO Prepared", Enabled = false)]
		public virtual short? InclQtyPOFixedPrepared
		{
			get
			{
				return this._InclQtyPOFixedPrepared;
			}
			set
			{
				this._InclQtyPOFixedPrepared = value;
			}
		}
		#endregion
		#region InclQtyPOFixedReceipts
		public abstract class inclQtyPOFixedReceipts : PX.Data.BQL.BqlShort.Field<inclQtyPOFixedReceipts> { }
		protected short? _InclQtyPOFixedReceipts;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Receipts for SO", Enabled = false)]
		public virtual short? InclQtyPOFixedReceipts
		{
			get
			{
				return this._InclQtyPOFixedReceipts;
			}
			set
			{
				this._InclQtyPOFixedReceipts = value;
			}
		}
		#endregion
		#region InclQtySODropShip
		public abstract class inclQtySODropShip : PX.Data.BQL.BqlShort.Field<inclQtySODropShip> { }
		protected short? _InclQtySODropShip;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "SO to Drop-Ship", Enabled = false)]
		public virtual short? InclQtySODropShip
		{
			get
			{
				return this._InclQtySODropShip;
			}
			set
			{
				this._InclQtySODropShip = value;
			}
		}
		#endregion
		#region InclQtyPODropShipOrders
		public abstract class inclQtyPODropShipOrders : PX.Data.BQL.BqlShort.Field<inclQtyPODropShipOrders> { }
		protected short? _InclQtyPODropShipOrders;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Drop-Ship for SO", Enabled = false)]
		public virtual short? InclQtyPODropShipOrders
		{
			get
			{
				return this._InclQtyPODropShipOrders;
			}
			set
			{
				this._InclQtyPODropShipOrders = value;
			}
		}
		#endregion
		#region InclQtyPODropShipPrepared
		public abstract class inclQtyPODropShipPrepared : PX.Data.BQL.BqlShort.Field<inclQtyPODropShipPrepared> { }
		protected short? _InclQtyPODropShipPrepared;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Drop-Ship for SO Prepared", Enabled = false)]
		public virtual short? InclQtyPODropShipPrepared
		{
			get
			{
				return this._InclQtyPODropShipPrepared;
			}
			set
			{
				this._InclQtyPODropShipPrepared = value;
			}
		}
		#endregion
		#region InclQtyPODropShipReceipts
		public abstract class inclQtyPODropShipReceipts : PX.Data.BQL.BqlShort.Field<inclQtyPODropShipReceipts> { }
		protected short? _InclQtyPODropShipReceipts;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Drop-Ship for SO Receipts", Enabled = false)]
		public virtual short? InclQtyPODropShipReceipts
		{
			get
			{
				return this._InclQtyPODropShipReceipts;
			}
			set
			{
				this._InclQtyPODropShipReceipts = value;
			}
		}
        #endregion
        #region DeleteOnEvent
        public abstract class deleteOnEvent : PX.Data.BQL.BqlBool.Field<deleteOnEvent> { }
		protected Boolean? _DeleteOnEvent;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Delete On Event", Enabled = false)]
		public virtual Boolean? DeleteOnEvent
		{
			get
			{
				return this._DeleteOnEvent;
			}
			set
			{
				this._DeleteOnEvent = value;
			}
		}
		#endregion
		#region ReplanOnEvent
		public abstract class replanOnEvent : PX.Data.BQL.BqlString.Field<replanOnEvent> { }
		protected String _ReplanOnEvent;
		[PXDBString(2, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Replan On Event")]
		public virtual String ReplanOnEvent
		{
			get
			{
				return this._ReplanOnEvent;
			}
			set
			{
				this._ReplanOnEvent = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
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
		#region Methods
		public static INPlanType operator -(INPlanType t1)
		{
			INPlanType ret = PXCache<INPlanType>.CreateCopy(t1);
			ret.InclQtyINIssues = (short)-ret.InclQtyINIssues;
			ret.InclQtyINReceipts = (short)-ret.InclQtyINReceipts;
			ret.InclQtyInTransit = (short)-ret.InclQtyInTransit;
            ret.InclQtyInTransitToSO = (short)-ret.InclQtyInTransitToSO;
            ret.InclQtyPOReceipts = (short)-ret.InclQtyPOReceipts;
			ret.InclQtyPOPrepared = (short)-ret.InclQtyPOPrepared;
			ret.InclQtyPOOrders = (short)-ret.InclQtyPOOrders;

            ret.InclQtyFSSrvOrdBooked = (short)-ret.InclQtyFSSrvOrdBooked;
            ret.InclQtyFSSrvOrdAllocated = (short)-ret.InclQtyFSSrvOrdAllocated;
            ret.InclQtyFSSrvOrdPrepared = (short)-ret.InclQtyFSSrvOrdPrepared;

            ret.InclQtySOBackOrdered = (short)-ret.InclQtySOBackOrdered;
			ret.InclQtySOPrepared = (short)-ret.InclQtySOPrepared;
			ret.InclQtySOBooked = (short)-ret.InclQtySOBooked;
			ret.InclQtySOShipped = (short)-ret.InclQtySOShipped;
			ret.InclQtySOShipping = (short)-ret.InclQtySOShipping;
			ret.InclQtyINAssemblySupply = (short)-ret.InclQtyINAssemblySupply;
			ret.InclQtyINAssemblyDemand = (short)-ret.InclQtyINAssemblyDemand;
            ret.InclQtyInTransitToProduction = (short)-ret.InclQtyInTransitToProduction;
            ret.InclQtyProductionSupplyPrepared = (short)-ret.InclQtyProductionSupplyPrepared;
            ret.InclQtyProductionSupply = (short)-ret.InclQtyProductionSupply;
            ret.InclQtyPOFixedProductionPrepared = (short)-ret.InclQtyPOFixedProductionPrepared;
            ret.InclQtyPOFixedProductionOrders = (short)-ret.InclQtyPOFixedProductionOrders;
            ret.InclQtyProductionDemandPrepared = (short)-ret.InclQtyProductionDemandPrepared;
            ret.InclQtyProductionDemand = (short)-ret.InclQtyProductionDemand;
            ret.InclQtyProductionAllocated = (short)-ret.InclQtyProductionAllocated;
            ret.InclQtySOFixedProduction = (short)-ret.InclQtySOFixedProduction;
		    ret.InclQtyProdFixedPurchase = (short)-ret.InclQtyProdFixedPurchase;
		    ret.InclQtyProdFixedProduction = (short)-ret.InclQtyProdFixedProduction;
		    ret.InclQtyProdFixedProdOrdersPrepared = (short)-ret.InclQtyProdFixedProdOrdersPrepared;
		    ret.InclQtyProdFixedProdOrders = (short)-ret.InclQtyProdFixedProdOrders;
		    ret.InclQtyProdFixedSalesOrdersPrepared = (short)-ret.InclQtyProdFixedSalesOrdersPrepared;
		    ret.InclQtyProdFixedSalesOrders = (short)-ret.InclQtyProdFixedSalesOrders;
            ret.InclQtyINReplaned = (short)-ret.InclQtyINReplaned;

            ret.InclQtyFixedFSSrvOrd = (short)-ret.InclQtyFixedFSSrvOrd;
            ret.InclQtyPOFixedFSSrvOrd = (short)-ret.InclQtyPOFixedFSSrvOrd;
            ret.InclQtyPOFixedFSSrvOrdPrepared = (short)-ret.InclQtyPOFixedFSSrvOrdPrepared;
            ret.InclQtyPOFixedFSSrvOrdReceipts = (short)-ret.InclQtyPOFixedFSSrvOrdReceipts;

            ret.InclQtySOFixed = (short)-ret.InclQtySOFixed;
			ret.InclQtyPOFixedOrders = (short)-ret.InclQtyPOFixedOrders;
			ret.InclQtyPOFixedPrepared = (short)-ret.InclQtyPOFixedPrepared;
			ret.InclQtyPOFixedReceipts = (short)-ret.InclQtyPOFixedReceipts;
			ret.InclQtySODropShip = (short)-ret.InclQtySODropShip;
			ret.InclQtyPODropShipOrders = (short)-ret.InclQtyPODropShipOrders;
			ret.InclQtyPODropShipPrepared = (short)-ret.InclQtyPODropShipPrepared;
			ret.InclQtyPODropShipReceipts = (short)-ret.InclQtyPODropShipReceipts;
            
            return ret;
		}

		public static INPlanType operator -(INPlanType t1, INPlanType t2)
		{
			INPlanType ret = PXCache<INPlanType>.CreateCopy(t1);
			ret.InclQtyINIssues = (short)(t1.InclQtyINIssues - t2.InclQtyINIssues);
			ret.InclQtyINReceipts = (short)(t1.InclQtyINReceipts - t2.InclQtyINReceipts);
			ret.InclQtyInTransit = (short)(t1.InclQtyInTransit - t2.InclQtyInTransit);
            ret.InclQtyInTransitToSO = (short)(t1.InclQtyInTransitToSO - t2.InclQtyInTransitToSO);
            ret.InclQtyPOReceipts = (short)(t1.InclQtyPOReceipts - t2.InclQtyPOReceipts);
			ret.InclQtyPOPrepared = (short)(t1.InclQtyPOPrepared - t2.InclQtyPOPrepared);
			ret.InclQtyPOOrders = (short)(t1.InclQtyPOOrders - t2.InclQtyPOOrders);

            ret.InclQtyFSSrvOrdBooked = (short)(t1.InclQtyFSSrvOrdBooked - t2.InclQtyFSSrvOrdBooked);
            ret.InclQtyFSSrvOrdAllocated = (short)(t1.InclQtyFSSrvOrdAllocated - t2.InclQtyFSSrvOrdAllocated);
            ret.InclQtyFSSrvOrdPrepared = (short)(t1.InclQtyFSSrvOrdPrepared - t2.InclQtyFSSrvOrdPrepared);

            ret.InclQtySOBackOrdered = (short)(t1.InclQtySOBackOrdered - t2.InclQtySOBackOrdered);
			ret.InclQtySOPrepared = (short)(t1.InclQtySOPrepared - t2.InclQtySOPrepared);
			ret.InclQtySOBooked = (short)(t1.InclQtySOBooked - t2.InclQtySOBooked);
			ret.InclQtySOShipped = (short)(t1.InclQtySOShipped - t2.InclQtySOShipped);
			ret.InclQtySOShipping = (short)(t1.InclQtySOShipping - t2.InclQtySOShipping);
			ret.InclQtyINAssemblySupply = (short)(t1.InclQtyINAssemblySupply - t2.InclQtyINAssemblySupply);
			ret.InclQtyINAssemblyDemand = (short)(t1.InclQtyINAssemblyDemand - t2.InclQtyINAssemblyDemand);
            ret.InclQtyInTransitToProduction = (short)(t1.InclQtyInTransitToProduction - t2.InclQtyInTransitToProduction);
            ret.InclQtyProductionSupplyPrepared = (short)(t1.InclQtyProductionSupplyPrepared - t2.InclQtyProductionSupplyPrepared);
            ret.InclQtyProductionSupply = (short)(t1.InclQtyProductionSupply - t2.InclQtyProductionSupply);
            ret.InclQtyPOFixedProductionPrepared = (short)(t1.InclQtyPOFixedProductionPrepared - t2.InclQtyPOFixedProductionPrepared);
            ret.InclQtyPOFixedProductionOrders = (short)(t1.InclQtyPOFixedProductionOrders - t2.InclQtyPOFixedProductionOrders);
            ret.InclQtyProductionDemandPrepared = (short)(t1.InclQtyProductionDemandPrepared - t2.InclQtyProductionDemandPrepared);
            ret.InclQtyProductionDemand = (short)(t1.InclQtyProductionDemand - t2.InclQtyProductionDemand);
            ret.InclQtyProductionAllocated = (short)(t1.InclQtyProductionAllocated - t2.InclQtyProductionAllocated);
            ret.InclQtySOFixedProduction = (short)(t1.InclQtySOFixedProduction - t2.InclQtySOFixedProduction);
		    ret.InclQtyProdFixedPurchase = (short)(t1.InclQtyProdFixedPurchase - t2.InclQtyProdFixedPurchase);
		    ret.InclQtyProdFixedProduction = (short)(t1.InclQtyProdFixedProduction - t2.InclQtyProdFixedProduction);
		    ret.InclQtyProdFixedProdOrdersPrepared = (short)(t1.InclQtyProdFixedProdOrdersPrepared - t2.InclQtyProdFixedProdOrdersPrepared);
		    ret.InclQtyProdFixedProdOrders = (short)(t1.InclQtyProdFixedProdOrders - t2.InclQtyProdFixedProdOrders);
		    ret.InclQtyProdFixedSalesOrdersPrepared = (short)(t1.InclQtyProdFixedSalesOrdersPrepared - t2.InclQtyProdFixedSalesOrdersPrepared);
		    ret.InclQtyProdFixedSalesOrders = (short)(t1.InclQtyProdFixedSalesOrders - t2.InclQtyProdFixedSalesOrders);
            ret.InclQtyINReplaned = (short)(t1.InclQtyINReplaned - t2.InclQtyINReplaned);

            ret.InclQtyFixedFSSrvOrd = (short)(t1.InclQtyFixedFSSrvOrd - t2.InclQtyFixedFSSrvOrd);
            ret.InclQtyPOFixedFSSrvOrd = (short)(t1.InclQtyPOFixedFSSrvOrd - t2.InclQtyPOFixedFSSrvOrd);
            ret.InclQtyPOFixedFSSrvOrdPrepared = (short)(t1.InclQtyPOFixedFSSrvOrdPrepared - t2.InclQtyPOFixedFSSrvOrdPrepared);
            ret.InclQtyPOFixedFSSrvOrdReceipts = (short)(t1.InclQtyPOFixedFSSrvOrdReceipts - t2.InclQtyPOFixedFSSrvOrdReceipts);

            ret.InclQtySOFixed = (short)(t1.InclQtySOFixed - t2.InclQtySOFixed);
			ret.InclQtyPOFixedOrders = (short)(t1.InclQtyPOFixedOrders - t2.InclQtyPOFixedOrders);
			ret.InclQtyPOFixedPrepared = (short)(t1.InclQtyPOFixedPrepared - t2.InclQtyPOFixedPrepared);
			ret.InclQtyPOFixedReceipts = (short)(t1.InclQtyPOFixedReceipts - t2.InclQtyPOFixedReceipts);
			ret.InclQtySODropShip = (short)(t1.InclQtySODropShip - t2.InclQtySODropShip);
			ret.InclQtyPODropShipOrders = (short)(t1.InclQtyPODropShipOrders - t2.InclQtyPODropShipOrders);
			ret.InclQtyPODropShipPrepared = (short)(t1.InclQtyPODropShipPrepared - t2.InclQtyPODropShipPrepared);
			ret.InclQtyPODropShipReceipts = (short)(t1.InclQtyPODropShipReceipts - t2.InclQtyPODropShipReceipts);

            return ret;
		}

		public static INPlanType operator +(INPlanType t1, INPlanType t2)
		{
			return t1 - (-t2);
		}

		/*
		public static INPlanType operator ^(INPlanType t1, INPlanType t2)
		{
			INPlanType ret = new INPlanType();

			ret.InclQtyINIssues = (short)(t1.InclQtyINIssues != 0 && t2.InclQtyINIssues != 0 ? 0 : 1);
			ret.InclQtyINReceipts = (short)(t1.InclQtyINReceipts != 0 && t2.InclQtyINReceipts != 0 ? 0 : 1);
			ret.InclQtyInTransit = (short)(t1.InclQtyInTransit != 0 && t2.InclQtyInTransit != 0 ? 0 : 1);
			ret.InclQtyPOReceipts = (short)(t1.InclQtyPOReceipts != 0 && t2.InclQtyPOReceipts != 0 ? 0 : 1);
			ret.InclQtyPOPrepared = (short)(t1.InclQtyPOPrepared != 0 && t2.InclQtyPOPrepared != 0 ? 0 : 1);
			ret.InclQtyPOOrders = (short)(t1.InclQtyPOOrders != 0 && t2.InclQtyPOOrders != 0 ? 0 : 1);
			ret.InclQtySOBackOrdered = (short)(t1.InclQtySOBackOrdered != 0 && t2.InclQtySOBackOrdered != 0 ? 0 : 1);
			ret.InclQtySOPrepared = (short)(t1.InclQtySOPrepared != 0 && t2.InclQtySOPrepared != 0 ? 0 : 1);
			ret.InclQtySOBooked = (short)(t1.InclQtySOBooked != 0 && t2.InclQtySOBooked != 0 ? 0 : 1);
			ret.InclQtySOShipped = (short)(t1.InclQtySOShipped != 0 && t2.InclQtySOShipped != 0 ? 0 : 1);
			ret.InclQtySOShipping = (short)(t1.InclQtySOShipping != 0 && t2.InclQtySOShipping != 0 ? 0 : 1);
			ret.InclQtyINAssemblySupply = (short)(t1.InclQtyINAssemblySupply != 0 && t2.InclQtyINAssemblySupply != 0 ? 0 : 1);
			ret.InclQtyINAssemblyDemand = (short)(t1.InclQtyINAssemblyDemand != 0 && t2.InclQtyINAssemblyDemand != 0 ? 0 : 1);
			ret.InclQtyINReplaned = (short)(t1.InclQtyINReplaned != 0 && t2.InclQtyINReplaned != 0 ? 0 : 1);
			ret.InclQtySOFixed = (short)(t1.InclQtySOFixed != 0 && t2.InclQtySOFixed != 0 ? 0 : 1);
			ret.InclQtyPOFixedOrders = (short)(t1.InclQtyPOFixedOrders != 0 && t2.InclQtyPOFixedOrders != 0 ? 0 : 1);
			ret.InclQtyPOFixedPrepared = (short)(t1.InclQtyPOFixedPrepared != 0 && t2.InclQtyPOFixedPrepared != 0 ? 0 : 1);
			ret.InclQtyPOFixedReceipts = (short)(t1.InclQtyPOFixedReceipts != 0 && t2.InclQtyPOFixedReceipts != 0 ? 0 : 1);
			ret.InclQtySODropShip = (short)(t1.InclQtySODropShip != 0 && t2.InclQtySODropShip != 0 ? 0 : 1);
			ret.InclQtyPODropShipOrders = (short)(t1.InclQtyPODropShipOrders != 0 && t2.InclQtyPODropShipOrders != 0 ? 0 : 1);
			ret.InclQtyPODropShipPrepared = (short)(t1.InclQtyPODropShipPrepared != 0 && t2.InclQtyPODropShipPrepared != 0 ? 0 : 1);
			ret.InclQtyPODropShipReceipts = (short)(t1.InclQtyPODropShipReceipts != 0 && t2.InclQtyPODropShipReceipts != 0 ? 0 : 1);
			return ret;
		}
		*/

		/*
		public static INPlanType operator *(INPlanType t1, INPlanType t2)
		{
			INPlanType ret = new INPlanType();
			ret.InclQtyINIssues = (short)(t1.InclQtyINIssues * t2.InclQtyINIssues);
			ret.InclQtyINReceipts = (short)(t1.InclQtyINReceipts * t2.InclQtyINReceipts);
			ret.InclQtyInTransit = (short)(t1.InclQtyInTransit * t2.InclQtyInTransit);
			ret.InclQtyPOReceipts = (short)(t1.InclQtyPOReceipts * t2.InclQtyPOReceipts);
			ret.InclQtyPOPrepared = (short)(t1.InclQtyPOPrepared * t2.InclQtyPOPrepared);
			ret.InclQtyPOOrders = (short)(t1.InclQtyPOOrders * t2.InclQtyPOOrders);
			ret.InclQtySOBackOrdered = (short)(t1.InclQtySOBackOrdered * t2.InclQtySOBackOrdered);
			ret.InclQtySOPrepared = (short)(t1.InclQtySOPrepared * t2.InclQtySOPrepared);
			ret.InclQtySOBooked = (short)(t1.InclQtySOBooked * t2.InclQtySOBooked);
			ret.InclQtySOShipped = (short)(t1.InclQtySOShipped * t2.InclQtySOShipped);
			ret.InclQtySOShipping = (short)(t1.InclQtySOShipping * t2.InclQtySOShipping);
			ret.InclQtyINAssemblySupply = (short)(t1.InclQtyINAssemblySupply * t2.InclQtyINAssemblySupply);
			ret.InclQtyINAssemblyDemand = (short)(t1.InclQtyINAssemblyDemand * t2.InclQtyINAssemblyDemand);
			ret.InclQtyINReplaned = (short)(t1.InclQtyINReplaned * t2.InclQtyINReplaned);
			ret.InclQtySOFixed = (short)(t1.InclQtySOFixed * t2.InclQtySOFixed);
			ret.InclQtyPOFixedOrders = (short)(t1.InclQtyPOFixedOrders * t2.InclQtyPOFixedOrders);
			ret.InclQtyPOFixedPrepared = (short)(t1.InclQtyPOFixedPrepared * t2.InclQtyPOFixedPrepared);
			ret.InclQtyPOFixedReceipts = (short)(t1.InclQtyPOFixedReceipts * t2.InclQtyPOFixedReceipts);
			ret.InclQtySODropShip = (short)(t1.InclQtySODropShip * t2.InclQtySODropShip);
			ret.InclQtyPODropShipOrders = (short)(t1.InclQtyPODropShipOrders * t2.InclQtyPODropShipOrders);
			ret.InclQtyPODropShipPrepared = (short)(t1.InclQtyPODropShipPrepared * t2.InclQtyPODropShipPrepared);
			ret.InclQtyPODropShipReceipts = (short)(t1.InclQtyPODropShipReceipts * t2.InclQtyPODropShipReceipts);
			return ret;
		}
		*/

		public static implicit operator INPlanType(int n)
		{
			return new INPlanType
			{
				InclQtyINIssues = (short)n,
				InclQtyINReceipts = (short)n,
				InclQtyInTransit = (short)n,
                InclQtyInTransitToSO = (short)n,
				InclQtyPOReceipts = (short)n,
				InclQtyPOPrepared = (short)n,
				InclQtyPOOrders = (short)n,

                InclQtyFSSrvOrdBooked = (short)n,
                InclQtyFSSrvOrdAllocated = (short)n,
                InclQtyFSSrvOrdPrepared = (short)n,

                InclQtySOBackOrdered = (short)n,
				InclQtySOPrepared = (short)n,
				InclQtySOBooked = (short)n,
				InclQtySOShipped = (short)n,
				InclQtySOShipping = (short)n,
				InclQtyINAssemblySupply = (short)n,
				InclQtyINAssemblyDemand = (short)n,
                InclQtyInTransitToProduction = (short)n,
                InclQtyProductionSupplyPrepared = (short)n,
                InclQtyProductionSupply = (short)n,
                InclQtyPOFixedProductionPrepared = (short)n,
                InclQtyPOFixedProductionOrders = (short)n,
                InclQtyProductionDemandPrepared = (short)n,
                InclQtyProductionDemand = (short)n,
                InclQtyProductionAllocated = (short)n,
                InclQtySOFixedProduction = (short)n,
			    InclQtyProdFixedPurchase = (short)n,
			    InclQtyProdFixedProduction = (short)n,
			    InclQtyProdFixedProdOrdersPrepared = (short)n,
			    InclQtyProdFixedProdOrders = (short)n,
			    InclQtyProdFixedSalesOrdersPrepared = (short)n,
			    InclQtyProdFixedSalesOrders = (short)n,
                InclQtyINReplaned = (short)n,

                InclQtyFixedFSSrvOrd = (short)n,
                InclQtyPOFixedFSSrvOrd = (short)n,
                InclQtyPOFixedFSSrvOrdPrepared = (short)n,
                InclQtyPOFixedFSSrvOrdReceipts = (short)n,

                InclQtySOFixed = (short)n,
				InclQtyPOFixedOrders = (short)n,
				InclQtyPOFixedPrepared = (short)n,
				InclQtyPOFixedReceipts = (short)n,
				InclQtySODropShip = (short)n,
				InclQtyPODropShipOrders = (short)n,
				InclQtyPODropShipPrepared = (short)n,
				InclQtyPODropShipReceipts = (short)n
			};
		}

		public static implicit operator int(INPlanType t)
		{
			return 
				t.InclQtyINIssues > 0 ||
				t.InclQtyINReceipts > 0 ||
				t.InclQtyInTransit > 0 ||
                t.InclQtyInTransitToSO > 0 ||
				t.InclQtyPOReceipts > 0 ||
				t.InclQtyPOPrepared > 0 ||
				t.InclQtyPOOrders > 0 ||
				t.InclQtySOBackOrdered > 0 ||
				t.InclQtySOPrepared > 0 ||
				t.InclQtySOBooked > 0 ||
				t.InclQtySOShipped > 0 ||
				t.InclQtySOShipping > 0 ||
				t.InclQtyINAssemblySupply > 0 ||
				t.InclQtyINAssemblyDemand > 0 ||
				t.InclQtyINReplaned > 0 ||

				t.InclQtyFixedFSSrvOrd > 0 ||
				t.InclQtyPOFixedFSSrvOrd > 0 ||
				t.InclQtyPOFixedFSSrvOrdPrepared > 0 ||
				t.InclQtyPOFixedFSSrvOrdReceipts > 0 ||

				t.InclQtySOFixed > 0 ||
				t.InclQtyPOFixedOrders > 0 ||
				t.InclQtyPOFixedPrepared > 0 ||
				t.InclQtyPOFixedReceipts > 0 ||
				t.InclQtySODropShip > 0 ||
				t.InclQtyPODropShipOrders > 0 ||
				t.InclQtyPODropShipPrepared > 0 ||
				t.InclQtyPODropShipReceipts > 0 ? 1 : 
				t.InclQtyINIssues < 0 ||
				t.InclQtyINReceipts < 0 ||
				t.InclQtyInTransit < 0 ||
                t.InclQtyInTransitToSO < 0 ||
                t.InclQtyPOReceipts < 0 ||
				t.InclQtyPOPrepared < 0 ||
				t.InclQtyPOOrders < 0 ||

                t.InclQtyFSSrvOrdBooked < 0 ||
                t.InclQtyFSSrvOrdAllocated < 0 ||
                t.InclQtyFSSrvOrdPrepared < 0 ||

                t.InclQtySOBackOrdered < 0 ||
				t.InclQtySOPrepared < 0 ||
				t.InclQtySOBooked < 0 ||
				t.InclQtySOShipped < 0 ||
				t.InclQtySOShipping < 0 ||
				t.InclQtyINAssemblySupply < 0 ||
				t.InclQtyINAssemblyDemand < 0 ||
                t.InclQtyInTransitToProduction < 0 ||
                t.InclQtyProductionSupplyPrepared < 0 ||
                t.InclQtyProductionSupply < 0 ||
                t.InclQtyPOFixedProductionPrepared < 0 ||
                t.InclQtyPOFixedProductionOrders < 0 ||
                t.InclQtyProductionDemandPrepared < 0 ||
                t.InclQtyProductionDemand < 0 ||
                t.InclQtyProductionAllocated < 0 ||
                t.InclQtySOFixedProduction < 0 ||
		        t.InclQtyProdFixedPurchase < 0 ||
                t.InclQtyProdFixedProduction < 0 ||
                t.InclQtyProdFixedProdOrdersPrepared < 0 ||
                t.InclQtyProdFixedProdOrders < 0 ||
                t.InclQtyProdFixedSalesOrdersPrepared < 0 ||
                t.InclQtyProdFixedSalesOrders < 0 ||
                t.InclQtyINReplaned < 0 ||

                t.InclQtyFixedFSSrvOrd < 0 ||
                t.InclQtyPOFixedFSSrvOrd < 0 ||
                t.InclQtyPOFixedFSSrvOrdPrepared < 0 ||
                t.InclQtyPOFixedFSSrvOrdReceipts < 0 ||

                t.InclQtySOFixed < 0 ||
				t.InclQtyPOFixedOrders < 0 ||
				t.InclQtyPOFixedPrepared < 0 ||
				t.InclQtyPOFixedReceipts < 0 ||
				t.InclQtySODropShip < 0 ||
				t.InclQtyPODropShipOrders < 0 ||
				t.InclQtyPODropShipPrepared < 0 ||
				t.InclQtyPODropShipReceipts < 0 ? -1 : 0;
		}

		public static bool operator ==(INPlanType t1, INPlanType t2)
		{
			if (Object.Equals(t1,null) || Object.Equals(t2,null))
			{
				return (Object.Equals(t1, null) && Object.Equals(t2,null));
			}
			else
			{
				return
				(t1.InclQtyINIssues == t2.InclQtyINIssues) &&
				(t1.InclQtyINReceipts == t2.InclQtyINReceipts) &&
				(t1.InclQtyInTransit == t2.InclQtyInTransit) &&
                (t1.InclQtyInTransitToSO == t2.InclQtyInTransitToSO) &&
                (t1.InclQtyPOReceipts == t2.InclQtyPOReceipts) &&
				(t1.InclQtyPOPrepared == t2.InclQtyPOPrepared) &&
				(t1.InclQtyPOOrders == t2.InclQtyPOOrders) &&

                (t1.InclQtyFSSrvOrdBooked == t2.InclQtyFSSrvOrdBooked) &&
                (t1.InclQtyFSSrvOrdAllocated == t2.InclQtyFSSrvOrdAllocated) &&
                (t1.InclQtyFSSrvOrdPrepared == t2.InclQtyFSSrvOrdPrepared) &&

                (t1.InclQtySOBackOrdered == t2.InclQtySOBackOrdered) &&
				(t1.InclQtySOPrepared == t2.InclQtySOPrepared) &&
				(t1.InclQtySOBooked == t2.InclQtySOBooked) &&
				(t1.InclQtySOShipped == t2.InclQtySOShipped) &&
				(t1.InclQtySOShipping == t2.InclQtySOShipping) &&
				(t1.InclQtyINAssemblySupply == t2.InclQtyINAssemblySupply) &&
				(t1.InclQtyINAssemblyDemand == t2.InclQtyINAssemblyDemand) &&
                (t1.InclQtyInTransitToProduction == t2.InclQtyInTransitToProduction) &&
                (t1.InclQtyProductionSupplyPrepared == t2.InclQtyProductionSupplyPrepared) &&
                (t1.InclQtyProductionSupply == t2.InclQtyProductionSupply) &&
                (t1.InclQtyPOFixedProductionPrepared == t2.InclQtyPOFixedProductionPrepared) &&
                (t1.InclQtyPOFixedProductionOrders == t2.InclQtyPOFixedProductionOrders) &&
                (t1.InclQtyProductionDemandPrepared == t2.InclQtyProductionDemandPrepared) &&
                (t1.InclQtyProductionDemand == t2.InclQtyProductionDemand) &&
                (t1.InclQtyProductionAllocated == t2.InclQtyProductionAllocated) &&
                (t1.InclQtySOFixedProduction == t2.InclQtySOFixedProduction) &&

                (t1.InclQtyFixedFSSrvOrd == t2.InclQtyFixedFSSrvOrd) &&
                (t1.InclQtyPOFixedFSSrvOrd == t2.InclQtyPOFixedFSSrvOrd) &&
                (t1.InclQtyPOFixedFSSrvOrdPrepared == t2.InclQtyPOFixedFSSrvOrdPrepared) &&
                (t1.InclQtyPOFixedFSSrvOrdReceipts == t2.InclQtyPOFixedFSSrvOrdReceipts) &&

				(t1.InclQtyProdFixedPurchase == t2.InclQtyProdFixedPurchase) &&
				(t1.InclQtyProdFixedProduction == t2.InclQtyProdFixedProduction) &&
				(t1.InclQtyProdFixedProdOrdersPrepared == t2.InclQtyProdFixedProdOrdersPrepared) &&
				(t1.InclQtyProdFixedProdOrders == t2.InclQtyProdFixedProdOrders) &&
				(t1.InclQtyProdFixedSalesOrdersPrepared == t2.InclQtyProdFixedSalesOrdersPrepared) &&
				(t1.InclQtyProdFixedSalesOrders == t2.InclQtyProdFixedSalesOrders) &&
                (t1.InclQtySOFixed == t2.InclQtySOFixed) &&
				(t1.InclQtyPOFixedOrders == t2.InclQtyPOFixedOrders) &&
				(t1.InclQtyPOFixedPrepared == t2.InclQtyPOFixedPrepared) &&
				(t1.InclQtyPOFixedReceipts == t2.InclQtyPOFixedReceipts) &&
				(t1.InclQtySODropShip == t2.InclQtySODropShip) &&
				(t1.InclQtyPODropShipOrders == t2.InclQtyPODropShipOrders) &&
				(t1.InclQtyPODropShipPrepared == t2.InclQtyPODropShipPrepared) &&
				(t1.InclQtyPODropShipReceipts == t2.InclQtyPODropShipReceipts);
			}	
		}

		public static bool operator !=(INPlanType t1, INPlanType t2)
		{
			return !(t1 == t2);
		}

		public override bool Equals(object obj)
		{
			return this == (INPlanType)obj;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
	}

	public class INPlanConstants
	{
		public const string Plan10 = "10";
		public const string Plan20 = "20";
		public const string Plan40 = "40";
		public const string Plan41 = "41";
		public const string Plan42 = "42";
		public const string Plan43 = "43";
        public const string Plan44 = "44";
        public const string Plan45 = "45";
		public const string Plan50 = "50";
		public const string Plan51 = "51";
		public const string Plan60 = "60";
		public const string Plan61 = "61";
		public const string Plan62 = "62";
		public const string Plan63 = "63";
		public const string Plan64 = "64";
		public const string Plan65 = "65";
		public const string Plan66 = "66";
		public const string Plan68 = "68";
		public const string Plan69 = "69";
		public const string Plan6B = "6B";
		public const string Plan6T = "6T";
		public const string Plan6D = "6D";
		public const string Plan6E = "6E";
		public const string Plan70 = "70";
		public const string Plan71 = "71";
		public const string Plan72 = "72";
		public const string Plan73 = "73";
		public const string Plan74 = "74";
		public const string Plan75 = "75";
		public const string Plan76 = "76";
		public const string Plan77 = "77";
		public const string Plan78 = "78";
		public const string Plan79 = "79";
		public const string Plan7B = "7B";
		public const string Plan90 = "90";
        public const string Plan93 = "93";
		public const string Plan94 = "94";
		public const string Plan95 = "95";
        public const string Plan96 = "96";
        public const string PlanM0 = "M0";
        public const string PlanM1 = "M1";
        public const string PlanM2 = "M2";
        public const string PlanM3 = "M3";
        public const string PlanM4 = "M4";
        public const string PlanM5 = "M5";
        public const string PlanM6 = "M6";
        public const string PlanM7 = "M7";
        public const string PlanM8 = "M8";
	    public const string PlanM9 = "M9";
	    public const string PlanMA = "MA";
	    public const string PlanMB = "MB";
	    public const string PlanMC = "MC";
	    public const string PlanMD = "MD";
	    public const string PlanME = "ME";
        public const string PlanF0 = "F0";
        public const string PlanF1 = "F1";
        public const string PlanF2 = "F2";
        public const string PlanF5 = "F5";
        public const string PlanF6 = "F6";
        public const string PlanF7 = "F7";
        public const string PlanF8 = "F8";
        public const string PlanF9 = "F9";


        public class plan10 : PX.Data.BQL.BqlString.Constant<plan10>
		{
			public plan10() : base(Plan10) { ;}
		}

		public class plan20 : PX.Data.BQL.BqlString.Constant<plan20>
		{
			public plan20() : base(Plan20) { ;}
		}
		
		public class plan40 : PX.Data.BQL.BqlString.Constant<plan40>
		{
			public plan40() : base(Plan40) { ;}
		}

		public class plan41 : PX.Data.BQL.BqlString.Constant<plan41>
		{
			public plan41() : base(Plan41) { ;}
		}

		public class plan42 : PX.Data.BQL.BqlString.Constant<plan42>
		{
			public plan42() : base(Plan42) { ;}
		}

		public class plan43 : PX.Data.BQL.BqlString.Constant<plan43>
		{
			public plan43() : base(Plan43) { ;}
		}

        public class plan44 : PX.Data.BQL.BqlString.Constant<plan44>
		{
            public plan44() : base(Plan44) { ;}
        }

        public class plan45 : PX.Data.BQL.BqlString.Constant<plan45>
		{
            public plan45() : base(Plan45) { ;}
        }

		public class plan50 : PX.Data.BQL.BqlString.Constant<plan50>
		{
			public plan50() : base(Plan50) { ;}
		}

		public class plan51 : PX.Data.BQL.BqlString.Constant<plan51>
		{
			public plan51() : base(Plan51) { ;}
		}

		public class plan60 : PX.Data.BQL.BqlString.Constant<plan60>
		{
			public plan60() : base(Plan60) { ;}
		}

		public class plan61 : PX.Data.BQL.BqlString.Constant<plan61>
		{
			public plan61() : base(Plan61) { ;}
		}

		public class plan62 : PX.Data.BQL.BqlString.Constant<plan62>
		{
			public plan62() : base(Plan62) { ;}
		}

		public class plan63 : PX.Data.BQL.BqlString.Constant<plan63>
		{
			public plan63() : base(Plan63) { ;}
		}

		public class plan64 : PX.Data.BQL.BqlString.Constant<plan64>
		{
			public plan64() : base(Plan64) { ;}
		}

		/*
		public class plan65 : PX.Data.BQL.BqlString.Constant<plan65>
		{
			public plan65() : base(Plan65) { ;}
		}
		*/

		public class plan66 : PX.Data.BQL.BqlString.Constant<plan66>
		{
			public plan66() : base(Plan66) { ;}
		}

		public class plan68 : PX.Data.BQL.BqlString.Constant<plan68>
		{
			public plan68() : base(Plan68) { ;}
		}
		public class plan69 : PX.Data.BQL.BqlString.Constant<plan69>
		{
			public plan69() : base(Plan69) {; }
		}
		public class plan6B : PX.Data.BQL.BqlString.Constant<plan6B>
		{
			public plan6B() : base(Plan6B) { ;}
		}
		public class plan6D : PX.Data.BQL.BqlString.Constant<plan6D>
		{
			public plan6D() : base(Plan6D) { ;}
		}
		public class plan6E : PX.Data.BQL.BqlString.Constant<plan6E>
		{
			public plan6E() : base(Plan6E) { ;}
		}
		/*
		public class plan6T : PX.Data.BQL.BqlString.Constant<plan6T>
		{
			public plan6T() : base(Plan6T) { ;}
		}
        */
		public class plan70 : PX.Data.BQL.BqlString.Constant<plan70>
		{
			public plan70() : base(Plan70) { ;}
		}

		public class plan71 : PX.Data.BQL.BqlString.Constant<plan71>
		{
			public plan71() : base(Plan71) { ;}
		}

		public class plan72 : PX.Data.BQL.BqlString.Constant<plan72>
		{
			public plan72() : base(Plan72) { ;}
		}
		public class plan73 : PX.Data.BQL.BqlString.Constant<plan73>
		{
			public plan73() : base(Plan73) { ;}
		}
		public class plan74 : PX.Data.BQL.BqlString.Constant<plan74>
		{
			public plan74() : base(Plan74) { ;}
		}
		public class plan75 : PX.Data.BQL.BqlString.Constant<plan75>
		{
			public plan75() : base(Plan75) { ;}
		}
		public class plan76 : PX.Data.BQL.BqlString.Constant<plan76>
		{
			public plan76() : base(Plan76) { ;}
		}
		public class plan77 : PX.Data.BQL.BqlString.Constant<plan77>
		{
			public plan77() : base(Plan77) { ;}
		}
		public class plan78 : PX.Data.BQL.BqlString.Constant<plan78>
		{
			public plan78() : base(Plan78) { ;}
		}
		public class plan79 : PX.Data.BQL.BqlString.Constant<plan79>
		{
			public plan79() : base(Plan79) { ;}
		}
		public class plan7B : PX.Data.BQL.BqlString.Constant<plan7B>
		{
			public plan7B() : base(Plan7B) { ;}
		}		
		public class plan90 : PX.Data.BQL.BqlString.Constant<plan90>
		{
			public plan90() : base(Plan90) { ;}
		}

        public class plan93 : PX.Data.BQL.BqlString.Constant<plan93>
		{
            public plan93() : base(Plan93) { ;}
        }
		
		public class plan94 : PX.Data.BQL.BqlString.Constant<plan94>
		{
			public plan94() : base(Plan94) { ;}
		}

		public class plan95 : PX.Data.BQL.BqlString.Constant<plan95>
		{
			public plan95() : base(Plan95) { ;}
		}

        public class plan96 : PX.Data.BQL.BqlString.Constant<plan96>
		{
            public plan96() : base(Plan96) { ;}
        }

        public class planM0 : PX.Data.BQL.BqlString.Constant<planM0>
		{
            public planM0() : base(PlanM0) {; }
        }

        public class planM1 : PX.Data.BQL.BqlString.Constant<planM1>
		{
            public planM1() : base(PlanM1) {; }
        }

        public class planM2 : PX.Data.BQL.BqlString.Constant<planM2>
		{
            public planM2() : base(PlanM2) {; }
        }

        public class planM3 : PX.Data.BQL.BqlString.Constant<planM3>
		{
            public planM3() : base(PlanM3) {; }
        }

        public class planM4 : PX.Data.BQL.BqlString.Constant<planM4>
		{
            public planM4() : base(PlanM4) {; }
        }

        public class planM5 : PX.Data.BQL.BqlString.Constant<planM5>
		{
            public planM5() : base(PlanM5) {; }
        }

        public class planM6 : PX.Data.BQL.BqlString.Constant<planM6>
		{
            public planM6() : base(PlanM6) {; }
        }

        public class planM7 : PX.Data.BQL.BqlString.Constant<planM7>
		{
            public planM7() : base(PlanM7) {; }
        }

        public class planM8 : PX.Data.BQL.BqlString.Constant<planM8>
		{
            public planM8() : base(PlanM8) {; }
        }

	    public class planM9 : PX.Data.BQL.BqlString.Constant<planM9>
		{
	        public planM9() : base(PlanM9) { }
	    }

	    public class planMA : PX.Data.BQL.BqlString.Constant<planMA>
		{
	        public planMA() : base(PlanMA) { }
	    }

	    public class planMB : PX.Data.BQL.BqlString.Constant<planMB>
		{
	        public planMB() : base(PlanMB) { }
	    }

	    public class planMC : PX.Data.BQL.BqlString.Constant<planMC>
		{
	        public planMC() : base(PlanMC) { }
	    }

	    public class planME : PX.Data.BQL.BqlString.Constant<planME>
		{
	        public planME() : base(PlanME) { }
	    }

	    public class planMD : PX.Data.BQL.BqlString.Constant<planMD>
		{
	        public planMD() : base(PlanMD) { }
	    }
        public class planF0 : PX.Data.BQL.BqlString.Constant<planF0>
		{
            public planF0() : base(PlanF0) {; }
        }

        public class planF1 : PX.Data.BQL.BqlString.Constant<planF1>
		{
            public planF1() : base(PlanF1) {; }
        }

        public class planF2 : PX.Data.BQL.BqlString.Constant<planF2>
		{
            public planF2() : base(PlanF2) {; }
        }

        public class planF5 : PX.Data.BQL.BqlString.Constant<planF5>
		{
            public planF5() : base(PlanF5) {; }
        }

        public class planF6 : PX.Data.BQL.BqlString.Constant<planF6>
		{
            public planF6() : base(PlanF6) {; }
        }
        public class planF7 : PX.Data.BQL.BqlString.Constant<planF7>
		{
            public planF7() : base(PlanF7) {; }
        }
        public class planF8 : PX.Data.BQL.BqlString.Constant<planF8>
		{
            public planF8() : base(PlanF8) {; }
        }
        public class planF9 : PX.Data.BQL.BqlString.Constant<planF9>
		{
            public planF9() : base(PlanF9) {; }
        }


        public static bool IsFixed(string PlanType)
		{
			return
				PlanType == Plan64 ||
				PlanType == Plan66 ||
                PlanType == Plan6B ||
                PlanType == Plan6E ||
				PlanType == Plan6D ||
				PlanType == Plan93 ||
                PlanType == PlanF5 ||
                PlanType == PlanF6 ||
                PlanType == PlanM8;
        }

        public static bool IsAllocated(string PlanType)
        {
            return 
                PlanType == Plan61 || 
                PlanType == Plan63 ||
                PlanType == PlanM7 ||
                PlanType == PlanF2;
        }

		public static string ToModuleField(string planType)
		{
			switch (planType)
			{
				case Plan10:
				case Plan43:
				case Plan20:
				case Plan40:
				case Plan41:
				case Plan42:
				case Plan94:
					return GL.BatchModule.IN;
				case Plan44:
				case Plan93:
					return GL.BatchModule.SO;
				case Plan70:
				case Plan73:
				case Plan71:
				case Plan72:
					return GL.BatchModule.PO;
				case Plan69:
				case Plan60:
				case Plan61:
				case Plan63:
				case Plan62:
				case Plan68:
					return GL.BatchModule.SO;
				case Plan50:
				case Plan51:
					return GL.BatchModule.IN;
				case Plan66:
				case Plan6B:
					return GL.BatchModule.SO;
				case Plan76:
				case Plan78:
				case Plan45:
				case Plan77:
					return GL.BatchModule.PO;
				case Plan6D:
				case Plan6E:
				case Plan6T:
					return GL.BatchModule.SO;
				case Plan74:
				case Plan79:
				case Plan75:
					return GL.BatchModule.PO;
				case Plan90:
					return GL.BatchModule.IN;
				case PlanM0:
				case PlanM1:
				case PlanM2:
				case PlanM3:
				case PlanM4:
				case PlanM5:
				case PlanM6:
				case PlanM7:
				case PlanM8:
				case PlanM9:
				case PlanMA:
				case PlanMB:
				case PlanMC:
				case PlanMD:
				case PlanME:
					return "AM";

				case PlanF0:
				case PlanF1:
				case PlanF2:
				case PlanF6:
					return "FS";

				case PlanF7:
				case PlanF8:
				case PlanF9:
					return GL.BatchModule.PO;

				default:
					return null;
			}
		}


		public static Type ToInclQtyField(string planType)
	    {
            switch (planType)
            {
                case Plan10:
                case Plan43:
                    return typeof(INPlanType.inclQtyINReceipts);
                case Plan20:
                case Plan40:
                case Plan41:
                    return typeof(INPlanType.inclQtyINIssues);
                case Plan42:
                case Plan94:
                    return typeof(INPlanType.inclQtyInTransit);
                case Plan44:
                case Plan93:
                    return typeof(INPlanType.inclQtyInTransitToSO);
                case Plan70:
                    return typeof(INPlanType.inclQtyPOOrders);
                case Plan73:
                    return typeof(INPlanType.inclQtyPOPrepared);
                case Plan71:
                case Plan72:
                    return typeof(INPlanType.inclQtyPOReceipts);
                case Plan69:
                    return typeof(INPlanType.inclQtySOPrepared);
                case Plan60:
                    return typeof(INPlanType.inclQtySOBooked);
                case Plan61:
                case Plan63:
                    return typeof(INPlanType.inclQtySOShipping);
                case Plan62:
                    return typeof(INPlanType.inclQtySOShipped);
                case Plan68:
                    return typeof(INPlanType.inclQtySOBackOrdered);
                case Plan50:
                    return typeof(INPlanType.inclQtyINAssemblyDemand);
                case Plan51:
                    return typeof(INPlanType.inclQtyINAssemblySupply);
                case Plan66:
                case Plan6B:
                    return typeof(INPlanType.inclQtySOFixed);
                case Plan76:
                    return typeof(INPlanType.inclQtyPOFixedOrders);
                case Plan78:
                    return typeof(INPlanType.inclQtyPOFixedPrepared);
                case Plan45:
                case Plan77:
                    return typeof(INPlanType.inclQtyPOFixedReceipts);
                case Plan6D:
                case Plan6E:
                case Plan6T:
                    return typeof(INPlanType.inclQtySODropShip);
                case Plan74:
                    return typeof(INPlanType.inclQtyPODropShipOrders);
                case Plan79:
                    return typeof(INPlanType.inclQtyPODropShipPrepared);
                case Plan75:
                    return typeof(INPlanType.inclQtyPODropShipReceipts);
                case Plan90:
                    return typeof(INPlanType.inclQtyINReplaned);
                case PlanM0:
                    return typeof(INPlanType.inclQtyInTransitToProduction);
                case PlanM1:
                    return typeof(INPlanType.inclQtyProductionSupplyPrepared);
                case PlanM2:
                    return typeof(INPlanType.inclQtyProductionSupply);
                case PlanM3:
                    return typeof(INPlanType.inclQtyPOFixedProductionPrepared);
                case PlanM4:
                    return typeof(INPlanType.inclQtyPOFixedProductionOrders);
                case PlanM5:
                    return typeof(INPlanType.inclQtyProductionDemandPrepared);
                case PlanM6:
                    return typeof(INPlanType.inclQtyProductionDemand);
                case PlanM7:
                    return typeof(INPlanType.inclQtyProductionAllocated);
                case PlanM8:
                    return typeof(INPlanType.inclQtySOFixedProduction);
                case PlanM9:
                    return typeof(INPlanType.inclQtyProdFixedPurchase);
                case PlanMA:
                    return typeof(INPlanType.inclQtyProdFixedProduction);
                case PlanMB:
                    return typeof(INPlanType.inclQtyProdFixedProdOrdersPrepared);
                case PlanMC:
                    return typeof(INPlanType.inclQtyProdFixedProdOrders);
                case PlanMD:
                    return typeof(INPlanType.inclQtyProdFixedSalesOrdersPrepared);
                case PlanME:
                    return typeof(INPlanType.inclQtyProdFixedSalesOrders);

                case PlanF0:
                    return typeof(INPlanType.inclQtyFSSrvOrdPrepared);
                case PlanF1:
                    return typeof(INPlanType.inclQtyFSSrvOrdBooked);
                case PlanF2:
                    return typeof(INPlanType.inclQtyFSSrvOrdAllocated);
                case PlanF6:
                    return typeof(INPlanType.inclQtyFixedFSSrvOrd);
                case PlanF7:
                    return typeof(INPlanType.inclQtyPOFixedFSSrvOrd);
                case PlanF8:
                    return typeof(INPlanType.inclQtyPOFixedFSSrvOrdPrepared);
                case PlanF9:
                    return typeof(INPlanType.inclQtyPOFixedFSSrvOrdReceipts);

                default:
                    return null;
            }
        }
    }	
}