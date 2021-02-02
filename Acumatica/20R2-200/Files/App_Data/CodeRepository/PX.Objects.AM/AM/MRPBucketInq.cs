using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;
using PX.Objects.AP;

namespace PX.Objects.AM
{
    /// <summary>
    /// Graph for Manufacturing MRP Requirements By Item
    /// </summary>
    public class MRPBucketInq : PXGraph<MRPBucketInq>
    {
        public PXCancel<BucketFilter> Cancel;

        public PXFilter<BucketFilter> BucketLookup;

        public PXSelect<AMMRPBucketInq,
            Where<AMMRPBucketInq.bucketID, Equal<Current<BucketFilter.bucketID>>, 
            And<AMMRPBucketInq.inventoryID, Equal<Current<BucketFilter.inventoryID>>,
            And<AMMRPBucketInq.subItemID, Equal<Current<BucketFilter.subItemID>>,
            And<AMMRPBucketInq.siteID, Equal<Current<BucketFilter.siteID>>>>>>> BucketRecord;

        public PXSelectReadonly<AMMRPBucketDetailInq,
            Where<AMMRPBucketDetailInq.bucketID, Equal<Current<BucketFilter.bucketID>>,
            And<AMMRPBucketDetailInq.inventoryID, Equal<Current<BucketFilter.inventoryID>>,
            And<AMMRPBucketDetailInq.siteID, Equal<Current<BucketFilter.siteID>>,
            And<AMMRPBucketDetailInq.subItemID, Equal<Current<BucketFilter.subItemID>>>>>>,
            OrderBy<Asc<AMMRPBucketDetailInq.fromDate>>> BucketDetailRecords;

        [PXHidden]
        public PXSetup<AMRPSetup> amrpSetup;

        public MRPBucketInq()
        {
            InquiriesMenu.AddMenuAction(MRPDetailInquiry);
            InquiriesMenu.AddMenuAction(InventoryAllocationDetails);
            InquiriesMenu.AddMenuAction(InventoryTransactionDetails);

            if (AM.InventoryHelper.MultiWarehousesFeatureEnabled)
            {
                InquiriesMenu.AddMenuAction(ItemWarehouseDetails);
            }

            BucketRecord.Cache.AutoSave = true;
            BucketDetailRecords.Cache.AutoSave = true;
        }

        public virtual void EnableButtons(bool enable)
        {
            MRPDetailInquiry.SetEnabled(enable);
            ItemWarehouseDetails.SetEnabled(enable);
            InventoryAllocationDetails.SetEnabled(enable);
            InventoryTransactionDetails.SetEnabled(enable);
        }

        /// <summary>
        /// Is the filter incomplete or empty (unable to process detail data)
        /// </summary>
        public bool EmptyFilter =>
            BucketLookup?.Current?.BucketID == null || 
            BucketLookup.Current.InventoryID == null ||
            BucketLookup.Current.SiteID == null ||
            (BucketLookup.Current.SubItemID == null && AM.InventoryHelper.SubItemFeatureEnabled);

        public void BucketFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            EnableButtons(!EmptyFilter);
        }

        protected virtual void BucketFilter_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (BucketLookup.Current == null)
            {
                return;
            }

            sender.SetDefaultExt<BucketFilter.subItemID>(e.Row);
            sender.SetDefaultExt<BucketFilter.siteID>(e.Row);

