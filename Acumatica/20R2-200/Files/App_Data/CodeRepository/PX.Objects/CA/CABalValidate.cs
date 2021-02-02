using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CA
{
	[TableAndChartDashboardType]
	public class CABalValidate : PXGraph<CABalValidate>
	{
		#region Internal Types Definition
		[Serializable]
		public partial class CABalanceValidationPeriodFilter : IBqlTable
		{
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

			[FinPeriodNonLockedSelector]			
			[PXUIField(DisplayName = GL.Messages.FinPeriod)]
			public virtual string FinPeriodID
			{
				get;
				set;
			}
		}
		#endregion
		public CABalValidate()
		{
			CASetup setup = CASetup.Current;
			CABalanceValidationPeriodFilter filter = PeriodsFilter.Current;
			CABalValidateList.SetProcessDelegate<CATranEntryLight>((CATranEntryLight graph, CashAccount cashAccount)=> Validate(graph, cashAccount, filter));

			CABalValidateList.SetProcessCaption(Messages.Validate);
			CABalValidateList.SetProcessAllCaption(Messages.ValidateAll);
			CABalValidateList.SuppressMerge = true;
			CABalValidateList.SuppressUpdate = true;

			PXUIFieldAttribute.SetEnabled<CashAccount.selected>(CABalValidateList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<CashAccount.cashAccountCD>(CABalValidateList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CashAccount.descr>(CABalValidateList.Cache, null, false);
		}

		public PXCancel<CABalanceValidationPeriodFilter> Cancel;

		public PXFilter<CABalanceValidationPeriodFilter> PeriodsFilter;

		[PXFilterable]
		public PXFilteredProcessing<CashAccount, CABalanceValidationPeriodFilter, Where<CashAccount.active, Equal<boolTrue>>> CABalValidateList;
		public PXSetup<CASetup> CASetup;

		public virtual void CABalanceValidationPeriodFilter_RowSelected(PXCache sender ,PXRowSelectedEventArgs e)
		{
			bool errorsOnForm = PXUIFieldAttribute.GetErrors(sender, null, PXErrorLevel.Error, PXErrorLevel.RowError).Count > 0;
			CABalValidateList.SetProcessEnabled(!errorsOnForm);
			CABalValidateList.SetProcessAllEnabled(!errorsOnForm);
		}

		private static void Validate(CATranEntryLight graph, CashAccount cashAccount, CABalanceValidationPeriodFilter filter)
		{
			if (string.IsNullOrEmpty(filter.FinPeriodID))
			{
				throw new PXException(GL.Messages.ProcessingRequireFinPeriodID);
			}

			MasterFinPeriod period = PXSelect<MasterFinPeriod, Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>.Select(graph, filter.FinPeriodID);
			if (period == null)
			{
				throw new PXException(GL.Messages.ProcessingRequireFinPeriodID);
			}

			ValidateAccount(graph, cashAccount);
			graph.Clear();

			ValidateCleared(graph, cashAccount, period);
			graph.Clear();

			ValidateCAAdjustments(graph, cashAccount, period);
			graph.Clear();

			ValidateCATransfers(graph, cashAccount, period);
			graph.Clear();

			ValidateCATrans(graph, cashAccount, period);
			graph.Clear();

			ValidateCADailySummary(graph, cashAccount, period);
		}
		private static void ValidateCADailySummary(CATranEntryLight graph, CashAccount cashAccount, IFinPeriod period)
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{

					graph.dailycache.Clear();
					ts.Complete(graph);
				}
			}

			PXDatabase.Delete<CADailySummary>(
					new PXDataFieldRestrict(nameof(CADailySummary.CashAccountID), PXDbType.Int, 4, cashAccount.CashAccountID, PXComp.EQ),
					new PXDataFieldRestrict(nameof(CADailySummary.TranDate), PXDbType.DateTime, 8, period.StartDate, PXComp.GE));

			foreach (CATran tran in PXSelect<CATran, 
				Where<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>, 
				And<CATran.tranDate, GreaterEqual<Required<CATran.tranDate>>>>>.Select(graph, cashAccount.CashAccountID, period.StartDate))
			{
				CADailyAccumulatorAttribute.RowInserted<CATran.tranDate>(graph.catrancache, tran);
			}

			graph.dailycache.Persist(PXDBOperation.Insert);
			graph.dailycache.Persist(PXDBOperation.Update);

			graph.dailycache.Persisted(false);

		}

		private static void ValidateCATrans(CATranEntryLight graph, CashAccount cashAccount, IFinPeriod period)
		{
			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.module>(graph.Caches[typeof(GLTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.batchNbr>(graph.Caches[typeof(GLTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.ledgerID>(graph.Caches[typeof(GLTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.finPeriodID>(graph.Caches[typeof(GLTran)], null, false);

			using (new PXConnectionScope())
			{
				const int rowsPerCycle = 10000;
				bool noMoreTran = false;
				int? lastGLTranIDOnPreviousStep = null;
				int previousCountRows = 0;

				while (!noMoreTran)
				{
					noMoreTran = true;
					int countRows = 0;
					int? lastGLTranID = null;
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						foreach (PXResult<GLTran, Ledger, Batch> res in PXSelectJoin<GLTran, 
									InnerJoin<Ledger, 
										On<Ledger.ledgerID, Equal<GLTran.ledgerID>>,
									InnerJoin<Batch,
										On<Batch.module, Equal<GLTran.module>, 
										And<Batch.batchNbr, Equal<GLTran.batchNbr>,
										And<Batch.scheduled, Equal<False>, And<Batch.voided, NotEqual<True>>>>>>>,
									Where<GLTran.accountID, Equal<Required<GLTran.accountID>>,
										And<GLTran.subID, Equal<Required<GLTran.subID>>,
										And<GLTran.branchID, Equal<Required<GLTran.branchID>>,
										And<Ledger.balanceType, Equal<LedgerBalanceType.actual>,
										//ignoring CM because DefaultValues always return null for CM
										And<GLTran.module, NotEqual<BatchModule.moduleCM>,
										And<GLTran.cATranID, IsNull,
										And<GLTran.tranPeriodID, GreaterEqual<Required<GLTran.tranPeriodID>>>>>>>>>, 
									OrderBy<Asc<GLTran.tranID>>>.SelectWindowed(graph, 0, rowsPerCycle, cashAccount.AccountID, cashAccount.SubID, cashAccount.BranchID, period.FinPeriodID))
						{
							GLTran gltran = (GLTran)res;
							lastGLTranID = gltran.TranID;
							noMoreTran = false;
							countRows++;
							CATran catran = GLCashTranIDAttribute.DefaultValues<GLTran.cATranID>(graph.gltrancache, gltran);
							if (catran != null)
							{
								long id;
								bool newCATRan = false;
								if (graph.catrancache.Locate(catran) == null)
								{
									catran = (CATran)graph.catrancache.Insert(catran);
									newCATRan = true;
									graph.catrancache.PersistInserted(catran);
									id = Convert.ToInt64(PXDatabase.SelectIdentity());
								}
								else
								{
									catran = (CATran)graph.catrancache.Update(catran);
									graph.catrancache.PersistUpdated(catran);
									id = catran.TranID.Value;
								}

								gltran.CATranID = id;
								graph.gltrancache.Update(gltran);

								if (catran.OrigModule != GLTranType.GLEntry)
								{
									switch (catran.OrigModule)
									{
										case BatchModule.AR:
											ARPayment arPayment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
												And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(graph, catran.OrigTranType, catran.OrigRefNbr);
											if (arPayment != null && (arPayment.CATranID == null || newCATRan))
											{
												arPayment.CATranID = id;
												arPayment = (ARPayment)graph.Caches[typeof(ARPayment)].Update(arPayment);
												graph.Caches[typeof(ARPayment)].PersistUpdated(arPayment);
											}
											break;
										case BatchModule.AP:
											APPayment apPayment = PXSelect<APPayment, Where<APPayment.docType, Equal<Required<APPayment.docType>>,
												And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(graph, catran.OrigTranType, catran.OrigRefNbr);
											if (apPayment != null && (apPayment.CATranID == null || newCATRan))
											{
												apPayment.CATranID = id;
												apPayment = (APPayment)graph.Caches[typeof(APPayment)].Update(apPayment);
												graph.Caches[typeof(APPayment)].PersistUpdated(apPayment);
											}
											break;
									}
								}
							}
						}

						if (!noMoreTran && countRows == previousCountRows && lastGLTranID == lastGLTranIDOnPreviousStep)
						{
							throw new PXException(Messages.ProcessCannotBeCompleted);
						}

						previousCountRows = countRows;
						lastGLTranIDOnPreviousStep = lastGLTranID;

						graph.gltrancache.ClearQueryCache();
						graph.gltrancache.Persist(PXDBOperation.Update);
						graph.gltrancache.Clear();
						graph.catrancache.Clear();
						graph.catrancache.ClearQueryCacheObsolete();
						graph.Caches[typeof(APPayment)].Clear();
						graph.Caches[typeof(ARPayment)].Clear();
						ts.Complete(graph);
					}
				}
				
				graph.gltrancache.Persisted(false);
				graph.catrancache.Persisted(false);
			}
		}

		private static void ValidateCATransfers(CATranEntryLight graph, CashAccount cashAccount, IFinPeriod period)
		{
			using (new PXConnectionScope())
			{
				PXCache transfercache = graph.Caches[typeof(CATransfer)];

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (PXResult<CATransfer, CATran> res in PXSelectJoin<CATransfer,
						LeftJoin<CATran,
							On<CATran.tranID, Equal<CATransfer.tranIDIn>>>,
						Where<CATransfer.inAccountID, Equal<Required<CATransfer.inAccountID>>,
						And<CATran.tranID, IsNull,
						And<CATransfer.inDate, GreaterEqual<Required<CATransfer.inDate>>>>>>.Select(graph, cashAccount.CashAccountID, period.StartDate))
					{
						CATransfer catransfer = (CATransfer)res;

						transfercache.SetValue<CATransfer.tranIDIn>(catransfer, null);
						transfercache.SetValue<CATransfer.clearedIn>(catransfer, false);
						if (transfercache.GetValue<CATransfer.clearedOut>(catransfer) == null)
						{
							transfercache.SetValue<CATransfer.clearedOut>(catransfer, false);
						}

						CATran catran = TransferCashTranIDAttribute.DefaultValues<CATransfer.tranIDIn>(transfercache, catransfer);

						if (catran != null)
						{
							catran = (CATran)graph.catrancache.Insert(catran);
							graph.catrancache.PersistInserted(catran);
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());

							transfercache.SetValue<CATransfer.tranIDIn>(catransfer, id);
							transfercache.Update(catransfer);
						}
					}

					foreach (PXResult<CATransfer, CATran> res in PXSelectJoin<CATransfer, 
						LeftJoin<CATran, 
							On<CATran.tranID, Equal<CATransfer.tranIDOut>>>, 
						Where<CATransfer.outAccountID, Equal<Required<CATransfer.outAccountID>>, 
						And<CATran.tranID, IsNull,
						And<CATransfer.outDate, GreaterEqual<Required<CATransfer.outDate>>>>>>.Select(graph, cashAccount.CashAccountID, period.StartDate))
					{
						CATransfer catransfer = (CATransfer)res;

						transfercache.SetValue<CATransfer.tranIDOut>(catransfer, null);
						transfercache.SetValue<CATransfer.clearedOut>(catransfer, false);
						if (transfercache.GetValue<CATransfer.clearedIn>(catransfer) == null)
						{
							transfercache.SetValue<CATransfer.clearedIn>(catransfer, false);
						}

						CATran catran = TransferCashTranIDAttribute.DefaultValues<CATransfer.tranIDOut>(transfercache, catransfer);

						if (catran != null)
						{
							catran = (CATran)graph.catrancache.Insert(catran);
							graph.catrancache.PersistInserted(catran);
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());

							transfercache.SetValue<CATransfer.tranIDOut>(catransfer, id);
							transfercache.Update(catransfer);
						}
					}

					transfercache.Persist(PXDBOperation.Update);

					ts.Complete(graph);
				}

				transfercache.Persisted(false);
				graph.catrancache.Persisted(false);
			}
		}

		private static void ValidateCAAdjustments(CATranEntryLight graph, CashAccount cashAccount, IFinPeriod period)
		{
			using (new PXConnectionScope())
			{
				PXCache adjcache = graph.Caches[typeof(CAAdj)];

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (PXResult<CAAdj, CATran> res in PXSelectJoin<CAAdj, 
							LeftJoin<CATran, 
								On<CATran.tranID, Equal<CAAdj.tranID>>,
							LeftJoin<GLTranDoc, 
								On<GLTranDoc.refNbr, Equal<CAAdj.adjRefNbr>, 
								And<GLTranDoc.tranType, Equal<CAAdj.adjTranType>, 
								And<GLTranDoc.tranModule, Equal<BatchModule.moduleCA>>>>>>,
							Where<CAAdj.cashAccountID, Equal<Required<CAAdj.cashAccountID>>, 
							And<CATran.tranID, IsNull, 
							And<GLTranDoc.refNbr, IsNull,
							And<CAAdj.tranPeriodID, GreaterEqual<Required<CAAdj.tranPeriodID>>>>>>>.Select(graph, cashAccount.CashAccountID, period.FinPeriodID))
					{
						CAAdj caadj = (CAAdj)res;

						GLTran gltran = PXSelectJoin<GLTran, InnerJoin<CashAccount,
								On<CashAccount.accountID, Equal<GLTran.accountID>,
								And<CashAccount.subID, Equal<GLTran.subID>,
								And<CashAccount.branchID, Equal<GLTran.branchID>>>>>,
							Where<GLTran.cATranID, Equal<Required<CAAdj.tranID>>,
								And<CashAccount.cashAccountID, Equal<Required<CAAdj.cashAccountID>>>>>.Select(graph, caadj.TranID, caadj.CashAccountID);

						adjcache.SetValue<CAAdj.tranID>(caadj, null);
						adjcache.SetValue<CAAdj.cleared>(caadj, false);

						CATran catran = AdjCashTranIDAttribute.DefaultValues<CAAdj.tranID>(adjcache, caadj);
						catran.BatchNbr = gltran?.BatchNbr;

						long? id = null;

						if (catran != null)
						{
							catran = (CATran)graph.catrancache.Insert(catran);
							graph.catrancache.PersistInserted(catran);
							id = Convert.ToInt64(PXDatabase.SelectIdentity());

							graph.SelectTimeStamp();

							adjcache.SetValue<CAAdj.tranID>(caadj, id);
							adjcache.Update(caadj);
						}

						if (id.HasValue && gltran?.TranID != null)
						{
							gltran.CATranID = id;
							graph.gltrancache.Update(gltran);
							graph.gltrancache.Persist(PXDBOperation.Update);
						}
					}

					adjcache.Persist(PXDBOperation.Update);

					ts.Complete(graph);
				}

				adjcache.Persisted(false);
				graph.catrancache.Persisted(false);
				graph.gltrancache.Persisted(false);
			}
		}

		private static void ValidateCleared(CATranEntryLight graph, CashAccount cashAccount, IFinPeriod period)
		{
			if (cashAccount.Reconcile != true)
			{
				graph.Clear();
				using (new PXConnectionScope())
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						foreach (CATran catran in PXSelect<CATran, Where<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>,
							And<CATran.tranPeriodID, GreaterEqual<Required<CATran.tranPeriodID>>>>>.Select(graph, cashAccount.CashAccountID, period.FinPeriodID))
						{
							if (cashAccount.Reconcile != true && (catran.Cleared != true || catran.TranDate == null))
							{
								catran.Cleared = true;
								catran.ClearDate = catran.TranDate;
							}
							graph.catrancache.Update(catran);
						}
						graph.catrancache.Persist(PXDBOperation.Update);
						ts.Complete(graph);
					}
					graph.catrancache.Persisted(false);
				}
			}
		}

		private static void ValidateAccount(CATranEntryLight graph, CashAccount cashAccount)
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					Account account = PXSelect<Account, Where<Account.isCashAccount, Equal<False>,
						And<Account.accountID, Equal<Required<Account.accountID>>>>>.Select(graph, cashAccount.AccountID);

					if (account != null)
					{
						account.IsCashAccount = true;
						graph.account.Update(account);
						graph.account.Cache.Persist(PXDBOperation.Update);
						graph.account.Cache.Persisted(false);
					}
					ts.Complete(graph);
				}
			}
		}
	}

	public class CATranEntryLight : CATranEntry
	{
		public class CATranEntryLightGLCATranToExpenseReceiptMatchingGraphExtension : CABankTransactionsMaint.GLCATranToExpenseReceiptMatchingGraphExtension<CATranEntryLight>
		{

		}

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Enabled = false)]
		public override void CATran_BatchNbr_CacheAttached(PXCache sender)
		{
		}

		public PXSelect<Account> account;
	}

}
