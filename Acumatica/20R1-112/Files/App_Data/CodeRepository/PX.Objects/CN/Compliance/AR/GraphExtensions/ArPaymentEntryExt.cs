using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.AR.CacheExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.AR.GraphExtensions
{
    public class ArPaymentEntryExt : PXGraphExtension<ARPaymentEntry>
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
                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.arPaymentID>>>,
            Where<ComplianceDocumentReference.type, Equal<Current<ARPayment.docType>>,
                And<ComplianceDocumentReference.referenceNumber, Equal<Current<ARPayment.refNbr>>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;
        public PXSelect<ComplianceDocumentReference> DocumentReference;

        private ComplianceDocumentService service;

        public override void Initialize()
        {
            base.Initialize();
            service = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            service.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            service.AddExpirationDateEventHandlers();
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForArPayment(ComplianceDocuments.Cache);
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

        public IEnumerable adjustments_History()
        {
            foreach (PXResult result in GetAdjustmentHistory())
            {
                ValidateAdjustments<ARAdjust.displayRefNbr, ARAdjust.displayCustomerID>(result.GetItem<ARAdjust>());
                yield return result;
            }
        }

        protected virtual void _(Events.RowSelected<ARAdjust> args)
        {
            if (args.Row != null)
            {
                ValidateAdjustments<ARAdjust.adjdRefNbr, ARAdjust.adjdCustomerID>(args.Row);
            }
        }

        protected virtual void _(Events.RowSelecting<ARPayment> args)
        {
            var documents = GetComplianceDocuments();
            service.ValidateComplianceDocuments(args.Cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void ArPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments,
            PXRowSelected baseHandler)
        {
            if (arguments.Row is ARPayment)
            {
                baseHandler(cache, arguments);
                Base.Document.Cache.AllowUpdate = true;
                ComplianceDocuments.Select();
                ComplianceDocuments.AllowInsert = !Base.Document.Cache.Inserted.Any_();
            }
        }

        protected virtual void _(Events.RowSelected<ARPayment> args)
        {
            if (args.Row is ARPayment payment)
            {
                service.ValidateRelatedField<ARPayment, ComplianceDocument.customerID, ARPayment.customerID>(
                    payment, payment.CustomerID);
            }
        }

        protected virtual void _(Events.RowDeleted<ARPayment> args)
        {
            var payment = args.Row;
            if (payment == null)
            {
                return;
            }
            var documents = GetComplianceDocuments();
            foreach (var document in documents)
            {
                document.ArPaymentID = null;
                ComplianceDocuments.Update(document);
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            service.UpdateExpirationIndicator(args.Row);
        }

        protected virtual void _(Events.RowInserted<ComplianceDocument> args)
        {
            var currentArPayment = Base.Document.Current;
            if (currentArPayment != null)
            {
                var complianceDocument = args.Row;
                if (complianceDocument != null)
                {
                    complianceDocument.CustomerID = currentArPayment.CustomerID;
                    complianceDocument.CustomerName = GetCustomerName(complianceDocument.CustomerID);
                    complianceDocument.ArPaymentID = CreateComplianceDocumentReference(currentArPayment)
                        .ComplianceDocumentReferenceId;
                    complianceDocument.ArPaymentMethodID = currentArPayment.PaymentMethodID;
                }
            }
        }

        private void ValidateAdjustments<TInvoiceField, TCustomerField>(ARAdjust adjustment)
            where TInvoiceField : IBqlField
            where TCustomerField : IBqlField
        {
            var arInvoice = GetArInvoice(adjustment);
            if (arInvoice == null)
            {
                return;
            }
            var invoiceHasExpiredCompliance =
                service.ValidateRelatedField<ARAdjust, ComplianceDocument.invoiceID, TInvoiceField>(adjustment,
                    ComplianceDocumentReferenceRetriever.GetComplianceDocumentReferenceId(Base, arInvoice));
            var customerHasExpiredCompliance =
                service.ValidateRelatedField<ARAdjust, ComplianceDocument.customerID, TCustomerField>(adjustment,
                    arInvoice.CustomerID);
            service.ValidateRelatedRow<ARAdjust, ArAdjustExt.hasExpiredComplianceDocuments>(adjustment,
                invoiceHasExpiredCompliance || customerHasExpiredCompliance);
        }

        private ARInvoice GetArInvoice(ARAdjust adjustment)
        {
            return new PXSelect<ARInvoice,
                    Where<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>,
                        And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>>>>(Base)
                .SelectSingle(adjustment.AdjdRefNbr, adjustment.AdjdDocType);
        }

        private IEnumerable GetAdjustmentHistory()
        {
            return (IEnumerable) Base.GetType()
                .GetMethod("adjustments_history", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(Base, null);
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
                                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.arPaymentID>>>,
                            Where<ComplianceDocumentReference.type, Equal<Current<ARPayment.docType>>,
                                And<ComplianceDocumentReference.referenceNumber, Equal<Current<ARPayment.refNbr>>>>>(Base)
                        .Select(Base.Document.Current.DocType, Base.Document.Current.RefNbr).FirstTableItems.ToList();
                }
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        private ComplianceDocumentReference CreateComplianceDocumentReference(ARRegister arPayment)
        {
            var reference = new ComplianceDocumentReference
            {
                ComplianceDocumentReferenceId = Guid.NewGuid(),
                Type = arPayment.DocType,
                ReferenceNumber = arPayment.RefNbr,
                RefNoteId = arPayment.NoteID
            };
            return DocumentReference.Insert(reference);
        }

        public static bool IsActive()
        {
	        return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
	}
}
