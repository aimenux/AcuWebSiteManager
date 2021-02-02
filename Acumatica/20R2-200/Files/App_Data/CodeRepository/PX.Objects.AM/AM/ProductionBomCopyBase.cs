using System;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    public abstract class ProductionBomCopyBase : PXGraph<ProductionBomCopyBase, AMProdItem>
    {
        public PXSelect<AMProdItem> ProdItemRecords;

        public PXFilter<AMOrder> Order;

        [PXHidden]
        public PXSelect<AMProdTotal, Where<AMProdTotal.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdTotal.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdTotalRecords;
        [PXHidden]
        public PXSelect<AMProdOper, Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdOperRecords;
        [PXHidden]
        public PXSelect<AMProdStep, Where<AMProdStep.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdStep.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdStepRecords;
        [PXHidden]
        public PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdMatl.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdMatlRecords;
        [PXHidden]
        public PXSelect<AMProdOvhd, Where<AMProdOvhd.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdOvhd.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdOvhdRecords;
        [PXHidden]
        public PXSelect<AMProdTool, Where<AMProdTool.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdTool.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdToolRecords;
        [PXHidden]
        public PXSelect<AMSchdOper, Where<AMSchdOper.orderType, Equal<Current<AMOrder.orderType>>, And<AMSchdOper.prodOrdID, Equal<Current<AMOrder.orderNbr>>>>> SchdOperRecords;
        [PXHidden]
        public PXSelect<AMSchdItem, Where<AMSchdItem.orderType, Equal<Current<AMOrder.orderType>>, And<AMSchdItem.prodOrdID, Equal<Current<AMOrder.orderNbr>>>>> SchdItemRecords;
        [PXHidden]
        public PXSelect<AMWCSchd> WorkCenterSchdRecs;
        [PXHidden]
        public PXSelect<AMWCSchdDetail> WorkCenterSchdDetailRecs;
        [PXHidden]
        public PXSelect<AMSchdOperDetail> SchdOperDetails;
        [PXHidden]
        public PXSelect<AMProdAttribute, Where<AMProdAttribute.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdAttribute.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProductionAttributes;

        public PXSetup<AMPSetup> Setup;

        public PXSetup<AMOrderType>.Where<AMOrderType.orderType.IsEqual<AMOrder.orderType.FromCurrent>> amOrderType;

        protected OrderedOperationDetails _operationDetails;
        [PXHidden]
        public PXSelect<AMProdItemSplit,
            Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdItemSplits;
        [PXHidden]
        public PXSelect<AMProdMatlSplit,
            Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
                    And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
                        And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>> ProdMatlSplits;

        [PXHidden]
        public PXSelect<AMBomItem> BomItem;

        private PXGraph _processingGraph;

        public PXGraph ProcessingGraph
        {
            get
            {
                if (_processingGraph == null)
                {
                    _processingGraph = this;
                }

                return _processingGraph;
            }
            set { _processingGraph = value; }
        }

        //Required to support LSSelect
        public override string PrimaryView => nameof(ProdItemRecords);

        public DateTime BusinessDate => Common.Current.BusinessDate(ProcessingGraph);

        protected enum Position
        {
            Before,
            After
        }

        protected virtual void SetCurrentProdItem(AMProdItem prodItem)
        {
            ProcessingGraph.Caches<AMProdItem>().Update(prodItem);
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMProdItemSplitPlanID(typeof(AMProdItem.noteID), typeof(AMProdItem.hold))]
        protected virtual void AMProdItemSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMProdMatlSplitPlanID(typeof(AMProdMatl.noteID), typeof(AMProdItem.hold))]
        protected virtual void AMProdMatlSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        public virtual AMProdItem CurrentProdItem => (AMProdItem)ProcessingGraph.Caches<AMProdItem>().Current;
        public virtual AMProdOper CurrentProdOper => (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Current;

        public virtual AMOrderType CurrentOrderType
        {
            get
            {
                var current = (AMOrderType) ProcessingGraph.Caches<AMOrderType>().Current;

                if (current == null)
                {
                    current =
                        (AMOrderType)
                        PXSelect<
                            AMOrderType, 
                            Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>
                            .Select(ProcessingGraph, CurrentProdItem?.OrderType);
                    ProcessingGraph.Caches<AMOrderType>().Current = current;
                }

                return current;
            }
        }

        public virtual AMProdTotal CurrentProdTotal
        {
            get
            {
                var current = (AMProdTotal)ProcessingGraph.Caches<AMProdTotal>().Current;

                if (current == null)
                {
                    current =
                        (AMProdTotal)
                        PXSelect<
                            AMProdTotal, 
                            Where<AMProdTotal.orderType, Equal<Required<AMProdTotal.orderType>>, 
                                And<AMProdTotal.prodOrdID, Equal<Required<AMProdTotal.prodOrdID>>>>>
                            .Select(ProcessingGraph, CurrentProdItem?.OrderType, CurrentProdItem?.ProdOrdID);
                    ProcessingGraph.Caches<AMProdTotal>().Current = current;
                }

                return current;
            }
        }

        public virtual AMPSetup CurrentSetup
        {
            get
            {
                var current = (AMPSetup)ProcessingGraph.Caches<AMPSetup>().Current;

                if (current == null)
                {
                    current = (AMPSetup)PXSelect<AMPSetup>.Select(ProcessingGraph);
                    ProcessingGraph.Caches<AMPSetup>().Current = current;
                }

                return current;
            }
        }

        public virtual AMSchdItem CurrentSchdItem
        {
            get => (AMSchdItem)ProcessingGraph.Caches<AMSchdItem>().Current;
            set => ProcessingGraph.Caches<AMSchdItem>().Current = value;
        }

        public virtual AMSchdOper CurrentSchdOper
        {
            get => (AMSchdOper)ProcessingGraph.Caches<AMSchdOper>().Current;
            set => ProcessingGraph.Caches<AMSchdOper>().Current = value;
        }

        public virtual AMWCSchdDetail CurrentWCSchdDetail
        {
            get => (AMWCSchdDetail)ProcessingGraph.Caches<AMWCSchdDetail>().Current;
            set => ProcessingGraph.Caches<AMWCSchdDetail>().Current = value;
        }

        public virtual AMWCSchd CurrentWCSchd
        {
            get => (AMWCSchd)ProcessingGraph.Caches<AMWCSchd>().Current;
            set => ProcessingGraph.Caches<AMWCSchd>().Current = value;
        }

        public virtual AMSchdOperDetail CurrentSchdOperDetail
        {
            get => (AMSchdOperDetail)ProcessingGraph.Caches<AMSchdOperDetail>().Current;
            set => ProcessingGraph.Caches<AMSchdOperDetail>().Current = value;
        }

        protected virtual void SetCurrentProdItemDescription()
        {
            if (CurrentProdItem == null)
            {
                return;
            }

            var copy = PXCache<AMProdItem>.CreateCopy(CurrentProdItem);

            if (copy == null || !string.IsNullOrWhiteSpace(copy.Descr))
            {
                return;
            }

            copy.Descr = GetDescription(copy);
            ProcessingGraph.Caches<AMProdItem>().Update(copy);
        }

        protected virtual void CacheLoadProdAttributes()
        {
            //Load prod source attributes to cache for use later
            PXSelect<AMProdAttribute, Where<AMProdAttribute.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMProdAttribute.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>.Select(ProcessingGraph);
        }

        protected bool ProdAttributeOperationFieldVerifyingCancelApplied = false;
        /// <summary>
        /// Special insert method to manage unique attribute labels.
        /// Determines insert or update.
        /// </summary>
        /// <param name="prodAttribute">new AMPRodAttribute row</param>
        /// <returns>True if inserted, false if excluded by some condition</returns>
        protected virtual bool TryInsertAMProdAttribute(AMProdAttribute prodAttribute)
        {
            if (prodAttribute == null
                || string.IsNullOrWhiteSpace(prodAttribute.Label))
            {
                return false;
            }

            if (!ProdAttributeOperationFieldVerifyingCancelApplied)
            {
                ProcessingGraph.FieldVerifying.AddHandler<AMProdAttribute.operationID>((sender, e) => { e.Cancel = true; });
                ProdAttributeOperationFieldVerifyingCancelApplied = true;
            }

            try
            {
                var existingAttribute = FindProdAttributeByLabel(prodAttribute);

                if (existingAttribute == null)
                {
                    ProcessingGraph.Caches<AMProdAttribute>().Insert(prodAttribute);
                    return true;
                }

                if (CanReplaceExistingProdAttribute(prodAttribute, existingAttribute))
                {
                    var status = ProcessingGraph.Caches<AMProdAttribute>().GetStatus(existingAttribute);
                    if (status == PXEntryStatus.Deleted)
                    {
                        ProcessingGraph.Caches<AMProdAttribute>().Insert(prodAttribute);
                        return true;
                    }

                    var updatedAttribute = ProductionBomCopyMap.CopyAttributes(prodAttribute, existingAttribute);
                    ProcessingGraph.Caches<AMProdAttribute>().Update(updatedAttribute);
                    return true;
                }
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
            }

            return false;
        }

        protected virtual bool CanReplaceExistingProdAttribute(AMProdAttribute newAttribute, AMProdAttribute existingAttribute)
        {
            if (newAttribute == null)
            {
                return false;
            }

            if (existingAttribute == null)
            {
                return true;
            }

            switch (newAttribute.Source)
            {
                case AMAttributeSource.Configuration:
                    return true;
                case AMAttributeSource.BOM:
                    return existingAttribute.Source != AMAttributeSource.Configuration;
                case AMAttributeSource.Production:
                    return existingAttribute.Source == AMAttributeSource.Production;
            }

            return false;
        }

        private AMProdAttribute FindProdAttributeByLabel(AMProdAttribute prodAttribute)
        {
            if (string.IsNullOrWhiteSpace(prodAttribute.Label))
            {
                return null;
            }

            foreach (AMProdAttribute cachedAttribute in ProcessingGraph.Caches<AMProdAttribute>().Inserted)
            {
                if (prodAttribute.Label.EqualsWithTrim(cachedAttribute.Label))
                {
                    return cachedAttribute;
                }
            }

            foreach (AMProdAttribute cachedAttribute in ProcessingGraph.Caches<AMProdAttribute>().Cached) // includes the attributes in the order copied from
            {
                if (prodAttribute.Label.EqualsWithTrim(cachedAttribute.Label) && prodAttribute.OrderType.Equals(cachedAttribute.OrderType) &&
                    prodAttribute.ProdOrdID.EqualsWithTrim(cachedAttribute.ProdOrdID))
                {
                    return cachedAttribute;
                }
            }

            return null;
        }

        protected virtual AMProdItem FindProdItem(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return null;
            }

            return FindProdItem(amProdItem.OrderType, amProdItem.ProdOrdID) ?? amProdItem;
        }

        public virtual AMProdItem FindProdItem(string orderType, string prodOrdID)
        {
            if (string.IsNullOrWhiteSpace(orderType) || string.IsNullOrWhiteSpace(prodOrdID))
            {
                return null;
            }

            return PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>
                    >.Select(ProcessingGraph, orderType, prodOrdID);
        }

        protected static AMOrder ConvertToOrder(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return new AMOrder();
            }

            var newOrder = new AMOrder
            {
                OrderType = amProdItem.OrderType,
                OrderNbr = amProdItem.ProdOrdID,
                InventoryID = amProdItem.InventoryID,
                SubItemID = amProdItem.SubItemID,
                OrderQty = amProdItem.BaseQtytoProd.GetValueOrDefault(),
                PlanDate = amProdItem.ConstDate ?? Common.Dates.Today,
                SiteID = amProdItem.SiteID,
                SourceDate = amProdItem.BOMEffDate ?? Common.Dates.Today
            };

            switch (amProdItem.DetailSource)
            {
                case ProductionDetailSource.Estimate:
                    newOrder.SourceID = amProdItem.EstimateID;
                    newOrder.RevisionID = amProdItem.EstimateRevisionID;
                    break;
                case ProductionDetailSource.ProductionRef:
                    newOrder.SourceID = amProdItem.SourceProductionNbr;
                    newOrder.SourceOrderType = amProdItem.SourceOrderType;
                    break;
                default:
                    //Default as BOM
                    newOrder.SourceID = amProdItem.BOMID;
                    newOrder.RevisionID = amProdItem.BOMRevisionID;
                    break;
            }

            return newOrder;
        }

        protected virtual void SetPhtmMatlReferences<Table>(ref Table prodRow, OperationDetail operationDetail) where Table : IBqlTable, IPhantomBomReference
        {
            prodRow.PhtmLevel = operationDetail.Level;
            prodRow.PhtmMatlBOMID = operationDetail.MatlBomID;
            prodRow.PhtmMatlRevisionID = operationDetail.MatlRevisionID;
            prodRow.PhtmMatlOperationID = operationDetail.MatlOperationID;
            prodRow.PhtmMatlLineRef = operationDetail.MatlLineId;
        }

        /// <summary>
        /// Calculate the operation details for use in building order details and/or scheduling
        /// </summary>
        /// <param name="order"></param>
        public virtual void CreateOperationDetail(AMOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (string.IsNullOrWhiteSpace(order.SourceID))
            {
                throw new PXArgumentException("Source ID");
            }

            if (order.SiteID == null)
            {
                throw new ArgumentException("Site ID");
            }

            if (order.OrderQty == null)
            {
                throw new ArgumentException("Order Qty");
            }

            if (Common.Dates.IsDefaultDate(order.PlanDate))
            {
                throw new ArgumentException("Order plan date");
            }

            _operationDetails = new OrderedOperationDetails();

            Order.Current = order;

            // Build list of operations accounting for phantoms
            BuildOperationDetail(order.SourceID, order.RevisionID);

            if (InventoryHelper.MultiWarehousesFeatureEnabled && CurrentOrderType.SubstituteWorkCenters.GetValueOrDefault())
            {
                SubstituteWorkCenters();
            }

            SetProductionOperNbrs();
        }

        protected virtual bool CheckProdRowInsert<TProdTable, TEstimateTable>(TProdTable prodRow, TEstimateTable estimateRow, out PXException ex) where TProdTable : IBqlTable, IProdOper where TEstimateTable : IBqlTable
        {
            ex = null;
            if (prodRow?.OrderType != null)
            {
                return false;
            }

            var rowKeys = new EntityHelper(ProcessingGraph).GetEntityRowKeys(typeof(TEstimateTable), estimateRow);
            var rowKeysMsg = rowKeys == null ? string.Empty : string.Join(",", rowKeys);
            ex = new PXException($"Unable to insert {Common.Cache.GetCacheName(typeof(TProdTable))} from {Common.Cache.GetCacheName(typeof(TEstimateTable))} row {rowKeysMsg}");
            return true;
        }

        protected virtual void DeleteProductionDetail(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return;
            }

            try
            {
                //Update the ProdTotal Caches to zero so they are correct when rebuilding details
                AMProdTotal totalRow = PXSelect<AMProdTotal,
                        Where<AMProdTotal.orderType, Equal<Required<AMProdTotal.orderType>>,
                            And<AMProdTotal.prodOrdID, Equal<Required<AMProdTotal.prodOrdID>>>>
                >.Select(ProcessingGraph, amProdItem.OrderType, amProdItem.ProdOrdID);

                //Seing these to zero to make sure the fields of AMProdTotal are correct when rebuilding details
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planLaborTime>(totalRow, 0);
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planLabor>(totalRow, 0m);
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planMachine>(totalRow, 0m);
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planMaterial>(totalRow, 0m);
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planTool>(totalRow, 0m);
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planFixedOverhead>(totalRow, 0m);
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planVariableOverhead>(totalRow, 0m);
                ProcessingGraph.Caches<AMProdTotal>().SetValue<AMProdTotal.planTotal>(totalRow, 0m);

                ProcessingGraph.Caches<AMProdTotal>().Update(totalRow);
                

                //We only need to delete the operation records - PXParent DACS of Prod Oper will  cascade delete.
                foreach (AMProdOper row in PXSelect<AMProdOper, 
                        Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>, 
                            And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
                >.Select(ProcessingGraph, amProdItem.OrderType, amProdItem.ProdOrdID))
                {
                    //Doing this crazy set of zero to make sure the parent fields of AMProdTotal are correct when rebuilding details
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planLaborTime>(row, 0);
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planLabor>(row, 0m);
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planMachine>(row, 0m);
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planMaterial>(row, 0m);
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planTool>(row, 0m);
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planFixedOverhead>(row, 0m);
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planVariableOverhead>(row, 0m);
                    ProcessingGraph.Caches<AMProdOper>().SetValue<AMProdOper.planTotal>(row, 0m);
                
                    ProcessingGraph.Caches<AMProdOper>().Delete(row);
                }

                // Delete the BOM and Configuration Attributes
                foreach (AMProdAttribute prodAttribute in PXSelect<
                    AMProdAttribute, 
                    Where<AMProdAttribute.orderType,
                Equal<Required<AMProdAttribute.orderType>>, 
                        And<AMProdAttribute.prodOrdID, Equal<Required<AMProdAttribute.prodOrdID>>>>>
                    .Select(ProcessingGraph, amProdItem.OrderType, amProdItem.ProdOrdID))
                {
                    /* For query selection on keys, restrict deleted records here */
                    if (prodAttribute?.Source == null || AMAttributeSource.IsOrderLevelAttributes(prodAttribute.Source))
                    {
                        continue;
                    }

                    // Delete the BOM Source Attribute
                    ProcessingGraph.Caches<AMProdAttribute>().Delete(prodAttribute);
                }
            }
            catch (Exception exception)
            {
                PXTraceHelper.PxTraceException(exception);
                throw;
            }
        }

        protected virtual string GetDescription(AMProdItem amProdItem)
        {
            if (amProdItem?.DetailSource == null)
            {
                return null;
            }

            switch (amProdItem.DetailSource)
            {
                case ProductionDetailSource.BOM:
                    if (!string.IsNullOrWhiteSpace(amProdItem.BOMID)
                        && !string.IsNullOrWhiteSpace(amProdItem.BOMRevisionID))
                    {
                        AMBomItem bomItem = PXSelect<AMBomItem,
                                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                                    And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                            .Select(ProcessingGraph, amProdItem.BOMID, amProdItem.BOMRevisionID);
                        return string.IsNullOrWhiteSpace(bomItem?.Descr) ? null : bomItem.Descr; //Avoid legacy fixed string spacing;
                    }
                    break;
                case ProductionDetailSource.Estimate:
                    if (!string.IsNullOrWhiteSpace(amProdItem.EstimateID)
                        && !string.IsNullOrWhiteSpace(amProdItem.EstimateRevisionID))
                    {
                        AMEstimateItem estimateItem = PXSelect<AMEstimateItem,
                            Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                            And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>
                            >>>.Select(ProcessingGraph, amProdItem.EstimateID, amProdItem.EstimateRevisionID);
                        return estimateItem?.ItemDesc;
                    }
                    break;
                case ProductionDetailSource.ProductionRef:
                    if (!string.IsNullOrWhiteSpace(amProdItem.SourceOrderType)
                        && !string.IsNullOrWhiteSpace(amProdItem.SourceProductionNbr))
                    {
                        AMProdItem prodItem = PXSelect<AMProdItem, 
                            Where< AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>, 
                            And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>
                            >>>.Select(ProcessingGraph, amProdItem.SourceOrderType, amProdItem.SourceProductionNbr);
                        return string.IsNullOrWhiteSpace(prodItem?.Descr) ? null : prodItem.Descr; //Avoid legacy fixed string spacing;
                    }
                    break;
                case ProductionDetailSource.Configuration:
                    if (!string.IsNullOrWhiteSpace(amProdItem.OrderType)
                        && !string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
                    {
                        AMConfigurationResults configResult = PXSelect<AMConfigurationResults,
                                Where<AMConfigurationResults.prodOrderType,
                                    Equal<Required<AMConfigurationResults.prodOrderType>>,
                                    And<AMConfigurationResults.prodOrderNbr,
                                        Equal<Required<AMConfigurationResults.prodOrderNbr>>>>>
                            .Select(ProcessingGraph, amProdItem.OrderType, amProdItem.ProdOrdID);
                        return configResult?.TranDescription;
                    }
                    break;
            }

            return null;
        }

        protected virtual void UpdatePlannedOperationTotals()
        {
            foreach (AMProdOper prodOper in ProcessingGraph.Caches<AMProdOper>().Cached)
            {
                var status = ProcessingGraph.Caches<AMProdOper>().GetStatus(prodOper);
                if (status != PXEntryStatus.Inserted && status != PXEntryStatus.Updated)
                {
                    continue;
                }

                ProductionTransactionHelper.UpdatePlannedOperTotal(ProcessingGraph, CurrentProdItem, prodOper);
            }
        }

        public virtual void CopyOrderTypeAttributes(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType))
            {
                return;
            }

            var orderType = CurrentOrderType;
            if (orderType == null)
            {
                return;
            }

            foreach (AMOrderTypeAttribute orderTypeAttribute in PXSelect<AMOrderTypeAttribute, 
                Where<AMOrderTypeAttribute.orderType, Equal<Required<AMOrderTypeAttribute.orderType>>
                >>.Select(ProcessingGraph, orderType.OrderType))
            {
                var newProdAttribute = new AMProdAttribute
                {
                    OrderType = CurrentProdItem?.OrderType,
                    ProdOrdID = CurrentProdItem?.ProdOrdID,
                    Level = AMAttributeLevels.Order,
                    OperationID = null,
                    Source = AMAttributeSource.OrderType,
                    AttributeID = orderTypeAttribute.AttributeID,
                    Label = orderTypeAttribute.Label,
                    Descr = orderTypeAttribute.Descr,
                    Enabled = orderTypeAttribute.Enabled,
                    TransactionRequired = orderTypeAttribute.TransactionRequired,
                    Value = orderTypeAttribute.Value
                };
                TryInsertAMProdAttribute(newProdAttribute);
            }
        }

        protected virtual void SetProductionOperNbrs()
        {
            _operationDetails.SetProdOperNbrs(OrderedOperationDetails.DefaultOperNbrLength, OrderedOperationDetails.DefaultStartingOperNbr, OrderedOperationDetails.DefaultOperNbrStep);
        }

        protected virtual AMBomItem GetOrderBomItem(string bomId, string revisionId)
        {
            if (BomItem?.Current?.RevisionID != null &&
                BomItem.Current.BOMID.EqualsWithTrim(bomId) &&
                BomItem.Current.RevisionID.EqualsWithTrim(revisionId))
            {
                return BomItem?.Current;
            }

            return PXSelect<AMBomItem,
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                .Select(ProcessingGraph, bomId, revisionId);
        }

        protected virtual void BuildOperationDetail(string bomId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                throw new ArgumentException(nameof(bomId));
            }

            if (string.IsNullOrWhiteSpace(revisionId))
            {
                throw new ArgumentException(nameof(revisionId));
            }

            var amBomItem = GetOrderBomItem(bomId, revisionId);

            if (amBomItem == null)
            {
                throw new InvalidBOMException(bomId);
            }

            foreach (PXResult<AMBomOper, AMWC> result in PXSelectJoin<
                    AMBomOper,
                    InnerJoin<AMWC, On<AMWC.wcID, Equal<AMBomOper.wcID>>>,
                    Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                        And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>,
                    OrderBy<
                        Asc<AMBomOper.operationCD>>>
                .Select(ProcessingGraph, bomId, revisionId))
            {
                LoadingPhantomsFirstLevel(result, result);
            }
        }
        
        protected virtual int LoadingPhantomsFirstLevel(AMBomOper amBomOper, AMWC wc)
        {
            if (string.IsNullOrWhiteSpace(amBomOper?.BOMID))
            {
                throw new PXArgumentException(nameof(amBomOper));
            }

            var opDetail = ConvertBomOperToOperationDetail(amBomOper);
            opDetail.Level = 0;
            opDetail.IsProdBom = true;
            opDetail.IncludeOper = true;
            opDetail.BomQtyReq = 1;
            opDetail.WcSiteID = wc.SiteID;

            //make sure these ops always go to the end
            var forceToEnd = _operationDetails.OrderedList.Count + 1;

            var index = _operationDetails.Add(forceToEnd, Position.After, opDetail);
            return LoadPhantoms(amBomOper, 1, index);
        }

        protected virtual OperationDetail ConvertBomOperToOperationDetail(AMBomOper amBomOper)
        {
            return new OperationDetail
            {
                BomOper = amBomOper,
                BomID = amBomOper.BOMID,
                BomRevisionID = amBomOper.RevisionID,
                BomOperationID = amBomOper.OperationID,
                BomOperationCD = amBomOper.OperationCD,
                WcID = amBomOper.WcID,
                WcDesc = amBomOper.Descr,
                WcBFlushLabor = amBomOper.BFlush
            };
        }

        protected virtual int LoadPhantoms(AMBomOper amBomOper, int level, int currentIndex)
        {
            return LoadPhantoms(amBomOper, level, currentIndex, 1m);
        }

        protected virtual int LoadPhantoms(AMBomOper amBomOper, int level, int currentIndex, decimal? parentQtyRequired)
        {
            var lastIndex = currentIndex;

            foreach (AMBomMatl amBomMatl in PXSelect<AMBomMatl,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomOper.revisionID>>,
                    And<AMBomMatl.operationID, Equal<Required<AMBomOper.operationID>>,
                        And<AMBomMatl.materialType, Equal<AMMaterialType.phantom>>>>>,
                OrderBy<Asc<AMBomMatl.sortOrder, Asc<AMBomMatl.lineID>>>
                >.Select(ProcessingGraph, amBomOper.BOMID, amBomOper.RevisionID, amBomOper.OperationID))
            {
                if (SkipMaterial(amBomMatl))
                {
                    continue;
                }

                lastIndex = BuildOperationByBom(amBomMatl, level, currentIndex, parentQtyRequired);
            }

            return lastIndex;
        }

        /// <summary>
        /// Should the given material item be skipped from the production order building process?
        /// </summary>
        /// <returns>True when ignoring the material item</returns>
        protected virtual bool SkipMaterial(AMBomMatl amBomMatl)
        {
            var effDate = Order?.Current?.SourceDate ?? Order?.Current?.PlanDate ?? CurrentProdItem?.StartDate ?? Accessinfo.BusinessDate.GetValueOrDefault();
            //Exclude expired/not yet effective material
            return string.IsNullOrWhiteSpace(amBomMatl?.BOMID) ||
                   amBomMatl.EffDate != null && amBomMatl.EffDate > effDate && !Common.Dates.DatesEqual(amBomMatl.EffDate, Common.Dates.EndOfTimeDate)
                   || amBomMatl.ExpDate != null && amBomMatl.ExpDate <= effDate && !Common.Dates.DatesEqual(amBomMatl.ExpDate, Common.Dates.BeginOfTimeDate);
        }

        public virtual void AMProdItem_BOMID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual bool TryGetSubassemblyBom(AMBomMatl bomMatl, out string bomId, out AMBomItem bomItem)
        {
            bomId = bomMatl.CompBOMID;
            if (bomMatl.CompBOMID != null)
            {
                if (bomMatl.CompBOMRevisionID != null)
                {
                    bomItem = (AMBomItem)PXSelect<AMBomItem,
                            Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                                And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                        .SelectWindowed(ProcessingGraph, 0, 1, bomMatl.CompBOMID, bomMatl.CompBOMRevisionID);
                    if (bomItem?.Status == AMBomStatus.Active)
                    {
                        return true;
                    }
                }

                bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(ProcessingGraph, bomMatl.CompBOMID);
                if (bomItem?.RevisionID != null)
                {
                    return true;
                }
            }

            bomId = new PrimaryBomIDManager(ProcessingGraph).GetPrimaryAllLevels(bomMatl.InventoryID, bomMatl.SiteID ?? Order.Current.SiteID, bomMatl.SubItemID);

            bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(ProcessingGraph, bomId);
            return bomItem?.BOMID != null;
        }

        protected virtual int BuildOperationByBom(AMBomMatl parentAmBomMatl, int level, int currentOpIndex, decimal? parentQtyRequired)
        {
            if (parentAmBomMatl == null)
            {
                return currentOpIndex;
            }

            if (level < 0 || level >= LowLevel.MaxLowLevel)
            {
                // Most likely a recursive bom - prevent getting stuck in infinite loop
#if DEBUG
                AMDebug.TraceWriteMethodName($"Error in Level value: {level}");
#endif
                return currentOpIndex;
            }

            if (!TryGetSubassemblyBom(parentAmBomMatl, out var bomId, out var bomItem))
            {
                InventoryItem inventoryItem = PXSelect<
                    InventoryItem, 
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(ProcessingGraph, parentAmBomMatl.InventoryID);

                AMBomOper bomOper = PXSelect<
                    AMBomOper,
                    Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                        And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
                        And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>>>>
                    .Select(ProcessingGraph, parentAmBomMatl.BOMID, parentAmBomMatl.RevisionID, parentAmBomMatl.OperationID);

                var msg = Messages.GetLocal(Messages.NoActiveBomForPhantomMaterial, 
                    inventoryItem?.InventoryCD?.TrimIfNotNullEmpty(), 
                    parentAmBomMatl.BOMID,
                    parentAmBomMatl.RevisionID, 
                    bomOper?.OperationCD ?? $"[{parentAmBomMatl.OperationID}]", 
                    parentAmBomMatl.LineID, 
                    level);

                if (string.IsNullOrWhiteSpace(bomId))
                {
                    PXTrace.WriteWarning(msg);
                    return currentOpIndex;
                }

                // Only an exception when a bomid is found (might be comp bomid reference or default on stock item) for the phantom but no active revision is found.
                throw new InvalidBOMException(bomId, msg);
            }

            return BuildOperation(
                bomItem.BOMID,
                bomItem.RevisionID,
                parentAmBomMatl.BOMID,
                parentAmBomMatl.RevisionID,
                parentAmBomMatl.LineID,
                parentAmBomMatl.OperationID,
                parentAmBomMatl.QtyReq,
                parentAmBomMatl.PhantomRouting.GetValueOrDefault(),
                level,
                currentOpIndex,
                parentQtyRequired);
        }

        protected virtual int BuildOperation(
            string bomId,
            string bomRevisionId,
            string matlBomId,
            string matlRevisionId,
            int? matlLineId,
            int? matlOperationId,
            decimal? matlQtyReq,
            int matlPhantomRouting,
            int level,
            int currentOpIndex)
        {
            return BuildOperation(bomId, bomRevisionId, matlBomId, matlRevisionId, matlLineId, matlOperationId, matlQtyReq, matlPhantomRouting, level, currentOpIndex, null);
        }

        protected virtual int BuildOperation(
            string bomId,
            string bomRevisionId,
            string matlBomId,
            string matlRevisionId,
            int? matlLineId,
            int? matlOperationId,
            decimal? matlQtyReq,
            int matlPhantomRouting,
            int level,
            int currentOpIndex,
            decimal? parentQtyRequired)
        {
            var position = Position.After;
            if (matlPhantomRouting != PhantomRoutingOptions.Exclude)
            {
                position = matlPhantomRouting == PhantomRoutingOptions.Before
                    ? Position.Before
                    : Position.After;
            }
            var opIndex = currentOpIndex;
            var firstOper = true;
            OperationDetail lastOperationDetail = null;
            OperationDetail positionOperationDetail = _operationDetails.FindParentByIndex(opIndex, level);
            if (positionOperationDetail != null && !positionOperationDetail.IncludeOper && positionOperationDetail.ParentIncludeOpReferenceKey != 0)
            {
                opIndex = _operationDetails.IndexOf(positionOperationDetail.ParentIncludeOpReferenceKey);
            }

            foreach (PXResult<AMBomOper, AMWC> result in PXSelectJoin<
                AMBomOper,
                InnerJoin<AMWC, On<AMWC.wcID, Equal<AMBomOper.wcID>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>,
                OrderBy<
                    Asc<AMBomOper.operationCD>>>
                .Select(ProcessingGraph, bomId, bomRevisionId))
            {
                var amBomOper = (AMBomOper) result;
                var wc = (AMWC)result;

                if (amBomOper?.BOMID == null || wc?.WcID == null)
                {
                    continue;
                }

                var opDetail = ConvertBomOperToOperationDetail(amBomOper);
                opDetail.Level = level;
                opDetail.IsProdBom = false;
                opDetail.IncludeOper = matlPhantomRouting != PhantomRoutingOptions.Exclude;
                opDetail.MatlBomID = matlBomId;
                opDetail.MatlRevisionID = matlRevisionId;
                opDetail.MatlLineId = matlLineId;
                opDetail.MatlOperationID = matlOperationId;
                opDetail.BomQtyReq = matlQtyReq.GetValueOrDefault(1) * parentQtyRequired.GetValueOrDefault(1);
                opDetail.WcSiteID = wc.SiteID;

                if (positionOperationDetail != null)
                {
                    opDetail.ParentReferenceKey = positionOperationDetail.ReferenceKey;
                    opDetail.ParentIncludeOpReferenceKey = positionOperationDetail.IncludeOper ? positionOperationDetail.ReferenceKey : positionOperationDetail.ParentIncludeOpReferenceKey;
#if DEBUG
                    AMDebug.TraceWriteMethodName($"NEWOPD[{opIndex}] BOMID = {amBomOper.BOMID}-{amBomOper.RevisionID}; Oper = {amBomOper.OperationCD}; level = {level}; ReferenceKey = {opDetail.ReferenceKey}; ParentIncludeOpReferenceKey = {opDetail.ParentIncludeOpReferenceKey}; BomId = {opDetail.BomID}; BomQtyReq = {UomHelper.FormatQty(opDetail.BomQtyReq)}".Indent(level+2));
                    AMDebug.TraceWriteMethodName($"PARENT[{opIndex}] BOMID = {amBomOper.BOMID}-{amBomOper.RevisionID}; Oper = {amBomOper.OperationCD}; level = {level}; ReferenceKey = {positionOperationDetail.ReferenceKey}; ParentIncludeOpReferenceKey = {positionOperationDetail.ParentIncludeOpReferenceKey}; BomQtyReq = {UomHelper.FormatQty(opDetail.BomQtyReq)}".Indent(level + 2));
                }
                else
                {
                    AMDebug.TraceWriteMethodName($"NEWOPD[{opIndex}] BOMID = {amBomOper.BOMID}-{amBomOper.RevisionID}; Oper = {amBomOper.OperationCD}; level = {level}; ReferenceKey = {opDetail.ReferenceKey}; ParentIncludeOpReferenceKey = {opDetail.ParentIncludeOpReferenceKey}; BomId = {opDetail.BomID}; BomQtyReq = {UomHelper.FormatQty(opDetail.BomQtyReq)}".Indent(level + 2));
                    AMDebug.TraceWriteMethodName("!!! parentOperationDetail == NULL !!!");
#endif
                }

                if (!firstOper)
                {
                    var lastIndex = _operationDetails.IndexOf(lastOperationDetail);
                    opIndex = lastIndex > opIndex ? lastIndex : opIndex;
                    position = Position.After;
                }

                lastOperationDetail = opDetail;
                var index = _operationDetails.Add(opIndex, position, opDetail);

                opIndex = LoadPhantoms(amBomOper, level + 1, index, opDetail.BomQtyReq);
                firstOper = false;
            }

            return opIndex;
        }

        /// <summary>
        /// Manage correct phantom/operation order
        /// </summary>
        [Serializable]
        protected class OrderedOperationDetails
        {
            public List<OperationDetail> OrderedList { get; private set; }
            public const int DefaultOperNbrLength = 4;
            public const int DefaultStartingOperNbr = 10;
            public const int DefaultOperNbrStep = 10;
            public const char OperNbrPadChar = '0';
            private int _currentKey;

            public OrderedOperationDetails()
            {
                OrderedList = new List<OperationDetail>();
                _currentKey = 1;
            }

            public int Add(OperationDetail currentOperationDetail, Position position, OperationDetail newOperationDetail)
            {
                int currentIndex = IndexOf(currentOperationDetail);
                return Add(currentIndex, position, newOperationDetail);
            }

            private int UseNextKey()
            {
                _currentKey++;
                return _currentKey;
            }

            public int Add(int currentIndex, Position position, OperationDetail newOperationDetail)
            {

                if (newOperationDetail == null)
                {
                    return currentIndex;
                }

                int addingIndex = currentIndex + (position == Position.Before ? 0 : 1);

                if (addingIndex < 0)
                {
                    addingIndex = 0;
                }

                newOperationDetail.ReferenceKey = UseNextKey();

                if (newOperationDetail.IsProdBom)
                {
                    newOperationDetail.ParentReferenceKey = newOperationDetail.ReferenceKey;
                    newOperationDetail.ParentIncludeOpReferenceKey = newOperationDetail.ReferenceKey;
                }

                if (addingIndex > OrderedList.Count || OrderedList.Count == 0)
                {
                    OrderedList.Add(newOperationDetail);
                    addingIndex = OrderedList.Count - 1;
                }
                else
                {
                    OrderedList.Insert(addingIndex, newOperationDetail);
                }

                return addingIndex;
            }

            /// <summary>
            /// Sets the Production Operation Numbers in the ordered collection
            /// </summary>
            /// <param name="operNbrLength">Operation number length (Ex: 4 results in '0000')</param>
            /// <param name="startingOperNbr">First order operation number (Ex: 10)</param>
            /// <param name="operNbrStep">Step between each operation number</param>
            public void SetProdOperNbrs(int operNbrLength, int startingOperNbr, int operNbrStep)
            {
                if (operNbrStep == 0)
                {
                    operNbrStep = DefaultOperNbrStep;
                }

                if (startingOperNbr == 0)
                {
                    startingOperNbr = DefaultStartingOperNbr;
                }

                var currentOperNbr = startingOperNbr;
                var cntr = 0;
                foreach (var operationDetail in OrderedList)
                {
                    if (!operationDetail.IncludeOper)
                    {
                        continue;
                    }

                    operationDetail.SortOrder = ++cntr;
                    operationDetail.ProdOperationCD = currentOperNbr.ToString().PadLeft(operNbrLength, OperNbrPadChar);

                    currentOperNbr += operNbrStep;
                }

                foreach (var operationDetail in OrderedList)
                {
                    if (operationDetail.IncludeOper || !string.IsNullOrWhiteSpace(operationDetail.ProdOperationCD))
                    {
                        continue;
                    }
                    operationDetail.ProdOperationCD = FindByKey(operationDetail.ParentIncludeOpReferenceKey).ProdOperationCD;
                }
            }

            /// <summary>
            /// Find the related parent operation detail from the current index and level
            /// </summary>
            /// <param name="currentIndex">Current processing index</param>
            /// <param name="currentLevel">Current processing level</param>
            /// <returns>Parent operation detail</returns>
            public OperationDetail FindParentByIndex(int currentIndex, int currentLevel)
            {
                var opDetail = FindByIndex(currentIndex);

                if (currentLevel == 0 
                    || opDetail == null
                    || opDetail.Level < currentLevel)
                {
                    return opDetail;
                }

                return FindByKey(opDetail.ParentReferenceKey);
            }

            public OperationDetail FindByIndex(int value)
            {
                if (value < 0 || value > OrderedList.Count)
                {
                    return null;
                }

                return OrderedList[value];
            }

            public OperationDetail FindByKey(int value)
            {
                int refKey = IndexOf(value);

                return OrderedList[refKey];
            }

            public int IndexOf(int referenceKey)
            {
                return OrderedList.FindIndex(r => r.ReferenceKey.Equals(referenceKey));
            }

            public int IndexOf(OperationDetail value)
            {
                return IndexOf(value.MatlOperationID, value.MatlLineId, value.BomID, value.BomOperationID);
            }

            public int IndexOf(int? matlOperationID, int? matlLineID, string bomID, int? bomOperationID)
            {
                if (matlOperationID == null)
                {
                    throw new ArgumentNullException($"Arg: {nameof(matlOperationID)} [{matlOperationID}, {matlLineID}, {bomID}, {bomOperationID}]");
                }

                if (matlLineID == null)
                {
                    throw new ArgumentNullException($"Arg: {nameof(matlLineID)} [{matlOperationID}, {matlLineID}, {bomID}, {bomOperationID}]");
                }

                if (bomID == null)
                {
                    throw new ArgumentNullException($"Arg: {nameof(bomID)} [{matlOperationID}, {matlLineID}, {bomID}, {bomOperationID}]");
                }

                if (bomOperationID == null)
                {
                    throw new ArgumentNullException($"Arg: {nameof(bomOperationID)} [{matlOperationID}, {matlLineID}, {bomID}, {bomOperationID}]");
                }

                return OrderedList.FindIndex(
                    r => r.MatlOperationID.Equals(matlOperationID)
                    && r.MatlLineId.Equals(matlLineID)
                    && r.BomID.Equals(bomID)
                    && r.BomOperationID.Equals(bomOperationID)
                    );
            }
        }

        protected virtual void SubstituteWorkCenters()
        {
            if (_operationDetails?.OrderedList == null)
            {
                return;
            }

            foreach (var operationDetail in _operationDetails.OrderedList)
            {
                // We sub when a different warehouse is used
                if (operationDetail.WcSiteID == null || operationDetail.WcSiteID == Order.Current?.SiteID)
                {
                    continue;
                }

                var result = (PXResult<AMWCSubstitute, AMWC>) PXSelectJoin<AMWCSubstitute,
                    InnerJoin<AMWC, On<AMWC.wcID, Equal<AMWCSubstitute.substituteWcID>>>,   
                    Where<AMWCSubstitute.wcID, Equal<Required<AMWCSubstitute.wcID>>,
                        And<AMWCSubstitute.siteID, Equal<Required<AMWCSubstitute.siteID>>>>
                >.Select(ProcessingGraph, operationDetail.WcID, Order.Current?.SiteID);

                var aMWCSubstitute = (AMWCSubstitute) result;
                var amWC = (AMWC) result;

                if (aMWCSubstitute?.WcID == null || amWC?.WcID == null || !amWC.ActiveFlg.GetValueOrDefault())
                {
                    continue;
                }

                operationDetail.WcID = amWC.WcID;
                operationDetail.WcDesc = aMWCSubstitute.UpdateOperDesc.GetValueOrDefault() ? amWC.Descr : operationDetail.WcDesc;
                operationDetail.WcBFlushLabor = amWC.BflushLbr.GetValueOrDefault();
            }
        }

        protected virtual void CopyWorkCenterOverheads(OperationDetail operationDetail)
        {
            foreach (PXResult<AMWCOvhd, AMOverhead> result in PXSelectJoin<
                AMWCOvhd,
                InnerJoin<AMOverhead,
                    On<AMWCOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMWCOvhd.wcID, Equal<Required<AMWCOvhd.wcID>>>
                    >.Select(ProcessingGraph, operationDetail.WcID))
            {
                var amWcOvhd = (AMWCOvhd)result;
                var overHead = (AMOverhead)result;

                if (overHead?.OvhdID == null || string.IsNullOrWhiteSpace(amWcOvhd?.OvhdID))
                {
                    continue;
                }

                var newProdOvhd = new AMProdOvhd
                {
                    OFactor = amWcOvhd.OFactor,
                    OvhdID = amWcOvhd.OvhdID,
                    WCFlag = true
                };

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdOvhd, operationDetail);
                }

                ProcessingGraph.Caches<AMProdOvhd>().Insert(newProdOvhd);
            }
        }

        /// <summary>
        /// From _operationDetails what are the unique bom/revision combinations
        /// </summary>
        /// <returns>List of Tuple where Item1 = bomid and Item2 = revision</returns>
        protected List<Tuple<string, string>> GetUniqueOperationDetailBomRevisions()
        {
            if (_operationDetails?.OrderedList == null)
            {
                return null;
            }
            var retList = new List<Tuple<string, string>>();
            var bomidHash = new HashSet<string>();

            foreach (var operationDetail in _operationDetails.OrderedList)
            {
                if (!bomidHash.Add(string.Join(";", operationDetail.BomID, operationDetail.BomRevisionID)))
                {
                    continue;
                }

                retList.Add(new Tuple<string, string>(operationDetail.BomID, operationDetail.BomRevisionID));
            }

            return retList;
        }

        [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
        protected class OperationDetail
        {
            internal string DebuggerDisplay => $"ReferenceKey = {ReferenceKey}; ParentIncludeOpReferenceKey = {ParentIncludeOpReferenceKey}; BomId = {BomID}; BomOperationCD = {BomOperationCD}; BomOperationID = {BomOperationID}; BomQtyReq = {BomQtyReq}";

            public int ReferenceKey { get; set; }
            /// <summary>
            /// Direct parent reference key
            /// (doesn't consider include/exclude of operation)
            /// </summary>
            public int ParentReferenceKey { get; set; }
            /// <summary>
            /// Parent reference key related to included operation
            /// </summary>
            public int ParentIncludeOpReferenceKey { get; set; }
            /// <summary>
            /// <see cref="AMBomOper"/> related to <see cref="BomID"/> <see cref="BomRevisionID"/> and <see cref="BomOperationID"/>
            /// </summary>
            public AMBomOper BomOper { get; set; }
            /// <summary>
            /// Source ID such as BOMID, EstimateID, ConfigurationID
            /// </summary>
            public string BomID { get; set; }
            public string BomRevisionID { get; set; }
            /// <summary>
            /// OperNbr (similar to OperationCD)
            /// </summary>
            public string BomOperationCD { get; set; }
            public int? BomOperationID { get; set; }
            public decimal BomQtyReq { get; set; }
            public string MatlBomID { get; set; }
            public string MatlRevisionID { get; set; }
            public int? MatlOperationID { get; set; }
            public int? MatlLineId { get; set; }
            public int Level { get; set; }
            /// <summary>
            /// Is the BOM the source production order BOM
            /// </summary>
            public bool IsProdBom { get; set; }
            /// <summary>
            /// Is the phantom routing included in the production order?
            /// </summary>
            public bool IncludeOper { get; set; }
            public string ProdOperationCD { get; set; }
            public int SortOrder { get; set; }
            public string WcID { get; set; }
            public string WcDesc { get; set; }
            public bool? WcBFlushLabor { get; set; }
            public int? WcSiteID { get; set; }
        }

        [PXCacheName(Messages.AMOrder)]
        [Serializable]
        public class AMOrder : IBqlTable
        {
            #region OrderType

            public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

            [AMOrderTypeField]
            public virtual string OrderType { get; set; }
            #endregion
            #region OrderNbr
            public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

            [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "Order Nbr", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String OrderNbr { get; set; }
            #endregion
            #region SourceID
            public abstract class sourceID : PX.Data.BQL.BqlString.Field<sourceID> { }

            [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "Source ID", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String SourceID { get; set; }
            #endregion
            #region RevisionID
            public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

            protected String _RevisionID;
            [RevisionIDField]
            public virtual String RevisionID
            {
                get { return this._RevisionID; }
                set { this._RevisionID = value; }
            }
            #endregion
            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

            protected Int32? _SubItemID;
            [PXDBInt]
            public virtual Int32? SubItemID { get; set; }
            #endregion
            #region SourceDate
            /// <summary>
            /// Source date of production detail
            /// </summary>
            public abstract class sourceDate : PX.Data.IBqlField
            {
            }
            /// <summary>
            /// Source date of production detail
            /// </summary>
            [PXDBDate]
            public virtual DateTime? SourceDate { get; set; }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

            protected Int32? _InventoryID;
            [PXDBInt]
            public virtual Int32? InventoryID { get; set; }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

            [PXDBInt]
            public virtual Int32? SiteID { get; set; }
            #endregion
            #region OrderQty
            public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? OrderQty { get; set; }
            #endregion
            #region PlanDate
            public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

            [PXDBDate]
            [PXUIField(DisplayName = "Plan Date")]
            public virtual DateTime? PlanDate { get; set; }
            #endregion
            #region SourceOrderType

            public abstract class sourceOrderType : PX.Data.BQL.BqlString.Field<sourceOrderType> { }

            [AMOrderTypeField]
            public virtual string SourceOrderType { get; set; }
            #endregion
        }
    }
}
