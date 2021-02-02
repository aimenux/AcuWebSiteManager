using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class DuplicatesSearchEngine<TEntity> : DuplicatesSearchEngineBase<TEntity>
		where TEntity : class, IBqlTable, new()
	{
		protected readonly Dictionary<TEntity, TEntity> _items;

		public DuplicatesSearchEngine(PXCache cache, IEnumerable<Type> keyFields, ICollection<TEntity> items)
			: base(cache, keyFields)
		{
			_items = new Dictionary<TEntity, TEntity>(_comparator);

			foreach (var item in items)
				AddItem(item);
		}

		public override TEntity Find(TEntity item)
		{
			TEntity found;
			return _items.TryGetValue(item, out found)
				? found
				: null;
		}

		public override void AddItem(TEntity item)
		{
			if (!_items.ContainsKey(item))
				_items.Add(item, item);
		}
	}
}
