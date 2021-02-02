using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
    [System.SerializableAttribute()]
	[PXCacheName(Messages.INSiteLotSerial)]
    public partial class INSiteLotSerial : PX.Data.IBqlTable
    {
		#region Keys
		public class PK : PrimaryKeyOf<INSiteLotSerial>.By<inventoryID, siteID, lotSerialNbr>
		{
			public static INSiteLotSerial Find(PXGraph graph, int? inventoryID, int? siteID, string lotSerialNbr)
				=> FindBy(graph, inventoryID, siteID, lotSerialNbr);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INSiteLotSerial>.By<siteID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INSiteLotSerial>.By<inventoryID> { }
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [IN.Site(IsKey = true, TabOrder = 2)]
        [PXDefault()]
        [PXParent(typeof(FK.Site))]
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
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        protected String _LotSerialNbr;
        [PXDefault()]
		[LotSerialNbr(IsKey = true)]
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
        #region ExpireDate
        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        protected DateTime? _ExpireDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Expiry Date")]
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
        #region UpdateExpireDate
        protected bool? _UpdateExpireDate;
        [PXBool()]
        public virtual bool? UpdateExpireDate
        {
            get
            {
                return this._UpdateExpireDate;
            }
            set
            {
                this._UpdateExpireDate = value;
            }
        }
        #endregion
        #region LotSerTrack
        public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
        protected String _LotSerTrack;
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public virtual String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion
        #region LotSerAssign
        public abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }
        protected String _LotSerAssign;
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        public virtual String LotSerAssign
        {
            get
            {
                return this._LotSerAssign;
            }
            set
            {
                this._LotSerAssign = value;
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
