using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacture Production Attributes Inquiry
    /// </summary>
    public class ProductionAttributesInq : PXGraph<ProductionAttributesInq>
    {
        public PXFilter<ProductionAttributesFilter> Filter;
        public PXCancel<ProductionAttributesFilter> Cancel;

        public PXSelect<ProductionAttributeRecords> ProductionAttributes;

        [PXHidden]
        public PXSetup<AMPSetup> ampsetup;

        public PXAction<ProductionAttributesFilter> FirstOrder;
        public PXAction<ProductionAttributesFilter> PreviousOrder;
        public PXAction<ProductionAttributesFilter> NextOrder;
        public PXAction<ProductionAttributesFilter> LastOrder;

        public ProductionAttributesInq()
        {
            this.ProductionAttributes.Cache.AllowInsert = false;
            this.ProductionAttributes.Cache.AllowDelete = false;
            this.ProductionAttributes.Cache.AllowUpdate = false;
        }

        /// <summary>
        /// Open this graph using pxredirectexception
        /// </summary>
        public static void RedirectGraph(AMProdItem amProdItem)
        {
            RedirectGraph(amProdItem, PXRedirectHelper.WindowMode.New);
        }

        /// <summary>
        /// Open this graph using pxredirectexception
        /// </summary>
        public static void RedirectGraph(AMProdItem amProdItem, PXRedirectHelper.WindowMode windowMode)
        {
            if (amProdItem == null)
            {
                throw new PXArgumentException("amProdItem");
            }
            RedirectGraph(amProdItem.OrderType, amProdItem.ProdOrdID, windowMode);
        }

        /// <summary>
        /// Open this graph using pxredirectexception
        /// </summary>
        public static void RedirectGraph(string orderType, string prodOrdID)
        {
            RedirectGraph(orderType, prodOrdID, PXRedirectHelper.WindowMode.New);
        }

        /// <summary>
        /// Open this graph using pxredirectexception
        /// </summary>
        public static void RedirectGraph(string orderType, string prodOrdID, PXRedirectHelper.WindowMode windowMode)
        {
            if (string.IsNullOrWhiteSpace(orderType))
            {
                throw new PXArgumentException("orderType");
            }

            if (string.IsNullOrWhiteSpace(prodOrdID))
            {
                throw new PXArgumentException("prodOrdID");
            }

            var graph = PXGraph.CreateInstance<ProductionAttributesInq>();
            graph.Filter.Current.OrderType = orderType;
            graph.Filter.Current.ProdOrdID = prodOrdID;
            PXRedirectHelper.TryRedirect(graph, windowMode);
        }

        protected virtual IEnumerable productionAttributes()
        {
            return LoadAllData();
        }

        public virtual List<ProductionAttributeRecords> LoadAllData()
        {
            var productionAttributeRecords = new List<ProductionAttributeRecords>();

            if (Filter.Current == null || Filter.Current.OrderType == null || Filter.Current.ProdOrdID == null ||
                (Filter.Current.ShowOrderAttributes == false && Filter.Current.ShowTransactionAttributes == false))
            {
                return productionAttributeRecords;
            }

            var lineCounter = 1;

            if (Filter.Current.ShowOrderAttributes == true)
            {
                foreach (PXResult<AMProdAttribute, AMProdItem, InventoryItem> result in
                    PXSelectJoin<
                        AMProdAttribute, 
                        LeftJoin<AMProdItem,
                            On<AMProdAttribute.orderType, Equal<AMProdItem.orderType>,
                            And<AMProdAttribute.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                        LeftJoin<InventoryItem, 
                            On<AMProdItem.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                        Where<AMProdAttribute.orderType, Equal<Current<ProductionAttributesFilter.orderType>>,
                            And<AMProdAttribute.prodOrdID, Equal<Current<ProductionAttributesFilter.prodOrdID>>>>>
                        .Select(this))
                {
                    var prodAttribute = (AMProdAttribute)result;
                    var prodItem = (AMProdItem)result;
                    var inventoryItem = (InventoryItem)result;

                    var record = new ProductionAttributeRecords
                    {
                        LineNbr = ++lineCounter,
                        OrderType = prodAttribute.OrderType,
                        ProdOrdID = prodAttribute.ProdOrdID,
                        Level = prodAttribute.Level,
                        OperationID = prodAttribute.OperationID,
                        Source = prodAttribute.Source,
                        AttributeID = prodAttribute.AttributeID,
                        Label = prodAttribute.Label,
                        Descr = prodAttribute.Descr,
                        Enabled = prodAttribute.Enabled,
                        TransactionRequired = prodAttribute.TransactionRequired,
                        Value = prodAttribute.Value,
                        InventoryID = prodItem.InventoryID,
                        SubItemID = prodItem.SubItemID,
                        InventoryItemDescr = inventoryItem.Descr,
                        SiteID = prodItem.SiteID
                    };

                    productionAttributeRecords.Add(record);
                }
            }

            if (Filter.Current.ShowTransactionAttributes == true)
            {
                foreach (PXResult<AMMTranAttribute, AMMTran, AMProdAttribute, AMProdItem, InventoryItem> result in 
                    PXSelectJoin<
                        AMMTranAttribute, 
                        LeftJoin<AMMTran, 
                            On<AMMTranAttribute.docType, Equal<AMMTran.docType>, 
                            And<AMMTranAttribute.batNbr, Equal<AMMTran.batNbr>, 
                            And<AMMTranAttribute.tranLineNbr, Equal<AMMTran.lineNbr>>>>, 
                        LeftJoin<AMProdAttribute, 
                            On<AMMTranAttribute.orderType, Equal<AMProdAttribute.orderType>,
                            And<AMMTranAttribute.prodOrdID, Equal<AMProdAttribute.prodOrdID>,
                            And<AMMTranAttribute.prodAttributeLineNbr, Equal<AMProdAttribute.lineNbr>>>>, 
                        LeftJoin<AMProdItem, 
                            On<AMProdAttribute.orderType, Equal<AMProdItem.orderType>,
                            And<AMProdAttribute.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                        LeftJoin<InventoryItem, 
                            On<AMProdItem.inventoryID, Equal<InventoryItem.inventoryID>>>>>>,
                        Where<AMMTranAttribute.orderType, Equal<Current<ProductionAttributesFilter.orderType>>,
                            And<AMMTranAttribute.prodOrdID, Equal<Current<ProductionAttributesFilter.prodOrdID>>,
                            And<AMMTran.released, Equal<boolTrue>>>>>
                        .Select(this))
                {
                    var tranAttribute = (AMMTranAttribute)result;
                    var prodAttribute = (AMProdAttribute) result;
                    var tran = (AMMTran)result;
                    var prodItem = (AMProdItem) result;
                    var inventoryItem = (InventoryItem)result;

                    var record = new ProductionAttributeRecords
                    {
                        LineNbr = lineCounter,
                        OrderType = prodAttribute.OrderType,
                        ProdOrdID = prodAttribute.ProdOrdID,
                        Level = prodAttribute.Level,
                        OperationID = prodAttribute.OperationID,
                        Source = prodAttribute.Source,
                        AttributeID = prodAttribute.AttributeID,
                        Label = prodAttribute.Label,
                        Descr = prodAttribute.Descr,
                        Enabled = prodAttribute.Enabled,
                        TransactionRequired = prodAttribute.TransactionRequired,
                        Value = tranAttribute.Value,
                        DocType = tranAttribute.DocType,
                        BatNbr = tranAttribute.BatNbr,
                        TranLineNbr = tranAttribute.TranLineNbr,
                        TranOperationID = tran.OperationID,
                        Qty = tran.Qty,
                        TranDate = tran.TranDate,
                        InventoryID = prodItem.InventoryID,
                        SubItemID = prodItem.SubItemID,
                        InventoryItemDescr = inventoryItem.Descr,
                        SiteID = prodItem.SiteID
                    };

                    productionAttributeRecords.Add(record);
                    lineCounter += 1;
                }
            }

            return productionAttributeRecords;
        }

        protected virtual void ProductionAttributesFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            ProductionAttributesFilter row = (ProductionAttributesFilter)e.Row;
            ProductionAttributesFilter old = (ProductionAttributesFilter)e.OldRow;
            if (row.OrderType != old.OrderType
                || row.ProdOrdID != old.ProdOrdID
                || row.ShowTransactionAttributes != old.ShowTransactionAttributes
                || row.ShowOrderAttributes != old.ShowOrderAttributes)
            {
                ProductionAttributes.Cache.Clear();
            }
        }

        protected virtual void ProductionAttributesFilter_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ProductionAttributesFilter productionAttributesFilter = (ProductionAttributesFilter)e.Row;
            if (productionAttributesFilter == null || productionAttributesFilter.OrderType == null)
            {
                return;
            }

            productionAttributesFilter.ProdOrdID = null;
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXFirstButton]
        public virtual IEnumerable firstOrder(PXAdapter adapter)
        {
            if (Filter.Current == null || string.IsNullOrWhiteSpace(Filter.Current.OrderType))
            {
                return adapter.Get();
            }

            AMProdItem prodItem = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.planned>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>>,
                OrderBy<Asc<AMProdItem.prodOrdID>>>.SelectWindowed(this, 0, 1, Filter.Current.OrderType);

            if (prodItem != null)
            {
                Filter.Current.ProdOrdID = prodItem.ProdOrdID;
                ProductionAttributes.Cache.Clear(); //Required to refresh the details
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXPreviousButton]
        public virtual IEnumerable previousOrder(PXAdapter adapter)
        {
            if (Filter.Current == null || string.IsNullOrWhiteSpace(Filter.Current.OrderType))
            {
                return adapter.Get();
            }

            AMProdItem prodItem = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Less<Required<AMProdItem.prodOrdID>>,
                    And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.planned>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>>>,
                OrderBy<Desc<AMProdItem.prodOrdID>>>.SelectWindowed(this, 0, 1, Filter.Current.OrderType, Filter.Current.ProdOrdID);

            if (prodItem != null)
            {
                Filter.Current.ProdOrdID = prodItem.ProdOrdID;
                ProductionAttributes.Cache.Clear(); //Required to refresh the details
            }

            if (prodItem == null || string.IsNullOrWhiteSpace(Filter.Current.ProdOrdID))
            {
                return lastOrder(adapter);
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXNextButton]
        public virtual IEnumerable nextOrder(PXAdapter adapter)
        {
            if (Filter.Current == null || string.IsNullOrWhiteSpace(Filter.Current.OrderType))
            {
                return adapter.Get();
            }

            AMProdItem prodItem = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Greater<Required<AMProdItem.prodOrdID>>,
                    And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.planned>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>>>,
                OrderBy<Asc<AMProdItem.prodOrdID>>>.SelectWindowed(this, 0, 1, Filter.Current.OrderType, Filter.Current.ProdOrdID);

            if (prodItem != null)
            {
                Filter.Current.ProdOrdID = prodItem.ProdOrdID;
                ProductionAttributes.Cache.Clear(); //Required to refresh the details
            }

            if (prodItem == null || string.IsNullOrWhiteSpace(Filter.Current.ProdOrdID))
            {
                return firstOrder(adapter);
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLastButton]
        public virtual IEnumerable lastOrder(PXAdapter adapter)
        {
            if (Filter.Current == null || string.IsNullOrWhiteSpace(Filter.Current.OrderType))
            {
                return adapter.Get();
            }

            AMProdItem prodItem = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.planned>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>>,
                OrderBy<Desc<AMProdItem.prodOrdID>>>.SelectWindowed(this, 0, 1, Filter.Current.OrderType);

            if (prodItem != null)
            {
                Filter.Current.ProdOrdID = prodItem.ProdOrdID;
                ProductionAttributes.Cache.Clear(); //Required to refresh the details
            }
            return adapter.Get();
        }
    }

    /// <summary>
    /// Production Attributes inquiry filter
    /// </summary>
    [Serializable]
    [PXCacheName("Production Attributes Filter")]
    public class ProductionAttributesFilter : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
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

        protected string _ProdOrdID;
        [ProductionNbr(Visibility = PXUIVisibility.SelectorVisible)]
        [ProductionOrderSelector(typeof(ProductionAttributesFilter.orderType), true, DescriptionField = typeof(AMProdItem.descr))]
        public virtual string ProdOrdID
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
        #region ShowTransactionAttributes
        public abstract class showTransactionAttributes : PX.Data.BQL.BqlBool.Field<showTransactionAttributes> { }

        protected Boolean? _ShowTransactionAttributes;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Transaction Attributes")]
        public virtual Boolean? ShowTransactionAttributes
        {
            get
            {
                return this._ShowTransactionAttributes;
            }
            set
            {
                this._ShowTransactionAttributes = value;
            }
        }
        #endregion
        #region ShowOrderAttributes
        public abstract class showOrderAttributes : PX.Data.BQL.BqlBool.Field<showOrderAttributes> { }

        protected Boolean? _ShowOrderAttributes;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Order Attributes")]
        public virtual Boolean? ShowOrderAttributes
        {
            get
            {
                return this._ShowOrderAttributes;
            }
            set
            {
                this._ShowOrderAttributes = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Production Attributes inquiry detail
    /// </summary>
    [Serializable]
    [PXCacheName("Production Attribute Records")]
    public class ProductionAttributeRecords : IBqlTable
    {
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Doc Type", Visible = true, Enabled = false)]
        [AMDocType.List]
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
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Bat Nbr.", Visible = true, Enabled = false)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<Current<ProductionAttributeRecords.docType>>>>), 
            ValidateValue = false)]
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
        #region TranLineNbr
        public abstract class tranLineNbr : PX.Data.BQL.BqlInt.Field<tranLineNbr> { }

        protected Int32? _TranLineNbr;
        [PXInt]
        [PXUIField(DisplayName = "Tran Line Nbr.",Visible = false, Enabled = false)]
        public virtual Int32? TranLineNbr
        {
            get
            {
                return this._TranLineNbr;
            }
            set
            {
                this._TranLineNbr = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(Visible = false, Enabled = false, IsDBField = false)]
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

        protected string _ProdOrdID;
        [ProductionNbr(Visible = false, Enabled = false, IsDBField = false)]
        public virtual string ProdOrdID
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
        #region Level
        public abstract class level : PX.Data.BQL.BqlInt.Field<level> { }

        protected int? _Level;
        [PXInt]
        [PXUIField(DisplayName = "Level")]
        [AMAttributeLevels.ProdOrderList]
        public virtual int? Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        [OperationIDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<ProductionAttributeRecords.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<ProductionAttributeRecords.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        public virtual int? OperationID { get; set; }
        #endregion
        #region Source
        public abstract class source : PX.Data.BQL.BqlInt.Field<source> { }

        protected int? _Source;
        [PXInt]
        [PXUIField(DisplayName = "Source")]
        [AMAttributeSource.ProductionList]
        public virtual int? Source
        {
            get
            {
                return this._Source;
            }
            set
            {
                this._Source = value;
            }
        }
        #endregion
        #region AttributeID
        public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }

        protected string _AttributeID;
        [PXString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Attribute ID")]
        [PXSelector(typeof(CSAttribute.attributeID), ValidateValue = false)]
        public virtual string AttributeID
        {
            get
            {
                return this._AttributeID;
            }
            set
            {
                this._AttributeID = value;
            }
        }
        #endregion
        #region Label
        public abstract class label : PX.Data.BQL.BqlString.Field<label> { }

        protected string _Label;
        [PXString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Label")]
        public virtual string Label
        {
            get
            {
                return this._Label;
            }
            set
            {
                this._Label = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected string _Descr;
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr
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
        #region Enabled
        public abstract class enabled : PX.Data.BQL.BqlBool.Field<enabled> { }

        protected bool? _Enabled;
        [PXBool]
        [PXUIField(DisplayName = "Enabled")]
        public virtual bool? Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
            }
        }
        #endregion
        #region TransactionRequired
        public abstract class transactionRequired : PX.Data.BQL.BqlBool.Field<transactionRequired> { }

        protected bool? _TransactionRequired;
        [PXBool]
        [PXUIField(DisplayName = "Trans. Required")]
        public virtual bool? TransactionRequired
        {
            get
            {
                return this._TransactionRequired;
            }
            set
            {
                this._TransactionRequired = value;
            }
        }
        #endregion
        #region Value
        public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

        protected string _Value;
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Value")]
        [AMAttributeValue(typeof(ProductionAttributeRecords.attributeID))]
        public virtual string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
            }
        }
        #endregion
        #region TranOperationID
        public abstract class tranoperationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        [OperationIDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<ProductionAttributeRecords.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<ProductionAttributeRecords.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        public virtual int? TranOperationID { get; set; }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXQuantity]
        [PXUIField(DisplayName = "Move Qty")]
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
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDate]
        [PXUIField(DisplayName = "Tran. Date")]
        public virtual DateTime? TranDate
        {
            get
            {
                return this._TranDate;
            }
            set
            {
                this._TranDate = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem(Visible = false)]
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(typeof(ProductionAttributeRecords.inventoryID), Visible = false)]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region InventoryItemDescr
        public abstract class inventoryItemdescr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _InventoryItemDescr;
        [PXString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Inventory Description", Visible = false)]
        public virtual String InventoryItemDescr
        {
            get
            {
                return this._InventoryItemDescr;
            }
            set
            {
                this._InventoryItemDescr = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(Visible = false)]
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
    }
}