using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.PM.CacheExtensions;
using PX.Objects.CN.Compliance.PM.Services;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.PM.GraphExtensions
{
    public class ChangeOrderEntryExt : PXGraphExtension<ChangeOrderEntry>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

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
            Where<ComplianceDocument.changeOrderNumber, Equal<Current<PMChangeOrder.refNbr>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;

        public PXSetup<LienWaiverSetup> LienWaiverSetup;

        private ComplianceDocumentService complianceDocumentService;

        private ChangeOrderValidationService changeOrderValidationService;

        public override void Initialize()
        {
            ValidateComplianceSetup();
            complianceDocumentService = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            complianceDocumentService.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            complianceDocumentService.AddExpirationDateEventHandlers();
            changeOrderValidationService = new ChangeOrderValidationService(Base, ComplianceDocuments);
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForChangeOrder(ComplianceDocuments.Cache);
        }

        private void ValidateComplianceSetup()
        {
            if (LienWaiverSetup.Current == null)
                throw new PXSetupNotEnteredException<LienWaiverSetup>();
        }

        public IEnumerable complianceDocuments()
        {
            var documents = GetComplianceDocuments().ToList();
            complianceDocumentService.ValidateComplianceDocuments(null, documents, ComplianceDocuments.Cache);
            return documents;
        }

        public virtual void _(Events.RowUpdated<ComplianceDocument> args)
        {
            ComplianceDocuments.View.RequestRefresh();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
        [PXSelector(typeof(Search5<PMTask.taskID,
                LeftJoin<PMChangeOrderLine, On<PMChangeOrderLine.taskID, Equal<PMTask.taskID>>,
                    LeftJoin<PMChangeOrderBudget, On<PMChangeOrderBudget.projectTaskID, Equal<PMTask.taskID>>>>,
                Where2<Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
                        Or<PMChangeOrderBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>>>,
                    And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>,
                Aggregate<GroupBy<PMTask.taskID>>>),
            SubstituteKey = typeof(PMTask.taskCD),
            DescriptionField = typeof(PMTask.description))]
        protected virtual void ComplianceDocument_CostTaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
        [PXSelector(typeof(Search5<PMTask.taskID,
                LeftJoin<PMChangeOrderLine, On<PMChangeOrderLine.taskID, Equal<PMTask.taskID>>,
                    LeftJoin<PMChangeOrderBudget, On<PMChangeOrderBudget.projectTaskID, Equal<PMTask.taskID>>>>,
                Where2<Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
                        Or<PMChangeOrderBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>>>,
                    And<PMTask.type, NotEqual<ProjectTaskType.cost>>>,
                Aggregate<GroupBy<PMTask.taskID>>>),
            SubstituteKey = typeof(PMTask.taskCD),
            DescriptionField = typeof(PMTask.description))]
        protected virtual void ComplianceDocument_RevenueTaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXDimensionSelectorAttribute))]
        [PXDimensionSelector(PMCostCode.costCodeCD.DimensionName, typeof(Search5<PMCostCode.costCodeID,
            LeftJoin<PMChangeOrderLine, On<PMChangeOrderLine.costCodeID, Equal<PMCostCode.costCodeID>>,
                LeftJoin<PMChangeOrderBudget, On<PMChangeOrderBudget.costCodeID, Equal<PMCostCode.costCodeID>>>>,
            Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
                Or<PMChangeOrderBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>>>,
            Aggregate<GroupBy<PMCostCode.costCodeID>>>), typeof(PMCostCode.costCodeCD))]
        protected virtual void ComplianceDocument_CostCodeID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.RowSelected<PMChangeOrder> args)
        {
            if (args.Row is PMChangeOrder changeOrder)
            {
                complianceDocumentService.ValidateRelatedProjectField<PMChangeOrder, PMChangeOrder.projectID>(
                    changeOrder, changeOrder.ProjectID);
            }
        }

        protected virtual void PMChangeOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
            PXRowSelected baseHandler)
        {
            if (!(args.Row is PMChangeOrder))
            {
                return;
            }
            baseHandler(cache, args);
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.Document.Cache.Inserted.Any_();
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            var complianceDocument = args.Row;
            if (complianceDocument != null)
            {
                complianceDocumentService.UpdateExpirationIndicator(args.Row);
                changeOrderValidationService.ValidateComplianceFields(complianceDocument);
            }
        }

        protected virtual void _(Events.RowSelected<PMChangeOrderLine> args)
        {
            if (args.Row != null)
            {
                ValidateChangeOrderLine(args.Row);
            }
        }

        protected virtual void _(Events.RowSelected<PMChangeOrderRevenueBudget> args)
        {
            if (args.Row != null)
            {
                ValidateChangeOrderRevenueBudget(args.Row);
            }
        }

        protected virtual void _(Events.RowSelected<PMChangeOrderCostBudget> args)
        {
            if (args.Row != null)
            {
                ValidateChangeOrderCostBudget(args.Row);
            }
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var changeOrder = Base.Document.Current;
            var complianceDocument = args.Row;
            if (changeOrder != null && complianceDocument != null)
            {
                FillChangeOrderInfo(complianceDocument, changeOrder);
            }
        }

        protected virtual void _(Events.RowSelecting<PMChangeOrder> args)
        {
            var documents = GetComplianceDocuments();
            complianceDocumentService.ValidateComplianceDocuments(args.Cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowDeleted<PMChangeOrder> args)
        {
            var order = args.Row;
            if (order == null)
            {
                return;
            }
            var documents = GetComplianceDocuments();
            documents.ForEach(RemoveComplianceReference);
        }

        private void RemoveComplianceReference(ComplianceDocument document)
        {
            document.ChangeOrderNumber = null;
            ComplianceDocuments.Update(document);
        }

        private void FillChangeOrderInfo(ComplianceDocument complianceDocument, PMChangeOrder changeOrder)
        {
            complianceDocument.ProjectID = changeOrder.ProjectID;
            complianceDocument.CustomerID = changeOrder.CustomerID;
            complianceDocument.CustomerName = GetCustomerName(changeOrder.CustomerID);
            complianceDocument.ChangeOrderNumber = changeOrder.RefNbr;
            FillAdditionalChangeOrderInfo(complianceDocument);
        }

        private void FillAdditionalChangeOrderInfo(ComplianceDocument complianceDocument)
        {
            var revenueBudgets = Base.RevenueBudget.Select().FirstTableItems.ToList();
            var commitments = Base.Details.Select().FirstTableItems.ToList();
            var costBudgets = Base.CostBudget.Select().FirstTableItems.ToList();
            complianceDocument.VendorID = GetVendorIfSingle(commitments);
            complianceDocument.VendorName = GetVendorName(complianceDocument.VendorID);
            complianceDocument.CostCodeID = GetCostCodeIfSingle(commitments, costBudgets);
            complianceDocument.CostTaskID = GetCostTaskIfSingle(commitments, costBudgets);
            complianceDocument.RevenueTaskID = GetRevenueTaskIfSingle(revenueBudgets);
        }

        private static int? GetCostTaskIfSingle(IReadOnlyCollection<PMChangeOrderLine> commitments,
            IReadOnlyCollection<PMChangeOrderCostBudget> costBudgets)
        {
            var firstCostTask = commitments.FirstOrDefault()?.TaskID ?? costBudgets.FirstOrDefault()?.ProjectTaskID;
            if (commitments.Any(c => c.TaskID != firstCostTask) || costBudgets.Any(c => c.TaskID != firstCostTask))
            {
                return null;
            }
            return firstCostTask;
        }

        private static int? GetCostCodeIfSingle(IReadOnlyCollection<PMChangeOrderLine> commitments,
            IReadOnlyCollection<PMChangeOrderCostBudget> costBudgets)
        {
            var firstCostCodeId = commitments.FirstOrDefault()?.CostCodeID ?? costBudgets.FirstOrDefault()?.CostCodeID;
            if (commitments.Any(c => c.CostCodeID != firstCostCodeId) ||
                costBudgets.Any(c => c.CostCodeID != firstCostCodeId))
            {
                return null;
            }
            return firstCostCodeId;
        }

        private static int? GetVendorIfSingle(IReadOnlyCollection<PMChangeOrderLine> commitments)
        {
            var firstVendorId = commitments.FirstOrDefault()?.VendorID;
            return commitments.Any(c => c.VendorID != firstVendorId)
                ? null
                : firstVendorId;
        }

        private static int? GetRevenueTaskIfSingle(IReadOnlyCollection<PMChangeOrderRevenueBudget> revenueBudgets)
        {
            var revenueTask = revenueBudgets.FirstOrDefault()?.ProjectTaskID;
            return revenueBudgets.Any(c => c.ProjectTaskID != revenueTask)
                ? null
                : revenueTask;
        }

        private void ValidateChangeOrderLine(PMChangeOrderLine changeOrderLine)
        {
            var costTaskHasExpiredCompliance = complianceDocumentService
                .ValidateRelatedField<PMChangeOrderLine, ComplianceDocument.costTaskID, PMChangeOrderLine.taskID>(
                    changeOrderLine, changeOrderLine.TaskID);
            var vendorHasExpiredCompliance = complianceDocumentService
                .ValidateRelatedField<PMChangeOrderLine, ComplianceDocument.vendorID, PMChangeOrderLine.vendorID>(
                    changeOrderLine, changeOrderLine.VendorID);
            complianceDocumentService.ValidateRelatedRow<
                PMChangeOrderLine, PmChangeOrderLineExt.hasExpiredComplianceDocuments>(
                changeOrderLine, costTaskHasExpiredCompliance || vendorHasExpiredCompliance);
        }

        private void ValidateChangeOrderRevenueBudget(PMChangeOrderRevenueBudget revenueBudgetLine)
        {
            var revenueTaskHasExpiredCompliance = complianceDocumentService
                .ValidateRelatedField<PMChangeOrderRevenueBudget, ComplianceDocument.revenueTaskID,
                    PMChangeOrderRevenueBudget.projectTaskID>(revenueBudgetLine, revenueBudgetLine.ProjectTaskID);
            complianceDocumentService.ValidateRelatedRow<PMChangeOrderRevenueBudget,
                PmChangeOrderRevenueBudgetExt.hasExpiredComplianceDocuments>(revenueBudgetLine,
                revenueTaskHasExpiredCompliance);
        }

        private void ValidateChangeOrderCostBudget(PMChangeOrderCostBudget costBudgetLine)
        {
            var costTaskHasExpiredCompliance =
                complianceDocumentService.ValidateRelatedField<PMChangeOrderCostBudget, ComplianceDocument.costTaskID,
                    PMChangeOrderCostBudget.projectTaskID>(costBudgetLine, costBudgetLine.ProjectTaskID);
            complianceDocumentService.ValidateRelatedRow<
                PMChangeOrderCostBudget, PmChangeOrderCostBudgetExt.hasExpiredComplianceDocuments>(
                costBudgetLine, costTaskHasExpiredCompliance);
        }

        private string GetVendorName(int? vendorId)
        {
            if (!vendorId.HasValue)
            {
                return null;
            }
            var vendor = new PXSelect<Vendor,
                Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(Base).SelectSingle(vendorId);
            return vendor?.AcctName;
        }

        private string GetCustomerName(int? customerId)
        {
            if (!customerId.HasValue)
            {
                return null;
            }
            var customer = new PXSelect<Customer,
                Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(Base).SelectSingle(customerId);
            return customer?.AcctName;
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Document.Current != null)
            {
                using (new PXConnectionScope())
                {
                    return new PXSelect<ComplianceDocument,
                            Where<ComplianceDocument.changeOrderNumber,
                                Equal<Required<PMChangeOrder.refNbr>>>>(Base)
                        .Select(Base.Document.Current.RefNbr).FirstTableItems.ToList();
                }
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }
    }
}