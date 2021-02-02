using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Core.Model;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Shopify.API.REST
{
    public class InventoryLocationRestDataProvider : RestDataProviderBase,  IParentRestDataProvider<InventoryLocationData>
    {
        protected override string GetListUrl   { get; } = "locations.json";
        protected override string GetSingleUrl { get; } = "locations/{id}.json";
        protected override string GetCountUrl { get; } = "locations/count.json";
		protected override string GetSearchUrl => throw new NotImplementedException();
		protected string GetLevelsUrl { get; } = "locations/{id}/inventory_levels.json";

		public InventoryLocationRestDataProvider(IShopifyRestClient restClient) : base()
		{
            ShopifyRestClient = restClient;
		}

		public InventoryLocationData Create(InventoryLocationData entity) => throw new NotImplementedException();

		public InventoryLocationData Update(InventoryLocationData entity) => throw new NotImplementedException();
		public InventoryLocationData Update(InventoryLocationData entity, string id) => throw new NotImplementedException();

		public bool Delete(InventoryLocationData entity, string id) => throw new NotImplementedException();

		public bool Delete(string id) => throw new NotImplementedException();

		public IEnumerable<InventoryLocationData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			return GetCurrentList<InventoryLocationData, InventoryLocationsResponse>(out previousList, out nextList, filter);
		}

		public IEnumerable<InventoryLocationData> GetAll(IFilter filter = null)
		{
			return GetAll<InventoryLocationData, InventoryLocationsResponse>(filter);
		}

		public InventoryLocationData GetByID(string id)
		{
			var segments = MakeUrlSegments(id);
			var entity = base.GetByID<InventoryLocationData, InventoryLocationResponse>(segments);
			return entity;
		}

		public ItemCount Count()
		{
			return base.GetCount();
		}

		public ItemCount Count(IFilter filter)
		{
			return base.GetCount(filter);
		}

		public List<InventoryLevelData> GetInventoryLevelsByLocation(string locationId)
		{
			var request = BuildRequest(GetLevelsUrl, nameof(GetInventoryLevelsByLocation), MakeUrlSegments(locationId), null);
			return ShopifyRestClient.GetAll<InventoryLevelData, InventoryLevelsResponse>(request).ToList();
		}
	}
}
