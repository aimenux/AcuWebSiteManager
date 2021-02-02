using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PX.Data;
using System.Collections;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common.Logging;
using PX.Objects.Common.Tools;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.GL.ConsolidationImport;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Reports;

namespace PX.Objects.GL
{
	[Serializable]
	[PXHidden]
	public partial class GLConsolRead : GLConsolData
	{
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		//[Account(typeof(Account.curyID))]
		//[PXDefault(typeof(Search<Account.accountID, Where<Account.accountCD, Equal<Current<ConsolRead.accountCD>>>>))]
        [PXInt]
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
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		//[SubAccount(typeof(GLTran.accountID))]
		//[PXDefault(typeof(Search<GLTran.subID, Where<GLTran.module, Equal<Current<GLTran.module>>, And<GLTran.batchNbr, Equal<Current<GLTran.batchNbr>>>>>))]
        [PXInt]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class GLConsolReadMaint : PXGraph<GLConsolReadMaint>
	{
		public PXCancel<GLConsolSetup> Cancel;
		[PXFilterable]
		public PXProcessing<GLConsolSetup,
			Where<GLConsolSetup.isActive, Equal<boolTrue>>>
			ConsolSetupRecords;
		public PXSelectOrderBy<GLConsolData,
			OrderBy<Asc<GLConsolData.finPeriodID>>> ConsolRecords;

		protected PXSelect<Segment,
			Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>>> SubaccountSegmentsView;

		#region Mappers

		protected virtual IExportSubaccountMapper CreateExportSubaccountMapper()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				var segments = SubaccountSegmentsView.Select().RowCast<Segment>().ToArray();

				var subaccountSegmentValuesView = new PXSelect<SegmentValue,
					Where<SegmentValue.dimensionID, Equal<SubAccountAttribute.dimensionName>>>(this);

				var segmentValues = subaccountSegmentValuesView.Select().RowCast<SegmentValue>();

				return new ExportSubaccountMapper(segments, segmentValues);
			}
			else
			{
				return new SubOffExportSubaccountMapper();
			}
		}

		protected virtual IImportSubaccountMapper CreateImportSubaccountMapper(GLConsolSetup glConsolSetup)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				var segments = SubaccountSegmentsView.Select().RowCast<Segment>().ToArray();

