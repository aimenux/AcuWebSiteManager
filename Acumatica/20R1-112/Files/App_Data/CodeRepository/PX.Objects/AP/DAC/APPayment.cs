using System;

using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.AP.Standalone;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.Common.Interfaces;
using PX.Objects.CR;
using PX.Objects.Common.Attributes;
using PX.Objects.GL.Descriptor;
using PX.Objects.PO;

namespace PX.Objects.AP
{
	public interface IPrintCheckControlable
	{
		string DocType { get; }
		bool? Printed { get; }
		bool? PrintCheck { get; }
		bool? IsPrintingProcess { get; }
		bool? IsMigratedRecord { get; }
		bool? IsReleaseCheckProcess { get; }
	}

    /// <summary>
    /// Represents Checks, Debit Adjustments, Prepayments, Refunds and Void Checks in Accounts Payable module.
    /// This DAC extends <see cref="APRegister"/> with the fields specific to the documents of the above types.
    /// </summary>
	[Serializable]
	[PXTable]
	[PXSubstitute(GraphType = typeof(APPaymentEntry))]
	[PXPrimaryGraph(
		new Type[] {
		typeof(APQuickCheckEntry),
		typeof(APPaymentEntry)
	},
		new Type[] {
		typeof(Select<APQuickCheck, 
			Where<APQuickCheck.docType, Equal<Current<APPayment.docType>>, 
			And<APQuickCheck.refNbr, Equal<Current<APPayment.refNbr>>>>>),
		typeof(Select<APPayment, 
			Where<APPayment.docType, Equal<Current<APPayment.docType>>, 
			And<APPayment.refNbr, Equal<Current<APPayment.refNbr>>>>>)
		})]
	[PXEMailSource]
	[PXCacheName(Messages.APPayment)]
	public class APPayment : APRegister, IInvoice, IPrintCheckControlable, IAssign, IApprovable, IApprovalDescription, IReserved
	{
		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        /// <summary>
        /// [key] Type of the document.
        /// </summary>
        /// <value>
        /// Possible values are: "CHK" - Check, "ADR" - Debit Adjustment,
        /// "PPM" - Prepayment, "REF" - Refund, "VCK" - Void Check
        /// </value>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[APPaymentType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PXFieldDescription]
		public override String DocType
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
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        /// <summary>
        /// [key] Reference number of the document.
        /// </summary>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APPaymentType.RefNbr(typeof(Search2<Standalone.APRegisterAlias.refNbr,
			InnerJoinSingleTable<APPayment, On<APPayment.docType, Equal<Standalone.APRegisterAlias.docType>,
				And<APPayment.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Vendor, On<Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<Standalone.APRegisterAlias.docType, Equal<Current<APPayment.docType>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>, 
			OrderBy<Desc<Standalone.APRegisterAlias.refNbr>>>) ,Filterable=true, IsPrimaryViewCompatible = true)]
		[APPaymentType.Numbering()]
		[PXFieldDescription]
		public override String RefNbr
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
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		#endregion
		#region VendorLocationID
		public new abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		#endregion
		#region RemitAddressID
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;

        /// <summary>
        /// Remittance address for the document.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="PX.Objects.CR.Location.VRemitAddressID">remittance address</see> of the vendor.
        /// </value>
		[PXDBInt()]
		[APAddress(typeof(Select2<Location,
			InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
			InnerJoin<Address, On<Address.addressID, Equal<Location.remitAddressID>, And<Where<Address.bAccountID, Equal<Location.bAccountID>, Or<Address.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
			LeftJoin<APAddress, On<APAddress.vendorID, Equal<Address.bAccountID>, And<APAddress.vendorAddressID, Equal<Address.addressID>, And<APAddress.revisionID, Equal<Address.revisionID>, And<APAddress.isDefaultAddress, Equal<True>>>>>>>>,
			Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>))]
		public virtual Int32? RemitAddressID
		{
			get
			{
				return this._RemitAddressID;
			}
			set
			{
				this._RemitAddressID = value;
			}
		}
		#endregion
		#region RemitContactID
		public abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }

        /// <summary>
        /// Remittance contact for the document.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="PX.Objects.CR.Location.VRemitContactID">remittance contact</see> of the vendor.
        /// </value>
		[PXDBInt]
		[PXSelector(typeof(APContact.contactID), ValidateValue = false)]    //Attribute for showing contact email field on Automatic Notifications screen in the list of availible emails for
																			//Checks and Payments screen. Relies on the work of platform, which uses PXSelector to compose email list
		[PXUIField(DisplayName = "Remittance Contact", Visible = false)]    //Attribute for displaying user friendly contact email field on Automatic Notifications screen in the list of availible emails.
		[APContact(typeof(Select2<Location,
							InnerJoin<
								    BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
							InnerJoin<
									Contact, On<Contact.contactID, Equal<Location.remitContactID>,
								    And<
										Where<Contact.bAccountID, Equal<Location.bAccountID>,
										   Or<Contact.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
							LeftJoin<
								   APContact, On<APContact.vendorID, Equal<Contact.bAccountID>,
							       And<APContact.vendorContactID, Equal<Contact.contactID>,
								   And<APContact.revisionID, Equal<Contact.revisionID>,
								   And<APContact.isDefaultContact, Equal<True>>>>>>>>,
							Where<
								   Location.bAccountID, Equal<Current<APPayment.vendorID>>,
								   And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>))]
		public virtual int? RemitContactID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.GL.Branch">Branch</see>, to which the document belongs.
		/// The field must be located before the <see cref="APPayment.APAccountID"/> and <see cref="APPayment.APSubID"/> fields for correct Restriction Groups operation.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.GL.Branch.BranchID">Branch.BranchID</see> field.
        /// </value>
		[Branch(typeof(Coalesce<
			Search<Location.vBranchID, Where<Location.bAccountID, Equal<Current<APRegister.vendorID>>, And<Location.locationID, Equal<Current<APRegister.vendorLocationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>),IsDetail = false)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region APAccountID
		public new abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }

		/// <summary>
		/// This property was copied from the <see cref="APRegister">APRegister</see> class. It is overriden only for correct order of the fields 
		/// in _ClassFields list of PXCache<TNode> class. The order of the fields is important for proper Restriction Groups operation.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		[Account(typeof(APRegister.branchID), typeof(Search<Account.accountID,
					Where2<Match<Current<AccessInfo.userName>>,
						 And<Account.active, Equal<True>,
						 And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
						  Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AP Account",
			ControlAccountForModule = ControlAccountModule.AP)]
		public override Int32? APAccountID
		{
			get
			{
				return this._APAccountID;
			}
			set
			{
				this._APAccountID = value;
			}
		}
		#endregion
		#region APSubID
		public new abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }

		/// <summary>
		/// This property was copied from the <see cref="APRegister">APRegister</see> class. It is overriden only for correct order of the fields 
		/// in _ClassFields list of PXCache<TNode> class. The order of the fields is important for proper Restriction Groups operation.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
		[SubAccount(typeof(APRegister.aPAccountID), typeof(APRegister.branchID), true, DescriptionField = typeof(Sub.description), DisplayName = "AP Subaccount", Visibility = PXUIVisibility.Visible)]
		public override Int32? APSubID
		{
			get
			{
				return this._APSubID;
			}
			set
			{
				this._APSubID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;

        /// <summary>
        /// The <see cref="PX.Objects.CA.PaymentMethod">payment method</see> used for the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CA.PaymentMethod.PaymentMethodID">PaymentMethod.PaymentMethodID</see> field.
        /// Defaults to the payment method associated with the vendor location.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Location.paymentMethodID, 
							Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, 
						  Where<PaymentMethod.useForAP,Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>))]
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
        /// The <see cref="PX.Objects.CA.CashAccount">cash account</see> associated with the <see cref="PaymentMethodID">payment method</see>.
        /// The field is irrelevant for debit adjustments.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CA.CashAccount.CashAccountID">CashAccount.CashAccountID</see> field.
        /// Defaults to the cash account associated with the payment method and location. 
        /// </value>
		[PXDefault(typeof(Coalesce<Search2<Location.cashAccountID,
										InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<Location.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
											And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
										Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, 
											And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>,
											And<Location.vPaymentMethodID,Equal<Current<APPayment.paymentMethodID>>>>>>,
								   Search2<PaymentMethodAccount.cashAccountID,InnerJoin<CashAccount,On<CashAccount.cashAccountID,Equal<PaymentMethodAccount.cashAccountID>>>, 
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<APPayment.paymentMethodID>>, 
											And<CashAccount.branchID, Equal<Current<APPayment.branchID>>,
											And<PaymentMethodAccount.useForAP,Equal<True>,												
											And<PaymentMethodAccount.aPIsDefault,Equal<True>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]

        [CashAccount(typeof(APPayment.branchID), typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount, 
							On<PaymentMethodAccount.cashAccountID,Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>, 							
							And<PaymentMethodAccount.paymentMethodID,Equal<Current<APPayment.paymentMethodID>>,
							And<PaymentMethodAccount.useForAP,Equal<True>,
							And<Where<CashAccount.clearingAccount, Equal<False>,
								Or<Current<APPayment.docType>, In3<APDocType.refund, APDocType.voidRefund>>>>>>>>), 
											Visibility = PXUIVisibility.Visible, SuppressCurrencyValidation = true)]
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

		[Obsolete("The constructor is obsolete ater AC-81186 fix")] //Only for customization backward compatibility
		public abstract class updateNextNumber : PX.Data.IBqlField { }
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
        /// <summary>
        /// A payment reference number, which can be a system-generated number or an external reference number 
        /// (such as a wire transfer number or a bank check number) entered manually.
        /// Irrelevant for Debit Adjustments.
        /// </summary>
		[PXDBString(40, IsUnicode=true)]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		[PaymentRef(typeof(APPayment.cashAccountID), typeof(APPayment.paymentMethodID),typeof(APPayment.stubCntr))]
		public virtual string ExtRefNbr { get; set; }
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region AdjDate
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;

        /// <summary>
        /// The date when the payment is applied.
        /// </summary>
        /// <value>
        /// Defaults to the current <see cref="AccessInfo.BusinessDate">business date</see>.
        /// </value>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Application Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
		#region AdjFinPeriodID
		public abstract class adjFinPeriodID : PX.Data.BQL.BqlString.Field<adjFinPeriodID> { }
		protected String _AdjFinPeriodID;

		/// <summary>
		/// The <see cref="PX.Objects.GL.Obsolete.FinPeriod">financial period</see> of payment application.
		/// </summary>
		/// <value>
		/// The value of this field is determined by the <see cref="AdjDate"/> field, but can be overriden manually.
		/// </value>
		[PXUIField(DisplayName = "Application Period", Visibility = PXUIVisibility.SelectorVisible)]
		[APOpenPeriod(
			typeof(APPayment.adjDate),
            masterFinPeriodIDType: typeof(APPayment.adjTranPeriodID),
            selectionModeWithRestrictions: FinPeriodSelectorAttribute.SelectionModesWithRestrictions.All,
			sourceSpecificationTypes: 
			new[]
			{
				typeof(CalendarOrganizationIDProvider.SourceSpecification<APPayment.branchID, True>),
				typeof(CalendarOrganizationIDProvider.SourceSpecification<
						APPayment.cashAccountID, 
						Selector<APPayment.cashAccountID, CashAccount.branchID>, 
						False>),
			},
            IsHeader = true)]
		public virtual String AdjFinPeriodID
		{
			get
			{
				return this._AdjFinPeriodID;
			}
			set
			{
				this._AdjFinPeriodID = value;
			}
		}
		#endregion
		#region AdjTranPeriodID
		public abstract class adjTranPeriodID : PX.Data.BQL.BqlString.Field<adjTranPeriodID> { }
		protected String _AdjTranPeriodID;

        /// <summary>
        /// The <see cref="PX.Objects.GL.Obsolete.FinPeriod">financial period</see> of payment application.
        /// </summary>
        /// <value>
        /// Unlike the <see cref="AdjFinPeriodID"/>, the value of this field is determined solely by the <see cref="AdjDate"/> field and can't be overriden.
        /// </value>
        [PeriodID]
		public virtual String AdjTranPeriodID
		{
			get
			{
				return this._AdjTranPeriodID;
			}
			set
			{
				this._AdjTranPeriodID = value;
			}
		}
		#endregion
		#region DocDate
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

        /// <summary>
        /// The date of the payment document.
        /// </summary>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Payment Date", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		public override DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }

        /// <summary>
        /// The <see cref="PX.Objects.GL.Obsolete.FinPeriod">financial period</see> of the document corresponding to the <see cref="DocDate">payment date</see>.
        /// </summary>
        /// <value>
        /// Unlike the <see cref="FinPeriodID"/>, the value of this field is determined solely by the <see cref="AdjDate"/> field and can't be overriden.
        /// </value>
		[PeriodID]
		public override String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        /// <summary>
        /// The <see cref="PX.Objects.GL.Obsolete.FinPeriod">financial period</see> of the document.
        /// </summary>
        /// <value>
        /// The value of this field is determined by the <see cref="DocDate"/> field, but can be overriden manually.
        /// </value>
		[APOpenPeriod(
			typeof(APPayment.docDate),
		    masterFinPeriodIDType: typeof(APPayment.tranPeriodID),
            selectionModeWithRestrictions: FinPeriodSelectorAttribute.SelectionModesWithRestrictions.All,
			sourceSpecificationTypes:
			new[]
			{
				typeof(CalendarOrganizationIDProvider.SourceSpecification<APPayment.branchID, True>),
				typeof(CalendarOrganizationIDProvider.SourceSpecification<
					APPayment.cashAccountID,
					Selector<APPayment.cashAccountID, CashAccount.branchID>,
					False>),
			})]
        [PXDefault()]
        [PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		#endregion
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }

        /// <summary>
        /// The total payment amount that should be applied to the documents.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APPayment.curyInfoID), typeof(APPayment.origDocAmt))]
		[PXUIField(DisplayName = "Payment Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CuryOrigDocAmt
		{
			get
			{
				return this._CuryOrigDocAmt;
			}
			set
			{
				this._CuryOrigDocAmt = value;
			}
		}
		#endregion
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		#endregion
		#region AdjCntr
		public new abstract class adjCntr : PX.Data.BQL.BqlInt.Field<adjCntr> { }
		#endregion
		#region StubCntr
		public abstract class stubCntr : PX.Data.BQL.BqlInt.Field<stubCntr> { }
		protected Int32? _StubCntr;

        /// <summary>
        /// The counter of the related pay stubs.
        /// Note that this field is used internally for numbering purposes and its value may not reflect the actual count of the pay stubs.
        /// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? StubCntr
		{
			get
			{
				return this._StubCntr;
			}
			set
			{
				this._StubCntr = value;
			}
		}
		#endregion
		#region BillCntr
		public abstract class billCntr : PX.Data.BQL.BqlInt.Field<billCntr> { }
		protected Int32? _BillCntr;

        /// <summary>
        /// The counter of the related bills.
        /// Note that this field is used internally for numbering purposes and its value may not reflect the actual count of the bills.
        /// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? BillCntr
		{
			get
			{
				return this._BillCntr;
			}
			set
			{
				this._BillCntr = value;
			}
		}
		#endregion
        #region ChargeCntr
        public abstract class chargeCntr : PX.Data.BQL.BqlInt.Field<chargeCntr> { }
        protected Int32? _ChargeCntr;

        /// <summary>
        /// The counter of the related charge entries.
        /// Note that this field is used internally for numbering purposes and its value may not reflect the actual count of the charges.
        /// </summary>
        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? ChargeCntr
        {
            get
            {
                return this._ChargeCntr;
            }
            set
            {
                this._ChargeCntr = value;
            }
        }
        #endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		#endregion
		#region CuryUnappliedBal
		public abstract class curyUnappliedBal : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBal> { }
		protected Decimal? _CuryUnappliedBal;

        /// <summary>
        /// The balance that has not been applied. This will be a nonzero value if the payment amount is not equal to a document’s total amount.
        /// Checks shall always have a zero unapplied balance.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
		[PXCurrency(typeof(APPayment.curyInfoID), typeof(APPayment.unappliedBal))]
		[PXUIField(DisplayName = "Unapplied Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXFormula(typeof(Sub<APPayment.curyDocBal, APPayment.curyApplAmt>))]
		public virtual Decimal? CuryUnappliedBal
		{
			get
			{
				return this._CuryUnappliedBal;
			}
			set
			{
				this._CuryUnappliedBal = value;
			}
		}
		#endregion
		#region UnappliedBal
		public abstract class unappliedBal : PX.Data.BQL.BqlDecimal.Field<unappliedBal> { }
		protected Decimal? _UnappliedBal;

        /// <summary>
        /// The balance that has not been applied. This will be a nonzero value if the payment amount is not equal to a document’s total amount.
        /// Checks shall always have a zero unapplied balance.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDecimal(4)]
		public virtual Decimal? UnappliedBal
		{
			get
			{
				return this._UnappliedBal;
			}
			set
			{
				this._UnappliedBal = value;
			}
		}
		#endregion
		#region CuryInitDocBal
		public new abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }

		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.initDocBal))]
		[PXUIField(DisplayName = "Unapplied Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXUIVerify(typeof(
			Where<APPayment.hold, Equal<True>, 
				Or<APPayment.openDoc, NotEqual<True>,
				Or<APPayment.isMigratedRecord, NotEqual<True>,
				Or2<Where<APPayment.voidAppl, Equal<True>,
					And<APPayment.curyInitDocBal, LessEqual<decimal0>,
					And<APPayment.curyInitDocBal, GreaterEqual<APPayment.curyOrigDocAmt>>>>,
				Or<Where<APPayment.voidAppl, NotEqual<True>,
					And<APPayment.curyInitDocBal, GreaterEqual<decimal0>,
					And<APPayment.curyInitDocBal, LessEqual<APPayment.curyOrigDocAmt>>>>>>>>>),
			PXErrorLevel.Error, Common.Messages.IncorrectMigratedBalance,
			CheckOnInserted = false, 
			CheckOnRowSelected = false, 
			CheckOnVerify = false, 
			CheckOnRowPersisting = true)]
		public override decimal? CuryInitDocBal
		{
			get;
			set;
		}
		#endregion
		#region CuryApplAmt
		public abstract class curyApplAmt : PX.Data.BQL.BqlDecimal.Field<curyApplAmt> { }
		protected Decimal? _CuryApplAmt;

        /// <summary>
        /// The amount to be applied on the application date.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
		[PXCurrency(typeof(APPayment.curyInfoID), typeof(APPayment.applAmt))]
		[PXUIField(DisplayName = "Application Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CuryApplAmt
		{
			get
			{
				return this._CuryApplAmt;
			}
			set
			{
				this._CuryApplAmt = value;
			}
		}
		#endregion
		#region ApplAmt
		public abstract class applAmt : PX.Data.BQL.BqlDecimal.Field<applAmt> { }
		protected Decimal? _ApplAmt;

        /// <summary>
        /// The amount to be applied on the application date.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDecimal(4)]
		public virtual Decimal? ApplAmt
		{
			get
			{
				return this._ApplAmt;
			}
			set
			{
				this._ApplAmt = value;
			}
		}
		#endregion
		#region Cleared
        public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
        protected Boolean? _Cleared;

        /// <summary>
        /// When set to <c>true</c> indicates that the check was cleared in the process of reconciliation.
        /// </summary>
        [PXDBBool]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Cleared")]
        public virtual Boolean? Cleared
        {
            get
            {
                return this._Cleared;
            }
            set
            {
                this._Cleared = value;
            }
        }
        #endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
		protected DateTime? _ClearDate;

        /// <summary>
        /// The date when the check was cleared.
        /// </summary>
        [PXDBDate]
        [PXUIField(DisplayName = "Clear Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
				this._ClearDate = value;
			}
		}
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region OpenDoc
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }

        /// <summary>
        /// When set to <c>true</c> indicates that the document was voided. In this case <see cref="VoidBatchNbr"/> field will hold the number of the voiding <see cref="Batch"/>.
        /// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Voided", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public override Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region VendorID_Vendor_acctName
		public new abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
		#endregion
		#region PrintCheck
		public abstract class printCheck : PX.Data.BQL.BqlBool.Field<printCheck> { }
		private bool? _printCheck;
		/// <summary>
		/// When set to <c>true</c> indicates that a check must be printed for the payment represented by this record.
		/// </summary>
		[PXDBBool]
		[FormulaDefault(typeof(
			IsNull<
				IIf<Where<
					isMigratedRecord, Equal<True>>,
					False,
					Selector<paymentMethodID, PaymentMethod.printOrExport>>,
				False>))]
		[PXUIField(DisplayName = "Print Check")]
		public virtual bool? PrintCheck
		{
			get { return _printCheck; }
			set { _printCheck = value; }
		}

		#endregion

		#region IsPrintingProcess
		public abstract class isPrintingProcess : PX.Data.BQL.BqlBool.Field<isPrintingProcess> { }

		/// <summary>
		/// Indicates that this check under printing processing to prevent update <see cref="CashAccountCheck"/> table by <see cref="AP.PaymentRefAttribute"/> />
		/// </summary>
		[PXBool]
		public virtual bool? IsPrintingProcess { get; set; }
		#endregion
		#region IsReleaseProcess
		public abstract class isReleaseProcess : PX.Data.BQL.BqlBool.Field<isReleaseProcess> { }

		/// <summary>
		/// Indicates that this check under release processing to prevent the question about the saving of the last check number by <see cref="AP.PaymentRefAttribute"/> />
		/// </summary>
		[PXBool]
		public virtual bool? IsReleaseCheckProcess { get; set; }
		#endregion

		#region Printed
		public new abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
		[PXDBBool]
		[PXDefault]
		[PXFormula(typeof(IIf<Where<APPayment.printCheck, NotEqual<True>, And<Selector<APPayment.paymentMethodID, PaymentMethod.printOrExport>, Equal<True>, 
			Or<Selector<APPayment.paymentMethodID, PaymentMethod.printOrExport>, NotEqual<True>>>>, True, False>))]
		public override bool? Printed
		{
			get
			{
				return this._Printed;
			}
			set
			{
				this._Printed = value;
			}
		}
		#endregion
		#region VoidAppl
		public abstract class voidAppl : PX.Data.BQL.BqlBool.Field<voidAppl> { }

        /// <summary>
        /// When <c>true</c> indicates that the document is Void Check.
        /// </summary>
        /// <value>
        /// Setting this field to <c>true</c> will change the <see cref="DocType">type of the document</see> to Void Check (<c>"VCK"</c>).
        /// </value>
		[PXBool()]
		[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return APPaymentType.VoidAppl(this._DocType);
			}
			set
			{
				if ((bool)value && !APPaymentType.VoidAppl(DocType))
				{
					this._DocType = APPaymentType.GetVoidingAPDocType(DocType);
				}
			}
		}
		#endregion
		#region CanHaveBalance
		public abstract class canHaveBalance : PX.Data.BQL.BqlBool.Field<canHaveBalance> { }

        /// <summary>
        /// Read-only field indicating whether the document can have balance.
        /// </summary>
        /// <value>
        /// <c>true</c> for Debit Adjustments, Prepayments, Quick Checks and Void Checks. <c>false</c> for other documents.
        /// </value>
		[PXBool()]
		[PXUIField(DisplayName = "Can Have Balance", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? CanHaveBalance
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return APPaymentType.CanHaveBalance(this._DocType);
			}
			set	
			{
			}
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		protected string _DrCr;

        /// <summary>
        /// Read-only field indicating whether the document is of debit or credit type.
        /// The value of this field is based solely on the <see cref="DocType"/> field.
        /// </summary>
        /// <value>
        /// Possible values are <c>"D"</c> (for Refund and Void Quick Check)
        /// and <c>"C"</c> (for Check, Void Check, Debit Adjustment, Prepayment and Quick Check).
        /// </value>
		[PXString(1, IsFixed = true)]
		public string DrCr
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return APPaymentType.DrCr(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region CATranID
		public abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }

        /// <summary>
        /// Identifier of the related <see cref="PX.Objects.CA.CATran">transaction in Cash Management module</see>.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CA.CATran.CATranID">CATran.CATranID</see> field.
        /// </value>
		[PXDBLong]
		[APCashTranID()]
		public virtual Int64? CATranID
		{
			get;
			set;
		}
		#endregion
		#region AmountToWords
		public abstract class amountToWords : PX.Data.BQL.BqlString.Field<amountToWords> { }
		protected string _AmountToWords;

        /// <summary>
        /// Returns the word representation of the amount of the document. (English only)
        /// </summary>
		[ToWords(typeof(APPayment.curyOrigDocAmt))]
		public virtual string AmountToWords
		{
			get
			{
				return this._AmountToWords;
			}
			set
			{
				this._AmountToWords = value;
			}
		}
		#endregion

		#region CuryOrigTaxDiscAmt
		public abstract class curyOrigTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? CuryOrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigTaxDiscAmt
		public abstract class origTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<origTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? OrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion

		#region CARefTranAccountID
		protected Int32? _CARefTranAccountID;

        /// <summary>
        /// Together with the <see cref="CARefTranID"/> and <see cref="CARefSplitLineNbr"/> the field is used to link the payment
        /// to the appropriate Cash Management entities (See <see cref="CATran"/>, <see cref="CASplit"/>, <see cref="CashAccount"/>)
        /// during the payments reclassification process.
        /// </summary>
		public virtual Int32? CARefTranAccountID
		{
			get
			{
				return this._CARefTranAccountID;
			}
			set
			{
				this._CARefTranAccountID = value;
			}
		}
		#endregion
		#region CARefTranID
		protected Int64? _CARefTranID;

        /// <summary>
        /// Together with the <see cref="CARefTranAccountID"/> and <see cref="CARefSplitLineNbr"/> the field is used to link the payment
        /// to the appropriate Cash Management entities (See <see cref="CATran"/>, <see cref="CASplit"/>, <see cref="CashAccount"/>)
        /// during the payments reclassification process.
        /// </summary>
		public virtual Int64? CARefTranID
		{
			get
			{
				return this._CARefTranID;
			}
			set
			{
				this._CARefTranID = value;
			}
		}
		#endregion
        #region CARefSplitLineNbr
        protected Int32? _CARefSplitLineNbr;

        /// <summary>
        /// Together with the <see cref="CARefTranID"/> and <see cref="CARefTranAccountID"/> the field is used to link the payment
        /// to the appropriate Cash Management entities (See <see cref="CATran"/>, <see cref="CASplit"/>, <see cref="CashAccount"/>)
        /// during the payments reclassification process.
        /// </summary>
        public virtual Int32? CARefSplitLineNbr
        {
            get
            {
                return this._CARefSplitLineNbr;
            }
            set
            {
                this._CARefSplitLineNbr = value;
            }
        }
        #endregion
		#region DiscDate

        /// <summary>
        /// Doesn't bear any meaning in the context of <see cref="APPayment"/> records.
        /// </summary>
		public virtual DateTime? DiscDate
		{
			get
			{
				return new DateTime();
			}
			set
			{
				;
			}
		}
		#endregion		
		#region DepositAsBatch
		public abstract class depositAsBatch : PX.Data.BQL.BqlBool.Field<depositAsBatch> { }
		protected Boolean? _DepositAsBatch;

        /// <summary>
        /// When set to <c>true</c> indicates that the payment can be included in a deposit.
        /// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Batch Deposit", Enabled = false)]
		[PXDefault(false, typeof(Search<CashAccount.clearingAccount, Where<CashAccount.cashAccountID, Equal<Current<APPayment.cashAccountID>>>>))]
		public virtual Boolean? DepositAsBatch
		{
			get
			{
				return this._DepositAsBatch;
			}
			set
			{
				this._DepositAsBatch = value;
			}
		}
		#endregion
		#region DepositAfter
		public abstract class depositAfter : PX.Data.BQL.BqlDateTime.Field<depositAfter> { }
		protected DateTime? _DepositAfter;

        /// <summary>
        /// Informational date specified on the document, which is the source of the deposit.
        /// </summary>
        /// <value>
        /// Normally defaults to <see cref="AdjDate"/> for payments with <see cref="DepositAsBatch"/> set to <c>true</c>.
        /// </value>
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deposit After", Enabled = false, Visible = false)]
		public virtual DateTime? DepositAfter
		{
			get
			{
				return this._DepositAfter;
			}
			set
			{
				this._DepositAfter = value;
			}
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited> { }
		protected Boolean? _Deposited;

        /// <summary>
        /// When equal to <c>true</c> indicates that the payment was deposited.
        /// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Deposited", Enabled = false)]
		[PXDefault(false)]
		public virtual Boolean? Deposited
		{
			get
			{
				return this._Deposited;
			}
			set
			{
				this._Deposited = value;
			}
		}
		#endregion
		#region DepositDate
		public abstract class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate> { }
		protected DateTime? _DepositDate;

        /// <summary>
        /// The date of deposit.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CA.CADeposit.TranDate">CADeposit.TranDate</see> field
        /// </value>
		[PXDBDate()]
		[PXUIField(DisplayName = "Batch Deposit Date", Enabled = false)]
		public virtual DateTime? DepositDate
		{
			get
			{
				return this._DepositDate;
			}
			set
			{
				this._DepositDate = value;
			}
		}
		#endregion
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType> { }
		protected String _DepositType;

        /// <summary>
        /// The type of the <see cref="PX.Objects.CA.CADeposit">deposit document</see>.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CA.CADeposit.TranType">CADeposit.TranType</see> field
        /// </value>
		[PXUIField(Enabled = false)]
		[PXDBString(3, IsFixed = true)]
		public virtual String DepositType
		{
			get
			{
				return this._DepositType;
			}
			set
			{
				this._DepositType = value;
			}
		}
		#endregion
		#region DepositNbr
		public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr> { }
		protected String _DepositNbr;

		/// <summary>
        /// The reference number of the <see cref="PX.Objects.CA.CADeposit">deposit document</see>.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CA.CADeposit.RefNbr">CADeposit.RefNbr</see> field
        /// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Deposit Nbr.", Enabled = false)]
		public virtual String DepositNbr
		{
			get
			{
				return this._DepositNbr;
			}
			set
			{
				this._DepositNbr = value;
			}
		}
		#endregion
		#region IsMigratedRecord
		public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
		#endregion
        #region NoteID
        public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
		[PXSearchable(SM.SearchCategory.AP, Messages.SearchableTitleDocument, new Type[] { typeof(APPayment.docType), typeof(APPayment.refNbr), typeof(APPayment.vendorID), typeof(Vendor.acctName) },
            new Type[] { typeof(APPayment.extRefNbr), typeof(APPayment.docDesc)},
		   NumberFields = new Type[] { typeof(APPayment.refNbr) },
		   Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(APPayment.docDate), typeof(APPayment.status), typeof(APPayment.extRefNbr) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(APPayment.docDesc) },
		   WhereConstraint = typeof(Where<APRegister.docType, NotEqual<APDocType.quickCheck>, And<APRegister.docType, NotEqual<APDocType.voidQuickCheck>, And<APRegister.docType, NotEqual<APDocType.debitAdj>>>>),
		   MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>),
		   SelectForFastIndexing = typeof(Select2<APPayment, InnerJoin<Vendor, On<APPayment.vendorID, Equal<Vendor.bAccountID>>>, Where<APRegister.docType, NotEqual<APDocType.quickCheck>, And<APRegister.docType, NotEqual<APDocType.voidQuickCheck>, And<APRegister.docType, NotEqual<APDocType.debitAdj>>>>>)
		)]
        [PXNote]
        public override Guid? NoteID
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

		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		/// <summary>
		/// Status of the document. The field is calculated based on the values of status flag. It can't be changed directly.
		/// The fields tht determine status of a document are: <see cref="Hold"/>, <see cref="Released"/>, <see cref="Voided"/>, <see cref="Scheduled"/>, <see cref="Prebooked"/>, <see cref="Printed"/>
		/// </summary>
		/// <value>
		/// Possible values are: <c>"H"</c> - Hold, <c>"B"</c> - Balanced, <c>"V"</c> - Voided, <c>"S"</c> - Scheduled, <c>"N"</c> - Open, <c>"C"</c> - Closed, <c>"P"</c> - Printed, <c>"K"</c> - Prebooked.
		/// Defaults to Hold.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(APDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[APDocStatus.List]
		[SetStatusCheck]
		[PXDependsOnFields(
			typeof(APPayment.voided),
			typeof(APPayment.hold),
			typeof(APPayment.scheduled),
			typeof(APPayment.released),
			typeof(APPayment.printed),
			typeof(APPayment.prebooked),
			typeof(APPayment.openDoc),
			typeof(APPayment.printCheck),
			typeof(APPayment.approved),
			typeof(APPayment.dontApprove),
			typeof(APPayment.rejected),
			typeof(APPayment.docType))]
		public override string Status
		{
			get { return _Status; }
			set { _Status = value; }
		}

		public class SetStatusCheckAttribute : SetStatusAttribute
		{
			protected override void StatusSet(PXCache cache, APRegister item, bool? holdVal)
			{
				base.StatusSet(cache, item, holdVal);

				IPrintCheckControlable controlable = item as IPrintCheckControlable;
				if (controlable != null 
					&& item.Voided != true 
					&& item.Hold != true 
					&& item.Scheduled != true 
					&& item.Released != true)
				{
					//TODO: this logic is required to correctly process nonprintable docs and should be eliminated. Logic in SetStatusCheck resets 
					// status Printed and PendingPrint. Duct tape!!!
					if ((item.Approved != null) && (item.Status == APDocStatus.PendingApproval || item.Status == APDocStatus.Rejected || item.Status == APDocStatus.Prebooked))
						return;
					if (item.Status == APDocStatus.Printed && controlable.Printed == true && controlable.PrintCheck == false)
					{
						if (item.Approved != true && item.DontApprove != true)
						{
							item.Status = APDocStatus.PendingApproval;
							return;
						}
					}

					if (item.Printed == true
						&& controlable.PrintCheck == true
						)
					{
						item.Status = APDocStatus.Printed;
					}
					else if (item.Printed == false
						&& controlable.PrintCheck == true
						)
					{
						item.Status = APDocStatus.PendingPrint;
					}
					else if (item.Prebooked == true)
					{
						item.Status = APDocStatus.Prebooked;
					}
					else
					{
						item.Status = APDocStatus.Balanced;
					}
				}
			}
		}
		#endregion
		#region CuryPOApplAmt
		public abstract class curyPOApplAmt : PX.Data.BQL.BqlDecimal.Field<curyPOApplAmt> { }

		/// <summary>
		/// The total prepayment amount that should be applied to the PO Orders.
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(APPayment.curyInfoID), typeof(APPayment.pOApplAmt))]
		[PXUIField(DisplayName = "Applied to Order", Visibility = PXUIVisibility.Visible, Enabled = false, Visible = false)]
		public virtual Decimal? CuryPOApplAmt
		{
			get;
			set;
		}
		#endregion
		#region POApplAmt
		public abstract class pOApplAmt : PX.Data.BQL.BqlDecimal.Field<pOApplAmt> { }

		/// <summary>
		/// The total prepayment amount that should be applied to the PO Orders.
		/// </summary>
		[PXBaseCury()]
		public virtual Decimal? POApplAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryPOUnreleasedApplAmt
		public abstract class curyPOUnreleasedApplAmt : PX.Data.BQL.BqlDecimal.Field<curyPOUnreleasedApplAmt> { }

		/// <summary>
		/// The total prepayment amount (unreleased documents) that should be applied to the PO Orders.
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(APPayment.curyInfoID), typeof(APPayment.pOApplAmt))]
		public virtual Decimal? CuryPOUnreleasedApplAmt
		{
			get;
			set;
		}
		#endregion
		#region POUnreleasedApplAmt
		public abstract class pOUnreleasedApplAmt : PX.Data.BQL.BqlDecimal.Field<pOUnreleasedApplAmt> { }

		/// <summary>
		/// The total prepayment amount (unreleased documents) that should be applied to the PO Orders.
		/// </summary>
		[PXBaseCury()]
		public virtual Decimal? POUnreleasedApplAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryPOFullApplAmt
		public abstract class curyPOFullApplAmt : PX.Data.BQL.BqlDecimal.Field<curyPOFullApplAmt> { }

		/// <summary>
		/// The total prepayment amount (released and not, request and not documents) that should be applied to the PO Orders.
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(APPayment.curyInfoID), typeof(APPayment.pOApplAmt))]
		public virtual Decimal? CuryPOFullApplAmt
		{
			get;
			set;
		}
		#endregion
		#region POFullApplAmt
		public abstract class pOFullApplAmt : PX.Data.BQL.BqlDecimal.Field<pOFullApplAmt> { }

		/// <summary>
		/// The total prepayment amount (released and not, request and not documents) that should be applied to the PO Orders.
		/// </summary>
		[PXBaseCury()]
		public virtual Decimal? POFullApplAmt
		{
			get;
			set;
		}
		#endregion


		#region IsRequestPrepayment
		public abstract class isRequestPrepayment : PX.Data.BQL.BqlBool.Field<isRequestPrepayment> { }
		[PXBool]
		public virtual bool? IsRequestPrepayment
		{
			get;
			set;
		}
		#endregion
	}
}

