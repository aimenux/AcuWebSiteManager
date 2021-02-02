namespace PX.Objects.AP
 {
	 using System;
	 using PX.Data;
	 using PX.Objects.Common.Discount;

	 [System.SerializableAttribute()]
	 [PXCacheName(Messages.APDiscount)]
	 [PXPrimaryGraph(
		new Type[] { typeof(APDiscountSequenceMaint) },
		new Type[] { typeof(Select<VendorDiscountSequence, Where<VendorDiscountSequence.discountID, Equal<Current<discountID>>, And<VendorDiscountSequence.vendorID, Equal<Current<bAccountID>>>>>) }
	 )]
	public partial class APDiscount : PX.Data.IBqlTable
	 {
		 #region BAccountID
		 public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
         protected int? _BAccountID;
         [PXDBInt(IsKey = true)]
		 [PXDBDefault(typeof(Vendor.bAccountID))]
		 [PXParent(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APDiscount.bAccountID>>>>))]
		 public virtual int? BAccountID
		 {
			 get
			 {
				 return this._BAccountID;
			 }
			 set
			 {
				 this._BAccountID = value;
			 }
		 }
		 #endregion
		 #region DiscountID
		 public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		 protected string _DiscountID;
		 [PXDBString(10, IsUnicode = true, IsKey = true ,InputMask = ">aaaaaaaaaa")]
         [PXDefault]
		 [PXUIField(DisplayName = "Discount Code", Visibility = PXUIVisibility.SelectorVisible)]
		 public virtual string DiscountID
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
		 protected string _Description;
		 [PXDBString(250, IsUnicode = true)]
		 [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		 public virtual string Description
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
		 protected string _Type;
		 [PXDBString(1, IsFixed = true)]
		 [PXDefault(DiscountType.Line)]
		 [DiscountType.List()]
		 [PXUIField(DisplayName = "Discount Type", Visibility = PXUIVisibility.SelectorVisible)]
		 public virtual string Type
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
		 protected string _ApplicableTo;
		 [PXDBString(2, IsFixed = true)]
         [PXDefault(DiscountTarget.VendorAndInventory)]
		 [PXUIField(DisplayName = "Applicable To", Visibility = PXUIVisibility.SelectorVisible)]
		 public virtual string ApplicableTo
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
		 #region IsManual
		 public abstract class isManual : PX.Data.BQL.BqlBool.Field<isManual> { }
		 protected bool? _IsManual;
		 [PXDBBool()]
		 [PXDefault(false)]
		 [PXUIField(DisplayName = "Manual", Visibility = PXUIVisibility.Visible)]
		 public virtual bool? IsManual
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
         protected bool? _ExcludeFromDiscountableAmt;
		 [PXDBBool()]
		 [PXDefault(false)]
		 [PXUIField(DisplayName = "Exclude From Discountable Amount", Visibility = PXUIVisibility.Visible)]
         public virtual bool? ExcludeFromDiscountableAmt
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
         protected bool? _SkipDocumentDiscounts;
		 [PXDBBool()]
		 [PXDefault(false)]
		 [PXUIField(DisplayName = "Skip Document Discounts", Visibility = PXUIVisibility.Visible)]
         public virtual bool? SkipDocumentDiscounts
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
		 protected bool? _IsAutoNumber;
		 [PXDBBool()]
		 [PXDefault(false)]
		 [PXUIField(DisplayName = "Auto-Numbering", Visibility = PXUIVisibility.Visible)]
		 public virtual bool? IsAutoNumber
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
		 protected string _LastNumber;
		 [PXDBString(10, IsUnicode = true)]
		 [PXUIField(DisplayName = "Last Number", Visibility = PXUIVisibility.Visible)]
		 public virtual string LastNumber
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
		 protected string _CreatedByScreenID;
		 [PXDBCreatedByScreenID()]
		 public virtual string CreatedByScreenID
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
		 [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		 protected string _LastModifiedByScreenID;
		 [PXDBLastModifiedByScreenID()]
		 public virtual string LastModifiedByScreenID
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
		 [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		 #region tstamp
		 public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		 protected byte[] _tstamp;
		 [PXDBTimestamp()]
		 public virtual byte[] tstamp
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
