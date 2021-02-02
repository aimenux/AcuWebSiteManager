using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using PX.Common;

using PX.Data;

using PX.SM;

using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.Repositories;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

using CashAccountAttribute = PX.Objects.GL.CashAccountAttribute;
using PX.Objects.GL.Helpers;
using PX.Objects.TX;
using PX.Objects.IN;

namespace PX.Objects.AR
{
	namespace Override
	{
		[Serializable]
        public partial class BAccount : IBqlTable
        {
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            [PXDBInt(IsKey = true)]
            public virtual Int32? BAccountID { get; set; }
            #endregion
            #region AcctName
            public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
            [PXDBString(60, IsUnicode = true)]
            public virtual string AcctName { get; set; }
            #endregion
            #region ConsolidateToParent
            public abstract class consolidateToParent : PX.Data.BQL.BqlBool.Field<consolidateToParent> { }
            [PXDBBool]
            public virtual bool? ConsolidateToParent { get; set; }
            #endregion
            #region ParentBAccountID
            public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
            [PXDBInt]
            public virtual Int32? ParentBAccountID { get; set; }
            #endregion
            #region ConsolidatingBAccountID
            public abstract class consolidatingBAccountID : PX.Data.BQL.BqlInt.Field<consolidatingBAccountID> { }
            [PXDBInt]
            public virtual Int32? ConsolidatingBAccountID { get; set; }
            #endregion
        }

        [Serializable]
        public partial class Customer : IBqlTable
        {
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            [PXDBInt(IsKey = true)]
            public virtual int? BAccountID { get; set; }
            #endregion
            #region StatementCycleId
            public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
            [PXDBString(10, IsUnicode = true)]
            public virtual string StatementCycleId { get; set; }
            #endregion
            #region ConsolidateStatements
            public abstract class consolidateStatements : PX.Data.BQL.BqlBool.Field<consolidateStatements> { }
            [PXDBBool]
            public virtual bool? ConsolidateStatements { get; set; }
            #endregion
            #region SharedCreditCustomerID
            public abstract class sharedCreditCustomerID : PX.Data.BQL.BqlInt.Field<sharedCreditCustomerID> { }
            [PXDBInt]
            public virtual int? SharedCreditCustomerID { get; set; }
            #endregion
            #region SharedCreditPolicy
            public abstract class sharedCreditPolicy : PX.Data.BQL.BqlBool.Field<sharedCreditPolicy> { }
            [PXDBBool]
            public virtual bool? SharedCreditPolicy { get; set; }
			#endregion
			#region StatementLastDate
			public abstract class statementLastDate : PX.Data.BQL.BqlDateTime.Field<statementLastDate> { }
			[PXDBDate]
			public virtual DateTime? StatementLastDate { get; set; }
			#endregion
		}

        public class ExtendedCustomer : Tuple<Customer, BAccount>
        {
            public Customer Customer => Item1;
            public BAccount BusinessAccount => Item2;

            public ExtendedCustomer(Customer customer, BAccount businessAccount)
                : base(customer, businessAccount)
            { }
        }
    }

    /// <summary>
	/// Contains helper methods to retrieve customer families.
	/// </summary>
    public static class CustomerFamilyHelper
    {
        /// <summary>
        /// Gets the family of customers whose ID is either equal to the provided parent customer ID, 
        /// or which contain that ID in the specified parent field.
        /// </summary>
        /// <param name="customerID">
        /// The parent customer ID. If <c>null</c>, the value will be taken from the Current customer used.
        /// </param>
        public static IEnumerable<Override.ExtendedCustomer> GetCustomerFamily<ParentField>(PXGraph graph, int? customerID = null)
            where ParentField : IBqlField
        {
            return PXSelectReadonly2<
                Override.Customer,
                    InnerJoin<Override.BAccount,
                        On<Override.Customer.bAccountID, Equal<Override.BAccount.bAccountID>>>,
                Where<
                    Override.BAccount.bAccountID, Equal<Optional<Customer.bAccountID>>,
                    Or<ParentField, Equal<Optional<Customer.bAccountID>>>>>
                .Select(graph, customerID, customerID).AsEnumerable()
                .Cast<PXResult<Override.Customer, Override.BAccount>>()
                .Select(result => new Override.ExtendedCustomer(result, result));
        }
    }

	public class CustomerMaint : BusinessAccountGraphBase<Customer, Customer, Where<BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.combinedType>>>>
	{
		#region State
		protected ARStatementCycleRepository _statementCycleRepository; 
		#endregion

		#region InternalTypes

