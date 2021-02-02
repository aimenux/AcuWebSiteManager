using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.PO;
using System.Linq;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Common;
using POLine = PX.Objects.PO.POLine;
using POOrder = PX.Objects.PO.POOrder;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.AM.GraphExtensions;

namespace PX.Objects.AM
{
    public class ProdDetail : PXGraph<ProdDetail, AMProdItem>
    {
        [PXViewName(Messages.ProductionOrder)]
        public PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Optional<AMProdItem.orderType>>>> ProdItemRecords;
        [PXViewName(Messages.Operations)]
        [PXImport(typeof(AMProdItem))]
        public PXSelect<AMProdOper,
            Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdOper.prodOrdID, Equal<Optional<AMProdItem.prodOrdID>>>>,
            OrderBy<Asc<AMProdOper.operationCD>>> ProdOperRecords;
        [PXHidden]
        public PXSelect<AMProdOper, Where<AMProdOper.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdOper.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdOper.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdOperSelected;
        [PXViewName(Messages.Material)]
        [PXImport(typeof(AMProdItem))]
        public AMOrderedMatlSelect<AMProdItem, AMProdMatl,
            Where<AMProdMatl.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdMatl.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>,
            OrderBy<Asc<AMProdMatl.sortOrder, Asc<AMProdMatl.lineID>>>> ProdMatlRecords;
        [PXViewName(Messages.Steps)]
        [PXImport(typeof(AMProdItem))]
        public PXSelect<AMProdStep, Where<AMProdStep.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdStep.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdStep.operationID, Equal<Current<AMProdOper.operationID>>>>>,
            OrderBy<Asc<AMProdStep.sortOrder, Asc<AMProdStep.lineID>>>> ProdStepRecords;
        [PXViewName(Messages.Tools)]
        [PXImport(typeof(AMProdItem))]
        public PXSelect<AMProdTool, Where<AMProdTool.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdTool.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdTool.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdToolRecords;
        [PXImport(typeof(AMProdItem))]
        public PXSelectJoin<AMProdOvhd, InnerJoin<AMOverhead, On<AMProdOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
            Where<AMProdOvhd.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdOvhd.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdOvhd.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdOvhdRecords;
        [PXHidden]
        public PXSelect<AMToolMst, Where<AMToolMst.toolID, Equal<Current<AMProdTool.toolID>>>> ToolMstRec;
        [PXHidden]
        public PXSelect<AMProdEvnt, Where<AMProdEvnt.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdEvnt.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdEventRecords;
        [PXCopyPasteHiddenView]
        public PXSelect<AMProdTotal, Where<AMProdTotal.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdTotal.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdTotalRecs;
        [PXHidden]
        public PXSelect<AMSchdItem, Where<AMSchdItem.orderType, Equal<Current<AMProdItem.orderType>>, And<AMSchdItem.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> SchdItemRecords;
        [PXHidden]
        public PXSelect<AMSchdOper, Where<AMSchdOper.orderType, Equal<Current<AMProdItem.orderType>>, And<AMSchdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> SchdOperRecords;

        [PXHidden]
        public PXSelect<AMProdAttribute, Where<AMProdAttribute.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdAttribute.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProductionAttributes;

        public PXSetup<AMPSetup> ampsetup;

        public PXSelect<AMProdItemSplit,
            Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>,
            And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> prodItemSplits;

        public LSAMProdItem lsSelectItem;

        public PXSelect<AMProdMatlSplit,
            Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
            And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
            And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
            And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>> ProdMatlSplits;

        public LSAMProdMatl lsSelectMatl;

        [PXHidden]
        public PXSelect<AMProdOper,
            Where<AMProdOper.orderType, Equal<Current<AMProdOper.orderType>>,
            And<AMProdOper.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>,
            And<AMProdOper.operationID, Equal<Current<AMProdOper.operationID>>>>>> OutsideProcessingOperationSelected;

        public PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Optional<AMProdMatl.orderType>>, 
            And<AMProdMatl.prodOrdID, Equal<Optional<AMProdMatl.prodOrdID>>,
            And<AMProdMatl.operationID, Equal<Optional<AMProdMatl.operationID>>,
            And<AMProdMatl.lineID, Equal<Optional<AMProdMatl.lineID>>>>>>> currentposupply;

        [PXCopyPasteHiddenView()]
        public PXSelect<POLineMatl> posupply;

        [PXCopyPasteHiddenView()]
        public PXSelect<POOrder> poorderlink;

        public virtual void PersistBase()
        {
            base.Persist();
        }

        #region CACHE ATTACHED

