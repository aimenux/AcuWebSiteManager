using System;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.PO;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Manufacturing extension for purchase order entry
    /// </summary>
    public class POOrderEntryAMExtension : PXGraphExtension<POOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXHidden]
        public PXSelect<AMProdMatlSplit> ProdMatlSplits;
        [PXHidden]
        public PXSelect<AMProdOper> ProdOperRecords;

        [PXOverride]
        public virtual void Persist(Action del)
        {
            ProdDetail.ProcessPOOrder(Base, Base.Document.Current);
            del?.Invoke();
        }

        /// <summary>
        /// Saving the Last fixed demand entry for row inserted. This is used from the POCreate process.
        /// </summary>
        protected AMProdMatlSplit FillLineDemandLastSplit;

        [PXOverride]
        public virtual void FillPOLineFromDemand(POLine dest, POFixedDemand demand, string OrderType, POOrderEntry.SOLineSplit3 solineSplit, Action<POLine, POFixedDemand, string, POOrderEntry.SOLineSplit3> method)
        {
            method?.Invoke(dest, demand, OrderType, solineSplit);

            if (dest.LineType == POLineType.GoodsForManufacturing)
            {
                InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, dest.InventoryID);

                if (inventoryItem?.StkItem == false)
                {
                    dest.LineType = POLineType.NonStockForManufacturing;
                }
            }

            if (demand?.PlanID == null || 
                demand.PlanType != INPlanConstants.PlanM5 &&
                demand.PlanType != INPlanConstants.PlanM6 &&
                demand.PlanType != INPlanConstants.PlanM9)
            {
                return;
            }

            FillLineDemandLastSplit =
                            PXSelect<
                                AMProdMatlSplit,
                                Where<AMProdMatlSplit.planID, Equal<Required<AMProdMatlSplit.planID>>>>
                                .Select(Base, demand.PlanID);

            //THE PROBLEM WE HAVE HERE... dest is not inserted yet so no row number is worthless. Continue in POLine_RowInserted...
        }

        protected virtual void POLine_RowInserted(PXCache cache, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            del?.Invoke(cache, e);

            if (string.IsNullOrWhiteSpace(FillLineDemandLastSplit?.ProdOrdID))
            {
                return;
            }

            var prodMatlSplit = ProdMatlSplits.Cache.LocateElseCopy(FillLineDemandLastSplit);
            FillLineDemandLastSplit = null;

            if (string.IsNullOrWhiteSpace(prodMatlSplit?.ProdOrdID))
            {
                return;
            }

            var row = (POLine)e.Row;
            if (row?.LineNbr == null)
            {
                return;
            }

            prodMatlSplit.POOrderNbr = row.OrderNbr;
            prodMatlSplit.POOrderType = row.OrderType;
            prodMatlSplit.POLineNbr = row.LineNbr;
            prodMatlSplit.POCreate = true;
            prodMatlSplit.VendorID = row.VendorID;
            prodMatlSplit.RefNoteID = Base?.Document?.Current?.NoteID;
            prodMatlSplit = ProdMatlSplits.Update(prodMatlSplit);

            if (prodMatlSplit?.ProdOrdID == null)
            {
                return;
            }

            //Make sure new created Order Nbr set in prod matl split
            PXDBDefaultAttribute.SetDefaultForUpdate<AMProdMatlSplit.pOOrderNbr>(ProdMatlSplits.Cache, prodMatlSplit, true);

            var prodOper = ProdOperRecords.Cache.LocateElseCopy(AMProdOper.PK.Find(Base, prodMatlSplit.OrderType, prodMatlSplit.ProdOrdID, prodMatlSplit.OperationID));

            if (prodOper == null || prodOper.POOrderNbr != null || prodOper.POLineNbr != null)
            {
                return;
            }

            prodOper.POOrderNbr = row.OrderNbr;
            prodOper.POLineNbr = row.LineNbr;
            prodOper = ProdOperRecords.Update(prodOper);
                                
            //Make sure new created Order Nbr set in prod oper
            PXDBDefaultAttribute.SetDefaultForUpdate<AMProdOper.pOOrderNbr>(ProdOperRecords.Cache, prodOper, true);
        }

        //When changing the linetype the default order qty will re-fire which seems silly as the user entered order qty is cleared...
        protected virtual void POLine_OrderQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting del)
        {
            del?.Invoke(sender, e);

            var row = (POLine)e.Row;
            if (row == null || !IsMfgLineType(row.LineType))
            {
                return;
            }

            // If MFG line type, keep the current value
            e.NewValue = row.OrderQty.GetValueOrDefault();
        }

        public static bool IsMfgLineType(string lineType)
        {
            return !string.IsNullOrWhiteSpace(lineType) &&
                   (lineType == POLineType.GoodsForManufacturing || lineType == POLineType.NonStockForManufacturing);
        }

        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof(POOrder.orderNbr), DefaultForInsert = false, DefaultForUpdate = false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
        protected virtual void AMProdMatlSplit_POOrderNbr_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [POLineTypeListMfg(typeof(POLine.orderType), typeof(POLine.inventoryID))]
        protected virtual void POLine_LineType_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBLong(IsImmutable = true)]
        [PXUIField(DisplayName = "Plan ID", Visible = false, Enabled = false)]
        [INItemPlanIDSimple()]
        protected virtual void AMProdMatlSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof(POOrder.orderNbr), DefaultForInsert = false, DefaultForUpdate = false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
        protected virtual void AMProdOper_POOrderNbr_CacheAttached(PXCache sender)
        {
        }

        protected virtual void POLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            del?.Invoke(sender, e);

            var row = (POLine)e.Row;

            foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(Base, row.PlanID))
            {
                plan.SupplyPlanID = null;
                Base.itemPlans.Update(plan);
            }

            // Delete AMProdMatlSplit References to the line
            foreach (AMProdMatlSplit prodMatlSplit in PXSelect<AMProdMatlSplit,
                Where<AMProdMatlSplit.pOOrderType, Equal<Required<AMProdMatlSplit.pOOrderType>>,
                    And<AMProdMatlSplit.pOOrderNbr, Equal<Required<AMProdMatlSplit.pOOrderNbr>>,
                        And<AMProdMatlSplit.pOLineNbr, Equal<Required<AMProdMatlSplit.pOLineNbr>>
                        >>>>.Select(Base, row.OrderType, row.OrderNbr, row.LineNbr))
            {
                if (prodMatlSplit?.ProdOrdID == null)
                {
                    continue;
                }

                prodMatlSplit.POOrderType = null;
                prodMatlSplit.POOrderNbr = null;
                prodMatlSplit.POLineNbr = null;
                ProdMatlSplits.Cache.Update(prodMatlSplit);
            }
            
            // Delete AMProdOper References to the line
            AMProdOper prodOper = PXSelect<AMProdOper,
                Where<AMProdOper.pOOrderNbr, Equal<Required<AMProdOper.pOOrderNbr>>,
                    And<AMProdOper.pOLineNbr, Equal<Required<AMProdOper.pOLineNbr>>
                >>>.Select(Base, row.OrderNbr, row.LineNbr);

            if(prodOper != null)
            {
                prodOper.POOrderNbr = null;
                prodOper.POLineNbr = null;
                ProdOperRecords.Cache.Update(prodOper);
            }
        }

    }
}
