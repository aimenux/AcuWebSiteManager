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
using PX.Objects.CS;
using System.Diagnostics;
using System.Collections.Generic;

namespace PX.Objects.EndpointAdapters
{
	[PXInternalUseOnly]
	[PXVersion("20.200.001", "Default")]
	public class DefaultEndpointImpl20 : DefaultEndpointImpl18
	{
		[FieldsProcessed(new[] {
			"AttributeID",
			"AttributeDescription",
			"RefNoteID",
			"Value",
			"ValueDescription"
		})]
		protected new void AttributeValue_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var attributeID = targetEntity.Fields.OfType<EntityValueField>().FirstOrDefault(f => f.Name == "AttributeID")?.Value;
			if (attributeID == null)
			{
				Debug.Fail("Cannot get AttributeID");
				return;
			}
			ProcessAttribute(graph, targetEntity, attributeID);
		}

		[FieldsProcessed(new[] {
			"AttributeID",
			"AttributeDescription",
			"RefNoteID",
			"Value",
			"ValueDescription"
		})]
		protected void AttributeValue_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!targetEntity.InternalKeys.TryGetValue(CS.Messages.CSAnswers, out var answersKeys)
				|| !answersKeys.TryGetValue(nameof(CSAnswers.AttributeID), out var attributeID))
			{
				Debug.Fail("Cannot get AttributeID");
				return;
			}
			ProcessAttribute(graph, targetEntity, attributeID);
		}

		private void ProcessAttribute(PXGraph graph, EntityImpl targetEntity, string attributeID)
		{
			var value = targetEntity.Fields.OfType<EntityValueField>().FirstOrDefault(f => f.Name == "Value");
			if (value == null)
				return;

			var view = graph.Views[CS.Messages.CSAnswers];
			var cache = view.Cache;

			var rows = view.SelectMulti().OrderBy(row =>
			{
				var orderState = cache.GetStateExt<CSAnswers.order>(row) as PXFieldState;
				return orderState.Value;
			}).ToArray();

			foreach (CSAnswers row in rows)
			{
				var attributeDescr = (cache.GetStateExt<CSAnswers.attributeID>(row) as PXFieldState)?.Value?.ToString();
				if (attributeID.OrdinalEquals(row.AttributeID) || attributeID.OrdinalEquals(attributeDescr))
				{
					var state = cache.GetStateExt<CSAnswers.value>(row) as PXStringState;
					if (state != null && state.ValueLabelDic != null)
					{
						foreach (var rec in state.ValueLabelDic)
						{
							if (rec.Value == value.Value)
							{
								value.Value = rec.Key;
								break;
							}
						}
					}
					cache.SetValueExt<CSAnswers.value>(row, value.Value);
					cache.Update(row);
					break;
				}
			}
		}
		
		[FieldsProcessed(new[] {
			"POLineNbr",
			"POOrderType",
			"POOrderNbr",
			"POReceiptLineNbr",
			"POReceiptNbr",
			"TransferOrderType",
			"TransferOrderNbr",
			"TransferShipmentNbr"
		})]
		protected override void PurchaseReceiptDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var receiptEntry = (POReceiptEntry)graph;

			if (receiptEntry.Document.Current != null)
			{
				var receiptNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POReceiptNbr") as EntityValueField;

				var detailsCache = receiptEntry.transactions.Cache;

				if (receiptEntry.Document.Current.ReceiptType == POReceiptType.POReturn && receiptNbr != null)
				{
					var receiptLineNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POReceiptLineNbr") as EntityValueField;

					bool insertViaAddPRLine = receiptLineNbr != null && receiptNbr != null;
					bool insertViaAddPR = receiptNbr != null;

					if (insertViaAddPRLine)
					{
						FillInAddPRFilter(receiptEntry, receiptNbr);

						var receiptLines = receiptEntry.poReceiptLineReturn.Select().Select(r => r.GetItem<POReceiptLineReturn>());
						var receiptLine = receiptLines.FirstOrDefault(o => o.LineNbr == int.Parse(receiptLineNbr.Value));
						if (receiptLine == null)
						{
							throw new PXException(PO.Messages.PurchaseReceiptLineNotFound);
						}
						receiptLine.Selected = true;
						receiptEntry.poReceiptLineReturn.Update(receiptLine);
						receiptEntry.Actions["AddPOReceiptLineReturn2"].Press();
						return;
					}
					else if (insertViaAddPR)
					{
						FillInAddPRFilter(receiptEntry, receiptNbr);

						var order = receiptEntry.poReceiptReturn.Select().Select(r => r.GetItem<POReceiptReturn>()).FirstOrDefault();

						order.Selected = true;
						receiptEntry.poReceiptReturn.Update(order);
						receiptEntry.Actions["AddPOReceiptReturn2"].Press();
						return;
					}
				}

				base.PurchaseReceiptDetail_Insert(graph, entity, targetEntity);

				if (receiptEntry.Document.Current.ReceiptType == POReceiptType.POReturn && receiptNbr == null && detailsCache.Current != null
					&& ((POReceiptLine)detailsCache.Current).InventoryID == null)
				{
					SetFieldsNeedToInsertAllocations(targetEntity, receiptEntry, (POReceiptLine)detailsCache.Current);
				}
			}
		}

		protected virtual void FillInAddPRFilter(POReceiptEntry receiptEntry, EntityValueField receiptNbr)
		{
			receiptEntry.returnFilter.Cache.Remove(receiptEntry.returnFilter.Current);
			receiptEntry.returnFilter.Cache.Insert(new POReceiptReturnFilter());
			var filter = receiptEntry.returnFilter.Current;

			receiptEntry.returnFilter.Cache.SetValueExt(filter, "ReceiptNbr", receiptNbr.Value);
			filter = receiptEntry.returnFilter.Update(filter);

			Dictionary<string, string> filterErrors = PXUIFieldAttribute.GetErrors(receiptEntry.returnFilter.Cache, filter);

			if (filterErrors.Count() > 0)
			{
				throw new PXException(string.Join(";", filterErrors.Select(x => x.Key + "=" + x.Value)));
			}
		}

	}
}
