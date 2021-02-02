using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentTypeAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is ComplianceDocument complianceDocument)
            {
                cache.SetValue<ComplianceDocument.documentTypeValue>(complianceDocument, null);
            }
        }
    }
}