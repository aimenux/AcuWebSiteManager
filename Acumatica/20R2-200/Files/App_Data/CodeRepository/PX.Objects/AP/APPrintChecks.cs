using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Web;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.AP.MigrationMode;
using PX.Common;
using PX.Objects.Common;

namespace PX.Objects.AP
{
	//DO NOT LOCALIZE, DO NOT MODIFY
	public static class ReportMessages
	{
		public const string CheckReportFlag = "PrintFlag";
		public const string CheckReportFlagValue = "PRINT";
	}

	[TableAndChartDashboardType]
	public class APPrintChecks : PXGraph<APPrintChecks>
	{
		public PXFilter<PrintChecksFilter> Filter;
		public PXCancel<PrintChecksFilter> Cancel;
		public PXAction<PrintChecksFilter> ViewDocument;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (APPaymentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(APPaymentList.Cache, APPaymentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXFilterable]
		public PXFilteredProcessingJoin<APPayment, PrintChecksFilter,
			InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>,
			Where<boolTrue, Equal<boolTrue>>,
			OrderBy<Asc<Vendor.acctName, Asc<APPayment.refNbr>>>> APPaymentList;

		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		#region Setups
		public APSetupNoMigrationMode APSetup;
		public PXSetup<Company> Company;
		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Current<PrintChecksFilter.payAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Current<PrintChecksFilter.payTypeID>>>>> cashaccountdetail;
		public PXSetup<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PrintChecksFilter.payTypeID>>>> paymenttype;
		#endregion

		public APPrintChecks()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled(APPaymentList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.selected>(APPaymentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(APPaymentList.Cache, null, true);

			APPaymentList.SetSelected<APPayment.selected>();
			PXUIFieldAttribute.SetDisplayName<APPayment.vendorID>(APPaymentList.Cache, Messages.VendorID);
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[APDocType.List]
		protected virtual void APPayment_DocType_CacheAttached(PXCache sender) { }

		private bool cleared;
		public override void Clear()
		{
			Filter.Current.CurySelTotal = 0m;
			Filter.Current.SelTotal = 0m;
			Filter.Current.SelCount = 0;
			cleared = true;
			base.Clear();
		}

		private readonly Dictionary<object, object> _copies = new Dictionary<object, object>();

		protected virtual IEnumerable appaymentlist()
		{
			if (cleared)
			{
				foreach (APPayment doc in APPaymentList.Cache.Updated)
				{
					doc.Passed = false;
				}
			}

			foreach (PXResult<APPayment, Vendor, PaymentMethod, CABatchDetail> doc in PXSelectJoin<APPayment,
				InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>,
				InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<APPayment.paymentMethodID>>,
				LeftJoin<CABatchDetail, On<CABatchDetail.origModule, Equal<BatchModule.moduleAP>,
						And<CABatchDetail.origDocType, Equal<APPayment.docType>,
						And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>>>>,
				Where2<Where<APPayment.status, Equal<APDocStatus.pendingPrint>,
					And<CABatchDetail.batchNbr, IsNull,
					And<APPayment.cashAccountID, Equal<Current<PrintChecksFilter.payAccountID>>,
					And<APPayment.paymentMethodID, Equal<Current<PrintChecksFilter.payTypeID>>,
					And<Match<Vendor, Current<AccessInfo.userName>>>>>>>,
					And<APPayment.docType, In3<APDocType.check, APDocType.prepayment, APDocType.quickCheck>>>>.Select(this))
			{
				yield return new PXResult<APPayment, Vendor>(doc, doc);
				if (_copies.ContainsKey((APPayment)doc))
				{
					_copies.Remove((APPayment)doc);
				}
				_copies.Add((APPayment)doc, PXCache<APPayment>.CreateCopy(doc));
			}
		}

		public virtual void AssignNumbers(APPaymentEntry pe, APPayment doc, ref string NextCheckNbr, bool skipStubs = false)
		{
			pe.RowPersisting.RemoveHandler<APAdjust>(pe.APAdjust_RowPersisting);
			pe.Clear();
			doc = pe.Document.Current = pe.Document.Search<APPayment.refNbr>(doc.RefNbr, doc.DocType);
			PaymentMethodAccount det = pe.cashaccountdetail.Select();

			doc.IsPrintingProcess = true; // indicates that the payment under printing process (to prevent update by PaymentRefAttribute)

			if (string.IsNullOrEmpty(NextCheckNbr))
			{
				throw new PXException(Messages.NextCheckNumberIsRequiredForProcessing);
			}

			if (string.IsNullOrEmpty(pe.Document.Current.ExtRefNbr))
			{
				pe.Document.Current.StubCntr = 1;
				pe.Document.Current.BillCntr = 0;
				pe.Document.Current.ExtRefNbr = NextCheckNbr;
				pe.Document.Update(pe.Document.Current);

				if (pe.Document.Current.DocType.IsIn(APDocType.QuickCheck, APDocType.Prepayment) && pe.Document.Current.CuryOrigDocAmt <= 0m)
				{
					throw new PXException(Messages.ZeroCheck_CannotPrint);
				}

				if (!skipStubs) // print check
				{
					pe.DeletePrintList();
					var adjustments = pe.GetAdjustmentsPrintList();

					PaymentMethod pt = pe.paymenttype.Select();
					int stubCount = (int)Math.Ceiling(adjustments.Count() / (decimal)(pt.APStubLines ?? 1));
					if (stubCount > 1 && pt.APPrintRemittance != true)
					{
						string endNumber = AutoNumberAttribute.NextNumber(NextCheckNbr, stubCount - 1);
						string[] duplicates = PXSelect<CashAccountCheck,
							Where<CashAccountCheck.accountID, Equal<Required<PrintChecksFilter.payAccountID>>,
								And<CashAccountCheck.paymentMethodID, Equal<Required<PrintChecksFilter.payTypeID>>,
								And<CashAccountCheck.checkNbr, GreaterEqual<Required<PrintChecksFilter.nextCheckNbr>>,
								And<CashAccountCheck.checkNbr, LessEqual<Required<PrintChecksFilter.nextCheckNbr>>>>>>>
							.Select(this, det.CashAccountID, det.PaymentMethodID, NextCheckNbr, endNumber)
							.RowCast<CashAccountCheck>()
							.Select(check => check.CheckNbr)
							.ToArray();

						if (duplicates.Any())
						{
							throw new PXException(
								Messages.TooSmallCheckNumbersGap,
								stubCount,
								NextCheckNbr,
								endNumber,
								string.Join(",", duplicates));
						}
					}

					short ordinalInStub = 0;
					int stubOrdinal = 0;
					foreach (var group in adjustments)
					{
						pe.Document.Current.BillCntr++;

						if (ordinalInStub > pt.APStubLines - 1)
						{
							//AssignCheckNumber only for first StubLines in check, other/all lines will be printed on remittance report
							if (pt.APPrintRemittance == true)
							{
								foreach (IAdjustmentStub adj in group.GroupedStubs)
								{
									adj.StubNbr = null;

									if (adj.Persistent)
										pe.Caches[adj.GetType()].Update(adj);
								}

								pe.AddCheckDetail(group, null);

								continue;
							}
							NextCheckNbr = AutoNumberAttribute.NextNumber(NextCheckNbr);

							pe.Document.Current.StubCntr++;
							ordinalInStub = 0;
							stubOrdinal++;
						}

						foreach (IAdjustmentStub adj in group.GroupedStubs)
						{
							SetAdjustmentStubNumber(pe, doc, adj, NextCheckNbr);
						}
						pe.AddCheckDetail(group, NextCheckNbr);
						
						StoreStubNumber(pe, doc, det, NextCheckNbr, stubOrdinal);

						ordinalInStub++;
					}
				}
				else // create batch payment
				{
					//Update last number
					det.APLastRefNbr = NextCheckNbr;
				}

				pe.cashaccountdetail.Update(det); // det.APLastRefNumber was modified in StoreStubNumber method

				NextCheckNbr = AutoNumberAttribute.NextNumber(NextCheckNbr);
				pe.Document.Current.Printed = true;
				pe.Document.Current.Hold = false;
				pe.Document.Update(pe.Document.Current);
			}
			else
			{
				if (pe.Document.Current.Printed != true || pe.Document.Current.Hold == true)
				{
					pe.Document.Current.Printed = true;
					pe.Document.Current.Hold = false;
					pe.Document.Update(pe.Document.Current);
				}
			}
		}

		public virtual void StoreStubNumber(APPaymentEntry pe, APPayment doc, PaymentMethodAccount det, string StubNbr, int stubOrdinal)
		{
			if (doc.VoidAppl == true || StubNbr == null) return;

			pe.CACheck.Insert(new CashAccountCheck
			{
				AccountID = doc.CashAccountID,
				PaymentMethodID = doc.PaymentMethodID,
				CheckNbr = StubNbr,
				DocType = doc.DocType,
				RefNbr = doc.RefNbr,
				FinPeriodID = doc.FinPeriodID,
				DocDate = doc.DocDate,
				VendorID = doc.VendorID
			});

			det.APLastRefNbr = StubNbr;
		}

		private static void SetAdjustmentStubNumber(APPaymentEntry pe, APPayment doc, IAdjustmentStub adj, string StubNbr)
		{
			adj.StubNbr = StubNbr;
			adj.CashAccountID = doc.CashAccountID;
			adj.PaymentMethodID = doc.PaymentMethodID;

			if (adj.Persistent)
				pe.Caches[adj.GetType()].Update(adj);
		}

		public static void AssignNumbersWithNoAdditionalProcessing(APPaymentEntry pe, APPayment doc)
		{
			PaymentMethod method = pe.paymenttype.Select(doc.PaymentMethodID);
			if (method == null || method.PrintOrExport == true)
			{
				// To persist the manual changes from "Release Checks" screen (AC-94830)
				pe.Document.Cache.MarkUpdated(doc);
				return;
			}

			pe.RowPersisting.RemoveHandler<APAdjust>(pe.APAdjust_RowPersisting);
			pe.Clear();
			pe.Document.Current = pe.Document.Search<APPayment.refNbr>(doc.RefNbr, doc.DocType);
			PaymentMethodAccount det = pe.cashaccountdetail.Select();

			pe.Document.Current.StubCntr = 1;
			pe.Document.Current.BillCntr = 0;
			pe.Document.Current.ExtRefNbr = doc.ExtRefNbr;

			var adjustments = pe.GetAdjustmentsPrintList();

			foreach (var group in adjustments)
			{
				pe.Document.Current.BillCntr++;

				foreach (IAdjustmentStub adj in group.GroupedStubs)
				{
					SetAdjustmentStubNumber(pe, doc, adj, doc.ExtRefNbr);
				}
			}

			det.APLastRefNbr = doc.ExtRefNbr;
			pe.cashaccountdetail.Update(det);

			pe.Document.Current.Printed = true;
			pe.Document.Current.Hold = false;
			pe.Document.Update(pe.Document.Current);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		public static CABatch CreateBatchPayment(List<APPayment> list, PrintChecksFilter filter)
		{
			CABatchEntry be = CreateInstance<CABatchEntry>();
			CABatch result = be.Document.Insert(new CABatch());
			be.Document.Current = result;

			CABatch copy = (CABatch)be.Document.Cache.CreateCopy(result);
			copy.CashAccountID = filter.PayAccountID;
			copy.PaymentMethodID = filter.PayTypeID;
			result = be.Document.Update(copy);

			foreach (APPayment iPmt in list)
			{
				if (iPmt.CashAccountID != result.CashAccountID || iPmt.PaymentMethodID != result.PaymentMethodID)
				{
					throw new PXException(Messages.APPaymentDoesNotMatchCABatchByAccountOrPaymentType);
				}

				if (string.IsNullOrEmpty(iPmt.ExtRefNbr) && string.IsNullOrEmpty(filter.NextCheckNbr))
				{
					throw new PXException(Messages.NextCheckNumberIsRequiredForProcessing);
				}

				be.AddPayment(iPmt, true);
			}

			be.Save.Press();
			result = be.Document.Current;
			return result;
		}

		protected virtual void PrintPayments(List<APPayment> list, PrintChecksFilter filter, PaymentMethod paymentMethod)
		{
			if (list.Count == 0) return;

			if (paymentMethod.UseForAP == true)
			{
				if (paymentMethod.APPrintChecks == true && string.IsNullOrEmpty(paymentMethod.APCheckReportID))
				{
					throw new PXException(Messages.FieldNotSetInPaymentMethod, PXUIFieldAttribute.GetDisplayName<PaymentMethod.aPCheckReportID>(paymenttype.Cache), paymentMethod.PaymentMethodID);
				}

				if (paymentMethod.APPrintChecks == true && paymentMethod.APPrintRemittance == true && string.IsNullOrEmpty(paymentMethod.APRemittanceReportID))
				{
					throw new PXException(Messages.FieldNotSetInPaymentMethod, PXUIFieldAttribute.GetDisplayName<PaymentMethod.aPRemittanceReportID>(paymenttype.Cache), paymentMethod.PaymentMethodID);
				}
			}

			bool printAdditionRemit = false;
			if (paymentMethod.APCreateBatchPayment == true)
			{
				CABatchEntry be = CreateInstance<CABatchEntry>();
				CABatch result = be.Document.Insert(new CABatch());
				be.Document.Current = result;

				CABatch copy = (CABatch)be.Document.Cache.CreateCopy(result);
				copy.CashAccountID = filter.PayAccountID;
				copy.PaymentMethodID = filter.PayTypeID;
				result = be.Document.Update(copy);

				CABatch batch = be.Document.Current;

				if (batch != null)
				{
					bool failed = false;
					APPaymentEntry pe = CreateInstance<APPaymentEntry>();

					string NextCheckNbr = filter.NextCheckNbr;
					foreach (APPayment pmt in list)
					{
						APPayment payment = pmt;

						if (pmt.CashAccountID != result.CashAccountID || pmt.PaymentMethodID != result.PaymentMethodID)
						{
							throw new PXException(Messages.APPaymentDoesNotMatchCABatchByAccountOrPaymentType);
						}

						if (string.IsNullOrEmpty(pmt.ExtRefNbr) && string.IsNullOrEmpty(filter.NextCheckNbr))
						{
							throw new PXException(Messages.NextCheckNumberIsRequiredForProcessing);
						}

						PXProcessing<APPayment>.SetCurrentItem(payment);
						try
						{
							payment = pe.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
							if (payment.PrintCheck != true)
							{
								throw new PXException(Messages.CantPrintNonprintableCheck);
							}
						    if (payment.DocType.IsIn(APDocType.Check, APDocType.QuickCheck, APDocType.Prepayment) &&
						        payment.Status != APDocStatus.PendingPrint)
						    {
						        throw new PXException(Messages.ChecksMayBePrintedInPendingPrintStatus);
						    }

							AssignNumbers(pe, payment, ref NextCheckNbr, true);

							if (payment.Passed == true)
							{
								pe.TimeStamp = payment.tstamp;
							}
							pe.Save.Press();
							payment.tstamp = pe.TimeStamp;
							pe.Clear();

							be.AddPayment(payment, true);
						}
						catch (PXException e)
						{
							PXProcessing<APPayment>.SetError(e);
							failed = true;
						}
					}

					bool batchHasPayments = be.BatchPayments.Select().Count > 0;

					if (failed && batchHasPayments)
					{
						be.Save.Press();
						batch = be.Document.Current;
						throw new PXOperationCompletedWithErrorException(Messages.APPaymentsAreAddedToTheBatchFailedPaymentsExcluded, batch.BatchNbr);
					}

					if (failed && !batchHasPayments)
					{
						throw new PXOperationCompletedWithErrorException();
					}

					be.Save.Press();
					batch = be.Document.Current;

					RedirectToResultWithCreateBatch(batch);
				}
			}
			else
			{
				APReleaseChecks pp = CreateInstance<APReleaseChecks>();
				ReleaseChecksFilter filter_copy = PXCache<ReleaseChecksFilter>.CreateCopy(pp.Filter.Current);
				filter_copy.PayAccountID = filter.PayAccountID;
				filter_copy.PayTypeID = filter.PayTypeID;
				filter_copy.CuryID = filter.CuryID;
				pp.Filter.Cache.Update(filter_copy);

				APPaymentEntry pe = CreateInstance<APPaymentEntry>();
				bool failed = false;
				Dictionary<string, string> d = new Dictionary<string, string>();

				string nextCheckNbr = filter.NextCheckNbr;
				string prevCheckNbr = nextCheckNbr;

				int idxReportFilter = 0;
				foreach (APPayment pmt in list)
				{
					APPayment payment = pmt;
					PXProcessing<APPayment>.SetCurrentItem(payment);
					try
					{
						prevCheckNbr = nextCheckNbr;
						if (filter.IsNextNumberDuplicated(this, nextCheckNbr))
						{
							string duplicate = nextCheckNbr;
							nextCheckNbr = AutoNumberAttribute.NextNumber(nextCheckNbr);
							throw new PXException(Messages.ConflictWithExistingCheckNumber, duplicate);
						}

						payment = pe.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
						if (payment.PrintCheck != true)
						{
							throw new PXException(Messages.CantPrintNonprintableCheck);
						}
						if (payment.DocType.IsIn(APDocType.Check, APDocType.QuickCheck, APDocType.Prepayment) &&
							payment.Status != APDocStatus.PendingPrint)
						{
							throw new PXException(Messages.ChecksMayBePrintedInPendingPrintStatus);
						}
						AssignNumbers(pe, payment, ref nextCheckNbr);

						if (payment.Passed == true)
						{
							pe.TimeStamp = payment.tstamp;
						}
						pe.Save.Press();
						payment.tstamp = pe.TimeStamp;
						pe.Clear();

						APPayment seldoc = pe.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
						seldoc.Selected = true;
						seldoc.Passed = true;
						seldoc.tstamp = payment.tstamp;
						pp.APPaymentList.Cache.Update(seldoc);
						pp.APPaymentList.Cache.SetStatus(seldoc, PXEntryStatus.Updated);

						printAdditionRemit |= seldoc.BillCntr > paymentMethod.APStubLines;

						StringBuilder sbDocType = new StringBuilder($"{nameof(APPayment)}.{nameof(APPayment.DocType)}");
						sbDocType.Append(Convert.ToString(idxReportFilter));
						StringBuilder sbRefNbr = new StringBuilder($"{nameof(APPayment)}.{nameof(APPayment.RefNbr)}");
						sbRefNbr.Append(Convert.ToString(idxReportFilter));

						idxReportFilter++;

						d[sbDocType.ToString()] = payment.DocType.IsIn(APDocType.Prepayment, APDocType.QuickCheck) ? payment.DocType : APDocType.Check;
						d[sbRefNbr.ToString()] = payment.RefNbr;
						PXProcessing<APPayment>.SetProcessed();
					}
					catch (PXException e)
					{
						PXProcessing<APPayment>.SetError(e);
						failed = true;
						nextCheckNbr = prevCheckNbr;
					}
				}

				if (failed)
				{
					PXReportRequiredException report = null;
					if (d.Count > 0)
					{
						d[ReportMessages.CheckReportFlag] = ReportMessages.CheckReportFlagValue;
						report = new PXReportRequiredException(d, paymentMethod.APCheckReportID, PXBaseRedirectException.WindowMode.New, "Check");
					}
					throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased, report);
				}
				else
				{
					if (d.Count > 0)
					{
						RedirectToResultNoBatch(pp, d, paymentMethod, printAdditionRemit, nextCheckNbr);
					}
				}
			}
		}

		protected virtual void RedirectToResultWithCreateBatch(CABatch batch)
		{
			CABatchEntry be = CreateInstance<CABatchEntry>();
			be.Document.Current = be.Document.Search<CABatch.batchNbr>(batch.BatchNbr);
			be.SelectTimeStamp();
			throw new PXRedirectRequiredException(be, "Redirect");
		}

		protected virtual void RedirectToResultNoBatch(APReleaseChecks pp, Dictionary<string, string> d, PaymentMethod paymentType, bool printAdditionRemit, string NextCheckNbr)
		{
			d[ReportMessages.CheckReportFlag] = ReportMessages.CheckReportFlagValue;
			var requiredException = new PXReportRequiredException(d, paymentType.APCheckReportID, PXBaseRedirectException.WindowMode.New, "Check");
			if (paymentType.APPrintRemittance == true
				&& !string.IsNullOrEmpty(paymentType.APRemittanceReportID)
				&& printAdditionRemit)
			{
				Dictionary<string, string> p = new Dictionary<string, string>(d)
				{
					["StartCheckNbr"] = "",
					["EndCheckNbr"] = NextCheckNbr
				};
				requiredException.SeparateWindows = true;
				requiredException.AddSibling(paymentType.APRemittanceReportID, p);
			}

			throw new PXRedirectWithReportException(pp, requiredException, "Preview");
		}

		protected virtual void PrintChecksFilter_PayTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter.Cache.SetDefaultExt<PrintChecksFilter.payAccountID>(e.Row);
		}

		protected virtual void PrintChecksFilter_PayAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter.Cache.SetDefaultExt<PrintChecksFilter.curyID>(e.Row);
		}

		protected virtual void PrintChecksFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//refresh last number when saved values are populated in filter
			PrintChecksFilter oldRow = (PrintChecksFilter) e.OldRow;
			PrintChecksFilter row = (PrintChecksFilter) e.Row;

			if ((oldRow.PayAccountID == null && oldRow.PayTypeID == null)
				|| (oldRow.PayAccountID!= row.PayAccountID || oldRow.PayTypeID != row.PayTypeID))
			{
				((PrintChecksFilter)e.Row).CurySelTotal = 0m;
				((PrintChecksFilter)e.Row).SelTotal = 0m;
				((PrintChecksFilter)e.Row).SelCount = 0;
				((PrintChecksFilter)e.Row).NextCheckNbr = null;
				APPaymentList.Cache.Clear();
			}
		}

