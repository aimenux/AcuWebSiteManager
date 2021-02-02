using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Text;
using PX.Objects.GL;
using System.Text.RegularExpressions;
using PX.Data.SQLTree;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.Objects.CT;
using PX.Objects.AR;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CS
{
	public class DimensionMaint : PXGraph<DimensionMaint, Dimension>
	{
		private static readonly HashSet<string> TreeCompatibleDimensions =
			new HashSet<String>
			{
				INItemClass.Dimension,
				PM.PMCostCode.costCodeCD.DimensionName
			};

		#region Selects
		public PXSelect<Dimension, Where<Dimension.dimensionID, InFieldClassActivated, Or<Dimension.dimensionID, IsNull>>> Header;

		public PXSelect<Segment,
			Where<Segment.dimensionID, Equal<Current<Dimension.dimensionID>>>>
			Detail;

		public PXSelect<SegmentValue> Values;
		#endregion

		#region Data Handlers

		protected virtual IEnumerable detail()
		{
			if (Header.Current == null) return new Segment[0];
			return Header.Current.ParentDimensionID != null
				? GetComplexDetails(Header.Current)
				: GetSimpleDetails(Header.Current);
		}

		protected virtual IEnumerable GetDetails(string dimId)
		{
			Dimension dim;
			if (string.IsNullOrEmpty(dimId) ||
				(dim = (Dimension)PXSelect<Dimension>.Search<Dimension.dimensionID>(this, dimId)) == null)
			{
				return new Segment[0];
			}
			return dim.ParentDimensionID != null ? GetComplexDetails(dim) : GetSimpleDetails(dim);
		}

		private IEnumerable GetComplexDetails(Dimension dim)
		{
			var hashtable = new Hashtable();
			foreach (Segment row in PXSelect<Segment,
					Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>
					.Select(this, dim.ParentDimensionID))
			{
				var item = (Segment)Detail.Cache.CreateCopy(row);
				var key = (short)item.SegmentID;
				item.ParentDimensionID = dim.ParentDimensionID;
				item.Inherited = true;
				item.DimensionID = dim.DimensionID;
				hashtable.Add(key, item);
			}
			foreach (Segment item in PXSelect<Segment,
				 Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>.
				 Select(this, dim.DimensionID))
			{
				if (item.ParentDimensionID != null && hashtable.ContainsKey(item.SegmentID))
				{
					hashtable.Remove(item.SegmentID);
					yield return item;
				}
			}
			foreach (Segment item in hashtable.Values)
			{
				if (Detail.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
				{
					Detail.Cache.SetStatus(item, PXEntryStatus.Inserted);
				}
				yield return item;
			}
		}

		private IEnumerable GetSimpleDetails(Dimension dim)
		{
			foreach (Segment item in
				PXSelect<Segment,
					Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>, And<Segment.dimensionID, InFieldClassActivated>>>
				.Select(this, dim.DimensionID))
			{
				yield return item;
			}
		}

		#endregion

		#region Actions
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable cancel(PXAdapter a)
		{
			foreach (Dimension e in (new PXCancel<Dimension>(this, "Cancel")).Press(a))
			{
				if (Header.Cache.GetStatus(e) == PXEntryStatus.Inserted)
				{
					Dimension e1 = PXSelectReadonly<Dimension,
						Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
						.Select(this, e.DimensionID);
					if (e1 != null)
					{
						Header.Cache.RaiseExceptionHandling<Dimension.dimensionID>(e, e.DimensionID,
							new PXSetPropertyException(Messages.FieldClassRestricted));
					}
				}
				yield return e;
			}
		}

		public PXAction<Dimension> ViewSegment;

		[PXButton]
		[PXUIField(DisplayName = Messages.ViewSegment)]
		protected virtual IEnumerable viewSegment(PXAdapter adapter)
		{
			var current = Detail.Current;
			if (current != null)
			{
				Segment row;
				if (current.Inherited == true)
				{
					row = (Segment)PXSelectReadonly2<Segment,
					InnerJoin<Dimension, On<Dimension.parentDimensionID, Equal<Segment.dimensionID>>>,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>,
						And<Segment.segmentID, Equal<Required<Segment.segmentID>>>>>.
						Select(this, current.DimensionID, current.SegmentID);
				}
				else
				{
					row = (Segment)PXSelectReadonly<Segment>.
						Search<Segment.dimensionID, Segment.segmentID>(this, current.DimensionID, current.SegmentID);
				}
				PXRedirectHelper.TryRedirect(Caches[typeof(Segment)], row, string.Empty);
			}
			return adapter.Get();
		}

		public override void Persist()
		{
			if (Header.Cache.IsDirty && Detail.Select().Count == 0 && Header.Current != null)
			{
				throw new PXException(Messages.DimensionIsEmpty);
			}

			var dimension = Header.Current;
			if (dimension == null)
			{
				base.Persist();
			}
			else
			{
				try
				{
					using (PXTransactionScope tscope = new PXTransactionScope())
					{
						if (Header.Cache.GetStatus(dimension) != PXEntryStatus.Deleted)
						{
							InsertNumberingValue(dimension);

							DimensionUpdate graph = PXGraph.CreateInstance<DimensionUpdate>();
							Dictionary<short?, SegmentChanges> updatedSegments = GetChangesOfSegments();

							foreach (ISegmentKeysModifier query in GetSegmentKeysModifiers()
																		.Where(resizer => resizer.Key == dimension.DimensionID)
																		.Select(resizer => resizer.Value))
							{
								query.UpdateSegment(graph, updatedSegments, dimension);
							}

							CorrectChildDimensions();
						}
						PXDimensionAttribute.Clear();
						base.Persist();
						Header.Current = dimension;
						PXDimensionAttribute.Clear();

						tscope.Complete();
					}
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.DeleteForeignKeyConstraintViolation)
						throw new PXException(Messages.SegmentHasValues, e.Keys[1]);
					throw;
				}
			}
			PXPageCacheUtils.InvalidateCachedPages();
		}


		[PXProjection(typeof(Select4<INSubItemDup, Aggregate<GroupBy<INSubItemDup.subItemCD>>>))]
		[PXHidden]
		public class INSubItemDup : INSubItem
		{
			#region SubItemCD
			public new abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
			//[PXDBString(30, IsUnicode = true)]
			[PXDBCalced(typeof(Left<INSubItem.subItemCD, CurrentValue<Dimension.segments>>), typeof(string))]
			public override string SubItemCD
			{
				get;
				set;
			}
			#endregion
		}

		public class NoSort : IBqlSortColumn
		{
			public Type GetReferencedType()
			{
				return null;
			}
			public bool IsDescending
			{
				get
				{
					return false;
				}
			}

			public void AppendQuery(Query query, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			{
				info.SortColumns?.Add(this);
			}

			public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			{
				info.SortColumns?.Add(this);
				return true;
			}

			public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
			}
		}
		#endregion

		#region Dimenstion Event Handlers

		protected virtual void Dimension_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			Dimension newRow = (Dimension)e.NewRow;
			Dimension currow = (Dimension)Header.Current;
			int maxLength = PXDimensionAttribute.GetMaxLength(currow.DimensionID);
			if (newRow.Length > maxLength)
				cache.RaiseExceptionHandling<Dimension.length>(e.Row, newRow.Length,
					new PXSetPropertyException<Dimension.length>(Messages.DimensionLengthOutOfRange));
		}

		protected virtual void Dimension_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			Dimension dim = (Dimension)e.Row;
			dim.MaxLength = PXDimensionAttribute.GetMaxLength(dim.DimensionID);

			PXUIFieldAttribute.SetEnabled<Dimension.length>(cache, dim, false);
			PXUIFieldAttribute.SetVisible<Dimension.maxLength>(cache, dim, HasMaxLength(dim));
			PXUIFieldAttribute.SetEnabled<Dimension.segments>(cache, dim, false);

			Boolean isTreeCompatible = TreeCompatibleDimensions.Contains(dim.DimensionID);
			var allowed = cache
				.GetAttributesReadonly<Dimension.lookupMode>()
				.OfType<PXStringListAttribute>()
				.First()
				.ValueLabelDic
				.Where(kvp => kvp.Key != DimensionLookupMode.BySegmentsAndChildSegmentValues || isTreeCompatible)
				.UnZip((values, labels) => new { Values = values.ToArray(), Labels = labels.ToArray() });
			PXStringListAttribute.SetLocalizable<Dimension.lookupMode>(cache, null, false);
			PXStringListAttribute.SetList<Dimension.lookupMode>(cache, e.Row, allowed.Values, allowed.Labels);

			bool numberingIDEnabled = true;
			bool specificModule = true;
			bool lookupModeEnabled = false;

			Detail.Cache.AllowInsert = true;
			Detail.Cache.AllowUpdate = true;
			Detail.Cache.AllowDelete = true;

			switch (dim.DimensionID)
			{
				case SubItemAttribute.DimensionName:
					lookupModeEnabled = true;
					PXUIFieldAttribute.SetEnabled<Segment.autoNumber>(Detail.Cache, null, false);
					break;

				case AccountAttribute.DimensionName:
					break;

				case AR.CustomerAttribute.DimensionName:
				case AR.SalesPersonAttribute.DimensionName:
				case IN.InventoryAttribute.DimensionName:
				case IN.SiteAttribute.DimensionName:
				case IN.LocationAttribute.DimensionName:
				case AP.VendorAttribute.DimensionName:
				case AR.CustomerRawAttribute.DimensionName:
				case EP.EmployeeRawAttribute.DimensionName:
				case CR.LeadRawAttribute.DimensionName:
					lookupModeEnabled = dim.Validate == false;
					PXUIFieldAttribute.SetEnabled<Segment.autoNumber>(Detail.Cache, null, true);
					PXUIFieldAttribute.SetVisible<Segment.isCosted>(Detail.Cache, null, false);
					break;

				default:
					lookupModeEnabled = true;
					break;
			}

			bool validateEnabled = lookupModeEnabled && dim.LookupMode == DimensionLookupMode.BySegmentsAndChildSegmentValues;

			cache.AllowDelete = dim.Internal != true;

			PXUIFieldAttribute.SetVisible<Segment.consolOrder>(Detail.Cache, null, dim.DimensionID == SubAccountAttribute.DimensionName);
			PXUIFieldAttribute.SetVisible<Segment.consolNumChar>(Detail.Cache, null, dim.DimensionID == SubAccountAttribute.DimensionName);

			PXUIFieldAttribute.SetVisible<Segment.isCosted>(Detail.Cache, null, dim.DimensionID == SubItemAttribute.DimensionName);

			bool hasParent = dim.ParentDimensionID != null;
			Detail.Cache.AllowInsert &= !hasParent;
			PXUIFieldAttribute.SetVisible<Segment.inherited>(Detail.Cache, null, hasParent);
			PXUIFieldAttribute.SetVisible<Segment.isOverrideForUI>(Detail.Cache, null, hasParent);

			PXUIFieldAttribute.SetEnabled<Dimension.numberingID>(cache, e.Row, numberingIDEnabled);
			PXUIFieldAttribute.SetEnabled<Dimension.specificModule>(cache, e.Row, specificModule);
			PXUIFieldAttribute.SetEnabled<Dimension.validate>(cache, e.Row, validateEnabled);
			PXUIFieldAttribute.SetEnabled<Dimension.lookupMode>(cache, e.Row, lookupModeEnabled);
		}

		protected virtual void Dimension_Validate_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnValue != null)
			{
				e.ReturnValue = !((bool)e.ReturnValue);
			}
		}

		protected virtual void Dimension_Validate_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			PXBoolAttribute.ConvertValue(e);
			if (e.NewValue != null)
			{
				e.NewValue = !((bool)e.NewValue);
			}
		}

		protected virtual void Dimension_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CheckLength(cache, e.Row);
		}

		#endregion

		#region Segment Event Handlers

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Dimension.dimensionID))]
		[PXUIField(DisplayName = "Dimension ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXSelector(typeof(Dimension.dimensionID), DirtyRead = true)]
		protected virtual void Segment_DimensionID_CacheAttached(PXCache sender)
		{

		}

		[PXDBShort(IsKey = true)]
		[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.Visible, Enabled = false)]
		protected virtual void Segment_SegmentID_CacheAttached(PXCache sender)
		{

		}

		protected virtual void Segment_SegmentID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Segment currow = e.Row as Segment;
			if (currow == null) return;

			short maxsegid = 0;

			foreach (Segment segrow in GetDetails(currow.DimensionID))
			{
				if (segrow.SegmentID > maxsegid)
				{
					maxsegid = (short)segrow.SegmentID;
				}
			}
			e.NewValue = ++maxsegid;
			currow.ConsolOrder = maxsegid;
			e.Cancel = true;

			PXUIFieldAttribute.SetEnabled<Segment.segmentID>(cache, currow, false);
		}

		protected virtual void Segment_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Segment seg = (Segment)e.Row;
			if (seg == null) return;

			e.Cancel = seg.ParentDimensionID != null && seg.Inherited == true &&
					   cache.GetStatus(seg) == PXEntryStatus.Inserted;
		}

		protected virtual void Segment_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			UpdateHeader(e.Row);
			CheckLength(Header.Cache, Header.Current);
		}

		protected virtual void Segment_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Segment seg = (Segment)e.Row;
			if (seg == null) return;

			if (seg.ParentDimensionID != null) seg.Inherited = false;

			UpdateHeader(e.Row);
			CheckLength(Header.Cache, Header.Current);
		}

		protected virtual void Segment_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			UpdateHeader(e.Row);
			CheckLength(Header.Cache, Header.Current);
		}

		protected virtual void Segment_DimensionID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void Segment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Segment row = e.Row as Segment;
			if (row == null)
				return;

			if (row.ParentDimensionID != null)
			{
				Segment parent = row;
				if (row.Inherited != true)
				{
					parent = PXSelectReadonly<Segment, Where<Segment.dimensionID, Equal<Current<Segment.parentDimensionID>>,
														And<Segment.segmentID, Equal<Current<Segment.segmentID>>>>>.SelectSingleBound(this, new object[] { row });
				}
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				PXUIFieldAttribute.SetEnabled<Segment.descr>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<Segment.editMask>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<Segment.autoNumber>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<Segment.validate>(sender, row, parent != null && parent.Validate != true);
			}
		}

		protected virtual void Segment_Length_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Segment currow = e.Row as Segment;
			if (currow == null) return;
			currow.ConsolNumChar = currow.Length;
		}

		protected virtual void Segment_Length_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((short)e.NewValue <= 0)
				throw new PXSetPropertyException(Messages.SegmentLengthLessThenZero);
		}

		protected virtual void Segment_AutoNumber_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Segment currow = e.Row as Segment;

			if (currow.Validate != null && (bool)currow.Validate && currow.AutoNumber != (bool)e.NewValue && (bool)e.NewValue)
			{
				currow.Validate = false;
				cache.Update(currow);
			}
		}

		protected virtual void Segment_Validate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Segment currow = e.Row as Segment;

			if (currow.AutoNumber != null && (bool)currow.AutoNumber && currow.Validate != (bool)e.NewValue && (bool)e.NewValue)
			{
				currow.AutoNumber = false;
				cache.Update(currow);
			}
		}

		protected virtual void Segment_ConsolNumChar_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((short)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.FieldShouldNotBeNegative,
					PXUIFieldAttribute.GetDisplayName<Segment.consolNumChar>(cache));
			}
		}

		protected virtual void Segment_ConsolOrder_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var segment = (Segment)e.Row;
			var existingSegment = (Segment)PXSelect<Segment,
											Where<Segment.dimensionID, Equal<Current<Dimension.dimensionID>>,
													And<Segment.segmentID, NotEqual<Required<Segment.segmentID>>,
													And<Segment.consolOrder, Equal<Required<Segment.consolOrder>>>>>>
											.Select(this, segment.SegmentID, e.NewValue);

			if (existingSegment != null)
			{
				throw new PXSetPropertyException(Messages.TheSegmentWithConsolOrderValueEqualToAlreadyExists,
					existingSegment.SegmentID, e.NewValue);
			}
		}

		protected virtual void _(Events.FieldVerifying<Segment, Segment.isCosted> e) => CheckSubItems(e.Row);

		protected virtual void Segment_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (Segment)e.Row;
			var dimensionID = row.DimensionID;
			var segmentID = row.SegmentID;

			if (Header.Current.ParentDimensionID != null && row.Inherited == true)
			{
				throw new PXException(Messages.SegmentNotOverridden, segmentID);
			}
			if (PXSelectReadonly<Segment, Where<Segment.parentDimensionID, Equal<Current<Segment.dimensionID>>,
											And<Segment.segmentID, Equal<Current<Segment.segmentID>>>>>.SelectMultiBound(this, new object[] { row }).Count > 0)
			{
				throw new PXException(Messages.SegmentHasChilds, segmentID);
			}
			Segment lastSegmeent = PXSelect<Segment, Where<Segment.dimensionID, Equal<Current<Segment.dimensionID>>>,
											OrderBy<Desc<Segment.segmentID>>>.SelectSingleBound(this, new object[] { row });
			if (lastSegmeent != null && lastSegmeent.SegmentID > segmentID)
			{
				throw new PXException(Messages.SegmentIsNotLast, segmentID);
			}
			if (((SegmentValue)PXSelect<SegmentValue,
				Where<SegmentValue.dimensionID, Equal<Optional<Segment.dimensionID>>,
					And<SegmentValue.segmentID, Equal<Optional<Segment.segmentID>>>>>.
				Select(this, dimensionID, segmentID)) != null)
			{
				if (row.ParentDimensionID == null) throw new PXException(Messages.SegmentHasValues, segmentID);

				var answer = Header.Ask(Messages.Warning,
					PXMessages.LocalizeFormatNoPrefixNLA(Messages.SegmentHasValuesQuestion, segmentID),
					MessageButtons.YesNoCancel,
					MessageIcon.Warning);
				switch (answer)
				{
					case WebDialogResult.Yes:
						break;
					case WebDialogResult.Cancel:
						e.Cancel = true;
						break;
					case WebDialogResult.No:
					default:
						throw new PXException(Messages.SegmentHasValues, segmentID);
				}
			}
		}

		protected virtual void Segment_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			var row = (Segment)e.Row;
			var newRow = ((Segment)e.NewRow);

			CheckSegmentValidateFieldIfNeeds(newRow);

			if (newRow.AutoNumber == false && (row.Length != newRow.Length || row.EditMask != newRow.EditMask))
			{
				foreach (SegmentValue val in PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Optional<Segment.dimensionID>>, And<SegmentValue.segmentID, Equal<Optional<Segment.segmentID>>>>>.Select(this, row.DimensionID, row.SegmentID))
				{
					if (newRow.Length != val.Value.Length)
					{
						e.Cancel = true;
						cache.RaiseExceptionHandling<Segment.length>(e.NewRow, newRow.Length, new PXSetPropertyException(Messages.SegmentHasValuesFailedUpdate, row.SegmentID));
						return;
					}

					bool matchMask = true;
					switch (newRow.EditMask[0])
					{
						case 'C':
							break;
						case 'a':
							if (val.Value.Any(x => !(Char.IsLetterOrDigit(x) || Char.IsWhiteSpace(x))))
								matchMask = false;
							break;
						case '9':
							if (val.Value.Any(x => !(Char.IsDigit(x) || Char.IsWhiteSpace(x))))
								matchMask = false;
							break;
						case '?':
							if (val.Value.Any(x => !(Char.IsLetter(x) || Char.IsWhiteSpace(x))))
								matchMask = false;
							break;
					}

					if (!matchMask)
					{
						e.Cancel = true;
						cache.RaiseExceptionHandling<Segment.editMask>(e.NewRow, newRow.EditMask, new PXSetPropertyException(Messages.SegmentHasValuesFailedUpdate, row.SegmentID));
						return;
					}
				}
			}

			PXResultset<Segment> overridingSegments = PXSelect<Segment, Where<Segment.parentDimensionID, Equal<Current<Segment.dimensionID>>,
											And<Segment.segmentID, Equal<Current<Segment.segmentID>>>>>.SelectMultiBound(this, new object[] { row });

			//bool editMask = false;
			bool separator = false;
			bool align = false;
			bool caseConvert = false;
			if (/*(editMask = row.EditMask != newRow.EditMask) ||*/
				(separator = row.Separator != newRow.Separator) || (align = row.Align != newRow.Align) ||
				(caseConvert = row.CaseConvert != newRow.CaseConvert))
			{
				if (overridingSegments != null && overridingSegments.Count > 0)
				{
					e.Cancel = true;

					string overridingSegmentsString = string.Join(", ",
						overridingSegments
							.RowCast<Segment>()
							.Distinct(x => x.DimensionID)
							.Select(x => string.Format("'{0}'", x.DimensionID)));

					var error = new PXSetPropertyException(Messages.SegmentHasChildsFailedUpdate, row.SegmentID, overridingSegmentsString);

					if (separator)
					{
						cache.RaiseExceptionHandling<Segment.separator>(e.NewRow, newRow.Separator, error);
					}
					else if (align)
					{
						cache.RaiseExceptionHandling<Segment.align>(e.NewRow, newRow.Align, error);
					}
					else
					{
						cache.RaiseExceptionHandling<Segment.caseConvert>(e.NewRow, newRow.CaseConvert, error);
					}

					return;
				}
			}
			else if(row.Validate != newRow.Validate)
			{
				// Get child dimension IDs, where has no overriden segments and dimension feature is enabled.
				var inheritedDimensions = 
					SelectFrom<Dimension>
						.LeftJoin<Segment>.On<Dimension.dimensionID.IsEqual<Segment.dimensionID>>
					.Where
						<Dimension.dimensionID.Is<InFieldClassActivated>.And
						<Dimension.parentDimensionID.IsEqual<Dimension.dimensionID.FromCurrent>.And
						<Segment.parentDimensionID.IsNull>>>
					.View
					.Select(this)
					.RowCast<Dimension>()
					.Distinct(dimension => dimension.DimensionID)
					.Select(dimension => $"'{dimension.DimensionID}'")
					.ToArray();					

				if (inheritedDimensions.Length > 0)
				{
					string concatenatedSegments = string.Join(", ", inheritedDimensions);
					var warning = new PXSetPropertyException(Messages.SegmentHasChildsWarningUpdate, 
						PXErrorLevel.Warning, 
						row.SegmentID, 
						concatenatedSegments);

					cache.RaiseExceptionHandling<Segment.validate>(e.NewRow, newRow.Validate, warning);
				}
			}

			if (row.Length != newRow.Length && overridingSegments != null)
			{
				foreach (Segment child in overridingSegments)
				{
					Segment copy = (Segment)cache.CreateCopy(child);
					copy.Length = newRow.Length;
					copy = Detail.Update(copy);
					if (copy == null)
					{
						e.Cancel = true;
						cache.RaiseExceptionHandling<Segment.length>(e.NewRow, newRow.Length, new PXSetPropertyException(Messages.SegmentHasValuesFailedUpdate, row.SegmentID));
						return;
					}
				}
			}
		}

		#endregion

		#region UpdateSegmentsKeys
		public class SegmentChanges
		{
			public SegmentChanges(Segment oldValue, Segment newValue)
			{
				OldValue = oldValue;
				NewValue = newValue;
			}
			public SegmentChanges(Segment oldValue)
			{
				OldValue = oldValue;
			}
			public Segment OldValue { get; }
			public Segment NewValue { get; set; }
		}

		public interface ISegmentKeysModifier
		{
			void UpdateSegment(PXGraph graph, Dictionary<short?, SegmentChanges> updatedSegments, Dimension header);
		}

		private class SegmentKeysModifier<TTable, TFieldId, TFieldCd> : SegmentKeysModifier<TTable, TFieldId, TFieldCd, Where<True, Equal<True>>>
						   where TTable : class, IBqlTable, new()
						   where TFieldId : IBqlField
						   where TFieldCd : IBqlField
		{

		}

		private class SegmentKeysModifier<TTable, TFieldId, TFieldCd, TWhere> : ISegmentKeysModifier
				where TTable : class, IBqlTable, new()
				where TFieldId : IBqlField
				where TFieldCd : IBqlField
				where TWhere : IBqlWhere, new()
		{
			public virtual void UpdateSegment(PXGraph graph, Dictionary<short?, SegmentChanges> updatedSegments, Dimension header)
			{
				var updateKeys = new Dictionary<int?, string>();
				var cantUpdateKeys = new List<string>();
				const int countRecordsToShow = 10;

				foreach (TTable segmentKey in PXSelectReadonly<TTable>.Select(graph))
				{
					string segmentKeyOld = (string)graph.Caches<TTable>().GetValue(segmentKey, typeof(TFieldCd).Name);
					StringBuilder sbSegmentKeyNew = new StringBuilder();
					int subSegmentsPosition = 0;
					bool cantUpdateKey = false;

					if (segmentKeyOld == null) continue;

					foreach (SegmentChanges segmentChanges in updatedSegments.Values)
					{
						int oldSegmentLenght = segmentKeyOld.Length < (int)segmentChanges.OldValue.Length ? segmentKeyOld.Length : (int)segmentChanges.OldValue.Length;
						int newSegmentLenght = (int)(segmentChanges.NewValue?.Length ?? 0);
						string subSegmentKey = segmentKeyOld.Substring(subSegmentsPosition, oldSegmentLenght);

						if (!string.IsNullOrWhiteSpace(subSegmentKey) && segmentChanges?.NewValue !=null && !MatchMask(segmentChanges.NewValue.EditMask[0], subSegmentKey))
						{
							cantUpdateKey = true;
							throw new PXException(Messages.EditMaskCantBeChangedThereAreTableIdentifiersToWhichNewEditMaskCannotBeApplied, graph.Caches<TTable>().DisplayName, segmentKeyOld);
						}

						if (oldSegmentLenght > newSegmentLenght)
						{
							subSegmentKey = subSegmentKey.TrimEnd();
							if (subSegmentKey.Length > newSegmentLenght)
							{
								cantUpdateKey = true;
								break;
							}
						}

						subSegmentKey = subSegmentKey.PadRight((int)(segmentChanges.NewValue?.Length ?? 0));
						subSegmentsPosition += oldSegmentLenght;
						sbSegmentKeyNew.Append(subSegmentKey);
					}

					string segmentKeyNew = sbSegmentKeyNew.ToString();
					if (cantUpdateKey)
					{
						cantUpdateKeys.Add($"\"{segmentKeyOld}\"");
						if (cantUpdateKeys.Count == countRecordsToShow)
						{
							break;
						}
					}
					else if (segmentKeyOld != segmentKeyNew)
					{
						updateKeys.Add((int?)graph.Caches<TTable>().GetValue(segmentKey, typeof(TFieldId).Name), segmentKeyNew);
					}
				}

				if (cantUpdateKeys.Any())
				{
					throw new PXException(Messages.ThereAreSubaccountIdentifiersWhoseLengthIsGreaterThanLengthOfSegmentedKey, graph.Caches<TTable>().DisplayName, string.Join(", ", cantUpdateKeys.ToArray()));
				}
				else
				{
					foreach (KeyValuePair<int?, string> keyCd in updateKeys)
					{
						PXUpdate<Set<TFieldCd, Required<TFieldCd>>, TTable,
							Where<TFieldId, Equal<Required<TFieldId>>,
							And<TWhere>>>.Update(graph, keyCd.Value, keyCd.Key);
					}
				}
			}

			protected bool MatchMask(char editMask, String value)
			{
				if (value == null)
					return false;

				value = value.TrimEnd();

				if (value.Length == 0)
					return false;

				switch (editMask)
				{
					case 'C':
						break;
					case 'a':
						if (value.Any(x => !(Char.IsLetterOrDigit(x) || Char.IsWhiteSpace(x))))
							return false;
						break;
					case '9':
						if (value.Any(x => !(Char.IsDigit(x) || Char.IsWhiteSpace(x))))
							return false;
						break;
					case '?':
						if (value.Any(x => !(Char.IsLetter(x) || Char.IsWhiteSpace(x))))
							return false;
						break;
				}

				return true;
			}
		}

		private class SegmentKeysModifierLight<TTable, TFieldId, TFieldCd> : SegmentKeysModifier<TTable, TFieldId, TFieldCd>
			where TTable : class, IBqlTable, new()
			where TFieldId : IBqlField
			where TFieldCd : IBqlField
		{
			public override void UpdateSegment(PXGraph graph, Dictionary<short?, SegmentChanges> updatedSegments, Dimension header)
			{
				var updateKeys = new Dictionary<int?, string>();
				var cantUpdateKeys = new List<string>();
				const int countRecordsToShow = 10;

				foreach (TTable segmentKey in PXSelectReadonly<TTable>.Select(graph))
				{
					string segmentKeyOld = (string)graph.Caches<TTable>().GetValue(segmentKey, typeof(TFieldCd).Name);

					if (segmentKeyOld == null) continue;

					segmentKeyOld = segmentKeyOld.Trim();
					int newLength = header.Length ?? 0;

					if (segmentKeyOld.Length > newLength)
					{
						cantUpdateKeys.Add($"\"{segmentKeyOld}\"");
						if (cantUpdateKeys.Count == countRecordsToShow)
						{
							break;
						}
					}
					else
					{
						string segmentKeyNew = segmentKeyOld.PadRight(newLength);
						int subSegmentsPosition = 0;

						foreach (SegmentChanges segmentChanges in updatedSegments.Values)
						{
							if (segmentChanges.NewValue == null) continue;

							int newSegmentLenght = (int)segmentChanges.NewValue.Length;
							string subSegmentKey = segmentKeyNew.Substring(subSegmentsPosition, newSegmentLenght);

							if (!string.IsNullOrWhiteSpace(subSegmentKey) && !MatchMask(segmentChanges.NewValue.EditMask[0], subSegmentKey))
							{
								throw new PXException(Messages.EditMaskCantBeChangedThereAreTableIdentifiersToWhichNewEditMaskCannotBeApplied, graph.Caches<TTable>().DisplayName, segmentKeyOld);
							}

							subSegmentsPosition += newSegmentLenght;
						}

						int? segmentID = (int?)graph.Caches<TTable>().GetValue(segmentKey, typeof(TFieldId).Name);
						if (segmentID != null)
						{
							updateKeys.Add(segmentID, segmentKeyNew);
						}
					}
				}

				if (cantUpdateKeys.Any())
				{
					throw new PXException(Messages.ThereAreSubaccountIdentifiersWhoseLengthIsGreaterThanLengthOfSegmentedKey, graph.Caches<TTable>().DisplayName, string.Join(", ", cantUpdateKeys.ToArray()));
				}
				else
				{
					foreach (KeyValuePair<int?, string> keyCd in updateKeys)
					{
						PXUpdate<Set<TFieldCd, Required<TFieldCd>>, TTable,
							Where<TFieldId, Equal<Required<TFieldId>>>>.Update(graph, keyCd.Value, keyCd.Key);
					}
				}
			}
		}

		protected virtual List<KeyValuePair<string, ISegmentKeysModifier>> GetSegmentKeysModifiers()
		{
			return SegmentKeysModifiers;
		}

		private static readonly List<KeyValuePair<string, ISegmentKeysModifier>> SegmentKeysModifiers = new List<KeyValuePair<string, ISegmentKeysModifier>>
		{
			new KeyValuePair<string, ISegmentKeysModifier>("ACCGROUP",
				new SegmentKeysModifier<PMAccountGroup, PMAccountGroup.groupID, PMAccountGroup.groupCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("ACCOUNT",
				new SegmentKeysModifier<Account, Account.accountID, Account.accountCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("BIZACCT",
				new SegmentKeysModifier<BAccount, BAccount.bAccountID, BAccount.acctCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("BIZACCT",
				new SegmentKeysModifier<Branch, Branch.branchID, Branch.branchCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("CASHACCOUNT",
				new SegmentKeysModifier<CashAccount, CashAccount.cashAccountID, CashAccount.cashAccountCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("CONTRACTITEM",
				new SegmentKeysModifier<ContractItem, ContractItem.contractItemID, ContractItem.contractItemCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("INLOCATION",
				new SegmentKeysModifier<INLocation, INLocation.locationID, INLocation.locationCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("INSITE",
				new SegmentKeysModifier<INSite, INSite.siteID, INSite.siteCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("INSUBITEM",
				new SegmentKeysModifier<INSubItem, INSubItem.subItemID, INSubItem.subItemCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("INVENTORY",
				new SegmentKeysModifier<InventoryItem, InventoryItem.inventoryID, InventoryItem.inventoryCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("MLISTCD",
				new SegmentKeysModifier<CRMarketingList, CRMarketingList.marketingListID, CRMarketingList.mailListCode>()),
			new KeyValuePair<string, ISegmentKeysModifier>("LOCATION",
				new SegmentKeysModifier<Location, Location.locationID, Location.locationCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("PROJECT",
				new SegmentKeysModifier<Contract, Contract.contractID, Contract.contractCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("PROTASK",
				new SegmentKeysModifier<PMTask, PMTask.taskID, PMTask.taskCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("SALESPER",
				new SegmentKeysModifier<SalesPerson, SalesPerson.salesPersonID, SalesPerson.salesPersonCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("SUBACCOUNT",
				new SegmentKeysModifier<Sub, Sub.subID, Sub.subCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>(INItemClass.Dimension,
				new SegmentKeysModifierLight<INItemClass, INItemClass.itemClassID, INItemClass.itemClassCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("COSTCODE",
				new SegmentKeysModifier<PMCostCode, PMCostCode.costCodeID, PMCostCode.costCodeCD>()),
			new KeyValuePair<string, ISegmentKeysModifier>("TMPROJECT",
				new SegmentKeysModifier<PMProject, PMProject.contractID, PMProject.contractCD, Where<PMProject.baseType, Equal<CTPRType.projectTemplate>>>())
		};

		private Dictionary<short?, SegmentChanges> GetChangesOfSegments()
		{
			var updatedSegments = new Dictionary<short?, SegmentChanges>();

			foreach (Segment oldSegment in PXSelectReadonly<Segment,
															Where<Segment.dimensionID, Equal<Required<Dimension.dimensionID>>>,
															OrderBy<Asc<Segment.segmentID>>>.Select(this, Header.Current.DimensionID))
			{
				if (oldSegment.SegmentID == null)
				{
					throw new ArgumentNullException(nameof(oldSegment.SegmentID));
				}

				updatedSegments.Add(oldSegment.SegmentID, new SegmentChanges(oldSegment));
			}

			foreach (Segment newSegment in Detail.Select())
			{
				if (newSegment.SegmentID == null)
				{
					throw new ArgumentNullException(nameof(newSegment.SegmentID));
				}
				else
				{
					SegmentChanges segmentChanges;
					if (updatedSegments.TryGetValue(newSegment.SegmentID, out segmentChanges))
					{
						segmentChanges.NewValue = newSegment;
					}
				}
			}
			return updatedSegments;
		}
		#endregion

		#region Private Methods

		private void CorrectChildDimensions()
		{
			var header = Header.Current;
			if (header != null)
			{
				var oldNumbering = header.DimensionID.
					With(id => (Dimension)PXSelectReadonly<Dimension>.
						Search<Dimension.dimensionID>(this, id)).
					With(dim => dim.NumberingID);
				foreach (Dimension item in
					PXSelect<Dimension, Where<Dimension.parentDimensionID, Equal<Required<Dimension.parentDimensionID>>>>.
						Select(this, header.DimensionID))
				{
					item.Length = header.Length;
					item.Segments = header.Segments;

					if (string.IsNullOrEmpty(item.NumberingID)) item.NumberingID = header.NumberingID;
					var numbErrors = new List<Exception>();
					var oldNumbID = item.NumberingID;
					if (oldNumbID == oldNumbering)
					{
						item.NumberingID = header.NumberingID;
						CheckLength(Header.Cache, item, numbErrors);
						if (numbErrors.Count > 0)
						{
							numbErrors.Clear();
							item.NumberingID = oldNumbID;
						}
						else oldNumbID = item.NumberingID;
					}
					CheckLength(Header.Cache, item, numbErrors);
					if (numbErrors.Count > 0 && oldNumbID != header.NumberingID)
					{
						var oldNumbErrorsCount = numbErrors.Count;
						item.NumberingID = header.NumberingID;
						CheckLength(Header.Cache, item, numbErrors);
						if (numbErrors.Count == oldNumbErrorsCount) numbErrors.Clear();
						else item.NumberingID = oldNumbID;
					}
					if (numbErrors.Count > 0)
					{
						var sb = new StringBuilder();
						foreach (var error in numbErrors)
							sb.AppendLine(error.Message);
						throw new Exception(sb.ToString());
					}

					Header.Cache.Update(item);

					InsertNumberingValue(item);
				}
			}
		}

		private void InsertNumberingValue(Dimension dimension)
		{
			foreach (Segment segrow in GetDetails(dimension.DimensionID))
			{
				if (segrow.Inherited == true) continue;
				if (segrow.AutoNumber == true)
				{
					segrow.EditMask = "C";
					segrow.Validate = false;
					//segrow.CaseConvert = 0;
					segrow.FillCharacter = " ";
					Detail.Update(segrow);

					Numbering numb = PXSelect<Numbering,
										Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.
									 Select(this, dimension.NumberingID);
					bool numvalueexists = false;
					if (numb == null)
					{
						throw new PXException(Messages.NumberingIDRequired);
					}

					var valuesCache = Caches[typeof(SegmentValue)];
					foreach (SegmentValue val in
						PXSelect<SegmentValue,
						Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
							And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>>>>.
						Select(this, segrow.DimensionID, segrow.SegmentID))
					{
						if (!object.Equals(val.Value, numb.NewSymbol))
						{
							valuesCache.Delete(val);
						}
						else
						{
							numvalueexists = true;
						}
					}

					if (!numvalueexists)
					{
						SegmentValue val = new SegmentValue();
						val.DimensionID = segrow.DimensionID;
						val.SegmentID = segrow.SegmentID;
						val.Value = numb.NewSymbol;
						valuesCache.Insert(val);
					}
				}
			}
		}

		private void UpdateHeader(object row)
		{
			//
			short segcnt = 0;
			short seglen = 0;

			Segment currow = row as Segment;
			bool IsRefreshNeeded = false;

			if ((bool)currow.AutoNumber)
			{
				foreach (Segment segrow in GetDetails(currow.DimensionID))
				{
					if (segrow.Inherited == true) continue;
					if ((bool)segrow.AutoNumber && segrow.SegmentID != currow.SegmentID)
					{
						segrow.AutoNumber = (bool)false;
						Detail.Cache.Update(segrow);
						IsRefreshNeeded = true;
					}
				}
			}

			foreach (Segment segrow in GetDetails(currow.DimensionID))
			{
				if (segrow.Inherited != true && currow.DimensionID == IN.SubItemAttribute.DimensionName)
				{
					if ((bool)((Segment)row).IsCosted)
					{
						if (segrow.SegmentID < ((Segment)row).SegmentID)
						{
							segrow.IsCosted = true;
							Detail.Cache.Update(segrow);
							IsRefreshNeeded = true;
						}
					}
					else
					{
						if (segrow.SegmentID > ((Segment)row).SegmentID)
						{
							segrow.IsCosted = false;
							Detail.Cache.Update(segrow);
							IsRefreshNeeded = true;
						}
					}
				}

				segcnt++;
				seglen += (short)segrow.Length;
			}

			Dimension dim = Header.Current as Dimension;
			dim.Segments = segcnt;
			dim.Length = seglen;
			Header.Cache.Update(dim);

			if (IsRefreshNeeded)
			{
				Detail.View.RequestRefresh();
			}
		}

		private void CheckLength(PXCache cache, object row)
		{
			CheckLength(cache, row, null);
		}

		private bool HasMaxLength(Dimension row)
		{
			return row.MaxLength != null && row.MaxLength != PXDimensionAttribute.NoMaxLength;
		}
		private void CheckLength(PXCache cache, object row, ICollection<Exception> errors)
		{
			Dimension currow = row as Dimension;

			// Max length checking
			if (HasMaxLength(currow) && currow.Length > currow.MaxLength)
				Header.Cache.RaiseExceptionHandling<Dimension.length>(currow, currow.Length,
					new PXSetPropertyException<Dimension.length>(Messages.DimensionLengthOutOfRange));

			short seglen = 0;

			foreach (Segment segrow in GetDetails(currow.DimensionID))
			{
				seglen += (short)segrow.Length;

				if ((bool)segrow.AutoNumber)
				{
					if (currow.NumberingID == null)
					{
						if (errors == null)
						{
							cache.RaiseExceptionHandling<Dimension.numberingID>(currow, currow.NumberingID,
								new PXSetPropertyException(Messages.NumberingIDRequired));
						}
						else
						{
							errors.Add(new PXSetPropertyException(Messages.NumberingIDRequiredCustom, currow.DimensionID));
						}
					}
					else
					{
						foreach (NumberingSequence num in PXSelect<NumberingSequence, Where<NumberingSequence.numberingID, Equal<Required<Dimension.numberingID>>>>.Select(this, currow.NumberingID))
						{
							if (num.StartNbr.Length != segrow.Length && (!(currow.Length == seglen && num.StartNbr.Length < segrow.Length)))
							{
								if (errors == null)
								{
									cache.RaiseExceptionHandling<Dimension.numberingID>(currow, currow.NumberingID,
										new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegment + Messages.EnsureSegmentLength, segrow.SegmentID.ToString()));
								}
								else
								{
									errors.Add(new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegmentCustom + Messages.EnsureSegmentLength,
										currow.DimensionID.ToString(), currow.NumberingID.ToString(), segrow.SegmentID.ToString()));
								}
							}

							string mask = Regex.Replace(Regex.Replace(num.StartNbr, "[0-9]", "9"), "[^0-9]", "?");
							if (segrow.EditMask == "?" && mask.Contains("9") || segrow.EditMask == "9" && mask.Contains("?"))
							{
								if (errors == null)
								{
									cache.RaiseExceptionHandling<Dimension.numberingID>(currow, currow.NumberingID,
										new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegment + Messages.EnsureSegmentMask, segrow.SegmentID.ToString()));
								}
								else
								{
									errors.Add(new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegmentCustom + Messages.EnsureSegmentMask,
										currow.DimensionID.ToString(), currow.NumberingID.ToString(), segrow.SegmentID.ToString()));
								}
							}
						}
					}
				}
			}
		}

		private void CheckSegmentValidateFieldIfNeeds(Segment segment)
		{
			if (segment.DimensionID == SubAccountAttribute.DimensionName)
			{
				if (segment.ConsolNumChar > 0 && segment.Length != segment.ConsolNumChar && segment.Validate != true)
				{
					throw new PXSetPropertyException(Messages.SelectSubaccountSegmentValidateCheckBox);
				}
			}
		}

		private void CheckSubItems(Segment currentSegment)
		{
			if (currentSegment == null || currentSegment.DimensionID != SubItemAttribute.DimensionName)
				return;

			var cost = (INCostStatus)PXSelect<INCostStatus, Where<INCostStatus.qtyOnHand, NotEqual<decimal0>>>.Select(this);

			if (cost != null)
				throw new PXSetPropertyException(Messages.SegmentSubItemIncludeInCostError);
		}

		#endregion

		#region public helpers

		public static bool IsAutonumbered(PXGraph graph, string dimensionID, bool fully = true)
		{
			Dimension dm = PXSelect<Dimension, Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>.Select(graph, dimensionID);

			if (dm.NumberingID == null)
				return false;

			Segment[] segments = PXSelect<Segment, 
										Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>
										.Select(graph, dm.DimensionID)
										.RowCast<Segment>()
										.ToArray();

			if (dm.ParentDimensionID != null)
			{
				Segment[] inheritedSegments = PXSelect<Segment,
													Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>,
														And<Segment.segmentID, NotIn<Required<Segment.segmentID>>>>>
													.Select(graph,
														dm.ParentDimensionID,
														segments.Select(s => s.SegmentID).ToArray())
													.RowCast<Segment>()
													.ToArray();

				segments = segments.Union(inheritedSegments).ToArray();
			}

			bool res = fully ? true : false;

			foreach (Segment segment in segments)
			{
				if (segment.AutoNumber == true)
				{
					if (!fully)
					{
						res = true;
						break;
					}
				}
				else if (fully)
				{
					res = false;
					break;
				}
			}

			return res;
		}

		#endregion
	}
	public class DimensionUpdate : PXGraph<DimensionUpdate>
	{

	}
}