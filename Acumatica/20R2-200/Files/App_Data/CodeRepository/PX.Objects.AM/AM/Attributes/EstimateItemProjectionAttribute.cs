using PX.Data;
using System;

namespace PX.Objects.AM
{
    public class EstimateItemProjectionAttribute : PXProjectionAttribute
    {
        public EstimateItemProjectionAttribute(Type select)
            : base(select, new [] { typeof(Standalone.AMEstimateItem) })
        {
        }

        public override bool PersistInserted(PXCache sender, object row)
        {
            Standalone.AMEstimatePrimary estimatePrimary = null;
            var estimateItem = (AMEstimateItem)row;
            if (estimateItem != null)
            {
                AMDebug.TraceWriteMethodName($"Keys = {estimateItem.GetRowKeyValues(sender.Graph)}; PEstimateID = {estimateItem.PEstimateID}; PrimaryRevisionID = {estimateItem.PrimaryRevisionID}");
                if (string.IsNullOrWhiteSpace(estimateItem.PrimaryRevisionID))
                {
                    estimatePrimary = new Standalone.AMEstimatePrimary
                    {
                        EstimateID = estimateItem.EstimateID,
                        PrimaryRevisionID = estimateItem.RevisionID,
                        EstimateStatus = estimateItem.EstimateStatus,
                        QuoteSource = estimateItem.QuoteSource,
                        IsLockedByQuote = estimateItem.IsLockedByQuote,
                        LineCntrHistory = estimateItem.LineCntrHistory
                    };
                }
            }

            var basePersisted = base.PersistInserted(sender, row);

            if (!basePersisted)
            {
                return basePersisted;
            }

            if (estimatePrimary != null)
            {
                //Manually inserting 
                InsertPersistInserted(sender.Graph, estimatePrimary);
                return basePersisted;
            }

            TryPersistUpdateEstimatePrimary(sender, estimateItem);

            return basePersisted;
        }

        public override bool PersistUpdated(PXCache sender, object row)
        {
            var basePersisted = base.PersistUpdated(sender, row);

            if (basePersisted)
            {
                TryPersistUpdateEstimatePrimary(sender, (AMEstimateItem)row);
            }

            return basePersisted;
        }

        public override bool PersistDeleted(PXCache sender, object row)
        {
            var basePersisted = base.PersistDeleted(sender, row);

            var estimateItem = (AMEstimateItem) row;
            if (!basePersisted || estimateItem == null || !estimateItem.IsPrimary.GetValueOrDefault())
            {
                return basePersisted;
            }

            var nextEstimateRevision = GetNextEstimateRevision(sender.Graph, estimateItem);
            if (nextEstimateRevision?.RevisionID == null)
            {
                //We need to delete...
                DeletePersistDeleted(sender.Graph, GetEstimatePrimary(sender.Graph, estimateItem));
                return basePersisted;
            }

            // We need to update...
            var estimatePrimary = GetEstimatePrimary(sender.Graph, estimateItem);
            if (estimatePrimary != null)
            {
                estimatePrimary.PrimaryRevisionID = nextEstimateRevision.RevisionID;
                UpdatePersistUpdated(sender.Graph, estimatePrimary);
            }
            
            return basePersisted;
        }

        protected virtual void TryPersistUpdateEstimatePrimary(PXCache estimateItemCache, AMEstimateItem row)
        {
            if (row == null)
            {
                return;
            }

            var unchangedRow = (AMEstimateItem)estimateItemCache.GetOriginal(row);

            if (estimateItemCache.ObjectsEqual<
                AMEstimateItem.primaryRevisionID, 
                AMEstimateItem.quoteSource,
                AMEstimateItem.estimateStatus,
                AMEstimateItem.isLockedByQuote,
                AMEstimateItem.lineCntrHistory
                    >(row, unchangedRow))
            {
                return;
            }

            var estimatePrimary = GetEstimatePrimary(estimateItemCache.Graph, row);
            if (estimatePrimary == null)
            {
                if (string.IsNullOrWhiteSpace(row.PrimaryRevisionID))
                {
                    InsertPersistInserted(estimateItemCache.Graph, new Standalone.AMEstimatePrimary
                    {
                        EstimateID = row.EstimateID,
                        PrimaryRevisionID = row.RevisionID,
                        LineCntrHistory = row.LineCntrHistory ?? 0
                    });
                }
                return;
            }

            estimatePrimary.PrimaryRevisionID = row.PrimaryRevisionID;
            estimatePrimary.EstimateStatus = row.EstimateStatus;
            estimatePrimary.QuoteSource = row.QuoteSource;
            estimatePrimary.IsLockedByQuote = row.IsLockedByQuote;
            estimatePrimary.LineCntrHistory = row.LineCntrHistory;

            UpdatePersistUpdated(estimateItemCache.Graph, estimatePrimary);
        }

