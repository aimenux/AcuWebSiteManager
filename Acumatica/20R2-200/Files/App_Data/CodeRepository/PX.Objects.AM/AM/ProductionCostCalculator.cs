using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Calculate the costs during production completions of the production item
    /// </summary>
    public class ProductionCostCalculator
    {
        protected readonly PXGraph Graph;
        protected readonly AMPSetup ProductionSetup;
        protected readonly bool IncludeScrap;

        /// <summary>
        /// Indicates of the cost should be returned in base units (Default is in transaction units)
        /// </summary>
        public bool CostInBaseUnits { get; set; }

        public ProductionCostCalculator(PXGraph graph) : this(graph, PXSelect<AMPSetup>.Select(graph))
        {
        }

        public ProductionCostCalculator(PXGraph graph, AMPSetup productionSetup)
        {
            Graph = graph;

            ProductionSetup = productionSetup;

            IncludeScrap = ProductionSetup != null && ProductionSetup.InclScrap.GetValueOrDefault();
            CostInBaseUnits = false;
        }

        public ProductionOperationCostResults CalculateCompletedUnitCostByOperation(AMMTran ammTran, AMProdItem amProdItem, MoveOperationQtyTotals moveOperationQtyTotals)
        {
            if (ammTran == null || string.IsNullOrWhiteSpace(ammTran.ProdOrdID) || ammTran.InventoryID == null
                || amProdItem == null || amProdItem.InventoryID != ammTran.InventoryID || !amProdItem.ProdOrdID.EqualsWithTrim(ammTran.ProdOrdID))
            {
                return null;
            }

            if (amProdItem?.InventoryID == null || !amProdItem.ProdOrdID.EqualsWithTrim(ammTran.ProdOrdID))
            {
                throw new PXArgumentException(nameof(amProdItem));
            }

            InventoryItem inventoryItem = PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>
            >.Select(Graph, ammTran.InventoryID);

            if (inventoryItem?.InventoryID == null || inventoryItem.InventoryID != amProdItem.InventoryID)
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                    Common.Cache.GetCacheName(typeof(InventoryItem))));
            }

            if (inventoryItem.ValMethod == INValMethod.Standard)
            {
                var prodOperCostResults = GetEstimatedUnitCostByOperation(ammTran, inventoryItem, amProdItem, moveOperationQtyTotals);
                prodOperCostResults.UnitAmount = GetStandardUnitCost(ammTran, inventoryItem).GetValueOrDefault();
                return prodOperCostResults;
            }

            if (amProdItem.CostMethod == CostMethod.Actual)
            {
                return GetActualUnitCostByOperations(ammTran, amProdItem, inventoryItem, moveOperationQtyTotals);
            }

            return GetEstimatedUnitCostByOperation(ammTran, inventoryItem, amProdItem, moveOperationQtyTotals);
        }

        /// <summary>
        /// Get the standard cost of the production item
        /// </summary>
        public decimal? GetStandardUnitCost(AMMTran ammTran)
        {
            if (ammTran?.InventoryID == null)
            {
                return 0;
            }

            InventoryItem inventoryItem = PXSelect
            <InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>
            >.Select(Graph, ammTran.InventoryID);

            return GetStandardUnitCost(ammTran, inventoryItem);
        }

        /// <summary>
        /// Get the standard cost of the production item
        /// </summary>
        protected decimal? GetStandardUnitCost(AMMTran ammTran, InventoryItem inventoryItem)
        {
            if (inventoryItem == null || ammTran == null
                || inventoryItem.InventoryID == null || ammTran.InventoryID == null
                || ammTran.InventoryID != inventoryItem.InventoryID)
            {
                return 0;
            }

            if (inventoryItem.ValMethod != INValMethod.Standard)
            {
                throw new PXException(Messages.ItemIsNotStandardCost);
            }

            INItemSite inItemSite = PXSelect
            <INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>
                    , And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>
            >.Select(Graph, ammTran.InventoryID, ammTran.SiteID);

            var baseUnitCost = inItemSite != null && inItemSite.StdCost.GetValueOrDefault() != 0
                ? inItemSite.StdCost.GetValueOrDefault()
                : inventoryItem.StdCost.GetValueOrDefault();

            return UomHelper.PriceCostRound(ConvertUnitCost(ammTran, inventoryItem.BaseUnit, baseUnitCost) ?? 0);
        }

        protected virtual decimal? ConvertUnitCost(AMMTran ammTran, string baseUom, decimal? baseUnitCost )
        {
            return ConvertUnitCost<AMMTran, AMMTran.inventoryID>(ammTran, ammTran.UOM, baseUom, baseUnitCost);
        }

        protected virtual decimal? ConvertUnitCost<TDac, TField>(object row, string tranUom, string baseUom, decimal? baseUnitCost)
            where TDac : IBqlTable
            where TField : IBqlField
        {
            return ConvertUnitCost<TField>(Graph.Caches[typeof(TDac)], row, tranUom, baseUom, baseUnitCost, CostInBaseUnits);
        }

        protected static decimal? ConvertUnitCost<TField>(PXCache cache, object row, string tranUom, string baseUom, decimal? baseUnitCost)
            where TField : IBqlField
        {
            return ConvertUnitCost<TField>(cache, row, tranUom, baseUom, baseUnitCost, tranUom.EqualsWithTrim(baseUom));
        }

        protected static decimal? ConvertUnitCost<TField>(PXCache cache, object row,string tranUom, string baseUom, decimal? baseUnitCost, bool costInBaseUnits)
            where TField : IBqlField
        {
            if (row == null || string.IsNullOrWhiteSpace(baseUom)
                || string.IsNullOrWhiteSpace(tranUom))
            {
                return baseUnitCost;
            }

            if (!costInBaseUnits && !tranUom.EqualsWithTrim(baseUom))
            {
                if (UomHelper.TryConvertFromToCost<TField>(cache, row, baseUom, tranUom, baseUnitCost.GetValueOrDefault(), out var tranUnitCost))
                {
                    return tranUnitCost.GetValueOrDefault();
                }
            }

            return baseUnitCost;
        }

        private ProductionOperationCostResults GetActualUnitCostByOperations(AMMTran ammTran, AMProdItem amProdItem, InventoryItem inventoryItem, MoveOperationQtyTotals moveOperationQtyTotals)
        {
            var operResults = new ProductionOperationCostResults(amProdItem);

            // For actual cost its all based on the current moving qty for the last operation (for all operations) + total of all transaction lines
            var transactionBaseQty = ammTran.LastOper.GetValueOrDefault()
                ? moveOperationQtyTotals.GetLastOperation(amProdItem)?.TransactionTotalMoveBaseQty ?? ammTran.BaseQty.GetValueOrDefault() + ammTran.BaseQtyScrapped.GetValueOrDefault()
                : 0m;

            foreach (var operationTotal in moveOperationQtyTotals.OperationTotalsList)
            {
                var costResults = GetActualCostByOperation(ammTran,
                    inventoryItem,
                    operationTotal.ProdOper,
                    transactionBaseQty,
                    moveOperationQtyTotals.GetAdditionalOperationWipCost(operationTotal.ProdOper));

                operResults.AddOperationCost(operationTotal.ProdOper,
                    costResults?.UnitCost ?? 0m,
                    costResults?.TotalCost ?? 0m);
            }
            return operResults;
        }

        /// <summary>
        /// Calculate the actual unit cost for the production item and transaction.
        /// </summary>
        protected virtual PlannedCosts GetActualCostByOperation(AMMTran ammTran, InventoryItem inventoryItem, AMProdOper amProdOper, decimal transactionBaseQty, decimal additionalWip)
        {
            if (ammTran == null || string.IsNullOrWhiteSpace(ammTran.ProdOrdID) || ammTran.InventoryID == null)
            {
                return new PlannedCosts(0, 0);
            }

            if (string.IsNullOrWhiteSpace(amProdOper?.ProdOrdID))
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                    Common.Cache.GetCacheName(typeof(AMProdOper))));
            }

            if (inventoryItem?.InventoryID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                    Common.Cache.GetCacheName(typeof(InventoryItem))));
            }

            var wipBalance = amProdOper.WIPBalance.GetValueOrDefault() + additionalWip;

            var unitCost = transactionBaseQty == 0 ? 0 : (wipBalance / transactionBaseQty);
            unitCost = ConvertUnitCost(ammTran, inventoryItem.BaseUnit, unitCost).GetValueOrDefault();
            var totalCost = ConvertUnitCost(ammTran, inventoryItem.BaseUnit, wipBalance).GetValueOrDefault();
