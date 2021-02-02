using System;
using PX.Data;
using PX.Objects.IN;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class MaterialEntry : AMBatchEntryBase
    {
        public PXSelect<AMBatch, Where<AMBatch.docType, Equal<AMDocType.material>>> batch;
        [PXImport(typeof(AMBatch))]
        public PXSelect<AMMTran, Where<AMMTran.docType, Equal<Current<AMBatch.docType>>, And<AMMTran.batNbr, Equal<Current<AMBatch.batNbr>>>>> transactions;
            
        [PXCopyPasteHiddenView]
        public PXSelect<AMMTranSplit, Where<AMMTranSplit.docType, Equal<Current<AMMTran.docType>>, And<AMMTranSplit.batNbr, Equal<Current<AMMTran.batNbr>>, And<AMMTranSplit.lineNbr, Equal<Current<AMMTran.lineNbr>>>>>> splits;
        public LSAMMaterial lsselect;

        //for cache attached only
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<AMProdMatl> ProdMaterial;

        private bool _skipReleasedReferenceDocsCheck;
        internal bool IsInternalQtySet;
        internal bool IsInternalCall;

        #region Buttons
        
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

            PXLongOperation.StartOperation(this,
                delegate()
                {
                    AMDocumentRelease.ReleaseDoc(list);
                });

            return list;
        }

        public PXAction<AMBatch> ReleaseMatlWizard;
        [PXUIField(DisplayName = Messages.Wizard, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageUrl = "~/Icons/Menu/process_16.gif", DisabledImageUrl = "~/Icons/Menu/process_16D.gif")]
        public virtual IEnumerable releaseMatlWizard(PXAdapter adapter)
        {
            throw new PXRedirectRequiredException(PXGraph.CreateInstance<MatlWizard1>(), "Material Wizard 1");
        }

        #endregion
        public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);

            this.Caches.AddCacheMapping(typeof(INLotSerialStatus), typeof(INLotSerialStatus));
        }

        public override void Persist()
        {
            if (ReferenceDeleteGraph.ContainsDeletes<AMBatch, AMMTran>(this))
            {
                using (var ts = new PXTransactionScope())
                {
                    ReferenceDeleteGraph.DeleteReferenceTransactions(this);
                    base.Persist();
                    ts.Complete();
                }

                return;
            }

            base.Persist();
        }

        #region AMBatch Events
        protected virtual void AMBatch_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Material;
        }

        protected virtual void AMBatch_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMBatch)e.Row;
            if (row == null)
            {
                return;
            }

            _skipReleasedReferenceDocsCheck = true;

            if (string.IsNullOrWhiteSpace(row.OrigBatNbr) && ReferenceDeleteGraph.HasReleasedReferenceDocs(this, row))
            {
                //Have the user confirm the delete when released references exist
                if (batch.Ask(Messages.ConfirmDeleteTitle, Messages.ConfirmReleasedBatchExist, MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                PXTrace.WriteInformation($"AMBatch:{row.DocType}:{row.BatNbr} - {Messages.ConfirmReleasedBatchExist} - Answered Yes");
            }
        }

        protected virtual void AMBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            AMBatch ambatch = (AMBatch)e.Row;

            if (ambatch == null)
            {
                return;
            }

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

                if (PXCurrencyAttribute.IsNullOrEmpty(((AMBatch)e.Row).TotalQty) == false)
                {
                    sender.SetValue<AMBatch.controlQty>(e.Row, ((AMBatch)e.Row).TotalQty);
                }
                else
                {
                    sender.SetValue<AMBatch.controlQty>(e.Row, 0m);
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

                    if (((AMBatch)e.Row).TotalQty != ((AMBatch)e.Row).ControlQty)
                    {
                        sender.RaiseExceptionHandling<AMBatch.controlQty>(e.Row, ((AMBatch)e.Row).ControlQty, new PXSetPropertyException(PX.Objects.IN.Messages.DocumentOutOfBalance));
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<AMBatch.controlQty>(e.Row, ((AMBatch)e.Row).ControlQty, null);
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

            release.SetEnabled(e.Row != null && ((AMBatch) e.Row).Hold == false && editablebatch);

            sender.AllowInsert = true;
            sender.AllowUpdate = editablebatch;
            sender.AllowDelete = editablebatch;

            transactions.Cache.AllowInsert =
                transactions.Cache.AllowUpdate =
                    transactions.Cache.AllowDelete = editablebatch;

            splits.Cache.AllowInsert =
                splits.Cache.AllowUpdate =
                    splits.Cache.AllowDelete = editablebatch;

            PXUIFieldAttribute.SetVisible<AMBatch.controlQty>(sender, e.Row, (ampsetup.Current.RequireControlTotal ?? false)); 
            PXUIFieldAttribute.SetVisible<AMBatch.controlAmount>(sender, e.Row, (ampsetup.Current.RequireControlTotal ?? false));
            PXUIFieldAttribute.SetEnabled<AMBatch.status>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<AMBatch.hold>(sender, e.Row, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.finPeriodID>(sender, e.Row, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlQty>(sender, e.Row, (ampsetup.Current.RequireControlTotal ?? false) && editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlAmount>(sender, e.Row, (ampsetup.Current.RequireControlTotal ?? false) && editablebatch);
            
            _skipReleasedReferenceDocsCheck = false;
        }

        #endregion

        #region AMMTran Events

        protected virtual void AMMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;
            if (ammTran == null)
            {
                return;
            }

            bool unitCostEnabled = ammTran.TranType == AMTranType.Receipt && ammTran.IsByproduct.GetValueOrDefault() &&
                                   ammTran.Qty.GetValueOrDefault() < 0 && !ammTran.Released.GetValueOrDefault();
            PXUIFieldAttribute.SetEnabled<AMMTran.unitCost>(sender, e.Row, unitCostEnabled);

            //Reference only exists if batch release attempted but not completed (user needs to delete the line and readd new line)
            if (ammTran.HasReference ?? false)
            {
                //Disable all fields
                PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
            }

        }

        protected virtual void AMMTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
            {
                return;
            }

            //Only prompt when a non referenced batch
            if (batch.Current != null && string.IsNullOrWhiteSpace(batch.Current.OrigBatNbr)
                && row.DocType == batch.Current.DocType && row.BatNbr == batch.Current.BatNbr
                && !_skipReleasedReferenceDocsCheck
                && ReferenceDeleteGraph.HasReleasedReferenceDocs(this, row))
            {
                //Have the user confirm the delete when released references exist
                if (transactions.Ask(Messages.ConfirmDeleteTitle, Messages.ConfirmReleasedTransactionsExist, MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                PXTrace.WriteInformation($"AMMTran:{row.DocType}:{row.BatNbr}:{row.LineNbr} - {Messages.ConfirmReleasedTransactionsExist} - Answered Yes");
            }
        }

        protected virtual void _(Events.FieldUpdated<AMMTran, AMMTran.operationID> e)
        {
            if (e.Row?.OperationID == null || e.Row.InventoryID == null || IsInternalCall)
            {
                return;
            }

            SetItemFields(e.Cache, e.Row);
            object newValue = e.Row.InventoryID;
            e.Cache.RaiseFieldVerifying<AMMTran.inventoryID>(e.Row, ref newValue);
        }

        protected virtual void AMMTran_ProdOrdID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var ammtran = (AMMTran)e.Row;
            if (ammtran == null)
            {
                return;
            }

            cache.SetDefaultExt<AMMTran.wIPAcctID>(e.Row);
            cache.SetDefaultExt<AMMTran.wIPSubID>(e.Row);
        }

        protected virtual AMProdMatl GetRelatedProdMatl(AMMTran ammTran, decimal qtyToProduce)
        {
            if (ammTran == null
                || string.IsNullOrWhiteSpace(ammTran.ProdOrdID)
                || ammTran.OperationID == null
                || ammTran.InventoryID == null)
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
                    >.Select(this, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.InventoryID, ammTran.SubItemID))
            {
                if (prodMatl == null)
                {
                    prodMatl = row;
                }

                var totalRequiredQty = row.GetTotalReqQty(qtyToProduce);
                 
                decimal newQty = totalRequiredQty - row.QtyActual.GetValueOrDefault();

                if (newQty > 0)
                {
                    //return the first row with available qty to issue
                    return row;
                }
            }
            return prodMatl;
        }

        protected virtual void AMMTran_InventoryID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var row = (AMMTran) e.Row;
            if (row == null)
            {
                return;
            }

            var orderType = (AMOrderType)PXSelectorAttribute.Select<AMMTran.orderType>(cache, row);
            if (orderType == null || CanSkipIsInventoryOnProductionOrderCheck(cache, row, orderType) || IsInventoryOnProductionOrder(cache, Convert.ToInt32(e.NewValue), row.OrderType, row.ProdOrdID))
            {
                return;
            }

            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.NewValue);

            var errorLevel = orderType.IssueMaterialOnTheFly == SetupMessage.ErrorMsg
                ? PXErrorLevel.Error
                : PXErrorLevel.Warning;

            var setPropertyEx = new PXSetPropertyException(Messages.ItemNotOnProductionOrder,
                errorLevel,
                item?.InventoryCD, row.OrderType, row.ProdOrdID)
            {
                ErrorValue = item?.InventoryCD
            };

            if (errorLevel == PXErrorLevel.Error)
            {
                throw setPropertyEx;
            }

            cache.RaiseExceptionHandling<AMMTran.inventoryID>(row, row.InventoryID, setPropertyEx);
        }

        protected virtual bool CanSkipIsInventoryOnProductionOrderCheck(PXCache cache, AMMTran ammTran, AMOrderType orderType)
        {
            return IsInternalCall || ProductionTransactionHelper.CanSkipOrderTypeCheck<AMOrderType.issueMaterialOnTheFly>(cache.Graph, orderType);
        }

        protected virtual bool IsInventoryOnProductionOrder(PXCache cache, int? inventoryId, string orderType, string prodOrdId)
        {
            return ProductionTransactionHelper.IsInventoryOnProductionOrder(cache.Graph, inventoryId, orderType, prodOrdId);
        }

        protected virtual void AMMTran_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var ammtran = (AMMTran)e.Row;
            if (ammtran == null || IsInternalCall)
            {
                return;
            }

            if (ammtran.InventoryID != null && ammtran.InventoryID != (int?)e.OldValue)
            {
                //Reset these fields based on change of Inventory
                cache.SetDefaultExt<AMMTran.qty>(ammtran);
                cache.SetDefaultExt<AMMTran.uOM>(ammtran);
                cache.SetDefaultExt<AMMTran.subItemID>(ammtran);
            }

            if (!string.IsNullOrWhiteSpace(ammtran.ProdOrdID) && ammtran.OperationID != null && ammtran.InventoryID != null)
            {
                SetItemFields(cache, ammtran);
            }
        }

        protected virtual void AMMTran_SubItemID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var ammtran = (AMMTran)e.Row;
            if (ammtran == null
                || !InventoryHelper.SubItemFeatureEnabled
                || string.IsNullOrWhiteSpace(ammtran.ProdOrdID)
                || ammtran.OperationID == null
                || ammtran.InventoryID == null
                || ammtran.SubItemID == null
                || IsInternalCall)
            {
                return;
            }

            SetItemFields(cache, ammtran);
        }

        protected virtual void AMMTran_SiteId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitCost(sender, e);
        }

        protected virtual void AMMTran_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Material;
        }

        [PXDBString(3, IsFixed = true)]
        [PXDefault(AMTranType.Issue)]
        [PXUIField(Enabled = false, Visible = false)]
        protected virtual void AMMTran_TranType_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Material Line ID", Visible = false)]
        [PXSelector(typeof(Search<AMProdMatl.lineID, 
            Where<AMProdMatl.orderType, Equal<Current<AMMTran.orderType>>,
                And<AMProdMatl.prodOrdID, Equal<Current<AMMTran.prodOrdID>>,
                And<AMProdMatl.operationID, Equal<Current<AMMTran.operationID>>,
                And<AMProdMatl.inventoryID, Equal<Current<AMMTran.inventoryID>>,
                And<Where<AMProdMatl.subItemID, Equal<Current<AMMTran.subItemID>>,
                    Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>>>>>>>),
            typeof(AMProdMatl.lineNbr),
            typeof(AMProdMatl.inventoryID),
            typeof(AMProdMatl.subItemID),
            typeof(AMProdMatl.statusID),
            typeof(AMProdMatl.qtyRemaining),
            typeof(AMProdMatl.uOM),
            typeof(AMProdMatl.descr))]
        protected virtual void AMMTran_MatlLineId_CacheAttached(PXCache sender)
        {
        }

        //Adding this supports the LineID on the MatlLineID selector
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
        protected virtual void AMProdMatl_LineID_CacheAttached(PXCache sender)
        {
        }

        [MfgLocationAvail(typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID), typeof(AMMTran.siteID),
            IsSalesType: typeof(Where<AMMTran.tranType, Equal<AMTranType.issue>>),
            IsReceiptType: typeof(Where<AMMTran.tranType, Equal<AMTranType.receipt>, Or<AMMTran.tranType, Equal<AMTranType._return>>>))]
        [PXFormula(typeof(Validate<AMMTran.tranType>))]
        protected virtual void AMMTran_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [MfgLocationAvail(typeof(AMMTranSplit.inventoryID), typeof(AMMTranSplit.subItemID), typeof(AMMTranSplit.siteID),
            IsSalesType: typeof(Where<Parent<AMMTran.tranType>, Equal<AMTranType.issue>>),
            IsReceiptType: typeof(Where<Parent<AMMTran.tranType>, Equal<AMTranType.receipt>, Or<Parent<AMMTran.tranType>, Equal<AMTranType._return>>>))]
        protected virtual void AMMTranSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.disassemble>>), Messages.IncorrectOrderTypeFunction)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void AMMTran_OrderType_CacheAttached(PXCache sender)
        {
        }

        protected virtual void AMMTranSplit_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Material;
        }

        protected virtual void AMMTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;

            if (ammTran == null)
            {
                return;
            }

            if (!ammTran.IsStockItem.GetValueOrDefault(true))
            {
                e.NewValue = 0;
                return;
            }

            e.NewValue = AMTranType.InvtMult(ammTran.TranType ?? AMTranType.Issue);
        }

        protected virtual void AMMTranSplit_InvtMult_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            AMMTranSplit row = (AMMTranSplit)e.Row;

            if (row == null)
            {
                return;
            }

            PXCache parentCache = sender.Graph.Caches[typeof(AMMTran)];
            if (parentCache == null || parentCache.Current == null)
            {
                return;
            }

            AMMTran parentRow = (AMMTran)parentCache.Current;

            if (row.LineNbr == parentRow.LineNbr
                && row.TranType.EqualsWithTrim(parentRow.TranType)
                && parentRow.IsByproduct.GetValueOrDefault())
            {
                var expectedValue =  AMTranType.InvtMult(row.TranType ?? AMTranType.Issue);
                if (expectedValue.GetValueOrDefault() != row.InvtMult.GetValueOrDefault() &&
                    row.InvtMult.GetValueOrDefault() != 0)
                {
                    //Force value as LSSelect thinkgs this is a reverse but it is not for byproduct
                    sender.SetValue<AMMTranSplit.invtMult>(row, expectedValue);
                }
            }
        }

        protected virtual void AMMTran_IsByproduct_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null || row.IsByproduct.GetValueOrDefault() == Convert.ToBoolean(e.OldValue ?? false))
            {
                return;
            }

            SetTranTypeInvtMult(row, sender);
        }

        protected virtual void AMMTran_MatlLineId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if(IsSetItemFields || IsInternalCall)
            {
                return;
            }

            if (((AMMTran) e.Row)?.MatlLineId == null)
            {
                sender.SetValueExt<AMMTran.isByproduct>(e.Row, ((AMMTran)e.Row)?.Qty.GetValueOrDefault() < 0);
                return;
            }

            SetItemFields(sender, (AMMTran)e.Row);
        }

        protected virtual void AMMTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
            {
                return;
            }

            var qtySignsChanged = ProductionTransactionHelper.SignsChanged(row.Qty, e.OldValue);

            if (qtySignsChanged && row.MatlLineId == null)
            {
                sender.SetValueExt<AMMTran.isByproduct>(row, row.Qty.GetValueOrDefault() < 0);
            }
            else
            {
                SetTranTypeInvtMult(row, sender);
            }
            
            if (row.ProdOrdID == null || row.OperationID == null || row.InventoryID == null)
            {
                return;
            }

            var isByproductReturn = row.IsByproduct.GetValueOrDefault() && row.Qty.GetValueOrDefault() > 0;

            //need to handle returns and update the unit cost accordingly
            if (qtySignsChanged || row.Qty.GetValueOrDefault() < 0 ||
                (Convert.ToDecimal(e.OldValue) == 0m && isByproductReturn))
            {
                //the code further below looks for a regular by product (neg qty) to get production unit cost
                // we will use neg qty for both regular returns and byproduct cost lookup
                if (row.Qty.GetValueOrDefault() < 0 || isByproductReturn)
                {
                    //Return entry - find the unit cost to use
                    
                    //First check if standard cost item
                    var item = InventoryItem.PK.Find(this, row.InventoryID);

                    if (item != null && item.ValMethod == INValMethod.Standard)
                    {
                        if (isByproductReturn || !row.IsByproduct.GetValueOrDefault())
                        {
                            DeleteLotSerialSplits(sender, row);
                        }
                        // let the default on unit cost get the standard cost. 
                        //  The default should already be loaded in the row - nothing to lookup here
                        return;
                    }

                    PXResultset<AMProdMatl> relatedMaterial = PXSelect<AMProdMatl,
                        Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                            And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                                And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                                    And<AMProdMatl.inventoryID, Equal<Required<AMProdMatl.inventoryID>>,
                                        And<IsNull<AMProdMatl.subItemID, int0>, Equal<Required<AMProdMatl.subItemID>>>>>>>
                    >.Select(this, row.OrderType, row.ProdOrdID, row.OperationID, row.InventoryID, row.SubItemID.GetValueOrDefault());

                    AMProdMatl byproductMaterial = null;
                    AMProdMatl foundMaterial = null;
                    if (relatedMaterial != null)
                    {
                        foreach (AMProdMatl material in relatedMaterial)
                        {
                            if (material.IsByproduct.GetValueOrDefault())
                            {
                                byproductMaterial = material;
                                continue;
                            }

                            foundMaterial = material;
                        }
                    }

                    //Second check if byproduct
                    if (byproductMaterial != null)
                    {
                        //This indicates a byproduct - use the bom for the source of unit cost
                        sender.SetValueExt<AMMTran.unitCost>(row, byproductMaterial.UnitCost);
                        DefaultUnitCost(sender, e);
                        return;
                    }

                    //Last check: get a value based on what was already issued (if an existing item). If the item is added as an on the fly by product we will skip this check
                    if (foundMaterial != null)
                    {
                        var returningUnitCost = ProductionTransactionHelper.GetMaterialReturnUnitCost(this, row);
                        if (returningUnitCost != null)
                        {
                            sender.SetValueExt<AMMTran.unitCost>(row, returningUnitCost);
                        }

                        if (qtySignsChanged)
                        {
                            DeleteLotSerialSplits(sender, row);
                        }

                        return;
                    }
                    
                    // at this point we have an on the fly by product...
                    sender.SetValueExt<AMMTran.isByproduct>(row, true);
                }

                //normal/positive entry
                sender.SetDefaultExt<AMMTran.unitCost>(row);
                DefaultUnitCost(sender, e);
            }
        }

        protected virtual void DeleteLotSerialSplits(PXCache sender, AMMTran row)
        {
            if (row == null || sender.Graph.IsCopyPasteContext)
            {
                return;
            }

            var splitCache = sender.Graph.Caches[typeof(AMMTranSplit)];

            foreach (AMMTranSplit split in PXSelect<
                AMMTranSplit,
                Where<AMMTranSplit.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTranSplit.batNbr, Equal<Required<AMMTran.batNbr>>,
                        And<AMMTranSplit.lineNbr, Equal<Required<AMMTran.lineNbr>>>>>>.Select(sender.Graph, row.DocType, row.BatNbr, row.LineNbr))
            {
                if (string.IsNullOrWhiteSpace(split?.LotSerialNbr))
                {
                    continue;
                }

                splitCache.Delete(split);
            }
        }

        protected virtual void AMMTran_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMMTran) e.Row;
            if (row?.ProdOrdID == null || row.OperationID == null || row.InventoryID == null)
            {
                return;
            }

            if (!CheckOverIssueMaterialOnEntry(sender, row, Convert.ToDecimal(e.NewValue)))
            {
                return;
            }
            e.NewValue = row.Qty.GetValueOrDefault();
            e.Cancel = true;
        }

        protected virtual void AMMTranSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMMTranSplit)e.Row;
            if (row?.InventoryID == null)
            {
                return;
            }

            if (!CheckOverIssueMaterialOnEntry(sender, row, Convert.ToDecimal(e.NewValue)))
            {
                return;
            }
            e.NewValue = row.Qty.GetValueOrDefault();
            e.Cancel = true;
        }

        protected virtual void AMMTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitCost(sender,e);
        }

        protected virtual void AMMTran_TranType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            SetTranTypeInvtMult((AMMTran)e.Row, cache);
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(AMSubcontractSource.None)]
        protected virtual void AMMTran_SubcontractSource_CacheAttached(PXCache sender)
        {
        }

        #endregion

        protected bool IsSetItemFields;

        protected virtual void SetItemFields(PXCache cache, AMMTran ammTran)
        {
            var item = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(cache, ammTran);
            if (item != null)
            {
                SetItemFields(cache, ammTran, item);
            }
        }

        protected virtual void SetItemFields(PXCache cache, AMMTran ammTran, AMProdItem amProdItem)
        {
            var amprodmatl = GetRelatedProdMatl(ammTran, amProdItem.BaseQtytoProd.GetValueOrDefault());
            if (amprodmatl == null)
            {
                //cache.SetDefaultExt<AMMTran.isByproduct>(ammTran);
                cache.SetDefaultExt<AMMTran.matlLineId>(ammTran);
                return;
            }

            try
            {
                IsSetItemFields = true;

                //must set these fields before qty to make sure invtmult/trantypes/qty checks/etc. get set/query correctly
                cache.SetValueExt<AMMTran.isByproduct>(ammTran, amprodmatl.IsByproduct);
                cache.SetValueExt<AMMTran.matlLineId>(ammTran, amprodmatl.LineID);

            // Set the SubcontractSource
            cache.SetValueExt<AMMTran.subcontractSource>(ammTran, amprodmatl.SubcontractSource);

            if (!IsImport && !IsContractBasedAPI)
            {
                IsInternalQtySet = true;
                cache.SetValueExt<AMMTran.qty>(ammTran, amprodmatl.QtyRemaining.GetValueOrDefault());
                IsInternalQtySet = false;
            }

                cache.SetValueExt<AMMTran.uOM>(ammTran, amprodmatl.UOM);

                //Do not use SetValueExt for subitem
                cache.SetValue<AMMTran.subItemID>(ammTran, amprodmatl.SubItemID);
                if (!ammTran.SubItemID.Equals(amprodmatl.SubItemID))
                {
                    object newValue = amprodmatl.SubItemID;
                    cache.RaiseFieldVerifying<AMMTran.subItemID>(ammTran, ref newValue);
                }
            
                if (amprodmatl.SiteID == null || IsImport || IsContractBasedAPI)
                {
                    return;
                }
                cache.SetValueExt<AMMTran.siteID>(ammTran, amprodmatl.SiteID);
                cache.SetValueExt<AMMTran.locationID>(ammTran, amprodmatl.LocationID);
            }
            finally
            {
                IsSetItemFields = false;
            }
        }

        protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            object MatlUnitCost;
            var ammTran = (AMMTran) e.Row;
            if (ammTran == null)
            {
                return;
            }
            sender.RaiseFieldDefaulting<AMMTran.unitCost>(e.Row, out MatlUnitCost);

            if (MatlUnitCost != null && (decimal)MatlUnitCost != 0m)
            {
                try
                {
                    var matlUnitCost = INUnitAttribute.ConvertToBase<AMMTran.inventoryID>(sender, e.Row, ammTran.UOM, (decimal)MatlUnitCost, INPrecision.UNITCOST);
                    sender.SetValueExt<AMMTran.unitCost>(e.Row, matlUnitCost);
                }
                catch (PXUnitConversionException)
                {
                    sender.SetValueExt<AMMTran.unitCost>(e.Row, MatlUnitCost);
                }
            }
        }

        protected void SetTranTypeInvtMult(AMMTran ammTran, PXCache sender)
        {
            if (ammTran == null)
            {
                return;
            }

            string tranType = ammTran.Qty.GetValueOrDefault() < 0
                ? ammTran.IsByproduct.GetValueOrDefault() ? AMTranType.Receipt : AMTranType.Return
                : AMTranType.Issue;
            sender.SetValueExt<AMMTran.tranType>(ammTran, tranType);

            if (!ammTran.IsStockItem.GetValueOrDefault(true))
            {
                sender.SetValueExt<AMMTran.invtMult>(ammTran, (short)0);
                return;
            }

            sender.SetValueExt<AMMTran.invtMult>(ammTran, AMTranType.InvtMult(tranType));
        }

        /// <summary>
        /// Checks for Over Issue of Material for a given material entry.
        /// If over issue found related to check level. (given cache to call RaiseExceptionHandling when condition is met).
        /// </summary>
        /// <returns>True when the condition is an error</returns>
        protected virtual bool CheckOverIssueMaterialOnEntry(PXCache sender, AMMTran ammTran, decimal? newQty)
        {
            if (ammTran?.ProdOrdID == null || ammTran.OperationID == null || ammTran.InventoryID == null || IsInternalCall ||
                !ProductionTransactionHelper.TryCheckOverIssueMaterialSetPropertyException(sender, ammTran,
                    newQty, out var setPropertyException))
            {
                return false;
            }
            
            if (!IsInternalQtySet)
            {
                sender.RaiseExceptionHandling<AMMTran.qty>(
                    ammTran,
                    ammTran.Qty,
                    setPropertyException);
            }

            return setPropertyException?.ErrorLevel == PXErrorLevel.Error || setPropertyException?.ErrorLevel == PXErrorLevel.RowError;
        }

        /// <summary>
        /// Checks for Over Issue of Material for a given material entry.
        /// If over issue found related to check level. (given cache to call RaiseExceptionHandling when condition is met).
        /// </summary>
        /// <returns>True when the condition is an error</returns>
        protected virtual bool CheckOverIssueMaterialOnEntry(PXCache sender, AMMTranSplit ammTranSplit, decimal? newSplitRowQty)
        {
            if (ammTranSplit == null || newSplitRowQty.GetValueOrDefault() == 0 || IsInternalCall)
            {
                return false;
            }

            var ammTran = transactions.Current;
            if (ammTran == null || ammTran.LineNbr != ammTranSplit.LineNbr)
            {
                ammTran = (AMMTran) PXParentAttribute.SelectParent(sender, ammTranSplit, typeof(AMMTran));
            }
            ammTran = sender.Graph.Caches<AMMTran>().LocateElse(ammTran);
            if (ammTran?.ProdOrdID == null || ammTran.OperationID == null || ammTran.InventoryID == null)
            {
                return false;
            }

            if (ProductionTransactionHelper.CanSkipOrderTypeCheck<AMOrderType.overIssueMaterial>(sender.Graph,
                (AMOrderType)PXSelectorAttribute.Select<AMMTran.orderType>(sender.Graph.Caches<AMMTran>(), ammTran)))
            {
                return false;
            }

            var newQty = GetTotalSplitQty(sender, ammTran, ammTranSplit, newSplitRowQty);

            if (!ProductionTransactionHelper.TryCheckOverIssueMaterialSetPropertyException(sender, ammTran,
                    newQty, out var setPropertyException))
            {
                return false;
            }

            sender.RaiseExceptionHandling<AMMTranSplit.qty>(
                ammTranSplit,
                ammTranSplit.Qty,
                setPropertyException);

            return setPropertyException?.ErrorLevel == PXErrorLevel.Error || setPropertyException?.ErrorLevel == PXErrorLevel.RowError;
        }

        /// <summary>
        /// Find the total split qty for the given transaction line
        /// </summary>
        protected static decimal GetTotalSplitQty(PXCache tranSplitCache, AMMTran ammTran, AMMTranSplit ammTranSplit, decimal? newSplitRowQty)
        {
            if (ammTran?.LineNbr == null || ammTranSplit?.LineNbr == null)
            {
                return 0;
            }

            var totalQty = 0m;
            foreach (AMMTranSplit split in PXSelect<AMMTranSplit,
                Where<AMMTranSplit.docType, Equal<Current<AMMTran.docType>>,
                    And<AMMTranSplit.batNbr, Equal<Current<AMMTran.batNbr>>,
                        And<AMMTranSplit.lineNbr, Equal<Current<AMMTran.lineNbr>>>>>>.SelectMultiBound(tranSplitCache.Graph, new object[] { ammTran }))
            {
                var splitRow = tranSplitCache.LocateElse(split);
                if (splitRow == null)
                {
                    continue;
                }

                if (splitRow.SplitLineNbr == ammTranSplit.SplitLineNbr)
                {
                    totalQty += newSplitRowQty.GetValueOrDefault();
                    continue;
                }

                totalQty += splitRow.Qty.GetValueOrDefault();
            }

            return totalQty;
        }

        #region InsertAMMTran

        public static AMMTran InsertAMMTran(MaterialEntry releaseMatlGraph, AMMTran newAmmTran)
        {
            return InsertAMMTran(releaseMatlGraph, newAmmTran, null);
        }

        public static AMMTran InsertAMMTran(MaterialEntry releaseMatlGraph, AMMTran newAmmTran, AMMTran origAmmTran)
        {
            if (releaseMatlGraph == null)
            {
                throw new ArgumentNullException(nameof(releaseMatlGraph));
            }

            return releaseMatlGraph.InsertNewTransaction(newAmmTran, null, origAmmTran);
        }

        /// <summary>
        /// Insert Release Material Transaction lines
        /// </summary>
        public virtual AMMTran InsertNewTransaction(AMMTran newAmmTran, List<AMMTranSplit> newAmmTranSplits, AMMTran origAmmTran)
        {
            if (newAmmTran == null)
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                        Common.Cache.GetCacheName(typeof(AMMTran))));
            }

            if (newAmmTran.Qty.GetValueOrDefault() == 0 || newAmmTran.InventoryID == null)
            {
                return null;
            }

            if (batch.Current == null)
            {
                batch.Insert();
            }

            if (batch.Current == null ||
                batch.Current.Released.GetValueOrDefault())
            {
                return null;
            }

            if (origAmmTran != null &&
                !string.IsNullOrWhiteSpace(origAmmTran.DocType) &&
                !string.IsNullOrWhiteSpace(origAmmTran.BatNbr) && 
                origAmmTran.LineNbr != null)
            {
                newAmmTran.OrigBatNbr = origAmmTran.BatNbr;
                newAmmTran.OrigDocType = origAmmTran.DocType;
                newAmmTran.OrigLineNbr = origAmmTran.LineNbr;
            }

            var insertBySplits = newAmmTranSplits != null && newAmmTranSplits.Count > 0;
            var isLotSerialNbrSupplied = false;
            var newAmmTranCopy = (AMMTran)transactions.Cache.CreateCopy(newAmmTran);
            var isReturn = newAmmTranCopy.Qty.GetValueOrDefault() < 0;
            var qty = newAmmTran.Qty.GetValueOrDefault();
            //Insert split with zero qty to allow splits to build up the qty
            newAmmTranCopy.Qty = 0m;
            newAmmTranCopy.BaseQty = 0m;
            newAmmTranCopy.LotSerialNbr = insertBySplits ? null : newAmmTranCopy.LotSerialNbr;

            newAmmTran = (AMMTran)transactions.Cache.CreateCopy(transactions.Insert(newAmmTranCopy));

            //We skip only if positive split entry as allocations will roll up qty.
            if (!insertBySplits || isReturn)
            {
                newAmmTran.Qty = qty;
                newAmmTran = transactions.Update(newAmmTran);
                // some times we loose the location value?
                if(newAmmTran.LocationID != newAmmTranCopy.LocationID)
                {
                    newAmmTran.LocationID = newAmmTranCopy.LocationID;
                    newAmmTran = transactions.Update(newAmmTran);
                }
            }

            if (insertBySplits)
            {
                foreach (var newAmmTranSplit in newAmmTranSplits)
                {
                    isLotSerialNbrSupplied |= !string.IsNullOrWhiteSpace(newAmmTranSplit.LotSerialNbr);
                    InsertNewTransactionSplit(newAmmTranSplit);
                }

                newAmmTran = (AMMTran)transactions.Cache.CreateCopy(transactions.Current);
            }

            if (!isReturn || isLotSerialNbrSupplied)
            {
                // When lot/serial supplied we do not want the below return code filling in values as these values were already determined.
                return newAmmTran;
            }

            #region Handle Returns / ByProducts
            // ----------------------------------------------------------------------------------------------------
            //  Handle returns and adding in the lot/serial information including the correct unit cost to use here
            // ----------------------------------------------------------------------------------------------------
            try
            {
                decimal? tranReturningQty = -1 * newAmmTran.Qty.GetValueOrDefault();
                decimal? issuedQtyRT = 0m;      // Running total of qty issued (Value does not account for returned issued qty!!)
                decimal? returnedQtyRT = 0m;    // Running total of qty returned
                decimal? tranAmountRT = 0m;     // Running total of transaction amount (related to issuedQtyRT)
                decimal? curQty = 0m;           // Manage ammtran.qty
                decimal? curQtyLotSer = 0m;     //Current lot/serial tran qty during lot/serial lookup
                decimal? curQtySpecificCostLotSer = 0m;
                decimal? LotSerQtySpecCostRT = 0m;
                decimal? LotSerQtyRT = 0m;      //Running total for lot/serial lookup
                decimal? curAmount = 0m;        // Manage ammtran.tranamt
                decimal? curUnitCost = 0m;
                var SplitQtyReturned = new AMMTranSplit();
                var isBackflush = !string.IsNullOrWhiteSpace(newAmmTran.OrigBatNbr);

                InventoryItem inventoryItem = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(
                    this, newAmmTran.InventoryID);

                if (inventoryItem != null)
                {
                    INLotSerClass lotSerClass = PXSelect<INLotSerClass,
                        Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>
                    >.Select(this, inventoryItem.LotSerClassID);

                    if (newAmmTran.IsByproduct.GetValueOrDefault() 
                        && isBackflush
                        && lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered 
                        && lotSerClass.LotSerIssueMethod != INLotSerIssueMethod.Sequential)
                    {
                        throw new PXException(Messages.ByProductBackflushLS, inventoryItem.InventoryCD.TrimIfNotNullEmpty());
                    }

                    if (!newAmmTran.IsByproduct.GetValueOrDefault())
                    {
                        //  Returns will use the most recent transactions working back in time/batch
                        //  This process will determine the cost and the lot/serial of each ammtran
                        foreach (AMMTran releasedTran in PXSelect<
                            AMMTran,
                            Where<AMMTran.docType, Equal<AMDocType.material>,
                                And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                                And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                                And<AMMTran.operationID, Equal<Required<AMMTran.operationID>>, 
                                And<AMMTran.released, Equal<boolTrue>,
                                And<AMMTran.inventoryID, Equal<Required<AMMTran.inventoryID>>,
                                And<IsNull<AMMTran.subItemID, int0>, Equal<Required<AMMTran.subItemID>>>>>>>>>, 
                            OrderBy<
                                Desc<AMMTran.tranDate, 
                                Desc<AMMTran.batNbr, 
                                Desc<AMMTran.lineNbr>>>>
                        >
                            .Select(this, newAmmTran.OrderType, newAmmTran.ProdOrdID, newAmmTran.OperationID, newAmmTran.InventoryID, newAmmTran.SubItemID.GetValueOrDefault()))
                        {
                            curQty = releasedTran.Qty.GetValueOrDefault() <= 0 ? releasedTran.Qty.GetValueOrDefault() * -1 : releasedTran.Qty.GetValueOrDefault();
                            curAmount = releasedTran.TranAmt.GetValueOrDefault() <= 0 ? releasedTran.TranAmt.GetValueOrDefault() * -1 : releasedTran.TranAmt.GetValueOrDefault();

                            if (curQty.GetValueOrDefault() != 0 && curAmount.GetValueOrDefault() != 0)
                            {
                                curUnitCost = curAmount / curQty;

                                if (releasedTran.TranType == AMTranType.Return)
                                {
                                    returnedQtyRT += curQty;
                                    curQty = 0m;
                                    curAmount = 0m;
                                }
                                else
                                {
                                    //the current issue transaction was returned (by trandate/batch desc order)
                                    //  Cannot count it because it was already returned.
                                    if (returnedQtyRT > 0)
                                    {
                                        returnedQtyRT -= curQty;

                                        //partial return, use the remaining qty as issue qty
                                        if (returnedQtyRT < 0)
                                        {
                                            curQty = Math.Abs(returnedQtyRT.GetValueOrDefault());
                                            curAmount = curUnitCost * curQty;
                                            returnedQtyRT = 0m;
                                        }
                                        else
                                        {
                                            curQty = 0m;
                                            curAmount = 0m;
                                        }
                                    }

                                    //Only part of the current transaction is required
                                    if (issuedQtyRT + curQty > tranReturningQty)
                                    {
                                        curQty = tranReturningQty - issuedQtyRT;
                                        curAmount = curUnitCost * curQty;
                                    }
                                }

                                //Loop through lot/serial and/or specific cost...
                                if (lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered && curQty > 0m)
                                {
                                    LotSerQtyRT = 0m;

                                    foreach (AMMTranSplit ammTranSplit in
                                        PXSelect<AMMTranSplit, Where<AMMTranSplit.docType, Equal<Required<AMMTranSplit.docType>>
                                            , And<AMMTranSplit.batNbr, Equal<Required<AMMTranSplit.batNbr>>
                                                , And<AMMTranSplit.lineNbr, Equal<Required<AMMTranSplit.lineNbr>>>>>>.Select(this, releasedTran.DocType, releasedTran.BatNbr, releasedTran.LineNbr))
                                    {
                                        //If null for some reason set to zero
                                        curQtyLotSer = ammTranSplit.Qty.GetValueOrDefault();

                                        //Handle referenced returned splits
                                        SplitQtyReturned = PXSelectGroupBy<AMMTranSplit, Where<AMMTranSplit.origSource, Equal<Common.moduleAM>
                                                , And<AMMTranSplit.origBatNbr, Equal<Required<AMMTranSplit.batNbr>>
                                                    , And<AMMTranSplit.origLineNbr, Equal<Required<AMMTranSplit.lineNbr>>
                                                        , And<AMMTranSplit.origSplitLineNbr, Equal<Required<AMMTranSplit.splitLineNbr>>>>>>,
                                            Aggregate<Sum<AMMTran.qty>>>.Select(this, ammTranSplit.BatNbr, ammTranSplit.LineNbr, ammTranSplit.SplitLineNbr);

                                        if (SplitQtyReturned != null)
                                        {
                                            if (SplitQtyReturned.Qty.GetValueOrDefault() != 0m)
                                            {
                                                curQtyLotSer -= SplitQtyReturned.Qty;
#if DEBUG
                                                AMDebug.TraceWriteMethodName($"!! Found a qty[{SplitQtyReturned.Qty}] for lot/serial[{ammTranSplit.LotSerialNbr}] was returned !!");
#endif
                                            }
                                        }

                                        if ((curQtyLotSer > 0m) && !string.IsNullOrEmpty(ammTranSplit.LotSerialNbr))
                                        {
                                            if (inventoryItem.ValMethod == INValMethod.Specific)
                                            {
                                                #region Returning Specific Cost
                                                //Get specific cost from loop below
                                                curAmount = 0m;

                                                LotSerQtySpecCostRT = 0m;

                                                //INTranCost when Specific cost item will have the LotSerialNbr field populated. 
                                                //  All other valmethod items (even if lot/serial) will not have LotSerialNbr data (null).
                                                foreach (INTranCost inTranCost in
                                                    PXSelect<INTranCost,
                                                        Where<INTranCost.tranType, Equal<Required<INTranCost.tranType>>
                                                            , And<INTranCost.refNbr, Equal<Required<INTranCost.refNbr>>
                                                                , And<INTranCost.lineNbr, Equal<Required<INTranCost.lineNbr>>
                                                                    , And<INTranCost.inventoryID, Equal<Required<INTranCost.inventoryID>>
                                                                        , And<INTranCost.lotSerialNbr, Equal<Required<INTranCost.lotSerialNbr>>
                                                                        >>>>>
                                                        , OrderBy<Asc<INTranCost.lotSerialNbr>>
                                                    >.Select(this, releasedTran.TranType, releasedTran.INBatNbr, releasedTran.INLineNbr, ammTranSplit.InventoryID, ammTranSplit.LotSerialNbr))
                                                {
                                                    //If null for some reason set to zero
                                                    curQtySpecificCostLotSer = inTranCost.Qty.GetValueOrDefault();

                                                    if ((curQtySpecificCostLotSer > 0m) && !string.IsNullOrEmpty(inTranCost.LotSerialNbr))
                                                    {
                                                        var newAMMTranSplit = new AMMTranSplit
                                                            {
                                                                TranType = newAmmTran.TranType,
                                                                BatNbr = newAmmTran.BatNbr,
                                                                LineNbr = newAmmTran.LineNbr,
                                                                InventoryID = newAmmTran.InventoryID,
                                                                SubItemID = newAmmTran.SubItemID,
                                                                UOM = newAmmTran.UOM,
                                                                SiteID = newAmmTran.SiteID,
                                                                LocationID = newAmmTran.LocationID
                                                            };

                                                        //This is not setting correctly anywhere else. So forcing this here
                                                        newAMMTranSplit.TranType = AMTranType.Return;
                                                        newAMMTranSplit.InvtMult = AMTranType.InvtMult(newAMMTranSplit.TranType);

                                                        curUnitCost = inTranCost.TranCost / curQtySpecificCostLotSer;

                                                        if (LotSerQtySpecCostRT + curQtySpecificCostLotSer > curQtyLotSer)
                                                        {   
                                                            //Get partial qty for this entry
                                                            curQtySpecificCostLotSer = (curQtyLotSer - LotSerQtySpecCostRT);
                                                            curAmount += curQtySpecificCostLotSer * curUnitCost;
#if DEBUG
                                                            AMDebug.TraceWriteMethodName(
                                                                string.Format("[Specific Cost][{4}][Returning Lot/Serial: {0}][*Partial* Qty: {1}][curAmount: {2}][RefNbr: {3}]"
                                                                    , inTranCost.LotSerialNbr, curQtySpecificCostLotSer, curAmount, inTranCost.RefNbr, this.batch.Current.BatNbr)
                                                            );
#endif
                                                        }
                                                        else
                                                        {   //Full qty

                                                            curAmount += inTranCost.TranCost;
#if DEBUG
                                                            AMDebug.TraceWriteMethodName(
                                                                string.Format("[Specific Cost][{4}][Returning Lot/Serial: {0}][Full Qty: {1}][curAmount: {2}][RefNbr: {3}]"
                                                                    , inTranCost.LotSerialNbr, curQtySpecificCostLotSer, curAmount, inTranCost.RefNbr, this.batch.Current.BatNbr)
                                                            );
#endif
                                                        }

                                                        newAMMTranSplit.Qty = curQtySpecificCostLotSer;
                                                        newAMMTranSplit.LotSerialNbr = inTranCost.LotSerialNbr;

                                                        //KEEP TRACK OF ORIGINAL BATCH INFO BEING RETURNED
                                                        newAMMTranSplit.OrigSource = Common.ModuleAM;
                                                        newAMMTranSplit.OrigBatNbr = ammTranSplit.BatNbr;
                                                        newAMMTranSplit.OrigLineNbr = ammTranSplit.LineNbr;
                                                        newAMMTranSplit.OrigSplitLineNbr = ammTranSplit.SplitLineNbr;

                                                        newAMMTranSplit = this.splits.Insert(newAMMTranSplit);

                                                        LotSerQtySpecCostRT += curQtySpecificCostLotSer;
                                                        if (LotSerQtySpecCostRT >= curQtyLotSer)
                                                        {
                                                            //done/stop loop - used up all the qty for the Manufacturing transaction
                                                            break;
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region All other non specific cost - stock - lot or serial items

                                                var newAMMTranSplit = new AMMTranSplit
                                                {
                                                    TranType = newAmmTran.TranType,
                                                    BatNbr = newAmmTran.BatNbr,
                                                    LineNbr = newAmmTran.LineNbr,
                                                    InventoryID = newAmmTran.InventoryID,
                                                    SubItemID = newAmmTran.SubItemID,
                                                    UOM = newAmmTran.UOM,
                                                    SiteID = newAmmTran.SiteID,
                                                    LocationID = newAmmTran.LocationID
                                                };

                                                //This is not setting correctly anywhere else. So forcing this here
                                                newAMMTranSplit.TranType = AMTranType.Return;
                                                newAMMTranSplit.InvtMult = AMTranType.InvtMult(newAMMTranSplit.TranType);

                                                if (LotSerQtyRT + curQtyLotSer > curQty)
                                                {   
                                                    //Get partial qty for this entry
#if DEBUG
                                                    AMDebug.TraceWriteMethodName(
                                                        $"[{this.batch.Current.BatNbr}][ValMethod: {inventoryItem.ValMethod}][Returning Lot/Serial: {ammTranSplit.LotSerialNbr}][*Partial* Qty: {curQtyLotSer}]"
                                                    );
#endif
                                                    curQtyLotSer = curQty - LotSerQtyRT;
                                                }
#if DEBUG
                                                else
                                                {   //Full qty
                                                    AMDebug.TraceWriteMethodName(
                                                        $"[{this.batch.Current.BatNbr}][ValMethod: {inventoryItem.ValMethod}][Returning Lot/Serial: {ammTranSplit.LotSerialNbr}][Full Qty: {curQtyLotSer}]"
                                                    );
                                                }
#endif
                                                newAMMTranSplit.Qty = curQtyLotSer;
                                                newAMMTranSplit.LotSerialNbr = ammTranSplit.LotSerialNbr;

                                                //KEEP TRACK OF ORIGINAL BATCH INFO BEING RETURNED
                                                newAMMTranSplit.OrigSource = Common.ModuleAM;
                                                newAMMTranSplit.OrigBatNbr = ammTranSplit.BatNbr;
                                                newAMMTranSplit.OrigLineNbr = ammTranSplit.LineNbr;
                                                newAMMTranSplit.OrigSplitLineNbr = ammTranSplit.SplitLineNbr;

                                                newAMMTranSplit = this.splits.Insert(newAMMTranSplit);

                                                LotSerQtyRT += curQtyLotSer;
                                                if (LotSerQtyRT >= curQty)
                                                {
                                                    //done/stop loop - used up all the qty for the Manufacturing transaction
                                                    break;
                                                }

                                                #endregion
                                            }

                                        }
                                    }
                                }

                                issuedQtyRT += curQty;
                                tranAmountRT += curAmount;

                                if (issuedQtyRT >= tranReturningQty)
                                {
                                    //End loop on trans - found all the trans/qty required for returning
                                    break;
                                }
                            }
                        }

                        if (issuedQtyRT == tranReturningQty && issuedQtyRT.GetValueOrDefault() != 0)
                        {
                            newAmmTran.UnitCost = tranAmountRT / issuedQtyRT;
                        }

                        if ((lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered)
                            && (this.splits.Current.BatNbr == newAmmTran.BatNbr)
                            && (this.splits.Current.LineNbr == newAmmTran.LineNbr)
                            && (this.splits.Current.InventoryID == newAmmTran.InventoryID))
                        {
                            //Not lot number item - make sure the split is still setup correctly
                            //Make sure the tran type and invtmult are set correctly
                            this.splits.Current.TranType = AMTranType.Return;
                            this.splits.Current.InvtMult = AMTranType.InvtMult(AMTranType.Return);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                if (IsImport)
                {
                    PXTrace.WriteError($"Error received during material import: {e.Message}");
                    throw;
                }

                PXTrace.WriteWarning(e);
#if DEBUG
                AMDebug.TraceWriteLine("----------------[ #### Exception in tran return lookup ####]----------------");
                AMDebug.TraceWriteMethodName(e.Source);
                AMDebug.TraceWriteMethodName(e.Message);
                AMDebug.TraceWriteLine("----------------[ #### Exception in tran return lookup ####]----------------");
#endif
            }

            //  UPDATE AMMTRAN
            //  Make sure unit cost is updated (Returns only)
            return this.transactions.Update(newAmmTran);

            //End of returns
            #endregion
        }

        protected virtual AMMTranSplit InsertNewTransactionSplit(AMMTranSplit tranSplit)
        {
            if (tranSplit == null)
            {
                return null;
            }

            if (transactions?.Current == null)
            {
                throw new PXArgumentException(nameof(transactions));
            }

            var newSplit = splits.Insert();
            newSplit.LocationID = tranSplit.LocationID;
            newSplit.LotSerialNbr = tranSplit.LotSerialNbr;
            newSplit.ExpireDate = tranSplit.ExpireDate;
            // Allocations are always in the base UOM (use base qty is correct)
            newSplit.Qty = tranSplit.BaseQty;
            newSplit.OrigSource = tranSplit.OrigSource;
            newSplit.OrigBatNbr = tranSplit.OrigBatNbr;
            newSplit.OrigLineNbr = tranSplit.OrigLineNbr;
            newSplit.OrigSplitLineNbr = tranSplit.OrigSplitLineNbr;
            return splits.Update(newSplit);
        }

        #endregion

        #region AMBatchEntryBase members

        public override PXSelectBase<AMBatch> AMBatchDataMember => batch;
        public override PXSelectBase<AMMTran> AMMTranDataMember => transactions;
        public override LSAMMTran LSSelectDataMember => lsselect;
        public override PXSelectBase<AMMTranSplit> AMMTranSplitDataMember => splits;

        #endregion
    }
}