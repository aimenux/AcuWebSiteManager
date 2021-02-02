using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.RUTROT;
using PX.Objects.SO;
using PX.Objects.Common.Discount;
using PX.SM;
using PX.Web.UI;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data.DependencyInjection;

namespace PX.Objects.IN
{
	public abstract class InventoryItemMaintBase : PXGraph<InventoryItemMaintBase>
	{
		public InventoryItemMaintBase()
		{
			INSetup record = insetup.Current;
			SOSetup soSetup = sosetup.Current;
			CommonSetup commonSetup = commonsetup.Current;

			PXUIFieldAttribute.SetVisible<INUnit.toUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INUnit.toUnit>(itemunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INUnit.sampleToUnit>(itemunits.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INUnit.sampleToUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetVisible<INUnit.priceAdjustmentMultiplier>(itemunits.Cache, null, soSetup?.UsePriceAdjustmentMultiplier == true);

			PXDBDefaultAttribute.SetDefaultForInsert<INItemXRef.inventoryID>(itemxrefrecords.Cache, null, true);
		}

		public virtual void Initialize()
		{
			OnBeforeCommit += (graph) => ValidateINUnit(graph.Caches<InventoryItem>().Current as InventoryItem);
		}

		public abstract bool IsStockItemFlag { get; }

		#region Public members
		public bool doResetDefaultsOnItemClassChange;
		#endregion

		#region Cache Attached
		#region INItemClass
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INParentItemClass]
		protected virtual void INItemClass_ParentItemClassID_CacheAttached(PXCache sender) { }

		#endregion
		#region POVendorInventory
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXFormula(typeof(Selector<POVendorInventory.vendorID, Vendor.defLocationID>))]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>,
												And<Location.locationID, Equal<Current<POVendorInventory.vendorLocationID>>>>>))]
		protected virtual void POVendorInventory_VendorLocationID_CacheAttached(PXCache sender) { }
		#endregion
		#region INComponent
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXRestrictor(typeof(Where<DRDeferredCode.multiDeliverableArrangement, NotEqual<boolTrue>,
						And<DRDeferredCode.accountType, Equal<DeferredAccountType.income>>>), DR.Messages.ComponentsCantUseMDA)]
		[PXSelector(typeof(DRDeferredCode.deferredCodeID))]
		protected virtual void INComponent_DeferredCode_CacheAttached(PXCache sender) { }
		#endregion

		#region INItemXRef	
		[PXParent(typeof(INItemXRef.FK.InventoryItem))]
		[Inventory(Filterable = true, DirtyRead = true, Enabled = false, IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID), DefaultForInsert = true, DefaultForUpdate = false)]
		protected virtual void INItemXRef_InventoryID_CacheAttached(PXCache sender) { }
		#endregion

		#region INItemCategory
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(INCategory.categoryID), DescriptionField = typeof(INCategory.description))]
		[PXUIField(DisplayName = "Category ID")]
		protected virtual void INItemCategory_CategoryID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region Selects

		[PXViewName(Messages.InventoryItem)]
		public PXSelect<InventoryItem, 
			Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
			And<InventoryItem.isTemplate, Equal<False>,
			And<Match<Current<AccessInfo.userName>>>>>> Item;

		[PXCopyPasteHiddenView]
		public INSubItemSegmentValueList SegmentValues;

		[PXCopyPasteHiddenFields(typeof(InventoryItem.body))]
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> ItemSettings;
		public INUnitSelect<INUnit, InventoryItem.inventoryID, InventoryItem.parentItemClassID, InventoryItem.salesUnit, InventoryItem.purchaseUnit, InventoryItem.baseUnit, InventoryItem.lotSerClassID> itemunits;

		public PXSelect<INComponent, Where<INComponent.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> Components;

		public PXSelectOrderBy<INCategory, OrderBy<Asc<INCategory.sortOrder>>> Categories;
		public PXSelect<ARSalesPrice> SalesPrice;

		public PXSetupOptional<INSetup> insetup;
		public PXSetupOptional<SOSetup> sosetup;
		public PXSetupOptional<CommonSetup> commonsetup;
		public PXSetup<Company> Company;

		public PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Optional<InventoryItem.itemClassID>>>> ItemClass;

		public POVendorInventorySelect<POVendorInventory,
				LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POVendorInventory.vendorID>>,
				LeftJoin<CRLocation, On<CRLocation.bAccountID, Equal<POVendorInventory.vendorID>,
							And<CRLocation.locationID, Equal<POVendorInventory.vendorLocationID>>>>>,
				Where<POVendorInventory.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
				InventoryItem> VendorItems;

		public PXSelect<INItemXRef, Where<INItemXRef.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> itemxrefrecords;

		public CRAttributeList<InventoryItem> Answers;

		public PXSelectJoin<INItemCategory,
				InnerJoin<INCategory, On<INItemCategory.FK.Category>>,
				Where<INItemCategory.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> Category;

		public PXSelect<CacheEntityItem,
			Where<CacheEntityItem.path, Equal<CacheEntityItem.path>>,
			OrderBy<Asc<CacheEntityItem.number>>> EntityItems;

		public PXSelect<ARPriceWorksheetDetail> arpriceworksheetdetails;
		public PXSelect<DiscountItem> discountitems;
		public PXSelect<PM.PMItemRate> pmitemrates;
		public PXSelect<INKitSpecHdr> kitheaders;
		public PXSelect<INKitSpecStkDet> kitspecs;
		public PXSelect<INKitSpecNonStkDet> kitnonstockdet;

		protected IEnumerable entityItems(string parent) => GetEntityItems(parent);

		private IEnumerable GetEntityItems(String parent)
		{
			string screenID = IsStockItemFlag ? "IN202500" : "IN202000";

			PXSiteMapNode siteMap = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID);
			if (siteMap != null)
				foreach (var entry in EMailSourceHelper.TemplateEntity(this, parent, null, siteMap.GraphType, true))
					yield return entry;
		}

		#endregion

		#region Delegates
		protected virtual IEnumerable categories(
			[PXInt]
			int? categoryID
		)
		{
			if (categoryID == null)
			{
				yield return new INCategory()
				{
					CategoryID = 0,
					Description = PXSiteMap.RootNode.Title
				};
			}
			foreach (INCategory item in PXSelect<INCategory,
				Where<INCategory.parentID,
					Equal<Required<INCategory.parentID>>>,
				OrderBy<Asc<INCategory.sortOrder>>>.Select(this, categoryID))
			{
				if (!string.IsNullOrEmpty(item.Description))
					yield return item;
			}
		}

		#endregion

		#region Buttons definition		

		public PXSave<InventoryItem> Save;
		public PXAction<InventoryItem> cancel;
		public PXInsert<InventoryItem> Insert;
		public PXCopyPasteAction<InventoryItem> Edit;
		public PXDelete<InventoryItem> Delete;
		public PXFirst<InventoryItem> First;
		public PXPrevious<InventoryItem> Prev;
		public PXNext<InventoryItem> Next;
		public PXLast<InventoryItem> Last;

		public PXChangeID<InventoryItem, InventoryItem.inventoryCD> ChangeID;

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
		{
			foreach (InventoryItem e in (new PXCancel<InventoryItem>(this, "Cancel")).Press(a))
			{
				if (Item.Cache.GetStatus(e) == PXEntryStatus.Inserted)
				{
					InventoryItem e1 = PXSelectReadonly<InventoryItem,
						Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>
						.Select(this, e.InventoryCD);
					if (e1 != null)
					{
						Item.Cache.RaiseExceptionHandling<InventoryItem.inventoryCD>(e, e.InventoryCD,
							new PXSetPropertyException(
								e1.IsTemplate == true ? Messages.TemplateItemExists :
								e1.StkItem == true ? Messages.StockItemExists : Messages.NonStockItemExists));
					}
				}
				yield return e;
			}
		}

		#endregion

		#region Event handlers

		#region InventoryItem
		protected virtual void InventoryItem_ItemClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			this.doResetDefaultsOnItemClassChange = false;
			INItemClass ic = ItemClass.Select(e.NewValue);

			if (ic != null)
			{
				this.doResetDefaultsOnItemClassChange = true;
				if (e.ExternalCall && cache.GetStatus(e.Row) != PXEntryStatus.Inserted)
				{
					if (Item.Ask(AR.Messages.Warning, Messages.ItemClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
					{
						this.doResetDefaultsOnItemClassChange = false;
					}
				}
			}
		}

		protected virtual void InventoryItem_IsSplitted_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = e.Row as InventoryItem;
			if (row != null)
			{
				if (row.IsSplitted == false)
				{
					foreach (INComponent c in Components.Select())
					{
						Components.Delete(c);
					}

					row.TotalPercentage = 100;
				}
				else
					row.TotalPercentage = 0;
			}
		}

		protected virtual void InventoryItem_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (e.Row == null) { return; }

			InventoryItem ii_rec = (InventoryItem)e.Row;

			// deleting only inventory-specific uoms
			foreach (INUnit inunit_rec in PXSelect<INUnit,
				Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
				And<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>>>>.Select(this, ii_rec.InventoryID))
			{
				itemunits.Delete(inunit_rec);
			}
		}

		public static void CheckSameTermOnAllComponents(PXCache cache, IEnumerable<INComponent> components)
		{
			DRTerms.Term term = null;

			foreach (var component in components)
			{
				var code = (DRDeferredCode)PXSelectorAttribute.Select<INComponent.deferredCode>(cache, component);
				if (DeferredMethodType.RequiresTerms(code))
				{
					if (term == null)
					{
						term = new DRTerms.Term(component.DefaultTerm, component.DefaultTermUOM);
						continue;
					}

					if (term.Length != component.DefaultTerm || term.UOM != component.DefaultTermUOM)
					{
						if (cache.RaiseExceptionHandling<INComponent.defaultTerm>(component, component.DefaultTerm,
							new PXSetPropertyException<INComponent.defaultTerm>(DR.Messages.DefaultTermMustBeTheSameForAllComponents)))
							throw new PXRowPersistingException(typeof(INComponent.defaultTerm).Name, component.DefaultTerm, DR.Messages.DefaultTermMustBeTheSameForAllComponents);
					}
				}
			}
		}

		protected virtual void InventoryItem_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			InventoryItem item = (InventoryItem)e.Row;
			if (e.Operation.Command() == PXDBOperation.Update && item.KitItem != true
				&& (bool?)Item.Cache.GetValueOriginal<InventoryItem.kitItem>(item) == true)
			{
				INKitSpecHdr kitSpec = PXSelectReadonly<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Required<InventoryItem.inventoryID>>>>
					.SelectWindowed(this, 0, 1, item.InventoryID);

				if (kitSpec != null)
				{
					if (sender.RaiseExceptionHandling<InventoryItem.kitItem>(
							e.Row, item.KitItem, new PXSetPropertyException<InventoryItem.kitItem>(Messages.KitSpecificationExists)))
					{
						throw new PXRowPersistingException(nameof(InventoryItem.kitItem), item.KitItem, Messages.KitSpecificationExists);
					}
				}
			}
		}

		public static void VerifyComponentPercentages(PXCache itemCache, InventoryItem item, IEnumerable<INComponent> components)
		{
			var hasResiduals = components.Any(c => c.AmtOption == INAmountOption.Residual);

			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				return;
			}

			if (item.TotalPercentage != 100 && hasResiduals == false)
			{
				if (itemCache.RaiseExceptionHandling<InventoryItem.totalPercentage>(item, item.TotalPercentage, new PXSetPropertyException(Messages.SumOfAllComponentsMustBeHundred)))
				{
					throw new PXRowPersistingException(typeof(InventoryItem.totalPercentage).Name, item.TotalPercentage, Messages.SumOfAllComponentsMustBeHundred);
				}
			}
			else if (item.TotalPercentage >= 100 && hasResiduals == true)
			{
				if (itemCache.RaiseExceptionHandling<InventoryItem.totalPercentage>(item, item.TotalPercentage, new PXSetPropertyException(Messages.SumOfAllComponentsMustBeLessHundredWithResiduals)))
				{
					throw new PXRowPersistingException(typeof(InventoryItem.totalPercentage).Name, item.TotalPercentage, Messages.SumOfAllComponentsMustBeLessHundredWithResiduals);
				}
			}
		}

		public static void VerifyOnlyOneResidualComponent(PXCache itemCache, InventoryItem item, IEnumerable<INComponent> components)
		{
			if (components.Count(c => c.AmtOption == INAmountOption.Residual) > 1)
			{
				if (itemCache.RaiseExceptionHandling<InventoryItem.totalPercentage>(item, item.TotalPercentage, new PXSetPropertyException(Messages.OnlyOneResidualComponentAllowed)))
				{
					throw new PXRowPersistingException(typeof(InventoryItem.totalPercentage).Name, item.TotalPercentage, Messages.OnlyOneResidualComponentAllowed);
				}
			}
		}

		public static void SetDefaultTermControlsState(PXCache cache, InventoryItem item)
		{
			if (item == null)
				return;

			var code = (DRDeferredCode)PXSelectorAttribute.Select<InventoryItem.deferredCode>(cache, item);
			bool enableTerms = DeferredMethodType.RequiresTerms(code);
			PXUIFieldAttribute.SetEnabled<InventoryItem.defaultTerm>(cache, item, enableTerms);
			PXUIFieldAttribute.SetEnabled<InventoryItem.defaultTermUOM>(cache, item, enableTerms);
		}
		#endregion

		#region INItemXRef Event Handlers
		protected virtual void INItemXRef_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = (INItemXRef)e.Row;
			if (row == null) return;

			VerifyXRefUOMExists(sender, row);
		}

		private void VerifyXRefUOMExists(PXCache sender, INItemXRef row)
		{
			var item = Item.Current;
			sender.RaiseExceptionHandling<INItemXRef.uOM>(
				row,
				row.UOM,
				itemunits.Select()
						.RowCast<INUnit>()
						.Select(u => u.FromUnit)
						.Concat(new[] { null, item.BaseUnit })
						.Contains(row.UOM)
					? null
					: new PXSetPropertyException(Messages.UOMAssignedToAltIDIsNotDefined, PXErrorLevel.Warning));
		}

		protected virtual void INItemXRef_AlternateID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as INItemXRef;

			if (row == null || Item.Current == null || e.NewValue == null)
				return;

			e.NewValue = ((String)e.NewValue).Trim();
			if ((String)e.NewValue == String.Empty)
				e.NewValue = null;
		}

		protected virtual void INItemXRef_InventoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Item.Current != null)
			{
				e.NewValue = Item.Current.InventoryID;
				e.Cancel = true;
			}
		}

		protected virtual void INItemXRef_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}


		protected virtual void INItemXRef_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var xRef = (INItemXRef)e.Row;
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update)
				&& xRef.AlternateType.IsNotIn(INAlternateType.VPN, INAlternateType.CPN))
			{
				xRef.BAccountID = 0;
				sender.Normalize();
			}
		}

		protected virtual void INItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
			{
				e.Cancel = true;
			}
		}
		protected virtual void INItemXRef_AlternateID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as INItemXRef;

			if (row == null || Item.Current == null || row.AlternateID == null)
				return;

			if (!PXDimensionAttribute.MatchMask<InventoryItem.inventoryCD>(Item.Cache, row.AlternateID))
			{
				sender.RaiseExceptionHandling<INItemXRef.alternateID>(row, row.AlternateID, new PXSetPropertyException(Messages.AlternateIDDoesNotCorrelateWithCurrentSegmentRules, PXErrorLevel.Warning));
			}
		}

		protected virtual void INItemXRef_BAccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && e.NewValue == null && ((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
			{
				e.NewValue = (int)0;
				e.Cancel = true;
			}
		}

		#endregion

		#region InventoryItem Event Handlers

		protected virtual void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			InventoryItem row = (InventoryItem)e.Row;

			if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
			{
				PXUIFieldAttribute.SetWarning<InventoryItem.weightUOM>(sender, row, string.IsNullOrEmpty(commonsetup.Current.WeightUOM)
					? Messages.BaseCompanyUomIsNotDefined : null);
				PXUIFieldAttribute.SetWarning<InventoryItem.volumeUOM>(sender, row, string.IsNullOrEmpty(commonsetup.Current.VolumeUOM)
					? Messages.BaseCompanyUomIsNotDefined : null);
			}

			sender.Adjust<PXUIFieldAttribute>().For<InventoryItem.itemClassID>(a => a.Enabled = row.TemplateItemID == null)
				.SameFor<InventoryItem.baseUnit>()
				.SameFor<InventoryItem.decimalBaseUnit>()
				.SameFor<InventoryItem.purchaseUnit>()
				.SameFor<InventoryItem.decimalPurchaseUnit>()
				.SameFor<InventoryItem.salesUnit>()
				.SameFor<InventoryItem.decimalSalesUnit>()
				.SameFor<InventoryItem.itemType>()
				.SameFor<InventoryItem.taxCategoryID>();
		}

		protected virtual void InventoryItem_ItemClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (InventoryItem)e.Row;
			if (row != null && row.ItemClassID < 0)
			{
				INItemClass ic = ItemClass.Select();
				row.ParentItemClassID = ic?.ParentItemClassID;
			}
			else if (row != null)
			{
				row.ParentItemClassID = row.ItemClassID;
			}

			if (doResetDefaultsOnItemClassChange)
			{
				ResetConversionsSettings(sender, row);

				if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
					sender.SetDefaultExt<InventoryItem.dfltSiteID>(e.Row);

				if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
				{
					sender.SetDefaultExt<InventoryItem.deferredCode>(e.Row);
					sender.SetDefaultExt<InventoryItem.postClassID>(e.Row);
					sender.SetDefaultExt<InventoryItem.markupPct>(e.Row);
					sender.SetDefaultExt<InventoryItem.minGrossProfitPct>(e.Row);
				}

				sender.SetDefaultExt<InventoryItem.taxCategoryID>(e.Row);
				sender.SetDefaultExt<InventoryItem.itemType>(e.Row);
				sender.SetDefaultExt<InventoryItem.priceClassID>(e.Row);
				sender.SetDefaultExt<InventoryItem.priceWorkgroupID>(e.Row);
				sender.SetDefaultExt<InventoryItem.priceManagerID>(e.Row);
				sender.SetDefaultExt<InventoryItem.undershipThreshold>(e.Row);
				sender.SetDefaultExt<InventoryItem.overshipThreshold>(e.Row);

				INItemClass ic = ItemClass.Select();
				if (ic != null)
				{
					sender.SetValue<InventoryItem.priceWorkgroupID>(e.Row, ic.PriceWorkgroupID);
					sender.SetValue<InventoryItem.priceManagerID>(e.Row, ic.PriceManagerID);
				}
			}
		}

		protected virtual void InventoryItem_DeferredCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var item = e.Row as InventoryItem;
			UpdateSplittedFromDeferralCode(sender, item);
			SetDefaultTerm(sender, item, typeof(InventoryItem.deferredCode), typeof(InventoryItem.defaultTerm), typeof(InventoryItem.defaultTermUOM));
		}

		public static void UpdateSplittedFromDeferralCode(PXCache cache, InventoryItem item)
		{
			if (item == null)
				return;

			var code = (DRDeferredCode)PXSelectorAttribute.Select<InventoryItem.deferredCode>(cache, item);
			cache.SetValueExt<InventoryItem.isSplitted>(item, code != null && code.MultiDeliverableArrangement == true);
		}

		#endregion

		#region INComponent Event Handles

		protected virtual void INComponent_Percentage_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null && row.AmtOption == INAmountOption.Percentage)
			{
				row.Percentage = GetRemainingPercentage();
			}
		}

		protected virtual void INComponent_ComponentID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null)
			{
				InventoryItem item = InventoryItem.PK.FindDirty(this, row.ComponentID);
				var deferralCode = (DRDeferredCode)PXSelectorAttribute.Select<InventoryItem.deferredCode>(Item.Cache, item);
				bool useDeferralFromItem = deferralCode != null && deferralCode.MultiDeliverableArrangement != true;

				if (item != null)
				{
					row.SalesAcctID = item.SalesAcctID;
					row.SalesSubID = item.SalesSubID;
					row.UOM = item.SalesUnit;
					row.DeferredCode = useDeferralFromItem ? item.DeferredCode : null;
					row.DefaultTerm = item.DefaultTerm;
					row.DefaultTermUOM = item.DefaultTermUOM;
				}
			}
		}

		protected virtual void INComponent_AmtOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null)
			{
				if (row.AmtOption == INAmountOption.Percentage)
				{
					row.FixedAmt = null;
					row.Percentage = GetRemainingPercentage();
				}
				else
				{
					row.Percentage = 0;
				}

				if (row.AmtOption == INAmountOption.Residual)
				{
					sender.SetValueExt<INComponent.deferredCode>(row, null);
					sender.SetDefaultExt<INComponent.fixedAmt>(row);
					sender.SetDefaultExt<INComponent.percentage>(row);
				}
			}
		}

		protected virtual void INComponent_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var component = e.Row as INComponent;
			SetComponentControlsState(sender, component);

			InventoryHelper.CheckZeroDefaultTerm<INComponent.deferredCode, INComponent.defaultTerm>(sender, component);
		}

		protected virtual void INComponent_DeferredCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetDefaultTerm(sender, e.Row, typeof(INComponent.deferredCode), typeof(INComponent.defaultTerm), typeof(INComponent.defaultTermUOM));
		}


		public static void SetDefaultTerm(PXCache cache, object row, Type deferralCode, Type defaultTerm, Type defaultTermUOM)
		{
			if (row == null)
				return;

			var code = (DRDeferredCode)PXSelectorAttribute.Select(cache, row, deferralCode.Name);

			if (code == null || DeferredMethodType.RequiresTerms(code) == false)
			{
				cache.SetDefaultExt(row, defaultTerm.Name);
				cache.SetDefaultExt(row, defaultTermUOM.Name);
			}
		}

		public static void SetComponentControlsState(PXCache cache, INComponent component)
		{
			bool disabledFixedAmtAndPercentage = false;

			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				disabledFixedAmtAndPercentage = true;
				PXUIFieldAttribute.SetEnabled<INComponent.amtOption>(cache, null, false);
				PXUIFieldAttribute.SetEnabled<INComponent.fixedAmt>(cache, null, false);
				PXUIFieldAttribute.SetEnabled<INComponent.percentage>(cache, null, false);
				PXDefaultAttribute.SetPersistingCheck<INComponent.amtOption>(cache, null, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<INComponent.fixedAmt>(cache, null, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<INComponent.percentage>(cache, null, PXPersistingCheck.Nothing);
			}

			if (component == null)
				return;

			bool isResidual = component.AmtOption == INAmountOption.Residual;

			PXDefaultAttribute.SetPersistingCheck<INComponent.deferredCode>(cache, component, isResidual ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

			PXUIFieldAttribute.SetEnabled<INComponent.deferredCode>(cache, component, isResidual == false);
			PXUIFieldAttribute.SetEnabled<INComponent.fixedAmt>(cache, component, disabledFixedAmtAndPercentage == false && component.AmtOption == INAmountOption.FixedAmt);
			PXUIFieldAttribute.SetEnabled<INComponent.percentage>(cache, component, disabledFixedAmtAndPercentage == false && component.AmtOption == INAmountOption.Percentage);

			var code = (DRDeferredCode)PXSelectorAttribute.Select<INComponent.deferredCode>(cache, component);
			bool enableTerms = DeferredMethodType.RequiresTerms(code) && isResidual == false;
			PXUIFieldAttribute.SetEnabled<INComponent.defaultTerm>(cache, component, enableTerms);
			PXUIFieldAttribute.SetEnabled<INComponent.defaultTermUOM>(cache, component, enableTerms);
		}
		#endregion

		#region INUnit Event Handlers
		protected virtual void INUnit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INUnit row = (INUnit)e.Row;
			PXFieldState state = (PXFieldState)sender.GetStateExt<INUnit.fromUnit>(row);
			uint msgNbr;
			String msqPrefix;
			if (Item.Current != null && row.ToUnit == Item.Current.BaseUnit && (state.Error == null || state.Error == PXMessages.Localize(Messages.BaseUnitNotSmallest, out msgNbr, out msqPrefix) || state.ErrorLevel == PXErrorLevel.RowInfo))
			{
				if (row.UnitMultDiv == MultDiv.Multiply && row.UnitRate < 1 || row.UnitMultDiv == MultDiv.Divide && row.UnitRate > 1)
					sender.RaiseExceptionHandling<INUnit.fromUnit>(row, row.FromUnit, new PXSetPropertyException(Messages.BaseUnitNotSmallest, PXErrorLevel.RowWarning));
				else
					sender.RaiseExceptionHandling<INUnit.fromUnit>(row, row.FromUnit, null);
			}
		}

		public virtual decimal? GetUnitRate(PXCache sender, INUnit unit, int? itemClassID)
		{
			decimal? unitRate = unit != null ? unit.UnitRate ?? 1m : 1m;
			INUnit existingUnit = PXSelect<INUnit,
					Where<INUnit.unitType, NotEqual<INUnitType.inventoryItem>,
					And<INUnit.fromUnit, Equal<Current<INUnit.fromUnit>>,
					And<INUnit.toUnit, Equal<Current<INUnit.toUnit>>,
					And<INUnit.unitMultDiv, Equal<Current<INUnit.unitMultDiv>>,
						And<Where<INUnit.itemClassID, Equal<Required<INUnit.itemClassID>>, Or<INUnit.unitType, Equal<INUnitType.global>>>>>>>>,
					OrderBy<Asc<INUnit.unitType>>>.SelectSingleBound(sender.Graph, new object[] { unit }, itemClassID);
			if (existingUnit != null)
			{
				unitRate = existingUnit.UnitRate;
			}

			return unitRate;
		}

		/// <summary><see cref="INUnit"/> Updated</summary>
		protected virtual void INUnit_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INUnit unit = e.Row as INUnit;

			if (unit != null && unit.FromUnit != null && !itemunits.Cache.ObjectsEqual<INUnit.fromUnit>(e.Row, e.OldRow) && Item.Current != null)
			{
				unit.UnitRate = GetUnitRate(sender, unit, Item.Current.ItemClassID);
			}

			foreach (var row in itemxrefrecords.Select())
				VerifyXRefUOMExists(itemxrefrecords.Cache, row);
		}

		/// <summary><see cref="INUnit"/> Inserted</summary>
		protected virtual void INUnit_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INUnit unit = e.Row as INUnit;

			if (unit != null && unit.FromUnit != null && Item.Current != null)
			{
				unit.UnitRate = GetUnitRate(sender, unit, Item.Current.ItemClassID);
			}

			foreach (var row in itemxrefrecords.Select())
				VerifyXRefUOMExists(itemxrefrecords.Cache, row);
		}

		/// <summary><see cref="INUnit"/> Deleted</summary>
		protected virtual void INUnit_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			foreach (var row in itemxrefrecords.Select())
				VerifyXRefUOMExists(itemxrefrecords.Cache, row);
		}
		#endregion

		#region POVendorInventory
		protected virtual void POVendorInventory_IsDefault_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			POVendorInventory current = e.Row as POVendorInventory;
			if ((POVendorInventory)this.VendorItems.SelectWindowed(0, 1, current.InventoryID) == null)
			{
				e.NewValue = true;
				e.Cancel = true;
			}
		}

		protected virtual void POVendorInventory_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			if (Item.Cache.GetStatus(this.Item.Current).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)) return;

			InventoryItem upd = PXCache<InventoryItem>.CreateCopy(this.Item.Current);
			POVendorInventory vendor = e.Row as POVendorInventory;
			object isdefault = cache.GetValueExt<POVendorInventory.isDefault>(e.Row);
			if (isdefault is PXFieldState)
			{
				isdefault = ((PXFieldState)isdefault).Value;
			}
			if ((bool?)isdefault == true)
			{
				upd.PreferredVendorID = null;
				upd.PreferredVendorLocationID = null;
				this.Item.Update(upd);
			}
		}
		protected virtual void POVendorInventory_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			POVendorInventory current = e.Row as POVendorInventory;
			if (current.VendorID != null && current.IsDefault == true && (!IsStockItemFlag || current.SubItemID != null))
			{
				InventoryItem upd = this.Item.Current;
				upd.PreferredVendorID = current.IsDefault == true ? current.VendorID : null;
				upd.PreferredVendorLocationID = current.IsDefault == true ? current.VendorLocationID : null;
				Item.Update(upd);
			}
		}

		protected virtual void POVendorInventory_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			POVendorInventory current = e.Row as POVendorInventory;
			if (e.OldRow == null || current == null || current.VendorID == null || (IsStockItemFlag && current.SubItemID == null))
				return;

			if ((current.IsDefault == true && (this.Item.Current.PreferredVendorID != current.VendorID || this.Item.Current.PreferredVendorLocationID != current.VendorLocationID) ||
					(current.IsDefault != true && this.Item.Current.PreferredVendorID == current.VendorID && this.Item.Current.PreferredVendorLocationID == current.VendorLocationID)))
			{
				InventoryItem upd = this.Item.Current;
				upd.PreferredVendorID = current.IsDefault == true ? current.VendorID : null;
				upd.PreferredVendorLocationID = current.IsDefault == true ? current.VendorLocationID : null;
				Item.Update(upd);
				if (current.IsDefault == true)
				{
					foreach (POVendorInventory vendorInventory in VendorItems.Select())
					{
						if (vendorInventory.RecordID != current.RecordID && vendorInventory.IsDefault == true)
							VendorItems.Cache.SetValue<POVendorInventory.isDefault>(vendorInventory, false);
					}
					this.VendorItems.Cache.ClearQueryCacheObsolete();
					this.VendorItems.View.RequestRefresh();
				}
			}
		}
		#endregion

		#endregion

		public override void Persist()
		{
			using (PXTransactionScope tscope = new PXTransactionScope())
			{
				INItemClass itemClassOnTheFly = null;
				if (Item.Current != null && Item.Current.ItemClassID < 0)
				{
					itemClassOnTheFly = ItemClass.Select();
					var itemClassGraph = PXGraph.CreateInstance<INItemClassMaint>();
					var itemClassCopy = (INItemClass)itemClassGraph.itemclass.Cache.CreateCopy(itemClassOnTheFly);
					itemClassCopy = itemClassGraph.itemclass.Insert(itemClassCopy);
					itemClassGraph.Actions.PressSave();

					foreach (var row in ItemClass.Cache.Inserted)
					{
						ItemClass.Cache.SetStatus(row, PXEntryStatus.Held);
					}
					Item.Current.ItemClassID = itemClassGraph.itemclass.Current.ItemClassID;
				}
				try
				{
					base.Persist();
				}
				catch
				{
					if (itemClassOnTheFly != null)
					{
						Item.Current.ItemClassID = itemClassOnTheFly.ItemClassID;
						ItemClass.Cache.SetStatus(itemClassOnTheFly, PXEntryStatus.Inserted);
					}
					throw;
				}
				ItemClass.Cache.Clear();

				tscope.Complete();
			}
		}

		protected decimal GetRemainingPercentage()
		{
			decimal result = 100;

			foreach (INComponent comp in Components.Select())
			{
				if (comp.AmtOption == INAmountOption.Percentage)
					result -= (comp.Percentage ?? 0);
			}

			if (result > 0)
				return result;
			else
				return 0;
		}

		protected decimal SumComponentsPercentage()
		{
			decimal result = 0;

			foreach (INComponent comp in Components.Select())
			{
				if (comp.AmtOption == INAmountOption.Percentage)
					result += (comp.Percentage ?? 0);
			}

			return result;
		}

		public bool AlwaysFromBaseCurrency
		{
			get
			{
				bool alwaysFromBase = false;

				ARSetup arsetup = PXSelect<ARSetup>.Select(this);
				if (arsetup != null)
				{
					alwaysFromBase = arsetup.AlwaysFromBaseCury == true;
				}

				return alwaysFromBase;
			}
		}
		public string SalesPriceUpdateUnit => SalesPriceUpdateUnitType.BaseUnit;

		protected virtual void ResetConversionsSettings(PXCache cache, InventoryItem item)
		{
			//sales and purchase units must be cleared not to be added to item unit conversions on base unit change.
			cache.SetValueExt<InventoryItem.baseUnit>(item, null);
			cache.SetValue<InventoryItem.salesUnit>(item, null);
			cache.SetValue<InventoryItem.purchaseUnit>(item, null);

			cache.SetDefaultExt<InventoryItem.baseUnit>(item);
			cache.SetDefaultExt<InventoryItem.salesUnit>(item);
			cache.SetDefaultExt<InventoryItem.purchaseUnit>(item);

			cache.SetDefaultExt<InventoryItem.decimalBaseUnit>(item);
			cache.SetDefaultExt<InventoryItem.decimalSalesUnit>(item);
			cache.SetDefaultExt<InventoryItem.decimalPurchaseUnit>(item);
		}

		protected virtual void ValidateINUnit(InventoryItem item)
		{
			if (item == null)
				return;

			using (PXDataRecord record = PXDatabase.SelectSingle<INUnit>(new PXDataField<INUnit.toUnit>(),
				new PXDataFieldValue<INUnit.unitType>(INUnitType.InventoryItem),
				new PXDataFieldValue<INUnit.inventoryID>(item.InventoryID),
				new PXDataFieldValue<INUnit.toUnit>(PXDbType.NVarChar, 6, item.BaseUnit, PXComp.NE)))
			{
				if (record != null)
					throw new PXException(Messages.WrongInventoryItemToUnitValue, record.GetString(0), item.InventoryCD, item.BaseUnit);
			}

			IEnumerable<PXDataRecord> baseConversions = PXDatabase.SelectMulti<INUnit>(new PXDataField<INUnit.toUnit>(),
				new PXDataFieldValue<INUnit.unitType>(INUnitType.InventoryItem),
				new PXDataFieldValue<INUnit.inventoryID>(item.InventoryID),
				new PXDataFieldValue<INUnit.fromUnit>(PXDbType.NVarChar, item.BaseUnit),
				new PXDataFieldValue<INUnit.toUnit>(PXDbType.NVarChar, item.BaseUnit));

			if (baseConversions.Count() != 1)
				throw new PXException(Messages.BaseConversionNotFound, item.BaseUnit, item.BaseUnit, item.InventoryCD);
		}
	}
}