        [ProductionNbr(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        [ProductionOrderSelector(typeof(AMProdItem.orderType), true, DescriptionField = typeof(AMProdItem.descr))]
        [ProductionNumbering]
        [PXDefault]
        [PXFieldDescription]
        protected virtual void AMProdItem_ProdOrdID_CacheAttached(PXCache sender) { }

        [OperationIDField(IsKey = true, Enabled = false, Visible = false, DisplayName = "Operation DB ID")]
        [PXLineNbr(typeof(AMProdItem.lineCntrOperation))]
        protected virtual void AMProdOper_OperationID_CacheAttached(PXCache sender)
        {
#if DEBUG
            //Cache attached to change display name so we can provide the user with a way to see the DB ID if needed 
#endif
        }

        //Removing PXUIRequired
        [RevisionIDField(DisplayName = "BOM Revision")]
        protected virtual void AMProdItem_BOMRevisionID_CacheAttached(PXCache sender) { }

        //Removing PXUIRequired
        [AMOrderTypeField(DisplayName = "Source Order Type", Visibility = PXUIVisibility.Undefined)]
        protected virtual void AMProdItem_SourceOrderType_CacheAttached(PXCache sender) { }

        //Removing PXUIRequired
        [ProductionNbr(DisplayName = "Source Production Nbr", Visibility = PXUIVisibility.Undefined)]
        protected virtual void AMProdItem_SourceProductionNbr_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [SortOrderDefault(typeof(AMProdStep.lineID))]
        protected virtual void AMProdStep_SortOrder_CacheAttached(PXCache sender) { }

        #endregion

        public override void Persist()
        {
            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                //if the graph is currently processing a long operation (most likely scheduling the order) then we need to wait until the process completes before we change the status
                throw new PXException(Messages.OrderUpdatingInLongProcess);
            }

            var reschedule = ChangesRequireReschedule();

            var currentProdItem = ProdItemRecords.Current;
            if (currentProdItem != null && reschedule && !ProdItemRecords.Cache.IsRowDeleted(currentProdItem))
            {
                var operations = ProdOperRecords.Select().ToFirstTableList();
                ProductionTransactionHelper.ResetOperationValues(ProdOperRecords.Cache, operations, currentProdItem);
                var firstLastOperations = GetFirstLastOperationIds(operations);
                currentProdItem.FirstOperationID = firstLastOperations?.Item1;
                currentProdItem.LastOperationID = firstLastOperations?.Item2;

                //Write change of operation production events...
                if(currentProdItem.StatusID == ProductionOrderStatus.InProcess
                    || currentProdItem.StatusID == ProductionOrderStatus.Completed)
                {
                    WriteOperationDeletedProductionEvents();
                    WriteOperationChangeProductionEvents(operations);
                    SyncSchdOper(currentProdItem, operations);
                }

                ProdItemRecords.Update(currentProdItem);
            }

            using (var ts = new PXTransactionScope())
            {
                if (ProdItemRecords.Current != null && ProdItemRecords.Current.StatusID.EqualsWithTrim(ProductionOrderStatus.Planned))
                {
                    try
                    {
                        BOMCostRoll.UpdatePlannedMaterialCosts(this, ProdItemRecords.Current);
                        AMProdTotal amProdTotal1 = PXSelect<AMProdTotal,
                            Where<AMProdTotal.orderType, Equal<Required<AMProdTotal.orderType>>,
                                And<AMProdTotal.prodOrdID, Equal<Required<AMProdTotal.prodOrdID>>>>>
                                .Select(this, ProdItemRecords.Current.OrderType, ProdItemRecords.Current.ProdOrdID);
                        if (amProdTotal1 == null)
                        {
                            amProdTotal1 = ProdTotalRecs.Insert(new AMProdTotal());
                        }
                        ProductionTransactionHelper.UpdatePlannedProductionTotals(this, ProdItemRecords.Current, amProdTotal1);
                    }
                    catch (Exception exception)
                    {
                        PXTraceHelper.PxTraceException(exception);
                        var msg = Messages.GetLocal(Messages.RecalculatePlanCostException,
                            ProdItemRecords.Current == null
                                ? string.Empty
                                : ProdItemRecords.Current.OrderType.TrimIfNotNullEmpty(),
                            ProdItemRecords.Current == null
                                ? string.Empty
                                : ProdItemRecords.Current.ProdOrdID.TrimIfNotNullEmpty());
                        throw new PXException($"{msg}: {exception.Message}");
                    }
                }
#if DEBUG
                AMDebug.TraceModifiedCaches(this);
#endif
                base.Persist();

                ts.Complete();
            }

            //no need for a transaction... make sure plan costs and schedule are re-processed the same as it is from prodmaint.

            if (ProdItemRecords.Current == null || PXLongOperation.GetCurrentItem() != null)
            {
                return;
            }

            reschedule = reschedule && (ProdItemRecords.Current.StatusID == ProductionOrderStatus.Planned || ProdItemRecords.Current.StatusID == ProductionOrderStatus.Released);

            if (!reschedule)
            {
                return;
            }

            PXLongOperation.StartOperation(this, delegate ()
            {
                try
                {
                    ProductionScheduleEngine.ProcessSchedule(this, ProdItemRecords.Current);
                    ProdItemRecords.Current = ProdItemRecords.Cache.LocateElse(ProdItemRecords.Current);

                    if (IsDirty)
                    {
                        base.Persist();
                    }
                }
                catch (Exception exception)
                {
                    PXTraceHelper.PxTraceException(exception);
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine(Messages.GetLocal(Messages.RescheduleOrderException,
                        ProdItemRecords.Current?.OrderType.TrimIfNotNullEmpty(),
                        ProdItemRecords.Current?.ProdOrdID.TrimIfNotNullEmpty()));
                    sb.AppendLine(exception.Message);

                    throw new PXException(sb.ToString());
                }
            });
        }

        /// <summary>
        /// Changes to the order indicate a need to reschedule the order
        /// </summary>
        protected virtual bool ChangesRequireReschedule()
        {
            if (!ProdOperRecords.Cache.IsInsertedUpdatedDeleted)
            {
                return false;
            }

            var updatedRows = new List<AMProdOper>();
            foreach (AMProdOper row in ProdOperRecords.Cache.Cached)
            {
                var rowStatus = ProdOperRecords.Cache.GetStatus(row);
                if (rowStatus == PXEntryStatus.Inserted || rowStatus == PXEntryStatus.Deleted)
                {
                    return true;
                }

                if (rowStatus == PXEntryStatus.Updated)
                {
                    updatedRows.Add(row);
                }
            }

            foreach (var updatedRow in updatedRows)
            {
                var origRow = (AMProdOper)ProdOperRecords.Cache.GetOriginal(updatedRow);
                if (OperationChangeRequiresReschedule(origRow, updatedRow))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void WriteOperationDeletedProductionEvents()
        {
            foreach (AMProdOper oper in ProdOperRecords.Cache.Deleted)
            {
                var eventDesc = Messages.GetLocal(Messages.DeletedOperationEvent, oper.OperationCD, oper.Descr.TrimIfNotNullEmpty(), oper.WcID);
                ProdEventRecords.Insert(ProductionEventHelper.BuildEvent(ProductionEventType.OperationChange, eventDesc, oper.ProdOrdID, oper.OrderType));
            }
        }

        protected virtual void WriteOperationChangeProductionEvents(List<AMProdOper> operations)
        {
            if(operations == null)
            {
                return;
            }

            foreach (var oper in operations)
            {
                WriteOperationChangeProductionEvent(oper);
            }
        }

        protected virtual void WriteOperationChangeProductionEvent(AMProdOper prodOper)
        {
            if(prodOper?.OperationID == null)
            {
                return;
            }

            var row = ProdOperRecords.Cache.LocateElse(prodOper);
            var rowStatus = ProdOperRecords.Cache.GetStatus(row);

            if (rowStatus == PXEntryStatus.Inserted)
            {
                var operationValue = $"'{row.OperationCD}'";
                if (!string.IsNullOrWhiteSpace(row.Descr))
                {
                    operationValue = $"'{row.OperationCD}' - '{row.Descr}'";
                }

                var insertMsg = Messages.GetLocal(Messages.NewRowInserted, Common.Cache.GetCacheName(typeof(AMProdOper)), operationValue);
                ProdEventRecords.Insert(ProductionEventHelper.BuildEvent(ProductionEventType.OperationChange, insertMsg, prodOper.ProdOrdID, prodOper.OrderType));
                return;
            }

            if (rowStatus != PXEntryStatus.Updated)
            {
                return;
            }

            var origRow = ProdOperRecords.Cache.GetOriginal(row);

            var sb = new System.Text.StringBuilder();
            var operationCdChanged = ProductionEventHelper.ValueChanged<AMProdOper.operationCD>(
                ProdOperRecords.Cache, origRow, row, out var operationCDmsg);
            if (operationCdChanged)
            {
                sb.Append($"{operationCDmsg}.");
            }

            if (ProductionEventHelper.ValueChanged<AMProdOper.wcID>(ProdOperRecords.Cache, origRow, row, out var wcIDmsg))
            {
                var additionalMsg = " ";
                if (!operationCdChanged)
                {
                    additionalMsg = $" {PXUIFieldAttribute.GetDisplayName<AMProdOper.operationCD>(ProdOperRecords.Cache)} {prodOper.OperationCD} ";
                }

                sb.Append($"{additionalMsg}{wcIDmsg}.");
            }

            if (sb.Length == 0)
            {
                return;
            }

            ProdEventRecords.Insert(ProductionEventHelper.BuildEvent(ProductionEventType.OperationChange, sb.ToString(), prodOper.ProdOrdID, prodOper.OrderType));
        }

        internal virtual void SyncSchdOper(AMProdItem prodItem, List<AMProdOper> operations)
        {
            var schdOpers =
                PXSelect<
                    AMSchdOper,
                    Where<AMSchdOper.orderType, Equal<Required<AMProdItem.orderType>>,
                        And<AMSchdOper.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>
                    .Select(this, prodItem?.OrderType, prodItem?.ProdOrdID).ToFirstTableList();

            foreach (var operation in operations)
            {
                var schdOper = schdOpers?.Where(x => x.OperationID == operation.OperationID).FirstOrDefault();
                if (schdOper?.OperationID == null)
                {
                    //Insert
                    var newSchdOper = ProductionScheduleEngine.ConstructSchdOperationFromProdOper(prodItem, operation);
                    if (newSchdOper?.OperationID != null)
                    {
                        SchdOperRecords.Insert(newSchdOper);
                    }
                    continue;
                }

                //Update
                schdOper = ProductionScheduleEngine.SetSchdOperationFromProdOper(operation, SchdOperRecords.Cache.LocateElseCopy(schdOper));
                if (schdOper?.OperationID != null)
                {
                    SchdOperRecords.Update(schdOper);
                }
            }
        }

        /// <summary>
        /// Find the first and last operation IDs from the given operations
        /// </summary>
        /// <param name="prodOpers"></param>
        /// <returns>Item1 = first opeartionId, Item2 = last opeartionId</returns>
        internal static Tuple<int?, int?> GetFirstLastOperationIds(List<AMProdOper> prodOpers)
        {
            AMProdOper firstOper = null;
            AMProdOper lastoper = null;

            if (prodOpers == null || prodOpers.Count == 0)
            {
                return new Tuple<int?, int?>(null, null);
            }

            var orderedProdOper = prodOpers.OrderOperations().ToList();
            firstOper = orderedProdOper.First();
            lastoper = orderedProdOper.Last();

            return new Tuple<int?, int?>(firstOper?.OperationID, lastoper?.OperationID);
        }

        /// <summary>
        /// Operation row values changed that would impact the orders schedule
        /// </summary>
        protected virtual bool OperationChangeRequiresReschedule(AMProdOper row1, AMProdOper row2)
        {
#if DEBUG
            AMDebug.PrintChangedValues(row1, row2);
#endif
            return row1 != null && row2 != null
                                && (row1.WcID != row2.WcID
                                    || row1.OperationCD != row2.OperationCD
                                    || row1.SetupTime != row2.SetupTime
                                    || row1.RunUnitTime != row2.RunUnitTime
                                    || row1.RunUnits != row2.RunUnits
                                    || row1.MachineUnitTime != row2.MachineUnitTime
                                    || row1.MachineUnits != row2.MachineUnits
                                    || row1.MoveTime != row2.MoveTime
                                    || row1.QueueTime != row2.QueueTime);
        }

        public PXAction<AMProdItem> ViewCompBomID;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable viewCompBomID(PXAdapter adapter)
        {
            BOMMaint.Redirect(ProdMatlRecords?.Current?.CompBOMID, ProdMatlRecords?.Current?.CompBOMRevisionID);
            return adapter.Get();
        }

        public PXAction<AMProdItem> InquiresDropMenu;
        [PXUIField(DisplayName = Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable inquiresDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMProdItem> CriticalMatl;
        [PXUIField(DisplayName = Messages.CriticalMaterial, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable criticalMatl(PXAdapter adapter)
        {
            if (this.ProdItemRecords.Current != null)
            {
                var graph = PXGraph.CreateInstance<CriticalMaterialsInq>();
                graph.ProdItemRecs.Current.OrderType = ProdItemRecords.Current.OrderType;
                graph.ProdItemRecs.Current.ProdOrdID = ProdItemRecords.Current.ProdOrdID;
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
            }
            return adapter.Get();
        }

        public PXAction<AMProdItem> AttributesInq;
        [PXUIField(DisplayName = Messages.Attributes, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable attributesInq(PXAdapter adapter)
        {
            if (this.ProdItemRecords.Current != null)
            {
                ProductionAttributesInq.RedirectGraph(ProdItemRecords.Current);
            }
            return adapter.Get();
        }

        /// <summary>
        /// Production Item allocation details redirect (Action)
        /// </summary>
        public PXAction<AMProdItem> InventoryAllocationDetailInq;
        /// <summary>
        /// Production Item allocation details redirect (Delegate)
        /// </summary>
        [PXUIField(DisplayName = "Allocation Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable inventoryAllocationDetailInq(PXAdapter adapter)
        {
            var allocGraph =
                InventoryHelper.GetInventoryAllocDetEnq<AMProdItem.inventoryID, AMProdItem.siteID, AMProdItem.subItemID>(
                    ProdItemRecords.Cache, ProdItemRecords.Current);
            if (allocGraph != null)
            {
                PXRedirectHelper.TryRedirect(allocGraph, PXRedirectHelper.WindowMode.New);
            }

            return adapter.Get();
        }

        /// <summary>
        /// Material allocation details redirect (Action)
        /// </summary>
        public PXAction<AMProdItem> InventoryAllocationDetailInqMatl;
        /// <summary>
        /// Material allocation details redirect (Delegate)
        /// </summary>
        [PXUIField(DisplayName = "Alloc. Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable inventoryAllocationDetailInqMatl(PXAdapter adapter)
        {
            var allocGraph =
                InventoryHelper.GetInventoryAllocDetEnq<AMProdMatl.inventoryID, AMProdMatl.siteID, AMProdMatl.subItemID>(
                    ProdMatlRecords.Cache, ProdMatlRecords.Current);
            if (allocGraph != null)
            {
                PXRedirectHelper.TryRedirect(allocGraph, PXRedirectHelper.WindowMode.New);
            }

            return adapter.Get();
        }
        
        public PXAction<AMProdItem> CreatePurchaseOrderInq;
        [PXUIField(DisplayName = Messages.CreatePurchaseOrdersInq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable createPurchaseOrderInq(PXAdapter adapter)
        {
            var prodItem = ProdItemRecords?.Current;
            if (prodItem?.InventoryID == null)
            {
                return adapter.Get();
            }

            var graph = CreateInstance<POCreate>();
            var filterExt = PXCache<POCreate.POCreateFilter>.GetExtension<POCreateFilterExt>(graph.Filter?.Current);
            if (filterExt == null)
            {
                return adapter.Get();
            }

            filterExt.AMOrderType = prodItem.OrderType;
            filterExt.ProdOrdID = prodItem.ProdOrdID;

            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);

            return adapter.Get();
        }

        public PXAction<AMProdItem> CreateProductionOrderInq;
        [PXUIField(DisplayName = Messages.CreateProductionOrdersInq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable createProductionOrderInq(PXAdapter adapter)
        {
            var prodItem = ProdItemRecords?.Current;
            if (prodItem?.InventoryID == null)
            {
                return adapter.Get();
            }

            var graph = CreateInstance<CreateProductionOrdersProcess>();
            graph.Filter.Current.OrderType = prodItem.OrderType;
            graph.Filter.Current.ProdOrdID = prodItem.ProdOrdID;

            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);

            return adapter.Get();
        }

        public PXAction<AMProdItem> CreatePurchaseOrder;
        [PXButton]
        [PXUIField(DisplayName = Messages.CreatePurchaseOrder)]
        public virtual IEnumerable createPurchaseOrder(PXAdapter adapter)
        {
            if (this.ProdItemRecords.Current != null && this.ProdOperSelected.Current != null)
            {
                var listProdMatl = new List<AMProdMatl>();
                var vendorID = ProdOperSelected.Current.VendorID;
                var vendorLocationID = ProdOperSelected.Current.VendorLocationID;
                var first = true;

                foreach (AMProdMatl prodMatl in PXSelect<AMProdMatl,
                    Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                        And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                        And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                        And<AMProdMatl.materialType, Equal<AMMaterialType.subcontract>,
                        And<AMProdMatl.subcontractSource, Equal<AMSubcontractSource.purchase>,
                        And<AMProdMatl.pOCreate, Equal<True>>>>>>>,
                    OrderBy<Asc<AMProdMatl.sortOrder>
                    >>.Select(this, ProdOperSelected.Current.OrderType, ProdOperSelected.Current.ProdOrdID, ProdOperSelected.Current.OperationID))
                {
                    if (prodMatl == null)
                    {
                        throw new PXException(Messages.GetLocal(Messages.NoMaterialOnOperationForThisProcess));
                    }

                    // Get the Preferred Vendor ID and Location
                    InventoryItem invItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>
                        >>.Select(this, prodMatl.InventoryID);

                    if (first)
                    {
                        if (vendorID == null)
                        {
                            vendorID = invItem.PreferredVendorID;
                            vendorLocationID = invItem.PreferredVendorLocationID;
                        }

                        if (vendorID == null || vendorLocationID == null)
                        {
                            throw new PXException(Messages.GetLocal(Messages.NoVendorSelectedOrPreferredVendor));
                        }

                        listProdMatl.Add(prodMatl);
                        first = false;
                    }
                    else
                    {
                        if (invItem.PreferredVendorID == null || invItem.PreferredVendorID == vendorID)
                        {
                            listProdMatl.Add(prodMatl);
                        }
                    }
                }

                ProcessPOCreate(this, listProdMatl, vendorID, vendorLocationID);
            }

            return adapter.Get();
        }

        public PXAction<AMProdItem> pOSupplyOK;
        [PXUIField(DisplayName = "PO Link", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
        protected virtual IEnumerable POSupplyOK(PXAdapter adapter)
        {

            if (ProdMatlRecords.Current != null &&
                ProdMatlRecords.Current.POCreate == true &&
                currentposupply.AskExt((graph, viewName) =>
                {
                    foreach (POLineMatl supply in posupply.Cache.Updated.ToArray<POLineMatl>())
                    {
                        if (supply.AMProdMatlSplitLineNbr == null)
                        {
                            posupply.Cache.SetStatus(supply, PXEntryStatus.Notchanged);
                            posupply.Cache.Remove(supply);
                        }
                    }
                }) == WebDialogResult.OK)
            {
                LinkSupplyDemand();
            }

            return adapter.Get();
        }


        #region AMPRODITEM Methods

        protected bool OrderOnHold
        {
            get
            {
                if (ProdItemRecords.Cache.Current == null)
                {
                    return false;
                }

                return ((AMProdItem) ProdItemRecords.Cache.Current).Hold.GetValueOrDefault();
            }
        }

        public ProdDetail()
        {
            var setup = ampsetup.Current;
            AMPSetup.CheckSetup(setup);

            ProdItemRecords.AllowDelete = false;
            
            InquiresDropMenu.AddMenuAction(CriticalMatl);
            InquiresDropMenu.AddMenuAction(AttributesInq);
            InquiresDropMenu.AddMenuAction(InventoryAllocationDetailInq);
            InquiresDropMenu.AddMenuAction(CreatePurchaseOrderInq);
            InquiresDropMenu.AddMenuAction(CreateProductionOrderInq);
        }

        protected virtual void AMProdItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null)
            {
                return;
            }

            EnableRecords(amProdItem);

            var closedCancelledStatus = ProductionStatus.IsClosedOrCanceled(amProdItem.StatusID);

            PXUIFieldAttribute.SetEnabled<AMProdItem.prodDate>(sender, null, false);
            PXUIFieldAttribute.SetEnabled<AMProdItem.inventoryID>(sender, null, false);
            PXUIFieldAttribute.SetEnabled<AMProdItem.subItemID>(sender, null, false);
            PXUIFieldAttribute.SetEnabled<AMProdItem.siteID>(sender, null, false);
            PXUIFieldAttribute.SetEnabled<AMProdItem.hold>(sender, null, !closedCancelledStatus);

            PXUIFieldAttribute.SetEnabled<AMProdOper.operationCD>(ProdOperRecords.Cache, null, ProdOperRecords.AllowInsert);

            var isOpen = !ProductionStatus.IsClosedOrCanceled(amProdItem);
            CreatePurchaseOrderInq.SetEnabled(isOpen);
            CreateProductionOrderInq.SetEnabled(isOpen);

            //Hide an oper field from here...
            var isStandardCostItem = amProdItem.CostMethod == CostMethod.Standard;
            PXUIFieldAttribute.SetVisible<AMProdOper.wIPBalance>(ProdOperSelected.Cache, null, !isStandardCostItem);
            PXUIFieldAttribute.SetVisible<AMProdOper.wIPComp>(ProdOperSelected.Cache, null, !isStandardCostItem);
        }

        #endregion

        #region Enable methods

        /// <summary>
        /// Indicates true when the operation exists on a release manufacturing transaction
        /// </summary>
        protected virtual bool OperationHasReleasedTransactions(AMProdOper amProdOper)
        {
            if (string.IsNullOrWhiteSpace(amProdOper?.OrderType) ||
                string.IsNullOrWhiteSpace(amProdOper.ProdOrdID) ||
                amProdOper.OperationID == null)
            {
                return false;
            }

            AMMTran ammTran = PXSelectReadonly<AMMTran,
                Where<AMMTran.released, Equal<True>,
                    And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                    And<AMMTran.operationID, Equal<Required<AMMTran.operationID>>>>>>
                    >.SelectWindowed(this, 0, 1, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID);

            return ammTran != null;
        }

        /// <summary>
        /// Indicates true when the operation exists on a release manufacturing transaction
        /// </summary>
        protected virtual bool OperationHasTransactions(AMProdOper amProdOper)
        {
            if (amProdOper == null ||
                string.IsNullOrWhiteSpace(amProdOper.OrderType) ||
                string.IsNullOrWhiteSpace(amProdOper.ProdOrdID) ||
                amProdOper.OperationID == null)
            {
                return false;
            }

            return FirstOperationTransaction(amProdOper) != null;
        }

        protected virtual AMMTran FirstOperationTransaction(AMProdOper amProdOper)
        {
            if (amProdOper == null ||
                string.IsNullOrWhiteSpace(amProdOper.OrderType) ||
                string.IsNullOrWhiteSpace(amProdOper.ProdOrdID) ||
                amProdOper.OperationID == null)
            {
                return null;
            }

            return PXSelectReadonly<AMMTran,
                Where<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                    And<AMMTran.operationID, Equal<Required<AMMTran.operationID>>>>>
                    >.SelectWindowed(this, 0, 1, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID);
        }

        protected virtual void EnableRecords(AMProdItem amProdItem)
        {
            var enabled = false;
            if (amProdItem != null)
            {
                enabled = ProductionStatus.IsEditableStatus(amProdItem);

                if (amProdItem.InventoryID == null)
                {
                    enabled = false;
                }
            }

            //  OPERATIONS - ProdOperRecords
            ProdOperRecords.Cache.AllowInsert = enabled;
            ProdOperRecords.Cache.AllowUpdate = enabled;
            ProdOperRecords.Cache.AllowDelete = enabled;

            // MATERIAL - ProdMatlRecords
            ProdMatlRecords.Cache.AllowInsert = enabled;
            ProdMatlRecords.Cache.AllowUpdate = enabled;
            ProdMatlRecords.Cache.AllowDelete = enabled;

            // STEPS - ProdStepRecords
            ProdStepRecords.Cache.AllowInsert = enabled;
            ProdStepRecords.Cache.AllowUpdate = enabled;
            ProdStepRecords.Cache.AllowDelete = enabled;

            // TOOLS - ProdToolRecords
            ProdToolRecords.Cache.AllowInsert = enabled;
            ProdToolRecords.Cache.AllowUpdate = enabled;
            ProdToolRecords.Cache.AllowDelete = enabled;

            // OVERHEADS - ProdOvhdRecords
            ProdOvhdRecords.Cache.AllowInsert = enabled;
            ProdOvhdRecords.Cache.AllowUpdate = enabled;
            ProdOvhdRecords.Cache.AllowDelete = enabled;

            // OutsideProcessing
            OutsideProcessingOperationSelected.Cache.AllowInsert = enabled;
            OutsideProcessingOperationSelected.Cache.AllowUpdate = enabled;
            OutsideProcessingOperationSelected.Cache.AllowDelete = enabled;
        }

        /// <summary>
        /// Returns true if the given production record contains values that restricts the rows usage in terms of update/delete
        /// </summary>
        /// <param name="row">AMProd* record</param>
        /// <returns></returns>
        protected virtual bool RowIsEnableRestricted(object row)
        {
            if (row == null)
            {
                return false;
            }

            // OPERATION
            // na

            // MATERIAL
            if (row is AMProdMatl)
            {
                return ((AMProdMatl)row).QtyActual.GetValueOrDefault() != 0
                       || ((AMProdMatl)row).TotActCost.GetValueOrDefault() != 0;
            }

            // TOOLS
            if (row is AMProdTool)
            {
                return ((AMProdTool)row).TotActUses.GetValueOrDefault() != 0
                       || ((AMProdTool)row).TotActCost.GetValueOrDefault() != 0;
            }

            // OVERHEAD
            if (row is AMProdOvhd)
            {
                return ((AMProdOvhd)row).TotActCost.GetValueOrDefault() != 0;
            }

            return false;
        }
        #endregion

        protected virtual AMWC GetCurrentWorkcenter()
        {
            AMWC workCenter = PXSelect<AMWC, Where<AMWC.wcID, Equal<Current<AMProdOper.wcID>>>>.Select(this);

            if (workCenter == null)
            {
                //SET DEFAULTS TO ALWAYS RETURN A WORKCENTER OBJECT
                workCenter = new AMWC()
                {
                    BflushLbr = false,
                    BflushMatl = false
                };
            }

            return workCenter;
        }

        #region AMPRODOPER Methods

        protected virtual void AMProdOper_StatusID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var prodItem = ProdItemRecords.Current;
            var row = (AMProdOper)e.Row;
            if (row == null || prodItem?.StatusID == null)
            {
                return;
            }

            e.NewValue = prodItem.StatusID == ProductionOrderStatus.Planned
                ? ProductionOrderStatus.Planned
                : ProductionOrderStatus.Released;
        }

        protected virtual void AMProdOper_OperationCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var prodItem = ProdItemRecords.Current;
            var prodOper = (AMProdOper)e.Row;
            var newOperationCD = (string)e.NewValue;
            if(prodItem?.OrderType == null || prodOper?.OperationID == null
                || string.IsNullOrWhiteSpace(newOperationCD)
                || CanOperationCDChange(prodItem, prodOper, newOperationCD, out var exception))
            {
                return;
            }

            if(exception != null)
            {
                e.Cancel = true;
                e.NewValue = prodOper.OperationCD;
                PXTrace.WriteError($"Unable to change operation CD: {exception.Message}");
                cache.RaiseExceptionHandling<AMProdOper.operationCD>(prodOper, prodOper.OperationCD, exception);
            }
        }

        protected virtual bool CanOperationCDChange(AMProdItem prodItem, AMProdOper prodOper, string newOperationCD, out PXSetPropertyException ex)
        {
            ex = null;
            if (prodItem?.OrderType == null)
            {
                throw new ArgumentNullException(nameof(prodItem));
            }

            if (prodOper?.OperationID == null)
            {
                throw new ArgumentNullException(nameof(prodOper));
            }

            if (string.IsNullOrWhiteSpace(newOperationCD))
            {
                throw new ArgumentNullException(nameof(newOperationCD));
            }

            if (!ProductionStatus.IsEditableStatus(prodItem))
            {
                throw new PXException(Messages.ProductionOrderNotEditable, prodItem.OrderType, prodItem.ProdOrdID);
            }

            if(prodItem.StatusID != ProductionOrderStatus.InProcess)
            {
                return true;
            }

            var operations = PXSelect<
                AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>
                .Select(this, prodItem.OrderType, prodItem.ProdOrdID)
                .ToFirstTableList();

            if(operations == null || operations.Count == 0)
            {
                return true;
            }

            var operBefore = OperationHelper.GetOperationBefore(operations, newOperationCD, prodOper.OperationCD);
            if(operBefore?.OperationID != null && operBefore.QtyComplete.GetValueOrDefault() < prodOper.QtyComplete.GetValueOrDefault())
            {
                ex = new PXSetPropertyException(Messages.ChangingOperationCDQtyCompleteLessThan,
                    PXErrorLevel.Error,
                    prodItem.UOM,
                    operBefore.OperationCD, operBefore.QtyComplete.GetValueOrDefault(),
                    newOperationCD, prodOper.QtyComplete.GetValueOrDefault());
                return false;
            }

            var operAfter = OperationHelper.GetOperationAfter(operations, newOperationCD, prodOper.OperationCD);
            if(operAfter?.OperationID != null && operAfter.QtyComplete.GetValueOrDefault() > prodOper.QtyComplete.GetValueOrDefault())
            {
                ex = new PXSetPropertyException(Messages.ChangingOperationCDQtyCompleteGreaterThan,
                    PXErrorLevel.Error,
                    prodItem.UOM,
                    operAfter.OperationCD, operAfter.QtyComplete.GetValueOrDefault(),
                    newOperationCD, prodOper.QtyComplete.GetValueOrDefault());
                return false;
            }

            if(operAfter?.OperationID == null)
            {
                // LAST OPERATION CHANGING

                var changingOperTranBatches = GetUnreleasedBatches(prodItem.OrderType, prodItem.ProdOrdID, prodOper.OperationID);
                if(changingOperTranBatches != null && changingOperTranBatches.Count > 0)
                {
                    ex = new PXSetPropertyException(Messages.ProductionOrderOperationUnreleasedTransactions,
                        PXErrorLevel.Error,
                        prodItem.OrderType, prodItem.ProdOrdID, prodOper.OperationCD,
                        $"[{string.Join(",", changingOperTranBatches.ToArray())}]");
                    return false;
                }

                if(operBefore?.OperationID != null)
                {
                    var previousLastOperTranBatches = GetUnreleasedBatches(prodItem.OrderType, prodItem.ProdOrdID, prodOper.OperationID);
                    if (previousLastOperTranBatches != null && previousLastOperTranBatches.Count > 0)
                    {
                        ex = new PXSetPropertyException(Messages.ProductionOrderOperationUnreleasedTransactions,
                            PXErrorLevel.Error,
                            prodItem.OrderType, prodItem.ProdOrdID, operBefore.OperationCD,
                            $"[{string.Join(",", previousLastOperTranBatches.ToArray())}]");
                        return false;
                    }
                }
            }

            return true;
        }

        private HashSet<string> GetUnreleasedBatches(string orderType, string prodOrdID, int? operationID)
        {
            var tranBatches = new HashSet<string>();
            foreach (AMMTran tran in PXSelectReadonly<AMMTran,
                    Where<AMMTran.released, Equal<False>,
                        And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                        And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                        And<AMMTran.operationID, Equal<Required<AMMTran.operationID>>,
                        And<AMMTran.docType, In<Required<AMMTran.docType>>>>>>>
                        >.Select(this, orderType, prodOrdID, operationID, new string[] { AMDocType.Move, AMDocType.Labor, AMDocType.Disassembly }))
            {
                if (tran?.DocType == null || (tran.DocType == AMDocType.Labor && tran.Qty.GetValueOrDefault() == 0))
                {
                    continue;
                }

                tranBatches.Add($"{AMDocType.GetDocTypeDesc(tran.DocType)} {tran.BatNbr}");
            }
            return tranBatches;
        }

        protected virtual void _(Events.RowSelected<AMProdOper> e)
        {
            var isOutsideProcess = e?.Row?.OutsideProcess == true;
            PXUIFieldAttribute.SetEnabled<AMProdOper.dropShippedToVendor>(e.Cache, e.Row, isOutsideProcess && ProdOperRecords.AllowUpdate);
            PXUIFieldAttribute.SetEnabled<AMProdOper.vendorID>(e.Cache, e.Row, isOutsideProcess && ProdOperRecords.AllowUpdate);
            PXUIFieldAttribute.SetEnabled<AMProdOper.vendorLocationID>(e.Cache, e.Row, isOutsideProcess && ProdOperRecords.AllowUpdate);
        }

        protected virtual void _(Events.FieldUpdated<AMProdOper, AMProdOper.outsideProcess> e)
        {
            var oldValue = Convert.ToBoolean(e.OldValue ?? false);
            if (!oldValue || e.Row.OutsideProcess == true)
            {
                return;
            }

            e.Cache.SetValueExt<AMProdOper.dropShippedToVendor>(e.Row, false);
            e.Cache.SetValueExt<AMProdOper.vendorID>(e.Row, null);
            e.Cache.SetValueExt<AMProdOper.vendorLocationID>(e.Row, null);

            foreach (AMProdMatl matl in ProdMatlRecords.Select())
            {
                ProdMatlRecords.Cache.SetValueExt<AMProdMatl.materialType>(matl, AMMaterialType.Regular);
            }
        }

        protected virtual void AMProdOper_WcID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMProdOper)e.Row;
            if (row == null)
            {
                return;
            }

            var amWC = GetCurrentWorkcenter();
            if (string.IsNullOrWhiteSpace(row.Descr))
            {
                cache.SetValueExt<AMProdOper.descr>(row, amWC.Descr);
            }
            if (!row.BFlush.GetValueOrDefault())
            {
                cache.SetValueExt<AMProdOper.bFlush>(row, amWC.BflushLbr);
            }

            // Set the Scrap Action from Work Center
            cache.SetValueExt<AMProdOper.scrapAction>(row, amWC.ScrapAction);

            // Set Outside Processing from Work Center
            cache.SetValueExt<AMBomOper.outsideProcess>(row, amWC.OutsideFlg.GetValueOrDefault());

            DeleteWorkCenterRelatedOverhead(row);
            CopyOverheadsFromWorkCenter(row);
        }

        protected virtual void AMProdOper_TotalQty_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var row = (AMProdOper)e.Row;
            if (row == null || ProdItemRecords.Current == null)
            {
                return;
            }
            e.NewValue = ProdItemRecords.Current.QtytoProd.GetValueOrDefault();
        }

        protected virtual void AMProdOper_BaseTotalQty_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var row = (AMProdOper)e.Row;
            if (row == null || ProdItemRecords.Current == null)
            {
                return;
            }
            e.NewValue = ProdItemRecords.Current.BaseQtytoProd.GetValueOrDefault();
        }

        protected virtual void AMProdOper_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            var row = (AMProdOper) e.Row;
            if (row == null)
            {
                return;
            }

            if (cache.GetStatus(row) != PXEntryStatus.InsertedDeleted)
            {
                var transaction = FirstOperationTransaction(row);
                if (transaction != null && !string.IsNullOrWhiteSpace(transaction.BatNbr))
                {
                    e.Cancel = true;
                    throw new PXException(Messages.CannotDeleteOperationWithTransaction,
                        row.OperationCD,
                        AMDocType.GetDocTypeDesc(transaction.DocType),
                        transaction.BatNbr.TrimIfNotNullEmpty());
                }
            }

            AMProdAttribute prodOperAttribute = PXSelect<
                AMProdAttribute,
                Where<AMProdAttribute.orderType, Equal<Required<AMProdAttribute.orderType>>,
                    And<AMProdAttribute.prodOrdID, Equal<Required<AMProdAttribute.prodOrdID>>,
                    And<AMProdAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>>
                        >>>.Select(this, row.OrderType, row.ProdOrdID, row.OperationID);

            if (prodOperAttribute != null)
            {
                //Have the user confirm the delete when atributes exist
                if (ProdOperRecords.Ask(Messages.ConfirmDeleteTitle, Messages.GetLocal(Messages.ConfirmOperationDeleteWhenAttributesExist),
                    MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    e.Cancel = true;
                }

                DeleteProductionOperationAttributes(row);
            }
        }

        protected virtual void DeleteProductionOperationAttributes(AMProdOper row)
        {
            foreach(AMProdAttribute prodOperAttribute in PXSelect<
                AMProdAttribute,
                Where<AMProdAttribute.orderType, Equal<Required<AMProdAttribute.orderType>>,
                    And<AMProdAttribute.prodOrdID, Equal<Required<AMProdAttribute.prodOrdID>>,
                    And<AMProdAttribute.operationID, Equal<Required<AMProdAttribute.operationID>>>
                        >>>.Select(this, row.OrderType, row.ProdOrdID, row.OperationID))
            {
                ProductionAttributes.Delete(prodOperAttribute);
            }
        }

        #endregion

        #region AMPRODMATL Methods

        protected virtual void AMProdMatl_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            AMProdMatl matl = (AMProdMatl)e.Row;

            if (matl == null)
            {
                return;
            }

            // Require SUBITEMID when the item is a stock item
            if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && matl.InventoryID != null && matl.IsStockItem.GetValueOrDefault() && matl.SubItemID == null)
            {
                cache.RaiseExceptionHandling<AMProdMatl.subItemID>(
                        matl,
                        matl.SubItemID,
                        new PXSetPropertyException(Messages.SubItemIDRequiredForStockItem, PXErrorLevel.Error));
            }
        }

        public void AMProdMatl_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            AMProdMatl row = (AMProdMatl)e.Row;

            if (row != null)
            {
                cache.SetDefaultExt<AMProdMatl.subItemID>(e.Row);
                cache.SetDefaultExt<AMProdMatl.descr>(e.Row);
                cache.SetDefaultExt<AMProdMatl.unitCost>(e.Row);
                row.BFlush = GetCurrentWorkcenter().BflushMatl;
            }
        }

        protected virtual void AMProdMatl_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (RowIsEnableRestricted(e.Row))
            {
                e.Cancel = true;
                throw new PXException(Messages.GetLocal(Messages.CannotDeleteRecWithQtyCost));
            }
        }

        protected virtual void AMProdMatl_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (AMProdMatl) e.Row;
            if (row == null)
            {
                return;
            }

            var isDisassembly = (ProdItemRecords?.Current?.Function ?? OrderTypeFunction.Regular) == OrderTypeFunction.Disassemble;

            if (RowIsEnableRestricted(e.Row))
            {
                //Disable all fields
                PXUIFieldAttribute.SetEnabled(cache, e.Row, false);

                //Then enable a select few if order is on hold...
                PXUIFieldAttribute.SetEnabled<AMProdMatl.qtyReq>(cache, null, OrderOnHold);
                PXUIFieldAttribute.SetEnabled<AMProdMatl.descr>(cache, null, OrderOnHold);
                PXUIFieldAttribute.SetEnabled<AMProdMatl.bFlush>(cache, null, OrderOnHold && !isDisassembly);
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMProdMatl.bFlush>(cache, null, !isDisassembly);
            PXUIFieldAttribute.SetEnabled<AMProdMatl.subItemID>(cache, e.Row, row.IsStockItem.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMProdMatl.subcontractSource>(cache, e.Row, row.MaterialType == AMMaterialType.Subcontract);

            // Warehouse Override Checked or unchecked enable or disable warehouse
            PXUIFieldAttribute.SetEnabled<AMProdMatl.siteID>(cache, e.Row, row.WarehouseOverride.GetValueOrDefault());

            // Allow only POCreate or ProdCreate to be selected at any time. Cannot select both
            PXUIFieldAttribute.SetEnabled<AMProdMatl.prodCreate>(cache, e.Row, !row.POCreate.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMProdMatl.pOCreate>(cache, e.Row, !row.ProdCreate.GetValueOrDefault() && row.MaterialType == AMMaterialType.Regular);
        }

        protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            object MatlUnitCost;
            sender.RaiseFieldDefaulting<AMProdMatl.unitCost>(e.Row, out MatlUnitCost);

            if (MatlUnitCost != null && (decimal)MatlUnitCost != 0m)
            {
                decimal? matlUnitCost = INUnitAttribute.ConvertToBase<AMProdMatl.inventoryID>(sender, e.Row, ((AMProdMatl)e.Row).UOM, (decimal)MatlUnitCost, INPrecision.UNITCOST);
                sender.SetValueExt<AMProdMatl.unitCost>(e.Row, matlUnitCost);
            }
        }

        protected virtual void AMProdMatl_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitCost(sender, e);
        }

        protected virtual void AMProdMatl_WarehouseOverride_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMProdMatl) e.Row;
            var prodItem = this.ProdItemRecords.Current;
            if (prodItem == null || row == null || row.WarehouseOverride.GetValueOrDefault() ||
                row.SiteID != null && row.SiteID == prodItem.SiteID)
            {
                return;
            }

            //No longer overriding - reset back to the order warehouse
            sender.SetValue<AMProdMatl.siteID>(row, prodItem.SiteID);
        }

        protected virtual void AMProdMatl_SubcontractSource_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var prodMatl = (AMProdMatl)e.Row;
            var newContractSource = (int)e.NewValue;
            if (prodMatl?.MaterialType == null)
            {
                return;
            }

            if (prodMatl.MaterialType == AMMaterialType.Regular && newContractSource != AMSubcontractSource.None)
            {
                e.NewValue = AMSubcontractSource.None;
                e.Cancel = true;
            }
            else if (prodMatl.MaterialType == AMMaterialType.Subcontract && newContractSource == AMSubcontractSource.None)
            {
                e.NewValue = prodMatl.SubcontractSource;
                e.Cancel = true;
            }
        }

        protected virtual void AMProdMatl_MaterialType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var prodMatl = (AMProdMatl)e.Row;

            if (prodMatl?.MaterialType == null)
            {
                return;
            }

            if (prodMatl.MaterialType == AMMaterialType.Regular)
            {
                prodMatl.SubcontractSource = AMSubcontractSource.None;
                prodMatl.POCreate = false;
            }
            else
            {
                prodMatl.SubcontractSource = AMSubcontractSource.Purchase;
                prodMatl.POCreate = true;
            }
        }

        protected virtual void AMProdMatl_SubcontractSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var prodMatl = (AMProdMatl)e.Row;

            if (prodMatl?.SubcontractSource == null)
            {
                return;
            }

            prodMatl.POCreate = prodMatl.SubcontractSource == AMSubcontractSource.Purchase && prodMatl.MaterialType == AMMaterialType.Subcontract;
        }

