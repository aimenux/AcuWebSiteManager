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
using PX.Common;
using PX.Data.BQL;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.Common;
using Newtonsoft.Json;
using PX.Data.PushNotifications;
using PX.Objects.CA;

namespace PX.Commerce.Shopify
{
	public class SPPaymentEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Payment;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedPayment Payment;
		public MappedOrder Order;
	}

	public class SPPaymentsRestrictor : BCBaseRestrictor, IRestrictor
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
					if (obj.Extern.Status == TransactionStatus.Error)
					{
						// we should skip payments with error
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedError, obj.Extern.Id));
					}

					if (obj.Extern.Kind == TransactionType.Void)
					{
						// we should skip void payments
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedVoid, obj.Extern.Id));
					}

                    if (string.IsNullOrWhiteSpace(obj.Extern.Gateway))
                    {
						// we should skip payments without the payment gateway
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodEmpty, obj.Extern.Id));
                    }

                    if (obj.Extern.Kind == TransactionType.Refund)
                    {
						// we should skip refund payments now, and we will support later
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodNotSupported, obj.Extern.Id, TransactionType.Refund.ToString()));
                    }

                    if (obj.Extern.Kind == TransactionType.Capture)
                    {
                        // we should skip Capture to avoid the duplicated processing
                        return new FilterResult(FilterStatus.Ignore,
                            PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodNotSupported, obj.Extern.Id, TransactionType.Capture.ToString()));
                    }

                    if(obj.Extern.ParentId != null && processor.SelectStatus(BCEntitiesAttribute.Payment, new Object[] { obj.Extern.OrderId, obj.Extern.ParentId }.KeyCombine()) != null)
                    {
                        return new FilterResult(FilterStatus.Ignore,
                            PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedParentSynced, obj.Extern.Id, obj.Extern.ParentId));
                    }

                    IEnumerable<BCPaymentMethods> paymentMethod = PXSelectReadonly<BCPaymentMethods,
                                         Where<BCPaymentMethods.bindingID, Equal<Required<BCPaymentMethods.bindingID>>>>
                                        .Select((PXGraph)processor, processor.Operation.Binding).Select(x => x.GetItem<BCPaymentMethods>())
                                        .ToList().Where(x => x.StorePaymentMethod == obj.Extern.Gateway.ToUpper());
                    if (paymentMethod != null && paymentMethod.Count() > 0 && paymentMethod.All(x => x.Active != true))
                    {
						//skip if active is not true
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedNotConfigured, obj.Extern.Id, obj.Extern.Gateway));
                    }
                }

				if (processor.SelectStatus(BCEntitiesAttribute.Order, obj.Extern?.OrderId.ToString()) == null)
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

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Payment, BCCaptions.Payment,
		IsInternal = false,
		Direction = SyncDirection.Import,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.AR.ARPaymentEntry),
		ExternTypes = new Type[] { typeof(OrderTransaction) },
		LocalTypes = new Type[] { typeof(Payment) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.ARPayment),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.AR.ARPayment.refNbr, Where<PX.Objects.AR.ARPayment.docType, Equal<ARDocType.payment>>>),
		URL = "orders/{0}",
		Requires = new string[] { BCEntitiesAttribute.Order })]
	[BCProcessorRealtime(PushSupported = false, HookSupported = false)]
	public class SPPaymentProcessor : BCProcessorSingleBase<SPPaymentProcessor, SPPaymentEntityBucket, MappedPayment>, IProcessor
	{
		protected OrderRestDataProvider orderDataProvider;
		protected BCBinding currentBinding;
		protected BCBindingExt currentBindingExt;
		protected BCBindingShopify currentShopifySettings;
		protected List<BCPaymentMethods> paymentMethods;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			currentBinding = GetBinding();
			currentBindingExt = GetBindingExt<BCBindingExt>();
			currentShopifySettings = GetBindingExt<BCBindingShopify>();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			orderDataProvider = new OrderRestDataProvider(client);
			paymentMethods = PXSelectReadonly<BCPaymentMethods,
				Where<BCPaymentMethods.bindingID, Equal<Required<BCPaymentMethods.bindingID>>>>
				.Select(this, Operation.Binding).Select(x => x.GetItem<BCPaymentMethods>()).ToList();
		}
		#endregion

		public override IEnumerable<MappedPayment> PullSimilar(IExternEntity entity, out string uniqueField)
		{
			var externEntity=(OrderTransaction)entity;

			uniqueField = externEntity?.Authorization ?? externEntity?.Id?.ToString();
			if (string.IsNullOrEmpty(uniqueField))
				return null;

			Payment[] impls = cbapi.GetAll<Payment>(new Payment() {  PaymentRef = uniqueField.SearchField() },
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
			var data = orderDataProvider.GetOrderSingleTransaction(externID.KeySplit(0), externID.KeySplit(1));
			if (data == null) return null;

			MappedPayment obj = new MappedPayment(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateModifiedAt.ToDate(false), data.CalculateHash());

			return obj;
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterOrders filter = new FilterOrders { Status = OrderStatus.Open, Fields = "id,source_name,financial_status,updated_at,created_at,cancelled_at,closed_at" };
			if (minDateTime != null) filter.UpdatedAtMin = minDateTime.Value.ToLocalTime().AddSeconds(-GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0);
			if (maxDateTime != null) filter.UpdatedAtMax = maxDateTime.Value.ToLocalTime();

			IEnumerable<OrderData> datas = orderDataProvider.GetAll(filter);

			foreach (OrderData orderData in datas)
			{
				if (this.SelectStatus(BCEntitiesAttribute.Order, orderData.Id.ToString()) == null)
					continue; //Skip if order not synced

				var transactionList = orderDataProvider.GetOrderTransactions(orderData.Id.ToString());
				if (transactionList == null || transactionList.Count == 0) continue;
				//Only process the successful Transaction
				foreach (OrderTransaction data in transactionList)
                {
					SPPaymentEntityBucket bucket = CreateBucket();

					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedAt.ToDate(false));
					EntityStatus orderStatus = EnsureStatus(order);

					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateModifiedAt.ToDate(false), data.CalculateHash()).With(_ => { _.ParentID = order.SyncID; return _; });
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
				}
			}
		}
		public override EntityStatus GetBucketForImport(SPPaymentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			OrderData orderData = orderDataProvider.GetByID(syncstatus.ExternID.KeySplit(0), false, true, false);
			if (orderData == null || orderData.Transactions == null || !orderData.Transactions.Any(x => x?.Id.ToString() == syncstatus.ExternID.KeySplit(1)))
				return EntityStatus.None;

			OrderTransaction data = orderData.Transactions.FirstOrDefault(x => x?.Id.ToString() == syncstatus.ExternID.KeySplit(1));

			OrderTransaction lastPayment = orderData.Transactions.LastOrDefault(
				x => x.ParentId == data.Id && x.Status != TransactionStatus.Error && x.Status != TransactionStatus.Failure && x.Kind != TransactionType.Refund);
            if (lastPayment != null) //Evaluate last transaction event
            {
                data.Kind = lastPayment.Kind;
                data.Status = lastPayment.Status;
                data.DateModifiedAt = lastPayment.DateCreatedAt;
            }

			MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateModifiedAt.ToDate(false), data.CalculateHash());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedAt.ToDate(false));
			EntityStatus orderStatus = EnsureStatus(order);

			return status;
		}

		public override void MapBucketImport(SPPaymentEntityBucket bucket, IMappedEntity existing)
		{
			MappedPayment obj = bucket.Payment;

			OrderTransaction data = obj.Extern;
			Payment impl = obj.Local = new Payment();
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

			impl.Type = PX.Objects.AP.Messages.Prepayment.ValueField();
			impl.CustomerID = customer.AcctCD.ValueField();
			impl.CustomerLocationID = location.LocationCD.ValueField();
			impl.CurrencyID = data.Currency.ValueField();
			impl.ApplicationDate = data.DateCreatedAt.ToDate(false, PXTimeZoneInfo.FindSystemTimeZoneById(currentBindingExt.OrderTimeZone)).ValueField();
			impl.PaymentAmount = ((decimal)data.Amount).ValueField();
			impl.PaymentRef = (data.Authorization ?? data.Id.ToString()).ValueField();
			impl.BranchID = Branch.PK.Find(this, currentBinding.BranchID)?.BranchCD?.ValueField();
			impl.Hold = false.ValueField();

			BCPaymentMethods methodMapping = GetPaymentMethodMapping(data.Gateway, data.Currency,
				out string paymentMethod, out string cashAcount, out string processingCenter);
			impl.PaymentMethod = paymentMethod?.Trim()?.ValueField();
			impl.CashAccount = cashAcount?.Trim()?.ValueField();
			impl.Description = $"{currentBinding.BindingName} | Order: {bucket.Order?.Extern?.Name} | Type: {data.Kind.ToString()} | Status: {data.Status?.ToString()} | Gateway: {data.Gateway}".ValueField();
			impl.NeedRelease = methodMapping?.ReleasePayments ?? false;

			//Credit Card:
			if (obj.Extern.Authorization != null && processingCenter != null)
			{
				String paymentTran = ParceTransactionNumber(obj.Extern.Authorization);

				impl.IsNewCard = true.ValueField();
				impl.SaveCard = false.ValueField();
				impl.ProcessingCenterID = processingCenter.ValueField();

				CreditCardTransactionDetail detail = new CreditCardTransactionDetail();
				detail.TranNbr = paymentTran.ValueField();
				detail.TranDate = impl.ApplicationDate;
				//detail.ExtProfileId = data.PaymentInstrumentToken.ValueField();
				switch (data.Kind)
				{
					case TransactionType.Authorization:
						detail.TranType = "AUT".ValueField();
						break;
					case TransactionType.Capture:
						detail.TranType = "PAC".ValueField();
						break;
					case TransactionType.Sale:
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
			foreach (SOAdjust adj in PXSelect<SOAdjust,
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

		public virtual BCPaymentMethods GetPaymentMethodMapping(string gateway, string currency, out string paymentMethod, out string cashAccount, out string processingCenter)
		{
			paymentMethod = null;
			cashAccount = null;
			processingCenter = null;

			BCPaymentMethods result = null;
			if (!paymentMethods.Any(x => x.StorePaymentMethod.Equals(gateway.ToUpper(), StringComparison.OrdinalIgnoreCase)))
			{
				PXCache cache = base.Caches[typeof(BCPaymentMethods)];
				BCPaymentMethods newMapping = new BCPaymentMethods()
				{
					BindingID = currentBinding.BindingID,
					StorePaymentMethod = gateway.ToUpper(),
					Active = true,
				};
				newMapping = (BCPaymentMethods)cache.Insert(newMapping);
				cache.Persist(newMapping, PXDBOperation.Insert);

				throw new PXException(BCMessages.OrderPaymentMethodIsMissing, paymentMethod, gateway?.ToUpper(), currency);
			}
			else if (!paymentMethods.Any(x => x.StorePaymentMethod.Equals(gateway.ToUpper(), StringComparison.OrdinalIgnoreCase) && x.Active == true))
			{
				throw new PXException(BCMessages.OrderPaymentMethodIsMissing, paymentMethod, gateway?.ToUpper(), currency);
			}
			else
			{
				result = paymentMethods.FirstOrDefault(x => x.StorePaymentMethod.Equals(gateway.ToUpper(), StringComparison.OrdinalIgnoreCase) && x.Active == true);

				CashAccount ca = null;
				Company baseCurrency = PXSelect<Company, Where<Company.companyCD, Equal<Required<Company.companyCD>>>>.Select(this, this.Accessinfo.CompanyName);
				var multiCurrency = PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.multicurrency>();
				if (baseCurrency?.BaseCuryID?.Trim() != currency && multiCurrency)
				{
					BCMultiCurrencyPaymentMethod bCMultiCurrency = PXSelect<BCMultiCurrencyPaymentMethod, Where<BCMultiCurrencyPaymentMethod.paymentMappingID, Equal<Required<BCPaymentMethods.storePaymentMethod>>,
						And<BCMultiCurrencyPaymentMethod.curyID, Equal<Required<BCMultiCurrencyPaymentMethod.curyID>>,
						And<BCMultiCurrencyPaymentMethod.bindingID, Equal<Required<BCMultiCurrencyPaymentMethod.bindingID>>>>>>.Select(this, result?.PaymentMappingID, currency, result?.BindingID);
					ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, bCMultiCurrency?.CashAccountID);
				}
				else
					ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, result?.CashAccountID);

				paymentMethod = result?.PaymentMethodID;
				cashAccount = ca?.CashAccountCD;
				processingCenter = result?.ProcessingCenterID;

				if (cashAccount == null || paymentMethod == null)
				{
					throw new PXException(BCMessages.OrderPaymentMethodIsMissing, paymentMethod, gateway?.ToUpper(), currency);
				}
				return result;
			}
		}
		public virtual string ParceTransactionNumber(String externalTranId)
		{
			String paymentTran = externalTranId;
			if (!String.IsNullOrWhiteSpace(paymentTran) && paymentTran.IndexOf("#") >= 0)
				paymentTran = paymentTran.Substring(0, paymentTran.IndexOf("#"));

			return paymentTran;
		}

		public override void SaveBucketImport(SPPaymentEntityBucket bucket, IMappedEntity existing, String operation)
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
				impl = cbapi.Invoke<Payment, ReleasePayment>(null, obj.LocalID);
				bucket.Payment.AddLocal(impl, impl.SyncID, impl.SyncTime);
				UpdateStatus(obj, operation);
			}

			if (orderStatus?.LocalID != null) //Payment save updates the order, we need to change the saved timestamp.
			{
				orderStatus.LocalTS = BCSyncExactTimeAttribute.SelectDateTime<SOOrder.lastModifiedDateTime>(orderStatus.LocalID.Value);
				orderStatus = (BCSyncStatus)Statuses.Cache.Update(orderStatus);
			}
		}
		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
		}
		public override EntityStatus GetBucketForExport(SPPaymentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			Payment impl = cbapi.GetByID<Payment>(syncstatus.LocalID);
			if (impl == null) return EntityStatus.None;

			MappedPayment obj = bucket.Payment = bucket.Payment.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Payment, SyncDirection.Export);

			return status;
		}

		public override void MapBucketExport(SPPaymentEntityBucket bucket, IMappedEntity existing)
		{
		}
		public override void SaveBucketExport(SPPaymentEntityBucket bucket, IMappedEntity existing, String operation)
		{
		}
		#endregion
	}
}
