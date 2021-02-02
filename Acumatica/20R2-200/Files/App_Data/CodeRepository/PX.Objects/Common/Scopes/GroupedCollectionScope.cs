using PX.Common;
using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common.Scopes
{
	public abstract class CacheOperationsScope<TEntity> : IDisposable
		where TEntity : class, IBqlTable, new()
	{
		protected PXGraph Graph { get; }

		protected CacheOperationsScope(PXGraph graph)
		{
			Graph = graph;
		}

		public virtual void SubscribeCacheEvents()
		{
			Graph.RowInserted.AddHandler<TEntity>(RowInserted);
			Graph.RowUpdated.AddHandler<TEntity>(RowUpdated);
			Graph.RowDeleted.AddHandler<TEntity>(RowDeleted);
		}

		public virtual void UnSubscribeCacheEvents()
		{
			Graph.RowInserted.RemoveHandler<TEntity>(RowInserted);
			Graph.RowUpdated.RemoveHandler<TEntity>(RowUpdated);
			Graph.RowDeleted.RemoveHandler<TEntity>(RowDeleted);
		}

		protected abstract void RowInserted(PXCache cache, PXRowInsertedEventArgs e);

		protected abstract void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e);

		protected abstract void RowDeleted(PXCache cache, PXRowDeletedEventArgs e);

		public virtual void Dispose()
		{
			UnSubscribeCacheEvents();
		}
	}

	public class GroupedCollectionScope<TEntity> : CacheOperationsScope<TEntity>
		where TEntity : class, IBqlTable, new()
	{
		private readonly IGroupedCollection<TEntity> _collection;

		public GroupedCollectionScope(IGroupedCollection<TEntity> collection)
			: base(collection.Cache.Graph)
		{
			_collection = collection;
		}

		protected override void RowInserted(PXCache cache, PXRowInsertedEventArgs e) => _collection.Insert((TEntity)e.Row);

		protected override void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e) => _collection.Update((TEntity)e.OldRow, (TEntity)e.Row);

		protected override void RowDeleted(PXCache cache, PXRowDeletedEventArgs e) => _collection.Delete((TEntity)e.Row);
	}

	public interface IGroupedCollection<TEntity> : ICollection<TEntity>
		where TEntity : class, IBqlTable, new()
	{
		PXCache Cache { get; }
		void Insert(TEntity data);
		void InsertRange(TEntity group, IEnumerable<TEntity> data);
		void Update(TEntity oldData, TEntity newData);
		void Delete(TEntity data);
		IEnumerable<TEntity> GetItems(TEntity group);
		IEnumerable<TEntity> LoadItems(TEntity group);

		IGroupedCollection<TEntity> Clone();
	}

	public delegate IEnumerable<TEntity> GroupItemsLoadHandler<TEntity>(TEntity group) where TEntity : class, IBqlTable, new();

	[PXInternalUseOnly]
	public static class GroupedCollectionUtils
	{
		public static GroupedCollection<TEntity, TCollection> SplitBy<TEntity, TCollection>(
			this TCollection collection,
			PXCache<TEntity> cache,
			Type[] byFields)
			where TEntity : class, IBqlTable, new()
			where TCollection : ICollection<TEntity>, new()
		{
			var groupComparer = new KeyValuesComparer<TEntity>(cache, byFields);

			var uniqueKeyComparer = new KeyValuesComparer<TEntity>(cache, cache.BqlKeys);

			var grouped = new GroupedCollection<TEntity, TCollection>(cache, groupComparer);
			grouped.UniqueKeyComparer = uniqueKeyComparer;

			return grouped;
		}

		public static GroupedCollection<TEntity, GroupedCollection<TEntity, TCollection>> SplitBy<TEntity, TCollection>(
			this GroupedCollection<TEntity, TCollection> collection,
			Type[] byFields,
			GroupItemsLoadHandler<TEntity> itemLoader)
			where TEntity : class, IBqlTable, new()
			where TCollection : ICollection<TEntity>, new()
		{
			var groupComparer = new KeyValuesComparer<TEntity>(collection.Cache, byFields);
			return new UpperGroupedCollection<TEntity, GroupedCollection<TEntity, TCollection>>(groupComparer, itemLoader, collection);
		}
	}

	[PXInternalUseOnly]
	public class GroupedCollection<TEntity, TCollection> : IGroupedCollection<TEntity>
		where TEntity : class, IBqlTable, new()
		where TCollection : ICollection<TEntity>, new()
	{
		protected Dictionary<TEntity, TCollection> Groups;

		protected IEqualityComparer<TEntity> Comparer;

		public IEqualityComparer<TEntity> UniqueKeyComparer { get; set; }

		public PXCache Cache { get; private set; }

		protected GroupItemsLoadHandler<TEntity> ItemsLoader;

		public int Count => Groups?.Count ?? 0;

		public bool IsReadOnly => false;

		public GroupedCollection() { }

		public GroupedCollection(PXCache cache, IEqualityComparer<TEntity> comparer)
		{
			Cache = cache;
			Comparer = comparer;
		}

		protected GroupedCollection(PXCache cache, IEqualityComparer<TEntity> comparer, GroupItemsLoadHandler<TEntity> itemsLoader)
			: this(cache, comparer)
		{
			ItemsLoader = itemsLoader;
		}

		public void Clear() => Groups?.Clear();

		public virtual void Insert(TEntity data)
		{
			PrepareGroup();

			var collection = PrepareCollection(data);
			collection.Add(data);

			if (!Groups.ContainsKey(data))
				Groups.Add(data, collection);
		}

		protected void PrepareGroup()
		{
			Groups = Groups ?? new Dictionary<TEntity, TCollection>(Comparer);
		}

		public virtual void InsertRange(TEntity rangeKey, IEnumerable<TEntity> range)
		{
			PrepareGroup();

			foreach (var group in range.ToLookup(x => x, Comparer))
			{
				TCollection collection;
				if (Groups.TryGetValue(group.Key, out collection))
					throw new KeyNotFoundException();
				collection = PrepareCollection(group.Key);
				foreach (var item in group)
					collection.Add(item);
				Groups.Add(group.Key, collection);
			}
		}

		public virtual void Update(TEntity oldData, TEntity newData)
		{
			ThrowIfNoCollection(oldData);

			var collection = PrepareCollection(oldData);

			if (UniqueKeyComparer != null)
				oldData = collection.FirstOrDefault(x => UniqueKeyComparer.Equals(x, oldData));

			if (oldData != null)
				collection.Remove(oldData);

			collection.Add(newData);
		}

		public virtual void Delete(TEntity data)
		{
			ThrowIfNoCollection(data);

			var collection = PrepareCollection(data);

			if (UniqueKeyComparer != null)
				data = collection.FirstOrDefault(x => UniqueKeyComparer.Equals(x, data));

			if (data != null)
				collection.Remove(data);
		}

		public IEnumerable<TEntity> GetItems(TEntity group)
		{
			PrepareGroup();
			TCollection collection;
			if (!Groups.TryGetValue(group, out collection))
			{
				var items = LoadItems(group);
				if (items == null)
					return Array<TEntity>.Empty;
				InsertRange(group, items);
				if (!Groups.TryGetValue(group, out collection))
					return Array<TEntity>.Empty;
			}

			return GetItems(group, collection);
		}

		protected virtual IEnumerable<TEntity> GetItems(TEntity group, TCollection collection) => collection.ToArray();

		public IEnumerable<TEntity> LoadItems(TEntity group)
		{
			if (ItemsLoader == null)
				return null;
			return ItemsLoader(group);
		}

		protected virtual TCollection PrepareCollection(TEntity group)
		{
			TCollection collection;
			if (Groups != null && Groups.TryGetValue(group, out collection))
				return collection;
			return new TCollection();
		}

		public void Add(TEntity item) => Insert(item);

		public bool Contains(TEntity item)
		{
			TCollection items;
			return Groups != null && Groups.TryGetValue(item, out items) && items.Contains(item);
		}

		public void CopyTo(TEntity[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public virtual IGroupedCollection<TEntity> Clone()
		{
			var grouped = new GroupedCollection<TEntity, TCollection>(Cache, Comparer, ItemsLoader);
			grouped.UniqueKeyComparer = UniqueKeyComparer;
			return grouped;
		}

		public bool Remove(TEntity item)
		{
			Delete(item);
			return true;
		}

		public IEnumerator<TEntity> GetEnumerator() => Groups?.Values.SelectMany(x => x).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		protected void ThrowIfNoCollection(TEntity group)
		{
			if (Groups == null || !Groups.ContainsKey(group))
				throw new NullReferenceException();
		}
	}

	[PXInternalUseOnly]
	public class UpperGroupedCollection<TEntity, TCollection> : GroupedCollection<TEntity, TCollection>
		where TEntity : class, IBqlTable, new()
		where TCollection : IGroupedCollection<TEntity>, new()
	{
		protected TCollection ChildTemplate;

		public UpperGroupedCollection()
		{
		}

		public UpperGroupedCollection(IEqualityComparer<TEntity> comparer, GroupItemsLoadHandler<TEntity> itemsLoader, TCollection childTemplate)
			: base(childTemplate.Cache, comparer, itemsLoader)
		{
			ChildTemplate = childTemplate;
		}

		public override void Insert(TEntity data)
		{
			ThrowIfNoCollection(data);

			var collection = PrepareCollection(data);
			collection.Insert(data);

			if (!Groups.ContainsKey(data))
				Groups.Add(data, collection);
		}

		public override void InsertRange(TEntity group, IEnumerable<TEntity> range)
		{
			var items = range.Where(x => Comparer.Equals(group, x)).ToList();
			var collection = PrepareCollection(group);
			collection.InsertRange(group, items);
			Groups.Add(group, collection);
		}

		public override void Update(TEntity oldData, TEntity newData)
		{
			ThrowIfNoCollection(oldData);

			var collection = PrepareCollection(oldData);
			collection.Update(oldData, newData);
		}

		public override void Delete(TEntity data)
		{
			ThrowIfNoCollection(data);

			var collection = PrepareCollection(data);
			collection.Delete(data);
		}

		protected override IEnumerable<TEntity> GetItems(TEntity group, TCollection collection)
		{
			return collection.GetItems(group);
		}

		public override IGroupedCollection<TEntity> Clone()
		{
			return new UpperGroupedCollection<TEntity, TCollection>(Comparer, ItemsLoader, ChildTemplate);
		}

		protected override TCollection PrepareCollection(TEntity group)
		{
			TCollection collection;
			if (Groups != null && Groups.TryGetValue(group, out collection) == true)
				return collection;

			if (ChildTemplate == null)
				return base.PrepareCollection(group);
			return (TCollection)ChildTemplate.Clone();
		}
	}
}
