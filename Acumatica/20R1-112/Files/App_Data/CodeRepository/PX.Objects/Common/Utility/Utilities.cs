using System;
using PX.Data;

namespace PX.Objects.Common
{
	public static class Utilities
	{
		public static void Swap<T>(ref T first, ref T second)
		{
			T temp = first;
			first = second;
			second = temp;
		}

		[Obsolete("This method is obsolete and will be removed in 2020R1. Use PX.Objects.GL.BatchModule.GetDisplayName instead.")]
		public static string GetSubledgerTitle(this PXGraph graph, string subledgerPrefix)
		{
			return PX.Objects.GL.BatchModule.GetDisplayName(subledgerPrefix);
		}

		[Obsolete("This method is obsolete and will be removed in 2020R1. Use PX.Objects.GL.BatchModule.GetDisplayName instead.")]
		public static string GetSubledgerTitle<TSubledgerConst>(this PXGraph graph)
			where TSubledgerConst : IConstant<string>, IBqlOperand, new()
		{
			return PX.Objects.GL.BatchModule.GetDisplayName<TSubledgerConst>();
		}

		public static TDestinationDAC Clone<TSourceDAC, TDestinationDAC>(PXGraph graph, TSourceDAC source)
			where TSourceDAC : class, IBqlTable, new()
			where TDestinationDAC : class, IBqlTable, new()
		{
			PXCache sourceCache = graph.Caches<TSourceDAC>();
			PXCache destinationCache = graph.Caches<TDestinationDAC>();
			TDestinationDAC result = (TDestinationDAC)destinationCache.CreateInstance();
			foreach (string field in destinationCache.Fields)
			{
				if (sourceCache.Fields.Contains(field))
				{
					destinationCache.SetValue(result, field, sourceCache.GetValue(source, field));
				}
			}

			return result;
		}

		public static PXResultset<TSourceDAC> ToResultset<TSourceDAC>(TSourceDAC item)
			where TSourceDAC : class, IBqlTable, new ()
		{
			return new PXResultset<TSourceDAC>()
			{
				new PXResult<TSourceDAC>(item)
			};
		}
	}
}
