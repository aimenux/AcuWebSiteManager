using System;
using System.Linq;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public abstract class DocumentSelectorProvider
    {
        protected readonly PXGraph Graph;
        protected readonly PXCache Cache;

        private const string DescriptionFieldName = "_DocumentDescription";

        private readonly EntityHelper entityHelper;
        private readonly string fieldName;
        private readonly string[] selectorFields;
        private readonly string[] selectorHeaders;
        private readonly string viewName;

        protected DocumentSelectorProvider(PXGraph graph, string fieldName)
        {
            Graph = graph;
            this.fieldName = fieldName;
            entityHelper = new EntityHelper(graph);
            Cache = graph.Caches[SelectorType];
            selectorFields = GetSelectorFields();
            selectorHeaders = GetSelectorHeaderNames();
            viewName = GetViewName();
        }

        public abstract string DocumentType
        {
            get;
        }

        protected abstract Type SelectorType
        {
            get;
        }

        protected abstract Type SelectorQuery
        {
            get;
        }

        protected abstract Type[] SelectorFieldTypes
        {
            get;
        }

        public abstract void NavigateToDocument(Guid? noteId);

        public void CreateViewIfRequired()
        {
            if (!Graph.Views.ContainsKey(viewName))
            {
                Graph.Views.Add(viewName, CreateView());
            }
        }

        public PXFieldState GetFieldState(RequestForInformationRelation requestForInformationRelation,
            object returnState)
        {
            var fieldState = CreateFieldState(returnState);
            fieldState.ValueField = EntityHelper.GetNoteField(SelectorType);
            fieldState.DescriptionName = DescriptionFieldName;
            fieldState.SelectorMode = PXSelectorMode.TextModeSearch;
            fieldState.Value = GetDocumentDescription(requestForInformationRelation.DocumentNoteId);
            return fieldState;
        }

        public void AddDescriptionFieldIfRequired()
        {
            if (!Cache.Fields.Contains(DescriptionFieldName))
            {
                Cache.Fields.Add(DescriptionFieldName);
                Graph.FieldSelecting.AddHandler(SelectorType, DescriptionFieldName,
                    SelectorEntity_Description_FieldSelecting);
            }
        }

        protected virtual string[] GetSelectorHeaderNames()
        {
            return selectorFields.Select(x => PXUIFieldAttribute.GetDisplayName(Cache, x)).ToArray();
        }

        protected virtual string GetDocumentDescription(Guid? noteId)
        {
            return entityHelper.GetEntityDescription(noteId, SelectorType);
        }

        private void SelectorEntity_Description_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            var description = GetDocumentDescription(entityHelper.GetEntityNoteID(args.Row));
            args.ReturnState = PXFieldState.CreateInstance(description, null, null, null,
                null, null, null, null, DescriptionFieldName, null, null, null, PXErrorLevel.Undefined,
                false, false, null, PXUIVisibility.Invisible, viewName);
        }

        private PXFieldState CreateFieldState(object returnState)
        {
            return PXFieldState.CreateInstance(returnState, null, null, null,
                null, null, null, null, fieldName, null, null, null, PXErrorLevel.Undefined, null, true, null,
                PXUIVisibility.Undefined, viewName, selectorFields, selectorHeaders);
        }

        private PXView CreateView()
        {
            var bqlCommand = BqlCommand.CreateInstance(SelectorQuery);
            return new PXView(Graph, true, bqlCommand);
        }

        private string GetViewName()
        {
            return GetType().FullName;
        }

        private string[] GetSelectorFields()
        {
            return SelectorFieldTypes.Select(x => Cache.GetField(x)).ToArray();
        }
    }
}