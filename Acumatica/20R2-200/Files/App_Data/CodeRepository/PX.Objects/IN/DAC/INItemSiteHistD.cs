using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;

namespace PX.Objects.IN
{
    [Serializable]
    [PXHidden]
    public partial class INItemSiteHistD : PX.Data.IBqlTable
    {
	    #region Keys
	    public class PK : PrimaryKeyOf<INItemSiteHistD>.By<inventoryID, siteID, subItemID, sDate>
	    {
		    public static INItemSiteHistD Find(PXGraph graph, int? inventoryID, int? siteID, int? subItemID, DateTime? sDate)
			    => FindBy(graph, inventoryID, siteID, subItemID, sDate);
	    }
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemSiteHistD>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemSiteHistD>.By<siteID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INItemSiteHistD>.By<subItemID> { }
			public class ItemSiteReplenishment : INItemSiteReplenishment.PK.ForeignKeyOf<INItemSiteHistD>.By<inventoryID, siteID, subItemID> { }
		}
	    #endregion
		#region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [StockItem(IsKey = true)]
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
        #region SDate
        public abstract class sDate : PX.Data.BQL.BqlDateTime.Field<sDate> { }
        protected DateTime? _SDate;
        [PXDBDate(IsKey = true)]
        public virtual DateTime? SDate
        {
            get
            {
                return this._SDate;
            }
            set
            {
                this._SDate = value;
            }
        }
        #endregion
        #region SYear
        public abstract class sYear : PX.Data.BQL.BqlInt.Field<sYear> { }
        protected int? _SYear;
        [PXDBInt]
        public virtual int? SYear
        {
            get
            {
                return this._SYear;
            }
            set
            {
                this._SYear = value;
            }
        }
        #endregion
        #region SMonth
        public abstract class sMonth : PX.Data.BQL.BqlInt.Field<sMonth> { }
        protected int? _SMonth;
        [PXDBInt]
        public virtual int? SMonth
        {
            get
            {
                return this._SMonth;
            }
            set
            {
                this._SMonth = value;
            }
        }
        #endregion
        #region SQuater
        public abstract class sQuater : PX.Data.BQL.BqlInt.Field<sQuater> { }
        protected int? _SQuater;
        [PXDBInt]
        public virtual int? SQuater
        {
            get
            {
                return this._SQuater;
            }
            set
            {
                this._SQuater = value;
            }
        }
        #endregion
        #region SDay
        public abstract class sDay : PX.Data.BQL.BqlInt.Field<sDay> { }
        protected int? _SDay;
        [PXDBInt]
        public virtual int? SDay
        {
            get
            {
                return this._SDay;
            }
            set
            {
                this._SDay = value;
            }
        }
        #endregion
        #region SDayOfWeek
        public abstract class sDayOfWeek : PX.Data.BQL.BqlInt.Field<sDayOfWeek> { }
        protected int? _SDayOfWeek;
        [PXDBInt]
        public virtual int? SDayOfWeek
        {
            get
            {
                return this._SDayOfWeek;
            }
            set
            {
                this._SDayOfWeek = value;
            }
        }
        #endregion
        #region QtyReceived
        public abstract class qtyReceived : PX.Data.BQL.BqlDecimal.Field<qtyReceived> { }
        protected Decimal? _QtyReceived;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Received")]
        public virtual Decimal? QtyReceived
        {
            get
            {
                return this._QtyReceived;
            }
            set
            {
                this._QtyReceived = value;
            }
        }
        #endregion
        #region QtyIssued
        public abstract class qtyIssued : PX.Data.BQL.BqlDecimal.Field<qtyIssued> { }
        protected Decimal? _QtyIssued;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Issued")]
        public virtual Decimal? QtyIssued
        {
            get
            {
                return this._QtyIssued;
            }
            set
            {
                this._QtyIssued = value;
            }
        }
        #endregion
        #region QtySales
        public abstract class qtySales : PX.Data.BQL.BqlDecimal.Field<qtySales> { }
        protected Decimal? _QtySales;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Sales")]
        public virtual Decimal? QtySales
        {
            get
            {
                return this._QtySales;
            }
            set
            {
                this._QtySales = value;
            }
        }
        #endregion
        #region QtyCreditMemos
        public abstract class qtyCreditMemos : PX.Data.BQL.BqlDecimal.Field<qtyCreditMemos> { }
        protected Decimal? _QtyCreditMemos;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Credit Memos")]
        public virtual Decimal? QtyCreditMemos
        {
            get
            {
                return this._QtyCreditMemos;
            }
            set
            {
                this._QtyCreditMemos = value;
            }
        }
        #endregion
        #region QtyDropShipSales
        public abstract class qtyDropShipSales : PX.Data.BQL.BqlDecimal.Field<qtyDropShipSales> { }
        protected Decimal? _QtyDropShipSales;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Drop Ship Sales")]
        public virtual Decimal? QtyDropShipSales
        {
            get
            {
                return this._QtyDropShipSales;
            }
            set
            {
                this._QtyDropShipSales = value;
            }
        }
        #endregion
        #region QtyTransferIn
        public abstract class qtyTransferIn : PX.Data.BQL.BqlDecimal.Field<qtyTransferIn> { }
        protected Decimal? _QtyTransferIn;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Transfer In")]
        public virtual Decimal? QtyTransferIn
        {
            get
            {
                return this._QtyTransferIn;
            }
            set
            {
                this._QtyTransferIn = value;
            }
        }
        #endregion
        #region QtyTransferOut
        public abstract class qtyTransferOut : PX.Data.BQL.BqlDecimal.Field<qtyTransferOut> { }
        protected Decimal? _QtyTransferOut;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Transfer Out")]
        public virtual Decimal? QtyTransferOut
        {
            get
            {
                return this._QtyTransferOut;
            }
            set
            {
                this._QtyTransferOut = value;
            }
        }
        #endregion
        #region QtyAssemblyIn
        public abstract class qtyAssemblyIn : PX.Data.BQL.BqlDecimal.Field<qtyAssemblyIn> { }
        protected Decimal? _QtyAssemblyIn;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Assembly In")]
        public virtual Decimal? QtyAssemblyIn
        {
            get
            {
                return this._QtyAssemblyIn;
            }
            set
            {
                this._QtyAssemblyIn = value;
            }
        }
        #endregion
        #region QtyAssemblyOut
        public abstract class qtyAssemblyOut : PX.Data.BQL.BqlDecimal.Field<qtyAssemblyOut> { }
        protected Decimal? _QtyAssemblyOut;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Assembly Out")]
        public virtual Decimal? QtyAssemblyOut
        {
            get
            {
                return this._QtyAssemblyOut;
            }
            set
            {
                this._QtyAssemblyOut = value;
            }
        }
        #endregion
        #region QtyAdjusted
        public abstract class qtyAdjusted : PX.Data.BQL.BqlDecimal.Field<qtyAdjusted> { }
        protected Decimal? _QtyAdjusted;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Adjusted")]
        public virtual Decimal? QtyAdjusted
        {
            get
            {
                return this._QtyAdjusted;
            }
            set
            {
                this._QtyAdjusted = value;
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
    }
}
