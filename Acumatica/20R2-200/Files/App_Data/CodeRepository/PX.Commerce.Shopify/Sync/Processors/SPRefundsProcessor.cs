using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using System;
using System.Collections.Generic;
using PX.Objects.SO;
using System.Linq;
using PX.Common;
using PX.Objects.AR;
using PX.Api.ContractBased.Models;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.Objects.GL;
using Newtonsoft.Json;

namespace PX.Commerce.Shopify
{
	public class SPRefundsBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary { get => Refunds; }
		public IMappedEntity[] Entities => new IMappedEntity[] { Refunds };
		public override IMappedEntity[] PostProcessors { get => new IMappedEntity[] { Order }; }

		public MappedRefunds Refunds;
		public MappedOrder Order;
	}

	public class SPRefundsRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			return null;
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped)
		{
			return base.Restrict<MappedRefunds>(mapped, delegate (MappedRefunds obj)
			{
				if (obj.Extern != null)
				{
					if (!obj.Extern.Refunds.All(x => x.Transactions.Any(a => a.Kind == TransactionType.Refund && a.Status == TransactionStatus.Success)))
					{
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogRefundSkippedStatus, obj.Extern.Id));
					}
				}

				if (BCSyncStatus.ExternIDIndex.Find((PXGraph)processor, processor.Operation.ConnectorType, processor.Operation.Binding, BCEntitiesAttribute.Order, obj.Extern?.Id.ToString()) == null)
				{
					//Skip if order not synced
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogRefundSkippedOrderNotSynced, obj.Extern.Id));
				}

				return null;
			});
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.OrderRefunds, BCCaptions.Refunds,
		IsInternal = false,
		Direction = SyncDirection.Import,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.SO.SOOrderEntry),
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		DetailTypes = new String[] { BCEntitiesAttribute.CustomerRefundOrder, BCCaptions.CustomerRefundOrder },
		AcumaticaPrimaryType = typeof(PX.Objects.SO.SOOrder),
		AcumaticaPrimarySelect = typeof(Search2<PX.Objects.SO.SOOrder.orderNbr,
			InnerJoin<BCBinding, On<BCBindingExt.orderType, Equal<SOOrder.orderType>>>,
			Where<BCBinding.connectorType, Equal<Current<BCSyncStatusEdit.connectorType>>,
				And<BCBinding.bindingID, Equal<Current<BCSyncStatusEdit.bindingID>>>>>),
		URL = "orders/{0}",
		Requires = new string[] { BCEntitiesAttribute.Order, BCEntitiesAttribute.Payment }
	)]
	[BCProcessorRealtime(PushSupported = false, HookSupported = true,
		 WebHookType = typeof(WebHookMessage),
		WebHooks = new String[]
		{
			"refunds/create"
		})]
	public class SPRefundsProcessor : SPOrderBaseProcessor<SPRefundsProcessor, SPRefundsBucket, MappedRefunds>, IProcessor
	{
		protected List<BCPaymentMethods> paymentMethods;
		protected BCPaymentMethods currentPayment;
		[PXCopyPasteHiddenView]
		public PXSelect<BCPaymentMethods> ExistedPaymentMethods;
		protected OrderRestDataProvider orderDataProvider;
		protected BCBindingShopify currentShopifySettings;

		#region Initialization
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			currentShopifySettings = GetBindingExt<BCBindingShopify>();
			PXView view = new PXView(this, false, ExistedPaymentMethods.View.BqlSelect);
			paymentMethods = view.SelectMulti().Select(x => (BCPaymentMethods)x).Where(x => x.BindingID == currentBinding.BindingID).ToList();
			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());
			orderDataProvider = new OrderRestDataProvider(client);
		}
		#endregion

		#region Pull
		public override MappedRefunds PullEntity(Guid? localID, Dictionary<string, object> fields)
		{
			SalesOrder impl = cbapi.GetByID<SalesOrder>(localID);
			if (impl == null) return null;

			MappedRefunds obj = new MappedRefunds(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override MappedRefunds PullEntity(string externID, string jsonObject)
		{
			dynamic msg = JsonConvert.DeserializeObject(jsonObject);

			string orderId = (string)msg.order_id;
			if (orderId == null) return null;
			var orderData = orderDataProvider.GetByID(orderId);
			if (orderData == null) return null;
			if (orderData.Refunds == null || orderData.Refunds.Count == 0) return null;
			var date = orderData.Refunds.FirstOrDefault(x => x.Id.ToString() == externID)?.DateCreatedAt.ToDate(false);
			if (date == null) return null;
			MappedRefunds obj = new MappedRefunds(orderData, orderData.Id.ToString(), date);

			return obj;
		}
		#endregion

		public override IEnumerable<MappedRefunds> PullSimilar(IExternEntity entity, out string uniqueField)
		{
			uniqueField = ((OrderData)entity)?.Name;
			if (string.IsNullOrEmpty(uniqueField))
				return null;
			uniqueField = APIHelper.ReferenceMake(uniqueField, currentBinding.BindingName);

			SalesOrder[] impls = cbapi.GetAll<SalesOrder>(new SalesOrder() { OrderType = currentBindingExt.OrderType.SearchField(), ExternalRef = uniqueField.SearchField(), Status = new StringReturn() },
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>());
			if (impls == null) return null;

			List<MappedRefunds> result = new List<MappedRefunds>();
			foreach (SalesOrder impl in impls)
			{
				SalesOrder data = cbapi.GetByID<SalesOrder>(impl.SyncID);
				if (data != null)
				{
					result.Add(new MappedRefunds(data, data.SyncID, data.SyncTime));
				}
			}
			return result;
		}

		#region Export

		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{

		}
		public override EntityStatus GetBucketForExport(SPRefundsBucket bucket, BCSyncStatus syncstatus)
		{
			SalesOrder impl = cbapi.GetByID<SalesOrder>(syncstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			bucket.Refunds = bucket.Refunds.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Refunds, SyncDirection.Export);


			return status;
		}

		public override void SaveBucketExport(SPRefundsBucket bucket, IMappedEntity existing, string operation)
		{
		}
		#endregion

		#region Import

		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterOrders filter = new FilterOrders { Status = OrderStatus.Any, Fields = "id,source_name,financial_status,updated_at,created_at,refunds" };
			if (minDateTime != null) filter.UpdatedAtMin = minDateTime.Value.ToLocalTime().AddSeconds(-GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0);
			if (maxDateTime != null) filter.UpdatedAtMax = maxDateTime.Value.ToLocalTime();
			IEnumerable<OrderData> datas = orderDataProvider.GetAll(filter);

			foreach (OrderData orderData in datas)
			{
				if (orderData.Refunds == null || orderData.Refunds.Count == 0) continue;
				SPRefundsBucket bucket = CreateBucket();
				MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedAt.ToDate(false));
				EntityStatus orderStatus = EnsureStatus(order);
				var date = orderData.Refunds.Max(x => x.DateCreatedAt.ToDate(false));
				MappedRefunds obj = bucket.Refunds = bucket.Refunds.Set(orderData, orderData.Id.ToString(), date).With(_ => { _.ParentID = order.SyncID; return _; });
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import, true);

			}
		}
		public override EntityStatus GetBucketForImport(SPRefundsBucket bucket, BCSyncStatus syncstatus)
		{
			OrderData orderData = orderDataProvider.GetByID(syncstatus.ExternID.KeySplit(0).ToString());
			if (orderData == null) return EntityStatus.None;
			EntityStatus status = EntityStatus.None;
			if (orderData.Refunds == null || orderData.Refunds.Count == 0) return status;

			var date = orderData.Refunds.Max(x => x.DateCreatedAt.ToDate(false));
			MappedRefunds obj = bucket.Refunds = bucket.Refunds.Set(orderData, orderData.Id.ToString(), date);
			status = EnsureStatus(obj, SyncDirection.Import);
			MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.DateModifiedAt.ToDate(false));
			EntityStatus orderStatus = EnsureStatus(order);
			return status;
		}
		public override void MapBucketImport(SPRefundsBucket bucket, IMappedEntity existing)
		{
			MappedRefunds obj = bucket.Refunds;
			OrderData orderData = obj.Extern;

			PXResult<PX.Objects.SO.SOOrder, PX.Objects.AR.Customer, PX.Objects.CR.Location, BCSyncStatus> result = PXSelectJoin<PX.Objects.SO.SOOrder,
				InnerJoin<PX.Objects.AR.Customer, On<PX.Objects.AR.Customer.bAccountID, Equal<SOOrder.customerID>>,
				InnerJoin<PX.Objects.CR.Location, On<PX.Objects.CR.Location.locationID, Equal<SOOrder.customerLocationID>>,
				InnerJoin<BCSyncStatus, On<PX.Objects.SO.SOOrder.noteID, Equal<BCSyncStatus.localID>>>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>
				.Select(this, BCEntitiesAttribute.Order, orderData.Id).Select(r => (PXResult<SOOrder, PX.Objects.AR.Customer, PX.Objects.CR.Location, BCSyncStatus>)r).FirstOrDefault();
			if (result == null) throw new PXException(BCMessages.OrderNotSyncronized, orderData.Id);
			PX.Objects.SO.SOOrder order = result.GetItem<PX.Objects.SO.SOOrder>();
			PX.Objects.AR.Customer customer = result.GetItem<PX.Objects.AR.Customer>();
			PX.Objects.CR.Location location = result.GetItem<PX.Objects.CR.Location>();

			if (order?.Status == SOOrderStatus.Open || order?.Status == SOOrderStatus.Hold)
			{
				CreateRefundPaymentAndEditSO(bucket, order, customer, location, existing as MappedRefunds);
			}
			else if (order?.Status == SOOrderStatus.Completed || order?.Status == SOOrderStatus.Invoiced)
			{
				CreateCROrder(bucket, customer, location, existing as MappedRefunds, order);
			}
			else
				throw new PXException(BCMessages.OrderStatusNotValid, orderData.Id);

		}

		public virtual void CreateCROrder(SPRefundsBucket bucket, PX.Objects.AR.Customer customer, Location location, MappedRefunds existing, SOOrder OrignalOrder)
		{
			SalesOrder origOrder = bucket.Refunds.Local = new SalesOrder();
			origOrder.RefundType = RefundType.CashRefund;
			OrderData orderData = bucket.Refunds.Extern;
			List<OrderRefund> refunds = orderData.Refunds;
			origOrder.CROrders = new List<SalesOrder>();
			origOrder.Payment = new List<Payment>();
			var branch = PX.Objects.GL.Branch.PK.Find(this, currentBinding.BranchID)?.BranchCD?.ValueField();
			IEnumerable<PXResult<PX.Objects.AR.ARPayment, BCSyncStatus>> result = PXSelectJoin<PX.Objects.AR.ARPayment,
					InnerJoin<BCSyncStatus, On<PX.Objects.AR.ARPayment.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncStatus.parentSyncID, Equal<Required<BCSyncStatus.parentSyncID>>
					>>>>>.Select(this, BCEntitiesAttribute.Payment, bucket.Refunds.ParentID).Cast<PXResult<PX.Objects.AR.ARPayment, BCSyncStatus>>();
			foreach (OrderRefund data in refunds)
			{
				SalesOrder impl = new SalesOrder();
				// check if refund is already imported as CRPayment
				if (existing != null)
				{
					if (existing?.Details?.Count() > 0)
					{
						if (existing.Details.Any(d => d.EntityType == BCEntitiesAttribute.Payment && d.ExternID.KeySplit(0) == data.Id.ToString())) continue;
					}
					else
					{
						bool synced = false;
						foreach (var trans in data.Transactions.Where(x => x.Status == TransactionStatus.Success))
						{
							var arPayment = result.FirstOrDefault(x => x.GetItem<BCSyncStatus>().ExternID.KeySplit(1) == trans.ParentId.ToString())?.GetItem<ARPayment>();
							if (arPayment?.RefNbr == null) break;
							var existinCRPayment = PXSelectJoin<PX.Objects.AR.ARPayment, InnerJoin<ARAdjust, On<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>>>>,
								Where<ARPayment.docType, Equal<Required<ARPayment.docType>>>>.Select(this, arPayment.RefNbr, ARPaymentType.Refund);
							if (existinCRPayment != null && existinCRPayment.Count > 0)
							{
								var crPayment = existinCRPayment.FirstOrDefault(x => x.GetItem<ARPayment>().ExtRefNbr.Contains(trans.Id.ToString()));
								if (crPayment != null)
								{
									Payment payment = new Payment();
									payment.NoteID = crPayment.GetItem<ARPayment>().NoteID.ValueField();
									payment.TransactionID = new object[] { data.Id, trans.Id.ToString() }.KeyCombine();
									origOrder.Payment.Add(payment);
									synced = true;
								}
								else
									break;
							}
							else
								break;
						}
						if (synced) continue;
					}

				}
				impl.ExternalRef = APIHelper.ReferenceMake(data.Id, currentBinding.BindingName).ValueField();
				//Check if refund is already imported as CR Order
				var existingCR = cbapi.GetAll<SalesOrder>(new SalesOrder()
				{
					OrderType = currentBindingExt.ReturnOrderType.SearchField(),
					ExternalRef = impl.ExternalRef.Value.SearchField(),
					Details = new List<SalesOrderDetail>() { new SalesOrderDetail() { InventoryID = new StringReturn() } },
					DiscountDetails = new List<SalesOrdersDiscountDetails>() { new SalesOrdersDiscountDetails() { ExternalDiscountCode = new StringReturn() } }
				},
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>());
				if (existingCR.Count() > 1)
				{
					throw new PXException(BCMessages.MultipleEntitiesWithUniqueField, BCCaptions.SyncDirectionImport,
						  Connector.GetEntities().First(e => e.EntityType == Operation.EntityType).EntityName, data.Id.ToString());
				}
				var presentCROrder = existingCR?.FirstOrDefault();
				impl.Id = presentCROrder?.Id;

				origOrder.CROrders.Add(impl);

				impl.RefundID = data.Id.ToString();
				impl.OrderType = currentBindingExt.ReturnOrderType.ValueField();
				impl.FinancialSettings = new FinancialSettings();
				impl.FinancialSettings.Branch = branch;
				impl.Date = data.DateCreatedAt.ToDate(false, PXTimeZoneInfo.FindSystemTimeZoneById(currentBindingExt.OrderTimeZone)).ValueField();
				impl.RequestedOn = data.DateCreatedAt.ToDate(false).ValueField();
				impl.Branch = branch;
				impl.PaymentRef = data.Id.ToString().ValueField();
				impl.CustomerID = customer.AcctCD.ValueField();
				impl.LocationID = location.LocationCD.ValueField();
				var desc = $"{currentBinding.BindingName} | {OrignalOrder.OrderNbr}";
				impl.Description = desc.ValueField();
				impl.Details = new List<SalesOrderDetail>();
				impl.Totals = new Totals();
				impl.Totals.OverrideFreightAmount = new BooleanValue() { Value = true };
				List<OrderAdjustment> refundOrderAdjustments = null;
				List<RefundLineItem> refundItems = null;
				refundOrderAdjustments = data.OrderAdjustments;
				refundItems = data.RefundLineItems;

				decimal shippingrefundAmt = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.ShippingRefund)?.Sum(x => (-x.Amount) ?? 0m) ?? 0m;
				decimal shippingrefundAmtTax = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.ShippingRefund)?.Sum(x => (-x.TaxAmount) ?? 0m) ?? 0m;

				impl.ShipVia = OrignalOrder.ShipVia.ValueField();
				impl.Totals.Freight = shippingrefundAmt.ValueField();
				var totalOrderRefundAmout = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.RefundDiscrepancy)?.Sum(y => (y.Amount)) ?? 0;
				//Add arderAdjustments
				if (totalOrderRefundAmout != 0)
				{
					var detail = InsertRefundAmountItem(-totalOrderRefundAmout, impl.Branch);
					if (presentCROrder?.Details != null)
						presentCROrder?.Details.FirstOrDefault(x => x.InventoryID.Value == detail.InventoryID.Value).With(e => detail.Id = e.Id);
					impl.Details.Add(detail);
				}

				var taxes = PXSelect<SOTaxTran, Where<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>,
				And<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>>>>.Select(this, OrignalOrder.OrderType, OrignalOrder.OrderNbr).RowCast<SOTaxTran>();
				if (taxes?.Count() > 0)
				{
					impl.TaxDetails = new List<TaxDetail>();
					if (bucket.Order.Extern.TaxLines?.Count > 0)
					{

						impl.IsTaxValid = true.ValueField();
						foreach (var tax in bucket.Order.Extern.TaxLines)
						{
							//Third parameter set to tax name in order to simplify process (if tax names are equal and user don't want to fill lists)
							String mappedTaxName = GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxSubstitutionListID, tax.TaxName, tax.TaxName);
							mappedTaxName = TrimAutomaticTaxNameForAvalara(mappedTaxName);
							decimal? taxable = 0m;
							if (tax.TaxRate != 0m)
							{
								var refundsItemWithTaxes = refundItems.Where(x => x.TotalTax != 0 && x.OrderLineItem.TaxLines?.Count > 0 && x.OrderLineItem.TaxLines.Any(t => t.TaxAmount > 0m && t.TaxName == tax.TaxName));
								taxable = refundsItemWithTaxes.Sum(x => x.SubTotal ?? 0m);
								var taxAmount = taxable * tax.TaxRate;
								if (bucket.Order.Extern.ShippingLines.Any(x => x.TaxLines?.Count > 0 && x.TaxLines.Any(t => t.TaxAmount > 0m && t.TaxName == tax.TaxName)))
								{
									taxAmount += shippingrefundAmt * tax.TaxRate;
									taxable += shippingrefundAmt;
								}
							}
							TaxDetail inserted = impl.TaxDetails.FirstOrDefault(i => i.TaxID.Value?.Equals(mappedTaxName, StringComparison.InvariantCultureIgnoreCase) == true);
							if (inserted == null)
							{
								if (String.IsNullOrEmpty(mappedTaxName)) throw new PXException(PX.Commerce.Objects.BCObjectsMessages.TaxNameDoesntExist);

								impl.TaxDetails.Add(new TaxDetail()
								{
									TaxID = mappedTaxName.ValueField(),
									TaxAmount = (taxable * tax.TaxRate).ValueField(),
									TaxRate = (tax.TaxRate * 100).ValueField(),
									TaxableAmount = (taxable).ValueField()
								});
							}
							else
							{
								if (inserted.TaxAmount != null)
								{
									inserted.TaxAmount.Value += tax.TaxAmount;
								}
							}
						}
					}
				}
				String[] tooLongTaxIDs = ((impl.TaxDetails ?? new List<TaxDetail>()).Select(x => x.TaxID?.Value).Where(x => (x?.Length ?? 0) > PX.Objects.TX.Tax.taxID.Length).ToArray());
				if (tooLongTaxIDs != null && tooLongTaxIDs.Length > 0)
				{
					throw new PXException(PX.Commerce.Objects.BCObjectsMessages.CannotFindSaveTaxIDs, String.Join(",", tooLongTaxIDs), PX.Objects.TX.Tax.taxID.Length);
				}


				impl.FinancialSettings.OverrideTaxZone = true.ValueField();
				impl.FinancialSettings.CustomerTaxZone = OrignalOrder.TaxZoneID?.ValueField();
				decimal? totalDiscount = 0m;
				if (data.RefundLineItems?.Count > 0)
				{
					totalDiscount = AddSOLine(bucket, impl, data, existing, branch, presentCROrder);
				}

				#region Discounts
				if (currentBindingExt.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount)
				{
					impl.DisableAutomaticDiscountUpdate = true.ValueField();
					impl.DiscountDetails = new List<SalesOrdersDiscountDetails>();

					SalesOrdersDiscountDetails discountDetail = new SalesOrdersDiscountDetails();
					discountDetail.Type = PX.Objects.Common.Discount.DiscountType.ExternalDocument.ValueField();
					discountDetail.DiscountAmount = totalDiscount.ValueField();
					discountDetail.Description = ShopifyMessages.RefundDiscount.ValueField();
					discountDetail.ExternalDiscountCode = ShopifyMessages.RefundDiscount.ValueField();
					impl.DiscountDetails.Add(discountDetail);
					if (presentCROrder != null)
					{
						presentCROrder.DiscountDetails?.ForEach(e => impl.DiscountDetails?.FirstOrDefault(n => n.ExternalDiscountCode.Value == e.ExternalDiscountCode.Value).With(n => n.Id = e.Id));
						impl.DiscountDetails?.AddRange(presentCROrder.DiscountDetails == null ? Enumerable.Empty<SalesOrdersDiscountDetails>()
					: presentCROrder.DiscountDetails.Where(e => impl.DiscountDetails == null || !impl.DiscountDetails.Any(n => e.Id == n.Id)).Select(n => new SalesOrdersDiscountDetails() { Id = n.Id, Delete = true })); ;
					}
				}
				#endregion
			}
		}

		public virtual void CreateRefundPaymentAndEditSO(SPRefundsBucket bucket, SOOrder order, PX.Objects.AR.Customer customer, PX.Objects.CR.Location location, MappedRefunds existing)
		{
			SalesOrder impl = bucket.Refunds.Local = new SalesOrder();
			OrderData orderData = bucket.Refunds.Extern;
			List<OrderRefund> refunds = orderData.Refunds;
			impl.Payment = new List<Payment>();
			var branch = PX.Objects.GL.Branch.PK.Find(this, currentBinding.BranchID)?.BranchCD?.ValueField();
			IEnumerable<PXResult<PX.Objects.AR.ARPayment, BCSyncStatus>> result = PXSelectJoin<PX.Objects.AR.ARPayment,
					InnerJoin<BCSyncStatus, On<PX.Objects.AR.ARPayment.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncStatus.parentSyncID, Equal<Required<BCSyncStatus.parentSyncID>>
					>>>>>.Select(this, BCEntitiesAttribute.Payment, bucket.Refunds.ParentID).Cast<PXResult<PX.Objects.AR.ARPayment, BCSyncStatus>>();


			//	var transactions = refunds.SelectMany(x => x.Transactions).Where(y => y.Kind == TransactionType.Refund && y.Status == TransactionStatus.Success).GroupBy(z => z.ParentId).ToDictionary(x => x.Key, x => x.ToList()); ;
			foreach (var refund in refunds)
			{
				foreach (var trans in refund.Transactions)
				{
					//var trans = data.Value.FirstOrDefault();
					if (trans?.Status == TransactionStatus.Success)
					{
						Payment payment = new Payment();
						payment.Type = PX.Objects.AR.Messages.Refund.ValueField();
						payment.CustomerID = customer.AcctCD.ValueField();
						payment.CustomerLocationID = location.LocationCD.ValueField();
						payment.CurrencyID = order?.CuryID?.ValueField();
						payment.ApplicationDate = order.OrderDate.ValueField();
						payment.PaymentRef = trans.Id.ToString().ValueField();
						payment.BranchID = branch;
						payment.TransactionID = new object[] { refund.Id, trans.Id.ToString() }.KeyCombine();
						currentPayment = null;

						payment.DocumentsToApply = new List<Core.API.PaymentDetail>();

						Core.API.PaymentDetail paymentDetail = new Core.API.PaymentDetail();// do this for multiple transactions
						bool isactive = GetPaymentMethodAndCashAccount(trans.Gateway, trans.Currency, out String cashAcount, out String processingCenter);
						var arPayment = result.FirstOrDefault(x => x.GetItem<BCSyncStatus>().ExternID.KeySplit(1) == trans.ParentId.ToString())?.GetItem<ARPayment>();
						if (arPayment == null || arPayment?.Released != true) throw new PXException(ShopifyMessages.OriginalPaymentNotReleased, trans.ParentId.ToString(), orderData.Id.ToString());
						else if (arPayment != null && !isactive)
						{
							string errorMessage = string.Format(BCMessages.PaymentMethodIsMissing, trans.Gateway, trans.Currency);
							throw new PXException(errorMessage);
						}
						payment.PaymentMethod = currentPayment?.PaymentMethodID?.ValueField();
						payment.CashAccount = cashAcount?.Trim()?.ValueField();
						paymentDetail.ReferenceNbr = arPayment?.RefNbr.ValueField();
						var desc = $"{currentBinding.BindingName} | {order.OrderNbr} | {arPayment?.RefNbr}";
						payment.Description = desc.ValueField();
						paymentDetail.DocType = ARPaymentType.Prepayment.ValueField();
						paymentDetail.AmountPaid = (trans.Amount ?? 0).ValueField();
						payment.PaymentAmount = paymentDetail.AmountPaid;
						payment.DocumentsToApply.Add(paymentDetail);
						if (existing != null)
						{
							if (existing?.Details?.Count() > 0)
							{
								existing?.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.Payment && d.ExternID == payment.TransactionID).With(p => payment.NoteID = p.LocalID.ValueField());
							}
							if (payment.NoteID?.Value == null)
							{
								var existinCRPayment = PXSelectJoin<PX.Objects.AR.ARPayment, InnerJoin<ARAdjust, On<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>>>>,
								Where<ARPayment.docType, Equal<Required<ARPayment.docType>>>>.Select(this, arPayment.RefNbr, ARPaymentType.Refund);
								if (existinCRPayment != null && existinCRPayment.Count > 0)
								{
									var crPayment = existinCRPayment.FirstOrDefault(x => x.GetItem<ARPayment>().ExtRefNbr.Contains(trans.Id.ToString()));
									if (crPayment != null)
										payment.NoteID = crPayment.GetItem<ARPayment>().NoteID.ValueField();
									else if (existinCRPayment.Any(x => x.GetItem<ARPayment>().Released == false))
										throw new PXException(ShopifyMessages.UnreleasedCRPayment, arPayment?.RefNbr, existinCRPayment.FirstOrDefault(x => x.GetItem<ARPayment>().Released == false).GetItem<ARPayment>().RefNbr);
								}
							}
						}
						var existingPayment = impl.Payment.FirstOrDefault(x => x.DocumentsToApply.FirstOrDefault()?.ReferenceNbr?.Value == paymentDetail.ReferenceNbr?.Value && x.NoteID?.Value == null);
						if (existingPayment != null)
						{
							//combine
							existingPayment.DocumentsToApply.FirstOrDefault().AmountPaid = (existingPayment.DocumentsToApply.FirstOrDefault().AmountPaid.Value + (trans.Amount ?? 0)).ValueField();
							existingPayment.PaymentAmount = existingPayment.DocumentsToApply.FirstOrDefault().AmountPaid;
							existingPayment.PaymentRef = new object[] { existingPayment.PaymentRef.Value, trans.Id }.KeyCombine().ValueField();
						}
						else
							impl.Payment.Add(payment);

					}
				}
			}
		}

		public virtual decimal? AddSOLine(SPRefundsBucket bucket, SalesOrder impl, OrderRefund data, MappedRefunds existing, StringValue branch, SalesOrder presentCROrder)
		{
			decimal? totalDiscount = 0m;
			foreach (var item in data.RefundLineItems)
			{
				SalesOrderDetail detail = new SalesOrderDetail();
				String inventoryCD = GetInventoryCDByExternID(item.OrderLineItem.ProductId?.ToString(), item.OrderLineItem.VariantId.ToString(), item.OrderLineItem.Sku, item.OrderLineItem.Name, out string uom);
				if (item.OrderLineItem.DiscountAllocations?.Count > 0)
				{
					var itemDiscount = item.OrderLineItem.DiscountAllocations.Sum(x => x.DiscountAmount);

					itemDiscount = itemDiscount + item.SubTotal - (item.OrderLineItem.Price * item.Quantity);
					totalDiscount += itemDiscount;
					if (currentBindingExt.PostDiscounts == BCPostDiscountAttribute.LineDiscount)
					{
						detail.DiscountAmount = itemDiscount.ValueField();
					}
					else
					{
						detail.DiscountAmount = 0m.ValueField();
					}

				}
				detail.Branch = impl.Branch = branch;
				detail.InventoryID = inventoryCD?.TrimEnd().ValueField();
				detail.OrderQty = ((decimal)item.Quantity).ValueField();
				detail.UOM = uom.ValueField();
				detail.UnitPrice = item.OrderLineItem.Price.ValueField();
				detail.ManualPrice = true.ValueField();
				detail.ReasonCode = currentBindingExt.ReasonCode?.ValueField();
				impl.Details.Add(detail);
				if (existing?.Details?.Count() > 0)
				{
					existing?.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && item.Id.ToString() == d.ExternID.KeySplit(1) && data.Id.ToString() == d.ExternID.KeySplit(0)).With(d => detail.Id = d.LocalID);
				}
				else if (existing?.Details?.Count() == 0 && presentCROrder != null && presentCROrder.Details?.Count > 0)
				{
					presentCROrder.Details.FirstOrDefault(x => x.InventoryID.Value == detail.InventoryID.Value).With(e => detail.Id = e.Id);
				}
			}

			return totalDiscount;
		}

		public override void SaveBucketImport(SPRefundsBucket bucket, IMappedEntity existing, string operation)
		{
			MappedRefunds obj = bucket.Refunds;
			// create CR payment and release it
			SalesOrder order = obj.Local;

			if (order.Payment != null)
			{
				foreach (var payment in order.Payment)
				{
					Payment paymentResp = null;
					Guid? localId = payment.NoteID?.Value;
					if (payment.NoteID?.Value == null)
					{
						paymentResp = cbapi.Put<Payment>(payment);
						localId = paymentResp?.NoteID?.Value;
					}
					if (!obj.Details.Any(x => x.LocalID == payment.NoteID?.Value) && localId != null)
					{
						obj.AddDetail(BCEntitiesAttribute.Payment, localId, payment.TransactionID.ToString());

					}

				}
			}
			if (order.RefundType == RefundType.CustomerRefund)
			{
				UpdateStatus(obj, operation);
				bucket.Order.ExternTimeStamp = DateTime.MaxValue;
				EnsureStatus(bucket.Order, SyncDirection.Import);
			}
			if (order.CROrders != null)
			{
				foreach (var cr in order.CROrders)
				{

					#region Taxes
					//Logging for taxes
					LogTaxDetails(obj.SyncID, cr);
					#endregion

					SalesOrder impl = cbapi.Put<SalesOrder>(cr, cr.Id);
					#region Taxes
					ValidateTaxes(obj.SyncID, impl, cr);
					#endregion
					if (!obj.Details.Any(x => x.LocalID == impl.Id))
					{
						obj.AddDetail(BCEntitiesAttribute.CustomerRefundOrder, impl.Id, cr.RefundID);
					}
					foreach (var lineitem in impl.Details)
					{
						if (!obj.Details.Any(x => x.LocalID == lineitem.NoteID.Value))
						{
							if (lineitem.InventoryID.Value.Trim() == refundItem.InventoryCD.Trim())
								continue;
							else
							{
								var detail = obj.Extern.Refunds.FirstOrDefault(x => x.Id.ToString() == cr.RefundID).RefundLineItems.FirstOrDefault(x => !obj.Details.Any(o => x.Id.ToString() == o.ExternID)
							 && x.OrderLineItem.Sku == lineitem.InventoryID.Value);
								if (detail != null)
									obj.AddDetail(BCEntitiesAttribute.OrderLine, lineitem.NoteID.Value, new object[] { cr.RefundID, detail.Id }.KeyCombine());
								else
									throw new PXException(BCMessages.CannotMapLines);
							}
						}

					}
				}
				UpdateStatus(obj, operation);
				bucket.Order = null;
			}
		}

		public virtual bool GetPaymentMethodAndCashAccount(string gateway, string currency, out string cashAccount, out string processingCenter)
		{
			cashAccount = null;
			processingCenter = null;
			if (!paymentMethods.Any(x => x.StorePaymentMethod.Equals(gateway.ToUpper(), StringComparison.OrdinalIgnoreCase)))
			{
				BCPaymentMethods newMapping = new BCPaymentMethods()
				{
					BindingID = currentBinding.BindingID,
					StorePaymentMethod = gateway.ToUpper(),
					Active = true,
				};
				newMapping = ExistedPaymentMethods.Insert(newMapping);
				ExistedPaymentMethods.Cache.Persist(newMapping, PXDBOperation.Insert);
				return false;
			}
			else if (!paymentMethods.Any(x => x.StorePaymentMethod.Equals(gateway.ToUpper(), StringComparison.OrdinalIgnoreCase) && x.Active == true))
			{
				return false;
			}
			else
			{
				currentPayment = paymentMethods.FirstOrDefault(x => x.StorePaymentMethod.Equals(gateway.ToUpper(), StringComparison.OrdinalIgnoreCase) && x.Active == true);

				CashAccount ca = null;
				Company baseCurrency = PXSelect<Company, Where<Company.companyCD, Equal<Required<Company.companyCD>>>>.Select(this, this.Accessinfo.CompanyName);
				var multiCurrency = PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.multicurrency>();
				if (baseCurrency?.BaseCuryID?.Trim() != currency && multiCurrency)
				{
					BCMultiCurrencyPaymentMethod bCMultiCurrency = PXSelect<BCMultiCurrencyPaymentMethod, Where<BCMultiCurrencyPaymentMethod.paymentMappingID, Equal<Required<BCPaymentMethods.storePaymentMethod>>,
						And<BCMultiCurrencyPaymentMethod.curyID, Equal<Required<BCMultiCurrencyPaymentMethod.curyID>>,
						And<BCMultiCurrencyPaymentMethod.bindingID, Equal<Required<BCMultiCurrencyPaymentMethod.bindingID>>>>>>.Select(this, currentPayment?.PaymentMappingID, currency, currentPayment?.BindingID);
					ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, bCMultiCurrency?.CashAccountID);
				}
				else
					ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, currentPayment?.CashAccountID);

				cashAccount = ca?.CashAccountCD;
				processingCenter = currentPayment?.ProcessingCenterID;

				if (cashAccount == null) return false;
				return true;
			}
		}

		#endregion

	}
}
