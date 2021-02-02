using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Automation;
using PX.Api.ContractBased.Models;
using PX.Common;
using PX.Data;
using PX.Data.Automation;
using PX.Data.BQL;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EndpointAdapters
{
	internal static class CbApiWorkflowApplicator
	{
		internal class OpportunityApplicator : CbApiWorkflowApplicator<CROpportunity, CROpportunity.status>
		{
			public OpportunityApplicator(IWorkflowServiceWrapper workflowService) : base(workflowService, "Opportunity")
			{
			}
		}

		internal class CaseApplicator : CbApiWorkflowApplicator<CRCase, CRCase.status>
		{
			public CaseApplicator(IWorkflowServiceWrapper workflowService) : base(workflowService, "Case")
			{
			}
		}

		internal class LeadApplicator : CbApiWorkflowApplicator<CRLead, CRLead.status>
		{
			public LeadApplicator(IWorkflowServiceWrapper workflowService) : base(workflowService, "Lead")
			{
			}
		}
	}

	internal abstract class CbApiWorkflowApplicator<TTable, TStatusField>
		where TTable : class, IBqlTable, new()
		where TStatusField : IBqlField
	{
		private readonly IWorkflowServiceWrapper _workflowService;
		private readonly string _entityName;

		public CbApiWorkflowApplicator(IWorkflowServiceWrapper workflowService, string entityName)
		{
			_workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
			_entityName = entityName;
		}

		public void ApplyStatusChange(PXGraph graph, IEntityMetadataProvider metadataProvider, EntityImpl entity)
		{
			var viewName = metadataProvider.GetPrimaryViewName();
			var statusFieldName = metadataProvider.GetMappedFields()
				.Where(i => i.MappedObject == viewName
							&& i.MappedField.Equals(typeof(TStatusField).Name, StringComparison.OrdinalIgnoreCase))
				.Select(i => i.FieldName)
				.FirstOrDefault();
			if (statusFieldName == null)
				throw new NotSupportedException($"Cannot find mapping for Status field: {typeof(TStatusField).Name}.");

			var cache = graph.Caches<TTable>();
			var status = entity
					.Fields
					.OfType<EntityValueField>()
					.Where(f => statusFieldName.Equals(f.Name, StringComparison.OrdinalIgnoreCase))
					.Select(f => f.Value)
					.FirstOrDefault();

			if (status == null || _workflowService.IsInSpecifiedStatus(graph, status))
				return;

			var transition = GetTransition(graph, status);

			var action = graph.Actions[transition.ActionName];
			if (action == null)
				throw new InvalidOperationException($"Action named: {transition.ActionName} for specified status: {status} is not available.");

			graph.OnAfterPersist += handler;

			// need mark as dirty to call persist in any case, even if fields weren't updated,
			// otherwise action wouldn't be triggered
			cache.IsDirty = true;

			void handler(PXGraph g)
			{
				g.OnAfterPersist -= handler;
				_workflowService.FillFormValues(transition, entity, graph, metadataProvider);
				action.Press();
			}
		}

		private TransitionInfo GetTransition(PXGraph graph, string status)
		{
			var transitions = _workflowService.GetPossibleTransition(graph, status).ToList();

			if (transitions.Count == 0)
			{
				string error = $"Cannot find workflow transition applicable for entity: \"{_entityName}\" to status: \"{status}\".";
				PXTrace.WriteWarning("CB-API Warning: " + error);
				throw new InvalidOperationException(error);
			}
			if (transitions.Count > 1)
			{
				PXTrace.WriteVerbose($"CB-API Info: More than one workflow transition applicable for entity: \"{_entityName}\" to status: \"{status}\" is found. ");
			}

			PXTrace.WriteVerbose($"CB-API Info: Use workflow transition with name: { transitions[0].Name} for entity: \"{_entityName}\" to status: \"{status}\".");

			return transitions[0];
		}
	}
}
