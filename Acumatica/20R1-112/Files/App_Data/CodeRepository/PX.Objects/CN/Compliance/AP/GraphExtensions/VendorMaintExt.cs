using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.AP.GraphExtensions
{
    public class VendorMaintExt : PXGraphExtension<VendorMaint>
    {
        [PXCopyPasteHiddenView]
        [PXViewDetailsButton(typeof(Customer), typeof(Select<Customer,
            Where<Customer.bAccountID, Equal<Current<ComplianceDocument.customerID>>>>))]
        [PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
            Where<Vendor.bAccountID, Equal<Current<ComplianceDocument.vendorID>>>>))]
        [PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
            Where<Vendor.bAccountID, Equal<Current<ComplianceDocument.secondaryVendorID>>>>))]
        [PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
            Where<Vendor.bAccountID, Equal<Current<ComplianceDocument.jointVendorInternalId>>>>))]
        [PXViewDetailsButton(typeof(PMProject), typeof(Select<PMProject,
            Where<PMProject.contractID, Equal<Current<ComplianceDocument.projectID>>>>))]
        [PXViewDetailsButton(typeof(PMTask), typeof(Select<PMTask,
            Where<PMTask.taskID, Equal<Current<ComplianceDocument.costTaskID>>>>))]
        [PXViewDetailsButton(typeof(PMTask), typeof(Select<PMTask,
            Where<PMTask.taskID, Equal<Current<ComplianceDocument.revenueTaskID>>>>))]
        [PXViewDetailsButton(typeof(PMCostCode), typeof(Select<PMCostCode,
            Where<PMCostCode.costCodeID, Equal<Current<ComplianceDocument.costCodeID>>>>))]
        public PXSelect<ComplianceDocument,
            Where<ComplianceDocument.vendorID, Equal<Current<VendorR.bAccountID>>,
                Or<ComplianceDocument.secondaryVendorID, Equal<Current<VendorR.bAccountID>>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;

        private ComplianceDocumentService service;

        public override void Initialize()
        {
            base.Initialize();
            service = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            service.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            service.AddExpirationDateEventHandlers();
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForVendor(ComplianceDocuments.Cache);
        }

        public virtual void _(Events.RowUpdated<ComplianceDocument> args)
        {
            ComplianceDocuments.View.RequestRefresh();
        }

        public IEnumerable complianceDocuments()
        {
            var documents = GetComplianceDocuments().ToList();
            service.ValidateComplianceDocuments(null, documents, ComplianceDocuments.Cache);
            return documents;
        }

        protected virtual void ComplianceDocument_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments)
        {
            service.UpdateExpirationIndicator(arguments.Row as ComplianceDocument);
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var vendor = Base.BAccount.Current;
            if (vendor == null)
            {
                return;
            }
            var complianceDocument = args.Row;
            if (complianceDocument == null)
            {
                return;
            }
            complianceDocument.VendorID = vendor.BAccountID;
            complianceDocument.VendorName = vendor.AcctName;
        }

        protected virtual void Vendor_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments)
        {
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.BAccount.Cache.Inserted.Any_();
        }

        protected virtual void Vendor_RowSelecting(PXCache cache, PXRowSelectingEventArgs arguments)
        {
            var documents = GetComplianceDocuments();
            service.ValidateComplianceDocuments(cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void Vendor_RowDeleted(PXCache cache, PXRowDeletedEventArgs arguments)
        {
            if (arguments.Row is Vendor vendor)
            {
                UpdateDeletedVendorComplianceDocuments(vendor.BAccountID);
            }
        }

        private void UpdateDeletedVendorComplianceDocuments(int? vendorId)
        {
            var documents = GetComplianceDocuments();
            foreach (var document in documents)
            {
                UpdateVendorReference(document, vendorId);
            }
        }

        private void UpdateVendorReference(ComplianceDocument document, int? vendorId)
        {
            if (document.VendorID == vendorId)
            {
                document.VendorID = null;
            }
            if (document.SecondaryVendorID == vendorId)
            {
                document.SecondaryVendorID = null;
            }
            if (document.JointVendorInternalId == vendorId)
            {
                document.JointVendorInternalId = null;
            }
            ComplianceDocuments.Update(document);
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.CurrentVendor.Current != null)
            {
                using (new PXConnectionScope())
                {
                    return new PXSelect<ComplianceDocument,
                            Where<ComplianceDocument.vendorID, Equal<Required<VendorR.bAccountID>>,
                                Or<ComplianceDocument.secondaryVendorID, Equal<Required<VendorR.bAccountID>>,
                                    Or<ComplianceDocument.jointVendorInternalId, Equal<Required<VendorR.bAccountID>>>>>>(Base)
                        .Select(Base.CurrentVendor.Current.BAccountID, Base.CurrentVendor.Current.BAccountID,
                            Base.CurrentVendor.Current.BAccountID).FirstTableItems.ToList();
                }
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        public static bool IsActive()
        {
	        return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
    }
}
