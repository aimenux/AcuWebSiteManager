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
using PX.Objects.PO;
using PX.Data.BQL.Fluent;
using PX.Api.ContractBased.Models;

namespace PX.Commerce.Shopify
{
	public class SPShipmentEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Shipment;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary }.Concat(Orders).ToArray();

		public MappedShipment Shipment;
		public List<MappedOrder> Orders = new List<MappedOrder>();
	}

	public class SPShipmentsRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			#region Shipments
			return base.Restrict<MappedShipment>(mapped, delegate (MappedShipment obj)
			{
				if (obj.Local != null)
				{
					if (obj.Local.Confirmed?.Value == false)
					{
						return new FilterResult(FilterStatus.Invalid,
								PXMessages.Localize(BCMessages.LogShipmentSkippedNotConfirmed));
					}

					if (obj.Local?.Shipment != null)
					{
						BCBindingExt binding = processor.GetBindingExt<BCBindingExt>();

						Boolean anyFound = false;
						foreach (ShipmentOrderDetail detail in obj.Local?.Shipment.Orders)
						{
							if (detail.OrderType?.Value != binding.OrderType) continue;
							if (processor.SelectStatus(BCEntitiesAttribute.Order, detail.OrderNoteID?.Value) == null) continue;

							anyFound = true;
						}
						if (!anyFound)
						{
							return new FilterResult(FilterStatus.Invalid,
								PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogShipmentSkippedNoOrder, obj.Local.Shipment.ShipmentNbr?.Value ?? obj.Local.SyncID.ToString()));
						}
					}

					if (obj.Local?.POReceipt != null)
					{
						BCBindingExt binding = processor.GetBindingExt<BCBindingExt>();

						Boolean anyFound = false;
						foreach (PurchaseReceiptDetail detail in obj.Local?.POReceipt.Details)
						{
							if (detail.SOOrderType?.Value != binding.OrderType) continue;
							if (processor.SelectStatus(BCEntitiesAttribute.Order, detail.SONoteID?.Value) == null) continue;

							anyFound = true;
						}
						if (!anyFound)
						{
							return new FilterResult(FilterStatus.Invalid,
								PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogShipmentSkippedNoOrder, obj.Local.POReceipt.ShipmentNbr?.Value ?? obj.Local.SyncID.ToString()));
						}
					}
				}

				return null;
			});
			#endregion
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped)
		{
			return null;
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Shipment, BCCaptions.Shipment,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { typeof(FulfillmentData) },
		LocalTypes = new Type[] { typeof(BCShipments) },
		DetailTypes = new String[] { BCEntitiesAttribute.ShipmentLine, BCCaptions.ShipmentLine, BCEntitiesAttribute.ShipmentBoxLine, BCCaptions.ShipmentLineBox },
		AcumaticaPrimaryType = typeof(PX.Objects.SO.SOShipment),
		AcumaticaPrimarySelect = typeof(PX.Objects.SO.SOShipment.shipmentNbr),
		URL = "orders/{0}",
		Requires = new string[] { BCEntitiesAttribute.Order }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Shipments" })]
	public class SPShipmentProcessor : BCProcessorSingleBase<SPShipmentProcessor, SPShipmentEntityBucket, MappedShipment>, IProcessor
	{
		protected OrderRestDataProvider orderDataProvider;
		protected FulfillmentRestDataProvider fulfillmentDataProvider;

		protected List<BCShippingMappings> shippingMappings;
		protected BCBinding currentBinding;
		protected BCBindingExt currentBindingExt;
		protected BCBindingShopify currentShopifySettings;
		private long? shopifyLocationId;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			currentBinding = GetBinding();
			currentBindingExt = GetBindingExt<BCBindingExt>();
			currentShopifySettings = GetBindingExt<BCBindingShopify>();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			orderDataProvider = new OrderRestDataProvider(client);
			fulfillmentDataProvider = new FulfillmentRestDataProvider(client);

			shippingMappings = PXSelectReadonly<BCShippingMappings,
				Where<BCShippingMappings.bindingID, Equal<Required<BCShippingMappings.bindingID>>>>
				.Select(this, Operation.Binding).Select(x => x.GetItem<BCShippingMappings>()).ToList();
			var inventoryLocations = ConnectorHelper.GetConnector(currentBinding.ConnectorType)?.GetExternalInfo<InventoryLocationData>(BCObjectsConstants.BCInventoryLocation, currentBinding.BindingID);
			if (inventoryLocations == null || inventoryLocations.Count() == 0)
			{
				throw new PXException(ShopifyMessages.InventoryLocationNotFound);
			}
			else
				shopifyLocationId = inventoryLocations.First().Id;
		}
		#endregion

		public override void NavigateLocal(IConnector connector, ISyncStatus status)
		{
			SOOrderShipment orderShipment = PXSelect<SOOrderShipment, Where<SOOrderShipment.shippingRefNoteID, Equal<Required<SOOrderShipment.shippingRefNoteID>>>>.Select(this, status?.LocalID);
			if (orderShipment.ShipmentType == SOShipmentType.DropShip)//dropshipment
			{
				POReceiptEntry extGraph = PXGraph.CreateInstance<POReceiptEntry>();
				EntityHelper helper = new EntityHelper(extGraph);
				helper.NavigateToRow(extGraph.GetPrimaryCache().GetItemType().FullName, status.LocalID, PXRedirectHelper.WindowMode.NewWindow);

			}
			if (orderShipment.ShipmentType == SOShipmentType.Issue && orderShipment.ShipmentNoteID == null) //Invoice
			{
				POReceiptEntry extGraph = PXGraph.CreateInstance<POReceiptEntry>();
				EntityHelper helper = new EntityHelper(extGraph);
				helper.NavigateToRow(extGraph.GetPrimaryCache().GetItemType().FullName, status.LocalID, PXRedirectHelper.WindowMode.NewWindow);

			}
			else//shipment
			{
				SOShipmentEntry extGraph = PXGraph.CreateInstance<SOShipmentEntry>();
				EntityHelper helper = new EntityHelper(extGraph);
				helper.NavigateToRow(extGraph.GetPrimaryCache().GetItemType().FullName, status.LocalID, PXRedirectHelper.WindowMode.NewWindow);

			}

		}

		#region Pull
		public override MappedShipment PullEntity(Guid? localID, Dictionary<string, object> externalInfo)
		{
			BCShipments giResult = cbapi.Put<BCShipments>(new BCShipments() { OrderType = currentBindingExt.OrderType.ValueField(), ShippingNoteID = localID.ValueField() });
			var result = giResult?.Results.FirstOrDefault();
			if (result == null) return null;
			GetOrderShipment(result, giResult);
			if (giResult.Shipment == null && giResult.POReceipt == null) return null;
			MapFilterFields(result, giResult);
			MappedShipment obj = new MappedShipment(giResult, result.NoteID.Value, result.LastModifiedDateTime.Value);
			return obj;


		}
		public override MappedShipment PullEntity(String externID, String externalInfo)
		{
			FulfillmentData data = fulfillmentDataProvider.GetByID(externID.KeySplit(0), externID.KeySplit(1));
			if (data == null) return null;

			MappedShipment obj = new MappedShipment(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedAt.ToDate(false), data.CalculateHash());

			return obj;
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
		}
		public override EntityStatus GetBucketForImport(SPShipmentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			bucket.Shipment = bucket.Shipment.Set(new FulfillmentData(), syncstatus.ExternID, syncstatus.ExternTS);

			return EntityStatus.None;
		}

		public override void MapBucketImport(SPShipmentEntityBucket bucket, IMappedEntity existing)
		{
		}
		public override void SaveBucketImport(SPShipmentEntityBucket bucket, IMappedEntity existing, String operation)
		{
		}
		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			BCShipments giResult = cbapi.Put<BCShipments>(new BCShipments()
			{
				OrderType = currentBindingExt.OrderType.ValueField(),
				LastModifiedDateTime = minDateTime.ValueField()
			});

			if (giResult.Results != null)
			{
				foreach (BCShipmentsResult result in giResult.Results)
				{
					MapFilterFields(result, giResult);

					MappedShipment obj = new MappedShipment(giResult, result.NoteID.Value, result.LastModifiedDateTime.Value);
					EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				}
			}
		}

		private void MapFilterFields(BCShipmentsResult result, BCShipments impl)
		{
			impl.ShippingNoteID = result.NoteID;
			impl.OrderType = currentBindingExt.OrderType.ValueField();
			impl.OrderNbr = result.OrderNbr;
			impl.ShipmentNumber = result.ShipmentNumber;
			impl.ShipmentType = result.ShipmentType;
			impl.Confirmed = result.Confirmed;
		}

		public override EntityStatus GetBucketForExport(SPShipmentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			SOOrderShipments impl = new SOOrderShipments();
			BCShipments giResult = cbapi.Put<BCShipments>(new BCShipments()
			{
				OrderType = currentBindingExt.OrderType.ValueField(),
				ShippingNoteID = syncstatus.LocalID.ValueField(),
				Results = new List<BCShipmentsResult>() { new BCShipmentsResult() { Custom = GetCustomFieldsForExport() } }
			});
			var result = giResult?.Results?.FirstOrDefault();
			if (result == null) return EntityStatus.None;

			MapFilterFields(result, giResult);
			if (result.ShipmentType.Value == SOShipmentType.DropShip)
			{
				return GetDropShipment(bucket, result, giResult);
			}
			else if (result.ShipmentType.Value == SOShipmentType.Invoice)
			{
				return GetInvoice(bucket, result, giResult);
			}
			else
			{
				return GetShipment(bucket, result, giResult);
			}
		}


		public override void MapBucketExport(SPShipmentEntityBucket bucket, IMappedEntity existing)
		{
			MappedShipment obj = bucket.Shipment;
			if (obj.Local?.Confirmed?.Value == false) throw new PXException(BCMessages.ShipmentNotConfirmed);
			if (obj.Local.ShipmentType.Value == SOShipmentType.DropShip)
			{
				PurchaseReceipt impl = obj.Local.POReceipt;
				MapDropShipment(bucket, obj, impl);
			}
			else if (obj.Local.ShipmentType.Value == SOShipmentType.Issue)
			{
				Shipment impl = obj.Local.Shipment;
				MapShipment(bucket, obj, impl);
			}
			else
			{
				FulfillmentData shipmentData = obj.Extern = new FulfillmentData();
			}
		}

		public override CustomField GetLocalCustomField(SPShipmentEntityBucket bucket, string viewName, string fieldName)
		{
			MappedShipment obj = bucket.Shipment;
			BCShipments impl = obj.Local;
			if (impl?.Results?.Count() > 0)
				return impl.Results[0].Custom?.Where(x => x.ViewName == viewName && x.FieldName == fieldName).FirstOrDefault();
			else return null;
		}

		public override void SaveBucketExport(SPShipmentEntityBucket bucket, IMappedEntity existing, String operation)
		{
			MappedShipment obj = bucket.Shipment;

			StringBuilder key = new StringBuilder();
			String origExternId = obj.ExternID;
			var ShipmentItems = obj.Extern.LineItems;
			string errorMsg = string.Empty;
			List<DetailInfo> existingDetails = new List<DetailInfo>(obj.Details);
			if (existingDetails != null)
			{
				if (ShipmentItems.All(x => x.PackageId != null))
				{
					foreach (var detail in existingDetails)
					{
						CancelFullfillment(bucket, detail);
					}
				}
				else
				{
					foreach (var detail in existingDetails)
					{
						bool orderExist = bucket.Orders.Any(x => x.LocalID == detail.LocalID);
						if (detail.EntityType == BCEntitiesAttribute.ShipmentBoxLine || !orderExist)
						{
							CancelFullfillment(bucket, detail);
						}
					}
				}
			}
			obj.ClearDetails();
			foreach (MappedOrder order in bucket.Orders)
			{
				FulfillmentData ordersShipmentData = new FulfillmentData();
				ordersShipmentData = obj.Extern;
				ordersShipmentData.LineItems = ShipmentItems.Where(x => x.OrderId == order.ExternID.ToLong())?.ToList();

				String foundExternalID = null;
				try
				{

					if (ordersShipmentData.LineItems.All(x => x.PackageId != null))
					{
						ordersShipmentData.TrackingNumbers.Clear();
						var packages = ordersShipmentData.LineItems.GroupBy(x => x.PackageId).ToDictionary(x => x.Key, x => x.ToList());
						var externOrder = orderDataProvider.GetByID(order.ExternID, false, true);

						foreach (var package in packages)
						{
							ordersShipmentData.LineItems = package.Value;
							var trackingNumber = obj.Local.Shipment.Packages.FirstOrDefault(x => x.NoteID?.Value == package.Key)?.TrackingNbr?.Value;
							if (!string.IsNullOrEmpty(trackingNumber)) ordersShipmentData.TrackingNumbers.Add(trackingNumber);
							//Check the fulfillments in the extern order
							foundExternalID = existingDetails.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.ShipmentBoxLine && d.LocalID == package.Key)?.ExternID;
							foundExternalID = foundExternalID.HasParent() ? foundExternalID.KeySplit(1) : foundExternalID;
							FulfillmentData data = SaveFullfillment(order, externOrder, ordersShipmentData, foundExternalID);
							obj.With(_ => { _.ExternID = null; return _; }).AddExtern(data, new object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedAt.ToDate(false));
							obj.AddDetail(BCEntitiesAttribute.ShipmentBoxLine, order.LocalID, new object[] { data.OrderId, data.Id }.KeyCombine());
							key.Append(key.Length > 0 ? "|" + obj.ExternID : obj.ExternID);
							ordersShipmentData.TrackingNumbers.Clear();
						}
					}
					else
					{
						var externOrder = orderDataProvider.GetByID(order.ExternID, false, true);
						foundExternalID = existingDetails.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.ShipmentLine && d.LocalID == order.LocalID)?.ExternID;
						foundExternalID = foundExternalID.HasParent() ? foundExternalID.KeySplit(1) : foundExternalID;
						//Check the fulfillments in the extern order
						FulfillmentData data = SaveFullfillment(order, externOrder, ordersShipmentData, foundExternalID);
						obj.With(_ => { _.ExternID = null; return _; }).AddExtern(data, new object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedAt.ToDate(false));
						obj.AddDetail(BCEntitiesAttribute.ShipmentLine, order.LocalID, new object[] { data.OrderId, data.Id }.KeyCombine());
						key.Append(key.Length > 0 ? "|" + obj.ExternID : obj.ExternID);

					}
				}
				catch (Exception ex)
				{
					Log(bucket?.Primary?.SyncID, SyncDirection.Export, ex);
					errorMsg += $"{ex.InnerException?.Message ?? ex.Message} \n";
				}

			}



			obj.ExternID = key.ToString()?.TrimExternID();
			if (!string.IsNullOrEmpty(errorMsg))
				UpdateStatus(obj, BCSyncOperationAttribute.ExternFailed, errorMsg);
			else
				UpdateStatus(obj, operation);
		}

		private void CancelFullfillment(SPShipmentEntityBucket bucket, DetailInfo detail)
		{
			if (detail.ExternID.HasParent())
			{
				try
				{
					fulfillmentDataProvider.CancelFulfillment(detail.ExternID.KeySplit(0), detail.ExternID.KeySplit(1));
				}
				catch (Exception ex)
				{
					Log(bucket?.Primary?.SyncID, SyncDirection.Export, ex);
				}
			}

		}

		private FulfillmentData SaveFullfillment(MappedOrder order, OrderData externOrder, FulfillmentData ordersShipmentData, string foundExternalID)
		{
			//if (externOrder != null && externOrder.FinancialStatus == OrderFinancialStatus.Authorized && externOrder.Transactions?.Count > 0)
			//{
			//	//If Order financial status is Authorized, we should change to Capture first before sending Shipments.
			//	OrderTransaction transaction = new OrderTransaction();
			//	transaction.Kind = TransactionType.Capture;
			//	transaction.Authorization = externOrder.Transactions.FirstOrAny(x => x.Kind == TransactionType.Authorization && x.Status == TransactionStatus.Success)?.Authorization;
			//	orderDataProvider.PostPaymentToCapture(transaction, order.ExternID);
			//}
			if (externOrder?.Fulfillments != null && externOrder.Fulfillments.Where(x => x.Status != FulfillmentStatus.Cancelled)?.ToList()?.Count > 0)
			{
				if (foundExternalID == null && externOrder.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled)
				{
					foreach (var oneItem in externOrder.Fulfillments)
					{
						//If we cannot find the External Fulfillment ID but it existed in Shopify, we should match and update it.
						if (oneItem.Status != FulfillmentStatus.Cancelled && oneItem.LineItems.All(x => ordersShipmentData.LineItems.Any(item => item.Id == x.Id && item.Quantity == x.Quantity)))
						{
							foundExternalID = oneItem.Id.ToString();
							break;
						}
					}
					if (foundExternalID == null)
						throw new PXException(ShopifyMessages.OrderShipmentNotCreated, externOrder.Name);
				}
				else if (foundExternalID == null && externOrder.FulfillmentStatus == OrderFulfillmentStatus.Partial)
				{
					//Have to check unfulfill items' quantity, if it's less than the quantity in the fulfillment items, should throe an error
					var QtyByItem = externOrder.LineItems.ToDictionary(x => x.Id.ToString(), x => x.Quantity ?? 0);

					if (externOrder.Refunds?.Count > 0)
					{
						externOrder.Refunds.SelectMany(x => x.RefundLineItems).GroupBy(x => x.LineItemId).ForEach(x =>
						{
							if (QtyByItem.ContainsKey(x.Key.ToString()))
							{
								QtyByItem[x.Key.ToString()] = QtyByItem[x.Key.ToString()] - x.Sum(s => s.Quantity ?? 0);
							}
						});
					}
					externOrder.Fulfillments.Where(x => x.Status == FulfillmentStatus.Success).SelectMany(x => x.LineItems).GroupBy(x => x.Id).ForEach(x =>
					{
						if (QtyByItem.ContainsKey(x.Key.ToString()))
						{
							QtyByItem[x.Key.ToString()] = QtyByItem[x.Key.ToString()] - x.Sum(s => s.Quantity ?? 0);
						}
					});
					ordersShipmentData.LineItems.ForEach(x =>
					{
						//Check the qty whether match the unfulfilled qty
						if (QtyByItem.TryGetValue(x.Id.ToString(), out var leftQty) && leftQty < x.Quantity)
						{
							throw new PXException(ShopifyMessages.ShipmentQtyNotMatch, x.Quantity, x.Sku, leftQty, externOrder.Name);
						}
					});
				}

			}
			FulfillmentData data = null;
			if (foundExternalID != null)
			{
				data = fulfillmentDataProvider.Update(ordersShipmentData, order.ExternID, foundExternalID);
			}
			else
				data = fulfillmentDataProvider.Create(ordersShipmentData, order.ExternID);
			return data;
		}

		protected virtual void GetOrderShipment(BCShipmentsResult result, BCShipments bCShipments)
		{
			if (result.ShipmentType.Value == SOShipmentType.DropShip)
				GetDropShipmentByShipmentNbr(result, bCShipments);
			else
				bCShipments.Shipment = cbapi.GetByID<Shipment>(result.NoteID.Value);

		}

		protected virtual void GetDropShipmentByShipmentNbr(BCShipmentsResult result, BCShipments bCShipments)
		{
			bCShipments.POReceipt = new PurchaseReceipt();
			bCShipments.POReceipt.ShipmentNbr = result.ShipmentNumber;
			bCShipments.POReceipt.VendorRef = result.VendorRef;
			bCShipments.POReceipt.Details = new List<PurchaseReceiptDetail>();

			foreach (PXResult<SOLineSplit, POOrder, SOOrder> item in PXSelectJoin<SOLineSplit,
				InnerJoin<POOrder, On<POOrder.orderNbr, Equal<SOLineSplit.pONbr>>,
				InnerJoin<SOOrder, On<SOLineSplit.orderNbr, Equal<SOOrder.orderNbr>>>>,
				Where<SOLineSplit.pOReceiptNbr, Equal<Required<SOLineSplit.pOReceiptNbr>>>>
			.Select(this, result.ShipmentNumber.Value))
			{
				SOLineSplit lineSplit = item.GetItem<SOLineSplit>();
				SOOrder line = item.GetItem<SOOrder>();
				POOrder poOrder = item.GetItem<POOrder>();
				PurchaseReceiptDetail detail = new PurchaseReceiptDetail();
				detail.SOOrderNbr = lineSplit.OrderNbr.ValueField();
				detail.SOLineNbr = lineSplit.LineNbr.ValueField();
				detail.SOOrderType = lineSplit.OrderType.ValueField();
				detail.ReceiptQty = lineSplit.ShippedQty.ValueField();
				detail.ShipVia = poOrder.ShipVia.ValueField();
				detail.SONoteID = line.NoteID.ValueField();
				bCShipments.POReceipt.Details.Add(detail);
			}
		}

		protected virtual EntityStatus GetDropShipment(SPShipmentEntityBucket bucket, BCShipmentsResult result, BCShipments bCShipments)
		{
			GetDropShipmentByShipmentNbr(result, bCShipments);
			if (bCShipments.POReceipt == null || bCShipments.POReceipt?.Details?.Count == 0)
				return EntityStatus.None;

			MappedShipment obj = bucket.Shipment = bucket.Shipment.Set(bCShipments, result.NoteID.Value, result.LastModifiedDateTime.Value);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			IEnumerable<PurchaseReceiptDetail> lines = bCShipments.POReceipt.Details
				.GroupBy(r => new { OrderType = r.SOOrderType.Value, OrderNbr = r.SOOrderNbr.Value })
				.Select(r => r.First());
			foreach (PurchaseReceiptDetail line in lines)
			{
				SalesOrder orderImpl = cbapi.Get<SalesOrder>(new SalesOrder() { OrderType = line.SOOrderType.Value.SearchField(), OrderNbr = line.SOOrderNbr.Value.SearchField() });
				if (orderImpl == null) throw new PXException(BCMessages.OrderNotFound, bCShipments.POReceipt.ShipmentNbr.Value);

				MappedOrder orderObj = new MappedOrder(orderImpl, orderImpl.SyncID, orderImpl.SyncTime);
				EntityStatus orderStatus = EnsureStatus(orderObj);

				if (orderObj.ExternID == null) throw new PXException(BCMessages.OrderNotSyncronized, orderImpl.OrderNbr.Value);

				bucket.Orders.Add(orderObj);
			}
			return status;
		}
		protected virtual EntityStatus GetShipment(SPShipmentEntityBucket bucket, BCShipmentsResult result, BCShipments bCShipment)
		{
			bCShipment.Shipment = cbapi.GetByID<Shipment>(result.NoteID.Value);
			if (bCShipment.Shipment == null || bCShipment.Shipment?.Details?.Count == 0)
				return EntityStatus.None;

			MappedShipment obj = bucket.Shipment = bucket.Shipment.Set(bCShipment, result.NoteID.Value, result.LastModifiedDateTime.Value);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			IEnumerable<ShipmentDetail> lines = bCShipment.Shipment.Details
				.GroupBy(r => new { OrderType = r.OrderType.Value, OrderNbr = r.OrderNbr.Value })
				.Select(r => r.First());
			foreach (ShipmentDetail line in lines)
			{
				SalesOrder orderImpl = cbapi.Get<SalesOrder>(new SalesOrder() { OrderType = line.OrderType.Value.SearchField(), OrderNbr = line.OrderNbr.Value.SearchField() });
				if (orderImpl == null) throw new PXException(BCMessages.OrderNotFound, bCShipment.Shipment.ShipmentNbr.Value);
				MappedOrder orderObj = new MappedOrder(orderImpl, orderImpl.SyncID, orderImpl.SyncTime);
				EntityStatus orderStatus = EnsureStatus(orderObj);

				if (orderObj.ExternID == null) throw new PXException(BCMessages.OrderNotSyncronized, orderImpl.OrderNbr.Value);

				bucket.Orders.Add(orderObj);
			}
			return status;
		}
		protected virtual EntityStatus GetInvoice(SPShipmentEntityBucket bucket, BCShipmentsResult result, BCShipments bCShipment)
		{
			bCShipment.Shipment = new Shipment();
			bCShipment.ShipmentNumber = result.ShipmentNumber;
			bCShipment.Shipment.Details = new List<ShipmentDetail>();

			foreach (PXResult<ARTran, SOOrder> item in PXSelectJoin<ARTran, InnerJoin<SOOrder, On<ARTran.sOOrderNbr, Equal<SOOrder.orderNbr>>>,
			Where<ARTran.refNbr, Equal<Required<ARTran.refNbr>>, And<ARTran.sOOrderType, Equal<Required<ARTran.sOOrderType>>>>>
			.Select(this, result.ShipmentNumber.Value, bCShipment.OrderType.Value))
			{
				ARTran line = item.GetItem<ARTran>();
				ShipmentDetail detail = new ShipmentDetail();
				detail.OrderNbr = line.SOOrderNbr.ValueField();
				detail.OrderLineNbr = line.SOOrderLineNbr.ValueField();
				detail.OrderType = line.SOOrderType.ValueField();
				bCShipment.Shipment.Details.Add(detail);
			}
			if (bCShipment.Shipment?.Details?.Count == 0) return EntityStatus.None;

			MappedShipment obj = bucket.Shipment = bucket.Shipment.Set(bCShipment, result.NoteID.Value, result.LastModifiedDateTime.Value);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			IEnumerable<ShipmentDetail> lines = bCShipment.Shipment.Details
				.GroupBy(r => new { OrderType = r.OrderType.Value, OrderNbr = r.OrderNbr.Value })
				.Select(r => r.First());
			foreach (ShipmentDetail line in lines)
			{
				SalesOrder orderImpl = cbapi.Get<SalesOrder>(new SalesOrder() { OrderType = line.OrderType.Value.SearchField(), OrderNbr = line.OrderNbr.Value.SearchField() });
				if (orderImpl == null) throw new PXException(BCMessages.OrderNotFound, bCShipment.Shipment.ShipmentNbr.Value);
				MappedOrder orderObj = new MappedOrder(orderImpl, orderImpl.SyncID, orderImpl.SyncTime);
				EntityStatus orderStatus = EnsureStatus(orderObj);

				if (orderObj.ExternID == null) throw new PXException(BCMessages.OrderNotSyncronized, orderImpl.OrderNbr.Value);

				bucket.Orders.Add(orderObj);
			}
			return status;
		}

		protected virtual void MapDropShipment(SPShipmentEntityBucket bucket, MappedShipment obj, PurchaseReceipt impl)
		{
			FulfillmentData shipmentData = obj.Extern = new FulfillmentData();
			shipmentData.LineItems = new List<OrderLineItem>();
			shipmentData.LocationId = shopifyLocationId;
			var shipvia = impl.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.ShipVia?.Value))?.ShipVia?.Value ?? string.Empty;
			shipmentData.TrackingCompany = GetCarrierName(shipvia);
			shipmentData.TrackingNumbers = new List<string>() { impl.VendorRef?.Value };

			foreach (MappedOrder order in bucket.Orders)
			{
				foreach (PurchaseReceiptDetail line in impl.Details ?? new List<PurchaseReceiptDetail>())
				{
					SalesOrderDetail orderLine = order.Local.Details.FirstOrDefault(d =>
						order.Local.OrderType.Value == line.SOOrderType.Value && order.Local.OrderNbr.Value == line.SOOrderNbr.Value && d.LineNbr.Value == line.SOLineNbr.Value);
					if (orderLine == null) continue; //skip shipment that is not from this order

					DetailInfo lineInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && d.LocalID == orderLine.NoteID.Value);
					if (lineInfo == null) throw new PXException(BCMessages.OrderShippingLineSyncronized, order.Local.OrderNbr.Value);

					OrderLineItem shipItem = new OrderLineItem();
					shipItem.Id = lineInfo.ExternID.ToLong();
					shipItem.Quantity = (int)line.ReceiptQty.Value;
					shipItem.OrderId = order.ExternID.ToLong();

					shipmentData.LineItems.Add(shipItem);
				}
			}
		}
		protected virtual void MapShipment(SPShipmentEntityBucket bucket, MappedShipment obj, Shipment impl)
		{
			FulfillmentData shipmentData = obj.Extern = new FulfillmentData();
			shipmentData.LineItems = new List<OrderLineItem>();
			shipmentData.LocationId = shopifyLocationId;
			shipmentData.TrackingCompany = GetCarrierName(impl.ShipVia?.Value ?? string.Empty);
			shipmentData.TrackingNumbers = new List<string>();
			var PackageDetails = PXSelect<SOShipLineSplitPackage,
			Where<SOShipLineSplitPackage.shipmentNbr, Equal<Required<SOShipLineSplitPackage.shipmentNbr>>
			>>.Select(this, impl.ShipmentNbr?.Value).RowCast<SOShipLineSplitPackage>().ToList();
			var packages = impl.Packages ?? new List<ShipmentPackage>();
			if (packages.Count == 1)
			{
				var trackingNumber = packages.FirstOrDefault()?.TrackingNbr?.Value;
				if (!string.IsNullOrEmpty(trackingNumber)) shipmentData.TrackingNumbers.Add(trackingNumber);
			}
			else
				foreach (ShipmentPackage package in packages)
				{
					var detail = PackageDetails.Where(x => x.PackageLineNbr == package.LineNbr?.Value && x.PackedQty != 0)?.ToList() ?? new List<SOShipLineSplitPackage>();
					if (detail.Count == 0)
						throw new PXException(BCMessages.BoxesWithoutItems, impl.ShipmentNbr.Value, package.TrackingNbr?.Value);
					package.ShipmentLineNbr.AddRange(detail.Select(x => new Tuple<int?, decimal?>(x.ShipmentLineNbr, x.PackedQty)));
				}


			foreach (MappedOrder order in bucket.Orders)
			{
				foreach (ShipmentDetail line in impl.Details ?? new List<ShipmentDetail>())
				{
					List<ShipmentPackage> details = null;
					if (packages.Count > 1)
					{
						details = packages.Where(x => (x.ShipmentLineNbr.Select(y => y.Item1)).Contains(line.LineNbr?.Value))?.ToList();
						if (details == null || (details != null && (details.SelectMany(x => x.ShipmentLineNbr)?.Where(x => x.Item1 == line.LineNbr.Value && x.Item2 != null)?.Sum(x => x.Item2) ?? 0) != line.ShippedQty?.Value))//  check if shipped item quatity matches the  quantity of item in package 
							throw new PXException(BCMessages.ItemsWithoutBoxes, impl.ShipmentNbr.Value, line.InventoryID?.Value);
					}
					SalesOrderDetail orderLine = order.Local.Details.FirstOrDefault(d =>
						order.Local.OrderType.Value == line.OrderType.Value && order.Local.OrderNbr.Value == line.OrderNbr.Value && d.LineNbr.Value == line.OrderLineNbr.Value);
					if (orderLine == null) continue; //skip shipment that is not from this order

					DetailInfo lineInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && d.LocalID == orderLine.NoteID.Value);
					if (lineInfo == null) throw new PXException(BCMessages.OrderShippingLineSyncronized, order.Local.OrderNbr.Value);
					if (details != null)
					{
						foreach (var detail in details)
						{
							OrderLineItem shipItem = new OrderLineItem();
							shipItem.Id = lineInfo.ExternID.ToLong();
							shipItem.OrderId = order.ExternID.ToLong();
							shipItem.Quantity = (int)(detail?.ShipmentLineNbr.Where(x => x.Item1 == line.LineNbr?.Value)?.Sum(x => x.Item2) ?? 0);
							shipItem.PackageId = detail?.NoteID?.Value;
							shipmentData.LineItems.Add(shipItem);
						}
					}
					else
					{
						OrderLineItem shipItem = new OrderLineItem();
						shipItem.Id = lineInfo.ExternID.ToLong();
						shipItem.Quantity = (int)line.ShippedQty.Value;
						shipItem.OrderId = order.ExternID.ToLong();
						shipmentData.LineItems.Add(shipItem);
					}
				}

			}
		}

		protected virtual string GetCarrierName(string shipVia)
		{
			string company = null;
			if (!string.IsNullOrEmpty(shipVia))
			{
				PX.Objects.CS.Carrier carrierData = SelectFrom<PX.Objects.CS.Carrier>.Where<PX.Objects.CS.Carrier.carrierID.IsEqual<@P.AsString>>.View.Select(this, shipVia);
				if (!string.IsNullOrEmpty(carrierData?.CarrierPluginID))
				{
					company = carrierData?.CarrierPluginID;
				}
				else
					company = shipVia;
				company = GetSubstituteExternByLocal(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.Carriers), company, company);
			}

			return company;
		}
		#endregion
	}
}