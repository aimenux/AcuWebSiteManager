using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CS;
using System;
using System.Collections;

namespace PX.Objects.IN.RelatedItems
{
	/// <summary>
	/// The extension provide an ability to edit related items for the current <see cref="InventoryItem"/>.
	/// The extension is active only if <see cref="FeaturesSet.commerceIntegration"/> feature is enabled.
	/// </summary>
	/// <typeparam name="TGraph"></typeparam>
	public class RelatedItemsTab<TGraph> : PXGraphExtension<TGraph>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
		where TGraph : PXGraph
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.commerceIntegration>();

		[PXImport(typeof(INRelatedInventory))]
		public SelectFrom<INRelatedInventory>
			.Where<INRelatedInventory.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>
			.OrderBy<INRelatedInventory.relation.Asc, INRelatedInventory.rank.Asc, INRelatedInventory.inventoryID.Asc>.View RelatedItems;

		#region Actions

		public PXAction<InventoryItem> viewRelatedItem;
		[PXUIField(DisplayName = "View Related Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewRelatedItem(PXAdapter adapter)
		{
			var relatedInventory = InventoryItem.PK.Find(Base, RelatedItems.Current?.RelatedInventoryID);

			if (relatedInventory != null)
				PXRedirectHelper.TryRedirect(Base.Caches[typeof(InventoryItem)], relatedInventory, "View Related Item", PXRedirectHelper.WindowMode.NewWindow);

			return adapter.Get();
		}

		#endregion

		#region event handlers

		protected virtual void _(Events.FieldUpdated<INRelatedInventory, INRelatedInventory.relation> e)
		{
			if (e.Row == null)
				return;
			e.Cache.SetValue<INRelatedInventory.rank>(e.Row, 0);
			e.Cache.SetDefaultExt<INRelatedInventory.rank>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<INRelatedInventory, INRelatedInventory.isActive> e)
		{
			if (e.Row == null)
				return;
			if ((bool?)e.NewValue == true && e.OldValue != e.NewValue)
			{
				e.Cache.SetValue<INRelatedInventory.expirationDate>(e.Row, null);
				e.Cache.SetDefaultExt<INRelatedInventory.effectiveDate>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<INRelatedInventory, INRelatedInventory.effectiveDate> e)
		{
			if (e.Row == null)
				return;
			var startDate = (DateTime?)e.NewValue;

			if (!IsCorrectActivePeriod((DateTime?)e.NewValue, e.Row.ExpirationDate))
				e.Cache.SetValue<INRelatedInventory.expirationDate>(e.Row, null);

			if (startDate != null && startDate > Base.Accessinfo.BusinessDate)
				e.Cache.SetValue<INRelatedInventory.isActive>(e.Row, false);
		}

		protected virtual void _(Events.FieldUpdated<INRelatedInventory, INRelatedInventory.expirationDate> e)
		{
			if (e.Row == null)
				return;
			var endDate = (DateTime?)e.NewValue;

			if (!IsCorrectActivePeriod(endDate, e.Row.ExpirationDate) 
				|| (endDate != null && endDate < Base.Accessinfo.BusinessDate))
				e.Cache.SetValue<INRelatedInventory.isActive>(e.Row, false);
		}

		protected virtual bool IsCorrectActivePeriod(DateTime? startTime, DateTime? endTime) => endTime == null || startTime == null || startTime <= endTime;

		#endregion

		#region Fields defaulting

		protected virtual void _(Events.FieldDefaulting<INRelatedInventory, INRelatedInventory.rank> e)
		{
			var relation = e.Row?.Relation;
			if (relation == null)
				return;
			INRelatedInventory maxRankedRelation = new SelectFrom<INRelatedInventory>
				.Where<INRelatedInventory.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>
				.And<INRelatedInventory.relation.IsEqual<@P.AsString>>>
				.OrderBy<INRelatedInventory.rank.Desc>.View(Base).SelectWindowed(0, 1, relation);
			e.NewValue = (maxRankedRelation?.Rank ?? 0) + 1;
		}

		#endregion

		#region Find of duplicates

		protected virtual Type[] GetAlternativeKeyFields()
		{
			return new[]
			{
				typeof(INRelatedInventory.relatedInventoryID),
				typeof(INRelatedInventory.relation)
			};
		}

		private DuplicatesSearchEngine<INRelatedInventory> _duplicateFinder;

		private bool DontUpdateExistRecords
		{
			get
			{
				object dontUpdateExistRecords;
				return Base.IsImportFromExcel && PXExecutionContext.Current.Bag.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS, out dontUpdateExistRecords) &&
					true.Equals(dontUpdateExistRecords);
			}
		}

		#endregion

		#region PXImportAttribute.IPXPrepareItems and PXImportAttribute.IPXProcess implementations

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName.Equals(nameof(RelatedItems), StringComparison.InvariantCultureIgnoreCase) && !DontUpdateExistRecords)
			{
				if (_duplicateFinder == null)
				{
					var items = RelatedItems.SelectMain();
					_duplicateFinder = new DuplicatesSearchEngine<INRelatedInventory>(RelatedItems.Cache, GetAlternativeKeyFields(), items);
				}
				var duplicate = _duplicateFinder.Find(values);
				if (duplicate != null)
				{
					if (keys.Contains(nameof(INRelatedInventory.LineID)))
						keys[nameof(INRelatedInventory.LineID)] = duplicate.LineID;
					else
						keys.Add(nameof(INRelatedInventory.LineID), duplicate.LineID);
				}
			}
			return true;
		}

		public virtual bool RowImporting(string viewName, object row) => row == null;
		public virtual bool RowImported(string viewName, object row, object oldRow) => oldRow == null;
		public virtual void PrepareItems(string viewName, IEnumerable items) { }
		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			_duplicateFinder = null;
		}
		#endregion
	}
}
