using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentCustomerAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is ComplianceDocument document)
            {
                var customerName = GetCustomerName(document.CustomerID, sender.Graph);
                sender.SetValue<ComplianceDocument.customerName>(document, customerName);
            }
        }

        private string GetCustomerName(int? customerId, PXGraph senderGraph)
        {
            if (!customerId.HasValue)
            {
                return null;
            }
            var customer = new PXSelect<Customer,
                    Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(senderGraph)
                .SelectSingle(customerId);
            return customer?.AcctName;
        }
    }
}
