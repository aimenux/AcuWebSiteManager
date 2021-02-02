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

	internal class KeyValuesComparer<TEntity> : IEqualityComparer<TEntity>
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

	internal class DuplicatesSearchEngine<TEntity> where TEntity : class, IBqlTable, new()
	{
		private readonly PXCache _cache;
		private readonly Type[] _keyFields;
		private readonly KeyValuesComparer<TEntity> _comparator;
		private readonly Dictionary<TEntity, TEntity> _items;
		private readonly TEntity _template;

		public DuplicatesSearchEngine(PXCache cache, IEnumerable<Type> keyFields, ICollection<TEntity> items)
		{
			_cache = cache;
			_keyFields = keyFields.ToArray();
			_comparator = new KeyValuesComparer<TEntity>(cache, keyFields);
			_items = new Dictionary<TEntity, TEntity>(_comparator);
			foreach (var item in items)
				AddItem(item);
			_template = new TEntity();
		}

		public TEntity Find(TEntity item)
		{
			TEntity found;
			return _items.TryGetValue(item, out found) 
				? found
				: null;
		}

		public TEntity Find(IDictionary itemValues)
		{
			foreach(var field in _keyFields)
			{
				var value = itemValues[_cache.GetField(field)];
				if (value != null && !_cache.RaiseFieldUpdating(field.Name, null, ref value))
					value = null;
				_cache.SetValue(_template, field.Name, value);
			}
			return Find(_template);
		}

		public void AddItem(TEntity item)
		{
			if (!_items.ContainsKey(item))
				_items.Add(item, item);
		}
	}
}
