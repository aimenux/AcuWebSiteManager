using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Container for a transaction and related operations as they would exist for the currently processing transaction
    /// </summary>
    public sealed class MoveOperationQtyTotals
    {
        public readonly string Key;
        public AMMTran Transaction;
        public Dictionary<string, MoveOperationQtyTotal> OperationTotals { get; }
        private readonly Dictionary<string, decimal> _addtionalOperationWip;

        /// <summary>
        /// OperationTotals in the form of a List
        /// </summary>
        public List<MoveOperationQtyTotal> OperationTotalsList => OperationTotals?.Values.ToList();

        public List<AMProdOper> OperationsList => OperationTotals.Values.Select(totalsValue => totalsValue.ProdOper).ToList();

        public MoveOperationQtyTotals(AMMTran transaction)
        {
            Transaction = transaction ?? throw new PXArgumentException(nameof(transaction));
            OperationTotals = new Dictionary<string, MoveOperationQtyTotal>();
            Key = transaction.JoinDacKeys();
            _addtionalOperationWip = new Dictionary<string, decimal>();
        }

        public decimal GetAdditionalOperationWipCost(AMProdOper prodOper)
        {
            _addtionalOperationWip.TryGetValue(prodOper.JoinKeys(), out var wip);
            return wip;
        }

        public void SetAdditionalOperationWipCost(AMProdOper prodOper, decimal wip)
        {
            _addtionalOperationWip[prodOper.JoinKeys()] = wip;
        }

        public void SetTransactionTotalMoveBaseQty(AMProdOper prodOper, decimal qty)
        {
            var moveOperQtyTotal = GetOperationTotal(prodOper);
            if(moveOperQtyTotal == null)
            {
                return;
            }
            moveOperQtyTotal.TransactionTotalMoveBaseQty = qty;
            UpdateOperation(moveOperQtyTotal);
        }

        public MoveOperationQtyTotal GetLastOperation(AMProdItem prodItem)
        {
            return GetOperationTotal(prodItem?.OrderType, prodItem?.ProdOrdID, prodItem?.LastOperationID);
        }

        public MoveOperationQtyTotal AddUpdateOperation(AMProdOper amProdOper, AMWC workCenter)
        {
            return UpdateOperation(new MoveOperationQtyTotal(amProdOper, workCenter));
        }

        public MoveOperationQtyTotal AddUpdateOperation(MoveOperationQtyTotal projectedOperationQtyTotal, decimal previousMoveTotalBaseQty, decimal currentMoveBaseQty)
        {
            return UpdateOperation(new MoveOperationQtyTotal(projectedOperationQtyTotal, previousMoveTotalBaseQty, currentMoveBaseQty));
        }

        public MoveOperationQtyTotal GetSetOperationTotal(PXResult<AMProdOper, AMWC> operResult)
        {
            return GetOperationTotal((AMProdOper)operResult) ?? UpdateOperation(new MoveOperationQtyTotal(operResult, operResult));
        }

        public MoveOperationQtyTotal GetTransactionOperationTotal()
        {
            return GetOperationTotal(Transaction?.OrderType, Transaction?.ProdOrdID, Transaction?.OperationID);
        }

        public MoveOperationQtyTotal GetOperationTotal(IProdOper prodOper)
        {
            return GetOperationTotal(prodOper?.OrderType, prodOper?.ProdOrdID, prodOper?.OperationID);
        }

        public MoveOperationQtyTotal GetOperationTotal(string orderType, string prodOrdId, int? operationId)
        {
            if (string.IsNullOrWhiteSpace(orderType) || string.IsNullOrWhiteSpace(prodOrdId) ||
                operationId == null)
            {
                return null;
            }

            var key = GetOperKey(orderType, prodOrdId, operationId);
            return OperationTotals.ContainsKey(key) ? OperationTotals[key] : null;
        }

        private string GetOperKey(string orderType, string prodOrdId, int? operationId)
        {
            return string.Join("~", orderType.TrimIfNotNullEmpty(), prodOrdId.TrimIfNotNullEmpty(), operationId.GetValueOrDefault());
        }

        public MoveOperationQtyTotal UpdateOperation(MoveOperationQtyTotal moveOperationQtyTotal)
        {
            OperationTotals[moveOperationQtyTotal.Key] = moveOperationQtyTotal;
            return moveOperationQtyTotal;
        }
    }
}