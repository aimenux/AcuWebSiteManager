using System;
using System.Collections;
using PX.Data;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.AP;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.CR;

namespace PX.Objects.AM
{
    /// <summary>
    /// Results of running MRP Regeneration. Displays planned recommendations
    /// </summary>
    public class MRPDisplay : PXGraph<MRPDisplay>
    {
        [PXFilterable]
        public PXSelect<AMRPDetail,
            Where<AMRPDetail.processed, NotEqual<True>>,
            OrderBy<Asc<AMRPDetail.actionDate>>> Detailrecs;

        public PXSetup<AMRPSetup> setup;
        public PXSetup<SOSetup> SOSetup;
        public PXSetup<POSetup> POSetup;
        public PXSetup<AMPSetup> ProdSetup;

        public PXFilter<PlanTransferFilter> PlanTransferOrderFilter;
        public PXFilter<PlanPurchaseFilter> PlanPurchaseOrderFilter;
        public PXFilter<PlanManufactureFilter> PlanManufactureOrderFilter;

        [PXHidden]
        public PXSelect<AMOrderCrossRef,
            Where<AMOrderCrossRef.userID, Equal<Current<AccessInfo.userID>>>,
            OrderBy<Asc<AMOrderCrossRef.siteID, Asc<AMOrderCrossRef.inventoryID, Asc<AMOrderCrossRef.subItemID, 
            Asc<AMOrderCrossRef.uOM, Asc<AMOrderCrossRef.planDate>>>>>>> ProcessRecords;

        [PXHidden]
        public PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID, Equal<Numbering.numberingID>>>,
                    Where<AMOrderType.orderType, Equal<Current<AMOrderType.orderType>>>> ProductionNumbering;

        [PXHidden]
        public PXSelectJoin<Numbering, LeftJoin<SOOrderType, On<SOOrderType.orderNumberingID, Equal<Numbering.numberingID>>>,
                    Where<SOOrderType.orderType, Equal<Current<PlanTransferFilter.orderType>>>> TransferNumbering;

        [PXHidden]
        public PXSelect<Numbering, Where<Numbering.numberingID, Equal<Current<POSetup.regularPONumberingID>>>> PurchaseNumbering;

        //For cache attached only
        [PXHidden]
        public PXSelect<AMSchdItem> SchdItem;

        // For cache attached
        [PXHidden]
        public PXSelect<AMProdOper> ProdOper;

        #region CacheAttahed

        //Changing the production order keys for display of related document
        [OperationIDField(IsKey = false, Visible = false, Enabled = false)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationID> e) { }

        //Changing the production order keys for display of related document
        [OperationCDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationCD> e) { }
        
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Schedule ID", Enabled = false, Visible = false)]
        protected virtual void _(Events.CacheAttached<AMSchdItem.schdID> e)
        {
            //Removing this field as a key so the display of Related Document looks better
        }

        #endregion

        public bool HasCrossRefRecords
        {
            get
            {
                AMOrderCrossRef xref = PXSelectReadonly<AMOrderCrossRef,
                        Where<AMOrderCrossRef.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);

                return xref != null;
            }
        }

        protected Numbering CurrentTransferNumbering
        {
            get
            {
                TransferNumbering.Current = PXSelectJoin
                    <Numbering, LeftJoin<SOOrderType, On<SOOrderType.orderNumberingID, Equal<Numbering.numberingID>>>,
                        Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>
                        >.Select(this, PlanTransferOrderFilter.Current.OrderType);
                return TransferNumbering.Current;
            }
        }

        protected Numbering GetProductionNumbering(string orderType)
        {
            Numbering numbering = PXSelectJoin
                    <Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID,
                        Equal<Numbering.numberingID>>>, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>
                        >.Select(this, orderType);
            
           return numbering;
        }

        protected Numbering CurrentPurchaseNumbering
        {
            get
            {
                PurchaseNumbering.Current = PXSelect
                    <Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>
                        >.Select(this, POSetup.Current.RegularPONumberingID);
                return PurchaseNumbering.Current;
            }
        }

        protected virtual void PlanTransferFilter_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            PlanTransferFilter row = (PlanTransferFilter) e.Row;
            if (row == null)
            {
                return;
            }

            row.OrderNbr = null;

