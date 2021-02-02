using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.Common.Attributes;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents the customer-specific settings of a <see cref="PaymentMethod">
	/// payment method</see>. For instance payment methods (such as credit cards), a
	/// customer payment method record is obligatory and defines all details
	/// (see <see cref="CustomerPaymentMethodDetail"/>) necessary to use the method
	/// to record payments. For generic payment methods (such as cash or wire transfer),
	/// the presence of a customer-specific payment method record is optional,
	/// but it can nevertheless be defined to override the default payment method settings.
	/// The entities of this type are edited on the Customer Payment Methods (AR303010)
	/// form, which corresponds to the <see cref="CustomerPaymentMethodMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXEMailSource]
	[PXCacheName(Messages.CustomerPaymentMethod)]
	[PXPrimaryGraph(typeof(CustomerPaymentMethodMaint))]
	public partial class CustomerPaymentMethod : PX.Data.IBqlTable, ICCPaymentProfile
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the payment
		/// method record has been selected for processing.
		/// </summary>
		/// <value>
		/// This is a non-database bound field.
		/// </value>
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
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		/// <summary>
		/// The identifier of <see cref="Customer">customer</see> to
		/// which the payment method belongs. This field is a part
		/// of the compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDefault(typeof(Customer.bAccountID))]
		[Customer(DescriptionField = typeof(Customer.acctName), IsKey = true, DirtyRead = true)]
		[PXParent(typeof(Select<Customer,
			Where<Customer.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>,
			And<BAccount.type, NotEqual<BAccountType.combinedType>>>>))]
		public virtual Int32? BAccountID
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
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID>
		{
            /// <summary>
            /// Provides a selector for a Customer Payment Method - for example,<br/>
            /// a list a credit cards that customer has. Customer is taken from the row<br/>
            /// </summary>
			public class PMInstanceIDSelectorAttribute : PXSelectorAttribute
			{
				public PMInstanceIDSelectorAttribute()
					: base(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>>>))
				{
				}
				public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
				{
				}
			}
		}
		protected Int32? _PMInstanceID;
		/// <summary>
		/// The unique identifier of the customer payment method.
		/// This field is part of the compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMInstance.PMInstanceID"/>
		/// field. The <see cref="PMInstance"/> table provides identifiers
		/// for both <see cref="PaymentMethod">generic payment methods</see>
		/// and <see cref="CustomerPaymentMethod">customer payment methods</see>.
		/// </value>
		[PXDBForeignIdentity(typeof(PMInstance), IsKey = true)]
		[pMInstanceID.PMInstanceIDSelector]
		[PXUIField(DisplayName = "Card Number")]
		[PXReferentialIntegrityCheck]
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
		/// The identifier of the <see cref="PaymentMethod">payment method</see>
		/// associated with the customer payment method. The settings of this payment
		/// method are used as a template for the customer payment method.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PaymentMethod.PaymentMethodID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Payment Method", Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.isActive,Equal<True>,
                            And<PaymentMethod.useForAR,Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXForeignReference(typeof(Field<paymentMethodID>.IsRelatedTo<PaymentMethod.paymentMethodID>))]
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
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;
		/// <summary>
		/// The identifier of the <see cref="CashAccount">cash account</see>
		/// associated with the customer payment method.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
		/// </value>
		[CashAccount(null, typeof(Search2<CashAccount.cashAccountID, InnerJoin<PaymentMethodAccount,
									On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
									And<PaymentMethodAccount.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>,
									And<PaymentMethodAccount.useForAR,Equal<True>>>>>,
									Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", DescriptionField = typeof(CashAccount.descr), Visibility = PXUIVisibility.Visible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		/// <summary>
		/// The description of the payment method.
		/// </summary>
		/// <value>
		/// The value for this field is automatically generated
		/// by the system from the payment method description and
		/// <see cref="CustomerPaymentMethodDetail">payment method
		/// details</see> with applied display masks (if any).
		/// </value>
		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXDefault("",PersistingCheck =PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Card/Account No", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
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
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the customer
		/// payment method is available for recording payments.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region IsDefault
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		protected Boolean? _IsDefault;
		/// <summary>
		/// An unused obsolete field.
		/// </summary>
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		[PXBool()]
		[PXDefault(false, PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Is Default", Enabled=false)]
		public virtual Boolean? IsDefault
		{
			get
			{
				return this._IsDefault;
			}
			set
			{
				this._IsDefault = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(CustomerPaymentMethod.descr))]
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
		#region ExpirationDate
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
		protected DateTime? _ExpirationDate;
		/// <summary>
		/// The expiration date of the customer payment method.
        /// Don't move the ExpirationDateAttribute down, it must be first.
		/// </summary>
		/// <value>
		/// The value of this field is filled in by the system automatically
		/// from the <see cref="CustomerPaymentMethodDetail">payment method
		/// detail</see> that corresponds to the expiration date, but only if
		/// the expiration date has no display mask.
		/// </value>
        [CCPaymentProcessing.ExpirationDate]
		[PXDBDateString(DateFormat = "MM/yy")]
		[PXUIField(DisplayName = "Expiration Date", Enabled = false)]
		public virtual DateTime? ExpirationDate
		{
			get
			{
				return this._ExpirationDate;
			}
			set
			{
				this._ExpirationDate = value;
			}
		}
		#endregion
		#region CVVVerifyTran
		public abstract class cVVVerifyTran : PX.Data.BQL.BqlInt.Field<cVVVerifyTran> { }
		protected Int32? _CVVVerifyTran;
		/// <summary>
		/// The identifier of the CVV code verification transaction.
		/// </summary>
		/// <value>
		/// Corresponds to <see cref="CCProcTran.TranNbr"/>.
		/// </value>
		[PXDBInt()]
		public virtual Int32? CVVVerifyTran
		{
			get
			{
				return this._CVVVerifyTran;
			}
			set
			{
				this._CVVVerifyTran = value;
			}
		}
		#endregion
        #region BillAddressID
        public abstract class billAddressID : PX.Data.BQL.BqlInt.Field<billAddressID> { }
		protected Int32? _BillAddressID;
		/// <summary>
		/// For customer payment methods that require remittance information
		/// (that is, have <see cref="HasBillingInfo"/> set to <c>true</c>),
		/// contains the identifier of the <see cref="Address">billing
		/// address</see> associated with the payment method. The field
		/// defaults to the <see cref="Customer.DefBillAddressID"> default
		/// billing address of the customer</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Address.AddressID"/> field.
		/// </value>
		[PXDBInt()]
        [PXDBChildIdentity(typeof(Address.addressID))]
        public virtual Int32? BillAddressID
        {
            get
            {
                return this._BillAddressID;
            }
            set
            {
                this._BillAddressID = value;
            }
        }

        #endregion
        #region BillContactID
        public abstract class billContactID : PX.Data.BQL.BqlInt.Field<billContactID> { }

        protected int? _BillContactID;
		/// <summary>
		/// For customer payment methods that require remittance information
		/// (that is, have <see cref="HasBillingInfo"/> set to <c>true</c>),
		/// contains the identifier of the <see cref="Contact">billing contact</see>
		/// associated with the payment method. The field defaults to the
		/// <see cref="Customer.DefBillContactID">default billing contact
		/// of the customer</see>.
		/// </summary>
        [PXDBInt()]
        [PXDBChildIdentity(typeof(Contact.contactID))]
        public virtual Int32? BillContactID
        {
            get
            {
                return this._BillContactID;
            }
            set
            {
                this._BillContactID = value;
            }
        }

        #endregion
        #region ARHasBillingInfo
        public abstract class hasBillingInfo : PX.Data.BQL.BqlBool.Field<hasBillingInfo> { }
        protected bool? _HasBillingInfo;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the customer payment
		/// method requires remittance information. Defaults to
		/// <see cref="PaymentMethod.ARHasBillingInfo"/>.
		/// </summary>
        [PXBool()]
        [PXUIField(DisplayName = "Has Billing Info", Visible= false, Enabled=false)]
        public virtual bool? HasBillingInfo
        {
            get
            {
                return this._HasBillingInfo;
            }
            set
            {
                this._HasBillingInfo = value;
            }
        }
        #endregion

        #region IsBillAddressSameAsMain
        public abstract class isBillAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isBillAddressSameAsMain> { }
        protected bool? _IsBillAddressSameAsMain;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the customer payment
		/// method should use the <see cref="Customer.DefBillAddressID">
		/// default billing address</see> of the associated customer record
		/// for sending payment remittance information.
		/// </summary>
        [PXBool()]
        [PXUIField(DisplayName = "Same as Main")]
        public virtual bool? IsBillAddressSameAsMain
        {
            get
            {
                return this._IsBillAddressSameAsMain;
            }
            set
            {
                this._IsBillAddressSameAsMain = value;
            }
        }
        #endregion
        #region IsBillContSameAsMain
        public abstract class isBillContactSameAsMain : PX.Data.BQL.BqlBool.Field<isBillContactSameAsMain> { }
        protected bool? _IsBillContactSameAsMain;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the customer payment
		/// method should use the <see cref="Customer.DefBillContactID">
		/// default billing contact</see> of the associated customer
		/// record for sending payment remittance information.
		/// </summary>
        [PXBool()]
        [PXUIField(DisplayName = "Same as Main")]
        public virtual bool? IsBillContactSameAsMain
        {
            get
            {
                return this._IsBillContactSameAsMain;
            }
            set
            {
                this._IsBillContactSameAsMain = value;
            }
        }
        #endregion
		#region CCProcessingCenterID
		public abstract class cCProcessingCenterID : PX.Data.BQL.BqlString.Field<cCProcessingCenterID> { }
		/// <summary>
		/// The identifier of the credit card processing center.
		/// </summary>
		/// <value>
		/// The field has a value if the customer payment method is configured
		/// to process payments through a payment gateway. The value corresponds to the
		/// value of the <see cref="CCProcessingCenterPmntMethod.processingCenterID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(Search2<CCProcessingCenterPmntMethod.processingCenterID,
			InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>,
			Where<CCProcessingCenterPmntMethod.isDefault, Equal<True>,
				And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
				And<CCProcessingCenter.isActive, Equal<True>,
				And<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<CCProcessingCenterPmntMethod.processingCenterID, 
			InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>,
			Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>,
				And<CCProcessingCenterPmntMethod.isActive,Equal<True>,
				And<CCProcessingCenter.isActive, Equal<True>>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, Visible = false)]
		[DisabledProcCenter(CheckFieldValue = DisabledProcCenterAttribute.CheckFieldVal.ProcessingCenterId)]
		[DeprecatedProcessing(ChckVal = DeprecatedProcessingAttribute.CheckVal.ProcessingCenterId)]
		public virtual string CCProcessingCenterID { get; set; }
		#endregion
		#region CustomerCCPID
		public abstract class customerCCPID : PX.Data.BQL.BqlString.Field<customerCCPID> { }
		/// <summary>
		/// The identifier of the customer profile associated with the customer
		/// account in Acumatica ERP and Authorize.Net. The main purpose of
		/// the identifier is to link multiple bank cards to a single customer
		/// entity and to synchronize record details between systems.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CustomerProcessingCenterID.CustomerCCPID"/> field.
		/// </value>
		[PXDBString(1024, IsUnicode = true)]
		[PXDefault(typeof(Search<CustomerProcessingCenterID.customerCCPID,
			Where<CustomerProcessingCenterID.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>,
				And<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Current<CustomerPaymentMethod.cCProcessingCenterID>>>>,
			OrderBy<Desc<CustomerProcessingCenterID.createdDateTime>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CustomerProcessingCenterID.customerCCPID, Where<CustomerProcessingCenterID.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>,
			And<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Current<CustomerPaymentMethod.cCProcessingCenterID>>>>>),
			ValidateValue = false)]
		[PXUIField(DisplayName = "Customer Profile ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, Visible = false)]
		public virtual string CustomerCCPID { get; set; }
		#endregion

		#region Converted
		public abstract class converted : PX.Data.BQL.BqlBool.Field<converted> { }
		protected Boolean? _Converted;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the customer payment 
		/// method is the result of another payment method conversion using 
		/// the Payment Method Converter (CA207000) form, which corresponds 
		/// to the <see cref="PaymentMethodConverter"/> graph.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		[Obsolete("This field will be removed in 2018R2 version.")]
		public virtual Boolean? Converted
		{
			get
			{
				return this._Converted;
			}
			set
			{
				this._Converted = value;
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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedDateTime]
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

		#region LastNotificationDate
		public abstract class lastNotificationDate : PX.Data.BQL.BqlDateTime.Field<lastNotificationDate> { }
		protected DateTime? _LastNotificationDate;
		/// <summary>
		/// The date of last notification to the customer about payment
		/// method expiration (e.g. such as expiration of a credit card).
		/// </summary>
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Notification Date")]
		public virtual DateTime? LastNotificationDate
		{
			get
			{
				return this._LastNotificationDate;
			}
			set
			{
				this._LastNotificationDate = value;
			}
		}
		#endregion



	}

	[Serializable]
	[PXCacheName(Messages.CustomerPaymentMethodInputMode)]
	public partial class CustomerPaymentMethodInputMode : IBqlTable
	{
		#region InputMode
		public abstract class inputMode : PX.Data.BQL.BqlString.Field<inputMode> { }
		protected string _InputMode;
		[PXString(1)]
		[InputModeType.List]
		[PXDefault(InputModeType.Details, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Input Mode", Visible = false, Enabled = true)]
		public virtual string InputMode
		{
			get
			{
				return this._InputMode;
			}
			set
			{
				this._InputMode = value;
			}
		}
		#endregion
		
	}

	public class InputModeType
	{
		public const string Token = "T";
		public const string Details = "D";

		public class token : PX.Data.BQL.BqlString.Constant<token>
		{
			public token() : base(Token) { }
		}

		public class details : PX.Data.BQL.BqlString.Constant<details>
		{
			public details() : base(Details) { }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Token, Details }, new string[] { Messages.TokenInputMode, Messages.DetailsInputMode })
			{
			}
		}
	}
}
