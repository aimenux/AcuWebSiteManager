using System;
using System.Collections;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver
{
    public sealed class LienWaiverReportSelectorAttribute : PXCustomSelectorAttribute
    {
        private static readonly Type[] Fields =
        {
            typeof(ComplianceDocument.creationDate),
            typeof(ComplianceDocument.projectID),
            typeof(ComplianceDocument.vendorID),
            typeof(ComplianceDocument.vendorName),
            typeof(ComplianceDocument.documentTypeValue),
            typeof(ComplianceDocument.status),
            typeof(ComplianceDocument.required),
            typeof(ComplianceDocument.received),
            typeof(ComplianceDocument.isReceivedFromJointVendor),
            typeof(ComplianceDocument.isProcessed),
            typeof(ComplianceDocument.isVoided),
            typeof(ComplianceDocument.isCreatedAutomatically),
            typeof(ComplianceDocument.customerID),
            typeof(ComplianceDocument.customerName),
            typeof(ComplianceDocument.subcontract),
            typeof(ComplianceDocument.billID),
            typeof(ComplianceDocument.billAmount),
            typeof(ComplianceDocument.lienWaiverAmount),
            typeof(ComplianceDocument.lienNoticeAmount),
            typeof(ComplianceDocument.apCheckId),
            typeof(ComplianceDocument.paymentDate),
            typeof(ComplianceDocument.throughDate),
            typeof(ComplianceDocument.jointVendorInternalId),
            typeof(ComplianceDocument.jointVendorExternalName),
            typeof(ComplianceDocument.jointAmount),
            typeof(ComplianceDocument.jointLienWaiverAmount),
            typeof(ComplianceDocument.jointLienNoticeAmount)
        };

        private readonly string LienWaiverType;

        public LienWaiverReportSelectorAttribute(string lienWaiverType)
            : base(typeof(ComplianceDocument.complianceDocumentID), Fields)
        {
            DescriptionField = typeof(ComplianceDocument.vendorID);
            LienWaiverType = lienWaiverType;
        }

        public IEnumerable GetRecords()
        {
            return SelectFrom<ComplianceDocument>
                .InnerJoin<ComplianceAttribute>
                    .On<ComplianceDocument.documentTypeValue.IsEqual<ComplianceAttribute.attributeId>>
                .Where<ComplianceDocument.vendorID.IsNotNull
                    .And<ComplianceDocument.projectID.IsNotNull>
                    .And<ComplianceAttribute.value.IsEqual<P.AsString>>>.View.Select(_Graph, LienWaiverType);
        }       
    }
}