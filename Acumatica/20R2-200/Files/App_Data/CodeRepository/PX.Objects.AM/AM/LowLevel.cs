using System;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    public class LowLevel : PXGraph<LowLevel>
    {
        public PXSelect<InventoryItem> InventoryItemRecs;
        public PXSetup<AMBSetup> BomSetup;

        //Required as a workaround to AEF InventoryItemExt updates - Acumatica case 031594
        public PXSetup<INSetup> InvSetup;
        public PXSetup<CommonSetup> CSetup;
        
        /// <summary>
        /// Number of levels found
        /// </summary>
        public int CurrentMaxLowLevel;
        /// <summary>
        /// Was the process skipped (no boms changed from last run)
        /// </summary>
        public bool ProcessLevelsSkipped;
        public const int MaxLowLevel = 25;
        protected const int MaxNumberOfErrors = 50;
        private List<int> _updateErrorItemsList;
        private int _currentNumberOfErrors;

        /// <summary>
        /// Keeps track of all item low levels to call one DB update at the end of set all
        /// </summary>
        private Dictionary<int, int> _lowLevelDictionary;


        public LowLevel()
        {
            _updateErrorItemsList = new List<int>();
            _lowLevelDictionary = new Dictionary<int, int>();
            CurrentMaxLowLevel = 0;
            ProcessLevelsSkipped = false;

            InventoryItemRecs.AllowDelete = false;
            InventoryItemRecs.AllowInsert = false;
        }

        public static LowLevel Construct()
        {
            return CreateInstance<LowLevel>();
        }

        /// <summary>
        /// Determine if BOM data has changed. If not then no need to recalc low levels
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="fromDateTime">Date and time to check from for bom changes</param>
        /// <returns></returns>
        public static bool BomDataChanged(PXGraph graph, DateTime? fromDateTime)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (fromDateTime == null)
            {
                return true;
            }

            var bomItemAdded = (AMBomItem)PXSelect<AMBomItem,
                Where<AMBomItem.createdDateTime, GreaterEqual<Required<AMBomItem.createdDateTime>>>>
               .SelectWindowed(graph, 0, 1, fromDateTime);

            if (bomItemAdded?.BOMID != null)
            {
                return true;
            }
            // Joining AMBomItem as ECC reuses BOM Matl tables and changes to ECC should not impact low level logic
            var bomMatlAddedUpdated = (AMBomMatl)PXSelectJoin<
                    AMBomMatl,
                    InnerJoin<AMBomItem, On<AMBomMatl.bOMID, Equal<AMBomItem.bOMID>>>,
                    Where<AMBomMatl.lastModifiedDateTime, GreaterEqual<Required<AMBomMatl.lastModifiedDateTime>>>>
                .SelectWindowed(graph, 0, 1, fromDateTime);

            return bomMatlAddedUpdated?.BOMID != null;
        }

        #region Dictionary Methods

        protected void UpdateLowLevelDictionary(int? key, int? value)
        {
            if (key.GetValueOrDefault() == 0 || value.GetValueOrDefault() == 0)
            {
                return;
            }

            if (_lowLevelDictionary.ContainsKey(key.GetValueOrDefault()))
            {
                _lowLevelDictionary.Remove(key.GetValueOrDefault());
            }

            _lowLevelDictionary.Add(key.GetValueOrDefault(), value.GetValueOrDefault());
        }

        protected int GetLowLevelDictionaryValue(int? key)
        {
            return _lowLevelDictionary.TryGetValue(key.GetValueOrDefault(), out var lowLevelReturn) ? lowLevelReturn : 0;
        }
        #endregion

        protected void UpdateInventoryItem(InventoryItem row, bool persistRow = false)
        {
            if (row == null)
            {
                return;
            }

            var currentItemCd = row.InventoryCD ?? string.Empty;
            var currentItemId = row.InventoryID ?? 0;

            try
            {
                if (persistRow)
                {
                    InventoryItemRecs.Cache.Persist(row, PXDBOperation.Update);
                }
                else
                {
                    InventoryItemRecs.Cache.Update(row);
                }
            }
            catch (PXUnitConversionException unitException)
            {
                //These occur in the standard demo database and have nothing to do with this process.
                _currentNumberOfErrors++;

                if (currentItemId != 0 && !_updateErrorItemsList.Contains(currentItemId))
                {
                    _updateErrorItemsList.Add(currentItemId);

                    var msg = Messages.GetLocal(Messages.LowLevelUnableToUpdateItem, currentItemCd, unitException.Message);
                    PXTrace.WriteWarning(msg);
#if DEBUG
                    AMDebug.TraceWriteMethodName(msg);
#endif
                }
            }
            catch (PXOuterException e)
            {
                _currentNumberOfErrors++;

                PXTraceHelper.PxTraceOuterException(e, PXTraceHelper.ErrorLevel.Warning);
            }
            catch (Exception e)
            {
                _currentNumberOfErrors++;

                if(currentItemId != 0 && !_updateErrorItemsList.Contains(currentItemId))
                {
                    _updateErrorItemsList.Add(currentItemId);
                }

                if (string.IsNullOrWhiteSpace(currentItemCd))
                {
                    currentItemCd = $"ID:{currentItemId}";
                }

                var msg = Messages.GetLocal(Messages.LowLevelUnableToUpdateItem, currentItemCd, e.Message);
                PXTrace.WriteWarning(msg);
#if DEBUG
                AMDebug.TraceWriteMethodName(msg);
#endif
            }

            CheckForMaxErrorsReached();
        }

        protected virtual void CheckForMaxErrorsReached()
        {
            if (_currentNumberOfErrors >= MaxNumberOfErrors)
            {
                throw new PXException(Messages.LowLevelMaxErrorsReceived);
            }
        }

        protected virtual void PersistDictionary(bool persistEachRow)
        {
            foreach (InventoryItem inventoryItem in InventoryItemRecs.Select())
            {
                var inventoryItemExt = inventoryItem.GetExtension<InventoryItemExt>();

                if (inventoryItemExt != null)
                {
                    int? newLowLevel = GetLowLevelDictionaryValue(inventoryItem.InventoryID);

                    if (newLowLevel >= MaxLowLevel)
                    {
                        // to help in troubleshooting items related to circular reference
                        PXTrace.WriteInformation(Messages.GetLocal(Messages.LowLevelMaxLevelReachedForItem, inventoryItem.InventoryCD.TrimIfNotNullEmpty(), inventoryItemExt.AMLowLevel.GetValueOrDefault()));
                    }

                    //if the value did not change we do not need to update/persist
                    if (newLowLevel.GetValueOrDefault() != inventoryItemExt.AMLowLevel.GetValueOrDefault(-1))
                    {
                        inventoryItemExt.AMLowLevel = newLowLevel.GetValueOrDefault();
                        UpdateInventoryItem(inventoryItem, persistEachRow);
                    }
                }
            }

            if (IsDirty)
            {
                Persist();
            }
        }

        /// <summary>
        /// Persist with a retry for each row vs first attempt at mass update.
        /// This exists due to various customer item table error that exist before this process runs preventing the update from occuring.
        /// </summary>
        protected virtual void PersistDictionaryWithRetry()
        {
            // Try a mass persist and if that fails then we will try each row (each item to determine which item(s) are having issues).
            int retryCount = 1;
            for (int retry = 0; retry <= retryCount; retry++)
            {
                bool persistEachRow = retry != 0; 

                try
                {
                    PersistDictionary(persistEachRow);
                    retry = retryCount;
                }
                catch
                {
                    if (retry >= retryCount)
                    {
                        throw;
                    }

                    // Required to clear out the updated values and retry
                    InventoryItemRecs.Cache.Clear();
                    // Required due to extensions - the values are not corrected unless the query cache is cleared. 
                    //      Was keeping the previously updated values even though the above Clear was called.
                    InventoryItemRecs.Cache.ClearQueryCache();
                }
            }
        }

        /// <summary>
        /// Sets Low Level for all Inventory Id's
        /// </summary>
        public virtual void SetAll()
        {
            var lastLowLevelDateTime = BomSetup?.Current?.LastLowLevelCompletedDateTime;
            if (BomDataChanged(this, lastLowLevelDateTime))
            {
                ProcessAllLevels();
                ProcessLevelsSkipped = false;
                return;
            }
            ProcessLevelsSkipped = true;
            CurrentMaxLowLevel = BomSetup?.Current?.LastMaxLowLevel ?? 0;
            PXTrace.WriteInformation($"No bom changes found from {lastLowLevelDateTime}. Low level process skipped");
        }

        protected virtual void ResetAllLowLevels()
        {
            PXDatabase.Update<InventoryItem>(
                new PXDataFieldAssign<InventoryItemExt.aMLowLevel>(PXDbType.Int, 0));
        }

        protected virtual List<BomInventoryItem> GetInventory()
        {
            return PXSelect<BomInventoryItem>.Select(this).ToFirstTableList();
        }

        protected virtual void ProcessAllLevels()
        {
            _updateErrorItemsList = new List<int>();
            _lowLevelDictionary = new Dictionary<int, int>();

            CurrentMaxLowLevel = 0;
            var lowLevel = 0;
            var hasMoreLevels = true;
            _currentNumberOfErrors = 0;

            ResetAllLowLevels();

            var inventoryItemParents = GetInventory();

            if (inventoryItemParents == null || inventoryItemParents.Count == 0)
            {
                return;
            }

            while (hasMoreLevels)
            {
                foreach (var inventoryItemParent in inventoryItemParents)
                {
                    int currentlevel = GetLowLevelDictionaryValue(inventoryItemParent.InventoryID);
                    if (currentlevel >= MaxLowLevel)
                    {
                        continue;
                    }

                    //Exclude check for active from Query to avoid need for Index. Non Active boms are not common.
                    foreach (PXResult<LowLevelBomMatl, LowLevelBomItem> result in PXSelectReadonly2<LowLevelBomMatl,
                            InnerJoin<LowLevelBomItem, On<LowLevelBomMatl.bOMID, Equal<LowLevelBomItem.bOMID>>>,
                            Where<LowLevelBomItem.inventoryID, Equal<Required<LowLevelBomItem.inventoryID>>>>.Select(this, inventoryItemParent.InventoryID))
                    {
                        var amBomMatl = (LowLevelBomMatl)result;
                        var amBomItem = (LowLevelBomItem)result;

                        if (string.IsNullOrWhiteSpace(amBomMatl?.BOMID) ||
                            string.IsNullOrWhiteSpace(amBomItem?.BOMID))
                        {
                            continue;
                        }

                        var childLowLevel = GetLowLevelDictionaryValue(amBomMatl.InventoryID);
                        if (childLowLevel <= currentlevel)
                        {
                            childLowLevel = currentlevel + 1;

                            if (childLowLevel > CurrentMaxLowLevel)
                            {
                                CurrentMaxLowLevel = childLowLevel;
                            }

                            UpdateLowLevelDictionary(amBomMatl.InventoryID, childLowLevel);
                        }
                    }
                }

                lowLevel++;

                if (lowLevel > CurrentMaxLowLevel || lowLevel >= MaxLowLevel)
                {
                    //Either no more levels to process or the max has been reached - done being loopy
                    hasMoreLevels = false;
                }
            }

            PersistDictionaryWithRetry();

            if (CurrentMaxLowLevel >= MaxLowLevel)
            {
                PXTrace.WriteError(Messages.GetLocal(Messages.LowLevelMaxLevelReached, MaxLowLevel));
            }

            UpdateBomSetup();

            Clear();
        }

        protected virtual void UpdateBomSetup()
        {
            if (BomSetup?.Current == null)
            {
                BomSetup?.Select();
            }

            var setup = BomSetup?.Current;
            if (setup == null)
            {
                return;
            }

            setup.LastLowLevelCompletedDateTime = Common.Dates.Now;
            setup.LastMaxLowLevel = CurrentMaxLowLevel;

            BomSetup.Cache.PersistUpdated(setup);
        }

        [PXProjection(typeof(Select<AMBomMatl>), Persistent = false)]
        [Serializable]
        [PXHidden]
        public class LowLevelBomMatl : IBqlTable
        {
            #region BOMID
            public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
            [BomID(BqlField = typeof(AMBomMatl.bOMID))]
            public virtual String BOMID { get; set; }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            [PXDBInt(BqlField = typeof(AMBomMatl.inventoryID))]
            [PXUIField(DisplayName = "Inventory ID")]
            public virtual Int32? InventoryID { get; set; }
            #endregion
        }

        [PXProjection(typeof(Select<AMBomItem>), Persistent = false)]
        [Serializable]
        [PXHidden]
        public class LowLevelBomItem : IBqlTable
        {
            #region BOMID
            public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
            [BomID(BqlField = typeof(AMBomItem.bOMID))]
            public virtual String BOMID { get; set; }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            [PXDBInt(BqlField = typeof(AMBomItem.inventoryID))]
            [PXUIField(DisplayName = "Inventory ID")]
            public virtual Int32? InventoryID { get; set; }
            #endregion
        }
    }
}
