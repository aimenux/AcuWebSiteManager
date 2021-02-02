using Microsoft.Practices.ServiceLocation;
using System;
using PX.Data;

namespace PX.Objects.Common.Extensions
{
	public static class PXGraphClassExtensions
	{
		public static void DisableCaches(this PXGraph graph)
		{
			foreach (PXCache graphCache in graph.Caches.Values)
			{
				graphCache.AllowInsert =
				graphCache.AllowUpdate =
				graphCache.AllowDelete = false;
			}
		}

		internal static TService GetService<TService>(this PXGraph graph)
		{
			Func<PXGraph, TService> factoryFunc = (Func<PXGraph, TService>)ServiceLocator.Current.GetService(typeof(Func<PXGraph, TService>));
			return factoryFunc(graph);
		}
	}
}
