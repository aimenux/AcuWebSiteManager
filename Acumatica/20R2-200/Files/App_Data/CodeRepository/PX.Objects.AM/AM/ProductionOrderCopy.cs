using System;
using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Graph for building production details based on another Production Order
    /// </summary>
    public class ProductionOrderCopy : ProductionBomCopy
    {
        public override void CreateProductionDetails(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || amProdItem.DetailSource != ProductionDetailSource.ProductionRef
                || string.IsNullOrWhiteSpace(amProdItem.SourceOrderType)
                || string.IsNullOrWhiteSpace(amProdItem.SourceProductionNbr))
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
                if (CurrentProdItem == null)
                {
                    throw new PXException(Messages.InvalidProductionNbr, amProdItem.OrderType.TrimIfNotNullEmpty(), amProdItem.ProdOrdID.TrimIfNotNullEmpty());
                }
                
                CacheLoadProdAttributes();

                CreateOperationDetail(order);
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

            if (string.IsNullOrWhiteSpace(order.SourceOrderType))
            {
                throw new PXArgumentException("Order Type");
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

            var toProdItem = ProcessingGraph.Caches<AMProdItem>().LocateElseCopy(CurrentProdItem);
            var productionCopyItem = FindProductionCopyItem(order);
            if (productionCopyItem == null)
            {
                return;
            }

            CopyProdItem(productionCopyItem, toProdItem);
            CopyOperations(order, productionCopyItem);
        }

        /// <summary>
        /// Copy ProdItem level fields
        /// </summary>
        /// <param name="copyFromProdItem"></param>
        /// <param name="copyToProdItem"></param>
        protected virtual void CopyProdItem(AMProdItem copyFromProdItem, AMProdItem copyToProdItem)
        {
            if (copyFromProdItem?.ProdOrdID == null)
            {
                throw new ArgumentNullException(nameof(copyFromProdItem));
            }

            if (copyToProdItem?.ProdOrdID == null)
            {
                throw new ArgumentNullException(nameof(copyToProdItem));
            }

            copyToProdItem.Descr = string.IsNullOrWhiteSpace(copyFromProdItem.Descr) ? null : copyFromProdItem.Descr; //Avoid legacy fixed string spacing

            if (copyFromProdItem.BOMID != null)
            {
                // Check the BOMID to be passed to new order
                AMBomItem copyBomItem = PXSelect<
                    AMBomItem, 
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                    .Select(ProcessingGraph, copyFromProdItem.BOMID, copyFromProdItem.BOMRevisionID);

                if (copyBomItem?.BOMID != null)
                {
                    copyToProdItem.BOMID = copyFromProdItem.BOMID;
                    copyToProdItem.BOMRevisionID = copyFromProdItem.BOMRevisionID;
                }
            }

            copyFromProdItem.CustomerID = copyFromProdItem.CustomerID;
            ProcessingGraph.Caches<AMProdItem>().Current = ProcessingGraph.Caches<AMProdItem>().Update(copyToProdItem);
        }

        protected virtual void CopyOperations(AMOrder order, AMProdItem productionCopyItem)
        {
            var fromRows = PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>
                >>.Select(ProcessingGraph, productionCopyItem.OrderType, productionCopyItem.ProdOrdID);

            foreach (AMProdOper fromRow in fromRows)
            {
                var toRow = PXCache<AMProdOper>.CreateCopy(fromRow);
                toRow.OrderType = CurrentProdItem?.OrderType;
                toRow.ProdOrdID = CurrentProdItem?.ProdOrdID;
                toRow.OperationID = null;
                toRow.TotalQty = CurrentProdItem?.QtytoProd.GetValueOrDefault();
                toRow.BaseTotalQty = CurrentProdItem?.BaseQtytoProd.GetValueOrDefault();
                toRow.QtytoProd = null;
                toRow.BaseQtytoProd = null;
                toRow.QtyComplete = null;
                toRow.BaseQtyComplete = null;
                toRow.QtyScrapped = null;
                toRow.BaseQtyScrapped = null;
                toRow.LineCntrMatl = null;
                toRow.LineCntrOvhd = null;
                toRow.LineCntrStep = null;
                toRow.LineCntrTool = null;
                toRow.StatusID = null;
                toRow.QtyComplete = null;
                toRow.ActStartDate = null;
                toRow.ActEndDate = null;
                toRow.StartDate = null;
                toRow.EndDate = null;
                toRow.NoteID = null;
                toRow.ActualLabor = 0m;
                toRow.ActualLaborTime = 0;
                toRow.ActualMachine = 0m;
                toRow.ActualMaterial = 0m;
                toRow.ActualTool = 0m;
                toRow.ActualFixedOverhead = 0m;
                toRow.ActualVariableOverhead = 0m;
                toRow.WIPAdjustment = 0m;
                toRow.ScrapAmount = 0m;
                toRow.WIPComp = 0m;
                
                var prodCopy = (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Insert(toRow);
                if (CurrentOrderType?.CopyNotesOper == true && prodCopy != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMProdOper>(), fromRow, ProcessingGraph.Caches<AMProdOper>(), prodCopy);
                }
                prodCopy = (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Update(prodCopy);

                CopyMaterial(order, productionCopyItem, fromRow);
                CopyStep(order, productionCopyItem, fromRow);
                CopyTool(order, productionCopyItem, fromRow);
                CopyOverheads(order, productionCopyItem, fromRow);
                CopyBomOperationLevelAttributes(prodCopy);
            }

            UpdatePlannedOperationTotals();

            CopyBomOrderLevelAttributes(CurrentProdItem);
            CopyOrderTypeAttributes(CurrentProdItem);
            CopyProductionOrderAttributes(CurrentProdItem);
        }

        protected virtual void CopyMaterial(AMOrder order, AMProdItem productionCopyItem, AMProdOper fromOper)
        {
            var fromRows = PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                        And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>>>
                >>.Select(ProcessingGraph, productionCopyItem.OrderType, productionCopyItem.ProdOrdID, fromOper?.OperationID);

            foreach (AMProdMatl fromRow in fromRows)
            {
                var toRow = PXCache<AMProdMatl>.CreateCopy(fromRow);
                toRow.OrderType = CurrentProdItem?.OrderType;
                toRow.ProdOrdID = CurrentProdItem?.ProdOrdID;
                toRow.LineID = null;
                toRow.TotalQtyRequired = null;
                toRow.BaseTotalQtyRequired = null;
                toRow.QtyActual = null;
                toRow.BaseQtyActual = null;
                toRow.QtyRemaining = null;
                toRow.BaseQtyRemaining = null;
                toRow.TotActCost = null;
                toRow.StatusID = null;

                if (toRow.IsByproduct.GetValueOrDefault() && CurrentProdItem != null && CurrentProdItem?.Function == OrderTypeFunction.Disassemble)
                {
                    // By products not supported on disassemble order types
                    continue;
                }

                var prodCopy = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Insert(toRow);
                if (CurrentOrderType?.CopyNotesMatl == true && prodCopy != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMProdMatl>(), fromRow, ProcessingGraph.Caches<AMProdMatl>(), prodCopy);
                }
                ProcessingGraph.Caches<AMProdMatl>().Update(prodCopy);
            }
        }

        protected virtual void CopyStep(AMOrder order, AMProdItem productionCopyItem, AMProdOper fromOper)
        {
            var fromRows = PXSelect<AMProdStep,
                Where<AMProdStep.orderType, Equal<Required<AMProdStep.orderType>>,
                    And<AMProdStep.prodOrdID, Equal<Required<AMProdStep.prodOrdID>>,
                        And<AMProdStep.operationID, Equal<Required<AMProdStep.operationID>>>>
                >>.Select(ProcessingGraph, productionCopyItem.OrderType, productionCopyItem.ProdOrdID, fromOper?.OperationID);

            foreach (AMProdStep fromRow in fromRows)
            {
                var toRow = PXCache<AMProdStep>.CreateCopy(fromRow);
                toRow.OrderType = CurrentProdItem?.OrderType;
                toRow.ProdOrdID = CurrentProdItem?.ProdOrdID;

                var prodCopy = (AMProdStep)ProcessingGraph.Caches<AMProdStep>().Insert(toRow);
                if (CurrentOrderType?.CopyNotesStep == true && prodCopy != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMProdStep>(), fromRow, ProcessingGraph.Caches<AMProdStep>(), prodCopy);
                }
                ProcessingGraph.Caches<AMProdStep>().Update(prodCopy);
            }
        }

        protected virtual void CopyTool(AMOrder order, AMProdItem productionCopyItem, AMProdOper fromOper)
        {
            var fromRows = PXSelect<AMProdTool,
                Where<AMProdTool.orderType, Equal<Required<AMProdTool.orderType>>,
                    And<AMProdTool.prodOrdID, Equal<Required<AMProdTool.prodOrdID>>,
                        And<AMProdTool.operationID, Equal<Required<AMProdTool.operationID>>>>
                >>.Select(ProcessingGraph, productionCopyItem.OrderType, productionCopyItem.ProdOrdID, fromOper?.OperationID);

            foreach (AMProdTool fromRow in fromRows)
            {
                var toRow = PXCache<AMProdTool>.CreateCopy(fromRow);
                toRow.OrderType = CurrentProdItem?.OrderType;
                toRow.ProdOrdID = CurrentProdItem?.ProdOrdID;
                toRow.TotActCost = null;
                toRow.TotActUses = null;

                var prodCopy = (AMProdTool)ProcessingGraph.Caches<AMProdTool>().Insert(toRow);
                if (CurrentOrderType?.CopyNotesTool == true && prodCopy != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMProdTool>(), fromRow, ProcessingGraph.Caches<AMProdTool>(), prodCopy);
                }
                ProcessingGraph.Caches<AMProdTool>().Update(prodCopy);
            }
        }

        protected virtual void CopyOverheads(AMOrder order, AMProdItem productionCopyItem, AMProdOper fromOper)
        {
            var fromRows = PXSelect<AMProdOvhd,
                Where<AMProdOvhd.orderType, Equal<Required<AMProdOvhd.orderType>>,
                    And<AMProdOvhd.prodOrdID, Equal<Required<AMProdOvhd.prodOrdID>>,
                        And<AMProdOvhd.operationID, Equal<Required<AMProdOvhd.operationID>>>>
                >>.Select(ProcessingGraph, productionCopyItem.OrderType, productionCopyItem.ProdOrdID, fromOper?.OperationID);

            foreach (AMProdOvhd fromRow in fromRows)
            {
                var toRow = PXCache<AMProdOvhd>.CreateCopy(fromRow);
                toRow.OrderType = CurrentProdItem?.OrderType;
                toRow.ProdOrdID = CurrentProdItem?.ProdOrdID;
                toRow.TotActCost = null;

                var prodCopy = (AMProdOvhd)ProcessingGraph.Caches<AMProdOvhd>().Insert(toRow);
                if (CurrentOrderType?.CopyNotesOvhd == true && prodCopy != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMProdOvhd>(), fromRow, ProcessingGraph.Caches<AMProdOvhd>(), prodCopy);
                }
                ProcessingGraph.Caches<AMProdOvhd>().Update(prodCopy);
            }
        }

        public virtual void CopyProductionOrderAttributes(AMProdItem amProdItem)
        {
            if (string.IsNullOrWhiteSpace(amProdItem?.SourceOrderType) || string.IsNullOrWhiteSpace(amProdItem.SourceProductionNbr))
            {
                return;
            }

            foreach (AMProdAttribute prodAttribute in PXSelect<AMProdAttribute,
                Where<AMProdAttribute.orderType, Equal<Required<AMProdAttribute.orderType>>,
                    And<AMProdAttribute.prodOrdID, Equal<Required<AMProdAttribute.prodOrdID>>,
                        And<AMProdAttribute.source, Equal<AMAttributeSource.production>>
                    >>>.Select(ProcessingGraph, amProdItem.SourceOrderType, amProdItem.SourceProductionNbr))
            {
                TryInsertAMProdAttribute(new AMProdAttribute
                {
                    OrderType = amProdItem.OrderType,
                    ProdOrdID = amProdItem.ProdOrdID,
                    Level = prodAttribute.Level,
                    OperationID = prodAttribute.OperationID,
                    Source = prodAttribute.Source,
                    AttributeID = prodAttribute.AttributeID,
                    Label = prodAttribute.Label,
                    Descr = prodAttribute.Descr,
                    Enabled = prodAttribute.Enabled,
                    TransactionRequired = prodAttribute.TransactionRequired,
                    Value = prodAttribute.Value
                });
            }
        }

        protected virtual AMProdItem FindProductionCopyItem(AMOrder order)
        {
            if (order == null)
            {
                return null;
            }

            return FindProductionCopyItem(order.SourceOrderType, order.SourceID);
        }

        public virtual AMProdItem FindProductionCopyItem(string orderType, string prodOrdID)
        {
            if (string.IsNullOrWhiteSpace(orderType) || string.IsNullOrWhiteSpace(prodOrdID))
            {
                return null;
            }

            return PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>
                    >.Select(ProcessingGraph, orderType, prodOrdID);
        }

        protected virtual void CopyBomOperationLevelAttributes(AMProdOper amProdOper)
        {
            if (amProdOper?.PhtmBOMOperationID == null || string.IsNullOrWhiteSpace(amProdOper.PhtmBOMID))
            {
                return;
            }

            CopyBomOperationLevelAttributes(amProdOper.OperationID, amProdOper.PhtmBOMID, amProdOper.PhtmBOMID, amProdOper.PhtmBOMOperationID);
        }
    }
}