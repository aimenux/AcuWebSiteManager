using System;
using PX.Data;
using PX.Objects.IN;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.AM
{
    public class LSAMDisassembleMaterialTran : LSSelect<AMDisassembleTran, AMDisassembleTranSplit, 
        Where<AMDisassembleTranSplit.docType, Equal<Current<AMDisassembleBatch.docType>>, 
        And<AMDisassembleTranSplit.batNbr, Equal<Current<AMDisassembleBatch.batchNbr>>>>>
    {
        public LSAMDisassembleMaterialTran(PXGraph graph)
            : base(graph)
        {
            this.MasterQtyField = typeof(AMDisassembleTran.qty);
            graph.FieldDefaulting.AddHandler<AMDisassembleTranSplit.subItemID>(AMDisassembleTranSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMDisassembleTranSplit.locationID>(AMDisassembleTranSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMDisassembleTranSplit.invtMult>(AMDisassembleTranSplit_InvtMult_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMDisassembleTranSplit.lotSerialNbr>(AMDisassembleTranSplit_LotSerialNbr_FieldDefaulting);
            graph.FieldVerifying.AddHandler<AMDisassembleTranSplit.qty>(AMDisassembleTranSplit_Qty_FieldVerifying);
            graph.FieldVerifying.AddHandler<AMDisassembleTran.qty>(AMDisassembleTran_Qty_FieldVerifying);
            graph.RowUpdated.AddHandler<AMDisassembleBatch>(AMDisassembleBatch_RowUpdated);
            graph.RowUpdated.AddHandler<AMDisassembleTran>(AMDisassembleTran_RowUpdated);
        }

        protected virtual void AMDisassembleBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (!sender.ObjectsEqual<AMDisassembleBatch.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<AMDisassembleBatch.hold>(e.Row) == false)
            {
                PXCache cache = sender.Graph.Caches[typeof(AMDisassembleTran)];

                foreach (AMDisassembleTran item in PXParentAttribute.SelectSiblings(cache, null, typeof(AMDisassembleBatch)))
                {
                    if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
                    {
                        cache.RaiseExceptionHandling<AMDisassembleTran.qty>(item, item.Qty, new PXSetPropertyException(PX.Objects.IN.Messages.BinLotSerialNotAssigned));

                        if (cache.GetStatus(item) == PXEntryStatus.Notchanged)
                        {
                            cache.SetStatus(item, PXEntryStatus.Updated);
                        }
                    }
                }
            }
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                PXCache cache = sender.Graph.Caches[typeof(AMDisassembleBatch)];
                object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(AMDisassembleBatch)) ?? cache.Current;

                bool? OnHold = (bool?)cache.GetValue<AMDisassembleBatch.hold>(doc);

                if (OnHold == false && Math.Abs((decimal)((AMDisassembleTran)e.Row).BaseQty) >= 0.0000005m && (((AMDisassembleTran)e.Row).UnassignedQty >= 0.0000005m || ((AMDisassembleTran)e.Row).UnassignedQty <= -0.0000005m))
                {
                    if (sender.RaiseExceptionHandling<AMDisassembleTran.qty>(e.Row, ((AMDisassembleTran)e.Row).Qty, new PXSetPropertyException(PX.Objects.IN.Messages.BinLotSerialNotAssigned)))
                    {
                        throw new PXRowPersistingException(typeof(AMDisassembleTran.qty).Name, ((AMDisassembleTran)e.Row).Qty, PX.Objects.IN.Messages.BinLotSerialNotAssigned);
                    }
                }
            }
            base.Master_RowPersisting(sender, e);
        }

        protected virtual void AMDisassembleTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            AMDisassembleTran row = (AMDisassembleTran)e.Row;
            if (row == null) return;
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                var availability = AvailabilityFetch(sender, (AMDisassembleTran)e.Row, AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMDisassembleTran)e.Row).InventoryID);

                    availability.QtyOnHand = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
                    availability.QtyAvail = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
                    availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
                    availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);

                    AvailabilityCheck(sender, (AMDisassembleTran)e.Row, availability);
                }
            }
        }
        protected override DateTime? ExpireDateByLot(PXCache sender, ILSMaster item, ILSMaster master)
        {
            if (master != null && master.InvtMult > 0)
            {
                item.ExpireDate = null;
                return base.ExpireDateByLot(sender, item, null);
            }
            else return base.ExpireDateByLot(sender, item, master);
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                var availability = AvailabilityFetch(sender, (AMDisassembleTran)e.Row, e.Row != null && ((AMDisassembleTran)e.Row).Released == true ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMDisassembleTran)e.Row).InventoryID);

                    availability.QtyOnHand = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
                    availability.QtyAvail = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
                    availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
                    availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);
                    availability.QtyActual = INUnitAttribute.ConvertFromBase<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(sender, e.Row, (decimal)availability.QtyActual, INPrecision.QUANTITY);

                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(PX.Objects.IN.Messages.Availability_ActualInfo,
                        sender.GetValue<AMMTran.uOM>(e.Row),
                        FormatQty(availability.QtyOnHand),
                        FormatQty(availability.QtyAvail),
                        FormatQty(availability.QtyHardAvail),
                        FormatQty(availability.QtyActual));
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

        public override void AvailabilityCheck(PXCache sender, ILSMaster Row, IQtyAllocated availability)
        {
            base.AvailabilityCheck(sender, Row, availability);

            if (Row.InvtMult == (short)-1 && Row.BaseQty > 0m)
            {
                if (availability != null && availability.QtyAvail < Row.Qty)
                {
                    if (availability is LotSerialStatus)
                    {
                        RaiseQtyExceptionHandling(sender, Row, Row.Qty, new PXExceptionInfo(PX.Objects.IN.Messages.StatusCheck_QtyLotSerialNegative));
                    }
                    else if (availability is LocationStatus)
                    {
                        RaiseQtyExceptionHandling(sender, Row, Row.Qty, new PXExceptionInfo(PX.Objects.IN.Messages.StatusCheck_QtyLocationNegative));
                    }
                    else if (availability is SiteStatus)
                    {
                        RaiseQtyExceptionHandling(sender, Row, Row.Qty, new PXExceptionInfo(PX.Objects.IN.Messages.StatusCheck_QtyNegative));
                    }
                }
            }
        }

        public override AMDisassembleTranSplit Convert(AMDisassembleTran item)
        {
            using (var ms = new InvtMultScope<AMDisassembleTran>(item))
            {
                AMDisassembleTranSplit ret = item;
                ret.BaseQty = item.BaseQty - item.UnassignedQty;
                return ret;
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

        public virtual void AMDisassembleTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                if (((AMDisassembleTranSplit)e.Row).BaseQty != 0m && ((AMDisassembleTranSplit)e.Row).LocationID == null)
                {
                    ThrowFieldIsEmpty<AMDisassembleTranSplit.locationID>(sender, e.Row);
                }
            }
        }

        public virtual void AMDisassembleTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMDisassembleTran)];
            if (cache.Current != null && (e.Row == null || ((AMDisassembleTran)cache.Current).LineNbr == ((AMDisassembleTranSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMDisassembleTran)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public virtual void AMDisassembleTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMDisassembleTran)];
            if (cache.Current != null && (e.Row == null || ((AMDisassembleTran)cache.Current).LineNbr == ((AMDisassembleTranSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMDisassembleTran)cache.Current).LocationID;
                e.Cancel = true;
            }
        }

        public virtual void AMDisassembleTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var cache = sender.Graph.Caches[typeof(AMDisassembleTran)];
            if (cache.Current != null && (e.Row == null || ((AMDisassembleTran)cache.Current).LineNbr == ((AMDisassembleTranSplit)e.Row).LineNbr))
            {
                using (var ms = new InvtMultScope<AMDisassembleTran>((AMDisassembleTran)cache.Current))
                {
                    e.NewValue = ((AMDisassembleTran)cache.Current).InvtMult ?? (short)1;
                    e.Cancel = true;
                }
            }
        }

        public virtual void AMDisassembleTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMDisassembleTranSplit)e.Row).InventoryID);

            if (item != null)
            {
                object InvtMult = ((AMDisassembleTranSplit)e.Row).InvtMult;
                if (InvtMult == null)
                {
                    sender.RaiseFieldDefaulting<AMDisassembleTranSplit.invtMult>(e.Row, out InvtMult);
                }

                object TranType = ((AMDisassembleTranSplit)e.Row).TranType;
                if (TranType == null)
                {
                    sender.RaiseFieldDefaulting<AMDisassembleTranSplit.tranType>(e.Row, out TranType);
                }

                if ((short?)InvtMult == 1 && ((INLotSerClass)item).LotSerAssign == INLotSerAssign.WhenReceived || ((INLotSerClass)item).LotSerAssign == INLotSerAssign.WhenUsed)
                {
                    INLotSerTrack.Mode Mode = INLotSerTrack.Mode.None;
                    foreach (AMDisassembleTranSplit split in INLotSerialNbrAttribute.CreateNumbers<AMDisassembleTranSplit>(sender, item, Mode, 1M))
                    {
                        e.NewValue = split.LotSerialNbr;
                        e.Cancel = true;
                    }
                }
            }
        }

        public virtual void AMDisassembleTranSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMDisassembleTranSplit)e.Row).InventoryID);

            if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((AMDisassembleTranSplit)e.Row).TranType, ((AMDisassembleTranSplit)e.Row).InvtMult))
            {
                if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m)
                {
                    e.NewValue = 1m;
                }
            }
        }

        public virtual void AMDisassembleTran_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((decimal?)e.NewValue <= 0m)
            {
                throw new PXSetPropertyException(PX.Objects.CS.Messages.Entry_GT, PXErrorLevel.Error, 0);
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo e)
        {
            if (row is AMDisassembleTran)
            {
                sender.RaiseExceptionHandling<AMDisassembleTran.qty>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<AMDisassembleTran.inventoryID>(row), sender.GetStateExt<AMDisassembleTran.subItemID>(row), sender.GetStateExt<AMDisassembleTran.siteID>(row), sender.GetStateExt<AMDisassembleTran.locationID>(row), sender.GetValue<AMDisassembleTran.lotSerialNbr>(row)));
            }
            else
            {
                sender.RaiseExceptionHandling<AMDisassembleTranSplit.qty>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<AMDisassembleTranSplit.inventoryID>(row), sender.GetStateExt<AMDisassembleTranSplit.subItemID>(row), sender.GetStateExt<AMDisassembleTranSplit.siteID>(row), sender.GetStateExt<AMDisassembleTranSplit.locationID>(row), sender.GetValue<AMDisassembleTranSplit.lotSerialNbr>(row)));
            }
        }
    }
}
