using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Linq;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	internal class DefaultEndpointImplCR : DefaultEndpointImplCRBase
	{
		public DefaultEndpointImplCR(
			CbApiWorkflowApplicator.CaseApplicator caseApplicator,
			CbApiWorkflowApplicator.OpportunityApplicator opportunityApplicator,
			CbApiWorkflowApplicator.LeadApplicator leadApplicator)
			: base(caseApplicator, opportunityApplicator, leadApplicator) { }

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Opportunity_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureAndGetCurrentForInsert<CROpportunity>(graph);
			OpportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Opportunity_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureAndGetCurrentForUpdate<CROpportunity>(graph);
			OpportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Case_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureAndGetCurrentForInsert<CRCase>(graph);
			CaseApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Case_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureAndGetCurrentForUpdate<CRCase>(graph);
			CaseApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Lead_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureAndGetCurrentForInsert<CRLead>(graph);
			LeadApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Lead_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EnsureAndGetCurrentForUpdate<CRLead>(graph);
			LeadApplicator.ApplyStatusChange(graph, MetadataProvider, entity);
		}
	}
}
