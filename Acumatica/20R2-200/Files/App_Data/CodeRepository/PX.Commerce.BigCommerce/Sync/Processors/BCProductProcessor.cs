using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Objects.Substitutes;
using PX.Data;
using PX.Objects.IN.RelatedItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
namespace PX.Commerce.BigCommerce
{
	public abstract class BCProductProcessor<TGraph, TEntityBucket, TPrimaryMapped> : BCProcessorSingleBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		private IChildRestDataProvider<ProductsImageData> productImageDataProvider;
		private IChildRestDataProvider<ProductsVideo> productVideoDataProvider;
		protected ProductRestDataProvider productDataProvider;
		protected TaxDataProvider taxDataProvider;
		protected IChildRestDataProvider<ProductsCustomFieldData> productsCustomFieldDataProvider;
		protected List<ProductsTaxData> taxClasses;
		protected BCRestClient client;

		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			productDataProvider = new ProductRestDataProvider(client);
			productImageDataProvider = new ProductImagesDataProvider(client);
			productVideoDataProvider = new ProductVideoDataProvider(client);
			productsCustomFieldDataProvider = new ProductCustomFieldRestDataProvider(client);
			taxDataProvider = new TaxDataProvider(client);
			taxClasses = taxDataProvider.GetAll();
		}

		public List<ProductData> PullSimilar(string description, string inventoryId, out string uniqueField)
		{
			uniqueField = inventoryId;

			List<ProductData> datas = null;
			if (!string.IsNullOrEmpty(inventoryId))
			{

				datas = productDataProvider.GetAll(new FilterProducts() { SKU = inventoryId })?.ToList();
			}
			if (datas == null || datas.Count == 0)
			{

				uniqueField = description;
				datas = productDataProvider.GetAll(new FilterProducts() { Name = description })?.ToList();
			}

			if (datas == null) return null;
			var id = datas.FirstOrDefault(x => x.Id != null)?.Id;
			if (id != null)
			{
				var statuses = PXSelect<BCSyncStatus,
					Where<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>.Select(this, Operation.ConnectorType, Operation.Binding, id);
				if (statuses != null)
				{
					if ((Operation.EntityType == BCEntitiesAttribute.ProductWithVariant && statuses.Any(x => x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.StockItem || x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.NonStockItem)) ||
						(Operation.EntityType == BCEntitiesAttribute.StockItem && statuses.Any(x => x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.ProductWithVariant || x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.NonStockItem)) ||
						(Operation.EntityType == BCEntitiesAttribute.NonStockItem && statuses.Any(x => x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.StockItem || x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.ProductWithVariant)))
					{
						throw new PXException(BigCommerceMessages.MappedToOtherEntity, uniqueField);
					}

				}
			}

			return datas;
		}
		public virtual void SaveImages(IMappedEntity obj, List<InventoryFileUrls> urls)
		{
			var fileURLs = urls?.Where(x => x.FileType?.Value == BCCaptions.Image);
			if (fileURLs == null) return;
			foreach (var image in fileURLs)
			{
				if (!string.IsNullOrEmpty(image.FileURL?.Value))
				{
					var productImage = new ProductsImageData();
					productImage.ImageUrl = image.FileURL?.Value;
					try
					{
						ProductsImageData response;
						if (obj.Details?.Any(x => x.LocalID == image.NoteID?.Value) == false)
						{
							response = productImageDataProvider.Create(productImage, obj.ExternID);
							obj.AddDetail(BCEntitiesAttribute.ProductImage, image.NoteID.Value, response.Id.ToString());
						}
						else
						{
							var detail = obj.Details.FirstOrDefault(x => x.LocalID == image.NoteID?.Value);
							response = productImageDataProvider.Update(productImage, detail.ExternID, obj.ExternID);
						}
					}
					catch (RestException ex)
					{
						if (ex.ResponceStatusCode == HttpStatusCode.BadRequest.ToString())
							throw new PXException(BigCommerceMessages.InvalidImage);
						throw;
					}
				}
			}
		}
		public virtual void SaveVideos(IMappedEntity obj, List<InventoryFileUrls> urls)
		{
			var fileURLs = urls?.Where(x => x.FileType?.Value == BCCaptions.Video);
			if (fileURLs == null) return;

			//map Videos
			foreach (var video in fileURLs)
			{
				if (!string.IsNullOrEmpty(video.FileURL?.Value) && obj.Details?.Any(x => x.LocalID == video.NoteID?.Value) == false)
				{
					var productVideo = new ProductsVideo();
					try
					{
						productVideo.VideoId = Regex.Match(video.FileURL?.Value, @"^.*(?:(?:youtu\.be\/|v\/|vi\/|u\/\w\/|embed\/)|(?:(?:watch)?\?v(?:i)?=|\&v(?:i)?=))([^#\&\?]*).*", RegexOptions.IgnoreCase).Groups[1].Value;
						ProductsVideo response = productVideoDataProvider.Create(productVideo, obj.ExternID);
						obj.AddDetail(BCEntitiesAttribute.ProductVideo, video.NoteID.Value, response.Id.ToString());
					}
					catch (RestException ex)
					{
						if (ex.ResponceStatusCode == HttpStatusCode.Conflict.ToString())
							throw new PXException(BigCommerceMessages.InvalidVideo);
						throw;
					}
				}
			}

		}

		public virtual void ExportCustomFields(IMappedEntity obj, IList<ProductsCustomField> customFields, ProductData data)
		{
			if (customFields == null || customFields.Count <= 0) return;

			var cFields = new List<ProductsCustomField>(customFields);
			if (obj.ExternID == null)
			{
				foreach (var cdata in cFields.Where(x => !String.IsNullOrEmpty(x.Data.Value)))
				{
					productsCustomFieldDataProvider.Create(cdata.Data, data.Id.ToString());
				}
			}
			else
			{
				var externalcustomFields = productsCustomFieldDataProvider.Get(data.Id.ToString());
				foreach (var cdata in cFields)
				{
					var extID = externalcustomFields.Where(x => x.Name == cdata.Data.Name).FirstOrDefault();
					//Update Custom field if value is specified in local system
					if (extID != null && !String.IsNullOrEmpty(cdata.Data.Value))
					{
						productsCustomFieldDataProvider.Update(cdata.Data, extID.Id.ToString(), data.Id.ToString());
					}
					//Delete Custom field if value is not specified in local system but exists in external
					else if (extID != null && String.IsNullOrEmpty(cdata.Data.Value))
					{
						productsCustomFieldDataProvider.Delete(extID.Id.ToString(), data.Id.ToString());
					}
					else
					{
						if (!String.IsNullOrEmpty(cdata.Data.Value))
						{
							productsCustomFieldDataProvider.Create(cdata.Data, data.Id.ToString());
						}
					}
				}
			}
		}
		public override void AddExternCustomField(TEntityBucket bucket, string cFiledName, object cFieldValue)
		{
			IMappedEntity obj = bucket.Primary;
			ProductData data = (ProductData)obj.Extern;

			var cData = new ProductsCustomField() { Data = new ProductsCustomFieldData() { Id = null, Name = cFiledName, Value = Convert.ToString(cFieldValue) } };
			data.CustomFields.Add(cData);
		}
		public virtual List<int> MapRelatedItems(string inventoryID)
		{
			string[] categoriesAllowed = GetBindingExt<BCBindingExt>().RelatedItems?.Split(',');
			BCBinding binding = GetBinding();
			List<int> ids = new List<int>();
			if (categoriesAllowed != null && categoriesAllowed.Count() > 0 && !String.IsNullOrWhiteSpace(categoriesAllowed[0]))
			{
				PXResultset<PX.Objects.IN.InventoryItem, INRelatedInventory, BCChildrenInventoryItem, BCSyncStatus> relates = PXSelectJoin<PX.Objects.IN.InventoryItem,
					InnerJoin<INRelatedInventory, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<INRelatedInventory.inventoryID>,
						And<INRelatedInventory.isActive, Equal<True>>>,
					InnerJoin<BCChildrenInventoryItem, On<INRelatedInventory.relatedInventoryID, Equal<BCChildrenInventoryItem.inventoryID>>,
					InnerJoin<BCSyncStatus, On<BCSyncStatus.localID, Equal<BCChildrenInventoryItem.noteID>,
						And<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
						And2<Where<BCSyncStatus.entityType, Equal<BCEntitiesAttribute.stockItem>, Or<BCSyncStatus.entityType, Equal<BCEntitiesAttribute.nonStockItem>>>,
						And<BCSyncStatus.syncID, IsNotNull,
						And<BCSyncStatus.externID, IsNotNull>>>>>>>>>,
					Where<PX.Objects.IN.InventoryItem.inventoryCD, Equal<Required<PX.Objects.IN.InventoryItem.inventoryCD>>>>
						.Select<PXResultset<PX.Objects.IN.InventoryItem, INRelatedInventory, BCChildrenInventoryItem, BCSyncStatus>>(this,
						binding.ConnectorType,
						binding.BindingID,
						inventoryID);

				foreach (var rel in relates)
					if (categoriesAllowed.Contains(rel.GetItem<INRelatedInventory>().Relation)
						&& (rel.GetItem<INRelatedInventory>().ExpirationDate == null || rel.GetItem<INRelatedInventory>().ExpirationDate > DateTime.Now))
						ids.Add((int)rel.GetItem<BCSyncStatus>().ExternID.ToInt());
			}
			return ids.Count() > 0 ? ids.ToList() : new List<int>();
		}

		public virtual void UpdateRelatedItems(TEntityBucket bucket, string inventoryID, int updatedID)
		{
			List<int> ids = new List<int>();
			string[] categoriesAllowed = GetBindingExt<BCBindingExt>().RelatedItems?.Split(',');
			BCBinding binding = GetBinding();
			if (categoriesAllowed != null && categoriesAllowed.Count() > 0 && !String.IsNullOrWhiteSpace(categoriesAllowed[0]))
			{
				PXResultset<ChildSyncStatus, BCChildrenInventoryItem, BCChildrenRelatedInventory, BCParentInventoryItem,
					BCSyncStatus, INRelatedInventory, PX.Objects.IN.InventoryItem> relates = PXSelectJoin<ChildSyncStatus,
				InnerJoin<BCChildrenInventoryItem, On<ChildSyncStatus.localID, Equal<BCChildrenInventoryItem.noteID>>,
				InnerJoin<BCChildrenRelatedInventory, On<BCChildrenInventoryItem.inventoryID, Equal<BCChildrenRelatedInventory.relatedInventoryID>,
					And<BCChildrenRelatedInventory.isActive, Equal<True>>>,
				InnerJoin<BCParentInventoryItem, On<BCChildrenRelatedInventory.inventoryID, Equal<BCParentInventoryItem.inventoryID>>,
				InnerJoin <BCSyncStatus, On<
					BCSyncStatus.localID, Equal<BCParentInventoryItem.noteID>,
					And<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
					And2<Where<BCSyncStatus.entityType, Equal<BCEntitiesAttribute.stockItem>,
						Or<BCSyncStatus.entityType, Equal<BCEntitiesAttribute.nonStockItem>>>,
					And<BCSyncStatus.externID, IsNotNull,
					And<BCSyncStatus.syncID, IsNotNull>>>>>>,
				InnerJoin<INRelatedInventory, On<BCParentInventoryItem.inventoryID, Equal<INRelatedInventory.inventoryID>,
					And<INRelatedInventory.isActive, Equal<True>>>,
				InnerJoin<PX.Objects.IN.InventoryItem, On<INRelatedInventory.relatedInventoryID, Equal<PX.Objects.IN.InventoryItem.inventoryID>,
					And<PX.Objects.IN.InventoryItem.inventoryCD, Equal<Required< PX.Objects.IN.InventoryItem .inventoryCD>>>>>>>>>>,
				Where <
					ChildSyncStatus.connectorType, Equal<BCSyncStatus.connectorType>,
					And <ChildSyncStatus.bindingID, Equal<BCSyncStatus.bindingID>,
					And2<Where<ChildSyncStatus.entityType, Equal<BCEntitiesAttribute.stockItem>,
						Or<ChildSyncStatus.entityType, Equal<BCEntitiesAttribute.nonStockItem>>>,
					And<ChildSyncStatus.syncID, IsNotNull>>>>>.Select<PXResultset<ChildSyncStatus, BCChildrenInventoryItem, BCChildrenRelatedInventory, BCParentInventoryItem,
					BCSyncStatus, INRelatedInventory, PX.Objects.IN.InventoryItem>>(this,
					binding.ConnectorType,
					binding.BindingID,
					inventoryID);
				List<RelatedProductsData> relatedProductsData = new List<RelatedProductsData>();
				List<IMappedEntity> relatedMappedProducts = new List<IMappedEntity>();
				foreach (PXResult<ChildSyncStatus, BCChildrenInventoryItem, BCChildrenRelatedInventory, BCParentInventoryItem,
					BCSyncStatus, INRelatedInventory, PX.Objects.IN.InventoryItem> rel in relates)
					if (!String.IsNullOrWhiteSpace(rel.GetItem<BCSyncStatus>().ExternID)
						  && categoriesAllowed.Contains(rel.GetItem<BCChildrenRelatedInventory>().Relation)
						&& (rel.GetItem<BCChildrenRelatedInventory>().ExpirationDate == null || rel.GetItem<BCChildrenRelatedInventory>().ExpirationDate > DateTime.Now)
						&& (rel.GetItem<ChildSyncStatus>().ExternID != null || rel.GetItem<BCChildrenInventoryItem>().InventoryCD.TrimEnd().Equals(inventoryID)))
					{
						string entityType = rel.GetItem<BCSyncStatus>().EntityType;
						IMappedEntity item;
						if (!relatedMappedProducts.Any(i => i.SyncID.Equals(rel.GetItem<BCSyncStatus>().SyncID)))
						{
							if (entityType.Equals(BCEntitiesAttribute.NonStockItem))
								item = new MappedNonStockItem() { SyncID = rel.GetItem<BCSyncStatus>().SyncID }.Set(rel.GetItem<BCSyncStatus>());
							else
								item = new MappedStockItem() { SyncID = rel.GetItem<BCSyncStatus>().SyncID }.Set(rel.GetItem<BCSyncStatus>());
							relatedMappedProducts.Add(item);
							relatedProductsData.Add(new RelatedProductsData()
							{
								Id = item.ExternID.ToInt(),
								RelatedProducts = new List<int>() { !rel.GetItem<BCChildrenInventoryItem>().InventoryCD.TrimEnd().Equals(inventoryID)?
									(int)rel.GetItem<ChildSyncStatus>().ExternID.ToInt() : updatedID },
							});
						}
						else
							relatedProductsData.FirstOrDefault(i => i.Id.Equals(rel.GetItem<BCSyncStatus>().ExternID.ToInt()))?.RelatedProducts.Add(
								!rel.GetItem<BCChildrenInventoryItem>().InventoryCD.TrimEnd().Equals(inventoryID) ?
									(int)rel.GetItem<ChildSyncStatus>().ExternID.ToInt() : updatedID);
					}
				bool retryAttempt = true;
				while (retryAttempt && relatedProductsData.Count() > 0)
				{
					bool attemptedToRemoveFailingEntry = false;
					retryAttempt = false;
					productDataProvider.UpdateAllRelations(relatedProductsData, delegate (ItemProcessCallback<RelatedProductsData> callback)
					{
						IMappedEntity item;
						if (callback.IsSuccess)
						{
							item = relatedMappedProducts.FirstOrDefault(i => i.ExternID?.ToInt() == callback.Result.Id);
							item.ExternTimeStamp = callback.Result.DateModified;
							UpdateStatus(item, BCSyncOperationAttribute.ExternUpdate);
						}
						else
						{
							if (!attemptedToRemoveFailingEntry && callback.Error.ResponceStatusCode.Equals("422"))
							{
								attemptedToRemoveFailingEntry = true;
								string[] messages = callback.Error.Message.Split('\n');
								int failedID;
								string clean = Regex.Replace(messages.First(), "[^0-9]", "");
								if (messages.First().ToLower().Contains("not found") && int.TryParse(clean, out failedID))
								{
									RelatedProductsData failedItem = relatedProductsData.Find(i => i.Id == failedID);
									relatedProductsData.Remove(failedItem);
									Log(failedID, SyncDirection.Export, callback.Error);
									retryAttempt = true;
									return;
								}
							}
							if (attemptedToRemoveFailingEntry && !retryAttempt)
								productDataProvider.UpdateAllRelations(new List<RelatedProductsData>() { relatedProductsData[callback.Index] }, delegate (ItemProcessCallback<RelatedProductsData> retrycallback)
								{
									item = relatedMappedProducts[callback.Index];
									if (retrycallback.IsSuccess)
									{
										item.ExternTimeStamp = retrycallback.Result.DateModified;
										UpdateStatus(item, BCSyncOperationAttribute.ExternUpdate);
									}
									else
									{
										Log(item.SyncID, SyncDirection.Export, callback.Error);
									}
								});
						}
					});
				}
			}
		}
	}
}