            if (CurrentTransferNumbering != null && !CurrentTransferNumbering.UserNumbering.GetValueOrDefault())
            {
                row.OrderNbr = CurrentTransferNumbering.NewSymbol;
            }
        }

        protected virtual void PlanTransferFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            PlanTransferFilter row = (PlanTransferFilter) e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<PlanTransferFilter.orderNbr>(cache, row,
                CurrentTransferNumbering != null && CurrentTransferNumbering.UserNumbering.GetValueOrDefault());
        }

        public virtual void PlanTransferFilter_OrderNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            PlanTransferFilter planTransferFilter = (PlanTransferFilter) e.Row;

            if (planTransferFilter == null)
            {
                return;
            }

            if (CurrentTransferNumbering.UserNumbering == true)
            {
                SOOrder soOrder = PXSelect
                    <SOOrder, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                        And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>
                        >>.Select(this, planTransferFilter.OrderType, planTransferFilter.OrderNbr);
                if (soOrder != null)
                {
                    throw new PXSetPropertyException(Messages.GetLocal(Messages.TransferIDIsAlreadyUsed,
                            planTransferFilter.OrderNbr, planTransferFilter.OrderType));
                }
            }
        }

        protected virtual void PlanPurchaseFilter_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (PlanPurchaseFilter) e.Row;
            if (row == null)
            {
                return;
            }

            row.OrderNbr = null;

            if (CurrentPurchaseNumbering != null && !CurrentPurchaseNumbering.UserNumbering.GetValueOrDefault())
            {
                row.OrderNbr = CurrentPurchaseNumbering.NewSymbol;
            }
        }

        protected virtual void PlanPurchaseFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (PlanPurchaseFilter) e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<PlanPurchaseFilter.orderNbr>(cache, row,
                CurrentPurchaseNumbering != null && CurrentPurchaseNumbering.UserNumbering.GetValueOrDefault());
        }

        protected virtual void PlanPurchaseFilter_OrderNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var planPurchaseFilter = (PlanPurchaseFilter) e.Row;
            if (planPurchaseFilter == null)
            {
                return;
            }

            if (CurrentPurchaseNumbering.UserNumbering == true)
            {
                POOrder poOrder = PXSelect
                    <POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                        And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>
                        >>.Select(this, planPurchaseFilter.OrderType, planPurchaseFilter.OrderNbr);
                if (poOrder != null)
                {
                    throw new PXSetPropertyException(Messages.GetLocal(Messages.PurchaseOrderIDIsAlreadyUsed,
                            planPurchaseFilter.OrderNbr, planPurchaseFilter.OrderType));
                }
            }
        }

        #region Buttons

        public PXAction<AMRPDetail> PurchaseButton;
        [PXUIField(DisplayName = "Purchase", Enabled = true)]
        [PXButton()]
        public virtual IEnumerable purchaseButton(PXAdapter adapter)
        {
            if (!HasCrossRefRecords)
            {
                try
                {
                    GeneratePurchaseProcessRecords();
                }
                catch
                {
                    ResetOrderCrossRef();
                    throw;
                }
            }

            if (CurrentPurchaseNumbering != null && !CurrentPurchaseNumbering.UserNumbering.GetValueOrDefault())
            {
                PlanPurchaseOrderFilter.Current.OrderNbr = CurrentPurchaseNumbering.NewSymbol;
            }

            if (PlanPurchaseOrderFilter.AskExt(true) == WebDialogResult.OK)
            {
                try
                {
                    this.Persist();
                    GeneratePurchaseOrder();
                }
                catch
                {
                    ResetOrderCrossRef();
                    throw;
                }
            }

            ResetOrderCrossRef();

            return adapter.Get();
        }

        public PXAction<AMRPDetail> ManufactureButton;

        [PXUIField(DisplayName = "Manufacture", Enabled = true)]
        [PXButton()]
        public virtual IEnumerable manufactureButton(PXAdapter adapter)
        {
            if (!HasCrossRefRecords)
            {
                try
                {
                    GenerateManufactureProcessRecords();
                }
                catch
                {
                    ResetOrderCrossRef();
                    throw;
                }
            }

            if (PlanManufactureOrderFilter.AskExt(true) == WebDialogResult.OK)
            {
                try
                {
                    this.Persist();
                    GenerateProductionOrders();
                }
                catch
                {
                    ResetOrderCrossRef();
                    throw;
                }
            }

            ResetOrderCrossRef();

            return adapter.Get();
        }

        public PXAction<AMRPDetail> TransferButton;
        [PXUIField(DisplayName = "Transfer", Enabled = true)]
        [PXButton()]
        public virtual IEnumerable transferButton(PXAdapter adapter)
        {
            if (!HasCrossRefRecords)
            {
                try
                {
                    GenerateTransferProcessRecords();
                }
                catch
                {
                    ResetOrderCrossRef();
                    throw;
                }
            }

            if (CurrentTransferNumbering != null && !CurrentTransferNumbering.UserNumbering.GetValueOrDefault())
            {
                PlanTransferOrderFilter.Current.OrderNbr = CurrentTransferNumbering.NewSymbol;
            }

            if (PlanTransferOrderFilter.AskExt(true) == WebDialogResult.OK)
            {
                try
                {
                    this.Persist();
                    GenerateTransferOrder();
                }
                catch
                {
                    ResetOrderCrossRef();
                    throw;
                }
            }

            ResetOrderCrossRef();

            return adapter.Get();
        }

        public PXAction<AMRPDetail> inquiry;
        [PXUIField(DisplayName = Messages.Inquiries, MapEnableRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable Inquiry(PXAdapter adapter)
        {
            return adapter.Get();
        }


        public PXAction<AMRPDetail> MrpDetailInquiry;

        [PXUIField(DisplayName = Messages.MRPDetailInquiry, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable mrpDetailInquiry(PXAdapter adapter)
        {
            if (Detailrecs.Current != null)
            {
                MRPDetail.Redirect(new InvLookup
                {
                    InventoryID = Detailrecs.Current.InventoryID,
                    SiteID = Detailrecs.Current.SiteID,
                    SubItemID = AM.InventoryHelper.SubItemFeatureEnabled ? Detailrecs.Current.SubItemID : null
                });
            }

            return adapter.Get();
        }

        #endregion

        public MRPDisplay()
        {
            Detailrecs.AllowInsert = false;
            Detailrecs.AllowDelete = false;

            inquiry.AddMenuAction(MrpDetailInquiry);

            ProcessRecords.AllowInsert = false;
            ProcessRecords.AllowDelete = false;
            ProcessRecords.AllowUpdate = true;
        }

        public virtual void ProcessNotCompletedOkMsg()
        {
            ProcessCompletedOkMsg(Messages.ProcessError, Messages.CreateOrdersError);
        }

        /// <summary>
        /// Use to display a pop-up message to the user with an OK button
        /// </summary>
        /// <param name="header">Message box header</param>
        /// <param name="message"><Message</param>
        public virtual void ProcessCompletedOkMsg(string header, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            PXTrace.WriteInformation($"{header}: {message}");
            this.Detailrecs.Ask(header, message, MessageButtons.OK);
        }

        protected virtual void AMRPDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            //Disable all fields
            PXUIFieldAttribute.SetEnabled(cache, e.Row, false);

            //Then enable these fields only
            PXUIFieldAttribute.SetEnabled<AMRPDetail.preferredVendorID>(Detailrecs.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMRPDetail.replenishmentSource>(Detailrecs.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMRPDetail.selected>(Detailrecs.Cache, null, true);
        }

        protected virtual void AMOrderCrossRef_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            AMOrderCrossRef orderCrossRef = (AMOrderCrossRef)e.Row;

            if (orderCrossRef == null)
            {
                return;
            }

            if (orderCrossRef.OrderType == null)
            {
                ProcessRecords.Cache.RaiseExceptionHandling<AMOrderCrossRef.orderType>(
                    orderCrossRef,
                    orderCrossRef.OrderType,
                    new PXSetPropertyException<AMOrderCrossRef.orderType>(Messages.GetLocal(Messages.OrderTypeIsRequiredForProductionOrderCreation), 
                    PXErrorLevel.RowWarning));
            }

            PXUIFieldAttribute.SetEnabled<AMOrderCrossRef.prodOrdID>(cache, orderCrossRef, orderCrossRef.ManualNumbering.GetValueOrDefault());
        }

        protected virtual void AMOrderCrossRef_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            AMOrderCrossRef orderCrossRef = (AMOrderCrossRef)e.Row;

            if (orderCrossRef == null)
            {
                return;
            }

            Numbering numbering = GetProductionNumbering(orderCrossRef.OrderType);

            if (numbering != null && !numbering.UserNumbering.GetValueOrDefault())
            {
                orderCrossRef.ProdOrdID = numbering.NewSymbol;
                orderCrossRef.ManualNumbering = false;
            }
            else
            {
                orderCrossRef.ProdOrdID = String.Empty;
                orderCrossRef.ManualNumbering = true;
            }
        }

        protected virtual void AMOrderCrossRef_ProdOrdID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var orderCrossRef = (AMOrderCrossRef)e.Row;
            if (orderCrossRef == null)
            {
                return;
            }

            if (orderCrossRef.ManualNumbering == true)
            {
                AMProdItem amProdItem = PXSelect
                    <AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                        And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>
                        >>.Select(this, orderCrossRef.OrderType, orderCrossRef.ProdOrdID);
                if (amProdItem != null)
                {
                    throw new PXSetPropertyException(Messages.GetLocal(Messages.ProductionOrderIDIsAlreadyUsed,
                            orderCrossRef.ProdOrdID, orderCrossRef.OrderType));
                }

                AMOrderCrossRef amOrderCrossRef = PXSelect
                    <AMOrderCrossRef, Where<AMOrderCrossRef.orderType, Equal<Required<AMOrderCrossRef.orderType>>,
                        And<AMOrderCrossRef.prodOrdID, Equal<Required<AMOrderCrossRef.prodOrdID>>,
                            And<AMOrderCrossRef.lineNbr, NotEqual<Required<AMOrderCrossRef.lineNbr>>>
                            >>>.Select(this, orderCrossRef.OrderType, orderCrossRef.ProdOrdID, orderCrossRef.LineNbr);
                if (amOrderCrossRef != null)
                {
                    throw new PXSetPropertyException(Messages.GetLocal(Messages.ProductionOrderIDIsAlreadyUsedOrderCreate,
                            orderCrossRef.ProdOrdID, orderCrossRef.OrderType));
                }
            }
        }

        protected virtual void ResetOrderCrossRef()
        {
            DeleteProcessRecords();

            //Clear the cache for Purchase and Transfer Filters
            PlanPurchaseOrderFilter.Current = null;
            PlanPurchaseOrderFilter.Cache.Clear();
            PlanPurchaseOrderFilter.Cache.ClearQueryCache();

            PlanTransferOrderFilter.Current = null;
            PlanTransferOrderFilter.Cache.Clear();
            PlanTransferOrderFilter.Cache.ClearQueryCache();

            this.Actions.PressSave();
        }

        protected virtual void GeneratePurchaseProcessRecords()
        {
            int groupNumber = 0;
            int inventoryID = 0;
            int? subItemID = null;
            int siteID = 0;
            int windowDays = 0;
            string uom = string.Empty;
            DateTime? windowDate = null;

            DeleteProcessRecords();

            PlanPurchaseOrderFilter.Current.VendorID = null;

            Purchase(Detailrecs.Cache.Cached.Cast<AMRPDetail>().Where(row => row.Selected == true).ToList());

            foreach (AMOrderCrossRef crossRefRecord in ProcessRecords.Select())
            {
                if (crossRefRecord.InventoryID == inventoryID
                    && crossRefRecord.SubItemID.GetValueOrDefault() == subItemID.GetValueOrDefault()
                    && crossRefRecord.SiteID == siteID && crossRefRecord.UOM == uom
                    && windowDate != null)
                {
                    if (Common.Dates.Compare(crossRefRecord.PlanDate, windowDate) <= 0)
                    {
                        crossRefRecord.GroupNumber = groupNumber;
                    }
                    else
                    {
                        windowDate = crossRefRecord.PlanDate.GetValueOrDefault().AddDays(windowDays);
                        groupNumber++;
                        crossRefRecord.GroupNumber = groupNumber;
                    }
                    continue;
                }

                inventoryID = crossRefRecord.InventoryID.GetValueOrDefault();
                subItemID = crossRefRecord.SubItemID;
                siteID = crossRefRecord.SiteID.GetValueOrDefault();
                uom = crossRefRecord.UOM;
                windowDate = null;
                crossRefRecord.GroupNumber = null;

                windowDays = GetWindowDays(inventoryID, siteID);
                if (windowDays > 0)
                {
                    windowDate = crossRefRecord.PlanDate.GetValueOrDefault().AddDays(windowDays);
                    groupNumber++;
                    crossRefRecord.GroupNumber = groupNumber;
                }
            }

            Actions.PressSave();
        }

        protected virtual void GenerateTransferProcessRecords()
        {
            int groupNumber = 0;
            int inventoryID = 0;
            int? subItemID = null;
            int siteID = 0;
            int windowDays = 0;
            string uom = string.Empty;
            DateTime? windowDate = null;

            DeleteProcessRecords();

            PlanTransferOrderFilter.Current.FromSiteID = null;

            Transfer(Detailrecs.Cache.Cached.Cast<AMRPDetail>().Where(row => row.Selected == true).ToList());

            foreach (AMOrderCrossRef crossRefRecord in ProcessRecords.Select())
            {
                if (crossRefRecord.InventoryID == inventoryID
                    && crossRefRecord.SubItemID.GetValueOrDefault() == subItemID.GetValueOrDefault()
                    && crossRefRecord.SiteID == siteID && crossRefRecord.UOM == uom
                    && windowDate != null)
                {
                    if (Common.Dates.Compare(crossRefRecord.PlanDate, windowDate) <= 0)
                    {
                        crossRefRecord.GroupNumber = groupNumber;
                    }
                    else
                    {
                        windowDate = crossRefRecord.PlanDate.GetValueOrDefault().AddDays(windowDays);
                        groupNumber++;
                        crossRefRecord.GroupNumber = groupNumber;
                    }
                    continue;
                }

                inventoryID = crossRefRecord.InventoryID.GetValueOrDefault();
                subItemID = crossRefRecord.SubItemID;
                siteID = crossRefRecord.SiteID.GetValueOrDefault();
                uom = crossRefRecord.UOM;
                windowDate = null;
                crossRefRecord.GroupNumber = null;

                windowDays = GetWindowDays(inventoryID, siteID);
                if (windowDays > 0)
                {
                    windowDate = crossRefRecord.PlanDate.GetValueOrDefault().AddDays(windowDays);
                    groupNumber++;
                    crossRefRecord.GroupNumber = groupNumber;
                }
            }
            Actions.PressSave();
        }

        protected virtual void GenerateManufactureProcessRecords()
        {
            int groupNumber = 0;
            int inventoryID = 0;
            int? subItemID = null;
            int siteID = 0;
            int windowDays = 0;
            string uom = string.Empty;
            DateTime? windowDate = null;

            DeleteProcessRecords();

            Manufacture(Detailrecs.Cache.Cached.Cast<AMRPDetail>().Where(row => row.Selected == true).ToList());

            foreach (AMOrderCrossRef crossRefRecord in ProcessRecords.Select())
            {
                if (crossRefRecord.InventoryID == inventoryID
                    && crossRefRecord.SubItemID.GetValueOrDefault() == subItemID.GetValueOrDefault()
                    && crossRefRecord.SiteID == siteID && crossRefRecord.UOM == uom
                    && windowDate != null)
                {
                    if (Common.Dates.Compare(crossRefRecord.PlanDate, windowDate) <= 0)
                    {
                        crossRefRecord.GroupNumber = groupNumber;
                    }
                    else
                    {
                        windowDate = crossRefRecord.PlanDate.GetValueOrDefault().AddDays(windowDays);
                        groupNumber++;
                        crossRefRecord.GroupNumber = groupNumber;
                    }
                    continue;
                }

                inventoryID = crossRefRecord.InventoryID.GetValueOrDefault();
                subItemID = crossRefRecord.SubItemID;
                siteID = crossRefRecord.SiteID.GetValueOrDefault();
                uom = crossRefRecord.UOM;
                windowDate = null;
                crossRefRecord.GroupNumber = null;

                windowDays = GetWindowDays(inventoryID, siteID);
                if (windowDays > 0)
                {
                    windowDate = crossRefRecord.PlanDate.GetValueOrDefault().AddDays(windowDays);
                    groupNumber++;
                    crossRefRecord.GroupNumber = groupNumber;
                }
            }

            Actions.PressSave();
        }

        /// <summary>
        /// Lookup the window date in item warehouse/item order
        /// </summary>
        public virtual int GetWindowDays(int inventoryID, int siteID)
        {
            INItemSite inItemSite = PXSelect
                <INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(this, inventoryID, siteID);

            if (inItemSite != null)
            {
                INItemSiteExt extItemSite = PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);
                if (extItemSite != null && extItemSite.AMGroupWindow.GetValueOrDefault() > 0)
                {
                    return extItemSite.AMGroupWindow.GetValueOrDefault();
                }
            }

            InventoryItem inventoryItem = PXSelect
                <InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>
                    >>.Select(this, inventoryID);

            if (inventoryItem != null)
            {
                InventoryItemExt extInv =
                    PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);
                if (extInv != null && extInv.AMGroupWindow.GetValueOrDefault() > 0)
                {
                    return extInv.AMGroupWindow.GetValueOrDefault();
                }
            }

            return 0;
        }

        /// <summary>
        /// Delete old selected records if exist
        /// </summary>
        public virtual void DeleteProcessRecords()
        {
            foreach (AMOrderCrossRef orderCrossRef in PXSelectReadonly<AMOrderCrossRef,
                    Where<AMOrderCrossRef.userID, Equal<Current<AccessInfo.userID>>>>.Select(this))
            {
                this.ProcessRecords.Cache.Delete(orderCrossRef);
            }

            if (ProcessRecords.Cache.Deleted.Any_())
            {
                ProcessRecords.Cache.Persist(PXDBOperation.Delete);
            }
            ProcessRecords.Cache.Clear();
            ProcessRecords.Cache.ClearQueryCache();
        }

        protected virtual void MarkProcessed(List<AMOrderCrossRef> list)
        {
            if (list == null)
            {
                return;
            }

            foreach (AMOrderCrossRef amOrderCrossRef in list)
            {
                if ((amOrderCrossRef.DmdDetRecNbr ?? 0) == 0)
                {
                    continue;
                }

                //Make sure the record is not already recently processed by someone else
                AMRPDetail mrpDetail = PXSelectReadonly<AMRPDetail
                    , Where<AMRPDetail.recordID, Equal<Required<AMRPDetail.recordID>>>
                    >.Select(this, amOrderCrossRef.DmdDetRecNbr);

                if (mrpDetail == null || (mrpDetail.Processed ?? false))
                {
                    continue;
                }

                mrpDetail.Processed = true;

                Detailrecs.Update(mrpDetail);
            }
            Actions.PressSave();
        }

        protected static void MarkProcessed(PXGraph graph, int? demandDetailRecNbr)
        {
            if (demandDetailRecNbr == null)
            {
                return;
            }

            AMRPDetail mrpDetail = PXSelect<
                AMRPDetail, 
                Where<AMRPDetail.recordID, Equal<Required<AMRPDetail.recordID>>>
            >.Select(graph, demandDetailRecNbr);

            if (mrpDetail?.RecordID == null || mrpDetail.Processed == true)
            {
                return;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"RecordID = {mrpDetail.RecordID}");
#endif

            try
            {
                mrpDetail.Processed = true;
                graph.Caches<AMRPDetail>().PersistUpdated(graph.Caches<AMRPDetail>().Update(mrpDetail));
            }
            catch (Exception e)
            {
                //Maybe it was already marked as processed?
                PXTrace.WriteWarning(e);
            }
        }

        /// <summary>
        /// Generate the Purchase Order
        /// </summary>
        protected virtual void GeneratePurchaseOrder()
        {
            var purchaseOrderRefs = new List<AMOrderCrossRef>();
            var groupedPurchaseRefs = new List<AMOrderCrossRef>(); //used to mark as processed
            string lastGroup = string.Empty;
            string currentGroup = string.Empty;

            foreach (AMOrderCrossRef detail in PXSelect<AMOrderCrossRef,
                Where<AMOrderCrossRef.userID, Equal<Current<AccessInfo.userID>>>,
                OrderBy<Asc<AMOrderCrossRef.siteID, Asc<AMOrderCrossRef.inventoryID,
                Asc<AMOrderCrossRef.subItemID, Asc<AMOrderCrossRef.groupNumber>>>>>>.Select(this))
            {
                if (detail.Qty.GetValueOrDefault() == 0)
                {
                    continue;
                }

                if (detail.GroupNumber.GetValueOrDefault() == 0)
                {
                    purchaseOrderRefs.Add(detail);
                    continue;
                }

                currentGroup = string.Format("{0}{1}{2}{3}",
                    detail.SiteID.GetValueOrDefault(),
                    detail.InventoryID.GetValueOrDefault(),
                    detail.SubItemID.GetValueOrDefault(),
                    detail.GroupNumber.GetValueOrDefault());

                if (currentGroup.Equals(lastGroup))
                {
                    groupedPurchaseRefs.Add(detail);
                    continue;
                }

                AMOrderCrossRef resultAggregate = PXSelectGroupBy<AMOrderCrossRef,
                    Where<AMOrderCrossRef.siteID, Equal<Required<AMOrderCrossRef.siteID>>,
                        And<AMOrderCrossRef.inventoryID, Equal<Required<AMOrderCrossRef.inventoryID>>,
                            And<IsNull<AMOrderCrossRef.subItemID, int0>, Equal<Required<AMOrderCrossRef.subItemID>>,
                                And<AMOrderCrossRef.groupNumber, Equal<Required<AMOrderCrossRef.groupNumber>>>>>>,
                    Aggregate<Sum<AMOrderCrossRef.qty, Min<AMOrderCrossRef.planDate>>>
                    >.Select(this, detail.SiteID, detail.InventoryID, detail.SubItemID.GetValueOrDefault(),
                        detail.GroupNumber.GetValueOrDefault());

                if (resultAggregate.Qty.GetValueOrDefault() == 0)
                {
                    continue;
                }

                detail.Qty = resultAggregate.Qty;
                detail.PlanDate = resultAggregate.PlanDate;

                lastGroup = string.Format("{0}{1}{2}{3}",
                    detail.SiteID.GetValueOrDefault(),
                    detail.InventoryID.GetValueOrDefault(),
                    detail.SubItemID.GetValueOrDefault(),
                    detail.GroupNumber.GetValueOrDefault());
                    
                purchaseOrderRefs.Add(detail);
            }

            string purchaseOrderNbr = string.Empty;

            if ((PlanPurchaseOrderFilter.Current.VendorID ?? 0) != 0 && purchaseOrderRefs.Any_())
            {
                purchaseOrderNbr = ProcessPurchaseOrder(purchaseOrderRefs, PlanPurchaseOrderFilter.Current, CurrentPurchaseNumbering);
                
                if (purchaseOrderNbr != null)
                {
                    MarkProcessed(purchaseOrderRefs);
                    MarkProcessed(groupedPurchaseRefs);
                    ProcessCompletedOkMsg(Messages.CreatedPurchaseOrder, string.Format("Type '{0}' '{1}'", PlanPurchaseOrderFilter.Current.OrderType, purchaseOrderNbr));
                    return;
                }

                ProcessNotCompletedOkMsg();
            }
        }

        /// <summary>
        /// Create a single Purchase Order for each item in the passed list
        /// </summary>
        /// <param name="list">List of order cross ref records</param>
        /// <param name="filter">Current Plan Purchase filter</param>
        /// <param name="numbering">Current Purchase Order Numbering</param>
        /// <returns>PO Number Created</returns>
        public virtual string ProcessPurchaseOrder(List<AMOrderCrossRef> list, PlanPurchaseFilter filter, Numbering numbering)
        {
            if (filter == null)
            {
                throw new PXArgumentException("filter");
            }

            if (list == null || list.Count == 0)
            {
                return String.Empty;
            }

            MRPDisplay mrpGraph = CreateInstance<MRPDisplay>();
            mrpGraph.PlanPurchaseOrderFilter.Current = filter;

            //Verify filter here...
            var sb = new System.Text.StringBuilder();

            if (numbering != null
                && numbering.UserNumbering.GetValueOrDefault()
                && (string.IsNullOrWhiteSpace(filter.OrderNbr) || filter.OrderNbr.EqualsWithTrim(numbering.NewSymbol)))
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<CreatePurchaseOrdFilter.orderNbr>(mrpGraph.PlanPurchaseOrderFilter.Cache)));
            }

            if (filter.VendorID.GetValueOrDefault() == 0)
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<CreatePurchaseOrdFilter.vendorID>(mrpGraph.PlanPurchaseOrderFilter.Cache)));
            }

            if (string.IsNullOrWhiteSpace(filter.OrderType))
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<CreatePurchaseOrdFilter.orderType>(mrpGraph.PlanPurchaseOrderFilter.Cache)));
            }

            if (sb.Length != 0)
            {
                throw new PXException(sb.ToString());
            }

            string createdPurchaseOrder = null;

            createdPurchaseOrder = CreatePurchaseOrder(list, filter, numbering);

            return createdPurchaseOrder;
        }

        /// <summary>
        /// Create (persist) a new Purchase order
        /// </summary>
        /// <param name="list">List of order cross ref records</param>
        /// <param name="filter">PlanPurchaseFilter used for order creation</param>
        /// <param name="numbering">Create purchase order Numbering </param>
        /// <returns>true when successful in creation, false otherwise</returns>
        public virtual string CreatePurchaseOrder(List<AMOrderCrossRef> list, PlanPurchaseFilter filter, Numbering numbering)
        {
            string purchaseOrderNbr = String.Empty;

            if (list == null || list.Count == 0)
            {
                return purchaseOrderNbr;
            }

            var poOrderEntryGraph = CreateInstance<POOrderEntry>();
            poOrderEntryGraph.Clear();
            poOrderEntryGraph.Document.Current = null;

            DocumentList<POOrder> created = new DocumentList<POOrder>(poOrderEntryGraph);
            DocumentList<POLine> ordered = new DocumentList<POLine>(poOrderEntryGraph);

            using (PXTransactionScope scope = new PXTransactionScope())
            {
                foreach (var orderCrossRef in list)
                {
                    if (orderCrossRef.InventoryID.GetValueOrDefault() == 0 || orderCrossRef.SiteID.GetValueOrDefault() == 0 ||
                        orderCrossRef.Qty.GetValueOrDefault() == 0)
                    {
                        continue;
                    }

                    try
                    {
                        // Does PO Order Exist
                        var order = created.Find<POOrder.orderNbr>(purchaseOrderNbr) ?? new POOrder();

                        if (order.OrderNbr == null)
                        {
                            POOrder poOrder = (POOrder)poOrderEntryGraph.Document.Cache.CreateInstance();
                            poOrder.OrderType = filter.OrderType;

                            poOrder.OrderNbr = numbering != null && numbering.UserNumbering.GetValueOrDefault()
                                ? filter.OrderNbr : null;
                            POOrder copy1 =
                                PXCache<POOrder>.CreateCopy(poOrderEntryGraph.Document.Insert(poOrder));

                            if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
                            {
                                //GetValuePending will fail in CurrencyInfo_CuryIdFieldSelecting()
                                poOrderEntryGraph.currencyinfo.Current.CuryID = null;
                            }
                            copy1.VendorID = filter.VendorID;
                            copy1.VendorLocationID = filter.VendorLocationID;

                            PXCache<POOrder>.CreateCopy(poOrderEntryGraph.Document.Update(copy1));
                        }
                        else
                        {
                            //  PO Exists
                            if (poOrderEntryGraph.Document.Cache.ObjectsEqual(poOrderEntryGraph.Document.Current,
                                    order) == false)
                            {
                                poOrderEntryGraph.Document.Current = poOrderEntryGraph.Document.Search<POOrder.orderNbr>(order.OrderNbr,
                                        order.OrderType);
                            }
                        }

                        var line = InsertPOLine(poOrderEntryGraph, orderCrossRef);
                        if(line == null)
                        {
                            continue;
                        }

                        ordered.Add(line);

                        if (poOrderEntryGraph.Transactions.Cache.Inserted.Any_())
                        {
                            PersistPoOrder(poOrderEntryGraph);

                            if (created.Find(poOrderEntryGraph.Document.Current) == null)
                            {
                                created.Add(poOrderEntryGraph.Document.Current);
                                purchaseOrderNbr = created[0].OrderNbr;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        PXTraceHelper.PxTraceException(e);
                        throw;
                    }
                }

                scope.Complete();
            }

            return purchaseOrderNbr;
        }

        private static POLine InsertPOLine(POOrderEntry graph, AMOrderCrossRef amOrderCrossRef)
        {
            string lineType = POLineType.GoodsForInventory;

            Decimal? nullable = amOrderCrossRef.Qty;
            if ((!(nullable.GetValueOrDefault() <= new Decimal(0)) ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
            {
                return (POLine)null;
            }

            POLine poLine1 = (POLine)graph.Transactions.Cache.CreateInstance();
            poLine1.OrderType = graph.Document.Current.OrderType;
            poLine1.OrderNbr = graph.Document.Current.OrderNbr;
            poLine1.LineType = POLineType.Description;

            POLine copyX = graph.Transactions.Insert(poLine1);
            POLine copy1 = PXCache<POLine>.CreateCopy(copyX);
            copy1.LineType = lineType;
            copy1.InventoryID = amOrderCrossRef.InventoryID;

            graph.Transactions.Cache.RaiseFieldUpdated<POLine.inventoryID>(copy1, null);

            copy1.SubItemID = amOrderCrossRef.SubItemID;

            copy1.UOM = amOrderCrossRef.UOM;
            graph.Transactions.Cache.RaiseFieldUpdated<POLine.uOM>(copy1, null);
            graph.Transactions.Cache.SetDefaultExt<POLine.rcptQtyAction>(copy1);

            POLine poLine2 = graph.Transactions.Update(copy1);
            if (amOrderCrossRef.SiteID.HasValue)
                graph.Transactions.Cache.RaiseExceptionHandling<POLine.siteID>((object)poLine2, (object)null, (Exception)null);
            POLine copy2 = PXCache<POLine>.CreateCopy(poLine2);
            if (amOrderCrossRef.SiteID.HasValue)
            {
                graph.Transactions.Cache.RaiseExceptionHandling<POLine.siteID>((object)copy2, (object)null, (Exception)null);
                copy2.SiteID = amOrderCrossRef.SiteID;
            }
            copy2.OrderQty = amOrderCrossRef.Qty;
            graph.Transactions.Cache.RaiseFieldUpdated<POLine.uOM>(copy2, null);

            if (DateTime.Compare((amOrderCrossRef.PlanDate ?? Common.Dates.BeginOfTimeDate), (copy2.PromisedDate ?? Common.Current.BusinessDate(graph))) > 0)
            {
                //only set when date is greater than calculated promised date 
                copy2.PromisedDate = amOrderCrossRef.PlanDate;
            }

            POLine poLine3 = graph.Transactions.Update(copy2);

            return poLine3;
        }

        private static void PersistPoOrder(POOrderEntry graph)
        {
            POOrder poOrder1 = (POOrder)graph.Document.Cache.CreateCopy((object)graph.Document.Current);
            poOrder1.Hold = true;
            poOrder1.CuryControlTotal = poOrder1.CuryOrderTotal;
            graph.Document.Update(poOrder1);
            graph.Save.Press();
        }

        public void Purchase(List<AMRPDetail> list)
        {
            int? sharedVendorID = 0;
            bool singleVendorSelected = true;
            int? lineNbrCntr = 0;

            foreach (AMRPDetail detail in list)
            {
                lineNbrCntr++;

                var processRecord = new AMOrderCrossRef
                {
                    ProcessSource = PX.Objects.AM.Attributes.OrderCrossRefProcessSource.MRP,
                    LineNbr = lineNbrCntr,
                    UserID = Accessinfo.UserID,
                    InventoryID = detail.InventoryID,
                    SubItemID = detail.SubItemID,
                    SiteID = detail.SiteID,
                    PlanDate = detail.PromiseDate,
                    VendorID = detail.PreferredVendorID,
                    DmdDetRecNbr = detail.RecordID,
                    Source = detail.ReplenishmentSource,
                    UOM = detail.BaseUOM,
                    Qty = detail.BaseQty
                };

                // Update UOM to Purchase Unit 
                UpdateToPOUnit(processRecord);

                if ((sharedVendorID ?? 0) == 0
                    && (detail.PreferredVendorID ?? 0) != 0
                    && singleVendorSelected)
                {
                    sharedVendorID = detail.PreferredVendorID;
                }

                if ((sharedVendorID ?? 0) != 0
                    && (detail.PreferredVendorID ?? 0) != 0
                    && sharedVendorID != detail.PreferredVendorID)
                {
                    singleVendorSelected = false;
                }
               
                processRecord.ReferenceLineNbr = detail.RecordID;

                ProcessRecords.Insert(processRecord);
            }

            PlanPurchaseOrderFilter.Current.VendorID = null;
            PlanPurchaseOrderFilter.Current.VendorLocationID = null;
            if (singleVendorSelected && (sharedVendorID ?? 0) != 0)
            {
                PlanPurchaseOrderFilter.Current.VendorID = sharedVendorID;
                PlanPurchaseOrderFilter.Cache.RaiseFieldDefaulting<PlanPurchaseFilter.vendorLocationID>(
                    PlanPurchaseOrderFilter.Current, out object newDefault);
                PlanPurchaseOrderFilter.Current.VendorLocationID = (int?)newDefault;
            }

            if (ProcessRecords.Cache.Inserted.Any_()
                && ProcessRecords.Cache.IsDirty != true)
            {
                ProcessRecords.Cache.Persist(PXDBOperation.Insert);
            }
        }

        public void Transfer(List<AMRPDetail> list)
        {
            int? lineNbrCntr = 0;
            int? siteID = 0;

            foreach (AMRPDetail detail in list)
            {
                lineNbrCntr++;

                var processRecord = new AMOrderCrossRef
                {
                    ProcessSource = PX.Objects.AM.Attributes.OrderCrossRefProcessSource.MRP,
                    LineNbr = lineNbrCntr,
                    UserID = Accessinfo.UserID,
                    InventoryID = detail.InventoryID,
                    SubItemID = detail.SubItemID,
                    SiteID = detail.SiteID,
                    PlanDate = detail.PromiseDate,
                    DmdDetRecNbr = detail.RecordID,
                    Source = detail.ReplenishmentSource,
                    UOM = detail.BaseUOM,
                    Qty = detail.BaseQty
                };

                if (lineNbrCntr == 1)
                {
                    siteID = detail.SiteID;
                }

                if (detail.SiteID != siteID)
                {
                    throw new PXException(Messages.GetLocal(Messages.TransferCannotContainMultipleWarehouses));
                }

                processRecord.ReferenceLineNbr = detail.RecordID;

                ProcessRecords.Insert(processRecord);
            }

            PlanTransferOrderFilter.Current.ToSiteID = siteID;
            
            if (ProcessRecords.Cache.Inserted.Any_()
                && ProcessRecords.Cache.IsDirty != true)
            {
                ProcessRecords.Cache.Persist(PXDBOperation.Insert);
            }
        }

        public void Manufacture(List<AMRPDetail> list)
        {
            int? lineNbrCntr = 0;
            
            foreach (AMRPDetail detail in list)
            {
                lineNbrCntr++;

                var processRecord = new AMOrderCrossRef
                {
                    ProcessSource = OrderCrossRefProcessSource.MRP,
                    LineNbr = lineNbrCntr,
                    UserID = Accessinfo.UserID,
                    InventoryID = detail.InventoryID,
                    SubItemID = detail.SubItemID,
                    SiteID = detail.SiteID,
                    PlanDate = detail.PromiseDate,
                    DmdDetRecNbr = detail.RecordID,
                    Source = detail.ReplenishmentSource,
                    UOM = detail.BaseUOM,
                    Qty = detail.BaseQty
                };

                processRecord.ReferenceLineNbr = detail.RecordID;

                if (detail.Type == MRPPlanningType.MPS)
                {
                    processRecord.BOMID = detail.BOMID;
                }
                else if (!string.IsNullOrWhiteSpace(detail.BOMID))
                {
                    processRecord.BOMID = detail.BOMID;
                }

                processRecord.OrderType = ProdSetup.Current.DefaultOrderType;

                Numbering numbering = GetProductionNumbering(ProdSetup.Current.DefaultOrderType);

                if (numbering != null && !numbering.UserNumbering.GetValueOrDefault())
                {
                    processRecord.ProdOrdID = numbering.NewSymbol;
                    processRecord.ManualNumbering = false;
                }
                else
                {
                    processRecord.ManualNumbering = true;
                }

                ProcessRecords.Insert(processRecord);
            }

            if (ProcessRecords.Cache.Inserted.Any_()
                && ProcessRecords.Cache.IsDirty != true)
            {
                ProcessRecords.Cache.Persist(PXDBOperation.Insert);
            }

        }
        
        /// <summary>
        /// Create the Transfer Order
        /// </summary>
        protected virtual void GenerateTransferOrder()
        {
            var transferOrderRefs = new List<AMOrderCrossRef>();
            var groupedTransferRefs = new List<AMOrderCrossRef>(); //used to mark as processed
            string lastGroup = string.Empty;
            string currentGroup = string.Empty;

            foreach (AMOrderCrossRef detail in PXSelect<AMOrderCrossRef,
                Where<AMOrderCrossRef.userID, Equal<Current<AccessInfo.userID>>>,
                OrderBy<Asc<AMOrderCrossRef.siteID, Asc<AMOrderCrossRef.inventoryID,
                Asc<AMOrderCrossRef.subItemID, Asc<AMOrderCrossRef.groupNumber>>>>>>.Select(this))
            {
                if (detail.Qty.GetValueOrDefault() == 0)
                {
                    continue;
                }

                if (detail.GroupNumber.GetValueOrDefault() == 0)
                {
                    transferOrderRefs.Add(detail);
                    continue;
                }

                currentGroup = string.Format("{0}{1}{2}{3}",
                    detail.SiteID.GetValueOrDefault(),
                    detail.InventoryID.GetValueOrDefault(),
                    detail.SubItemID.GetValueOrDefault(),
                    detail.GroupNumber.GetValueOrDefault());

                if (currentGroup.Equals(lastGroup))
                {
                    groupedTransferRefs.Add(detail);
                    continue;
                }

                AMOrderCrossRef resultAggregate = PXSelectGroupBy<AMOrderCrossRef,
                    Where<AMOrderCrossRef.siteID, Equal<Required<AMOrderCrossRef.siteID>>,
                        And<AMOrderCrossRef.inventoryID, Equal<Required<AMOrderCrossRef.inventoryID>>,
                            And<IsNull<AMOrderCrossRef.subItemID, int0>, Equal<Required<AMOrderCrossRef.subItemID>>,
                                And<AMOrderCrossRef.groupNumber, Equal<Required<AMOrderCrossRef.groupNumber>>>>>>,
                    Aggregate<Sum<AMOrderCrossRef.qty, Min<AMOrderCrossRef.planDate>>>
                    >.Select(this, detail.SiteID, detail.InventoryID, detail.SubItemID.GetValueOrDefault(),
                        detail.GroupNumber.GetValueOrDefault());

                if (resultAggregate.Qty.GetValueOrDefault() == 0)
                {
                    continue;
                }

                detail.Qty = resultAggregate.Qty;
                detail.PlanDate = resultAggregate.PlanDate;

                lastGroup = string.Format("{0}{1}{2}{3}",
                    detail.SiteID.GetValueOrDefault(),
                    detail.InventoryID.GetValueOrDefault(),
                    detail.SubItemID.GetValueOrDefault(),
                    detail.GroupNumber.GetValueOrDefault());

                transferOrderRefs.Add(detail);
            }

            string transferOrderNbr = string.Empty;

            if ((PlanTransferOrderFilter.Current.FromSiteID ?? 0) != 0 && transferOrderRefs.Any_())
            {
                transferOrderNbr = ProcessTransferOrder(transferOrderRefs, PlanTransferOrderFilter.Current, CurrentTransferNumbering);

                if (transferOrderNbr != null)
                {
                    MarkProcessed(transferOrderRefs);
                    MarkProcessed(groupedTransferRefs);
                    ProcessCompletedOkMsg(Messages.CreatedTransferOrder, string.Format("Type '{0}' '{1}'", PlanTransferOrderFilter.Current.OrderType, transferOrderNbr));
                    return;
                }
                ProcessNotCompletedOkMsg();
            }
        }

        /// <summary>
        /// Create a single Transfer Order for each item in the passed list
        /// </summary>
        /// <param name="list">List of order cross ref records</param>
        /// <param name="filter">Current Plan Transfer </param>
        /// <param name="numbering">Current Transfer Order Numbering</param>
        /// <returns>SO Number Created</returns>
        public virtual string ProcessTransferOrder(List<AMOrderCrossRef> list, PlanTransferFilter filter, Numbering numbering)
        {
            if (filter == null)
            {
                throw new PXArgumentException("filter");
            }

            if (list == null || list.Count == 0)
            {
                return String.Empty;
            }

            MRPDisplay mrpGraph = CreateInstance<MRPDisplay>();
            mrpGraph.PlanTransferOrderFilter.Current = filter;

            //Verify filter here...
            var sb = new System.Text.StringBuilder();
            if (filter.ToSiteID.GetValueOrDefault() == 0)
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<TROrderFilter.toSiteID>(mrpGraph.PlanTransferOrderFilter.Cache)));
            }

            if (filter.FromSiteID.GetValueOrDefault() == 0)
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<TROrderFilter.fromSiteID>(mrpGraph.PlanTransferOrderFilter.Cache)));
            }

            if (string.IsNullOrWhiteSpace(filter.OrderType))
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<TROrderFilter.orderType>(mrpGraph.PlanTransferOrderFilter.Cache)));
            }
            else
            {
                var result = (PXResult<Numbering, SOOrderType>)PXSelectJoin<Numbering,
                    LeftJoin<SOOrderType, On<SOOrderType.orderNumberingID, Equal<Numbering.numberingID>>>,
                    Where<SOOrderType.orderType, Equal<Required<TROrderFilter.orderType>>>>.SelectWindowed(mrpGraph, 0, 1,
                        filter.OrderType);

                if (result != null)
                {
                    numbering = (Numbering)result;
                    var soOrderType = (SOOrderType)result;
                    if (soOrderType == null || !soOrderType.Active.GetValueOrDefault())
                    {
                        sb.AppendLine(PXMessages.Localize(PX.Objects.SO.Messages.OrderTypeInactive));
                    }
                }
            }

            if (numbering != null && numbering.UserNumbering.GetValueOrDefault()
                && (string.IsNullOrWhiteSpace(filter.OrderNbr) || filter.OrderNbr.EqualsWithTrim(numbering.NewSymbol)))
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<TROrderFilter.orderNbr>(mrpGraph.PlanTransferOrderFilter.Cache)));
            }

            if (sb.Length != 0)
            {
                throw new PXException(sb.ToString());
            }

            string createdTransferOrder = null;

            createdTransferOrder = CreateTransferOrder(list, filter, numbering);

            return createdTransferOrder;
        }

        /// <summary>
        /// Create (persist) a new Transfer order
        /// </summary>
        /// <param name="list">List of order cross ref records</param>
        /// <param name="filter">PlanPurchaseFilter used for order creation</param>
        /// <param name="numbering">Create transfer order Numbering </param>
        /// <returns>true when successful in creation, false otherwise</returns>
        public virtual string CreateTransferOrder(List<AMOrderCrossRef> list, PlanTransferFilter filter, Numbering numbering)
        {
            string transferOrderNbr = String.Empty;

            if (list == null || list.Count == 0)
            {
                return transferOrderNbr;
            }

            SOOrderEntry soOrderEntryGraph = PXGraph.CreateInstance<SOOrderEntry>();
            soOrderEntryGraph.Clear();
            soOrderEntryGraph.Document.Current = (SOOrder)null;

            DocumentList<SOOrder> created = new DocumentList<SOOrder>(soOrderEntryGraph);
            DocumentList<SOLine> ordered = new DocumentList<SOLine>(soOrderEntryGraph);

            using (PXTransactionScope scope = new PXTransactionScope())
            {
                var earliestPromiseDate = Common.Dates.EndOfTimeDate;
                var earliestShipDate = Common.Dates.EndOfTimeDate;

                foreach (var orderCrossRef in list)
                {
                    if (orderCrossRef.InventoryID.GetValueOrDefault() == 0 || orderCrossRef.SiteID.GetValueOrDefault() == 0 ||
                        orderCrossRef.Qty.GetValueOrDefault() == 0)
                    {
                        continue;
                    }

                    try
                    {
                        // Does SO Order Exist
                        var order = created.Find<SOOrder.orderNbr>(transferOrderNbr) ?? new SOOrder();

                        if (order.OrderNbr == null)
                        {
                            SOOrder soOrder = (SOOrder)soOrderEntryGraph.Document.Cache.CreateInstance();
                            soOrder.OrderType = filter.OrderType;

                            soOrder.OrderNbr = numbering != null && numbering.UserNumbering.GetValueOrDefault()
                                ? filter.OrderNbr : null;
                            SOOrder copy1 =
                                PXCache<SOOrder>.CreateCopy(soOrderEntryGraph.Document.Insert(soOrder));
                            copy1.DestinationSiteID = filter.ToSiteID;
                            soOrderEntryGraph.Document.Cache.RaiseFieldUpdated<SOOrder.destinationSiteID>(copy1, null);
                            PXCache<SOOrder>.CreateCopy(soOrderEntryGraph.Document.Update(copy1));
                        }
                        else
                        {
                            //  SO Exists
                            if (soOrderEntryGraph.Document.Cache.ObjectsEqual(soOrderEntryGraph.Document.Current,
                                    order) == false)
                            {
                                soOrderEntryGraph.Document.Current = soOrderEntryGraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr,
                                        order.OrderType);
                            }
                        }

                        var line = InsertSOLine(soOrderEntryGraph, orderCrossRef, filter.FromSiteID);

                        ordered.Add(line);

                        if (soOrderEntryGraph.Transactions.Select().Any())
                        {
                            // Set the Order Requested Date
                            earliestPromiseDate = earliestPromiseDate >= line.RequestDate.GetValueOrDefault() 
                                ? line.RequestDate.GetValueOrDefault() : earliestPromiseDate;
                            soOrderEntryGraph.Document.Current.RequestDate = earliestPromiseDate;

                            // Set the Order Ship on Date
                            earliestShipDate = earliestShipDate >= line.ShipDate.GetValueOrDefault()
                                ? line.ShipDate.GetValueOrDefault() : earliestShipDate;
                            soOrderEntryGraph.Document.Current.ShipDate = earliestShipDate;

                            PersistSoOrder(soOrderEntryGraph);
                            if (created.Find(soOrderEntryGraph.Document.Current) == null)
                            {
                                created.Add(soOrderEntryGraph.Document.Current);
                                transferOrderNbr = created[0].OrderNbr;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        PXTraceHelper.PxTraceException(e);
                        throw;
                    }
                }

                scope.Complete();
            }

            return transferOrderNbr;
        }

        protected virtual SOLine InsertSOLine(SOOrderEntry docgraph, AMOrderCrossRef amOrderCrossRef, int? siteID)
        {
            if (docgraph == null
                || amOrderCrossRef == null
                || amOrderCrossRef.Qty.GetValueOrDefault() <= 0
                || siteID == null)
            {
                return null;
            }

            try
            {
                SOLine tran = new SOLine();
                tran = docgraph.Transactions.Insert(tran);
                if (tran == null)
                {
                    return null;
                }

                tran.InventoryID = amOrderCrossRef.InventoryID;
                tran.SubItemID = amOrderCrossRef.SubItemID;
                //tran.TranDesc = amOrderCrossRef.Descr;
                tran.Qty = amOrderCrossRef.Qty;
                tran.UOM = amOrderCrossRef.UOM;
                tran.SiteID = siteID;
                tran.RequestDate = amOrderCrossRef.PlanDate;

                int transferLeadTime = InventoryHelper.GetTransferLeadTime(docgraph, tran.InventoryID, siteID);

                DateTime requestDate = (DateTime)tran.RequestDate;
                tran.ShipDate = requestDate.AddDays(-transferLeadTime);

                return docgraph.Transactions.Update(tran);
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);

                InventoryItem inventoryItem = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>
                    >.SelectWindowed(this, 0, 1, amOrderCrossRef.InventoryID);

                throw new PXException($"Unable to add Inventory ID {inventoryItem?.InventoryCD} to transfer order", e);
            }
        }

        private static void PersistSoOrder(SOOrderEntry graph)
        {
            SOOrder soOrder1 = (SOOrder)graph.Document.Cache.CreateCopy((object)graph.Document.Current);

            SOOrderType orderType = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>
                >>.Select(graph, soOrder1.OrderType);

            soOrder1.Hold = orderType?.HoldEntry == true;

            soOrder1.CuryControlTotal = soOrder1.CuryOrderTotal;
            graph.Document.Update(soOrder1);
            graph.Save.Press();
        }

        protected virtual void GenerateProductionOrders()
        {
            GenerateProductionOrders(GroupOrderCrossRef(PXSelect<AMOrderCrossRef,
                    Where<AMOrderCrossRef.userID, Equal<Current<AccessInfo.userID>>>,
                    OrderBy<Asc<AMOrderCrossRef.siteID, Asc<AMOrderCrossRef.inventoryID,
                        Asc<AMOrderCrossRef.subItemID, Asc<AMOrderCrossRef.groupNumber>>>>>>.Select(this)
                .ToFirstTableList()));
        }

        protected virtual void GenerateProductionOrders(List<AMOrderCrossRef> list)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }

            if (list.Count == 1)
            {
                PXLongOperation.StartOperation(this, () => 
                {
                    CreateProductionOrders(CreateInstance<ProdMaint>(), list);
                });
                return;
            }

            // ...
            // Continue for 2 or more orders to run in parallel
            // ...

            //Splits our list into 2 equal parts (last list to have more if odd number)
            var split = list.Split(2).ToArray();

            // Run 2 parallel jobs for creating production orders
            var uid1 = GenerateProductionOrdersInLongOperation(split[0].ToList());
            var uid2 = GenerateProductionOrdersInLongOperation(split[1].ToList());

            PXLongOperation.StartOperation(this, () =>
            {
                //This call is only used to show the processing time to the user in the UI
                PXLongOperation.WaitCompletion(uid1);
                PXLongOperation.WaitCompletion(uid2);
            });
        }

        private object GenerateProductionOrdersInLongOperation(List<AMOrderCrossRef> list)
        {
            var prodMaint = CreateInstance<ProdMaint>();
            PXLongOperation.StartOperation(prodMaint, () =>
            {
                CreateProductionOrders(prodMaint, list);
            });
            return prodMaint.UID;
        }

        public static void CreateProductionOrders(ProdMaint prodMaint, List<AMOrderCrossRef> list)
        {
            if (list == null || list.Count == 0 || prodMaint == null)
            {
                return;
            }

            foreach (var orderCrossRef in list)
            {
                try
                {
                    CreateProductionOrders(prodMaint, orderCrossRef);
                }
                catch (Exception exception)
                {
                    var recordNbrs = string.IsNullOrWhiteSpace(orderCrossRef.DmdDetGroupedRecNbrs)
                        ? orderCrossRef.DmdDetRecNbr.ToString()
                        : orderCrossRef.DmdDetGroupedRecNbrs;
                    PXTrace.WriteError($"Unable to create production order for AMRPDetail Record ID {recordNbrs}. {exception.Message}");
                    throw;
                }
            }
        }

        public static void CreateProductionOrders(ProdMaint prodMaint, AMOrderCrossRef orderCrossRef)
        {
            prodMaint.Clear();
            prodMaint.IsImport = true;

            if (orderCrossRef == null)
            {
                return;
            }

            var amProdItem = new AMProdItem { OrderType = orderCrossRef.OrderType };

            if (orderCrossRef.ManualNumbering == true)
            {
                AMProdItem amProdItemCheck = PXSelect
                    <AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                        And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>
                        >>.Select(prodMaint, orderCrossRef.OrderType, orderCrossRef.ProdOrdID);
                if (amProdItemCheck != null)
                {
                    throw new PXException(Messages.GetLocal(Messages.ProductionOrderIDIsAlreadyUsed,
                            orderCrossRef.ProdOrdID, orderCrossRef.OrderType));
                }
                amProdItem.ProdOrdID = orderCrossRef.ProdOrdID;
            }

            amProdItem.DetailSource = ProductionDetailSource.BOM;
            amProdItem.BOMEffDate = orderCrossRef.PlanDate;

            amProdItem = PXCache<AMProdItem>.CreateCopy(prodMaint.ProdMaintRecords.Insert(amProdItem));

            amProdItem.InventoryID = orderCrossRef.InventoryID;
            amProdItem.SubItemID = orderCrossRef.SubItemID;
            amProdItem.QtytoProd = orderCrossRef.Qty.GetValueOrDefault();
            amProdItem.UOM = orderCrossRef.UOM;
            amProdItem.SiteID = orderCrossRef.SiteID;
            amProdItem.LocationID = orderCrossRef.LocationID.GetValueOrDefault() == 0
                ? InventoryHelper.DfltLocation.GetDefault(prodMaint, InventoryHelper.DfltLocation.BinType.Receipt,
                    orderCrossRef.InventoryID, orderCrossRef.SiteID, true)
                : orderCrossRef.LocationID;

            amProdItem = PXCache<AMProdItem>.CreateCopy(prodMaint.ProdMaintRecords.Update(amProdItem));

            if (Common.Dates.Compare(orderCrossRef.PlanDate, prodMaint.Accessinfo.BusinessDate) < 0)
            {
                amProdItem.SchedulingMethod = ScheduleMethod.StartOn;
                amProdItem.ConstDate = prodMaint.Accessinfo.BusinessDate;
            }
            else
            {
                amProdItem.SchedulingMethod = ScheduleMethod.FinishOn;
                amProdItem.ConstDate = orderCrossRef.PlanDate;
            }

            amProdItem.BuildProductionBom = true;
            amProdItem.Reschedule = true;
            prodMaint.ProdMaintRecords.Update(amProdItem);

            prodMaint.Actions.PressSave();

            if (prodMaint?.ProdMaintRecords?.Current?.ProdOrdID == null)
            {
                PXTrace.WriteWarning("Order not created");
                return;
            }

            PXTrace.WriteInformation(Messages.GetLocal(Messages.CreatedProductionOrderFromMrp, prodMaint.ProdMaintRecords.Current.OrderType, prodMaint.ProdMaintRecords.Current.ProdOrdID));

            if (string.IsNullOrWhiteSpace(orderCrossRef.DmdDetGroupedRecNbrs))
            {
                MarkProcessed(prodMaint, orderCrossRef.DmdDetRecNbr);
                return;
            }

            foreach (var demandDetailNbr in orderCrossRef.DmdDetGroupedRecNbrs.Split(AMOrderCrossRef.DmdDetGroupedRecNbrsDelimiter.ToCharArray()))
            {
                if (int.TryParse(demandDetailNbr, out var asInt))
                {
                    MarkProcessed(prodMaint, asInt);
                }
            }
        }

        public static List<AMOrderCrossRef> GroupOrderCrossRef(List<AMOrderCrossRef> orderCrossRefs)
        {
            if (orderCrossRefs == null)
            {
                return null;
            }

            var groupedDemandNbr = new HashSet<int>();
            var grouped = new List<AMOrderCrossRef>();
            foreach (var orderCrossRef in orderCrossRefs)
            {
                if (!groupedDemandNbr.Add(orderCrossRef.DmdDetRecNbr.GetValueOrDefault()))
                {
                    continue;
                }

                if (orderCrossRef.GroupNumber == null)
                {
                    grouped.Add(orderCrossRef);
                    continue;
                }

                var related = orderCrossRefs.Where(x =>
                    x.SiteID == orderCrossRef.SiteID && x.InventoryID == orderCrossRef.InventoryID &&
                    x.SubItemID.GetValueOrDefault() == orderCrossRef.SubItemID.GetValueOrDefault() &&
                    x.GroupNumber == orderCrossRef.GroupNumber).ToList();

                if (related.Count <= 1)
                {
                    grouped.Add(orderCrossRef);
                    continue;
                }

                var groupedCrossRef = orderCrossRef;
                groupedCrossRef.BaseQty = 0;
                groupedCrossRef.Qty = 0;
                var demandNbrs = new List<int>();
                foreach (var crossRef in related)
                {
                    demandNbrs.Add(crossRef.DmdDetRecNbr.GetValueOrDefault());
                    groupedDemandNbr.Add(crossRef.DmdDetRecNbr.GetValueOrDefault());
                    if (groupedCrossRef.PlanDate == null ||
                        crossRef.PlanDate != null && crossRef.PlanDate.LessThan(groupedCrossRef.PlanDate))
                    {
                        groupedCrossRef.PlanDate = crossRef.PlanDate;
                    }

                    groupedCrossRef.BaseQty += crossRef.BaseQty.GetValueOrDefault();
                    //This works because MRP is always in BASE units ...
                    groupedCrossRef.Qty += crossRef.Qty.GetValueOrDefault();
                }
                groupedCrossRef.DmdDetGroupedRecNbrs = string.Join(AMOrderCrossRef.DmdDetGroupedRecNbrsDelimiter, demandNbrs.ToArray());
                grouped.Add(groupedCrossRef);
            }

            return grouped;
        }

        public virtual void UpdateToPOUnit(AMOrderCrossRef amOrderCrossRef)
        {
            if (amOrderCrossRef == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(amOrderCrossRef.UOM))
            {
                return;
            }

            var poUnit = GetPOUnit(amOrderCrossRef);
            if (amOrderCrossRef.UOM != poUnit)
            {
                PXCache cache = this.Caches<AMOrderCrossRef>();

                if (UomHelper.TryConvertFromToQty<AMOrderCrossRef.inventoryID>(cache, amOrderCrossRef,
                    amOrderCrossRef.UOM,
                    poUnit,
                    amOrderCrossRef.Qty.GetValueOrDefault(),
                    out var poQty))
                {
                    amOrderCrossRef.UOM = poUnit;
                    amOrderCrossRef.Qty = poQty.GetValueOrDefault();
                }
            }
        }

        public virtual string GetPOUnit(AMOrderCrossRef amOrderCrossRef)
        {
            return InventoryHelper.GetPurchaseOrderUnit(this, amOrderCrossRef.InventoryID, amOrderCrossRef.VendorID, amOrderCrossRef.UOM);
        }

    }

    [Serializable]
    [PXCacheName("Plan Transfer Filter")]
    public class PlanTransferFilter : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXString(IsFixed = true, InputMask = ">aa")]
        [PXDefault(typeof(SOSetup.transferOrderType), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof (Search5<SOOrderType.orderType, InnerJoin<SOOrderTypeOperation,
            On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
                And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
            LeftJoin<SOSetupApproval, On<SOOrderType.orderType, Equal<SOSetupApproval.orderType>>>>,
            Where<SOOrderTypeOperation.iNDocType, Equal<INTranType.transfer>>,
            Aggregate<GroupBy<SOOrderType.orderType>>>))]
        [PXRestrictor(typeof (Where<SOOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [PXUIField(DisplayName = "Order Type")]
        public virtual String OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }
        #endregion
        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

        protected String _OrderNbr;
        [PXString(IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Order Nbr.")]
        public virtual String OrderNbr
        {
            get { return this._OrderNbr; }
            set { this._OrderNbr = value; }
        }
        #endregion
        #region ToSiteID
        public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }

        protected Int32? _ToSiteID;
        [PXDefault]
        [Site(DisplayName = "To Warehouse", DescriptionField = typeof (INSite.descr))]
        public virtual Int32? ToSiteID
        {
            get { return this._ToSiteID; }
            set { this._ToSiteID = value; }
        }
        #endregion
        #region FromSiteID
        public abstract class fromSiteID : PX.Data.BQL.BqlInt.Field<fromSiteID> { }

        protected Int32? _FromSiteID;
        [PXDefault]
        [Site(DisplayName = "From Warehouse", DescriptionField = typeof (INSite.descr))]
        public virtual Int32? FromSiteID
        {
            get { return this._FromSiteID; }
            set { this._FromSiteID = value; }
        }
        #endregion
    }

    [Serializable]
    [PXCacheName("Plan Purchase Order Filter")]
    public class PlanPurchaseFilter : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXString(2, IsFixed = true)]
        [PXUnboundDefault(POOrderType.RegularOrder)]
        [POOrderType.List()]
        [PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual String OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }
        #endregion
        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

        protected String _OrderNbr;
        [PXString(IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Order Nbr.")]
        public virtual String OrderNbr
        {
            get { return this._OrderNbr; }
            set { this._OrderNbr = value; }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        protected Int32? _VendorID;
        [POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
        public virtual Int32? VendorID
        {
            get { return this._VendorID; }
            set { this._VendorID = value; }
        }
        #endregion
        #region VendorLocationID

        public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
        protected Int32? _VendorLocationID;
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<PlanPurchaseFilter.vendorID>>,
            And<Location.isActive, Equal<True>,
            And<MatchWithBranch<Location.vBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible,
            DisplayName = "Vendor Location")]
        [PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
            InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<PlanPurchaseFilter.vendorID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.vBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<PlanPurchaseFilter.vendorID>>,
            And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<PlanPurchaseFilter.vendorID>))]
        public virtual Int32? VendorLocationID
        {
            get
            {
                return this._VendorLocationID;
            }
            set
            {
                this._VendorLocationID = value;
            }
        }
        #endregion
    }

    [Serializable]
    [PXCacheName("Plan Production Order Filter")]
    public class PlanManufactureFilter : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }


        protected String _OrderType;
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
        [AMOrderTypeField]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }
        #endregion
    }
}