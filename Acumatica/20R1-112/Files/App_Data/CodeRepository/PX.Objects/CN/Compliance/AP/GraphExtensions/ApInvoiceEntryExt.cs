using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Compliance.AP.CacheExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.SM;
using ApTranExt = PX.Objects.CN.Subcontracts.AP.CacheExtensions.ApTranExt;

namespace PX.Objects.CN.Compliance.AP.GraphExtensions
{
    public class ApInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
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
                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.billID>>>,
            Where<ComplianceDocumentReference.type, Equal<Current<APInvoice.docType>>,
                And<ComplianceDocumentReference.referenceNumber, Equal<Current<APInvoice.refNbr>>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;
        public PXSelect<ComplianceDocumentReference> DocumentReference;

        private ComplianceDocumentService service;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() && !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        public override void Initialize()
        {
            base.Initialize();
            service = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            service.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            service.AddExpirationDateEventHandlers();
            ComplianceDocumentFieldVisibilitySetter.ConfigureComplianceGridColumnsForApBill(ComplianceDocuments.Cache);
        }

        public IEnumerable complianceDocuments()
        {
            var documents = GetComplianceDocuments().ToList();
            service.ValidateComplianceDocuments(null, documents, ComplianceDocuments.Cache);
            return documents;
        }

        public virtual void _(Events.RowUpdated<ComplianceDocument> args)
        {
            ComplianceDocuments.View.RequestRefresh();
        }

