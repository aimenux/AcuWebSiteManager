using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType;
using PX.Objects.Common.Extensions;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public class DrawingLogRedirectionService<TGraph, TCache, TDrawingLogReference, TDrawingLogExtension> : IRedirectionService
        where TGraph : PXGraph, new()
        where TCache : class, IBqlTable, IProjectManagementDocumentBase, new()
        where TDrawingLogReference : DrawingLogReferenceBase, IBqlTable, new()
        where TDrawingLogExtension : DrawingLogBaseExtension<TGraph, TCache, TDrawingLogReference>, new()
    {
        private readonly PXGraph graph;

        private readonly TDrawingLogExtension drawingLogExtension;

        public DrawingLogRedirectionService()
        {
            graph = PXGraph.CreateInstance<TGraph>();
            drawingLogExtension = graph.GetExtension<TDrawingLogExtension>();
        }

        public void RedirectToEntity(List<DrawingLog> selectedDrawingLogs)
        {
            var entity = CreateEntity(selectedDrawingLogs);
            RemoveDefaultTaskPrefilling(entity.ProjectTaskId);
            graph.Caches[typeof(TCache)].Insert(entity);
            selectedDrawingLogs.ForEach(x => drawingLogExtension.InsertDrawingLogLink(x.DrawingLogId));
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
        }

        public void RedirectToEntity(DrawingLog drawingLog)
        {
            var entity = CreateEntity(drawingLog.ProjectId, drawingLog.ProjectTaskId);
            RemoveDefaultTaskPrefilling(entity.ProjectTaskId);
            graph.Caches[typeof(TCache)].Insert(entity);
            drawingLogExtension.InsertDrawingLogLink(drawingLog.DrawingLogId);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
        }

        private void RemoveDefaultTaskPrefilling(int? projectTaskId)
        {
            graph.Caches[typeof(TCache)].GetAttributes(nameof(projectTaskId))
                .OfType<ProjectTaskWithTypeAttribute>()
                .ForEach(attribute => attribute.NeedsPrefilling = false);
        }

        private static TCache CreateEntity(IReadOnlyCollection<DrawingLog> drawingLogs)
        {
            var distinctProjectTaskIds = drawingLogs.Select(x => x.ProjectTaskId).Distinct().ToList();
            var projectTaskId = distinctProjectTaskIds.HasAtLeastTwoItems()
                ? null
                : distinctProjectTaskIds.First();
            return CreateEntity(drawingLogs.First().ProjectId, projectTaskId);
        }

        private static TCache CreateEntity(int? projectId, int? projectTaskId)
        {
            return new TCache
            {
                ProjectId = projectId,
                ProjectTaskId = projectTaskId
            };
        }
    }
}