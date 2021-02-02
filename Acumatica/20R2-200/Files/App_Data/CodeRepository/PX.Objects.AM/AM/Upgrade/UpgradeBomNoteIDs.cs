using System;
using System.Collections.Generic;
using Customization;
using PX.Objects.AM.Reports;
using PX.Common;
using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// In older versions of Manufacturing the noteids were not correctly reset when copying boms. As a result the noteids are repeated a lot and we are using NoteID for the
    /// BOM Compare process which could result in a lot of funky results.
    /// </summary>
    internal sealed class UpgradeBomNoteIDs : UpgradeProcessVersionBase
    {
        public UpgradeBomNoteIDs(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public UpgradeBomNoteIDs(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public override int Version => UpgradeVersions.Version2019R1Ver03;

        public override void ProcessTables()
        {
            // We only need to make sure NoteID is unique per table - not entire database
            ProcessBomItem();
            ProcessOperation();
            ProcessMaterial();
            ProcessSteps();
            ProcessTools();
            ProcessOverhead();
        }

        private TRow SetNoteID<TRow>(TRow row)
            where TRow : class, IBqlTable, INotable, new()
        {
            if (row == null)
            {
                return null;
            }

            _upgradeGraph.Caches<TRow>().SetDefaultExt(row, nameof(row.NoteID));
            return (TRow)_upgradeGraph.Caches<TRow>().Update(row);
        }

        private void FixNoteID<TRow>(TRow row, Note note)
            where TRow : class, IBqlTable, INotable, new()
        {
            if (row == null)
            {
                return;
            }

            var origRow = PXCache<TRow>.CreateCopy(row);
            var updatedRow = SetNoteID(PXCache<TRow>.CreateCopy(row));
            if (updatedRow?.NoteID == null || note?.NoteID == null)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"{typeof(TRow).Name}: From NoteID = {origRow?.NoteID}; To NoteID = {updatedRow?.NoteID}");
#endif
            // Here we know we have a note record which means the user must have an entered comment or a attached file(s) - so we need to copy those
            PXNoteAttribute.CopyNoteAndFiles(_upgradeGraph.Caches<TRow>(), origRow, _upgradeGraph.Caches<TRow>(), updatedRow, true, true);
            _upgradeGraph.Caches<TRow>().Update(updatedRow);
        }

        private void ProcessBomRow<TRow>(TRow row, Note note, ref HashSet<Guid> noteHash)
            where TRow : class, IBqlTable, INotable, new()
        {
            if (row == null)
            {
                return;
            }

            if (row.NoteID == null)
            {
                SetNoteID(PXCache<TRow>.CreateCopy(row));
                return;
            }

            if (noteHash.Add(row.NoteID.GetValueOrDefault()))
            {
                //First noteid gets to keep its noteid
                return;
            }

            FixNoteID(row, note);
        }

        private void ProcessBomItem()
        {
            var noteHash = new HashSet<Guid>();
            var failedCounter = 0;

            using (new DisableSelectorValidationScope(_upgradeGraph.BomItem.Cache))
            {
                var foundNull = false;
                foreach (var aggregate in PXSelectGroupBy<AMBomItemNoteUpgrade, Aggregate<GroupBy<AMBomItemNoteUpgrade.noteID, Count<AMBomItemNoteUpgrade.tstamp>>>>.Select(_upgradeGraph))
                {
                    var row = (AMBomItemNoteUpgrade)aggregate;
                    if (row == null)
                    {
                        continue;
                    }

                    foundNull |= row.NoteID == null;
                    if (aggregate.RowCount <= 1 || row.NoteID == null)
                    {
                        //Not a duplicate
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Duplicate NoteID {row.NoteID} repeats {aggregate.RowCount}");
#endif

                    // Sub query ok because amount of duplicates should be low. No other good way to find and query only the duplicates

                    //  Duplicate NoteIDs...
                    foreach (PXResult<AMBomItem, Note> result in PXSelectJoin<
                        AMBomItem,
                        LeftJoin<Note,
                            On<AMBomItem.noteID, Equal<Note.noteID>>>,
                        Where<AMBomItem.noteID, Equal<Required<AMBomItem.noteID>>>>
                        .Select(_upgradeGraph, row.NoteID))
                    {
                        try
                        {
                            ProcessBomRow<AMBomItem>(result, result, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomItem.noteID>(), e.Message));
                        }
                    }
                }

                if (foundNull)
                {
                    // In case any NoteID is null...
                    foreach (AMBomItem bomRow in PXSelect<
                            AMBomItem,
                            Where<AMBomItem.noteID, IsNull>>
                        .Select(_upgradeGraph))
                    {
                        try
                        {
                            ProcessBomRow(bomRow, null, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomItem.noteID>(), e.Message));
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                var updated = _upgradeGraph.BomItem.Cache.Updated.Count();
                if (updated > 0)
                {
                    sb.AppendLine($"Updated {updated} {Common.Cache.GetCacheName(typeof(AMBomItem))} record notes.");
                }
                if (failedCounter > 0)
                {
                    sb.AppendLine($"Failed to update {failedCounter} {Common.Cache.GetCacheName(typeof(AMBomItem))} records.");
                }

                if (sb.Length > 0)
                {
                    WriteInfo(sb.ToString());
                }
            }
        }

        private void ProcessOperation()
        {
            var noteHash = new HashSet<Guid>();
            var failedCounter = 0;

            using (new DisableSelectorValidationScope(_upgradeGraph.BomOper.Cache))
            {
                var foundNull = false;
                foreach (var aggregate in PXSelectGroupBy<AMBomOperNoteUpgrade, Aggregate<GroupBy<AMBomOperNoteUpgrade.noteID, Count<AMBomOperNoteUpgrade.tstamp>>>>.Select(_upgradeGraph))
                {
                    var row = (AMBomOperNoteUpgrade)aggregate;
                    if (row == null)
                    {
                        continue;
                    }

                    foundNull |= row.NoteID == null;
                    if (aggregate.RowCount <= 1 || row.NoteID == null)
                    {
                        //Not a duplicate
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Duplicate NoteID {row.NoteID} repeats {aggregate.RowCount}");
#endif

                    // Sub query ok because amount of duplicates should be low. No other good way to find and query only the duplicates

                    //  Duplicate NoteIDs...
                    foreach (PXResult<AMBomOper, Note> result in PXSelectJoin<
                        AMBomOper,
                        LeftJoin<Note,
                            On<AMBomOper.noteID, Equal<Note.noteID>>>,
                        Where<AMBomOper.noteID, Equal<Required<AMBomOper.noteID>>>>
                        .Select(_upgradeGraph, row.NoteID))
                    {
                        try
                        {
                            ProcessBomRow<AMBomOper>(result, result, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomOper.noteID>(), e.Message));
                        }
                    }

                }

                if (foundNull)
                {
                    // In case any NoteID is null...
                    foreach (AMBomOper bomRow in PXSelect<
                            AMBomOper,
                            Where<AMBomOper.noteID, IsNull>>
                        .Select(_upgradeGraph))
                    {
                        try
                        {
                            ProcessBomRow(bomRow, null, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomOper.noteID>(), e.Message));
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                var updated = _upgradeGraph.BomOper.Cache.Updated.Count();
                if (updated > 0)
                {
                    sb.AppendLine($"Updated {updated} {Common.Cache.GetCacheName(typeof(AMBomOper))} record notes.");
                }
                if (failedCounter > 0)
                {
                    sb.AppendLine($"Failed to update {failedCounter} {Common.Cache.GetCacheName(typeof(AMBomOper))} records.");
                }

                if (sb.Length > 0)
                {
                    WriteInfo(sb.ToString());
                }
            }
        }

        private void ProcessMaterial()
        {
            var noteHash = new HashSet<Guid>();
            var failedCounter = 0;

            using (new DisableSelectorValidationScope(_upgradeGraph.BomMatl.Cache))
            using (new DisableFormulaCalculationScope(_upgradeGraph.BomMatl.Cache, typeof(AMBomMatl.isStockItem))) // Avoids sub query to InventoryItem
            {
                var foundNull = false;
                foreach (var aggregate in PXSelectGroupBy<AMBomMatlNoteUpgrade, Aggregate<GroupBy<AMBomMatlNoteUpgrade.noteID, Count<AMBomMatlNoteUpgrade.tstamp>>>>.Select(_upgradeGraph))
                {
                    var row = (AMBomMatlNoteUpgrade) aggregate;
                    if (row == null)
                    {
                        continue;
                    }

                    foundNull |= row.NoteID == null;
                    if (aggregate.RowCount <= 1 || row.NoteID == null)
                    {
                        //Not a duplicate
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Duplicate NoteID {row.NoteID} repeats {aggregate.RowCount}");
#endif

                    // Sub query ok because amount of duplicates should be low. No other good way to find and query only the duplicates

                    //  Duplicate NoteIDs...
                    foreach (PXResult<AMBomMatl, Note> result in PXSelectJoin<
                        AMBomMatl,
                        LeftJoin<Note,
                            On<AMBomMatl.noteID, Equal<Note.noteID>>>,
                        Where<AMBomMatl.noteID, Equal<Required<AMBomMatl.noteID>>>>
                        .Select(_upgradeGraph, row.NoteID))
                    {
                        try
                        {
                            ProcessBomRow<AMBomMatl>(result, result, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomMatl.noteID>(), e.Message));
                        }
                    }

                }

                if (foundNull)
                {
                    // In case any NoteID is null...
                    foreach (AMBomMatl bomRow in PXSelect<
                            AMBomMatl,
                            Where<AMBomMatl.noteID, IsNull>>
                        .Select(_upgradeGraph))
                    {
                        try
                        {
                            ProcessBomRow(bomRow, null, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomMatl.noteID>(), e.Message));
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                var updated = _upgradeGraph.BomMatl.Cache.Updated.Count();
                if (updated > 0)
                {
                    sb.AppendLine($"Updated {updated} {Common.Cache.GetCacheName(typeof(AMBomMatl))} record notes.");
                }
                if (failedCounter > 0)
                {
                    sb.AppendLine($"Failed to update {failedCounter} {Common.Cache.GetCacheName(typeof(AMBomMatl))} records.");
                }

                if (sb.Length > 0)
                {
                    WriteInfo(sb.ToString());
                }
            }
        }

        private void ProcessSteps()
        {
            var noteHash = new HashSet<Guid>();
            var failedCounter = 0;

            using (new DisableSelectorValidationScope(_upgradeGraph.BomStep.Cache))
            {
                var foundNull = false;
                foreach (var aggregate in PXSelectGroupBy<AMBomStepNoteUpgrade, Aggregate<GroupBy<AMBomStepNoteUpgrade.noteID, Count<AMBomStepNoteUpgrade.tstamp>>>>.Select(_upgradeGraph))
                {
                    var row = (AMBomStepNoteUpgrade)aggregate;
                    if (row == null)
                    {
                        continue;
                    }

                    foundNull |= row.NoteID == null;
                    if (aggregate.RowCount <= 1 || row.NoteID == null)
                    {
                        //Not a duplicate
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Duplicate NoteID {row.NoteID} repeats {aggregate.RowCount}");
#endif

                    // Sub query ok because amount of duplicates should be low. No other good way to find and query only the duplicates

                    //  Duplicate NoteIDs...
                    foreach (PXResult<AMBomStep, Note> result in PXSelectJoin<
                        AMBomStep,
                        LeftJoin<Note,
                            On<AMBomStep.noteID, Equal<Note.noteID>>>,
                        Where<AMBomStep.noteID, Equal<Required<AMBomStep.noteID>>>>
                        .Select(_upgradeGraph, row.NoteID))
                    {
                        try
                        {
                            ProcessBomRow<AMBomStep>(result, result, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomStep.noteID>(), e.Message));
                        }
                    }
                }

                if (foundNull)
                {
                    // In case any NoteID is null...
                    foreach (AMBomStep bomRow in PXSelect<
                            AMBomStep,
                            Where<AMBomStep.noteID, IsNull>>
                        .Select(_upgradeGraph))
                    {
                        try
                        {
                            ProcessBomRow(bomRow, null, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomStep.noteID>(), e.Message));
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                var updated = _upgradeGraph.BomStep.Cache.Updated.Count();
                if (updated > 0)
                {
                    sb.AppendLine($"Updated {updated} {Common.Cache.GetCacheName(typeof(AMBomStep))} record notes.");
                }
                if (failedCounter > 0)
                {
                    sb.AppendLine($"Failed to update {failedCounter} {Common.Cache.GetCacheName(typeof(AMBomStep))} records.");
                }

                if (sb.Length > 0)
                {
                    WriteInfo(sb.ToString());
                }
            }
        }

        private void ProcessTools()
        {
            var noteHash = new HashSet<Guid>();
            var failedCounter = 0;

            using (new DisableSelectorValidationScope(_upgradeGraph.BomTool.Cache))
            {
                var foundNull = false;
                foreach (var aggregate in PXSelectGroupBy<AMBomToolNoteUpgrade, Aggregate<GroupBy<AMBomToolNoteUpgrade.noteID, Count<AMBomToolNoteUpgrade.tstamp>>>>.Select(_upgradeGraph))
                {
                    var row = (AMBomToolNoteUpgrade)aggregate;
                    if (row == null)
                    {
                        continue;
                    }

                    foundNull |= row.NoteID == null;
                    if (aggregate.RowCount <= 1 || row.NoteID == null)
                    {
                        //Not a duplicate
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Duplicate NoteID {row.NoteID} repeats {aggregate.RowCount}");
#endif

                    // Sub query ok because amount of duplicates should be low. No other good way to find and query only the duplicates

                    //  Duplicate NoteIDs...
                    foreach (PXResult<AMBomTool, Note> result in PXSelectJoin<
                        AMBomTool,
                        LeftJoin<Note,
                            On<AMBomTool.noteID, Equal<Note.noteID>>>,
                        Where<AMBomTool.noteID, Equal<Required<AMBomTool.noteID>>>>
                        .Select(_upgradeGraph, row.NoteID))
                    {
                        try
                        {
                            ProcessBomRow<AMBomTool>(result, result, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomTool.noteID>(), e.Message));
                        }
                    }
                }

                if (foundNull)
                {
                    // In case any NoteID is null...
                    foreach (AMBomTool bomRow in PXSelect<
                            AMBomTool,
                            Where<AMBomTool.noteID, IsNull>>
                        .Select(_upgradeGraph))
                    {
                        try
                        {
                            ProcessBomRow(bomRow, null, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomTool.noteID>(), e.Message));
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                var updated = _upgradeGraph.BomTool.Cache.Updated.Count();
                if (updated > 0)
                {
                    sb.AppendLine($"Updated {updated} {Common.Cache.GetCacheName(typeof(AMBomTool))} record notes.");
                }
                if (failedCounter > 0)
                {
                    sb.AppendLine($"Failed to update {failedCounter} {Common.Cache.GetCacheName(typeof(AMBomTool))} records.");
                }

                if (sb.Length > 0)
                {
                    WriteInfo(sb.ToString());
                }
            }
        }

        private void ProcessOverhead()
        {
            var noteHash = new HashSet<Guid>();
            var failedCounter = 0;

            using (new DisableSelectorValidationScope(_upgradeGraph.BomOvhd.Cache))
            {
                var foundNull = false;
                foreach (var aggregate in PXSelectGroupBy<AMBomOvhdNoteUpgrade, Aggregate<GroupBy<AMBomOvhdNoteUpgrade.noteID, Count<AMBomOvhdNoteUpgrade.tstamp>>>>.Select(_upgradeGraph))
                {
                    var row = (AMBomOvhdNoteUpgrade)aggregate;
                    if (row == null)
                    {
                        continue;
                    }

                    foundNull |= row.NoteID == null;
                    if (aggregate.RowCount <= 1 || row.NoteID == null)
                    {
                        //Not a duplicate
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Duplicate NoteID {row.NoteID} repeats {aggregate.RowCount}");
#endif

                    // Sub query ok because amount of duplicates should be low. No other good way to find and query only the duplicates

                    //  Duplicate NoteIDs...
                    foreach (PXResult<AMBomOvhd, Note> result in PXSelectJoin<
                        AMBomOvhd,
                        LeftJoin<Note,
                            On<AMBomOvhd.noteID, Equal<Note.noteID>>>,
                        Where<AMBomOvhd.noteID, Equal<Required<AMBomOvhd.noteID>>>>
                        .Select(_upgradeGraph, row.NoteID))
                    {
                        try
                        {
                            ProcessBomRow<AMBomOvhd>(result, result, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomOvhd.noteID>(), e.Message));
                        }
                    }
                }

                if (foundNull)
                {
                    // In case any NoteID is null...
                    foreach (AMBomOvhd bomRow in PXSelect<
                            AMBomOvhd,
                            Where<AMBomOvhd.noteID, IsNull>>
                        .Select(_upgradeGraph))
                    {
                        try
                        {
                            ProcessBomRow(bomRow, null, ref noteHash);
                        }
                        catch (Exception e)
                        {
                            failedCounter++;
                            PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToUpdateField, ReportHelper.GetDacFieldNameString<AMBomOvhd.noteID>(), e.Message));
                        }
                    }
                }

                var sb = new System.Text.StringBuilder();
                var updated = _upgradeGraph.BomOvhd.Cache.Updated.Count();
                if (updated > 0)
                {
                    sb.AppendLine($"Updated {updated} {Common.Cache.GetCacheName(typeof(AMBomOvhd))} record notes.");
                }
                if (failedCounter > 0)
                {
                    sb.AppendLine($"Failed to update {failedCounter} {Common.Cache.GetCacheName(typeof(AMBomOvhd))} records.");
                }

                if (sb.Length > 0)
                {
                    WriteInfo(sb.ToString());
                }
            }
        }
    }

    /// <summary>
    /// Used for performance to exclude all non necessary fields
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMBomItem>), Persistent = false)]
    public class AMBomItemNoteUpgrade : IBqlTable, INotable
    {
        #region NoteID

        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
        {
        }

        protected Guid? _NoteID;

        [PXDBGuid(BqlField = typeof(AMBomItem.noteID))]
        public virtual Guid? NoteID
        {
            get { return this._NoteID; }
            set { this._NoteID = value; }
        }

        #endregion
        #region tstamp

        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp>
        {
        }

        protected Byte[] _tstamp;

        [PXDBTimestamp(BqlField = typeof(AMBomItem.Tstamp))]
        public virtual Byte[] Tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
    }

    /// <summary>
    /// Used for performance to exclude all non necessary fields
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMBomOper>), Persistent = false)]
    public class AMBomOperNoteUpgrade : IBqlTable, INotable
    {
        #region NoteID

        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
        {
        }

        protected Guid? _NoteID;

        [PXDBGuid(BqlField = typeof(AMBomOper.noteID))]
        public virtual Guid? NoteID
        {
            get { return this._NoteID; }
            set { this._NoteID = value; }
        }

        #endregion
        #region tstamp

        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp>
        {
        }

        protected Byte[] _tstamp;

        [PXDBTimestamp(BqlField = typeof(AMBomOper.Tstamp))]
        public virtual Byte[] Tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
    }

    /// <summary>
    /// Used for performance to exclude all non necessary fields
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMBomMatl>), Persistent = false)]
    public class AMBomMatlNoteUpgrade : IBqlTable, INotable
    {
        #region NoteID

        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
        {
        }

        protected Guid? _NoteID;

        [PXDBGuid(BqlField = typeof(AMBomMatl.noteID))]
        public virtual Guid? NoteID
        {
            get { return this._NoteID; }
            set { this._NoteID = value; }
        }

        #endregion
        #region tstamp

        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp>
        {
        }

        protected Byte[] _tstamp;

        [PXDBTimestamp(BqlField = typeof(AMBomMatl.Tstamp))]
        public virtual Byte[] Tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
    }

    /// <summary>
    /// Used for performance to exclude all non necessary fields
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMBomStep>), Persistent = false)]
    public class AMBomStepNoteUpgrade : IBqlTable, INotable
    {
        #region NoteID

        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
        {
        }

        protected Guid? _NoteID;

        [PXDBGuid(BqlField = typeof(AMBomStep.noteID))]
        public virtual Guid? NoteID
        {
            get { return this._NoteID; }
            set { this._NoteID = value; }
        }

        #endregion
        #region tstamp

        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp>
        {
        }

        protected Byte[] _tstamp;

        [PXDBTimestamp(BqlField = typeof(AMBomStep.Tstamp))]
        public virtual Byte[] Tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
    }

    /// <summary>
    /// Used for performance to exclude all non necessary fields
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMBomTool>), Persistent = false)]
    public class AMBomToolNoteUpgrade : IBqlTable, INotable
    {
        #region NoteID

        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
        {
        }

        protected Guid? _NoteID;

        [PXDBGuid(BqlField = typeof(AMBomTool.noteID))]
        public virtual Guid? NoteID
        {
            get { return this._NoteID; }
            set { this._NoteID = value; }
        }

        #endregion
        #region tstamp

        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp>
        {
        }

        protected Byte[] _tstamp;

        [PXDBTimestamp(BqlField = typeof(AMBomTool.Tstamp))]
        public virtual Byte[] Tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
    }

    /// <summary>
    /// Used for performance to exclude all non necessary fields
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMBomOvhd>), Persistent = false)]
    public class AMBomOvhdNoteUpgrade : IBqlTable, INotable
    {
        #region NoteID

        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
        {
        }

        protected Guid? _NoteID;

        [PXDBGuid(BqlField = typeof(AMBomOvhd.noteID))]
        public virtual Guid? NoteID
        {
            get { return this._NoteID; }
            set { this._NoteID = value; }
        }

        #endregion
        #region tstamp

        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp>
        {
        }

        protected Byte[] _tstamp;

        [PXDBTimestamp(BqlField = typeof(AMBomOvhd.Tstamp))]
        public virtual Byte[] Tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
    }
}