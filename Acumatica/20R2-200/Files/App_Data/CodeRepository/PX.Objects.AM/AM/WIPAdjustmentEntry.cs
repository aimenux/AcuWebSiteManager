using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing WIP Adjustment Entry
    /// </summary>
    public class WIPAdjustmentEntry : AMBatchSimpleEntryBase
    {
        public PXSelect<AMBatch, Where<AMBatch.docType, Equal<AMDocType.wipAdjust>>> batch;
        [PXImport(typeof(AMBatch))]
        public PXSelect<AMMTran, Where<AMMTran.docType, Equal<Current<AMBatch.docType>>, And<AMMTran.batNbr, Equal<Current<AMBatch.batNbr>>>>> transactions;

        /// <summary>
        /// Unrleased WIP Amount by production order operation
        /// (key = AMProdOper.JoinKey())
        /// </summary>
        protected Dictionary<string, decimal> UnreleasedWipByOperation;

        public override void Clear()
        {
            base.Clear();
            UnreleasedWipByOperation = null;
        }

        public PXAction<AMBatch> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            PXCache cache = batch.Cache;
            List<AMBatch> list = new List<AMBatch>();
            foreach (AMBatch amdoc in adapter.Get<AMBatch>())
            {
                if (amdoc.Hold == false && amdoc.Released == false)
                {
                    cache.Update(amdoc);
                    list.Add(amdoc);
                }
            }
            if (list.Count == 0)
            {
                throw new PXException(PX.Objects.IN.Messages.Document_Status_Invalid);
            }
            Save.Press();

            PXLongOperation.StartOperation(this, delegate() { AMDocumentRelease.ReleaseDoc(list); });
            return list;
        }

        protected virtual void AMBatch_TranDesc_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = Messages.ProdGLEntry_WIPAdjustment;
        }

        protected virtual void AMMTran_TranDesc_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = Messages.ProdGLEntry_WIPAdjustment;
        }

        protected virtual void AMBatch_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.WipAdjust;
        }

        protected virtual void AMMTran_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.WipAdjust;
        }

        protected virtual void AMMTran_TranType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMTranType.WIPadjustment;
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Tran Description", Visible = true)]
        protected virtual void _(Events.CacheAttached<AMMTran.tranDesc> e) { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        protected virtual void AMMTran_UnitCost_CacheAttached(PXCache sender)
        {
        }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Cost Adj", Enabled = true, Visible = true)]
        [PXFormula(null, typeof(SumCalc<AMBatch.totalAmount>))]
        protected virtual void AMMTran_TranAmt_CacheAttached(PXCache sender)
        {
        }

        [Account]
        [PXDefault(typeof(Coalesce<
            Search<ReasonCode.accountID, Where<ReasonCode.reasonCodeID, Equal<Current<AMMTran.reasonCodeID>>>>,
            Search<AMProdItem.wIPVarianceAcctID, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>>))]
        [PXFormula(typeof(Default<AMMTran.prodOrdID, AMMTran.reasonCodeID>))]
        protected virtual void AMMTran_AcctID_CacheAttached(PXCache sender)
        {
        }

        [SubAccount(typeof(AMMTran.acctID))]
        [PXFormula(typeof(Default<AMMTran.prodOrdID, AMMTran.reasonCodeID>))]
        protected virtual void AMMTran_SubID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void AMMTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (string.IsNullOrWhiteSpace(row?.ProdOrdID))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.ReasonCodeID))
            {
                // Sub by production order
                var prodItem = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(sender, e.Row);
                e.NewValue = prodItem?.WIPVarianceSubID;
                return;
            }

            // Sub by reason code... (This configuration required to get the correct string value then set the correct int value for NEW SUB COMBO ON THE FLY)

            object reasonCodeSubId = GLAccountHelper.GetReasonCodeSubIDString(sender, row);
            sender.RaiseFieldUpdating<AMMTran.subID>(row, ref reasonCodeSubId);

            e.NewValue = (int?) reasonCodeSubId;
        }

        protected virtual void AMMTran_SubID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (AMMTran) e.Row;
            if (string.IsNullOrWhiteSpace(row?.ReasonCodeID))
            {
                return;
            }

            var subStringValue = GLAccountHelper.GetReasonCodeSubIDString(sender, row);

            if (!string.IsNullOrWhiteSpace(subStringValue))
            {
                e.NewValue = subStringValue;
            }
        }

        [PXDefault]
        [Inventory(Visible = false)]
        protected virtual void AMMTran_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [SubItem(
            typeof(AMMTran.inventoryID),
            typeof(LeftJoin<INSiteStatus,
                On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
                    And<INSiteStatus.inventoryID, Equal<Optional<AMMTran.inventoryID>>,
                        And<INSiteStatus.siteID, Equal<Optional<AMMTran.siteID>>>>>>), Visible = false)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void AMMTran_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXUIField(DisplayName = "Qty Scrapped", Visible = false)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void AMMTran_QtyScrapped_CacheAttached(PXCache sender)
        {
        }

        [Site(Visible = false)]
        protected virtual void AMMTran_SiteID_CacheAttached(PXCache sender)
        {
        }

        [Location(Visible = false)]
        protected virtual void AMMTran_LocationID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void AMBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            AMBatch ambatch = (AMBatch)e.Row;

            if (ampsetup.Current.RequireControlTotal == false)
            {
                if (PXCurrencyAttribute.IsNullOrEmpty(((AMBatch)e.Row).TotalAmount) == false)
                {
                    sender.SetValue<AMBatch.controlAmount>(e.Row, ((AMBatch)e.Row).TotalAmount);
                }
                else
                {
                    sender.SetValue<AMBatch.controlAmount>(e.Row, 0m);
                }

            }

            if (((AMBatch)e.Row).Hold == false && ((AMBatch)e.Row).Released == false)
            {
                if ((bool)ampsetup.Current.RequireControlTotal)
                {
                    if (((AMBatch)e.Row).TotalAmount != ((AMBatch)e.Row).ControlAmount)
                    {
                        sender.RaiseExceptionHandling<AMBatch.controlAmount>(e.Row, ((AMBatch)e.Row).ControlAmount, new PXSetPropertyException(PX.Objects.IN.Messages.DocumentOutOfBalance));
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<AMBatch.controlAmount>(e.Row, ((AMBatch)e.Row).ControlAmount, null);
                    }

                }
            }
        }

        protected virtual void AMBatch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var editablebatch = ((AMBatch)e.Row).EditableBatch.GetValueOrDefault();

            release.SetEnabled(e.Row != null && ((AMBatch)e.Row).Hold == false && editablebatch);

            sender.AllowInsert = true;
            sender.AllowUpdate = editablebatch;
            sender.AllowDelete = editablebatch;

            //Manage rights to line detail
            transactions.Cache.AllowInsert = editablebatch;
            transactions.Cache.AllowUpdate = editablebatch;
            transactions.Cache.AllowDelete = editablebatch;

            PXUIFieldAttribute.SetVisible<AMBatch.controlQty>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault());
            PXUIFieldAttribute.SetVisible<AMBatch.controlAmount>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMBatch.status>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<AMBatch.hold>(sender, e.Row, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.finPeriodID>(sender, e.Row, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlQty>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault() && editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlAmount>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault() && editablebatch);
        }

        protected virtual void AMMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMMTran) e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMMTran.inventoryID>(sender, row, false);
            PXUIFieldAttribute.SetEnabled<AMMTran.subItemID>(sender, row, false);
            PXUIFieldAttribute.SetEnabled<AMMTran.siteID>(sender, row, false);
            PXUIFieldAttribute.SetEnabled<AMMTran.locationID>(sender, row, false);
            PXUIFieldAttribute.SetEnabled<AMMTran.qtyScrapped>(sender, row, false);

            PXUIFieldAttribute.SetEnabled<AMMTran.acctID>(sender, row, string.IsNullOrWhiteSpace(row.ReasonCodeID));
            PXUIFieldAttribute.SetEnabled<AMMTran.subID>(sender, row, string.IsNullOrWhiteSpace(row.ReasonCodeID));
        }

        protected virtual void AMMTran_ProdOrdID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var tran = (AMMTran)e.Row;
            if (tran == null || string.IsNullOrWhiteSpace(tran.ProdOrdID))
            {
                return;
            }

            var amproditem = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(cache, e.Row);

            if (amproditem == null)
            {
                tran.ProdOrdID = null;
                return;
            }

            cache.SetValueExt<AMMTran.operationID>(tran, null);
            cache.SetValueExt<AMMTran.inventoryID>(tran, amproditem.InventoryID);
            cache.SetValueExt<AMMTran.uOM>(tran, amproditem.UOM);
            cache.SetValueExt<AMMTran.siteID>(tran, amproditem.SiteID);
            cache.SetValueExt<AMMTran.locationID>(tran, amproditem.LocationID);
        }

        /// <summary>
        /// Create transactions related to a move transaction and scrap. Scrap cost is distributed across the current and previous operations (reason for a list of AMMTran as return)
        /// </summary>
        /// <param name="moveTransaction">Move transaction driving scrap transactions</param>
        /// <param name="prodItem">Production Item record related to the currently processing move transaction</param>
        /// <param name="operationsList">List of production operations related to moving production order</param>
        /// <param name="prodOperCache">AMProdOper cache for locating cached updates</param>
        /// <returns></returns>
        public virtual List<AMMTran> CreateScrapTransactions(AMMTran moveTransaction, InventoryItem inventoryItem, AMProdItem prodItem, MoveOperationQtyTotals moveOperQtyTotals, PXCache prodOperCache)
        {
            var list = new List<AMMTran>();
            if (moveTransaction?.LineNbr == null)
            {
                throw new PXArgumentException(nameof(moveTransaction));
            }

            if (moveOperQtyTotals == null)
            {
                throw new PXArgumentException(nameof(moveOperQtyTotals));
            }

            if (inventoryItem == null)
            {
                throw new PXArgumentException(nameof(inventoryItem));
            }

            if (prodItem == null)
            {
                throw new PXArgumentException(nameof(prodItem));
            }

            if (prodOperCache == null)
            {
                throw new PXArgumentException(nameof(prodOperCache));
            }

            var isQuarantine = moveTransaction.IsScrap == true && moveTransaction.ScrapAction == ScrapAction.Quarantine;
            var operations = moveOperQtyTotals.OperationsList.OrderBy(x => x.OperationCD).ToList();

            //For scrap we want the total being moved for the transaction operation which will apply to that operation and all previous operations
            var tranTransactionTotalMoveBaseQty = moveOperQtyTotals.GetOperationTotal(moveTransaction.OrderType,
                moveTransaction.ProdOrdID, moveTransaction.OperationID)?.TransactionTotalMoveBaseQty ?? 0m;

            foreach (var prodOper in operations)
            {
                //Only operations at or before the transaction operation
                if (prodOper.OperationGreaterThan(moveTransaction, operations))
                {
                    continue;
                }

                var processQty = tranTransactionTotalMoveBaseQty;

                if (processQty <= 0)
                {
                    continue; 
                }

                var wipAdjustment = CopyMoveTransaction(moveTransaction, prodOper);

                wipAdjustment.TranAmt = GetScrapAmount(wipAdjustment, inventoryItem, prodItem, prodOper, 
                    moveTransaction.UOM, processQty, moveTransaction);

                if (!isQuarantine && wipAdjustment.TranAmt == 0)
                {
                    continue;
                }

                list.Add(wipAdjustment);
            }
            return list;
        }

        /// <summary>
        /// Get the correct scrap amount based on the given transaction and order information
        /// </summary>
        /// <returns></returns>
        protected virtual decimal GetScrapAmount(AMMTran scrapTransaction, InventoryItem inventoryItem, AMProdItem prodItem, AMProdOper prodOper, string tranUom, decimal remainingQty, AMMTran sourceTran)
        {
            if (prodItem?.CostMethod == null)
            {
                throw new PXArgumentException(nameof(prodItem));
            }

            if (prodOper?.OperationID == null)
            {
                throw new PXArgumentException(nameof(prodOper));
            }

            if (scrapTransaction == null || scrapTransaction.QtyScrapped.GetValueOrDefault() == 0 || remainingQty == 0)
            {
                return 0m;
            }

            if (prodItem.CostMethod == CostMethod.Actual)
            {
                if (UnreleasedWipByOperation == null && sourceTran != null)
                {
                    LoadUnreleasedWipByOperation(this, sourceTran);
                }

                return GetEstimatedWipBalance(prodOper) / remainingQty *
                       scrapTransaction.QtyScrapped.GetValueOrDefault();
            }

            ProductionCostCalculator.GetEstimatedUnitCostByOperation(this, inventoryItem, prodItem,
                prodOper, remainingQty, tranUom, 0m, scrapTransaction.QtyScrapped.GetValueOrDefault(),
                out var operExpectedCost);

            var expectedUnitCost = prodOper.TotalQty.GetValueOrDefault() == 0
                ? 0m
                : operExpectedCost / prodOper.TotalQty.GetValueOrDefault();

            return expectedUnitCost * scrapTransaction.QtyScrapped.GetValueOrDefault();
        }

        protected virtual decimal GetEstimatedWipBalance(AMProdOper prodOper)
        {
            return prodOper.WIPTotal.GetValueOrDefault() + GetUnreleasedWipByOperation(prodOper) - prodOper.WIPComp.GetValueOrDefault();
        }

        protected virtual AMMTran CopyMoveTransaction(AMMTran moveTransaction, AMProdOper amProdOper)
        {
            var wipAdjustment = (AMMTran)transactions.Cache.CreateCopy(moveTransaction);
            wipAdjustment.DocType = null;
            wipAdjustment.BatNbr = null;
            wipAdjustment.LineNbr = null;
            wipAdjustment.GLBatNbr = null;
            wipAdjustment.GLLineNbr = null;
            wipAdjustment.INDocType = null;
            wipAdjustment.INBatNbr = null;
            wipAdjustment.INLineNbr = null;
            wipAdjustment.NoteID = null;

            wipAdjustment.OperationID = amProdOper.OperationID;
            wipAdjustment.Qty = 0m;
            wipAdjustment.OrigDocType = moveTransaction.DocType;
            wipAdjustment.OrigBatNbr = moveTransaction.BatNbr;
            wipAdjustment.OrigLineNbr = moveTransaction.LineNbr;
            wipAdjustment.LaborTime = 0;
            wipAdjustment.UnitCost = 0m;
            wipAdjustment.TranAmt = 0m;
            wipAdjustment.InvtMult = (short)0;
            wipAdjustment.QtyScrapped = moveTransaction.IsScrap.GetValueOrDefault() ? moveTransaction.Qty : moveTransaction.QtyScrapped;
            wipAdjustment.TranType = moveTransaction.ScrapAction == ScrapAction.Quarantine
                ? AMTranType.ScrapQuarantine 
                : AMTranType.ScrapWriteOff;
            wipAdjustment.TranDesc = Messages.GetLocal(Messages.ForOperation, AMTranType.GetTranDescription(wipAdjustment.TranType), amProdOper.OperationCD);
            return wipAdjustment;
        }

        /// <summary>
        /// Cache related unreleased transaction totals by production operation linked to the given BATCH for use later
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="ammTran"></param>
        protected virtual void LoadUnreleasedWipByOperation(PXGraph graph, AMMTran ammTran)
        {
            UnreleasedWipByOperation = ProductionTransactionHelper.GetUnreleasedWipByOperation(graph, ammTran.DocType, ammTran.BatNbr);
        }

        /// <summary>
        /// Safe lookup to UnreleasedWipByOperation for an unrelased production WIP amount by operation
        /// </summary>
        /// <param name="prodOper"></param>
        /// <returns></returns>
        protected virtual decimal GetUnreleasedWipByOperation(AMProdOper prodOper)
        {
            if (UnreleasedWipByOperation != null &&
                UnreleasedWipByOperation.TryGetValue(prodOper.JoinKeys(), out var keyValue))
            {
                return keyValue;
            }

            return 0m;
        }

        #region AMBatchSimpleEntryBase members

        public override PXSelectBase<AMBatch> AMBatchDataMember => batch;
        public override PXSelectBase<AMMTran> AMMTranDataMember => transactions;

        #endregion
    }
}
