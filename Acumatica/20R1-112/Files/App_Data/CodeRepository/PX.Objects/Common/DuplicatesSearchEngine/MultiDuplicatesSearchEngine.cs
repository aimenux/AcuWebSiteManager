using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class MultiDuplicatesSearchEngine<TEntity> : DuplicatesSearchEngineBase<TEntity>
		where TEntity : class, IBqlTable, new()
	{
		protected readonly Dictionary<TEntity, List<TEntity>> _items;

		public MultiDuplicatesSearchEngine(PXCache cache, IEnumerable<Type> keyFields, ICollection<TEntity> items)
			: base(cache, keyFields)
		{
			_items = new Dictionary<TEntity, List<TEntity>>(_comparator);

			foreach (var item in items)
				AddItem(item);
		}

		public override void AddItem(TEntity item)
		{
			if (_items.TryGetValue(item, out List<TEntity> list))
			{
				list.Add(item);
			}
			else
			{
				_items.Add(item, new List<TEntity>() { item });
			}
		}

		public override TEntity Find(TEntity item)
		{
			List<TEntity> found;
			return _items.TryGetValue(item, out found) && found != null && found.Any()
				? found.First()
				: null;
		}

		public virtual bool RemoveItem(TEntity item)
		{
			if (_items.TryGetValue(item, out List<TEntity> list))
			{
				if (list.Remove(item))
				{
					if (list.Count == 0)
						_items.Remove(item);

					return true;
				}
			}

			return false;
		}
	}
}
