using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Extensions.PinActivity
{
	#region DACs

	[Serializable]
	[PXHidden]
	public class CRActivityPin : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXParent(typeof(SelectFrom<CRActivity>.
			Where<CRActivity.noteID.IsEqual<CRActivityPin.noteID.FromCurrent>>))]
		[PXParent(typeof(SelectFrom<CRSMEmail>.
			Where<CRSMEmail.noteID.IsEqual<CRActivityPin.noteID.FromCurrent>>))]
		[PXParent(typeof(SelectFrom<CRPMTimeActivity>.
			Where<CRPMTimeActivity.noteID.IsEqual<CRActivityPin.noteID.FromCurrent>>))]
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRActivity.noteID))]
		[PXUIField(Visible = false, DisplayName = "NoteID of Pinned Activity")]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID(IsKey = true)]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region Tstamp
		[PXDBTimestamp()]
		public virtual byte[] Tstamp { get; set; }
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		#endregion
		#endregion
	}

	[Serializable]
	public sealed class CRActivityPinCacheExtension : PXCacheExtension<CRActivity>
	{
		#region IsPinned
		public abstract class isPinned : PX.Data.BQL.BqlString.Field<isPinned>
		{
			public static string Pinned = Sprite.Ac.GetFullUrl(Sprite.Ac.Pin);
			public static string Unpinned = Sprite.Control.GetFullUrl(Sprite.Control.Empty);
		}

		[PXDBCustomImage(HeaderImage = (Sprite.AliasAc + "@" + Sprite.Ac.Pin))]
		[PXUIField(DisplayName = "Is Pinned", IsReadOnly = true)]
		public string IsPinned { get; set; }
		#endregion
	}

	#endregion

	#region Helpers

	public class PXDBCustomImageAttribute : PXDBImageAttribute
	{
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) ||
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Update) ||
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete))
			{
				e.ExcludeFromInsertUpdate();
				e.Expr = null;
			}
			else
			{
				e.Expr = SQLExpression.Null();
			}
		}
	}

	public class PXPredefinedOrderedView<TBqlField> : PXView
		where TBqlField : IBqlField
	{
		public PXPredefinedOrderedView(PXGraph graph, bool isReadOnly, BqlCommand select)
			: base(graph, isReadOnly, select)
		{
		}

		public bool IsCompare { get; set; }
		protected override int Compare(object a, object b, compareDelegate[] comparisons)
		{
			IsCompare = true;
			int compare = base.Compare(a, b, comparisons);
			IsCompare = false;
			return compare;
		}

		public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (searches != null && searches.Where(x => x != null).Any())
			{
				return base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}

			List<object> newSearches = new List<object>();
			List<string> newSorts = new List<string>();
			List<bool> newDescs = new List<bool>();

			newSorts.Add(typeof(TBqlField).Name);
			newDescs.Add(false);
			newSearches.Add(null);

			if (searches != null)
			{
				newSearches.AddRange(searches);
			}
			if (sortcolumns != null)
			{
				newSorts.AddRange(sortcolumns);
			}
			if (descendings != null)
			{
				newDescs.AddRange(descendings);
			}

			return base.Select(currents, parameters, newSearches.ToArray(), newSorts.ToArray(), newDescs.ToArray(), filters, ref startRow, maximumRows, ref totalRows);
		}
	}

	#endregion

	public class CRActivityPinnedViewAttribute : PXViewExtensionAttribute
	{
		public CRActivityPinnedViewAttribute()
			: base() { }

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			graph.Views.Caches.Add(typeof(CRActivityPin));

			graph.Views[viewName] = new PXPredefinedOrderedView<CRActivityPinCacheExtension.isPinned>(graph, graph.Views[viewName].IsReadOnly, graph.Views[viewName].BqlSelect);

			Type itemType = graph.Views[viewName].Cache.GetItemType();

			graph.CommandPreparing.AddHandler(viewName, nameof(CRActivityPinCacheExtension.IsPinned), (PXCache sender, PXCommandPreparingEventArgs args) => IsPinned_CommandPreparing(itemType, sender, args));

			graph.FieldSelecting.AddHandler(viewName, nameof(CRActivityPinCacheExtension.IsPinned), (PXCache sender, PXFieldSelectingEventArgs args) => IsPinned_FieldSelecting(viewName, sender, args));

			graph.OnAfterPersist += pxGraph => pxGraph.Views[viewName].Cache.Clear();

			PXNamedAction.AddAction(graph, graph.PrimaryItemType, nameof(TogglePinActivity), Messages.PinUnpin, adapter => TogglePinActivity(graph, viewName, adapter));
		}

		protected virtual void IsPinned_FieldSelecting(string viewName, PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (sender.Graph.Views[viewName] is PXPredefinedOrderedView<CRActivityPinCacheExtension.isPinned>)
			{
				PXPredefinedOrderedView<CRActivityPinCacheExtension.isPinned> view = (sender.Graph.Views[viewName] as PXPredefinedOrderedView<CRActivityPinCacheExtension.isPinned>);
				if (view.IsCompare)
				{
					return;
				}
			}

			Guid? selectingNoteId = sender.GetValue(e.Row, nameof(INotable.NoteID)) as Guid?;

			foreach (CRActivityPin pin in sender.Graph.Caches[typeof(CRActivityPin)]
				.Dirty
				.Cast<CRActivityPin>()
				.Where(x => x.NoteID == selectingNoteId))
			{
				e.ReturnState =
					sender.Graph.Caches[typeof(CRActivityPin)].GetStatus(pin) == PXEntryStatus.Deleted
						? CRActivityPinCacheExtension.isPinned.Unpinned
						: CRActivityPinCacheExtension.isPinned.Pinned;
			}
		}

		protected virtual void IsPinned_CommandPreparing(Type itemType, PXCache sender, PXCommandPreparingEventArgs e)
		{
			Query q = new Query();

			q.Field(new SQLConst(true))
				.From(new SimpleTable<CRActivityPin>())
					.Where(new Column<CRActivityPin.noteID>()
						.EQ(new Column(nameof(INotable.NoteID), new SimpleTable(itemType.Name)))
						.And(new Column<CRActivityPin.createdByScreenID>())
							.EQ(Unmask(sender?.Graph?.Accessinfo?.ScreenID)));

			SQLExpression whenExpr = new SubQuery(q).Exists();
			SQLSwitch switchExpr = new SQLSwitch()
				.Case(whenExpr, new SQLConst(CRActivityPinCacheExtension.isPinned.Pinned))
				.Default(new SQLConst(CRActivityPinCacheExtension.isPinned.Unpinned));
			e.Expr = switchExpr;
			e.BqlTable = itemType;

			e.Cancel = true;
			e.DataType = PXDbType.Bit;
			e.DataLength = 1;
			e.DataValue = e.Value;
		}

		[PXButton]
		public virtual IEnumerable TogglePinActivity(PXGraph graph, string viewName, PXAdapter adapter)
		{
			PXCache cache = graph.Views[viewName].Cache;

			INotable activity = cache?.Current as INotable;

			if (activity == null)
			{
				return adapter.Get();
			}

			bool wasPinned = (cache.GetStateExt(activity, nameof(CRActivityPinCacheExtension.IsPinned)) as PXFieldState)?.Value as string == CRActivityPinCacheExtension.isPinned.Pinned;
			string screenID = Unmask(graph?.Accessinfo?.ScreenID);

			if (wasPinned)
			{
				graph.Caches[typeof(CRActivityPin)].Delete(new CRActivityPin()
				{
					NoteID = activity.NoteID,
					CreatedByScreenID = screenID
				});
			}
			else
			{
				graph.Caches[typeof(CRActivityPin)].Insert(new CRActivityPin()
				{
					NoteID = activity.NoteID
				});
			}

			return adapter.Get();
		}

		protected string Unmask(string screenID)
		{
			if (string.IsNullOrEmpty(screenID))
			{
				return screenID;
			}

			return screenID.Replace(".", "");
		}
	}
}
