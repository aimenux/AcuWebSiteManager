using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.IN;
using PX.Objects.PO;
using System;

namespace PX.Objects.SO.Attributes
{
	public class ShippingRefNoteAttribute : PXGuidAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (sender.Graph.PrimaryItemType == null)
				return;

			PXButtonDelegate delgate = delegate (PXAdapter adapter)
			{
				PXCache cache = adapter.View.Graph.Caches[typeof(SOOrderShipment)];
				if (cache.Current != null)
				{
					var helper = new EntityHelper(cache.Graph);
					object val = cache.GetValueExt(cache.Current, _FieldName);
					var state = val as PXRefNoteBaseAttribute.PXLinkState;
					if (state != null)
					{
						helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
					}
					else
					{
						helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, _FieldName), PXRedirectHelper.WindowMode.NewWindow);
					}
				}

				return adapter.Get();
			};

			string actionName = $"{ sender.GetItemType().Name }~{ _FieldName }~Link";
			PXNamedAction.AddHiddenAction(sender.Graph, sender.Graph.PrimaryItemType, actionName, delgate);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			GetTargetTypeAndKeys(e.Row as SOOrderShipment, out Type targetType, out object[] targetKeys);

			if (targetType != null)
			{
				e.ReturnValue = PXRefNoteBaseAttribute.GetEntityRowID(sender.Graph.Caches[targetType], targetKeys, ", ");
			}
			e.ReturnState = PXRefNoteBaseAttribute.PXLinkState.CreateInstance(e.ReturnState, targetType, targetKeys);
		}

		public static void GetTargetTypeAndKeys(SOOrderShipment row, out Type targetType, out object[] targetKeys)
		{
			targetType = null;
			targetKeys = null;

			if (row != null && row.ShipmentType.IsIn(INDocType.Issue, INDocType.Transfer) && !string.Equals(row.ShipmentNbr, Constants.NoShipmentNbr))
			{
				targetType = typeof(SOShipment);
				targetKeys = new object[] { row.ShipmentNbr };
			}
			else if (row != null && row.ShipmentType == INDocType.DropShip && !string.IsNullOrEmpty(row.ShipmentNbr))
			{
				targetType = typeof(POReceipt);
				targetKeys = new object[] { PO.POReceiptType.POReceipt, row.ShipmentNbr };
			}
			else if (row != null
				&& (row.ShipmentType == INDocType.Issue && string.Equals(row.ShipmentNbr, Constants.NoShipmentNbr) || row.ShipmentType == INDocType.Invoice)
				&& !string.IsNullOrEmpty(row.InvoiceType) && !string.IsNullOrEmpty(row.InvoiceNbr))
			{
				targetType = typeof(ARInvoice);
				targetKeys = new object[] { row.InvoiceType, row.InvoiceNbr };
			}
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.External) == PXDBOperation.External)
				return;

			base.CommandPreparing(sender, e);
		}
	}
}
