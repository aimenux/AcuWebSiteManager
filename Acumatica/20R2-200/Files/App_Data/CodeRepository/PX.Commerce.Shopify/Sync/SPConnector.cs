using CommonServiceLocator;
using Newtonsoft.Json;
using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Core.Model;
using PX.Commerce.Objects;
using PX.Data;
using PX.Data.PushNotifications;
using PX.Objects.Common;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using Serilog.Context;
using System.Security.Cryptography;
using System.Text;

namespace PX.Commerce.Shopify
{
	#region SPConnectorFactory

	public class SPConnectorFactory : BaseConnectorFactory<SPConnector>, IConnectorFactory
	{
		public override string Type => SPConnector.TYPE;
		public override string Description => SPConnector.NAME;
		public override bool Enabled => FeaturesHelper.ShopifyConnector;


		public SPConnectorFactory(IEnumerable<IProcessorsFactory> processors)
			: base(processors)
		{
		}

		public IConnectorDescriptor GetDescriptor()
		{
			return new SPConnectorDescriptor(_processors.Values.ToList());
		}
	}
	#endregion
	#region SPProcessorsFactory
	public class SPProcessorsFactory : IProcessorsFactory
	{
		public string ConnectorType => SPConnector.TYPE;

		public IEnumerable<Type> GetProcessorTypes()
		{
			yield return typeof(SPCustomerProcessor);
			yield return typeof(SPLocationProcessor);
			//yield return typeof(SPCategoryProcessor);
			yield return typeof(SPStockItemProcessor);
			yield return typeof(SPNonStockItemProcessor);
			yield return typeof(SPTemplateItemProcessor);
			////yield return typeof(BCSalesPriceProcessor);
			////yield return typeof(BCPriceListProcessor);
			yield return typeof(SPImageProcessor);
			yield return typeof(SPAvailabilityProcessor);
			yield return typeof(SPSalesOrderProcessor);
			yield return typeof(SPPaymentProcessor);
			yield return typeof(SPShipmentProcessor);
			yield return typeof(SPRefundsProcessor);
			yield break;
		}
	}
	#endregion

	public class SPConnector : BCConnectorBase<SPConnector>, IConnector
	{
		public static SemaphoreSlim ApiCallsPool = null;
		#region IConnector
		public const string TYPE = "SPC";
		public const string NAME = "Shopify";

		public override string ConnectorType { get => TYPE; }
		public override string ConnectorName { get => NAME; }
		public class spConnectorType : PX.Data.BQL.BqlString.Constant<spConnectorType>
		{
			public spConnectorType() : base(TYPE) { }
		}

        public override void Initialise(List<EntityInfo> entities)
        {
            //Initialize ApiCallsPool here if it doesn't initialize
            if (SPConnector.ApiCallsPool == null)
            {
                ApiCallsPool = new SemaphoreSlim(1, 1);
            }
            base.Initialise(entities);
        }

