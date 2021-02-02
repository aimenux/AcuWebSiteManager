using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.AP.CacheExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.CL.Services.DataProviders;
using PX.Objects.CN.Compliance.PO.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.PO;
using ApPaymentExt = PX.Objects.CN.JointChecks.AP.CacheExtensions.ApPaymentExt;

namespace PX.Objects.CN.JointChecks.AP.Services.Creators
{
    public class LienWaiverCreator : ILienWaiverCreator
    {
        private LienWaiverGenerationKey generationKey;
        private ComplianceDocument lienWaiver;

        public LienWaiverCreator(PXGraph graph)
        {
            Graph = (APPaymentEntry) graph;
            LienWaiverTransactionRetriever = Graph.GetService<ILienWaiverTransactionRetriever>();
            BusinessAccountDataProvider = Graph.GetService<IBusinessAccountDataProvider>();
            ComplianceAttributeTypeDataProvider = Graph.GetService<IComplianceAttributeTypeDataProvider>();
            ProjectDataProvider = Graph.GetService<IProjectDataProvider>();
            ProjectTaskDataProvider = Graph.GetService<IProjectTaskDataProvider>();
            LienWaiverDataProvider = Graph.GetService<ILienWaiverDataProvider>();
        }

        public ILienWaiverTransactionRetriever LienWaiverTransactionRetriever
        {
            get;
            set;
        }

        public IBusinessAccountDataProvider BusinessAccountDataProvider
        {
            get;
            set;
        }

        public IComplianceAttributeTypeDataProvider ComplianceAttributeTypeDataProvider
        {
            get;
            set;
        }

        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        public IProjectTaskDataProvider ProjectTaskDataProvider
        {
            get;
            set;
        }

        public ILienWaiverDataProvider LienWaiverDataProvider
        {
            get;
            set;
        }

        private APPaymentEntry Graph
        {
            get;
        }

        private LienWaiverSetup LienWaiverSetup =>
            Graph.GetExtension<ApPaymentEntryLienWaiverExtension>().LienWaiverSetup.Current;

        private PXCache Cache => Graph.Caches[typeof(ComplianceDocument)];

        public void CreateLienWaiver(LienWaiverGenerationKey lienWaiverGenerationKey, APPayment payment,
            ComplianceAttribute complianceAttribute)
        {
            generationKey = lienWaiverGenerationKey;
            CreateLienWaiver(payment, complianceAttribute);
            Cache.Update(lienWaiver);
            Cache.Persist(PXDBOperation.Insert);
            Cache.Persisted(false);
        }

        private void CreateLienWaiver(APPayment payment, ComplianceAttribute complianceAttribute)
        {
            lienWaiver = CreateLienWaiver();
            lienWaiver.DocumentTypeValue = complianceAttribute.AttributeId;
            lienWaiver.Required = true;
            lienWaiver.IsCreatedAutomatically = true;
            SetProjectDependentFields();
            SetCommitments();
            SetVendorDependentFields();
            SetTransactionDependentFields(payment, complianceAttribute);
            SetJointPayeeVendorDependentFields(payment, complianceAttribute);
            lienWaiver.ApPaymentMethodID = payment.PaymentMethodID;
        }

        private void SetTransactionDependentFields(APPayment payment, ComplianceAttribute complianceAttribute)
        {
            var transactions = LienWaiverTransactionRetriever.GetTransactions(generationKey).ToList();
            SetBillId(transactions);
            lienWaiver.LienWaiverAmount = GetLienWaiverAmount(transactions, complianceAttribute.Value);
            SetLienNoticeAmount();
            SetAccountId(transactions);
			ComplianceDocumentRefNoteAttribute.SetComplianceDocumentReference<ComplianceDocument.apCheckId>(
				Cache, lienWaiver, payment.DocType, payment.RefNbr, payment.NoteID);
			lienWaiver.PaymentDate = payment.AdjDate;
            lienWaiver.ThroughDate = GetThroughDate(complianceAttribute.Value, payment, transactions);
            lienWaiver.SourceType = ComplianceDocumentSourceTypeAttribute.ApBill;
        }

        private void SetLienNoticeAmount()
        {
            var lienWaivers = LienWaiverDataProvider.GetNotVoidedLienWaivers(generationKey);
            var lienNoticeAmounts = lienWaivers.Select(lw => lw.LienNoticeAmount).Where(lna => lna != null).Distinct();
            lienWaiver.LienNoticeAmount = lienNoticeAmounts.SingleOrNull();
        }

        private void SetJointPayeeVendorDependentFields(APPayment payment, ComplianceAttribute complianceAttribute)
        {
            var paymentExtension = PXCache<APPayment>.GetExtension<ApPaymentExt>(payment);
            lienWaiver.IsRequiredJointCheck = paymentExtension.IsJointCheck ?? false;
            lienWaiver.JointVendorInternalId = generationKey.JointPayeeVendorId;
            var jointPayeePayments = GetJointPayeePaymentsWithJointPayees(payment).ToList();
            SetJointAmount(jointPayeePayments);
            SetJointLienWaiverAmount(complianceAttribute, jointPayeePayments);
        }

