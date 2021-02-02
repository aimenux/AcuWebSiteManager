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
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Api.ContractBased.Models;
using Serilog.Context;

namespace PX.Commerce.Shopify
{
	public class SPTemplateItemEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public MappedTemplateItem Product;
		public Dictionary<string, long?> VariantMappings = new Dictionary<string, long?>();
	}

	public class SPTemplateItemRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			#region TemplateItemss
			return base.Restrict<MappedTemplateItem>(mapped, delegate (MappedTemplateItem obj)
			{
				if (obj.Local != null && (obj.Local.Matrix == null || obj.Local.Matrix?.Count == 0))
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogTemplateSkippedNoMatrix, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
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

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.ProductWithVariant, BCCaptions.TemplateItem,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.InventoryItemMaint),
		ExternTypes = new Type[] { typeof(ProductData) },
		LocalTypes = new Type[] { typeof(TemplateItems) },
		DetailTypes = new String[] { BCEntitiesAttribute.Variant, BCCaptions.Variant},
		AcumaticaPrimaryType = typeof(PX.Objects.IN.InventoryItem),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.IN.InventoryItem.inventoryCD, Where<PX.Objects.IN.InventoryItem.isTemplate, Equal<True>>>),
		URL = "products/{0}"
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Variants" })]
	public class SPTemplateItemProcessor : BCProcessorSingleBase<SPTemplateItemProcessor, SPTemplateItemEntityBucket, MappedTemplateItem>, IProcessor
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
		public override MappedTemplateItem PullEntity(Guid? localID, Dictionary<string, object> externalInfo)
		{
			TemplateItems impl = cbapi.GetByID<TemplateItems>(localID);
			if (impl == null) return null;

			MappedTemplateItem obj = new MappedTemplateItem(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override MappedTemplateItem PullEntity(String externID, String externalInfo)
		{
			ProductData data = productDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedTemplateItem obj = new MappedTemplateItem(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));

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
					SPTemplateItemEntityBucket bucket = CreateBucket();

					MappedTemplateItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
					if (data.Variants?.Count > 0)
					{
						data.Variants.ForEach(x => { bucket.VariantMappings[x.Sku] = x.Id; });
					}
				}
			}
		}
		public override EntityStatus GetBucketForImport(SPTemplateItemEntityBucket bucket, BCSyncStatus syncstatus)
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
			MappedTemplateItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}

		public override void MapBucketImport(SPTemplateItemEntityBucket bucket, IMappedEntity existing)
		{
			
		}
		public override void SaveBucketImport(SPTemplateItemEntityBucket bucket, IMappedEntity existing, String operation)
		{
			
		}
		#endregion

		#region Export
		public override IEnumerable<MappedTemplateItem> PullSimilar(ILocalEntity entity, out string uniqueField)
		{
            TemplateItems localEnity = (TemplateItems)entity;
            uniqueField = localEnity?.InventoryID?.Value;
            IEnumerable<ProductData> datas = null;
            List<string> matrixIds = new List<string>();
            if(localEnity?.Matrix?.Count > 0)
            {
                matrixIds = localEnity.Matrix.Select(x => x?.InventoryID?.Value).ToList();
                if (ExternProductVariantData == null || ExternProductVariantData.Count() == 0 || !ExternProductVariantData.Any(x => matrixIds.Any(id => id.Equals(x.Sku, StringComparison.InvariantCultureIgnoreCase))))
                    ExternProductVariantData = productVariantDataProvider.GetAllWithoutParent(new FilterWithFields() { Fields = "id,product_id,sku,title" });
                var existedItems = ExternProductVariantData?.Where(x => matrixIds.Any(id => id.Equals(x.Sku, StringComparison.InvariantCultureIgnoreCase)));
                if (existedItems != null && existedItems?.Count() > 0)
                {
                    var matchedVariants = existedItems.Select(x => x.ProductId).Distinct().ToList();
                    if (matchedVariants != null && matchedVariants.Count > 0)
                    {
                        datas = productDataProvider.GetAll(new FilterProducts() { IDs = string.Join(",", matchedVariants) });
                    }
                }
            }

			return datas == null ? null : datas.Select(data => new MappedTemplateItem(data, data.Id.ToString(), data.DateModifiedAt.ToDate(false)));
		}

		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			TemplateItems item = new TemplateItems()
			{
				InventoryID = new StringReturn(),
				IsStockItem = new BooleanReturn(),
				Matrix = new List<MatrixItems>() { new MatrixItems() { InventoryID = new StringReturn() } },
				Categories = new List<CategoryStockItem>() { new CategoryStockItem() { CategoryID = new IntReturn() } }
			};
			IEnumerable<TemplateItems> impls = cbapi.GetAll<TemplateItems>(item, minDateTime, maxDateTime, filters, GetCustomFieldsForExport());

			if (impls != null && impls.Count() > 0)
			{
				int countNum = 0;
				List<IMappedEntity> mappedList = new List<IMappedEntity>();
				foreach (TemplateItems impl in impls)
				{
                    IMappedEntity obj = new MappedTemplateItem(impl, impl.SyncID, impl.SyncTime);

					mappedList.Add(obj);
					countNum++;
					if (countNum % BatchFetchCount == 0 || countNum == impls.Count())
					{
                        ProcessMappedListForExport(ref mappedList);
                    }
				}
			}
		}
		public override EntityStatus GetBucketForExport(SPTemplateItemEntityBucket bucket, BCSyncStatus syncstatus)
		{
			TemplateItems impl = cbapi.GetByID<TemplateItems>(syncstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null || impl.Matrix?.Count == 0) return EntityStatus.None;

			impl.AttributesDef = new List<AttributeDefinition>();
			impl.AttributesValues = new List<AttributeValue>();
			int? inventoryID = null;
			foreach (PXResult<CSAttribute, CSAttributeGroup, INItemClass, InventoryItem> attributeDef in PXSelectJoin<CSAttribute,
			   InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>,
			   InnerJoin<INItemClass, On<INItemClass.itemClassID, Equal<CSAttributeGroup.entityClassID>>,
			   InnerJoin<InventoryItem, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>,
			  Where<InventoryItem.isTemplate, Equal<True>,
			  And<InventoryItem.noteID, Equal<Required<InventoryItem.noteID>>,
			  And<CSAttribute.controlType, Equal<Required<CSAttribute.controlType>>,
			  And<CSAttributeGroup.isActive, Equal<True>,
			  And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>
			  >>>>>>.Select(this, impl.Id, 2))
			{
				AttributeDefinition def = new AttributeDefinition();
				var inventory = (InventoryItem)attributeDef;
				inventoryID = inventory.InventoryID;
				var attribute = (CSAttribute)attributeDef;
				def.AttributeID = attribute.AttributeID.ValueField();
				def.Description = attribute.Description.ValueField();
				def.NoteID = attribute.NoteID.ValueField();
				def.Values = new List<AttributeDefinitionValue>();
				var attributedetails = PXSelect<CSAttributeDetail, Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>>.Select(this, def.AttributeID.Value);
				foreach (CSAttributeDetail value in attributedetails)
				{
					AttributeDefinitionValue defValue = new AttributeDefinitionValue();
					defValue.NoteID = value.NoteID.ValueField();
					defValue.ValueID = value.ValueID.ValueField();
					defValue.Description = value.Description.ValueField();
					defValue.SortOrder = (value.SortOrder??0).ToInt().ValueField();
					def.Values.Add(defValue);
				}

				if (def != null)
					impl.AttributesDef.Add(def);
			}
			
			foreach (PXResult<InventoryItem, CSAnswers> attributeDef in PXSelectJoin<InventoryItem,
			   InnerJoin<CSAnswers, On<InventoryItem.noteID, Equal<CSAnswers.refNoteID>>>,
			  Where<InventoryItem.templateItemID, Equal<Required<InventoryItem.templateItemID>>
			  >>.Select(this, inventoryID))
			{
				var inventory = (InventoryItem)attributeDef;
				var attribute = (CSAnswers)attributeDef;
				AttributeValue def = new AttributeValue();
				def.AttributeID = attribute.AttributeID.ValueField();
				def.NoteID = inventory.NoteID.ValueField();
				def.InventoryID = inventory.InventoryCD.ValueField();
				def.Value = attribute.Value.ValueField();
				impl.AttributesValues.Add(def);
			}

			MappedTemplateItem obj = bucket.Product = bucket.Product.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
			if(impl.AttributesDef.Count > ShopifyCaptions.ProductOptionsLimit)
			{
				throw new PXException(ShopifyMessages.ProductOptionsOutOfScope, impl.AttributesDef.Count, impl.InventoryID.Value, ShopifyCaptions.ProductOptionsLimit);
			}
			if(impl.Matrix?.Count > 0)
			{
				var activeMatrixItems = impl.Matrix.Where(x => x?.ItemStatus?.Value == PX.Objects.IN.Messages.Active);
				if (activeMatrixItems.Count() == 0)
				{
					throw new PXException(BCMessages.NoMatrixCreated);
				}
				if(activeMatrixItems.Count() > ShopifyCaptions.ProductVarantsLimit)
				{
					throw new PXException(ShopifyMessages.ProductVariantsOutOfScope, activeMatrixItems.Count(), impl.InventoryID.Value, ShopifyCaptions.ProductVarantsLimit);
				}
				foreach(var item in activeMatrixItems)
				{
					if (!bucket.VariantMappings.ContainsKey(item.InventoryID?.Value))
						bucket.VariantMappings.Add(item.InventoryID?.Value, null);
				}
			}
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

		public override void MapBucketExport(SPTemplateItemEntityBucket bucket, IMappedEntity existing)
		{
			MappedTemplateItem obj = bucket.Product;

			TemplateItems impl = obj.Local;
			ProductData data = obj.Extern = new ProductData();

			data.Title = impl.Description?.Value;
			data.BodyHTML = ClearHTMLContent(impl.Content?.Value);
			data.ProductType = impl.ItemClass?.Value;
			//data.Vendor = (impl.VendorDetails?.FirstOrDefault(v => v.Default?.Value == true)?? impl.VendorDetails?.FirstOrDefault())?.VendorName?.Value;
			//Put all categories to the Tags later if CombineCategoriesToTags setting is true
			var categories = impl.Categories?.Select(x => { if (SalesCategories.TryGetValue(x.CategoryID.Value.Value, out var desc)) return desc; else return string.Empty; }).Where(x => !string.IsNullOrEmpty(x)).ToList();
			if (categories != null && categories.Count > 0)
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
				data.Metafields = new List<MetafieldData>() { new MetafieldData() { Key = ShopifyCaptions.Product, Value = impl.Id.Value.ToString(), ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global } };
                data.Metafields.Add(new MetafieldData() { Key = ShopifyCaptions.ProductId, Value = impl.InventoryID.Value, ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global });
                data.Metafields.Add(new MetafieldData() { Key = nameof(ProductTypes), Value = BCCaptions.TemplateItem, ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global });
			}
			/////
			if (impl.AttributesDef?.Count > 0)
			{
				data.Options = new List<ProductOptionData>();
				int optionSortOrder = 1;
				//Shopify only allows maximum 3 options
				foreach (var attribute in impl.AttributesDef.Take(ShopifyCaptions.ProductOptionsLimit))
				{
					data.Options.Add(new ProductOptionData() { Name = attribute.Description?.Value, Position = optionSortOrder });
					optionSortOrder++;
				}
			}
            var results = PXSelectJoin<InventoryItem,
            InnerJoin<INItemXRef, On<InventoryItem.inventoryID, Equal<INItemXRef.inventoryID>,
                And<Where2<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
                                And<INItemXRef.bAccountID, Equal<InventoryItem.preferredVendorID>>>,
                     Or<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>>>, Where<InventoryItem.templateItemID, Equal<Required<InventoryItem.templateItemID>>>>.
                     Select(this, impl.InventoryItemID).Cast<PXResult<InventoryItem, INItemXRef>>()?.ToList();

            var variants = data.Variants = new List<ProductVariantData>();
			foreach (var item in obj.Local.Matrix.Where(x => x.ItemStatus?.Value == PX.Objects.IN.Messages.Active).Take(ShopifyCaptions.ProductVarantsLimit))
			{
				ProductVariantData variant = new ProductVariantData();
				variant.LocalID = item.Id.Value;
				variant.Id = bucket.VariantMappings.ContainsKey(item.InventoryID?.Value) ? bucket.VariantMappings[item.InventoryID?.Value] : null;
				variant.Title = item.Description?.Value?? impl.Description?.Value;
				variant.Price = item.DefaultPrice.Value;
				variant.Sku = item.InventoryID?.Value;
				variant.OriginalPrice = item.MSRP?.Value;
				variant.Weight = item.DimensionWeight?.Value??impl.DimensionWeight?.Value;
				variant.WeightUnit = (item.WeightUOM?.Value?? impl.WeightUOM?.Value)?.ToLower();
				bool isTaxable;
				bool.TryParse(GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxCategorySubstitutionListID, impl.TaxCategory?.Value, String.Empty), out isTaxable);
				variant.Taxable = isTaxable;
                if (!string.IsNullOrWhiteSpace(obj.Local.BaseUOM?.Value))
                {
                    variant.Barcode = (results?.FirstOrDefault(x => x.GetItem<InventoryItem>().InventoryCD.Trim() == item.InventoryID?.Value?.Trim() && x.GetItem<INItemXRef>().AlternateType == INAlternateType.Barcode
                                && x.GetItem<INItemXRef>().UOM == obj.Local.BaseUOM.Value) ??
                                results?.FirstOrDefault(x => x.GetItem<InventoryItem>().InventoryCD.Trim() == item.InventoryID?.Value?.Trim() && x.GetItem<INItemXRef>().AlternateType == INAlternateType.Barcode
                                && string.IsNullOrEmpty(x.GetItem<INItemXRef>().UOM)))?.GetItem<INItemXRef>().AlternateID;
                }
                if (impl.IsStockItem?.Value == true)
				{
					variant.InventoryPolicy = InventoryPolicy.Continue;
					variant.InventoryManagement = availability == BCCaptions.AvailableTrack ? ShopifyCaptions.InventoryManagement_Shopify : null;
                }
				else
				{
					variant.InventoryPolicy = InventoryPolicy.Continue;
					variant.InventoryManagement = null;
					variant.RequiresShipping = false;
				}
				var def = obj.Local.AttributesValues.Where(x => x.NoteID.Value == item.Id).ToList();
				foreach(var attrItem in def)
				{
					var optionObj = obj.Local.AttributesDef.FirstOrDefault(x => x.AttributeID.Value == attrItem.AttributeID.Value);
					if(optionObj != null)
					{
						var option = data.Options.FirstOrDefault(x => optionObj != null && x.Name == optionObj.Description?.Value);
						if (option == null) continue;
						var attrValue = optionObj.Values.FirstOrDefault(x => x.ValueID?.Value == attrItem?.Value.Value);
                        
						switch (option.Position)
						{
							case 1:
								{
									variant.Option1 = attrValue?.Description?.Value;
                                    variant.OptionSortOrder1 = attrValue.SortOrder.Value.Value;
									break;
								}
							case 2:
								{
									variant.Option2 = attrValue?.Description?.Value;
                                    variant.OptionSortOrder2 = attrValue.SortOrder.Value.Value;
                                    break;
								}
							case 3:
								{
									variant.Option3 = attrValue?.Description?.Value;
                                    variant.OptionSortOrder3 = attrValue.SortOrder.Value.Value;
                                    break;
								}
							default:
								break;
						}
					}
				}
				if(variant.Id == null || variant.Id == 0)
					variant.Metafields = new List<MetafieldData>() { new MetafieldData() { Key = ShopifyCaptions.Variant, Value = item.Id.Value.ToString(), ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global } };
                variants.Add(variant);
			}
            if(variants.Count > 0)
            {
                int i = 1;
                foreach(var item in variants.OrderBy(x => x.OptionSortOrder1).ThenBy(x => x.OptionSortOrder2).ThenBy(x => x.OptionSortOrder3))
                {
                    item.Position = i;
                    i++;
                }
            }
            
		}

		public override object GetAttribute(SPTemplateItemEntityBucket bucket, string attributeID)
		{
			MappedTemplateItem obj = bucket.Product;
			TemplateItems impl = obj.Local;
			return impl.AttributesValues.Where(x => string.Equals(x?.AttributeID?.Value, attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

		public override void SaveBucketExport(SPTemplateItemEntityBucket bucket, IMappedEntity existing, String operation)
		{
			MappedTemplateItem obj = bucket.Product;
			ProductData data = null;
			if (obj.Extern.Categories?.Count > 0 && GetBindingExt<BCBindingShopify>()?.CombineCategoriesToTags == BCSalesCategoriesExportAttribute.ExportAsTags)
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
					if (notExistedVariantIds?.Count > 0)
					{
						obj.Details.ToList().RemoveAll(x => notExistedVariantIds.Contains(x.ExternID.ToLong()));
						notExistedVariantIds.ForEach(x =>
						{
							if (x != null) productVariantDataProvider.Delete(obj.ExternID, x.ToString());
						});
					}
					data = productDataProvider.Update(obj.Extern, obj.ExternID);
				}
			}
			catch (Exception ex)
			{
				throw new PXException(ex.Message);
			}

			obj.AddExtern(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
			if(data.Variants?.Count > 0)
			{
				var localVariants = obj.Local.Matrix;
				foreach(var externVariant in data.Variants)
				{
					var matchItem = localVariants.FirstOrDefault(x => x.InventoryID?.Value == externVariant.Sku);
					if(matchItem != null)
					{
						obj.AddDetail(BCEntitiesAttribute.Variant, matchItem.Id.Value, externVariant.Id.ToString());
					}
				}
			}
			UpdateStatus(obj, operation);
		}
		#endregion
	}
}