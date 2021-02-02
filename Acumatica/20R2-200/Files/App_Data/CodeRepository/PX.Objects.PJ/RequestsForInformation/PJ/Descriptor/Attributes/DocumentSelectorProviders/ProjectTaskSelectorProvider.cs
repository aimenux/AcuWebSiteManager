using System;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ProjectTaskSelectorProvider : DocumentSelectorProvider
    {
        public ProjectTaskSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.ProjectTask;

        protected override Type SelectorType => typeof(PMTask);

        protected override Type SelectorQuery => typeof(Select<PMTask>);

        protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(PMTask.taskCD),
                typeof(PMTask.description),
                typeof(PMTask.customerID),
                typeof(PMTask.billingOption),
                typeof(PMTask.completedPctMethod),
                typeof(PMTask.status),
                typeof(PMTask.approverID)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var projectTaskEntry = PXGraph.CreateInstance<ProjectTaskEntry>();
            projectTaskEntry.Task.Current = GetProjectTask(noteId);
            throw new PXRedirectRequiredException(projectTaskEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private PMTask GetProjectTask(Guid? noteId)
        {
            var query = new PXSelect<PMTask,
                Where<PMTask.noteID, Equal<Required<PMTask.noteID>>>>(Graph);
            return query.SelectSingle(noteId);
        }
    }
}