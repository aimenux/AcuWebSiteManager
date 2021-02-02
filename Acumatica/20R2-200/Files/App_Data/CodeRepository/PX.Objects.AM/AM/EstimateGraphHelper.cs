using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM
{
    public enum EstimateReferenceOrderAction
    {
        None,
        /// <summary>
        /// Estimate is a new estimate
        /// </summary>
        New,
        /// <summary>
        /// Adding an existing estimate to a different estimate source (Ex: Adding an existing estimate to a sales order)
        /// </summary>
        Add,
        /// <summary>
        /// Simple estimate update
        /// </summary>
        Update,
        /// <summary>
        /// Removing an estimate from an order
        /// </summary>
        Remove
    }

    /// <summary>
    /// Manufacturing Estimate Graph shared functions helper class
    /// </summary>
    public class EstimateGraphHelper
    {
        private PXGraph _graph;
        public readonly Numbering EstimateNumbering;

        /// <summary>
        /// Is the Customer Management > Sales Quotes feature enabled
        /// </summary>
        public bool IsOpportunityQuoteFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.salesQuotes>();

        public AMEstimateItem CurrentEstimateItem
        {
            get
            {
                if (_graph is EstimateMaint)
                {
                    return ((EstimateMaint) _graph).Documents.Current;
                }

                if (_graph is EstimateOperMaint)
                {
                    if (((EstimateOperMaint) _graph).EstimateRecordSelected.Current == null)
                    {
                        ((EstimateOperMaint) _graph).EstimateRecordSelected.Current =
                            ((EstimateOperMaint) _graph).EstimateRecordSelected.SelectSingle();
                    }

                    //Refresh reference
                    if (((EstimateOperMaint) _graph).EstimateRecordSelected.Current != null
                        && ((EstimateOperMaint)_graph).EstimateReferenceRecord.Current == null)
                    {
                        ((EstimateOperMaint) _graph).EstimateReferenceRecord.Current =
                            ((EstimateOperMaint) _graph).EstimateReferenceRecord.SelectSingle();
                    }

                    return ((EstimateOperMaint)_graph).EstimateRecordSelected.Current;
                }

                return null;
            }
        }

        public EstimateGraphHelper(EstimateMaint estimateMaintGraph)
        {
            _graph = estimateMaintGraph ?? throw new PXArgumentException(nameof(estimateMaintGraph));
            EstimateNumbering = PXSelectJoin<Numbering,
                LeftJoin<AMEstimateSetup, On<AMEstimateSetup.estimateNumberingID, Equal<Numbering.numberingID>>>,
                Where<Numbering.numberingID, Equal<AMEstimateSetup.estimateNumberingID>>>.Select(_graph);
        }

        public EstimateGraphHelper(EstimateOperMaint estimateOperMaintGraph)
        {
            _graph = estimateOperMaintGraph ?? throw new PXArgumentException(nameof(estimateOperMaintGraph));
            EstimateNumbering = PXSelectJoin<Numbering,
                LeftJoin<AMEstimateSetup, On<AMEstimateSetup.estimateNumberingID, Equal<Numbering.numberingID>>>,
                Where<Numbering.numberingID, Equal<AMEstimateSetup.estimateNumberingID>>>.Select(_graph);
        }

        protected virtual void PersistBaseGraph()
        {
            if (_graph is EstimateMaint)
            {
                ((EstimateMaint)_graph).PersistBase();
                return;
            }

            if (_graph is EstimateOperMaint)
            {
                ((EstimateOperMaint)_graph).PersistBase();
                return;
            }

            _graph.Persist();
        }

        /// <summary>
        /// Get the cache updated <see cref="AMEstimateReference"/> from the calling graph and removes it from the calling graphs cache
        /// </summary>
        protected virtual List<AMEstimateReference> GetRemoveUpdatedEstimateReferences()
        {
            var updatedList = GetUpdatedEstimateReferences();
            RemoveEstimatedReferenceFromCache(updatedList);
            return updatedList;
        }

        /// <summary>
        /// Get the cache updated <see cref="AMEstimateReference"/> from the calling graph
        /// </summary>
        protected virtual List<AMEstimateReference> GetUpdatedEstimateReferences()
        {
            return _graph?.Caches<AMEstimateReference>()?.Updated?.Cast<AMEstimateReference>().ToList();
        }

        /// <summary>
        /// Remove the given list of <see cref="AMEstimateReference"/> from the calling graph cache
        /// </summary>
        protected virtual void RemoveEstimatedReferenceFromCache(List<AMEstimateReference> estReferences)
        {
            if (estReferences == null)
            {
                return;
            }

            foreach (var estReference in estReferences)
            {
                RemoveEstimatedReferenceFromCache(estReference);
            }
        }

        /// <summary>
        /// Remove the given <see cref="AMEstimateReference"/> object from the calling graph cache
        /// </summary>
        protected virtual void RemoveEstimatedReferenceFromCache(AMEstimateReference estReference)
        {
            if (estReference?.RevisionID == null)
            {
                return;
            }
            _graph?.Caches<AMEstimateReference>()?.Remove(estReference);
        }

        public virtual Numbering CurrentEstimateNumbering => EstimateNumbering;

        public virtual string EstimateNewNumberSymbol => CurrentEstimateNumbering == null ? string.Empty : CurrentEstimateNumbering.NewSymbol;

        public virtual bool IsValidEstimateID(AMEstimateItem estimateItem)
        {
            return estimateItem != null && IsValidEstimateID(estimateItem.EstimateID);
        }
        public virtual bool IsValidEstimateID(string estimateID)
        {
            return !string.IsNullOrWhiteSpace(estimateID) && !estimateID.EqualsWithTrim(EstimateNewNumberSymbol);
        }

        /// <summary>
        /// Initialize/insert a new Estimate Reference record if one does not exist
        /// </summary>
        /// <param name="estimateItem"></param>
        protected virtual AMEstimateReference InitEstimateReferenceRecord(AMEstimateItem estimateItem)
        {
            if (estimateItem?.RevisionID == null)
            {
                return null;
            }

            var existingReference = FindEstimateReference(estimateItem);
            if (existingReference != null)
            {
                return existingReference;
            }

            var inserted = (AMEstimateReference) _graph.Caches[typeof(AMEstimateReference)].Insert(MakeEstimateReference(estimateItem));
            return inserted;
        }

        protected virtual AMEstimateReference MakeEstimateReference(AMEstimateItem estimateItem)
        {
            string taxCategory = string.Empty;
            if (!estimateItem.IsNonInventory.GetValueOrDefault() && estimateItem.InventoryID != null)
            {
                InventoryItem inventoryItem = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(_graph, estimateItem.InventoryID);
                if (inventoryItem != null)
                {
                    taxCategory = inventoryItem.TaxCategoryID;
                }
            }

            if (string.IsNullOrWhiteSpace(taxCategory))
            {
                AMEstimateClass estimateClass = PXSelect<AMEstimateClass,
                    Where<AMEstimateClass.estimateClassID, Equal<Required<AMEstimateClass.estimateClassID>>>>.Select(_graph, estimateItem.EstimateClassID);
                if (estimateClass != null)
                {
                    taxCategory = estimateClass.TaxCategoryID;
                }
            }

            return new AMEstimateReference
            {
                BranchID = _graph.Accessinfo.BranchID,
                TaxCategoryID = taxCategory,
                OrderQty = estimateItem.OrderQty,
                CuryUnitPrice = estimateItem.CuryUnitPrice
            };
        }

        protected virtual AMEstimateReference FindEstimateReference(AMEstimateItem estimateItem)
        {
            if (estimateItem == null)
            {
                return null;
            }

            if (_graph.Caches<AMEstimateItem>().GetStatus(estimateItem) == PXEntryStatus.Inserted)
            {
                var insertedRef = FindInsertedEstimateReference(estimateItem);
                if (insertedRef != null)
                {
                    return insertedRef;
                }
            }

            var estimateReference = new AMEstimateReference
            {
                EstimateID = estimateItem.EstimateID,
                RevisionID = estimateItem.RevisionID
            };

            var cacheReference = (AMEstimateReference)_graph.Caches[typeof(AMEstimateReference)].Locate(estimateReference);

            if (cacheReference != null)
            {
                return cacheReference;
            }

            return PXSelect<AMEstimateReference,
                Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateReference.estimateID>>,
                    And<AMEstimateReference.revisionID, Equal<Required<AMEstimateReference.revisionID>>>>>.Select(_graph, estimateItem.EstimateID, estimateItem.RevisionID);
        }

        private AMEstimateReference FindInsertedEstimateReference(AMEstimateItem estimateItem)
        {
            if (estimateItem?.EstimateID == null)
            {
                return null;
            }

            foreach (AMEstimateReference insertedEstimateRef in _graph.Caches<AMEstimateReference>().Inserted)
            {
                if (insertedEstimateRef?.RevisionID == null || !IsSameEstimateRevision(estimateItem, insertedEstimateRef))
                {
                    continue;
                }

                return insertedEstimateRef;
            }

            return null;
        }

        /// <summary>
        /// Updates order <see cref="AMEstimateReference"/> cache based on existing references and the updating reference.
        /// </summary>
        /// <param name="estimateReferenceOrderCache">Cache linked to an order of type <see cref="AMEstimateReference"/></param>
        /// <param name="currentOrderEstReferences"></param>
        /// <param name="updatedEstReferences"></param>
        /// <param name="estimateReference"></param>
        protected virtual void UpdateExistingOrderEstimateReference(PXCache estimateReferenceOrderCache, List<AMEstimateReference> currentOrderEstReferences, List<AMEstimateReference> updatedEstReferences, AMEstimateReference estimateReference)
        {
            if (currentOrderEstReferences == null)
            {
                return;
            }

            if (estimateReferenceOrderCache.GetItemType() != typeof(AMEstimateReference))
            {
                return;
            }

            foreach (var orderEstReference in currentOrderEstReferences)
            {
                if (orderEstReference == null)
                {
                    continue;
                }

                if (IsSameEstimateRevision(orderEstReference, estimateReference))
                {
                    continue;
                }

                if (!TryFindInList(orderEstReference, updatedEstReferences, out var found))
                {
                    if (IsSameEstimateDifferentRevision(orderEstReference, estimateReference))
                    {
                        //...but exists on the order
                        found = orderEstReference;
                    }
                }

                if (found == null)
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"[{orderEstReference.QuoteType}:{orderEstReference.QuoteNbr}] Removing Estimate Reference {found.EstimateID} {found.RevisionID} Order Qty from {found.OrderQty} to zero and Unit Price from {found.CuryUnitPrice} to zero");
#endif
                //Here we are "removing" the qty and cost from the previous revision and to re-add below...
                found.OpportunityID = orderEstReference.OpportunityID;
                found.OpportunityQuoteID = orderEstReference.OpportunityQuoteID;
                found.QuoteType = orderEstReference.QuoteType;
                found.QuoteNbr = orderEstReference.QuoteNbr;
                found.TaxLineNbr = orderEstReference.TaxLineNbr;
                found.OrderQty = 0;
                found.CuryUnitPrice = 0;
                // Update to reduce any totals on the order...
                found = (AMEstimateReference)estimateReferenceOrderCache.Update(found);
                // ... Then "remove" the reference from the order
                ClearQuoteReferenceFields(ref found);
                estimateReferenceOrderCache.Update(found);
            }
        }

        protected virtual void PersistSOOrderEntry(AMEstimateReference estimateReference, PXEntryStatus status, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (status == PXEntryStatus.Notchanged)
            {
                return;
            }

            if (estimateReference?.RevisionID == null)
            {
                throw new PXArgumentException(nameof(estimateReference));
            }

            if (string.IsNullOrWhiteSpace(estimateReference.QuoteNbr)
                || string.IsNullOrWhiteSpace(estimateReference.QuoteType))
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateReferenceMissingSalesOrderNbrOrType));
            }

            var soOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
            soOrderEntry.Document.Current = soOrderEntry.Document.Search<SOOrder.orderNbr>(estimateReference.QuoteNbr, estimateReference.QuoteType);
            if (soOrderEntry.Document.Current == null)
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateReferenceSalesOrderNotFound));
            }

            var orderAllowsEdit = soOrderEntry.GetExtension<SOOrderEntryAMExtension>()?.OrderAllowsEdit == true;
            if (!orderAllowsEdit)
            {
                PXTrace.WriteWarning("order does not allow changes. No order updates performed.");
                throw new PXException(Messages.GetLocal(Messages.CannotAddEstimateSalesOrderClosedCancelled));
            }

            PersistSOOrderEntry(soOrderEntry, estimateReference, status, referenceOrderAction);
        }

        protected virtual void PersistSOOrderEntry(SOOrderEntry soOrderEntry, AMEstimateReference estimateReference, PXEntryStatus status, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (status == PXEntryStatus.Notchanged)
            {
                return;
            }

            if (soOrderEntry?.Document.Current == null)
            {
                throw new PXArgumentException(nameof(soOrderEntry));
            }

            if (estimateReference == null)
            {
                throw new PXArgumentException(nameof(estimateReference));
            }

            SOOrderEntryAMExtension soOrderEntryExtension = null;
            try
            {
                using (var ts = new PXTransactionScope())
                {
                    soOrderEntryExtension = soOrderEntry.GetExtension<SOOrderEntryAMExtension>();

                    switch (status)
                    {
                        case PXEntryStatus.Inserted:
                            UpdateExistingOrderEstimateReference(soOrderEntryExtension.OrderEstimateRecords.Cache,
                                soOrderEntryExtension.OrderEstimateRecords.Select().ToFirstTableList(),
                                GetRemoveUpdatedEstimateReferences(),
                                estimateReference);
                            PersistBaseGraph();
                            var doc = CurrentEstimateItem;
                            estimateReference.EstimateID = doc?.EstimateID;
                            estimateReference.RevisionID = doc?.RevisionID;
                            estimateReference.TaxLineNbr = (int?)PXLineNbrAttribute.NewLineNbr<AMEstimateReference.taxLineNbr>(soOrderEntryExtension.OrderEstimateRecords.Cache, soOrderEntry.Document.Current);
                            estimateReference.CuryInfoID = soOrderEntry.Document.Current.CuryInfoID;
                            soOrderEntryExtension.OrderEstimateRecords.Insert(estimateReference);
                            break;
                        case PXEntryStatus.Updated:
                            UpdateExistingOrderEstimateReference(soOrderEntryExtension.OrderEstimateRecords.Cache,
                                soOrderEntryExtension.OrderEstimateRecords.Select().ToFirstTableList(),
                                GetRemoveUpdatedEstimateReferences(),
                                estimateReference);
                            if (estimateReference.TaxLineNbr == null || referenceOrderAction == EstimateReferenceOrderAction.Add)
                            {
                                estimateReference.TaxLineNbr = (int?)PXLineNbrAttribute.NewLineNbr<AMEstimateReference.taxLineNbr>(soOrderEntryExtension.OrderEstimateRecords.Cache, soOrderEntry.Document.Current);
                            }
                            estimateReference.CuryInfoID = soOrderEntry.Document.Current.CuryInfoID;
#if DEBUG
                            AMDebug.TraceWriteMethodName($"[{estimateReference.QuoteType}:{estimateReference.QuoteNbr}] Updating Estimate Reference {estimateReference.EstimateID} {estimateReference.RevisionID} Order Qty {estimateReference.OrderQty} and Unit Price {estimateReference.CuryUnitPrice}");
#endif
                            soOrderEntryExtension.OrderEstimateRecords.Update(estimateReference);
                            break;
                        case PXEntryStatus.Deleted:
                            soOrderEntryExtension.OrderEstimateRecords.Delete(estimateReference);
                            break;
                    }

                    if (soOrderEntry.IsDirty)
                    {
                        soOrderEntry.RecalculateExternalTaxesSync = true;
                        soOrderEntry.Actions.PressSave();
                    }

                    if (_graph.IsDirty)
                    {
                        PersistBaseGraph();
                    }

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                soOrderEntryExtension?.OrderEstimateRecords.Cache.Clear();

                PXTraceHelper.PxTraceException(ex);
                throw new PXException(Messages.UnableToSaveCahngesWithSO, ex.Message);
            }
        }

        public virtual void PersistSOOrderEntry(SOOrderEntry soOrderEntry)
        {
            PersistEstimateReference(soOrderEntry);
        }

        public virtual void PersistSOOrderEntry(SOOrderEntry soOrderEntry, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (soOrderEntry?.Document.Current == null)
            {
                throw new PXArgumentException(nameof(soOrderEntry));
            }

            PersistEstimateReference(soOrderEntry, referenceOrderAction);
        }

        public virtual void PersistSOOrderEntryRemove(SOOrderEntry soOrderEntry, List<AMEstimateReference> estimateReferenceRemoves)
        {
            if (soOrderEntry?.Document.Current == null)
            {
                throw new PXArgumentException(nameof(soOrderEntry));
            }

            if (estimateReferenceRemoves == null || estimateReferenceRemoves.Count == 0)
            {
                return;
            }

            var estimateRefCache = _graph.Caches[typeof(AMEstimateReference)];
            foreach (var estimateReference in estimateReferenceRemoves)
            {
                var estimateInsert = PXCache<AMEstimateReference>.CreateCopy(estimateReference);

                //find the record in the current cache to "remove" before the insert
                var cacheRow = estimateRefCache.Locate(estimateReference);
                if (cacheRow != null)
                {
                    estimateRefCache.Remove(estimateReference);
                }

                estimateInsert.QuoteType = null;
                estimateInsert.QuoteNbr = null;

                //mark as insert to bring the record back before the SO order delete
                estimateRefCache.Insert(estimateInsert);

                //Delete on sales order to allow for correct order updates...
                PersistSOOrderEntry(soOrderEntry, estimateReference, PXEntryStatus.Deleted, EstimateReferenceOrderAction.Remove);
            }
        }

        protected virtual void PersistOpportunityMaint(AMEstimateReference estimateReference, PXEntryStatus status)
        {
            PersistOpportunityMaint(estimateReference, status, EstimateReferenceOrderAction.None);
        }

        protected virtual void PersistOpportunityMaint(AMEstimateReference estimateReference, PXEntryStatus status, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (status == PXEntryStatus.Notchanged)
            {
                return;
            }

            if (estimateReference?.RevisionID == null)
            {
                throw new PXArgumentException(nameof(estimateReference));
            }

            if (string.IsNullOrWhiteSpace(estimateReference.OpportunityID)
                || estimateReference.OpportunityQuoteID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateReferenceMissingOpportunityID));
            }

            var opportunityMaint = PXGraph.CreateInstance<OpportunityMaint>();
            opportunityMaint.Opportunity.Current = opportunityMaint.Opportunity.Search<CROpportunity.opportunityID>(estimateReference.OpportunityID);
            if (opportunityMaint.Opportunity.Current == null)
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateReferenceOpportunityNotFound));
            }

            var orderAllowsEdit = opportunityMaint.GetExtension<OpportunityMaintAMExtension>()?.OrderAllowsEdit == true;

            //Opportunities appear to be enabled/disabled via automation steps. Check to see if the view allows for updates and deletes
            if (!orderAllowsEdit)
            {
                PXTrace.WriteWarning("Opportunity does not allow changes. No updates performed.");
                throw new PXException(Messages.GetLocal(Messages.CannotAddorEditEstimateOpportunityClosed));
            }

            PersistOpportunityMaint(opportunityMaint, estimateReference, status, referenceOrderAction);
        }

        protected virtual void PersistOpportunityMaint(OpportunityMaint opportunityMaint, AMEstimateReference estimateReference, PXEntryStatus status, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (status == PXEntryStatus.Notchanged)
            {
                return;
            }

            if (opportunityMaint?.Opportunity.Current == null)
            {
                throw new PXArgumentException(nameof(opportunityMaint));
            }

            if (estimateReference.RevisionID == null)
            {
                throw new PXArgumentException(nameof(estimateReference));
            }

            OpportunityMaintAMExtension opportunityMaintExtension = null;
            try
            {
                using (var ts = new PXTransactionScope())
                {
                    opportunityMaintExtension = opportunityMaint.GetExtension<OpportunityMaintAMExtension>();
                    switch (status)
                    {
                        case PXEntryStatus.Inserted:
                            UpdateExistingOrderEstimateReference(opportunityMaintExtension.OpportunityEstimateRecords.Cache,
                                opportunityMaintExtension.OpportunityEstimateRecords.Select().ToFirstTableList(),
                                GetRemoveUpdatedEstimateReferences(),
                                estimateReference);
                            PersistBaseGraph();
                            var doc = CurrentEstimateItem;
                            estimateReference.EstimateID = doc?.EstimateID;
                            estimateReference.RevisionID = doc?.RevisionID;
                            estimateReference.TaxLineNbr = (int?)PXLineNbrAttribute.NewLineNbr<AMEstimateReference.taxLineNbr>(opportunityMaintExtension.OpportunityEstimateRecords.Cache, opportunityMaint.Opportunity.Current);
                            estimateReference.CuryInfoID = opportunityMaint.Opportunity.Current.CuryInfoID;
                            opportunityMaintExtension.OpportunityEstimateRecords.Insert(estimateReference);
                            break;
                        case PXEntryStatus.Updated:
                            UpdateExistingOrderEstimateReference(opportunityMaintExtension.OpportunityEstimateRecords.Cache,
                                opportunityMaintExtension.OpportunityEstimateRecords.Select().ToFirstTableList(),
                                GetRemoveUpdatedEstimateReferences(),
                                estimateReference);
                            if (estimateReference.TaxLineNbr == null || referenceOrderAction == EstimateReferenceOrderAction.Add)
                            {
                                estimateReference.TaxLineNbr = (int?)PXLineNbrAttribute.NewLineNbr<AMEstimateReference.taxLineNbr>(opportunityMaintExtension.OpportunityEstimateRecords.Cache, opportunityMaint.Opportunity.Current);
                            }
                            estimateReference.CuryInfoID = opportunityMaint.Opportunity.Current.CuryInfoID;

                            if (referenceOrderAction == EstimateReferenceOrderAction.Add)
                            {
                                //Required to get the tax record created
                                opportunityMaintExtension.OpportunityEstimateRecords.Cache.RaiseRowInserted(estimateReference);
                            }
#if DEBUG
                            AMDebug.TraceWriteMethodName($"[{estimateReference.OpportunityID}:{estimateReference.OpportunityQuoteID}:{estimateReference.QuoteNbr}] Updating Estimate Reference {estimateReference.EstimateID} {estimateReference.RevisionID} Order Qty {estimateReference.OrderQty} and Unit Price {estimateReference.CuryUnitPrice}");
#endif
                            opportunityMaintExtension.OpportunityEstimateRecords.Update(estimateReference);
                            break;
                        case PXEntryStatus.Deleted:
                            opportunityMaintExtension.OpportunityEstimateRecords.Delete(estimateReference);
                            break;
                    }

                    if (opportunityMaint.IsDirty)
                    {
                        // Using persist over Actions.PressSave because that call will fail when using External Tax Providers
                        opportunityMaint.Persist();
                    }

                    if (_graph.IsDirty)
                    {
                        PersistBaseGraph();
                    }

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                opportunityMaintExtension?.OpportunityEstimateRecords.Cache.Clear();

                PXTraceHelper.PxTraceException(ex);
                throw new PXException(Messages.UnableToSaveCahngesWithOpp, ex.Message);
            }
        }

        public virtual void PersistOpportunityMaint(OpportunityMaint opportunityMaint)
        {
            PersistEstimateReference(opportunityMaint);
        }

        public virtual void PersistOpportunityMaint(OpportunityMaint opportunityMaint, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (opportunityMaint?.Opportunity.Current == null)
            {
                throw new PXArgumentException(nameof(opportunityMaint));
            }

            PersistEstimateReference(opportunityMaint, referenceOrderAction);
        }

        public virtual void PersistOpportunityMaintRemove(OpportunityMaint opportunityMaint, List<AMEstimateReference> estimateReferenceRemoves)
        {
            if (opportunityMaint?.Opportunity.Current == null)
            {
                throw new PXArgumentException(nameof(opportunityMaint));
            }

            if (estimateReferenceRemoves == null || estimateReferenceRemoves.Count == 0)
            {
                return;
            }

            var estimateRefCache = _graph.Caches[typeof(AMEstimateReference)];
            foreach (var estimateReference in estimateReferenceRemoves)
            {
                var estimateInsert = PXCache<AMEstimateReference>.CreateCopy(estimateReference);

                //find the record in the current cache to "remove" before the insert
                var cacheRow = estimateRefCache.Locate(estimateReference);
                if (cacheRow != null)
                {
                    estimateRefCache.Remove(estimateReference);
                }

                estimateInsert.OpportunityID = null;
                estimateInsert.OpportunityQuoteID = null;
                estimateInsert.QuoteNbr = null;

                //mark as insert to bring the record back before the SO order delete
                estimateRefCache.Insert(estimateInsert);

                //Delete on sales order to allow for correct order updates...
                PersistOpportunityMaint(opportunityMaint, estimateReference, PXEntryStatus.Deleted, EstimateReferenceOrderAction.Remove);
            }
        }

        public virtual void PersistQuoteMaintRemove(QuoteMaint quoteMaint, List<AMEstimateReference> estimateReferenceRemoves)
        {
            if (quoteMaint?.Quote.Current == null)
            {
                throw new PXArgumentException(nameof(quoteMaint));
            }

            if (estimateReferenceRemoves == null || estimateReferenceRemoves.Count == 0)
            {
                return;
            }

            var estimateRefCache = _graph.Caches[typeof(AMEstimateReference)];
            foreach (var estimateReference in estimateReferenceRemoves)
            {
                var estimateInsert = PXCache<AMEstimateReference>.CreateCopy(estimateReference);

                //find the record in the current cache to "remove" before the insert
                var cacheRow = estimateRefCache.Locate(estimateReference);
                if (cacheRow != null)
                {
                    estimateRefCache.Remove(estimateReference);
                }

                estimateInsert.OpportunityID = null;
                estimateInsert.OpportunityQuoteID = null;
                estimateInsert.QuoteNbr = null;

                //mark as insert to bring the record back before the SO order delete
                estimateRefCache.Insert(estimateInsert);

                //Delete on sales order to allow for correct order updates...
                PersistQuoteMaint(quoteMaint, estimateReference, PXEntryStatus.Deleted, EstimateReferenceOrderAction.Remove);
            }
        }

        public virtual void PersistQuoteMaint(QuoteMaint quoteMaint, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (quoteMaint?.Quote.Current == null)
            {
                throw new PXArgumentException(nameof(quoteMaint));
            }

            PersistEstimateReference(quoteMaint, referenceOrderAction);
        }

        protected virtual void PersistQuoteMaint(AMEstimateReference estimateReference, PXEntryStatus status, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (status == PXEntryStatus.Notchanged)
            {
                return;
            }

            if (estimateReference.RevisionID == null)
            {
                throw new PXArgumentException(nameof(estimateReference));
            }

            if (string.IsNullOrWhiteSpace(estimateReference.OpportunityID)
                || estimateReference.OpportunityQuoteID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateReferenceMissingOpportunityID));
            }

            var quoteMaint = PXGraph.CreateInstance<QuoteMaint>();
            quoteMaint.Quote.Current = quoteMaint.Quote.Search<CRQuote.quoteNbr>(estimateReference.QuoteNbr, estimateReference.OpportunityID);
            if (quoteMaint.Quote.Current == null)
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateReferenceOpportunityNotFound));
            }

            var orderAllowsEdit = quoteMaint.GetExtension<QuoteMaintAMExtension>()?.OrderAllowsEdit == true;
            if (!orderAllowsEdit)
            {
                PXTrace.WriteWarning("Opportunity Quote does not allow changes. No updates performed.");
                throw new PXException(Messages.GetLocal(Messages.CannotAddorEditEstimateOpportunityQuoteClosed));
            }

            PersistQuoteMaint(quoteMaint, estimateReference, status, referenceOrderAction);
        }

        protected virtual void PersistQuoteMaint(QuoteMaint quoteMaint, AMEstimateReference estimateReference, PXEntryStatus status, EstimateReferenceOrderAction referenceOrderAction)
        {
            if (status == PXEntryStatus.Notchanged)
            {
                return;
            }

            if (quoteMaint?.Quote?.Current == null)
            {
                throw new PXArgumentException(nameof(quoteMaint));
            }

            if (estimateReference.RevisionID == null)
            {
                throw new PXArgumentException(nameof(estimateReference));
            }

            QuoteMaintAMExtension quoteMaintExtension = null;
            try
            {
                using (var ts = new PXTransactionScope())
                {
                    quoteMaintExtension = quoteMaint.GetExtension<QuoteMaintAMExtension>();
                    switch (status)
                    {
                        case PXEntryStatus.Inserted:
                            UpdateExistingOrderEstimateReference(quoteMaintExtension.OpportunityEstimateRecords.Cache,
                                quoteMaintExtension.OpportunityEstimateRecords.Select().ToFirstTableList(),
                                GetRemoveUpdatedEstimateReferences(),
                                estimateReference);
                            PersistBaseGraph();
                            var doc = CurrentEstimateItem;
                            estimateReference.EstimateID = doc?.EstimateID;
                            estimateReference.RevisionID = doc?.RevisionID;
                            estimateReference.TaxLineNbr = (int?)PXLineNbrAttribute.NewLineNbr<AMEstimateReference.taxLineNbr>(quoteMaintExtension.OpportunityEstimateRecords.Cache, quoteMaint.Quote.Current);
                            estimateReference.CuryInfoID = quoteMaint.Quote.Current.CuryInfoID;
                            quoteMaintExtension.OpportunityEstimateRecords.Insert(estimateReference);
                            break;
                        case PXEntryStatus.Updated:
                            UpdateExistingOrderEstimateReference(quoteMaintExtension.OpportunityEstimateRecords.Cache,
                                quoteMaintExtension.OpportunityEstimateRecords.Select().ToFirstTableList(),
                                GetRemoveUpdatedEstimateReferences(),
                                estimateReference);
                            if (estimateReference.TaxLineNbr == null || referenceOrderAction == EstimateReferenceOrderAction.Add)
                            {
                                estimateReference.TaxLineNbr = (int?)PXLineNbrAttribute.NewLineNbr<AMEstimateReference.taxLineNbr>(quoteMaintExtension.OpportunityEstimateRecords.Cache, quoteMaint.Quote.Current);
                            }
                            estimateReference.CuryInfoID = quoteMaint.Quote.Current.CuryInfoID;

                            if (referenceOrderAction == EstimateReferenceOrderAction.Add)
                            {
                                //Required to get the tax record created
                                quoteMaintExtension.OpportunityEstimateRecords.Cache.RaiseRowInserted(estimateReference);
                            }
#if DEBUG
                            AMDebug.TraceWriteMethodName($"[{estimateReference.OpportunityID}:{estimateReference.OpportunityQuoteID}:{estimateReference.QuoteNbr}] Updating Estimate Reference {estimateReference.EstimateID} {estimateReference.RevisionID} Order Qty {estimateReference.OrderQty} and Unit Price {estimateReference.CuryUnitPrice}");
#endif
                            quoteMaintExtension.OpportunityEstimateRecords.Update(estimateReference);
                            if (referenceOrderAction != EstimateReferenceOrderAction.Add)
                            {
                                SetCuryProductsAmount(quoteMaint, true);
                            }
                            break;
                        case PXEntryStatus.Deleted:
                            quoteMaintExtension.OpportunityEstimateRecords.Delete(estimateReference);
                            break;
                    }

                    if (quoteMaint.IsDirty)
                    {
                        // Using persist over Actions.PressSave because that call will fail when using External Tax Providers
                        quoteMaint.Persist();
                    }

                    if (_graph.IsDirty)
                    {
                        PersistBaseGraph();
                    }

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                quoteMaintExtension?.OpportunityEstimateRecords.Cache.Clear();

                PXTraceHelper.PxTraceException(ex);
                throw new PXException(Messages.UnableToSaveCahngesWithOppQuote, ex.Message);
            }
        }

        internal static void SetCuryProductsAmount(QuoteMaint quoteMaintGraph, bool useCache)
        {
            if (quoteMaintGraph.Quote?.Current != null)
            {
                var copy = (CRQuote)quoteMaintGraph.Quote.Cache.CreateCopy(quoteMaintGraph.Quote.Current);

                var curyDocTotal =
                    copy.ManualTotalEntry.GetValueOrDefault()
                        ? copy.CuryAmount.GetValueOrDefault()
                        : copy.CuryProductsAmount.GetValueOrDefault();

                var estimateTotal = 0m;
                foreach (AMEstimateReference row in PXSelectReadonly<
                        AMEstimateReference,
                        Where<AMEstimateReference.opportunityQuoteID, Equal<Required<AMEstimateReference.opportunityQuoteID>
                            >>>
                    .Select(quoteMaintGraph, copy.QuoteID))
                {
                    var curyExtPrice =
                        (useCache
                            ? quoteMaintGraph.Caches<AMEstimateReference>().LocateElse(row)?.CuryExtPrice
                            : row?.CuryExtPrice) ?? 0m;
                    
                    estimateTotal += curyExtPrice;
                }

                // The total when copy is not getting the estimate number into the total
                var docTotalWithEst = curyDocTotal + estimateTotal;

                if (copy.CuryProductsAmount.GetValueOrDefault() == docTotalWithEst)
                {
                    return;
                }

                copy.CuryProductsAmount = docTotalWithEst;
                quoteMaintGraph.Quote.Update(copy);
            }
        }

        public virtual bool PersistEstimateReference()
        {
            return PersistEstimateReference(graph: null);
        }

        public virtual bool PersistEstimateReference(PXGraph graph)
        {
            return PersistEstimateReference(graph, EstimateReferenceOrderAction.None);
        }

        /// <summary>
        /// Persists the graph and the estimate reference to the correct related graph if the source reference is not estimate.
        /// </summary>
        /// <param name="graph">calling graph outside of the estimate maint graph (Ex: sales order graph)</param>
        /// <returns>True when the persists are completed, false when not and the calling program should persist the graph</returns>
        public virtual bool PersistEstimateReference(PXGraph graph, EstimateReferenceOrderAction referenceOrderAction)
        {
            var currentEstimateItem = CurrentEstimateItem;
            if (currentEstimateItem == null || !currentEstimateItem.IsPrimary.GetValueOrDefault())
            {
                return false;
            }

            var estimateItemCache = _graph.Caches[typeof(AMEstimateItem)];
            var estimateItemStatus = estimateItemCache.GetStatus(currentEstimateItem);
            var estimateRef = FindEstimateReference(currentEstimateItem);
            if (estimateRef == null)
            {
                //Init will make sure there is an estimate ref inserted into the cache for the current estimate
                estimateRef = InitEstimateReferenceRecord(currentEstimateItem);
            }

            if (estimateRef == null)
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateReferenceMissing));
            }

            var estimateReferenceCache = _graph.Caches[typeof(AMEstimateReference)];
            var estimateRefStatus = estimateReferenceCache.GetStatus(estimateRef);

            var estimateRefChange = PXCache<AMEstimateReference>.CreateCopy(estimateRef);
            bool qtyOrCostChanged = false;

            if (graph != null && graph is SOOrderEntry && referenceOrderAction == EstimateReferenceOrderAction.Remove)
            {
                estimateReferenceCache.Remove(estimateRefChange);
                PersistSOOrderEntry((SOOrderEntry)graph, estimateRefChange, PXEntryStatus.Updated, referenceOrderAction);
                return true;
            }

            if (graph != null && graph is OpportunityMaint && referenceOrderAction == EstimateReferenceOrderAction.Remove)
            {
                estimateReferenceCache.Remove(estimateRefChange);
                PersistOpportunityMaint((OpportunityMaint)graph, estimateRefChange, PXEntryStatus.Updated, referenceOrderAction);
                return true;
            }

            if (graph != null && graph is QuoteMaint && referenceOrderAction == EstimateReferenceOrderAction.Remove)
            {
                estimateReferenceCache.Remove(estimateRefChange);
                PersistQuoteMaint((QuoteMaint)graph, estimateRefChange, PXEntryStatus.Updated, referenceOrderAction);
                return true;
            }

            if ((estimateItemStatus == PXEntryStatus.Inserted || estimateItemStatus == PXEntryStatus.Updated
                 || estimateRefStatus == PXEntryStatus.Inserted || estimateRefStatus == PXEntryStatus.Updated)
                && estimateRef.EstimateID.EqualsWithTrim(currentEstimateItem.EstimateID)
                && estimateRef.RevisionID.EqualsWithTrim(currentEstimateItem.RevisionID)
                && !EstimateStatus.IsFinished(currentEstimateItem.EstimateStatus))
            {
                if (estimateRefChange.OrderQty.GetValueOrDefault() != currentEstimateItem.OrderQty.GetValueOrDefault())
                {
                    estimateRefChange.OrderQty = currentEstimateItem.OrderQty.GetValueOrDefault();
                    qtyOrCostChanged = true;
                }
                if (estimateRefChange.CuryUnitPrice.GetValueOrDefault() != currentEstimateItem.CuryUnitPrice.GetValueOrDefault())
                {
                    estimateRefChange.CuryUnitPrice = currentEstimateItem.CuryUnitPrice.GetValueOrDefault();
                    qtyOrCostChanged = true;
                }

                estimateRefChange = PXCache<AMEstimateReference>.CreateCopy((AMEstimateReference)estimateReferenceCache.Update(estimateRefChange));
            }

            //we need to use the update on the sales order graph for changes related to qty,cost as the revision matches but also if Tax Category changes
            if (!EstimateStatus.IsFinished(currentEstimateItem.EstimateStatus)
                && (qtyOrCostChanged || estimateRefStatus == PXEntryStatus.Inserted || estimateRefStatus == PXEntryStatus.Updated))
            {
                if (currentEstimateItem.QuoteSource == EstimateSource.SalesOrder)
                {
                    estimateRefStatus = estimateReferenceCache.GetStatus(estimateRefChange);
                    estimateReferenceCache.Remove(estimateRefChange);
                    if (graph != null && graph is SOOrderEntry)
                    {
                        PersistSOOrderEntry((SOOrderEntry)graph, estimateRefChange, estimateRefStatus, referenceOrderAction);
                        return true;
                    }
                    PersistSOOrderEntry(estimateRefChange, estimateRefStatus, referenceOrderAction);
                    return true;
                }

                if (currentEstimateItem.QuoteSource == EstimateSource.Opportunity)
                {
                    estimateRefStatus = estimateReferenceCache.GetStatus(estimateRefChange);
                    estimateReferenceCache.Remove(estimateRefChange);

                    if (string.IsNullOrWhiteSpace(estimateRefChange.QuoteNbr) || !IsOpportunityQuoteFeatureEnabled)
                    {
                        // No link to an Opp Quote will use the 
                        if (graph != null && graph is OpportunityMaint)
                        {
                            PersistOpportunityMaint((OpportunityMaint)graph, estimateRefChange, estimateRefStatus, referenceOrderAction);
                            return true;
                        }
                        PersistOpportunityMaint(estimateRefChange, estimateRefStatus, referenceOrderAction);
                        return true;
                    }
                    if (graph != null && graph is QuoteMaint)
                    {
                        PersistQuoteMaint((QuoteMaint)graph, estimateRefChange, estimateRefStatus, referenceOrderAction);
                        return true;
                    }
                    PersistQuoteMaint(estimateRefChange, estimateRefStatus, referenceOrderAction);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Do the <see cref="AMEstimateItem"/> and <see cref="AMEstimateReference"/> have the same EstimateIDs
        /// </summary>
        public static bool IsSameEstimate(AMEstimateItem estItem, AMEstimateReference estReference)
        {
            return estItem?.RevisionID != null && estReference?.RevisionID != null &&
                   estItem.EstimateID.EqualsWithTrim(estReference.EstimateID);
        }

        /// <summary>
        /// Do the two <see cref="AMEstimateReference"/> have the same EstimateID
        /// </summary>
        public static bool IsSameEstimate(AMEstimateReference estReference1, AMEstimateReference estReference2)
        {
            return estReference1?.RevisionID != null && estReference2?.RevisionID != null &&
                   estReference1.EstimateID.EqualsWithTrim(estReference2.EstimateID);
        }

        /// <summary>
        /// Do the <see cref="AMEstimateItem"/> and <see cref="AMEstimateReference"/> have the same keys (EstimateID and RevisionID)
        /// </summary>
        public static bool IsSameEstimateRevision(AMEstimateItem estItem, AMEstimateReference estReference)
        {
            return estItem?.RevisionID != null && estReference?.RevisionID != null &&
                   estItem.EstimateID.EqualsWithTrim(estReference.EstimateID) &&
                   estItem.RevisionID.EqualsWithTrim(estReference.RevisionID);
        }

        /// <summary>
        /// Do the two <see cref="AMEstimateReference"/> have the same keys (EstimateID and RevisionID)
        /// </summary>
        public static bool IsSameEstimateRevision(AMEstimateReference estReference1, AMEstimateReference estReference2)
        {
            return estReference1?.RevisionID != null && estReference2?.RevisionID != null &&
                   estReference1.EstimateID.EqualsWithTrim(estReference2.EstimateID) &&
                   estReference1.RevisionID.EqualsWithTrim(estReference2.RevisionID);
        }

        /// <summary>
        /// Do the two <see cref="AMEstimateReference"/> have the same EstimateID but different RevisionID
        /// </summary>
        public static bool IsSameEstimateDifferentRevision(AMEstimateReference estReference1, AMEstimateReference estReference2)
        {
            return estReference1?.RevisionID != null && estReference2?.RevisionID != null &&
                   estReference1.EstimateID.EqualsWithTrim(estReference2.EstimateID) &&
                   !estReference1.RevisionID.EqualsWithTrim(estReference2.RevisionID);
        }

        /// <summary>
        /// Does the given estimate contain a reference to a quote
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="estimateItem"></param>
        /// <param name="excludeGivenRevision"></param>
        /// <returns></returns>
        public static bool IsEstimateReferencedToQuote(PXGraph graph, AMEstimateItem estimateItem, bool excludeGivenRevision)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (estimateItem == null)
            {
                throw new ArgumentNullException(nameof(estimateItem));
            }

            foreach (AMEstimateReference estReference in PXSelect<AMEstimateReference, Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateReference.estimateID>>>>.Select(graph, estimateItem.EstimateID))
            {
                if (!HasQuoteReference(estReference) || excludeGivenRevision && IsSameEstimateRevision(estimateItem, estReference))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static bool HasQuoteReference(AMEstimateReference estReference)
        {
            return estReference != null && (estReference.OpportunityID != null ||
                                            estReference.OpportunityQuoteID != null ||
                                            !string.IsNullOrWhiteSpace(estReference.QuoteType) ||
                                            !string.IsNullOrWhiteSpace(estReference.QuoteNbr));
        }

        public static void ClearQuoteReferenceFields(ref AMEstimateReference estReference)
        {
            if (estReference == null)
            {
                return;
            }

            estReference.QuoteType = null;
            estReference.QuoteNbr = null;
            estReference.OpportunityID = null;
            estReference.OpportunityQuoteID = null;
        }

        /// <summary>
        /// Add the reference fields from the included fromReference parameter to the existing estimate item instance
        /// </summary>
        public static AMEstimateReference AddEstimateReference(AMEstimateReference fromReference, AMEstimateReference toReference)
        {
            if (fromReference == null || toReference == null)
            {
                return toReference;
            }

            toReference.BranchID = fromReference.BranchID ?? toReference.BranchID;
            toReference.TaxCategoryID = string.IsNullOrWhiteSpace(fromReference.TaxCategoryID)
                ? toReference.TaxCategoryID
                : fromReference.TaxCategoryID;
            toReference.TaxLineNbr = fromReference.TaxLineNbr;
            toReference.BAccountID = fromReference.BAccountID;
            toReference.ExternalRefNbr = fromReference.ExternalRefNbr;
            toReference.QuoteType = fromReference.QuoteType;
            toReference.QuoteNbr = fromReference.QuoteNbr;
            toReference.OrderType = fromReference.OrderType;
            toReference.OrderNbr = fromReference.OrderNbr;
            toReference.OpportunityID = fromReference.OpportunityID;
            toReference.OpportunityQuoteID = fromReference.OpportunityQuoteID;

            return toReference;
        }

        /// <summary>
        /// Find the given <see cref="AMEstimateReference"/> in the list by keys (EstimateID and RevisionID)
        /// </summary>
        protected static bool TryFindInList(AMEstimateReference find, List<AMEstimateReference> list, out AMEstimateReference found)
        {
            found = FindInList(find, list);
            return found != null;
        }

        /// <summary>
        /// Find the given <see cref="AMEstimateReference"/> in the list by keys (EstimateID and RevisionID)
        /// </summary>
        /// <returns>The object from the list that matches</returns>
        protected static AMEstimateReference FindInList(AMEstimateReference find, List<AMEstimateReference> list)
        {
            if (list == null || find?.RevisionID == null)
            {
                return null;
            }

            foreach (var estimateReference in list)
            {
                if (IsSameEstimateRevision(find, estimateReference))
                {
                    return estimateReference;
                }
            }

            return null;
        }

        public static bool InventoryCDUpdateRequired<InventoryCdField>(PXCache cache)
            where InventoryCdField : IBqlField
        {
            var currentRow = cache.Current;
            if (currentRow == null)
            {
                return false;
            }

            var origRow = cache.GetOriginal(currentRow);
            if (origRow == null || cache.GetValue<InventoryCdField>(origRow) == null || cache.ObjectsEqual<InventoryCdField>(origRow, currentRow))
            {
                return false;
            }

            return true;
        }

        public static void UpdateEstimateInventoryCD(InventoryItem invItem, PXGraph graph)
        {
            PXUpdate<Set<AMEstimateItem.inventoryCD, Required<AMEstimateItem.inventoryCD>>, AMEstimateItem,
                Where<AMEstimateItem.inventoryID, Equal<Required<AMEstimateItem.inventoryID>
                >>>.Update(graph, invItem.InventoryCD, invItem.InventoryID);

            PXUpdate<Set<AMEstimateMatl.inventoryCD, Required<AMEstimateMatl.inventoryCD>>, AMEstimateMatl,
                Where<AMEstimateMatl.inventoryID, Equal<Required<AMEstimateMatl.inventoryID>
                >>>.Update(graph, invItem.InventoryCD, invItem.InventoryID);
        }

        /// <summary>
        /// Provides a way to self correct line counters for estimate history
        /// </summary>
        internal static bool TryCorrectHistoryLineCounters(PXCache estimateItemCache, PXCache estimateHistoryCache)
        {
            var curEst = (AMEstimateItem)estimateItemCache?.Current;
            if (curEst?.EstimateID == null)
            {
                return false;
            }

            var maxHistory = (AMEstimateHistory)PXSelectGroupBy<AMEstimateHistory,
                Where<AMEstimateHistory.estimateID, Equal<Required<AMEstimateHistory.estimateID>>>,
                Aggregate<
                    GroupBy<AMEstimateHistory.estimateID>>>.Select(estimateItemCache.Graph, curEst.EstimateID);

            if (maxHistory?.LineNbr == null)
            {
                return false;
            }

            curEst.LineCntrHistory = maxHistory.LineNbr + 1;
            estimateItemCache.Update(curEst);

            // Fix inserted history to get next persist to go through...
            var newEstimateHistory = new List<AMEstimateHistory>();
            foreach (AMEstimateHistory newHistory in estimateHistoryCache.Inserted)
            {
                var newCopy = PXCache<AMEstimateHistory>.CreateCopy(newHistory);
                newCopy.LineNbr = null;
                newEstimateHistory.Add(newCopy);
            }
            estimateHistoryCache.Clear();
            foreach (var estimateHistory in newEstimateHistory)
            {
                estimateHistoryCache.Insert(estimateHistory);
            }

            return true;
        }
    }
}