using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.PO.DAC.Projections;
using System.Linq;
using System;
using PX.Common;

namespace PX.Objects.EndpointAdapters
{
	[PXInternalUseOnly]
	[PXVersion("18.200.001", "Default")]
	public class DefaultEndpointImpl18 : DefaultEndpointImpl
	{
		/// <summary>
		/// Handles creation of document details in the Bills and Adjustments (AP301000) screen
		/// for cases when po entities are specified
		/// using the <see cref="APInvoiceEntry.addPOOrder2">Add PO action</see>
		/// and the <see cref="APInvoiceEntry.addPOReceipt2">Add PO Receipt action</see>.
		/// </summary>
		[FieldsProcessed(new[] {
			"POOrderType",
			"POOrderNbr",
			"POLine",
			"POReceiptNbr",
			"POReceiptLine"
		})]
		protected override void BillDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EntityValueField orderType = targetEntity.Fields.SingleOrDefault(f => f.Name == "POOrderType") as EntityValueField;
			EntityValueField orderNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POOrderNbr") as EntityValueField;
			EntityValueField orderLineNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POLine") as EntityValueField;
			EntityValueField receiptNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POReceiptNbr") as EntityValueField;
			EntityValueField receiptLineNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POReceiptLine") as EntityValueField;

			if (orderType != null && orderNbr != null)
			{
				if (orderLineNbr != null)
					AddPOOrderLineToBill(graph, (APInvoiceEntry)graph, orderType, orderNbr, orderLineNbr);
				else
					AddPOOrderToBill(graph, (APInvoiceEntry)graph, orderType, orderNbr);
			}
			else if (receiptNbr != null)
			{
				if (receiptLineNbr != null)
					AddPOReceiptLineToBill(graph, (APInvoiceEntry)graph, receiptNbr, receiptLineNbr);
				else
					AddPOReceiptToBill(graph, (APInvoiceEntry)graph, receiptNbr);
			}
			else
			{
				base.BillDetail_Insert(graph, entity, targetEntity);
			}
		}

		private static void AddPOOrderToBill(PXGraph graph, APInvoiceEntry invoiceEntry, EntityValueField orderType, EntityValueField orderNbr)
		{
			var state = invoiceEntry.Transactions.Cache.GetStateExt<APTran.pOOrderType>(new APTran { }) as PXStringState;

			if (state != null && state.AllowedLabels.Contains(orderType.Value))
			{
				orderType.Value = state.ValueLabelDic.Single(p => p.Value == orderType.Value).Key;
			}

			if (orderType.Value == POOrderType.RegularSubcontract)
			{
				var constructionExt = graph.GetExtension<CN.Subcontracts.AP.GraphExtensions.ApInvoiceEntryAddSubcontractsExtension>();
				POOrderRS line = (POOrderRS)(constructionExt.Subcontracts.Select().Where(x => (((POOrderRS)x).OrderType == orderType.Value && ((POOrderRS)x).OrderNbr == orderNbr.Value)).FirstOrDefault());
				if (line == null)
				{
					throw new PXException($"Subcontract {orderNbr.Value} was not found.");
				}

				line.Selected = true;
				constructionExt.Subcontracts.Update(line);
				constructionExt.AddSubcontract.Press();
			}
			else
			{
				var orderExtension = graph.GetExtension<PO.GraphExtensions.APInvoiceSmartPanel.AddPOOrderExtension>();
				POOrderRS line = (POOrderRS)(orderExtension.poorderslist.Select().Where(x => (((POOrderRS)x).OrderType == orderType.Value && ((POOrderRS)x).OrderNbr == orderNbr.Value)).FirstOrDefault());
				
				if (line == null)
				{
					throw new PXException($"Purchase Order {orderType.Value} {orderNbr.Value} was not found.");
				}

				line.Selected = true;
				orderExtension.poorderslist.Update(line);
				orderExtension.addPOOrder2.Press();
			}			
		}

		private static void AddPOOrderLineToBill(PXGraph graph, APInvoiceEntry invoiceEntry, EntityValueField orderType, EntityValueField orderNbr, EntityValueField orderLineNbr)
		{
			var state = invoiceEntry.Transactions.Cache.GetStateExt<APTran.pOOrderType>(new APTran { }) as PXStringState;

			if (state != null && state.AllowedLabels.Contains(orderType.Value))
			{
				orderType.Value = state.ValueLabelDic.Single(p => p.Value == orderType.Value).Key;
			}

			if (orderType.Value == POOrderType.RegularSubcontract)
			{
				var constructionExt = graph.GetExtension<CN.Subcontracts.AP.GraphExtensions.ApInvoiceEntryAddSubcontractsExtension>();
				int poLineNbr = int.Parse(orderLineNbr.Value);
				POLineRS line = (POLineRS)(constructionExt.SubcontractLines.Select().Where(x => (((POLineRS)x).OrderType == orderType.Value && ((POLineRS)x).OrderNbr == orderNbr.Value && ((POLineRS)x).LineNbr == poLineNbr)).FirstOrDefault());
				
				if (line == null)
				{
					throw new PXException($"Subcontract {orderNbr.Value}, Line Nbr.: {orderLineNbr.Value} was not found.");
				}

				line.Selected = true;
				constructionExt.SubcontractLines.Update(line);
				constructionExt.AddSubcontractLine.Press();
			}
			else
			{
				var orderLineExtension = graph.GetExtension<PO.GraphExtensions.APInvoiceSmartPanel.AddPOOrderLineExtension>();
				int poLineNbr = int.Parse(orderLineNbr.Value);

				POLineRS line = (POLineRS)(orderLineExtension.poorderlineslist.Select().Where(x => (((POLineRS)x).OrderType == orderType.Value && ((POLineRS)x).OrderNbr == orderNbr.Value && ((POLineRS)x).LineNbr == poLineNbr)).FirstOrDefault());

				if (line == null)
				{
					throw new PXException($"Order Line: {orderType.Value} {orderNbr.Value}, Line Nbr.: {orderLineNbr.Value} not found.");
				}

				line.Selected = true;
				orderLineExtension.poorderlineslist.Update(line);
				orderLineExtension.addPOOrderLine2.Press();
			}
		}

