using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;
using PX.Objects.Common;
using PX.Data.BQL.Fluent;

namespace PX.Commerce.Shopify
{
	public class SPAvailabilityEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public MappedAvailability Product;
		public long? ExternProductID;
		public long? ExternProductVariantID;
		public Dictionary<string, List<StorageDetailsResult>> LocationMappings = new Dictionary<string, List<StorageDetailsResult>>();
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.ProductAvailability, BCCaptions.ProductAvailability,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.InventorySummaryEnq),
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(InventoryItem),
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.ProductWithVariant },
		URL = "products/{0}"
		)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-AvailabilityStockItem", "BC-PUSH-AvailabilityTemplates" })]
	public class SPAvailabilityProcessor : BCProcessorBulkBase<SPAvailabilityProcessor, SPAvailabilityEntityBucket, MappedAvailability>, IProcessor
	{
		protected InventoryLevelRestDataProvider levelProvider;
		protected ProductVariantRestDataProvider productVariantDataProvider;
		protected IEnumerable<InventoryLocationData> inventoryLocations;
		protected BCBinding currentBinding;
		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			currentBinding = GetBinding();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			levelProvider = new InventoryLevelRestDataProvider(client);
			productVariantDataProvider = new ProductVariantRestDataProvider(client);
			inventoryLocations = ConnectorHelper.GetConnector(currentBinding.ConnectorType)?.GetExternalInfo<InventoryLocationData>(BCObjectsConstants.BCInventoryLocation, currentBinding.BindingID);
			if (inventoryLocations == null || inventoryLocations.Count() == 0)
			{
				throw new PXException(ShopifyMessages.InventoryLocationNotFound);
			}
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

		public override MappedAvailability PullEntity(Guid? localID, Dictionary<string, object> fields)
		{
			if (localID == null) return null;
			DateTime? timeStamp = fields.Where(f => f.Key.EndsWith(nameof(BCEntity.LastModifiedDateTime), StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToDate();
			int? parentID = fields.Where(f => f.Key.EndsWith(nameof(BCSyncStatus.SyncID), StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToInt();
			localID = fields.Where(f => f.Key.EndsWith("TemplateItem_noteID", StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToGuid() ?? localID;
			return new MappedAvailability(new StorageDetailsResult(), localID, timeStamp, parentID);
		}
		#endregion

		#region Import
		public override List<SPAvailabilityEntityBucket> FetchBucketsImport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			return null;
		}
		public override void MapBucketImport(SPAvailabilityEntityBucket bucket, IMappedEntity existing)
		{
			throw new NotImplementedException();
		}
		public override void SaveBucketsImport(List<SPAvailabilityEntityBucket> buckets)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export
		public override List<SPAvailabilityEntityBucket> FetchBucketsExport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			List<SPAvailabilityEntityBucket> buckets = new List<SPAvailabilityEntityBucket>();
			BCEntityStats entityStats = GetEntityStats();
			var invIDs = new List<string>();

			var warehouses = new Dictionary<string, INSite>();
			var locations = new Dictionary<string, string>();
			var allVariantsData = productVariantDataProvider.GetAllWithoutParent(new FilterWithFields() { Fields = "id,product_id,sku,inventory_item_id" }).ToList();
			if (allVariantsData == null && allVariantsData.Count() == 0) return buckets;

			foreach (PXResult<BCLocations, INSite, INLocation> result in PXSelectJoin<BCLocations,
			InnerJoin<INSite, On<INSite.siteID, Equal<BCLocations.siteID>>,
			InnerJoin<INLocation, On<INLocation.siteID, Equal<BCLocations.siteID>, And<BCLocations.locationID, IsNull, Or<BCLocations.locationID, Equal<INLocation.locationID>>>>>>,
			Where<BCLocations.bindingID, Equal<Required<BCLocations.bindingID>>>>.Select(this, currentBinding.BindingID))
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

			List<dynamic> entitiesData = new List<dynamic>();
			foreach (PXResult<BCSyncStatus, BCSyncDetail> result in SelectFrom<BCSyncStatus>.
				LeftJoin<BCSyncDetail>.On<BCSyncStatus.syncID.IsEqual<BCSyncDetail.syncID>>.
				Where<BCSyncStatus.connectorType.IsEqual<@P.AsString>.
				And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>.
				And<Brackets<BCSyncStatus.entityType.IsEqual<@P.AsString>.Or<BCSyncStatus.entityType.IsEqual<@P.AsString>.Or<BCSyncStatus.entityType.IsEqual<@P.AsString>>>>>>>.
				View.Select(this, currentBinding.ConnectorType, currentBinding.BindingID, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.ProductWithVariant))
			{
				var syncRecord = (BCSyncStatus)result;
				var recordDetail = (BCSyncDetail)result;

				if (syncRecord != null && syncRecord.PendingSync != true && syncRecord.Deleted != true)
				{
					var variantData = allVariantsData.FirstOrDefault(x => x.ProductId.ToString() == syncRecord.ExternID && !string.IsNullOrEmpty(recordDetail.ExternID) && x.Id.ToString() == recordDetail.ExternID) ??
						allVariantsData.FirstOrDefault(x => x.ProductId.ToString() == syncRecord.ExternID && (string.IsNullOrEmpty(recordDetail.ExternID) || x.Id.ToString() == recordDetail.ExternID));
					if (variantData == null) continue;
					entitiesData.Add(new
					{
						PSyncID = syncRecord.SyncID,
						PLocalID = syncRecord.LocalID,
						PExternID = variantData.ProductId,
						PEntityType = syncRecord.EntityType,
						CSyncID = recordDetail.SyncID,
						CLocalID = recordDetail.LocalID,
						CExternID = variantData.Id,
						InventoryItemID = variantData.InventoryItemId.ToString()
					});
				}
			}
			List<StorageDetailsResult> results = new List<StorageDetailsResult>();
			if (response.Results != null && response.Results.Count > 0)
			{
				foreach (var detailsGroup in response.Results.GroupBy(r => new { InventoryID = r.InventoryID?.Value, /*SiteID = r.SiteID?.Value*/ }))
				{
					StorageDetailsResult result = detailsGroup.First();
					result.SiteLastModifiedDate = detailsGroup.Where(d => d.SiteLastModifiedDate != null).Select(d => d.SiteLastModifiedDate.Value).Max().ValueField();
					result.LocationLastModifiedDate = detailsGroup.Where(d => d.LocationLastModifiedDate != null).Select(d => d.LocationLastModifiedDate.Value).Max().ValueField();
					result.SiteOnHand = detailsGroup.Sum(k => k.SiteOnHand?.Value ?? 0m).ValueField();
					result.SiteAvailable = detailsGroup.Sum(k => k.SiteAvailable?.Value ?? 0m).ValueField();
					result.SiteAvailableforIssue = detailsGroup.Sum(k => k.SiteAvailableforIssue?.Value ?? 0m).ValueField();
					result.SiteAvailableforShipping = detailsGroup.Sum(k => k.SiteAvailableforShipping?.Value ?? 0m).ValueField();
					if (detailsGroup.Any(i => i.SiteID?.Value != null))
					{
						result.LocationOnHand = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (locations.ContainsKey(k.SiteID?.Value?.Trim()) && (locations[k.SiteID?.Value?.Trim()] == string.Empty || (k.LocationID?.Value != null && locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value))))).Sum(k => k.LocationOnHand?.Value ?? 0m).ValueField() : null;
						result.LocationAvailable = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (locations.ContainsKey(k.SiteID?.Value?.Trim()) && (locations[k.SiteID?.Value?.Trim()] == string.Empty || (k.LocationID?.Value != null && locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value))))).Sum(k => k.LocationAvailable?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforIssue = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (locations.ContainsKey(k.SiteID?.Value?.Trim()) && (locations[k.SiteID?.Value?.Trim()] == string.Empty || (k.LocationID?.Value != null && locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value))))).Sum(k => k.LocationAvailableforIssue?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforShipping = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (locations.ContainsKey(k.SiteID?.Value?.Trim()) && (locations[k.SiteID?.Value?.Trim()] == string.Empty || (k.LocationID?.Value != null && locations[k.SiteID?.Value?.Trim()].Contains(k.LocationID?.Value))))).Sum(k => k.LocationAvailableforShipping?.Value ?? 0m).ValueField() : null;
					}
					else
						result.LocationOnHand = result.LocationAvailable = result.LocationAvailableforIssue = result.LocationAvailableforShipping = null;
					results.Add(result);
				}
			}
			if (results == null || results.Count() == 0) return buckets;
			if (ids != null && ids.Count > 0 && (Operation.PrepareMode == PrepareMode.None || syncIDs != null))
			{
				var localIds = ids.Select(x => x.LocalID);
				results = results.Where(s => localIds.Contains(s.InventoryNoteID.Value))?.ToList();
				if (results == null || results?.Count() == 0) return buckets;
			}

			foreach (StorageDetailsResult line in results)
			{
				Guid? noteID = line.InventoryNoteID?.Value;
				var productSyncRecord = entitiesData.FirstOrDefault(p => (p.PEntityType == BCEntitiesAttribute.ProductWithVariant && ((Guid?)p.CLocalID) == noteID) || (p.PEntityType != BCEntitiesAttribute.ProductWithVariant && ((Guid?)p.PLocalID) == noteID));
				if (productSyncRecord == null) continue;
				DateTime? lastModified;
				lastModified = new DateTime?[] { line.LocationLastModifiedDate?.Value, line.SiteLastModifiedDate?.Value, line.InventoryLastModifiedDate.Value }.Where(d => d != null).Select(d => d.Value).Max();

				if (Operation.PrepareMode == PrepareMode.Incremental && entityStats?.LastIncrementalExportDateTime != null && lastModified < entityStats.LastIncrementalExportDateTime)
					continue;

				SPAvailabilityEntityBucket bucket = new SPAvailabilityEntityBucket();
				MappedAvailability obj = bucket.Product = new MappedAvailability(line, noteID, lastModified, ((int?)productSyncRecord.PSyncID));
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				var externId = new object[] { productSyncRecord.PExternID, productSyncRecord.InventoryItemID }.KeyCombine();
				if (obj.ExternID == null || obj.ExternID != externId)
					obj.ExternID = externId;
				if (!bucket.LocationMappings.ContainsKey(inventoryLocations.First()?.Id.ToString()))
					bucket.LocationMappings.Add(inventoryLocations.First()?.Id.ToString(), new List<StorageDetailsResult>() { line });
				bucket.ExternProductID = productSyncRecord.PExternID;
				bucket.ExternProductVariantID = productSyncRecord.CExternID;
				if (Operation.PrepareMode != PrepareMode.Reconciliation && status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force) continue;

				buckets.Add(bucket);
			}

			return buckets;
		}

		public int GetInventoryLevel(BCBindingExt bindingExt, StorageDetailsResult detailsResult)
		{
			switch (bindingExt.AvailabilityCalcRule)
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

		public override void SaveBucketsExport(List<SPAvailabilityEntityBucket> buckets)
		{
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			foreach (var bucket in buckets)
			{
				MappedAvailability obj = bucket.Product;
				StorageDetailsResult impl = obj.Local;
				obj.Extern = new InventoryLevelData();
				InventoryLevelData data = null;

				var errorMsg = string.Empty;
				Boolean isItemActive = !(impl.ItemStatus?.Value == PX.Objects.IN.Messages.Inactive || impl.ItemStatus?.Value == PX.Objects.IN.Messages.ToDelete || impl.ItemStatus?.Value == PX.Objects.IN.Messages.NoSales);
				string availability = impl.Availability?.Value;
				if (availability == null || availability == BCCaptions.StoreDefault)
				{
					availability = BCItemAvailabilities.Convert(bindingExt.Availability);
				}
				string notAvailMode = impl.NoQtyMode?.Value;
				if (notAvailMode == null || notAvailMode == BCCaptions.StoreDefault)
				{
					notAvailMode = BCItemNotAvailModes.Convert(bindingExt.NotAvailMode);
				}

				errorMsg += UpdateVariantInfo(bucket.ExternProductID, bucket.ExternProductVariantID, availability, notAvailMode, isItemActive);
				//Update invenotry only if availability is set to "track qty".
				if (availability == BCCaptions.AvailableTrack)
				{
					foreach (var locationItem in bucket.LocationMappings)
					{
						data = new InventoryLevelData();
						var externId = obj.ExternID.KeySplit(1, obj.ExternID.KeySplit(0));
						data.InventoryItemId = externId.ToLong();
						data.LocationId = locationItem.Key.ToLong();
						data.Available = isItemActive ? locationItem.Value.Sum(x => GetInventoryLevel(bindingExt, x)) : 0;
						data.DisconnectIfNecessary = true;
						try
						{
							data = levelProvider.SetInventory(data);
						}
						catch (Exception ex)
						{
							Log(bucket?.Primary?.SyncID, SyncDirection.Export, ex);
							errorMsg += ex.InnerException?.Message ?? ex.Message + "\n";
						}
					}
				}

				if (!string.IsNullOrEmpty(errorMsg))
				{
					UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternFailed, errorMsg);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Error, new Exception(errorMsg)));
				}
				else
				{
					bucket.Product.ExternID = new object[] { obj.ExternID }.KeyCombine();
					bucket.Product.AddExtern(data, obj.ExternID, data?.DateModifiedAt.ToDate(false));
					UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternUpdate);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
				}
			}

		}

		public string UpdateVariantInfo(long? productId, long? productVariantId, string availability, string notAvailMode, bool isItemActive)

		{
			string errorMsg = string.Empty;
			var variantData = new ProductVariantData();
			variantData.Id = productVariantId;
			variantData.ProductId = productId;

			if (availability == BCCaptions.AvailableTrack)
			{
				variantData.InventoryManagement = ShopifyCaptions.InventoryManagement_Shopify;
			}
			else
			{
				variantData.InventoryManagement = null;
			}

			switch (notAvailMode)
			{
				case BCCaptions.DisableItem:
					{
						variantData.InventoryPolicy = InventoryPolicy.Deny;
						break;
					}
				case BCCaptions.DoNothing:
				case BCCaptions.PreOrderItem:
					{
						variantData.InventoryPolicy = isItemActive ? InventoryPolicy.Continue : InventoryPolicy.Deny;
						break;
					}
			}
			try
			{
				productVariantDataProvider.Update(variantData, productId.ToString(), productVariantId.ToString());
			}
			catch (Exception ex)
			{
				errorMsg = ex.InnerException?.Message ?? ex.Message;
			}
			return errorMsg;
		}
		#endregion
	}
}
