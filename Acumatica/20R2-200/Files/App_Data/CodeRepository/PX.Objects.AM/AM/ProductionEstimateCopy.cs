using System;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Graph for building production details based on an Estimate
    /// </summary>
    public class ProductionEstimateCopy : ProductionBomCopy
    {
        public override void CreateProductionDetails(AMProdItem amProdItem)
        {
            if (amProdItem == null 
                || amProdItem.DetailSource != ProductionDetailSource.Estimate
                || string.IsNullOrWhiteSpace(amProdItem.EstimateID)
                || string.IsNullOrWhiteSpace(amProdItem.EstimateRevisionID))
            {
                return;
            }

            try
            {
                var order = ConvertToOrder(amProdItem);
                Order.Current = order;

                var estimateItem = FindEstimateItem(order);
                if (estimateItem == null)
                {
                    return;
                }

                SetCurrentProdItem(amProdItem);
                if (CurrentProdItem == null)
                {
                    throw new PXException(Messages.InvalidProductionNbr, amProdItem.OrderType.TrimIfNotNullEmpty(), amProdItem.ProdOrdID.TrimIfNotNullEmpty());
                }

                DeleteProductionDetail(amProdItem);

                SetCurrentProdItemDescription();

                CreateOperationDetail(order);

                CopyBomsToProductionOrder();
            }
            catch (Exception e)
            {
                var oe = e as PXOuterException;
                if (oe != null)
                {
                    PXTraceHelper.PxTraceOuterException(oe, PXTraceHelper.ErrorLevel.Error);
                }

                throw;
            }
        }

        protected override AMProdOper CopyOper(OperationDetail operationDetail)
        {
            if (SourceIsEstimate(operationDetail))
            {
                return CopyEstimateOper(operationDetail);
            }

            return base.CopyOper(operationDetail);
        }

        protected virtual AMProdOper CopyEstimateOper(OperationDetail operationDetail)
        {
            if (operationDetail == null 
                || !operationDetail.IncludeOper 
                || !SourceIsEstimate(operationDetail))
            {
                return null;
            }

            AMEstimateOper oper = PXSelect<
                AMEstimateOper,
                Where<AMEstimateOper.estimateID, Equal<Required<AMEstimateOper.estimateID>>,
                    And<AMEstimateOper.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                    And<AMEstimateOper.operationID, Equal<Required<AMEstimateOper.operationID>>>>>
                >
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID);

            if (oper == null)
            {
                return null;
            }

            try
            {
                var newProdOper = ProductionBomCopyMap.CopyOper(EstimateBomMap.CopyOperToBom(oper), (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Insert());
                newProdOper.OperationCD = operationDetail.ProdOperationCD;
                newProdOper.TotalQty = CurrentProdItem?.QtytoProd.GetValueOrDefault();
                newProdOper.BaseTotalQty = CurrentProdItem?.BaseQtytoProd.GetValueOrDefault();
                newProdOper.BaseQtytoProd = 0m;
                newProdOper = (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Update(newProdOper);
                if (newProdOper == null)
                {
                    throw new PXException(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMProdOper)));
                }

                if (CurrentOrderType?.CopyNotesOper == true)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches[typeof(AMEstimateOper)], oper, ProcessingGraph.Caches<AMProdOper>(), newProdOper);
                }

                newProdOper.PhtmBOMID = oper.EstimateID;
                newProdOper.PhtmBOMRevisionID = oper.RevisionID;
                newProdOper.PhtmBOMOperationID = oper.OperationID;
                newProdOper.PhtmLevel = 0;

                return (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Update(newProdOper);
            }

            catch (Exception e)
            {
                throw new PXException(e, Messages.GetLocal(Messages.ErrorInsertingProductionOperation,
                    Messages.GetLocal(Messages.Estimate),
                    operationDetail?.ProdOperationCD,
                    oper?.RevisionID,
                    oper?.EstimateID,
                    oper?.OperationCD,
                    oper?.Description.TrimIfNotNullEmpty(),
                    oper?.WcID,
                    e.Message));
            }
        }

        protected override void CopyMatl(OperationDetail operationDetail)
        {
            if (SourceIsEstimate(operationDetail))
            {
                CopyEstimateMatl(operationDetail);
            }
            base.CopyMatl(operationDetail);
        }

        protected virtual void CopyEstimateMatl(OperationDetail operationDetail)
        {
            if (operationDetail == null || !SourceIsEstimate(operationDetail))
            {
                return;
            }

            foreach (AMEstimateMatl estimateMatl in PXSelect<AMEstimateMatl,
                Where<AMEstimateMatl.estimateID, Equal<Required<AMEstimateMatl.estimateID>>,
                    And<AMEstimateMatl.revisionID, Equal<Required<AMEstimateMatl.revisionID>>,
                    And<AMEstimateMatl.operationID, Equal<Required<AMEstimateMatl.operationID>>>>>,
                OrderBy<Asc<AMEstimateMatl.sortOrder, Asc<AMEstimateMatl.lineID>>>
                >.Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                if (estimateMatl.MaterialType == AMMaterialType.Phantom)
                {
                    continue;
                }

                //Prevent creation when material is a non-inventory. User must convert non-inventory material 
                //  to stock/non-stock before a production order can be created.
                if (estimateMatl.IsNonInventory.GetValueOrDefault()
                    || estimateMatl.InventoryID == null)
                {
                    throw new PXException(Messages.CannotCreateProductionOrderFromEstimateNonInventory, 
                        estimateMatl.EstimateID.TrimIfNotNullEmpty(),
                        estimateMatl.RevisionID.TrimIfNotNullEmpty(),
                        estimateMatl.InventoryCD.TrimIfNotNullEmpty(),
                        operationDetail.BomOperationCD.TrimIfNotNullEmpty());
                }
                
                var newProdMatl = ProductionBomCopyMap.CopyMatl(EstimateBomMap.CopyMatlToBom(estimateMatl));
                newProdMatl.OrderType = CurrentProdItem?.OrderType;
                newProdMatl.ProdOrdID = CurrentProdItem?.ProdOrdID;

                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Insert(newProdMatl);

                if (CheckProdRowInsert(newProdMatl, estimateMatl, out var ex))
                {
                    throw ex;
                }

                if (newProdMatl.SortOrder == null)
                {
                    newProdMatl.SortOrder = newProdMatl.LineID;
                }

                // exists to provide a reference only (phantom bom or not)
                newProdMatl.PhtmBOMID = estimateMatl.EstimateID;
                newProdMatl.PhtmBOMRevisionID = estimateMatl.RevisionID;
                newProdMatl.PhtmBOMLineRef = estimateMatl.LineID;
                newProdMatl.PhtmBOMOperationID = estimateMatl.OperationID;
                newProdMatl.PhtmLevel = 0;

                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Update(newProdMatl);
                if (newProdMatl == null)
                {
                    continue;
                }
                
                if (CurrentOrderType?.CopyNotesMatl == true)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches[typeof(AMEstimateMatl)], estimateMatl, ProcessingGraph.Caches<AMProdMatl>(), newProdMatl);
                    ProcessingGraph.Caches<AMProdMatl>().Update(newProdMatl);
                }
            }
        }

        protected override void CopyTool(OperationDetail operationDetail)
        {
            if (SourceIsEstimate(operationDetail))
            {
                CopyEstimateTool(operationDetail);
            }
            base.CopyTool(operationDetail);
        }

        protected virtual void CopyEstimateTool(OperationDetail operationDetail)
        {
            if (operationDetail == null || !SourceIsEstimate(operationDetail))
            {
                return;
            }

            foreach (AMEstimateTool estimateTool in PXSelectJoin<AMEstimateTool, 
                InnerJoin<AMToolMst, On<AMEstimateTool.toolID, Equal<AMToolMst.toolID>>>
                , Where<AMEstimateTool.estimateID, Equal<Required<AMEstimateTool.estimateID>>
                    , And<AMEstimateTool.revisionID, Equal<Required<AMEstimateTool.revisionID>>
                    , And<AMEstimateTool.operationID, Equal<Required<AMEstimateTool.operationID>>>>>
                >.Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdTool = ProductionBomCopyMap.CopyTool(EstimateBomMap.CopyToolToBom(estimateTool));
                newProdTool.OrderType = CurrentProdItem?.OrderType;
                newProdTool.ProdOrdID = CurrentProdItem?.ProdOrdID;

                // exists to provide a reference only (phantom bom or not)
                newProdTool.PhtmBOMID = estimateTool.EstimateID;
                newProdTool.PhtmBOMRevisionID = estimateTool.RevisionID;
                newProdTool.PhtmBOMLineRef = estimateTool.LineID;
                newProdTool.PhtmBOMOperationID = estimateTool.OperationID;
                newProdTool.PhtmLevel = 0;

                // Inserting and then updating the the prod tool records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodTool = (AMProdTool)ProcessingGraph.Caches<AMProdTool>().Insert(newProdTool);
                if(CheckProdRowInsert(prodTool, estimateTool, out var ex))
                {
                    throw ex;
                }

                if (CurrentOrderType?.CopyNotesTool == true && prodTool != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches[typeof(AMEstimateTool)], estimateTool, ProcessingGraph.Caches<AMProdTool>(), prodTool);
                    ProcessingGraph.Caches<AMProdTool>().Update(prodTool);
                }
            }
        }

        protected override void CopyOvhd(OperationDetail operationDetail)
        {
            if (SourceIsEstimate(operationDetail))
            {
                CopyEstimateOvhd(operationDetail);
            }
            base.CopyOvhd(operationDetail);
        }

        protected virtual void CopyEstimateOvhd(OperationDetail operationDetail)
        {
            if (operationDetail == null || !SourceIsEstimate(operationDetail))
            {
                return;
            }

            foreach (AMEstimateOvhd estimateOvhd in PXSelectJoin<AMEstimateOvhd,
                InnerJoin<AMOverhead, On<AMEstimateOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                    Where<AMEstimateOvhd.estimateID, Equal<Required<AMEstimateOvhd.estimateID>>
                    , And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOvhd.revisionID>>
                    , And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOvhd.operationID>>>>>
                >.Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdOvhd = ProductionBomCopyMap.CopyOvhd(EstimateBomMap.CopyOvhdToBom(estimateOvhd));
                newProdOvhd.OrderType = CurrentProdItem?.OrderType;
                newProdOvhd.ProdOrdID = CurrentProdItem?.ProdOrdID;
                newProdOvhd.WCFlag = false;

                newProdOvhd.PhtmBOMID = estimateOvhd.EstimateID;
                newProdOvhd.PhtmBOMRevisionID = estimateOvhd.RevisionID;
                newProdOvhd.PhtmBOMLineRef = estimateOvhd.LineID;
                newProdOvhd.PhtmBOMOperationID = estimateOvhd.OperationID;
                newProdOvhd.PhtmLevel = 0;

                var prodovhd = (AMProdOvhd)ProcessingGraph.Caches<AMProdOvhd>().Insert(newProdOvhd);
                if (CheckProdRowInsert(prodovhd, estimateOvhd, out var ex))
                {
                    throw ex;
                }

                if (CurrentOrderType?.CopyNotesOvhd == true && prodovhd != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches[typeof(AMEstimateOvhd)], estimateOvhd, ProcessingGraph.Caches<AMProdOvhd>(), prodovhd);
                    ProcessingGraph.Caches<AMProdOvhd>().Update(prodovhd);
                }
            }
        }

        protected override void CopyStep(OperationDetail operationDetail)
        {
            if (SourceIsEstimate(operationDetail))
            {
                CopyEstimateStep(operationDetail);
            }
            base.CopyStep(operationDetail);
        }

        protected virtual void CopyEstimateStep(OperationDetail operationDetail)
        {
            if (operationDetail == null || !SourceIsEstimate(operationDetail))
            {
                return;
            }

            foreach (AMEstimateStep estimateStep in PXSelect<AMEstimateStep,
                Where<AMEstimateStep.estimateID, Equal<Required<AMEstimateStep.estimateID>>
                    , And<AMEstimateStep.revisionID, Equal<Required<AMEstimateStep.revisionID>>
                    , And<AMEstimateStep.operationID, Equal<Required<AMEstimateStep.operationID>>>>>
                >.Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdStep = ProductionBomCopyMap.CopyStep(EstimateBomMap.CopyStepToBom(estimateStep));
                newProdStep.OrderType = CurrentProdItem?.OrderType;
                newProdStep.ProdOrdID = CurrentProdItem?.ProdOrdID;

                // exists to provide a reference only (phantom bom or not)
                newProdStep.PhtmBOMID = estimateStep.EstimateID;
                newProdStep.PhtmBOMRevisionID = estimateStep.RevisionID;
                newProdStep.PhtmBOMLineRef = estimateStep.LineID;
                newProdStep.PhtmBOMOperationID = estimateStep.OperationID;
                newProdStep.PhtmLevel = 0;

                // Inserting and then updating the the prod step records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodStep = (AMProdStep)ProcessingGraph.Caches<AMProdStep>().Insert(newProdStep);
                if (CheckProdRowInsert(prodStep, estimateStep, out var ex))
                {
                    throw ex;
                }

                if (CurrentOrderType?.CopyNotesStep == true && prodStep != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches[typeof(AMEstimateStep)], estimateStep, ProcessingGraph.Caches<AMProdStep>(), prodStep);
                    ProcessingGraph.Caches<AMProdStep>().Update(prodStep);
                }
            }
        }

        protected virtual bool SourceIsEstimate(OperationDetail operationDetail)
        {
            return operationDetail == null || operationDetail.IsProdBom;
        }

        public override void CreateOperationDetail(AMOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (string.IsNullOrWhiteSpace(order.SourceID))
            {
                throw new PXArgumentException("Source ID");
            }

            if (string.IsNullOrWhiteSpace(order.RevisionID))
            {
                throw new PXArgumentException("Revision ID");
            }

            if (order.SiteID == null)
            {
                throw new ArgumentException("Site ID");
            }

            if (order.OrderQty.GetValueOrDefault() == 0)
            {
                throw new ArgumentException("Order Qty");
            }

            if (Common.Dates.IsDefaultDate(order.PlanDate))
            {
                throw new ArgumentException("Order plan date");
            }

            _operationDetails = new OrderedOperationDetails();

            Order.Current = order;

            //Build list of operations accounting for phantoms
            BuildOperationDetail(FindEstimateItem(order));

            //Set the production operation numbers
            SetProductionOperNbrs();
        }

        protected virtual void BuildOperationDetail(AMEstimateItem estimateItem)
        {
            if (string.IsNullOrWhiteSpace(estimateItem?.EstimateID))
            {
                throw new PXArgumentException(nameof(estimateItem));
            }

            if (string.IsNullOrWhiteSpace(estimateItem.RevisionID))
            {
                throw new PXArgumentException(nameof(estimateItem.RevisionID));
            }

            var cntr = 0;
            foreach (AMEstimateOper amEstimateOper in PXSelect<AMEstimateOper,
                Where<AMEstimateOper.estimateID, Equal<Required<AMEstimateOper.estimateID>>,
                And<AMEstimateOper.revisionID, Equal<Required<AMEstimateOper.revisionID>>>>,
                OrderBy<Asc<AMEstimateOper.operationCD>>>.Select(ProcessingGraph, estimateItem.EstimateID, estimateItem.RevisionID))
            {
                var opDetail = new OperationDetail
                {
                    BomID = amEstimateOper.EstimateID,
                    BomRevisionID = amEstimateOper.RevisionID,
                    BomOperationCD = amEstimateOper.OperationCD,
                    BomOperationID = amEstimateOper.OperationID,
                    SortOrder = ++cntr,
                    Level = 0,
                    IsProdBom = true,
                    IncludeOper = true,
                    MatlLineId = 0,
                    MatlOperationID = amEstimateOper.OperationID,
                    BomQtyReq = 1
                };

                //make sure these ops always go to the end
                var forceToEnd = _operationDetails.OrderedList.Count + 1;

                var index = _operationDetails.Add(forceToEnd, Position.After, opDetail);
                LoadPhantoms(amEstimateOper, 1, index, opDetail.BomQtyReq);
            }
        }

        protected virtual int LoadPhantoms(AMEstimateOper amEstimateOper, int level, int currentIndex)
        {
            return LoadPhantoms(amEstimateOper, level, currentIndex, 1m);
        }

        protected virtual int LoadPhantoms(AMEstimateOper amEstimateOper,  int level, int currentIndex, decimal? parentQtyRequired)
        {
            int lastIndex = currentIndex;

            foreach (AMEstimateMatl estimateMatl in PXSelect<AMEstimateMatl,
                Where<AMEstimateMatl.estimateID, Equal<Required<AMEstimateMatl.estimateID>>,
                    And<AMEstimateMatl.revisionID, Equal<Required<AMEstimateMatl.revisionID>>,
                    And<AMEstimateMatl.operationID, Equal<Required<AMEstimateMatl.operationID>>,
                    And<AMEstimateMatl.materialType, Equal<AMMaterialType.phantom>>>>>,
                    OrderBy<Asc<AMEstimateMatl.sortOrder, Asc<AMEstimateMatl.lineID>>>
                >.Select(ProcessingGraph, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                lastIndex = BuildOperationByBom(estimateMatl, amEstimateOper.OperationCD, level, currentIndex, parentQtyRequired);
            }

            return lastIndex;
        }

        protected virtual int BuildOperationByBom(AMEstimateMatl parentEstimateMatl, string parentOperationCD, int level, int currentOpIndex, decimal? parentQtyRequired)
        {
            if (parentEstimateMatl == null)
            {
                return currentOpIndex;
            }

            if (level < 0 || level >= LowLevel.MaxLowLevel)
            {
                // Most likely a recursive bom - prevent getting stuck in infinite loop
                return currentOpIndex;
            }

            var siteID = parentEstimateMatl.SiteID ?? Order.Current.SiteID;

            var bomId = new PrimaryBomIDManager(ProcessingGraph).GetPrimaryAllLevels(parentEstimateMatl.InventoryID, siteID, parentEstimateMatl.SubItemID);
            
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return currentOpIndex;
            }

            var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(ProcessingGraph, bomId);

            if (bomItem?.BOMID == null)
            {
                return currentOpIndex;
            }

            var position = Position.After;
            if (parentEstimateMatl.PhantomRouting != PhantomRoutingOptions.Exclude)
            {
                position = parentEstimateMatl.PhantomRouting == PhantomRoutingOptions.Before
                    ? Position.Before
                    : Position.After;
            }
            var opIndex = currentOpIndex;
            var firstOper = true;
            OperationDetail lastOperationDetail = null;
            OperationDetail positionOperationDetail = _operationDetails.FindParentByIndex(opIndex, level);
            if (positionOperationDetail != null && !positionOperationDetail.IncludeOper && positionOperationDetail.ParentIncludeOpReferenceKey != 0)
            {
                opIndex = _operationDetails.IndexOf(positionOperationDetail.ParentIncludeOpReferenceKey);
            }

            foreach (PXResult<AMBomOper, AMWC> result in PXSelectJoin<
                AMBomOper,
                InnerJoin<AMWC, 
                    On<AMWC.wcID, Equal<AMBomOper.wcID>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>,
                OrderBy<
                    Asc<AMBomOper.operationCD>>>
                .Select(ProcessingGraph, bomId, bomItem.RevisionID))
            {
                var amBomOper = (AMBomOper)result;
                var wc = (AMWC)result;

                if (amBomOper?.BOMID == null || wc?.WcID == null)
                {
                    continue;
                }

                var opDetail = ConvertBomOperToOperationDetail(amBomOper);
                opDetail.BomRevisionID = bomItem.RevisionID;
                opDetail.Level = level;
                opDetail.IsProdBom = false;
                opDetail.IncludeOper = parentEstimateMatl.PhantomRouting != PhantomRoutingOptions.Exclude;
                opDetail.MatlLineId = parentEstimateMatl.LineID.GetValueOrDefault();
                opDetail.MatlOperationID = parentEstimateMatl.OperationID;
                opDetail.BomQtyReq = parentEstimateMatl.QtyReq.GetValueOrDefault(1) * parentQtyRequired.GetValueOrDefault(1);
                opDetail.WcSiteID = wc.SiteID;

                if (positionOperationDetail != null)
                {
                    opDetail.ParentReferenceKey = positionOperationDetail.ReferenceKey;
                    opDetail.ParentIncludeOpReferenceKey = positionOperationDetail.IncludeOper ? positionOperationDetail.ReferenceKey : positionOperationDetail.ParentIncludeOpReferenceKey;
                }
#if DEBUG
                if (positionOperationDetail != null)
                {
                    AMDebug.TraceWriteMethodName(
                        $"[{opIndex}] BOMID {amBomOper.BOMID} {amBomOper.RevisionID} Oper {amBomOper.OperationCD}; level = {level}; parentOperationDetail {positionOperationDetail.BomID} {positionOperationDetail.BomRevisionID} {positionOperationDetail.BomOperationCD} BomQtyReq = {positionOperationDetail.BomQtyReq}; new opDetail.BomQtyReq = {opDetail.BomQtyReq}");
                }
                else
                {
                    AMDebug.TraceWriteMethodName("!!! NULL !!!");
                }
#endif
                
                if (!firstOper)
                {
                    var lastIndex = _operationDetails.IndexOf(lastOperationDetail);
                    opIndex = lastIndex > opIndex ? lastIndex : opIndex;
                    position = Position.After;
                }

                lastOperationDetail = opDetail;
                var index = _operationDetails.Add(opIndex, position, opDetail);

                opIndex = LoadPhantoms(amBomOper, level + 1, index, opDetail.BomQtyReq);
                firstOper = false;
            }

            return opIndex;
        }

        protected virtual AMEstimateItem FindEstimateItem(AMOrder order)
        {
            if (order == null)
            {
                return null;
            }

            return FindEstimateItem(order.SourceID, order.RevisionID);
        }

        public virtual AMEstimateItem FindEstimateItem(string estimateID, string revisionID)
        {
            if (string.IsNullOrWhiteSpace(estimateID) || string.IsNullOrWhiteSpace(revisionID))
            {
                return null;
            }

            return PXSelect<AMEstimateItem,
                Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                    And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.estimateID>>>>
                    >.Select(ProcessingGraph, estimateID, revisionID);
        }
    }
}