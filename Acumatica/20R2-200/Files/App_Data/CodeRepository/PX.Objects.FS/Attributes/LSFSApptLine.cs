using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Common;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.GL;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;
using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using System.Globalization;
using PX.Objects.CM;
using PX.Objects.CA;
using System.Linq;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Data.BQL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Exceptions;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    public class LSFSApptLine : SO.LSSelectSOBase<FSAppointmentDet, FSApptLineSplit,
        Where<FSApptLineSplit.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>,
            And<FSApptLineSplit.apptNbr, Equal<Current<FSAppointment.refNbr>>>>>
    {
        #region State
        public virtual bool IsLotSerialRequired
        {
            get
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(MasterCache, MasterCurrent.InventoryID);
                var lsClass = (INLotSerClass)item;
                if (lsClass == null || lsClass.LotSerTrack == null || lsClass.LotSerTrack == INLotSerTrack.NotNumbered)
                    return false;

                return true;
            }
        }

        protected FSINLotSerialNbrAttribute _LotSerialSelector = null;

        public delegate void VerifySrvOrdQtyDelegate(PXCache cache, FSAppointmentDet apptLine, object newValue, Type QtyField, bool runningFieldVerifying);
        public VerifySrvOrdQtyDelegate VerifySrvOrdQtyMethod { get; set; }
        #endregion
        #region Ctor
        public LSFSApptLine(PXGraph graph)
            : base(graph)
        {
            MasterQtyField = typeof(FSAppointmentDet.effTranQty);
            graph.FieldDefaulting.AddHandler<FSApptLineSplit.subItemID>(FSApptLineSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSApptLineSplit.locationID>(FSApptLineSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSApptLineSplit.invtMult>(FSApptLineSplit_InvtMult_FieldDefaulting);
            graph.RowPersisting.AddHandler<FSAppointmentDet>(FSAppointmentDet_RowPersisting);
            graph.RowPersisting.AddHandler<FSApptLineSplit>(FSApptLineSplit_RowPersisting);

            graph.RowUpdating.AddHandler<FSApptLineSplit>(Detail_RowUpdating);
            graph.RowDeleting.AddHandler<FSApptLineSplit>(Detail_RowDeleting);
            graph.RowSelected.AddHandler<FSApptLineSplit>(FSApptLineSplit_RowSelected);

            graph.FieldUpdated.AddHandler<FSApptLineSplit.lotSerialNbr>(FSApptLineSplit_lotSerialNbr_FieldUpdated);

            var allocationAction = graph.Actions[Prefixed("binLotSerial")];
            if (allocationAction != null)
            {
                allocationAction.SetCaption(TX.Messages.LotsSerials);
                allocationAction.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.inventory>());
            }
        }

        #endregion
        public override IEnumerable BinLotSerial(PXAdapter adapter)
        {
            if (MasterCache.Current == null)
                return adapter.Get();

            FSAppointmentDet detail = (FSAppointmentDet)MasterCache.Current;

            if (detail.InventoryID == null)
            {
                throw new PXException(TX.Error.NotValidFunctionWithInstructionOrCommentLines);
            }

            if ((detail.LineType == ID.LineType_ALL.SERVICE
                 || detail.LineType == ID.LineType_ALL.NONSTOCKITEM)
                 && detail.EnablePO == false)
            {
                throw new PXException(SO.Messages.BinLotSerialInvalid);
            }

            detail.IsLotSerialRequired = IsLotSerialRequired;

            View.AskExt(true);

            return adapter.Get();
        }

        #region Implementation

        private void FSApptLineSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            bool IsLotSerialRequired = ((FSAppointmentDet)MasterCache.Current)?.IsLotSerialRequired == true;

            DetailCache.AllowInsert = IsLotSerialRequired;
            DetailCache.AllowDelete = IsLotSerialRequired;
            DetailCache.AllowUpdate = IsLotSerialRequired;


            var row = (FSApptLineSplit)e.Row;
            if (row == null)
                return;

            PXUIFieldAttribute.SetEnabled<FSApptLineSplit.subItemID>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<FSApptLineSplit.siteID>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<FSApptLineSplit.locationID>(sender, e.Row, false);
        }

        public override FSAppointmentDet CloneMaster(FSAppointmentDet item)
        {
            FSAppointmentDet copy = base.CloneMaster(item);
            copy.OrigSrvOrdNbr = null;
            copy.OrigLineNbr = null;

            return copy;
        }

        protected virtual void OrderAvailabilityCheck(PXCache sender, FSAppointmentDet Row)
        {
        }

        public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
        {
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            using (ResolveNotDecimalUnitErrorRedirectorScope<FSApptLineSplit.qty>(e.Row))
            {
                base.Master_RowUpdated(sender, e);
                if (!sender.ObjectsEqual<FSAppointmentDet.lotSerialNbr>(e.Row, e.OldRow))
                {
                    sender.RaiseFieldUpdated<FSAppointmentDet.lotSerialNbr>(e.Row, ((FSAppointmentDet)e.OldRow).LotSerialNbr);
                }
            }
        }
        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            FSAppointmentDet apptLine = (FSAppointmentDet)e.Row;

            if (apptLine.InventoryID != null
                    && apptLine.Status == FSAppointmentDet.status.COMPLETED
                    && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            )
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, apptLine.InventoryID);
                if (item == null)
                {
                    throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(InventoryItem)));
                }

                string lotSerTrack = ((INLotSerClass)item).LotSerTrack;

                if (lotSerTrack == INLotSerTrack.SerialNumbered || lotSerTrack == INLotSerTrack.LotNumbered)
                {
                    decimal baseExistingSplitTotalQty;
                    decimal existingSplitTotalQty;
                    GetExistingSplits(apptLine, out baseExistingSplitTotalQty, out existingSplitTotalQty);

                    if (apptLine.EffTranQty != existingSplitTotalQty)
                    {
                        PXException exception = new PXSetPropertyException(TX.Error.CannotCompleteLineBecauseLotSerialTotalQtyDoesNotMatchItemLineQty, PXErrorLevel.Error);

                        sender.RaiseExceptionHandling<FSAppointmentDet.status>(apptLine, apptLine.Status, exception);
                    }
                }
            }

            base.Master_RowPersisting(sender, e);
        }

        public int? lastComponentID = null;
        protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<FSAppointmentDet> e)
        {
            if (lastComponentID == e.Row.InventoryID)
            {
                return;
            }

            if (e.Row.IsCanceledNotPerformed == true)
            {
                if (!sender.Graph.IsContractBasedAPI)
                {
                    if (e.OldRow.InventoryID != e.Row.InventoryID)
                    {
                        ((FSAppointmentDet)e.Row).LotSerialNbr = null;
                        ((FSAppointmentDet)e.Row).ExpireDate = null;
                    }
                    else if (e.OldRow.InvtMult != e.Row.InvtMult)
                    {
                        if (((FSAppointmentDet)e.Row).LotSerialNbr == ((FSAppointmentDet)e.OldRow).LotSerialNbr)
                        {
                            ((FSAppointmentDet)e.Row).LotSerialNbr = null;
                        }
                        if (((FSAppointmentDet)e.Row).ExpireDate == ((FSAppointmentDet)e.OldRow).ExpireDate)
                        {
                            ((FSAppointmentDet)e.Row).ExpireDate = null;
                        }
                    }
                }

                RaiseRowDeleted(sender, e.OldRow);
            }
            else
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);
                INLotSerClass lotSerClass = (INLotSerClass)item;

                if (string.IsNullOrEmpty(e.Row.LotSerialNbr) == false
                        && e.Row.LotSerialNbr != e.OldRow.LotSerialNbr
                        && (lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered || lotSerClass.LotSerTrack == INLotSerTrack.LotNumbered)
                )
                {
                    UpdateLotSerialSplitsBasedOnLineLotSerial(sender, e.Row, lotSerClass.LotSerTrack, lotSerClass.LotSerTrackExpiration);
                }

                InsertLotSerialsFromServiceOrder(sender, e.Row);

                _TruncateNumbers(sender, e.Row, (decimal)e.Row.BaseQty);

                _UpdateParent(sender, e.Row);
            }
        }

        protected virtual void UpdateLotSerialSplitsBasedOnLineLotSerial(PXCache apptLineCache, FSAppointmentDet apptLine, string lotSerTrack, bool? lotSerTrackExpiration)
        {
            if (string.IsNullOrEmpty(apptLine.LotSerialNbr) == true || lotSerTrack == INLotSerTrack.NotNumbered)
            {
                return;
            }

            decimal baseExistingSplitTotalQty;
            List<FSApptLineSplit> existingSplits = GetExistingSplits(apptLine, out baseExistingSplitTotalQty);

            FSApptLineSplit lotSerialSplit = null;

            // Delete all splits except the split with the specified Lot/Serial
            foreach (FSApptLineSplit split in existingSplits)
            {
                if (split.LotSerialNbr == apptLine.LotSerialNbr)
                {
                    lotSerialSplit = split;
                }
                else
                {
                    DetailCache.Delete(split);
                }
            }

            decimal? qtyDefault;
            GetLotSerialQtyDefault(DetailCache, apptLine.LotSerialNbr, apptLine, lotSerTrack, out qtyDefault);

            if (qtyDefault > apptLine.EffTranQty)
            {
                qtyDefault = apptLine.EffTranQty;
            }

            if (lotSerialSplit == null && qtyDefault > 0m)
            {
                lotSerialSplit = (FSApptLineSplit)DetailCache.CreateCopy(DetailCache.Insert(new FSApptLineSplit()));

                lotSerialSplit.LotSerialNbr = apptLine.LotSerialNbr;

                if (lotSerTrackExpiration == true)
                    lotSerialSplit.ExpireDate = ExpireDateByLot(apptLineCache, lotSerialSplit, apptLine);
            }

            if (qtyDefault > 0m)
            {
                lotSerialSplit.Qty = qtyDefault;
                lotSerialSplit.BaseQty = INUnitAttribute.ConvertToBase(DetailCache, lotSerialSplit.InventoryID, lotSerialSplit.UOM, lotSerialSplit.Qty ?? 0m, INPrecision.QUANTITY);
            }
            else
            {
                lotSerialSplit.Qty = 0m;
                lotSerialSplit.BaseQty = 0m;
            }

            lotSerialSplit = (FSApptLineSplit)DetailCache.Update(lotSerialSplit);
        }

        protected virtual void GetLotSerialQtyDefault(PXCache sender, string lotSerialNbr, FSAppointmentDet apptLine, string lotSerTrack, out decimal? qtyDefault)
        {
            decimal lotSerialAvailQty;
            decimal lotSerialUsedQty;
            bool foundServiceOrderAllocation;

            GetLotSerialAvailability(sender.Graph,
                                     apptLine,
                                     lotSerialNbr,
                                     true,
                                     out lotSerialAvailQty,
                                     out lotSerialUsedQty,
                                     out foundServiceOrderAllocation);

            qtyDefault = lotSerialAvailQty - lotSerialUsedQty;
        }

        protected virtual void InsertLotSerialsFromServiceOrder(PXCache sender, FSAppointmentDet apptLine)
        {
            if (apptLine.SODetID == null || apptLine.SODetID < 0)
            {
                return;
            }

            var srvOrdType = (FSSrvOrdType)sender.Graph.Caches[typeof(FSSrvOrdType)].Current;

            if (srvOrdType == null || srvOrdType.SetLotSerialNbrInAppts == false)
            {
                return;
            }

            decimal baseExistingSplitTotalQty;
            List<FSApptLineSplit> existingSplits = GetExistingSplits(apptLine, out baseExistingSplitTotalQty);
            decimal basePendingQty = (decimal)apptLine.BaseQty - baseExistingSplitTotalQty;

            if (basePendingQty >= 0m)
            {
                FSSODet soDetRow = PXSelect<FSSODet, Where<FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>.Select(_Graph, apptLine.SODetID);
                if (soDetRow == null)
                {
                    throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                }

                List<FSSODetSplit> soSplitsWithLotSerial = PXSelect<FSSODetSplit,
                        Where<
                            FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                            And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                            And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>,
                            And<FSSODetSplit.pOCreate, Equal<False>>>>>,
                        OrderBy<Asc<FSSODetSplit.splitLineNbr>>>
                    .Select(_Graph, soDetRow.SrvOrdType, soDetRow.RefNbr, soDetRow.LineNbr)
                    .RowCast<FSSODetSplit>()
                    .Where(row => string.IsNullOrEmpty(row.LotSerialNbr) == false)
                    .ToList();

                foreach (FSSODetSplit soSplit in soSplitsWithLotSerial)
                {
                    decimal baseSOSplitBalance = (decimal)soSplit.BaseQty;

                    var otherApptLines = AppointmentEntry.GetRelatedApptLines(_Graph, apptLine.SODetID,
                            excludeSpecificApptLine: true, apptDetID: apptLine.AppDetID, onlyMarkForPOLines: false, sortResult: true);

                    foreach (FSAppointmentDet otherApptLine in otherApptLines)
                    {
                        FSApptLineSplit otherApptSplitUsingSrvOrdSplit = PXSelect<FSApptLineSplit,
                                Where<
                                    FSApptLineSplit.srvOrdType, Equal<Required<FSApptLineSplit.srvOrdType>>,
                                    And<FSApptLineSplit.apptNbr, Equal<Required<FSApptLineSplit.apptNbr>>,
                                    And<FSApptLineSplit.lineNbr, Equal<Required<FSApptLineSplit.lineNbr>>,
                                    And<FSApptLineSplit.lotSerialNbr, Equal<Required<FSApptLineSplit.lotSerialNbr>>>>>>>
                            .Select(_Graph, otherApptLine.SrvOrdType, otherApptLine.RefNbr, otherApptLine.LineNbr, soSplit.LotSerialNbr);

                        if (otherApptSplitUsingSrvOrdSplit != null)
                        {
                            baseSOSplitBalance -= (decimal)otherApptSplitUsingSrvOrdSplit.BaseQty;
                        }
                    }

                    FSApptLineSplit apptSplit = existingSplits.Find(x => x.LotSerialNbr == soSplit.LotSerialNbr);
                    if (apptSplit != null)
                    {
                        baseSOSplitBalance -= (decimal)apptSplit.BaseQty;
                    }

                    if (baseSOSplitBalance > 0m)
                    {
                        if (apptSplit == null)
                        {
                            apptSplit = Convert(apptLine);
                            apptSplit.BaseQty = 0;
                            apptSplit.LotSerialNbr = soSplit.LotSerialNbr;
                        }

                        if (baseSOSplitBalance > basePendingQty)
                        {
                            baseSOSplitBalance = basePendingQty;
                        }

                        apptSplit.BaseQty += baseSOSplitBalance;
                        apptSplit.Qty = INUnitAttribute.ConvertFromBase(DetailCache, apptSplit.InventoryID, apptSplit.UOM, (decimal)apptSplit.BaseQty, INPrecision.QUANTITY);

                        basePendingQty -= baseSOSplitBalance;

                        DetailCache.Update(apptSplit);
                    }

                    if (basePendingQty <= 0m)
                    {
                        break;
                    }
                }
            }
        }

        public virtual void _TruncateNumbers(PXCache sender, FSAppointmentDet Row, decimal baseQty)
        {
            decimal baseExistingSplitTotalQty;
            GetExistingSplits(Row, out baseExistingSplitTotalQty);

            if (baseExistingSplitTotalQty > baseQty)
            {
                TruncateNumbers(sender, Row, baseExistingSplitTotalQty - baseQty);
            }
        }

        public virtual void _UpdateParent(PXCache sender, FSAppointmentDet row)
        {
            UpdateParent(sender, row);

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);
            string LotSerTrack = ((INLotSerClass)item).LotSerTrack;

            if (LotSerTrack == INLotSerTrack.SerialNumbered && row.BaseEffTranQty > 1m)
            {
                row.LotSerialNbr = null;
            }
            else if (LotSerTrack == INLotSerTrack.LotNumbered)
            {
                decimal baseExistingSplitTotalQty;
                List<FSApptLineSplit> existingSplits = GetExistingSplits(row, out baseExistingSplitTotalQty);

                if (existingSplits.Count > 1)
                {
                    row.LotSerialNbr = null;
                }
            }
        }

        protected void Detail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            if (e.ExternalCall && IsLotSerialRequired == false)
            {
                throw new PXException(TX.Error.CannotEditSplitBecauseItIsReservedForReceiptAllocationInfo);
            }
        }

        protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            FSApptLineSplit row = (FSApptLineSplit)e.Row;

            if (!sender.ObjectsEqual<FSApptLineSplit.lotSerialNbr>(e.Row, e.OldRow) && ((FSApptLineSplit)e.Row).LotSerialNbr != null && ((FSApptLineSplit)e.Row).Operation == SO.SOOperation.Issue)
            {
                LotSerialNbr_Updated(sender, e);
            }

            if (!sender.ObjectsEqual<FSApptLineSplit.locationID>(e.Row, e.OldRow) && ((FSApptLineSplit)e.Row).LotSerialNbr != null && e.ExternalCall)
            {
                Location_Updated(sender, e);
            }

            base.Detail_RowUpdated(sender, e);
            MarkParentAsUpdated();
        }

        public virtual bool VerifyLotSerialTotalQty(PXCache sender, FSApptLineSplit split, decimal newIncrease)
        {
            if (newIncrease < 0m)
            {
                return true;
            }

            FSAppointmentDet apptLine = (FSAppointmentDet)PXParentAttribute.SelectParent(sender, split, typeof(FSAppointmentDet));

            decimal baseExistingSplitTotalQty;
            decimal existingSplitTotalQty;
            GetExistingSplits(apptLine, out baseExistingSplitTotalQty, out existingSplitTotalQty);

            decimal newSplitTotalQty = existingSplitTotalQty + newIncrease;

            if (newSplitTotalQty > apptLine.EffTranQty)
            {
                throw new PXSetPropertyException(TX.Error.TotalLotSerialQtyXExceedsTheQtyRequiredX, PXErrorLevel.Error, newSplitTotalQty.ToString("0"), ((decimal)apptLine.EffTranQty).ToString("0"));
            }

            return true;
        }

        protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            if (e.ExternalCall && IsLotSerialRequired == false)
            {
                throw new PXException(TX.Error.CannotEditSplitBecauseItIsReservedForReceiptAllocationInfo);
            }

            FSApptLineSplit row = (FSApptLineSplit)e.Row;

            PXResult<InventoryItem, INLotSerClass> res = ReadInventoryItem(sender, row.InventoryID);
            InventoryItem item = (InventoryItem)res;
            bool NonStockKit = item.KitItem == true && item.StkItem == false;

            if (NonStockKit)
            {
                row.InventoryID = null;
            }

            base.Detail_RowInserting(sender, e);
        }

        protected override void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            base.Detail_RowInserted(sender, e);
            MarkParentAsUpdated();
        }

        protected virtual void Detail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            if (e.ExternalCall && IsLotSerialRequired == false)
            {
                throw new PXException(TX.Error.CannotEditSplitBecauseItIsReservedForReceiptAllocationInfo);
            }
        }

        protected override void Detail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            base.Detail_RowDeleted(sender, e);
            MarkParentAsUpdated();
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
        }

        protected int _detailsRequested = 0;

        protected override IQtyAllocated AvailabilityFetch<TNode>(ILSDetail Row, IQtyAllocated allocated, IStatus status, AvailabilityFetchMode fetchMode)
        {
            return allocated;
        }

        protected virtual bool LotSerialNbr_Updated(PXCache sender, EventArgs e)
        {
            FSApptLineSplit split = (FSApptLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);
            INSiteLotSerial siteLotSerial = PXSelect<INSiteLotSerial,
                Where<INSiteLotSerial.inventoryID, Equal<Required<INSiteLotSerial.inventoryID>>,
                And<INSiteLotSerial.siteID, Equal<Required<INSiteLotSerial.siteID>>,
                And<INSiteLotSerial.lotSerialNbr, Equal<Required<INSiteLotSerial.lotSerialNbr>>>>>>.Select(sender.Graph, split.InventoryID, split.SiteID, split.LotSerialNbr);

            if (INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null && siteLotSerial != null && siteLotSerial.LotSerAssign != INLotSerAssign.WhenUsed)
            {
                if (split.BaseQty <= 0m)
                {
                    split.BaseQty = 1;
                    sender.SetValueExt<FSApptLineSplit.qty>(split, INUnitAttribute.ConvertFromBase(DetailCache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY));
                }
            }
            return true;
        }

        protected virtual void Location_Updated(PXCache sender, EventArgs e)
        {
            FSApptLineSplit split = (FSApptLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);

            FSAppointmentDet line = SelectMaster(sender, split);
            if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null)
            {
                INLotSerialStatus res = PXSelect<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
                 And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
                 And<INLotSerialStatus.siteID, Equal<Required<INLotSerialStatus.siteID>>,
                 And<INLotSerialStatus.lotSerialNbr, Equal<Required<INLotSerialStatus.lotSerialNbr>>,
                 And<INLotSerialStatus.locationID, Equal<Required<INLotSerialStatus.locationID>>>>>>>>.Select(sender.Graph, split.InventoryID, split.SubItemID, split.SiteID, split.LotSerialNbr, split.LocationID);
                if (res == null)
                {
                    split.LotSerialNbr = null;
                }
            }
        }

        public override FSApptLineSplit Convert(FSAppointmentDet item)
        {
            return StaticConvert(item);
        }

        public static FSApptLineSplit StaticConvert(FSAppointmentDet item)
        {
            using (InvtMultScope<FSAppointmentDet> ms = new InvtMultScope<FSAppointmentDet>(item))
            {
                FSApptLineSplit ret = item;
                //baseqty will be overriden in all cases but AvailabilityFetch
                ret.BaseQty = item.BaseQty - item.UnassignedQty;
                ret.LotSerialNbr = string.Empty;
                ret.SplitLineNbr = null;
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
        public virtual void FSAppointmentDet_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && AdvancedAvailCheck(sender, e.Row) &&
                ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                if (((FSAppointmentDet)e.Row).BaseQty != 0m)
                {
                    AvailabilityCheck(sender, (FSAppointmentDet)e.Row);
                }
            }
        }

        public virtual void FSApptLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            FSApptLineSplit apptLineSplit = (FSApptLineSplit)e.Row;
            if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                bool RequireLocationAndSubItem = ((FSApptLineSplit)e.Row).IsStockItem == true && ((FSApptLineSplit)e.Row).BaseQty != 0m;

                PXDefaultAttribute.SetPersistingCheck<FSApptLineSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSApptLineSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

                if (AdvancedAvailCheck(sender, e.Row) && ((FSApptLineSplit)e.Row).BaseQty != 0m)
                {
                    AvailabilityCheck(sender, (FSApptLineSplit)e.Row);
                }

                bool lotSerialRequired = apptLineSplit.POReceiptNbr == null;

                PXDefaultAttribute.SetPersistingCheck<FSApptLineSplit.lotSerialNbr>(sender, e.Row, lotSerialRequired == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

                if (apptLineSplit.Qty == 0m)
                {
                    PXSetPropertyException exception = new PXSetPropertyException(TX.Error.QtyMustBeDifferentFromZeroOnAllLotSerialLines, PXErrorLevel.Error);
                    sender.RaiseExceptionHandling<FSApptLineSplit.lotSerialNbr>(apptLineSplit, null, exception);
                }
            }
        }

        public virtual void FSApptLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSAppointmentDet)];
            if (cache.Current != null && (e.Row == null || ((FSAppointmentDet)cache.Current).LineNbr == ((FSApptLineSplit)e.Row).LineNbr && ((FSApptLineSplit)e.Row).IsStockItem == true))
            {
                e.NewValue = ((FSAppointmentDet)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public virtual void FSApptLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSAppointmentDet)];
            if (cache.Current != null && (e.Row == null || ((FSAppointmentDet)cache.Current).LineNbr == ((FSApptLineSplit)e.Row).LineNbr && ((FSApptLineSplit)e.Row).IsStockItem == true))
            {
                e.NewValue = ((FSAppointmentDet)cache.Current).LocationID;
                e.Cancel = (_InternallCall == true || e.NewValue != null);
            }
        }

        public virtual void FSApptLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSAppointmentDet)];
            if (cache.Current != null && (e.Row == null || ((FSAppointmentDet)cache.Current).LineNbr == ((FSApptLineSplit)e.Row).LineNbr))
            {
                using (InvtMultScope<FSAppointmentDet> ms = new InvtMultScope<FSAppointmentDet>((FSAppointmentDet)cache.Current))
                {
                    e.NewValue = ((FSAppointmentDet)cache.Current).InvtMult;
                    e.Cancel = true;
                }
            }
        }

        private void FSApptLineSplit_lotSerialNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var apptSplit = (FSApptLineSplit)e.Row;

            apptSplit.OrigLineNbr = null;
            apptSplit.OrigSplitLineNbr = null;
            apptSplit.OrigSplitLineNbr = null;

            var apptLine = (FSAppointmentDet)MasterCache.Current;
            if (apptLine == null || apptLine.SODetID == null || apptLine.SODetID < 0)
            {
                return;
            }

            FSSODet soLine = PXSelect<FSSODet, Where<FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>.Select(sender.Graph, apptLine.SODetID);
            if (soLine != null)
            {
                apptSplit.OrigLineNbr = soLine.LineNbr;

                if (string.IsNullOrEmpty(apptSplit.LotSerialNbr) == false)
                {
                    FSSODetSplit soSplit = PXSelect<FSSODetSplit,
                                            Where<FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>,
                                                And<FSSODetSplit.lotSerialNbr, Equal<Required<FSSODetSplit.lotSerialNbr>>>>>>>
                                        .Select(sender.Graph, soLine.SrvOrdType, soLine.RefNbr, soLine.LineNbr, apptSplit.LotSerialNbr);

                    if (soSplit != null)
                    {
                        apptSplit.OrigSplitLineNbr = soSplit.SplitLineNbr;
                    }
                }
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
        {
            PXErrorLevel level = AdvancedAvailCheck(sender, row) ? PXErrorLevel.Error : PXErrorLevel.Warning;
            if (row is FSAppointmentDet)
            {
                sender.RaiseExceptionHandling<FSAppointmentDet.effTranQty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, level, sender.GetStateExt<FSAppointmentDet.inventoryID>(row), sender.GetStateExt<FSAppointmentDet.subItemID>(row), sender.GetStateExt<FSAppointmentDet.siteID>(row), sender.GetStateExt<FSAppointmentDet.locationID>(row), sender.GetValue<FSAppointmentDet.lotSerialNbr>(row)));
            }
            else
            {
                sender.RaiseExceptionHandling<FSApptLineSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, level, sender.GetStateExt<FSApptLineSplit.inventoryID>(row), sender.GetStateExt<FSApptLineSplit.subItemID>(row), sender.GetStateExt<FSApptLineSplit.siteID>(row), sender.GetStateExt<FSApptLineSplit.locationID>(row), sender.GetValue<FSApptLineSplit.lotSerialNbr>(row)));
            }
        }

        protected bool AdvancedAvailCheck(PXCache sender, object row)
        {
            return false;
        }

        public void OverrideAdvancedAvailCheck(bool checkRequired)
        {
            _advancedAvailCheck = checkRequired;
        }
        private bool? _advancedAvailCheck;
        #endregion

        protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatus> cmd, FSAppointmentDet Row, INLotSerClass lotSerClass)
        {
            if (Row.SubItemID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
            }
            if (Row.LocationID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
            }
            else
            {
                switch (Row.TranType)
                {
                    case INTranType.Transfer:
                        cmd.WhereAnd<Where<INLocation.transfersValid, Equal<boolTrue>>>();
                        break;
                    default:
                        cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
                        break;
                }
            }

            if (lotSerClass.IsManualAssignRequired == true)
            {
                if (string.IsNullOrEmpty(Row.LotSerialNbr))
                {
                    cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
                }
                else
                    cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
            }
        }

        public override void AvailabilityCheck(PXCache sender, ILSMaster Row, IQtyAllocated availability)
        {
        }
        private void RaiseQtyRowExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
        {
            PXErrorLevel level = AdvancedAvailCheck(sender, row) ? PXErrorLevel.Error : PXErrorLevel.Warning;
            if (row is FSAppointmentDet)
            {
                sender.RaiseExceptionHandling<FSAppointmentDet.effTranQty>(row, newValue,
                    e == null ? e : new PXSetPropertyException(e.Message, level, sender.GetStateExt<FSAppointmentDet.inventoryID>(row), sender.GetStateExt<FSAppointmentDet.subItemID>(row), sender.GetStateExt<FSAppointmentDet.siteID>(row), sender.GetStateExt<FSAppointmentDet.locationID>(row), sender.GetValue<FSAppointmentDet.lotSerialNbr>(row)));
            }
            else
            {
                sender.RaiseExceptionHandling<FSApptLineSplit.qty>(row, newValue,
                    e == null ? e : new PXSetPropertyException(e.Message, level, sender.GetStateExt<FSApptLineSplit.inventoryID>(row), sender.GetStateExt<FSApptLineSplit.subItemID>(row), sender.GetStateExt<FSApptLineSplit.siteID>(row), sender.GetStateExt<INTranSplit.locationID>(row), sender.GetValue<FSApptLineSplit.lotSerialNbr>(row)));
            }
        }

        /// <summary>
        /// Inserts FSAppointmentDet into cache without adding the splits.
        /// The Splits have to be added manually.
        /// </summary>
        /// <param name="line">Master record.</param>
        public virtual FSAppointmentDet InsertMasterWithoutSplits(FSAppointmentDet line)
        {
            _InternallCall = true;
            try
            {
                var row = (FSAppointmentDet)MasterCache.Insert(line);
                DetailCounters.Remove(row);
                return row;
            }
            finally
            {
                _InternallCall = false;
            }
        }

        protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
        {
            return INLotSerTrack.Mode.Manual;
        }

        //public override void CreateNumbers(PXCache sender, FSAppointmentDet Row, decimal BaseQty, bool ForceAutoNextNbr)
        //{
        //}

        protected virtual List<FSApptLineSplit> GetExistingSplits(FSAppointmentDet parentRow, out decimal baseExistingSplitTotalQty)
        {
            decimal existingSplitTotalQty;
            return GetExistingSplits(parentRow, out baseExistingSplitTotalQty, out existingSplitTotalQty);
        }

        protected virtual List<FSApptLineSplit> GetExistingSplits(FSAppointmentDet parentRow, out decimal baseExistingSplitTotalQty, out decimal existingSplitTotalQty)
        {
            baseExistingSplitTotalQty = 0m;
            existingSplitTotalQty = 0m;
            var existingSplits = new List<FSApptLineSplit>();

            foreach (FSApptLineSplit existingSplit in PXParentAttribute.SelectChildren(DetailCache, parentRow, typeof(FSAppointmentDet)))
            {
                baseExistingSplitTotalQty += (decimal)existingSplit.BaseQty;
                existingSplitTotalQty += (decimal)existingSplit.Qty;

                existingSplits.Add(existingSplit);
            }

            return existingSplits;
        }

        protected virtual FSINLotSerialNbrAttribute GetLotSerialSelector(PXCache cache)
        {
            if (_LotSerialSelector != null)
            {
                return _LotSerialSelector;
            }

            foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<FSApptLineSplit.lotSerialNbr>())
            {
                if (attr is FSINLotSerialNbrAttribute)
                {
                    _LotSerialSelector = (FSINLotSerialNbrAttribute)attr;
                    return _LotSerialSelector;
                }
            }

            return null;
        }

        public override void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (VerifySrvOrdQtyMethod != null)
            {
                VerifySrvOrdQtyMethod(sender, (FSAppointmentDet)e.Row, e.NewValue, MasterQtyField, true);
            }

            base.Master_Qty_FieldVerifying(sender, e);
        }

        public override void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            base.Detail_Qty_FieldVerifying(sender, e);

            if (e.NewValue == null || (e.NewValue is decimal) == false || (decimal)e.NewValue == 0m)
            {
                return;
            }

            var apptLineSplit = (FSApptLineSplit)e.Row;
            FSAppointmentDet apptLine = (FSAppointmentDet)MasterCache.Current;

            // Validates that the total quantity of the split does not exceed the quantity required by the master line.
            VerifyLotSerialTotalQty(sender, (FSApptLineSplit)e.Row, (decimal)e.NewValue - (decimal)apptLineSplit.Qty);

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, apptLineSplit.InventoryID);
            if (item == null)
                return;

            var lotSerClass = (INLotSerClass)item;

            // Validates the available quantity for the Lot/Serial number
            if (string.IsNullOrEmpty(apptLineSplit.LotSerialNbr) == false && lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered)
            {
                decimal lotSerialAvailQty;
                decimal lotSerialUsedQty;
                bool foundServiceOrderAllocation;

                GetLotSerialAvailability(sender.Graph,
                                         apptLine,
                                         apptLineSplit.LotSerialNbr,
                                         true,
                                         out lotSerialAvailQty,
                                         out lotSerialUsedQty,
                                         out foundServiceOrderAllocation);

                decimal remainingQty = lotSerialAvailQty - lotSerialUsedQty;

                if (remainingQty < (decimal)e.NewValue)
                {
                    if (lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
                    {
                        if (foundServiceOrderAllocation == false)
                        {
                            throw new PXSetPropertyException(TX.Error.LotSerialNotAvailable);
                        }
                        else
                        {
                            throw new PXSetPropertyException(TX.Error.LotSerialNbrOnOtherAppointment);
                        }
                    }
                    else
                    {
                        if (foundServiceOrderAllocation == false)
                        {
                            throw new PXSetPropertyException(TX.Error.QtyEnteredXForLotNumberGreaterThanINAvailQtyX, ((decimal)e.NewValue).ToString("0"), lotSerialAvailQty.ToString("0"));
                        }
                        else
                        {
                            if (lotSerialUsedQty == 0)
                            {
                                throw new PXSetPropertyException(TX.Error.QtyEnteredXForLotNumberGreaterThanServiceOrderAllocQtyX, ((decimal)e.NewValue).ToString("0"), lotSerialAvailQty.ToString("0"));
                            }
                            else
                            {
                                throw new PXSetPropertyException(TX.Error.QtyEnteredXForLotNumberPlusOtherApptsQtyXGreaterThanServiceOrderAllocQtyX, ((decimal)e.NewValue).ToString("0"), lotSerialUsedQty.ToString("0"), lotSerialAvailQty.ToString("0"));
                            }
                        }
                    }
                }
            }
        }

        protected override IEnumerable<PXExceptionInfo> GetAvailabilityCheckErrors(PXCache sender, ILSMaster Row, IQtyAllocated availability)
        {
            if (Row.InvtMult == -1 && Row.BaseQty > 0m && availability != null)
            {
                if (availability.QtyNotAvail < 0m && (availability.QtyAvail + availability.QtyNotAvail) < 0m)
                {
                    if (availability is LotSerialStatus)
                        yield return new PXExceptionInfo(PXErrorLevel.Warning, IN.Messages.StatusCheck_QtyLotSerialNegative);
                    else if (availability is LocationStatus)
                        yield return new PXExceptionInfo(PXErrorLevel.Warning, IN.Messages.StatusCheck_QtyLocationNegative);
                    else if (availability is SiteStatus)
                        yield return new PXExceptionInfo(PXErrorLevel.Warning, IN.Messages.StatusCheck_QtyNegative);
                }
            }
        }

        // TODO:
        [Obsolete("This method will be deleted in the next major release.")]
        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, int? soDetID, int? apptDetID, string lotSerialNbr, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
            => FSApptLotSerialNbrAttribute.GetLotSerialAvailabilityInt(graphToQuery, apptLine, soDetID, apptDetID, lotSerialNbr, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, string lotSerialNbr, bool ignoreUseByApptLine, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        => FSApptLotSerialNbrAttribute.GetLotSerialAvailabilityInt(graphToQuery, apptLine, lotSerialNbr, ignoreUseByApptLine, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

        public virtual void MarkParentAsUpdated()
        {
            if (MasterCache.Current == null)
            {
                return;
            }

            if (MasterCache.GetStatus(MasterCache.Current) == PXEntryStatus.Notchanged)
            {
                MasterCache.SetStatus(MasterCache.Current, PXEntryStatus.Updated);
            }

            FSAppointment header = (FSAppointment)_Graph.Caches[typeof(FSAppointment)].Current;
            if (header != null)
            {
                header.MustUpdateServiceOrder = true;
            }
        }
    }

    #region FSApptLotSerialNbrAttribute

    public class FSApptLotSerialNbrAttribute : SOShipLotSerialNbrAttribute
    {
        public FSApptLotSerialNbrAttribute(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType)
            : base(SiteID, InventoryType, SubItemType, LocationType)
        {
            CreateCustomSelector(SiteID, InventoryType, SubItemType, LocationType);
        }

        public FSApptLotSerialNbrAttribute(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType)
            : base(SiteID, InventoryType, SubItemType, LocationType, ParentLotSerialNbrType)
        {
            CreateCustomSelector(SiteID, InventoryType, SubItemType, LocationType);
        }

        protected virtual void CreateCustomSelector(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType)
        {
            var selector = (PXSelectorAttribute)_Attributes[_SelAttrIndex];

            var customSelector = new FSINLotSerialNbrAttribute(SiteID, InventoryType, SubItemType, LocationType, SrvOrdLineID: null);

            _Attributes[_SelAttrIndex] = customSelector;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldUpdated.AddHandler<FSApptLineSplit.lotSerialNbr>(LotSerialNumberUpdated);
            sender.Graph.FieldVerifying.AddHandler<FSApptLineSplit.lotSerialNbr>(LotSerialNumberFieldVerifying);
        }

        protected override void LotSerialNumberUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
        }

        protected void LotSerialNumberFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            decimal lotSerialAvailQty;
            decimal lotSerialUsedQty;
            bool foundServiceOrderAllocation;

            FSApptLineSplit apptLineSplit = (FSApptLineSplit)e.Row;
            FSAppointmentDet apptLine = (FSAppointmentDet)PXParentAttribute.SelectParent(sender, apptLineSplit, typeof(FSAppointmentDet));

            GetLotSerialAvailability(sender.Graph,
                                     apptLine,
                                     (string)e.NewValue,
                                     true,
                                     out lotSerialAvailQty,
                                     out lotSerialUsedQty,
                                     out foundServiceOrderAllocation);

            decimal remainingQty = lotSerialAvailQty - lotSerialUsedQty;

            if (remainingQty < 1m)
            {
                if (foundServiceOrderAllocation == false)
                {
                    throw new PXSetPropertyException(TX.Error.LotSerialNotAvailable);
                }
                else
                {
                    throw new PXSetPropertyException(TX.Error.LotSerialNbrOnOtherAppointment);
                }
            }
        }

        public override void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.RowSelected(sender, e);
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
        }

        // TODO:
        [Obsolete("This method will be deleted in the next major release.")]
        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, int? soDetID, int? apptDetID, string lotSerialNbr, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        => GetLotSerialAvailabilityInt(graphToQuery, apptLine, soDetID, apptDetID, lotSerialNbr, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, string lotSerialNbr, bool ignoreUseByApptLine, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        => GetLotSerialAvailabilityInt(graphToQuery, apptLine, lotSerialNbr, ignoreUseByApptLine, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

        // TODO:
        [Obsolete("This method will be deleted in the next major release.")]
        public static void GetLotSerialAvailabilityInt(PXGraph graphToQuery, FSAppointmentDet apptLine, int? soDetID, int? apptDetID, string lotSerialNbr, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        {
            GetLotSerialAvailabilityInt(graphToQuery, apptLine, lotSerialNbr, true, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);
        }

        public static void GetLotSerialAvailabilityInt(PXGraph graphToQuery, FSAppointmentDet apptLine, string lotSerialNbr, bool ignoreUseByApptLine, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        {
            lotSerialAvailQty = 0m;
            lotSerialUsedQty = 0m;
            foundServiceOrderAllocation = false;

            if (string.IsNullOrEmpty(lotSerialNbr) == true)
            {
                return;
            }

            bool searchINAvailQty = true;
            FSSODet fsSODetRow = null;
            FSSODetSplit soDetSplit = null;

            if (apptLine.SODetID != null && apptLine.SODetID > 0)
            {
                fsSODetRow = PXSelect<FSSODet,
                                Where<
                                    FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                                .Select(graphToQuery, apptLine.SODetID);

                if (fsSODetRow != null)
                {
                    soDetSplit = PXSelect<FSSODetSplit,
                                    Where<
                                        FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                        And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                        And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>,
                                        And<FSSODetSplit.lotSerialNbr, Equal<Required<FSSODetSplit.lotSerialNbr>>>>>>>
                                    .Select(graphToQuery, fsSODetRow.SrvOrdType, fsSODetRow.RefNbr, fsSODetRow.LineNbr, lotSerialNbr);

                    if (soDetSplit != null)
                    {
                        searchINAvailQty = false;
                    }
                }
            }

            if (searchINAvailQty == true)
            {
                INLotSerialStatus lotSerialStatus = IN.INLotSerialStatus.PK.Find(graphToQuery, apptLine.InventoryID, apptLine.SubItemID, apptLine.SiteID, apptLine.LocationID, lotSerialNbr);

                if (lotSerialStatus != null)
                {
                    lotSerialAvailQty = (decimal)lotSerialStatus.QtyAvail;
                }
            }
            else
            {
                lotSerialAvailQty = (decimal)soDetSplit.Qty;

                BqlCommand bqlCommand = new Select4<FSApptLineSplit,
                                                     Where<
                                                         FSApptLineSplit.origSrvOrdType, Equal<Required<FSApptLineSplit.origSrvOrdType>>,
                                                         And<FSApptLineSplit.origSrvOrdNbr, Equal<Required<FSApptLineSplit.origSrvOrdNbr>>,
                                                         And<FSApptLineSplit.origLineNbr, Equal<Required<FSApptLineSplit.origLineNbr>>,
                                                         And<FSApptLineSplit.lotSerialNbr, Equal<Required<FSApptLineSplit.lotSerialNbr>>>>>>,
                                                     Aggregate<
                                                         Sum<FSApptLineSplit.qty>>>();

                List<object> parameters = new List<object>();
                parameters.Add(soDetSplit.SrvOrdType);
                parameters.Add(soDetSplit.RefNbr);
                parameters.Add(soDetSplit.LineNbr);
                parameters.Add(soDetSplit.LotSerialNbr);

                if (ignoreUseByApptLine == true)
                {
                    bqlCommand = bqlCommand.WhereAnd(typeof(Where<
                                                                 FSApptLineSplit.srvOrdType, NotEqual<Required<FSApptLineSplit.srvOrdType>>,
                                                                 Or<FSApptLineSplit.apptNbr, NotEqual<Required<FSApptLineSplit.apptNbr>>,
                                                                 Or<FSApptLineSplit.lineNbr, NotEqual<Required<FSApptLineSplit.lineNbr>>>>>));
                    parameters.Add(apptLine.SrvOrdType);
                    parameters.Add(apptLine.RefNbr);
                    parameters.Add(apptLine.LineNbr);
                }

                FSApptLineSplit otherSplitsSum = (FSApptLineSplit)new PXView(graphToQuery, false, bqlCommand).SelectSingle(parameters.ToArray());

                decimal? usedQty = otherSplitsSum != null ? otherSplitsSum.Qty : 0m;
                lotSerialUsedQty = usedQty != null ? (decimal)usedQty : 0m;
                foundServiceOrderAllocation = true;
            }
        }
    }
    #endregion
}
