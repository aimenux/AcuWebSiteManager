using CommonServiceLocator;
using PX.Common;
using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.CN.Common.Extensions
{
    public static class GraphExtensions
    {
        public static void RedirectToEntity(this PXGraph graph, object row, PXRedirectHelper.WindowMode windowMode)
        {
            if (row != null)
            {
                PXRedirectHelper.TryRedirect(graph, row, windowMode);
            }
        }

        public static TService GetService<TService>(this PXGraph graph)
        {
			return ((Func<PXGraph, TService>)ServiceLocator.Current
                .GetService(typeof(Func<PXGraph, TService>)))(graph);
        }

		public static bool HasErrors(this PXGraph graph)
		{
			var attributes = graph.Views.Caches
				.Where(key => graph.Caches.ContainsKey(key))
				.SelectMany(key => graph.Caches[key].GetAttributes(null));
			return attributes.Any(a =>
				a is IPXInterfaceField field && field.ErrorLevel.IsIn(PXErrorLevel.Error, PXErrorLevel.RowError));
		}
    }
}