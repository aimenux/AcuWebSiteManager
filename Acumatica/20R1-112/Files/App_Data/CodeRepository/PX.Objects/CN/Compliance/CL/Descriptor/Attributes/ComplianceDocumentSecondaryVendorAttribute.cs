using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentSecondaryVendorAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is ComplianceDocument row)
            {
                var secondaryVendorName = GetVendorName(row.SecondaryVendorID, sender.Graph);
                sender.SetValue<ComplianceDocument.secondaryVendorName>(row, secondaryVendorName);
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
