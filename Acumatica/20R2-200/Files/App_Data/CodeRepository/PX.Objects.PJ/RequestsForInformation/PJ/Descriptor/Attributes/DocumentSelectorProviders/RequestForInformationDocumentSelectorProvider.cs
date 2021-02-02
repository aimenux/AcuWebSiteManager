using System;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class RequestForInformationDocumentSelectorProvider : DocumentSelectorProvider
    {
        public RequestForInformationDocumentSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.RequestForInformation;

        protected override Type SelectorType => typeof(RequestForInformation);

        protected override Type SelectorQuery => typeof(Select<RequestForInformation>);

        protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(RequestForInformation.requestForInformationCd),
                typeof(RequestForInformation.status),
                typeof(RequestForInformation.projectId),
                typeof(RequestForInformation.projectTaskId),
                typeof(RequestForInformation.summary),
                typeof(RequestForInformation.dueResponseDate),
                typeof(RequestForInformation.incoming),
                typeof(RequestForInformation.isScheduleImpact),
                typeof(RequestForInformation.isCostImpact)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var requestForInformationMaint = PXGraph.CreateInstance<RequestForInformationMaint>();
            requestForInformationMaint.RequestForInformation.Current = GetRequestForInformation(noteId);
            throw new PXRedirectRequiredException(requestForInformationMaint, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private RequestForInformation GetRequestForInformation(Guid? noteId)
        {
            var query = new PXSelect<RequestForInformation,
                Where<RequestForInformation.noteID, Equal<Required<RequestForInformation.noteID>>>>(Graph);
            return query.SelectSingle(noteId);
        }
    }
}