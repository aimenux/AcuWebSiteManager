using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using ItemStats = PX.Objects.IN.Overrides.INDocumentRelease.ItemStats;
using System.Collections;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;

namespace PX.Objects.IN
{
	public class INItemSiteMaint : PXGraph<INItemSiteMaint, INItemSite>
	{
		public PXFilter<AP.Vendor> _Vendor_;
		public PXFilter<EP.EPEmployee> _Employee_;

		public PXSelectJoin<INItemSite,
			LeftJoin<INSite, On<INItemSite.FK.Site>,
			LeftJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>>,
			Where<INItemSite.inventoryID, Equal<Optional<INItemSite.inventoryID>>,
			And<Where<INSite.siteID, IsNull, Or<Match<INSite, Current<AccessInfo.userName>>>>>>,
			OrderBy<Asc<InventoryItem.inventoryCD>>> itemsiterecord;

		public PXSelectJoin<INItemSite, 
			InnerJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>, 
			Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>, 
			And<INItemSite.siteID, Equal<Current<INItemSite.siteID>>>>> itemsitesettings;

		public PXSelect<SiteStatus> sitestatus;

		public PXSetup<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>> itemrecord;
		public PXSetup<INPostClass, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>> postclass;
		public PXSetup<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Current<InventoryItem.lotSerClassID>>>> lotserclass;
		public PXSetup<INSite, Where<INSite.siteID, Equal<Current<INItemSite.siteID>>>> insite;

		public INUnitSelect<INUnit, INItemSite.inventoryID, InventoryItem.itemClassID, INItemSite.dfltSalesUnit, INItemSite.dfltPurchaseUnit, InventoryItem.baseUnit, InventoryItem.lotSerClassID> itemunits;
		public PXSelect<INItemSiteReplenishment,
			Where<INItemSiteReplenishment.siteID, Equal<Current<INItemSite.siteID>>,
			And<INItemSiteReplenishment.inventoryID, Equal<Current<INItemSite.inventoryID>>>>> subitemrecords;

		public PXSetup<INSetup> insetup;

		public PXSelectJoin<POVendorInventory, 
			InnerJoin<InventoryItem, On<POVendorInventory.FK.InventoryItem>>> PreferredVendorItem;

		protected IEnumerable preferredVendorItem()
		{
			foreach (var item in PXSelectJoin<POVendorInventory,
			InnerJoin<InventoryItem, On<POVendorInventory.FK.InventoryItem>>,
			Where<POVendorInventory.inventoryID, Equal<Current<INItemSite.inventoryID>>,
				And<POVendorInventory.vendorID, Equal<Current<INItemSite.preferredVendorID>>,
				And<POVendorInventory.subItemID, Equal<Current<InventoryItem.defaultSubItemID>>,
				And<POVendorInventory.purchaseUnit, Equal<InventoryItem.purchaseUnit>,
				And<Where<POVendorInventory.vendorLocationID, Equal<Current<INItemSite.preferredVendorLocationID>>,
								Or<POVendorInventory.vendorLocationID, IsNull>>>>>>>,
				OrderBy<Desc<POVendorInventory.vendorLocationID,
							Asc<POVendorInventory.recordID>>>>.SelectSingleBound(this, null))
			{
				yield return item;
			}
		}
		public PXSelect<ItemStats> itemstats;

