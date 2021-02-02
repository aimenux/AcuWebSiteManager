using System;
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.CS
{
	public class SegmentMaint : PXGraph<SegmentMaint>
	{
		private class GroupColumnsCache
		{
			private class Box
			{
				private readonly string _name;
				private readonly PXFieldSelecting _selectingHandler;
				private readonly PXFieldUpdating _updatingHandler;

				public Box(string name, PXFieldSelecting selectingHandler, PXFieldUpdating updatingHandler)
				{
					_name = name;
					_selectingHandler = selectingHandler;
					_updatingHandler = updatingHandler;
				}

				public string Name
				{
					get { return _name; }
				}

				public PXFieldSelecting SelectingHandler
				{
					get { return _selectingHandler; }
				}

				public PXFieldUpdating UpdatingHandler
				{
					get { return _updatingHandler; }
				}
			}

			private List<Box> _items;

			private string _prevDimensionId;
			private string _prevSpecificModule;

			public GroupColumnsCache()
			{
				_items = new List<Box>();
			}

			public void TryInjectColumns(PXCache target, string dimensionId)
			{
				if (target == null) throw new ArgumentNullException("target");

				if (object.Equals(dimensionId, _prevDimensionId)) return;

				var graph = target.Graph;
				var dim = ReadDimension(graph, dimensionId);
				var specificModule = dim.With(_ => _.SpecificModule);

				if (dim == null || string.IsNullOrEmpty(specificModule))
					RemovePreviousInjection(target);
				else if (!object.Equals(specificModule, _prevSpecificModule))
				{
					RemovePreviousInjection(target);
					AddNewInjection(target, specificModule);
				}

				_prevDimensionId = dimensionId;
				_prevSpecificModule = specificModule;
			}

			private void AddNewInjection(PXCache target, string specificModule)
			{
				var graph = target.Graph;
				foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup,
					Where<PX.SM.RelationGroup.specificModule, Equal<Required<Dimension.specificModule>>,
						And<PX.SM.RelationGroup.specificType, Equal<Required<PX.SM.RelationGroup.specificType>>>>>
					.Select(graph, specificModule, typeof(SegmentValue).FullName))
				{
					string name = @group.GroupName;
					byte[] mask = @group.GroupMask;
					if (!target.Fields.Contains(name) && mask != null)
					{
						PXFieldSelecting fieldSelectingHandler = delegate (PXCache cache, PXFieldSelectingEventArgs a)
						{
							a.ReturnState = PXFieldState.CreateInstance(a.ReturnState, typeof(Boolean), null, null, null, null, null, null, name, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Dynamic, null, null, null);
							SegmentValue value = (SegmentValue)a.Row;
							if (value != null)
							{
								a.ReturnValue = false;
								if (value.GroupMask != null)
								{
									for (int i = 0; i < value.GroupMask.Length && i < mask.Length; i++)
									{
										if (mask[i] != 0x00 && (mask[i] & value.GroupMask[i]) == mask[i])
										{
											a.ReturnValue = true;
											return;
										}
									}
								}
							}
						};
						PXFieldUpdating fieldUpdatingHandler = delegate (PXCache cache, PXFieldUpdatingEventArgs a)
						{
							SegmentValue value = (SegmentValue)a.Row;
							if (value != null && a.NewValue != null)
							{
								bool included = false;
								if (a.NewValue is string)
								{
									bool.TryParse((string)a.NewValue, out included);
								}
								else
								{
									included = (bool)a.NewValue;
								}
								if (value.GroupMask == null)
								{
									value.GroupMask = new byte[mask.Length];
								}
								else if (value.GroupMask.Length < mask.Length)
								{
									byte[] arr = value.GroupMask;
									Array.Resize<byte>(ref arr, mask.Length);
									value.GroupMask = arr;
								}
								for (int i = 0; i < mask.Length; i++)
								{
									if (mask[i] != 0x00)
									{
										if (included)
										{
											value.GroupMask[i] = (byte)(value.GroupMask[i] | mask[i]);
										}
										else
										{
											value.GroupMask[i] = (byte)(value.GroupMask[i] & ~mask[i]);
										}
										if (target.Locate(value) != null && target.GetStatus(value) == PXEntryStatus.Notchanged)
										{
											target.SetStatus(value, PXEntryStatus.Updated);
										}
										return;
									}
								}
							}
						};
						_items.Add(new Box(name, fieldSelectingHandler, fieldUpdatingHandler));
						target.Fields.Add(name);
						graph.FieldSelecting.AddHandler(typeof(SegmentValue), name, fieldSelectingHandler);
						graph.FieldUpdating.AddHandler(typeof(SegmentValue), name, fieldUpdatingHandler);
					}
				}
			}

			private Dimension ReadDimension(PXGraph graph, string dimensionId)
			{
				if (string.IsNullOrEmpty(dimensionId)) return null;

				return PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Segment.dimensionID>>>>.
					Select(graph, dimensionId);
			}

			private void RemovePreviousInjection(PXCache target)
			{
				var graph = target.Graph;
				foreach (Box item in _items)
				{
					var name = item.Name;
					target.Fields.Remove(name);
					graph.FieldSelecting.RemoveHandler(typeof(SegmentValue), name, item.SelectingHandler);
					graph.FieldUpdating.RemoveHandler(typeof(SegmentValue), name, item.UpdatingHandler);
				}
				_items.Clear();
			}
		}

		public PXSelect<Segment, Where<Segment.dimensionID, InFieldClassActivated>>
			Segment;

		[PXImport(typeof(Segment))]
		public PXSelect<SegmentValue,
			Where<SegmentValue.dimensionID, Equal<Optional<Segment.dimensionID>>,
				And<SegmentValue.segmentID, Equal<Optional<Segment.segmentID>>>>>
			Values;

		private GroupColumnsCache _groupColumnsCache;

		public SegmentMaint()
		{
			_groupColumnsCache = new GroupColumnsCache();

			Segment.Cache.AllowInsert = false;
			Segment.Cache.AllowDelete = false;
		}

		public PXSave<Segment> Save;
		public PXCancel<Segment> Cancel;
		public PXFirst<Segment> First;
		public PXPrevious<Segment> Previous;
		public PXNext<Segment> Next;
		public PXLast<Segment> Last;

		public override void Persist()
		{
			try
			{
				PXDimensionAttribute.Clear();
				base.Persist();
				PXDimensionAttribute.Clear();
			}
			catch (PXDatabaseException e)
			{
				if (e.ErrorCode == PXDbExceptions.DeleteForeignKeyConstraintViolation)
					throw new PXException(Messages.SegmentHasValues, e.Keys[1]);
				throw;
			}
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Segmented Key ID", Visibility = PXUIVisibility.Visible, TabOrder = 0)]
		[PXSelector(typeof(Search5<Dimension.dimensionID,
			InnerJoin<Segment, On<Segment.dimensionID, Equal<Dimension.dimensionID>>>,
			Where<Dimension.dimensionID, InFieldClassActivated>,
			Aggregate<GroupBy<Dimension.dimensionID>>>))]
		protected virtual void Segment_DimensionID_CacheAttached(PXCache sender)
		{
		}

		[PXDBShort(IsKey = true)]
		[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Segment.segmentID, Where<Segment.dimensionID, Equal<Current<Segment.dimensionID>>>>))]
		[PXParent(typeof(Select<Dimension, Where<Dimension.dimensionID, Equal<Current<Segment.dimensionID>>>>))]
		protected virtual void Segment_SegmentID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void Segment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as Segment;
			if (row != null)
			{
				Values.Cache.AllowInsert = row.AutoNumber != true;
				Values.Cache.AllowUpdate = row.AutoNumber != true;
				Values.Cache.AllowDelete = row.AutoNumber != true;

				_groupColumnsCache.TryInjectColumns(Values.Cache, row.DimensionID);
			}
			else
			{
				Values.Cache.AllowInsert = true;
				Values.Cache.AllowUpdate = true;
				Values.Cache.AllowDelete = true;
			}
		}

		protected virtual void SegmentValue_Value_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null || cache.HasAttributes(e.Row))
			{
				Segment seg = Segment.Current as Segment;
				short i;

				if (seg != null && seg.SegmentID != null)
				{
					StringBuilder bld = new StringBuilder();

					if (seg.AutoNumber != true)
					{
						bld.Append((seg.CaseConvert == 1) ? ">" : "");
						bld.Append((seg.CaseConvert == 2) ? "<" : "");
					}

					for (i = 0; i < seg.Length; i++)
					{
						if (seg.AutoNumber == true)
						{
							bld.Append("C");
						}
						else
						{
							bld.Append(seg.EditMask);
						}
					}

					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, "Value", null, null, bld.ToString(), null, null, null, null);
				}
			}
		}

		protected virtual void SegmentValue_IsConsolidatedValue_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SegmentValue currow = e.Row as SegmentValue;
			if ((bool)currow.IsConsolidatedValue)
			{
				foreach (SegmentValue segrow in Values.Select(currow.DimensionID, currow.SegmentID))
				{
					if ((bool)segrow.IsConsolidatedValue && segrow.Value != currow.Value)
					{
						segrow.IsConsolidatedValue = (bool)false;
						Values.Cache.Update(segrow);
					}
				}
			}

			Values.View.RequestRefresh();
		}

		protected virtual void SegmentValue_IsConsolidatedValue_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
			{
				Segment seg = Segment.Current;
				if (seg != null)
				{
					bool isSubitem = seg.DimensionID == IN.SubItemAttribute.DimensionName;
					PXUIFieldAttribute.SetVisible<SegmentValue.isConsolidatedValue>(cache, null, isSubitem);
					PXUIFieldAttribute.SetEnabled<SegmentValue.isConsolidatedValue>(cache, null, isSubitem);
				}
				else
					PXUIFieldAttribute.SetVisible<SegmentValue.isConsolidatedValue>(cache, null, false);
			}
		}

		protected virtual void SegmentValue_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			SegmentValue segValue = (SegmentValue)e.Row;
			if (segValue != null && segValue.SegmentID.HasValue && string.IsNullOrEmpty(segValue.Value) == false)
			{
				if (IsSegmentValueExist(segValue))
				{
					if (segValue.IsConsolidatedValue != true)
					{
						cache.RaiseExceptionHandling<SegmentValue.value>(e.Row, segValue.Value, new PXException(Messages.SegmentValueExistsType));
						e.Cancel = true;
					}
					else
					{
						throw new PXException(Messages.SegmentValueExistsType);
					}
				}
			}
		}

		protected virtual void SegmentValue_MappedSegValue_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
			{
				Segment seg = Segment.Current;
				if (seg != null)
				{
					if (seg.DimensionID == SubAccountAttribute.DimensionName)
						PXUIFieldAttribute.SetVisible<SegmentValue.mappedSegValue>(cache, null, true);
					else
						PXUIFieldAttribute.SetVisible<SegmentValue.mappedSegValue>(cache, null, false);
				}
				else
					PXUIFieldAttribute.SetVisible<SegmentValue.mappedSegValue>(cache, null, false);
			}
		}

		protected virtual void SegmentValue_MappedSegValue_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!String.IsNullOrEmpty((string)e.NewValue)
				&& Segment.Current.ConsolNumChar > 0
				&& e.NewValue.ToString().Length > Segment.Current.ConsolNumChar)
			{
				throw new PXSetPropertyException(Messages.MappedSegValueLength, Segment.Current.ConsolNumChar);
			}
		}

		private bool IsSegmentValueExist(SegmentValue segValue)
		{
			PXSelectBase select = new PXSelect<SegmentValue,
				Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
				And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>,
				And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>(this);
			var result = select.View.SelectSingle(segValue.DimensionID, segValue.SegmentID, segValue.Value);
			return (result != null);
		}
	}
}