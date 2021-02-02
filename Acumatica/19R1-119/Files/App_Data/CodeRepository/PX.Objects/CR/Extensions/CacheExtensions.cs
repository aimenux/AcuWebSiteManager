using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CR
{
	internal static class CacheExtensions
	{

		public static object StrictDelete(this PXCache cache, object item)
		{
			object res = cache.Delete(item);
			PXEntryStatus itemStatus = cache.GetStatus(item);
			if (itemStatus != PXEntryStatus.Deleted && itemStatus != PXEntryStatus.InsertedDeleted)
				throw new Exception(string.Format("Cant delete item {0} from cache", item.GetType()));
			return res;
		}

		public static IEnumerable<T> TCached<T>(this PXCache<T> cache) where T : class,IBqlTable, new()
		{
			return cache.Cached.Cast<T>();
		}

		public static IEnumerable<T> TInserted<T>(this PXCache<T> cache) where T : class,IBqlTable, new()
		{
			return cache.Inserted.Cast<T>();
		}

		public static object StrictInsert(this PXCache cache, object item)
		{
			object res = cache.Insert(item);
			if (cache.GetStatus(res) != PXEntryStatus.Inserted || !cache.Inserted.Cast<object>().Contains(res))
				throw new Exception(string.Format("Cant insert item {0} in cache", item.GetType()));
			return res;
		}
	}
}