				return new ImportSubaccountMapper(segments, GLSetup.Current, glConsolSetup,
													subCD => SubAccountMaint.FindSubIDByCD(this, subCD),
													new AppLogger());
			}
			else
			{
				return new SubOffImportSubaccountMapper(SubAccountAttribute.TryGetDefaultSubID);
			}
		}

		#endregion

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		protected virtual IEnumerable consolRecords(
			[PXDBString]
			string ledgerCD,
			[PXDBString]
			string branchCD
			)
		{
			Ledger ledger = PXSelect<Ledger,
				Where<Ledger.consolAllowed, Equal<True>,
				And<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>>>>.Select(this, ledgerCD);

			if (ledger == null)
			{
				throw new PXException(Messages.CantFindConsolidationLedger, ledgerCD);
			}

			Branch branch = PXSelect<Branch,
				Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.Select(this, branchCD);

			if (!string.IsNullOrEmpty(branchCD) && branch == null)
			{
				throw new PXException(Messages.CantFindConsolidationBranch, branchCD);
			}

			if (branch != null && ledger.BalanceType != LedgerBalanceType.Report)
			{
				Organization organization = OrganizationMaint.FindOrganizationByID(this, branch.OrganizationID);

				if (organization.OrganizationType == OrganizationTypes.WithBranchesNotBalancing)
				{
					throw new PXException(Messages.BranchCannotBeConsolidated, branchCD);
				}
			}

			var exportSubaccountMapper = CreateExportSubaccountMapper();

			var noSegmentsToExport = false;
			if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				noSegmentsToExport = SubaccountSegmentsView.Select()
					.RowCast<Segment>()
					.All(segment => segment.ConsolNumChar <= 0);
			}

			PXSelectBase<GLHistory> cmd = new PXSelectJoin<GLHistory,
				InnerJoin<Account, On<Account.accountID, Equal<GLHistory.accountID>>,
				InnerJoin<Sub, On<Sub.subID, Equal<GLHistory.subID>>,
				InnerJoin<Ledger, On<Ledger.ledgerID, Equal<GLHistory.ledgerID>>,
				InnerJoin<Branch, On<Branch.branchID, Equal<GLHistory.branchID>>>>>>,
				Where<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>,
				And<GLHistory.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
				OrderBy<Asc<GLHistory.finPeriodID, Asc<Account.accountCD, Asc<Sub.subCD>>>>>(this);

			if (!string.IsNullOrEmpty(branchCD))
			{
				cmd.WhereAnd<Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>();
			}

			foreach (PXResult<GLHistory, Account, Sub> result in cmd.Select(ledgerCD, branchCD))
			{
				GLHistory history = result;
				Account account = result;
				Sub sub = result;

				string accountCD = account.GLConsolAccountCD;
				string subCD = exportSubaccountMapper.GetMappedSubaccountCD(sub);

				if (accountCD != null && accountCD.TrimEnd() != ""
					&& (subCD != null && subCD.TrimEnd() != "" || noSegmentsToExport))
				{
					GLConsolData consolData = new GLConsolData();
					consolData.MappedValue = subCD;
					consolData.AccountCD = accountCD;
					consolData.FinPeriodID = history.FinPeriodID;
					consolData = ConsolRecords.Locate(consolData);
					if (consolData != null)
					{
						consolData.ConsolAmtDebit += history.TranPtdDebit;
						consolData.ConsolAmtCredit += history.TranPtdCredit;
					}
					else
					{
						consolData = new GLConsolData();
						consolData.MappedValue = subCD;
						consolData.MappedValueLength = subCD.Length;
						consolData.AccountCD = accountCD;
						consolData.FinPeriodID = history.FinPeriodID;
						consolData.ConsolAmtDebit = history.TranPtdDebit;
						consolData.ConsolAmtCredit = history.TranPtdCredit;
						ConsolRecords.Insert(consolData);
					}
				}
			}

			return ConsolRecords.Cache.Inserted;
		}



		public GLConsolReadMaint()
		{
			GLSetup setup = GLSetup.Current;

			PXCache cache = ConsolSetupRecords.Cache;

			SubaccountSegmentsView =
				new PXSelect<Segment, Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>>>(this);

			PXUIFieldAttribute.SetEnabled<GLConsolSetup.description>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastConsDate>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastPostPeriod>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.ledgerId>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.login>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.password>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.pasteFlag>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.segmentValue>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.url>(cache, null, false);

			cache.AllowInsert = false;
			cache.AllowDelete = false;

			ConsolSetupRecords.SetProcessDelegate<GLConsolReadMaint>(ProcessConsolidationRead);
			ConsolSetupRecords.SetProcessAllVisible(false);
		}

		protected static void ProcessConsolidationRead(GLConsolReadMaint processor, GLConsolSetup item)
		{
			processor.Clear();
			processor.listConsolRead.Clear();
			int cnt = processor.ConsolidationRead(item);
			if (cnt > 0)
			{
				throw new PXSetPropertyException(Messages.NumberRecodsProcessed, PXErrorLevel.RowInfo, cnt);
			}
			else
			{
				throw new PXSetPropertyException(Messages.NoRecordsToProcess, PXErrorLevel.RowInfo);
			}
		}


		public List<GLConsolRead> listConsolRead = new List<GLConsolRead>();
		protected GLConsolSetup consolSetup = null;

		private GLConsolSetup DecryptRemoteUserPassword(GLConsolSetup item)
		{
			GLConsolSetup setup = item;
			var cache = Caches[typeof(GLConsolSetup)];
			PXDBCryptStringAttribute.SetDecrypted<GLConsolSetup.password>(cache, true);
			setup.Password = cache.GetValueExt<GLConsolSetup.password>(setup).ToString();
			PXDBCryptStringAttribute.SetDecrypted<GLConsolSetup.password>(cache, false);

			return setup;
		}

		protected virtual int ConsolidationRead(GLConsolSetup item)
		{
			int cnt = 0;

			string aFiscalPeriod = null;
			int? ledgerID = item.LedgerId;
			int? branchID = item.BranchID;

			var importSubaccountCDCalculator = CreateImportSubaccountMapper(item);

			var glConsolHistory = PXSelect<GLConsolHistory,
				Where<GLConsolHistory.setupID, Equal<Required<GLConsolHistory.setupID>>>>
				.Select(this, item.SetupID).ToList();

			var roundFunc = GetRoundDelegateForLedger(ledgerID);

			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			if (item.BypassAccountSubValidation == true)
			{
				je.FieldVerifying.AddHandler<GLTran.accountID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				je.FieldVerifying.AddHandler<GLTran.subID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			}

			consolSetup = DecryptRemoteUserPassword(item);

			using (PXSoapScope scope = new PXSoapScope(consolSetup.Url, consolSetup.Login, consolSetup.Password))
			{
				GLConsolReadMaint reader = PXGraph.CreateInstance<GLConsolReadMaint>();
				reader.ConsolRecords.Select(item.SourceLedgerCD, item.SourceBranchCD);
				scope.Process(reader);

				int min = 0;
				if (!String.IsNullOrEmpty(item.StartPeriod))
				{
					int.TryParse(item.StartPeriod, out min);
				}
				int max = 0;
				if (!String.IsNullOrEmpty(item.EndPeriod))
				{
					int.TryParse(item.EndPeriod, out max);
				}
				foreach (GLConsolData row in reader.ConsolRecords.Select())
				{
					if (min > 0 || max > 0)
					{
						if (!String.IsNullOrEmpty(row.FinPeriodID))
						{
							int p;
							if (int.TryParse(row.FinPeriodID, out p))
							{
								if (min > 0 && p < min || max > 0 && p > max)
								{
									continue;
								}
							}
						}
					}
					if (aFiscalPeriod == null)
					{
						aFiscalPeriod = row.FinPeriodID;
					}
					else if (aFiscalPeriod != row.FinPeriodID)
					{
						if (listConsolRead.Count > 0)
						{
							cnt += AppendRemapped(aFiscalPeriod, ledgerID, branchID, item.SetupID, roundFunc);
							CreateBatch(je, aFiscalPeriod, ledgerID, branchID, item);
						}
						aFiscalPeriod = row.FinPeriodID;
						listConsolRead.Clear();
					}

					GLConsolRead read = new GLConsolRead();

					var account = AccountMaint.GetAccountByCD(this, row.AccountCD);

					if (account.AccountID == GLSetup.Current.YtdNetIncAccountID)
					{
						throw new PXException(Messages.ImportingYTDNetIncomeAccountDataIsProhibited);
					}

					read.AccountCD = account.AccountCD;
					read.AccountID = account.AccountID;

					var mappedValue = GetMappedValue(row);
					var subKeys = importSubaccountCDCalculator.GetMappedSubaccountKeys(mappedValue);

					read.MappedValue = subKeys.SubCD;
					read.SubID = subKeys.SubID;
					read.FinPeriodID = row.FinPeriodID;

					GLConsolHistory history = new GLConsolHistory();
					history.SetupID = item.SetupID;
					history.FinPeriodID = read.FinPeriodID;
					history.AccountID = read.AccountID;
					history.SubID = read.SubID;
					history.LedgerID = item.LedgerId;
					history.BranchID = item.BranchID;
					history = (GLConsolHistory)Caches[typeof(GLConsolHistory)].Locate(history);

					if (history != null)
					{
						read.ConsolAmtCredit = roundFunc(row.ConsolAmtCredit) - roundFunc(history.PtdCredit);
						read.ConsolAmtDebit = roundFunc(row.ConsolAmtDebit) - roundFunc(history.PtdDebit);
						history.PtdCredit = 0m;
						history.PtdDebit = 0m;
					}
					else
					{
						read.ConsolAmtCredit = roundFunc(row.ConsolAmtCredit);
						read.ConsolAmtDebit = roundFunc(row.ConsolAmtDebit);
					}

					if (read.ConsolAmtCredit != 0m || read.ConsolAmtDebit != 0m)
					{
						listConsolRead.Add(read);
						cnt++;
					}
				}
			}

			if (listConsolRead.Count > 0)
			{
				cnt += AppendRemapped(aFiscalPeriod, ledgerID, branchID, item.SetupID, roundFunc);
				CreateBatch(je, aFiscalPeriod, ledgerID, branchID, item);
			}

			if (exception != null)
			{
				PXException ex = exception;
				exception = null;
				throw ex;
			}

			return cnt;
		}

		public PXSetup<GLSetup> GLSetup;

		public int AppendRemapped(string periodId, int? ledgerID, int? branchID, int? setupID, Func<decimal?, decimal> roundFunc)
		{
			int ret = 0;
			foreach (GLConsolHistory history in Caches[typeof(GLConsolHistory)].Cached)
			{
				if (history.SetupID == setupID
					&& history.FinPeriodID == periodId
					&& history.LedgerID == ledgerID
					&& history.BranchID == branchID
					&& (roundFunc(history.PtdCredit) != 0m || roundFunc(history.PtdDebit) != 0m))
				{
					GLConsolRead read = new GLConsolRead();
					read.AccountID = history.AccountID;
					read.SubID = history.SubID;
					read.FinPeriodID = history.FinPeriodID;
					read.ConsolAmtCredit = -roundFunc(history.PtdCredit);
					read.ConsolAmtDebit = -roundFunc(history.PtdDebit);
					listConsolRead.Add(read);
					ret++;
				}
			}
			return ret;
		}

		public void CreateBatch(JournalEntry je, string periodId, int? ledgerID, int? branchID, GLConsolSetup item)
		{
			je.Clear();

			je.glsetup.Current.RequireControlTotal = false;

			Ledger ledger = PXSelect<Ledger,
				Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>
				.Select(this, ledgerID);

			FinPeriod finPeriod = FinPeriodRepository.FindByID(PXAccess.GetParentOrganizationID(branchID), periodId);

			if (finPeriod == null)
			{
				throw new FiscalPeriodInvalidException(periodId);
			}

			je.Accessinfo.BusinessDate = finPeriod.EndDate.Value.AddDays(-1);

			CurrencyInfo info = new CurrencyInfo();
			info.CuryID = ledger.BaseCuryID;
			info.CuryEffDate = finPeriod.EndDate.Value.AddDays(-1);
			info.CuryRate = (decimal)1.0;
			info = je.currencyinfo.Insert(info);

			Batch batch = new Batch();
			batch.BranchID = branchID;
			batch.LedgerID = ledgerID;
			batch.Module = BatchModule.GL;
			batch.Hold = false;
			batch.Released = false;
			batch.CuryID = ledger.BaseCuryID;
			batch.CuryInfoID = info.CuryInfoID;
			batch.FinPeriodID = periodId;
			batch.CuryID = ledger.BaseCuryID;
			batch.BatchType = BatchTypeCode.Consolidation;
			batch.Description = PXMessages.LocalizeFormatNoPrefix(Messages.ConsolidationBatch, item.Description);
			batch = je.BatchModule.Insert(batch);

			foreach (GLConsolRead read in listConsolRead)
			{
				Action<decimal?, decimal?> insertTransaction = (debitAmt, creditAmt) =>
				{
					GLTran tran = new GLTran();

					tran.AccountID = read.AccountID;
					tran.SubID = read.SubID;
					tran.CuryInfoID = info.CuryInfoID;
					tran.CuryCreditAmt = creditAmt;
					tran.CuryDebitAmt = debitAmt;
					tran.CreditAmt = creditAmt;
					tran.DebitAmt = debitAmt;
					tran.TranType = GLTran.tranType.Consolidation;
					tran.TranClass = GLTran.tranClass.Consolidation;
					tran.TranDate = finPeriod.EndDate.Value.AddDays(-1);
					tran.TranDesc = Messages.ConsolidationDetail;
					tran.FinPeriodID = periodId;
					tran.RefNbr = "";
					tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
					tran = je.GLTranModuleBatNbr.Insert(tran);

					if (tran != null && tran.SubID == null && read.MappedValue != null)
					{
						je.GLTranModuleBatNbr.SetValueExt<GLTran.subID>(tran, read.MappedValue);
					}

					if (tran == null || tran.AccountID == null || tran.SubID == null)
					{
						throw new PXException(Messages.AccountOrSubNotFound, read.AccountCD, read.MappedValue);
					}
				};

				if (Math.Abs((decimal)read.ConsolAmtDebit) > 0)
				{
					insertTransaction(read.ConsolAmtDebit, 0m);
				}

				if (Math.Abs((decimal)read.ConsolAmtCredit) > 0)
				{
					insertTransaction(0m, read.ConsolAmtCredit);
				}
			}

			item.LastPostPeriod = finPeriod.FinPeriodID;
			item.LastConsDate = DateTime.Now;
			je.Caches[typeof(GLConsolSetup)].Update(item);
			if (!je.Views.Caches.Contains(typeof(GLConsolSetup)))
			{
				je.Views.Caches.Add(typeof(GLConsolSetup));
			}
			GLConsolBatch cb = new GLConsolBatch();
			cb.SetupID = item.SetupID;
			je.Caches[typeof(GLConsolBatch)].Insert(cb);
			if (!je.Views.Caches.Contains(typeof(GLConsolBatch)))
			{
				je.Views.Caches.Add(typeof(GLConsolBatch));
			}

			try
			{
				je.Save.Press();
			}
			catch (PXException e)
			{
				try
				{
					if (!String.IsNullOrEmpty(PXUIFieldAttribute.GetError<Batch.curyCreditTotal>(je.BatchModule.Cache, je.BatchModule.Current))
						|| !String.IsNullOrEmpty(PXUIFieldAttribute.GetError<Batch.curyDebitTotal>(je.BatchModule.Cache, je.BatchModule.Current)))
					{
						je.BatchModule.Current.Hold = true;
						je.BatchModule.Update(je.BatchModule.Current);
					}
					je.Save.Press();
					if (exception == null)
					{
						exception = new PXException(Messages.ConsolidationBatchOutOfBalance, je.BatchModule.Current.BatchNbr);
					}
					else
					{
						exception = new PXException(exception.Message + Messages.ConsolidationBatchOutOfBalance, je.BatchModule.Current.BatchNbr);
					}
				}
				catch
				{
					throw e;
				}
			}
		}

		public PXException exception;

		protected virtual void GLConsolSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			if (!PXLongOperation.Exists(UID))
			{
				var glConsolSetup = (GLConsolSetup)e.Row;

				CheckUnpostedBatchesNotExist(glConsolSetup);
			}
		}

		private void CheckUnpostedBatchesNotExist(GLConsolSetup glConsolSetup)
		{
			var getUnpostedBatchesQuery = new PXSelectJoin<Batch,
															InnerJoin<GLConsolBatch,
																On<Batch.batchNbr, Equal<GLConsolBatch.batchNbr>>>,
															Where
																<GLConsolBatch.setupID, Equal<Required<GLConsolBatch.setupID>>,
																And<Batch.posted, Equal<False>,
																And<Batch.module, Equal<BatchModule.moduleGL>>>>>(this);

			var pars = new List<object>()
			{
				glConsolSetup.SetupID
			};

			if (glConsolSetup.StartPeriod != null)
			{
				getUnpostedBatchesQuery.WhereAnd<Where<Batch.finPeriodID, GreaterEqual<Required<Batch.finPeriodID>>>>();
				pars.Add(glConsolSetup.StartPeriod);
			}
			if (glConsolSetup.EndPeriod != null)
			{
				getUnpostedBatchesQuery.WhereAnd<Where<Batch.finPeriodID, LessEqual<Required<Batch.finPeriodID>>>>();
				pars.Add(glConsolSetup.EndPeriod);
			}

			var unpostedBatch = (Batch) getUnpostedBatchesQuery.Select(pars.ToArray());

			if (unpostedBatch != null)
			{
				ConsolSetupRecords.Cache.RaiseExceptionHandling<GLConsolSetup.selected>(glConsolSetup, glConsolSetup.Selected,
					new PXSetPropertyException(Messages.UnpostedBatchesExist, PXErrorLevel.RowError));
			}
		}

		public Func<decimal?, decimal> GetRoundDelegateForLedger(int? ledgerID)
		{
			var currency = (Currency)PXSelectJoin<Currency,
				InnerJoin<Ledger,
					On<Currency.curyID, Equal<Ledger.baseCuryID>>>,
				Where
					<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>
				.Select(this, ledgerID);

			if (currency == null)
			{
				throw new PXException(
					PXMessages.LocalizeFormat(Messages.CurrencyForLedgerCannotBeFound, ledgerID));
			}

			return value => Math.Round(value.Value, currency.DecimalPlaces.Value, MidpointRounding.AwayFromZero);
		}

		private string GetMappedValue(GLConsolData data)
		{
			return data.MappedValue.PadRight(data.MappedValueLength.Value, ' ');
		}
	}
}
