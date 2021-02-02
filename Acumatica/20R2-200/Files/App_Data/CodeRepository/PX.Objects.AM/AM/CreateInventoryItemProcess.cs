using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.TX;

namespace PX.Objects.AM
{
    /// <summary>
    /// Estimate module - creation of non-inventory items to stock/non-stock items
    /// </summary>
    public class CreateInventoryItemProcess : PXGraph<CreateInventoryItemProcess>
    {
        public PXCancel<CreateInventoryItemFilter> Cancel;

        public PXFilter<CreateInventoryItemFilter> Filter;
        public PXFilteredProcessing<NonInventoryItem, CreateInventoryItemFilter> NonInventoryItems;
        
        [PXHidden]
        public PXSelect<AMEstimateItem> EstimateRecords;
        [PXHidden]
        public PXSelect<AMEstimateMatl> EstimateMatlRecords;

        public CreateInventoryItemProcess()
        {
            var currentFilter = Filter.Current;
            NonInventoryItems.SetProcessDelegate(
               delegate(List<NonInventoryItem> list)
               {
#pragma warning disable PX1088 // Processing delegates cannot use the data views from processing graphs except for the data views of the PXFilter, PXProcessingBase, or PXSetup types
#if DEBUG
                   // OK to ignore PX1088 per Dmitry 11/19/2019 - Cause is use of EstimateRecords and EstimateMatlRecords to update the estimate with the new item created information
#endif
                   Process(list, currentFilter);
#pragma warning restore PX1088
               });

            PXUIFieldAttribute.SetEnabled<NonInventoryItem.inventoryCD>(NonInventoryItems.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<NonInventoryItem.description>(NonInventoryItems.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<NonInventoryItem.siteID>(NonInventoryItems.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<NonInventoryItem.baseUnit>(NonInventoryItems.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<NonInventoryItem.itemClassID>(NonInventoryItems.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<NonInventoryItem.taxCategoryID>(NonInventoryItems.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<NonInventoryItem.postClassID>(NonInventoryItems.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<NonInventoryItem.lotSerClassID>(NonInventoryItems.Cache, null, true);
        }

        public override bool IsDirty => false;

        protected virtual void CreateInventoryItemFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CreateInventoryItemFilter row = (CreateInventoryItemFilter)e.Row;
            CreateInventoryItemFilter old = (CreateInventoryItemFilter)e.OldRow;
            if (row.EstimateID != old.EstimateID || row.RevisionID != old.RevisionID)
            {
                NonInventoryItems.Cache.Clear();
            }
        }

        protected virtual void CreateInventoryItemFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CreateInventoryItemFilter filter = (CreateInventoryItemFilter)e.Row;

            if (filter == null)
            {
                return;
            }
            NonInventoryItems.SetProcessAllEnabled(!string.IsNullOrEmpty(filter.EstimateID) && !string.IsNullOrEmpty(filter.RevisionID));
            NonInventoryItems.SetProcessEnabled(!string.IsNullOrEmpty(filter.EstimateID) && !string.IsNullOrEmpty(filter.RevisionID));
        }

        protected virtual void NonInventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = e.Row as NonInventoryItem;
            if (row == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(row.InventoryCD)
                && !string.IsNullOrWhiteSpace(row.OriginalInventoryCD)
                && !row.InventoryCD.EqualsWithTrim(row.OriginalInventoryCD))
            {
                sender.RaiseExceptionHandling<NonInventoryItem.inventoryCD>(row, row.InventoryCD,
                    new PXSetPropertyException(Messages.ValueChangedFromEstimateValue, PXErrorLevel.Warning, row.InventoryCD.TrimIfNotNullEmpty(), row.OriginalInventoryCD.TrimIfNotNullEmpty()));
            }
        }

        protected virtual void CreateInventoryItemFilter_EstimateID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CreateInventoryItemFilter filterRec = (CreateInventoryItemFilter) e.Row;
            if (filterRec == null)
            {
                return;
            }
            filterRec.RevisionID = null;
        }

        protected virtual void CreateInventoryItemFilter_RevisionID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var filterRec = (CreateInventoryItemFilter)e.Row;
            if (filterRec == null)
            {
                return;
            }

            AMEstimateItem estimateItem = PXSelect<AMEstimateItem, 
                Where<AMEstimateItem.estimateID, Equal<Current<CreateInventoryItemFilter.estimateID>>, 
                    And<AMEstimateItem.revisionID, Equal<Current<CreateInventoryItemFilter.revisionID>>>>>.Select(this);

            filterRec.Status = estimateItem?.EstimateStatus;
            filterRec.RevisionDate = estimateItem?.RevisionDate;
        }

        [EstimateID(IsKey = true)]
        [PXDefault]
        protected virtual void AMEstimateItem_EstimateID_CacheAttached(PXCache cache)
        {
        }

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC", IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        protected virtual void AMEstimateItem_RevisionID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void NonInventoryItem_InventoryCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            // Will validate the value during item creation
            e.Cancel = true;
        }

        /// <summary>
        /// Trim the given inventory CD to fit the mask.
        /// Necessary due to module release without segment/dimension support
        /// </summary>
        protected virtual string GetMaskedInventoryCDValue(NonInventoryItem row, string inventoryCD)
        {
            if (row == null || string.IsNullOrWhiteSpace(inventoryCD))
            {
                return inventoryCD;
            }
            return EstimateInventoryRawAttribute.GetDisplayMaskValue<NonInventoryItem.inventoryCD>(NonInventoryItems.Cache, row, inventoryCD);
        }

        protected virtual IEnumerable nonInventoryItems()
        {
            bool itVar1 = false;
            IEnumerator enumerator = this.NonInventoryItems.Cache.Inserted.GetEnumerator();
            while (enumerator.MoveNext())
            {
                NonInventoryItem itVar2 = (NonInventoryItem)enumerator.Current;
                itVar1 = true;
                yield return itVar2;
            }

            if (!itVar1 &&
                !string.IsNullOrWhiteSpace(Filter.Current.EstimateID) &&
                !string.IsNullOrWhiteSpace(Filter.Current.RevisionID))
            {
                int lineNbr = 0;
                // Get the estimateItem and add to non inventory grid if Non Inventory
                AMEstimateItem estimateItem = PXSelect<AMEstimateItem, 
                    Where<AMEstimateItem.estimateID, Equal<Current<CreateInventoryItemFilter.estimateID>>, 
                        And<AMEstimateItem.revisionID, Equal<Current<CreateInventoryItemFilter.revisionID>>, 
                            And<AMEstimateItem.isNonInventory, Equal<True>>>>>.Select(this);
                if (estimateItem != null)
                {
                    // Get the Reference Record
                    AMEstimateReference estimateReference = PXSelect<AMEstimateReference, 
                        Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateReference.estimateID>>,
                            And<AMEstimateReference.revisionID, Equal<Required<AMEstimateReference.revisionID>>>>>.Select(this, estimateItem.EstimateID, estimateItem.RevisionID);

                    // Get the Item Class
                    INItemClass itemClass = PXSelect<INItemClass, Where<INItemClass.itemClassID,
                        Equal<Required<INItemClass.itemClassID>>>>.Select(this, estimateItem.ItemClassID);

                    // Get the Estimate Class
                    AMEstimateClass estimateClass = PXSelect<AMEstimateClass, Where<AMEstimateClass.itemClassID,
                        Equal<Required<AMEstimateClass.itemClassID>>>>.Select(this, estimateItem.ItemClassID);

                    NonInventoryItem record = new NonInventoryItem
                    {
                        LineNbr = lineNbr++,
                        Level = NonInventoryItem.NonInventoryLevel.Estimate,
                        EstimateID = estimateItem.EstimateID,
                        RevisionID = estimateItem.RevisionID,
                        InventoryCD = estimateItem.InventoryCD,
                        OriginalInventoryCD = estimateItem.InventoryCD,
                        Description = estimateItem.ItemDesc,
                        ItemClassID = estimateItem.ItemClassID,
                        SiteID = estimateItem.SiteID,
                        BaseUnit = estimateItem.UOM,
                        UnitCost = estimateItem.UnitCost,
                        Body = estimateItem.Body,
                        ImageURL = estimateItem.ImageURL,
                        StockItem = true
                    };
                    //validate here....

                    if (!string.IsNullOrEmpty(estimateReference.TaxCategoryID))
                    {
                        record.TaxCategoryID = estimateReference.TaxCategoryID;
                    }
                    if (record.TaxCategoryID == null && !string.IsNullOrEmpty(itemClass.TaxCategoryID))
                    {
                        record.TaxCategoryID = itemClass.TaxCategoryID;
                    }
                    if (record.TaxCategoryID == null && !string.IsNullOrEmpty(estimateClass.TaxCategoryID))
                    {
                        record.TaxCategoryID = estimateClass.TaxCategoryID;
                    }

                    if (itemClass != null)
                    {
                        record.PostClassID = itemClass.PostClassID;
                        record.LotSerClassID = itemClass.LotSerClassID;
                    }
                        
                    record = NonInventoryItems.Insert(record);
                    if (record != null)
                    {
                        record.InventoryCD = GetMaskedInventoryCDValue(record, record.OriginalInventoryCD);
                        record = NonInventoryItems.Update(record);
                    }
                    yield return record;
                }

                // Get the estimate material records and add to non inventory grid if Non Inventory
                foreach (AMEstimateMatl slctdEstimateMatl in PXSelect<AMEstimateMatl, Where<AMEstimateMatl.estimateID,
                    Equal<Current<CreateInventoryItemFilter.estimateID>>, And<AMEstimateMatl.revisionID, 
                    Equal<Current<CreateInventoryItemFilter.revisionID>>, And<AMEstimateMatl.isNonInventory, 
                    Equal<boolTrue>>>>>.Select(this))
                {
                    // Get the Item Class
                    INItemClass itemClass = PXSelect<INItemClass, Where<INItemClass.itemClassID,
                        Equal<Required<INItemClass.itemClassID>>>>.Select(this, slctdEstimateMatl.ItemClassID);

                    NonInventoryItem record = new NonInventoryItem
                    {
                        LineNbr = lineNbr++,
                        Level = NonInventoryItem.NonInventoryLevel.Material,
                        EstimateID = slctdEstimateMatl.EstimateID,
                        RevisionID = slctdEstimateMatl.RevisionID,
                        OperationID = slctdEstimateMatl.OperationID,
                        LineID = slctdEstimateMatl.LineID,
                        InventoryCD = slctdEstimateMatl.InventoryCD,
                        OriginalInventoryCD = slctdEstimateMatl.InventoryCD,
                        Description = slctdEstimateMatl.ItemDesc,
                        ItemClassID = slctdEstimateMatl.ItemClassID,
                        SiteID = slctdEstimateMatl.SiteID,
                        BaseUnit = slctdEstimateMatl.UOM,
                        UnitCost = slctdEstimateMatl.UnitCost
                    };

                    if (itemClass != null)
                    {
                        record.TaxCategoryID = itemClass.TaxCategoryID;
                        record.PostClassID = itemClass.PostClassID;
                        record.LotSerClassID = itemClass.LotSerClassID;
                        record.StockItem = itemClass.StkItem;
                    }

                    record = NonInventoryItems.Insert(record);
                    if (record != null)
                    {
                        record.InventoryCD = GetMaskedInventoryCDValue(record, record.OriginalInventoryCD);
                        record = NonInventoryItems.Update(record);
                    }
                    yield return record;
                }
            }
            NonInventoryItems.Cache.IsDirty = false;
        }

        /// <summary>
        /// Process selected grid contents
        /// </summary>
        /// <param name="list">List of selected grid rows</param>
        ///// <param name="filter">Graph Filter</param>
        public static void Process(List<NonInventoryItem> list, CreateInventoryItemFilter currentFilter)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }

