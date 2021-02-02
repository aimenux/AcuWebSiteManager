using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA.BankStatementHelpers;
using PX.Objects.CA.BankStatementProtoHelpers;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.AP.MigrationMode;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.EP;
using static PX.Objects.Common.UIState;

namespace PX.Objects.CA
{
    public partial class CABankTransactionsMaint : PXGraph<CABankTransactionsMaint>
	{
		#region Constructor
		public CABankTransactionsMaint()
		{
			Details.Cache.AllowInsert = false;
			Details.Cache.AllowDelete = false;
			Details.Cache.AllowUpdate = false;

			DetailMatchesCA.Cache.AllowInsert = false;
			DetailMatchesCA.Cache.AllowDelete = false;

			Adjustments.Cache.AllowInsert = false;
			Adjustments.Cache.AllowDelete = false;

			DetailMatchingInvoices.Cache.AllowInsert = false;
			DetailMatchingInvoices.Cache.AllowDelete = false;

			Details.AllowUpdate = false;
			TranSplit.AllowInsert = false;

			APSetupNoMigrationMode.EnsureMigrationModeDisabled(this);
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			matchSettingsPanel.StateSelectingEvents += StateSelectingEventsHandler;
			processMatched.StateSelectingEvents += StateSelectingEventsHandler;
			uploadFile.StateSelectingEvents += StateSelectingEventsHandler;
			autoMatch.StateSelectingEvents += StateSelectingEventsHandler;

			FieldDefaulting.AddHandler<CR.BAccountR.type>(SetDefaultBaccountType);

			PXUIFieldAttribute.SetVisible<CABankTranDetail.projectID>(TranSplit.Cache, null, false);
			EnableCreateTab(Details.Cache, null, false);
				}

		private void StateSelectingEventsHandler(PXCache sender, PXFieldSelectingEventArgs e)
			{
			TimeSpan timespan;
			Exception message;
			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out message);

			if (status == PXLongRunStatus.NotExists)
				return;

			PXButtonState state = PXButtonState.CreateInstance(e.ReturnState, null, null, null, null, null, false,
															   PXConfirmationType.Unspecified, null, null, null, null, null,
															   null, null, null, null, null, null, typeof(Filter));
			state.Enabled = false;
			e.ReturnState = state;
				}

		/// <summary>
		/// Sets default baccount type. Method is used as a workaround for the redirection problem with the edit button of the empty Business Account field.
		/// </summary>
		private void SetDefaultBaccountType(PXCache sender, PXFieldDefaultingEventArgs e)
				{
			CABankTran bankTransaction = DetailsForPaymentCreation.Current;

			if (e.Row == null || bankTransaction == null)
				return;

			if (bankTransaction.OrigModule == BatchModule.AP)
				e.NewValue = CR.BAccountType.VendorType;
			else if (bankTransaction.OrigModule == BatchModule.AR)
				e.NewValue = CR.BAccountType.CustomerType;
		}
        #endregion

        #region Internal Classes definitions

        [Serializable]
		public partial class Filter : IBqlTable
		{
			#region CashAccountID
			public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
			protected Int32? _CashAccountID;
			[CashAccount(null, typeof(Search<
			    CashAccount.cashAccountID, 
			    Where<Match<Current<AccessInfo.userName>>>>), IsKey = true)] // IsKey required to action 'Cancel' did not clear the filter fields.   
			public virtual int? CashAccountID
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
			#region TranType
			public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			protected String _TranType;
			[PXDBString(1, IsFixed = true)]
			[PXDefault(typeof(CABankTranType.statement))]
			[CABankTranType.List()]
			public virtual String TranType
			{
				get
				{
					return this._TranType;
				}
				set
				{
					this._TranType = value;
				}
			}
            #endregion
            #region IsCorpCardCashAccount
		    public abstract class isCorpCardCashAccount : PX.Data.BQL.BqlBool.Field<isCorpCardCashAccount> { }

            [PXUIField(DisplayName = "IsCorpCardCashAccount", Visible = false)]
            [PXBool]
            public bool? IsCorpCardCashAccount { get; set; }
		    #endregion
		}

		#endregion
		#region Public Memebrs
		public PXFilter<Filter> TranFilter;
		public PXSelect<AR.Standalone.ARRegister> dummyARRegister;
		public PXSelect<AP.Standalone.APRegister> dummyAPRegister;
		[PXFilterable]
		public PXSelect<CABankTran> Details;
		public PXSelect<
		    CABankTran, 
		    Where<CABankTran.processed, Equal<False>,
		        And<CABankTran.cashAccountID, Equal<Current<Filter.cashAccountID>>,
		        And<CABankTran.tranType, Equal<Current<Filter.tranType>>,
		        And<CABankTran.documentMatched, Equal<False>>>>>> 
		    UnMatchedDetails;
		public PXSelect<
		    CABankTran, 
		    Where<CABankTran.tranID, Equal<Current<CABankTran.tranID>>>> 
		    DetailsForPaymentCreation;
		public PXSelect<
		    CABankTran, 
		    Where<CABankTran.tranID, Equal<Current<CABankTran.tranID>>>> 
		    DetailsForInvoiceApplication;

		public PXSetup<CASetup> CASetup;
		public CMSetupSelect CMSetup;
		public PXSetup<APSetup> APSetup;
		public PXSetup<ARSetup> arsetup;

		//these view are here for correct StatementsMatchingProto.UpdateSourceDoc work
		public PXSelect<CATran, Where<CATran.tranID, IsNull>> caTran;
		public PXSelect<APPayment, Where<APPayment.docType, IsNull>> apPayment;
		public PXSelect<ARPayment, Where<ARPayment.docType, IsNull>> arPayment;
		public PXSelect<CADeposit, Where<CADeposit.tranType, IsNull>> caDeposit;
		public PXSelect<CAAdj, Where<CAAdj.adjRefNbr, IsNull>> caAdjustment;
		public PXSelect<CATransfer, Where<CATransfer.transferNbr, IsNull>> caTransfer;

		public PXSelect<CATranExt, Where<CATranExt.matchRelevance, IsNotNull>, OrderBy<Desc<CATranExt.matchRelevance>>> DetailMatchesCA;
		public PXSelect<CABankTranMatch, Where<CABankTranMatch.tranID, Equal<Required<CABankTran.tranID>>>> TranMatch;
		public PXSelect<
		    CABankTranMatch, 
		    Where<CABankTranMatch.tranID, Equal<Required<CABankTran.tranID>>,
		        And<CABankTranMatch.tranType, Equal<Required<CABankTranMatch.tranType>>,
		        And<CABankTranMatch.docModule, Equal<Required<CABankTranMatch.docModule>>,
		        And<CABankTranMatch.docType, Equal<Required<CABankTranMatch.docType>>,
		        And<CABankTranMatch.docRefNbr, Equal<Required<CABankTranMatch.docRefNbr>>>>>>>> 
		    TranMatchInvoices;
		public PXSelectJoin<
		    CABankTranAdjustment,
		    LeftJoin<ARInvoice,
		        On<CABankTranAdjustment.adjdModule, Equal<BatchModule.moduleAR>,
		        And<CABankTranAdjustment.adjdRefNbr, Equal<ARInvoice.refNbr>,
		        And<CABankTranAdjustment.adjdDocType, Equal<ARInvoice.docType>>>>>,
		    Where<CABankTranAdjustment.tranID, Equal<Optional<CABankTran.tranID>>>> 
		    Adjustments;
		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<Filter.cashAccountID>>>> cashAccount;

		[PXCopyPasteHiddenView]
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<CABankTran.curyInfoID>>>> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<CABankTranAdjustment.adjgCuryInfoID>>>> currencyinfo_adjustment;
		public PXSelectReadonly<CashAccount, Where<CashAccount.extRefNbr, Equal<Optional<CashAccount.extRefNbr>>>> cashAccountByExtRef;

		public PXSelect<CABankTranInvoiceMatch, Where<True, Equal<False>>, OrderBy<Desc<CABankTranInvoiceMatch.matchRelevance>>> DetailMatchingInvoices;

		public PXSelect<CABankTranExpenseDetailMatch, 
					Where<True, Equal<True>>,
					OrderBy<Desc<CABankTranExpenseDetailMatch.matchRelevance,
							Asc<CABankTranExpenseDetailMatch.curyDocAmtDiff>>>> 
	        ExpenseReceiptDetailMatches;

        [PXCopyPasteHiddenView]
		public PXSelect<CAExpense> cAExpense;

		[PXHidden]
		public PXSelect<CABankTranRule, Where<CABankTranRule.isActive, Equal<True>>> Rules;

		public PXSelect<CABankTranDetail, 
						Where<CABankTranDetail.bankTranID, Equal<Optional<CABankTran.tranID>>, 
							And<CABankTranDetail.bankTranType, Equal<Optional<CABankTran.tranType>>>>> 
						TranSplit;

		public PXSelect<CR.BAccountR> BaccountCache;

		public PXSelect<EPExpenseClaimDetails> ExpenseReceipts;

		public virtual IMatchSettings CurrentMatchSesstings
		{
			get { return cashAccount.Current ?? cashAccount.Select(); }
		}

		#endregion
        #region Delegates
        protected virtual IEnumerable detailMatchingInvoices()
		{
			CABankTran detail = this.Details.Current;
			if (detail == null) yield break;
			PXCache cache = this.DetailMatchingInvoices.Cache;
			cache.Clear();
			detail.CountInvoiceMatches = 0;
			IEnumerable matches = this.FindDetailMatchingInvoices(detail, CurrentMatchSesstings);
			if (matches.Any_())
			{
				List<CABankTranMatch> existingMatches = new List<CABankTranMatch>();
				foreach (CABankTranMatch match in TranMatch.Select(detail.TranID))
				{
					if (match.DocModule != null && match.DocType != null && match.DocRefNbr != null)
					{
						existingMatches.Add(match);
					}
				}

				foreach (CABankTranInvoiceMatch it in matches)
				{
					CABankTranInvoiceMatch invMatch = cache.Insert(it) as CABankTranInvoiceMatch;
					if (invMatch != null)
					{
						bool matched = false;
						if (existingMatches.Any(existingMatch => existingMatch.DocModule == invMatch.OrigModule
													&& existingMatch.DocType == invMatch.OrigTranType
													&& existingMatch.DocRefNbr == invMatch.OrigRefNbr))
						{
							matched = true;
						}
						cache.SetValue<CABankTranInvoiceMatch.isMatched>(invMatch, matched);
						yield return invMatch;
					}
				}
			}
			cache.IsDirty = false;
			yield break;
		}

	    protected virtual IEnumerable expenseReceiptDetailMatches()
	    {
	        CABankTran detail = Details.Current;

	        if (detail == null)
	            yield break;

	        PXCache matchesCache = ExpenseReceiptDetailMatches.Cache;

	        matchesCache.Clear();

	        detail.CountExpenseReceiptDetailMatches = 0;

	        IList<CABankTranExpenseDetailMatch> matches = this.FindExpenseReceiptDetailMatches(detail, CurrentMatchSesstings);

            if (matches.Any())
            {
                CABankTranMatch existingMatch = null;
                foreach (CABankTranMatch match in TranMatch.Select(detail.TranID))
                {
                    if (match.DocModule != null && match.DocType != null && match.DocRefNbr != null)
                    {
                        existingMatch = match;
                        break;
                    }
                }

                foreach (CABankTranExpenseDetailMatch it in matches)
                {
                    CABankTranExpenseDetailMatch expenseMatch = (CABankTranExpenseDetailMatch)matchesCache.Insert(it);

                    if (expenseMatch != null)
                    {
                        bool matched = false;
                        if (existingMatch != null 
                            && existingMatch.DocModule == BatchModule.EP 
                            && existingMatch.DocType == EPExpenseClaimDetails.DocType 
                            && existingMatch.DocRefNbr == expenseMatch.RefNbr)
                        {
                            matched = true;
                            existingMatch = null;//we've already found a match. There can be only one match in current implementation.
                        }
                        matchesCache.SetValue<CABankTranExpenseDetailMatch.isMatched>(expenseMatch, matched);
                        yield return expenseMatch;
                    }
                }
            }
            matchesCache.IsDirty = false;
	        yield break;
        }

        protected virtual IEnumerable details()
		{
			Filter current = TranFilter.Current;
			if (current == null || current.CashAccountID == null) yield break;

			Func<IEnumerable<CABankTran>> getTransactions = () =>
			{
				return PXSelect<
				    CABankTran,
				    Where<CABankTran.processed, Equal<False>,
				        And<CABankTran.cashAccountID, Equal<Required<CABankTran.cashAccountID>>,
				        And<CABankTran.tranType, Equal<Required<CABankTran.tranType>>>>>>
				    .Select(this, current.CashAccountID, current.TranType)
				    .RowCast<CABankTran>();
			};

			TimeSpan timespan;
			Exception ex;

			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);

			IEnumerable<CABankTran> recordsInProcessing = null;
			if (status != PXLongRunStatus.NotExists)
			{
				object[] processingList;
				var customInfo = PXLongOperation.GetCustomInfo(this.UID, out processingList);

				if (processingList != null)
				{
					recordsInProcessing = processingList.Cast<CABankTran>();
				}
			}

