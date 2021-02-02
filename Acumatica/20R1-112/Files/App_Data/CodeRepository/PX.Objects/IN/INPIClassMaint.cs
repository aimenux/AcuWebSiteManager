using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
	#region PI Generator Settings
	[Serializable]
	public partial class INPILocationFilter : IBqlTable
	{
		#region StartLocationID
		[Location(typeof(INPIClass.siteID), DisplayName = "Start Location ID")]
		public virtual Int32? StartLocationID
		{
			get => _StartLocationID;
			set => _StartLocationID = value;
		}
		public abstract class startLocationID : BqlInt.Field<startLocationID> { }
		protected Int32? _StartLocationID;
		#endregion
		#region EndLocationID
		[Location(typeof(INPIClass.siteID), DisplayName = "End Location ID")]
		public virtual Int32? EndLocationID
		{
			get => _EndLocationID;
			set => _EndLocationID = value;
		}
		public abstract class endLocationID : BqlInt.Field<endLocationID> { }
		protected Int32? _EndLocationID;
		#endregion
	}

	[Serializable]
	public partial class INPIInventoryFilter : IBqlTable
	{
		#region StartInventoryID
		[StockItem(DisplayName = "Start Inventory ID")]
		public virtual Int32? StartInventoryID
		{
			get => _StartInventoryID;
			set => _StartInventoryID = value;
		}
		public abstract class startInventoryID : BqlInt.Field<startInventoryID> { }
		protected Int32? _StartInventoryID;
		#endregion
		#region EndInventoryID
		[StockItem(DisplayName = "End Inventory ID")]
		public virtual Int32? EndInventoryID
		{
			get => _EndInventoryID;
			set => _EndInventoryID = value;
		}
		public abstract class endInventoryID : BqlInt.Field<endInventoryID> { }
		protected Int32? _EndInventoryID;
		#endregion
	}
	#endregion

	public class INPIClassMaint : PXGraph<INPIClassMaint, INPIClass>
	{
		#region Views
		public
			PXSelect<INPIClass>
			Classes;

		public
			PXSelect<INPIClass,
			Where<INPIClass.pIClassID, Equal<Current<INPIClass.pIClassID>>>>
			CurrentClass;

		public
			PXSelectJoin<INPIClassItem,
			LeftJoin<InventoryItem, On<INPIClassItem.FK.InventoryItem>>,
			Where<INPIClassItem.pIClassID, Equal<Current<INPIClass.pIClassID>>>>
			Items;

		public
			PXSelectJoin<INPIClassItemClass,
			LeftJoin<INItemClass, On<INPIClassItemClass.FK.ItemClass>>,
			Where<INPIClassItemClass.pIClassID, Equal<Current<INPIClass.pIClassID>>>>
			ItemClasses;

		public
			PXSelectJoin<INPIClassLocation,
			LeftJoin<INLocation, On<INPIClassLocation.FK.Location>>,
			Where<INPIClassLocation.pIClassID, Equal<Current<INPIClass.pIClassID>>>>
			Locations;

		public PXFilter<INPILocationFilter> LocationFilter;
		public PXFilter<INPIInventoryFilter> InventoryFilter;
		#endregion

		#region Event Handlers
		protected virtual void INPIClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INPIClass row = (INPIClass)e.Row;
			if (row == null)
				return;

			PXUIFieldAttribute.SetEnabled<INPIClass.includeZeroItems>(sender, row, AllowIncludeZeroItems(row));

			PXUIFieldAttribute.SetVisible<INPIClass.lastCountPeriod>(sender, row, row.SelectedMethod == PIInventoryMethod.LastCountDate);
			PXUIFieldAttribute.SetVisible<INPIClass.randomItemsLimit>(sender, row, row.SelectedMethod == PIInventoryMethod.RandomlySelectedItems);

			PXUIFieldAttribute.SetEnabled<INPIClass.aBCCodeID>(sender, row, row.ByFrequency != true);
			PXUIFieldAttribute.SetEnabled<INPIClass.movementClassID>(sender, row, row.ByFrequency != true);
			PXUIFieldAttribute.SetEnabled<INPIClass.cycleID>(sender, row, row.ByFrequency != true);

			Items.Cache.AllowInsert =
			Items.Cache.AllowUpdate =
			Items.Cache.AllowDelete =
				row.SelectedMethod == PIInventoryMethod.ListOfItems;
			AddItem.SetEnabled(row.SelectedMethod == PIInventoryMethod.ListOfItems);

			Locations.Cache.AllowInsert =
			Locations.Cache.AllowUpdate =
			Locations.Cache.AllowDelete =
				row.SiteID != null;

			PXUIFieldAttribute.SetWarning<INPIClass.unlockSiteOnCountingFinish>(sender, row, row.UnlockSiteOnCountingFinish == true
				? Messages.PITypeEarlyInventoryUnfreezeWarning
				: null);
		}

		protected virtual void INPIClass_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (PXResult<INPIClassLocation> r in Locations.View.SelectMultiBound(new object[] { e.Row }))
				Locations.Delete(r);
		}

		protected virtual void INPIClass_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INPIClass row = (INPIClass)e.Row;
			INPIClass oldrow = (INPIClass)e.OldRow;
			if (row != null && oldrow != null)
			{
				if (row.ByFrequency == true && oldrow.ByFrequency != true)
				{
					row.ABCCodeID = null;
					row.MovementClassID = null;
					row.CycleID = null;
				}

				if (!PIMethod.IsByFrequencyAllowed(row.Method))
				{
					row.ByFrequency = false;
				}
			}

			if (!AllowIncludeZeroItems(row))
			{
				row.IncludeZeroItems = false;
			}
		}
		#endregion

		#region Buttons
		public PXAction<INPIClass> AddLocation;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(Tooltip = Messages.AddNewLine, ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		protected virtual IEnumerable addLocation(PXAdapter e)
		{
			if (LocationFilter.AskExt((graph, viewName) => graph.Views[viewName].Cache.Clear(), true) == WebDialogResult.OK)
			{
				var source = LocationFilter.Current.EndLocationID == null
					?	SelectFrom<INLocation>.
						Where<
							INLocation.siteID.IsEqual<INPIClass.siteID.FromCurrent>.
							And<INLocation.locationCD.IsGreaterEqual<@P.AsString>>>.
						View.ReadOnly
						.Select(this,
								INLocation.PK.Find(this, LocationFilter.Current.StartLocationID)?.LocationCD)
					:	SelectFrom<INLocation>.
						Where<
							INLocation.siteID.IsEqual<INPIClass.siteID.FromCurrent>.
							And<INLocation.locationCD.IsBetween<@P.AsString, @P.AsString>>>.
						View.ReadOnly
						.Select(this,
								INLocation.PK.Find(this, LocationFilter.Current.StartLocationID)?.LocationCD,
								INLocation.PK.Find(this, LocationFilter.Current.EndLocationID)?.LocationCD);

				HashSet<int?> existingLocations =
					SelectFrom<INPIClassLocation>.
					Where<INPIClassLocation.pIClassID.IsEqual<INPIClass.pIClassID.FromCurrent>>.
					View
					.Select(this)
					.Select(l => ((INPIClassLocation)l).LocationID)
					.Where(l => l.HasValue)
					.ToHashSet();

				foreach (INLocation l in source.RowCast<INLocation>().Where(s => !existingLocations.Contains(s.LocationID)))
				{
					INPIClassLocation item = (INPIClassLocation)Locations.Cache.CreateInstance();
					item.LocationID = l.LocationID;
					Locations.Insert(item);
				}
			}
			return e.Get();
		}

		public PXAction<INPIClass> AddItem;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(Tooltip = Messages.AddNewLine, ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		protected virtual IEnumerable addItem(PXAdapter e)
		{
			if (InventoryFilter.AskExt((graph, viewName) => graph.Views[viewName].Cache.Clear(), true) == WebDialogResult.OK)
			{
				var source = InventoryFilter.Current.EndInventoryID == null
					?	SelectFrom<InventoryItem>.
						Where<
							InventoryItem.stkItem.IsEqual<True>.
							And<InventoryItem.itemStatus.IsNotIn<InventoryItemStatus.inactive, InventoryItemStatus.markedForDeletion>>.
							And<InventoryItem.inventoryCD.IsGreaterEqual<@P.AsString>>>.
						View.ReadOnly
						.Select(this,
								InventoryItem.PK.Find(this, InventoryFilter.Current.StartInventoryID)?.InventoryCD)
					:	SelectFrom<InventoryItem>.
						Where<
							InventoryItem.stkItem.IsEqual<True>.
							And<InventoryItem.itemStatus.IsNotIn<InventoryItemStatus.inactive, InventoryItemStatus.markedForDeletion>>.
							And<InventoryItem.inventoryCD.IsBetween<@P.AsString, @P.AsString>>>.
						View.ReadOnly
						.Select(this,
								InventoryItem.PK.Find(this, InventoryFilter.Current.StartInventoryID)?.InventoryCD,
								InventoryItem.PK.Find(this, InventoryFilter.Current.EndInventoryID)?.InventoryCD);

				foreach (InventoryItem l in source)
				{
					INPIClassItem item = (INPIClassItem)Items.Cache.CreateInstance();
					if (Items.Locate(item) == null)
					{
						item.InventoryID = l.InventoryID;
						Items.Insert(item);
					}
				}
			}
			return e.Get();
		}
		#endregion

		protected virtual bool AllowIncludeZeroItems(INPIClass row)
		{
			return !(row?.Method == PIMethod.FullPhysicalInventory ||
				row?.Method == PIMethod.ByInventoryItemSelected && row?.SelectedMethod == PIInventoryMethod.ItemsHavingNegativeBookQty);
		}
	}

	public static class PIMethod
	{
		public const string FullPhysicalInventory = "F";
		public const string ByInventoryItemSelected = "I";
		public const string ByMovementClass = "M";
		public const string ByABCClass = "A";
		public const string ByCycle = "Y";
		public const string ByItemClassID = "C";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				Pair(FullPhysicalInventory, Messages.FullPhysicalInventory),
				Pair(ByInventoryItemSelected, Messages.ByInventory),
				Pair(ByMovementClass, Messages.ByMovementClass),
				Pair(ByABCClass, Messages.ByABCClass),
				Pair(ByCycle, Messages.ByCycleID),
				Pair(ByItemClassID, Messages.ByItemClassID))
			{ }
		}

		public class fullPhysicalInventory : PX.Data.BQL.BqlString.Constant<fullPhysicalInventory>
		{
			public fullPhysicalInventory() : base(FullPhysicalInventory) {}
		}

		public static bool IsByFrequencyAllowed(string generationMethod) => generationMethod.IsIn(ByMovementClass, ByABCClass, ByCycle);
	}

	public static class PIInventoryMethod
	{
		/*
		public const string LastCountDate = "D";
		public const string ByPreviousPIID = "P";
		*/
		public const string ItemsHavingNegativeBookQty = "N";
		public const string RandomlySelectedItems = "R";
		public const string ListOfItems = "L";
		public const string LastCountDate = "I";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				Pair(ItemsHavingNegativeBookQty, Messages.ItemsHavingNegativeBookQty),
				Pair(RandomlySelectedItems, Messages.RandomlySelectedItems),
				Pair(ListOfItems, Messages.ListOfItems),
				Pair(LastCountDate, Messages.LastCountDate))
			{ }
		}
	}
}