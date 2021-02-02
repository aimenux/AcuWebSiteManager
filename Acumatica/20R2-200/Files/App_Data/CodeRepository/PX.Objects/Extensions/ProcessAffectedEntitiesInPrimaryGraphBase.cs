using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.Extensions
{
	public abstract class ProcessAffectedEntitiesInPrimaryGraphBase<TSelf, TGraph, TEntity, TPrimaryGraphOfEntity> : PXGraphExtension<TGraph>
		where TSelf : ProcessAffectedEntitiesInPrimaryGraphBase<TSelf, TGraph, TEntity, TPrimaryGraphOfEntity>
		where TGraph : PXGraph
		where TEntity : class, IBqlTable, new()
		where TPrimaryGraphOfEntity : PXGraph, new()
	{
		[PXOverride]
		public virtual void Persist(Action basePersist)
		{
			IEnumerable<TEntity> affectedEntities = GetAffectedEntities();

			if (affectedEntities.Any())
			{
				if (PersistInSameTransaction)
				{
					using (var tran = new PXTransactionScope())
					{
						basePersist();
						var typesOfDirtyCaches = ProcessAffectedEntities(affectedEntities);

						tran.Complete();

						ClearCaches(Base, typesOfDirtyCaches);
					}
				}
				else
				{
					void OnAfterPersistHandler(PXGraph graph)
					{
						graph.OnAfterPersist -= OnAfterPersistHandler;

						var typesOfDirtyCaches = graph.FindImplementation<TSelf>().ProcessAffectedEntities(affectedEntities);

						ClearCaches(graph, typesOfDirtyCaches);
					}

					Base.OnAfterPersist += OnAfterPersistHandler;
					basePersist();
				}
			}
			else
				basePersist();
		}

		private IEnumerable<TEntity> GetAffectedEntities()
		{
			return Base
				.Caches<TEntity>()
				.Updated
				.Cast<TEntity>()
				.Where(EntityIsAffected)
				.ToArray();
		}

		private IEnumerable<Type> ProcessAffectedEntities(IEnumerable<TEntity> affectedEntities)
		{
			var typesOfDirtyCaches = new HashSet<Type>();
			var foreignGraph = PXGraph.CreateInstance<TPrimaryGraphOfEntity>();
			foreach (var entity in affectedEntities)
			{
				foreignGraph.Caches<TEntity>().Current = ActualizeEntity(foreignGraph, entity);

				ProcessAffectedEntity(foreignGraph, entity);

				if (foreignGraph.IsDirty)
				{
					foreach (var kvp in foreignGraph.Caches.Where(ch => ch.Value?.IsDirty == true))
						typesOfDirtyCaches.Add(kvp.Key);

					foreignGraph.Actions.PressSave();
					foreignGraph.Clear();
				}
			}
			return typesOfDirtyCaches;
		}

		private void ClearCaches(PXGraph graph, IEnumerable<Type> typesOfDirtyCaches)
		{
			foreach (Type cacheItemType in typesOfDirtyCaches)
				if (graph.Caches.Keys.Contains(cacheItemType))
					graph.Caches[cacheItemType].Clear();

			graph.SelectTimeStamp();
		}

		protected abstract bool PersistInSameTransaction { get; }
		protected abstract bool EntityIsAffected(TEntity entity);
		protected abstract void ProcessAffectedEntity(TPrimaryGraphOfEntity primaryGraph, TEntity entity);
		protected virtual TEntity ActualizeEntity(TPrimaryGraphOfEntity primaryGraph, TEntity entity) => entity;
	}
}