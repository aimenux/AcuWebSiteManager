using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Objects.Substitutes;
using PX.Data;
using Serilog.Context;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce
{
	public class BCStockItemEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public override IMappedEntity[] PreProcessors { get => Categories.ToArray(); }
		public override IMappedEntity[] PostProcessors { get => StockItems.ToArray(); }

		public MappedStockItem Product;
						
		public List<MappedCategory> Categories = new List<MappedCategory>();
		public List<MappedStockItem> StockItems = new List<MappedStockItem>();
	}

	public class BCStockItemRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			#region StockItems
			return base.Restrict<MappedStockItem>(mapped, delegate (MappedStockItem obj)
			{
				if (obj.Local != null && obj.Local.TemplateItemID?.Value != null)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogStockSkippedVariant, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
				}

				if (obj.Local != null && (obj.Local.Categories == null || obj.Local.Categories.Count <= 0) && processor.GetBindingExt<BCBindingExt>().StockSalesCategoriesIDs == null)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogItemSkippedNoCategories, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
				}

				return null;
			});
			#endregion
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped)
		{
			return null;
		}
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.StockItem, BCCaptions.StockItem,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.InventoryItemMaint),
		ExternTypes = new Type[] { typeof(ProductData) },
		LocalTypes = new Type[] { typeof(StockItem) },
		DetailTypes = new String[] { BCEntitiesAttribute.ProductVideo, BCCaptions.ProductVideo },
		AcumaticaPrimaryType = typeof(PX.Objects.IN.InventoryItem),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.IN.InventoryItem.inventoryCD, Where<PX.Objects.IN.InventoryItem.stkItem, Equal<True>>>),
		URL = "products/{0}/edit",
		Requires = new string[] { BCEntitiesAttribute.SalesCategory }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Stocks" },
		WebHookType = typeof(WebHookProduct),
		WebHooks = new String[]
		{
			"store/product/created",
			"store/product/updated",
			"store/product/deleted"
		})]
	public class BCStockItemProcessor : BCProductProcessor<BCStockItemProcessor, BCStockItemEntityBucket, MappedStockItem>, IProcessor
	{
		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
		}
		#endregion

		#region Common
		public override MappedStockItem PullEntity(Guid? localID, Dictionary<string, object> externalInfo)
		{
			StockItem impl = cbapi.GetByID<StockItem>(localID);
			if (impl == null) return null;

			MappedStockItem obj = new MappedStockItem(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override MappedStockItem PullEntity(String externID, String externalInfo)
		{
			ProductData data = productDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedStockItem obj = new MappedStockItem(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());

			return obj;
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			//No DateTime filtering for Category
			FilterProducts filter = new FilterProducts { Type = ProductTypes.physical.ToString(),
				MinDateModified = minDateTime == null ? null : minDateTime,
			MaxDateModified = maxDateTime == null ? null : maxDateTime};

			IEnumerable<ProductData> datas = productDataProvider.GetAll(filter);

			foreach (ProductData data in datas)
			{
				BCStockItemEntityBucket bucket = CreateBucket();

				MappedStockItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
			}
		}
		public override EntityStatus GetBucketForImport(BCStockItemEntityBucket bucket, BCSyncStatus syncstatus)
		{
			FilterProducts filter = new FilterProducts { Include = "images,modifiers" };
			ProductData data = productDataProvider.GetByID(syncstatus.ExternID, filter);
			if (data == null) return EntityStatus.None;

			MappedStockItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}

		public override void MapBucketImport(BCStockItemEntityBucket bucket, IMappedEntity existing)
		{
			MappedStockItem obj = bucket.Product;

			ProductData data = obj.Extern;
			StringValue tax = obj.Extern?.TaxClassId != null ? GetSubstituteLocalByExtern(
					BCSubstitute.TaxClasses,
					taxClasses?.Find(i => i.Id == obj.Extern?.TaxClassId)?.Name, "").ValueField() :
					obj.Local.TaxCategory;
			StockItem impl = obj.Local = new StockItem();
			impl.Custom = GetCustomFieldsForImport();

			//Product
			impl.InventoryID = GetEntityKey(PX.Objects.IN.InventoryAttribute.DimensionName, data.Name).ValueField();
			impl.Description = data.Name.ValueField();
			impl.ItemClass = obj.LocalID == null || existing?.Local == null ? PX.Objects.IN.INItemClass.PK.Find(this, GetBindingExt<BCBindingExt>().StockItemClassID)?.ItemClassCD.ValueField() : null;
			impl.DefaultPrice = data.Price.ValueField();
			impl.TaxCategory = tax;
			if (data.Categories != null)
				impl.Categories = new List<CategoryStockItem>();
			foreach (int cat in data.Categories)
			{
				PX.Objects.IN.INCategory incategory = PXSelectJoin<PX.Objects.IN.INCategory,
				LeftJoin<BCSyncStatus, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, BCEntitiesAttribute.SalesCategory, cat);

				if (incategory == null || incategory.CategoryID == null) throw new PXException(BCMessages.CategoryNotSyncronizedForItem, data.Name);

				impl.Categories.Add(new CategoryStockItem() { CategoryID = incategory.CategoryID.ValueField() });
			}
		}
		public override void SaveBucketImport(BCStockItemEntityBucket bucket, IMappedEntity existing, String operation)
		{
			MappedStockItem obj = bucket.Product;

			if (existing?.Local != null) obj.Local.InventoryID = ((StockItem)existing.Local).InventoryID.Value.SearchField();

			StockItem impl = cbapi.Put<StockItem>(obj.Local, obj.LocalID);

			bucket.Product.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);
		}
		#endregion

		#region Export
		public override IEnumerable<MappedStockItem> PullSimilar(ILocalEntity entity, out string uniqueField)
		{
			List<ProductData> datas = PullSimilar(((StockItem)entity)?.Description?.Value, ((StockItem)entity)?.InventoryID?.Value, out uniqueField);
			return datas == null ? null : datas.Select(data => new MappedStockItem(data, data.Id.ToString(), data.DateModifiedUT.ToDate()));
		}

		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			BCEntity entity = GetEntity(Operation.EntityType);

			StockItem item = new StockItem()
			{
				InventoryID = new StringReturn(),
				TemplateItemID = new StringReturn(),
				Categories = new List<CategoryStockItem>() { new CategoryStockItem() { CategoryID = new IntReturn() } }
			};
			IEnumerable<StockItem> impls = cbapi.GetAll<StockItem>(item, minDateTime, maxDateTime, filters, GetCustomFieldsForExport());

			if (impls != null && impls.Count() > 0)
			{
				int countNum = 0;
				List<IMappedEntity> mappedList = new List<IMappedEntity>();
				String[] allowedRels = GetBindingExt<BCBindingExt>().RelatedItems?.Split(',');
				Dictionary<string, int> stats = new Dictionary<string, int>();
				foreach (string impl in impls.Select(i => i.InventoryID.Value))
					stats.Add(impl, 0);
				if (allowedRels != null && allowedRels.Count() > 0 && !String.IsNullOrWhiteSpace(allowedRels[0]))
				{
					PXResultset<PX.Objects.IN.InventoryItem, INRelatedInventory> relationsFetched = PXSelectJoin<PX.Objects.IN.InventoryItem,
						InnerJoin<INRelatedInventory, On<INRelatedInventory.inventoryID, Equal<PX.Objects.IN.InventoryItem.inventoryID>>>>
							.Select<PXResultset<PX.Objects.IN.InventoryItem, INRelatedInventory>>(this);
					foreach (PXResult<PX.Objects.IN.InventoryItem, INRelatedInventory> relation in relationsFetched)
						if (stats.ContainsKey(relation.GetItem<PX.Objects.IN.InventoryItem>().InventoryCD.TrimEnd()) && allowedRels.Contains(relation.GetItem<INRelatedInventory>().Relation) &&
							(relation.GetItem<INRelatedInventory>().ExpirationDate == null || relation.GetItem<INRelatedInventory>().ExpirationDate > DateTime.Now))
							stats[relation.GetItem<PX.Objects.IN.InventoryItem>().InventoryCD.TrimEnd()] += 1;
				}
				foreach (string key in stats.OrderBy(i => i.Value).Select(i => i.Key))
				{
					StockItem impl = impls.First(i => i.InventoryID.Value.Equals(key));
                    IMappedEntity obj = new MappedStockItem(impl, impl.SyncID, impl.SyncTime);
					if (entity.EntityType != obj.EntityType)
						entity = GetEntity(obj.EntityType);

					mappedList.Add(obj);
					countNum++;
					if (countNum % BatchFetchCount == 0 || countNum == impls.Count())
					{
                        ProcessMappedListForExport(ref mappedList);
                    }
				}
			}
		}
		public override EntityStatus GetBucketForExport(BCStockItemEntityBucket bucket, BCSyncStatus syncstatus)
		{
			StockItem impl = cbapi.GetByID<StockItem>(syncstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			MappedStockItem obj = bucket.Product = bucket.Product.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			if (obj.Local.Categories != null)
			{
				foreach (CategoryStockItem item in obj.Local.Categories)
				{
					BCSyncStatus result = PXSelectJoin<BCSyncStatus,
						InnerJoin<PX.Objects.IN.INCategory, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
						Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
							And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
							And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							And<PX.Objects.IN.INCategory.categoryID, Equal<Required<PX.Objects.IN.INCategory.categoryID>>>>>>>
						.Select(this, BCEntitiesAttribute.SalesCategory, item.CategoryID.Value);
					if (result != null && result.ExternID != null && result.LocalID != null) continue;

					BCItemSalesCategory implCat = cbapi.Get<BCItemSalesCategory>(new BCItemSalesCategory() { CategoryID = new IntSearch() { Value = item.CategoryID.Value } });
					if (implCat == null) continue;

					MappedCategory mappedCategory = new MappedCategory(implCat, implCat.SyncID, implCat.SyncTime);
					EntityStatus mappedCategoryStatus = EnsureStatus(mappedCategory, SyncDirection.Export);
					if (mappedCategoryStatus == EntityStatus.Deleted)
						throw new PXException(BCMessages.CategoryIsDeletedForItem, item.CategoryID.Value, impl.Description.Value);
					if (mappedCategory.IsNew)
					{
						LogWarning(Operation.LogScope(obj), BCMessages.LogItemCategoryNotSynchronized, 
							implCat.Description?.Value, impl.InventoryID?.Value, mappedCategoryStatus.ToString());
						continue;
					}

					bucket.Categories.Add(mappedCategory);
				}
			}
			return status;
		}

		public override void MapBucketExport(BCStockItemEntityBucket bucket, IMappedEntity existing)
		{
			MappedStockItem obj = bucket.Product;

			StockItem impl = obj.Local;
			ProductData data = obj.Extern = new ProductData();

			//Inventory Item
			data.Name = impl.Description?.Value;
			data.Description = ClearHTMLContent(impl.Content?.Value);
			data.Type = ProductsType.Physical.ToEnumMemberAttrValue();
			data.Price = impl.DefaultPrice.Value;
			data.Weight = impl.DimensionWeight.Value;
			data.CostPrice = impl.CurrentStdCost.Value;
			data.RetailPrice = impl.MSRP.Value;
			data.BinPickingNumber = impl.DefaultIssueLocationID?.Value;
			data.Sku = impl.InventoryID?.Value; 
			data.TaxClassId = taxClasses?.Find(i => i.Name.Equals(GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxCategorySubstitutionListID, impl.TaxCategory?.Value, String.Empty)))?.Id;

			//custom fields mapping
			data.PageTitle = impl.PageTitle?.Value;
			data.MetaDescription = impl.MetaDescription?.Value;
			data.MetaKeywords = impl.MetaKeywords?.Value != null ? impl.MetaKeywords?.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
			data.SearchKeywords = impl.SearchKeywords?.Value;
			var vendor = impl.VendorDetails?.FirstOrDefault(v => v.Default?.Value == true);
			if (vendor != null)
				data.MPN = impl.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == "Vendor Part Number" && x.VendorOrCustomer?.Value == vendor.VendorID?.Value )?.AlternateID?.Value;
			if (!string.IsNullOrWhiteSpace(impl.BaseUOM?.Value))
				data.Upc = (impl.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == "Barcode" && x.UOM?.Value == impl.BaseUOM?.Value ) ??
					impl.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == "Barcode" && string.IsNullOrWhiteSpace(x.UOM?.Value)))?.AlternateID?.Value;
			if (impl.CustomURL?.Value != null) data.CustomUrl = new ProductCustomUrl() { Url = impl.CustomURL?.Value, IsCustomized = true };
			if (obj.IsNew) data.InventoryTracking = "none"; // TODO CHECK
			data.RelatedProducts = MapRelatedItems(obj.Local.InventoryID.Value.TrimEnd());
			switch (impl.Visibility?.Value)
			{
				case BCCaptions.Visible:
					data.IsVisible = true;
					break;
				case BCCaptions.Featured:
					{
						data.IsVisible = true;
						data.IsFeatured = true;
						break;
					}
				case BCCaptions.Invisible:
				default:
					{
						data.IsFeatured = false;
						data.IsVisible = false;
						break;
					}
			}
			Boolean isItemActive = !(impl.ItemStatus?.Value == PX.Objects.IN.Messages.Inactive || impl.ItemStatus?.Value == PX.Objects.IN.Messages.ToDelete || impl.ItemStatus?.Value == PX.Objects.IN.Messages.NoSales);
			data.Availability = "disabled";
			if (isItemActive)
			{
				string availability = impl?.Availability?.Value;
				if (availability == null || availability == BCCaptions.StoreDefault) availability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
				switch (availability)
				{
					case BCCaptions.AvailableTrack: data.Availability = "available"; break;
					case BCCaptions.AvailableSkip: data.Availability = "available"; break;
					case BCCaptions.PreOrder: data.Availability = "preorder"; break;
					case BCCaptions.Disabled: data.Availability = "disabled"; break;
				}
			}


			foreach (PXResult<PX.Objects.IN.INCategory, PX.Objects.IN.INItemCategory, PX.Objects.IN.InventoryItem, BCSyncStatus> result in PXSelectJoin<PX.Objects.IN.INCategory,
					InnerJoin<PX.Objects.IN.INItemCategory, On<PX.Objects.IN.INItemCategory.categoryID, Equal<PX.Objects.IN.INCategory.categoryID>>,
					InnerJoin<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<PX.Objects.IN.INItemCategory.inventoryID>>,
					LeftJoin<BCSyncStatus, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<PX.Objects.IN.InventoryItem.noteID, Equal<Required<PX.Objects.IN.InventoryItem.noteID>>>>>>>
							.Select(this, BCEntitiesAttribute.SalesCategory, obj.LocalID))
			{
				BCSyncStatus status = result.GetItem<BCSyncStatus>();
				if (status == null || status.ExternID == null) throw new PXException(BCMessages.CategoryNotSyncronizedForItem, impl.Description.Value);

				data.Categories.Add(status.ExternID.ToInt().Value);
			}
			if (data.Categories.Count <= 0)
			{
				String categories = GetBindingExt<BCBindingExt>().StockSalesCategoriesIDs;
				if (!String.IsNullOrEmpty(categories))
				{
					Int32?[] categoriesArray = categories.Split(',').Select(c => { return Int32.TryParse(c, out Int32 i) ? (int?)i : null; }).Where(i => i != null).ToArray();

					foreach (BCSyncStatus status in PXSelectJoin<BCSyncStatus,
						LeftJoin<PX.Objects.IN.INCategory, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
						Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
							And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
							And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							And<PX.Objects.IN.INCategory.categoryID, In<Required<PX.Objects.IN.INCategory.categoryID>>>>>>>
							.Select(this, BCEntitiesAttribute.SalesCategory, categoriesArray))
					{
						if (status == null || status.ExternID == null) throw new PXException(BCMessages.CategoryNotSyncronizedForItem, impl.Description.Value);

						if (status != null) data.Categories.Add(status.ExternID.ToInt().Value);
					}
				}
			}
		}

		public override object GetAttribute(BCStockItemEntityBucket bucket, string attributeID)
		{
			MappedStockItem obj = bucket.Product;
			StockItem impl = obj.Local;
			return impl.Attributes?.Where(x => string.Equals(x?.AttributeID?.Value, attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
		public override void AddAttributeValue(BCStockItemEntityBucket bucket, string attributeID, object attributeValue)
		{
			MappedStockItem obj = bucket.Product;
			StockItem impl = obj.Local;
			impl.Attributes = impl.Attributes ?? new List<AttributeValue>();
			AttributeValue attributeDetail = new AttributeValue();
			attributeDetail.AttributeID = new StringValue() { Value = attributeID };
			attributeDetail.Value = new StringValue() { Value = attributeValue.ToString() };
			impl.Attributes.Add(attributeDetail);
		}

		public override void SaveBucketExport(BCStockItemEntityBucket bucket, IMappedEntity existing, String operation)
		{
			MappedStockItem obj = bucket.Product;

			ProductData data = null;
			bool extNotEx = obj.ExternID == null;
			if (obj.ExternID == null)
				data = productDataProvider.Create(obj.Extern);
			else
				data = productDataProvider.Update(obj.Extern, obj.ExternID.ToInt().Value);

			ExportCustomFields(obj, obj.Extern.CustomFields, data);
			obj.AddExtern(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());

			SaveImages(obj, obj.Local?.FileURLs);
			SaveVideos(obj, obj.Local?.FileURLs);

			UpdateStatus(obj, operation);
			if (data != null && extNotEx)
				UpdateRelatedItems(bucket, obj.Local.InventoryID.Value.TrimEnd(), (int)data.Id);
		}
		#endregion
	}
}