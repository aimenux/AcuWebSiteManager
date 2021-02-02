using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Objects.Common;
using PX.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Api.ContractBased.Models;

namespace PX.Commerce.Shopify
{
	public class SPSalesOrderBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary { get => Order; }
		public IMappedEntity[] Entities => new IMappedEntity[] { Order };

		public override IMappedEntity[] PreProcessors { get => new IMappedEntity[] { Customer }; }
		public override IMappedEntity[] PostProcessors { get => Enumerable.Empty<IMappedEntity>().Concat(Payments).Concat(Shipments).ToArray(); }

		public MappedOrder Order;
		public MappedCustomer Customer;
		public MappedLocation Location;
		public List<MappedPayment> Payments = new List<MappedPayment>();
		public List<MappedShipment> Shipments = new List<MappedShipment>();
	}

	public class SPSalesOrderRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			#region Orders
			return base.Restrict<MappedOrder>(mapped, delegate (MappedOrder obj)
			{
				Boolean syncCompleted = false;
				if (!syncCompleted && obj.Local != null && obj.Local.Status != null)
				{
					if (((obj.Local.Status.Value == PX.Objects.SO.Messages.Hold && obj.IsNew)
						|| obj.Local.Status.Value == PX.Objects.SO.Messages.Completed
						|| obj.Local.Status.Value == PX.Objects.SO.Messages.Cancelled))
					{
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedStatusNotSupported, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString(), obj.Local.Status.Value));
					}
				}
				if (obj.Local?.OrderType != null && obj.Local?.OrderType?.Value != processor.GetBindingExt<BCBindingExt>().OrderType)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedTypeNotSupported, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString(), obj.Local?.OrderType));
				}

				//skip order that has only gift certificate in order line
				BCBindingExt bindingExt = processor.GetBindingExt<BCBindingExt>();
				var guestID = PX.Objects.AR.Customer.PK.Find((PXGraph)processor, bindingExt.GuestCustomerID)?.AcctCD;
				if (!String.IsNullOrEmpty(obj.Local?.CustomerID?.Value.Trim()) && guestID.Trim().Equals(obj.Local?.CustomerID?.Value.Trim()))
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedGuestOrder, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString()));
				}

				if (!String.IsNullOrEmpty(obj.Local?.ExternalRef?.Value?.Trim()))
				{
					return new FilterResult(FilterStatus.Filtered,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedWithExternalRef, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString()));
				}

				return null;
			});
			#endregion
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped)
		{
			#region Orders
			return base.Restrict<MappedOrder>(mapped, delegate (MappedOrder obj)
			{
				Boolean syncCompleted = false;
				if (!syncCompleted && obj.Extern != null)
				{
					if (obj.IsNew && obj.Extern.CancelledAt != null)
					{
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedExtStatusNotSupported, $"{obj.Extern.Name}({obj.Extern.Id})", "Cancelled"));
					}
				}

				if (!syncCompleted && obj.Extern != null)
				{
					if (obj.IsNew && obj.Extern.ClosedAt != null && !string.Equals(obj.Extern.SourceName, "pos", StringComparison.OrdinalIgnoreCase))
					{
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedExtStatusNotSupported, $"{obj.Extern.Name}({obj.Extern.Id})", "Archived"));
					}
				}

				return null;
			});
			#endregion
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Order, BCCaptions.Order,
		IsInternal = false,
		Direction = SyncDirection.Import,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.SO.SOOrderEntry),
		ExternTypes = new Type[] { typeof(OrderData) },
		LocalTypes = new Type[] { typeof(SalesOrder) },
		DetailTypes = new String[] { BCEntitiesAttribute.OrderLine, BCCaptions.OrderLine, BCEntitiesAttribute.OrderAddress, BCCaptions.OrderAddress },
		AcumaticaPrimaryType = typeof(PX.Objects.SO.SOOrder),
		AcumaticaPrimarySelect = typeof(Search2<PX.Objects.SO.SOOrder.orderNbr,
			InnerJoin<BCBindingExt, On<BCBindingExt.orderType, Equal<SOOrder.orderType>>,
			InnerJoin<BCBinding, On<BCBindingExt.bindingID, Equal<BCBinding.bindingID>>>>,
			Where<BCBinding.connectorType, Equal<Current<BCSyncStatusEdit.connectorType>>,
				And<BCBinding.bindingID, Equal<Current<BCSyncStatusEdit.bindingID>>>>>),
		URL = "orders/{0}",
		Requires = new string[] { BCEntitiesAttribute.Customer },
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.NonStockItem + "." + BCEntitiesAttribute.ProductWithVariant }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = true,
		WebHookType = typeof(WebHookMessage),
		WebHooks = new String[]
		{
			"orders/cancelled",
			"orders/paid",
			"orders/updated"
		})]
	public class SPSalesOrderProcessor : SPOrderBaseProcessor<SPSalesOrderProcessor, SPSalesOrderBucket, MappedOrder>, IProcessor
	{
		public SPPaymentProcessor paymentProcessor = PXGraph.CreateInstance<SPPaymentProcessor>();

		protected OrderRestDataProvider orderDataProvider;
		protected StoreRestDataProvider storeDataProvider;
		protected List<ShippingZoneData> storeShippingZones;
		protected BCBindingShopify currentShopifySettings;
		protected List<BCShippingMappings> shippingMappings;
		protected List<long> skipOrderItems;
		public PXSelect<State, Where<State.name, Equal<Required<State.name>>, Or<State.stateID, Equal<Required<State.stateID>>>>> states;

		#region Initialization
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			currentShopifySettings = GetBindingExt<BCBindingShopify>();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			orderDataProvider = new OrderRestDataProvider(client);
			storeDataProvider = new StoreRestDataProvider(client);
			storeShippingZones = storeDataProvider.GetShippingZones().ToList();
			skipOrderItems = new List<long>();
			shippingMappings = PXSelectReadonly<BCShippingMappings,
				Where<BCShippingMappings.bindingID, Equal<Required<BCShippingMappings.bindingID>>>>.Select(this, Operation.Binding).Select(x => x.GetItem<BCShippingMappings>()).ToList();

			paymentProcessor.Initialise(iconnector, operation.Clone().With(_ => { _.EntityType = BCEntitiesAttribute.Payment; return _; }));
		}
		#endregion

		#region Common
		public override MappedOrder PullEntity(Guid? localID, Dictionary<String, Object> fields)
		{
			SalesOrder impl = cbapi.GetByID<SalesOrder>(localID);
			if (impl == null) return null;

			MappedOrder obj = new MappedOrder(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override MappedOrder PullEntity(String externID, String jsonObject)
		{
			OrderData data = orderDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedOrder obj = new MappedOrder(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));

			return obj;
		}

		public override IEnumerable<MappedOrder> PullSimilar(IExternEntity entity, out string uniqueField)
		{
			uniqueField = ((OrderData)entity)?.Name;
			if (string.IsNullOrEmpty(uniqueField))
				return null;
			uniqueField = APIHelper.ReferenceMake(uniqueField, currentBinding.BindingName);

			SalesOrder[] impls = cbapi.GetAll<SalesOrder>(new SalesOrder() { OrderType = currentBindingExt.OrderType.SearchField(), ExternalRef = uniqueField.SearchField() },
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>());
			if (impls == null) return null;

			List<MappedOrder> result = new List<MappedOrder>();
			foreach (SalesOrder impl in impls)
			{
				SalesOrder data = cbapi.GetByID<SalesOrder>(impl.SyncID);
				if (data != null)
				{
					result.Add(new MappedOrder(data, data.SyncID, data.SyncTime));
				}
			}
			return result;
		}
		public override IEnumerable<MappedOrder> PullSimilar(ILocalEntity entity, out string uniqueField)
		{
			uniqueField = ((SalesOrder)entity)?.ExternalRef?.Value;
			if (string.IsNullOrEmpty(uniqueField))
				return null;
			uniqueField = APIHelper.ReferenceParse(uniqueField, currentBinding.BindingName);

			IEnumerable<OrderData> similarOrders = orderDataProvider.GetAll(new FilterOrders() { Name = uniqueField });
			if (similarOrders == null || similarOrders.Count() == 0) return null;
			OrderData data = similarOrders.First();
			List<MappedOrder> result = new List<MappedOrder>();
			result.Add(new MappedOrder(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false)));
			return result;
		}

		public override Boolean ControlModification(IMappedEntity mapped, BCSyncStatus status, string operation)
		{
			if (mapped is MappedOrder)
			{
				MappedOrder order = mapped as MappedOrder;
				if (operation == BCSyncOperationAttribute.ExternChanged && !order.IsNew && order?.Extern != null && status?.PendingSync == false)
				{
					//We should prevent order from sync if it is updated by shipment
					if (order.Extern.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled || order.Extern.FulfillmentStatus == OrderFulfillmentStatus.Partial)
					{
						DateTime? orderdate = order.Extern.DateModifiedAt.ToDate(false);
						DateTime? shipmentDate = order.Extern.Fulfillments?.Max(x => x.DateModifiedAt)?.ToDate(false);

						if (orderdate != null && shipmentDate != null && Math.Abs((orderdate - shipmentDate).Value.TotalSeconds) < 5) //Modification withing 5 sec
							return false;
					}
				}
			}

			return base.ControlModification(mapped, status, operation);
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterOrders filter = new FilterOrders { Status = OrderStatus.Any, Fields = "id,name,source_name,financial_status,updated_at,created_at,cancelled_at,closed_at" };
			if (minDateTime != null) filter.UpdatedAtMin = minDateTime.Value.ToLocalTime().AddSeconds(-GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0);
			if (maxDateTime != null) filter.UpdatedAtMax = maxDateTime.Value.ToLocalTime();

			IEnumerable<OrderData> datas = orderDataProvider.GetAll(filter);

			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			foreach (OrderData data in datas)
			{
				IMappedEntity obj = new MappedOrder(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));

				mappedList.Add(obj);
				countNum++;
				if (countNum % BatchFetchCount == 0)
				{
					ProcessMappedListForImport(ref mappedList);
				}
			}
			if (mappedList.Any())
			{
				ProcessMappedListForImport(ref mappedList);
			}
		}
		public override EntityStatus GetBucketForImport(SPSalesOrderBucket bucket, BCSyncStatus syncstatus)
		{
			OrderData data = orderDataProvider.GetByID(syncstatus.ExternID, true, true, true, true);
			if (data == null) return EntityStatus.None;

			MappedOrder obj = bucket.Order = bucket.Order.Set(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

            if (status != EntityStatus.Pending && status != EntityStatus.Syncronized)
                return status;

			if (data.Customer != null && data.Customer.Id > 0 && (!string.IsNullOrEmpty(data.Customer.Email) || !string.IsNullOrEmpty(data.Customer.Phone)))
			{
				MappedCustomer customerObj = bucket.Customer = bucket.Customer.Set(data.Customer, data.Customer.Id?.ToString(), data.Customer.DateModifiedAt.ToDate(false));
				EntityStatus customerStatus = EnsureStatus(customerObj);

				if (data.ShippingAddress != null)
				{
					//Find proper location by all fields.
					CustomerAddressData address = data.Customer.Addresses?.FirstOrDefault(x => String.Equals(x.City ?? string.Empty, data.ShippingAddress.City ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.Company ?? string.Empty, data.ShippingAddress.Company ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.CountryCode ?? string.Empty, data.ShippingAddress.CountryCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.FirstName ?? string.Empty, data.ShippingAddress.FirstName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.LastName ?? string.Empty, data.ShippingAddress.LastName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.Phone ?? string.Empty, data.ShippingAddress.Phone ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.ProvinceCode ?? string.Empty, data.ShippingAddress.ProvinceCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.Address1 ?? string.Empty, data.ShippingAddress.Address1 ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.Address2 ?? string.Empty, data.ShippingAddress.Address2 ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
					&& String.Equals(x.PostalCode ?? string.Empty, data.ShippingAddress.PostalCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase));

					if (address != null)
					{
						bucket.Location = bucket.Location.Set(address, new Object[] { address.CustomerId, address.Id }.KeyCombine(), address.CalculateHash()).With(_ => { _.ParentID = customerObj.SyncID; return _; });
					}
					else
						LogWarning(Operation.LogScope(syncstatus), BCMessages.LogOrderLocationUnidentified, $"{data.Name}|{data.Id}");
				}
			}

			if (GetEntity(BCEntitiesAttribute.Payment)?.IsActive == true && data.Transactions?.Count > 0)
			{
				foreach (OrderTransaction tranData in data.Transactions)
				{
					BCSyncStatus paymentSyncStatus = BCSyncStatus.ExternIDIndex.Find(this, Operation.ConnectorType, Operation.Binding, BCEntitiesAttribute.Payment, new Object[] { data.Id, tranData.Id }.KeyCombine());
					if (paymentSyncStatus != null)
					{
						OrderTransaction lastPayment = data.Transactions.LastOrDefault(
							x => x.ParentId == tranData.Id && x.Status != TransactionStatus.Error && x.Status != TransactionStatus.Failure);
						if (lastPayment != null) //Evaluate last transaction event
						{
							tranData.Kind = lastPayment.Kind;
							tranData.Status = lastPayment.Status;
							tranData.DateModifiedAt = lastPayment.DateCreatedAt;
						}
						else if (paymentSyncStatus.LocalID != null && paymentSyncStatus.PendingSync != true)
							continue;
					}
					MappedPayment paymentObj = new MappedPayment(tranData, new Object[] { data.Id, tranData.Id }.KeyCombine(), tranData.DateModifiedAt.ToDate(false), tranData.CalculateHash()).With(_ => { _.ParentID = obj.SyncID; return _; });
					EntityStatus paymentStatus = EnsureStatus(paymentObj, SyncDirection.Import);

					if (paymentStatus == EntityStatus.Pending)
					{
						bucket.Payments.Add(paymentObj);
					}
				}
			}

			return status;
		}

		public override void MapBucketImport(SPSalesOrderBucket bucket, IMappedEntity existing)
		{
			MappedOrder obj = bucket.Order;

			OrderData data = obj.Extern;
			SalesOrder impl = obj.Local = new SalesOrder();
			SalesOrder presented = existing?.Local as SalesOrder;
			// we can update only open orders
			if (presented != null && presented.Status?.Value != "Open" && presented.Status?.Value != "On Hold")
			{
				throw new PXException(BCMessages.OrderStatusDoesNotAllowModification, presented.OrderNbr?.Value);
			}
			bool cancelledOrder = obj.Extern?.CancelledAt != null ? true : false;
			if (presented != null && cancelledOrder == true) return;
			impl.Custom = GetCustomFieldsForImport();

			#region SalesOrder
			impl.OrderType = currentBindingExt.OrderType.ValueField();
			var date = data.DateCreatedAt.ToDate(false, PXTimeZoneInfo.FindSystemTimeZoneById(currentBindingExt.OrderTimeZone));
			if (date.HasValue)
				impl.Date = (new DateTime(date.Value.Date.Ticks)).ValueField();
			impl.RequestedOn = impl.Date;
			var desc = $"{currentBinding.BindingName} | {data.Name} | {data.FinancialStatus.Value.ToString()}";
			desc = data.OrderRisks?.Count > 0 ? (desc + $" | Risks:{string.Join(";", data.OrderRisks.Select(x => x.Recommendation.ToString()).ToArray())}") : desc;
			impl.Description = desc.ValueField();
			impl.CurrencyID = data.Currency.ValueField();
			//impl.CurrencyRate = data.CurrencyExchangeRate.ValueField();
			impl.ExternalRef = APIHelper.ReferenceMake(data.Name, currentBinding.BindingName).ValueField();
			impl.Note = data.Note;
			if (data.OrderRisks?.Count > 0)
			{
				data.OrderRisks.ForEach(x =>
				{
					impl.Note += $"\nOrder Risk Recommendation : {x.Recommendation.ToString()}, Risk Score : {x.Score}, Risk Msg : {x.Message}";
				});
			}
			impl.ExternalOrderOriginal = true.ValueField();

			PX.Objects.AR.Customer customer = null;
			//Customer ID
			if (bucket.Customer != null && data.Customer.Id > 0 && (!string.IsNullOrEmpty(data.Customer.Email) || !string.IsNullOrEmpty(data.Customer.Phone)))
			{
				customer = PXSelectJoin<PX.Objects.AR.Customer,
					LeftJoin<BCSyncStatus, On<PX.Objects.AR.Customer.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, BCEntitiesAttribute.Customer, data.Customer.Id);
				if (customer == null) throw new PXException(BCMessages.CustomerNotSyncronized, data.Customer.Id);
				if (customer.CuryID != impl.CurrencyID.Value && !customer.AllowOverrideCury.Value) throw new PXException(BCMessages.OrderCurrencyNotMathced, impl.CurrencyID.Value, customer.CuryID);
				impl.CustomerID = customer.AcctCD.ValueField();
			}
			else
			{
				if (!string.IsNullOrEmpty(data.Email))
				{
					customer = PXSelectJoin<PX.Objects.AR.Customer,
						LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defContactID, Equal<PX.Objects.CR.Contact.contactID>>>,
						Where<PX.Objects.CR.Contact.eMail, Equal<Required<PX.Objects.CR.Contact.eMail>>>>.Select(this, data.Email);
				}
				if (customer == null && !string.IsNullOrEmpty(data.Phone))
				{
					customer = PXSelectJoin<PX.Objects.AR.Customer,
						LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defContactID, Equal<PX.Objects.CR.Contact.contactID>>>,
						Where<PX.Objects.CR.Contact.phone1, Equal<Required<PX.Objects.CR.Contact.phone1>>, Or<PX.Objects.CR.Contact.phone2, Equal<Required<PX.Objects.CR.Contact.phone2>>>>>.Select(this, data.Phone, data.Phone);
				}
				if (customer == null)
				{
					customer = PX.Objects.AR.Customer.PK.Find(this, currentBindingExt.GuestCustomerID);
					if (customer == null) throw new PXException(ShopifyMessages.NoGuestCustomer);
				}
				else
				{
					if (customer.CuryID != impl.CurrencyID.Value && !customer.AllowOverrideCury.Value) throw new PXException(BCMessages.OrderCurrencyNotMathced, impl.CurrencyID.Value, customer.CuryID);
				}
				impl.CustomerID = customer.AcctCD.ValueField();
			}
			PX.Objects.CR.Location location = null;
			//Location ID
			if (bucket.Location != null)
			{
				location = PXSelectJoin<PX.Objects.CR.Location,
					LeftJoin<BCSyncStatus, On<PX.Objects.CR.Location.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, BCEntitiesAttribute.Address, bucket?.Location?.ExternID);
				if (location != null)
				{
					impl.LocationID = location.LocationCD.ValueField();
				}
			}
			impl.FinancialSettings = new FinancialSettings();
			impl.FinancialSettings.Branch = Branch.PK.Find(this, currentBinding.BranchID)?.BranchCD?.ValueField();
			#endregion

			#region ShippingSettings
			//Freight
			impl.Totals = new Totals();
			impl.Totals.OverrideFreightAmount = new BooleanValue() { Value = true };
			impl.Totals.OrderWeight = data.TotalWeightInGrams.ValueField();
			List<OrderAdjustment> refundOrderAdjustments = null;
			List<RefundLineItem> refundItems = null;
			refundOrderAdjustments = data.Refunds.Count > 0 ? data.Refunds.SelectMany(x => x.OrderAdjustments)?.ToList() : null;
			refundItems = data.Refunds?.Count > 0 ? data.Refunds.SelectMany(x => x.RefundLineItems)?.ToList() : null;

			decimal shippingrefundAmt = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.ShippingRefund)?.Sum(x => (-x.Amount) ?? 0m) ?? 0m;
			decimal shippingrefundAmtTax = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.ShippingRefund)?.Sum(x => (-x.TaxAmount) ?? 0m) ?? 0m;
			//Included the shipping discount, if there is a free shipping discount applied, freight fee should be 0.
			//reduce the shipping refund amount from freight
			impl.Totals.Freight = (data.ShippingLines.Sum(x => x.ShippingCostExcludingTax) - data.ShippingLines.SelectMany(x => x.DiscountAllocations)?.Sum(x => x?.DiscountAmount ?? 0m) - shippingrefundAmt).ValueField();
			if (impl.Totals.Freight.Value < 0)
				throw new PXException(BCMessages.ValueCannotBeLessThenZero, data.ShippingLines.Sum(x => x.DiscountAllocations?.Count > 0 ? x.DiscountAllocations.Sum(d => d.DiscountAmount) ?? 0m : 0m), data.ShippingLines.Sum(x => x.ShippingCostExcludingTax));

			State state;
			//ShippingSettings
			impl.ShippingSettings = new ShippingSettings();
			PXCache cache = base.Caches[typeof(BCShippingMappings)];
			//Ship-To Address && Contact
			if (data.ShippingAddress != null)
			{
				storeShippingZones = storeShippingZones ?? storeDataProvider.GetShippingZones().ToList();
				if (storeShippingZones.Count() > 0)
				{
					var shippingZone = storeShippingZones.FirstOrDefault(x => x.Countries?.Count > 0 && x.Countries.Any(c => c.Code == data.ShippingAddress.CountryCode)) ??
										storeShippingZones.FirstOrDefault(x => x.Countries?.Count > 0 && x.Countries.Any(c => c.Code == "*"));
					var shippingLine = data.ShippingLines.FirstOrDefault();
					if (shippingZone != null && shippingLine != null)
					{
						var mappingValue = shippingMappings?.FirstOrDefault(x => x.ShippingZone == shippingZone.Name && x.ShippingMethod == shippingLine.Title) ??
							shippingMappings?.FirstOrDefault(x => x.ShippingZone == shippingZone.Name && x.ShippingMethod == BCObjectsConstants.ShippingMethod_Default);
						if (mappingValue != null)
						{
							impl.ShipVia = impl.ShippingSettings.ShipVia = mappingValue.CarrierID.ValueField();
							impl.ShippingSettings.ShippingZone = mappingValue.ZoneID.ValueField();
							impl.ShippingSettings.ShippingTerms = mappingValue.ShipTermsID.ValueField();
						}
						else
						{
							BCShippingMappings inserted = new BCShippingMappings() { BindingID = Operation.Binding, ShippingZone = shippingZone.Name, ShippingMethod = shippingLine.Title };
							cache.Insert(inserted);
						}
					}
				}
				if (cache.Inserted.Count() > 0)
					cache.Persist(PXDBOperation.Insert);

				impl.ShipToAddressOverride = true.ValueField();
				impl.ShipToAddress = new Address();
				impl.ShipToAddress.AddressLine1 = data.ShippingAddress.Address1.ValueField();
				impl.ShipToAddress.AddressLine2 = data.ShippingAddress.Address2.ValueField();
				impl.ShipToAddress.City = data.ShippingAddress.City.ValueField();
				impl.ShipToAddress.Country = data.ShippingAddress.CountryCode.ValueField();
				if (!string.IsNullOrEmpty(data.ShippingAddress.ProvinceCode))
				{
					state = states.Select(data.ShippingAddress.Province, data.ShippingAddress.ProvinceCode);
					if (state == null)
						impl.ShipToAddress.State = GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), data.ShippingAddress.Province, data.ShippingAddress.ProvinceCode).ValueField();
					else
						impl.ShipToAddress.State = state.StateID?.ValueField();
				}
				else
					impl.ShipToAddress.State = string.Empty.ValueField();
				impl.ShipToAddress.PostalCode = data.ShippingAddress.PostalCode?.ToUpperInvariant()?.ValueField();

				impl.ShipToContactOverride = true.ValueField();
				impl.ShipToContact = new DocContact();
				impl.ShipToContact.Phone1 = data.ShippingAddress.Phone.ValueField();
				impl.ShipToContact.Email = data.Email.ValueField();
				impl.ShipToContact.Attention = data.ShippingAddress.Name.ValueField();
				impl.ShipToContact.BusinessName = data.ShippingAddress.Company.ValueField();
			}

			//Bill-To Address && Contact
			impl.BillToAddressOverride = true.ValueField();
			impl.BillToAddress = new Address();
			impl.BillToContactOverride = true.ValueField();
			impl.BillToContact = new DocContact();
			if (data.BillingAddress == null && data.ShippingAddress != null)
			{
				impl.BillToAddress.AddressLine1 = impl.ShipToAddress.AddressLine1;
				impl.BillToAddress.AddressLine2 = impl.ShipToAddress.AddressLine2;
				impl.BillToAddress.City = impl.ShipToAddress.City;
				impl.BillToAddress.Country = impl.ShipToAddress.Country;
				impl.BillToAddress.State = impl.ShipToAddress.State;
				impl.BillToAddress.PostalCode = impl.ShipToAddress.PostalCode;

				impl.BillToContact.Phone1 = impl.ShipToContact.Phone1;
				impl.BillToContact.Email = impl.ShipToContact.Email;
				impl.BillToContact.BusinessName = impl.ShipToContact.BusinessName;
				impl.BillToContact.Attention = impl.ShipToContact.Attention;
			}
			else if (data.BillingAddress != null)
			{
				impl.BillToAddress.AddressLine1 = data.BillingAddress.Address1.ValueField();
				impl.BillToAddress.AddressLine2 = data.BillingAddress.Address2.ValueField();
				impl.BillToAddress.City = data.BillingAddress.City.ValueField();
				impl.BillToAddress.Country = data.BillingAddress.CountryCode.ValueField();
				if (!string.IsNullOrEmpty(data.BillingAddress.ProvinceCode) && data.BillingAddress.ProvinceCode.Equals(data.ShippingAddress?.ProvinceCode))
				{
					impl.BillToAddress.State = impl.ShipToAddress.State;
				}
				else if (!string.IsNullOrEmpty(data.BillingAddress.ProvinceCode))
				{
					state = states.Select(data.BillingAddress.Province, data.BillingAddress.ProvinceCode);
					if (state == null)
					{
						impl.BillToAddress.State = GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), data.BillingAddress.Province, data.BillingAddress.ProvinceCode).ValueField();
					}
					else
						impl.BillToAddress.State = state.StateID?.ValueField();
				}
				else
					impl.BillToAddress.State = string.Empty.ValueField();
				impl.BillToAddress.PostalCode = data.BillingAddress.PostalCode?.ToUpperInvariant()?.ValueField();

				impl.BillToContact.Phone1 = data.BillingAddress.Phone.ValueField();
				impl.BillToContact.Email = data.Email.ValueField();
				impl.BillToContact.BusinessName = data.BillingAddress.Company.ValueField();
				impl.BillToContact.Attention = data.BillingAddress.Name.ValueField();
			}
			else if (data.BillingAddress == null && data.Customer?.DefaultAddress != null)
			{
				impl.BillToAddress.AddressLine1 = data.Customer?.DefaultAddress.Address1.ValueField();
				impl.BillToAddress.AddressLine2 = data.Customer?.DefaultAddress.Address2.ValueField();
				impl.BillToAddress.City = data.Customer?.DefaultAddress.City.ValueField();
				impl.BillToAddress.Country = data.Customer?.DefaultAddress.CountryCode.ValueField();
				if (!string.IsNullOrEmpty(data.Customer.DefaultAddress.ProvinceCode) && data.Customer.DefaultAddress.ProvinceCode.Equals(data.ShippingAddress?.ProvinceCode))
				{
					impl.BillToAddress.State = impl.ShipToAddress.State;
				}
				else if (!string.IsNullOrEmpty(data.Customer?.DefaultAddress.ProvinceCode))
				{
					state = states.Select(data.Customer?.DefaultAddress.Province, data.Customer?.DefaultAddress.ProvinceCode);
					if (state == null)
					{
						impl.BillToAddress.State = GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), data.Customer?.DefaultAddress.Province, data.Customer?.DefaultAddress.ProvinceCode).ValueField();
					}
					else
						impl.BillToAddress.State = state.StateID?.ValueField();
				}
				else
					impl.BillToAddress.State = string.Empty.ValueField();
				impl.BillToAddress.PostalCode = data.Customer?.DefaultAddress.PostalCode?.ToUpperInvariant()?.ValueField();

				impl.BillToContact.Phone1 = data.Customer?.DefaultAddress.Phone.ValueField();
				impl.BillToContact.Email = data.Email.ValueField();
				impl.BillToContact.BusinessName = data.Customer?.DefaultAddress.Company.ValueField();
				impl.BillToContact.Attention = data.Customer?.DefaultAddress.Name.ValueField();
			}
			else
			{
				impl.BillToAddressOverride = false.ValueField();
				impl.BillToContactOverride = false.ValueField();
			}
			#endregion

			#region Products
			impl.Details = new List<SalesOrderDetail>();
			Decimal? totalDiscount = 0m;
			foreach (var orderItem in data.LineItems)
			{
				decimal? quantity = orderItem.Quantity;
				decimal? subTotal = orderItem.Price * quantity;
				//Check refund data whether have this orderItem data
				List<RefundLineItem> matchedRefundItems = null;
				decimal? refundSubtotal = 0;
				decimal? refundQuantity = 0;
				SalesOrderDetail detail = new SalesOrderDetail();
				detail.DiscountAmount = 0m.ValueField();
				if (cancelledOrder == false && refundItems?.Count > 0 && refundItems.Any(x => x.LineItemId == orderItem.Id))
				{
					matchedRefundItems = refundItems.Where(x => x.LineItemId == orderItem.Id).ToList();
					refundQuantity = matchedRefundItems.Sum(x => x.Quantity);

					//If Admin modifies the item quantity and then changes back to original quantity, Shopify will keep the total quantity and use it to do the calculation in the item;
					//and add a new record to the refund item to keep the same amount. So we have to use this data to re-calculate the tax and discount if they applied.
					quantity = orderItem.Quantity - refundQuantity;
					subTotal = orderItem.Price * quantity;
					refundSubtotal = matchedRefundItems.Sum(x => x.SubTotal);
				}

				if (orderItem.DiscountAllocations?.Count > 0)
				{
					var itemDiscount = orderItem.DiscountAllocations.Sum(x => x.DiscountAmount);
					if (refundSubtotal != 0)
					{
						itemDiscount = itemDiscount + refundSubtotal - (orderItem.Price * refundQuantity);
					}
					totalDiscount += itemDiscount;
					if (currentBindingExt.PostDiscounts == BCPostDiscountAttribute.LineDiscount)
					{
						detail.DiscountAmount = itemDiscount.ValueField();
					}

				}
				//If the refund item have the same quantity, that means this item have been removed from Order.
				if (quantity == 0)
				{
					skipOrderItems.Add(orderItem.Id.Value);
					continue;
				}

				String inventoryCD = GetInventoryCDByExternID(orderItem.ProductId?.ToString(), orderItem.VariantId.ToString(), orderItem.Sku ?? string.Empty, orderItem.Name, out string uom);

				detail.Branch = impl.Branch = impl.FinancialSettings.Branch;
				detail.InventoryID = inventoryCD?.TrimEnd().ValueField();
				detail.OrderQty = quantity.ValueField();
				detail.UOM = uom.ValueField();
				detail.UnitPrice = orderItem.Price.ValueField();
				detail.LineDescription = orderItem.Name.ValueField();
				detail.ExtendedPrice = subTotal.ValueField();
				detail.FreeItem = (orderItem.Price == 0m).ValueField();
				detail.ManualPrice = true.ValueField();

				//Check for existing
				if (existing?.Details?.Count() > 0)
				{
					existing?.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && orderItem.Id.ToString() == d.ExternID).With(d => detail.Id = d.LocalID);
				}
				else if (existing?.Details?.Count() == 0 && presented != null && presented.Details?.Count > 0)
				{

					presented.Details.FirstOrDefault(x => x.InventoryID.Value == detail.InventoryID.Value).With(e => detail.Id = e.Id);
				}

				impl.Details.Add(detail);
			}
			#endregion

			var totalOrderRefundAmout = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.RefundDiscrepancy)?.Sum(y => (y.Amount)) ?? 0;
			if (totalOrderRefundAmout != 0)
			{
				var detail = InsertRefundAmountItem(totalOrderRefundAmout, impl.Branch);

				if (presented != null && presented.Details?.Count > 0)
				{
					presented.Details.FirstOrDefault(x => x.InventoryID.Value == detail.InventoryID.Value).With(e => detail.Id = e.Id);
				}
				impl.Details.Add(detail);
			}

			#region Taxes
			impl.TaxDetails = new List<TaxDetail>();
			if (data.TaxLines?.Count > 0)
			{
				if (currentBindingExt.SyncTaxes == BCTaxSyncAttribute.ManualTaxes || currentBindingExt.SyncTaxes == BCTaxSyncAttribute.AutomaticTaxes)
				{
					impl.IsTaxValid = true.ValueField();
					foreach (OrderTaxLine tax in data.TaxLines)
					{
						//Third parameter set to tax name in order to simplify process (if tax names are equal and user don't want to fill lists)
						String mappedTaxName = GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxSubstitutionListID, tax.TaxName, tax.TaxName);
						mappedTaxName = TrimAutomaticTaxNameForAvalara(mappedTaxName);
						if (String.IsNullOrEmpty(mappedTaxName)) throw new PXException(PX.Commerce.Objects.BCObjectsMessages.TaxNameDoesntExist);

						decimal? taxable = 0m, taxableExcludeRefundItems = 0m;
						decimal? taxAmount = 0m;
						if (tax.TaxRate != 0m)
						{
							var lineItemsWithTax = data.LineItems.Where(x => x.TaxLines?.Count > 0 && x.TaxLines.Any(t => t.TaxAmount > 0m && t.TaxName == tax.TaxName));
							var shippingItemsWithTax = data.ShippingLines.Where(x => x.TaxLines?.Count > 0 && x.TaxLines.Any(t => t.TaxAmount > 0m && t.TaxName == tax.TaxName));
							taxable = lineItemsWithTax.Sum(x => (x?.Price * x?.Quantity) ?? 0m) - lineItemsWithTax.SelectMany(x => x.DiscountAllocations)?.Sum(x => x.DiscountAmount ?? 0m);
							taxable += shippingItemsWithTax.Sum(x => x.ShippingCostExcludingTax ?? 0m) - shippingItemsWithTax.SelectMany(x => x.DiscountAllocations)?.Sum(x => x.DiscountAmount ?? 0m) - shippingrefundAmt;
							taxableExcludeRefundItems = taxable;
							taxAmount = tax.TaxAmount;
							if (cancelledOrder == false && refundItems?.Count > 0)
							{
								//If the line item shows in the Refunds field, we have to calculate the Tax manually.
								var lineItemIds = lineItemsWithTax.Select(x => x.Id).Distinct();
								var refundItemsWithTax = refundItems.Where(x => lineItemIds.Contains(x.LineItemId)).ToList();
								if (refundItemsWithTax?.Count > 0)
								{
									taxableExcludeRefundItems = taxable - refundItemsWithTax.Sum(x => x.SubTotal ?? 0m);
									taxAmount = Math.Round((decimal)(taxableExcludeRefundItems * tax.TaxRate), 2);
								}
							}
						}

						TaxDetail inserted = impl.TaxDetails.FirstOrDefault(i => i.TaxID.Value?.Equals(mappedTaxName, StringComparison.InvariantCultureIgnoreCase) == true);
						if (inserted == null)
						{
							impl.TaxDetails.Add(new TaxDetail()
							{
								TaxID = mappedTaxName.ValueField(),
								TaxAmount = taxAmount.ValueField(),
								TaxRate = (tax.TaxRate * 100).ValueField(),
								TaxableAmount = taxableExcludeRefundItems.ValueField()
							});
						}
						else if (inserted.TaxAmount != null && taxable == taxableExcludeRefundItems)
						{
							inserted.TaxAmount.Value += tax.TaxAmount;
						}
					}
				}
			}

			//Check for tax Ids with more than 30 characters
			String[] tooLongTaxIDs = ((impl.TaxDetails ?? new List<TaxDetail>()).Select(x => x.TaxID?.Value).Where(x => (x?.Length ?? 0) > PX.Objects.TX.Tax.taxID.Length).ToArray());
			if (tooLongTaxIDs != null && tooLongTaxIDs.Length > 0)
			{
				throw new PXException(PX.Commerce.Objects.BCObjectsMessages.CannotFindSaveTaxIDs, String.Join(",", tooLongTaxIDs), PX.Objects.TX.Tax.taxID.Length);
			}

			if (currentBindingExt.SyncTaxes != BCTaxSyncAttribute.NoSync && currentBindingExt.PrimaryTaxZoneID != null)
			{
				impl.FinancialSettings.OverrideTaxZone = true.ValueField();
				impl.FinancialSettings.CustomerTaxZone = currentBindingExt.PrimaryTaxZoneID.ValueField();
			}
			#endregion

			#region Discounts
			impl.DisableAutomaticDiscountUpdate = true.ValueField();
			impl.DiscountDetails = new List<SalesOrdersDiscountDetails>();
			if (data.DiscountApplications?.Count > 0)
			{
				SalesOrdersDiscountDetails itemDiscountDetail = null;
				var totalItemDiscounts = data.LineItems.SelectMany(x => x.DiscountAllocations).ToList();
				//If there is a shipping discount, it has been applied to the Freight fee calculation above.
				for (int i = 0; i < data.DiscountApplications.Count; i++)
				{
					var discountItem = data.DiscountApplications[i];
					SalesOrdersDiscountDetails detail = new SalesOrdersDiscountDetails();
					detail.Type = PX.Objects.Common.Discount.DiscountType.ExternalDocument.ValueField();
					detail.ExternalDiscountCode = discountItem.Type == DiscountApplicationType.DiscountCode ? discountItem.Code.ValueField() : (discountItem.Title ?? discountItem.Description).ValueField();
					detail.Description = (discountItem.Description ?? string.Empty).ValueField();
					if (discountItem.TargetType == DiscountTargetType.ShippingLine)
					{
						detail.Description = ShopifyMessages.DiscountAppliedToShippingItem.ValueField();
						detail.DiscountAmount = 0m.ValueField();
						impl.DiscountDetails.Add(detail);
					}
					else
					{
						var matchedDiscounts = totalItemDiscounts.Where(x => x.DiscountApplicationIndex == i);
						if (currentBindingExt.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount)
						{
							detail.DiscountAmount = matchedDiscounts.Sum(x => x.DiscountAmount ?? 0m).ValueField();
						}
						else
						{
							detail.Description = ShopifyMessages.DiscountAppliedToLineItem.ValueField();
							detail.DiscountAmount = 0m.ValueField();
						}
						//If the refund items have discount, we cannot get the accurate discount amount, we have to combine all discounts to the order level.
						if (cancelledOrder == false && refundItems?.Count > 0 && refundItems.Any(x => x.OrderLineItem.DiscountAllocations?.Count > 0) && currentBindingExt.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount)
						{
							itemDiscountDetail = detail;
							itemDiscountDetail.ExternalDiscountCode = ShopifyMessages.RefundDiscount.ValueField();
							itemDiscountDetail.DiscountAmount = totalDiscount.ValueField();
							break;
						}
						else
						{
							impl.DiscountDetails.Add(detail);
						}
					}
				}
				if (itemDiscountDetail != null)
				{
					impl.DiscountDetails.Add(itemDiscountDetail);
				}
			}
			#endregion

			#region Payment
			if (existing == null && !paymentProcessor.ImportMappings.Select().Any())
			{
				impl.Payments = new List<SalesOrderPayment>();
				foreach (MappedPayment payment in bucket.Payments)
				{
					OrderTransaction dataPayment = payment.Extern;
					SalesOrderPayment implPament = new SalesOrderPayment();
					if (!payment.IsNew)
						continue;
					implPament.ExternalID = payment.ExternID;

					//Product
					implPament.DocType = PX.Objects.AR.Messages.Prepayment.ValueField();
					implPament.CurrencyID = impl.CurrencyID;
					implPament.ApplicationDate = dataPayment.DateCreatedAt.ToDate(false, PXTimeZoneInfo.FindSystemTimeZoneById(currentBindingExt.OrderTimeZone)).ValueField();
					implPament.PaymentAmount = ((decimal)dataPayment.Amount).ValueField();
					implPament.PaymentRef = (dataPayment.Authorization ?? dataPayment.Id.ToString()).ValueField();
					implPament.Hold = false.ValueField();
					implPament.AppliedToOrder = ((decimal)dataPayment.Amount).ValueField();

					PX.Objects.AR.ARPayment existingPayment = PXSelect<PX.Objects.AR.ARPayment,
							Where<PX.Objects.AR.ARPayment.extRefNbr, Equal<Required<PX.Objects.AR.ARPayment.extRefNbr>>>>.Select(this, implPament.PaymentRef.Value);
					if (existingPayment != null) continue; //skip if payment with same ref nbr exists already.

					BCPaymentMethods methodMapping = paymentProcessor.GetPaymentMethodMapping(dataPayment.Gateway, dataPayment.Currency,
						out string paymentMethod, out string cashAcount, out string processingCenter);
					if (methodMapping.ReleasePayments ?? false) continue; //don't save payment with the order if the require release.

					implPament.PaymentMethod = paymentMethod?.ValueField();
					implPament.CashAccount = cashAcount?.Trim()?.ValueField();
					implPament.Description = $"{currentBinding.BindingName} | Order: {bucket.Order?.Extern?.Name} | Type: {dataPayment.Kind.ToString()} | Status: {dataPayment.Status?.ToString()} | Gateway: {dataPayment.Gateway}".ValueField();

					//Credit Card:
					if (dataPayment.Authorization != null && processingCenter != null)
					{
						String paymentTran = paymentProcessor.ParceTransactionNumber(dataPayment.Authorization);

						//implPament.IsNewCard = true.ValueField();
						implPament.SaveCard = false.ValueField();
						implPament.ProcessingCenterID = processingCenter.ValueField();

						SalesOrderCreditCardTransactionDetail creditCardDetail = new SalesOrderCreditCardTransactionDetail();
						creditCardDetail.TranNbr = paymentTran.ValueField();
						creditCardDetail.TranDate = implPament.ApplicationDate;
						//creditCardDetail.ExtProfileId = dataPayment.PaymentInstrumentToken.ValueField();
						switch (dataPayment.Kind)
						{
							case TransactionType.Authorization:
								creditCardDetail.TranType = "AUT".ValueField();
								break;
							case TransactionType.Capture:
								creditCardDetail.TranType = "PAC".ValueField();
								break;
							case TransactionType.Sale:
								creditCardDetail.TranType = "AAC".ValueField();
								break;
							default:
								creditCardDetail.TranType = "UKN".ValueField();
								break;
						}

						implPament.CreditCardTransactionInfo = new List<SalesOrderCreditCardTransactionDetail>(new[] { creditCardDetail });
					}

					impl.Payments.Add(implPament);
				}
			}
			#endregion

			#region Adjust for Existing
			if (presented != null)
			{
				obj.Local.OrderType = presented.OrderType; //Keep the same order Type

				//remap entities if existing
				presented.DiscountDetails?.ForEach(e => obj.Local.DiscountDetails?.FirstOrDefault(n => n.ExternalDiscountCode.Value == e.ExternalDiscountCode.Value).With(n => n.Id = e.Id));
				presented.Payments?.ForEach(e => obj.Local.Payments?.FirstOrDefault(n => n.PaymentRef.Value == e.PaymentRef.Value).With(n => n.Id = e.Id));

				//delete unnecessary entities
				obj.Local.Details?.AddRange(presented.Details == null ? Enumerable.Empty<SalesOrderDetail>()
					: presented.Details.Where(e => obj.Local.Details == null || !obj.Local.Details.Any(n => e.Id == n.Id)).Select(n => new SalesOrderDetail() { Id = n.Id, Delete = true, InventoryID = n.InventoryID }));
				obj.Local.DiscountDetails?.AddRange(presented.DiscountDetails == null ? Enumerable.Empty<SalesOrdersDiscountDetails>()
					: presented.DiscountDetails.Where(e => obj.Local.DiscountDetails == null || !obj.Local.DiscountDetails.Any(n => e.Id == n.Id)).Select(n => new SalesOrdersDiscountDetails() { Id = n.Id, Delete = true }));
				obj.Local.Payments?.AddRange(presented.Payments == null ? Enumerable.Empty<SalesOrderPayment>()
					: presented.Payments.Where(e => obj.Local.Payments == null || !obj.Local.Payments.Any(n => e.Id == n.Id)).Select(n => new SalesOrderPayment() { Id = n.Id, Delete = true }));
			}
			#endregion
		}

		public override void SaveBucketImport(SPSalesOrderBucket bucket, IMappedEntity existing, String operation)
		{
			MappedOrder obj = bucket.Order;
			SalesOrder local = obj.Local;
			SalesOrder presented = existing?.Local as SalesOrder;
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			SalesOrder impl;
			//If we need to cancel the order in Acumatica
			if (obj.Extern?.CancelledAt != null && presented != null)
			{
				impl = cbapi.Invoke<SalesOrder, CancelSalesOrder>(null, obj.LocalID);
			}
			else
			{
				List<SalesOrderDetail> deletedLines = null;
				if (obj.Local.Details.Any(x => x.Delete) && obj.Local.Details.Any(x => !x.Delete))
				{
					deletedLines = obj.Local.Details.Where(x => x.Delete).ToList(); // only deleted lines
					obj.Local.Details = obj.Local.Details.Where(x => x.Delete == false).ToList();
				}


				#region Taxes
				LogTaxDetails(obj.SyncID, obj.Local);
				#endregion

				impl = cbapi.Put<SalesOrder>(obj.Local, obj.LocalID);

				#region Taxes
				ValidateTaxes(obj.SyncID, impl, obj.Local);
				#endregion

				//Due to the saving issue we should delete all needed order lines first with the separate calls.
				if (deletedLines != null)
				{
					obj.Local.Details = deletedLines;
					impl = cbapi.Put<SalesOrder>(obj.Local, obj.LocalID);
				}


				//If we need to cancel the order in Acumatica
				if (obj.Extern?.CancelledAt != null)
				{
					impl = cbapi.Invoke<SalesOrder, CancelSalesOrder>(null, impl.SyncID);
				}
			}
			obj.AddLocal(impl, impl.SyncID, impl.SyncTime);

			// Save Details
			DetailInfo[] oldDetails = obj.Details.ToArray();
			obj.ClearDetails();
			if (bucket.Location?.Extern != null && bucket.Location?.ExternID != null)
			{
				obj.AddDetail(BCEntitiesAttribute.OrderAddress, impl.SyncID, bucket.Location.ExternID); //Shipment ID detail	
			}
			foreach (var orderItem in obj.Extern.LineItems) //Line ID detail
			{
				if (orderItem.Quantity == 0 || skipOrderItems.Contains(orderItem.Id.Value)) continue;

				SalesOrderDetail detail = null;
				detail = impl.Details.FirstOrDefault(x => x.NoteID.Value == oldDetails.FirstOrDefault(o => o.ExternID == orderItem.Id.ToString())?.LocalID);
				if (detail == null)
				{
					String inventoryCD;
					inventoryCD = GetInventoryCDByExternID(orderItem.ProductId.ToString(), orderItem.VariantId.ToString(), orderItem.Sku ?? string.Empty, orderItem.Name, out string uom);
					detail = impl.Details.FirstOrDefault(x => !obj.Details.Any(o => x.NoteID.Value == o.LocalID)
					&& x.InventoryID.Value == inventoryCD);
				}
				if (detail != null)
				{
					obj.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, orderItem.Id.ToString());
					continue;
				}
				throw new PXException(BCMessages.CannotMapLines);
			}

			UpdateStatus(obj, operation);

			#region Payments
			if (existing == null && local.Payments != null && impl.Payments != null)
			{
				for (int i = 0; i < local.Payments.Count; i++)
				{
					SalesOrderPayment sent = local.Payments[i];

					SalesOrderPayment received = impl.Payments.FirstOrDefault(x => sent.PaymentRef?.Value == x.PaymentRef?.Value);
					if (received == null && impl.Payments.Count > i) received = impl.Payments[i];
					if (received == null) continue;

					String docType = (new PX.Objects.AR.ARDocType()).ValueLabelPairs.First(p => p.Label == received.DocType.Value).Value;
					PX.Objects.AR.ARPayment payment = PX.Objects.AR.ARPayment.PK.Find(this, docType, received.ReferenceNbr.Value);
					if (payment == null) continue;

					MappedPayment objPayment = bucket.Payments.FirstOrDefault(x => x.ExternID == sent.ExternalID);
					if (objPayment == null) continue;

					objPayment.AddLocal(null, payment.NoteID, payment.LastModifiedDateTime);
					UpdateStatus(objPayment, operation);
				}
			}
			#endregion
		}


		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			IEnumerable<SalesOrder> impls = cbapi.GetAll<SalesOrder>(
				new SalesOrder()
				{
					OrderType = currentBindingExt.OrderType.SearchField(),
					OrderNbr = new StringReturn(),
					Status = new StringReturn(),
					CustomerID = new StringReturn(),
					ExternalRef = new StringReturn(),
					Details = new List<SalesOrderDetail>() { new SalesOrderDetail() {
						ReturnBehavior = ReturnBehavior.OnlySpecified,
						InventoryID = new StringReturn() } }
				},
				minDateTime, maxDateTime, filters, GetCustomFieldsForExport());
			var invIDs = new List<string>();

			BCEntity entity = GetEntity(Operation.EntityType);
			if (impls != null && impls.Count() > 0)
			{
				List<BCSyncStatus> bcSyncStatusList = null;
				int countNum = 0;
				List<MappedOrder> mappedList = new List<MappedOrder>();
				foreach (SalesOrder impl in impls)
				{
					MappedOrder obj = new MappedOrder(impl, impl.SyncID, impl.SyncTime);

					mappedList.Add(obj);
					countNum++;
					if (countNum % BatchFetchCount == 0 || countNum == impls.Count())
					{
						var localIDs = mappedList.Select(x => x.LocalID.Value).ToArray();
						bcSyncStatusList = GetBCSyncStatusResult(entity.EntityType, null, localIDs, null).Select(x => x.GetItem<BCSyncStatus>()).ToList();
						EntityStatus status = 0;
						foreach (var oneMapped in mappedList)
						{
							status = EnsureStatusBulk(bcSyncStatusList, oneMapped, SyncDirection.Export, true);
						}
						if (status == EntityStatus.Pending) invIDs.Add(impl?.OrderNbr?.Value ?? "__");
						mappedList.Clear();
					}
				}
			}
		}
		public override EntityStatus GetBucketForExport(SPSalesOrderBucket bucket, BCSyncStatus syncstatus)
		{
			SalesOrder impl = cbapi.GetByID<SalesOrder>(syncstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			MappedOrder obj = bucket.Order = bucket.Order.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Order, SyncDirection.Export);

			if (status != EntityStatus.Pending && status != EntityStatus.Syncronized)
				return status;

			if (GetEntity(BCEntitiesAttribute.Shipment)?.IsActive == true && impl.Shipments != null)
			{
				foreach (SalesOrderShipment orderShipmentImpl in impl.Shipments)
				{
					if (orderShipmentImpl.ShippingNoteID?.Value == null) continue;

					BCShipments shipmentImpl = new BCShipments();
					shipmentImpl.ShippingNoteID = orderShipmentImpl.ShippingNoteID;
					shipmentImpl.OrderType = impl.OrderType;
					shipmentImpl.OrderNbr = impl.OrderNbr;
					shipmentImpl.ShipmentNumber = orderShipmentImpl.ShipmentNbr;
					shipmentImpl.ShipmentType = orderShipmentImpl.ShipmentType;
					shipmentImpl.Confirmed = (orderShipmentImpl.Status?.Value == BCAPICaptions.Confirmed).ValueField();

					MappedShipment shipmentObj = new MappedShipment(shipmentImpl, shipmentImpl.ShippingNoteID.Value, orderShipmentImpl.LastModifiedDateTime.Value);
					EntityStatus shipmentStatus = EnsureStatus(shipmentObj, SyncDirection.Export);

					if (shipmentStatus == EntityStatus.Pending)
						bucket.Shipments.Add(shipmentObj);
				}
			}

			BCSyncStatus customerStatus = PXSelectJoin<BCSyncStatus,
				InnerJoin<PX.Objects.AR.Customer, On<PX.Objects.AR.Customer.noteID, Equal<BCSyncStatus.localID>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<PX.Objects.AR.Customer.acctCD, Equal<Required<PX.Objects.AR.Customer.acctCD>>>>>>>
				.Select(this, BCEntitiesAttribute.Customer, impl.CustomerID?.Value);
			if (customerStatus == null)
			{
				Customer implCust = cbapi.Get<Customer>(new Customer() { CustomerID = new StringSearch() { Value = impl.CustomerID.Value } });
				if (implCust == null)
					throw new PXException(BCMessages.NoCustomerForOrder, obj.Local.OrderNbr.Value);
				MappedCustomer objCust = new MappedCustomer(implCust, implCust.SyncID, implCust.SyncTime);
				EntityStatus custStatus = EnsureStatus(objCust, SyncDirection.Export);

				if (custStatus == EntityStatus.Pending)
					bucket.Customer = objCust;
			}
			return status;
		}

		public override void MapBucketExport(SPSalesOrderBucket bucket, IMappedEntity existing)
		{
			//MappedOrder obj = bucket.Order;
			//if (obj.Local.Details == null || obj.Local.Details.Count == 0) throw new PXException(ShopifyMessages.NoOrderDetails, obj.Local.OrderNbr.Value);
			//SalesOrder impl = obj.Local;
			//OrderData orderData = obj.Extern = new OrderData();

			//obj.Extern.Id = APIHelper.ReferenceParse(impl.ExternalRef?.Value, CurrentBinding.BindingName).ToInt();
			//BCSyncStatus customerStatus = PXSelectJoin<BCSyncStatus,
			//	InnerJoin<PX.Objects.AR.Customer, On<PX.Objects.AR.Customer.noteID, Equal<BCSyncStatus.localID>>>,
			//	Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
			//		And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
			//		And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
			//		And<PX.Objects.AR.Customer.acctCD, Equal<Required<PX.Objects.AR.Customer.acctCD>>>>>>>
			//	.Select(this, BCEntitiesAttribute.Customer, impl.CustomerID?.Value);
			//var customer = PX.Objects.AR.Customer.PK.Find(this, CurrentBindingExt.GuestCustomerID);
			//if (customer != null && customer?.AcctCD.Trim() == impl.CustomerID?.Value?.Trim())
			//{
			//	orderData.Customer = null;
			//}
			//else if (customerStatus == null || customerStatus.ExternID == null)
			//{
			//	throw new PXException(ShopifyMessages.CustomerNotSyncronized, impl.CustomerID?.Value);
			//}
			//else
			//{
			//	orderData.Customer = new CustomerData();
			//	orderData.Customer.Id = customerStatus.ExternID.ToLong();
			//}
			//orderData.TotalDiscount = impl.Totals.DiscountTotal?.Value ?? 0m;
			//if (obj.IsNew) orderData.ProcessedAt = impl.Date.Value;

			//string attentionB = impl.BillToContact.Attention?.Value ?? impl.BillToContact.BusinessName?.Value;
			//orderData.Email = impl.BillToContact.Email?.Value;
			//orderData.BillingAddress = new OrderAddressData();
			//orderData.BillingAddress.Address1 = impl.BillToAddress.AddressLine1?.Value;
			//orderData.BillingAddress.Address2 = impl.BillToAddress.AddressLine2?.Value;
			//orderData.BillingAddress.City = impl.BillToAddress.City?.Value;
			//orderData.BillingAddress.PostalCode = impl.BillToAddress.PostalCode?.Value;
			//orderData.BillingAddress.Country = GetSubstituteExternByLocal(BCSubstitute.Country, impl.BillToAddress.Country?.Value, Country.PK.Find(this, impl.BillToAddress.Country?.Value)?.Description);
			//orderData.BillingAddress.Company = impl.BillToContact.BusinessName?.Value;
			//orderData.BillingAddress.Phone = impl.BillToContact.Phone1?.Value;
			//orderData.BillingAddress.FirstName = attentionB.FieldsSplit(0, attentionB);
			//orderData.BillingAddress.LastName = attentionB.FieldsSplit(1, attentionB);
			//orderData.ShippingLines = new List<OrderShippingLine>() { new OrderShippingLine() { ShippingCostExcludingTax = impl.Totals?.Freight?.Value ?? 0} };
			//orderData.TotalIncludingTax = impl.OrderTotal?.Value ?? 0;
			//orderData.TotalExcludingTax = orderData.TotalIncludingTax > 0 ? orderData.TotalIncludingTax - (impl.TaxTotal?.Value ?? 0) : orderData.TotalIncludingTax;

			//State state = PXSelect<State, Where<State.stateID, Equal<Required<State.stateID>>>>.Select(this, impl.BillToAddress.State?.Value);
			//if (state == null)
			//	orderData.BillingAddress.State = GetSubstituteExternByLocal(BCSubstitute.State, impl.BillToAddress.State?.Value, impl.BillToAddress.State?.Value);
			//else
			//	orderData.BillingAddress.State = state.Name;
			//orderData.BillingAddress.State = string.IsNullOrEmpty(orderData.BillingAddress.State) ? " " : orderData.BillingAddress.State;

			//string attentionS = impl.ShipToContact.Attention?.Value ?? impl.ShipToContact.BusinessName?.Value;
			//OrdersShippingAddressData shippingAddress = new OrdersShippingAddressData();
			//orderData.OrderShippingAddresses = new OrdersShippingAddressData[] { shippingAddress }.ToList();
			//shippingAddress.Id = obj.Details.Where(x => x.EntityType == BCEntitiesAttribute.OrderAddress && x.LocalID == obj.LocalID.Value).FirstOrDefault()?.ExternID.ToInt();
			//shippingAddress.Street1 = impl.ShipToAddress.AddressLine1?.Value;
			//shippingAddress.Street2 = impl.ShipToAddress.AddressLine2?.Value;
			//shippingAddress.City = impl.ShipToAddress.City?.Value;
			//shippingAddress.ZipCode = impl.ShipToAddress.PostalCode?.Value;
			//shippingAddress.Country = GetSubstituteExternByLocal(BCSubstitute.Country, impl.ShipToAddress.Country?.Value, Country.PK.Find(this, impl.ShipToAddress.Country?.Value)?.Description);
			//shippingAddress.Company = impl.ShipToContact.BusinessName?.Value;
			//shippingAddress.Phone = impl.ShipToContact.Phone1?.Value;
			//shippingAddress.FirstName = attentionS.FieldsSplit(0, attentionS);
			//shippingAddress.LastName = attentionS.FieldsSplit(1, attentionS);
			//state = PXSelect<State, Where<State.stateID, Equal<Required<State.stateID>>>>.Select(this, impl.ShipToAddress.State?.Value);
			//if (state == null)
			//	shippingAddress.State = GetSubstituteExternByLocal(BCSubstitute.State, impl.ShipToAddress.State?.Value, impl.ShipToAddress.State?.Value);
			//else
			//	shippingAddress.State = state.Name;

			//List<OrdersProductData> productData = new List<OrdersProductData>();
			//if (existing != null && existing.Extern != null)
			//{
			//	var syncDetails = PXSelectJoin<BCSyncDetail,
			//		InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>,
			//		Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
			//			And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
			//			And<BCSyncDetail.entityType, Equal<Required<BCSyncDetail.entityType>>,
			//			And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>
			//		.Select(this, BCEntitiesAttribute.OrderLine, existing.ExternID);
			//	var deletedList = syncDetails.Where(p => !impl.Details.Any(p2 => p2.NoteID.Value == ((BCSyncDetail)p).LocalID));

			//	foreach (BCSyncDetail detail in deletedList)
			//	{

			//		OrdersProductData product = new OrdersProductData();
			//		product.Id = detail.ExternID.ToInt();
			//		product.Quantity = 0;
			//		product.PriceExcludingTax = 0;
			//		product.PriceIncludingTax = 0;
			//		productData.Add(product);
			//	}
			//}
			//PX.Objects.IN.InventoryItem giftCertificate = bindingExt.GiftCertificateItemID != null ? PX.Objects.IN.InventoryItem.PK.Find(this, bindingExt.GiftCertificateItemID) : null;
			//foreach (SalesOrderDetail detail in impl.Details)
			//{
			//	OrdersProductData product = new OrdersProductData();
			//	if (giftCertificate != null && detail.InventoryID?.Value?.Trim() == giftCertificate.InventoryCD?.Trim()) continue;

			//	String key = GetProductExternIDByProductCD(detail.InventoryID?.Value, out Dictionary<Int32, String> options, out string sku);
			//	if (key == null) throw new PXException(ShopifyMessages.InvenotryNotSyncronized, detail.InventoryID?.Value);

			//	product.Id = obj.Details.Where(x => x.EntityType == BCEntitiesAttribute.OrderLine && x.LocalID == detail.NoteID.Value).FirstOrDefault()?.ExternID.ToInt();
			//	product.Quantity = (int)detail.OrderQty.Value;
			//	product.PriceExcludingTax = (decimal)detail.OrderQty.Value == 0 ? detail.Amount.Value.Value : detail.Amount.Value.Value / detail.OrderQty.Value.Value;
			//	product.PriceIncludingTax = (decimal)detail.OrderQty.Value == 0 ? detail.Amount.Value.Value : detail.Amount.Value.Value / detail.OrderQty.Value.Value;
			//	if (!String.IsNullOrEmpty(key.KeySplit(0, String.Empty))) product.ProductId = key.KeySplit(0, String.Empty).ToInt() ?? 0;
			//	//if (!String.IsNullOrEmpty(key.KeySplit(1, String.Empty))) product.VariandId = key.KeySplit(1, String.Empty).ToInt() ?? 0;
			//	if (!String.IsNullOrEmpty(key.KeySplit(1, String.Empty))) // add options
			//	{
			//		product.ProductOptions = new List<OrdersProductsOption>();
			//		foreach (KeyValuePair<Int32, String> pair in options)
			//		{
			//			product.ProductOptions.Add(new OrdersProductsOption() { Id = pair.Key, Value = pair.Value, });
			//		}
			//	}

			//	productData.Add(product);
			//}
			//orderData.OrderProducts = productData;
		}
		public override void SaveBucketExport(SPSalesOrderBucket bucket, IMappedEntity existing, String operation)
		{
			//	BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			//	MappedOrder obj = bucket.Order;

			//	OrderData data = null;
			//	if (obj.ExternID != null && existing != null && obj.Local.ExternalOrderOriginal?.Value == true) // Update status only if order is unmodified
			//	{
			//		data = new OrderData();
			//		data.StatusId = obj.Extern.StatusId;
			//		data = orderDataProvider.Update(data, obj.ExternID.ToInt().Value);
			//	}
			//	else
			//	{
			//		if (obj.ExternID == null || existing == null)
			//		{
			//			data = orderDataProvider.Create(obj.Extern);
			//		}
			//		else
			//		{
			//			data = orderDataProvider.Update(obj.Extern, obj.ExternID.ToInt().Value);
			//		}

			//		PX.Objects.IN.InventoryItem giftCertificate = bindingExt.GiftCertificateItemID != null ? PX.Objects.IN.InventoryItem.PK.Find(this, bindingExt.GiftCertificateItemID) : null;
			//		List<OrdersProductData> products = orderProductsRestDataProvider.Get(data.Id?.ToString()) ?? new List<OrdersProductData>();
			//		DetailInfo[] oldDetails = obj.Details.ToArray(); obj.ClearDetails();
			//		if (data.ShippingAddressCount > 0)
			//		{
			//			var ShippingAddresses = orderShippingAddressesRestDataProvider.Get(data.Id?.ToString()) ?? new List<OrdersShippingAddressData>();
			//			if (ShippingAddresses.Count > 0) obj.AddDetail(BCEntitiesAttribute.OrderAddress, obj.LocalID, ShippingAddresses.First().Id.ToString()); //Shipment ID detail
			//		}
			//		foreach (SalesOrderDetail detail in obj.Local.Details) //Line ID detail
			//		{
			//			OrdersProductData product = null;
			//			if (giftCertificate != null && detail.InventoryID?.Value?.Trim() == giftCertificate.InventoryCD?.Trim())
			//			{
			//				product = products.FirstOrDefault(x => x.ProductType == OrdersProductsType.GiftCertificate);
			//				if (product != null)
			//					obj.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, product.Id.ToString());
			//				continue;
			//			}

			//			product = products.FirstOrDefault(x => x.ProductId.ToString() == oldDetails.FirstOrDefault(o => o.LocalID == detail.NoteID.Value)?.ExternID);
			//			if (product == null)
			//			{
			//				String externalID = GetProductExternIDByProductCD(detail.InventoryID.Value, out Dictionary<Int32, String> options, out string sku);
			//				product = products.FirstOrDefault(x => !obj.Details.Any(o => x.Id.ToString() == o.ExternID)
			//					&& x.ProductId.ToString() == externalID.KeySplit(0) 
			//					&& (String.IsNullOrEmpty(externalID.KeySplit(1, String.Empty)) 
			//						|| x.VariandId.ToString() == externalID.KeySplit(1, String.Empty) 
			//						|| (!string.IsNullOrEmpty(sku) && x.Sku == sku)));
			//			}
			//			if (product != null)
			//			{
			//				obj.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, product.Id.ToString());
			//				continue;
			//			}
			//			throw new PXException(ShopifyMessages.CannotMapLines);
			//		}
			//	}
			//	obj.AddExtern(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
			//	UpdateStatus(obj, operation);
		}
		#endregion

		#region Methods


		//public static OrderStatuses ConvertStatus(String status)
		//{
		//	switch (status)
		//	{
		//		case PX.Objects.SO.Messages.BackOrder:
		//			return OrderStatuses.PartiallyShipped;
		//		case PX.Objects.SO.Messages.Cancelled:
		//			return OrderStatuses.Cancelled;
		//	case PX.Objects.SO.Messages.Completed:
		//			return OrderStatuses.Shipped;
		//		case PX.Objects.SO.Messages.CreditHold:
		//			return OrderStatuses.VerificationRequired;
		//		case PX.Objects.SO.Messages.Hold:
		//			return OrderStatuses.Pending;
		//		case PX.Objects.SO.Messages.Open:
		//			return OrderStatuses.AwaitingFulfillment;
		//		case PX.Objects.EP.Messages.Balanced:
		//			return OrderStatuses.VerificationRequired;
		//		case PX.Objects.SO.Messages.Shipping:
		//			return OrderStatuses.AwaitingShipment;
		//		case PX.Objects.EP.Messages.Voided:
		//			return OrderStatuses.Declined;
		//		case PX.Objects.SO.Messages.Invoiced:
		//			return OrderStatuses.AwaitingFulfillment;
		//		default:
		//			return OrderStatuses.Pending;
		//	}
		//}
		//protected virtual String GetProductExternIDByProductCD(String inventoryCD, out Dictionary<Int32, String> options, out string sku)
		//{
		//	sku = null;
		//	options = new Dictionary<Int32, String>();

		//	PX.Objects.IN.InventoryItem item = PXSelect<PX.Objects.IN.InventoryItem,
		//			Where<PX.Objects.IN.InventoryItem.inventoryCD, Equal<Required<PX.Objects.IN.InventoryItem.inventoryCD>>>>.Select(this, inventoryCD);
		//	if (item?.InventoryID == null) throw new PXException(ShopifyMessages.InvenotryNotSyncronized, inventoryCD);
		//	if (item?.ItemStatus == PX.Objects.IN.INItemStatus.Inactive) throw new PXException(ShopifyMessages.InvenotryInactive, inventoryCD);

		//	BCSyncStatus itemStatus = PXSelect<BCSyncStatus,
		//		Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
		//			And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
		//			And2<Where<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
		//				Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>,
		//			And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>>>
		//		.Select(this, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.StockItem, item.NoteID);

		//	Int32? productID = null, variantID = null;
		//	if (itemStatus?.ExternID == null && item.TemplateItemID != null) // Check for parent item - for variants
		//	{
		//		foreach (var status in PXSelectJoin<BCSyncStatus,
		//				InnerJoin<InventoryItem, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncStatus.localID>>,
		//				InnerJoin<BCSyncDetail, On<BCSyncDetail.syncID, Equal<BCSyncStatus.syncID>>>>,
		//				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
		//					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
		//					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
		//					And<BCSyncDetail.localID, Equal<Required<BCSyncDetail.localID>>>>>>>
		//				.Select(this, BCEntitiesAttribute.ProductWithVariant, item.NoteID))
		//		{
		//			BCSyncDetail variantStatus = status.GetItem<BCSyncDetail>();
		//			BCSyncStatus parentStatus = status.GetItem<BCSyncStatus>();
		//			if (variantStatus?.ExternID != null && parentStatus?.ExternID != null)
		//			{
		//				//TODO Need to Review - Search for options by variant ID thought API
		//				ProductsVariantData variant = productVariantsDataProvider.GetByID(variantStatus.ExternID, parentStatus.ExternID);

		//				if (variant.Sku == item.InventoryCD.Trim())
		//				{
		//					sku = variant.Sku;
		//					variantID = variant.Id;
		//					productID = variant.ProductId;							
		//					foreach (ProductVariantOptionValueData option in variant.OptionValues)
		//					{
		//						if (option.OptionId == null) continue;
		//						options[option.OptionId.Value] = option.Id.ToString();
		//					}
		//				}
		//			}
		//		}
		//	}
		//	else if (itemStatus?.ExternID != null) productID = itemStatus?.ExternID.ToInt();

		//	if(productID == null) //TODO Need to Review - If product not found search through API.
		//	{
		//		ProductData product = productDataProvider.Get(new FilterProducts() { SKU = inventoryCD }).FirstOrDefault();
		//		if(product != null)
		//		{
		//			sku = product.Sku;
		//			productID = product.Id;					
		//		}
		//	}

		//	if (productID == null) throw new PXException(ShopifyMessages.InvenotryNotSyncronized, inventoryCD);
		//	return variantID == null ? productID?.ToString() : new object[] { productID, variantID }.KeyCombine();
		//}


		#endregion
	}
}