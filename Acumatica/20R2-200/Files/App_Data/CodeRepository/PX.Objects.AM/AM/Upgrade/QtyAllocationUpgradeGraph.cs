using System;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Upgrade
{
    [PXHidden]
    public class QtyAllocationUpgradeGraph : PXGraph<QtyAllocationUpgradeGraph>
    {
        public PXSelect<AMProdItem> ProdItems;

        public PXSelect<
            AMProdItemSplit, 
            Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>, 
                And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> 
            ProdItemSplits;

        public PXSelect<
            AMProdOper, 
            Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>, 
                And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> 
            ProdOpers;

        public PXSelect<
            AMProdMatl, 
            Where<AMProdMatl.orderType, Equal<Current<AMProdItem.orderType>>, 
                And<AMProdMatl.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, 
                And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>> 
            ProdMatls;

        public PXSelect<
            AMProdMatlSplit,
            Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
                And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
                And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>> 
            ProdMatlSplits;

        public override int Persist(Type cacheType, PXDBOperation operation)
        {
            try
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}");
#endif
                return base.Persist(cacheType, operation);
            }
            catch (Exception e)
            {
                PXTrace.WriteInformation($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; Error: {e.Message}");
                throw;
            }
        }

        // Allow blank due to old records potentially containing blanks.
        [WorkCenterIDField]
        [PXDefault(typeof(Search<AMBSetup.wcID>), PersistingCheck = PXPersistingCheck.Null)]
        public void AMProdOper_WcID_CacheAttached(PXCache sender)
        {
            // Changing PersistingCheck from NullorBlank to just Null
        }

        // For performance as this graph is only used during upgrades
        [PXRemoveBaseAttribute(typeof(PXCheckUnique))]
        protected virtual void AMProdOper_OperationCD_CacheAttached(PXCache sender) { }

        [PXDBInt]
        [PXUIField(DisplayName = "Prod Item Project")]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public void AMProdItem_ProjectID_CacheAttached(PXCache sender)
        {
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public void AMProdItem_UpdateProject_CacheAttached(PXCache sender)
        {
        }

        [RevisionIDField(DisplayName = "BOM Revision")]
        public void AMProdItem_BOMRevisionID_CacheAttached(PXCache sender)
        {
        }

        // overrides to InventoryID, SubItemID to avoid errors around PXRestrictors - Ex: Item is inactive
        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID")]
        protected virtual void AMProdItem_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID")]
        protected virtual void AMProdItemSplit_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID")]
        protected virtual void AMProdMatl_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID")]
        protected virtual void AMProdMatlSplit_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
        protected virtual void AMProdItem_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
        protected virtual void AMProdItemSplit_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
        protected virtual void AMProdMatl_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
        protected virtual void AMProdMatlSplit_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE")]
        protected virtual void AMProdItem_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE")]
        protected virtual void AMProdItemSplit_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE")]
        protected virtual void AMProdMatl_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE")]
        protected virtual void AMProdMatlSplit_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Location", FieldClass = "INLOCATION")]
        protected virtual void AMProdItem_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Location", FieldClass = "INLOCATION")]
        protected virtual void AMProdItemSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Location", FieldClass = "INLOCATION")]
        protected virtual void AMProdMatl_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Location", FieldClass = "INLOCATION")]
        protected virtual void AMProdMatlSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMProdItemSplitPlanIDUpgrade(typeof(AMProdItem.noteID), typeof(AMProdItem.hold))]
        protected virtual void AMProdItemSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMProdMatlSplitPlanIDUpgrade(typeof(AMProdMatl.noteID), typeof(AMProdItem.hold))]
        protected virtual void AMProdMatlSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXBool]
        [PXUIField(DisplayName = "Stock Item", Enabled = false, Visible = false)]
        protected virtual void AMProdMatl_IsStockItem_CacheAttached(PXCache sender)
        {
            //Remove unbound formula on InventoryItem.StkItem
        }

        [PXDBDate(DisplayMask = "d", InputMask = "d")]
        [PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial")]
        protected virtual void AMProdItem_ExpireDate_CacheAttached(PXCache sender)
        {
            // Remove INExpireDateAttribute
        }

        [PXDBDate(DisplayMask = "d", InputMask = "d")]
        [PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial")]
        protected virtual void AMProdItemSplit_ExpireDate_CacheAttached(PXCache sender)
        {
            // Remove INExpireDateAttribute
        }

        [PXDBDate(DisplayMask = "d", InputMask = "d")]
        [PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial")]
        protected virtual void AMProdMatl_ExpireDate_CacheAttached(PXCache sender)
        {
            // Remove INExpireDateAttribute
        }

        [PXDBDate(DisplayMask = "d", InputMask = "d")]
        [PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial")]
        protected virtual void AMProdMatlSplit_ExpireDate_CacheAttached(PXCache sender)
        {
            // Remove INExpireDateAttribute
        }

        protected static IEnumerable<AMProdItem> GetProdItemsWithoutSplits(PXGraph graph)
        {
            return PXSelectJoin<
                AMProdItem,
                LeftJoin<AMProdItemSplit, 
                    On<AMProdItem.orderType, Equal<AMProdItemSplit.orderType>,
                    And<AMProdItem.prodOrdID, Equal<AMProdItemSplit.prodOrdID>>>>,
                Where<AMProdItem.statusID, NotEqual<ProductionOrderStatus.cancel>,
                    And<AMProdItem.statusID, NotEqual<ProductionOrderStatus.closed>,
                    And<AMProdItemSplit.prodOrdID, IsNull>>>>
                .Select(graph)
                .ToFirstTable();
        }

        public void SyncProdItemSplits()
        {
            foreach (var prodItem in GetProdItemsWithoutSplits(this))
            {
                var prodItem2 = ProdItems.Cache.LocateElse(prodItem);
                if (prodItem2 == null)
                {
                    continue;
                }

                //Convert
                var split = (AMProdItemSplit)prodItem2;
                if (split == null)
                {
                    continue;
                }
                ProdItemSplits.Insert(split);
            }
        }

        protected static PXResultset<AMProdMatl> GetProdMaterialWithoutSplits(PXGraph graph)
        {
            return PXSelectJoin<
                AMProdMatl,
                InnerJoin<InventoryItem, 
                    On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>, 
                InnerJoin<AMProdItemStatus, 
                    On<AMProdMatl.orderType, Equal<AMProdItemStatus.orderType>,
                    And<AMProdMatl.prodOrdID, Equal<AMProdItemStatus.prodOrdID>>>,
                LeftJoin <AMProdMatlSplit,
                    On<AMProdMatl.orderType, Equal<AMProdMatlSplit.orderType>,
                    And<AMProdMatl.prodOrdID, Equal<AMProdMatlSplit.prodOrdID>,
                    And<AMProdMatl.operationID, Equal<AMProdMatlSplit.operationID>,
                    And<AMProdMatl.lineID, Equal<AMProdMatlSplit.lineID>>>>>>>>,
                Where<AMProdMatl.statusID, NotEqual<ProductionOrderStatus.cancel>,
                    And<AMProdMatl.statusID, NotEqual<ProductionOrderStatus.closed>,
                    And<AMProdMatlSplit.prodOrdID, IsNull>>>>
                .Select(graph);
        }

        public void SyncProdMatlSplits(out int updatesSkipped)
        {
            updatesSkipped = 0;
            foreach (PXResult<AMProdMatl, InventoryItem, AMProdItemStatus, AMProdMatlSplit> result in GetProdMaterialWithoutSplits(this))
            {
                var prodMatl = ProdMatls.Cache.LocateElse((AMProdMatl) result);
                if (prodMatl?.SiteID == null)
                {
                    //If no site at this point its a bad record - leave it be
                    continue;
                }

                var inventoryItem = (InventoryItem)result;
                if (inventoryItem == null)
                {
                    continue;
                }

                if (prodMatl.SubItemID == null && inventoryItem.DefaultSubItemID != null)
                {
                    prodMatl.SubItemID = inventoryItem.DefaultSubItemID;
                    prodMatl = ProdMatls.Update(prodMatl);
                }

                if (prodMatl.SubItemID == null)
                {
                    updatesSkipped++;
                    continue;
                }

                if (!UomHelper.HasValidUOM<AMProdMatl.uOM>(ProdMatls.Cache, prodMatl, prodMatl.UOM))
                {
                    var uomError = Messages.GetLocal(PX.Objects.IN.Messages.MissingUnitConversionVerbose, prodMatl.UOM);

                    AMProdOper prodOper = PXSelect<AMProdOper,
                        Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                            And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                                And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>
                    >.Select(this, prodMatl.OrderType, prodMatl.ProdOrdID, prodMatl.OperationID);

                    PXTrace.WriteWarning(Messages.GetLocal(Messages.ErrorProcessingProdMaterial,
                        inventoryItem.InventoryCD.TrimIfNotNullEmpty(), prodMatl.OrderType, prodMatl.ProdOrdID,
                        prodOper?.OperationCD ?? $"[{prodMatl.OperationID}]", prodMatl.LineID, uomError));
                    updatesSkipped++;
                    continue;
                }

                // checks for null or 1/1/1900 date
                if (Common.Dates.IsDateNull(prodMatl.TranDate))
                {
                    var prodItem = (AMProdItemStatus)result;
                    prodMatl.TranDate = prodItem?.ProdDate ?? Common.Dates.BeginOfTimeDate;
                }

                if (string.IsNullOrWhiteSpace(prodMatl.TranType))
                {
                    var tranType = PXFormulaAttribute.Evaluate<AMProdMatl.tranType>(ProdMatls.Cache, prodMatl);
                    if (tranType != null)
                    {
                        prodMatl.TranType = Convert.ToString(tranType);
                        prodMatl.InvtMult = INTranType.InvtMult(prodMatl.TranType);
                    }
                }

                //To make sure the order is in cache to avoid a select trip to the database later
                var updatedProdMatl = ProdMatls.Update(prodMatl);
                if (updatedProdMatl == null)
                {
                    updatesSkipped++;
                    continue;
                }

                //Convert
                var split = (AMProdMatlSplit)updatedProdMatl;
                if (split == null)
                {
                    updatesSkipped++;
                    continue;
                }

                split.IsStockItem = inventoryItem.StkItem == true;
                //Allocations is stored in base units...
                split.UOM = inventoryItem.BaseUnit;
                split.Qty = split.BaseQty.GetValueOrDefault();

                ProdMatlSplits.Insert(split);
            }
        }
    }

    public class AMProdItemSplitPlanIDUpgradeAttribute : AMProdItemSplitPlanIDAttribute
    {
        public AMProdItemSplitPlanIDUpgradeAttribute(Type ParentNoteID, Type ParentHoldEntry) : base(ParentNoteID, ParentHoldEntry)
        {
        }

        protected override AMProdItem GetParentProdItem(PXCache sender, AMProdItemSplit split)
        {
            // Avoids hitting the database if we dont need to - use what is in cache
            var parentInCache = (AMProdItem) sender.Graph.Caches<AMProdItem>().Locate(new AMProdItem
            {
                OrderType = split?.OrderType,
                ProdOrdID = split?.ProdOrdID
            });
            return parentInCache ?? base.GetParentProdItem(sender, split);
        }
    }

    public class AMProdMatlSplitPlanIDUpgradeAttribute : AMProdMatlSplitPlanIDAttribute
    {
        public AMProdMatlSplitPlanIDUpgradeAttribute(Type ParentNoteID, Type ParentHoldEntry) : base(ParentNoteID, ParentHoldEntry)
        {
        }

        protected override AMProdItem GetParentProdItem(PXCache sender, AMProdMatlSplit split)
        {
            // Avoids hitting the database if we dont need to - use what is in cache
            var parentInCache = (AMProdItem)sender.Graph.Caches<AMProdItem>().Locate(new AMProdItem
            {
                OrderType = split?.OrderType,
                ProdOrdID = split?.ProdOrdID
            });
            return parentInCache ?? base.GetParentProdItem(sender, split);
        }

        protected override AMProdMatl GetParentProdMatl(PXCache sender, AMProdMatlSplit split)
        {
            // Avoids hitting the database if we dont need to - use what is in cache
            var parentInCache = (AMProdMatl)sender.Graph.Caches<AMProdMatl>().Locate(new AMProdMatl
            {
                OrderType = split?.OrderType,
                ProdOrdID = split?.ProdOrdID,
                OperationID = split?.OperationID,
                LineID = split?.LineID
            });
            return parentInCache ?? base.GetParentProdMatl(sender, split);
        }
    }
}