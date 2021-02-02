using System.Linq;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders;
using PX.Data;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationRelationDocumentSelectorAttribute : PXEventSubscriberAttribute,
        IPXFieldSelectingSubscriber
    {
        private DocumentSelectorProvider[] selectorProviders;

        public static void NavigateToDocument(PXGraph graph, RequestForInformationRelation relation)
        {
            if (relation.Type != null && relation.DocumentNoteId != null)
            {
                var documentSelectorProvider = GetDocumentSelectorProvider(graph, relation);
                documentSelectorProvider?.NavigateToDocument(relation.DocumentNoteId);
            }
        }

        public void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation)
            {
                var selectorProvider = GetDocumentSelectorProvider(requestForInformationRelation);
                if (selectorProvider != null)
                {
                    args.ReturnState = GetFieldState(selectorProvider, requestForInformationRelation, args.ReturnState);
                    return;
                }
            }
            args.ReturnState = GetEmptyFieldState(cache.Graph, args.ReturnState);
        }

        public override void CacheAttached(PXCache cache)
        {
            var graph = cache.Graph;
            selectorProviders = new DocumentSelectorProvider[]
            {
                new ProjectSelectorProvider(graph, _FieldName),
                new ProjectTaskSelectorProvider(graph, _FieldName),
                new PurchaseOrderSelectorProvider(graph, _FieldName),
                new SubcontractDocumentSelectorProvider(graph, _FieldName),
                new ApInvoiceDocumentSelectorProvider(graph, FieldName),
                new ArInvoiceDocumentSelectorProvider(graph, _FieldName),
                new RequestForInformationDocumentSelectorProvider(graph, _FieldName)
            };
        }

        private static PXFieldState GetFieldState(DocumentSelectorProvider selectorProvider,
            RequestForInformationRelation requestForInformationRelation, object returnState)
        {
            selectorProvider.AddDescriptionFieldIfRequired();
            selectorProvider.CreateViewIfRequired();
            return selectorProvider.GetFieldState(requestForInformationRelation, returnState);
        }

        private DocumentSelectorProvider GetDocumentSelectorProvider(RequestForInformationRelation relation)
        {
            return selectorProviders.SingleOrDefault(x => x.DocumentType == relation.Type);
        }

        private PXFieldState GetEmptyFieldState(PXGraph graph, object returnState)
        {
            return PXFieldState.CreateInstance(returnState, null, null, null, null,
                null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, false, null,
                null, PXUIVisibility.Undefined, graph.PrimaryView);
        }

        private static DocumentSelectorProvider GetDocumentSelectorProvider(PXGraph graph,
            RequestForInformationRelation requestForInformationRelation)
        {
            return graph.Caches[typeof(RequestForInformationRelation)]
                .GetAttributes<RequestForInformationRelation.documentNoteId>(requestForInformationRelation)
                .OfType<RequestForInformationRelationDocumentSelectorAttribute>().SingleOrDefault()?.selectorProviders
                .SingleOrDefault(x => x.DocumentType == requestForInformationRelation.Type);
        }
    }
}