using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentTypeAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber,
        IPXRowPersistingSubscriber
    {
        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is ComplianceDocument complianceDocument)
            {
                cache.SetValue<ComplianceDocument.documentTypeValue>(complianceDocument, null);
            }
        }

        public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
        {
            if (args.Row is ComplianceDocument complianceDocument &&
                complianceDocument.DocumentType == GetInsuranceDocumentTypeId(cache))
            {
                var policyNumbers = GetPolicyNumbers(cache, complianceDocument);
                if (policyNumbers.Contains(complianceDocument.Policy))
                {
                    cache.RaiseExceptionHandling<ComplianceDocument.policy>(complianceDocument,
                        complianceDocument.Policy,
                        new PXSetPropertyException(ComplianceMessages.UniqueConstraintMessage));
                }
            }
        }

        private static IEnumerable<string> GetPolicyNumbers(PXCache cache, ComplianceDocument complianceDocument)
        {
            var insurances = new PXSelect<ComplianceDocument,
                    Where<ComplianceDocument.documentType, Equal<ComplianceDocument.insuranceDocumentTypeId>,
                        And<ComplianceDocument.complianceDocumentID,
                            NotEqual<Required<ComplianceDocument.complianceDocumentID>>,
                        And<ComplianceDocument.policy, IsNotNull>>>>(cache.Graph)
                .Select(complianceDocument.ComplianceDocumentID).FirstTableItems;
            return insurances.Select(i => i.Policy);
        }

        private static int? GetInsuranceDocumentTypeId(PXCache cache)
        {
            return new PXSelect<ComplianceAttributeType,
                    Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.insurance>>>(cache.Graph)
                .SelectSingle()?.ComplianceAttributeTypeID;
        }
    }
}