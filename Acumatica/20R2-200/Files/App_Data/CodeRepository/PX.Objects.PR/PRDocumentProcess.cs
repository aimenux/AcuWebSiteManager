using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRDocumentProcess : PXGraph<PRDocumentProcess>
	{
		public PXCancel<PRDocumentProcessFilter> Cancel;

		public PXFilter<PRDocumentProcessFilter> Filter;
		public PXFilteredProcessing<PRPayment, PRDocumentProcessFilter> DocumentList;

		public override bool IsDirty => false;

		protected IEnumerable documentList()
		{
			PRDocumentProcessFilter filter = Filter.Current;
			var paymentQuery = new SelectFrom<PRPayment>
				.Where<PRPayment.status.IsEqual<P.AsString>>.View(this);

			switch (filter?.Operation)
			{
				case PRDocumentProcessFilter.operation.PutOnHold:
					paymentQuery.WhereOr(typeof(Where<PRPayment.status, Equal<PaymentStatus.pendingPrintOrPayment>>));
					return paymentQuery.Select(PaymentStatus.NeedCalculation);
				case PRDocumentProcessFilter.operation.RemoveFromHold:
					return paymentQuery.Select(PaymentStatus.Hold);
				case PRDocumentProcessFilter.operation.Calculate:
				case PRDocumentProcessFilter.operation.Recalculate:
					return SelectFrom<PRPayment>.
						Where<PRPayment.calculated.IsEqual<P.AsBool>.
							And<PRPayment.hold.IsEqual<False>>.
							And<PRPayment.printed.IsEqual<False>>.
							And<PRPayment.released.IsEqual<False>>.
							And<PRPayment.voided.IsEqual<False>>>.View.Select(this, filter.Operation != PRDocumentProcessFilter.operation.Calculate);
				case PRDocumentProcessFilter.operation.Release:
					paymentQuery.WhereAnd(typeof(Where<PRPayment.docType, NotEqual<PayrollType.voidCheck>>));
					paymentQuery.WhereOr(typeof(Where<PRPayment.docType.IsEqual<PayrollType.voidCheck>.And<PRPayment.released.IsEqual<False>>>));
					return paymentQuery.Select(PaymentStatus.CheckPrintedOrPaid);
				case PRDocumentProcessFilter.operation.Void:
					HashSet<string> unreleasedVoidChecks = SelectFrom<PRPayment>
						.Where<PRPayment.docType.IsEqual<PayrollType.voidCheck>
							.And<PRPayment.released.IsNotEqual<True>>>.View.Select(this).FirstTableItems.Select(x => x.RefNbr).ToHashSet();
					paymentQuery.WhereAnd(typeof(Where<PRPayment.docType, NotEqual<PayrollType.voidCheck>>));
					return paymentQuery.Select(PaymentStatus.Released).FirstTableItems.Where(x => !unreleasedVoidChecks.Contains(x.RefNbr));
			}

			return new List<PRPayment>();
		}

		protected static void ProcessPayments(List<PRPayment> list, string operation)
		{
			PRPayChecksAndAdjustments payCheckGraph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
			foreach (PRPayment payment in list)
			{
				try
				{
					switch (operation)
					{
						case PRDocumentProcessFilter.operation.PutOnHold:
							payment.Hold = true;
							payCheckGraph.Document.Update(payment);
							payCheckGraph.Persist();
							break;
						case PRDocumentProcessFilter.operation.RemoveFromHold:
							payment.Hold = false;
							payCheckGraph.Document.Update(payment);
							payCheckGraph.Persist();
							break;
						case PRDocumentProcessFilter.operation.Calculate:
						case PRDocumentProcessFilter.operation.Recalculate:
							payCheckGraph.CurrentDocument.Current = payment;
							payCheckGraph.Calculate.Press();
							break;
						case PRDocumentProcessFilter.operation.Release:
							payCheckGraph.CurrentDocument.Current = payment;
							payCheckGraph.Release.Press();
							break;
						case PRDocumentProcessFilter.operation.Void:
							payCheckGraph.CurrentDocument.Current = payment;
							payCheckGraph.VoidPayment.Press();
							payCheckGraph.Save.Press();
							break;
					}
				}
				catch (Exception ex)
				{
					PXProcessing<PRPayment>.SetError(list.IndexOf(payment), ex);
				}
			}
		}

		#region Events
		protected virtual void _(Events.RowSelected<PRDocumentProcessFilter> e)
		{
			PRDocumentProcessFilter row = e.Row as PRDocumentProcessFilter;
			if (row == null)
			{
				return;
			}

			DocumentList.SetProcessDelegate(list => ProcessPayments(list, row.Operation));
		}
		#endregion

		#region Static release methods
		public static DocumentList<Batch> ReleaseDoc(List<PRPayment> list, bool isMassProcess)
		{
			return ReleaseDoc(list, isMassProcess, true);
		}

		public static DocumentList<Batch> ReleaseDoc(List<PRPayment> list, bool isMassProcess, bool autoPost)
		{
			bool failed = false;

			PRReleaseProcess rg = PXGraph.CreateInstance<PRReleaseProcess>();
			JournalEntry je = rg.UpdateGL ? CreateJournalEntryGraph() : null;
			PostGraph pg = rg.UpdateGL ? PXGraph.CreateInstance<PostGraph>() : null;

			List<int> batchbind = new List<int>();

			DocumentList<Batch> batchlist = new DocumentList<Batch>(rg);

			for (int i = 0; i < list.Count; i++)
			{
				PRPayment doc = list[i];
				try
				{
					rg.Clear();
					rg.VerifyPayment(doc);
					doc = rg.OnBeforeRelease(doc);
					rg.ReleaseDocProc(je, doc);

					if (rg.UpdateGL)
					{
						if (je.BatchModule.Current != null && batchlist.Find(je.BatchModule.Current) == null)
						{
							batchlist.Add(je.BatchModule.Current);
							batchbind.Add(i);
						} 
					}

					if (isMassProcess)
					{
						PXProcessing<PRPayment>.SetInfo(i, ActionsMessages.RecordProcessed);
					}
				}
				catch (Exception e)
				{
					if (rg.UpdateGL)
					{
						batchlist.Remove(je.BatchModule.Current);
						je.Clear();
					}

					Exception exception = e;
					if (e is PXFieldValueProcessingException && e.InnerException is PXTaskIsCompletedException taskIsCompletedException)
					{
						PXResult<PMTask, PMProject> query =
							(PXResult<PMTask, PMProject>) SelectFrom<PMTask>
								.InnerJoin<PMProject>.On<PMTask.projectID.IsEqual<PMProject.contractID>>
								.Where<PMTask.taskID.IsEqual<P.AsInt>>
								.View.SelectSingleBound(rg, null, taskIsCompletedException.TaskID);

						PMTask task = query;
						PMProject project = query;
						
						if (task != null && project != null)
							exception = new PXException(e, Messages.ProjectTaskIsNotActive, task.TaskCD, project.ContractCD);
					}

					if (isMassProcess)
					{
						PXProcessing<PRPayment>.SetError(i, exception);
						failed = true;
					}
					else
					{
						throw new Common.PXMassProcessException(i, exception);
					}
				}
			}

			if (rg.UpdateGL)
			{
				for (int i = 0; i < batchlist.Count; i++)
				{
					Batch batch = batchlist[i];
					try
					{
						if (rg.AutoPost && autoPost)
						{
							pg.Clear();
							pg.PostBatchProc(batch);
						}
					}
					catch (Exception e)
					{
						if (isMassProcess)
						{
							failed = true;
							PXProcessing<PRPayment>.SetError(batchbind[i], e);
						}
						else
						{
							throw new Common.PXMassProcessException(batchbind[i], e);
						}
					}
				} 
			}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
			return rg.AutoPost && rg.UpdateGL ? batchlist : new DocumentList<Batch>(rg);
		}
		#endregion Static release methods

		/// <summary>
		/// Creates a JournalEntry graph instance without some restrictions (like PXRestrictorAttribute), which prevents releasing documents.
		/// </summary>
		private static JournalEntry CreateJournalEntryGraph()
		{
			var graph = PXGraph.CreateInstance<JournalEntry>();
			foreach (PXRestrictorAttribute restrictorAttribute in graph.Caches[typeof(GLTran)].GetAttributesOfType<PXRestrictorAttribute>(null, nameof(GLTran.projectID)))
			{
				var bql = BqlCommand.Decompose(restrictorAttribute.RestrictingCondition);
				if (bql.Contains(typeof(PMProject.isCompleted)) || bql.Contains(typeof(PMProject.isActive)) || bql.Contains(typeof(PMProject.isCancelled)))
				{
					restrictorAttribute.SuppressVerify = true;
				}
			}

			BaseProjectTaskAttribute taskAttribute = graph.Caches[typeof(GLTran)].GetAttributesOfType<BaseProjectTaskAttribute>(null, nameof(GLTran.taskID)).Single();
			taskAttribute.SuppressVerify = true;

			return graph;
		}
	}

	[PXHidden]
	public class PRReleaseProcess : PXGraph<PRReleaseProcess>
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region Views
		public PXSelect<PRPayment> PRDocument;
		public SelectFrom<PRPayment>
			.Where<PRPayment.docType.IsEqual<P.AsString>
			.And<PRPayment.refNbr.IsEqual<P.AsString>>>.View SpecificPayment;
		public PXSelect<Batch> Batch;
		public PXSetup<GLSetup> GLSetup;
		public PXSetup<PRSetup> PRSetup;

		public PXSelect<CATran> CashTran;
		public PXSelect<PRYtdEarnings> YtdEarnings;
		public PXSelect<PRYtdDeductions> YtdDeductions;
		
		public SelectFrom<PRYtdTaxes>
			.Where<PRYtdTaxes.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRYtdTaxes.taxID.IsEqual<P.AsInt>.And<PRYtdTaxes.year.IsEqual<P.AsString>>>>.View YtdTaxes;

		public PTOHelper.EmployeePTOHistorySelect.View PTOHistory;

		public PXSelect<PRPeriodTaxes> PeriodTaxes;

		public SelectFrom<PREarningDetail>.
					Where<PREarningDetail.employeeID.IsEqual<P.AsInt>.
					And<PREarningDetail.paymentDocType.IsEqual<P.AsString>>.
					And<PREarningDetail.paymentRefNbr.IsEqual<P.AsString>>>.
					OrderBy<Asc<PREarningDetail.date>>.View Earnings;

		public SelectFrom<PRDeductionDetail>.
			InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionDetail.codeID>>.
			Where<PRDeductionDetail.employeeID.IsEqual<P.AsInt>.
				And<PRDeductionDetail.paymentDocType.IsEqual<P.AsString>.
				And<PRDeductionDetail.paymentRefNbr.IsEqual<P.AsString>>>>.View DeductionDetails;

		public SelectFrom<PRBenefitDetail>.
			InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRBenefitDetail.codeID>>.
			Where<PRBenefitDetail.employeeID.IsEqual<P.AsInt>.
				And<PRBenefitDetail.paymentDocType.IsEqual<P.AsString>.
				And<PRBenefitDetail.paymentRefNbr.IsEqual<P.AsString>>>>.View BenefitDetails;

		public SelectFrom<PRTaxDetail>.
			InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRTaxDetail.taxID>>.
			Where<PRTaxDetail.employeeID.IsEqual<P.AsInt>.
				And<PRTaxDetail.paymentDocType.IsEqual<P.AsString>.
				And<PRTaxDetail.paymentRefNbr.IsEqual<P.AsString>>>>.View TaxDetails;

		public SelectFrom<PRPaymentEarning>.
				Where<PRPaymentEarning.docType.IsEqual<P.AsString>.
				And<PRPaymentEarning.refNbr.IsEqual<P.AsString>>>.View SummaryEarnings;
		public SelectFrom<PRPaymentDeduct>.
				Where<PRPaymentDeduct.docType.IsEqual<P.AsString>.
				And<PRPaymentDeduct.refNbr.IsEqual<P.AsString>.
				And<PRPaymentDeduct.isActive.IsEqual<True>>>>.View SummaryDeductions;


		public SelectFrom<PRPaymentTax>.
				Where<PRPaymentTax.docType.IsEqual<P.AsString>.
				And<PRPaymentTax.refNbr.IsEqual<P.AsString>>>.View SummaryTaxes;

		public SelectFrom<PREmployeeDeduct>.
			InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PREmployeeDeduct.codeID>>.
			Where<PREmployeeDeduct.bAccountID.IsEqual<P.AsInt>.
			And<PREmployeeDeduct.codeID.IsEqual<P.AsInt>.
			And<PRDeductCode.isGarnishment.IsEqual<True>>>>.View Garnishment;

		public SelectFrom<PRPaymentPTOBank>.
				Where<PRPaymentPTOBank.docType.IsEqual<P.AsString>.
				And<PRPaymentPTOBank.refNbr.IsEqual<P.AsString>>>.View PTOBanks;
		public SelectFrom<PRPayGroupYear>
			.Where<PRPayGroupYear.payGroupID.IsEqual<P.AsString>
			.And<PRPayGroupYear.year.IsEqual<P.AsString>>>.View PayrollYear;

		public SelectFrom<PRPayGroupPeriod>
			.Where<PRPayGroupPeriod.payGroupID.IsEqual<P.AsString>
			.And<PRPayGroupPeriod.finPeriodID.IsEqual<P.AsString>>>.View PayPeriod;

		public SelectFrom<PRBatch>.Where<PRBatch.batchNbr.IsEqual<P.AsString>>.View PayBatches;

		public SelectFrom<PRPayment>.Where<PRPayment.released.IsEqual<False>.
			And<PRPayment.payBatchNbr.IsEqual<P.AsString>>>.View NonReleasedPayBatchPayments;

		public SelectFrom<PRDirectDepositSplit>
			.Where<PRDirectDepositSplit.docType.IsEqual<P.AsString>
				.And<PRDirectDepositSplit.refNbr.IsEqual<P.AsString>>>.View DirectDepositSplits;
		#endregion Views

		public bool AutoPost
		{
			get
			{
				return (bool)PRSetup.Cache.GetValue<PRSetup.autoPost>(PRSetup.Current);
			}
		}

		public bool UpdateGL
		{
			get
			{
				return (bool)PRSetup.Cache.GetValue<PRSetup.updateGL>(PRSetup.Current);
			}
		}

		public virtual void ReleaseDocProc(JournalEntry je, PRPayment doc)
		{
			if (doc.Released != true)
			{
				CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, doc.CuryInfoID);
				if (UpdateGL)
				{
					SegregateBatch(je, doc.BranchID, info.CuryID, doc.TransactionDate, doc.FinPeriodID, doc.DocDesc, info);
				}
				var isDebit = doc.DrCr == GL.DrCr.Debit;

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (UpdateGL)
					{
						//Credit cash account for the amount equal to net pay total
						IEnumerable<GLTran> transactions = WriteNetPayTransactions(doc, info);
						foreach (var tran in transactions)
						{
							je.GLTranModuleBatNbr.Insert(tran);
						}
					}

					//Loop over earnings, debit line account for line amount
					foreach (IGrouping<Tuple<string, int?>, PXResult<PREarningDetail>> groupedEarningDetails in Earnings.Select(doc.EmployeeID, doc.DocType, doc.RefNbr)
						.GroupBy(x => new Tuple<string, int?>(((PREarningDetail)x).TypeCD, ((PREarningDetail)x).LocationID)))
					{
						foreach (PREarningDetail earningDetail in groupedEarningDetails)
						{
							if (UpdateGL)
							{
								//Insert GLTran
								GLTran earningTran = WriteEarning(doc, earningDetail, info, je.BatchModule.Current);
								earningTran = je.GLTranModuleBatNbr.Insert(earningTran);
							}

							earningDetail.Released = true;
							Earnings.Update(earningDetail);
						}

						//Insert YTDEarnings
						var ytdEarning = new PRYtdEarnings();
						ytdEarning.Year = doc.TransactionDate.Value.Year.ToString();
						ytdEarning.Month = doc.TransactionDate.Value.Month;
						ytdEarning.EmployeeID = doc.EmployeeID;
						ytdEarning.TypeCD = groupedEarningDetails.Key.Item1;
						ytdEarning.LocationID = groupedEarningDetails.Key.Item2;
						ytdEarning.Amount = (isDebit ? 1 : -1) * groupedEarningDetails.Sum(x => ((PREarningDetail)x).Amount ?? 0m);
						ytdEarning = YtdEarnings.Insert(ytdEarning);
					}

					foreach (PXResult<PRDeductionDetail, PRDeductCode> deductionDetail in DeductionDetails.Select(doc.EmployeeID, doc.DocType, doc.RefNbr))
					{
						if (UpdateGL)
						{
							//Insert GLTran
							GLTran deductionTran = WriteDeduction(doc, deductionDetail, deductionDetail, info, je.BatchModule.Current);
							je.GLTranModuleBatNbr.Insert(deductionTran);
						}

						((PRDeductionDetail)deductionDetail).Released = true;
						DeductionDetails.Update(deductionDetail);
					}

					foreach (PXResult<PRBenefitDetail, PRDeductCode> benefitDetail in BenefitDetails.Select(doc.EmployeeID, doc.DocType, doc.RefNbr))
					{
						if (UpdateGL)
						{
							//Insert GLTran
							GLTran expenseTran = WriteBenefitExpense(doc, benefitDetail, benefitDetail, info, je.BatchModule.Current);
							je.GLTranModuleBatNbr.Insert(expenseTran);
							if (((PRDeductCode)benefitDetail).IsPayableBenefit != true)
							{
								GLTran liabilityTran = WriteBenefitLiability(doc, benefitDetail, benefitDetail, info, je.BatchModule.Current);
								je.GLTranModuleBatNbr.Insert(liabilityTran);
							}
						}

						((PRBenefitDetail)benefitDetail).Released = true;
						BenefitDetails.Update(benefitDetail);
					}

					foreach (PXResult<PRTaxDetail, PRTaxCode> taxDetail in TaxDetails.Select(doc.EmployeeID, doc.DocType, doc.RefNbr))
					{
						if (UpdateGL)
						{
							//Insert GLTran
							GLTran liabilityTran = WriteTaxLiability(doc, taxDetail, taxDetail, info, je.BatchModule.Current);
							je.GLTranModuleBatNbr.Insert(liabilityTran);
							if (((PRTaxDetail)taxDetail).TaxCategory == TaxCategory.EmployerTax)
							{
								GLTran expenseTran = WriteTaxExpense(doc, taxDetail, taxDetail, info, je.BatchModule.Current);
								je.GLTranModuleBatNbr.Insert(expenseTran);
							}
						}

						((PRTaxDetail)taxDetail).Released = true;
						TaxDetails.Update(taxDetail);
					}

					//Insert YTDDeductions and update garnishment info
					foreach (IGrouping<int?, PXResult<PRPaymentDeduct>> groupedDeductions in SummaryDeductions.Select(doc.DocType, doc.RefNbr).GroupBy(x => ((PRPaymentDeduct)x).CodeID))
					{
						var ytdDeduction = new PRYtdDeductions();
						ytdDeduction.Year = doc.TransactionDate.Value.Year.ToString();
						ytdDeduction.EmployeeID = doc.EmployeeID;
						ytdDeduction.CodeID = groupedDeductions.Key;
						ytdDeduction.Amount = (isDebit ? 1 : -1) * groupedDeductions.Sum(x => ((PRPaymentDeduct)x).DedAmount ?? 0m);
						ytdDeduction.EmployerAmount = (isDebit ? 1 : -1) * groupedDeductions.Sum(x => ((PRPaymentDeduct)x).CntAmount ?? 0m);
						YtdDeductions.Insert(ytdDeduction);

						var employeeDeduct = (PREmployeeDeduct)Garnishment.SelectSingle(doc.EmployeeID, groupedDeductions.Key);
						if (employeeDeduct != null)
						{
							decimal delta = groupedDeductions.Sum(x => ((PRPaymentDeduct)x).DedAmount ?? 0m);
							employeeDeduct.GarnPaidAmount = (employeeDeduct.GarnPaidAmount ?? 0m) + (isDebit ? 1 : -1) * delta;
							Garnishment.Update(employeeDeduct);
						}
					}

					PRPayGroupYear payYear = PayrollYear.SelectSingle(doc.PayGroupID, doc.TransactionDate.Value.Year.ToString());
					DayOfWeek payWeekStart = payYear.StartDate.Value.DayOfWeek == DayOfWeek.Saturday ? DayOfWeek.Sunday : payYear.StartDate.Value.DayOfWeek + 1;
					PRPayGroupPeriod payPeriod = PayPeriod.SelectSingle(doc.PayGroupID, doc.PayPeriodID);

					// Insert Period and YTD Taxes
					foreach (PRPaymentTax taxLine in SummaryTaxes.Select(doc.DocType, doc.RefNbr))
					{
						var ytdTax = new PRYtdTaxes();
						ytdTax.Year = doc.TransactionDate.Value.Year.ToString();
						ytdTax.EmployeeID = doc.EmployeeID;
						ytdTax.TaxID = taxLine.TaxID;
						ytdTax.TaxableWages = (isDebit ? 1 : -1) * taxLine.WageBaseAmount;
						if (isDebit)
						{
							ytdTax.MostRecentWH = taxLine.TaxAmount;
						}
						YtdTaxes.Insert(ytdTax);

						var periodTax = new PRPeriodTaxes();
						periodTax.Year = doc.TransactionDate.Value.Year.ToString();
						periodTax.EmployeeID = doc.EmployeeID;
						periodTax.TaxID = taxLine.TaxID;
						periodTax.PeriodNbr = payPeriod.PeriodNbrAsInt;
						periodTax.Amount = (isDebit ? 1 : -1) * taxLine.TaxAmount;
						periodTax.Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(doc.TransactionDate.Value, CalendarWeekRule.FirstDay, payWeekStart);
						periodTax.Month = doc.TransactionDate.Value.Month;
						PeriodTaxes.Insert(periodTax);
					}

					ProcessPTO(doc);					
					doc.Released = true;
					doc.HasUpdatedGL = UpdateGL;
					if (UpdateGL)
					{
						if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
						{
							je.Save.Press();
						}

						if (!je.BatchModule.Cache.IsDirty && string.IsNullOrEmpty(doc.BatchNbr))
						{
							doc.BatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
						}
					}

					doc = (PRPayment)PRDocument.Cache.Update(doc);

					ClosePayBatchIfAllPaymentsAreReleased(doc.PayBatchNbr);
					if (doc.DocType == PayrollType.VoidCheck)
					{
						UpdateVoidedCheck(doc);
					}

					//After release, update MTD, QTD and YTD Amounts.
					foreach (PRPaymentEarning summary in SummaryEarnings.Select(doc.DocType, doc.RefNbr))
					{
						PRPayChecksAndAdjustments.UpdateSummaryEarning(this, doc, summary);
						summary.MTDAmount += summary.Amount;
						summary.QTDAmount += summary.Amount;
						summary.YTDAmount += summary.Amount;
						SummaryEarnings.Update(summary);
					}
					foreach (PRPaymentDeduct summary in SummaryDeductions.Select(doc.DocType, doc.RefNbr))
					{
						PRPayChecksAndAdjustments.UpdateSummaryDeductions(this, doc, summary);
						summary.YtdAmount += summary.DedAmount ?? 0m;
						summary.EmployerYtdAmount += summary.CntAmount ?? 0m;
						SummaryDeductions.Update(summary);
					}
					foreach (PRPaymentTax summary in SummaryTaxes.Select(doc.DocType, doc.RefNbr))
					{
						decimal amount = YtdTaxes.Select(summary.TaxID, doc.TransactionDate.Value.Year).TopFirst?.Amount ?? 0;
						summary.YtdAmount = amount + (summary.TaxAmount ?? 0m);
						SummaryTaxes.Update(summary);
					}

					this.Actions.PressSave();
					ts.Complete(this);
				}
			}
		}


		public virtual PRPayment OnBeforeRelease(PRPayment payment)
		{
			return payment;
		}

		private void UpdateVoidedCheck(PRPayment voidcheck)
		{
			foreach (PRPayment res in SpecificPayment.Select(voidcheck.OrigDocType, voidcheck.OrigRefNbr))
			{
				PRPayment payment = res;
				PRPayment cached = (PRPayment)PRDocument.Cache.Locate(payment);
				if (cached != null)
				{
					payment = cached;
				}

				payment.Voided = true;
				payment.Hold = false;
				PRDocument.Cache.Update(payment);
			}
		}

		private void ClosePayBatchIfAllPaymentsAreReleased(string payBatchNumber)
		{
			if (string.IsNullOrWhiteSpace(payBatchNumber))
				return;

			PRPayment nonReleasedPaymentInBatch = NonReleasedPayBatchPayments.SelectSingle(payBatchNumber);
			if (nonReleasedPaymentInBatch != null)
				return;

			PRBatch batch = PayBatches.Select(payBatchNumber);
			batch.Closed = true;
			PayBatches.Update(batch);
		}

		#region Helpers

		public virtual IEnumerable<GLTran> WriteNetPayTransactions(PRPayment doc, CurrencyInfo currencyInfo)
		{
			var paymentMethod = (PaymentMethod)PXSelectorAttribute.Select<PRPayment.paymentMethodID>(PRDocument.Cache, doc);
			var paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();
			var cashAccount = (CashAccount)PXSelectorAttribute.Select<PRPayment.cashAccountID>(PRDocument.Cache, doc);
			var isDebit = doc.DrCr == GL.DrCr.Debit;
			var transactions = new List<GLTran>();

			if (paymentMethodExt.PRCreateBatchPayment == false || !DirectDepositSplits.Select(doc.DocType, doc.RefNbr).FirstTableItems.Any())
			{
				//Print check method
				GLTran tran = new GLTran();
				tran.SummPost = PRSetup.Current.SummPost;
				tran.BranchID = cashAccount.BranchID;
				tran.AccountID = cashAccount.AccountID;
				tran.SubID = cashAccount.SubID;
				tran.ReclassificationProhibited = true;
				tran.CuryDebitAmt = isDebit ? 0m : doc.NetAmount;
				tran.DebitAmt = isDebit ? 0m : doc.NetAmount;
				tran.CuryCreditAmt = isDebit ? doc.NetAmount : 0m;
				tran.CreditAmt = isDebit ? doc.NetAmount : 0m;
				tran.TranType = doc.DocType;
				tran.RefNbr = doc.RefNbr;
				tran.TranDesc = doc.DocDesc;
				tran.TranPeriodID = doc.PayPeriodID;
				tran.FinPeriodID = doc.FinPeriodID;
				tran.TranDate = doc.TransactionDate;
				tran.CuryInfoID = currencyInfo.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : doc.EmployeeID;
				tran.CATranID = doc.CATranID;
				transactions.Add(tran);
			}
			else
			{
				//Direct Deposit method
				foreach (PRDirectDepositSplit split in DirectDepositSplits.Select(doc.DocType, doc.RefNbr))
				{
					GLTran tran = new GLTran();
					tran.SummPost = false;
					tran.BranchID = cashAccount.BranchID;
					tran.AccountID = cashAccount.AccountID;
					tran.SubID = cashAccount.SubID;
					tran.ReclassificationProhibited = true;
					tran.CuryDebitAmt = isDebit ? 0m : split.Amount;
					tran.DebitAmt = isDebit ? 0m : split.Amount;
					tran.CuryCreditAmt = isDebit ? split.Amount : 0m;
					tran.CreditAmt = isDebit ? split.Amount : 0m;
					tran.TranType = doc.DocType;
					tran.RefNbr = doc.RefNbr;
					tran.TranPeriodID = doc.PayPeriodID;
					tran.FinPeriodID = doc.FinPeriodID;
					tran.TranDate = doc.TransactionDate;
					tran.CuryInfoID = currencyInfo.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : doc.EmployeeID;
					tran.CATranID = split.CATranID;
					tran.TranDesc = split.BankRoutingNbr + split.BankAcctNbr;
					transactions.Add(tran);
				}
			}

			return transactions;
		}

		private GLTran WriteEarning(PRPayment payment, PREarningDetail earningDetail, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			PRxGLTran glTranExt = PXCache<GLTran>.GetExtension<PRxGLTran>(tran);
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = earningDetail.BranchID;
			tran.AccountID = earningDetail.AccountID;
			tran.SubID = earningDetail.SubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? earningDetail.Amount : 0m;
			tran.DebitAmt = isDebit ? earningDetail.Amount : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : earningDetail.Amount;
			tran.CreditAmt = isDebit ? 0m : earningDetail.Amount;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.EarningDescriptionFormat, earningDetail.TypeCD, earningDetail.Date.Value.ToString("d"));
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : earningDetail.EmployeeID;
			tran.ProjectID = CostAssignmentType.GetEarningSetting(payment.LaborCostSplitType).AssignCostToProject && earningDetail.ProjectID != null ? earningDetail.ProjectID : ProjectDefaultAttribute.NonProject();
			tran.TaskID = CostAssignmentType.GetEarningSetting(payment.LaborCostSplitType).AssignCostToProject ? earningDetail.ProjectTaskID : null;
			tran.CostCodeID = earningDetail.CostCodeID;
			tran.InventoryID = earningDetail.LabourItemID;
			tran.Qty = earningDetail.UnitType == UnitType.Hour ? earningDetail.Hours :
				earningDetail.UnitType == UnitType.Misc ? earningDetail.Units : 1;
			glTranExt.PayrollWorkLocationID = earningDetail.LocationID;
			glTranExt.EarningTypeCD = earningDetail.TypeCD;
			return tran;
		}

		private GLTran WriteDeduction(PRPayment payment, PRDeductionDetail deductionDetail, PRDeductCode deductCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = deductionDetail.BranchID;
			tran.AccountID = deductionDetail.AccountID;
			tran.SubID = deductionDetail.SubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? 0m : deductionDetail.Amount;
			tran.DebitAmt = isDebit ? 0m : deductionDetail.Amount;
			tran.CuryCreditAmt = isDebit ? deductionDetail.Amount : 0m;
			tran.CreditAmt = isDebit ? deductionDetail.Amount : 0m;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.DeductionLiabilityFormat, deductCode.CodeCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : deductionDetail.EmployeeID;
			tran.Qty = 1;
			return tran;
		}

		private GLTran WriteBenefitExpense(PRPayment payment, PRBenefitDetail benefitDetail, PRDeductCode deductCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = benefitDetail.BranchID;
			tran.AccountID = benefitDetail.ExpenseAccountID;
			tran.SubID = benefitDetail.ExpenseSubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.DebitAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.CreditAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.BenefitExpenseFormat, deductCode.CodeCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : benefitDetail.EmployeeID;
			tran.ProjectID = CostAssignmentType.GetBenefitSetting(payment.LaborCostSplitType).AssignCostToProject && benefitDetail.ProjectID != null ? benefitDetail.ProjectID : ProjectDefaultAttribute.NonProject();
			tran.TaskID = CostAssignmentType.GetBenefitSetting(payment.LaborCostSplitType).AssignCostToProject ? benefitDetail.ProjectTaskID : null;
			tran.CostCodeID = benefitDetail.CostCodeID;
			tran.InventoryID = benefitDetail.LabourItemID;
			tran.Qty = 1;
			return tran;
		}

		private GLTran WriteBenefitLiability(PRPayment payment, PRBenefitDetail benefitDetail, PRDeductCode deductCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = benefitDetail.BranchID;
			tran.AccountID = benefitDetail.LiabilityAccountID;
			tran.SubID = benefitDetail.LiabilitySubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.DebitAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.CuryCreditAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.CreditAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.BenefitLiabilityFormat, deductCode.CodeCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : benefitDetail.EmployeeID;
			tran.Qty = 1;
			return tran;
		}

		private GLTran WriteTaxExpense(PRPayment payment, PRTaxDetail taxDetail, PRTaxCode taxCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = taxDetail.BranchID;
			tran.AccountID = taxDetail.ExpenseAccountID;
			tran.SubID = taxDetail.ExpenseSubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? taxDetail.Amount : 0m;
			tran.DebitAmt = isDebit ? taxDetail.Amount : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : taxDetail.Amount;
			tran.CreditAmt = isDebit ? 0m : taxDetail.Amount;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TaxExpenseFormat, taxCode.TaxCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : taxDetail.EmployeeID;
			tran.ProjectID = CostAssignmentType.GetTaxSetting(payment.LaborCostSplitType).AssignCostToProject && taxDetail.ProjectID != null ? taxDetail.ProjectID : ProjectDefaultAttribute.NonProject();
			tran.TaskID = CostAssignmentType.GetTaxSetting(payment.LaborCostSplitType).AssignCostToProject ? taxDetail.ProjectTaskID : null;
			tran.CostCodeID = taxDetail.CostCodeID;
			tran.InventoryID = taxDetail.LabourItemID;
			tran.Qty = 1;
			return tran;
		}

		private GLTran WriteTaxLiability(PRPayment payment, PRTaxDetail taxDetail, PRTaxCode taxCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = taxDetail.BranchID;
			tran.AccountID = taxDetail.LiabilityAccountID;
			tran.SubID = taxDetail.LiabilitySubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? 0m : taxDetail.Amount;
			tran.DebitAmt = isDebit ? 0m : taxDetail.Amount; ;
			tran.CuryCreditAmt = isDebit ? taxDetail.Amount : 0m;
			tran.CreditAmt = isDebit ? taxDetail.Amount : 0m;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TaxLiabilityFormat, taxCode.TaxCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : taxDetail.EmployeeID;
			tran.Qty = 1;
			return tran;
		}

		/// <summary>
		/// Insert accumulated and used PTO
		/// </summary>
		public virtual void ProcessPTO(PRPayment doc)
		{
			var isDebit = doc.DrCr == GL.DrCr.Debit;
			foreach (PRPaymentPTOBank paymentBank in PTOBanks.Select(doc.DocType, doc.RefNbr))
			{
				var result = PTOHelper.PTOBankSelect.View.Select(this, doc.EmployeeID, paymentBank.BankID)
					.Select(x => (PXResult<PRPTOBank, PREmployee, PREmployeeClassPTOBank, PREmployeePTOBank>)x).First();
				IPTOBank sourceBank = PTOHelper.GetSourceBank(result, result, result);

				PTOHelper.GetPTOBankYear(doc.TransactionDate.Value, sourceBank.StartDate.Value, out DateTime startDate, out DateTime endDate);
				PTOHelper.GetPTOHistory(this, doc.TransactionDate.Value, doc.EmployeeID.Value, sourceBank, out decimal accumulatedToDate, out decimal usedToDate, out decimal availableToDate);
				decimal newAccrualAmount = paymentBank.IsActive == true ? paymentBank.TotalAccrual.GetValueOrDefault() : 0m;
				paymentBank.AccumulatedAmount = accumulatedToDate + newAccrualAmount;
				paymentBank.UsedAmount = usedToDate + paymentBank.TotalDisbursement.GetValueOrDefault();
				if(sourceBank.DisburseFromCarryover == true)
				{
					availableToDate += paymentBank.CarryoverAmount.GetValueOrDefault();
				}
				else
				{
					availableToDate += newAccrualAmount;
				}
				paymentBank.AvailableAmount = availableToDate - paymentBank.TotalDisbursement.GetValueOrDefault();
				PTOBanks.Update(paymentBank);
			}
		}

		private void SegregateBatch(JournalEntry je, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo)
		{
			JournalEntry.SegregateBatch(je, BatchModule.PR, branchID, curyID, docDate, finPeriodID, description, curyInfo, null);
		}

		public virtual void VerifyPayment(PRPayment doc)
		{
			if (doc.GrossAmount.GetValueOrDefault() == 0 && doc.DocType != PayrollType.Adjustment && doc.DocType != PayrollType.VoidCheck)
			{
				throw new PXException(Messages.CantReleasePaymentWithoutGrossPay);
			}
		}

		#endregion Helpers
	}

	[Serializable]
	[PXHidden]
	public class PRDocumentProcessFilter : IBqlTable
	{
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation>
		{
			public const string PutOnHold = "HLD";
			public const string RemoveFromHold = "RHL";
			public const string Calculate = "CAL";
			public const string Recalculate = "REC";
			public const string Release = "REL";
			public const string Void = "VOI";
		}
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Action")]
		[PXStringList(
			new string[] { operation.PutOnHold, operation.RemoveFromHold, operation.Calculate, operation.Recalculate, operation.Release, operation.Void },
			new string[] { Messages.PutOnHoldAction, Messages.RemoveFromHoldAction, Messages.Calculate, Messages.Recalculate, Messages.Release, Messages.Void })]
		public string Operation { get; set; }
		#endregion
	}
}
