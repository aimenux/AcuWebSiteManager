using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Core.Model;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
    public class InventoryLevelRestDataProvider : RestDataProviderBase,  IParentRestDataProvider<InventoryLevelData>
    {
        protected override string GetListUrl   { get; } = "inventory_levels.json";
        protected override string GetSingleUrl => throw new NotImplementedException();
		protected override string GetCountUrl => throw new NotImplementedException();
		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetDeleteUrl { get; } = "inventory_levels.json?inventory_item_id={0}&location_id={1}";
		private string GetPostSetUrl { get; } = "inventory_levels/set.json";
		private string GetPostAdjustUrl { get; } = "inventory_levels/adjust.json";
		private string GetPostConnectUrl { get; } = "inventory_levels/connect.json";

		public InventoryLevelRestDataProvider(IShopifyRestClient restClient) : base()
		{
            ShopifyRestClient = restClient;
		}

		public InventoryLevelData Create(InventoryLevelData entity) => throw new NotImplementedException();

		public InventoryLevelData Update(InventoryLevelData entity) => throw new NotImplementedException();
		public InventoryLevelData Update(InventoryLevelData entity, string id) => throw new NotImplementedException();

		public bool Delete(InventoryLevelData entity, string id) => throw new NotImplementedException();

		public bool Delete(string id) => throw new NotImplementedException();

		public bool Delete(string inventoryItemId, string inventoryLocationId)
		{
			var request = BuildRequest(string.Format(GetDeleteUrl, inventoryItemId, inventoryLocationId), nameof(Delete), null, null);
			return ShopifyRestClient.Delete(request);
		}

		public IEnumerable<InventoryLevelData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			if (filter == null) throw new Exception("You must include inventory_item_ids, location_ids, or both as filter parameters");
			return GetCurrentList<InventoryLevelData, InventoryLevelsResponse>(out previousList, out nextList, filter);
		}

		public IEnumerable<InventoryLevelData> GetAll(IFilter filter = null)
		{
			if (filter == null) throw new Exception("You must include inventory_item_ids, location_ids, or both as filter parameters");
			return GetAll<InventoryLevelData, InventoryLevelsResponse>(filter);
		}

		public InventoryLevelData GetByID(string id) => throw new NotImplementedException();

		public ItemCount Count() => throw new NotImplementedException();

		public ItemCount Count(IFilter filter) => throw new NotImplementedException();

		public InventoryLevelData AdjustInventory(InventoryLevelData entity)
		{
			ShopifyRestClient.Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
				.ForContext("Object", entity)
				.Verbose("{CommerceCaption}: adjusting {EntityType} entry", BCCaptions.CommerceLogCaption, entity.GetType().ToString());
			var request = BuildRequest(GetPostAdjustUrl, nameof(AdjustInventory), null, null);
			return ShopifyRestClient.Post<InventoryLevelData, InventoryLevelResponse>(request, entity, false);
		}

		public InventoryLevelData SetInventory(InventoryLevelData entity)
		{
			ShopifyRestClient.Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
				.ForContext("Object", entity)
				.Verbose("{CommerceCaption}: setting {EntityType} entry", BCCaptions.CommerceLogCaption, entity.GetType().ToString());
			var request = BuildRequest(GetPostSetUrl, nameof(SetInventory), null, null);
			return ShopifyRestClient.Post<InventoryLevelData, InventoryLevelResponse>(request, entity, false);
		}

		public InventoryLevelData ConnectInventory(InventoryLevelData entity)
		{
			ShopifyRestClient.Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
				.ForContext("Object", entity)
				.Verbose("{CommerceCaption}: connecting {EntityType} entry", BCCaptions.CommerceLogCaption, entity.GetType().ToString());
			var request = BuildRequest(GetPostConnectUrl, nameof(ConnectInventory), null, null);
			return ShopifyRestClient.Post<InventoryLevelData, InventoryLevelResponse>(request, entity, false);
		}
	}
}
