using PX.Api;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	/// <summary>
	/// This graph display CABatches created from PRPayments with a Direct Deposit payment method. It can release the batch and export ACH files.
	/// </summary>
	public class PRDirectDepositBatchEntry : PXGraph<PRDirectDepositBatchEntry>
	{
		private const string _DefaultAchFileName = "ACH";

		#region Toolbar buttons
		public PXSave<CABatch> Save;
		public PXCancel<CABatch> Cancel;
		public PXDelete<CABatch> Delete;
		public PXFirst<CABatch> First;
		public PXPrevious<CABatch> Previous;
		public PXNext<CABatch> Next;
		public PXLast<CABatch> Last;
		#endregion

		#region Views

		public SelectFrom<CABatch>
			.Where<CABatch.origModule.IsEqual<GL.BatchModule.modulePR>>.View Document;

		public SelectFrom<CABatchDetail>
				.LeftJoin<PRPayment>
					.On<PRPayment.docType.IsEqual<CABatchDetail.origDocType>
					.And<PRPayment.refNbr.IsEqual<CABatchDetail.origRefNbr>>>
				.LeftJoin<PRDirectDepositSplit>
					.On<PRDirectDepositSplit.docType.IsEqual<CABatchDetail.origDocType>
					.And<PRDirectDepositSplit.refNbr.IsEqual<CABatchDetail.origRefNbr>>
					.And<PRDirectDepositSplit.lineNbr.IsEqual<CABatchDetail.origLineNbr>>>
				.Where<CABatchDetail.batchNbr.IsEqual<CABatch.batchNbr.FromCurrent>>.View BatchPaymentsDetails;

		public SelectFrom<PRPayment>
				.InnerJoin<CABatchDetail>.On<CABatchDetail.origDocType.IsEqual<PRPayment.docType>
					.And<CABatchDetail.origRefNbr.IsEqual<PRPayment.refNbr>
					.And<CABatchDetail.origModule.IsEqual<GL.BatchModule.modulePR>>>>
				.Where<CABatchDetail.batchNbr.IsEqual<CABatchDetail.batchNbr.AsOptional>>
				.AggregateTo<GroupBy<PRPayment.docType>, GroupBy<PRPayment.refNbr>>.View Payments;

		public PXSelectReadonly<CashAccountPaymentMethodDetail,
			Where<CashAccountPaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
				And<Current<PRPayment.docType>, IsNotNull,
				And<Current<PRPayment.refNbr>, IsNotNull,
				And<CashAccountPaymentMethodDetail.accountID, Equal<Current<CABatch.cashAccountID>>,
				And<CashAccountPaymentMethodDetail.detailID, Equal<Required<CashAccountPaymentMethodDetail.detailID>>>>>>>> CashAccountSettings;

		public SelectFrom<PRDirectDepositSplit>
			.Where<PRDirectDepositSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRDirectDepositSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View EmployeePaymentSplits;

		public SelectFrom<BAccount>
			.Where<BAccount.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View Employee;

		#endregion Views

		public PRDirectDepositBatchEntry()
		{
			BatchPaymentsDetails.AllowInsert = false;
			BatchPaymentsDetails.AllowUpdate = false;
			BatchPaymentsDetails.AllowDelete = false;
		}

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(CABatchType.RefNbrAttribute))]
		[CABatchType.RefNbr(typeof(Search<CABatch.batchNbr, Where<CABatch.origModule, Equal<GL.BatchModule.modulePR>>>))]
		public virtual void _(Events.CacheAttached<CABatch.batchNbr> e) { }
		#endregion CacheAttached

		#region Events

		public void _(Events.RowSelected<CABatch> e)
		{
			PXUIFieldAttribute.SetEnabled<CABatch.tranDesc>(e.Cache, e.Row, e.Row.Released == false);
			Release.SetEnabled(e.Row.Released == false && e.Row.Hold == false);
			Export.SetEnabled(e.Row.Released == true);
			Delete.SetEnabled(e.Row.Released == false);
			Document.AllowUpdate = e.Row.Released == false || this.IsImport;
		}

		public void _(Events.RowDeleted<CABatch> e)
		{
			if (e.Row == null)
			{
				return;
			}

			foreach (PRPayment payment in Payments.Select(e.Row.BatchNbr))
			{
				payment.Printed = false;
				Payments.Update(payment);
			}
		}

		#endregion Events

		#region Actions

		public PXAction<CABatch> Release;
		[PXUIField(DisplayName = CA.Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			CheckPrevOperation();
			this.Save.Press();
			CABatch document = this.Document.Current;
			PXLongOperation.StartOperation(this, delegate () { ReleaseDoc(document); });

			return adapter.Get();
		}

		public PXAction<CABatch> Export;
		[PXUIField(DisplayName = CA.Messages.Export, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable export(PXAdapter adapter)
		{
			CheckPrevOperation();
			CABatch document = this.Document.Current;
			if (document != null && document.Released == true)
			{
				var result = (PXResult<PaymentMethod, SYMapping>)PXSelectJoin<PaymentMethod,
									LeftJoin<SYMapping, On<SYMapping.mappingID, Equal<PRxPaymentMethod.prBatchExportSYMappingID>>>,
										Where<PaymentMethod.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>, 
											And<PRxPaymentMethod.prCreateBatchPayment, Equal<True>, 
											And<PRxPaymentMethod.prBatchExportSYMappingID, IsNotNull>>>>.Select(this);
				PaymentMethod paymentMethod = result;
				SYMapping mapping = result;
				if (paymentMethod != null && mapping != null)
				{
					string defaultFileName = this.GenerateFileName(document);
					PXLongOperation.StartOperation(this, delegate ()
					{
						SYExportProcess.RunScenario(mapping.Name,
							SYMapping.RepeatingOption.All,
							true,
							true,
							new PXSYParameter(CABatchEntry.ExportProviderParams.FileName, defaultFileName),
							new PXSYParameter(CABatchEntry.ExportProviderParams.BatchNbr, document.BatchNbr));
					});
				}
				else
				{
					throw new PXException(CA.Messages.CABatchExportProviderIsNotConfigured);
				}
			}
			return adapter.Get();
		}

		public PXAction<CABatch> ViewPRDocument;
		[PXUIField(DisplayName = Messages.ViewPRDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable viewPRDocument(PXAdapter adapter)
		{
			CABatchDetail detail = BatchPaymentsDetails.Current;
			if (detail == null)
			{
				return adapter.Get();
			}

			PRPayment payment = PXSelect<PRPayment,
							Where<PRPayment.docType, Equal<Required<PRPayment.docType>>,
							And<PRPayment.refNbr, Equal<Required<PRPayment.refNbr>>>>>.Select(this, detail.OrigDocType, detail.OrigRefNbr);
			if (payment == null)
			{
				return adapter.Get();
			}

			var graph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
			graph.Document.Current = graph.Document.Search<PRPayment.refNbr>(payment.RefNbr, payment.DocType);
			if (graph.Document.Current != null)
			{
				throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<CABatch> PrintPayStubs;
		[PXUIField(DisplayName = Messages.PrintPayStubs, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable printPayStubs(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				var parameters = new Dictionary<string, string>();
				parameters[PayStubsDirectDepositReportParameters.BatchNbr] = Document.Current.BatchNbr;
				throw new PXReportRequiredException(parameters, PayStubsDirectDepositReportParameters.ReportID, PXBaseRedirectException.WindowMode.New, Messages.PrintPayStubs);
			}

			return adapter.Get();
		}

		#endregion Actions

		public static void ReleaseDoc(CABatch batch)
		{
			if (batch.Released == true || batch.Hold == true)
			{
				throw new PXException(CA.Messages.CABatchStatusIsNotValidForProcessing);
			}

			var batchUpdateGraph = PXGraph.CreateInstance<PRCABatchUpdate>();
			batchUpdateGraph.Document.Current = batch;

			PRPayment voidedPayments = PXSelectReadonly2<PRPayment,
							InnerJoin<CABatchDetail, On<CABatchDetail.origModule, Equal<GL.BatchModule.modulePR>,
							And<CABatchDetail.origDocType, Equal<PRPayment.docType>,
							And<CABatchDetail.origRefNbr, Equal<PRPayment.refNbr>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>,
								And<PRPayment.voided, Equal<True>>>>.Select(batchUpdateGraph, batch.BatchNbr);

			if (voidedPayments != null && !string.IsNullOrEmpty(voidedPayments.RefNbr))
			{
				throw new PXException(Messages.BatchContainsVoidedPayments);
			}

			PXSelectBase<PRPayment> selectUnreleased = new PXSelectReadonly2<PRPayment,
							InnerJoin<CABatchDetail,
								On<CABatchDetail.origModule, Equal<GL.BatchModule.modulePR>,
								And<CABatchDetail.origDocType, Equal<PRPayment.docType>,
								And<CABatchDetail.origRefNbr, Equal<PRPayment.refNbr>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>,
								And<PRPayment.released, Equal<False>>>>(batchUpdateGraph);

			var unreleasedPayments = selectUnreleased.Select(batch.BatchNbr).FirstTableItems.GroupBy(x => new { x.DocType, x.RefNbr }).Select(x => x.First()).ToList();
			if (unreleasedPayments.Any())
			{
				unreleasedPayments.ForEach(x => PRPayChecksAndAdjustments.DefaultDescription(selectUnreleased.Cache, x));
				PRDocumentProcess.ReleaseDoc(unreleasedPayments, true);
			}

			batch.Released = true;
			batch.DateSeqNbr = CABatchEntry.GetNextDateSeqNbr(batchUpdateGraph, batch);
			batch = batchUpdateGraph.Document.Update(batch);
			batchUpdateGraph.Actions.PressSave();
		}

		private void CheckPrevOperation()
		{
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
		}

		public virtual string GenerateFileName(CABatch batch)
		{
			if (batch.CashAccountID != null && !string.IsNullOrEmpty(batch.PaymentMethodID))
			{
				CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, batch.CashAccountID);
				if (acct != null)
				{
					return string.Format(CA.Messages.CABatchDefaultExportFilenameTemplate, batch.PaymentMethodID, acct.CashAccountCD, batch.TranDate.Value, batch.DateSeqNbr);
				}
			}

			return _DefaultAchFileName;
		}
	}

	#region Processing Graph Definition
	[PXHidden]
	public class PRCABatchUpdate : PXGraph<PRCABatchUpdate>
	{
		public PXSelect<CABatch> Document;
		public SelectFrom<PRDirectDepositSplit>
				.InnerJoin<CABatchDetail>.On<CABatchDetail.origDocType.IsEqual<PRDirectDepositSplit.docType>
					.And<CABatchDetail.origRefNbr.IsEqual<PRDirectDepositSplit.refNbr>
					.And<CABatchDetail.origModule.IsEqual<GL.BatchModule.modulePR>>>>
				.Where<CABatchDetail.batchNbr.IsEqual<CABatch.batchNbr.FromCurrent>>.View Splits;
	
		public virtual void RecalcTotals()
		{
			CABatch row = this.Document.Current;
			if (row != null)
			{
				var lines = Splits.Select().FirstTableItems.GroupBy(x => (x.DocType, x.RefNbr, x.LineNbr));
				var total = lines.Sum(x => x.First().Amount);
				row.CuryDetailTotal = total;
				row.DetailTotal = total;
				row.Total = total;
			}
		}
	}
	#endregion
}