using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	public class LSARTran : LSSelectSOBase<ARTran, ARTranAsSplit,
		Where<ARTranAsSplit.tranType, Equal<Current<ARInvoice.docType>>, And<ARTranAsSplit.refNbr, Equal<Current<ARInvoice.refNbr>>,
		And<ARTranAsSplit.lineType, NotEqual<SOLineType.freight>, And<ARTranAsSplit.lineType, NotEqual<SOLineType.discount>>>>>>
	{
		public LSARTran(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof(ARTran.qty);
			graph.FieldDefaulting.AddHandler<ARTran.locationID>(ARTran_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<ARTranAsSplit.subItemID>(ARTranSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<ARTranAsSplit.locationID>(ARTranSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<ARTranAsSplit.invtMult>(ARTranSplit_InvtMult_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<ARTranAsSplit.lotSerialNbr>(ARTranSplit_LotSerialNbr_FieldDefaulting);
			graph.FieldVerifying.AddHandler<ARTran.uOM>(ARTran_UOM_FieldVerifying);
		}

		#region Implementation

		protected override void Master_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Master_RowSelected(sender, e);

			ARTran row = (ARTran)e.Row;
			if (row == null) return;

			bool directLine = (row.InvtMult != 0),
				directStockLine = directLine && (row.LineType == SOLineType.Inventory);
			PXUIFieldAttribute.SetEnabled<ARTran.subItemID>(sender, row, directStockLine);
			PXUIFieldAttribute.SetEnabled<ARTran.siteID>(sender, row, directLine);
			PXUIFieldAttribute.SetEnabled<ARTran.locationID>(sender, row, directStockLine);

			PXPersistingCheck checkValues = directStockLine ? PXPersistingCheck.Null : PXPersistingCheck.Nothing;
			PXDefaultAttribute.SetPersistingCheck<ARTran.subItemID>(sender, row, checkValues);
			PXDefaultAttribute.SetPersistingCheck<ARTran.siteID>(sender, row, checkValues);
			PXDefaultAttribute.SetPersistingCheck<ARTran.locationID>(sender, row, checkValues);
		}

		public virtual void ARTran_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			if (row != null && (row.InvtMult == 0 || row.LineType != SOLineType.Inventory))
			{
				e.Cancel = true;
			}
		}

		public virtual void ARTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(ARTran)];
			if (cache.Current != null && (e.Row == null || ((ARTran)cache.Current).LineNbr == ((ARTranAsSplit)e.Row).LineNbr))
			{
				e.NewValue = ((ARTran)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void ARTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(ARTran)];
			if (cache.Current != null && (e.Row == null || ((ARTran)cache.Current).LineNbr == ((ARTranAsSplit)e.Row).LineNbr))
			{
				e.NewValue = ((ARTran)cache.Current).LocationID;
				e.Cancel = true;
			}
		}

		public virtual void ARTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(ARTran)];
			if (cache.Current != null && (e.Row == null || ((ARTran)cache.Current).LineNbr == ((ARTranAsSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<ARTran> ms = new InvtMultScope<ARTran>((ARTran)cache.Current))
				{
					e.NewValue = ((ARTran)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void ARTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ARTranAsSplit)e.Row).InventoryID);

			if (item != null)
			{
				object InvtMult = ((ARTranAsSplit)e.Row).InvtMult;
				if (InvtMult == null)
				{
					sender.RaiseFieldDefaulting<ARTranAsSplit.invtMult>(e.Row, out InvtMult);
				}

				INLotSerTrack.Mode mode = GetTranTrackMode((ILSMaster)e.Row, item);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
					foreach (ARTranAsSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<ARTranAsSplit>(sender, item, lotSerNum, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		public virtual void ARTran_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (ARTran)e.Row;
			if (row.InvtMult == 0)
				return;

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);
			string inTranType = INTranType.TranTypeFromInvoiceType(row.TranType, row.Qty);
			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, inTranType, row.InvtMult))
			{
				object newval;

				sender.RaiseFieldDefaulting<ARTran.uOM>(e.Row, out newval);

				if (object.Equals(newval, e.NewValue) == false)
				{
					e.NewValue = newval;
					sender.RaiseExceptionHandling<ARTran.uOM>(e.Row, null, new PXSetPropertyException(IN.Messages.SerialItemAdjustment_UOMUpdated, PXErrorLevel.Warning, newval));
				}
			}
		}

		public override void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (ARTran)e.Row;
			if (row.InvtMult == 0)
				return;

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);
			string inTranType = INTranType.TranTypeFromInvoiceType(row.TranType, row.Qty);
			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, inTranType, row.InvtMult))
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m && (decimal)e.NewValue != -1m)
				{
					e.NewValue = (decimal)e.NewValue > 0 ? 1m : -1m;
					sender.RaiseExceptionHandling<ARTran.qty>(e.Row, null, new PXSetPropertyException(IN.Messages.SerialItemAdjustment_LineQtyUpdated, PXErrorLevel.Warning, ((InventoryItem)item).BaseUnit));
				}
			}
		}

		public override void CreateNumbers(PXCache sender, ARTran Row, decimal BaseQty, bool AlwaysAutoNextNbr)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			INLotSerClass itemclass = item;

			if (itemclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				itemclass.LotSerAssign == INLotSerAssign.WhenReceived &&
				(Row.SubItemID == null || Row.LocationID == null))
				return;

			base.CreateNumbers(sender, Row, BaseQty, AlwaysAutoNextNbr);
		}

		public override void IssueNumbers(PXCache sender, ARTran Row, decimal BaseQty)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			INLotSerClass itemclass = item;

			if (itemclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				itemclass.LotSerAssign == INLotSerAssign.WhenReceived &&
				(Row.SubItemID == null || Row.LocationID == null))
				return;

			base.IssueNumbers(sender, Row, BaseQty);
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				ARTran row = (ARTran)e.Row;
				if (Math.Abs((decimal)row.BaseQty) >= 0.0000005m && (row.UnassignedQty >= 0.0000005m || row.UnassignedQty <= -0.0000005m))
				{
					if (sender.RaiseExceptionHandling<ARTran.qty>(row, row.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
					{
						throw new PXRowPersistingException(typeof(ARTran.qty).Name, row.Qty, Messages.BinLotSerialNotAssigned);
					}
				}
				try
				{
					returnRecords = null;
				if (!MemoAvailabilityCheckQty(sender, row))
				{
						RaiseAvailabilityException(sender, row, typeof(ARTran.qty), Messages.InvoiceCheck_DecreaseQty, true,
							sender.GetValueExt<ARTran.origInvoiceNbr>(row),
							sender.GetValueExt<ARTran.inventoryID>(row),
							returnRecords == null ? string.Empty : string.Join(", ", returnRecords.Select(x => x.DocumentNbr)));
					}
				}
				finally
				{
					returnRecords = null;
				}

				OrderAvailabilityCheck(sender, row, onPersist: true);
			}
			base.Master_RowPersisting(sender, e);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		private ReturnRecord[] returnRecords;

		protected override bool CheckInvoicedAndReturnedQty(PXCache sender, int? returnInventoryID, IEnumerable<SOInvoicedRecords.Record> invoiced, IEnumerable<ReturnRecord> returned)
		{
			var success = base.CheckInvoicedAndReturnedQty(sender, returnInventoryID, invoiced, returned);
			if (!success)
			{
				string refNbr = ((ARTran)sender.Current)?.RefNbr;
				returnRecords = returned.Where(x => x.DocumentNbr != refNbr).ToArray();
			}
			return success;
		}

		protected virtual bool MemoAvailabilityCheckQty(PXCache sender, ARTran row)
		{
			bool directInvoiceLine = string.IsNullOrEmpty(row.SOOrderNbr);
			bool isEmptyOrigInvoiceType = string.IsNullOrEmpty(row.OrigInvoiceType),
				isEmptyOrigInvoiceNbr = string.IsNullOrEmpty(row.OrigInvoiceNbr),
				isEmptyOrigInvoiceLineNbr = !row.OrigInvoiceLineNbr.HasValue;
			if (directInvoiceLine && (!isEmptyOrigInvoiceType || !isEmptyOrigInvoiceNbr || !isEmptyOrigInvoiceLineNbr))
			{
				Type emptyFieldType =
					isEmptyOrigInvoiceType ? typeof(ARTran.origInvoiceType)
					: isEmptyOrigInvoiceNbr ? typeof(ARTran.origInvoiceNbr)
					: isEmptyOrigInvoiceLineNbr ? typeof(ARTran.origInvoiceLineNbr) : null;
				if (emptyFieldType != null)
				{
					string emptyFieldDisplayName = PXUIFieldAttribute.GetDisplayName(sender, emptyFieldType.Name);
					RaiseAvailabilityException(sender, row, emptyFieldType, Messages.IncompleteLinkToOrigInvoiceLine, true, emptyFieldDisplayName);
				}
			}

			return MemoAvailabilityCheckQty(sender, row.InventoryID, row.OrigInvoiceType, row.OrigInvoiceNbr, row.OrigInvoiceLineNbr, null, null, null);
		}

		public virtual void OrderAvailabilityCheck(PXCache sender, ARTran row, bool onPersist = false)
		{
			if (row.InvtMult == 0 || row.SOOrderType == null && row.SOOrderNbr == null && row.SOOrderLineNbr == null || row.Released == true)
				return;

			if (row.LineType == SOLineType.MiscCharge)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.inventoryID), Messages.NonStockNoShipCantBeInvoicedDirectly, onPersist,
					sender.GetValueExt<ARTran.inventoryID>(row));
			}

			var line = SOLine.PK.Find(sender.Graph, row.SOOrderType, row.SOOrderNbr, row.SOOrderLineNbr);
			if (line == null)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.sOOrderNbr), Messages.SOLineNotFound, onPersist);
			}

			SOOrderType orderType = SOOrderType.PK.Find(sender.Graph, line.OrderType);
			if (orderType.OrderType == null || orderType.RequireShipping == false || orderType.ARDocType == ARDocType.NoUpdate)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.sOOrderType), Messages.SOTypeCantBeInvoicedDirectly, onPersist,
					sender.GetValueExt<ARTran.sOOrderType>(row));
			}

			if (line.Completed == true)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.sOOrderNbr), Messages.CompletedSOLineCantBeInvoicedDirectly, onPersist);
			}

			if (line.CustomerID != row.CustomerID)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.sOOrderNbr), Messages.CustomerDiffersInvoiceAndSO, onPersist);
			}

			if (line.POCreate == true)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.sOOrderNbr), Messages.SOLineMarkedForPOCantBeInvoicedDirectly, onPersist);
			}

			if (line.InventoryID != row.InventoryID)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.inventoryID), Messages.InventoryItemDiffersInvoiceAndSO, onPersist);
			}

			int arTranInvtMult = Math.Sign((row.InvtMult * row.Qty) ?? 0m);
			if (arTranInvtMult != 0)
			{
				int soLineInvtMult = (line.Operation == SOOperation.Receipt) ? 1 : -1;
				if (soLineInvtMult != arTranInvtMult)
				{
					RaiseAvailabilityException(sender, row, typeof(ARTran.qty), Messages.OperationDiffersInvoiceAndSO, onPersist);
				}
			}

			decimal absQty = Math.Abs(row.BaseQty ?? 0m);
			if (PXDBQuantityAttribute.Round((decimal)(line.BaseOrderQty * line.CompleteQtyMax / 100m - line.BaseShippedQty - absQty)) < 0m)
			{
				RaiseAvailabilityException(sender, row, typeof(ARTran.qty), Messages.OrderCheck_QtyNegative, onPersist,
					sender.GetValueExt<ARTran.inventoryID>(row), sender.GetValueExt<ARTran.subItemID>(row),
					sender.GetValueExt<ARTran.sOOrderType>(row), sender.GetValueExt<ARTran.sOOrderNbr>(row));
			}
		}

		protected virtual void RaiseAvailabilityException(PXCache sender, object row, Type field, string errorMessage, bool onPersist, params object[] args)
		{
			var propertyException = new PXSetPropertyException(errorMessage, args);
			if (onPersist)
			{
				object value = sender.GetValueExt(row, field.Name);
				bool raised = sender.RaiseExceptionHandling(field.Name, row, value, propertyException);
				if (raised)
				{
					throw new PXRowPersistingException(field.Name, value, errorMessage, args);
				}
			}
			else
			{
				throw propertyException;
			}
		}

		protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = (ARTran)e.Row;
			if (row.InvtMult != (short)0)
			{
				base.Master_RowInserted(sender, e);
			}
		}

		protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (((ARTran)e.Row).InvtMult != (short)0)
			{
				base.Master_RowDeleted(sender, e);
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (((ARTran)e.Row).InvtMult != (short)0)
			{
				if (Equals(((ARTran)e.Row).TranType, ((ARTran)e.OldRow).TranType) == false)
				{
					sender.SetDefaultExt<ARTran.invtMult>(e.Row);
				}

				base.Master_RowUpdated(sender, e);
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
		{
			if (row is ARTran)
			{
				sender.RaiseExceptionHandling<ARTran.qty>(row, null, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<ARTran.inventoryID>(row), sender.GetStateExt<ARTran.subItemID>(row), sender.GetStateExt<ARTran.siteID>(row), sender.GetStateExt<ARTran.locationID>(row), sender.GetValue<ARTran.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<ARTranAsSplit.qty>(row, null, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<ARTranAsSplit.inventoryID>(row), sender.GetStateExt<ARTranAsSplit.subItemID>(row), sender.GetStateExt<ARTranAsSplit.siteID>(row), sender.GetStateExt<ARTranAsSplit.locationID>(row), sender.GetValue<ARTranAsSplit.lotSerialNbr>(row)));
			}
		}

		#endregion

		protected override PXSelectBase<INLotSerialStatus> GetSerialStatusCmdBase(PXCache sender, ARTran Row, PXResult<InventoryItem, INLotSerClass> item)
		{
			return new PXSelectJoin<INLotSerialStatus,
				InnerJoin<INLocation, 
					On<INLotSerialStatus.FK.Location>,
				InnerJoin<INSiteLotSerial, On<INSiteLotSerial.inventoryID, Equal<INLotSerialStatus.inventoryID>,
					And<INSiteLotSerial.siteID, Equal<INLotSerialStatus.siteID>,
					And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>>>,
				Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
					And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>,
					And<INLotSerialStatus.qtyOnHand, Greater<decimal0>,
					And<INSiteLotSerial.qtyHardAvail, Greater<decimal0>>>>>>(sender.Graph);
		}

		protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatus> cmd, ARTran row, INLotSerClass lotSerClass)
		{
			if (row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
			}

			if(!string.IsNullOrEmpty(row.LotSerialNbr))
				cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			else if (lotSerClass.IsManualAssignRequired == true)
						cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
			}

		public override ARTranAsSplit Convert(ARTran item)
		{
			using (new InvtMultScope<ARTran>(item))
			{
				ARTranAsSplit ret = ARTranAsSplit.FromARTran(item);
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}

		public override void UpdateParent(PXCache sender, ARTran Row, ARTranAsSplit Det, ARTranAsSplit OldDet, out decimal BaseQty)
		{
			ARTran oldRow = (ARTran)sender.CreateCopy(Row);

			base.UpdateParent(sender, Row, Det, OldDet, out BaseQty);

			if (!sender.ObjectsEqual<ARTran.subItemID, ARTran.locationID, ARTran.lotSerialNbr, ARTran.expireDate>(oldRow, Row))
			{
				ARTranPlanIDAttribute.RaiseRowUpdated(sender, Row, oldRow);
			}
		}

		public override SOInvoicedRecords SelectInvoicedRecords(string arDocType, string arRefNbr)
		{
			return SelectInvoicedRecords(arDocType, arRefNbr, includeDirectLines: true);
		}

		protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
		{
			string inTranType = INTranType.TranTypeFromInvoiceType(row.TranType, row.Qty);
			return INLotSerialNbrAttribute.TranTrackMode(lotSerClass, inTranType, row.InvtMult);
		}

		public class ARLotSerialNbrAttribute : INLotSerialNbrAttribute
		{
			public ARLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType)
				: base(InventoryType, SubItemType, LocationType)
			{
			}

			protected override bool IsTracked(ILSMaster row, INLotSerClass lotSerClass, string tranType, int? invMult)
			{
				string inTranType = INTranType.TranTypeFromInvoiceType(tranType, row.Qty);
				return (invMult == 0) ? false : base.IsTracked(row, lotSerClass, inTranType, invMult);
			}
		}

		public class ARExpireDateAttribute : INExpireDateAttribute
		{
			public ARExpireDateAttribute(Type InventoryType)
				: base(InventoryType)
			{
			}

			protected override bool IsTrackExpiration(PXCache sender, ILSMaster row)
			{
				return (row.InvtMult == 0) ? false : base.IsTrackExpiration(sender, row);
			}
		}
	}
}
