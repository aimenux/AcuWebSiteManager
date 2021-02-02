using System;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    using Attributes;

    /// <summary>
    /// Graph used for building the production order details (copy Bom detail to the order)
    /// </summary>
    public class ProductionBomCopy : ProductionBomCopyBase
    {
        public virtual void CreateProductionDetails(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return;
            }

            try
            {
                var order = ConvertToOrder(amProdItem);

                Order.Current = order;

                //Delete existing production detail records
                DeleteProductionDetail(amProdItem);

                SetCurrentProdItem(amProdItem);
                SetCurrentProdItemDescription();

                CacheLoadProdAttributes();

                CreateOperationDetail(order);

                CopyBomsToProductionOrder();
            }
            catch (Exception e)
            {
                PXTrace.WriteError(e.InnerException ?? e);

                throw new PXException(Messages.ErrorInCopyBomProcess, e.Message);
            }
        }

        protected virtual void CopyBomsToProductionOrder()
        {
            foreach (var operationDetail in _operationDetails.OrderedList)
            {
                var oper = CopyOper(operationDetail);
                if (oper != null)
                {
                    //It is possible to have oper be null but we need to continue because of phantom routings and excluded operations.
                    // OperationDetails will load in correct order so first operation is loaded then followed by all excludes until the next Include Operation

                    // Must set the current so the line counters are in sync
                    ProcessingGraph.Caches<AMProdOper>().Current = oper;
                }
                
                CopyMatl(operationDetail);
                CopyOvhd(operationDetail);
                CopyTool(operationDetail);
                CopyStep(operationDetail);
                CopyBomOperationLevelAttributes(CurrentProdOper?.OperationID, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID);
            }
            UpdatePlannedOperationTotals();

            //Line Counters are screwy if we try to do before first operation insert. Cache has 2 updates to AMPRodItem (one with ProdOrdID number with no trailing spaces, one with trailing spaces)
            CopyBomOrderLevelAttributes(CurrentProdItem);
            CopyOrderTypeAttributes(CurrentProdItem);
        }

        /// <summary>
        /// Copy the given BOM Level Attributes to the given production item record
        /// </summary>
        /// <param name="amProdItem"></param>
        public virtual void CopyBomOrderLevelAttributes(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType)
                || string.IsNullOrWhiteSpace(amProdItem.BOMID))
            {
                return;
            }

            AMOrderType orderType = CurrentOrderType;
            if (orderType == null)
            {
                return;
            }

            foreach (AMBomAttribute bomAttribute in PXSelect<AMBomAttribute, 
                    Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                    And2<Where<AMBomAttribute.orderFunction, Equal<Required<AMOrderType.function>>,
                        Or<AMBomAttribute.orderFunction, Equal<OrderTypeFunction.all>>>,
                    And<AMBomAttribute.level, Equal<AMAttributeLevels.bOM>>
                    >>>.Select(ProcessingGraph, amProdItem.BOMID, orderType.Function))
            {
                var newProdAttribute = ProductionBomCopyMap.CopyAttributes(bomAttribute);
                if (newProdAttribute == null
                    || string.IsNullOrWhiteSpace(newProdAttribute.Label))
                {
                    continue;
                }
                newProdAttribute.OrderType = CurrentProdItem?.OrderType;
                newProdAttribute.ProdOrdID = CurrentProdItem?.ProdOrdID;
                TryInsertAMProdAttribute(newProdAttribute);
            }
        }

        protected virtual void CopyBomOperationLevelAttributes(int? newOperationId, string fromBomId, string fromRevisionId, int? fromOperationId)
        {
            if (newOperationId == null)
            {
                throw new ArgumentNullException(nameof(newOperationId));
            }

            AMOrderType orderType = CurrentOrderType;
            if (orderType == null)
            {
                return;
            }

            foreach (AMBomAttribute amBomAttribute in PXSelect<AMBomAttribute, 
                    Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                        And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>,
                    And<AMBomAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>,
                    And2<Where<AMBomAttribute.orderFunction, Equal<Required<AMOrderType.function>>,
                        Or<AMBomAttribute.orderFunction, Equal<OrderTypeFunction.all>>>,
                    And<AMBomAttribute.level, Equal<AMAttributeLevels.operation>>>
                    >>>>.Select(ProcessingGraph, fromBomId, fromRevisionId, fromOperationId, orderType.Function)) 
                        
            {
                var newProdAttribute = ProductionBomCopyMap.CopyAttributes(amBomAttribute);
                if (string.IsNullOrWhiteSpace(newProdAttribute?.Label))
                {
                    continue;
                }
                newProdAttribute.OrderType = CurrentProdItem?.OrderType;
                newProdAttribute.ProdOrdID = CurrentProdItem?.ProdOrdID;
                newProdAttribute.OperationID = newOperationId;

                TryInsertAMProdAttribute(newProdAttribute);
            }
        }

        protected virtual void CopyStep(OperationDetail operationDetail)
        {
            foreach (AMBomStep amBomStep in PXSelect<
                AMBomStep, 
                Where<AMBomStep.bOMID, Equal<Required<AMBomStep.bOMID>>,
                    And<AMBomStep.revisionID, Equal<Required<AMBomStep.revisionID>>,
                    And<AMBomStep.operationID, Equal<Required<AMBomStep.operationID>>>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdStep = ProductionBomCopyMap.CopyStep(amBomStep);

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdStep, operationDetail);
                }

                // Inserting and then updating the prod Step records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodStep = ProcessingGraph.Caches<AMProdStep>().Insert(newProdStep);

                if (CurrentOrderType?.CopyNotesStep == true && prodStep != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomStep>(), amBomStep, ProcessingGraph.Caches<AMProdStep>(), prodStep);
                    ProcessingGraph.Caches<AMProdStep>().Update(prodStep);
                }
            }
        }

        protected virtual void CopyTool(OperationDetail operationDetail)
        {
            foreach (AMBomTool amBomTool in PXSelectJoin<
                AMBomTool, 
                InnerJoin<AMToolMst, 
                    On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
                Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
                    And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>,
                    And<AMBomTool.operationID, Equal<Required<AMBomTool.operationID>>>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdTool = ProductionBomCopyMap.CopyTool(amBomTool);

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdTool, operationDetail);
                }

                // Inserting and then updating the the prod tool records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodTool = (AMProdTool)ProcessingGraph.Caches<AMProdTool>().Insert(newProdTool);
                if (CurrentOrderType?.CopyNotesTool == true && prodTool != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomTool>(), amBomTool, ProcessingGraph.Caches<AMProdTool>(), prodTool);
                    ProcessingGraph.Caches<AMProdTool>().Update(prodTool);
                }
            }
        }

        protected virtual void CopyOvhd(OperationDetail operationDetail)
        {
            foreach (AMBomOvhd amBomOvhd in PXSelectJoin<
                AMBomOvhd,
                InnerJoin<AMOverhead, 
                    On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMBomOvhd.bOMID, Equal<Required<AMBomOvhd.bOMID>>,
                    And<AMBomOvhd.revisionID, Equal<Required<AMBomOvhd.revisionID>>,
                    And<AMBomOvhd.operationID, Equal<Required<AMBomOvhd.operationID>>>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdOvhd = ProductionBomCopyMap.CopyOvhd(amBomOvhd);
                newProdOvhd.WCFlag = false;

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdOvhd, operationDetail);
                }

                // Inserting and then updating the the prod overhead records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodovhd = (AMProdOvhd)ProcessingGraph.Caches<AMProdOvhd>().Insert(newProdOvhd);
                if (CurrentOrderType?.CopyNotesOvhd == true && prodovhd != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomOvhd>(), amBomOvhd, ProcessingGraph.Caches<AMProdOvhd>(), prodovhd);
                    ProcessingGraph.Caches<AMProdOvhd>().Update(prodovhd);
                }
            }

            if (operationDetail.IncludeOper)
            {
                //Copy workcenter overheads to production order
                CopyWorkCenterOverheads(operationDetail);
            }
        }

        protected virtual void CopyMatl(OperationDetail operationDetail)
        {
            var costRoll = CreateInstance<BOMCostRoll>();
            foreach (PXResult<AMBomMatl, InventoryItem> result in PXSelectJoin<
                AMBomMatl,
                InnerJoin<InventoryItem, 
                    On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>>,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomOper.revisionID>>,
                    And<AMBomMatl.operationID, Equal<Required<AMBomOper.operationID>>,
                    And<AMBomMatl.materialType, NotEqual<AMMaterialType.phantom>>>>>,
                OrderBy<
                    Asc<AMBomMatl.sortOrder, 
                    Asc<AMBomMatl.lineID>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var amBomMatl = (AMBomMatl)result;
                var inventoryItem = (InventoryItem)result;
                var inventoryItemExt = inventoryItem?.GetExtension<InventoryItemExt>();

                if (inventoryItem?.InventoryID == null || SkipMaterial(amBomMatl))
                {
                    continue; 
                }

                var newProdMatl = ProductionBomCopyMap.CopyMatl(amBomMatl);
                newProdMatl.QtyRoundUp = inventoryItemExt?.AMQtyRoundUp ?? false;

                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Insert(newProdMatl);

                if (newProdMatl == null)
                {
                    PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToInsertProdMatlFromBom, inventoryItem?.InventoryCD, amBomMatl.BOMID, amBomMatl.OperationID, amBomMatl.LineID));
                    continue;
                }

                //Copy all to show the sources for non phantoms to so users can perform reporting on the 
                //  production details specifically relating back to the location on the source BOM.
                //  This is useful as the operation numbers are not necessary the same on the production order as they
                //  might be on the BOM due to included phantom operations.
                newProdMatl.PhtmBOMID = amBomMatl.BOMID;
                newProdMatl.PhtmBOMRevisionID = amBomMatl.RevisionID;
                newProdMatl.PhtmBOMLineRef = amBomMatl.LineID;
                newProdMatl.PhtmBOMOperationID = amBomMatl.OperationID;
                newProdMatl.PhtmLevel = 0;

                if (newProdMatl.IsByproduct.GetValueOrDefault() && CurrentProdItem != null && CurrentProdItem?.Function == OrderTypeFunction.Disassemble)
                {
                    // By products not supported on disassemble order types
                    continue;
                }

                if (!operationDetail.IsProdBom)
                {
                    newProdMatl.QtyReq = newProdMatl.QtyReq * operationDetail.BomQtyReq;
                    SetPhtmMatlReferences(ref newProdMatl, operationDetail);
                }

                // Inserting and then updating the the prod matl records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Update(newProdMatl);
                if (CurrentOrderType?.CopyNotesMatl == true && newProdMatl != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomMatl>(), amBomMatl, ProcessingGraph.Caches<AMProdMatl>(), newProdMatl);
                }
                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Update(newProdMatl);
                BOMCostRoll.UpdatePlannedMaterialCost(ProcessingGraph, costRoll, CurrentProdItem, newProdMatl, result);
            }
        }

        protected virtual AMProdOper CopyOper(OperationDetail operationDetail)
        {
            if (!operationDetail.IncludeOper)
            {
                return null;
            }

            AMBomOper amBomOper = PXSelect<
                AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
                    And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID);

            if (amBomOper == null)
            {
                return null;
            }

            try
            {
                var newProdOper = ProductionBomCopyMap.CopyOper(amBomOper, (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Insert());
                newProdOper.OperationCD = operationDetail.ProdOperationCD;
                newProdOper.TotalQty = CurrentProdItem?.QtytoProd.GetValueOrDefault();
                newProdOper.BaseTotalQty = CurrentProdItem?.BaseQtytoProd.GetValueOrDefault();
                newProdOper.BaseQtytoProd = 0m;
                newProdOper.WcID = operationDetail.WcID;
                newProdOper.BFlush = operationDetail.WcBFlushLabor.GetValueOrDefault();
                newProdOper.Descr = operationDetail.WcDesc;

                newProdOper = (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Update(newProdOper);

#if DEBUG
                AMDebug.TraceWriteMethodName(newProdOper?.DebuggerDisplay);
#endif
                if (CurrentOrderType?.CopyNotesOper == true)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomOper>(), amBomOper, ProcessingGraph.Caches<AMProdOper>(), newProdOper);
                }

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdOper, operationDetail);
                    newProdOper.PhtmPriorLevelQty = operationDetail.BomQtyReq;
                }

                return (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Update(newProdOper);
            }

            catch (Exception e)
            {
                throw new PXException(e, Messages.GetLocal(Messages.ErrorInsertingProductionOperation, 
                    Messages.GetLocal(Messages.BOM),
                    operationDetail?.ProdOperationCD,
                    amBomOper?.RevisionID,
                    amBomOper?.BOMID,
                    amBomOper?.OperationCD,
                    amBomOper?.Descr.TrimIfNotNullEmpty(),
                    amBomOper?.WcID,
                    e.Message));
            }
        }
    }
}