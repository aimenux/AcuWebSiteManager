using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices
{
    public class JointPayeePaymentService
    {
        private readonly PXGraph graph;

        public JointPayeePaymentService(PXGraph graph)
        {
            this.graph = graph;
        }

        public void DeleteJointPayeePayments(APPayment payment)
        {
            var jointPayeePayments = JointPayeePaymentDataProvider.GetJointPayeePayments(graph, payment);
            graph.Caches<JointPayeePayment>().DeleteAll(jointPayeePayments);
        }

        public void DeleteJointPayeePayments(APAdjust adjustment)
        {
            var jointPayeePayments = JointPayeePaymentDataProvider.GetJointPayeePayments(graph, adjustment);
            graph.Caches<JointPayeePayment>().DeleteAll(jointPayeePayments);
        }

        public void AddJointPayeePayments(APAdjust adjustment)
        {
            var originalBillReferenceNumber =
                InvoiceDataProvider.GetOriginalInvoice(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType).RefNbr;
            var jointPayees = JointPayeeDataProvider.GetJointPayees(graph, originalBillReferenceNumber, adjustment.AdjdLineNbr);
            foreach (var jointPayee in jointPayees)
            {
                AddJointPayeePayment(jointPayee, adjustment);
            }
        }

		public void ClosePaymentCycleWorkflowIfNeeded(APAdjust adjustment)
		{
			var invoice = InvoiceDataProvider.GetInvoice(graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
			if (!JointPayeePaymentDataProvider.DoesAnyNonReleasedJointPayeePaymentExist(graph, invoice))
			{
				UpdateIsPaymentCycleWorkflow(invoice, false);
			}
		}

		public void InitializePaymentCycleWorkflowIfRequired(APAdjust adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            var invoiceExtension = invoice.GetExtension<APInvoiceJCExt>();
            if (invoiceExtension.IsJointPayees == true && invoiceExtension.IsPaymentCycleWorkflow == false)
            {
                UpdateIsPaymentCycleWorkflow(invoice, true);
            }
        }

        private void AddJointPayeePayment(JointPayee jointPayee, APAdjust adjustment)
        {
            var jointPayeePayment = CreateJointPayeePayment(jointPayee, adjustment);
            graph.Caches<JointPayeePayment>().Insert(jointPayeePayment);
        }

        private static JointPayeePayment CreateJointPayeePayment(JointPayee jointPayee, APAdjust adjustment)
        {
            return new JointPayeePayment
            {
                JointPayeeId = jointPayee.JointPayeeId,
                PaymentRefNbr = adjustment.AdjgRefNbr,
                PaymentDocType = adjustment.AdjgDocType,
                InvoiceRefNbr = adjustment.AdjdRefNbr,
                InvoiceDocType = adjustment.AdjdDocType,
                AdjustmentNumber = adjustment.AdjNbr,
                BillLineNumber = adjustment.AdjdLineNbr
            };
        }

        // Fix for https://jira.itransition.com/browse/LEGODEV-2320
        // This implementation doesn`t raise any additional events for AccountAttribute.
        // Update APRegister table is required for update 'tstamp' column by SQL Server.
        // Can be removed in case Acumatica provide another fix.
        private void UpdateIsPaymentCycleWorkflow(APInvoice invoice, bool isPaymentCycleWorkflow)
        {
	        PXDatabase.Update(typeof(APInvoice),
                    new PXDataFieldAssign(nameof(APInvoiceJCExt.IsPaymentCycleWorkflow), PXDbType.Bit,
                        isPaymentCycleWorkflow),
                    new PXDataFieldRestrict(nameof(APInvoice.DocType), PXDbType.Char, invoice.DocType),
                    new PXDataFieldRestrict(nameof(APInvoice.RefNbr), PXDbType.NVarChar, invoice.RefNbr));
                PXDatabase.Update(typeof(APRegister),
                    new PXDataFieldAssign(nameof(APRegister.DocType), PXDbType.Char, invoice.DocType),
                    new PXDataFieldRestrict(nameof(APRegister.DocType), PXDbType.Char, invoice.DocType),
                    new PXDataFieldRestrict(nameof(APRegister.RefNbr), PXDbType.NVarChar, invoice.RefNbr));
        }
    }
}