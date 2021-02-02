namespace PX.Objects.AR
{
    using System;
    using System.Collections;
    using PX.Data;
    using PX.Data.ReferentialIntegrity.Attributes;
    using PX.Objects.IN;
    using PX.Objects.CM;
    using PX.Objects.GL;
    using PX.Objects.SO;
	using PX.Objects.CS;
    using PX.Objects.Common.Discount;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a document or group level discount that has been applied 
	/// to an Accounts Receivable invoice or memo. The records of this type 
	/// can be edited on the Invoices and Memos (AR301000) form, which corresponds 
	/// to the <see cref="ARInvoiceEntry"/> graph.
	/// </summary>
	/// <remarks>
	/// Line-level discounts are specified in the document line itself,
	/// see <see cref="ARTran.DiscPct"/>.
	/// </remarks>
	[System.SerializableAttribute()]
    [PXCacheName(Messages.ARInvoiceDiscountDetail)]
    public partial class ARInvoiceDiscountDetail : PX.Data.IBqlTable, IDiscountDetail
    {
		#region Keys
		public static class FK
		{
			public class DiscountSequence : AR.DiscountSequence.PK.ForeignKeyOf<ARInvoiceDiscountDetail>.By<discountID, discountSequenceID> { }
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
		[PXLineNbr(typeof(ARRegister))]
		//[PXLineNbr(typeof(ARRegister), ReuseGaps = true)]
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
		[PXUIEnabled(typeof(Where<ARInvoiceDiscountDetail.type, NotEqual<DiscountType.ExternalDocumentDiscount>, And<ARInvoiceDiscountDetail.discountID, IsNotNull>>))]
		[PXUIField(DisplayName = "Skip Discount")]
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
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        protected String _DocType;
        [PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARRegister.docType))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, TabOrder = 0)]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        protected String _RefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARRegister.refNbr))]
		[PXParent(typeof(Select<ARRegister, Where<ARRegister.docType, Equal<Current<ARInvoiceDiscountDetail.docType>>, And<ARRegister.refNbr, Equal<Current<ARInvoiceDiscountDetail.refNbr>>>>>))]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
        public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
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
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<ARInvoiceDiscountDetail.orderType>>>>), ValidateValue = false)]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
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
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
		[PXUIEnabled(typeof(Where<ARInvoiceDiscountDetail.orderNbr, IsNull, And<ARInvoiceDiscountDetail.type, NotEqual<DiscountType.ExternalDocumentDiscount>>>))]
		[PXUIField(DisplayName = "Discount Code", Required = false)]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, NotEqual<DiscountType.LineDiscount>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouse>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomer>, 
              And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomerPrice>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventory>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventoryPrice>>>>>>>>))]
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
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
		[PXUIEnabled(typeof(Where<ARInvoiceDiscountDetail.orderNbr, IsNull, And<ARInvoiceDiscountDetail.type, NotEqual<DiscountType.ExternalDocumentDiscount>>>))]
		[PXUIField(DisplayName = "Sequence ID")]
        [PXSelector(typeof(Search<DiscountSequence.discountSequenceID, Where<DiscountSequence.isActive, Equal<True>, And<DiscountSequence.discountID, Equal<Current<ARInvoiceDiscountDetail.discountID>>>>>))]
		[PXForeignReference(typeof(FK.DiscountSequence))]
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
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        [DiscountType.List()]
        [PXUIField(DisplayName = "Type", Enabled = false)]
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
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        protected Int64? _CuryInfoID;
        [PXDBLong()]
        [CurrencyInfo(typeof(ARInvoice.curyInfoID))]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
        #region DiscountableAmt
        public abstract class discountableAmt : PX.Data.BQL.BqlDecimal.Field<discountableAmt> { }
        protected Decimal? _DiscountableAmt;
        [PXDBDecimal(4)]
        public virtual Decimal? DiscountableAmt
        {
            get
            {
                return this._DiscountableAmt;
            }
            set
            {
                this._DiscountableAmt = value;
            }
        }
        #endregion
        #region CuryDiscountableAmt
        public abstract class curyDiscountableAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountableAmt> { }
        protected Decimal? _CuryDiscountableAmt;
        [PXDBCurrency(typeof(ARInvoiceDiscountDetail.curyInfoID), typeof(ARInvoiceDiscountDetail.discountableAmt))]
        [PXUIField(DisplayName = "Discountable Amt.", Enabled = false)]
        public virtual Decimal? CuryDiscountableAmt
        {
            get
            {
                return this._CuryDiscountableAmt;
            }
            set
            {
                this._CuryDiscountableAmt = value;
            }
        }
        #endregion
        #region DiscountableQty
        public abstract class discountableQty : PX.Data.BQL.BqlDecimal.Field<discountableQty> { }
        protected Decimal? _DiscountableQty;
        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Discountable Qty.", Enabled = false)]
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
        #region DiscountAmt
        public abstract class discountAmt : PX.Data.BQL.BqlDecimal.Field<discountAmt> { }
        protected Decimal? _DiscountAmt;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscountAmt
        {
            get
            {
                return this._DiscountAmt;
            }
            set
            {
                this._DiscountAmt = value;
            }
        }
        #endregion
        #region CuryDiscountAmt
        public abstract class curyDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountAmt> { }
        protected Decimal? _CuryDiscountAmt;
        [PXDBCurrency(typeof(ARInvoiceDiscountDetail.curyInfoID), typeof(ARInvoiceDiscountDetail.discountAmt))]
		[PXUIEnabled(typeof(Where<ARInvoiceDiscountDetail.orderNbr, IsNull, And<Where<ARInvoiceDiscountDetail.type, Equal<DiscountType.DocumentDiscount>, Or<ARInvoiceDiscountDetail.type, Equal<DiscountType.ExternalDocumentDiscount>>>>>))]
		[PXUIField(DisplayName = "Discount Amt.")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDiscountAmt
        {
            get
            {
                return this._CuryDiscountAmt;
            }
            set
            {
                this._CuryDiscountAmt = value;
            }
        }
        #endregion
        #region DiscountPct
        public abstract class discountPct : PX.Data.BQL.BqlDecimal.Field<discountPct> { }
        protected Decimal? _DiscountPct;
        [PXDBDecimal(2)]
		[PXUIEnabled(typeof(Where<ARInvoiceDiscountDetail.orderNbr, IsNull, And<Where<ARInvoiceDiscountDetail.type, Equal<DiscountType.DocumentDiscount>, Or<ARInvoiceDiscountDetail.type, Equal<DiscountType.ExternalDocumentDiscount>>>>>))]
		[PXUIField(DisplayName = "Discount Percent")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? DiscountPct
        {
            get
            {
                return this._DiscountPct;
            }
            set
            {
                this._DiscountPct = value;
            }
        }
        #endregion
        #region FreeItemID
        public abstract class freeItemID : PX.Data.BQL.BqlInt.Field<freeItemID> { }
        protected Int32? _FreeItemID;
        [Inventory(DisplayName = "Free Item", Enabled = false)]
        [PXForeignReference(typeof(Field<freeItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
        [PXUIField(DisplayName = "Free Item Qty.", Enabled = false)]
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
		[PXFormula(typeof(Switch<Case<Where<ARInvoiceDiscountDetail.orderNbr, IsNotNull>, True>, False>))]
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
		[PXDefault(typeof(Search<DiscountSequence.description, Where<DiscountSequence.discountID, Equal<Current<ARInvoiceDiscountDetail.discountID>>, And<DiscountSequence.discountSequenceID, Equal<Current<ARInvoiceDiscountDetail.discountSequenceID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region CuryRetainedDiscountAmt
		public abstract class curyRetainedDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedDiscountAmt> { }

		[PXDBCurrency(typeof(ARInvoiceDiscountDetail.curyInfoID), typeof(ARInvoiceDiscountDetail.retainedDiscountAmt))]
		[PXUIField(DisplayName = "Retained Discount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryRetainedDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainedDiscountAmt
		public abstract class retainedDiscountAmt : PX.Data.BQL.BqlDecimal.Field<retainedDiscountAmt> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainedDiscountAmt
		{
			get;
			set;
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

		public class ARInvoiceDiscountDetailComparer : IEqualityComparer<ARInvoiceDiscountDetail>
		{
			public bool Equals(ARInvoiceDiscountDetail discountDetail1, ARInvoiceDiscountDetail discountDetail2)
			{
				return discountDetail1.DiscountID == discountDetail2.DiscountID &&
					discountDetail1.DiscountSequenceID == discountDetail2.DiscountSequenceID &&
					discountDetail1.Type == discountDetail2.Type &&
					discountDetail1.DocType == discountDetail2.DocType &&
					discountDetail1.RefNbr == discountDetail2.RefNbr &&
					discountDetail1.OrderType == discountDetail2.OrderType &&
					discountDetail1.OrderNbr == discountDetail2.OrderNbr;
			}

			public int GetHashCode(ARInvoiceDiscountDetail discountDetail)
			{
				int hashCode = 17;
				hashCode = (hashCode * 11) + discountDetail.DiscountID?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.DiscountSequenceID?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.Type?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.DocType?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.RefNbr?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.OrderType?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.OrderNbr?.GetHashCode() ?? 0;
				return hashCode;
			}
		}
	}
}