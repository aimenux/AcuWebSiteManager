using System;
using System.Collections.Generic;
using PX.Objects.IN;
using PX.Data;

namespace PX.Objects.AM
{
    using PX.Objects.AM.Attributes;

    /// <summary>
    /// BOM Mass Change Graph
    /// </summary>
    public class BOMMassChange : PXGraph<BOMMassChange>
    {
        public PXFilter<BOMFilter> UpdateBomMatlRecs;

        public PXFilteredProcessingJoin<
            AMBomMatl, BOMFilter,
            InnerJoin<AMBomItem,
                On<AMBomMatl.bOMID, Equal<AMBomItem.bOMID>,
                And<AMBomMatl.revisionID, Equal<AMBomItem.revisionID>>>>,
            Where<AMBomMatl.inventoryID, Equal<Current<BOMFilter.currInvID>>,
                And<Where<AMBomMatl.expDate, IsNull,
                    Or<AMBomMatl.expDate, GreaterEqual<Current<BOMFilter.effStartDate>>>>>>> 
            SelectedBoms;
        
        //Used for cache attached
        [PXHidden]
        public PXSelect<AMBomItem> BomItems;

        // Required for the material line counters to persist correctly
        [PXHidden]
        public PXSelect<AMBomOper> BomOper;

        [InventoryItemNoRestrict]
        [PXDefault]
        protected virtual void AMBomMatl_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMBomOper.operationID,
                Where<AMBomOper.bOMID, Equal<Current<AMBomMatl.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Current<AMBomMatl.revisionID>>>>>),
            SubstituteKey = typeof(AMBomOper.operationCD))]
        protected virtual void AMBomMatl_OperationID_CacheAttached(PXCache sender)
        {
        }

        [InventoryItemNoRestrict(DisplayName = "Parent Inventory ID")]
        [PXDefault]
        protected virtual void AMBomItem_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBDate]
        [PXUIField(DisplayName = "Revision Start Date", Visible = false)]
        protected virtual void AMBomItem_EffStartDate_CacheAttached(PXCache sender)
        {
        }

        [PXDBDate]
        [PXUIField(DisplayName = "Revision End Date", Visible = false)]
        protected virtual void AMBomItem_EffEndDate_CacheAttached(PXCache sender)
        {
        }

        public BOMMassChange()
        {
            PXUIFieldAttribute.SetDisplayName<AMBomMatl.siteID>(SelectedBoms.Cache, "Material Warehouse");
            PXUIFieldAttribute.SetDisplayName<AMBomMatl.descr>(SelectedBoms.Cache, "Material Description");
            PXUIFieldAttribute.SetDisplayName<AMBomItem.siteID>(BomItems.Cache, "BOM Warehouse");
            PXUIFieldAttribute.SetDisplayName<AMBomItem.subItemID>(BomItems.Cache, "Parent Subitem");
            PXUIFieldAttribute.SetDisplayName<AMBomItem.status>(BomItems.Cache, "Revision Status");
            PXUIFieldAttribute.SetVisible<AMBomMatl.bOMID>(SelectedBoms.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMBomMatl.revisionID>(SelectedBoms.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMBomMatl.operationID>(SelectedBoms.Cache, null, true);

            var filter = UpdateBomMatlRecs.Current;
#pragma warning disable PX1088 // Processing delegates cannot use the data views from processing graphs except for the data views of the PXFilter, PXProcessingBase, or PXSetup types
#if DEBUG
            // OK to ignore PX1088 per Dmitry 11/19/2019 - Simple cache locate and set of current on "BomOper" view to make sure Line Counters work for parent data
#endif
            SelectedBoms.SetProcessDelegate(list => ProcessChange(list, filter));
#pragma warning restore PX1088
        }

        protected virtual void BOMFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var filter = (BOMFilter)e.Row;
            if (filter?.CurrInvID == null || filter?.NewInvID == null)
            {
                SelectedBoms.SetProcessAllEnabled(false);
                SelectedBoms.SetProcessEnabled(false);
                return;
            }

            // Check for Non Stock and ignore Subitems if item is Non Stock
            InventoryItem invItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, filter.CurrInvID);
            if (invItem?.InventoryID == null)
            {
                SelectedBoms.SetProcessAllEnabled(false);
                SelectedBoms.SetProcessEnabled(false);
                return;
            }

            var hasSubItemsEntered = filter.CurrSubItemID != null && filter.NewSubItemID != null;

            var processEnabled = !InventoryHelper.SubItemFeatureEnabled || !invItem.StkItem.GetValueOrDefault() || hasSubItemsEntered;

            SelectedBoms.SetProcessAllEnabled(processEnabled);
            SelectedBoms.SetProcessEnabled(processEnabled);
        }

        protected virtual void BOMFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (!sender.ObjectsEqual<BOMFilter.currInvID, BOMFilter.currSubItemID>(e.Row, e.OldRow))
            {
                SelectedBoms.Cache.Clear();
            }
        }

        protected virtual void ChangeMaterial(AMBomMatl oldBomMatl)
        {
            if (oldBomMatl?.BOMID == null)
            {
                return;
            }

            if (oldBomMatl?.BOMID == null || SelectedBoms.Cache.IsRowDeleted(oldBomMatl))
            {
                return;
            }

            if (TryInsertNewMaterialFromOldMaterial(oldBomMatl))
            {
                if (UpdateBomMatlRecs.Current.EffStartDate > Accessinfo.BusinessDate)
                {
                    DateTime updateExpDate = (DateTime)UpdateBomMatlRecs.Current.EffStartDate;
                    oldBomMatl.ExpDate = updateExpDate.AddDays(-1);
                    SelectedBoms.Update(oldBomMatl);
                }
                else
                {
                    SelectedBoms.Delete(oldBomMatl);
                }
            }
        }

        protected virtual AMBomOper GetBomOper<T>(T oper) where T : IBqlTable, IBomOper
        {
            var cachedOper = BomOper.Locate(new AMBomOper
            {
                BOMID = oper.BOMID,
                RevisionID = oper.RevisionID,
                OperationID = oper.OperationID
            });

            if (cachedOper?.OperationID != null)
            {
                return cachedOper;
            }

            return PXSelect<AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
                        And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>
                    >>>.SelectWindowed(this, 0, 1, oper.BOMID, oper.RevisionID, oper.OperationID);
        }

        protected virtual bool TryInsertNewMaterialFromOldMaterial(AMBomMatl oldBomMatl)
        {
            var newBomMatl = CopyBomMatl(oldBomMatl);
            if (newBomMatl == null)
            {
                return false;
            }

            var bomOper = GetBomOper(newBomMatl);
            if (bomOper?.OperationID == null)
            {
                return false;
            }

            // Assist in set of line counters
            //To resolve PX1088 we cannot refer to the BomOper view directly
            this.Caches<AMBomOper>().Current = bomOper;

            var insertedBomMatl = SelectedBoms.Insert(newBomMatl);
            return insertedBomMatl?.LineID != null;
        }

        protected virtual AMBomMatl CopyBomMatl(AMBomMatl oldBomMatl)
        {
            if (oldBomMatl.BOMID == null)
            {
                return null;
            }

            var newBomMatl = (AMBomMatl)SelectedBoms.Cache.CreateCopy(oldBomMatl);
            newBomMatl.LineID = null;
            newBomMatl.Descr = null;
            newBomMatl.NoteID = null;
            newBomMatl.CompBOMID = null;
            newBomMatl.CompBOMRevisionID = null;
            newBomMatl.EffDate = UpdateBomMatlRecs.Current.EffStartDate > Accessinfo.BusinessDate ? UpdateBomMatlRecs.Current.EffStartDate : null;
            newBomMatl.ExpDate = null;
            newBomMatl.UOM = null; //let the insert find the default UOM to be used (as change of item might have different UOM allowed)
            newBomMatl.InventoryID = UpdateBomMatlRecs?.Current?.NewInvID;
            newBomMatl.SortOrder = oldBomMatl.SortOrder;

            if (InventoryHelper.SubItemFeatureEnabled)
            {
                newBomMatl.SubItemID = UpdateBomMatlRecs?.Current?.NewSubItemID;
            }

            return newBomMatl;
        }

        public static void ProcessChange(List<AMBomMatl> list, BOMFilter filter)
        {
            var failed = false;

            var graph = CreateInstance<BOMMassChange>();
            
            for (var i = 0; i < list.Count; i++)
            {
                try
                {
                    graph.Clear();
                    graph.UpdateBomMatlRecs.Current = filter;

                    if (graph.UpdateBomMatlRecs.Current?.NewInvID == null)
                    {
                        throw new ArgumentNullException(nameof(graph.UpdateBomMatlRecs.Current.NewInvID));
                    }

                    graph.ChangeMaterial(list[i]);
                    graph.Persist();
                    PXProcessing<AMBomMatl>.SetInfo(i, PX.Data.ActionsMessages.RecordProcessed);
                }
                catch (Exception e)
                {
                    if (list.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }

                    PXProcessing<AMBomMatl>.SetError(i, e);
                    failed = true;

                    //Record error to trace window 
                    PXTraceHelper.PxTraceException(e);
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }
        }

        #region Hyperlinks

        public PXAction<BOMFilter> ViewBOM;
        [PXLookupButton]
        [PXUIField(DisplayName = "View BOM")]
        protected virtual void viewBOM()
        {
            if (SelectedBoms.Current == null)
            {
                return;
            }

            var graphBOM = PXGraph.CreateInstance<BOMMaint>();

            AMBomItem bomItem = PXSelect<AMBomItem,
                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                    And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>
                    >>>.Select(graphBOM, SelectedBoms.Current.BOMID, SelectedBoms.Current.RevisionID);

            if (bomItem != null)
            {
                graphBOM.Documents.Current = bomItem;
            }

            if (graphBOM.Documents.Current != null)
            {
                throw new PXRedirectRequiredException(graphBOM, true, string.Empty);
            }
        }
        #endregion
    }

    #region BOM Filter DAC Fields
    [Serializable]
    [PXCacheName("BOM Inventory Filter")]
    public class BOMFilter : IBqlTable
    {
        #region Current InventoryID
        public abstract class currInvID : PX.Data.BQL.BqlInt.Field<currInvID> { }

        protected Int32? _CurrInvID;
        [InventoryItemNoRestrict(DisplayName = "Current Inventory ID", Required = true)]
        [PXDefault]
        public virtual Int32? CurrInvID
        {
            get
            {
                return _CurrInvID;
            }
            set
            {
                _CurrInvID = value;
            }
        }
        #endregion
        #region Current SubItemID
        public abstract class currSubItemID : PX.Data.BQL.BqlInt.Field<currSubItemID> { }

        protected Int32? _CurrSubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<BOMFilter.currInvID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXFormula(typeof(Default<BOMFilter.newInvID>))]
        [SubItem(typeof(BOMFilter.currInvID), DisplayName = "Current SubItem ID")]
        public virtual Int32? CurrSubItemID
        {
            get
            {
                return this._CurrSubItemID;
            }
            set
            {
                this._CurrSubItemID = value;
            }
        }
        #endregion
        #region  New InventoryID
        public abstract class newInvID : PX.Data.BQL.BqlInt.Field<newInvID> { }

        protected Int32? _NewInvID;
        [Inventory(DisplayName = "New Inventory ID", Required = true)]
        [PXDefault]
        public virtual Int32? NewInvID
        {
            get
            {
                return _NewInvID;
            }
            set
            {
                _NewInvID = value;
            }
        }
        #endregion
        #region New SubItemID
        public abstract class newSubItemID : PX.Data.BQL.BqlInt.Field<newSubItemID> { }

        protected Int32? _NewSubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<BOMFilter.newInvID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXFormula(typeof(Default<BOMFilter.newInvID>))]
        [SubItem(typeof(BOMFilter.newInvID), DisplayName = "New SubItem ID")]
        public virtual Int32? NewSubItemID
        {
            get
            {
                return this._NewSubItemID;
            }
            set
            {
                this._NewSubItemID = value;
            }
        }
        #endregion
        #region EffStartDate
        public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }

        protected DateTime? _EffStartDate;
        [DaysOffsetDate(MinOffsetDays = "0")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Eff. Start Date")]
        public virtual DateTime? EffStartDate
        {
            get
            {
                return this._EffStartDate;
            }
            set
            {
                this._EffStartDate = value;
            }
        }
        #endregion
    }
    #endregion
}
