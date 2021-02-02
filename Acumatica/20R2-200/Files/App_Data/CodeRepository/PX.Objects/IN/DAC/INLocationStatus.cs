using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INLocationStatus)]
	public partial class INLocationStatus : PX.Data.IBqlTable, IStatus
	{
		#region Keys
		public class PK : PrimaryKeyOf<INLocationStatus>.By<inventoryID, subItemID, siteID, locationID>
		{
			public static INLocationStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID)
				=> FindBy(graph, inventoryID, subItemID, siteID, locationID);
		}
		public static class FK
		{
			public class Location : INLocation.PK.ForeignKeyOf<INLocationStatus>.By<locationID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INLocationStatus>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INLocationStatus>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INLocationStatus>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INLocationStatus>.By<inventoryID, siteID> { }
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected Boolean? _Selected = false;
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual Boolean? Selected
		{
			get
			{
				return this._Selected;
			}
			set
			{
				this._Selected = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey=true)]
		[PXDefault()]
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
		[SubItem(IsKey=true)]
		[PXDefault()]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(IsKey=true)]
		[PXDefault()]
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(IsKey=true)]
		[PXDefault()]
		[PXForeignReference(typeof(FK.Location))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected bool? _Active = false;
		[PXExistance()]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion
		#region QtyNotAvail
		public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		protected Decimal? _QtyNotAvail;
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? QtyNotAvail
		{
			get
			{
				return this._QtyNotAvail;
			}
			set
			{
				this._QtyNotAvail = value;
			}
		}
		#endregion
		#region QtyExpired
		public abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
		protected Decimal? _QtyExpired;
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? QtyExpired
		{
			get
			{
				return this._QtyExpired;
			}
			set
			{
				this._QtyExpired = value;
			}
		}
		#endregion
		#region QtyHardAvail
		public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		protected Decimal? _QtyHardAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Hard Available")]
		public virtual Decimal? QtyHardAvail
		{
			get
			{
				return this._QtyHardAvail;
			}
			set
			{
				this._QtyHardAvail = value;
			}
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : PX.Data.BQL.BqlDecimal.Field<qtyActual> { }
		protected decimal? _QtyActual;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available for Issue")]
		public virtual decimal? QtyActual
		{
			get { return _QtyActual; }
			set { _QtyActual = value; }
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		protected Decimal? _QtyInTransit;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Qty. In-Transit")]
		public virtual Decimal? QtyInTransit
		{
			get
			{
				return this._QtyInTransit;
			}
			set
			{
				this._QtyInTransit = value;
			}
		}
		#endregion
        #region QtyInTransitToSO
        public abstract class qtyInTransitToSO : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
        protected Decimal? _QtyInTransitToSO;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? QtyInTransitToSO
        {
            get
            {
                return this._QtyInTransitToSO;
            }
            set
            {
                this._QtyInTransitToSO = value;
            }
        }
        #endregion
		#region QtyINReplaned
		public decimal? QtyINReplaned
		{
			get { return 0m; }
			set { }
		}
		#endregion
		#region QtyPOPrepared
		public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		protected Decimal? _QtyPOPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyPOPrepared
		{
			get
			{
				return this._QtyPOPrepared;
			}
			set
			{
				this._QtyPOPrepared = value;
			}
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		protected Decimal? _QtyPOOrders;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Qty. Purchase Orders")]
		public virtual Decimal? QtyPOOrders
		{
			get
			{
				return this._QtyPOOrders;
			}
			set
			{
				this._QtyPOOrders = value;
			}
		}
		#endregion
		#region QtyPOReceipts
		public abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		protected Decimal? _QtyPOReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Qty. Purchase Receipts")]
		public virtual Decimal? QtyPOReceipts
		{
			get
			{
				return this._QtyPOReceipts;
			}
			set
			{
				this._QtyPOReceipts = value;
			}
		}
		#endregion

        #region QtyFSSrvOrdBooked
        public abstract class qtyFSSrvOrdBooked : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
        protected Decimal? _QtyFSSrvOrdBooked;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. FS Booked", FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdBooked
        {
            get
            {
                return this._QtyFSSrvOrdBooked;
            }
            set
            {
                this._QtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region QtyFSSrvOrdAllocated
        public abstract class qtyFSSrvOrdAllocated : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
        protected Decimal? _QtyFSSrvOrdAllocated;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. FS Allocated", FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdAllocated
        {
            get
            {
                return this._QtyFSSrvOrdAllocated;
            }
            set
            {
                this._QtyFSSrvOrdAllocated = value;
            }
        }
        #endregion
        #region QtyFSSrvOrdPrepared
        public abstract class qtyFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
        protected Decimal? _QtyFSSrvOrdPrepared;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. FS Prepared", FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdPrepared
        {
            get
            {
                return this._QtyFSSrvOrdPrepared;
            }
            set
            {
                this._QtyFSSrvOrdPrepared = value;
            }
        }
        #endregion

		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		protected Decimal? _QtySOBackOrdered;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtySOBackOrdered
		{
			get
			{
				return this._QtySOBackOrdered;
			}
			set
			{
				this._QtySOBackOrdered = value;
			}
		}
		#endregion
		#region QtySOPrepared
		public abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		protected Decimal? _QtySOPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtySOPrepared
		{
			get
			{
				return this._QtySOPrepared;
			}
			set
			{
				this._QtySOPrepared = value;
			}
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : PX.Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		protected Decimal? _QtySOBooked;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Qty. SO Booked")]
		public virtual Decimal? QtySOBooked
		{
			get
			{
				return this._QtySOBooked;
			}
			set
			{
				this._QtySOBooked = value;
			}
		}
		#endregion
		#region QtySOShipped
		public abstract class qtySOShipped : PX.Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		protected Decimal? _QtySOShipped;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Qty. SO Shipped")]
		public virtual Decimal? QtySOShipped
		{
			get
			{
				return this._QtySOShipped;
			}
			set
			{
				this._QtySOShipped = value;
			}
		}
		#endregion
		#region QtySOShipping
		public abstract class qtySOShipping : PX.Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		protected Decimal? _QtySOShipping;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Qty. SO Shipping")]
		public virtual Decimal? QtySOShipping
		{
			get
			{
				return this._QtySOShipping;
			}
			set
			{
				this._QtySOShipping = value;
			}
		}
		#endregion
		#region QtyINIssues
		public abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		protected Decimal? _QtyINIssues;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Inventory Issues")]
		public virtual Decimal? QtyINIssues
		{
			get
			{
				return this._QtyINIssues;
			}
			set
			{
				this._QtyINIssues = value;
			}
		}
		#endregion
		#region QtyINReceipts
		public abstract class qtyINReceipts : PX.Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		protected Decimal? _QtyINReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Inventory Receipts")]
		public virtual Decimal? QtyINReceipts
		{
			get
			{
				return this._QtyINReceipts;
			}
			set
			{
				this._QtyINReceipts = value;
			}
		}
		#endregion
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		protected Decimal? _QtyINAssemblyDemand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty Demanded by Kit Assembly")]
		public virtual Decimal? QtyINAssemblyDemand
		{
			get
			{
				return this._QtyINAssemblyDemand;
			}
			set
			{
				this._QtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		protected Decimal? _QtyINAssemblySupply;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Kit Assembly")]
		public virtual Decimal? QtyINAssemblySupply
		{
			get
			{
				return this._QtyINAssemblySupply;
			}
			set
			{
				this._QtyINAssemblySupply = value;
			}
		}
        #endregion
        #region QtyInTransitToProduction
        public abstract class qtyInTransitToProduction : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToProduction> { }
        protected Decimal? _QtyInTransitToProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity In Transit to Production.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty In Transit to Production")]
        public virtual Decimal? QtyInTransitToProduction
        {
            get
            {
                return this._QtyInTransitToProduction;
            }
            set
            {
                this._QtyInTransitToProduction = value;
            }
        }
        #endregion
        #region QtyProductionSupplyPrepared
        public abstract class qtyProductionSupplyPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupplyPrepared> { }
        protected Decimal? _QtyProductionSupplyPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity Production Supply Prepared.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Production Supply Prepared")]
        public virtual Decimal? QtyProductionSupplyPrepared
        {
            get
            {
                return this._QtyProductionSupplyPrepared;
            }
            set
            {
                this._QtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region QtyProductionSupply
        public abstract class qtyProductionSupply : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupply> { }
        protected Decimal? _QtyProductionSupply;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity Production Supply.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Supply")]
        public virtual Decimal? QtyProductionSupply
        {
            get
            {
                return this._QtyProductionSupply;
            }
            set
            {
                this._QtyProductionSupply = value;
            }
        }
        #endregion
        #region QtyPOFixedProductionPrepared
        public abstract class qtyPOFixedProductionPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
        protected Decimal? _QtyPOFixedProductionPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Purchase for Prod. Prepared.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Purchase for Prod. Prepared")]
        public virtual Decimal? QtyPOFixedProductionPrepared
        {
            get
            {
                return this._QtyPOFixedProductionPrepared;
            }
            set
            {
                this._QtyPOFixedProductionPrepared = value;
            }
        }
        #endregion
        #region QtyPOFixedProductionOrders
        public abstract class qtyPOFixedProductionOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionOrders> { }
        protected Decimal? _QtyPOFixedProductionOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Purchase for Production.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Purchase for Production")]
        public virtual Decimal? QtyPOFixedProductionOrders
        {
            get
            {
                return this._QtyPOFixedProductionOrders;
            }
            set
            {
                this._QtyPOFixedProductionOrders = value;
            }
        }
        #endregion
        #region QtyProductionDemandPrepared
        public abstract class qtyProductionDemandPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemandPrepared> { }
        protected Decimal? _QtyProductionDemandPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production Demand Prepared.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Demand Prepared")]
        public virtual Decimal? QtyProductionDemandPrepared
        {
            get
            {
                return this._QtyProductionDemandPrepared;
            }
            set
            {
                this._QtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region QtyProductionDemand
        public abstract class qtyProductionDemand : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemand> { }
        protected Decimal? _QtyProductionDemand;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production Demand.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Demand")]
        public virtual Decimal? QtyProductionDemand
        {
            get
            {
                return this._QtyProductionDemand;
            }
            set
            {
                this._QtyProductionDemand = value;
            }
        }
        #endregion
        #region QtyProductionAllocated
        public abstract class qtyProductionAllocated : PX.Data.BQL.BqlDecimal.Field<qtyProductionAllocated> { }
        protected Decimal? _QtyProductionAllocated;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production Allocated.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Allocated")]
        public virtual Decimal? QtyProductionAllocated
        {
            get
            {
                return this._QtyProductionAllocated;
            }
            set
            {
                this._QtyProductionAllocated = value;
            }
        }
        #endregion
        #region QtySOFixedProduction
        public abstract class qtySOFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtySOFixedProduction> { }
        protected Decimal? _QtySOFixedProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On SO to Production.  
        /// </summary>
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On SO to Production")]
        public virtual Decimal? QtySOFixedProduction
        {
            get
            {
                return this._QtySOFixedProduction;
            }
            set
            {
                this._QtySOFixedProduction = value;
            }
        }
        #endregion

        #region QtyFixedFSSrvOrd
        public abstract class qtyFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
        protected decimal? _QtyFixedFSSrvOrd;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyFixedFSSrvOrd
        {
            get
            {
                return this._QtyFixedFSSrvOrd;
            }
            set
            {
                this._QtyFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrd
        public abstract class qtyPOFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrd> { }
        protected decimal? _QtyPOFixedFSSrvOrd;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyPOFixedFSSrvOrd
        {
            get
            {
                return this._QtyPOFixedFSSrvOrd;
            }
            set
            {
                this._QtyPOFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrdPrepared
        public abstract class qtyPOFixedFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdPrepared> { }
        protected decimal? _QtyPOFixedFSSrvOrdPrepared;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyPOFixedFSSrvOrdPrepared
        {
            get
            {
                return this._QtyPOFixedFSSrvOrdPrepared;
            }
            set
            {
                this._QtyPOFixedFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrdReceipts
        public abstract class qtyPOFixedFSSrvOrdReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdReceipts> { }
        protected decimal? _QtyPOFixedFSSrvOrdReceipts;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyPOFixedFSSrvOrdReceipts
        {
            get
            {
                return this._QtyPOFixedFSSrvOrdReceipts;
            }
            set
            {
                this._QtyPOFixedFSSrvOrdReceipts = value;
            }
        }
        #endregion

        #region QtyProdFixedPurchase
        // M9
        public abstract class qtyProdFixedPurchase : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedPurchase> { }
        protected Decimal? _QtyProdFixedPurchase;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production to Purchase.  
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production to Purchase", Enabled = false)]
        public virtual Decimal? QtyProdFixedPurchase
        {
            get
            {
                return this._QtyProdFixedPurchase;
            }
            set
            {
                this._QtyProdFixedPurchase = value;
            }
        }
        #endregion
        #region QtyProdFixedProduction
        // MA
        public abstract class qtyProdFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProduction> { }
        protected Decimal? _QtyProdFixedProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production to Production
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production to Production", Enabled = false)]
        public virtual Decimal? QtyProdFixedProduction
        {
            get
            {
                return this._QtyProdFixedProduction;
            }
            set
            {
                this._QtyProdFixedProduction = value;
            }
        }
        #endregion
        #region QtyProdFixedProdOrdersPrepared
        // MB
        public abstract class qtyProdFixedProdOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrdersPrepared> { }
        protected Decimal? _QtyProdFixedProdOrdersPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for Prod. Prepared
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for Prod. Prepared", Enabled = false)]
        public virtual Decimal? QtyProdFixedProdOrdersPrepared
        {
            get
            {
                return this._QtyProdFixedProdOrdersPrepared;
            }
            set
            {
                this._QtyProdFixedProdOrdersPrepared = value;
            }
        }
        #endregion
        #region QtyProdFixedProdOrders
        // MC
        public abstract class qtyProdFixedProdOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrders> { }
        protected Decimal? _QtyProdFixedProdOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for Production
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for Production", Enabled = false)]
        public virtual Decimal? QtyProdFixedProdOrders
        {
            get
            {
                return this._QtyProdFixedProdOrders;
            }
            set
            {
                this._QtyProdFixedProdOrders = value;
            }
        }
        #endregion
        #region QtyProdFixedSalesOrdersPrepared
        // MD
        public abstract class qtyProdFixedSalesOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrdersPrepared> { }
        protected Decimal? _QtyProdFixedSalesOrdersPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for SO Prepared
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for SO Prepared", Enabled = false)]
        public virtual Decimal? QtyProdFixedSalesOrdersPrepared
        {
            get
            {
                return this._QtyProdFixedSalesOrdersPrepared;
            }
            set
            {
                this._QtyProdFixedSalesOrdersPrepared = value;
            }
        }
        #endregion
        #region QtyProdFixedSalesOrders
        // ME
        public abstract class qtyProdFixedSalesOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrders> { }
        protected Decimal? _QtyProdFixedSalesOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for SO
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for SO", Enabled = false)]
        public virtual Decimal? QtyProdFixedSalesOrders
        {
            get
            {
                return this._QtyProdFixedSalesOrders;
            }
            set
            {
                this._QtyProdFixedSalesOrders = value;
            }
        }
        #endregion
        #region QtySOFixed
        public abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
		protected decimal? _QtySOFixed;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOFixed
		{
			get
			{
				return this._QtySOFixed;
			}
			set
			{
				this._QtySOFixed = value;
			}
		}
		#endregion
		#region QtyPOFixedOrders
		public abstract class qtyPOFixedOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
		protected decimal? _QtyPOFixedOrders;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedOrders
		{
			get
			{
				return this._QtyPOFixedOrders;
			}
			set
			{
				this._QtyPOFixedOrders = value;
			}
		}
		#endregion
		#region QtyPOFixedPrepared
		public abstract class qtyPOFixedPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
		protected decimal? _QtyPOFixedPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedPrepared
		{
			get
			{
				return this._QtyPOFixedPrepared;
			}
			set
			{
				this._QtyPOFixedPrepared = value;
			}
		}
		#endregion
		#region QtyPOFixedReceipts
		public abstract class qtyPOFixedReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
		protected decimal? _QtyPOFixedReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedReceipts
		{
			get
			{
				return this._QtyPOFixedReceipts;
			}
			set
			{
				this._QtyPOFixedReceipts = value;
			}
		}
		#endregion
		#region QtySODropShip
		public abstract class qtySODropShip : PX.Data.BQL.BqlDecimal.Field<qtySODropShip> { }
		protected decimal? _QtySODropShip;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySODropShip
		{
			get
			{
				return this._QtySODropShip;
			}
			set
			{
				this._QtySODropShip = value;
			}
		}
		#endregion
		#region QtyPODropShipOrders
		public abstract class qtyPODropShipOrders : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
		protected decimal? _QtyPODropShipOrders;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipOrders
		{
			get
			{
				return this._QtyPODropShipOrders;
			}
			set
			{
				this._QtyPODropShipOrders = value;
			}
		}
		#endregion
		#region QtyPODropShipPrepared
		public abstract class qtyPODropShipPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
		protected decimal? _QtyPODropShipPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipPrepared
		{
			get
			{
				return this._QtyPODropShipPrepared;
			}
			set
			{
				this._QtyPODropShipPrepared = value;
			}
		}
		#endregion
		#region QtyPODropShipReceipts
		public abstract class qtyPODropShipReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
		protected decimal? _QtyPODropShipReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipReceipts
		{
			get
			{
				return this._QtyPODropShipReceipts;
			}
			set
			{
				this._QtyPODropShipReceipts = value;
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

    //Copy of original but with location restriction removed. Made for transitline support
    [System.SerializableAttribute()]
    public partial class INLocationStatus2 : INLocationStatus
    {
	    #region Keys
	    public new class PK : PrimaryKeyOf<INLocationStatus2>.By<inventoryID, subItemID, siteID, locationID>
	    {
		    public static INLocationStatus2 Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID)
			    => FindBy(graph, inventoryID, subItemID, siteID, locationID);
	    }
		public static new class FK
		{
			public class Location : INLocation.PK.ForeignKeyOf<INLocationStatus2>.By<locationID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INLocationStatus2>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INLocationStatus2>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INLocationStatus2>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INLocationStatus2>.By<inventoryID, siteID> { }
		}
	    #endregion
		#region Selected
        public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        
        #endregion
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        
        #endregion
        #region SiteID
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        
        #endregion
        #region LocationID
        public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public new Int32? LocationID
        {
            get
            {
                return this._LocationID;
            }
            set
            {
                this._LocationID = value;
            }
        }
        #endregion
        #region Active
        public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        
        #endregion
        #region QtyOnHand
        public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        
        #endregion
        #region QtyAvail
        public new abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
        
        #endregion
        #region QtyNotAvail
        public new abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        
        #endregion
        #region QtyExpired
        public new abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
        
        #endregion
        #region QtyHardAvail
        public new abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
        
        #endregion
        #region QtyInTransit
        public new abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
        
        #endregion
        #region QtyInTransitToSO
        public new abstract class qtyInTransitToSO : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
        
        #endregion
        #region QtyPOPrepared
        public new abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
        
        #endregion
        #region QtyPOOrders
        public new abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
        #endregion
        #region QtyPOReceipts
        public new abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
        
        #endregion
        #region QtySOBackOrdered
        public new abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
        
        #endregion
        #region QtySOPrepared
        public new abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
        
        #endregion
        #region QtySOBooked
        public new abstract class qtySOBooked : PX.Data.BQL.BqlDecimal.Field<qtySOBooked> { }
        
        #endregion
        #region QtySOShipped
        public new abstract class qtySOShipped : PX.Data.BQL.BqlDecimal.Field<qtySOShipped> { }
        
        #endregion
        #region QtySOShipping
        public new abstract class qtySOShipping : PX.Data.BQL.BqlDecimal.Field<qtySOShipping> { }
        
        #endregion
        #region QtyINIssues
        public new abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
        
        #endregion
        #region QtyINReceipts
        public new abstract class qtyINReceipts : PX.Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
        
        #endregion
        #region QtyINAssemblyDemand
        public new abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
        
        #endregion
        #region QtyINAssemblySupply
        public new abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
        
        #endregion
        #region QtySOFixed
        public new abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
        
        #endregion
        #region QtyPOFixedOrders
        public new abstract class qtyPOFixedOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
        
        #endregion
        #region QtyPOFixedPrepared
        public new abstract class qtyPOFixedPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
        
        #endregion
        #region QtyPOFixedReceipts
        public new abstract class qtyPOFixedReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
        
        #endregion
        #region QtySODropShip
        public new abstract class qtySODropShip : PX.Data.BQL.BqlDecimal.Field<qtySODropShip> { }
        
        #endregion
        #region QtyPODropShipOrders
        public new abstract class qtyPODropShipOrders : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
        
        #endregion
        #region QtyPODropShipPrepared
        public new abstract class qtyPODropShipPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
        
        #endregion
        #region QtyPODropShipReceipts
        public new abstract class qtyPODropShipReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
        
        #endregion
        #region tstamp
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        
        #endregion
    }
}