		private static void AddPOReceiptToBill(PXGraph graph, APInvoiceEntry invoiceEntry, EntityValueField receiptNbr)
		{
			var receiptExtension = graph.GetExtension<PO.GraphExtensions.APInvoiceSmartPanel.AddPOReceiptExtension>();

			POReceipt line = (POReceipt)(receiptExtension.poreceiptslist.Select().Where(x => ((POReceipt)x).ReceiptNbr == receiptNbr.Value).FirstOrDefault());

			if (line == null)
			{
				throw new PXException($"Purchase Receipt {receiptNbr.Value} was not found.");
			}

			line.Selected = true;



			receiptExtension.poreceiptslist.Update(line);

			receiptExtension.addPOReceipt2.Press();
		}

		private static void AddPOReceiptLineToBill(PXGraph graph, APInvoiceEntry invoiceEntry, EntityValueField receiptNbr, EntityValueField receiptLineNbr)
		{
			var receiptLineExtension = graph.GetExtension<PO.GraphExtensions.APInvoiceSmartPanel.AddPOReceiptLineExtension>();

			POReceiptLineAdd line = receiptLineExtension.ReceipLineAdd.Select(receiptNbr.Value, receiptLineNbr.Value);

			if (line == null)
			{
				throw new PXException($"Receipt Line {receiptNbr.Value} - {receiptLineNbr.Value} not found.");
			}

			line.Selected = true;

			receiptLineExtension.poReceiptLinesSelection.Update(line);

            receiptLineExtension.addReceiptLine2.Press();
		}

		[FieldsProcessed(new string[0])]
		protected void Payments_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EntityValueField docType = targetEntity.Fields.SingleOrDefault(f => f.Name == "DocType") as EntityValueField;
			EntityValueField appliedToOrderEntity = targetEntity.Fields.SingleOrDefault(f => f.Name == "AppliedToOrder") as EntityValueField;

			if (appliedToOrderEntity != null && appliedToOrderEntity.Value != null)
			{
				decimal appliedToOrder = decimal.Parse(appliedToOrderEntity.Value);

				SOOrderEntry orderEntry = (SOOrderEntry)graph;
				orderEntry.Save.Press();

				orderEntry.CheckTermsInstallmentType();

				if (docType != null)
				{
					var state = graph.Caches[typeof(ARRegister)].GetStateExt(new ARRegister(), "DocType") as PXStringState;
					if (state != null && state.ValueLabelDic != null)
					{
						bool keyFound = false;
						foreach (var rec in state.ValueLabelDic)
						{
							if (rec.Value == docType.Value || rec.Key == docType.Value)
							{
								keyFound = true;
								docType.Value = rec.Key;
								break;
							}
						}
						if (!keyFound)
							docType = null;
					}
				}

				PXGraph target;
		
				orderEntry.CreatePaymentProc(orderEntry.Document.Current, out target, docType != null ? docType.Value : ARPaymentType.Payment);
				ARPaymentEntry paymentEntry = (ARPaymentEntry)target;

				var adjustment = paymentEntry.SOAdjustments.Current;
				adjustment.CuryAdjgAmt = appliedToOrder;
				paymentEntry.SOAdjustments.Update(adjustment);

				ARPayment document = (ARPayment)paymentEntry.Document.Cache.CreateCopy(paymentEntry.Document.Current);

				EntityValueField paymentMethodEntity = targetEntity.Fields.SingleOrDefault(f => f.Name == "PaymentMethod") as EntityValueField;
				if (paymentMethodEntity != null && paymentMethodEntity.Value != null)
				{
					paymentEntry.Document.Cache.SetValueExt<ARPayment.paymentMethodID>(document, paymentMethodEntity.Value);
				}
				EntityValueField cashAccountEntity = targetEntity.Fields.SingleOrDefault(f => f.Name == "CashAccount") as EntityValueField;
				if (cashAccountEntity != null && cashAccountEntity.Value != null)
				{
					paymentEntry.Document.Cache.SetValueExt<ARPayment.cashAccountID>(document, cashAccountEntity.Value);
				}
				EntityValueField paymentAmountEntity = targetEntity.Fields.SingleOrDefault(f => f.Name == "PaymentAmount") as EntityValueField;
				if (paymentAmountEntity != null && paymentAmountEntity.Value != null)
				{
					decimal paymentAmount = decimal.Parse(paymentAmountEntity.Value);
					paymentEntry.Document.Cache.SetValueExt<ARPayment.curyOrigDocAmt>(document, paymentAmount);
				}
				else
				{
					paymentEntry.Document.Cache.SetValueExt<ARPayment.curyOrigDocAmt>(document, appliedToOrder);
				}
				paymentEntry.Document.Update(document);

				paymentEntry.Save.Press();
				orderEntry.Cancel.Press();
				try
				{
					orderEntry.Adjustments.Current = orderEntry.Adjustments.Select().Where(x => (((SOAdjust)x).AdjgDocType == paymentEntry.Document.Current.DocType && ((SOAdjust)x).AdjgRefNbr == paymentEntry.Document.Current.RefNbr)).First();
				}
				catch
				{
					throw new PXException($"Payment {paymentEntry.Document.Current.DocType} {paymentEntry.Document.Current.RefNbr} was not found in the list of payments applied to order.");
				}
			}
		}
	}
}
