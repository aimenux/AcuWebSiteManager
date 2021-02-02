using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.AP.CacheExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.CL.Services
{
    internal class ComplianceDocumentService
    {
        private readonly PXGraph graph;
        private readonly CommonAttributeColumnCreator columnCreator;
        private readonly PXSelectBase<ComplianceDocument> complianceDocuments;
        private readonly string complianceDocumentsViewName;

        public ComplianceDocumentService(PXGraph graph, PXSelectBase<CSAttributeGroup> attributeGroups,
            PXSelectBase<ComplianceDocument> complianceDocuments,
            string complianceDocumentsViewName)
        {
            this.graph = graph;
            this.complianceDocuments = complianceDocuments;
            this.complianceDocumentsViewName = complianceDocumentsViewName;
            columnCreator = new CommonAttributeColumnCreator(graph, attributeGroups);
        }

        public void GenerateColumns(PXCache cache, string documentAnswerView)
        {
            columnCreator.GenerateColumns(cache, complianceDocumentsViewName, documentAnswerView);
        }

        public void AddExpirationDateEventHandlers()
        {
            graph.FieldSelecting.AddHandler<ComplianceDocument.expirationDate>((cache, arguments) =>
                ValidateExpirationDateOnFieldSelecting(arguments.Row as ComplianceDocument,
                    complianceDocuments.Cache));
            graph.FieldVerifying.AddHandler<ComplianceDocument.expirationDate>((cache, arguments) =>
                ValidateExpirationDateOnFieldVerifying(arguments.Row as ComplianceDocument,
                    complianceDocuments.Cache, arguments.NewValue as DateTime?));
        }

        public void UpdateExpirationIndicator(ComplianceDocument document)
        {
            if (document != null)
            {
                document.IsExpired = document.ExpirationDate < graph.Accessinfo.BusinessDate;
            }
        }

        public void ValidateComplianceDocuments(PXCache eventCache, IEnumerable<ComplianceDocument> documents,
            PXCache documentsCache)
        {
            if (eventCache != null && eventCache.Updated.Any_())
            {
                return;
            }
            var expiredDocuments = documents.Where(d => d.ExpirationDate < graph.Accessinfo.BusinessDate);
            expiredDocuments.ForEach(d => RaiseComplianceDocumentIsExpiredException(
                documentsCache, d, d.ExpirationDate));
        }

        public IEnumerable<ComplianceDocument> GetComplianceDocuments<TField>(object value)
            where TField : IBqlField
        {
            return new PXSelect<ComplianceDocument, Where<TField, Equal<Required<TField>>>>(graph)
                .Select(value).FirstTableItems;
        }

        public void ValidateApAdjustment<TField>(APAdjust adjustment)
            where TField : IBqlField
        {
	        if (adjustment.IsSelfAdjustment())
		        return;

			var apInvoice = InvoiceDataProvider.GetInvoice(graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            var hasExpiredComplianceDocuments =
                ValidateRelatedField<APAdjust, ComplianceDocument.billID, TField>(adjustment,
                    ComplianceDocumentReferenceRetriever.GetComplianceDocumentReferenceId(graph, apInvoice));
            ValidateRelatedRow<APAdjust, ApAdjustExt.hasExpiredComplianceDocuments>(adjustment,
                hasExpiredComplianceDocuments);
        }

        public bool ValidateRelatedField<TEntity, TComplianceDocumentField, TEntityField>(
            TEntity entity, object fieldValue)
            where TEntity : class, IBqlTable, new()
            where TComplianceDocumentField : IBqlField
            where TEntityField : IBqlField
        {
            var cache = graph.Caches<TEntity>();
            var hasExpiredDocuments = DoExpiredDocumentsExist<TComplianceDocumentField>(fieldValue);
            RaiseOrClearExceptionForRelatedField<TEntityField>(cache, entity, hasExpiredDocuments,
                PXErrorLevel.Warning);
            return hasExpiredDocuments;
        }

        public bool ValidateRelatedProjectField<TEntity, TEntityField>(TEntity entity, object fieldValue)
            where TEntity : class, IBqlTable, new()
            where TEntityField : IBqlField
        {
            var cache = graph.Caches<TEntity>();
            var hasExpiredDocuments = DoExpiredDocumentsExist<ComplianceDocument.projectID>(fieldValue);
            var isDefaultProject = fieldValue as int? == ProjectDefaultAttribute.NonProject();
            var validationErrorNeeded = hasExpiredDocuments && !isDefaultProject;
            RaiseOrClearExceptionForRelatedField<TEntityField>(cache, entity, validationErrorNeeded,
                PXErrorLevel.Warning);
            return validationErrorNeeded;
        }

        public void ValidateRelatedRow<TEntity, THasExpiredDocumentsField>(TEntity entity, bool rowHasExpiredCompliance)
            where TEntity : class, IBqlTable, new()
            where THasExpiredDocumentsField : IBqlField
        {
            var cache = graph.Caches<TEntity>();
            cache.SetValue<THasExpiredDocumentsField>(entity, rowHasExpiredCompliance);
            RaiseOrClearExceptionForRelatedField<THasExpiredDocumentsField>(cache, entity, rowHasExpiredCompliance,
                PXErrorLevel.RowWarning);
        }

        private static void RaiseOrClearExceptionForRelatedField<TField>(
            PXCache cache, object entity, bool validationErrorNeeded, PXErrorLevel errorLevel)
            where TField : IBqlField
        {
            if (validationErrorNeeded)
            {
                RaiseCorrectExceptionForRelatedField<TField>(
                    cache, entity, ComplianceMessages.ExpiredComplianceMessage, errorLevel);
            }
            else
            {
                cache.ClearFieldErrorIfExists<TField>(entity, ComplianceMessages.ExpiredComplianceMessage);
            }
        }

        private bool DoExpiredDocumentsExist<TField>(object value)
            where TField : IBqlField
        {
            return GetComplianceDocuments<TField>(value)
                .Any(x => x.ExpirationDate < graph.Accessinfo.BusinessDate);
        }

        private static void RaiseCorrectExceptionForRelatedField<TField>(PXCache cache, object entity,
            string errorMessage, PXErrorLevel errorLevel)
            where TField : IBqlField
        {
            var previousErrorMessage = PXUIFieldAttribute.GetError<TField>(cache, entity);
            if (previousErrorMessage == null)
            {
                RaiseExceptionForRelatedField<TField>(cache, entity, errorMessage, errorLevel);
            }
        }

        private static void RaiseExceptionForRelatedField<TField>(PXCache cache, object entity, string errorMessage,
            PXErrorLevel errorLevel)
            where TField : IBqlField
        {
            var exception = new PXSetPropertyException<TField>(errorMessage, errorLevel);
            cache.RaiseExceptionHandling<TField>(entity, cache.GetValue<TField>(entity), exception);
        }

        private void ValidateExpirationDateOnFieldSelecting(ComplianceDocument document, PXCache documentsCache)
        {
            if (document != null && document.ExpirationDate < graph.Accessinfo.BusinessDate)
            {
                RaiseComplianceDocumentIsExpiredException(documentsCache, document, document.ExpirationDate);
            }
        }

        private void ValidateExpirationDateOnFieldVerifying(ComplianceDocument document, PXCache documentsCache,
            DateTime? expirationDate)
        {
            documentsCache.ClearItemAttributes();
            if (expirationDate != null && expirationDate < graph.Accessinfo.BusinessDate)
            {
                RaiseComplianceDocumentIsExpiredException(documentsCache, document, expirationDate);
            }
        }

        private static void RaiseComplianceDocumentIsExpiredException(PXCache cache, ComplianceDocument document,
            DateTime? expirationDate)
        {
            RaiseSingleIsExpiredException<ComplianceDocument.expirationDate>(
                cache, document, expirationDate, PXErrorLevel.Warning);
            RaiseSingleIsExpiredException<ComplianceDocument.isExpired>(
                cache, document, document.IsExpired, PXErrorLevel.RowWarning);
        }

        private static void RaiseSingleIsExpiredException<TField>(PXCache cache, ComplianceDocument document,
            object fieldValue, PXErrorLevel errorLevel)
            where TField : IBqlField
        {
            var exception = new PXSetPropertyException<TField>(
                ComplianceMessages.ComplianceDocumentIsExpiredMessage, errorLevel);
            cache.RaiseExceptionHandling<TField>(document, fieldValue, exception);
        }
    }
}