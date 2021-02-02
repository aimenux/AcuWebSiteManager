namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.CA;
	using PX.Objects.AR.CCPaymentProcessing.Interfaces;
	/// <summary>
	/// Represents a payment method setting for a <see cref="CustomerPaymentMethod">
	/// customer payment method</see>. The purpose of this entity is to define
	/// customer-specific values for <see cref="PaymentMethodDetail">payment 
	/// method settings</see>. The entities of this type are created and edited
	/// on the Customer Payment Methods (AR303010) form, which corresponds to
	/// the <see cref="CustomerPaymentMethodMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CustomerPaymentMethodDetail)]
	public partial class CustomerPaymentMethodDetail : PX.Data.IBqlTable,ICCPaymentProfileDetail
	{
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected Int32? _PMInstanceID;
		/// <summary>
		/// The identifier of the payment method instance from the
		/// parent <see cref="CustomerPaymentMethod">customer payment
		/// method</see> record. This field is a part of the compound 
		/// key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CustomerPaymentMethod.PMInstanceID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(CustomerPaymentMethod.pMInstanceID))]
		[PXParent(typeof(Select<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<CustomerPaymentMethodDetail.pMInstanceID>>>>))]
		public virtual Int32? PMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
				this._PMInstanceID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		/// <summary>
		/// The identifier of the payment method from the parent
		/// <see cref="CustomerPaymentMethod">customer payment method</see>
		/// record. This field is a part of the compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CustomerPaymentMethod.PMInstanceID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Search<CustomerPaymentMethod.paymentMethodID, Where<CustomerPaymentMethod.pMInstanceID,Equal<Current<CustomerPaymentMethodDetail.pMInstanceID>>>>))]		
		public virtual String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID>
		{
			//To Override Unconditional Select
			public class DetailIDSelectorAttribute : PXSelectorAttribute
			{
				public DetailIDSelectorAttribute()
					: base(typeof(Search<PaymentMethodDetail.detailID,
						Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CustomerPaymentMethodDetail.paymentMethodID>>,
						And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>))
				{
					_UnconditionalSelect = _PrimarySelect;
					DescriptionField = typeof(PaymentMethodDetail.descr);
					SelectorMode = PXSelectorMode.DisplayModeText;
				}
			}
		}
		protected String _DetailID;
		/// <summary>
		/// The name of the payment method setting, such as Card Number,
		/// Expiration Date. This field is a part of the compound key 
		/// of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PaymentMethodDetail.DetailID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName="Description", Enabled=false, IsReadOnly=true)]
		[detailID.DetailIDSelector()] //To Override Unconditional Select
		public virtual String DetailID
		{
			get
			{
				return this._DetailID;
			}
			set
			{
				this._DetailID = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		protected String _Value;
		/// <summary>
		/// The value for the customer payment method setting,
		/// such as the actual credit card number, the expiration date.
		/// This value in this field can be subject to dynamic value
		/// validation depending on the regular expression defined
		/// in the corresponding <see cref="PaymentMethodDetail"/>.
		/// </summary>
		//[PXDBStringWithMask(255, typeof(Search<PaymentMethodDetail.entryMask, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CustomerPaymentMethodDetail.paymentMethodID>>,
		//                              And<PaymentMethodDetail.detailID, Equal<Current<CustomerPaymentMethodDetail.detailID>>>>>), IsUnicode = true)]
		[DynamicValueValidation(typeof(Search<PaymentMethodDetail.validRegexp, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CustomerPaymentMethodDetail.paymentMethodID>>,
										And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
									  And<PaymentMethodDetail.detailID, Equal<Current<CustomerPaymentMethodDetail.detailID>>>>>>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRSACryptStringWithMaskAttribute(1028, typeof(Search<PaymentMethodDetail.entryMask, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CustomerPaymentMethodDetail.paymentMethodID>>,
									  And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
									  And<PaymentMethodDetail.detailID, Equal<Current<CustomerPaymentMethodDetail.detailID>>>>>>), IsUnicode = true)]
		//[PXRSACryptStringAttribute(1028)]
		[PXUIField(DisplayName = "Value")]
		[PXPersonalDataFieldAttribute.Value]
		public virtual String Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this._Value = value;
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
	}
}
