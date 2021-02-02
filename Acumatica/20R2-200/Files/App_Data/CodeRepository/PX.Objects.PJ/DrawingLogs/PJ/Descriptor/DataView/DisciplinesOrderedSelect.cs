using System;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.DataView
{
    [PXDynamicButton(
        new[]
        {
            DrawingLogLabels.DisciplinesOrderedSelect.PasteLine,
            DrawingLogLabels.DisciplinesOrderedSelect.ResetOrder
        },
        new[]
        {
            DrawingLogLabels.DisciplinesOrderedSelect.PasteLine,
            DrawingLogLabels.DisciplinesOrderedSelect.ResetOrder
        })]
    public sealed class DisciplinesOrderedSelect<TPrimary, TEntity, TOrderBy> : PXOrderedSelectBase<TPrimary, TEntity>
        where TPrimary : class, IBqlTable, new()
        where TEntity : class, IBqlTable, ISortOrder, new()
        where TOrderBy : IBqlOrderBy, new()
    {
        public DisciplinesOrderedSelect(PXGraph graph)
        {
            _Graph = graph;
            Initialize();
            View = new PXView(graph, false, new Select3<TEntity, TOrderBy>());
        }

        public DisciplinesOrderedSelect(PXGraph graph, Delegate handler)
        {
            _Graph = graph;
            Initialize();
            View = new PXView(graph, false, new Select3<TEntity, TOrderBy>(), handler);
        }
    }
}