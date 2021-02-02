using System.Linq;

using PX.Data;

namespace PX.Objects.Common.Extensions
{
	public static class DataRecordExtensions
	{
		public static bool AreAllKeysFilled<TNode>(this TNode record, PXGraph graph)
			where TNode : class, IBqlTable, new()
			=> AreAllKeysFilled(record, graph.Caches[typeof(TNode)]);

		public static bool AreAllKeysFilled<TNode>(this TNode record, PXCache cache)
			where TNode : class, IBqlTable, new()
			=> cache.Keys.All(fieldName => cache.GetValue(record, fieldName) != null);
	}
}
