using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// upgrade logic around TFS work item 1945
    /// </summary>
    internal sealed class TFS1945
    {
        private readonly int _upgradeFromVersion;
        private readonly PXGraph _graph;

        // Versions where bug 1945 data issues could exist. If not upgrading from these versions then we can skip the sql round trip.
        private bool _version2017R2UpgradeRequired => _upgradeFromVersion.BetweenInclusive(UpgradeVersions.Version2017R2Ver38, UpgradeVersions.Version2017R2Ver49);
        private bool _version2018R1UpgradeRequired => _upgradeFromVersion.BetweenInclusive(UpgradeVersions.Version2018R1Ver20, UpgradeVersions.Version2018R1Ver27);

        public List<Exception> ProcessExceptions { get; private set; }
        public bool HasProcessErrors => ProcessExceptions.Count > 0;

        public int ProductionOrderUpdateCounter;
        public int TransactionUpdateCounter;

        public TFS1945(int upgradeFromVersion, PXGraph graph)
        {
            _upgradeFromVersion = upgradeFromVersion;
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            ProcessExceptions = new List<Exception>();
        }

        public bool UpgradeRequired => _version2017R2UpgradeRequired || _version2018R1UpgradeRequired;

        private List<AMMTran> GetTransactionsToUpdate()
        {
            return PXSelect<
                AMMTran, 
                Where<AMMTran.released, Equal<True>,
                    And<AMMTran.iNBatNbr, IsNotNull,
                    And<
                    Where2<
                        Where<AMMTran.qty, Less<decimal0>,
                            And<AMMTran.tranAmt, Greater<decimal0>>>,
                        Or<Where<AMMTran.qty, Less<decimal0>,
                            And<AMMTran.tranAmt, Greater<decimal0>>>>>>>>>
                .Select(_graph)
                .ToFirstTableList();
        }

        private List<AMMTran> GetRelatedTransactions(List<AMMTran> transactions, AMProdItem prodItem)
        {
            if (transactions == null || transactions.Count == 0 || prodItem?.ProdOrdID == null)
            {
                return null;
            }

            return transactions.Where(x => x?.OrderType == prodItem.OrderType && x?.ProdOrdID == prodItem.ProdOrdID).ToList();
        }

        /// <summary>
        /// Get the list of orders effected by the updated transactions
        /// </summary>
        private List<AMProdItem> GetProductionOrders()
        {
            return PXSelectJoin<AMProdItem,
                InnerJoin<AMTranTFS1945UpgradeAggregate, On<AMTranTFS1945UpgradeAggregate.orderType, Equal<AMProdItem.orderType>,
                And<AMTranTFS1945UpgradeAggregate.prodOrdID, Equal<AMProdItem.prodOrdID>>>>>.Select(_graph).ToFirstTableList();
        }

        private void UpdateByProductionOrder(UpgradeProcess graph, AMProdItem prodItem, List<AMMTran> allTransactions, out Exception exception)
        {
            exception = null;
            graph.Clear();

            var relatedTransactions = GetRelatedTransactions(allTransactions, prodItem);
            if (relatedTransactions == null || relatedTransactions.Count == 0)
            {
                return;
            }

            var tranUpdateCounter = 0;

            try
            {
                // Correct transactions...
                foreach (var relatedTransaction in relatedTransactions)
                {
                    if (relatedTransaction?.InventoryID == null || relatedTransaction.Qty.GetValueOrDefault().IsSameSign(relatedTransaction.TranAmt.GetValueOrDefault()))
                    {
                        continue;
                    }

                    var newTranAmt = relatedTransaction.TranAmt.GetValueOrDefault() * -1;
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Production Order {relatedTransaction.OrderType} {relatedTransaction.ProdOrdID} Transaction {relatedTransaction.DocType}:{relatedTransaction.BatNbr}:{relatedTransaction.LineNbr}; Changing amount from {UomHelper.FormatCost(relatedTransaction.TranAmt)} to {UomHelper.FormatCost(newTranAmt)}");
#endif
                    UpdateProductionMaterial(graph, relatedTransaction, relatedTransaction.TranAmt.GetValueOrDefault(), newTranAmt);
                    relatedTransaction.TranAmt = newTranAmt;
                    graph.Transactions.Update(relatedTransaction);
                    tranUpdateCounter++;
                }

                if (tranUpdateCounter == 0)
                {
                    return;
                }

                // Update order actual totals to reflect the updated values...
                ProductionTransactionHelper.UpdateProdOperActualCostTotals(graph, prodItem);

                if (!graph.IsDirty)
                {
                    return;
                }

                graph.ProdItem.Current = prodItem;

                // Record a production event to know which orders were updated...
                graph.ProdEventRecords.Insert(ProductionEventHelper.BuildEvent(ProductionEventType.Info, $"Actual order totals updated in upgrade", prodItem));

                graph.Persist();
                ProductionOrderUpdateCounter++;
                TransactionUpdateCounter += tranUpdateCounter;
            }
            catch (Exception e)
            {
                exception = e;
                graph.Clear();
            }
        }

        private static void UpdateProductionMaterial(UpgradeProcess graph, AMMTran materialTran, decimal oldTotal, decimal newTotal)
        {
            if (materialTran == null || materialTran.DocType != AMDocType.Material || materialTran.MatlLineId == null || oldTotal == newTotal)
            {
                return;
            }

            var prodMatl = (AMProdMatl)PXSelect<
                AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                    And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                    And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>>>>>
            >
                .SelectWindowed(graph, 0, 1, materialTran.OrderType, materialTran.ProdOrdID, materialTran.OperationID, materialTran.MatlLineId);

            prodMatl = graph.ProdMatlRecords.Cache.LocateElse(prodMatl);
            if (prodMatl == null)
            {
                return;
            }

            var diff = oldTotal - newTotal;

#if DEBUG
            AMDebug.TraceWriteMethodName($"Production Order Material {prodMatl.OrderType}:{prodMatl.ProdOrdID}:({prodMatl.OperationID}):{prodMatl.LineID}; Changing TotActCost {UomHelper.FormatCost(prodMatl.TotActCost.GetValueOrDefault())} by {UomHelper.FormatCost(diff)} to {UomHelper.FormatCost(prodMatl.TotActCost.GetValueOrDefault() - diff)}");
#endif

            prodMatl.TotActCost = prodMatl.TotActCost.GetValueOrDefault() - diff;
            graph.ProdMatlRecords.Update(prodMatl);
        }

        public void ProcessUpgrade()
        {
            if (!UpgradeRequired)
            {
                return;
            }

            var productionOrders = GetProductionOrders();
            if (productionOrders == null || productionOrders.Count == 0)
            {
                return;
            }

            var transactions = GetTransactionsToUpdate();
            if (transactions == null || transactions.Count == 0)
            {
                return;
            }

            var graph = PXGraph.CreateInstance<UpgradeProcess>();

            foreach (var prodItem in productionOrders)
            {
                UpdateByProductionOrder(graph, prodItem, transactions, out var exception);

                if (exception != null)
                {
                    ProcessExceptions.Add(new Exception($"Error updating production order {prodItem.OrderType} {prodItem.ProdOrdID}: {exception.Message}", exception));
                }
            }
        }
    }

    /// <summary>
    /// Upgrade projection query only
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select4<
        AMMTran,
        Where<AMMTran.released, Equal<True>,
            And<AMMTran.iNBatNbr, IsNotNull,
                And<
                    Where2<
                        Where<AMMTran.qty, Less<decimal0>,
                            And<AMMTran.tranAmt, Greater<decimal0>>>,
                        Or<Where<AMMTran.qty, Less<decimal0>,
                            And<AMMTran.tranAmt, Greater<decimal0>>>>>>>>,
        Aggregate<
            GroupBy<AMMTran.orderType,
                GroupBy<AMMTran.prodOrdID>>>>), Persistent = false)]
    public class AMTranTFS1945UpgradeAggregate : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(BqlField = typeof(AMMTran.orderType))]
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
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(BqlField = typeof(AMMTran.prodOrdID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
    }
}