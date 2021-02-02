using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
	public class ProjectManagementActivityList<TProjectManagementEntity> : CRActivityList<TProjectManagementEntity>
		where TProjectManagementEntity : class, IBqlTable, new()
	{
		public ProjectManagementActivityList(PXGraph graph)
			: base(graph)
		{
		}

		public Guid? CurrentProjectManagementEntityNoteId
		{
			get;
			set;
		}

		protected override void Activity_Body_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
		{
			if (args.Row is CRPMTimeActivity timeActivity && timeActivity.ClassID == CRActivityClass.Email)
			{
				args.ReturnValue = GetEmailBody(cache.Graph, timeActivity.NoteID)?.Body;
			}
		}

		protected override void Activity_RefNoteID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
		{
			if (cache.Graph.IsMobile)
			{
				args.NewValue = CurrentProjectManagementEntityNoteId;
			}
			else
			{
				base.Activity_RefNoteID_FieldDefaulting(cache, args);
			}
		}

		private SMEmailBody GetEmailBody(PXGraph graph, Guid? noteId)
		{
			return SelectFrom<SMEmailBody>
				.Where<SMEmailBody.refNoteID.IsEqual<P.AsGuid>>.View.Select(graph, noteId);
		}
	}
}