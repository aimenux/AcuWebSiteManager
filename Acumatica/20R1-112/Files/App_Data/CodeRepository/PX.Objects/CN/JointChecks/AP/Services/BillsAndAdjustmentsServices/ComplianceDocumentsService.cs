using System;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CN.Compliance.AP.GraphExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices
{
    public class ComplianceDocumentsService
    {
        private readonly APInvoiceEntry graph;
        private readonly IEnumerable<JointPayeePayment> jointPayeePayments;

        public ComplianceDocumentsService(APInvoiceEntry graph, IEnumerable<JointPayeePayment> jointPayeePayments)
        {
            this.graph = graph;
            this.jointPayeePayments = jointPayeePayments;
        }

        public void UpdateComplianceDocumentsForVendorCheck(
            IEnumerable<ComplianceDocument> complianceDocuments, APRegister vendorCheck)
        {
            complianceDocuments.ForEach(cd => UpdateComplianceDocumentForVendorCheck(cd, vendorCheck));
        }

        public void UpdateComplianceForJointCheck(JointPayee jointPayee, ComplianceDocument complianceDocument,
            APRegister check)
        {
            if (IsSameJointVendorInternalId(jointPayee, complianceDocument)
                || IsSameJointVendorExternalName(jointPayee, complianceDocument))
            {
                UpdateComplianceDocument(complianceDocument, check, false);
            }
        }

        public void ClearLinkToPaymentFlag(List<ComplianceDocument> complianceDocuments)
        {
            foreach (var complianceDocument in complianceDocuments)
            {
                complianceDocument.LinkToPayment = false;
                var complianceDocumentCache = graph.Caches<ComplianceDocument>();
                complianceDocumentCache.Update(complianceDocument);
                complianceDocumentCache.PersistUpdated(complianceDocument);
            }
        }

        public List<ComplianceDocument> GetComplianceDocumentsToLink()
        {
            var complianceExtension = graph.GetExtension<ApInvoiceEntryExt>();
            return complianceExtension.ComplianceDocuments.Select().FirstTableItems
                .Where(cd => cd.LinkToPayment == true).ToList();
        }

        private void UpdateComplianceDocumentForVendorCheck(ComplianceDocument complianceDocument,
            APRegister vendorCheck)
        {
            if (!complianceDocument.JointVendorInternalId.HasValue &&
                complianceDocument.JointVendorExternalName.IsNullOrEmpty())
            {
                UpdateComplianceDocument(complianceDocument, vendorCheck, true);
            }
        }

        private void UpdateComplianceDocument(ComplianceDocument complianceDocument, APRegister check,
            bool isVendorCheck)
        {
            if (complianceDocument.DocumentType == GetLienWaiverDocumentTypeId())
            {
                UpdateComplianceDocumentForLienWaiverType(complianceDocument, check, isVendorCheck);
            }
			ComplianceDocumentRefNoteAttribute.SetComplianceDocumentReference<ComplianceDocument.apCheckId>(
				graph.Caches<ComplianceDocument>(), complianceDocument, check.DocType, check.RefNbr, check.NoteID);
		}

        private void UpdateComplianceDocumentForLienWaiverType(ComplianceDocument complianceDocument, IRegister check,
            bool isVendorCheck)
        {
            if (isVendorCheck)
            {
                complianceDocument.JointAmount = null;
                complianceDocument.LienWaiverAmount = check.CuryOrigDocAmt + GetJointAmountToPaySum();
            }
            else
            {
                complianceDocument.JointAmount = check.CuryOrigDocAmt;
                complianceDocument.LienWaiverAmount = check.CuryOrigDocAmt;
            }
        }

        private decimal? GetJointAmountToPaySum()
        {
            return jointPayeePayments?.Sum(x => x.JointAmountToPay);
        }

        private static bool IsSameJointVendorInternalId(JointPayee jointPayee, ComplianceDocument complianceDocument)
        {
            return jointPayee.JointPayeeInternalId.HasValue
                && complianceDocument.JointVendorInternalId.HasValue
                && jointPayee.JointPayeeInternalId == complianceDocument.JointVendorInternalId;
        }

        private static bool IsSameJointVendorExternalName(JointPayee jointPayee, ComplianceDocument complianceDocument)
        {
            var jointPayeeExternalName = jointPayee.JointPayeeExternalName;
            var jointVendorExternalName = complianceDocument.JointVendorExternalName;
            return !jointPayeeExternalName.IsNullOrEmpty()
                && !jointVendorExternalName.IsNullOrEmpty()
                && string.Equals(jointPayeeExternalName.Trim(), jointVendorExternalName.Trim(),
                    StringComparison.CurrentCultureIgnoreCase);
        }

        private int? GetLienWaiverDocumentTypeId()
        {
            return new PXSelect<ComplianceAttributeType,
                    Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.lienWaiver>>>(graph)
                .SelectSingle()?.ComplianceAttributeTypeID;
        }
    }
}
