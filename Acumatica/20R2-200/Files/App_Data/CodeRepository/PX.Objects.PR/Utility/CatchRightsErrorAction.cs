using PX.Common;
using PX.Data;
using System;
using System.Collections;

namespace PX.Objects.PR
{
	public class CatchRightsErrorAction<TNode> : PXAction<TNode>
			where TNode : class, IBqlTable, new()
	{
		public CatchRightsErrorAction(PXGraph graph, string name) : base(graph, name) { }
		public CatchRightsErrorAction(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public override IEnumerable Press(PXAdapter adapter)
		{
			try
			{
				return base.Press(adapter).ToArray<object>();
			}
			catch (PXActionDisabledException)
			{
				return adapter.Get();
			}
		}
	}
}
