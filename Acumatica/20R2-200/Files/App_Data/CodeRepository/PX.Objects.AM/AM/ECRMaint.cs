using PX.Data;
using PX.Objects.IN;
using System;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Objects.CS;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.AM
{
    /// <summary>
    /// Engineering Change Request graph
    /// Main graph for managing a Engineering Change Request (ECR)
    /// </summary>
    public class ECRMaint : PXGraph<ECRMaint, AMECRItem>
    {
        [PXViewName(Messages.ECRItem)]
        public PXSelect<AMECRItem> Documents;

        [PXImport(typeof(AMECRItem))]
        public PXSelect<AMBomOper,
            Where<AMBomOper.bOMID, Equal<Current<AMECRItem.eCRID>>,
                And<AMBomOper.revisionID, Equal<AMECRItem.eCRRev>>>,
            OrderBy<Asc<AMBomOper.operationCD>>> BomOperRecords;

        [PXImport(typeof(AMECRItem))]
        [PXCopyPasteHiddenFields]
        public AMOrderedMatlSelect<AMECRItem, AMBomMatl,
            Where<AMBomMatl.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomMatl.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomMatl.operationID, Equal<Current<AMBomOper.operationID>>>>>,
            OrderBy<Asc<AMBomMatl.sortOrder, Asc<AMBomMatl.lineID>>>> BomMatlRecords;

        [PXImport(typeof(AMECRItem))]
        public PXSelect<AMBomStep,
            Where<AMBomStep.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomStep.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomStep.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomStepRecords;

        [PXImport(typeof(AMECRItem))]
        public PXSelectJoin<AMBomTool,
            InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
            Where<AMBomTool.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomTool.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomTool.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomToolRecords;

        [PXImport(typeof(AMECRItem))]
        public PXSelectJoin<AMBomOvhd,
            InnerJoin<AMOverhead, On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
            Where<AMBomOvhd.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomOvhd.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomOvhd.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomOvhdRecords;

        public PXSelect<AMBomRef,
            Where<AMBomRef.bOMID, Equal<Current<AMBomMatl.bOMID>>,
                And<AMBomRef.revisionID, Equal<Current<AMBomMatl.revisionID>>,
                And<AMBomRef.operationID, Equal<Current<AMBomMatl.operationID>>,
                And<AMBomRef.matlLineID, Equal<Current<AMBomMatl.lineID>>>>>>> BomRefRecords;

        public PXSetup<AMBSetup> ambsetup;
        public PXSetup<AMPSetup> ProdSetup;

        public PXSelect<AMECRItem, Where<AMECRItem.eCRID, Equal<Current<AMECRItem.eCRID>>>> CurrentDocument;

        [PXHidden]
        public PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Current<AMECRItem.inventoryID>>,
            And<INItemSite.siteID, Equal<Current<AMECRItem.siteID>>>>> ItemSiteRecord;

        [PXHidden]
        public PXSelect<
            AMBomAttribute,
            Where<AMBomAttribute.bOMID, Equal<Current<AMECRItem.eCRID>>,
                And<AMBomAttribute.revisionID, Equal<AMECRItem.eCRRev>>>> BomAttributes;

        [PXHidden]
        public PXSelect<AMBomOper,
            Where<AMBomOper.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomOper.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomOper.operationID, Equal<Current<AMBomOper.operationID>>>>>> OutsideProcessingOperationSelected;

        public ECRMaint()
        {
            var bomSetup = ambsetup.Current;
            if (string.IsNullOrWhiteSpace(bomSetup?.ECRNumberingID))
            {
                throw new BOMSetupNotEnteredException();
            }
            ActionDropMenu.AddMenuAction(CreateECO);
            ActionDropMenu.AddMenuAction(BOMCompare);

            var prodSetup = ProdSetup.Current;
            AMPSetup.CheckNeedsUpgrade(prodSetup);
        }

        public PXSelect<AMECRSetupApproval> SetupApproval;

        [PXViewName(PX.Objects.EP.Messages.Approval)]
        public PX.Objects.EP.EPApprovalAutomation<AMECRItem, AMECRItem.approved, AMECRItem.rejected, AMECRItem.hold, AMECRSetupApproval> Approval;

        #region CACHE ATTACHED

        [PXDBBool]
        [PXDefault(false, typeof(Search<AMWC.bflushMatl, Where<AMWC.wcID, Equal<Current<AMBomOper.wcID>>>>))]
        [PXUIField(DisplayName = "Backflush")]
        protected virtual void AMBomMatl_BFlush_CacheAttached(PXCache sender)
        {
        }

        [BomID(DisplayName = "Comp BOM ID")]
        [BOMIDSelector(typeof(Search2<AMBomItemActive.bOMID,
            LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMBomItemActive.inventoryID>>>,
            Where<AMBomItemActive.inventoryID, Equal<Current<AMBomMatl.inventoryID>>>>))]
        protected virtual void AMBomMatl_CompBOMID_CacheAttached(PXCache sender)
        {
        }

        [OperationIDField(IsKey = true, Visible = false, Enabled = false, DisplayName = "Operation DB ID")]
        [PXLineNbr(typeof(AMECRItem.lineCntrOperation))]
        protected virtual void AMBomOper_OperationID_CacheAttached(PXCache sender)
        {
#if DEBUG
            //Cache attached to change display name so we can provide the user with a way to see the DB ID if needed 
#endif
        }

        [BomID(IsKey = true, Visible = false, Enabled = false)]
        [BOMIDSelector(ValidateValue = false)]
        [PXDBDefault(typeof(AMECRItem.eCRID))]
        [PXParent(typeof(Select<AMECRItem, Where<AMECRItem.eCRID, Equal<Current<AMBomOper.bOMID>>,
            And<AMECRItem.eCRRev, Equal<Current<AMBomOper.revisionID>>>>>))]
        protected virtual void AMBomOper_BOMID_CacheAttached(PXCache sender)
        {
        }

        [BomID(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMECRItem.eCRID))]
        [PXParent(typeof(Select<AMECRItem, Where<AMECRItem.eCRID, Equal<Current<AMBomAttribute.bOMID>>,
            And<AMECRItem.eCRRev, Equal<Current<AMBomAttribute.revisionID>>>>>))]
        protected virtual void AMBomAttribute_BOMID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMECRItem.lineCntrAttribute))]
        protected virtual void AMBomAttribute_LineNbr_CacheAttached(PXCache sender)
        { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECRItem))]
        [PXDefault]
        protected virtual void AMBomAttribute_RowStatus_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECRItem))]
        [PXDefault]
        protected virtual void AMBomMatl_RowStatus_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECRItem))]
        [PXDefault]
        protected virtual void AMBomOper_RowStatus_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECRItem))]
        [PXDefault]
        protected virtual void AMBomOvhd_RowStatus_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECRItem))]
        [PXDefault]
        protected virtual void AMBomRef_RowStatus_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECRItem))]
        [PXDefault]
        protected virtual void AMBomStep_RowStatus_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECRItem))]
        [PXDefault]
        protected virtual void AMBomTool_RowStatus_CacheAttached(PXCache sender) { }

        [PXDBDate]
        [PXDefault(typeof(AMECRItem.requestDate), PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXDefault(typeof(AMECRItem.requestor), PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var ecr = Documents.Current;
            if (ecr != null)
            {
                InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<AMECRItem.inventoryID>(Documents.Cache, ecr);
                if(item != null)
                {
                    e.NewValue = ECRMaint.BOMRevItemDisplay(ecr.BOMID, ecr.BOMRevisionID, item.InventoryCD);
                }
            }
        }

        protected virtual void EPApproval_Descr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var ecr = Documents.Current;
            if (ecr != null)
            {
                e.NewValue = ecr.Descr;
            }
        }

        #endregion

        #region EP Approval Actions

        public PXAction<AMECRItem> hold;
        [PXUIField(DisplayName = "Hold", Visible = false, MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Hold(PXAdapter adapter)
        {
            IEnumerable<AMECRItem> items = adapter.Get<AMECRItem>().ToArray();

            Save.Press();

            foreach (AMECRItem item in items)
            {
                item.Hold = true;
                item.Approved = false;
                item.Rejected = false;
                Documents.Update(item);

                Save.Press();

                yield return item;
            }
        }

        public PXAction<AMECRItem> approve;
        public PXAction<AMECRItem> reject;

        [PXUIField(DisplayName = "Approve", Visible = false, MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable Approve(PXAdapter adapter)
        {
            IEnumerable<AMECRItem> items = adapter.Get<AMECRItem>().ToArray();

            Save.Press();

            foreach (AMECRItem item in items)
            {
                item.Approved = true;
                Documents.Update(item);

                Save.Press();

                yield return item;
            }
        }

        [PXUIField(DisplayName = "Reject", Visible = false, MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable Reject(PXAdapter adapter)
        {
            IEnumerable<AMECRItem> items = adapter.Get<AMECRItem>().ToArray();

            Save.Press();

            foreach (AMECRItem item in items)
            {
                item.Rejected = true;
                Documents.Update(item);

                Save.Press();

                yield return item;
            }
        }

        public PXAction<AMECRItem> submit;

        [PXUIField(DisplayName = "Submit", Visible = false, MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable Submit(PXAdapter adapter)
        {
            IEnumerable<AMECRItem> items = adapter.Get<AMECRItem>().ToArray();

            Save.Press();

            foreach (AMECRItem item in items)
            {
                item.Hold = false;
                item.Rejected = false;
                Documents.Update(item);

                Save.Press();

                yield return item;
            }
           
        }

        public PXAction<AMECRItem> ActionDropMenu;
        [PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable actionDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMECRItem> CreateECO;
        [PXUIField(DisplayName = "Create ECO", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable createECO(PXAdapter adapter)
        {
            var currentItem = Documents.Current;
            if (currentItem?.RevisionID == null)
            {
                return adapter.Get();
            }

            var ecoGraph = CreateInstance<ECOMaint>();

            var newEco = ecoGraph.Documents.Insert();
            if (newEco == null)
            {
                return adapter.Get();
            }
            ecoGraph.CopyECRtoECO(ecoGraph.Documents.Cache, newEco, currentItem);
            ecoGraph.UpdateECRStatus(currentItem, AMECRStatus.Completed);

            PXRedirectHelper.TryRedirect(ecoGraph, PXRedirectHelper.WindowMode.NewWindow);

            return adapter.Get();
        }
        #endregion

        public PXAction<AMECRItem> BOMCompare;
        [PXUIField(DisplayName = "Compare BOM", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable bOMCompare(PXAdapter adapter)
        {
            var item = Documents.Current;
            if (item != null)
            {
                var graph = CreateInstance<BOMCompareInq>();

                graph.Filter.Current.IDType1 = BOMCompareInq.IDTypes.ECR;
                graph.Filter.Current.ECRID1 = item.ECRID;
                graph.Filter.Current.BOMID1 = item.ECRID;
                graph.Filter.Current.RevisionID1 = AMECRItem.ECRRev;

                graph.Filter.Current.IDType2 = BOMCompareInq.IDTypes.BOM;
                graph.Filter.Current.BOMID2 = item.BOMID;
                graph.Filter.Current.RevisionID2 = item.BOMRevisionID;
                throw new PXRedirectRequiredException(graph, Messages.BOMCompare);
            }

            return adapter.Get();
        }
        
        protected virtual void AMECRItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            Approval.AllowSelect = ambsetup?.Current?.ECRRequestApproval == true;

            var item = (AMECRItem)e.Row;
            if (item == null)
            {
                return;
            }

            EnableECRItemFields(sender, item);
            // When inserted we want to disable because updated row status is not possible as no save yet
            var holdEnabled = item.Hold.GetValueOrDefault() && !sender.IsRowInserted(item);
            EnableOperCache(holdEnabled);
            EnableOperChildCache(holdEnabled);
            EnableApprovedActions(!sender.IsRowInserted(item) && item.Status == AMECRStatus.Approved);
            EnableButtons(!sender.IsRowInserted(item));
        }

        protected virtual void EnableOperCache(bool enabled)
        {
            BomOperRecords.AllowInsert = enabled;
            BomOperRecords.AllowUpdate = enabled;
            BomOperRecords.AllowDelete = enabled;
        }

        protected virtual void EnableOperChildCache(bool enabled)
        {
            BomMatlRecords.AllowInsert = enabled;
            BomMatlRecords.AllowUpdate = enabled;
            BomMatlRecords.AllowDelete = enabled;

            BomStepRecords.AllowInsert = enabled;
            BomStepRecords.AllowUpdate = enabled;
            BomStepRecords.AllowDelete = enabled;

            BomOvhdRecords.AllowInsert = enabled;
            BomOvhdRecords.AllowUpdate = enabled;
            BomOvhdRecords.AllowDelete = enabled;

            BomToolRecords.AllowInsert = enabled;
            BomToolRecords.AllowUpdate = enabled;
            BomToolRecords.AllowDelete = enabled;

            BomRefRecords.AllowInsert = enabled;
            BomRefRecords.AllowUpdate = enabled;
            BomRefRecords.AllowDelete = enabled;
        }

        protected virtual void EnableApprovedActions(bool enable)
        {
            CreateECO.SetEnabled(enable);
        }

        protected virtual void EnableButtons(bool enable)
        {
            BOMCompare.SetEnabled(enable);
        }

        protected virtual void AMECRItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            var row = (AMECRItem) e.Row;
            if (row?.BOMID == null || row.BOMRevisionID == null || cache.ObjectsEqual<AMECRItem.bOMID, AMECRItem.bOMRevisionID>(row, e.OldRow))
            {
                return;
            }

            CopyBomToECR(cache, row,
                PXSelect<AMBomItem, Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                    .Select(this, row.BOMID, row.BOMRevisionID));
        }

        protected virtual void AMECRItem_RevisionID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMECRItem)e.Row;
            if(row == null || e.NewValue == null)
            {
                return;
            }
            AMBomItem item = PXSelect<AMBomItem, Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>.Select(this, row.BOMID, e.NewValue);
            if (item != null)
            {
                e.Cancel = true;
                throw new PXSetPropertyException(Messages.GetLocal(Messages.BomRevisionExists), PXErrorLevel.Error, row.BOMID, e.NewValue);
            }
        }

        #region BOM Oper Processes

        protected virtual AMWC GetCurrentWorkcenter()
        {
            AMWC workCenter = PXSelect<AMWC, Where<AMWC.wcID, Equal<Current<AMBomOper.wcID>>>>.Select(this);

            if (this.Caches<AMWC>() != null)
            {
                this.Caches<AMWC>().Current = workCenter;
            }

            return workCenter;
        }

        protected virtual void AMBomOper_WcID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            SetWorkCenterFields(cache, (AMBomOper)e.Row);
        }

        protected virtual void AMBomOper_RevisionID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMECRItem.ECRRev;
        }

        protected virtual void AMBomOper_WcID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetWorkCenterFields(cache, (AMBomOper)e.Row);
        }

        protected virtual void SetWorkCenterFields(PXCache cache, AMBomOper bomOper)
        {
            if (cache == null || bomOper == null)
            {
                return;
            }

            var amWC = GetCurrentWorkcenter();

            if (amWC == null)
            {
                return;
            }

            bool isInsert = cache.GetStatus(bomOper) == PXEntryStatus.Inserted;

            if (string.IsNullOrWhiteSpace(bomOper.Descr) || isInsert)
            {
                cache.SetValueExt<AMBomOper.descr>(bomOper, amWC.Descr);
            }

            if (!bomOper.BFlush.GetValueOrDefault() || isInsert)
            {
                cache.SetValueExt<AMBomOper.bFlush>(bomOper, amWC.BflushLbr.GetValueOrDefault());
            }

            // Set the Scrap Action from Work Center
            cache.SetValueExt<AMBomOper.scrapAction>(bomOper, amWC.ScrapAction);
            cache.SetValueExt<AMBomOper.outsideProcess>(bomOper, amWC.OutsideFlg.GetValueOrDefault());
        }

        protected virtual void AMBomOper_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            var row = (AMBomOper)e.Row;
            if (row == null || Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            AMBomAttribute bomOperAttribute = PXSelect<AMBomAttribute,
                Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>,
                And<AMBomAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>>>
                >>.Select(this, row.BOMID, row.RevisionID, row.OperationID);

            if (bomOperAttribute != null)
            {
                e.Cancel |= BomOperRecords.Ask(Messages.ConfirmDeleteTitle,
                                Messages.GetLocal(Messages.ConfirmOperationDeleteWhenAttributesExist),
                                MessageButtons.YesNo) != WebDialogResult.Yes;
            }

            if (e.Cancel)
            {
                return;
            }

            DeleteBomOperationAttributes(row);
        }

        protected virtual void DeleteBomOperationAttributes(AMBomOper row)
        {
            foreach (AMBomAttribute bomOperAttribute in PXSelect<AMBomAttribute,
                Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                    And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>,
                    And<AMBomAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>
                    >>>>.Select(this, row.BOMID, row.RevisionID, row.OperationID))
            {
                BomAttributes.Delete(bomOperAttribute);
            }
        }

        #endregion

        #region BOM Matl Processes

        protected virtual void CompBOMIDFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void AMBomMatl_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMBomMatl)e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMBomMatl.subItemID>(sender, e.Row, row.IsStockItem.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMBomMatl.subcontractSource>(sender, e.Row, row.MaterialType == AMMaterialType.Subcontract);

            if (IsImport || IsContractBasedAPI)
            {
                return;
            }

            var isMatlExpired = row.ExpDate > Common.Current.BusinessDate(this) || Common.Dates.IsDateNull(row.ExpDate);
            if (!isMatlExpired)
            {
                sender.RaiseExceptionHandling<AMBomMatl.inventoryID>(row, row.InventoryID,
                    new PXSetPropertyException(Messages.MaterialExpiredOnBom, PXErrorLevel.Warning, row.BOMID, row.RevisionID));
            }
        }

        protected virtual void AMBomMatl_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            var matl = (AMBomMatl)e.Row;
            if (matl == null)
            {
                return;
            }

            var subItemFeatureEnabled = InventoryHelper.SubItemFeatureEnabled;

            // Require SUBITEMID when the item is a stock item
            if (subItemFeatureEnabled && matl.InventoryID != null && matl.IsStockItem.GetValueOrDefault() && matl.SubItemID == null)
            {
                cache.RaiseExceptionHandling<AMBomMatl.subItemID>(
                        matl,
                        matl.SubItemID,
                        new PXSetPropertyException(Messages.SubItemIDRequiredForStockItem, PXErrorLevel.Error));
            }

            //  PREVENT A USER FROM ADDING THE MATERIAL ITEM TO ITSELF
            //      More in depth prevention can be added down the road
            if (Documents.Current != null && matl.InventoryID.GetValueOrDefault() != 0)
            {
                if (matl.InventoryID == Documents.Current.InventoryID)
                {
                    if (subItemFeatureEnabled
                        && matl.IsStockItem.GetValueOrDefault()
                        && Documents.Current.SubItemID != null
                        && matl.SubItemID.GetValueOrDefault() != Documents.Current.SubItemID.GetValueOrDefault())
                    {
                        //this should allow different sub items to be consumed on the same BOM as the item being built
                        return;
                    }

                    cache.RaiseExceptionHandling<AMBomMatl.inventoryID>(
                        matl,
                        matl.InventoryID,
                        new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error));
                }
            }
        }

        protected virtual void AMBomMatl_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var amBomMatl = (AMBomMatl)e.Row;
            if (amBomMatl == null || Documents.Current == null
                || e.NewValue == null || amBomMatl.InventoryID == null
                || !InventoryHelper.SubItemFeatureEnabled)
            {
                return;
            }

            int? subItemID = Convert.ToInt32(e.NewValue ?? 0);
            if (amBomMatl.InventoryID == Documents.Current.InventoryID
                && (Documents.Current.SubItemID == null
                || Documents.Current.SubItemID.GetValueOrDefault() == subItemID))
            {
                e.NewValue = null;
                e.Cancel = true;
                throw new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error);
            }

            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, amBomMatl.InventoryID);
            if (item == null)
            {
                return;
            }
            CheckDuplicateEntry(e, amBomMatl, item, subItemID);
        }

        protected virtual void AMBomMatl_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var amBomMatl = (AMBomMatl)e.Row;
            if (amBomMatl == null || Documents.Current == null
                || e.NewValue == null || InventoryHelper.SubItemFeatureEnabled)
            {
                return;
            }

            int? inventoryID = Convert.ToInt32(e.NewValue);
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);

            if (item == null)
            {
                return;
            }

            //  PREVENT A USER FROM ADDING THE MATERIAL ITEM TO ITSELF
            //      More in depth prevention can be added down the road
            if (inventoryID == Documents.Current.InventoryID)
            {
                e.NewValue = item.InventoryCD;
                e.Cancel = true;
                throw new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error);
            }
        }

        protected virtual void AMBomMatl_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amBomMatl = (AMBomMatl)e.Row;
            if (amBomMatl == null)
            {
                return;
            }

            if (Documents.Current != null && amBomMatl.InventoryID.GetValueOrDefault() != 0)
            {
                cache.SetDefaultExt<AMBomMatl.descr>(e.Row);
                cache.SetDefaultExt<AMBomMatl.subItemID>(e.Row);
                cache.SetDefaultExt<AMBomMatl.uOM>(e.Row);
                cache.SetDefaultExt<AMBomMatl.unitCost>(e.Row);
            }
        }

        protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            object MatlUnitCost;
            sender.RaiseFieldDefaulting<AMBomMatl.unitCost>(e.Row, out MatlUnitCost);

            if (MatlUnitCost != null && (decimal)MatlUnitCost != 0m)
            {
                decimal? matlUnitCost = INUnitAttribute.ConvertToBase<AMBomMatl.inventoryID>(sender, e.Row, ((AMBomMatl)e.Row).UOM, (decimal)MatlUnitCost, INPrecision.UNITCOST);
                sender.SetValueExt<AMBomMatl.unitCost>(e.Row, matlUnitCost);
            }

        }

        protected virtual void AMBomMatl_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitCost(sender, e);
        }

        protected virtual void AMBomAttribute_RevisionID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMECRItem.ECRRev;
        }

        protected virtual void AMBomAttribute_AttributeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMBomAttribute)e.Row;
            if (row == null)
            {
                return;
            }

            var item = (CSAttribute)PXSelectorAttribute.Select<AMBomAttribute.attributeID>(sender, row);
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                sender.SetValueExt<AMBomAttribute.label>(row, item.AttributeID);
            }
            if (string.IsNullOrWhiteSpace(row.Descr))
            {
                sender.SetValueExt<AMBomAttribute.descr>(row, item.Description);
            }
        }

        /// <summary>
        /// Checks for duplicate item in a BOM
        /// </summary>
        /// <param name="e">Calling Field Verifying event args</param>
        /// <param name="matlRow">source material row to check against</param>
        /// <param name="inventoryItem">Inventory item row of newly entered inventory ID (from field verifying)</param>
        /// <returns>True if the row can be added, false otherwise</returns>
        protected virtual void CheckDuplicateEntry(PXFieldVerifyingEventArgs e, AMBomMatl matlRow, InventoryItem inventoryItem)
        {
            CheckDuplicateEntry(e, matlRow, inventoryItem, null);
        }

        /// <summary>
        /// Checks for duplicate item in a BOM
        /// </summary>
        /// <param name="e">Calling Field Verifying event args</param>
        /// <param name="matlRow">source material row to check against</param>
        /// <param name="inventoryItem">Inventory item row of newly entered inventory ID (from field verifying)</param>
        /// <param name="subItemID">SUbItemID</param>
        /// <returns>True if the row can be added, false otherwise</returns>
        protected virtual void CheckDuplicateEntry(PXFieldVerifyingEventArgs e, AMBomMatl matlRow, InventoryItem inventoryItem, int? subItemID)
        {
            AMDebug.TraceWriteMethodName();

            if (matlRow == null || this.ambsetup.Current == null || inventoryItem == null)
            {
                return;
            }

            AMBSetup bomSetup = this.ambsetup.Current;

            //If pages running as import treat warnings the same as allow
            if (IsImport && bomSetup.DupInvBOM.Trim() == SetupMessage.WarningMsg)
            {
                bomSetup.DupInvBOM = SetupMessage.AllowMsg;
            }
            if (IsImport && bomSetup.DupInvOper.Trim() == SetupMessage.WarningMsg)
            {
                bomSetup.DupInvOper = SetupMessage.AllowMsg;
            }

            if (bomSetup.DupInvBOM.Trim() == SetupMessage.AllowMsg
                && bomSetup.DupInvOper.Trim() == SetupMessage.AllowMsg)
            {
                // both allow = nothing to validate
                return;
            }

            AMBomMatl dupBomMatl = null;
            AMBomMatl dupOperMatl = null;

            foreach (AMBomMatl duplicateAMBomMatl in PXSelect<AMBomMatl,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>,
                    And<AMBomMatl.inventoryID, Equal<Required<AMBomMatl.inventoryID>>
                    >>>>.Select(this, matlRow.BOMID, matlRow.RevisionID, inventoryItem.InventoryID))
            {
                if (subItemID != null && duplicateAMBomMatl.SubItemID.GetValueOrDefault() != subItemID.GetValueOrDefault() && InventoryHelper.SubItemFeatureEnabled)
                {
                    continue;
                }
                if (duplicateAMBomMatl.OperationID.Equals(matlRow.OperationID) && duplicateAMBomMatl.LineID != matlRow.LineID && dupOperMatl == null)
                {
                    dupOperMatl = duplicateAMBomMatl;
                }

                if (!duplicateAMBomMatl.OperationID.Equals(matlRow.OperationID) && dupBomMatl == null)
                {
                    dupBomMatl = duplicateAMBomMatl;
                }

                if (dupOperMatl != null && dupBomMatl != null)
                {
                    break;
                }
            }

            var skipBomCheck = false;
            if (dupOperMatl != null && bomSetup.DupInvOper.Trim() != SetupMessage.AllowMsg)
            {
                DuplicateEntryMessage(e, dupOperMatl, inventoryItem, bomSetup.DupInvOper.Trim());
                skipBomCheck = true;
            }

            if (dupBomMatl != null && !skipBomCheck && bomSetup.DupInvBOM.Trim() != SetupMessage.AllowMsg)
            {
                DuplicateEntryMessage(e, dupBomMatl, inventoryItem, bomSetup.DupInvBOM.Trim());
            }
        }

        /// <summary>
        /// Builds and creates the warning/error message related to duplicates items on a BOM
        /// </summary>
        /// <param name="e">Calling Field Verifying event args</param>
        /// <param name="duplicateAMBomMatl">The found duplicate AMBomMatl row</param>
        /// <param name="inventoryItem">Inventory item row of newly entered inventory ID (from field verifying)</param>
        /// <param name="setupCheck">BOM Setup duplicate setup option indicating warning or error</param>
        protected virtual void DuplicateEntryMessage(PXFieldVerifyingEventArgs e, AMBomMatl duplicateAMBomMatl, InventoryItem inventoryItem, string setupCheck)
        {
            if (duplicateAMBomMatl == null ||
                duplicateAMBomMatl.InventoryID == null ||
                inventoryItem == null ||
                string.IsNullOrWhiteSpace(setupCheck))
            {
                return;
            }

            string userMessage = Messages.GetLocal(Messages.BomMatlDupItems, duplicateAMBomMatl.OperationID, duplicateAMBomMatl.BOMID.Trim());

            switch (setupCheck)
            {
                case SetupMessage.WarningMsg:
                    WebDialogResult response = BomMatlRecords.Ask(
                        Messages.Warning,
                        $"{userMessage} {Messages.GetLocal(Messages.Continue)}?",
                        MessageButtons.YesNo);

                    if (response != WebDialogResult.Yes)
                    {
                        e.NewValue = inventoryItem.InventoryCD;
                        e.Cancel = true;
                        throw new PXSetPropertyException(userMessage, PXErrorLevel.Error);
                    }
                    break;
                case SetupMessage.ErrorMsg:
                    e.NewValue = inventoryItem.InventoryCD;
                    e.Cancel = true;
                    throw new PXSetPropertyException(userMessage, PXErrorLevel.Error);
            }
        }

        #endregion

        protected virtual void AMBomTool_ToolID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMBomTool)e.Row;
            if (row == null)
            {
                return;
            }

            var toolMst = (AMToolMst)PXSelectorAttribute.Select<AMBomTool.toolID>(cache, row);

            row.Descr = toolMst?.Descr;
            row.UnitCost = toolMst?.UnitCost ?? 0m;
        }


        #region Button - Copy Bom
        protected virtual void CopyBomToECR(PXCache ecrItemCache, AMECRItem ecr, AMBomItem sourceBOM)
        {
            if (sourceBOM?.BOMID == null || ecr == null)
            {
                return;
            }

            //Should prompt before deleting, but for now...
            DeleteAllCurrentECRDetail(ecr);

            ecrItemCache.SetValueExt<AMECRItem.revisionID>(ecr, AutoNumberHelper.NextNumber(GetMaxRevForBOM(sourceBOM.BOMID)));
            ecrItemCache.SetValueExt<AMECRItem.inventoryID>(ecr, sourceBOM.InventoryID);
            if (sourceBOM.SubItemID != null)
            {
                ecrItemCache.SetValueExt<AMECRItem.subItemID>(ecr, sourceBOM.SubItemID);
            }
            ecrItemCache.SetValueExt<AMECRItem.siteID>(ecr, sourceBOM.SiteID);
            ecrItemCache.SetValueExt<AMECRItem.descr>(ecr, sourceBOM.Descr);
            ecrItemCache.SetValueExt<AMECRItem.bOMID>(ecr, sourceBOM.BOMID);
            ecrItemCache.SetValueExt<AMECRItem.bOMRevisionID>(ecr, sourceBOM.RevisionID);
            ecrItemCache.SetValueExt<AMECRItem.lineCntrAttribute>(ecr, sourceBOM.LineCntrAttribute);

            PXNoteAttribute.CopyNoteAndFiles(this.Caches<AMBomItem>(), sourceBOM, ecrItemCache, ecr);

            CopyBomOper(sourceBOM, ecr.ECRID, AMECRItem.ECRRev, true);
            CopyBomMatl(sourceBOM, ecr.ECRID, AMECRItem.ECRRev, true);
            CopyBomStep(sourceBOM, ecr.ECRID, AMECRItem.ECRRev, true);
            CopyBomRef(sourceBOM, ecr.ECRID, AMECRItem.ECRRev);
            CopyBomTool(sourceBOM, ecr.ECRID, AMECRItem.ECRRev, true);
            CopyBomOvhd(sourceBOM, ecr.ECRID, AMECRItem.ECRRev, true);
            CopyBomAttributes(sourceBOM, ecr.ECRID, AMECRItem.ECRRev);
        }

        protected virtual void DeleteAllCurrentECRDetail(AMECRItem ecrItem)
        {
            foreach (AMBomAttribute bomAttribute in PXSelect<AMBomAttribute,
                Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                    And<AMBomAttribute.revisionID, Equal<AMECRItem.eCRRev>>>
            >.Select(this, ecrItem.ECRID))
            {
                BomAttributes.Delete(bomAttribute);
            }

            foreach (AMBomOper bomOper in PXSelect<AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<AMECRItem.eCRRev>>>
            >.Select(this, ecrItem.ECRID))
            {
                BomOperRecords.Delete(bomOper);
            }
        }

        protected virtual void CopyBomOper(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelect<AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>
                    >.Select(this, sourceBOM.BOMID, sourceBOM.RevisionID);

            foreach (AMBomOper fromRow in fromRows)
            {
                var toRow = PXCache<AMBomOper>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomOperRecords.Insert(toRow);
                toRow.RowStatus = AMRowStatus.Unchanged;
                toRow = BomOperRecords.Update(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomOperRecords.Cache, fromRow, BomOperRecords.Cache, toRow);
                    BomOperRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomMatl(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            foreach (PXResult<AMBomMatl, InventoryItem, AMBomItem, INItemSite> result in PXSelectJoin<
                AMBomMatl,
                InnerJoin<InventoryItem,
                    On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<AMBomItem,
                    On<AMBomMatl.bOMID, Equal<AMBomItem.bOMID>,
                    And<AMBomMatl.revisionID, Equal<AMBomItem.revisionID>>>,
                LeftJoin<INItemSite,
                    On<AMBomMatl.inventoryID, Equal<INItemSite.inventoryID>,
                    And<AMBomItem.siteID, Equal<INItemSite.siteID>>>>>>,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>,
                    And<Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
                        And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>>,
                OrderBy<
                    Asc<AMBomMatl.sortOrder,
                    Asc<AMBomMatl.lineID>>>
                >
                .Select(this, sourceBOM.BOMID, sourceBOM.RevisionID))
            {
                var fromRow = (AMBomMatl)result;
                var inventoryItem = (InventoryItem)result;

                if (fromRow == null || inventoryItem == null ||
                    fromRow.ExpDate.GetValueOrDefault(Common.Dates.BeginOfTimeDate) != Common.Dates.BeginOfTimeDate
                    && fromRow.ExpDate.GetValueOrDefault() < Accessinfo.BusinessDate.GetValueOrDefault())
                {
                    //no point in copying expired material
                    continue;
                }

                var toRow = PXCache<AMBomMatl>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;

                if (toRow.CompBOMID != null && !IsValidBom(toRow.CompBOMID, toRow.CompBOMRevisionID))
                {
                    toRow.CompBOMID = null;
                    toRow.CompBOMRevisionID = null;
                }

                try
                {
                    toRow = BomMatlRecords.Insert(toRow);
                    toRow.RowStatus = AMRowStatus.Unchanged;
                    toRow = BomMatlRecords.Update(toRow);

                    // The result uses the bom siteid, so if material has a site id we still want to call DefaultItemSite
                    var materialItemSite = (INItemSite)result;
                    if (toRow.SiteID != null || materialItemSite == null)
                    {
                        DefaultItemSite(toRow.InventoryID, toRow.SiteID ?? Documents.Current.SiteID);
                    }

                    if (copyNotes)
                    {
                        PXNoteAttribute.CopyNoteAndFiles(BomMatlRecords.Cache, fromRow, BomMatlRecords.Cache, toRow);
                        BomMatlRecords.Update(toRow);
                    }
                }
                catch (Exception exception)
                {
                    PXTrace.WriteError(
                            Messages.GetLocal(Messages.UnableToCopyMaterialFromToBomID),
                            inventoryItem?.InventoryCD.TrimIfNotNullEmpty(),
                            fromRow?.BOMID,
                            fromRow?.RevisionID,
                            toRow?.BOMID,
                            toRow?.RevisionID,
                            exception.Message);
                    throw;
                }
            }
        }
        protected virtual void CopyBomStep(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelect<AMBomStep,
                Where<AMBomStep.bOMID, Equal<Required<AMBomStep.bOMID>>,
                    And<AMBomStep.revisionID, Equal<Required<AMBomStep.revisionID>>
                    >>>.Select(this, sourceBOM.BOMID, sourceBOM.RevisionID);

            foreach (AMBomStep fromRow in fromRows)
            {
                var toRow = PXCache<AMBomStep>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomStepRecords.Insert(toRow);
                toRow.RowStatus = AMRowStatus.Unchanged;
                toRow = BomStepRecords.Update(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomStepRecords.Cache, fromRow, BomStepRecords.Cache, toRow);
                    BomStepRecords.Update(toRow);
                }
            }
        }
        protected virtual void CopyBomRef(AMBomItem sourceBOM, string newBOMID, string newRevisionID)
        {
            var fromRows = PXSelect<AMBomRef,
                Where<AMBomRef.bOMID, Equal<Required<AMBomRef.bOMID>>,
                    And<AMBomRef.revisionID, Equal<Required<AMBomRef.revisionID>>
                    >>>.Select(this, sourceBOM.BOMID, sourceBOM.RevisionID);

            foreach (AMBomRef fromRow in fromRows)
            {
                var toRow = PXCache<AMBomRef>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomRefRecords.Insert(toRow);
                toRow.RowStatus = AMRowStatus.Unchanged;
                BomRefRecords.Update(toRow);
            }
        }

        protected virtual void CopyBomTool(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelectJoin<AMBomTool,
                InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
                Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
                    And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>
                    >>>.Select(this, sourceBOM.BOMID, sourceBOM.RevisionID);

            foreach (AMBomTool fromRow in fromRows)
            {
                var toRow = PXCache<AMBomTool>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomToolRecords.Insert(toRow);
                toRow.RowStatus = AMRowStatus.Unchanged;
                toRow = BomToolRecords.Update(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomToolRecords.Cache, fromRow, BomToolRecords.Cache, toRow);
                    BomToolRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomOvhd(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelectJoin<AMBomOvhd,
                InnerJoin<AMOverhead, On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMBomOvhd.bOMID, Equal<Required<AMBomOvhd.bOMID>>,
                    And<AMBomOvhd.revisionID, Equal<Required<AMBomOvhd.revisionID>>
                    >>>.Select(this, sourceBOM.BOMID, sourceBOM.RevisionID);

            foreach (AMBomOvhd fromRow in fromRows)
            {
                var toRow = PXCache<AMBomOvhd>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomOvhdRecords.Insert(toRow);
                toRow.RowStatus = AMRowStatus.Unchanged;
                toRow = BomOvhdRecords.Update(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomOvhdRecords.Cache, fromRow, BomOvhdRecords.Cache, toRow);
                    BomOvhdRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomAttributes(AMBomItem sourceBOM, string newBOMID, string newRevisionID)
        {
            FieldVerifying.AddHandler<AMBomAttribute.operationID>((sender, e) => { e.Cancel = true; });

            foreach (PXResult<AMBomAttribute, AMBomOper> result in PXSelectJoin<AMBomAttribute,
                    LeftJoin<AMBomOper, On<AMBomAttribute.bOMID, Equal<AMBomOper.bOMID>,
                            And<AMBomAttribute.revisionID, Equal<AMBomOper.revisionID>,
                        And<AMBomAttribute.operationID, Equal<AMBomOper.operationID>>>>>,
                Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                    And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>>>>
                .Select(this, sourceBOM.BOMID, sourceBOM.RevisionID))
            {
                var fromBomAttribute = (AMBomAttribute)result;
                var fromBomAttOper = (AMBomOper)result;

                int? newOperationId = null;
                if (fromBomAttOper?.OperationCD != null)
                {
                    var newOperation = FindInsertedBomOperByCd(fromBomAttOper.OperationCD);
                    if (newOperation?.OperationID == null)
                    {
                        continue;
                    }

                    newOperationId = newOperation.OperationID;
                }

                var newBomAtt = (AMBomAttribute)BomAttributes.Cache.CreateCopy(fromBomAttribute);
                newBomAtt.BOMID = newBOMID;
                newBomAtt.RevisionID = newRevisionID;
                newBomAtt.OperationID = newOperationId;

                var insertedAttribute = BomAttributes.Insert(newBomAtt);
                if (insertedAttribute != null)
                {
                    insertedAttribute.RowStatus = AMRowStatus.Unchanged;
                    BomAttributes.Update(insertedAttribute);
                    continue;
                }

                PXTrace.WriteWarning($"Unable to copy {Common.Cache.GetCacheName(typeof(AMBomAttribute))} from ({fromBomAttribute.BOMID};{fromBomAttribute.RevisionID};{fromBomAttribute.LineNbr})");
#if DEBUG
                AMDebug.TraceWriteMethodName($"Unable to copy {Common.Cache.GetCacheName(typeof(AMBomAttribute))} from ({fromBomAttribute.BOMID};{fromBomAttribute.RevisionID};{fromBomAttribute.LineNbr})");
#endif
            }
        }

        private AMBomOper FindInsertedBomOperByCd(string operationCd)
        {
            //Not including bom/rev as inserts should only be checked during copy process
            return BomOperRecords.Cache.Inserted.ToArray<AMBomOper>().FirstOrDefault(x => x.OperationCD == operationCd);
        }

        protected bool IsValidBom(string bomId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(revisionId))
            {
                return (AMBomItem)PXSelect<
                    AMBomItem,
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.status, NotEqual<AMBomStatus.archived>>>>
                    .SelectWindowed(this, 0, 1, bomId) != null;
            }

            return (AMBomItem)PXSelect<AMBomItem,
                    Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                        And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>,
                        And<AMBomItem.status, NotEqual<AMBomStatus.archived>>>>>
                .SelectWindowed(this, 0, 1, bomId, revisionId) != null;
        }
        #endregion

        protected virtual void EnableECRItemFields(PXCache cache, AMECRItem item)
        {
            if (item == null || item.Hold.GetValueOrDefault())
            {
                return;
            }

            bool isCompleted = (item.Status == AMECRStatus.Completed);

            PXUIFieldAttribute.SetEnabled(cache, item, false);
            PXUIFieldAttribute.SetEnabled<AMECRItem.eCRID>(cache, item, true);
            PXUIFieldAttribute.SetEnabled<AMECRItem.hold>(cache, item, !isCompleted);
        }

        /// <summary>
        /// Create an INItemSite record if one doesn't exist for the bom item/site
        /// </summary>
        /// <param name="amECRItem">BOM containing item/site ids</param>
        protected virtual void DefaultItemSiteByBOM(AMECRItem amECRItem)
        {
            if (amECRItem?.InventoryID == null
                || amECRItem.SiteID == null)
            {
                return;
            }

            DefaultItemSite(amECRItem.InventoryID, amECRItem.SiteID);
        }

        /// <summary>
        /// Create an INItemSite record if one doesn't exist for the bom item/site
        /// </summary>
        protected virtual void DefaultItemSite(int? inventoryID, int? siteID)
        {
            if (inventoryID == null || siteID == null || !InventoryHelper.MultiWarehousesFeatureEnabled)
            {
                return;
            }

            INItemSite inItemSite = null;
            if (InventoryHelper.MakeItemSiteByItem(this, inventoryID, siteID, out inItemSite))
            {
                INItemSite itemSite = ItemSiteRecord.Locate(inItemSite);
                if (itemSite == null)
                {
                    ItemSiteRecord.Insert(inItemSite);
                }
            }
        }

        /// <summary>
        /// Insert INItemSite records based on inserted bom item or matl records
        /// </summary>
        protected virtual void InsertMissingINItemSite()
        {
            foreach (AMECRItem amECRItem in this.Documents.Cache.Inserted)
            {
                DefaultItemSiteByBOM(amECRItem);
            }

            foreach (AMBomMatl amBomMatl in this.BomMatlRecords.Cache.Inserted)
            {
                var matlSiteID = amBomMatl.SiteID;
                if (matlSiteID == null)
                {
                    foreach (AMBomItem amBomItem in this.Documents.Cache.Cached.Cast<AMBomItem>().Where(amBomItem => amBomItem.BOMID == amBomMatl.BOMID && amBomItem.RevisionID == amBomMatl.RevisionID))
                    {
                        matlSiteID = amBomItem.SiteID;
                    }
                }

                DefaultItemSite(amBomMatl.InventoryID, matlSiteID);
            }
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

        internal string GetMaxRevForBOM(string bomid)
        {
            List<string> list = new List<string>();

            AMBomItem item = PXSelectGroupBy<AMBomItem, Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>>,
                Aggregate<Max<AMBomItem.revisionID>>>.Select(this, bomid);
            list.Add(item != null ? item.RevisionID : "");

            AMECRItem ecr = PXSelectGroupBy<AMECRItem, Where<AMECRItem.bOMID, Equal<Required<AMECRItem.bOMID>>>,
                Aggregate<Max<AMECRItem.revisionID>>>.Select(this, bomid);
            list.Add(ecr != null ? ecr.RevisionID : "");

            AMECOItem eco = PXSelectGroupBy<AMECOItem, Where<AMECOItem.bOMID, Equal<Required<AMECOItem.bOMID>>>,
            Aggregate<Max<AMECOItem.revisionID>>>.Select(this, bomid);
            list.Add(eco != null ? eco.RevisionID : "");

            return list.Max();
        }

        public static string BOMRevItemDisplay(string bomid, string rev, string invtid)
        {
            var display = "";
            if (!string.IsNullOrEmpty(bomid))
            {
                display += bomid.Trim();
                if (!string.IsNullOrEmpty(rev))
                {
                    display += " - " + rev.Trim();
                }
                if (!string.IsNullOrEmpty(invtid))
                {
                    display += ", " + invtid.Trim();
                }
            }
            return display;
        }
    }
}