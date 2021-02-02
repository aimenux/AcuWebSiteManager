using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;
using PX.Objects.Common;
using Serilog.Context;

namespace PX.Commerce.BigCommerce
{
	public class BCAvailabilityEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedAvailability Product;
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.ProductAvailability, BCCaptions.ProductAvailability,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.InventorySummaryEnq),
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(InventoryItem),
		Requires = new string[] { BCEntitiesAttribute.StockItem },
		URL = "products/{0}/edit"
		)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-AvailabilityStockItem", "BC-PUSH-AvailabilityTemplates" } )]
	public class BCAvailabilityProcessor : BCProcessorBulkBase<BCStockItemProcessor, BCAvailabilityEntityBucket, MappedAvailability>, IProcessor
	{
		protected ProductRestDataProvider productDataProvider;
		protected ProductVariantBatchRestDataProvider variantBatchRestDataProvider;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			productDataProvider = new ProductRestDataProvider(BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>()));
			variantBatchRestDataProvider = new ProductVariantBatchRestDataProvider(BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>()));
		}
		#endregion

		#region Common
		public override void NavigateLocal(IConnector connector, ISyncStatus status)
		{
			PX.Objects.IN.InventorySummaryEnq extGraph = PXGraph.CreateInstance<PX.Objects.IN.InventorySummaryEnq>();
			InventorySummaryEnqFilter filter = extGraph.Filter.Current;
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.noteID, Equal<Required<InventoryItem.noteID>>>>.Select(this, status.LocalID);
			filter.InventoryID = item.InventoryID;

			if (filter.InventoryID != null)
				throw new PXRedirectRequiredException(extGraph, "Navigation") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}
		public override MappedAvailability PullEntity(Guid? localID, Dictionary<string,object> fields)
		{
			if (localID == null) return null;
			DateTime? timeStamp = fields.Where(f => f.Key.EndsWith(nameof(BCEntity.LastModifiedDateTime), StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToDate();
			int? parentID = fields.Where(f => f.Key.EndsWith(nameof(BCSyncStatus.SyncID), StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToInt();
			localID = fields.Where(f => f.Key.EndsWith("TemplateItem_noteID", StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToGuid()??localID;
			return new MappedAvailability(new StorageDetailsResult() , localID, timeStamp, parentID);
		}
		#endregion

		#region Import
		public override List<BCAvailabilityEntityBucket> FetchBucketsImport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			return null;
		}
		public override void MapBucketImport(BCAvailabilityEntityBucket bucket, IMappedEntity existing)
		{
			throw new NotImplementedException();
		}
		public override void SaveBucketsImport(List<BCAvailabilityEntityBucket> buckets)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export
		public override List<BCAvailabilityEntityBucket> FetchBucketsExport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			BCEntityStats entityStats = GetEntityStats();
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			var invIDs = new List<string>();

			var warehouses = new Dictionary<string, INSite>();
			var locations = new Dictionary<string, string>();

			foreach (PXResult<BCLocations, INSite, INLocation> result in PXSelectJoin<BCLocations,
				InnerJoin<INSite, On<INSite.siteID, Equal<BCLocations.siteID>>,
				InnerJoin<INLocation, On<INLocation.siteID, Equal<BCLocations.siteID>, And<BCLocations.locationID, IsNull, Or<BCLocations.locationID, Equal<INLocation.locationID>>>>>>,
				Where<BCLocations.bindingID, Equal<Required<BCLocations.bindingID>>>>.Select(this, binding.BindingID))
			{
				var bl = (BCLocations)result;
				var site = (INSite)result;
				var location = (INLocation)result;
				warehouses[site.SiteCD.Trim()] = site;
				if (location != null && bl.LocationID != null)
				{
					locations[site.SiteCD.Trim()] = locations.ContainsKey(site.SiteCD.Trim()) ? (locations[site.SiteCD.Trim()] + "," + location.LocationCD) : location.LocationCD;
				}
				//If customer specifies the warehouse but not specifis the location, should include all locations and all items that not assigned to any location.
				if (bl.LocationID == null)
				{
					locations[site.SiteCD.Trim()] = string.Empty;
				}
			}
			Boolean anyLocation = warehouses.Any() && locations.Any(x => x.Value != string.Empty);

			StorageDetails request = new StorageDetails();
			request.Warehouse = string.Join(",", warehouses.Keys.ToArray()).ValueField();
			request.SplitByLocation = anyLocation.ValueField();
			StorageDetails response = cbapi.Put<StorageDetails>(request, null);

			//Synced Stock Items
			List<BCSyncStatus> parentEntities = PXSelect<BCSyncStatus,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<Where<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>, Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>
					>>>>.Select(this, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.ProductWithVariant).RowCast<BCSyncStatus>().ToList();
			List<StorageDetailsResult> results = new List<StorageDetailsResult>();
			if(response.Results != null && response.Results.Count>0)
				foreach (var detailsGroup in response.Results.GroupBy(r => new { InventoryID = r.InventoryID?.Value, /*SiteID = r.SiteID?.Value*/ }))
				{
					StorageDetailsResult result = detailsGroup.First();
					result.SiteLastModifiedDate = detailsGroup.Where(d => d.SiteLastModifiedDate != null).Select(d => d.SiteLastModifiedDate.Value).Max().ValueField();
					result.LocationLastModifiedDate = detailsGroup.Where(d => d.LocationLastModifiedDate != null).Select(d => d.LocationLastModifiedDate.Value).Max().ValueField();
					result.SiteOnHand = detailsGroup.Sum(k => k.SiteOnHand?.Value ?? 0m).ValueField();
					result.SiteAvailable = detailsGroup.Sum(k => k.SiteAvailable?.Value ?? 0m).ValueField();
					result.SiteAvailableforIssue = detailsGroup.Sum(k => k.SiteAvailableforIssue?.Value ?? 0m).ValueField();
					result.SiteAvailableforShipping = detailsGroup.Sum(k => k.SiteAvailableforShipping?.Value ?? 0m).ValueField();
					if(detailsGroup.Any(i => i.SiteID?.Value != null))
					{
						result.LocationOnHand = anyLocation ? detailsGroup.Where
							(k => warehouses.Count <= 0
							|| (locations.ContainsKey(k.SiteID?.Value?.Trim())
								&& (locations[k.SiteID?.Value?.Trim()] == string.Empty
								|| (k.LocationID?.Value != null
									&& locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value)))))
							.Sum(k => k.LocationOnHand?.Value ?? 0m).ValueField() : null;
						result.LocationAvailable = anyLocation ? detailsGroup.Where(
							k => warehouses.Count <= 0
							|| (locations.ContainsKey(k.SiteID?.Value?.Trim())
								&& (locations[k.SiteID?.Value?.Trim()] == string.Empty
								|| (k.LocationID?.Value != null
									&& locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value)))))
							.Sum(k => k.LocationAvailable?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforIssue = anyLocation ? detailsGroup.Where(
							k => warehouses.Count <= 0
							|| (locations.ContainsKey(k.SiteID?.Value?.Trim())
								&& (locations[k.SiteID?.Value?.Trim()] == string.Empty
								|| (k.LocationID?.Value != null
								&& locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value)))))
							.Sum(k => k.LocationAvailableforIssue?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforShipping = anyLocation ? detailsGroup.Where(
							k => warehouses.Count <= 0
							|| (locations.ContainsKey(k.SiteID?.Value?.Trim())
								&& (locations[k.SiteID?.Value?.Trim()] == string.Empty
								|| (k.LocationID?.Value != null
								&& locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value)))))
							.Sum(k => k.LocationAvailableforShipping?.Value ?? 0m).ValueField() : null;
					}else
						result.LocationOnHand = result.LocationAvailable = result.LocationAvailableforIssue = result.LocationAvailableforShipping = null;
					results.Add(result);
				}

			var allVariants = results.Where(x => x.TemplateItemID?.Value != null);
			//Filter results if specific entity provided or if we don't need to fetch new changes
			List<BCAvailabilityEntityBucket> buckets = new List<BCAvailabilityEntityBucket>();
			if (ids != null && ids.Count > 0 && (Operation.PrepareMode == PrepareMode.None || syncIDs != null))
			{
				var localIds = ids.Select(x => x.LocalID);
				results = results.Where(s => localIds.Contains(s.InventoryNoteID.Value))?.ToList();
			}
			if (results != null)
			{
				var stockItems = results.Where(x => x.TemplateItemID?.Value == null);
				if (stockItems != null)
					foreach (StorageDetailsResult line in stockItems)
					{
						Guid? noteID = line.InventoryNoteID?.Value;
						DateTime? lastModified;
						if (line.IsTemplate?.Value == true)
						{
							line.VariantDetails = new List<StorageDetailsResult>();
							line.VariantDetails.AddRange(allVariants.Where(x => x.TemplateItemID?.Value == line.InventoryID.Value));
							if (line.VariantDetails.Count() == 0) continue;
							lastModified = line.VariantDetails.Select(x => new DateTime?[] { x.LocationLastModifiedDate?.Value, x.SiteLastModifiedDate?.Value, x.InventoryLastModifiedDate.Value }.Where(d => d != null).Select(d => d.Value).Max()).Max();
						}
						else
						{
							lastModified = new DateTime?[] { line.LocationLastModifiedDate?.Value, line.SiteLastModifiedDate?.Value, line.InventoryLastModifiedDate.Value }.Where(d => d != null).Select(d => d.Value).Max();
						}
						BCSyncStatus current = ids.FirstOrDefault(s => s.LocalID == noteID);
						BCSyncStatus parent = parentEntities.FirstOrDefault(s => s.LocalID == noteID);
						if (parent == null || parent?.ExternID == null)
						{
							LogWarning(Operation.LogScope(current), BCMessages.LogAvailabilitySkippedItemNotSynced, line.InventoryID);
							continue; //if Stock is not found, skip  //TODO Logging
						}

						if (Operation.PrepareMode == PrepareMode.Incremental && current != null && current.PendingSync != true
							&& entityStats?.LastIncrementalExportDateTime != null && lastModified < entityStats.LastIncrementalExportDateTime)
							continue; //TODO Check if we need it at all.

						BCAvailabilityEntityBucket bucket = new BCAvailabilityEntityBucket();
						MappedAvailability obj = bucket.Product = new MappedAvailability(line, noteID, lastModified, parent.SyncID);
						EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
						obj.ParentID = parent.SyncID;
						if (Operation.PrepareMode != PrepareMode.Reconciliation && Operation.PrepareMode != PrepareMode.Full && status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force)
						{
							SynchronizeStatus(bucket.Product, BCSyncOperationAttribute.Reconfiguration);
							Statuses.Cache.Persist(PXDBOperation.Update);
							Statuses.Cache.Persisted(false);
							continue;
						}
						invIDs.Add(line?.InventoryID?.Value);

						buckets.Add(bucket);
					}

			}

			return buckets;
		}


		public override void MapBucketExport(BCAvailabilityEntityBucket bucket, IMappedEntity existing)
		{
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			MappedAvailability obj = bucket.Product;

			StorageDetailsResult impl = obj.Local;
			ProductQtyData data = obj.Extern = new ProductQtyData();

			BCSyncStatus parentStatus = BCSyncStatus.PK.Find(this, obj.ParentID);
			data.Id = parentStatus.ExternID.ToInt();

			string availability = impl.Availability?.Value;
			if (availability == null || availability == BCCaptions.StoreDefault) availability = BCItemAvailabilities.Convert(bindingExt.Availability);
			string notAvailMode = impl.NoQtyMode?.Value;
			if (notAvailMode == null || notAvailMode == BCCaptions.StoreDefault) notAvailMode = BCItemNotAvailModes.Convert(bindingExt.NotAvailMode);
			if (availability == BCCaptions.AvailableTrack)
			{
				data.Availability = "available";

				if (impl.IsTemplate?.Value == true)
				{
					data.InventoryTracking = "variant";
					data.Variants = new List<ProductsVariantData>();
					foreach (var variant in impl.VariantDetails)
					{
						ProductsVariantData variantData = new ProductsVariantData();
						BCSyncDetail variantStatus = PXSelectJoin<BCSyncDetail,
						InnerJoin<InventoryItem, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncDetail.localID>>,
						InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>>,
						Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncDetail.localID, Equal<Required<BCSyncDetail.localID>>>>>>>
						.Select(this, BCEntitiesAttribute.ProductWithVariant, variant.InventoryNoteID.Value);

						if (variantStatus?.ExternID != null)
						{
							variantData.Id = variantStatus?.ExternID.ToInt();
							variantData.ProductId = data.Id;
							variantData.OptionValues = null;
							//Inventory Level
							variantData.InventoryLevel = GetInventoryLevel(bindingExt, variant);
							if (variantData.InventoryLevel < 0)
								variantData.InventoryLevel = 0;
							data.Variants.Add(variantData);

						}
					}
					if (data.Variants.All(x => x.InventoryLevel <= 0))
					{
						switch (notAvailMode)
						{
							case BCCaptions.DisableItem:
								data.Availability = "disabled";
								break;
							case BCCaptions.PreOrderItem:
								data.Availability = "preorder";
								break;
						}
					}
				}
				else
				{
					data.InventoryTracking = "product";
					//Inventory Level
					data.InventoryLevel = GetInventoryLevel(bindingExt, impl);
					//Not In Stock mode
					if (data.InventoryLevel <= 0)
					{
						data.InventoryLevel = 0;

						switch (notAvailMode)
						{
							case BCCaptions.DisableItem:
								data.Availability = "disabled";
								break;
							case BCCaptions.PreOrderItem:
								data.Availability = "preorder";
								break;
						}
					}
				}

			}
			else
			{
				data.InventoryTracking = "none";

				switch (availability)
				{
					case BCCaptions.AvailableSkip: data.Availability = "available"; break;
					case BCCaptions.PreOrder: data.Availability = "preorder"; break;
					case BCCaptions.Disabled: data.Availability = "disabled"; break;
				}
			}

			Boolean isItemActive = !(impl.ItemStatus?.Value == PX.Objects.IN.Messages.Inactive || impl.ItemStatus?.Value == PX.Objects.IN.Messages.ToDelete || impl.ItemStatus?.Value == PX.Objects.IN.Messages.NoSales);
			if (!isItemActive)
			{
				data.Availability = "disabled";
			}

			if (data.Availability == "disabled")
				data.IsPriceHidden = true;

		}

		public int GetInventoryLevel(BCBindingExt store, StorageDetailsResult detailsResult)
		{
			switch (store.AvailabilityCalcRule)
			{
				case BCAvailabilityLevelsAttribute.Available:
					return (int)(detailsResult.LocationAvailable?.Value ?? detailsResult.SiteAvailable.Value);
				case BCAvailabilityLevelsAttribute.AvailableForShipping:
					return (int)(detailsResult.LocationAvailableforShipping?.Value ?? detailsResult.SiteAvailableforShipping.Value);
				case BCAvailabilityLevelsAttribute.OnHand:
					return (int)(detailsResult.LocationOnHand?.Value ?? detailsResult.SiteOnHand.Value);
				default:
					return 0;
			}
		}

		public override void SaveBucketsExport(List<BCAvailabilityEntityBucket> buckets)
		{
			productDataProvider.UpdateAllQty(buckets.Select(b => b.Product.Extern).ToList(), delegate (ItemProcessCallback<ProductQtyData> callback)
			{
				Exception Error = null;
				BCAvailabilityEntityBucket bucket = buckets[callback.Index];
				if (callback.IsSuccess)
				{
					ProductQtyData data = callback.Result;
					if (bucket.Product.Extern.Variants != null && bucket.Product.Extern.Variants.Count > 0)
					{
						variantBatchRestDataProvider.UpdateAll(bucket.Product.Extern.Variants.ToList(), delegate (ItemProcessCallback<ProductsVariantData> callbackVariant)
						{
							if (!callbackVariant.IsSuccess)
							{
								Error = callbackVariant.Error;
							}
						});
					}
					if (Error == null)
					{
						bucket.Product.AddExtern(data, data.Id?.ToString(), data.DateModified);
						UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternUpdate);
                        Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
                    }
					else
					{
						Log(bucket.Product?.SyncID, SyncDirection.Export, Error);
						UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternFailed, Error.ToString());
                        Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Error, Error));
                    }
				}
				else
				{
					productDataProvider.UpdateAllQty(new List<ProductQtyData>() { bucket.Product.Extern }, delegate (ItemProcessCallback<ProductQtyData> retrycallback)
					{
						if (retrycallback.IsSuccess)
						{
							ProductQtyData data = retrycallback.Result;
							bucket.Product.AddExtern(data, data.Id?.ToString(), data.DateModified);
							UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternUpdate);
                            Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
                        }
						else
						{
							if (retrycallback.Error?.ResponceStatusCode == "422") //id not found
							{
								DeleteStatus(BCSyncStatus.PK.Find(this, bucket.Product.ParentID), BCSyncOperationAttribute.NotFound);
								DeleteStatus(bucket.Product, BCSyncOperationAttribute.NotFound);
                                Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Deleted));
                            }
							else
							{
								Log(bucket.Product?.SyncID, SyncDirection.Export, retrycallback.Error);

								UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternFailed, retrycallback.Error.ToString());
                                Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Error, retrycallback.Error));
                            }
						}
					});
				}

			});
		}
		#endregion
	}
}
