namespace PX.Objects.AM
{
    public static class DACExtensionMethods
    {
        public static bool OperationsEqual(this IProdOper oper1, IProdOper oper2)
        {
            return OperationsEqual(oper1, oper2?.OrderType, oper2?.ProdOrdID, oper2?.OperationID);
        }

        public static bool OperationsEqual(this IProdOper oper, string orderType, string prodOrdID, int? operationId)
        {
            return oper != null && 
                   oper.OrderType.EqualsWithTrim(orderType) &&
                   oper.ProdOrdID.EqualsWithTrim(prodOrdID) &&
                   oper.OperationID.GetValueOrDefault() == operationId.GetValueOrDefault();
        }

        public static decimal GetMachineUnitsPerHour(this IOperationMaster oper)
        {
            if (oper == null)
            {
                return 0m;
            }

            return oper.MachineUnitTime.GetValueOrDefault() == 0 ? 0m
                : UomHelper.Round(60m * oper.MachineUnits.GetValueOrDefault() / oper.MachineUnitTime.GetValueOrDefault(), 10);
        }

        public static decimal GetRunUnitsPerHour(this IOperationMaster oper)
        {
            if (oper == null)
            {
                return 0m;
            }

            return oper.RunUnitTime.GetValueOrDefault() == 0 ? 0m
                : UomHelper.Round(60m * oper.RunUnits.GetValueOrDefault() / oper.RunUnitTime.GetValueOrDefault(), 10);
        }

        /// <summary>
        /// Combine the key fields into a single string.
        /// </summary>
        public static string JoinKeys(this AMProdOper prodOper)
        {
            return prodOper == null
                ? string.Empty
                : string.Join("~", prodOper.OrderType.TrimIfNotNullEmpty(), prodOper.ProdOrdID.TrimIfNotNullEmpty(), prodOper.OperationID);
        }

        #region AMMTran

        /// <summary>
        /// Does the given <see cref="AMMTran"/> contain a reference to the given <see cref="AMProdMatl"/>
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="prodMatl"></param>
        /// <returns></returns>
        public static bool IsSameMatl(this AMMTran tran, AMProdMatl prodMatl)
        {
            return tran?.MatlLineId != null && prodMatl?.LineID != null && tran.OrderType == prodMatl.OrderType &&
                   tran.ProdOrdID == prodMatl.ProdOrdID && tran.OperationID == prodMatl.OperationID &&
                   tran.MatlLineId == prodMatl.LineID;
        }

        /// <summary>
        /// Is this transaction the original to the given transaction
        /// </summary>
        public static bool IsTransactionOriginal(this AMMTran tran1, AMMTran tran2)
        {
            return tran1 != null && tran2 != null &&
                   tran1.DocType == tran2.OrigDocType &&
                   tran1.BatNbr == tran2.OrigBatNbr &&
                   tran1.LineNbr == tran2.OrigLineNbr;
        }

#if DEBUG
        // do not use JoinKeys for AMMTran because that is an instance method name used on AMMTran that we are making obsolete 
#endif
        /// <summary>
        /// Combine the key fields into a single string.
        /// </summary>
        public static string JoinDacKeys(this AMMTran row)
        {
            return row == null
                ? string.Empty
                : string.Join("~", row.DocType.TrimIfNotNullEmpty(), row.BatNbr.TrimIfNotNullEmpty(), row.LineNbr);
        }

        #endregion

        #region AMProdMatl

        /// <summary>
        /// Gets the total required qty for a given order qty with a rounded value by the decimal places configured for the branch/company
        /// </summary>
        public static decimal GetTotalReqQtyCompanyRounded(this AMProdMatl prodMatl, decimal totalOrderQty)
        {
            return UomHelper.QuantityRound(GetTotalReqQty(prodMatl, totalOrderQty));
        }

        /// <summary>
        /// Gets the total required qty for a given order qty
        /// </summary>
        public static decimal GetTotalReqQty(this AMProdMatl prodMatl, decimal totalOrderQty)
        {
            return GetTotalReqQty(prodMatl, totalOrderQty, prodMatl?.QtyRoundUp == true);
        }

        /// <summary>
        /// Gets the total required qty for a given order qty
        /// </summary>
        public static decimal GetTotalReqQty(this AMProdMatl prodMatl, decimal totalOrderQty, bool roundUp)
        {
            return AMProdMatl.GetTotalRequiredQty(prodMatl, totalOrderQty, roundUp);
        }

        /// <summary>
        /// Gets the total base required qty for a given order qty
        /// </summary>
        public static decimal GetTotalBaseReqQty(this AMProdMatl prodMatl, decimal totalOrderQty)
        {
            return GetTotalBaseReqQty(prodMatl, totalOrderQty, prodMatl?.QtyRoundUp == true);
        }

        /// <summary>
        /// Gets the total base required qty for a given order qty
        /// </summary>
        public static decimal GetTotalBaseReqQty(this AMProdMatl prodMatl, decimal totalOrderQty, bool roundUp)
        {
            return AMProdMatl.GetTotalBaseRequiredQty(prodMatl, totalOrderQty, roundUp);
        }

        #endregion

        /// <summary>
        /// Map extension fields from InventoryItemExt to INItemSiteExt
        /// </summary>
        public static void MapAMExtensionFields(this CacheExtensions.INItemSiteExt toExtension, CacheExtensions.InventoryItemExt fromExtension)
        {
            if (fromExtension == null || toExtension == null)
            {
                return;
            }

            toExtension.AMMinOrdQty = fromExtension.AMMinOrdQty.GetValueOrDefault();
            toExtension.AMMaxOrdQty = fromExtension.AMMaxOrdQty.GetValueOrDefault();
            toExtension.AMLotSize = fromExtension.AMLotSize.GetValueOrDefault();
            toExtension.AMMFGLeadTime = fromExtension.AMMFGLeadTime.GetValueOrDefault();
            toExtension.AMGroupWindow = fromExtension.AMGroupWindow.GetValueOrDefault();

            // Unbound fields - will set correct bound field when used with the INItemSite graph
            toExtension.AMReplenishmentSource = fromExtension.AMReplenishmentSource;
            toExtension.AMSafetyStock = fromExtension.AMSafetyStock.GetValueOrDefault();
            toExtension.AMMinQty = fromExtension.AMMinQty.GetValueOrDefault();

            // Scrap Warehouse and Location
            toExtension.AMScrapSiteID = fromExtension.AMScrapSiteID;
            toExtension.AMScrapLocationID = fromExtension.AMScrapLocationID;
        }
    }
}