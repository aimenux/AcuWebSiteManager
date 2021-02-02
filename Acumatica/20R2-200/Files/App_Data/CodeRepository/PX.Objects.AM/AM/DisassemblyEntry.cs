using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Common;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Disassembly entry graph
    /// </summary>
    public class DisassemblyEntry : PXGraph<DisassemblyEntry, AMDisassembleBatch>
    {
        public PXSelect<AMDisassembleBatch, Where<AMDisassembleBatch.docType, Equal<AMDocType.disassembly>>> Document;
        public PXSelect<AMDisassembleTran, Where<AMDisassembleTran.docType, Equal<Current<AMDisassembleBatch.docType>>,
            And<AMDisassembleTran.batNbr, Equal<Current<AMDisassembleBatch.batchNbr>>,
                And<AMDisassembleTran.lineNbr, NotEqual<Current<AMDisassembleBatch.refLineNbr>>>>>> MaterialTransactionRecords;
        public PXSelect<AMDisassembleBatch, Where<AMDisassembleBatch.docType, Equal<Current<AMDisassembleBatch.docType>>, 
            And<AMDisassembleBatch.batchNbr, Equal<Current<AMDisassembleBatch.batchNbr>>>>> CurrentDocument;
        public PXSetup<AMPSetup> ampsetup;
        [PXCopyPasteHiddenView]
        public PXSelect<AMDisassembleBatchAttribute, Where<AMDisassembleBatchAttribute.docType, Equal<Current<AMDisassembleBatch.docType>>,
            And<AMDisassembleBatchAttribute.batNbr, Equal<Current<AMDisassembleBatch.batchNbr>>, And<AMDisassembleBatchAttribute.tranLineNbr,
                Equal<Current<AMDisassembleBatch.lineNbr>>>>>> TransactionAttributes;

        [PXCopyPasteHiddenView]
        public PXSelect<AMDisassembleTranSplit,
            Where<AMDisassembleTranSplit.docType, Equal<Current<AMDisassembleTran.docType>>,
            And<AMDisassembleTranSplit.batNbr, Equal<Current<AMDisassembleTran.batNbr>>,
            And<AMDisassembleTranSplit.lineNbr, Equal<Current<AMDisassembleTran.lineNbr>>>>>> MaterialSplits;

        [PXCopyPasteHiddenView]
        public PXSelect<AMDisassembleBatchSplit,
            Where<AMDisassembleBatchSplit.docType, Equal<Current<AMDisassembleBatch.docType>>,
            And<AMDisassembleBatchSplit.batNbr, Equal<Current<AMDisassembleBatch.batchNbr>>,
            And<AMDisassembleBatchSplit.lineNbr, Equal<Current<AMDisassembleBatch.refLineNbr>>>>>> MasterSplits;
        
        public LSAMDisassembleMaterialTran lsselect;
        public LSAMDisassembleMasterTran lsselect2;

        protected bool SkipReleasedReferenceDocsCheck;

        public DisassemblyEntry()
        {
            var setup = ampsetup.Current;
            AMPSetup.CheckSetup(setup);

            if (setup != null && setup.DisassemblyNumberingID == null)
            {
                throw new ProductionSetupNotEnteredException(Messages.GetLocal(Messages.DisassemblyNumberingRequired));
            }

            Views["LSAMDisassembleMasterTran_lotseropts"].AllowSelect =
                PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>()
                && (
                    PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()
                    || PXAccess.FeatureInstalled<FeaturesSet.subItem>()
                    || PXAccess.FeatureInstalled<FeaturesSet.replenishment>()
                );
        }

        public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);
            this.Caches.AddCacheMapping(typeof(INLotSerialStatus), typeof(INLotSerialStatus));
        }

        /// <summary>
        /// Creates a new instances of <see cref="DisassemblyEntry"/> for the given production order
        /// </summary>
        /// <param name="prodItem">Production order to disassemble</param>
        /// <returns>Graph with loaded production order</returns>
        public static DisassemblyEntry ConstructNewDisassemblyEntry(AMProdItem prodItem)
        {
            if (prodItem?.ProdOrdID == null)
            {
                throw new ArgumentNullException(nameof(prodItem));
            }

            if (prodItem.Function != OrderTypeFunction.Disassemble)
            {
                throw new PXException(Messages.IncorrectOrderTypeFunction);
            }

            var disEntry = CreateInstance<DisassemblyEntry>();
            disEntry.Clear();
            // Must use Cache.CreateCopy or this graph looses its mind without it (no allocations, no production attributes)
            var newBatch = (AMDisassembleBatch)disEntry.Document.Cache.CreateCopy(disEntry.Document.Insert(new AMDisassembleBatch()));
            newBatch.OrderType = prodItem.OrderType;
            newBatch.ProdOrdID = prodItem.ProdOrdID;
            var updatedBatch = disEntry.Document.Update(newBatch);
            return disEntry;
        }

        public override void Persist()
        {
            using (var ts = new PXTransactionScope())
            {
                try
                {
                    ReferenceDeleteGraph.DeleteReferenceTransactionsDisassembly(this);

                    base.Persist();
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                    throw;
                }

                ts.Complete();
            }
        }

        #region Disassemble Batch Methods
        protected virtual void AMDisassembleBatch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMDisassembleBatch)e.Row;
            if (row == null)
            {
                return;
            }
            var editablebatch = ((AMDisassembleBatch)e.Row).EditableBatch.GetValueOrDefault();

            release.SetEnabled(!row.Hold.GetValueOrDefault() && editablebatch);

            sender.AllowInsert = true;
            sender.AllowUpdate = 
                sender.AllowDelete = 
                    lsselect.AllowInsert =
                        lsselect.AllowUpdate =
                            lsselect.AllowDelete =
                                //lsselect2.AllowInsert = (DO NOT SET MASTER LS TO ALLOW INSERT - CAUSES ISSUES ON AMDisassembleBatch INSERT button)
                                    lsselect2.AllowUpdate =
                                        lsselect2.AllowDelete =
                                            TransactionAttributes.AllowUpdate = editablebatch;

            SkipReleasedReferenceDocsCheck = false;

            //Supporting transaction attributes from API calls. Users can provide the correct attributes and the graph will manage them correctly based on the expected production attributes.
            //  To allow the calling app the chance to set the tran attributes, we will make the view and fields updatable only to this type of call to this graph
            TransactionAttributes.AllowInsert = editablebatch && (IsImport || IsContractBasedAPI);
            TransactionAttributes.AllowDelete = editablebatch && (IsImport || IsContractBasedAPI);
            PXUIFieldAttribute.SetEnabled<AMDisassembleBatchAttribute.label>(TransactionAttributes.Cache, null, editablebatch && (IsImport || IsContractBasedAPI));
        }

        protected virtual void AMDisassembleBatch_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMDisassembleBatch) e.Row;
            if (row == null || e.Operation == PXDBOperation.Delete)
            {
                return;
            }

            TransactionChecks(sender, row);
        }

        protected virtual void AMDisassembleBatch_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMDisassembleBatch)e.Row;
            if (row == null)
            {
                return;
            }

            SkipReleasedReferenceDocsCheck = true;

            if (ReferenceDeleteGraph.HasReleasedReferenceDocs(this, (AMBatch)row))
            {
                //Have the user confirm the delete when released references exist
                if (Document.Ask(Messages.ConfirmDeleteTitle, Messages.GetLocal(Messages.ConfirmReleasedBatchExist), MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        protected virtual void TransactionChecks(PXCache sender, AMDisassembleBatch row)
        {
            // When any of the checks is an error - we need to break.
            //  Without it another check could be a warning and trump the error (and just show the yellow icon on qty).

            if (CheckMoveOnCompletedOperation(sender, row))
            {
                return;
            }

            CheckOverCompletedOrders(sender, row);
        }

        /// <summary>
        /// Checks for Operation is complete and handle exceptions as needed.
        /// If condition found related to check level. cache received raised exception handling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="row"></param>
        /// <returns>True when raiseexceptionhandling is called on cache AS AN ERROR</returns>
        protected virtual bool CheckMoveOnCompletedOperation(PXCache sender, AMDisassembleBatch row)
        {
            if (row?.OrderType == null || row.Qty.GetValueOrDefault() == 0 && row.QtyScrapped.GetValueOrDefault() == 0)
            {
                return false;
            }

            var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMDisassembleBatch.orderType>(sender, row);
            if (amOrderType == null
                || amOrderType.MoveCompletedOrders == SetupMessage.AllowMsg)
            {
                return false;
            }

            var prodItem = (AMProdItem)PXSelectorAttribute.Select<AMDisassembleBatch.prodOrdID>(sender, row);
            var prodOper = (AMProdOper)PXSelectorAttribute.Select<AMDisassembleBatch.operationID>(sender, row);

            if (ProductionTransactionHelper.CheckMoveOnCompletedOperation(sender, (AMMTran)row, prodOper, prodItem, amOrderType.MoveCompletedOrders, out var exception)
                && exception != null)
            {
                sender.RaiseExceptionHandling<AMDisassembleBatch.qty>(
                    row,
                    row.Qty,
                    new PXSetPropertyException(
                        exception.Message,
                        exception.IsWarning ? PXErrorLevel.Warning : PXErrorLevel.Error));

                if (exception.IsWarning && !IsImport && !IsContractBasedAPI)
                {
                    PXTrace.WriteWarning(exception.Message);
                }

                return !exception.IsWarning;
            }
            return false;
        }

        /// <summary>
        /// Checks for transaction attempt to over complete the order at the last operation
        /// If condition found related to check level. cache received raised exception handling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="row"></param>
        /// <returns>True when raiseexceptionhandling is called on cache AS AN ERROR</returns>
        protected virtual bool CheckOverCompletedOrders(PXCache sender, AMDisassembleBatch row)
        {
            if (row?.OrderType == null || !row.LastOper.GetValueOrDefault() || row.Qty.GetValueOrDefault() == 0 || row.TranType == AMTranType.Adjustment)
            {
                return false;
            }

            var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMDisassembleBatch.orderType>(sender, row);
            if (amOrderType == null
                || amOrderType.OverCompleteOrders == SetupMessage.AllowMsg)
            {
                return false;
            }

            var prodItem = (AMProdItem)PXSelectorAttribute.Select<AMDisassembleBatch.prodOrdID>(sender, row);

            if (ProductionTransactionHelper.CheckOverCompletedOrders(sender, (AMMTran)row, prodItem, amOrderType.OverCompleteOrders,
                ampsetup.Current.InclScrap.GetValueOrDefault(), out var exception)
                && exception != null)
            {
                sender.RaiseExceptionHandling<AMDisassembleBatch.qty>(
                    row,
                    row.Qty,
                    new PXSetPropertyException(
                        exception.Message,
                        exception.IsWarning ? PXErrorLevel.Warning : PXErrorLevel.Error));

                if (exception.IsWarning && !IsImport && !IsContractBasedAPI)
                {
                    PXTrace.WriteWarning(exception.Message);
                }

                return !exception.IsWarning;
            }
            return false;
        }

        protected virtual void AMDisassembleBatch_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Disassembly;
        }

        protected virtual void AMDisassembleBatch_TranDocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Disassembly;
        }

        protected virtual void AMDisassembleBatch_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMDisassembleBatch)e.Row;
            if (row?.InventoryID == null || row.UOM == null)
            {
                e.NewValue = (short) 0;
                return;
            }

            e.NewValue = (short)((row.TranType ?? AMTranType.Disassembly) == AMTranType.Disassembly ? -1 : 1);
        }

        protected virtual void AMDisassembleBatch_Hold_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (AMDisassembleBatch)e.Row;
            if (row == null || !row.Hold.GetValueOrDefault())
            {
                return;
            }

            if (row.Hold.GetValueOrDefault() && !System.Convert.ToBoolean(e.NewValue) && CheckBatchAttributes(false))
            {
                sender.RaiseExceptionHandling<AMDisassembleBatch.hold>(row, row.Hold,
                    new PXSetPropertyException(Messages.GetLocal(Messages.TransactionRequiresAttributes), PXErrorLevel.Error));

                e.NewValue = true;
                e.Cancel = true;   
            }
        }

        protected virtual void DeleteMaterialTransactionRecords()
        {
            foreach (AMDisassembleTran disassembleTran in PXSelect<AMDisassembleTran, 
                Where<AMDisassembleTran.docType, Equal<Required<AMDisassembleBatch.docType>>,
                And<AMDisassembleTran.batNbr, Equal<Required<AMDisassembleBatch.batchNbr>>
                >>>.Select(this, Document.Current.DocType, Document.Current.BatchNbr))
            {
                MaterialTransactionRecords.Delete(disassembleTran);
            }
        }

        /// <summary>
        /// Convert given DACs to AMDisassembleTran
        /// </summary>
        /// <param name="prodMatl">Production Material</param>
        /// <param name="batch">Document batch record</param>
        /// <param name="tran">base transaction recording used for converting (null acceptable)</param>
        /// <returns></returns>
        protected virtual AMDisassembleTran Convert(AMProdMatl prodMatl, AMDisassembleBatch batch, AMDisassembleTran tran)
        {
            if (prodMatl == null)
            {
                throw new ArgumentNullException(nameof(prodMatl));
            }

            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            var tranReturn = tran == null
                ? new AMDisassembleTran()
                : (AMDisassembleTran)MaterialTransactionRecords.Cache.CreateCopy(tran);

            tranReturn.TranOverride = false;
            tranReturn.OrderType = batch.OrderType;
            tranReturn.ProdOrdID = batch.ProdOrdID;
            tranReturn.OperationID = prodMatl.OperationID;
            tranReturn.InventoryID = prodMatl.InventoryID;
            tranReturn.SubItemID = prodMatl.SubItemID;
            tranReturn.SiteID = prodMatl.SiteID;
            tranReturn.LocationID = prodMatl.LocationID;
            if (tranReturn.SiteID == null)
            {
                tranReturn.SiteID = batch.SiteID;
                tranReturn.LocationID = batch.LocationID;
            }
            var matlQty = CalculateMaterialQty(prodMatl, batch);
            tranReturn.Qty = Math.Abs(matlQty);
            tranReturn.TranType = ToTranTranType(batch, matlQty, tranReturn.IsScrap, tranReturn.IsStockItem);
            tranReturn.InvtMult = GetInvtMult(tranReturn);
            tranReturn.UOM = prodMatl.UOM;
            tranReturn.TranDesc = prodMatl.Descr;
            tranReturn.MatlLineId = prodMatl.LineID;

            return tranReturn;
        }

        protected static decimal CalculateMaterialQty(AMProdMatl prodMatl, AMDisassembleBatch batch)
        {
            return prodMatl == null || batch == null
                ? 0m
                : (prodMatl.BatchSize.GetValueOrDefault() == 0m && prodMatl.QtyActual.GetValueOrDefault() != 0 
                    ? 0m 
                    : prodMatl.GetTotalReqQty(batch.BaseQty.GetValueOrDefault()));
        }

        protected virtual void LoadMaterialTransactionRecords(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return;
            }

            foreach (AMProdMatl amProdMatl in PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>>
            >.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                var newTran = Convert(amProdMatl, Document.Current, null);
                if (newTran == null || newTran.Qty.GetValueOrDefault() == 0)
                {
                    // Calculated Qty such as fixed size material would result in no add as material transaction
                    continue;
                }
                
                MaterialTransactionRecords.Insert(newTran);
            }
        }

        protected virtual void UpdateMaterialTransactionRecords(AMDisassembleBatch row)
        {
            if (row?.DocType == null)
            {
                return;
            }

            foreach (PXResult<AMDisassembleTran, AMProdMatl> result in PXSelectJoin<AMDisassembleTran,
                InnerJoin<AMProdMatl,
                    On<AMDisassembleTran.orderType, Equal<AMProdMatl.orderType>,
                        And<AMDisassembleTran.prodOrdID, Equal<AMProdMatl.prodOrdID>,
                            And<AMDisassembleTran.operationID, Equal<AMProdMatl.operationID>,
                                And<AMDisassembleTran.matlLineId, Equal<AMProdMatl.lineID>>>>>>,
                Where<AMDisassembleTran.docType, Equal<Required<AMDisassembleTran.docType>>,
                    And<AMDisassembleTran.batNbr, Equal<Required<AMDisassembleTran.batNbr>>>>>.Select(this, row.DocType, row.BatchNbr))
            {
                var tran = MaterialTransactionRecords.Cache.LocateElseCopy((AMDisassembleTran)result);
                if (tran?.MatlLineId == null || tran.TranOverride.GetValueOrDefault())
                {
                    continue;
                }

                var prodMatl = (AMProdMatl)result;

                MaterialTransactionRecords.Current = tran; // Must set current for the lot/serial logic to correctly work
                var matlQty = CalculateMaterialQty(prodMatl, row);
                tran.Qty = Math.Abs(matlQty);
                tran.TranType = ToTranTranType(row, matlQty, tran.IsScrap, tran.IsStockItem);
                tran.InvtMult = GetInvtMult(tran);
                MaterialTransactionRecords.Update(tran);
            }
        }

        protected static string FlipTranType(AMDisassembleBatch batch, AMDisassembleTran tran)
        {
            var isDisassembly = (batch?.TranType ?? AMTranType.Disassembly) == AMTranType.Disassembly;
            if (tran.IsScrap.GetValueOrDefault() && !tran.IsStockItem.GetValueOrDefault())
            {
                return isDisassembly
                    ? AMTranType.Receipt
                    : AMTranType.Adjustment;
            }

            var isReceipt = (tran?.TranType ?? AMTranType.Receipt) == AMTranType.Receipt;
            return isReceipt ? AMTranType.Adjustment : AMTranType.Receipt;
        }

        protected static string ToTranTranType(AMDisassembleBatch batch, decimal materialQty, bool? isScrap, bool? isStockItem)
        {
            var isDisassembly = (batch?.TranType ?? AMTranType.Disassembly) == AMTranType.Disassembly;
            if (materialQty >= 0 || isScrap.GetValueOrDefault() && !isStockItem.GetValueOrDefault())
            {
                return isDisassembly
                    ? AMTranType.Receipt
                    : AMTranType.Adjustment;
            }

            return isDisassembly
                ? AMTranType.Adjustment
                : AMTranType.Receipt;
        }
        

        protected virtual void UpdateMaterialTranType(AMDisassembleBatch row)
        {
            if (row?.DocType == null)
            {
                return;
            }

            foreach (AMDisassembleTran result in MaterialTransactionRecords.Select())
            {
                if (result.TranOverride.GetValueOrDefault())
                {
                    continue;
                }

                result.TranType = FlipTranType(row, result);
                result.InvtMult = GetInvtMult(result);
                MaterialTransactionRecords.Update(result);
            }
        }

        protected virtual void AMDisassembleBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (AMDisassembleBatch)e.Row;
            if (row == null || row.OrderType == null || row.ProdOrdID == null)
            {
                return;
            }

            var isAutoProcess = IsCopyPasteContext || IsImport || IsContractBasedAPI;
            var orderChanged = !sender.ObjectsEqual<AMDisassembleBatch.orderType, AMDisassembleBatch.prodOrdID>(e.Row, e.OldRow);
            if (orderChanged ||
                // Check for status/cache inserted needed for redirects to work correctly (old/new rows always contain the values)
                sender.GetStatus(row) == PXEntryStatus.Inserted && !MaterialTransactionRecords.Cache.Inserted.Any_()
                )
            {
                if (!isAutoProcess)
                {
                    DeleteMaterialTransactionRecords();
                    var amProdItem = PXSelectorAttribute.Select<AMDisassembleBatch.prodOrdID>(sender, row) as AMProdItem;
                    if (amProdItem?.ProdOrdID == null)
                    {
                        return;
                    }

                    LoadMaterialTransactionRecords(amProdItem);
                }
                if (orderChanged)
                {
                    SyncTransactionAttributes(row);
                }
                return;
            }

            if (isAutoProcess)
            {
                return;
            }

            if (!sender.ObjectsEqual<AMDisassembleBatch.baseQty>(e.Row, e.OldRow))
            {
                UpdateMaterialTransactionRecords(row);
                return;
            }

            if (!sender.ObjectsEqual<AMDisassembleBatch.tranType>(e.Row, e.OldRow))
            {
                UpdateMaterialTranType(row);
            }
        }

        #endregion

        #region Disassemble Tran Methods

        protected virtual void AMDisassembleTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;

            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.reasonCodeID>(sender, row, row.IsScrap.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.tranOverride>(sender, row, row.MatlLineId != null || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.operationID>(sender, row, row.MatlLineId == null || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.inventoryID>(sender, row, row.MatlLineId == null || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.subItemID>(sender, row, row.MatlLineId == null || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.qty>(sender, row, row.TranOverride.GetValueOrDefault() || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.unitCost>(sender, row, row.TranOverride.GetValueOrDefault() || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.uOM>(sender, row, row.TranOverride.GetValueOrDefault() || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.siteID>(sender, row, row.TranOverride.GetValueOrDefault() || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.tranType>(sender, row, row.TranOverride.GetValueOrDefault() || IsCopyPasteContext);
            PXUIFieldAttribute.SetEnabled<AMDisassembleTran.tranDesc>(sender, row, row.TranOverride.GetValueOrDefault() || IsCopyPasteContext);

#if DEBUG
            AMDebug.TraceWriteMethodName($"LINE {row.LineNbr} Is Override = {row.TranOverride} - TranOverride enabled = {Common.Cache.GetEnabled<AMDisassembleTran.tranOverride>(sender, row)}");
            AMDebug.TraceWriteMethodName($"LINE {row.LineNbr} Is Override = {row.TranOverride} - Qty enabled = {Common.Cache.GetEnabled<AMDisassembleTran.qty>(sender, row)}");
#endif
        }

        protected virtual void AMDisassembleTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (string.IsNullOrWhiteSpace(row?.ProdOrdID) || e.Operation == PXDBOperation.Delete)
            {
                return;
            }
            
            // Check For Reason Code Required and null
            if (row.ReasonCodeID == null && row.IsScrap.GetValueOrDefault() && row.Qty.GetValueOrDefault() != 0)
            {
                sender.RaiseExceptionHandling<AMDisassembleTran.reasonCodeID>(row, row.ReasonCodeID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
            }

            if (row.AcctID == null && row.ReasonCodeID != null)
            {
                sender.RaiseExceptionHandling<AMDisassembleTran.acctID>(row, row.AcctID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
            }

            if (row.SubID == null && row.ReasonCodeID != null)
            {
                sender.RaiseExceptionHandling<AMDisassembleTran.subID>(row, row.SubID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
            }
        }

        protected virtual void AMDisassembleTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (row == null)
            {
                return;
            }

            //Only prompt when a non referenced batch
            if (Document.Current != null && string.IsNullOrWhiteSpace(Document.Current.OrigBatNbr)
                && row.DocType == Document.Current.DocType && row.BatNbr == Document.Current.BatchNbr
                && !SkipReleasedReferenceDocsCheck)
            {
                if (ReferenceDeleteGraph.HasReleasedReferenceDocs(this, (AMMTran)row))
                {
                    //Have the user confirm the delete when released references exist
                    if (MaterialTransactionRecords.Ask(Messages.ConfirmDeleteTitle, Messages.GetLocal(Messages.ConfirmReleasedTransactionsExist), MessageButtons.YesNo) != WebDialogResult.Yes)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        /// <summary>
        /// Checks for Over Issue of Material for a given material entry.
        /// If over issue found related to check level. cache received raised exception handling.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="sender"></param>
        /// <param name="ammTran"></param>
        /// <param name="newQty"></param>
        protected virtual bool CheckOverIssueMaterialOnEntry(PXGraph graph, PXCache sender, AMDisassembleTran ammTran, decimal? newQty)
        {
            if (ammTran == null
                || ammTran.DocType == null
                || ammTran.OrderType == null
                || ammTran.ProdOrdID == null
                || newQty.GetValueOrDefault() == 0)
            {
                return false;
            }

            var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMDisassembleTran.orderType>(sender, ammTran);

            if (amOrderType == null || amOrderType.OverIssueMaterial == SetupMessage.AllowMsg
                || amOrderType.OverIssueMaterial == SetupMessage.WarningMsg && (graph.IsImport || graph.IsContractBasedAPI))
            {
                return false;
            }

            AMProdMatl amProdMatl = PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>
                >>>>>.Select(this, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.MatlLineId);

            if (amProdMatl == null)
            {
                return false;
            }

            var qtyRemaining = amProdMatl.QtyRemaining.GetValueOrDefault();
            if (ammTran.UOM != null && !amProdMatl.UOM.EqualsWithTrim(ammTran.UOM))
            {
                //Convert production UOM qty remaining...
                decimal? convertedQty = 0;
                if (UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(Caches[typeof(AMProdMatl)], amProdMatl,
                    amProdMatl.UOM, ammTran.UOM, qtyRemaining, out convertedQty))
                {
                    qtyRemaining = convertedQty.GetValueOrDefault();
                }
            }

            if (newQty.GetValueOrDefault() > qtyRemaining)
            {
                var currentInventoryID = (string)sender.GetStateExt<AMDisassembleTran.inventoryID>(ammTran);
                var currentOperationCD = (string)sender.GetStateExt<AMDisassembleTran.operationID>(ammTran);

                var exceptionMsg = Messages.GetLocal(Messages.MaterialQuantityOverIssue,
                    ammTran.UOM,
                    UomHelper.FormatQty(newQty.GetValueOrDefault()),
                    UomHelper.FormatQty(qtyRemaining),
                    $"{ammTran.OrderType} {ammTran.ProdOrdID.TrimIfNotNullEmpty()}",
                    currentOperationCD,
                    currentInventoryID,
                    amProdMatl.LineNbr
                    );

                sender.RaiseExceptionHandling<AMDisassembleTran.qty>(
                    ammTran,
                    ammTran.Qty,
                    new PXSetPropertyException(
                        exceptionMsg,
                        amOrderType.OverIssueMaterial == SetupMessage.WarningMsg ? PXErrorLevel.Warning : PXErrorLevel.Error));

                if (amOrderType.OverIssueMaterial == SetupMessage.WarningMsg && !IsImport && !IsContractBasedAPI)
                {
                    PXTrace.WriteWarning(exceptionMsg);
                }
            }

            return amOrderType.OverIssueMaterial != SetupMessage.WarningMsg;
        }

        protected virtual void AMDisassembleTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (row == null)
            {
                return;
            }

            e.NewValue = GetInvtMult(row);
        }

        protected virtual void AMDisassembleTran_TranOverride_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (row == null)
            {
                return;
            }

            if (IsCopyPasteContext)
            {
                e.Cancel = true;
            }
            
        }

        protected virtual short GetInvtMult(AMDisassembleTran row)
        {
            if (!row.IsStockItem.GetValueOrDefault(true) || row.IsScrap.GetValueOrDefault())
            {
                return (short)0;
            }
            return (short)((row.TranType ?? AMTranType.Receipt) == AMTranType.Receipt ? 1 : -1);
        }

        protected virtual void AMDisassembleTran_TranType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (row == null)
            {
                return;
            }

            e.NewValue = (Document?.Current?.TranType ?? AMTranType.Disassembly) == AMTranType.Disassembly ? AMTranType.Receipt : AMTranType.Adjustment;
        }

        protected virtual void AMDisassembleTran_TranOverride_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (row == null || e.NewValue == null)
            {
                return;
            }

            if (!System.Convert.ToBoolean(e.NewValue) && row.MatlLineId == null && !IsCopyPasteContext)
            {
                // Not possible to set not override if line not linkedto production material line
                e.NewValue = true;
            }
        }

        protected virtual void AMDisassembleTran_IsScrap_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMDisassembleTran) e.Row;
            if (row == null || e.OldValue == null)
            {
                return;
            }

            if (!row.IsScrap.GetValueOrDefault() && System.Convert.ToBoolean(e.OldValue))
            {
                //Changed to unchecked...
                sender.SetValueExt<AMDisassembleTran.subID>(row, null);
                sender.SetValueExt<AMDisassembleTran.acctID>(row, null);
                sender.SetValueExt<AMDisassembleTran.reasonCodeID>(row, null);
                // Check value set via DAC field defaults...
            }
        }

        protected virtual void AMDisassembleTran_Qty_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            AMDisassembleTran row = (AMDisassembleTran)e.Row;

            if (row == null)
            {
                return;
            }

            if (CheckOverIssueMaterialOnEntry(this, sender, row, (decimal?)e.NewValue))
            {
                e.NewValue = row.Qty.GetValueOrDefault();
                e.Cancel = true;
            }
        }

        protected virtual void AMDisassembleTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (row == null)
            {
                return;
            }

            if (!sender.ObjectsEqual<AMDisassembleTran.tranOverride>(e.Row, e.OldRow) && row.TranOverride == false)
            {
                AMProdMatl amProdMatl = PXSelect<AMProdMatl,
                    Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                        And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                        And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                        And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>>
                            >>>>.Select(this, row.OrderType, row.ProdOrdID, row.OperationID, row.MatlLineId);

                MaterialTransactionRecords.Update(Convert(amProdMatl, Document.Current, row));
            }


        }

        protected virtual void AMDisassembleTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (string.IsNullOrWhiteSpace(row?.ProdOrdID))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.ReasonCodeID))
            {
                e.NewValue = null;
                return;
            }

            // Sub by reason code... (This configuration required to get the correct string value then set the correct int value for NEW SUB COMBO ON THE FLY)

            object reasonCodeSubId = GLAccountHelper.GetReasonCodeSubIDString(sender, (AMMTran)row);
            sender.RaiseFieldUpdating<AMDisassembleTran.subID>(row, ref reasonCodeSubId);

            e.NewValue = (int?)reasonCodeSubId;
        }

        protected virtual void AMDisassembleTran_SubID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (AMDisassembleTran)e.Row;
            if (string.IsNullOrWhiteSpace(row?.ReasonCodeID))
            {
                return;
            }

            var subStringValue = GLAccountHelper.GetReasonCodeSubIDString(sender, (AMMTran)row);
            if (!string.IsNullOrWhiteSpace(subStringValue))
            {
                e.NewValue = subStringValue;
            }
        }

        #endregion

        protected virtual void AMDisassembleTranSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMDisassembleTranSplit) e.Row;

            if (row == null)
            {
                return;
            }

            var parent = (AMDisassembleTran)PXParentAttribute.LocateParent(sender, row, typeof(AMDisassembleTran));
            if (parent == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMDisassembleTranSplit.qty>(sender, row, parent.TranOverride == true);
        }

        #region Disassemble Batch Attribute Methods
        protected virtual void AMDisassembleBatchAttribute_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Disassembly;
        }

        protected virtual void AMDisassembleBatchAttribute_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            var row = (AMDisassembleBatchAttribute)e.Row;
            var isApi = IsImport || IsContractBasedAPI;
            if (row == null || !isApi || IsCopyPasteContext)
            {
                return;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"DocType = {row.DocType}, BatNbr = {row.BatNbr}, TranLineNbr = {row.TranLineNbr}, LineNbr = {row.LineNbr}, Label = {row.Label}, Value = {row.Value}, Order = {row.OrderType}-{row.ProdOrdID}");
            // We cannot cancel the row insert for API calls or we will end up with this message to the service call...
            //      PX.Api.ContractBased.OutcomeEntityHasErrorsException: PX.Data.PXException: Error: The system failed to commit the TransactionAttributes row.
            //e.Cancel = true; 
            //lets update the values of this row inserting to equal what is already in place for the row with the same label and remove the other row (as labels are unique)
