using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Create Production Orders Process Graph 
    /// Processing Graph for Creating and Scheduling Creation of production orders
    /// </summary>
    [PX.Objects.GL.TableAndChartDashboardType]
    [Serializable]
    public class CreateProductionOrdersProcess : PXGraph<CreateProductionOrdersProcess>
    {
        public PXFilter<ProductionOrdersCreateFilter> Filter;
        public PXCancel<ProductionOrdersCreateFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessingJoin<AMFixedDemand, ProductionOrdersCreateFilter,
            InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMFixedDemand.inventoryID>>,
            LeftJoin<SOOrder, On<SOOrder.noteID, Equal<AMFixedDemand.refNoteID>>,
            LeftJoin<SOLineSplit, On<SOLineSplit.planID, Equal<AMFixedDemand.planID>>,
            LeftJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
            LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>,
            LeftJoin<AMConfigurationResults, On<AMConfigurationResults.ordTypeRef, Equal<SOLine.orderType>,
                And<AMConfigurationResults.ordNbrRef, Equal<SOLine.orderNbr>,
                And<AMConfigurationResults.ordLineRef, Equal<SOLine.lineNbr>>>>,
            LeftJoin<AMProdMatlSplit, On<AMProdMatlSplit.planID, Equal<AMFixedDemand.planID>>,
            LeftJoin<AMProdMatl,
                On<AMProdMatl.orderType, Equal<AMProdMatlSplit.orderType>,
                And<AMProdMatl.prodOrdID, Equal<AMProdMatlSplit.prodOrdID>,
                And<AMProdMatl.operationID, Equal<AMProdMatlSplit.operationID>,
                And<AMProdMatl.lineID, Equal<AMProdMatlSplit.lineID>>>>>>>>>>>>>,
            Where2<Where<AMFixedDemand.inventoryID, Equal<Current<ProductionOrdersCreateFilter.inventoryID>>, Or<Current<ProductionOrdersCreateFilter.inventoryID>, IsNull>>,
            And2<Where<AMFixedDemand.siteID, Equal<Current<ProductionOrdersCreateFilter.siteID>>, Or<Current<ProductionOrdersCreateFilter.siteID>, IsNull>>,
            And2<Where<SOOrder.customerID, Equal<Current<ProductionOrdersCreateFilter.customerID>>, Or<Current<ProductionOrdersCreateFilter.customerID>, IsNull>>,
            And2<Where<SOOrder.orderType, Equal<Current<ProductionOrdersCreateFilter.sOOrderType>>, Or<Current<ProductionOrdersCreateFilter.sOOrderType>, IsNull>>,
            And2<Where<SOOrder.orderNbr, Equal<Current<ProductionOrdersCreateFilter.sOOrderNbr>>, Or<Current<ProductionOrdersCreateFilter.sOOrderNbr>, IsNull>>,
            And2<Where<AMFixedDemand.planDate, GreaterEqual<Current<ProductionOrdersCreateFilter.requestedOnStartDate>>, Or<Current<ProductionOrdersCreateFilter.requestedOnStartDate>, IsNull>>,
            And2<Where<AMFixedDemand.planDate, LessEqual<Current<ProductionOrdersCreateFilter.requestedOnEndDate>>, Or<Current<ProductionOrdersCreateFilter.requestedOnEndDate>, IsNull>>,
            And2<Where<INItemClass.itemClassCD, Like<Current<ProductionOrdersCreateFilter.itemClassCDWildcard>>, Or<Current<ProductionOrdersCreateFilter.itemClassCDWildcard>, IsNull>>,
            And2<Where<AMConfigurationResults.configResultsID, IsNull, Or<AMConfigurationResults.completed, Equal<True>>>,
            And2<Where<AMProdMatlSplit.orderType, Equal<Current<ProductionOrdersCreateFilter.orderType>>, Or<Current<ProductionOrdersCreateFilter.orderType>, IsNull>>,
            And2<Where<AMProdMatlSplit.prodOrdID, Equal<Current<ProductionOrdersCreateFilter.prodOrdID>>, Or<Current<ProductionOrdersCreateFilter.prodOrdID>, IsNull>>,
            And<Where<AMFixedDemand.supplyPlanID, IsNull>>>>>>>>>>>>>> FixedDemand;

        public PXSetup<AMPSetup> ampsetup;
        public PXSetupOptional<AMConfiguratorSetup> ConfiguratorSetup;
        public PXSetup<SOSetup> sosetup;
        
        public CreateProductionOrdersProcess()
        {
            InquiresDropMenu.AddMenuAction(inventorySummary);
            InquiresDropMenu.AddMenuAction(inventoryAllocationDetails);

            var setup = ampsetup.Current;
            AMPSetup.CheckSetup(setup);

            var filter = Filter.Current;
            FixedDemand.SetProcessDelegate(delegate (List<AMFixedDemand> list)
            {
                CreateProcess(list, filter);
            });

            PXUIFieldAttribute.SetDisplayName<InventoryItem.descr>(Caches[typeof(InventoryItem)], PX.Objects.PO.Messages.InventoryItemDescr);
            PXUIFieldAttribute.SetDisplayName<INSite.descr>(Caches[typeof(INSite)], PX.Objects.PO.Messages.SiteDescr);
            PXUIFieldAttribute.SetDisplayName<Customer.acctName>(Caches[typeof(Customer)], PX.Objects.PO.Messages.CustomerAcctName);
            PXUIFieldAttribute.SetDisplayName<SOOrder.customerLocationID>(Caches[typeof(SOOrder)], PX.Objects.PO.Messages.CustomerLocationID);
            PXUIFieldAttribute.SetDisplayName<INPlanType.descr>(Caches[typeof(INPlanType)], PX.Objects.PO.Messages.PlanTypeDescr);
        }

        protected virtual IEnumerable fixedDemand()
        {
            var fixedDemands =
                PXSelectJoin<AMFixedDemand,
                InnerJoin<InventoryItem,
                    On<InventoryItem.inventoryID, Equal<AMFixedDemand.inventoryID>>,
                LeftJoin<SOOrder,
                    On<SOOrder.noteID, Equal<AMFixedDemand.refNoteID>>,
                LeftJoin<SOLineSplit,
                    On<SOLineSplit.planID, Equal<AMFixedDemand.planID>>,
                LeftJoin<SOLine,
                    On<SOLine.orderType, Equal<SOLineSplit.orderType>,
                    And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>,
                    And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
                LeftJoin<INItemClass,
                    On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>,
                LeftJoin<SOOrderType,
                    On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
                LeftJoin<AMProdMatlSplit, On<AMProdMatlSplit.planID, Equal<AMFixedDemand.planID>>,
                LeftJoin<AMProdMatl,
                    On<AMProdMatl.orderType, Equal<AMProdMatlSplit.orderType>,
                    And<AMProdMatl.prodOrdID, Equal<AMProdMatlSplit.prodOrdID>,
                    And<AMProdMatl.operationID, Equal<AMProdMatlSplit.operationID>,
                    And<AMProdMatl.lineID, Equal<AMProdMatlSplit.lineID>>>>>>>>>>>>>,
                Where<
                    Where<AMFixedDemand.supplyPlanID, IsNull,
                        And2<Where<AMFixedDemand.inventoryID, Equal<Current<ProductionOrdersCreateFilter.inventoryID>>,
                            Or<Current<ProductionOrdersCreateFilter.inventoryID>, IsNull>>,
                        And2<Where<AMFixedDemand.siteID, Equal<Current<ProductionOrdersCreateFilter.siteID>>,
                                Or<Current<ProductionOrdersCreateFilter.siteID>, IsNull>>,
                        And2<Where<SOOrder.customerID, Equal<Current<ProductionOrdersCreateFilter.customerID>>,
                                Or<Current<ProductionOrdersCreateFilter.customerID>, IsNull>>,
                        And2<Where<SOOrder.orderType, Equal<Current<ProductionOrdersCreateFilter.sOOrderType>>,
                                Or<Current<ProductionOrdersCreateFilter.sOOrderType>, IsNull>>,
                        And2<Where<SOOrder.orderNbr, Equal<Current<ProductionOrdersCreateFilter.sOOrderNbr>>,
                                Or<Current<ProductionOrdersCreateFilter.sOOrderNbr>, IsNull>>,
                        And2<Where<AMFixedDemand.planDate, GreaterEqual<Current<ProductionOrdersCreateFilter.requestedOnStartDate>>,
                                Or<Current<ProductionOrdersCreateFilter.requestedOnStartDate>, IsNull>>,
                        And2<Where<AMFixedDemand.planDate, LessEqual<Current<ProductionOrdersCreateFilter.requestedOnEndDate>>,
                                Or<Current<ProductionOrdersCreateFilter.requestedOnEndDate>, IsNull>>,
                        And2<Where<AMProdMatlSplit.orderType, Equal<Current<ProductionOrdersCreateFilter.orderType>>,
                                Or<Current<ProductionOrdersCreateFilter.orderType>, IsNull>>,
                        And2<Where<AMProdMatlSplit.prodOrdID, Equal<Current<ProductionOrdersCreateFilter.prodOrdID>>,
                                Or<Current<ProductionOrdersCreateFilter.prodOrdID>, IsNull>>,
                        And<Where<INItemClass.itemClassCD, Like<Current<ProductionOrdersCreateFilter.itemClassCDWildcard>>,
                                Or<Current<ProductionOrdersCreateFilter.itemClassCDWildcard>, IsNull>>>>>>>>>>>>>>>
                    .Select(this);

            return EnumerateAndPrepareFixedDemands(fixedDemands);
        }

        /// <summary>
		/// Enumerates the and prepares fixed demands for the view delegate. 
		/// </summary>
		/// <param name="fixedDemands">The fixed demands.</param>
		/// <returns/>
		protected virtual IEnumerable EnumerateAndPrepareFixedDemands(PXResultset<AMFixedDemand> fixedDemands)
        {
            foreach (PXResult<AMFixedDemand, InventoryItem, SOOrder, SOLineSplit, SOLine, INItemClass, SOOrderType, AMProdMatlSplit, AMProdMatl> rec in fixedDemands)
            {
                var demand = (AMFixedDemand)rec;
                var order = (SOOrder)rec;
                var line = (SOLine)rec;
                var orderType = (SOOrderType)rec;
                var matlSplit = (AMProdMatlSplit)rec;
                var matl = (AMProdMatl)rec;

                if (demand?.PlanType == INPlanConstants.PlanM8 && !OrderAllowsProductionCreation(orderType, order))
                {
                    // Sales Order link however is not allowed to create a production order
                    continue;
                }

                if (demand?.PlanType == INPlanConstants.PlanMA && demand?.Hold == true)
                {
                    // Production to Production link however we skip on hold orders
                    continue;
                }

                if (demand?.InventoryID != null && demand.UOM != null && Filter.Current.CreationDate != null && line?.OrderType != null && line?.OrderNbr != null
                    && line?.LineNbr != null)
                {
                    demand.SOOrderType = line.OrderType;
                    demand.SOOrderNbr = line.OrderNbr;
                    demand.SOLineNbr = line.LineNbr;
                    demand.Descr = line.TranDesc;
                }

                if (demand?.InventoryID != null && demand.UOM != null && Filter.Current.CreationDate != null && matlSplit?.OrderType != null && matlSplit?.ProdOrdID != null
                    && matlSplit?.LineID != null)
                {
                    demand.AMOrderType = matlSplit.OrderType;
                    demand.AMProdOrdID = matlSplit.ProdOrdID;
                    demand.AMOperationID = matlSplit.OperationID;
                    demand.AMLineID = matlSplit.LineID;
                    demand.Descr = matl.Descr;
                }

                yield return rec;
            }
        }

        #region Actions

        public PXAction<ProductionOrdersCreateFilter> InquiresDropMenu;
        [PXUIField(DisplayName = Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected IEnumerable inquiresDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<ProductionOrdersCreateFilter> inventorySummary;
        [PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable InventorySummary(PXAdapter adapter)
        {
            PXCache tCache = FixedDemand.Cache;
            AMFixedDemand line = FixedDemand.Current;
            if (line == null) return adapter.Get();

            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<AMFixedDemand.inventoryID>(tCache, line);
            if (item != null && item.StkItem == true)
            {
                INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<AMFixedDemand.subItemID>(tCache, line);
                InventorySummaryEnq.Redirect(item.InventoryID,
                                             ((sbitem != null) ? sbitem.SubItemCD : null),
                                             line.SiteID,
                                             line.LocationID);
            }
            return adapter.Get();
        }

        public PXAction<ProductionOrdersCreateFilter> inventoryAllocationDetails;
        [PXUIField(DisplayName = "Inventory Allocation Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable InventoryAllocationDetails(PXAdapter adapter)
        {
            PXCache tCache = FixedDemand.Cache;
            AMFixedDemand line = FixedDemand.Current;
            if (line == null) return adapter.Get();

            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<AMFixedDemand.inventoryID>(tCache, line);
            if (item != null && item.StkItem == true)
            {
                INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<AMFixedDemand.subItemID>(tCache, line);
                InventoryAllocDetEnq.Redirect(item.InventoryID,
                                             ((sbitem != null) ? sbitem.SubItemCD : null),
                                             line.LotSerialNbr,
                                             line.SiteID,
                                             line.LocationID);
            }
            return adapter.Get();
        }

        public PXAction<ProductionOrdersCreateFilter> viewDocument;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            var line = FixedDemand.Current;
            if (line?.RefNoteID == null) return adapter.Get();

            SOOrder doc = PXSelect<SOOrder, Where<SOOrder.noteID, Equal<Required<AMFixedDemand.refNoteID>>>>.Select(this, line.RefNoteID);

            if (doc != null)
            {
                SOOrderEntry graph = PXGraph.CreateInstance<SOOrderEntry>();
                graph.Document.Current = doc;
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        #endregion

        public static void CreateProcess(List<AMFixedDemand> list, ProductionOrdersCreateFilter filter)
        {
            var prodMaintGraph = CreateInstance<ProdMaint>();
            var createProdOrdersGraph = CreateInstance<CreateProductionOrdersProcess>();

            Numbering prodNumbering = PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID,
                        Equal<Numbering.numberingID>>>, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>
                        >.Select(prodMaintGraph, filter.CreationOrderType);

            if (prodNumbering == null)
            {
                throw new PXException(Messages.GetLocal(Messages.NumberingMissingExceptionProduction,
                    filter.CreationOrderType));
            }

            if (prodNumbering.UserNumbering.GetValueOrDefault())
            {
                throw new PXException(Messages.GetLocal(Messages.ManualNumberingNotAllowedForProcess,
                    filter.CreationOrderType));
            }

            createProdOrdersGraph.CreateProductionOrders(list, filter, prodMaintGraph); 

        }

        /// <summary>
        /// Create production orders based on a set list of sales lines
        /// Called from the SOOrderEntry Screen
        /// </summary>
        public virtual void CreateProductionOrdersFromSalesLines(List<SOLine> list)
        {
            var createProdOrdersGraph = CreateInstance<CreateProductionOrdersProcess>();
            var demandList = new List<AMFixedDemand>();
            var graph = CreateInstance<ProdMaint>();

            foreach (var line in list)
            {
                var soLineAMExtension = line.GetExtension<SOLineExt>();

                SOLineSplit soLineSplit = PXSelect<SOLineSplit,
                    Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
                        And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
                        And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>
                    >>>>.Select(this, line.OrderType, line.OrderNbr, line.LineNbr);

                if (soLineAMExtension == null
                    || !soLineAMExtension.AMSelected.GetValueOrDefault()
                    || line.Qty.GetValueOrDefault() <= 0
                    || !line.IsStockItem.GetValueOrDefault()
                    || soLineSplit == null)
                {
                    continue;
                }
                
                AMFixedDemand demand = PXSelectReadonly<AMFixedDemand,
                    Where<AMFixedDemand.planID, Equal<Required<AMFixedDemand.planID>>>>.Select(createProdOrdersGraph, soLineSplit.PlanID);

                if (demand?.PlanID == null)
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"Adding fixed demand material planid {demand.PlanID}");
#endif
                demand.Selected = true;
                demand.SOOrderType = line.OrderType;
                demand.SOOrderNbr = line.OrderNbr;
                demand.SOLineNbr = line.LineNbr;
                demand.Descr = line.TranDesc;
                demand.AMOrderType = soLineAMExtension.AMOrderType;
                demand.AMProdOrdID = soLineAMExtension.AMProdOrdID;

                demandList.Add(createProdOrdersGraph.FixedDemand.Update(demand));
            }

            CreateProductionOrders(demandList, null, graph);
        }

        /// <summary>
        /// Create production orders 
        /// </summary>
        public virtual void CreateProductionOrders(List<AMFixedDemand> list, ProductionOrdersCreateFilter filter, ProdMaint prodMaintGraph)
        {
            if (list == null
                || list.Count == 0)
            {
                return;
            }

            prodMaintGraph.IsImport = true;

            var errorsFound = false;
            var configSetup = (AMConfiguratorSetup)PXSelect<AMConfiguratorSetup>.Select(prodMaintGraph);

            for (var i = 0; i < list.Count; i++)
            {
                var demand = list[i];
                try
                {
                    prodMaintGraph.Clear();

                    SOLine soLine = null;
                    SOLineExt soLineExt = null;
                    var isSO2Production = demand.PlanType == INPlanConstants.PlanM8;
                    var isProd2Prod = demand.PlanType == INPlanConstants.PlanMA;

                    if (isSO2Production)
                    {
                        soLine = PXSelect<SOLine,
                            Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                                And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                                    And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>
                        >.SelectWindowed(prodMaintGraph, 0, 1, demand.SOOrderType, demand.SOOrderNbr, demand.SOLineNbr);
                        if (soLine != null)
                        {
                            soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
                        }

                        if (soLine == null || soLineExt == null)
                        {
                            continue;
                        }
                    }

                    var amProdItem = prodMaintGraph.ProdMaintRecords.Insert(new AMProdItem
                    {
                        OrderType = filter == null ? demand.AMOrderType : filter.CreationOrderType,
                        ProdOrdID = filter == null ? demand.AMProdOrdID : null,
                        SupplyType = isSO2Production
                            ? ProductionSupplyType.SalesOrder
                            : ProductionSupplyType.Production,
                        QtytoProd = demand.PlanQty,
                        BOMEffDate = demand.PlanDate
                    });

                    amProdItem.Descr = demand.Descr;
                    amProdItem.ProdDate = filter != null ? filter.CreationDate : prodMaintGraph.Accessinfo.BusinessDate;
                    amProdItem.InventoryID = demand.InventoryID;
                    amProdItem.SubItemID = demand.SubItemID;
                    amProdItem.ConstDate = demand.PlanDate;
                    amProdItem.SchedulingMethod = ScheduleMethod.FinishOn;
                    amProdItem.Qty = demand.PlanQty;
                    amProdItem.UOM = InventoryHelper.GetbaseUOM(prodMaintGraph, demand.InventoryID);
                    amProdItem.OrdLineRef = isSO2Production ? demand.SOLineNbr : null;
                    amProdItem.OrdTypeRef = isSO2Production ? demand.SOOrderType : null;
                    amProdItem.OrdNbr = isSO2Production ? demand.SOOrderNbr : null;
                    amProdItem.UpdateProject = false;
                    amProdItem.CustomerID = demand.BAccountID;

                    var customer = (BAccountR) PXSelectorAttribute.Select<AMProdItem.customerID>(prodMaintGraph.ProdMaintRecords.Cache, amProdItem);
                    if (customer?.BAccountID == null || customer.Type != BAccountType.CustomerType && customer.Type != BAccountType.CombinedType)
                    {
                        //not a valid CustomerID (such as planning from a Transfer order)
                        amProdItem.CustomerID = null;
                    }

                    AMProdItem parentProdItem = null;
                    if (isProd2Prod)
                    {
                        parentProdItem = PXSelect<
                            AMProdItem, 
                            Where<AMProdItem.orderType, Equal<Required<AMProdItem.prodOrdID>>,
                                And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>
                            .Select(this, demand.AMOrderType, demand.AMProdOrdID);

                        if (parentProdItem?.ProdOrdID != null)
                        {
                            amProdItem.ParentOrderType = parentProdItem.OrderType;
                            amProdItem.ParentOrdID = parentProdItem.ProdOrdID;
                            amProdItem.ProductOrderType = parentProdItem.ProductOrderType;
                            amProdItem.ProductOrdID = parentProdItem.ProductOrdID;

                            if (amProdItem.ProductOrdID == null)
                            {
                                amProdItem.ProductOrderType = parentProdItem.OrderType;
                                amProdItem.ProductOrdID = parentProdItem.ProdOrdID;
                            }

                            if (parentProdItem.ProjectID != null)
                            {
                                amProdItem.ProjectID = parentProdItem.ProjectID;
                                amProdItem.TaskID = parentProdItem.TaskID;
                                amProdItem.CostCodeID = parentProdItem.CostCodeID;
                            }
                        }
                    }

                    var productionDetailSource = ProductionDetailSource.NoSource;

                    if (isSO2Production)
                    {
                        // Acumatica allows null on SOLine.ProjectID field as it is not a project attribute field.
                        //      Because our field uses the project attribute it needs at least the non project value which it gets from the default value
                        if (soLine.ProjectID != null)
                        {
                            amProdItem.ProjectID = soLine.ProjectID;
                            amProdItem.TaskID = soLine.TaskID;
                            amProdItem.CostCodeID = soLine.CostCodeID;
                        }

                        if (soLineExt.IsConfigurable.GetValueOrDefault() &&
                            configSetup != null &&
                            !string.IsNullOrWhiteSpace(configSetup.ConfigNumberingID) &&
                            OrderTypeAllowConfigurations(prodMaintGraph, demand.SOOrderType))
                        {
                            var configRow = GetConfiguration(prodMaintGraph, soLine);
                            var configuration = (AMConfiguration) PXSelect<AMConfiguration,
                                Where<AMConfiguration.configurationID,
                                    Equal<Required<AMConfigurationResults.configurationID>>,
                                    And<AMConfiguration.revision,
                                        Equal<Required<AMConfigurationResults.revision>>>>>.Select(prodMaintGraph,
                                configRow.ConfigurationID, configRow.Revision);

                            if (configuration?.IsCompletionRequired == true && configRow.Completed != true)
                            {
                                var cfgMsg = Messages.GetLocal(Messages.ProductionOrderConfigurationUncompleted, soLine.TranDesc);
                                PXTrace.WriteWarning(cfgMsg);
                                errorsFound = true;

                                PXProcessing<AMFixedDemand>.SetError(i, cfgMsg);
                                continue;
                            }

                            if (configRow != null)
                            {
                                productionDetailSource = ProductionDetailSource.Configuration;
                                prodMaintGraph.ItemConfiguration.Current =
                                    prodMaintGraph.ItemConfiguration.Update(configRow);
                                prodMaintGraph.ItemConfiguration.Cache
                                    .SetDefaultExt<AMConfigurationResults.prodOrderType>(configRow);
                                prodMaintGraph.ItemConfiguration.Cache
                                    .SetDefaultExt<AMConfigurationResults.prodOrderNbr>(configRow);
                            }
                        }

                        if (productionDetailSource == ProductionDetailSource.NoSource &&
                            !string.IsNullOrWhiteSpace(soLineExt.AMEstimateID) &&
                            !string.IsNullOrWhiteSpace(soLineExt.AMEstimateRevisionID))
                        {
                            productionDetailSource = ProductionDetailSource.Estimate;
                            amProdItem.EstimateID = soLineExt.AMEstimateID;
                            amProdItem.EstimateRevisionID = soLineExt.AMEstimateRevisionID;
                        }
                    }

                    if (productionDetailSource == ProductionDetailSource.NoSource)
                    {
                        productionDetailSource = ProductionDetailSource.BOM;
                    }

                    amProdItem.DetailSource = productionDetailSource;
                    amProdItem.SiteID = demand.SiteID;
                    amProdItem.LocationID = demand.LocationID ?? InventoryHelper.DfltLocation.GetDefault(prodMaintGraph,
                                                InventoryHelper.DfltLocation.BinType.Receipt, demand.InventoryID,
                                                demand.SiteID, true);
                    var amProdItem2 = prodMaintGraph.ProdMaintRecords.Update(amProdItem);

                    prodMaintGraph.AMProdItemCopyBOMNotes(amProdItem2);
                    amProdItem2.Reschedule = true;
                    amProdItem2.BuildProductionBom = true;
                    amProdItem2.DetailSource = productionDetailSource;
                    prodMaintGraph.ProdMaintRecords.Update(amProdItem2);

                    var eventRefNote = parentProdItem?.ProdOrdID != null ? parentProdItem.NoteID : demand.RefNoteID;
                    //This call must be before the first save of PordMaint
                    prodMaintGraph.InsertCreatedOrderEventMessage(GetProductionCreatedEventMessage(amProdItem2, demand, soLine, parentProdItem), eventRefNote);

                    prodMaintGraph.Actions.PressSave();

                    var rowSetWithWarning = false;
                    if (prodMaintGraph.ProdItemSelected.Current != null
                        && prodMaintGraph.ProdItemSelected.Current.DetailSource == ProductionDetailSource.BOM
                        && string.IsNullOrWhiteSpace(prodMaintGraph.ProdItemSelected.Current.BOMID)
                        && !string.IsNullOrWhiteSpace(prodMaintGraph.ProdItemSelected.Current.ProdOrdID))
                    {
                        var msg = Messages.GetLocal(Messages.ProductionOrderCreatedWithoutBom,
                            prodMaintGraph.ProdMaintRecords.Current?.OrderType?.TrimIfNotNullEmpty(),
                            prodMaintGraph.ProdMaintRecords.Current?.ProdOrdID?.TrimIfNotNullEmpty());
                        PXTrace.WriteWarning(msg);
                        PXProcessing<AMFixedDemand>.SetWarning(i, msg);
                        rowSetWithWarning = true;
                    }

                    if (isSO2Production)
                    {
                        soLineExt.AMOrderType = prodMaintGraph.ProdMaintRecords?.Current?.OrderType;
                        soLineExt.AMProdOrdID = prodMaintGraph.ProdMaintRecords?.Current?.ProdOrdID;

                        if (!string.IsNullOrWhiteSpace(soLineExt.AMProdOrdID) &&
                            !string.IsNullOrWhiteSpace(soLineExt.AMOrderType))
                        {
                            Common.Cache.AddCacheView<SOLine>(prodMaintGraph);
                            prodMaintGraph.Caches<SOLine>().Update(soLine);
                        }

                        SOLineSplit soLineSplit =
                            PXSelect<SOLineSplit, Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>>>
                                .Select(prodMaintGraph, demand.PlanID);
                        var splitExt = soLineSplit?.GetExtension<SOLineSplitExt>();

                        soLineSplit.RefNoteID = prodMaintGraph.ProdMaintRecords.Current.NoteID;
                        splitExt.AMOrderType = prodMaintGraph.ProdMaintRecords?.Current?.OrderType;
                        splitExt.AMProdOrdID = prodMaintGraph.ProdMaintRecords?.Current?.ProdOrdID;

                        Common.Cache.AddCacheView<SOLineSplit>(prodMaintGraph);
                        prodMaintGraph.Caches<SOLineSplit>().Update(soLineSplit);
                    }

                    if (isProd2Prod && prodMaintGraph.ProdMaintRecords?.Current?.NoteID != null)
                    {
                        var parentMatlSplit = (AMProdMatlSplit)PXSelect<AMProdMatlSplit,
                                Where<AMProdMatlSplit.planID, Equal<Required<AMProdMatlSplit.planID>>>>
                                .Select(prodMaintGraph, demand.PlanID);

                        if (parentMatlSplit?.ProdOrdID != null)
                        {
                            parentMatlSplit.RefNoteID = prodMaintGraph.ProdMaintRecords.Current.NoteID;
                            parentMatlSplit.AMOrderType = prodMaintGraph.ProdMaintRecords.Current.OrderType;
                            parentMatlSplit.AMProdOrdID = prodMaintGraph.ProdMaintRecords.Current.ProdOrdID;
                        }

                        prodMaintGraph.ProdMatlSplits.Update(parentMatlSplit);
                    }

                    var supplyPlanID = prodMaintGraph?.splits?.Current?.PlanID;
                    if (supplyPlanID != null && demand?.PlanID != null)
                    {
                        demand.SupplyPlanID = supplyPlanID;
                        prodMaintGraph.Caches[typeof(INItemPlan)].Update(demand);
                    }

                    prodMaintGraph.Actions.PressSave();

                    if (!rowSetWithWarning)
                    {
                        var newOrderType = prodMaintGraph.ProdMaintRecords?.Current?.OrderType;
                        var newProdOrdID = prodMaintGraph.ProdMaintRecords?.Current?.ProdOrdID;

                        switch (demand.PlanType)
                        {
                            //Linked to sales order
                            case INPlanConstants.PlanM8:
                                PXProcessing<AMFixedDemand>.SetInfo(i, Messages.GetLocal(Messages.CreatedProductionOrderForSalesOrder, newOrderType, newProdOrdID, soLine?.OrderType, soLine?.OrderNbr));
                                break;

                            //Linked to production order
                            case INPlanConstants.PlanMA:
                                PXProcessing<AMFixedDemand>.SetInfo(i, Messages.GetLocal(Messages.CreatedProductionOrderForProductionOrder, newOrderType, newProdOrdID, demand?.AMOrderType, demand?.AMProdOrdID));
                                break;

                            //No Link
                            default:
                                PXProcessing<AMFixedDemand>.SetInfo(i, ActionsMessages.RecordProcessed);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    PXProcessing<AMFixedDemand>.SetError(i, e);
                    PXTraceHelper.PxTraceException(e);
                    errorsFound = true;
                }
            }

            if (errorsFound)
            {
                throw new PXException(
                    $"{Messages.GetLocal(Messages.CreatingProductionOrders)}: {Messages.GetLocal(PX.Data.ErrorMessages.SeveralItemsFailed, typeof(PX.Data.ErrorMessages))}");
            }
        }

        /// <summary>
        /// Build the production event for the order created event message
        /// </summary>
        protected virtual string GetProductionCreatedEventMessage(AMProdItem prodItem, AMFixedDemand demand, SOLine soLine, AMProdItem parentProdItem)
        {
            if (soLine?.OrderNbr != null && soLine.OrderType != null)
            {
                return Messages.GetLocal(Messages.CreatedFromOrderTypeOrderNbr, soLine.OrderType.Trim(), soLine.OrderNbr.Trim());
            }

            if (parentProdItem?.ProdOrdID != null && parentProdItem.OrderType != null)
            {
                return Messages.GetLocal(Messages.CreatedFromParentProductionOrder, parentProdItem.OrderType.Trim(), parentProdItem.ProdOrdID.Trim());
            }

            var functionDesc = prodItem?.Function == null
                ? string.Empty
                : OrderTypeFunction.GetDescription(prodItem.Function);

            return Messages.GetLocal(Messages.CreatedOrder, functionDesc);
        }

        private static bool OrderTypeAllowConfigurations(PXGraph graph, string strOrderType)
        {
            SOOrderType orderType = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>.Select(graph, strOrderType);

            if (orderType != null)
            {
                var soOrderTypeExt = orderType.GetExtension<SOOrderTypeExt>();
                return soOrderTypeExt != null && soOrderTypeExt.AMConfigurationEntry.GetValueOrDefault();
            }
            return false;
        }

        /// <summary>
        /// Get/calculate the sales lines plan date for production
        /// </summary>
        /// <param name="soLine">sales line</param>
        /// <returns>production plan date</returns>
        private static DateTime? GetPlanDate(SOLine soLine, DateTime busienssDate)
        {
            DateTime planDate = soLine.ShipDate ?? soLine.RequestDate.GetValueOrDefault(busienssDate);
            if (Common.Dates.IsMinMaxDate(planDate)
                || Common.Dates.IsDefaultDate(planDate))
            {
                planDate = busienssDate;
            }

            //  lets remove a day to account for production completion before ship date
            return planDate.AddDays(-1);
        }

        private static AMConfigurationResults GetConfiguration(PXGraph graph, SOLine soLine)
        {
            if (soLine == null)
            {
                throw new PXArgumentException(nameof(soLine));
            }

            return PXSelect<AMConfigurationResults,
                    Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                        And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                            And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
                >.SelectWindowed(graph, 0, 1, soLine.OrderType, soLine.OrderNbr, soLine.LineNbr);
        }

        protected virtual void ProductionOrdersCreateFilter_SOOrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ProductionOrdersCreateFilter)e.Row;
            if (row == null)
            {
                return;
            }

            row.SOOrderNbr = null;
        }

        protected virtual void ProductionOrdersCreateFilter_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ProductionOrdersCreateFilter)e.Row;
            if (row == null)
            {
                return;
            }

            row.ProdOrdID = null;
        }

        protected virtual bool OrderAllowsProductionCreation(SOOrderType orderType, SOOrder order)
        {
            return SOOrderEntryAMExtension.CanCreateProductionOrder(orderType, order);
        }

        [Serializable]
        [PXCacheName(Messages.ProductionOrdersCreateFilter)]
        public partial class ProductionOrdersCreateFilter : IBqlTable
        {
            #region CreationOrderType
            public abstract class creationOrderType : PX.Data.BQL.BqlString.Field<creationOrderType> { }

            protected String _CreationOrderType;
            [PXDefault(typeof(AMPSetup.defaultOrderType))]
            [AMOrderTypeField(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Creation Order Type")]
            [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
            [AMOrderTypeSelector]
            public virtual String CreationOrderType
            {
                get
                {
                    return this._CreationOrderType;
                }
                set
                {
                    this._CreationOrderType = value;
                }
            }
            #endregion
            #region CreationDate
            public abstract class creationDate : PX.Data.BQL.BqlDateTime.Field<creationDate> { }

            protected DateTime? _CreationDate;
            [PXDBDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Creation Date", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual DateTime? CreationDate
            {
                get
                {
                    return this._CreationDate;
                }
                set
                {
                    this._CreationDate = value;
                }
            }
            #endregion
            #region CurrentOwnerID
            public abstract class currentOwnerID : PX.Data.BQL.BqlInt.Field<currentOwnerID> { }

            [PXDBInt]
            [CRCurrentOwnerID]
            public virtual int? CurrentOwnerID { get; set; }
            #endregion
            #region MyOwner
            public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }

            protected Boolean? _MyOwner;
            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Me")]
            public virtual Boolean? MyOwner
            {
                get
                {
                    return _MyOwner;
                }
                set
                {
                    _MyOwner = value;
                }
            }
            #endregion
            #region OwnerID
            public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

            protected int? _OwnerID;
            [PX.TM.SubordinateOwner(DisplayName = "Product Manager")]
            public virtual int? OwnerID
            {
                get
                {
                    return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
                }
                set
                {
                    _OwnerID = value;
                }
            }
            #endregion
            #region WorkGroupID
            public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }

            protected Int32? _WorkGroupID;
            [PXDBInt]
            [PXUIField(DisplayName = "Product  Workgroup")]
            [PXSelector(typeof(Search<EPCompanyTree.workGroupID,
                Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<AccessInfo.contactID>>>>),
             SubstituteKey = typeof(EPCompanyTree.description))]
            public virtual Int32? WorkGroupID
            {
                get
                {
                    return (_MyWorkGroup == true) ? null : _WorkGroupID;
                }
                set
                {
                    _WorkGroupID = value;
                }
            }
            #endregion
            #region MyWorkGroup
            public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }

            protected Boolean? _MyWorkGroup;
            [PXDefault(false)]
            [PXDBBool]
            [PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? MyWorkGroup
            {
                get
                {
                    return _MyWorkGroup;
                }
                set
                {
                    _MyWorkGroup = value;
                }
            }
            #endregion
            #region FilterSet
            public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }

            [PXDefault(false)]
            [PXDBBool]
            public virtual Boolean? FilterSet
            {
                get
                {
                    return
                        this.OwnerID != null ||
                        this.WorkGroupID != null ||
                        this.MyWorkGroup == true;
                }
            }
            #endregion
            #region ItemClassCD
            public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
            protected string _ItemClassCD;

            [PXDBString(30, IsUnicode = true)]
            [PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
            [PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
            public virtual string ItemClassCD
            {
                get { return this._ItemClassCD; }
                set { this._ItemClassCD = value; }
            }
            #endregion
            #region ItemClassCDWildcard
            public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
            [PXString(IsUnicode = true)]
            [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
            [PXDimension(INItemClass.Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(INItemClass.itemClassCD))]
            public virtual string ItemClassCDWildcard
            {
                get { return ItemClassTree.MakeWildcard(ItemClassCD); }
                set { }
            }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

            protected Int32? _InventoryID;
            [StockItem]
            public virtual Int32? InventoryID
            {
                get
                {
                    return this._InventoryID;
                }
                set
                {
                    this._InventoryID = value;
                }
            }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

            protected Int32? _SiteID;
            [Site(DisplayName = "Warehouse ID")]
            public virtual Int32? SiteID
            {
                get
                {
                    return this._SiteID;
                }
                set
                {
                    this._SiteID = value;
                }
            }
            #endregion
            #region RequestedOnStartDate
            public abstract class requestedOnStartDate : PX.Data.BQL.BqlDateTime.Field<requestedOnStartDate> { }

            protected DateTime? _RequestedOnStartDate;
            [PXDBDate]
            [PXUIField(DisplayName = "Requested On Start Date")]
            public virtual DateTime? RequestedOnStartDate
            {
                get
                {
                    return this._RequestedOnStartDate;
                }
                set
                {
                    this._RequestedOnStartDate = value;
                }
            }
            #endregion
            #region RequestedOnEndDate
            public abstract class requestedOnEndDate : PX.Data.BQL.BqlDateTime.Field<requestedOnEndDate> { }

            protected DateTime? _RequestedOnEndDate;
            [PXDBDate]
            [PXUIField(DisplayName = "Requested On End Date")]
            public virtual DateTime? RequestedOnEndDate
            {
                get
                {
                    return this._RequestedOnEndDate;
                }
                set
                {
                    this._RequestedOnEndDate = value;
                }
            }
            #endregion
            #region CustomerID
            public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

            protected Int32? _CustomerID;
            [Customer]
            public virtual Int32? CustomerID
            {
                get
                {
                    return this._CustomerID;
                }
                set
                {
                    this._CustomerID = value;
                }
            }
            #endregion
            #region SOOrderType
            public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }

            protected String _SOOrderType;
            [PXDBString(2, IsFixed = true, InputMask = ">aa")]
            [PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<boolTrue>>>))]
            [PXUIField(DisplayName = "SO Order Type", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String SOOrderType
            {
                get
                {
                    return this._SOOrderType;
                }
                set
                {
                    this._SOOrderType = value;
                }
            }
            #endregion
            #region SOOrderNbr
            public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }

            protected String _SOOrderNbr;
            [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "SO Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
            [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<ProductionOrdersCreateFilter.sOOrderType>>>>))]
            public virtual String SOOrderNbr
            {
                get
                {
                    return this._SOOrderNbr;
                }
                set
                {
                    this._SOOrderNbr = value;
                }
            }
            #endregion
            #region OrderType
            public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

            protected String _OrderType;
            [AMOrderTypeField(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Prod. Order Type")]
            [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
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
            [ProductionNbr(Visibility = PXUIVisibility.SelectorVisible)]
            [ProductionOrderSelector(typeof(ProductionOrdersCreateFilter.orderType), true)]
            [PX.Data.EP.PXFieldDescription]
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

        /// <summary>
        /// Specialized version of the Projection Attribute. Defines Projection as <br/>
        /// a select of INItemPlan Join INPlanType Join InventoryItem Join INUnit Left Join INItemSite <br/>
        /// filtered by InventoryItem.workgroupID and InventoryItem.productManagerID according to the values <br/>
        /// in the ProductionOrdersCreateFilter: <br/>
        /// 1. ProductionOrdersCreateFilter.ownerID is null or  ProductionOrdersCreateFilter.ownerID = InventoryItem.productManagerID <br/>
        /// 2. ProductionOrdersCreateFilter.workGroupID is null or  ProductionOrdersCreateFilter.workGroupID = InventoryItem.productWorkgroupID <br\>
        /// 3. ProductionOrdersCreateFilter.myWorkGroup = false or  InventoryItem.productWorkgroupID =InMember<ProductionOrdersCreateFilter.currentOwnerID> <br/>
        /// 4. InventoryItem.productWorkgroupID is null or  InventoryItem.productWorkgroupID =Owened<ProductionOrdersCreateFilter.currentOwnerID><br/>        
        /// </summary>
        public class ProductionOrdersCreateFilterProjectionAttribute : PX.TM.OwnedFilter.ProjectionAttribute
        {
            /// <summary>
            /// Default ctor
            /// </summary>
			public ProductionOrdersCreateFilterProjectionAttribute()
                : base(typeof(ProductionOrdersCreateFilter),
                BqlCommand.Compose(
            typeof(Select2<,,>),
                typeof(INItemPlan),
                typeof(InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemPlan.inventoryID>>,
                InnerJoin<INUnit, On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>, And<INUnit.fromUnit, Equal<InventoryItem.salesUnit>, And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>,
                LeftJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>,
                LeftJoin<SOLine, On<SOLineSplit.orderType, Equal<SOLine.orderType>, And<SOLineSplit.orderNbr, Equal<SOLine.orderNbr>, And<SOLineSplit.lineNbr, Equal<SOLine.lineNbr>>>>,
                LeftJoin<PX.Objects.IN.S.INItemSite, On<PX.Objects.IN.S.INItemSite.inventoryID, Equal<INItemPlan.inventoryID>, And<PX.Objects.IN.S.INItemSite.siteID, Equal<INItemPlan.siteID>>>>>>>>>),
            typeof(Where2<,>),
            typeof(Where<INItemPlan.fixedSource, Equal<INReplenishmentSource.manufactured>,
                      And<INPlanType.isFixed, Equal<True>, 
                      And<INPlanType.isDemand, Equal<True>,
                      And<Where<INItemPlan.supplyPlanID, IsNull,
                              And<Where<INItemPlan.planType, Equal<INPlanConstants.planM8>,
                                     Or<INItemPlan.planType, Equal<INPlanConstants.planMA>>>>>>>>>),
            typeof(And<>),
            PX.TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
            typeof(ProductionOrdersCreateFilter),
            typeof(InventoryItem.productWorkgroupID),
            typeof(InventoryItem.productManagerID))))
            {
            }
        }
    }

    [CreateProductionOrdersProcess.ProductionOrdersCreateFilterProjection]
    [Serializable]
    [PXCacheName(Messages.AMFixedDemand)]
    public class AMFixedDemand : INItemPlan
    {
        #region Selected
        public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        #endregion
        #region SiteID
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        #endregion
        #region PlanDate
        public new abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

        [PXDBDate]
        [PXDefault]
        [PXUIField(DisplayName = "Requested On")]
        public override DateTime? PlanDate
        {
            get
            {
                return this._PlanDate;
            }
            set
            {
                this._PlanDate = value;
            }
        }
        #endregion
        #region PlanID
        public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        #endregion
        #region FixedSource
        public new abstract class fixedSource : PX.Data.BQL.BqlString.Field<fixedSource> { }

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Fixed Source", Enabled = false)]
        [PXDefault(INReplenishmentSource.Manufactured, PersistingCheck = PXPersistingCheck.Nothing)]
        [INReplenishmentSource.INPlanList]
        public override String FixedSource
        {
            get
            {
                return this._FixedSource;
            }
            set
            {
                this._FixedSource = value;
            }
        }
        #endregion
        #region PlanType
        public new abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }

        [PXDBString(2, IsFixed = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Plan Type")]
        [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true, DescriptionField = typeof(INPlanType.descr))]
        public override String PlanType
        {
            get
            {
                return this._PlanType;
            }
            set
            {
                this._PlanType = value;
            }
        }
        #endregion
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        #endregion
        #region LocationID
        public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        #endregion
        #region PlanQty
        public new abstract class planQty : PX.Data.BQL.BqlDecimal.Field<planQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Requested Qty.")]
        public override Decimal? PlanQty
        {
            get
            {
                return this._PlanQty;
            }
            set
            {
                this._PlanQty = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDBString(BqlField = typeof(INUnit.fromUnit))]
        [PXUIField(DisplayName = "UOM")]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region UnitMultDiv
        public abstract class unitMultDiv : PX.Data.BQL.BqlString.Field<unitMultDiv> { }

        protected String _UnitMultDiv;
        [PXDBString(1, IsFixed = true, BqlField = typeof(INUnit.unitMultDiv))]
        public virtual String UnitMultDiv
        {
            get
            {
                return this._UnitMultDiv;
            }
            set
            {
                this._UnitMultDiv = value;
            }
        }
        #endregion
        #region UnitRate
        public abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }

        protected Decimal? _UnitRate;
        [PXDBDecimal(6, BqlField = typeof(INUnit.unitRate))]
        public virtual Decimal? UnitRate
        {
            get
            {
                return this._UnitRate;
            }
            set
            {
                this._UnitRate = value;
            }
        }
        #endregion
        #region PlanUnitQty
        public abstract class planUnitQty : PX.Data.BQL.BqlDecimal.Field<planUnitQty> { }

        protected Decimal? _PlanUnitQty;
        [PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>, Mult<INItemPlan.planQty, INUnit.unitRate>>, Div<INItemPlan.planQty, INUnit.unitRate>>), typeof(decimal))]
        [PXQuantity]
        public virtual Decimal? PlanUnitQty
        {
            get
            {
                return this._PlanUnitQty;
            }
            set
            {
                this._PlanUnitQty = value;
            }
        }
        #endregion
        #region OrderQty
        public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

        protected Decimal? _OrderQty;
        [PXQuantity]
        [PXUIField(DisplayName = "Quantity")]
        public virtual Decimal? OrderQty
        {
            [PXDependsOnFields(typeof(planUnitQty))]
            get
            {
                return this._OrderQty ?? this._PlanUnitQty;
            }
            set
            {
                this._OrderQty = value;
            }
        }
        #endregion
        #region RefNoteID
        public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        [PXRefNote]
        [PXUIField(DisplayName = "Reference Nbr.")]
        public override Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
        #region Hold
        public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        #endregion
        #region InventoryID_InventoryItem_descr
        public abstract class inventoryID_InventoryItem_descr : PX.Data.BQL.BqlString.Field<inventoryID_InventoryItem_descr> { }

        #endregion
        #region SiteID_INSite_descr
        public abstract class siteID_INSite_descr : PX.Data.BQL.BqlString.Field<siteID_INSite_descr> { }

        #endregion
        #region AddLeadTimeDays
        public abstract class addLeadTimeDays : PX.Data.BQL.BqlShort.Field<addLeadTimeDays> { }

        protected Int16? _AddLeadTimeDays;
        [PXShort]
        [PXUIField(DisplayName = "Add. Lead Time (Days)")]
        public virtual Int16? AddLeadTimeDays
        {
            get
            {
                return this._AddLeadTimeDays;
            }
            set
            {
                this._AddLeadTimeDays = value;
            }
        }
        #endregion
        #region AlternateID
        public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }

        protected String _AlternateID;
        [PXUIField(DisplayName = "Alternate ID")]
        [PXDBString(50, IsUnicode = true, InputMask = "", BqlField = typeof(SOLine.alternateID))]
        public virtual String AlternateID
        {
            get
            {
                return this._AlternateID;
            }
            set
            {
                this._AlternateID = value;
            }
        }
        #endregion
        #region SOOrderType
        public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }

        protected String _SOOrderType;
        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "SO Order Type", Enabled = false)]
        [PXSelector(typeof(Search<SOOrderType.orderType>))]
        public virtual String SOOrderType
        {
            get { return this._SOOrderType; }
            set { this._SOOrderType = value; }
        }
        #endregion
        #region SOOrderNbr
        public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }

        protected String _SOOrderNbr;
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "SO Order Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<SOOrder.orderNbr,
            Where<SOOrder.orderType, Equal<Optional<AMFixedDemand.sOOrderType>>>>))]
        public virtual String SOOrderNbr
        {
            get { return this._SOOrderNbr; }
            set { this._SOOrderNbr = value; }
        }
        #endregion
        #region SOLineNbr
        public abstract class sOLineNbr : PX.Data.BQL.BqlInt.Field<sOLineNbr> { }

        protected Int32? _SOLineNbr;
        [PXInt]
        [PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
        public virtual Int32? SOLineNbr
        {
            get { return this._SOLineNbr; }
            set { this._SOLineNbr = value; }
        }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        protected String _AMOrderType;
        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Prod. Order Type", Enabled = false)]
        [AMOrderTypeSelector]
        public virtual String AMOrderType
        {
            get { return this._AMOrderType; }
            set { this._AMOrderType = value; }
        }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        protected String _AMProdOrdID;
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Production Nbr.", Enabled = false)]
        [ProductionOrderSelector(typeof(AMFixedDemand.aMOrderType), true, ValidateValue = false)]
        public virtual String AMProdOrdID
        {
            get { return this._AMProdOrdID; }
            set { this._AMProdOrdID = value; }
        }
        #endregion
        #region AMOperationID
        public abstract class aMOperationID : PX.Data.BQL.BqlInt.Field<aMOperationID> { }

        protected Int32? _AMOperationID;
        [PXInt]
        [PXUIField(DisplayName = "Prod. Operation ID", Enabled = false)]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Optional<AMFixedDemand.aMOrderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<AMFixedDemand.aMProdOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        public virtual Int32? AMOperationID
        {
            get { return this._AMOperationID; }
            set { this._AMOperationID = value; }
        }
        #endregion
        #region AMLineID
        public abstract class aMLineID : PX.Data.BQL.BqlInt.Field<aMLineID> { }

        protected Int32? _AMLineID;
        [PXInt]
        [PXUIField(DisplayName = "Prod. LineID", Enabled = false)]
        public virtual Int32? AMLineID
        {
            get { return this._AMLineID; }
            set { this._AMLineID = value; }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false, Visible = false)]
        public virtual String Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion
    }
}