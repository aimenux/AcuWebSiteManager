using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.TX;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CA
{
	[Serializable]
	public partial class AddTrxFilter : IBqlTable
	{
		#region OnlyExpense
		public abstract class onlyExpense : PX.Data.BQL.BqlBool.Field<onlyExpense> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? OnlyExpense
		{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Doc. Date", Required = true)]
		[AddFilter]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDefault]
		[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(typeof(Search<CashAccount.branchID, Where<CashAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>))]
		public virtual int? BranchID
		{
            get;
            set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Ref.", Required = true)]
		[AP.PaymentRef(typeof(AddTrxFilter.cashAccountID), typeof(AddTrxFilter.paymentMethodID), null)]
		public virtual string ExtRefNbr { get; set; }
		#endregion
		#region EntryTypeID
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
			InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
			Where<CashAccountETDetail.accountID, Equal<Current<AddTrxFilter.cashAccountID>>,
				And2<Where<Current<AddTrxFilter.onlyExpense>, Equal<False>, 
					Or<CAEntryType.module, Equal<BatchModule.moduleCA>>>,
				And2<Where<Current<APSetup.migrationMode>, NotEqual<True>,
					Or<CAEntryType.module, NotEqual<BatchModule.moduleAP>>>,
				And<Where<Current<ARSetup.migrationMode>, NotEqual<True>,
					Or<CAEntryType.module, NotEqual<BatchModule.moduleAR>>>>>>>>),
			DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Entry Type", Required = true)]
		public virtual string EntryTypeID
		{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }

		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(Search<CAEntryType.module, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>))]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[CAAPAROpenPeriod(typeof(origModule), typeof(tranDate), typeof(branchID),
			masterFinPeriodIDType: typeof(tranPeriodID),
			ValidatePeriod = PeriodValidation.DefaultSelectUpdate)]
		[PXFormula(typeof(Default<tranDate, cashAccountID, branchID, entryTypeID>))]
		[PXUIField(DisplayName = "Fin. Period", Required = true)]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : IBqlField { }

		[PeriodID]
		public virtual string TranPeriodID { get; set; }
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }

		[PXDefault(GL.DrCr.Debit, typeof(Search<CAEntryType.drCr, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>))]
		[PXDBString(1, IsFixed = true)]
		[CADrCr.List]
		[PXUIField(DisplayName = "Disb. / Receipt", Enabled = false)]
		public virtual string DrCr
		{
			get;
			set;
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXDefault(typeof(Search<CashAccountETDetail.taxZoneID, Where<CashAccountETDetail.accountID, Equal<Current<AddTrxFilter.cashAccountID>>, And<CashAccountETDetail.entryTypeID, Equal<Current<AddTrxFilter.entryTypeID>>>>>))]
		public virtual string TaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDefault(typeof(Search<CAEntryType.descr, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		
		[PXDefault(typeof(Coalesce<Search<CashAccountETDetail.offsetAccountID, Where<CashAccountETDetail.entryTypeID, Equal<Current<AddTrxFilter.entryTypeID>>,
							And<CashAccountETDetail.accountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>,
						Search<CAEntryType.accountID, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>>))]
		[Account(typeof(AddTrxFilter.branchID), typeof(Search2<Account.accountID, LeftJoin<CashAccount, On<CashAccount.accountID, Equal<Account.accountID>>,
													InnerJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>>,
												Where2<Where2<
													Where<CAEntryType.useToReclassifyPayments, Equal<False>,
														And<Where<Account.curyID, IsNull, 
															Or<Account.curyID, Equal<Current<AddTrxFilter.curyID>>>>>>,
													Or<Where<CashAccount.cashAccountID, IsNotNull,
														And<CashAccount.curyID, Equal<Current<AddTrxFilter.curyID>>,
															And<CashAccount.cashAccountID, NotEqual<Current<AddTrxFilter.cashAccountID>>>>>>>,
													And<Match<Current<AccessInfo.userName>>>>>),
			DisplayName = "Offset Account",
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[SubAccount(typeof(AddTrxFilter.accountID), DisplayName = "Offset Subaccount", DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Coalesce<Search<CashAccountETDetail.offsetSubID, Where<CashAccountETDetail.entryTypeID, Equal<Current<AddTrxFilter.entryTypeID>>,
							And<CashAccountETDetail.accountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>,
				Search<CAEntryType.subID, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>>))]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDBInt]
		[PXDefault(typeof(Search<CAEntryType.referenceID, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>))]
		[PXVendorCustomerSelector(typeof(AddTrxFilter.origModule))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.Visible)]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Required = true)]
		[PXDBCurrency(typeof(AddTrxFilter.curyInfoID), typeof(AddTrxFilter.tranAmt))]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared")]
		public virtual bool? Cleared
		{
			get;
			set;
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Date Cleared")]
		public virtual DateTime? ClearDate
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXDBLong]
		[CurrencyInfo(ModuleCode = BatchModule.CA, CuryIDField = "CuryID")]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<AddTrxFilter.referenceID>>, And<Location.isActive, Equal<boolTrue>>>), DescriptionField = typeof(Location.descr))]
		[PXUIField(DisplayName = "Location ID")]
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<AddTrxFilter.referenceID>>>>))]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Required = true)]
		[PXDefault(typeof(Coalesce<
							Coalesce<
								Search2<Customer.defPaymentMethodID, InnerJoin<PaymentMethodAccount,
									   On<PaymentMethodAccount.paymentMethodID, Equal<Customer.defPaymentMethodID>,
										And<PaymentMethodAccount.useForAR, Equal<True>,
										And<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>>,
									   Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>,
										And<Customer.bAccountID, Equal<Current<AddTrxFilter.referenceID>>>>>,
								Search<PaymentMethodAccount.paymentMethodID,
										Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>,
											And<PaymentMethodAccount.useForAR, Equal<True>,
											And<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>,
											OrderBy<Desc<PaymentMethodAccount.aRIsDefault>>>>,
							 Coalesce<
									Search2<Location.paymentMethodID, InnerJoin<PaymentMethodAccount,
										On<PaymentMethodAccount.paymentMethodID, Equal<Location.paymentMethodID>,
										And<PaymentMethodAccount.useForAP, Equal<True>,
										And<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>>,
									Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAP>,
										And<Location.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
										And<Location.locationID, Equal<Current<AddTrxFilter.locationID>>>>>>,
									Search<PaymentMethodAccount.paymentMethodID,
										Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAP>,
										And<PaymentMethodAccount.useForAP, Equal<True>,
										And<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>,
										OrderBy<Desc<PaymentMethodAccount.aPIsDefault>>>>>),
										PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<PaymentMethod.paymentMethodID,
							InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
								And<Where2<Where<PaymentMethodAccount.useForAR, Equal<True>,
									And<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>>>,
								Or<Where<PaymentMethodAccount.useForAP, Equal<True>,
									And<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAP>>>>>>>>,
								Where<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>))]
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
								Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>,
								And<Customer.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
								And<CustomerPaymentMethod.isActive, Equal<True>,
								And<CustomerPaymentMethod.paymentMethodID, Equal<Current<AddTrxFilter.paymentMethodID>>>>>>>,
							Search<CustomerPaymentMethod.pMInstanceID,
								Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>,
									And<CustomerPaymentMethod.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
									And<CustomerPaymentMethod.paymentMethodID, Equal<Current<AddTrxFilter.paymentMethodID>>,
									And<CustomerPaymentMethod.isActive, Equal<True>>>>>,
								OrderBy<Desc<CustomerPaymentMethod.expirationDate,
								Desc<CustomerPaymentMethod.pMInstanceID>>>>>),
									PersistingCheck = PXPersistingCheck.Nothing)]

		[PXSelector(typeof(Search2<CustomerPaymentMethod.pMInstanceID,
								InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID,
									Equal<CustomerPaymentMethod.paymentMethodID>,
								And<PaymentMethodAccount.useForAR, Equal<True>>>>,
								Where<CustomerPaymentMethod.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
								  And<CustomerPaymentMethod.paymentMethodID, Equal<Current<AddTrxFilter.paymentMethodID>>,
								  And<CustomerPaymentMethod.isActive, Equal<boolTrue>,
								  And<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>>>),
								  DescriptionField = typeof(CustomerPaymentMethod.descr))]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXString(11, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false, Visible = false)]
		[BatchStatus.List]
		public virtual string Status
		{
			get
			{
				if (Hold == true)
				{
					return BatchStatus.Hold;
				}
				else
				{
					return BatchStatus.Balanced;
				}
			}
			set
			{
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		[PXDBBool]
		[PXDefault(typeof(Search<CASetup.holdEntry>))]
		[PXUIField(DisplayName = "Hold")]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region Methods
		public static CATran VerifyAndCreateTransaction(PXGraph graph, PXFilter<AddTrxFilter> addFilter, PXSelectBase<CurrencyInfo> currencyinfo)
		{
			AddTrxFilter parameters = addFilter.Current;
			VerifyCommonFields(addFilter, parameters);
			CurrencyInfo defcurrencyinfo = currencyinfo.Current;
			if (parameters.OrigModule == GL.BatchModule.AP)
			{
				VerifyARAPFields(addFilter, parameters);
				VerifyAPFields(graph, addFilter, parameters);
				return CreateAPTransaction(graph, parameters, defcurrencyinfo);
			}
			else if (parameters.OrigModule == GL.BatchModule.AR)
			{
				VerifyARAPFields(addFilter, parameters);
				VerifyARFields(graph, addFilter, parameters);
				return CreateARTransaction(graph, parameters, defcurrencyinfo);
			}
			else if (parameters.OrigModule == GL.BatchModule.CA)
			{
				VerifyCAFields(graph, addFilter, parameters);
				return CreateCATransaction(graph, parameters, currencyinfo);
			}
			else
			{
				throw new PXException(Messages.UnknownModule);
			}
		}
		private static CATran CreateAPTransaction(PXGraph graph, AddTrxFilter parameters, CurrencyInfo defcurrencyinfo)
		{
			APPaymentEntry te = PXGraph.CreateInstance<APPaymentEntry>();
			te.Document.View.Answer = WebDialogResult.No;
			APPayment doc = new APPayment();
			if (parameters.DrCr == CADrCr.CACredit)
			{
				doc.DocType = APDocType.Prepayment;
			}
			else
			{
				doc.DocType = APDocType.Refund;
			}
			doc = PXCache<APPayment>.CreateCopy(te.Document.Insert(doc));

			doc.VendorID = parameters.ReferenceID;
			doc.VendorLocationID = parameters.LocationID;
			doc.CashAccountID = parameters.CashAccountID;
			doc.PaymentMethodID = parameters.PaymentMethodID;
			doc.CuryID = parameters.CuryID;
			doc.CuryOrigDocAmt = parameters.CuryTranAmt;
			doc.DocDesc = parameters.Descr;
			doc.Cleared = parameters.Cleared;
			doc.AdjDate = parameters.TranDate;
			doc.FinPeriodID = parameters.FinPeriodID;
			doc.AdjFinPeriodID = parameters.FinPeriodID;
			doc.ExtRefNbr = parameters.ExtRefNbr;
			doc.Hold = true;
			doc = PXCache<APPayment>.CreateCopy(te.Document.Update(doc));

			foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>>.Select(te))
			{
				CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(defcurrencyinfo);
				new_info.CuryInfoID = info.CuryInfoID;
				te.currencyinfo.Cache.Update(new_info);
			}

			te.Save.Press();
			return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Current<APPayment.cATranID>>>>.Select(te);
		}
		private static CATran CreateARTransaction(PXGraph graph, AddTrxFilter parameters, CurrencyInfo defcurrencyinfo)
		{
			ARPaymentEntry te = PXGraph.CreateInstance<ARPaymentEntry>();
			ARPayment doc = new ARPayment();

			if (parameters.DrCr == CADrCr.CACredit)
			{
				doc.DocType = ARDocType.Refund;
			}
			else
			{
				doc.DocType = ARDocType.Payment;
			}

			doc = PXCache<ARPayment>.CreateCopy(te.Document.Insert(doc));

			doc.CustomerID = parameters.ReferenceID;
			doc.CustomerLocationID = parameters.LocationID;
			doc.PaymentMethodID = parameters.PaymentMethodID;
			doc.PMInstanceID = parameters.PMInstanceID;
			doc.CashAccountID = parameters.CashAccountID;
			doc.CuryOrigDocAmt = parameters.CuryTranAmt;
			doc.DocDesc = parameters.Descr;
			doc.Cleared = parameters.Cleared;
			doc.AdjDate = parameters.TranDate;
			doc.AdjFinPeriodID = parameters.FinPeriodID;
			doc.FinPeriodID = parameters.FinPeriodID;
			doc.ExtRefNbr = parameters.ExtRefNbr;
			doc.Hold = true;
			doc = PXCache<ARPayment>.CreateCopy(te.Document.Update(doc));

			foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARPayment.curyInfoID>>>>.Select(te))
			{
				CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(defcurrencyinfo);
				new_info.CuryInfoID = info.CuryInfoID;
				te.currencyinfo.Cache.Update(new_info);
			}

			te.Save.Press();

			return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Current<ARPayment.cATranID>>>>.Select(te);
		}
		private static CATran CreateCATransaction(PXGraph graph, AddTrxFilter parameters, PXSelectBase<CurrencyInfo> currencyinfo)
		{
			CATranEntry te = PXGraph.CreateInstance<CATranEntry>();
			CurrencyInfo info = null;
			CurrencyInfo filterInfo = currencyinfo.Search<CurrencyInfo.curyInfoID>(parameters.CuryInfoID);
			if (filterInfo != null)
			{
				info = te.currencyinfo.Insert(filterInfo);
			}

			CAAdj adj = new CAAdj();
			bool isTransferExpense = graph is CashTransferEntry;
			adj.AdjTranType = (isTransferExpense ? CATranType.CATransferExp : CATranType.CAAdjustment);
			if (isTransferExpense)
			{
				adj.TransferNbr = (graph as CashTransferEntry).Transfer.Current.TransferNbr;
			}
			adj.CashAccountID = parameters.CashAccountID;
			adj.Released = false;
			adj.CuryID = parameters.CuryID;
			adj.CuryInfoID = info?.CuryInfoID;
			adj.DrCr = parameters.DrCr;
			adj.TranDate = parameters.TranDate;
			adj.EntryTypeID = parameters.EntryTypeID;
			adj.FinPeriodID = parameters.FinPeriodID;
			adj = te.CAAdjRecords.Insert(adj);

			adj.ExtRefNbr = parameters.ExtRefNbr;
			adj.Cleared = parameters.Cleared;
			adj.TranDesc = parameters.Descr;
			adj.CuryControlAmt = parameters.CuryTranAmt;
			adj.CuryTranAmt = parameters.CuryTranAmt;
			adj.Hold = parameters.Hold;
			adj.TaxZoneID = parameters.TaxZoneID;
			adj.TaxCalcMode = TaxCalculationMode.TaxSetting;
			adj = te.CAAdjRecords.Update(adj);

			//handling on-the-fly creation of Sub accounts
			foreach (var SubAccount in graph.Caches[typeof(GL.Sub)].Inserted)
			{
				if (((GL.Sub)SubAccount).SubID == parameters.SubID)
				{
					te.Caches[typeof(GL.Sub)].Insert(SubAccount);
				}					
			}

			CASplit split = new CASplit();
			split.AdjTranType = adj.AdjTranType;
			split.AccountID = parameters.AccountID;
			split.CuryInfoID = info?.CuryInfoID;
			split.Qty = (decimal)1.0;
			split.CuryUnitPrice = parameters.CuryTranAmt;
			split.CuryTranAmt = parameters.CuryTranAmt;
			split.SubID = parameters.SubID;
			split=te.CASplitRecords.Insert(split);
			split.TranDesc = parameters.Descr;
			te.CASplitRecords.Update(split);

			adj.CuryTaxAmt = adj.CuryTaxTotal;
			adj = te.CAAdjRecords.Update(adj);
			te.Save.Press();
			adj = (CAAdj)te.Caches[typeof(CAAdj)].Current;
			return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Required<CAAdj.tranID>>>>.Select(te, adj.TranID);
		}
		private static void VerifyCAFields(PXGraph graph, PXFilter<AddTrxFilter> AddFilter, AddTrxFilter parameters)
		{
			if (string.IsNullOrEmpty(parameters.ExtRefNbr) && ((CASetup)PXSelect<CASetup>.Select(graph)).RequireExtRefNbr == true)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.extRefNbr));
			}
			else
			{
				ThrowIfUIException<AddTrxFilter.extRefNbr>(AddFilter.Cache, parameters);
			}

			if (parameters.AccountID == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.accountID));
			}
			else
			{
				ThrowIfUIException<AddTrxFilter.accountID>(AddFilter.Cache, parameters);
			}

			if (parameters.SubID == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.subID));
			}
			else
			{
				ThrowIfUIException<AddTrxFilter.subID>(AddFilter.Cache, parameters);
			}
		}

		private static FinPeriod GetFinPeriod(PXGraph graph, AddTrxFilter parameters)
		{
			return graph.GetService<IFinPeriodRepository>().GetByID(parameters.FinPeriodID, PXAccess.GetParentOrganizationID(parameters.BranchID));
		}

		private static void VerifyARAPFields(PXFilter<AddTrxFilter> AddFilter, AddTrxFilter parameters)
		{
			if (parameters.ReferenceID == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.referenceID));
			}

			if (parameters.LocationID == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.locationID));
			}

			if (string.IsNullOrEmpty(parameters.PaymentMethodID))
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.paymentMethodID));
			}
			if (parameters.CuryTranAmt==null || parameters.CuryTranAmt==Decimal.Zero)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.curyTranAmt));
			}
		}
		private static void VerifyAPFields(PXGraph graph, PXFilter<AddTrxFilter> AddFilter, AddTrxFilter parameters)
		{
			APSetup setup = PXSelect<APSetup>.Select(graph);
			if (setup.RequireVendorRef == true && string.IsNullOrEmpty(parameters.ExtRefNbr))
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.extRefNbr));
			}
			Vendor vend = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(graph, parameters.ReferenceID);
			if (vend == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.referenceID));
			}
		}
		private static void VerifyARFields(PXGraph graph, PXFilter<AddTrxFilter> AddFilter, AddTrxFilter parameters)
		{
			ARSetup setup = PXSelect<ARSetup>.Select(graph);
			if (setup.RequireExtRef == true && string.IsNullOrEmpty(parameters.ExtRefNbr))
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.extRefNbr));
			}
			if (parameters.PMInstanceID == null)
			{
				bool isPMInstanceRequired = false;
				if (String.IsNullOrEmpty(parameters.PaymentMethodID) == false)
				{
					PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(graph, parameters.PaymentMethodID);
					isPMInstanceRequired = (pm.IsAccountNumberRequired == true);
				}
				if (isPMInstanceRequired)
				{
					ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.pMInstanceID));
				}
			}
			Customer cust = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(graph, parameters.ReferenceID);

			if (cust == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.referenceID));
			}

		}
		private static void VerifyCommonFields(PXFilter<AddTrxFilter> AddFilter, AddTrxFilter parameters)
		{
			if (parameters.CashAccountID == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.cashAccountID));
			}
			if (string.IsNullOrEmpty(parameters.EntryTypeID))
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.entryTypeID));
			}
			if (parameters.TranDate == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.tranDate));
			}
			if (parameters.FinPeriodID == null)
			{
				ThrowFieldIsEmpty(AddFilter.Cache, parameters, typeof(AddTrxFilter.finPeriodID));
			}

			CAAPAROpenPeriodAttribute.VerifyPeriod<AddTrxFilter.finPeriodID>(AddFilter.Cache, parameters);
		}
		private static void ThrowFieldIsEmpty(PXCache cache, IBqlTable row, Type fieldType)
		{
			string fieldName = fieldType.Name;
			string displayName = PXUIFieldAttribute.GetDisplayName(cache, fieldName);
			if (String.IsNullOrEmpty(displayName))
				displayName = fieldName;
			cache.RaiseExceptionHandling(fieldName, row,
				null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, displayName));
			throw new PXRowPersistingException(fieldName, null, ErrorMessages.FieldIsEmpty, displayName);
		}

		private static void ThrowIfUIException<Field>(PXCache cache, IBqlTable row) where Field : IBqlField
		{
			string error = PXUIFieldAttribute.GetErrorOnly<Field>(cache, row);
			if (!string.IsNullOrWhiteSpace(error))
			{
				Type fieldType = typeof(Field);
				string fieldName = fieldType.Name;
				string displayName = PXUIFieldAttribute.GetDisplayName(cache, fieldName);
				if (String.IsNullOrEmpty(displayName))
					displayName = fieldName;
				
				throw new PXRowPersistingException(fieldName, null, error, displayName);
			}
		}
		#endregion
		#region Obsolete
		public static void Clear(PXGraph graph, PXFilter<AddTrxFilter> AddFilter)
		{
			AddTrxFilter parameters = AddFilter.Current;

			parameters.AccountID = null;
			parameters.SubID = null;
			parameters.CuryTranAmt = (decimal)0.0;
			parameters.Descr = null;
			parameters.EntryTypeID = null;
			parameters.ExtRefNbr = null;
			parameters.OrigModule = null;
			parameters.ReferenceID = null;
			parameters.LocationID = null;
			parameters.Cleared = null;
			parameters.Hold = null;
			parameters.PaymentMethodID = null;
			parameters.PMInstanceID = null;
			parameters.PaymentMethodID = null;
			AddFilter.Cache.SetDefaultExt<AddTrxFilter.tranDate>(parameters);
			AddFilter.Cache.SetDefaultExt<AddTrxFilter.hold>(parameters);
		}
		#endregion
		#region Descriptor
		public class AddFilterAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
		{
			public AddFilterAttribute()
				: base()
			{
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.entryTypeID>(AddTrxFilter_EntryTypeId_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.drCr>(AddTrxFilter_DrCr_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.referenceID>(AddTrxFilter_ReferenceID_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.tranDate>(AddTrxFilter_TranDate_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.cashAccountID>(AddTrxFilter_CashAccountID_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.accountID>(AddTrxFilter_AccountID_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.paymentMethodID>(AddTrxFilter_PaymentMethodID_FieldUpdated);
				sender.Graph.RowUpdated.AddHandler<AddTrxFilter>(AddTrxFilter_RowUpdated);
				sender.Graph.RowSelected.AddHandler<AddTrxFilter>(AddTrxFilter_RowSelected);
			}

			public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				if (row != null)
				{
					CASetup casetup = (CASetup)PXSelect<CASetup>.Select(sender.Graph);

					PXUIFieldAttribute.SetEnabled<AddTrxFilter.curyID>(sender, row, false);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.accountID>(sender, row, (row.OrigModule == GL.BatchModule.CA));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.subID>(sender, row, (row.OrigModule == GL.BatchModule.CA));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.referenceID>(sender, row, (row.OrigModule == GL.BatchModule.AP || row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.locationID>(sender, row, (row.OrigModule == GL.BatchModule.AP || row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.paymentMethodID>(sender, row, (row.OrigModule == GL.BatchModule.AP || row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.pMInstanceID>(sender, row, (row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.extRefNbr>(sender, row, true);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.drCr>(sender, row, false);
					PXUIFieldAttribute.SetEnabled<AddTrxFilter.cashAccountID>(sender, row, false);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.origModule>(sender, row, false);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.hold>(sender, row, (row.OrigModule == GL.BatchModule.CA) && (casetup.HoldEntry == true));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.status>(sender, row, (row.OrigModule == GL.BatchModule.CA) && (casetup.HoldEntry == true));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.taxZoneID>(sender, row, (row.OrigModule == GL.BatchModule.CA && row.DrCr == GL.DrCr.Credit));

					CashAccount acct = (CashAccount)PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(sender.Graph, row.CashAccountID);
					bool needReconcilation = (acct != null) && (acct.Reconcile == true);

					PXUIFieldAttribute.SetEnabled<AddTrxFilter.cleared>(sender, row, needReconcilation);
					PXUIFieldAttribute.SetEnabled<AddTrxFilter.clearDate>(sender, row, needReconcilation && (row.Cleared == true));

					if (row.OrigModule == GL.BatchModule.AP)
					{
						PaymentMethodAccount cDet = PXSelect<PaymentMethodAccount,
													Where<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>,
													And<PaymentMethodAccount.paymentMethodID, Equal<Current<AddTrxFilter.paymentMethodID>>,
													And<PaymentMethodAccount.useForAP, Equal<True>>>>>.Select(sender.Graph);
						if ((cDet != null) && (cDet.APAutoNextNbr == true))
						{
							PXUIFieldAttribute.SetEnabled<AddTrxFilter.extRefNbr>(sender, row, false);
						}
						else
						{
							PXUIFieldAttribute.SetEnabled<AddTrxFilter.extRefNbr>(sender, row, true);
						}
					}
					if (row.OrigModule == GL.BatchModule.AR)
					{
						bool isInstanceRequired = false;
						if (String.IsNullOrEmpty(row.PaymentMethodID) == false)
						{
							PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(sender.Graph, row.PaymentMethodID);
							isInstanceRequired = (pm != null && pm.IsAccountNumberRequired == true);
						}
						PXUIFieldAttribute.SetEnabled<AddTrxFilter.pMInstanceID>(sender, row, isInstanceRequired);
					}
					if (row.OrigModule == GL.BatchModule.CA)
					{
						bool singleCashAcct = false;
						if (String.IsNullOrEmpty(row.EntryTypeID) == false)
						{
							if (row.AccountID.HasValue)
							{
								var cashAccts = PXSelectReadonly<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>>>.Select(sender.Graph, row.AccountID);
								singleCashAcct = (cashAccts.Count == 1);
							}
							else
							{
								CAEntryType entryType = PXSelectReadonly<CAEntryType, Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.Select(sender.Graph, row.EntryTypeID);
								if (entryType != null && entryType.UseToReclassifyPayments == true)
								{
									CashAccount availableAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, NotEqual<Required<CashAccount.cashAccountID>>,
																		And<CashAccount.curyID, Equal<Required<CashAccount.curyID>>>>>.Select(sender.Graph, row.CashAccountID, row.CuryID);
									if (availableAccount == null)
									{
										sender.RaiseExceptionHandling<AddTrxFilter.cashAccountID>(row, null, new PXSetPropertyKeepPreviousException(Messages.EntryTypeRequiresCashAccountButNoOneIsConfigured, PXErrorLevel.Warning, row.CuryID));
										sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, new PXSetPropertyException(Messages.EntryTypeRequiresCashAccountButNoOneIsConfigured, row.CuryID, PXErrorLevel.Warning));
									}
									else
									{
										sender.RaiseExceptionHandling<AddTrxFilter.cashAccountID>(row, null, null);
										sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, null);
									}
								}
								else
								{
									sender.RaiseExceptionHandling<AddTrxFilter.cashAccountID>(row, null, null);
									sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, null);
								}
							}
						}
						PXUIFieldAttribute.SetEnabled<AddTrxFilter.subID>(sender, row, !singleCashAcct);
					}
				}
			}

			public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				sender.Current = row;
				if (row.DrCr == GL.DrCr.Debit)
				{
					sender.SetValueExt<AddTrxFilter.taxZoneID>(row, null);
				}
			}

			public virtual void AddTrxFilter_EntryTypeId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				sender.SetDefaultExt<AddTrxFilter.origModule>(row);
				sender.SetDefaultExt<AddTrxFilter.drCr>(row);
				sender.RaiseExceptionHandling<AddTrxFilter.accountID>(row, null, null);
				sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, null);

				sender.SetDefaultExt<AddTrxFilter.descr>(row);
				sender.SetDefaultExt<AddTrxFilter.accountID>(row);
				sender.SetDefaultExt<AddTrxFilter.subID>(row);
				sender.SetDefaultExt<AddTrxFilter.taxZoneID>(row);
				sender.SetDefaultExt<AddTrxFilter.referenceID>(row);
				sender.SetValue<AddTrxFilter.locationID>(row, null);
				sender.SetValue<AddTrxFilter.paymentMethodID>(row, null);
				sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);

			}

			public virtual void AddTrxFilter_DrCr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;

				if (row.ExtRefNbr != null)
				{
					row.ExtRefNbr = null;
				}
			}

			public virtual void AddTrxFilter_TranDate_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				if (row == null) return;

				CashAccount acct = (CashAccount)PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(sender.Graph, row.CashAccountID);
				if ((acct != null) && (acct.Reconcile != true))
				{
					row.ClearDate = row.TranDate;
				}
			}

			public virtual void AddTrxFilter_ReferenceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				string error = null;
				switch (row.OrigModule)
				{
					case GL.BatchModule.AP:

						sender.SetDefaultExt<AddTrxFilter.locationID>(row);
						sender.SetDefaultExt<AddTrxFilter.paymentMethodID>(row);

						error = PXUIFieldAttribute.GetError<AddTrxFilter.paymentMethodID>(sender, row);
						if (string.IsNullOrEmpty(error) == false)
						{
							sender.SetValue<AddTrxFilter.paymentMethodID>(row, null);
							PXUIFieldAttribute.SetError<AddTrxFilter.paymentMethodID>(sender, row, null, null);
						}

						sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);
						break;

					case GL.BatchModule.AR:

						sender.SetDefaultExt<AddTrxFilter.locationID>(row);

						sender.SetDefaultExt<AddTrxFilter.paymentMethodID>(row);
						error = PXUIFieldAttribute.GetError<AddTrxFilter.paymentMethodID>(sender, row);
						if (string.IsNullOrEmpty(error) == false)
						{
							sender.SetValue<AddTrxFilter.paymentMethodID>(row, null);
							PXUIFieldAttribute.SetError<AddTrxFilter.paymentMethodID>(sender, row, null, null);
						}
						sender.SetDefaultExt<AddTrxFilter.pMInstanceID>(row);

						error = PXUIFieldAttribute.GetError<AddTrxFilter.pMInstanceID>(sender, row);
						if (string.IsNullOrEmpty(error) == false)
						{
							sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);
							PXUIFieldAttribute.SetError<AddTrxFilter.pMInstanceID>(sender, row, null, null);
						}
						break;
					default:
						sender.SetValue<AddTrxFilter.locationID>(row, null);
						sender.SetValue<AddTrxFilter.paymentMethodID>(row, null);
						sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);
						break;
				}
			}

			public virtual void AddTrxFilter_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				string error = null;
				if (row.OrigModule == GL.BatchModule.AR)
				{
					sender.SetDefaultExt<AddTrxFilter.pMInstanceID>(row);
					error = PXUIFieldAttribute.GetError<AddTrxFilter.pMInstanceID>(sender, row);
					if (string.IsNullOrEmpty(error) == false)
					{
						sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);
						PXUIFieldAttribute.SetError<AddTrxFilter.pMInstanceID>(sender, row, null, null);
					}
				}
			}
			public virtual void AddTrxFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				sender.SetDefaultExt<AddTrxFilter.curyID>(e.Row);
			}
			public virtual void AddTrxFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				if (row.OrigModule != null)
				{
					if (row.OrigModule == BatchModule.CA)
					{
						CASetup setup = (CASetup)PXSelect<CASetup>.Select(sender.Graph);
						PXUIFieldAttribute.SetRequired<AddTrxFilter.extRefNbr>(sender, setup.RequireExtRefNbr ?? false);
					}
					else if (row.OrigModule == BatchModule.AP)
					{
						APSetup setup = (APSetup)PXSelect<APSetup>.Select(sender.Graph);
						PXUIFieldAttribute.SetRequired<AddTrxFilter.extRefNbr>(sender, setup.RequireVendorRef ?? false);
					}
					else if (row.OrigModule == BatchModule.AR)
					{
						ARSetup setup = (ARSetup)PXSelect<ARSetup>.Select(sender.Graph);
						PXUIFieldAttribute.SetRequired<AddTrxFilter.extRefNbr>(sender, setup.RequireExtRef ?? false);
					}
				}
			}
			public virtual void AddTrxFilter_TranDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				PXCache currencycache = sender.Graph.Caches[typeof(CurrencyInfo)];

				CurrencyInfoAttribute.SetEffectiveDate<AddTrxFilter.tranDate>(sender, e);
			}

			public virtual void AddTrxFilter_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				CashAccount cashacct = (CashAccount)PXSelectorAttribute.Select<AddTrxFilter.cashAccountID>(sender, row);
				sender.SetDefaultExt<AddTrxFilter.curyID>(row);
				PXView view = sender.Graph.Views["_" + typeof(AddTrxFilter).Name + "_CurrencyInfo_"];
				CurrencyInfo info = view.SelectSingle(row.CuryInfoID) as CurrencyInfo;

				if (cashacct.CuryRateTypeID != null)
				{
					view.Cache.SetValueExt<CurrencyInfo.curyRateTypeID>(info, cashacct.CuryRateTypeID);
				}
				else
				{
					view.Cache.SetDefaultExt<CurrencyInfo.curyRateTypeID>(info);
				}

				sender.SetDefaultExt<AddTrxFilter.branchID>(row);
				sender.SetDefaultExt<AddTrxFilter.entryTypeID>(row);
				if (cashacct != null)
				{
					row.Cleared = false;
					row.ClearDate = null;

					if (cashacct.Reconcile != true)
					{
						row.Cleared = true;
						row.ClearDate = row.TranDate;
					}
				}
			}
			public virtual void AddTrxFilter_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				if (row.AccountID.HasValue && row.AccountID != (int?)e.OldValue)
				{
					CashAccount offsetCashAcct = CATranDetailHelper.GetCashAccount(sender.Graph, row.AccountID, null, row.BranchID, true);
					if (offsetCashAcct!=null && offsetCashAcct.SubID!=null)
						sender.SetValue<AddTrxFilter.subID>(row, offsetCashAcct.SubID);
				}
			}
		}
		#endregion
	}
}
