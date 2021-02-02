using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class ProductionTransactionHelper
    {
        public readonly AMReleaseProcess AmReleaseGraph;
        public bool IncludeScrap;
        public ProductionGLTranBuilder ProductionGlTranBuilder;
        public MaterialEntry ReleaseMatlGraph;
        /// <summary>
        /// Is the related backflush material transaction released
        /// </summary>
        internal bool BackflushMaterialReleased;
        /// <summary>
        /// Is the related backflush material transaction's inventory batch released
        /// </summary>
        internal bool BackflushMaterialINRegisterReleased;
        public WIPAdjustmentEntry WIPAdjustmentGraph;
        /// <summary>
        /// Container for released material transaction lines stored using row key values
        /// </summary>
        protected HashSet<string> refReleasedMaterialLines;

        public ProductionTransactionHelper(AMReleaseProcess graph)
        {
            AMDebug.TraceWriteMethodName();

            AmReleaseGraph = graph ?? throw new ArgumentNullException(nameof(graph));
            IncludeScrap = false;
            
            ProductionGlTranBuilder = new ProductionGLTranBuilder();

            ReleaseMatlGraph = PXGraph.CreateInstance<MaterialEntry>();
            ReleaseMatlGraph.Clear();
            ReleaseMatlGraph.IsImport = true;
            ReleaseMatlGraph.IsInternalCall = true;
            if (ReleaseMatlGraph.ampsetup.Current != null)
            {
                ReleaseMatlGraph.ampsetup.Current.RequireControlTotal = false;
            }

            // Create Instance of WIP Adjustment graph
            WIPAdjustmentGraph = PXGraph.CreateInstance<WIPAdjustmentEntry>();
            WIPAdjustmentGraph.Clear();
            
            if (WIPAdjustmentGraph.ampsetup.Current != null)
            {
                WIPAdjustmentGraph.ampsetup.Current.RequireControlTotal = false;
            }
        }

        public bool HasWIPAdjustments => WIPAdjustmentGraph.transactions.Select().Count > 0;

        protected AMProdItem CurrentAmProdItem => (AmReleaseGraph == null ? null : AmReleaseGraph.ProductionItems.Current ?? (AmReleaseGraph.ProductionItems.Current = AmReleaseGraph.ProductionItems.Select())) ?? new AMProdItem();

        /// <summary>
        /// Calculate operation quantity and build related production cost transactions (Labor, Machine, BF Material, Tools, Overhead)
        /// </summary>
        /// <param name="moveTransaction">Current production transaction object</param>
        /// <param name="moveOperQtyTotals">Pro processed move qty totals related to the processing transaction</param>
        public virtual void BuildRelatedTransactions(AMMTran moveTransaction, MoveOperationQtyTotals moveOperQtyTotals)
        {
            if (moveTransaction == null || string.IsNullOrWhiteSpace(moveTransaction.ProdOrdID))
            {
                return;
            }

            if (CurrentAmProdItem == null)
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                        Common.Cache.GetCacheName(typeof(AMProdItem))));
            }

            var orderUpdateOnly = AmReleaseGraph.UpdateProduction;
            var lastOperQtyComplete = CurrentAmProdItem == null ? 0m : CurrentAmProdItem.QtytoProd.GetValueOrDefault();
            var lastOperQtyCompleteBase = CurrentAmProdItem == null ? 0m : CurrentAmProdItem.BaseQtytoProd.GetValueOrDefault();

            if (moveOperQtyTotals == null || moveOperQtyTotals.OperationTotals.Count == 0)
            {
                throw new PXArgumentException(nameof(moveOperQtyTotals));
            }

            var hasReleasedBfMaterial = !orderUpdateOnly && HasReleasedBflushMaterial(moveTransaction);

            // Operation totals are stored in reverse order of operations (highest oper first) - make sure we are processing lowest to highest order
            foreach (var operationQtyTotal in moveOperQtyTotals.OperationTotals.Values.OrderBy(x => x.ProdOper.OperationCD))
            {
                var amProdOper = AmReleaseGraph.ProductionOpers.Cache.LocateElseCopy(operationQtyTotal.ProdOper);
                var reverseCost = moveTransaction.DocType == AMDocType.Disassembly && moveTransaction.TranType == AMTranType.Adjustment;
                if (operationQtyTotal.CurrentMoveBaseQty != 0)
                {
                    BackflushLabor(amProdOper, moveTransaction, operationQtyTotal.CurrentMoveBaseQty, 
                        operationQtyTotal.CurrentMoveVarLaborTime + operationQtyTotal.CurrentMoveFixLaborTime, orderUpdateOnly, reverseCost);
                    Machine(amProdOper, moveTransaction, operationQtyTotal.CurrentMoveBaseQty, orderUpdateOnly, reverseCost);
                    if (!orderUpdateOnly && !hasReleasedBfMaterial && CurrentAmProdItem.Function == OrderTypeFunction.Regular && !BackflushMaterialReleased && !BackflushMaterialINRegisterReleased)
                    {
                        BackflushMaterial(amProdOper, moveTransaction, operationQtyTotal.CurrentMoveBaseQty);
                    }
                    Tool(amProdOper, moveTransaction, operationQtyTotal.CurrentMoveBaseQty, orderUpdateOnly, reverseCost);
                    Overhead(amProdOper, moveTransaction, operationQtyTotal.CurrentMoveBaseQty, orderUpdateOnly, reverseCost);
                }

                var scrapQty = moveTransaction.OperationsEqual(amProdOper)
                    ? moveTransaction.BaseQtyScrapped.GetValueOrDefault()
                    : 0m;
                var qtyComp = operationQtyTotal.CurrentMoveBaseQty - scrapQty;
                if (operationQtyTotal.CurrentMoveBaseQty > 0 && qtyComp < 0)
                {
                    qtyComp = 0;
                }

                var qtyCompInProdUom = GetTranUomQty(moveTransaction, qtyComp);

                amProdOper.QtyComplete += qtyCompInProdUom;
                amProdOper.BaseQtyComplete += qtyComp;

                //this should cover neg move putting the order back into zero complete
                amProdOper.StatusID = ProductionOrderStatus.Released;

                if (amProdOper.QtyComplete.GetValueOrDefault() != 0
                    || amProdOper.QtyScrapped.GetValueOrDefault() != 0
                    || amProdOper.ActualLabor.GetValueOrDefault() != 0
                    || amProdOper.ActualMachine.GetValueOrDefault() != 0
                    || amProdOper.ActualLaborTime.GetValueOrDefault() != 0
                    || amProdOper.ActualMachineTime.GetValueOrDefault() != 0
                    || OperationHasMaterialIssued(amProdOper))
                {
                    if (Common.Dates.IsDateNull(amProdOper.ActStartDate) &&
                        (amProdOper.StatusID == ProductionOrderStatus.Released
                         || amProdOper.StatusID == ProductionOrderStatus.Planned))
                    {
                        amProdOper.ActStartDate = Common.Current.BusinessDate(AmReleaseGraph);
                    }

                    amProdOper.StatusID = ProductionOrderStatus.InProcess;

                    if (IsOperationComplete(amProdOper))
                    {
                        amProdOper.StatusID = ProductionOrderStatus.Completed;
                        amProdOper.ActEndDate = Common.Current.BusinessDate(AmReleaseGraph);
                    }
                }

                amProdOper.QtytoProd = lastOperQtyComplete;
                amProdOper.BaseQtytoProd = lastOperQtyCompleteBase;
                lastOperQtyComplete = amProdOper.QtyComplete.GetValueOrDefault();
                lastOperQtyCompleteBase = amProdOper.BaseQtyComplete.GetValueOrDefault();
                AmReleaseGraph.ProductionOpers.Update(amProdOper);
            }
        }

        //Necessary when user is modifying operations
        internal static void ResetOperationValues(PXCache operationCache, List<AMProdOper> operations, AMProdItem prodItem)
        {
            if(operationCache == null || prodItem == null || operations == null || operations.Count == 0 || !ProductionStatus.IsOpenOrder(prodItem))
            {
                return;
            }

            var operationsOrdered = operations.OrderOperations().ToList();

            var lastOper = new AMProdOper
            {
                QtyComplete = prodItem.QtytoProd.GetValueOrDefault(),
                BaseQtyComplete = prodItem.BaseQtytoProd.GetValueOrDefault(),
                StartDate = prodItem.StartDate,
                EndDate = prodItem.EndDate
            };

            foreach(var oper in operationsOrdered)
            {
                var locatedOper = operationCache.LocateElse(oper);
                var updated = locatedOper.QtytoProd != lastOper?.QtyComplete.GetValueOrDefault();
                locatedOper.QtytoProd = lastOper?.QtyComplete.GetValueOrDefault();
                locatedOper.BaseQtytoProd = lastOper?.BaseQtyComplete.GetValueOrDefault();

                if (locatedOper.StartDate == null)
                {
                    updated |= locatedOper.StartDate != lastOper.EndDate;
                    locatedOper.StartDate = lastOper.EndDate;
                    locatedOper.EndDate = lastOper.EndDate;
                }

                if (updated)
                {
                    operationCache.Update(locatedOper); 
                }

                lastOper = locatedOper;
            }
        }

        protected virtual HashSet<string> GetRefReleasedMaterialLines(string docType, string batNbr)
        {
            var hash = new HashSet<string>();
            foreach (AMMTran matlTran in PXSelectJoin<
            	AMMTran,
            	InnerJoin<AMBatch,
            		On<AMMTran.docType, Equal<AMBatch.docType>,
            		And<AMMTran.batNbr, Equal<AMBatch.batNbr>>>>,
            	Where<AMBatch.docType, Equal<AMDocType.material>,
            		And<AMBatch.released, Equal<True>,
	                And<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>,
            		And<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>>>>>>
            	.Select(AmReleaseGraph, docType, batNbr))
            {
                if (string.IsNullOrWhiteSpace(matlTran.OrigBatNbr))
                {
                    continue;
                }

                hash.Add(string.Join(":", matlTran.OrigDocType.TrimIfNotNullEmpty(),
                    matlTran.OrigBatNbr.TrimIfNotNullEmpty(), matlTran.OrigLineNbr));
            }

            return hash;
        }

        /// <summary>
        /// Does the given move/labor transaction contain released material references.
        /// </summary>
        /// <param name="moveLine">Move/Labor transaction line</param>
        /// <returns>True if released material backflush transactions exist</returns>
        protected virtual bool HasReleasedBflushMaterial(AMMTran moveLine)
        {
            if (refReleasedMaterialLines == null)
            {
                refReleasedMaterialLines = GetRefReleasedMaterialLines(moveLine.DocType, moveLine.BatNbr);
            }

            return moveLine != null && refReleasedMaterialLines != null
                                    && refReleasedMaterialLines.Contains(string.Join(":",
                                        moveLine.DocType.TrimIfNotNullEmpty(),
                                        moveLine.BatNbr.TrimIfNotNullEmpty(), moveLine.LineNbr));
        }

        /// <summary>
        /// Returns the converted qty based on the tran UOM
        /// </summary>
        /// <param name="tran">transaction being processed</param>
        /// <param name="baseQty">processing quantity in base units</param>
        /// <returns></returns>
        protected static decimal GetTranUomQty(AMMTran tran, decimal baseQty)
        {
            if (baseQty == 0)
            {
                return 0m;
            }

            var conversion = tran.BaseQty.GetValueOrDefault() == 0
                ? 0m
                : tran.Qty.GetValueOrDefault() / tran.BaseQty.GetValueOrDefault();
            if (conversion == 0)
            {
                conversion = tran.BaseQtyScrapped.GetValueOrDefault() == 0
                    ? 0m
                    : tran.QtyScrapped.GetValueOrDefault() / tran.BaseQtyScrapped.GetValueOrDefault();
            }
            if (conversion != 1)
            {
                //Must be a conversion involved..
                return UomHelper.QuantityRound(conversion * baseQty);
            }
            return baseQty;
        }

        public virtual void CreateScrapWipAdjustements(List<Tuple<AMMTran, MoveOperationQtyTotals>> transactions)
        {
            if (transactions == null)
            {
                return;
            }

            foreach (Tuple<AMMTran, MoveOperationQtyTotals> transaction in transactions)
            {
                CreateScrapWipAdjustment(transaction.Item1, transaction.Item2);
            }
        }

        private void CreateScrapWipAdjustment(AMMTran productionAmmTran, MoveOperationQtyTotals moveOperQtyTotals)
        {
            if (productionAmmTran == null || productionAmmTran.Released.GetValueOrDefault() || moveOperQtyTotals == null)
            {
                return;
            }

            var result = (PXResult<AMProdItem, InventoryItem>)PXSelectJoin<AMProdItem,
                InnerJoin<InventoryItem, On<AMProdItem.inventoryID, Equal<InventoryItem.inventoryID>>>,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>
                >>>.SelectWindowed(WIPAdjustmentGraph, 0, 1, productionAmmTran.OrderType, productionAmmTran.ProdOrdID);

            var amProdItem = (AMProdItem) result;
            if (amProdItem == null)
            {
                return;
            }

            if (WIPAdjustmentGraph.batch.Current == null)
            {
                // Check for Existing WIP Adjustment Batch
                AMBatch existingBatch = PXSelect<AMBatch,
                    Where<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>,
                        And<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>,
                            And<AMBatch.docType, Equal<AMDocType.wipAdjust>>
                        >>>.Select(WIPAdjustmentGraph, productionAmmTran.BatNbr, productionAmmTran.DocType);

                if (existingBatch != null && existingBatch.Released.GetValueOrDefault())
                {
                    PXTraceHelper.WriteInformation($"Existing WIP Batch {existingBatch.BatNbr.TrimIfNotNullEmpty()} Released. Creating a new batch");
                    existingBatch = null;
                }

                var amBatch = new AMBatch();
                if (existingBatch == null)
                {
                    // Create WIP Adjustment batch
                    amBatch = WIPAdjustmentGraph.batch.Insert(amBatch);
                    amBatch.Hold = true;
                    amBatch.TranDate = productionAmmTran.TranDate;
                    amBatch.FinPeriodID = productionAmmTran.FinPeriodID;
                    amBatch.TranDesc = Messages.ScrapTransaction;
                    amBatch.OrigBatNbr = productionAmmTran.BatNbr;
                    amBatch.OrigDocType = productionAmmTran.DocType;
                    WIPAdjustmentGraph.batch.Update(amBatch);
                }
                else
                {
                    WIPAdjustmentGraph.batch.Current = (AMBatch)WIPAdjustmentGraph.batch.Cache.CreateCopy(existingBatch);
                    WIPAdjustmentGraph.transactions.Select();
                }
            }

            foreach (var scrapTransaction in WIPAdjustmentGraph.CreateScrapTransactions(productionAmmTran, result, amProdItem, moveOperQtyTotals, AmReleaseGraph.ProductionOpers.Cache))
            {
                var insertedWipAdj = WIPAdjustmentGraph.transactions.Insert(new AMMTran());

                if (insertedWipAdj == null)
                {
                    throw new PXException($"unable to insert WIP scrap transaction for {AMDocType.GetDocTypeDesc(productionAmmTran.DocType)} batch {productionAmmTran.BatNbr.TrimIfNotNullEmpty()} line {productionAmmTran.LineNbr}");
                }

                scrapTransaction.DocType = insertedWipAdj.DocType;
                scrapTransaction.BatNbr = insertedWipAdj.BatNbr;
                scrapTransaction.LineNbr = insertedWipAdj.LineNbr;
                scrapTransaction.OrigDocType = productionAmmTran.DocType;
                scrapTransaction.OrigBatNbr = productionAmmTran.BatNbr;
                scrapTransaction.OrigLineNbr = productionAmmTran.LineNbr;
                scrapTransaction.NoteID = insertedWipAdj.NoteID;
                scrapTransaction.CreatedByID = insertedWipAdj.CreatedByID;
                scrapTransaction.CreatedByScreenID = insertedWipAdj.CreatedByScreenID;
                scrapTransaction.CreatedDateTime = insertedWipAdj.CreatedDateTime;
                scrapTransaction.LastModifiedByID = insertedWipAdj.CreatedByID;
                scrapTransaction.LastModifiedByScreenID = insertedWipAdj.LastModifiedByScreenID;
                scrapTransaction.LastModifiedDateTime = insertedWipAdj.LastModifiedDateTime;

                WIPAdjustmentGraph.transactions.Update(scrapTransaction);
            }
        }

        protected static List<AMProdOper> GetOperationList(PXGraph graph, AMMTran ammTran)
        {
            var list = new List<AMProdOper>();
            foreach (AMProdOper prodOper in PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
            >.Select(graph, ammTran.OrderType, ammTran.ProdOrdID))
            {
                list.Add(prodOper);
            }
            return list;
        }

        /// <summary>
        /// Indicates of the given operation has material issued or not
        /// </summary>
        /// <param name="amProdOper"></param>
        /// <returns>True when issued material found</returns>
        protected virtual bool OperationHasMaterialIssued(AMProdOper amProdOper)
        {
            if (amProdOper == null || string.IsNullOrWhiteSpace(amProdOper.ProdOrdID))
            {
                throw new PXArgumentException("amProdOper");
            }

            var q = PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                    And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                        And<AMProdMatl.baseQtyActual, NotEqual<decimal0>>>>>
                >.SelectWindowed(AmReleaseGraph, 0, 1, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID);
            return q != null && q.Count != 0;
        }

        /// <summary>
        /// Indicates if the operation has been completed
        /// </summary>
        /// <param name="amProdOper">current operation row</param>
        /// <returns>True if completed</returns>
        protected virtual bool IsOperationComplete(AMProdOper amProdOper)
        {
            decimal? prodItemQtyComplete = IncludeScrap
                ? CurrentAmProdItem.QtyComplete.GetValueOrDefault() + CurrentAmProdItem.QtyScrapped.GetValueOrDefault()
                : CurrentAmProdItem.QtyComplete.GetValueOrDefault();
            
            decimal? operQtyRemaining = amProdOper.TotalQty.GetValueOrDefault() - amProdOper.QtyComplete.GetValueOrDefault()
                - amProdOper.QtyScrapped.GetValueOrDefault();

            return prodItemQtyComplete.GetValueOrDefault() >= CurrentAmProdItem.QtytoProd.GetValueOrDefault()
                || operQtyRemaining <= 0m;
        }

        protected virtual void BackflushLabor(AMProdOper amProdOper, AMMTran productionAmmTran, decimal? currentFgQty, int laborTime,
            bool orderUpdateOnly, bool reverseCost)
        {
            if (!amProdOper.BFlush.GetValueOrDefault() || currentFgQty == 0 || laborTime == 0)
            {
                return;
            }

            var wcResult = (PXResult<AMWC, AMShift, AMShiftMst, AMLaborCode>)PXSelectJoin<AMWC,
                InnerJoin<AMShift, On<AMWC.wcID, Equal<AMShift.wcID>>,
                    InnerJoin<AMShiftMst, On<AMShift.shiftID, Equal<AMShiftMst.shiftID>>,
                        InnerJoin<AMLaborCode, On<AMShift.laborCodeID, Equal<AMLaborCode.laborCodeID>>>>>,
                Where<AMWC.wcID, Equal<Required<AMWC.wcID>>>,
                OrderBy<Asc<AMShift.shiftID>>>.SelectWindowed(AmReleaseGraph, 0, 1, amProdOper.WcID);

            AMWC amwc = wcResult;
            AMShift amShift = wcResult;
            AMShiftMst amShiftMst = wcResult;
            AMLaborCode amLaborCode = wcResult;

            if (string.IsNullOrWhiteSpace(amwc?.WcID))
            {
                var missingRecord = AM.Messages.GetLocal(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMWC)));
                throw new PXException($"{productionAmmTran.DocType.TrimIfNotNullEmpty()}-{productionAmmTran.BatNbr.TrimIfNotNullEmpty()}-{productionAmmTran.LineNbr}. {missingRecord}");
            }

            if (string.IsNullOrWhiteSpace(amShift?.WcID))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(amLaborCode?.LaborCodeID))
            {
                throw new PXException(Messages.GetLocal(Messages.MissingLaborCode), amShift.LaborCodeID.TrimIfNotNullEmpty());
            }

            var lt = laborTime * 1;
            if (lt < 0 && (amProdOper.ActualLaborTime.GetValueOrDefault() + laborTime) < 0)
            {
                // Make sure not returning more labor that originally issued
                lt = amProdOper.ActualLaborTime.GetValueOrDefault() * -1;
            }

            if (lt == 0)
            {
                return;
            }

            var wcCost = ShiftDiffType.GetShiftDifferentialCost(amwc, amShiftMst);
            var tranAmount = UomHelper.PriceCostRound(lt.ToHours() * wcCost) * (reverseCost ? -1 : 1);

            if (!orderUpdateOnly)
            {
                var descCodes = string.IsNullOrWhiteSpace(amwc?.WcID)
                    ? amLaborCode.LaborCodeID
                    : $"{amLaborCode.LaborCodeID.TrimIfNotNullEmpty()}, {amwc.WcID.TrimIfNotNullEmpty()}";

                ProductionGlTranBuilder.CreateAmGlLine(CurrentAmProdItem, productionAmmTran, tranAmount,
                    amLaborCode.LaborAccountID,
                    amLaborCode.LaborSubID, AMTranType.BFLabor, amProdOper.OperationID,
                    BuildTransactionDescription(AMTranType.BFLabor, descCodes),
                    laborTime, amwc?.WcID);
            }

            //These only work when not order update to feed overhead calculations for example when calculating pre-update transactions. The values will get updated later on in UpdateProdOperActualCostTotals
            amProdOper.ActualLabor += tranAmount;
            amProdOper.ActualLaborTime += laborTime;

            AmReleaseGraph.ProductionOpers.Update(amProdOper);
        }

        internal static decimal GetMachineUnitsPerHour(AMProdOper prodOper)
        {
            if (prodOper == null)
            {
                throw new ArgumentNullException(nameof(prodOper));
            }

            return prodOper.MachineUnitTime.GetValueOrDefault() == 0 ? 0m
                : UomHelper.Round(60m * prodOper.MachineUnits.GetValueOrDefault() / prodOper.MachineUnitTime.GetValueOrDefault(), 10);
        }

        protected virtual void Machine(AMProdOper amProdOper, AMMTran productionAmmTran, decimal currentFgQty, bool orderUpdateOnly, 
            bool reverseCost)
        {
            if (amProdOper.MachineUnitTime == 0)
            {
                return;
            }

            decimal? newHours = 0m;
            var machUnitsPerHour = GetMachineUnitsPerHour(amProdOper);
            if (currentFgQty != 0m && machUnitsPerHour != 0m)
            {
                newHours = currentFgQty / machUnitsPerHour;
            }

            if (newHours.GetValueOrDefault() != 0)
            {
                decimal totalMachCost = 0;
                foreach (PXResult<AMWCMach, AMMach> result in PXSelectJoin<AMWCMach,
                    InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>>,
                        Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>>
                            >.Select(AmReleaseGraph, amProdOper.WcID))
                {
                    var wcMachine = (AMWCMach)result;
                    var machine = (AMMach)result;

                    if (wcMachine == null || string.IsNullOrWhiteSpace(wcMachine.MachID)
                        || machine == null || string.IsNullOrWhiteSpace(machine.MachID)
                        || !machine.ActiveFlg.GetValueOrDefault())
                    {
                        continue;
                    }

                    int? machineAcctID = machine.MachAcctID;
                    int? machineSubID = machine.MachSubID;
                    var standardCost = machine.StdCost.GetValueOrDefault();
                    if (wcMachine.MachineOverride.GetValueOrDefault())
                    {
                        standardCost = wcMachine.StdCost.GetValueOrDefault();
                        machineAcctID = wcMachine.MachAcctID;
                        machineSubID = wcMachine.MachSubID;
                    }

                    var tranAmount = newHours.GetValueOrDefault() * standardCost * (reverseCost ? -1 : 1);
                    if (tranAmount != 0 && !orderUpdateOnly)
                    {
                        ProductionGlTranBuilder.CreateAmGlLine(CurrentAmProdItem, productionAmmTran, tranAmount,
                            machineAcctID, machineSubID, AMTranType.Machine, amProdOper.OperationID, BuildTransactionDescription(AMTranType.Machine, wcMachine.MachID), null, wcMachine.MachID);
                    }

                    totalMachCost += tranAmount;
                }

                amProdOper.ActualMachine += totalMachCost;
                amProdOper.ActualMachineTime += (int)(newHours.GetValueOrDefault() * 60);
                AmReleaseGraph.ProductionOpers.Update(amProdOper);
            }
        }

        /// <summary>
        /// Grab the current <see cref="AMMTran"/> and <see cref="AMMTranSplit"/> inserted into cache and return as a list to use for processing additional transactions
        /// </summary>
        /// <returns></returns>
        private List<AMMTran> GetMatlGraphInsertedTrans()
        {
            var list = new List<AMMTran>();
            foreach (AMMTranSplit insertedProdMatlSplit in ReleaseMatlGraph.splits.Cache.Inserted)
            {
                var tranFromSplit = (AMMTran)insertedProdMatlSplit;
                if (tranFromSplit == null)
                {
                    continue;
                }

                var parent = (AMMTran)ReleaseMatlGraph.transactions.Cache.Locate(tranFromSplit);
                if (parent != null)
                {
                    if (parent.Qty.GetValueOrDefault() < 0 && tranFromSplit.Qty.GetValueOrDefault() > 0)
                    {
                        tranFromSplit.Qty *= -1;
                        tranFromSplit.BaseQty *= -1;
                    }

                    tranFromSplit.OrderType = parent.OrderType;
                    tranFromSplit.ProdOrdID = parent.ProdOrdID;
                    tranFromSplit.OperationID = parent.OperationID;
                    tranFromSplit.MatlLineId = parent.MatlLineId;
                }

                list.Add(tranFromSplit);
            }

            return list;
        }

        protected virtual void BackflushMaterial(AMProdOper amProdOper, AMMTran productionAmmTran, decimal? currentFgQty)
        {
            var exceptionList = new List<string>();
            var matlbuilder = new MaterialTranBuilder(AmReleaseGraph)
            {
                // Needed to excluded hard allocated lot/serial
                StatusByLotSerial = true
            };
            var ammTrans = new List<AMMTran>();

            foreach (var t in GetMatlGraphInsertedTrans())
            {
                //Assist in tracking what has already been issued from previously inserted backflush lines...
                // Need to look at splits and convert to trans. Need splits for lot/serial nbr now that we are setting them via backflush
                matlbuilder.AddBuiltTranLine(t);
            }

            foreach (PXResult<AMProdMatl, AMOrderType, InventoryItem, INLotSerClass> result in PXSelectJoin<AMProdMatl, 
                InnerJoin<AMOrderType, On<AMProdMatl.orderType, Equal<AMOrderType.orderType>>,
                InnerJoin<InventoryItem, On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<INLotSerClass, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>>>>, 
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                    And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                    And<AMProdMatl.bFlush, Equal<True>>>>>
                >.Select(AmReleaseGraph, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID))
            {
                var amProdMatlBflush = AmReleaseGraph.ProductionMatl.Cache.LocateElseCopy((AMProdMatl)result);
                var inventoryItem = (InventoryItem) result;
                var orderType = (AMOrderType) result;

                if (string.IsNullOrWhiteSpace(amProdMatlBflush?.ProdOrdID) || string.IsNullOrWhiteSpace(inventoryItem?.InventoryCD))
                {
                    continue;
                }

                var qtyToIssue = amProdMatlBflush.QtyActual.GetValueOrDefault() != 0 && amProdMatlBflush.IsFixedMaterial.GetValueOrDefault() 
                    ? 0m 
                    : amProdMatlBflush.GetTotalReqQty(currentFgQty.GetValueOrDefault());

                var siteId = CurrentAmProdItem.SiteID;
                int? locationId = null;
                if (amProdMatlBflush.SiteID != null)
                {
                    siteId = amProdMatlBflush.SiteID;
                    locationId = amProdMatlBflush.LocationID;
                }

                var trans = matlbuilder.BuildTransactions(
                        amProdMatlBflush,
                        (InventoryItem) result,
                        (INLotSerClass) result,
                        qtyToIssue,
                        amProdMatlBflush.UOM,
                        siteId,
                        locationId,
                        out var newException
                    );

                if (newException != null && orderType != null && orderType.BackflushUnderIssueMaterial == SetupMessage.ErrorMsg)
                {
                    exceptionList.Add(newException);
                }

                if (trans != null && trans.Count > 0)
                {
                    ammTrans.AddRange(trans);
                }
            }

            if (exceptionList.Count > 0)
            {
                throw new PXException(string.Join("; ", exceptionList));
            }

            MaterialTranBuilder.CreateMaterialTransaction(ReleaseMatlGraph, ammTrans, productionAmmTran);
        }

        protected virtual void Tool(AMProdOper amProdOper, AMMTran productionAmmTran, decimal? currentFgQty, bool orderUpdateOnly, 
            bool reverseCost)
        {
            foreach (PXResult<AMProdTool, AMToolMst> result in PXSelectJoin<AMProdTool, 
                InnerJoin<AMToolMst, On<AMProdTool.toolID, Equal<AMToolMst.toolID>>>,
                Where<AMProdTool.orderType, Equal<Required<AMProdTool.orderType>>,
                    And<AMProdTool.prodOrdID, Equal<Required<AMProdTool.prodOrdID>>,
                    And<AMProdTool.operationID, Equal<Required<AMProdTool.operationID>>,
                    And<AMProdTool.qtyReq, Greater<decimal0>>>>
                >>.Select(AmReleaseGraph, productionAmmTran.OrderType, productionAmmTran.ProdOrdID, amProdOper.OperationID))
            {
                var amProdTool = (AMProdTool) result;
                var amToolMst = (AMToolMst) result;

                if (string.IsNullOrWhiteSpace(amToolMst?.ToolID) || string.IsNullOrWhiteSpace(amProdTool?.ToolID))
                {
                    continue;
                }

                var actualUses = currentFgQty.GetValueOrDefault() * amProdTool.QtyReq.GetValueOrDefault();
                if (actualUses == 0)
                {
                    continue;
                }

                var tranAmount = actualUses * amToolMst.UnitCost.GetValueOrDefault() * (reverseCost ? -1 : 1);
                if (tranAmount != 0)
                {
                    if (!orderUpdateOnly)
                    {
                        ProductionGlTranBuilder.CreateAmGlLine(CurrentAmProdItem, productionAmmTran,
                            tranAmount, amToolMst.AcctID, amToolMst.SubID,
                            AMTranType.Tool, amProdOper.OperationID, BuildTransactionDescription(AMTranType.Tool, amToolMst.ToolID), null, amToolMst.ToolID);
                    }

                    amToolMst.ActualCost = amToolMst.ActualCost.GetValueOrDefault() + tranAmount;
                    amProdTool.TotActCost = amProdTool.TotActCost.GetValueOrDefault() + tranAmount;
                }

                amToolMst.ActualUses += actualUses;
                amProdTool.TotActUses += actualUses;

                AmReleaseGraph.ToolMaster.Update(amToolMst);
                AmReleaseGraph.ProductionTool.Update(amProdTool);
            }
        }

        protected virtual void Overhead(AMProdOper amProdOper, AMMTran productionAmmTran, decimal? currentFgQty, bool orderUpdateOnly,
            bool reverseCost)
        {
            var scrapQty = productionAmmTran.OrderType.EqualsWithTrim(amProdOper.OrderType) &&
                           productionAmmTran.ProdOrdID.EqualsWithTrim(amProdOper.ProdOrdID) &&
                           productionAmmTran.OperationID == amProdOper.OperationID
                ? productionAmmTran.BaseQtyScrapped.GetValueOrDefault()
                : 0m;

            foreach (PXResult<AMProdOvhd, AMOverhead> result in PXSelectJoin<AMProdOvhd,
                InnerJoin<AMOverhead, On<AMProdOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>, 
                Where<AMProdOvhd.orderType, Equal<Required<AMProdOvhd.orderType>>, 
                    And<AMProdOvhd.prodOrdID, Equal<Required<AMProdOvhd.prodOrdID>>,
                    And<AMProdOvhd.operationID, Equal<Required<AMProdOvhd.operationID>>,
                    And<AMProdOvhd.ovhdID, NotEqual<StringEmpty>>>>>
                        >.Select(AmReleaseGraph, productionAmmTran.OrderType, productionAmmTran.ProdOrdID, amProdOper.OperationID))
            {
                var amProdOvhd = AmReleaseGraph.ProductionOvhd.Cache.LocateElse((AMProdOvhd)result);
                var amOverhead = (AMOverhead) result;

                var isReversingEntry = currentFgQty < 0 || scrapQty < 0;

                var ovhdAmt = isReversingEntry
                    ? CalcReturnOverheadAmount(amProdOper, amProdOvhd, amOverhead, currentFgQty.GetValueOrDefault(), scrapQty)
                    : CalcOverheadAmount(amProdOper, amProdOvhd, amOverhead, currentFgQty.GetValueOrDefault(), scrapQty);

                ovhdAmt = ovhdAmt * (reverseCost ? -1 : 1);
                if (ovhdAmt == 0)
                {
                    continue;
                }

                if (!orderUpdateOnly)
                {
                    var tranType = amOverhead.OvhdType == OverheadType.FixedType
                        ? AMTranType.FixOvhd
                        : AMTranType.VarOvhd;

                    ProductionGlTranBuilder.CreateAmGlLine(
                        CurrentAmProdItem,
                        productionAmmTran,
                        ovhdAmt,
                        amOverhead.AcctID,
                        amOverhead.SubID,
                        tranType,
                        amProdOper.OperationID,
                        BuildTransactionDescription(tranType, amProdOvhd.OvhdID), null, amProdOvhd.OvhdID);
                }

                amProdOvhd.TotActCost += ovhdAmt;
                AmReleaseGraph.ProductionOvhd.Update(amProdOvhd);
            }
        }

        protected virtual decimal CalcOverheadAmount(AMProdOper amProdOper, AMProdOvhd amProdOvhd,
            AMOverhead amOverhead, decimal baseMoveQty, decimal scrapQty)
        {
            if (amOverhead == null || string.IsNullOrWhiteSpace(amOverhead.OvhdID)
                || amProdOvhd == null || string.IsNullOrWhiteSpace(amProdOvhd.OvhdID)
                || amProdOper?.ProdOrdID == null)
            {
                return 0;
            }

            var ovhdAmt = 0m;

            var checkAgainstTotal = true;
            var factorCostRate = amProdOvhd.OFactor.GetValueOrDefault() * amOverhead.CostRate.GetValueOrDefault();
            switch (amOverhead.OvhdType)
            {
                case OverheadType.FixedType:
                    if (amProdOvhd.TotActCost.GetValueOrDefault() == 0)
                    {
                        ovhdAmt = 0;
                        if (baseMoveQty > 0)
                        {
                            ovhdAmt = factorCostRate;
                        }
                    }
                    else
                    {
                        checkAgainstTotal = false;
                        ovhdAmt = 0;
                    }
                    break;

                case OverheadType.VarLaborHrs:
                    ovhdAmt = factorCostRate * amProdOper.ActualLaborTime.ToHours(9).GetValueOrDefault();
                    break;

                case OverheadType.VarMachHrs:
                    ovhdAmt = factorCostRate * amProdOper.ActualMachineTime.ToHours(9).GetValueOrDefault();
                    break;

                case OverheadType.VarLaborCost:
                    ovhdAmt = factorCostRate * amProdOper.ActualLabor.GetValueOrDefault();
                    break;

                case OverheadType.VarMatlCost:
                    decimal matlTotActCostSum = 0;
                    foreach (AMProdMatl amProdMatl in PXSelect<AMProdMatl,
                        Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                            And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                            And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>>>>
                            >.Select(AmReleaseGraph, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID))
                    {
                        matlTotActCostSum += amProdMatl.TotActCost.GetValueOrDefault();
                    }

                    if (matlTotActCostSum != 0)
                    {
                        ovhdAmt = factorCostRate * matlTotActCostSum;
                    }

                    break;
                case OverheadType.VarQtyComp:
                    checkAgainstTotal = false;
                    var qtyComp = baseMoveQty - scrapQty;
                    if (qtyComp < 0)
                    {
                        qtyComp = 0;
                    }

                    ovhdAmt = factorCostRate * qtyComp;
                    break;
                default:
                    checkAgainstTotal = false;
                    ovhdAmt = factorCostRate * baseMoveQty;
                    break;
            }

            if (ovhdAmt == 0)
            {
                return 0;
            }

            if (checkAgainstTotal)
            {
                ovhdAmt = ovhdAmt - amProdOvhd.TotActCost.GetValueOrDefault();
            }

            return ovhdAmt;
        }

        protected virtual decimal CalcReturnOverheadAmount(AMProdOper amProdOper, AMProdOvhd amProdOvhd,
            AMOverhead amOverhead, decimal baseMoveQty, decimal scrapQty)
        {
            var ovhdAmt = 0m;
            if (amOverhead == null || string.IsNullOrWhiteSpace(amOverhead.OvhdID)
                || amProdOvhd == null || string.IsNullOrWhiteSpace(amProdOvhd.OvhdID)
                || amProdOper?.ProdOrdID == null)
            {
                return ovhdAmt;
            }
            
            var origProdOper = (AMProdOper)AmReleaseGraph.ProductionOpers.Cache.GetOriginal(amProdOper);
            if (origProdOper?.ProdOrdID == null)
            {
                return ovhdAmt;
            }

            var returnQty = Math.Abs(baseMoveQty);
            var returnScrap = Math.Abs(scrapQty);

            var operBaseQtyComplete = (amProdOper.BaseQtyComplete.GetValueOrDefault() - returnQty - scrapQty).NotLessZero();

            var factorCostRate = amProdOvhd.OFactor.GetValueOrDefault() * amOverhead.CostRate.GetValueOrDefault();
            switch (amOverhead.OvhdType)
            {
                case OverheadType.FixedType:
                    if (operBaseQtyComplete == 0)
                    {
                        ovhdAmt = amProdOvhd.TotActCost.GetValueOrDefault();
                    }

                    break;

                case OverheadType.VarLaborHrs:
                    var laborTimeReturn = (origProdOper.ActualLaborTime.GetValueOrDefault() - amProdOper.ActualLaborTime.GetValueOrDefault()).NotLessZero();
                    ovhdAmt = factorCostRate * laborTimeReturn.ToHours(9);
                    break;

                case OverheadType.VarMachHrs:
                    var machineTimeReturn = (origProdOper.ActualMachineTime.GetValueOrDefault() - amProdOper.ActualMachineTime.GetValueOrDefault()).NotLessZero();
                    ovhdAmt = factorCostRate * machineTimeReturn.ToHours(9);
                    break;

                case OverheadType.VarLaborCost:
                    var laborCostReturn = (origProdOper.ActualLabor.GetValueOrDefault() - amProdOper.ActualLabor.GetValueOrDefault()).NotLessZero();
                    ovhdAmt = factorCostRate * laborCostReturn;
                    break;

                case OverheadType.VarMatlCost:

                    // If full qty returning just return the full overhead amount
                    if (operBaseQtyComplete == 0)
                    {
                        ovhdAmt = amProdOvhd.TotActCost.GetValueOrDefault();
                        break;
                    }

                    //else calculate material per each using totactcost for a per unit cost
                    // This is incorrect because we really do not know what was used from the last transaction.
                    //  Task #2908 to calc overhead by matl on the material transaction.

                    // TEMP REMOVAL UNTIL 2908 is resolved
                    //var planMatlTotalCost = ProductionCostCalculator.GetPlannedMaterialCost(AmReleaseGraph, amProdOper, amProdOper.BaseTotalQty.GetValueOrDefault())?.TotalCost ?? 0m;
                    //if (planMatlTotalCost != 0 && amProdOper.BaseTotalQty.GetValueOrDefault() != 0)
                    //{
                    //    var perUnit = planMatlTotalCost / amProdOper.BaseTotalQty.GetValueOrDefault();
                    //    var totalReturn = PXCurrencyAttribute.BaseRound(AmReleaseGraph, (returnQty + returnScrap) * perUnit);
                    //    ovhdAmt = factorCostRate * totalReturn;
                    //}

                    break;
                case OverheadType.VarQtyComp:
                    var qtyComp = returnQty + returnScrap;
                    ovhdAmt = factorCostRate * qtyComp;
                    break;
                default:
                    ovhdAmt = factorCostRate * returnQty;
                    break;
            }

            if (ovhdAmt > amProdOvhd.TotActCost.GetValueOrDefault())
            {
                ovhdAmt = amProdOvhd.TotActCost.GetValueOrDefault();
            }

            return ovhdAmt * -1;
        }

        /// <summary>
        /// WIP Complete by operation is not calculated as a record in the cost batch (one record for each operation)
        /// </summary>
        public virtual void CreateWipCompleteCostEntry(AMMTran moveTran, AMProdOper wipCompleteProdOper, decimal wipCompleteTotal, decimal moveQty)
        {
            if (wipCompleteTotal == 0)
            {
                return;
            }

            if (moveTran?.DocType == null)
            {
                throw new PXArgumentException(nameof(moveTran));
            }

            if (wipCompleteProdOper?.ProdOrdID == null)
            {
                throw new PXArgumentException(nameof(wipCompleteProdOper));
            }

            ProductionGlTranBuilder.CreateNonGLCostBatchEntry(
                moveTran,
                wipCompleteProdOper.OperationID,
                wipCompleteTotal,
                moveQty,
                AMTranType.OperWIPComplete,
                BuildTransactionDescription(AMTranType.OperWIPComplete, wipCompleteProdOper.OperationCD));
        }

        public static bool UsingAlternativeUom(AMMTran moveTran)
        {
            return moveTran != null
                && (moveTran.Qty.GetValueOrDefault() != moveTran.BaseQty.GetValueOrDefault()
                || moveTran.QtyScrapped.GetValueOrDefault() != moveTran.BaseQtyScrapped.GetValueOrDefault());
        }

        public static string BuildTransactionDescription(string tranType, string costDesc = "")
        {
            string tranDesc = AMTranType.GetTranDescription(tranType);

            if (!string.IsNullOrWhiteSpace(tranDesc) && !string.IsNullOrWhiteSpace(costDesc))
            {
                tranDesc = $"{tranDesc.Trim()}: {costDesc.Trim()}";
            }

            return tranDesc;
        }

        /// <summary>
        /// Finds the correct operation record to use for calculating operation totals.
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="amProdOper">Selected AMProdOper record</param>
        /// <param name="updatedOperations">processing storage of updated operations</param>
        /// <returns>The correct AMProdOper record to use during processing</returns>
        private static AMProdOper FindProdOperForActualCost(PXGraph graph, AMProdOper amProdOper, ref Dictionary<int, AMProdOper> updatedOperations)
        {
            if(amProdOper == null || string.IsNullOrWhiteSpace(amProdOper.ProdOrdID) || amProdOper.OperationID == null)
            {
                throw new PXArgumentException(nameof(amProdOper));
            }

            if (updatedOperations.ContainsKey(amProdOper.OperationID.GetValueOrDefault()))
            {
                return updatedOperations[amProdOper.OperationID.GetValueOrDefault()];
            }

            AMProdOper prodOper = graph.Caches<AMProdOper>().LocateElseCopy(amProdOper);

            //Set operation costs to zero to add up later. Call the update at the end should still allow sumcalc to work.
            prodOper.ActualLabor = 0m;
            prodOper.ActualLaborTime = 0;
            prodOper.ActualMachine = 0m;
            prodOper.ActualMaterial = 0m;
            prodOper.ActualTool = 0m;
            prodOper.ActualFixedOverhead = 0m;
            prodOper.ActualVariableOverhead = 0m;
            prodOper.WIPAdjustment = 0m;
            prodOper.ScrapAmount = 0m;
            prodOper.WIPComp = 0m;
            prodOper.ActualSubcontract = 0m;


            updatedOperations.Add(prodOper.OperationID.GetValueOrDefault(), prodOper);

            return prodOper;
        }

        public static void UpdateProdOperActualCostTotals(PXGraph graph, AMProdItem amproditem)
        {
            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (amproditem == null
                || string.IsNullOrWhiteSpace(amproditem.OrderType)
                || string.IsNullOrWhiteSpace(amproditem.ProdOrdID))
            {
                throw new PXArgumentException(nameof(amproditem));
            }

            AMProdTotal amprodtotal = PXSelect<AMProdTotal,
                Where<AMProdTotal.orderType, Equal<Required<AMProdTotal.orderType>>,
                    And<AMProdTotal.prodOrdID, Equal<Required<AMProdTotal.prodOrdID>>>>
            >.Select(graph, amproditem.OrderType, amproditem.ProdOrdID);

            UpdateProdOperActualCostTotals(graph, amproditem, amprodtotal);
        }

        /// <summary>
        /// Calculates all costs for the AMProdTotal Actual Values
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="amproditem">Production Item DAC to update costs</param>
        /// <param name="amprodtotal"></param>
        public static void UpdateProdOperActualCostTotals(PXGraph graph, AMProdItem amproditem, AMProdTotal amprodtotal)
        {
            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (string.IsNullOrWhiteSpace(amproditem?.OrderType) || string.IsNullOrWhiteSpace(amproditem.ProdOrdID))
            {
                throw new PXArgumentException(nameof(amproditem));
            }

            // Check ProdTotal for record, add new if null
            if (amprodtotal == null)
            {
                amprodtotal = new AMProdTotal
                {
                    OrderType = amproditem.OrderType,
                    ProdOrdID = amproditem.ProdOrdID
                };

                if (graph.Caches[typeof(AMProdTotal)].IsDirty != true)
                {
                    amprodtotal = (AMProdTotal)graph.Caches[typeof(AMProdTotal)].Insert(amprodtotal);
                }
            }

            if (amprodtotal.PlanCostDate == null)
            {
                amprodtotal.PlanCostDate = amprodtotal.CreatedDateTime ?? amproditem.ProdDate;
                amprodtotal = (AMProdTotal)graph.Caches[typeof(AMProdTotal)].Update(amprodtotal);
            }

            graph.Caches[typeof(AMProdTotal)].Current = amprodtotal;
            var prodItemWipCompDirect = 0m;

            // DO NOT UPDATE AMPRODTOTAL BELOW HERE...
            var isStandardCostItem = amproditem.CostMethod == CostMethod.Standard;
            var updatedOperations = new Dictionary<int, AMProdOper>();
            //Index: AMMTran_IX_ReleasedByOrder
            foreach (PXResult<AMMTran, AMProdOper> result in PXSelectJoin<AMMTran,
                InnerJoin<AMProdOper, On<AMMTran.orderType, Equal<AMProdOper.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMMTran.operationID, Equal<AMProdOper.operationID>>>>>,
                Where<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                        And<AMMTran.released, Equal<True>>>>
            >.Select(graph, amproditem.OrderType, amproditem.ProdOrdID))
            {
                var tranLine = Common.Cache.FindRow(graph, (AMMTran) result);
                var prodOper = FindProdOperForActualCost(graph, result, ref updatedOperations);

                if (string.IsNullOrWhiteSpace(tranLine?.ProdOrdID) || string.IsNullOrWhiteSpace(prodOper?.ProdOrdID))
                {
                    continue;
                }

                var isMoveTran = tranLine.DocType == AMDocType.Labor || tranLine.DocType == AMDocType.Move;
                switch (tranLine.TranType)
                {
                    case AMTranType.BFLabor:        //Labor Totals
                    case AMTranType.Labor:
                        prodOper.ActualLabor += tranLine.TranAmt.GetValueOrDefault();
                        prodOper.ActualLaborTime += tranLine.LaborTime.GetValueOrDefault();
                        break;
                    case AMTranType.Machine:        //Machine Totals
                        prodOper.ActualMachine += tranLine.TranAmt.GetValueOrDefault();
                        break;
                    case AMTranType.Return:
                    case AMTranType.Issue:          //Material/Sub Totals
                        if (tranLine.SubcontractSource == null || tranLine.SubcontractSource == AMSubcontractSource.None)
                        {
                            prodOper.ActualMaterial += tranLine.TranAmt.GetValueOrDefault();
                            // Assists in determining operation status
                            prodOper.BaseMaterialQty = prodOper.BaseMaterialQty.GetValueOrDefault() + Math.Abs(tranLine.BaseQty.GetValueOrDefault()) * tranLine.InvtMult.GetValueOrDefault();
                            break;
                        }
                        
                        if(tranLine.SubcontractSource == AMSubcontractSource.Purchase ||
                            tranLine.SubcontractSource == AMSubcontractSource.DropShip ||
                            tranLine.SubcontractSource == AMSubcontractSource.ShipToVendor)
                        {
                            prodOper.ActualSubcontract += tranLine.TranAmt.GetValueOrDefault();
                            // Assists in determining operation status
                            prodOper.BaseMaterialQty = prodOper.BaseMaterialQty.GetValueOrDefault() + Math.Abs(tranLine.BaseQty.GetValueOrDefault()) * tranLine.InvtMult.GetValueOrDefault();
                        }
                        break;
                    case AMTranType.Tool:           //Tool Totals
                        prodOper.ActualTool += tranLine.TranAmt.GetValueOrDefault();
                        break;
                    case AMTranType.FixOvhd:        //Fixed Overhead Totals
                        prodOper.ActualFixedOverhead += tranLine.TranAmt.GetValueOrDefault();
                        break;
                    case AMTranType.VarOvhd:        //Variable Overhead Totals
                        prodOper.ActualVariableOverhead += tranLine.TranAmt.GetValueOrDefault();
                        break;
                    case AMTranType.WIPadjustment:
                    case AMTranType.WIPvariance:
                        prodOper.WIPAdjustment += tranLine.TranAmt.GetValueOrDefault();
                        break;
                    case AMTranType.OperWIPComplete: //WIP Complete Total
                        if (!isStandardCostItem)
                        {
                            prodOper.WIPComp += tranLine.TranAmt.GetValueOrDefault();
                        }
                        break;
                    case AMTranType.Disassembly:
                        prodItemWipCompDirect += tranLine.TranAmt.GetValueOrDefault() * -1;
                        break;
                    case AMTranType.Receipt:
                        if (tranLine.DocType == AMDocType.Material && tranLine.IsByproduct.GetValueOrDefault())
                        {
                            prodOper.ActualMaterial += tranLine.TranAmt.GetValueOrDefault();
                            break;
                        }
                        if (tranLine.DocType == AMDocType.Disassembly)
                        {
                            if (tranLine.IsScrap.GetValueOrDefault())
                            {
                                prodOper.ScrapAmount += tranLine.TranAmt.GetValueOrDefault();
                                break;
                            }

                            prodOper.ActualMaterial += tranLine.TranAmt.GetValueOrDefault() * -1;
                            break;
                        }
                        if (isMoveTran && isStandardCostItem && tranLine.LastOper.GetValueOrDefault())
                        {
                            prodItemWipCompDirect += tranLine.TranAmt.GetValueOrDefault();
                        }
                        break;
                    case AMTranType.Adjustment:
                        if (tranLine.DocType == AMDocType.Disassembly)
                        {
                            if(tranLine.LineNbr == 0)
                            {
                                // Assumes this is the main disassembly item which also shows a tran line and when "Correcting" results in an Adjustment
                                prodItemWipCompDirect += tranLine.TranAmt.GetValueOrDefault();
                                break;
                            }
                            if (tranLine.IsScrap.GetValueOrDefault())
                            {
                                prodOper.ScrapAmount += tranLine.TranAmt.GetValueOrDefault() * -1;
                                break;
                            }

                            prodOper.ActualMaterial += tranLine.TranAmt.GetValueOrDefault();
                        }
                        else if (isMoveTran && isStandardCostItem && tranLine.LastOper.GetValueOrDefault())
                        {
                            prodItemWipCompDirect += tranLine.TranAmt.GetValueOrDefault();
                        }
                        break;
                    case AMTranType.ScrapWriteOff:
                    case AMTranType.ScrapQuarantine:
                        prodOper.ScrapAmount += tranLine.TranAmt.GetValueOrDefault();
                        break;
                }
            }

            if ((amproditem.Function == OrderTypeFunction.Disassemble || isStandardCostItem) && amproditem.WIPComp.GetValueOrDefault() != prodItemWipCompDirect)
            {
                // Not tracking WIP Complete by Operation for (Direct update to the order):
                //  + disassemble order types. 
                //  + standard cost items of regular orders.
                amproditem.WIPComp = prodItemWipCompDirect;
                graph.Caches<AMProdItem>().Update(amproditem);
            }

            foreach (var updatedOper in updatedOperations.Values)
            {
                var operHasCost = updatedOper.HasActualCost.GetValueOrDefault();
                if ((operHasCost || updatedOper.BaseMaterialQty.GetValueOrDefault() != 0) &&
                    updatedOper.StatusID == ProductionOrderStatus.Released)
                {
                    updatedOper.StatusID = ProductionOrderStatus.InProcess;

                    if(amproditem.StatusID == ProductionOrderStatus.Released)
                    {
                        amproditem.StatusID = ProductionOrderStatus.InProcess;
                        graph.Caches<AMProdItem>().Update(amproditem);
                    }
                }

                if (!operHasCost &&
                    updatedOper.BaseMaterialQty.GetValueOrDefault() == 0 &&
                    updatedOper.QtyComplete.GetValueOrDefault() == 0 &&
                    updatedOper.StatusID == ProductionOrderStatus.InProcess)
                {
                    updatedOper.StatusID = ProductionOrderStatus.Released;
                }

                if (updatedOper.PlanCostDate == null)
                {
                    updatedOper.PlanCostDate = updatedOper.CreatedDateTime ?? amproditem.ProdDate;
                }

                if (updatedOper.ActStartDate == null && (updatedOper.HasActualCost.GetValueOrDefault() ||
                                                         updatedOper.QtyComplete.GetValueOrDefault() != 0m ||
                                                         updatedOper.QtyScrapped.GetValueOrDefault() != 0m ||
                                                         updatedOper.ActualLaborTime.GetValueOrDefault() != 0 ||
                                                         updatedOper.ActualMachineTime.GetValueOrDefault() != 0))
                {
                    updatedOper.ActStartDate = graph.Accessinfo.BusinessDate;
                }

                graph.Caches<AMProdOper>().Update(updatedOper);
            }
        }

        /// <summary>
        /// Calculates costs for the list of production ids
        /// </summary>
        /// <param name="list">list of production item keys</param>
        /// <param name="graph">Calling graph</param>
        public static void UpdateProdItemCosts(List<AMProdItem> list, PXGraph graph)
        {
            AMProdItem currentAMProdItem = null;

            try
            {
                foreach (var prodItem in list)
                {
                    currentAMProdItem = prodItem;

                    var cahceProdItem = (AMProdItem)graph.Caches[typeof(AMProdItem)].Locate(prodItem);

                    AMProdItem prodrec = cahceProdItem == null ? PXSelect<AMProdItem,
                        Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                            And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>
                        >.Select(graph, currentAMProdItem.OrderType, currentAMProdItem.ProdOrdID)
                        : PXCache<AMProdItem>.CreateCopy(cahceProdItem);

                    UpdateProdOperActualCostTotals(graph, prodrec);
                }
            }
            catch (Exception)
            {
                if (currentAMProdItem != null)
                {
                    PXTrace.WriteWarning(Messages.GetLocal(Messages.ExceptionInUpdatingProductionCosts,
                        currentAMProdItem.OrderType.TrimIfNotNullEmpty(), currentAMProdItem.ProdOrdID.TrimIfNotNullEmpty()));
                }
                throw;
            }
        }

        protected static decimal GetPlanOperQty(PXGraph graph, AMProdItem amProdItem)
        {
            if (amProdItem.BaseQtytoProd.GetValueOrDefault() <= 0 && amProdItem.QtytoProd.GetValueOrDefault() > 0)
            {
                decimal? qtyInBase = 0;
                if (UomHelper.TryConvertToBaseQty<AMProdItem.inventoryID>(graph.Caches[typeof(AMProdItem)],
                    amProdItem, amProdItem.UOM, amProdItem.QtytoProd.GetValueOrDefault(), out qtyInBase))
                {
                    return qtyInBase.GetValueOrDefault();
                }
            }

            return amProdItem.BaseQtytoProd.GetValueOrDefault();
        }

        public static AMProdOper UpdatePlannedOperTotal(PXGraph graph, AMProdItem amProdItem, AMProdOper amProdOper)
        {
            return UpdatePlannedOperTotal(graph, amProdItem, amProdOper, GetPlanOperQty(graph, amProdItem));
        }

        protected static AMProdOper UpdatePlannedOperTotal(PXGraph graph, AMProdItem amProdItem, AMProdOper amProdOper, decimal planQty)
        {
            var laborHours = ProductionCostCalculator.GetPlannedLaborHoursTotal(amProdOper, planQty);
            var laborCosts = ProductionCostCalculator.GetPlannedLaborCost(graph, amProdOper, planQty, planQty, laborHours);
            var machineTime = ProductionCostCalculator.GetPlannedMachineTimeTotalRaw(amProdOper, planQty);
            var machineHours = machineTime.ToHours(9);
            var machineCosts = ProductionCostCalculator.GetPlannedMachineCost(graph, amProdOper, planQty, machineHours);
            var materialCosts = ProductionCostCalculator.GetPlannedMaterialCost(graph, amProdOper, planQty);
            var toolCosts = ProductionCostCalculator.GetPlannedToolCost(graph, amProdOper, planQty);
            var fixedOverheadCosts = ProductionCostCalculator.GetPlannedFixedOverheadCost(graph, amProdOper, planQty);
            var variableOverheadCosts = ProductionCostCalculator.GetPlannedVariableOverheadCost(graph, amProdOper, planQty,
                laborCosts?.UnitCost.GetValueOrDefault(), ((materialCosts?.RegularMaterial?.UnitCost).GetValueOrDefault() + (materialCosts?.Subcontract?.UnitCost).GetValueOrDefault()),
                laborHours.GetValueOrDefault(), machineHours.GetValueOrDefault());

            amProdOper.PlanQtyToProduce = planQty;
            amProdOper.PlanLaborTime = (laborHours*60m).ToCeilingInt();
            amProdOper.PlanLabor = laborCosts?.TotalCost.GetValueOrDefault();
            amProdOper.PlanMachineTime = machineTime.ToCeilingInt();
            amProdOper.PlanMachine = machineCosts.TotalCost.GetValueOrDefault();
            amProdOper.PlanMaterial = (materialCosts?.RegularMaterial?.TotalCost).GetValueOrDefault() * (amProdItem.Function == OrderTypeFunction.Disassemble ? -1 : 1);
            amProdOper.PlanTool = toolCosts.TotalCost.GetValueOrDefault();
            amProdOper.PlanFixedOverhead = fixedOverheadCosts.TotalCost.GetValueOrDefault();
            amProdOper.PlanVariableOverhead = variableOverheadCosts.TotalCost.GetValueOrDefault();
            amProdOper.PlanCostDate = graph.Accessinfo.BusinessDate;
            amProdOper.PlanSubcontract = (materialCosts?.Subcontract?.TotalCost).GetValueOrDefault();
            amProdOper.PlanReferenceMaterial = (materialCosts?.ReferenceMaterial?.TotalCost).GetValueOrDefault();

            return graph.Caches[typeof(AMProdOper)].Update(amProdOper) as AMProdOper;
        }

        public static void UpdatePlannedProductionTotals(PXGraph graph, AMProdItem amProdItem, AMProdTotal amProdTotal)
        {
            if(graph == null || amProdItem == null || amProdTotal == null
                || !amProdItem.ProdOrdID.EqualsWithTrim(amProdTotal.ProdOrdID))
            {
                return;
            }

            UpdatePlannedProductionTotals(graph, amProdItem, amProdTotal, PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
            >.Select(graph, amProdItem.OrderType, amProdItem.ProdOrdID).ToLocatedFirstTableList(graph));
        }

        public static void UpdatePlannedProductionTotals(PXGraph graph, AMProdItem amProdItem, AMProdTotal amProdTotal, List<AMProdOper> amProdOpers)
        {
            if (graph == null || amProdItem == null || amProdTotal == null
                || !amProdItem.ProdOrdID.EqualsWithTrim(amProdTotal.ProdOrdID))
            {
                return;
            }

            var planQty = GetPlanOperQty(graph, amProdItem);

            foreach (AMProdOper amProdOper in amProdOpers)
            {
                UpdatePlannedOperTotal(graph, amProdItem, FindAMProdOper(graph, amProdOper), planQty);
            }

            amProdTotal.PlanQtyToProduce = planQty;
            amProdTotal.PlanCostDate = graph.Accessinfo.BusinessDate;

            graph.Caches[typeof(AMProdTotal)].Update(amProdTotal);
        }

        /// <summary>
        /// Get the returning material unit cost in base units based on the material transaction history
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="materialAmmTran">Transaction record that needs the unit cost value</param>
        /// <returns>Calculated unit cost in base units</returns>
        public static decimal? GetMaterialReturnBaseUnitCost(PXGraph graph, AMMTran materialAmmTran)
        {
            if (graph == null || materialAmmTran == null || materialAmmTran.BaseQty.GetValueOrDefault() == 0 
                || materialAmmTran.DocType != AMDocType.Material)
            {
                return null;
            }

            decimal targetQty = Math.Abs(materialAmmTran.BaseQty.GetValueOrDefault());
            decimal runningCost = 0;
            decimal runningQty = 0;

            //look for issued material to get the correct unit cost to return
            //Must process from most recent transactions going back
            foreach (AMMTran ammTran in PXSelect<AMMTran,
                Where<AMMTran.docType, Equal<AMDocType.material>,
                    And<AMMTran.released, Equal<True>,
                    And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                    And<AMMTran.operationID, Equal<Required<AMMTran.operationID>>,
                    And<AMMTran.inventoryID, Equal<Required<AMMTran.inventoryID>>,
                    And<IsNull<AMMTran.subItemID, int0>, Equal<Required<AMMTran.subItemID>>>>>>>>>,
                        OrderBy<Desc<AMMTran.tranDate, Desc<AMMTran.batNbr, Desc<AMMTran.lineNbr>>>>
                >.Select(graph, materialAmmTran.OrderType, materialAmmTran.ProdOrdID, materialAmmTran.OperationID, materialAmmTran.InventoryID, materialAmmTran.SubItemID.GetValueOrDefault()))
            {
                if (ammTran.BatNbr == materialAmmTran.BatNbr)
                {
                    continue;
                }

                if (runningQty + ammTran.BaseQty.GetValueOrDefault() > targetQty)
                {
                    decimal partialQty = targetQty - runningQty;
                    runningQty += partialQty;

                    //need to get a base unit cost
                    decimal baseUnitCost = 0m;
                    if (ammTran.BaseQty.GetValueOrDefault() != 0)
                    {
                        baseUnitCost = UomHelper.Round(ammTran.TranAmt.GetValueOrDefault() / ammTran.BaseQty.GetValueOrDefault(), 6);
                    }

                    runningCost += UomHelper.PriceCostRound(baseUnitCost*partialQty);
                }
                else
                {
                    runningQty += ammTran.BaseQty.GetValueOrDefault();
                    runningCost += ammTran.TranAmt.GetValueOrDefault();
                }

                if (runningQty >= targetQty)
                {
                    break;
                }
            }

            if (runningQty == 0)
            {
                return 0;
            }

            return UomHelper.PriceCostRound(runningCost / runningQty);
        }

        /// <summary>
        /// Get the returning material unit cost based on the material transaction history
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="materialAmmTran">Transaction record that needs the unit cost value</param>
        /// <returns>Calculated unit cost based on the materialAmmTran.UOM</returns>
        public static decimal? GetMaterialReturnUnitCost(PXGraph graph, AMMTran materialAmmTran)
        {
            decimal? baseUnitCost = GetMaterialReturnBaseUnitCost(graph, materialAmmTran);

            if (baseUnitCost == null)
            {
                return null;
            }

            decimal? convertedUnitCost = 0;
            PXCache cache = graph.Caches[typeof(AMMTran)];

            //Acumatica conversions for cost appear backwards. Use the ToBase to get the value from the base to the AMMTran.UOM
            if (UomHelper.TryConvertToBaseCost<AMMTran.inventoryID>(cache, materialAmmTran, materialAmmTran.UOM, baseUnitCost.GetValueOrDefault(), out convertedUnitCost))
            {
                return convertedUnitCost;
            }

            return baseUnitCost;
        }

        /// <summary>
        /// Check a dac decimal field to see if the new value had the sign changed [ (+/-) to (-/+) ]
        /// </summary>
        /// <returns>True if the signs have changed, false for all other conditions</returns>
        public static bool SignsChanged(decimal? newValue, object oldValue)
        {
            if (newValue == null || oldValue == null)
            {
                return false;
            }

            decimal? nullableDecimal = Convert.ToDecimal(oldValue);

            return (newValue < 0 && nullableDecimal >= 0)
                    || (nullableDecimal < 0 && newValue >= 0);
        }

        public static bool CheckUnderIssuedMaterial(PXGraph graph, AMMTran ammTran, string setupMessageLevel, bool stopOnFirstCheckException, out AMTransactionFailedCheckException exception)
        {
            exception = null;
            if (ammTran?.DocType == null || ammTran.OrderType == null || ammTran.ProdOrdID == null || ammTran.OperationID == null)
            {
                return false;
            }
            
            return CheckUnderIssuedMaterial(graph, ammTran, MoveOperationQtyBuilder.ConstructSingleBuilder(graph, ammTran), setupMessageLevel, stopOnFirstCheckException, out exception);
        }

        public static bool CheckUnderIssuedMaterial(PXGraph graph, AMMTran ammTran,
            MoveOperationQtyTotals operationTotals, string setupMessageLevel, bool stopOnFirstCheckException,
            out AMTransactionFailedCheckException exception)
        {
            exception = null;
            if (ammTran?.DocType == null
                || ammTran.OrderType == null
                || ammTran.ProdOrdID == null
                || (ammTran.Qty.GetValueOrDefault() == 0 && ammTran.QtyScrapped.GetValueOrDefault() == 0)
                || string.IsNullOrWhiteSpace(setupMessageLevel)
                || setupMessageLevel == SetupMessage.AllowMsg
                || (setupMessageLevel == SetupMessage.WarningMsg 
                    && (graph.IsImport || graph.IsContractBasedAPI))
                || operationTotals == null)
            {
                return false;
            }

            if (ammTran.DocType != AMDocType.Move
                && ammTran.DocType != AMDocType.Labor
                && ammTran.DocType != AMDocType.Disassembly)
            {
                throw new PXException(Messages.GetLocal(Messages.IncorrectDocTypeForProcess, AMDocType.GetDocTypeDesc(ammTran.DocType)));
            }

            if (operationTotals.OperationTotals.Count == 0)
            {
                return false;
            }

            var maxMaterial = 8;
            var materialCntr = 0;
            var sb = new StringBuilder();
            var transactionTotal = operationTotals.GetTransactionOperationTotal();
            if (transactionTotal?.ProdOper == null)
            {
                return false;
            }

            foreach (AMProdMatlInventory prodMatl in PXSelect<
                AMProdMatlInventory,
                Where<AMProdMatlInventory.orderType, Equal<Required<AMProdMatlInventory.orderType>>,
                    And<AMProdMatlInventory.prodOrdID, Equal<Required<AMProdMatlInventory.prodOrdID>>,
                    And<AMProdMatlInventory.operationCD, LessEqual<Required<AMProdMatlInventory.operationCD>>,
                    And<AMProdMatlInventory.bFlush, Equal<False>>>>>>
                .Select(graph, transactionTotal.ProdOper.OrderType, transactionTotal.ProdOper.ProdOrdID, transactionTotal.ProdOper.OperationCD))
            {
                if (prodMatl?.ProdOrdID == null ||
                    prodMatl.QtyReq.GetValueOrDefault() <= 0)
                {
                    continue;
                }

                var oper = operationTotals.GetOperationTotal(prodMatl);
                if (oper == null)
                {
                    continue;
                }

                //Using the BaseQtyComplete from the MoveOperationQtyTotal will contain the total from previous and current transaction
                var totalExpected = prodMatl.GetTotalReqQtyCompanyRounded(oper.ProdOper?.BaseQtyComplete ?? 0m);
                if (UomHelper.QuantityRound(prodMatl.QtyActual.GetValueOrDefault()) >= totalExpected || totalExpected <= 0)
                {
                    continue;
                }

                sb.AppendLine(Messages.GetLocal(
                    Messages.UnderIssuedMaterial,
                    prodMatl.InventoryCD.TrimIfNotNullEmpty(),
                    prodMatl.OrderType.TrimIfNotNullEmpty(),
                    prodMatl.ProdOrdID.TrimIfNotNullEmpty(),
                    prodMatl.OperationCD,
                    prodMatl.LineID.GetValueOrDefault(),
                    prodMatl.UOM.TrimIfNotNullEmpty(),
                    UomHelper.FormatQty(prodMatl.QtyActual),
                    UomHelper.FormatQty(totalExpected)));

                materialCntr++;

                if (stopOnFirstCheckException || materialCntr >= maxMaterial)
                {
                    break;
                }
            }

            if (sb.Length == 0)
            {
                return false;
            }

            exception = new AMTransactionFailedCheckException(sb.ToString())
            {
                IsWarning = setupMessageLevel != SetupMessage.ErrorMsg
            };
            return true;
        }

        public static bool CheckMoveOnCompletedOperation(PXCache cache, AMMTran ammTran, string setupMessageLevel, out AMTransactionFailedCheckException exception)
        {
            exception = null;

            if (ammTran == null
                || ammTran.DocType == null
                || ammTran.OrderType == null
                || ammTran.ProdOrdID == null
                || (ammTran.Qty.GetValueOrDefault() == 0 && ammTran.QtyScrapped.GetValueOrDefault() == 0)
                || string.IsNullOrWhiteSpace(setupMessageLevel)
                || setupMessageLevel == SetupMessage.AllowMsg
                || (setupMessageLevel == SetupMessage.WarningMsg
                    && (cache.Graph.IsImport || cache.Graph.IsContractBasedAPI)))
            {
                return false;
            }

            var prodOper = (AMProdOper)PXSelectorAttribute.Select<AMMTran.operationID>(cache, ammTran);
            var prodItem = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(cache, ammTran);

            return CheckMoveOnCompletedOperation(cache, ammTran, prodOper, prodItem, setupMessageLevel, out exception);
        }

        public static bool CheckMoveOnCompletedOperation(PXCache cache, AMMTran ammTran, AMProdOper prodOper, AMProdItem prodItem, string setupMessageLevel, out AMTransactionFailedCheckException exception)
        {
            exception = null;

            if (ammTran == null
                || ammTran.DocType == null
                || ammTran.OrderType == null
                || ammTran.ProdOrdID == null
                || (ammTran.Qty.GetValueOrDefault() == 0 && ammTran.QtyScrapped.GetValueOrDefault() == 0)
                || string.IsNullOrWhiteSpace(setupMessageLevel)
                || setupMessageLevel == SetupMessage.AllowMsg
                || (setupMessageLevel == SetupMessage.WarningMsg
                    && (cache.Graph.IsImport || cache.Graph.IsContractBasedAPI))
                || prodOper == null
                || prodItem == null)
            {
                return false;
            }

            var isWarning = setupMessageLevel == SetupMessage.WarningMsg ||
                            ammTran.Qty.GetValueOrDefault() < 0 ||
                            ammTran.QtyScrapped.GetValueOrDefault() < 0;


            if (prodItem.StatusID == ProductionOrderStatus.Completed)
            {
                exception = new AMTransactionFailedCheckException(
                Messages.GetLocal(Messages.ProductionComplete,
                Messages.GetLocal(Messages.Order),
                prodOper.OrderType.TrimIfNotNullEmpty(),
                prodOper.ProdOrdID.TrimIfNotNullEmpty(),
                string.Empty))
                {
                    IsWarning = isWarning
                };
                return true;
            }

            if (prodOper.StatusID == ProductionOrderStatus.Completed)
            {
                exception = new AMTransactionFailedCheckException(
                Messages.GetLocal(Messages.ProductionComplete,
                Messages.GetLocal(Messages.Operation),
                prodOper.OrderType.TrimIfNotNullEmpty(),
                prodOper.ProdOrdID.TrimIfNotNullEmpty(),
                prodOper.OperationCD))
                {
                    IsWarning = isWarning
                };
                return true;
            }

            return false;
        }

        public static bool CheckOverCompletedOrders(PXCache cache, AMMTran ammTran, string setupMessageLevel, bool includeScrapInCompletion, out AMTransactionFailedCheckException exception)
        {
            exception = null;

            if (ammTran == null)
            {
                return false;
            }

            var prodItem = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(cache, ammTran);

            return CheckOverCompletedOrders(cache, ammTran, prodItem, setupMessageLevel, includeScrapInCompletion, out exception);
        }

        public static bool CheckOverCompletedOrders(PXCache cache, AMMTran ammTran, AMProdItem prodItem, string setupMessageLevel, bool includeScrapInCompletion, out AMTransactionFailedCheckException exception)
        {
            exception = null;

            if (ammTran == null
                || ammTran.DocType == null
                || ammTran.OrderType == null
                || ammTran.ProdOrdID == null
                || (ammTran.Qty.GetValueOrDefault() == 0 && ammTran.QtyScrapped.GetValueOrDefault() == 0)
                || string.IsNullOrWhiteSpace(setupMessageLevel)
                || setupMessageLevel == SetupMessage.AllowMsg
                || (setupMessageLevel == SetupMessage.WarningMsg
                    && (cache.Graph.IsImport || cache.Graph.IsContractBasedAPI))
                || prodItem == null)
            {
                return false;
            }

            var moveQty = ammTran.Qty.GetValueOrDefault() + (includeScrapInCompletion ? ammTran.QtyScrapped.GetValueOrDefault() : 0m);

            if (prodItem.QtyRemaining.GetValueOrDefault() >= moveQty)
            {
                return false;
            }

            exception = new AMTransactionFailedCheckException(
                Messages.GetLocal(Messages.TransactionQtyOverCompleteRemaining,
                ammTran.UOM.TrimIfNotNullEmpty(),
                UomHelper.FormatQty(moveQty),
                UomHelper.FormatQty(prodItem.QtyRemaining.GetValueOrDefault()),
                ammTran.OrderType.TrimIfNotNullEmpty(),
                ammTran.ProdOrdID.TrimIfNotNullEmpty()))
            {
                IsWarning = setupMessageLevel != SetupMessage.ErrorMsg
            };
            return true;
        }

        public static bool CheckOverCompletedOrders(PXGraph graph, AMMTran ammTran, AMProdItem prodItem,
            MoveOperationQtyTotals operationTotals, string setupMessageLevel, bool includeScrap, 
            out AMTransactionFailedCheckException exception)
        {
            exception = null;
            if (ammTran?.DocType == null || ammTran.OrderType == null || ammTran.ProdOrdID == null ||
                (ammTran.Qty.GetValueOrDefault() == 0 && ammTran.QtyScrapped.GetValueOrDefault() == 0) ||
                !ammTran.LastOper.GetValueOrDefault() || string.IsNullOrWhiteSpace(setupMessageLevel) ||
                setupMessageLevel == SetupMessage.AllowMsg || (setupMessageLevel == SetupMessage.WarningMsg
                && (graph.IsImport || graph.IsContractBasedAPI)) || operationTotals == null || operationTotals.OperationTotals.Count == 0)
            {
                return false;
            }

            if (ammTran.DocType != AMDocType.Move
                && ammTran.DocType != AMDocType.Labor
                && ammTran.DocType != AMDocType.Disassembly)
            {
                throw new PXException(Messages.GetLocal(Messages.IncorrectDocTypeForProcess, AMDocType.GetDocTypeDesc(ammTran.DocType)));
            }

            var sb = new StringBuilder();

            var operTotal = operationTotals.GetOperationTotal(new AMProdOper { OrderType = ammTran.OrderType, ProdOrdID = ammTran.ProdOrdID, OperationID = ammTran.OperationID });
            if (operTotal == null)
            {
                return false;
            }

            var moveQty = operTotal.TransactionTotalMoveBaseQty;
            var qtyRemaining = prodItem.BaseQtyRemaining.GetValueOrDefault();
            if (moveQty > qtyRemaining)
            {
                if (UomHelper.TryConvertFromBaseQty<AMMTran.inventoryID>(graph.Caches<AMMTran>(), ammTran,
                    ammTran.UOM,
                    moveQty,
                    out var moveQtyTranUom) && moveQtyTranUom != null)
                {
                    moveQty = moveQtyTranUom.GetValueOrDefault();
                    qtyRemaining = prodItem.QtyRemaining.GetValueOrDefault();
                }

                sb.AppendLine(Messages.GetLocal(Messages.TransactionQtyOverCompleteRemaining,
                    ammTran.UOM.TrimIfNotNullEmpty(),
                    UomHelper.FormatQty(moveQty),
                    UomHelper.FormatQty(qtyRemaining),
                    ammTran.OrderType.TrimIfNotNullEmpty(),
                    ammTran.ProdOrdID.TrimIfNotNullEmpty()));
            }

            if (sb.Length == 0)
            {
                return false;
            }

            exception = new AMTransactionFailedCheckException(sb.ToString())
            {
                IsWarning = setupMessageLevel != SetupMessage.ErrorMsg
            };
            return true;

        }

        protected static AMProdOper FindAMProdOper(PXGraph graph, AMProdOper row)
        {
            if (row == null
                || string.IsNullOrWhiteSpace(row.OrderType)
                || string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return row;
            }

            return graph.Caches[typeof(AMProdOper)].LocateElse(row);
        }

        /// <summary>
        /// Determine/update the correct total qty values for each operation.
        /// Material total updated as operations are updated.
        /// </summary>
        public static void UpdateOperationQty(PXGraph graph, AMProdItem amProdItem, bool includeScrap)
        {
            UpdateOperationQty(graph, amProdItem, PXSelect<AMProdOper,
                        Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                            And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>,
                        OrderBy<Asc<AMProdOper.orderType,
                            Asc<AMProdOper.prodOrdID,
                                Desc<AMProdOper.operationCD>>>>>.Select(graph, amProdItem.OrderType, amProdItem.ProdOrdID)
                    .ToLocatedCopiedFirstTableList(graph),
                includeScrap);
        }

        /// <summary>
        /// Determine/update the correct total qty values for each operation.
        /// Material total updated as operations are updated.
        /// </summary>
        public static void UpdateOperationQty(PXGraph graph, AMProdItem amProdItem, List<AMProdOper> amProdOpers, bool includeScrap)
        {
            graph.FieldUpdated.AddHandler<AMProdOper.baseTotalQty>((cache, args) =>
            {
                foreach (AMProdMatl prodMaterial in PXSelect<AMProdMatl,
                    Where<AMProdMatl.orderType, Equal<Current<AMProdOper.orderType>>,
                        And<AMProdMatl.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>,
                            And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>>.SelectMultiBound(graph, new object[] { (AMProdOper)args.Row }))
                {
                    // this will re-trigger the calc of totalqty on prodmatl...
                    graph.Caches<AMProdMatl>().RaiseFieldUpdated<AMProdMatl.batchSize>(prodMaterial, prodMaterial.BatchSize);
                    graph.Caches<AMProdMatl>().Update(prodMaterial);
                }
            });

            foreach (var updatedOper in GetUpdateOperationQty(amProdItem, amProdOpers, includeScrap))
            {
                graph.Caches[typeof(AMProdOper)].Update(updatedOper);
            }
        }

        /// <summary>
        /// Determine the correct total qty values for each operation.
        /// </summary>
        /// <param name="amProdItem">production order</param>
        /// <param name="amProdOpers">list of production operations</param>
        /// <param name="includeScrap">is scrap included in completions</param>
        /// <returns>Operations to be updated</returns>
        protected static List<AMProdOper> GetUpdateOperationQty(AMProdItem amProdItem, List<AMProdOper> amProdOpers, bool includeScrap)
        {
            return includeScrap
                ? GetUpdateOperationQtyIncludeScrap(amProdItem, amProdOpers)
                : GetUpdateOperationQtyExcludeScrap(amProdItem, amProdOpers);
        }

        private static List<AMProdOper> GetUpdateOperationQtyIncludeScrap(AMProdItem amProdItem, List<AMProdOper> amProdOpers)
        {
            var list = new List<AMProdOper>();
            var scrapTotal = 0m;
            var scrapBaseTotal = 0m;
            foreach (var prodOper in amProdOpers.OrderBy(x => x.OperationCD))
            {
                var calculatedTotalQty = amProdItem.QtytoProd.GetValueOrDefault() - scrapTotal;
                var calculatedBaseTotalQty = amProdItem.BaseQtytoProd.GetValueOrDefault() - scrapBaseTotal;

                if (calculatedTotalQty != prodOper.TotalQty ||
                    calculatedBaseTotalQty != prodOper.BaseTotalQty)
                {
                    prodOper.TotalQty = calculatedTotalQty;
                    prodOper.BaseTotalQty = calculatedBaseTotalQty;
                    list.Add(prodOper);
                }

                scrapTotal += Math.Abs(prodOper.QtyScrapped.GetValueOrDefault());
                scrapBaseTotal += Math.Abs(prodOper.BaseQtyScrapped.GetValueOrDefault());
            }
            return list;
        }

        private static List<AMProdOper> GetUpdateOperationQtyExcludeScrap(AMProdItem amProdItem, List<AMProdOper> amProdOpers)
        {
            var list = new List<AMProdOper>();
            var scrapTotal = 0m;
            var scrapBaseTotal = 0m;
            foreach (var prodOper in amProdOpers.OrderByDescending(x => x.OperationCD))
            {
                scrapTotal += Math.Abs(prodOper.QtyScrapped.GetValueOrDefault());
                scrapBaseTotal += Math.Abs(prodOper.BaseQtyScrapped.GetValueOrDefault());

                var calculatedTotalQty = amProdItem.QtytoProd.GetValueOrDefault() + scrapTotal;
                var calculatedBaseTotalQty = amProdItem.BaseQtytoProd.GetValueOrDefault() + scrapBaseTotal;

                if (calculatedTotalQty != prodOper.TotalQty ||
                    calculatedBaseTotalQty != prodOper.BaseTotalQty)
                {
                    prodOper.TotalQty = calculatedTotalQty;
                    prodOper.BaseTotalQty = calculatedBaseTotalQty;
                    list.Add(prodOper);
                }
            }
            return list;
        }

        public static bool CanSkipOrderTypeCheck<TField>(PXGraph graph, AMOrderType orderType)
            where TField : class, IBqlField
        {
            if (orderType == null || graph == null)
            {
                return false;
            }

            var checkValue = (string)graph.Caches<AMOrderType>()?.GetValue<TField>(orderType);

            if (string.IsNullOrWhiteSpace(checkValue))
            {
                return false;
            }

            return checkValue == SetupMessage.AllowMsg
                   || checkValue == SetupMessage.WarningMsg &&
                   (graph.IsImport || graph.IsContractBasedAPI);
        }

        /// <summary>
        /// Checks for Over Issue of Material for a given material entry.
        /// If over issue found related to check level. cache received raised exception handling.
        /// </summary>
        public static bool TryCheckOverIssueMaterialSetPropertyException(PXCache sender, AMMTran ammTran, decimal? newQty, out PXSetPropertyException exception)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            exception = null;
            if (ammTran?.DocType == null || ammTran.OrderType == null || ammTran.ProdOrdID == null || newQty.GetValueOrDefault() == 0 || ammTran.DocType != AMDocType.Material)
            {
                return false;
            }

            var amOrderType = (AMOrderType)PXSelect<AMOrderType, 
                Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>
                .Select(sender.Graph, ammTran.OrderType);

            if (CanSkipOrderTypeCheck<AMOrderType.overIssueMaterial>(sender.Graph, amOrderType))
            {
                return false;
            }

            AMProdMatl amProdMatl = PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>
                >>>>>.Select(sender.Graph, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.MatlLineId);

            if (amProdMatl == null)
            {
                return false;
            }

            var qtyRemaining = amProdMatl.QtyRemaining.GetValueOrDefault();
            if (ammTran.UOM != null && !amProdMatl.UOM.EqualsWithTrim(ammTran.UOM))
            {
                //Convert production UOM qty remaining...
                if (UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(sender.Graph.Caches<AMProdMatl>(), amProdMatl,
                    amProdMatl.UOM, ammTran.UOM, qtyRemaining, out var convertedQty))
                {
                    qtyRemaining = convertedQty.GetValueOrDefault();
                }
            }

            var unreleasedQty = amOrderType.IncludeUnreleasedOverIssueMaterial.GetValueOrDefault()
                ? GetUnreleasedMaterialQty(sender, amProdMatl, ammTran.UOM, ammTran)
                : 0m;
            qtyRemaining -= unreleasedQty;
            
            if (newQty.GetValueOrDefault() <= qtyRemaining)
            {
                return exception != null;
            }

            var currentInventoryId = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.inventoryID>(ammTran);
            var currentOperationCd = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.operationID>(ammTran);
            var msgParams = new List<object>
            {
                ammTran.UOM,
                UomHelper.FormatQty(newQty.GetValueOrDefault()),
                UomHelper.FormatQty(qtyRemaining.NotLessZero()),
                $"{ammTran.OrderType} {ammTran.ProdOrdID.TrimIfNotNullEmpty()}",
                currentOperationCd ?? ammTran.OperationID,
                currentInventoryId == null ? string.Empty : Convert.ToString(currentInventoryId).TrimIfNotNullEmpty(),
                amProdMatl.LineNbr
            };
            var containsUnreleasedQty = unreleasedQty != 0m;

            if (containsUnreleasedQty)
            {
                msgParams.Add(UomHelper.FormatQty(unreleasedQty));
            }

            var exceptionMsg =
                Messages.GetLocal(
                    containsUnreleasedQty
                        ? Messages.MaterialQuantityOverIssueWithUnreleasedQty
                        : Messages.MaterialQuantityOverIssue, 
                    msgParams.ToArray());

            exception = new PXSetPropertyException(
                exceptionMsg,
                amOrderType.OverIssueMaterial == SetupMessage.WarningMsg ? PXErrorLevel.Warning : PXErrorLevel.Error);

            return true;
        }

        protected static decimal GetUnreleasedMaterialQty(PXCache cache, AMProdMatl prodMatl, string tranUom, AMMTran excludingTran)
        {
            var baseQty = GetUnreleasedMaterialBaseQty(cache.Graph, prodMatl, excludingTran);

            if (baseQty != 0 && UomHelper.TryConvertFromBaseQty<AMProdMatl.inventoryID>(cache, prodMatl,
                tranUom,
                baseQty,
                out var tranQty))
            {
                return tranQty.GetValueOrDefault();
            }

            return baseQty;
        }

        public static decimal GetUnreleasedMaterialBaseQty(PXGraph graph, AMProdMatl prodMatl)
        {
            return GetUnreleasedMaterialBaseQty(graph, prodMatl, null);
        }

        /// <summary>
        /// Uses SQL Index: AMMTran_IX_GetUnreleasedMaterialBaseQty
        /// </summary>
        protected static decimal GetUnreleasedMaterialBaseQty(PXGraph graph, AMProdMatl prodMatl, AMMTran excludingTran)
        {
            //For some reason (in some random cases) the Projection doesn't work unless the graph contains a cache for AMMTran.
            Common.Cache.AddCache<AMMTran>(graph);

            PXSelectBase<UnreleasedMaterialTran> bqlCmd = new PXSelectGroupBy<UnreleasedMaterialTran,
                Where<UnreleasedMaterialTran.orderType, Equal<Required<UnreleasedMaterialTran.orderType>>,
                    And<UnreleasedMaterialTran.prodOrdID, Equal<Required<UnreleasedMaterialTran.prodOrdID>>,
                        And<UnreleasedMaterialTran.operationID, Equal<Required<UnreleasedMaterialTran.operationID>>,
                            And<UnreleasedMaterialTran.matlLineId, Equal<Required<UnreleasedMaterialTran.matlLineId>>>>>>,
                Aggregate<Sum<UnreleasedMaterialTran.baseQty, Sum<UnreleasedMaterialTran.qty, Sum<UnreleasedMaterialTran.tranAmt>>>>>(graph);

            var bqlParams = new List<object>
            {
                prodMatl?.OrderType,
                prodMatl?.ProdOrdID,
                prodMatl?.OperationID,
                prodMatl?.LineID
            };

            if (excludingTran?.LineNbr != null && excludingTran.DocType == AMDocType.Material)
            {
                bqlCmd.WhereAnd<Where<UnreleasedMaterialTran.batNbr, NotEqual<Required<UnreleasedMaterialTran.batNbr>>>>();
                bqlParams.Add(excludingTran.BatNbr);
            }

            return ((UnreleasedMaterialTran)bqlCmd.Select(bqlParams.ToArray()))?.BaseQty ?? 0m;
        }

        [PXHidden]
        [Serializable]
        [PXProjection(typeof(Select<
            AMMTran,
            Where<AMMTran.docType, Equal<AMDocType.material>,
                And<AMMTran.released, Equal<False>>>>))]
        public class UnreleasedMaterialTran : IBqlTable
        {
            #region DocType

            public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

            protected String _DocType;
            [PXDBString(1, IsFixed = true, BqlField = typeof(AMMTran.docType))]
            public virtual String DocType
            {
                get
                {
                    return this._DocType;
                }
                set
                {
                    this._DocType = value;
                }
            }
            #endregion
            #region BatNbr
            public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

            protected String _BatNbr;
            [PXDBString(15, IsUnicode = true, BqlField = typeof(AMMTran.batNbr))]
            public virtual String BatNbr
            {
                get
                {
                    return this._BatNbr;
                }
                set
                {
                    this._BatNbr = value;
                }
            }
            #endregion
            #region Qty
            public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

            protected Decimal? _Qty;
            [PXDBQuantity(BqlField = typeof(AMMTran.qty))]
            [PXUIField(DisplayName = "Quantity")]
            public virtual Decimal? Qty
            {
                get
                {
                    return this._Qty;
                }
                set
                {
                    this._Qty = value;
                }
            }
            #endregion
            #region BaseQty
            public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

            protected Decimal? _BaseQty;
            [PXDBQuantity(BqlField = typeof(AMMTran.baseQty))]
            [PXUIField(DisplayName = "Base Qty")]
            public virtual Decimal? BaseQty
            {
                get
                {
                    return this._BaseQty;
                }
                set
                {
                    this._BaseQty = value;
                }
            }
            #endregion
            #region TranAmt
            public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

            protected Decimal? _TranAmt;
            [PXDBBaseCury(BqlField = typeof(AMMTran.tranAmt))]
            [PXUIField(DisplayName = "Ext. Cost", Enabled = false)]
            public virtual Decimal? TranAmt

            {
                get
                {
                    return this._TranAmt;
                }
                set
                {
                    this._TranAmt = value;
                }
            }
            #endregion
            #region OrderType
            public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

            protected String _OrderType;
            [AMOrderTypeField(IsKey = true, BqlField = typeof(AMMTran.orderType))]
            [AMOrderTypeSelector]
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
            [ProductionNbr(IsKey = true, BqlField = typeof(AMMTran.prodOrdID))]
            [ProductionOrderSelector(typeof(UnreleasedMaterialTran.orderType))]
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
            #region OperationID
            public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

            protected int? _OperationID;
            [OperationIDField(IsKey = true, BqlField = typeof(AMMTran.operationID))]
            public virtual int? OperationID
            {
                get
                {
                    return this._OperationID;
                }
                set
                {
                    this._OperationID = value;
                }
            }
            #endregion
            #region MatlLineId
            public abstract class matlLineId : PX.Data.BQL.BqlInt.Field<matlLineId> { }

            protected Int32? _MatlLineId;
            [PXDBInt(IsKey = true, BqlField = typeof(AMMTran.matlLineId))]
            [PXUIField(DisplayName = "Material Line ID")]
            public virtual Int32? MatlLineId
            {
                get
                {
                    return this._MatlLineId;
                }
                set
                {
                    this._MatlLineId = value;
                }
            }
            #endregion
        }

        public static bool IsInventoryOnProductionOrder(PXGraph graph, int? inventoryId, string orderType, string prodOrdId)
        {
            if (graph == null || inventoryId.GetValueOrDefault() == 0 || string.IsNullOrWhiteSpace(orderType) ||
                string.IsNullOrWhiteSpace(prodOrdId))
            {
                return false;
            }

            return (AMProdMatl)PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                        And<AMProdMatl.inventoryID, Equal<Required<AMProdMatl.inventoryID>>>>>>
                .SelectWindowed(graph, 0, 1, orderType, prodOrdId, inventoryId) != null;
        }

        public static bool TryCheckNegativeMoveQty(PXCache cache, AMMTran ammTran, decimal moveQty, List<AMMTran> allRows, out Exception exception)
        {
            exception = null;
            if (ammTran == null || moveQty >= 0 || ammTran.IsScrap.GetValueOrDefault())
            {
                return false;
            }
      
            var oper = (AMProdOper) PXSelectorAttribute.Select<AMMTran.operationID>(cache, ammTran)
                // For some reason during the release process it cannot find the correct row using the above line.
                ?? (AMProdOper) PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                        And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>
                >>.Select(cache.Graph, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID);

            if (oper == null)
            {
                exception = new PXException(
                    Messages.GetLocal(Messages.UnableToVerifyQtyforNegMove),
                    ammTran.OrderType, 
                    ammTran.ProdOrdID);
                return true;
            }

            var totalNegQty = GetTotalTranNegativeMoveQty(cache, ammTran, moveQty, allRows);
            var absTotalNegQty = Math.Abs(totalNegQty);
            if (absTotalNegQty > oper.QtyComplete.GetValueOrDefault())
            {
                exception = new PXSetPropertyException(
                    Messages.GetLocal(Messages.NegQtyGreaterThanOperQtyComplete),
                    UomHelper.FormatQty(totalNegQty), 
                    UomHelper.FormatQty(oper.QtyComplete.GetValueOrDefault()),
                    ammTran.UOM.TrimIfNotNullEmpty(), 
                    ammTran.OrderType,
                    ammTran.ProdOrdID,
                    oper.OperationCD);
                return true;
            }

            if (ammTran.LastOper.GetValueOrDefault())
            {
                return false;
            }

            var item = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(cache, ammTran);

            if (item != null && absTotalNegQty >
                oper.QtyComplete.GetValueOrDefault() - item.QtyComplete.GetValueOrDefault())
            {
                exception = new PXSetPropertyException(
                    Messages.GetLocal(Messages.NegQtyGreaterThanTotQtyComplete),
                    UomHelper.FormatQty(totalNegQty),
                    UomHelper.FormatQty(oper.QtyComplete.GetValueOrDefault()),
                    UomHelper.FormatQty(item.QtyComplete.GetValueOrDefault()),
                    item.UOM.TrimIfNotNullEmpty(),
                    ammTran.OrderType,
                    ammTran.ProdOrdID,
                    oper.OperationCD);
                return true;
            }

            return false;
        }

        protected static decimal GetTotalTranNegativeMoveQty(PXCache cache, AMMTran ammTran, decimal moveQty, List<AMMTran> allRows)
        {
            var qty = moveQty;
            if (allRows == null)
            {
                return qty;
            }
            foreach (var row in allRows)
            {
                if (!cache.ObjectsEqual<AMMTran.orderType>(ammTran, row) ||
                    !cache.ObjectsEqual<AMMTran.prodOrdID>(ammTran, row) ||
                    !cache.ObjectsEqual<AMMTran.operationID>(ammTran, row) ||
                    row.LineNbr == ammTran.LineNbr ||
                    row.Qty.GetValueOrDefault() >= 0)
                {
                    continue;
                }

                qty += row.Qty.GetValueOrDefault();
            }
            return qty;
        }

        public static Dictionary<string, decimal> GetUnreleasedWipByOperation(PXGraph graph, AMBatch amBatch)
        {
            if (amBatch == null)
            {
                throw new PXArgumentException(nameof(amBatch));
            }

            return GetUnreleasedWipByOperation(graph, amBatch.DocType, amBatch.BatNbr);
        }

        internal static Dictionary<string, decimal> GetUnreleasedWipByOperation(PXGraph graph, string docType, string batNbr)
        {
            if (string.IsNullOrWhiteSpace(docType))
            {
                throw new PXArgumentException(nameof(docType));
            }

            var dic = new Dictionary<string, decimal>();
            foreach (AMMTranUnreleasedWipByOperation result in PXSelect<
                    AMMTranUnreleasedWipByOperation,
                    Where<AMMTranUnreleasedWipByOperation.origDocType, Equal<Required<AMMTranUnreleasedWipByOperation.origDocType>>,
                                    And<AMMTranUnreleasedWipByOperation.origBatNbr, Equal<Required<AMMTranUnreleasedWipByOperation.origBatNbr>>>>>
                .Select(graph, docType, batNbr))
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"[AMMTranUnreleasedWipByOperation] {result.DebuggerDisplay}");
#endif
                dic[MakeOrderDicKey(result)] = result.TranAmt.GetValueOrDefault();
            }

            return dic;
        }

        internal static string MakeOrderDicKey(AMMTranUnreleasedWipByOperation ammTran)
        {
            return string.Join("~", ammTran.OrderType.TrimIfNotNullEmpty(), ammTran.ProdOrdID.TrimIfNotNullEmpty(), ammTran.OperationID);
        }

        public static bool ProductionOrderHasUnreleasedTransactions(PXGraph graph, AMProdItem prodItem, out string msg)
        {
            msg = null;
            AMMTran ammTran = PXSelect<AMMTran,
                Where<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                        And<AMMTran.released, Equal<False>>>>>.SelectWindowed(graph, 0, 1, prodItem.OrderType, prodItem.ProdOrdID);
            if (ammTran != null)
            {
                msg = Messages.GetLocal(Messages.ProductionOrderUnreleasedTransactions, prodItem.OrderType, prodItem.ProdOrdID);
                PXTrace.WriteInformation($"{msg}. {Messages.GetLocal(Messages.UnRelPo_UnReleased)} {AMDocType.GetDocTypeDesc(ammTran.DocType)} {ammTran.BatNbr}");
            }

            return !string.IsNullOrWhiteSpace(msg);
        }

        /// <summary>
        /// Returns PeriodID from the given date.
        /// </summary>
        public static string PeriodFromDate(PXGraph graph, DateTime? date)
        {
            var finPeriod = new PX.Objects.GL.FinPeriods.FinPeriodRepository(graph).GetFinPeriodByDate(date,
                PXAccess.GetParentOrganizationID(graph.Accessinfo.BranchID));
            return finPeriod?.FinPeriodID;
        }
    }
}