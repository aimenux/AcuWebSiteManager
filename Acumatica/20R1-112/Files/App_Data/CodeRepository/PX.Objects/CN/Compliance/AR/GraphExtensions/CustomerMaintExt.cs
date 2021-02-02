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

namespace PX.Objects.CN.Compliance.AR.GraphExtensions
{
    public class CustomerMaintExt : PXGraphExtension<CustomerMaint>
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
            Where<ComplianceDocument.customerID, Equal<Current<Customer.bAccountID>>>> ComplianceDocuments;

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
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForCustomer(ComplianceDocuments.Cache);
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
            var customer = Base.BAccount.Current;
            if (customer == null)
            {
                return;
            }
            var complianceDocument = args.Row;
            if (complianceDocument == null)
            {
                return;
            }
            complianceDocument.CustomerID = customer.BAccountID;
            complianceDocument.CustomerName = customer.AcctName;
            complianceDocument.AccountID = Base.CustomerClass.Current?.ARAcctID;
        }

        protected virtual void Customer_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments)
        {
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.BAccount.Cache.Inserted.Any_();
        }

        protected virtual void Customer_RowSelecting(PXCache cache, PXRowSelectingEventArgs arguments)
        {
            var documents = GetComplianceDocuments();
            service.ValidateComplianceDocuments(cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowDeleted<Customer> args)
        {
            if (args.Row == null)
            {
                return;
            }
            var documents = GetComplianceDocuments();
            foreach (var document in documents)
            {
                document.CustomerID = null;
                ComplianceDocuments.Update(document);
            }
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.BAccount.Current != null)
            {
                using (new PXConnectionScope())
                {
                    return new PXSelect<ComplianceDocument,
                            Where<ComplianceDocument.customerID, Equal<Required<Customer.bAccountID>>>>(Base)
                        .Select(Base.BAccount.Current.BAccountID).FirstTableItems.ToList();
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