#if DEBUG
            AMDebug.TraceWriteMethodName($"Operation Actual Cost (WIP Bal) {amProdOper.WIPBalance.GetValueOrDefault(-1)} (Additional Wip) {additionalWip}; Tran Base Qty {transactionBaseQty}; Unit Cost {unitCost}; ProdOrdID {ammTran.OrderType}-{ammTran.ProdOrdID}[{amProdOper.OperationCD}]; BatNbr {ammTran.BatNbr.TrimIfNotNullEmpty()}; Line Nbr {ammTran.LineNbr}; Tran/Base UOM {ammTran.UOM.TrimIfNotNullEmpty()}/{inventoryItem.BaseUnit.TrimIfNotNullEmpty()}");
#endif
            // Unit cost ok to be negative. Ex: byproduct operation (negative material)
            return new PlannedCosts(unitCost, totalCost);
        }

        /// <summary>
        /// Calculate the estimated actual unit cost for the production item 
        /// based on actual and remaining units.
        /// </summary>
        private ProductionOperationCostResults GetEstimatedUnitCostByOperation(AMMTran ammTran, InventoryItem inventoryItem, AMProdItem amProdItem, MoveOperationQtyTotals moveOperationQtyTotals)
        {
            if (string.IsNullOrWhiteSpace(ammTran?.BatNbr))
            {
                throw new PXArgumentException(nameof(ammTran));
            }
            if (amProdItem?.InventoryID == null || !amProdItem.ProdOrdID.EqualsWithTrim(ammTran.ProdOrdID))
            {
                throw new PXArgumentException(nameof(amProdItem));
            }
            if (inventoryItem?.InventoryID == null || inventoryItem.InventoryID != amProdItem.InventoryID)
            {
                throw new PXArgumentException(nameof(inventoryItem));
            }

            if (moveOperationQtyTotals == null)
            {
                throw new PXArgumentException(nameof(moveOperationQtyTotals));
            }

            var lastOperTotal = ammTran.LastOper.GetValueOrDefault()
                ? moveOperationQtyTotals.GetOperationTotal(ammTran)
                : null;
            var orderQtyCompl = amProdItem.BaseQtyComplete.GetValueOrDefault();
            if (IncludeScrap)
            {
                orderQtyCompl += amProdItem.BaseQtyScrapped.GetValueOrDefault();
            }

            var orderQtyTot = amProdItem.BaseQtytoProd.GetValueOrDefault();
            var overCompleteQty = (orderQtyCompl + (lastOperTotal?.TransactionTotalMoveBaseQty ?? 0m) - orderQtyTot).NotLessZero();
            var orderQtyRem = (orderQtyTot + overCompleteQty - orderQtyCompl).NotLessZero();

            var unitCostQty = orderQtyRem.NotLessZero();
#if DEBUG
            AMDebug.TraceWriteMethodName($"Order total Qty {orderQtyTot}; Order Qty Complete {orderQtyCompl}; Over Complete Qty {overCompleteQty}; Order Qty Remaining {orderQtyRem}; UnitCostQty {unitCostQty}; Last Oper Tran Total Qty {lastOperTotal?.TransactionTotalMoveBaseQty}");
#endif
            var operResults = new ProductionOperationCostResults(amProdItem);
            var totExpectedCost = 0m;
            foreach (var operationTotal in moveOperationQtyTotals.OperationTotalsList)
            {
                operResults.AddOperationCost(GetEstimatedUnitCostByOperation(Graph, inventoryItem, amProdItem, operationTotal.ProdOper, unitCostQty,
                    ammTran.UOM,
                    moveOperationQtyTotals.GetAdditionalOperationWipCost(operationTotal.ProdOper), ammTran.BaseQtyScrapped.GetValueOrDefault(),
                    out var operExpectedCost), true);
                totExpectedCost += operExpectedCost;
            }

            decimal? totalWipComplete = amProdItem.WIPComp.GetValueOrDefault();
            var unitCost = totExpectedCost - totalWipComplete.GetValueOrDefault();
            if (unitCostQty != 0)
            {
                unitCost /= unitCostQty;
            }
            unitCost = ConvertUnitCost(ammTran, inventoryItem.BaseUnit, unitCost).GetValueOrDefault();
            operResults.UnitAmount = unitCost;
#if DEBUG
            AMDebug.TraceWriteMethodName(
                $"Total Expected {totExpectedCost}; Total WIP Complete {totalWipComplete.GetValueOrDefault(-1)}; Unit Cost Qty {unitCostQty}; Unit Cost {unitCost}");
#endif
            return operResults;
        }

        public static ProductionOperationCostResults.OperationCostResult GetEstimatedUnitCostByOperation(PXGraph graph,
            InventoryItem inventoryItem, AMProdItem amProdItem, AMProdOper amProdOper, decimal unitCostQty, string tranUom, 
            decimal additionalWip, decimal scrapQty, out decimal operExpectedCost)
        {
            operExpectedCost = 0m;
            if (!amProdOper.ProdOrdID.EqualsWithTrim(amProdItem.ProdOrdID))
            {
                throw new PXArgumentException(nameof(amProdOper.ProdOrdID));
            }

            var laborHours = GetPlannedLaborHoursTotal(amProdOper, amProdOper.BaseTotalQty.GetValueOrDefault());
            var laborCosts = GetPlannedLaborCost(graph, amProdOper, amProdOper.BaseTotalQty.GetValueOrDefault(), amProdOper.BaseQtyRemaining.GetValueOrDefault(), laborHours);
            var machineHours = GetPlannedMachineTimeTotalRaw(amProdOper, amProdOper.BaseTotalQty).ToHours(9);
            var machineCosts = GetPlannedMachineCost(graph, amProdOper, amProdOper.BaseTotalQty, machineHours);
            var materialCosts = GetPlannedMaterialCost(graph, amProdOper, amProdOper.BaseTotalQty.GetValueOrDefault());
            var toolCosts = GetPlannedToolCost(graph, amProdOper, amProdOper.BaseTotalQty.GetValueOrDefault());
            var fixedOverheadCosts = GetPlannedFixedOverheadCost(graph, amProdOper, amProdOper.BaseTotalQty.GetValueOrDefault());
            var variableOverheadCosts = GetPlannedVariableOverheadCost(graph, amProdOper, amProdOper.BaseQtyRemaining,
                amProdItem.BaseQtytoProd.GetValueOrDefault(), amProdOper.BaseTotalQty.GetValueOrDefault() + scrapQty,
                laborCosts.UnitCost.GetValueOrDefault(), ((materialCosts?.RegularMaterial?.UnitCost).GetValueOrDefault() + (materialCosts?.Subcontract?.UnitCost).GetValueOrDefault()),
                laborHours.GetValueOrDefault(), machineHours.GetValueOrDefault());

            operExpectedCost =
                laborCosts.TotalCost.GetValueOrDefault() +
                machineCosts.TotalCost.GetValueOrDefault() +
                (materialCosts?.RegularMaterial?.TotalCost).GetValueOrDefault() +
                (materialCosts?.Subcontract?.TotalCost).GetValueOrDefault() +
                fixedOverheadCosts.TotalCost.GetValueOrDefault() +
                variableOverheadCosts.TotalCost.GetValueOrDefault() +
                toolCosts.TotalCost.GetValueOrDefault();

            var remainingOperCost = operExpectedCost - amProdOper.WIPComp.GetValueOrDefault() + additionalWip;
            var operUnitcost = 0m;
            if (unitCostQty != 0)
            {
                operUnitcost = ConvertUnitCost<AMProdItem.inventoryID>(graph.Caches[typeof(AMProdItem)], amProdItem, tranUom, inventoryItem.BaseUnit, remainingOperCost / unitCostQty).GetValueOrDefault();
            }
            
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}]({amProdOper.OperationID}) Oper Qty Complete:  {amProdOper.BaseQtyComplete.GetValueOrDefault()} ; Oper BaseTotalQty: {amProdOper.BaseTotalQty.GetValueOrDefault()}; UnitCostQty: {unitCostQty}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Oper WIP Complete:  {amProdOper.WIPComp.GetValueOrDefault()} ; Oper Unit Cost: {operUnitcost} ; Oper Remaining Cost: {remainingOperCost}");
            if (additionalWip != 0)
            {
                AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Oper Additional WIP:  {additionalWip}");
            }
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Expected Labor Cost: {laborCosts.TotalCost.GetValueOrDefault(-1)}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Expected Machine Cost: {machineCosts.TotalCost.GetValueOrDefault(-1)}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Expected Material Cost: {(materialCosts?.RegularMaterial?.TotalCost).GetValueOrDefault(-1)}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Expected Subcontract Cost: {(materialCosts?.Subcontract?.TotalCost).GetValueOrDefault(-1)}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Expected Fix Ovhd Cost: {fixedOverheadCosts.TotalCost.GetValueOrDefault(-1)}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Expected Var Ovhd Cost: {variableOverheadCosts.TotalCost.GetValueOrDefault(-1)}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Expected Tool Cost: {toolCosts.TotalCost.GetValueOrDefault(-1)}");
            AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] TOTAL Expected Oper Cost: {operExpectedCost}");