        protected void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs args, PXRowSelected baseHandler)
        {
            if (args.Row is APInvoice bill)
            {
                service.ValidateRelatedField<APInvoice, ComplianceDocument.vendorID, APInvoice.vendorID>(bill,
                    bill.VendorID);
                baseHandler(cache, args);
                ComplianceDocuments.Select();
                ComplianceDocuments.AllowInsert = !Base.Document.Cache.Inserted.Any_();
            }
        }

        protected virtual void _(Events.RowPersisted<APInvoice> args)
        {
            ComplianceDocuments.AllowInsert = true;
        }

		[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2)]
        protected virtual void _(Events.RowSelecting<APInvoice> args)
        {
        }

		protected virtual void _(Events.RowSelected<APInvoice> args)
        {
            var compliances = GetComplianceDocuments();
            service.ValidateComplianceDocuments(args.Cache, compliances, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowDeleted<APInvoice> args)
        {
            var bill = args.Row;
            if (bill == null)
            {
                return;
            }
            var compliances = GetComplianceDocuments();
            foreach (var compliance in compliances)
            {
                compliance.BillID = null;
                ComplianceDocuments.Update(compliance);
            }
        }

        protected virtual void _(Events.FieldUpdated<APInvoice.curyOrigDocAmt> args)
        {
            if (args.Row is APInvoice bill)
            {
                var compliances = ComplianceDocuments.Select().FirstTableItems
                    .Where(x => x.InvoiceID == GetComplianceDocumentReferenceId(bill))
                    .ToList();
                foreach (var compliance in compliances)
                {
                    compliance.BillAmount = bill.OrigDocAmt;
                    ComplianceDocuments.Update(compliance);
                }
            }
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var currentBill = Base.Document.Current;
            var compliance = args.Row;
            if (currentBill != null && compliance != null)
            {
                var transaction = Base.Transactions.Select().FirstTableItems.OrderBy(x => x.LineNbr).FirstOrDefault();
                FillInvoiceInfo(compliance, currentBill, transaction);
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            service.UpdateExpirationIndicator(args.Row);
        }

        protected virtual void _(Events.RowSelected<APTran> args)
        {
            if (args.Row != null)
            {
                ValidateTransaction(args.Row);
            }
        }

        protected virtual void _(Events.RowSelected<APInvoiceEntry.APAdjust> args)
        {
            if (args.Row != null)
            {
                ValidateAdjustment(args.Row);
            }
        }

        private void FillInvoiceInfo(ComplianceDocument complianceDocument, APInvoice invoice, APTran transaction)
        {
            complianceDocument.VendorID = invoice.VendorID;
            complianceDocument.VendorName = GetVendorName(complianceDocument.VendorID);
			ComplianceDocumentRefNoteAttribute.SetComplianceDocumentReference<ComplianceDocument.billID>(
				ComplianceDocuments.Cache, complianceDocument, invoice.DocType, invoice.RefNbr, invoice.NoteID);

			complianceDocument.BillAmount = invoice.OrigDocAmt;
            complianceDocument.AccountID = GetAccountId();
            complianceDocument.ProjectID = transaction?.ProjectID;
            complianceDocument.CostTaskID = transaction?.TaskID;
            complianceDocument.CostCodeID = transaction?.CostCodeID;
        }

        private void ValidateAdjustment(APInvoiceEntry.APAdjust adjustment)
        {
            var referenceId = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReferenceId(Base, adjustment);
            var hasExpiredComplianceDocuments = service.ValidateRelatedField<
                APInvoiceEntry.APAdjust, ComplianceDocument.apCheckId, APAdjust.displayRefNbr>(adjustment, referenceId);
            service.ValidateRelatedRow<APInvoiceEntry.APAdjust, ApAdjustExt.hasExpiredComplianceDocuments>(
                adjustment, hasExpiredComplianceDocuments);
        }

        private void ValidateTransaction(APTran transaction)
        {
            var projectHasExpiredCompliance =
                service.ValidateRelatedProjectField<APTran, APTran.projectID>(transaction, transaction.ProjectID);
            var taskHasExpiredCompliance =
                service.ValidateRelatedField<APTran, ComplianceDocument.costTaskID, APTran.taskID>(transaction,
                    transaction.TaskID);
            var purchaseOrderOrSubcontractHasExpiredCompliance = IsSubcontract(transaction)
                ? service.ValidateRelatedField<APTran, ComplianceDocument.subcontract,
                    ApTranExt.subcontractNbr>(transaction, transaction.PONbr)
                : service.ValidateRelatedField<APTran, ComplianceDocument.purchaseOrder, APTran.pONbr>(
                    transaction, ComplianceDocumentReferenceRetriever.GetComplianceDocumentReferenceId(Base, transaction));
            service.ValidateRelatedRow<APTran, PX.Objects.CN.Compliance.AP.CacheExtensions.ApTranExt.hasExpiredComplianceDocuments>(transaction,
                projectHasExpiredCompliance || taskHasExpiredCompliance || purchaseOrderOrSubcontractHasExpiredCompliance);
        }

        private int? GetAccountId()
        {
            var transaction = new PXSelect<APTran,
                Where<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>,
                OrderBy<Asc<APTran.lineNbr>>>(Base).SelectSingle();
            return transaction?.AccountID;
        }

        private string GetVendorName(int? vendorId)
        {
            if (!vendorId.HasValue)
            {
                return null;
            }
            Vendor vendor = PXSelect<Vendor,
                Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, vendorId);
            return vendor?.AcctName;
        }

        private bool IsSubcontract(APTran apTran)
        {
            var order = GetPurchaseOrder(apTran);
            return order != null && order.OrderType == POOrderType.RegularSubcontract;
        }

        private POOrder GetPurchaseOrder(APTran apTran)
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                    And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>(Base);
            return query.SelectSingle(apTran.POOrderType, apTran.PONbr);
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Document.Current != null)
            {
	            return new PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.billID>>>,
                            Where<ComplianceDocumentReference.type, Equal<Current<APInvoice.docType>>,
                                And<ComplianceDocumentReference.referenceNumber, Equal<Current<APInvoice.refNbr>>>>>(Base)
                        .Select(Base.Document.Current.DocType, Base.Document.Current.RefNbr).FirstTableItems.ToList();
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        private Guid? GetComplianceDocumentReferenceId(APInvoice bill)
        {
            return ComplianceDocumentReferenceRetriever.GetComplianceDocumentReferenceId(Base, bill);
        }
    }
}
