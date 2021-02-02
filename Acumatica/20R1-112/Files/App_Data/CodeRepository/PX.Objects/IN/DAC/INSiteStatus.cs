using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.Common;
using PX.Objects.Common.Bql;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INSiteStatus)]
	public partial class INSiteStatus : PX.Data.IBqlTable, IStatus
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSiteStatus>.By<inventoryID, subItemID, siteID>
		{
			public static INSiteStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID)
				=> FindBy(graph, inventoryID, subItemID, siteID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INSiteStatus>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INSiteStatus>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INSiteStatus>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INSiteStatus>.By<inventoryID, siteID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
			public class InventoryBaseUnitRule :
				InventoryItem.baseUnit.PreventEditIfExists<
					Select<INSiteStatus,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
					And<Where<qtyOnHand, NotEqual<decimal0>,
						Or<qtyAvail, NotEqual<decimal0>>>>>>>
			{
			}
		}
		protected Int32? _InventoryID;
		[Inventory(IsKey = true)]
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
		[SubItem(IsKey = true)]
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
		[Site(IsKey = true)]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
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
		#region QtyNotAvail
		public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		protected Decimal? _QtyNotAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Not Available")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXUIField(DisplayName = "Qty. In Transit to SO")]
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
		#region QtyPOPrepared
		public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		protected Decimal? _QtyPOPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Prepared")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXUIField(DisplayName = "Qty. SO Backordered")]
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
		[PXUIField(DisplayName = "Qty. SO Prepared")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXUIField(DisplayName = "Qty On Production for SO Prepared", Enabled = false)]
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
        #region QtyINReplaned
        public abstract class qtyINReplaned : PX.Data.BQL.BqlDecimal.Field<qtyINReplaned> { }
		protected Decimal? _QtyINReplaned;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Replaned")]
		public virtual Decimal? QtyINReplaned
		{
			get
			{
				return this._QtyINReplaned;
			}
			set
			{
				this._QtyINReplaned = value;
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

    [System.SerializableAttribute()]
	public partial class INSiteStatusFilter : IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;	
		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<INRegister.branchID>>>))]
		[PXDefault(typeof(INRegister.siteID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[Location(typeof(INSiteStatusFilter.siteID), KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
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

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory()]
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

		#region SubItem
		public abstract class subItem : PX.Data.BQL.BqlString.Field<subItem> { }
		protected String _SubItem;
		[SubItemRawExt(typeof(INSiteStatusFilter.inventoryID), DisplayName = "Subitem")]
		public virtual String SubItem
		{
			get
			{
				return this._SubItem;
			}
			set
			{
				this._SubItem = value;
			}
		}
		#endregion

		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return this._SubItem == null ? null : SubCDUtils.CreateSubCDWildcard(this._SubItem, SubItemAttribute.DimensionName);
			}
		}
		#endregion		

		#region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName="Barcode")]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion

		#region BarCode Wildcard
		public abstract class barCodeWildcard : PX.Data.BQL.BqlString.Field<barCodeWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String BarCodeWildcard
		{
			get
			{				
				return this._BarCode == null ? null : "%" + this._BarCode + "%";
			}
		}
		#endregion		

		#region Inventory
		public abstract class inventory : PX.Data.BQL.BqlString.Field<inventory> { }
		protected String _Inventory;
		[PXDBString(IsUnicode=true)]
		[PXUIField(DisplayName = "Inventory")]
		public virtual String Inventory
		{
			get
			{
				return this._Inventory;
			}
			set
			{
				this._Inventory = value;
			}
		}
		#endregion		

		#region Inventory Wildcard
		public abstract class inventory_Wildcard : PX.Data.BQL.BqlString.Field<inventory_Wildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String Inventory_Wildcard
		{
			get
			{
				String wildcard = PXDatabase.Provider.SqlDialect.WildcardAnything;
				return this._Inventory == null ? null : wildcard + this._Inventory + wildcard;
			}
		}
		#endregion		

		#region OnlyAvailable
		public abstract class onlyAvailable : PX.Data.BQL.BqlBool.Field<onlyAvailable> { }
		protected bool? _OnlyAvailable;
		[PXBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Show Available Items Only")]
		public virtual bool? OnlyAvailable
		{
			get
			{
				return _OnlyAvailable;
			}
			set
			{
				_OnlyAvailable = value;
			}
		}
		#endregion

		#region ItemClass
		public abstract class itemClass : PX.Data.BQL.BqlString.Field<itemClass> { }
		protected string _ItemClass;

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		public virtual string ItemClass
		{
			get { return this._ItemClass; }
			set { this._ItemClass = value; }
		}
		#endregion
		#region ItemClassCDWildcard
		public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
		[PXString(IsUnicode = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXDimension(INItemClass.Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCDWildcard
		{
			get { return ItemClassTree.MakeWildcard(ItemClass); }
			set { }
		}
		#endregion
	}

	[System.SerializableAttribute()]
	[PXProjection(typeof(Select2<InventoryItem,
		LeftJoin<INLocationStatus,
						On<INLocationStatus.FK.InventoryItem>,
        LeftJoin<INLocation,
                        On<INLocationStatus.locationID, Equal<INLocation.locationID>>,
        LeftJoin<INSubItem,
						On<INLocationStatus.FK.SubItem>,
		LeftJoin<INSite,
						On<INSite.siteID, Equal<INLocationStatus.siteID>, And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>,
		LeftJoin<INItemXRef,
						On<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>,
						And2<Where<INItemXRef.subItemID, Equal<INLocationStatus.subItemID>,
								Or<INLocationStatus.subItemID, IsNull>>,
						And<Where<CurrentValue<INSiteStatusFilter.barCode>, IsNotNull,
						And<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>>>,
		LeftJoin<INItemClass,
						On<InventoryItem.FK.ItemClass>,
		LeftJoin<INPriceClass,
						On<InventoryItem.FK.PriceClass>>>>>>>>,

		Where2<CurrentMatch<InventoryItem, AccessInfo.userName>,
			And2<Where<INLocationStatus.siteID, IsNull, Or<INSite.branchID, IsNotNull, And2<CurrentMatch<INSite, AccessInfo.userName>,
					And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
						Or<SameOrganizationBranch<INSite.branchID, Current<INRegister.branchID>>>>>>>>,
			And2<Where<INLocationStatus.subItemID, IsNull,
							Or<CurrentMatch<INSubItem, AccessInfo.userName>>>,
			And2<Where<CurrentValue<INSiteStatusFilter.onlyAvailable>, Equal<CS.boolFalse>,
				 Or<INLocationStatus.qtyOnHand, Greater<CS.decimal0>>>,
			And<InventoryItem.stkItem, Equal<PX.Objects.CS.boolTrue>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>>>>>), Persistent = false)]
	public partial class INSiteStatusSelected : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
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

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(InventoryItem.inventoryID), IsKey = true)]
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

		#region InventoryCD
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		protected String _InventoryCD;
		[PXDefault()]
		[InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
		public virtual String InventoryCD
		{
			get
			{
				return this._InventoryCD;
			}
			set
			{
				this._InventoryCD = value;
			}
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(60, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
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

		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class ID", Visible = false)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), ValidComboRequired = true)]
		public virtual int? ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion

		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;
		[PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCD
		{
			get
			{
				return this._ItemClassCD;
			}
			set
			{
				this._ItemClassCD = value;
			}
		}
		#endregion

		#region ItemClassDescription
		public abstract class itemClassDescription : PX.Data.BQL.BqlString.Field<itemClassDescription> { }
		protected String _ItemClassDescription;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INItemClass.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Item Class Description", Visible = false)]
		public virtual String ItemClassDescription
		{
			get
			{
				return this._ItemClassDescription;
			}
			set
			{
				this._ItemClassDescription = value;
			}
		}
		#endregion

		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
		protected String _PriceClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
		[PXUIField(DisplayName = "Price Class ID", Visible = false)]
		public virtual String PriceClassID
		{
			get
			{
				return this._PriceClassID;
			}
			set
			{
				this._PriceClassID = value;
			}
		}
		#endregion

		#region PriceClassDescription
		public abstract class priceClassDescription : PX.Data.BQL.BqlString.Field<priceClassDescription> { }
		protected String _PriceClassDescription;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class Description", Visible = false)]
		public virtual String PriceClassDescription
		{
			get
			{
				return this._PriceClassDescription;
			}
			set
			{
				this._PriceClassDescription = value;
			}
		}
		#endregion

		#region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(255, BqlField = typeof(INItemXRef.alternateID), IsUnicode = true)]
		//[PXUIField(DisplayName = "Barcode")]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected int? _SiteID;
		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute(BqlField = typeof(INLocationStatus.siteID))]
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

        #region SiteCD
        public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
        protected String _SiteCD;
        [PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<RTrim<INSite.siteCD>, Empty>), typeof(string))]
		public virtual String SiteCD
        {
            get
            {
                return this._SiteCD;
            }
            set
            {
                this._SiteCD = value;
            }
        }
        #endregion

        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INSiteStatusSelected.siteID), BqlField = typeof(INLocationStatus.locationID))]
		[PXDefault()]
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

        #region LocationCD
        public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
        protected String _locationCD;
        [PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<RTrim<INLocation.locationCD>, Empty>), typeof(string))]
        public virtual String LocationCD
        {
            get
            {
                return this._locationCD;
            }
            set
            {
                this._locationCD = value;
            }
        }
        #endregion

        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected int? _SubItemID;
		[SubItem(typeof(INSiteStatusSelected.inventoryID), BqlField = typeof(INSubItem.subItemID))]
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

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<RTrim<INSubItem.subItemCD>, Empty>), typeof(string))]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion

		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		protected String _BaseUnit;
		[PXDefault(typeof(Search<INItemClass.baseUnit, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(InventoryItem.baseUnit))]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion

		#region QtySelected
		public abstract class qtySelected : PX.Data.BQL.BqlDecimal.Field<qtySelected> { }
		protected Decimal? _QtySelected;
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Selected")]
		public virtual Decimal? QtySelected
		{
			get
			{
				return this._QtySelected ?? 0m;
			}
			set
			{
				if (value != null && value != 0m)
					this._Selected = true;
				this._QtySelected = value;
			}
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INLocationStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
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
		[PXDBQuantity(BqlField = typeof(INLocationStatus.qtyAvail))]
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

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(InventoryItem.noteID))]
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
	}
}