namespace PX.Objects.AP.Standalone
{
	[Serializable()]
	[PXHidden(ServiceVisible = false)]
	public partial class APPayment : PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected string _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
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
		protected string _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
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
		#region RemitAddressID
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;
		[PXDBInt()]
		public virtual Int32? RemitAddressID
		{
			get
			{
				return this._RemitAddressID;
			}
			set
			{
				this._RemitAddressID = value;
			}
		}
		#endregion
		#region RemitContactID
		public abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }
		protected Int32? _RemitContactID;
		[PXDBInt()]
		public virtual Int32? RemitContactID
		{
			get
			{
				return this._RemitContactID;
			}
			set
			{
				this._RemitContactID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
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
		[PXDBInt()]
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
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region AdjDate
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;
		[PXDBDate()]
		[PXDefault()]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
		#region AdjFinPeriodID
		public abstract class adjFinPeriodID : PX.Data.BQL.BqlString.Field<adjFinPeriodID> { }
		protected String _AdjFinPeriodID;
		[PXDBString(6, IsFixed=true)]
		public virtual String AdjFinPeriodID
		{
			get
			{
				return this._AdjFinPeriodID;
			}
			set
			{
				this._AdjFinPeriodID = value;
			}
		}
		#endregion
		#region AdjTranPeriodID
		public abstract class adjTranPeriodID : PX.Data.BQL.BqlString.Field<adjTranPeriodID> { }
		protected String _AdjTranPeriodID;
		[PXDBString(6, IsFixed = true)]
		public virtual String AdjTranPeriodID
		{
			get
			{
				return this._AdjTranPeriodID;
			}
			set
			{
				this._AdjTranPeriodID = value;
			}
		}
		#endregion
		#region StubCntr
		public abstract class stubCntr : PX.Data.BQL.BqlInt.Field<stubCntr> { }
		protected Int32? _StubCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? StubCntr
		{
			get
			{
				return this._StubCntr;
			}
			set
			{
				this._StubCntr = value;
			}
		}
		#endregion
		#region BillCntr
		public abstract class billCntr : PX.Data.BQL.BqlInt.Field<billCntr> { }
		protected Int32? _BillCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? BillCntr
		{
			get
			{
				return this._BillCntr;
			}
			set
			{
				this._BillCntr = value;
			}
		}
		#endregion
        #region ChargeCntr
        public abstract class chargeCntr : PX.Data.BQL.BqlInt.Field<chargeCntr> { }
        protected Int32? _ChargeCntr;
        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? ChargeCntr
        {
            get
            {
                return this._ChargeCntr;
            }
            set
            {
                this._ChargeCntr = value;
            }
        }
        #endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
		protected Boolean? _Cleared;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Cleared
		{
			get
			{
				return this._Cleared;
			}
			set
			{
				this._Cleared = value;
			}
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
		protected DateTime? _ClearDate;
		[PXDBDate()]
		public virtual DateTime? ClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
				this._ClearDate = value;
			}
		}
		#endregion
		#region CATranID
		public abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }
		protected Int64? _CATranID;
		[PXDBLong()]
		public virtual Int64? CATranID
		{
			get
			{
				return this._CATranID;
			}
			set
			{
				this._CATranID = value;
			}
		}
		#endregion
		#region PrintCheck
		public abstract class printCheck : PX.Data.BQL.BqlBool.Field<printCheck> { }

		[PXDBBool]
		[PXDefault(typeof(Search<PaymentMethod.printOrExport, Where<PaymentMethod.paymentMethodID, Equal<Current<APQuickCheck.paymentMethodID>>>>))]
		public virtual bool? PrintCheck { get; set; }
		#endregion

		#region CuryOrigTaxDiscAmt
		public abstract class curyOrigTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? CuryOrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigTaxDiscAmt
		public abstract class origTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<origTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? OrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion

		#region DepositAsBatch
		public abstract class depositAsBatch : PX.Data.BQL.BqlBool.Field<depositAsBatch> { }
		protected Boolean? _DepositAsBatch;
		[PXDBBool()]
		public virtual Boolean? DepositAsBatch
		{
			get
			{
				return this._DepositAsBatch;
			}
			set
			{
				this._DepositAsBatch = value;
			}
		}
		#endregion
		#region DepositAfter
		public abstract class depositAfter : PX.Data.BQL.BqlDateTime.Field<depositAfter> { }
		protected DateTime? _DepositAfter;
		[PXDBDate()]
		public virtual DateTime? DepositAfter
		{
			get
			{
				return this._DepositAfter;
			}
			set
			{
				this._DepositAfter = value;
			}
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited> { }
		protected Boolean? _Deposited;
		[PXDBBool()]
		public virtual Boolean? Deposited
		{
			get
			{
				return this._Deposited;
			}
			set
			{
				this._Deposited = value;
			}
		}
		#endregion
		#region DepositDate
		public abstract class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate> { }
		protected DateTime? _DepositDate;
		[PXDBDate()]	
		public virtual DateTime? DepositDate
		{
			get
			{
				return this._DepositDate;
			}
			set
			{
				this._DepositDate = value;
			}
		}
		#endregion
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType> { }
		protected String _DepositType;
		
		[PXDBString(3, IsFixed = true)]
		public virtual String DepositType
		{
			get
			{
				return this._DepositType;
			}
			set
			{
				this._DepositType = value;
			}
		}
		#endregion
		#region DepositNbr
		public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr> { }
		protected String _DepositNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String DepositNbr
		{
			get
			{
				return this._DepositNbr;
			}
			set
			{
				this._DepositNbr = value;
			}
		}
		#endregion
	}
}
