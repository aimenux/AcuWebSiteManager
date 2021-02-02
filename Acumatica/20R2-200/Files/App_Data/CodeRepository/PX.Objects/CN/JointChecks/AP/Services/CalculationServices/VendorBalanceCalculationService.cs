using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class VendorBalanceCalculationService : CalculationServiceBase
    {
        protected readonly IEnumerable<JointPayee> JointPayees;

        public VendorBalanceCalculationService(PXGraph graph, IEnumerable<JointPayee> jointPayees)
            : base(graph)
        {
            JointPayees = jointPayees;
        }

        public decimal? GetVendorBalancePerBill(APInvoice invoice)
        {
            if (invoice.IsRetainageDocument == true)
            {

                var openBalanceRelatedRetainageBills = GetOpenBalanceByAllBillsFromRetainageGroup(invoice);
                var jointPayeeBalance = JointPayees.Sum(x => x.JointBalance);
                var availablePaymentAmountForPrimaryVendor = openBalanceRelatedRetainageBills - jointPayeeBalance;
                return Math.Min(invoice.CuryDocBal.GetValueOrDefault(),
                    availablePaymentAmountForPrimaryVendor.GetValueOrDefault());
            }
            else
            {
                return invoice.CuryDocBal - JointPayees.Sum(x => x.JointBalance);
            }
        }

        protected virtual decimal? GetOpenBalanceByAllBillsFromRetainageGroup(APInvoice invoice)
        {
            var originalBill = InvoiceDataProvider.GetOriginalInvoice(Graph, invoice.RefNbr, invoice.DocType);
            var relatedRetainageBills =
                InvoiceDataProvider.GetRelatedRetainageBills(Graph, originalBill.RefNbr, originalBill.DocType);
            return originalBill.CuryDocBal + originalBill.CuryRetainageUnreleasedAmt +
                   relatedRetainageBills.Sum(x => x.CuryDocBal);
        }
    }
}