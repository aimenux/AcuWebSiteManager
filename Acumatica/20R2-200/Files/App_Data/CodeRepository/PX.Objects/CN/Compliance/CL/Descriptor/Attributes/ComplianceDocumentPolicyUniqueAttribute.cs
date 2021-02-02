using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
	public class ComplianceDocumentPolicyUniqueAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
		{
			if (args.Operation != PXDBOperation.Delete
				&& args.Row is ComplianceDocument complianceDocument
				&& !string.IsNullOrWhiteSpace(complianceDocument.Policy))
			{
				var insuranceDocumentTypeId = new PXSelectReadonly<ComplianceAttributeType,
						Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.insurance>>>(cache.Graph)
							.SelectSingle()?.ComplianceAttributeTypeID;

				if (insuranceDocumentTypeId != null 
					&& complianceDocument.DocumentType == insuranceDocumentTypeId)
				{
					var insurances = new PXSelect<ComplianceDocument,
						Where<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.insuranceDocumentTypeId>>,
							And<ComplianceDocument.policy, Equal<Required<ComplianceDocument.policy>>>>>(cache.Graph)
						.Select(insuranceDocumentTypeId, complianceDocument.Policy).FirstTableItems;

					if (insurances.HasAtLeast(2))
					{
						cache.RaiseExceptionHandling<ComplianceDocument.policy>(args.Row, complianceDocument.Policy,
							new PXSetPropertyException(ComplianceMessages.UniqueConstraintMessage));
					}
				}
			}
		}
	}
}