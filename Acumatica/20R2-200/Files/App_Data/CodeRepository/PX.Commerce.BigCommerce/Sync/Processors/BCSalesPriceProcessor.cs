using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.BigCommerce.API.REST.Filters;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using Serilog.Context;

namespace PX.Commerce.BigCommerce
{
	public class BCSalesPriceEnityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Price;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedBaseSalesPrice Price;
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.SalesPrice, BCCaptions.SalesPrice,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(InventoryItem),
		URL = "products/{0}/edit"
		)]
	public class BCSalesPriceProcessor : BCProcessorBulkBase<BCSalesPriceProcessor, BCSalesPriceEnityBucket, MappedBaseSalesPrice>, IProcessor
	{
		protected ProductBulkPricingRestDataProvider productBulkPricingRestDataProvider;
		protected ProductBatchBulkRestDataProvider productBatchBulkRestDataProvider;
		protected ProductRestDataProvider productRestDataProvider;
		protected StoreCurrencyDataProvider storCurrencyDataProvider;
		protected ProductVariantBatchRestDataProvider variantBatchRestDataProvider;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());

			productBulkPricingRestDataProvider = new ProductBulkPricingRestDataProvider(client);
			productBatchBulkRestDataProvider = new ProductBatchBulkRestDataProvider(client);
			productRestDataProvider = new ProductRestDataProvider(client);
			storCurrencyDataProvider = new StoreCurrencyDataProvider(client);
			variantBatchRestDataProvider = new ProductVariantBatchRestDataProvider(client);
		}
		#endregion
		#region Common
		public override void NavigateLocal(IConnector connector, ISyncStatus status)
		{
			ARSalesPriceMaint extGraph = PXGraph.CreateInstance<ARSalesPriceMaint>();
			ARSalesPriceFilter filter = extGraph.Filter.Current;
			filter.PriceType = PriceTypes.BasePrice;
			InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.noteID, Equal<Required<InventoryItem.noteID>>>>.Select(this, status?.LocalID);
			filter.InventoryID = inventory.InventoryID;

			throw new PXRedirectRequiredException(extGraph, "Navigation") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}
		#endregion

		#region Import
		public override List<BCSalesPriceEnityBucket> FetchBucketsImport(List<BCSyncStatus> ids, int?[] syncIDs)
		{
			throw new NotImplementedException();
		}
		public override void SaveBucketsImport(List<BCSalesPriceEnityBucket> buckets)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Export

		public override List<BCSalesPriceEnityBucket> FetchBucketsExport(List<BCSyncStatus> ids, int?[] syncIDs)
		{
			BCEntityStats entityStats = GetEntityStats();
			List<BCSyncStatus> parentEntities = PXSelect<
				BCSyncStatus,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<Where<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>
						>>>>>
				.Select(this, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.ProductWithVariant)
				.RowCast<BCSyncStatus>()
				.ToList();


			var details = PXSelectJoin<BCSyncDetail,
					InnerJoin<InventoryItem, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncDetail.localID>>,
					InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>>,
					   Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncDetail.entityType, Equal<Required<BCSyncDetail.entityType>>
						>>>>.Select(this, BCEntitiesAttribute.Variant);

			List<BCSyncStatus> CurrentEntities = PXSelect<
				BCSyncStatus,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<BCSyncStatus.deleted, Equal<Required<BCSyncStatus.deleted>
						>>>>>>
				.Select(this, BCEntitiesAttribute.SalesPrice, false)
				.RowCast<BCSyncStatus>()
				.ToList();

			// Get sales price data from acumatica
			SalesPricesInquiry response = cbapi.Put<SalesPricesInquiry>(new SalesPricesInquiry(), null);
			List<BCSalesPriceEnityBucket> buckets = new List<BCSalesPriceEnityBucket>();
			if (response.SalesPriceDetails == null) return buckets;
			BCBinding binding = GetBinding();
			//get base currency from BC store
			List<Currency> currencies = storCurrencyDataProvider.Get();
			var defaultCurrency = currencies.Where(x => x.Default == true).FirstOrDefault();
			if (defaultCurrency == null) return buckets;
			var baseCurrency = Branch.PK.Find(this, binding.BranchID)?.BaseCuryID.ValueField();
			//Filter by base currency and Type "Baseprice"
			response.SalesPriceDetails = response.SalesPriceDetails.Where(x => x.PriceType?.Value == BCObjectsConstants.BasePrice && (x.CurrencyID?.Value ?? baseCurrency?.Value) == defaultCurrency.CurrencyCode) ?.Select(x => { x.BreakQty = (x.BreakQty?.Value ?? 1).ValueField(); return x; })?.ToList();
			var salesPriceDetails = response.SalesPriceDetails;

			var inventories = salesPriceDetails?.GroupBy(x => x.InventoryID.Value)?.ToDictionary(d => d.Key, d => d.ToList());
			if (inventories == null) return buckets;
			FilterPrices filter = new FilterPrices { Include = "bulk_pricing_rules,variants" };
			List<ProductData> products = productRestDataProvider.GetAll(filter)?.ToList();
			if (products == null) products = new List<ProductData>();

			foreach (var item in inventories)
			{
				BCSyncStatus parent = null;
				var inventory = InventoryItem.UK.Find(this, item.Key);
				if (inventory == null) continue;

				bool isVariant = inventory.TemplateItemID != null;
				BCSyncDetail detail = null;
				if (isVariant)
				{
					detail = details.FirstOrDefault(x => x.GetItem<BCSyncDetail>().LocalID == inventory.NoteID);
					parent = parentEntities.FirstOrDefault(p => p.SyncID == detail?.SyncID);
				}
				else
					parent = parentEntities.FirstOrDefault(p => p.LocalID.Value == inventory.NoteID);

				if (parent == null || parent?.ExternID == null || parent.Deleted == true)
				{
					LogWarning(Operation.LogScope(), BCMessages.LogPricesSkippedItemNotSynce, inventory.InventoryCD);
					continue; //if Inventory is not found, skip  
				}

				//for variants consider only breakqty 0 or 1
				if (isVariant)
					item.Value.RemoveAll(x => x.BreakQty?.Value > 1);

				DateTime? maxDateTime = null;

				bool updatedAny = false;
				bool forceSync = false;

				//store in sync status by inventory
				SalesPricesInquiry productsSalesPrice = new SalesPricesInquiry();
				productsSalesPrice.ExternalInventoryID = isVariant ? detail.ExternID.ValueField() : parent.ExternID.ValueField();
				productsSalesPrice.SalesPriceDetails = new List<SalesPriceDetail>();
				productsSalesPrice.Inventory_ID = item.Key;
				productsSalesPrice.Isvariant = isVariant;
				BCSyncStatus current;
				if (ids != null && ids.Count > 0 && (Operation.PrepareMode == PrepareMode.None || syncIDs != null))
				{
					var localIds = ids.Select(x => x.LocalID);
					current = ids.FirstOrDefault(s => s.LocalID == inventory.NoteID);

					if (!localIds.Contains(inventory.NoteID)) continue;
				}
				else
					current = CurrentEntities.FirstOrDefault(s => s.LocalID == inventory.NoteID);
				ProductData productData = null; ;
				List<ProductsBulkPricingRules> existingPriceRules = null;
				ProductsVariantData variant = null;
				productData = products.FirstOrDefault(x => x.Id == parent.ExternID.ToInt());
				if (productData == null)
				{
					if (current != null)
					{
						Statuses.Delete(BCSyncStatus.PK.Find(this, current.SyncID));
					}
					continue;
				}
				if (isVariant)
				{
					var existingVariantPrice = productData.Variants?.ToList();
					variant = existingVariantPrice.FirstOrDefault(x => x.Id == detail.ExternID.ToInt());
				}
				else
				{
					existingPriceRules = productData.BulkPricingRules?.ToList();
					if (existingPriceRules == null) existingPriceRules = new List<ProductsBulkPricingRules>();
					productsSalesPrice.existingId = existingPriceRules.Select(x => x.Id.Value)?.ToList() ?? new List<int>();
				}
				foreach (SalesPriceDetail basePrice in item.Value)
				{
					basePrice.Isvariant = isVariant;
					//skip prices that are expired or are not yet effective
					if ((inventory.BaseUnit != basePrice.UOM?.Value || basePrice.Warehouse?.Value != null)  ||
						(basePrice.ExpirationDate?.Value != null && ((DateTime)basePrice.ExpirationDate.Value).Date < PX.Common.PXTimeZoneInfo.Now.Date) ||
						(basePrice.EffectiveDate?.Value != null && ((DateTime)basePrice.EffectiveDate.Value).Date > PX.Common.PXTimeZoneInfo.Now.Date))
					{
						continue;
					}

					basePrice.SyncTime = basePrice.LastModifiedDateTime?.Value;
					maxDateTime = maxDateTime ?? basePrice.SyncTime;
					maxDateTime = maxDateTime > basePrice.SyncTime ? maxDateTime : basePrice.SyncTime;

					productsSalesPrice.SalesPriceDetails.Add(basePrice);
					if (Operation.PrepareMode == PrepareMode.Incremental)
					{
						if (entityStats?.LastIncrementalExportDateTime != null && basePrice.SyncTime < entityStats.LastIncrementalExportDateTime)
						{
							continue;

						}
					}

					updatedAny = true;
				}
				//get difference
				if (isVariant)
				{
					if (variant.SalePrice > 0)
					{
						if (!productsSalesPrice.SalesPriceDetails.Any(x => x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1)) forceSync = true;
						if (productsSalesPrice.SalesPriceDetails.Where(x => x.BreakQty.Value <= 1).Count() == 1 && productsSalesPrice.SalesPriceDetails.FirstOrDefault(x => x.BreakQty.Value <= 1).Price.Value.Value != variant.SalePrice.Value) forceSync = true;
					}
				}
				else
				{
					var breakQtyPrices = productsSalesPrice.SalesPriceDetails.Where(x => x.BreakQty.Value > 1)?.ToList();
					if (existingPriceRules.Count() > 0 && (breakQtyPrices == null || breakQtyPrices.Count() == 0)) forceSync = true; // Lines deletd or no longer valid at acumatica 
					if (productData.SalePrice > 0)
					{
						if (!productsSalesPrice.SalesPriceDetails.Any(x => x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1)) forceSync = true; // 0 or 1 breakqty is not in sync
						if (productsSalesPrice.SalesPriceDetails.Where(x => x.BreakQty.Value <= 1).Count() == 1 && productsSalesPrice.SalesPriceDetails.FirstOrDefault(x => x.BreakQty.Value <= 1).Price.Value.Value != productData.SalePrice.Value) forceSync = true;
					}
					if (existingPriceRules != null)//lines exist for product but some brekqty line is delted
					{

						if (breakQtyPrices != null)
						{
							if (existingPriceRules.Any(c => breakQtyPrices.All(x => x.BreakQty.Value != c.QuantityMin))) forceSync = true;
							if (breakQtyPrices.Any(c => existingPriceRules.All(x => x.QuantityMin != c.BreakQty.Value))) forceSync = true;
						}
					}
				}
				if (!updatedAny && !forceSync && current?.PendingSync == false) continue;
				MappedBaseSalesPrice obj = new MappedBaseSalesPrice(productsSalesPrice, inventory?.NoteID, maxDateTime, parent.SyncID);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export, resync: forceSync);
				if (Operation.PrepareMode != PrepareMode.Reconciliation && Operation.PrepareMode != PrepareMode.Full
					&& status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force) continue;
				buckets.Add(new BCSalesPriceEnityBucket() { Price = obj });
			}
			// If all sales price in acumatica is deleted for inventory
			if (CurrentEntities == null) return buckets;

			foreach (var entity in CurrentEntities)
			{
				BCSyncStatus parent = null;
				InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.noteID, Equal<Required<InventoryItem.noteID>>>>.Select(this, entity.LocalID);
				if (inventories.Any(x => x.Key == inventory.InventoryCD.Trim())) continue;
				bool isVariant = inventory.TemplateItemID != null;
				BCSyncDetail detail = null;
				if (isVariant)
				{
					detail = details.FirstOrDefault(x => x.GetItem<BCSyncDetail>().LocalID == inventory.NoteID);
					parent = parentEntities.FirstOrDefault(p => p.SyncID == detail?.SyncID);
				}
				else
					parent = parentEntities.FirstOrDefault(p => p.LocalID.Value == inventory.NoteID);

				if (parent == null || parent?.ExternID == null || parent?.Deleted == true)
				{
					continue; //if Inventory is not found, skip  
				}

				MappedBaseSalesPrice obj = new MappedBaseSalesPrice(new SalesPricesInquiry()
				{
					SalesPriceDetails = new List<SalesPriceDetail>(),
					ExternalInventoryID = isVariant ? detail.ExternID.ValueField() : parent.ExternID.ValueField(),
					existingId = new List<int>(),
					Isvariant = isVariant,
					Inventory_ID = inventory.InventoryCD
				}, entity.LocalID, entity.LastModifiedDateTime, null); ;
				EnsureStatus(obj, SyncDirection.Export);
				buckets.Add(new BCSalesPriceEnityBucket() { Price = obj });
			}

			return buckets;
		}

		public override void MapBucketExport(BCSalesPriceEnityBucket bucket, IMappedEntity existing)
		{
			MappedBaseSalesPrice obj = bucket.Price;

			BulkPricingWithSalesPrice product = obj.Extern = new BulkPricingWithSalesPrice();
			SalesPricesInquiry salesPricesInquiry = obj.Local;
			product.Data = new List<ProductsBulkPricingRules>();
			product.SalePrice = 0;
			BCSyncStatus parentStatus = BCSyncStatus.PK.Find(this, obj.ParentID);
			product.Id = parentStatus.ExternID.ToInt();
			salesPricesInquiry.SalesPriceDetails = salesPricesInquiry.SalesPriceDetails.GroupBy(x => x.BreakQty.Value).Select(x => x.Count() > 1 ? x.FirstOrDefault(y => y.Promotion.Value == false) : x.FirstOrDefault()).ToList();


			if (salesPricesInquiry.Isvariant)
			{
				product.Variant = new ProductsVariantData();
				product.Variant.Id = salesPricesInquiry.ExternalInventoryID?.Value.ToInt();
				product.Variant.ProductId = parentStatus.ExternID.ToInt();
				product.Variant.OptionValues = null;
				product.Variant.SalePrice = 0;
				if (salesPricesInquiry.SalesPriceDetails.Any(x => x.BreakQty?.Value == 0) && salesPricesInquiry.SalesPriceDetails.Any(x => x.BreakQty?.Value == 1))
				{
					var basePrice = salesPricesInquiry.SalesPriceDetails.FirstOrDefault(x => x.BreakQty?.Value == 0);
					salesPricesInquiry.SalesPriceDetails.Remove(basePrice);
				}
				foreach (var impl in salesPricesInquiry.SalesPriceDetails)
				{
					product.Variant.SalePrice = impl.Price?.Value ?? 0;
				}
			}

			else
			{
				if (salesPricesInquiry.SalesPriceDetails.Count() > 0)
				{
					var prices = salesPricesInquiry.SalesPriceDetails.OrderBy(x => x.BreakQty.Value)?.ToList();
					if (prices.Any(x => x.BreakQty?.Value == 0) && prices.Any(x => x.BreakQty?.Value == 1))
					{
						var basePrice = prices.FirstOrDefault(x => x.BreakQty?.Value == 0);
						prices.Remove(basePrice);
					}
					for (int i = 0; i < prices.Count(); i++)
					{
						ProductsBulkPricingRules bulkPricingRules = new ProductsBulkPricingRules();
						var impl = prices[i];
						if (impl.BreakQty?.Value > 1)
						{
							bulkPricingRules.QuantityMax = Convert.ToInt32((i + 1) >= prices.Count() ? 0 : prices[i + 1].BreakQty.Value - 1);
							bulkPricingRules.Type = BCObjectsConstants.Fixed;
							bulkPricingRules.Amount = impl.Price?.Value;

							bulkPricingRules.QuantityMin = Convert.ToInt32(impl.BreakQty?.Value);
							product.Data.Add(bulkPricingRules);

						}
						else
						{
							product.SalePrice = bulkPricingRules.Amount = impl.Price?.Value ?? 0;
						}

					}
				}
			}

		}

		public override void SaveBucketsExport(List<BCSalesPriceEnityBucket> buckets)
		{

			var bulkPrices = buckets.Where(x => !x.Price.Local.Isvariant).ToList();
			foreach (var price in bulkPrices)
			{
				foreach (var id in price.Price.Local.existingId)
					try
					{
						productBulkPricingRestDataProvider.Delete(id.ToString(), price.Price.Extern.Id.ToString());
					}
					catch { }
			}
			productBatchBulkRestDataProvider.UpdateAll(bulkPrices.Select(x => x.Price.Extern).ToList(), delegate (ItemProcessCallback<BulkPricingWithSalesPrice> callback)
				  {
					  BCSalesPriceEnityBucket obj = bulkPrices[callback.Index];
					  if (callback.IsSuccess)
					  {
						  BulkPricingWithSalesPrice data = callback.Result;
						  obj.Price.ExternID = null;
						  if (obj.Price.Extern.Data.Count() == 0 && obj.Price.Extern.SalePrice == 0)
						  {
							  Statuses.Delete(BCSyncStatus.PK.Find(this, obj.Price.SyncID));
						  }
						  else
						  {
							  obj.Price.AddExtern(obj.Price.Extern, new object[] { data.Id }.KeyCombine(), data.DateModifiedUT.ToDate());
							  UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternUpdate);
						  }

					  }
					  else
					  {
						  Log(obj.Price.SyncID, SyncDirection.Export, callback.Error);
						  UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternFailed, callback.Error.ToString());

					  }
				  });

			var variantPrices = buckets.Where(x => x.Price.Local.Isvariant).ToList();
			variantBatchRestDataProvider.UpdateAll(variantPrices.Select(x => x.Price.Extern.Variant).ToList(), delegate (ItemProcessCallback<ProductsVariantData> callbackVariant)
			{
				BCSalesPriceEnityBucket obj = variantPrices[callbackVariant.Index];
				if (callbackVariant.IsSuccess)
				{
					ProductsVariantData data = callbackVariant.Result;
					obj.Price.ExternID = null;
					if (obj.Price.Extern.Variant.SalePrice == 0)
					{
						Statuses.Delete(BCSyncStatus.PK.Find(this, obj.Price.SyncID));
					}
					else
					{
						obj.Price.AddExtern(obj.Price.Extern, new object[] { data.ProductId, data.Id }.KeyCombine(), data.CalculateHash());
						UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternUpdate);
					}
				}
				else
				{
					Log(obj.Price.SyncID, SyncDirection.Export, callbackVariant.Error);
					UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternFailed, callbackVariant.Error.ToString());
				}
			});
		}
		#endregion


	}
}