        private IEnumerable<PXResult<JointPayeePayment>> GetJointPayeePaymentsWithJointPayees(APRegister payment)
        {
            var insertedJointPayeePayments = Graph.Caches<JointPayeePayment>().Inserted.RowCast<JointPayeePayment>();
            var insertedJointPayeePaymentsAndJointPayees =
                insertedJointPayeePayments.Select(GetInsertedJointPayeePaymentsWithJointPayees);
            var jointPayeePaymentsAndJointPayees =
                JointPayeePaymentDataProvider.GetJointPayeePaymentsAndJointPayees(Graph, payment);
            return jointPayeePaymentsAndJointPayees.Concat(insertedJointPayeePaymentsAndJointPayees).AsEnumerable()
                .Where(DoesJointPayeeInternalIdMatchGenerationKey);
        }

        private PXResult<JointPayeePayment, JointPayee> GetInsertedJointPayeePaymentsWithJointPayees(
            JointPayeePayment jointPayeePayment)
        {
            var jointPayee = JointPayeeDataProvider.GetJointPayee(Graph, jointPayeePayment);
            return new PXResult<JointPayeePayment, JointPayee>(jointPayeePayment, jointPayee);
        }

        private void SetJointLienWaiverAmount(ComplianceAttribute complianceAttribute,
            IReadOnlyCollection<PXResult<JointPayeePayment>> jointPayeePayments)
        {
            var jointLienWaiverAmount = GetJointLienWaiverAmount(jointPayeePayments, complianceAttribute.Value);
            lienWaiver.JointLienWaiverAmount = jointLienWaiverAmount == decimal.Zero
                ? null
                : jointLienWaiverAmount;
        }

        private void SetJointAmount(IReadOnlyCollection<PXResult<JointPayeePayment>> jointPayeePayments)
        {
            var jointAmount = GetJointAmount(jointPayeePayments);
            lienWaiver.JointAmount = jointAmount == decimal.Zero
                ? null
                : jointAmount;
        }

        private bool DoesJointPayeeInternalIdMatchGenerationKey(PXResult jointPayeePayment)
        {
            var jointPayee = jointPayeePayment.GetItem<JointPayee>();
            return jointPayee.JointPayeeInternalId != null &&
                jointPayee.JointPayeeInternalId == generationKey.JointPayeeVendorId;
        }

        private void SetVendorDependentFields()
        {
            lienWaiver.VendorID = generationKey.VendorId;
            lienWaiver.VendorName =
                BusinessAccountDataProvider.GetBusinessAccount(Graph, generationKey.VendorId).AcctName;
        }

        private void SetProjectDependentFields()
        {
            SetCostTask();
            SetCostCode();
            SetCustomer();
        }

        private void SetCommitments()
        {
            SetPurchaseOrder();
            SetPurchaseOrderLine();
            SetSubcontract();
            SetSubcontractLine();
        }

        private void SetAccountId(IEnumerable<APTran> transactions)
        {
            var accountIds = transactions.Select(tran => tran.AccountID).Distinct();
            lienWaiver.AccountID = accountIds.SingleOrNull();
        }

        private void SetBillId(IEnumerable<APTran> transactions)
        {
            var invoices = InvoiceDataProvider.GetInvoices(Graph, transactions);
            var invoice = invoices.SingleOrNull();

			if (invoice != null)
			{
				ComplianceDocumentRefNoteAttribute.SetComplianceDocumentReference<ComplianceDocument.billID>(
					Cache, lienWaiver, invoice.DocType, invoice.RefNbr, invoice.NoteID);
			}
		}

        private void SetSubcontract()
        {
            lienWaiver.Subcontract = lienWaiver.PurchaseOrder == null
                ? generationKey.OrderNumber
                : null;
            Cache.Update(lienWaiver);
        }

        private void SetSubcontractLine()
        {
            var subcontractOrderLines = CommitmentDataProvider.GetCommitmentLines(Graph, generationKey.OrderNumber,
                POOrderType.RegularSubcontract, generationKey.ProjectId);
            var subcontractLineItem = subcontractOrderLines.SingleOrNull()?.LineNbr;
            Cache.SetValueExt<ComplianceDocument.subcontractLineItem>(lienWaiver, subcontractLineItem);
        }

        private void SetPurchaseOrder()
        {
            var purchaseOrder =
                CommitmentDataProvider.GetCommitment(Graph, generationKey.OrderNumber, POOrderType.RegularOrder);
            if (purchaseOrder == null)
            {
                return;
            }
			ComplianceDocumentRefNoteAttribute.SetComplianceDocumentReference<ComplianceDocument.purchaseOrder>(
				Cache, lienWaiver, purchaseOrder.OrderType, purchaseOrder.OrderNbr, purchaseOrder.NoteID);
			Cache.Update(lienWaiver);
        }

