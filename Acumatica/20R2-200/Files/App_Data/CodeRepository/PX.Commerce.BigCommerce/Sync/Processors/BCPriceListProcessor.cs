using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.BigCommerce.API.REST.Filters;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Context;
using PX.Commerce.Objects;

namespace PX.Commerce.BigCommerce
{
	public class BCPriceListEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Price;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedPriceList Price;
	}
	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.PriceList, BCCaptions.PriceList,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(ARPriceClass),
		URL = "products/pricelists/{0}/edit"
		)]
	public class BCPriceListProcessor : BCProcessorBulkBase<BCPriceListProcessor, BCPriceListEntityBucket, MappedPriceList>, IProcessor
	{
		protected PriceListRestDataProvider priceListRestDataProvider;
		protected PriceListRecordRestDataProvider priceListrecordRestDataProvider;
		protected StoreCurrencyDataProvider storCurrencyDataProvider;
		protected CustomerPriceClassRestDataProvider customerPriceClassRestDataProvider;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			priceListRestDataProvider = new PriceListRestDataProvider(client);
			priceListrecordRestDataProvider = new PriceListRecordRestDataProvider(client);
			storCurrencyDataProvider = new StoreCurrencyDataProvider(client);
			customerPriceClassRestDataProvider = new CustomerPriceClassRestDataProvider(client);
		}
		#endregion

		#region Common

		public override void NavigateLocal(IConnector connector, ISyncStatus status)
		{
			ARSalesPriceMaint extGraph = PXGraph.CreateInstance<ARSalesPriceMaint>();
			ARSalesPriceFilter filter = extGraph.Filter.Current;
			filter.PriceType = PriceTypes.CustomerPriceClass;
			ARPriceClass priceClass = PXSelect<ARPriceClass, Where<ARPriceClass.noteID, Equal<Required<ARPriceClass.noteID>>>>.Select(this, status?.LocalID);
			filter.PriceCode = priceClass?.PriceClassID?.Trim();

			throw new PXRedirectRequiredException(extGraph, "Navigation") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		#endregion

		#region Import
		public override List<BCPriceListEntityBucket> FetchBucketsImport(List<BCSyncStatus> ids, int?[] syncIDs)
		{
			throw new NotImplementedException();
		}
		public override void SaveBucketsImport(List<BCPriceListEntityBucket> buckets)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export
		public override List<BCPriceListEntityBucket> FetchBucketsExport(List<BCSyncStatus> ids, int?[] syncIDs)
		{
			BCEntityStats entityStats = GetEntityStats();

			List<BCSyncStatus> parentInventoryEntities = PXSelect<BCSyncStatus,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<Where<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>>>>>>
				.Select(this, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.ProductWithVariant).RowCast<BCSyncStatus>().ToList();


			List<BCSyncStatus> parentPriceClassEntities = PXSelect<BCSyncStatus,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>>>
				.Select(this, BCEntitiesAttribute.CustomerPriceClass).RowCast<BCSyncStatus>().ToList();


			List<BCSyncStatus> CurrentEntities = PXSelect<
				BCSyncStatus,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>
						>>>>>
				.Select(this, BCEntitiesAttribute.PriceList)
				.RowCast<BCSyncStatus>()
				.ToList();

			//// Get sales price data from acumatica
			SalesPricesInquiry response = cbapi.Put<SalesPricesInquiry>(new SalesPricesInquiry(), null);
			List<BCPriceListEntityBucket> buckets = new List<BCPriceListEntityBucket>();
			if (response.SalesPriceDetails == null) return buckets;
			var binding = GetBinding();
			//get BC store currencies
			List<Currency> currencies = storCurrencyDataProvider.Get();
			var baseCurrency = Branch.PK.Find(this, binding.BranchID)?.BaseCuryID.ValueField();
			//group by price code and iterate
			var priceCodes = response.SalesPriceDetails.Where(x => x.PriceType.Value == BCCaptions.CustomerPriceClass)?.Select(x => { x.BreakQty = (x.BreakQty?.Value ?? 1).ValueField(); return x; })?.GroupBy(x => x.PriceCode.Value).ToDictionary(x => x.Key, x => x.ToList());
			if (priceCodes == null) return buckets;
			foreach (var priceCode in priceCodes)
			{
				DateTime? maxDateTime = null;

				bool updatedAny = false;
				bool forceSync = false;

				PriceListSalesPrice productsSalesPrice = new PriceListSalesPrice();
				productsSalesPrice.SalesPriceDetails = new List<SalesPriceDetail>();
				productsSalesPrice.PriceClassID = priceCode.Key;
				var customerPrices = priceCode.Value;
				BCSyncStatus current;
				ARPriceClass priceClass = PXSelect<ARPriceClass, Where<ARPriceClass.priceClassID, Equal<Required<ARPriceClass.priceClassID>>>>.Select(this, priceCode.Key);
				if (priceClass == null) continue;
				//process only id's that are passed or all
				if (ids != null && ids.Count > 0 && (Operation.PrepareMode == PrepareMode.None || syncIDs != null))
				{

					var localIds = ids.Select(x => x.LocalID);
					current = ids.FirstOrDefault(s => s.LocalID == priceClass.NoteID);

					if (!localIds.Contains(priceClass.NoteID)) continue;
				}
				else
				{
					current = CurrentEntities.FirstOrDefault(x => x.LocalID == priceClass.NoteID);
				}
				List<PriceListRecord> existingpPriceListRecords = new List<PriceListRecord>();
				if (current?.ExternID != null)
				{
					FilterPrices filter = new FilterPrices { Include = "bulk_pricing_tiers" };
					existingpPriceListRecords = priceListrecordRestDataProvider.GetAllRecords(current?.ExternID, filter)?.ToList();
					if (existingpPriceListRecords == null) existingpPriceListRecords = new List<PriceListRecord>();
				}

				BCSyncStatus pStatus = parentPriceClassEntities.FirstOrDefault(p => p.LocalID == priceClass.NoteID);
				if (pStatus?.ExternID == null || pStatus?.Deleted == true) continue;

				foreach (var custmerPrice in customerPrices)
				{
					if (string.IsNullOrEmpty(custmerPrice.CurrencyID?.Value)) custmerPrice.CurrencyID = baseCurrency;

					bool invalidPriceLine = false;
					var inventory = InventoryItem.UK.Find(this, custmerPrice.InventoryID?.Value);
					if (inventory == null || inventory?.TemplateItemID != null) continue;
					if (inventory.IsTemplate == true) custmerPrice.TemplateID = inventory.InventoryID;
					BCSyncStatus parentInventory = parentInventoryEntities.FirstOrDefault(p => p.LocalID == inventory.NoteID);
					if (parentInventory?.ExternID == null || parentInventory?.Deleted == true)
					{
						LogWarning(Operation.LogScope(current), BCMessages.LogPricesSkippedItemNotSynce, inventory.InventoryCD);
						continue; //if Inventory is not found, skip  
					}
					custmerPrice.ExternalInventoryID = parentInventory.ExternID.ValueField();
					if (!currencies.Any(x => x.CurrencyCode == custmerPrice.CurrencyID?.Value)) invalidPriceLine = true;

					if (inventory.BaseUnit != custmerPrice.UOM?.Value || custmerPrice.Warehouse?.Value != null) invalidPriceLine = true;

					if (custmerPrice.ExpirationDate?.Value != null && ((DateTime)custmerPrice.ExpirationDate.Value).Date < PX.Common.PXTimeZoneInfo.Now.Date) invalidPriceLine = true;

					if (custmerPrice.EffectiveDate?.Value != null && ((DateTime)custmerPrice.EffectiveDate.Value).Date > PX.Common.PXTimeZoneInfo.Now.Date) invalidPriceLine = true;
					custmerPrice.SyncTime = custmerPrice.LastModifiedDateTime?.Value;
					maxDateTime = maxDateTime ?? custmerPrice.SyncTime;
					maxDateTime = maxDateTime > custmerPrice.SyncTime ? maxDateTime : custmerPrice.SyncTime;
					if (invalidPriceLine)
					{
						var existing = existingpPriceListRecords.FirstOrDefault(x => x.ProductID == parentInventory.ExternID.ToInt() && x.Currency.ToUpper() == custmerPrice.CurrencyID.Value);
						if (existing != null)
						{
							if (custmerPrice.BreakQty.Value <= 1) { updatedAny = true; }
							else if (existing.BulKPricingTier.Any(x => x.QuantityMinimum == custmerPrice.BreakQty.Value)) { updatedAny = true; }

						}
						continue;
					}
					productsSalesPrice.SalesPriceDetails.Add(custmerPrice);
					if (Operation.PrepareMode == PrepareMode.Incremental)
					{
						if (entityStats?.LastIncrementalExportDateTime != null && custmerPrice.SyncTime < entityStats.LastIncrementalExportDateTime)
						{
							continue;

						}
					}


					updatedAny = true;

				}
				//get deletd lines
				foreach (var existing in existingpPriceListRecords)
				{
					var prices = productsSalesPrice.SalesPriceDetails.Where(x => x.ExternalInventoryID?.Value != null && x.ExternalInventoryID.Value == existing.ProductID.ToString() && x.CurrencyID.Value == existing.Currency.ToUpper())?.ToList();
					if (prices == null) forceSync = true;
					if (!prices.Any(x => x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1)) forceSync = true;
					if (prices.Where(x => x.BreakQty.Value <= 1).Count() == 1 && prices.FirstOrDefault(x => x.BreakQty.Value <= 1).Price.Value.Value != existing.SalesPrice.Value) forceSync = true;

					if (existing.BulKPricingTier != null)//lines exist for product but some brekqty line is delted
					{
						var breakQtyPrices = prices.Where(x => x.BreakQty.Value > 1)?.ToList();
						if (breakQtyPrices != null)
							if (existing.BulKPricingTier.Any(c => breakQtyPrices.All(x => x.BreakQty.Value != c.QuantityMinimum))) forceSync = true;
						if (breakQtyPrices.Any(c => existing.BulKPricingTier.All(x => x.QuantityMinimum != c.BreakQty.Value))) forceSync = true;
					}
				}
				if (existingpPriceListRecords.Count() == 0 && productsSalesPrice.SalesPriceDetails.Count() > 0) forceSync = true;
				if (!updatedAny && !forceSync && current?.PendingSync == false) continue;


				MappedPriceList obj = new MappedPriceList(productsSalesPrice, pStatus?.LocalID.Value, maxDateTime, pStatus.SyncID);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export, resync: forceSync);
				if (Operation.PrepareMode != PrepareMode.Reconciliation && status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force) continue;

				buckets.Add(new BCPriceListEntityBucket() { Price = obj });
			}
			if (CurrentEntities == null) return buckets;
			foreach (var entity in CurrentEntities)
			{
				if (entity.Deleted != true)
				{
					ARPriceClass priceClass = PXSelect<ARPriceClass, Where<ARPriceClass.noteID, Equal<Required<ARPriceClass.noteID>>>>.Select(this, entity.LocalID);
					if (priceCodes.Any(x => x.Key == priceClass?.PriceClassID?.Trim())) continue;
					MappedPriceList obj = new MappedPriceList(new PriceListSalesPrice() { PriceClassID = priceClass.PriceClassID }, entity.LocalID, entity.LastModifiedDateTime, null);
					EnsureStatus(obj, SyncDirection.Export);
					buckets.Add(new BCPriceListEntityBucket() { Price = obj });
				}
			}


			return buckets;
		}

		public override void MapBucketExport(BCPriceListEntityBucket bucket, IMappedEntity existing)
		{
			MappedPriceList obj = bucket.Price;
			PriceList priceList = obj.Extern = new PriceList();
			PriceListSalesPrice impl = obj.Local;
			BCSyncStatus status = BCSyncStatus.PK.Find(this, obj.ParentID);
			if (status == null)
			{
				throw new PXException(BigCommerceMessages.CustomerPriceClassNotSyncronized);
			}
			priceList.Name = impl.PriceClassID;
			priceList.ExtrenalPriceClassID = status.ExternID;
			priceList.priceListRecords = new List<PriceListRecord>();
			var inventories = impl.SalesPriceDetails?.GroupBy(x => x.InventoryID.Value).ToDictionary(x => x.Key, x => x.ToList());
			if (inventories == null) return;
			//  create PriceListRecords
			foreach (var inventory in inventories)
			{
				var CurrencyBased = inventory.Value?.GroupBy(x => x.CurrencyID.Value).ToDictionary(x => x.Key, x => x.ToList());
				foreach (var currency in CurrencyBased)
				{
					var priceListRecord = new PriceListRecord();
					priceListRecord.SKU = inventory.Key;
					var message = string.Format(BCMessages.SalesPriceWithoutBasePrice, inventory.Key, currency.Key);
					if (!currency.Value.Any(x => x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1)) throw new PXException(message);
					var prices = currency.Value.ToList();
					prices = prices.GroupBy(x => x.BreakQty.Value).Select(x => x.Count() > 1 ? x.FirstOrDefault(y => y.Promotion.Value == false) : x.FirstOrDefault()).ToList();

					if (prices.Any(x => x.BreakQty?.Value == 0) && prices.Any(x => x.BreakQty?.Value == 1))
						prices.Remove(prices.FirstOrDefault(x => x.BreakQty?.Value == 0));
					priceListRecord.BulKPricingTier = new List<BulKPricingTier>();
					prices.ForEach(x =>
					{
						priceListRecord.ProductID = x.ExternalInventoryID?.Value?.ToInt();
						if (x.BreakQty.Value == 1 || x.BreakQty.Value == 0) // if breakqty is null then set it as New sales price
						{
							priceListRecord.Price = 0;
							priceListRecord.SalesPrice = x.Price.Value;
							priceListRecord.Currency = x.CurrencyID?.Value;
						}
						else if (x.BreakQty.Value > 1)
						{
							BulKPricingTier bulKPricingTier = new BulKPricingTier();
							bulKPricingTier.Amount = x.Price?.Value;
							bulKPricingTier.PriceCode = x.PriceCode?.Value;
							bulKPricingTier.Type = BCObjectsConstants.Fixed;
							bulKPricingTier.QuantityMinimum = Convert.ToInt32(x.BreakQty?.Value);
							priceListRecord.BulKPricingTier.Add(bulKPricingTier);
						}
					});
					var template = prices.FirstOrDefault(x => x.TemplateID != null);
					if (template?.TemplateID != null)//means inventory is template
					{
						List<InventoryItem> childItems = PXSelect<InventoryItem, Where<InventoryItem.templateItemID, Equal<Required<InventoryItem.templateItemID>>>>.Select(this, template?.TemplateID).RowCast<InventoryItem>().ToList();
						foreach (var child in childItems)
						{
							PriceListRecord childPriceListRecord = priceListRecord.ShallowCopy();
							childPriceListRecord.SKU = child.InventoryCD.Trim();
							priceList.priceListRecords.Add(childPriceListRecord);
						}
					}
					else
						priceList.priceListRecords.Add(priceListRecord);
				}
			}
		}

		public override void SaveBucketsExport(List<BCPriceListEntityBucket> buckets)
		{
			string priceListId;
			//Get all price lists
			var priceLists = priceListRestDataProvider.GetAll();

			//iterate by pricecode
			foreach (var priceCode in buckets)
			{
				try
				{
					var mappedExternObj = priceCode.Price.Extern;

					string exteranlGroupId = mappedExternObj.ExtrenalPriceClassID;

					//check price code in Price list
					PriceList priceListresponse = priceLists.Where(x => x.Name.Trim().ToLower() == mappedExternObj.Name.Trim().ToLower()).FirstOrDefault();
					priceListId = priceListresponse?.ID?.ToString();

					if (priceListId == null)
					{
						//create price list
						priceListresponse = priceListRestDataProvider.Create(new PriceList() { Name = mappedExternObj.Name });
						priceListId = priceListresponse.ID.ToString();
					}
					if (mappedExternObj.priceListRecords?.Count > 0)
					{
						var existingpPriceListRecords = priceListrecordRestDataProvider.GetAllRecords(priceListId)?.ToList();
						if (existingpPriceListRecords?.Count > 0)
						{
							//add that are not present with price= null
							foreach (var record in existingpPriceListRecords)
							{
								if (mappedExternObj.priceListRecords.Any(x => x.ProductID == record.ProductID && x.Currency.ToUpper() == record.Currency))
									continue;
								priceListrecordRestDataProvider.DeleteRecords(priceListId, record.VariantID, record.Currency);
							}

						}
						bool errorOcured = false;
						//call upsert
						priceListrecordRestDataProvider.Upsert(mappedExternObj.priceListRecords, priceListId, delegate (ItemProcessCallback<PriceListRecord> callback)
						{
							if (!callback.IsSuccess)
							{
								errorOcured = true;
								Log(priceCode?.Primary?.SyncID, SyncDirection.Export, callback.Error);
								UpdateStatus(priceCode.Price, BCSyncOperationAttribute.ExternFailed, callback.Error.ToString());
							}

						});
						if (errorOcured) continue;
						priceCode.Price.ExternID = null;
						//Add extern and updateStatus for each record
						priceCode.Price.AddExtern(priceListresponse, priceListId, priceListresponse.CalculateHash());
						UpdateStatus(priceCode.Price, BCSyncOperationAttribute.ExternUpdate);

						//link customer group to price list
						customerPriceClassRestDataProvider.Update(new CustomerGroupData()
						{
							DiscountRule = new List<DiscountRule>()
									{
									new DiscountRule()
										{
											Type ="price_list",
											PriceListId = priceListId.ToInt()
										}
									}
						}
						, exteranlGroupId);

					}
					else
					{
						//delete it
						priceListRestDataProvider.DeletePriceList(priceListId);
						//remove from bcsyncstatus
						Statuses.Delete(BCSyncStatus.PK.Find(this, priceCode.Price.SyncID));

					}
				}
				catch (Exception ex)
				{
					Log(priceCode?.Primary?.SyncID, SyncDirection.Export, ex);
					UpdateStatus(priceCode.Price, BCSyncOperationAttribute.ExternFailed, ex.InnerException?.Message ?? ex.Message);

				}

			}
		}
		#endregion
	}
}
