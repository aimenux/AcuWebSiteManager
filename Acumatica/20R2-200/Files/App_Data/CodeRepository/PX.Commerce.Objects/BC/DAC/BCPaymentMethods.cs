using PX.Commerce.Core;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.GL;
using System;

namespace PX.Commerce.Objects
{
	[Serializable]
	[PXCacheName("BCPaymentMethods")]
	public class BCPaymentMethods : IBqlTable
	{
		#region Keys
		
		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCPaymentMethods>.By<bindingID> { }
			public class CashAcc : CashAccount.PK.ForeignKeyOf<BCPaymentMethods>.By<cashAccountID> { }
		}
		#endregion

		#region PaymentMappingID
		[PXDBIdentity(IsKey = true)]
		public int? PaymentMappingID { get; set; }
		public abstract class paymentMappingID : IBqlField { }
		#endregion
		#region BindingID
		[PXDBInt()]
		[PXUIField(DisplayName = "Store")]
		[PXSelector(typeof(BCBinding.bindingID),
					typeof(BCBinding.bindingName),
					SubstituteKey = typeof(BCBinding.bindingName))]
		[PXParent(typeof(Select<BCBinding,
			Where<BCBinding.bindingID, Equal<Current<BCPaymentMethods.bindingID>>>>))]
		[PXDBDefault(typeof(BCBinding.bindingID),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		#region StorePaymentMethod
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Payment Method")]
		[BCCapitalLettersAttribute]
		public virtual string StorePaymentMethod { get; set; }
		public abstract class storePaymentMethod : IBqlField { }
		#endregion

		#region StoreOrderPaymentMethod
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Order Payment Method", Visible = false)]
		[BCCapitalLettersAttribute]
		public virtual string StoreOrderPaymentMethod { get; set; }
		public abstract class storeOrderPaymentMethod : IBqlField { }
		#endregion

		#region Currency
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXFormula(typeof(Selector<cashAccountID, CashAccount.curyID>))]
		public virtual string CuryID { get; set; }
		public abstract class curyID : IBqlField { }
		#endregion

		#region PaymentMethodID
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method ID")]
		[PXSelector(typeof(Search4<PaymentMethod.paymentMethodID,
			Where<PaymentMethod.isActive, Equal<True>, 
				And<PaymentMethod.useForAR, Equal<True>>>,			
			Aggregate<GroupBy<PaymentMethod.paymentMethodID, GroupBy<PaymentMethod.useForAR, GroupBy<PaymentMethod.useForAP>>>>>), 
			DescriptionField = typeof(PaymentMethod.descr))]		
		public virtual string PaymentMethodID { get; set; }
		public abstract class paymentMethodID : IBqlField { }
		#endregion

		#region CashAcccount
		[PXDBInt]
		[PXUIField(DisplayName = "Cash Account")]
		[PXSelector(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<BCPaymentMethods.paymentMethodID>>,
							And<PaymentMethodAccount.useForAR, Equal<True>>>>,
						CrossJoin<Company>>,
						Where<CashAccount.curyID, Equal<Company.baseCuryID>,
							And<CashAccount.branchID,Equal<Current<BCBinding.branchID>>>>>),
				 DescriptionField = typeof(CashAccount.descr),
					SubstituteKey = typeof(CashAccount.cashAccountCD)
			)]		
		public virtual int? CashAccountID { get; set; }
		public abstract class cashAccountID : IBqlField { }
		#endregion

		#region ProcessingCenterID
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center ID")]
		[PXSelector(typeof(Search2<CCProcessingCenter.processingCenterID,
			InnerJoin<CCProcessingCenterPmntMethod, 
				On<CCProcessingCenterPmntMethod.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>>,
			Where<CCProcessingCenter.isActive, Equal<True>,
				And<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<BCPaymentMethods.paymentMethodID>>>>>))]
		public virtual string ProcessingCenterID { get; set; }
		public abstract class processingCenterID : IBqlField { }
		#endregion

		#region ReleasePayments
		[PXDBBool]
		[PXUIField(DisplayName = "Release Payments")]
		[PXDefault(false)]
		public virtual bool? ReleasePayments { get; set; }
		public abstract class releasePayments : IBqlField { }
		#endregion

		#region Active
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(false)]
		public virtual bool? Active { get; set; }
		public abstract class active : IBqlField { }
		#endregion

		#region CreatePaymentFromOrder
		[PXDBBool]
		[PXUIField(DisplayName = "Create Payment from Order", Visible = false)]
		[PXDefault(false)]
		public virtual bool? CreatePaymentFromOrder { get; set; }
		public abstract class createPaymentFromOrder : IBqlField { }
		#endregion
	}

	[Serializable]
	[PXCacheName("BCMultiCurrencyPaymentMethod")]
	public class BCMultiCurrencyPaymentMethod : IBqlTable
	{
		#region BindingID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Store")]
		[PXSelector(typeof(BCBinding.bindingID),
					typeof(BCBinding.bindingName),
					SubstituteKey = typeof(BCBinding.bindingName))]
		[PXParent(typeof(Select<BCPaymentMethods,
			Where<BCPaymentMethods.bindingID, Equal<Current<BCMultiCurrencyPaymentMethod.bindingID>>,
				And<BCPaymentMethods.paymentMappingID, Equal<Current<BCMultiCurrencyPaymentMethod.paymentMappingID>>>>>))]
		[PXDBDefault(typeof(BCPaymentMethods.bindingID),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		#region PaymentMappingID
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BCPaymentMethods.paymentMappingID))]
		public int? PaymentMappingID { get; set; }
		public abstract class paymentMappingID : IBqlField { }
		#endregion

		#region CashAcccount
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Cash Account")]
		[PXSelector(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current2<BCPaymentMethods.paymentMethodID>>,
							And<PaymentMethodAccount.useForAR, Equal<True>>>>,
						CrossJoin<Company>>,
						Where<CashAccount.curyID, NotEqual<Company.baseCuryID>,
								And<CashAccount.branchID, Equal<Current<BCBinding.branchID>>>
					>>),
				 DescriptionField = typeof(CashAccount.descr),
				 SubstituteKey = typeof(CashAccount.cashAccountCD)
			)]
		public virtual int? CashAccountID { get; set; }
		public abstract class cashAccountID : IBqlField { }
		#endregion

		#region Currency
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXFormula(typeof(Selector<cashAccountID, CashAccount.curyID>))]
		public virtual string CuryID { get; set; }
		public abstract class curyID : IBqlField { }
		#endregion
	}

	[Serializable]
	[PXHidden]
	public class BCBigCommercePayment : IBqlTable
	{		
		#region Name
		[PXDBString(IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual string Name { get; set; }
		public abstract class name : IBqlField { }
		#endregion

		#region Create Payment from Order
		[PXDBBool]
		[PXUIField(DisplayName = "Create Payment from Order")]
		[PXDefault(false)]
		public virtual bool? CreatePaymentfromOrder { get; set; }
		public abstract class createPaymentfromOrder : IBqlField { }
		#endregion
	}
}
