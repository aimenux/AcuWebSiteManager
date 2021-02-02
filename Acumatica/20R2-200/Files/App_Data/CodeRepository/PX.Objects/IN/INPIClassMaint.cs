using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	#region PI Generator Settings
    [Serializable]
	public partial class INPILocationFilter : IBqlTable
	{
		#region StartLocationID
		public abstract class startLocationID : PX.Data.BQL.BqlInt.Field<startLocationID> { }
		protected Int32? _StartLocationID;
		[Location(typeof(INPIClass.siteID), DisplayName = "Start Location ID")]
		public virtual Int32? StartLocationID
		{
			get
			{
				return this._StartLocationID;
			}
			set
			{
				this._StartLocationID = value;
			}
		}
		#endregion  
		#region EndLocationID
		public abstract class endLocationID : PX.Data.BQL.BqlInt.Field<endLocationID> { }
		protected Int32? _EndLocationID;
		[Location(typeof(INPIClass.siteID), DisplayName = "End Location ID")]
		public virtual Int32? EndLocationID
		{
			get
			{
				return this._EndLocationID;
			}
			set
			{
				this._EndLocationID = value;
			}
		}
		#endregion  
	}

    [Serializable]
	public partial class INPIInventoryFilter : IBqlTable
	{
		#region StartInventoryID
		public abstract class startInventoryID : PX.Data.BQL.BqlInt.Field<startInventoryID> { }
		protected Int32? _StartInventoryID;
		[StockItem(DisplayName = "Start Inventory ID")]
		public virtual Int32? StartInventoryID
		{
			get
			{
				return this._StartInventoryID;
			}
			set
			{
				this._StartInventoryID = value;
			}
		}
		#endregion

		#region EndInventoryID
		public abstract class endInventoryID : PX.Data.BQL.BqlInt.Field<endInventoryID> { }
		protected Int32? _EndInventoryID;
		[StockItem(DisplayName = "End Inventory ID")]
		public virtual Int32? EndInventoryID
		{
			get
			{
				return this._EndInventoryID;
			}
			set
			{
				this._EndInventoryID = value;
			}
		}
		#endregion
	}
	#endregion

	public class INPIClassMaint : PXGraph<INPIClassMaint, INPIClass>
	{
		public PXSelect<INPIClass> Classes;
		public PXSelect<INPIClass, Where<INPIClass.pIClassID, Equal<Current<INPIClass.pIClassID>>>> CurrentClass;
		public PXSelectJoin<INPIClassItem, 
				LeftJoin<InventoryItem, On<INPIClassItem.FK.InventoryItem>>,
			Where<INPIClassItem.pIClassID, Equal<Current<INPIClass.pIClassID>>>> Items;
		public PXSelectJoin<INPIClassItemClass,
			LeftJoin<INItemClass, On<INPIClassItemClass.FK.ItemClass>>,
			Where<INPIClassItemClass.pIClassID, Equal<Current<INPIClass.pIClassID>>>> ItemClasses;
		public PXSelectJoin<INPIClassLocation, 
				LeftJoin<INLocation, On<INPIClassLocation.FK.Location>>,
			Where<INPIClassLocation.pIClassID, Equal<Current<INPIClass.pIClassID>>>> Locations;

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

			this.Items.Cache.AllowInsert =
			this.Items.Cache.AllowUpdate =
			this.Items.Cache.AllowDelete =
				row.SelectedMethod == PIInventoryMethod.ListOfItems;
            AddItem.SetEnabled(row.SelectedMethod == PIInventoryMethod.ListOfItems);

			this.Locations.Cache.AllowInsert =
			this.Locations.Cache.AllowUpdate =
			this.Locations.Cache.AllowDelete =
				row.SiteID != null;

			PXUIFieldAttribute.SetWarning<INPIClass.unlockSiteOnCountingFinish>(sender, row, row.UnlockSiteOnCountingFinish == true
				? Messages.PITypeEarlyInventoryUnfreezeWarning
				: null);
		}

		protected virtual void INPIClass_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach(PXResult<INPIClassLocation> r in Locations.View.SelectMultiBound(new object[]{e.Row}))
			{
				Locations.Delete(r);
			}
		}

		protected virtual void INPIClass_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INPIClass row = (INPIClass)e.Row;
			INPIClass oldrow = (INPIClass)e.OldRow;
			if(row != null && oldrow != null)
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

		protected virtual bool AllowIncludeZeroItems(INPIClass row)
		{
			return !(row?.Method == PIMethod.FullPhysicalInventory ||
				row?.Method == PIMethod.ByInventoryItemSelected && row?.SelectedMethod == PIInventoryMethod.ItemsHavingNegativeBookQty);
		}

		public PXFilter<INPILocationFilter> LocationFilter;
		public PXFilter<INPIInventoryFilter> InventoryFilter;

		public PXAction<INPIClass> AddLocation;

		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(Tooltip = Messages.AddNewLine, ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		protected virtual IEnumerable addLocation(PXAdapter e)
		{
			if (this.LocationFilter.AskExt((graph, viewName) => graph.Views[viewName].Cache.Clear(), true) == WebDialogResult.OK)
				{
					var source =
					this.LocationFilter.Current.EndLocationID == null
						? PXSelectReadonly<INLocation,
								Where<INLocation.siteID, Equal<Current<INPIClass.siteID>>,
								  And<INLocation.locationCD, GreaterEqual<Required<INLocation.locationCD>>>>>
								.Select(this,
												this.LocationFilter.Cache.GetValueExt<INPILocationFilter.startLocationID>(
													this.LocationFilter.Current).ToString())
						: PXSelectReadonly
								<INLocation,
								Where<INLocation.siteID, Equal<Current<INPIClass.siteID>>,
								  And<INLocation.locationCD, Between<Required<INLocation.locationCD>, Required<INLocation.locationCD>>>>>
								.Select(this,
												this.LocationFilter.Cache.GetValueExt<INPILocationFilter.startLocationID>(this.LocationFilter.Current).ToString(),
												this.LocationFilter.Cache.GetValueExt<INPILocationFilter.endLocationID>(this.LocationFilter.Current).ToString());

						HashSet<int?> existingLocations = PXSelect<INPIClassLocation,
															Where<INPIClassLocation.pIClassID, Equal<Current<INPIClass.pIClassID>>>>.Select(this)
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
			if (this.InventoryFilter.AskExt((graph, viewName) => graph.Views[viewName].Cache.Clear(), true) == WebDialogResult.OK)
			{
				var source =
					this.InventoryFilter.Current.EndInventoryID == null
						? PXSelectReadonly<InventoryItem, 
							Where<InventoryItem.stkItem, Equal<True>,
							And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive> , 
							And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>, 
							And <InventoryItem.inventoryCD, GreaterEqual<Required<InventoryItem.inventoryCD>>>>>>>
						  	.Select(this,
						  	        this.InventoryFilter.Cache.GetValueExt<INPIInventoryFilter.startInventoryID>(
													this.InventoryFilter.Current).ToString())
						: PXSelectReadonly<InventoryItem,
							Where< InventoryItem.stkItem, Equal<True>,
							And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
							And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
							And<InventoryItem.inventoryCD, Between<Required<InventoryItem.inventoryCD>, Required<InventoryItem.inventoryCD>>>>>>>
						  	.Select(this,
									this.InventoryFilter.Cache.GetValueExt<INPIInventoryFilter.startInventoryID>(this.InventoryFilter.Current).ToString(),
									this.InventoryFilter.Cache.GetValueExt<INPIInventoryFilter.endInventoryID>(this.InventoryFilter.Current).ToString());
								
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
				new[]
				{
					Pair(FullPhysicalInventory, Messages.FullPhysicalInventory),
					Pair(ByInventoryItemSelected, Messages.ByInventory),
					Pair(ByMovementClass, Messages.ByMovementClass),
					Pair(ByABCClass, Messages.ByABCClass),
					Pair(ByCycle, Messages.ByCycleID),
					Pair(ByItemClassID, Messages.ByItemClassID),
				}) {}
		}

		public class fullPhysicalInventory : PX.Data.BQL.BqlString.Constant<fullPhysicalInventory>
		{
			public fullPhysicalInventory() : base(FullPhysicalInventory) {}
		}

		public static bool IsByFrequencyAllowed(string generationMethod)
		{
			return 
				generationMethod == ByMovementClass || 
				generationMethod == ByABCClass || 
				generationMethod == ByCycle;

		}
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
				new[]
				{
					Pair(ItemsHavingNegativeBookQty, Messages.ItemsHavingNegativeBookQty),
					Pair(RandomlySelectedItems, Messages.RandomlySelectedItems),
					Pair(ListOfItems, Messages.ListOfItems),
					Pair(LastCountDate, Messages.LastCountDate),
				}) {}
		}
	}
}