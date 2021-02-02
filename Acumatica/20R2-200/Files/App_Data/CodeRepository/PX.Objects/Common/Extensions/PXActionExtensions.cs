using System;
using System.Collections;
using System.Collections.Generic;
using PX.Common;
using PX.Data;

namespace PX.Objects.Common.Extensions
{
	public static class PXActionExtensions
	{
		/// <summary>
		/// Executes the action in mass process manner.
		/// </summary>
		public static IEnumerable Press<TTable>(this PXAction<TTable> action, IEnumerable<TTable> items)
			where TTable : class, IBqlTable, new()
		{
			return action.Press(new PXAdapter(PXView.Dummy.For(action.Graph, items)) { MassProcess = true });
		}
	}
}