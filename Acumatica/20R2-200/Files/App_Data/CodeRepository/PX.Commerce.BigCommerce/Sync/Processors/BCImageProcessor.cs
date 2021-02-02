using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.BigCommerce.API.WebDAV;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PX.Commerce.BigCommerce
{
	public class BCImageEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Image;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedProductImage Image;
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.ProductImage, BCCaptions.ProductImage,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.SM.WikiFileMaintenance),
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(PX.SM.UploadFileWithIDSelector),
		URL = "products/{0}/edit",
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.NonStockItem }
	)]
	public class BCImageProcessor : BCProcessorBulkBase<BCImageProcessor, BCImageEntityBucket, MappedStockItem>, IProcessor
	{
		protected IChildRestDataProvider<ProductsImageData> productImageDataProvider;
		protected VariantImageDataProvider variantImageDataProvider;
		protected IBigCommerceWebDAVClient webDavClient;
		protected UploadFileMaintenance uploadGraph;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			productImageDataProvider = new ProductImagesDataProvider(client);
			variantImageDataProvider = new VariantImageDataProvider(client);
			uploadGraph = PXGraph.CreateInstance<UploadFileMaintenance>();

			webDavClient = BCConnector.GetWebDavClient(GetBindingExt<BCBindingBigCommerce>());
		}
		#endregion

		#region Import
		public override void SaveBucketsImport(List<BCImageEntityBucket> buckets)
		{
			throw new NotImplementedException();
		}

		public override List<BCImageEntityBucket> FetchBucketsImport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export
		public override void SaveBucketsExport(List<BCImageEntityBucket> buckets)
		{
			Dictionary<string, List<ProductsImageData>> existingImages = new Dictionary<string, List<ProductsImageData>>();
			foreach (BCImageEntityBucket bucket in buckets)
			{
				try
				{
					var localObj = bucket.Image.Local;
					if (localObj.TemplateItemID?.Value == null)//for product
					{
						// to avoid duplication of image linking to product
						#region Check if Exists
						ProductsImageData productsImageData = null;
						List<ProductsImageData> files = new List<ProductsImageData>();

						if (!existingImages.TryGetValue(localObj.ExternalInventoryID, out files))
						{
							IList<ProductsImageData> productImages;
							try
							{
								productImages = productImageDataProvider.Get(localObj.ExternalInventoryID);
							}
							catch (RestException ex)
							{
								BCImageEntityBucket failBucket = new BCImageEntityBucket() { Image = bucket.Image };
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
							if (productImages != null && productImages.Count() > 0)
							{

								files = productImages.ToList();
								existingImages.Add(localObj.ExternalInventoryID, files);
							}
						}
						if (files != null && files.Count() > 0)
						{
							productsImageData = files.Where(x => x.ImageFile.Contains(localObj.FileID?.Value?.ToString())).FirstOrDefault();
							if (productsImageData != null)
							{
								//to update default image
								if (localObj.IsDefault.Value.Value != productsImageData.IsThumbnail)
								{
									productsImageData.IsThumbnail = true;
									productsImageData = productImageDataProvider.Update(productsImageData, productsImageData.Id.ToString(), localObj.ExternalInventoryID);
								}

								bucket.Image.AddExtern(productsImageData, new object[] { localObj.ExternalInventoryID, productsImageData.Id.ToString() }.KeyCombine(), productsImageData.DateModified.ToDate());
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
						//Get File Content
						var data=UploadFile(localObj);
						if (data != null)
						{
							// Link image to product
							if (localObj.IsDefault.Value.Value)
							{
								data.IsThumbnail = true;
								data.UrlThumbnail = data.ImageUrl;
							}

							productsImageData = productImageDataProvider.Create(data, localObj.ExternalInventoryID);

							bucket.Image.ExternID = null;

							bucket.Image.AddExtern(productsImageData, new object[] { localObj.ExternalInventoryID, productsImageData.Id.ToString() }.KeyCombine(), productsImageData.DateModified.ToDate());
							UpdateStatus(bucket.Image, BCSyncOperationAttribute.ExternUpdate);
                            Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
                        }
					}
					else// for variants
					{
						//Get File Content
						var data = UploadFile(localObj);
						var productsImageData =variantImageDataProvider.Create(data,  localObj.ExternalInventoryID, localObj.ExternalVarientID);

						bucket.Image.ExternID = null;
						var externId = new object[] { localObj.ExternalInventoryID, localObj.ExternalVarientID }.KeyCombine();
						BCSyncStatus status = PXSelect<BCSyncStatus, Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, BCEntitiesAttribute.ProductImage, externId);
						if (status != null) Statuses.Delete(status);
						bucket.Image.AddExtern(productsImageData, new object[] { localObj.ExternalInventoryID, localObj.ExternalVarientID }.KeyCombine(), productsImageData.CalculateHash());
						UpdateStatus(bucket.Image, BCSyncOperationAttribute.ExternUpdate);
                        Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
                    }
				}
				catch (Exception ex)
				{
					Log(bucket?.Primary?.SyncID, SyncDirection.Export, ex);
					UpdateStatus(bucket.Image, BCSyncOperationAttribute.ExternFailed, ex.InnerException?.Message ?? ex.Message);
                    Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Error, ex));
                }
			}
		}

		public virtual ProductsImageData UploadFile(ItemImageDetails localObj)
		{
			int retryCount = 0;
			FileInfo file = uploadGraph.GetFile(localObj.FileID.Value.Value);
			if (file == null)
				return null;

			localObj.Content = file.BinData;
			string fileName = string.Format("{0}_{1}.{2}", localObj.FileID?.Value, localObj.InventoryID?.Value, localObj.Extension?.Value);
			fileName = string.Join("_", fileName.Split(System.IO.Path.GetInvalidFileNameChars()));

			ProductsImageData data = null;
			while (retryCount < 3)//certain files are uploaded in 2-3 attempts
			{
				try
				{
					retryCount++;
					//Upload File to Web dav
					data = webDavClient.Upload<ProductsImageData>(localObj.Content, fileName);
					break;
				}
				catch (Exception)
				{
					if (retryCount == 3)
						throw;
				}

			}
			return data;
		}
		public override List<BCImageEntityBucket> FetchBucketsExport(List<BCSyncStatus> ids, Int32?[] syncIDs)
		{
			BCEntityStats entityStats = GetEntityStats();
			var invIDs = new List<string>();

			List<BCImageEntityBucket> buckets = new List<BCImageEntityBucket>();
			List<BCSyncStatus> parentEntities = PXSelect<BCSyncStatus,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<Where<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>>>>>>
				.Select(this, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.ProductWithVariant).RowCast<BCSyncStatus>().ToList();

			var details = PXSelectJoin<BCSyncDetail,
					InnerJoin<InventoryItem, On<InventoryItem.noteID, Equal<BCSyncDetail.localID>>,
					InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>>,
					   Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncDetail.entityType, Equal<Required<BCSyncDetail.entityType>>
						>>>>.Select(this, BCEntitiesAttribute.Variant);

			// Get Files meta data
			ItemImages response = cbapi.Put<ItemImages>(new ItemImages(), null);
			if (response.Results == null) return buckets;

			if (ids != null && ids.Count > 0 && (Operation.PrepareMode == PrepareMode.None || syncIDs != null))
			{
				var localIds = ids.Select(x => x.LocalID);
				response.Results = response.Results.Where(s => localIds.Contains(s.FileNoteID.Value))?.ToList();
				if (response.Results == null || response.Results?.Count == 0) return buckets;
			}
			List<ItemImageDetails> results = new List<ItemImageDetails>();
			var nonVariant = response.Results.Where(x => x.TemplateItemID?.Value == null)?.ToList();
			if (nonVariant != null) results.AddRange(nonVariant);
			var variant = response.Results.Where(x => x.TemplateItemID?.Value != null)?.GroupBy(x => x.InventoryID?.Value)?.ToDictionary(d => d.Key, d => d.ToList());
			if (variant != null)
			{
				foreach (var item in variant)
				{
					results.Add(item.Value?.FirstOrDefault(x => x.IsDefault?.Value == true) ?? item.Value?.FirstOrDefault());

				}
			}
			foreach (ItemImageDetails impl in results)
			{
				BCSyncStatus parent;
				BCSyncDetail detail = null;
				// sync images for products that are synced
				if (impl.TemplateItemID?.Value != null)
				{
					detail = details.FirstOrDefault(x => x.GetItem<BCSyncDetail>().LocalID == impl.InventoryNoteID.Value);
					parent = parentEntities.FirstOrDefault(p => p.SyncID == detail?.SyncID);
					impl.ExternalVarientID = detail?.ExternID;
				}
				else
				{
					parent = parentEntities.FirstOrDefault(p => p.LocalID.Value == impl.InventoryNoteID?.Value);

				}
				if (parent == null || parent.ExternID == null)
				{
					BCSyncStatus toDelete = BCSyncStatus.LocalIDIndex.Find(this, Operation.ConnectorType, Operation.Binding, Operation.EntityType, impl.FileNoteID?.Value);
					if (toDelete != null) DeleteStatus(toDelete, BCSyncOperationAttribute.NotFound);
					continue;
				}
				impl.SyncTime = new DateTime?[] { impl.LastModifiedDateTime?.Value, impl.InventoryLastModifiedDateTime?.Value }.Where(d => d != null).Select(d => d.Value).Max();
				if (Operation.PrepareMode == PrepareMode.Incremental)
				{
					if (entityStats?.LastIncrementalExportDateTime != null && impl.SyncTime < entityStats.LastIncrementalExportDateTime)
						continue;
				}
				impl.ExternalInventoryID = parent.ExternID;
				MappedProductImage obj = new MappedProductImage(impl, impl.FileNoteID.Value, impl.SyncTime, parent.SyncID);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				if (Operation.PrepareMode == PrepareMode.Incremental && status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force) continue;
				invIDs.Add(impl?.InventoryID?.Value);

				buckets.Add(new BCImageEntityBucket() { Image = obj });

			}
			return buckets;

		}
		#endregion
	}
}