            var createInventoryItemsGraph = PXGraph.CreateInstance<CreateInventoryItemProcess>();
            var failed = false;
            
            for (var i = 0; i < list.Count; i++)
            {
                var row = list[i];
                try
                {
                    createInventoryItemsGraph.Clear();
                    createInventoryItemsGraph.Filter.Current = currentFilter;
                    createInventoryItemsGraph.CheckRequiredFields(row);
                    createInventoryItemsGraph.ProcessRow(row);
                    
                    PXProcessing<NonInventoryItem>.SetInfo(i, PX.Data.ActionsMessages.RecordProcessed);
                }
                catch (Exception e)
                {
                    PXProcessing<NonInventoryItem>.SetError(i, e);
                    failed = true;
                    PXTraceHelper.PxTraceException(e);
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }
        }

        /// <summary>
        /// Make sure all required fields have a value
        /// </summary>
        /// <param name="selectedInventoryItem">Row to check for valid required fields</param>
        protected virtual void CheckRequiredFields(NonInventoryItem selectedInventoryItem)
        {
            if (string.IsNullOrWhiteSpace(selectedInventoryItem.InventoryCD))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty,
                    PXUIFieldAttribute.GetDisplayName<NonInventoryItem.inventoryCD>(this.NonInventoryItems.Cache));
            }

