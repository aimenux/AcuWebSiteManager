using System;
using System.Linq;
using PX.Data;

using PX.Objects.CR.MassProcess.DacRelations;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	[Obsolete("Will be removed in 7.0 version")]
	class OneToOneManager:OneToOneManager<OneToOneRelation>
    {
	    
    }
	[Obsolete("Will be removed in 7.0 version")]
	class OneToOneManager<T> : RelationManagerBase<T>  where T:Relation
	{
		public override void SetRelation(PXGraph graph, Relation relation, object oldRecord, Type changingField, object newValue)
		{
			if (!CanProcess(relation)) return;
			var oneToOneRelation = (OneToOneRelation)relation;

			if (changingField == oneToOneRelation.Left.ReferenceField)
			{
				Type referencedDacType = oneToOneRelation.Left.TargetField.DeclaringType;
				Type referencedDacIdField = oneToOneRelation.Left.TargetField;
				Type changingDacIdField = oneToOneRelation.Right.TargetField;
				PXCache referencedDacCache = graph.Caches[referencedDacType];
				PXCache changingDacCache = graph.Caches[changingField.DeclaringType];

				object newReferencedDac = graph.SelectFirst(referencedDacIdField, newValue);

				//delete old referencedDac
				object oldReferencedDacId = changingDacCache.GetValue(oldRecord, oneToOneRelation.Left.ReferenceField.Name);
				object oldReferencedDac = graph.SelectFirst(referencedDacIdField, oldReferencedDacId);

				referencedDacCache.Delete(oldReferencedDac);
				referencedDacCache.SetStatus(oldReferencedDac,PXEntryStatus.Deleted);

				//set reference to new referencedDac
				object changingDacId = changingDacCache.GetValue(oldRecord, changingDacIdField.Name);

				ChangeReference(graph, oneToOneRelation.Right, newReferencedDac, changingDacId);
				ChangeReference(graph, oneToOneRelation.Left,  oldRecord, newValue);
			}
		}

		public override void RemapRelations(PXGraph graph, Relation relation, object oldRecord, object remapToRecord)
		{
			if (!CanProcess(relation)) return;
			var oneToManyRelation = (OneToOneRelation)relation;

			Type referencedDacType = oneToManyRelation.Left.TargetField.DeclaringType;
			Type referencedDacForeignField = oneToManyRelation.Right.ReferenceField;
			Type deletingDacIDField = relation.Right.TargetField;

			PXCache referencedDacCache = graph.Caches[referencedDacType];
			PXCache deletingDacCache = graph.Caches[deletingDacIDField.DeclaringType];

			object deletingDacId = deletingDacCache.GetValue(oldRecord, deletingDacIDField.Name);

			//referenceDac could be changed by custom relationManagers provided by user graph
			object referenceDac = graph.SelectById(referencedDacForeignField, deletingDacId).FirstOrDefault();
			if(referenceDac != null)
				referencedDacCache.SetStatus(referenceDac,PXEntryStatus.Deleted);
		}
	}
}
