using System;
using System.Collections;
using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.Common;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PM.Descriptor.Attributes
{
	public class CommitmentRefNoteAttribute : PXRefNoteBaseAttribute
	{
		public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
		{
			using (new PXReadBranchRestrictedScope())
			{
				var noteId = (Guid?)cache.GetValue(args.Row, _FieldOrdinal);
				args.ReturnValue = GetDescription(cache.Graph, noteId);
			}
		}

		public override void CacheAttached(PXCache cache)
		{
			base.CacheAttached(cache);
			var actionName = $"{cache.GetItemType().Name}${_FieldName}$Link";
			cache.Graph.Actions[actionName] = (PXAction)Activator.CreateInstance(
				typeof(PXNamedAction<>).MakeGenericType(typeof(CommitmentInquiry.ProjectBalanceFilter)),
				cache.Graph, actionName, (PXButtonDelegate)RedirectToRelatedScreen, GetEventSubscriberAttributes());
		}

		private IEnumerable RedirectToRelatedScreen(PXAdapter adapter)
		{
			var cache = adapter.View.Graph.Caches[typeof(PMCommitment)];
			if (cache.Current != null)
			{
				var fieldName = _FieldName;
				var fieldNameValue = cache.GetValueExt(cache.Current, fieldName);
				if (fieldNameValue is PXLinkState linkState)
				{
					helper.NavigateToRow(linkState.target.FullName, linkState.keys,
						PXRedirectHelper.WindowMode.NewWindow);
				}
				else
				{
					var noteId = (Guid?)cache.GetValue(cache.Current, fieldName);
					var note = helper.SelectNote(noteId);
					if (IsSubcontract(adapter.View.Graph, note))
					{
						RedirectToSubcontractScreen(noteId);
					}
					helper.NavigateToRow(noteId, PXRedirectHelper.WindowMode.NewWindow);
				}
			}
			return adapter.Get();
		}

		private void RedirectToSubcontractScreen(Guid? noteId)
		{
			var graph = PXGraph.CreateInstance<SubcontractEntry>();
			var row = (POOrder)helper.GetEntityRow(noteId);
			graph.Document.Current = row;
			throw GetRedirectException(graph);
		}

		private string GetDescription(PXGraph graph, Guid? noteId)
		{
			var note = helper.SelectNote(noteId);
			if (note == null)
			{
				return string.Empty;
			}
			return IsSubcontract(graph, note)
				? helper.GetEntityRowID(noteId, null)
				: helper.GetEntityRowID(noteId);
		}

		private static PXRedirectRequiredException GetRedirectException(PXGraph graph)
		{
			return new PXRedirectRequiredException(graph, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		private static bool IsSubcontract(PXGraph graph, Note note)
		{
			var purchaseOrder = GetPurchaseOrder(graph, note);
			return purchaseOrder.OrderType == POOrderType.RegularSubcontract;
		}

		private static PXEventSubscriberAttribute[] GetEventSubscriberAttributes()
		{
			return new PXEventSubscriberAttribute[]
			{
				new PXUIFieldAttribute
				{
					MapEnableRights = PXCacheRights.Select
				}
			};
		}

		private static POOrder GetPurchaseOrder(PXGraph graph, Note note)
		{
			var query = new PXSelect<POOrder,
				Where<POOrder.noteID, Equal<Required<Note.noteID>>>>(graph);
			using (new PXReadBranchRestrictedScope())
			{
				return query.SelectSingle(note.NoteID);
			}
		}
	}
}
