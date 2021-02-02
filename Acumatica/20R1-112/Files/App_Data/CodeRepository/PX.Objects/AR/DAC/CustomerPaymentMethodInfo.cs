using System;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.Common.Attributes;

namespace PX.Objects.AR
{

	/// <summary>
	/// A service projection DAC that is used to display customer payment method
	/// settings on the Customers (AR303000) form. The DAC is designed so that the 
	/// <see cref="CustomerPaymentMethod">customer payment method</see> settings are 
	/// displayed if they exist; <see cref="PaymentMethod">generic payment method</see> 
	/// settings are displayed otherwise.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.CustomerPaymentMethodInfo)]
	[PXProjection(
		typeof(Select2<PMInstance, LeftJoin<PaymentMethod, On<PMInstance.pMInstanceID, Equal<PaymentMethod.pMInstanceID>>,
				LeftJoin<CustomerPaymentMethod, On<PMInstance.pMInstanceID, Equal<CustomerPaymentMethod.pMInstanceID>>,
					LeftJoin<PaymentMethodActive, On<PaymentMethod.paymentMethodID, Equal<PaymentMethodActive.paymentMethodID>,
						Or<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethodActive.paymentMethodID>>>>>>,
			Where2<Where<PaymentMethod.aRIsOnePerCustomer, Equal<True>, Or<PaymentMethod.aRIsOnePerCustomer, IsNull>>,
				And2<Where<PaymentMethod.useForAR, Equal<True>, Or<PaymentMethod.useForAR, IsNull>>,
					And<Where<PaymentMethod.pMInstanceID, IsNotNull, Or<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>))]
	public partial class CustomerPaymentMethodInfo : PX.Data.IBqlTable
	{
		#region BAccountID

		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		/// <summary>
		/// The identifier of the <see cref="Customer"/> record
		/// from the <see cref="CustomerPaymentMethod">customer
		/// payment method</see> settings.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CustomerPaymentMethod.BAccountID"/> field.
		/// </value>
		[PXDBInt(BqlField = typeof(CustomerPaymentMethod.bAccountID))]
		public virtual Int32? BAccountID { get; set; }

		#endregion

		#region IsDefault

		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the payment method
		/// is used by default for the customer.
		/// </summary>
		/// <value>
		/// This is an unbound field.
		/// </value>
		[PXDBBool()]
		[PXUIField(DisplayName = "Is Default")]
		public virtual Boolean? IsDefault { get; set; }

		#endregion

		#region PaymentMethodID

		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		/// <summary>
		/// The identifier of the <see cref="PaymentMethod">payment method</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PaymentMethod.PaymentMethodID"/> field.
		/// </value>
		[PXString(10, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(
				Switch
				<Case<Where<PaymentMethod.paymentMethodID, IsNotNull>, PaymentMethod.paymentMethodID>,
					CustomerPaymentMethod.paymentMethodID>), typeof(string))]
		public virtual String PaymentMethodID { get; set; }

		#endregion

		#region PMInstanceID

		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		/// <summary>
		/// The identifier of the <see cref="PMInstance">payment method instance</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMInstance.PMInstanceID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(PMInstance.pMInstanceID))]
		[DisabledProcCenter(ErrorMappedFieldName = nameof(CustomerPaymentMethodInfo.Descr))]
		public virtual Int32? PMInstanceID { get; set; }

		#endregion

		#region CashAccountID

		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		/// <summary>
		/// The identifier of the <see cref="CashAccount">cash account</see>
		/// from the <see cref="CustomerPaymentMethod">customer payment method</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
		/// </value>
		[PXDBInt(BqlField = typeof(CustomerPaymentMethod.cashAccountID))]
		[PXSelector(typeof(CashAccount.cashAccountID), SubstituteKey = typeof(CashAccount.cashAccountCD))]
		[PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Int32? CashAccountID { get; set; }

		#endregion

		#region Descr

		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
		/// <summary>
		/// The description of the payment method.
		/// </summary>
		/// <value>
		/// The value of the field is taken from either the <see cref="PaymentMethod.Descr"/> 
		/// field, or the <see cref="CustomerPaymentMethod.Descr"/> field (if the 
		/// <see cref="PaymentMethod.Descr"/> field is empty).
		/// </value>
		[PXDBLocalizableString(255, IsUnicode = true, NonDB = true, BqlField = typeof(PaymentMethod.descr))]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(Switch<Case<Where<PaymentMethod.descr, IsNotNull>, PaymentMethod.descr>, CustomerPaymentMethod.descr>),
			typeof(string))]
		public virtual String Descr { get; set; }

		#endregion

		#region IsActive

		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the payment method is active. 
		/// </summary>
		/// <value>
		/// The value of the field is taken from either the <see cref="PaymentMethod.IsActive"/>
		/// field, or the <see cref="CustomerPaymentMethod.IsActive"/> field (if the 
		/// <see cref="PaymentMethod.IsActive"/> field is <c>null</c> or <c>false</c>).
		/// </value>
		[PXBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(
				Switch<
					Case<Where<PaymentMethodActive.isActive, Equal<True>>,
						Switch<
							Case<Where<CustomerPaymentMethod.isActive, IsNotNull>, CustomerPaymentMethod.isActive>, PaymentMethod.isActive>>,
					Null>
			),
			typeof(bool))]
		public virtual Boolean? IsActive { get; set; }

		#endregion

		#region ARIsOnePerCustomer

		public abstract class aRIsOnePerCustomer : PX.Data.BQL.BqlBool.Field<aRIsOnePerCustomer> { }
		/// <summary>
		/// Indicates (if set to <c>true</c>) that there can be only one
		/// instance of the payment method for each <see cref="Customer">
		/// customer</see>.
		/// </summary>
		[PXDBBool(BqlField = typeof(PaymentMethod.aRIsOnePerCustomer))]
        [PXDefault(false)]
        public virtual Boolean? ARIsOnePerCustomer { get; set; }
        #endregion     
		#region IsCustomerPaymentMethod

		public abstract class isCustomerPaymentMethod : PX.Data.BQL.BqlBool.Field<isCustomerPaymentMethod> { }
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the payment method has
		/// been overridden at the <see cref="Customer">customer</see>
		/// level; that is, there is a <see cref="CustomerPaymentMethod"/> 
		/// record defined for the combination of the customer record 
		/// and the generic payment method.
		/// </summary>
		[PXBool()]
		[PXDBCalced(typeof(Switch<Case<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>, True>, False>), typeof(bool))]
		[PXUIField(DisplayName = "Override", Enabled = false)]
		public virtual bool? IsCustomerPaymentMethod { get; set; }

		#endregion
	}
}