		public INItemSiteMaint()
		{
			INSetup record = insetup.Current;

			PXUIFieldAttribute.SetVisible<INUnit.toUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INUnit.toUnit>(itemunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INUnit.sampleToUnit>(itemunits.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INUnit.sampleToUnit>(itemunits.Cache, null, false);

			this.PreferredVendorItem.Cache.AllowUpdate = false;
			PXUIFieldAttribute.SetEnabled(this.PreferredVendorItem.Cache, null, false);			

			bool enableSubItemReplenishment = PXAccess.FeatureInstalled<FeaturesSet.replenishment>() && PXAccess.FeatureInstalled<FeaturesSet.subItem>();
			subitemrecords.AllowSelect = enableSubItemReplenishment;
			PXUIFieldAttribute.SetVisible<INItemSite.subItemOverride>(itemsiterecord.Cache, null, enableSubItemReplenishment);
		}

		protected virtual void INItemSite_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{       
			INItemSite itemsite = (INItemSite)e.Row;
			if (itemsite != null && itemsite.InventoryID != null && itemsite.SiteID != null)
			{
				InventoryItem item = itemrecord.Current;
				if (itemrecord.Current != null)
				{
					DefaultItemSiteByItem(this, itemsite, item, insite.Current, postclass.Current);					
				}
                this.itemrecord.Cache.IsDirty = true;
			}
		}


		public static void DefaultItemSiteByItem(PXGraph graph, INItemSite itemsite, InventoryItem item, INSite site, INPostClass postclass)
		{
			if (item != null)
			{
				itemsite.PendingStdCost = item.PendingStdCost;
				itemsite.PendingStdCostDate = item.PendingStdCostDate;
				itemsite.StdCost = item.StdCost;
				itemsite.StdCostDate = item.StdCostDate;
				itemsite.LastStdCost = item.LastStdCost;

				itemsite.BasePrice = item.BasePrice;

				itemsite.MarkupPct = item.MarkupPct;
				itemsite.RecPrice = item.RecPrice;

				itemsite.ABCCodeID = item.ABCCodeID;
				itemsite.ABCCodeIsFixed = item.ABCCodeIsFixed;
				itemsite.MovementClassID = item.MovementClassID;
				itemsite.MovementClassIsFixed = item.MovementClassIsFixed;

				itemsite.PreferredVendorID = item.PreferredVendorID;
				itemsite.PreferredVendorLocationID = item.PreferredVendorLocationID;
				itemsite.ReplenishmentClassID = site != null ? site.ReplenishmentClassID : null;
				itemsite.DfltReceiptLocationID = site.ReceiptLocationID;
				itemsite.DfltShipLocationID = site.ShipLocationID;

				DefaultItemReplenishment(graph, itemsite);
				DefaultSubItemReplenishment(graph, itemsite);
			}			
		}

		public static void DefaultInvtAcctSub(PXGraph graph, INItemSite itemsite, InventoryItem item, INSite site, INPostClass postclass)
		{
			if (site != null && site.OverrideInvtAccSub == true)
			{
				itemsite.InvtAcctID = site.InvtAcctID;
				itemsite.InvtSubID = site.InvtSubID;
			}
			else if (postclass != null)
			{
				itemsite.InvtAcctID = INReleaseProcess.GetAccountDefaults<INPostClass.invtAcctID>(graph, item, site, postclass);
				itemsite.InvtSubID = INReleaseProcess.GetAccountDefaults<INPostClass.invtSubID>(graph, item, site, postclass);
			}
		}

		public static bool DefaultItemReplenishment(PXGraph graph, INItemSite itemsite)
		{			
			if (itemsite == null) return false;

			INItemRep rep = 
			(INItemRep)PXSelect<INItemRep,
				Where<INItemRep.inventoryID, Equal<Current<INItemSite.inventoryID>>,
				And<INItemRep.replenishmentClassID, Equal<Current<INItemSite.replenishmentClassID>>>>>
				.SelectSingleBound(graph, new object[] {itemsite}) 
				?? new INItemRep();

			if (itemsite.ReplenishmentPolicyOverride != true)
			{
				itemsite.ReplenishmentPolicyID = rep.ReplenishmentPolicyID;
				itemsite.ReplenishmentMethod = rep.ReplenishmentMethod ?? INReplenishmentMethod.None;
				itemsite.ReplenishmentSource = rep.ReplenishmentSource ?? INReplenishmentSource.None;
				itemsite.ReplenishmentSourceSiteID = rep.ReplenishmentSourceSiteID;				
			}

			if (itemsite.ReplenishmentMethod == INReplenishmentMethod.None)
			{
				itemsite.MaxShelfLifeOverride = false;
				itemsite.LaunchDateOverride = false;
				itemsite.TerminationDateOverride = false;
				itemsite.ServiceLevelOverride = false;
				itemsite.SafetyStockOverride = false;
				itemsite.MinQtyOverride = false;
				itemsite.MaxQtyOverride = false;
			}

			if (itemsite.MaxShelfLifeOverride != true)
				itemsite.MaxShelfLife = rep.MaxShelfLife;

			if (itemsite.LaunchDateOverride != true)
				itemsite.LaunchDate = rep.LaunchDate;

			if (itemsite.TerminationDateOverride != true)
				itemsite.TerminationDate = rep.TerminationDate;

			if (itemsite.SafetyStockOverride != true)
				itemsite.SafetyStock = rep.SafetyStock;

			if(itemsite.ServiceLevelOverride != true)
				itemsite.ServiceLevel = rep.ServiceLevel;

			if (itemsite.MinQtyOverride != true)
				itemsite.MinQty = rep.MinQty;

			if (itemsite.MaxQtyOverride != true)
				itemsite.MaxQty = rep.MaxQty;

			if (itemsite.TransferERQOverride != true)
				itemsite.TransferERQ = rep.TransferERQ;

			return
				itemsite.ReplenishmentPolicyOverride != true ||
				itemsite.SafetyStockOverride != true ||
				itemsite.MaxShelfLifeOverride != true ||
				itemsite.LaunchDateOverride != true ||
				itemsite.TerminationDateOverride != true ||
				itemsite.ServiceLevelOverride != true ||
				itemsite.MinQtyOverride != true ||
				itemsite.MaxQtyOverride != true ||
				itemsite.TransferERQOverride != true;

		}

		public static void DefaultSubItemReplenishment(PXGraph graph, INItemSite itemsite)
		{
			if (itemsite == null) return;

			foreach (INItemSiteReplenishment r in 
				PXSelect<INItemSiteReplenishment,
						Where<INItemSiteReplenishment.siteID, Equal<Current<INItemSite.siteID>>,
						  And<INItemSiteReplenishment.inventoryID, Equal<Current<INItemSite.inventoryID>>>>>
							.SelectMultiBound(graph, new object[] {itemsite}))
			{
				graph.Caches[typeof(INItemSiteReplenishment)].Delete(r);	
			}				

			foreach (INSubItemRep r in
				PXSelect<INSubItemRep,
				Where<INSubItemRep.inventoryID, Equal<Current<INItemSite.inventoryID>>,
					And<INSubItemRep.replenishmentClassID, Equal<Current<INItemSite.replenishmentClassID>>>>>
				.Select(graph,itemsite.InventoryID))
			{
				INItemSiteReplenishment sr = new INItemSiteReplenishment();
				sr.InventoryID = r.InventoryID;
				sr.SiteID = itemsite.SiteID;
				sr.SubItemID = r.SubItemID;
				sr.SafetyStock = r.SafetyStock;
				sr.MinQty = r.MinQty;
				sr.MaxQty = r.MaxQty;
				sr.TransferERQ = r.TransferERQ;
				sr.ItemStatus = r.ItemStatus;
				graph.Caches[typeof(INItemSiteReplenishment)].Insert(sr);
			}
			if (graph.Caches[typeof(INItemSiteReplenishment)].IsDirty)
				graph.Views.Caches.Add(typeof(INItemSiteReplenishment));			
		}

		protected virtual void INItemSite_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INItemSite itemsite = (INItemSite)e.Row;
			INItemSite olditemsite = (INItemSite)e.OldRow;
			InventoryItem item = itemrecord.Current;

			bool costOverrideWasDisabled = olditemsite.StdCostOverride == true && itemsite.StdCostOverride == false;
			bool costOverrideWasEnabled = olditemsite.StdCostOverride == false && itemsite.StdCostOverride == true;

			if (costOverrideWasDisabled && (item.PendingStdCostDate != null || item.StdCost == itemsite.StdCost))
			{
				itemsite.PendingStdCost = item.PendingStdCost;
				itemsite.PendingStdCostDate = item.PendingStdCostDate;
			}
			else if (costOverrideWasDisabled)
			{
				itemsite.PendingStdCost = item.StdCost;
				itemsite.PendingStdCostDate = null;
				itemsite.PendingStdCostReset = true;
			}
			else if (costOverrideWasEnabled)
			{
				itemsite.PendingStdCostReset = false;
			}

			if (olditemsite.BasePriceOverride == true && itemsite.BasePriceOverride == false)
			{
				itemsite.BasePrice = item.BasePrice;
			}

			if (olditemsite.ABCCodeOverride == true && itemsite.ABCCodeOverride == false)
			{
				itemsite.ABCCodeID = item.ABCCodeID;
				itemsite.ABCCodeIsFixed = item.ABCCodeIsFixed;
			}

			if (olditemsite.MovementClassOverride == true && itemsite.MovementClassOverride == false)
			{
				itemsite.ABCCodeID = item.MovementClassID;
				itemsite.ABCCodeIsFixed = item.MovementClassIsFixed;
			}

			if (olditemsite.RecPriceOverride == true && itemsite.RecPriceOverride == false)
			{
				itemsite.RecPrice = item.RecPrice;
			}
			if (olditemsite.MarkupPctOverride == true && itemsite.MarkupPctOverride == false)
			{
				itemsite.MarkupPct = item.MarkupPct;
			}
			if (olditemsite.PreferredVendorOverride == true && itemsite.PreferredVendorOverride == false)
			{
				itemsite.PreferredVendorID = item.PreferredVendorID;
				itemsite.PreferredVendorLocationID = item.PreferredVendorLocationID;

				ClearInsertedVendorInventories();
			}		

			INItemSiteMaint.DefaultItemReplenishment(this, itemsite);

			foreach (ItemStats stats in itemstats.Cache.Inserted)
			{
				itemstats.Cache.Delete(stats);
			}

            if (itemsite.PreferredVendorID == null)
            {
                itemsite.PreferredVendorLocationID = null;
            }

			if (itemsite.LastCostDate != null && itemsite.LastCost != null && itemsite.LastCost != 0m)
			{
				ItemStats stats = new ItemStats();
				stats.InventoryID = itemsite.InventoryID;
				stats.SiteID = itemsite.SiteID;

				stats = itemstats.Insert(stats);

				stats.LastCost = itemsite.LastCost;
                stats.LastCostDate = DateTime.Now;
            }
			if((olditemsite.ReplenishmentClassID != itemsite.ReplenishmentClassID && itemsite.SubItemOverride != true) ||
				 (olditemsite.SubItemOverride == true && itemsite.SubItemOverride == false))
				DefaultSubItemReplenishment(this, itemsite);			
			
			if(itemsite.PreferredVendorOverride == true && item.DefaultSubItemID != null &&
				(itemsite.PreferredVendorID != olditemsite.PreferredVendorID ||
				 itemsite.PreferredVendorLocationID != olditemsite.PreferredVendorLocationID))
			{
				if (itemsite.PreferredVendorID == null)
				{
					ClearInsertedVendorInventories();
				}
				else
				{
				POVendorInventory rec = PXSelect<POVendorInventory,
					Where<POVendorInventory.inventoryID, Equal<Current<INItemSite.inventoryID>>,
						And<POVendorInventory.subItemID, Equal<Current<InventoryItem.defaultSubItemID>>,
						And<POVendorInventory.purchaseUnit, Equal<Current<InventoryItem.purchaseUnit>>,
						And<POVendorInventory.vendorID, Equal<Current<INItemSite.preferredVendorID>>,
						And<Where2<Where<Current<INItemSite.preferredVendorLocationID>, IsNotNull, 
										And<POVendorInventory.vendorLocationID, Equal<Current<INItemSite.preferredVendorLocationID>>>>,
						  Or<Where<Current<INItemSite.preferredVendorLocationID>, IsNull, 
										 And<POVendorInventory.vendorLocationID, IsNull>>>>>>>>>>
						.SelectSingleBound(this, new object[] { itemsite, item });
				
					if (rec == null)
				{
						ClearInsertedVendorInventories();

					rec = new POVendorInventory();
					rec.InventoryID = item.InventoryID;
					rec.SubItemID = item.DefaultSubItemID;
					rec.PurchaseUnit = item.PurchaseUnit;
					rec.VendorID = itemsite.PreferredVendorID;
					rec.VendorLocationID = itemsite.PreferredVendorLocationID;					
					this.PreferredVendorItem.Insert(rec);			
				}
			}
		}
		}

		private void ClearInsertedVendorInventories()
		{
			foreach (POVendorInventory vi in PreferredVendorItem.Cache.Inserted)
				PreferredVendorItem.Cache.Delete(vi);
		}

		protected virtual void INItemSite_dfltReceiptLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem item = itemrecord.Current;
			INItemSite itemSite = (INItemSite)e.Row;
			if (item != null && itemSite != null && itemSite.SiteID == item.DfltSiteID)
			{
				itemrecord.Cache.SetValueExt<InventoryItem.dfltReceiptLocationID>(item, itemSite.DfltReceiptLocationID);
				itemrecord.Cache.Update(item);
			}
		}

