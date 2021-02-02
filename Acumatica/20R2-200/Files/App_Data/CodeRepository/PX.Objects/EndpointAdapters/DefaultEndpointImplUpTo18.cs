using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.IN;
using System.Linq;
using System;
using System.Collections.Generic;
using PX.Common;

namespace PX.Objects.EndpointAdapters
{
	[PXInternalUseOnly]
	[PXVersion("5.30.001", "Default")]
	[PXVersion("6.00.001", "Default")]
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	public class DefaultEndpointImplUpTo18 : DefaultEndpointImpl
	{
		protected void ItemWarehouse_SetProductManagerFields(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var productManagerValueField = (EntityValueField)targetEntity.Fields.SingleOrDefault(f => f.Name.OrdinalEquals("ProductManager"));
			var productWorkgroupValueField = (EntityValueField)targetEntity.Fields.SingleOrDefault(f => f.Name.OrdinalEquals("ProductWorkgroup"));

			if (productManagerValueField == null && productWorkgroupValueField == null) return;

			var maint = (INItemSiteMaint)graph;

			var inventoryCD = entity.InternalKeys[nameof(maint.itemsiterecord)][nameof(INItemSite.InventoryID)];
			InventoryItem inventory = null;
			if (maint.Caches<InventoryItem>().Locate(new Dictionary<string, object> { { nameof(InventoryItem.InventoryCD), inventoryCD } }) > 0)
				inventory = maint.Caches<InventoryItem>().Current as InventoryItem;

			var siteCD = entity.InternalKeys[nameof(maint.itemsiterecord)][nameof(INItemSite.SiteID)];
			var site = (INSite)new PXSelectReadonly<INSite,
				Where<INSite.siteCD, Equal<Required<INSite.siteCD>>>>(graph).Select(siteCD);

			INItemSite row = maint.itemsitesettings.Select(inventory.InventoryID, site.SiteID);
			if (row == null)
			{
				var cache = maint.itemsiterecord.Cache;
				row = cache.CreateInstance() as INItemSite;
				row.InventoryID = inventory.InventoryID;
				row.SiteID = site.SiteID;
				row = maint.itemsiterecord.Insert(row);
			}

			bool productManagerValueExists = row.ProductManagerID != null;
			if (productManagerValueField != null)
				productManagerValueExists = !string.IsNullOrEmpty(productManagerValueField.Value);

			bool productWorkgroupValueExists = row.ProductWorkgroupID != null;
			if (productWorkgroupValueField != null)
				productWorkgroupValueExists = !string.IsNullOrEmpty(productWorkgroupValueField.Value);

			bool productManagerOverride = productManagerValueExists || productWorkgroupValueExists;

			maint.itemsitesettings.Cache.SetValueExt<INItemSite.productManagerOverride>(row, productManagerOverride);

			if (productManagerValueField != null)
				maint.itemsitesettings.Cache.SetValueExt<INItemSite.productManagerID>(row, productManagerValueField.Value);

			if (productWorkgroupValueField != null)
				maint.itemsitesettings.Cache.SetValueExt<INItemSite.productWorkgroupID>(row, productWorkgroupValueField.Value);

			maint.itemsitesettings.Update(row);
		}

		[FieldsProcessed(new[] {
			"ProductManager",
			"ProductWorkgroup"
			})]
		protected void ItemWarehouse_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
			=> ItemWarehouse_SetProductManagerFields(graph, entity, targetEntity);

		[FieldsProcessed(new[] {
			"ProductManager",
			"ProductWorkgroup"
			})]
		protected void ItemWarehouse_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
			=> ItemWarehouse_SetProductManagerFields(graph, entity, targetEntity);

	}
}