		protected virtual void PrintChecksFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool SuggestNextNumber = false;
			PrintChecksFilter filter = (PrintChecksFilter)e.Row;
			PXUIFieldAttribute.SetVisible<PrintChecksFilter.curyID>(sender, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			if (e.Row != null
				&& cashaccountdetail.Current != null
				&& (!Equals(cashaccountdetail.Current.CashAccountID, filter.PayAccountID)
					|| !Equals(cashaccountdetail.Current.PaymentMethodID, filter.PayTypeID)))
			{
				cashaccountdetail.Current = null;
				SuggestNextNumber = true;
			}

			if (e.Row != null && paymenttype.Current != null && (!Equals(paymenttype.Current.PaymentMethodID, filter.PayTypeID)))
			{
				paymenttype.Current = null;
			}

			if (e.Row != null && string.IsNullOrEmpty(filter.NextCheckNbr))
			{
				SuggestNextNumber = true;
			}

			PXUIFieldAttribute.SetVisible<PrintChecksFilter.nextCheckNbr>(sender, null, true);

			if (e.Row == null) return;

			if (cashaccountdetail.Current != null && true == cashaccountdetail.Current.APAutoNextNbr && SuggestNextNumber)
			{
				filter.NextCheckNbr = PaymentRefAttribute.GetNextPaymentRef(
					sender.Graph,
					cashaccountdetail.Current.CashAccountID,
					cashaccountdetail.Current.PaymentMethodID);
			}

			bool printOrExport = paymenttype.Current == null || paymenttype.Current.PrintOrExport == true;

			bool isNextNumberDuplicated = false;
			if (paymenttype.Current != null && paymenttype.Current.PrintOrExport == true && string.IsNullOrEmpty(filter.NextCheckNbr))
			{
				sender.RaiseExceptionHandling<PrintChecksFilter.nextCheckNbr>(e.Row, filter.NextCheckNbr,
					new PXSetPropertyException(Messages.NextCheckNumberIsRequiredForProcessing, PXErrorLevel.Warning));
			}
			else if (!string.IsNullOrEmpty(filter.NextCheckNbr) && !AutoNumberAttribute.CanNextNumber(filter.NextCheckNbr))
			{
				sender.RaiseExceptionHandling<PrintChecksFilter.nextCheckNbr>(e.Row, filter.NextCheckNbr,
					new PXSetPropertyException(Messages.NextCheckNumberCanNotBeInc, PXErrorLevel.Warning));
			}
			else if (paymenttype.Current != null && paymenttype.Current.APPrintChecks == true
				&& (isNextNumberDuplicated = filter.IsNextNumberDuplicated(sender.Graph, filter.NextCheckNbr)))
			{
				sender.RaiseExceptionHandling<PrintChecksFilter.nextCheckNbr>(e.Row, filter.NextCheckNbr,
					new PXSetPropertyException(Messages.ConflictWithExistingCheckNumber, filter.NextCheckNbr));
			}
			else
			{
				sender.RaiseExceptionHandling<PrintChecksFilter.nextCheckNbr>(e.Row, filter.NextCheckNbr, null);
			}

			APPaymentList.SetProcessEnabled(!isNextNumberDuplicated && printOrExport && filter.PayTypeID != null);
			APPaymentList.SetProcessAllEnabled(!isNextNumberDuplicated && printOrExport && filter.PayTypeID != null);

			if (HttpContext.Current != null && Filter.Current.BranchID != PXAccess.GetBranchID())
			{
				Filter.Current.BranchID = PXAccess.GetBranchID();
			}

			PaymentMethod pt = paymenttype.Current;
			APPaymentList.SetProcessDelegate(
				delegate(List<APPayment> list)
				{
					APPrintChecks graph = CreateInstance<APPrintChecks>();
						graph.PrintPayments(list, filter, pt);
				}
			);
		}

