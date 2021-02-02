using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.Common.Extensions;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.Services
{
    public class ChangeOrderValidationService
    {
        private readonly ChangeOrderEntry graph;

        private readonly PXSelectBase<ComplianceDocument> complianceDocumentView;

        public ChangeOrderValidationService(ChangeOrderEntry graph,
            PXSelectBase<ComplianceDocument> complianceDocumentView)
        {
            this.graph = graph;
            this.complianceDocumentView = complianceDocumentView;
        }

        public void ValidateComplianceFields(ComplianceDocument complianceDocument)
        {
            ValidateFieldOnLinkedToChangeOrder<ComplianceDocument.costTaskID>(complianceDocument, IsTaskLinkedToChangeOrder,
                complianceDocument.CostTaskID, ComplianceMessages.CostTaskIsNotLinkedToChangeOrder);
            ValidateFieldOnLinkedToChangeOrder<ComplianceDocument.revenueTaskID>(complianceDocument,
                IsTaskLinkedToChangeOrder, complianceDocument.RevenueTaskID,
                ComplianceMessages.RevenueTaskIsNotLinkedToChangeOrder);
            ValidateFieldOnLinkedToChangeOrder<ComplianceDocument.costCodeID>(complianceDocument,
                IsCostCodeLinkedToChangeOrder, complianceDocument.CostCodeID,
                ComplianceMessages.CostCodeIsNotLinkedToChangeOrder);
        }

        private void ValidateFieldOnLinkedToChangeOrder<TField>(ComplianceDocument complianceDocument,
            Func<int?, bool> isFieldLinkedToChangeOrder, int? fieldId, string warningMessage)
            where TField : IBqlField
        {
            if (isFieldLinkedToChangeOrder(fieldId))
            {
                RaiseExceptionForComplianceFields<TField>(complianceDocumentView.Cache,
                    complianceDocument, fieldId, warningMessage);
            }
            else
            {
                RemoveErrorWarning<TField>(complianceDocumentView.Cache,
                    complianceDocument, warningMessage);
            }
        }

        private bool IsTaskLinkedToChangeOrder(int? taskId)
        {
            var budgetLines = GetChangeOrderBudgetLines();
            var orderLines = GetChangeOrderLines();
            return taskId != null && budgetLines.All(s => s.TaskID != taskId) &&
                   orderLines.All(s => s.TaskID != taskId);
        }

        private bool IsCostCodeLinkedToChangeOrder(int? costCodeId)
        {
            var budgetLines = GetChangeOrderBudgetLines();
            var orderLines = GetChangeOrderLines();
            return costCodeId != null && budgetLines.All(s => s.CostCodeID != costCodeId) &&
                   orderLines.All(s => s.CostCodeID != costCodeId);
        }

        private IEnumerable<PMChangeOrderBudget> GetChangeOrderBudgetLines()
        {
            return new PXSelect<PMChangeOrderBudget,
                Where<PMChangeOrderBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>>>(graph).Select().FirstTableItems;
        }

        private IEnumerable<PMChangeOrderLine> GetChangeOrderLines()
        {
            return new PXSelect<PMChangeOrderLine,
                Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>>>(graph).Select().FirstTableItems;
        }

        private static void RaiseExceptionForComplianceFields<TField>(PXCache cache, ComplianceDocument document,
            object fieldValue, string warningMessage)
            where TField : IBqlField
        {
            var exception = new PXSetPropertyException<TField>(warningMessage, PXErrorLevel.Warning);
            cache.RaiseExceptionHandling<TField>(document, fieldValue, exception);
        }

        private static void RemoveErrorWarning<TField>(PXCache cache, object entity, string errorMessage)
            where TField : IBqlField
        {
            var fieldName = cache.GetField(typeof(TField));
            var hasError = cache.GetAttributes(entity, fieldName).OfType<IPXInterfaceField>()
                .Any(x => x.ErrorText == errorMessage);
            if (hasError)
            {
                cache.ClearFieldErrors<TField>(entity);
            }
        }
    }
}