#endif
            return new ProductionOperationCostResults.OperationCostResult(amProdOper, operUnitcost, remainingOperCost);
        }

        /// <summary>
        /// Get the planned Fixed Overhead unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedFixedOverheadCost(PXGraph graph, AMProdOper amProdOper, decimal? productionQty)
        {
            decimal? totalCost = 0;

            foreach (PXResult<AMProdOvhd, AMOverhead> result in PXSelectJoin<AMProdOvhd,
                InnerJoin<AMOverhead, On<AMProdOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMProdOvhd.orderType, Equal<Required<AMProdOvhd.orderType>>,
                    And<AMProdOvhd.prodOrdID, Equal<Required<AMProdOvhd.prodOrdID>>,
                        And<AMProdOvhd.operationID, Equal<Required<AMProdOvhd.operationID>>,
                            And<AMOverhead.ovhdType, Equal<OverheadType.fixedType>>>>>
            >.Select(graph, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID))
            {
                var amProdOvhd = (AMProdOvhd)result;
                var amOverhead = (AMOverhead)result;

                if (amProdOvhd == null || string.IsNullOrWhiteSpace(amProdOvhd.OvhdID)
                    || amOverhead == null || string.IsNullOrWhiteSpace(amOverhead.OvhdID))
                {
                    continue;
                }

                totalCost += amProdOvhd.OFactor.GetValueOrDefault() * amOverhead.CostRate.GetValueOrDefault();
            }

            decimal? unitCost = totalCost.GetValueOrDefault();
            if (productionQty.GetValueOrDefault() != 0)
            {
                unitCost = totalCost.GetValueOrDefault() / productionQty.GetValueOrDefault();
            }

            return new PlannedCosts(unitCost, totalCost);
        }

        public static PlannedCosts GetPlannedVariableOverheadCost(PXGraph graph, AMProdOper amProdOper,
            decimal? remainingProductionQty,
            decimal? operLaborUnitCost, decimal? operMaterialUnitCost, decimal? operLaborHours, decimal? operMachHours)
        {
            return GetPlannedVariableOverheadCost(graph, amProdOper, remainingProductionQty, remainingProductionQty,
                remainingProductionQty, operLaborUnitCost, operMaterialUnitCost, operLaborHours, operMachHours);
        }

        /// <summary>
        /// Get the planned variable overhead unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedVariableOverheadCost(PXGraph graph, AMProdOper amProdOper,
            decimal? remainingProductionQty,
            decimal? totalProductionQty, decimal? totalProductionQtyWithScrap, decimal? operLaborUnitCost,
            decimal? operMaterialUnitCost, decimal? operLaborHours, decimal? operMachHours)
        {
            return GetPlannedVariableOverheadCost(amProdOper,
                remainingProductionQty,
                totalProductionQty,
                totalProductionQtyWithScrap,
                operLaborUnitCost,
                operMaterialUnitCost,
                operLaborHours,
                operMachHours,
                PXSelectJoin<AMProdOvhd,
                InnerJoin<AMOverhead, On<AMProdOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMProdOvhd.orderType, Equal<Required<AMProdOvhd.orderType>>,
                    And<AMProdOvhd.prodOrdID, Equal<Required<AMProdOvhd.prodOrdID>>,
                        And<AMProdOvhd.operationID, Equal<Required<AMProdOvhd.operationID>>,
                            And<AMOverhead.ovhdType, NotEqual<OverheadType.fixedType>>>>>
            >.Select(graph, amProdOper?.OrderType, amProdOper?.ProdOrdID, amProdOper?.OperationID));
        }

        /// <summary>
        /// Get the planned variable overhead unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedVariableOverheadCost(AMProdOper amProdOper,
            decimal? remainingProductionQty,
            decimal? totalProductionQty, decimal? totalProductionQtyWithScrap, decimal? operLaborUnitCost,
            decimal? operMaterialUnitCost, decimal? operLaborHours, decimal? operMachHours,
            PXResultset<AMProdOvhd> productionOperations)
        {
            if(productionOperations == null || productionOperations.Count == 0)
            {
                return new PlannedCosts(0, 0);
            }

#if DEBUG
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(amProdOper.DebuggerDisplay);
            sb.AppendLine($"remainingProductionQty={remainingProductionQty}".Indent(2));
            sb.AppendLine($"totalProductionQty={totalProductionQty}".Indent(2));
            sb.AppendLine($"operLaborUnitCost={operLaborUnitCost}".Indent(2));
            sb.AppendLine($"operMatlerialUnitCost={operMaterialUnitCost}".Indent(2));
            sb.AppendLine($"operLaborHours={operLaborHours}".Indent(2));
            sb.AppendLine($"operMachHours={operMachHours}".Indent(2));
            AMDebug.TraceWriteMethodName(sb.ToString());
#endif
            var overheadUnitCost = 0m;
            var overheadTotal = 0m;
            foreach (PXResult<AMProdOvhd, AMOverhead> result in productionOperations)
            {
                var amProdOvhd = (AMProdOvhd)result;
                var amOverhead = (AMOverhead)result;

                if (string.IsNullOrWhiteSpace(amProdOvhd?.OvhdID)
                    || string.IsNullOrWhiteSpace(amOverhead?.OvhdID))
                {
                    continue;
                }

                //covers OverheadType.VarQtyComp & OverheadType.VarQtyTot
                var calcOvdCost = amProdOvhd.OFactor.GetValueOrDefault() * amOverhead.CostRate.GetValueOrDefault();
                var orderQty = totalProductionQty.GetValueOrDefault(); // remainingProductionQty.GetValueOrDefault();
                var operOvdCalc = 0m;
                switch (amOverhead.OvhdType)
                {
                    case OverheadType.FixedType:
                        calcOvdCost = 0m;
                        break;

                    case OverheadType.VarLaborHrs:
                        // operLaborHours is the total for the full qty
                        calcOvdCost *= operLaborHours.GetValueOrDefault();
                        operOvdCalc = calcOvdCost;
                        break;

                    case OverheadType.VarMachHrs:
                        // operMachHours is the total for the full qty
                        calcOvdCost *= operMachHours.GetValueOrDefault();
                        operOvdCalc = calcOvdCost;
                        break;

                    case OverheadType.VarLaborCost:
                        calcOvdCost *= operLaborUnitCost.GetValueOrDefault();
                        operOvdCalc = calcOvdCost * orderQty;
                        break;

                    case OverheadType.VarMatlCost:
                        calcOvdCost *= operMaterialUnitCost.GetValueOrDefault();
                        operOvdCalc = calcOvdCost * orderQty;
                        break;

                    case OverheadType.VarQtyComp:
                        orderQty = totalProductionQty.GetValueOrDefault();
                        operOvdCalc = calcOvdCost * orderQty;
                        break;

                    case OverheadType.VarQtyTot:
                        orderQty = totalProductionQtyWithScrap.GetValueOrDefault();
                        operOvdCalc = calcOvdCost * orderQty;
                        break;
                }

                overheadTotal += Math.Max(amProdOvhd.TotActCost.GetValueOrDefault(), operOvdCalc);
                overheadUnitCost += calcOvdCost;
#if DEBUG
                AMDebug.TraceWriteMethodName($"{amOverhead.OvhdID} {OverheadType.GetTypeDesc(amOverhead.OvhdType)}: operOvdCalc={operOvdCalc}; TotActCost={amProdOvhd.TotActCost}; OFactor={amProdOvhd.OFactor}; CostRate={amOverhead.CostRate}");
#endif
            }

            return new PlannedCosts(overheadUnitCost, overheadTotal);
        }

        private static decimal GetPlannedMachineRate(AMProdOper amProdOper)
        {
            if (amProdOper == null
                || string.IsNullOrWhiteSpace(amProdOper.ProdOrdID)
                || amProdOper.OperationID == null)
            {
                return 0m;
            }

            return amProdOper.MachineUnits.GetValueOrDefault() == 0
                ? 0m
                : amProdOper.MachineUnitTime.GetValueOrDefault() / amProdOper.MachineUnits.GetValueOrDefault();
        }

        /// <summary>
        /// Un-rounded total machine time
        /// </summary>
        public static decimal? GetPlannedMachineTimeTotalRaw(AMProdOper amProdOper, decimal? productionQty)
        {
            return GetPlannedMachineRate(amProdOper) * productionQty.GetValueOrDefault();
        }

        public static int? GetPlannedMachineTimeTotal(AMProdOper amProdOper, decimal? productionQty)
        {
            return GetPlannedMachineTimeTotalRaw(amProdOper, productionQty).ToCeilingInt();
        }

        public static decimal? GetPlannedMachineHours(AMProdOper operation, decimal? productionQty)
        {
            return GetPlannedMachineTimeTotal(operation, productionQty).ToHours();
        }

        /// <summary>
        /// Get the planned machine unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedMachineCost(PXGraph graph, AMProdOper amProdOper, decimal? productionQty, decimal? machineHours)
        {
            return GetPlannedMachineCost(
                PXSelectJoin<
                    AMWCMach,
                    InnerJoin<AMMach,
                        On<AMWCMach.machID, Equal<AMMach.machID>>>,
                    Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>>>
                    .Select(graph, amProdOper?.WcID),
                amProdOper, productionQty, machineHours
                );
        }

        /// <summary>
        /// Get the planned machine unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedMachineCost(PXResultset<AMWCMach> machines, AMProdOper amProdOper, decimal? productionQty, decimal? machineHours)
        {
            if(machines == null || machines.Count == 0 || machineHours.GetValueOrDefault() == 0 || productionQty.GetValueOrDefault() == 0)
            {
                return new PlannedCosts(0, 0);
            }

            var operMachTot = 0m;

            foreach (PXResult<AMWCMach, AMMach> result in machines)
            {
                var wcMachine = (AMWCMach)result;
                var machine = (AMMach)result;

                if (string.IsNullOrWhiteSpace(wcMachine?.MachID)
                    || string.IsNullOrWhiteSpace(machine?.MachID))
                {
                    continue;
                }

                if(!machine.ActiveFlg.GetValueOrDefault())
                {
                    PXTrace.WriteInformation(Messages.GetLocal(Messages.MachineIsInactive, machine.MachID));
                    continue;
                }

                var standardCost = wcMachine.MachineOverride.GetValueOrDefault()
                    ? wcMachine.StdCost.GetValueOrDefault()
                    : machine.StdCost.GetValueOrDefault();

                operMachTot += machineHours.GetValueOrDefault() * standardCost;
            }

            var perUnit = operMachTot / productionQty;

            return new PlannedCosts(perUnit, operMachTot);
        }

        /// <summary>
        /// Get the planned labor unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedLaborCost(PXGraph graph, AMProdOper amProdOper, decimal? productionQty)
        {
            return GetPlannedLaborCost(graph, amProdOper, productionQty, null);
        }

        /// <summary>
        /// Get the planned labor unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedLaborCost(PXGraph graph, AMProdOper amProdOper, decimal? productionQty, decimal? remainingProductionQty)
        {
            return GetPlannedLaborCost(graph, amProdOper, productionQty, remainingProductionQty, null);
        }

        /// <summary>
        /// Get the planned labor unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedLaborCost(PXGraph graph, AMProdOper amProdOper, decimal? productionQty, decimal? remainingProductionQty, decimal? totalHours)
        {
            var wc = (AMWC)PXSelect<AMWC, Where<AMWC.wcID, Equal<Required<AMWC.wcID>>>>.Select(graph, amProdOper?.WcID);
            var shiftMst = (AMShiftMst)PXSelectJoin<AMShiftMst,
                                InnerJoin<AMShift, On<AMShiftMst.shiftID, Equal<AMShift.shiftID>>>,
                                Where<AMShift.wcID, Equal<Required<AMShift.wcID>>>,
                                    OrderBy<Asc<AMShift.shiftID>>>.SelectWindowed(graph, 0, 1, wc?.WcID);
            return GetPlannedLaborCost(wc, shiftMst, amProdOper, productionQty, remainingProductionQty, totalHours, UomHelper.CostDecimalPrecision);
        }

        /// <summary>
        /// Get the planned labor unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedLaborCost(AMWC wc, AMShiftMst shiftMst, AMProdOper amProdOper, decimal? productionQty, decimal? remainingProductionQty)
        {
            return GetPlannedLaborCost(wc, shiftMst, amProdOper, productionQty, remainingProductionQty, null, UomHelper.CostDecimalPrecision);
        }

        /// <summary>
        /// Get the planned labor unit and total cost for the given operation
        /// </summary>
        public static PlannedCosts GetPlannedLaborCost(AMWC wc, AMShiftMst shiftMst, AMProdOper amProdOper, decimal? productionQty, decimal? remainingProductionQty, decimal? totalHours, int decimalPrecision)
        {
            if (string.IsNullOrWhiteSpace(amProdOper?.OrderType)
                || string.IsNullOrWhiteSpace(amProdOper.ProdOrdID)
                || amProdOper.OperationID == null
                || productionQty.GetValueOrDefault() == 0
                || wc == null)
            {
                return new PlannedCosts(0, 0);
            }

            if(remainingProductionQty == null)
            {
                remainingProductionQty = productionQty;
            }

            if(totalHours == null)
            {
                totalHours = GetPlannedLaborHoursTotal(amProdOper, productionQty).GetValueOrDefault();
            }

            var actualLaborHours = amProdOper.ActualLaborTime.GetValueOrDefault().ToHours(9);
            var hoursRemaining = Math.Round(totalHours.GetValueOrDefault() - actualLaborHours, 3);

            if (hoursRemaining < 0)
            {
                hoursRemaining = 0;
            }

            var planLaborRate = ShiftDiffType.GetShiftDifferentialCost(wc, shiftMst);
            var laborTotalCost = Math.Round(amProdOper.ActualLabor.GetValueOrDefault() + (planLaborRate * hoursRemaining), decimalPrecision);
            var laborUnitCost = productionQty == 0 ? laborTotalCost : (laborTotalCost / productionQty);
#if DEBUG
            if (productionQty.GetValueOrDefault() != 0)
            {
                AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Total Plan Hours {totalHours}; Act Time {amProdOper.ActualLaborTime.GetValueOrDefault()} (Hours {actualLaborHours}); Remaining Hours {hoursRemaining}");
                AMDebug.TraceWriteMethodName($"[{amProdOper.OperationCD}] Total Labor Cost {laborTotalCost}; Act Labor Cost {amProdOper.ActualLabor.GetValueOrDefault()}; Remaining Labor Cost {planLaborRate * hoursRemaining}; Labor Unit Cost {laborUnitCost}; Labor Rate {planLaborRate}");
            }
#endif

            return new PlannedCosts(laborUnitCost, laborTotalCost);
        }

        private static decimal GetPlannedLaborRate(AMProdOper amProdOper, decimal? productionQty)
        {
            if (string.IsNullOrWhiteSpace(amProdOper?.ProdOrdID)
                || amProdOper.OperationID == null)
            {
                return 0m;
            }

            decimal? setupTimePerQty = productionQty.GetValueOrDefault() == 0
                ? amProdOper.SetupTime.GetValueOrDefault()
                : amProdOper.SetupTime.GetValueOrDefault() / productionQty.GetValueOrDefault();

            decimal? runTimeRate = amProdOper.RunUnits.GetValueOrDefault() == 0
                ? 0m
                : amProdOper.RunUnitTime.GetValueOrDefault() / amProdOper.RunUnits.GetValueOrDefault();

            return setupTimePerQty.GetValueOrDefault() + runTimeRate.GetValueOrDefault();
        }

        /// <summary>
        /// Get the planned labor time (minutes) per production qty for the given operation.
        /// </summary>
        public static decimal? GetPlannedLaborTimePerQty(AMProdOper amProdOper, decimal? productionQty)
        {
            return GetPlannedLaborRate(amProdOper, productionQty);
        }

        public static int? GetPlannedLaborTimeTotal(AMProdOper amProdOper, decimal? productionQty)
        {
            return GetPlannedLaborTimeTotalRaw(amProdOper, productionQty).ToCeilingInt();
        }

        public static decimal? GetPlannedLaborTimeTotalRaw(AMProdOper amProdOper, decimal? productionQty)
        {
            return Math.Round(GetPlannedLaborRate(amProdOper, productionQty) * productionQty.GetValueOrDefault(), 9);
        }

        /// <summary>
        /// Get the planned labor hours per production qty for the given operation
        /// </summary>
        public static decimal? GetPlannedLaborHoursPerQty(AMProdOper amProdOper, decimal? productionQty)
        {
            return GetPlannedLaborTimePerQty(amProdOper, productionQty).GetValueOrDefault().ToHours(9);
        }

        public static decimal? GetPlannedLaborHoursTotal(AMProdOper amProdOper, decimal? productionQty)
        {
            return GetPlannedLaborTimeTotal(amProdOper, productionQty).ToHours(9);
        }

        /// <summary>
        /// Get the planned material unit and total cost for the given operation
        /// </summary>
        public static MaterialPlannedCosts GetPlannedMaterialCost(PXGraph graph, AMProdOper amProdOper, decimal? productionQty)
        {
            var regMatlUnitCost = 0m;
            var regMatlTot = 0m;

            var refMatlUnitCost = 0m;
            var refMatlTot = 0m;

            var subMatlUnitCost = 0m;
            var subMatlTot = 0m;
            
            var decimalPlaces = CurrencyHelper.GetBaseCuryDecimalPlaces(graph);

            foreach (AMProdMatl matl in PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                    And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>>
            >>>.Select(graph, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID))
            {
                if (matl?.ProdOrdID == null)
                {
                    continue;
                }

                var operMatlQtyRem = matl.QtyRemaining.GetValueOrDefault();
                var isByProduct = matl.IsByproduct.GetValueOrDefault();
                if (operMatlQtyRem < 0 && !isByProduct ||
                    operMatlQtyRem > 0 && isByProduct)
                {
                    operMatlQtyRem = 0;
                }

                var matlUnitCost = (productionQty.GetValueOrDefault() == 0 
                    ? matl.QtyReqWithScrap.GetValueOrDefault() 
                    : matl.TotalQtyRequired.GetValueOrDefault() / productionQty.GetValueOrDefault() )
                                    * matl.UnitCost.GetValueOrDefault();

                var matlTot = matl.TotActCost.GetValueOrDefault() + Math.Round(matl.UnitCost.GetValueOrDefault() * operMatlQtyRem, decimalPlaces, MidpointRounding.AwayFromZero);

                if (matl.MaterialType != AMMaterialType.Subcontract)
                {
                    // Regular Material Costs
                    regMatlUnitCost += matlUnitCost;
                    regMatlTot += matlTot;
                    continue;
                }

                if (matl.SubcontractSource == AMSubcontractSource.VendorSupplied)
                {
                    // Reference Material
                    refMatlUnitCost += matlUnitCost;
                    refMatlTot += matlTot;
                    continue;
                }

                // Subcontract Material
                subMatlUnitCost += matlUnitCost;
                subMatlTot += matlTot;
            }

            return new MaterialPlannedCosts
            {
                ReferenceMaterial = new PlannedCosts(refMatlUnitCost, refMatlTot),
                RegularMaterial = new PlannedCosts(regMatlUnitCost, regMatlTot),
                Subcontract = new PlannedCosts(subMatlUnitCost, subMatlTot)
            };
        }

        /// <summary>
        /// Get the current planned tool cost for a give production order operation
        /// </summary>
        /// <param name="graph">Calling Graph</param>
        /// <param name="amProdOper">Source production operation to calculate planned cost against</param>
        /// <param name="productionQty">Source remaining qty </param>
        /// <returns>Planned Cost object containing unit and total cost</returns>
        public static PlannedCosts GetPlannedToolCost(PXGraph graph, AMProdOper amProdOper, decimal? productionQty)
        {
            decimal? operToolUnitCost = 0;
            decimal? operToolTot = 0;

            foreach (PXResult<AMProdTool, AMToolMst> result in PXSelectJoin<AMProdTool,
                InnerJoin<AMToolMst, On<AMProdTool.toolID, Equal<AMToolMst.toolID>>>,
                Where<AMProdTool.orderType, Equal<Required<AMProdTool.orderType>>,
                    And<AMProdTool.prodOrdID, Equal<Required<AMProdTool.prodOrdID>>,
                        And<AMProdTool.operationID, Equal<Required<AMProdTool.operationID>>>>>
            >.Select(graph, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationID))
            {
                var amProdTool = (AMProdTool)result;
                var amToolMst = (AMToolMst)result;

                if (amProdTool == null || string.IsNullOrWhiteSpace(amProdTool.ToolID)
                    || amToolMst == null || string.IsNullOrWhiteSpace(amToolMst.ToolID))
                {
                    continue;
                }

                decimal? operToolQtyRem = amProdTool.QtyReq.GetValueOrDefault() * productionQty.GetValueOrDefault() - amProdTool.TotActUses.GetValueOrDefault();

                if (operToolQtyRem.GetValueOrDefault() < 0)
                {
                    operToolQtyRem = 0;
                }

                operToolUnitCost += amProdTool.QtyReq.GetValueOrDefault() * amToolMst.UnitCost.GetValueOrDefault();
                operToolTot += amProdTool.TotActCost.GetValueOrDefault() + (amToolMst.UnitCost.GetValueOrDefault() * operToolQtyRem.GetValueOrDefault());
#if DEBUG
                decimal? remainingCost = amProdTool.UnitCost.GetValueOrDefault() * operToolQtyRem.GetValueOrDefault();
                if (operToolQtyRem.GetValueOrDefault() != 0 || remainingCost.GetValueOrDefault() != 0)
                {
                    AMDebug.TraceWriteMethodName(string.Format("Remaining Qty {0} Cost {1} for Tool ID {2}", operToolQtyRem.GetValueOrDefault(), remainingCost.GetValueOrDefault(), amProdTool.ToolID));
                }
#endif
            }

            return new PlannedCosts(operToolUnitCost, operToolTot);
        }

        /// <summary>
        /// Calculate a negative (return) move/labor transaction with qty. 
        /// Determines the unit cost as it relates to all previously transacted move qty for a given order.
        /// </summary>
        /// <param name="ammTran">Current transaction being processed requesting the unit cost</param>
        /// <param name="amProdItem">Production order related to the current transaction</param>
        /// <returns>calculated unit cost</returns>
        public ProductionOperationCostResults CalculateReturnUnitCost(AMMTran ammTran, AMProdItem amProdItem, MoveOperationQtyTotals moveOperationQtyTotals, decimal? previouslyReturnedBaseQty)
        {
            if (ammTran == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            if (string.IsNullOrWhiteSpace(amProdItem?.ProdOrdID))
            {
                throw new PXArgumentException(nameof(amProdItem));
            }

            var operCostResults = new ProductionOperationCostResults(amProdItem);

            if (string.IsNullOrWhiteSpace(ammTran.ProdOrdID)
                || ammTran.Qty.GetValueOrDefault() >= 0)
            {
                operCostResults.UnitAmount = ammTran.UnitCost.GetValueOrDefault();
                operCostResults.TotalAmount = ammTran.TranAmt.GetValueOrDefault();
                return operCostResults;
            }

            if (!ammTran.ProdOrdID.EqualsWithTrim(amProdItem.ProdOrdID))
            {
                throw new PXException(Messages.GetLocal(Messages.InvalidProductionNbr, amProdItem.OrderType, amProdItem.ProdOrdID));
            }

            InventoryItem inventoryItem = PXSelect<
            	InventoryItem, 
            	Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
            	.Select(Graph, amProdItem.InventoryID);

            if (inventoryItem == null)
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                    Common.Cache.GetCacheName(typeof(InventoryItem))));
            }

            decimal? valMethodUnitCost = null;
            switch (inventoryItem.ValMethod)
            {
                case INValMethod.Standard:
                    operCostResults.UnitAmount = GetStandardUnitCost(ammTran, inventoryItem).GetValueOrDefault();
                    operCostResults.TotalAmount = operCostResults.UnitAmount * ammTran.BaseQty.GetValueOrDefault();
                    return operCostResults;
                case INValMethod.Specific:
                    valMethodUnitCost = GetSpecificUnitCost(ammTran, inventoryItem);
                    break;
                case INValMethod.FIFO:
                    valMethodUnitCost = GetFIFOUnitCost(ammTran, inventoryItem);
                    break;
            }

            var operWipCompleteResults = PXSelect<
            	AMMTran,
            	Where<AMMTran.released, Equal<True>,
            		And<AMMTran.tranType, Equal<AMTranType.operWIPComplete>,
            		And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
            		And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>>>>>,
            	OrderBy<
            		Desc<AMMTran.tranDate, 
            		Desc<AMMTran.batNbr, 
            		Desc<AMMTran.lineNbr>>>>>
            	.Select(Graph, ammTran.OrderType, ammTran.ProdOrdID)
            	.ToFirstTableList();

            var releasedTransactions = PXSelect<
            	AMMTran,
            	Where<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
            		And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
            		And<AMMTran.released, Equal<True>,
            		And2<
            			Where<AMMTran.docType, Equal<AMDocType.move>,
            				Or<AMMTran.docType, Equal<AMDocType.labor>>>,
            			And<AMMTran.qty, NotEqual<decimal0>>>>>>,
            	OrderBy<
            		Desc<AMMTran.tranDate, 
            		Desc<AMMTran.batNbr, 
            		Desc<AMMTran.lineNbr>>>>
            >
            	.Select(Graph, amProdItem.OrderType, amProdItem.ProdOrdID)
            	.ToFirstTableList();

            var calcReturn = CalculateReturnUnitCost(operCostResults, ammTran, inventoryItem, previouslyReturnedBaseQty, releasedTransactions, operWipCompleteResults);

            if (valMethodUnitCost.GetValueOrDefault() != 0m && calcReturn != null && calcReturn.UnitAmount != valMethodUnitCost.GetValueOrDefault())
            {
                calcReturn.UnitAmount = valMethodUnitCost.GetValueOrDefault();
                calcReturn.TotalAmount = Math.Abs(ammTran.BaseQty.GetValueOrDefault()) * calcReturn.UnitAmount;
            }

            return calcReturn;
        }

        public ProductionOperationCostResults CalculateReturnUnitCost(ProductionOperationCostResults operCostResults,
            AMMTran ammTran, InventoryItem inventoryItem, decimal? previouslyReturnedBaseQty,
            List<AMMTran> releasedTransactions, List<AMMTran> operWipCompleteResults)
        {
            if (releasedTransactions == null)
            {
                return operCostResults;
            }

            var tranReturningQty = Math.Abs(ammTran.BaseQty.GetValueOrDefault());
            var receivedQtyRT = 0m;    // Running total of qty issued (Value does not account for returned issued qty!!)
            var returnedQtyRT = Math.Abs(previouslyReturnedBaseQty.GetValueOrDefault());    // Running total of qty returned
            var currentQty = 0m;           // Manage ammtran.qty
            
            foreach (var releasedTran in releasedTransactions.OrderByDescending(tran => tran.LastModifiedDateTime))
            {
                //we need to find the related transaction to the unique cost method/original transaction

                if (inventoryItem.ValMethod == INValMethod.Specific &&
                    !releasedTran.LotSerialNbr.EqualsWithTrim(ammTran.LotSerialNbr))
                {
                    continue;
                }

                if (inventoryItem.ValMethod == INValMethod.FIFO &&
                    releasedTran.INDocType == INDocType.Receipt &&
                    (string.IsNullOrWhiteSpace(ammTran.ReceiptNbr) ||
                     releasedTran.INBatNbr != ammTran.ReceiptNbr))
                {
                    continue;
                }

                var isReturnQty = releasedTran.BaseQty.GetValueOrDefault() < 0;
                var multiplyer = isReturnQty ? -1 : 1;
                currentQty = releasedTran.BaseQty.GetValueOrDefault() * multiplyer;

                if (currentQty == 0 )
                {
                    continue;
                }

                if (isReturnQty)
                {
                    returnedQtyRT += currentQty;
                    currentQty = 0m;
                }
                else
                {
                    if (returnedQtyRT > 0)
                    {
                        returnedQtyRT -= currentQty;

                        //partial return
                        if (returnedQtyRT < 0)
                        {
                            currentQty = Math.Abs(returnedQtyRT);
                            returnedQtyRT = 0m;
                        }
                        else
                        {
                            currentQty = 0m;
                        }
                    }

                    //Only part of the current transaction is required
                    if (receivedQtyRT + currentQty > tranReturningQty)
                    {
                        currentQty = tranReturningQty - receivedQtyRT;
                    }
                }

                if (currentQty != 0 && operWipCompleteResults != null)
                {
                    //Find the related Mfg to Inv. by Oper records...
                    foreach (var operWipComplete in GetRelatedOperWipCompleteRecords(releasedTran, operWipCompleteResults))
                    {
                        var unitAmount = GetUnitCostFromOperWipComplete(operWipComplete, Math.Abs(releasedTran.BaseQty.GetValueOrDefault()));
                        if (unitAmount == null)
                        {
                            continue;
                        }

                        var processQty = currentQty * -1;
                        //Potential add for multiple WIP complete for same oper... this is ok
                        operCostResults.AddOperationCost(GetProdOper(operWipComplete), unitAmount.GetValueOrDefault(), unitAmount.GetValueOrDefault() * processQty, processQty);
                    }
                }

                receivedQtyRT += currentQty;

                if (receivedQtyRT >= tranReturningQty)
                {
                    //found all the trans/qty required for returning
                    break;
                }
            }

            return operCostResults;
        }

        protected virtual AMProdOper GetProdOper(AMMTran tran)
        {
            return (AMProdOper)Graph.Caches<AMProdOper>().Locate(new AMProdOper
            {
                OrderType = tran.OrderType,
                ProdOrdID = tran.ProdOrdID,
                OperationID = tran.OperationID
            }) ?? PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                        And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>
            >.Select(Graph, tran.OrderType, tran.ProdOrdID, tran.OperationID);
        }

        protected virtual List<AMMTran> GetRelatedOperWipCompleteRecords(AMMTran ammTran, List<AMMTran> operWipCompleteResults)
        {
            var list = new List<AMMTran>();
            if (ammTran?.BatNbr == null || operWipCompleteResults == null)
            {
                return list;
            }

            foreach (var wipCompleteResult in operWipCompleteResults)
            {
                if (ammTran.IsTransactionOriginal(wipCompleteResult) && wipCompleteResult.TranType == AMTranType.OperWIPComplete)
                {
                    list.Add(wipCompleteResult);
                }
            }
            return list;
        }

        protected virtual decimal? GetUnitCostFromOperWipComplete(AMMTran operWipCompleteTran, decimal? baseQty)
        {
            var baseQtyValue = baseQty ?? operWipCompleteTran.BaseQty.GetValueOrDefault();
            return operWipCompleteTran == null || baseQtyValue == 0
                ? (decimal?)null
                : operWipCompleteTran.TranAmt.GetValueOrDefault() / baseQtyValue;
        }

        protected decimal? GetSpecificUnitCost(AMMTran ammTran, InventoryItem inventoryItem)
        {
            if (inventoryItem?.InventoryID == null)
            {
                throw new PXArgumentException(nameof(inventoryItem));
            }

            return GetSpecificUnitCost(ammTran, inventoryItem.BaseUnit);
        }

        protected decimal? GetSpecificUnitCost(AMMTran ammTran, string baseUnit)
        {
            if (ammTran?.InventoryID == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            if (string.IsNullOrWhiteSpace(baseUnit))
            {
                throw new PXArgumentException(nameof(baseUnit));
            }

            if (!string.IsNullOrEmpty(ammTran.LotSerialNbr))
            {
                INCostStatus status = PXSelectJoin<INCostStatus,
                    LeftJoin<INLocation, On<INLocation.locationID, Equal<Current<AMMTran.locationID>>>,
                        InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>>>,
                    Where<INCostStatus.inventoryID, Equal<Current<AMMTran.inventoryID>>,
                        And2<Where<INLocation.isCosted, Equal<boolFalse>,
                                And<INCostStatus.costSiteID, Equal<Current<AMMTran.siteID>>,
                                    Or<INCostStatus.costSiteID, Equal<Current<AMMTran.locationID>>>>>,
                            And<INCostSubItemXRef.subItemID, Equal<Current<AMMTran.subItemID>>,
                                And<INCostStatus.lotSerialNbr, Equal<Current<AMMTran.lotSerialNbr>>>>>>>.SelectSingleBound(Graph, new object[] { ammTran });
                if (status != null && status.QtyOnHand.GetValueOrDefault() != 0m)
                {
                    return ConvertUnitCost(ammTran, baseUnit, PXDBPriceCostAttribute.Round(status.TotalCost.GetValueOrDefault() / status.QtyOnHand.GetValueOrDefault()));
                }
            }
            return null;
        }

        protected decimal? GetFIFOUnitCost(AMMTran ammTran, InventoryItem inventoryItem)
        {
            if (inventoryItem == null || inventoryItem.InventoryID == null)
            {
                throw new PXArgumentException(nameof(inventoryItem));
            }

            return GetFIFOUnitCost(ammTran, inventoryItem.BaseUnit);
        }

        protected decimal? GetFIFOUnitCost(AMMTran ammTran, string baseUnit)
        {
            if (ammTran == null || ammTran.InventoryID == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            if (string.IsNullOrWhiteSpace(baseUnit))
            {
                throw new PXArgumentException(nameof(baseUnit));
            }

            if (!string.IsNullOrEmpty(ammTran.ReceiptNbr))
            {
                INCostStatus status = PXSelectJoin<INCostStatus,
                    LeftJoin<INLocation, On<INLocation.locationID, Equal<Current<AMMTran.locationID>>>,
                        InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>>>,
                    Where<INCostStatus.inventoryID, Equal<Current<AMMTran.inventoryID>>,
                        And2<Where<INLocation.isCosted, Equal<boolFalse>,
                                And<INCostStatus.costSiteID, Equal<Current<AMMTran.siteID>>,
                                    Or<INCostStatus.costSiteID, Equal<Current<AMMTran.locationID>>>>>,
                            And<INCostSubItemXRef.subItemID, Equal<Current<AMMTran.subItemID>>,
                                And<INCostStatus.receiptNbr, Equal<Current<AMMTran.receiptNbr>>>>>>>.SelectSingleBound(Graph, new object[] { ammTran });
                if (status != null && status.QtyOnHand != 0m)
                {
                    return ConvertUnitCost(ammTran, baseUnit, PXDBPriceCostAttribute.Round(status.TotalCost.GetValueOrDefault() / status.QtyOnHand.GetValueOrDefault()));
                }
            }
            return null;
        }

        public sealed class MaterialPlannedCosts
        {
            public PlannedCosts RegularMaterial;
            public PlannedCosts Subcontract;
            public PlannedCosts ReferenceMaterial;
        }

        /// <summary>
        /// Transfer object to store planned total and unit cost values
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("TotalCost = {TotalCost}; UnitCost = {UnitCost}")]
        public sealed class PlannedCosts
        {
            public readonly decimal? TotalCost;
            public readonly decimal? UnitCost;

            public PlannedCosts(decimal? unitCost, decimal? totalCost)
            {
                UnitCost = unitCost.GetValueOrDefault();
                TotalCost = totalCost.GetValueOrDefault();
            }
        }
    }
}