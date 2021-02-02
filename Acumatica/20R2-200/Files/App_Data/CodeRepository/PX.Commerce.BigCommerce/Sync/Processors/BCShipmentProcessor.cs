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
using PX.Objects.PO;
using PX.Api.ContractBased.Models;
using Serilog.Context;
using static PX.Objects.SO.SOShipmentEntry;

namespace PX.Commerce.BigCommerce
{
	public class BCShipmentEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Shipment;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary }.Concat(Orders).ToArray();

		public MappedShipment Shipment;
		public List<MappedOrder> Orders = new List<MappedOrder>();
	}

	public class BCShipmentsRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			#region Shipments
			return base.Restrict<MappedShipment>(mapped, delegate (MappedShipment obj)
			{
				if (obj.Local != null)
				{
					if(obj.Local.Confirmed?.Value ==false)
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

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.Shipment, BCCaptions.Shipment,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { typeof(OrdersShipmentData) },
		LocalTypes = new Type[] { typeof(BCShipments) },
		DetailTypes = new String[] { BCEntitiesAttribute.ShipmentLine, BCCaptions.ShipmentLine, BCEntitiesAttribute.ShipmentBoxLine, BCCaptions.ShipmentLineBox },
		AcumaticaPrimaryType = typeof(PX.Objects.SO.SOShipment),
		AcumaticaPrimarySelect = typeof(PX.Objects.SO.SOShipment.shipmentNbr),
		URL = "orders?keywords={0}&searchDeletedOrders=no",
		Requires = new string[] { BCEntitiesAttribute.Order }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Shipments" })]
	public class BCShipmentProcessor : BCProcessorSingleBase<BCShipmentProcessor, BCShipmentEntityBucket, MappedShipment>, IProcessor
	{
		protected OrderRestDataProvider orderDataProvider;
		protected IChildRestDataProvider<OrdersShipmentData> orderShipmentRestDataProvider;

		protected List<BCShippingMappings> shippingMappings;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());

			orderDataProvider = new OrderRestDataProvider(client);
			orderShipmentRestDataProvider = new OrderShipmentsRestDataProvider(client);

			shippingMappings = PXSelectReadonly<BCShippingMappings,
				Where<BCShippingMappings.bindingID, Equal<Required<BCShippingMappings.bindingID>>>>
				.Select(this, Operation.Binding).Select(x => x.GetItem<BCShippingMappings>()).ToList();
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
			BCBindingExt binding = GetBindingExt<BCBindingExt>();
			BCShipments giResult = cbapi.Put<BCShipments>(new BCShipments() { OrderType = binding.OrderType.ValueField(), ShippingNoteID = localID.ValueField() });
			var result = giResult?.Results.FirstOrDefault();
			if (result == null) return null;
			GetOrderShipment(result, giResult);
			if (giResult.Shipment == null && giResult.POReceipt == null) return null;
			MapFilterFields(binding, result, giResult);
			MappedShipment obj = new MappedShipment(giResult, result.NoteID.Value, result.LastModifiedDateTime.Value);
			return obj;


		}
		public override MappedShipment PullEntity(String externID, String externalInfo)
		{
			OrdersShipmentData data = orderShipmentRestDataProvider.GetByID(externID.KeySplit(1), externID.KeySplit(0));
			if (data == null) return null;

			MappedShipment obj = new MappedShipment(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedUT.ToDate(), data.CalculateHash());

			return obj;
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
		}
		public override EntityStatus GetBucketForImport(BCShipmentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			bucket.Shipment = bucket.Shipment.Set(new OrdersShipmentData(), syncstatus.ExternID, syncstatus.ExternTS);

			return EntityStatus.None;
		}

		public override void MapBucketImport(BCShipmentEntityBucket bucket, IMappedEntity existing)
		{
		}
		public override void SaveBucketImport(BCShipmentEntityBucket bucket, IMappedEntity existing, String operation)
		{
		}
		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			BCBindingExt binding = GetBindingExt<BCBindingExt>();
			BCShipments giResult = cbapi.Put<BCShipments>(new BCShipments() { OrderType = binding.OrderType.ValueField(), LastModifiedDateTime = minDateTime.ValueField() });

			if (giResult.Results != null)
			{
				foreach (BCShipmentsResult result in giResult.Results)
				{
					MapFilterFields(binding, result, giResult);

					MappedShipment obj = new MappedShipment(giResult, result.NoteID.Value, result.LastModifiedDateTime.Value);
					EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				}
			}
		}

		private static void MapFilterFields(BCBindingExt binding, BCShipmentsResult result, BCShipments impl)
		{
			impl.ShippingNoteID = result.NoteID;
			impl.OrderType = binding.OrderType.ValueField();
			impl.OrderNbr = result.OrderNbr;
			impl.ShipmentNumber = result.ShipmentNumber;
			impl.ShipmentType = result.ShipmentType;
			impl.Confirmed = result.Confirmed;
		}

		public override EntityStatus GetBucketForExport(BCShipmentEntityBucket bucket, BCSyncStatus syncstatus)
		{
			BCBindingExt binding = GetBindingExt<BCBindingExt>();
			SOOrderShipments impl = new SOOrderShipments();

			BCShipments giResult = cbapi.Put<BCShipments>(new BCShipments()
			{
				OrderType = binding.OrderType.ValueField(),
				ShippingNoteID = syncstatus.LocalID.ValueField(),
				Results = new List<BCShipmentsResult>() { new BCShipmentsResult() { Custom = GetCustomFieldsForExport() } }
			});
			var result = giResult?.Results?.FirstOrDefault();
			if (result == null) return EntityStatus.None;

			MapFilterFields(binding, result, giResult);
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

		public override void MapBucketExport(BCShipmentEntityBucket bucket, IMappedEntity existing)
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
				OrdersShipmentData shipmentData = obj.Extern = new OrdersShipmentData();
			}
		}

		public override CustomField GetLocalCustomField(BCShipmentEntityBucket bucket, string viewName, string fieldName)
		{
			MappedShipment obj = bucket.Shipment;
			BCShipments impl = obj.Local;
			if (impl?.Results?.Count() > 0)
				return impl.Results[0].Custom?.Where(x => x.ViewName == viewName && x.FieldName == fieldName).FirstOrDefault();
			else return null;
		}

		public override void SaveBucketExport(BCShipmentEntityBucket bucket, IMappedEntity existing, String operation)
		{
			MappedShipment obj = bucket.Shipment;

			StringBuilder key = new StringBuilder();
			String origExternId = obj.ExternID;
			var ShipmentItems = obj.Extern.ShipmentItems;
			OrdersShipmentData ordersShipmentData = new OrdersShipmentData();
			ordersShipmentData = obj.Extern;
			List<DetailInfo> existingDetails = new List<DetailInfo>(obj.Details);
			List<string> notFound = new List<string>();
			obj.ClearDetails();
			if (existingDetails != null)
			{
				foreach (var externShipment in existingDetails.Where(d => d.EntityType == BCEntitiesAttribute.ShipmentLine || d.EntityType == BCEntitiesAttribute.ShipmentBoxLine))
				{
					var orderExist = bucket.Orders.FirstOrDefault(x => x.LocalID == externShipment.LocalID);
					if (externShipment.ExternID.HasParent() || orderExist != null)
					{
						orderShipmentRestDataProvider.Delete(externShipment.ExternID.HasParent() ? externShipment.ExternID.KeySplit(1): externShipment.ExternID, externShipment.ExternID.HasParent() ? externShipment.ExternID.KeySplit(0) : orderExist.ExternID);
					}
					else
						notFound.Add(externShipment.ExternID);
					if (externShipment.ExternID.HasParent() && orderExist == null)
					{
						//if order is removed from shipment then delete the shipment from BC and change status back to Awaiting Full fillment
						OrderStatus orderStatus = new OrderStatus();
						orderStatus.StatusId = OrderStatuses.AwaitingFulfillment.GetHashCode();
						orderStatus = orderDataProvider.Update(orderStatus, externShipment.ExternID.KeySplit(1));
					}
				}
			}
			foreach (MappedOrder order in bucket.Orders)
			{
				DetailInfo addressInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderAddress && d.LocalID == order.LocalID);
				if (addressInfo != null)
				{
					ordersShipmentData.OrderAddressId = addressInfo.ExternID.ToInt().Value;

					ordersShipmentData.ShipmentItems = ShipmentItems.Where(x => x.OrderID == order.ExternID)?.ToList();
					//If for some reason existing shipment is not dleted try deleting 
					if (notFound.Count > 0)
					{
						var existingshipments = orderShipmentRestDataProvider.Get(order.ExternID);
						if (existingshipments?.Count > 0)
						{
							existingshipments.Where(x => notFound.Contains(x.Id.ToString()))?.ForEach(y =>
							{ 
								orderShipmentRestDataProvider.Delete(y.Id.ToString(), y.OrderId.ToString());
								notFound.Remove(y.Id.ToString());
							});
						}
					}
					
					if (ordersShipmentData.ShipmentItems.All(x => x.PackageId != null))
					{
						var packages = ordersShipmentData.ShipmentItems.GroupBy(x => x.PackageId).ToDictionary(x => x.Key, x => x.ToList());
						foreach (var package in packages)
						{
							ordersShipmentData.ShipmentItems = package.Value;
							ordersShipmentData.TrackingNumber = obj.Local.Shipment.Packages.FirstOrDefault(x => x.NoteID?.Value == package.Key)?.TrackingNbr?.Value;
							OrdersShipmentData data = orderShipmentRestDataProvider.Create(ordersShipmentData, order.ExternID);
							obj.With(_ => { _.ExternID = null; return _; }).AddExtern(data, new object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedUT.ToDate());
							obj.AddDetail(BCEntitiesAttribute.ShipmentBoxLine, order.LocalID, new object[] { data.OrderId, data.Id }.KeyCombine());
							key.Append(key.Length > 0 ? "|" + obj.ExternID : obj.ExternID);
						}
					}
					else
					{
						OrdersShipmentData data = orderShipmentRestDataProvider.Create(ordersShipmentData, order.ExternID);
						obj.With(_ => { _.ExternID = null; return _; }).AddExtern(data, new object[] { data.OrderId, data.Id }.KeyCombine(), data.DateCreatedUT.ToDate());
						obj.AddDetail(BCEntitiesAttribute.ShipmentLine, order.LocalID, new object[] { data.OrderId, data.Id }.KeyCombine());
						key.Append(key.Length > 0 ? "|" + obj.ExternID : obj.ExternID);
					}
				}

				OrderStatus orderStatus = new OrderStatus();
				if (obj.Local.ShipmentType.Value == SOShipmentType.Invoice)
					orderStatus.StatusId = OrderStatuses.Completed.GetHashCode();
				else
					orderStatus.StatusId = BCSalesOrderProcessor.ConvertStatus(order.Local.Status?.Value).GetHashCode();

				orderStatus = orderDataProvider.Update(orderStatus, order.ExternID);

				order.AddExtern(null, orderStatus.Id?.ToString(), orderStatus.DateModifiedUT.ToDate());
				UpdateStatus(order, null);
			}
			if (obj.Extern.OrderAddressId != 0)
				obj.ExternID = key.ToString().TrimExternID(); ;

			UpdateStatus(obj, operation);
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
		protected virtual EntityStatus GetDropShipment(BCShipmentEntityBucket bucket, BCShipmentsResult result, BCShipments bCShipments)
		{
			GetDropShipmentByShipmentNbr(result, bCShipments);
			if (bCShipments.POReceipt == null) return EntityStatus.None;

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
		protected virtual EntityStatus GetShipment(BCShipmentEntityBucket bucket, BCShipmentsResult result, BCShipments bCShipment)
		{
			bCShipment.Shipment = cbapi.GetByID<Shipment>(result.NoteID.Value);
			if (bCShipment.Shipment == null) return EntityStatus.None;

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
		protected virtual EntityStatus GetInvoice(BCShipmentEntityBucket bucket, BCShipmentsResult result, BCShipments bCShipment)
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

		protected virtual void MapDropShipment(BCShipmentEntityBucket bucket, MappedShipment obj, PurchaseReceipt impl)
		{
			OrdersShipmentData shipmentData = obj.Extern = new OrdersShipmentData();
			shipmentData.ShipmentItems = new List<OrdersShipmentItem>();
			shipmentData.ShippingProvider = string.Empty;
			var shipvia = impl.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.ShipVia?.Value))?.ShipVia?.Value ?? string.Empty;
			shipmentData.ShippingMethod = shipvia;

			if (!string.IsNullOrEmpty(shipvia))
			{
				var shippingmethods = shippingMappings.Where(x => x.CarrierID == shipvia)?.ToList();
				if (shippingmethods?.Count == 1)
					shipmentData.ShippingMethod = shippingmethods.FirstOrDefault().ShippingMethod;
			}
			shipmentData.TrackingNumber = impl.VendorRef?.Value;

			foreach (MappedOrder order in bucket.Orders)
			{
				DetailInfo addressInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderAddress && d.LocalID == order.LocalID);
				if (addressInfo != null)
					shipmentData.OrderAddressId = addressInfo.ExternID.ToInt().Value;
				foreach (PurchaseReceiptDetail line in impl.Details ?? new List<PurchaseReceiptDetail>())
				{
					SalesOrderDetail orderLine = order.Local.Details.FirstOrDefault(d =>
						order.Local.OrderType.Value == line.SOOrderType.Value && order.Local.OrderNbr.Value == line.SOOrderNbr.Value && d.LineNbr.Value == line.SOLineNbr.Value);
					if (orderLine == null) continue; //skip shipment that is not from this order

					DetailInfo lineInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && d.LocalID == orderLine.NoteID.Value);
					if (lineInfo == null) throw new PXException(BCMessages.OrderShippingLineSyncronized, order.Local.OrderNbr.Value);


					OrdersShipmentItem shipItem = new OrdersShipmentItem();
					shipItem.OrderProductId = lineInfo.ExternID.ToInt();
					shipItem.Quantity = (int)line.ReceiptQty.Value;
					shipItem.OrderID = order.ExternID;

					shipmentData.ShipmentItems.Add(shipItem);
				}
			}
		}
		protected virtual void MapShipment(BCShipmentEntityBucket bucket, MappedShipment obj, Shipment impl)
		{
			OrdersShipmentData shipmentData = obj.Extern = new OrdersShipmentData();
			shipmentData.ShipmentItems = new List<OrdersShipmentItem>();
			shipmentData.ShippingProvider = string.Empty;
			shipmentData.ShippingMethod = impl.ShipVia?.Value ?? string.Empty;
			if (!string.IsNullOrEmpty(impl.ShipVia?.Value))
			{
				var shippingmethods = shippingMappings.Where(x => x.CarrierID == impl.ShipVia?.Value)?.ToList();
				if (shippingmethods?.Count == 1)
					shipmentData.ShippingMethod = shippingmethods.FirstOrDefault().ShippingMethod;

			}
			var PackageDetails = PXSelect<SOShipLineSplitPackage,
			Where<SOShipLineSplitPackage.shipmentNbr, Equal<Required<SOShipLineSplitPackage.shipmentNbr>>
			>>.Select(this, impl.ShipmentNbr?.Value).RowCast<SOShipLineSplitPackage>().ToList();
			var packages = impl.Packages ?? new List<ShipmentPackage>();
			if (packages.Count == 1)
				shipmentData.TrackingNumber = packages.FirstOrDefault()?.TrackingNbr?.Value;
			else
				foreach (ShipmentPackage package in packages)
				{
					var detail = PackageDetails.Where(x => x.PackageLineNbr == package.LineNbr?.Value && x.PackedQty != 0)?.ToList() ?? new List<SOShipLineSplitPackage>();
					if (detail.Count == 0)//if box is emty
						throw new PXException(BCMessages.BoxesWithoutItems, impl.ShipmentNbr.Value, package.TrackingNbr?.Value);
					package.ShipmentLineNbr.AddRange(detail.Select(x => new Tuple<int?, decimal?>(x.ShipmentLineNbr, x.PackedQty)));
				}

			foreach (MappedOrder order in bucket.Orders)
			{
				DetailInfo addressInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderAddress && d.LocalID == order.LocalID);
				if (addressInfo != null)
					shipmentData.OrderAddressId = addressInfo.ExternID.ToInt().Value;

				foreach (ShipmentDetail line in impl.Details ?? new List<ShipmentDetail>())
				{
					List<ShipmentPackage> details = null;
					if (packages.Count > 1)
					{
						details = packages.Where(x => x.ShipmentLineNbr.Select(y => y.Item1).Contains(line.LineNbr?.Value))?.ToList();
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
							OrdersShipmentItem shipItem = new OrdersShipmentItem();
							shipItem.OrderProductId = lineInfo.ExternID.ToInt();
							shipItem.OrderID = order.ExternID;
							shipItem.Quantity = (int)(detail?.ShipmentLineNbr.Where(x => x.Item1 == line.LineNbr?.Value)?.Sum(x => x.Item2) ?? 0);
							shipItem.PackageId = detail?.NoteID?.Value;
							shipmentData.ShipmentItems.Add(shipItem);
						}
					}
					else
					{
						OrdersShipmentItem shipItem = new OrdersShipmentItem();
						shipItem.OrderProductId = lineInfo.ExternID.ToInt();
						shipItem.OrderID = order.ExternID;
						shipItem.Quantity = (int)line.ShippedQty.Value;
						shipmentData.ShipmentItems.Add(shipItem);
					}
				}

			}
		}
		#endregion
	}
}