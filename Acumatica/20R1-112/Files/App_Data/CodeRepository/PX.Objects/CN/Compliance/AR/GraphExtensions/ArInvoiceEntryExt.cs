using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Compliance.AR.CacheExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.AR.GraphExtensions
{
    public class ArInvoiceEntryExt : PXGraphExtension<ARInvoiceEntry>
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
        public PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.invoiceID>>>,
            Where<ComplianceDocumentReference.type, Equal<Current<ARInvoice.docType>>,
                And<ComplianceDocumentReference.referenceNumber, Equal<Current<ARInvoice.refNbr>>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;
        public PXSelect<ComplianceDocumentReference> DocumentReference;

        private ComplianceDocumentService service;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() && !SiteMapExtension.IsInvoicesScreenId();
        }

        public override void Initialize()
        {
            base.Initialize();
            service = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            service.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            service.AddExpirationDateEventHandlers();
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForArInvoice(ComplianceDocuments.Cache);
        }

        public IEnumerable complianceDocuments()
        {
            var documents = GetComplianceDocuments().ToList();
            service.ValidateComplianceDocuments(null, documents, ComplianceDocuments.Cache);
            return documents;
        }

        protected virtual void _(Events.RowSelected<ARTran> args)
        {
            if (args.Row != null)
            {
                ValidateTransaction(args.Row);
            }
        }

        protected virtual void _(Events.RowSelected<ARAdjust2> args)
        {
            if (args.Row != null)
            {
                ValidateAdjustment(args.Row);
            }
        }

        protected virtual void ArInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
            PXRowSelected baseHandler)
        {
            if (args.Row is ARInvoice invoice)
            {
                service.ValidateRelatedProjectField<ARInvoice, ARInvoice.projectID>(invoice, invoice.ProjectID);
                service.ValidateRelatedField<ARInvoice, ComplianceDocument.customerID, ARInvoice.customerID>(invoice,
                    invoice.CustomerID);
                baseHandler(cache, args);
                ComplianceDocuments.Select();
                ComplianceDocuments.AllowInsert = !Base.Document.Cache.Inserted.Any_();
            }
        }

        protected virtual void _(Events.RowUpdated<ComplianceDocument> args)
        {
            Base.Transactions.Cache.ClearItemAttributes();
            Base.Adjustments.Cache.ClearItemAttributes();
            ComplianceDocuments.View.RequestRefresh();
        }

        protected virtual void _(Events.RowSelecting<ARInvoice> args)
        {
            var compliances = GetComplianceDocuments();
            service.ValidateComplianceDocuments(args.Cache, compliances, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowDeleted<ARInvoice> args)
        {
            var invoice = args.Row;
            if (invoice == null)
            {
                return;
            }
            var compliances = GetComplianceDocuments();
            foreach (var compliance in compliances)
            {
                compliance.InvoiceID = null;
                ComplianceDocuments.Update(compliance);
            }
        }

        protected virtual void _(Events.FieldUpdated<ARInvoice.curyOrigDocAmt> args)
        {
            if (args.Row is ARInvoice invoice)
            {
                var compliances = ComplianceDocuments.Select()
                    .FirstTableItems.Where(x => x.InvoiceID == GetComplianceDocumentReferenceId(invoice))
                    .ToList();
                foreach (var compliance in compliances)
                {
                    compliance.InvoiceAmount = invoice.OrigDocAmt;
                    ComplianceDocuments.Update(compliance);
                }
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            service.UpdateExpirationIndicator(args.Row);
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var currentInvoice = Base.Document.Current;
            if (currentInvoice != null)
            {
                var compliance = args.Row;
                if (compliance != null)
                {
                    compliance.CustomerID = currentInvoice.CustomerID;
                    compliance.CustomerName = GetCustomerName(compliance.CustomerID);
                    compliance.InvoiceID = CreateComplianceDocumentReference(currentInvoice).ComplianceDocumentReferenceId;
                    compliance.InvoiceAmount = currentInvoice.OrigDocAmt;
                    compliance.AccountID = GetAccountId();
                }
            }
        }

        private void ValidateTransaction(ARTran transaction)
        {
            var documentHasExpiredCompliance =
                service.ValidateRelatedField<ARTran, ComplianceDocument.revenueTaskID, ARTran.taskID>(transaction,
                    transaction.TaskID);
            service.ValidateRelatedRow<ARTran, ArTranExt.hasExpiredComplianceDocuments>(transaction,
                documentHasExpiredCompliance);
        }

        private void ValidateAdjustment(ARAdjust2 adjustment)
        {
            var documentHasExpiredCompliance =
                service.ValidateRelatedField<ARAdjust2, ComplianceDocument.arPaymentID, ARAdjust2.adjgRefNbr>(adjustment,
                    ComplianceDocumentReferenceRetriever.GetComplianceDocumentReferenceId(Base, adjustment));
            service.ValidateRelatedRow<ARAdjust2, ArAdjust2Ext.hasExpiredComplianceDocuments>(adjustment,
                documentHasExpiredCompliance);
        }

        private int? GetAccountId()
        {
            var transaction = new PXSelect<ARTran,
                Where<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>>,
                OrderBy<Asc<ARTran.lineNbr>>>(Base).SelectSingle();
            return transaction?.AccountID;
        }

        private string GetCustomerName(int? customerId)
        {
            if (!customerId.HasValue)
            {
                return null;
            }
            Customer customer = PXSelect<Customer,
                Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, customerId);
            return customer?.AcctName;
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Document.Current != null)
            {
                using (new PXConnectionScope())
                {
                    return new PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.invoiceID>>>,
                            Where<ComplianceDocumentReference.type, Equal<Current<ARInvoice.docType>>,
                                And<ComplianceDocumentReference.referenceNumber, Equal<Current<ARInvoice.refNbr>>>>>(Base)
                        .Select(Base.Document.Current.DocType, Base.Document.Current.RefNbr).FirstTableItems.ToList();
                }
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        private Guid? GetComplianceDocumentReferenceId(ARInvoice invoice)
        {
            return ComplianceDocumentReferenceRetriever.GetComplianceDocumentReferenceId(Base, invoice);
        }

        private ComplianceDocumentReference CreateComplianceDocumentReference(ARRegister invoice)
        {
            var reference = new ComplianceDocumentReference
            {
                ComplianceDocumentReferenceId = Guid.NewGuid(),
                Type = invoice.DocType,
                ReferenceNumber = invoice.RefNbr,
                RefNoteId = invoice.NoteID
            };
            return DocumentReference.Insert(reference);
        }
    }
}