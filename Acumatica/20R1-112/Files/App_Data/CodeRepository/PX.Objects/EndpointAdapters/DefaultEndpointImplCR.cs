using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	internal class DefaultEndpointImplCR : IAdapterWithMetadata
	{
		public IEntityMetadataProvider MetadataProvider { private get; set; }

		private readonly CbApiWorkflowApplicator.CaseApplicator _caseApplicator;
		private readonly CbApiWorkflowApplicator.OpportunityApplicator _opportunityApplicator;
		private readonly CbApiWorkflowApplicator.LeadApplicator _leadApplicator;

		public DefaultEndpointImplCR(
			CbApiWorkflowApplicator.CaseApplicator caseApplicator,
			CbApiWorkflowApplicator.OpportunityApplicator opportunityApplicator,
			CbApiWorkflowApplicator.LeadApplicator leadApplicator)
		{
			_caseApplicator = caseApplicator;
			_opportunityApplicator = opportunityApplicator;
			_leadApplicator = leadApplicator;
		}

		private void EnsureCurrentForInsert<T>(PXGraph graph) where T : class, IBqlTable, new()
		{
			var cache = graph.Caches<T>();
			if (cache.Insert() as T == null)
			{
				// this means there is trash in graph from previous session
				graph.Clear();
				cache.Insert();
			}
		}

		private void EnsureCurrentForUpdate<T>(PXGraph graph) where T : class, IBqlTable, new()
		{
			var cache = graph.Caches<T>();
			if (cache.Current as T == null)
			{
				// just for sure, if there is trash too
				graph.Clear();
				cache.Insert();
			}
		}



		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Opportunity_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureCurrentForInsert<CROpportunity>(graph);
			_opportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Opportunity_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureCurrentForUpdate<CROpportunity>(graph);
			_opportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}


		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Case_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureCurrentForInsert<CRCase>(graph);
			_caseApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Case_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureCurrentForUpdate<CRCase>(graph);
			_caseApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Lead_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureCurrentForInsert<CRLead>(graph);
			_leadApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Lead_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureCurrentForUpdate<CRLead>(graph);
			_leadApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}
	}
}
