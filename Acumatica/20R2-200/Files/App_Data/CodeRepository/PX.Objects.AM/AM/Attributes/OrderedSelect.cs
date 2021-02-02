using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Selector allowing a view to be ordered.
    /// </summary>
    public class OrderedSelect<Table, Where, OrderBy> : PXSelectBase<Table>
            where Table : class, IBqlTable, new()
            where Where : IBqlWhere, new()
            where OrderBy : IBqlOrderBy, new()
    {
        public OrderedSelect(PXGraph graph)
        {
            _Graph = graph;
            View = new PXView(graph, false, new Select<Table, Where, OrderBy>());
        }

        public OrderedSelect(PXGraph graph, Delegate handler)
        {
            _Graph = graph;
            View = new PXView(graph, false, new Select<Table, Where, OrderBy>(), handler);
        }
    }
}
