using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CR.MassProcess.DacRelations;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	[Obsolete("Will be removed in 7.0 version")]
	class FieldMerger : IMerger
	{
		private readonly Dictionary<Type, List<MergerRelationPair>> _fieldSetters = new Dictionary<Type, List<MergerRelationPair>>();

		private readonly Dictionary<Relation, List<IRelationManager>>    _recordRemappers =
			new Dictionary<Relation, List<IRelationManager>>();

		public FieldMerger(IEnumerable<Relation> relations,IMassMerge customMerger = null)
		{
			var availableMergers = new IRelationManager[]
				                       {
					                       new OneToManyManager(),
					                       new OneToOneManager()
				                       };

			foreach (Relation relation in relations)
			{
				bool customMergerFound = false;
				if (customMerger != null)
				foreach (IRelationManager merger in customMerger.CustomRelationManagers.Where(m => m.CanProcess(relation)))
				{
					AddRelationManager(relation, merger);
					customMergerFound = true;
				}

				if(!customMergerFound)
				foreach (IRelationManager merger in availableMergers.Where(m => m.CanProcess(relation)))
					AddRelationManager(relation, merger);
			}
		}

		private void AddRelationManager(Relation relation, IRelationManager merger)
		{
			AddToRecordRemappers(relation, merger);

			var mergeRelationPair = new MergerRelationPair {Manager = merger, Relation = relation};

			if (relation.Left != null)
				AddToFieldSetters(relation.Left, mergeRelationPair);

			if (relation.Right != null)
				AddToFieldSetters(relation.Right, mergeRelationPair);
		}

		private void AddToRecordRemappers(Relation relation, IRelationManager merger)
		{
			if(_recordRemappers.ContainsKey(relation)) 
				_recordRemappers[relation].Add(merger);
			else
				_recordRemappers.Add(relation,new List<IRelationManager>{merger});
		}


		private void AddToFieldSetters(DacReference reference, MergerRelationPair merger)
		{
			if (_fieldSetters.ContainsKey(reference.TargetField))
				_fieldSetters[reference.TargetField].Add(merger);
			else
				_fieldSetters.Add(reference.TargetField, new List<MergerRelationPair> { merger });

			if (_fieldSetters.ContainsKey(reference.ReferenceField))
				_fieldSetters[reference.ReferenceField].Add(merger);
			else
				_fieldSetters.Add(reference.ReferenceField, new List<MergerRelationPair> { merger });
		}

		internal IEnumerable<PXCache> GetImpactedCaches(PXGraph graph)
		{
			return _fieldSetters.Keys.Select(k => graph.Caches[k.DeclaringType]).Distinct();
		}

		public void SetField(PXGraph graph, object resultOldValuesRecord, Type changingField, object newValue)
		{
			//object oldValue = graph.Caches[changingField.DeclaringType].GetValue(resultOldValuesRecord, changingField.Name);
			//if (newValue != null && newValue.Equals(oldValue)) return;
			//if (newValue == null && oldValue == null) return;

			if (_fieldSetters.ContainsKey(changingField))
				_fieldSetters[changingField].ForEach(
					pair => pair.Manager.SetRelation(graph, pair.Relation, resultOldValuesRecord, changingField, newValue));
			else
			{																				 
				graph.Caches[changingField.DeclaringType].SetValueExt(resultOldValuesRecord, changingField.Name, newValue);
			}
		}

		

		public void DeleteRecord(PXGraph graph, object deletingRecord, object remappingRecord,Type deletingRecordIdField)
		{
			_recordRemappers.ForEach(pair => 
						pair.Value.ForEach(mngr => 
								mngr.RemapRelations(graph, pair.Key, deletingRecord, remappingRecord)));

			graph.Caches[deletingRecordIdField.DeclaringType].SetStatus(deletingRecord, PXEntryStatus.Deleted);
		}

	}
}
