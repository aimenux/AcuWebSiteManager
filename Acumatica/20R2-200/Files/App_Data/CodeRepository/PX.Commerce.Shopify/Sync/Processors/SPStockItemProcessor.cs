using Newtonsoft.Json;
using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using PX.Api.ContractBased.Models;
using Serilog.Context;

namespace PX.Commerce.Shopify
{
	public class SPStockItemEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public MappedStockItem Product;
		public Dictionary<string, long?> VariantMappings = new Dictionary<string, long?>();
	}

	public class SPStockItemRestrictor : BCBaseRestrictor, IRestrictor
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

				return null;
			});
			#endregion
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped)
		{
			return null;
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.StockItem, BCCaptions.StockItem,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.InventoryItemMaint),
		ExternTypes = new Type[] { typeof(ProductData) },
		LocalTypes = new Type[] { typeof(StockItem) },
		AcumaticaPrimaryType = typeof(PX.Objects.IN.InventoryItem),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.IN.InventoryItem.inventoryCD, Where<PX.Objects.IN.InventoryItem.stkItem, Equal<True>>>),
		URL = "products/{0}"
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Stocks" })]
	public class SPStockItemProcessor : BCProcessorSingleBase<SPStockItemProcessor, SPStockItemEntityBucket, MappedStockItem>, IProcessor
	{
		protected IParentRestDataProvider<ProductData> productDataProvider;
		protected IChildRestDataProvider<ProductVariantData> productVariantDataProvider;
		private IEnumerable<ProductVariantData> ExternProductVariantData = new List<ProductVariantData>();
		private Dictionary<int, string> SalesCategories;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			productDataProvider = new ProductRestDataProvider(client);
			productVariantDataProvider = new ProductVariantRestDataProvider(client);

			SalesCategories = new Dictionary<int, string>();
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

			MappedStockItem obj = new MappedStockItem(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));

			return obj;
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			//No DateTime filtering for Category
			FilterProducts filter = new FilterProducts {
				UpdatedAtMin = minDateTime == null ? (DateTime?)null : minDateTime.Value.ToLocalTime().AddSeconds(-GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0),
				UpdatedAtMax = maxDateTime == null ? (DateTime?)null : maxDateTime.Value.ToLocalTime()
			};

			IEnumerable<ProductData> datas = productDataProvider.GetAll(filter);

			if (datas?.Count() > 0)
			{
				foreach (ProductData data in datas)
				{
					SPStockItemEntityBucket bucket = CreateBucket();

					MappedStockItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
					if(data.Variants?.Count > 0)
					{
						data.Variants.ForEach(x => { bucket.VariantMappings[x.Sku] = x.Id; });
					}
				}
			}
		}
		public override EntityStatus GetBucketForImport(SPStockItemEntityBucket bucket, BCSyncStatus syncstatus)
		{
			ProductData data = productDataProvider.GetByID(syncstatus.ExternID);
			if (data == null) return EntityStatus.None;

			if (data.Variants?.Count > 0)
			{
				data.Variants.ForEach(x =>
				{
					if (bucket.VariantMappings.ContainsKey(x.Sku))
						bucket.VariantMappings[x.Sku] = x.Id;
					else
						bucket.VariantMappings.Add(x.Sku, x.Id);
				});
			}
			MappedStockItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}

		public override void MapBucketImport(SPStockItemEntityBucket bucket, IMappedEntity existing)
		{
			
		}
		public override void SaveBucketImport(SPStockItemEntityBucket bucket, IMappedEntity existing, String operation)
		{
			
		}
		#endregion

		#region Export
		public override IEnumerable<MappedStockItem> PullSimilar(ILocalEntity entity, out string uniqueField)
		{
			var uniqueStr = uniqueField = ((StockItem)entity)?.InventoryID?.Value;

            IEnumerable<ProductData> datas = null;
			if (!string.IsNullOrEmpty(uniqueStr))
			{
				if(ExternProductVariantData == null || ExternProductVariantData.Count() == 0 || !ExternProductVariantData.Any(x => x.Sku.Equals(uniqueStr, StringComparison.InvariantCultureIgnoreCase)))
					ExternProductVariantData = productVariantDataProvider.GetAllWithoutParent(new FilterWithFields() { Fields = "id,product_id,sku,title" });
				var existedItems = ExternProductVariantData?.Where(x => x.Sku.Equals(uniqueStr, StringComparison.InvariantCultureIgnoreCase));
				if(existedItems != null && existedItems?.Count() > 0)
				{
					var matchedVariants = existedItems.Select(x => x.ProductId).Distinct().ToList();
					if (matchedVariants != null && matchedVariants.Count > 0)
					{
						datas = productDataProvider.GetAll(new FilterProducts() { IDs = string.Join(",", matchedVariants) });
					}
				}
			}
			
			return datas == null ? null : datas.Select(data => new MappedStockItem(data, data.Id.ToString(), data.DateModifiedAt.ToDate(false)));
		}

		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			StockItem item = new StockItem()
			{
				InventoryID = new StringReturn(),
				TemplateItemID = new StringReturn(),
				Categories = new List<CategoryStockItem>() { new CategoryStockItem() { CategoryID = new IntReturn() } },
			};
			IEnumerable<StockItem> impls = cbapi.GetAll<StockItem>(item, minDateTime, maxDateTime, filters, GetCustomFieldsForExport());

			if (impls != null && impls.Count() > 0)
			{
				int countNum = 0;
				List<IMappedEntity> mappedList = new List<IMappedEntity>();
				foreach (StockItem impl in impls)
				{
                    IMappedEntity obj = new MappedStockItem(impl, impl.SyncID, impl.SyncTime);

					mappedList.Add(obj);
					countNum++;
					if (countNum % BatchFetchCount == 0 || countNum == impls.Count())
					{
                        ProcessMappedListForExport(ref mappedList);
                    }
				}
			}
		}
		public override EntityStatus GetBucketForExport(SPStockItemEntityBucket bucket, BCSyncStatus syncstatus)
		{
			StockItem impl = cbapi.GetByID<StockItem>(syncstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			MappedStockItem obj = bucket.Product = bucket.Product.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
			if (!bucket.VariantMappings.ContainsKey(impl.InventoryID?.Value))
				bucket.VariantMappings.Add(impl.InventoryID?.Value, null);
			if (obj.Local.Categories != null)
			{
				foreach (CategoryStockItem item in obj.Local.Categories)
				{
					if (!SalesCategories.ContainsKey(item.CategoryID.Value.Value))
					{
						BCItemSalesCategory implCat = cbapi.Get<BCItemSalesCategory>(new BCItemSalesCategory() { CategoryID = new IntSearch() { Value = item.CategoryID.Value } });
						if (implCat == null) continue;
						if (item.CategoryID.Value != null)
						{
							SalesCategories[item.CategoryID.Value.Value] = implCat.Description.Value;
						}
					}
				}
			}
			return status;
		}

		public override void MapBucketExport(SPStockItemEntityBucket bucket, IMappedEntity existing)
		{
			MappedStockItem obj = bucket.Product;

			StockItem impl = obj.Local;
			ProductData data = obj.Extern = new ProductData();

			data.Title = impl.Description?.Value;
			data.BodyHTML = ClearHTMLContent(impl.Content?.Value);
			data.ProductType = impl.ItemClass.Value;
			data.Vendor = (impl.VendorDetails?.FirstOrDefault(v => v.Default?.Value == true)?? impl.VendorDetails?.FirstOrDefault())?.VendorName?.Value;
			//Put all categories to the Tags
			var categories = impl.Categories?.Select(x => { if (SalesCategories.TryGetValue(x.CategoryID.Value.Value, out var desc)) return desc; else return string.Empty; }).Where(x => !string.IsNullOrEmpty(x)).ToList();
			if(categories != null && categories.Count > 0)
				data.Categories = categories;
			if (!string.IsNullOrEmpty(impl.SearchKeywords?.Value))
				data.Tags = impl.SearchKeywords?.Value;
			if (!string.IsNullOrEmpty(impl.MetaKeywords?.Value))
				data.GlobalTitleTag = impl.MetaKeywords?.Value;
			if (!string.IsNullOrEmpty(impl.MetaDescription?.Value))
				data.GlobalDescriptionTag = impl.MetaDescription?.Value;
            string availability = impl.Availability?.Value;
            if (availability == null || availability == BCCaptions.StoreDefault) availability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
            data.Published = impl.Visibility?.Value == BCCaptions.Invisible
                || impl.ItemStatus?.Value != PX.Objects.IN.Messages.Active
                || availability == BCCaptions.Disabled ? false : true;
            if (!string.IsNullOrEmpty(obj.ExternID))
			{
				data.Id = obj.ExternID.ToLong();
			}
			else
			{
				data.Metafields = new List<MetafieldData>() { new MetafieldData() { Key = ShopifyCaptions.Product, Value = impl.NoteID.Value.ToString(), ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global } };
				data.Metafields.Add(new MetafieldData() { Key = nameof(ProductTypes), Value = BCCaptions.StockItem, ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global });
			}
			/////
			
			data.Variants = data.Variants ?? new List<ProductVariantData>();
			bool isTaxable;
			bool.TryParse(GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxCategorySubstitutionListID, impl.TaxCategory?.Value, String.Empty), out isTaxable);
			data.Variants.Add(new ProductVariantData()
			{
				Id = bucket.VariantMappings.ContainsKey(impl.InventoryID?.Value) ? bucket.VariantMappings[impl.InventoryID?.Value] : null,
				Title = impl.Description?.Value,
				Price = impl.DefaultPrice.Value,
				Sku = impl.InventoryID?.Value,
				OriginalPrice = impl.MSRP?.Value,
				Barcode = !string.IsNullOrWhiteSpace(impl.BaseUOM?.Value) ? ((impl.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == "Barcode" && x.UOM?.Value == impl.BaseUOM?.Value) ??
					impl.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == "Barcode" && string.IsNullOrWhiteSpace(x.UOM?.Value)))?.AlternateID?.Value) : null,
				Weight = impl.DimensionWeight?.Value,
				WeightUnit = impl.WeightUOM?.Value?.ToLower(),
				Taxable = isTaxable,
				InventoryPolicy = InventoryPolicy.Continue,
				InventoryManagement = availability == BCCaptions.AvailableTrack ? ShopifyCaptions.InventoryManagement_Shopify : null,
				Metafields = bucket.VariantMappings.ContainsKey(impl.InventoryID?.Value) ? null :
					new List<MetafieldData>() { new MetafieldData() { Key = ShopifyCaptions.Variant, Value = impl.NoteID.Value.ToString(), ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global } }
			});
		}

		public override object GetAttribute(SPStockItemEntityBucket bucket, string attributeID)
		{
			MappedStockItem obj = bucket.Product;
			StockItem impl = obj.Local;
			return impl.Attributes?.Where(x => string.Equals(x?.AttributeID?.Value,attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
		}

		public override void AddAttributeValue(SPStockItemEntityBucket bucket, string attributeID, object attributeValue)
		{
			MappedStockItem obj = bucket.Product;
			StockItem impl = obj.Local;
			impl.Attributes = impl.Attributes ?? new List<AttributeValue>();
			AttributeValue attributeDetail = new AttributeValue();
			attributeDetail.AttributeID = new StringValue() { Value = attributeID };
			attributeDetail.Value = new StringValue() { Value = attributeValue.ToString() };
			impl.Attributes.Add(attributeDetail);
		}

		public override void SaveBucketExport(SPStockItemEntityBucket bucket, IMappedEntity existing, String operation)
		{
			MappedStockItem obj = bucket.Product;
			ProductData data = null;
			if(obj.Extern.Categories?.Count > 0 && GetBindingExt<BCBindingShopify>()?.CombineCategoriesToTags == BCSalesCategoriesExportAttribute.ExportAsTags)
			{
				obj.Extern.Tags = string.Join(",", obj.Extern.Categories) + (string.IsNullOrEmpty(obj.Extern.Tags) ? "" : $",{obj.Extern.Tags}");
			}
			try
			{
				if (obj.ExternID == null)
					data = productDataProvider.Create(obj.Extern);
				else
				{
					var skus = obj.Extern.Variants.Select(x => x.Sku).ToList();
					var notExistedVariantIds = bucket.VariantMappings.Where(x => !skus.Contains(x.Key)).Select(x => x.Value).ToList();
					data = productDataProvider.Update(obj.Extern, obj.ExternID);
					if (notExistedVariantIds?.Count > 0)
					{
						notExistedVariantIds.ForEach(x =>
						{
							if (x != null) productVariantDataProvider.Delete(obj.ExternID, x.ToString());
						});
					}
				}
			}
			catch (Exception ex)
			{
				throw new PXException(ex.Message);
			}

			obj.AddExtern(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
			if(data.Variants?.Count > 0)
			{
				data.Variants.ForEach(x => {
					obj.AddDetail(BCEntitiesAttribute.Variant, obj.LocalID.Value, x.Id.ToString());
				});
			}
			UpdateStatus(obj, operation);
		}
		#endregion
	}
}