        public virtual IEnumerable<TInfo> GetExternalInfo<TInfo>(string infoType, int? bindingID)
			where TInfo : class
		{
			if (string.IsNullOrEmpty(infoType) || bindingID == null) return null;
			BCBindingShopify binding = BCBindingShopify.PK.Find(this, bindingID);
			if (binding == null) return null;

			try
			{
				List<TInfo> result = new List<TInfo>();
				if (infoType == BCObjectsConstants.BCPayment)
				{
					OrderRestDataProvider dataProvider = new OrderRestDataProvider(GetRestClient(binding));
					FilterOrders filter = new FilterOrders { FinancialStatus = "authorized,paid", Fields = "payment_gateway_names", Limit = 250 };
					var orderList = dataProvider.GetCurrentList<OrderData, OrdersResponse>(out _, out _, filter);
					List<PaymentMethod> payments = new List<PaymentMethod>();
					payments.Add(new PaymentMethod() { Name = "shopify_payments"});
					if (orderList != null && orderList.Any())
					{
						var paymentList = orderList.SelectMany(x => x.PaymentGatewayNames.Where(n =>n != null && !payments.Any(p => p.Name.Equals(n, StringComparison.OrdinalIgnoreCase))).Distinct().Select(s => new PaymentMethod() { Name = s }));
						payments.AddRange(paymentList);
					}
					result = payments.Cast<TInfo>().ToList();
				}
				else if (infoType == BCObjectsConstants.BCShippingZone)
				{
					var dataList = (new StoreRestDataProvider(GetRestClient(binding))).GetShippingZones();
					foreach (var shippingZone in dataList ?? new List<ShippingZoneData>())
					{
						if(shippingZone != null)
						{
							shippingZone.Enabled = true;
							var methods = new List<IShippingMethod>();

							methods.Add(new ShippingMehtod() { Id = 1, Name = BCObjectsConstants.ShippingMethod_Default, Enabled = true, Type = "" });

							if (shippingZone.PriceBasedShippingRates != null && shippingZone.PriceBasedShippingRates.Count > 0)
							{
								shippingZone.PriceBasedShippingRates.ForEach(p =>
								{
									if (!methods.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
									{
										methods.Add(new ShippingMehtod()
										{
											Id = p.ShippingZoneId,
											Name = p.Name,
											Type = "Shopify",
											Enabled = true
										});
									}
								});
							}

							if (shippingZone.WeightBasedShippingRates != null && shippingZone.WeightBasedShippingRates.Count > 0)
							{
								shippingZone.WeightBasedShippingRates.ForEach(p =>
								{
									if (!methods.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
									{
										methods.Add(new ShippingMehtod()
										{
											Id = p.ShippingZoneId,
											Name = p.Name,
											Type = "Shopify",
											Enabled = true
										});
									}
								});
							}

							if (shippingZone.CarrierShippingRates != null && shippingZone.CarrierShippingRates.Count > 0)
							{
								shippingZone.CarrierShippingRates.ForEach(p =>
								{
									var carrierService = p.GetService();
									if (carrierService != null && carrierService.Count > 0)
									{
										foreach (var service in carrierService)
										{
											if (!methods.Any(x => x.Name.Equals(service, StringComparison.OrdinalIgnoreCase)))
											{
												methods.Add(new ShippingMehtod()
												{
													Id = p.ShippingZoneId,
													Name = service,
													Type = "Carrier",
													Enabled = true
												});
											}
										}
									}
								});
							}

							shippingZone.ShippingMethods = methods;
						}
						result.Add(shippingZone as TInfo);
					}
				}
				else if(infoType == BCObjectsConstants.BCInventoryLocation)
				{
					InventoryLocationRestDataProvider provider = new InventoryLocationRestDataProvider(GetRestClient(binding));
					var locations = provider.GetAll(null);
					result = locations.Cast<TInfo>().ToList();
				}
				return result;
			}
			catch (Exception ex)
			{
				LogError(new BCLogTypeScope(typeof(SPConnector)), ex);
			}

			return null;
		}
		#endregion

		#region Navigation
		public void NavigateExtern(ISyncStatus status)
		{
			if (status?.ExternID == null) return;

			EntityInfo info = GetEntities().FirstOrDefault(e => e.EntityType == status.EntityType);
			BCBindingShopify binding  = BCBindingShopify.PK.Find(this, status.BindingID);
			if (binding == null || string.IsNullOrEmpty(binding.ShopifyApiBaseUrl) || string.IsNullOrEmpty(info.URL) ) return;

			string[] parts = status.ExternID.Split(new char[] { ';' });
			string url = string.Format(info.URL, parts.Length > 2 ? parts.Take(2).ToArray() : parts);
			string redirectUrl = binding.ShopifyApiBaseUrl.TrimEnd('/') + "/" + url;

			throw new PXRedirectToUrlException(redirectUrl, PXBaseRedirectException.WindowMode.New, string.Empty);

		}
		#endregion

		#region Process
		public virtual SyncInfo[] Process(ConnectorOperation operation, Int32?[] syncIDs = null)
		{
			LogInfo(operation.LogScope(), BCMessages.LogConnectorStarted, NAME);

			EntityInfo info = GetEntities().FirstOrDefault(e => e.EntityType == operation.EntityType);
			using (IProcessor graph = (IProcessor)PXGraph.CreateInstance(info.ProcessorType))
			{
				graph.Initialise(this, operation);
				return graph.Process(syncIDs);
			}
		}

		public DateTime GetSyncTime(ConnectorOperation operation)
		{
			BCBindingShopify binding = BCBindingShopify.PK.Find(this, operation.Binding);

			//Shopify Server Response Time
			StoreData store = new StoreRestDataProvider(GetRestClient(binding)).Get();
			DateTime syncTime = store.ResponseTime;
			syncTime = syncTime.ToUniversalTime();

			//Acumatica Time
			PXDatabase.SelectDate(out DateTime dtLocal, out DateTime dtUtc);
			if(syncTime > dtUtc) 
				syncTime = PX.Common.PXTimeZoneInfo.ConvertTimeFromUtc(dtUtc, PX.Common.LocaleInfo.GetTimeZone());
			else
				syncTime = PX.Common.PXTimeZoneInfo.ConvertTimeFromUtc(syncTime, PX.Common.LocaleInfo.GetTimeZone());

			return syncTime;
		}
		#endregion

		#region Notifications
		public override void StartWebHook(String baseUrl, BCWebHook hook)
		{
			BCBinding store = BCBinding.PK.Find(this, hook.ConnectorType, hook.BindingID);
			BCBindingShopify storeShopify = BCBindingShopify.PK.Find(this, hook.BindingID);

			WebHookRestDataProvider restClient = new WebHookRestDataProvider(GetRestClient(storeShopify));

			//URL and HASH
			string url = new Uri(baseUrl, UriKind.RelativeOrAbsolute).ToString();
			if (url.EndsWith("/")) url = url.TrimEnd('/');
			url += hook.Destination;
			if(url.EndsWith("/")) url = url.TrimEnd('/');
			url += $"?type={hook.ConnectorType}&company={PXAccess.GetCompanyName()??""}&binding={store.BindingID}";

			//Searching for the existing hook
			if (hook.HookRef != null)
			{
				WebHookData data = restClient.GetByID(hook.HookRef.Value.ToString());
				if (data != null)
				{
					if (data.Address != url || data.Topic != hook.Scope)
						restClient.Delete(hook.HookRef.Value.ToString());
					else if (hook.IsActive == false)
					{
						hook.IsActive = true;
						hook.ValidationHash = storeShopify.StoreSharedSecret;
						Hooks.Update(hook);
						Actions.PressSave();
						return;
					}
					else
						return;
				}
			}
			else
			{
				foreach (WebHookData data in restClient.GetAll())
				{
					if (data.Topic == hook.Scope && data.Address == url)
					{
						hook.IsActive = true;
						hook.HookRef = data.Id;
						hook.ValidationHash = storeShopify.StoreSharedSecret;
						Hooks.Update(hook);
						Actions.PressSave();
						return;
					}
				}
			}

			//Create a new Hook
			WebHookData webHook = new WebHookData();
			webHook.Topic = hook.Scope;
			webHook.Address = url;
			webHook.Format = "json";
			webHook.Fields = new string[] { "id", "created_at", "updated_at"};
			webHook = restClient.Create(webHook);

			//Saving
			hook.IsActive = true;
			hook.HookRef = webHook.Id;
			hook.ValidationHash = storeShopify.StoreSharedSecret;

			Hooks.Update(hook);
			Actions.PressSave();
		}
		public override void StopWebHook(String baseUrl, BCWebHook hook)
		{
			BCBindingShopify storeShopify = BCBindingShopify.PK.Find(this, hook.BindingID);

			WebHookRestDataProvider restClient = new WebHookRestDataProvider(GetRestClient(storeShopify));

			if (hook.HookRef != null)
			{
				WebHookData data = restClient.GetByID(hook.HookRef.ToString());
				if (data != null)
				{
					restClient.Delete(hook.HookRef.Value.ToString());
				}
			}
			else if(baseUrl != null)
			{
				string url = new Uri(baseUrl, UriKind.RelativeOrAbsolute).ToString();
				if (url.EndsWith("/")) url = url.TrimEnd('/');
				url += hook.Destination;
				if (url.EndsWith("/")) url = url.TrimEnd('/');
				url += $"?type={hook.ConnectorType}&company={PXAccess.GetCompanyName() ?? ""}&binding={storeShopify.BindingID}";

				foreach (WebHookData data in restClient.GetAll())
				{
					if (data.Topic == hook.Scope && data.Address == url)
					{
						restClient.Delete(data.Id.Value.ToString());
					}
				}
			}

			//Saving
			hook.IsActive = false;
			hook.HookRef = null;
			hook.ValidationHash = null;

			Hooks.Update(hook);
			Actions.PressSave();
		}

		public virtual void ProcessHook(IEnumerable<BCExternQueueMessage> messages)
		{
			Dictionary<RecordKey, RecordValue<String>> toProcess = new Dictionary<RecordKey, RecordValue<String>>();
			foreach (BCExternQueueMessage message in messages)
			{
				WebHookMessage messageData = JsonConvert.DeserializeObject<WebHookMessage>(message.Json);
				string scope = message.AdditionalInfo["X-Shopify-Topic"]?.ToString();
				string validation = message.AdditionalInfo["X-Shopify-Hmac-Sha256"]?.ToString();
				string apiVersion = message.AdditionalInfo["X-Shopify-API-Version"]?.ToString();
				string storehash = message.AdditionalInfo["X-Shopify-Shop-Domain"]?.ToString();
				string bindingId = message.AdditionalInfo["bindingId"]?.ToString();

				DateTime? created = message.TimeStamp.ToDate();

				foreach (BCWebHook hook in PXSelect<BCWebHook,
					Where<BCWebHook.connectorType, Equal<SPConnector.spConnectorType>,
						And<BCWebHook.bindingID, Equal<Required<BCWebHook.bindingID>>,
						And<BCWebHook.scope, Equal<Required<BCWebHook.scope>>>>>>.Select(this, bindingId, scope))
				{
					HMACSHA256 code = new HMACSHA256(Encoding.UTF8.GetBytes(hook.ValidationHash));
					string hash = Convert.ToBase64String(code.ComputeHash(Encoding.UTF8.GetBytes(message.Json)));
					if (hash != validation)
					{
						LogError(new BCLogTypeScope(typeof(SPConnector)), new PXException(BCMessages.WrongValidationHash, storehash ?? "", scope));
						continue;
					}

					foreach (EntityInfo info in this.GetEntities().Where(e => e.ExternRealtime.Supported && e.ExternRealtime.WebHookType != null && e.ExternRealtime.WebHooks.Contains(scope)))
					{
						BCBinding binding = BCBinding.PK.Find(this, TYPE, hook.BindingID.Value);
						BCEntity entity = BCEntity.PK.Find(this, TYPE, hook.BindingID.Value, info.EntityType);

						if (binding == null || !(binding.IsActive ?? false) || entity == null || !(entity.IsActive ?? false) 
							|| entity?.ImportRealTimeStatus != BCRealtimeStatusAttribute.Run || entity.Direction == BCSyncDirectionAttribute.Export)
							continue;

						if (messageData == null || messageData.Id == null) continue;

						toProcess[new RecordKey(entity.ConnectorType, entity.BindingID, entity.EntityType, messageData.Id.ToString())] 
							= new RecordValue<String>((entity.RealTimeMode == BCSyncModeAttribute.PrepareAndProcess), (DateTime)created, message.Json);
					}
				}
			}

			Dictionary<Int32, ConnectorOperation> toSync = new Dictionary<int, ConnectorOperation>();
			foreach(KeyValuePair<RecordKey, RecordValue<String>> pair in toProcess)
			{
				//Trigger Provider
				ConnectorOperation operation = new ConnectorOperation();
				operation.ConnectorType = pair.Key.ConnectorType;
				operation.Binding = pair.Key.BindingID.Value;
				operation.EntityType = pair.Key.EntityType;
				operation.PrepareMode = PrepareMode.None;
				operation.SyncMethod = SyncMode.Changed;

				Int32 ? syncID = null;
				EntityInfo info = this.GetEntities().FirstOrDefault(e => e.EntityType == pair.Key.EntityType);

				//Performance optimization - skip push if no value for that
				BCSyncStatus status = null;
				if (pair.Value.Timestamp != null)
				{
					status = BCSyncStatus.ExternIDIndex.Find(this, operation.ConnectorType, operation.Binding, operation.EntityType, pair.Key.RecordID);
					if (status != null && (status.Deleted == true || status.LastOperation == BCSyncOperationAttribute.Skipped
						|| (status.ExternTS != null && pair.Value.Timestamp <= status.ExternTS)))
						continue;
				}

				if (status == null || status.PendingSync == null || status.PendingSync != true)
				{
					using (IProcessor graph = (IProcessor)PXGraph.CreateInstance(info.ProcessorType))
					{
						syncID = graph.ProcessHook(this, operation, pair.Key.RecordID, pair.Value.Timestamp, pair.Value.ExternalInfo, status);
					}
				}
				else if (status.SyncInProcess == false) syncID = status.SyncID;

				if (syncID != null && pair.Value.AutoSync) toSync[syncID.Value] = operation;
			}
			if(toSync.Count > 0)
			{
				PXLongOperation.StartOperation(this, delegate ()
				{
					foreach (KeyValuePair<Int32, ConnectorOperation> pair in toSync)
					{
						try
						{
							IConnector connector = ConnectorHelper.GetConnector(pair.Value.ConnectorType);
							connector.Process(pair.Value, new Int32?[] { pair.Key } );
						}
						catch (Exception ex)
						{
							LogError(pair.Value.LogScope(pair.Key), ex);
						}
					}
				});
			}
			
		}
		#endregion

		#region Public Static
		public static SPRestClient GetRestClient(BCBindingShopify binding)
		{
			return GetRestClient(binding.ShopifyApiBaseUrl, binding.ShopifyApiKey, binding.ShopifyApiPassword, binding.StoreSharedSecret, binding.ApiCallLimit);
		}
		public static SPRestClient GetRestClient(String url, String clientID, String token, String sharedSecret, int? apiCallLimit)
		{
			RestOptions options = new RestOptions
			{
				BaseUri = url,
				XAuthClient = clientID,
				XAuthTocken = token,
				SharedSecret = sharedSecret,
				ApiCallLimit = apiCallLimit.Value
			};
			JsonSerializer serializer = new JsonSerializer
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                Formatting = Formatting.Indented,
                ContractResolver = new Core.REST.GetOnlyContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
			RestJsonSerializer restSerializer = new RestJsonSerializer(serializer);
			SPRestClient client = new SPRestClient(restSerializer, restSerializer, options,
				ServiceLocator.Current.GetInstance<Serilog.ILogger>()
				);

			return client;
		}
		#endregion
	}

	#region SPConnectorDescriptor
	public class SPConnectorDescriptor : IConnectorDescriptor
	{
		protected IList<EntityInfo> _entities;

		public SPConnectorDescriptor(IList<EntityInfo> entities)
		{
			_entities = entities;
		}

		public virtual Guid? GenerateExternID(BCExternNotification message)
		{
			string scope = message.AdditionalInfo["X-Shopify-Topic"]?.ToString();
			string validation = message.AdditionalInfo["X-Shopify-Hmac-Sha256"]?.ToString();
			string apiVersion = message.AdditionalInfo["X-Shopify-API-Version"]?.ToString();
			string storehash = message.AdditionalInfo["X-Shopify-Shop-Domain"]?.ToString();
			string bindingId = message.AdditionalInfo["bindingId"]?.ToString();

			EntityInfo info = _entities.FirstOrDefault(e => e.ExternRealtime.Supported && e.ExternRealtime.WebHookType != null && e.ExternRealtime.WebHooks.Contains(scope));
			WebHookMessage messageData = JsonConvert.DeserializeObject< WebHookMessage>(message.Json);
			String id = messageData.Id?.ToString();

			if (messageData == null || string.IsNullOrEmpty(id)) return null;

			Byte[] bytes = new Byte[16];
			BitConverter.GetBytes(SPConnector.TYPE.GetHashCode()).CopyTo(bytes, 0); //Connector
			BitConverter.GetBytes(info.EntityType.GetHashCode()).CopyTo(bytes, 4); //EntityType
			BitConverter.GetBytes(bindingId.GetHashCode()).CopyTo(bytes, 8); //Store
			BitConverter.GetBytes(id.GetHashCode()).CopyTo(bytes, 12); //ID

			return new Guid(bytes);
		}
		public virtual Guid? GenerateLocalID(BCLocalNotification message)
		{
            Guid? noteId = message.Fields.First(v => v.Key.EndsWith("NoteID", StringComparison.InvariantCultureIgnoreCase) && v.Value != null).Value.ToGuid();
            Byte[] bytes = new Byte[16];
            BitConverter.GetBytes(SPConnector.TYPE.GetHashCode()).CopyTo(bytes, 0); //Connector
            BitConverter.GetBytes(message.Entity.GetHashCode()).CopyTo(bytes, 4); //EntityType
            BitConverter.GetBytes(message.Binding.GetHashCode()).CopyTo(bytes, 8); //Store
            BitConverter.GetBytes(noteId.GetHashCode()).CopyTo(bytes, 12); //ID
            return new Guid(bytes);
		}

		public List<Tuple<String, String, String>> GetExternalFields(String type, Int32? binding, String entity)
		{
			List<Tuple<String, String, String>> fieldsList = new List<Tuple<string, string, string>>();
			if (entity != BCEntitiesAttribute.ProductAvailability || entity != BCEntitiesAttribute.Shipment) return fieldsList;

			InventoryLocationRestDataProvider provider = new InventoryLocationRestDataProvider(GetSPRestClient(binding));
			var fields = provider.GetAll(null);
			if (fields == null || fields.Count() <= 0) return fieldsList;
			foreach (var item in fields.Where(x => x.Active == true))
			{
				fieldsList.Add(new Tuple<string, string, string>(entity, item.Id?.ToString(), item.Name));
			}
			return fieldsList;
		}

		protected SPRestClient GetSPRestClient(Int32? binding)
		{
			String url = string.Empty;
			String client = string.Empty;
			String token = string.Empty;
			String sharedSecret = string.Empty;
			int? apiLimit = ShopifyCaptions.ApiCallLimitDefault;

			PXGraph graph = PXGraph.CreateInstance<PXGraph>();
			foreach (BCBindingShopify bcb in PXSelectReadonly<BCBindingShopify, 
				Where<BCBinding.bindingID, Equal<Required<BCBindingShopify.bindingID>>>>.Select(graph, binding))
			{
				url = bcb.ShopifyApiBaseUrl;
				client = bcb.ShopifyApiKey;
				token = bcb.ShopifyApiPassword;
				sharedSecret = bcb.StoreSharedSecret;
				apiLimit = bcb.ApiCallLimit;
			}

			return SPConnector.GetRestClient(url, client, token, sharedSecret, apiLimit);
		}
	}
	#endregion
}
