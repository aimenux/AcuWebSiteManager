using System;
using PX.Data;

using PX.Objects.CR.MassProcess.DacRelations;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	[Obsolete("Will be removed in 7.0 version")]
	class OneToManyManager:RelationManagerBase<OneToManyRelation>
	{

		public override void SetRelation(PXGraph graph, Relation relation, object oldRecord, Type changingField, object newValue)
		{
			if (!CanProcess(relation)) return;
			var oneToManyRelation = (OneToManyRelation) relation;

			Type mainDacType = oneToManyRelation.Right.TargetField.DeclaringType;
			Type parentField = oneToManyRelation.Right.TargetField;

			Type childForeignKeyField = oneToManyRelation.Right.ReferenceField;


			if (changingField == parentField)
			{
				object oldParentValue = graph.Caches[mainDacType].GetValue(oldRecord, parentField.Name); ;

				if (!oldParentValue.Equals(newValue))
					foreach (object correctingEntity in GraphExtensions.SelectById(graph, childForeignKeyField, oldParentValue))
						ChangeReference(graph, oneToManyRelation.Right, correctingEntity, newValue);
			}

			if (changingField == childForeignKeyField)
				ChangeReference(graph, oneToManyRelation.Right, oldRecord, newValue);
		}

		public override void RemapRelations(PXGraph graph, Relation relation, object oldRecord, object remapToRecord)
		{
			if (!CanProcess(relation)) return;
			var oneToManyRelation = (OneToManyRelation)relation;

			Type childForeignKeyField = oneToManyRelation.Right.ReferenceField;
			Type deletingDacKeyField = oneToManyRelation.Right.TargetField;

			PXCache deletingDacCache = graph.Caches[deletingDacKeyField.DeclaringType];

			object oldParentValue = deletingDacCache.GetValue(oldRecord, deletingDacKeyField.Name);
			object newValue = deletingDacCache.GetValue(remapToRecord, deletingDacKeyField.Name);

			foreach (object correctingEntity in graph.SelectById(childForeignKeyField, oldParentValue))
				ChangeReference(graph, oneToManyRelation.Right, correctingEntity, newValue);

		}

	}
}
