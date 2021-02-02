using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using EntityHelper = PX.Data.EntityHelper;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("20.200.001", "Default")]
	internal class DefaultEndpointImplCR20 : DefaultEndpointImplCRBase
	{
		public DefaultEndpointImplCR20(
			CbApiWorkflowApplicator.CaseApplicator caseApplicator,
			CbApiWorkflowApplicator.OpportunityApplicator opportunityApplicator,
			CbApiWorkflowApplicator.LeadApplicator leadApplicator)
			: base(caseApplicator, opportunityApplicator, leadApplicator)
		{
		}

		// (almost) copy paster from ServiceManager
		/// <summary>
		/// Returns true when specifeid type is a BqlTable object
		/// </summary>
		static bool IsTable(Type t)
		{

			return t != null
				&& typeof(IBqlTable).IsAssignableFrom(t)
				&& !typeof(PXMappedCacheExtension).IsAssignableFrom(t);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Activity_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForInsert<CRActivity>(graph), targetEntity);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Activity_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForUpdate<CRActivity>(graph), targetEntity);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Email_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForInsert<CRSMEmail>(graph), targetEntity);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Email_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForUpdate<CRSMEmail>(graph), targetEntity);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Task_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForInsert<CRActivity>(graph), targetEntity);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Task_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForUpdate<CRActivity>(graph), targetEntity);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Event_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForInsert<CRActivity>(graph), targetEntity);
		}

		[FieldsProcessed(new[] {
			"RelatedEntityNoteID",
			"RelatedEntityType",
			"RelatedEntityDescription" })]
		protected virtual void Event_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			//only as Top Level
			if (entity == targetEntity)
				Activity_Insert_Update(graph, EnsureAndGetCurrentForUpdate<CRActivity>(graph), targetEntity);
		}


		protected virtual void Activity_Insert_Update(PXGraph graph, CRActivity activity, EntityImpl targetEntity)
		{
			var refNoteIDstr = GetField(targetEntity, "RelatedEntityNoteID")?.Value;
			var refNoteType = GetField(targetEntity, "RelatedEntityType")?.Value;
			if (refNoteIDstr != null && Guid.TryParse(refNoteIDstr, out Guid refNoteID))
			{
				var helper = new EntityHelper(graph);
				var note = helper.SelectNote(refNoteID);
				Type type;
				if (note == null)
				{
					if (refNoteType == null)
					{
						PXTrace.WriteError("Note cannot be found and RelatedEntityType is not a specified");
						return;
					}
					type = PXBuildManager.GetType(refNoteType, false);
					if (!IsTable(type))
					{
						PXTrace.WriteError("Note cannot be found and RelatedEntityType is not a table: {type}", refNoteType);
						return;
					}
					var noteAttribute = EntityHelper.GetNoteAttribute(type);
					if (noteAttribute == null || !noteAttribute.ShowInReferenceSelector)
					{
						PXTrace.WriteError("RelatedEntityType is not supported as Related Entity. Type: {type}", refNoteType);
						return;
					}

					PXNoteAttribute.InsertNoteRecord(graph.Caches[type], refNoteID);
				}
				else
				{
					type = System.Web.Compilation.PXBuildManager.GetType(note.EntityType, false);
				}
				if (type == typeof(BAccount))
				{
					activity.BAccountID = graph
						.Select<BAccount>()
						.Where(b => b.NoteID == refNoteID)
						.Select(b => b.BAccountID)
						.FirstOrDefault();
				}
				else if (type == typeof(Contact))
				{
					var item = graph
						.Select<Contact>()
						.Where(c => c.NoteID == refNoteID)
						.Select(c => new { c.BAccountID, c.ContactID })
						.FirstOrDefault();

					activity.BAccountID = item?.BAccountID;
					activity.ContactID = item?.ContactID;
				}

				activity.RefNoteID = refNoteID;
			}
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Opportunity_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var current = EnsureAndGetCurrentForInsert<CROpportunity>(graph);
			OpportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Opportunity_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var current = EnsureAndGetCurrentForUpdate<CROpportunity>(graph);
			OpportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Case_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var current = EnsureAndGetCurrentForInsert<CRCase>(graph);
			CaseApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Case_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var current = EnsureAndGetCurrentForUpdate<CRCase>(graph);
			CaseApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Lead_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var current = EnsureAndGetCurrentForInsert<CRLead>(graph);
			LeadApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Lead_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var current = EnsureAndGetCurrentForUpdate<CRLead>(graph);
			LeadApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new string[0])]
		protected virtual void ActivityDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			graph.RowPersisting.AddHandler("Activities", (s, e) =>
			{
				throw new PXOuterException(new Dictionary<string, string>(), graph.GetType(),
					e.Row,
					MessagesNoPrefix.CbApi_Activities_ActivityCannotBeInsertUpdatedDeleted);
			});
		}

		[FieldsProcessed(new string[0])]
		protected virtual void ActivityDetail_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			graph.RowPersisting.AddHandler("Activities", (s, e) =>
			{
				throw new PXOuterException(new Dictionary<string, string>(), graph.GetType(),
					e.Row,
					MessagesNoPrefix.CbApi_Activities_ActivityCannotBeInsertUpdatedDeleted);
			});
		}

		protected virtual void ActivityDetail_Delete(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// TODO: replace with normal error

			throw new PXOuterException(new Dictionary<string, string>(), graph.GetType(),
				graph.Views["Activities"].Cache.Current,
				MessagesNoPrefix.CbApi_Activities_ActivityCannotBeInsertUpdatedDeleted);

		}
	}
}