            SetFilterValues();
        }

        protected virtual void BucketFilter_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetFilterValues();
        }

        protected virtual void BucketFilter_SubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetFilterValues();
        }

        public virtual void SetFilterValues()
        {
            if (BucketLookup.Current == null)
            {
                return;
            }

            if (EmptyFilter)
            {
                return;
            }

            BucketDetailRecords.Cache.Clear();

            AMRPItemSite mrpItemSite = PXSelect<AMRPItemSite,
                Where<AMRPItemSite.inventoryID, Equal<Required<AMRPItemSite.inventoryID>>,
                And2<Where<AMRPItemSite.subItemID, Equal<Required<AMRPItemSite.subItemID>>,
                    Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>,
                And<AMRPItemSite.siteID, Equal<Required<AMRPItemSite.siteID>>
                >>>>.Select(this, BucketLookup.Current.InventoryID, BucketLookup.Current.SubItemID.GetValueOrDefault(),
                                BucketLookup.Current.SiteID);

            BucketLookup.Current.QtyOnHand = 0m;
            BucketLookup.Current.ProductManagerID = null;
            BucketLookup.Current.ReplenishmentSource = null;
            BucketLookup.Current.PreferredVendorID = null;
            BucketLookup.Current.LeadTime = 0;
            BucketLookup.Current.SafetyStock = 0m;

            if (mrpItemSite != null)
            {
                BucketLookup.Current.QtyOnHand = mrpItemSite.QtyOnHand.GetValueOrDefault();
                BucketLookup.Current.ProductManagerID = mrpItemSite.ProductManagerID;
                BucketLookup.Current.ReplenishmentSource = mrpItemSite.ReplenishmentSource;
                BucketLookup.Current.PreferredVendorID = mrpItemSite.PreferredVendorID;
                BucketLookup.Current.LeadTime = mrpItemSite.LeadTime.GetValueOrDefault();
                BucketLookup.Current.SafetyStock = amrpSetup.Current.StockingMethod == AMRPSetup.MRPStockingMethod.SafetyStock
                    ? mrpItemSite.SafetyStock.GetValueOrDefault() : mrpItemSite.ReorderPoint.GetValueOrDefault();
            }
        }

        protected virtual void CheckMrpRunning()
        {
            try
            {
                PXLongOperationHelper.CheckForProcessIsRunningByCompany(typeof(Fullregen));
            }
            catch (Exception e)
            {
                if (e is PXException && PXLongOperationHelper.ExceptionContainsDataKeys(e))
                {
                    throw new PXException(AM.Messages.GetLocal(AM.Messages.MrpIsRunningWaitFinished, ((PXException)e).MessageNoPrefix), e);
                }
                throw;
            }
        }

        protected virtual IEnumerable bucketDetailRecords()
        {
            if (BucketLookup.Current == null)
            {
                yield break;
            }

            if (EmptyFilter)
            {
                yield break;
            }

            CheckMrpRunning();

            AMMRPBucketInq bucketInq = PXSelect<AMMRPBucketInq,
                Where<AMMRPBucketInq.bucketID, Equal<Required<AMMRPBucketInq.bucketID>>,
                And<AMMRPBucketInq.inventoryID, Equal<Required<AMMRPBucketInq.inventoryID>>,
                And2<Where<AMMRPBucketInq.subItemID, Equal<Required<AMMRPBucketInq.subItemID>>,
                    Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>,
                And<AMMRPBucketInq.siteID, Equal<Required< AMMRPBucketInq.siteID>>
                >>>>>.Select(this, BucketLookup.Current.BucketID, BucketLookup.Current.InventoryID, 
                    BucketLookup.Current.SubItemID.GetValueOrDefault(), BucketLookup.Current.SiteID);

            if (bucketInq == null)
            {
                FillBuckets();
            }

            bool itVar1 = false;
            IEnumerator enumerator = this.BucketDetailRecords.Cache.Inserted.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AMMRPBucketDetailInq itVar2 = (AMMRPBucketDetailInq)enumerator.Current;
                itVar1 = true;
                yield return itVar2;
            }

            if (!itVar1)
            {
                PXSelectBase<AMMRPBucketDetailInq> bucketDetailRecords = new PXSelect<AMMRPBucketDetailInq,
                    Where<AMMRPBucketDetailInq.bucketID, Equal<Current<BucketFilter.bucketID>>,
                    And<AMMRPBucketDetailInq.inventoryID, Equal<Current<BucketFilter.inventoryID>>,
                    And<AMMRPBucketDetailInq.siteID, Equal<Current<BucketFilter.siteID>>>>>,
                    OrderBy<Asc<AMMRPBucketDetailInq.fromDate>>>(this);

                if (AM.InventoryHelper.SubItemFeatureEnabled)
                {
                    bucketDetailRecords.WhereAnd<Where<AMMRPBucketDetailInq.subItemID, Equal<Current<BucketFilter.subItemID>>>>();
                }

                foreach (AMMRPBucketDetailInq bucket in bucketDetailRecords.Select())
                {
                    var row = bucket;
                    yield return row;
                }
            }
        }

        /// <summary>
        /// Make the buckets related to from/to dates including past due
        /// </summary>
        /// <returns></returns>
        protected virtual List<AMMRPBucketDetailInq> GetBuckets()
        {
            var list = new List<AMMRPBucketDetailInq>();
            DateTime? fromDate = Accessinfo.BusinessDate.GetValueOrDefault().AddDays(-1);

            //Buckets for past due
            foreach (AMMRPBucketDetail bucket in PXSelect<AMMRPBucketDetail,
                Where<AMMRPBucketDetail.bucketID, Equal<Required<AMMRPBucketDetail.bucketID>>,
                    And<AMMRPBucketDetail.bucket, LessEqual<Zero>>>,
                OrderBy<Desc<AMMRPBucketDetail.bucket>>>.Select(this, BucketLookup.Current.BucketID))
            {
                var toDate = GetEndDate(fromDate, bucket, true);

                list.Insert(0, new AMMRPBucketDetailInq
                {
                    BucketID = BucketLookup.Current.BucketID,
                    Bucket = bucket.Bucket,
                    InventoryID = BucketLookup.Current.InventoryID,
                    SubItemID = BucketLookup.Current.SubItemID,
                    SiteID = BucketLookup.Current.SiteID,
                    FromDate = toDate,
                    ToDate = fromDate,
                });

                fromDate = toDate.AddDays(-1);
            }

            fromDate = Accessinfo.BusinessDate;

            //Buckets from today going forward
            foreach (AMMRPBucketDetail bucket in PXSelect<AMMRPBucketDetail,
                Where<AMMRPBucketDetail.bucketID, Equal<Required<AMMRPBucketDetail.bucketID>>,
                    And<AMMRPBucketDetail.bucket, Greater<Zero>>
                    >>.Select(this, BucketLookup.Current.BucketID))
            {
                var toDate = GetEndDate(fromDate, bucket);

                list.Add(new AMMRPBucketDetailInq
                {
                    BucketID = BucketLookup.Current.BucketID,
                    Bucket = bucket.Bucket,
                    InventoryID = BucketLookup.Current.InventoryID,
                    SubItemID = BucketLookup.Current.SubItemID,
                    SiteID = BucketLookup.Current.SiteID,
                    FromDate = fromDate,
                    ToDate = toDate,
                });

                fromDate = toDate.AddDays(1);
            }
            return list;
        }

        protected virtual void FillBuckets()
        {
            if (EmptyFilter)
            {
                return;
            }

            var newBucketRecord = BucketRecord.Insert(new AMMRPBucketInq
            {
                BucketID = BucketLookup.Current.BucketID,
                InventoryID = BucketLookup.Current.InventoryID,
                SubItemID = BucketLookup.Current.SubItemID,
                SiteID = BucketLookup.Current.SiteID,
                QtyOnHand = BucketLookup.Current.QtyOnHand.GetValueOrDefault(),
                ProductManagerID = BucketLookup.Current.ProductManagerID,
                SafetyStock = BucketLookup.Current.SafetyStock.GetValueOrDefault(),
                ReplenishmentSource = BucketLookup.Current.ReplenishmentSource,
                PreferredVendorID = BucketLookup.Current.PreferredVendorID,
                LeadTime = BucketLookup.Current.LeadTime.GetValueOrDefault()
            });

            decimal? beginQty = newBucketRecord.QtyOnHand.GetValueOrDefault();
#if DEBUG
            var sb = new System.Text.StringBuilder();
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            foreach (var bucket in GetBuckets())
            {
                var newBucket = PXCache<AMMRPBucketDetailInq>.CreateCopy(bucket);
                newBucket.BeginQty = beginQty;
                SetQuantities(ref newBucket);
                BucketDetailRecords.Insert(newBucket);

                beginQty = newBucket.EndQty.GetValueOrDefault();
#if DEBUG
                sb.AppendLine($"Bucket {bucket.Bucket}  = {Common.Strings.TimespanMessage(sw.Elapsed)}");
#endif
            }
#if DEBUG
            sw.Stop();
            sb.AppendLine($"Total = {Common.Strings.TimespanMessage(sw.Elapsed)}");
            AMDebug.TraceWriteLine(sb.ToString());
#endif
        }

        /// <summary>
        /// Calculate the end date from a given start date and interval bucket record
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="bucket"></param>
        /// <returns>Calculated end date based on given interval</returns>
        public static DateTime GetEndDate(DateTime? startDate, AMMRPBucketDetail bucket)
        {
            return GetEndDate(startDate, bucket, false);
        }

        /// <summary>
        /// Calculate the end date from a given start date and interval bucket record
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="bucket"></param>
        /// <param name="reverse">Is the calculation processing in reverse (days past)</param>
        /// <returns>Calculated end date based on given interval</returns>
        public static DateTime GetEndDate(DateTime? startDate, AMMRPBucketDetail bucket, bool reverse)
        {
            if (startDate == null)
            {
                throw new PXArgumentException(nameof(startDate));
            }

            var mult = reverse ? -1 : 1;
            switch (bucket.Interval)
            {
                case AMIntervals.Week:
                    return startDate.GetValueOrDefault().AddDays(bucket.Value.GetValueOrDefault() * mult * 6);
                case AMIntervals.Month:
                    return startDate.GetValueOrDefault().AddMonths(bucket.Value.GetValueOrDefault() * mult).AddDays(-1*mult);
                case AMIntervals.Year:
                    return startDate.GetValueOrDefault().AddYears(bucket.Value.GetValueOrDefault() * mult).AddDays(-1 * mult);
            }

            // Covers Day
            return startDate.GetValueOrDefault();
        }

        /// <summary>
        /// Determines if the MRPPlanningType value is an MRP plan type (if not then its an Actual value)
        /// </summary>
        /// <param name="mrpPlan"></param>
        /// <returns>True if the type is for planning</returns>
        protected static bool IsPlanType(AMRPPlan mrpPlan)
        {
            return mrpPlan.Type == MRPPlanningType.MPS
                   || mrpPlan.Type == MRPPlanningType.MrpPlan
                   || mrpPlan.Type == MRPPlanningType.MrpRequirement
                   || mrpPlan.Type == MRPPlanningType.ForecastDemand
                   || mrpPlan.Type == MRPPlanningType.MPS;
        }

        protected virtual void SetQuantities(ref AMMRPBucketDetailInq bucketDetailInq)
        {
            if (bucketDetailInq == null)
            {
                throw new PXArgumentException(nameof(bucketDetailInq));
            }

            bucketDetailInq.ActualDemand = 0m;
            bucketDetailInq.ActualSupply = 0m;
            bucketDetailInq.PlannedDemand = 0m;
            bucketDetailInq.PlannedSupply = 0m;

            foreach (AMRPPlan mrpPlan in PXSelect<AMRPPlan,
                Where<AMRPPlan.inventoryID, Equal<Required<AMRPPlan.inventoryID>>,
                And2<Where<AMRPPlan.subItemID, Equal<Required<AMRPPlan.subItemID>>,
                    Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>,
                And<AMRPPlan.siteID, Equal<Required<AMRPPlan.siteID>>,
                And<AMRPPlan.promiseDate, Between<Required<AMRPPlan.promiseDate>, Required<AMRPPlan.promiseDate>>
                >>>>>.Select(this, bucketDetailInq.InventoryID, bucketDetailInq.SubItemID,
                    bucketDetailInq.SiteID, bucketDetailInq.FromDate, bucketDetailInq.ToDate))
            {
                var isPlanType = IsPlanType(mrpPlan);
                if (mrpPlan.SDflag == MRPSDFlag.Supply)
                {
                    if (isPlanType)
                    {
                        bucketDetailInq.PlannedSupply += Math.Abs(mrpPlan.BaseQty.GetValueOrDefault());
                        continue;
                    }

                    bucketDetailInq.ActualSupply += Math.Abs(mrpPlan.BaseQty.GetValueOrDefault());
                    continue;
                }

                if (isPlanType)
                {
                    bucketDetailInq.PlannedDemand += Math.Abs(mrpPlan.BaseQty.GetValueOrDefault());
                    continue;
                }

                bucketDetailInq.ActualDemand += Math.Abs(mrpPlan.BaseQty.GetValueOrDefault());
            }

            bucketDetailInq.NetQty = bucketDetailInq.BeginQty.GetValueOrDefault() +
                                     bucketDetailInq.ActualSupply.GetValueOrDefault() -
                                     bucketDetailInq.ActualDemand.GetValueOrDefault();
            bucketDetailInq.EndQty = bucketDetailInq.NetQty.GetValueOrDefault() +
                                     bucketDetailInq.PlannedSupply.GetValueOrDefault() -
                                     bucketDetailInq.PlannedDemand.GetValueOrDefault();
        }

        public PXAction<BucketFilter> MRPDetailInquiry;
        [PXUIField(DisplayName = AM.Messages.MRPDetailInquiry, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable mRPDetailInquiry(PXAdapter adapter)
        {
            if (!EmptyFilter)
            {
                MRPDetail.Redirect(new InvLookup
                {
                    InventoryID = BucketLookup.Current.InventoryID,
                    SiteID = BucketLookup.Current.SiteID,
                    SubItemID = AM.InventoryHelper.SubItemFeatureEnabled ? BucketLookup.Current.SubItemID : null
                });
            }
            return adapter.Get();
        }

        public PXAction<BucketFilter> ItemWarehouseDetails;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.INItemSiteMaint, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable itemWarehouseDetails(PXAdapter adapter)
        {
            if (!EmptyFilter)
            {
                var graph = CreateInstance<INItemSiteMaint>();

                INItemSite itemsite = PXSelect<INItemSite,
                Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(graph, BucketLookup.Current.InventoryID, BucketLookup.Current.SiteID);

                if (itemsite == null)
                {
                    return adapter.Get();
                }

                graph.itemsitesettings.Current = itemsite;
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
            }
            return adapter.Get();
        }

        public PXAction<BucketFilter> InventoryAllocationDetails;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.InventoryAllocDetEnq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable inventoryAllocationDetails(PXAdapter adapter)
        {
            if (!EmptyFilter)
            {
                InventoryAllocDetEnq inventoryAllocDetEnqGraph = CreateInstance<InventoryAllocDetEnq>();
                inventoryAllocDetEnqGraph.Filter.Current.InventoryID = BucketLookup.Current.InventoryID;
                inventoryAllocDetEnqGraph.Filter.Current.SubItemCD = GetCurrentSubItemCD();
                inventoryAllocDetEnqGraph.Filter.Current.SiteID = BucketLookup.Current.SiteID;
                PXRedirectHelper.TryRedirect(inventoryAllocDetEnqGraph, PXRedirectHelper.WindowMode.New);
            }
            return adapter.Get();
        }

        public PXAction<BucketFilter> InventoryTransactionDetails;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.InventoryTranDetEnq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable inventoryTransactionDetails(PXAdapter adapter)
        {
            if (!EmptyFilter)
            {
                InventoryTranDetEnq inventoryTranDetEnqGraph = CreateInstance<InventoryTranDetEnq>();
                inventoryTranDetEnqGraph.Filter.Current.InventoryID = BucketLookup.Current.InventoryID;
                inventoryTranDetEnqGraph.Filter.Current.SubItemCD = GetCurrentSubItemCD();
                inventoryTranDetEnqGraph.Filter.Current.SiteID = BucketLookup.Current.SiteID;
                PXRedirectHelper.TryRedirect(inventoryTranDetEnqGraph, PXRedirectHelper.WindowMode.New);
            }
            return adapter.Get();
        }

        protected virtual string GetCurrentSubItemCD()
        {
            return ((INSubItem)PXSelect<INSubItem, Where<INSubItem.subItemID, Equal<Required<INSubItem.subItemID>>
                    >>.Select(this, BucketLookup.Current.SubItemID))?.SubItemCD;
        }

        #region Menus
        public PXAction<BucketFilter> InquiriesMenu;
        [PXButton(CommitChanges = true, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Inquiries")]
        protected virtual void inquiriesMenu()
        {

        }

        public PXAction<BucketFilter> ReportsMenu;
        [PXButton(CommitChanges = true, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports")]
        protected virtual void reportsMenu()
        {

        }
        #endregion
    }

    /// <summary>
    /// Filter dac
    /// </summary>
    [Serializable]
    [PXCacheName(AM.Messages.BucketFilter)]
    public class BucketFilter : IBqlTable
    {
        #region Bucket ID
        public abstract class bucketID : PX.Data.BQL.BqlString.Field<bucketID> { }

        protected String _BucketID;
        [PXString(30, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Bucket ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMMRPBucket.bucketID, Where<AMMRPBucket.activeFlg, Equal<True>>>))]
        public virtual String BucketID
        {
            get
            {
                return this._BucketID;
            }
            set
            {
                this._BucketID = value;
            }
        }
        #endregion
        #region Inventory ID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem]
        [PXDefault]
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
        #region SubItem ID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(typeof(BucketFilter.inventoryID), Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<BucketFilter.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
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
        #region Site ID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site]
        [PXDefault(typeof(Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current<BucketFilter.inventoryID>>>>))]
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
        #region Qty On Hand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }

        protected Decimal? _QtyOnHand;
        [PXQuantity]
        [PXUIField(DisplayName = "Qty. on Hand", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? QtyOnHand
        {
            get
            {
                return this._QtyOnHand;
            }
            set
            {
                this._QtyOnHand = value;
            }
        }
        #endregion
        #region Product Manager ID
        public abstract class productManagerID : PX.Data.BQL.BqlInt.Field<productManagerID> { }

        protected int? _ProductManagerID;
        [PX.TM.Owner(DisplayName = "Product Mgr.", Enabled = false)]
        public virtual int? ProductManagerID
        {
            get
            {
                return this._ProductManagerID;
            }
            set
            {
                this._ProductManagerID = value;
            }
        }
        #endregion
        #region Safety Stock
        public abstract class safetyStock : PX.Data.BQL.BqlDecimal.Field<safetyStock> { }

        protected Decimal? _SafetyStock;
        [PXQuantity]
        [PXUIField(DisplayName = "Safety Stock", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? SafetyStock
        {
            get
            {
                return this._SafetyStock;
            }
            set
            {
                this._SafetyStock = value;
            }
        }
        #endregion
        #region Replenishment Source
        public abstract class replenishmentSource : PX.Data.BQL.BqlString.Field<replenishmentSource> { }

        protected string _ReplenishmentSource;
        [PXString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Rep. Source", Enabled = false)]
        [INReplenishmentSource.List]
        public virtual string ReplenishmentSource
        {
            get
            {
                return this._ReplenishmentSource;
            }
            set
            {
                this._ReplenishmentSource = value;
            }
        }
        #endregion
        #region Preferred Vendor ID
        public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

        protected Int32? _PreferredVendorID;
        [Vendor(DisplayName = "Preferred Vendor ID", Enabled = false, Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName))]
        public virtual Int32? PreferredVendorID
        {
            get
            {
                return this._PreferredVendorID;
            }
            set
            {
                this._PreferredVendorID = value;
            }
        }
        #endregion
        #region Lead Time
        public abstract class leadTime : PX.Data.BQL.BqlInt.Field<leadTime> { }

        protected Int32? _LeadTime;
        [PXInt]
        [PXUIField(DisplayName = "Lead Time", Enabled = false)]
        [PXUnboundDefault(TypeCode.Int32, "0")]
        public virtual Int32? LeadTime
        {
            get
            {
                return this._LeadTime;
            }
            set
            {
                this._LeadTime = value;
            }
        }
        #endregion
    }
}
