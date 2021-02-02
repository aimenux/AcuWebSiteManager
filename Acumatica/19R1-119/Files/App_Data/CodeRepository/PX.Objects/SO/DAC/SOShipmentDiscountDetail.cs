namespace PX.Objects.SO
{
	using System;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.Common.Discount;
	using PX.Data;
    using PX.Objects.IN;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOShipmentDiscountDetail)]
	public partial class SOShipmentDiscountDetail : PX.Data.IBqlTable, IDiscountDetail
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOShipmentDiscountDetail>.By<recordID, orderType, orderNbr, shipmentNbr, type>
		{
			public static SOShipmentDiscountDetail Find(PXGraph graph, int? recordID, string orderType, string orderNbr, string shipmentNbr, string type)
				=> FindBy(graph, recordID, orderType, orderNbr, shipmentNbr, type);
		}
		public static class FK
		{
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOShipmentDiscountDetail>.By<shipmentNbr> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOShipmentDiscountDetail>.By<orderType> { }
			public class Order : SOOrder.PK.ForeignKeyOf<SOShipmentDiscountDetail>.By<orderType, orderNbr> { }
			public class FreeItem : InventoryItem.PK.ForeignKeyOf<SOShipmentDiscountDetail>.By<freeItemID> { }
			public class DiscountSequence : AR.DiscountSequence.PK.ForeignKeyOf<SOShipmentDiscountDetail>.By<discountID, discountSequenceID> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected ushort? _LineNbr;
		[PXDBUShort()]
		[PXLineNbr(typeof(SOShipment))]
		//[PXLineNbr(typeof(SOShipment), ReuseGaps = true)]
		public virtual ushort? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region SkipDiscount
		public abstract class skipDiscount : PX.Data.BQL.BqlBool.Field<skipDiscount> { }
        protected Boolean? _SkipDiscount;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Skip Discount", Enabled = true)]
        public virtual Boolean? SkipDiscount
        {
            get
            {
                return this._SkipDiscount;
            }
            set
            {
                this._SkipDiscount = value;
            }
        }
        #endregion
        #region ShipmentNbr
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		protected String _ShipmentNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(SOShipment.shipmentNbr))]
        [PXParent(typeof(FK.Shipment))]
		[PXUIField(DisplayName = "ShipmentNbr")]
		public virtual String ShipmentNbr
		{
			get
			{
				return this._ShipmentNbr;
			}
			set
			{
				this._ShipmentNbr = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type", Visibility=PXUIVisibility.Visible)]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Visible)]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
        #region DiscountID
        public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
        protected String _DiscountID;
		[PXDBString(10, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Discount ID")]
		[PXForeignReference(typeof(FK.DiscountSequence))]
        public virtual String DiscountID
        {
            get
            {
                return this._DiscountID;
            }
            set
            {
                this._DiscountID = value;
            }
        }
        #endregion
        #region DiscountSequenceID
        public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
        protected String _DiscountSequenceID;
		[PXDBString(10, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Sequence ID")]
        public virtual String DiscountSequenceID
        {
            get
            {
                return this._DiscountSequenceID;
            }
            set
            {
                this._DiscountSequenceID = value;
            }
        }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
        protected String _Type;
        [PXDBString(1, IsKey = true)]
        [PXDefault()]
        [DiscountType.List()]
        [PXUIField(DisplayName = "Type")]
        public virtual String Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
            }
        }
        #endregion

        #region DiscountableQty
        public abstract class discountableQty : PX.Data.BQL.BqlDecimal.Field<discountableQty> { }
        protected Decimal? _DiscountableQty;
        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Discountable Qty.")]
        public virtual Decimal? DiscountableQty
        {
            get
            {
                return this._DiscountableQty;
            }
            set
            {
                this._DiscountableQty = value;
            }
        }
        #endregion
        #region FreeItemID
        public abstract class freeItemID : PX.Data.BQL.BqlInt.Field<freeItemID> { }
        protected Int32? _FreeItemID;
        [Inventory(DisplayName = "Free Item")]
        [PXForeignReference(typeof(FK.FreeItem))]
        public virtual Int32? FreeItemID
        {
            get
            {
                return this._FreeItemID;
            }
            set
            {
                this._FreeItemID = value;
            }
        }
        #endregion
        #region FreeItemQty
        public abstract class freeItemQty : PX.Data.BQL.BqlDecimal.Field<freeItemQty> { }
        protected Decimal? _FreeItemQty;
        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Free Item Qty.")]
        public virtual Decimal? FreeItemQty
        {
            get
            {
                return this._FreeItemQty;
            }
            set
            {
                this._FreeItemQty = value;
            }
        }
        #endregion
        #region IsManual
        public abstract class isManual : PX.Data.BQL.BqlBool.Field<isManual> { }
        protected Boolean? _IsManual;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount", Enabled = false)]
        public virtual Boolean? IsManual
        {
            get
            {
                return this._IsManual;
            }
            set
            {
                this._IsManual = value;
            }
        }
		#endregion
		#region IsOrigDocDiscount
		public abstract class isOrigDocDiscount : PX.Data.BQL.BqlBool.Field<isOrigDocDiscount> { }
		protected Boolean? _IsOrigDocDiscount;
		[PXBool()]
		[PXFormula(typeof(False))]
		public virtual Boolean? IsOrigDocDiscount
		{
			get
			{
				return this._IsOrigDocDiscount;
			}
			set
			{
				this._IsOrigDocDiscount = value;
			}
		}
		#endregion
		#region ExtDiscCode
		public abstract class extDiscCode : PX.Data.BQL.BqlString.Field<extDiscCode> { }
		protected String _ExtDiscCode;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "External Discount Code")]
		public virtual String ExtDiscCode
		{
			get
			{
				return this._ExtDiscCode;
			}
			set
			{
				this._ExtDiscCode = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		[PXDefault(typeof(Search<PX.Objects.AR.DiscountSequence.description, Where<PX.Objects.AR.DiscountSequence.discountID, Equal<Current<SOShipmentDiscountDetail.discountID>>, And<PX.Objects.AR.DiscountSequence.discountSequenceID, Equal<Current<SOShipmentDiscountDetail.discountSequenceID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion

		#region System Columns
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
        #endregion

        #region IDiscountDetail Members


        public decimal? CuryDiscountableAmt
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        public decimal? CuryDiscountAmt
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        public decimal? DiscountPct
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        #endregion
    }
}
