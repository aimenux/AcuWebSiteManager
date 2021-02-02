using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Advanced Planning and Scheduling Rough Cut Process
    /// </summary>
    public class APSRoughCutProcess : PXGraph<APSRoughCutProcess>
    {
        public PXCancel<AMSchdItem> Cancel;

        /// <summary>
        /// Processing records
        /// </summary>
        [PXFilterable]
        public PXProcessingJoin<AMSchdItem,
            InnerJoin<AMProdItem, On<AMSchdItem.orderType, Equal<AMProdItem.orderType>,
                And<AMSchdItem.prodOrdID, Equal<AMProdItem.prodOrdID>>>>,
            Where<AMProdItem.statusID, Equal<ProductionOrderStatus.planned>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>>>>> OrderList;

        /// <summary>
        /// Processing page filter
        /// </summary>
        public PXFilter<APSRoughCutProcessFilter> Filter;

        public APSRoughCutProcess()
        {
            var filter = Filter.Current;
            OrderList.SetProcessDelegate(
            delegate (List<AMSchdItem> list)
            {
                ProcessSchedule(list, filter, true);
            });

            OrderList.SetProcessCaption("Schedule");
            OrderList.SetProcessAllCaption("Schedule All");

            PXUIFieldAttribute.SetEnabled<AMSchdItem.schPriority>(OrderList.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMSchdItem.constDate>(OrderList.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMSchdItem.firmSchedule>(OrderList.Cache, null, true);

            InquiresMenu.AddMenuAction(OrderScheduleInquiry);
            InquiresMenu.AddMenuAction(ProductionDetails);
            InquiresMenu.AddMenuAction(CriticalMatl);
            InquiresMenu.AddMenuAction(InventoryAllocationDetailInq);
        }

        public PXAction<AMSchdItem> InquiresMenu;
        [PXUIField(DisplayName = Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable inquiresMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMSchdItem> OrderScheduleInquiry;
        [PXUIField(DisplayName = "Order Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable orderscheduleInquiry(PXAdapter adapter)
        {
            if (OrderList.Current != null)
            {
                var gi = new GIWorkCenterSchedule();
                gi.SetParameter(GIWorkCenterSchedule.Parameters.OrderType, OrderList.Current.OrderType);
                gi.SetParameter(GIWorkCenterSchedule.Parameters.ProductionNbr, OrderList.Current.ProdOrdID);
                gi.CallGenericInquiry(PXBaseRedirectException.WindowMode.New);
            }
            return adapter.Get();
        }

        public PXAction<AMSchdItem> ProductionDetails;
        [PXUIField(DisplayName = Messages.ProductionDetail, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable productionDetails(PXAdapter adapter)
        {
            if (OrderList.Current != null)
            {
                var pd = CreateInstance<ProdDetail>();
                pd.ProdItemRecords.Current = pd.ProdItemRecords.Search<AMProdItem.prodOrdID>(OrderList.Current.ProdOrdID, OrderList.Current.OrderType);

                if (pd.ProdItemRecords.Current?.ProdOrdID != null)
                {
                    PXRedirectHelper.TryRedirect(pd, PXRedirectHelper.WindowMode.New);
                }
            }
            return adapter.Get();
        }

        public PXAction<AMSchdItem> CriticalMatl;
        [PXUIField(DisplayName = Messages.CriticalMaterial, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable criticalMatl(PXAdapter adapter)
        {
            if (OrderList.Current != null)
            {
                var cm = CreateInstance<CriticalMaterialsInq>();
                cm.ProdItemRecs.Current.OrderType = OrderList.Current.OrderType;
                cm.ProdItemRecs.Current.ProdOrdID = OrderList.Current.ProdOrdID;
                cm.ProdItemRecs.Current.ShowAll = false;
                throw new PXRedirectRequiredException(cm, Messages.CriticalMaterial);
            }
            return adapter.Get();
        }

        public PXAction<AMSchdItem> InventoryAllocationDetailInq;
        [PXUIField(DisplayName = "Allocation Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable inventoryAllocationDetailInq(PXAdapter adapter)
        {
            if (OrderList?.Current?.InventoryID != null)
            {
                var allocGraph = CreateInstance<InventoryAllocDetEnq>();
                allocGraph.Filter.Current.InventoryID = OrderList.Current.InventoryID;
                if (InventoryHelper.SubItemFeatureEnabled)
                {
                    var subItem = (INSubItem)PXSelectorAttribute.Select<AMProdItem.subItemID>(OrderList.Cache, OrderList.Current);
                    if (!string.IsNullOrWhiteSpace(subItem?.SubItemCD))
                    {
                        allocGraph.Filter.Current.SubItemCD = subItem.SubItemCD;
                    }
                }
                allocGraph.Filter.Current.SiteID = OrderList.Current.SiteID;
                allocGraph.RefreshTotal.Press();
                PXRedirectHelper.TryRedirect(allocGraph, PXRedirectHelper.WindowMode.New);
            }

            return adapter.Get();
        }

        public static void ProcessSchedule(List<AMSchdItem> list, APSRoughCutProcessFilter filter, bool isMassProcess)
        {
            var failed = false;
            var schdEngine = CreateInstance<ProductionScheduleEngineAdv>();
            var schdList = schdEngine.OrderScheduleBy(list);

            if (schdList == null || schdList.Count == 0)
            {
                PXTrace.WriteInformation("No schedules to process");
                return;
            }

            try
            {
                //will persist the schedule delete on first successful order schedule below
                schdEngine.DeleteSchedule(schdList, false);
            }
            catch
            {
                PXTrace.WriteWarning("Error deleting schedules");
                throw;
            }

            for (var i = 0; i < schdList.Count; i++)
            {
                var schdItem = schdList[i];
                if (schdItem == null)
                {
                    continue;
                }

                PXTrace.WriteInformation($"Scheduling {schdItem.OrderType.TrimIfNotNullEmpty()} {schdItem.ProdOrdID.TrimIfNotNullEmpty()} with dispatch priority {schdItem.SchPriority.GetValueOrDefault()}");
#if DEBUG
                AMDebug.TraceWriteMethodName(
                    $"Processing '{schdItem.OrderType.TrimIfNotNullEmpty()}' '{schdItem.ProdOrdID.TrimIfNotNullEmpty()}' with dispatch priority '{schdItem.SchPriority.GetValueOrDefault()}' and firm '{schdItem.FirmSchedule.GetValueOrDefault()}' and current end date {schdItem.EndDate}");
#endif

                try
                {
                    schdEngine.Clear();
                    schdEngine.Process(schdEngine.SchdItems.Update(schdItem));
                    schdEngine.Persist();

                    if (filter != null && filter.ReleaseOrders.GetValueOrDefault())
                    {
                        var prodItem = (AMProdItem)schdEngine.ProdItems.Cache.Locate(new AMProdItem
                        {
                            OrderType = schdItem.OrderType,
                            ProdOrdID = schdItem.ProdOrdID
                        }) ?? PXSelect<AMProdItem,
                                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>
                            .Select(schdEngine, schdItem.OrderType, schdItem.ProdOrdID);

                        if (prodItem != null && prodItem.StatusID == ProductionOrderStatus.Planned)
                        {
                            ProductionStatus.SetStatus(new List<AMProdItem> { prodItem }, ProductionOrderStatus.Released);
                        }
                    }

                    if (isMassProcess)
                    {
                        PXProcessing<AMSchdItem>.SetInfo(i, PX.Data.ActionsMessages.RecordProcessed);
                    }
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                    failed = true;

                    if (isMassProcess)
                    {
                        PXProcessing<AMSchdItem>.SetError(i, e);
                    }
                    else if (schdList.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }
        }
    }

    /// <summary>
    /// Filter DAC for APSRoughCutProcess graph
    /// </summary>
    [Serializable]
    [PXCacheName("Rough Cut Process Filter")]
    public class APSRoughCutProcessFilter : IBqlTable
    {
        #region ReleaseOrders
        /// <summary>
        /// During processing, should the selected orders be released (true)
        /// </summary>
        public abstract class releaseOrders : PX.Data.BQL.BqlBool.Field<releaseOrders> { }

        protected Boolean? _ReleaseOrders;
        /// <summary>
        /// During processing, should the selected orders be released (true)
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Release Orders")]
        public virtual Boolean? ReleaseOrders
        {
            get
            {
                return this._ReleaseOrders;
            }
            set
            {
                this._ReleaseOrders = value;
            }
        }
        #endregion
    }
}