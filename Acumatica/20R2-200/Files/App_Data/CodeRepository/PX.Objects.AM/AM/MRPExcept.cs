using System.Collections;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// MRP Exceptions
    /// </summary>
    public class MRPExcept : PXGraph<MRPExcept>
    {
        [PXFilterable]
        public PXSelect<AMRPExceptions> ExceptRecs;
        public PXSetup<AMRPSetup> Setup;

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

        #endregion

        public MRPExcept()
        {
            ExceptRecs.AllowInsert = false;
            ExceptRecs.AllowUpdate = false;
            ExceptRecs.AllowDelete = false;

            InquiresDropMenu.AddMenuAction(mrpDetailInquiry);
            InquiresDropMenu.AddMenuAction(inventorySummary);
            InquiresDropMenu.AddMenuAction(inventoryAllocationDetails);
        }

        public PXAction<AMRPExceptions> InquiresDropMenu;
        [PXUIField(DisplayName = AM.Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected IEnumerable inquiresDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMRPExceptions> inventorySummary;

        [PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable InventorySummary(PXAdapter adapter)
        {
            var line = ExceptRecs.Current;
            if (line == null)
            {
                return adapter.Get();
            }

            var item = (InventoryItem)PXSelectorAttribute.Select<AMRPExceptions.inventoryID>(ExceptRecs.Cache, line);
            if (item != null && item.StkItem == true)
            {
                var sbitem = (INSubItem)PXSelectorAttribute.Select<AMRPExceptions.subItemID>(ExceptRecs.Cache, line);
                InventorySummaryEnq.Redirect(item.InventoryID,
                                             sbitem != null ? sbitem.SubItemCD : null,
                                             line.SiteID,
                                             null);
            }
            return adapter.Get();
        }

        public PXAction<AMRPExceptions> inventoryAllocationDetails;

        [PXUIField(DisplayName = "Inventory Allocation Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable InventoryAllocationDetails(PXAdapter adapter)
        {
            var line = ExceptRecs.Current;
            if (line == null)
            {
                return adapter.Get();
            }

            var item = (InventoryItem)PXSelectorAttribute.Select<AMRPExceptions.inventoryID>(ExceptRecs.Cache, line);
            if (item != null && item.StkItem == true)
            {
                var sbitem = (INSubItem)PXSelectorAttribute.Select<AMRPExceptions.subItemID>(ExceptRecs.Cache, line);
                InventoryAllocDetEnq.Redirect(item.InventoryID,
                    sbitem != null ? sbitem.SubItemCD : null,
                    null,
                    line.SiteID,
                    null);
            }
            return adapter.Get();
        }

        public PXAction<AMRPExceptions> mrpDetailInquiry;

        [PXUIField(DisplayName = AM.Messages.MRPDetailInquiry, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable MrpDetailInquiry(PXAdapter adapter)
        {
            var line = ExceptRecs.Current;
            if (line == null)
            {
                return adapter.Get();
            }

            MRPDetail.Redirect(new InvLookup
            {
                InventoryID = line.InventoryID,
                SiteID = line.SiteID,
                SubItemID = AM.InventoryHelper.SubItemFeatureEnabled ? line.SubItemID : null
            });

            return adapter.Get();
        }
    }
}