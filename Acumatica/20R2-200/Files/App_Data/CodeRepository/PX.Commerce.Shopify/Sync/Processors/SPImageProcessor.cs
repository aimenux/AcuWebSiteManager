using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Net;

namespace PX.Commerce.Shopify
{
	public class SPImageEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Image;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedProductImage Image;
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.ProductImage, BCCaptions.ProductImage,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.SM.WikiFileMaintenance),
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(PX.SM.UploadFileWithIDSelector),
		URL = "products/{0}",
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.NonStockItem + "." + BCEntitiesAttribute.ProductWithVariant }
	)]
	public class SPImageProcessor : BCProcessorBulkBase<SPImageProcessor, SPImageEntityBucket, MappedStockItem>, IProcessor
	{
		protected IParentRestDataProvider<ProductData> productDataProvider;
		protected IChildRestDataProvider<ProductImageData> productImageDataProvider;
		protected Dictionary<string, List<ProductImageData>> existingImages;
		protected UploadFileMaintenance uploadGraph;
		protected BCBinding currentBinding;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			currentBinding = GetBinding();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			productDataProvider = new ProductRestDataProvider(client);
			productImageDataProvider = new ProductImageRestDataProvider(client);
			uploadGraph = PXGraph.CreateInstance<UploadFileMaintenance>();
			existingImages = new Dictionary<string, List<ProductImageData>>(); 
		}
		#endregion

		#region Import
		public override void SaveBucketsImport(List<SPImageEntityBucket> buckets)
		{
			throw new NotImplementedException();
		}

		public override List<SPImageEntityBucket> FetchBucketsImport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export
		public override void SaveBucketsExport(List<SPImageEntityBucket> buckets)
		{
			foreach (SPImageEntityBucket bucket in buckets)
			{
				try
				{
					var localObj = bucket.Image.Local;

					// to avoid duplication of image linking to product
					#region Check if Exists
					
					ProductImageData productImageData = null;
					List<ProductImageData> imageList = null;
					var externalIds = localObj.ExternalInventoryID.Split(';');
					var externalProductId = externalIds.First();
					var externalVariantId = externalIds.Length == 2 ? externalIds.LastOrDefault() : null;
					if (!existingImages.TryGetValue(externalProductId, out imageList))
					{
						try
						{
							imageList = productImageDataProvider.GetAll(externalProductId, new FilterWithFields() { Fields = "id,product_id,src,variant_ids,position"}).ToList();
						}
						catch (RestException ex)
						{
							SPImageEntityBucket failBucket = new SPImageEntityBucket() { Image = bucket.Image };
							if (failBucket.Primary != null)
							{
								if (ex.ResponceStatusCode == HttpStatusCode.NotFound.ToString())
								{
									DeleteStatus(BCSyncStatus.PK.Find(this, bucket.Image.ParentID), BCSyncOperationAttribute.NotFound);
									DeleteStatus(bucket.Image, BCSyncOperationAttribute.NotFound);
									Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Deleted));
								}
								else
								{
									Log(failBucket.Primary.SyncID, SyncDirection.Export, ex);
									UpdateStatus(failBucket.Primary, BCSyncOperationAttribute.ExternFailed, ex.Message);
									Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Error, ex));
								}
							}
							continue;
						}
						if (imageList != null && imageList.Count() > 0)
						{
							
							existingImages.Add(externalProductId, imageList);
						}
					}
					if (imageList != null)
					{
						productImageData = imageList.Where(x => (x.Metafields != null && x.Metafields.Any(m => m.Key == "ProductImage" && m.Value == localObj.FileNoteID?.Value?.ToString()))).FirstOrDefault();
						if (productImageData != null && (externalVariantId == null || (externalVariantId != null && productImageData.VariantIds.Contains(externalVariantId.ToLong()))))
						{
                            bucket.Image.ExternID = null;
                            bucket.Image.AddExtern(productImageData, new object[] { localObj.ExternalInventoryID, productImageData.Id.ToString() }.KeyCombine(), productImageData.DateModifiedAt.ToDate(false));
							UpdateStatus(bucket.Image, BCSyncOperationAttribute.ExternUpdate);
							Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
							continue;
						}
					};
					#endregion
					if (bucket.Image.ExternID != null && Operation.SyncMethod != SyncMode.Force)
					{
						DeleteStatus(bucket.Image, BCSyncOperationAttribute.ExternDelete);
						Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Deleted));
						continue;
					}

					ProductImageData newImageData = null;
					if(productImageData != null)
					{//For Update
						newImageData = new ProductImageData() { Id = productImageData.Id };
						if (externalVariantId != null) 
							newImageData.VariantIds = new long?[] { externalVariantId.ToLong() };
						if (externalVariantId == null && localObj.IsDefault?.Value == true)
							newImageData.Position = 1;
						newImageData = productImageDataProvider.Update(newImageData, externalProductId, productImageData.Id.ToString());
					}
					else
					{ //For Creation
						FileInfo file = uploadGraph.GetFile(localObj.FileID.Value.Value);
						if (file == null)
							return;
						localObj.Content = file.BinData;

						//Convert to BaseString64
						string base64Image = Convert.ToBase64String(localObj.Content);
						string fileName = string.Format("{0}_{1}.{2}", localObj.FileNoteID?.Value.ToString(), localObj.InventoryID?.Value, localObj.Extension?.Value);
						newImageData = new ProductImageData()
						{
							Attachment = base64Image,
							Filename = fileName,
							Metafields = new List<MetafieldData>() { new MetafieldData() { Key = "ProductImage", Value = localObj.FileNoteID.Value.ToString(), ValueType = ShopifyCaptions.ValueType_String, Namespace = ShopifyCaptions.Namespace_Global } },
						};
						var metafields = newImageData.Metafields;
						if (externalVariantId != null)
							newImageData.VariantIds = new long?[] { externalVariantId.ToLong() };
						if (externalVariantId == null && localObj.IsDefault?.Value == true)
							newImageData.Position = 1;
						newImageData = productImageDataProvider.Create(newImageData, externalProductId);
						if (existingImages.ContainsKey(externalProductId))
						{
							newImageData.Metafields = metafields;
							existingImages[externalProductId].Add(newImageData);
						}
					}

					bucket.Image.ExternID = null;
					bucket.Image.AddExtern(newImageData, new object[] { localObj.ExternalInventoryID, newImageData.Id.ToString() }.KeyCombine(), newImageData.DateModifiedAt.ToDate(false));
					UpdateStatus(bucket.Image, BCSyncOperationAttribute.ExternUpdate);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
				}
				catch (Exception ex)
				{
					Log(bucket?.Primary?.SyncID, SyncDirection.Export, ex);
					UpdateStatus(bucket.Image, BCSyncOperationAttribute.ExternFailed, ex.InnerException?.Message ?? ex.Message);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Error, ex));
				}
			}
		}

		public override List<SPImageEntityBucket> FetchBucketsExport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			BCEntityStats entityStats = GetEntityStats();

			List<SPImageEntityBucket> buckets = new List<SPImageEntityBucket>();
			List<dynamic> entitiesData = new List<dynamic>();
			foreach(PXResult<BCSyncStatus,BCSyncDetail> result in SelectFrom<BCSyncStatus>.LeftJoin<BCSyncDetail>.On<BCSyncStatus.syncID.IsEqual<BCSyncDetail.syncID>>.
				Where<BCSyncStatus.connectorType.IsEqual<@P.AsString>.
				And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>.
				And<Brackets<BCSyncStatus.entityType.IsEqual<@P.AsString>.Or<BCSyncStatus.entityType.IsEqual<@P.AsString>.Or<BCSyncStatus.entityType.IsEqual<@P.AsString>>>>>>>.
				View.Select(this, currentBinding.ConnectorType, currentBinding.BindingID, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.ProductWithVariant))
			{
				var syncRecord = (BCSyncStatus)result;
				var recordDetail = (BCSyncDetail)result;
				if (syncRecord != null && syncRecord.PendingSync != true && syncRecord.Deleted != true)
				{
					entitiesData.Add(new
					{
						PSyncID = syncRecord.SyncID,
						PLocalID = syncRecord.LocalID,
						PExternID = syncRecord.ExternID,
						PEntityType = syncRecord.EntityType,
						CSyncID = recordDetail.SyncID,
						CLocalID = recordDetail.LocalID,
						CExternID = recordDetail.ExternID
					});
				}
			}

			if (entitiesData == null || entitiesData.Count <= 0) return buckets;

			// Get Files meta data
			ItemImages response = cbapi.Put<ItemImages>(new ItemImages(), null);
			if (response.Results == null || response.Results.Count == 0) return buckets;

            var originalResultList = response.Results.GroupBy(x => x.InventoryNoteID?.Value).Select(g => new { Key = g.Key, Count = g.Count(), hasDefault = g.Any(x => x.IsDefault?.Value == true), ItemList = g.ToList()}).ToList();
			if (ids != null && ids.Count > 0 && (Operation.PrepareMode == PrepareMode.None || syncIDs != null))
			{
				var localIds = ids.Select(x => x.LocalID);
				response.Results = response.Results.Where(s => localIds.Contains(s.FileNoteID.Value))?.ToList();
				if (response.Results == null || response.Results?.Count == 0) return buckets;
			}
			foreach (ItemImageDetails impl in response.Results)
			{
				var productSyncRecord = entitiesData.FirstOrDefault(p => (p.PEntityType == BCEntitiesAttribute.ProductWithVariant && ((Guid?)p.CLocalID) == impl.InventoryNoteID?.Value) || (p.PEntityType != BCEntitiesAttribute.ProductWithVariant && ((Guid?)p.PLocalID) == impl.InventoryNoteID?.Value));
				if (productSyncRecord == null || productSyncRecord.PExternID == null) continue;
				impl.SyncTime = new DateTime?[] { impl.LastModifiedDateTime?.Value, impl.InventoryLastModifiedDateTime?.Value }.Where(d => d != null).Select(d => d.Value).Max();
				if (Operation.PrepareMode == PrepareMode.Incremental)
				{
					if (entityStats?.LastIncrementalExportDateTime != null && impl.SyncTime < entityStats.LastIncrementalExportDateTime)
						continue;
				}
				string externId = productSyncRecord.PExternID;
				if (productSyncRecord.CLocalID != null && ((Guid?)productSyncRecord.CLocalID) == impl.InventoryNoteID?.Value && productSyncRecord.CExternID != null)
				{
                    if(impl.IsDefault?.Value == true)
                    {
                        externId = $"{productSyncRecord.PExternID};{productSyncRecord.CExternID}";
                    }
                    else
                    {
                        var sameImpls = originalResultList.FirstOrDefault(x => x.Key == impl.InventoryNoteID?.Value);
                        if ((sameImpls.Count == 1) || (sameImpls.hasDefault == false && sameImpls.ItemList.First().FileNoteID == impl.FileNoteID))
                        {
                            externId = $"{productSyncRecord.PExternID};{productSyncRecord.CExternID}";
                        }
                    }
				}
				impl.ExternalInventoryID = externId;

				MappedProductImage obj = new MappedProductImage(impl, impl.FileNoteID.Value, impl.SyncTime, ((int?)productSyncRecord.PSyncID));
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				if (Operation.PrepareMode != PrepareMode.Reconciliation && status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force) continue;

				buckets.Add(new SPImageEntityBucket() { Image = obj });

			}
			return buckets;

		}
		#endregion
	}
}
