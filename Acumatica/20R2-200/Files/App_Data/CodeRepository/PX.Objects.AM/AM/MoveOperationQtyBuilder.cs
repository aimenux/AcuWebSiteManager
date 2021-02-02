using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Calculate production move operation quantity for all transaction lines and all related operations
    /// </summary>
    public class MoveOperationQtyBuilder
    {
        public List<PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper>> AllTranRows;
        public List<AMMTran> AllTranRowsAMMTran => AllTranRows.Select(result => (AMMTran) result).ToList();
        protected Dictionary<string, Dictionary<string, PXResult<AMProdOper, AMWC>>> AllOperations;
        protected Dictionary<string, decimal> OperationMovingQty;

        /// <summary>
        /// For each move transaction line, the set of operations and their calculated move quantities
        /// </summary>
        public Dictionary<string, MoveOperationQtyTotals> MoveTranOperationTotals;

        /// <summary>
        /// Construct the builder based on a single transaction
        /// </summary>
        public static MoveOperationQtyTotals ConstructSingleBuilder(PXGraph graph, AMMTran singleTransaction)
        {
            PXResultset<AMMTran> results = PXSelectJoin<AMMTran,
                InnerJoin<InventoryItem, On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<AMOrderType, On<AMMTran.orderType, Equal<AMOrderType.orderType>>,
                    InnerJoin<AMProdItem, On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                            And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                        InnerJoin<AMProdOper, On<AMMTran.orderType, Equal<AMProdOper.orderType>,
                            And<AMMTran.prodOrdID, Equal<AMProdOper.prodOrdID>,
                                And<AMMTran.operationID, Equal<AMProdOper.operationID>>>>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>,
                    And<AMMTran.lineNbr, Equal<Required<AMMTran.lineNbr>>>>>
            >.SelectWindowed(graph, 0, 1, singleTransaction?.DocType, singleTransaction?.BatNbr, singleTransaction?.LineNbr);

            var firstTran = results.FirstTableItems.FirstOrDefault();

            if (firstTran == null)
            {
                if (singleTransaction?.OperationID != null)
                {
                    AMProdOper prodOper = PXSelect<
                            AMProdOper, 
                            Where<AMProdOper.orderType, Equal<Required<AMProdItem.orderType>>, 
                                And<AMProdOper.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>,
                                    And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                        .Select(graph, singleTransaction.OrderType, singleTransaction.ProdOrdID, singleTransaction.OperationID);

                    if (prodOper?.OperationID == null)
                    {
                        throw new PXException(Messages.InvalidOperationOnTransaction, AMDocType.GetDocTypeDesc(singleTransaction.DocType), singleTransaction.BatNbr, singleTransaction.LineNbr);
                    }
                }

                throw new PXException(Messages.UnableToGetFirstTransaction);
            }

            if (firstTran.OrderType == null || firstTran.ProdOrdID == null || firstTran.OperationID == null)
            {
                return null;
            }

            var allOperations = PXSelectJoin<AMProdOper,
                InnerJoin<AMWC, On<AMProdOper.wcID, Equal<AMWC.wcID>>>,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
            >.Select(graph, firstTran.OrderType, firstTran.ProdOrdID);

            if (allOperations == null || allOperations.Count == 0)
            {
                return null;
            }

            return new MoveOperationQtyBuilder(graph, results, allOperations).GetMoveOperationTotals(singleTransaction);
        }

        /// <summary>
        /// Construct the builder based on a set of transactions
        /// </summary>
        public static MoveOperationQtyBuilder Construct(PXGraph graph, PXResultset<AMMTran> allTranResultSet)
        {
            return Construct(graph, allTranResultSet, false);
        }

        /// <summary>
        /// Construct the builder based on a set of transactions
        /// </summary>
        public static MoveOperationQtyBuilder ConstructPostOperUpdate(PXGraph graph, PXResultset<AMMTran> allTranResultSet)
        {
            return Construct(graph, allTranResultSet, true);
        }

        /// <summary>
        /// Construct the builder based on a set of transactions
        /// </summary>
        private static MoveOperationQtyBuilder Construct(PXGraph graph, PXResultset<AMMTran> allTranResultSet, bool postOperUpdate)
        {
            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (allTranResultSet == null)
            {
                throw new PXArgumentException(nameof(allTranResultSet));
            }

            var firstTran = allTranResultSet.FirstTableItems.FirstOrDefault();

            var allOperations = PXSelectJoin<AMProdOper,
                InnerJoin<AMWC, On<AMProdOper.wcID, Equal<AMWC.wcID>>,
                    RightJoin<AMMTran,
                        On<AMProdOper.orderType, Equal<AMMTran.orderType>,
                            And<AMProdOper.prodOrdID, Equal<AMMTran.prodOrdID>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>
            >.Select(graph, firstTran?.DocType, firstTran?.BatNbr);

            return new MoveOperationQtyBuilder(graph, allTranResultSet, allOperations, postOperUpdate);
        }

        public MoveOperationQtyBuilder(PXGraph graph,
            PXResultset<AMMTran> allTranResultSet,
            PXResultset<AMProdOper> allOperResultSet)
            : this(graph, allTranResultSet, allOperResultSet, false)
        {
        }

        public MoveOperationQtyBuilder(PXGraph graph,
            PXResultset<AMMTran> allTranResultSet,
            PXResultset<AMProdOper> allOperResultSet,
            bool postOperUpdate)
            : this(allTranResultSet.ToList<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper>(),
                allOperResultSet.ToLocatedCopiedList<AMProdOper, AMWC>(graph), postOperUpdate)
        {
        }

        public MoveOperationQtyBuilder(List<PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper>> allTranRows,
            List<PXResult<AMProdOper, AMWC>> allOperations)
            : this(allTranRows, allOperations, false)
        {
        }

        /// <summary>
        /// Class constructor.
        /// To avoid invalid results, make sure allOperations uses a set of COPIED objects and not the original reference to the operations.
        /// </summary>
        public MoveOperationQtyBuilder(List<PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper>> allTranRows,
            List<PXResult<AMProdOper, AMWC>> allOperations, bool postOperUpdate)
        {
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
#endif
                AllTranRows = SortAllTranRows(allTranRows) ?? throw new PXArgumentException(nameof(allTranRows));

                AllOperations = new Dictionary<string, Dictionary<string, PXResult<AMProdOper, AMWC>>>();

                if (allOperations != null)
                {
                    foreach (PXResult<AMProdOper, AMWC> result in allOperations)
                    {
                        AllOperationsAdd(result);
                    }
                }

                MoveTranOperationTotals = new Dictionary<string, MoveOperationQtyTotals>();
                OperationMovingQty = new Dictionary<string, decimal>();
                ProcessAllTranOperations(postOperUpdate);
                PostProcessAllTranRows();
#if DEBUG
            }
            finally
            {
                sw.Stop();
                AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "Calculate Move Qty"));
            }
