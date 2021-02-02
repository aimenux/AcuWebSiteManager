using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using System.Linq;
using PX.Common;
using PX.Objects.Common.Extensions;

namespace PX.Objects.IN
{
	#region Filter

	[Serializable]
	public partial class InventorySummaryEnqFilter : PX.Data.IBqlTable
	{

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr),
			Required = true)]
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

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[SubItemRawExt(typeof(InventorySummaryEnqFilter.inventoryID), DisplayName = "Subitem")]
		//[SubItemRaw(DisplayName = "Subitem")]
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

		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return SubCDUtils.CreateSubCDWildcard(this._SubItemCD, SubItemAttribute.DimensionName);
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		//        [Site(Visibility = PXUIVisibility.Visible)]
		[Site(DescriptionField = typeof(INSite.descr), DisplayName = "Warehouse")]
		//        [PXDefault()]
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
		[Location(typeof(InventorySummaryEnqFilter.siteID), Visibility = PXUIVisibility.Visible, KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
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

		#region ExpandByLotSerialNbr
		public abstract class expandByLotSerialNbr : PX.Data.BQL.BqlBool.Field<expandByLotSerialNbr> { }
		protected bool? _ExpandByLotSerialNbr;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Expand By Lot/Serial Number", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ExpandByLotSerialNbr
		{
			get
			{
				return this._ExpandByLotSerialNbr;
			}
			set
			{
				this._ExpandByLotSerialNbr = value;
			}
		}
		#endregion

	}
	#endregion

	#region ResultSet
    [Serializable]
	public partial class InventorySummaryEnquiryResult : PX.Data.IBqlTable, IStatus, ICostStatus 
	{
		public const int TotalLocationID = -1;

		public InventorySummaryEnquiryResult() { }

		#region GridLineNbr
		public abstract class gridLineNbr : PX.Data.BQL.BqlInt.Field<gridLineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual Int32? GridLineNbr { get; set; }
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Inventory ID", Visible = false)]
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
		[SubItem(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Subitem")]
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
		#region SiteId
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Warehouse")]
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
		[Location(typeof(siteID), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
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
		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXUIField(DisplayName = "Available for Shipment", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXUIField(DisplayName = "Available for Issue", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual decimal? QtyActual
		{
			get { return _QtyActual; }
			set { _QtyActual = value; }
		}
		#endregion
		#region QtyNotAvail
		public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		protected Decimal? _QtyNotAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Not Available", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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

        #region QtyFSSrvOrdPrepared
        public abstract class qtyFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
        protected Decimal? _QtyFSSrvOrdPrepared;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "FS Prepared", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "SERVICEMANAGEMENT")]
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
        #region QtyFSSrvOrdBooked
        public abstract class qtyFSSrvOrdBooked : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
        protected Decimal? _QtyFSSrvOrdBooked;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "FS Booked", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "SERVICEMANAGEMENT")]
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
        [PXUIField(DisplayName = "FS Allocated", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "SERVICEMANAGEMENT")]
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

        #region QtySOPrepared
        public abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		protected Decimal? _QtySOPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Prepared", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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
		[PXUIField(DisplayName = "SO Booked", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXUIField(DisplayName = "SO Shipped", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXUIField(DisplayName = "SO Allocated", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		protected Decimal? _QtySOBackOrdered;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Back Ordered", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		protected Decimal? _QtyINAssemblyDemand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In Assembly Demand", Visibility = PXUIVisibility.Visible, Enabled = false, Visible = false)]
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

		#region QtyINIssues
		public abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		protected Decimal? _QtyINIssues;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "IN Issues", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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
		[PXUIField(DisplayName = "IN Receipts", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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
		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		protected Decimal? _QtyInTransit;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In-Transit", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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
        [PXUIField(DisplayName = "In-Transit to SO", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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
		[PXUIField(DisplayName = "Purchase Prepared", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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
		[PXUIField(DisplayName = "Purchase Orders", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXUIField(DisplayName = "PO Receipts", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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

        #region QtyFixedFSSrvOrd
        public abstract class qtyFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
        protected decimal? _QtyFixedFSSrvOrd;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "FS to Purchase", FieldClass = "SERVICEMANAGEMENT")]
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
        [PXUIField(DisplayName = "Purchase for FS", FieldClass = "SERVICEMANAGEMENT")]
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
        [PXUIField(DisplayName = "Purchase for FS Prepared", FieldClass = "SERVICEMANAGEMENT")]
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
        [PXUIField(DisplayName = "Receipts for FS", FieldClass = "SERVICEMANAGEMENT")]
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
		[PXUIField(DisplayName = "SO to Purchase", Visible = false)]
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
		[PXUIField(DisplayName = "Purchase for SO", Visible = false)]
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
		[PXUIField(DisplayName = "Purchase for SO Prepared", Visible = false)]
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
        [PXUIField(DisplayName = "Receipts for SO", Visible = false)]
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
		[PXUIField(DisplayName = "SO to Drop-Ship", Visible = false)]
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
		[PXUIField(DisplayName = "Drop-Ship for SO", Visible = false)]
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
		[PXUIField(DisplayName = "Drop-Ship for SO Prepared", Visible = false)]
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
		[PXUIField(DisplayName = "Drop-Ship for SO Receipts", Visible = false)]
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
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		protected Decimal? _QtyINAssemblySupply;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In Assembly Supply", Visibility = PXUIVisibility.Visible, Enabled = false, Visible = false)]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Transit to Production")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Supply Prepared")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Supply")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Purchase for Prod. Prepared")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Purchase for Production")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Demand Prepared")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Demand")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Allocated")]
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
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "SO to Production")]
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
        [PXUIField(DisplayName = "Production to Purchase")]
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
        [PXUIField(DisplayName = "Production to Production")]
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
        [PXUIField(DisplayName = "Production for Prod. Prepared")]
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
        [PXUIField(DisplayName = "Production for Production")]
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
        [PXUIField(DisplayName = "Production for SO Prepared")]
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
        [PXUIField(DisplayName = "Production for SO")]
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

        #region QtyOnHand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "On Hand", Visibility = PXUIVisibility.SelectorVisible)]
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

		#region QtyExpired
		public abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
		protected Decimal? _QtyExpired;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Expired", Visibility = PXUIVisibility.SelectorVisible)]
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

		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		protected String _BaseUnit;
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible)]
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

		#region Qty		
		protected Decimal? _Qty;
		[PXQuantity()]		
		public virtual Decimal? Qty
		{
			[PXDependsOnFields(typeof(qtyOnHand),typeof(qtyInTransit))]
			get
			{
				return this.QtyOnHand + this.QtyInTransit;
			}			
		}
		#endregion

		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion

		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Total Cost")]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Number", Visible = false)]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion

		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Expiration Date", Visible = false)]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region IsTotal
		public abstract class isTotal : PX.Data.BQL.BqlBool.Field<isTotal>
        {
		}
		[PXBool()]
		public virtual bool? IsTotal
		{
			get;
			set;
		}
		#endregion

		public string ControlTimetamp { get; set; }
	}

	#endregion


	[PX.Objects.GL.TableAndChartDashboardType]    
    [Serializable]
	public class InventorySummaryEnq : PXGraph<InventorySummaryEnq>
	{				
		private decimal Round(decimal? a)
		{
			return Math.Round(a ?? 0m, (int)commonsetup.Current.DecPlQty, MidpointRounding.AwayFromZero);
		}

		private decimal RoundUnitCost(decimal? a)
		{
			return Math.Round(a ?? 0m, (int)commonsetup.Current.DecPlPrcCst, MidpointRounding.AwayFromZero);
		}

		private void Add(IStatus dest, IStatus source)
		{
			dest.QtyAvail += source.QtyAvail ?? 0m;
			dest.QtyHardAvail += source.QtyHardAvail ?? 0m;
			dest.QtyActual += source.QtyActual ?? 0m;
			dest.QtyNotAvail += source.QtyNotAvail ?? 0m;
			dest.QtyExpired += source.QtyExpired ?? 0m;
			dest.QtyOnHand += source.QtyOnHand ?? 0m;

            dest.QtyFSSrvOrdPrepared += source.QtyFSSrvOrdPrepared ?? 0m;
            dest.QtyFSSrvOrdBooked += source.QtyFSSrvOrdBooked ?? 0m;
            dest.QtyFSSrvOrdAllocated += source.QtyFSSrvOrdAllocated ?? 0m;

            dest.QtySOPrepared += source.QtySOPrepared ?? 0m;
			dest.QtySOBooked += source.QtySOBooked ?? 0m;
			dest.QtySOShipping += source.QtySOShipping ?? 0m;
			dest.QtySOShipped += source.QtySOShipped ?? 0m;
			dest.QtySOBackOrdered += source.QtySOBackOrdered ?? 0m;
			dest.QtyINAssemblyDemand += source.QtyINAssemblyDemand ?? 0m;
			dest.QtyINAssemblySupply += source.QtyINAssemblySupply ?? 0m;
            dest.QtyInTransitToProduction += source.QtyInTransitToProduction ?? 0m;
            dest.QtyProductionSupplyPrepared += source.QtyProductionSupplyPrepared ?? 0m;
            dest.QtyProductionSupply += source.QtyProductionSupply ?? 0m;
            dest.QtyPOFixedProductionPrepared += source.QtyPOFixedProductionPrepared ?? 0m;
            dest.QtyPOFixedProductionOrders += source.QtyPOFixedProductionOrders ?? 0m;
            dest.QtyProductionDemandPrepared += source.QtyProductionDemandPrepared ?? 0m;
            dest.QtyProductionDemand += source.QtyProductionDemand ?? 0m;
            dest.QtyProductionAllocated += source.QtyProductionAllocated ?? 0m;
            dest.QtySOFixedProduction += source.QtySOFixedProduction ?? 0m;
		    dest.QtyProdFixedPurchase += source.QtyProdFixedPurchase ?? 0m;
		    dest.QtyProdFixedProduction += source.QtyProdFixedProduction ?? 0m;
		    dest.QtyProdFixedProdOrdersPrepared += source.QtyProdFixedProdOrdersPrepared ?? 0m;
		    dest.QtyProdFixedProdOrders += source.QtyProdFixedProdOrders ?? 0m;
		    dest.QtyProdFixedSalesOrdersPrepared += source.QtyProdFixedSalesOrdersPrepared ?? 0m;
		    dest.QtyProdFixedSalesOrders += source.QtyProdFixedSalesOrders ?? 0m;
            dest.QtyINIssues += source.QtyINIssues ?? 0m;
			dest.QtyINReceipts += source.QtyINReceipts ?? 0m;
			dest.QtyInTransit += source.QtyInTransit ?? 0m;
            dest.QtyInTransitToSO += source.QtyInTransitToSO ?? 0m;
			dest.QtyPOPrepared += source.QtyPOPrepared ?? 0m;
			dest.QtyPOOrders += source.QtyPOOrders ?? 0m;
			dest.QtyPOReceipts += source.QtyPOReceipts ?? 0m;

            dest.QtyFixedFSSrvOrd += source.QtyFixedFSSrvOrd ?? 0m;
            dest.QtyPOFixedFSSrvOrd += source.QtyPOFixedFSSrvOrd ?? 0m;
            dest.QtyPOFixedFSSrvOrdPrepared += source.QtyPOFixedFSSrvOrdPrepared ?? 0m;
            dest.QtyPOFixedFSSrvOrdReceipts += source.QtyPOFixedFSSrvOrdReceipts ?? 0m;

            dest.QtySOFixed += source.QtySOFixed ?? 0m;
			dest.QtyPOFixedOrders += source.QtyPOFixedOrders ?? 0m;
			dest.QtyPOFixedPrepared += source.QtyPOFixedPrepared ?? 0m;
			dest.QtyPOFixedReceipts += source.QtyPOFixedReceipts ?? 0m;
			dest.QtySODropShip += source.QtySODropShip ?? 0m;
			dest.QtyPODropShipOrders += source.QtyPODropShipOrders ?? 0m;
			dest.QtyPODropShipPrepared += source.QtyPODropShipPrepared ?? 0m;
			dest.QtyPODropShipReceipts += source.QtyPODropShipReceipts ?? 0m;

			ICostStatus costdest = dest as ICostStatus;
			ICostStatus costsource = source as ICostStatus;
			if (costdest != null && costsource != null)
			{
				costdest.TotalCost += costsource.TotalCost;
			}
		}

		private void Subtract(IStatus dest, IStatus source)
		{
			dest.QtyAvail -= source.QtyAvail ?? 0m;
			dest.QtyHardAvail -= source.QtyHardAvail ?? 0m;
			dest.QtyActual -= source.QtyActual ?? 0m;
			dest.QtyNotAvail -= source.QtyNotAvail ?? 0m;
			dest.QtyExpired -= source.QtyExpired ?? 0m;
			dest.QtyOnHand -= source.QtyOnHand ?? 0m;

            dest.QtyFSSrvOrdPrepared -= source.QtyFSSrvOrdPrepared ?? 0m;
            dest.QtyFSSrvOrdBooked -= source.QtyFSSrvOrdBooked ?? 0m;
            dest.QtyFSSrvOrdAllocated -= source.QtyFSSrvOrdAllocated ?? 0m;

            dest.QtySOPrepared -= source.QtySOPrepared ?? 0m;
			dest.QtySOBooked -= source.QtySOBooked ?? 0m;
			dest.QtySOShipping -= source.QtySOShipping ?? 0m;
			dest.QtySOShipped -= source.QtySOShipped ?? 0m;
			dest.QtySOBackOrdered -= source.QtySOBackOrdered ?? 0m;
			dest.QtyINAssemblyDemand -= source.QtyINAssemblyDemand ?? 0m;
			dest.QtyINAssemblySupply -= source.QtyINAssemblySupply ?? 0m;
            dest.QtyInTransitToProduction -= source.QtyInTransitToProduction ?? 0m;
            dest.QtyProductionSupplyPrepared -= source.QtyProductionSupplyPrepared ?? 0m;
            dest.QtyProductionSupply -= source.QtyProductionSupply ?? 0m;
            dest.QtyPOFixedProductionPrepared -= source.QtyPOFixedProductionPrepared ?? 0m;
            dest.QtyPOFixedProductionOrders -= source.QtyPOFixedProductionOrders ?? 0m;
            dest.QtyProductionDemandPrepared -= source.QtyProductionDemandPrepared ?? 0m;
            dest.QtyProductionDemand -= source.QtyProductionDemand ?? 0m;
            dest.QtyProductionAllocated -= source.QtyProductionAllocated ?? 0m;
            dest.QtySOFixedProduction -= source.QtySOFixedProduction ?? 0m;
		    dest.QtyProdFixedPurchase -= source.QtyProdFixedPurchase ?? 0m;
		    dest.QtyProdFixedProduction -= source.QtyProdFixedProduction ?? 0m;
		    dest.QtyProdFixedProdOrdersPrepared -= source.QtyProdFixedProdOrdersPrepared ?? 0m;
		    dest.QtyProdFixedProdOrders -= source.QtyProdFixedProdOrders ?? 0m;
		    dest.QtyProdFixedSalesOrdersPrepared -= source.QtyProdFixedSalesOrdersPrepared ?? 0m;
		    dest.QtyProdFixedSalesOrders -= source.QtyProdFixedSalesOrders ?? 0m;
            dest.QtyINIssues -= source.QtyINIssues ?? 0m;
			dest.QtyINReceipts -= source.QtyINReceipts ?? 0m;
			dest.QtyInTransit -= source.QtyInTransit ?? 0m;
            dest.QtyInTransitToSO -= source.QtyInTransitToSO ?? 0m;
			dest.QtyPOPrepared -= source.QtyPOPrepared ?? 0m;
			dest.QtyPOOrders -= source.QtyPOOrders ?? 0m;
			dest.QtyPOReceipts -= source.QtyPOReceipts ?? 0m;

            dest.QtyFixedFSSrvOrd -= source.QtyFixedFSSrvOrd ?? 0m;
            dest.QtyPOFixedFSSrvOrd -= source.QtyPOFixedFSSrvOrd ?? 0m;
            dest.QtyPOFixedFSSrvOrdPrepared -= source.QtyPOFixedFSSrvOrdPrepared ?? 0m;
            dest.QtyPOFixedFSSrvOrdReceipts -= source.QtyPOFixedFSSrvOrdReceipts ?? 0m;

            dest.QtySOFixed -= source.QtySOFixed ?? 0m;
			dest.QtyPOFixedOrders -= source.QtyPOFixedOrders ?? 0m;
			dest.QtyPOFixedPrepared -= source.QtyPOFixedPrepared ?? 0m;
			dest.QtyPOFixedReceipts -= source.QtyPOFixedReceipts ?? 0m;
			dest.QtySODropShip -= source.QtySODropShip ?? 0m;
			dest.QtyPODropShipOrders -= source.QtyPODropShipOrders ?? 0m;
			dest.QtyPODropShipPrepared -= source.QtyPODropShipPrepared ?? 0m;
			dest.QtyPODropShipReceipts -= source.QtyPODropShipReceipts ?? 0m;

			ICostStatus costdest = dest as ICostStatus;
			ICostStatus costsource = source as ICostStatus;
			if (costdest != null && costsource != null)
			{
				costdest.TotalCost -= costsource.TotalCost;
			}
		}

		private void Copy(IStatus dest, IStatus source)
		{
			dest.QtyAvail = source.QtyAvail ?? 0m;
			dest.QtyHardAvail = source.QtyHardAvail ?? 0m;
			dest.QtyActual = source.QtyActual ?? 0m;
			dest.QtyNotAvail = source.QtyNotAvail ?? 0m;
			dest.QtyOnHand = source.QtyOnHand ?? 0m;

            dest.QtyFSSrvOrdPrepared = source.QtyFSSrvOrdPrepared ?? 0m;
            dest.QtyFSSrvOrdBooked = source.QtyFSSrvOrdBooked ?? 0m;
            dest.QtyFSSrvOrdAllocated = source.QtyFSSrvOrdAllocated ?? 0m;

            dest.QtySOPrepared = source.QtySOPrepared ?? 0m;
			dest.QtySOBooked = source.QtySOBooked ?? 0m;
			dest.QtySOShipping = source.QtySOShipping ?? 0m;
			dest.QtySOShipped = source.QtySOShipped ?? 0m;
			dest.QtySOBackOrdered = source.QtySOBackOrdered ?? 0m;
			dest.QtyINAssemblyDemand = source.QtyINAssemblyDemand ?? 0m;
			dest.QtyINAssemblySupply = source.QtyINAssemblySupply ?? 0m;
            dest.QtyInTransitToProduction = source.QtyInTransitToProduction ?? 0m;
            dest.QtyProductionSupplyPrepared = source.QtyProductionSupplyPrepared ?? 0m;
            dest.QtyProductionSupply = source.QtyProductionSupply ?? 0m;
            dest.QtyPOFixedProductionPrepared = source.QtyPOFixedProductionPrepared ?? 0m;
            dest.QtyPOFixedProductionOrders = source.QtyPOFixedProductionOrders ?? 0m;
            dest.QtyProductionDemandPrepared = source.QtyProductionDemandPrepared ?? 0m;
            dest.QtyProductionDemand = source.QtyProductionDemand ?? 0m;
            dest.QtyProductionAllocated = source.QtyProductionAllocated ?? 0m;
            dest.QtySOFixedProduction = source.QtySOFixedProduction ?? 0m;
		    dest.QtyProdFixedPurchase = source.QtyProdFixedPurchase ?? 0m;
		    dest.QtyProdFixedProduction = source.QtyProdFixedProduction ?? 0m;
		    dest.QtyProdFixedProdOrdersPrepared = source.QtyProdFixedProdOrdersPrepared ?? 0m;
		    dest.QtyProdFixedProdOrders = source.QtyProdFixedProdOrders ?? 0m;
		    dest.QtyProdFixedSalesOrdersPrepared = source.QtyProdFixedSalesOrdersPrepared ?? 0m;
		    dest.QtyProdFixedSalesOrders = source.QtyProdFixedSalesOrders ?? 0m;
            dest.QtyINIssues = source.QtyINIssues ?? 0m;
			dest.QtyINReceipts = source.QtyINReceipts ?? 0m;
			dest.QtyInTransit = source.QtyInTransit ?? 0m;
            dest.QtyInTransitToSO = source.QtyInTransitToSO ?? 0m;
			dest.QtyPOPrepared = source.QtyPOPrepared ?? 0m;
			dest.QtyPOOrders = source.QtyPOOrders ?? 0m;
			dest.QtyPOReceipts = source.QtyPOReceipts ?? 0m;

            dest.QtyFixedFSSrvOrd = source.QtyFixedFSSrvOrd ?? 0m;
            dest.QtyPOFixedFSSrvOrd = source.QtyPOFixedFSSrvOrd ?? 0m;
            dest.QtyPOFixedFSSrvOrdPrepared = source.QtyPOFixedFSSrvOrdPrepared ?? 0m;
            dest.QtyPOFixedFSSrvOrdReceipts = source.QtyPOFixedFSSrvOrdReceipts ?? 0m;

            dest.QtySOFixed = source.QtySOFixed ?? 0m;
			dest.QtyPOFixedOrders = source.QtyPOFixedOrders ?? 0m;
			dest.QtyPOFixedPrepared = source.QtyPOFixedPrepared ?? 0m;
			dest.QtyPOFixedReceipts = source.QtyPOFixedReceipts ?? 0m;
			dest.QtySODropShip = source.QtySODropShip ?? 0m;
			dest.QtyPODropShipOrders = source.QtyPODropShipOrders ?? 0m;
			dest.QtyPODropShipPrepared = source.QtyPODropShipPrepared ?? 0m;
			dest.QtyPODropShipReceipts = source.QtyPODropShipReceipts ?? 0m;
		}

		private T Copy<T>(IStatus source)
			where T : class, IStatus, new()
		{
			T dest = new T();
			Copy(dest, source);
			return dest;
		}

		private T Subtract<T>(IStatus a, IStatus b)
			where T : class, IBqlTable, IStatus, new()
		{
			T c = new T();
			if (typeof(T).IsAssignableFrom(a.GetType()))
			{
				PXCache<T>.RestoreCopy(c, (T)a);
				Subtract(c, b);
				return c;
			}

			c.QtyAvail = Round((a.QtyAvail ?? 0m) - (b.QtyAvail ?? 0m));
			c.QtyHardAvail = Round((a.QtyHardAvail ?? 0m) - (b.QtyHardAvail ?? 0m));
			c.QtyActual = Round((a.QtyActual ?? 0m) - (b.QtyActual ?? 0m));
			c.QtyNotAvail = Round((a.QtyNotAvail ?? 0m) - (b.QtyNotAvail ?? 0m));
			c.QtyExpired = Round((a.QtyExpired ?? 0m) - (b.QtyExpired ?? 0m));
			c.QtyOnHand = Round((a.QtyOnHand ?? 0m) - (b.QtyOnHand ?? 0m));

            c.QtyFSSrvOrdPrepared = Round((a.QtyFSSrvOrdPrepared ?? 0m) - (b.QtyFSSrvOrdPrepared ?? 0m));
            c.QtyFSSrvOrdBooked = Round((a.QtyFSSrvOrdBooked ?? 0m) - (b.QtyFSSrvOrdBooked ?? 0m));
            c.QtyFSSrvOrdAllocated = Round((a.QtyFSSrvOrdAllocated ?? 0m) - (b.QtyFSSrvOrdAllocated ?? 0m));

            c.QtySOPrepared = Round((a.QtySOPrepared ?? 0m) - (b.QtySOPrepared ?? 0m));
			c.QtySOBooked = Round((a.QtySOBooked ?? 0m) - (b.QtySOBooked ?? 0m));
			c.QtySOShipping = Round((a.QtySOShipping ?? 0m) - (b.QtySOShipping ?? 0m));
			c.QtySOShipped = Round((a.QtySOShipped ?? 0m) - (b.QtySOShipped ?? 0m));
			c.QtySOBackOrdered = Round((a.QtySOBackOrdered ?? 0m) - (b.QtySOBackOrdered ?? 0m));
			c.QtyINIssues = Round((a.QtyINIssues ?? 0m) - (b.QtyINIssues ?? 0m));
			c.QtyINReceipts = Round((a.QtyINReceipts ?? 0m) - (b.QtyINReceipts ?? 0m));
			c.QtyInTransit = Round((a.QtyInTransit ?? 0m) - (b.QtyInTransit ?? 0m));
            c.QtyInTransitToSO = Round((a.QtyInTransitToSO ?? 0m) - (b.QtyInTransitToSO ?? 0m));
			c.QtyINAssemblyDemand = Round((a.QtyINAssemblyDemand ?? 0m) - (b.QtyINAssemblyDemand ?? 0m));
			c.QtyINAssemblySupply = Round((a.QtyINAssemblySupply ?? 0m) - (b.QtyINAssemblySupply ?? 0m));
            c.QtyInTransitToProduction = Round((a.QtyInTransitToProduction ?? 0m) - (b.QtyInTransitToProduction ?? 0m));
            c.QtyProductionSupplyPrepared = Round((a.QtyProductionSupplyPrepared ?? 0m) - (b.QtyProductionSupplyPrepared ?? 0m));
            c.QtyProductionSupply = Round((a.QtyProductionSupply ?? 0m) - (b.QtyProductionSupply ?? 0m));
            c.QtyPOFixedProductionPrepared = Round((a.QtyPOFixedProductionPrepared ?? 0m) - (b.QtyPOFixedProductionPrepared ?? 0m));
            c.QtyPOFixedProductionOrders = Round((a.QtyPOFixedProductionOrders ?? 0m) - (b.QtyPOFixedProductionOrders ?? 0m));
            c.QtyProductionDemandPrepared = Round((a.QtyProductionDemandPrepared ?? 0m) - (b.QtyProductionDemandPrepared ?? 0m));
            c.QtyProductionDemand = Round((a.QtyProductionDemand ?? 0m) - (b.QtyProductionDemand ?? 0m));
            c.QtyProductionAllocated = Round((a.QtyProductionAllocated ?? 0m) - (b.QtyProductionAllocated ?? 0m));
            c.QtySOFixedProduction = Round((a.QtySOFixedProduction ?? 0m) - (b.QtySOFixedProduction ?? 0m));
		    c.QtyProdFixedPurchase = Round((a.QtyProdFixedPurchase ?? 0m) - (b.QtyProdFixedPurchase ?? 0m));
		    c.QtyProdFixedProduction = Round((a.QtyProdFixedProduction ?? 0m) - (b.QtyProdFixedProduction ?? 0m));
		    c.QtyProdFixedProdOrdersPrepared = Round((a.QtyProdFixedProdOrdersPrepared ?? 0m) - (b.QtyProdFixedProdOrdersPrepared ?? 0m));
		    c.QtyProdFixedProdOrders = Round((a.QtyProdFixedProdOrders ?? 0m) - (b.QtyProdFixedProdOrders ?? 0m));
		    c.QtyProdFixedSalesOrdersPrepared = Round((a.QtyProdFixedSalesOrdersPrepared ?? 0m) - (b.QtyProdFixedSalesOrdersPrepared ?? 0m));
		    c.QtyProdFixedSalesOrders = Round((a.QtyProdFixedSalesOrders ?? 0m) - (b.QtyProdFixedSalesOrders ?? 0m));
            c.QtyPOPrepared = Round((a.QtyPOPrepared ?? 0m) - (b.QtyPOPrepared ?? 0m));
			c.QtyPOOrders = Round((a.QtyPOOrders ?? 0m) - (b.QtyPOOrders ?? 0m));
			c.QtyPOReceipts = Round((a.QtyPOReceipts ?? 0m) - (b.QtyPOReceipts ?? 0m));

            c.QtyFixedFSSrvOrd = Round((a.QtyFixedFSSrvOrd ?? 0m) - (b.QtyFixedFSSrvOrd ?? 0m));
            c.QtyPOFixedFSSrvOrd = Round((a.QtyPOFixedFSSrvOrd ?? 0m) - (b.QtyPOFixedFSSrvOrd ?? 0m));
            c.QtyPOFixedFSSrvOrdPrepared = Round((a.QtyPOFixedFSSrvOrdPrepared ?? 0m) - (b.QtyPOFixedFSSrvOrdPrepared ?? 0m));
            c.QtyPOFixedFSSrvOrdReceipts = Round((a.QtyPOFixedFSSrvOrdReceipts ?? 0m) - (b.QtyPOFixedFSSrvOrdReceipts ?? 0m));

            c.QtySOFixed = Round((a.QtySOFixed ?? 0m) - (b.QtySOFixed ?? 0m));
			c.QtyPOFixedOrders = Round((a.QtyPOFixedOrders ?? 0m) - (b.QtyPOFixedOrders ?? 0m));
			c.QtyPOFixedPrepared = Round((a.QtyPOFixedPrepared ?? 0m) - (b.QtyPOFixedPrepared ?? 0m));
			c.QtyPOFixedReceipts = Round((a.QtyPOFixedReceipts ?? 0m) - (b.QtyPOFixedReceipts ?? 0m));
			c.QtySODropShip = Round((a.QtySODropShip ?? 0m) - (b.QtySODropShip ?? 0m));
			c.QtyPODropShipOrders = Round((a.QtyPODropShipOrders ?? 0m) - (b.QtyPODropShipOrders ?? 0m));
			c.QtyPODropShipPrepared = Round((a.QtyPODropShipPrepared ?? 0m) - (b.QtyPODropShipPrepared ?? 0m));
			c.QtyPODropShipReceipts = Round((a.QtyPODropShipReceipts ?? 0m) - (b.QtyPODropShipReceipts ?? 0m));

			ICostStatus cost_a = a as ICostStatus;
			ICostStatus cost_b = b as ICostStatus;
			ICostStatus cost_c = c as ICostStatus;
			if (cost_a != null && cost_b != null && cost_c != null)
			{
				cost_c.TotalCost = (cost_a.TotalCost ?? 0m) - (cost_b.TotalCost ?? 0m);
			}

			return c;
		}

		private T Add<T>(IStatus a, IStatus b)
			where T : class, IBqlTable, IStatus, new()
		{
			T c = new T();
			if (typeof(T).IsAssignableFrom(a.GetType()))
			{
				PXCache<T>.RestoreCopy(c, (T)a);
				Add(c, b);
				return c;
			}

			c.QtyAvail = Round((a.QtyAvail ?? 0m) + (b.QtyAvail ?? 0m));
			c.QtyHardAvail = Round((a.QtyHardAvail ?? 0m) + (b.QtyHardAvail ?? 0m));
			c.QtyActual = Round((a.QtyActual ?? 0m) + (b.QtyActual ?? 0m));
			c.QtyNotAvail = Round((a.QtyNotAvail ?? 0m) + (b.QtyNotAvail ?? 0m));
			c.QtyExpired = Round((a.QtyExpired ?? 0m) + (b.QtyExpired ?? 0m));

            c.QtyFSSrvOrdPrepared = Round((a.QtyFSSrvOrdPrepared ?? 0m) + (b.QtyFSSrvOrdPrepared ?? 0m));
            c.QtyFSSrvOrdBooked = Round((a.QtyFSSrvOrdBooked ?? 0m) + (b.QtyFSSrvOrdBooked ?? 0m));
            c.QtyFSSrvOrdAllocated = Round((a.QtyFSSrvOrdAllocated ?? 0m) + (b.QtyFSSrvOrdAllocated ?? 0m));

            c.QtyOnHand = Round((a.QtyOnHand ?? 0m) + (b.QtyOnHand ?? 0m));
			c.QtySOPrepared = Round((a.QtySOPrepared ?? 0m) + (b.QtySOPrepared ?? 0m));
			c.QtySOBooked = Round((a.QtySOBooked ?? 0m) + (b.QtySOBooked ?? 0m));
			c.QtySOShipping = Round((a.QtySOShipping ?? 0m) + (b.QtySOShipping ?? 0m));
			c.QtySOShipped = Round((a.QtySOShipped ?? 0m) + (b.QtySOShipped ?? 0m));
			c.QtySOBackOrdered = Round((a.QtySOBackOrdered ?? 0m) + (b.QtySOBackOrdered ?? 0m));
			c.QtyINIssues = Round((a.QtyINIssues ?? 0m) + (b.QtyINIssues ?? 0m));
			c.QtyINReceipts = Round((a.QtyINReceipts ?? 0m) + (b.QtyINReceipts ?? 0m));
			c.QtyInTransit = Round((a.QtyInTransit ?? 0m) + (b.QtyInTransit ?? 0m));
            c.QtyInTransitToSO = Round((a.QtyInTransitToSO ?? 0m) + (b.QtyInTransitToSO ?? 0m));
			c.QtyINAssemblyDemand = Round((a.QtyINAssemblyDemand ?? 0m) + (b.QtyINAssemblyDemand ?? 0m));
			c.QtyINAssemblySupply = Round((a.QtyINAssemblySupply ?? 0m) + (b.QtyINAssemblySupply ?? 0m));
            c.QtyInTransitToProduction = Round((a.QtyInTransitToProduction ?? 0m) + (b.QtyInTransitToProduction ?? 0m));
            c.QtyProductionSupplyPrepared = Round((a.QtyProductionSupplyPrepared ?? 0m) + (b.QtyProductionSupplyPrepared ?? 0m));
            c.QtyProductionSupply = Round((a.QtyProductionSupply ?? 0m) + (b.QtyProductionSupply ?? 0m));
            c.QtyPOFixedProductionPrepared = Round((a.QtyPOFixedProductionPrepared ?? 0m) + (b.QtyPOFixedProductionPrepared ?? 0m));
            c.QtyPOFixedProductionOrders = Round((a.QtyPOFixedProductionOrders ?? 0m) + (b.QtyPOFixedProductionOrders ?? 0m));
            c.QtyProductionDemandPrepared = Round((a.QtyProductionDemandPrepared ?? 0m) + (b.QtyProductionDemandPrepared ?? 0m));
            c.QtyProductionDemand = Round((a.QtyProductionDemand ?? 0m) + (b.QtyProductionDemand ?? 0m));
            c.QtyProductionAllocated = Round((a.QtyProductionAllocated ?? 0m) + (b.QtyProductionAllocated ?? 0m));
            c.QtySOFixedProduction = Round((a.QtySOFixedProduction ?? 0m) + (b.QtySOFixedProduction ?? 0m));
		    c.QtyProdFixedPurchase = Round((a.QtyProdFixedPurchase ?? 0m) + (b.QtyProdFixedPurchase ?? 0m));
		    c.QtyProdFixedProduction = Round((a.QtyProdFixedProduction ?? 0m) + (b.QtyProdFixedProduction ?? 0m));
		    c.QtyProdFixedProdOrdersPrepared = Round((a.QtyProdFixedProdOrdersPrepared ?? 0m) + (b.QtyProdFixedProdOrdersPrepared ?? 0m));
		    c.QtyProdFixedProdOrders = Round((a.QtyProdFixedProdOrders ?? 0m) + (b.QtyProdFixedProdOrders ?? 0m));
		    c.QtyProdFixedSalesOrdersPrepared = Round((a.QtyProdFixedSalesOrdersPrepared ?? 0m) + (b.QtyProdFixedSalesOrdersPrepared ?? 0m));
		    c.QtyProdFixedSalesOrders = Round((a.QtyProdFixedSalesOrders ?? 0m) + (b.QtyProdFixedSalesOrders ?? 0m));
            c.QtyPOPrepared = Round((a.QtyPOPrepared ?? 0m) + (b.QtyPOPrepared ?? 0m));
			c.QtyPOOrders = Round((a.QtyPOOrders ?? 0m) + (b.QtyPOOrders ?? 0m));
			c.QtyPOReceipts = Round((a.QtyPOReceipts ?? 0m) + (b.QtyPOReceipts ?? 0m));

            c.QtyFixedFSSrvOrd = Round((a.QtyFixedFSSrvOrd ?? 0m) + (b.QtyFixedFSSrvOrd ?? 0m));
            c.QtyPOFixedFSSrvOrd = Round((a.QtyPOFixedFSSrvOrd ?? 0m) + (b.QtyPOFixedFSSrvOrd ?? 0m));
            c.QtyPOFixedFSSrvOrdPrepared = Round((a.QtyPOFixedFSSrvOrdPrepared ?? 0m) + (b.QtyPOFixedFSSrvOrdPrepared ?? 0m));
            c.QtyPOFixedFSSrvOrdReceipts = Round((a.QtyPOFixedFSSrvOrdReceipts ?? 0m) + (b.QtyPOFixedFSSrvOrdReceipts ?? 0m));

            c.QtySOFixed = Round((a.QtySOFixed ?? 0m) + (b.QtySOFixed ?? 0m));
			c.QtyPOFixedOrders = Round((a.QtyPOFixedOrders ?? 0m) + (b.QtyPOFixedOrders ?? 0m));
			c.QtyPOFixedPrepared = Round((a.QtyPOFixedPrepared ?? 0m) + (b.QtyPOFixedPrepared ?? 0m));
			c.QtyPOFixedReceipts = Round((a.QtyPOFixedReceipts ?? 0m) + (b.QtyPOFixedReceipts ?? 0m));
			c.QtySODropShip = Round((a.QtySODropShip ?? 0m) + (b.QtySODropShip ?? 0m));
			c.QtyPODropShipOrders = Round((a.QtyPODropShipOrders ?? 0m) + (b.QtyPODropShipOrders ?? 0m));
			c.QtyPODropShipPrepared = Round((a.QtyPODropShipPrepared ?? 0m) + (b.QtyPODropShipPrepared ?? 0m));
			c.QtyPODropShipReceipts = Round((a.QtyPODropShipReceipts ?? 0m) + (b.QtyPODropShipReceipts ?? 0m));

			ICostStatus cost_a = a as ICostStatus;
			ICostStatus cost_b = b as ICostStatus;
			ICostStatus cost_c = c as ICostStatus;
			if (cost_a != null && cost_b != null && cost_c != null)
			{
				cost_c.TotalCost = (cost_a.TotalCost ?? 0m) + (cost_b.TotalCost ?? 0m);
			}

			return c;
		}


		public PXCancel<InventorySummaryEnqFilter> Cancel;
		public PXAction<InventorySummaryEnqFilter> viewAllocDet;
		public PXFilter<InventorySummaryEnqFilter> Filter;
		[PXFilterable]
		public PXSelect<InventorySummaryEnquiryResult> ISERecords; // ISE = Inventory Summary Enquiry
		protected virtual IEnumerable iSERecords()
		{
			string controlTimeStamp = ControlTimeStamp;
			if (PXView.MaximumRows == 1 && PXView.Searches != null && PXView.Searches.Length == 1)
			{
				InventorySummaryEnquiryResult other = new InventorySummaryEnquiryResult();
				other.GridLineNbr = (int?)PXView.Searches[0];
				other = (InventorySummaryEnquiryResult)ISERecords.Cache.Locate(other);
				if (other != null && other.ControlTimetamp == controlTimeStamp)
				{
					PXDelegateResult oneRowResult = new PXDelegateResult();
					oneRowResult.Add(other);
					oneRowResult.IsResultFiltered = true;
					oneRowResult.IsResultSorted = true;
					oneRowResult.IsResultTruncated = true;

					return oneRowResult;
				}
			}
			int lineIndex = 0;

			if (!ISERecords.Cache.Cached.Any_() || ISERecords.Cache.Cached.RowCast<InventorySummaryEnquiryResult>().First().ControlTimetamp != controlTimeStamp)
			{
				ISERecords.Cache.Clear();
				foreach (var item in iSERecordsFetch().OfType<InventorySummaryEnquiryResult>())
				{
					item.GridLineNbr = ++lineIndex;
					ISERecords.Cache.Hold(item);
				}
			}
			else
				lineIndex = (int)ISERecords.Cache.Cached.Count();

			if (!IsImport && !IsExport && !IsContractBasedAPI)
			{
				var resultset = ISERecords.Cache.Cached.RowCast<InventorySummaryEnquiryResult>();
				var total = CalculateSummaryTotal(resultset);
				total.GridLineNbr = ++lineIndex;

				return SortSummaryResult(resultset, total);
			}
			else
			{
				return ISERecords.Cache.Cached;
			}
		}

		public PXSetupOptional<CommonSetup> commonsetup;

		private Lazy<string> _locationDisplayName;


		public InventorySummaryEnq()
		{
			ISERecords.Cache.AllowInsert = false;
			ISERecords.Cache.AllowDelete = false;
			ISERecords.Cache.AllowUpdate = false;

			object record = commonsetup.Current;

			_locationDisplayName = new Lazy<string>(GetLocationDisplayName);
		}

		protected virtual void InventorySummaryEnqFilter_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void InventorySummaryEnquiryResult_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void InventorySummaryEnqFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			InventorySummaryEnqFilter filter = (InventorySummaryEnqFilter)e.Row;
			if (filter == null) return;

			bool expandByLotSerialNbr = filter.ExpandByLotSerialNbr ?? false;

			PXUIFieldAttribute.SetVisible<InventorySummaryEnquiryResult.lotSerialNbr>(ISERecords.Cache, null, expandByLotSerialNbr);
			PXUIFieldAttribute.SetVisible<InventorySummaryEnquiryResult.expireDate>(ISERecords.Cache, null, expandByLotSerialNbr);
			PXUIFieldAttribute.SetVisible<InventorySummaryEnquiryResult.qtyExpired>(ISERecords.Cache, null, expandByLotSerialNbr);
		}

		protected virtual void _(Events.RowInserted<InventorySummaryEnqFilter> e) => ISERecords.Cache.Clear();
		protected virtual void _(Events.RowUpdated<InventorySummaryEnqFilter> e) => ISERecords.Cache.Clear();
		protected virtual void _(Events.RowDeleted<InventorySummaryEnqFilter> e) => ISERecords.Cache.Clear();

		protected virtual void AppendFilters<T>(PXSelectBase<T> cmd, InventorySummaryEnqFilter filter)
			where T : class, IBqlTable, new()
		{
			if (filter.InventoryID != null)
			{
				cmd.WhereAnd<Where<InventoryItem.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubItemCD))
			{
				cmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventorySummaryEnqFilter.subItemCDWildcard>>>>();
			}

			if (filter.SiteID != null)
			{
				cmd.WhereAnd<Where<INSite.siteID, Equal<Current<InventorySummaryEnqFilter.siteID>>>>();
			}

			if (typeof(T) != typeof(INSiteStatus) && typeof(T) != typeof(INItemPlan) && filter.LocationID != null)
			{
				cmd.WhereAnd<Where<INLocation.locationID, Equal<Current<InventorySummaryEnqFilter.locationID>>>>();
			}
		}

		protected virtual void InventorySummaryEnquiryResult_LocationID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string locationName = null;

			switch (e.ReturnValue)
			{
				case null:
					locationName = Messages.Unassigned;
					break;
				case InventorySummaryEnquiryResult.TotalLocationID:
					locationName = Messages.TotalLocation;
					break;
			}

			if (locationName != null)
			{
				e.ReturnState = PXFieldState.CreateInstance(PXMessages.LocalizeNoPrefix(locationName), typeof(string), false, null, null, null, null, null,
					nameof(InventorySummaryEnquiryResult.locationID), null, _locationDisplayName.Value, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
				e.Cancel = true;
			}
		}

		private string GetLocationDisplayName()
		{
			var displayName = PXUIFieldAttribute.GetDisplayName<InventorySummaryEnquiryResult.locationID>(ISERecords.Cache);
			if (displayName != null) displayName = PXMessages.LocalizeNoPrefix(displayName);

			return displayName;
		}

		protected virtual IEnumerable iSERecordsFetch()
		{
			var resultset = new List<(InventorySummaryEnquiryResult Result, INSubItem SubItem, INSite Site, INLocation Location)>();

			int decPlQty = (commonsetup.Current.DecPlQty ?? 6);
			int decPlPrcCst = (commonsetup.Current.DecPlPrcCst ?? 6);

			InventorySummaryEnqFilter filter = Filter.Current;

			this.Caches[typeof(INSiteStatus)].Clear();
			this.Caches[typeof(INLocationStatus)].Clear();

			this.Caches[typeof(SiteStatus)].Clear();
			this.Caches[typeof(LocationStatus)].Clear();

			PXSelectBase<INLotSerialStatus> cmd_lss = new PXSelectReadonly2<INLotSerialStatus,
				InnerJoin<INLocation,
					On<INLotSerialStatus.FK.Location>,
				InnerJoin<InventoryItem,
					On<InventoryItem.inventoryID, Equal<INLotSerialStatus.inventoryID>,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
				InnerJoin<INSite,
					On2<INLotSerialStatus.FK.Site,
					And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>,
				InnerJoin<INSubItem,
					On<INLotSerialStatus.FK.SubItem>,
				LeftJoin<INLotSerClass,
					On<InventoryItem.FK.LotSerClass>,
				LeftJoin<INLocationCostStatus,
					On<INLocationCostStatus.inventoryID, Equal<INLotSerialStatus.inventoryID>,
						And<INLocationCostStatus.subItemID, Equal<INLotSerialStatus.subItemID>,
						And<INLocationCostStatus.locationID, Equal<INLotSerialStatus.locationID>>>>,
				LeftJoin<INSiteCostStatus,
					On<INSiteCostStatus.inventoryID, Equal<INLotSerialStatus.inventoryID>,
						And<INSiteCostStatus.subItemID, Equal<INLotSerialStatus.subItemID>,
						And<INSiteCostStatus.siteID, Equal<INLotSerialStatus.siteID>>>>>>>>>>>,
				Where<INLotSerialStatus.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>,
					And<Where<Current<InventorySummaryEnqFilter.expandByLotSerialNbr>, Equal<True>, And<INLotSerClass.lotSerAssign, Equal<INLotSerAssign.whenReceived>,
					Or<InventoryItem.valMethod, Equal<INValMethod.specific>>>>>>>(this);

			AppendFilters<INLotSerialStatus>(cmd_lss, filter);

			var cmdCostStatus = new PXSelectReadonly<INLotSerialCostStatus,
				Where<INLotSerialCostStatus.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>,
					And<INLotSerialCostStatus.lotSerialNbr, IsNotNull>>>(this);

			if (filter.SiteID != null)
				cmdCostStatus.WhereAnd<Where<INLotSerialCostStatus.siteID, Equal<Current<InventorySummaryEnqFilter.siteID>>>>();

			if (filter.InventoryID != null)
				foreach(INLotSerialCostStatus res in cmdCostStatus.Select())
					cmdCostStatus.Cache.SetStatus(res, PXEntryStatus.Notchanged);

			using (new PXFieldScope(cmd_lss.View,
				typeof(INLotSerialStatus),
				typeof(INSite.siteCD),
				typeof(INSubItem.subItemCD),
				typeof(INLocation.locationCD),
				typeof(INLocation.inclQtyAvail),
				typeof(InventoryItem.baseUnit),
				typeof(INLocationCostStatus),
				typeof(INSiteCostStatus)))
			{
				foreach (PXResult<INLotSerialStatus, INLocation, InventoryItem, INSite, INSubItem> res in cmd_lss.Select())
				{
					INLotSerialStatus lss_rec = res;
					INLocation loc_rec = res;
					InventoryItem item_rec = res;

					InventorySummaryEnquiryResult ret = Copy<InventorySummaryEnquiryResult>(lss_rec);
					ret.InventoryID = lss_rec.InventoryID;
					ret.SubItemID = lss_rec.SubItemID;
					ret.SiteID = lss_rec.SiteID;
					ret.LocationID = lss_rec.LocationID;
					ret.LotSerialNbr = lss_rec.LotSerialNbr;
					ret.ExpireDate = lss_rec.ExpireDate;
					ret.BaseUnit = item_rec.BaseUnit;
					ret.TotalCost = 0m;
					ret.QtyExpired = 0m;
					ret.ControlTimetamp = ControlTimeStamp;

					INLotSerialCostStatus lscs_rec = (INLotSerialCostStatus)cmdCostStatus.Cache.Locate(new INLotSerialCostStatus()
					{
						InventoryID = lss_rec.InventoryID,
						SubItemID = lss_rec.SubItemID,
						SiteID = lss_rec.SiteID,
						LotSerialNbr = lss_rec.LotSerialNbr
					});

					INLocationCostStatus lcs_rec = PXResult.Unwrap<INLocationCostStatus>(res);
					INSiteCostStatus scs_rec = PXResult.Unwrap<INSiteCostStatus>(res);

					if (lscs_rec?.InventoryID != null)
					{
						ret.UnitCost = lscs_rec.UnitCost;
						ret.TotalCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(ret.QtyOnHand * lscs_rec.UnitCost));
					}
					else if (lcs_rec.InventoryID != null)
					{
						ret.UnitCost = lcs_rec.UnitCost;
						ret.TotalCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(ret.QtyOnHand * lcs_rec.UnitCost));
					}
					else if (scs_rec.InventoryID != null)
					{
						ret.UnitCost = scs_rec.UnitCost;
						ret.TotalCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(ret.QtyOnHand * scs_rec.UnitCost));
					}

					if (loc_rec.InclQtyAvail == false)
					{
						ret.QtyNotAvail = ret.QtyAvail;
						ret.QtyAvail = 0m;
						ret.QtyHardAvail = 0m;
						ret.QtyActual = 0m;
					}
					else
					{
						ret.QtyNotAvail = 0m;
					}

					ret.ExpireDate = lss_rec.ExpireDate;
					if (lss_rec.ExpireDate != null && DateTime.Compare((DateTime)this.Accessinfo.BusinessDate, (DateTime)lss_rec.ExpireDate) > 0)
					{
						ret.QtyExpired = lss_rec.QtyOnHand;
					}

					if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
					{
						PXCache cache = this.Caches[typeof(LocationStatus)];
						LocationStatus aggregate = new LocationStatus();
						aggregate.InventoryID = ret.InventoryID;
						aggregate.SubItemID = ret.SubItemID;
						aggregate.SiteID = ret.SiteID;
						aggregate.LocationID = ret.LocationID;

						aggregate = (LocationStatus)cache.Insert(aggregate);
						Add(aggregate, ret);
					}
					else
					{
						PXCache cache = this.Caches[typeof(SiteStatus)];
						SiteStatus aggregate = new SiteStatus();
						aggregate.InventoryID = ret.InventoryID;
						aggregate.SubItemID = ret.SubItemID;
						aggregate.SiteID = ret.SiteID;

						aggregate = (SiteStatus)cache.Insert(aggregate);
						Add(aggregate, ret);
					}

					if (filter.ExpandByLotSerialNbr == true)
					{
						ret.QtyAvail -= ret.QtyExpired;
						if (!ret.IsZero()) resultset.Add((ret, res, res, loc_rec));
					}
				}
			}

			PXSelectBase<INLocationStatus> cmd_ls = new PXSelectReadonly2<INLocationStatus,
				InnerJoin<INLocation,
					On<INLocationStatus.FK.Location>,
				InnerJoin<InventoryItem,
					On<InventoryItem.inventoryID, Equal<INLocationStatus.inventoryID>,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
				InnerJoin<INSite,
					On2<INLocationStatus.FK.Site,
					And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>,
				InnerJoin<INSubItem,
					On<INLocationStatus.FK.SubItem>,
				LeftJoin<INLotSerClass,
					On<InventoryItem.FK.LotSerClass>,
				LeftJoin<INLocationCostStatus,
					On<INLocationCostStatus.inventoryID, Equal<INLocationStatus.inventoryID>,
						And<INLocationCostStatus.subItemID, Equal<INLocationStatus.subItemID>,
						And<INLocationCostStatus.locationID, Equal<INLocationStatus.locationID>>>>,
				LeftJoin<INSiteCostStatus,
					On<INSiteCostStatus.inventoryID, Equal<INLocationStatus.inventoryID>,
						And<INSiteCostStatus.subItemID, Equal<INLocationStatus.subItemID>,
						And<INSiteCostStatus.siteID, Equal<INLocationStatus.siteID>>>>>>>>>>>,
				Where<INLocationStatus.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>(this);

			AppendFilters<INLocationStatus>(cmd_ls, filter);

			foreach (PXResult<INLocationStatus, INLocation, InventoryItem, INSite, INSubItem> res in cmd_ls.Select())
			{
				INLocationStatus ls_rec = res;
				INLocation loc_rec = res;
				InventoryItem item_rec = res;

				InventorySummaryEnquiryResult ret = Copy<InventorySummaryEnquiryResult>(ls_rec);
				ret.InventoryID = ls_rec.InventoryID;
				ret.SubItemID = ls_rec.SubItemID;
				ret.SiteID = ls_rec.SiteID;
				ret.LocationID = ls_rec.LocationID;
				ret.BaseUnit = item_rec.BaseUnit;
				ret.UnitCost = ret.UnitCost ?? 0m;
				ret.TotalCost = 0m;
				ret.QtyExpired = 0m;
				ret.ControlTimetamp = ControlTimeStamp;

				INLocationCostStatus lcs_rec = PXResult.Unwrap<INLocationCostStatus>(res);
				INSiteCostStatus scs_rec = PXResult.Unwrap<INSiteCostStatus>(res);
				if (lcs_rec.InventoryID != null)
				{
					ret.UnitCost = lcs_rec.UnitCost;
					ret.TotalCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(ret.QtyOnHand * lcs_rec.UnitCost));
				}
				else if (scs_rec.InventoryID != null)
				{
					ret.UnitCost = scs_rec.UnitCost;
					ret.TotalCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(ret.QtyOnHand * scs_rec.UnitCost));
				}

				if (loc_rec.InclQtyAvail == false)
				{
					ret.QtyNotAvail = ret.QtyAvail;
					ret.QtyAvail = 0m;
					ret.QtyHardAvail = 0m;
					ret.QtyActual = 0m;
				}
				else
				{
					ret.QtyNotAvail = 0m;
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
				{
					PXCache cache = this.Caches[typeof(SiteStatus)];
					SiteStatus aggregate = new SiteStatus();
					aggregate.InventoryID = ret.InventoryID;
					aggregate.SubItemID = ret.SubItemID;
					aggregate.SiteID = ret.SiteID;

					aggregate = (SiteStatus)cache.Insert(aggregate);
					Add(aggregate, ret);
				}

				{
					PXCache cache = this.Caches[typeof(LocationStatus)];
					LocationStatus aggregate = new LocationStatus();
					aggregate.InventoryID = ret.InventoryID;
					aggregate.SubItemID = ret.SubItemID;
					aggregate.SiteID = ret.SiteID;
					aggregate.LocationID = ret.LocationID;

					aggregate = (LocationStatus)cache.Insert(aggregate);
					if (filter.ExpandByLotSerialNbr == true)
					{
						ret = Subtract<InventorySummaryEnquiryResult>(ret, aggregate);

						if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
						{
							if (!ret.IsZero()) resultset.Add((ret, res, res, loc_rec));
						}
						continue;
					}
					else if (aggregate.TotalCost != 0m)
					{
						ret.TotalCost = aggregate.TotalCost;
						ret.UnitCost = RoundUnitCost(aggregate.QtyOnHand != 0m ? aggregate.TotalCost / aggregate.QtyOnHand : 0m);
					}
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
				{
					if (!ret.IsZero()) resultset.Add((ret, res, res, loc_rec));
				}
			}


			if (filter.ExpandByLotSerialNbr == true)
			{
                var shippingPlanTypes = PXSelectReadonly<INPlanType, Where<INPlanType.inclQtySOShipping, Equal<True>>>.SelectMultiBound(this, null).AsEnumerable();
				PXSelectBase<INItemPlan> cmd_plans = new PXSelectReadonly2<INItemPlan,
				InnerJoin<InventoryItem,
					On2<INItemPlan.FK.InventoryItem,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
				InnerJoin<INSite,
					On2<INItemPlan.FK.Site,
					And<Match<INSite, Current<AccessInfo.userName>>>>,
				InnerJoin<INSubItem,
                    On<INItemPlan.FK.SubItem>,
				InnerJoin<INPlanType,
                    On<INItemPlan.FK.PlanType>>>>>,
				Where<
				INPlanType.inclQtySOShipping, Equal<decimal1>,
				And<INItemPlan.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>>(this);

				AppendFilters<INItemPlan>(cmd_plans, filter);

				List<PXResult<INItemPlan, InventoryItem, INSite, INSubItem>> lstPlans = new List<PXResult<INItemPlan, InventoryItem, INSite, INSubItem>>();

				foreach (PXResult<INItemPlan, InventoryItem, INSite, INSubItem> res in cmd_plans.Select())
				{
					lstPlans.Add(res);
				}

				for (int i = 0; i < lstPlans.Count; i++)
				{
					INItemPlan plan_rec = lstPlans[i];

					if (shippingPlanTypes.Any(x => ((INPlanType)x).PlanType == plan_rec.OrigPlanType))
					{
						for (int j = 0; j < lstPlans.Count; j++)
						{
							INItemPlan origplan_rec = lstPlans[j];

							if (origplan_rec.PlanID == plan_rec.OrigPlanID
								||
								(origplan_rec.RefNoteID == plan_rec.OrigNoteID &&
								origplan_rec.PlanType == plan_rec.OrigPlanType &&
								origplan_rec.Reverse == plan_rec.Reverse &&
								origplan_rec.SubItemID == plan_rec.SubItemID &&
								origplan_rec.SiteID == plan_rec.SiteID &&
								origplan_rec.LocationID == null &&
								origplan_rec.LotSerialNbr == plan_rec.LotSerialNbr))
							{
								origplan_rec.PlanQty -= plan_rec.PlanQty;
								plan_rec.PlanQty = 0m;
								break;
							}
						}
					}
				}

				foreach (PXResult<INItemPlan, InventoryItem, INSite, INSubItem> res in lstPlans)
				{
					INItemPlan plan_rec = res;
					InventoryItem item_rec = res;

					if (plan_rec.PlanQty == 0m || String.IsNullOrEmpty(plan_rec.LotSerialNbr) || plan_rec.LocationID != null)
						continue;
					var ss_rec = new INSiteStatus();
					ss_rec.SiteID = plan_rec.SiteID;
					ss_rec.InventoryID = item_rec.InventoryID;
					ss_rec.SubItemID = plan_rec.SubItemID;

					InventorySummaryEnquiryResult ret = Copy<InventorySummaryEnquiryResult>(ss_rec);
					ret.LotSerialNbr = plan_rec.LotSerialNbr;
					ret.InventoryID = ss_rec.InventoryID;
					ret.SubItemID = ss_rec.SubItemID;
					ret.SiteID = ss_rec.SiteID;
					ret.BaseUnit = item_rec.BaseUnit;
					ret.UnitCost = ret.UnitCost ?? 0m;
					ret.TotalCost = 0m;
					ret.QtyExpired = 0m;
					ret.ControlTimetamp = ControlTimeStamp;

					PXCache cache = this.Caches[typeof(SiteStatus)];
					SiteStatus aggregate = new SiteStatus();
					aggregate.InventoryID = ret.InventoryID;
					aggregate.SubItemID = ret.SubItemID;
					aggregate.SiteID = ret.SiteID;

					aggregate = (SiteStatus)cache.Insert(aggregate);
					var origaggregate = (SiteStatus)cache.CreateCopy(aggregate);

					INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<SiteStatus>(this, aggregate, plan_rec, shippingPlanTypes.First(x => ((INPlanType)x).PlanType == plan_rec.PlanType), aggregate.InclQtyAvail.GetValueOrDefault());

					Add(ret, aggregate);
					ret = Subtract<InventorySummaryEnquiryResult>(ret, origaggregate);

                    if (!ret.IsZero())
					{
						resultset.Add((ret, res, res, INLocation.PK.Find(this, ret.LocationID)));
					}

				}
			}

			PXSelectBase<INSiteStatus> cmd_ss = new PXSelectReadonly2<INSiteStatus,
				InnerJoin<InventoryItem,
					On<InventoryItem.inventoryID, Equal<INSiteStatus.inventoryID>,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
				InnerJoin<INSite,
					On2<INSiteStatus.FK.Site,
					And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>,
				InnerJoin<INSubItem,
					On<INSiteStatus.FK.SubItem>>>>,
				Where<INSiteStatus.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>(this);

			AppendFilters<INSiteStatus>(cmd_ss, filter);

			foreach (PXResult<INSiteStatus, InventoryItem, INSite, INSubItem> res in cmd_ss.Select())
			{
				INSiteStatus ss_rec = res;
				InventoryItem item_rec = res;

				InventorySummaryEnquiryResult ret = Copy<InventorySummaryEnquiryResult>(ss_rec);
				ret.InventoryID = ss_rec.InventoryID;
				ret.SubItemID = ss_rec.SubItemID;
				ret.SiteID = ss_rec.SiteID;
				ret.BaseUnit = item_rec.BaseUnit;
				ret.UnitCost = ret.UnitCost ?? 0m;
				ret.TotalCost = 0m;
				ret.QtyExpired = 0m;
				ret.ControlTimetamp = ControlTimeStamp;

				if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() || filter.ExpandByLotSerialNbr == true)
				{
					PXCache cache = this.Caches[typeof(SiteStatus)];
					SiteStatus aggregate = new SiteStatus();
					aggregate.InventoryID = ret.InventoryID;
					aggregate.SubItemID = ret.SubItemID;
					aggregate.SiteID = ret.SiteID;

					aggregate = (SiteStatus)cache.Insert(aggregate);

					ret = Subtract<InventorySummaryEnquiryResult>(ret, aggregate);
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() || filter.LocationID == null)
				{
					if (!ret.IsZero())
					{
						resultset.Add((ret, res, res, null));
					}

				}
			}

			return resultset
				.OrderBy(x => x.SubItem?.SubItemCD)
				.ThenBy(x => x.Site?.SiteCD)
				.ThenBy(x => x.Location?.LocationCD)
				.ThenBy(x => x.Result.LotSerialNbr)
				.Select(x => x.Result);
		}

		protected virtual IEnumerable SortSummaryResult(IEnumerable<InventorySummaryEnquiryResult> resultset,
			InventorySummaryEnquiryResult total)
		{
			var delegateResult = new PXDelegateResult() { IsResultSorted = true };

			var sortedResultset = PXView.Sort(resultset).RowCast<InventorySummaryEnquiryResult>();

			if (resultset.Any())
			{
				if (!PXView.ReverseOrder)
				{
					delegateResult.AddRange(sortedResultset);
					delegateResult.Add(total);
				}
				else
				{
					delegateResult.Add(total);
					delegateResult.AddRange(sortedResultset);
				}
			}

			return delegateResult;
		}

		protected virtual InventorySummaryEnquiryResult CalculateSummaryTotal(IEnumerable<InventorySummaryEnquiryResult> resultset)
		{
			InventorySummaryEnquiryResult total = resultset.CalculateSumTotal(ISERecords.Cache);
			total.IsTotal = true;
			total.LocationID = InventorySummaryEnquiryResult.TotalLocationID;
			total.SiteID = null;
			total.UnitCost = null;
			return total;
		}

		public override bool IsDirty => false;

		
		[PXUIField(DisplayName = "")]
		[PXEditDetailButton]
		protected virtual IEnumerable ViewAllocDet(PXAdapter a)
		{
			if (this.ISERecords.Current != null)
			{
				object subItem =
						this.ISERecords.Cache.GetValueExt<InventorySummaryEnquiryResult.subItemID>
					(this.ISERecords.Current);

				if (subItem is PXSegmentedState)
					subItem = ((PXSegmentedState)subItem).Value;

				InventoryAllocDetEnq.Redirect(
					this.ISERecords.Current.InventoryID,
					subItem != null ? (string)subItem : null,
					this.Filter.Current.ExpandByLotSerialNbr == true ?
					this.ISERecords.Current.LotSerialNbr : null,
					this.ISERecords.Current.SiteID,
					this.ISERecords.Current.LocationID);
			}
			return a.Get();
		}

		public PXAction<InventorySummaryEnqFilter> viewItem;
		[PXButton()]
		[PXUIField(DisplayName = "")]
		protected virtual IEnumerable ViewItem(PXAdapter a)
		{
			if (this.ISERecords.Current != null)
				InventoryItemMaint.Redirect(this.ISERecords.Current.InventoryID, true);
			return a.Get();
		}

		public static void Redirect(int? inventoryID, string subItemCD, int? siteID, int? locationID)
		{
			Redirect(inventoryID, subItemCD, siteID, locationID, true);
		}

		public static void Redirect(int? inventoryID, string subItemCD, int? siteID, int? locationID, bool newWindow)
		{
			InventorySummaryEnq graph = PXGraph.CreateInstance<InventorySummaryEnq>();
			graph.Filter.Current.InventoryID = inventoryID;
			graph.Filter.Current.SubItemCD = subItemCD;
			graph.Filter.Current.SiteID = siteID;
			graph.Filter.Current.LocationID = locationID;

			InventorySummaryEnqFilter filter = new InventorySummaryEnqFilter();
			if (newWindow)
				throw new PXRedirectRequiredException(graph, true, Messages.InventorySummary) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			else
				throw new PXRedirectRequiredException(graph, Messages.InventorySummary);
			//throw new PXPopupRedirectException(graph, Messages.InventorySummary, false, false);
		}


		#region Aging strategy
		private bool timestampSelected = false;
		private String ControlTimeStamp
		{
			get
			{
				if (!timestampSelected)
				{
					PXDatabase.SelectTimeStamp();
					timestampSelected = true;
				}
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					PXContext.SetSlot<Definition>(
					defs = PXDatabase.GetSlot<Definition>(nameof(InventorySummaryEnq) + "$ControlTimeStampDefinition",
						new Type[] /// <see cref="iSERecordsFetch"/> for proper tables
						{
							typeof(InventoryItem),
							typeof(INSubItem),
							typeof(INItemPlan),

							typeof(INSite),
							typeof(INSiteStatus),
							typeof(INSiteCostStatus),

							typeof(INLocation),
							typeof(INLocationStatus),
							typeof(INLocationCostStatus),

							typeof(INLotSerClass),
							typeof(INLotSerialStatus),
							typeof(INItemLotSerial),
							typeof(INCostStatus),
							typeof(INCostSubItemXRef),
						})
					);
				}
				return defs.TimeStamp;
			}
		}
		public class Definition : IPrefetchable
		{
			public String TimeStamp { get; private set; }

			public void Prefetch()
			{
				TimeStamp = System.Text.Encoding.Default.GetString(PXDatabase.Provider.SelectTimeStamp());
			}
		} 
		#endregion
	}
}