        private void SetPurchaseOrderLine()
        {
            var purchaseOrderLines = CommitmentDataProvider.GetCommitmentLines(Graph, generationKey.OrderNumber,
                POOrderType.RegularOrder, generationKey.ProjectId);
            var purchaseOrderLineItem = purchaseOrderLines.SingleOrNull()?.LineNbr;
            Cache.SetValueExt<ComplianceDocument.purchaseOrderLineItem>(lienWaiver, purchaseOrderLineItem);
        }

        private void SetCustomer()
        {
            var project = ProjectDataProvider.GetProject(Graph, generationKey.ProjectId);
            lienWaiver.CustomerID = project.CustomerID;
            lienWaiver.CustomerName =
                BusinessAccountDataProvider.GetBusinessAccount(Graph, project.CustomerID)?.AcctName;
        }

        private void SetCostCode()
        {
            var transactionCostCodeId =
                LienWaiverTransactionRetriever.GetTransactions(generationKey).SingleOrNull()?.CostCodeID;
            var costCodes = BudgetedCostCodeDataProvider.GetBudgetedCostCodes(Graph, generationKey.ProjectId,
                lienWaiver.CostTaskID ?? lienWaiver.RevenueTaskID);
            lienWaiver.CostCodeID = costCodes.FirstOrDefault(cc => cc.CostCodeID == transactionCostCodeId)?.CostCodeID;
        }

        private void SetCostTask()
        {
            var transactionTaskId =
                LienWaiverTransactionRetriever.GetTransactions(generationKey).SingleOrNull()?.TaskID;
            var costTasks = ProjectTaskDataProvider.GetProjectTasks<ProjectTaskType.cost>(Graph, generationKey.ProjectId);
            var combinedTasks =
                ProjectTaskDataProvider.GetProjectTasks<ProjectTaskType.costRevenue>(Graph, generationKey.ProjectId);
            var tasks = costTasks.Concat(combinedTasks);
            lienWaiver.CostTaskID = tasks.FirstOrDefault(t => t.TaskID == transactionTaskId)?.TaskID;
        }

        private ComplianceDocument CreateLienWaiver()
        {
            lienWaiver = new ComplianceDocument
            {
                DocumentType = ComplianceAttributeTypeDataProvider.GetComplianceAttributeType(
                    Graph, ComplianceDocumentType.LienWaiver).ComplianceAttributeTypeID,
                ProjectID = generationKey.ProjectId
            };
            return (ComplianceDocument) Cache.Insert(lienWaiver);
        }

        private DateTime? GetThroughDate(string complianceAttributeValue, APPayment payment,
            IEnumerable<APTran> transactions)
        {
            var throughDateSource = complianceAttributeValue.IsIn(
                Constants.LienWaiverDocumentTypeValues.ConditionalFinal,
                Constants.LienWaiverDocumentTypeValues.ConditionalPartial)
                ? LienWaiverSetup.ThroughDateSourceConditional
                : LienWaiverSetup.ThroughDateSourceUnconditional;
            switch (throughDateSource)
            {
                case LienWaiverThroughDateSource.BillDate:
                    return InvoiceDataProvider.GetInvoices(Graph, transactions).Max(invoice => invoice.DocDate);
                case LienWaiverThroughDateSource.PaymentDate:
                    return payment.AdjDate;
                default:
                    return FinancialPeriodDataProvider.GetFinancialPeriod(Graph, payment.AdjFinPeriodID).EndDate
                        .GetValueOrDefault().AddDays(-1);
            }
        }

        private decimal? GetJointLienWaiverAmount(IReadOnlyCollection<PXResult<JointPayeePayment>> jointPayeePayments,
            string documentTypeOption)
        {
            if (IsCheckSingleOrJoint(jointPayeePayments))
            {
                return documentTypeOption == Constants.LienWaiverDocumentTypeValues.ConditionalFinal
                    ? LienWaiverAmountCalculationService.GetJointAmountOwed(jointPayeePayments)
                    : LienWaiverAmountCalculationService.GetJointAmountToPay(jointPayeePayments);
            }
            return null;
        }

        private decimal? GetJointAmount(IReadOnlyCollection<PXResult<JointPayeePayment>> jointPayeePayments)
        {
            return IsCheckSingleOrJoint(jointPayeePayments)
                ? LienWaiverAmountCalculationService.GetJointAmountToPay(jointPayeePayments)
                : null;
        }

        private bool IsCheckSingleOrJoint(IEnumerable<PXResult<JointPayeePayment>> jointPayeePayments)
        {
            return jointPayeePayments.Sum(jpp => jpp.GetItem<JointPayeePayment>().JointAmountToPay) > 0;
        }

        private decimal? GetLienWaiverAmount(IEnumerable<APTran> transactions,
            string documentTypeOption)
        {
            var insertedAdjustments = Graph.Caches<APAdjust>().Inserted.RowCast<APAdjust>();
            return documentTypeOption == Constants.LienWaiverDocumentTypeValues.ConditionalFinal
                ? LienWaiverAmountCalculationService.GetBillAmount(transactions)
                : LienWaiverAmountCalculationService.GetAmountPaid(Graph.Adjustments.SelectMain()
                    .Concat(insertedAdjustments));
        }
    }
}