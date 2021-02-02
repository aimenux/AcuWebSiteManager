using PX.Commerce.BigCommerce.API.REST;
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
using Serilog.Context;
using PX.Api.ContractBased.Models;

namespace PX.Commerce.BigCommerce
{
	public class BCSalesOrderBucket : EntityBucketBase, IEntityBucket
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

	public class BCSalesOrderRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			#region Orders
			return base.Restrict<MappedOrder>(mapped, delegate (MappedOrder obj)
			{
				if (obj.IsNew && obj.Local != null && obj.Local.Status != null)
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
				PX.Objects.IN.InventoryItem giftCertificate = bindingExt.GiftCertificateItemID != null ? PX.Objects.IN.InventoryItem.PK.Find((PXGraph)processor, bindingExt.GiftCertificateItemID) : null;
				if (giftCertificate != null && obj.Local?.Details?.Count() > 0)
				{
					bool isgiftItem = obj.Local.Details.All(x => x.InventoryID?.Value?.Trim() == giftCertificate.InventoryCD.Trim());
					if (isgiftItem)
					{
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedGiftCert, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString(), obj.Local.Status.Value));
					}
				}

				//var guestID = processor.GetBindingExt<BCBindingExt>().GuestCustomerID;
				var guestID = PX.Objects.AR.Customer.PK.Find((PXGraph)processor, bindingExt.GuestCustomerID)?.AcctCD;
				if (!String.IsNullOrEmpty(obj.Local?.CustomerID?.Value.Trim()) && guestID != null && guestID.Trim().Equals(obj.Local?.CustomerID?.Value.Trim()))
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedGuestOrder, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString()));
				}
				if (obj.IsNew && !String.IsNullOrEmpty(obj.Local?.ExternalRef?.Value?.Trim()))
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
				if (obj.IsNew && obj.Extern != null
					&& (obj.Extern.StatusId == OrderStatuses.Shipped.GetHashCode()
						|| obj.Extern.StatusId == OrderStatuses.Cancelled.GetHashCode()
						|| obj.Extern.StatusId == OrderStatuses.Declined.GetHashCode()))
				{
					return new FilterResult(FilterStatus.Filtered,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedExtStatusNotSupported, obj.Extern.Id, obj.Extern.Status.ToString()));
				}
				if (obj.IsNew && obj.Extern != null
					&& (obj.Extern.StatusId == OrderStatuses.Incomplete.GetHashCode()
						|| obj.Extern.StatusId == OrderStatuses.Pending.GetHashCode()
						|| obj.Extern.StatusId == OrderStatuses.VerificationRequired.GetHashCode()))
				{
					return new FilterResult(FilterStatus.Filtered,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedExtStatusNotSupported, obj.Extern.Id, obj.Extern.Status.ToString()));
				}
				if (obj.IsNew && obj.Extern != null && obj.Extern.ShippingAddressCount > 1)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedExtMultipleShipments, obj.Extern.Id));
				}

				return null;
			});
			#endregion
		}
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.Order, BCCaptions.Order,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.SO.SOOrderEntry),
		ExternTypes = new Type[] { typeof(OrderData) },
		LocalTypes = new Type[] { typeof(SalesOrder) },
		DetailTypes = new String[] {BCEntitiesAttribute.OrderLine, BCCaptions.OrderLine, BCEntitiesAttribute.OrderAddress, BCCaptions.OrderAddress },
		AcumaticaPrimaryType = typeof(PX.Objects.SO.SOOrder),
		AcumaticaPrimarySelect = typeof(Search2<PX.Objects.SO.SOOrder.orderNbr,
			InnerJoin<BCBindingExt, On<BCBindingExt.orderType, Equal<SOOrder.orderType>>,
			InnerJoin<BCBinding, On<BCBindingExt.bindingID, Equal<BCBinding.bindingID>>>>,
			Where<BCBinding.connectorType, Equal<Current<BCSyncStatusEdit.connectorType>>,
				And<BCBinding.bindingID, Equal<Current<BCSyncStatusEdit.bindingID>>>>>),
		URL = "orders?keywords={0}&searchDeletedOrders=no",
		Requires = new string[] { BCEntitiesAttribute.Customer },
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.NonStockItem }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = true,
		PushSources = new String[] { "BC-PUSH-Orders" },
		WebHookType = typeof(WebHookOrder),
		WebHooks = new String[]
		{
			"store/order/created",
			"store/order/updated",
			//"store/order/archived",
			//"store/order/statusUpdated",
			"store/order/message/created"
		})]
	public class BCSalesOrderProcessor : BCProcessorSingleBase<BCSalesOrderProcessor, BCSalesOrderBucket, MappedOrder>, IProcessor
	{
		public BCPaymentProcessor paymentProcessor = PXGraph.CreateInstance<BCPaymentProcessor>();

		protected OrderRestDataProvider orderDataProvider;
		protected CustomerRestDataProviderV3 customerDataProviderV3;
		protected IChildRestDataProvider<OrdersProductData> orderProductsRestDataProvider;
		protected IChildRestDataProvider<OrdersShippingAddressData> orderShippingAddressesRestDataProvider;
		protected IChildRestDataProvider<OrdersCouponData> orderCouponsRestDataProvider;
		protected IChildRestDataProvider<OrdersShipmentData> orderShipmentsDataProvider;
		protected IChildReadOnlyRestDataProvider<OrdersTransactionData> orderTransactionsRestDataProvider;
		protected IChildReadOnlyRestDataProvider<OrdersTaxData> orderTaxesRestDataProvider;
		protected IChildRestDataProvider<ProductsVariantData> productVariantsDataProvider;
		protected ProductRestDataProvider productDataProvider;

		protected TaxDataProvider taxDataProvider;
		protected List<ProductsTaxData> taxClasses;
		protected List<BCShippingMappings> shippingMappings;
		protected List<BCPaymentMethods> paymentMethods;

		protected Dictionary<int, string> prodClasses = new Dictionary<int, string>();

		public PXSelect<State, Where<State.name, Equal<Required<State.name>>, Or<State.stateID, Equal<Required<State.stateID>>>>> states;

		#region Initialization
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			productDataProvider = new ProductRestDataProvider(client);
			orderDataProvider = new OrderRestDataProvider(client);
			orderProductsRestDataProvider = new OrderProductsRestDataProvider(client);
			orderShippingAddressesRestDataProvider = new OrderShippingAddressesRestDataProvider(client);
			orderCouponsRestDataProvider = new OrderCouponsRestDataProvider(client);
			orderShipmentsDataProvider = new OrderShipmentsRestDataProvider(client);
			orderTransactionsRestDataProvider = new OrderTransactionsRestDataProvider(client);
			orderTaxesRestDataProvider = new OrderTaxesRestDataProvider(client);
			customerDataProviderV3 = new CustomerRestDataProviderV3(client);
			productVariantsDataProvider = new ProductVariantRestDataProvider(client);
			taxDataProvider = new TaxDataProvider(client);

			shippingMappings = PXSelectReadonly<BCShippingMappings,
				Where<BCShippingMappings.bindingID, Equal<Required<BCShippingMappings.bindingID>>>>
				.Select(this, Operation.Binding).Select(x => x.GetItem<BCShippingMappings>()).ToList();
			paymentMethods = PXSelectReadonly<BCPaymentMethods,
				Where<BCPaymentMethods.bindingID, Equal<Required<BCPaymentMethods.bindingID>>>>
				.Select(this, Operation.Binding).Select(x => x.GetItem<BCPaymentMethods>()).ToList();
			taxClasses = taxDataProvider.GetAll();

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

			MappedOrder obj = new MappedOrder(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());

			return obj;
		}

		public override IEnumerable<MappedOrder> PullSimilar(IExternEntity entity, out string uniqueField)
		{
			uniqueField = ((OrderData)entity)?.Id?.ToString();
			if (string.IsNullOrEmpty(uniqueField))
				return null;
			uniqueField = APIHelper.ReferenceMake(uniqueField, GetBinding().BindingName);

			SalesOrder[] impls = cbapi.GetAll<SalesOrder>(new SalesOrder() { OrderType = GetBindingExt<BCBindingExt>().OrderType.SearchField(), ExternalRef = uniqueField.SearchField() },
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
			uniqueField = APIHelper.ReferenceParse(uniqueField, GetBinding().BindingName);

			OrderData data = orderDataProvider.GetByID(uniqueField);
			if (data == null || data.IsDeleted == true) return null;

			List<MappedOrder> result = new List<MappedOrder>();
			result.Add(new MappedOrder(data, data.Id.ToString(), data.DateModifiedUT.ToDate()));
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
                    if (order.Extern.ItemsTotal == order.Extern.ItemsShipped)
                    {
                        DateTime? orderdate = order.Extern.DateModifiedUT.ToDate();
                        DateTime? shipmentDate = order.Extern.DateShippedUT.ToDate();

                        if (orderdate != null && shipmentDate != null && Math.Abs((orderdate - shipmentDate).Value.TotalSeconds) < 5) //Modification withing 5 sec
                            return false;
                    }
                    if (order.Extern.ItemsShipped > 0 && order.Extern.ItemsShipped < order.Extern.ItemsTotal)
                    {
                        //TODO Need review. Calling shipments API here may lead to bad fetch performance
                        foreach (OrdersShipmentData shipment in orderShipmentsDataProvider.Get(order.ExternID) ?? new List<OrdersShipmentData>())
                        {
                            DateTime? orderdate = order.Extern.DateModifiedUT.ToDate();
                            DateTime? shipmentDate = shipment.DateCreatedUT.ToDate();

                            if (orderdate != null && shipmentDate != null && Math.Abs((orderdate - shipmentDate).Value.TotalSeconds) < 5) //Modification withing 5 sec
                                return false;
                        }
                    }
                }
            }

			return base.ControlModification(mapped, status, operation);
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterOrders filter = new FilterOrders { IsDeleted = "false", };
			if (minDateTime != null) filter.MinDateModified = minDateTime;
			if (maxDateTime != null) filter.MaxDateModified = maxDateTime;

			IEnumerable<OrderData> datas = orderDataProvider.GetAll(filter);

			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			foreach (OrderData data in datas)
			{
				IMappedEntity obj = new MappedOrder(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());

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
		public override EntityStatus GetBucketForImport(BCSalesOrderBucket bucket, BCSyncStatus syncstatus)
		{
			OrderData data = orderDataProvider.GetByID(syncstatus.ExternID);
			if (data == null || data.IsDeleted == true) return EntityStatus.None;

			data.OrderProducts = orderProductsRestDataProvider.Get(syncstatus.ExternID) ?? new List<OrdersProductData>();
			data.Shipments = orderShipmentsDataProvider.Get(syncstatus.ExternID) ?? new List<OrdersShipmentData>();
			data.OrdersCoupons = orderCouponsRestDataProvider.Get(syncstatus.ExternID) ?? new List<OrdersCouponData>();
			data.Taxes = orderTaxesRestDataProvider.Get(syncstatus.ExternID) ?? new List<OrdersTaxData>();
			data.OrderShippingAddresses = orderShippingAddressesRestDataProvider.Get(syncstatus.ExternID) ?? new List<OrdersShippingAddressData>();
			data.Transactions = orderTransactionsRestDataProvider.Get(syncstatus.ExternID) ?? new List<OrdersTransactionData>();
			FilterCustomers filter = new FilterCustomers { Include = "addresses", Id = data.CustomerId?.ToString() };
			data.Customer = data.CustomerId != 0 ? customerDataProviderV3.GetAll(filter).FirstOrDefault() : null;

			MappedOrder obj = bucket.Order = bucket.Order.Set(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
           
			if (status != EntityStatus.Pending && status != EntityStatus.Syncronized)
                return status;

            if (data.Customer != null && data.CustomerId > 0)
			{
				MappedCustomer customerObj = bucket.Customer = bucket.Customer.Set(data.Customer, data.Customer.Id?.ToString(), data.Customer.DateModifiedUT.ToDate());
				EntityStatus customerStatus = EnsureStatus(customerObj);

				//Find proper location by all fields. Unfortunately BC does not provide ID of existing address with order;
				CustomerAddressData address = null;
				foreach (OrdersShippingAddressData shipAddr in data.OrderShippingAddresses)
				{
					foreach (CustomerAddressData custAddr in data.Customer.Addresses)
					{
						if (custAddr.City == shipAddr.City
							&& custAddr.Company == shipAddr.Company
							&& custAddr.CountryCode == shipAddr.CountryIso2
							&& custAddr.FirstName == shipAddr.FirstName
							&& custAddr.LastName == shipAddr.LastName
							&& custAddr.Phone == shipAddr.Phone
							&& custAddr.State == shipAddr.State
							&& custAddr.Address1 == shipAddr.Street1
							&& custAddr.Address2 == shipAddr.Street2
							&& custAddr.PostalCode == shipAddr.ZipCode)
						{
							address = custAddr;
							break;
						}
					}
				}
				if (address != null)
				{
					bucket.Location = bucket.Location.Set(address, new Object[] { address.CustomerId, address.Id }.KeyCombine(), address.CalculateHash()).With(_ => { _.ParentID = customerObj.SyncID; return _; });
				}
				else LogWarning(Operation.LogScope(syncstatus), BCMessages.LogOrderLocationUnidentified, data.Id);
			}

			if (GetEntity(BCEntitiesAttribute.Payment)?.IsActive == true)
			{
				foreach (OrdersTransactionData tranData in data.Transactions)
				{
					BCSyncStatus paymentSyncStatus = BCSyncStatus.ExternIDIndex.Find(this, Operation.ConnectorType, Operation.Binding, BCEntitiesAttribute.Payment, new Object[] { data.Id, tranData.Id }.KeyCombine());
                    if (paymentSyncStatus != null)
                    {
                        OrdersTransactionData lastPayment = data.Transactions.LastOrDefault(x => x.Gateway == tranData.Gateway && x.Status != OrderPaymentStatus.Error);
                        if (lastPayment != null && tranData.Event != lastPayment.Event) //Evaluate last transaction event
                            tranData.Event = lastPayment.Event;
                        else if (paymentSyncStatus.LocalID != null && paymentSyncStatus.PendingSync != true)
                            continue;
                    }
                    tranData.OrderPaymentMethod = data.PaymentMethod;
                    MappedPayment paymentObj = new MappedPayment(tranData, new Object[] { data.Id, tranData.Id }.KeyCombine(), tranData.DateCreatedUT.ToDate(), tranData.CalculateHash()).With(_ => { _.ParentID = obj.SyncID; return _; });
                    EntityStatus paymentStatus = EnsureStatus(paymentObj, SyncDirection.Import);
                    if (paymentStatus == EntityStatus.Pending)
                    {
                        bucket.Payments.Add(paymentObj);
                    }
				}
				if (CreatePaymentfromOrder(data.PaymentMethod))
				{
					BCSyncStatus paymentSyncStatus = BCSyncStatus.ExternIDIndex.Find(this, Operation.ConnectorType, Operation.Binding, BCEntitiesAttribute.Payment, new Object[] { data.Id }.KeyCombine());
					if (paymentSyncStatus?.LocalID == null)
					{
						OrdersTransactionData transdata = new OrdersTransactionData();

						transdata.Id = data.Id.Value;
						transdata.OrderId = data.Id.ToString();
						transdata.Gateway = data.PaymentMethod;
						transdata.Currency = data.CurrencyCode;
						transdata.DateCreatedUT = data.DateModifiedUT;
						transdata.Amount = Convert.ToDouble(data.TotalIncludingTax);
						transdata.PaymentMethod = BCConstants.Emulated;
						MappedPayment paymentObj = new MappedPayment(transdata, data.Id.ToString(), data.DateModifiedUT.ToDate(), transdata.CalculateHash());
						EntityStatus paymentStatus = EnsureStatus(paymentObj, SyncDirection.Import);

						if (paymentStatus == EntityStatus.Pending)
						{
							bucket.Payments.Add(paymentObj);
						}
					}
				}
			}

			return status;
		}

		public override void MapBucketImport(BCSalesOrderBucket bucket, IMappedEntity existing)
		{
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			MappedOrder obj = bucket.Order;

			OrderData data = obj.Extern;
			SalesOrder impl = obj.Local = new SalesOrder();
			SalesOrder presented = existing?.Local as SalesOrder;
			// we can update only open orders
			if (presented != null && presented.Status?.Value != "Open" && presented.Status?.Value != "On Hold")
			{
				throw new PXException(BCMessages.OrderStatusDoesNotAllowModification, presented.OrderNbr?.Value);
			}

			impl.Custom = GetCustomFieldsForImport();

			#region SalesOrder

			impl.OrderType = bindingExt.OrderType.ValueField();
			var date = data.DateCreatedUT.ToDate(PXTimeZoneInfo.FindSystemTimeZoneById(bindingExt.OrderTimeZone));
			if (date.HasValue)
				impl.Date = (new DateTime(date.Value.Date.Ticks)).ValueField();
			impl.RequestedOn = impl.Date;
			impl.Description = String.Format("BigCommerce Order {0}", data.Id).ValueField();
			impl.CurrencyID = data.CurrencyCode.ValueField();
			impl.CurrencyRate = data.CurrencyExchangeRate.ValueField();
			impl.ExternalRef = APIHelper.ReferenceMake(data.Id, binding.BindingName).ValueField();
			String note = String.Empty;
			if (!String.IsNullOrEmpty(data.CustomerMessage)) note = String.Concat("Customer Notes:", Environment.NewLine, data.CustomerMessage);
			if (!String.IsNullOrEmpty(data.CustomerMessage) && !String.IsNullOrEmpty(data.StaffNotes)) note += String.Concat(Environment.NewLine, Environment.NewLine);
			if (!String.IsNullOrEmpty(data.StaffNotes)) note += String.Concat("Staff Notes:", Environment.NewLine, data.StaffNotes);
			impl.Note = String.IsNullOrWhiteSpace(note) ? null : note;
			impl.ExternalOrderOriginal = true.ValueField();

			PX.Objects.AR.Customer customer;
			//Customer ID
			if (bucket.Customer != null)
			{
				customer = PXSelectJoin<PX.Objects.AR.Customer,
					LeftJoin<BCSyncStatus, On<PX.Objects.AR.Customer.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, BCEntitiesAttribute.Customer, data.CustomerId);
				if (customer == null) throw new PXException(BCMessages.CustomerNotSyncronized, data.CustomerId);
				if (customer.CuryID != impl.CurrencyID.Value && !customer.AllowOverrideCury.Value) throw new PXException(BCMessages.OrderCurrencyNotMathced, impl.CurrencyID.Value, customer.CuryID);
				impl.CustomerID = customer.AcctCD?.Trim().ValueField();
			}
			else
			{
				customer = PXSelectJoin<PX.Objects.AR.Customer,
					LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defContactID, Equal<PX.Objects.CR.Contact.contactID>>>,
					Where<PX.Objects.CR.Contact.eMail, Equal<Required<PX.Objects.CR.Contact.eMail>>>>.Select(this, data.BillingAddress.Email);
				if (customer == null)
				{
					customer = PX.Objects.AR.Customer.PK.Find(this, bindingExt.GuestCustomerID);
					if (customer == null) throw new PXException(BigCommerceMessages.NoGuestCustomer);
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

			//FinancialSettings
			impl.FinancialSettings = new FinancialSettings();
			impl.FinancialSettings.Branch = Branch.PK.Find(this, binding.BranchID)?.BranchCD?.ValueField();
			#endregion

			#region ShippingSettings
			//Freight
			impl.Totals = new Totals();
			impl.Totals.OverrideFreightAmount = new BooleanValue() { Value = true };
			impl.Totals.Freight = (data.BaseShippingCost + data.HandlingCostExcludingTax).ValueField();

			State state;
			//ShippingSettings
			impl.ShippingSettings = new ShippingSettings();
			PXCache cache = base.Caches[typeof(BCShippingMappings)];
			//Ship-To Address && Contact
			foreach (OrdersShippingAddressData shippingAddress in data.OrderShippingAddresses)
			{
				bool found = false;
				//Search for mapping value
				foreach (BCShippingMappings mapping in shippingMappings)
				{
					if (mapping.ShippingMethod == null || shippingAddress.ShippingMethod == null) continue;
					if (mapping.ShippingZone != shippingAddress.ShippingZoneName) continue;

					String shippingMethod = mapping.ShippingMethod.Replace(".", "").Replace("_", "").Replace("-", "");
					String shippingMethodPart1 = shippingMethod.FieldsSplit(0, String.Empty, BCConstants.Separator).Replace(" ", "").Trim();
					String shippingMethodPart2 = shippingMethod.FieldsSplit(1, String.Empty, BCConstants.Separator).Replace(" ", "").Trim();

					String target = shippingAddress.ShippingMethod.Replace(".", "").Replace("_", "").Replace("-", "").Replace(" ", "");

					if (target.Contains(shippingMethodPart1, StringComparison.InvariantCultureIgnoreCase)
						&& (String.IsNullOrEmpty(shippingMethodPart2) || target.Contains(shippingMethodPart2, StringComparison.InvariantCultureIgnoreCase)))
					{
						found = true;
						if (mapping.CarrierID == null && mapping.ZoneID == null && mapping.ShipTermsID == null) continue;
						impl.ShipVia = impl.ShippingSettings.ShipVia = mapping.CarrierID.ValueField();
						impl.ShippingSettings.ShippingZone = mapping.ZoneID.ValueField();
						impl.ShippingSettings.ShippingTerms = mapping.ShipTermsID.ValueField();

						break;
					}
				}
				if (!found && !shippingMappings.Any(x => x.ShippingZone == shippingAddress.ShippingZoneName && x.ShippingMethod == shippingAddress.ShippingMethod))
				{
					BCShippingMappings inserted = new BCShippingMappings() { BindingID = Operation.Binding, ShippingZone = shippingAddress.ShippingZoneName, ShippingMethod = shippingAddress.ShippingMethod };
					cache.Insert(inserted);
				}

				impl.ShipToAddressOverride = true.ValueField();
				impl.ShipToAddress = new Address();
				impl.ShipToAddress.AddressLine1 = shippingAddress.Street1.ValueField();
				impl.ShipToAddress.AddressLine2 = shippingAddress.Street2.ValueField();
				impl.ShipToAddress.City = shippingAddress.City.ValueField();
				impl.ShipToAddress.Country = shippingAddress.CountryIso2.ValueField();
				if (!string.IsNullOrEmpty(shippingAddress.State))
				{
					state = states.Select(shippingAddress.State, shippingAddress.State);
					if (state == null)
						impl.ShipToAddress.State = GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), shippingAddress.State, shippingAddress.State).ValueField();
					else
						impl.ShipToAddress.State = state.StateID?.ValueField();
				}
				else
					impl.ShipToAddress.State = string.Empty.ValueField();
				impl.ShipToAddress.PostalCode = shippingAddress.ZipCode?.ToUpperInvariant()?.ValueField();

				impl.ShipToContactOverride = true.ValueField();
				impl.ShipToContact = new DocContact();
				impl.ShipToContact.Phone1 = shippingAddress.Phone.ValueField();
				impl.ShipToContact.Email = shippingAddress.Email.ValueField();
				var attentionShip = shippingAddress.FirstName.Trim();
				if (shippingAddress.LastName.Trim().ToLower() != attentionShip.ToLower()) attentionShip += " " + shippingAddress.LastName.Trim();
				impl.ShipToContact.Attention = attentionShip.ValueField();
				impl.ShipToContact.BusinessName = shippingAddress.Company.ValueField();

				break;
			}
			if (cache.Inserted.Count() > 0)
				cache.Persist(PXDBOperation.Insert);
			//Bill-To Address && Contact
			impl.BillToAddressOverride = true.ValueField();
			impl.BillToAddress = new Address();
			impl.BillToAddress.AddressLine1 = data.BillingAddress.Street1.ValueField();
			impl.BillToAddress.AddressLine2 = data.BillingAddress.Street2.ValueField();
			impl.BillToAddress.City = data.BillingAddress.City.ValueField();
			impl.BillToAddress.Country = data.BillingAddress.CountryIso2.ValueField();
			if (!string.IsNullOrEmpty(data.BillingAddress.State))
			{
				state = states.Select(data.BillingAddress.State, data.BillingAddress.State);
				if (state == null)
				{
					impl.BillToAddress.State = GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), data.BillingAddress.State, data.BillingAddress.State).ValueField();
				}
				else
					impl.BillToAddress.State = state.StateID?.ValueField();
			}
			else
				impl.BillToAddress.State = string.Empty.ValueField();
			impl.BillToAddress.PostalCode = data.BillingAddress.ZipCode?.ToUpperInvariant()?.ValueField();

			impl.BillToContactOverride = true.ValueField();
			impl.BillToContact = new DocContact();
			impl.BillToContact.Phone1 = data.BillingAddress.Phone.ValueField();
			impl.BillToContact.Email = data.BillingAddress.Email.ValueField();
			impl.BillToContact.BusinessName = data.BillingAddress.Company.ValueField();
			var attentionBill = data.BillingAddress.FirstName.Trim();
			if (data.BillingAddress.LastName.Trim().ToLower() != attentionBill.ToLower()) attentionBill += " " + data.BillingAddress.LastName.Trim();
			impl.BillToContact.Attention = attentionBill.ValueField();
			#endregion

			#region Products

			//Products
			impl.Details = new List<SalesOrderDetail>();
			foreach (OrdersProductData productData in data.OrderProducts)
			{
				//if (productData.ProductId <= 0) continue; //product that does not exists in big Commerce
				if (productData.Quantity == 0) continue;

				String inventoryCD = GetInventoryCDByExternID(
					productData.ProductId.ToString(),
					productData.OptionSetId >= 0 ? productData.VariandId.ToString() : null,
					productData.Sku,
					productData.ProductType,
					out string uom);

				SalesOrderDetail detail = new SalesOrderDetail();
				detail.Branch = impl.Branch = impl.FinancialSettings.Branch;
				detail.InventoryID = inventoryCD?.TrimEnd().ValueField();
				detail.OrderQty = ((decimal?)productData.Quantity).ValueField();
				detail.UOM = uom.ValueField();
				detail.UnitPrice = productData.BasePrice.ValueField();
				detail.LineDescription = productData.ProductName.ValueField();
				detail.ExtendedPrice = productData.TotalExcludingTax.ValueField();
				detail.FreeItem = (productData.TotalExcludingTax == 0m && productData.BasePrice == 0m).ValueField();
				detail.ManualPrice = true.ValueField();
				if (bindingExt.PostDiscounts == BCPostDiscountAttribute.LineDiscount && productData.AppliedDiscounts != null)
					detail.DiscountAmount = productData.AppliedDiscounts.Select(p => p.DiscountAmount).Sum().ValueField();
				else detail.DiscountAmount = 0m.ValueField();

				//Check for existing
				if (existing?.Details?.Count() > 0)
				{
					existing?.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && productData.Id.ToString() == d.ExternID).With(d => detail.Id = d.LocalID);
				}
				else if (existing?.Details?.Count() == 0 && presented != null && presented.Details?.Count > 0)
				{
					presented.Details.FirstOrDefault(x => x.InventoryID.Value == detail.InventoryID.Value).With(e => detail.Id = e.Id);
				}

				impl.Details.Add(detail);
			}
			#endregion

			#region Taxes
			//Insert Taxes if Importing Them
			impl.TaxDetails = new List<TaxDetail>();
			if (!(data.Taxes.Count == 1 && data.Taxes.First().Class.Equals(BCObjectsConstants.AutomaticTax)))
			{
				if (bindingExt.SyncTaxes == BCTaxSyncAttribute.ManualTaxes || bindingExt.SyncTaxes == BCTaxSyncAttribute.AutomaticTaxes)
				{
					impl.IsTaxValid = true.ValueField();
					foreach (OrdersTaxData tax in data.Taxes)
					{
						//Third parameter set to tax name in order to simplify process (if tax names are equal and user don't want to fill lists)
						String mappedTaxName = mappedTaxName = GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxSubstitutionListID, tax.Name, tax.Name);
						mappedTaxName = TrimAutomaticTaxNameForAvalara(mappedTaxName);
						if (String.IsNullOrEmpty(mappedTaxName)) throw new PXException(PX.Commerce.Objects.BCObjectsMessages.TaxNameDoesntExist);

						TaxDetail inserted = impl.TaxDetails.FirstOrDefault(i => i.TaxID.Value?.Equals(mappedTaxName, StringComparison.InvariantCultureIgnoreCase) == true);
						//We are deciding on taxable to be updated accordinly only if tax amount is greated then 0. AC-169616
						decimal taxable = tax.LineAmount > 0?  data.OrderProducts.FirstOrDefault(i => i.Id == tax.OrderProductId)?.TotalExcludingTax ?? 0 : 0;

						if (inserted == null)
						{
							impl.TaxDetails.Add(new TaxDetail()
							{
								TaxID = mappedTaxName.ValueField(),
								TaxAmount = tax.LineAmount.ValueField(),
								TaxRate = tax.Rate.ValueField(),
								TaxableAmount = taxable.ValueField()
							});
						}
						else if (inserted.TaxAmount != null)
						{
							inserted.TaxAmount.Value += tax.LineAmount;
							inserted.TaxableAmount.Value += taxable;
						}
					}
				}
			}
			if (data.Taxes.Count == 1 && data.Taxes.First().Class.Equals(BCObjectsConstants.AutomaticTax))
				impl.IsTaxValid = false.ValueField();

			//Check for tax Ids with more than 30 characters
			String[] tooLongTaxIDs = ((impl.TaxDetails ?? new List<TaxDetail>()).Select(x => x.TaxID?.Value).Where(x => (x?.Length ?? 0) > PX.Objects.TX.Tax.taxID.Length).ToArray());
			if(tooLongTaxIDs != null && tooLongTaxIDs.Length > 0)
			{
				throw new PXException(PX.Commerce.Objects.BCObjectsMessages.CannotFindSaveTaxIDs, String.Join(",", tooLongTaxIDs), PX.Objects.TX.Tax.taxID.Length);
			}	

			//TaxZone if Automatic tax mode is on
			if (bindingExt.SyncTaxes != BCTaxSyncAttribute.NoSync && bindingExt.PrimaryTaxZoneID != null)
			{
				impl.FinancialSettings.OverrideTaxZone = true.ValueField();
				impl.FinancialSettings.CustomerTaxZone = bindingExt.PrimaryTaxZoneID.ValueField();
			}
			#endregion

			#region Coupons

			//Coupons 
			impl.DisableAutomaticDiscountUpdate = true.ValueField();
			impl.DiscountDetails = new List<SalesOrdersDiscountDetails>();
			foreach (OrdersCouponData couponData in data.OrdersCoupons)
			{
				SalesOrdersDiscountDetails detail = new SalesOrdersDiscountDetails();
				detail.ExternalDiscountCode = couponData.CouponCode.ValueField();
				detail.Description = string.Format(BCMessages.DiscountCouponDesctiption, couponData.CouponType.GetDescription(), couponData.Discount)?.ValueField();
				if (couponData.CouponType == OrdersCouponType.FreeShipping)
				{
					impl.Totals.Freight = 0m.ValueField();
					detail.DiscountAmount = 0m.ValueField();
				}
				else if (couponData.CouponType == OrdersCouponType.DollarAmountOffShippingTotal)
				{
					impl.Totals.Freight.Value -= couponData.CouponAmount;
					if (impl.Totals.Freight.Value < 0)
						throw new PXException(BCMessages.ValueCannotBeLessThenZero, couponData.CouponAmount, impl.Totals.Freight.Value);
				}
				else
				{
					if (bindingExt.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount)
						detail.DiscountAmount = couponData.Discount.ValueField();
					else detail.DiscountAmount = 0m.ValueField();

				}
				impl.DiscountDetails.Add(detail);
			}
			#endregion

			#region Discounts

			//Discounts
			if (bindingExt.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount && data.DiscountAmount > 0)
			{
				SalesOrdersDiscountDetails detail = new SalesOrdersDiscountDetails();
				detail.DiscountAmount = data.DiscountAmount.ValueField();
				detail.ExternalDiscountCode = "Manual".ValueField();
				String[] discounts = data.OrderProducts.SelectMany(p => p.AppliedDiscounts).Where(d => d != null && d.Id != "coupon").Select(d => d.Id).Distinct().ToArray();
				detail.Description = string.Format("Discounts applied: {0}", String.Join(", ", discounts)).ValueField();

				impl.DiscountDetails.Add(detail);
			}
			#endregion

			#region Payment
			if (existing == null && !paymentProcessor.ImportMappings.Select().Any())
			{
				impl.Payments = new List<SalesOrderPayment>();
				foreach (MappedPayment payment in bucket.Payments)
				{
					OrdersTransactionData dataPayment = payment.Extern;
					SalesOrderPayment implPament = new SalesOrderPayment();
                    if (!payment.IsNew)
                        continue;
                    implPament.ExternalID = payment.ExternID;

					//Product
					implPament.DocType = PX.Objects.AR.Messages.Prepayment.ValueField();
					implPament.CurrencyID = impl.CurrencyID;
					implPament.ApplicationDate = dataPayment.DateCreatedUT.ToDate(PXTimeZoneInfo.FindSystemTimeZoneById(bindingExt.OrderTimeZone)).ValueField();
					implPament.PaymentAmount = ((decimal)dataPayment.Amount).ValueField();
					implPament.PaymentRef = (dataPayment.GatewayTransactionId ?? (dataPayment.ReferenceTransactionId == null ? data.Id.ToString() : dataPayment.ReferenceTransactionId.ToString())).ValueField();
					implPament.Hold = false.ValueField();
					implPament.AppliedToOrder = ((decimal)dataPayment.Amount).ValueField();

					PX.Objects.AR.ARPayment existingPayment = PXSelect<PX.Objects.AR.ARPayment,
						Where<PX.Objects.AR.ARPayment.extRefNbr, Equal<Required<PX.Objects.AR.ARPayment.extRefNbr>>>>.Select(this, implPament.PaymentRef.Value);
					if (existingPayment != null) continue; //skip if payment with same ref nbr exists already.

					BCPaymentMethods methodMapping = paymentProcessor.GetPaymentMethodMapping(paymentProcessor.GetPaymentMethodName(dataPayment), data.PaymentMethod, dataPayment.Currency, 
						out string paymentMethod, out string cashAcount, out string processingCenter);
					if (methodMapping.ReleasePayments ?? false) continue; //don't save payment with the order if the require release.

					implPament.PaymentMethod = paymentMethod?.ValueField();
					implPament.CashAccount = cashAcount?.Trim()?.ValueField();
					if (methodMapping.StorePaymentMethod == BCObjectsConstants.GiftCertificateCode)
						implPament.Description = string.Format("{0}; Payment Method: {1}({4}); Order: {2}; Payment ID: {3}", BCConnector.NAME, methodMapping.StorePaymentMethod, data.Id, dataPayment.Id, dataPayment.GiftCertificate?.Code).ValueField();
					else
						implPament.Description = string.Format("{0}; Payment Method: {1}; Order: {2}; Payment ID: {3}", BCConnector.NAME, methodMapping.StorePaymentMethod, data.Id, dataPayment.Id).ValueField();

					//Credit Card:
					if (dataPayment.GatewayTransactionId != null && processingCenter != null)
					{
						String paymentTran = paymentProcessor.ParceTransactionNumber(dataPayment.GatewayTransactionId);

						//implPament.IsNewCard = true.ValueField();
						implPament.SaveCard = (!String.IsNullOrWhiteSpace(dataPayment.PaymentInstrumentToken)).ValueField();
						implPament.ProcessingCenterID = processingCenter.ValueField();

						SalesOrderCreditCardTransactionDetail creditCardDetail = new SalesOrderCreditCardTransactionDetail();
						creditCardDetail.TranNbr = paymentTran.ValueField();
						creditCardDetail.TranDate = implPament.ApplicationDate;
						creditCardDetail.ExtProfileId = dataPayment.PaymentInstrumentToken.ValueField();
						switch (dataPayment.Event)
						{
							case OrderPaymentEvent.Authorization:
								creditCardDetail.TranType = "AUT".ValueField();
								break;
							case OrderPaymentEvent.Capture:
								creditCardDetail.TranType = "PAC".ValueField();
								break;
							case OrderPaymentEvent.Purchase:
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

		public virtual bool CreatePaymentfromOrder(string method)
		{
			var paymentMethod = paymentMethods.FirstOrDefault(x => String.Equals(x.StorePaymentMethod, method, StringComparison.InvariantCultureIgnoreCase) && x.CreatePaymentFromOrder == true && x.Active == true);
			return (paymentMethod != null);
		}

		public virtual string TrimAutomaticTaxNameForAvalara(string mappedTaxName)
		{
			return mappedTaxName.Split(new string[] { " - " }, StringSplitOptions.None).FirstOrDefault() ?? mappedTaxName;
		}

		public override void SaveBucketImport(BCSalesOrderBucket bucket, IMappedEntity existing, String operation)
		{
			MappedOrder obj = bucket.Order;
			SalesOrder local = obj.Local;
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			SalesOrder impl; //Due to the saving issue we should delete all needed order lines first with the separate calls.
			if (obj.Local.Details.Any(x => x.Delete) && obj.Local.Details.Any(x => !x.Delete))
			{
				List<SalesOrderDetail> updatedLines = obj.Local.Details.Where(x => x.Delete == false).ToList();

				obj.Local.Details = obj.Local.Details.Where(x => x.Delete).ToList(); // only deleted lines
				impl = cbapi.Put<SalesOrder>(obj.Local, obj.LocalID);

				obj.Local.Details = updatedLines.ToList();
			}

			#region Taxes
			//Logging for taxes
			if ((bindingExt.SyncTaxes != BCTaxSyncAttribute.NoSync) && (obj.Local.IsTaxValid?.Value == true))
			{
				String sentTaxes = String.Join("; ", obj.Local.TaxDetails?.Select(x => String.Join("=", x.TaxID?.Value, x.TaxAmount?.Value)).ToArray() ?? new String[] { BCConstants.None });
				this.LogInfo(Operation.LogScope(obj), BCMessages.LogTaxesOnOrderSent,
					obj.Local.OrderNbr?.Value ?? BCConstants.None,
					obj.Local.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None,
					String.IsNullOrEmpty(sentTaxes) ? BCConstants.None : sentTaxes);
			}
			#endregion

			impl = cbapi.Put<SalesOrder>(obj.Local, obj.LocalID);

			#region Taxes
			if (impl != null && (bindingExt.SyncTaxes != BCTaxSyncAttribute.NoSync) && (obj.Local.IsTaxValid?.Value == true))
			{				
				String receivedTaxes = String.Join("; ", impl.TaxDetails?.Select(x => String.Join("=", x.TaxID?.Value, x.TaxAmount?.Value)).ToArray() ?? new String[] { BCConstants.None });
				this.LogInfo(Operation.LogScope(obj), BCMessages.LogTaxesOnOrderReceived,
					impl.OrderNbr?.Value,
					impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None,
					String.IsNullOrEmpty(receivedTaxes) ? BCConstants.None : receivedTaxes);

				List<TaxDetail> sentTaxesToValidate = obj.Local?.TaxDetails.ToList() ?? new List<TaxDetail>();
				List<TaxDetail> receivedTaxesToValidate = impl.TaxDetails.ToList() ?? new List<TaxDetail>();
				//Validate Tax Zone
				if (sentTaxesToValidate.Count > 0  && impl.FinancialSettings.CustomerTaxZone.Value == null)
				{
					throw new PXException(BCObjectsMessages.CannotFindTaxZone, 
						String.Join(", ", sentTaxesToValidate.Select(x => x.TaxID?.Value).Where(x => x != null).ToArray() ?? new String[] { BCConstants.None }));
				}
				//Validate tax codes and amounts
				List<TaxDetail> invalidSentTaxes = new List<TaxDetail>();
				foreach (TaxDetail sent in sentTaxesToValidate)
				{
					TaxDetail received = receivedTaxesToValidate.FirstOrDefault(x => String.Equals(x.TaxID?.Value, sent.TaxID?.Value, StringComparison.InvariantCultureIgnoreCase));
					// This is the line to filter out the incoming taxes that has 0 value, thus if settings in AC are correct they wont be created as lines on SO
					if ((received == null && sent.TaxAmount.Value != 0)
						|| (received != null && sent.TaxAmount?.Value != received.TaxAmount?.Value))
					{
						invalidSentTaxes.Add(sent);
					}

					if (received != null) receivedTaxesToValidate.Remove(received);
				}
				if(invalidSentTaxes.Count > 0)
				{
					throw new PXException(BCObjectsMessages.CannotFindMatchingTaxExt,
						String.Join(",", invalidSentTaxes.Select(x => x.TaxID?.Value)),
						impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None);
				}
				List<TaxDetail> invalidReceivedTaxes = receivedTaxesToValidate.Where(x => (x.TaxAmount?.Value ?? 0m) != 0m && (x.TaxableAmount?.Value ?? 0m) != 0m).ToList();
				if (invalidReceivedTaxes.Count > 0)
				{
					throw new PXException(BCObjectsMessages.CannotFindMatchingTaxAcu, 
						String.Join(",", invalidReceivedTaxes.Select(x => x.TaxID?.Value)),
						impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None);
				}
			}
			#endregion

			//If we need to cancel the order in Acumatica
			if (obj.Extern?.StatusId == OrderStatuses.Cancelled.GetHashCode() || obj.Extern?.StatusId == OrderStatuses.Declined.GetHashCode())
			{
				impl = cbapi.Invoke<SalesOrder, CancelSalesOrder>(null, impl.SyncID);
			}
			obj.AddLocal(impl, impl.SyncID, impl.SyncTime);

			// Save Details
			DetailInfo[] oldDetails = obj.Details.ToArray(); obj.ClearDetails();
			List<OrdersShippingAddressData> shippingAddresses = obj.Extern.OrderShippingAddresses ?? new List<OrdersShippingAddressData>();
			if (shippingAddresses.Count > 0) obj.AddDetail(BCEntitiesAttribute.OrderAddress, impl.SyncID, shippingAddresses.First().Id.ToString()); //Shipment ID detail	
			foreach (OrdersProductData product in obj.Extern.OrderProducts) //Line ID detail
			{
				if (product.Quantity == 0) continue;

				SalesOrderDetail detail = null;
				detail = impl.Details.FirstOrDefault(x => x.NoteID.Value == oldDetails.FirstOrDefault(o => o.ExternID == product.Id.ToString())?.LocalID);
				if (detail == null)
				{
					String inventoryCD = GetInventoryCDByExternID(
						product.ProductId.ToString(),
						product.OptionSetId >= 0 ? product.VariandId.ToString() : null,
						product.Sku,
						product.ProductType,
						out string uom);
					detail = impl.Details.FirstOrDefault(x => !obj.Details.Any(o => x.NoteID.Value == o.LocalID)
					&& x.InventoryID.Value == inventoryCD);
				}
				if (detail != null)
				{
					obj.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, product.Id.ToString());
					continue;
				}
				throw new PXException(BCMessages.CannotMapLines);
			}

			UpdateStatus(obj, operation);

			#region Payments
			if (existing == null && local.Payments != null && impl.Payments != null)
			{
				for (int i = 0; i < bucket.Payments.Count; i++)
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
				new SalesOrder() { 
					OrderType = GetBindingExt<BCBindingExt>().OrderType.SearchField(), 
					OrderNbr = new StringReturn(), 
					Status = new StringReturn(), 
					CustomerID = new StringReturn(),
					ExternalRef = new StringReturn(),
					Details = new List<SalesOrderDetail>() { new SalesOrderDetail() { 
						ReturnBehavior = ReturnBehavior.OnlySpecified, 
						InventoryID = new StringReturn() } } },
				minDateTime, maxDateTime, filters, GetCustomFieldsForExport());

			if (impls != null && impls.Count() > 0)
			{
				int countNum = 0;
				List<IMappedEntity> mappedList = new List<IMappedEntity>();
				foreach (SalesOrder impl in impls)
				{
					IMappedEntity obj = new MappedOrder(impl, impl.SyncID, impl.SyncTime);

					mappedList.Add(obj);
					countNum++;
					if (countNum % BatchFetchCount == 0 || countNum == impls.Count())
					{
						ProcessMappedListForExport(ref mappedList);
					}
				}
			}
		}
		public override EntityStatus GetBucketForExport(BCSalesOrderBucket bucket, BCSyncStatus syncstatus)
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

		public override void MapBucketExport(BCSalesOrderBucket bucket, IMappedEntity existing)
		{
			MappedOrder obj = bucket.Order;
			if (obj.Local.Details == null || obj.Local.Details.Count == 0) throw new PXException(BigCommerceMessages.NoOrderDetails, obj.Local.OrderNbr.Value);
			SalesOrder impl = obj.Local;
			OrderData orderData = obj.Extern = new OrderData();

			obj.Extern.Id = APIHelper.ReferenceParse(impl.ExternalRef?.Value, GetBinding().BindingName).ToInt();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			BCSyncStatus customerStatus = PXSelectJoin<BCSyncStatus,
				InnerJoin<PX.Objects.AR.Customer, On<PX.Objects.AR.Customer.noteID, Equal<BCSyncStatus.localID>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<PX.Objects.AR.Customer.acctCD, Equal<Required<PX.Objects.AR.Customer.acctCD>>>>>>>
				.Select(this, BCEntitiesAttribute.Customer, impl.CustomerID?.Value);
			var customer = PX.Objects.AR.Customer.PK.Find(this, bindingExt.GuestCustomerID);
			if (customer != null && customer?.AcctCD.Trim() == impl.CustomerID?.Value?.Trim())
			{
				orderData.CustomerId = 0;
			}
			else if (customerStatus == null || customerStatus.ExternID == null)
			{
				throw new PXException(BCMessages.CustomerNotSyncronized, impl.CustomerID?.Value);
			}
			else
			{
				orderData.CustomerId = customerStatus.ExternID.ToInt();
			}
			orderData.StatusId = ConvertStatus(impl.Status?.Value).GetHashCode();
			orderData.DiscountAmount = impl.Totals.DiscountTotal?.Value ?? 0m;
			if (obj.IsNew && impl.Date.Value != null) orderData.DateCreatedUT = impl.Date.Value.TDToString(PXTimeZoneInfo.FindSystemTimeZoneById(bindingExt.OrderTimeZone));

			string attentionB = impl.BillToContact.Attention?.Value ?? impl.BillToContact.BusinessName?.Value;
			orderData.BillingAddress = new OrderAddressData();
			orderData.BillingAddress.Street1 = impl.BillToAddress.AddressLine1?.Value;
			orderData.BillingAddress.Email = impl.BillToContact.Email?.Value;
			orderData.BillingAddress.Street2 = impl.BillToAddress.AddressLine2?.Value;
			orderData.BillingAddress.City = impl.BillToAddress.City?.Value;
			orderData.BillingAddress.ZipCode = impl.BillToAddress.PostalCode?.Value;
			orderData.BillingAddress.Country = GetSubstituteExternByLocal(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.Country), impl.BillToAddress.Country?.Value, Country.PK.Find(this, impl.BillToAddress.Country?.Value)?.Description);
			orderData.BillingAddress.Company = impl.BillToContact.BusinessName?.Value;
			orderData.BillingAddress.Phone = impl.BillToContact.Phone1?.Value;
			orderData.BillingAddress.FirstName = attentionB.FieldsSplit(0, attentionB);
			orderData.BillingAddress.LastName = attentionB.FieldsSplit(1, attentionB);
			orderData.BaseShippingCost = (impl.Totals?.Freight?.Value ?? 0) + (impl.Totals?.PremiumFreight?.Value ?? 0);
			orderData.ShippingCostIncludingTax = orderData.BaseShippingCost;
			orderData.ShippingCostExcludingTax = orderData.BaseShippingCost;
			orderData.TotalIncludingTax = impl.OrderTotal?.Value ?? 0;
			orderData.TotalExcludingTax = orderData.TotalIncludingTax > 0 ? orderData.TotalIncludingTax - (impl.TaxTotal?.Value ?? 0) : orderData.TotalIncludingTax;

			State state = PXSelect<State, Where<State.stateID, Equal<Required<State.stateID>>>>.Select(this, impl.BillToAddress.State?.Value);
			if (state == null)
				orderData.BillingAddress.State = GetSubstituteExternByLocal(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), impl.BillToAddress.State?.Value, impl.BillToAddress.State?.Value);
			else
				orderData.BillingAddress.State = state.Name;
			orderData.BillingAddress.State = string.IsNullOrEmpty(orderData.BillingAddress.State) ? " " : orderData.BillingAddress.State;

			string attentionS = impl.ShipToContact.Attention?.Value ?? impl.ShipToContact.BusinessName?.Value;
			OrdersShippingAddressData shippingAddress = new OrdersShippingAddressData();
			orderData.OrderShippingAddresses = new OrdersShippingAddressData[] { shippingAddress }.ToList();
			shippingAddress.Id = obj.Details.Where(x => x.EntityType == BCEntitiesAttribute.OrderAddress && x.LocalID == obj.LocalID.Value).FirstOrDefault()?.ExternID.ToInt();
			shippingAddress.Street1 = impl.ShipToAddress.AddressLine1?.Value;
			shippingAddress.Street2 = impl.ShipToAddress.AddressLine2?.Value;
			shippingAddress.City = impl.ShipToAddress.City?.Value;
			shippingAddress.ZipCode = impl.ShipToAddress.PostalCode?.Value;
			shippingAddress.Country = GetSubstituteExternByLocal(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.Country), impl.ShipToAddress.Country?.Value, Country.PK.Find(this, impl.ShipToAddress.Country?.Value)?.Description);
			shippingAddress.Company = impl.ShipToContact.BusinessName?.Value;
			shippingAddress.Phone = impl.ShipToContact.Phone1?.Value;
			shippingAddress.FirstName = attentionS.FieldsSplit(0, attentionS);
			shippingAddress.LastName = attentionS.FieldsSplit(1, attentionS);
			state = PXSelect<State, Where<State.stateID, Equal<Required<State.stateID>>>>.Select(this, impl.ShipToAddress.State?.Value);
			if (state == null)
				shippingAddress.State = GetSubstituteExternByLocal(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), impl.ShipToAddress.State?.Value, impl.ShipToAddress.State?.Value);
			else
				shippingAddress.State = state.Name;

			List<OrdersProductData> productData = new List<OrdersProductData>();
			if (existing != null && existing.Extern != null)
			{
				var syncDetails = PXSelectJoin<BCSyncDetail,
					InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncDetail.entityType, Equal<Required<BCSyncDetail.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>
					.Select(this, BCEntitiesAttribute.OrderLine, existing.ExternID);
				var deletedList = syncDetails.Where(p => !impl.Details.Any(p2 => p2.NoteID.Value == ((BCSyncDetail)p).LocalID));

				foreach (BCSyncDetail detail in deletedList)
				{

					OrdersProductData product = new OrdersProductData();
					product.Id = detail.ExternID.ToInt();
					product.Quantity = 0;
					product.PriceExcludingTax = 0;
					product.PriceIncludingTax = 0;
					productData.Add(product);
				}
			}
			PX.Objects.IN.InventoryItem giftCertificate = bindingExt.GiftCertificateItemID != null ? PX.Objects.IN.InventoryItem.PK.Find(this, bindingExt.GiftCertificateItemID) : null;
			foreach (SalesOrderDetail detail in impl.Details)
			{
				OrdersProductData product = new OrdersProductData();
				if (giftCertificate != null && detail.InventoryID?.Value?.Trim() == giftCertificate.InventoryCD?.Trim()) continue;

				String key = GetProductExternIDByProductCD(detail.InventoryID?.Value, out string sku, out string uom);
				if (key == null) throw new PXException(BCMessages.InvenotryNotSyncronized, detail.InventoryID?.Value);
				if (uom != detail?.UOM?.Value) throw new PXException(BCMessages.NotBaseUOMUsed, detail.InventoryID?.Value);
				if (detail.OrderQty.Value % 1 != 0) throw new PXException(BCMessages.NotDecimalQtyUsed, detail.InventoryID?.Value);

				product.Id = obj.Details.Where(x => x.EntityType == BCEntitiesAttribute.OrderLine && x.LocalID == detail.NoteID.Value).FirstOrDefault()?.ExternID.ToInt();
				product.Quantity = (int)detail.OrderQty.Value;
				product.PriceExcludingTax = (decimal)detail.OrderQty.Value == 0 ? detail.Amount.Value.Value : detail.Amount.Value.Value / detail.OrderQty.Value.Value;
				product.PriceIncludingTax = (decimal)detail.OrderQty.Value == 0 ? detail.Amount.Value.Value : detail.Amount.Value.Value / detail.OrderQty.Value.Value;
				if (!String.IsNullOrEmpty(key.KeySplit(0, String.Empty))) product.ProductId = key.KeySplit(0, String.Empty).ToInt() ?? 0;
				if (!String.IsNullOrEmpty(key.KeySplit(1, String.Empty))) product.VariandId = key.KeySplit(1, String.Empty).ToInt() ?? 0;

				productData.Add(product);
			}
			orderData.OrderProducts = productData;
		}
		public override void SaveBucketExport(BCSalesOrderBucket bucket, IMappedEntity existing, String operation)
		{
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			MappedOrder obj = bucket.Order;

			OrderData data = null;
			if (obj.ExternID != null && existing != null && obj.Local.ExternalOrderOriginal?.Value == true) // Update status only if order is unmodified
			{
				data = new OrderData();
				data.StatusId = obj.Extern.StatusId;
				data = orderDataProvider.Update(data, obj.ExternID.ToInt().Value);
			}
			else
			{
				if (obj.ExternID == null || existing == null)
				{
					data = orderDataProvider.Create(obj.Extern);
				}
				else
				{
					data = orderDataProvider.Update(obj.Extern, obj.ExternID.ToInt().Value);
				}

				PX.Objects.IN.InventoryItem giftCertificate = bindingExt.GiftCertificateItemID != null ? PX.Objects.IN.InventoryItem.PK.Find(this, bindingExt.GiftCertificateItemID) : null;
				List<OrdersProductData> products = orderProductsRestDataProvider.Get(data.Id?.ToString()) ?? new List<OrdersProductData>();
				DetailInfo[] oldDetails = obj.Details.ToArray(); obj.ClearDetails();
				if (data.ShippingAddressCount > 0)
				{
					var ShippingAddresses = orderShippingAddressesRestDataProvider.Get(data.Id?.ToString()) ?? new List<OrdersShippingAddressData>();
					if (ShippingAddresses.Count > 0) obj.AddDetail(BCEntitiesAttribute.OrderAddress, obj.LocalID, ShippingAddresses.First().Id.ToString()); //Shipment ID detail
				}
				foreach (SalesOrderDetail detail in obj.Local.Details) //Line ID detail
				{
					OrdersProductData product = null;
					if (giftCertificate != null && detail.InventoryID?.Value?.Trim() == giftCertificate.InventoryCD?.Trim())
					{
						product = products.FirstOrDefault(x => x.ProductType == OrdersProductsType.GiftCertificate);
						if (product != null)
							obj.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, product.Id.ToString());
						continue;
					}

					product = products.FirstOrDefault(x => x.ProductId.ToString() == oldDetails.FirstOrDefault(o => o.LocalID == detail.NoteID.Value)?.ExternID);
					if (product == null)
					{
						String externalID = GetProductExternIDByProductCD(detail.InventoryID.Value, out string sku, out string uom);
						product = products.FirstOrDefault(x => !obj.Details.Any(o => x.Id.ToString() == o.ExternID)
							&& x.ProductId.ToString() == externalID.KeySplit(0)
							&& (String.IsNullOrEmpty(externalID.KeySplit(1, String.Empty))
								|| x.VariandId.ToString() == externalID.KeySplit(1, String.Empty)
								|| (!string.IsNullOrEmpty(sku) && x.Sku == sku)));
					}
					if (product != null)
					{
						obj.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, product.Id.ToString());
						continue;
					}
					throw new PXException(BCMessages.CannotMapLines);
				}
			}
			obj.AddExtern(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());
			UpdateStatus(obj, operation);

			#region Update ExternalRef
			string externalRef = APIHelper.ReferenceMake(data.Id?.ToString(), GetBinding().BindingName);

			string[] keys = obj.Local?.ExternalRef?.Value?.Split(';');
			if (keys?.Contains(externalRef) != true)
			{
				if (!string.IsNullOrEmpty(obj.Local?.ExternalRef?.Value))
					externalRef = new object[] { obj.Local?.ExternalRef?.Value, externalRef }.KeyCombine();

				if (externalRef.Length < 40 && obj.Local.SyncID != null)
					PXDatabase.Update<PX.Objects.SO.SOOrder>(
								  new PXDataFieldAssign(typeof(PX.Objects.SO.SOOrder.customerRefNbr).Name, PXDbType.NVarChar, externalRef),
								  new PXDataFieldRestrict(typeof(PX.Objects.SO.SOOrder.noteID).Name, PXDbType.UniqueIdentifier, obj.Local.SyncID.Value)
								  );
			}
			#endregion
		}
		#endregion

		#region Methods
		public static OrderStatuses ConvertStatus(String status)
		{
			switch (status)
			{
				case PX.Objects.SO.Messages.BackOrder:
					return OrderStatuses.PartiallyShipped;
				case PX.Objects.SO.Messages.Cancelled:
					return OrderStatuses.Cancelled;
				case PX.Objects.SO.Messages.Completed:
					return OrderStatuses.Shipped;
				case PX.Objects.SO.Messages.CreditHold:
					return OrderStatuses.VerificationRequired;
				case PX.Objects.SO.Messages.Hold:
					return OrderStatuses.Pending;
				case PX.Objects.SO.Messages.Open:
					return OrderStatuses.AwaitingFulfillment;
				case PX.Objects.EP.Messages.Balanced:
					return OrderStatuses.VerificationRequired;
				case PX.Objects.SO.Messages.Shipping:
					return OrderStatuses.AwaitingShipment;
				case PX.Objects.EP.Messages.Voided:
					return OrderStatuses.Declined;
				case PX.Objects.SO.Messages.Invoiced:
					return OrderStatuses.AwaitingFulfillment;
				default:
					return OrderStatuses.Pending;
			}
		}
		protected virtual String GetProductExternIDByProductCD(String inventoryCD, out string sku, out string uom)
		{
			sku = null;

			PX.Objects.IN.InventoryItem item = PXSelect<PX.Objects.IN.InventoryItem,
					Where<PX.Objects.IN.InventoryItem.inventoryCD, Equal<Required<PX.Objects.IN.InventoryItem.inventoryCD>>>>.Select(this, inventoryCD);
			if (item?.InventoryID == null) throw new PXException(BCMessages.InvenotryNotSyncronized, inventoryCD);
			if (item?.ItemStatus == PX.Objects.IN.INItemStatus.Inactive) throw new PXException(BCMessages.InvenotryInactive, inventoryCD);
			uom = item?.BaseUnit?.Trim();

			BCSyncStatus itemStatus = PXSelect<BCSyncStatus,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And2<Where<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>,
					And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>>>
				.Select(this, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.StockItem, item.NoteID);

			Int32? productID = null, variantID = null;
			if (itemStatus?.ExternID == null && item.TemplateItemID != null) // Check for parent item - for variants
			{
				foreach (var status in PXSelectJoin<BCSyncStatus,
						InnerJoin<InventoryItem, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncStatus.localID>>,
						InnerJoin<BCSyncDetail, On<BCSyncDetail.syncID, Equal<BCSyncStatus.syncID>>>>,
						Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
							And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
							And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							And<BCSyncDetail.localID, Equal<Required<BCSyncDetail.localID>>>>>>>
						.Select(this, BCEntitiesAttribute.ProductWithVariant, item.NoteID))
				{
					BCSyncDetail variantStatus = status.GetItem<BCSyncDetail>();
					BCSyncStatus parentStatus = status.GetItem<BCSyncStatus>();
					if (variantStatus?.ExternID != null && parentStatus?.ExternID != null)
					{
						variantID = variantStatus.ExternID.ToInt();
						productID = parentStatus.ExternID.ToInt();
					}
				}
			}
			else if (itemStatus?.ExternID != null) productID = itemStatus?.ExternID.ToInt();

			if (productID == null)
			{
				ProductData product = productDataProvider.Get(new FilterProducts() { SKU = inventoryCD }).FirstOrDefault();
				if (product != null)
				{
					sku = product.Sku;
					productID = product.Id;
				}
			}

			if (productID == null) throw new PXException(BCMessages.InvenotryNotSyncronized, inventoryCD);

			return variantID == null ? productID?.ToString() : new object[] { productID, variantID }.KeyCombine();
		}
		protected virtual String GetInventoryCDByExternID(String productID, String variantID, String sku, OrdersProductsType type, out string uom)
		{
			if (type == OrdersProductsType.GiftCertificate)
			{
				BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
				PX.Objects.IN.InventoryItem inventory = bindingExt?.GiftCertificateItemID != null ? PX.Objects.IN.InventoryItem.PK.Find(this, bindingExt?.GiftCertificateItemID) : null;
				if (inventory?.InventoryCD == null)
					throw new PXException(BigCommerceMessages.NoGiftCertificateItem);

				uom = inventory.BaseUnit?.Trim();
				return inventory.InventoryCD.Trim();
			}

			string key = variantID != null ? new Object[] { productID, variantID }.KeyCombine() : productID;
			PX.Objects.IN.InventoryItem item = null;
			if (variantID != null)
			{
				item = PXSelectJoin<PX.Objects.IN.InventoryItem,
						InnerJoin<BCSyncDetail, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncDetail.localID>>,
						InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>>,
							Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
								And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
								And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
								And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>,
								And<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>>>>>
						.Select(this, BCEntitiesAttribute.ProductWithVariant, productID, sku);
			}
			else
			{
				item = PXSelectJoin<PX.Objects.IN.InventoryItem,
					   LeftJoin<BCSyncStatus, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncStatus.localID>>>,
					   Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						   And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						   And2<Where<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							   Or<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>,
						   And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>
					   .Select(this, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, key);
			}

			if (item == null)
				item = PXSelect<PX.Objects.IN.InventoryItem,
							Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>
							.Select(this, sku);

			if (item == null)
				throw new PXException(BCMessages.InvenotryNotFound, sku, key);
			if (item.ItemStatus == PX.Objects.IN.INItemStatus.Inactive)
				throw new PXException(BCMessages.InvenotryInactive, item.InventoryCD);

			uom = item?.BaseUnit?.Trim();
			return item?.InventoryCD?.Trim();
		}
		#endregion
	}
}
