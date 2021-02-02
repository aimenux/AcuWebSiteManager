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

		public static string GetSubledgerTitle(this PXGraph graph, string subledgerPrefix)
		{
			using (new PXLoginScope(PXAccess.GetFullUserName(), PXAccess.GetAdministratorRoles()))
			{
				return ((PXDatabaseSiteMapProvider)PXSiteMap.Provider).FindSiteMapNodeByScreenID($"{subledgerPrefix ?? string.Empty}000000")?.Title;
			}
		}

		public static string GetSubledgerTitle<TSubledgerConst>(this PXGraph graph)
			where TSubledgerConst : IConstant<string>, IBqlOperand, new()
		{
			return graph.GetSubledgerTitle(new TSubledgerConst().Value.ToString());
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
