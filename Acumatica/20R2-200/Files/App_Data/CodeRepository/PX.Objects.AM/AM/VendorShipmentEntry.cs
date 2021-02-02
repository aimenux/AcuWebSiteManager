using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.CR;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Vendor Shipments (AM310000)
    /// </summary>
    [PXCacheName(Messages.VendorShipments)]
    public class VendorShipmentEntry : PXGraph<VendorShipmentEntry, AMVendorShipment>
    {
        public PXSelect<AMVendorShipment> Document;
        public PXSelect<AMVendorShipment, Where<AMVendorShipment.shipmentNbr, Equal<Current<AMVendorShipment.shipmentNbr>>>> CurrentDocument;
        public PXSelect<AMVendorShipLine,
            Where<AMVendorShipLine.shipmentNbr, Equal<Current<AMVendorShipment.shipmentNbr>>>> Transactions;
        public PXSelect<AMVendorShipLineSplit, Where<AMVendorShipLineSplit.shipmentNbr, Equal<Current<AMVendorShipLine.shipmentNbr>>,
            And<AMVendorShipLineSplit.lineNbr, Equal<Current<AMVendorShipLine.lineNbr>>>>> Splits;
        public PXSelect<AMVendorShipmentContact, Where<AMVendorShipmentContact.contactID, Equal<Current<AMVendorShipment.shipContactID>>>> ShippingContact;
        public PXSelect<AMVendorShipmentAddress, Where<AMVendorShipmentAddress.addressID, Equal<Current<AMVendorShipment.shipAddressID>>>> ShippingAddress;
        public PXSetup<AMPSetup> Setup;
        [PXHidden]
        public PXSelect<AMProdOper> ProdOperRecs;
        public PXSetup<SOSetup> sosetup;
        public LSAMShipLine lsselect;
        
        public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);

            this.Caches.AddCacheMapping(typeof(INLotSerialStatus), typeof(INLotSerialStatus));
        }

        public PXAction<AMVendorShipment> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable Action(PXAdapter adapter,
            [PXInt]
            [VendorShipmentEntryActions]
            int? actionID,
            [PXString]
            string ActionName
        )
        {
            if (!string.IsNullOrEmpty(ActionName))
            {
                PXAction action = this.Actions[ActionName];

                if (action != null)
                {
                    Save.Press();
                    List<object> result = new List<object>();
                    foreach (object data in action.Press(adapter))
                    {
                        result.Add(data);
                    }
                    return result;
                }
            }

            var list = new List<AMVendorShipment>();
            foreach (var order in adapter.Get<AMVendorShipment>())
            {
                list.Add(order);
            }

            switch (actionID)
            {
                case VendorShipmentEntryActionsAttribute.ConfirmShipment:
                    PXLongOperation.StartOperation(this, delegate ()
                    {
                        var docgraph = PXGraph.CreateInstance<VendorShipmentEntry>();
                        foreach (var shipment in list)
                        {
                            try
                            {
                                if (adapter.MassProcess)
                                {
                                    PXProcessing<AMVendorShipment>.SetCurrentItem(shipment);
                                }

                                docgraph.ConfirmShipment(shipment);
                            }
                            catch (Exception ex)
                            {
                                if (!adapter.MassProcess)
                                {
                                    throw;
                                }
                                PXProcessing<AMVendorShipment>.SetError(ex);
                            }
                        }
                    });

                    break;
            }

            return list;
        }

        public PXAction<AMVendorShipment> report;
        [PXUIField(DisplayName = Messages.Reports, MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
        protected virtual IEnumerable Report(PXAdapter adapter,
            [PXString(8, InputMask = "CC.CC.CC.CC")]
            string reportID
        )
        {
            List<AMVendorShipment> list = adapter.Get<AMVendorShipment>().ToList();
            if (string.IsNullOrWhiteSpace(reportID))
            {
                return list;
            }

            Save.Press();

            PXReportRequiredException ex = null;

            foreach (var shipment in list)
            {
                var parameters = GetReportParameters(shipment, reportID);
                ex = PXReportRequiredException.CombineReport(ex, reportID, parameters);
            }

            if (ex != null)
            {
                throw ex;
            }

            return list;
        }

        public PXAction<AMVendorShipment> hold;
        [PXUIField(DisplayName = "Hold")]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
        protected virtual IEnumerable Hold(PXAdapter adapter)
        {
            return adapter.Get();
        }

        protected virtual Dictionary<string, string> GetReportParameters(AMVendorShipment shipment, string reportId)
        {
            var parameters = new Dictionary<string, string>
            {
                [Reports.ReportHelper.GetDacFieldNameString<AMVendorShipment.shipmentNbr>()] = shipment.ShipmentNbr
            };
            return parameters;
        }


        protected virtual void AMVendorShipment_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (e.Row != null)
            {
                using (ReadOnlyScope rs = new ReadOnlyScope(ShippingContact.Cache))
                {
                    try
                    {
                        AMVendorShipmentAddressAttribute.DefaultRecord<AMVendorShipment.shipAddressID>(sender, e.Row);
                    }
                    catch (SharedRecordMissingException)
                    {
                        sender.RaiseExceptionHandling<AMVendorShipment.siteID>(e.Row, sender.GetValueExt<AMVendorShipment.siteID>(e.Row),
                            new PXSetPropertyException(PX.Objects.PO.Messages.ShippingAddressMayNotBeEmpty, PXErrorLevel.Error));
                    }
                    try
                    {
                        AMVendorShipmentContactAttribute.DefaultRecord<AMVendorShipment.shipContactID>(sender, e.Row);
                    }
                    catch (SharedRecordMissingException)
                    {
                        sender.RaiseExceptionHandling<AMVendorShipment.siteID>(e.Row, sender.GetValueExt<AMVendorShipment.siteID>(e.Row),
                            new PXSetPropertyException(PX.Objects.PO.Messages.ShippingContactMayNotBeEmpty, PXErrorLevel.Error));
                    }
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<AMVendorShipment, AMVendorShipment.status> e)
        {
            e.NewValue = Setup.Current?.HoldShipmentsOnEntry == true ? VendorShipmentStatus.Hold : VendorShipmentStatus.Open;
        }

        protected virtual void _(Events.FieldDefaulting<AMVendorShipment, AMVendorShipment.hold> e)
        {
            e.NewValue = Setup.Current?.HoldShipmentsOnEntry == true;
        }

        protected virtual void _(Events.FieldUpdated<AMVendorShipment, AMVendorShipment.hold> e)
        {
            e.Cache.SetValueExt<AMVendorShipment.status>(e.Row, e.Row.Hold == true ? VendorShipmentStatus.Hold : VendorShipmentStatus.Open);
        }

        protected virtual void _(Events.RowSelected<AMVendorShipment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetVisible<AMVendorShipment.controlQty>(e.Cache, e.Row, Setup?.Current?.ValidateShipmentTotalOnConfirm == true);

            var isReleased = e.Row.Released == true;

            if (isReleased)
            {
                PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
            }

            // key fields always enabled for navigation
            PXUIFieldAttribute.SetEnabled<AMVendorShipment.shipmentNbr>(e.Cache, null, true);

            Document.AllowDelete = !isReleased;

            // We should be able to configure via automation steps?
            CurrentDocument.AllowUpdate = !isReleased;

            Transactions.AllowInsert =
                Transactions.AllowUpdate =
                    Transactions.AllowDelete = !isReleased;

            Splits.AllowInsert =
                Splits.AllowUpdate =
                    Splits.AllowDelete = !isReleased;

            ShippingAddress.AllowInsert =
                ShippingAddress.AllowUpdate =
                    ShippingAddress.AllowDelete = !isReleased;

            ShippingContact.AllowInsert =
                ShippingContact.AllowUpdate =
                    ShippingContact.AllowDelete = !isReleased;
        }

        protected virtual void AMVendorShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (sosetup.Current.RequireShipmentTotal == false)
            {
                if (PXCurrencyAttribute.IsNullOrEmpty(((AMVendorShipment)e.Row).ShipmentQty) == false)
                {
                    sender.SetValueExt<AMVendorShipment.controlQty>(e.Row, ((AMVendorShipment)e.Row).ShipmentQty);
                }
                else
                {
                    sender.SetValueExt<AMVendorShipment.controlQty>(e.Row, 0m);
                }
            }

            if (((AMVendorShipment)e.Row).Hold == false)
            {
                if ((bool)sosetup.Current.RequireShipmentTotal)
                {
                    if (((AMVendorShipment)e.Row).ShipmentQty != ((AMVendorShipment)e.Row).ControlQty && ((AMVendorShipment)e.Row).ControlQty != 0m)
                    {
                        sender.RaiseExceptionHandling<AMVendorShipment.controlQty>(e.Row, ((AMVendorShipment)e.Row).ControlQty, new PXSetPropertyException(PX.Objects.SO.Messages.DocumentOutOfBalance));
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<AMVendorShipment.controlQty>(e.Row, ((AMVendorShipment)e.Row).ControlQty, null);
                    }
                }
            }
        }

        protected virtual void AMVendorShipment_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            AMVendorShipment row = (AMVendorShipment)e.Row;
            if (row != null)
            {
                try
                {
                    AMVendorShipmentAddressAttribute.DefaultRecord<AMVendorShipment.shipAddressID>(sender, e.Row);
                }
                catch (SharedRecordMissingException)
                {
                    sender.RaiseExceptionHandling<AMVendorShipment.siteID>(e.Row, sender.GetValueExt<AMVendorShipment.siteID>(e.Row),
                        new PXSetPropertyException(PX.Objects.PO.Messages.ShippingAddressMayNotBeEmpty, PXErrorLevel.Error));
                }
                try
                {
                    AMVendorShipmentAddressAttribute.DefaultRecord<AMVendorShipment.shipContactID>(sender, e.Row);
                }
                catch (SharedRecordMissingException)
                {
                    sender.RaiseExceptionHandling<AMVendorShipment.siteID>(e.Row, sender.GetValueExt<AMVendorShipment.siteID>(e.Row),
                        new PXSetPropertyException(PX.Objects.PO.Messages.ShippingContactMayNotBeEmpty, PXErrorLevel.Error));
                }

                Location loc = PXSelect<Location, Where<Location.locationID, Equal<Required<Location.locationID>>>>.Select(this, row.VendorLocationID);
                if (loc == null)
                    return;
                sender.SetValueExt<AMVendorShipment.siteID>(e.Row, loc.VSiteID);
                sender.SetValueExt<AMVendorShipment.shipVia>(e.Row, loc.VCarrierID);
                sender.SetValueExt<AMVendorShipment.fOBPoint>(e.Row, loc.VFOBPointID);
                sender.SetValueExt<AMVendorShipment.shipTermsID>(e.Row, loc.VShipTermsID);
            }
        }

        protected virtual void _(Events.RowPersisting<AMVendorShipLine> e)
        {
            if (e.Row == null || e.Row.Qty != 0)
            {
                return;
            }

            e.Cache.RaiseExceptionHandling<AMVendorShipLine.qty>(
                e.Row,
                e.Row.Qty,
                new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMVendorShipLine.qty>(e.Cache)), PXErrorLevel.Error));
        }

        protected virtual void _(Events.RowDeleting<AMVendorShipLine> e)
        {
            if (e.Row?.Released != true)
            {
                return;
            }

            e.Cancel = true;
            throw new PXException(Messages.ShipLineCannotDeleteIsReleased, e.Row.ShipmentNbr, e.Row.LineNbr);
        }
        
        protected virtual void AMVendorShipLine_LineType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMVendorShipLine)e.Row;
            var header = Document.Current;
            if (row == null|| header == null)
            {
                return;
            }
            if (row.LineType == AMShipLineType.WIP)
                sender.SetValueExt<AMVendorShipLine.tranType>(row, AMTranType.Receipt);
            else
                sender.SetValueExt<AMVendorShipLine.tranType>(row, (header.ShipmentType == AMShipType.Return) ? AMTranType.Return : AMTranType.Issue);

            ClearShipLine(sender, row);
        }

        protected virtual void ClearShipLine(PXCache sender, AMVendorShipLine row)
        {
            sender.SetValueExt<AMVendorShipLine.invtMult>(row, AMTranType.InvtMult(row?.TranType ?? AMTranType.Issue));
            sender.SetValueExt<AMVendorShipLine.prodOrdID>(row, null);
            sender.SetValueExt<AMVendorShipLine.inventoryID>(row, null);
            sender.SetValueExt<AMVendorShipLine.subItemID>(row, null);
            sender.SetValueExt<AMVendorShipLine.siteID>(row, null);
            sender.SetValueExt<AMVendorShipLine.locationID>(row, null);
            sender.SetValueExt<AMVendorShipLine.uOM>(row, null);
            sender.SetValueExt<AMVendorShipLine.lotSerialNbr>(row, null);
            sender.SetDefaultExt<AMVendorShipLine.orderType>(row);
            sender.SetDefaultExt<AMVendorShipLine.qty>(row);
            sender.SetValueExt<AMVendorShipLine.pOOrderNbr>(row, null);
            sender.SetValueExt<AMVendorShipLine.pOLineNbr>(row, null);
        }

        protected virtual void AMVendorShipLine_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            AMVendorShipLine shipLine = (AMVendorShipLine)e.Row;

            if (shipLine == null)
            {
                return;
            }

            e.NewValue = AMTranType.InvtMult(shipLine.TranType ?? AMTranType.Issue);
        }

        protected virtual void AMVendorShipLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            EnableVendorShipLineFields(sender, (AMVendorShipLine) e.Row);
        }

        protected virtual void EnableVendorShipLineFields(PXCache cache, AMVendorShipLine row)
        {
            if (row?.LineType == null)
            {
                return;
            }

            if (row.Released == true)
            {
                PXUIFieldAttribute.SetEnabled(cache, row, false);
                return;
            }

            var isMaterialLine = row.LineType == AMShipLineType.Material;

            PXUIFieldAttribute.SetEnabled<AMVendorShipLine.inventoryID>(cache, row, isMaterialLine);
            PXUIFieldAttribute.SetEnabled<AMVendorShipLine.subItemID>(cache, row, isMaterialLine);
            PXUIFieldAttribute.SetEnabled<AMVendorShipLine.uOM>(cache, row, isMaterialLine);
            PXUIFieldAttribute.SetEnabled<AMVendorShipLine.matlLineID>(cache, row, isMaterialLine);
        }

        protected virtual void AMVendorShipLine_ProdOrdID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMVendorShipLine)e.Row;
            if (row?.ProdOrdID == null || row.LineType != AMShipLineType.WIP)
            {
                return;
            }

            var proditem = (AMProdItem)PXSelectorAttribute.Select<AMVendorShipLine.prodOrdID>(sender, row);
            if (proditem == null)
            {
                return;
            }

            sender.SetValueExt<AMVendorShipLine.inventoryID>(row, proditem.InventoryID);
            sender.SetValueExt<AMVendorShipLine.subItemID>(row, proditem.SubItemID);
            sender.SetValueExt<AMVendorShipLine.siteID>(row, proditem.SiteID);
            sender.SetValueExt<AMVendorShipLine.locationID>(row, proditem.LocationID);
            sender.SetValueExt<AMVendorShipLine.uOM>(row, proditem.UOM);
        }

        protected virtual void _(Events.FieldUpdated<AMVendorShipLine, AMVendorShipLine.operationID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            var prodOper = (AMProdOper)PXSelectorAttribute.Select<AMMTran.operationID>(e.Cache, e.Row);
            e.Cache.SetValueExt<AMVendorShipLine.pOOrderNbr>(e.Row, prodOper?.POOrderNbr);
            e.Cache.SetValueExt<AMVendorShipLine.pOLineNbr>(e.Row, prodOper?.POLineNbr);

            if (e.Row.OperationID == null)
            {
                e.Cache.SetValueExt<AMVendorShipLine.matlLineID>(e.Row, null);
                return;
            }

            if (e.Row.LineType == AMShipLineType.Material && !string.IsNullOrWhiteSpace(e.Row.ProdOrdID) && e.Row.OperationID != null && e.Row.InventoryID != null)
            {
                var item = (AMProdItem)PXSelectorAttribute.Select<AMVendorShipLine.prodOrdID>(e.Cache, e.Row);
                if (item == null)
                {
                    e.Cache.SetValueExt<AMVendorShipLine.matlLineID>(e.Row, null);
                    return;
                }
                SetItemFields(e.Cache, e.Row, item);
                return;
            }

            if (IsImport || IsContractBasedAPI || prodOper == null)
            {
                return;
            }

            e.Cache.SetValueExt<AMVendorShipLine.qty>(e.Row, prodOper.QtyRemaining.GetValueOrDefault());
        }

        protected virtual void AMVendorShipLine_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var shipLine = (AMVendorShipLine)e.Row;
            if (shipLine == null)
            {
                return;
            }

            var itemChanged = shipLine.InventoryID != (int?) e.OldValue;
            if (itemChanged)
            {
                cache.SetDefaultExt<AMVendorShipLine.qty>(shipLine);
            }

            if (shipLine.LineType == AMShipLineType.Material && !string.IsNullOrWhiteSpace(shipLine.ProdOrdID) && shipLine.OperationID != null && shipLine.InventoryID != null)
            {
                var item = (AMProdItem)PXSelectorAttribute.Select<AMVendorShipLine.prodOrdID>(cache, shipLine);
                if (item != null)
                {
                    SetItemFields(cache, shipLine, item);
                }
            }
        }

        protected virtual void AMVendorShipLine_SubItemID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var shipLine = (AMVendorShipLine)e.Row;
            if (shipLine == null
                || shipLine.LineType != AMShipLineType.Material
                || !InventoryHelper.SubItemFeatureEnabled
                || string.IsNullOrWhiteSpace(shipLine.ProdOrdID)
                || shipLine.OperationID == null
                || shipLine.InventoryID == null
                || shipLine.SubItemID == null)
            {
                return;
            }

            var item = (AMProdItem)PXSelectorAttribute.Select<AMVendorShipLine.prodOrdID>(cache, shipLine);
            if (item != null)
            {
                SetItemFields(cache, shipLine, item);
            }
        }

        protected virtual void AMVendorShipLine_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMVendorShipLine)e.Row;
            if (row == null)
            {
                return;
            }

            if (row.TranType == INTranType.Receipt)
                e.NewValue = null;
        }

        protected virtual void AMVendorShipLineSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMVendorShipLineSplit)e.Row;
            if (row == null)
            {
                return;
            }
            if (row.Released == true)
            {
                PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
            }
        }

        protected virtual void AMVendorShipLineSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMVendorShipLineSplit)e.Row;
            if (row == null)
            {
                return;
            }

            if (row.TranType == INTranType.Receipt)
                e.NewValue = null;
        }

        protected virtual void ConfirmShipment(AMVendorShipment shipment)
        {
            this.Clear();

            if (string.IsNullOrWhiteSpace(shipment?.ShipmentNbr))
            {
                throw new PXArgumentException(nameof(shipment));
            }

            if (shipment.Released == true)
            {
                return;
            }

            Document.Current = Document.Search<AMVendorShipment.shipmentNbr>(shipment.ShipmentNbr);

            if (Setup?.Current?.ValidateShipmentTotalOnConfirm == true && Document.Current.ShipmentQty != Document.Current.ControlQty)
            {
                throw new PXException(PX.Objects.SO.Messages.MissingShipmentControlTotal);
            }

            if (Document.Current.ShipmentQty == 0 || Transactions.Select().Count == 0)
            {
                throw new PXException(PX.Objects.SO.Messages.UnableConfirmZeroShipment, Document.Current.ShipmentNbr);
            }

            var rm = PXGraph.CreateInstance<MaterialEntry>();
            rm.ampsetup.Current.HoldEntry = false;
            rm.ampsetup.Current.RequireControlTotal = false;
            var WIPTrans = new List<AMVendorShipLine>();

            //check for an existing batch that was not fully released
            AMBatch batch = PXSelect<AMBatch, Where<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>,
                        And<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>>>>.Select(this, AMDocType.VendorShipment, shipment.ShipmentNbr);

            //if batch existed and not released, delete the lines and run normally, otherwise set the shipment and lines to released
            if (batch != null && batch.Released == false)
                DeleteExistingLines(rm, batch);

            foreach (AMVendorShipLine line in PXSelect<
                AMVendorShipLine,
                Where<AMVendorShipLine.shipmentNbr, Equal<Required<AMVendorShipLine.shipmentNbr>>>>
                .Select(this, shipment.ShipmentNbr))
            {
                if (line.Released == true)
                {
                    continue;
                }

                if (line.LineType == AMShipLineType.Material)
                {
                    if (batch == null)
                    {
                        batch = rm.batch.Insert();
                        batch.OrigDocType = AMDocType.VendorShipment;
                        batch.OrigBatNbr = shipment.ShipmentNbr;
                        batch = rm.batch.Update(batch);
                    }

                    if (batch.Released == false)
                    {
                        CreateTransAndSplits(rm, line);
                    }
                    
                    line.Released = true;
                    Transactions.Update(line);
                    continue;
                }

                WIPTrans.Add(line);
            }

            SaveAndReleaseMaterial(rm, batch, shipment);
            Persist();
            ProcessWIPLines(WIPTrans);
            

            shipment.Released = true;
            shipment.Status = VendorShipmentStatus.Completed;

            Document.Update(shipment);

            Persist();
        }
        
        protected virtual void SaveAndReleaseMaterial(MaterialEntry rm, AMBatch batch, AMVendorShipment shipment)
        {
            if (!rm.transactions.Cache.Inserted.Any_())
            {
                return;
            }

            rm.Persist();
            batch = rm.batch.Current;

            if (batch?.BatNbr == null)
            {
                return;
            }

            try
            {
                AMDocumentRelease.ReleaseDoc(new List<AMBatch> { batch }, false);
                PXTrace.WriteInformation(Messages.ReleasedMaterialBatchForShipment, batch.BatNbr, shipment.ShipmentNbr);
            }
            catch (Exception exception)
            {
                PXTrace.WriteError(exception);

                throw new PXException(Messages.UnableToReleaseMaterialForShipment, batch.BatNbr, shipment.ShipmentNbr);
            }
        }

        protected virtual void ProcessWIPLines(List<AMVendorShipLine> lines)
        {
            foreach (var line in lines)
            {
                var operation = (AMProdOper)ProdOperRecs.Search<AMProdOper.orderType, AMProdOper.prodOrdID, AMProdOper.operationID>(line.OrderType, line.ProdOrdID, line.OperationID).FirstOrDefault();
                if (operation != null)
                {
                    operation.ShippedQuantity += (line.Qty * line.InvtMult);
                    operation = ProdOperRecs.Update(operation);
                }
                line.Released = true;
                Transactions.Update(line);
            }
        }

        protected virtual void CreateTransAndSplits(MaterialEntry rm, AMVendorShipLine line)
        {
            var newTran = CreateAMMTran(line);
            newTran = rm.transactions.Insert(newTran);

            //check to see if there is already a split for the newly created line
            foreach (AMMTranSplit tranSplit in rm.splits.Cache.Inserted)
            {
                if (tranSplit.BatNbr == newTran.BatNbr && tranSplit.LineNbr == newTran.LineNbr)
                    rm.splits.Delete(tranSplit);
            }
            PXResultset<AMVendorShipLineSplit> splits = PXSelect<AMVendorShipLineSplit,
                Where<AMVendorShipLineSplit.shipmentNbr, Equal<Required<AMVendorShipLineSplit.shipmentNbr>>,
                    And<AMVendorShipLineSplit.lineNbr, Equal<Required<AMVendorShipLineSplit.lineNbr>>>>>.Select(this, line.ShipmentNbr, line.LineNbr);
            foreach (AMVendorShipLineSplit split in splits)
            {
                AMMTranSplit newSplit = CreateAMMTranSplit(split);
                newSplit = rm.splits.Insert(newSplit);
                split.Released = true;
                Splits.Update(split);
            }
        }

        protected virtual void DeleteExistingLines(MaterialEntry rm, AMBatch batch)
        {
            var trans = PXSelect<AMMTran, Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>.Select(this,batch.DocType, batch.BatNbr);
            foreach(var tran in trans)
            {
                rm.transactions.Delete(tran);
                
            }
        }

        protected virtual AMMTranSplit CreateAMMTranSplit(AMVendorShipLineSplit split)
        {
            var newSplit = new AMMTranSplit()
            {
                TranDate = split.TranDate,
                InventoryID = split.InventoryID,
                SubItemID = split.SubItemID,
                SiteID = split.SiteID,
                LocationID = split.LocationID,
                UOM = split.UOM,
                Qty = split.Qty,
                OrigBatNbr = split.ShipmentNbr,
                OrigLineNbr = split.LineNbr,
                OrigSplitLineNbr = split.SplitLineNbr,
                LotSerialNbr = split.LotSerialNbr
            };
            return newSplit;
        }

        protected virtual AMMTran CreateAMMTran(AMVendorShipLine line)
        {
            var tran = new AMMTran()
            {
                OrderType = line.OrderType,
                ProdOrdID = line.ProdOrdID,
                OperationID = line.OperationID,
                InventoryID = line.InventoryID,
                SiteID = line.SiteID,
                LocationID = line.LocationID,
                UOM = line.UOM,
                Qty = line.TranType == AMTranType.Return ? line.Qty * -1 : 0,
                LotSerialNbr = line.LotSerialNbr,
                OrigDocType = AMDocType.VendorShipment,
                OrigBatNbr = line.ShipmentNbr,
                OrigLineNbr = line.LineNbr,
                TranDesc = line.TranDesc
            };
            return tran;
        }

        protected bool IsSetItemFields;

        protected virtual void SetItemFields(PXCache cache, AMVendorShipLine shipLine, AMProdItem amProdItem)
        {
            if (IsSetItemFields)
            {
                //Prevent recursive
                return;
            }

            try
            {
                IsSetItemFields = true;

                var amprodmatl = GetRelatedProdMatl(shipLine, amProdItem.BaseQtytoProd.GetValueOrDefault());
                if (amprodmatl == null)
                {
                    cache.SetValueExt<AMVendorShipLine.matlLineID>(shipLine, null);
                    cache.SetDefaultExt<AMVendorShipLine.subItemID>(shipLine);
                    cache.SetDefaultExt<AMVendorShipLine.qty>(shipLine);
                    return;
                }

                //must set these fields before qty to make sure invtmult/trantypes/qty checks/etc. get set/query correctly
                cache.SetValueExt<AMVendorShipLine.matlLineID>(shipLine, amprodmatl.LineID);
                cache.SetValueExt<AMVendorShipLine.subItemID>(shipLine, amprodmatl.SubItemID);

                if (IsImport || IsContractBasedAPI)
                {
                    return;
                }

                cache.SetValueExt<AMVendorShipLine.qty>(shipLine, amprodmatl.QtyRemaining.GetValueOrDefault());

                if (amprodmatl.SiteID == null)
                {
                    return;
                }
                
                cache.SetValueExt<AMVendorShipLine.siteID>(shipLine, amprodmatl.SiteID);
                cache.SetValueExt<AMVendorShipLine.locationID>(shipLine, amprodmatl.LocationID);
            }
            finally
            {
                IsSetItemFields = false;
            }
        }

        protected virtual AMProdMatlSplit GetRelatedProdMatlSplit(AMProdMatl matl)
        {
            return (AMProdMatlSplit)PXSelect<AMProdMatlSplit, Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>,
                And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>,
                And<AMProdMatlSplit.operationID, Equal<Required<AMProdMatlSplit.operationID>>,
                And<AMProdMatlSplit.lineID, Equal<Required<AMProdMatlSplit.lineID>>>>>>>.Select(this, matl.OrderType, matl.ProdOrdID, matl.OperationID, matl.LineID).FirstOrDefault();
        }

        protected virtual AMProdMatl GetRelatedProdMatl(AMVendorShipLine shipLine, decimal qtyToProduce)
        {
            if (shipLine == null
                || string.IsNullOrWhiteSpace(shipLine.ProdOrdID)
                || shipLine.OperationID == null
                || shipLine.InventoryID == null)
            {
                return null;
            }

            AMProdMatl prodMatl = null;
            foreach (AMProdMatl row in PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                    And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                    And<AMProdMatl.inventoryID, Equal<Required<AMProdMatl.inventoryID>>,
                    And<Where<AMProdMatl.subItemID, Equal<Required<AMProdMatl.subItemID>>,
                        Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>>>>>>
                    >.Select(this, shipLine.OrderType, shipLine.ProdOrdID, shipLine.OperationID, shipLine.InventoryID, shipLine.SubItemID))
            {
                if (prodMatl == null)
                {
                    prodMatl = row;
                }

                var totalRequiredQty = row.GetTotalReqQty(qtyToProduce);

                var newQty = totalRequiredQty - row.QtyActual.GetValueOrDefault();

                if (newQty > 0)
                {
                    //return the first row with available qty to issue
                    return row;
                }
            }
            return prodMatl;
        }

    }
}