using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentVendorAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is ComplianceDocument row)
            {
                var vendorName = GetVendorName(row.VendorID, sender.Graph);
                sender.SetValue<ComplianceDocument.vendorName>(row, vendorName);
            }
        }

        private string GetVendorName(int? vendorId, PXGraph senderGraph)
        {
            if (!vendorId.HasValue)
            {
                return null;
            }
            var vendor = new PXSelect<Vendor,
                Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(senderGraph).SelectSingle(vendorId);
            return vendor?.AcctName;
        }
    }
}