            if (!EstimateInventoryRawAttribute.IsDimensionValid<InventoryItem.inventoryCD>(this.Caches[typeof(InventoryItem)], selectedInventoryItem.InventoryCD))
            {
                throw new PXException(Messages.EstimateInventoryCDNotValidDimension, selectedInventoryItem.InventoryCD);
            }

            if (selectedInventoryItem.ItemClassID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty,
                    PXUIFieldAttribute.GetDisplayName<NonInventoryItem.itemClassID>(this.NonInventoryItems.Cache));
            }

            if (string.IsNullOrWhiteSpace(selectedInventoryItem.BaseUnit))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty,
                    PXUIFieldAttribute.GetDisplayName<NonInventoryItem.baseUnit>(this.NonInventoryItems.Cache));
            }

            if (string.IsNullOrWhiteSpace(selectedInventoryItem.TaxCategoryID))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty,
                    PXUIFieldAttribute.GetDisplayName<NonInventoryItem.taxCategoryID>(this.NonInventoryItems.Cache));
            }

            if (selectedInventoryItem.Level == NonInventoryItem.NonInventoryLevel.Estimate || selectedInventoryItem.StockItem == true)
            {
                INItemClass itemClass = PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>
                        >>.Select(this, selectedInventoryItem.ItemClassID);
                if (itemClass.StkItem != true)
                {
                    throw new PXException(Messages.GetLocal(Messages.InvalidItemClass));
                }
            }
        }

        /// <summary>
        /// Process a single row
        /// </summary>
        /// <param name="row">Row to process</param>
        protected virtual void ProcessRow(NonInventoryItem row)
        {
            if (row == null || Filter.Current == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.InventoryCD))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<NonInventoryItem.inventoryCD>(this.NonInventoryItems.Cache));
            }

            var inventoryItem = EstimateInventoryRawAttribute.FindByInventoryCD(this, row.InventoryCD);

            if (inventoryItem != null && !Filter.Current.ReuseExistingInventoryID.GetValueOrDefault())
            {
                throw new PXException(Messages.InventoryIDAlreadyExists, row.InventoryCD);
            }

            if(inventoryItem == null)
            {
                inventoryItem = CreateInventoryItem(row);
            }

            UpdateEstimate(row, inventoryItem);
        }

        /// <summary>
        /// Create a stock/non stock item
        /// </summary>
        /// <param name="row">row used to create stock item</param>
        /// <returns>Newly created inventory item row</returns>
        protected virtual InventoryItem CreateInventoryItem(NonInventoryItem row)
        {
            return row.StockItem.GetValueOrDefault() 
                ? CreateInventoryItem(CreateInstance<InventoryItemMaint>(), row) 
                : CreateInventoryItem(CreateInstance<NonStockItemMaint>(), row);
        }

        protected virtual InventoryItem CreateInventoryItem(PXGraph graph, NonInventoryItem row)
        {
            try
            {
                var inventoryItem = (InventoryItem)graph.Caches[typeof(InventoryItem)].Insert(new InventoryItem
                {
                    InventoryCD = row.InventoryCD,
                    Descr = row.Description
                });
                
                inventoryItem.ItemClassID = row.ItemClassID;
                inventoryItem.BaseUnit = row.BaseUnit;
                inventoryItem.SalesUnit = row.BaseUnit;
                inventoryItem.PurchaseUnit = row.BaseUnit;
                inventoryItem.TaxCategoryID = row.TaxCategoryID;
                inventoryItem.PostClassID = row.PostClassID;
                inventoryItem.LotSerClassID = row.LotSerClassID;
                inventoryItem.DfltSiteID = row.SiteID;

                if (row.Level == NonInventoryItem.NonInventoryLevel.Estimate
                    && Filter.Current.CopyDetailedDescription.GetValueOrDefault())
                {
                    inventoryItem.Body = row.Body;
                }

                inventoryItem = (InventoryItem)graph.Caches[typeof(InventoryItem)].Update(inventoryItem);

                CopyNotesAndFiles(row, graph.Caches[typeof(InventoryItem)], inventoryItem);

                graph.Actions.PressSave();

                string itemImageUrl;
                if (TryGetInventoryImageURL(graph.Caches[typeof(InventoryItem)], row, inventoryItem, out itemImageUrl))
                {
                    inventoryItem.ImageUrl = itemImageUrl;
                    graph.Caches[typeof(InventoryItem)].Update(inventoryItem);
                    graph.Actions.PressSave();
                }
                
                return (InventoryItem)graph.Caches[typeof(InventoryItem)].Current;
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw new PXException(Messages.GetLocal(Messages.InventoryIDCouldNotBeCreated, row.InventoryCD.TrimIfNotNullEmpty(), e.Message));
            }
        }

        protected virtual void CopyNotesAndFiles(NonInventoryItem nonInventoryItem, PXCache itemCache, InventoryItem inventoryItem)
        {
            if (nonInventoryItem == null
                || nonInventoryItem.Level != NonInventoryItem.NonInventoryLevel.Estimate 
                || (!Filter.Current.CopyNotes.GetValueOrDefault() && !Filter.Current.CopyFiles.GetValueOrDefault()))
            {
                return;
            }

            AMEstimateItem estimateItem = PXSelect<AMEstimateItem,
                        Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                            And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>
                            >>>.Select(this, nonInventoryItem.EstimateID, nonInventoryItem.RevisionID);

            if (estimateItem == null || inventoryItem == null)
            {
                return;
            }

            PXNoteAttribute.CopyNoteAndFiles(EstimateRecords.Cache, estimateItem, itemCache, inventoryItem,
                Filter.Current.CopyNotes.GetValueOrDefault(), Filter.Current.CopyFiles.GetValueOrDefault());
        }

        protected virtual bool TryGetInventoryImageURL(PXCache cache, NonInventoryItem nonInventoryItem, InventoryItem inventoryItem, out string imageUrl)
        {
            imageUrl = null;
            if (nonInventoryItem == null 
                || !Filter.Current.CopyFiles.GetValueOrDefault()
                || nonInventoryItem.Level != NonInventoryItem.NonInventoryLevel.Estimate
                || string.IsNullOrWhiteSpace(nonInventoryItem.ImageURL))
            {
                return false;
            }

            imageUrl = NoteFileHelper.GetFileNameFromFileName(cache, inventoryItem, NoteFileHelper.GetFileName(nonInventoryItem.ImageURL));

            return !string.IsNullOrWhiteSpace(imageUrl);
        }

        protected virtual List<AMEstimateItem> GetEstimates(NonInventoryItem row, bool returnAllRevs)
        {
            return GetEstimates(row.EstimateID, row.RevisionID, returnAllRevs);
        }

        protected virtual List<AMEstimateItem> GetEstimates(string estimateID, string revisionID, bool returnAllRevs)
        {
            PXSelectBase<AMEstimateItem> cmd = new PXSelect<AMEstimateItem, 
                Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>>>(this);

            var paramList = new List<object>(){estimateID};
            if (!returnAllRevs)
            {
                paramList.Add(revisionID);
                cmd.WhereAnd<Where<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>>>();
            }

            return cmd.Select(paramList.ToArray()).FirstTableItems.ToList();
        }

        protected virtual List<AMEstimateMatl> GetEstimateMaterial(NonInventoryItem row, bool returnAllRevs)
        {
            if (row == null)
            {
                return null;
            }

            PXSelectBase<AMEstimateMatl> cmd = new PXSelect<AMEstimateMatl, 
                Where<AMEstimateMatl.estimateID, Equal<Required<AMEstimateMatl.estimateID>>, 
                    And<AMEstimateMatl.operationID, Equal<Required<AMEstimateMatl.operationID>>,
                    And<AMEstimateMatl.lineID, Equal<Required<AMEstimateMatl.lineID>>>>>,
                OrderBy<Asc<AMEstimateMatl.sortOrder, Asc<AMEstimateMatl.lineID>>>>(this);

            var paramList = new List<object>() { row.EstimateID, row.OperationID, row.LineID };
            if (!returnAllRevs)
            {
                paramList.Add(row.RevisionID);
                cmd.WhereAnd<Where<AMEstimateMatl.revisionID, Equal<Required<AMEstimateMatl.revisionID>>>>();
            }

            return cmd.Select(paramList.ToArray()).FirstTableItems.ToList();
        }

        /// <summary>
        /// Update the estimate revisions using the created item
        /// </summary>
        /// <param name="row">Processing row</param>
        /// <param name="inventoryItem">Created/Found inventory item</param>
        protected virtual void UpdateEstimate(NonInventoryItem row, InventoryItem inventoryItem)
        {
            if (row == null || inventoryItem == null || inventoryItem.InventoryID.GetValueOrDefault() == 0)
            {
                string inventoryCD = row == null ? string.Empty : row.InventoryCD;
                throw new PXException(Messages.UnableToUpdateEstimateRevisionsUsingInventoryID, inventoryCD);
            }

            if (row.Level == NonInventoryItem.NonInventoryLevel.Estimate)
            {
                UpdateEstimateItems(inventoryItem,
                    GetEstimates(row, Filter.Current.UpdateAllRevisions.GetValueOrDefault()));
            }

            if (row.Level == NonInventoryItem.NonInventoryLevel.Material )
            {
                UpdateEstimateMatls(inventoryItem,
                    GetEstimateMaterial(row, Filter.Current.UpdateAllRevisions.GetValueOrDefault()));
            }

            // Do Note Remove: To prevent false message of "Record has been deleted" warning on Inventory CD UI after processing...
            PXSelectorAttribute.Select<NonInventoryItem.inventoryCD>(this.NonInventoryItems.Cache, row);
            
            Actions.PressSave();
        }

        protected virtual void UpdateEstimateItems(InventoryItem inventoryItem, List<AMEstimateItem> estimateItems)
        {
            if (inventoryItem == null || estimateItems == null)
            {
                return;
            }

            foreach (var estimateItem in estimateItems)
            {
                if (inventoryItem.InventoryID.GetValueOrDefault() <= 0)
                {
                    Common.Cache.AddCacheView<InventoryItem>(this);
                    var itemCd = string.IsNullOrWhiteSpace(inventoryItem.InventoryCD) ? estimateItem.InventoryCD : inventoryItem.InventoryCD;
                    throw new PXException(Messages.UnableToUpdateEstimateInventory, PXUIFieldAttribute.GetDisplayName<InventoryItem.inventoryID>(this.Caches<InventoryItem>()), itemCd.TrimIfNotNullEmpty(), estimateItem.EstimateID, estimateItem.RevisionID);
                }
                
                EstimateRecords.Current = estimateItem;
                estimateItem.ItemClassID = inventoryItem.ItemClassID;
                estimateItem.InventoryID = inventoryItem.InventoryID;
                estimateItem.InventoryCD = inventoryItem.InventoryCD;
                estimateItem.ItemDesc = inventoryItem.Descr;
                estimateItem.IsNonInventory = false;
                EstimateRecords.Update(estimateItem);
            }
        }

        protected virtual void UpdateEstimateMatls(InventoryItem inventoryItem, List<AMEstimateMatl> estimateMatls)
        {
            if (inventoryItem == null || estimateMatls == null)
            {
                return;
            }

            foreach (var estimatematl in estimateMatls)
            {
                if (inventoryItem.InventoryID.GetValueOrDefault() <= 0)
                {
                    Common.Cache.AddCacheView<InventoryItem>(this);
                    var itemCd = string.IsNullOrWhiteSpace(inventoryItem.InventoryCD) ? estimatematl.InventoryCD : inventoryItem.InventoryCD;
                    throw new PXException(Messages.UnableToUpdateEstimateInventory, PXUIFieldAttribute.GetDisplayName<InventoryItem.inventoryID>(this.Caches<InventoryItem>()), itemCd.TrimIfNotNullEmpty(), estimatematl.EstimateID, estimatematl.RevisionID);
                }

                EstimateMatlRecords.Current = estimatematl;
                estimatematl.ItemClassID = inventoryItem.ItemClassID;
                estimatematl.InventoryID = inventoryItem.InventoryID;
                estimatematl.InventoryCD = inventoryItem.InventoryCD;
                estimatematl.ItemDesc = inventoryItem.Descr;
                estimatematl.IsNonInventory = false;
                EstimateMatlRecords.Update(estimatematl);
            }
        }

        /// <summary>
        /// Filter DAC for Create Inventory Items Graph
        /// </summary>
        [Serializable]
        [PXCacheName(Messages.CreateInventoryFilter)]
        public class CreateInventoryItemFilter : IBqlTable
        {
            #region EstimateID
            public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

            protected string _EstimateID;
            [EstimateID(Required = true)]
            [EstimateIDSelectAll]
            public virtual string EstimateID
            {
                get { return this._EstimateID; }
                set { this._EstimateID = value; }
            }
            #endregion
            #region RevisionID
            public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

            protected string _RevisionID;
            [PXString]
            [PXUIField(DisplayName = "Revision", Required = true)]
            [PXSelector(typeof(Search<AMEstimateItem.revisionID, Where<AMEstimateItem.estimateID, Equal<Current<CreateInventoryItemFilter.estimateID>>>>),
                ValidateValue = false)]
            public virtual string RevisionID
            {
                get { return this._RevisionID; }
                set { this._RevisionID = value; }
            }
            #endregion
            #region Status
            public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }

            protected int? _Status;
            [PXInt]
            [EstimateStatus.List]
            [PXUIField(DisplayName = "Status", Enabled = false)]
            public virtual int? Status
            {
                get { return this._Status; }
                set { this._Status = value; }
            }
            #endregion
            #region RevisionDate
            public abstract class revisionDate : PX.Data.BQL.BqlDateTime.Field<revisionDate> { }

            protected DateTime? _RevisionDate;
            [PXDate()]
            [PXUIField(DisplayName = "Revision Date", Enabled = false)]
            public virtual DateTime? RevisionDate
            {
                get
                {
                    return this._RevisionDate;
                }
                set
                {
                    this._RevisionDate = value;
                }
            }
            #endregion
            #region UpdateAllRevisions
            public abstract class updateAllRevisions : PX.Data.BQL.BqlBool.Field<updateAllRevisions> { }

            protected bool? _UpdateAllRevisions = false;
            [PXBool]
            [PXDefault(typeof(AMEstimateSetup.updateAllRevisions), PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Update All Revisions", Enabled = true)]
            public virtual bool? UpdateAllRevisions
            {
                get { return _UpdateAllRevisions; }
                set { _UpdateAllRevisions = value; }
            }
            #endregion
            #region ReuseExistingInventoryID
            public abstract class reuseExistingInventoryID : PX.Data.BQL.BqlBool.Field<reuseExistingInventoryID> { }

            protected bool? _ReuseExistingInventoryID;
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = "Reuse Existing Inventory ID", Enabled = true)]
            public virtual bool? ReuseExistingInventoryID
            {
                get { return _ReuseExistingInventoryID; }
                set { _ReuseExistingInventoryID = value; }
            }
            #endregion
            #region CopyDetailedDescription
            public abstract class copyDetailedDescription : PX.Data.BQL.BqlBool.Field<copyDetailedDescription> { }

            protected bool? _CopyDetailedDescription;
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = "Copy Detailed Description", Enabled = true)]
            public virtual bool? CopyDetailedDescription
            {
                get { return _CopyDetailedDescription; }
                set { _CopyDetailedDescription = value; }
            }
            #endregion
            #region CopyFiles
            public abstract class copyFiles : PX.Data.BQL.BqlBool.Field<copyFiles> { }

            protected bool? _CopyFiles;
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = "Copy Files/Image", Enabled = true)]
            public virtual bool? CopyFiles
            {
                get { return _CopyFiles; }
                set { _CopyFiles = value; }
            }
            #endregion
            #region CopyNotes
            public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }

            protected bool? _CopyNotes;
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = "Copy Notes", Enabled = true)]
            public virtual bool? CopyNotes
            {
                get { return _CopyNotes; }
                set { _CopyNotes = value; }
            }
            #endregion
        }

        /// <summary>
        /// Grid detail DAC containing non-inventory estimate/material items
        /// </summary>
        [Serializable]
        [PXCacheName(Messages.NonInventoryFilter)]
        public class NonInventoryItem : IBqlTable
        {
            #region Selected
            public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

            protected bool? _Selected;
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = "Selected", Enabled = true)]
            public virtual bool? Selected
            {
                get { return _Selected; }
                set { _Selected = value; }
            }
            #endregion
            #region LineNbr (KEY)
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

            protected Int32? _LineNbr;
            [PXInt(IsKey = true)]
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
            #region EstimateID
            public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

            protected string _EstimateID;
            [EstimateID(DisplayName = "Estimate ID", IsDBField = false)]
            [PXDefault]
            public virtual string EstimateID
            {
                get { return this._EstimateID; }
                set { this._EstimateID = value; }
            }
            #endregion
            #region RevisionID
            public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

            protected string _RevisionID;
            [PXString]
            [PXUIField(DisplayName = "Revision", Required = true)]
            public virtual string RevisionID
            {
                get { return this._RevisionID; }
                set { this._RevisionID = value; }
            }
            #endregion
            #region Operation ID
            public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

            protected Int32? _OperationID;
            [PXInt]
            public virtual Int32? OperationID
            {
                get { return this._OperationID; }
                set { this._OperationID = value; }
            }
            #endregion
            #region Line ID
            public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

            protected Int32? _LineID;
            [PXInt]
            public virtual Int32? LineID
            {
                get { return this._LineID; }
                set { this._LineID = value; }
            }
            #endregion
            #region Level
            public abstract class level : PX.Data.BQL.BqlInt.Field<level> { }

            protected int? _Level;
            [PXInt]
            [PXUIField(DisplayName = "Level", Visible = true)]
            [NonInventoryLevel.List]
            public virtual int? Level
            {
                get { return this._Level; }
                set { this._Level = value; }
            }
            #endregion
            #region InventoryCD
            public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }

            protected string _InventoryCD;
            [PXDefault]
            [EstimateInventoryRaw]
            public virtual string InventoryCD
            {
                get { return this._InventoryCD; }
                set { this._InventoryCD = value; }
            }
            #endregion
            #region OriginalInventoryCD
            public abstract class originalInventoryCD : PX.Data.BQL.BqlString.Field<originalInventoryCD> { }

            protected string _OriginalInventoryCD;
            [PXString]
            [PXUIField(DisplayName = "Original Inventory ID", Enabled = false, Visible = false)]
            public virtual string OriginalInventoryCD
            {
                get { return this._OriginalInventoryCD; }
                set { this._OriginalInventoryCD = value; }
            }
            #endregion
            #region Description
            public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

            protected string _Description;
            [PXString(255, IsUnicode = true)]
            [PXUIField(DisplayName = "Description")]
            public virtual string Description
            {
                get { return this._Description; }
                set { this._Description = value; }
            }
            #endregion
            #region ItemClassID
            public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

            protected int? _ItemClassID;
            [PXDBInt]
            [PXUIField(DisplayName = "Item Class")]
            [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
            [PXDefault]
            public virtual int? ItemClassID
            {
                get
                {
                    return this._ItemClassID;
                }
                set
                {
                    this._ItemClassID = value;
                }
            }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

            protected Int32? _SiteID;
            [Site(DisplayName = "Default Warehouse", IsDBField = false)]
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
            #region BaseUnit
            public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }

            protected string _BaseUnit;
            [INUnit(DisplayName = "Base Unit", IsDBField = false)]
            [PXDefault]
            public virtual string BaseUnit
            {
                get { return this._BaseUnit; }
                set { this._BaseUnit = value; }
            }
            #endregion
            #region TaxCategoryID
            public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

            protected string _TaxCategoryID;
            [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
            [PXRestrictor(typeof(Where<TaxCategory.active, Equal<PX.Data.True>>), "Tax Category '{0}' is inactive", new System.Type[] { typeof(TaxCategory.taxCategoryID) })]
            [PXUIField(DisplayName = "Tax Category")]
            [PXString(10, IsUnicode = true)]
            public virtual string TaxCategoryID
            {
                get { return this._TaxCategoryID; }
                set { this._TaxCategoryID = value; }
            }
            #endregion
            #region PostClassID
            public abstract class postClassID : PX.Data.BQL.BqlString.Field<postClassID> { }

            protected string _PostClassID;
            [PXString(10, IsUnicode = true)]
            [PXFormula(typeof(Selector<NonInventoryItem.itemClassID, INItemClass.postClassID>))]
            [PXUIField(DisplayName = "Posting Class")]
            [PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
            public virtual string PostClassID
            {
                get { return this._PostClassID; }
                set { this._PostClassID = value; }
            }
            #endregion
            #region LotSerClassID
            public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

            protected string _LotSerClassID;
            [PXString(10, IsUnicode = true)]
            [PXFormula(typeof(Selector<NonInventoryItem.itemClassID, INItemClass.lotSerClassID>))]
            [PXUIField(DisplayName = "Lot/Serial Class")]
            [PXSelector(typeof(INLotSerClass.lotSerClassID), DescriptionField = typeof(INLotSerClass.descr))]
            public virtual string LotSerClassID
            {
                get { return this._LotSerClassID; }
                set { this._LotSerClassID = value; }
            }
            #endregion
            #region StockItem
            public abstract class stockItem : PX.Data.BQL.BqlBool.Field<stockItem> { }

            protected bool? _StockItem;
            [PXBool]
            [PXUIField(DisplayName = "Stock Item", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
            [PXFormula(typeof(Selector<NonInventoryItem.itemClassID, INItemClass.stkItem>))]
            public virtual bool? StockItem
            {
                get { return _StockItem; }
                set { _StockItem = value; }
            }
            #endregion
            #region UnitCost
            public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

            protected Decimal? _UnitCost;
            [PXPriceCost]
            [PXUnboundDefault(TypeCode.Decimal, "0.00000")]
            [PXUIField(DisplayName = "Unit Cost")]
            public virtual Decimal? UnitCost
            {
                get
                {
                    return this._UnitCost;
                }
                set
                {
                    this._UnitCost = value;
                }
            }
            #endregion
            #region Image URL
            public abstract class imageURL : PX.Data.BQL.BqlString.Field<imageURL> { }

            protected String _ImageURL;
            [PXString(255)]
            public virtual String ImageURL
            {
                get
                {
                    return this._ImageURL;
                }
                set
                {
                    this._ImageURL = value;
                }
            }
            #endregion
            #region Body
            public abstract class body : PX.Data.BQL.BqlString.Field<body> { }

            protected String _Body;
            /// <summary>
            /// Rich text description of the item/estimate
            /// </summary>
            [PXString(IsUnicode = true)]
            [PXUIField(DisplayName = "Detail Description")]
            public virtual String Body
            {
                get
                {
                    return this._Body;
                }
                set
                {
                    this._Body = value;
                }
            }
            #endregion

            /// <summary>
            /// Non-Inventory Level attribute
            /// </summary>
            public class NonInventoryLevel
            {
                public const int Estimate = 1;
                public const int Material = 2;

                /// <summary>
                /// Description/labels for identifiers
                /// </summary>
                public class Desc
                {
                    public static string Estimate => Messages.GetLocal(Messages.Estimate);
                    public static string Material => Messages.GetLocal(Messages.Material);
                }

                public class estimate : PX.Data.BQL.BqlInt.Constant<estimate>
                {
                    public estimate() : base(Estimate) {; }
                }
                public class material : PX.Data.BQL.BqlInt.Constant<material>
                {
                    public material() : base(Material) {; }
                }

                public class ListAttribute : PXIntListAttribute
                {
                    public ListAttribute()
                        : base(
                            new int[] {
                                Estimate,
                                Material, },
                            new string[] {
                                Messages.Estimate,
                                Messages.Material })
                    { }
                }
            }
        }
    }
}