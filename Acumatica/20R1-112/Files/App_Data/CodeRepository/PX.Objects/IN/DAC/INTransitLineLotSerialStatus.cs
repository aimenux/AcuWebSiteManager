using System;
using PX.Data;

namespace PX.Objects.IN
{
    [System.SerializableAttribute()]
    [PXProjection(typeof(Select2<INLotSerialStatus2, InnerJoin<INTransitLine, On<INTransitLine.costSiteID, Equal<INLotSerialStatus2.locationID>>>>), Persistent = false)]
    public partial class INTransitLineLotSerialStatus : PX.Data.IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [StockItem(BqlField = typeof(INLotSerialStatus2.inventoryID))]
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
        [SubItem(BqlField = typeof(INLotSerialStatus2.subItemID))]
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
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr>
		{
            public const int LENGTH = 100;
        }
        protected String _LotSerialNbr;
        [PXDBString(100, IsUnicode = true, BqlField = typeof(INLotSerialStatus2.lotSerialNbr))]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [Site(BqlField = typeof(INLotSerialStatus2.siteID))]
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

        #region CostSiteID
        public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        protected Int32? _CostSiteID;
        [PXDBInt(BqlField = typeof(INTransitLine.costSiteID))]
        public virtual Int32? CostSiteID
        {
            get
            {
                return this._CostSiteID;
            }
            set
            {
                this._CostSiteID = value;
            }
        }
        #endregion

        #region FromSiteID
        public abstract class fromSiteID : PX.Data.BQL.BqlInt.Field<fromSiteID> { }
        protected Int32? _FromSiteID;
        [IN.Site(DisplayName = "From Warehouse ID", DescriptionField = typeof(INSite.descr), BqlField = typeof(INTransitLine.siteID))]
        public virtual Int32? FromSiteID
        {
            get
            {
                return this._FromSiteID;
            }
            set
            {
                this._FromSiteID = value;
            }
        }
        #endregion
        #region ToSiteID
        public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
        protected Int32? _ToSiteID;
        [IN.ToSite(DisplayName = "To Warehouse ID", DescriptionField = typeof(INSite.descr), BqlField = typeof(INTransitLine.toSiteID))]
        public virtual Int32? ToSiteID
        {
            get
            {
                return this._ToSiteID;
            }
            set
            {
                this._ToSiteID = value;
            }
        }
        #endregion

        #region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule>
		{
			public const string PI = "PI";

			public class List : PXStringListAttribute
			{
				public List() : base(
					new[]
					{
						Pair(GL.BatchModule.SO, GL.Messages.ModuleSO),
						Pair(GL.BatchModule.PO, GL.Messages.ModulePO),
						Pair(GL.BatchModule.IN, GL.Messages.ModuleIN),
						Pair(PI, Messages.ModulePI),
						Pair(GL.BatchModule.AP, GL.Messages.ModuleAP),
					}) {}
			}
		}
        protected String _OrigModule;
        [PXDBString(2, IsFixed = true, BqlField = typeof(INTransitLine.origModule))]
        [PXDefault(GL.BatchModule.IN)]
        [PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [origModule.List]
        public virtual String OrigModule
        {
            get
            {
                return this._OrigModule;
            }
            set
            {
                this._OrigModule = value;
            }
        }
        #endregion

        #region ToLocationID
        public abstract class toLocationID : PX.Data.BQL.BqlInt.Field<toLocationID> { }
        protected Int32? _ToLocationID;
        [IN.Location(DisplayName = "To Location ID", BqlField = typeof(INTransitLine.toLocationID))]
        public virtual Int32? ToLocationID
        {
            get
            {
                return this._ToLocationID;
            }
            set
            {
                this._ToLocationID = value;
            }
        }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote(BqlField = typeof(INTransitLine.noteID))]
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