#endif
        }

        private List<PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper>> SortAllTranRows(List<PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper>> tranRows)
        {
            if (tranRows == null)
            {
                return null;
            }

            return tranRows
                .Where(x => ((AMMTran)x).ProdOrdID != null)
                .OrderBy(x => ((AMMTran)x).OrderType)
                .ThenBy(x => ((AMMTran)x).ProdOrdID)
                .ThenBy(x => ((AMProdOper)x).OperationCD)
                .ThenBy(x => ((AMMTran)x).LineNbr)
                .ToList();
        }

        private static string MakeKey(params string[] args)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(arg.TrimIfNotNullEmpty());
            }
            return sb.ToString();
        }

        private void AllOperationsAdd(PXResult<AMProdOper, AMWC> operationResult)
        {
            var operation = (AMProdOper) operationResult;

            if (operation == null || string.IsNullOrWhiteSpace(operation.ProdOrdID))
            {
                return;
            }

            var key = MakeKey(operation.OrderType, operation.ProdOrdID);

            Dictionary<string, PXResult<AMProdOper, AMWC>> operationsDic;
            if (!AllOperations.TryGetValue(key, out operationsDic))
            {
                operationsDic = new Dictionary<string, PXResult<AMProdOper, AMWC>>();
            }
            operationsDic[operation.JoinKeys()] = operationResult;

            AllOperations[key] = operationsDic;
        }

        private void UpdateOperation(AMProdOper prodOper)
        {
            var operResults = AllOperations[MakeKey(prodOper.OrderType, prodOper.ProdOrdID)];
            if (operResults == null)
            {
                return;
            }
            var operResult = operResults[prodOper.JoinKeys()];
            if (operResult == null)
            {
                return;
            }
            operResults[prodOper.JoinKeys()] = new PXResult<AMProdOper, AMWC>(prodOper, operResult);
        }

        private void UpdateMoveTransactionTotals(AMMTran ammTran, MoveOperationQtyTotals opTotals)
        {
            MoveTranOperationTotals[ammTran.JoinDacKeys()] = opTotals;
        }

        private List<PXResult<AMProdOper, AMWC>> GetOperations(AMMTran ammTran)
        {
            return GetOperations(ammTran?.OrderType, ammTran?.ProdOrdID);
        }

        private List<PXResult<AMProdOper, AMWC>> GetOperationsDescending(AMMTran ammTran)
        {
            return GetOperations(ammTran)?.OrderByDescending(x => ((AMProdOper) x).OperationCD)?.ToList();
        }

        private List<PXResult<AMProdOper, AMWC>> GetOperations(string orderType, string prodOrdID)
        {
            if (string.IsNullOrWhiteSpace(orderType) || string.IsNullOrWhiteSpace(prodOrdID))
            {
                return null;
            }

            Dictionary<string, PXResult<AMProdOper, AMWC>> operationsDic;
            if (!AllOperations.TryGetValue(MakeKey(orderType, prodOrdID), out operationsDic))
            {
                return null;
            }
            return operationsDic.Values.ToList();
        }

        public MoveOperationQtyTotals GetMoveOperationTotals(AMMTran ammTran)
        {
            if (ammTran == null)
            {
                return null;
            }

            var key = ammTran.JoinDacKeys();
            if (MoveTranOperationTotals.ContainsKey(key))
            {
                return MoveTranOperationTotals[key];
            }

            var newOpTotals = new MoveOperationQtyTotals(ammTran);
            MoveTranOperationTotals[key] = newOpTotals;
            return newOpTotals;
        }

        protected decimal GetOperationMovingQty(AMProdOper prodOper)
        {
            var key = prodOper.JoinKeys();
            if (OperationMovingQty.ContainsKey(key))
            {
                return OperationMovingQty[key];
            }
            OperationMovingQty[key] = 0m;
            return 0m;
        }

        protected decimal UpdateOperationMovingQty(AMProdOper prodOper, decimal qtyComplete)
        {
            var qtyCompleteTotal = GetOperationMovingQty(prodOper) + qtyComplete;
            OperationMovingQty[prodOper.JoinKeys()] = qtyCompleteTotal;
            return qtyCompleteTotal;
        }

        /// <summary>
        /// Process move qty results
        /// </summary>
        /// <param name="postOperUpdate">If builder called post processing we do not want to re-add the moving qty to operation totals...</param>
        private void ProcessAllTranOperations(bool postOperUpdate)
        {
            foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper> result in AllTranRows)
            {
                var ammTran = (AMMTran)result;
                var tranProdOper = (AMProdOper)result;

                if (ammTran?.LineNbr == null || tranProdOper?.OperationID == null)
                {
                    continue;
                }

                if (ammTran.Qty.GetValueOrDefault() >= 0 && ammTran.QtyScrapped.GetValueOrDefault() >= 0)
                {
                    ProcessPositiveMove(ammTran, tranProdOper, postOperUpdate);
                    continue;
                }

                ProcessNegativeMove(ammTran, tranProdOper, postOperUpdate);
            }
        }

        private void ProcessPositiveMove(AMMTran ammTran, AMProdOper tranProdOper, bool postOperupdate)
        {
            var opTotals = GetMoveOperationTotals(ammTran);
            var operationResults = GetOperationsDescending(ammTran);

            if (operationResults == null)
            {
                throw new Exception($"Missing operations for transaction {ammTran.DocType.TrimIfNotNullEmpty()}-{ammTran.BatNbr.TrimIfNotNullEmpty()}-{ammTran.LineNbr}. Order: {ammTran.OrderType.TrimIfNotNullEmpty()}-{ammTran.ProdOrdID.TrimIfNotNullEmpty()}");
            }

            decimal maxOperBaseQty = 0;
            foreach (PXResult<AMProdOper, AMWC> operResult in operationResults)
            {
                var operation = (AMProdOper)operResult;
                var opTotal = opTotals.GetSetOperationTotal(operResult);

                var baseMovedQty = operation.BaseQtyComplete.GetValueOrDefault();

                var sameOper = operation.OperationID == tranProdOper.OperationID;

                var tranScrapQty = sameOper && !postOperupdate ? ammTran.BaseQtyScrapped.GetValueOrDefault() : 0m;
                var tranMoveQty = sameOper && !postOperupdate && !ammTran.IsScrap.GetValueOrDefault() ? ammTran.BaseQty.GetValueOrDefault() : 0m;
                var tranTotalMoveQty = tranMoveQty + tranScrapQty;

                if (baseMovedQty + tranTotalMoveQty > maxOperBaseQty)
                {
                    maxOperBaseQty = baseMovedQty + tranTotalMoveQty;
                }

                var planMoveQty = maxOperBaseQty - baseMovedQty;

                if (planMoveQty != 0)
                {
                    UpdateOperationMovingQty(operation, planMoveQty);
                    opTotal.CurrentMoveBaseQty += planMoveQty;
                    opTotals.UpdateOperation(opTotal);
                }

                //Transfer new max to next oper when accounting for scrap qty for a new max operation qty
                if (baseMovedQty + planMoveQty + operation.BaseQtyScrapped.GetValueOrDefault() > maxOperBaseQty)
                {
                    maxOperBaseQty = baseMovedQty + planMoveQty + operation.BaseQtyScrapped.GetValueOrDefault();
                }

                var qtyComplete = planMoveQty - tranScrapQty < 0 ? 0m : planMoveQty - tranScrapQty;
                UpdateOper(operation, ammTran, qtyComplete, tranScrapQty);
            }
            UpdateMoveTransactionTotals(ammTran, opTotals);
        }

        private void ProcessNegativeMove(AMMTran ammTran, AMProdOper tranProdOper, bool postOperUpdate)
        {
            var opTotals = GetMoveOperationTotals(ammTran);
            var operationResults = GetOperations(ammTran);

            if (operationResults == null)
            {
                throw new Exception($"Missing operations for transaction {ammTran.DocType.TrimIfNotNullEmpty()}-{ammTran.BatNbr.TrimIfNotNullEmpty()}-{ammTran.LineNbr}. Order: {ammTran.OrderType.TrimIfNotNullEmpty()}-{ammTran.ProdOrdID.TrimIfNotNullEmpty()}");
            }

            decimal lastOperQtyComplete = 0;
            foreach (PXResult<AMProdOper, AMWC> operResult in operationResults)
            {
                var operation = (AMProdOper)operResult;
  
                var operCompare = string.CompareOrdinal(operation.OperationCD, tranProdOper.OperationCD);

                if (operCompare < 0)
                {
                    continue;
                }

                var opTotal = opTotals.GetSetOperationTotal(operResult);
                var sameOper = operCompare == 0;

                var tranScrapQty = sameOper && !postOperUpdate && ammTran.BaseQtyScrapped.GetValueOrDefault() < 0 ? Math.Abs(ammTran.BaseQtyScrapped.GetValueOrDefault()) : 0m;
                var tranMoveQty = sameOper && !postOperUpdate && ammTran.BaseQty.GetValueOrDefault() < 0 ? Math.Abs(ammTran.BaseQty.GetValueOrDefault()) : 0m;
                var tranTotalMoveQty = tranMoveQty + tranScrapQty;

                if (sameOper)
                {
                    lastOperQtyComplete = operation.BaseQtyComplete.GetValueOrDefault() - tranTotalMoveQty;
                    if (lastOperQtyComplete < 0)
                    {
                        lastOperQtyComplete = 0;
                    }
                }

                var min = Math.Min(operation.BaseQtyComplete.GetValueOrDefault(), lastOperQtyComplete);

                var planMoveQty = operation.BaseQtyComplete.GetValueOrDefault() - min;
                if (planMoveQty < 0)
                {
                    planMoveQty = 0;
                }
                planMoveQty *= -1;

                if (planMoveQty != 0)
                {
                    UpdateOperationMovingQty(operation, planMoveQty);
                    opTotal.CurrentMoveBaseQty += planMoveQty;
                    opTotals.UpdateOperation(opTotal);
                }

                var qtyComplete = planMoveQty + tranScrapQty;
                var updatedOper = UpdateOper(operation, ammTran, qtyComplete, tranScrapQty);
                lastOperQtyComplete = updatedOper?.BaseQtyComplete ?? 0m;
            }
            UpdateMoveTransactionTotals(ammTran, opTotals);
        }

        private AMProdOper UpdateOper(AMProdOper operation, AMMTran ammTran, decimal qtyComplete, decimal tranScrapQty)
        {
            if (operation == null || ammTran == null)
            {
                return operation;
            }

            operation.BaseQtyComplete = (operation.BaseQtyComplete.GetValueOrDefault() + qtyComplete).NotLessZero();
            operation.BaseQtyScrapped = (operation.BaseQtyScrapped.GetValueOrDefault() + tranScrapQty).NotLessZero();

            var laborCost = 0m;
            var laborTime = 0;
            if(ammTran.OrderType == operation.OrderType && ammTran.ProdOrdID == operation.ProdOrdID && ammTran.OperationID == operation.OperationID)
            {
                laborCost = ammTran.ExtCost.GetValueOrDefault();
                operation.ActualLabor = (operation.ActualLabor.GetValueOrDefault() + laborCost).NotLessZero();
                laborTime = ammTran.LaborTime.GetValueOrDefault();
                operation.ActualLaborTime = (operation.ActualLaborTime.GetValueOrDefault() + laborTime).NotLessZero();
            }

            var qtyChanged = qtyComplete != 0m || tranScrapQty != 0m;
            if (qtyChanged || laborCost != 0 || laborTime != 0)
            {
                if (qtyChanged)
                {
                    // BaseQtyRemaining - Formula from DAC:
                    // [PXFormula(typeof(SubNotLessThanZero<AMProdOper.baseTotalQty, Add<AMProdOper.baseQtyComplete, AMProdOper.baseQtyScrapped>>))]
                    operation.BaseQtyRemaining =
                        (operation.BaseTotalQty.GetValueOrDefault() -
                         operation.BaseQtyComplete.GetValueOrDefault() + operation.BaseQtyScrapped.GetValueOrDefault()).NotLessZero();

                    // QtyRemaining - Formula from DAC:
                    // [PXFormula(typeof(SubNotLessThanZero<AMProdOper.totalQty, Add<AMProdOper.qtyComplete, AMProdOper.qtyScrapped>>))]
                    operation.QtyRemaining =
                        (operation.TotalQty.GetValueOrDefault() -
                            operation.QtyComplete.GetValueOrDefault() + operation.QtyScrapped.GetValueOrDefault()).NotLessZero();
                }

                UpdateOperation(operation);
            }

            return operation;
        }

        /// <summary>
        /// Post process for looking all total calculated numbers
        /// </summary>
        private void PostProcessAllTranRows()
        {
            foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper> result in AllTranRows)
            {
                var ammTran = (AMMTran)result;
                var tranProdOper = (AMProdOper)result;

                if (ammTran?.LineNbr == null || tranProdOper?.OperationID == null)
                {
                    continue;
                }

                var opTotals = GetMoveOperationTotals(ammTran);
                var operationResults = GetOperationsDescending(ammTran);

                if (operationResults == null)
                {
                    throw new Exception($"Missing operations for transaction {ammTran.DocType.TrimIfNotNullEmpty()}-{ammTran.BatNbr.TrimIfNotNullEmpty()}-{ammTran.LineNbr}. Order: {ammTran.OrderType.TrimIfNotNullEmpty()}-{ammTran.ProdOrdID.TrimIfNotNullEmpty()}");
                }

                foreach (PXResult<AMProdOper, AMWC> operResult in operationResults)
                {
                    var operation = (AMProdOper)operResult;
                    var opTotal = opTotals.GetSetOperationTotal(operResult);

                    var totalMovingQty = GetOperationMovingQty(operation);

                    var update = false;
                    if (totalMovingQty != opTotal.TransactionTotalMoveBaseQty)
                    {
                        // Total all qty from direct/indirect numbers
                        opTotal.TransactionTotalMoveBaseQty = totalMovingQty;
                        update = true;
                    }

                    // When Qty Complete back to zero we want to keep the fixed labor. BaseQtyComplete here is already adjusted for the transaction qty
                    if (opTotal.HasLaborBeforeTransaction && operation?.BaseQtyComplete.GetValueOrDefault() > 0)
                    {
                        // If any hours are posted to the operation BEFORE the transaction... we will remove the fixed labor assuming fixed labor is already applied
                        opTotal.ExcludeFixedLabor(true);
                        update = true;
                    }

                    if (update)
                    {
                        opTotals.UpdateOperation(opTotal);
                    }
                }
                UpdateMoveTransactionTotals(ammTran, opTotals);
            }
        }
    }
}