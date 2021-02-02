namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.Common.Discount;

	/// <summary>
	/// Represents an Accounts Receivable discount code used to define
	/// <see cref="DiscountSequence">discount sequences</see> on the
	/// Discounts (AR209500) form. The primary function of a discount
	/// code is to specify the type and applicability of the discount.
	/// For example, a document discount can be applied to specific customers,
	/// or a line discount can be applied to specific inventory items. The
	/// entities of this type are edited on the Discount Codes (AR209000)
	/// form, which corresponds to the <see cref="ARDiscountMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph( new Type[] { typeof(ARDiscountSequenceMaint) },
					 new Type[] { typeof(Select<DiscountSequence, Where<DiscountSequence.discountID, Equal<Current<ARDiscount.discountID>>>>)}
					 )]
	[PXCacheName(Messages.ARDiscount)]
	public partial class ARDiscount : PX.Data.IBqlTable
	{
		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		protected String _DiscountID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName="Discount Code", Visibility=PXUIVisibility.SelectorVisible)]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(250, IsUnicode = true)]
		[PXUIField(DisplayName="Description", Visibility=PXUIVisibility.SelectorVisible)]
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
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(DiscountType.Line)]
		[DiscountType.List()]
		[PXUIField(DisplayName="Discount Type", Visibility=PXUIVisibility.SelectorVisible)]
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
        #region ApplicableTo
        public abstract class applicableTo : PX.Data.BQL.BqlString.Field<applicableTo> { }
		protected String _ApplicableTo;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(DiscountTarget.Inventory)]
		[PXUIField(DisplayName = "Applicable To", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ApplicableTo
		{
			get
			{
				return this._ApplicableTo;
			}
			set
			{
				this._ApplicableTo = value;
			}
		}
		#endregion

		#region IsAppliedToDR
		public abstract class isAppliedToDR : PX.Data.BQL.BqlBool.Field<isAppliedToDR> { }

		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "Apply to Deferred Revenue")]
		public bool? IsAppliedToDR { get; set; }
		#endregion

		#region IsManual
		public abstract class isManual : PX.Data.BQL.BqlBool.Field<isManual> { }
        protected Boolean? _IsManual;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual", Visibility = PXUIVisibility.Visible)]
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

        #region ExcludeFromDiscountableAmt
        public abstract class excludeFromDiscountableAmt : PX.Data.BQL.BqlBool.Field<excludeFromDiscountableAmt> { }
        protected Boolean? _ExcludeFromDiscountableAmt;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Exclude From Discountable Amount", Visibility = PXUIVisibility.Visible)]
        public virtual Boolean? ExcludeFromDiscountableAmt
        {
            get
            {
                return this._ExcludeFromDiscountableAmt;
            }
            set
            {
                this._ExcludeFromDiscountableAmt = value;
            }
        }
        #endregion

        #region SkipDocumentDiscounts
        public abstract class skipDocumentDiscounts : PX.Data.BQL.BqlBool.Field<skipDocumentDiscounts> { }
        protected Boolean? _SkipDocumentDiscounts;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Skip Document Discounts", Visibility = PXUIVisibility.Visible)]
        public virtual Boolean? SkipDocumentDiscounts
        {
            get
            {
                return this._SkipDocumentDiscounts;
            }
            set
            {
                this._SkipDocumentDiscounts = value;
            }
        }
        #endregion

		#region IsAutoNumber
		public abstract class isAutoNumber : PX.Data.BQL.BqlBool.Field<isAutoNumber> { }
		protected Boolean? _IsAutoNumber;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Auto-Numbering", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? IsAutoNumber
		{
			get
			{
				return this._IsAutoNumber;
			}
			set
			{
				this._IsAutoNumber = value;
			}
		}
		#endregion
		#region LastNumber
		public abstract class lastNumber : PX.Data.BQL.BqlString.Field<lastNumber> { }
		protected String _LastNumber;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Number", Visibility = PXUIVisibility.Visible)]
		public virtual String LastNumber
		{
			get
			{
				return this._LastNumber;
			}
			set
			{
				this._LastNumber = value;
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
	}
}
