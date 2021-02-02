using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class ProductionStatus
    {
        public static bool IsEditableStatus(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return false;
            }

            return IsEditableStatus(amProdItem.StatusID, amProdItem.Hold.GetValueOrDefault());
        }

        public static bool IsOpenOrder(AMProdItem prodItem)
        {
            if (prodItem?.StatusID == null)
            {
                return false;
            }

            switch (prodItem.StatusID)
            {
                case ProductionOrderStatus.Planned:
                    return false;

                case ProductionOrderStatus.Released:
                    return true;

                case ProductionOrderStatus.InProcess:
                    return true;

                case ProductionOrderStatus.Cancel:
                    return false;

                case ProductionOrderStatus.Completed:
                    return false;

                case ProductionOrderStatus.Closed:
                    return false;

                default:
                    return false;
            }
        }

        public static bool IsEditableStatus(string productionStatusID, bool isOrderOnHold)
        {
            switch (productionStatusID)
            {
                case ProductionOrderStatus.Planned:
                    return true;

                case ProductionOrderStatus.Released:
                    return isOrderOnHold;

                case ProductionOrderStatus.InProcess:
                    //overall order can be enabled only if on hold and in process (field level enabling might be required)
                    return isOrderOnHold;

                case ProductionOrderStatus.Hold:
                    return true;

                case ProductionOrderStatus.Cancel:
                    return false;

                case ProductionOrderStatus.Completed:
                    return false;

                case ProductionOrderStatus.Closed:
                    return false;

                default:
                    return false;
            }
        }

        public static bool CanPlanAllocations(AMProdItem prodItem)
        {
            if (prodItem == null)
            {
                return false;
            }

            switch (prodItem.StatusID.TrimIfNotNullEmpty())
            {
                case ProductionOrderStatus.Released:
                case ProductionOrderStatus.InProcess:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Does the given production order allow for a transaction entry/process release
        /// </summary>
        public static bool IsValidTransactionStatus(PXGraph graph, string orderType, string productionOrderNbr)
        {
            if (string.IsNullOrWhiteSpace(orderType) || string.IsNullOrWhiteSpace(productionOrderNbr))
            {
                return false;
            }

            return IsValidTransactionStatus(PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(graph, orderType, productionOrderNbr));
        }

        /// <summary>
        /// Does the given production order allow for a transaction entry/process release
        /// </summary>
        public static bool IsValidTransactionStatus(AMProdItem prodItem)
        {
            if (prodItem == null || string.IsNullOrWhiteSpace(prodItem.ProdOrdID))
            {
                return false;
            }

            if (prodItem.Function == null
                || prodItem.Function != OrderTypeFunction.Regular
                || prodItem.Hold.GetValueOrDefault())
            {
                return false;
            }

            switch (prodItem.StatusID.TrimIfNotNullEmpty())
            {
                case ProductionOrderStatus.Planned:
                case ProductionOrderStatus.Released:
                case ProductionOrderStatus.InProcess:
                case ProductionOrderStatus.Completed:
                    return true;
                default:
                    return false;
            }

        }

        public static bool IsReleasedTransactionStatus(AMProdItem prodItem)
        {
            if (prodItem == null || string.IsNullOrWhiteSpace(prodItem.ProdOrdID))
            {
                return false;
            }

            if (prodItem.Function == null
                || (prodItem.Function != OrderTypeFunction.Regular && prodItem.Function != OrderTypeFunction.Disassemble)
                || prodItem.Hold.GetValueOrDefault())
            {
                return false;
            }

            switch (prodItem.StatusID.TrimIfNotNullEmpty())
            {
                case ProductionOrderStatus.Released:
                case ProductionOrderStatus.InProcess:
                case ProductionOrderStatus.Completed:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsClosedOrCanceled(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                return true;
            }

            return IsClosedOrCanceled(amProdItem.StatusID);
        }

        public static bool IsClosedOrCanceled(string productionStatusID)
        {
            if (string.IsNullOrWhiteSpace(productionStatusID))
            {
                return true;
            }

            switch (productionStatusID)
            {
                case ProductionOrderStatus.Closed:
                case ProductionOrderStatus.Cancel:
                    return true;
                default:
                    return false;
            }
        }

        public static void SetStatus(List<AMProdItem> productionItemList, string newStatusID, string periodID = "")
        {
            SetStatus(productionItemList, newStatusID, new FinancialPeriod() { FinancialPeriodID = periodID });
        }

        /// <summary>
        /// Use SetProductionStatus for any status change of a production order
        /// Replaces stored procedure "amProdItem_SetStatus" and possibly others
        /// production orders in a single call to keep all GL entries in a single batch when possible.
        /// </summary>
        /// <param name="productionItemList">List of all production orders that will have their status changed. (AMProdItem)</param>
        /// <param name="newStatusID">Production status the production orders are changing to.</param>
        /// <param name="financialPeriod">Financial Period to use for accounting</param>
        public static void SetStatus(List<AMProdItem> productionItemList, string newStatusID,  FinancialPeriod financialPeriod)
        {
            SetStatus(productionItemList, newStatusID, financialPeriod, false, false);
        }

        internal static void SetStatusTranScope(List<AMProdItem> productionItemList, string newStatusID, FinancialPeriod financialPeriod, bool isMassProcese, bool checkUnreleasedTrans)
        {
            var prodDetail = PXGraph.CreateInstance<ProdDetail>();
            var wipAdjustmentEntry = PXGraph.CreateInstance<WIPAdjustmentEntry>();
            wipAdjustmentEntry.Clear();
            PXOperationCompletedException completedException = null;
            try
            {
                SetStatusTranScope(productionItemList, newStatusID, financialPeriod, isMassProcese, checkUnreleasedTrans, prodDetail, wipAdjustmentEntry);
            }
            catch (PXOperationCompletedException e)
            {
                completedException = e;
            }

            if (wipAdjustmentEntry.batch.Select().Count > 0)
            {
                AMDocumentRelease.ReleaseDoc(new List<AMBatch> { wipAdjustmentEntry.batch.Current });
            }

            if (completedException != null)
            {
                throw completedException;
            }
        }

        internal static void SetStatus(List<AMProdItem> productionItemList, string newStatusID, FinancialPeriod financialPeriod, bool isMassProcese, bool checkUnreleasedTrans)
        {
            var prodDetail = PXGraph.CreateInstance<ProdDetail>();
            var wipAdjustmentEntry = PXGraph.CreateInstance<WIPAdjustmentEntry>();
            wipAdjustmentEntry.Clear();
            PXOperationCompletedException completedException = null;
            try
            {
                SetStatus(productionItemList, newStatusID, financialPeriod, isMassProcese, checkUnreleasedTrans, prodDetail, wipAdjustmentEntry);
            }
            catch (PXOperationCompletedException e)
            {
                completedException = e;
            }

            if (wipAdjustmentEntry.batch.Select().Count > 0)
            {
                AMDocumentRelease.ReleaseDoc(new List<AMBatch> { wipAdjustmentEntry.batch.Current });
            }

            if (completedException != null)
            {
                throw completedException;
            }
        }

        internal static void SetStatusTranScope(List<AMProdItem> productionItemList, string newStatusID, FinancialPeriod financialPeriod, bool isMassProcess, bool checkUnreleasedTrans, ProdDetail prodDetail, WIPAdjustmentEntry wipAdjustmentEntry)
        {
            wipAdjustmentEntry.ampsetup.Current.RequireControlTotal = false;
            wipAdjustmentEntry.ampsetup.Current.HoldEntry = false;

            if (string.IsNullOrWhiteSpace(financialPeriod.FinancialPeriodID))
            {
                financialPeriod.FinancialPeriodID = ProductionTransactionHelper.PeriodFromDate(prodDetail, wipAdjustmentEntry.Accessinfo.BusinessDate);
            }

            PXOperationCompletedException completedException = null;
            using (var SetProdStatusScope = new PXTransactionScope())
            {
                try
                {
                    SetStatus(productionItemList, newStatusID, financialPeriod, isMassProcess, checkUnreleasedTrans, prodDetail, wipAdjustmentEntry);
                }
                catch (PXOperationCompletedException e)
                {
                    completedException = e;
                }

                SetProdStatusScope.Complete();
            }

            if (completedException != null)
            {
                throw completedException;
            }
        }

        internal static void SetStatus(List<AMProdItem> productionItemList, string newStatusID, FinancialPeriod financialPeriod, bool isMassProcess, bool checkUnreleasedTrans, ProdDetail prodDetail, WIPAdjustmentEntry wipAdjustmentEntry)
        {
            wipAdjustmentEntry.ampsetup.Current.RequireControlTotal = false;
            wipAdjustmentEntry.ampsetup.Current.HoldEntry = false;

            if (string.IsNullOrWhiteSpace(financialPeriod.FinancialPeriodID))
            {
                financialPeriod.FinancialPeriodID = ProductionTransactionHelper.PeriodFromDate(prodDetail, wipAdjustmentEntry.Accessinfo.BusinessDate);
            }

            var failed = false;

            for (var i = 0; i < productionItemList.Count; i++)
            {
                var productionItem = productionItemList[i];

                if (productionItem == null)
                {
                    continue;
                }

                var origStatus = productionItem.StatusID;
                try
                {
                    prodDetail.Clear();
                    prodDetail.ProdItemRecords.Current = prodDetail.ProdItemRecords.Search<AMProdItem.prodOrdID>(productionItem.ProdOrdID, productionItem.OrderType);

                    if (checkUnreleasedTrans && ProductionTransactionHelper.ProductionOrderHasUnreleasedTransactions(prodDetail, productionItem, out var unreleasedMsg))
                    {
                        throw new PXException(unreleasedMsg);
                    }

                    SetProductionStatus(prodDetail, productionItem, newStatusID, financialPeriod, wipAdjustmentEntry);
                    prodDetail.PersistBase();

                    if (isMassProcess)
                    {
                        PXProcessing<AMProdItem>.SetInfo(i, PX.Data.ActionsMessages.RecordProcessed);
                    }
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<AMProdItem>.SetError(i, e);
                        failed = true;
                    }
                    else if (productionItemList.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }
                    else
                    {
                        failed = true;
                    }

                    PXTrace.WriteError(Messages.GetLocal(Messages.ProductionOrderStatusChangeError,
                        productionItem.OrderType, productionItem.ProdOrdID,
                        ProductionOrderStatus.GetStatusDescription(origStatus),
                        ProductionOrderStatus.GetStatusDescription(newStatusID),
                        PXTraceHelper.GetExceptionMessage(e)
                    ));
                }
            }

            if (wipAdjustmentEntry.batch.Select().Count > 0)
            {
                wipAdjustmentEntry.Persist();
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }
        }

        /// <summary>
        /// Process for setting the given production item's production status
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="productionItem">production order receiving the status update</param>
        /// <param name="newStatusID">new production status ID</param>
        /// <returns>List of batches to be released if any (Use AMDocumentRelease.ReleaseDoc)</returns>
        public static WIPAdjustmentEntry SetProductionStatus(PXGraph graph, AMProdItem productionItem, string newStatusID)
        {
            return SetProductionStatus(graph, productionItem, newStatusID, null);
        }

        /// <summary>
        /// Process for setting the given production item's production status
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="productionItem">production order receiving the status update</param>
        /// <param name="newStatusID">new production status ID</param>
        /// <param name="financialPeriod">financial period related to the process (used for created transactions if any)</param>
        /// <returns>List of batches to be released if any (Use AMDocumentRelease.ReleaseDoc)</returns>
        public static WIPAdjustmentEntry SetProductionStatus(PXGraph graph, AMProdItem productionItem, string newStatusID, FinancialPeriod financialPeriod)
        {
            WIPAdjustmentEntry wipAdjustmentEntry = PXGraph.CreateInstance<WIPAdjustmentEntry>();
            wipAdjustmentEntry.Clear();
            wipAdjustmentEntry.ampsetup.Current.RequireControlTotal = false;

            SetProductionStatus(graph, productionItem, newStatusID, financialPeriod, wipAdjustmentEntry);

            return wipAdjustmentEntry;
        }

        /// <summary>
        /// Process for setting the given production item's production status
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="productionItem">production order receiving the status update</param>
        /// <param name="newStatusID">new production status ID</param>
        /// <param name="financialPeriod">financial period related to the process (used for created transactions if any)</param>
        /// <param name="wipAdjustmentEntry">used in WIP adjustment creation depending on the status change</param>
        /// <returns>List of batches to be released if any (Use AMDocumentRelease.ReleaseDoc)</returns>
        public static void SetProductionStatus(PXGraph graph, AMProdItem productionItem, string newStatusID, FinancialPeriod financialPeriod, WIPAdjustmentEntry wipAdjustmentEntry)
        {
            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (productionItem == null
                || string.IsNullOrWhiteSpace(productionItem.OrderType))
            {
                throw new PXArgumentException(nameof(productionItem));
            }

            var origStatusID = productionItem.StatusID;

            try
            {
                if (string.IsNullOrWhiteSpace(newStatusID))
                {
                    throw new PXArgumentException(nameof(newStatusID));
                }

                if (financialPeriod == null || string.IsNullOrWhiteSpace(financialPeriod.FinancialPeriodID))
                {
                    DateTime financialDate = wipAdjustmentEntry == null
                        ? graph.Accessinfo.BusinessDate.GetValueOrDefault()
                        : wipAdjustmentEntry.Accessinfo.BusinessDate.GetValueOrDefault();
                    financialPeriod = new FinancialPeriod
                    {
                        FinancialPeriodID = ProductionTransactionHelper.PeriodFromDate(graph, financialDate)
                    };
                }

                var newStatusRequiresWIPVar = false;
                var newProdEvent = new AMProdEvnt();
                AMProdTotal amProdTotal = PXSelect<AMProdTotal,
                    Where<AMProdTotal.orderType, Equal<Required<AMProdTotal.orderType>>,
                        And<AMProdTotal.prodOrdID, Equal<Required<AMProdTotal.prodOrdID>>>
                    >>.Select(graph, productionItem.OrderType, productionItem.ProdOrdID);

                //Specifics from the change in Status
                switch (newStatusID)
                {
                    case ProductionOrderStatus.Planned:
                        productionItem.RelDate = null;
                        newProdEvent = ProductionEventHelper.BuildStatusEvent(productionItem, productionItem.StatusID, newStatusID);
                        ProductionTransactionHelper.UpdatePlannedProductionTotals(graph, productionItem, amProdTotal);
                        break;
                    case ProductionOrderStatus.Released:

                        if(productionItem.DetailSource == ProductionDetailSource.Configuration)
                        {
                            // If order is marked as a configuration - the configuration must be complete before releasing
                            AMConfigurationResults config = PXSelect<AMConfigurationResults,
                                Where<AMConfigurationResults.prodOrderType, Equal<Required<AMProdItem.orderType>>,
                                    And<AMConfigurationResults.prodOrderNbr, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(graph, productionItem.OrderType, productionItem.ProdOrdID);
                            if (config == null || !config.Completed.GetValueOrDefault())
                            {
                                throw new PXException(Messages.ProdConfigNotFinish, productionItem.OrderType.TrimIfNotNullEmpty(), productionItem.ProdOrdID.TrimIfNotNullEmpty());
                            }
                        }

                        productionItem.RelDate = graph.Accessinfo.BusinessDate;
                        newProdEvent = ProductionEventHelper.BuildStatusEvent(productionItem, productionItem.StatusID, newStatusID);
                        BOMCostRoll.UpdatePlannedMaterialCosts(graph, productionItem);
                        ProductionTransactionHelper.UpdatePlannedProductionTotals(graph, productionItem, amProdTotal);
                        break;
                    case ProductionOrderStatus.Completed:
                        newProdEvent = ProductionEventHelper.BuildStatusEvent(productionItem, productionItem.StatusID, newStatusID);
                        break;
                    case ProductionOrderStatus.Hold:
                        newProdEvent = ProductionEventHelper.BuildStatusEvent(productionItem, productionItem.StatusID, newStatusID);
                        break;
                    case ProductionOrderStatus.Cancel:
                        newStatusRequiresWIPVar = true;
                        ProductionTransactionHelper.UpdateProdOperActualCostTotals(graph, productionItem);
                        productionItem.PerClose = financialPeriod.FinancialPeriodID;
                        newProdEvent = ProductionEventHelper.BuildStatusEvent(productionItem, productionItem.StatusID, newStatusID);
                        break;
                    case ProductionOrderStatus.Closed:
                        newStatusRequiresWIPVar = true;
                        ProductionTransactionHelper.UpdateProdOperActualCostTotals(graph, productionItem);
                        productionItem.PerClose = financialPeriod.FinancialPeriodID;
                        newProdEvent = ProductionEventHelper.BuildStatusEvent(productionItem, productionItem.StatusID, newStatusID);
                        break;
                }

                //Change prod item status and handle specific status changes
                productionItem.StatusID = newStatusID;
                productionItem.TranDate = graph.Accessinfo.BusinessDate;

                productionItem = (AMProdItem)graph.Caches[typeof(AMProdItem)].Update(productionItem);

                foreach (AMProdOper operation in PXSelectReadonly<AMProdOper,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
                >.Select(graph, productionItem.OrderType, productionItem.ProdOrdID))
                {
                    var productionOperation = graph.Caches[typeof(AMProdOper)].LocateElse(operation);

                    //Change operation status and then handle specific status changes
                    productionOperation.StatusID = newStatusID;
                    switch (newStatusID)
                    {
                        case ProductionOrderStatus.Planned:
                            break;
                        case ProductionOrderStatus.Released:
                            if (productionItem.FirstOperationID == productionOperation.OperationID)
                            {
                                productionOperation.QtytoProd = productionItem.QtytoProd;
                                productionOperation.BaseQtytoProd = productionItem.BaseQtytoProd;
                            }
                            break;
                        case ProductionOrderStatus.InProcess:
                            break;
                        case ProductionOrderStatus.Completed:
                            productionOperation.ActEndDate = graph.Accessinfo.BusinessDate;
                            break;
                    }

                    graph.Caches[typeof(AMProdOper)].Current = graph.Caches[typeof(AMProdOper)].Update(productionOperation);
                }

                //HANDLE EACH MATERIAL 
                foreach (AMProdMatl productionMaterial in PXSelect<AMProdMatl, 
                    Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                        And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>>
                        >.Select(graph, productionItem.OrderType, productionItem.ProdOrdID))
                {
                    var cachedMatl = graph.Caches[typeof(AMProdMatl)].LocateElse(productionMaterial);
                    if (cachedMatl == null)
                    {
                        continue;
                    }
                    cachedMatl.StatusID = newStatusID;
                    graph.Caches[typeof(AMProdMatl)].Update(cachedMatl);
                }

                if (newStatusRequiresWIPVar && wipAdjustmentEntry != null && productionItem.WIPBalance.GetValueOrDefault() != 0)
                {
                    wipAdjustmentEntry.ampsetup.Current.RequireControlTotal = false;

                    if (wipAdjustmentEntry.batch.Current == null)
                    {
                        var aAMBatch = wipAdjustmentEntry.batch.Insert();
                        aAMBatch.Hold = false;
                        aAMBatch.FinPeriodID = financialPeriod.FinancialPeriodID;
                        aAMBatch.TranDesc = AMTranType.GetTranDescription(AMTranType.WIPvariance);
                        wipAdjustmentEntry.batch.Update(aAMBatch);
                    }

                    var ammTranWipVar = wipAdjustmentEntry.transactions.Insert();
                    ammTranWipVar.OrderType = productionItem.OrderType;
                    ammTranWipVar.ProdOrdID = productionItem.ProdOrdID;

                    ammTranWipVar.OperationID = productionItem.LastOperationID;
                    ammTranWipVar.TranAmt = productionItem.WIPBalance * -1;
                    ammTranWipVar.TranType = AMTranType.WIPvariance;

                    ammTranWipVar.TranDesc = $"{AMTranType.GetTranDescription(AMTranType.WIPvariance)} - {productionItem.ProdOrdID}";
                    if (productionItem.StatusID == ProductionOrderStatus.Cancel
                        && !string.IsNullOrWhiteSpace(ProductionOrderStatus.GetStatusDescription(productionItem.StatusID)))
                    {
                        ammTranWipVar.TranDesc = Messages.GetLocal(Messages.StatusChangedTo, ammTranWipVar.TranDesc, ProductionOrderStatus.GetStatusDescription(productionItem.StatusID));
                    }
                    ammTranWipVar.FinPeriodID = financialPeriod.FinancialPeriodID;
                    ammTranWipVar.WIPAcctID = productionItem.WIPAcctID;
                    ammTranWipVar.WIPSubID = productionItem.WIPSubID;
                    ammTranWipVar.AcctID = productionItem.WIPVarianceAcctID;
                    ammTranWipVar.SubID = productionItem.WIPVarianceSubID;
                    wipAdjustmentEntry.transactions.Update(ammTranWipVar);
                }

                foreach (AMProdItemSplit prodItemSplit in PXSelect<AMProdItemSplit,
                     Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
                        And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>>>>
                        .Select(graph, productionItem.OrderType, productionItem.ProdOrdID))
                {
                    prodItemSplit.StatusID = productionItem.StatusID;
                    graph.Caches[typeof(AMProdItemSplit)].Update(prodItemSplit);
                }

                PXTrace.WriteInformation(Messages.GetLocal(Messages.ProductionOrderStatusUpdatedTo,
                    productionItem.OrderType.TrimIfNotNullEmpty(),
                    productionItem.ProdOrdID.TrimIfNotNullEmpty(),
                    ProductionOrderStatus.GetStatusDescription(productionItem.StatusID)));

                if (newStatusID == ProductionOrderStatus.Cancel || newStatusID == ProductionOrderStatus.Closed)
                {
                    ProductionScheduleEngine.DeleteOrderSchedule(productionItem);
                    // Delete existing Material Splits for closed or canceled order to remove ItemPlan records
                    DeleteMaterialSplits(graph, productionItem);
                }

                // Need production event insert after the update of all the changes to Prod item in cache otherwise loosing linecntr for event LineNbr
                if (newProdEvent != null && !string.IsNullOrWhiteSpace(newProdEvent.ProdOrdID))
                {
                    if (graph.Caches[typeof(AMProdItem)].Current == null)
                    {
                        graph.Caches[typeof(AMProdItem)].Current = productionItem;
                    }
                    graph.Caches[typeof(AMProdEvnt)].Insert(newProdEvent);
                }
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw new PXException(Messages.GetLocal(Messages.ProductionOrderStatusChangeError,
                    productionItem.OrderType.TrimIfNotNullEmpty(),
                    productionItem.ProdOrdID.TrimIfNotNullEmpty(),
                    ProductionOrderStatus.GetStatusDescription(origStatusID),
                    ProductionOrderStatus.GetStatusDescription(newStatusID),
                    e.Message));
            }
        }

        /// <summary>
        /// Check the current status of a production order. If invalid PXExceptions are 
        /// thrown with correct messages. Passing production order in DAC AMProdItem.
        /// </summary>
        /// <param name="aAMProdItem">AMProdItem DAC</param>
        /// <param name="PlanStatusValid">Is a plan status a valid status for transactions</param>
        public static void VerifyStatus(AMProdItem aAMProdItem, bool PlanStatusValid)
        {
            if (aAMProdItem != null)
            {
                string vpsDescription = ValidStatusDescription(aAMProdItem, PlanStatusValid);

                if (!string.IsNullOrEmpty(vpsDescription))
                {
                    //Invalid status found
                    throw new PXException(Messages.GetLocal(Messages.ProdStatusInvalidForProcess, aAMProdItem.OrderType.TrimIfNotNullEmpty(), aAMProdItem.ProdOrdID.TrimIfNotNullEmpty(), vpsDescription));
                }
            }
        }

        /// <summary>
        /// Check the current status of a production order. If invalid PXExceptions are 
        /// thrown with correct messages. Passing production number as a string.
        /// </summary>
        /// <param name="graph">a PXGraph</param>
        /// <param name="orderType"></param>
        /// <param name="productionOrderNbr">Production order number string</param>
        /// <param name="planStatusValid">Is a plan status a valid status for transactions</param>
        public static void VerifyStatus(PXGraph graph, string orderType, string productionOrderNbr, bool planStatusValid)
        {
            if (!string.IsNullOrWhiteSpace(orderType) && !string.IsNullOrEmpty(productionOrderNbr))
            {
                AMProdItem api = PXSelect<AMProdItem,
                    Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                        And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>
                    >.Select(graph, orderType, productionOrderNbr);

                VerifyStatus(api, planStatusValid);
            }
        }

        #region valid production status description (return status description if invalid)
        /// <summary>
        /// Public used within Manufacturing Shared for other verify production status methods
        /// </summary>
        /// <param name="aAMProdItem">AMProdItem DAC</param>
        /// <param name="PlanStatusValid">Is a plan status a valid status for transactions</param>
        /// <returns>If string not empty it will be the full status description that is an invalid status</returns>
        public static string ValidStatusDescription(AMProdItem aAMProdItem, bool PlanStatusValid)
        {
            var curProdStatusDescription = string.Empty;
            var validOrderTypes = "RIM";

            if (PlanStatusValid)
            {
                validOrderTypes += "P";
            }

            //ProdStatusInvalidForTransaction
            if (aAMProdItem != null)
            {
                if (aAMProdItem.Hold == true)
                {   //On Hold
                    curProdStatusDescription = Messages.Hold;
                }
                else
                {   //If NOT Released; In Process; Completed (M)
                    if (!(validOrderTypes.Contains(aAMProdItem.StatusID.Trim())))
                    {
                        curProdStatusDescription = ProductionOrderStatus.GetStatusDescription(aAMProdItem.StatusID.Trim());
                    }
                }
            }
            else
            {
                //Should never be null unless it was deleted
                curProdStatusDescription = Messages.Deleted;
            }

            return curProdStatusDescription;
        }
        #endregion

        /// <summary>
        /// Indicates if the change of status is allowed by a user
        /// </summary>
        /// <returns>True if the change is allowed</returns>
        public static bool UserChangeOfStatusValid(AMProdItem amProdItem, string toStatus)
        {
            return amProdItem != null
                && !amProdItem.Hold.GetValueOrDefault()
                && UserChangeOfStatusValid(amProdItem.StatusID, toStatus);
        }

        /// <summary>
        /// Indicates if the change of status is allowed by a user
        /// </summary>
        /// <returns>True if the change is allowed</returns>
        public static bool UserChangeOfStatusValid(string fromStatus, string toStatus)
        {
            if (string.IsNullOrWhiteSpace(fromStatus) || string.IsNullOrWhiteSpace(toStatus))
            {
                return false;
            }

            switch (fromStatus)
            {
                case ProductionOrderStatus.Planned:
                    return toStatus == ProductionOrderStatus.Released
                        || toStatus == ProductionOrderStatus.Hold;

                case ProductionOrderStatus.Released:
                    return toStatus == ProductionOrderStatus.Planned
                        || toStatus == ProductionOrderStatus.InProcess
                        || toStatus == ProductionOrderStatus.Hold
                        || toStatus == ProductionOrderStatus.Cancel;

                case ProductionOrderStatus.InProcess:
                    return toStatus == ProductionOrderStatus.Hold
                        || toStatus == ProductionOrderStatus.Cancel
                        || toStatus == ProductionOrderStatus.Completed;

                case ProductionOrderStatus.Hold:
                    return toStatus == ProductionOrderStatus.Planned
                        || toStatus == ProductionOrderStatus.Released
                        || toStatus == ProductionOrderStatus.InProcess
                        || toStatus == ProductionOrderStatus.Cancel
                        || toStatus == ProductionOrderStatus.Completed
                        || toStatus == ProductionOrderStatus.Closed;

                case ProductionOrderStatus.Cancel:
                    return false;

                case ProductionOrderStatus.Completed:
                    return toStatus == ProductionOrderStatus.InProcess
                        || toStatus == ProductionOrderStatus.Closed;

                case ProductionOrderStatus.Closed:
                    return false;
            }

            return false;
        }

        public static void DeleteMaterialSplits(PXGraph graph, AMProdItem aMProdItem)
        {
            foreach (AMProdMatlSplit prodMatlSplit in PXSelect<AMProdMatlSplit,
                     Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>,
                        And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>
                        >>>.Select(graph, aMProdItem.OrderType, aMProdItem.ProdOrdID))
            {
                if (prodMatlSplit == null)
                {
                    continue;
                }
                
                graph.Caches[typeof(AMProdMatlSplit)].Delete(prodMatlSplit);
            }
        }
    }
}