        #region TransferNbr
        public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
        protected String _TransferNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(INTransitLine.transferNbr), IsKey = true)]
        public virtual String TransferNbr
        {
            get
            {
                return this._TransferNbr;
            }
            set
            {
                this._TransferNbr = value;
            }
        }
        #endregion
        #region TransferLineNbr
        public abstract class transferLineNbr : PX.Data.BQL.BqlInt.Field<transferLineNbr> { }
        protected Int32? _TransferLineNbr;

        [PXDBInt(BqlField = typeof(INTransitLine.transferLineNbr), IsKey = true)]
        public virtual Int32? TransferLineNbr
        {
            get
            {
                return this._TransferLineNbr;
            }
            set
            {
                this._TransferLineNbr = value;
            }
        }
        #endregion

        #region SOOrderType
        public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
        protected String _SOOrderType;
        [PXDBString(2, IsFixed = true, BqlField = typeof(INTransitLine.sOOrderType))]
        public virtual String SOOrderType
        {
            get
            {
                return this._SOOrderType;
            }
            set
            {
                this._SOOrderType = value;
            }
        }
        #endregion
        #region SOOrderNbr
        public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
        protected String _SOOrderNbr;
        [PXDBString(15, InputMask = "", IsUnicode = true, BqlField = typeof(INTransitLine.sOOrderNbr))]
        public virtual String SOOrderNbr
        {
            get
            {
                return this._SOOrderNbr;
            }
            set
            {
                this._SOOrderNbr = value;
            }
        }
        #endregion

        #region SOOrderLineNbr
        public abstract class sOOrderLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
        protected Int32? _SOOrderLineNbr;

        [PXDBInt(BqlField = typeof(INTransitLine.sOOrderLineNbr))]
        public virtual Int32? SOOrderLineNbr
        {
            get
            {
                return this._SOOrderLineNbr;
            }
            set
            {
                this._SOOrderLineNbr = value;
            }
        }
        #endregion

        #region SOShipmentType
        public abstract class sOShipmentType : PX.Data.BQL.BqlString.Field<sOShipmentType> { }
        protected String _SOShipmentType;
        [PXDBString(1, IsFixed = true, BqlField = typeof(INTransitLine.sOShipmentType))]
        public virtual String SOShipmentType
        {
            get
            {
                return this._SOShipmentType;
            }
            set
            {
                this._SOShipmentType = value;
            }
        }
        #endregion
        #region SOShipmentNbr
        public abstract class sOShipmentNbr : PX.Data.BQL.BqlString.Field<sOShipmentNbr> { }
        protected String _SOShipmentNbr;
        [PXDBString(15, InputMask = "", IsUnicode = true, BqlField = typeof(INTransitLine.sOShipmentNbr))]
        public virtual String SOShipmentNbr
        {
            get
            {
                return this._SOShipmentNbr;
            }
            set
            {
                this._SOShipmentNbr = value;
            }
        }
        #endregion

        #region SOShipmentLineNbr
        public abstract class sOShipmentLineNbr : PX.Data.BQL.BqlInt.Field<sOShipmentLineNbr> { }
        protected Int32? _SOShipmentLineNbr;

        [PXDBInt(BqlField = typeof(INTransitLine.sOShipmentLineNbr))]
        public virtual Int32? SOShipmentLineNbr
        {
            get
            {
                return this._SOShipmentLineNbr;
            }
            set
            {
                this._SOShipmentLineNbr = value;
            }
        }
        #endregion

        #region QtyOnHand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        protected Decimal? _QtyOnHand;
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyOnHand))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyAvail))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyHardAvail))]
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
        #region QtyInTransit
        public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
        protected Decimal? _QtyInTransit;
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyInTransit))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyInTransitToSO))]
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
        #region QtyPOPrepared
        public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
        protected Decimal? _QtyPOPrepared;
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPOPrepared))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPOOrders))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPOReceipts))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        #region QtySOBackOrdered
        public abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
        protected Decimal? _QtySOBackOrdered;
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtySOBackOrdered))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtySOPrepared))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtySOBooked))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtySOShipped))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtySOShipping))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyINIssues))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyINReceipts))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyINAssemblyDemand))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyINAssemblySupply))]
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
        #region QtySOFixed
        public abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
        protected decimal? _QtySOFixed;
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtySOFixed))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPOFixedOrders))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPOFixedPrepared))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPOFixedReceipts))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtySODropShip))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPODropShipOrders))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPODropShipPrepared))]
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
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyPODropShipReceipts))]
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

        #region QtyNotAvail
        public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
        protected decimal? _QtyNotAvail;
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyNotAvail))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyNotAvail
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
        protected decimal? _QtyExpired;
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus2.qtyExpired))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyExpired
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
    }
}

