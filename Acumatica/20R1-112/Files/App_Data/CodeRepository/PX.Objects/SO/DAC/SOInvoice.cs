using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.PM;

namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.AR;
	using PX.Objects.CS;
	using PX.Objects.TX;
	using PX.Objects.CR;
	using PX.Objects.GL;
	using PX.Objects.IN;
	using PX.Objects.CA;
	using ARPayment = PX.Objects.AR.Standalone.ARPayment;
	using PX.Objects.AR.CCPaymentProcessing;
	using PX.Objects.Common.Attributes;
	using PX.Data.ReferentialIntegrity.Attributes;

	[Serializable]
    [CRCacheIndependentPrimaryGraphList(new Type[] {
		typeof(SOInvoiceEntry)
	},
           new Type[] {
		typeof(Select<ARInvoice, 
			Where<ARInvoice.docType, Equal<Current<SOInvoice.docType>>, 
				And<ARInvoice.refNbr, Equal<Current<SOInvoice.refNbr>>>>>)
		})]
	[PXCacheName(Messages.SOInvoice)]
	[PXProjection(typeof(Select2<SOInvoice, InnerJoin<ARRegister, On<ARRegister.docType, Equal<SOInvoice.docType>, And<ARRegister.refNbr, Equal<SOInvoice.refNbr>>>, LeftJoin<ARPayment, On<SOInvoice.docType, Equal<ARPayment.docType>, And<SOInvoice.refNbr, Equal<ARPayment.refNbr>>>>>>), Persistent = true)]    
	public partial class SOInvoice : PX.Data.IBqlTable, ICCCapturePayment
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOInvoice>.By<docType, refNbr>
		{
            public static SOInvoice Find(PXGraph graph, string docType, string refNbr) => FindBy(graph, docType, refNbr);
		}
        public static class FK
        {
            public class ARInvoice : AR.ARInvoice.PK.ForeignKeyOf<SOInvoice>.By<docType, refNbr> { }
            public class BillAddress : SOBillingAddress.PK.ForeignKeyOf<SOInvoice>.By<billAddressID> { }
            public class ShipAddress : SOShippingAddress.PK.ForeignKeyOf<SOInvoice>.By<shipAddressID> { }
            public class BillContact : SOBillingContact.PK.ForeignKeyOf<SOInvoice>.By<billContactID> { }
            public class ShipContact : SOShippingContact.PK.ForeignKeyOf<SOInvoice>.By<shipContactID> { }
        }
		#endregion
		//SOInvoice
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool()]
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
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(ARInvoice.docType))]
		[ARDocType.List()]
		[PXUIField(DisplayName = "Type")]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBLiteDefault(typeof(ARInvoice.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXParent(typeof(FK.ARInvoice))]
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true)]
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
		
		#region BillAddressID
		// Bill Address ID from Sales Order
		public abstract class billAddressID : PX.Data.BQL.BqlInt.Field<billAddressID> { }
		protected Int32? _BillAddressID;
		[PXDBInt()]
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
		// Bill Contact ID from Sales Order
		public abstract class billContactID : PX.Data.BQL.BqlInt.Field<billContactID> { }
		protected Int32? _BillContactID;
		[PXDBInt()]
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
		#region ShipAddressID
		// Ship Address ID from Sales Order
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
		protected Int32? _ShipAddressID;
		[PXDBInt()]
		public virtual Int32? ShipAddressID
		{
			get
			{
				return this._ShipAddressID;
			}
			set
			{
				this._ShipAddressID = value;
			}
		}
		#endregion
		#region ShipContactID
		// Ship Contact ID from Sales Order
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }
		protected Int32? _ShipContactID;
		[PXDBInt()]
		public virtual Int32? ShipContactID
		{
			get
			{
				return this._ShipContactID;
			}
			set
			{
				this._ShipContactID = value;
			}
		}
		#endregion
        #region DocDisc
        public abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
        protected Decimal? _DocDisc;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? DocDisc
        {
            get
            {
                return this._DocDisc;
            }
            set
            {
                this._DocDisc = value;
            }
        }
        #endregion
        #region CuryDocDisc
        public abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
        protected Decimal? _CuryDocDisc;
        [PXCurrency(typeof(SOInvoice.curyInfoID), typeof(SOInvoice.docDisc))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Document Discount", Enabled = true)]
        public virtual Decimal? CuryDocDisc
        {
            get
            {
                return this._CuryDocDisc;
            }
            set
            {
                this._CuryDocDisc = value;
            }
        }
        #endregion
		#region CuryManDisc
		public abstract class curyManDisc : PX.Data.BQL.BqlDecimal.Field<curyManDisc> { }
		protected Decimal? _CuryManDisc;
		[PXDBCurrency(typeof(SOInvoice.curyInfoID), typeof(SOInvoice.manDisc))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Manual Total")]
		public virtual Decimal? CuryManDisc
		{
			get
			{
				return this._CuryManDisc;
			}
			set
			{
				this._CuryManDisc = value;
			}
		}
		#endregion
		#region ManDisc
		public abstract class manDisc : PX.Data.BQL.BqlDecimal.Field<manDisc> { }
		protected Decimal? _ManDisc;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ManDisc
		{
			get
			{
				return this._ManDisc;
			}
			set
			{
				this._ManDisc = value;
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
		#region CuryPaymentAmt
		public abstract class curyPaymentAmt : PX.Data.BQL.BqlDecimal.Field<curyPaymentAmt> { }
		protected Decimal? _CuryPaymentAmt;
		[PXDBCurrency(typeof(SOInvoice.curyInfoID), typeof(SOInvoice.paymentAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Payment Amount", Enabled = false)]
		public virtual Decimal? CuryPaymentAmt
		{
			get
			{
				return this._CuryPaymentAmt;
			}
			set
			{
				this._CuryPaymentAmt = value;
			}
		}
		#endregion
		#region PaymentAmt
		public abstract class paymentAmt : PX.Data.BQL.BqlDecimal.Field<paymentAmt> { }
		protected Decimal? _PaymentAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? PaymentAmt
		{
			get
			{
				return this._PaymentAmt;
			}
			set
			{
				this._PaymentAmt = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault()]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
        #region PaymentMethodID
        public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
        protected string _PaymentMethodID;
		[PXUIField(DisplayName = "Payment Method")]
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID, InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>,
                                        Where<Customer.bAccountID, Equal<Current<SOInvoice.customerID>>,
                                              And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>>>>,
                                   Search<Customer.defPaymentMethodID,
                                         Where<Customer.bAccountID, Equal<Current<SOInvoice.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search5<PaymentMethod.paymentMethodID, LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
                                    And<CustomerPaymentMethod.bAccountID, Equal<Current<SOInvoice.customerID>>>>>,
                                Where<PaymentMethod.isActive, Equal<boolTrue>,
                                And<PaymentMethod.useForAR, Equal<boolTrue>,
                                And<Where<PaymentMethod.aRIsOnePerCustomer, Equal<True>,
                                    Or<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>, Aggregate<GroupBy<PaymentMethod.paymentMethodID>>>), DescriptionField = typeof(PaymentMethod.descr))]        
        //[PXDefault(typeof(Search<CustomerPaymentMethod.paymentMethodID, Where<CustomerPaymentMethod.bAccountID, Equal<Current<SOInvoice.customerID>>, And<CustomerPaymentMethod.pMInstanceID, Equal<Current<SOInvoice.pMInstanceID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected Int32? _PMInstanceID;

		[PXDBInt()]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXDefault(typeof(Coalesce<
                        Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
                                And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
                                Where<Customer.bAccountID, Equal<Current2<SOInvoice.customerID>>,
									And<CustomerPaymentMethod.isActive, Equal<True>,
									And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOInvoice.paymentMethodID>>>>>>,
                        Search<CustomerPaymentMethod.pMInstanceID,
                                Where<CustomerPaymentMethod.bAccountID, Equal<Current2<SOInvoice.customerID>>,
                                    And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOInvoice.paymentMethodID>>,
                                    And<CustomerPaymentMethod.isActive, Equal<True>>>>,
								OrderBy<Desc<CustomerPaymentMethod.expirationDate, 
								Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
                        , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current<SOInvoice.customerID>>,
            And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOInvoice.paymentMethodID>>,
            And<Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
                    Equal<Current2<SOInvoice.pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[DeprecatedProcessing]
		[DisabledProcCenter]
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
		
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;

		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
										InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
											And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
										Where<CustomerPaymentMethod.bAccountID, Equal<Current2<SOInvoice.customerID>>,
											And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<SOInvoice.pMInstanceID>>>>>,
								Search2<CashAccount.cashAccountID,
									InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
										And<PaymentMethodAccount.useForAR, Equal<True>,
										And<PaymentMethodAccount.aRIsDefault, Equal<True>,
										And<PaymentMethodAccount.paymentMethodID, Equal<Current2<SOInvoice.paymentMethodID>>>>>>>,
                                    Where<CashAccount.branchID,Equal<Current<ARRegister.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [CashAccount(typeof(ARRegister.branchID), typeof(Search2<CashAccount.cashAccountID,
                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                    And<PaymentMethodAccount.paymentMethodID, Equal<Current2<SOInvoice.paymentMethodID>>,
                    And<PaymentMethodAccount.useForAR, Equal<True>>>>>, Where<Match<Current<AccessInfo.userName>>>>), Visibility = PXUIVisibility.Visible)]
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
		protected string _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
		protected Boolean? _Cleared;
		[PXDBBool()]
		[PXUIField(DisplayName = "Cleared")]
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
		[PXUIField(DisplayName = "Clear Date")]
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
		[SOCashSaleCashTranID()]
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
		#region RefTranExtNbr
		public abstract class refTranExtNbr : PX.Data.BQL.BqlString.Field<refTranExtNbr> { }
		protected String _RefTranExtNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<ExternalTransaction.tranNumber, 
			Where<ExternalTransaction.pMInstanceID, Equal<Current<SOInvoice.pMInstanceID>>,
				And<ExternalTransaction.processingStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>>>, 
			OrderBy<Desc<ExternalTransaction.transactionID>>>), 
			typeof(ExternalTransaction.transactionID), typeof(ExternalTransaction.docType), typeof(ExternalTransaction.refNbr), typeof(ExternalTransaction.amount))]
		[PXUIField(DisplayName = "Orig. PC Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String RefTranExtNbr
		{
			get
			{
				return this._RefTranExtNbr;
			}
			set
			{
				this._RefTranExtNbr = value;
			}
		}
		#endregion

		//ARRegister
		#region DocType
		public abstract class aRRegisterDocType : PX.Data.BQL.BqlString.Field<aRRegisterDocType> { }
		[PXDBString(3, IsFixed = true, BqlField = typeof(ARRegister.docType))]
		[PXRestriction()]
		public virtual String ARRegisterDocType
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
		#region RefNbr
		public abstract class aRRegisterRefNbr : PX.Data.BQL.BqlString.Field<aRRegisterRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlField = typeof(ARRegister.refNbr))]
		[PXRestriction()]
		public virtual String ARRegisterRefNbr
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
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool(BqlField = typeof(ARRegister.released))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool(BqlField = typeof(ARRegister.hold))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		protected string _DocDesc;
		[PXUIField(DisplayName = "Description")]
		[PXDBString(150, IsUnicode = true, BqlField = typeof(ARRegister.docDesc))]
		public virtual string DocDesc
		{
			get
			{
				return this._DocDesc;
			}
			set
			{
				this._DocDesc = value;
			}
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		
		protected DateTime? _DocDate;
		[PXDBDate(BqlField = typeof(ARRegister.docDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
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
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true, BqlField = typeof(ARRegister.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List()]
		public virtual String Status
		{
            get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		protected Boolean? _IsTaxValid;
		[PXDBBool(BqlField = typeof(ARRegister.isTaxValid))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? IsTaxValid
		{
			get
			{
				return this._IsTaxValid;
			}
			set
			{
				this._IsTaxValid = value;
			}
		}
		#endregion
		#region IsTaxSaved
		public abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }
		[PXDBBool(BqlField = typeof(ARRegister.isTaxSaved))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax has been saved in the external tax provider", Enabled = false)]
		public virtual Boolean? IsTaxSaved
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.SO | SM.SearchCategory.AR, "{0}: {1} - {3}", new Type[] { typeof(SOInvoice.docType), typeof(SOInvoice.refNbr), typeof(SOInvoice.customerID), typeof(Customer.acctName) },
			new Type[] { typeof(SOInvoice.extRefNbr), typeof(SOInvoice.docDesc) },
			NumberFields = new Type[] { typeof(SOInvoice.refNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(SOInvoice.docDate), typeof(SOInvoice.status), typeof(SOInvoice.extRefNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(SOInvoice.docDesc) },
			MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<SOInvoice.customerID>>>),
			SelectForFastIndexing = typeof(Select2<SOInvoice, InnerJoin<Customer, On<SOInvoice.customerID, Equal<Customer.bAccountID>>>>)
		)]
		[PXNote(DescriptionField = typeof(SOInvoice.refNbr), BqlField = typeof(ARRegister.noteID))]
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

		//ARPayment
		#region DocType
		public abstract class aRPaymentDocType : PX.Data.BQL.BqlString.Field<aRPaymentDocType> { }
		[PXDBString(3, IsFixed = true, BqlField = typeof(ARPayment.docType))]
		[PXRestriction()]
		public virtual String ARPaymentDocType
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return this._DocType == ARDocType.CashSale || this._DocType == ARDocType.CashReturn ? this._DocType : null;
			}
			set
			{
			}
		}
		#endregion
		#region RefNbr
		public abstract class aRPaymentRefNbr : PX.Data.BQL.BqlString.Field<aRPaymentRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlField = typeof(ARPayment.refNbr))]
		[PXRestriction()]
		public virtual String ARPaymentRefNbr
		{
			[PXDependsOnFields(typeof(docType),typeof(refNbr))]
			get
			{
				return this._DocType == ARDocType.CashSale || this._DocType == ARDocType.CashReturn ? this._RefNbr : null;
			}
			set
			{
			}
		}
		#endregion
		#region PMInstanceID
		[PXDBInt(BqlField=typeof(ARPayment.pMInstanceID))]
		public virtual Int32? ARPaymentPMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
			}
		}
		#endregion
		#region PaymentMethodID
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARPayment.paymentMethodID))]
		public virtual String ARPaymentPaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
			}
		}
		#endregion
		#region CashAccountID
		[PXDBInt(BqlField=typeof(ARPayment.cashAccountID))]
		public virtual Int32? ARPaymentCashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
			}
		}
		#endregion
		#region ExtRefNbr
		[PXDBString(15, IsUnicode = true, BqlField = typeof(ARPayment.extRefNbr))]
		public virtual String ARPaymentExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
			}
		}
		#endregion
		#region AdjDate
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;
		[PXDBDate(BqlField = typeof(ARPayment.adjDate))]
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
		[PXDBString(BqlField = typeof(ARPayment.adjFinPeriodID))]
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
		[PXDBString(BqlField = typeof(ARPayment.adjTranPeriodID))]
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
		#region Cleared
		[PXDBBool(BqlField=typeof(ARPayment.cleared))]
		public virtual Boolean? ARPaymentCleared
		{
			get
			{
				return this._Cleared;
			}
			set
			{
			}
		}
		#endregion
		#region ClearDate
		[PXDBDate(BqlField=typeof(ARPayment.clearDate))]
		public virtual DateTime? ARPaymentClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
			}
		}
		#endregion
		#region CATranID
		[PXDBLong(BqlField=typeof(ARPayment.cATranID))]
		public virtual Int64? ARPaymentCATranID
		{
			get
			{
				return this._CATranID;
			}
			set
			{
			}
		}
		#endregion
		#region DepositAsBatch
		public abstract class depositAsBatch : PX.Data.BQL.BqlBool.Field<depositAsBatch> { }
		protected Boolean? _DepositAsBatch;
		[PXDBBool(BqlField=typeof(ARPayment.depositAsBatch))]
		[PXDefault(false, typeof(Search<CashAccount.clearingAccount, Where<CashAccount.cashAccountID, Equal<Current<cashAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<cashAccountID>))]
		public virtual Boolean? ARPaymentDepositAsBatch
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
		[PXDBDate(BqlField=typeof(ARPayment.depositAfter))]
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
		#region DepositDate
		public abstract class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate> { }
		protected DateTime? _DepositDate;
		[PXDBDate(BqlField=typeof(ARPayment.depositDate))]
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
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited> { }
		protected Boolean? _Deposited;
		[PXDBBool(BqlField=typeof(ARPayment.deposited))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType> { }
		protected String _DepositType;
		[PXDBString(3, IsFixed = true, BqlField = typeof(ARPayment.depositType))]
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
		[PXDBString(15, IsUnicode = true, BqlField=typeof(ARPayment.depositNbr))]
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
		#region ChargeCntr
		public abstract class chargeCntr : PX.Data.BQL.BqlInt.Field<chargeCntr> { }
		protected Int32? _ChargeCntr;
		[PXDBInt(BqlField = typeof(ARPayment.chargeCntr))]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region CuryConsolidateChargeTotal
        public abstract class curyConsolidateChargeTotal : PX.Data.BQL.BqlDecimal.Field<curyConsolidateChargeTotal> { }
        protected Decimal? _CuryConsolidateChargeTotal;
        [PXDBCurrency(typeof(SOInvoice.curyInfoID), typeof(SOInvoice.consolidateChargeTotal), BqlField=typeof(ARPayment.curyConsolidateChargeTotal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck=PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Consolidate Charges", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Decimal? CuryConsolidateChargeTotal
        {
            get
            {
                return this._CuryConsolidateChargeTotal;
            }
            set
            {
                this._CuryConsolidateChargeTotal = value;
            }
        }
        #endregion
        #region ConsolidateChargeTotal
        public abstract class consolidateChargeTotal : PX.Data.BQL.BqlDecimal.Field<consolidateChargeTotal> { }
        protected Decimal? _ConsolidateChargeTotal;
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck=PXPersistingCheck.Nothing)]
        [PXDBDecimal(4, BqlField=typeof(ARPayment.consolidateChargeTotal))]
        public virtual Decimal? ConsolidateChargeTotal
        {
            get
            {
                return this._ConsolidateChargeTotal;
            }
            set
            {
                this._ConsolidateChargeTotal = value;
            }
        }
        #endregion
		#region IsCCPayment
		public abstract class isCCPayment : PX.Data.BQL.BqlBool.Field<isCCPayment> { }

		protected bool? _IsCCPayment;
		[PXBool()]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual bool? IsCCPayment
		{
			get
			{
				return this._IsCCPayment;
			}
			set
			{
				this._IsCCPayment = value;
			}
		}
		#endregion
		#region CCPaymentStateDescr
		public abstract class cCPaymentStateDescr : PX.Data.BQL.BqlString.Field<cCPaymentStateDescr> { }
		protected String _CCPaymentStateDescr;
		[PXString(255)]
		[PXUIField(DisplayName = "Processing Status", Enabled = false)]
		public virtual String CCPaymentStateDescr
		{
			get
			{
				return this._CCPaymentStateDescr;
			}
			set
			{
				this._CCPaymentStateDescr = value;
			}
		}
		#endregion

		#region CCAuthTranNbr
		public abstract class cCAuthTranNbr : PX.Data.BQL.BqlInt.Field<cCAuthTranNbr> { }
		protected Int32? _CCAuthTranNbr;
		[PXInt()]
		[PXUIField(DisplayName = "CC Tran. Nbr.")]
		public virtual Int32? CCAuthTranNbr
		{
			get
			{
				return this._CCAuthTranNbr;
			}
			set
			{
				this._CCAuthTranNbr = value;
			}
		}
		#endregion
		
		#region ICCPayment Members

		decimal? ICCPayment.CuryDocBal
		{
			get
			{
				return (this.CuryPaymentAmt);
			}
			set
			{

			}
		}


		string ICCPayment.DocType
		{
			get
			{
				return this.DocType;
			}
			set
			{

			}
		}

		string ICCPayment.RefNbr
		{
			get
			{
				return this.RefNbr;
			}
			set
			{

			}
		}

		string ICCPayment.OrigDocType
		{
			get { return null; }
		}

		string ICCPayment.OrigRefNbr
		{
			get { return null; }
		}

		#endregion
		
		#region CuryCCCapturedAmt
		public abstract class curyCCCapturedAmt : PX.Data.BQL.BqlDecimal.Field<curyCCCapturedAmt> { }
		protected Decimal? _CuryCCCapturedAmt;
		[PXDBCurrency(typeof(SOInvoice.curyInfoID),typeof(SOInvoice.cCCapturedAmt))]		
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Captured Amount", Enabled = false)]
		public virtual Decimal? CuryCCCapturedAmt
		{
			get
			{
				return this._CuryCCCapturedAmt;
			}
			set
			{
				this._CuryCCCapturedAmt = value;
			}
		}
		#endregion
		#region CCCapturedAmt
		public abstract class cCCapturedAmt : PX.Data.BQL.BqlDecimal.Field<cCCapturedAmt> { }
		protected Decimal? _CCCapturedAmt;
		[PXDBDecimal(4)]		
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pre-Authorized Amount", Enabled = false)]
		public virtual Decimal? CCCapturedAmt
		{
			get
			{
				return this._CCCapturedAmt;
			}
			set
			{
				this._CCCapturedAmt = value;
			}
		}
		#endregion
		#region IsCCCaptured
		public abstract class isCCCaptured : PX.Data.BQL.BqlBool.Field<isCCCaptured> { }
		protected bool? _IsCCCaptured = false;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "CC Captured")]
		public virtual bool? IsCCCaptured
		{
			get
			{
				return _IsCCCaptured;
			}
			set
			{
				_IsCCCaptured = value;
			}
		}
		#endregion

		#region IsCCCaptureFailed
		public abstract class isCCCaptureFailed : PX.Data.BQL.BqlBool.Field<isCCCaptureFailed> { }
		protected bool? _IsCCCaptureFailed = false;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "CC Captured")]
		public virtual bool? IsCCCaptureFailed
		{
			get
			{
				return _IsCCCaptureFailed;
			}
			set
			{
				_IsCCCaptureFailed = value;
			}
		}
		#endregion
		#region CuryAmtToCapture
		public abstract class curyAmtToCapture : PX.Data.BQL.BqlDecimal.Field<curyAmtToCapture> { }
		[PXCury(typeof(SOInvoice.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount to Capture", Enabled = false)]
		public virtual Decimal? CuryAmtToCapture
		{
			[PXDependsOnFields(typeof(curyPaymentAmt),typeof(curyCCCapturedAmt))]
			get
			{
				return (this.CuryPaymentAmt - this.CuryCCCapturedAmt);
			}
			set
			{
				
			}
		}
		#endregion
		#region AmtToCapture
		public abstract class amtToCapture : PX.Data.BQL.BqlDecimal.Field<amtToCapture> { }
		[PXBaseCury(typeof(SOInvoice.curyInfoID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount to Capture", Enabled = false)]
		public virtual Decimal? AmtToCapture
		{
			[PXDependsOnFields(typeof(paymentAmt),typeof(cCCapturedAmt))]
			get
			{
				return  (this.PaymentAmt -this.CCCapturedAmt);
			}
			set
			{

			}
		}
		#endregion
		#region PaymentProjectID
		public abstract class paymentProjectID : PX.Data.BQL.BqlInt.Field<paymentProjectID> { }
		protected Int32? _PaymentProjectID;
		[ProjectDefault(BatchModule.AR)]
		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInAR, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute(BqlField = typeof(ARPayment.projectID))]
		public virtual Int32? PaymentProjectID
		{
			get
			{
				return this._PaymentProjectID;
			}
			set
			{
				this._PaymentProjectID = value;
			}
		}
		#endregion
		#region PaymentTaskID
		public abstract class paymentTaskID : PX.Data.BQL.BqlInt.Field<paymentTaskID> { }
		protected Int32? _PaymentTaskID;
		[ActiveProjectTask(typeof(SOInvoice.paymentProjectID), BatchModule.AR, DisplayName = "Project Task", BqlField = typeof(ARPayment.taskID))]
		public virtual Int32? PaymentTaskID
		{
			get
			{
				return this._PaymentTaskID;
			}
			set
			{
				this._PaymentTaskID = value;
			}
		}
		#endregion

		#region CreateINDoc
		public abstract class createINDoc : PX.Data.BQL.BqlBool.Field<createINDoc> { }
		/// <summary>
		/// The flag indicates that the Invoice contains at least one line with a Stock Item, therefore an Inventory Document should be created.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? CreateINDoc { get; set; }
		#endregion

        [Serializable]
        [PXHidden]
		public partial class ARRegister : AR.ARRegister
		{
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

			[PXDBString(1, IsFixed = true)]
			public override String Status
			{
				get
				{
					return this._Status;
				}
				set
				{
					this._Status = value;
				}
			}

			public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			[PXDBLong()]
			public override Int64? CuryInfoID
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
		}
	}

	// Used in AR676000.rpx
	public class FilterARTranType : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3, IsFixed = true)]
		[AR.ARDocType.SOEntryList]
		[PXUIField(DisplayName = "Type")]
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

	}

	// Used in SO643000.rpx
	public partial class SOInvoicePrintFormFilter : IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[ARInvoiceType.RefNbr(typeof(Search2<AR.Standalone.ARRegisterAlias.refNbr,
			InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>,
				And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Customer, On<AR.Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
			Where<AR.Standalone.ARRegisterAlias.docType, Equal<Optional<ARInvoice.docType>>,
				And<AR.Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleSO>, 
				And<Match<Customer, Current<AccessInfo.userName>>>>>, 
			OrderBy<Desc<AR.Standalone.ARRegisterAlias.refNbr>>>), Filterable = true)]
		[ARInvoiceType.Numbering()]
		[ARInvoiceNbr()]
		public String RefNbr { get; set; }
		#endregion

	}



}
