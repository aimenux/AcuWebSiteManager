using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Objects.AM.GraphExtensions;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.SO;
using PX.Objects.AM.Attributes;
using System.Linq;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.PO;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Production Order Maintenance Graph
    /// </summary>
    [PXCacheName(Messages.ProductionOrder)]
    public class ProdMaint : PXGraph<ProdMaint, AMProdItem>
    {
        [PXViewName(Messages.ProductionOrder)]
        [PXCopyPasteHiddenFields(typeof(AMProdItem.prodDate), typeof(AMProdItem.hold),
            typeof(AMProdItem.productOrderType), typeof(AMProdItem.productOrdID),
            typeof(AMProdItem.parentOrderType), typeof(AMProdItem.parentOrdID))]
        public PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Optional<AMProdItem.orderType>>>> ProdMaintRecords;
        [PXCopyPasteHiddenFields(typeof(AMProdItem.prodDate), typeof(AMProdItem.hold),
            typeof(AMProdItem.productOrderType), typeof(AMProdItem.productOrdID),
            typeof(AMProdItem.parentOrderType), typeof(AMProdItem.parentOrdID))]
        public PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdItemSelected;
        [PXCopyPasteHiddenView]
        public PXSelect<AMProdItemSplit, Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> splits;
        [PXHidden]
        public PXSelect<AMProdEvnt, Where<AMProdEvnt.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdEvnt.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdEventRecords;
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<AMProdItem.inventoryID>>>> itemrecord;
        [PXHidden]
        public PXSetup<AMPSetup> ampsetup;

        public PXSetup<AMOrderType>.Where<AMOrderType.orderType.IsEqual<AMProdItem.orderType.AsOptional>> amOrderType;
        [PXCopyPasteHiddenView]
        public PXSelect<AMProdTotal, Where<AMProdTotal.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdTotal.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdTotalRecs;
        [PXHidden]
        public PXSelect<AMProdOper, Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdOperRecords;
        [PXHidden]
        public PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdMatl.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdMatlRecords;
        [PXHidden]
        public PXSelect<AMProdStep, Where<AMProdStep.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdStep.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdStep.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdStepRecords;
        [PXHidden]
        public PXSelect<AMProdTool, Where<AMProdTool.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdTool.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdTool.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdToolRecords;
        [PXHidden]
        public PXSelect<AMProdOvhd, Where<AMProdOvhd.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdOvhd.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdOvhd.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdOvhdRecords;
        [PXHidden]
        public PXSelect<AMSchdItem, Where<AMSchdItem.orderType, Equal<Current<AMProdItem.orderType>>, And<AMSchdItem.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> SchdItemRecords;
        [PXHidden]
        public PXSelect<AMSchdOper, Where<AMSchdOper.orderType, Equal<Current<AMProdItem.orderType>>, And<AMSchdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> SchdOperRecords;
        [PXHidden]
        public PXSelect<AMSchdOperDetail, Where<AMSchdOperDetail.orderType, Equal<Current<AMProdItem.orderType>>, And<AMSchdOperDetail.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> SchdOperDetailRecords;
        [PXHidden]
        public PXSelect<AMWCSchd> WCSchdRecords;
        [PXHidden]
        public PXSelect<AMWCSchdDetail, Where<AMWCSchdDetail.schdKey, Equal<Current<AMSchdOperDetail.schdKey>>>> WCSchdDetailRecords;
        [PXHidden]
        public PXSelect<AMMachSchd> MachSchdRecords;
        [PXHidden]
        public PXSelect<AMMachSchdDetail, Where<AMMachSchdDetail.schdKey, Equal<Current<AMSchdOperDetail.schdKey>>>> MachSchdDetailRecords;
        [PXHidden]
        public PXSelect<AMToolSchdDetail, Where<AMToolSchdDetail.schdKey, Equal<Current<AMSchdOperDetail.schdKey>>>> ToolSchdDetailRecords;
        [PXImport(typeof(AMProdItem))]
        public PXSelect<AMProdAttribute, Where<AMProdAttribute.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdAttribute.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProductionAttributes;
        [PXHidden]
        public PXSelect<AMProdItemRelated> RelatedProdItems;
        
        [PXHidden]
        public PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID, Equal<Numbering.numberingID>>>,
                    Where<AMOrderType.orderType, Equal<Current<AMOrderType.orderType>>>> ProductionNumbering;

        /// <summary>
        /// Records changes to be made to the related sales order line
        /// </summary>
        [PXHidden]
        public PXFilter<SalesLineUpdate> SalesLineUpdateFilter;

        [PXHidden]
        public ConfigurationSelect<
            Where<AMConfigurationResults.prodOrderType,
                Equal<Current<AMProdItem.orderType>>,
            And<AMConfigurationResults.prodOrderNbr,
                Equal<Current<AMProdItem.prodOrdID>>>>> ItemConfiguration;

        public LSAMProdItem lsSelectItem;

        public PXSelect<AMProdMatlSplit,
            Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
            And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
            And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
            And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>> ProdMatlSplits;

        public LSAMProdMatl lsSelectMatl;

        public bool ContainsSOReference => ProdMaintRecords.Current != null
                                           && !string.IsNullOrWhiteSpace(ProdMaintRecords.Current.OrdTypeRef)
                                           && !string.IsNullOrEmpty(ProdMaintRecords.Current.OrdNbr)
                                           && ProdMaintRecords.Current.OrdLineRef != null;

        #region Cache Attached

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PX.Objects.CM.CurrencyInfo]
        protected virtual void AMConfigurationResults_CuryInfoID_CacheAttached(PXCache sender)
        {
        }

        #endregion

        #region Buttons

        public PXAction<AMProdItem> ActionDropMenu;
        [PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable actionDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMProdItem> InquiresDropMenu;
        [PXUIField(DisplayName = Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected IEnumerable inquiresDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMProdItem> report;
        [PXUIField(DisplayName = Messages.Reports, MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
        protected virtual IEnumerable Report(PXAdapter adapter,
            [PXString(8, InputMask = "CC.CC.CC.CC")]
            string reportID
            )
        {
            List<AMProdItem> list = adapter.Get<AMProdItem>().ToList();
            if(string.IsNullOrWhiteSpace(reportID))
            {
                return list;
            }

            Save.Press();

            PXReportRequiredException ex = null;

            foreach (var prodItem in list)
            {
                var parameters = GetReportParameters(prodItem, reportID);
                ex = PXReportRequiredException.CombineReport(ex, reportID, parameters);
                if (IsPrintProductionReportID(reportID))
                {
                    MarkReportPrinted(prodItem, reportID);
                }
            }

            if (ex != null)
            {
                if (ProdEventRecords.Cache.IsDirty)
                {
                    PersistBase();
                }

                throw ex;
            }

            return list;
        }

        protected virtual Dictionary<string, string> GetReportParameters(AMProdItem prodItem, string reportId)
        {
            var parameters = new Dictionary<string, string>();
            parameters[Reports.ReportHelper.GetDacFieldNameString<AMProdItem.orderType>()] = prodItem.OrderType;
            parameters[Reports.ReportHelper.GetDacFieldNameString<AMProdItem.prodOrdID>()] = prodItem.ProdOrdID;
            return parameters;
        }

        protected virtual bool IsPrintProductionReportID(string reportId)
        {
            AMOrderType ot = amOrderType?.Current ?? amOrderType.Select();
            if(ot == null || string.IsNullOrWhiteSpace(reportId))
            {
                return false;
            }

            return ot.ProductionReportID.EqualsWithTrim(reportId);
        }

        protected virtual void MarkReportPrinted(AMProdItem prodItem, string reportId)
        {
            if (prodItem?.OrderType == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(reportId))
            {
                throw new ArgumentNullException(nameof(reportId));
            }

            if (!prodItem.IsReportPrinted.GetValueOrDefault())
            {
                prodItem.IsReportPrinted = true;
                ProdMaintRecords.Update(prodItem);
            }

            CreatePrintReportProductionEvent(prodItem);
        }

        protected virtual void CreatePrintReportProductionEvent(AMProdItem prodItem)
        {
            ProductionEventHelper.InsertReportPrintedEvent(this, prodItem);
        }

        public PXAction<AMProdItem> plan;
        [PXUIField(DisplayName = Messages.PlanOrder, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXProcessButton]
        public virtual IEnumerable Plan(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                //if the graph is currently processing a long operation (most likely scheduling the order) then we need to wait until the process completes before we change the status
                this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                return adapter.Get();
            }

            if (!ProductionStatus.UserChangeOfStatusValid(ProdMaintRecords.Current, ProductionOrderStatus.Planned))
            {
                var strMsg = Messages.GetLocal(Messages.ProdStatusChangeInvalid,
                    ProductionOrderStatus.GetStatusDescription(ProdMaintRecords.Current.StatusID),
                    ProductionOrderStatus.GetStatusDescription(ProductionOrderStatus.Planned));

                if (ProdMaintRecords.Current.Hold.GetValueOrDefault())
                {
                    strMsg = Messages.GetLocal(Messages.ProdStatusOnHold);
                }
                throw new PXException(strMsg);
            }

            SetCurrentProductionStatus(ProductionOrderStatus.Planned);

            return adapter.Get();
        }

        public PXAction<AMProdItem> release;
        [PXUIField(DisplayName = Messages.ReleaseOrdAction, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                //if the graph is currently processing a long operation (most likely scheduling the order) then we need to wait until the process completes before we change the status
                this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                return adapter.Get();
            }

            if (!ProductionStatus.UserChangeOfStatusValid(ProdMaintRecords.Current, ProductionOrderStatus.Released))
            {
                var strMsg = Messages.GetLocal(Messages.ProdStatusChangeInvalid,
                    ProductionOrderStatus.GetStatusDescription(ProdMaintRecords.Current.StatusID.Trim()),
                    ProductionOrderStatus.GetStatusDescription(ProductionOrderStatus.Released));
                if (ProdMaintRecords.Current.Hold.GetValueOrDefault())
                {
                    strMsg = Messages.GetLocal(Messages.ProdStatusOnHold);
                }

                throw new PXException(strMsg);
            }

            SetCurrentProductionStatus(ProductionOrderStatus.Released);

            return adapter.Get();
        }

        public PXAction<AMProdItem> completeorder;
        [PXUIField(DisplayName = Messages.CompleteOrder, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXProcessButton]
        public virtual IEnumerable CompleteOrder(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                return adapter.Get();
            }

            if (!ProductionStatus.UserChangeOfStatusValid(ProdMaintRecords.Current, ProductionOrderStatus.Completed))
            {
                var strMsg = Messages.GetLocal(Messages.ProdStatusChangeInvalid,
                    ProductionOrderStatus.GetStatusDescription(ProdMaintRecords.Current.StatusID.Trim()),
                    ProductionOrderStatus.GetStatusDescription(ProductionOrderStatus.Completed));
                if (ProdMaintRecords.Current.Hold.GetValueOrDefault())
                {
                    strMsg = Messages.GetLocal(Messages.ProdStatusOnHold);
                }
                throw new PXException(strMsg);
            }

            //ASK USER TO CONFIRM COMPLETE...
            if (ProdMaintRecords.Ask(Messages.Confirm, Messages.ConfirmCompleteMessage, MessageButtons.YesNo) != WebDialogResult.Yes)
            {
                //...NO to CONFIRM COMPLETE
                return adapter.Get();
            }

            SetCurrentProductionStatus(ProductionOrderStatus.Completed);

            return adapter.Get();
        }

        public PXAction<AMProdItem> cancelorder;
        [PXUIField(DisplayName = Messages.CancelOrdAction, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXProcessButton]
        public virtual IEnumerable CancelOrder(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                //if the graph is currently processing a long operation (most likely scheduling the order) then we need to wait until the process completes before we change the status
                this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                return adapter.Get();
            }

            if (!ProductionStatus.UserChangeOfStatusValid(ProdMaintRecords.Current, ProductionOrderStatus.Cancel))
            {
                string strMsg = Messages.GetLocal(Messages.ProdStatusChangeInvalid,
                    ProductionOrderStatus.GetStatusDescription(ProdMaintRecords.Current.StatusID.Trim()),
                    ProductionOrderStatus.GetStatusDescription(ProductionOrderStatus.Cancel));

                if (ProdMaintRecords.Current.Hold.GetValueOrDefault())
                {
                    strMsg = Messages.GetLocal(Messages.ProdStatusOnHold);
                }
                throw new PXException(strMsg);
            }

            if (ProductionTransactionHelper.ProductionOrderHasUnreleasedTransactions(this, ProdMaintRecords.Current, out var unreleasedMsg))
            {
                throw new PXException(unreleasedMsg);
            }

            if (ProdMaintRecords.Ask(
                Messages.ConfirmCancelTitle,
                ContainsSOReference ? Messages.GetLocal(Messages.ConfirmCancelProductionOrderWithSORef) : Messages.GetLocal(Messages.ConfirmCancelProductionOrder),
                MessageButtons.YesNo) != WebDialogResult.Yes)
            {
                //...NO to CONFIRM CANCEL
                ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID,
                    new PXSetPropertyException($"{Messages.GetLocal(Messages.ConfirmCancelTitle)} {Messages.GetLocal(Messages.Canceled)}", PXErrorLevel.Warning));
                return adapter.Get();
            }

            SetCurrentProductionStatus(ProductionOrderStatus.Cancel);

            return adapter.Get();
        }

        public PXAction<AMProdItem> closeorder;

        [PXUIField(DisplayName = Messages.CloseOrder, MapEnableRights = PXCacheRights.Delete,
            MapViewRights = PXCacheRights.Delete, Visible = false)]
        [PXButton]
        public virtual IEnumerable CloseOrder(PXAdapter adapter)
        {
            var prodItem = ProdMaintRecords.Current;
            if (prodItem?.StatusID == null || prodItem.StatusID != ProductionOrderStatus.Completed)
            {
                return adapter.Get();
            }

            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(prodItem, prodItem.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                return adapter.Get();
            }

            var closeGraph = CreateInstance<CloseOrderProcess>();
            closeGraph.CompletedOrders.Select();
            var located = (AMProdItem)closeGraph.CompletedOrders.Cache.Locate(prodItem);
            if (located != null)
            {
                located.Selected = true;
                closeGraph.CompletedOrders.Update(located);
                PXRedirectHelper.TryRedirect(closeGraph, PXRedirectHelper.WindowMode.Same);
            }

            return adapter.Get();
        }

        public PXAction<AMProdItem> calculatePlanCost;
        [PXUIField(DisplayName = Messages.CalculatePlanCost, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXProcessButton]
        public virtual IEnumerable CalculatePlanCost(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current != null && (ProdMaintRecords.Current.StatusID == ProductionOrderStatus.Planned
                || ProdMaintRecords.Current.Hold.GetValueOrDefault()))
            {
                if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
                {
                    //if the graph is currently processing a long operation (most likely scheduling the order) then we need to wait until the process completes before we change the status
                    this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                    return adapter.Get();
                }

                InternalSave();

                UpdatePlannedMaterialCosts(ProdMaintRecords.Current);
                UpdatePlanProductionTotals();
                Persist();
            }
            return adapter.Get();
        }

        public PXAction<AMProdItem> ProductionDetails;
        [PXUIField(DisplayName = Messages.ProductionDetail, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable productionDetails(PXAdapter adapter)
        {
            if (this.ProdItemSelected.Current != null)
            {
                var pd = CreateInstance<ProdDetail>();
                var curr = this.ProdItemSelected.Current;
                pd.ProdItemRecords.Current = pd.ProdItemRecords.Search<AMProdItem.prodOrdID>(curr.ProdOrdID, curr.OrderType);
                throw new PXRedirectRequiredException(pd, Messages.ProductionDetail);
            }
            return adapter.Get();
        }

        public PXAction<AMProdItem> CriticalMatl;
        [PXUIField(DisplayName = Messages.CriticalMaterial, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable criticalMatl(PXAdapter adapter)
        {
            if (this.ProdItemSelected.Current != null)
            {
                CriticalMaterialsInq cm = PXGraph.CreateInstance<CriticalMaterialsInq>();
                cm.ProdItemRecs.Current.OrderType = this.ProdItemSelected.Current.OrderType;
                cm.ProdItemRecs.Current.ProdOrdID = this.ProdItemSelected.Current.ProdOrdID;
                cm.ProdItemRecs.Current.ShowAll = false;
                throw new PXRedirectRequiredException(cm, Messages.CriticalMaterial);
            }
            return adapter.Get();
        }

        public PXAction<AMProdItem> ViewSchedule;
        [PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        protected virtual IEnumerable viewSchedule(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            var gi = new GIWorkCenterSchedule();
            gi.SetParameter(GIWorkCenterSchedule.Parameters.OrderType, ProdMaintRecords.Current.OrderType);
            gi.SetParameter(GIWorkCenterSchedule.Parameters.ProductionNbr, ProdMaintRecords.Current.ProdOrdID);
            gi.SetParameter(GIWorkCenterSchedule.Parameters.DateFrom, ProdMaintRecords.Current.StartDate, this); //Set From Date in case schedule before today (parameter defaults to todays date)
            gi.SetParameter(GIWorkCenterSchedule.Parameters.DateTo, AMGenericInquiry.NullValue); //For some reason the GI when setting the from date will auto fill in the to date with the same value. Mark as null to force empty
            gi.CallGenericInquiry(PXBaseRedirectException.WindowMode.New);

            return adapter.Get();
        }

        public PXAction<AMProdItem> AttributesInq;
        [PXUIField(DisplayName = Messages.Attributes, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable attributesInq(PXAdapter adapter)
        {
            if (this.ProdItemSelected.Current != null)
            {
                ProductionAttributesInq.RedirectGraph(ProdItemSelected.Current);
            }
            return adapter.Get();
        }

        public PXAction<AMProdItem> TransactionsByProductionOrderInq;
        [PXUIField(DisplayName = "Production Transactions", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable transactionsByProductionOrderInq(PXAdapter adapter)
        {
            CallTransactionsByProductionOrderGenericInquiry(ProdMaintRecords.Current);

            return adapter.Get();
        }

        public PXAction<AMProdItem> InventoryAllocationDetailInq;
        [PXUIField(DisplayName = "Allocation Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable inventoryAllocationDetailInq(PXAdapter adapter)
        {
            if (ProdMaintRecords?.Current?.InventoryID != null)
            {
                var allocGraph = CreateInstance<InventoryAllocDetEnq>();
                allocGraph.Filter.Current.InventoryID = ProdMaintRecords.Current.InventoryID;
                if (InventoryHelper.SubItemFeatureEnabled)
                {
                    var subItem = (INSubItem)PXSelectorAttribute.Select<AMProdItem.subItemID>(ProdMaintRecords.Cache, ProdMaintRecords.Current);
                    if (!string.IsNullOrWhiteSpace(subItem?.SubItemCD))
                    {
                        allocGraph.Filter.Current.SubItemCD = subItem.SubItemCD;
                    }
                }
                allocGraph.Filter.Current.SiteID = ProdMaintRecords.Current.SiteID;
                allocGraph.RefreshTotal.Press();
                PXRedirectHelper.TryRedirect(allocGraph, PXRedirectHelper.WindowMode.New);
            }

            return adapter.Get();
        }

        protected virtual void CallTransactionsByProductionOrderGenericInquiry(AMProdItem prodItem)
        {
            if (string.IsNullOrWhiteSpace(prodItem?.OrderType) || string.IsNullOrWhiteSpace(prodItem.ProdOrdID))
            {
                return;
            }

            var gi = new GITransactionsByProductionOrder();
            gi.SetFilterByProductionOrder(prodItem.OrderType, prodItem.ProdOrdID);
            gi.CallGenericInquiry();
        }

        public PXAction<AMProdItem> CreatePurchaseOrderInq;
        [PXUIField(DisplayName = Messages.CreatePurchaseOrdersInq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable createPurchaseOrderInq(PXAdapter adapter)
        {
            var prodItem = ProdMaintRecords?.Current;
            if (prodItem?.InventoryID == null)
            {
                return adapter.Get();
            }

            var graph = CreateInstance<POCreate>();
            var filterExt = PXCache<POCreate.POCreateFilter>.GetExtension<POCreateFilterExt>(graph.Filter?.Current);
            if (filterExt == null)
            {
                return adapter.Get();
            }

            filterExt.AMOrderType = prodItem.OrderType;
            filterExt.ProdOrdID = prodItem.ProdOrdID;

            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);

            return adapter.Get();
        }

        public PXAction<AMProdItem> CreateProductionOrderInq;
        [PXUIField(DisplayName = Messages.CreateProductionOrdersInq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable createProductionOrderInq(PXAdapter adapter)
        {
            var prodItem = ProdMaintRecords?.Current;
            if (prodItem?.InventoryID == null)
            {
                return adapter.Get();
            }
            
            var graph = CreateInstance<CreateProductionOrdersProcess>();
            graph.Filter.Current.OrderType = prodItem.OrderType;
            graph.Filter.Current.ProdOrdID = prodItem.ProdOrdID;

            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);

            return adapter.Get();
        }

        public PXAction<AMProdItem> ConfigureEntry;
        [PXUIField(DisplayName = Messages.Configure, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable configureEntry(PXAdapter adapter)
        {
            if (this.ProdMaintRecords.Current.DetailSource != ProductionDetailSource.Configuration || this.ItemConfiguration.Current == null
                || this.ProdMaintRecords.Current.Function == OrderTypeFunction.Disassemble)
            {
                throw new PXException(Messages.NotConfigurableItem);
            }

            InternalSave();

            var graph = PXGraph.CreateInstance<ConfigurationEntry>();
            graph.Results.Current = graph.Results.Search<AMConfigurationResults.configResultsID>(this.ItemConfiguration.Current.ConfigResultsID);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

            return adapter.Get();
        }

        public PXAction<AMProdItem> Reconfigure;
        [PXUIField(DisplayName = Messages.DeleteConfig, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable reconfigure(PXAdapter adapter)
        {
            if (this.ItemConfiguration.Current != null &&
                !this.ItemConfiguration.Current.Closed.GetValueOrDefault() &&
                (this.ProdMaintRecords.Ask(Messages.ConfirmDeleteConfigResults, MessageButtons.YesNo) == WebDialogResult.Yes))
            {
                ItemConfiguration.Delete(this.ItemConfiguration.Current);
                string configurationID = null;
                if (ItemConfiguration.TryGetDefaultConfigurationID(ProdMaintRecords.Current.InventoryID, ProdMaintRecords.Current.SiteID, out configurationID))
                {
                    ItemConfiguration.Insert(new AMConfigurationResults
                    {
                        ConfigurationID = configurationID,
                        InventoryID = ProdMaintRecords.Current.InventoryID
                    });
                    return adapter.Get();
                }

                throw new PXException(Messages.NotActiveRevision);
            }

            return adapter.Get();
        }

        public PXAction<AMProdItem> releaseMaterial;
        [PXUIField(DisplayName = Messages.ReleaseMaterial, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXButton]
        public virtual IEnumerable ReleaseMaterial(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                //if the graph is currently processing a long operation (most likely scheduling the order) then we need to wait until the process completes before we change the status
                this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                return adapter.Get();
            }

            QuickReleaseMaterial();

            return adapter.Get();
        }

        public PXAction<AMProdItem> createMove;
        [PXUIField(DisplayName = Messages.CreateMove, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXButton]
        public virtual IEnumerable CreateMove(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            if (PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
            {
                //if the graph is currently processing a long operation (most likely scheduling the order) then we need to wait until the process completes before we change the status
                this.ProdMaintRecords.Cache.RaiseExceptionHandling<AMProdItem.statusID>(ProdMaintRecords.Current, ProdMaintRecords.Current.StatusID, new PXSetPropertyException(Messages.OrderUpdatingInLongProcess));
                return adapter.Get();
            }

            var moveGraph = CreateInstance<MoveEntry>();
            moveGraph.batch.Insert();
            var tran = moveGraph.transactions.Insert();

            if(tran == null)
            {
                return adapter.Get();
            }

            tran.OrderType = ProdMaintRecords.Current.OrderType;
            tran.ProdOrdID = ProdMaintRecords.Current.ProdOrdID;

            moveGraph.transactions.Update(tran);

            PXRedirectHelper.TryRedirect(moveGraph, PXRedirectHelper.WindowMode.NewWindow);

            return adapter.Get();
        }

        public PXAction<AMProdItem> createLinkedOrders;
        [PXUIField(DisplayName = Messages.AutoCreateLinkedOrders, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXProcessButton]
        public virtual IEnumerable CreateLinkedOrders(PXAdapter adapter)
        {
            if (ProdMaintRecords.Current == null)
            {
                return adapter.Get();
            }

            InternalSave();

            var createdOrderList = new List<AMProdItem>();
            PXLongOperation.StartOperation(this, delegate ()
            {
                try
                {
                    createdOrderList = CriticalMaterialsInq.CreateLinkedProductionOrders(ProdMaintRecords.Current, createdOrderList);
                    PXTrace.WriteInformation(CriticalMaterialsInq.CreateListOfOrders(createdOrderList)?.ToString());
                }
                catch
                {
                    Actions.PressCancel();
                    throw;
                }
            });

            return adapter.Get();
        }

        public PXAction<AMProdItem> disassemble;
        [PXUIField(DisplayName = Messages.Disassemble, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable Disassemble(PXAdapter adapter)
        {
            if (ProdItemSelected.Current != null)
            {
                PXRedirectHelper.TryRedirect(DisassemblyEntry.ConstructNewDisassemblyEntry(ProdItemSelected.Current), PXRedirectHelper.WindowMode.New);
            }
            return adapter.Get();
        }

        #endregion

        public bool FixedMfgTimesEnabled
        {
            get
            {
                if (ampsetup.Current == null)
                {
                    return true;
                }

                return !string.IsNullOrWhiteSpace(ampsetup.Current.FixMfgCalendarID);
            }
        }

        /// <summary>
        /// Based on the current production order - launch the material wizard 2 screen to release material
        /// </summary>
        protected virtual void QuickReleaseMaterial()
        {
            if (ProdMaintRecords.Current == null
                || !ProductionStatus.IsValidTransactionStatus(ProdMaintRecords.Current))
            {
                return;
            }

            MatlWizard1.FillMatlWrk(ProdMaintRecords.Current);
        }

        protected virtual void SetCurrentProductionStatus(string newStatusID)
        {
            if (ProdMaintRecords.Current == null
                || string.IsNullOrWhiteSpace(newStatusID))
            {
                return;
            }

            InternalSave();

            var wipVarBatches = new List<AMBatch>();
            try
            {
                using (var setProdStatusScope = new PXTransactionScope())
                {
                    var wipAdjustmentEntry = ProductionStatus.SetProductionStatus(this, ProdMaintRecords.Current, newStatusID);

                    if (newStatusID == ProductionOrderStatus.Cancel)
                    {
                        RemovePlanReferences(ProdMaintRecords.Current);
                    }

                    InternalSave();

                    if (wipAdjustmentEntry?.batch.Current != null && wipAdjustmentEntry.transactions.Select().Any_())
                    {
                        wipAdjustmentEntry.Persist();
                        wipVarBatches.Add(wipAdjustmentEntry.batch.Current);
                    }
                    setProdStatusScope.Complete();
                }
            }
            catch (Exception)
            {
                Actions.PressCancel();
                throw;
            }
            if (wipVarBatches.Count > 0)
            {
                AMDocumentRelease.ReleaseDoc(wipVarBatches);
            }
#if DEBUG
            AMDebug.TraceDirtyCaches(this);
#endif
        }

        #region HANDLE HOLD CHECKBOX
        protected virtual void AMProdItem_Hold_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var api = (AMProdItem)e.Row;

            if (string.IsNullOrEmpty(api?.StatusID) || ProductionOrderStatus.CanHold(api.StatusID) || !(bool)e.NewValue)
            {
                return;
            }

            throw new PXSetPropertyException(Messages.GetLocal(Messages.ProdStatusHoldInvalid, ProductionOrderStatus.GetStatusDescription(api.StatusID)), PXErrorLevel.Warning);
        }

        protected virtual void AMProdItem_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMProdItem)e.Row;
            if (row == null || e.OldValue == null || row.Hold.GetValueOrDefault() == (bool)e.OldValue)
            {
                return;
            }

            if (row.Hold.GetValueOrDefault())
            {
                //To Hold
                ProductionEventHelper.InsertStatusEvent(this, row, row.StatusID, ProductionOrderStatus.Hold);
                return;
            }

            //From Hold
            ProductionEventHelper.InsertStatusEvent(this, row, ProductionOrderStatus.Hold, row.StatusID);
        }

        #endregion

        public ProdMaint()
        {
            ActionDropMenu.AddMenuAction(plan);
            ActionDropMenu.AddMenuAction(release);
            ActionDropMenu.AddMenuAction(completeorder);
            ActionDropMenu.AddMenuAction(cancelorder);
            ActionDropMenu.AddMenuAction(closeorder);
            ActionDropMenu.AddMenuAction(calculatePlanCost);
            ActionDropMenu.AddMenuAction(releaseMaterial);
            ActionDropMenu.AddMenuAction(createMove);
            ActionDropMenu.AddMenuAction(createLinkedOrders);
            InquiresDropMenu.AddMenuAction(ProductionDetails);
            InquiresDropMenu.AddMenuAction(CriticalMatl);
            InquiresDropMenu.AddMenuAction(AttributesInq);
            InquiresDropMenu.AddMenuAction(TransactionsByProductionOrderInq);
            InquiresDropMenu.AddMenuAction(InventoryAllocationDetailInq);
            InquiresDropMenu.AddMenuAction(CreatePurchaseOrderInq);
            InquiresDropMenu.AddMenuAction(CreateProductionOrderInq);
            InquiresDropMenu.AddMenuAction(ViewSchedule);

            var setup = ampsetup.Current;
            AMPSetup.CheckSetup(setup);

            splits.AllowDelete = false;

            var disassemblySetup = ampsetup?.Current?.DisassemblyNumberingID != null;
            disassemble.SetVisible(disassemblySetup);
            if (disassemblySetup)
            {
                ActionDropMenu.AddMenuAction(disassemble);
            }

            PXUIFieldAttribute.SetVisible<AMProdItem.fMLTime>(ProdMaintRecords.Cache, null, FixedMfgTimesEnabled);
        }

        protected virtual void DeleteSchdOperDetail(AMProdOper prodOper)
        {
            if (prodOper == null || string.IsNullOrWhiteSpace(prodOper.ProdOrdID))
            {
                return;
            }

            // Multiple queries because tools and machine can be multiple per AMSchdOperDetail

            ProductionScheduleEngine.DeleteMachSchdDetail(MachSchdDetailRecords.Cache, prodOper);
            ProductionScheduleEngine.DeleteToolSchdDetail(ToolSchdDetailRecords.Cache, prodOper);

            foreach (PXResult<AMSchdOperDetail, AMWCSchdDetail> result in PXSelectJoin<
                AMSchdOperDetail,
                LeftJoin<AMWCSchdDetail, 
                    On<AMSchdOperDetail.schdKey, Equal<AMWCSchdDetail.schdKey>>>,
                Where<AMSchdOperDetail.orderType, Equal<Required<AMSchdOperDetail.orderType>>,
                    And<AMSchdOperDetail.prodOrdID, Equal<Required<AMSchdOperDetail.prodOrdID>>,
                    And<AMSchdOperDetail.operationID, Equal<Required<AMSchdOperDetail.operationID>>>>>
                    >.Select(this, prodOper.OrderType, prodOper.ProdOrdID, prodOper.OperationID))
            {
                var schdOperDetail = (AMSchdOperDetail)result;
                var wcSchdDetail = (AMWCSchdDetail)result;

                if (schdOperDetail == null || string.IsNullOrWhiteSpace(schdOperDetail.ProdOrdID))
                {
                    continue;
                }

                SchdOperDetailRecords.Delete(schdOperDetail);

                if (wcSchdDetail == null || string.IsNullOrWhiteSpace(wcSchdDetail.WcID))
                {
                    continue;
                }

                WCSchdDetailRecords.Delete(wcSchdDetail);
            }
        }

        /// <summary>
        /// Resets the filter values back to their original settings
        /// </summary>
        protected virtual void ResetUpdateFilters()
        {
            this.SalesLineUpdateFilter.Current.UpdateSiteID = false;
        }

        /// <summary>
        /// Persists the sales order related updates (if any)
        /// </summary>
        protected static void PersistSalesOrderLine(AMProdItem amProdItem, SalesLineUpdate filter)
        {
            if (filter == null
                || amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID)
                || string.IsNullOrWhiteSpace(amProdItem.OrdNbr)
                || amProdItem.OrdLineRef == null)
            {
                return;
            }

            //Currently on siteid is wired for sync to sales line...
            if (!filter.UpdateSiteID.GetValueOrDefault())
            {
                return;
            }

            try
            {
                var orderEntry = CreateInstance<SOOrderEntry>();
                orderEntry.Clear();

                orderEntry.Document.Current = orderEntry.Document.Search<SOOrder.orderNbr>(amProdItem.OrdNbr,
                    amProdItem.OrdTypeRef);
                if (orderEntry.Document.Current == null
                    || orderEntry.Document.Current.Completed.GetValueOrDefault()
                    || orderEntry.Document.Current.Cancelled.GetValueOrDefault())
                {
                    return;
                }

                foreach (SOLine row in orderEntry.Transactions.Select())
                {
                    //Required current for config pricing
                    orderEntry.Transactions.Current = row;

                    if (row.LineNbr != amProdItem.OrdLineRef)
                    {
                        continue;
                    }

                    if (row.ShippedQty.GetValueOrDefault() != 0
                        || row.Completed.GetValueOrDefault())
                    {
                        PXTrace.WriteWarning($"Sales Line not updated. Shipped Qty = {row.ShippedQty.GetValueOrDefault()} or Completed = {row.Completed.GetValueOrDefault()}");
                        break;
                    }

                    if (row.SiteID != amProdItem.SiteID)
                    {
                        var soLineUpdate = (SOLine)orderEntry.Transactions.Cache.CreateCopy(row);
                        soLineUpdate.SiteID = amProdItem.SiteID;
                        orderEntry.Transactions.Update(soLineUpdate);
                    }
                    break;
                }

                if (orderEntry.IsDirty)
                {
                    orderEntry.Actions.PressSave();
                }
            }
            catch (Exception ex)
            {
                PXTraceHelper.PxTraceException(ex);
                PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateSalesOrderFromProductionOrder,
                    amProdItem.OrdTypeRef.TrimIfNotNullEmpty(),
                    amProdItem.OrdNbr.TrimIfNotNullEmpty(),
                    amProdItem.OrdLineRef,
                    amProdItem.OrderType.TrimIfNotNullEmpty(),
                    amProdItem.ProdOrdID.TrimIfNotNullEmpty(),
                    ex.Message));
            }
        }

        protected bool IsInternalSave;
        protected virtual void InternalSave()
        {
            if (!IsDirty)
            {
                return;
            }

            IsInternalSave = true;
            Persist();
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            //A long operation might happen on persist. We want to finish the save before going further
            PXLongOperation.WaitCompletion(UID);
#if DEBUG
            sw.Stop();
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, $"PXLongOperation.WaitCompletion({UID})"));
            AMDebug.TraceDirtyCaches(this);
#endif
            IsInternalSave = false;
        }

        //We get field name cannot be empty but no indication to which DAC, so we add this for improved error reporting
        public override int Persist(Type cacheType, PXDBOperation operation)
        {
            try
            {
                return base.Persist(cacheType, operation);
            }
            catch (Exception e)
            {
                PXTrace.WriteError($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#if DEBUG
                AMDebug.TraceWriteMethodName($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#endif
                throw;
            }
        }

        private void ResetConfiguration()
        {
            var prodItem = ProdMaintRecords?.Current;
            if (prodItem?.InventoryID == null || prodItem?.DetailSource != ProductionDetailSource.Configuration || !ResetConfigurationRequired())
            {
                return;
            }

            UpdateConfigurationResult(ProdMaintRecords.Current, ItemConfiguration.Current, ItemConfiguration.Current.ConfigurationID, true);

            prodItem.BOMID = GetConfigBomID();
            prodItem.BuildProductionBom = true;
            ProdMaintRecords.Update(prodItem);
        }

        private bool ResetConfigurationRequired()
        {
            var cfg = ItemConfiguration?.Current;
            if (cfg?.ConfigurationID == null || !ItemConfiguration.Cache.IsCurrentRowInsertedOrUpdated() || cfg.IsSalesReferenced.GetValueOrDefault() || cfg.IsOpportunityReferenced.GetValueOrDefault())
            {
                return false;
            }

            var firstAttribute = ItemConfiguration.ResultAttributes.Current;
            var firstFeature = ItemConfiguration.ResultFeatures.Current;

            var featureMatch = firstFeature == null ||
                                firstFeature.ConfigurationID.EqualsWithTrim(cfg.ConfigurationID) &&
                                firstFeature.Revision.EqualsWithTrim(cfg.Revision);

            var attributeMatch = firstAttribute == null ||
                                 firstAttribute.ConfigurationID.EqualsWithTrim(cfg.ConfigurationID) &&
                                 firstAttribute.Revision.EqualsWithTrim(cfg.Revision);

            return !featureMatch || !attributeMatch;
        }

        /// <summary>
        /// Is the production order queued for a reschedule
        /// </summary>
        protected virtual bool IsReschedule(AMProdItem prodItem)
        {
            if (prodItem?.InventoryID == null)
            {
                return false;
            }

            var cacheStatus = ProdMaintRecords.Cache.GetStatus(prodItem);
            return (cacheStatus == PXEntryStatus.Inserted ||
                    cacheStatus == PXEntryStatus.Updated)
                   && prodItem.Reschedule.GetValueOrDefault()
                   && (prodItem.StatusID == ProductionOrderStatus.Planned ||
                       prodItem.StatusID == ProductionOrderStatus.Released ||
                       prodItem.StatusID == ProductionOrderStatus.InProcess);
        }

        protected virtual void PersistBase()
        {
            base.Persist();
        }

        public override void Persist()
        {
            ResetConfiguration();
            var currentAMProdItem = ProdMaintRecords.Current;
            var buildProductionDetail = currentAMProdItem != null && currentAMProdItem.BuildProductionBom.GetValueOrDefault();
            var amProdItemPXEntryStatus = ProdMaintRecords.Cache.GetStatus(currentAMProdItem);
            var reschedule = IsReschedule(currentAMProdItem);

            var substituteWorkCenters = buildProductionDetail == false
                                        && currentAMProdItem != null
                                        && currentAMProdItem.StatusID == ProductionOrderStatus.Planned
                                        && InventoryHelper.MultiWarehousesFeatureEnabled
                                        && amOrderType.Current?.SubstituteWorkCenters == true
                                        && (amProdItemPXEntryStatus == PXEntryStatus.Inserted ||
                                            amProdItemPXEntryStatus == PXEntryStatus.Updated);

            //Note: Cache.GetStatus will never indicate a deleted status...
   
            TriggerItemPlanUpdates();
                                
            if (currentAMProdItem != null
                && amProdItemPXEntryStatus == PXEntryStatus.Inserted
                && currentAMProdItem.StatusID == ProductionOrderStatus.Planned)
            {
                if (!HasCreatedProdEvent())
                {
                    InsertCreatedOrderEventMessage(currentAMProdItem);
                }

                if (ProdTotalRecs.Current == null)
                {
                    ProdTotalRecs.Insert(new AMProdTotal());
                }
            }

            if (currentAMProdItem != null
                && amProdItemPXEntryStatus != PXEntryStatus.Inserted
                && (currentAMProdItem.StatusID == ProductionOrderStatus.Planned
                    || currentAMProdItem.StatusID == ProductionOrderStatus.Released))
            {
                SyncSchdItem(currentAMProdItem);
            }

            if (currentAMProdItem != null
                && (amProdItemPXEntryStatus == PXEntryStatus.Inserted ||
                    amProdItemPXEntryStatus == PXEntryStatus.Updated)
                && buildProductionDetail
                && currentAMProdItem.StatusID == ProductionOrderStatus.Planned)
            {
                RowDeleting.RemoveHandler<AMProdAttribute>(AMProdAttribute_RowDeleting);
                CopyBomToProductionDetails(currentAMProdItem);
            }

            if (substituteWorkCenters)
            {
                var origRow = ProdMaintRecords.Cache.GetOriginal(currentAMProdItem);
                if(origRow != null && !ProdMaintRecords.Cache.ObjectsEqual<AMProdItem.siteID>(origRow, currentAMProdItem))
                {
                    SubstituteWorkCenters(currentAMProdItem);
                }
            }
            
            var retryCount = 1;
            for (var retry = 0; retry <= retryCount; retry++)
            {
                try
                {
                    using (var ts = new PXTransactionScope())
                    {
                        PersistBase();
                        //Only retry on base persist - currently on retry due to data issue with event line counters
                        retry = retryCount;

                        currentAMProdItem = ProdMaintRecords.Current;
                        PersistSalesOrderLine(currentAMProdItem, SalesLineUpdateFilter.Current);

                        if (reschedule && (IsImport || IsContractBasedAPI || IsInternalSave))
                        {
                            RescheduleOrder(currentAMProdItem);
                            reschedule = false;
                        }

                        ResetUpdateFilters();
                        if (IsDirty)
                        {
                            PersistBase();
                        }

                        ts.Complete();
                    }
                }
                catch (PX.Data.PXLockViolationException lve)
                {
                    var prodItem = ProdMaintRecords?.Current;

                    // Only retry on production event lock
                    if (prodItem?.ProdOrdID == null || retry >= retryCount || lve.Table != typeof(AMProdEvnt))
                    {
                        throw;
                    }

                    PXTrace.WriteWarning(Messages.GetLocal(Messages.CorrectingHistoryLineCountersForProduction, prodItem.ProdOrdID));
                    if (!TryCorrectHistoryLineCounters(prodItem))
                    {
                        throw;
                    }
                }
                catch (PXOuterException oe)
                {
                    PXTraceHelper.PxTraceOuterException(oe, PXTraceHelper.ErrorLevel.Error);
                    throw;
                }
            }

            currentAMProdItem = ProdMaintRecords.Current;
            if (PXLongOperation.GetCurrentItem() == null
                && currentAMProdItem != null && reschedule)
            {
                PXLongOperation.StartOperation(this, delegate
                {
                    RescheduleOrder(currentAMProdItem, true);
                });
            }
        }

        private bool TryCorrectHistoryLineCounters(AMProdItem prodItem)
        {
            if (prodItem?.ProdOrdID == null)
            {
                return false;
            }

            var maxEvent = (AMProdEvnt)SelectFrom<AMProdEvnt>.
                Where<AMProdEvnt.orderType.IsEqual<P.AsString>.
                    And<AMProdEvnt.prodOrdID.IsEqual<P.AsString>>>.
                OrderBy<AMProdEvnt.lineNbr.Desc>.
                View.SelectWindowed(this, 0, 1, prodItem.OrderType, prodItem.ProdOrdID);

            if (maxEvent?.LineNbr == null || prodItem.LineCntrEvnt >= maxEvent.LineNbr)
            {
                return false;
            }

            prodItem.LineCntrEvnt = maxEvent.LineNbr + 1;
            ProdMaintRecords.Update(prodItem);

            // Fix inserted history to get next persist to go through...
            var newProdEvent = new List<AMProdEvnt>();
            foreach (AMProdEvnt newHistory in ProdEventRecords.Cache.Inserted)
            {
                var newCopy = PXCache<AMProdEvnt>.CreateCopy(newHistory);
                newCopy.LineNbr = null;
                newProdEvent.Add(newCopy);
                ProdEventRecords.Cache.Remove(newHistory);
            }

            foreach (var prodEvent in newProdEvent)
            {
                ProdEventRecords.Insert(prodEvent);
            }

            return true;
        }

        protected virtual void TriggerItemPlanUpdates()
        {
            //Not all field changes correctly trigger the update of INItemPlan records...

            var prodItem = ProdMaintRecords.Current;
            if (prodItem?.Hold == null)
            {
                return;
            }

            var prodItemRowStatus = ProdMaintRecords.Cache.GetStatus(prodItem);
            var oldHold = (bool?)ProdMaintRecords.Cache.GetValueOriginal<AMProdItem.hold>(prodItem);
            if (prodItemRowStatus != PXEntryStatus.Updated || oldHold == null || prodItem.Hold.GetValueOrDefault() == oldHold.GetValueOrDefault())
            {
                return;
            }

            foreach (AMProdItemSplit row in splits.Select())
            {
                splits.Update(row); //raise events to trigger hold change in AMProdItemSplitPlanIDAttribute
            }

            foreach (AMProdMatlSplit row in PXSelect<AMProdMatlSplit,
                Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>>>.Select(this, prodItem.OrderType, prodItem.ProdOrdID))
            {
                var prodMatl = ProdMatlSplits.Cache.LocateElse(row);
                if (prodMatl?.InventoryID == null)
                {
                    continue;
                }
                ProdMatlSplits.Update(row); //raise events to trigger hold change in AMProdMatlSplitPlanIDAttribute
            }
        }

        protected virtual void RemovePlanReferences(AMProdItem amProdItem)
        {
            RemoveSOLineReference(amProdItem);
            RemoveParentItemPlanReference(amProdItem);
        }

        /// <summary>
        /// Removes the Sales Order's production order line reference if one exits for the given production item
        /// </summary>
        public virtual void RemoveSOLineReference(AMProdItem amProdItem)
        {
            if (string.IsNullOrWhiteSpace(amProdItem?.ProdOrdID))
            {
                return;
            }

            SOLineMfgOnly soLineExtension = PXSelectReadonly<SOLineMfgOnly,
                Where<SOLineMfgOnly.orderType, Equal<Required<AMProdItem.ordTypeRef>>,
                    And<SOLineMfgOnly.orderNbr, Equal<Required<AMProdItem.ordNbr>>,
                        And<SOLineMfgOnly.lineNbr, Equal<Required<AMProdItem.ordLineRef>>>>>
            >.SelectWindowed(this, 0, 1, amProdItem.OrdTypeRef, amProdItem.OrdNbr, amProdItem.OrdLineRef);

            if (soLineExtension == null)
            {
                return;
            }

            soLineExtension.AMOrderType = null;
            soLineExtension.AMProdOrdID = null;

            Common.Cache.AddCacheView<SOLineSplitMfgOnly>(this);
            Common.Cache.AddCacheView<SOLineMfgOnly>(this);

            this.Caches<SOLineMfgOnly>().Update(soLineExtension);

            foreach (SOLineSplitMfgOnly soLineSplitExtension in PXSelectReadonly<SOLineSplitMfgOnly,
                Where<SOLineSplitMfgOnly.orderType, Equal<Required<AMProdItem.ordTypeRef>>,
                    And<SOLineSplitMfgOnly.orderNbr, Equal<Required<AMProdItem.ordNbr>>,
                        And<SOLineSplitMfgOnly.lineNbr, Equal<Required<AMProdItem.ordLineRef>>>>>
            >.Select(this, amProdItem.OrdTypeRef, amProdItem.OrdNbr, amProdItem.OrdLineRef))
            {
                if(string.IsNullOrWhiteSpace(soLineSplitExtension.AMOrderType))
                {
                    continue;
                }

                soLineSplitExtension.AMOrderType = null;
                soLineSplitExtension.AMProdOrdID = null;
                soLineSplitExtension.AMProdStatusID = null;
                soLineSplitExtension.AMProdBaseQtyComplete = 0m;
                soLineSplitExtension.AMProdQtyComplete = 0m;
                this.Caches<SOLineMfgOnly>().Update(soLineExtension);
            }

            Common.Cache.AddCacheView<INItemPlan>(this);
            // unlink the allocation entries between production and sales line
            foreach (INItemPlan soItemPlan in PXSelectJoin<
                INItemPlan,
                InnerJoin<AMProdItemSplit, 
                    On<INItemPlan.supplyPlanID, Equal<AMProdItemSplit.planID>>>,
                Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
                    And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>>>>
                .Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                if (soItemPlan.SupplyPlanID == null)
                {
                    continue;
                }

                soItemPlan.SupplyPlanID = null;
                this.Caches<INItemPlan>().Update(soItemPlan);
            }
        }

        /// <summary>
        /// Removes all Parent production item plan references if one exits for the given production item
        /// </summary>
        protected virtual void RemoveParentItemPlanReference(AMProdItem amProdItem)
        {
            if (string.IsNullOrWhiteSpace(amProdItem?.ProdOrdID))
            {
                return;
            }
            
            Common.Cache.AddCacheView<INItemPlan>(this);
            Common.Cache.AddCacheView<AMProdMatlSplit2>(this);
            
            //unlink the allocation entries between production and sales line
            foreach (PXResult<INItemPlan, AMProdMatlSplit2, AMProdItemSplit> result in PXSelectJoin<
                INItemPlan,
                InnerJoin<AMProdMatlSplit2, 
                    On<INItemPlan.planID, Equal<AMProdMatlSplit2.planID>>,
                InnerJoin<AMProdItemSplit,
                    On<INItemPlan.supplyPlanID, Equal<AMProdItemSplit.planID>>>>,
                Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
                    And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>>>>
                .Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                var plan = (INItemPlan) result;
                var planMatl = (AMProdMatlSplit2) result;

                if (planMatl?.PlanID != null)
                {
                    planMatl.RefNoteID = null;
                    planMatl.AMOrderType = null;
                    planMatl.AMProdOrdID = null;

                    this.Caches<AMProdMatlSplit2>().Update(planMatl);
                }
                
                if (plan?.SupplyPlanID == null)
                {
                    continue;
                }

                plan.SupplyPlanID = null;

                this.Caches<INItemPlan>().Update(plan);
            }
        }

        protected virtual void _(Events.FieldUpdated<AMConfigurationResults.keyID> e)
        {
            var prodItem = ProdMaintRecords.Current;
            if (prodItem?.OrderType == null)
            {
                return;
            }

            ProdMaintRecords.Cache.SetValueExt<AMProdItem.buildProductionBom>(prodItem, true);
        }

        protected virtual void AMConfigurationResults_Revision_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var config = (AMConfiguration)PXSelectorAttribute.Select<AMConfigurationResults.revision>(cache, e.Row);
            if (config != null)
            {
                cache.SetValueExt<AMProdItem.bOMID>(e.Row, config.BOMID);
            }
        }

        protected virtual void AMProdItem_DetailSource_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var row = (AMProdItem)e.Row;
            if (row != null)
            {
                var newSource = (int?)e.NewValue;

                if (newSource == ProductionDetailSource.Configuration && row.Function != OrderTypeFunction.Disassemble)
                {
                    if (!ItemConfiguration.IsConfiguratorActive)
                    {
                        e.NewValue = ProductionDetailSource.NoSource;
                        e.Cancel = true;
                    }

                    if (row.IsConfigurable != true || row.Function == OrderTypeFunction.Disassemble)
                    {
                        cache.RaiseExceptionHandling<AMProdItem.detailSource>(row, e.NewValue,
                            new PXSetPropertyException(Messages.NotConfigurableItem));
                        e.NewValue = row.DetailSource;
                        e.Cancel = true;
                    }

                    return;
                }

                if (this.ItemConfiguration.Current != null && row.Function != OrderTypeFunction.Disassemble)
                {
                    if (FieldVerifyingLinkedToSO<AMProdItem.detailSource>(cache, e))
                    {
                        e.NewValue = row.DetailSource;
                        e.Cancel = true;
                        return;
                    }

                    if (!IsImport && !IsContractBasedAPI && ProdMaintRecords.Ask(Messages.ConfirmChangeSourceDeleteConfigResults, MessageButtons.YesNo) != WebDialogResult.Yes)
                    {
                        e.NewValue = row.DetailSource;
                        e.Cancel = true;
                    }
                }
            }
        }

        protected virtual void AMProdItem_DetailSource_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMProdItem)e.Row;
            if (row == null)
            {
                return;
            }

            if (amOrderType.Current.Function != OrderTypeFunction.Disassemble)
            {
                UpdateConfigurationResult(row, ItemConfiguration.Select());
            }

            switch (row.DetailSource)
            {
                case ProductionDetailSource.Configuration:
                    row.EstimateID = null;
                    row.EstimateRevisionID = null;
                    row.SourceOrderType = null;
                    row.SourceProductionNbr = null;
                    break;
                case ProductionDetailSource.Estimate:
                    row.BOMID = null;
                    row.BOMRevisionID = null;
                    row.SourceOrderType = null;
                    row.SourceProductionNbr = null;
                    break;
                case ProductionDetailSource.ProductionRef:
                    row.EstimateID = null;
                    row.EstimateRevisionID = null;
                    row.BOMID = null;
                    row.BOMRevisionID = null;
                    break;
                default:
                    row.SourceOrderType = null;
                    row.SourceProductionNbr = null;
                    row.EstimateID = null;
                    row.EstimateRevisionID = null;
                    row.BOMID = null;
                    row.BOMRevisionID = null;
                    break;
            }
        }

        public virtual AMBomItem GetBOMItem(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return null;
            }

            PXSelectBase<AMBomItem> amBomItemCmd = new PXSelect<
                AMBomItem,
                Where<AMBomItem.status, Equal<AMBomStatus.active>,
                    And<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>>>,
                OrderBy<Desc<AMBomItem.effStartDate, Desc<AMBomItem.revisionID>>>>(this);

            var selectArgs = new List<object> { amProdItem.BOMID };

            if (amProdItem.BOMEffDate != null)
            {
                amBomItemCmd.WhereAnd<Where<Required<AMBomItem.effStartDate>,
                    Between<AMBomItem.effStartDate, AMBomItem.effEndDate>,
                    Or<Where<AMBomItem.effStartDate, LessEqual<Required<AMBomItem.effStartDate>>,
                        And<AMBomItem.effEndDate, IsNull>>>>>();
                selectArgs.Add(amProdItem.BOMEffDate);
                selectArgs.Add(amProdItem.BOMEffDate);
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && amProdItem.SubItemID != null)
            {
                amBomItemCmd.WhereAnd<Where<AMBomItem.subItemID, Equal<Required<AMProdItem.subItemID>>, Or<AMBomItem.subItemID, IsNull>>>();
                selectArgs.Add(amProdItem.SubItemID);
            }

            return amBomItemCmd.SelectSingle(selectArgs.ToArray());
        }

        /// <summary>
        /// Copy the BOM header/item note to the current production item record
        /// </summary>
        /// <returns>True if note copy performed</returns>
        public virtual bool AMProdItemCopyBOMNotes()
        {
            return AMProdItemCopyBOMNotes(ProdMaintRecords.Current);
        }

        /// <summary>
        /// Copy the BOM header/item note to the given production item record
        /// </summary>
        /// <param name="amProdItem"></param>
        /// <returns>True if note copy performed</returns>
        public virtual bool AMProdItemCopyBOMNotes(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || amProdItem.DetailSource != ProductionDetailSource.BOM)
            {
                return false;
            }

            return AMProdItemCopyBOMNotes(amProdItem, GetBOMItem(amProdItem));
        }

        /// <summary>
        /// Copy the given BOM header/item note to the given production item record
        /// </summary>
        /// <param name="amProdItem"></param>
        /// <param name="amBomItem"></param>
        /// <returns>True if note copy performed</returns>
        public virtual bool AMProdItemCopyBOMNotes(AMProdItem amProdItem, AMBomItem amBomItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType)
                || amBomItem?.BOMID == null)
            {
                return false;
            }

            AMOrderType orderType = amOrderType.Select(amProdItem.OrderType);
            if (orderType == null)
            {
                return false;
            }

            if (orderType.CopyNotesItem.GetValueOrDefault())
            {
                //remove any files attached to the previous BOMID
                PXCache dcache = this.Caches[typeof(NoteDoc)];
                foreach (NoteDoc doc in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, amProdItem.NoteID))
                {
                    dcache.Delete(doc);
                }
                PXNoteAttribute.CopyNoteAndFiles(this.Caches[typeof(AMBomItem)], amBomItem, ProdMaintRecords.Cache, amProdItem);
                return true;
            }
            return false;
        }

        protected virtual void AMProdItem_BOMRevisionID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null)
            {
                return;
            }

            var oldBomRev = (string)e.OldValue;
            if (amProdItem.BOMRevisionID != oldBomRev)
            {
                amProdItem.BuildProductionBom = true;
            }
        }

        protected virtual void AMProdItem_BOMID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem?.InventoryID == null)
            {
                return;
            }

            var oldBomID = (string)e.OldValue;
            if (amProdItem.BOMID != oldBomID)
            {
                amProdItem.BuildProductionBom = true;
            }

            if (string.IsNullOrWhiteSpace(amProdItem.BOMID))
            {
                cache.SetValueExt<AMProdItem.bOMRevisionID>(amProdItem, null);
            }
            else
            {
                var amBomItem = setBomRevBomItem ?? GetBOMItem(amProdItem);
                cache.SetValueExt<AMProdItem.bOMRevisionID>(amProdItem, amBomItem?.RevisionID);
                AMProdItemCopyBOMNotes(amProdItem, amBomItem);
            }

            if (IsImport || IsContractBasedAPI || !string.IsNullOrWhiteSpace(amProdItem?.BOMID) ||
                amProdItem.DetailSource != ProductionDetailSource.BOM)
            {
                return;
            }

            var msg = PXAccess.FeatureInstalled<FeaturesSet.subItem>()
                ? Messages.GetLocal(Messages.NoBomWarningSubItem)
                : Messages.GetLocal(Messages.NoBomWarning);

            cache.RaiseExceptionHandling<AMProdItem.bOMID>(amProdItem, amProdItem.BOMID,
                new PXSetPropertyException<AMProdItem.bOMID>(msg, PXErrorLevel.Warning));
        }

        protected virtual void AMProdItem_SubItemID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (!InventoryHelper.SubItemFeatureEnabled || amProdItem == null || amProdItem.InventoryID == null
                || amProdItem.SubItemID == null || amProdItem.SiteID == null || amProdItem.DetailSource != ProductionDetailSource.BOM)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(amProdItem.BOMID) || amProdItem.SubItemID.GetValueOrDefault() != (int)(e.OldValue ?? 0))
            {
                SetBomRevision(cache, amProdItem);
            }
        }

        public virtual InventoryItem CurrentInventoryItem
        {
            get
            {
                if (itemrecord.Current == null
                    || (ProdMaintRecords.Current != null
                    && ProdMaintRecords.Current.InventoryID != null
                    && ProdMaintRecords.Current.InventoryID != itemrecord.Current.InventoryID))
                {
                    itemrecord.Current = PXSelect<InventoryItem,
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>
                            >.Select(this, ProdMaintRecords.Current.InventoryID);
                }

                return itemrecord.Current;
            }
        }

        protected virtual void AMProdItem_CostMethod_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;

            if (amProdItem == null
                || amProdItem.InventoryID == null
                || CurrentInventoryItem == null)
            {
                return;
            }

            var newValue = (int?)e.NewValue;

            if (newValue != null
                && newValue.GetValueOrDefault() == CostMethod.Standard
                && CurrentInventoryItem.ValMethod != INValMethod.Standard)
            {
                e.NewValue = amProdItem.CostMethod;
                e.Cancel = true;
            }
        }

        public virtual bool FieldVerifyingLinkedToSO<Field>(PXCache sender, PXFieldVerifyingEventArgs e) where Field : IBqlField
        {
            var row = (AMProdItem)e.Row;
            if (IsImport  || IsContractBasedAPI || UnattendedMode
                || row == null
                || string.IsNullOrWhiteSpace(row.OrdTypeRef)
                || string.IsNullOrWhiteSpace(row.OrdNbr)
                || row.OrdLineRef == null)
            {
                return false;
            }

            var spe = new PXSetPropertyException(Messages.FieldCannotBeChangedLinkedToSO,
                PXUIFieldAttribute.GetDisplayName<Field>(sender),
                row.OrderType.TrimIfNotNullEmpty(),
                row.ProdOrdID.TrimIfNotNullEmpty(),
                row.OrdTypeRef.TrimIfNotNullEmpty(),
                row.OrdNbr.TrimIfNotNullEmpty());

            if (typeof(Field) == typeof(AMProdItem.inventoryID)
                && ((AMProdItem)e.Row).InventoryID != null)
            {
                sender.RaiseExceptionHandling<Field>(row, e.NewValue, spe);
                return true;
            }

            if (typeof(Field) == typeof(AMProdItem.detailSource)
                && ((AMProdItem)e.Row).DetailSource != null
                && ((AMProdItem)e.Row).IsConfigurable.GetValueOrDefault()
                && ((AMProdItem)e.Row).DetailSource == ProductionDetailSource.Configuration
                && (int?)e.NewValue != ProductionDetailSource.Configuration)
            {
                sender.RaiseExceptionHandling<Field>(row, e.NewValue, spe);
                return true;
            }

            return false;
        }

        protected virtual void AMProdItem_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMProdItem)e.Row;
            if (row == null)
            {
                return;
            }

            if (FieldVerifyingLinkedToSO<AMProdItem.inventoryID>(sender, e))
            {
                e.NewValue = row.InventoryID;
                e.Cancel = true;
                return;
            }

            if (amOrderType.Current.Function == OrderTypeFunction.Disassemble || IsImport || IsContractBasedAPI)
            {
                return;
            }

            if (!UnattendedMode && row.IsConfigurable.GetValueOrDefault() && ItemConfiguration.IsConfiguratorActive
                && ProdMaintRecords.Ask(Messages.DeletingExistingConfiguration, MessageButtons.YesNo) == WebDialogResult.No)
            {
                e.NewValue = row.InventoryID;
                e.Cancel = true;
            }
        }

        protected virtual void AMProdItem_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null)
            {
                return;
            }

            itemrecord.Current = (InventoryItem)PXSelectorAttribute.Select<AMProdItem.inventoryID>(cache, amProdItem) ?? PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>
                    >.Select(this, amProdItem.InventoryID);
            bool itemChanged = (int)(e.OldValue ?? 0) != amProdItem.InventoryID && (int)(e.OldValue ?? 0) != 0;

            if (itemChanged)
            {
                amProdItem.BOMID = null;
                amProdItem.BOMRevisionID = null;
                amProdItem.SiteID = null;
                amProdItem.SubItemID = null;
                amProdItem.DetailSource = null;
            }

            if (itemChanged || cache.GetStatus(e.Row) == PXEntryStatus.Inserted)
            {
                SetAccountDefaults(cache);
                SetProductInfo(cache);
                SetScrapWarehouseLocation(amProdItem);
            }

            var itemExtension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(itemrecord.Current);
            if (itemExtension != null
                && ItemConfiguration.IsConfiguratorActive
                && !string.IsNullOrWhiteSpace(itemExtension.AMConfigurationID)
                && amOrderType.Current.Function != OrderTypeFunction.Disassemble)
            {
                cache.SetValueExt<AMProdItem.detailSource>(e.Row, ProductionDetailSource.Configuration);
            }

            if (amProdItem.SubItemID == null)
            {
                cache.SetDefaultExt<AMProdItem.subItemID>(e.Row);
            }

            if (amProdItem.SiteID == null)
            {
                cache.SetDefaultExt<AMProdItem.siteID>(e.Row);
            }

            if (amProdItem.LocationID == null && amProdItem.SiteID != null
                && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()
                && itemrecord.Current != null)
            {
                cache.SetValueExt<AMProdItem.locationID>(amProdItem, InventoryHelper.DfltLocation.GetDefault(this,
                    InventoryHelper.DfltLocation.BinType.Receipt, itemrecord.Current.InventoryID,
                    itemrecord.Current.DfltSiteID, false));
            }

            if (!IsImport && !IsContractBasedAPI)
            {
                cache.SetValueExt<AMProdItem.qtytoProd>(e.Row, InventoryHelper.GetMfgReorderQty(this, amProdItem.InventoryID, amProdItem.SiteID));
            }
            amProdItem.TranDate = Common.Current.BusinessDate(this);

            if (amProdItem.DetailSource == null)
            {
                cache.SetDefaultExt<AMProdItem.detailSource>(e.Row);
            }

            if (amProdItem.DetailSource == ProductionDetailSource.BOM)
            {
                SetBomRevision(cache, amProdItem);

                if (string.IsNullOrEmpty(amProdItem.BOMID) && !IsImport && !IsContractBasedAPI)
                {
                    cache.RaiseExceptionHandling<AMProdItem.bOMID>(amProdItem, null,
                        new PXSetPropertyException<AMProdItem.bOMID>(Messages.NoDefaultBOM, PXErrorLevel.Warning));
                }
            }
            else if (amProdItem.DetailSource == ProductionDetailSource.Configuration)
            {
                var configBom = GetConfigBomID();
                if (!configBom.EqualsWithTrim(amProdItem.BOMID))
                {
                    cache.SetValueExt<AMProdItem.bOMID>(amProdItem, configBom);
                }
            }

            amProdItem.Reschedule = true;
            amProdItem.BuildProductionBom = true;

            amProdItem.CostMethod = itemrecord.Current != null && itemrecord.Current.ValMethod == INValMethod.Standard
                ? CostMethod.Standard
                : amOrderType.Current.DefaultCostMethod;
        }

        protected virtual void AMProdItem_QtytoProd_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null ||
                e.OldValue != null && Convert.ToDecimal(e.OldValue) == ((AMProdItem)e.Row).QtytoProd)
            {
                return;
            }

            ((AMProdItem)e.Row).Reschedule = true;

            if (cache.GetStatus(e.Row) == PXEntryStatus.Inserted && ((AMProdItem)e.Row).BuildProductionBom.GetValueOrDefault())
            {
                return;
            }

            ProductionTransactionHelper.UpdateOperationQty(this, (AMProdItem)e.Row, ampsetup.Current?.InclScrap ?? false);
        }


        protected virtual void AMProdItem_RevisionNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null ||
                amProdItem.DetailSource != ProductionDetailSource.BOM)
            {
                return;
            }

            AMBomItem amBomItem = PXSelect<
                AMBomItem,
                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                    And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                .Select(this, amProdItem.BOMID, amProdItem.BOMRevisionID);

            if (amBomItem != null)
            {
                amProdItem.BuildProductionBom = true;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(amProdItem.BOMID))
                {
                    throw new PXSetPropertyException<AMProdItem.bOMID>(Messages.InvalidRevisionWarning,
                        PXErrorLevel.Warning);
                }
            }
        }

        protected virtual void AMProdItem_SchedulingMethod_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.NewValue != null && (string)e.NewValue == ScheduleMethod.UserDates && !FixedMfgTimesEnabled)
            {
                throw new PXSetPropertyException(Messages.GetLocal(Messages.UnableToUpdateField,
                    PXUIFieldAttribute.GetDisplayName<AMProdItem.schedulingMethod>(ProdMaintRecords.Cache),
                    Messages.GetLocal(Messages.MissingFixMfgCalendar)));
            }
        }


        protected virtual void AMProdItem_SchedulingMethod_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null || amProdItem.SchedulingMethod == null)
            {
                return;
            }

            string oldValue = (string)e.OldValue ?? string.Empty;
            if (!string.IsNullOrEmpty((string)e.OldValue) && !amProdItem.SchedulingMethod.EqualsWithTrim(oldValue))
            {
                ((AMProdItem)e.Row).Reschedule = true;

                if (amProdItem.SchedulingMethod == ScheduleMethod.UserDates && !amProdItem.FMLTime.GetValueOrDefault())
                {
                    ((AMProdItem)e.Row).FMLTime = true;
                }
            }
        }

        protected virtual void AMProdItem_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMProdItem)e.Row;
            if (row == null)
            {
                return;
            }

            if (amOrderType.Current.Function == OrderTypeFunction.Disassemble)
            {
                return;
            }

            if (IsImport || IsContractBasedAPI || UnattendedMode || !row.IsConfigurable.GetValueOrDefault() || !ItemConfiguration.IsConfiguratorActive)
            {
                return;
            }

            var itemConfiguration = (AMConfigurationResults)ItemConfiguration.Select();
            if (itemConfiguration != null && ConfigurationChangeRequired(row.InventoryID, e.NewValue as int?, itemConfiguration)
                && ProdMaintRecords.Ask(Messages.DeletingExistingConfiguration, MessageButtons.YesNo) == WebDialogResult.No)
            {
                e.NewValue = row.SiteID;
                e.Cancel = true;
            }
        }

        protected virtual void AMProdItem_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null
                || amProdItem.SiteID.GetValueOrDefault() == 0
                || amProdItem.SiteID.GetValueOrDefault() == (int)(e.OldValue ?? 0))
            {
                return;
            }

            if (amOrderType.Current.Function != OrderTypeFunction.Disassemble)
            {
                UpdateConfigurationResult(amProdItem, ItemConfiguration.Select());
            }

            if (!IsImport && !IsContractBasedAPI && !UnattendedMode
                && !string.IsNullOrWhiteSpace(amProdItem.OrdNbr)
                && amProdItem.OrdLineRef.GetValueOrDefault() != 0)
            {
                var confirmMsg = Messages.GetLocal(Messages.UpdateSalesLineWarehouse,
                    amProdItem.OrdTypeRef.TrimIfNotNullEmpty(),
                    amProdItem.OrdNbr.TrimIfNotNullEmpty(),
                    amProdItem.OrdLineRef);

                SalesLineUpdateFilter.Current.UpdateSiteID = ProdMaintRecords.Ask(amProdItem, Messages.Confirm, confirmMsg, MessageButtons.YesNo, false) == WebDialogResult.Yes;
            }

            int? loc = InventoryHelper.DfltLocation.GetDefault(this, InventoryHelper.DfltLocation.BinType.Receipt,
                amProdItem.InventoryID, amProdItem.SiteID, false);

            cache.SetValueExt<AMProdItem.locationID>(amProdItem, loc);

            SetAccountDefaults(cache);

            if (amProdItem.DetailSource == ProductionDetailSource.BOM)
            {
                SetBomRevision(cache, amProdItem);
            }
            else if (amProdItem.DetailSource == ProductionDetailSource.Configuration)
            {
                var configBom = GetConfigBomID();
                if (!configBom.EqualsWithTrim(amProdItem.BOMID))
                {
                    cache.SetValueExt<AMProdItem.bOMID>(amProdItem, configBom);
                }
            }

            SetProductInfo(cache);
            SetScrapWarehouseLocation(amProdItem);
            UpdateMaterialWarehouse(amProdItem);
        }

        protected virtual void UpdateMaterialWarehouse(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return;
            }

            foreach (AMProdMatl prodMatl in PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                    And<AMProdMatl.warehouseOverride, Equal<False>>
                    >>>.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                prodMatl.SiteID = amProdItem.SiteID;
                ProdMatlRecords.Cache.Update(prodMatl);
            }

        }

        protected virtual void SetScrapWarehouseLocation(AMProdItem amProdItem)
        {
            if (amProdItem?.InventoryID == null || amProdItem.SiteID == null)
            {
                return;
            }

            AMOrderType orderType = PXSelect<AMOrderType,
                Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>.Select(this, amProdItem.OrderType);

            if (orderType == null)
            {
                return;
            }

            switch (orderType.ScrapSource)
            {
                case ScrapSource.Item:
                    INItemSite itemSite = PXSelect<INItemSite,
                        Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>
                        >>>.Select(this, amProdItem.InventoryID, amProdItem.SiteID);
                    if (itemSite != null)
                    {
                        var ext = itemSite.GetExtension<INItemSiteExt>();
                        amProdItem.ScrapSiteID = ext.AMScrapSiteID;
                        amProdItem.ScrapLocationID = ext.AMScrapLocationID;
                    }
                    break;
                case ScrapSource.Warehouse:
                    INSite site = PXSelect<INSite,
                        Where<INSite.siteID, Equal<Required<INSite.siteID>>
                        >>.Select(this, amProdItem.SiteID);
                    if (site != null)
                    {
                        var ext = site.GetExtension<INSiteExt>();
                        amProdItem.ScrapSiteID = ext.AMScrapSiteID;
                        amProdItem.ScrapLocationID = ext.AMScrapLocationID;
                    }
                    break;
                case ScrapSource.OrderType:
                    amProdItem.ScrapSiteID = orderType.ScrapSiteID;
                    amProdItem.ScrapLocationID = orderType.ScrapLocationID;
                    break;
                default:
                    amProdItem.ScrapSiteID = null;
                    amProdItem.ScrapLocationID = null;
                    break;
            }
        }

        /// <summary>
        /// Set the AMProdItem account defaults based on the posting class configurations for the selected item/warehouse
        /// </summary>
        /// <param name="cache">AMProdItem cache to update</param>
        public virtual void SetAccountDefaults(PXCache cache)
        {
            if (cache?.Current == null)
            {
                return;
            }

            var current = cache.Current;
            if (current.GetType() != typeof(AMProdItem)
                || ((AMProdItem)cache.Current).InventoryID == null)
            {
                return;
            }

            //  Gather information to set default posting accounts
            PXResult<InventoryItem, INPostClass> itemPostClassRes = (PXResult<InventoryItem, INPostClass>)
                PXSelectJoin<InventoryItem, LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>>,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, ((AMProdItem)cache.Current).InventoryID);

            INSite site = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, ((AMProdItem)cache.Current).SiteID);

            ((AMProdItem)cache.Current).WIPAcctID = GLAccountHelper.GetAccountDefaults<INPostClassExt.aMWIPAcctID>
                (this, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes, amOrderType.Current);

            if (((AMProdItem)cache.Current).WIPAcctID != null)
            {
                ((AMProdItem)cache.Current).WIPSubID = GLAccountHelper.GetAccountDefaults<INPostClassExt.aMWIPSubID>
                    (this, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes, amOrderType.Current);
            }

            ((AMProdItem)cache.Current).WIPVarianceAcctID = GLAccountHelper.GetAccountDefaults<INPostClassExt.aMWIPVarianceAcctID>
                (this, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes, amOrderType.Current);

            if (((AMProdItem)cache.Current).WIPVarianceAcctID != null)
            {
                ((AMProdItem)cache.Current).WIPVarianceSubID = GLAccountHelper.GetAccountDefaults<INPostClassExt.aMWIPVarianceSubID>
                    (this, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes, amOrderType.Current);
            }
        }

        protected virtual void SetProductInfo(PXCache cache)
        {
            if (cache?.Current == null)
            {
                return;
            }

            var current = cache.Current;
            if (current.GetType() != typeof(AMProdItem)
                || ((AMProdItem)cache.Current).InventoryID == null)
            {
                return;
            }

            INItemSite itemSite = null;
            if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && ((AMProdItem)cache.Current).SiteID != null)
            {
                itemSite = PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(this, ((AMProdItem)cache.Current).InventoryID, ((AMProdItem)cache.Current).SiteID);
            }

            bool useItem = itemSite == null ||
                           (itemSite.ProductWorkgroupID == null && itemSite.ProductManagerID == null);

            InventoryItem inventoryItem = null;
            if (useItem)
            {
                inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>
                        >>.Select(this, ((AMProdItem)cache.Current).InventoryID);

                if (inventoryItem == null)
                {
                    throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                        Common.Cache.GetCacheName(typeof(AMProdItem))));
                }
            }

            ((AMProdItem)cache.Current).ProductWorkgroupID = useItem ? inventoryItem.ProductWorkgroupID : itemSite.ProductWorkgroupID;
            ((AMProdItem)cache.Current).ProductManagerID = useItem ? inventoryItem.ProductManagerID : itemSite.ProductManagerID;
        }

        protected virtual void AMProdItem_BOMID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