        #endregion

        #region AMPRODTOOL Methods
        public void AMProdTool_ToolID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            AMProdTool prodTool = (AMProdTool)e.Row;

            if (prodTool == null)
            {
                return;
            }

            AMToolMst amToolMst =
            PXSelect<AMToolMst, Where<AMToolMst.toolID, Equal<Required<AMToolMst.toolID>>>>.Select(this,
                prodTool.ToolID);

            if (amToolMst != null)
            {
                prodTool.Descr = amToolMst.Descr;
                prodTool.UnitCost = amToolMst.UnitCost;
            }
            else
            {
                prodTool.Descr = string.Empty;
                prodTool.UnitCost = 0;
            }

        }

        protected virtual void AMProdTool_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (RowIsEnableRestricted(e.Row))
            {
                e.Cancel = true;
                AMDebug.TraceWriteMethodName(Messages.CannotDeleteRecWithQtyCost);
                throw new PXException(Messages.CannotDeleteRecWithQtyCost);
            }
        }

        protected virtual void AMProdTool_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (RowIsEnableRestricted(e.Row))
            {
                //Disable all fields
                PXUIFieldAttribute.SetEnabled(cache, e.Row, false);

                //Then enable a select few if order is on hold...
                PXUIFieldAttribute.SetEnabled<AMProdTool.qtyReq>(cache, null, OrderOnHold);
            }
        }

        #endregion

        #region AMPRODOVHD Methods
        protected virtual void AMProdOvhd_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (RowIsEnableRestricted(e.Row))
            {
                e.Cancel = true;
                AMDebug.TraceWriteMethodName(Messages.CannotDeleteRecWithQtyCost);
                throw new PXException(Messages.CannotDeleteRecWithQtyCost);
            }
        }

        protected virtual void AMProdOvhd_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (RowIsEnableRestricted(e.Row))
            {
                //Disable all fields
                PXUIFieldAttribute.SetEnabled(cache, e.Row, false);

                //Then enable a select few if order is on hold...
                PXUIFieldAttribute.SetEnabled<AMProdOvhd.oFactor>(cache, null, OrderOnHold);
            }
        }
        #endregion

        public static void ProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list,
            string poReceiptType, string poReceiptNbr)
        {
            if (list == null)
            {
                return;
            }

            var prodOrder = new PXSelect<AMProdItem>(graph);
            Common.Cache.AddCacheView<AMProdItem>(graph);
            var prodMatl = new PXSelect<AMProdMatl>(graph);
            Common.Cache.AddCacheView<AMProdMatl>(graph);
            var prodMatlSplit = new PXSelect<AMProdMatlSplit>(graph);
            Common.Cache.AddCacheView<AMProdMatlSplit>(graph);
            var initemplan = new PXSelect<INItemPlan>(graph);

            var insertedSchedules = new List<AMProdMatlSplit>();
            var deleteSchedules = new List<AMProdMatlSplit>();
            var deletedPlans = new List<INItemPlan>();

            foreach (PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand> res in list)
            {
                var plan = PXCache<INItemPlan>.CreateCopy(res);
                var plantype = (INPlanType) res;

                //avoid ReadItem()
                if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
                {
                    initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);
                }

                AMProdMatlSplit origSchedule = PXSelect<AMProdMatlSplit, Where<AMProdMatlSplit.planID, Equal<Required<AMProdMatlSplit.planID>>>>.Select(graph, plan.DemandPlanID);

                if (origSchedule != null && (origSchedule.Completed == false || prodMatlSplit.Cache.GetStatus(origSchedule) == PXEntryStatus.Updated))
                {
                    var schedule = PXCache<AMProdMatlSplit>.CreateCopy(origSchedule);

                    schedule.BaseQtyReceived += plan.PlanQty;
                    schedule.QtyReceived = INUnitAttribute.ConvertFromBase(prodMatlSplit.Cache, schedule.InventoryID, schedule.UOM, schedule.BaseQtyReceived.GetValueOrDefault(), INPrecision.QUANTITY);

                    schedule = (AMProdMatlSplit)prodMatlSplit.Cache.Update(schedule);
                    var deleteOrigSchedule = schedule != null && schedule.BaseQtyReceived >= schedule.BaseQty;

                    INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.DemandPlanID);
                    if (origplan != null)
                    {
                        origplan.PlanQty = schedule.BaseQty - schedule.BaseQtyReceived;
                        initemplan.Cache.Update(origplan);
                    }

                    //select Allocated line if any, exclude allocated on Remote Whse
                    PXSelectBase<INItemPlan> cmd = new PXSelectJoin<INItemPlan, InnerJoin<AMProdMatlSplit, On<AMProdMatlSplit.planID, Equal<INItemPlan.planID>>>,
                        Where<INItemPlan.demandPlanID, Equal<Required<INItemPlan.demandPlanID>>,
                        And<AMProdMatlSplit.isAllocated, Equal<True>, And<AMProdMatlSplit.siteID, Equal<Required<AMProdMatlSplit.siteID>>>>>>(graph);
                    if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                    {
                        cmd.WhereAnd<Where<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>();
                    }
                    PXResult<INItemPlan> allocres = cmd.Select(plan.DemandPlanID, plan.SiteID, plan.LotSerialNbr);

                    if (allocres != null)
                    {
                        schedule = PXResult.Unwrap<AMProdMatlSplit>(allocres);
                        prodMatlSplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
                        schedule = PXCache<AMProdMatlSplit>.CreateCopy(schedule);
                        schedule.BaseQty += plan.PlanQty;
                        schedule.Qty = INUnitAttribute.ConvertFromBase(prodMatlSplit.Cache, schedule.InventoryID, schedule.UOM, schedule.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
                        schedule.POReceiptType = poReceiptType;
                        schedule.POReceiptNbr = poReceiptNbr;
                        schedule.InvtMult = 1;

                        prodMatlSplit.Cache.Update(schedule);

                        var allocplan = PXCache<INItemPlan>.CreateCopy(res);
                        allocplan.PlanQty += plan.PlanQty;

                        initemplan.Cache.Update(allocplan);

                        plantype = PXCache<INPlanType>.CreateCopy(plantype);
                        plantype.ReplanOnEvent = null;
                        plantype.DeleteOnEvent = true;
                    }
                    else
                    {
                        prodOrder.Current = (AMProdItem)PXParentAttribute.SelectParent(prodMatlSplit.Cache, schedule, typeof(AMProdItem));
                        prodMatl.Current = (AMProdMatl)PXParentAttribute.SelectParent(prodMatlSplit.Cache, schedule, typeof(AMProdMatl));
                        schedule = PXCache<AMProdMatlSplit>.CreateCopy(schedule);

                        ClearScheduleReferences(ref schedule);

                        //schedule.RefNoteID = origSchedule.RefNoteID;
                        schedule.VendorID = origSchedule.VendorID;
                        schedule.POOrderType = origSchedule.POOrderType;
                        schedule.POOrderNbr = origSchedule.POOrderNbr;
                        schedule.IsAllocated = plantype.ReplanOnEvent != INPlanConstants.Plan60;
                        schedule.LotSerialNbr = plan.LotSerialNbr;
                        schedule.POReceiptType = poReceiptType;
                        schedule.POReceiptNbr = poReceiptNbr;
                        schedule.SiteID = plan.SiteID;

                        schedule.BaseQtyReceived = 0m;
                        schedule.QtyReceived = 0m;
                        schedule.BaseQty = plan.PlanQty;
                        schedule.Qty = INUnitAttribute.ConvertFromBase(prodMatlSplit.Cache, schedule.InventoryID, schedule.UOM, schedule.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);

                        //update SupplyPlanID in existing item plans (replenishment)
                        foreach (PXResult<INItemPlan> demand_res in PXSelect<INItemPlan,
                            Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(graph, origSchedule.PlanID))
                        {
                            INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(demand_res);
                            initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);
                            demand_plan.SupplyPlanID = plan.PlanID;
                            initemplan.Cache.Update(demand_plan);
                        }

                        schedule.PlanID = plan.PlanID;

                        schedule = (AMProdMatlSplit)prodMatlSplit.Cache.Insert(schedule);
                        if (schedule != null)
                        {
                            insertedSchedules.Add(schedule);
                            if(deleteOrigSchedule)
                            {
                                deleteSchedules.Add(origSchedule);
                            }
                        }
                    }
                }
                else if (plan.DemandPlanID != null)
                {
                    //Original schedule was completed/plan record deleted by Cancel Order or Confirm Shipment
                    plantype = PXCache<INPlanType>.CreateCopy(plantype);
                    plantype.ReplanOnEvent = null;
                    plantype.DeleteOnEvent = true;
                }
                else
                {
                    //Original schedule Marked for PO
                    //TODO: verify this is sufficient for Original SO marked for TR.
                    var schedule = (AMProdMatlSplit)PXSelect<
                        AMProdMatlSplit,
                        Where<AMProdMatlSplit.planID, Equal<Required<AMProdMatlSplit.planID>>,
                            And<AMProdMatlSplit.completed, Equal<False>>>>
                        .Select(graph, plan.PlanID);
                    if (schedule != null)
                    {
                        prodMatlSplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
                        schedule = PXCache<AMProdMatlSplit>.CreateCopy(schedule);

                        schedule.Completed = true;
                        schedule.POCompleted = true;
                        schedule.PlanID = null;
                        //splitsToDeletePlanID.Add(schedule);
                        prodMatlSplit.Cache.Update(schedule);

                        INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.PlanID);
                        deletedPlans.Add(origplan);

                        initemplan.Cache.Delete(origplan);
                    }
                }

                if (plantype.ReplanOnEvent != null)
                {
                    //Using the "pre-allocated" plan type assumes sales allocated - here we want production allocated so we switch...
                    plan.PlanType = plantype.ReplanOnEvent == INPlanConstants.Plan61 ? INPlanConstants.PlanM7 : plantype.ReplanOnEvent;
                    plan.SupplyPlanID = null;
                    plan.DemandPlanID = null;
                    initemplan.Cache.Update(plan);
                }
                else if (plantype.DeleteOnEvent == true)
                {
                    initemplan.Delete(plan);
                }
            }

            //Create new schedules for partially received schedules marked for PO.
            AMProdMatlSplit prevSplit = null;
            foreach (var newsplit in insertedSchedules)
            {
                if (prevSplit != null && prevSplit.OrderType == newsplit.OrderType && prevSplit.ProdOrdID == newsplit.ProdOrdID
                    && prevSplit.OperationID == newsplit.OperationID && prevSplit.LineID == newsplit.LineID && prevSplit.InventoryID == newsplit.InventoryID
                    && prevSplit.SubItemID == newsplit.SubItemID && prevSplit.ParentSplitLineNbr == newsplit.ParentSplitLineNbr
                    && prevSplit.LotSerialNbr != null && newsplit.LotSerialNbr != null)
                {
                    continue;
                }

                AMProdMatlSplit parentschedule = PXSelect<
                    AMProdMatlSplit,
                    Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>,
                        And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>,
                        And<AMProdMatlSplit.operationID, Equal<Required<AMProdMatlSplit.operationID>>,
                        And<AMProdMatlSplit.lineID, Equal<Required<AMProdMatlSplit.lineID>>,
                        And<AMProdMatlSplit.splitLineNbr, Equal<Required<AMProdMatlSplit.parentSplitLineNbr>>>>>>>>
                    .Select(graph, newsplit.OrderType, newsplit.ProdOrdID, newsplit.OperationID, newsplit.LineID, newsplit.ParentSplitLineNbr);

                if (parentschedule != null && parentschedule.Completed == true && parentschedule.POCompleted == true && parentschedule.BaseQty > parentschedule.BaseQtyReceived
                            && deletedPlans.Exists(x => x.PlanID == parentschedule.PlanID))
                {
                    prodOrder.Current = (AMProdItem)PXParentAttribute.SelectParent(prodMatlSplit.Cache, parentschedule, typeof(AMProdItem));

                    parentschedule = PXCache<AMProdMatlSplit>.CreateCopy(parentschedule);
                    var demand = PXCache<INItemPlan>.CreateCopy(deletedPlans.First(x => x.PlanID == parentschedule.PlanID));

                    UpdateSchedulesFromCompletedPO(prodMatlSplit, initemplan, parentschedule, prodOrder, demand);
                }
                prevSplit = newsplit;
            }

            foreach (var deleteSchedule in deleteSchedules)
            {
                prodMatlSplit.Cache.Delete(deleteSchedule);
            }
        }

        public static void ProcessPOOrder(PXGraph graph, POOrder poOrder)
        {
            if (poOrder == null)
            {
                return;
            }

            var prodOrder = new PXSelect<AMProdItem>(graph);
            Common.Cache.AddCacheView<AMProdItem>(graph);
            //var prodMatl = new PXSelect<AMProdMatl>(graph);
            //Common.Cache.AddCacheView<AMProdMatl>(graph);
            var prodMatlSplit = new PXSelect<AMProdMatlSplit>(graph);
            Common.Cache.AddCacheView<AMProdMatlSplit>(graph);
            var initemplan = new PXSelect<INItemPlan>(graph);
            
            //Search for completed/cancelled POLines with uncompleted linked schedules
            foreach (PXResult<POLine, INItemPlan, AMProdMatlSplit, INPlanType> res in PXSelectJoin<POLine,
                InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<POLine.planID>>,
                InnerJoin<AMProdMatlSplit, On<AMProdMatlSplit.planID, Equal<INItemPlan.planID>, And<AMProdMatlSplit.pOOrderType, Equal<POLine.orderType>, And<AMProdMatlSplit.pOOrderNbr, Equal<POLine.orderNbr>, And<AMProdMatlSplit.pOLineNbr, Equal<POLine.lineNbr>>>>>,
                InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>>>>,
            Where<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>, And2<Where<POLine.cancelled, Equal<boolTrue>, Or<POLine.completed, Equal<boolTrue>>>,
                And<AMProdMatlSplit.qtyReceived, Less<AMProdMatlSplit.qty>, And<AMProdMatlSplit.pOCancelled, NotEqual<boolTrue>, And<AMProdMatlSplit.completed, NotEqual<boolTrue>>>>>>>>.Select(graph, poOrder.OrderType, poOrder.OrderNbr))
            {
                POLine poline = res;
                INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res);
                AMProdMatlSplit parentschedule = PXCache<AMProdMatlSplit>.CreateCopy(res);

                prodOrder.Current = (AMProdItem)PXParentAttribute.SelectParent(prodMatlSplit.Cache, parentschedule, typeof(AMProdItem));

                //partially received Drop-Ships are skipped
                if (parentschedule.Completed != true && parentschedule.POCancelled != true && parentschedule.BaseQty > parentschedule.BaseQtyReceived && !(poline.LineType == POLineType.GoodsForDropShip && poline.BaseReceivedQty != 0m))
                {
                    UpdateSchedulesFromCompletedPO(prodMatlSplit, initemplan, parentschedule, prodOrder, plan);

                    if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
                    {
                        initemplan.Delete(plan);
                    }

                    prodMatlSplit.Cache.SetStatus(parentschedule, PXEntryStatus.Notchanged);
                    parentschedule = PXCache<AMProdMatlSplit>.CreateCopy(parentschedule);

                    parentschedule.PlanID = null;
                    parentschedule.Completed = true;
                    parentschedule.POCompleted = true;
                    parentschedule.POCancelled = true;
                    prodMatlSplit.Cache.Update(parentschedule);
                }
            }
        }

        private static void UpdateSchedulesFromCompletedPO(PXSelect<AMProdMatlSplit> prodMatlSplit, PXSelect<INItemPlan> initemplan,
            AMProdMatlSplit parentschedule, PXSelect<AMProdItem> prodOrder, INItemPlan demand)
        {
            var newschedule = PXCache<AMProdMatlSplit>.CreateCopy(parentschedule);

            ClearScheduleReferences(ref newschedule);

            newschedule.LotSerialNbr = demand.LotSerialNbr;
            newschedule.SiteID = demand.SiteID;

            newschedule.BaseQty = parentschedule.BaseQty - parentschedule.BaseQtyReceived;
            newschedule.Qty = INUnitAttribute.ConvertFromBase(prodMatlSplit.Cache, newschedule.InventoryID, newschedule.UOM, (decimal)newschedule.BaseQty, INPrecision.QUANTITY);
            newschedule.BaseQtyReceived = 0m;
            newschedule.QtyReceived = 0m;

            //creating new plan
            var newPlan = PXCache<INItemPlan>.CreateCopy(demand);
            newPlan.PlanID = null;
            newPlan.SupplyPlanID = null;
            newPlan.DemandPlanID = null;
            newPlan.PlanQty = newschedule.BaseQty;
            newPlan.VendorID = null;
            newPlan.VendorLocationID = null;
            newPlan.FixedSource = INReplenishmentSource.None;
            newPlan.PlanType = prodOrder.Current != null && (prodOrder.Current.Hold == true || prodOrder.Current.StatusID == ProductionOrderStatus.Planned) ? INPlanConstants.PlanM5 : INPlanConstants.PlanM6;
            newPlan = (INItemPlan)initemplan.Cache.Insert(newPlan);

            newschedule.PlanID = newPlan.PlanID;
            prodMatlSplit.Cache.Insert(newschedule);
        }

        public static void ClearScheduleReferences(ref AMProdMatlSplit schedule)
        {
            schedule.ParentSplitLineNbr = schedule.SplitLineNbr;
            schedule.SplitLineNbr = null;
            schedule.Completed = false;
            schedule.PlanID = null;
            //clear PO references
            schedule.POCompleted = false;
            schedule.POCancelled = false;

            schedule.POCreate = false;
            schedule.VendorID = null;
            schedule.POOrderType = null;
            schedule.POOrderNbr = null;
            schedule.POLineNbr = null;
            schedule.POReceiptType = null;
            schedule.POReceiptNbr = null;
            schedule.RefNoteID = null;
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMProdItemSplitPlanID(typeof(AMProdItem.noteID), typeof(AMProdItem.hold))]
        protected virtual void AMProdItemSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMProdMatlSplitPlanID(typeof(AMProdMatl.noteID), typeof(AMProdItem.hold))]
        protected virtual void AMProdMatlSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        /// <summary>
        /// Copy work center level overheads to the production order operation
        /// </summary>
        protected virtual void CopyOverheadsFromWorkCenter(AMProdOper row)
        {
            CopyOverheadsFromWorkCenter(this, row);
        }

        /// <summary>
        /// Copy work center level overheads to the production order operation
        /// </summary>
        public static void CopyOverheadsFromWorkCenter(PXGraph graph, AMProdOper row)
        {
            if (row == null)
            {
                return;
            }

            foreach (AMWCOvhd amWcOvhd in PXSelect<
                AMWCOvhd,
                Where<AMWCOvhd.wcID, Equal<Required<AMWCOvhd.wcID>>>>
                .Select(graph, row?.WcID))
            {
                if (string.IsNullOrWhiteSpace(amWcOvhd?.OvhdID))
                {
                    continue;
                }

                var newProdOvhd = new AMProdOvhd
                {
                    OrderType = row.OrderType,
                    ProdOrdID = row.ProdOrdID,
                    OperationID = row.OperationID,
                    OFactor = amWcOvhd.OFactor,
                    OvhdID = amWcOvhd.OvhdID,
                    WCFlag = true
                };

                graph.Caches<AMProdOvhd>().Insert(newProdOvhd);
            }
        }

        /// <summary>
        /// Remove work center level overheads from the production order operation
        /// </summary>
        protected virtual void DeleteWorkCenterRelatedOverhead(AMProdOper row)
        {
            DeleteWorkCenterRelatedOverhead(this, row);
        }

        /// <summary>
        /// Remove work center level overheads from the production order operation
        /// </summary>
        public static void DeleteWorkCenterRelatedOverhead(PXGraph graph, AMProdOper row)
        {
            if (row == null)
            {
                return;
            }

            foreach (AMProdOvhd amProdOvhd in PXSelect<
                AMProdOvhd,
                Where<AMProdOvhd.wCFlag, Equal<True>,
                    And<AMProdOvhd.orderType, Equal<Required<AMProdOvhd.orderType>>,
                    And<AMProdOvhd.prodOrdID, Equal<Required<AMProdOvhd.prodOrdID>>,
                    And<AMProdOvhd.operationID, Equal<Required<AMProdOvhd.operationID>>>>>>>
                .Select(graph, row?.OrderType, row?.ProdOrdID, row?.OperationID))
            {
                if (amProdOvhd?.OvhdID == null)
                {
                    continue;
                }

                graph.Caches<AMProdOvhd>().Delete(amProdOvhd);
            }
        }

        public static void ProcessPOCreate(ProdDetail pdGraph, List<AMProdMatl> list, int? vendorID, int? vendorLocationID)
        {
            if (vendorID == null || vendorLocationID == null)
            {
                return;
            }

            var newProdMatl = new List<AMProdMatl>();
            var updated = false;
            var poGraph = PXGraph.CreateInstance<POCreate>();
            poGraph.Clear(PXClearOption.ClearAll);
            var demandList = new List<POFixedDemand>();

            foreach(var prodMatl in list)
            {

                if(prodMatl == null)
                {
                    return;
                }

                updated = true;

                AMProdMatlSplit split = PXSelect<AMProdMatlSplit,
                     Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>,
                        And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>,
                        And<AMProdMatlSplit.operationID, Equal<Required<AMProdMatlSplit.operationID>>,
                        And<AMProdMatlSplit.lineID, Equal<Required<AMProdMatlSplit.lineID>>>>>
                    >>.Select(pdGraph, prodMatl.OrderType, prodMatl.ProdOrdID, prodMatl.OperationID, prodMatl.LineID);

                if(split == null)
                {
                    continue;
                }

#if DEBUG
                AMDebug.TraceWriteMethodName($"Material split planid {split.PlanID}. POCreate = {split.POCreate}");
#endif
                newProdMatl.Add(prodMatl);

                if (!split.POCreate.GetValueOrDefault())
                {
                    updated = true;
                    split.POCreate = true;
                    pdGraph.ProdMatlSplits.Update(split);
                }

                POFixedDemand demand = PXSelectReadonly<POFixedDemand,
                    Where<POFixedDemand.planID, Equal<Required<POFixedDemand.planID>>>>.Select(poGraph, split.PlanID);
                if (demand?.PlanID == null)
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"Adding fixed demand material planid {demand.PlanID}");
#endif
                demand.Selected = true;
                demand.VendorID = vendorID;
                demand.VendorLocationID = vendorLocationID;

                demandList.Add(poGraph.FixedDemand.Update(demand));
            }

            //Is dirty is always true
            if (updated)
            {
                pdGraph.Actions.PressSave();
            }

            var purchaseDate = poGraph.Accessinfo.BusinessDate;
            POCreateAMExtension.POCreatePOOrders(demandList, purchaseDate);
        }

        public virtual void LinkSupplyDemand()
        {
            List<AMProdMatlSplit> ProdMatlSplits = this.ProdMatlSplits.Select().RowCast<AMProdMatlSplit>().ToList();

            //unlink first
            bool removedLink = false;
            foreach (POLineMatl supply in posupply.Cache.Updated)
            {
                AMProdMatl line = (AMProdMatl)currentposupply.Select(supply.AMProdMatlOrderType, supply.AMProdMatlProdOrdID, supply.AMProdMatlOperationID, supply.AMProdMatlLineNbr);

                line = PXCache<AMProdMatl>.CreateCopy(line);

                if (supply.Selected == false && supply.AMProdMatlSplitLineNbr != null)
                {
                    foreach (AMProdMatlSplit split in ProdMatlSplits)
                    {
                        if (this.ProdMatlSplits.Cache.GetStatus(split) == PXEntryStatus.Deleted || this.ProdMatlSplits.Cache.GetStatus(split) == PXEntryStatus.InsertedDeleted)
                            continue;

                        if (split.POOrderType == supply.OrderType && split.POOrderNbr == supply.OrderNbr && split.POLineNbr == supply.LineNbr &&
                            split.POCompleted == false && split.Completed == false) // || supply.OrderType == POOrderType.DropShip
                        {
                            if (split.POOrderType != null && split.POOrderNbr != null && split.POOrderType == supply.OrderType && split.POOrderNbr == supply.OrderNbr)
                            {
                                POOrder poorder = PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                                        And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(this, supply.OrderType, supply.OrderNbr);

                                if (poorder != null && poorder.OrderType == split.POOrderType && poorder.OrderNbr == split.POOrderNbr)
                                {
                                    if (split.RefNoteID == poorder.NoteID)
                                        split.RefNoteID = null;
                                    if (poorder.SOOrderType == split.OrderType && poorder.SOOrderNbr == split.ProdOrdID)
                                    {
                                        poorder.SOOrderType = null;
                                        poorder.SOOrderNbr = null;
                                        poorderlink.Update(poorder);
                                    }
                                }
                            }
                            if (split.POOrderType != null)
                            {
                                ClearPOReferences(split);
                                
                                split.POCompleted = false;
                                removedLink = true;
                            }

                            split.QtyReceived = 0m;
                            //split.ShippedQty = 0m;
                            split.Completed = false;

                            this.ProdMatlSplits.Update(split);
                                                        
                        }
                    }
                   supply.AMProdMatlSplitLineNbr = null;
                }
            }

            //then link
            bool addedLink = false;
            foreach (POLineMatl supply in posupply.Cache.Updated)
            {
                AMProdMatl line = (AMProdMatl)currentposupply.Select(supply.AMProdMatlOrderType, supply.AMProdMatlProdOrdID, supply.AMProdMatlOperationID,
                    supply.AMProdMatlLineNbr);

                line = PXCache<AMProdMatl>.CreateCopy(line);

                if (supply.Selected == true && supply.AMProdMatlSplitLineNbr == null)
                {
                    decimal? BaseOpenQty = supply.BaseOrderQty - supply.DemandQty;

                    for (int i = 0; i < ProdMatlSplits.Count; i++)
                    {
                        AMProdMatlSplit split = PXCache<AMProdMatlSplit>.CreateCopy(ProdMatlSplits[i]);

                        //TODO: it should not be possible to unallocate TR schedules
                        if (string.IsNullOrEmpty(split.POOrderNbr) && split.IsAllocated == false && split.Completed == false && split.BaseQty > 0m)
                        {
                            if (supply.OrderType != POOrderType.Blanket)
                            {
                                supply.LineType = POLineType.GoodsForManufacturing;
                            }

                            supply.AMProdMatlSplitLineNbr = split.SplitLineNbr;

                            INItemPlan plan;
                            if (supply.Completed == false)
                            {
                                plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this, supply.PlanID);
                                if (plan == null) continue;
                                if (supply.OrderType != PX.Objects.PO.POOrderType.Blanket)
                                {
                                    plan = PXCache<INItemPlan>.CreateCopy(plan);
                                    plan.PlanType = INPlanConstants.PlanM3;
                                    this.Caches[typeof(INItemPlan)].Update(plan);
                                }
                                                                
                            }

                            plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this, split.PlanID);

                            plan = PXCache<INItemPlan>.CreateCopy(plan);

                            plan.PlanType = INPlanConstants.PlanM4;
                            //plan.PlanType = (supply.OrderType == PO.POOrderType.Blanket) ?
                            //    (line.POSource == INReplenishmentSource.PurchaseToOrder ? INPlanConstants.Plan6B : INPlanConstants.Plan6E) :
                            //    (line.POSource == INReplenishmentSource.DropShipToOrder ? INPlanConstants.Plan6D : INPlanConstants.Plan66);

                            plan.FixedSource = INReplenishmentSource.Purchased;
                            plan.SupplyPlanID = supply.PlanID;

                            POOrder poorder = PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                                    And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(this, supply.OrderType, supply.OrderNbr);

                            if (poorder != null)
                            {
                                plan.VendorID = poorder.VendorID;
                                plan.VendorLocationID = poorder.VendorLocationID;
                            }
                            this.Caches[typeof(INItemPlan)].Update(plan);
                            
                            split.POCreate = true;
                            split.VendorID = supply.VendorID;
                            split.POOrderType = supply.OrderType;
                            split.POOrderNbr = supply.OrderNbr;
                            split.POLineNbr = supply.LineNbr;
                            addedLink = true;

                            if (split.BaseQty <= BaseOpenQty)
                            {
                                BaseOpenQty -= split.BaseQty;
                                split = this.ProdMatlSplits.Update(split);
                            }
                            else
                            {
                                AMProdMatlSplit copy = PXCache<AMProdMatlSplit>.CreateCopy(split);

                                copy.SplitLineNbr = null;
                                copy.IsAllocated = false;

                                ClearPOFlags(split);
                                ClearPOReferences(split);
                                
                                copy.VendorID = null;
                                copy.POCreate = true;

                                copy.BaseQty = copy.BaseQty - BaseOpenQty;
                                copy.Qty = INUnitAttribute.ConvertFromBase(this.ProdMatlSplits.Cache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
                                copy.QtyReceived = 0m;
                                copy.PlanID = null;
                                copy.Completed = false;

                                split.BaseQty = BaseOpenQty;
                                split.Qty = INUnitAttribute.ConvertFromBase(this.ProdMatlSplits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                                BaseOpenQty = 0m;
                                split = this.ProdMatlSplits.Update(split);

                                if ((copy = this.ProdMatlSplits.Insert(copy)) != null)
                                {
                                    ProdMatlSplits.Insert(i + 1, copy);
                                }
                            }
                            ProdMatlSplits[i] = split;
                       }
                       if (BaseOpenQty <= 0m) break;
                    }
                }
                else if (supply.Selected != true)
                {
                    posupply.Cache.SetStatus(supply, PXEntryStatus.Notchanged);
                    posupply.Cache.Remove(supply);
                }
            }

            var matlLine = ProdMatlRecords.Current;
            if (addedLink)
            {
                if (matlLine.POCreate != true)
                {
                    ProdMatlRecords.Cache.SetValue<AMProdMatl.pOCreate>(matlLine, true);
                }
            }
            else if (removedLink)
            {
                if (matlLine.POCreate == true)
                {
                    var linked = ProdMatlSplits.Any(x => x.POCreate == true && x.POOrderNbr != null);
                    if (!linked)
                    {
                        ProdMatlRecords.Cache.SetValue<AMProdMatl.pOCreate>(matlLine, linked);
                    }
                }
            }
        }

        private struct POProdMatlSupplyResult
        {
            public string POOrderType;
            public string POOrderNbr;
            public int? POLineNbr;
            public POLineMatl POLine;
            public POOrder POOrder;
            public List<AMProdMatlSplit> CurrentAMProdMatlSplits;
            public List<AMProdMatlSplit> ForeignAMProdMatlSplits;
        }

        public virtual IEnumerable POSupply()
        {
            AMProdMatl matlLine = (AMProdMatl)currentposupply.Select();

            List<POLineMatl> ret = new List<POLineMatl>();
            if (matlLine == null) return ret;

            List<POProdMatlSupplyResult> mergedResults = new List<POProdMatlSupplyResult>();

            foreach (PXResult<POLineMatl, PX.Objects.PO.POOrder, AMProdMatlSplit> res in
                PXSelectReadonly2<POLineMatl,
                InnerJoin<PX.Objects.PO.POOrder, On<PX.Objects.PO.POOrder.orderNbr, Equal<POLineMatl.orderNbr>>,
                LeftJoin<AMProdMatlSplit, On<AMProdMatlSplit.pOOrderType, Equal<POLineMatl.orderType>,
                    And<AMProdMatlSplit.pOOrderNbr, Equal<POLineMatl.orderNbr>,
                    And<AMProdMatlSplit.pOLineNbr, Equal<POLineMatl.lineNbr>>>>>>,
                Where<POLineMatl.orderType, Equal<PX.Objects.PO.POOrderType.regularOrder>,
                And2<Where<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.goodsForInventory>, //we should change this to In3<> once it is available
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.nonStock>,
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.goodsForManufacturing>,
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.nonStockForManufacturing>,
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.goodsForSalesOrder>,
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.goodsForServiceOrder>,
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.nonStockForSalesOrder>,
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.nonStockForServiceOrder>,
                    Or<POLineMatl.lineType, Equal<PX.Objects.PO.POLineType.goodsForReplenishment>>>>>>>>>>,
                And2<Where<POLineMatl.inventoryID, Equal<Required<AMProdMatl.inventoryID>>,
                And2<Where<Required<AMProdMatl.subItemID>, IsNull,
                        Or<POLineMatl.subItemID, Equal<Required<AMProdMatl.subItemID>>>>,
                    And<POLineMatl.siteID, Equal<Required<AMProdMatl.siteID>>>>>,
                And<Where<Required<AMProdMatl.vendorID>, IsNull,
                    Or<POLineMatl.vendorID, Equal<Required<AMProdMatl.vendorID>>>>
                >>>>>.Select(this, matlLine?.InventoryID, matlLine?.SubItemID, matlLine?.SubItemID, matlLine?.SiteID, matlLine?.VendorID, matlLine?.VendorID))
            {
                POLineMatl supply = PXCache<POLineMatl>.CreateCopy(res);
                POOrder poorder = (POOrder)this.Caches[typeof(POOrder)].CreateCopy(this.Caches[typeof(POOrder)].Locate((POOrder)res)) ?? res;
                AMProdMatlSplit split = PXCache<AMProdMatlSplit>.CreateCopy(res);
                AMProdMatlSplit foreignsplit = new AMProdMatlSplit();

                AMProdMatlSplit selectedSplitCached = (AMProdMatlSplit)this.Caches[typeof(AMProdMatlSplit)].Locate((AMProdMatlSplit)res);

                if (selectedSplitCached != null)
                {
                    //selected split found in cache, replace selected plan with cached plan
                    if (selectedSplitCached.POOrderNbr == null || selectedSplitCached.POOrderNbr != supply.OrderNbr ||
                        this.Caches[typeof(AMProdMatlSplit)].GetStatus(selectedSplitCached) == PXEntryStatus.Deleted)
                    {
                        split = new AMProdMatlSplit();
                    }
                    else
                    {
                        split = (AMProdMatlSplit)this.Caches[typeof(AMProdMatlSplit)].CreateCopy(selectedSplitCached);
                    }
                }

                if (split.POOrderNbr == null)
                {
                    split = new AMProdMatlSplit();
                }
                else if (split.OrderType != matlLine.OrderType || split.ProdOrdID != matlLine.ProdOrdID || split.LineID != matlLine.LineNbr)
                {
                    foreignsplit = (AMProdMatlSplit)this.Caches[typeof(AMProdMatlSplit)].CreateCopy(split);
                    split = new AMProdMatlSplit();
                }

                POProdMatlSupplyResult result = new POProdMatlSupplyResult
                {
                    POOrderType = supply.OrderType,
                    POOrderNbr = supply.OrderNbr,
                    POLineNbr = supply.LineNbr,
                    POLine = supply,
                    POOrder = poorder,
                    CurrentAMProdMatlSplits = split.SplitLineNbr != null ? new List<AMProdMatlSplit> { split } : new List<AMProdMatlSplit> { },
                    ForeignAMProdMatlSplits = foreignsplit.SplitLineNbr != null ? new List<AMProdMatlSplit> { foreignsplit } : new List<AMProdMatlSplit> { }
                };

                POProdMatlSupplyResult existingResult = mergedResults.FirstOrDefault(x => x.POOrderType == result.POOrderType && x.POOrderNbr == result.POOrderNbr && x.POLineNbr == result.POLineNbr);
                if (existingResult.POOrderNbr != null)
                {
                    if (!existingResult.CurrentAMProdMatlSplits.Any(x => x.OrderType == split.OrderType && x.ProdOrdID == split.ProdOrdID && x.LineID == split.LineID 
                        && x.SplitLineNbr == split.SplitLineNbr))
                    {
                        existingResult.CurrentAMProdMatlSplits.Add(split);
                    }
                    if (!existingResult.ForeignAMProdMatlSplits.Any(x => x.OrderType == foreignsplit.OrderType && x.ProdOrdID == foreignsplit.ProdOrdID 
                        && x.LineID == foreignsplit.LineID && x.SplitLineNbr == foreignsplit.SplitLineNbr))
                    {
                        existingResult.ForeignAMProdMatlSplits.Add(foreignsplit);
                    }
                }
                else
                {
                    mergedResults.Add(result);
                }
            }

            bool allSplitsCompleted = true;
            foreach (AMProdMatlSplit split in ProdMatlSplits.Select())
            {
                if (split.Completed != true)
                {
                    allSplitsCompleted = false;
                    break;
                }
            }
            //searching for other matching splits in cache and checking if all splits completed
            foreach (AMProdMatlSplit splitFromCache in PXSelect<AMProdMatlSplit, Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>, 
                And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>>
                >>.Select(this, matlLine.OrderType, matlLine.ProdOrdID).Where(x => ((AMProdMatlSplit)x).POOrderNbr != null))
            {
                POProdMatlSupplyResult existingResult = mergedResults.FirstOrDefault(x => x.POOrderType == splitFromCache.POOrderType && x.POOrderNbr == splitFromCache.POOrderNbr 
                    && x.POLineNbr == splitFromCache.POLineNbr);
                if (existingResult.POOrderNbr != null && splitFromCache.POOrderType == existingResult.POOrderType && splitFromCache.POOrderNbr == existingResult.POOrderNbr 
                    && splitFromCache.POLineNbr == existingResult.POLineNbr)
                {
                    if (splitFromCache.LineID == matlLine.LineNbr)
                    {
                        //matching splits for current AMProdMatl
                        if (!existingResult.CurrentAMProdMatlSplits.Any(x => x.SplitLineNbr == splitFromCache.SplitLineNbr))
                        {
                            existingResult.CurrentAMProdMatlSplits.Add((AMProdMatlSplit)this.Caches[typeof(AMProdMatlSplit)].CreateCopy(splitFromCache));
                        }
                    }
                    else
                    {
                        //matching splits for other AMProdMatl
                        if (!existingResult.ForeignAMProdMatlSplits.Any(x => x.LineID == splitFromCache.LineID && x.SplitLineNbr == splitFromCache.SplitLineNbr))
                        {
                            existingResult.ForeignAMProdMatlSplits.Add((AMProdMatlSplit)this.Caches[typeof(AMProdMatlSplit)].CreateCopy(splitFromCache));
                        }
                    }
                }
            }

            foreach (POProdMatlSupplyResult res in mergedResults)
            {
                POLineMatl supply = PXCache<POLineMatl>.CreateCopy(res.POLine);
                POOrder poorder = res.POOrder;

                decimal demandQty = 0m;
                AMProdMatlSplit linkWithCurrentDocument = null;
                foreach (AMProdMatlSplit split in res.CurrentAMProdMatlSplits)
                {
                    if (split.POOrderNbr != null)
                    {
                        if (split.PlanID != null && split.POCompleted != true)
                        {
                            demandQty += (split.BaseQty ?? 0m) - (split.BaseQtyReceived ?? 0m);
                        }

                        if (linkWithCurrentDocument == null)
                        {
                            linkWithCurrentDocument = split;
                        }
                    }
                }
                bool linkedWithCurrentDocument = linkWithCurrentDocument != null;
                //bool linkedWithForeignDocument = false;

                foreach (AMProdMatlSplit foreignsplit in res.ForeignAMProdMatlSplits)
                {
                    if (foreignsplit.POOrderNbr != null)
                    {
                        if (foreignsplit.PlanID != null && foreignsplit.POCompleted != true)
                        {
                            demandQty += (foreignsplit.BaseQty ?? 0m) - (foreignsplit.BaseQtyReceived ?? 0m);
                        }

                        //linkedWithForeignDocument = true;
                    }
                }

                if (!linkedWithCurrentDocument && (poorder.Hold == true || allSplitsCompleted || matlLine.BaseQtyRemaining == 0) &&
                    (matlLine.StatusID != ProductionOrderStatus.Released || matlLine.StatusID != ProductionOrderStatus.InProcess))
                {
                    continue;
                }

                if (linkedWithCurrentDocument || (supply.Completed == false && supply.Cancelled == false && supply.BaseOpenQty - demandQty > 0m))
                {
                    supply.Selected = linkedWithCurrentDocument;
                    supply.AMProdMatlOrderType = matlLine.OrderType;
                    supply.AMProdMatlProdOrdID = matlLine.ProdOrdID;
                    supply.AMProdMatlOperationID = matlLine.OperationID;
                    supply.AMProdMatlLineNbr = matlLine.LineID;
                    supply.AMProdMatlSplitLineNbr = linkedWithCurrentDocument ? linkWithCurrentDocument.SplitLineNbr : null;
                    supply.VendorRefNbr = poorder.VendorRefNbr;
                    supply.DemandQty = demandQty;

                    POLineMatl cached = posupply.Locate(supply);

                    //posupply cache should be kept in an up-to-date state. 
                    //Cached records will be updated with values from the current AMProdMatlLine.
                    //This cache is lately used in LinkSupplyDemand()
                    if (cached == null || (cached.Selected == supply.Selected && cached.Selected != true && cached.AMProdMatlLineNbr != supply.AMProdMatlLineNbr))
                    {
                        cached = (POLineMatl)posupply.Update(supply);
                        if (posupply.Cache.GetStatus(cached) == PXEntryStatus.Updated)
                        {
                            posupply.Cache.SetStatus(cached, PXEntryStatus.Held);
                        }
                    }
                    ret.Add(cached);
                }
            }
            return ret;
        }

        protected virtual void POLineMatl_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            POLineMatl supply = e.Row as POLineMatl;

            if (supply == null)
            {
                return;
            }

            AMProdMatl matlLine = PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Current<POLineMatl.aMProdMatlOrderType>>, 
                And<AMProdMatl.prodOrdID, Equal<Current<POLineMatl.aMProdMatlProdOrdID>>, 
                And<AMProdMatl.operationID, Equal<Current<POLineMatl.aMProdMatlOperationID>>,
                And<AMProdMatl.lineID, Equal<Current<POLineMatl.aMProdMatlLineNbr>>>>>>>.Select(this);

            bool RegularOrderMinMax = supply != null && matlLine != null && supply.BaseOpenQty - supply.DemandQty >= matlLine.BaseQtyRemaining;
                
            bool PartiallyReceipted = supply != null && matlLine != null && supply.Selected == true && supply.BaseOrderQty - supply.BaseOpenQty > 0
                && supply.AMProdMatlSplitLineNbr != null;

            PXUIFieldAttribute.SetEnabled<POLineMatl.selected>(sender, e.Row, (RegularOrderMinMax || supply.AMProdMatlSplitLineNbr != null) && !PartiallyReceipted);

            if (PartiallyReceipted)
            {
                PXUIFieldAttribute.SetWarning<POLineMatl.selected>(sender, e.Row, PX.Objects.SO.Messages.PurchaseOrderCannotBeDeselected);
            }
        }

        public virtual void ClearPOReferences(AMProdMatlSplit split) 
        {
            split.POOrderType = null;
            split.POOrderNbr = null;
            split.POLineNbr = null;

            split.POReceiptType = null;
            split.POReceiptNbr = null;
        }

        public virtual void ClearPOFlags(AMProdMatlSplit split)
        {
            split.POCompleted = false;
            split.POCancelled = false;

            split.POCreate = false;
            //split.POSource = null;
        }

        #region HANDLE HOLD CHECKBOX
        protected virtual void AMProdItem_Hold_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var api = (AMProdItem)e.Row;

            if (string.IsNullOrEmpty(api?.StatusID) || ProductionOrderStatus.CanHold(api.StatusID) || !(bool)e.NewValue)
            {
                return;
            }

            throw new PXSetPropertyException(Messages.GetLocal(Messages.ProdStatusHoldInvalid, ProductionOrderStatus.GetStatusDescription(api.StatusID)), PXErrorLevel.Warning);
        }

        protected virtual void AMProdItem_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMProdItem)e.Row;
            if (row == null || e.OldValue == null || row.Hold.GetValueOrDefault() == (bool)e.OldValue)
            {
                return;
            }

            if (row.Hold.GetValueOrDefault())
            {
                //To Hold
                ProductionEventHelper.InsertStatusEvent(this, row, row.StatusID, ProductionOrderStatus.Hold);
                return;
            }

            //From Hold
            ProductionEventHelper.InsertStatusEvent(this, row, ProductionOrderStatus.Hold, row.StatusID);
        }

        #endregion

    }

    [Serializable()]
    [PXProjection(typeof(Select<POLine>), Persistent = true)]
    [PXHidden]
    public partial class POLineMatl : PX.Data.IBqlTable, ISortOrder
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        protected String _OrderType;
        [PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POLine.orderType))]
        [PXDefault()]
        [PXUIField(DisplayName = "PO Type", Enabled = false)]
        [PX.Objects.PO.POOrderType.List()]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
        protected String _OrderNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POLine.orderNbr))]
        [PXDefault()]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<POLineMatl.orderType>>>>), DescriptionField = typeof(POOrder.orderDesc))]
        public virtual String OrderNbr
        {
            get
            {
                return this._OrderNbr;
            }
            set
            {
                this._OrderNbr = value;
            }
        }
        #endregion
        #region VendorRefNbr
        public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
        protected String _VendorRefNbr;
        [PXString(40)]
        [PXUIField(DisplayName = "Vendor Ref.", Enabled = false)]
        public virtual String VendorRefNbr
        {
            get
            {
                return this._VendorRefNbr;
            }
            set
            {
                this._VendorRefNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(POLine.lineNbr))]
        [PXDefault()]
        [PXUIField(DisplayName = "PO Line Nbr.", Visible = false)]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
        protected Int32? _SortOrder;
        [PXDBInt(BqlField = typeof(POLine.sortOrder))]
        public virtual Int32? SortOrder
        {
            get
            {
                return this._SortOrder;
            }
            set
            {
                this._SortOrder = value;
            }
        }
        #endregion
        #region LineType
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
        protected String _LineType;
        [PXDBString(2, IsFixed = true, BqlField = typeof(POLine.lineType))]
        [PX.Objects.PO.POLineType.List()]
        [PXUIField(DisplayName = "Line Type", Enabled = false)]
        public virtual String LineType
        {
            get
            {
                return this._LineType;
            }
            set
            {
                this._LineType = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [Inventory(Filterable = true, BqlField = typeof(POLine.inventoryID), Enabled = false)]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [SubItem(BqlField = typeof(POLine.subItemID), Enabled = false)]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
        protected Int64? _PlanID;
        [PXDBLong(BqlField = typeof(POLine.planID))]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual Int64? PlanID
        {
            get
            {
                return this._PlanID;
            }
            set
            {
                this._PlanID = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [PX.Objects.AP.Vendor(typeof(Search<BAccountR.bAccountID,
            Where<Vendor.type, NotEqual<BAccountType.employeeType>>>),
            BqlField = typeof(POLine.vendorID), Enabled = false)]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region OrderDate
        public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
        protected DateTime? _OrderDate;
        [PXDBDate(BqlField = typeof(POLine.orderDate))]
        [PXUIField(DisplayName = "Order Date", Enabled = false)]
        public virtual DateTime? OrderDate
        {
            get
            {
                return this._OrderDate;
            }
            set
            {
                this._OrderDate = value;
            }
        }
        #endregion
        #region PromisedDate
        public abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }
        protected DateTime? _PromisedDate;
        [PXDBDate(BqlField = typeof(POLine.promisedDate))]
        [PXUIField(DisplayName = "Promised", Enabled = false)]
        public virtual DateTime? PromisedDate
        {
            get
            {
                return this._PromisedDate;
            }
            set
            {
                this._PromisedDate = value;
            }
        }
        #endregion
        #region Cancelled
        public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
        protected Boolean? _Cancelled;
        [PXDBBool(BqlField = typeof(POLine.cancelled))]
        public virtual Boolean? Cancelled
        {
            get
            {
                return this._Cancelled;
            }
            set
            {
                this._Cancelled = value;
            }
        }
        #endregion
        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
        [PXDBBool(BqlField = typeof(POLine.completed))]
        public virtual bool? Completed
        {
            get;
            set;
        }
        #endregion
        #region Closed
        public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
        [PXDBBool(BqlField = typeof(POLine.closed))]
        public virtual bool? Closed
        {
            get;
            set;
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [PXDBInt(BqlField = typeof(POLine.siteID))]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        protected String _UOM;
        [PXDBString(6, IsUnicode = true, BqlField = typeof(POLine.uOM))]
        [PXUIField(DisplayName = "UOM", Enabled = false)]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region OrderQty
        public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
        protected Decimal? _OrderQty;
        [PXDBQuantity(BqlField = typeof(POLine.orderQty))]
        [PXUIField(DisplayName = "Order Qty.", Enabled = false)]
        public virtual Decimal? OrderQty
        {
            get
            {
                return this._OrderQty;
            }
            set
            {
                this._OrderQty = value;
            }
        }
        #endregion
        #region BaseOrderQty
        public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
        protected Decimal? _BaseOrderQty;
        [PXDBQuantity(BqlField = typeof(POLine.baseOrderQty))]
        public virtual Decimal? BaseOrderQty
        {
            get
            {
                return this._BaseOrderQty;
            }
            set
            {
                this._BaseOrderQty = value;
            }
        }
        #endregion
        #region OpenQty
        public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
        protected Decimal? _OpenQty;
        [PXDBQuantity(BqlField = typeof(POLine.openQty))]
        [PXUIField(DisplayName = "Open Qty.", Enabled = false)]
        public virtual Decimal? OpenQty
        {
            get
            {
                return this._OpenQty;
            }
            set
            {
                this._OpenQty = value;
            }
        }
        #endregion
        #region BaseOpenQty
        public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
        protected Decimal? _BaseOpenQty;
        [PXDBDecimal(6, BqlField = typeof(POLine.baseOpenQty))]
        public virtual Decimal? BaseOpenQty
        {
            get
            {
                return this._BaseOpenQty;
            }
            set
            {
                this._BaseOpenQty = value;
            }
        }
        #endregion
        #region ReceivedQty
        public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
        protected Decimal? _ReceivedQty;
        [PXDBDecimal(6, BqlField = typeof(POLine.receivedQty))]
        public virtual Decimal? ReceivedQty
        {
            get
            {
                return this._ReceivedQty;
            }
            set
            {
                this._ReceivedQty = value;
            }
        }
        #endregion
        #region BaseReceivedQty
        public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }
        protected Decimal? _BaseReceivedQty;
        [PXDBDecimal(6, BqlField = typeof(POLine.baseReceivedQty))]
        public virtual Decimal? BaseReceivedQty
        {
            get
            {
                return this._BaseReceivedQty;
            }
            set
            {
                this._BaseReceivedQty = value;
            }
        }
        #endregion
        #region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
        protected String _TranDesc;
        [PXDBString(256, IsUnicode = true, BqlField = typeof(POLine.tranDesc))]
        [PXUIField(DisplayName = "Line Description", Enabled = false)]
        public virtual String TranDesc
        {
            get
            {
                return this._TranDesc;
            }
            set
            {
                this._TranDesc = value;
            }
        }
        #endregion
        #region ReceiptStatus
        public abstract class receiptStatus : PX.Data.BQL.BqlString.Field<receiptStatus> { }
        protected String _ReceiptStatus;
        [PXDBString(1, IsFixed = true, BqlField = typeof(POLine.receiptStatus))]
        public virtual String ReceiptStatus
        {
            get
            {
                return this._ReceiptStatus;
            }
            set
            {
                this._ReceiptStatus = value;
            }
        }
        #endregion
        #region AMProdMatlOrderType
        public abstract class aMProdMatlOrderType : PX.Data.BQL.BqlString.Field<aMProdMatlOrderType> { }
        protected String _AMProdMatlOrderType;
        [PXString(2, IsFixed = true)]
        public virtual String AMProdMatlOrderType
        {
            get
            {
                return this._AMProdMatlOrderType;
            }
            set
            {
                this._AMProdMatlOrderType = value;
            }
        }
        #endregion
        #region AMProdMatlProdOrdID
        public abstract class aMProdMatlProdOrdID : PX.Data.BQL.BqlString.Field<aMProdMatlProdOrdID> { }
        protected String _AMProdMatlProdOrdID;
        [PXString(15, IsUnicode = true)]
        public virtual String AMProdMatlProdOrdID
        {
            get
            {
                return this._AMProdMatlProdOrdID;
            }
            set
            {
                this._AMProdMatlProdOrdID = value;
            }
        }
        #endregion
        #region AMProdMatlOperationID
        public abstract class aMProdMatlOperationID : PX.Data.BQL.BqlInt.Field<aMProdMatlOperationID> { }
        protected Int32? _AMProdMatlOperationID;
        [PXInt()]
        public virtual Int32? AMProdMatlOperationID
        {
            get
            {
                return this._AMProdMatlOperationID;
            }
            set
            {
                this._AMProdMatlOperationID = value;
            }
        }
        #endregion
        #region AMProdMatlLineNbr
        public abstract class aMProdMatlLineNbr : PX.Data.BQL.BqlInt.Field<aMProdMatlLineNbr> { }
        protected Int32? _AMProdMatlLineNbr;
        [PXInt()]
        public virtual Int32? AMProdMatlLineNbr
        {
            get
            {
                return this._AMProdMatlLineNbr;
            }
            set
            {
                this._AMProdMatlLineNbr = value;
            }
        }
        #endregion
        #region AMProdMatlSplitLineNbr
        public abstract class aMProdMatlSplitLineNbr : PX.Data.BQL.BqlInt.Field<aMProdMatlSplitLineNbr> { }
        protected Int32? _AMProdMatlSplitLineNbr;
        [PXInt()]
        public virtual Int32? AMProdMatlSplitLineNbr
        {
            get
            {
                return this._AMProdMatlSplitLineNbr;
            }
            set
            {
                this._AMProdMatlSplitLineNbr = value;
            }
        }
        #endregion
        #region DemandQty
        public abstract class demandQty : PX.Data.BQL.BqlDecimal.Field<demandQty> { }
        protected Decimal? _DemandQty;
        [PXDecimal(6)]
        public virtual Decimal? DemandQty
        {
            get
            {
                return this._DemandQty;
            }
            set
            {
                this._DemandQty = value;
            }
        }
        #endregion
    }
}