        protected virtual Standalone.AMEstimatePrimary GetEstimatePrimary(PXGraph graph, AMEstimateItem estimateItem)
        {
            if (estimateItem == null)
            {
                return null;
            }

            return (Standalone.AMEstimatePrimary)graph.Caches<Standalone.AMEstimatePrimary>().Locate(new Standalone.AMEstimatePrimary
                   {
                       EstimateID = estimateItem.EstimateID
                   }) ??
                   PXSelect<Standalone.AMEstimatePrimary, Where<Standalone.AMEstimatePrimary.estimateID,
                       Equal<Required<Standalone.AMEstimatePrimary.estimateID>>>>
                   .Select(graph, estimateItem.EstimateID);
        }

        protected virtual Standalone.AMEstimateItem GetNextEstimateRevision(PXGraph graph, AMEstimateItem estimateItem)
        {
            if (estimateItem == null)
            {
                return null;
            }

            return PXSelect<
            	Standalone.AMEstimateItem, 
            	Where<Standalone.AMEstimateItem.estimateID, Equal<Required<Standalone.AMEstimateItem.estimateID>>,
            		And<Standalone.AMEstimateItem.revisionID, NotEqual<Required<Standalone.AMEstimateItem.revisionID>>>>,
            	OrderBy<
            		Desc<Standalone.AMEstimateItem.estimateID,
            		Desc<Standalone.AMEstimateItem.revisionID>>>>
            	.SelectWindowed(graph, 0, 1, estimateItem.EstimateID, estimateItem.RevisionID);
        }

#if DEBUG
        private string DebugAMEstimatePrimary(Standalone.AMEstimatePrimary row) =>
            $"EstimateID = {row.EstimateID}; PrimaryRevisionID = {row.PrimaryRevisionID}; EstimateStatus = {Attributes.EstimateStatus.GetDescription(row.EstimateStatus)}; QuoteSource = {Attributes.EstimateSource.GetDescription(row.QuoteSource)}; LineCntrHistory = {row.LineCntrHistory}; IsLockedByQuote = {row.IsLockedByQuote}";
#endif

        protected virtual bool InsertPersistInserted(PXGraph graph, Standalone.AMEstimatePrimary row)
        {
            if (row?.EstimateID == null)
            {
                return false;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName(DebugAMEstimatePrimary(row));
#endif
            return graph.Caches<Standalone.AMEstimatePrimary>().PersistInserted(graph.Caches<Standalone.AMEstimatePrimary>().Insert(row));
        }

        protected virtual bool UpdatePersistUpdated(PXGraph graph, Standalone.AMEstimatePrimary row)
        {
            if (row?.EstimateID == null)
            {
                return false;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName(DebugAMEstimatePrimary(row));
#endif
            var updatedRow = graph.Caches[typeof(Standalone.AMEstimatePrimary)].Update(row);
            var ret = graph.Caches[typeof(Standalone.AMEstimatePrimary)].PersistUpdated(updatedRow);
            // Required during import to make sure each persist goes through to the database
            graph.Caches[typeof(Standalone.AMEstimatePrimary)].ResetPersisted(updatedRow);

            return ret;
        }

        protected virtual bool DeletePersistDeleted(PXGraph graph, Standalone.AMEstimatePrimary row)
        {
            if (row?.EstimateID == null)
            {
                return false;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName(DebugAMEstimatePrimary(row));
#endif
            var deletedRow = graph.Caches<Standalone.AMEstimatePrimary>().Delete(row);
            return graph.Caches<Standalone.AMEstimatePrimary>().PersistDeleted(deletedRow);
        }
    }
}