#if DEBUG
            //TODO: validate is at least a valid BOM ID... 
#endif
            e.Cancel = true;
        }

        protected virtual void EnableFields(PXCache cache, bool IsNewOrder)
        {
            var amProdItem = (AMProdItem)cache.Current;
            var plannedStatus = amProdItem.StatusID == ProductionOrderStatus.Planned || IsNewOrder;
            var closedCancelledStatus = ProductionStatus.IsClosedOrCanceled(amProdItem.StatusID);
            var openStatus = !closedCancelledStatus && amProdItem.StatusID != ProductionOrderStatus.Completed && !amProdItem.Hold.GetValueOrDefault();
            var isUiDirty = IsDirty && !IsImport && !IsContractBasedAPI && !IsInternalSave;
            var enableStatusButton = !IsNewOrder && !isUiDirty &&
                (amProdItem.Function.GetValueOrDefault() == OrderTypeFunction.Regular || amProdItem.Function.GetValueOrDefault() == OrderTypeFunction.Disassemble);
            var editableOrder = amProdItem.StatusID == ProductionOrderStatus.Planned || amProdItem.Hold.GetValueOrDefault();

            plan.SetEnabled(enableStatusButton && ProductionStatus.UserChangeOfStatusValid(amProdItem.StatusID, ProductionOrderStatus.Planned));
            release.SetEnabled(enableStatusButton && ProductionStatus.UserChangeOfStatusValid(amProdItem.StatusID, ProductionOrderStatus.Released) && !amProdItem.Hold.GetValueOrDefault());
            completeorder.SetEnabled(enableStatusButton && ProductionStatus.UserChangeOfStatusValid(amProdItem.StatusID, ProductionOrderStatus.Completed));
            cancelorder.SetEnabled(enableStatusButton && ProductionStatus.UserChangeOfStatusValid(amProdItem.StatusID, ProductionOrderStatus.Cancel));
            closeorder.SetEnabled(enableStatusButton && ProductionStatus.UserChangeOfStatusValid(amProdItem.StatusID, ProductionOrderStatus.Closed));
            CriticalMatl.SetEnabled(!IsNewOrder);
            ProductionDetails.SetEnabled(!IsNewOrder);
            TransactionsByProductionOrderInq.SetEnabled(!plannedStatus);
            InventoryAllocationDetailInq.SetEnabled(amProdItem?.InventoryID != null && amProdItem?.SiteID != null);
            CreatePurchaseOrderInq.SetEnabled(!IsNewOrder);
            CreateProductionOrderInq.SetEnabled(!IsNewOrder);
            AttributesInq.SetEnabled(!IsNewOrder);
            ViewSchedule.SetEnabled(!IsNewOrder && !closedCancelledStatus);
            releaseMaterial.SetEnabled(!plannedStatus && !closedCancelledStatus && openStatus && amProdItem.Function.GetValueOrDefault() == OrderTypeFunction.Regular);
            createMove.SetEnabled(!plannedStatus && !closedCancelledStatus && openStatus && amProdItem.Function.GetValueOrDefault() == OrderTypeFunction.Regular);
            createLinkedOrders.SetEnabled(!closedCancelledStatus && !IsNewOrder && amProdItem.Function.GetValueOrDefault() == OrderTypeFunction.Regular && amProdItem.StatusID != ProductionOrderStatus.Completed);
            disassemble.SetEnabled(!plannedStatus && !closedCancelledStatus && amProdItem.Function.GetValueOrDefault() == OrderTypeFunction.Disassemble);

            calculatePlanCost.SetEnabled(!IsNewOrder && !isUiDirty && (amProdItem.StatusID == ProductionOrderStatus.Planned
                                                                     || amProdItem.Hold.GetValueOrDefault() &&
                                                                     (amProdItem.StatusID == ProductionOrderStatus.Released ||
                                                                      amProdItem.StatusID == ProductionOrderStatus.InProcess)));

            PXUIFieldAttribute.SetEnabled<AMProdItem.costMethod>(cache, null, plannedStatus && CurrentInventoryItem?.ValMethod != INValMethod.Standard);
            PXUIFieldAttribute.SetEnabled<AMProdItem.uOM>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.inventoryID>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.subItemID>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.detailSource>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.bOMEffDate>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.bOMID>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.bOMRevisionID>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.estimateID>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.estimateRevisionID>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.sourceOrderType>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.sourceProductionNbr>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.siteID>(cache, null, plannedStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.locationID>(cache, null, editableOrder);

            PXUIFieldAttribute.SetEnabled<AMProdItem.prodDate>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.qtytoProd>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.descr>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.schedulingMethod>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.schPriority>(cache, null, editableOrder);

            var userDates = amProdItem.SchedulingMethod == ScheduleMethod.UserDates;
            PXUIFieldAttribute.SetEnabled<AMProdItem.constDate>(cache, null, !userDates && editableOrder);
            PXUIFieldAttribute.SetRequired<AMProdItem.constDate>(cache, !userDates && openStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.startDate>(cache, null, userDates && editableOrder);
            PXUIFieldAttribute.SetRequired<AMProdItem.startDate>(cache, userDates);
            PXUIFieldAttribute.SetEnabled<AMProdItem.endDate>(cache, null, userDates && editableOrder);
            PXUIFieldAttribute.SetRequired<AMProdItem.endDate>(cache, userDates);
            PXUIFieldAttribute.SetEnabled<AMProdItem.fMLTime>(cache, null, FixedMfgTimesEnabled && !userDates && editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.fMLTMRPOrdorOP>(cache, null, editableOrder);

            object schdconst = amProdItem.SchedulingMethod;
            cache.RaiseFieldVerifying<AMProdItem.schedulingMethod>(amProdItem, ref schdconst);

            var wipEnabled = plannedStatus || amProdItem.Hold.GetValueOrDefault() && amProdItem.StatusID == ProductionOrderStatus.Released;
            PXUIFieldAttribute.SetEnabled<AMProdItem.wIPAcctID>(cache, null, wipEnabled);
            PXUIFieldAttribute.SetEnabled<AMProdItem.wIPSubID>(cache, null, wipEnabled);
            PXUIFieldAttribute.SetEnabled<AMProdItem.updateProject>(cache, null, plannedStatus || amProdItem.StatusID == ProductionOrderStatus.Released);
            PXUIFieldAttribute.SetEnabled<AMProdItem.projectID>(cache, null, plannedStatus || amProdItem.StatusID == ProductionOrderStatus.Released);
            PXUIFieldAttribute.SetEnabled<AMProdItem.taskID>(cache, null, plannedStatus || amProdItem.StatusID == ProductionOrderStatus.Released);
            PXUIFieldAttribute.SetEnabled<AMProdItem.costCodeID>(cache, null, plannedStatus || amProdItem.StatusID == ProductionOrderStatus.Released);

            PXUIFieldAttribute.SetEnabled<AMProdItem.customerID>(cache, null, editableOrder && string.IsNullOrWhiteSpace(amProdItem.OrdNbr));
            PXUIFieldAttribute.SetEnabled<AMProdItem.parentOrderType>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.parentOrdID>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.productOrderType>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.productOrdID>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.wIPVarianceAcctID>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.wIPVarianceSubID>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.productWorkgroupID>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.productManagerID>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.hold>(cache, null, !closedCancelledStatus);
            PXUIFieldAttribute.SetEnabled<AMProdItem.excludeFromMRP>(cache, null, editableOrder);

            // Enable fields for Scrap
            PXUIFieldAttribute.SetEnabled<AMProdItem.scrapOverride>(cache, null, editableOrder);
            PXUIFieldAttribute.SetEnabled<AMProdItem.scrapSiteID>(cache, null, editableOrder && amProdItem.ScrapOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMProdItem.scrapLocationID>(cache, null, editableOrder && amProdItem.ScrapOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetVisible<AMProdItem.scrapOverride>(cache, null, amProdItem.Function.GetValueOrDefault() != OrderTypeFunction.Disassemble);
            PXUIFieldAttribute.SetVisible<AMProdItem.scrapSiteID>(cache, null, amProdItem.Function.GetValueOrDefault() != OrderTypeFunction.Disassemble);
            PXUIFieldAttribute.SetVisible<AMProdItem.scrapLocationID>(cache, null, amProdItem.Function.GetValueOrDefault() != OrderTypeFunction.Disassemble);
        }

        protected virtual void SetFieldsSource(PXCache cache, AMProdItem item)
        {
            var source = item.DetailSource.GetValueOrDefault();

            PXUIFieldAttribute.SetVisible<AMProdItem.bOMID>(cache, item, source == ProductionDetailSource.BOM);
            PXUIFieldAttribute.SetVisible<AMProdItem.bOMRevisionID>(cache, item, source == ProductionDetailSource.BOM);

            PXUIFieldAttribute.SetVisible<AMProdItem.estimateID>(cache, item, source == ProductionDetailSource.Estimate);
            PXUIFieldAttribute.SetVisible<AMProdItem.estimateRevisionID>(cache, item, source == ProductionDetailSource.Estimate);

            PXUIFieldAttribute.SetVisible<AMProdItem.sourceOrderType>(cache, item, source == ProductionDetailSource.ProductionRef);
            PXUIFieldAttribute.SetVisible<AMProdItem.sourceProductionNbr>(cache, item, source == ProductionDetailSource.ProductionRef);

            //This is the only control we are putting visible from the AMProdItem RowSelected event because it's the only one
            //registered in this cache. The other ones are registered in the AMConfigurationResults Cache and thus we are
            //setting their Visible/enable status there.
            ConfigureEntry.SetVisible(source == ProductionDetailSource.Configuration);
            Reconfigure.SetVisible(source == ProductionDetailSource.Configuration);
            PXUIFieldAttribute.SetVisible<AMConfigurationResults.configurationID>(ItemConfiguration.Cache, ItemConfiguration.Current, source == ProductionDetailSource.Configuration);
            PXUIFieldAttribute.SetVisible<AMConfigurationResults.keyID>(ItemConfiguration.Cache, ItemConfiguration.Current, source == ProductionDetailSource.Configuration);
        }

        public virtual string GetDefaultBomID(AMProdItem proditem)
        {
            if (proditem != null)
            {
                return new PrimaryBomIDManager(this).GetPrimaryAllLevels(proditem.InventoryID, proditem.SiteID, proditem.SubItemID);
            }

            return null;
        }

        protected virtual AMBomItem GetDefaultBomItem(AMProdItem proditem)
        {
            return PrimaryBomIDManager.GetActiveRevisionBomItemByDate(this, GetDefaultBomID(proditem), proditem?.BOMEffDate);
        }

        private AMBomItem setBomRevBomItem;
        public virtual void SetBomRevision(PXCache cache, AMProdItem prodItem)
        {
            try
            {
                setBomRevBomItem = GetDefaultBomItem(prodItem);

                if (setBomRevBomItem?.BOMID == null)
                {
                    if (prodItem?.BOMID != null)
                    {
                        cache.SetValueExt<AMProdItem.bOMID>(prodItem, null);
                    }

                    return;
                }

                cache.SetValueExt<AMProdItem.bOMID>(prodItem, setBomRevBomItem.BOMID);
            }
            finally
            {
                setBomRevBomItem = null;
            }
        }

        public virtual string GetConfigBomID()
        {
            ItemConfigurationLoadSelect();
            return ItemConfiguration?.Configuration?.Current?.BOMID;
        }

        protected virtual void ItemConfigurationLoadSelect()
        {
            //Unless the user goes to the REFERENCES tab the currents are not set correctly so cannot use the selects. Set currents here if not set correctly
            
            if (ProdMaintRecords?.Current == null)
            {
                return;
            }

            var configResult = ItemConfiguration?.Current;
            if (ItemConfiguration != null && string.IsNullOrWhiteSpace(configResult?.ConfigurationID))
            {
                ItemConfiguration.Current = ItemConfiguration.Select();
            }

            var config = ItemConfiguration?.Configuration?.Current;
            if (ItemConfiguration?.Configuration != null && string.IsNullOrWhiteSpace(config?.ConfigurationID))
            {
                ItemConfiguration.Configuration.Current = ItemConfiguration.Configuration.Select();
            }
        }
        
        protected virtual void _(Events.RowUpdated<AMProdItem> e)
        {
            var reschedule = e.Row.Reschedule == true;
            var buildProductionBom = e.Row.BuildProductionBom == true;

            if(reschedule && buildProductionBom)
            {
                return;
            }

            if (!reschedule)
            {
                reschedule = !e.Cache.ObjectsEqual<AMProdItem.constDate, AMProdItem.fMLTime>(e.Row, e.OldRow);
            }

            var valuesChanged = !e.Cache.ObjectsEqual<AMProdItem.estimateID, AMProdItem.sourceOrderType, AMProdItem.sourceProductionNbr>(e.Row, e.OldRow);
            if (valuesChanged)
            {
                reschedule = true;
                buildProductionBom = true;
            }

            e.Cache.SetValueExt<AMProdItem.reschedule>(e.Row, reschedule);
            e.Cache.SetValueExt<AMProdItem.buildProductionBom>(e.Row, buildProductionBom);
        }

        protected virtual void AMProdItem_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            var amproditem = (AMProdItem)e.Row;
            if (amproditem == null)
            {
                return;
            }

            amproditem.BuildProductionBom = false;
            amproditem.Reschedule = false;

            RemoveProductAndParentReferences(amproditem);

            RemovePlanReferences(amproditem);
        }

        protected virtual void AMProdItem_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null)
            {
                return;
            }

            if (amProdItem.Hold.GetValueOrDefault())
            {
                throw new PXSetPropertyException(Messages.GetLocal(Messages.ProdStatusDeleteInvalid, Messages.Hold));
            }

            if (!amProdItem.StatusID.EqualsWithTrim(ProductionOrderStatus.Planned))
            {
                var label = ProductionOrderStatus.GetStatusDescription(amProdItem.StatusID.TrimIfNotNullEmpty().ToUpper());
                throw new PXSetPropertyException(Messages.GetLocal(Messages.ProdStatusDeleteInvalid, label));
            }

            if (ContainsSOReference)
            {
                AMConfigurationResults confRow = ItemConfiguration.Current;
                if (ProdMaintRecords.Ask(Messages.ConfirmDeleteTitle, Messages.ConfirmSOLinkedOrderDelete, MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    e.Cancel = true;
                }
                else if (confRow != null)
                {
                    //We don't want to delete the Configuration if it's still linked with a SOLine
                    PXParentAttribute.SetLeaveChildren<AMConfigurationResults.prodOrderNbr>(Caches[typeof(AMConfigurationResults)], null, true);

                    //We want to remove the linked with the defunct production order.
                    confRow.ProdOrderType = null;
                    confRow.ProdOrderNbr = null;
                    this.ItemConfiguration.Update(confRow);
                }
            }

            var linkedOrders = new StringBuilder();

            // Check for existing Linked orders by ProductOrderType and ProductOrdID
            AMProdItem productItem = PXSelect<AMProdItem,
                Where<AMProdItem.productOrderType, Equal<Required<AMProdItem.productOrderType>>,
                And<AMProdItem.productOrdID, Equal<Required<AMProdItem.productOrdID>>
                >>>.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID);
            if (productItem != null)
            {
                linkedOrders = GetLinkedOrdersByProduct(amProdItem, linkedOrders);
            }

            // Check for existing Linked orders by ParentOrderType and ParentOrdID
            AMProdItem parentItem = PXSelect<AMProdItem,
                Where<AMProdItem.parentOrderType, Equal<Required<AMProdItem.parentOrderType>>,
                    And<AMProdItem.parentOrdID, Equal<Required<AMProdItem.parentOrdID>>
                    >>>.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID);
            if (parentItem != null)
            {
                linkedOrders = GetLinkedOrdersByParent(amProdItem, linkedOrders);
            }

            if (linkedOrders.Length != 0 && ProdMaintRecords.Ask(Messages.ConfirmDeleteTitle, linkedOrders.ToString(), MessageButtons.YesNo) != WebDialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            if (ProductionTransactionHelper.ProductionOrderHasUnreleasedTransactions(this, amProdItem, out var unreleasedMsg))
            {
                throw new PXException(unreleasedMsg);
            }
        }

        private StringBuilder GetLinkedOrdersByProduct(AMProdItem amProdItem, StringBuilder builder)
        {
            var strOrder = Messages.OrderHasProductReferences;
            builder.AppendLine(strOrder);

            foreach (AMProdItem prodItem in PXSelect<AMProdItem,
                Where<AMProdItem.productOrderType, Equal<Required<AMProdItem.productOrderType>>,
                And<AMProdItem.productOrdID, Equal<Required<AMProdItem.productOrdID>>
                >>>.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                builder.AppendLine($"{prodItem.OrderType.TrimIfNotNullEmpty()} - {prodItem.ProdOrdID.TrimIfNotNullEmpty()}");
            }

            return builder;
        }

        private StringBuilder GetLinkedOrdersByParent(AMProdItem amProdItem, StringBuilder builder)
        {
            var strOrder = Messages.OrderHasParentReferences;
            builder.AppendLine(strOrder);

            foreach (AMProdItem prodItem in PXSelect<AMProdItem,
                Where<AMProdItem.parentOrderType, Equal<Required<AMProdItem.parentOrderType>>,
                And<AMProdItem.parentOrdID, Equal<Required<AMProdItem.parentOrdID>>
                >>>.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                builder.AppendLine($"{prodItem.OrderType.TrimIfNotNullEmpty()} - {prodItem.ProdOrdID.TrimIfNotNullEmpty()}");
            }

            return builder;
        }

        protected virtual void RemoveProductAndParentReferences(AMProdItem amProdItem)
        {
            foreach (AMProdItemRelated relatedOrder in PXSelect<
                AMProdItemRelated,
                Where2<
                    Where<AMProdItemRelated.parentOrderType, Equal<Required<AMProdItemRelated.parentOrderType>>,
                        And<AMProdItemRelated.parentOrdID, Equal<Required<AMProdItemRelated.parentOrdID>>>>,
                    Or<Where<AMProdItemRelated.productOrderType, Equal<Required<AMProdItemRelated.productOrderType>>,
                        And<AMProdItemRelated.productOrdID, Equal<Required<AMProdItemRelated.productOrdID>>>>>>>
                .Select(this, amProdItem.OrderType, amProdItem.ProdOrdID, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                if (relatedOrder?.InventoryID == null)
                {
                    continue;
                }
                
                var row = RemoveProductAndParentReferences(relatedOrder);
                if(row?.InventoryID == null)
                {
                    continue;
                }

                RelatedProdItems.Update(row);
            }
        }

        protected virtual AMProdItemRelated RemoveProductAndParentReferences(AMProdItemRelated relatedOrder)
        {
            if (relatedOrder?.InventoryID == null)
            {
                return relatedOrder;
            }

            relatedOrder.ParentOrderType = null;
            relatedOrder.ParentOrdID = null;
            relatedOrder.ProductOrderType = null;
            relatedOrder.ProductOrdID = null;

            return relatedOrder;
        }

        protected virtual void AMProdMatl_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            var row = (AMProdMatl)e.Row;
            var isUpSert = e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update;
            if (row == null || !isUpSert || ProdMaintRecords?.Current?.Function == null)
            {
                return;
            }

            if (row.BFlush.GetValueOrDefault() && ProdMaintRecords.Current.Function == OrderTypeFunction.Disassemble)
            {
                row.BFlush = false;
            }
        }

        /// <summary>
        /// Insert a new production order <see cref="ProductionEventType.OrderCreated"/> event
        /// </summary>
        protected virtual void InsertCreatedOrderEventMessage(AMProdItem amProdItem)
        {
            string prodEventDescription = string.IsNullOrWhiteSpace(amProdItem.OrdNbr)
                ? GetCreatedFunctionMessage(amProdItem)
                : Messages.GetLocal(Messages.CreatedFromOrderTypeOrderNbr, amProdItem.OrdTypeRef.TrimIfNotNullEmpty(), amProdItem.OrdNbr.TrimIfNotNullEmpty());

            InsertCreatedOrderEventMessage(prodEventDescription, null);
        }

        /// <summary>
        /// Insert a new production order <see cref="ProductionEventType.OrderCreated"/> event based on the current order
        /// </summary>
        internal virtual void InsertCreatedOrderEventMessage(string description, Guid? refNoteID)
        {
            var prodEvent = ProductionEventHelper.BuildEvent(ProductionEventType.OrderCreated, description);
            prodEvent.RefNoteID = refNoteID;
            ProdEventRecords.Insert(prodEvent);
        }

        private bool HasCreatedProdEvent()
        {
            foreach (AMProdEvnt prodEvnt in ProdEventRecords.Cache.Inserted)
            {
                if (prodEvnt?.LineNbr == null || prodEvnt.EventType != ProductionEventType.OrderCreated)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        protected virtual string GetCreatedFunctionMessage(AMProdItem amProdItem)
        {
            return Messages.GetLocal(Messages.CreatedOrder, OrderTypeFunction.GetDescription(amProdItem.Function));
        }

        protected static void CalculatePlanCosts(AMProdItem prodItem)
        {
            var prodDetail = CreateInstance<ProdDetail>();
            prodDetail.ProdItemRecords.Current = prodDetail.ProdItemRecords.Search<AMProdItem.prodOrdID>(prodItem.OrderType, prodItem.ProdOrdID);

            if (prodDetail.ProdItemRecords.Current == null)
            {
                return;
            }

            //
            //  UpdatePlannedMaterialCosts
            //
            BOMCostRoll.UpdatePlannedMaterialCosts(prodDetail, prodDetail.ProdItemRecords.Current);
            prodDetail.PersistBase();

            //
            //  UpdatePlanProductionTotals
            //
            AMProdTotal amProdTotal = prodDetail.ProdTotalRecs.Select() ?? prodDetail.ProdTotalRecs.Insert(new AMProdTotal());
            ProductionTransactionHelper.UpdatePlannedProductionTotals(prodDetail, prodDetail.ProdItemRecords.Current, amProdTotal);

            if (prodDetail.ProdItemRecords.Current.StatusID == ProductionOrderStatus.InProcess)
            {
                prodDetail.ProdEventRecords.Insert(ProductionEventHelper.BuildEvent(ProductionEventType.Info,
                    Messages.GetLocal(Messages.CalculatePlanCost), prodDetail.ProdItemRecords.Current));
            }
            prodDetail.PersistBase();
        }

        /// <summary>
        /// Update the AMProdTotal cache with the calculated planned cost values
        /// </summary>
        protected virtual void UpdatePlanProductionTotals()
        {
            AMProdTotal amProdTotal = ProdTotalRecs.Select() ?? ProdTotalRecs.Insert(new AMProdTotal());
            ProductionTransactionHelper.UpdatePlannedProductionTotals(this, ProdMaintRecords.Current, amProdTotal);

            if (ProdMaintRecords.Current.StatusID == ProductionOrderStatus.InProcess)
            {
                ProdEventRecords.Insert(ProductionEventHelper.BuildEvent(ProductionEventType.Info,
                    Messages.GetLocal(Messages.CalculatePlanCost), ProdMaintRecords.Current));
            }
        }

        /// <summary>
        /// Update the production material planned unit cost values
        /// </summary>
        /// <param name="amProdItem">The production item record involded in the process</param>
        protected virtual void UpdatePlannedMaterialCosts(AMProdItem amProdItem)
        {
            var prodDetail = CreateInstance<ProdDetail>();
            BOMCostRoll.UpdatePlannedMaterialCosts(prodDetail, amProdItem);
            prodDetail.PersistBase();
        }

        /// <summary>
        /// Convert a BOM to the production order details
        /// </summary>
        /// <param name="amproditem">The production item record involved in the process</param>
        public virtual void CopyBomToProductionDetails(AMProdItem amproditem)
        {
            if (amproditem == null || !amproditem.BuildProductionBom.GetValueOrDefault()
                || amproditem.DetailSource == null
                || !amproditem.StatusID.EqualsWithTrim(ProductionOrderStatus.Planned)
                || amproditem.SiteID == null)
            {
                return;
            }

            using (new DisableSelectorValidationScope(ProdMaintRecords.Cache))
            using (new DisableSelectorValidationScope(ProdOperRecords.Cache))
            using (new DisableSelectorValidationScope(ProdMatlRecords.Cache))
            using (new DisableSelectorValidationScope(ProdOvhdRecords.Cache))
            using (new DisableSelectorValidationScope(ProdToolRecords.Cache))
            using (new DisableSelectorValidationScope(ProdStepRecords.Cache))
            using (new DisableSelectorValidationScope(ProductionAttributes.Cache))
            {
                switch (amproditem.DetailSource)
                {
                    case ProductionDetailSource.BOM:
                        if (!string.IsNullOrWhiteSpace(amproditem.BOMID))
                        {
                            var productionBomCopy = CreateInstance<ProductionBomCopy>();
                            productionBomCopy.ProcessingGraph = this;
                            productionBomCopy.UnattendedMode = true;
                            productionBomCopy.CreateProductionDetails(amproditem);
                        }
                        break;
                    case ProductionDetailSource.Estimate:
                        if (!string.IsNullOrWhiteSpace(amproditem.EstimateID)
                            && !string.IsNullOrWhiteSpace(amproditem.EstimateRevisionID))
                        {
                            var productionEstimateCopy = CreateInstance<ProductionEstimateCopy>();
                            productionEstimateCopy.ProcessingGraph = this;
                            productionEstimateCopy.CreateProductionDetails(amproditem);
                        }
                        break;
                    case ProductionDetailSource.Configuration:
                        var productionConfigurationCopy = CreateInstance<ProductionConfigurationCopy>();
                        productionConfigurationCopy.ProcessingGraph = this;
                        productionConfigurationCopy.ConfigResults.Current = ItemConfiguration.Current;
                        productionConfigurationCopy.CreateProductionDetails(amproditem);
                        break;
                    case ProductionDetailSource.ProductionRef:
                        var productionOrderCopy = CreateInstance<ProductionOrderCopy>();
                        productionOrderCopy.ProcessingGraph = this;
                        productionOrderCopy.CreateProductionDetails(amproditem);
                        break;
                }
            }

            var currentProdItem = ProdMaintRecords.Current;
            if (currentProdItem != null)
            {
                currentProdItem.BuildProductionBom = false;
                currentProdItem.Reschedule = true;

                var firstLastOperations = ProdDetail.GetFirstLastOperationIds(ProdOperRecords.Select().ToFirstTableList());

                currentProdItem.FirstOperationID = firstLastOperations?.Item1;
                currentProdItem.LastOperationID = firstLastOperations?.Item2;
                ProdMaintRecords.Update(currentProdItem);
            }

            var prodTotal = ProdTotalRecs.Current;
            if (prodTotal == null)
            {
                return;
            }

            prodTotal.PlanQtyToProduce = amproditem.BaseQtytoProd;
            prodTotal.PlanCostDate = Accessinfo.BusinessDate;
            ProdTotalRecs.Update(prodTotal);
        }

        /// <summary>
        /// Make sure any changes to prod item are in sync with the current schd item record(s)
        /// </summary>
        /// <param name="amProdItem"></param>
        protected virtual void SyncSchdItem(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType)
                || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                return;
            }

            var schdItemUpdates = new List<AMSchdItem>();
            foreach (AMSchdItem schdItem in SchdItemRecords.Select())
            {
                var update = false;
                if (Common.Dates.Compare(amProdItem.ConstDate, schdItem.ConstDate) != 0
                    && !Common.Dates.IsDefaultDate(schdItem.ConstDate))
                {
                    update = true;
                    schdItem.ConstDate = amProdItem.ConstDate;
                }

                if (amProdItem.SchPriority.GetValueOrDefault() != schdItem.SchPriority.GetValueOrDefault())
                {
                    update = true;
                    schdItem.SchPriority = amProdItem.SchPriority.GetValueOrDefault();
                }

                if (amProdItem.FirmSchedule.GetValueOrDefault() != schdItem.FirmSchedule.GetValueOrDefault())
                {
                    update = true;
                    schdItem.FirmSchedule = amProdItem.FirmSchedule.GetValueOrDefault();
                }

                if (amProdItem.InventoryID.GetValueOrDefault() != schdItem.InventoryID.GetValueOrDefault())
                {
                    update = true;
                    schdItem.InventoryID = amProdItem.InventoryID;
                }

                if (amProdItem.SiteID.GetValueOrDefault() != schdItem.SiteID.GetValueOrDefault())
                {
                    update = true;
                    schdItem.SiteID = amProdItem.SiteID;
                }

                if (update)
                {
                    schdItemUpdates.Add(schdItem);
                }
            }

            foreach (var schdItemUpdate in schdItemUpdates)
            {
                SchdItemRecords.Update(schdItemUpdate);
            }
        }

        /// <summary>
        /// Reschedule the production order dates based on run times and shop floor calendar
        /// </summary>
        internal virtual void RescheduleOrder(AMProdItem amProdItem)
        {
            RescheduleOrder(amProdItem, false);
        }

        /// <summary>
        /// Reschedule the production order dates based on run times and shop floor calendar
        /// </summary>
        public virtual void RescheduleOrder(AMProdItem amProdItem, bool persistWithSchedule)
        {
            amProdItem.Reschedule = false;

            if (persistWithSchedule)
            {
                ProductionScheduleEngine.ProcessPersistSchedule(amProdItem);
                return;
            }

            ProductionScheduleEngine.ProcessSchedule(this, amProdItem);
        }

        protected void AMConfigurationResults_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            PXUIFieldAttribute.SetVisible<AMConfigurationResults.configurationID>(cache, e.Row, e.Row != null);
            PXUIFieldAttribute.SetVisible<AMConfigurationResults.revision>(cache, e.Row, e.Row != null);
            PXUIFieldAttribute.SetVisible<AMConfigurationResults.keyID>(cache, e.Row, e.Row != null);

            if (e.Row != null)
            {
                var configIsPersisted = cache.GetStatus(e.Row) != PXEntryStatus.Inserted;
                var currentOrderIsClosedCanceled = ProdMaintRecords.Current == null || ProductionStatus.IsClosedOrCanceled(ProdMaintRecords.Current.StatusID);
                PXUIFieldAttribute.SetEnabled<AMConfigurationResults.configurationID>(cache, e.Row, !configIsPersisted && !currentOrderIsClosedCanceled);
                PXUIFieldAttribute.SetEnabled<AMConfigurationResults.revision>(cache, e.Row, !configIsPersisted && !currentOrderIsClosedCanceled);
                PXUIFieldAttribute.SetEnabled<AMConfigurationResults.keyID>(cache, e.Row, ProdMaintRecords.Current != null && ProdMaintRecords.Current.StatusID == ProductionOrderStatus.Planned &&
                    !((AMConfigurationResults)e.Row).IsSalesReferenced.GetValueOrDefault() && !((AMConfigurationResults)e.Row).IsOpportunityReferenced.GetValueOrDefault());
                ConfigureEntry.SetEnabled(configIsPersisted && !currentOrderIsClosedCanceled);
                Reconfigure.SetEnabled(configIsPersisted && ProdMaintRecords.Current != null && ProdMaintRecords.Current.StatusID != null && ProdMaintRecords.Current.StatusID == ProductionOrderStatus.Planned);
            }
        }

        protected void AMProdItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var amProdItem = (AMProdItem)e.Row;
            if (amProdItem == null)
            {
                return;
            }

            SetFieldsSource(cache, amProdItem);
            EnableFields(cache, cache.GetStatus(amProdItem) == PXEntryStatus.Inserted);
            var isOpenOrder = amProdItem.StatusID != ProductionOrderStatus.Closed && amProdItem.StatusID != ProductionOrderStatus.Cancel;

            ProductionAttributes.AllowDelete = amProdItem.StatusID == ProductionOrderStatus.Planned;
            ProductionAttributes.AllowInsert = amProdItem.StatusID == ProductionOrderStatus.Planned;
            ProductionAttributes.AllowUpdate = isOpenOrder;

            ProdEventRecords.AllowInsert =  
                ProdEventRecords.AllowUpdate =
                    ProdEventRecords.AllowDelete = isOpenOrder;
        }

        protected virtual void AMProdOper_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            DeleteSchdOperDetail(e.Row as AMProdOper);
        }

        /// <summary>
        /// Does the given parameters require the configuration to be rebuild/changed?
        /// </summary>
        /// <param name="inventoryID">Inventory ID to verify</param>
        /// <param name="siteID">Warehouse ID to verify</param>
        /// <param name="configuration">current related configuration</param>
        /// <returns>True if a change of configuration is required</returns>
        private bool ConfigurationChangeRequired(int? inventoryID, int? siteID, AMConfigurationResults configuration)
        {
            string configurationID;
            var isSuccessful = ItemConfiguration.TryGetDefaultConfigurationID(inventoryID, siteID, out configurationID);
            return ConfigurationChangeRequired(isSuccessful ? configurationID : configuration?.ConfigurationID, configuration) && ItemConfiguration.IsConfiguratorActive;
        }

        /// <summary>
        /// Does the given parameters require the configuration to be rebuild/changed?
        /// </summary>
        private bool ConfigurationChangeRequired(string configurationID, AMConfigurationResults configuration)
        {
            return configuration == null || !configuration.ConfigurationID.EqualsWithTrim(configurationID);
        }

        private void UpdateConfigurationResult(AMProdItem row, AMConfigurationResults configuration)
        {
            if (row.InventoryID == null || row.SiteID == null)
            {
                return;
            }

            ItemConfiguration.TryGetDefaultConfigurationID(row.InventoryID, row.SiteID, out var configurationID);

            UpdateConfigurationResult(row, configuration, configurationID);
        }

        private void UpdateConfigurationResult(AMProdItem row, AMConfigurationResults configuration, string configurationID)
        {
            UpdateConfigurationResult(row, configuration, configurationID, ConfigurationChangeRequired(configurationID, configuration));
        }

        private void UpdateConfigurationResult(AMProdItem row, AMConfigurationResults configuration, string configurationID, bool newConfigRequired)
        {
            if (row.InventoryID == null || row.SiteID == null)
            {
                return;
            }

            var isConfigSource = row.IsConfigurable.GetValueOrDefault() && row.DetailSource == ProductionDetailSource.Configuration;
            if (!newConfigRequired && isConfigSource)
            {
                return;
            }

            var isCurrentConfig = configuration != null;
            if (isCurrentConfig)
            {
                isCurrentConfig = ItemConfiguration.Delete(configuration) == null;
            }

            if (isConfigSource && !isCurrentConfig && !string.IsNullOrWhiteSpace(configurationID) && ItemConfiguration.IsConfiguratorActive)
            {
                ItemConfiguration.Insert(new AMConfigurationResults
                {
                    ConfigurationID = configurationID,
                    InventoryID = row.InventoryID
                });
            }
        }

        protected virtual void AMProdAttribute_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMProdAttribute)e.Row;
            if (row == null || row.Level == null)
            {
                return;
            }

            if (row.Source != AMAttributeSource.Production
                  || ProdMaintRecords.Current.StatusID != ProductionOrderStatus.Planned)
            {
                PXUIFieldAttribute.SetEnabled(sender, row, false);
            }

            PXUIFieldAttribute.SetEnabled<AMProdAttribute.value>(sender, e.Row,
                row.Enabled.GetValueOrDefault()
                && ProdMaintRecords.Current.StatusID != ProductionOrderStatus.Closed
                && ProdMaintRecords.Current.StatusID != ProductionOrderStatus.Cancel
                && row.Source != AMAttributeSource.Configuration);
        }

        protected virtual void AMProdAttribute_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            var prodAttribute = (AMProdAttribute)e.Row;
            if (prodAttribute == null)
            {
                return;
            }

            if (ProdMaintRecords.Cache.GetStatus(ProdMaintRecords.Current) == PXEntryStatus.Deleted)
            {
                //Allows for full order deletion while still in planned status
                return;
            }

            if (prodAttribute.Source != AMAttributeSource.Production)
            {
                e.Cancel = true;
                throw new PXException(Messages.GetLocal(Messages.CannotDeleteSourceAttributes, AMAttributeSource.GetDescription(prodAttribute.Source)));
            }
        }

        protected virtual void AMProdAttribute_AttributeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMProdAttribute)e.Row;
            if (row == null)
            {
                return;
            }

            var item = PXSelectorAttribute.Select<AMProdAttribute.attributeID>(sender, row) as CSAttribute;
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                sender.SetValueExt<AMProdAttribute.label>(row, item.AttributeID);
            }
            if (string.IsNullOrWhiteSpace(row.Descr))
            {
                sender.SetValueExt<AMProdAttribute.descr>(row, item.Description);
            }
        }

        protected virtual void AMProdAttribute_OperationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMProdAttribute)e.Row;
            if (row == null)
            {
                return;
            }

            if (ProdMaintRecords.Current != null &&
                ProdMaintRecords.Cache.GetStatus(ProdMaintRecords.Current) == PXEntryStatus.Inserted)
            {
                // Covers a late incorrect verifying on opernbr when building the production detials...
                e.Cancel = true;
            }
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

        protected virtual void AMProdItem_UpdateProject_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var newBoolValue = Convert.ToBoolean(e.NewValue);
            if (newBoolValue != true)
            {
                return;
            }

            if (!ProjectHelper.IsProjectFeatureEnabled())
            {
                e.NewValue = false;
                return;
            }

            var order = ProdMaintRecords.Current;
            var item = itemrecord.Current;
            if(!(ProjectHelper.IsProjectAcct(this, order.WIPAcctID) || ProjectHelper.IsProjectAcct(this, item.InvtAcctID)))
            {
                e.NewValue = false;
                cache.RaiseExceptionHandling<AMProdItem.updateProject>(e.Row, e.NewValue, 
                    new PXSetPropertyException(Messages.NotProjectAccounts, PXErrorLevel.Error));
            }
        }

        protected virtual void SubstituteWorkCenters(AMProdItem prodItem)
        {
            foreach (PXResult<AMProdOper, AMWC> resultOper in PXSelectJoin<AMProdOper,
                InnerJoin<AMWC, On<AMWC.wcID, Equal<AMProdOper.wcID>>>,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
                >.Select(this, prodItem.OrderType, prodItem.ProdOrdID))
            {
                var prodOper = (AMProdOper) resultOper;
                var amOperWC = (AMWC)resultOper;

                if (amOperWC.SiteID != prodItem.SiteID)
                {
                    var result = (PXResult<AMWCSubstitute, AMWC>)PXSelectJoin<AMWCSubstitute,
                        LeftJoin<AMWC, On<AMWC.wcID, Equal<AMWCSubstitute.substituteWcID>>>,
                        Where<AMWCSubstitute.wcID, Equal<Required<AMWCSubstitute.wcID>>,
                           And<AMWCSubstitute.siteID, Equal<Required<AMWCSubstitute.siteID>>>>
                        >.Select(this, prodOper.WcID, prodItem.SiteID);

                    var aMWCSubstitute = (AMWCSubstitute)result;
                    var amWC = (AMWC)result;
                    if (aMWCSubstitute != null && amWC != null && amWC.ActiveFlg.GetValueOrDefault())
                    {
                        prodOper.WcID = amWC.WcID;
                        prodOper.BFlush = amWC.BflushLbr.GetValueOrDefault();
                        prodOper.Descr = aMWCSubstitute.UpdateOperDesc.GetValueOrDefault() ? amWC.Descr : prodOper.Descr;
                        ProdOperRecords.Update(prodOper);

                        ProdDetail.DeleteWorkCenterRelatedOverhead(this, prodOper);
                        ProdDetail.CopyOverheadsFromWorkCenter(this, prodOper);
                    }
                }
            }
        }

        protected virtual void _(Events.RowSelected<AMProdEvnt> e)
        {
            if (!CanModifyProdEvent(e.Row))
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMProdEvnt.description>(e.Cache, e.Row, true);

            ProdEventRecords.AllowUpdate = true;
        }

        protected virtual void _(Events.RowDeleting<AMProdEvnt> e)
        {
            if (ProdMaintRecords.Cache.IsCurrentRowDeleted() || CanModifyProdEvent(e.Row))
            {
                return;
            }

            e.Cancel = true;
            throw new PXException(Messages.CannotDeleteProdEvent);
        }

        protected virtual bool CanModifyProdEvent(AMProdEvnt row)
        {
            return ProdEventRecords.AllowInsert && row?.EventType != null &&
                   row.EventType == ProductionEventType.Comment && row.CreatedByID == Accessinfo.UserID;
        }

        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        protected virtual void AMProdItem_Descr_CacheAttached(PXCache sender)
        {
        }

        [Serializable]
        [PXCacheName("Sales Line Update")]
        public class SalesLineUpdate : IBqlTable
        {
            #region UpdateSiteID
            public abstract class updateSiteID : PX.Data.BQL.BqlBool.Field<updateSiteID> { }
            protected Boolean? _UpdateSiteID;
            /// <summary>
            /// Indicates if the sales line warehouse should be updated when persisting the production order changes with the production warehouse
            /// </summary>
            [PXBool]
            [PXUIField(DisplayName = "Update Warehouse")]
            public virtual Boolean? UpdateSiteID
            {
                get
                {
                    return this._UpdateSiteID;
                }
                set
                {
                    this._UpdateSiteID = value;
                }
            }
            #endregion
        }
    }
}
