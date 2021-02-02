using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class CABankTransactionsImport : PXGraph<CABankTransactionsImport, CABankTranHeader>, PXImportAttribute.IPXPrepareItems
	{
		[Serializable]
		public partial class CABankTran2 : CABankTran
		{ }
		#region Public Selects
		public PXSelect<CABankTranHeader, Where<CABankTranHeader.cashAccountID, Equal<Optional<CABankTranHeader.cashAccountID>>, 
			And<CABankTranHeader.tranType, Equal<Current<CABankTranHeader.tranType>>>>, 
			OrderBy<Asc<CABankTranHeader.cashAccountID, Desc<CABankTranHeader.endBalanceDate>>>> Header;
        [PXImport(typeof(CABankTranHeader))]
		[PXCopyPasteHiddenFields(typeof(CABankTran.processed), typeof(CABankTran.documentMatched), typeof(CABankTran.createDocument),
			typeof(CABankTran.hidden), typeof(CABankTran.userDesc), typeof(CABankTran.acctName), typeof(CABankTran.payeeBAccountID1), 
			typeof(CABankTran.origModule1), typeof(CABankTran.payeeLocationID1), typeof(CABankTran.paymentMethodID1), typeof(CABankTran.entryTypeID1), 
			typeof(CABankTran.ruleID))]
		public PXSelect<CABankTran, Where<CABankTran.headerRefNbr, Equal<Current<CABankTranHeader.refNbr>>, 
			And<CABankTran.tranType, Equal<Current<CABankTranHeader.tranType>>>>> Details;
		public PXSelect<CABankTran2, Where<CABankTran2.headerRefNbr, Equal<Current<CABankTranHeader.refNbr>>,
			And<CABankTran2.tranType, Equal<Current<CABankTranHeader.tranType>>>>> SelectedDetail;
		public PXSelect<CABankTran, Where<CABankTran.headerRefNbr, Equal<Current<CABankTranHeader.refNbr>>,
			And<CABankTran.tranType, Equal<Current<CABankTranHeader.tranType>>,
			And<Where<CABankTran.processed, Equal<True>, Or<CABankTran.documentMatched, Equal<True>>>>>>> MatchedDetails;
		public PXSetup<CASetup> CASetup;
		public PXSelectReadonly<CashAccount, Where<CashAccount.extRefNbr, Equal<Optional<CashAccount.extRefNbr>>>> cashAccountByExtRef;
		public PXSelect<CABankTranMatch, Where<CABankTranMatch.tranID, Equal<Required<CABankTran.tranID>>>> TranMatch;
		public PXSelect<CABankTranAdjustment, Where<CABankTranAdjustment.tranID, Equal<Required<CABankTran.tranID>>>> TranAdj;
		public PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATran.tranID>>>> CATrans;
		public PXSelect<CABankTranDetail, Where<CABankTranDetail.bankTranID, Equal<Required<CABankTranDetail.bankTranID>>>> CABankTranSplits;
		public PXSelectJoin<CATran, InnerJoin<CABatchDetail, On<CATran.origModule, Equal<CABatchDetail.origModule>,
								And<CATran.origTranType, Equal<CABatchDetail.origDocType>,
								And<CATran.origRefNbr, Equal<CABatchDetail.origRefNbr>>>>>, Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>>> CATransInBatch;
		public PXSelect<EPExpenseClaimDetails> ExpenseReceipts;

		public PXSelect<CABankTranHeader> NewRevisionPanel;

		#endregion
		#region Actions
		public PXAction<CABankTranHeader> uploadFile;
		[PXUIField(DisplayName = Messages.UploadFile, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable UploadFile(PXAdapter adapter)
		{
			bool doImport = true;
			if (CASetup.Current.ImportToSingleAccount == true)
			{
				CABankTranHeader row = Header.Current;
				if (row == null || Header.Current.CashAccountID == null)
				{
					throw new PXException(Messages.CashAccountMustBeSelectedToImportStatement);
				}
				else
				{
					CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
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

			if (Header.Current != null && this.IsDirty == true)
			{
				if (CASetup.Current.ImportToSingleAccount != true)
				{
					if (Header.Ask(Messages.ImportConfirmationTitle, Messages.UnsavedDataInThisScreenWillBeLostConfirmation, MessageButtons.YesNo) != WebDialogResult.Yes)
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
					CABankTranHeader currHeader = Header.Current;
					const string PanelSessionKey = "ImportStatementProtoFile";
					PX.SM.FileInfo info = PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[PanelSessionKey] as PX.SM.FileInfo;
					System.Web.HttpContext.Current.Session.Remove(PanelSessionKey);
					ImportStatement(info, true);
					Save.Press();
					CABankTranHeader newRow = this.Header.Current;
					List<CABankTranHeader> result = new List<CABankTranHeader>();
					result.Add(newRow);
					return result;
				}
			}
			return adapter.Get();
		}

		public PXAction<CABankTranHeader> unhide;
		[PXUIField(DisplayName = Messages.UnhideTran, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable Unhide(PXAdapter adapter)
		{
			CABankTran detail = Details.Current;
			if (detail.Hidden == true)
			{
				detail.Hidden = false;
				detail.Processed = false;
                detail.RuleID = null;
				Details.Update(detail);
			}
			return adapter.Get();
		}

		public PXAction<CABankTranHeader> unmatch;
		[PXUIField(DisplayName = Messages.ClearMatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable Unmatch(PXAdapter adapter)
		{
			this.Save.Press();
			CABankTran detail = Details.Current;
			if (AskToUnmatchProcessedBankTransaction(detail))
			{
				PXLongOperation.StartOperation(this, delegate()
				{
					CABankTransactionsImport cABankTransactionsImport = PXGraph.CreateInstance<CABankTransactionsImport>();
					cABankTransactionsImport.UnmatchBankTran(detail, false);
				});	
			}
			return adapter.Get();
		}

		public PXAction<CABankTranHeader> unmatchAll;
		[PXUIField(DisplayName = Messages.ClearMatchAll, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable UnmatchAll(PXAdapter adapter)
		{
			this.Save.Press();
			CABankTranHeader cABankTranHeader = Header.Current;
			CABankTran cABankTran = PXSelect<CABankTran, Where<CABankTran.headerRefNbr, Equal<Required<CABankTranHeader.refNbr>>,
															And<CABankTran.tranType, Equal<Required<CABankTranHeader.tranType>>,
															And<CABankTran.processed, Equal<True>, And<CABankTran.documentMatched, Equal<True>>>>>>.Select(this, cABankTranHeader.RefNbr, cABankTranHeader.TranType);

			if(AskToUnmatchProcessedBankTransaction(cABankTran, unmatchAll: true))
			{
				PXLongOperation.StartOperation(this, delegate ()
				{
					CABankTransactionsImport cABankTransactionsImport = PXGraph.CreateInstance<CABankTransactionsImport>();
					cABankTransactionsImport.Header.Current = cABankTranHeader;
					cABankTransactionsImport.UnmatchAllProcess();
				});
			}
			return adapter.Get();
		}

		public PXAction<CABankTranHeader> viewDoc;
		[PXUIField(DisplayName = Messages.ViewMatchedDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			CABankTran detail = Details.Current;
			if (detail.DocumentMatched == true)
			{
				CABankTranMatch match = PXSelect<CABankTranMatch, Where<CABankTranMatch.tranID, Equal<Required<CABankTran.tranID>>, And<CABankTranMatch.tranType, Equal<Required<CABankTran.tranType>>>>>.Select(this, detail.TranID, detail.TranType);
				if (match != null)
				{
					CABankTranMatch.Redirect(this, match);
				}
			}
			return adapter.Get();
		}
		#endregion
		public CABankTransactionsImport()
        {
            PXUIFieldAttribute.SetVisible<CABankTran.invoiceInfo>(Details.Cache, null, true);
            PXUIFieldAttribute.SetVisible<CABankTran.extTranID>(Details.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<CABankTran.payeeBAccountID1>(Details.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<CABankTran.acctName>(Details.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<CABankTran.entryTypeID1>(Details.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<CABankTran.origModule1>(Details.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<CABankTran.paymentMethodID1>(Details.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<CABankTran.payeeLocationID1>(Details.Cache, null, true);
            PXUIFieldAttribute.SetVisible<CABankTran.userDesc>(Details.Cache,null,false);
            PXUIFieldAttribute.SetVisibility<CABankTran.userDesc>(Details.Cache, null, PXUIVisibility.Invisible);
			PXImportAttribute importAttribute = Details.Attributes.Find(a => a is PXImportAttribute) as PXImportAttribute;
			importAttribute.MappingPropertiesInit+=ImportAttributeMappingPropertiesInit;
        }

		
		#region Import-Related functions
		protected virtual void ImportAttributeMappingPropertiesInit(object sender, PXImportAttribute.MappingPropertiesInitEventArgs e)
		{
			HashSet<string> hiddenFields = new HashSet<string>();
			List<string> forcedFields = new List<string>();
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.payeeBAccountID)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.payeeBAccountIDCopy)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.processed)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.processed)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.createDocument)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.curyID)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.curyTranAmt)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.drCr)));

			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.headerRefNbr)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.payeeLocationID)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.payeeLocationID1)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.payeeLocationIDCopy)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.tranType)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.pMInstanceID)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.pMInstanceIDCopy)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.curyTotalAmt)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.paymentMethodID)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.paymentMethodID1)));
			hiddenFields.Add(Details.Cache.GetField(typeof(CABankTran.paymentMethodIDCopy)));

			hiddenFields.Add("CuryRate");
			hiddenFields.Add("CuryViewState");


			forcedFields.Add(Details.Cache.GetField(typeof(CABankTran.payeeName)));
			forcedFields.Add(Details.Cache.GetField(typeof(CABankTran.tranCode)));
			for (int i = 0; i < e.Names.Count; i++)
			{
				if (hiddenFields.Contains(e.Names[i]))
				{
					e.Names.RemoveAt(i);
					e.DisplayNames.RemoveAt(i);
					i--;
				}
			}
			foreach (string field in forcedFields)
			{
				if (!e.Names.Contains(field))
				{
					e.Names.Add(field);
					PXUIFieldAttribute uiAttribute = Details.Cache.GetAttributes(field).FirstOrDefault(a => a is PXUIFieldAttribute) as PXUIFieldAttribute;
					if (uiAttribute != null)
						e.DisplayNames.Add(uiAttribute.DisplayName);
					else e.DisplayNames.Add(field);
				}
			}
        }

		public virtual void ImportStatement(PX.SM.FileInfo aFileInfo, bool doRedirect)
		{
			bool isFormatRecognized = false;
			IStatementReader reader = this.CreateReader();
			if (reader != null && reader.IsValidInput(aFileInfo.BinData))
			{
				reader.Read(aFileInfo.BinData);
				List<CABankTranHeader> imported;
				reader.ExportToNew(aFileInfo, this, out imported);
				if (imported != null && doRedirect)
				{
					CABankTranHeader last = (imported != null && imported.Count > 0) ? imported[imported.Count - 1] : null;
					if (this.Header.Current == null || (last != null
						&& (this.Header.Current.CashAccountID != last.CashAccountID || this.Header.Current.RefNbr != last.RefNbr)))
					{
						this.Header.Current = this.Header.Search<CABankTranHeader.cashAccountID, CABankTranHeader.refNbr>(last.CashAccountID, last.RefNbr);
						throw new PXRedirectRequiredException(this, "Navigate to the uploaded record");
					}
				}
				isFormatRecognized = true;
			}
			if (!isFormatRecognized)
			{
				throw new PXException(Messages.UploadFileHasUnrecognizedBankStatementFormat);
			}
		}

		protected virtual IStatementReader CreateReader()
		{
			IStatementReader processor = null;
			bool importToSingleAccount = this.CASetup.Current.ImportToSingleAccount ?? false;
			string typeName = this.CASetup.Current.StatementImportTypeName;
			if (importToSingleAccount)
			{
				CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Optional<CABankTranHeader.cashAccountID>>>>.Select(this);
				if (acct != null)
					typeName = acct.StatementImportTypeName;
			}

			if (string.IsNullOrEmpty(typeName))
				return processor;
			try
			{
				Type processorType = System.Web.Compilation.PXBuildManager.GetType(typeName, true);
				processor = (IStatementReader)Activator.CreateInstance(processorType);
			}
			catch (Exception e)
			{
				throw new PXException(e, Messages.StatementServiceReaderCreationError, typeName);
			}
			return processor;
		}

		public bool IsAlreadyImported(int? aCashAccountID, string aExtTranID, out string aRefNbr)
		{
			aRefNbr = null;
			CABankTran detail = PXSelectReadonly<CABankTran,
											   Where<CABankTran.tranType, Equal<Current<CABankTranHeader.tranType>>,
												And<CABankTran.cashAccountID, Equal<Required<CABankTran.cashAccountID>>,
												And<CABankTran.extTranID, Equal<Required<CABankTran.extTranID>>>>>>.Select(this, aCashAccountID, aExtTranID);
			if (detail != null)
				aRefNbr = detail.TranID.ToString();
			return (detail != null);
		}

		#endregion

		public override void Persist()
		{
			List<CATran> list = new List<CATran>((IEnumerable<CATran>)this.CATrans.Cache.Cached);
			list.Concat_((IEnumerable<CATran>)this.CATransInBatch.Cache.Cached);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				base.Persist();

				foreach (CATran tran in list)
				{
					CAReconEntry.UpdateClearedOnSourceDoc(tran);
				}
				ts.Complete();
			}
		}

		#region Events
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDefault]
		[CashAccount(suppressActiveVerification: true, filterBranch:true, branchID: null, search: typeof(Search<CashAccount.cashAccountID,
				Where<CashAccount.active, Equal<True>,
					And<Match<Current<AccessInfo.userName>>>>>), IsKey = true, DisplayName = "Cash Account",
			Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr), Required = true)]
		protected virtual void CABankTranHeader_CashAccountID_CacheAttached(PXCache sender)
		{
		}
		protected virtual void CABankTranHeader_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABankTranHeader row = e.Row as CABankTranHeader;
			if (row == null) return;
			bool hasMatchedDetails = MatchedDetails.Any();
			PXUIFieldAttribute.SetEnabled<CABankTranHeader.docDate>(sender, row, !hasMatchedDetails);
			PXUIFieldAttribute.SetEnabled<CABankTranHeader.startBalanceDate>(sender, row, !hasMatchedDetails);
			PXUIFieldAttribute.SetEnabled<CABankTranHeader.curyBegBalance>(sender, row, !hasMatchedDetails);
			uploadFile.SetEnabled(CASetup.Current.ImportToSingleAccount != true || row.CashAccountID.HasValue);
			if (SelectedDetail.Current != null)
			{
				Details.Cache.ActiveRow = SelectedDetail.Current;
				SelectedDetail.Current = null;
			}

		}
		
		protected virtual void CABankTran_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row == null) return;
			if (Header.Current != null)
			{
				CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, Header.Current.CashAccountID);
				if (acct != null)
				{
					e.NewValue = acct.CuryID;
				}
			}
		}

		protected virtual void CABankTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled(sender, row, !(row.Processed == true || row.DocumentMatched == true));

			PXUIFieldAttribute.SetEnabled<CABankTran.processed>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CABankTran.documentMatched>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CABankTran.hidden>(sender, row, false);
            PXUIFieldAttribute.SetEnabled<CABankTran.ruleID>(sender, row, false);
		}

		protected virtual void CABankTran_TranDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			CABankTranHeader doc = this.Header.Current;
			if (row != null && doc != null)
			{
				e.NewValue = doc.TranMaxDate.HasValue ? doc.TranMaxDate : doc.DocDate;
				e.Cancel = true;
			}

		}

		protected virtual void CABankTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			CABankTran oldRow = e.OldRow as CABankTran;
			if (row.TranDate != oldRow.TranDate)
			{
				CABankTranHeader parent = this.Header.Current;
				if (parent != null)
				{
					if (!parent.TranMaxDate.HasValue || parent.TranMaxDate < row.TranDate)
					{
						parent.TranMaxDate = row.TranDate;
						this.Header.Update(parent);
					}
					else if (oldRow.TranDate.HasValue && parent.TranMaxDate == oldRow.TranDate)
					{
						CABankTran latest = PXSelect<CABankTran, Where<CABankTran.cashAccountID, Equal<Required<CABankTran.cashAccountID>>,
										 And<CABankTran.headerRefNbr, Equal<Required<CABankTran.headerRefNbr>>>>, OrderBy<Desc<CABankTran.tranDate>>>.Select(this, parent.CashAccountID, parent.RefNbr);
						parent.TranMaxDate = (latest != null ? latest.TranDate : null);
						this.Header.Update(parent);
					}

				}
			}
		}
		protected virtual void CABankTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			if (row.Processed == true || row.DocumentMatched == true)
				throw new PXSetPropertyException(Messages.CannotDeleteTran);
		}
		protected virtual void CABankTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			CABankTranHeader parent = this.Header.Current;
			if (parent != null && Header.Cache.GetStatus(parent) != PXEntryStatus.Deleted && parent.TranMaxDate.HasValue)
			{
				if (parent.TranMaxDate == row.TranDate.Value)
				{
					CABankTran latest = PXSelect<CABankTran, Where<CABankTran.cashAccountID, Equal<Required<CABankTran.cashAccountID>>,
															 And<CABankTran.headerRefNbr, Equal<Required<CABankTran.headerRefNbr>>>>, OrderBy<Desc<CABankTran.tranDate>>>.Select(this, parent.CashAccountID, parent.RefNbr);
					parent.TranMaxDate = (latest != null ? latest.TranDate : null);
					this.Header.Update(parent);
				}
			}
		}
		
		protected virtual void CABankTranHeader_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CABankTranHeader row = e.Row as CABankTranHeader;
			if (row == null) return;
			bool hasMatchedDetails = MatchedDetails.Any();
			if (hasMatchedDetails)
			{
				throw new PXSetPropertyException(Messages.CannotDeleteTranHeader);
			}
		}

		protected virtual void CABankTranHeader_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CABankTranHeader row = e.Row as CABankTranHeader;
			if (row == null) return;
			if (row.TranType == CABankTranType.Statement)
			{
				CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
					.Select(this, row.CashAccountID)?.First();
				if (cashAccount?.Active == false)
				{
					sender.RaiseExceptionHandling<CABankTranHeader.cashAccountID>(row, row.CashAccountID,
						new PXSetPropertyException(Messages.CashAccountInactive, PXErrorLevel.RowError, cashAccount.CashAccountCD));

				}
				if (row.CuryEndBalance != row.CuryDetailsEndBalance)
				{
					sender.RaiseExceptionHandling<CABankTranHeader.curyEndBalance>(row, row.CuryEndBalance,
						new PXSetPropertyException(Messages.EndBalanceDoesNotMatch, PXErrorLevel.Warning));
				}
				CABankTranHeader PrevRecord = PXSelect<CABankTranHeader,
					Where<CABankTranHeader.cashAccountID, Equal<Current<CABankTranHeader.cashAccountID>>,
						And<CABankTranHeader.tranType, Equal<Current<CABankTranHeader.tranType>>,
							And<CABankTranHeader.endBalanceDate, LessEqual<Current<CABankTranHeader.docDate>>,
								And<Where<Current<CABankTranHeader.refNbr>, IsNull, Or<CABankTranHeader.refNbr, NotEqual<Current<CABankTranHeader.refNbr>>>>>>>>,
					OrderBy<Desc<CABankTranHeader.startBalanceDate>>>.SelectWindowed(this, 0, 1);
				if (PrevRecord != null)
				{
					if (PrevRecord.CuryEndBalance != row.CuryBegBalance)
					{
						sender.RaiseExceptionHandling<CABankTranHeader.curyBegBalance>(row, row.CuryBegBalance,
							new PXSetPropertyException(Messages.BegBalanceDoesNotMatch, PXErrorLevel.Warning, PrevRecord.CuryEndBalance));
					}
					if (PrevRecord.EndBalanceDate != row.StartBalanceDate)
					{
						sender.RaiseExceptionHandling<CABankTranHeader.startBalanceDate>(row, row.EndBalanceDate,
							new PXSetPropertyException(Messages.BalanceDateDoesNotMatch, PXErrorLevel.Warning, PrevRecord.EndBalanceDate));
					}
				}
			}
		}

		protected virtual void CABankTranHeader_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankTranHeader row = e.Row as CABankTranHeader;
			if (row == null) return;

			CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
				.Select(this, e.NewValue)?.FirstOrDefault();
			if (cashAccount?.Active == false)
			{
				sender.RaiseExceptionHandling<CABankTranHeader.cashAccountID>(row, e.NewValue,
					new PXSetPropertyException(Messages.CashAccountInactive, PXErrorLevel.RowError, cashAccount.CashAccountCD));
				e.Cancel = true;
			}
		}

		#endregion

		#region IPXPrepareItems
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            return true;
		}

        public void PrepareItems(string viewName, IEnumerable items)
        {
        }

        public bool RowImported(string viewName, object row, object oldRow)
        {
            return oldRow == null;
        }

        public bool RowImporting(string viewName, object row)
        {
            return row == null;
        }
		#endregion
		#region Helpers
		private void UnmatchBankTran(CABankTran origTran, bool isMassRelease)
		{
			if (origTran.DocumentMatched != true)
			{
				return;
			}

			CABankTran copy = (CABankTran)Details.Cache.CreateCopy(origTran);
			copy.Processed = false;
			foreach (CABankTranMatch match in TranMatch.Select(copy.TranID))
			{
				if (match.DocModule == GL.BatchModule.AP && match.DocType == CATranType.CABatch)
				{
					foreach (CATran tran in CATransInBatch.Select(match.DocRefNbr))
					{
						if (tran != null && tran.TranID != null && tran.ReconNbr == null)
						{
							tran.ClearDate = null;
							tran.Cleared = false;
							CATransInBatch.Update(tran);
						}
					}
				}
				else
				{
					CATran tran = CATrans.Select(match.CATranID);
					if (tran != null && tran.TranID != null && tran.ReconNbr == null)
					{
						tran.ClearDate = null;
						tran.Cleared = false;
						CATrans.Update(tran);
					}
				}

				if (CABankTransactionsMaint.IsMatchedToExpenseReceipt(match))
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
			foreach (var adj in TranAdj.Select(copy.TranID))
			{
				TranAdj.Delete(adj);
			}
			foreach (var split in CABankTranSplits.Select(copy.TranID))
			{
				CABankTranSplits.Delete(split);
			}
			CABankTransactionsMaint.ClearFields(copy);
			Details.Cache.SetDefaultExt<CABankTran.curyApplAmt>(copy);
			Details.Cache.SetDefaultExt<CABankTran.curyApplAmtCA>(copy);
			Details.Cache.SetDefaultExt<CABankTran.curyApplAmtMatch>(copy);
			Details.Update(copy);
			if(isMassRelease == false)
			{
				this.Save.Press();
			}
		}
		private bool AskToUnmatchProcessedBankTransaction(CABankTran detail, bool unmatchAll = false)
		{
			if (detail != null && detail.DocumentMatched == true)
			{
				if (detail.Processed == true )
				{
					var unmatchHeader = unmatchAll ? Messages.ClearMatchAll : Messages.ClearMatch;
					var unmatchMessage = unmatchAll ? Messages.UnmatchAllTranMsg : Messages.UnmatchTranMsg;
					return Details.Ask(unmatchHeader, unmatchMessage, MessageButtons.OKCancel) == WebDialogResult.OK;
				}
				else
				{
					return true;
				}
			}
			return false;
		}
		private void UnmatchAllProcess()
		{
			foreach (var matchedLine in MatchedDetails.Select())
			{
				CABankTran detail = matchedLine;
				UnmatchBankTran(detail, true);
			}
			this.Save.Press();
		}
		#endregion
	}

	public class CABankTransactionsImportPayments : CABankTransactionsImport
	{
        [PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(CABankTranType.paymentImport))]
		[CABankTranType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Visible = false, TabOrder = 0)]
		protected virtual void CABankTranHeader_TranType_CacheAttached(PXCache sender) { }

        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Import Date")]
        protected virtual void CABankTranHeader_DocDate_CacheAttached(PXCache sender) { }

        [PXDBDate()]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Start Balance Date", Visible = false)]
        protected virtual void CABankTranHeader_StartBalanceDate_CacheAttached(PXCache sender) { }

        [PXDBDate()]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "End Balance Date", Visible = false)]
        protected virtual void CABankTranHeader_EndBalanceDate_CacheAttached(PXCache sender) { }

        [PXDBCury(typeof(CABankTranHeader.curyID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Beginning Balance", Visible = false)]
        protected virtual void CABankTranHeader_CuryBegBalance_CacheAttached(PXCache sender) { }

        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCury(typeof(CABankTranHeader.curyID))]
        [PXUIField(DisplayName = "Ending Balance", Visible = false)]
        protected virtual void CABankTranHeader_CuryEndBalance_CacheAttached(PXCache sender) { }
	}
}