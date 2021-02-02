using System;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing production order print processing graph
    /// </summary>
    public class ProductionOrderPrintProcess : PXGraph
    {
        public PXFilter<ProductionOrderPrintProcessFilter> Filter;
        public PXCancel<ProductionOrderPrintProcessFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessingJoin<PrintProductionOrders, ProductionOrderPrintProcessFilter,
            InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMProdItem.inventoryID>>,
            LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>>,
            Where2<Where<INItemClass.itemClassCD, Like<Current<ProductionOrderPrintProcessFilter.itemClassCDWildcard>>, Or<Current<ProductionOrderPrintProcessFilter.itemClassCDWildcard>, IsNull>>,
            And2<Where<PrintProductionOrders.inventoryID, Equal<Current<ProductionOrderPrintProcessFilter.inventoryID>>, Or<Current<ProductionOrderPrintProcessFilter.inventoryID>, IsNull>>,
            And2<Where<PrintProductionOrders.siteID, Equal<Current<ProductionOrderPrintProcessFilter.siteID>>, Or<Current<ProductionOrderPrintProcessFilter.siteID>, IsNull>>,
            And2<Where<PrintProductionOrders.startDate, GreaterEqual<Current<ProductionOrderPrintProcessFilter.startDate>>, Or<Current<ProductionOrderPrintProcessFilter.startDate>, IsNull>>,
            And2<Where<PrintProductionOrders.endDate, LessEqual<Current<ProductionOrderPrintProcessFilter.endDate>>, Or<Current<ProductionOrderPrintProcessFilter.endDate>, IsNull>>,
            And2<Where<PrintProductionOrders.customerID, Equal<Current<ProductionOrderPrintProcessFilter.customerID>>, Or<Current<ProductionOrderPrintProcessFilter.customerID>, IsNull>>,
            And2<Where<PrintProductionOrders.ordTypeRef, Equal<Current<ProductionOrderPrintProcessFilter.sOOrderType>>, Or<Current<ProductionOrderPrintProcessFilter.sOOrderType>, IsNull>>,
            And2<Where<PrintProductionOrders.ordNbr, Equal<Current<ProductionOrderPrintProcessFilter.sOOrderNbr>>, Or<Current<ProductionOrderPrintProcessFilter.sOOrderNbr>, IsNull>>,
            And2<Where<PrintProductionOrders.orderType, Equal<Current<ProductionOrderPrintProcessFilter.orderType>>, Or<Current<ProductionOrderPrintProcessFilter.orderType>, IsNull>>,
            And2<Where<PrintProductionOrders.prodOrdID, Equal<Current<ProductionOrderPrintProcessFilter.prodOrdID>>, Or<Current<ProductionOrderPrintProcessFilter.prodOrdID>, IsNull>>,
            And2<Where<PrintProductionOrders.statusID, NotEqual<ProductionOrderStatus.closed>>,
            And2<Where<PrintProductionOrders.statusID, NotEqual<ProductionOrderStatus.cancel>>,
            And<Where<PrintProductionOrders.isReportPrinted, Equal<False>, Or<Current<ProductionOrderPrintProcessFilter.reprint>, Equal<True>>>>>>>>>>>>>>>>> ProductionOrders;
        
        [PXHidden]
        public PXSelect<AMProdEvnt> ProductionEvents;

        public PXSetup<AMPSetup> ampsetup;

        public ProductionOrderPrintProcess()
        {
            var setup = ampsetup.Current;
            AMPSetup.CheckSetup(setup);

            ProductionOrders.SetProcessDelegate(PrintOrders);
        }

        public static void PrintOrders(List<PrintProductionOrders> list)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }

            var graph = CreateInstance<ProductionOrderPrintProcess>();
            graph.MarkOrderAsPrinted(list);

            PXReportRequiredException ex = null;

            // Reports can change by OrderType and we want to process by reportID
            foreach (var prodItem in list.OrderBy(x => x.OrderType))
            {
                var parameters = graph.GetReportParameters(prodItem);
                var reportID = prodItem?.ProductionReportID ?? Reports.ProductionTicketReportParams.ReportID;

                ex = graph.BuildReportRequiredException(prodItem, parameters, reportID, ex);
            }

            if (graph.IsDirty)
            {
                graph.Persist();
            }

            if (ex != null)
            {
                throw ex;
            }
        }

        protected virtual PXReportRequiredException BuildReportRequiredException(PrintProductionOrders prodItem,
            Dictionary<string, string> parameters, string reportID, PXReportRequiredException ex)
        {
            return PXReportRequiredException.CombineReport(ex, reportID, parameters);
        }

        protected virtual Dictionary<string, string> GetReportParameters(PrintProductionOrders prodItem)
        {
            var parameters = new Dictionary<string, string>
            {
                [Reports.ReportHelper.GetDacFieldNameString<AMProdItem.orderType>()] = prodItem?.OrderType,
                [Reports.ReportHelper.GetDacFieldNameString<AMProdItem.prodOrdID>()] = prodItem?.ProdOrdID
            };
            return parameters;
        }

        protected virtual void MarkOrderAsPrinted(List<PrintProductionOrders> prodItems)
        {
            foreach (var prodItem in prodItems)
            {
                if (prodItem?.OrderType == null)
                {
                    continue;
                }

                CreatePrintReportProductionEvent(prodItem);

                if (prodItem.IsReportPrinted.GetValueOrDefault())
                {
                    continue;
                }

                prodItem.IsReportPrinted = true;

                //Copy cached updates
                var cachedProdItem = AMProdItem.PK.FindDirty(this, prodItem.OrderType, prodItem.ProdOrdID);
                if (cachedProdItem?.ProdOrdID != null)
                {
                    // new events for printing are correctly increasing the line counter but the persist on PrintProductionOrders projection is not allowing the value to stick
                    prodItem.LineCntrEvnt = cachedProdItem.LineCntrEvnt;
                }

                ProductionOrders.Update(prodItem);
            }
        }

        private void CopyCachedProdItemUpdates(PrintProductionOrders prodOrder)
        {
            var prodItem = AMProdItem.PK.FindDirty(this, prodOrder?.OrderType, prodOrder?.ProdOrdID);
            if (prodItem?.ProdOrdID == null)
            {
                return;
            }


        }

        protected virtual void CreatePrintReportProductionEvent(AMProdItem prodItem)
        {
            ProductionEventHelper.InsertReportPrintedEvent(this, prodItem);
        }

        protected virtual void ProductionOrderPrintProcessFilter_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ProductionOrderPrintProcessFilter)e.Row;
            if (row == null)
            {
                return;
            }

            row.ProdOrdID = null;
        }

        protected virtual void ProductionOrderPrintProcessFilter_SOOrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ProductionOrderPrintProcessFilter)e.Row;
            if (row == null)
            {
                return;
            }

            row.SOOrderNbr = null;
        }

        [Serializable]
        [PXCacheName("Print Process Filter")]
        public class ProductionOrderPrintProcessFilter : IBqlTable
        {
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
            [StockItem()]
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
            #region StartDate
            public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

            protected DateTime? _StartDate;
            [PXDBDate()]
            [PXUIField(DisplayName = "Start Date")]
            public virtual DateTime? StartDate
            {
                get
                {
                    return this._StartDate;
                }
                set
                {
                    this._StartDate = value;
                }
            }
            #endregion
            #region EndDate
            public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

            protected DateTime? _EndDate;
            [PXDBDate()]
            [PXUIField(DisplayName = "End Date")]
            public virtual DateTime? EndDate
            {
                get
                {
                    return this._EndDate;
                }
                set
                {
                    this._EndDate = value;
                }
            }
            #endregion
            #region CustomerID
            public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

            protected Int32? _CustomerID;
            [Customer()]
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
            [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<ProductionOrderPrintProcessFilter.sOOrderType>>>>))]
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
            [AMOrderTypeField(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Order Type")]
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
            [ProductionOrderSelector(typeof(ProductionOrderPrintProcessFilter.orderType), true)]
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
            #region Reprint
            public abstract class reprint : PX.Data.BQL.BqlBool.Field<reprint> { }

            /// <summary>
            /// Shows all orders whether already Printed or not 
            /// </summary>
            protected Boolean? _Reprint;
            [PXBool]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Reprint")]
            public virtual Boolean? Reprint
            {
                get
                {
                    return this._Reprint;
                }
                set
                {
                    this._Reprint = value;
                }
            }
            #endregion
        }

        /// <summary>
        /// Specialized version of the Projection Attribute. Defines Projection as <br/>
        /// a select of AMProdItem Join InventoryItem <br/>
        /// filtered by InventoryItem.workgroupID and InventoryItem.productManagerID according to the values <br/>
        /// in the ProductionOrderPrintProcessFilter: <br/>
        /// 1. ProductionOrderPrintProcessFilter.ownerID is null or  ProductionOrderPrintProcessFilter.ownerID = InventoryItem.productManagerID <br/>
        /// 2. ProductionOrderPrintProcessFilter.workGroupID is null or  ProductionOrderPrintProcessFilter.workGroupID = InventoryItem.productWorkgroupID <br/>
        /// 3. ProductionOrderPrintProcessFilter.myWorkGroup = false or  InventoryItem.productWorkgroupID =InMember<ProductionOrderPrintProcessFilter.currentOwnerID> <br/>
        /// 4. InventoryItem.productWorkgroupID is null or  InventoryItem.productWorkgroupID =Owened<ProductionOrderPrintProcessFilter.currentOwnerID><br/>        
        /// </summary>
        public class ProdOrderPrintProcessFilterProjectionAttribute : PX.TM.OwnedFilter.ProjectionAttribute
        {
			public ProdOrderPrintProcessFilterProjectionAttribute()
                : base(typeof(ProductionOrderPrintProcessFilter),
                BqlCommand.Compose(
            typeof(Select2<,,>),
                typeof(AMProdItem),
                typeof(InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMProdItem.inventoryID>>,
                    InnerJoin<AMOrderType, On<AMOrderType.orderType, Equal<AMProdItem.orderType>>>>),
            typeof(Where2<,>),
            typeof(Where<AMProdItem.statusID, NotEqual<ProductionOrderStatus.closed>,
                      And<AMProdItem.statusID, NotEqual<ProductionOrderStatus.cancel>>>),
            typeof(And<>),
            PX.TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
            typeof(ProductionOrderPrintProcessFilter),
            typeof(InventoryItem.productWorkgroupID),
            typeof(InventoryItem.productManagerID))))
            {
                Persistent = true;
                // Persistent tables
                _tables = new[] {typeof(AMProdItem) };
            }
        }
    }

    [ProductionOrderPrintProcess.ProdOrderPrintProcessFilterProjection]
    [Serializable]
    [PXCacheName("Print Production Orders")]
    public class PrintProductionOrders : AMProdItem
    {
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        public new abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        public new abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
        public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        public new abstract class ordTypeRef : PX.Data.BQL.BqlString.Field<ordTypeRef> { }
        public new abstract class ordNbr : PX.Data.BQL.BqlString.Field<ordNbr> { }
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        public new abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }
        public new abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }
        public new abstract class isReportPrinted : PX.Data.BQL.BqlBool.Field<isReportPrinted> { }

        #region ProductionReportID
        [PXDefault(Reports.ProductionTicketReportParams.ReportID, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBString(8, InputMask = "CC.CC.CC.CC", BqlField = typeof(AMOrderType.productionReportID))]
        [PXUIField(DisplayName = "Print Production Report ID", Visible = false)]
        public virtual String ProductionReportID { get; set; }
        public abstract class productionReportID : PX.Data.BQL.BqlString.Field<productionReportID> { }
        #endregion
    }
}