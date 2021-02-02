using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CR.MassProcess.DacRelations;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	[Obsolete("Will be removed in 7.0 version")]
	public abstract class RelationManagerBase<T> : IRelationManager where T:Relation
	{
		protected object[] ChangeReference(PXGraph graph, DacReference reference, object childEntity, object newId)
		{
			PXCache cache = graph.Caches[reference.ReferenceField.DeclaringType];
			string changingFieldName = reference.ReferenceField.Name;
			Func<object,object[]> keysSelector = (item) => cache.Keys.Select(k => cache.GetValue(item, k)).ToArray();

			//for selectors
			cache.Current = childEntity;

			object oldValue = cache.GetValue(childEntity, changingFieldName);
			if (oldValue != null && oldValue.Equals(newId) ||
				oldValue == null && newId == null) return keysSelector(childEntity);

			//setting ordinary value
			if (cache.Keys.All(k => k.ToLower() != changingFieldName.ToLower()))
			{
				cache.SetValueExt(childEntity, changingFieldName, newId);
				cache.SetStatus(childEntity, PXEntryStatus.Updated);
				return keysSelector(childEntity);
			}

			//setting value that is part of PK key
			//if it is identity column, it would be changed after insert
			object newItem = cache.CreateCopy(childEntity);
			cache.SetValueExt(newItem, changingFieldName, newId);
			//resetting keys to persist new entity

			string[] identityKeys = cache.Keys.Where(k => cache.GetAttributesReadonly(newItem, k)
										.OfType<PXDBIdentityAttribute>()
										.Any()).ToArray();

			identityKeys.ForEach(k => cache.SetValueExt(newItem, k, null));
			cache.Update(newItem);
			cache.SetStatus(childEntity, PXEntryStatus.Deleted);

			EntityForIdentityInserting(graph,newItem);
			/*cache.PersistInserted(newItem);
			cache.SetStatus(newItem, PXEntryStatus.Notchanged);*/

			return keysSelector(newItem);
		}

		protected virtual void EntityForIdentityInserting(PXGraph graph,object entity) 
		{
			
		}

		public abstract void SetRelation(PXGraph graph, Relation relation, object oldRecord, Type changingField,
		                                 object newValue);

		public abstract void RemapRelations(PXGraph graph, Relation relation, object oldRecord, object remapToRecord);

		public virtual bool CanProcess(Relation relation)
		{
			return relation is T;
		}
	}
}
