using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing <see cref="INPlanConstants"/> helper class
    /// </summary>
    public static class INPlanTypeHelper
    {
        public static bool IsAllocated(string planType)
        {
            return INPlanConstants.IsAllocated(planType);
        }

        public static bool MrpExcluded(string planType)
        {
            return planType == INPlanConstants.Plan6D       //  Drop Ship
                   || planType == INPlanConstants.Plan6E    //  Drop Ship Blanket
                   || planType == INPlanConstants.Plan71    //  PO Receipt
                   || planType == INPlanConstants.Plan74    //  Drop Ship for Sales Order
                   || planType == INPlanConstants.Plan75    //  Drop Ship for Sales Order Receipt
                   || planType == INPlanConstants.Plan79    //  Drop Ship for Sales Order Prepared
                   || planType == INPlanConstants.Plan7B    //  PO Blanket
                   || planType == INPlanConstants.Plan90    //  IN Replenishment
                   || planType == INPlanConstants.Plan93    //  SO Intransit
                   || planType == INPlanConstants.Plan94    //  PO Intransit
                   || planType == INPlanConstants.Plan95    //  PO Complete
                   || planType == INPlanConstants.Plan96    //  <not listed in DB table>
                   || planType == INPlanConstants.Plan40    //  IN Transfer-1step
                   || planType == INPlanConstants.Plan41    //  IN Transfer-2step
                   || planType == INPlanConstants.Plan43;   //  IN Transfer-Receipt
        }

        public static bool IsSalesOrder(string planType)
        {
            return planType == INPlanConstants.Plan60       //  Sales Order
                   || planType == INPlanConstants.Plan61    //  SO Allocated
                   || planType == INPlanConstants.Plan63    //  SO Allocated
                   || planType == INPlanConstants.Plan64    //  Pre-Allocated
                   || planType == INPlanConstants.Plan66    //  SO to Purchase
                   || planType == INPlanConstants.Plan68    //  Back Order
                   || planType == INPlanConstants.Plan69    //  Sales Prepared
                   || planType == INPlanConstants.Plan6B    //  Sales Order To Blanket
                   || planType == INPlanConstants.Plan6D    //  Drop Ship
                   || planType == INPlanConstants.Plan6E    //  Drop Ship Blanket
                   || planType == INPlanConstants.Plan6T    //  Transfer
                   || planType == INPlanConstants.PlanM0    //  In Transit to Production
                   || planType == INPlanConstants.PlanM8;   //  SO to Production
        }

        public static bool IsShipments(string planType)
        {
            return planType == INPlanConstants.Plan62;       //  Shipped
        }

        public static bool IsPurchaseReceipt(string planType)
        {
            return planType == INPlanConstants.Plan71;      //  PO Receipt
        }

        public static bool IsPurchase(string planType)
        {
            return planType == INPlanConstants.Plan70       //  PO Order
                   || planType == INPlanConstants.Plan71    //  PO Receipt
                   || planType == INPlanConstants.Plan72    //  PO RC Return
                   || planType == INPlanConstants.Plan73    //  PO Prepare
                   || planType == INPlanConstants.Plan74    //  Drop Ship for Sales Order
                   || planType == INPlanConstants.Plan75    //  Drop Ship for Sales Order Receipt
                   || planType == INPlanConstants.Plan76    //  Purchase for Sales Order
                   || planType == INPlanConstants.Plan77    //  Receipt for SO
                   || planType == INPlanConstants.Plan78    //  Purchase for Sales Order Prepared
                   || planType == INPlanConstants.Plan79    //  Drop Ship for Sales Order Prepared
                   || planType == INPlanConstants.Plan7B    //  PO Blanket
                   || planType == INPlanConstants.PlanF7    //  Purchase for Service Order
                   || planType == INPlanConstants.PlanF8    //  Purchase for Service Order Prepared
                   || planType == INPlanConstants.PlanM3    //  Purchase for Prod. Prepared
                   || planType == INPlanConstants.PlanM4;   //  Purchase for Production
        }

        public static bool IsAssembly(string planType)
        {
            return IsAssemblyDemand(planType) || IsAssemblySupply(planType);
        }

        public static bool IsAssemblyDemand(string planType)
        {
            return planType == INPlanConstants.Plan50; //  Assembly Demand
        }

        public static bool IsAssemblySupply(string planType)
        {
            return planType == INPlanConstants.Plan51;  //  Assembly Supply
        }

        public static bool IsInventoryDemand(string planType)
        {
            return planType == INPlanConstants.Plan20       //  IN Issues
                   || planType == INPlanConstants.Plan40    //  IN Transfer-1step
                   || planType == INPlanConstants.Plan41;   //  IN Transfer-2step
        }

        public static bool IsInventorySupply(string planType)
        {
            return planType == INPlanConstants.Plan10       //  IN Receipts
                   || planType == INPlanConstants.Plan43    //  IN Transfer-Receipt
                   || planType == INPlanConstants.Plan45;   //  Receipt for SO
        }

        public static bool IsTransferSupply(string planType)
        {
            return planType == INPlanConstants.Plan42       //  IN Transfer-Intransit
                   || planType == INPlanConstants.Plan44;   //  IN Transfer-Intransit to SO
        }

        public static bool IsFieldService(string planType)
        {
            return IsFieldServiceSupply(planType) || IsFieldServiceDemand(planType);
        }

        public static bool IsFieldServiceDemand(string planType)
        {
            return planType == INPlanConstants.PlanF0 //  SrvOrd Prepared
                   || planType == INPlanConstants.PlanF1 //  SrvOrd Booked
                   || planType == INPlanConstants.PlanF2 //  SrvOrd Allocated
                   || planType == INPlanConstants.PlanF5 //  Pre-Allocated
                   || planType == INPlanConstants.PlanF6; //  FSSO to Purchase
        }

        public static bool IsFieldServiceSupply(string planType)
        {
            return planType == INPlanConstants.PlanF9; //  Receipt for FSSO
        }

        /// <summary>
        /// Is the plan type related to production supply or demand
        /// </summary>
        /// <param name="planType"></param>
        /// <returns></returns>
        public static bool IsProduction(string planType)
        {
            return IsProductionSupply(planType) || IsProductionDemand(planType);
        }

        public static bool IsProductionSupply(string planType)
        {
            return planType == INPlanConstants.PlanM1       //  Production Supply Prepared
                   || planType == INPlanConstants.PlanM2    //  Production Supply
                   || planType == INPlanConstants.PlanMB    //  Production for Prod. Prepared
                   || planType == INPlanConstants.PlanMC    //  Production for Production
                   || planType == INPlanConstants.PlanMD    //  Production for SO Prepared
                   || planType == INPlanConstants.PlanME;   //  Production for SO
        }

        public static bool IsProductionDemand(string planType)
        {
            return planType == INPlanConstants.PlanM5       //  Production Demand Prepared
                   || planType == INPlanConstants.PlanM6    //  Production Demand
                   || planType == INPlanConstants.PlanM7    //  Production Allocated
                   || planType == INPlanConstants.PlanM9    //  Production to Purchase
                   || planType == INPlanConstants.PlanMA;   //  Production to Production
        }

        /// <summary>
        /// Is the plan type of the INPlanType constants used by the Manufacturing modules
        /// </summary>
        public static bool IsMfgPlanType(string planType)
        {
            return !string.IsNullOrWhiteSpace(planType) &&
                   (planType == INPlanConstants.PlanM0       //  In Transit to Production
                    || planType == INPlanConstants.PlanM1    //  Production Supply Prepared
                    || planType == INPlanConstants.PlanM2    //  Production Supply
                    || planType == INPlanConstants.PlanM3    //  Purchase for Prod. Prepared
                    || planType == INPlanConstants.PlanM4    //  Purchase for Production
                    || planType == INPlanConstants.PlanM5    //  Production Demand Prepared
                    || planType == INPlanConstants.PlanM6    //  Production Demand
                    || planType == INPlanConstants.PlanM7    //  Production Allocated
                    || planType == INPlanConstants.PlanM8    //  SO to Production
                    || planType == INPlanConstants.PlanM9    //  Production to Purchase
                    || planType == INPlanConstants.PlanMA    //  Production to Production
                    || planType == INPlanConstants.PlanMB    //  Production for Prod. Prepared
                    || planType == INPlanConstants.PlanMC    //  Production for Production
                    || planType == INPlanConstants.PlanMD    //  Production for SO Prepared
                    || planType == INPlanConstants.PlanME);  //  Production for SO)
        }
    }
}