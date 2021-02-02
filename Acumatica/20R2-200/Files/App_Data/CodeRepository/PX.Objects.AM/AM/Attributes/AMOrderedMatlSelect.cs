using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    [PXDynamicButton(new string[] { PasteLineCommand, ResetOrderCommand },
        new string[] { AM.Messages.PasteLine, AM.Messages.ResetLines },
        TranslationKeyType = typeof(AM.Messages))]
    public class AMOrderedMatlSelect<Primary, Table, Where, OrderBy> : PXOrderedSelectBase<Primary, Table>
        where Primary : class, IBqlTable, new()
        where Table : class, IBqlTable, ISortOrder, new()
        where Where : IBqlWhere, new()
        where OrderBy : IBqlOrderBy, new()
    {
        public AMOrderedMatlSelect(PXGraph graph)
        {
            _Graph = graph;
            Initialize();

            View = new PXView(graph, false, new Select<Table, Where, OrderBy>());
        }
        public AMOrderedMatlSelect(PXGraph graph, Delegate handler)
        {
            _Graph = graph;
            Initialize();

            View = new PXView(graph, false, new Select<Table, Where, OrderBy>(), handler);
        }

        public override void Initialize()
        {
            base.Initialize();
            // Replacing "Reset Order" to "Reset Lines" ...
            _Graph.Actions[ResetOrderCommand].SetCaption(Messages.GetLocal(Messages.ResetLines));
            RenumberTailOnDelete = false;
        }

        public override void RenumberAll()
        {
            foreach (Table line in Select())
            {
                Cache.SetValue(line, nameof(ISortOrder.SortOrder), line.LineNbr);
                Cache.MarkUpdated(line);
                Cache.IsDirty = true;
            }
        }

        // Added to prevent renumbering when saving and losing the Sorted order
        protected override void OnBeforeGraphPersist(PXGraph graph)
        {
            if (!IsPrimaryEntityNewlyInserted)
            {
                RenumberTail();
            }
        }
    }
}