using System;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Copying estimates records per revision and/or estimate
    /// </summary>
    public class EstimateCopy : PXGraph<EstimateCopy, AMEstimateItem>
    {
        public PXSelect<AMEstimateItem> Documents;

        public PXSetup<AMEstimateSetup> EstimateSetup;

        public PXSelect<AMEstimateOper, Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
                And<AMEstimateOper.revisionID, Equal<Current<AMEstimateItem.revisionID>>>>,
                OrderBy<Asc<AMEstimateOper.operationCD>>> EstimateOperRecords;

        [PXHidden]
        public PXSelect<AMEstimateReference,
            Where<AMEstimateReference.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
                And<AMEstimateReference.revisionID, Equal<Current<AMEstimateItem.revisionID>>>>> EstimateReferenceRecord;

        [PXHidden]
        public PXSelect<AMEstimateHistory, Where<AMEstimateHistory.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
                And<AMEstimateHistory.revisionID, Equal<Current<AMEstimateItem.revisionID>>>>> EstimateHistoryRecords;

        [PXHidden]
        public PXSelect<AMEstimateMatl, Where<AMEstimateMatl.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateMatl.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateMatl.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> EstimateOperMatlRecords;

        [PXHidden]
        public PXSelect<AMEstimateTool, Where<AMEstimateTool.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateTool.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateTool.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> EstimateOperToolRecords;

        [PXHidden]
        public PXSelectJoin<AMEstimateOvhd, InnerJoin<AMOverhead, On<AMEstimateOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
            Where<AMEstimateOvhd.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateOvhd.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateOvhd.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> EstimateOperOvhdRecords;

        [PXHidden]
        public PXSelect<Numbering, Where<Numbering.numberingID, Equal<Current<AMEstimateSetup.estimateNumberingID>>>> EstimateNumbering;

        //Required for the BAccount selector on AMEstimateReference to work
        [PXHidden]
        public PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<AMEstimateReference.bAccountID>>>> CurrentBAccount;

        [PXHidden]
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<AMEstimateItem.curyInfoID>>>> currencyinfo;

        public EstimateCopy()
        {
            var estSetup = EstimateSetup?.Current;
            if (estSetup == null)
            {
                throw new EstimatingSetupNotEnteredException();
            }
        }

        public override void Persist()
        {
            if (Documents.Current != null
                && Documents.Cache.GetStatus(Documents.Current) == PXEntryStatus.Inserted)
            {
                EstimateHistoryRecords.Insert(new AMEstimateHistory
                {
                    RevisionID = Documents.Current.RevisionID,
                    Description = Messages.GetLocal(Messages.EstimateRevisionCreated, Documents.Current.RevisionID.TrimIfNotNullEmpty())
                });
            }

            try
            {
                base.Persist();
            }
            catch (Exception exception)
            {
                PXTraceHelper.PxTraceException(exception);
                throw;
            }
        }

        [EstimateID(IsKey = true)]
        [EstimateIDSelectAll(typeof(Search<AMEstimateItem.estimateID>), ValidateValue = false)]
        [PXDefault]
        protected virtual void AMEstimateItem_EstimateID_CacheAttached(PXCache cache)
        {
        }


        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC", IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void AMEstimateItem_RevisionID_CacheAttached(PXCache cache)
        {
        }

        [PXDBDefault(typeof(AMEstimateItem.estimateID))]
        [EstimateID(IsKey = true)]
        protected virtual void AMEstimateOper_EstimateID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void InsertListToCache<TDac>(List<TDac> list) where TDac : IBqlTable
        {
            if (list == null || list.Count == 0)
            {
                return;
            }

            foreach (var row in list)
            {
                this.Caches[typeof(TDac)].Insert(row);
            }
        }

        public virtual Numbering CurrentEstimateNumbering
        {
            get { return EstimateNumbering.Current ?? (EstimateNumbering.Current = EstimateNumbering.Select()); }
        }

        public virtual string EstimateNewNumberSymbol
        {
            get { return CurrentEstimateNumbering == null ? string.Empty : CurrentEstimateNumbering.NewSymbol; }
        }

        public virtual bool IsValidEstimateID(AMEstimateItem estimateItem)
        {
            return estimateItem != null && IsValidEstimateID(estimateItem.EstimateID);
        }

        public virtual bool IsValidEstimateID(string estimateID)
        {
            return !string.IsNullOrWhiteSpace(estimateID) && !estimateID.EqualsWithTrim(EstimateNewNumberSymbol);
        }

        public static AMEstimateItem CreatePersistNewRevision(AMEstimateItem sourceEstimate, string newRevisionID)
        {
            EstimateCopy copyGraph = CreateInstance<EstimateCopy>();
            var newEstimateItem = copyGraph.CreateNewRevision(sourceEstimate, newRevisionID);
            copyGraph.Persist();
            return copyGraph.Documents.Current ?? newEstimateItem;
        }

        public virtual AMEstimateItem CreateNewRevision(AMEstimateItem sourceEstimate, string newRevisionID)
        {
            return CreateNewRevision(sourceEstimate, CopyAsNewEstimateItem(sourceEstimate, newRevisionID, sourceEstimate.EstimateID, true));
        }

        public virtual AMEstimateItem CreateNewRevision(AMEstimateItem sourceEstimate, AMEstimateItem newEstimateItem)
        {
            this.Clear();

            if (newEstimateItem == null)
            {
                return null;
            }
            
            newEstimateItem = PXCache<AMEstimateItem>.CreateCopy(Documents.Insert(newEstimateItem));

            CreateNewEstimateReferenceRevision(sourceEstimate, newEstimateItem);

            CopyAllEstimateChildren(sourceEstimate, newEstimateItem, true);

            return Documents.Current ?? newEstimateItem;
        }


        protected virtual string GetNextEstimateNumber()
        {
            return AutoNumberAttribute.GetNextNumber(Documents.Cache, null, CurrentEstimateNumbering.NumberingID, Accessinfo.BusinessDate) ?? string.Empty;
        }

        /// <summary>
        /// Create a new estimate from the source estimate
        /// </summary>
        /// <param name="sourceEstimate">Source estimate to copy and create a new estimate</param>
        /// <returns>new/ estimate item record</returns>
        public virtual AMEstimateItem CreateNewEstimate(AMEstimateItem sourceEstimate)
        {
            return CreateNewEstimate(sourceEstimate, GetNextEstimateNumber());
        }

        /// <summary>
        /// Create a new estimate from the source estimate
        /// </summary>
        /// <param name="sourceEstimate">Source estimate to copy and create a new estimate</param>
        /// <param name="newEstimateID">new Estimate ID defined from the calling program</param>
        /// <returns>new/estimate item record</returns>
        public virtual AMEstimateItem CreateNewEstimate(AMEstimateItem sourceEstimate, string newEstimateID)
        {
            return CreateNewEstimate(sourceEstimate, null, newEstimateID);
        }

        /// <summary>
        /// Create a new estimate from the source estimate
        /// </summary>
        /// <param name="sourceEstimate">Source estimate to copy and create a new estimate</param>
        /// <param name="newEstimateReference">pre-defined estimate reference record for the new estimate</param>
        /// <returns>new/estimate item record</returns>
        public virtual AMEstimateItem CreateNewEstimate(AMEstimateItem sourceEstimate, AMEstimateReference newEstimateReference)
        {
            return CreateNewEstimate(sourceEstimate, newEstimateReference, GetNextEstimateNumber());
        }

        /// <summary>
        /// Create a new estimate from the source estimate
        /// </summary>
        /// <param name="sourceEstimate">Source estimate to copy and create a new estimate</param>
        /// <param name="newEstimateReference">pre-defined estimate reference record for the new estimate</param>
        /// <param name="newEstimateID">new Estimate ID defined from the calling program</param>
        /// <returns>new/estimate item record</returns>
        public virtual AMEstimateItem CreateNewEstimate(AMEstimateItem sourceEstimate, AMEstimateReference newEstimateReference, string newEstimateID)
        {
            this.Clear();

            EstimateSetup.Current.CopyEstimateFiles = false;
            EstimateSetup.Current.CopyEstimateNotes = false;
            EstimateSetup.Current.CopyOperationFiles = false;
            EstimateSetup.Current.CopyOperationNotes = false; 

            string revisionID = string.IsNullOrWhiteSpace(EstimateSetup.Current?.DefaultRevisionID)
                ? "01"
                : EstimateSetup.Current.DefaultRevisionID.TrimIfNotNullEmpty();

            var newEstimateItem = CopyAsNewEstimateItem(sourceEstimate, revisionID, newEstimateID, true);
            if (newEstimateItem == null)
            {
                return null;
            }
            newEstimateItem.PrimaryRevisionID = null;
            newEstimateItem.EstimateStatus = null;
            newEstimateItem.LineCntrHistory = null;

            newEstimateItem = PXCache<AMEstimateItem>.CreateCopy(Documents.Insert(newEstimateItem));
            if (Documents.Current == null)
            {
                Documents.Current = newEstimateItem;
            }

            var estimateReference = CopyAsNewEstimateReference(sourceEstimate, newEstimateItem);
            estimateReference = EstimateGraphHelper.AddEstimateReference(newEstimateReference, estimateReference);
            EstimateReferenceRecord.Insert(estimateReference);

            CopyAllEstimateChildren(sourceEstimate, newEstimateItem, true);

            return Documents.Current ?? newEstimateItem;
        }

        protected virtual void CopyAllEstimateChildren(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            CopyAllEstimateChildren(fromEstimateItem, toEstimateItem, false);
        }

        protected virtual void CopyAllEstimateChildren(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem, bool resetNoteID)
        {
            InsertListToCache(CopyAsNewOperations(fromEstimateItem, toEstimateItem, resetNoteID));
            InsertListToCache(CopyAsNewMaterial(fromEstimateItem, toEstimateItem));
            InsertListToCache(CopyAsNewTool(fromEstimateItem, toEstimateItem));
            InsertListToCache(CopyAsNewOverhead(fromEstimateItem, toEstimateItem));
        }

        /// <summary>
        /// Copy the estimate item from the from estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <returns>new to/destination estimate item</returns>
        public virtual AMEstimateItem CopyAsNewEstimateItem(AMEstimateItem fromEstimateItem, string newRevisionID, string newEstimateID, bool resetNoteID)
        {
            if (fromEstimateItem == null)
            {
                return null;
            }

            var newEstimateItem = AMEstimateItem.ResetChildCalcValues(PXCache<AMEstimateItem>.CreateCopy(fromEstimateItem));
            newEstimateItem.NoteID = null;
            newEstimateItem.tstamp = null;
            newEstimateItem.RevisionID = newRevisionID;
            newEstimateItem.RevisionDate = this.Accessinfo.BusinessDate.GetValueOrDefault();
            newEstimateItem.EstimateID = IsValidEstimateID(newEstimateID) ? newEstimateID : null;
            newEstimateItem.NoteID = resetNoteID ? null : newEstimateItem.NoteID;

            return newEstimateItem;
        }

        public virtual AMEstimateItem CopyAsNewEstimateItem(AMEstimateItem fromEstimateItem, string newRevisionID, bool resetNoteId)
        {
            return CopyAsNewEstimateItem(fromEstimateItem, newRevisionID, string.Empty, resetNoteId);
        }

        public static void CreateNewEstimateReferenceRevisionOnly(EstimateMaint graph, AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            if (graph == null)
            {
                return;
            }

            CreateNewEstimateReferenceRevision(graph, fromEstimateItem, toEstimateItem, toEstimateItem.EstimateID);
        }

        protected static AMEstimateReference CreateNewEstimateReferenceRevision(PXGraph graph, AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem, string toEstimateId)
        {
            AMEstimateReference fromReference = PXSelect<AMEstimateReference,
                Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateReference.estimateID>>,
                    And<AMEstimateReference.revisionID, Equal<Required<AMEstimateReference.revisionID>>>>
            >.Select(graph, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID);

            if (fromReference != null)
            {
                var toRef = CreateEstimateReferenceCopy(fromReference);
                toRef = EstimateGraphHelper.AddEstimateReference(fromReference, toRef);
                toRef.EstimateID = toEstimateId;
                toRef.RevisionID = toEstimateItem.RevisionID;

                if (!toEstimateItem.IsPrimary.GetValueOrDefault())
                {
                    EstimateGraphHelper.ClearQuoteReferenceFields(ref toRef);
                }
                else
                {
                    EstimateGraphHelper.ClearQuoteReferenceFields(ref fromReference);
                    graph.Caches<AMEstimateReference>().Update(fromReference);
                }

                toRef = (AMEstimateReference)graph.Caches<AMEstimateReference>().Insert(toRef);
                graph.Caches<AMEstimateReference>().Current = toRef;
                return toRef;
            }

            return null;
        }

        public virtual AMEstimateReference CreateNewEstimateReferenceRevision(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            var newEstimateReference = CreateNewEstimateReferenceRevision(this, fromEstimateItem, toEstimateItem,
                IsValidEstimateID(toEstimateItem) ? toEstimateItem.EstimateID : null);

            if (newEstimateReference != null)
            {
                return newEstimateReference;
            }

            return EstimateReferenceRecord.Insert(MakeEstimateReference(toEstimateItem));
        }

        public virtual AMEstimateReference CopyAsNewEstimateReference(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            AMEstimateReference fromReference = PXSelect<AMEstimateReference,
                Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateReference.estimateID>>,
                    And<AMEstimateReference.revisionID, Equal<Required<AMEstimateReference.revisionID>>>>
                >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID);

            if (fromReference != null)
            {
                var toRef = CreateEstimateReferenceCopy(fromReference);
                toRef.EstimateID = IsValidEstimateID(toEstimateItem) ? toEstimateItem.EstimateID : null;
                toRef.RevisionID = toEstimateItem.RevisionID;
                return toRef;
            }

            return MakeEstimateReference(toEstimateItem);
        }

        /// <summary>
        /// Create a new instance of <see cref="AMEstimateReference"/> copying some fields from an existing instance of <see cref="AMEstimateReference"/>
        /// </summary>
        /// <returns></returns>
        public static AMEstimateReference CreateEstimateReferenceCopy(AMEstimateReference fromReference)
        {
            return new AMEstimateReference
            {
                EstimateID = fromReference?.EstimateID,
                RevisionID = fromReference?.RevisionID,
                BranchID = fromReference?.BranchID,
                TaxCategoryID = fromReference?.TaxCategoryID,
                CuryInfoID = fromReference?.CuryInfoID,
                OrderQty = fromReference?.OrderQty,
                CuryUnitPrice = fromReference?.CuryUnitPrice,
                CuryExtPrice = fromReference?.CuryExtPrice
            };
        }

        public virtual AMEstimateReference MakeEstimateReference(AMEstimateItem ei)
        {
            if (ei == null)
            {
                return new AMEstimateReference();
            }

            string taxCategory = string.Empty;
            if (!ei.IsNonInventory.GetValueOrDefault() && ei.InventoryID != null)
            {
                InventoryItem inventoryItem = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>
                    >.Select(this, ei.InventoryID);
                if (inventoryItem != null)
                {
                    taxCategory = inventoryItem.TaxCategoryID;
                }
            }

            if (string.IsNullOrWhiteSpace(taxCategory))
            {
                AMEstimateClass estimateClass = PXSelect<AMEstimateClass,
                    Where<AMEstimateClass.estimateClassID, Equal<Required<AMEstimateClass.estimateClassID>>>
                    >.Select(this, ei.EstimateClassID);
                if (estimateClass != null)
                {
                    taxCategory = estimateClass.TaxCategoryID;
                }
            }

            return new AMEstimateReference
            {
                EstimateID = IsValidEstimateID(ei) ? ei.EstimateID : null,
                RevisionID = ei.RevisionID,
                BranchID = this.Accessinfo.BranchID,
                TaxCategoryID = taxCategory
            };
        }

        /// <summary>
        /// Copy the operations from the from estimate as operations of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        /// <param name="resetNoteID">clear out the noteid field when true</param>
        /// <returns>List of new/to estimate operations</returns>
        public virtual List<AMEstimateOper> CopyAsNewOperations(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem, bool resetNoteID)
        {
            var list = new List<AMEstimateOper>();
            foreach (AMEstimateOper estimateOper in PXSelect<AMEstimateOper, 
                Where<AMEstimateOper.estimateID, Equal<Required<AMEstimateOper.estimateID>>, 
                    And<AMEstimateOper.revisionID, Equal<Required<AMEstimateOper.revisionID>>>>
                        >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var newRow = AMEstimateOper.ClearCalcValues(PXCache<AMEstimateOper>.CreateCopy(estimateOper));
                if (newRow == null)
                {
                    continue;
                }

                newRow.RevisionID = toEstimateItem.RevisionID;
                newRow.EstimateID = IsValidEstimateID(toEstimateItem) ? toEstimateItem.EstimateID : null;
                newRow.NoteID = resetNoteID ? null : newRow.NoteID;

                if (EstimateSetup.Current.CopyOperationNotes.GetValueOrDefault() || EstimateSetup.Current.CopyOperationFiles.GetValueOrDefault())
                {
                    PXNoteAttribute.CopyNoteAndFiles(EstimateOperRecords.Cache, estimateOper, EstimateOperRecords.Cache, newRow,
                        EstimateSetup.Current.CopyOperationNotes.GetValueOrDefault(), EstimateSetup.Current.CopyOperationFiles.GetValueOrDefault());
                }

                list.Add(newRow);
            }
            return list;
        }

        /// <summary>
        /// Copy the material from the from estimate as material of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        /// <returns>List of new/to estimate material</returns>
        public virtual List<AMEstimateMatl> CopyAsNewMaterial(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            var list = new List<AMEstimateMatl>();
            foreach (AMEstimateMatl estimateMatl in PXSelect<AMEstimateMatl, 
                Where<AMEstimateMatl.estimateID, Equal<Required<AMEstimateMatl.estimateID>>, 
                    And<AMEstimateMatl.revisionID, Equal<Required<AMEstimateMatl.revisionID>>>>
                        >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var newRow = PXCache<AMEstimateMatl>.CreateCopy(estimateMatl);
                newRow.NoteID = null;
                newRow.RevisionID = toEstimateItem.RevisionID;
                newRow.EstimateID = IsValidEstimateID(toEstimateItem) ? toEstimateItem.EstimateID : null;
                list.Add(newRow);
            }
            return list;
        }

        /// <summary>
        /// Copy the tools from the from estimate as tools of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        /// <returns>List of new/to estimate tool</returns>
        public virtual List<AMEstimateTool> CopyAsNewTool(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            var list = new List<AMEstimateTool>();
            foreach (AMEstimateTool estimateTool in PXSelect<AMEstimateTool, 
                Where<AMEstimateTool.estimateID, Equal<Required<AMEstimateTool.estimateID>>, 
                    And<AMEstimateTool.revisionID, Equal<Required<AMEstimateTool.revisionID>>>>
                        >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var newRow = PXCache<AMEstimateTool>.CreateCopy(estimateTool);
                newRow.NoteID = null;
                newRow.RevisionID = toEstimateItem.RevisionID;
                newRow.EstimateID = IsValidEstimateID(toEstimateItem) ? toEstimateItem.EstimateID : null;
                list.Add(newRow);
            }
            return list;
        }

        /// <summary>
        /// Copy the overhead from the from estimate as overhead of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        /// <returns>List of new/to estimate overhead</returns>
        public virtual List<AMEstimateOvhd> CopyAsNewOverhead(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            var list = new List<AMEstimateOvhd>();
            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, 
                Where<AMEstimateOvhd.estimateID, Equal<Required<AMEstimateOvhd.estimateID>>, 
                    And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOvhd.revisionID>>>>
                        >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var newRow = PXCache<AMEstimateOvhd>.CreateCopy(estimateOvhd);
                newRow.NoteID = null;
                newRow.RevisionID = toEstimateItem.RevisionID;
                newRow.EstimateID = IsValidEstimateID(toEstimateItem) ? toEstimateItem.EstimateID : null;
                list.Add(newRow);
            }
            return list;
        }

        #region Update Variable Overhead Cost
        protected virtual void AMEstimateOper_FixedLaborCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.estimateID,
                Equal<Required<AMEstimateOper.estimateID>>, And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOper.operationID>>
                >>>>.Select(this, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                EstimateOperOvhdRecords.Cache.RaiseFieldUpdated<AMEstimateOvhd.overheadCostRate>(estimateOvhd, estimateOvhd.OverheadCostRate);
            }
            bool IsReadOnly = (cache.GetStatus(e.Row) == PXEntryStatus.Notchanged);
            PXFormulaAttribute.CalcAggregate<AMEstimateOvhd.variableOvhdOperCost>(EstimateOperOvhdRecords.Cache, e.Row, IsReadOnly);
            cache.RaiseFieldUpdated<AMEstimateOper.variableOverheadCalcCost>(e.Row, null);
        }

        protected virtual void AMEstimateOper_VariableLaborCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.estimateID,
                Equal<Required<AMEstimateOper.estimateID>>, And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOper.operationID>>
                >>>>.Select(this, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                EstimateOperOvhdRecords.Cache.RaiseFieldUpdated<AMEstimateOvhd.overheadCostRate>(estimateOvhd, estimateOvhd.OverheadCostRate);
            }
            var IsReadOnly = cache.GetStatus(e.Row) == PXEntryStatus.Notchanged;
            PXFormulaAttribute.CalcAggregate<AMEstimateOvhd.variableOvhdOperCost>(EstimateOperOvhdRecords.Cache, e.Row, IsReadOnly);
            cache.RaiseFieldUpdated<AMEstimateOper.variableOverheadCalcCost>(e.Row, null);
        }

        protected virtual void AMEstimateOper_MachineTimeHours_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.estimateID,
                Equal<Required<AMEstimateOper.estimateID>>, And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOper.operationID>>
                >>>>.Select(this, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                EstimateOperOvhdRecords.Cache.RaiseFieldUpdated<AMEstimateOvhd.overheadCostRate>(estimateOvhd, estimateOvhd.OverheadCostRate);
            }
            bool IsReadOnly = (cache.GetStatus(e.Row) == PXEntryStatus.Notchanged);
            PXFormulaAttribute.CalcAggregate<AMEstimateOvhd.variableOvhdOperCost>(EstimateOperOvhdRecords.Cache, e.Row, IsReadOnly);
            cache.RaiseFieldUpdated<AMEstimateOper.variableOverheadCalcCost>(e.Row, null);
        }

        protected virtual void AMEstimateOper_MaterialCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.estimateID,
                Equal<Required<AMEstimateOper.estimateID>>, And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOper.operationID>>
                >>>>.Select(this, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                EstimateOperOvhdRecords.Cache.RaiseFieldUpdated<AMEstimateOvhd.overheadCostRate>(estimateOvhd, estimateOvhd.OverheadCostRate);
            }
            bool IsReadOnly = (cache.GetStatus(e.Row) == PXEntryStatus.Notchanged);
            PXFormulaAttribute.CalcAggregate<AMEstimateOvhd.variableOvhdOperCost>(EstimateOperOvhdRecords.Cache, e.Row, IsReadOnly);
            cache.RaiseFieldUpdated<AMEstimateOper.variableOverheadCalcCost>(e.Row, null);
        }
        #endregion

        protected virtual void AMEstimateOper_WorkCenterID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            AMWC amwc = PXSelect<AMWC, Where<AMWC.wcID, Equal<Required<AMWC.wcID>>>>.Select(this, amEstimateOper.WorkCenterID);

            if (amwc != null)
            {
                amEstimateOper.Description = amwc.Descr;
                decimal? machStdCost = 0m;

                foreach (PXResult<AMWCMach, AMMach> result in PXSelectJoin<AMWCMach,
                    InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>>,
                    Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>>
                    >.Select(this, amwc.WcID))
                {
                    var wcMachine = (AMWCMach)result;
                    var machine = (AMMach)result;

                    if (string.IsNullOrWhiteSpace(wcMachine.MachID)
                        || string.IsNullOrWhiteSpace(machine.MachID)
                        || !machine.ActiveFlg.GetValueOrDefault())
                    {
                        continue;
                    }
                    machStdCost += machine.StdCost;
                }
                amEstimateOper.MachineStdCost = machStdCost;
            }

        }

    }
}