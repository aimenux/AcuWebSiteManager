using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Unreleased Allocated Production Material Process
    /// </summary>
    public class UnreleasedMaterialAllocations : PXGraph<UnreleasedMaterialAllocations>
    {
        public PXCancel<AMUnrelMaterialAllocationsFilter> Cancel;
        public PXAction<AMUnrelMaterialAllocationsFilter> Process;
        public PXAction<AMUnrelMaterialAllocationsFilter> ProcessAll;

        /// <summary>
        /// filter/search data view
        /// </summary>
        public PXFilter<AMUnrelMaterialAllocationsFilter> AMUnrelMaterialAllocationsFilterRecs;

        /// <summary>
        /// Grid Details data view
        /// </summary>
        public PXFilteredProcessingJoin<AMProdMatlSplit, 
            AMUnrelMaterialAllocationsFilter, 
            InnerJoin<AMProdItem, 
                On<AMProdItem.orderType, Equal<AMProdMatlSplit.orderType>,  
                    And<AMProdItem.prodOrdID, Equal<AMProdMatlSplit.prodOrdID>>>>,
            Where<AMProdMatlSplit.isAllocated, Equal<True>,
                And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                        Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                        Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>> UnrelMaterialAllocationsDetailRecs;
        
        public PXSelect<AMProdMatlSplit> AMUnrelMaterialAllocationsLineUpdateRecs;


        public UnreleasedMaterialAllocations()
        {
            UnrelMaterialAllocationsDetailRecs.SetSelected<AMProdMatlSplit.selected>();

            UnrelMaterialAllocationsDetailRecs.SetProcessDelegate(ProcessUnreleasedMaterialAllocations);
            PXUIFieldAttribute.SetVisible<AMProdMatlSplit.inventoryID>(UnrelMaterialAllocationsDetailRecs.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMProdMatlSplit.orderType>(UnrelMaterialAllocationsDetailRecs.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMProdMatlSplit.prodOrdID>(UnrelMaterialAllocationsDetailRecs.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMProdMatlSplit.operationID>(UnrelMaterialAllocationsDetailRecs.Cache, null, true);
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<AMProdMatlSplit.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<AMProdMatlSplit.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        protected virtual void AMProdMatlSplit_OperationID_CacheAttached(PXCache sender)
        {
        }

        [PX.Objects.AP.Vendor(typeof(Search<BAccountR.bAccountID,
            Where<PX.Objects.AP.Vendor.type, NotEqual<BAccountType.employeeType>>>))]
        protected virtual void AMProdMatlSplit_VendorID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [ProductionOrderSelector(typeof(AMProdMatlSplit.orderType), true)]
        protected virtual void AMProdMatlSplit_ProdOrdID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [ProductionOrderSelector(typeof(AMProdMatlSplit.aMOrderType), true)]
        protected virtual void AMProdMatlSplit_AMProdOrdID_CacheAttached(PXCache sender)
        {
        }

        protected virtual IEnumerable unrelMaterialAllocationsDetailRecs()
        {
            var currentFilter = this.AMUnrelMaterialAllocationsFilterRecs.Current;

            PXSelectBase<AMProdMatlSplit> cmd = new PXSelectReadonly2<AMProdMatlSplit,
                InnerJoin<AMProdItem, On<AMProdItem.orderType, Equal<AMProdMatlSplit.orderType>,
                    And<AMProdItem.prodOrdID, Equal<AMProdMatlSplit.prodOrdID>>>>,
                Where<AMProdMatlSplit.isAllocated, Equal<True>,
                    And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                            Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>>(this);

            if (currentFilter.InventoryID != null)
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.inventoryID, Equal<Current<AMUnrelMaterialAllocationsFilter.inventoryID>>>>();
            }
            if (currentFilter.InventoryID != null)
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.subItemID, Equal<Current<AMUnrelMaterialAllocationsFilter.subItemID>>>>();
            }
            if (currentFilter.SiteID != null)
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.siteID, Equal<Current<AMUnrelMaterialAllocationsFilter.siteID>>>>();
            }
            if (!string.IsNullOrEmpty(currentFilter.PONbr))
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.pOOrderNbr, Equal<Current<AMUnrelMaterialAllocationsFilter.pONbr>>>>();
            }
            if (!string.IsNullOrEmpty(currentFilter.ProductionOrderNbr))
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.prodOrdID, Equal<Current<AMUnrelMaterialAllocationsFilter.productionOrderNbr>>>>();
            }
            if (currentFilter.VendorID != null)
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.vendorID, Equal<Current<AMUnrelMaterialAllocationsFilter.vendorID>>>>();
            }
            if (!string.IsNullOrEmpty(currentFilter.ReceiptNbr))
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.pOReceiptNbr, Equal<Current<AMUnrelMaterialAllocationsFilter.receiptNbr>>>>();
            }
            if (!string.IsNullOrEmpty(currentFilter.SubAssyOrderType))
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.aMOrderType, Equal<Current<AMUnrelMaterialAllocationsFilter.subAssyOrderType>>>>();
            }
            if (!string.IsNullOrEmpty(currentFilter.SubAssyProdOrdID))
            {
                cmd.WhereAnd<Where<AMProdMatlSplit.aMProdOrdID, Equal<Current<AMUnrelMaterialAllocationsFilter.subAssyProdOrdID>>>>();
            }

            return cmd.Select();
        }

        protected static void ProcessUnreleasedMaterialAllocations(List<AMProdMatlSplit> selectedList)
        {
            var materialEntryGraph = CreateInstance<MaterialEntry>();
            materialEntryGraph.Clear();
            if (materialEntryGraph.ampsetup.Current != null)
            {
                materialEntryGraph.ampsetup.Current.RequireControlTotal = false;
            }

            try
            {
                var matlbuilder = new MaterialTranBuilder(materialEntryGraph);
                var ammTrans = new List<AMMTran>();

                foreach (var prodMatlSplit in selectedList)
                {
                    if (string.IsNullOrWhiteSpace(prodMatlSplit?.ProdOrdID))
                    {
                        continue;
                    }

                    var itemResult = InventoryHelper.GetItemLotSerClass(materialEntryGraph, prodMatlSplit.InventoryID);

                    var trans = matlbuilder.BuildMaterialIssueTransactions(
                        ToAMProdMatl(prodMatlSplit),
                        itemResult,
                        itemResult,
                        prodMatlSplit.Qty.GetValueOrDefault(),
                        prodMatlSplit.UOM,
                        prodMatlSplit.SiteID,
                        prodMatlSplit.LocationID,
                        out _);

                    if (trans != null && trans.Count > 0)
                    {
                        ammTrans.AddRange(trans);
                    }
                }

                MaterialTranBuilder.CreateMaterialTransaction(materialEntryGraph, ammTrans, null);

                materialEntryGraph.Persist();
            }
            catch (PXOuterException exception)
            {
                PXTraceHelper.PxTraceOuterException(exception, PXTraceHelper.ErrorLevel.Information);
                throw;
            }

            if (materialEntryGraph.batch.Current != null)
            {
                PXRedirectHelper.TryRedirect(materialEntryGraph, PXRedirectHelper.WindowMode.Same);
            }

            throw new PXException(Messages.NoBatchCreated);
        }

        protected static AMProdMatl ToAMProdMatl(AMProdMatlSplit split)
        {
            return new AMProdMatl
            {
                BaseQty = split.BaseQty.GetValueOrDefault(),
                QtyReq = split.Qty.GetValueOrDefault(),
                OrderType = split.OrderType,
                ProdOrdID = split.ProdOrdID,
                OperationID = split.OperationID,
                LineID = split.LineID,
                SortOrder = split.LineID,
                InventoryID = split.InventoryID,
                SubItemID = split.SubItemID,
                SiteID = split.SiteID,
                LocationID = split.LocationID,
                UOM = split.UOM,
                BFlush = false
            };
        }

        protected virtual void AMUnrelMaterialAllocationsFilter_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ((AMUnrelMaterialAllocationsFilter)e.Row).ProductionOrderNbr = null;
        }

        protected virtual void AMUnrelMaterialAllocationsFilter_SubAssyOrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ((AMUnrelMaterialAllocationsFilter)e.Row).SubAssyProdOrdID = null;
        }

        public PXAction<AMUnrelMaterialAllocationsFilter> ViewDetail;
        [PXLookupButton]
        [PXUIField(DisplayName = "View Detail")]
        protected virtual void viewDetail()
        {
            AMProdMatlSplit row = UnrelMaterialAllocationsDetailRecs.Current;
            ProdDetail graphDetail = PXGraph.CreateInstance<ProdDetail>();
            graphDetail.ProdItemRecords.Current = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>
                >>>.Select(this, row.OrderType, row.ProdOrdID);
            if (graphDetail.ProdItemRecords.Current != null)
            {
                throw new PXRedirectRequiredException(graphDetail, true, string.Empty);
            }
        }
    }

    #region Unreleased Material Allocations Filter (AMUnrelMaterialAllocationsFilter)
    [Serializable]
    [PXCacheName(Messages.UnreleasedMatlAllocationsFilter)]
    public class AMUnrelMaterialAllocationsFilter : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [Inventory(DisplayName = "Inventory ID")]
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
        [SubItem(typeof(AMUnrelMaterialAllocationsFilter.inventoryID))]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(DisplayName = "Warehouse")]
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
        #region PONbr
        public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }

        protected String _PONbr;
        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Order Nbr")]
        [PXSelector(typeof(Search<POOrder.orderNbr>), Filterable = true)]
        public virtual String PONbr
        {
            get
            {
                return this._PONbr;
            }
            set
            {
                this._PONbr = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        protected Int32? _VendorID;
        [POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName))]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(Required = false)]
        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
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
        #region ProductionOrderNbr
        public abstract class productionOrderNbr : PX.Data.BQL.BqlString.Field<productionOrderNbr> { }

        protected String _ProductionOrderNbr;
        [ProductionNbr]
        [ProductionOrderSelector(typeof(AMUnrelMaterialAllocationsFilter.orderType), includeAll: true)]
        public virtual String ProductionOrderNbr
        {
            get
            {
                return this._ProductionOrderNbr;
            }
            set
            {
                this._ProductionOrderNbr = value;
            }
        }
        #endregion
        #region ReceiptNbr
        public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }

        protected String _ReceiptNbr;
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Receipt Nbr")]
        [PXSelector(typeof(Search<POReceipt.receiptNbr>), Filterable = true)]
        public virtual String ReceiptNbr
        {
            get
            {
                return this._ReceiptNbr;
            }
            set
            {
                this._ReceiptNbr = value;
            }
        }
        #endregion
        #region SubAssyOrderType
        public abstract class subAssyOrderType : PX.Data.BQL.BqlString.Field<subAssyOrderType> { }

        protected String _SubAssyOrderType;
        [AMOrderTypeField(Required = false, DisplayName = "Sub. Assy. Order Type")]
        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String SubAssyOrderType
        {
            get
            {
                return this._SubAssyOrderType;
            }
            set
            {
                this._SubAssyOrderType = value;
            }
        }
        #endregion
        #region SubAssyProdOrdID
        public abstract class subAssyProdOrdID : PX.Data.BQL.BqlString.Field<subAssyProdOrdID> { }

        protected String _SubAssyProdOrdID;
        [ProductionNbr(DisplayName = "Sub. Assy. Production Nbr.")]
        [ProductionOrderSelector(typeof(AMUnrelMaterialAllocationsFilter.subAssyOrderType), includeAll: true)]
        public virtual String SubAssyProdOrdID
        {
            get
            {
                return this._SubAssyProdOrdID;
            }
            set
            {
                this._SubAssyProdOrdID = value;
            }
        }
        #endregion
    }
    #endregion
}