		protected virtual void INItemSite_dfltShipLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem item = itemrecord.Current;
			INItemSite itemSite = (INItemSite)e.Row;
			if (item != null && itemSite != null && itemSite.SiteID == item.DfltSiteID)
			{
				itemrecord.Cache.SetValueExt<InventoryItem.dfltShipLocationID>(item, itemSite.DfltShipLocationID);
				itemrecord.Cache.Update(item);
			}
        }

        protected virtual void INItemSite_LastCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ((INItemSite)e.Row).LastCostDate = DateTime.Now;
        }
        
		protected virtual void INItemSite_ReplenishmentSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INItemSite row = e.Row as INItemSite;
			if (row == null) return;
			if (row.ReplenishmentSource == INReplenishmentSource.PurchaseToOrder || row.ReplenishmentSource == INReplenishmentSource.DropShipToOrder)
			{
				sender.SetValueExt<INItemSite.replenishmentMethod>(row, INReplenishmentMethod.None);
			}
		}
		
		protected virtual void INItemSite_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            INItemSite row = (INItemSite)e.Row;
			
			bool isTransfer = (e.Row != null) && INReplenishmentSource.IsTransfer(row.ReplenishmentSource);
			bool isFixedReorderQty = (e.Row != null) && (row.ReplenishmentMethod == INReplenishmentMethod.FixedReorder);
			PXUIFieldAttribute.SetEnabled<INItemSite.pendingStdCost>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).StdCostOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.pendingStdCostDate>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).StdCostOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.pendingBasePrice>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).BasePriceOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.pendingBasePriceDate>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).BasePriceOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.aBCCodeID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ABCCodeOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.aBCCodeIsFixed>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ABCCodeOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.movementClassID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).MovementClassOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.movementClassIsFixed>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).MovementClassOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.preferredVendorID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).PreferredVendorOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.preferredVendorLocationID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).PreferredVendorOverride));

			bool ReplenishmentPolicyOverride = row != null && (bool)row.ReplenishmentPolicyOverride;

			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentPolicyID>(sender, e.Row, ReplenishmentPolicyOverride);
			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentSource>(sender, e.Row, ReplenishmentPolicyOverride);
			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentMethod>(sender, e.Row, (ReplenishmentPolicyOverride && row.ReplenishmentSource != INReplenishmentSource.PurchaseToOrder && row.ReplenishmentSource != INReplenishmentSource.DropShipToOrder));
			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentSourceSiteID>(sender, e.Row,
				(ReplenishmentPolicyOverride && (row.ReplenishmentSource == INReplenishmentSource.PurchaseToOrder || row.ReplenishmentSource == INReplenishmentSource.DropShipToOrder || row.ReplenishmentSource == INReplenishmentSource.Transfer || row.ReplenishmentSource == INReplenishmentSource.Purchased)));

            PXUIFieldAttribute.SetRequired<INItemSite.replenishmentSourceSiteID>(sender, isTransfer);

			bool ReplenishmentMethodNone = row != null && row.ReplenishmentMethod != INReplenishmentMethod.None;

			#region Override Replenishment CheckBox SetEnabled
			PXUIFieldAttribute.SetEnabled<INItemSite.maxShelfLifeOverride>(sender, e.Row, ReplenishmentMethodNone);
			PXUIFieldAttribute.SetEnabled<INItemSite.launchDateOverride>(sender, e.Row, ReplenishmentMethodNone);
			PXUIFieldAttribute.SetEnabled<INItemSite.terminationDateOverride>(sender, e.Row, ReplenishmentMethodNone);
			PXUIFieldAttribute.SetEnabled<INItemSite.serviceLevelOverride>(sender, e.Row, ReplenishmentMethodNone);
			PXUIFieldAttribute.SetEnabled<INItemSite.safetyStockOverride>(sender, e.Row, ReplenishmentMethodNone);
			PXUIFieldAttribute.SetEnabled<INItemSite.minQtyOverride>(sender, e.Row, ReplenishmentMethodNone);
			PXUIFieldAttribute.SetEnabled<INItemSite.maxQtyOverride>(sender, e.Row, ReplenishmentMethodNone);
			#endregion

			PXUIFieldAttribute.SetEnabled<INItemSite.maxShelfLife>(sender, e.Row, (row != null && row.MaxShelfLifeOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.launchDate>(sender, e.Row, (row != null && row.LaunchDateOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.terminationDate>(sender, e.Row, (row != null && row.TerminationDateOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.serviceLevel>(sender, e.Row, (row != null && row.ServiceLevelOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.serviceLevelPct>(sender, e.Row, (row != null && row.ServiceLevelOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.safetyStock>(sender, e.Row, (row != null && row.SafetyStockOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.minQty>(sender, e.Row, (row != null && row.MinQtyOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.maxQty>(sender, e.Row, (row != null && row.MaxQtyOverride == true && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.transferERQ>(sender, e.Row, (row != null && row.TransferERQOverride == true && isTransfer && isFixedReorderQty && ReplenishmentMethodNone));
			PXUIFieldAttribute.SetEnabled<INItemSite.transferERQOverride>(sender, e.Row, (row != null && isTransfer && isFixedReorderQty));
			PXUIFieldAttribute.SetEnabled<INItemSite.markupPct>(sender, e.Row, (row != null && row.MarkupPctOverride == true));
			PXUIFieldAttribute.SetEnabled<INItemSite.recPrice>(sender, e.Row, (row != null && row.RecPriceOverride == true));

		
			this.subitemrecords.Cache.AllowInsert =
				this.subitemrecords.Cache.AllowUpdate =
				this.subitemrecords.Cache.AllowDelete = ((INItemSite)e.Row).SubItemOverride == true;
			this.updateReplenishment.SetEnabled(this.subitemrecords.Cache.AllowInsert);

		    if (String.Equals(INReplenishmentSource.Transfer, row.ReplenishmentSource) && row.ReplenishmentSourceSiteID == null)
		    {
		        sender.RaiseExceptionHandling<INItemSite.replenishmentSourceSiteID>(e.Row, row.ReplenishmentSourceSiteID,
		            new PXSetPropertyException(IN.Messages.ReplenishmentSourceSiteRequiredInTransfer, PXErrorLevel.Error));
            } else if (isTransfer && row.ReplenishmentSourceSiteID == row.SiteID)
			{
				sender.RaiseExceptionHandling<INItemSite.replenishmentSourceSiteID>(e.Row,row.ReplenishmentSourceSiteID,new PXSetPropertyException(Messages.ReplenishmentSourceSiteMustBeDifferentFromCurrenSite, PXErrorLevel.Warning)); 
			}
			else 
			{
				sender.RaiseExceptionHandling<INItemSite.replenishmentSourceSiteID>(e.Row,row.ReplenishmentSourceSiteID,null); 
			}
			this.itemrecord.Cache.IsDirty = false;

			if (row != null && row.InvtAcctID == null)
			{
				try
				{
					INItemSiteMaint.DefaultInvtAcctSub(this, row, itemrecord.Current, insite.Current, postclass.Current);
				}
				catch (PXMaskArgumentException) { }
			}
		}

		public virtual void INItemSite_InvtAcctID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if ((bool?)sender.GetValueOriginal<INItemSite.overrideInvtAcctSub>(e.Row) == true && ((INItemSite)e.Row).OverrideInvtAcctSub != true)
				{
					sender.SetValue<INItemSite.invtAcctID>(e.Row, null);
					e.Value = null;
					return;
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (((INItemSite)e.Row).OverrideInvtAcctSub != true)
				{
				    e.ExcludeFromInsertUpdate();

				}
			}
		}

		public virtual void INItemSite_InvtSubID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if ((bool?)sender.GetValueOriginal<INItemSite.overrideInvtAcctSub>(e.Row) == true && ((INItemSite)e.Row).OverrideInvtAcctSub != true)
				{
					sender.SetValue<INItemSite.invtSubID>(e.Row, null);
					e.Value = null;
					return;
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (((INItemSite)e.Row).OverrideInvtAcctSub != true)
				{
				    e.ExcludeFromInsertUpdate();
				}
			}
		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			if (cacheType == typeof(INUnit) && operation == PXDBOperation.Update)
			{
				base.Persist(cacheType, PXDBOperation.Insert);
			}
			return base.Persist(cacheType, operation);
		}

		public override void Persist()
		{
			foreach (INItemSiteReplenishment repl in subitemrecords.Cache.Inserted)
			{
				sitestatus.Insert(new SiteStatus { InventoryID = repl.InventoryID, SubItemID = repl.SubItemID,
					SiteID = repl.SiteID, PersistEvenZero = true });
			}

			base.Persist();
		}

		protected virtual void INItemSite_DfltShipLocationID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;

			INItemSite is_row = (INItemSite)e.Row;

			INLocation l = INLocation.PK.Find(this, (int?)e.NewValue);
			if (l == null) return;
			if (!(l.SalesValid ?? true))
			{
				if (itemsiterecord.Ask(AP.Messages.Warning, Messages.IssuesAreNotAllowedFromThisLocationContinue, MessageButtons.YesNo, false) == WebDialogResult.No)
				{
					e.NewValue = is_row.DfltShipLocationID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void INItemSiteReplenishment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemSiteReplenishment row = (INItemSiteReplenishment)e.Row;
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.safetyStockSuggested>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.minQtySuggested>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.maxQtySuggested>(sender, null, false);			
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.demandPerDayAverage>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.demandPerDayMSE>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.demandPerDayMAD>(sender, null, false);		
		}
		public PXAction<INItemSite> updateReplenishment;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = "Default Settings", MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable UpdateReplenishment(PXAdapter adapter)
		{
			foreach (PXResult<INItemSite> r in adapter.Get())
			{
				INItemSite s = r;
				if (s.SubItemOverride == true && insetup.Current.UseInventorySubItem == true)
					foreach (INItemSiteReplenishment rep in this.subitemrecords.View.SelectMulti(new object[]{s}))
					{
						INItemSiteReplenishment upd = PXCache<INItemSiteReplenishment>.CreateCopy(rep);
						upd.SafetyStock = s.SafetyStock ?? 0m;
						upd.MinQty = s.MinQty ?? 0m;
						upd.MaxQty = s.MaxQty ?? 0m;
						upd.TransferERQ = s.TransferERQ ?? 0m;
						this.subitemrecords.Update(upd);
					}
				yield return s;
			}
		}

		protected virtual void _(Events.FieldUpdated<INItemSite, INItemSite.productManagerOverride> eventArguments)
		{
			if (eventArguments.Row != null &&
				eventArguments.Row.ProductManagerOverride != true)
			{
				itemsiterecord.Cache.SetDefaultExt<INItemSite.productWorkgroupID>(eventArguments.Row);
				itemsiterecord.Cache.SetDefaultExt<INItemSite.productManagerID>(eventArguments.Row);
			}
		}
	}
}
