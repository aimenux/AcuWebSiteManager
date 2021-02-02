using System.Collections.Generic;
using System.Linq;
using PX.Common.Mail;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common.Abstractions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
    public class PaymentCycleWorkflowValidationService : ValidationServiceBase
    {
        public PaymentCycleWorkflowValidationService(APPaymentEntry graph)
            : base(graph)
        {
        }

        public void Validate()
		{
			var insertedAdjustments = Graph.Caches<APAdjust>().Inserted.Cast<APAdjust>().ToList();

			Validate(insertedAdjustments);
		}

		public void Validate(List<APAdjust> adjustments)
		{
			foreach (var adjustment in adjustments)
			{
				var invoice = InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);

				var invoiceJCExt = invoice.GetExtension<APInvoiceJCExt>();

				if (invoiceJCExt.IsJointPayees == true)
				{
					APInvoice origBill = null;

					if (invoice.IsRetainageDocument == true)
					{
						origBill = InvoiceDataProvider.GetOriginalInvoice(Graph, invoice);
					}

					if (PaymentCycleWorkflowIsStartedAndItIsOtherPayment(adjustment.AdjgDocType,
						adjustment.AdjgRefNbr,
						origBill?.DocType ?? invoice.DocType,
						origBill?.RefNbr ?? invoice.RefNbr))
					{
						ShowErrorMessage<APAdjust.adjdRefNbr>(adjustment,
							JointCheckMessages.PaymentCycleWorkflowIsStarted);
					}
				}
			}
		}

		private bool PaymentCycleWorkflowIsStartedAndItIsOtherPayment(string paymentDocType, string paymentRefNbr, string origDocType, string origRefNbr)
		{
			PXResult<APInvoice, APAdjust>[] records = PXSelectJoinGroupBy<APInvoice,
									InnerJoin<APAdjust,
										 On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
											 And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
											 And<APAdjust.voided, Equal<False>>>>>,
									 Where2<Where<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
												And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>,
													Or<APInvoiceJCExt.isPaymentCycleWorkflow, Equal<True>>>>,
											And<Where<APInvoice.origDocType, Equal<Required<APInvoice.origDocType>>,
													 And<APInvoice.origRefNbr, Equal<Required<APInvoice.origRefNbr>>,
													 And<APInvoice.isRetainageDocument, Equal<True>,
													 Or<APInvoice.docType, Equal<Required<APInvoice.docType>>,
														 And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>>>>>,
									Aggregate<GroupBy<APInvoiceJCExt.isPaymentCycleWorkflow,
												GroupBy<APAdjust.adjgRefNbr,
												GroupBy<APAdjust.adjgDocType>>>>>
									 .Select(Graph, paymentDocType, paymentRefNbr, origDocType, origRefNbr, origDocType, origRefNbr)
									 .AsEnumerable()
									 .Cast<PXResult<APInvoice, APAdjust>>()
										.ToArray();

			return records.Any(record => ((APInvoice) record).GetExtension<APInvoiceJCExt>().IsPaymentCycleWorkflow == true)
			       && !records.Any(record =>
			       {
				       APAdjust adjust = record;
				       return adjust.AdjgDocType == paymentDocType && adjust.AdjgRefNbr == paymentRefNbr;
			       });
		}
    }
}