			foreach (CABankTran det in recordsInProcessing ?? getTransactions())
			{
				yield return det;
			}
		}

		protected virtual IEnumerable detailMatchesCA()
		{
			CABankTran detail = this.Details.Current;
			if (detail == null) yield break;
			PXCache cache = this.DetailMatchesCA.Cache;
			var items = cache.Cached.ToArray<CATranExt>();
			detail.CountMatches = 0;
			cache.Clear();
			foreach (CATranExt CATran in this.FindDetailMatches(detail, CurrentMatchSesstings, null))
			{
				CATranExt det = null;
				bool matched = CATran.IsMatched == true;
				CATran.IsMatched = null;
				if (CATran.OrigModule == BatchModule.AP && CATran.OrigTranType == CATranType.CABatch)
				{
					foreach (CATranExt inserted in items)
					{
						if (inserted.OrigModule == BatchModule.AP && inserted.OrigTranType == CATranType.CABatch && inserted.OrigRefNbr == CATran.OrigRefNbr)
						{
							CATran.TranID = inserted.TranID;
							det = DetailMatchesCA.Update(CATran);
							break;
						}
					}
				}
				if (det == null)
					det = DetailMatchesCA.Insert(CATran);
				det.IsMatched = matched;
				yield return det;
			}
			cache.IsDirty = false;
		}
		#endregion
		#region Internal Variables
		protected Dictionary<Object, List<CABankTranInvoiceMatch>> matchingInvoices;
		protected Dictionary<Object, List<CATranExt>> matchingTrans;
	    protected Dictionary<Object, List<CABankTranExpenseDetailMatch>> matchingExpenseReceiptDetails;

        public const decimal RelevanceTreshold = 0.2m;
		public const decimal PaymentMatchTreshold = 0.75m;
		public const decimal InvoiceMatchTreshold = 0.75m;
		protected bool skipLowRelevance = false; //This setting defines if all low relevance transactions are displayed in interface and search in automatch  
		#endregion
		#region Buttons
		public PXSave<Filter> Save;

		public PXCancel<Filter> Cancel;

		public PXAction<Filter> loadInvoices;
		[PXUIField(DisplayName = "Load Documents", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public virtual IEnumerable LoadInvoices(PXAdapter adapter)
		{
			CABankTran currentDoc = Details.Current;
			bool receipt = currentDoc.DrCr == CADrCr.CADebit;
			if (currentDoc.OrigModule == GL.BatchModule.AR)
			{
				ARPayment payment = new ARPayment();
				payment.Released = false;
				payment.OpenDoc = true;
				payment.Hold = false;
				payment.Status = ARDocStatus.Balanced;
				payment.CustomerID = Details.Current.PayeeBAccountID;
				payment.CashAccountID = Details.Current.CashAccountID;
				payment.PaymentMethodID = Details.Current.PaymentMethodID;
				payment.CuryOrigDocAmt = Details.Current.CuryOrigDocAmt;
				payment.DocType = ARDocType.Payment;
				payment.CuryID = Details.Current.CuryID;
				var opts = new ARPaymentEntry.LoadOptions();
				opts.LoadChildDocuments = ARPaymentEntry.LoadOptions.loadChildDocuments.IncludeCRM;
				PXResultset<ARInvoice> custdocs = ARPaymentEntry.GetCustDocs(opts, payment, arsetup.Current, this);
				List<string> existing = new List<string>();
				foreach (PXResult<CABankTranAdjustment> res in Adjustments.Select())
				{
					CABankTranAdjustment bankAdj = (CABankTranAdjustment)res;
					existing.Add(string.Format("{0}_{1}", bankAdj.AdjdDocType, bankAdj.AdjdRefNbr));
				}
				foreach (ARInvoice invoice in custdocs.AsEnumerable().Where(row => ((ARInvoice)row).PaymentsByLinesAllowed != true))
				{
					if (!receipt && (invoice.DocType == ARDocType.Invoice || invoice.DocType == ARDocType.DebitMemo || invoice.DocType == ARDocType.FinCharge))
						continue;
					if (receipt && (invoice.DocType == ARDocType.Prepayment || invoice.DocType == ARDocType.Payment))
						continue;
					string s = string.Format("{0}_{1}", invoice.DocType, invoice.RefNbr);
					if (existing.Contains(s) == false &&
						!IsMatchedOnCreateTab(invoice.RefNbr, invoice.DocType, BatchModule.AR) &&
						!IsInvoiceMatched(invoice.RefNbr, invoice.DocType, BatchModule.AR) &&
						BankStatementProtoHelpers.PXInvoiceSelectorAttribute.GetRecordsAR(invoice.DocType, currentDoc.TranID, null, currentDoc, Adjustments.Cache, this)
							.Where(inv => inv.DocType == invoice.DocType && inv.RefNbr == invoice.RefNbr)
							.Any())
					{
						if (currentDoc.CuryUnappliedBal == 0m && currentDoc.CuryOrigDocAmt > 0m)
						{
							break;
						}
						CABankTranAdjustment bankAdj = new CABankTranAdjustment();

						bankAdj = Adjustments.Insert(bankAdj);
						bankAdj.AdjdDocType = invoice.DocType;
						bankAdj.AdjdRefNbr = invoice.RefNbr;
						Adjustments.Update(bankAdj);
					}
				}
				if (currentDoc.CuryApplAmt < 0m)
				{
					List<CABankTranAdjustment> credits = new List<CABankTranAdjustment>();

					foreach (CABankTranAdjustment adj in Adjustments.Select())
					{
						if (adj.AdjdDocType == ARDocType.CreditMemo)
						{
							credits.Add(adj);
						}
					}

					credits.Sort((a, b) =>
					{
						return ((IComparable)a.CuryAdjgAmt).CompareTo(b.CuryAdjgAmt);
					});

					foreach (CABankTranAdjustment adj in credits)
					{
						if (adj.CuryAdjgAmt <= -currentDoc.CuryApplAmt)
						{
							Adjustments.Delete(adj);
						}
						else
						{
							CABankTranAdjustment copy = PXCache<CABankTranAdjustment>.CreateCopy(adj);
							copy.CuryAdjgAmt += currentDoc.CuryApplAmt;
							Adjustments.Update(copy);
						}
					}
				}

			}
			else if (currentDoc.OrigModule == GL.BatchModule.AP)
			{
				APPayment payment = new APPayment();
				payment.Released = false;
				payment.OpenDoc = true;
				payment.Hold = false;
				payment.Status = APDocStatus.Balanced;
				payment.VendorID = currentDoc.PayeeBAccountID;
				payment.CashAccountID = currentDoc.CashAccountID;
				payment.PaymentMethodID = currentDoc.PaymentMethodID;
				payment.CuryOrigDocAmt = currentDoc.CuryOrigDocAmt;
				payment.DocType = APDocType.Check;
				payment.CuryID = currentDoc.CuryID;
				PXResultset<APInvoice> venddocs = APPaymentEntry.GetVendDocs(payment, this, APSetup.Current);
				List<string> existing = new List<string>();
				foreach (PXResult<CABankTranAdjustment> res in Adjustments.Select())
				{
					CABankTranAdjustment bankAdj = (CABankTranAdjustment)res;
					existing.Add(string.Format("{0}_{1}", bankAdj.AdjdDocType, bankAdj.AdjdRefNbr));
				}
				foreach (APInvoice invoice in venddocs.AsEnumerable().Where(row => ((APInvoice)row).PaymentsByLinesAllowed != true))
				{
					if (receipt && (invoice.DocType == APDocType.CreditAdj || invoice.DocType == ARDocType.Invoice))
						continue;
					string s = string.Format("{0}_{1}", invoice.DocType, invoice.RefNbr);
					if (existing.Contains(s) == false &&
						!IsInvoiceMatched(invoice.RefNbr, invoice.DocType, BatchModule.AP) &&
						!IsMatchedOnCreateTab(invoice.RefNbr, invoice.DocType, BatchModule.AP) &&
						BankStatementProtoHelpers.PXInvoiceSelectorAttribute.GetRecordsAP(invoice.DocType, currentDoc.TranID, null, currentDoc, Adjustments.Cache, this)
							.Where(inv => inv.DocType == invoice.DocType && inv.RefNbr == invoice.RefNbr)
							.Any())
					{
						if (currentDoc.CuryUnappliedBal == 0m && currentDoc.CuryOrigDocAmt > 0m)
						{
							break;
						}
						CABankTranAdjustment bankAdj = new CABankTranAdjustment();

						bankAdj = Adjustments.Insert(bankAdj);
						bankAdj.AdjdDocType = invoice.DocType;
						bankAdj.AdjdRefNbr = invoice.RefNbr;
						Adjustments.Update(bankAdj);
					}
				}
				//removung debit adjustments that sets balance below 0
				if (currentDoc.CuryApplAmt < 0m)
				{
					List<CABankTranAdjustment> debits = new List<CABankTranAdjustment>();

					foreach (CABankTranAdjustment adj in Adjustments.Select())
					{
						if (adj.AdjdDocType == APDocType.DebitAdj)
						{
							debits.Add(adj);
						}
					}

					debits.Sort((a, b) =>
					{
						return ((IComparable)a.CuryAdjgAmt).CompareTo(b.CuryAdjgAmt);
					});

					foreach (CABankTranAdjustment adj in debits)
					{
						if (adj.CuryAdjgAmt <= -currentDoc.CuryApplAmt)
						{
							Adjustments.Delete(adj);
						}
						else
						{
							CABankTranAdjustment copy = PXCache<CABankTranAdjustment>.CreateCopy(adj);
							copy.CuryAdjgAmt += currentDoc.CuryApplAmt;
							Adjustments.Update(copy);
						}
					}
				}
			}

			return adapter.Get();
		}

		#region AutoMatch
		public PXAction<Filter> autoMatch;
		[PXUIField(DisplayName = Messages.AutoMatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable AutoMatch(PXAdapter adapter)
		{
			Save.Press();
			DoMatch();
			return adapter.Get();
		}
		#endregion

		public PXAction<Filter> processMatched;
		[PXUIField(DisplayName = AR.Messages.Process, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable ProcessMatched(PXAdapter adapter)
		{
			Save.Press();
			PXResultset<CABankTran> list = Details.Select();
			var toProcess = list.RowCast<CABankTran>().Where(t => t.DocumentMatched == true).ToList();
			if (toProcess.Count < 1)
				return adapter.Get();
			PXLongOperation.ClearStatus(this.UID);
			PXLongOperation.StartOperation(this, delegate () { DoProcessing(toProcess); });
			return adapter.Get();
		}

		public PXAction<Filter> matchSettingsPanel;
		[PXUIField(DisplayName = Messages.MatchSettings, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable MatchSettingsPanel(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXSelect<Filter> NewRevisionPanel;
		public PXAction<Filter> uploadFile;
		[PXUIField(DisplayName = Messages.UploadFile, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable UploadFile(PXAdapter adapter)
		{
			this.Save.Press();
			bool doImport = true;
			Filter row = TranFilter.Current;
			if (CASetup.Current.ImportToSingleAccount == true)
			{
				if (row == null || row.CashAccountID == null)
				{
					throw new PXException(Messages.CashAccountMustBeSelectedToImportStatement);
				}
				else
				{
					CashAccount acct = PXSelect<
					    CashAccount, 
					    Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
					    .Select(this, row.CashAccountID);
					if (acct != null && string.IsNullOrEmpty(acct.StatementImportTypeName))
					{
						throw new PXException(Messages.StatementImportServiceMustBeConfiguredForTheCashAccount);
					}
				}
			}
			else
			{
				if (string.IsNullOrEmpty(CASetup.Current.StatementImportTypeName))
				{
					throw new PXException(Messages.StatementImportServiceMustBeConfiguredInTheCASetup);
				}
			}

			if (Details.Current != null && this.IsDirty == true)
			{
				if (CASetup.Current.ImportToSingleAccount != true)
				{
					if (Details.Ask(Messages.ImportConfirmationTitle, Messages.UnsavedDataInThisScreenWillBeLostConfirmation, MessageButtons.YesNo) != WebDialogResult.Yes)
					{
						doImport = false;
					}
				}
				else
				{
					doImport = true;
				}
			}

			if (doImport)
			{
				if (this.NewRevisionPanel.AskExt() == WebDialogResult.OK)
				{
					Filter currFilter = (Filter)TranFilter.Cache.CreateCopy(TranFilter.Current);
					const string PanelSessionKey = "ImportStatementProtoFile";
					PX.SM.FileInfo info = PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[PanelSessionKey] as PX.SM.FileInfo;
					System.Web.HttpContext.Current.Session.Remove(PanelSessionKey);
					CABankTransactionsImport importGraph = PXGraph.CreateInstance<CABankTransactionsImport>();
					CABankTranHeader newCurrent = new CABankTranHeader() { CashAccountID = row.CashAccountID };
					if (CASetup.Current.ImportToSingleAccount == true)
					{
						newCurrent = importGraph.Header.Insert(newCurrent);
					}
					importGraph.Header.Current = newCurrent;
					importGraph.ImportStatement(info, false);
					importGraph.Save.Press();
					this.Clear();
					Caches[typeof(CABankTran)].ClearQueryCacheObsolete();
					this.TranFilter.Current = currFilter;
					if (!currFilter.CashAccountID.HasValue)
					{
						currFilter.CashAccountID = importGraph.Header.Current.CashAccountID;
						List<Filter> result = new List<Filter>();
						result.Add(currFilter);
						return result;
					}
				}
			}
			return adapter.Get();
		}


		public PXAction<Filter> clearMatch;
		[PXUIField(DisplayName = Messages.ClearMatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable ClearMatch(PXAdapter adapter)
		{
			CABankTran detail = Details.Current;
			ClearMatchProc(detail);
			return adapter.Get();
		}

		public PXAction<Filter> clearAllMatches;
		[PXUIField(DisplayName = Messages.ClearAllMatches, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable ClearAllMatches(PXAdapter adapter)
		{
			foreach (CABankTran detail in Details.Select())
			{
				ClearMatchProc(detail);
			}
			return adapter.Get();
		}

		protected virtual void ClearMatchProc(CABankTran detail)
		{
			if (detail.Processed == false && (detail.DocumentMatched == true || detail.CreateDocument == true))
			{
				foreach (CABankTranMatch match in TranMatch.Select(detail.TranID))
				{
					if (IsMatchedToExpenseReceipt(match))
					{
						EPExpenseClaimDetails receipt =
							PXSelect<EPExpenseClaimDetails,
									Where<EPExpenseClaimDetails.claimDetailCD,
										Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>
								.Select(this, match.DocRefNbr);

						receipt.BankTranDate = null;

						ExpenseReceipts.Update(receipt);
					}

					TranMatch.Delete(match);
				}

				ClearFields(detail);
				Details.Update(detail);
			}
		}

		public static void ClearFields(CABankTran detail)
		{
			detail.CreateDocument = false;
			detail.DocumentMatched = false;
			detail.MultipleMatching = false;
			detail.BAccountID = null;
			detail.OrigModule = null;
			detail.PaymentMethodID = null;
			detail.PMInstanceID = null;
			detail.LocationID = null;
			detail.EntryTypeID = null;
			detail.UserDesc = null;
			detail.InvoiceNotFound = null;
		}
		public PXAction<Filter> hide;
		[PXUIField(DisplayName = Messages.HideTran, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable Hide(PXAdapter adapter)
		{
			CABankTran detail = Details.Current;
			if (detail.Processed == false && Details.Ask(Messages.HideTran, Messages.HideTranMsg, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
				ClearMatchProc(detail);
				detail.Hidden = true;
				detail.Processed = true;
				Details.Update(detail);
			}
			return adapter.Get();
		}

		[Serializable]
		public class CreateRuleSettings : IBqlTable
		{
			public abstract class ruleName : PX.Data.BQL.BqlString.Field<ruleName> { }

			[PXDBString(30, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
			[PXUIField(DisplayName = "Rule ID", Required = true)]
			public virtual string RuleName { get; set; }
		}

		public PXFilter<CreateRuleSettings> RuleCreation;

		public PXAction<Filter> createRule;

		[PXUIField(DisplayName = Messages.CreateRule, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		public virtual void CreateRule()
		{
			var currentTran = DetailsForPaymentCreation.Current as CABankTran;

			if (currentTran == null || currentTran.CreateDocument != true)
				return;

			if (currentTran.OrigModule != BatchModule.CA)
				throw new PXException(Messages.BankRuleOnlyCADocumentsSupported);

			var rulesGraph = PXGraph.CreateInstance<CABankTranRuleMaintPopup>();
			var rule = new CABankTranRulePopup
			{
				RuleCD = RuleCreation.Current.RuleName,
				BankDrCr = currentTran.DrCr,
				BankTranCashAccountID = currentTran.CashAccountID,
				TranCode = currentTran.TranCode,
				BankTranDescription = currentTran.TranDesc,
				AmountMatchingMode = MatchingMode.Equal,
				CuryTranAmt = Math.Abs(currentTran.CuryTranAmt ?? 0.0m),
				DocumentModule = currentTran.OrigModule,
				DocumentEntryTypeID = currentTran.EntryTypeID,
				TranCuryID = currentTran.CuryID
			};
			rulesGraph.Rule.Cache.Insert(rule);

			PXRedirectHelper.TryRedirect(rulesGraph, PXRedirectHelper.WindowMode.Popup);
		}

		public PXAction<Filter> unapplyRule;

		[PXUIField(DisplayName = Messages.ClearRule, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		public virtual void UnapplyRule()
		{
			var currentTran = DetailsForPaymentCreation.Current as CABankTran;

			if (currentTran == null || currentTran.CreateDocument != true || currentTran.RuleID == null)
				return;

			ClearRule(DetailsForPaymentCreation.Cache, currentTran);
		}

		public PXAction<Filter> viewPayment;

		[PXUIField(DisplayName = Messages.ViewPayment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewPayment()
		{
			var currentPayment = DetailMatchesCA.Current;
			if (currentPayment == null)
				return;

			PXRedirectHelper.TryRedirect(DetailMatchesCA.Cache, currentPayment, "Document", PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<Filter> viewInvoice;

		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewInvoice()
		{
			var currentInvoice = DetailMatchingInvoices.Current;
			if (currentInvoice == null)
				return;

			PXCache cache = null;
			object document = null;

			switch (currentInvoice.OrigModule)
			{
				case BatchModule.AP:
					{
						document = (APInvoice)PXSelect<
						    APInvoice, 
						    Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, 
						        And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>
						    .Select(this, currentInvoice.OrigTranType, currentInvoice.OrigRefNbr);

						if (document != null)
						{
							cache = this.Caches[typeof(APInvoice)];
						}
					}
					break;
				case BatchModule.AR:
					{
						document = (ARInvoice)PXSelect<
						    ARInvoice, 
						    Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, 
						        And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
						    .Select(this, currentInvoice.OrigTranType, currentInvoice.OrigRefNbr);
						if (document != null)
						{
							cache = this.Caches[typeof(ARInvoice)];
						}
					}
					break;
			}

			if (cache != null && document != null)
			{
				PXRedirectHelper.TryRedirect(cache, document, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
		}
		public PXAction<Filter> refreshAfterRuleCreate;
		[PXButton]
		public virtual void RefreshAfterRuleCreate()
		{
			AttemptApplyRules((CABankTran)Details.Current, false);
		}

		public PXAction<Filter> viewDocumentToApply;
		[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewDocumentToApply(PXAdapter adapter)
		{
			CABankTran header = DetailsForPaymentCreation.Current;
			CABankTranAdjustment row = Adjustments.Current;

			if (header?.OrigModule == GL.BatchModule.AP)
			{
				APRegister doc = PXSelect<
				    APRegister, 
				    Where<APRegister.docType, Equal<Required<APRegister.docType>>, 
				        And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>
				    .Select(this, row.AdjdDocType, row.AdjdRefNbr);

				PXRedirectHelper.TryRedirect(dummyAPRegister.Cache, doc, Messages.ViewDocument, PXRedirectHelper.WindowMode.NewWindow);
			}

			if (header.OrigModule == GL.BatchModule.AR)
			{
				ARRegister doc = PXSelect<
				    ARRegister, 
				    Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
				        And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>
				    .Select(this, row.AdjdDocType, row.AdjdRefNbr);

				PXRedirectHelper.TryRedirect(dummyARRegister.Cache, doc, Messages.ViewDocument, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
		}

		public PXAction<Filter> ViewExpenseReceipt;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewExpenseReceipt(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(EPExpenseClaimDetails.DocType, ExpenseReceiptDetailMatches.Current.RefNbr, BatchModule.EP);

			return adapter.Get();
		}

		public PXAction<Filter> ResetMatchSettingsToDefault;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, DisplayName = "Reset to Default")]
		[PXButton]
		protected virtual IEnumerable resetMatchSettingsToDefault(PXAdapter adapter)
		{
			PXCache cache = cashAccount.Cache;
			CashAccount row = cashAccount.Current;

			cache.SetDefaultExt<CashAccount.receiptTranDaysBefore>(row);
			cache.SetDefaultExt<CashAccount.receiptTranDaysAfter>(row);
			cache.SetDefaultExt<CashAccount.disbursementTranDaysBefore>(row);
			cache.SetDefaultExt<CashAccount.disbursementTranDaysAfter>(row);
			cache.SetDefaultExt<CashAccount.allowMatchingCreditMemo>(row);
			cache.SetDefaultExt<CashAccount.refNbrCompareWeight>(row);
			cache.SetDefaultExt<CashAccount.dateCompareWeight>(row);
			cache.SetDefaultExt<CashAccount.payeeCompareWeight>(row);
			cache.SetDefaultExt<CashAccount.dateMeanOffset>(row);
			cache.SetDefaultExt<CashAccount.dateSigma>(row);
			cache.SetDefaultExt<CashAccount.curyDiffThreshold>(row);
			cache.SetDefaultExt<CashAccount.amountWeight>(row);
			cache.SetDefaultExt<CashAccount.emptyRefNbrMatching>(row);
			cache.SetDefaultExt<CashAccount.skipVoided>(row);

			row = (CashAccount)cache.Update(row);

			row.MatchSettingsPerAccount = false;

			return adapter.Get();
		}
		#endregion
		#region Events

		#region CABankTranInvoiceMatch Events

		protected virtual void CABankTranInvoiceMatch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABankTranInvoiceMatch row = e.Row as CABankTranInvoiceMatch;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CABankTranInvoiceMatch.isMatched>(sender, row, true);
		}

		protected virtual void CABankTranInvoiceMatch_IsMatched_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTranInvoiceMatch row = e.Row as CABankTranInvoiceMatch;
			if ((bool?)e.NewValue == true 
					&& ((Details.Current.DocumentMatched == true && (Details.Current.MultipleMatching != true || Details.Current.MatchedToInvoice != true)) 
						|| Details.Current.CreateDocument == true))
			{
				throw new PXSetPropertyException(Messages.AnotherOptionChosen, PXErrorLevel.RowWarning);
			}
		}

		protected virtual void CABankTranInvoiceMatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CABankTran currentTran = Details.Current;
			CABankTranInvoiceMatch row = e.Row as CABankTranInvoiceMatch;
			CABankTranInvoiceMatch oldRow = e.OldRow as CABankTranInvoiceMatch;
			if (!sender.ObjectsEqual<CABankTranInvoiceMatch.isMatched>(row, oldRow))
			{
				if (row.IsMatched == true)
				{
					bool cashDiscIsApplicable = row.CuryDiscAmt != null
												&& currentTran.TranDate != null
												&& row.DiscDate != null
												&& (DateTime)currentTran.TranDate <= (DateTime)row.DiscDate;
					
					CABankTranMatch match = new CABankTranMatch()
					{
						TranID = currentTran.TranID,
						TranType = currentTran.TranType,
						DocModule = row.OrigModule,
						DocType = row.OrigTranType,
						DocRefNbr = row.OrigRefNbr,
						ReferenceID = row.ReferenceID,
						CuryApplAmt = row.CuryTranAmt - (cashDiscIsApplicable ? row.CuryDiscAmt : 0)
					};
					TranMatch.Insert(match);
				}
				else
				{
					foreach (var match in TranMatch.Select(currentTran.TranID).RowCast<CABankTranMatch>()
												.Where(item => item.DocModule == row.OrigModule
													&& item.DocType == row.OrigTranType
													&& item.DocRefNbr == row.OrigRefNbr))
					{
						TranMatch.Delete(match);
					}
				}
				bool documentMatched = TranMatch.Select(currentTran.TranID).Any_();
				currentTran.DocumentMatched = documentMatched;

				if(!documentMatched)
				{
					Details.Cache.SetValueExt<CABankTran.origModule>(currentTran, null);
				}
				Details.Cache.Update(currentTran);
			}
			sender.IsDirty = false;
		}

		protected virtual void CABankTranInvoiceMatch_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CABankTranInvoiceMatch_OrigTranType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

        #endregion

        #region CABankTranExpenseClaimDetailMatch Events
        protected virtual void _(Events.RowSelected<CABankTranExpenseDetailMatch> e)
        {
            if (e.Row == null)
                return;
            PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<CABankTranExpenseDetailMatch.isMatched>(e.Cache, e.Row, true);
        }

        protected virtual void _(Events.RowUpdated<CABankTranExpenseDetailMatch> e)
        {
            CABankTran currentTran = Details.Current;

            if (!e.Cache.ObjectsEqual<CABankTranExpenseDetailMatch.isMatched>(e.Row, e.OldRow))
            {
	            EPExpenseClaimDetails receipt =
		            PXSelect<EPExpenseClaimDetails,
				            Where<EPExpenseClaimDetails.claimDetailCD,
					            Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>
			            .Select(this, e.Row.RefNbr);

				if (e.Row.IsMatched == true)
                {
                    CABankTranMatch match = new CABankTranMatch()
                    {
                        TranID = currentTran.TranID,
                        TranType = currentTran.TranType,
                        DocModule = BatchModule.EP,
                        DocRefNbr = e.Row.RefNbr,
                        DocType = EPExpenseClaimDetails.DocType,
                        ReferenceID = e.Row.ReferenceID
                    };

	                TranMatch.Insert(match);

	                receipt.BankTranDate = currentTran.TranDate;

					ExpenseReceipts.Update(receipt);
                }
                else
                {
                    foreach (var match in TranMatch.Select(currentTran.TranID))
                    {
                        TranMatch.Delete(match);
                    }

	                receipt.BankTranDate = null;

	                ExpenseReceipts.Update(receipt);

					Details.Cache.SetValueExt<CABankTran.origModule>(currentTran, null);
                }
                currentTran.DocumentMatched = e.Row.IsMatched;
                Details.Cache.Update(currentTran);
            }

            e.Cache.IsDirty = false;
        }

        protected virtual void _(Events.RowPersisting<CABankTranExpenseDetailMatch> e)
        {
            e.Cancel = true;
        }

		protected virtual void _(Events.FieldVerifying<CABankTranExpenseDetailMatch.isMatched> e)
		{
			CABankTranExpenseDetailMatch row = e.Row as CABankTranExpenseDetailMatch;
			if ((bool?)e.NewValue == true && (Details.Current.DocumentMatched == true || Details.Current.CreateDocument == true))
			{
				throw new PXSetPropertyException(Messages.AnotherOptionChosen, PXErrorLevel.RowWarning);
			}
		}

		#endregion

		#region CABankTranAdjustment Events

		protected virtual void CABankTranAdjustment_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;
			adj.AdjgDocDate = DetailsForPaymentCreation.Current.TranDate;
			adj.Released = false;
			adj.CuryDocBal = null;
			adj.CuryDiscBal = null;
			adj.CuryWhTaxBal = null;

			if (String.IsNullOrEmpty(adj.AdjdRefNbr))
			{
				sender.SetValueExt<CABankTranAdjustment.curyAdjgAmt>(adj, null);
				sender.SetValueExt<CABankTranAdjustment.curyAdjgDiscAmt>(adj, null);
				sender.SetValueExt<CABankTranAdjustment.curyAdjgWhTaxAmt>(adj, null);
				sender.SetValueExt<CABankTranAdjustment.curyAdjgWOAmt>(adj, null);
			}
			if (adj.CuryAdjgAmt == null || adj.CuryAdjgAmt == 0.0m)
			{
				adj.CuryAdjgAmt = null;

				if (DetailsForPaymentCreation.Current.OrigModule == GL.BatchModule.AP)
				{
					PopulateAdjustmentFieldsAP(adj);
					sender.SetDefaultExt<CABankTranAdjustment.adjdTranPeriodID>(e.Row);
				}
				else if (DetailsForPaymentCreation.Current.OrigModule == GL.BatchModule.AR)
				{
					PopulateAdjustmentFieldsAR(adj);
					sender.SetDefaultExt<CABankTranAdjustment.adjdTranPeriodID>(e.Row);
				}
			}
			else
			{
				sender.SetValueExt<CABankTranAdjustment.curyAdjgAmt>(adj, adj.CuryAdjgAmt);
			}
			if (adj.CuryAdjgDiscAmt != null && adj.CuryAdjgDiscAmt != 0.0m)
			{
				sender.SetValueExt<CABankTranAdjustment.curyAdjgDiscAmt>(adj, adj.CuryAdjgDiscAmt);
			}
			if (adj.CuryAdjgWhTaxAmt != null && adj.CuryAdjgWhTaxAmt != 0.0m)
			{
				sender.SetValueExt<CABankTranAdjustment.curyAdjgWhTaxAmt>(adj, adj.CuryAdjgWhTaxAmt);
			}
		}

		protected virtual void CABankTranAdjustment_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;
			if (adj == null)
				return;

			foreach (CABankTranAdjustment other in Adjustments.Select())
			{
				if (object.ReferenceEquals(adj, other))
					continue;

				if (other.AdjdDocType == adj.AdjdDocType && other.AdjdRefNbr == (string)e.NewValue && other.AdjdModule == adj.AdjdModule)
					throw new PXSetPropertyException<CABankTranAdjustment.adjdRefNbr>(Messages.PaymentAlreadyAppliedToThisDocument);
			}
		}

		protected virtual void CABankTranAdjustment_AdjdCuryRate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, ((int)0).ToString());
			}
		}

		protected virtual void CABankTranAdjustment_AdjdCuryRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;

			CurrencyInfo pay_info = PXSelect<
			    CurrencyInfo, 
			    Where<CurrencyInfo.curyInfoID, Equal<Current<CABankTranAdjustment.adjgCuryInfoID>>>>
			    .SelectSingleBound(this, new object[] { e.Row });
			CurrencyInfo vouch_info = PXSelect<
			    CurrencyInfo, 
			    Where<CurrencyInfo.curyInfoID, Equal<Current<CABankTranAdjustment.adjdCuryInfoID>>>>
			    .SelectSingleBound(this, new object[] { e.Row });

			decimal payment_docbal = (decimal)adj.CuryAdjgAmt;
			decimal discount_docbal = (decimal)adj.CuryAdjgDiscAmt;
			decimal invoice_amount;

			if (string.Equals(pay_info.CuryID, vouch_info.CuryID) && adj.AdjdCuryRate != 1m)
			{
				adj.AdjdCuryRate = 1m;
				vouch_info.SetCuryEffDate(currencyinfo.Cache, DetailsForPaymentCreation.Current.TranDate);
			}
			else if (string.Equals(vouch_info.CuryID, vouch_info.BaseCuryID))
			{
				adj.AdjdCuryRate = pay_info.CuryMultDiv == "M" ? 1 / pay_info.CuryRate : pay_info.CuryRate;
			}
			else
			{
				vouch_info.CuryRate = adj.AdjdCuryRate;
				vouch_info.RecipRate = Math.Round(1m / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);
				vouch_info.CuryMultDiv = "M";
				PXCurrencyAttribute.CuryConvBase(sender, vouch_info, (decimal)adj.CuryAdjdAmt, out payment_docbal);
				PXCurrencyAttribute.CuryConvBase(sender, vouch_info, (decimal)adj.CuryAdjdDiscAmt, out discount_docbal);
				PXCurrencyAttribute.CuryConvBase(sender, vouch_info, (decimal)adj.CuryAdjdAmt + (decimal)adj.CuryAdjdDiscAmt, out invoice_amount);

				vouch_info.CuryRate = Math.Round((decimal)adj.AdjdCuryRate * (pay_info.CuryMultDiv == "M" ? (decimal)pay_info.CuryRate : 1m / (decimal)pay_info.CuryRate), 8, MidpointRounding.AwayFromZero);
				vouch_info.RecipRate = Math.Round((pay_info.CuryMultDiv == "M" ? 1m / (decimal)pay_info.CuryRate : (decimal)pay_info.CuryRate) / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);

				if (payment_docbal + discount_docbal != invoice_amount)
					discount_docbal += invoice_amount - discount_docbal - payment_docbal;
			}

			Caches[typeof(CurrencyInfo)].MarkUpdated(vouch_info);

			if (payment_docbal != (decimal)adj.CuryAdjgAmt)
				sender.SetValue<CABankTranAdjustment.curyAdjgAmt>(e.Row, payment_docbal);

			if (discount_docbal != (decimal)adj.CuryAdjgDiscAmt)
				sender.SetValue<CABankTranAdjustment.curyAdjgDiscAmt>(e.Row, discount_docbal);

			UpdateBalance((CABankTranAdjustment)e.Row, true);
		}

		protected virtual void CABankTranAdjustment_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;
			if (adj != null && adj.AdjdRefNbr != null)
			{
				if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
				{
					UpdateBalance((CABankTranAdjustment)e.Row, false);
				}


				if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
				}

				if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt).ToString());
				}
			}
			else
			{
				e.NewValue = 0m;
			}
		}

		protected virtual void CABankTranAdjustment_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.OldValue != null && ((CABankTranAdjustment)e.Row).CuryDocBal == 0m && ((CABankTranAdjustment)e.Row).CuryAdjgAmt < (decimal)e.OldValue)
			{
				((CABankTranAdjustment)e.Row).CuryAdjgDiscAmt = 0m;
			}
			UpdateBalance((CABankTranAdjustment)e.Row, true);
		}

		protected virtual void CABankTranAdjustment_CuryAdjgDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;
			if (adj != null && adj.AdjdRefNbr != null)
			{
				if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
				{
					UpdateBalance((CABankTranAdjustment)e.Row, false);
				}

				if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
				}

				if ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
				}

				if (adj.CuryAdjgAmt != null && (sender.GetValuePending<CABankTranAdjustment.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<CABankTranAdjustment.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
				{
					if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
					{
						throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
					}
				}
			}
			else
			{
				e.NewValue = 0m;
			}
		}

		protected virtual void CABankTranAdjustment_CuryAdjgDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateBalance((CABankTranAdjustment)e.Row, true);
		}

		protected virtual void CABankTranAdjustment_CuryAdjgWhTaxAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;
			if (adj != null && adj.AdjdRefNbr != null)
			{
				if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
				{
					UpdateBalance((CABankTranAdjustment)e.Row, false);
				}

				if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
				}

				if ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
				}

				if (adj.CuryAdjgAmt != null && (sender.GetValuePending<CABankTranAdjustment.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<CABankTranAdjustment.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
				{
					if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
					{
						throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
					}
				}
			}
			else
			{
				e.NewValue = 0m;
			}
		}
		protected virtual void CABankTranAdjustment_CuryAdjgWOAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;
			if (adj != null && adj.AdjdRefNbr != null)
			{
				if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
				{
					UpdateBalance(adj, false);
				}

				if (adj.VoidAdjNbr == null && ((decimal)e.NewValue) < 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if (adj.VoidAdjNbr != null && ((decimal)e.NewValue) > 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
				}

				if ((adj.CuryWhTaxBal ?? 0m) + (adj.CuryAdjgWhTaxAmt ?? 0m) - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((adj.CuryWhTaxBal ?? 0m) + (adj.CuryAdjgWhTaxAmt ?? 0m)).ToString());
				}

				if (adj.CuryAdjgAmt != null && (sender.GetValuePending<CABankTranAdjustment.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<CABankTranAdjustment.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
				{
					if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
					{
						throw new PXSetPropertyException(CS.Messages.Entry_LE, ((adj.CuryDocBal ?? 0m) + (adj.CuryAdjgWhTaxAmt ?? 0m)).ToString());
					}
				}
			}
			else
			{
				e.NewValue = 0m;
			}
		}
		protected virtual void CABankTranAdjustment_CuryAdjgWOAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateBalance((CABankTranAdjustment)e.Row, false);
		}

		protected virtual void CABankTranAdjustment_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && ((CABankTranAdjustment)e.Row).AdjdCuryInfoID != null && ((CABankTranAdjustment)e.Row).CuryDocBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
			{
				UpdateBalance((CABankTranAdjustment)e.Row, false);
			}
			if (e.Row != null)
			{
				e.NewValue = ((CABankTranAdjustment)e.Row).CuryDocBal;
			}
			e.Cancel = true;
		}

		protected virtual void CABankTranAdjustment_CuryDiscBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && ((CABankTranAdjustment)e.Row).AdjdCuryInfoID != null && ((CABankTranAdjustment)e.Row).CuryDiscBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
			{
				UpdateBalance((CABankTranAdjustment)e.Row, false);
			}
			if (e.Row != null)
			{
				e.NewValue = ((CABankTranAdjustment)e.Row).CuryDiscBal;
			}
			e.Cancel = true;
		}

		protected virtual void CABankTranAdjustment_CuryWhTaxBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && ((CABankTranAdjustment)e.Row).AdjdCuryInfoID != null && ((CABankTranAdjustment)e.Row).CuryWhTaxBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
			{
				UpdateBalance((CABankTranAdjustment)e.Row, false);
			}
			if (e.Row != null)
			{
				e.NewValue = ((CABankTranAdjustment)e.Row).CuryWhTaxBal;
			}
			e.Cancel = true;
		}

		protected virtual void CABankTranAdjustment_AdjdDocType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTranAdjustment row = e.Row as CABankTranAdjustment;
			if (row == null) return;
			if (row.AdjdDocType != (string)e.OldValue)
			{
				sender.SetValueExt<CABankTranAdjustment.adjdRefNbr>(row, null);
				sender.SetValueExt<CABankTranAdjustment.curyAdjgAmt>(row, Decimal.Zero);
			}

		}

		protected virtual void CABankTranAdjustment_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CABankTranAdjustment row = (CABankTranAdjustment)e.Row;
			if (row != null)
			{
				foreach (CABankTranAdjustment adjustmentRecord in this.Adjustments.Select())
				{
					if (row.AdjdRefNbr != null && adjustmentRecord.AdjdRefNbr == row.AdjdRefNbr && adjustmentRecord.AdjdDocType == row.AdjdDocType)
					{
						PXEntryStatus status = this.Adjustments.Cache.GetStatus(adjustmentRecord);
						if (!(status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted))
						{
							sender.RaiseExceptionHandling<CABankTranAdjustment.adjdRefNbr>(e.Row, null, new PXException(Messages.DuplicatedKeyForRow));
							e.Cancel = true;
							break;
						}
					}
				}
			}
		}

		protected virtual void CABankTranAdjustment_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			CABankTranAdjustment row = (CABankTranAdjustment)e.Row;
			if (row == null) return;
			CABankTran det = Details.Current;
			sender.SetValue<CABankTranAdjustment.adjdModule>(row, det.OrigModule);
			CurrencyInfoAttribute.SetDefaults<CABankTran.curyInfoID>(Details.Cache, det);
			CurrencyInfoAttribute.SetDefaults<CABankTranAdjustment.adjgCuryInfoID>(sender, row);
		}

		protected virtual void CABankTranAdjustment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (((CABankTranAdjustment)e.Row).AdjdRefNbr == null)
			{
				Details.Cache.RaiseExceptionHandling<CABankTran.createDocument>(Details.Current, Details.Current.CreateDocument, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<CABankTranAdjustment.adjdRefNbr>(sender)));
			}
		}
		protected virtual void CABankTranAdjustment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;
			if (Details.Current != null && adj != null)
			{
				PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.adjdRefNbr>(sender, adj, adj.AdjdRefNbr == null);
				PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.adjdDocType>(sender, adj, adj.AdjdRefNbr == null);
				PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.curyAdjgAmt>(sender, adj, adj.AdjdRefNbr != null);
				PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.curyAdjgDiscAmt>(sender, adj, adj.AdjdRefNbr != null);
				PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.curyAdjgWhTaxAmt>(sender, adj, adj.AdjdRefNbr != null);

				Customer customer = PXSelect<
				    Customer, 
				    Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
				    .Select(this, Details.Current.PayeeBAccountID);
				PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.curyAdjgWOAmt>(sender, adj, canBeWrittenOff(customer, adj) && adj.Released != true && adj.Voided == false);
				PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.writeOffReasonCode>(sender, adj, canBeWrittenOff(customer, adj));

				if (Details.Current.InvoiceInfo != null && adj.AdjdRefNbr == Details.Current.InvoiceInfo
					&& (adj.CuryAdjgAmt != Details.Current.CuryTotalAmt || adj.CuryDocBal != decimal.Zero))
				{
					sender.RaiseExceptionHandling<CABankTranAdjustment.curyAdjgAmt>(adj, adj.CuryAdjgAmt, new PXSetPropertyException(Messages.NotExactAmount, PXErrorLevel.Warning));
				}
			}
			PXUIFieldAttribute.SetVisible<ARInvoice.customerID>(Caches[typeof(ARInvoice)], null, PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>() && DetailsForPaymentCreation.Current?.OrigModule == BatchModule.AR);
		}

		Func<Customer, CABankTranAdjustment, bool> canBeWrittenOff = (customer, adj) =>
			customer != null
			&& adj.AdjdRefNbr != null
			&& customer.SmallBalanceAllow == true
			&& customer.SmallBalanceLimit > 0
			&& adj.AdjdDocType != ARDocType.CreditMemo;

		protected virtual void CABankTranAdjustment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CABankTranAdjustment row = (CABankTranAdjustment)e.Row;
			if (row == null)
				return;
			CABankTran tranRow = PXSelect<
			    CABankTran, 
			    Where<CABankTran.tranID, Equal<Required<CABankTranAdjustment.tranID>>>>
			    .Select(this, row.TranID);

			if (row.AdjdRefNbr == null)
			{
				Details.Cache.RaiseExceptionHandling<CABankTran.createDocument>(tranRow, tranRow.CreateDocument, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<CABankTranAdjustment.adjdRefNbr>(sender)));
			}
			if (row.CuryWhTaxBal < 0m)
			{
				sender.RaiseExceptionHandling<CABankTran.createDocument>(e.Row, row.CuryAdjgWhTaxAmt, new PXSetPropertyException(AR.Messages.DocumentBalanceNegative));
			}

			if (tranRow.OrigModule == BatchModule.AR && row.CuryAdjgWhTaxAmt > 0 && string.IsNullOrEmpty(row.WriteOffReasonCode))
			{
				Details.Cache.RaiseExceptionHandling<CABankTran.createDocument>(tranRow, tranRow.CreateDocument, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CABankTranAdjustment.writeOffReasonCode>(sender), PXErrorLevel.RowError));
			}
		}

		#endregion
		#region CABankTran Events

		protected virtual void CABankTran_CreateDocument_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row.DocumentMatched == true && (bool?)e.NewValue == true)
			{
				throw new PXSetPropertyException("", PXErrorLevel.RowInfo);
			}
		}

		protected virtual bool ApplyInvoiceInfo(PXCache sender, CABankTran row)
		{
			if (row.CreateDocument == true && row.InvoiceInfo != null)
			{
				string Module;
				object linkedInvoice = FindInvoiceByInvoiceInfo(row, out Module);
				if (linkedInvoice != null)
				{
					int? payeeBAccountID;
					string paymentMethodID;
					int? pmInstanceID = null;
					string RefNbr;

					switch (Module)
					{
						case GL.BatchModule.AP:
							APInvoice APinvoice = linkedInvoice as APInvoice;
							if (APinvoice == null)
							{
								throw new PXSetPropertyException(Messages.WrongInvoiceType);
							}
							payeeBAccountID = APinvoice.VendorID;
							paymentMethodID = APinvoice.PayTypeID;
							RefNbr = APinvoice.RefNbr;
							break;
						case GL.BatchModule.AR:
							ARInvoice ARinvoice = linkedInvoice as ARInvoice;
							if (ARinvoice == null)
							{
								throw new PXSetPropertyException(Messages.WrongInvoiceType);
							}
							payeeBAccountID = ARinvoice.CustomerID;
							paymentMethodID = ARinvoice.PaymentMethodID;
							pmInstanceID = ARinvoice.PMInstanceID;
							RefNbr = ARinvoice.RefNbr;
							break;
						default:
							throw new PXSetPropertyException(Messages.UnknownModule);
					}
					sender.SetValueExt<CABankTran.origModule>(row, Module);
					sender.SetValue<CABankTran.payeeBAccountID>(row, payeeBAccountID);
					object refNbr = row.PayeeBAccountID;
					sender.RaiseFieldUpdating<CABankTran.payeeBAccountID>(row, ref refNbr);
					sender.RaiseFieldUpdated<CABankTran.payeeBAccountID>(row, null);
					if (paymentMethodID != null)
					{
						try
						{
							sender.SetValueExt<CABankTran.paymentMethodID>(row, paymentMethodID);
							sender.SetValueExt<CABankTran.pMInstanceID>(row, pmInstanceID);
						}
						catch (PXSetPropertyException)
						{
						}
					}

					try
					{
						CABankTranAdjustment adj = new CABankTranAdjustment()
						{
							TranID = row.TranID
						};

						adj = Adjustments.Insert(adj);

						adj.AdjdDocType = APInvoiceType.Invoice;
						adj.AdjdRefNbr = RefNbr;

						Adjustments.Update(adj);
					}
					catch
					{
						throw new PXSetPropertyException(Messages.CouldNotAddApplication, row.InvoiceInfo);
					}

					return true;
				}
				else
				{
					sender.SetValue<CABankTran.invoiceNotFound>(row, true);
				}
			}
			return false;
		}

		protected virtual void CABankTran_CreateDocument_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row == null) return;
			if (row.CreateDocument == false)
			{
				Details.Cache.SetValueExt<CABankTran.documentMatched>(row, row.CreateDocument);
			}
			ResetTranFields(sender, row);

			if (row.CreateDocument == true)
			{
				bool isInvoiceFound = false;

				try
				{
					isInvoiceFound = ApplyInvoiceInfo(sender, row);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<CABankTran.invoiceInfo>(row, row.CreateDocument, new PXSetPropertyException(ex.Message, PXErrorLevel.Warning));
					foreach (CABankTranAdjustment adj in Adjustments.Select())
					{
						Adjustments.Delete(adj);
					}
				}

				bool clearingAccount = ((CashAccount)cashAccount.Select())?.ClearingAccount == true;

				row.UserDesc = CutOffTranDescTo256(row.TranDesc);

				if (row.CreateDocument == true && isInvoiceFound == false && !clearingAccount)
				{
					AttemptApplyRules(row, e.ExternalCall == false);
				}

				row.UserDesc = CutOffTranDescTo256(row.TranDesc);

				Details.Cache.SetValueExt<CABankTran.documentMatched>(row, row.CreateDocument == true && ValidateTranFields(sender, row));
			}
			else
			{
				sender.SetValue<CABankTran.invoiceNotFound>(row, false);
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		private string CutOffTranDescTo256(string description)
		{
			return description?.Length > 256 ? description.Substring(0, 255) : description;
		}

		protected virtual void ResetTranFields(PXCache cache, CABankTran transaction)
		{
			cache.SetDefaultExt<CABankTran.ruleID>(transaction);
			cache.SetDefaultExt<CABankTran.origModule>(transaction);
			cache.SetDefaultExt<CABankTran.curyTotalAmt>(transaction);
		}

		protected virtual void ClearRule(PXCache cache, CABankTran transaction)
		{
			ResetTranFields(cache, transaction);
		}

		protected virtual void CABankTran_MultipleMatching_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row == null) return;

			if (row.MultipleMatching != true && row.DocumentMatched == true && row.MatchedToInvoice == true)
			{
				Details.Cache.SetValue<CABankTran.documentMatched>(row, false);
				Details.Cache.SetValue<CABankTran.matchedToExisting>(row, null);
				Details.Cache.SetValue<CABankTran.matchedToInvoice>(row, null);
				Details.Cache.SetValue<CABankTran.matchedToExpenseReceipt>(row, null);
				Details.Cache.SetValueExt<CABankTran.origModule>(row, null);
			}
		}

		protected virtual void CABankTran_OrigModule_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (string.IsNullOrEmpty((string)e.NewValue))
			{
				return;
			}
			CashAccount cashaccount = cashAccount.Select();
			bool clearingAccount = cashaccount.ClearingAccount == true;
			string newModule = (string)e.NewValue;
			if (clearingAccount && 
				(newModule == GL.BatchModule.CA || 
				(newModule == GL.BatchModule.AP && ((CABankTran)e.Row).DrCr == DrCr.Credit)))
			{
				throw new PXSetPropertyException<CABankTran.origModule>(Messages.NotAllDocumentsAllowedForClearingAccount);
			}
		}
		protected virtual void CABankTran_OrigModule_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row != null && row.CreateDocument == true)
			{
				CashAccount cashaccount = cashAccount.Select();
				bool clearingAccount = cashaccount.ClearingAccount == true;
				if (clearingAccount)
				{
					e.NewValue = GL.BatchModule.AR;
				}
				else if (!String.IsNullOrEmpty(row.InvoiceInfo))
				{
					if (row.DrCr == CADrCr.CACredit)
						e.NewValue = GL.BatchModule.AP;
					else if (row.DrCr == CADrCr.CADebit)
						e.NewValue = GL.BatchModule.AR;
				}
				else
				{
					e.NewValue = GL.BatchModule.CA;
				}
			}
			else
			{
				e.NewValue = null;
			}
			e.Cancel = true;
		}

		protected virtual void CABankTran_OrigModule_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<CABankTran.payeeBAccountID>(e.Row);
				sender.SetDefaultExt<CABankTran.entryTypeID>(e.Row);
			}
		}

		protected virtual void CABankTran_PayeeBAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<CABankTran.payeeLocationID>(e.Row);
				sender.SetDefaultExt<CABankTran.paymentMethodID>(e.Row);
				sender.SetDefaultExt<CABankTran.pMInstanceID>(e.Row);
				DetailMatchingInvoices.View.RequestRefresh();
			}
		}

		protected virtual void CABankTran_PayeeBAccountIDCopy_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row == null) return;
			sender.SetDefaultExt<CABankTran.payeeLocationID>(e.Row);
			sender.SetDefaultExt<CABankTran.paymentMethodID>(e.Row);
			sender.SetDefaultExt<CABankTran.pMInstanceID>(e.Row);
			DetailMatchingInvoices.View.RequestRefresh();
		}

		protected virtual void CABankTran_DocumentMatched_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row == null) return;
			if (row.DocumentMatched == true)
			{
				CABankTranMatch match = TranMatch.SelectSingle(row.TranID);
				if (match != null && !string.IsNullOrEmpty(match.DocRefNbr) && !IsMatchedToExpenseReceipt(match))
				{
					Details.Cache.SetValue<CABankTran.origModule>(row, match.DocModule);
					Details.Cache.SetValue<CABankTran.payeeBAccountIDCopy>(row, match.ReferenceID);
					object refNbr = row.PayeeBAccountIDCopy;
					Details.Cache.RaiseFieldUpdating<CABankTran.payeeBAccountIDCopy>(row, ref refNbr);
					Details.Cache.RaiseFieldUpdated<CABankTran.payeeBAccountIDCopy>(row, null);
				}
			}
		}
		protected virtual void CABankTran_EntryTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row.OrigModule == GL.BatchModule.CA && row.EntryTypeID != (string)e.OldValue)
			{
				foreach (CABankTranDetail split in TranSplit.Select())
				{
					TranSplit.Delete(split);
				}
				if (!string.IsNullOrEmpty(row.EntryTypeID))
				{
					CABankTranDetail newSplit = new CABankTranDetail();
					TranSplit.Insert(newSplit);
				}
			}
		}
		protected virtual void CABankTran_EntryTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row == null)
				return;
			if (row.OrigModule == GL.BatchModule.CA)
			{
				string entryTypeID = CASetup.Current.UnknownPaymentEntryTypeID;
				CAEntryType entryType = PXSelectJoin<
				    CAEntryType,
				    InnerJoin<CashAccountETDetail, 
				        On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
				    Where<CashAccountETDetail.accountID, Equal<Required<CABankTran.cashAccountID>>,
				        And<CAEntryType.module, Equal<GL.BatchModule.moduleCA>,
				        And<CAEntryType.drCr, Equal<Required<CABankTran.drCr>>,
				        And<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>>>>
				    .Select(this, row.CashAccountID, row.DrCr, entryTypeID);
				e.NewValue = (entryType != null) ? entryTypeID : null;
			}
			else
			{
				e.NewValue = null;
			}
			e.Cancel = true;
		}

		protected virtual void CABankTran_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<CABankTran.pMInstanceID>(e.Row);
			}
		}

		protected virtual void CABankTran_PaymentMethodIDCopy_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<CABankTran.pMInstanceID>(e.Row);
			}
		}

		protected virtual void CABankTran_EntryTypeId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row != null)
			{
				CAEntryType entryType = PXSelect<
				    CAEntryType,
				    Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>
				    .Select(this, row.EntryTypeID);
				if (entryType != null)
				{
					row.DrCr = entryType.DrCr;
					if (entryType.UseToReclassifyPayments == true && row.CashAccountID.HasValue)
					{
						CashAccount availableAccount = PXSelect<
						    CashAccount, 
						    Where<CashAccount.cashAccountID, NotEqual<Required<CashAccount.cashAccountID>>,
						        And<CashAccount.curyID, Equal<Required<CashAccount.curyID>>>>>
						    .SelectWindowed(sender.Graph, 0, 1, row.CashAccountID, row.CuryID);
						if (availableAccount == null)
						{
							sender.RaiseExceptionHandling<CABankTran.entryTypeID>(row, null, new PXSetPropertyException(Messages.EntryTypeRequiresCashAccountButNoOneIsConfigured, PXErrorLevel.Warning, row.CuryID));
						}
					}
				}

			}
		}

		protected virtual void CABankTran_MatchStatsInfo_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			if (row != null)
			{
				String message = null;
				if (row.DocumentMatched == true || row.CreateDocument == true)
				{
					if (row.MatchedToExisting == true && row.CreateDocument == false)
					{
						if (row.MatchedToInvoice == true)
						{
							message = PXMessages.LocalizeFormatNoPrefix(Messages.TransactionWillPayInvoice);
						}
					    else if (row.MatchedToExpenseReceipt == true)
					    {
					        message = PXMessages.LocalizeFormatNoPrefix(Messages.TransactionMatchedToExistingExpenseReceipt);
					    }
                        else
						{
							message = PXMessages.LocalizeFormatNoPrefix(Messages.TransactionMatchedToExistingDocument);
						}
					}
					else
					{
						if (row.RuleID != null)
						{
							message = PXMessages.LocalizeFormatNoPrefix(Messages.TransactionWillCreateNewDocumentBasedOnRuleDefined);
						}
						else
						{
							message = PXMessages.LocalizeFormatNoPrefix(Messages.TransactionWillCreateNewDocument);
						}
					}
				}
				else
				{
					message = PXMessages.LocalizeFormatNoPrefix(Messages.TRansactionNotMatched);
				}
				e.ReturnValue = message;
			}
		}

		protected virtual void CABankTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CABankTran row = (CABankTran)e.Row;
			CABankTran oldRow = (CABankTran)e.OldRow;

			if (row.CreateDocument == true)
			{
				if (oldRow.CreateDocument != true)
				{
					CurrencyInfoAttribute.SetDefaults<CABankTran.curyInfoID>(Details.Cache, row);
				}
			}
			if (row.CreateDocument == false || (row.OrigModule != GL.BatchModule.CA && row.OrigModule != GL.BatchModule.EP))
			{
				foreach (CABankTranDetail split in TranSplit.Select())
				{
					TranSplit.Delete(split);
				}
			}
			if ((oldRow != null && oldRow.CreateDocument == true) 
			    && (row.CreateDocument == false || (row.OrigModule != GL.BatchModule.AR 
														&& row.OrigModule != GL.BatchModule.AP
														&& row.OrigModule != GL.BatchModule.EP)))
			{
				foreach (CABankTranAdjustment adj in Adjustments.Select())
				{
					Adjustments.Delete(adj);
				}
			}
			if (oldRow != null && oldRow.MultipleMatching == true
							&& row.MultipleMatching == false)
			{
				foreach (CABankTranMatch match in TranMatch.Select(row.TranID))
				{
					if (!String.IsNullOrEmpty(match.DocRefNbr) && !IsMatchedToExpenseReceipt(match))
					{
						TranMatch.Delete(match);
					}
				}
			}
		}

		protected virtual void EnableTranFields(PXCache sender, CABankTran row)
		{
			bool matchedToCA = false;
			bool matchedToInv = false;
			bool matchedToReceipt = false;

			List<CABankTranMatch> matches = TranMatch.Select(row.TranID).RowCast<CABankTranMatch>().ToList();

			if (matches.Count != 0)
			{
				matchedToCA = matches.Any(match => match.CATranID.HasValue);
				matchedToReceipt = matches.Any(match => IsMatchedToExpenseReceipt(match));
				matchedToInv = matches.Any(match => !String.IsNullOrEmpty(match.DocRefNbr) && !IsMatchedToExpenseReceipt(match));

				int matchesTypeCount = 0;

				if (matchedToCA)
					matchesTypeCount++;
			    if (matchedToReceipt)
					matchesTypeCount++;
			    if (matchedToInv)
					matchesTypeCount++;

			    if (matchesTypeCount > 1)
			    {
				    throw new PXException(Messages.ErrorInMatchTable, row.TranID);
			    }
		    }

			bool needsPMInstance = false;
			if (row.OrigModule == BatchModule.AR)
			{
				var pm = (PaymentMethod)PXSelectorAttribute.Select<CABankTran.paymentMethodID>(sender, row);
				needsPMInstance = pm != null && pm.ARIsOnePerCustomer == false;
			}

			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CABankTran.multipleMatching>(sender, row, true);
			PXUIFieldAttribute.SetVisible<CABankTran.payeeLocationIDCopy>(sender, row, matchedToInv);
			PXUIFieldAttribute.SetEnabled<CABankTran.payeeLocationIDCopy>(sender, row, matchedToInv);
			PXUIFieldAttribute.SetVisible<CABankTran.paymentMethodIDCopy>(sender, row, matchedToInv);
			PXUIFieldAttribute.SetEnabled<CABankTran.paymentMethodIDCopy>(sender, row, matchedToInv);
			PXUIFieldAttribute.SetVisible<CABankTran.pMInstanceIDCopy>(sender, row, matchedToInv && needsPMInstance);
			PXUIFieldAttribute.SetEnabled<CABankTran.pMInstanceIDCopy>(sender, row, matchedToInv);
			PXUIFieldAttribute.SetVisible<CABankTran.curyTotalAmtCopy>(sender, row, matchedToInv);
			PXUIFieldAttribute.SetVisible<CABankTran.curyApplAmtMatch>(sender, row, matchedToInv);
			PXUIFieldAttribute.SetVisible<CABankTran.curyUnappliedBalMatch>(sender, row, matchedToInv);

			bool notMatched = !matchedToCA && !matchedToInv && !matchedToReceipt;

            PXUIFieldAttribute.SetEnabled<CABankTran.createDocument>(sender, row, notMatched);

			CAEntryType entryType = PXSelect<
			    CAEntryType,
			    Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>
			    .Select(this, row.EntryTypeID);
			if (entryType != null)
			{
				bool isReclassification = entryType.UseToReclassifyPayments ?? false;

				PXUIFieldAttribute.SetEnabled<CABankTranDetail.accountID>(TranSplit.Cache, null, !isReclassification);
				PXUIFieldAttribute.SetEnabled<CABankTranDetail.subID>(TranSplit.Cache, null, !isReclassification);
				PXUIFieldAttribute.SetEnabled<CABankTranDetail.branchID>(TranSplit.Cache, null, !isReclassification);
				PXUIFieldAttribute.SetEnabled<CABankTranDetail.cashAccountID>(TranSplit.Cache, null, isReclassification);
				PXUIFieldAttribute.SetVisible<CABankTranDetail.cashAccountID>(TranSplit.Cache, null, isReclassification);
				TranSplit.AllowInsert = true;
			}
			else
			{
				TranSplit.AllowInsert = false;
			}

			PXUIFieldAttribute.SetEnabled<CABankTranAdjustment.adjdCuryRate>(Adjustments.Cache, null, row.OrigModule == GL.BatchModule.AR || row.OrigModule == GL.BatchModule.AP);
			EnableCreateTab(sender, row, needsPMInstance);
		}

		private void EnableCreateTab(PXCache sender, CABankTran row, bool needsPMInstance)
		{
			bool CreatingDocument = row != null && row.CreateDocument == true;

			bool ruleApplied = row != null && row.RuleApplied == true;

			bool isARorAP = row != null && row.OrigModule != null && row.OrigModule != GL.BatchModule.CA && CreatingDocument;
			bool isAR = row != null && row.OrigModule == GL.BatchModule.AR && CreatingDocument;
			bool isCA = row != null && row.OrigModule == GL.BatchModule.CA && CreatingDocument;
			bool noAdjustmentsYet = Adjustments.Select().AsEnumerable().Any() == false;
			bool isReceipt = row != null && row.DrCr == DrCr.Debit;

			PXUIFieldAttribute.SetVisible<CABankTran.ruleID>(sender, row, CreatingDocument && ruleApplied);
			PXUIFieldAttribute.SetVisible<CABankTran.origModule>(sender, row, CreatingDocument);
			PXUIFieldAttribute.SetEnabled<CABankTran.origModule>(sender, row, CreatingDocument && noAdjustmentsYet && row.RuleID == null);
			PXUIFieldAttribute.SetVisible<CABankTran.entryTypeID>(sender, row, isCA);
			PXUIFieldAttribute.SetEnabled<CABankTran.entryTypeID>(sender, row, isCA && !ruleApplied);
			PXUIFieldAttribute.SetVisible<CABankTran.payeeBAccountID>(sender, row, isARorAP);
			PXUIFieldAttribute.SetEnabled<CABankTran.payeeBAccountID>(sender, row, isARorAP && noAdjustmentsYet);
			PXUIFieldAttribute.SetVisible<CABankTran.payeeLocationID>(sender, row, isARorAP);
			PXUIFieldAttribute.SetEnabled<CABankTran.payeeLocationID>(sender, row, isARorAP);
			PXUIFieldAttribute.SetVisible<CABankTran.paymentMethodID>(sender, row, isARorAP);
			PXUIFieldAttribute.SetEnabled<CABankTran.paymentMethodID>(sender, row, isARorAP);
			PXUIFieldAttribute.SetVisible<CABankTran.pMInstanceID>(sender, row, needsPMInstance);
			PXUIFieldAttribute.SetEnabled<CABankTran.pMInstanceID>(sender, row, isAR);
			PXUIFieldAttribute.SetVisible<CABankTran.invoiceInfo>(sender, row, isARorAP);
			PXUIFieldAttribute.SetEnabled<CABankTran.invoiceInfo>(sender, row, false);
			PXUIFieldAttribute.SetVisible<CABankTran.curyTotalAmt>(sender, row, isARorAP || isCA);
			PXUIFieldAttribute.SetVisible<CABankTran.curyApplAmt>(sender, row, isARorAP);
			PXUIFieldAttribute.SetVisible<CABankTran.curyUnappliedBal>(sender, row, isARorAP);
			PXUIFieldAttribute.SetVisible<CABankTran.curyWOAmt>(sender, row, isAR && isReceipt);
			PXUIFieldAttribute.SetVisible<CABankTranAdjustment.curyAdjgWOAmt>(Adjustments.Cache, null, isAR && isReceipt);
			PXUIFieldAttribute.SetVisible<CABankTranAdjustment.writeOffReasonCode>(Adjustments.Cache, null, isAR && isReceipt);
			PXUIFieldAttribute.SetVisible<CABankTran.curyApplAmtCA>(sender, row, isCA);
			PXUIFieldAttribute.SetVisible<CABankTran.curyUnappliedBalCA>(sender, row, isCA);
			PXUIFieldAttribute.SetVisible<CABankTran.userDesc>(sender, row, CreatingDocument);
			PXUIFieldAttribute.SetEnabled<CABankTran.userDesc>(sender, row, CreatingDocument);

			TranSplit.View.AllowSelect = isCA;
			Adjustments.View.AllowSelect = isARorAP;
		}

		protected virtual bool ValidateTranFields(PXCache sender, CABankTran row)
		{
			bool creatingDocument = row.CreateDocument == true;
			bool moduleCA = creatingDocument && row.OrigModule == GL.BatchModule.CA;
			bool moduleAR = creatingDocument && row.OrigModule == GL.BatchModule.AR;
			bool moduleAP = creatingDocument && row.OrigModule == GL.BatchModule.AP;
			bool matchedToInv = row.MatchedToInvoice == true;

			bool missingBAccount = (moduleAP || moduleAR) && row.BAccountID == null;
			bool missingLocation = (moduleAP || moduleAR) && row.LocationID == null;
			bool missingPaymentMethod = (moduleAP || moduleAR) && string.IsNullOrEmpty(row.PaymentMethodID);
			bool missingPaymentMethodInvoiceTab = row.MatchedToInvoice == true && String.IsNullOrEmpty(row.PaymentMethodIDCopy);
			bool missingEntryType = moduleCA && string.IsNullOrEmpty(row.EntryTypeID);
			bool unappliedBalAP = moduleAP && row.DrCr == DrCr.Debit && row.CuryUnappliedBal != null && row.CuryUnappliedBal > 0;
			bool unappliedBalAR = moduleAR && row.DrCr == DrCr.Credit && row.CuryUnappliedBal != null && row.CuryUnappliedBal > 0;
			bool unappliedBalCA = moduleCA && row.CuryUnappliedBalCA != null && row.CuryUnappliedBalCA > 0;
			bool unappliedBalMatch = matchedToInv && row.CuryUnappliedBalMatch != null && row.CuryUnappliedBalMatch != 0;
			bool missingPMInstance = false;
			if (row.PMInstanceID == null && moduleAR)
			{
				PaymentMethod pm = PXSelect<
				    PaymentMethod, 
				    Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>
				    .Select(this, row.PaymentMethodID);
				missingPMInstance = pm != null && pm.IsAccountNumberRequired == true;
			}
			CABankTranAdjustment adj = null;
			string errorMessage = null;
			if (creatingDocument && row.InvoiceInfo != null)
			{
				adj = Adjustments.Search<CABankTranAdjustment.adjdRefNbr, CABankTranAdjustment.adjdModule>(row.InvoiceInfo, row.OrigModule);
				if (adj == null)
				{
					string Module;
					try
					{
						object linkedInvoice = FindInvoiceByInvoiceInfo(row, out Module);
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
					}
				}
			}
			bool missingInvoice = creatingDocument && (row.InvoiceNotFound == true || (adj == null && row.InvoiceInfo != null));
			bool notExactAmount = adj != null && (adj.CuryAdjgAmt != Details.Current.CuryTotalAmt || adj.CuryDocBal != decimal.Zero);

			RaiseOrHideError<CABankTran.entryTypeID>(sender, row, missingEntryType, Messages.EntryTypeIsRequiredToCreateCADocument, PXErrorLevel.RowWarning);
			RaiseOrHideError<CABankTran.curyUnappliedBalCA>(sender, row, unappliedBalCA, Messages.AmountDiscrepancy, PXErrorLevel.RowWarning, row.CuryApplAmtCA, row.CuryTotalAmt);
			RaiseOrHideError<CABankTran.curyUnappliedBalMatch>(sender, row, unappliedBalMatch, Messages.MatchToInvoiceAmountDiscrepancy, PXErrorLevel.RowWarning, row.CuryApplAmtMatch, row.CuryTotalAmt);
			RaiseOrHideError<CABankTran.payeeBAccountID>(sender, row, missingBAccount, Messages.PayeeIsRequiredToCreateDocument, PXErrorLevel.RowWarning);
			RaiseOrHideError<CABankTran.payeeLocationID>(sender, row, missingLocation, Messages.PayeeLocationIsRequiredToCreateDocument, PXErrorLevel.RowWarning);
			RaiseOrHideError<CABankTran.curyUnappliedBal>(sender, row, unappliedBalAP || unappliedBalAR, Messages.DocumentMustByAppliedInFullBeforeItMayBeCreated, PXErrorLevel.RowWarning);
			RaiseOrHideError<CABankTran.paymentMethodID>(sender, row, missingPaymentMethod, Messages.PaymentMethodIsRequiredToCreateDocument, PXErrorLevel.RowWarning);
			RaiseOrHideError<CABankTran.paymentMethodIDCopy>(sender, row, missingPaymentMethodInvoiceTab, Messages.PaymentMethodIsRequiredToCreateDocument, PXErrorLevel.RowWarning);
			RaiseOrHideError<CABankTran.pMInstanceID>(sender, row, missingPMInstance, Messages.PaymentMethodIsRequiredToCreateDocument, PXErrorLevel.RowWarning);
			RaiseOrHideError<CABankTran.curyApplAmt>(sender, row, notExactAmount, Messages.NotExactAmount, PXErrorLevel.Warning);
			RaiseOrHideError<CABankTran.invoiceInfo>(sender, row, missingInvoice, row.InvoiceNotFound == true ? Messages.InvoiceNotFound : errorMessage ?? Messages.ApplicationremovedByUser, PXErrorLevel.Warning, row.InvoiceInfo);


			bool readyToProcess = true;
			sender.RaiseExceptionHandling<CABankTran.documentMatched>(row, row.DocumentMatched, null);
			Dictionary<string, string> errors = PXUIFieldAttribute.GetErrors(sender, row, PXErrorLevel.Error, PXErrorLevel.RowError);
			if (errors.Count != 0)
			{
				readyToProcess = false;
				sender.RaiseExceptionHandling<CABankTran.documentMatched>(row, row.DocumentMatched, new PXSetPropertyException(errors.Values.First(), PXErrorLevel.RowError));
			}
			else
			{
				Dictionary<string, string> rowWarnings = PXUIFieldAttribute.GetErrors(sender, row, PXErrorLevel.RowWarning);
				if (rowWarnings.Count != 0)
				{
					readyToProcess = false;
					sender.RaiseExceptionHandling<CABankTran.documentMatched>(row, row.DocumentMatched, new PXSetPropertyException(rowWarnings.Values.First(), PXErrorLevel.RowWarning));
				}
				else
				{
					Dictionary<string, string> warnings = PXUIFieldAttribute.GetErrors(sender, row, PXErrorLevel.Warning);
					if (warnings.Count != 0)
					{
						sender.RaiseExceptionHandling<CABankTran.documentMatched>(row, row.DocumentMatched, new PXSetPropertyException(warnings.Values.First(), PXErrorLevel.RowWarning));
					}
				}
			}
			return readyToProcess;
		}

		protected virtual void CABankTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CABankTran adj = (CABankTran)e.Row;
			if (adj.CashAccountID == null)
			{
				sender.RaiseExceptionHandling<CABankTran.cashAccountID>(adj, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CABankTran.cashAccountID).Name));
			}
			if (adj.CreateDocument == true)
			{
				adj.Validated = ValidateTranFields(sender, adj);
				Details.Cache.SetValueExt<CABankTran.documentMatched>(adj, adj.Validated);
			}
		}

		protected virtual void CABankTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row == null) return;

			if (TranFilter.Current?.CashAccountID == null)
				return;

			row.Validated = ValidateTranFields(sender, row);
			EnableTranFields(sender, row);

			if (row.MatchedToExisting == null)
			{
				row.MatchedToExisting = TranMatch.Select(row.TranID).Count != 0;
				if (row.MatchedToExisting == true)
				{
					CABankTranMatch match = ((CABankTranMatch)TranMatch.SelectSingle(row.TranID));

					row.MatchedToExpenseReceipt = IsMatchedToExpenseReceipt(match);
					row.MatchedToInvoice = IsMatchedToInvoice(row, match);

					PXFormulaAttribute.CalcAggregate<CABankTranMatch.curyApplAmt>(TranMatch.Cache, e.Row);
				}
			}

			StatementsMatchingProto.SetDocTypeList(Adjustments.Cache, row);

			Dictionary<int, PXSetPropertyException> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<int, PXSetPropertyException>;
			TimeSpan timespan;
			Exception ex;
			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);
			if ((status == PXLongRunStatus.Aborted || status == PXLongRunStatus.Completed) && listMessages != null)
			{
				int key = row.TranID.Value;
				if (listMessages.ContainsKey(key))
				{
					sender.RaiseExceptionHandling<CABankTran.documentMatched>(row, row.DocumentMatched, listMessages[key]);
				}
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXVendorCustomerSelectorAttribute))]
		[PXVendorCustomerWithCreditSelector(typeof(CABankTran.origModule))]
		protected virtual void CABankTran_PayeeBAccountID_CacheAttached(PXCache sender) { }

		private void FieldsDisableOnProcessing(PXLongRunStatus status)
		{
			bool noProcessing = status == PXLongRunStatus.NotExists;
			Details.Cache.AllowUpdate = noProcessing;
			DetailMatchesCA.Cache.AllowUpdate = noProcessing;
			DetailsForPaymentCreation.Cache.AllowUpdate = noProcessing;
			DetailMatchingInvoices.Cache.AllowUpdate = noProcessing;
		    ExpenseReceiptDetailMatches.Cache.AllowUpdate = noProcessing;
			Adjustments.Cache.AllowInsert = noProcessing;
			Adjustments.Cache.AllowUpdate = noProcessing;
			Adjustments.Cache.AllowDelete = noProcessing;

			TranSplit.Cache.AllowInsert = noProcessing;
			TranSplit.Cache.AllowUpdate = noProcessing;
			TranSplit.Cache.AllowDelete = noProcessing;
			autoMatch.SetEnabled(noProcessing);
			processMatched.SetEnabled(noProcessing);
			matchSettingsPanel.SetEnabled(noProcessing);
			uploadFile.SetEnabled(noProcessing);
			clearMatch.SetEnabled(noProcessing);
			clearAllMatches.SetEnabled(noProcessing);
			hide.SetEnabled(noProcessing);
		}

		#endregion
		#region Filter Events
		protected virtual void Filter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Filter row = e.Row as Filter;

			TimeSpan timespan;
			Exception ex;

			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);

			PXUIFieldAttribute.SetEnabled(sender, null, status == PXLongRunStatus.NotExists);

			bool enableDetails = (status == PXLongRunStatus.NotExists) && row != null && row.CashAccountID.HasValue;
			if (!enableDetails)
			{
				PXUIFieldAttribute.SetEnabled(this.Details.Cache, null, false);
			}

            FieldsDisableOnProcessing(status);
			autoMatch.SetEnabled(row.CashAccountID != null);
			processMatched.SetEnabled(row.CashAccountID != null);
			matchSettingsPanel.SetEnabled(row.CashAccountID != null);
		}

        protected virtual void _(Events.FieldUpdated<Filter.cashAccountID> e)
	    {
	        CashAccount account = CashAccount.PK.Find(this, (int?) e.NewValue);

            var row = (Filter) e.Row;

	        row.IsCorpCardCashAccount = account?.UseForCorpCard == true;
	    }
		protected virtual void _(Events.FieldVerifying<Filter.cashAccountID> e)
		{
			if (e == null) return;

			CashAccount cashAccount = CashAccount.PK.Find(this, (int?)e.NewValue);
			if (cashAccount?.Active == false)
			{
				e.Cache.RaiseExceptionHandling<Filter.cashAccountID>(e.Row, e.NewValue,
					new PXSetPropertyException(Messages.CashAccountInactive, PXErrorLevel.RowError, cashAccount.CashAccountCD));
				e.Cancel = true;
			}
		}

		#endregion

		#region CashAccount Events

		protected virtual void _(Events.RowSelected<CashAccount> e)
	    {
	        if (e.Row == null)
	            return;

			bool isCorpCard = TranFilter.Current.IsCorpCardCashAccount == true;

		    PXUIFieldAttribute.SetVisible<CashAccount.curyDiffThreshold>(e.Cache, null, isCorpCard);
		    PXUIFieldAttribute.SetVisible<CashAccount.amountWeight>(e.Cache, null, isCorpCard);
		    PXUIFieldAttribute.SetVisible<CashAccount.ratioInRelevanceCalculationLabel>(e.Cache, null, isCorpCard);
		}

		protected virtual void _(Events.RowUpdated<CashAccount> e)
		{
			if (!e.Cache.ObjectsEqual<
					CashAccount.receiptTranDaysBefore,
					CashAccount.receiptTranDaysAfter,
					CashAccount.disbursementTranDaysBefore,
					CashAccount.disbursementTranDaysAfter,
					CashAccount.allowMatchingCreditMemo,
					CashAccount.refNbrCompareWeight>(e.OldRow, e.Row)
				|| !e.Cache.ObjectsEqual<
				    CashAccount.dateCompareWeight,
					CashAccount.payeeCompareWeight,
					CashAccount.dateMeanOffset,
					CashAccount.dateSigma,
					CashAccount.skipVoided,
					CashAccount.curyDiffThreshold,
					CashAccount.amountWeight,
				    CashAccount.emptyRefNbrMatching>(e.OldRow, e.Row))
			{
				e.Row.MatchSettingsPerAccount = true;
			}
		}

        #endregion
        #region CATranExt Events

        protected virtual void CATranExt_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CATranExt row = e.Row as CATranExt;

			if (row == null)
                return;

			PXUIFieldAttribute.SetVisible<CATranExt.finPeriodID>(sender, null, false);
			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CATranExt.isMatched>(sender, row, true);
		}

		protected virtual void CATranExt_IsMatched_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CATranExt row = e.Row as CATranExt;

			if ((bool?)e.NewValue == true &&
					(Details.Current.DocumentMatched == true || Details.Current.CreateDocument == true))
			{
				throw new PXSetPropertyException(Messages.AnotherOptionChosen, PXErrorLevel.RowWarning);
			}
		}

		protected virtual void CATranExt_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CATranExt row = e.Row as CATranExt;
			CABankTran currentTran = Details.Current;

			if (!sender.ObjectsEqual<CATranExt.isMatched>(e.Row, e.OldRow))
			{
				if (row.IsMatched == true)
				{
					CABankTranMatch match = null;

					if (row.OrigTranType == CATranType.CABatch)
					{
						match = new CABankTranMatch()
						{
							TranID = currentTran.TranID,
							TranType = currentTran.TranType,
							DocModule = BatchModule.AP,
							DocType = CATranType.CABatch,
							DocRefNbr = row.OrigRefNbr,
							ReferenceID = row.ReferenceID
						};
					}
					else
					{
						match = new CABankTranMatch()
						{
							TranID = currentTran.TranID,
							TranType = currentTran.TranType,
							CATranID = row.TranID,
							ReferenceID = row.ReferenceID
						};
					}

					TranMatch.Insert(match);
				}
				else
				{
					foreach (var match in TranMatch.Select(currentTran.TranID))
					{
						TranMatch.Delete(match);
					}
				}

				Details.Cache.SetValueExt<CABankTran.documentMatched>(currentTran, row.IsMatched);
				Details.Cache.SetStatus(Details.Current, PXEntryStatus.Updated);
			}

			sender.IsDirty = false;
		}

		protected virtual void CATranExt_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion
		#region CurrencyInfo
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CashAccount cacct = cashAccount.Select();

				if (cacct != null && !string.IsNullOrEmpty(cacct.CuryID))
				{
					e.NewValue = cacct.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CashAccount cacct = cashAccount.Select();
				if (cacct != null && !string.IsNullOrEmpty(cacct.CuryRateTypeID))
				{
					e.NewValue = cacct.CuryRateTypeID;
					e.Cancel = true;
				}
				else
				{
					CMSetup setup = CMSetup.Current;
					CABankTran det = Details.Current;

					if (setup != null && det != null)
					{
						switch (det.OrigModule)
						{
							case GL.BatchModule.CA:
								e.NewValue = setup.CARateTypeDflt;
								break;
							case GL.BatchModule.AP:
								e.NewValue = setup.APRateTypeDflt;
								break;
							case GL.BatchModule.AR:
								e.NewValue = setup.ARRateTypeDflt;
								break;
						}

						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Details.Current != null)
			{
				e.NewValue = Details.Current.TranDate;
				e.Cancel = true;
			}
		}

		#endregion

		#region CABankTranDetail Events
		protected virtual void CABankTranDetail_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTranDetail split = e.Row as CABankTranDetail;
			CABankTran tran = Details.Current;

			if (tran == null || tran.EntryTypeID == null || split == null)
                return;

			e.NewValue = GetDefaultAccountValues(this, tran.CashAccountID, tran.EntryTypeID).AccountID;
			e.Cancel = e.NewValue != null;
		}

		protected virtual void CABankTranDetail_AccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CATranDetailHelper.OnAccountIdFieldUpdatedEvent(cache, e);
		}

		protected virtual void CABankTranDetail_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTran adj = Details.Current;
			CABankTranDetail split = e.Row as CABankTranDetail;

			if (adj == null || adj.EntryTypeID == null || split == null)
				return;

			e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).BranchID;
			e.Cancel = e.NewValue != null;
		}

		protected virtual void CABankTranDetail_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
            CATranDetailHelper.OnCashAccountIdFieldDefaultingEvent(sender, e);
		}

		protected virtual void CABankTranDetail_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CATranDetailHelper.OnCashAccountIdFieldVerifyingEvent(sender, e, Details.Current.CashAccountID);
		}

		protected virtual void CABankTranDetail_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            CATranDetailHelper.OnCashAccountIdFieldUpdatedEvent(sender, e);
		}


		protected virtual void CABankTranDetail_InventoryId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankTranDetail split = e.Row as CABankTranDetail;
			CABankTran adj = Details.Current;

			if (split != null && split.InventoryID != null)
			{
				InventoryItem invItem = PXSelect<
				    InventoryItem,
				    Where<InventoryItem.inventoryID, Equal<Required<CABankTranDetail.inventoryID>>>>
				    .Select(this, split.InventoryID);

				if (invItem != null && adj != null)
				{
					if (adj.DrCr == CADrCr.CADebit)
					{
						split.AccountID = invItem.SalesAcctID;
						split.SubID = invItem.SalesSubID;
					}
					else
					{
						split.AccountID = invItem.COGSAcctID;
						split.SubID = invItem.COGSSubID;
					}
				}

				sender.SetDefaultExt<CABankTranDetail.taxCategoryID>(split);
			}
		}

		protected virtual void CABankTranDetail_Qty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTranDetail split = e.Row as CABankTranDetail;
			e.NewValue = 1.0m;
		}

		protected virtual void CABankTranDetail_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTranDetail split = e.Row as CABankTranDetail;
			CABankTran adj = Details.Current;

			if (adj == null || adj.EntryTypeID == null || split == null)
                return;

			e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).SubID;
			e.Cancel = e.NewValue != null;
		}

		protected virtual void CABankTranDetail_TranDesc_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTranDetail split = e.Row as CABankTranDetail;

			CABankTran tran = Details.Current;

			if (tran != null && tran.EntryTypeID != null)
			{
				CAEntryType entryType = PXSelect<
				    CAEntryType,
				    Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>
				    .Select(this, tran.EntryTypeID);

				if (entryType != null)
				{
					e.NewValue = entryType.Descr;
				}
			}
		}


		protected virtual void CABankTranDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CABankTran tran = Details.Current;
			CABankTranDetail newSplit = e.Row as CABankTranDetail;
			CurrencyInfoAttribute.SetDefaults<CABankTranDetail.curyInfoID>(sender, newSplit);
			newSplit.Qty = 1.0m;
			newSplit.CuryUnitPrice = tran.CuryUnappliedBalCA > 0 ? tran.CuryUnappliedBalCA : 0;
			sender.SetValueExt<CABankTranDetail.curyTranAmt>(newSplit, newSplit.Qty * newSplit.CuryUnitPrice);
			newSplit.TranDesc = tran.UserDesc;
		}

		protected virtual void CABankTranDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e) => CATranDetailHelper.OnCATranDetailRowUpdatingEvent(sender, e);


		protected virtual void CABankTranDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABankTranDetail row = (CABankTranDetail)e.Row;

			if (row == null)
				return;

			
			CABankTran tranRow = PXSelect<CABankTran, Where<CABankTran.tranID, Equal<Required<CABankTranAdjustment.tranID>>>>.Select(this, row.BankTranID);
			var accountFieldState = sender.GetStateExt<CABankTranDetail.accountID>(row) as PXFieldState;			   

			if (row.AccountID == null)
			{
				Details.Cache.RaiseExceptionHandling<CABankTran.createDocument>(tranRow, tranRow.CreateDocument, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowWarning, PXUIFieldAttribute.GetDisplayName<CABankTranDetail.accountID>(sender)));
				sender.RaiseExceptionHandling<CABankTranDetail.accountID>(row, row.AccountID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowWarning, PXUIFieldAttribute.GetDisplayName<CABankTranDetail.accountID>(sender)));
			}
			else if (accountFieldState == null || accountFieldState.ErrorLevel < PXErrorLevel.Warning)
			{
				sender.RaiseExceptionHandling<CABankTranDetail.accountID>(row, row.AccountID, null);

				if (row.SubID == null)
				{
					Details.Cache.RaiseExceptionHandling<CABankTran.createDocument>(tranRow, tranRow.CreateDocument, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowWarning, PXUIFieldAttribute.GetDisplayName<CABankTranDetail.subID>(sender)));
					sender.RaiseExceptionHandling<CABankTranDetail.subID>(row, row.SubID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowWarning, PXUIFieldAttribute.GetDisplayName<CABankTranDetail.subID>(sender)));
				}
				else
				{
					sender.RaiseExceptionHandling<CABankTranDetail.subID>(row, row.SubID, null);
					Details.Cache.RaiseExceptionHandling<CABankTran.createDocument>(tranRow, tranRow.CreateDocument, null);
				}
			}
		}

		private CABankTranDetail GetDefaultAccountValues(PXGraph graph, int? cashAccountID, string entryTypeID)
		{
			return CATranDetailHelper.CreateCATransactionDetailWithDefaultAccountValues<CABankTranDetail>(graph, cashAccountID, entryTypeID);
		}

		public virtual void updateAmountPrice(CABankTranDetail oldSplit, CABankTranDetail newSplit)
		{
			CATranDetailHelper.UpdateNewTranDetailCuryTranAmtOrCuryUnitPrice(TranSplit.Cache, oldSplit, newSplit);
			}

		#endregion

		#region CABankTranMatch
		protected virtual void CABankTranMatch_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			CABankTranMatch row = e.Row as CABankTranMatch;
			CABankTran currtran = PXSelect<
			    CABankTran, 
			    Where<CABankTran.tranID, Equal<Required<CABankTran.tranID>>>>
			    .Select(this, row.TranID);
			Details.Cache.SetValue<CABankTran.matchedToExisting>(currtran, null);
			Details.Cache.SetValue<CABankTran.matchedToInvoice>(currtran, null);
		    Details.Cache.SetValue<CABankTran.matchedToExpenseReceipt>(currtran, null);
        }

		protected virtual void CABankTranMatch_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			CABankTranMatch row = e.Row as CABankTranMatch;
			CABankTran currtran = PXSelect<
			    CABankTran, 
			    Where<CABankTran.tranID, Equal<Required<CABankTran.tranID>>>>
			    .Select(this, row.TranID);
			Details.Cache.SetValue<CABankTran.matchedToExisting>(currtran, null);
			Details.Cache.SetValue<CABankTran.matchedToInvoice>(currtran, null);
		    Details.Cache.SetValue<CABankTran.matchedToExpenseReceipt>(currtran, null);
		}

		protected virtual void CABankTranMatch_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Insert)
			{
				return;
			}

			var row = (CABankTranMatch)e.Row;

			bool isAlreadyMatched = PXSelectReadonly2<
			    CABankTranMatch, 
			    InnerJoin<CABankTran, 
			        On<CABankTran.tranID, Equal<CABankTranMatch.tranID>>>, 
			    Where<CABankTranMatch.cATranID, Equal<Required<CABankTranMatch.cATranID>>, 
			        And<CABankTran.tranType, Equal<Current<CABankTran.tranType>>>>>
			    .Select(this, row.CATranID)
			    .AsEnumerable()
			    .Any(bankTran => Caches["CABankTranMatch"].GetStatus((CABankTranMatch)bankTran) != PXEntryStatus.Deleted);

			if (isAlreadyMatched)
			{
				CABankTran tranRow = PXSelect<
				    CABankTran, 
				    Where<CABankTran.tranID, Equal<Required<CABankTranMatch.tranID>>>>
				    .Select(this, row.TranID);

				Details.Cache.RaiseExceptionHandling<CABankTran.extRefNbr>(tranRow, tranRow.ExtRefNbr, 
					new PXSetPropertyException(Messages.DocumentIsAlreadyMatched, PXErrorLevel.RowError));
			}
		}
		#endregion

		#endregion
		#region Processing
		public static void DoProcessing(IEnumerable<CABankTran> list, Dictionary<int, PXSetPropertyException> listMessages)
		{
			CABankTransactionsMaint graph = PXGraph.CreateInstance<CABankTransactionsMaint>();
			bool hasErrors = false;
			List<Batch> toPost = new List<Batch>();
			foreach (CABankTran iDet in list)
			{
				graph.Clear();
				if (iDet.DocumentMatched == false) continue;
				CABankTran det = (CABankTran)graph.Details.Cache.CreateCopy(iDet);
				graph.Details.Current = det;
				graph.DetailsForPaymentCreation.Current = det;
				graph.TranFilter.Current = new Filter()
				{
					CashAccountID = det.CashAccountID,
					IsCorpCardCashAccount = CashAccount.PK.Find(graph, det.CashAccountID).UseForCorpCard,
					TranType = det.TranType
				};
				try
				{
					//assuming that all matches are of one type
					CABankTranMatch match = graph.TranMatch.SelectSingle(det.TranID);
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						if (match != null && IsMatchedToExpenseReceipt(match))
						{
							graph.MatchExpenseReceipt(iDet, match);
						}
						else
						{
							if (match != null && !String.IsNullOrEmpty(match.DocRefNbr) && match.DocType != CATranType.CABatch)
							{
								det = graph.MatchInvoices(det);
							}
							if (det.CreateDocument == true)
							{
								graph.CreateDocumentProc(det, false);
							}
							graph.MatchCATran(det, toPost);
						}

						det.Processed = true;
						det.DocumentMatched = true;
						det.RuleID = iDet.RuleID;
						graph.Details.Update(det);
						graph.Save.Press();
						ts.Complete(graph);
					}
					listMessages[det.TranID.Value] = new PXSetPropertyException(Messages.DeatsilProcess, PXErrorLevel.RowInfo);
				}
				catch (PXOuterException e)
				{
					listMessages[det.TranID.Value] = new PXSetPropertyException(e, PXErrorLevel.RowError, e.Message + " " + e.InnerMessages[0]);
					hasErrors = true;
				}
				catch (Exception e)
				{
					listMessages[det.TranID.Value] = new PXSetPropertyException(e, PXErrorLevel.RowError, e.Message);
					hasErrors = true;
				}
			}
			List<Batch> postFailedList = new List<Batch>();
			if (toPost.Count > 0)
			{
				PostGraph pg = PXGraph.CreateInstance<PostGraph>();
				foreach (Batch iBatch in toPost)
				{
					try
					{
						//if (rg.AutoPost)
						{
							pg.Clear();
							pg.PostBatchProc(iBatch);
						}
					}
					catch (Exception)
					{
						postFailedList.Add(iBatch);
					}
				}
			}
			if (postFailedList.Count > 0)
			{
				throw new PXException(GL.Messages.PostingOfSomeOfTheIncludedDocumentsFailed, postFailedList.Count, toPost.Count);
			}
			if (hasErrors)
			{
				throw new PXException(Messages.ErrorsInProcessing);
			}
		}
		public static void DoProcessing(IEnumerable<CABankTran> list)
		{
			Dictionary<int, PXSetPropertyException> listMessages = new Dictionary<int, PXSetPropertyException>();
			PXLongOperation.SetCustomInfo(listMessages, list.Cast<object>().ToArray());
			DoProcessing(list, listMessages);
		}

		protected virtual void VerifyBeforeMatchInvoices(CABankTran det)
		{
			if ((det.CuryUnappliedBalMatch ?? 0) != 0)
			{
				throw new PXSetPropertyException(Messages.MatchToInvoiceAmountDiscrepancy, det.CuryApplAmtMatch, det.CuryTotalAmt);
			}
		}

		protected virtual CABankTran MatchInvoices(CABankTran det)
		{
			VerifyBeforeMatchInvoices(det);

			var matches = TranMatch.Select(det.TranID).RowCast<CABankTranMatch>().ToList();

			var BAccountID = det.BAccountID;
			var LocationID = det.LocationID;
			var OrigModule = det.OrigModule;
			var PaymentMethodID = det.PaymentMethodID;

			ClearMatchProc(det);
			Details.Cache.SetValue<CABankTran.createDocument>(det, true);
			det = Details.Update(det);

			ClearRule(Details.Cache, det);
			foreach (CABankTranAdjustment adj in Adjustments.Select(det.TranID))
			{
				Adjustments.Delete(adj);
			}

			Details.Cache.SetValue<CABankTran.origModule>(det, OrigModule);
			Details.Cache.SetValue<CABankTran.payeeBAccountID>(det, BAccountID);
			Details.Cache.SetValue<CABankTran.payeeLocationID>(det, LocationID);
			Details.Cache.SetValue<CABankTran.paymentMethodID>(det, PaymentMethodID);
			det = Details.Update(det);

			foreach (CABankTranMatch match in matches)
			{
				try
				{
					CABankTranAdjustment adj = new CABankTranAdjustment()
					{
						TranID = det.TranID
					};

					adj = Adjustments.Insert(adj);

					adj.AdjdDocType = match.DocType;
					adj.AdjdRefNbr = match.DocRefNbr;
					adj.AdjdModule = match.DocModule;
					adj.CuryAdjgAmt = match.CuryApplAmt;

					Adjustments.Update(adj);
				}
				catch
				{
					throw new PXSetPropertyException(Messages.CouldNotAddApplication, match.DocRefNbr);
				}
			}

			return det;
		}

		protected virtual void MatchCATran(CABankTran det, List<Batch> externalPostList)
		{
			//1-n relations are not supported currently
			CABankTranMatch match = TranMatch.Select(det.TranID);
			if (match != null)
			{
				if (match.DocModule == BatchModule.AP && match.DocType == CATranType.CABatch)
				{
					bool cleared = true;
					foreach (CATranExt tran in PXSelectJoin<
					    CATranExt, 
					    InnerJoin<CABatchDetail,
					        On<CATranExt.origTranType, Equal<CABatchDetail.origDocType>,
					        And<CATranExt.origRefNbr, Equal<CABatchDetail.origRefNbr>,
					        And<CATranExt.origModule, Equal<CABatchDetail.origModule>>>>>,
					    Where<CABatchDetail.batchNbr, Equal<Required<CABatchDetail.batchNbr>>>>
					    .Select(this, match.DocRefNbr))
					{
						if (ProcessCATran(det, externalPostList, tran.TranID, false) != true)
						{
							cleared = false;
						}
					}

					if (cleared == true)
					{
						PXDatabase.Update<CABatch>(new PXDataFieldAssign<CABatch.cleared>(true),
													new PXDataFieldAssign<CABatch.clearDate>(det.TranDate),
													   new PXDataFieldRestrict<CABatch.batchNbr>(PXDbType.VarChar, 15, match.DocRefNbr, PXComp.EQ));
					}
					CABatch batch = PXSelectReadonly<
					    CABatch, 
					    Where<CABatch.batchNbr, Equal<Required<CABatch.batchNbr>>>>
					    .Select(this, match.DocRefNbr);
					if (batch.Released != true)
					{
						CABatchEntry batchEntryGraph = PXGraph.CreateInstance<CABatchEntry>();
						batchEntryGraph.Document.Current = batch;
						batchEntryGraph.SelectTimeStamp();
						batchEntryGraph.Release.Press();
					}
				}
				else
				{
					ProcessCATran(det, externalPostList, match.CATranID);
				}
			}
			else
			{
				throw new PXException(Messages.MatchNotFound, det.TranID);
			}
		}

		private bool ProcessCATran(CABankTran det, List<Batch> externalPostList, Int64? tranID, bool checkAmt = true)
		{
			Func<CATranExt> getCATran = () => (CATranExt)PXSelectReadonly<
			    CATranExt, 
			    Where<CATranExt.tranID, Equal<Required<CATranExt.tranID>>>>
			    .Select(this, tranID);
			CATranExt tran = getCATran();
			if (tran != null)
			{
				if (tran.CuryTranAmt != det.CuryTranAmt && checkAmt)
				{
					throw new PXException(Messages.AmountDiscrepancy, tran.CuryTranAmt, det.CuryTranAmt);
				}
				if (tran.Released != true)
				{
					PXGraph searchGraph = null;
					//errors when we cannot release?
					switch (tran.OrigModule)
					{
						case GL.BatchModule.AP:
							if (CASetup.Current.ReleaseAP == true)
							{
								CATrxRelease.ReleaseCATran(tran, ref searchGraph, externalPostList);
							}
							break;
						case GL.BatchModule.AR:
							if (CASetup.Current.ReleaseAR == true)
							{
								CATrxRelease.ReleaseCATran(tran, ref searchGraph, externalPostList);
							}
							break;
						case GL.BatchModule.CA:
							CATrxRelease.ReleaseCATran(tran, ref searchGraph, externalPostList);
							break;
						default:
							throw new Exception(Messages.ThisDocTypeNotAvailableForRelease);
					}
				}
				Caches[typeof(CATranExt)].ClearQueryCache();
				tran = getCATran();
				if (tran.Released == true && tran.Cleared == false)
				{
					this.SelectTimeStamp();
                    StatementsMatchingProto.UpdateSourceDoc(this, tran, det.TranDate);
                }
				return tran.Cleared ?? false;
			}
			else
			{
				throw new PXException(Messages.CATranNotFound);
			}
		}

		public void MatchExpenseReceipt(CABankTran bankTran, CABankTranMatch match)
		{
			EPExpenseClaimDetails receipt = PXSelect<EPExpenseClaimDetails,
													Where<EPExpenseClaimDetails.claimDetailCD, Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>
													.Select(this, match.DocRefNbr);
			this.SelectTimeStamp();

			//concurrency matching and release
			ExpenseReceipts.Update(receipt);

			if (receipt.Released != true)
				return;

			CATran caTran = null;

			if (receipt.PaidWith == EPExpenseClaimDetails.paidWith.CardCompanyExpense)
			{
				caTran = PXSelect<CATran,
							Where<CATran.origModule, Equal<BatchModule.moduleAP>,
								And<CATran.origTranType, Equal<Required<CATran.origTranType>>,
								And<CATran.origRefNbr, Equal<Required<CATran.origRefNbr>>>>>>
							.Select(this, receipt.APDocType, receipt.APRefNbr);
			}
			else if (receipt.PaidWith == EPExpenseClaimDetails.paidWith.CardPersonalExpense)
			{
				caTran = 
					PXSelectJoin<CATran,
						InnerJoin<GLTran,
							On<CATran.tranID, Equal<GLTran.cATranID>>>,
						Where<GLTran.module, Equal<BatchModule.moduleAP>,
								And<GLTran.tranType, Equal<Required<GLTran.tranType>>,
								And<GLTran.refNbr, Equal<Required<GLTran.refNbr>>,
								And<GLTran.tranLineNbr, Equal<Required<GLTran.tranLineNbr>>>>>>>
						.Select(this, receipt.APDocType, receipt.APRefNbr, receipt.APLineNbr);
			}
			else
			{
				throw new InvalidOperationException();
			}

			if (caTran == null)
				return;

			if (caTran.Released == true && caTran.Cleared == false)
			{
				if (receipt.PaidWith == EPExpenseClaimDetails.paidWith.CardCompanyExpense)
				{
					StatementsMatchingProto.UpdateSourceDoc(this, caTran, bankTran.TranDate);
				}
				else
				{
					caTran.Cleared = true;
					caTran.ClearDate = bankTran.TranDate ?? caTran.TranDate;

					Caches[typeof(CATran)].Update(caTran);
				}
			}

			match.CATranID = caTran.TranID;

			TranMatch.Update(match);
		}

		protected virtual void ValidateDataForDocumentCreation(CABankTran aRow)
		{
			if (TranMatch.Select(aRow.TranID).Count != 0)
			{
				throw new PXSetPropertyException(Messages.DocumentIsAlreadyCreatedForThisDetail);
			}
			if (aRow.BAccountID == null && aRow.OrigModule != GL.BatchModule.CA)
			{
				throw new PXSetPropertyException(Messages.PayeeIsRequiredToCreateDocument);
			}
			if (aRow.OrigModule == GL.BatchModule.CA && aRow.EntryTypeID == null)
			{
				throw new PXRowPersistingException(typeof(CABankTranDetail).Name, null, Messages.EntryTypeIsRequiredToCreateCADocument);
			}
			if (aRow.LocationID == null && aRow.OrigModule != GL.BatchModule.CA)
			{
				throw new PXSetPropertyException(Messages.PayeeLocationIsRequiredToCreateDocument);
			}

			if (aRow.OrigModule == GL.BatchModule.AR)
			{
				if (string.IsNullOrEmpty(aRow.PaymentMethodID))
				{
					throw new PXSetPropertyException(Messages.PaymentMethodIsRequiredToCreateDocument);
				}
				if (aRow.PMInstanceID == null)
				{
					PaymentMethod pm = PXSelect<
					    PaymentMethod, 
					    Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>
					    .Select(this, aRow.PaymentMethodID);
					if (pm != null && pm.IsAccountNumberRequired == true)
					{
						throw new PXSetPropertyException(Messages.PaymentMethodIsRequiredToCreateDocument);
					}
				}
			}
			if (aRow.OrigModule == GL.BatchModule.AP && string.IsNullOrEmpty(aRow.PaymentMethodID))
			{
				throw new PXSetPropertyException(Messages.PaymentMethodIsRequiredToCreateDocument);
			}
		}

		protected virtual object FindInvoiceByInvoiceInfo(CABankTran aRow, out string Module)
		{
			object res = null;
			Module = String.Empty;
			//ignore cases when there are more than 1 invoice found, for now
			//
			if (aRow.DrCr == CA.CADrCr.CACredit)
			{
				PXResult<APInvoice, APAdjust, APPayment> invResultAP = FindAPInvoiceByInvoiceInfo(aRow);
				if (invResultAP != null)
				{
					APInvoice invoiceToApply = invResultAP;
					APAdjust unreleasedAjustment = invResultAP;
					APPayment invoicePrepayment = invResultAP;
					if (invoiceToApply.Released == false && invoiceToApply.Prebooked == false)
					{
						throw new PXSetPropertyException(Messages.APPaymentApplicationInvoiceIsNotReleased, aRow.InvoiceInfo);
					}

					if (invoiceToApply.OpenDoc == false)
					{
						throw new PXSetPropertyException(Messages.APPaymentApplicationInvoiceIsClosed, aRow.InvoiceInfo);
					}

					if (APSetup.Current.EarlyChecks == false && (invoiceToApply.DocDate > aRow.TranDate))
					{
						throw new PXSetPropertyException(Messages.APPaymentApplicationInvoiceDateIsGreaterThenPaymentDate, aRow.InvoiceInfo);
					}

					if (unreleasedAjustment != null && string.IsNullOrEmpty(unreleasedAjustment.AdjgRefNbr) == false)
					{
						throw new PXSetPropertyException(Messages.APPaymentApplicationInvoiceUnrealeasedApplicationExist, aRow.InvoiceInfo);
					}

					if (aRow.DrCr == CADrCr.CACredit && invoicePrepayment != null && string.IsNullOrEmpty(invoicePrepayment.RefNbr) == false)
					{
						throw new PXSetPropertyException(Messages.APPaymentApplicationInvoiceIsPartOfPrepaymentOrDebitAdjustment, aRow.InvoiceInfo);
					}

					res = invoiceToApply;

					Module = GL.BatchModule.AP;
				}
			}

			if (aRow.DrCr == CA.CADrCr.CADebit)
			{
				PXResult<ARInvoice, ARAdjust> invResultAR = FindARInvoiceByInvoiceInfo(aRow);
				if (invResultAR != null)
				{
					ARInvoice invoiceToApply = invResultAR;
					ARAdjust unreleasedAjustment = invResultAR;
					if (invoiceToApply.Released == false)
					{
						throw new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceIsNotReleased, aRow.InvoiceInfo);
					}

					if (invoiceToApply.OpenDoc == false)
					{
						throw new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceIsClosed, aRow.InvoiceInfo);
					}

					if (invoiceToApply.DocDate > aRow.TranDate)
					{
						throw new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceDateIsGreaterThenPaymentDate, aRow.InvoiceInfo);
					}

					if (unreleasedAjustment != null && string.IsNullOrEmpty(unreleasedAjustment.AdjgRefNbr) == false)
					{
						throw new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceUnrealeasedApplicationExist, aRow.InvoiceInfo);
					}

					res = invoiceToApply;

					Module = GL.BatchModule.AR;
				}
			}

			return res;
		}

		protected virtual void CreateDocumentProc(CABankTran aRow, bool doPersist)
		{
			CATran result = null;
			PXCache sender = this.Details.Cache;
			ValidateDataForDocumentCreation(aRow);
			CurrencyInfo curyInfo = PXSelect<
			    CurrencyInfo, 
			    Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
			    .Select(this, aRow.CuryInfoID);

			if (aRow.OrigModule == GL.BatchModule.AR)
			{
				List<ARAdjust> adjustmentsAR = new List<ARAdjust>();

				foreach (CABankTranAdjustment adj in Adjustments.Select(aRow.TranID))
				{
					ARAdjust adjAR = new ARAdjust();
					adjAR.AdjdRefNbr = adj.AdjdRefNbr;
					adjAR.AdjdDocType = adj.AdjdDocType;
					adjAR.CuryAdjgAmt = adj.CuryAdjgAmt;
					adjAR.CuryAdjgDiscAmt = adj.CuryAdjgDiscAmt;
					adjAR.CuryAdjgWOAmt = adj.CuryAdjgWhTaxAmt;
					adjAR.AdjdCuryRate = adj.AdjdCuryRate;
					adjAR.WOBal = adj.WhTaxBal;
					adjAR.AdjWOAmt = adj.AdjWhTaxAmt;
					adjAR.CuryAdjdWOAmt = adj.CuryAdjdWhTaxAmt;
					adjAR.CuryAdjgWOAmt = adj.CuryAdjgWhTaxAmt;
					adjAR.CuryWOBal = adj.CuryWhTaxBal;
					adjAR.WriteOffReasonCode = adj.WriteOffReasonCode;
					adjustmentsAR.Add(adjAR);
				}
				bool OnHold = (this.CASetup.Current.ReleaseAR == false);
				result = AddARTransaction(aRow, curyInfo, adjustmentsAR, OnHold);
			}

			if (aRow.OrigModule == GL.BatchModule.AP)
			{
				List<ICADocAdjust> adjustments = new List<ICADocAdjust>();

				foreach (CABankTranAdjustment adj in Adjustments.Select(aRow.TranID))
				{
					adjustments.Add(adj);
				}
				bool OnHold = (this.CASetup.Current.ReleaseAP == false);
				result = AddAPTransaction(aRow, curyInfo, adjustments, OnHold);
			}

			if (aRow.OrigModule == GL.BatchModule.CA)
			{
				List<CASplit> splits = new List<CASplit>();
				foreach (PXResult<CABankTranDetail> res in TranSplit.Select(aRow.TranID, aRow.TranType))
				{
					CABankTranDetail det = (CABankTranDetail)res;
					CASplit split = new CASplit();
					split.AccountID = det.AccountID;
					split.BranchID = det.BranchID;
					split.CashAccountID = det.CashAccountID;
					split.CuryTranAmt = det.CuryTranAmt;
					split.CuryUnitPrice = det.CuryUnitPrice;
					split.InventoryID = det.InventoryID;
					split.NonBillable = det.NonBillable;
					split.NoteID = det.NoteID;
					split.ProjectID = det.ProjectID;
					split.Qty = det.Qty;
					split.SubID = det.SubID;
					split.TaskID = det.TaskID;
					split.CostCodeID = det.CostCodeID;
					split.TranDesc = det.TranDesc;
					splits.Add(split);
				}
				if (splits.Count > 0)
				{
					result = AddCATransaction(aRow, curyInfo, splits, false);
				}
				else
				{
					throw new PXRowPersistingException(typeof(CABankTranDetail).Name, null, Messages.UnableToProcessWithoutDetails);
				}
			}

			if (result != null)
			{
				CABankTranMatch match = new CABankTranMatch()
				{
					TranID = aRow.TranID,
					TranType = aRow.TranType,
					CATranID = result.TranID
				};
				TranMatch.Insert(match);
				aRow.CreateDocument = false;
				sender.Update(aRow);
			}

			if (doPersist)
				this.Save.Press();
		}
		protected virtual CATran AddARTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo, IEnumerable<ICADocAdjust> aAdjustments, bool aOnHold)
		{
			PaymentReclassifyProcess.CheckARTransaction(parameters);
			return PaymentReclassifyProcess.AddARTransaction(parameters, aCuryInfo, aAdjustments, aOnHold);
		}
		protected virtual CATran AddARTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo, IEnumerable<ARAdjust> aAdjustments, bool aOnHold)
		{
			PaymentReclassifyProcess.CheckARTransaction(parameters);
			return PaymentReclassifyProcess.AddARTransaction(parameters, aCuryInfo, aAdjustments, aOnHold);
		}

		protected virtual CATran AddAPTransaction(ICADocSource parameters, CurrencyInfo aCuryInfo, IList<ICADocAdjust> aAdjustments, bool aOnHold)
		{
			PaymentReclassifyProcess.CheckAPTransaction(parameters);
			return PaymentReclassifyProcess.AddAPTransaction(parameters, aCuryInfo, aAdjustments, aOnHold);
		}

		protected virtual CATran AddCATransaction(ICADocSource parameters, CurrencyInfo aCuryInfo, IEnumerable<CASplit> splits, bool IsTransferExpense)
		{
			CheckCATransaction(parameters, CASetup.Current);
			return AddCATransaction(this, parameters, aCuryInfo, splits, IsTransferExpense);
		}

		protected virtual bool IsARInvoiceSearchNeeded(CABankTran aRow)
		{
			return (aRow.OrigModule == GL.BatchModule.AR && String.IsNullOrEmpty(aRow.InvoiceInfo) == false);
		}

		protected virtual bool IsAPInvoiceSearchNeeded(CABankTran aRow)
		{
			return (aRow.OrigModule == GL.BatchModule.AP && String.IsNullOrEmpty(aRow.InvoiceInfo) == false);
		}

		/// <summary>
		/// Searches in database AR invoices, based on the the information in CABankTran record.
		/// The field used for the search are  - BAccountID and InvoiceInfo. First it is searching a invoice by it RefNbr, 
		/// then (if not found) - by invoiceNbr. 
		/// </summary>
		/// <param name="aRow">parameters for the search. The field used for the search are  - BAccountID and InvoiceInfo.</param>
		///	<returns>Returns null if nothing is found and PXResult<ARInvoice,ARAdjust> in the case of success.
		///		ARAdjust record represents unreleased adjustment (payment), applied to this Invoice
		///	</returns>
		protected virtual PXResult<ARInvoice, ARAdjust> FindARInvoiceByInvoiceInfo(CABankTran aRow)
		{
			PXResult<ARInvoice, ARAdjust> invResult = (PXResult<ARInvoice, ARAdjust>)PXSelectJoin<
				ARInvoice,
				LeftJoin<ARAdjust, 
					On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
					And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
					And<ARAdjust.released, Equal<boolFalse>>>>>,
				Where<ARInvoice.docType, Equal<AR.ARInvoiceType.invoice>,
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>,
					And<ARInvoice.paymentsByLinesAllowed, NotEqual<True>>>>>
				.Select(this, aRow.InvoiceInfo);

			if (invResult == null)
			{
				invResult = (PXResult<ARInvoice, ARAdjust>)PXSelectJoin<
					ARInvoice,
					LeftJoin<ARAdjust, 
						On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
						And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
						And<ARAdjust.released, Equal<boolFalse>>>>>,
					Where<ARInvoice.docType, Equal<AR.ARInvoiceType.invoice>,
						And<ARInvoice.invoiceNbr, Equal<Required<ARInvoice.invoiceNbr>>,
						And<ARInvoice.paymentsByLinesAllowed, NotEqual<True>>>>>
					.Select(this, aRow.InvoiceInfo);
			}
			return invResult;
		}

		/// <summary>
		/// Searches in database AR invoices, based on the the information in the CABankTran record.
		/// The field used for the search are  - BAccountID and InvoiceInfo. First it is searching a invoice by it RefNbr, 
		/// then (if not found) - by invoiceNbr. 
		/// </summary>
		/// <param name="aRow">Parameters for the search. The field used for the search are  - BAccountID and InvoiceInfo.</param>
		/// <returns>Returns null if nothing is found and PXResult<APInvoice,APAdjust,APPayment> in the case of success.
		/// APAdjust record represents unreleased adjustment (payment), applied to this APInvoice</returns>
		protected virtual PXResult<APInvoice, APAdjust, APPayment> FindAPInvoiceByInvoiceInfo(CABankTran aRow)
		{

			PXResult<APInvoice, APAdjust, APPayment> invResult = (PXResult<APInvoice, APAdjust, APPayment>)PXSelectJoin<
			    APInvoice,
			    LeftJoin<APAdjust, 
			        On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
			        And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
			        And<APAdjust.released, Equal<boolFalse>>>>,
			    LeftJoin<APPayment, 
			        On<APPayment.docType, Equal<APInvoice.docType>,
			        And<APPayment.refNbr, Equal<APInvoice.refNbr>,
			        And<Where<APPayment.docType, Equal<APDocType.prepayment>,
			            Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>,
			    Where<APInvoice.docType, Equal<AP.APInvoiceType.invoice>,
			        And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>,
			        And<APInvoice.paymentsByLinesAllowed, NotEqual<True>>>>>
			    .Select(this, aRow.InvoiceInfo);


			if (invResult == null)
			{
				invResult = (PXResult<APInvoice, APAdjust, APPayment>)PXSelectJoin<
				    APInvoice,
				    LeftJoin<APAdjust, 
				        On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
				        And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
				        And<APAdjust.released, Equal<boolFalse>>>>,
				    LeftJoin<APPayment, 
				        On<APPayment.docType, Equal<APInvoice.docType>,
				        And<APPayment.refNbr, Equal<APInvoice.refNbr>,
				        And<Where<APPayment.docType, Equal<APDocType.prepayment>,
				            Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>,
				    Where<APInvoice.docType, Equal<AP.APInvoiceType.invoice>,
				        And<APInvoice.invoiceNbr, Equal<Required<APInvoice.invoiceNbr>>,
				        And<APInvoice.paymentsByLinesAllowed, NotEqual<True>>>>>
				    .Select(this, aRow.InvoiceInfo);

			}
			return invResult;
		}

        #endregion
        #region MatchingMethods
        public virtual void DoMatch()
        {
            DoMatch(null);
        }

        public virtual void DoMatch(Dictionary<CABankTran, string> errorMatch)
		{
			PXCache cache = this.Details.Cache;
			IEnumerable<CABankTran> tranList = UnMatchedDetails.Select().RowCast<CABankTran>();
			DoMatch(cache, tranList, errorMatch);
		}
        public virtual void DoMatch(PXCache aUpdateCache, IEnumerable<CABankTran> aRows)
        {
            DoMatch(aUpdateCache, aRows, null);
        }

        public virtual void DoMatch(PXCache aUpdateCache, IEnumerable<CABankTran> aRows, Dictionary<CABankTran, string> errorMatch)
		{
			this.ClearCachedMatches();

			DoCAMatch(aUpdateCache, aRows);
			DoInvMatch(aUpdateCache, aRows);
			DoExpenseReceiptMatch(aUpdateCache, aRows);

			foreach (CABankTran det in aRows)
			{
				if (det.DocumentMatched != true && det.CreateDocument != true)
				{
					if (det.CountMatches.HasValue && det.CountMatches == 0 
						&& det.CountInvoiceMatches.HasValue && det.CountInvoiceMatches == 0
					    && (det.CountExpenseReceiptDetailMatches == null || det.CountExpenseReceiptDetailMatches == 0))
					{
						det.CreateDocument = true;

						aUpdateCache.Update(det);

						errorMatch?.Add(det, Messages.NoMatchingDocFound);
					}
					else
					{
						errorMatch?.Add(det, Messages.NoRelevantDocFound);
					}
				}
			}

			this.ClearCachedMatches();
		}

		protected virtual List<CABankTran> DoCAMatchInner(PXCache aUpdateCache, IEnumerable<CABankTran> aRows)
		{
			Dictionary<string, List<CABankTranDocRef>> cross = new Dictionary<string, List<CABankTranDocRef>>();
			Dictionary<int, CABankTran> rows = new Dictionary<int, CABankTran>();
			int rowCount = 0;
			
			foreach (object en in aRows)
			{
				rowCount++;
				CABankTran iDet = en as CABankTran;
				if (iDet.DocumentMatched == true || iDet.CreateDocument == true) continue; //Skip already matched records
				if (!rows.ContainsKey(iDet.TranID.Value))
				{
					rows.Add(iDet.TranID.Value, iDet);
				}
				List<CATranExt> list = null;
				CATranExt[] bestMatches = { null, null };
				list = (List<CATranExt>)this.FindDetailMatches(iDet, CurrentMatchSesstings, bestMatches);
				if (bestMatches[0] != null
					&& (bestMatches[1] == null && bestMatches[0].MatchRelevance >= RelevanceTreshold
					|| bestMatches[1] != null && (bestMatches[0].MatchRelevance - bestMatches[1].MatchRelevance) > RelevanceTreshold
					|| bestMatches[0].MatchRelevance > PaymentMatchTreshold))
				{
					CATranExt matchCandidate = bestMatches[0];
					CABankTranDocRef xref = new CABankTranDocRef();
					xref.Copy(iDet);
					xref.Copy(matchCandidate);
					if (xref.CATranID == null)
					{
						xref.DocModule = matchCandidate.OrigModule;
						xref.DocRefNbr = matchCandidate.OrigRefNbr;
						xref.DocType = matchCandidate.OrigTranType;
					}
					xref.MatchRelevance = matchCandidate.MatchRelevance;
					string key;
					if (matchCandidate.TranID.HasValue)
						key = matchCandidate.TranID.Value.ToString();
					else key = matchCandidate.OrigModule + matchCandidate.OrigTranType + matchCandidate.OrigRefNbr;

					if (cross.ContainsKey(key) == false)
					{
						cross.Add(key, new List<CABankTranDocRef>());
					}
					cross[key].Add(xref);
				}
			}

			return DoMatch(aUpdateCache, aRows, cross, rows);
		}

		public virtual void DoCAMatch(PXCache aUpdateCache, IEnumerable<CABankTran> aRows)
		{
			List<CABankTran> spareDetails = new List<CABankTran>();
			spareDetails.AddRange(aRows);

			while (spareDetails.Count > 0)
			{
				spareDetails = DoCAMatchInner(aUpdateCache, spareDetails);
			}

			foreach (CABankTran iRow in aRows)
			{
				if (iRow.DocumentMatched == false && iRow.CreateDocument == false && iRow.CountMatches > 0)
				{
					CATranExt[] bestMatches = { null, null };
					this.FindDetailMatches(iRow, CurrentMatchSesstings, bestMatches);
				}
			}
		}

		protected virtual List<CABankTran> DoDocumentMatchInner<TMatchRow>(
			PXCache aUpdateCache, 
			IEnumerable<CABankTran> aRows,
			Func<CABankTran, IMatchSettings, decimal, TMatchRow[], IEnumerable> findDetailMatching)
			where TMatchRow: CABankTranDocumentMatch
		{
			Dictionary<string, List<CABankTranDocRef>> cross = new Dictionary<string, List<CABankTranDocRef>>();
			Dictionary<int, CABankTran> rows = new Dictionary<int, CABankTran>();
			
			foreach (object en in aRows)
			{
				CABankTran iDet = en as CABankTran;
				if (iDet.DocumentMatched == true || iDet.CreateDocument == true) continue; //Skip already matched records
				if (!rows.ContainsKey(iDet.TranID.Value))
				{
					rows.Add(iDet.TranID.Value, iDet);
				}

				TMatchRow[] bestMatches = { null, null };

				findDetailMatching(iDet, CurrentMatchSesstings, Decimal.Zero, bestMatches);

				if (bestMatches[0] != null
				    && (bestMatches[1] == null && bestMatches[0].MatchRelevance >= RelevanceTreshold
				        || bestMatches[1] != null && (bestMatches[0].MatchRelevance - bestMatches[1].MatchRelevance) > RelevanceTreshold
				        || bestMatches[0].MatchRelevance > InvoiceMatchTreshold))
				{
					TMatchRow matchCandidate = bestMatches[0];
					CABankTranDocRef xref = new CABankTranDocRef();
					xref.Copy(iDet);
					matchCandidate.BuildDocRef(xref);
					xref.MatchRelevance = matchCandidate.MatchRelevance;
					string key = matchCandidate.GetDocumentKey();
					if (cross.ContainsKey(key) == false)
					{
						cross.Add(key, new List<CABankTranDocRef>());
					}
					cross[key].Add(xref);
				}
			}

			return DoMatch(aUpdateCache, aRows, cross, rows);
		}

		public virtual void DoInvMatch(PXCache aUpdateCache, IEnumerable<CABankTran> aRows)
		{
			List<CABankTran> spareDetails = new List<CABankTran>();
			spareDetails.AddRange(aRows);

			while (spareDetails.Count > 0)
			{
				spareDetails = DoDocumentMatchInner<CABankTranInvoiceMatch>(aUpdateCache, spareDetails, FindDetailMatchingInvoices);
			}

			foreach (CABankTran iRow in aRows)
			{
				if (iRow.DocumentMatched == false && iRow.CreateDocument == false && iRow.CountInvoiceMatches > 0)
				{
					//Recalculate counts - some of transactions may be matched on previous step and no available anymore 
					this.matchingInvoices.Remove(iRow);
					this.FindDetailMatchingInvoices(iRow, CurrentMatchSesstings);
				}
			}
		}

		public virtual void DoExpenseReceiptMatch(PXCache aUpdateCache, IEnumerable<CABankTran> aRows)
		{
			List<CABankTran> spareDetails = new List<CABankTran>();
			spareDetails.AddRange(aRows);

			while (spareDetails.Count > 0)
			{
				spareDetails = DoDocumentMatchInner<CABankTranExpenseDetailMatch>(
					aUpdateCache,
					spareDetails,
					(bankTran, settings, relevanceTreshold, bestMatches) => FindExpenseReceiptDetailMatches(bankTran, settings, relevanceTreshold, bestMatches));
			}

			foreach (CABankTran iRow in aRows)
			{
				if (iRow.DocumentMatched == false && iRow.CreateDocument == false && iRow.CountExpenseReceiptDetailMatches > 0)
				{
					//Recalculate counts - some of transactions may be matched on previous step and no available anymore 
					this.matchingExpenseReceiptDetails.Remove(iRow);
					this.FindExpenseReceiptDetailMatches(iRow, CurrentMatchSesstings);
				}
			}
		}

		public virtual List<CABankTran> DoMatch(PXCache aUpdateCache, IEnumerable<CABankTran> aRows, Dictionary<string, List<CABankTranDocRef>> cross, Dictionary<int, CABankTran> rows)
		{
			Dictionary<int, CABankTran> spareDetails = new Dictionary<int, CABankTran>();
			foreach (KeyValuePair<string, List<CABankTranDocRef>> iCandidate in cross)
			{
				CABankTranDocRef bestMatch = null;
				foreach (CABankTranDocRef iRef in iCandidate.Value)
				{
					if (bestMatch == null || bestMatch.MatchRelevance < iRef.MatchRelevance)
					{
						bestMatch = iRef;
					}
				}
				if (bestMatch != null && bestMatch.TranID != null)
				{
					CABankTran detail;
					if (!rows.TryGetValue(bestMatch.TranID.Value, out detail))
					{
						detail = this.Details
						    .SearchAll<
						    Asc<CABankTran.tranID, 
						    Asc<CABankTran.tranID>>>(new object[] { bestMatch.TranID });
						//(bestMatch.RefNbr).Search<>(bestMatch.LineNbr.Value);
						rows.Add(detail.TranID.Value, detail);
					}

					if (detail != null && TranMatch.Select(detail.TranID).Count == 0)
					{
						CABankTranMatch match = new CABankTranMatch()
						{
							TranID = detail.TranID,
							TranType = detail.TranType,
						};
						match.Copy(bestMatch);
						TranMatch.Insert(match);
						detail.DocumentMatched = true;
						aUpdateCache.Update(detail);

						if (match.DocModule == BatchModule.EP && match.DocType == EPExpenseClaimDetails.DocType)
						{
							EPExpenseClaimDetails receipt =
								PXSelect<EPExpenseClaimDetails,
										Where<EPExpenseClaimDetails.claimDetailCD,
											Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>
										.Select(this, match.DocRefNbr);

							receipt.BankTranDate = detail.TranDate;

							ExpenseReceipts.Update(receipt);
						}
					}

					spareDetails.Remove(bestMatch.TranID.Value);
					foreach (CABankTranDocRef iMatch in iCandidate.Value)
					{
						if (Object.ReferenceEquals(iMatch, bestMatch)) continue;
						spareDetails[iMatch.TranID.Value] = null;
					}
				}
			}
			cross.Clear();
			List<CABankTran> spareDetails1 = new List<CABankTran>(spareDetails.Keys.Count);
			foreach (KeyValuePair<int, CABankTran> iDet in spareDetails)
			{
				CABankTran detail;
				if (!rows.TryGetValue(iDet.Key, out detail))
				{
					detail = this.Details
					    .SearchAll<
					    Asc<CABankTran.tranID, 
					    Asc<CABankTran.tranID>>>(new object[] { iDet.Key });
					rows.Add(detail.TranID.Value, detail);
				}
				if (detail != null)
					spareDetails1.Add(detail);
			}
			return spareDetails1;
		}

		public virtual IEnumerable FindDetailMatches(CABankTran aDetail, IMatchSettings aSettings, CATranExt[] aBestMatches)
		{
			CashAccount cashaccount = cashAccount.Select();

			List<CATranExt> matchList = null;
			if (this.matchingTrans == null) this.matchingTrans = new Dictionary<Object, List<CATranExt>>();
			//Force search when the best matches are required.
			if (aBestMatches == null)
			{
				if (this.matchingTrans.TryGetValue(aDetail, out matchList))
				{
					aDetail.CountMatches = matchList.Count;
					//Auto match case
					if (aDetail.MatchedToExisting == true && matchList.Find((CATranExt tran) => { return tran.IsMatched == true; }) == null)
					{
						matchList.ForEach((CATranExt tran) =>
							{
								tran.IsMatched =
								PXSelect<
								    CABankTranMatch, 
								    Where<CABankTranMatch.tranID, Equal<Required<CABankTranMatch.tranID>>,
								        And<CABankTranMatch.cATranID, Equal<Required<CABankTranMatch.cATranID>>>>>
								    .Select(this, aDetail.TranID, tran.TranID)
								    .Count > 0
								||
								(tran.OrigModule == BatchModule.AP &&
								tran.OrigTranType == CATranType.CABatch &&
								PXSelect<
								    CABankTranMatch, 
								    Where<CABankTranMatch.tranID, Equal<Required<CABankTranMatch.tranID>>,
								        And<CABankTranMatch.docRefNbr, Equal<Required<CABankTranMatch.docRefNbr>>,
								        And<CABankTranMatch.docModule, Equal<Required<CABankTranMatch.docModule>>,
								        And<CABankTranMatch.docType, Equal<Required<CABankTranMatch.docType>>>>>>>
								    .Select(this, aDetail.TranID, tran.OrigRefNbr, tran.OrigModule, tran.OrigTranType)
								    .Count > 0);
							});
					}
					return matchList;
				}
			}
			decimal minRelevance = this.skipLowRelevance ? RelevanceTreshold : Decimal.Zero;
			matchList = (List<CATranExt>)StatementsMatchingProto.FindDetailMatches(this, Details.Cache, aDetail, aSettings, minRelevance, aBestMatches);
			matchingTrans[aDetail] = matchList;

			return matchList;
		}

		protected virtual IEnumerable FindDetailMatchingInvoices(CABankTran aDetail, IMatchSettings aSettings)
		{
			decimal relevanceTreshold = skipLowRelevance ? RelevanceTreshold : Decimal.Zero;
			return FindDetailMatchingInvoices(aDetail, aSettings, relevanceTreshold, null);
		}

	    protected virtual IList<CABankTranExpenseDetailMatch> FindExpenseReceiptDetailMatches(CABankTran detail, IMatchSettings settings)
	    {
	        decimal relevanceTreshold = skipLowRelevance ? RelevanceTreshold : Decimal.Zero;
	        return FindExpenseReceiptDetailMatches(detail, settings, relevanceTreshold, null);
	    }

        protected virtual IEnumerable FindDetailMatchingInvoices(CABankTran aDetail, IMatchSettings aSettings, decimal aRelevanceTreshold, CABankTranInvoiceMatch[] aBestMatches)
		{
			CashAccount cashaccount = cashAccount.Select();

			//const decimal relevanceTreshhold = 0.2m;
			List<CABankTranInvoiceMatch> result = null;
			if (this.matchingInvoices == null) this.matchingInvoices = new Dictionary<object, List<CABankTranInvoiceMatch>>(1);
			bool recalculateCounts = (aBestMatches != null);
			if (!recalculateCounts && this.matchingInvoices.TryGetValue(aDetail, out result))
			{
				aDetail.CountInvoiceMatches = result.Count;
				return result;
			}
			else
			{
				result = new List<CABankTranInvoiceMatch>();
				this.matchingInvoices[aDetail] = result;
			}
			Decimal? amount = aDetail.CuryTranAmt > 0 ? aDetail.CuryTranAmt : -aDetail.CuryTranAmt;
			CABankTranInvoiceMatch bestMatch = null;
			int bestMatchesNumber = aBestMatches != null ? aBestMatches.Length : 0;
			
			bool clearingAccount = cashaccount.ClearingAccount == true;
			if ((aDetail.DrCr == CA.CADrCr.CADebit || 
				aDetail.DrCr == CA.CADrCr.CACredit && aSettings.AllowMatchingCreditMemo == true) && 
				amount > 0)
			{
				PXSelectBase<Light.ARInvoice> sel = new PXSelectJoin<Light.ARInvoice,
					InnerJoin<Light.BAccount, On<Light.BAccount.bAccountID, Equal<Light.ARInvoice.customerID>>,
					LeftJoin<CABankTranMatch, On<CABankTranMatch.docModule, Equal<GL.BatchModule.moduleAR>,
						And<CABankTranMatch.docType, Equal<Light.ARInvoice.docType>,
						And<CABankTranMatch.docRefNbr, Equal<Light.ARInvoice.refNbr>,
							And<CABankTranMatch.tranType, Equal<Required<CABankTranMatch.tranType>>>>>>,
					LeftJoin<Light.ARAdjust, On<Light.ARAdjust.adjdDocType, Equal<Light.ARInvoice.docType>,
						And<Light.ARAdjust.adjdRefNbr, Equal<Light.ARInvoice.refNbr>,
						And<Light.ARAdjust.released, Equal<boolFalse>,
						And<Light.ARAdjust.voided, Equal<boolFalse>>>>>,
					LeftJoin<Light.CABankTranAdjustment, On<Light.CABankTranAdjustment.adjdDocType, Equal<Light.ARInvoice.docType>,
						And<Light.CABankTranAdjustment.adjdRefNbr, Equal<Light.ARInvoice.refNbr>,
						And<Light.CABankTranAdjustment.adjdModule, Equal<BatchModule.moduleAR>,
						And<Light.CABankTranAdjustment.released, Equal<boolFalse>>>>>>>>>,
						Where<Light.ARAdjust.adjgRefNbr, IsNull,
							And<Light.ARInvoice.openDoc, Equal<True>,
							And<Light.ARInvoice.released, Equal<True>,
							And<Light.ARInvoice.paymentsByLinesAllowed, NotEqual<True>,
							And<Light.ARInvoice.curyID, Equal<Required<ARInvoice.curyID>>>>>>>>(this);

				if (aDetail.MultipleMatching == true)
				{
					sel.WhereAnd<
							Where2<
								Where2<
									Where<Light.ARInvoice.discDate, Less<Required<Light.ARInvoice.discDate>>,
												Or<Light.ARInvoice.discDate, IsNull>>,
										And<Light.ARInvoice.curyDocBal, LessEqual<Required<ARInvoice.curyDocBal>>>>,
									Or<Where<Light.ARInvoice.discDate, GreaterEqual<Required<Light.ARInvoice.discDate>>,
										And<Sub<Light.ARInvoice.curyDocBal, Light.ARInvoice.curyDiscBal>, LessEqual<Required<ARInvoice.curyDocBal>>>>>>>();
				}
				else
				{
					sel.WhereAnd<
							Where2<
								Where2<
									Where<Light.ARInvoice.discDate, Less<Required<Light.ARInvoice.discDate>>,
												Or<Light.ARInvoice.discDate, IsNull>>,
										And<Light.ARInvoice.curyDocBal, Equal<Required<ARInvoice.curyDocBal>>>>,
									Or<Where<Light.ARInvoice.discDate, GreaterEqual<Required<Light.ARInvoice.discDate>>,
										And<Sub<Light.ARInvoice.curyDocBal, Light.ARInvoice.curyDiscBal>, Equal<Required<ARInvoice.curyDocBal>>>>>>>();
				}
				if (aDetail.DrCr == CA.CADrCr.CADebit)
				{
					sel
					    .WhereAnd<
					    Where<Light.ARInvoice.docType, Equal<ARDocType.invoice>>>();
				}
				else
				{
					sel
					    .WhereAnd<
					    Where<Light.ARInvoice.docType, Equal<ARDocType.creditMemo>>>();
				}
				if (aDetail.PayeeBAccountID != null)
				{
					sel.WhereAnd<
						Where<Light.ARInvoice.customerID,
							Equal<Required<ARInvoice.customerID>>>>();
				}

				foreach (PXResult<Light.ARInvoice, Light.BAccount, CABankTranMatch, Light.ARAdjust, Light.CABankTranAdjustment> it in
					sel.Select(aDetail.TranType, aDetail.CuryID, aDetail.TranDate, amount, aDetail.TranDate, amount, aDetail.PayeeBAccountID))
				{
					Light.ARInvoice iDoc = it;
					Light.BAccount iCustomer = it;
					CABankTranMatch iMatch = it;
					Light.CABankTranAdjustment iAdj = it;
					CABankTranInvoiceMatch tran = new CABankTranInvoiceMatch();
					tran.Copy(iDoc);
					tran.ReferenceName = iCustomer.AcctName;
					if (IsAlreadyMatched(tran, aDetail, iMatch) || IsAlreadyInAdjustment(iAdj, tran.OrigTranType, tran.OrigRefNbr, tran.OrigModule)) continue;
					tran.MatchRelevance = this.EvaluateMatching(aDetail, tran, aSettings);
					bool isMatchedToCurrent = (iMatch != null && iMatch.TranID.HasValue);
					if (isMatchedToCurrent == false && tran.MatchRelevance < aRelevanceTreshold)
						continue;
					if (bestMatchesNumber > 0)
					{
						for (int i = 0; i < bestMatchesNumber; i++)
						{
							if (aBestMatches[i] == null || aBestMatches[i].MatchRelevance < tran.MatchRelevance)
							{
								for (int j = bestMatchesNumber - 1; j > i; j--)
								{
									aBestMatches[j] = aBestMatches[j - 1];
								}
								aBestMatches[i] = tran;
								break;
							}
						}
					}
					else
					{
						if (bestMatch == null || bestMatch.MatchRelevance < tran.MatchRelevance)
						{
							bestMatch = tran;
						}
					}
					tran.IsBestMatch = false;
					result.Add(tran);
				}
			}
			if (aDetail.DrCr == CA.CADrCr.CACredit && !clearingAccount)
			{
				PXSelectBase<Light.APInvoice> sel = new PXSelectJoin<
						Light.APInvoice,
						InnerJoin<Light.BAccount,
							On<Light.BAccount.bAccountID, Equal<Light.APInvoice.vendorID>>,
						LeftJoin<CABankTranMatch,
							On<CABankTranMatch.docModule, Equal<GL.BatchModule.moduleAP>,
							And<CABankTranMatch.docType, Equal<Light.APInvoice.docType>,
							And<CABankTranMatch.docRefNbr, Equal<Light.APInvoice.refNbr>,
							And<CABankTranMatch.tranType, Equal<Required<CABankTranMatch.tranType>>>>>>,
						LeftJoin<Light.APAdjust,
							On<Light.APAdjust.adjdDocType, Equal<Light.APInvoice.docType>,
							And<Light.APAdjust.adjdRefNbr, Equal<Light.APInvoice.refNbr>,
							And<Light.APAdjust.released, Equal<boolFalse>>>>,
						LeftJoin<Light.APPayment,
							On<Light.APPayment.docType, Equal<Light.APInvoice.docType>,
							And<Light.APPayment.refNbr, Equal<Light.APInvoice.refNbr>>>,
						LeftJoin<Light.CABankTranAdjustment,
							On<Light.CABankTranAdjustment.adjdDocType, Equal<Light.APInvoice.docType>,
							And<Light.CABankTranAdjustment.adjdRefNbr, Equal<Light.APInvoice.refNbr>,
							And<Light.CABankTranAdjustment.adjdModule, Equal<BatchModule.moduleAP>,
							And<Light.CABankTranAdjustment.released, Equal<boolFalse>>>>>>>>>>,
						Where<Light.APAdjust.adjgRefNbr, IsNull,
							And<Light.APInvoice.openDoc, Equal<True>,
							And<Light.APInvoice.released, Equal<True>,
							And<Light.APInvoice.paymentsByLinesAllowed, NotEqual<True>,
							And<Light.APInvoice.docType, Equal<APDocType.invoice>,
							And<Light.APInvoice.curyID, Equal<Required<APInvoice.curyID>>,
							And<Light.APPayment.refNbr, IsNull,
							And<Where<Light.APInvoice.docDate, LessEqual<Required<Light.APInvoice.docDate>>,
								Or<Current<APSetup.earlyChecks>, Equal<True>>>>>>>>>>>>(this);

				if (aDetail.MultipleMatching == true)
				{
					sel.WhereAnd<
							Where2<
								Where2<
									Where<Light.APInvoice.discDate, Less<Required<Light.APInvoice.discDate>>,
												Or<Light.APInvoice.discDate, IsNull>>,
										And<Light.APInvoice.curyDocBal, LessEqual<Required<Light.APInvoice.curyDocBal>>>>,
									Or<Where<Light.APInvoice.discDate, GreaterEqual<Required<Light.APInvoice.discDate>>,
										And<Sub<Light.APInvoice.curyDocBal, Light.APInvoice.curyDiscBal>, LessEqual<Required<Light.APInvoice.curyDocBal>>>>>>>();
				}
				else
				{
					sel.WhereAnd<
							Where2<
								Where2<
									Where<Light.APInvoice.discDate, Less<Required<Light.APInvoice.discDate>>,
												Or<Light.APInvoice.discDate, IsNull>>,
										And<Light.APInvoice.curyDocBal, Equal<Required<Light.APInvoice.curyDocBal>>>>,
									Or<Where<Light.APInvoice.discDate, GreaterEqual<Required<Light.APInvoice.discDate>>,
										And<Sub<Light.APInvoice.curyDocBal, Light.APInvoice.curyDiscBal>, Equal<Required<Light.APInvoice.curyDocBal>>>>>>>();
				}
				if (aDetail.PayeeBAccountID != null)
				{
					sel.WhereAnd<
						Where<Light.APInvoice.vendorID,
							Equal<Required<APInvoice.vendorID>>>>();
				}

				foreach (PXResult<Light.APInvoice, Light.BAccount, CABankTranMatch, Light.APAdjust, Light.APPayment, Light.CABankTranAdjustment> it in
					sel.Select(aDetail.TranType, aDetail.CuryID, aDetail.TranDate, aDetail.TranDate, amount, aDetail.TranDate, amount, aDetail.PayeeBAccountID))
				{
					Light.APInvoice iDoc = it;
					Light.BAccount iPayee = it;
					CABankTranMatch iMatch = it;
					Light.CABankTranAdjustment iAdj = it;
					CABankTranInvoiceMatch tran = new CABankTranInvoiceMatch();
					tran.Copy(iDoc);
					tran.ReferenceName = iPayee.AcctName;
					if (IsAlreadyMatched(tran, aDetail, iMatch) || IsAlreadyInAdjustment(iAdj, tran.OrigTranType, tran.OrigRefNbr, tran.OrigModule)) continue;
					tran.MatchRelevance = this.EvaluateMatching(aDetail, tran, aSettings);
					bool isMatchedToCurrent = (iMatch != null && iMatch.TranID.HasValue);
					if (isMatchedToCurrent == false && tran.MatchRelevance < aRelevanceTreshold)
						continue;
					if (bestMatchesNumber > 0)
					{
						for (int i = 0; i < bestMatchesNumber; i++)
						{
							if (aBestMatches[i] == null || aBestMatches[i].MatchRelevance < tran.MatchRelevance)
							{
								for (int j = bestMatchesNumber - 1; j > i; j--)
								{
									aBestMatches[j] = aBestMatches[j - 1];
								}
								aBestMatches[i] = tran;
								break;
							}
						}
					}
					else
					{
						if (bestMatch == null || bestMatch.MatchRelevance < tran.MatchRelevance)
						{
							bestMatch = tran;
						}
					}
					tran.IsBestMatch = false;
					result.Add(tran);
				}
			}
			if (bestMatchesNumber > 0)
				bestMatch = aBestMatches[0];
			if (bestMatch != null)
			{
				bestMatch.IsBestMatch = true;
			}
			aDetail.CountInvoiceMatches = result.Count;
			return result;
		}

        protected virtual List<CABankTranExpenseDetailMatch> FindExpenseReceiptDetailMatches(
            CABankTran bankTran,
            IMatchSettings settings, 
            decimal relevanceTreshold, 
            CABankTranExpenseDetailMatch[] bestMatches)
        {
	        CashAccount cashaccount = cashAccount.Select();

	        if (cashaccount.UseForCorpCard != true)
		        return new List<CABankTranExpenseDetailMatch>();

			List<CABankTranExpenseDetailMatch> result = null;

            if (matchingExpenseReceiptDetails == null)
            {
                matchingExpenseReceiptDetails = new Dictionary<object, List<CABankTranExpenseDetailMatch>>(1);
            }

            bool recalculateCounts = (bestMatches != null);

            if (!recalculateCounts && this.matchingExpenseReceiptDetails.TryGetValue(bankTran, out result))
            {
                bankTran.CountExpenseReceiptDetailMatches = result.Count;
                return result;
            }
            else
            {
                result = new List<CABankTranExpenseDetailMatch>();
                this.matchingExpenseReceiptDetails[bankTran] = result;
            }

            Pair<DateTime, DateTime> tranDateRange = StatementsMatchingProto.GetDateRangeForMatch(bankTran, settings);

            Decimal amount = Math.Abs(bankTran.CuryTranAmt.Value);

	        decimal curyDiffThresholdValue = 0m;

	        if (settings.CuryDiffThreshold != null)
	        {
		        curyDiffThresholdValue = amount / 100 * settings.CuryDiffThreshold.Value;
	        }

            decimal amountFrom = amount - curyDiffThresholdValue;
            decimal amountTo = amount + curyDiffThresholdValue;

            int bestMatchesNumber = bestMatches != null ? bestMatches.Length : 0;

            if (bankTran.CuryTranAmt < 0)
            {
				CABankTranMatch existingBankTranMatch =
					PXSelect<CABankTranMatch,
						Where<CABankTranMatch.tranID, Equal<Required<CABankTranMatch.tranID>>,
								And<CABankTranMatch.docModule, Equal<BatchModule.moduleEP>,
								And<CABankTranMatch.docType, Equal<EPExpenseClaimDetails.docType>>>>>
						.Select(this, bankTran.TranID);

				var receipts = PXSelectJoin<
                                        EPExpenseClaimDetails,
                                        InnerJoin<Light.BAccount,
                                            On<EPExpenseClaimDetails.employeeID, Equal<Light.BAccount.bAccountID>>,
                                        InnerJoin<CACorpCard,
                                            On<CACorpCard.corpCardID, Equal<EPExpenseClaimDetails.corpCardID>>,
                                        LeftJoin<CABankTranMatch, 
                                            On<CABankTranMatch.docModule, Equal<GL.BatchModule.moduleEP>,
												And<CABankTranMatch.docType, Equal<EPExpenseClaimDetails.docType>,
												And<CABankTranMatch.docRefNbr, Equal<EPExpenseClaimDetails.claimDetailCD>>>>,
	                                    LeftJoin<CurrencyInfo,
											On<EPExpenseClaimDetails.claimCuryInfoID, Equal<CurrencyInfo.curyInfoID>>,
	                                    LeftJoin<CATran,//Quick Check CATran
		                                    On<CATran.origModule, Equal<BatchModule.moduleAP>,
			                                    And<EPExpenseClaimDetails.aPDocType, Equal<CATran.origTranType>,
												And<EPExpenseClaimDetails.aPRefNbr, Equal<CATran.origRefNbr>>>>,
			                            LeftJoin<GLTran,
											On<GLTran.module, Equal<BatchModule.moduleAP>,
												And<EPExpenseClaimDetails.aPDocType, Equal<GLTran.tranType>,
												And<EPExpenseClaimDetails.aPRefNbr, Equal<GLTran.refNbr>,
												And<EPExpenseClaimDetails.aPLineNbr, Equal<GLTran.tranLineNbr>>>>>,
										LeftJoin<CATran2,//Debit Adj CATran
											On<CATran2.origModule, Equal<BatchModule.moduleAP>,
												And<CATran2.origTranType, Equal<GLTranType.gLEntry>,
												And<CATran2.origRefNbr, Equal<GLTran.batchNbr>,
												And<CATran2.origLineNbr, Equal<GLTran.lineNbr>>>>>>>>>>>>,
                                        Where2<
	                                        Where<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes, Between<Required<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes>, Required<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes>>,
												And<EPExpenseClaimDetails.curyID, NotEqual<EPExpenseClaimDetails.cardCuryID>,
												Or<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes, Equal<Required<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes>>>>>,
											And<EPExpenseClaimDetails.hold, NotEqual<True>,
											And<EPExpenseClaimDetails.rejected, NotEqual<True>,
											And<EPExpenseClaimDetails.paidWith, NotEqual<EPExpenseClaimDetails.paidWith.cash>,
                                            And<CACorpCard.cashAccountID, Equal<Required<CACorpCard.cashAccountID>>,
                                            And<EPExpenseClaimDetails.expenseDate, GreaterEqual<Required<EPExpenseClaimDetails.expenseDate>>,
                                            And<EPExpenseClaimDetails.expenseDate, LessEqual<Required<EPExpenseClaimDetails.expenseDate>>,
											And<CATran.tranID, IsNull,
											And<CATran2.tranID, IsNull,
											Or<EPExpenseClaimDetails.claimDetailCD, Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>>>>>>>>>>
                                        .Select(this,
												amountFrom,
												amountTo,
												amount,
												cashaccount.CashAccountID,
                                                tranDateRange.first,
                                                tranDateRange.second,
												existingBankTranMatch?.DocRefNbr);


                foreach (PXResult<EPExpenseClaimDetails, Light.BAccount, CACorpCard, CABankTranMatch, CurrencyInfo> row in receipts)
                {
                    EPExpenseClaimDetails receipt = row;
                    Light.BAccount employeeBAccount = row;
                    CACorpCard card = row;
                    CABankTranMatch matchLink = row;
	                CurrencyInfo claimCurrencyInfo = row;

					CABankTranExpenseDetailMatch matchRow = new CABankTranExpenseDetailMatch
                    {
                        RefNbr = receipt.ClaimDetailCD,
                        ExtRefNbr = receipt.ExpenseRefNbr,
                        CuryDocAmt = receipt.ClaimCuryTranAmtWithTaxes,
						ClaimCuryID = claimCurrencyInfo?.CuryID ?? cashaccount.CuryID,
						CuryDocAmtDiff = Math.Abs(amount - receipt.ClaimCuryTranAmtWithTaxes.Value),
                        PaidWith = receipt.PaidWith,
                        ReferenceID = receipt.EmployeeID,
                        ReferenceName = employeeBAccount.AcctName,
                        DocDate = receipt.ExpenseDate,
                        CardNumber = card.CardNumber,
                        TranDesc = receipt.TranDesc
                    };

                    if (IsAlreadyMatched(BatchModule.EP, EPExpenseClaimDetails.DocType, matchRow.RefNbr, bankTran, matchLink)
                        || !IsCardNumberMatch(bankTran.CardNumber, matchRow.CardNumber))
                        continue;

                    matchRow.MatchRelevance = EvaluateMatching(bankTran, matchRow, settings);

                    bool isMatchedToCurrent = (matchLink != null && matchLink.TranID.HasValue);

                    if (isMatchedToCurrent == false && matchRow.MatchRelevance < relevanceTreshold)
                        continue;

                    if (bestMatchesNumber > 0)
                    {
                        for (int i = 0; i < bestMatchesNumber; i++)
                        {
                            if (bestMatches[i] == null || bestMatches[i].MatchRelevance < matchRow.MatchRelevance)
                            {
                                for (int j = bestMatchesNumber - 1; j > i; j--)
                                {
                                    bestMatches[j] = bestMatches[j - 1];
                                }
                                bestMatches[i] = matchRow;
                                break;
                            }
                        }
                    }

                    result.Add(matchRow);
                }
			}

            bankTran.CountExpenseReceiptDetailMatches = result.Count;

            return result;
        }

	    public virtual string ExtractCardNumber(string cardNumberRaw)
	    {
	        if (String.IsNullOrEmpty(cardNumberRaw))
	            return cardNumberRaw;

            int startPos = 0;
	        int endPos = -1;

	        for (int i = cardNumberRaw.Length - 1; i >= 0; i--)
	        {
	            if (endPos == -1 && Char.IsDigit(cardNumberRaw[i]))
	            {
	                endPos = i;

	                continue;
	            }

	            if (endPos != -1 && !Char.IsDigit(cardNumberRaw[i]))
	            {
	                startPos = i + 1;
	                break;
	            }
            }

	        return endPos == -1
	            ? string.Empty
	            : cardNumberRaw.Substring(startPos, endPos - startPos + 1);
	    }

	    public virtual bool IsCardNumberMatch(string bankTranCardNumberRaw, string receiptCardNumberRaw)
	    {
	        string bankTranCardNumber = ExtractCardNumber(bankTranCardNumberRaw);
	        string receiptCardNumber = ExtractCardNumber(receiptCardNumberRaw);

            if (String.IsNullOrEmpty(bankTranCardNumber))
	            return true;

	        if (String.IsNullOrEmpty(receiptCardNumber))
	            return false;

            return bankTranCardNumber.Length > receiptCardNumber.Length
                ? bankTranCardNumber.Contains(receiptCardNumber)
                : receiptCardNumber.Contains(bankTranCardNumber);
        }

        protected virtual bool IsMatchedOnCreateTab(string refNbr, string docType, string module)
		{
			return PXSelect<
			    CABankTranAdjustment,
			    Where<CABankTranAdjustment.adjdRefNbr, Equal<Required<CABankTranAdjustment.adjdRefNbr>>,
			        And<CABankTranAdjustment.adjdModule, Equal<Required<CABankTranAdjustment.adjdModule>>,
			        And<CABankTranAdjustment.adjdDocType, Equal<Required<CABankTranAdjustment.adjdDocType>>>>>>
			    .Select(this, refNbr, module, docType)
			    .Count > 0;
		}
		protected virtual bool IsInvoiceMatched(string refNbr, string docType, string module)
		{
			return PXSelect<
			    CABankTranMatch, 
			    Where<CABankTranMatch.docRefNbr, Equal<Required<CABankTranMatch.docRefNbr>>,
			        And<CABankTranMatch.docModule, Equal<Required<CABankTranMatch.docModule>>,
			        And<CABankTranMatch.docType, Equal<Required<CABankTranMatch.docType>>>>>>
			    .Select(this, refNbr, module, docType)
			    .Count > 0;
		}
		protected virtual bool IsAlreadyInAdjustment(Light.CABankTranAdjustment aAdjust, String aTranDocType, String aTranRefNbr, String aTranModule)
		{
			if (aAdjust != null)
				return IsAlreadyInAdjustment(aAdjust.TranID, aAdjust.AdjdRefNbr, aAdjust.AdjdDocType, aAdjust.AdjdModule, aTranDocType, aTranRefNbr, aTranModule);
			else return IsAlreadyInAdjustment(null, null, null, null, aTranDocType, aTranRefNbr, aTranModule);
		}
		protected virtual bool IsAlreadyInAdjustment(int? tranID, String adjdRefNbr, string adjdDocType, String adjdModule, String aTranDocType, String aTranRefNbr, String aTranModule)
		{
			PXCache cache = this.Caches[typeof(CABankTranAdjustment)];
			bool isInAdjustment = adjdRefNbr != null;
			if (!isInAdjustment)
			{
				foreach (CABankTranAdjustment adj in cache.Inserted)
				{
					if (adj.AdjdDocType == aTranDocType && adj.AdjdRefNbr == aTranRefNbr && adj.AdjdModule == aTranModule)
					{
						isInAdjustment = true;
						break;
					}
				}
			}
			else
			{
				foreach (CABankTranAdjustment adj in cache.Deleted)
				{
					if (adj.TranID == tranID && adj.AdjdDocType == adjdDocType && adj.AdjdRefNbr == adjdRefNbr && adj.AdjdModule == adjdModule)
					{
						isInAdjustment = false;
						break;
					}
				}
			}
			return isInAdjustment;
		}

	    protected virtual bool IsAlreadyMatched(CABankTranInvoiceMatch aMatch, CABankTran aTran, CABankTranMatch aTranMatch)
	    {
	        return IsAlreadyMatched(aMatch.OrigModule, aMatch.OrigTranType, aMatch.OrigRefNbr, aTran, aTranMatch);
	    }

        protected virtual bool IsAlreadyMatched(string module, string docType, string refNbr, CABankTran aTran, CABankTranMatch aTranMatch)
		{
			//CABankTranMatch has only one key field - TranID, if there will be new keys - this method is due to change
			PXCache cache = this.Caches[typeof(CABankTranMatch)];
			bool isMatched = aTranMatch.TranID != null;
			bool mathedToCurrent = aTranMatch.TranID == aTran.TranID;
			if (isMatched)
			{
				switch (cache.GetStatus(aTranMatch))
				{
					case PXEntryStatus.Deleted:
					case PXEntryStatus.InsertedDeleted:
						isMatched = false;
						mathedToCurrent = false;
						break;
					default:
							CABankTranMatch cached = (CABankTranMatch)cache.Locate(aTranMatch);
							if (cached != null && !cache.ObjectsEqual<CABankTranMatch.tranType,
								CABankTranMatch.docModule, CABankTranMatch.docType, CABankTranMatch.docRefNbr>(cached, aTranMatch))
							{
								isMatched = false;
								mathedToCurrent = false;
							}
						
						break;
				}
			}
			if (!isMatched)
			{
				foreach (CABankTranMatch iMatch in cache.Inserted)
				{
					if (iMatch.TranType == aTran.TranType &&
						iMatch.DocModule == module &&
						iMatch.DocType == docType &&
						iMatch.DocRefNbr == refNbr)
					{
						if (iMatch.TranID != aTran.TranID)
						{
							isMatched = true;
							mathedToCurrent = iMatch.TranID == aTran.TranID;
							break;
						}
					}
				}
			}
			if (!isMatched)
			{
				foreach (CABankTranMatch iMatch in cache.Updated)
				{
					if (iMatch.TranType == aTran.TranType &&
						iMatch.DocModule == module &&
						iMatch.DocType == docType &&
						iMatch.DocRefNbr == refNbr)
					{
						if (iMatch.TranID != aTran.TranID)
						{
							isMatched = true;
							mathedToCurrent = iMatch.TranID == aTran.TranID;
							break;
						}
					}
				}
			}

			return isMatched && !mathedToCurrent;
		}
		public virtual decimal EvaluateMatching(CABankTran aDetail, CATran aTran, IMatchSettings aSettings)
		{
			return StatementsMatchingProto.EvaluateMatching(this, aDetail, aTran, aSettings);
		}

		public virtual decimal EvaluateMatching(string aStr1, string aStr2, bool aCaseSensitive, bool matchEmpty = true)
		{
			return StatementMatching.EvaluateMatching(aStr1, aStr2, aCaseSensitive, matchEmpty);
		}

		public virtual decimal EvaluateMatching(CABankTran aDetail, CABankTranInvoiceMatch aTran, IMatchSettings aSettings)
		{
			return StatementsMatchingProto.EvaluateMatching(this, aDetail, aTran, aSettings);
		}

	    public virtual decimal EvaluateMatching(CABankTran aDetail, CABankTranExpenseDetailMatch matchRow, IMatchSettings aSettings)
	    {
	        return StatementsMatchingProto.EvaluateMatching(this, aDetail, matchRow, aSettings);
	    }

        public virtual decimal EvaluateTideMatching(string aStr1, string aStr2, bool aCaseSensitive, bool matchEmpty = true)
		{
			return StatementMatching.EvaluateTideMatching(aStr1, aStr2, aCaseSensitive, matchEmpty);
		}

		public virtual decimal CompareDate(CABankTran aDetail, CATran aTran, double meanValue, double sigma)
		{
			return StatementsMatchingProto.CompareDate(aDetail, aTran, meanValue, sigma);
		}

	    public virtual decimal CompareDate(CABankTran aDetail, CABankTranExpenseDetailMatch aTran, double meanValue, double sigma)
	    {
	        return StatementsMatchingProto.CompareDate(aDetail, aTran.DocDate, meanValue, sigma);
	    }

		public virtual decimal CompareExpenseReceiptAmount(CABankTran bankTran, CABankTranExpenseDetailMatch receipt, IMatchSettings settings)
		{
			double diff = Convert.ToDouble(Math.Abs(bankTran.CuryTranAmt.Value) - receipt.CuryDocAmt.Value);

			double sigma = Convert.ToDouble(settings.CuryDiffThreshold.Value * bankTran.CuryTranAmt.Value) / 100;

			decimal res = (decimal) Math.Exp(-(diff * diff / (2 * sigma * sigma)));

			return res > 0 ? res : 0.0m;
		}

		public virtual decimal CompareRefNbr(CABankTran aDetail, CATran aTran, bool looseCompare, IMatchSettings settings)
		{
			return StatementsMatchingProto.CompareRefNbr(this, aDetail, aTran, looseCompare, settings);
		}

		public virtual decimal CompareRefNbr(CABankTran aDetail, CABankTranInvoiceMatch aTran, bool looseCompare)
		{
			return StatementsMatchingProto.CompareRefNbr(this, aDetail, aTran, looseCompare);
		}

	    public virtual decimal CompareRefNbr(CABankTran aDetail, CABankTranExpenseDetailMatch aTran, bool looseCompare, IMatchSettings matchSettings)
	    {
	        return StatementsMatchingProto.CompareRefNbr(this, aDetail, aTran.ExtRefNbr, looseCompare, matchSettings);
	    }

        public virtual decimal ComparePayee(CABankTran aDetail, CATran aTran)
		{
			return StatementsMatchingProto.ComparePayee(this, aDetail, aTran);
		}

		public virtual decimal ComparePayee(CABankTran aDetail, CABankTranInvoiceMatch aTran)
		{
			return StatementsMatchingProto.ComparePayee(this, aDetail, aTran);
		}

		protected virtual void PopulateAdjustmentFieldsAR(CABankTranAdjustment adj)
		{
			StatementApplicationBalancesProto.PopulateAdjustmentFieldsAR(this, CurrencyInfo_CuryInfoID, DetailsForPaymentCreation.Current, adj);
		}

		protected virtual void PopulateAdjustmentFieldsAP(CABankTranAdjustment adj)
		{
			StatementApplicationBalancesProto.PopulateAdjustmentFieldsAP(this, CurrencyInfo_CuryInfoID, DetailsForPaymentCreation.Current, adj);
		}

		public virtual void UpdateBalance(CABankTranAdjustment adj, bool isCalcRGOL)
		{
			if (adj.AdjdDocType != null && adj.AdjdRefNbr != null)
			{
				StatementApplicationBalancesProto.UpdateBalance(this, CurrencyInfo_CuryInfoID, DetailsForPaymentCreation.Current, adj, isCalcRGOL);
			}
		}

		protected virtual void ClearCachedMatches()
		{
			if (this.matchingInvoices != null) this.matchingInvoices.Clear();
			if (this.matchingTrans != null) this.matchingTrans.Clear();
		    if (this.matchingExpenseReceiptDetails != null) this.matchingExpenseReceiptDetails.Clear();
        }

		#endregion
		#region Rules Matching
		protected virtual bool AttemptApplyRules(CABankTran transaction, bool applyHiding)
		{
			foreach (CABankTranRule rule in Rules.Select())
			{
				if ((applyHiding == true || rule.Action != RuleAction.HideTransaction)
					&& CheckRuleMatches(transaction, rule))
				{
					try
					{
						ApplyRule(transaction, rule);
						Details.Cache.RaiseExceptionHandling<CABankTran.entryTypeID>(transaction, transaction.EntryTypeID, null);
						PXUIFieldAttribute.SetError<CABankTran.ruleID>(Details.Cache, transaction, null);

						return true;
					}
					catch (PXException)
					{
						Details.Cache.RaiseExceptionHandling<CABankTran.entryTypeID>(transaction, transaction.EntryTypeID, null);
						PXUIFieldAttribute.SetWarning<CABankTran.ruleID>(Details.Cache, transaction, Messages.BankRuleFailedToApply);
						ResetTranFields(Details.Cache, transaction);
					}
				}
			}

			return false;
		}

		protected virtual bool CheckRuleMatches(CABankTran transaction, CABankTranRule rule)
		{
			if (rule.BankDrCr != transaction.DrCr)
				return false;

			bool match = true;

			decimal tranAmout = Math.Abs(transaction.CuryTranAmt ?? 0.0m);
			switch (rule.AmountMatchingMode)
			{
				case MatchingMode.Equal:
					match &= tranAmout == rule.CuryTranAmt;
					break;
				case MatchingMode.Between:
					match &= rule.CuryTranAmt <= tranAmout && tranAmout <= rule.MaxCuryTranAmt;
					break;
			}

			if (rule.BankTranCashAccountID != null && match)
			{
				match &= transaction.CashAccountID == rule.BankTranCashAccountID;
			}

			if (rule.TranCuryID != null && match)
			{
				match &= (transaction.CuryID ?? "").Trim() == rule.TranCuryID.Trim();
			}

			if (String.IsNullOrWhiteSpace(rule.TranCode) == false && match)
			{
				match &= String.Equals(transaction.TranCode ?? "", rule.TranCode, StringComparison.CurrentCultureIgnoreCase);
			}

			if (String.IsNullOrEmpty(rule.BankTranDescription) == false && match)
			{
				match &= rule.Regex.IsMatch(transaction.TranDesc ?? "");
			}

			return match;
		}

		protected virtual void ApplyRule(CABankTran transaction, CABankTranRule rule)
		{
			if (rule.Action == RuleAction.CreateDocument)
			{
				Details.Cache.SetValueExt<CABankTran.origModule>(transaction, rule.DocumentModule);
				Details.Cache.SetValueExt<CABankTran.entryTypeID>(transaction, rule.DocumentEntryTypeID);
			}
			else if (rule.Action == RuleAction.HideTransaction)
			{
				transaction.CreateDocument = false;
				transaction.DocumentMatched = false;
				transaction.Hidden = true;
				transaction.Processed = true;
			}

			Details.Cache.SetValue<CABankTran.ruleID>(transaction, rule.RuleID);
		}
		#endregion
		#region ModuleTranTypeSelector to CATranExt
		protected virtual void CATranExt_OrigTranType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), "PersistingCheck", PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CashAccount.acctSettingsAllowed> e) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), "PersistingCheck", PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CashAccount.pTInstancesAllowed> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", CR.Messages.BAccountName)]
		protected virtual void BAccountR_AcctName_CacheAttached(PXCache sender) { }

        public static void CheckCATransaction(ICADocSource parameters, CASetup setup)
        {
            if (parameters.OrigModule == GL.BatchModule.CA)
            {
                if (parameters.CashAccountID == null)
                {
                    throw new PXRowPersistingException(typeof(AddTrxFilter.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name);
                }

                if (string.IsNullOrEmpty(parameters.EntryTypeID))
                {
                    throw new PXRowPersistingException(typeof(AddTrxFilter.entryTypeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name);
                }

                if (string.IsNullOrEmpty(parameters.ExtRefNbr) && setup.RequireExtRefNbr == true)
                {
                    throw new PXRowPersistingException(typeof(AddTrxFilter.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.extRefNbr).Name);
                }
            }
        }
		
        public static CATran AddCATransaction(PXGraph graph, ICADocSource parameters, CurrencyInfo aCuryInfo, IEnumerable<CASplit> splits, bool IsTransferExpense)
        {
            if (parameters.OrigModule == GL.BatchModule.CA)
            {
                CATranEntry te = PXGraph.CreateInstance<CATranEntry>();
                CashAccount cashacct = (CashAccount)PXSelect<
                    CashAccount, 
                    Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
                    .Select(graph, parameters.CashAccountID);

                CurrencyInfo refInfo = aCuryInfo;
                if (refInfo == null)
                {
                    refInfo = (CurrencyInfo)PXSelectReadonly<
                        CurrencyInfo, 
                        Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
                        .Select(te, parameters.CuryInfoID);
                }
                if (refInfo != null)
                {
                    foreach (CurrencyInfo info in PXSelect<
                        CurrencyInfo, 
                        Where<CurrencyInfo.curyInfoID, Equal<Current<CAAdj.curyInfoID>>>>
                        .Select(te))
                    {
                        CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(refInfo);
                        new_info.CuryInfoID = info.CuryInfoID;
                        te.currencyinfo.Cache.Update(new_info);
                    }
                }
                else if ((cashacct != null) && (cashacct.CuryRateTypeID != null))
                {
                    refInfo = new CurrencyInfo();
                    refInfo.CuryID = cashacct.CuryID;
                    refInfo.CuryRateTypeID = cashacct.CuryRateTypeID;
                    refInfo = te.currencyinfo.Insert(refInfo);
                }

                CAAdj adj = new CAAdj();
                adj.AdjTranType = (IsTransferExpense ? CATranType.CATransferExp : CATranType.CAAdjustment);
                if (IsTransferExpense)
                {
                    adj.TransferNbr = (graph as CashTransferEntry).Transfer.Current.TransferNbr;
                }
                adj.CashAccountID = parameters.CashAccountID;
                adj.CuryID = parameters.CuryID;
                adj.CuryInfoID = refInfo.CuryInfoID;
                adj.DrCr = parameters.DrCr;
                adj.ExtRefNbr = parameters.ExtRefNbr;
                adj.Released = false;
                adj.Cleared = parameters.Cleared;
                adj.TranDate = parameters.TranDate;
                adj.TranDesc = parameters.TranDesc;
                adj.EntryTypeID = parameters.EntryTypeID;
                adj.CuryControlAmt = parameters.CuryOrigDocAmt;
                adj.NoteID = parameters.NoteID;
                adj.Hold = true;
                adj.TaxZoneID = null;
                adj = te.CAAdjRecords.Insert(adj);
                if (splits == null)
                {
                    CASplit split = new CASplit();
                    split.AdjTranType = adj.AdjTranType;
                    split.CuryInfoID = refInfo.CuryInfoID;
                    split.Qty = (decimal)1.0;
                    split.CuryUnitPrice = parameters.CuryOrigDocAmt;
                    split.CuryTranAmt = parameters.CuryOrigDocAmt;
                    split.TranDesc = parameters.TranDesc;
                    te.CASplitRecords.Insert(split);
                }
                else
                {
                    foreach (CASplit split in splits)
                    {
                        split.AdjTranType = adj.AdjTranType;
                        split.AdjRefNbr = adj.RefNbr;
                        te.CASplitRecords.Insert(split);
                    }
                }
                adj.CuryTaxAmt = adj.CuryTaxTotal;
				adj.Hold = false;
                adj = te.CAAdjRecords.Update(adj);
                te.hold.Press();
                te.Save.Press();
                adj = (CAAdj)te.Caches[typeof(CAAdj)].Current;
                return (CATran)PXSelect<
                    CATran, 
                    Where<CATran.tranID, Equal<Required<CAAdj.tranID>>>>
                    .Select(te, adj.TranID);
            }
            return null;
        }

		public static void RematchFromExpenseReceipt(
			PXGraph graph, 
			CABankTranMatch bankTranMatch, long? catranID, int? referenceID, 
			EPExpenseClaimDetails receipt)
		{
			bankTranMatch.CATranID = catranID;
			bankTranMatch.ReferenceID = referenceID;
			bankTranMatch.DocModule = null;
			bankTranMatch.DocType = null;
			bankTranMatch.DocRefNbr = null;

			graph.Caches[typeof(CABankTranMatch)].Update(bankTranMatch);

			CABankTran bankTran = PXSelect<CABankTran,
					Where<CABankTran.tranID, Equal<Required<CABankTran.tranID>>>>
				.Select(graph, bankTranMatch.TranID);

			graph.Caches[typeof(CABankTran)].Update(bankTran);

			receipt.BankTranDate = null;
		}

		public static bool IsMatchedToExpenseReceipt(CABankTranMatch match)
		{
			return match != null && match.DocModule == BatchModule.EP && match.DocType == EPExpenseClaimDetails.DocType;
		}

		public static bool IsMatchedToInvoice(CABankTran tran, CABankTranMatch match)
		{
			return !(match != null && (match.CATranID != null
									   || (match.DocType == CATranType.CABatch && match.DocModule == GL.BatchModule.AP)
									   || IsMatchedToExpenseReceipt(match)));
		}

		#region Helpers

		public class GLCATranToExpenseReceiptMatchingGraphExtension<TGraph> : PXGraphExtension<TGraph>
			where TGraph: PXGraph
		{
			protected void _(Events.RowInserted<CATran> e)
			{
				if (e.Row.OrigTranType == GLTranType.GLEntry && e.Row.OrigModule == BatchModule.AP)
				{
					PXResult<EPExpenseClaimDetails, CABankTranMatch> row = GetExpenseReceiptWithBankTranMatching(e.Row);

					CABankTranMatch bankTranMatch = (CABankTranMatch)row;

					if (bankTranMatch?.TranID != null)
					{
						CABankTran bankTran = CABankTran.PK.Find(Base, bankTranMatch.TranID);

						e.Row.Cleared = true;
						e.Row.ClearDate = bankTran.TranDate;
					}
				}
			}

			protected void _(Events.RowPersisted<CATran> e)
			{
				if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open
					&& e.Row.OrigTranType == GLTranType.GLEntry && e.Row.OrigModule == BatchModule.AP)
				{
					PXResult<EPExpenseClaimDetails, CABankTranMatch> row = GetExpenseReceiptWithBankTranMatching(e.Row);

					if (row != null)
					{
						CABankTranMatch bankTranMatch = (CABankTranMatch)row;

						if (bankTranMatch?.DocRefNbr != null)
						{
							EPExpenseClaimDetails receipt = row;

							RematchFromExpenseReceipt(Base, bankTranMatch, e.Row.TranID, e.Row.ReferenceID, receipt);

							Base.Caches[typeof(CABankTranMatch)].PersistUpdated(bankTranMatch);

							PXCache receiptCache = Base.Caches[typeof(EPExpenseClaimDetails)];

							PXDBTimestampAttribute timestampAttribute = receiptCache
								.GetAttributesOfType<PXDBTimestampAttribute>(null, nameof(EPExpenseClaimDetails.tstamp))
								.First();

							timestampAttribute.RecordComesFirst = true;

							receiptCache.PersistUpdated(receipt);
						}
					}
				}
			}

			private PXResult<EPExpenseClaimDetails, CABankTranMatch> GetExpenseReceiptWithBankTranMatching(CATran caTran)
			{
				GLTran glTran = 
					PXSelect<GLTran,
						Where<GLTran.module, Equal<Required<GLTran.module>>,
							And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
							And<GLTran.lineNbr, Equal<Required<GLTran.lineNbr>>,
							And<GLTran.tranType, Equal<APDocType.debitAdj>>>>>>
						.Select(Base, caTran.OrigModule, caTran.OrigRefNbr, caTran.OrigLineNbr);

				if (glTran == null)
					return null;

				 return PXSelectJoin<EPExpenseClaimDetails,
							LeftJoin<CABankTranMatch,
								On<EPExpenseClaimDetails.docType, Equal<CABankTranMatch.docType>,
									And<EPExpenseClaimDetails.claimDetailCD, Equal<CABankTranMatch.docRefNbr>,
									And<CABankTranMatch.docModule, Equal<BatchModule.moduleEP>>>>>,
							Where<EPExpenseClaimDetails.aPDocType, Equal<Required<EPExpenseClaimDetails.aPDocType>>,
								And<EPExpenseClaimDetails.aPRefNbr, Equal<Required<EPExpenseClaimDetails.aPRefNbr>>,
								And<EPExpenseClaimDetails.aPLineNbr, Equal<Required<EPExpenseClaimDetails.aPLineNbr>>>>>>
							.Select(Base, glTran.TranType, glTran.RefNbr, glTran.TranLineNbr)
							.AsEnumerable()
							.Cast<PXResult<EPExpenseClaimDetails, CABankTranMatch>>()
							.SingleOrDefault();
			}
		}

		#endregion
	}

	public class CABankIncomingPaymentsMaint : CABankTransactionsMaint
	{
		public PXFilter<MatchSettings> matchSettings;

		public override IMatchSettings CurrentMatchSesstings
		{
			get { return matchSettings.Current; }
		}

		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(CABankTranType.paymentImport))]
		[CABankTranType.List()]
		protected virtual void Filter_TranType_CacheAttached(PXCache sender) { }
	}
}
