using System;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.MassProcess;
using PX.Objects.CR;
using PX.Objects.EP;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
    public abstract class AssignBaseMassProcess<TGraph, TEntity, TField> : CRBaseMassProcess<TGraph, TEntity>,
        IMassProcess<TEntity>
        where TGraph : PXGraph, IMassProcess<TEntity>, new()
        where TEntity : class, IBqlTable, IAssign, new()
        where TField : IBqlField
    {
        public override void ProccessItem(PXGraph graph, TEntity entity)
        {
            var assignmentMapId = GetAssignmentMapId(graph);
            if (assignmentMapId == null)
            {
                throw new PXException(ProjectManagementMessages.AssignmentMapIdIsNotSpecified);
            }
            var cache = graph.Caches[typeof(TEntity)];
            var assignedEntity = AssignMapToEntity(entity, cache, assignmentMapId);
            cache.PersistUpdated(assignedEntity);
            cache.Update(assignedEntity);
        }

        private TEntity AssignMapToEntity(TEntity entity, PXCache cache, int? assignmentMapId)
        {
            var entityCopy = (TEntity) cache.CreateCopy(entity);
            var assignmentProcessor = CreateInstance<EPAssignmentProcessor<TEntity>>();
            if (!assignmentProcessor.Assign(entityCopy, assignmentMapId))
            {
                throw new PXException(ProjectManagementMessages.UnableToFindRouteForAssignmentProcess);
            }
            return entityCopy;
        }

        /// <summary>
        /// This method is used to get AssignmentMapId field value from different setup entities. We create search
        /// variable for get AssgnmentMapId field name and create view from which we will get this value.
        /// </summary>
        private static int? GetAssignmentMapId(PXGraph graph)
        {
            var search = (BqlCommand) Activator.CreateInstance(BqlCommand.Compose(typeof(Search<>), typeof(TField)));
            var view = new PXView(graph, true, BqlCommand.CreateInstance(search.GetSelectType()));
            var assignmentMapIdFieldName = ((IBqlSearch) search).GetField().Name;
            return view.SelectSingle().With(setup => (int?) view.Cache.GetValue(setup, assignmentMapIdFieldName));
        }
    }
}