		[Serializable]
        [PXHidden]
		public partial class CustomerBalanceSummary : IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[PXDBInt()]
			[PXDefault()]
			public virtual Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
				}
			}
			#endregion
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
			protected Decimal? _Balance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Balance", Visible = true, Enabled = false)]
			public virtual Decimal? Balance
			{
				get
				{
					return this._Balance;
				}
				set
				{
					this._Balance = value;
				}
			}
			#endregion
			#region ConsolidatedBalance
			public abstract class consolidatedbalance : PX.Data.BQL.BqlDecimal.Field<consolidatedbalance> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "Consolidated Balance", Visible = true, Enabled = false)]
			public virtual Decimal? ConsolidatedBalance { get; set; }
			#endregion
			#region UnreleasedBalance
			public abstract class unreleasedBalance : PX.Data.BQL.BqlDecimal.Field<unreleasedBalance> { }
			protected Decimal? _UnreleasedBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Unreleased Balance", Visible = true, Enabled = false)]
			public virtual Decimal? UnreleasedBalance
			{
				get
				{
					return this._UnreleasedBalance;
				}
				set
				{
					this._UnreleasedBalance = value;
				}
			}
			#endregion
			#region DepositsBalance
			public abstract class depositsBalance : PX.Data.BQL.BqlDecimal.Field<depositsBalance> { }
			protected Decimal? _DepositsBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Prepayments Balance",Visible=true,Enabled=false)]
			public virtual Decimal? DepositsBalance
			{
				get
				{
					return this._DepositsBalance;
				}
				set
				{
					this._DepositsBalance = value;
				}
			}
			#endregion
			#region SignedDepositsBalance
			public abstract class signedDepositsBalance : PX.Data.BQL.BqlDecimal.Field<signedDepositsBalance> { }
			
			[PXBaseCury()]
			[PXUIField(DisplayName = "Prepayment Balance",Visible=true,Enabled=false)]
			public virtual Decimal? SignedDepositsBalance
			{
				get
				{
					return (this._DepositsBalance* Decimal.MinusOne);
				}
				set
				{
					
				}
			}
			#endregion
			#region OpenOrdersBalance
			public abstract class openOrdersBalance : PX.Data.BQL.BqlDecimal.Field<openOrdersBalance> { }
			protected Decimal? _OpenOrdersBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Open Orders Balance", Visible = true, Enabled = false, FieldClass = "DISTR")]
			public virtual Decimal? OpenOrdersBalance
			{
				get
				{
					return this._OpenOrdersBalance;
				}
				set
				{
					this._OpenOrdersBalance = value;
				}
			}
			#endregion
			#region RemainingCreditLimit
			public abstract class remainingCreditLimit : PX.Data.BQL.BqlDecimal.Field<remainingCreditLimit> { }
			protected Decimal? _RemainingCreditLimit;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Remaining Credit Limit", Visible = true, Enabled = false)]
			public virtual Decimal? RemainingCreditLimit
			{
				get
				{
					return this._RemainingCreditLimit;
				}
				set
				{
					this._RemainingCreditLimit = value;
				}
			}
			#endregion
			#region OldInvoiceDate
			public abstract class oldInvoiceDate : PX.Data.BQL.BqlDateTime.Field<oldInvoiceDate> { }
			[PXDate]
			[PXUIField(DisplayName = "First Due Date", Visible = true, Enabled = false)]
			public virtual DateTime? OldInvoiceDate { get; set; }
			#endregion
			#region RetainageBalance
			public abstract class retainageBalance : PX.Data.BQL.BqlDecimal.Field<retainageBalance> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "Retained Balance", 
				Visibility = PXUIVisibility.Visible, 
				Enabled = false, 
				FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual decimal? RetainageBalance
			{
				get;
				set;
			}
			#endregion

			public virtual void Init()
			{
				if (!this.Balance.HasValue) this.Balance = Decimal.Zero;
				if (!this.UnreleasedBalance.HasValue) this.Balance = Decimal.Zero;
				if (!this.RemainingCreditLimit.HasValue) this.Balance = Decimal.Zero;
				if (!this.DepositsBalance.HasValue) this.DepositsBalance = Decimal.Zero;
				if (!this.OpenOrdersBalance.HasValue) this.OpenOrdersBalance = Decimal.Zero;
				if (!this.RetainageBalance.HasValue) this.RetainageBalance = Decimal.Zero;
			}
		}

		/// <summary>
		/// Used to fill the required statement date in the 
		/// "Generate On-Demand Statement" dialog box.
		/// <seealso cref="GenerateOnDemandStatement"/>
		/// </summary>
		[Serializable]
		[PXHidden]
		public class OnDemandStatementParameters : IBqlTable
		{
			public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
			[PXDate]
			[PXUIField(DisplayName = Messages.StatementDate)]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? StatementDate
			{
				get;
				set;
			}
		}

		public class PXCustSalesPersonException : PXException
		{
			public PXCustSalesPersonException(string message)
				: base(message)
			{
			}

			public PXCustSalesPersonException(SerializationInfo info, StreamingContext context)
			: base(info, context)
			{
			}
		}
		
		#endregion

		#region Cache Attached
		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<ARNotificationSource.customer>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
		[PXUIField(DisplayName = "Mailing ID")]		
		protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
		{
		}
		[GL.Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
			Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		protected virtual void NotificationSource_NBranchID_CacheAttached(PXCache sender)
		{

		}		
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SiteMap.screenID,
			Where<SiteMap.url, Like<Common.urlReports>,
				And<Where<SiteMap.screenID, Like<PXModule.ar_>,
							 Or<SiteMap.screenID, Like<PXModule.so_>,
							 Or<SiteMap.screenID, Like<PXModule.cr_>>>>>>,
			OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
			Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
			DescriptionField = typeof(SiteMap.title))]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region NotificationRecipient
		[PXDBInt]
		[PXDBLiteDefault(typeof(NotificationSource.sourceID))]
		protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10)]
		[PXDefault]
		[CustomerContactType.List]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationRecipient.contactID),
			Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,
			And<NotificationRecipient.refNoteID, Equal<Current<Customer.noteID>>,
			And<NotificationRecipient.contactID, IsNotNull>>>))]
		protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationRecipient.contactType), DirtyRead = true)]
		protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		protected virtual void NotificationRecipient_ClassID_CacheAttached(PXCache sender)
		{
		}
		[PXString()]
		[PXUIField(DisplayName = "Email", Enabled = false)]
		protected virtual void NotificationRecipient_Email_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region CustSalesPeople
		[SalesPerson(IsKey = true, DescriptionField = typeof(SalesPerson.descr))]
		[PXParent(typeof(Select<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<CustSalesPeople.salesPersonID>>>>))]
		public virtual void CustSalesPeople_SalesPersonID_CacheAttached(PXCache sender)
		{	
		}
		[PXDBInt]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CustSalesPeople.bAccountID>>>>))]
		public virtual void CustSalesPeople_BAccountID_CacheAttached(PXCache sender)
		{	
		}
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(LocationIDAttribute.DimensionName, typeof(Search<LocationExtAddress.locationID, Where<LocationExtAddress.bAccountID,
			Equal<Current<CustSalesPeople.bAccountID>>>>), typeof(LocationExtAddress.locationCD),
			typeof(Location.locationCD), typeof(Location.descr),
			DirtyRead = true, DescriptionField = typeof(LocationExtAddress.descr))]
		[PXDefault(typeof(Search<Customer.defLocationID, Where<Customer.bAccountID, Equal<Current<CustSalesPeople.bAccountID>>>>))]
		public virtual void CustSalesPeople_LocationID_CacheAttached(PXCache sender)
		{	
		}		
		[PXDBDecimal(6)]
		[PXDefault(typeof(Search<SalesPerson.commnPct, Where<SalesPerson.salesPersonID, Equal<Current<CustSalesPeople.salesPersonID>>>>))]
		[PXUIField(DisplayName = "Commission %")]
		public virtual void CustSalesPeople_CommisionPct_CacheAttached(PXCache sender)
		{
		}				
		#endregion

		#region CustomerPaymentMethod
		[PXDBInt()]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		//[Customer(DescriptionField = typeof(Customer.acctName), Visible = false, DirtyRead = true)]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>>>))]
		public virtual void CustomerPaymentMethod_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(10, IsUnicode = true,IsKey=true)]
		[PXUIField(DisplayName = "Payment Method", Enabled = false)]
		[PXDefault(typeof(Customer.defPaymentMethodID))]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.isActive, Equal<True>,
						And<PaymentMethod.useForAR,Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public virtual void CustomerPaymentMethod_PaymentMethodID_CacheAttached(PXCache sender)
		{
		}

		[CashAccount(null, typeof(Search2<CashAccount.cashAccountID, InnerJoin<PaymentMethodAccount,
			On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
			And<PaymentMethodAccount.useForAR,Equal<True>,
			And<PaymentMethodAccount.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>>>>>,
			Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(typeof(Search<CA.PaymentMethod.defaultCashAccountID, Where<CA.PaymentMethod.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>>>))]		
		public virtual void CustomerPaymentMethod_CashAccountID_CacheAttached(PXCache sender)
		{
		}	
		#endregion

		#region CarrierPluginCustomer
		[Customer(DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXUIField(DisplayName = "Customer ID")]
		[PXDBDefault(typeof(Customer.bAccountID))]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CarrierPluginCustomer.customerID>>>>))]
		public virtual void CarrierPluginCustomer_CustomerID_CacheAttached(PXCache sender)
		{
		}	
		#endregion

        #region Customer
        /// <summary>
        /// The cache attached field corresponds to the <see cref="Customer.StatementCustomerID"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Customer.parentBAccountID, IsNotNull, And<Customer.consolidateStatements, Equal<True>>>, Customer.parentBAccountID>, Customer.bAccountID>))]
        protected virtual void Customer_StatementCustomerID_CacheAttached(PXCache sender)
        {
        }

        /// <summary>
        /// The cache attached field corresponds to the <see cref="Customer.SharedCreditCustomerID"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<
            Case<Where<Customer.parentBAccountID, IsNotNull, And<Customer.sharedCreditPolicy, Equal<True>>>, Customer.parentBAccountID>,
            Customer.bAccountID>))]
        protected virtual void Customer_SharedCreditCustomerID_CacheAttached(PXCache sender)
        {
        }

        /// <summary>
        /// The cache attached field corresponds to the <see cref="Customer.IsBillSameAsMain"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Customer.defBillAddressID, Equal<Customer.defAddressID>>, True>, False>))]
        protected virtual void Customer_IsBillSameAsMain_CacheAttached(PXCache sender)
        {
        }

        /// <summary>
        /// The cache attached field corresponds to the <see cref="Customer.IsBillContSameAsMain"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Customer.defBillContactID, Equal<Customer.defContactID>>, True>, False>))]
        protected virtual void Customer_IsBillContSameAsMain_CacheAttached(PXCache sender)
        {
        }
        #endregion
		#endregion

		#region Selects Declarations

		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<Customer.bAccountID>>>> CurrentCustomer;

		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<Customer.bAccountID>>,
			  And<Address.addressID, Equal<Current<Customer.defBillAddressID>>>>> BillAddress;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<Customer.bAccountID>>,
			  And<Contact.contactID, Equal<Current<Customer.defBillContactID>>>>> BillContact;
		public PXSelect<CustSalesPeople, Where<CustSalesPeople.bAccountID, Equal<Current<Customer.bAccountID>>>, OrderBy<Asc<CustSalesPeople.salesPersonID, Asc<CustSalesPeople.locationID>>>> SalesPersons;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Optional<Customer.customerClassID>>>> CustomerClass;
		public PXSetup<ARSetup> ARSetup;
		public PXFilter<CustomerBalanceSummary> CustomerBalance;
		public PXSelect<ARBalances> Balance_for_auto_delete; 
		public PXSelect<CarrierPluginCustomer, Where<CarrierPluginCustomer.customerID, Equal<Current<Customer.bAccountID>>>> Carriers;

		[PXCopyPasteHiddenView(ShowInSimpleImport = true)]
		public PXSelect<CustomerPaymentMethod,
			Where<CustomerPaymentMethod.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>> DefPaymentMethodInstance;
		public PXSelect<CustomerPaymentMethodInfo,
			Where<CustomerPaymentMethodInfo.pMInstanceID, Equal<Current<Customer.defPMInstanceID>>>> DefPaymentMethodInstanceInfo;
		[PXCopyPasteHiddenView(ShowInSimpleImport = true)] 
		public PXSelect<CustomerPaymentMethodInfo, Where<CustomerPaymentMethodInfo.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>> DefPaymentMethod;
		
		[PXCopyPasteHiddenView(ShowInSimpleImport = true)]
		public PXSelect<CustomerPaymentMethodInfo, Where2<Where<CustomerPaymentMethodInfo.bAccountID, Equal<Current<Customer.bAccountID>>, Or<CustomerPaymentMethodInfo.bAccountID, IsNull>>,
			And<CustomerPaymentMethodInfo.isActive, IsNotNull>>> PaymentMethods;

		[PXViewName(CR.Messages.Answers)]
		public CRAttributeList<Customer> Answers;

		[CRReference(typeof(Customer.bAccountID), Persistent = true)]
		public CRActivityList<Customer>
			Activities;

		public IEnumerable paymentMethods()
		{
			PXResultset<CustomerPaymentMethodInfo> cpmInfo = PXSelect<CustomerPaymentMethodInfo, Where2<Where<CustomerPaymentMethodInfo.bAccountID, Equal<Current<Customer.bAccountID>>, Or<CustomerPaymentMethodInfo.bAccountID, IsNull>>,
				And<CustomerPaymentMethodInfo.isActive, IsNotNull>>>.Select(this);
			Dictionary<string, List<CustomerPaymentMethodInfo>> overrides = new Dictionary<string, List<CustomerPaymentMethodInfo>>();
			foreach (CustomerPaymentMethodInfo paymentMethodInfo in cpmInfo)
			{
				List<CustomerPaymentMethodInfo> infoList;
				string key = paymentMethodInfo.PaymentMethodID.ToLower();
				if (!overrides.TryGetValue(key, out infoList))
				{
					infoList = new List<CustomerPaymentMethodInfo>();
					overrides[key] = infoList;
				}
				infoList.Add(paymentMethodInfo);
			}

			foreach (KeyValuePair<string, List<CustomerPaymentMethodInfo>> kvpInfo in overrides)
			{
				if (kvpInfo.Value.Count > 1)
				{
					CustomerPaymentMethodInfo sharedPM = kvpInfo.Value.FindLast((CustomerPaymentMethodInfo info) => info.ARIsOnePerCustomer == true);
					if (sharedPM != null)
					{
						yield return kvpInfo.Value.FindLast((CustomerPaymentMethodInfo info) => info.IsCustomerPaymentMethod == true);
					}
					else
					{
						foreach (CustomerPaymentMethodInfo pmInfo in kvpInfo.Value)
						{
							yield return pmInfo;
						}
					}
				}
				else
				{
					yield return kvpInfo.Value[0];
				}
			}
		}

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<CustomerPaymentMethodDetail,
			  LeftJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
			  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
			  And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Current<Customer.defPMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> DefPaymentMethodInstanceDetails;

		public PXSelectJoin<CustomerPaymentMethodDetail,
			  LeftJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
			  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
			  And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Current<Customer.defPMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> DefPaymentMethodInstanceDetailsAll;

		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>,
			And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>> PMDetails;


		public IEnumerable defPaymentMethodInstanceDetails()
		{
			CustomerPaymentMethod currCPM = DefPaymentMethodInstance.Select(CurrentCustomer.Current.DefPMInstanceID);
			if (currCPM != null)
			{
				bool isTokenized = CCProcessingHelper.IsTokenizedPaymentMethod(this, currCPM.PMInstanceID);
				bool isHF = CCProcessingHelper.IsHFPaymentMethod(this, currCPM.PMInstanceID);
				return CCProcessingHelper.GetPMdetails(this, currCPM.PMInstanceID, isTokenized, !isTokenized);
			}
			else
			{
				return null;
			}
		}

		public PXSelect<PaymentMethod,
		  Where<PaymentMethod.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>>> PaymentMethodDef;

		public CRNotificationSourceList<Customer, Customer.customerClassID, ARNotificationSource.customer> NotificationSources;

		public CRNotificationRecipientList<Customer, Customer.customerClassID> NotificationRecipients;

		public PXSelectJoin<CCProcessingCenter, InnerJoin<CustomerPaymentMethod, 
			On<CCProcessingCenter.processingCenterID, Equal<CustomerPaymentMethod.cCProcessingCenterID>>>, 
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>> PMProcessingCenter;

		public PXSelect<CustomerProcessingCenterID, Where<CustomerProcessingCenterID.bAccountID, Equal<Current<Customer.bAccountID>>,
			And<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
			And<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerPaymentMethod.customerCCPID>>>>>> CustomerProcessingID;

		public PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail,
			On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
				And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<CustomerPaymentMethodDetail.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>>> ccpIdDet;

		public PXFilter<OnDemandStatementParameters> OnDemandStatementDialog;

		public CustomerMaint()
		{
			Activities.GetNewEmailAddress =
				() =>
				{
					var contact = (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Current<Customer.defContactID>>>>.
						Select(this);

					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail)
						? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
						: String.Empty;
				};

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });

			this.PaymentMethods.Cache.AllowInsert = false;
			this.PaymentMethods.Cache.AllowDelete = false;
			ARSetup setup = ARSetup.Current;
			this.DefPaymentMethodInstanceDetails.Cache.AllowInsert = false;
			this.DefPaymentMethodInstanceDetails.Cache.AllowDelete = false;
			this.DefPaymentMethodInstance.Cache.AllowUpdate = false;
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.cashAccountID>(this.DefPaymentMethodInstance.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodDetail.detailID>(this.DefPaymentMethodInstanceDetails.Cache, null, false);

			PXUIFieldAttribute.SetVisible<Customer.cOGSAcctID>(BAccount.Cache, null, false);
			PXUIFieldAttribute.SetVisible<Customer.cOGSSubID>(BAccount.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);

			action.AddMenuAction(ChangeID);

			PXUIFieldAttribute.SetVisible<Customer.localeName>(BAccount.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);

			LocationValidator = new LocationValidator();
			_statementCycleRepository = new ARStatementCycleRepository(this);
		}

		[PXHidden]
		public PXSelect<INItemXRef> xrefs;

		#endregion

		protected LocationValidator LocationValidator;

		#region Buttons

		public PXAction<Customer> viewRestrictionGroups;
		[PXUIField(DisplayName = GL.Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (CurrentCustomer.Current != null)
			{
				ARAccessDetail graph = CreateInstance<ARAccessDetail>();
				graph.Customer.Current = graph.Customer.Search<Customer.acctCD>(CurrentCustomer.Current.AcctCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		public PXAction<Customer> viewVendor;
		[PXUIField(DisplayName = Messages.ViewVendor, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null && (bacct.Type == BAccountType.VendorType || bacct.Type == BAccountType.CombinedType))
			{
				Save.Press();
				AP.VendorMaint editingBO = PXGraph.CreateInstance<AP.VendorMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<AP.VendorR.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, CR.Messages.EditVendor);
			}
			return adapter.Get();
		}

		public PXAction<Customer> viewBusnessAccount;
		[PXUIField(DisplayName = Messages.ViewBusnessAccount, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBusnessAccount(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null)
			{
				Save.Press();
				CR.BusinessAccountMaint editingBO = PXGraph.CreateInstance<PX.Objects.CR.BusinessAccountMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<BAccount.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, "Edit Business Account");
			}
			return adapter.Get();
		}

		public PXAction<Customer> extendToVendor;
		[PXUIField(DisplayName = Messages.ExtendToVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ExtendToVendor(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			Customer customer = this.CurrentCustomer.SelectSingle();

			if (bacct != null && (bacct.Type == BAccountType.CustomerType))
			{
				Save.Press();
				AP.VendorMaint editingBO = PXGraph.CreateInstance<AP.VendorMaint>();

				// Need to take TimeStamp from this because Current may have outdated tstamp
				// after PXUpdates in this.Persist().
				editingBO.TimeStamp = this.TimeStamp;
				AP.VendorR vendor = (AP.VendorR)editingBO.BAccount.Cache.Extend<BAccount>(bacct);
				editingBO.BAccount.Current = vendor;
				vendor.NoteID = bacct.NoteID;
				vendor.Type = BAccountType.CombinedType;
				vendor.LocaleName = customer?.LocaleName;

				LocationExtAddress defLocation = editingBO.DefLocation.Select();
				editingBO.DefLocation.Cache.RaiseRowSelected(defLocation);
				string locationType = LocTypeList.CombinedLoc;

				var defaultLocation = (LocationExtAddress)this.DefLocation.Select();
				if (defLocation != null && defaultLocation.CTaxZoneID != null)
					editingBO.DefLocation.Cache.SetValueExt<Location.vTaxZoneID>(defLocation, defaultLocation.CTaxZoneID);

				editingBO.InitVendorLocation(defLocation, locationType, false);
				defLocation = editingBO.DefLocation.Update(defLocation);
				foreach (LocationExtAddress iLoc in editingBO.Locations.Select())
				{
					if (iLoc.LocationID != defLocation.LocationID)
					{
						editingBO.InitVendorLocation(iLoc, locationType, false);
						editingBO.Locations.Update(iLoc);
					}
				}
				throw new PXRedirectRequiredException(editingBO, CR.Messages.EditVendor);
			}
			return adapter.Get();
		}

		public PXAction<Customer> customerDocuments;
		[PXUIField(DisplayName = "Customer Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable CustomerDocuments(PXAdapter adapter)
		{
			if (BAccount.Current != null && this.BAccount.Current.BAccountID > 0L)
			{
				ARDocumentEnq graph = PXGraph.CreateInstance<ARDocumentEnq>();
				graph.Filter.Current.CustomerID = BAccount.Current.BAccountID;
				graph.Filter.Select(); //Select() is called to trigger the filter delegate, in which the totals are calculated.
				throw new PXRedirectRequiredException(graph, "Customer Details");				
			}
			return adapter.Get();
		}

		protected virtual void VerifyCanHaveSeparateStatement(Customer customer)
		{
			if (customer.ParentBAccountID != null && customer.ConsolidateStatements == true)
				throw new PXException(Messages.CustomerCantHaveSeparateStatement, CurrentCustomer.Cache.GetValueExt<Customer.parentBAccountID>(customer));
		}

		protected virtual void VerifyCanHaveOnDemandStatement(Customer customer)
		{
			if (customer.StatementType != ARStatementType.OpenItem)
			{
				throw new PXException(Messages.OnDemandStatementsAvailableOnlyForOpenItemType);
			}
		}

		public PXAction<Customer> statementForCustomer;
		[PXUIField(DisplayName = Messages.CustomerStatementHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable StatementForCustomer(PXAdapter adapter)
		{
			if (BAccount.Current != null && this.BAccount.Current.BAccountID > 0L)
			{
				VerifyCanHaveSeparateStatement(CurrentCustomer.Current);

				ARStatementForCustomer graph = PXGraph.CreateInstance<ARStatementForCustomer>();
				graph.Filter.Current.CustomerID = BAccount.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Statement For Customer");
			}
			return adapter.Get();
		}

		public PXDBAction<Customer> viewPaymentMethod;
		[PXUIField(DisplayName = "View Payment Method", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewPaymentMethod(PXAdapter adapter)
		{
			if (this.PaymentMethods.Current != null)
			{
				CustomerPaymentMethodInfo current = this.PaymentMethods.Current as CustomerPaymentMethodInfo;
				Customer customer = this.BAccount.Current;
				if (customer != null && current != null && this.BAccount.Current.BAccountID > 0L)
				{
					if (current.ARIsOnePerCustomer != true)
					{
						CustomerPaymentMethodMaint graph = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
						graph.CustomerPaymentMethod.Current =
							graph.CustomerPaymentMethod.Search<CustomerPaymentMethod.pMInstanceID>(current.PMInstanceID, customer.AcctCD);
						PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
					}
					else
					{
						throw new PXSetPropertyException(Messages.NoPaymentInstance, PXErrorLevel.RowInfo);
					}
				}
			}
			return adapter.Get();
		}

		public PXDBAction<Customer> addPaymentMethod;
		[PXUIField(DisplayName = "Add Payment Method", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable AddPaymentMethod(PXAdapter adapter)
		{
			if (this.BAccount.Current != null && this.BAccount.Current.BAccountID > 0L)
			{
				Customer customer = this.BAccount.Current;
				CustomerPaymentMethodMaint graph = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
				CustomerPaymentMethod row = new CustomerPaymentMethod();
				row.BAccountID = customer.BAccountID;
				row = (CustomerPaymentMethod)graph.CustomerPaymentMethod.Insert(row);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}
		
		public PXAction<Customer> newInvoiceMemo;
		[PXUIField(DisplayName = "New Invoice/Memo", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewInvoiceMemo(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				ARInvoiceEntry invEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
				invEntry.Clear();
				ARInvoice newDoc = invEntry.Document.Insert(new ARInvoice());
                invEntry.Document.Cache.SetValueExt<ARInvoice.customerID>(newDoc, customer.BAccountID); //credit rule will be cleared on CustomerID_Updated()
                invEntry.customer.Current.CreditRule = customer.CreditRule;	//re-enforce the credit rule for the document.
				
				throw new PXRedirectRequiredException(invEntry, "ARInvoiceEntry");
			}
			return adapter.Get();
		}

		public PXAction<Customer> newSalesOrder;
		[PXUIField(DisplayName = "New Sales Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewSalesOrder(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				SOOrderEntry soEntry = PXGraph.CreateInstance<SOOrderEntry>();
				soEntry.Clear();
				SOOrder newDoc = (SOOrder) soEntry.Document.Cache.Insert();
				soEntry.Document.Cache.SetValueExt<ARInvoice.customerID>(newDoc, customer.BAccountID); //credit rule will be cleared on CustomerID_Updated()
				soEntry.customer.Current.CreditRule = customer.CreditRule;                      //re-enforce the credit rule for the document.
				if (soEntry.Document.Current.FreightTaxCategoryID != null)
				{
					SOOrder createdOrder = (SOOrder)soEntry.Document.Cache.CreateCopy(soEntry.Document.Current);
					soEntry.Document.Current.FreightTaxCategoryID = null;
					soEntry.Document.Update(createdOrder);
				}
				
				throw new PXRedirectRequiredException(soEntry, "SOOrderEntry");
			}
			return adapter.Get();
		}

		public PXAction<Customer> newPayment;
		[PXUIField(DisplayName = "New Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewPayment(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				ARPaymentEntry payEntry = PXGraph.CreateInstance<ARPaymentEntry>();
				payEntry.Clear();
				ARPayment newDoc = payEntry.Document.Insert(new ARPayment());
				newDoc.CustomerID = customer.BAccountID;
				payEntry.Document.Cache.RaiseFieldUpdated<ARInvoice.customerID>(newDoc, null);
				throw new PXRedirectRequiredException(payEntry, "ARPaymentEntry");
			}
			return adapter.Get();
		}

		public PXAction<Customer> writeOffBalance;
		[PXUIField(DisplayName = "Write Off Balance", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable WriteOffBalance(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				ARCreateWriteOff graph = PXGraph.CreateInstance<ARCreateWriteOff>();
				graph.Clear();
				graph.Filter.Current.CustomerID = customer.BAccountID;
				throw new PXRedirectRequiredException(graph, "WriteOffBalance");
			}
			return adapter.Get();
		}

		public PXAction<Customer> viewBillAddressOnMap;

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable ViewBillAddressOnMap(PXAdapter adapter)
		{
			Address addr = (Address)this.BillAddress.Select();
			BAccountUtility.ViewOnMap(addr);
			return adapter.Get();
		}


		public PXAction<Customer> regenerateLastStatement;
		[PXUIField(DisplayName = Messages.RegenerateLastStatement, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = true)]
		[PXButton]
		public virtual IEnumerable RegenerateLastStatement(PXAdapter adapter)
		{
			var customer = CurrentCustomer.Current;

			if (customer == null)
				return adapter.Get();

			VerifyCanHaveSeparateStatement(customer);

			var cycle = _statementCycleRepository.FindByID(CurrentCustomer.Current?.StatementCycleId);

			if (cycle == null)
			{
				throw new PXException(Messages.StatementCycleNotSpecified);
			}

			IEnumerable<ARStatement> customerStatements = PXSelect<
				ARStatement, 
				Where<
					ARStatement.customerID, Equal<Required<Customer.bAccountID>>,
														And<ARStatement.statementCustomerID, Equal<Required<Customer.statementCustomerID>>,
														And<ARStatement.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>>>>
				.SelectWindowed(this, 0, 1, customer.BAccountID, customer.BAccountID, cycle.StatementCycleId)
				.RowCast<ARStatement>();

			IEnumerable<ARStatement> regularStatements = PXSelect<
				ARStatement,
				Where<
					ARStatement.customerID, Equal<Required<Customer.bAccountID>>,
					And<ARStatement.statementCustomerID, Equal<Required<Customer.statementCustomerID>>,
					And<ARStatement.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>,
					And<ARStatement.onDemand, NotEqual<True>>>>>>
				.SelectWindowed(this, 0, 1, customer.BAccountID, customer.BAccountID, cycle.StatementCycleId)
				.RowCast<ARStatement>();

			if (!customerStatements.Any())
			{
				throw new PXException(Messages.NoStatementToRegenerate);
			}
			else if (!regularStatements.Any())
			{
				throw new PXException(Messages.OnDemandStatementsOnlyCannotRegenerate);
			}

			PXLongOperation.StartOperation(this, () =>
				{
				StatementCycleProcessBO process = CreateInstance<StatementCycleProcessBO>();
					StatementCycleProcessBO.RegenerateStatements(process, cycle, new Customer[] { customer });
				});

			return adapter.Get();
		}

		/// <summary>
		/// Generate an on-demand customer statement for the current customer
		/// without updating the statement cycle and customer's last statement date.
		/// </summary>
		public PXAction<Customer> generateOnDemandStatement;
		[PXButton]
		[PXUIField(
			DisplayName = Messages.GenerateStatementOnDemand, 
			MapEnableRights = PXCacheRights.Update, 
			MapViewRights = PXCacheRights.Update, 
			Visible = false)]
		public virtual IEnumerable GenerateOnDemandStatement(PXAdapter adapter)
		{
			Customer customer = CurrentCustomer.Current;

			if (customer == null)
			{
				return adapter.Get();
			}

			VerifyCanHaveSeparateStatement(customer);
			VerifyCanHaveOnDemandStatement(customer);
			
			if (OnDemandStatementDialog.AskExt() != WebDialogResult.OK
				|| OnDemandStatementDialog.Current?.StatementDate == null)
			{
				return adapter.Get();
			}

			DateTime statementDate = OnDemandStatementDialog.Current.StatementDate.Value;
			ARStatementCycle cycle = _statementCycleRepository.FindByID(CurrentCustomer.Current?.StatementCycleId);

			if (cycle == null)
			{
				throw new PXException(Messages.StatementCycleNotSpecified);
			}

			PXLongOperation.StartOperation(this, () => StatementCycleProcessBO.GenerateOnDemandStatement(
				PXGraph.CreateInstance<StatementCycleProcessBO>(),
				cycle,
				customer,
				statementDate));

			return adapter.Get();
		}

		#region MyButtons (MMK)
		public PXAction<Customer> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<Customer> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.InquiriesFolder)]
		protected virtual IEnumerable Inquiry(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<Customer> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter)
		{
			return adapter.Get();
		}
		#endregion

		//+ MMK 2011/10/04
		public PXAction<Customer> aRBalanceByCustomer;
		[PXUIField(DisplayName = AR.Messages.ARBalanceByCustomer, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable ARBalanceByCustomer(PXAdapter adapter)
		{
			Customer customer = this.BAccountAccessor.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR632500", AR.Messages.ARBalanceByCustomer); //?????
			}
			return adapter.Get();
		}

		public PXAction<Customer> customerHistory;
		[PXUIField(DisplayName = AR.Messages.CustomerHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable CustomerHistory(PXAdapter adapter)
		{
			Customer customer = this.BAccountAccessor.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR652000", AR.Messages.CustomerHistory); //?????
			}
			return adapter.Get();
		}

		public PXAction<Customer> aRAgedPastDue;
		[PXUIField(DisplayName = AR.Messages.ARAgedPastDueReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable ARAgedPastDue(PXAdapter adapter)
		{
			Customer customer = this.BAccountAccessor.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR631000", AR.Messages.ARAgedPastDueReport); //?????
			}
			return adapter.Get();
		}

		public PXAction<Customer> aRAgedOutstanding;
		[PXUIField(DisplayName = AR.Messages.ARAgedOutstandingReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable ARAgedOutstanding(PXAdapter adapter)
		{
			Customer customer = this.BAccountAccessor.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR631500", AR.Messages.ARAgedOutstandingReport); //?????
			}
			return adapter.Get();
		}

		public PXAction<Customer> aRRegister;
		[PXUIField(DisplayName = AR.Messages.ARRegister, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable ARRegister(PXAdapter adapter)
		{
			Customer customer = this.BAccountAccessor.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR621500", AR.Messages.ARRegister); //?????
			}
			return adapter.Get();
		}

		public PXAction<Customer> customerDetails;
		[PXUIField(DisplayName = AR.Messages.CustomerDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable CustomerDetails(PXAdapter adapter)
		{
			Customer customer = this.BAccountAccessor.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR651000", AR.Messages.CustomerDetails); //?????
			}
			return adapter.Get();
		}

		//- MMK 2011/10/04

		public PXAction<Customer> customerStatement;
		[PXUIField(DisplayName = AR.Messages.CustomerStatement, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable CustomerStatement(PXAdapter adapter)
		{
			Customer customer = CurrentCustomer.Current;

			if (customer == null)
			{
				return adapter.Get();
			}

			VerifyCanHaveSeparateStatement(CurrentCustomer.Current);

			ARStatementCycle cycle = new PXSelect<
				ARStatementCycle,
				Where<ARStatementCycle.statementCycleId, Equal<Required<Customer.statementCycleId>>>>(this)
				.SelectSingle(customer.StatementCycleId);

			if (cycle == null)
			{
				throw new PXException(Messages.StatementCycleNotSpecified);
			}

			ARStatement lastStatement = new ARStatementRepository(this).FindLastStatement(
				customer, 
				priorToDate: null, 
				includeOnDemand: true);

			if (lastStatement == null)
			{
				throw new PXException(Messages.NoStatementsForCustomer);
			}

			Dictionary<string, string> reportParameters = ARStatementReportParams.FromCustomer(customer);

			reportParameters[ARStatementReportParams.Parameters.StatementDate] = Convert.ToString(
				lastStatement.StatementDate.Value.Date, 
				System.Globalization.CultureInfo.InvariantCulture);

			string reportID = ARStatementReportParams.ReportIDForCustomer(this, customer, null);

			throw new PXReportRequiredException(reportParameters, reportID, Messages.CustomerStatement);
		}

		public PXAction<Customer> salesPrice;
		[PXUIField(DisplayName = "Sales Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
		public virtual IEnumerable SalesPrice(PXAdapter adapter)
		{
			if (CurrentCustomer.Current != null && CurrentCustomer.Current.BAccountID > 0)
			{
				ARSalesPriceMaint graph = PXGraph.CreateInstance<ARSalesPriceMaint>();
				graph.Filter.Current.PriceType = PriceTypes.Customer;
				graph.Filter.Current.PriceCode = CurrentCustomer.Current.AcctCD;
				throw new PXRedirectRequiredException(graph, "Sales Prices");
			}
			return adapter.Get();
		}

		#region Validate Address Button
		public new PXAction<Customer> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public override IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			Customer bacct = this.BAccount.Current;
			if (bacct != null)
			{
				Address address = FindAddress(bacct.DefAddressID); 
				if (address != null && address.IsValidated == false)
				{
					PXAddressValidator.Validate<Address>(this, address, true);
				}
				Address billAddress = FindAddress(bacct.DefBillAddressID); 
				if (billAddress != null && billAddress.IsValidated == false && (billAddress.AddressID != address.AddressID))
				{
					PXAddressValidator.Validate<Address>(this, billAddress, true);
				}

				LocationExtAddress locAddress = this.DefLocation.Select();
				//Needs to compare defAddress - AddressID  would be null
				if (locAddress != null && locAddress.IsValidated == false && locAddress.IsAddressSameAsMain == false)
				{
					PXAddressValidator.Validate<LocationExtAddress>(this, locAddress, true);
				}

			}
			return adapter.Get();
		}
		#endregion

		public PXChangeID<Customer, Customer.acctCD> ChangeID;

		#endregion

		#region Select Delegates and Sefault Accessors

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable billContact()
		{
			Contact cnt = null;
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.DefBillContactID != null)
			{
				cnt = FindContact(customer.DefBillContactID);
				if (cnt != null)
				{
					if (customer.IsBillContSameAsMain == true)
					{
						cnt = PXCache<Contact>.CreateCopy(cnt);
						PXUIFieldAttribute.SetEnabled(this.BillContact.Cache, cnt, false);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.BillContact.Cache, cnt, true);
					}
				}
			}
			return new Contact[] { cnt };
		}

        [Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable billAddress()
		{
			Address addr = null;
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.DefBillAddressID != null)
			{
				addr = FindAddress(customer.DefBillAddressID);
				if (addr != null)
				{
					if (customer.IsBillSameAsMain == true)
					{
						addr = PXCache<Address>.CreateCopy(addr);
						PXUIFieldAttribute.SetEnabled(this.BillAddress.Cache, addr, false);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.BillAddress.Cache, addr, true);
					}
				}
			}
			return new Address[] { addr };
		}

		protected virtual IEnumerable customerBalance()
		{
			Customer customer = this.BAccountAccessor.Current;
			List<CustomerBalanceSummary> list = new List<CustomerBalanceSummary>(1);
			if (customer != null && customer.BAccountID > 0L)
			{
				bool isInserted = (this.BAccountAccessor.Cache.GetStatus(customer) == PXEntryStatus.Inserted);
				if (!isInserted)
				{
					CustomerBalanceSummary res = new CustomerBalanceSummary();

					CuryARHistory prepaymentbal = PXSelectJoinGroupBy<CuryARHistory,
						InnerJoin<ARCustomerBalanceEnq.ARLatestHistory, On<ARCustomerBalanceEnq.ARLatestHistory.accountID, Equal<CuryARHistory.accountID>,
							And<ARCustomerBalanceEnq.ARLatestHistory.branchID, Equal<CuryARHistory.branchID>,
							And<ARCustomerBalanceEnq.ARLatestHistory.customerID, Equal<CuryARHistory.customerID>,
							And<ARCustomerBalanceEnq.ARLatestHistory.subID, Equal<CuryARHistory.subID>,
							And<ARCustomerBalanceEnq.ARLatestHistory.curyID, Equal<CuryARHistory.curyID>,
							And<ARCustomerBalanceEnq.ARLatestHistory.lastActivityPeriod, Equal<CuryARHistory.finPeriodID>>>>>>>>,
						Where<CuryARHistory.customerID, Equal<Current<Customer.bAccountID>>>,
						Aggregate<
							Sum<CuryARHistory.finYtdDeposits,
							Sum<CuryARHistory.finYtdRetainageWithheld,
							Sum<CuryARHistory.finYtdRetainageReleased>>>>>.Select(this);

                    ARBalances bal = GetCustomerBalances<Override.Customer.sharedCreditCustomerID>(this, customer.BAccountID);

					decimal? balance = (bal?.CurrentBal)??0m;
					decimal? consolidatedBalance = balance;

					if (PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>())
					{
						consolidatedBalance = 0m;

						CustomerBalances consolidateBal =
						PXSelectJoinGroupBy<CustomerBalances,
						InnerJoin<Override.BAccount, On<Override.BAccount.bAccountID, Equal<CustomerBalances.customerID>>>,
						Where2<
							Where<Override.BAccount.consolidateToParent, Equal<True>, And<Override.BAccount.parentBAccountID, Equal<Required<Override.Customer.bAccountID>>>>
								, Or<Override.BAccount.bAccountID, Equal<Required<Customer.bAccountID>>>>
						, Aggregate<
							Sum<CustomerBalances.balance,
							Sum<CustomerBalances.unreleasedBalance,
							Sum<CustomerBalances.openOrdersBalance,
							Min<CustomerBalances.oldInvoiceDate>>>>>>
						.Select(this, customer.BAccountID, customer.BAccountID);

						consolidatedBalance = (consolidateBal?.Balance)??0m;

						CustomerBalances customerBalance =
						PXSelect<CustomerBalances,
							Where<CustomerBalances.customerID, Equal<Required<Customer.bAccountID>>>>
						.Select(this, customer.BAccountID);

						balance = (customerBalance?.Balance)??0m;
					}

					res.Balance = balance;
					res.ConsolidatedBalance = consolidatedBalance;
					res.UnreleasedBalance = bal.UnreleasedBal;
					res.OpenOrdersBalance = bal.TotalOpenOrders;
					res.DepositsBalance = prepaymentbal.FinYtdDeposits;
					res.RetainageBalance = prepaymentbal.FinYtdRetainageWithheld - prepaymentbal.FinYtdRetainageReleased;

					if (customer.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE || customer.CreditRule == CreditRuleTypes.CS_BOTH)
					{
						res.OldInvoiceDate = bal.OldInvoiceDate;
						TimeSpan overdue = (DateTime)Accessinfo.BusinessDate - (DateTime)(bal.OldInvoiceDate ?? Accessinfo.BusinessDate);

						if (customer.SharedCreditChild != true && (customer.CreditDaysPastDue ?? 0) < overdue.Days)
						{
							CustomerBalance.Cache.RaiseExceptionHandling<CustomerBalanceSummary.oldInvoiceDate>(res, res.OldInvoiceDate, 
								new PXSetPropertyException(Messages.CreditDaysPastDueWereExceeded, PXErrorLevel.Warning));
						}
					}
					else
					{
						res.OldInvoiceDate = null;
					}

					if (customer.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT || customer.CreditRule == CreditRuleTypes.CS_BOTH)
					{
						ARBalances remBal = bal;
						if (customer.SharedCreditChild == true)
						{
							remBal = GetCustomerBalances<Override.Customer.sharedCreditCustomerID>(this, customer.SharedCreditCustomerID);
						}

						res.RemainingCreditLimit = customer.CreditLimit - 
							((remBal.CurrentBal ?? 0) + (remBal.UnreleasedBal ?? 0) + (remBal.TotalOpenOrders ?? 0) + (remBal.TotalShipped ?? 0) - (remBal.TotalPrepayments ?? 0));
						if(res.RemainingCreditLimit <= decimal.Zero)
						{
							CustomerBalance.Cache.RaiseExceptionHandling<CustomerBalanceSummary.remainingCreditLimit>(res, res.RemainingCreditLimit, 
								new PXSetPropertyException(Messages.CreditLimitWasExceeded, PXErrorLevel.Warning));
						}
					}
					else
					{
						res.RemainingCreditLimit = decimal.Zero;
					}
					
					list.Add(res);
				}
			}
			return list;
		}

		public static ARBalances GetCustomerBalances<ParentField>(PXGraph graph, int? CustomerID)
			where ParentField : IBqlField
		{
			using (new PXReadBranchRestrictedScope())
			{
				return (ARBalances)PXSelectJoinGroupBy<ARBalances,
						InnerJoin<Override.Customer, On<Override.Customer.bAccountID, Equal<ARBalances.customerID>>>,
					Where<ParentField, Equal<Required<ParentField>>,
							Or<Override.Customer.bAccountID, Equal<Required<ARBalances.customerID>>>>,
					Aggregate<
					Sum<ARBalances.currentBal,
					Sum<ARBalances.totalOpenOrders,
					Sum<ARBalances.totalPrepayments,
					Sum<ARBalances.totalShipped,
					Sum<ARBalances.unreleasedBal,
					Min<ARBalances.oldInvoiceDate>>>>>>>>
					.Select(graph, CustomerID, CustomerID);
			}
		}

		[Obsolete]
		public static ARBalances GetCustomerBalances(PXGraph graph, int?[] familyIDs)
		{
			using (new PXReadBranchRestrictedScope())
			{
				return (ARBalances)PXSelectGroupBy<ARBalances,
					Where<ARBalances.customerID, In<Required<ARBalances.customerID>>>,
					Aggregate<
					Sum<ARBalances.currentBal,
					Sum<ARBalances.totalOpenOrders,
					Sum<ARBalances.totalPrepayments,
					Sum<ARBalances.totalShipped,
					Sum<ARBalances.unreleasedBal,
					Min<ARBalances.oldInvoiceDate>>>>>>>>.Select(graph, familyIDs);
			}
		}

		#endregion

		#region Child Accounts Info

		#region ARLatestHistory
		[Serializable]
		[PXProjection(typeof(Select4<CuryARHistory,
		 Aggregate<
		 GroupBy<CuryARHistory.branchID,
		 GroupBy<CuryARHistory.customerID,
		 GroupBy<CuryARHistory.accountID,
		 GroupBy<CuryARHistory.subID,
		 GroupBy<CuryARHistory.curyID,
		 Max<CuryARHistory.finPeriodID
		 >>>>>>>>))]
		public partial class ARLatestHistory : PX.Data.IBqlTable
		{
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.branchID))]
			[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
			public virtual Int32? BranchID { get; set; }
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.customerID))]
			public virtual Int32? CustomerID { get; set; }
			#endregion
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.accountID))]
			public virtual Int32? AccountID { get; set; }
			#endregion
			#region SubID
			public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.subID))]
			public virtual Int32? SubID { get; set; }
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(CuryARHistory.curyID))]
			public virtual String CuryID { get; set; }
			#endregion
			#region LastActivityPeriod
			public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
			[GL.FinPeriodID(BqlField = typeof(CuryARHistory.finPeriodID))]
			public virtual String LastActivityPeriod { get; set; }
			#endregion
		}
		#endregion

		#region CustomerPrepaymentBalances
		[Serializable]
		[PXProjection(typeof(Select5<CuryARHistory,
			InnerJoin<ARLatestHistory, On<ARLatestHistory.accountID, Equal<CuryARHistory.accountID>,
				And<ARLatestHistory.branchID, Equal<CuryARHistory.branchID>,
				And<ARLatestHistory.customerID, Equal<CuryARHistory.customerID>,
				And<ARLatestHistory.subID, Equal<CuryARHistory.subID>,
				And<ARLatestHistory.curyID, Equal<CuryARHistory.curyID>,
				And<ARLatestHistory.lastActivityPeriod, Equal<CuryARHistory.finPeriodID>>>>>>>>,
			Aggregate<
				GroupBy<CuryARHistory.customerID, 
					Sum<CuryARHistory.finYtdDeposits
		>>>>))]
		[PXHidden]
		public partial class CustomerPrepaymentBalances : PX.Data.IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.customerID))]
			public virtual Int32? CustomerID { get; set; }
			#endregion
			#region DepositsBalance
			public abstract class depositsBalance : PX.Data.BQL.BqlDecimal.Field<depositsBalance> { }

			[PXDBBaseCury(BqlField = typeof(CuryARHistory.finYtdDeposits))]
			public virtual Decimal? DepositsBalance { get; set; }
			#endregion
		}
		#endregion

		#region CustomerBalances
		[Serializable]
		[PXProjection(typeof(Select4<ARBalances,
					Aggregate<
					GroupBy<ARBalances.customerID,
						Sum<ARBalances.currentBal,
						Sum<ARBalances.totalOpenOrders,
						Sum<ARBalances.totalPrepayments,
						Sum<ARBalances.totalShipped,
						Sum<ARBalances.unreleasedBal,
						Min<ARBalances.oldInvoiceDate>>>>>>>>>
		))]
		[PXHidden]
		public partial class CustomerBalances : PX.Data.IBqlTable
			{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

			[PXDBInt(IsKey = true, BqlField = typeof(ARBalances.customerID))]
			public virtual Int32? CustomerID { get; set; }
			#endregion

			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }

			[PXDBBaseCury(BqlField = typeof(ARBalances.currentBal))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance", Visible = true, Enabled = false)]
			public virtual Decimal? Balance { get; set; }
			#endregion
			#region UnreleasedBalance
			public abstract class unreleasedBalance : PX.Data.BQL.BqlDecimal.Field<unreleasedBalance> { }

			[PXDBBaseCury(BqlField = typeof(ARBalances.unreleasedBal))]
			[PXUIField(DisplayName = "Unreleased Balance", Visible = true, Enabled = false)]
			public virtual Decimal? UnreleasedBalance { get; set; }
			#endregion
			#region OpenOrdersBalance
			public abstract class openOrdersBalance : PX.Data.BQL.BqlDecimal.Field<openOrdersBalance> { }

			[PXDBBaseCury(BqlField = typeof(ARBalances.totalOpenOrders))]
			[PXUIField(DisplayName = "Open Orders Balance", Visible = true, Enabled = false, FieldClass = "DISTR")]
			public virtual Decimal? OpenOrdersBalance { get; set; }
			#endregion
			#region OldInvoiceDate
			public abstract class oldInvoiceDate : PX.Data.BQL.BqlDateTime.Field<oldInvoiceDate> { }

			[PXDBDate(BqlField = typeof(ARBalances.oldInvoiceDate))]
			[PXUIField(DisplayName = "First Due Date", Visible = true, Enabled = false)]
			public virtual DateTime? OldInvoiceDate { get; set; }
			#endregion
			}
			#endregion

		#region ChildCustomerBalanceSummary
		[Serializable]
		[PXProjection(typeof(Select2<Override.Customer,
					InnerJoin<Override.BAccount,
						On<Override.Customer.bAccountID, Equal<Override.BAccount.bAccountID>>,
					LeftJoin<CustomerBalances,
						On<CustomerBalances.customerID, Equal<Override.BAccount.bAccountID>>,
					LeftJoin<CustomerPrepaymentBalances,
						On<CustomerPrepaymentBalances.customerID, Equal<Override.BAccount.bAccountID>>
					>>>,
					Where2<
						Where<
								Override.BAccount.bAccountID, Equal<CurrentValue<Customer.bAccountID>>,
							Or<Override.BAccount.parentBAccountID, Equal<CurrentValue<Customer.bAccountID>>>>,
						And<Override.Customer.bAccountID, IsNotNull
		>>>), Persistent = false)]
		[PXHidden]
		public partial class ChildCustomerBalanceSummary : IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

			protected Int32? _CustomerID;

			[Customer(Enabled = false, IsKey = true, BqlField = typeof(Override.Customer.bAccountID))]
			public virtual int? CustomerID
			{
				get { return _CustomerID; }
				set { _CustomerID = value; }
			}
			#endregion
			#region CustomerName
			public abstract class customerName : PX.Data.BQL.BqlString.Field<customerName> { }

			[PXDBString(60, IsUnicode = true, BqlField = typeof(Override.BAccount.acctName))]
			[PXUIField(DisplayName = "Customer Name", Enabled = false)]
			public virtual string CustomerName { get; set; }
			#endregion
			#region IsParent
			public abstract class isParent : PX.Data.BQL.BqlBool.Field<isParent> { }
			[PXBool]
			[PXDBCalced(typeof(Switch<Case<Where<Override.Customer.bAccountID, Equal<CurrentValue<Customer.bAccountID>>>, True>, False>), typeof(Boolean))]
			public virtual bool? IsParent { get; set; }
			#endregion
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }

			[PXDBString(10, IsUnicode = true, BqlField = typeof(Override.Customer.statementCycleId))]
			[PXUIField(DisplayName = "Statement Cycle", Enabled = false)]
			[PXSelector(typeof(ARStatementCycle.statementCycleId))]
			public virtual String StatementCycleId { get; set; }
			#endregion
			#region ConsolidateToParent
			public abstract class consolidateToParent : PX.Data.BQL.BqlBool.Field<consolidateToParent> { }

			[PXDBBool(BqlField = typeof(Override.BAccount.consolidateToParent))]
			[PXUIField(DisplayName = "Consolidate Balance", Enabled = false)]
			public virtual bool? ConsolidateToParent { get; set; }
			#endregion
			#region ConsolidateStatements
			public abstract class consolidateStatements : PX.Data.BQL.BqlBool.Field<consolidateStatements> { }

			[PXDBBool(BqlField = typeof(Override.Customer.consolidateStatements))]
			[PXUIField(DisplayName = "Consolidate Statements", Enabled = false)]
			public virtual bool? ConsolidateStatements { get; set; }
			#endregion
			#region SharedCreditPolicy
			public abstract class sharedCreditPolicy : PX.Data.BQL.BqlBool.Field<sharedCreditPolicy> { }

			[PXDBBool(BqlField = typeof(Override.Customer.sharedCreditPolicy))]
			[PXUIField(DisplayName = "Share Credit Policy")]
			public virtual bool? SharedCreditPolicy { get; set; }
			#endregion
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }

			protected Decimal? _Balance;

			[PXDBBaseCury(BqlField = typeof(CustomerBalances.balance))]
			[PXUIField(DisplayName = "Balance", Visible = true, Enabled = false)]
			public virtual Decimal? Balance { get { return _Balance ?? 0m; } set { _Balance = value; } }
			#endregion
			#region UnreleasedBalance
			public abstract class unreleasedBalance : PX.Data.BQL.BqlDecimal.Field<unreleasedBalance> { }

			protected Decimal? _UnreleasedBalance;

			[PXDBBaseCury(BqlField = typeof(CustomerBalances.unreleasedBalance))]
			[PXUIField(DisplayName = "Unreleased Balance", Visible = true, Enabled = false)]
			public virtual Decimal? UnreleasedBalance { get { return _UnreleasedBalance ?? 0m; } set { _UnreleasedBalance = value; } }
			#endregion
			#region OpenOrdersBalance
			public abstract class openOrdersBalance : PX.Data.BQL.BqlDecimal.Field<openOrdersBalance> { }

			protected Decimal? _OpenOrdersBalance;

			[PXDBBaseCury(BqlField = typeof(CustomerBalances.openOrdersBalance))]
			[PXUIField(DisplayName = "Open Orders Balance", Visible = true, Enabled = false, FieldClass = "DISTR")]
			public virtual Decimal? OpenOrdersBalance { get { return _OpenOrdersBalance ?? 0m; } set { _OpenOrdersBalance = value; } }
			#endregion
			#region DepositsBalance
			public abstract class depositsBalance : PX.Data.BQL.BqlDecimal.Field<depositsBalance> { }

			protected Decimal? _DepositsBalance;

			[PXDBBaseCury(BqlField = typeof(CustomerPrepaymentBalances.depositsBalance))]
			[PXUIField(DisplayName = "Prepayments Balance", Visible = true, Enabled = false)]
			public virtual Decimal? DepositsBalance { get { return _DepositsBalance ?? 0m;} set { _DepositsBalance = value; } }
			#endregion
			#region SignedDepositsBalance
			public abstract class signedDepositsBalance : PX.Data.BQL.BqlDecimal.Field<signedDepositsBalance> { }

			[PXBaseCury()]
			[PXUIField(DisplayName = "Prepayment Balance", Visible = true, Enabled = false)]
			public virtual Decimal? SignedDepositsBalance{ get { return (this.DepositsBalance * Decimal.MinusOne);} set { }}
			#endregion
			#region OldInvoiceDate
			public abstract class oldInvoiceDate : PX.Data.BQL.BqlDateTime.Field<oldInvoiceDate> { }

			[PXDBDate(BqlField = typeof(CustomerBalances.oldInvoiceDate))]
			[PXUIField(DisplayName = "First Due Date", Visible = true, Enabled = false)]
			public virtual DateTime? OldInvoiceDate { get; set; }
			#endregion
		}
		#endregion

		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<ChildCustomerBalanceSummary, OrderBy<Desc<ChildCustomerBalanceSummary.isParent>>> ChildAccounts;

		protected virtual IEnumerable childAccounts()
		{
			return null;
		}

		protected virtual IEnumerable<Customer> GetChildAccounts(bool sharedCreditPolicy = false, bool consolidateToParent = false, bool consolidateStatements = false)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>())
			{
				PXSelectBase<Customer> select = new PXSelect<Customer, Where<Customer.parentBAccountID, Equal<Current<Customer.bAccountID>>>>(this);
				if (sharedCreditPolicy)
		{
					select.WhereAnd<Where<Customer.sharedCreditPolicy, Equal<True>>>(); 
				}
				if (consolidateToParent)
				{
					select.WhereAnd<Where<Customer.consolidateToParent, Equal<True>>>(); 
				}
				if (consolidateStatements)
				{
					select.WhereAnd<Where<Customer.consolidateStatements, Equal<True>>>(); 
		}

				return select.Select().RowCast<Customer>();
			}
			
			return Enumerable.Empty<Customer>();
		}

		public static bool HasChildren<ParentField>(PXGraph graph, int? customerID) 
			where ParentField : IBqlField
		{
			Type select = BqlCommand.Compose(
				 typeof(Select2<,,>),
				typeof(Override.Customer),
				typeof(InnerJoin<,>),
				typeof(Override.BAccount),
				typeof(On<Override.Customer.bAccountID, Equal<Override.BAccount.bAccountID>>),
				typeof(Where<ParentField, Equal<Required<ParentField>>>)
			 );			
			return (new PXView(graph, true, BqlCommand.CreateInstance(select))).SelectSingle(customerID)!=null;
		}

        public static IEnumerable<Override.ExtendedCustomer> GetChildAccountsAndSelfStripped<ParentField>(PXGraph graph, int? customerID)
			where ParentField : IBqlField
        {
			Type select = BqlCommand.Compose(
				typeof(Select2<,,>),
				typeof(Override.Customer),
				typeof(InnerJoin<,>),
				typeof(Override.BAccount),
				typeof(On<Override.Customer.bAccountID, Equal<Override.BAccount.bAccountID>>),
				typeof(Where<,,>),
				//Create a new graph instance to retrieve a specific type of the cache.
				//graph.Caches[] does not give us this capability, because it can returns a child type
				//instead of the required type, if it already exists in the graph.Caches[].
				FetchEmptyGraph().Caches[BqlCommand.GetItemType(typeof(ParentField))].GetBqlField(nameof(CR.BAccount.bAccountID)),
				typeof(Equal<Required<Customer.bAccountID>>),
				typeof(Or<ParentField, Equal<Required<ParentField>>>)
				);

			return new PXView(graph, true, BqlCommand.CreateInstance(select))
						.SelectMulti(customerID, customerID)
                .Cast<PXResult<Override.Customer, Override.BAccount>>()
                .Select(result => new Override.ExtendedCustomer(result, result));
        }

		private static PXGraph FetchEmptyGraph()
		{
			PXGraph emptyGraph = PXContext.GetSlot<PXGraph>(typeof(Override.Customer).Name);
			if (emptyGraph == null)
			{
				PXContext.SetSlot<PXGraph>(typeof(Override.Customer).Name, emptyGraph = CreateInstance<PXGraph>());
			}
			return emptyGraph;
		}
		#endregion

		public override void Persist()
		{
			if (CurrentCustomer.Current != null
				&& CurrentCustomer.Cache.GetStatus(CurrentCustomer.Current) == PXEntryStatus.Updated)
			{
				bool errorsExist = false;

				foreach (LocationExtAddress location in Locations.Select())
				{
					PXEntryStatus locationStatus = Locations.Cache.GetStatus(location);

					if (locationStatus != PXEntryStatus.Updated && locationStatus != PXEntryStatus.Inserted)
					{
						errorsExist |= !ValidateLocation(Locations.Cache, location);
					}
				}
				if (errorsExist)
				{
					throw new PXException(Messages.CustomerLocationContainErrors);
				}

				if (CurrentCustomer.Current != null)
				{
					Customer customer = (Customer)CurrentCustomer.Current;			
					var customerValidationHelper = new AccountAndSubValidationHelper(CurrentCustomer.Cache, customer);

					errorsExist |= !customerValidationHelper.SetErrorIfInactiveAccount<Customer.discTakenAcctID>(customer.DiscTakenAcctID)
						.SetErrorIfInactiveSubAccount<Customer.discTakenSubID>(customer.DiscTakenSubID)
						.SetErrorIfInactiveAccount<Customer.prepaymentAcctID>(customer.PrepaymentAcctID)
						.SetErrorIfInactiveSubAccount<Customer.prepaymentSubID>(customer.PrepaymentSubID)
						.IsValid;
				}

				if (errorsExist)
				{
					throw new PXException(Common.Messages.RecordCanNotBeSaved);
				}
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				//assuming only one instance of CustomerPaymentMethodC could be inserted at a time
				if (DefPaymentMethodInstance.Cache.Inserted.Count() > 0)
				{
					IEnumerator cpmEnumerator = DefPaymentMethodInstance.Cache.Inserted.GetEnumerator();
					if (cpmEnumerator.MoveNext())
					{
						CustomerPaymentMethod current = cpmEnumerator.Current as CustomerPaymentMethod;
						if (current != null && CCProcessingHelper.IsTokenizedPaymentMethod(this, current.PMInstanceID))
						{
							var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
							ICCPaymentProfileAdapter paymentProfile = new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(DefPaymentMethodInstance);
							ICCPaymentProfileDetailAdapter profileDetail = new GenericCCPaymentProfileDetailAdapter<CustomerPaymentMethodDetail, 
								PaymentMethodDetail>(DefPaymentMethodInstanceDetailsAll, PMDetails);
							graph.GetOrCreatePaymentProfile(this, paymentProfile,profileDetail);
						}
					}
				}
				try
				{
					base.Persist(typeof(Customer), PXDBOperation.Update);
				}
				catch
				{
					Caches[typeof(Customer)].Persisted(true);
					throw;
				}
				base.Persist();

				var customer = CurrentCustomer.Current;
				if (customer != null)
				{
					if (customer.StatementCustomerID == null || customer.StatementCustomerID < 0)
				{
					PXDatabase.Update<Customer>(
							new PXDataFieldAssign<Customer.statementCustomerID>(customer.BAccountID),
							new PXDataFieldRestrict<Customer.bAccountID>(customer.BAccountID),
							PXDataFieldRestrict.OperationSwitchAllowed);

					CurrentCustomer.Cache.SetValue<Customer.statementCustomerID>(customer, customer.BAccountID);
					SelectTimeStamp();
				}

					if(customer.ConsolidatingBAccountID == null || customer.ConsolidatingBAccountID < 0)
				{
					PXDatabase.Update<BAccount>(
							new PXDataFieldAssign<BAccount.consolidatingBAccountID>(customer.BAccountID),
							new PXDataFieldRestrict<BAccount.bAccountID>(customer.BAccountID),
							PXDataFieldRestrict.OperationSwitchAllowed);

					CurrentCustomer.Cache.SetValue<Customer.consolidatingBAccountID>(customer, customer.BAccountID);
					SelectTimeStamp();
				}

					if (customer.SharedCreditCustomerID == null || customer.SharedCreditCustomerID < 0)
					{
						PXDatabase.Update<Customer>(
							new PXDataFieldAssign<Customer.sharedCreditCustomerID>(customer.BAccountID),
							new PXDataFieldRestrict<Customer.bAccountID>(customer.BAccountID),
							PXDataFieldRestrict.OperationSwitchAllowed);

						CurrentCustomer.Cache.SetValue<Customer.sharedCreditCustomerID>(customer, customer.BAccountID);
						SelectTimeStamp();
					}
				}

				ts.Complete();
			}
		}

		#region Customer events

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<LocationExtAddress.locationID,
			Where<LocationExtAddress.bAccountID,
			Equal<Current<Customer.bAccountID>>>>),
			DescriptionField = typeof(LocationExtAddress.locationCD),
			DirtyRead = true)]
		protected virtual void Customer_DefLocationID_CacheAttached(PXCache sender)
		{
			
		}

		protected virtual void Customer_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
            PXRowInserting inserting = delegate (PXCache sender, PXRowInsertingEventArgs args)
			{
				Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
				if (branch != null)
				{
					object cd = branch.BranchCD;
					this.Locations.Cache.RaiseFieldUpdating<LocationExtAddress.locationCD>(args.Row, ref cd);
					((LocationExtAddress)args.Row).LocationCD = (string)cd;
				}
			};

			if (CustomerClass.Current != null && CustomerClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.AddHandler<LocationExtAddress>(inserting);
			}

			base.OnBAccountRowInserted(cache, e);

			if (CustomerClass.Current != null && CustomerClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.RemoveHandler<LocationExtAddress>(inserting);
			}

			bool needUpdate = false;
			Customer customer = (Customer)e.Row;
			if (customer.DefBillAddressID == null)
			{
				customer.DefBillAddressID = customer.DefAddressID;
				needUpdate = true;
			}
			if (customer.DefBillContactID == null)
			{
				customer.DefBillContactID = customer.DefContactID;
				needUpdate = true;
			}
			if (customer.CustomerClassID != null)
			{
				cache.SetDefaultExt<Customer.defPaymentMethodID>(customer);
				this.DefPaymentMethodInstance.Cache.IsDirty = false;
				this.DefPaymentMethodInstanceDetails.Cache.IsDirty = false;
			}

			if (needUpdate)
				this.BAccountAccessor.Cache.Update(customer);

            CustomerClassDefaultInserting();
        }

        private void CustomerClassDefaultInserting()
        {
            //errors are possible on insert, we should not update other values if this insert caused errors
            if (CustomerClass.Current?.SalesPersonID != null)
            {
                CustSalesPeople sperson = new CustSalesPeople { SalesPersonID = CustomerClass.Current.SalesPersonID, IsDefault = true };
                try
                {
                    SalesPersons.Insert(sperson);
                }
                catch (PXCustSalesPersonException)
                {

                }
            }
		}

		protected virtual void Customer_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Customer row = (Customer)e.Row;

			if (row == null)
				return;

			if (!row.DefBillContactID.HasValue)
			{
				row.DefBillContactID = row.DefContactID;
			}

			row.IsBillContSameAsMain = row.DefBillContactID == row.DefContactID;
			row.IsBillSameAsMain = row.DefBillAddressID == row.DefAddressID;

			PXEntryStatus status = cache.GetStatus(row);
			bool isExistingRecord = !(status == PXEntryStatus.Inserted && row.AcctCD == null);

			if (!isExistingRecord)
			{
				//Actions
				newInvoiceMemo.SetEnabled(false);
				newSalesOrder.SetEnabled(false);
				newPayment.SetEnabled(false);
				regenerateLastStatement.SetEnabled(false);
				ChangeID.SetEnabled(false);
				//Inquiries
				customerDocuments.SetEnabled(false);
				statementForCustomer.SetEnabled(false);
				salesPrice.SetEnabled(false);
				//Reports
				aRBalanceByCustomer.SetEnabled(false);
				customerHistory.SetEnabled(false);
				aRAgedPastDue.SetEnabled(false);
				aRAgedOutstanding.SetEnabled(false);
				aRRegister.SetEnabled(false);
				customerDetails.SetEnabled(false);
				customerStatement.SetEnabled(false);

				viewRestrictionGroups.SetEnabled(false);
				viewBusnessAccount.SetEnabled(false);
				generateOnDemandStatement.SetEnabled(false);
			}
			if (isExistingRecord && row.Status == CR.BAccount.status.Active)
			{
				//Actions
				newInvoiceMemo.SetEnabled(true);
				newSalesOrder.SetEnabled(true);
				newPayment.SetEnabled(true);
				regenerateLastStatement.SetEnabled(true);
				ChangeID.SetEnabled(true);
				//Inquiries
				customerDocuments.SetEnabled(true);
				statementForCustomer.SetEnabled(true);
				salesPrice.SetEnabled(true);
				//Reports
				aRBalanceByCustomer.SetEnabled(true);
				customerHistory.SetEnabled(true);
				aRAgedPastDue.SetEnabled(true);
				aRAgedOutstanding.SetEnabled(true);
				aRRegister.SetEnabled(true);
				customerDetails.SetEnabled(true);
				customerStatement.SetEnabled(true);

				viewRestrictionGroups.SetEnabled(true);
				viewBusnessAccount.SetEnabled(true);
				generateOnDemandStatement.SetEnabled(true);
			}
			viewVendor.SetEnabled((row.Type == BAccountType.VendorType || row.Type == BAccountType.CombinedType) && isExistingRecord);
			extendToVendor.SetEnabled(!(row.Type == BAccountType.VendorType || row.Type == BAccountType.CombinedType) && isExistingRecord);

			bool smallBalanceAllow = (row.SmallBalanceAllow ?? false);
			PXUIFieldAttribute.SetEnabled<Customer.smallBalanceLimit>(cache, row, smallBalanceAllow);
			if (!smallBalanceAllow) row.SmallBalanceLimit = 0;

			bool mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<Customer.curyID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<Customer.curyRateTypeID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<Customer.printCuryStatements>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<Customer.allowOverrideCury>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<Customer.allowOverrideRate>(cache, null, mcFeatureInstalled);

			bool hasChildren = PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>() &&
								HasChildren<Override.BAccount.parentBAccountID>(this, row.BAccountID);

			ChildAccounts.AllowSelect = hasChildren;
			ChildAccounts.AllowInsert = false;
			ChildAccounts.AllowUpdate = false;
			ChildAccounts.AllowDelete = false;

			bool isLegacyParentWithParent = row.ParentBAccountID != null && hasChildren;
			PXUIFieldAttribute.SetEnabled<Customer.parentBAccountID>(cache, row, row.ParentBAccountID != null || hasChildren == false);
			PXUIFieldAttribute.SetEnabled<Customer.consolidateToParent>(cache, row, isLegacyParentWithParent == false);
			PXUIFieldAttribute.SetEnabled<Customer.consolidateStatements>(cache, row, isLegacyParentWithParent == false);

			PXUIFieldAttribute.SetVisible<CustomerBalanceSummary.consolidatedbalance>(CustomerBalance.Cache, null, hasChildren);
			PXUIFieldAttribute.SetVisible<CustomerBalanceSummary.signedDepositsBalance>(CustomerBalance.Cache, null, hasChildren == false);

			bool creditRuleBoth = (row.CreditRule == CreditRuleTypes.CS_BOTH);
			PXUIFieldAttribute.SetEnabled<Customer.sharedCreditPolicy>(cache, row, row.ConsolidateToParent == true);
			PXUIFieldAttribute.SetEnabled<Customer.creditRule>(cache, row, row.SharedCreditChild != true);
			PXUIFieldAttribute.SetEnabled<Customer.creditLimit>(cache, row, row.SharedCreditChild != true && (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT || creditRuleBoth));
			PXUIFieldAttribute.SetEnabled<Customer.creditDaysPastDue>(cache, row, row.SharedCreditChild != true && (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE || creditRuleBoth));

			PXUIFieldAttribute.SetEnabled<Customer.printDunningLetters>(cache, row, row.SharedCreditChild != true);
			PXUIFieldAttribute.SetEnabled<Customer.mailDunningLetters>(cache, row, row.SharedCreditChild != true);

			PXUIFieldAttribute.SetEnabled<Customer.sendStatementByEmail>(cache, row, row.StatementChild != true);
			PXUIFieldAttribute.SetEnabled<Customer.printStatements>(cache, row, row.StatementChild != true);
			PXUIFieldAttribute.SetEnabled<Customer.statementType>(cache, row, row.StatementChild != true);
			PXUIFieldAttribute.SetEnabled<Customer.printCuryStatements>(cache, row, row.StatementChild != true);

			bool retainageApply = row.RetainageApply == true;
			PXUIFieldAttribute.SetEnabled<Customer.retainagePct>(cache, row, retainageApply);

			writeOffBalance.SetEnabled((row.SmallBalanceAllow ?? false) && isExistingRecord 
										&& row.Status != CR.BAccount.status.Hold 
										&& row.Status != CR.BAccount.status.Inactive);

			LocationExtAddress defLocation = DefLocation.Select();

			if (defLocation != null && this.CustomerClass.Current != null)
			{
				bool isRequired = (this.CustomerClass.Current.RequireTaxZone ?? false);
				PXDefaultAttribute.SetPersistingCheck<LocationExtAddress.cTaxZoneID>(this.DefLocation.Cache, defLocation, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<LocationExtAddress.cTaxZoneID>(this.DefLocation.Cache, isRequired);
				if (isRequired && string.IsNullOrEmpty(defLocation.CTaxZoneID))
				{
					this.DefLocation.Cache.MarkUpdated(defLocation);
				}
			}

			CustomerPaymentMethod cpm = DefPaymentMethodInstance.Select(CurrentCustomer.Current.DefPMInstanceID);
			bool cpmInserted = (DefPaymentMethodInstance.Cache.GetStatus(cpm) == PXEntryStatus.Inserted);
			bool enablePMEdit = cpm != null;
			this.PaymentMethodDef.Cache.RaiseRowSelected(PaymentMethodDef.Current);
			this.DefPaymentMethodInstance.Cache.AllowUpdate = enablePMEdit;
			this.DefPaymentMethodInstanceDetails.Cache.AllowUpdate = enablePMEdit && cpmInserted; //&& !CCProcessingUtils.isTokenizedPaymentMethod(this, row.DefPMInstanceID);

			if (row.ParentBAccountID != null)
			{
				Override.Customer parent = PXSelect<
					Override.Customer, 
					Where<
						Override.Customer.bAccountID, Equal<Required<Override.Customer.bAccountID>>>>
					.Select(this, row.ParentBAccountID);

				cache.RaiseExceptionHandling<Customer.statementCycleId>(row, row.ParentBAccountID,
					parent != null && row.ConsolidateToParent == true && parent.StatementCycleId != row.StatementCycleId ?
					new PXSetPropertyException<Customer.statementCycleId>(Messages.StatementCycleShouldBeTheSameOnParentAndChildCustomer, PXErrorLevel.Warning) :
					null);
			}
		}

		protected virtual void Customer_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			Customer row = e.Row as Customer;
			if (row != null && row.Type == BAccountType.CombinedType)
			{
				// We shouldn't delete BAccount entity when it is in use by Vendor entity
				PXTableAttribute tableAttr = cache.Interceptor as PXTableAttribute;
				tableAttr.BypassOnDelete(typeof(BAccount));
				PXNoteAttribute.ForceRetain<Customer.noteID>(cache);
				PXParentAttribute.SetLeaveChildren<Location.bAccountID>(this.Caches[typeof(Location)], null, true);
			}
		}

		protected virtual void Customer_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			Customer row = e.Row as Customer;
			if (row != null && row.Type == BAccountType.CombinedType)
				ChangeBAccountType(row, BAccountType.VendorType);
		}

		protected virtual void Customer_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row == null)
				return;

			if (e.Operation != PXDBOperation.Delete)
			{
				if (row.SendStatementByEmail == true || row.MailInvoices == true || (row.MailDunningLetters == true && PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>()))
				{
					Contact contact = PXSelect<Contact,
						Where<Contact.bAccountID, Equal<Required<Customer.bAccountID>>,
							And<Contact.contactID, Equal<Required<Customer.defBillContactID>>>>>
						.Select(this, row.BAccountID, row.DefBillContactID);

					PXCache contactCache = BillContact.Cache;
					if (contact != null && String.IsNullOrEmpty(contact.EMail))
					{
						contactCache.MarkUpdated(contact);
						RaiseEmailErrors(cache, contactCache, contact, row);
					}
				}

				if (row.ParentBAccountID == null && row.ConsolidateToParent != true && row.SharedCreditPolicy == true)
				{
					row.SharedCreditPolicy = false;
				}

				if (row.ParentBAccountID != null && (row.ConsolidateToParent == true || row.ConsolidateStatements == true))
				{
					var parent = (BAccount)PXSelectorAttribute.Select<Customer.parentBAccountID>(cache, row);

					if (parent != null && parent.IsCustomerOrCombined != true)
					{
						cache.RaiseExceptionHandling<Customer.parentBAccountID>(row, cache.GetValueExt<Customer.parentBAccountID>(row),
							new PXSetPropertyException<Customer.parentBAccountID>(Messages.ConsolidatingCustomersParentMustBeCustomer));
					}

					if (parent != null && parent.ParentBAccountID != null)
					{
						cache.RaiseExceptionHandling<Customer.parentBAccountID>(row, cache.GetValueExt<Customer.parentBAccountID>(row),
							new PXSetPropertyException<Customer.parentBAccountID>(Messages.ConsolidatingCustomersParentMustNotBeChild));
					}
				}

				if (row.ParentBAccountID != null &&
					row.ConsolidateToParent == true &&
					row.ConsolidateStatements != true &&
					row.StatementType == ARStatementType.CS_BALANCE_BROUGHT_FORWARD)
				{
					cache.RaiseExceptionHandling<Customer.consolidateStatements>(row, row.ConsolidateStatements,
						new PXSetPropertyException<Customer.consolidateStatements>(Messages.ChildCustomerShouldConsolidateBBFStatements));
				}

				Customer unchangedCustomer = PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.BAccountID);
				if (unchangedCustomer != null && unchangedCustomer.ParentBAccountID != null && unchangedCustomer.ConsolidateToParent == true
					&& cache.ObjectsEqual<Customer.consolidateToParent, Customer.parentBAccountID>(row, unchangedCustomer) == false)
				{
					VerifyUnreleasedParentChildApplications(cache, row);
				}

				if (unchangedCustomer != null && unchangedCustomer.ParentBAccountID != null && unchangedCustomer.ParentBAccountID != row.ParentBAccountID
					&& unchangedCustomer.ConsolidateStatements == true && unchangedCustomer.ConsolidateToParent == true
					&& String.IsNullOrEmpty(PXUIFieldAttribute.GetError<Customer.parentBAccountID>(cache, row)))
				{
					Customer oldParent = PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, unchangedCustomer.ParentBAccountID);
					Customer newParent = PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.ParentBAccountID);

					if (oldParent != null && oldParent.StatementType == ARStatementType.CS_BALANCE_BROUGHT_FORWARD
					|| newParent != null && newParent.StatementType == ARStatementType.CS_BALANCE_BROUGHT_FORWARD
					|| row.StatementType == ARStatementType.CS_BALANCE_BROUGHT_FORWARD)
					{
						cache.RaiseExceptionHandling<Customer.parentBAccountID>(row, row.StatementType,
							new PXSetPropertyException<Customer.parentBAccountID>(Messages.StatementTypeShouldBeOpenItemForParentChildAfterSeparation, PXErrorLevel.Warning));
					}
				}

				VerifyParentBAccountID<Customer.parentBAccountID>(this, cache, row, row);
			}
		}

		public static void VerifyParentBAccountID<ParentField>(PXGraph graph, PXCache cache, Customer customer, BAccount row)
			where ParentField : IBqlField
		{
			if (customer != null && row.ParentBAccountID != row.BAccountID && (row.ConsolidateToParent == true || customer.ConsolidateStatements == true))
			{
				PXResultset<Customer, CustomerMaster> parents =
					SelectFrom<Customer>
					.LeftJoin<CustomerMaster>
						.On<Customer.parentBAccountID.IsEqual<CustomerMaster.bAccountID>>
					.Where<Customer.bAccountID.IsEqual<@P.AsInt>>
					.View
					.SelectWindowed<PXResultset<Customer, CustomerMaster>>(graph, 0, 1, row.ParentBAccountID);

				Customer parent = (Customer)parents;
				CustomerMaster parentOfParent = (CustomerMaster)parents;
				PXSetPropertyException PropertyException = null;

				if (parent != null && (parent.ConsolidateToParent == true || parent.ConsolidateStatements == true) && parentOfParent != null && parentOfParent.BAccountID != null)
				{
					PropertyException =
						new PXSetPropertyException(
							Messages.CustomerWithParentAccountsForWhichBalanceConsolidationEnabled,
							PXErrorLevel.RowError,
							parent.AcctCD,
							customer.AcctCD);
				}
				else if (parent != null)
				{
					bool hasConsolidatedChild =
						SelectFrom<Customer>
						.Where<Customer.parentBAccountID.IsEqual<@P.AsInt>
							.And<Customer.consolidateToParent.IsEqual<True>
								.Or<Customer.consolidateStatements.IsEqual<True>>>>
						.View
						.SelectWindowed(graph, 0, 1, customer.BAccountID)
						.Any();
					if (hasConsolidatedChild)
					{
						PropertyException =
							new PXSetPropertyException(
								Messages.CustomerWithChildAccountsForWhichBalanceConsolidationEnabled,
								PXErrorLevel.RowError,
								parent.AcctCD,
								customer.AcctCD);
					}
				}

				if (PropertyException != null)
				{
					cache.RaiseExceptionHandling<ParentField>(
						row,
						customer.ParentBAccountID,
						PropertyException);
				}
			}
		}

		private void VerifyUnreleasedParentChildApplications(PXCache cache, Customer customer)
		{
			var unreleasedParentChildApplications = PXSelectGroupBy<ARAdjust,
				Where<ARAdjust.adjdCustomerID, Equal<Required<ARAdjust.adjdCustomerID>>,
					And<ARAdjust.adjdCustomerID, NotEqual<ARAdjust.customerID>,
					And<ARAdjust.released, NotEqual<True>>>>,
				Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr>>>>
				.Select(this, customer.BAccountID).RowCast<ARAdjust>();

			if (unreleasedParentChildApplications.Any())
			{
				var applicationsNames = String.Join(", ", unreleasedParentChildApplications
					.Select(a => PXMessages.LocalizeNoPrefix(a.AdjgDocType) + " " + a.AdjgRefNbr).ToArray());

				cache.RaiseExceptionHandling<Customer.parentBAccountID>(customer, cache.GetValueExt<Customer.parentBAccountID>(customer),
					new PXSetPropertyException<Customer.parentBAccountID>(Messages.CustomerRelationshipCannotBeBroken, applicationsNames));
			}
		}

		private void RaiseEmailErrors(PXCache persistingCache, PXCache contactCache, Contact contact, Customer customer)
		{
			bool ok = true;
			string statementsFieldName = PXUIFieldAttribute.GetDisplayName<Customer.sendStatementByEmail>(persistingCache);
			string invoicesFieldName = PXUIFieldAttribute.GetDisplayName<Customer.mailInvoices>(persistingCache);
			string dunningLetterFieldName = PXUIFieldAttribute.GetDisplayName<Customer.mailDunningLetters>(persistingCache);
			string allFields = PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>()
				? String.Join(", ", statementsFieldName, invoicesFieldName, dunningLetterFieldName)
				: String.Join(", ", statementsFieldName, invoicesFieldName);

			if (customer.MailInvoices == true)
			{
				ok = false;
				persistingCache.RaiseExceptionHandling<Customer.mailInvoices>(customer, customer.SendStatementByEmail, 
					new PXSetPropertyException(Messages.ERR_EmailIsRequiredForOption, PXErrorLevel.Error, invoicesFieldName));
			}
			if (customer.StatementChild != true && customer.SendStatementByEmail == true)
			{
				ok = false;
				persistingCache.RaiseExceptionHandling<Customer.sendStatementByEmail>(customer, customer.SendStatementByEmail, 
					new PXSetPropertyException(Messages.ERR_EmailIsRequiredForOption, PXErrorLevel.Error, statementsFieldName));
			}
			if (customer.SharedCreditChild != true && customer.MailDunningLetters == true && PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>())
			{
				ok = false;
				persistingCache.RaiseExceptionHandling<Customer.mailDunningLetters>(customer, customer.MailDunningLetters, 
					new PXSetPropertyException(Messages.ERR_EmailIsRequiredForOption, PXErrorLevel.Error, dunningLetterFieldName));
			}

			if (!ok)
			{
				contactCache.RaiseExceptionHandling<Contact.eMail>(contact, contact.EMail,
					new PXSetPropertyException(Messages.ERR_EmailIsRequiredForSendByEmailOptions, PXErrorLevel.Error, allFields));
			}
			}

		protected virtual void Customer_MailDunningLetters_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			Customer row = (Customer)e.Row;
			CheckExcludedFromDunning(cache, row);
		}

		protected virtual void Customer_PrintDunningLetters_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			Customer row = (Customer)e.Row;
			CheckExcludedFromDunning(cache, row);
		}

		protected virtual void Customer_MailDunningLetters_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			CheckExcludedFromDunning(cache, row);
			UpdateChildAccounts<Customer.mailDunningLetters>(cache, row, GetChildAccounts(sharedCreditPolicy: true));
		}

		protected virtual void Customer_PrintDunningLetters_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			CheckExcludedFromDunning(cache, row);
			UpdateChildAccounts<Customer.printDunningLetters>(cache, row, GetChildAccounts(sharedCreditPolicy: true));
		}

		private static void CheckExcludedFromDunning(PXCache cache, Customer row)
		{
			if (row != null && 
				row.MailDunningLetters == false && 
				row.PrintDunningLetters == false &&
				PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>() &&
				row.SharedCreditChild != true)
					{
						cache.RaiseExceptionHandling<Customer.mailDunningLetters>(row, row.MailDunningLetters, new PXSetPropertyException(Messages.DunningLetterExcludedCustomer, PXErrorLevel.Warning));
						cache.RaiseExceptionHandling<Customer.printDunningLetters>(row, row.PrintDunningLetters, new PXSetPropertyException(Messages.DunningLetterExcludedCustomer, PXErrorLevel.Warning));
					}
				}

		protected virtual void Customer_SendStatementByEmail_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			UpdateChildAccounts<Customer.sendStatementByEmail>(cache, (Customer)e.Row, GetChildAccounts(consolidateStatements: true));
			}

		protected virtual void Customer_PrintStatements_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			UpdateChildAccounts<Customer.printStatements>(cache, (Customer)e.Row, GetChildAccounts(consolidateStatements: true));
		}

		protected virtual void Customer_StatementType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			UpdateChildAccounts<Customer.statementType>(cache, (Customer)e.Row, GetChildAccounts(consolidateStatements: true));
		}

		protected virtual void Customer_PrintCuryStatements_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			UpdateChildAccounts<Customer.printCuryStatements>(cache, (Customer)e.Row, GetChildAccounts(consolidateStatements: true));
		}

		protected virtual void Customer_AcctName_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			base.OnBAccountAcctNameFieldUpdated(cache, e);
		}

		protected virtual void Customer_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				if (cmpany.Current == null || string.IsNullOrEmpty(cmpany.Current.BaseCuryID))
				{
					throw new PXException();
				}
				e.NewValue = cmpany.Current.BaseCuryID;
				e.Cancel = true;
			}
		}

		protected virtual void Customer_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				e.Cancel = true;
			}
		}

		protected virtual void Customer_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				e.Cancel = true;
			}
		}

		protected virtual void Customer_ParentBAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row.ParentBAccountID != null)
			{
				Customer parent = GetCustomerParent(row);
				if(parent != null)
				{
					sender.SetValueExt<Customer.consolidateToParent>(row, parent.ConsolidateToParent);
					sender.SetValueExt<Customer.consolidateStatements>(row, parent.ConsolidateStatements);
					sender.SetValueExt<Customer.sharedCreditPolicy>(row, parent.SharedCreditPolicy);
				}
			}
			else if (e.OldValue != null)
			{
				row.ConsolidateToParent = false;
				row.ConsolidateStatements = false;
				row.SharedCreditPolicy = false;
				row.CreditLimit = 0m;
			}
		}

		protected virtual void Customer_ConsolidateToParent_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row == null) return;

			if (row.ParentBAccountID == null)
		{
				IEnumerable<Customer> childs;
				string message = PXMessages.LocalizeFormatNoPrefix(Messages.RelatedFieldChangeOnParentWarning, 
					PXUIFieldAttribute.GetDisplayName<Customer.consolidateToParent>(sender));

				if ((childs = GetChildAccounts()).Any() && e.ExternalCall)
				{
					if (CurrentCustomer.Ask(message, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
					UpdateChildAccounts<Customer.consolidateToParent>(sender, row, childs);
			}
				}

				row.SharedCreditPolicy &= row.ConsolidateToParent;
		}
			else if (row.SharedCreditPolicy == true && row.ConsolidateToParent != true && (bool?)e.OldValue == true)
			{
				sender.SetValueExt<Customer.sharedCreditPolicy>(row, false);
			}
		}

		protected virtual void Customer_ConsolidateStatements_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row == null) return;

			if (row.ParentBAccountID == null)
		{
					IEnumerable<Customer> childs;
					string message = PXMessages.LocalizeFormatNoPrefix(Messages.RelatedFieldChangeOnParentWarning, 
						PXUIFieldAttribute.GetDisplayName<Customer.consolidateStatements>(sender));

				if ((childs = GetChildAccounts()).Any() && e.ExternalCall)
				{
					if (CurrentCustomer.Ask(message, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
						UpdateChildAccounts<Customer.consolidateStatements>(sender, row, childs);
			}
		}
			}
			else if (row.ConsolidateStatements == true)
		{
				Customer parent = GetCustomerParent(row);
				if (parent != null)
			{
					row.SendStatementByEmail = parent.SendStatementByEmail;
					row.PrintStatements = parent.PrintStatements;
					row.StatementType = parent.StatementType;
					row.PrintCuryStatements = parent.PrintCuryStatements;
				}
			}
		}

		protected virtual void Customer_SharedCreditPolicy_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row == null) return;

			if (row.ParentBAccountID == null)
			{
				IEnumerable<Customer> childs;
				string message = PXMessages.LocalizeFormatNoPrefix(Messages.RelatedFieldChangeOnParentWarning, 
					PXUIFieldAttribute.GetDisplayName<Customer.sharedCreditPolicy>(sender));

				if ((childs = GetChildAccounts(consolidateToParent: true)).Any() && e.ExternalCall)
				{
					if (CurrentCustomer.Ask(message, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
					UpdateChildAccounts<Customer.sharedCreditPolicy>(sender, row, childs);
				}
			}
			}
			else if (row.SharedCreditPolicy == true)
				{
				Customer parent = GetCustomerParent(row);
				if (parent != null)
					{
					Func<Customer, bool> func;
					string childStatus = GetSharedCreditChildStatus(parent.Status, out func);
					row.Status = func.Invoke(row) ? childStatus : row.Status;

					row.CreditRule = parent.CreditRule;
					row.CreditLimit = parent.CreditLimit;
					row.CreditDaysPastDue = parent.CreditDaysPastDue;

					row.PrintDunningLetters = parent.PrintDunningLetters;
					row.MailDunningLetters = parent.MailDunningLetters;
					}
				}
			else if ((bool?)e.OldValue == true)
			{
				row.CreditLimit = 0m;
			}
		}

		protected virtual void Customer_IsBillSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer owner = (Customer)e.Row;
			if(owner == null) return;

			if (owner.IsBillSameAsMain == true)
			{
				if (owner.DefBillAddressID != owner.DefAddressID)
				{
					Address extAddr = FindAddress(owner.DefBillAddressID);
					if (extAddr != null && extAddr.AddressID == owner.DefBillAddressID)
					{
						Address defAddr = FindAddress(owner.DefAddressID);
						defAddr.RevisionID = defAddr.RevisionID + 1;
						DefAddress.Cache.Update(defAddr);

						BillAddress.Delete(extAddr);
					}
					owner.DefBillAddressID = owner.DefAddressID;
				}
			}
			else if (owner.DefBillAddressID != null && owner.DefBillAddressID == owner.DefAddressID)
			{
				Address addr = PXCache<Address>.CreateCopy(FindAddress(owner.DefAddressID));
				addr.AddressID = null;
				addr.RevisionID++;
				addr = BillAddress.Insert(addr);
				owner.DefBillAddressID = addr.AddressID;
			}
		}

		protected virtual void Customer_IsBillContSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row != null)
			{
				if (row.IsBillContSameAsMain == true)
				{
					if (row.DefBillContactID != row.DefContactID)
					{
						Contact contact = FindContact(row.DefBillContactID);
						if (contact != null && contact.ContactID == row.DefBillContactID)
						{
							BillContact.Delete(contact);
						}
						row.DefBillContactID = row.DefContactID;
					}
				}
				else if (row.DefBillContactID != null && row.DefBillContactID == row.DefContactID)
				{
					Contact cont = PXCache<Contact>.CreateCopy(FindContact(row.DefContactID));
					cont.ContactID = null;
					cont.NoteID = null;
					cont.BAccountID = row.BAccountID;
					cont.ContactType = ContactTypesAttribute.BAccountProperty;
					cont = BillContact.Insert(cont);
					row.DefBillContactID = cont.ContactID;
				}
			}
		}

		protected virtual void Customer_CustomerClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Customer row = (Customer)e.Row;
			CustomerClass cc = (CustomerClass)PXSelectorAttribute.Select<Customer.customerClassID>(cache, row, e.NewValue);
			this.doCopyClassSettings = false;
			if (cc != null)
			{
				this.doCopyClassSettings = true;
				if (cache.GetStatus(row) != PXEntryStatus.Inserted)
				{
					if (BAccount.Ask(Messages.Warning, Messages.CustomerClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
					{
						this.doCopyClassSettings = false;
					}
				}
			}
		}

		protected virtual void Customer_CustomerClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row != null)
			{
				DefLocation.Current = DefLocation.Select();

				CustomerClass.RaiseFieldUpdated(cache, e.Row);

				if (CustomerClass.Current != null && CustomerClass.Current.DefaultLocationCDFromBranch == true)
				{
					Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
					if (branch != null && DefLocation.Current != null && DefLocation.Cache.GetStatus(DefLocation.Current) == PXEntryStatus.Inserted)
					{
						object cd = branch.BranchCD;
						this.Locations.Cache.RaiseFieldUpdating<LocationExtAddress.locationCD>(DefLocation.Current, ref cd);
						DefLocation.Current.LocationCD = (string)cd;
						DefLocation.Cache.Normalize();
					}
				}

				DefAddress.Current = DefAddress.Select();
				if (DefAddress.Current != null && DefAddress.Current.AddressID != null)
				{
					InitDefAddress(DefAddress.Current);
					DefAddress.Cache.MarkUpdated(DefAddress.Current);
				}

				if (this.doCopyClassSettings)
				{
                    CustomerClassDefaultInserting();

					CopyAccounts(cache, row);

					foreach (LocationExtAddress location in Locations.Select())
					{
						InitCustomerLocation(location, location.LocType, true);
					}
				}

				bool isInserted = (cache.GetStatus(row) == PXEntryStatus.Inserted);
				if (isInserted || String.IsNullOrEmpty(row.DefPaymentMethodID))
				{
					cache.SetDefaultExt<Customer.defPaymentMethodID>(row);
				}
			}
		}

		protected virtual void Customer_DefPaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			//assuming that this field assigned directly only when defaulting from Customer Class
			Customer row = (Customer) e.Row;
			if (row == null) return;

			if (row.DefPMInstanceID.HasValue)
			{
				if (this.DefPaymentMethodInstance.Current == null ||
					this.DefPaymentMethodInstance.Current.PMInstanceID != row.DefPMInstanceID)
				{
					this.DefPaymentMethodInstance.Current = this.DefPaymentMethodInstance.Select();
				}
				CustomerPaymentMethod current = this.DefPaymentMethodInstance.Current;
				if (current != null && current.PaymentMethodID != row.DefPaymentMethodID
					&& this.DefPaymentMethodInstance.Cache.GetStatus(current) == PXEntryStatus.Inserted)
				{
					this.DefPaymentMethodInstance.Delete(current);
					cache.SetValue<Customer.defPMInstanceID>(row, null);
				}
				else if (current == null)
				{
					cache.SetValue<Customer.defPMInstanceID>(row, null);
				}
			}

			PaymentMethod paymentMethod = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.DefPaymentMethodID);
			if (paymentMethod != null)
			{
				if (paymentMethod.ARIsOnePerCustomer == true)
				{
					cache.SetValueExt<Customer.defPMInstanceID>(row, paymentMethod.PMInstanceID);
				}
				else
				{
					CreateDefPaymentMethod(row);
				}
			}
			else
			{
				cache.SetValueExt<Customer.defPMInstanceID>(row, null);
			}
		}

		protected virtual void Customer_DefPMInstanceID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer) e.Row;
			if (row == null) return;
			if (row.DefPMInstanceID != null)
			{
				CustomerPaymentMethodInfo pmInfo = DefPaymentMethod.Select(row.DefPMInstanceID);
				if (pmInfo != null && pmInfo.PaymentMethodID != row.DefPaymentMethodID)
				{
					cache.SetValue<Customer.defPaymentMethodID>(row, pmInfo.PaymentMethodID);
				}
			}
		}

		protected virtual void Customer_CreditRule_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			bool creditRuleNoCheking = (row.CreditRule == CreditRuleTypes.CS_NO_CHECKING);
			
			if (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT || creditRuleNoCheking)
			{
				row.CreditDaysPastDue = 0;
			}

			if (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE || creditRuleNoCheking)
			{
				row.CreditLimit = 0m;
			}

			UpdateChildAccounts<Customer.creditRule>(cache, row, GetChildAccounts(sharedCreditPolicy: true));
		}

		protected virtual void Customer_CreditLimit_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			UpdateChildAccounts<Customer.creditLimit>(cache, (Customer)e.Row, GetChildAccounts(sharedCreditPolicy: true));
		}

		protected virtual void Customer_CreditDaysPastDue_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			UpdateChildAccounts<Customer.creditDaysPastDue>(cache, (Customer)e.Row, GetChildAccounts(sharedCreditPolicy: true));
		}

		protected virtual void Customer_Status_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Customer row = e.Row as Customer;
			if (row == null) return;

			if (e.ExternalCall && row.SharedCreditChild == true)
			{
				Customer parent = GetCustomerParent(row);
				if (parent != null)
				{
					string newValue = (string)e.NewValue;
					bool isValidStatus = parent.Status == CR.BAccount.status.CreditHold
						? newValue == CR.BAccount.status.Hold || newValue == CR.BAccount.status.Inactive || newValue == CR.BAccount.status.CreditHold
						: newValue != CR.BAccount.status.CreditHold;
					if (!isValidStatus)
					{
						cache.RaiseExceptionHandling<Customer.status>(row, row.Status, new PXSetPropertyException(Messages.SharedChildCreditHoldChange));
						e.NewValue = row.Status;
					}
				}
			}
		}

		protected virtual void Customer_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = e.Row as Customer;
			if (row == null) return;

			if (row.ParentBAccountID == null)
			{
				Func<Customer, bool> func;
				string newValue = GetSharedCreditChildStatus(row.Status, out func);
				UpdateChildAccounts<Customer.status>(cache, row, GetChildAccounts(sharedCreditPolicy: true).Where(func), newValue);
			}
		}

		/// <summary>
		/// This method returns correct status for child customers with selected "Share Credit Policy" option
		/// according with parent customer status.
		/// Out parameter returns boolean function which include conditions for child customers, 
		/// indicating whether it possible to set new status or not.
		/// </summary>
		protected virtual string GetSharedCreditChildStatus(string parentStatus, out Func<Customer, bool> func)
		{
			string childStatus;

			if (parentStatus == CR.BAccount.status.CreditHold)
			{
				childStatus = CR.BAccount.status.CreditHold;
				func = child => child.Status != CR.BAccount.status.Hold && child.Status != CR.BAccount.status.Inactive;
			}
			else if (parentStatus == CR.BAccount.status.Active || parentStatus == CR.BAccount.status.OneTime)
			{
				childStatus = CR.BAccount.status.Active;
				func = child => child.Status == CR.BAccount.status.CreditHold;
			}
			else if (parentStatus == CR.BAccount.status.Hold || parentStatus == CR.BAccount.status.Inactive)
			{
				childStatus = CR.BAccount.status.Hold;
				func = child => child.Status == CR.BAccount.status.CreditHold;
			}
			else
			{
				childStatus = null;
				func = child => false;
			}

			return childStatus;
		}

		protected virtual void UpdateChildAccounts<Field>(PXCache cache, Customer parent, IEnumerable<Customer> enumr, object sourceValue = null)
			where Field : IBqlField
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>() &&
				parent != null &&
				parent.ParentBAccountID == null)
			{
				sourceValue = sourceValue ?? cache.GetValue<Field>(parent);
				foreach (Customer child in enumr)
				{
					if (sourceValue != cache.GetValue<Field>(child))
					{
						cache.SetValue<Field>(child, sourceValue);
						cache.Update(child);
					}
				}
			}
		}

		protected virtual void Customer_SmallBalanceAllow_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			row.SmallBalanceLimit = 0m;
		}
		#endregion

		#region LocationExtAddress Events

		[PXDBString(10, IsUnicode = true, BqlField = typeof(CR.Standalone.Location.cTaxZoneID))]
		[PXUIField(DisplayName = "Tax Zone", Required = false)]
		[PXDefault(typeof(Search<AR.CustomerClass.taxZoneID, Where<AR.CustomerClass.customerClassID, Equal<Current<AR.Customer.customerClassID>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TX.TaxZone.taxZoneID>), DescriptionField = typeof(TX.TaxZone.descr), CacheGlobal = true)]
		public virtual void LocationExtAddress_CTaxZoneID_CacheAttached(PXCache sender)
		{ 
		}

		[PXDBString(1, IsFixed = true, BqlField = typeof(CR.Standalone.Location.cAvalaraCustomerUsageType))]
		[PXUIField(DisplayName = "Entity Usage Type", Required = true)]
		[TX.TXAvalaraCustomerUsageType.List]
		[PXDefault(TXAvalaraCustomerUsageType.Default,
			typeof(Search<AR.CustomerClass.avalaraCustomerUsageType,
				Where<AR.CustomerClass.customerClassID, Equal<Current<AR.Customer.customerClassID>>>>))]
		public virtual void LocationExtAddress_CAvalaraCustomerUsageType_CacheAttached(PXCache sender)
		{
		}

		protected virtual void LocationExtAddress_VBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void LocationExtAddress_CARAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.aRAcctID>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CARSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.aRSubID>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CSalesAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType))
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.SalesAcctID != null)
				{
					e.NewValue = cClass.SalesAcctID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CSalesSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType))
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.SalesSubID != null)
				{
					e.NewValue = cClass.SalesSubID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CPriceClassID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType))
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.PriceClassID != null)
				{
					e.NewValue = cClass.PriceClassID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CRetainageAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress location = e.Row as LocationExtAddress;
			if (location == null) return;

			BAccount acct = BAccountAccessor.Current;
			if (acct != null && (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType))
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.RetainageAcctID != null)
				{
					e.NewValue = cClass.RetainageAcctID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CRetainageSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress location = e.Row as LocationExtAddress;
			if (location == null) return;

			BAccount acct = BAccountAccessor.Current;
			if (acct != null && (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType))
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass?.RetainageSubID != null)
				{
					e.NewValue = cClass.RetainageSubID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CDiscountAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
				loc.IsDefault == true)
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.DiscountAcctID != null)
				{
					e.NewValue = cClass.DiscountAcctID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CDiscountSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType))
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.DiscountSubID != null)
				{
					e.NewValue = cClass.DiscountSubID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CFreightAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
				loc.IsDefault == true)
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.FreightAcctID != null)
				{
					e.NewValue = cClass.FreightAcctID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CFreightSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
				loc.IsDefault == true)
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.FreightSubID != null)
				{
					e.NewValue = cClass.FreightSubID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_CCarrierID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.shipVia>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CShipTermsID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.shipTermsID>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CShipComplete_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.shipComplete>(CustomerClass.Current) ?? SOShipComplete.CancelRemainder;
				e.Cancel = true;
			}
		}

		protected override void LocationExtAddress_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			base.LocationExtAddress_RowPersisting(cache, e);

			if (e.Cancel)
				return;

			LocationExtAddress location = (LocationExtAddress)e.Row;

			if (location == null)
				return;

			ValidateLocation(cache, location);
			VerifyAvalaraUsageType(CustomerClass.Current, location);
		}

		protected override void LocationExtAddress_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			base.LocationExtAddress_RowPersisted(sender, e);

			if (e.TranStatus == PXTranStatus.Completed)
				DiscountEngine.RemoveFromCachedCustomerPriceClasses(((LocationExtAddress)e.Row).BAccountID);
		}
		#endregion

		#region Address Events
		protected virtual void Address_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null &&
			  addr.AddressID == null)
			{
				e.Cancel = true;
			}
		}
		#endregion

		#region Contact Address
		protected virtual void Contact_EMail_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Contact row = (Contact)e.Row;
			Customer owner = BAccount.Current;
			if (owner != null && row != null)
			{
				if (row.ContactID == owner.DefBillContactID)
				{
					BAccount.Cache.MarkUpdated(owner);
				}
			}
		}
		protected virtual void NotificationRecipient_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			if (row == null) return;
			Contact contact = PXSelectorAttribute.Select<NotificationRecipient.contactID>(cache, row) as Contact;
			if (contact == null)
			{
				switch (row.ContactType)
				{
					case CustomerContactType.Primary:
						contact = DefContact.SelectWindowed(0, 1);
						break;
					case CustomerContactType.Billing:
						contact = BillContact.SelectWindowed(0, 1);
						break;
					case CustomerContactType.Shipping:
						contact = DefLocationContact.SelectWindowed(0, 1);
						break;
				}
			}
			if (contact != null)
				row.Email = contact.EMail;
		}
		#endregion

		#region CustSalesPersons Events
		public virtual void CustSalesPeople_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			CustSalesPeople row = (CustSalesPeople)e.Row;
			if (row != null)
			{
				List<CustSalesPeople> current = new List<CustSalesPeople>();
				bool duplicated = false;
				foreach (CustSalesPeople iSP in this.SalesPersons.Select())
				{
					if (row.SalesPersonID == iSP.SalesPersonID)
					{
						current.Add(iSP);
						if (row.LocationID == iSP.LocationID)
							duplicated = true;
					}
				}
				if (duplicated)
				{
					LocationExtAddress freeLocation = null;
					foreach (LocationExtAddress iLoc in this.Locations.Select())
					{
						bool found = current.Exists(new Predicate<CustSalesPeople>(delegate(CustSalesPeople op) { return (op.LocationID == iLoc.LocationID); }));
						if (!found)
						{
							freeLocation = iLoc;
							break;
						}
					}
					if (freeLocation != null)
					{
						row.LocationID = freeLocation.LocationID;
					}
					else
					{
						throw new PXCustSalesPersonException(Messages.SalesPersonAddedForAllLocations);
					}
				}

				Dictionary<Int32?, short> locationspcount = new Dictionary<int?, short>();
				locationspcount[row.LocationID] = 0;

				foreach (CustSalesPeople iSP in this.SalesPersons.Select())
				{
					if (iSP.IsDefault == true)
					{
						short counter;
						if (locationspcount.TryGetValue(iSP.LocationID, out counter))
						{
							locationspcount[iSP.LocationID] = ++counter;
						}
						else
						{
							locationspcount[iSP.LocationID] = 1;
						}
					}
				}

				//InitNewRow is false for salespersons grid
				if (locationspcount[row.LocationID] == 0)
				{
					row.IsDefault = true;
				}
				else
				{
					CheckDoubleDefault(cache, row);
				}
			}
		}

		protected virtual void CheckDoubleDefault(PXCache sender, CustSalesPeople row)
		{
			if (row != null && row.IsDefault == true)
			{
				PXResultset<CustSalesPeople> result = PXSelect<CustSalesPeople,
					Where<CustSalesPeople.bAccountID, Equal<Current<Customer.bAccountID>>,
					And<CustSalesPeople.isDefault, Equal<True>,
					And<CustSalesPeople.locationID, Equal<Required<CustSalesPeople.locationID>>,
					And<CustSalesPeople.salesPersonID, NotEqual<Required<CustSalesPeople.salesPersonID>>>>>>>.Select(sender.Graph, row.LocationID, row.SalesPersonID);
				foreach (CustSalesPeople iSP in result)
				{
					iSP.IsDefault = false;
					SalesPersons.Cache.Update(iSP);
				}

				if (result.Count > 0)
				{
					SalesPersons.View.RequestRefresh();
				}
			}
		}

		protected virtual void CustSalesPeople_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CheckDoubleDefault(sender, (CustSalesPeople)e.NewRow);
		}

		public virtual void CustSalesPeople_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustSalesPeople row = (CustSalesPeople)e.Row;
			if (row != null)
			{
				bool multipleLocations = false;
				int count = 0;
				foreach (LocationExtAddress iLoc in this.Locations.Select())
				{
					if (count > 0)
					{
						multipleLocations = true;
						break;
					}
					count++;
				}

				PXUIFieldAttribute.SetEnabled<CustSalesPeople.locationID>(this.SalesPersons.Cache, row, ((!row.LocationID.HasValue) || multipleLocations));
			}
		}

		#endregion

		#region Default Payment Method Events
		
		protected virtual void CustomerPaymentMethod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{			
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			PXUIFieldAttribute.SetEnabled(cache, null, false);
			if (row != null)
			{
				Customer acct = this.BAccount.Current;
				if (acct != null && acct.DefPMInstanceID == row.PMInstanceID)
				{
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.descr>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.cashAccountID>(cache, row, true);
					bool isInserted = (cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
					if (!String.IsNullOrEmpty(row.PaymentMethodID))
					{

						PaymentMethod pmDef = (PaymentMethod)this.PaymentMethodDef.Select();
						bool singleInstance = pmDef.ARIsOnePerCustomer ?? false;
						bool isIDMaskExists = false;
						if (!singleInstance)
						{

							foreach (PaymentMethodDetail iDef in this.PMDetails.Select(row.PaymentMethodID))
							{
								if ((iDef.IsIdentifier ?? false) && (!string.IsNullOrEmpty(iDef.DisplayMask)))
								{
									isIDMaskExists = true;
									break;
								}
							}
						}
						if (!(isIDMaskExists || singleInstance))
						{
							PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.descr>(cache, row, true);
						}
						bool ccpidVisivble = isInserted && pmDef.PaymentType == PaymentMethodType.CreditCard;
						PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.cCProcessingCenterID>(cache, row, ccpidVisivble);
						PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.cCProcessingCenterID>(cache, row, ccpidVisivble);
						
						bool customerIdVisisble = isInserted && pmDef.PaymentType == PaymentMethodType.CreditCard
												  && CCProcessingHelper.IsTokenizedPaymentMethod(this, row.PMInstanceID);
						PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.customerCCPID>(cache, row, customerIdVisisble);
						PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.customerCCPID>(cache, row, customerIdVisisble);
					}
					if (!isInserted && (!String.IsNullOrEmpty(row.PaymentMethodID)))
					{
						this.MergeDetailsWithDefinition(row);

						bool hasTransactions = ExternalTranHelper.HasTransactions(this, row.PMInstanceID);
						this.DefPaymentMethodInstanceDetails.Cache.AllowDelete = !hasTransactions;
						PXUIFieldAttribute.SetEnabled(this.DefPaymentMethodInstanceDetails.Cache, null, !hasTransactions);
					}

					if (row.CashAccountID.HasValue)
					{
						PaymentMethodAccount pmAcct = PXSelect<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
															And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
															And<PaymentMethodAccount.useForAR, Equal<True>>>>>.Select(this, row.CashAccountID, row.PaymentMethodID);
						PXUIFieldAttribute.SetWarning<CustomerPaymentMethod.cashAccountID>(cache, e.Row, pmAcct == null ? PXMessages.LocalizeFormatNoPrefixNLA(Messages.CashAccountIsNotConfiguredForPaymentMethodInAR, row.PaymentMethodID) : null);
					}
				}
			}
		}
		
		protected virtual void CustomerPaymentMethod_Descr_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			PaymentMethod pmDef = this.PaymentMethodDef.Select(row.PaymentMethodID);
			if (pmDef != null && (pmDef.ARIsOnePerCustomer ?? false))
			{
				row.Descr = pmDef.Descr;
			}
		}

		protected virtual void CustomerPaymentMethod_Descr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			PaymentMethod def = this.PaymentMethodDef.Select(row.PaymentMethodID);
			if (!(def.ARIsOnePerCustomer ?? false))
			{
				CustomerPaymentMethod existing = PXSelect<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
				And<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
				And<CustomerPaymentMethod.pMInstanceID, NotEqual<Required<CustomerPaymentMethod.pMInstanceID>>,
				And<CustomerPaymentMethod.descr, Equal<Required<CustomerPaymentMethod.descr>>>>>>>.Select(this, row.BAccountID, row.PaymentMethodID, row.PMInstanceID, row.Descr);
				if (existing != null)
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethod.descr>(row, row.Descr, new PXSetPropertyException(Messages.CustomerPMInstanceHasDuplicatedDescription, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void CustomerPaymentMethod_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			if (row == null) return;
			if (!string.IsNullOrEmpty(row.CustomerCCPID))
			{
				Customer currentCustomer = BAccount.Current;
				if (currentCustomer == null) return;
				CustomerProcessingCenterID test = CustomerProcessingID.Select(row.CCProcessingCenterID, row.CustomerCCPID);
				if (test == null)
				{
					CustomerProcessingCenterID cPCID = new CustomerProcessingCenterID();
					cPCID.BAccountID = currentCustomer.BAccountID;
					cPCID.CCProcessingCenterID = row.CCProcessingCenterID;
					cPCID.CustomerCCPID = row.CustomerCCPID;
					CustomerProcessingID.Insert(cPCID);
				}
			}
		}
		#region PM Details Events

		protected virtual void CustomerPaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			if (row != null)
			{
				PaymentMethodDetail iTempl = FindTemplate(row);
				CustomerPaymentMethod cpm = DefPaymentMethodInstance.Select(row.PMInstanceID);
				PXDefaultAttribute.SetPersistingCheck<CustomerPaymentMethodDetail.value>(cache, row, iTempl?.IsRequired == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDBCryptStringAttribute.SetDecrypted<CustomerPaymentMethodDetail.value>(cache, row, iTempl?.IsEncrypted != true);
				bool isValueEnabled = !string.IsNullOrEmpty(cpm?.CustomerCCPID) && iTempl?.IsCCProcessingID == true || iTempl?.IsCCProcessingID != true;
				PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodDetail.value>(cache, row, isValueEnabled);
			}
		}

		protected virtual void CustomerPaymentMethodDetail_Value_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethodDetail row = e.Row as CustomerPaymentMethodDetail;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def != null)
			{
				if (def.IsIdentifier ?? false)
				{
					string id = CustomerPaymentMethodMaint.IDObfuscator.MaskID(row.Value, def.EntryMask, def.DisplayMask);
					if (this.DefPaymentMethodInstance.Current.Descr != id)
					{
						CustomerPaymentMethod parent = this.DefPaymentMethodInstance.Current;
						parent.Descr = String.Format("{0}:{1}", parent.PaymentMethodID, id);
						this.DefPaymentMethodInstance.Update(parent);
					}
				}
				if (def.IsExpirationDate ?? false)
				{
					CustomerPaymentMethod parent = this.DefPaymentMethodInstance.Current;
					DefPaymentMethodInstance.Cache.SetValueExt<CustomerPaymentMethod.expirationDate>(parent, CustomerPaymentMethodMaint.ParseExpiryDate(this, parent, row.Value));
					this.DefPaymentMethodInstance.Update(parent);
				}
			}
		}

		protected virtual void CustomerPaymentMethodDetail_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			if (this.DefPaymentMethodInstance.Current != null)
			{
				PaymentMethodDetail def = this.FindTemplate(row);
				if (def != null && (def.IsIdentifier ?? false))
				{
					this.DefPaymentMethodInstance.Current.Descr = null;
				}
			}
		}
		#endregion
		#endregion

		#region CustomerPaymentMethodInfo
		
		protected virtual void CustomerPaymentMethodInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethodInfo row = e.Row as CustomerPaymentMethodInfo;
			if (row == null) return;

			Customer cust = CurrentCustomer.Current as Customer;
			if (cust == null) return;

			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodInfo.isDefault>(sender, row, row.IsActive == true);

			row.IsDefault = row.PMInstanceID == cust.DefPMInstanceID;
		}

		protected virtual void CustomerPaymentMethodInfo_IsDefault_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethodInfo row = e.Row as CustomerPaymentMethodInfo;
			if (row == null) return;
			Customer cust = BAccount.Current as Customer;
			//if there is a new customerPaymentMethod in Cache, than we have to delete it first
			if (DefPaymentMethodInstance.Cache.Inserted.Count() > 0)
			{
				CustomerPaymentMethod cpm = DefPaymentMethodInstance.Select();
				DefPaymentMethodInstance.Delete(cpm);
			}
			cust.DefPMInstanceID = row.IsDefault == false ? null : row.PMInstanceID;
			BAccount.Update(cust);
			PaymentMethods.View.RequestRefresh();
		}

		protected virtual void CustomerPaymentMethodInfo_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion
		#region Internal Fuctions
		protected virtual bool AllowChangeAccounts()
		{
			return true; //Add actual condition here 
		}

		protected override void InitDefAddress(Address aAddress)
		{
			base.InitDefAddress(aAddress);
			if (CurrentCustomer.Cache.GetStatus(CurrentCustomer.Current) == PXEntryStatus.Inserted || doCopyClassSettings)
			{
				aAddress.CountryID = CustomerClass.Current?.CountryID ?? aAddress.CountryID;
			}
		}

		protected override void SetAsDefault(ContactExtAddress row)
		{
			Customer account = this.BAccount.Current;
			if (account != null)
			{
				if (account.DefBillContactID.HasValue && (account.DefBillContactID == account.DefContactID))
				{
					int? defContactID = (row != null) ? row.ContactID : null;
					account.DefBillContactID = defContactID;
				}
			}
			base.SetAsDefault(row);
		}

		protected virtual void CreateDefPaymentMethod(Customer account)
		{			
			PaymentMethod defaultPM = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, account.DefPaymentMethodID);
			if (account.DefPMInstanceID == null && defaultPM != null && defaultPM.IsAccountNumberRequired == true)
			{
				CustomerPaymentMethod pmInstance = new CustomerPaymentMethod();
				pmInstance = this.DefPaymentMethodInstance.Insert(pmInstance);
				if (pmInstance.BAccountID == null)
				{
					pmInstance.BAccountID = account.BAccountID;
				}
				account.DefPMInstanceID = pmInstance.PMInstanceID;
				if (this.AddPMDetails() && (!defaultPM.ARIsOnePerCustomer ?? false))
				{
					this.DefPaymentMethodInstance.Current.Descr = account.DefPaymentMethodID;
				}
			}
		}

		protected virtual bool AddPMDetails()
		{
			string pmID = this.DefPaymentMethodInstance.Current.PaymentMethodID;
			bool setAccountNo = true;
			if (!String.IsNullOrEmpty(pmID))
			{
				foreach (PaymentMethodDetail it in this.PMDetails.Select())
				{
					CustomerPaymentMethodDetail det = new CustomerPaymentMethodDetail();
					if (it.IsIdentifier ?? false) setAccountNo = false;
					det.DetailID = it.DetailID;
					det = this.DefPaymentMethodInstanceDetails.Insert(det);
				}
			}
			return setAccountNo;
		}
		protected virtual void ClearPMDetails()
		{
			foreach (CustomerPaymentMethodDetail iDet in this.DefPaymentMethodInstanceDetailsAll.Select())
			{
				this.DefPaymentMethodInstanceDetails.Delete(iDet);
			}
		}
		protected virtual PaymentMethodDetail FindTemplate(CustomerPaymentMethodDetail aDet)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
				And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
			return res;
		}
		protected virtual void MergeDetailsWithDefinition(CustomerPaymentMethod aRow)
		{
			string paymentMethodID = aRow.PaymentMethodID;
			if (aRow.PMInstanceID != this.mergedPMInstance)
			{
				List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
				List<CustomerPaymentMethodDetail> toDelete = new List<CustomerPaymentMethodDetail>();
				foreach (PaymentMethodDetail it in this.PMDetails.Select(paymentMethodID))
				{
					CustomerPaymentMethodDetail detail = null;
					foreach (CustomerPaymentMethodDetail iPDet in this.DefPaymentMethodInstanceDetailsAll.Select())
					{
						if (iPDet.DetailID == it.DetailID)
						{
							detail = iPDet;
							break;
						}
					}
					if (detail == null && !(it.DetailID == CreditCardAttributes.CVV && aRow.CVVVerifyTran != null))
					{
						toAdd.Add(it);
					}
				}
				using (ReadOnlyScope rs = new ReadOnlyScope(this.DefPaymentMethodInstanceDetails.Cache))
				{
					foreach (PaymentMethodDetail it in toAdd)
					{
						CustomerPaymentMethodDetail detail = new CustomerPaymentMethodDetail();
						detail.DetailID = it.DetailID;
						detail = this.DefPaymentMethodInstanceDetails.Insert(detail);
					}

					if (toAdd.Count > 0 || toDelete.Count > 0)
					{
						this.DefPaymentMethodInstanceDetails.View.RequestRefresh();
					}
				}
				this.mergedPMInstance = aRow.PMInstanceID;
			}
		}

		#endregion

		#region Utility Functions
		
		private static List<Type> initLocationFields = new List<Type>
		{
			typeof(Location.cCarrierID),
			typeof(Location.cFOBPointID),
			typeof(Location.cResedential),
			typeof(Location.cSaturdayDelivery),
			typeof(Location.cLeadTime),
			typeof(Location.cShipComplete),
			typeof(Location.cShipTermsID),
			typeof(Location.cBranchID),
			typeof(Location.cTaxZoneID),
			typeof(Location.cTaxCalcMode),
			typeof(Location.cAvalaraCustomerUsageType),
			typeof(Location.cDiscountAcctID),
			typeof(Location.cDiscountSubID),
			typeof(Location.cFreightAcctID),
			typeof(Location.cFreightSubID),
			typeof(Location.cSalesSubID),
			typeof(Location.cSalesAcctID),
			typeof(Location.cARAccountID),
			typeof(Location.cARSubID),
			typeof(Location.cRetainageAcctID),
			typeof(Location.cRetainageSubID),
		};

		public virtual void InitCustomerLocation(IBqlTable location, string aLocationType, bool onlySetDefault)
		{
			if (location == null) return;

			PXCache cache = this.Caches[location.GetType()];
			foreach (var field in initLocationFields)
			{
				if (onlySetDefault || cache.GetValue(location, field.Name) == null)
					cache.SetDefaultExt(location, field.Name);
			}
			cache.SetValue<Location.locType>(location, aLocationType);
		}


		public virtual void CopyAccounts(PXCache cache, Customer row)
		{
			cache.SetDefaultExt<Customer.termsID>(row);
			cache.SetDefaultExt<Customer.curyID>(row);
			cache.SetDefaultExt<Customer.curyRateTypeID>(row);
			cache.SetDefaultExt<Customer.allowOverrideCury>(row);
			cache.SetDefaultExt<Customer.allowOverrideRate>(row);

			cache.SetDefaultExt<Customer.discTakenAcctID>(row);
			cache.SetDefaultExt<Customer.discTakenSubID>(row);

			cache.SetDefaultExt<Customer.localeName>(row);

			if (this.DefLocation.Current != null)
			{
				this.DefLocation.Cache.MarkUpdated(this.DefLocation.Current);
			}

			cache.SetDefaultExt<Customer.cOGSAcctID>(row);
			cache.SetDefaultExt<Customer.cOGSSubID>(row);

			cache.SetDefaultExt<Customer.smallBalanceAllow>(row);
			cache.SetDefaultExt<Customer.smallBalanceLimit>(row);
			cache.SetDefaultExt<Customer.autoApplyPayments>(row);
			cache.SetDefaultExt<Customer.printStatements>(row);
			cache.SetDefaultExt<Customer.printCuryStatements>(row);
			cache.SetDefaultExt<Customer.sendStatementByEmail>(row);

			cache.SetDefaultExt<Customer.creditLimit>(row);
			cache.SetDefaultExt<Customer.creditRule>(row);
			cache.SetDefaultExt<Customer.creditDaysPastDue>(row);
			cache.SetDefaultExt<Customer.statementCycleId>(row);
			cache.SetDefaultExt<Customer.statementType>(row);

			cache.SetDefaultExt<Customer.finChargeApply>(row);
			cache.SetDefaultExt<Customer.printInvoices>(row);
			cache.SetDefaultExt<Customer.mailInvoices>(row);

			cache.SetDefaultExt<Customer.printDunningLetters>(row);
			cache.SetDefaultExt<Customer.mailDunningLetters>(row);

			cache.SetDefaultExt<Customer.prepaymentAcctID>(row);
			cache.SetDefaultExt<Customer.prepaymentSubID>(row);

			cache.SetDefaultExt<Customer.groupMask>(row);

			cache.SetDefaultExt<Customer.retainageApply>(row);
			cache.SetDefaultExt<Customer.paymentsByLinesAllowed>(row);
		}

		protected override SelectedLocation CreateSelectedLocation()
		{
			return new SelectedCustomerLocation();
		}

		public Customer GetCustomerParent(Customer customer)
		{
			return PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, customer.ParentBAccountID);
		}

		#endregion
	
		#region Private members
		private bool doCopyClassSettings;
		private int? mergedPMInstance = null;
		#endregion

		#region Domain Functions

	    protected virtual bool ValidateLocation(PXCache cache, LocationExtAddress location)
	    {
			bool res = true;
			Customer acct = this.BAccountAccessor.Current;
			if (acct != null &&
					(acct.Type == BAccountType.CustomerType ||
					 acct.Type == BAccountType.CombinedType))
			{
				if (CustomerClass.Current?.RequireTaxZone == true && location.CTaxZoneID == null)
				{
					res &= ValidationHelper.SetErrorEmptyIfNull<Location.cTaxZoneID>(cache, location, location.CTaxZoneID);
				}

				res &= LocationValidator.ValidateCustomerLocation(cache, acct, location);
			}

		    return res;
	    }

		protected virtual void VerifyAvalaraUsageType(CustomerClass customerClass, LocationExtAddress location)
		{
			if (customerClass == null || customerClass.RequireAvalaraCustomerUsageType != true)
				return;

			if (location.CAvalaraCustomerUsageType == TXAvalaraCustomerUsageType.Default)
				throw new PXRowPersistingException(typeof(Location.cAvalaraCustomerUsageType).Name,
					location.CAvalaraCustomerUsageType, Common.Messages.NonDefaultAvalaraUsageType);
		}

		#endregion

		#region Soap-related handlers
		protected bool addressAdded;
		protected override void Address_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall)
			{
				if (!addressAdded)
				{
					base.Address_RowInserted(cache, e);
					addressAdded = true;
				}
				else if (CurrentCustomer.Cache.GetStatus(CurrentCustomer.Current) == PXEntryStatus.Inserted)
				{
					CurrentCustomer.Current.DefBillAddressID = DetailInserted<Address>(cache, (Address)e.Row, null);
					CurrentCustomer.Current.IsBillSameAsMain = false;
				}
			}
		}
		protected bool contactAdded;
		protected override void Contact_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall)
			{

				if (!contactAdded)
				{
					base.Contact_RowInserted(cache, e);
					contactAdded = true;
				}
				else if (CurrentCustomer.Cache.GetStatus(CurrentCustomer.Current) == PXEntryStatus.Inserted)
				{
					CurrentCustomer.Current.DefBillContactID = DetailInserted<Contact>(cache, (Contact)e.Row, null);
					CurrentCustomer.Current.IsBillContSameAsMain = false;
				}
			}
		}

		#endregion
	}
}



