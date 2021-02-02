using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using System;
using System.Linq;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	[PXVersion("20.200.001", "Default")]
	internal class DefaultEndpointImplWorkFlow : DefaultEndpointImplWorkFlowBase
	{
		public DefaultEndpointImplWorkFlow(
			CbApiWorkflowApplicator.SalesOrderApplicator salesOrderApplicator,
			CbApiWorkflowApplicator.ShipmentApplicator shipmentApplicator,
			CbApiWorkflowApplicator.SalesInvoiceApplicator salesInvoiceApplicator)
			: base(salesOrderApplicator, shipmentApplicator, salesInvoiceApplicator) { }

		[FieldsProcessed(new[] {
			"OrderType",
			"OrderNbr",
			"Hold",
			"CreditHold"
		})]
		protected virtual void SalesOrder_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var salesOrderGraph = (SOOrderEntry)graph;

			var orderTypeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrderType") as EntityValueField;
			var orderNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrderNbr") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			var creditHoldField = targetEntity.Fields.SingleOrDefault(f => f.Name == "CreditHold") as EntityValueField;

			var soOrder = (SOOrder)salesOrderGraph.Document.Cache.CreateInstance();

			if (orderTypeField != null)
				soOrder.OrderType = orderTypeField.Value;

			if (orderNbrField != null)
				soOrder.OrderNbr = orderNbrField.Value;

			salesOrderGraph.Document.Current = salesOrderGraph.Document.Insert(soOrder);

			if (holdField != null && holdField.Value != null)
			{
				PXAction action = Boolean.Parse(holdField.Value) ? salesOrderGraph.putOnHold : salesOrderGraph.releaseFromHold;
				SalesOrderApplicator.ExecuteAction(graph, action, salesOrderGraph.Document.Current);
			}

			if (creditHoldField != null && !Boolean.Parse(creditHoldField.Value))
			{
				SalesOrderApplicator.ExecuteAction(graph, salesOrderGraph.releaseFromCreditHold, salesOrderGraph.Document.Current);
			}
		}

		[FieldsProcessed(new[] {
			"Hold",
			"CreditHold"
		})]
		protected virtual void SalesOrder_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var salesOrderGraph = (SOOrderEntry)graph;

			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			var creditHoldField = targetEntity.Fields.SingleOrDefault(f => f.Name == "CreditHold") as EntityValueField;

			if (holdField != null && holdField.Value != null)
			{
                PXAction action = Boolean.Parse(holdField.Value) ? salesOrderGraph.putOnHold : salesOrderGraph.releaseFromHold;
				SalesOrderApplicator.ExecuteAction(graph, action, salesOrderGraph.Document.Current);
			}

			if (creditHoldField != null && !Boolean.Parse(creditHoldField.Value))
			{
                SalesOrderApplicator.ExecuteAction(graph, salesOrderGraph.releaseFromCreditHold, salesOrderGraph.Document.Current);
			}
		}

		[FieldsProcessed(new[] {
			"ShipmentNbr",
			"Type",
			"Hold"
		})]
		protected virtual void Shipment_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var shipmentGraph = (SOShipmentEntry)graph;

			var shipmentField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ShipmentNbr") as EntityValueField;
			var typeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Type") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			var soShipment = (SOShipment)shipmentGraph.Document.Cache.CreateInstance();

			if (typeField != null)
				shipmentGraph.Document.Cache.SetValueExt<SOShipment.shipmentType>(soShipment, typeField.Value);

			if (shipmentField != null)
				soShipment.ShipmentNbr = shipmentField.Value;

			shipmentGraph.Document.Current = shipmentGraph.Document.Insert(soShipment);

			if (holdField != null && holdField.Value != null)
			{
				PXAction action = Boolean.Parse(holdField.Value) ? shipmentGraph.putOnHold : shipmentGraph.releaseFromHold;
				ShipmentApplicator.ExecuteAction(graph, action, shipmentGraph.Document.Current);
			}
		}

		[FieldsProcessed(new[] {
			"Hold",
			"FreightAmount"
		})]
		protected virtual void Shipment_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var shipmentGraph = (SOShipmentEntry)graph;

			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			var freightPrice = targetEntity.Fields.SingleOrDefault(f => f.Name == "FreightAmount") as EntityValueField;
			
			if (freightPrice != null)
			{
				var shipmentEntry = (SOShipmentEntry)graph;
				var shipment = shipmentEntry.Document.Current;
				if (shipment.FreightAmountSource == CS.FreightAmountSourceAttribute.OrderBased)
				{
					throw new InvalidOperationException("Cannot assign freight price because the Invoice Freight Price Based On set to Order.");
				}

				shipmentEntry.Document.SetValueExt<SOShipment.overrideFreightAmount>(shipment, true);
				shipmentEntry.Document.SetValueExt<SOShipment.curyFreightAmt>(shipment, freightPrice.Value);
			}

			if (holdField != null && holdField.Value != null)
			{
				var holdValue = Boolean.Parse(holdField.Value);
				if (shipmentGraph.Document.Current != null && holdValue == shipmentGraph.Document.Current.Hold)
					return;

				PXAction action = holdValue ? shipmentGraph.putOnHold : shipmentGraph.releaseFromHold;
				ShipmentApplicator.ExecuteAction(graph, action, shipmentGraph.Document.Current);
			}
		}

		[FieldsProcessed(new[] {
			"Type",
			"ReferenceNbr",
			"Hold",
			"CreditHold"
		})]
		protected virtual void SalesInvoice_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var salesInvoiceGraph = (SOInvoiceEntry)graph;

			var typeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Type") as EntityValueField;
			var referenceNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ReferenceNbr") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			var creditHoldField = targetEntity.Fields.SingleOrDefault(f => f.Name == "CreditHold") as EntityValueField;

			var arInvoice = (ARInvoice)salesInvoiceGraph.Document.Cache.CreateInstance();

			if (typeField != null)
			{
				var arDocTypes = new ARInvoiceType.ListAttribute();
				if (arDocTypes.ValueLabelDic.ContainsValue(typeField.Value))
				{
					arInvoice.DocType = arDocTypes.ValueLabelDic.First(x => x.Value == typeField.Value).Key;
				}
				else
				{
					arInvoice.DocType = typeField.Value;
				}
			}

			if (referenceNbrField != null)
			{
				arInvoice.RefNbr = referenceNbrField.Value;
			}

			salesInvoiceGraph.Document.Current = salesInvoiceGraph.Document.Insert(arInvoice);

			if (holdField != null && holdField.Value != null)
			{
				PXAction action = Boolean.Parse(holdField.Value) ? salesInvoiceGraph.putOnHold : salesInvoiceGraph.releaseFromHold;
				SalesInvoiceApplicator.ExecuteAction(graph, action, salesInvoiceGraph.SODocument.Current);
			}

			bool creditHoldValue;
			if (creditHoldField != null && Boolean.TryParse(creditHoldField.Value, out creditHoldValue))
			{
				PXAction action = creditHoldValue ? salesInvoiceGraph.putOnCreditHold : salesInvoiceGraph.releaseFromCreditHold;
				SalesInvoiceApplicator.ExecuteAction(graph, action, salesInvoiceGraph.SODocument.Current);
			}
		}

		[FieldsProcessed(new[] {
			"Hold",
			"CreditHold"
		})]
		protected virtual void SalesInvoice_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var salesInvoiceGraph = (SOInvoiceEntry)graph;

			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			var creditHoldField = targetEntity.Fields.SingleOrDefault(f => f.Name == "CreditHold") as EntityValueField;

			if (holdField != null && holdField.Value != null)
			{
				// fix for situation when cache.Current and record in cache collection - different objects by reference 
				salesInvoiceGraph.Document.Cache.Update(salesInvoiceGraph.Document.Current);
				PXAction action = Boolean.Parse(holdField.Value) ? salesInvoiceGraph.putOnHold : salesInvoiceGraph.releaseFromHold;
				SalesInvoiceApplicator.ExecuteAction(graph, action, salesInvoiceGraph.SODocument.Current);
			}

			bool creditHoldValue;
			if (creditHoldField != null && Boolean.TryParse(creditHoldField.Value, out creditHoldValue))
			{
				// fix for situation when cache.Current and record in cache collection - different objects by reference 
				salesInvoiceGraph.Document.Cache.Update(salesInvoiceGraph.Document.Current);
				PXAction action = creditHoldValue ? salesInvoiceGraph.putOnCreditHold : salesInvoiceGraph.releaseFromCreditHold;
				SalesInvoiceApplicator.ExecuteAction(graph, action, salesInvoiceGraph.SODocument.Current);
			}
		}
	}
}
