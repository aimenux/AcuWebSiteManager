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
using PX.Common;
using PX.Data.BQL;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.Common;
using Newtonsoft.Json;
using PX.Data.PushNotifications;
using PX.Objects.CA;
using Serilog.Context;

namespace PX.Commerce.BigCommerce
{
	public class BCPaymentEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Payment;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedPayment Payment;
		public MappedOrder Order;
	}

	public class BCPaymentsRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			return null;
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped)
		{
			#region Payments
			return base.Restrict<MappedPayment>(mapped, delegate (MappedPayment obj)
			{
				if (obj.Extern != null)
				{
					if (obj.Extern.Status == OrderPaymentStatus.Error)
					{
						// we should skip payments with error
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedError, obj.Extern.Id));
					}

					if (obj.Extern.PaymentMethod == "custom")
					{
						// we should skip custom payments
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodNotSupported, obj.Extern.Id, obj.Extern.PaymentMethod));
					}

					if (String.IsNullOrEmpty(obj.Extern.PaymentMethod))
					{
						// we should skip custom payments
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodEmpty, obj.Extern.Id));
					}

					if (obj.Extern.Event != OrderPaymentEvent.Authorization && obj.Extern.Event != OrderPaymentEvent.Purchase && obj.Extern.Event != OrderPaymentEvent.Pending)
					{
						// we should skip payment transactions except Authorized or Purchase
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedEventNotSupported, obj.Extern.Id, obj.Extern.Event));
					}

					var paymentMethods = PXSelectReadonly<BCPaymentMethods,
										 Where<BCPaymentMethods.bindingID, Equal<Required<BCPaymentMethods.bindingID>>>>
										.Select((PXGraph)processor, processor.Operation.Binding).Select(x => x.GetItem<BCPaymentMethods>())
										.ToList();
					string method;
					IEnumerable<BCPaymentMethods> paymentMethod = null;
					if (obj.Extern.PaymentMethod == BCConstants.Emulated)
					{
						method = obj.Extern.Gateway;
						paymentMethod = paymentMethods.Where(x => x.StorePaymentMethod == method?.ToUpper());
					}
					else
					{
						method = string.Format("{0} ({1})", obj.Extern.Gateway, obj.Extern.PaymentMethod);
						paymentMethod = paymentMethods.Where(x => x.StorePaymentMethod == method.ToUpper() && x.StoreOrderPaymentMethod?.ToUpper() == obj.Extern.OrderPaymentMethod?.ToUpper());
						if (paymentMethod == null || paymentMethod?.Count() == 0)
							paymentMethod = paymentMethods.Where(x => x.StorePaymentMethod == method.ToUpper());

					}
					//skip if payment method not present at all or ProcessPayment is not true
					if (paymentMethod != null && paymentMethod.Count() > 0 && paymentMethod.All(x => x.Active != true))
					{
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedNotConfigured, obj.Extern.Id, obj.Extern.Gateway));
					}
				}

				if (processor.SelectStatus(BCEntitiesAttribute.Order, obj.Extern?.OrderId) == null)
				{
					//Skip if order not synced
					return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedOrderNotSynced, obj.Extern.Id, obj.Extern.OrderId));
				}

				return null;
			});
			#endregion
		}
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.Payment, BCCaptions.Payment,
		IsInternal = false,
		Direction = SyncDirection.Import,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.AR.ARPaymentEntry),
		ExternTypes = new Type[] { typeof(OrdersTransactionData) },
		LocalTypes = new Type[] { typeof(Payment) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.ARPayment),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.AR.ARPayment.refNbr, Where<PX.Objects.AR.ARPayment.docType, Equal<ARDocType.payment>>>),
		URL = "orders?keywords={0}&searchDeletedOrders=no",
		Requires = new string[] { BCEntitiesAttribute.Order })]
	[BCProcessorRealtime(PushSupported = false, HookSupported = false)]
	public class BCPaymentProcessor : BCProcessorSingleBase<BCPaymentProcessor, BCPaymentEntityBucket, MappedPayment>, IProcessor
	{
		protected OrderRestDataProvider orderDataProvider;
		protected IChildReadOnlyRestDataProvider<OrdersTransactionData> orderTransactionsRestDataProvider;
		protected List<BCPaymentMethods> paymentMethods;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			orderDataProvider = new OrderRestDataProvider(client);
			orderTransactionsRestDataProvider = new OrderTransactionsRestDataProvider(client);
			paymentMethods = PXSelectReadonly<BCPaymentMethods,
				Where<BCPaymentMethods.bindingID, Equal<Required<BCPaymentMethods.bindingID>>>>
				.Select(this, Operation.Binding).Select(x => x.GetItem<BCPaymentMethods>()).ToList();
		}
		#endregion

		#region Pull
		public override MappedPayment PullEntity(Guid? localID, Dictionary<string, object> fields)
		{
			Payment impl = cbapi.GetByID<Payment>(localID);
			if (impl == null) return null;

			MappedPayment obj = new MappedPayment(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override MappedPayment PullEntity(String externID, String jsonObject)
		{
			OrdersTransactionData data = orderTransactionsRestDataProvider.GetByID(externID.KeySplit(1), externID.KeySplit(0));
			if (data == null) return null;

			MappedPayment obj = new MappedPayment(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedUT.ToDate(), data.CalculateHash());

			return obj;
		}
		public override IEnumerable<MappedPayment> PullSimilar(IExternEntity entity, out string uniqueField)
		{
			var externEntity = (OrdersTransactionData)entity;

			uniqueField = externEntity.GatewayTransactionId ?? (externEntity.ReferenceTransactionId == null ? externEntity.Id.ToString() : externEntity.ReferenceTransactionId.ToString());
			if (string.IsNullOrEmpty(uniqueField))
				return null;

			Payment[] impls = cbapi.GetAll<Payment>(new Payment() { PaymentRef = uniqueField.SearchField() },
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>());
			if (impls == null) return null;

			List<MappedPayment> result = new List<MappedPayment>();
			foreach (Payment impl in impls)
			{
				Payment data = cbapi.GetByID<Payment>(impl.SyncID);
				if (data != null)
				{
					result.Add(new MappedPayment(data, data.SyncID, data.SyncTime));
				}
			}
			return result;
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterOrders filter = new FilterOrders { MinDateModified = minDateTime == null ? null : minDateTime, MaxDateModified = maxDateTime == null? null : maxDateTime };

			IEnumerable<OrderData> orders = orderDataProvider.GetAll(filter);

			foreach (OrderData orderData in orders)
			{
				if (this.SelectStatus(BCEntitiesAttribute.Order, orderData.Id.ToString()) == null)
					continue; //Skip if order not synced

				foreach (OrdersTransactionData data in orderTransactionsRestDataProvider.Get(orderData.Id.ToString()) ?? new List<OrdersTransactionData>())
				{
					BCPaymentEntityBucket bucket = CreateBucket();

					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
					EntityStatus orderStatus = EnsureStatus(order);

					data.OrderPaymentMethod = orderData.PaymentMethod;
					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedUT.ToDate(), data.CalculateHash()).With(_ => { _.ParentID = order.SyncID; return _; });
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
				}
				if (CreatePaymentfromOrder(orderData.PaymentMethod))
				{
					OrdersTransactionData data = CreateOrderTransactionData(orderData);
					BCPaymentEntityBucket bucket = CreateBucket();

					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
					EntityStatus orderStatus = EnsureStatus(order);

					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, data.Id.ToString(), orderData.DateModifiedUT.ToDate(), data.CalculateHash());
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
				}
			}
		}
		public override EntityStatus GetBucketForImport(BCPaymentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			if (syncstatus.ExternID.HasParent())
			{
				List<OrdersTransactionData> transactions = orderTransactionsRestDataProvider.Get(syncstatus.ExternID.KeySplit(0));

				OrdersTransactionData data = transactions.FirstOrDefault(x => x.Id.ToString() == syncstatus.ExternID.KeySplit(1));
				if (data == null) return EntityStatus.None;

				OrdersTransactionData lastPayment = transactions.LastOrDefault(x => x.Gateway == data.Gateway && x.Status != OrderPaymentStatus.Error);
				if (lastPayment != null) //Evaluate last transaction event
					data.Event = lastPayment.Event;

				OrderData orderData = orderDataProvider.GetByID(data.OrderId);
				data.OrderPaymentMethod = orderData.PaymentMethod;

				MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedUT.ToDate(), data.CalculateHash());
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

				MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
				EntityStatus orderStatus = EnsureStatus(order);

				return status;
			}
			else 
			{
				OrderData orderData = orderDataProvider.GetByID(syncstatus.ExternID);
				if (CreatePaymentfromOrder(orderData.PaymentMethod))
				{
					OrdersTransactionData data = CreateOrderTransactionData(orderData);
					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, data.Id.ToString(), orderData.DateModifiedUT.ToDate(), data.CalculateHash());
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
					EntityStatus orderStatus = EnsureStatus(order);
					return status;
				}
				return EntityStatus.None;
			}
		}

		public override void MapBucketImport(BCPaymentEntityBucket bucket, IMappedEntity existing)
		{
			MappedPayment obj = bucket.Payment;

			OrdersTransactionData data = obj.Extern;
			Payment impl = obj.Local = new Payment();
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			PXResult<PX.Objects.SO.SOOrder, PX.Objects.AR.Customer, PX.Objects.CR.Location, BCSyncStatus> result = PXSelectJoin<PX.Objects.SO.SOOrder,
				InnerJoin<PX.Objects.AR.Customer, On<PX.Objects.AR.Customer.bAccountID, Equal<SOOrder.customerID>>,
				InnerJoin<PX.Objects.CR.Location, On<PX.Objects.CR.Location.locationID, Equal<SOOrder.customerLocationID>>,
				InnerJoin<BCSyncStatus, On<PX.Objects.SO.SOOrder.noteID, Equal<BCSyncStatus.localID>>>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>
				.Select(this, BCEntitiesAttribute.Order, data.OrderId).Select(r => (PXResult<SOOrder, PX.Objects.AR.Customer, PX.Objects.CR.Location, BCSyncStatus>)r).FirstOrDefault();
			if (result == null) throw new PXException(BCMessages.OrderNotSyncronized, data.OrderId);
			PX.Objects.SO.SOOrder order = result.GetItem<PX.Objects.SO.SOOrder>();
			PX.Objects.AR.Customer customer = result.GetItem<PX.Objects.AR.Customer>();
			PX.Objects.CR.Location location = result.GetItem<PX.Objects.CR.Location>();
			if (order != null && order.OpenDoc == false) throw new PXException(BCMessages.OrderStatusDoesNotAllowPayments, order.OrderNbr);

			//Product
			impl.Type =PX.Objects.AR.Messages.Prepayment.ValueField();
			impl.CustomerID = customer.AcctCD.ValueField();
			impl.CustomerLocationID = location.LocationCD.ValueField();
			impl.CurrencyID = data.Currency.ValueField();
			impl.ApplicationDate = data.DateCreatedUT.ToDate(PXTimeZoneInfo.FindSystemTimeZoneById(bindingExt.OrderTimeZone)).ValueField();
			impl.PaymentAmount = ((decimal)data.Amount).ValueField();
			impl.PaymentRef = (data.GatewayTransactionId ?? (data.ReferenceTransactionId == null ? data.Id.ToString() : data.ReferenceTransactionId.ToString())).ValueField();
			impl.BranchID = Branch.PK.Find(this, binding.BranchID)?.BranchCD?.ValueField();
			impl.Hold = false.ValueField();

			BCPaymentMethods methodMapping = GetPaymentMethodMapping(GetPaymentMethodName(data), bucket.Order?.Extern?.PaymentMethod, data.Currency, 
				out string paymentMethod, out string cashAcount, out string processingCenter);
			impl.PaymentMethod = paymentMethod?.Trim()?.ValueField();
			impl.CashAccount = cashAcount?.Trim()?.ValueField();
			if (methodMapping.StorePaymentMethod == BCObjectsConstants.GiftCertificateCode)
				impl.Description = string.Format("{0}; Payment Method: {1}({4}); Order: {2}; Payment ID: {3}", BCConnector.NAME, methodMapping.StorePaymentMethod, data.OrderId, data.Id, data.GiftCertificate?.Code).ValueField();
			else
				impl.Description = string.Format("{0}; Payment Method: {1}; Order: {2}; Payment ID: {3}", BCConnector.NAME, methodMapping.StorePaymentMethod, data.OrderId, data.Id).ValueField();
			impl.NeedRelease = methodMapping?.ReleasePayments ?? false;

			//Credit Card:
			if (obj.Extern.GatewayTransactionId != null && processingCenter != null)
			{
				String paymentTran = ParceTransactionNumber(obj.Extern.GatewayTransactionId);

				impl.IsNewCard = true.ValueField();
				impl.SaveCard = (!String.IsNullOrWhiteSpace(obj.Extern.PaymentInstrumentToken)).ValueField();
				impl.ProcessingCenterID = processingCenter?.ValueField();

				CreditCardTransactionDetail detail = new CreditCardTransactionDetail();
				detail.TranNbr = paymentTran.ValueField();
				detail.TranDate = impl.ApplicationDate;
				detail.ExtProfileId = data.PaymentInstrumentToken.ValueField();
				switch (data.Event)
				{
					case OrderPaymentEvent.Authorization:
						detail.TranType = "AUT".ValueField();
						break;
					case OrderPaymentEvent.Capture:
						detail.TranType = "PAC".ValueField();
						break;
					case OrderPaymentEvent.Purchase:
						detail.TranType = "AAC".ValueField();
						break;
					default:
						detail.TranType = "UKN".ValueField();
						break;
				}

				impl.CreditCardTransactionInfo = new List<CreditCardTransactionDetail>(new[] { detail });
			}

			//Calculated Unpaid Balance
			decimal curyUnpaidBalance = order.CuryOrderTotal ?? 0m;
			foreach(SOAdjust adj in PXSelect<SOAdjust,
							Where<SOAdjust.adjdOrderType, Equal<Required<SOOrder.orderType>>,
								And<SOAdjust.adjdOrderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(this, order.OrderType, order.OrderNbr))
			{
				curyUnpaidBalance -= adj.CuryAdjdAmt ?? 0m;
			}

			//If we have applied already, than skip
			if ((existing as MappedPayment) == null || ((MappedPayment)existing).Local == null || 
                ((MappedPayment)existing).Local.OrdersToApply == null || !((MappedPayment)existing).Local.OrdersToApply.Any(d => d.OrderType?.Value == order.OrderType && d.OrderNbr?.Value == order.OrderNbr))
			{
				decimal applicationAmount = (decimal)data.Amount > curyUnpaidBalance ? curyUnpaidBalance : (decimal)data.Amount;

				//Order to Apply
				PaymentOrderDetail detail = new PaymentOrderDetail();
				detail.OrderType = order.OrderType.ValueField();
				detail.OrderNbr = order.OrderNbr.ValueField();
				detail.AppliedToOrder = applicationAmount.ValueField();
				impl.OrdersToApply = new List<PaymentOrderDetail>(new[] { detail });
			}
		}
		public override void SaveBucketImport(BCPaymentEntityBucket bucket, IMappedEntity existing, String operation)
		{
			MappedPayment obj = bucket.Payment;
			Boolean needRelease = obj.Local.NeedRelease;

			BCSyncStatus orderStatus = PXSelectJoin<BCSyncStatus,
				InnerJoin<SOOrder, On<SOOrder.noteID, Equal<BCSyncStatus.localID>,
					And<SOOrder.lastModifiedDateTime, Equal<BCSyncStatus.localTS>>>>,
				Where<BCSyncStatus.syncID, Equal<Required<BCSyncStatus.syncID>>>>.Select(this, bucket.Order.SyncID);

			Payment impl = cbapi.Put<Payment>(obj.Local, obj.LocalID);
			bucket.Payment.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);

			if (needRelease && obj.Local.ProcessingCenterID?.Value == null)
			{
				impl = cbapi.Invoke<Payment, ReleasePayment>(impl);
				bucket.Payment.AddLocal(null, obj.LocalID, impl.SyncTime);
				UpdateStatus(obj, operation);
			}

			if (orderStatus?.LocalID != null) //Payment save updates the order, we need to change the saved timestamp.
			{
				orderStatus.LocalTS = BCSyncExactTimeAttribute.SelectDateTime<SOOrder.lastModifiedDateTime>(orderStatus.LocalID.Value);
				orderStatus = (BCSyncStatus)Statuses.Cache.Update(orderStatus);
			}
		}


		public virtual string GetPaymentMethodName(OrdersTransactionData data)
		{
			if (data.PaymentMethod == BCConstants.Emulated)
				return data.Gateway?.ToUpper();
			return string.Format("{0} ({1})", data.Gateway, data.PaymentMethod ?? string.Empty)?.ToUpper();

		}
		public virtual bool CreatePaymentfromOrder(string method)
		{
			var paymentMethod = paymentMethods.FirstOrDefault(x => String.Equals(x.StorePaymentMethod, method, StringComparison.InvariantCultureIgnoreCase) && x.CreatePaymentFromOrder == true && x.Active == true);
			return (paymentMethod != null);
		}
		public virtual string ParceTransactionNumber(String externalTranId)
		{
			String paymentTran = externalTranId;
			if (!String.IsNullOrWhiteSpace(paymentTran) && paymentTran.IndexOf("#") >= 0)
				paymentTran = paymentTran.Substring(0, paymentTran.IndexOf("#"));

			return paymentTran;
		}

		public virtual BCPaymentMethods GetPaymentMethodMapping(string transactionMethod, string orderMethod, string currencyCode, out string paymentMethod, out string cashAccount, out string processingCenter)
		{
			paymentMethod = null;
			cashAccount = null;
			processingCenter = null;

			BCPaymentMethods result = null;
			if (paymentMethods.Any(x => x.StorePaymentMethod?.ToUpper() == transactionMethod && x.StoreOrderPaymentMethod?.ToUpper() == orderMethod?.ToUpper()))
			{
				result = paymentMethods.FirstOrDefault(x => x.StorePaymentMethod?.ToUpper() == transactionMethod && x.StoreOrderPaymentMethod?.ToUpper() == orderMethod?.ToUpper() && x.Active == true);
				if (result == null)
				{
					throw new PXException(BCMessages.OrderPaymentMethodIsMissing, paymentMethod, orderMethod?.ToUpper(), currencyCode);
				}
			}
			else
			{
				if (paymentMethods.Any(x => x.StorePaymentMethod == transactionMethod.ToUpper()))
				{
					result = paymentMethods.FirstOrDefault(x => x.StorePaymentMethod == transactionMethod.ToUpper() && x.Active == true);
				}
			}

			if (result == null)
			{
				PXCache cache = base.Caches[typeof(BCPaymentMethods)];
				BCPaymentMethods entry = new BCPaymentMethods()
				{
					StorePaymentMethod = transactionMethod.ToUpper(),
					BindingID = Operation.Binding,
					Active = true
				};
				cache.Insert(entry);
				cache.Persist(PXDBOperation.Insert);

				throw new PXException(BCMessages.OrderPaymentMethodIsMissing, paymentMethod, orderMethod?.ToUpper(), currencyCode);
			}
			else
			{
				CashAccount ca = null;
				Company baseCurrency = PXSelect<Company, Where<Company.companyCD, Equal<Required<Company.companyCD>>>>.Select(this, this.Accessinfo.CompanyName);
				var multiCurrency = PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.multicurrency>();
				if (baseCurrency?.BaseCuryID?.Trim() != currencyCode && multiCurrency)
				{
					BCMultiCurrencyPaymentMethod currency = PXSelect<BCMultiCurrencyPaymentMethod,
						Where<BCMultiCurrencyPaymentMethod.paymentMappingID, Equal<Required<BCPaymentMethods.paymentMappingID>>,
							And<BCMultiCurrencyPaymentMethod.curyID, Equal<Required<BCMultiCurrencyPaymentMethod.curyID>>,
							And<BCMultiCurrencyPaymentMethod.bindingID, Equal<Required<BCMultiCurrencyPaymentMethod.bindingID>>>>>>.Select(this, result.PaymentMappingID, currencyCode, result.BindingID);
					ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, currency?.CashAccountID);
				}
				else
					ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, result.CashAccountID);

				paymentMethod = result?.PaymentMethodID;
				cashAccount = ca?.CashAccountCD;
				processingCenter = result?.ProcessingCenterID;

				if (cashAccount == null || paymentMethod == null)
				{
					throw new PXException(BCMessages.OrderPaymentMethodIsMissing, paymentMethod, orderMethod?.ToUpper(), currencyCode);
				}

				return result;
			}
		}
		public virtual OrdersTransactionData CreateOrderTransactionData(OrderData orderData)
		{
			OrdersTransactionData data = new OrdersTransactionData();
			data.Id = orderData.Id.Value;
			data.OrderId = orderData.Id.ToString();
			data.Gateway = orderData.PaymentMethod;
			data.Currency = orderData.CurrencyCode;
			data.DateCreatedUT = orderData.DateCreatedUT;
			data.PaymentMethod = BCConstants.Emulated;
			data.Amount = Convert.ToDouble(orderData.TotalIncludingTax);
			return data;
		}
		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
		}
		public override EntityStatus GetBucketForExport(BCPaymentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			Payment impl = cbapi.GetByID<Payment>(syncstatus.LocalID);
			if (impl == null) return EntityStatus.None;

			MappedPayment obj = bucket.Payment = bucket.Payment.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Payment, SyncDirection.Export);

			return status;
		}

		public override void MapBucketExport(BCPaymentEntityBucket bucket, IMappedEntity existing)
		{
		}
		public override void SaveBucketExport(BCPaymentEntityBucket bucket, IMappedEntity existing, String operation)
		{
		}
		#endregion
	}
}
