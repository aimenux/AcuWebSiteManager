using System;
using PX.Data;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ProjectSelectorProvider : DocumentSelectorProvider
    {
        public ProjectSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.Project;

        protected override Type SelectorType => typeof(PMProject);

        protected override Type SelectorQuery =>
            typeof(Select<PMProject,
                Where<PMProject.nonProject, Equal<False>,
                    And<PMProject.baseType, Equal<CTPRType.project>>>>);

        protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(PMProject.contractCD),
                typeof(PMProject.customerID),
                typeof(PMProject.description),
                typeof(PMProject.status)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var projectEntry = PXGraph.CreateInstance<ProjectEntry>();
            projectEntry.Project.Current = GetProject(noteId);
            throw new PXRedirectRequiredException(projectEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private PMProject GetProject(Guid? noteId)
        {
            var query = new PXSelect<PMProject,
                Where<PMProject.noteID, Equal<Required<PMProject.noteID>>>>(Graph);
            return query.SelectSingle(noteId);
        }
    }
}