		protected virtual void APPayment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PrintChecksFilter filter = Filter.Current;
			if (filter != null)
			{
				object OldRow = e.OldRow;
				if (ReferenceEquals(e.Row, e.OldRow) && !_copies.TryGetValue(e.Row, out OldRow))
				{
					decimal? curyval = 0m;
					decimal? val = 0m;
					int? count = 0;
					foreach (APPayment res in APPaymentList.Select())
					{
						if (res.Selected == true)
						{
							curyval += res.CuryOrigDocAmt ?? 0m;
							val += res.OrigDocAmt ?? 0m;
							count++;
						}
					}

					filter.CurySelTotal = curyval;
					filter.SelTotal = val;
					filter.SelCount = count;
				}
				else
				{
					APPayment old_row = OldRow as APPayment;
					APPayment new_row = e.Row as APPayment;

					filter.CurySelTotal -= old_row.Selected == true ? old_row.CuryOrigDocAmt : 0m;
					filter.CurySelTotal += new_row.Selected == true ? new_row.CuryOrigDocAmt : 0m;

					filter.SelTotal -= old_row.Selected == true ? old_row.OrigDocAmt : 0m;
					filter.SelTotal += new_row.Selected == true ? new_row.OrigDocAmt : 0m;

					filter.SelCount -= old_row.Selected == true ? 1 : 0;
					filter.SelCount += new_row.Selected == true ? 1 : 0;
				}
			}
		}
	}
	[Serializable]
	public partial class PrintChecksFilter : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDefault(typeof(AccessInfo.branchID))]
		[Branch(Visible = true, Enabled = true)]
		public virtual Int32? BranchID
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
		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		[PXDefault]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>))]
		[PXRestrictor(typeof(Where<PaymentMethod.aPPrintChecks, Equal<True>,
			Or<PaymentMethod.aPCreateBatchPayment, Equal<True>>>),
			Messages.PaymentTypeNoPrintCheck, typeof(PaymentMethod.paymentMethodID))]
		public virtual string PayTypeID { get; set; }
		#endregion
		#region PayAccountID
		public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
		protected Int32? _PayAccountID;
		[CashAccount(typeof(PrintChecksFilter.branchID), typeof(Search2<CashAccount.cashAccountID,
							InnerJoin<PaymentMethodAccount,
								On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
							Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<PrintChecksFilter.payTypeID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>), Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search2<PaymentMethodAccount.cashAccountID,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<PrintChecksFilter.payTypeID>>,
											And<PaymentMethodAccount.useForAP, Equal<True>,
											And<PaymentMethodAccount.aPIsDefault, Equal<True>,
											And<CashAccount.branchID, Equal<Current<AccessInfo.branchID>>>>>>>))]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion

		#region NextCheckNbr
		public abstract class nextCheckNbr : PX.Data.BQL.BqlString.Field<nextCheckNbr> { }
		protected String _NextCheckNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Next Check Number", Visible = false)]
		public virtual String NextCheckNbr
		{
			get
			{
				return this._NextCheckNbr;
			}
			set
			{
				this._NextCheckNbr = value;
			}
		}
		#endregion
		#region Balance
		public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
		protected Decimal? _Balance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CurySelTotal
		public abstract class curySelTotal : PX.Data.BQL.BqlDecimal.Field<curySelTotal> { }
		protected Decimal? _CurySelTotal;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(PrintChecksFilter.curyInfoID), typeof(PrintChecksFilter.selTotal), BaseCalc = false)]
		[PXUIField(DisplayName = "Selection Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CurySelTotal
		{
			get
			{
				return this._CurySelTotal;
			}
			set
			{
				this._CurySelTotal = value;
			}
		}
		#endregion
		#region SelTotal
		public abstract class selTotal : PX.Data.BQL.BqlDecimal.Field<selTotal> { }
		protected Decimal? _SelTotal;
		[PXDBDecimal(4)]
		public virtual Decimal? SelTotal
		{
			get
			{
				return this._SelTotal;
			}
			set
			{
				this._SelTotal = value;
			}
		}
		#endregion
		#region SelCount
		public abstract class selCount : PX.Data.BQL.BqlInt.Field<selCount> { }
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Number of Payments", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual int? SelCount { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<PrintChecksFilter.payAccountID>>>>))]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = BatchModule.AP)]
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
		#region CashBalance
		public abstract class cashBalance : PX.Data.BQL.BqlDecimal.Field<cashBalance> { }
		protected Decimal? _CashBalance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PrintChecksFilter.curyID))]
		[PXUIField(DisplayName = "Available Balance", Enabled = false)]
		[CashBalance(typeof(PrintChecksFilter.payAccountID))]
		public virtual Decimal? CashBalance
		{
			get
			{
				return this._CashBalance;
			}
			set
			{
				this._CashBalance = value;
			}
		}
		#endregion
		#region PayFinPeriodID
		public abstract class payFinPeriodID : PX.Data.BQL.BqlString.Field<payFinPeriodID> { }
		protected string _PayFinPeriodID;
		[FinPeriodID(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public virtual String PayFinPeriodID
		{
			get
			{
				return this._PayFinPeriodID;
			}
			set
			{
				this._PayFinPeriodID = value;
			}
		}
		#endregion
		#region GLBalance
		public abstract class gLBalance : PX.Data.BQL.BqlDecimal.Field<gLBalance> { }
		protected Decimal? _GLBalance;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PrintChecksFilter.curyID))]
		[PXUIField(DisplayName = "GL Balance", Enabled = false)]
		[GLBalance(typeof(PrintChecksFilter.payAccountID), typeof(PrintChecksFilter.payFinPeriodID))]
		public virtual Decimal? GLBalance
		{
			get
			{
				return this._GLBalance;
			}
			set
			{
				this._GLBalance = value;
			}
		}
		#endregion

		public bool IsNextNumberDuplicated(PXGraph graph, string nextNumber)
		{
			return PaymentRefAttribute.IsNextNumberDuplicated(graph, PayAccountID, PayTypeID, nextNumber);
		}
	}
}
