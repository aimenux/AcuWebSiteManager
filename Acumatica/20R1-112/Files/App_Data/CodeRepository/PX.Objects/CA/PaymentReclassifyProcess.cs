using PX.SM;
using System;
using PX.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CM;

namespace PX.Objects.CA
{
	[Serializable]
	public partial class CASplitExt : IBqlTable, ICADocSource
	{

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region AdjTranType
		public abstract class adjTranType : PX.Data.BQL.BqlString.Field<adjTranType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[CATranType.List]
		[PXUIField(DisplayName = "Type", Visible = true)]
		public virtual string AdjTranType
		{
			get;
			set;
		}
		#endregion
		#region AdjRefNbr
		public abstract class adjRefNbr : PX.Data.BQL.BqlString.Field<adjRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXSelector(typeof(CAAdj.adjRefNbr))]
		[PXUIField(DisplayName = "Ref. Nbr", Visible = true)]
		public virtual string AdjRefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Document Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }


		[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Tran. Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		[PXDefault(CADrCr.CADebit)]
		[PXDBString(1, IsFixed = true)]
		[CADrCr.List()]
		[PXUIField(DisplayName = "Disb. / Receipt", Enabled = false, Visible = false)]
		public virtual string DrCr
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodSelector(null, 
			typeof(CASplitExt.tranDate),
			branchSourceType: typeof(cashAccountID),
			branchSourceFormulaType: typeof(Selector<cashAccountID, CashAccount.branchID>),
		    masterFinPeriodIDType: typeof(tranPeriodID))]
		[PXUIField(DisplayName = "Fin. Period", Visible = false)]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
        #endregion
	    #region TranPeriodID
	    public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
	    [PeriodID]
	    public virtual string TranPeriodID
	    {
	        get;
	        set;
	    }
	    #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion

		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(DisplayName = "Offset Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Visible = true)]

		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount(typeof(CASplitExt.accountID), DisplayName = "Offset Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region ReclassCashAccountID
		public abstract class reclassCashAccountID : PX.Data.BQL.BqlInt.Field<reclassCashAccountID> { }


		[CashAccount(DisplayName = "Reclassify Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? ReclassCashAccountID
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		[PXDBCurrency(typeof(CASplitExt.curyInfoID), typeof(CASplitExt.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount", Visible = false)]
		[PXFormula(null, typeof(SumCalc<CAAdj.tranAmt>))]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion

		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared", Visible = false)]
		public virtual bool? Cleared
		{
			get;
			set;
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
		[PXDate]
		public virtual DateTime? ClearDate
		{
			get;
			set;
		}
		#endregion

		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR }, new string[] { BatchModule.AP, BatchModule.AR })]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		[PXDefault(GL.BatchModule.AR)]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion

		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }
		[PXDBInt]
		[PXDefault]
		[PXVendorCustomerSelector(typeof(CASplitExt.origModule))]
		[PXUIField(DisplayName = "Customer/Vendor ID", Visibility = PXUIVisibility.Visible)]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CASplitExt.referenceID>>,
						And<Location.isActive, Equal<boolTrue>>>), DescriptionField = typeof(Location.descr))]
		[PXUIField(DisplayName = "Location ID")]
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<CASplitExt.referenceID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion

		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Coalesce<
							Coalesce<
								Search2<Customer.defPaymentMethodID, InnerJoin<PaymentMethodAccount,
								   On<PaymentMethodAccount.paymentMethodID, Equal<Customer.defPaymentMethodID>,
									And<PaymentMethodAccount.useForAR, Equal<True>>>,
								   InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>,
									And<CashAccount.accountID, Equal<Current<CASplitExt.accountID>>,
									And<CashAccount.subID, Equal<Current<CASplitExt.subID>>>>>>>,
								  Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAR>,
									And<Customer.bAccountID, Equal<Current<CASplitExt.referenceID>>>>>,
								Search2<PaymentMethodAccount.paymentMethodID,
									InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
									Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAR>,
										And<PaymentMethodAccount.useForAR, Equal<True>,
										And<CashAccount.accountID, Equal<Current<CASplitExt.accountID>>,
										And<CashAccount.subID, Equal<Current<CASplitExt.subID>>>>>>,
													OrderBy<Desc<PaymentMethodAccount.aRIsDefault>>>>,
						   Search2<PaymentMethod.paymentMethodID, InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID,
									   Equal<PaymentMethod.paymentMethodID>,
										 And<PaymentMethodAccount.useForAP, Equal<True>>>,
									   InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>,
										 And<CashAccount.accountID, Equal<Current<CASplitExt.accountID>>,
										 And<CashAccount.subID, Equal<Current<CASplitExt.subID>>>>>>>,
									   Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAP>,
									   And<PaymentMethod.useForAP, Equal<True>,
									   And<PaymentMethod.isActive, Equal<boolTrue>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]

		[PXSelector(typeof(Search2<PaymentMethod.paymentMethodID,
				InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID,
				Equal<PaymentMethod.paymentMethodID>,
				And<Where2<Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAP>, And<PaymentMethodAccount.useForAP, Equal<True>>>,
						Or<Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAR>, And<PaymentMethodAccount.useForAR, Equal<True>>>>>>>,
				InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>,
					And<CashAccount.accountID, Equal<Current<CASplitExt.accountID>>,
					And<CashAccount.subID, Equal<Current<CASplitExt.subID>>>>>>>,
				Where<PaymentMethod.isActive, Equal<boolTrue>,
					And<Where2<Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAP>, And<PaymentMethod.useForAP, Equal<True>>>,
						Or<Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAR>, And<PaymentMethod.useForAR, Equal<True>>>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]


		[PXUIField(DisplayName = "Payment Method", Visible = true)]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXDefault(typeof(Coalesce<
							Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
								And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
								Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAR>,
								And<Customer.bAccountID, Equal<Current<CASplitExt.referenceID>>,
								And<CustomerPaymentMethod.isActive, Equal<True>,
								And<CustomerPaymentMethod.paymentMethodID, Equal<Current<CASplitExt.paymentMethodID>>>>>>>,
							Search<CustomerPaymentMethod.pMInstanceID,
								Where<Current<CASplitExt.origModule>, Equal<GL.BatchModule.moduleAR>,
							   And<CustomerPaymentMethod.bAccountID, Equal<Current<CASplitExt.referenceID>>,
								And<CustomerPaymentMethod.paymentMethodID, Equal<Current<CASplitExt.paymentMethodID>>,
								And<CustomerPaymentMethod.isActive, Equal<True>>>>>,
							OrderBy<Desc<CustomerPaymentMethod.expirationDate,
							Desc<CustomerPaymentMethod.pMInstanceID>>>>>),
								PersistingCheck = PXPersistingCheck.Nothing)]

		[PXSelector(typeof(Search2<CustomerPaymentMethod.pMInstanceID,
								InnerJoin<PaymentMethodAccount,
									On<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
									And<PaymentMethodAccount.useForAR, Equal<True>>>,
								InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>>,
								Where<CustomerPaymentMethod.bAccountID, Equal<Current<CASplitExt.referenceID>>,
								  And<CustomerPaymentMethod.paymentMethodID, Equal<Current<CASplitExt.paymentMethodID>>,
								  And<CustomerPaymentMethod.isActive, Equal<boolTrue>,
								  And<CashAccount.accountID, Equal<Current<CASplitExt.accountID>>,
								  And<CashAccount.subID, Equal<Current<CASplitExt.subID>>>>>>>>),
								  DescriptionField = typeof(CustomerPaymentMethod.descr))]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion

		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID>
		{
		}

		[PXDBLong]
		public virtual long? TranID
		{
			get;
			set;
		}
		#endregion
		#region RefAccountID
		public abstract class refAccountID : PX.Data.BQL.BqlInt.Field<refAccountID> { }
        [PXInt]
		public virtual int? RefAccountID
		{
			get;
			set;
		}
		#endregion

		#region ChildTranID
		public abstract class childTranID : PX.Data.BQL.BqlLong.Field<childTranID> { }

		[PXDBLong]
		public virtual long? ChildTranID
		{
			get;
			set;
		}
		#endregion
		#region ChildAccountID
		public abstract class childAccountID : PX.Data.BQL.BqlInt.Field<childAccountID> { }
        [PXInt]
		public virtual int? ChildAccountID
		{
			get;
			set;
		}
		#endregion
		#region ChildOrigModule
		public abstract class childOrigModule : PX.Data.BQL.BqlString.Field<childOrigModule> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Module")]
		public virtual string ChildOrigModule
		{
			get;
			set;
		}
		#endregion
		#region ChildOrigTranType
		public abstract class childOrigTranType : PX.Data.BQL.BqlString.Field<childOrigTranType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Reclassified Doc. Type")]
        [CAAPARTranType.ListByModule(typeof(childOrigModule))]
		public virtual string ChildOrigTranType
		{
			get;
			set;
		}
		#endregion
		#region ChildOrigRefNbr
		public abstract class childOrigRefNbr : PX.Data.BQL.BqlString.Field<childOrigRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Reclassified Ref. Nbr.")]
		public virtual string ChildOrigRefNbr
		{
			get;
			set;
		}
		#endregion

		public virtual void CopyFrom(CAAdj aAdj)
		{
			this.AdjTranType = aAdj.AdjTranType;
			this.AdjRefNbr = aAdj.AdjRefNbr;
			this.ExtRefNbr = aAdj.ExtRefNbr;
			this.TranDate = aAdj.TranDate;
			this.DrCr = aAdj.DrCr;
			this.TranDesc = aAdj.TranDesc;
			this.CashAccountID = aAdj.CashAccountID;
			this.CuryID = aAdj.CuryID;
			this.FinPeriodID = aAdj.FinPeriodID;
			this.TranPeriodID = aAdj.TranPeriodID;
			this.Cleared = aAdj.Cleared;
			if (String.IsNullOrEmpty(this.TranDesc))
				this.TranPeriodID = aAdj.TranDesc;

		}
		public virtual void CopyFrom(CASplit aSrc)
		{
			this.LineNbr = aSrc.LineNbr;
			this.AccountID = aSrc.AccountID;
			this.SubID = aSrc.SubID;
			this.CuryInfoID = aSrc.CuryInfoID;
			this.CuryTranAmt = aSrc.CuryTranAmt;
			this.TranAmt = aSrc.TranAmt;
			this.TranDesc = aSrc.TranDesc;
		}
		public virtual void CopyFrom(CATran aSrc)
		{
			this.TranID = aSrc.TranID;
			this.RefAccountID = aSrc.CashAccountID;
		}
		public virtual void CopyFrom(PaymentReclassifyProcess.CATranRef aSrc)
		{
			this.ChildTranID = aSrc.TranID;
			this.ChildAccountID = aSrc.CashAccountID;
			this.ChildOrigModule = aSrc.OrigModule;
			this.ChildOrigTranType = aSrc.OrigTranType;
			this.ChildOrigRefNbr = aSrc.OrigRefNbr;
			this.OrigModule = aSrc.OrigModule;
		}
		public virtual void CopyFrom(ARPayment aSrc)
		{
			this.ReferenceID = aSrc.CustomerID;
			this.LocationID = aSrc.CustomerLocationID;
			this.PMInstanceID = aSrc.PMInstanceID;
			this.PaymentMethodID = aSrc.PaymentMethodID;
		}
		public virtual void CopyFrom(APPayment aSrc)
		{
			this.ReferenceID = aSrc.VendorID;
			this.LocationID = aSrc.VendorLocationID;
			this.PaymentMethodID = aSrc.PaymentMethodID;
		}
		public virtual void CopyFrom(CashAccount aSrc)
		{
			if (aSrc != null)
				this.ReclassCashAccountID = aSrc.CashAccountID;
			else
				this.ReclassCashAccountID = null;
		}

		#region ICADocSource Members

		int? ICADocSource.BAccountID
		{
			get
			{
				return this.ReferenceID;
			}
		}

		int? ICADocSource.CARefTranAccountID
		{
			get
			{
				return this.RefAccountID;
			}
		}

		long? ICADocSource.CARefTranID
		{
			get
			{
				return this.TranID;
			}
		}
		int? ICADocSource.CARefSplitLineNbr
		{
			get
			{
				return this.LineNbr;
			}
		}

		decimal? ICADocSource.CuryOrigDocAmt
		{
			get
			{
				return this.CuryTranAmt;
			}
		}

		string ICADocSource.EntryTypeID
		{
			get { return null; }
		}

		int? ICADocSource.CashAccountID
		{
			get
			{
				return this.ReclassCashAccountID;
			}
		}

		string ICADocSource.InvoiceNbr
		{
			get { return null; }
		}

        public virtual Guid? NoteID
        {
            get
            {
                return null;
            }
        }
		#endregion
	}

    [Serializable]
    public class PaymentReclassifyProcess : PXGraph<PaymentReclassifyProcess>
    {
        public PXCancel<Filter> Cancel;
        public PXFilter<Filter> filter;
        [PXFilterable]
        public PXFilteredProcessing<CASplitExt, Filter> Adjustments;
        public PXAction<Filter> viewResultDocument;

		#region Setup Views
		public PXSetup<APSetup> apSetup;
		public PXSetup<ARSetup> arSetup;
		#endregion

        #region Internal Types Definition
        [Serializable]
        public partial class Filter : IBqlTable
        {
            #region EntryTypeID
            public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }
            protected String _EntryTypeID;
            [PXDBString(10, IsUnicode = true)]
            [PXDefault(typeof(Search<CASetup.unknownPaymentEntryTypeID>))]
            [PXSelector(typeof(Search<CAEntryType.entryTypeId,
                                Where<CAEntryType.module, Equal<BatchModule.moduleCA>,
                                And<CAEntryType.useToReclassifyPayments, Equal<True>>>>), DescriptionField = typeof(CAEntryType.descr))]
            [PXUIField(DisplayName = "Entry Type", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String EntryTypeID
            {
                get
                {
                    return this._EntryTypeID;
                }
                set
                {
                    this._EntryTypeID = value;
                }
            }
            #endregion
            #region AccountID
            public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
            protected Int32? _AccountID;
            [CashAccount(null, typeof(Search2<CashAccount.cashAccountID,
                                    InnerJoin<CashAccountETDetail,
                                            On<CashAccount.cashAccountID, Equal<CashAccountETDetail.accountID>>>,
                                        Where<CashAccountETDetail.entryTypeID, Equal<Current<Filter.entryTypeID>>>>),
                                         Visibility = PXUIVisibility.Visible)]
            public virtual Int32? AccountID
            {
                get
                {
                    return this._AccountID;
                }
                set
                {
                    this._AccountID = value;
                }
            }
            #endregion
            #region CuryID
            public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
            protected String _CuryID;
            [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
            [PXUIField(DisplayName = "Currency", Enabled = false)]
            [PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<Filter.accountID>>>>))]
            [PXSelector(typeof(CM.Currency.curyID))]
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
            #region StartDate
            public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
            protected DateTime? _StartDate;
            [PXDBDate()]
            [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
            public virtual DateTime? StartDate
            {
                get
                {
                    return this._StartDate;
                }
                set
                {
                    this._StartDate = value;
                }
            }
            #endregion
            #region EndDate
            public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
            protected DateTime? _EndDate;
            [PXDBDate()]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
            public virtual DateTime? EndDate
            {
                get
                {
                    return this._EndDate;
                }
                set
                {
                    this._EndDate = value;
                }
            }
            #endregion
            #region ShowSummary
            public abstract class showSummary : PX.Data.BQL.BqlBool.Field<showSummary> { }
            protected Boolean? _ShowSummary;
            [PXDBBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Show Summary", Visible = false)]
            public virtual Boolean? ShowSummary
            {
                get
                {
                    return this._ShowSummary;
                }
                set
                {
                    this._ShowSummary = value;
                }
            }
            #endregion
            #region IncludeUnreleased
            public abstract class includeUnreleased : PX.Data.BQL.BqlBool.Field<includeUnreleased> { }
            protected Boolean? _IncludeUnreleased;
            [PXDBBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Include Unreleased", Visible = false)]
            public virtual Boolean? IncludeUnreleased
            {
                get
                {
                    return this._IncludeUnreleased;
                }
                set
                {
                    this._IncludeUnreleased = value;
                }
            }
            #endregion
            #region ShowReclassified
            public abstract class showReclassified : PX.Data.BQL.BqlBool.Field<showReclassified> { }
            protected Boolean? _ShowReclassified;
            [PXDBBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Show Reclassified", Visible = true)]
            public virtual Boolean? ShowReclassified
            {
                get
                {
                    return this._ShowReclassified;
                }
                set
                {
                    this._ShowReclassified = value;
                }
            }
            #endregion
        }

        

        [Serializable]
        [PXHidden]
        public partial class CATranRef : CATran
        {
            #region CashAccountID
            public new abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
            #endregion
            #region TranID
            public new abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }
            #endregion
            #region RefTranAccountID
            public new abstract class refTranAccountID : PX.Data.BQL.BqlInt.Field<refTranAccountID> { }
            #endregion
            #region RefTranID
            public new abstract class refTranID : PX.Data.BQL.BqlLong.Field<refTranID> { }
            #endregion
            #region RefSplitLineNbr
            public new abstract class refSplitLineNbr : PX.Data.BQL.BqlInt.Field<refSplitLineNbr> { }
            #endregion
            #region OrigModule
            public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
            #endregion
            #region OrigTranType
            public new abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType> { }
            #endregion
            #region OrigRefNbr
            public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
            #endregion
            
        }


        #endregion

        #region Ctor + Members
        public PaymentReclassifyProcess()
        {
            this.Adjustments.SetSelected<CASplitExt.selected>();
            this.Adjustments.Cache.AllowUpdate = true;
            PXUIFieldAttribute.SetEnabled<CASplitExt.origModule>(this.Adjustments.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<CASplitExt.referenceID>(this.Adjustments.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<CASplitExt.locationID>(this.Adjustments.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<CASplitExt.paymentMethodID>(this.Adjustments.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<CASplitExt.pMInstanceID>(this.Adjustments.Cache, null, true);

			if (arSetup.Current.RequireExtRef == true && apSetup.Current.RequireVendorRef == true)
			{
				PXDefaultAttribute.SetPersistingCheck<CASplitExt.extRefNbr>(Adjustments.Cache, null, PXPersistingCheck.NullOrBlank);
			}

            this.Adjustments.SetProcessDelegate(delegate(CASplitExt aRow)
            {
                ReclassifyPaymentProc(aRow);
            });
        }


        [PXUIField(DisplayName = Messages.ViewResultingDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton(VisibleOnProcessingResults = true)]
        public virtual IEnumerable ViewResultDocument(PXAdapter adapter)
        {
            CASplitExt doc = this.Adjustments.Current;            
            if (doc != null && doc.TranID.HasValue)
            {
                CATran refTran = PXSelectJoin<CATran, InnerJoin<CATranRef, On<CATranRef.tranID, Equal<CATran.refTranID>,
                                    And<CATranRef.cashAccountID, Equal<CATran.refTranAccountID>>>>,                                    
                                    Where<CATranRef.tranID, Equal<Required<CATran.tranID>>,
                                        And<CATran.refSplitLineNbr, Equal<Required<CATran.refSplitLineNbr>>>>>.Select(this, doc.TranID, doc.LineNbr);
                if (refTran != null)
                {
                    if (refTran.OrigModule == GL.BatchModule.AR)
                    {
                        ARPaymentEntry paymentGraph = PXGraph.CreateInstance<ARPaymentEntry>();
                        paymentGraph.Document.Current = paymentGraph.Document.Search<ARRegister.refNbr>(refTran.OrigRefNbr, refTran.OrigTranType);

                        if (paymentGraph.Document.Current != null)
                            throw new PXRedirectRequiredException(paymentGraph, "");
                    }
                    else
                    {
                        if (refTran.OrigModule == GL.BatchModule.AP)
                        {
                            APPaymentEntry paymentGraph = PXGraph.CreateInstance<APPaymentEntry>();
                            paymentGraph.Document.Current = paymentGraph.Document.Search<APRegister.refNbr>(refTran.OrigRefNbr, refTran.OrigTranType);

                            if (paymentGraph.Document.Current != null)
                                throw new PXRedirectRequiredException(paymentGraph, "");
                        }
                    }
                }
            }
            return adapter.Get();
        }

        public override bool IsDirty
        {
            get
            {
                return false;
            }
        }

		protected virtual IEnumerable adjustments()
		{
			Filter current = this.filter.Current;
			List<CASplitExt> result = new List<CASplitExt>();
			if (current == null)
				yield break;
			//return result;
			PXSelectBase<CASplit> sel = new PXSelectJoin<CASplit, InnerJoin<CAAdj, On<CASplit.adjTranType, Equal<CAAdj.adjTranType>,
																		And<CASplit.adjRefNbr, Equal<CAAdj.adjRefNbr>>>,
																		InnerJoin<CATran, On<CATran.origModule, Equal<GL.BatchModule.moduleCA>,
																		And<CATran.origTranType, Equal<CASplit.adjTranType>,
																		And<CATran.origRefNbr, Equal<CASplit.adjRefNbr>>>>,
																	InnerJoin<CashAccount, On<CashAccount.branchID, Equal<CASplit.branchID>,
																		And<CashAccount.accountID, Equal<CASplit.accountID>,
																		And<CashAccount.subID, Equal<CASplit.subID>>>>,
																   LeftJoin<CATranRef, On<CATranRef.refTranAccountID, Equal<CATran.cashAccountID>,
																	 And<CATranRef.refTranID, Equal<CATran.tranID>,
																	 And<CATranRef.refSplitLineNbr, Equal<CASplit.lineNbr>>>>,
																   LeftJoin<ARPayment, On<CATranRef.origTranType, Equal<ARPayment.docType>,
																		And<CATranRef.origRefNbr, Equal<ARPayment.refNbr>,
																		And<CATranRef.origModule, Equal<BatchModule.moduleAR>>>>,
																   LeftJoin<APPayment, On<CATranRef.origTranType, Equal<APPayment.docType>,
																		And<CATranRef.origRefNbr, Equal<APPayment.refNbr>,
																		And<CATranRef.origModule, Equal<BatchModule.moduleAP>>>>>>>>>>,
																	Where<CAAdj.entryTypeID, Equal<Current<Filter.entryTypeID>>>>(this);
			if (current.AccountID != null)
			{
				sel.WhereAnd<Where<CAAdj.cashAccountID, Equal<Current<Filter.accountID>>>>();
			}
			else
			{
				if (!string.IsNullOrEmpty(current.CuryID))
					sel.WhereAnd<Where<CAAdj.curyID, Equal<Current<Filter.curyID>>>>();
			}
			if (current.EndDate.HasValue)
			{
				sel.WhereAnd<Where<CAAdj.tranDate, LessEqual<Current<Filter.endDate>>>>();
			}
			if (current.StartDate.HasValue)
			{
				sel.WhereAnd<Where<CAAdj.tranDate, GreaterEqual<Current<Filter.startDate>>>>();
			}
			if ((bool)current.IncludeUnreleased == false)
			{
				sel.WhereAnd<Where<CAAdj.released, Equal<boolTrue>>>();
			}
			if ((bool)current.ShowReclassified == false)
			{
				sel.WhereAnd<Where<CATranRef.tranID, IsNull>>();
			}
			else
			{
				sel.WhereAnd<Where<CATranRef.tranID, IsNotNull>>();
			}
			int count = 0;
			foreach (PXResult<CASplit, CAAdj, CATran, CashAccount, CATranRef, ARPayment, APPayment> it in sel.Select())
			{
				CASplitExt res = new CASplitExt();
				CASplit split = (CASplit)it;
				CAAdj adj = (CAAdj)it;
				CashAccount cashAccount = (CashAccount)it;
				res.CopyFrom(split);
				res.CopyFrom(adj);
				res.CopyFrom((CATran)it);
				res.CopyFrom((CATranRef)it);
				res.CopyFrom(cashAccount);


				if (current.ShowReclassified == true)
				{
					if (res.ChildOrigModule == BatchModule.AR)
						res.CopyFrom((ARPayment)it);
					else
						res.CopyFrom((APPayment)it);
				}
				count++;

				CASplitExt row = null;
				foreach (CASplitExt jt in this.Adjustments.Cache.Inserted)
				{
					if (jt.AdjRefNbr == res.AdjRefNbr && jt.AdjTranType == res.AdjTranType && jt.LineNbr == res.LineNbr)
					{
						row = jt;
					}
				}
				if (row == null)
				{
					if (current.ShowReclassified == true)
						yield return res;
					else
						yield return row = this.Adjustments.Insert(res);
				}
				else
					yield return row;
			}

		}

        #endregion

        #region Events
        protected virtual void CASplitExt_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CASplitExt row = (CASplitExt)e.Row;

            if (row != null)
            {
                bool isReclassified = row.ChildTranID.HasValue;
                bool isAP = (row.OrigModule == GL.BatchModule.AP);
                bool isAR = (row.OrigModule == GL.BatchModule.AR);
                PXUIFieldAttribute.SetEnabled<CASplitExt.paymentMethodID>(sender, e.Row, !isReclassified && (isAP || isAR));
                bool isPMInstanceRequired = false;
                if (!isReclassified && isAR && String.IsNullOrEmpty(row.PaymentMethodID) == false)
                {
                    PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
                    isPMInstanceRequired = (pm.IsAccountNumberRequired == true);
                }

                CashAccount cashAcct = PXSelectReadonly<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
                    And<CashAccount.subID, Equal<Required<CashAccount.subID>>>>>.Select(this, row.AccountID, row.SubID);

				if (!isReclassified)
				{
					PaymentMethodAccount pmAccount = PXSelectReadonly2<PaymentMethodAccount, InnerJoin<PaymentMethod, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
												And<PaymentMethod.isActive, Equal<True>>>>,
												Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
													And<Where2<Where<PaymentMethodAccount.useForAP, Equal<Required<PaymentMethodAccount.useForAP>>,
																And<PaymentMethodAccount.useForAP, Equal<True>>>,
														Or<Where<PaymentMethodAccount.useForAR, Equal<Required<PaymentMethodAccount.useForAR>>,
																And<PaymentMethodAccount.useForAR, Equal<True>>>>>>>>.Select(this, cashAcct.CashAccountID, isAP, isAR);

					if (pmAccount == null || pmAccount.CashAccountID.HasValue == false)
					{
						Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.AccountID);
						string accountCD = account.AccountCD;
						sender.RaiseExceptionHandling<CASplitExt.paymentMethodID>(e.Row, null, new PXSetPropertyException(Messages.NoActivePaymentMethodIsConfigueredForCashAccountInModule, PXErrorLevel.Warning, cashAcct.CashAccountCD, row.OrigModule));
						sender.RaiseExceptionHandling<CASplitExt.accountID>(e.Row, accountCD, new PXSetPropertyException(Messages.NoActivePaymentMethodIsConfigueredForCashAccountInModule, PXErrorLevel.Warning, accountCD, row.OrigModule));
					}
					else
					{
						sender.RaiseExceptionHandling<CASplitExt.paymentMethodID>(e.Row, null, null);
						sender.RaiseExceptionHandling<CASplitExt.accountID>(e.Row, null, null);
					}
					APSetup apsetup = PXSelect<APSetup>.Select(this);
					ARSetup arsetup = PXSelect<ARSetup>.Select(this);
					if (String.IsNullOrEmpty(row.ExtRefNbr) && row.Selected == true &&
						((isAR && arsetup.RequireExtRef == true) || (isAP && apsetup.RequireVendorRef == true)))
					{
						sender.RaiseExceptionHandling<CASplitExt.extRefNbr>(row, row.ExtRefNbr, new PXException(ErrorMessages.FieldIsEmpty, typeof(CASplitExt.extRefNbr).Name));
					}
					else
					{
						sender.RaiseExceptionHandling<CASplitExt.extRefNbr>(row, null, null);
					}

					if (row.ReferenceID == null && row.Selected == true)
					{
						sender.RaiseExceptionHandling<CASplitExt.referenceID>(row, row.ReferenceID, new PXException(ErrorMessages.FieldIsEmpty, typeof(CASplitExt.referenceID).Name));
					}
					else
					{
						sender.RaiseExceptionHandling<CASplitExt.referenceID>(row, null, null);
					}
				}
                PXUIFieldAttribute.SetEnabled<CASplitExt.pMInstanceID>(sender, e.Row, isPMInstanceRequired);
            }
        }
        protected virtual void CASplitExt_OrigModule_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            CASplitExt row = (CASplitExt)e.Row;
            if (row != null)
            {
                if (row.DrCr == CADrCr.CACredit)
                    e.NewValue = GL.BatchModule.AP;
                else if (row.DrCr == CADrCr.CADebit)
                    e.NewValue = GL.BatchModule.AR;
            }
        }

        protected virtual void CASplitExt_OrigModule_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CASplitExt row = (CASplitExt)e.Row;
            if (this.filter.Current.ShowReclassified == false)
            {
                sender.SetDefaultExt<CASplitExt.referenceID>(e.Row);
                sender.SetDefaultExt<CASplitExt.locationID>(e.Row);
                sender.SetDefaultExt<CASplitExt.paymentMethodID>(row);
                sender.SetDefaultExt<CASplitExt.pMInstanceID>(e.Row);
            }
        }


        protected virtual void CASplitExt_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (this.filter.Current != null && this.filter.Current.ShowReclassified == true)
                e.Cancel = true;

        }

        protected virtual void CASplitExt_ReferenceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CASplitExt row = (CASplitExt)e.Row;
            sender.SetDefaultExt<CASplitExt.locationID>(e.Row);
            sender.SetDefaultExt<CASplitExt.paymentMethodID>(e.Row);
            sender.SetDefaultExt<CASplitExt.pMInstanceID>(e.Row);
        }

        protected virtual void CASplitExt_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<CASplitExt.pMInstanceID>(e.Row);
        }

        protected virtual void CASplitExt_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CASplitExt row = (CASplitExt)e.Row;
        }

        protected virtual void Filter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            Filter row = (Filter)e.Row;
            if (row != null)
            {
                bool showReclassified = (bool)row.ShowReclassified;
                PXCache cache = this.Adjustments.Cache;
				PXUIFieldAttribute.SetEnabled<CASplitExt.referenceID>(cache, null, !showReclassified);
				PXUIFieldAttribute.SetEnabled<CASplitExt.extRefNbr>(cache, null, !showReclassified);
                PXUIFieldAttribute.SetEnabled<CASplitExt.locationID>(cache, null, !showReclassified);
                PXUIFieldAttribute.SetEnabled<CASplitExt.paymentMethodID>(cache, null, !showReclassified);
                PXUIFieldAttribute.SetEnabled<CASplitExt.pMInstanceID>(cache, null, !showReclassified);
                PXUIFieldAttribute.SetEnabled<CASplitExt.origModule>(cache, null, !showReclassified);
                PXUIFieldAttribute.SetEnabled<CASplitExt.selected>(cache, null, !showReclassified);

                PXUIFieldAttribute.SetVisible<CASplitExt.childOrigModule>(cache, null, showReclassified);
                PXUIFieldAttribute.SetVisible<CASplitExt.childOrigTranType>(cache, null, showReclassified);
                PXUIFieldAttribute.SetVisible<CASplitExt.childOrigRefNbr>(cache, null, showReclassified);

				PXUIFieldAttribute.SetVisible<Filter.curyID>(sender, row, row.AccountID.HasValue);
            }
        }
      
        protected virtual void Filter_ShowReclassified_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Filter row = (Filter)e.Row;
            if (row.ShowReclassified == true)
            {
                int periodLength = 7;
                DateTime startDate = row.EndDate.HasValue ? row.EndDate.Value.AddDays(-periodLength) : DateTime.Today.AddDays(-periodLength);
                sender.SetValueExt<Filter.startDate>(row, startDate);
            }
            else
            {
                sender.SetValueExt<Filter.startDate>(row, null);
            }
        }
        #endregion

		protected virtual void Filter_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter row = (Filter)e.Row;
			if(row.AccountID != (int?)e.OldValue)
			{
				sender.SetDefaultExt<Filter.curyID>(row);
			}
		}

		protected virtual void Filter_EntryTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter row = (Filter)e.Row;
			if (row.EntryTypeID != (string)e.OldValue)
			{
				sender.SetDefaultExt<Filter.accountID>(row);
			}
		}

        #region Processing Functions
        protected static void ReclassifyPaymentProc(CASplitExt aRow)
        {
            if (aRow.OrigModule == GL.BatchModule.AR)
            {
                AddARTransaction(aRow, null);
            }
            if (aRow.OrigModule == GL.BatchModule.AP)
            {
                AddAPTransaction(aRow, null);
            }
        }

        public static CATran AddAPTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo)
        {
	        CheckAPTransaction(parameters);
            return AddAPTransaction(parameters, aCuryInfo, null, true);
        }

	    public static void CheckAPTransaction(ICADocSource parameters)
	    {
		    if (parameters.OrigModule == GL.BatchModule.AP)
		    {
				if (parameters.BAccountID == null)
				{
					throw new PXRowPersistingException(typeof(CASplitExt.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.referenceID).Name);
				}

				if (parameters.LocationID == null)
				{
					throw new PXRowPersistingException(typeof(CASplitExt.locationID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.locationID).Name);
				}

				if (string.IsNullOrEmpty(parameters.PaymentMethodID))
				{
					throw new PXRowPersistingException(typeof(CASplitExt.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.paymentMethodID).Name);
				}

				APPaymentEntry te = PXGraph.CreateInstance<APPaymentEntry>();
				Vendor vend = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(te, parameters.BAccountID);

				if (vend == null)
				{
					throw new PXRowPersistingException(typeof(CASplitExt.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.referenceID).Name);
				}
			}
	    }

        public static CATran AddAPTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo, IList<ICADocAdjust> aAdjustments, bool aOnHold)
        {
            if (parameters.OrigModule == GL.BatchModule.AP)
            {
				APPaymentEntry te = PXGraph.CreateInstance<APPaymentEntry>();
				te.Document.View.Answer = WebDialogResult.No;
                APPayment doc = new APPayment();

				if (!(parameters.DrCr == CADrCr.CACredit ^ Math.Sign(parameters.CuryOrigDocAmt.Value) > 0))
                {
                    decimal? sumOfAdjustments = aAdjustments == null ? 0.0m : aAdjustments
                        .Select(adj => new APAdjust { AdjgDocType = APDocType.Check, AdjdDocType = adj.AdjdDocType, CuryAdjgAmt = adj.CuryAdjgAmount })
                        .Sum(adj => adj.AdjgBalSign * adj.CuryAdjgAmt);

                    if (sumOfAdjustments == parameters.CuryOrigDocAmt)
                    {
                        doc.DocType = APDocType.Check;
                    }
                    else
                    {
                        doc.DocType = APDocType.Prepayment;

                        if (aAdjustments != null && aAdjustments.Any(adj => adj.AdjdDocType == APDocType.DebitAdj || adj.AdjdDocType == APDocType.Prepayment))
                            throw new PXException(Messages.CanApplyPrepaymentToDrAdjOrPrepayment);
                    }

                }
                else
                {
                    doc.DocType = APDocType.Refund;
                }

                doc = PXCache<APPayment>.CreateCopy(te.Document.Insert(doc));
                doc.VendorID = parameters.BAccountID;
                doc.VendorLocationID = parameters.LocationID;
                doc.CashAccountID = parameters.CashAccountID;
                doc.PaymentMethodID = parameters.PaymentMethodID;
                doc.AdjDate = parameters.TranDate;
                doc = PXCache<APPayment>.CreateCopy(te.Document.Update(doc));
                doc.CuryID = parameters.CuryID;
                doc.CuryOrigDocAmt = Math.Abs(parameters.CuryOrigDocAmt.Value);
                doc.DocDesc = parameters.TranDesc;
                doc.Cleared = parameters.Cleared;
				if (doc.Cleared == true)
				{
					doc.ClearDate = parameters.ClearDate;
				}
				doc.PrintCheck = false;
				doc.Printed = true;
                if (!String.IsNullOrEmpty(parameters.FinPeriodID))
                    doc.FinPeriodID = parameters.FinPeriodID;
                doc.ExtRefNbr = parameters.ExtRefNbr;
                doc.Hold = aOnHold;

                doc.CARefTranAccountID = parameters.CARefTranAccountID;
                doc.CARefTranID = parameters.CARefTranID;
                doc.CARefSplitLineNbr = parameters.CARefSplitLineNbr;

				doc.RefNoteID = parameters.NoteID;

	            var sourceCache = te.Caches[parameters.GetType()];
	            PXNoteAttribute.CopyNoteAndFiles(sourceCache, parameters, te.Document.Cache, doc);

                doc = PXCache<APPayment>.CreateCopy(te.Document.Update(doc));
                CurrencyInfo refInfo = aCuryInfo;
                if (refInfo == null)
                {
                    refInfo = (CurrencyInfo)PXSelectReadonly<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(te, parameters.CuryInfoID);
                }
                if (refInfo != null)
                {
                    foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>>.Select(te))
                    {
                        CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(refInfo);
                        new_info.CuryInfoID = info.CuryInfoID;
                        te.currencyinfo.Cache.Update(new_info);
                    }
                }

                if (aAdjustments != null)
                {
                    foreach (ICADocAdjust it in aAdjustments)
                    {
                        APAdjust adjust = new APAdjust();
                        adjust.AdjdDocType = it.AdjdDocType;
                        adjust.AdjdRefNbr = it.AdjdRefNbr;
						try
						{
							adjust = te.Adjustments.Insert(adjust);
						}
						catch (PXFieldValueProcessingException e)
						{
							APAdjust existingAdjust = PXSelectReadonly<APAdjust, Where<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>, And<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>, And<APAdjust.released, Equal<False>>>>>.Select(te, it.AdjdRefNbr, it.AdjdDocType);
							APInvoice invoice =PXSelectReadonly<APInvoice, Where<APInvoice.refNbr, Equal<Required<APAdjust.adjdRefNbr>>, And<APInvoice.docType, Equal<Required<APAdjust.adjdDocType>>>>>.Select(te, it.AdjdRefNbr, it.AdjdDocType);
							if (existingAdjust != null || invoice.Status == ARDocStatus.Closed)
								throw new PXException(Messages.CouldNotAddApplication, it.AdjdRefNbr);
							else throw e;
						}
						adjust.AdjdCuryRate = it.AdjdCuryRate;
						adjust = te.Adjustments.Update(adjust);
						if (it.CuryAdjgAmount.HasValue)
                        {
                            adjust.CuryAdjgAmt = it.CuryAdjgAmount;
                        }
                        adjust.CuryAdjgDiscAmt = it.CuryAdjgDiscAmt;
                        adjust.CuryAdjgWhTaxAmt = it.CuryAdjgWhTaxAmt;
                        adjust = te.Adjustments.Update(adjust);
                    }
                }
                te.Save.Press();
                return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Current<APPayment.cATranID>>>>.Select(te);
            }
            return null;
        }

        public static CATran AddARTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo)
        {
	        CheckARTransaction(parameters);
            return AddARTransaction(parameters, aCuryInfo, (IEnumerable<ICADocAdjust>)null, true);
        }

		public static void CheckARTransaction(ICADocSource parameters)
	    {
			if (parameters.OrigModule == GL.BatchModule.AR)
			{
				if (parameters.CashAccountID == null)
				{
                    throw new PXRowPersistingException(typeof(CASplitExt.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.cashAccountID).Name);
				}

				if (parameters.BAccountID == null)
				{
                    throw new PXRowPersistingException(typeof(CASplitExt.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.referenceID).Name);
				}

				if (parameters.LocationID == null)
				{
                    throw new PXRowPersistingException(typeof(CASplitExt.locationID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.locationID).Name);
				}

				if (String.IsNullOrEmpty(parameters.PaymentMethodID))
				{
                    throw new PXRowPersistingException(typeof(CASplitExt.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.paymentMethodID).Name);
				}

				ARPaymentEntry te = PXGraph.CreateInstance<ARPaymentEntry>();
				if (parameters.PMInstanceID == null)
				{
					PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(te, parameters.PaymentMethodID);
					if (pm.IsAccountNumberRequired == true)
					{
                        throw new PXRowPersistingException(typeof(CASplitExt.pMInstanceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.pMInstanceID).Name);
					}
				}

				Customer cust = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.
					Select(te, parameters.BAccountID);

				if (cust == null)
				{
                    throw new PXRowPersistingException(typeof(CASplitExt.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CASplitExt.referenceID).Name);
				}
			}
	    }

        public static CATran AddARTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo, IEnumerable<ICADocAdjust> aAdjustments, bool aOnHold)
        {
			List<ARAdjust> arAdjustments = new List<ARAdjust>();
			if (aAdjustments != null)
			{
				foreach (ICADocAdjust it in aAdjustments)
				{
					ARAdjust adjust = new ARAdjust();
					adjust.AdjdDocType = it.AdjdDocType;
					adjust.AdjdRefNbr = it.AdjdRefNbr;
					adjust.CuryAdjgDiscAmt = it.CuryAdjgDiscAmt;
					adjust.CuryAdjgWOAmt = it.CuryAdjgWhTaxAmt;
					adjust.AdjdCuryRate = it.AdjdCuryRate;
					if (it.CuryAdjgAmount.HasValue)
					{
						adjust.CuryAdjgAmt = it.CuryAdjgAmount;
					}
					adjust.CuryAdjgDiscAmt = it.CuryAdjgDiscAmt;
					adjust.CuryAdjgWOAmt = it.CuryAdjgWhTaxAmt;
					arAdjustments.Add(adjust);
				}
			}
			return AddARTransaction(parameters, aCuryInfo, arAdjustments, aOnHold);
		}
		public static CATran AddARTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo, IEnumerable<ARAdjust> aAdjustments, bool aOnHold)
		{
            if (parameters.OrigModule == GL.BatchModule.AR)
            {
                ARPayment doc = new ARPayment();
				ARPaymentEntry te = PXGraph.CreateInstance<ARPaymentEntry>();
                if (!(parameters.DrCr == CADrCr.CACredit ^ Math.Sign(parameters.CuryOrigDocAmt.Value) > 0))
                {
                    doc.DocType = ARDocType.Refund;
                }
                else
                {
                    doc.DocType = ARDocType.Payment;
                }
                doc = PXCache<ARPayment>.CreateCopy(te.Document.Insert(doc));

                doc.CustomerID = parameters.BAccountID;
                doc.CustomerLocationID = parameters.LocationID;
                doc.PaymentMethodID = parameters.PaymentMethodID;
                doc.PMInstanceID = parameters.PMInstanceID;
                doc.CashAccountID = parameters.CashAccountID;
                doc.CuryOrigDocAmt = Math.Abs(parameters.CuryOrigDocAmt.Value);
                doc.DocDesc = parameters.TranDesc;
                doc.Cleared = parameters.Cleared;
				if (doc.Cleared == true)
				{
					doc.ClearDate = parameters.ClearDate;
				}
				doc.AdjDate = parameters.TranDate;
                if (!String.IsNullOrEmpty(parameters.FinPeriodID))
                    doc.FinPeriodID = parameters.FinPeriodID;
                doc.ExtRefNbr = parameters.ExtRefNbr;
                doc.CARefTranAccountID = parameters.CARefTranAccountID;
                doc.CARefTranID = parameters.CARefTranID;
                doc.CARefSplitLineNbr = parameters.CARefSplitLineNbr;
                doc.Hold = aOnHold;
                if (aCuryInfo == null)
                    doc.CuryID = parameters.CuryID;
                doc.RefNoteID = parameters.NoteID;
                doc = PXCache<ARPayment>.CreateCopy(te.Document.Update(doc));

	            var sourceCache = te.Caches[parameters.GetType()];
				PXNoteAttribute.CopyNoteAndFiles(sourceCache, parameters, te.Document.Cache, doc);

				doc = PXCache<ARPayment>.CreateCopy(te.Document.Update(doc));
                CurrencyInfo refInfo = aCuryInfo;
                if (refInfo == null)
                {
                    refInfo = (CurrencyInfo)PXSelectReadonly<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(te, parameters.CuryInfoID);
                }

                if (refInfo != null)
                {
                    foreach (CurrencyInfo info in te.currencyinfo.Select())
                    {
                        PXCache<CurrencyInfo>.RestoreCopy(info, refInfo);
                        info.CuryInfoID = doc.CuryInfoID;
                        doc.CuryID = info.CuryID;
                    }
                }
                Decimal curyAppliedAmt = Decimal.Zero;
                if (aAdjustments != null)
                {
					foreach (ARAdjust it in aAdjustments)
                    {
						
						ARAdjust adjust = new ARAdjust();
						adjust.AdjdDocType = it.AdjdDocType;
						adjust.AdjdRefNbr = it.AdjdRefNbr;
						try
						{
							adjust = te.Adjustments.Insert(adjust);
						}
						catch (PXFieldValueProcessingException e)
						{
							ARAdjust existingAdjust = PXSelectReadonly<ARAdjust, Where<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.released, Equal<False>>>>>.Select(te, it.AdjdRefNbr, it.AdjdDocType);
							ARInvoice invoice =PXSelectReadonly<ARInvoice, Where<ARInvoice.refNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARInvoice.docType, Equal<Required<ARAdjust.adjdDocType>>>>>.Select(te, it.AdjdRefNbr, it.AdjdDocType);
							if (existingAdjust != null || invoice.Status == ARDocStatus.Closed)
								throw new PXException(Messages.CouldNotAddApplication, it.AdjdRefNbr);
							else throw e;
						}
						
						adjust.AdjdCuryRate = it.AdjdCuryRate;
						adjust = te.Adjustments.Update(adjust);
						if (adjust.CuryAdjgAmt.HasValue)
                        {
							adjust.CuryAdjgAmt = it.CuryAdjgAmt;
							curyAppliedAmt += (it.CuryAdjgAmt ?? Decimal.Zero);
                        }
						adjust.WriteOffReasonCode = it.WriteOffReasonCode;
						adjust.WOBal = it.WOBal;
						adjust.AdjWOAmt = it.AdjWOAmt;
						adjust.CuryAdjdWOAmt = it.CuryAdjdWOAmt;
						adjust.CuryAdjgWOAmt = it.CuryAdjgWOAmt;
						adjust.CuryWOBal = it.CuryWOBal;
						adjust.CuryAdjgDiscAmt = it.CuryAdjgDiscAmt;
						adjust.CuryAdjgWOAmt = it.CuryAdjgWOAmt;
						te.Adjustments.Update(adjust);
					}
                }

                if (doc.DocType == ARDocType.Refund && doc.Hold != true && curyAppliedAmt < doc.CuryOrigDocAmt) 
                {
                    throw new PXException(Messages.DocumentMustByAppliedInFullBeforeItMayBeCreated);
                }

                te.Save.Press();
                return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Current<ARPayment.cATranID>>>>.Select(te);
            }
            return null;
        }
        #endregion
    }

	public interface ICADocSource
	{
		string CuryID { get; }
		int? CashAccountID { get; }
		long? CuryInfoID { get; }
		DateTime? TranDate { get; }
		int? BAccountID { get; }
		int? LocationID { get; }
		string OrigModule { get; }
		decimal? CuryOrigDocAmt { get; }
		string DrCr { get; }
		string ExtRefNbr { get; }
		string TranDesc { get; }
		string FinPeriodID { get; }
		string PaymentMethodID { get; }
		string InvoiceNbr { get; }
		bool? Cleared { get; }
		DateTime? ClearDate { get; }
		int? CARefTranAccountID { get; }
		long? CARefTranID { get; }
		int? CARefSplitLineNbr { get; }
		int? PMInstanceID { get; }
		string EntryTypeID { get; }
		Guid? NoteID { get; }
	}

    public interface ICADocAdjust
    {
        string AdjdDocType { get; set; }
        string AdjdRefNbr { get; set; }
        Decimal? CuryAdjgAmount { get; set; }
        Decimal? CuryAdjgWhTaxAmt { get; set; }
        Decimal? CuryAdjgDiscAmt { get; set; }
        Decimal? AdjdCuryRate { get; set; }
    }
}