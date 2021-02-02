using System.Collections;
using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Bill of Material Maintenance graph
    /// Main graph for managing a Bill of Material (BOM)
    /// </summary>
    public class BOMMaint : PXRevisionableGraph<BOMMaint, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID>
    {
        //Primary view "Documents" comes from PXRevisionablegraph

        public BOMMaint()
        {
            var bomSetup = ambsetup.Current;
            if (string.IsNullOrWhiteSpace(bomSetup?.BOMNumberingID))
            {
                throw new BOMSetupNotEnteredException();
            }

            var prodSetup = ProdSetup.Current;
            AMPSetup.CheckNeedsUpgrade(prodSetup);

            ActionsMenu.AddMenuAction(ArchiveBom);
            ActionsMenu.AddMenuAction(AMCopyBom);
            ActionsMenu.AddMenuAction(AMBomCostSettings);
            ActionsMenu.AddMenuAction(MakeDefaultBomAction);
            ActionsMenu.AddMenuAction(MakePlanningBomAction);
            ActionsMenu.AddMenuAction(Attributes);
            ActionsMenu.AddMenuAction(CreateECR);
            ActionsMenu.AddMenuAction(BOMCompare);
            ReportsMenu.AddMenuAction(ReportBOMSummary);
            ReportsMenu.AddMenuAction(ReportMultiLevel);
        }

        #region CACHE ATTACHED

        [BomID(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [Rev.Key(typeof(AMBSetup.bOMNumberingID),
                    typeof(AMBomItem.bOMID),
                    typeof(AMBomItem.revisionID),
                    typeof(AMBomItem.bOMID),
                    typeof(AMBomItem.revisionID),
                    typeof(AMBomItem.inventoryID),
                    typeof(AMBomItem.subItemID),
                    typeof(AMBomItem.siteID))]
        protected virtual void AMBomItem_BOMID_CacheAttached(PXCache sender)
        {
            // Do Not Remove this
            // used to allow BOM Maint to use the Revisionable Graph for Copy and navigation while Reports and other screens use standard selectors
        }

        [RevisionIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [PXDefault(typeof(AMBSetup.defaultRevisionID), PersistingCheck = PXPersistingCheck.Nothing)]
        [Rev.ID(typeof(AMBSetup.defaultRevisionID),
            typeof(AMBomItem.bOMID),
            typeof(AMBomItem.revisionID),
            typeof(AMBomItem.revisionID),
            typeof(AMBomItem.status),
            typeof(AMBomItem.descr),
            typeof(AMBomItem.effStartDate),
            typeof(AMBomItem.effEndDate))]
        protected virtual void AMBomItem_RevisionID_CacheAttached(PXCache sender)
        {
            // Do Not Remove this
            // used to allow BOM Maint to use the Revisionable Graph for Copy and navigation while Reports and other screens use standard selectors
        }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Lot Size")]
        protected virtual void RollupSettings_LotSize_CacheAttached(PXCache sender)
        {
            //Removing default
        }

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
        [PXLineNbr(typeof(AMBomItem.lineCntrOperation))]
        protected virtual void AMBomOper_OperationID_CacheAttached(PXCache sender)
        {
#if DEBUG
            //Cache attached to change display name so we can provide the user with a way to see the DB ID if needed 
#endif
        }

        #endregion

        // Users should use the Copy BOM action
        public override bool CanClipboardCopyPaste()
        {
            return false;
        }

        /// <summary>
        /// Redirect to this graph for the given BOM
        /// </summary>
        public static void Redirect(string bomId)
        {
            Redirect(bomId, null);
        }

        /// <summary>
        /// Redirect to this graph for the given BOM / effective date
        /// </summary>
        public static void Redirect(string bomId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return;
            }

            var graph = CreateInstance<BOMMaint>();

            var bomItem = revisionId == null
                ? PrimaryBomIDManager.GetNotArchivedRevisionBomItem(graph, bomId)
                : (AMBomItem) PXSelect<
                        AMBomItem,
                        Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                            And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                    .SelectWindowed(graph, 0, 1, bomId, revisionId);

            if (bomItem == null)
            {
                PXTrace.WriteInformation($"No BOM record found for BOM ID {bomId}");
                return;
            }

            graph.Documents.Current = bomItem;
            if (graph.Documents.Current != null)
            {
                throw new PXRedirectRequiredException(graph, true, Messages.BOMMaint);
            }
        }

        public PXFilter<CopyBomFilter> copyBomFilter;
        public PXFilter<RollupSettings> rollsettings;

        #region Views - Data Selections
        
        [PXImport(typeof(AMBomItem))]
        public PXSelect<AMBomOper,
            Where<AMBomOper.bOMID, Equal<Current<AMBomItem.bOMID>>,
                And<AMBomOper.revisionID, Equal<Current<AMBomItem.revisionID>>>>,
            OrderBy<Asc<AMBomOper.operationCD>>> BomOperRecords;

        [PXImport(typeof(AMBomItem))]
        [PXCopyPasteHiddenFields]
        public AMOrderedMatlSelect<AMBomItem, AMBomMatl,
            Where<AMBomMatl.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomMatl.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomMatl.operationID, Equal<Current<AMBomOper.operationID>>>>>,
            OrderBy<Asc<AMBomMatl.sortOrder, Asc<AMBomMatl.lineID>>>> BomMatlRecords;

        [PXImport(typeof(AMBomItem))]
        public PXSelect<AMBomStep,
            Where<AMBomStep.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomStep.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomStep.operationID, Equal<Current<AMBomOper.operationID>>>>>,
            OrderBy<Asc<AMBomStep.sortOrder, Asc<AMBomStep.lineID>>>> BomStepRecords;

        [PXImport(typeof(AMBomItem))]
        public PXSelectJoin<AMBomTool,
            InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
            Where<AMBomTool.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomTool.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomTool.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomToolRecords;

        [PXImport(typeof(AMBomItem))]
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

        [PXHidden]
        public PXSelect<AMBomCost, Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>,
            And<AMBomCost.bOMID, Equal<Current<AMBomItem.bOMID>>>>> BomCostRecs;
        [PXHidden]
        public PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Current<AMBomItem.inventoryID>>,
            And<INItemSite.siteID, Equal<Current<AMBomItem.siteID>>>>> ItemSiteRecord;
        [PXHidden]
        public PXFilter<DefaultBomLevels> DefaultBomLevelsFilter;
        [PXHidden]
        public PXFilter<DefaultBomLevels> PlanningBomLevelsFilter;
        [PXHidden]
        public PXSetup<Numbering,
            LeftJoin<AMBSetup, On<AMBSetup.bOMNumberingID, Equal<Numbering.numberingID>>>,
            Where<Numbering.numberingID, Equal<AMBSetup.bOMNumberingID>>> BomNumbering;

        [PXHidden]
        public PXSelect<
            AMBomAttribute, 
            Where<AMBomAttribute.bOMID, Equal<Current<AMBomItem.bOMID>>,
                And<AMBomAttribute.revisionID, Equal<Current<AMBomItem.revisionID>>>>> BomAttributes;

        [PXHidden]
        public PXSelect<AMECOItem> EcoItem;

        [PXHidden]
        public PXSelect<AMBomOper, 
            Where<AMBomOper.bOMID, Equal<Current<AMBomOper.bOMID>>, 
                And<AMBomOper.revisionID, Equal<Current<AMBomOper.revisionID>>, 
                And<AMBomOper.operationID, Equal<Current<AMBomOper.operationID>>>>>> OutsideProcessingOperationSelected;

        #endregion

        public PXAction<AMBomItem> ActionsMenu;
        [PXButton(CommitChanges = true, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Actions")]
        protected virtual void actionsMenu()
        {

        }

        public PXAction<AMBomItem> ReportsMenu;
        [PXButton(CommitChanges = true, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports")]
        protected virtual void reportsMenu()
        {

        }

        public PXAction<AMBomItem> ViewCompBomID;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable viewCompBomID(PXAdapter adapter)
        {
            BOMMaint.Redirect(BomMatlRecords?.Current?.CompBOMID, BomMatlRecords?.Current?.CompBOMRevisionID);
            return adapter.Get();
        }

        #region BOM Item Processes

        protected virtual bool HasAnotherRevision(AMBomItem bomItem)
        {
            var otherBomRev = (AMBomItem)PXSelect<
                AMBomItem, 
                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                    And<AMBomItem.revisionID, NotEqual<Required<AMBomItem.revisionID>>>>>
                .Select(this, bomItem?.BOMID, bomItem?.RevisionID);
            return otherBomRev != null;
        }

        protected virtual void AMBomItem_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            //Other areas where both bomid and revision are required should have the PXForeignReference attribute attached.
            //  The problem here - we want to first see if this is the only BOM rev for this BOMID and if so check to see if its on records where bom rev is optional

            var row = (AMBomItem) e.Row;
            if (row?.BOMID == null || HasAnotherRevision(row))
            {
                return;
            }

            var bomMatl = (AMBomMatl) PXSelect<
                AMBomMatl,
                Where<AMBomMatl.compBOMID, Equal<Required<AMBomMatl.compBOMID>>>>
                .SelectWindowed(this, 0, 1, row.BOMID);

            if (bomMatl != null)
            {
                throw new PXException($"{Common.Cache.GetCacheName(typeof(AMBomItem))} cannot be deleted. BOM {row.BOMID} is referenced in {PXUIFieldAttribute.GetDisplayName<AMBomMatl.compBOMID>(BomMatlRecords.Cache)} on {Common.Cache.GetCacheName(typeof(AMBomMatl))} ({bomMatl.BOMID}, {bomMatl.RevisionID}, {bomMatl.OperationID}, {bomMatl.LineID})");
            }

            var prodMatl = (AMProdMatl)PXSelect<
                    AMProdMatl,
                    Where<AMProdMatl.compBOMID, Equal<Required<AMProdMatl.compBOMID>>>>
                .SelectWindowed(this, 0, 1, row.BOMID);

            if (prodMatl != null)
            {
                throw new PXException($"{Common.Cache.GetCacheName(typeof(AMBomItem))} cannot be deleted. BOM {row.BOMID} is referenced in {PXUIFieldAttribute.GetDisplayName<AMProdMatl.compBOMID>(BomMatlRecords.Cache)} on {Common.Cache.GetCacheName(typeof(AMProdMatl))} ({prodMatl.OrderType}, {prodMatl.ProdOrdID}, {prodMatl.OperationID}, {prodMatl.LineID})");
            }
        }

        protected virtual void AMBomItem_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            var row = (AMBomItem)e.Row;
            if (row?.BOMID == null || HasAnotherRevision(row))
            {
                return;
            }

            try
            {
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
                RemovePrimeBomID(row);
#pragma warning restore PX1043 // Changes cannot be saved to the database from the event handler
            }
            catch (Exception exception)
            {
                PXTrace.WriteError(exception);
            }
        }

        protected virtual void AMBomItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var item = (AMBomItem)e.Row;
            if (item == null)
            {
                return;
            }

            var eccAllowEdit = ECRAllowEdit(item.BOMID);
            EnableBomItemFields(sender, item);
            var hasRequiredFields = item.InventoryID != null && item.SiteID != null && !IsImport && !IsContractBasedAPI;
            EnableOperCache(item.Hold.GetValueOrDefault() && eccAllowEdit && hasRequiredFields);
            EnableOperChildCache(item.Hold.GetValueOrDefault() && eccAllowEdit && hasRequiredFields);

            EnableButtons(!sender.IsRowInserted(item));

            MakeDefaultBomAction.SetEnabled(item.Status.GetValueOrDefault() == AMBomStatus.Active);
            MakePlanningBomAction.SetEnabled(item.Status.GetValueOrDefault() == AMBomStatus.Active);
        }

        protected virtual void AMBomItem_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            var row = (AMBomItem)e.Row;
            if (row == null)
            {
                return;
            }

            if (!Common.Dates.StartBeforeEnd(row.EffStartDate, row.EffEndDate))
            {
                cache.RaiseExceptionHandling<AMBomItem.effEndDate>(row, row.EffStartDate,
                    new PXSetPropertyException(AM.Messages.MustBeGreaterThanOrEqualTo,
                        PXUIFieldAttribute.GetDisplayName<AMBomItem.effEndDate>(Documents.Cache),
                        PXUIFieldAttribute.GetDisplayName<AMBomItem.effStartDate>(Documents.Cache)));
            }
        }

        protected virtual void EnableButtons(bool enable)
        {
            ArchiveBom.SetEnabled(enable);
            AMCopyBom.SetEnabled(enable);
            AMBomCostSettings.SetEnabled(enable);
            ReportBOMSummary.SetEnabled(enable);
            ReportMultiLevel.SetEnabled(enable);
            Attributes.SetEnabled(enable);
            CreateECR.SetEnabled(enable);
            BOMCompare.SetEnabled(enable);
        }

        protected virtual void EnableOperCache(bool enabled)
        {
            BomOperRecords.AllowInsert = enabled;
            BomOperRecords.AllowUpdate = enabled;
            BomOperRecords.AllowDelete = enabled;

            // OutsideProcessing
            OutsideProcessingOperationSelected.AllowInsert = enabled;
            OutsideProcessingOperationSelected.AllowUpdate = enabled;
            OutsideProcessingOperationSelected.AllowDelete = enabled;
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

        protected virtual void AMBomItem_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMBomItem)e.Row;
            if (row == null || Documents.Current == null)
            {
                return;
            }
            
            row.Status = row.Hold.GetValueOrDefault() ? AMBomStatus.Hold : AMBomStatus.Active;
        }

        #endregion

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

            // Set Outside Processing from Work Center
            cache.SetValueExt<AMBomOper.outsideProcess>(bomOper, amWC.OutsideFlg.GetValueOrDefault());
        }

        protected virtual void _(Events.RowSelected<AMBomOper> e)
        {
            if (IsImport || IsContractBasedAPI)
            {
                return;
            }

            var isOutsideProcess = e?.Row?.OutsideProcess == true;
            PXUIFieldAttribute.SetEnabled<AMBomOper.dropShippedToVendor>(e.Cache, e.Row, isOutsideProcess && BomOperRecords.AllowUpdate);
            PXUIFieldAttribute.SetEnabled<AMBomOper.vendorID>(e.Cache, e.Row, isOutsideProcess && BomOperRecords.AllowUpdate);
            PXUIFieldAttribute.SetEnabled<AMBomOper.vendorLocationID>(e.Cache, e.Row, isOutsideProcess && BomOperRecords.AllowUpdate);
        }

        protected virtual void AMBomOper_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            var row = (AMBomOper)e.Row;
            if (row == null || Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            var configResults = GetActiveConfiguraitonForOperation(row);
            if (configResults != null)
            {
                var configOption = (AMConfigurationOption)configResults;
                var configFeature = (AMConfigurationFeature)configResults;
                if (configOption != null
                    && configFeature != null
                    && !string.IsNullOrWhiteSpace(configFeature.ConfigurationID)
                    && !string.IsNullOrWhiteSpace(configFeature.Revision))
                {
                    e.Cancel = true;
                    throw new PXException(Messages.BomOperNbrOnConfiguration,
                        row.BOMID.TrimIfNotNullEmpty(),
                        row.OperationCD.TrimIfNotNullEmpty(),
                        configFeature.ConfigurationID.TrimIfNotNullEmpty(),
                        configFeature.Revision.TrimIfNotNullEmpty(),
                        configFeature.Label.TrimIfNotNullEmpty(),
                        row.RevisionID.TrimIfNotNullEmpty());
                }
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

        protected virtual PXResult<AMConfigurationOption, AMConfigurationFeature, AMConfiguration> GetActiveConfiguraitonForOperation(AMBomOper amBomOper)
        {
            if (amBomOper?.OperationID == null)
            {
                return null;
            }

            return (PXResult<AMConfigurationOption, AMConfigurationFeature, AMConfiguration>)PXSelectJoin<AMConfigurationOption,
               InnerJoin<AMConfigurationFeature,
                   On<AMConfigurationOption.configurationID, Equal<AMConfigurationFeature.configurationID>,
                       And<AMConfigurationOption.revision, Equal<AMConfigurationFeature.revision>,
                       And<AMConfigurationOption.configFeatureLineNbr, Equal<AMConfigurationFeature.lineNbr>>>>,
               InnerJoin<AMConfiguration,
                   On<AMConfigurationOption.configurationID, Equal<AMConfiguration.configurationID>,
                       And<AMConfigurationOption.revision, Equal<AMConfiguration.revision>>>>>,
                   Where<AMConfiguration.status, NotEqual<ConfigRevisionStatus.inactive>,
                       And<AMConfiguration.bOMID, Equal<Required<AMConfiguration.bOMID>>,
                           And<AMConfiguration.revision, Equal<Required<AMConfiguration.revision>>,
                       And<AMConfigurationOption.operationID, Equal<Required<AMConfigurationOption.operationID>>>>>>
                       >.SelectWindowed(this, 0, 1, amBomOper.BOMID, amBomOper.RevisionID, amBomOper.OperationID);
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
            object matlUnitCost;
            sender.RaiseFieldDefaulting<AMBomMatl.unitCost>(e.Row, out matlUnitCost);

            if (matlUnitCost == null || (decimal)(matlUnitCost ?? 0m) == 0m)
            {
                return;
            }
                
            sender.SetValueExt<AMBomMatl.unitCost>(e.Row, matlUnitCost);
        }

        protected virtual void AMBomMatl_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitCost(sender, e);
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

            var userMessage = Messages.GetLocal(Messages.BomMatlDupItems, duplicateAMBomMatl.OperationID, duplicateAMBomMatl.BOMID.Trim());

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

        protected virtual void _(Events.FieldVerifying<AMBomMatl, AMBomMatl.subcontractSource> e)
        {
            var newContractSource = (int)e.NewValue;
            if (e.Row?.MaterialType == null)
            {
                return;
            }

            if (e.Row.MaterialType == AMMaterialType.Regular && newContractSource != AMSubcontractSource.None)
            {
                e.NewValue = AMSubcontractSource.None;
                e.Cancel = true;
                return;
            }

            if (e.Row.MaterialType == AMMaterialType.Subcontract && newContractSource == AMSubcontractSource.None)
            {
                e.NewValue = e.Row.SubcontractSource;
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.FieldUpdated<AMBomOper, AMBomOper.outsideProcess> e)
        {
            var oldValue = Convert.ToBoolean(e.OldValue ?? false);
            if (!oldValue || e.Row?.OutsideProcess == true)
            {
                return;
            }

            e.Cache.SetValueExt<AMBomOper.dropShippedToVendor>(e.Row, false);
            e.Cache.SetValueExt<AMBomOper.vendorID>(e.Row, null);
            e.Cache.SetValueExt<AMBomOper.vendorLocationID>(e.Row, null);

            foreach (AMBomMatl matl in BomMatlRecords.Select())
            {
                BomMatlRecords.Cache.SetValueExt<AMBomMatl.materialType>(matl, AMMaterialType.Regular);    
            }
        }

        protected virtual void _(Events.FieldUpdated<AMBomMatl, AMBomMatl.materialType> e)
        {
            if (e.Row?.MaterialType == null)
            {
                return;
            }

            if (e.Row.MaterialType == AMMaterialType.Regular)
            {
                e.Cache.SetValueExt<AMBomMatl.subcontractSource>(e.Row, AMSubcontractSource.None);
                return;
            }

            e.Cache.SetValueExt<AMBomMatl.subcontractSource>(e.Row, AMSubcontractSource.Purchase);
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

        #region Report Buttons

        public PXAction<AMBomItem> ReportBOMSummary;
        [PXUIField(DisplayName = Messages.BOMSummary, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable reportBOMSummary(PXAdapter adapter)
        {
            if (Documents.Current != null)
            {
                var parameters = Reports.BOMSummaryReportParams.FromBomId(Documents.Current.BOMID, Documents.Current.RevisionID);
                throw new PXReportRequiredException(parameters, Reports.BOMSummaryReportParams.ReportID, PXBaseRedirectException.WindowMode.New, Reports.BOMSummaryReportParams.ReportName);
            }
            return adapter.Get();
        }

        public PXAction<AMBomItem> ReportMultiLevel;
        [PXUIField(DisplayName = Messages.MultiLevel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable reportMultiLevel(PXAdapter adapter)
        {
            if (Documents.Current != null)
            {
                MultiLevelBomInq.RunReportNewWindow(Documents.Current.BOMID, Documents.Current.RevisionID);
            }
            return adapter.Get();
        }

        #endregion

        #region Button - Copy Bom

        public PXAction<AMBomItem> MakeDefaultBomAction;
        [PXUIField(DisplayName = Messages.MakeDefaultBom, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable makeDefaultBomAction(PXAdapter adapter)
        {
            if (Documents.Current == null)
            {
                return adapter.Get();
            }

            if (DefaultBomLevelsFilter.AskExt() == WebDialogResult.OK)
            {
                try
                {
                    var bomIDManager = new PrimaryBomIDManager(this);
                    bomIDManager.PersistChanges = false;
                    bomIDManager.SetPrimaryOverride(Documents.Current,
                        DefaultBomLevelsFilter.Current.Item.GetValueOrDefault(),
                        DefaultBomLevelsFilter.Current.Warehouse.GetValueOrDefault(),
                        DefaultBomLevelsFilter.Current.SubItem.GetValueOrDefault());

                    ItemSiteRecord.Select(); //required

                    Persist();
                }
                catch (Exception exception)
                {
                    PXTraceHelper.PxTraceException(exception);
                    throw new Exception(Messages.MakeDefaultBomFailed, exception);
                }
            }

            return adapter.Get();
        }

        public PXAction<AMBomItem> MakePlanningBomAction;
        [PXUIField(DisplayName = Messages.MakePlanningBom, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable makePlanningBomAction(PXAdapter adapter)
        {
            if (Documents.Current == null)
            {
                return adapter.Get();
            }

            if (PlanningBomLevelsFilter.AskExt() == WebDialogResult.OK)
            {
                try
                {
                    var bomIDManager = new PrimaryBomIDManager(this);
                    bomIDManager.PersistChanges = false;
                    bomIDManager.BOMIDType = PrimaryBomIDManager.BomIDType.Planning;
                    bomIDManager.SetPrimaryOverride(Documents.Current,
                    PlanningBomLevelsFilter.Current.Item.GetValueOrDefault(),
                    PlanningBomLevelsFilter.Current.Warehouse.GetValueOrDefault(),
                    PlanningBomLevelsFilter.Current.SubItem.GetValueOrDefault());

                    ItemSiteRecord.Select(); //required

                    Persist();
                }
                catch (Exception exception)
                {
                    PXTraceHelper.PxTraceException(exception);
                    throw new Exception(Messages.MakePrimaryBomFailed, exception);
                }
            }

            return adapter.Get();
        }

        /// <summary>
        /// BOM attributes redirect action
        /// </summary>
        public PXAction<AMBomItem> Attributes;
        /// <summary>
        /// BOM attributes redirect delegate
        /// </summary>
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel, Tooltip = "Launch BOM Attributes")]
        [PXUIField(DisplayName = Messages.Attributes, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        public virtual IEnumerable attributes(PXAdapter adapter)
        {
            if (Documents.Current != null)
            {
                var graph = CreateInstance<BOMAttributeMaint>();
                graph.Documents.Current = this.Documents.Current;
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
            }

            return adapter.Get();
        }

        /// <summary>
        /// Copy bom smart panel to copy the current BOM contents to a new BOM using the entered item/site/sub item
        /// </summary>
        public PXAction<AMBomItem> AMCopyBom;
        /// <summary>
        /// Copy bom smart panel to copy the current BOM contents to a new BOM using the entered item/site/sub item
        /// </summary>
        [PXUIField(DisplayName = Messages.CopyBom, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable aMCopyBom(PXAdapter adapter)
        {
            var list = adapter.Get<AMBomItem>().ToList();

            copyBomFilter.Current.FromBOMID = Documents.Current.BOMID;
            copyBomFilter.Current.FromRevisionID = Documents.Current.RevisionID;
            copyBomFilter.Current.FromInventoryID = Documents.Current.InventoryID;
            copyBomFilter.Current.FromSubItemID = Documents.Current.SubItemID;
            copyBomFilter.Current.FromSiteID = Documents.Current.SiteID;
                
            if (string.IsNullOrEmpty(copyBomFilter.Current.ToBOMID) && !BomNumbering.Current.UserNumbering.GetValueOrDefault())
            {
                copyBomFilter.Current.ToBOMID = BomNumbering.Current.NewSymbol;
            }
            if (copyBomFilter.Current.ToInventoryID.GetValueOrDefault() == 0)
            {
                copyBomFilter.Current.ToInventoryID = Documents.Current.InventoryID;
                copyBomFilter.Cache.SetDefaultExt<CopyBomFilter.toSubItemID>(copyBomFilter.Current);
                copyBomFilter.Cache.SetDefaultExt<CopyBomFilter.toSubItemCD>(copyBomFilter.Current);
            }
            if (copyBomFilter.Current.ToSiteID.GetValueOrDefault() == 0)
            {
                copyBomFilter.Current.ToSiteID = Documents.Current.SiteID;
            }

            if (copyBomFilter.AskExt() == WebDialogResult.OK)
            {
                if (BomNumbering.Current.UserNumbering.GetValueOrDefault())
                {
                    CheckExistingBom(copyBomFilter.Current.ToBOMID);
                }

                if (!string.IsNullOrWhiteSpace(copyBomFilter.Current.ToSubItemCD) && InventoryHelper.SubItemFeatureEnabled)
                {
                    copyBomFilter.Current.ToSubItemID = InventoryHelper.GetSubItem(this, copyBomFilter.Current.ToSubItemCD, true)?.SubItemID;
                }

                if (!ValidCopyBomFilter(copyBomFilter.Current))
                {
                    throw new ArgumentException(Messages.GetLocal(Messages.ArgumentInObjectNameInvalid, copyBomFilter.Name));
                }

                var sourceBom = PXCache<AMBomItem>.CreateCopy(Documents.Current);

                CopyBom(sourceBom, copyBomFilter.Current);
                    
                var rs = new List<AMBomItem> { Documents.Current };
                return rs;
            }
            return list;
        }

        #endregion

        public PXAction<AMBomItem> ArchiveBom;
        [PXUIField(DisplayName = Messages.ArchiveBom, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable archiveBom(PXAdapter adapter)
        {
            var currentBomItem = Documents.Current;
            if (currentBomItem == null)
            {
                return adapter.Get();
            }

            if (currentBomItem.Hold.GetValueOrDefault())
            {
                //Change of hold status will reset the overall status. Update in 2 steps if currently on hold.
                currentBomItem.Hold = false;
                Documents.Update(currentBomItem);
            }

            currentBomItem.Status = AMBomStatus.Archived;
            
            if (currentBomItem.EffEndDate == null)
            {
                currentBomItem.EffEndDate = Accessinfo.BusinessDate;
            }

            Documents.Update(currentBomItem);

            Persist();

            return adapter.Get();
        }

        public PXAction<AMBomItem> CreateECR;
        [PXUIField(DisplayName = "Create ECR", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visibility =PXUIVisibility.Invisible)]
        [PXButton]
        public virtual IEnumerable createECR(PXAdapter adapter)
        {
            var currentBomItem = Documents.Current;
            if (currentBomItem?.RevisionID == null)
            {
                return adapter.Get();
            }

            var ecrGraph = CreateInstance<ECRMaint>();

            ecrGraph.FieldDefaulting.AddHandler<AMECRItem.bOMRevisionID>((sender, e) => { e.NewValue = currentBomItem.RevisionID; });

            var newEcr = ecrGraph.Documents.Insert();
            if (newEcr == null)
            {
                return adapter.Get();
            }
            newEcr.BOMID = currentBomItem.BOMID;
            newEcr.BOMRevisionID = currentBomItem.RevisionID;
            ecrGraph.Documents.Update(newEcr);

            PXRedirectHelper.TryRedirect(ecrGraph, PXRedirectHelper.WindowMode.NewWindow);

            return adapter.Get();
        }

        public PXAction<AMBomItem> BOMCompare;
        [PXUIField(DisplayName = "Compare BOM", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable bOMCompare(PXAdapter adapter)
        {
            var currentBomItem = Documents.Current;
            if (currentBomItem != null)
            { 
                var graph = CreateInstance<BOMCompareInq>();

                graph.Filter.Current.IDType1 = BOMCompareInq.IDTypes.BOM;
                graph.Filter.Current.BOMID1 = currentBomItem.BOMID;
                graph.Filter.Current.RevisionID1 = currentBomItem.RevisionID;

                throw new PXRedirectRequiredException(graph, Messages.BOMCompare);
            }

            return adapter.Get();
        }

        #region BOM Cost Settings
        public PXAction<AMBomItem> AMBomCostSettings;
        [PXUIField(DisplayName = Messages.BOMCostSummary, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable aMBomCostSettings(PXAdapter adapter)
        {
            if (Documents.Current != null
                && rollsettings.Current != null
                && rollsettings.Current.LotSize == null)
            {
                rollsettings.Cache.SetValueExt<RollupSettings.lotSize>(rollsettings.Current,
                    InventoryHelper.GetMfgReorderQty(this,
                        Documents.Current.InventoryID,
                        Documents.Current.SiteID));
            }

            if (rollsettings.Current != null
                && rollsettings.Current.LotSize.GetValueOrDefault() <= 0)
            {
                rollsettings.Current.LotSize = 1;
            }

            if (rollsettings.AskExt() == WebDialogResult.OK)
            {
                rollsettings.Current.ApplyPend = false;
                rollsettings.Current.BOMID = Documents.Current.BOMID;
                rollsettings.Current.RevisionID = Documents.Current.RevisionID;
                rollsettings.Current.IncFixed = true;
                rollsettings.Current.IncMatScrp = true;
                rollsettings.Current.UpdateMaterial = false;
                // Call the action to run and display the cost roll
                aMBomCostSummary(adapter);
            }

            rollsettings.Cache.Clear();
            rollsettings.ClearDialog();

            return adapter.Get();
        }

        #region BOM Cost Summary
        public PXAction<AMBomItem> AMBomCostSummary;
        [PXUIField(DisplayName = Messages.BOMCostSummary, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable aMBomCostSummary(PXAdapter adapter)
        {
            if (rollsettings.Current != null)
            {
                if (Documents.Current?.EffEndDate != null)
                {
                    rollsettings.Current.EffectiveDate = rollsettings.Current.EffectiveDate.LesserDateTime(Documents.Current?.EffEndDate);
                }
                BOMCostRoll.RollCostsRetry(rollsettings.Current);
                BomCostRecs.Select();
                BomCostRecs.AskExt();
            }
            return adapter.Get();
        }
        #endregion

        #endregion

        #region Get/Set Primary BOMID

        protected virtual void SetPrimeBomID(AMBomItem bomItem)
        {
            if (bomItem?.RevisionID == null)
            {
                return;
            }

            new PrimaryBomIDManager(this).SetAllFirstOnlyPrimary(bomItem);
        }

        protected virtual void RemovePrimeBomID(AMBomItem bomItem)
        {
            if (bomItem?.RevisionID == null)
            {
                return;
            }

            new PrimaryBomIDManager(this)
            {
                PersistChanges = false
            }.RemovePrimary(bomItem);
        }

        #endregion

        protected virtual void CopyBomFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            //set fields enabled status
            PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromBOMID>(cache, null, false);
            PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromRevisionID>(cache, null, false);
            PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromInventoryID>(cache, null, false);
            PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromSubItemID>(cache, null, false);
            PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromSiteID>(cache, null, false);
            PXUIFieldAttribute.SetEnabled<CopyBomFilter.toBOMID>(cache, null, BomNumbering?.Current?.UserNumbering == true);

            PXUIFieldAttribute.SetVisible<CopyBomFilter.updateMaterialWarehouse>(cache, null, InventoryHelper.MultiWarehousesFeatureEnabled);
        }

        protected virtual void EnableBomItemFields(PXCache cache, AMBomItem item)
        {
            //  Handle enable status of inventory id and site id for a bom already created
            //      to prevent a user from changing accidentally

            var enableItemChange = cache.IsRowInserted(item);

            PXUIFieldAttribute.SetEnabled<AMBomItem.inventoryID>(cache, null, enableItemChange);
            PXUIFieldAttribute.SetEnabled<AMBomItem.subItemID>(cache, null, enableItemChange);
            PXUIFieldAttribute.SetEnabled<AMBomItem.siteID>(cache, null, enableItemChange);
            PXUIFieldAttribute.SetEnabled<AMBomItem.descr>(cache, null, item.Hold.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMBomItem.effStartDate>(cache, null, item.Hold.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMBomItem.effEndDate>(cache, null, item.Hold.GetValueOrDefault());
        }

        /// <summary>
        /// Create an INItemSite record if one doesn't exist for the bom item/site
        /// </summary>
        /// <param name="amBomItem">BOM containing item/site ids</param>
        protected virtual void DefaultItemSiteByBOM(AMBomItem amBomItem)
        {
            if (amBomItem?.InventoryID == null
                || amBomItem.SiteID == null)
            {
                return;
            }

            DefaultItemSite(amBomItem.InventoryID, amBomItem.SiteID);
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
            foreach (AMBomItem amBomItem in this.Documents.Cache.Inserted)
            {
                DefaultItemSiteByBOM(amBomItem);
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

        public override bool CanCreateNewRevision(BOMMaint fromGraph, BOMMaint toGraph, string keyValue,
            string revisionValue, out string error)
        {
#if DEBUG
            AMDebug.TraceWriteMethodName($"key '{keyValue}' rev '{revisionValue}'");
#endif
            if(Features.ECCEnabled() && ambsetup.Current?.ForceECR == true)
            {
                error = Messages.GetLocal(Messages.ECRRequired);
                return false;
            }

            // Always returns true as new revisions can be created at any time
            error = string.Empty;
            return true;
        }

        public override void CopyRevision(BOMMaint fromGraph, BOMMaint toGraph, string keyValue, string revisionValue)
        {
            if (toGraph?.Documents?.Current == null || fromGraph?.Documents?.Current == null)
            {
                //api calls should create new revs on their own - this causes issues when calling from api so we need to turn the copy rev logic off
                return;
            }

            toGraph.Documents.Cache.SetDefaultExt<AMBomItem.hold>(toGraph.Documents.Current);
            toGraph.Documents.Cache.SetDefaultExt<AMBomItem.status>(toGraph.Documents.Current);
            toGraph.Documents.Cache.SetDefaultExt<AMBomItem.effStartDate>(toGraph.Documents.Current);
            toGraph.Documents.Cache.SetDefaultExt<AMBomItem.effEndDate>(toGraph.Documents.Current);
            toGraph.Documents.Current.NoteID = null;

            PXNoteAttribute.CopyNoteAndFiles(fromGraph.Documents.Cache, fromGraph.Documents.Current,
                toGraph.Documents.Cache, toGraph.Documents.Current, true, true);

            if (SkipAutoCreateNewRevision())
            {
                return;
            }

            PXTrace.WriteInformation($"Copy BOM {fromGraph.Documents.Current?.BOMID} revision {fromGraph.Documents.Current?.RevisionID} to BOM {keyValue} revision {revisionValue}");
            CreateNewRevision(toGraph, fromGraph.Documents.Current, keyValue, revisionValue);
        }

        public virtual void CreateNewRevision(BOMMaint toGraph, AMBomItem sourceBOM, string newBOMID, string newRevisionID)
        {
            PXTrace.WriteInformation($"Creating new BOM {newBOMID} revision {newRevisionID}");
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Creating new BOM {newBOMID} revision {newRevisionID}");
#endif
            try
            {
                FieldUpdated.RemoveHandler<AMBomOper.wcID>(AMBomOper_WcID_FieldUpdated);
                FieldVerifying.AddHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);

                // prevent update from here
                copyBomFilter.Current.UpdateMaterialWarehouse = false;

#if DEBUG
                sw.Start();
#endif
                CopyBomOper(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
                var lastElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(lastElapsed, "CopyBomOper"));
#endif
                CopyBomMatl(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
                var currElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomMatl"));
                lastElapsed = currElapsed;
#endif
                CopyBomStep(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
                currElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomStep"));
                lastElapsed = currElapsed;
#endif
                CopyBomRef(sourceBOM, newBOMID, newRevisionID);
#if DEBUG
                currElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomRef"));
                lastElapsed = currElapsed;
#endif
                CopyBomTool(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
                currElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomTool"));
                lastElapsed = currElapsed;
#endif
                CopyBomOvhd(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
                currElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomOvhd"));
                lastElapsed = currElapsed;
#endif
                CopyBomAttributes(sourceBOM, newBOMID, newRevisionID);
#if DEBUG
                currElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomAttributes"));
                lastElapsed = currElapsed;
#endif

                // Set the primary bom ID if none currently set
                new PrimaryBomIDManager(this).SetAllFirstOnlyPrimary(
                    newBOMID,
                    sourceBOM.InventoryID,
                    sourceBOM.SiteID,
                    sourceBOM.SubItemID);
#if DEBUG
                currElapsed = sw.Elapsed;
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, $"PrimaryBomIDManager.SetAllFirstOnlyPrimary({newBOMID}, {sourceBOM.InventoryID}, {sourceBOM.SiteID}, {sourceBOM.SubItemID})"));
#endif
            }
            finally
            {
#if DEBUG
                sw.Stop();
                sb.AppendLine(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, $"Total CopyRevision Process Time / ElapsedTicks = {sw.ElapsedTicks}"));
                PXTraceHelper.WriteInformation(sb.ToString());
#endif
                FieldUpdated.AddHandler<AMBomOper.wcID>(AMBomOper_WcID_FieldUpdated);
                FieldVerifying.RemoveHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);
            }
        }

        public virtual void CopyBom(AMBomItem sourceBOM, CopyBomFilter copyFilter)
        {
            Clear(PXClearOption.PreserveTimeStamp);

            copyBomFilter.Current = copyFilter;

            if (sourceBOM == null)
            {
                throw new ArgumentNullException(nameof(sourceBOM));
            }

            if (copyFilter == null)
            {
                throw new ArgumentNullException(nameof(copyFilter));
            }

            var manualNumbering = BomNumbering?.Current?.UserNumbering ?? false;
            var newBomId = manualNumbering ? copyFilter.ToBOMID : null;

            if (manualNumbering && string.IsNullOrWhiteSpace(newBomId))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toBOMID>(this.copyBomFilter.Cache));
            }

            if (string.IsNullOrWhiteSpace(copyFilter.ToRevisionID))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toRevisionID>(this.copyBomFilter.Cache));
            }

            if (copyFilter.ToInventoryID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toInventoryID>(this.copyBomFilter.Cache));
            }

            if (InventoryHelper.SubItemFeatureEnabled && copyFilter.ToSubItemID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toSubItemID>(this.copyBomFilter.Cache));
            }

            if (InventoryHelper.MultiWarehousesFeatureEnabled && copyFilter.ToSiteID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toSiteID>(this.copyBomFilter.Cache));
            }

            PXTrace.WriteInformation($"Copy BOM {sourceBOM.BOMID} revision {sourceBOM.RevisionID} to BOM {newBomId} revision {copyFilter.ToRevisionID}");

            try
            {
                FieldUpdated.RemoveHandler<AMBomOper.wcID>(AMBomOper_WcID_FieldUpdated);
                FieldVerifying.AddHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);

                var targetBOM = PXCache<AMBomItem>.CreateCopy(sourceBOM);
                targetBOM.BOMID = newBomId;
                targetBOM.RevisionID = copyFilter.ToRevisionID;
                targetBOM.InventoryID = copyFilter.ToInventoryID;
                targetBOM.SubItemID = copyFilter.ToSubItemID;
                targetBOM.SiteID = copyFilter.ToSiteID;
                targetBOM.Hold = null;
                targetBOM.Status = null;
                targetBOM.NoteID = null;
                targetBOM.EffStartDate = null;
                targetBOM.EffEndDate = null;

                Documents.Cache.ForceExceptionHandling = true;
                targetBOM = Documents.Insert(targetBOM);

                if (copyFilter.CopyNotesItem.GetValueOrDefault())
                {
                    PXNoteAttribute.CopyNoteAndFiles(Documents.Cache, sourceBOM, Documents.Cache, targetBOM, true, true);
                }

                targetBOM = Documents.Update(targetBOM);

                CopyBomOper(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesOper.GetValueOrDefault());
                CopyBomMatl(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesMatl.GetValueOrDefault());
                CopyBomStep(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesStep.GetValueOrDefault());
                CopyBomRef(sourceBOM, newBomId, copyFilter.ToRevisionID);
                CopyBomTool(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesTool.GetValueOrDefault());
                CopyBomOvhd(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesOvhd.GetValueOrDefault());
                CopyBomAttributes(sourceBOM, newBomId, copyFilter.ToRevisionID);

                // Set the primary bom ID if none currently set
                new PrimaryBomIDManager(this).SetAllFirstOnlyPrimary(
                    newBomId,
                    targetBOM.InventoryID,
                    targetBOM.SiteID,
                    targetBOM.SubItemID);
            }
            finally
            {
                FieldUpdated.AddHandler<AMBomOper.wcID>(AMBomOper_WcID_FieldUpdated);
                FieldVerifying.RemoveHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);
            }
        }

        protected virtual void CheckExistingBom(string bomId)
        {
            //check if manual BOM ID already exists
            AMBomItem existingBomItem = PXSelect<AMBomItem, Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>>>.Select(this, bomId);

            if (existingBomItem != null)
            {
                throw new PXException(Messages.GetLocal(Messages.BomExists, bomId));
            }
        }

        protected virtual void CopyBomOper(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            CopyBomOper(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
        }

        internal virtual void CopyBomOper(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            foreach (AMBomOper fromRow in PXSelect<AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomItem.revisionID>>>>
            >.Select(this, sourceID, sourceRevisionID))
            {
                if (fromRow.RowStatus == AMRowStatus.Deleted)
                {
                    continue;
                }

                var toRow = PXCache<AMBomOper>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow.RowStatus = null;
                toRow = BomOperRecords.Insert(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomOperRecords.Cache, fromRow, BomOperRecords.Cache, toRow);
                    BomOperRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomMatl(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            CopyBomMatl(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
        }

        internal virtual void CopyBomMatl(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            foreach (PXResult<AMBomMatl, InventoryItem, AMBomItem, INItemSite> result in PXSelectJoin<
                AMBomMatl,
                InnerJoin<InventoryItem,
                    On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
               // Left Join because ECC uses this logic and no bom item
                LeftJoin<AMBomItem,
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
                .Select(this, sourceID, sourceRevisionID))
            {
                var fromRow = (AMBomMatl)result;
                var inventoryItem = (InventoryItem)result;
                
                if (fromRow.RowStatus == AMRowStatus.Deleted || inventoryItem == null ||
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
                toRow.RowStatus = null;

                if (toRow.CompBOMID != null && !IsValidBom(toRow.CompBOMID, toRow.CompBOMRevisionID))
                {
                    toRow.CompBOMID = null;
                    toRow.CompBOMRevisionID = null;
                }

                try
                {
                    toRow = BomMatlRecords.Insert(toRow);

                    if (toRow.SiteID != null && InventoryHelper.MultiWarehousesFeatureEnabled &&
                        copyBomFilter.Current.UpdateMaterialWarehouse == true && toRow.SiteID != copyBomFilter.Current.ToSiteID)
                    {
                        toRow.SiteID = copyBomFilter.Current.ToSiteID;

                        if(toRow.LocationID != null)
                        {
                            toRow.LocationID = null;
                        }
                    }

                    // The result uses the bom siteid, so if material has a site id we still want to call DefaultItemSite
                    var materialItemSite = (INItemSite) result;
                    if ((toRow.SiteID != null || materialItemSite == null) && ((AMBomItem)result)?.RevisionID != null)
                    {
                        DefaultItemSite(toRow.InventoryID, toRow.SiteID ?? copyBomFilter.Current.ToSiteID);
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
                .SelectWindowed(this, 0, 1, bomId, revisionId) != null ;
        }

        protected virtual void CopyBomStep(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            CopyBomStep(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
        }

        internal virtual void CopyBomStep(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelect<AMBomStep,
                Where<AMBomStep.bOMID, Equal<Required<AMBomStep.bOMID>>,
                    And<AMBomStep.revisionID, Equal<Required<AMBomStep.revisionID>>
                    >>>.Select(this, sourceID, sourceRevisionID);

            foreach (AMBomStep fromRow in fromRows)
            {
                if (fromRow.RowStatus == AMRowStatus.Deleted)
                {
                    continue;
                }

                var toRow = PXCache<AMBomStep>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow.RowStatus = null;
                toRow = BomStepRecords.Insert(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomStepRecords.Cache, fromRow, BomStepRecords.Cache, toRow);
                    BomStepRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomRef(AMBomItem sourceBOM, string newBOMID, string newRevisionID)
        {
            CopyBomRef(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID);
        }

        internal virtual void CopyBomRef(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID)
        {
            var fromRows = PXSelect<AMBomRef,
                Where<AMBomRef.bOMID, Equal<Required<AMBomRef.bOMID>>,
                    And<AMBomRef.revisionID, Equal<Required<AMBomRef.revisionID>>
                    >>>.Select(this, sourceID, sourceRevisionID);

            foreach (AMBomRef fromRow in fromRows)
            {
                if (fromRow.RowStatus == AMRowStatus.Deleted)
                {
                    continue;
                }

                var toRow = PXCache<AMBomRef>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow.RowStatus = null;
                BomRefRecords.Insert(toRow);
            }
        }

        protected virtual void CopyBomTool(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            CopyBomTool(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
        }

        internal virtual void CopyBomTool(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelectJoin<AMBomTool,
                InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
                Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
                    And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>
                    >>>.Select(this, sourceID, sourceRevisionID);

            foreach (AMBomTool fromRow in fromRows)
            {
                if (fromRow.RowStatus == AMRowStatus.Deleted)
                {
                    continue;
                }

                var toRow = PXCache<AMBomTool>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow.RowStatus = null;
                toRow = BomToolRecords.Insert(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomToolRecords.Cache, fromRow, BomToolRecords.Cache, toRow);
                    BomToolRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomOvhd(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
        {
            CopyBomOvhd(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
        }

        internal virtual void CopyBomOvhd(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelectJoin<AMBomOvhd,
                InnerJoin<AMOverhead, On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMBomOvhd.bOMID, Equal<Required<AMBomOvhd.bOMID>>,
                    And<AMBomOvhd.revisionID, Equal<Required<AMBomOvhd.revisionID>>
                    >>>.Select(this, sourceID, sourceRevisionID);

            foreach (AMBomOvhd fromRow in fromRows)
            {
                if (fromRow.RowStatus == AMRowStatus.Deleted)
                {
                    continue;
                }

                var toRow = PXCache<AMBomOvhd>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow.RowStatus = null;
                toRow = BomOvhdRecords.Insert(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomOvhdRecords.Cache, fromRow, BomOvhdRecords.Cache, toRow);
                    BomOvhdRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomAttributes(AMBomItem sourceBOM, string newBOMID, string newRevisionID)
        {
            CopyBomAttributes(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID);
        }

        internal virtual void CopyBomAttributes(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID)
        {
            FieldVerifying.AddHandler<AMBomAttribute.operationID>((sender, e) => { e.Cancel = true; });

            foreach (PXResult<AMBomAttribute, AMBomOper> result in PXSelectJoin<AMBomAttribute,
                    LeftJoin<AMBomOper, On<AMBomAttribute.bOMID, Equal<AMBomOper.bOMID>,
                            And<AMBomAttribute.revisionID, Equal<AMBomOper.revisionID>,
                        And<AMBomAttribute.operationID, Equal<AMBomOper.operationID>>>>>,
                Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                    And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>>>>
                .Select(this, sourceID, sourceRevisionID))
            {
                var fromBomAttribute = (AMBomAttribute) result;
                if (fromBomAttribute.RowStatus == AMRowStatus.Deleted)
                {
                    continue;
                }

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
                newBomAtt.RowStatus = null;

                var insertedAttribute = BomAttributes.Insert(newBomAtt);
                if (insertedAttribute != null)
                {
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

        protected void TraceCopyBomFilterFieldEmpty<Field>() where Field : IBqlField
        {
            PXTrace.WriteWarning(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<Field>(copyBomFilter.Cache));
        }

        protected bool ValidCopyBomFilter(CopyBomFilter copyBomFilter)
        {
            if (copyBomFilter == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(copyBomFilter.ToBOMID) && BomNumbering.Current.UserNumbering.GetValueOrDefault())
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.toBOMID>();
                return false;
            }

            if (string.IsNullOrWhiteSpace(copyBomFilter.FromBOMID))
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.fromBOMID>();
                return false;
            }

            if (string.IsNullOrWhiteSpace(copyBomFilter.ToRevisionID))
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.toRevisionID>();
                return false;
            }

            if (string.IsNullOrWhiteSpace(copyBomFilter.FromRevisionID))
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.fromRevisionID>();
                return false;
            }

            if (copyBomFilter.FromInventoryID == null)
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.fromInventoryID>();
                return false;
            }

            if (copyBomFilter.ToInventoryID == null)
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.toInventoryID>();
                return false;
            }

            if (InventoryHelper.MultiWarehousesFeatureEnabled && copyBomFilter.FromSiteID == null)
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.fromSiteID>();
                return false;
            }

            if (InventoryHelper.MultiWarehousesFeatureEnabled && copyBomFilter.ToSiteID == null)
            {
                TraceCopyBomFilterFieldEmpty<CopyBomFilter.toSiteID>();
                return false;
            }

            if (!ambsetup.Current.AllowEmptyBOMSubItemID.GetValueOrDefault() && InventoryHelper.SubItemFeatureEnabled)
            {
                if (string.IsNullOrWhiteSpace(copyBomFilter.ToSubItemCD))
                {
                    TraceCopyBomFilterFieldEmpty<CopyBomFilter.toSubItemCD>();
                    return false;
                }

                if (copyBomFilter.ToSubItemID.GetValueOrDefault() <= 0 &&
                    !string.IsNullOrWhiteSpace(copyBomFilter.ToSubItemCD))
                {
                    throw new PXException(ErrorMessages.ElementOfFieldDoesntExist,
                        copyBomFilter.ToSubItemCD,
                        PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toSubItemID>(this.copyBomFilter.Cache));
                }

            }

            return true;
        }

        public override void Persist()
        {
            InsertMissingINItemSite();

            var setPrimary = Documents.Cache.GetStatus(Documents.Current) == PXEntryStatus.Inserted &&
                             !HasAnotherRevision(Documents.Current);

            var estimateHistoryRow = GetInsertedEstimateHistory();

            base.Persist();

            if (setPrimary)
            {
                // Set Primary BOM is after persist so we can use the BOMID. Its doing a separate persist call
                SetPrimeBomID(Documents.Current);
            }

            PersistEstimateHistoryRow(Documents?.Current, estimateHistoryRow);
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

        protected virtual AMEstimateHistory GetInsertedEstimateHistory()
        {
            if (!this.Caches<AMEstimateHistory>().IsCurrentRowInserted())
            {
                return null;
            }

            var row = (AMEstimateHistory)this.Caches<AMEstimateHistory>().CreateCopy(this.Caches<AMEstimateHistory>().Current);
            this.Caches<AMEstimateHistory>().Remove(this.Caches<AMEstimateHistory>().Current);
            return row;
        }

        protected virtual void PersistEstimateHistoryRow(AMBomItem bomItem, AMEstimateHistory row)
        {
            if (Documents?.Current?.BOMID == null || row?.EstimateID == null)
            {
                return;
            }

            EstimateMaint.InsertEstimateHistory(row.EstimateID, row.RevisionID,
                Messages.GetLocal(Messages.EstimateCreatedBOM, Documents.Current.BOMID, Documents.Current.RevisionID, row.RevisionID));
        }

        protected virtual bool ECRAllowEdit(string bomid)
        {
            if(Features.ECCEnabled() && ambsetup.Current?.ForceECR == true)
            {
                var existingECR = (AMECRItem)PXSelect<AMECRItem, Where<AMECRItem.bOMID, Equal<Required<AMECRItem.bOMID>>>>.SelectWindowed(this, 0, 1, bomid);
                if (existingECR != null)
                {
                    return false;
                }
                var existingECO = (AMECOItem)PXSelect<AMECOItem, Where<AMECOItem.bOMID, Equal<Required<AMECOItem.bOMID>>>>.SelectWindowed(this, 0, 1, bomid);
                if (existingECO != null)
                {
                    return false;
                }
            }

            return true;
        }

        [Serializable]
        [PXCacheName("Default BOM Levels")]
        public class DefaultBomLevels : IBqlTable
        {
            #region Item
            public abstract class item : PX.Data.BQL.BqlBool.Field<item> { }
            protected bool? _Item;
            [PXBool]
            [PXUnboundDefault(true)]
            [PXUIField(DisplayName = "Item")]
            public virtual bool? Item
            {
                get
                {
                    return this._Item;
                }
                set
                {
                    this._Item = value;
                }
            }
            #endregion
            #region Warehouse
            public abstract class warehouse : PX.Data.BQL.BqlBool.Field<warehouse> { }
            protected bool? _Warehouse;
            [PXBool]
            [PXUnboundDefault(true)]
            [PXUIField(DisplayName = "Warehouse")]
            public virtual bool? Warehouse
            {
                get
                {
                    return this._Warehouse;
                }
                set
                {
                    this._Warehouse = value;
                }
            }
            #endregion
            #region SubItem
            public abstract class subItem : PX.Data.BQL.BqlBool.Field<subItem> { }
            protected bool? _SubItem;
            [PXBool]
            [PXUnboundDefault(true)]
            [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
            public virtual bool? SubItem
            {
                get
                {
                    return this._SubItem;
                }
                set
                {
                    this._SubItem = value;
                }
            }
            #endregion
        }
    }
}