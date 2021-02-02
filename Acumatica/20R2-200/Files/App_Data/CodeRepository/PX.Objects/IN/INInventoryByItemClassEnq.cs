using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Common.Extensions;
using PX.Data;
using PX.Objects.CS;
using PX.Web.UI;

namespace PX.Objects.IN
{
	public class INInventoryByItemClassEnq : PXGraph<INInventoryByItemClassEnq>
	{
		public static class ShowItemsMode
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new[] { ChildrenOfCurrentClass, AllChildren },
					new[] { "Related to Only the Current Item Class", "Related to the Current and Child Item Classes" })
				{ }
			}
			public const string ChildrenOfCurrentClass = "C";
			public const string AllChildren = "A";
		}

		#region DAC
		[Serializable]
		public class InventoryByClassFilter : IBqlTable
		{
			#region InventoryID
			[Inventory]
			public virtual int? InventoryID { get; set; }
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region ShowItems
			[PXString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Show Items")]
			[ShowItemsMode.List]
			[PXDefault(ShowItemsMode.ChildrenOfCurrentClass)]
			public virtual String ShowItems { get; set; }
			public abstract class showItems : PX.Data.BQL.BqlString.Field<showItems> { }
			#endregion
		}

		[Serializable]
		public class INItemClassFilter : IBqlTable
		{
			#region ItemClassID
			public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
			[PXInt]
			[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<False>, Or<Where<INItemClass.stkItem, Equal<True>, And<FeatureInstalled<FeaturesSet.distributionModule>>>>>>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
			public virtual int? ItemClassID { get; set; }
			#endregion
		}

		[Serializable, PXHidden]
		public class ItemBuffer : IBqlTable
		{
			#region InventoryID
			[PXInt(IsKey = true), PXDefault]
			public  Int32? InventoryID { get; set; }
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
		}
		#endregion

		#region State
		private bool _allowToSyncTreeCurrentWithPrimaryViewCurrent;
		private bool _forbidToSyncTreeCurrentWithPrimaryViewCurrent;
		private bool _keepInventoryFilter;
		private readonly Lazy<bool> _timestampSelected = new Lazy<bool>(() => { PXDatabase.SelectTimeStamp(); return true; });
		#endregion

		#region Selects
		public PXSelectReadonly<INItemClass> ItemClassFilter;
		public PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Current<INItemClass.itemClassID>>>> TreeViewAndPrimaryViewSynchronizationHelper;
		public PXFilter<InventoryByClassFilter> InventoryFilter;
		public PXFilter<ItemBuffer> CutBuffer;

		public PXSelectReadonly<ItemClassTree.INItemClass> ItemClasses;
		protected virtual IEnumerable itemClasses([PXInt] int? itemClassID) => ItemClassTree.EnrollNodes(itemClassID);

		public PXSelectJoin<InventoryItem, InnerJoin<INItemClass, On<InventoryItem.FK.ItemClass>>> Inventories;
		protected virtual IEnumerable inventories() => GetRelevantInventories(InventoryFilter.Current.ShowItems);
		#endregion

		#region Actions
		public PXFirst<INItemClass> First;
		public PXPrevious<INItemClass> Previous;
		public PXNext<INItemClass> Next;
		public PXLast<INItemClass> Last;
		public PXSave<INItemClass> Save;
		public PXCancel<INItemClass> Cancel;

		public PXAction<INItemClass> Cut;
		[PXButton(ImageKey = Sprite.Main.Cut, Tooltip = Messages.ttipCutSelectedRecords)]
		[PXUIField(DisplayName = ActionsMessages.Cut, Enabled = false)]
		internal IEnumerable cut(PXAdapter adapter) => PerformCut(adapter);

		public PXAction<INItemClass> Paste;
		[PXButton(ImageKey = Sprite.Main.Paste, Tooltip = Messages.ttipPasteRecords)]
		[PXUIField(DisplayName = ActionsMessages.Paste, Enabled = false)]
		internal IEnumerable paste(PXAdapter adapter) => PerformPaste(adapter);
		
		public PXAction<INItemClass> GoToNodeSelectedInTree;
		[PXButton, PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable goToNodeSelectedInTree(PXAdapter adapter)
		{
			_forbidToSyncTreeCurrentWithPrimaryViewCurrent = true;
			ItemClassFilter.Current = PXSelect<INItemClass>.Search<INItemClass.itemClassID>(this, ItemClasses.Current?.ItemClassID);
			yield return ItemClassFilter.Current;
		}
		#endregion

		#region Event Handlers
		/// <summary><see cref="InventoryItem.BaseItemWeight"/> CacheAttached</summary>
		[PXDBQuantity, PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void InventoryItem_BaseItemWeight_CacheAttached(PXCache sender) { }

		/// <summary><see cref="InventoryItem.BaseItemVolume"/> CacheAttached</summary>
		[PXDBQuantity, PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void InventoryItem_BaseItemVolume_CacheAttached(PXCache sender) { }

		/// <summary><see cref="INItemClass"/> Selected</summary>
		protected virtual void INItemClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			_timestampSelected.Init();
			SyncTreeCurrentWithPrimaryViewCurrent((INItemClass) e.Row);
		}

		/// <summary><see cref="INItemClass"/> Updating</summary>
		protected virtual void INItemClass_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e) => e.Cancel = true;

		/// <summary><see cref="InventoryByClassFilter.InventoryID"/> Updated</summary>
		protected virtual void InventoryByClassFilter_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (InventoryByClassFilter)e.Row;
			if (row?.InventoryID != null)
			{
				INItemClass current = PXSelectReadonly2<INItemClass,
					InnerJoin<InventoryItem, On<InventoryItem.FK.ItemClass>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
					.SelectWindowed(this, 0, 1, row.InventoryID);
				_allowToSyncTreeCurrentWithPrimaryViewCurrent = true;
				_forbidToSyncTreeCurrentWithPrimaryViewCurrent = false;
				_keepInventoryFilter = true;
				ItemClassFilter.Current = current;
			}
		}
		#endregion

		public override IEnumerable ExecuteSelect(String viewName, Object[] parameters, Object[] searches, String[] sortcolumns, Boolean[] descendings, PXFilterRow[] filters, ref Int32 startRow, Int32 maximumRows, ref Int32 totalRows)
		{
			if (viewName == nameof(TreeViewAndPrimaryViewSynchronizationHelper))
				_allowToSyncTreeCurrentWithPrimaryViewCurrent = true;
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		private void SyncTreeCurrentWithPrimaryViewCurrent(INItemClass primaryViewCurrent)
		{
			if (_allowToSyncTreeCurrentWithPrimaryViewCurrent && !_forbidToSyncTreeCurrentWithPrimaryViewCurrent
				&& primaryViewCurrent != null && (ItemClasses.Current == null || ItemClasses.Current.ItemClassID != primaryViewCurrent.ItemClassID))
			{
				ItemClassTree.INItemClass current = ItemClassTree.Instance.GetNodeByID(primaryViewCurrent.ItemClassID ?? 0);
				ItemClasses.Current = current;
				ItemClasses.Cache.ActiveRow = current;
			}

			if (_keepInventoryFilter == false)
				InventoryFilter.Cache.SetValueExt<InventoryByClassFilter.inventoryID>(InventoryFilter.Current, null);
		}

		private IEnumerable GetRelevantInventories(string showItemsMode)
		{
			if (ItemClasses.Current == null) yield break;

			var selectCommand = new PXSelectJoin<InventoryItem,
				InnerJoin<INItemClass, On<InventoryItem.FK.ItemClass>>,
				Where2<
					Where<InventoryItem.inventoryID, Equal<Current<InventoryByClassFilter.inventoryID>>, Or<Current<InventoryByClassFilter.inventoryID>, IsNull>>, 
					And<Match<Current<AccessInfo.userName>>>>>(this);

			if (showItemsMode == ShowItemsMode.ChildrenOfCurrentClass)
			{
				selectCommand.WhereAnd<Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>>>();
				foreach (var res in selectCommand.Select(ItemClasses.Current.ItemClassID))
				{
					yield return res;
				}
			}
			else if (showItemsMode == ShowItemsMode.AllChildren)
			{
				selectCommand.WhereAnd<Where<INItemClass.itemClassCD, Like<Required<INItemClass.itemClassCD>>>>();
				foreach (var res in selectCommand.Select(ItemClasses.Current.ItemClassCDWildcard))
				{
					yield return res;
				}
			}
			else throw new PXInvalidOperationException();
		}

		private IEnumerable PerformCut(PXAdapter adapter)
		{
			CutBuffer.Cache.Clear();
			bool allAreStocks = true;
			bool allAreNonStocks = true;
			foreach (InventoryItem inventory in GetRelevantInventories(InventoryFilter.Current.ShowItems).RowCast<InventoryItem>().Where(i => i.Selected == true))
			{
				var insertnode = (ItemBuffer)CutBuffer.Cache.CreateInstance();
				insertnode.InventoryID = inventory.InventoryID;
				CutBuffer.Cache.Insert(insertnode);

				if (inventory.StkItem == true)
					allAreNonStocks = false;
				else
					allAreStocks = false;
			}

			if (CutBuffer.Cache.Cached.Count() != 0 && allAreStocks == false && allAreNonStocks == false)
				throw new PXInvalidOperationException(Messages.DifferentItemsCouldNotBeMovedToItemClass);

			return adapter.Get();
		}

		private IEnumerable PerformPaste(PXAdapter adapter)
		{
			var buffer = CutBuffer.Cache.Cached.Cast<ItemBuffer>().ToArray();
			if (buffer.Any() == false) return adapter.Get();

			int newItemClassID = (int)ItemClasses.Current.ItemClassID;
			object[] inventoryIDs = buffer.Select(b => b.InventoryID).Cast<object>().ToArray();
			var inventories = 
				PXSelectReadonly<InventoryItem, 
				Where<InventoryItem.inventoryID, In<Required<InventoryItem.inventoryID>>>>
				.Select(this, new[] {inventoryIDs}).RowCast<InventoryItem>().ToArray();
			var notMatchItems = inventories.Where(i => i.StkItem != ItemClasses.Current.StkItem).ToArray();
			if (notMatchItems.Any())
			{
				String segmentedItemClassID = ItemClasses.Current.SegmentedClassCD.FirstSegment(' ');
				if (notMatchItems.Length == 1)
				{
					throw new PXInvalidOperationException(
						Messages.ItemClassAndInventoryItemStkItemShouldBeSameSingleItem,
						notMatchItems[0].InventoryCD.TrimEnd(),
						segmentedItemClassID);
				}
				else
				{
					PXTrace.WriteInformation(
						notMatchItems.Aggregate(
							new StringBuilder().AppendLine(PXMessages.LocalizeFormatNoPrefix(Messages.CouldNotBeMovedToItemClassItemsList, segmentedItemClassID)),
							(sb, item) => sb.AppendLine(item.InventoryCD.TrimEnd()),
							sb => sb.ToString()));

					throw new PXInvalidOperationException(
						Messages.ItemClassAndInventoryItemStkItemShouldBeSameManyItems,
						segmentedItemClassID);
				}
			}

			bool needToDefault = Inventories.Ask(AR.Messages.Warning, Messages.ItemClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.Yes;

			Lazy<InventoryItemMaint> stockItemMaint = new Lazy<InventoryItemMaint>(CreateInstance<InventoryItemMaint>);
			Lazy<NonStockItemMaint> nonStockItemMaint = new Lazy<NonStockItemMaint>(CreateInstance<NonStockItemMaint>);
			foreach (InventoryItem inventory in inventories)
			{
				if (needToDefault)
				{
					InventoryItemMaintBase inventoryItemMaint = inventory.StkItem == true ? (InventoryItemMaintBase)stockItemMaint.Value : nonStockItemMaint.Value;

					inventoryItemMaint.Item.Current = inventory;
					using (inventoryItemMaint.MakeRuleWeakeningScopeFor<InventoryItem.lotSerClassID>(RuleWeakenLevel.SuppressError))
					using (inventoryItemMaint.MakeRuleWeakeningScopeFor<InventoryItem.baseUnit>(RuleWeakenLevel.SuppressError))
					using (stockItemMaint.Value.MakeRuleWeakeningScopeFor<InventoryItem.decimalBaseUnit>(RuleWeakenLevel.SuppressError))
					{
						var copy = (InventoryItem)inventoryItemMaint.Item.Cache.CreateCopy(inventory);
						copy.ItemClassID = newItemClassID;
						inventoryItemMaint.Item.Update(copy);
					}
					inventoryItemMaint.Actions.PressSave();
				}
				else
				{
					inventory.ItemClassID = newItemClassID;
					Inventories.Cache.Update(inventory);
				}
				Inventories.SetValueExt<InventoryItem.selected>(Inventories.Locate(inventory), false);
			}

			CutBuffer.Cache.Clear();

			if (needToDefault)
				Actions.PressCancel();
			else
				Actions.PressSave();

			return adapter.Get();
		}
	}
}