#endif

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                return;
            }

            var parentRow = (AMDisassembleBatch)PXParentAttribute.SelectParent(sender, row, typeof(AMDisassembleBatch));
            if (parentRow == null)
            {
                return;
            }

            var prodTranAttributes = GetProductionAttributeDictionary(parentRow);
            if (prodTranAttributes == null || !prodTranAttributes.ContainsKey(row.Label.Trim()))
            {
                throw new PXException(Messages.GetLocal(Messages.OrderAttributeNotFound, row.Label.TrimIfNotNullEmpty(), parentRow.OrderType.TrimIfNotNullEmpty(), parentRow.ProdOrdID.TrimIfNotNullEmpty()));
            }

            var cachedTranAttributes = GetTransactionAttributeDictionary(parentRow, true);
            if (!cachedTranAttributes.ContainsKey(row.Label.Trim()))
            {
                return;
            }
            var tranAttWithSameLabel = cachedTranAttributes[row.Label.Trim()];
            row.OrderType = tranAttWithSameLabel.OrderType;
            row.ProdOrdID = tranAttWithSameLabel.ProdOrdID;
            row.OperationID = tranAttWithSameLabel.OperationID;
            row.ProdAttributeLineNbr = tranAttWithSameLabel.LineNbr;
            row.AttributeID = tranAttWithSameLabel.AttributeID;
            row.Label = tranAttWithSameLabel.Label;
            row.Descr = tranAttWithSameLabel.Descr;
            row.TransactionRequired = tranAttWithSameLabel.TransactionRequired;
            if (string.IsNullOrWhiteSpace(row.Value))
            {
                row.Value = tranAttWithSameLabel.Value;
            }

            DeleteAMMTranAttribute(sender, tranAttWithSameLabel);
        }

        protected virtual void AMDisassembleBatchAttribute_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMDisassembleBatchAttribute)e.Row;
            if (row == null || e.Operation == PXDBOperation.Delete || Document?.Current == null)
            {
                return;
            }

            CheckBatchAttribute(sender, row, Document?.Current?.Hold ?? false);
        }

        protected virtual bool CheckBatchAttribute(PXCache cache, AMDisassembleBatchAttribute row, bool isDocumentOnHold)
        {
            if (row != null && row.TransactionRequired.GetValueOrDefault() && row.Value == null && TransactionAttributes.Cache.GetStatus(row) != PXEntryStatus.Deleted)
            {
                cache.RaiseExceptionHandling<AMDisassembleBatchAttribute.value>(row, row.Value,
                    new PXSetPropertyException(Messages.GetLocal(Messages.TranLineRequiresAttribute,
                        AMDocType.GetDocTypeDesc(row.DocType),
                        cache.GetStatus(row) == PXEntryStatus.Inserted ? string.Empty : row.BatNbr.TrimIfNotNullEmpty(),
                        null, row.Label), isDocumentOnHold ? PXErrorLevel.Warning : PXErrorLevel.Error));
                return !isDocumentOnHold;
            }
            return false;
        }

        protected virtual bool CheckBatchAttributes(bool isDocumentOnHold)
        {
            var containsErrors = false;
            foreach (AMDisassembleBatchAttribute row in TransactionAttributes.Select())
            {
                containsErrors |= CheckBatchAttribute(TransactionAttributes.Cache, row, isDocumentOnHold);
            }
            return containsErrors;
        }

        /// <summary>
        /// Query all production attributes related to transaction
        /// </summary>
        /// <param name="row">transaction row</param>
        /// <returns>query results in a dictionary with a key by Label</returns>
        protected virtual Dictionary<string, AMProdAttribute> GetProductionAttributeDictionary(AMDisassembleBatch row)
        {
            if (string.IsNullOrWhiteSpace(row?.ProdOrdID))
            {
                return null;
            }

            PXSelectBase<AMProdAttribute> cmd = new PXSelect<AMProdAttribute,
                Where<AMProdAttribute.orderType, Equal<Required<AMProdAttribute.orderType>>,
                    And<AMProdAttribute.prodOrdID, Equal<Required<AMProdAttribute.prodOrdID>>,
                        And<AMProdAttribute.enabled, Equal<boolTrue>,
                            And<AMProdAttribute.source, NotEqual<AMAttributeSource.configuration>>>>>>(this);

            var dic = new Dictionary<string, AMProdAttribute>();
            foreach (AMProdAttribute result in cmd.Select(row.OrderType, row.ProdOrdID))
            {
                if (result.Label == null || dic.ContainsKey(result.Label.Trim()))
                {
                    continue;
                }

                dic.Add(result.Label.Trim(), result);
            }
            return dic;
        }

        /// <summary>
        /// Insert given production attribute into the cache as a tran attribute
        /// </summary>
        /// <param name="prodAttribute">Production attribute row</param>
        /// <param name="row">Related parent transaction row</param>
        /// <returns>Inserted transaction attribute</returns>
        protected virtual AMDisassembleBatchAttribute InsertAMMTranAttribute(AMProdAttribute prodAttribute, AMDisassembleBatch row)
        {
            return TransactionAttributes.Insert(new AMDisassembleBatchAttribute
            {
                // Need the keys for API calls to work correctly
                DocType = row.DocType,
                BatNbr = row.BatchNbr,
                TranLineNbr = row.LineNbr,
                OrderType = row.OrderType,
                ProdOrdID = row.ProdOrdID,
                OperationID = prodAttribute.OperationID,
                ProdAttributeLineNbr = prodAttribute.LineNbr,
                AttributeID = prodAttribute.AttributeID,
                Label = prodAttribute.Label,
                Descr = prodAttribute.Descr,
                TransactionRequired = prodAttribute.TransactionRequired,
                Value = prodAttribute.Value
            });
        }

        /// <summary>
        /// Delete the given transaction attribute
        /// </summary>
        /// <param name="cache">cache of AMMTranAttribute</param>
        /// <param name="row">AMMTranAttribute to delete</param>
        protected virtual void DeleteAMMTranAttribute(PXCache cache, AMDisassembleBatchAttribute row)
        {
            var status = cache.GetStatus(cache.LocateElse(row));
            if (status == PXEntryStatus.Inserted)
            {
                cache.Remove(row);
                return;
            }
            cache.Delete(row);
        }

        /// <summary>
        /// Sync the given transaction row's production transaction attributes. Add/Update/Delete tran attributes based on changed values
        /// </summary>
        /// <param name="row">Parent transaction row</param>
        protected virtual void SyncTransactionAttributes(AMDisassembleBatch row)
        {
            if (string.IsNullOrWhiteSpace(row.OrderType) && string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return;
            }

            var tranAttributes = GetTransactionAttributeDictionary(row);
            var prodAttributes = GetProductionAttributeDictionary(row);

            var deleteList = new List<AMDisassembleBatchAttribute>();

            // Check for DELETES of incorrectly added attributes...
            foreach (var kvp in tranAttributes)
            {
                if (!prodAttributes.ContainsKey(kvp.Key))
                {
                    deleteList.Add(kvp.Value);
                    continue;
                }
            }

            // Then delete those tran attributes from the cache...
            foreach (var tranAttribute in deleteList)
            {
                if (tranAttribute.Label != null && tranAttributes.ContainsKey(tranAttribute.Label.Trim()))
                {
                    tranAttributes.Remove(tranAttribute.Label.Trim());
                }
                DeleteAMMTranAttribute(TransactionAttributes.Cache, tranAttribute);
            }

            // Next insert/update tran attributes for missing or found but needs updated results
            foreach (var kvp in prodAttributes)
            {
                if (!tranAttributes.ContainsKey(kvp.Key))
                {
                    InsertAMMTranAttribute(kvp.Value, row);
                    continue;
                }

                //Update tranAttributes
                var cachedRow = TransactionAttributes.Cache.LocateElseCopy(tranAttributes[kvp.Key]);
                if (cachedRow == null)
                {
                    continue;
                }

                cachedRow.AttributeID = kvp.Value.AttributeID;
                cachedRow.Descr = kvp.Value.Descr;
                cachedRow.ProdAttributeLineNbr = kvp.Value.LineNbr;
                cachedRow.OperationID = kvp.Value.OperationID;
                cachedRow.TransactionRequired = kvp.Value.TransactionRequired;
                if (cachedRow.Value == null)
                {
                    cachedRow.Value = kvp.Value.Value;
                }
            }
        }

        /// <summary>
        /// Get the existing transaction attributes in the form of a dictionary by label key
        /// </summary>
        /// <param name="row">Parent transaction row</param>
        /// <returns></returns>
        protected virtual Dictionary<string, AMDisassembleBatchAttribute> GetTransactionAttributeDictionary(AMDisassembleBatch row)
        {
            return GetTransactionAttributeDictionary(row, false);
        }

        /// <summary>
        /// Get the existing transaction attributes in the form of a dictionary by label key
        /// </summary>
        /// <param name="row">Parent transaction row</param>
        /// <param name="cachedOnly">only return cached rows (no select on AMMTranAttributes)</param>
        /// <returns></returns>
        protected virtual Dictionary<string, AMDisassembleBatchAttribute> GetTransactionAttributeDictionary(AMDisassembleBatch row, bool cachedOnly)
        {
            if (row == null)
            {
                return null;
            }

            var tranAttributeDic = new Dictionary<string, AMDisassembleBatchAttribute>();
            foreach (AMDisassembleBatchAttribute att in TransactionAttributes.Cache.Cached)
            {
                if (att.Label == null || att.TranLineNbr != row.LineNbr || tranAttributeDic.ContainsKey(att.Label.Trim()))
                {
                    continue;
                }

                tranAttributeDic.Add(att.Label.Trim(), att);
            }

            if (cachedOnly)
            {
                return tranAttributeDic;
            }

            foreach (AMDisassembleBatchAttribute att in PXSelect<AMDisassembleBatchAttribute, 
                Where<AMDisassembleBatchAttribute.docType, Equal<Current<AMDisassembleBatch.docType>>,
                    And<AMDisassembleBatchAttribute.batNbr, Equal<Current<AMDisassembleBatch.batchNbr>>, 
                    And<AMDisassembleBatchAttribute.tranLineNbr, Equal<Current<AMDisassembleBatch.lineNbr>>>>>>
                .SelectMultiBound(this, new object[] { row }))
            {
                if (att.Label == null || att.TranLineNbr != row.LineNbr || tranAttributeDic.ContainsKey(att.Label.Trim()))
                {
                    continue;
                }

                tranAttributeDic.Add(att.Label.Trim(), att);
            }
            return tranAttributeDic;
        }
        /// <summary>
        /// When deleting then inserting, the line counters are not getting set correctly on the insert. 
        /// Use this to increase the line counter so the inserts work correctly
        /// </summary>
        /// <param name="row"></param>
        protected virtual void BurnAttributeLineNbr(AMDisassembleBatch row)
        {
            PXLineNbrAttribute.NewLineNbr<AMDisassembleBatchAttribute.lineNbr>(TransactionAttributes.Cache, row);
        }

        protected virtual AMDisassembleBatch GetParent(PXCache cache, AMDisassembleBatchAttribute amDisassembleBatchAttribute)
        {
            var parent = (AMDisassembleBatch)PXParentAttribute.LocateParent(cache, amDisassembleBatchAttribute, typeof(AMDisassembleBatch));

            if (parent != null)
            {
                return parent;
            }

            parent = (AMDisassembleBatch)PXParentAttribute.SelectParent(cache, amDisassembleBatchAttribute, typeof(AMDisassembleBatch));

            if (parent != null)
            {
                return parent;
            }
            
            return null;
        }
        #endregion

        #region AMDisassembleBatchSplit

        protected virtual void AMDisassembleBatchSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AMDisassembleBatchSplit row = e.Row as AMDisassembleBatchSplit;
            if (row != null)
            {
                PXUIFieldAttribute.SetEnabled<AMDisassembleBatchSplit.lotSerialNbr>(sender, row, row.DocType == AMDocType.Disassembly);
                PXUIFieldAttribute.SetEnabled<AMDisassembleBatchSplit.subItemID>(sender, row, sender.GetStatus(row) == PXEntryStatus.Inserted);
                PXUIFieldAttribute.SetEnabled<AMDisassembleBatchSplit.locationID>(sender, row, sender.GetStatus(row) == PXEntryStatus.Inserted);
            }
        }

        #endregion

        #region Buttons

        public PXAction<AMDisassembleBatch> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            var disBatchlist = new List<AMDisassembleBatch>();
            foreach (var amdoc in adapter.Get<AMDisassembleBatch>())
            {
                if (amdoc.Hold.GetValueOrDefault() || amdoc.Released.GetValueOrDefault())
                {
                    continue;
                }
                var amdoc2 = Document.Update(amdoc);
                disBatchlist.Add(amdoc2);
            }
            if (disBatchlist.Count == 0)
            {
                throw new PXException(PX.Objects.IN.Messages.Document_Status_Invalid);
            }
            Save.Press();

            var list = new List<AMBatch>();
            foreach (var disassembleBatch in disBatchlist)
            {
                AMBatch doc = PXSelect<AMBatch, Where<AMBatch.docType, Equal<Required<AMBatch.docType>>, And<AMBatch.batNbr, Equal<Required<AMBatch.batNbr>>>>>.Select(this, disassembleBatch.DocType, disassembleBatch.BatchNbr);
                if (doc == null)
                {
                    continue;
                }
                list.Add(doc);
            }

            PXLongOperation.StartOperation(this, delegate { AMDocumentRelease.ReleaseDoc(list); });

            return disBatchlist;
        }

        public PXAction<AMDisassembleBatch> copyLine;
        [PXUIField(DisplayName = Messages.CopyLine, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable CopyLine(PXAdapter adapter)
        {
            if (MaterialTransactionRecords.Current == null)
            {
                return adapter.Get();
            }
            var copyRecord = PXCache<AMDisassembleTran>.CreateCopy(this.MaterialTransactionRecords.Current);
            copyRecord.LineNbr = null;
            copyRecord.TranOverride = true;
            this.MaterialTransactionRecords.Insert(copyRecord);
            return adapter.Get();
        }

        #endregion
    }
}