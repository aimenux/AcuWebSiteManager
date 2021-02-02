using PX.Common;
using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	/// <summary>
	/// Compares two data records based on the equality of their keys.
	/// The collection of keys is defined by the specified
	/// <see cref="PXCache"/> object.
	/// </summary>
	public class RecordKeyComparer<TRecord> : IEqualityComparer<TRecord>
		where TRecord : class, IBqlTable, new()
	{
		private PXCache _cache;

		public RecordKeyComparer(PXCache cache)
		{
			if (cache == null) throw new ArgumentNullException(nameof(cache));

			_cache = cache;
		}

		public bool Equals(TRecord first, TRecord second)
			=> _cache.ObjectsEqual(first, second);

		public int GetHashCode(TRecord record)
			=> _cache.GetObjectHashCode(record);
	}

	[PXInternalUseOnly]
	public class KeyValuesComparer<TEntity> : IEqualityComparer<TEntity>
			where TEntity : class, IBqlTable
	{
		private readonly int[] _keyOrdinals;
		private readonly PXCache _cache;

		public KeyValuesComparer(PXCache cache, IEnumerable<Type> keyFields)
		{
			_cache = cache;
			_keyOrdinals = keyFields.Select(f => _cache.GetFieldOrdinal(f.Name)).ToArray();
		}

		public bool Equals(TEntity x, TEntity y)
		{
			if (ReferenceEquals(x, y))
				return true;
			foreach(var fieldOrdinal in _keyOrdinals)
			{
				if (!Equals(_cache.GetValue(x, fieldOrdinal), _cache.GetValue(y, fieldOrdinal)))
					return false;
			}
			return true;
		}

		public int GetHashCode(TEntity entity)
		{
			if (entity == null)
				return 0;
			unchecked
			{
				int ret = 13;
				foreach (var fieldOrdinal in _keyOrdinals)
				{
					ret = ret * 37 + (_cache.GetValue(entity, fieldOrdinal) ?? 0).GetHashCode();
				}
				return ret;
			}
		}
	}
}
