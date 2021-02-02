using System;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;

namespace PX.Objects.AM
{
    public class LSAMDisassembleMasterTran : LSSelect<AMDisassembleBatch, AMDisassembleBatchSplit, 
        Where<AMDisassembleBatchSplit.docType, Equal<Current<AMDisassembleBatch.docType>>, 
        And<AMDisassembleBatchSplit.batNbr, Equal<Current<AMDisassembleBatch.batchNbr>>>>>
    {
        #region Ctor
        public LSAMDisassembleMasterTran(PXGraph graph)
            : base(graph)
        {
            graph.FieldDefaulting.AddHandler<AMDisassembleBatchSplit.subItemID>(AMDisassembleBatchSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMDisassembleBatchSplit.locationID>(AMDisassembleBatchSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMDisassembleBatchSplit.invtMult>(AMDisassembleBatchSplit_InvtMult_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMDisassembleBatchSplit.lotSerialNbr>(AMDisassembleBatchSplit_LotSerialNbr_FieldDefaulting);
            graph.FieldVerifying.AddHandler<AMDisassembleBatchSplit.qty>(AMDisassembleBatchSplit_Qty_FieldVerifying);
            graph.FieldVerifying.AddHandler<AMDisassembleBatch.qty>(AMDisassembleBatch_Qty_FieldVerifying);
            graph.RowUpdated.AddHandler<AMDisassembleBatch>(AMDisassembleBatch_RowUpdated);
        }
        #endregion

        #region Implementation
        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                var availability = AvailabilityFetch(sender, (AMDisassembleBatch)e.Row, e.Row != null && ((AMDisassembleBatch)e.Row).Released == true ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMDisassembleBatch)e.Row).InventoryID);

                    availability.QtyOnHand = INUnitAttribute.ConvertFromBase<AMDisassembleBatch.inventoryID, AMDisassembleBatch.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
                    availability.QtyAvail = INUnitAttribute.ConvertFromBase<AMDisassembleBatch.inventoryID, AMDisassembleBatch.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
                    availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<AMDisassembleBatch.inventoryID, AMDisassembleBatch.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
                    availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<AMDisassembleBatch.inventoryID, AMDisassembleBatch.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);
                    availability.QtyActual = INUnitAttribute.ConvertFromBase<AMDisassembleBatch.inventoryID, AMDisassembleBatch.uOM>(sender, e.Row, (decimal)availability.QtyActual, INPrecision.QUANTITY);

                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(PX.Objects.IN.Messages.Availability_ActualInfo,
                        sender.GetValue<AMMTran.uOM>(e.Row),
                        FormatQty(availability.QtyOnHand),
                        FormatQty(availability.QtyAvail),
                        FormatQty(availability.QtyHardAvail),
                        FormatQty(availability.QtyActual));

                    AvailabilityCheck(sender, (AMDisassembleBatch)e.Row, availability);
                }
                else
                {
                    e.ReturnValue = string.Empty;
                }
            }
            else
            {
                e.ReturnValue = string.Empty;
            }

            base.Availability_FieldSelecting(sender, e);
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (AMDisassembleBatch)e.Row;
            if (row?.InventoryID == null || row.UOM == null || row.InvtMult.GetValueOrDefault() == 0)
            {
                return;
            }

            base.Master_RowUpdated(sender, e);
        }

        public override AMDisassembleBatchSplit Convert(AMDisassembleBatch item)
        {
            using (new LSSelect<AMDisassembleBatch, AMDisassembleBatchSplit,
                Where<AMDisassembleBatchSplit.docType, Equal<Current<AMDisassembleBatch.docType>>,
                And<AMDisassembleBatchSplit.batNbr, Equal<Current<AMDisassembleBatch.batchNbr>>
                >>>.InvtMultScope<AMDisassembleBatch>(item))
            {
                AMDisassembleBatchSplit inTranSplit1 = (AMDisassembleBatchSplit)item;
                AMDisassembleBatchSplit inTranSplit2 = inTranSplit1;
                Decimal? baseQty = item.BaseQty;
                Decimal? unassignedQty = item.UnassignedQty;
                Decimal? nullable = baseQty.HasValue & unassignedQty.HasValue ? new Decimal?(baseQty.GetValueOrDefault() - unassignedQty.GetValueOrDefault()) : new Decimal?();
                inTranSplit2.BaseQty = nullable;
                return inTranSplit1;
            }
        }

        public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
            where Field : IBqlField
        {
            if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
            {
                throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
            }
        }

        public virtual void AMMTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                if (((AMDisassembleBatchSplit)e.Row).BaseQty != 0m && ((AMDisassembleBatchSplit)e.Row).LocationID == null)
                {
                    ThrowFieldIsEmpty<AMDisassembleBatchSplit.locationID>(sender, e.Row);
                }
            }
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                AMDisassembleBatch row = e.Row as AMDisassembleBatch;
                if (row != null && row.Hold == false && Math.Abs((decimal)row.BaseQty) >= 0.0000005m && (row.UnassignedQty >= 0.0000005m || row.UnassignedQty <= -0.0000005m))
                {
                    if (sender.RaiseExceptionHandling<AMDisassembleBatch.qty>(e.Row, row.Qty, new PXSetPropertyException(PX.Objects.IN.Messages.BinLotSerialNotAssigned)))
                    {
                        throw new PXRowPersistingException(typeof(AMDisassembleBatch.qty).Name, row.Qty, PX.Objects.IN.Messages.BinLotSerialNotAssigned);
                    }
                }
            }
            base.Master_RowPersisting(sender, e);
        }

        protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            var row = (AMDisassembleBatch)e.Row;
            if (row.InvtMult != (short)0)
            {
                //base.Master_RowInserted(sender, e);
            }
            else
            {
                //this piece of code supposed to support dropships and landed costs for dropships.ReceiptCostAdjustment is generated for landedcosts and ppv adjustments, so we need actual lotSerialNbr, thats why it has to stay
                if (row.TranType == AMTranType.Disassembly)
                {
                    sender.SetValue<AMMTran.lotSerialNbr>(e.Row, null);
                    sender.SetValue<AMMTran.expireDate>(e.Row, null);
                }
            }
        }

        protected virtual void AMDisassembleBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (!sender.ObjectsEqual<AMDisassembleBatch.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<AMDisassembleBatch.hold>(e.Row) == false)
            {
                if (((AMDisassembleBatch)e.Row).UnassignedQty != 0)
                {
                    sender.RaiseExceptionHandling<AMDisassembleBatch.qty>(e.Row, ((AMDisassembleBatch)e.Row).Qty, new PXSetPropertyException(PX.Objects.IN.Messages.BinLotSerialNotAssigned));
                }
            }
        }

        public override void UpdateParent(PXCache sender, AMDisassembleBatch Row, AMDisassembleBatchSplit Det, AMDisassembleBatchSplit OldDet, out decimal BaseQty)
        {
            base.UpdateParent(sender, Row, Det, OldDet, out BaseQty);
            if (counters.RecordCount > 0)
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
                INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, Row.TranType, Row.InvtMult);
                if (mode == INLotSerTrack.Mode.None)
                {
                    Row.LotSerialNbr = string.Empty;
                }
                else if ((mode & INLotSerTrack.Mode.Create) > 0 || (mode & INLotSerTrack.Mode.Issue) > 0)
                {
                    //if more than 1 split exist at lotserial creation time ignore equalness and display <SPLIT>
                    Row.LotSerialNbr = null;
                }
            }
        }

        public virtual void AMDisassembleBatchSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMDisassembleBatch)];
            if (cache.Current != null && (e.Row == null || ((AMDisassembleBatch)cache.Current).LineNbr == ((AMDisassembleBatchSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMDisassembleBatch)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public virtual void AMDisassembleBatchSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMDisassembleBatch)];
            if (cache.Current != null && (e.Row == null || ((AMDisassembleBatch)cache.Current).LineNbr == ((AMDisassembleBatchSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMDisassembleBatch)cache.Current).LocationID;
                e.Cancel = true;
            }
        }

        public virtual void AMDisassembleBatchSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMDisassembleBatch)];
            if (cache.Current != null && (e.Row == null || ((AMDisassembleBatch)cache.Current).LineNbr == ((AMDisassembleBatchSplit)e.Row).LineNbr))
            {
                using (InvtMultScope<AMDisassembleBatch> ms = new InvtMultScope<AMDisassembleBatch>((AMDisassembleBatch)cache.Current))
                {
                    e.NewValue = ((AMDisassembleBatch)cache.Current).InvtMult;
                    e.Cancel = true;
                }
            }
        }

        public virtual void AMDisassembleBatchSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMDisassembleBatchSplit)e.Row).InventoryID);

            if (item != null)
            {
                object InvtMult = ((AMDisassembleBatchSplit)e.Row).InvtMult;
                if (InvtMult == null)
                {
                    sender.RaiseFieldDefaulting<AMDisassembleBatchSplit.invtMult>(e.Row, out InvtMult);
                }

                object TranType = ((AMDisassembleBatchSplit)e.Row).TranType;
                if (TranType == null)
                {
                    sender.RaiseFieldDefaulting<AMDisassembleBatchSplit.tranType>(e.Row, out TranType);
                }

                INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, (string)TranType, (short?)InvtMult);
                if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
                {
                    foreach (AMDisassembleBatchSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<AMDisassembleBatchSplit>(sender, item, mode, 1m))
                    {
                        e.NewValue = lssplit.LotSerialNbr;
                        e.Cancel = true;
                    }
                }
                //otherwise default via attribute
            }
        }

        public virtual void AMDisassembleBatchSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMDisassembleBatchSplit)e.Row).InventoryID);

            if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((AMDisassembleBatchSplit)e.Row).TranType, ((AMDisassembleBatchSplit)e.Row).InvtMult))
            {
                if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m)
                {
                    e.NewValue = 1m;
                }
            }
        }

        public virtual void AMDisassembleBatch_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((decimal?)e.NewValue < 0m)
            {
                throw new PXSetPropertyException(PX.Objects.CS.Messages.Entry_GE, PXErrorLevel.Error, (int)0);
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo e)
        {
            if (row is AMDisassembleBatch)
            {
                sender.RaiseExceptionHandling<AMDisassembleBatch.qty>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<AMDisassembleBatch.inventoryID>(row), sender.GetStateExt<AMDisassembleBatch.subItemID>(row), sender.GetStateExt<AMDisassembleBatch.siteID>(row), sender.GetStateExt<AMDisassembleBatch.locationID>(row), sender.GetValue<AMDisassembleBatch.lotSerialNbr>(row)));
            }
            else
            {
                sender.RaiseExceptionHandling<AMDisassembleBatchSplit.qty>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<AMDisassembleBatchSplit.inventoryID>(row), sender.GetStateExt<AMDisassembleBatchSplit.subItemID>(row), sender.GetStateExt<AMDisassembleBatchSplit.siteID>(row), sender.GetStateExt<AMDisassembleBatchSplit.locationID>(row), sender.GetValue<AMDisassembleBatchSplit.lotSerialNbr>(row)));
            }
        }

        protected override object[] SelectDetail(PXCache sender, AMDisassembleBatch row)
        {
            PXSelectBase<AMDisassembleBatchSplit> select = new PXSelect<AMDisassembleBatchSplit,
                Where<AMDisassembleBatchSplit.docType, Equal<Required<AMDisassembleBatch.docType>>,
                And<AMDisassembleBatchSplit.batNbr, Equal<Required<AMDisassembleBatch.batchNbr>>,
                And<AMDisassembleBatchSplit.lineNbr, Equal<Required<AMDisassembleBatch.refLineNbr>>>>>>(_Graph);

            PXResultset<AMDisassembleBatchSplit> res = select.Select(row.DocType, row.BatchNbr, row.RefLineNbr);

            List<object> list = new List<object>(res.Count);

            foreach (AMDisassembleBatchSplit detail in res)
            {
                list.Add(detail);
            }

            return list.ToArray();
        }

        protected override object[] SelectDetail(PXCache sender, AMDisassembleBatchSplit row)
        {
            AMDisassembleBatch refRow = (AMDisassembleBatch)PXParentAttribute.SelectParent(sender, row, typeof(AMDisassembleBatch));

            return SelectDetail(sender, refRow);
        }


        #endregion
    }
}
