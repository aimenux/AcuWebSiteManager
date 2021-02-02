using System.Collections.Generic;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Responsible for deleted referenced transactions to a deleted line/batch in Manufacturing
    /// </summary>
    public class ReferenceDeleteGraph : PXGraph<ReferenceDeleteGraph>
    {
        public PXSelect<AMProdEvnt> AMProdEvntRecords; 

        public enum DeleteModeType
        {
            Batch,
            Line
        }

        public DeleteModeType DeleteMode { get; set; }

        public ReferenceDeleteGraph()
        {
            DeleteMode = DeleteModeType.Line;
        }

        /// <summary>
        /// Performs the delete and persist of related transactions found in calling graph
        /// </summary>
        /// <param name="callingGraph">calling graph containing deleted AMMTran or AMBatch cache</param>
        public static void DeleteReferenceTransactionsDisassembly(PXGraph callingGraph)
        {
            _DeleteReferenceTransactions<AMDisassembleBatch, AMDisassembleTran>(callingGraph);
        }

        /// <summary>
        /// Performs the delete and persist of related transactions found in calling graph
        /// </summary>
        /// <param name="callingGraph">calling graph containing deleted AMMTran or AMBatch cache</param>
        public static void DeleteReferenceTransactions(PXGraph callingGraph)
        {
            _DeleteReferenceTransactions<AMBatch, AMMTran>(callingGraph);
        }

        public static bool ContainsDeletes<Batch, Tran>(PXGraph callingGraph)
            where Batch : IBqlTable
            where Tran : IBqlTable
        {
            return callingGraph.Caches[typeof(Tran)].Deleted.Any_() ||
                   callingGraph.Caches[typeof(Batch)].Deleted.Any_();
        }

        private static void _DeleteReferenceTransactions<Batch, Tran>(PXGraph callingGraph) 
            where Batch : IBqlTable 
            where Tran : IBqlTable
        {
            if (callingGraph == null)
            {
                return;
            }

            if (!ContainsDeletes<Batch, Tran>(callingGraph))
            {
                //calling graph does not contain any deletes
                return;
            }

            var refDeleteGraph = CreateInstance<ReferenceDeleteGraph>();

            if (callingGraph.Caches[typeof(Batch)].Deleted.Any_())
            {
                refDeleteGraph.DeleteMode = DeleteModeType.Batch;
                foreach (var row in GetDeletedBatch(callingGraph, typeof(Batch)))
                {
                    refDeleteGraph.LoadFromDeletedAMBatch(row);
                }
            }

            //include load from ammtran to capture production events
            foreach (var row in GetDeletedTrans(callingGraph, typeof(Tran)))
            {
                refDeleteGraph.LoadFromDeletedAMMTran(row);
            }

            refDeleteGraph.Persist();
        }

        protected static List<AMBatch> GetDeletedBatch(PXGraph callingGraph, System.Type type)
        {
            var list = new List<AMBatch>();
            foreach (var deletedRow in callingGraph.Caches[type].Deleted)
            {
                if (deletedRow is AMDisassembleBatch)
                {
                    list.Add(AMDisassembleBatch.Convert((AMDisassembleBatch)deletedRow));
                    continue;
                }

                list.Add((AMBatch)deletedRow);
            }
            return list;
        }

        protected static List<AMMTran> GetDeletedTrans(PXGraph callingGraph, System.Type type)
        {
            var list = new List<AMMTran>();
            foreach (var deletedRow in callingGraph.Caches[type].Deleted)
            {
                var ammDeletedrow = (AMMTran)deletedRow;
                list.Add(ammDeletedrow);
            }
            return list;
        }

        public override void Persist()
        {
            PersistINDeletes();
            PersistGLDeletes();
            PersistAMDeletes();

            base.Persist();
        }

        protected virtual void PersistAMDeletes()
        {
            var amMatlBatNBrs = new UniqueStringCollection();
            var amCostBatNBrs = new UniqueStringCollection();
            foreach (AMBatch row in this.Caches[typeof(AMBatch)].Deleted)
            {
                if (row.DocType == AMDocType.Material && !string.IsNullOrWhiteSpace(row.OrigBatNbr))
                {
                    amMatlBatNBrs.Add(row.BatNbr);
                    continue;
                }

                if (row.DocType == AMDocType.ProdCost)
                {
                    amCostBatNBrs.Add(row.BatNbr);
                }
            }

            var materialTranList = new List<AMMTran>();
            var costTranList = new List<AMMTran>();
            foreach (AMMTran row in this.Caches[typeof(AMMTran)].Deleted)
            {
                if (row.DocType == AMDocType.Material && !string.IsNullOrWhiteSpace(row.OrigBatNbr))
                {
                    amMatlBatNBrs.Add(row.BatNbr);
                    materialTranList.Add(row);
                    continue;
                }

                if (row.DocType == AMDocType.ProdCost)
                {
                    amCostBatNBrs.Add(row.BatNbr);
                    costTranList.Add(row);
                }
            }

            Caches[typeof(AMMTran)].Clear();
            Caches[typeof(AMMTran)].ClearQueryCache();
            Caches[typeof(AMMTranSplit)].Clear();
            Caches[typeof(AMMTranSplit)].ClearQueryCache();
            Caches[typeof(AMBatch)].Clear();
            Caches[typeof(AMBatch)].ClearQueryCache();

            PersistWithMaterialEntry(amMatlBatNBrs, materialTranList);
            PersistWithProductionCostEntry(amCostBatNBrs, costTranList);
        }

        protected virtual void PersistWithMaterialEntry(UniqueStringCollection amMatlBatNBrs, List<AMMTran> ammTranList)
        {
            if (amMatlBatNBrs == null || !amMatlBatNBrs.HasValues)
            {
                return;
            }

            var amMaterialEntryGraph = CreateInstance<MaterialEntry>();
            amMaterialEntryGraph.batch.AllowDelete = true;
            amMaterialEntryGraph.transactions.AllowDelete = true;

            if (amMaterialEntryGraph.ampsetup.Current != null)
            {
                amMaterialEntryGraph.ampsetup.Current.RequireControlTotal = false;
            }

            foreach (var amMatlBatNBr in amMatlBatNBrs)
            {
                amMaterialEntryGraph.Clear();

                AMBatch amBatch = PXSelect<AMBatch,
                    Where<AMBatch.docType, Equal<AMDocType.material>,
                        And<AMBatch.batNbr, Equal<Required<AMBatch.batNbr>>>>>.Select(amMaterialEntryGraph, amMatlBatNBr);

                if (amBatch == null)
                {
                    continue;
                }

                amMaterialEntryGraph.batch.Current = amBatch;

                if (DeleteMode == DeleteModeType.Batch)
                {
                    amMaterialEntryGraph.transactions.Select();
                    amMaterialEntryGraph.batch.Delete(amMaterialEntryGraph.batch.Current);
                    amMaterialEntryGraph.Persist();
                    continue;
                }

                if (ammTranList == null)
                {
                    continue;
                }

                foreach (var ammTran in ammTranList)
                {
                    amMaterialEntryGraph.transactions.Delete(ammTran);
                }

                if (amMaterialEntryGraph.IsDirty)
                {
                    amMaterialEntryGraph.Persist();
                }
            }
        }

        protected virtual void PersistWithProductionCostEntry(UniqueStringCollection amCostBatNBrs, List<AMMTran> ammTranList)
        {
            if (amCostBatNBrs == null || !amCostBatNBrs.HasValues)
            {
                return;
            }

            var amProductionCostEntryGraph = CreateInstance<ProductionCostEntry>();
            amProductionCostEntryGraph.batch.AllowDelete = true;
            amProductionCostEntryGraph.transactions.AllowDelete = true;

            if (amProductionCostEntryGraph.ampsetup.Current != null)
            {
                amProductionCostEntryGraph.ampsetup.Current.RequireControlTotal = false;
            }
            
            foreach (var amCostBatNbrs in amCostBatNBrs)
            {
                amProductionCostEntryGraph.Clear();

                AMBatch amBatch = PXSelect<AMBatch,
                    Where<AMBatch.docType, Equal<AMDocType.prodCost>,
                        And<AMBatch.batNbr, Equal<Required<AMBatch.batNbr>>>>>.Select(amProductionCostEntryGraph, amCostBatNbrs);

                if (amBatch == null)
                {
                    continue;
                }

                amProductionCostEntryGraph.batch.Current = amBatch;

                if (DeleteMode == DeleteModeType.Batch)
                {
                    amProductionCostEntryGraph.transactions.Select();
                    amProductionCostEntryGraph.batch.Delete(amProductionCostEntryGraph.batch.Current);
                    amProductionCostEntryGraph.Persist();
                    continue;
                }

                if (ammTranList == null)
                {
                    continue;
                }

                foreach (var ammTran in ammTranList)
                {
                    amProductionCostEntryGraph.transactions.Delete(ammTran);
                }

                if (amProductionCostEntryGraph.IsDirty)
                {
                    amProductionCostEntryGraph.Persist();
                }
            }
        }

        protected virtual void PersistGLDeletes()
        {
            var glBatchNbrs = new UniqueStringCollection();
            foreach (Batch row in Caches[typeof(Batch)].Deleted)
            {
                glBatchNbrs.Add(row.BatchNbr);
            }

            var glTranList = new List<GLTran>();
            foreach (GLTran row in Caches[typeof(GLTran)].Deleted)
            {
                glBatchNbrs.Add(row.BatchNbr);
                glTranList.Add(row);
            }

            this.Caches[typeof(GLTran)].Clear();
            this.Caches[typeof(GLTran)].ClearQueryCache();
            this.Caches[typeof(Batch)].Clear();
            this.Caches[typeof(Batch)].ClearQueryCache();

            var journalEntry = CreateInstance<JournalEntry>();

            foreach (var glBatchNbr in glBatchNbrs)
            {
                //Process each batch individually
                journalEntry.Clear();
                if (journalEntry.glsetup.Current != null)
                {
                    journalEntry.glsetup.Current.RequireControlTotal = false;
                }
                JournalEntryAMExtension.SetIsInternalCall(journalEntry, true);

                Batch glBatch = PXSelect<Batch,
                        Where<Batch.module, Equal<BatchModule.moduleGL>,
                        And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>,
                            And<Batch.released, Equal<False>>>>
                        >.Select(journalEntry, glBatchNbr);

                if (glBatch == null)
                {
                    continue;
                }

                if (glBatch.Released.GetValueOrDefault())
                {
                    PXTrace.WriteWarning(Messages.GetLocal(Messages.JournalEntryIsReleased, glBatch.BatchNbr));
                    continue;
                }

                journalEntry.BatchModule.Current = glBatch;

                if (DeleteMode == DeleteModeType.Batch)
                {
                    journalEntry.Delete.Press();
                }
                else
                {
                    foreach (var glTran in glTranList.FindAll(x => x.BatchNbr == glBatchNbr && !x.Released.GetValueOrDefault()))
                    {
                        journalEntry.GLTranModuleBatNbr.Delete(glTran);
                    }
                }

                journalEntry.Actions.PressSave();
            }

        }

        protected virtual void PersistINDeletes()
        {
            var receiptRefNbrs = new UniqueStringCollection();
            var issueRefNbrs = new UniqueStringCollection();
            var adjustmentRefNbrs = new UniqueStringCollection();
            foreach (INRegister row in Caches[typeof(INRegister)].Deleted)
            {
                if (row.DocType == INDocType.Receipt)
                {
                    receiptRefNbrs.Add(row.RefNbr);
                    continue;
                }

                if (row.DocType == INDocType.Issue)
                {
                    issueRefNbrs.Add(row.RefNbr);
                    continue;
                }

                if (row.DocType == INDocType.Adjustment)
                {
                    adjustmentRefNbrs.Add(row.RefNbr);
                }
            }

            var inTranLines = new List<INTran>();

            foreach (INTran row in Caches[typeof(INTran)].Deleted)
            {
                if (row.DocType == INDocType.Receipt)
                {
                    receiptRefNbrs.Add(row.RefNbr);
                }

                if (row.DocType == INDocType.Issue)
                {
                    issueRefNbrs.Add(row.RefNbr);
                }

                if (row.DocType == INDocType.Adjustment)
                {
                    adjustmentRefNbrs.Add(row.RefNbr);
                }

                inTranLines.Add(row);
            }

            this.Caches[typeof(INTran)].Clear();
            this.Caches[typeof(INTran)].ClearQueryCache();
            this.Caches[typeof(INRegister)].Clear();
            this.Caches[typeof(INRegister)].ClearQueryCache();

            PersistINReceiptDeletes(receiptRefNbrs, inTranLines, DeleteMode);
            PersistINIssueDeletes(issueRefNbrs, inTranLines, DeleteMode);
            PersistINAdjustmentDeletes(adjustmentRefNbrs, inTranLines, DeleteMode);
        }

        protected static INRegister GetINRegister(PXGraph graph, string inDocType, string refNbr)
        {
            if (string.IsNullOrWhiteSpace(inDocType) || string.IsNullOrWhiteSpace(refNbr))
            {
                return null;
            }

            return PXSelect<INRegister,
                        Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                        And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>.SelectWindowed(graph, 0, 1, inDocType, refNbr);
        }

        protected static List<INTran> GetINTransForRef(string inDocType, string refNbr, List<INTran> inTrans)
        {
            if (string.IsNullOrWhiteSpace(inDocType))
            {
                throw new PXArgumentException("inDocType");
            }
            if (string.IsNullOrWhiteSpace(refNbr))
            {
                throw new PXArgumentException("refNbr");
            }
            if (inTrans == null || inTrans.Count == 0)
            {
                return null;
            }

            return inTrans.FindAll(x => x.DocType == inDocType && x.RefNbr == refNbr);
        }

        /// <summary>
        /// Persist the deletes per each ref nbr
        /// </summary>
        /// <param name="refNbrs">All related IN RefNbrs involved in the delete</param>
        /// <param name="inTrans">All specific transaction lines to be deleted</param>
        /// <param name="deleteModeType">Delete mode (Batch or Line)</param>
        protected static void PersistINReceiptDeletes(UniqueStringCollection refNbrs, List<INTran> inTrans, DeleteModeType deleteModeType)
        {
            var inGraph = PXGraph.CreateInstance<INReceiptEntry>();
            foreach (string receiptRefNbr in refNbrs)
            {
                var inRegister = GetINRegister(inGraph, INDocType.Receipt, receiptRefNbr);
                if (deleteModeType == DeleteModeType.Batch)
                {
                    PersistINReceiptDeletes(inGraph, inRegister);
                    return;
                }
                PersistINReceiptDeletes(inGraph, inRegister, GetINTransForRef(INDocType.Receipt, receiptRefNbr, inTrans));
            }
        }

        protected static INReceiptEntry InitINReceiptEntryGraph(INReceiptEntry graph, INRegister inRegister)
        {
            if (graph == null)
            {
                throw new PXArgumentException("graph");
            }

            if (inRegister == null || inRegister.Released.GetValueOrDefault())
            {
                return null;
            }

            if (inRegister.DocType != INDocType.Receipt)
            {
                throw new PXArgumentException("inRegister");
            }

            graph.Clear();
            if (graph.insetup.Current != null)
            {
                graph.insetup.Current.RequireControlTotal = false;
            }
            graph.receipt.Current = inRegister;
            return graph;
        }

        /// <summary>
        /// Delete an entire IN batch
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inRegister">IN batch record to delete</param>
        protected static void PersistINReceiptDeletes(INReceiptEntry graph, INRegister inRegister)
        {
            var graph2 = InitINReceiptEntryGraph(graph, inRegister);
            if (graph2 == null || graph2.receipt.Current == null)
            {
                return;
            }
            graph2.transactions.Select();
            graph2.receipt.Delete(graph.receipt.Current);
            graph2.Actions.PressSave();
        }

        /// <summary>
        /// Delete specific transaction lines
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inRegister">parent IN register record</param>
        /// <param name="inTrans">lines to be deleted</param>
        protected static void PersistINReceiptDeletes(INReceiptEntry graph, INRegister inRegister, List<INTran> inTrans)
        {
            var graph2 = InitINReceiptEntryGraph(graph, inRegister);
            if (graph2 == null || graph2.receipt.Current == null
                || inTrans == null || inTrans.Count == 0)
            {
                return;
            }

            foreach (INTran inTran in inTrans)
            {
                if (inTran.DocType != inRegister.DocType
                    || !inTran.RefNbr.EqualsWithTrim(inRegister.RefNbr))
                {
                    //safety net
                    continue;
                }

                graph2.transactions.Delete(inTran);
            }

            graph2.Actions.PressSave();
        }

        /// <summary>
        /// Persist the deletes per each ref nbr
        /// </summary>
        /// <param name="refNbrs">All related IN RefNbrs involved in the delete</param>
        /// <param name="inTrans">All specific transaction lines to be deleted</param>
        /// <param name="deleteModeType">Delete mode (Batch or Line)</param>
        protected static void PersistINIssueDeletes(UniqueStringCollection refNbrs, List<INTran> inTrans, DeleteModeType deleteModeType)
        {
            var inGraph = PXGraph.CreateInstance<INIssueEntry>();
            foreach (string receiptRefNbr in refNbrs)
            {
                var inRegister = GetINRegister(inGraph, INDocType.Issue, receiptRefNbr);
                if (deleteModeType == DeleteModeType.Batch)
                {
                    PersistINIssueDeletes(inGraph, inRegister);
                    return;
                }
                PersistINIssueDeletes(inGraph, inRegister, GetINTransForRef(INDocType.Issue, receiptRefNbr, inTrans));
            }
        }

        protected static INIssueEntry InitINIssueEntryGraph(INIssueEntry graph, INRegister inRegister)
        {
            if (graph == null)
            {
                throw new PXArgumentException("graph");
            }

            if (inRegister == null || inRegister.Released.GetValueOrDefault())
            {
                return null;
            }

            if (inRegister.DocType != INDocType.Issue)
            {
                throw new PXArgumentException("inRegister");
            }

            graph.Clear();
            graph.issue.AllowDelete = true;
            graph.transactions.AllowDelete = true;
            if (graph.insetup.Current != null)
            {
                graph.insetup.Current.RequireControlTotal = false;
            }
            graph.issue.Current = inRegister;
            return graph;
        }

        /// <summary>
        /// Delete an entire IN batch
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inRegister">IN batch record to delete</param>
        protected static void PersistINIssueDeletes(INIssueEntry graph, INRegister inRegister)
        {
            var graph2 = InitINIssueEntryGraph(graph, inRegister);
            if (graph2 == null || graph2.issue.Current == null)
            {
                return;
            }
            graph2.transactions.Select();
            graph2.issue.Delete(graph.issue.Current);
            graph2.Actions.PressSave();
        }

        /// <summary>
        /// Delete specific transaction lines
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inRegister">parent IN register record</param>
        /// <param name="inTrans">lines to be deleted</param>
        protected static void PersistINIssueDeletes(INIssueEntry graph, INRegister inRegister, List<INTran> inTrans)
        {
            var graph2 = InitINIssueEntryGraph(graph, inRegister);
            if (graph2 == null || graph2.issue.Current == null
                || inTrans == null || inTrans.Count == 0)
            {
                return;
            }

            foreach (INTran inTran in inTrans)
            {
                if (inTran.DocType != inRegister.DocType
                    || !inTran.RefNbr.EqualsWithTrim(inRegister.RefNbr))
                {
                    //safety net
                    continue;
                }

                graph2.transactions.Delete(inTran);
            }

            graph2.Actions.PressSave();
        }

        /// <summary>
        /// Persist the deletes per each ref nbr
        /// </summary>
        /// <param name="refNbrs">All related IN RefNbrs involved in the delete</param>
        /// <param name="inTrans">All specific transaction lines to be deleted</param>
        /// <param name="deleteModeType">Delete mode (Batch or Line)</param>
        protected static void PersistINAdjustmentDeletes(UniqueStringCollection refNbrs, List<INTran> inTrans, DeleteModeType deleteModeType)
        {
            var inGraph = PXGraph.CreateInstance<INAdjustmentEntry>();
            foreach (string AdjustmentRefNbr in refNbrs)
            {
                var inRegister = GetINRegister(inGraph, INDocType.Adjustment, AdjustmentRefNbr);
                if (deleteModeType == DeleteModeType.Batch)
                {
                    PersistINAdjustmentDeletes(inGraph, inRegister);
                    return;
                }
                PersistINAdjustmentDeletes(inGraph, inRegister, GetINTransForRef(INDocType.Adjustment, AdjustmentRefNbr, inTrans));
            }
        }

        protected static INAdjustmentEntry InitINAdjustmentEntryGraph(INAdjustmentEntry graph, INRegister inRegister)
        {
            if (graph == null)
            {
                throw new PXArgumentException("graph");
            }

            if (inRegister == null || inRegister.Released.GetValueOrDefault())
            {
                return null;
            }

            if (inRegister.DocType != INDocType.Adjustment)
            {
                throw new PXArgumentException("inRegister");
            }

            graph.Clear();
            if (graph.insetup.Current != null)
            {
                graph.insetup.Current.RequireControlTotal = false;
            }
            graph.adjustment.Current = inRegister;
            return graph;
        }

        /// <summary>
        /// Delete an entire IN batch
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inRegister">IN batch record to delete</param>
        protected static void PersistINAdjustmentDeletes(INAdjustmentEntry graph, INRegister inRegister)
        {
            var graph2 = InitINAdjustmentEntryGraph(graph, inRegister);
            if (graph2 == null || graph2.adjustment.Current == null)
            {
                return;
            }
            graph2.transactions.Select();
            graph2.adjustment.Delete(graph.adjustment.Current);
            graph2.Actions.PressSave();
        }

        /// <summary>
        /// Delete specific transaction lines
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inRegister">parent IN register record</param>
        /// <param name="inTrans">lines to be deleted</param>
        protected static void PersistINAdjustmentDeletes(INAdjustmentEntry graph, INRegister inRegister, List<INTran> inTrans)
        {
            var graph2 = InitINAdjustmentEntryGraph(graph, inRegister);
            if (graph2 == null || graph2.adjustment.Current == null
                || inTrans == null || inTrans.Count == 0)
            {
                return;
            }

            foreach (INTran inTran in inTrans)
            {
                if (inTran.DocType != inRegister.DocType
                    || !inTran.RefNbr.EqualsWithTrim(inRegister.RefNbr))
                {
                    //safety net
                    continue;
                }

                graph2.transactions.Delete(inTran);
            }

            graph2.Actions.PressSave();
        }

        public virtual void LoadFromDeletedAMBatch(AMBatch amBatch)
        {
            DeleteRelatedAMTransactions(this, amBatch, true);
            DeleteRelatedINTransacitons(this, amBatch);
            DeleteRelatedGLTransacitons(this, amBatch);
        }

        public virtual void LoadFromDeletedAMMTran(AMMTran ammTran)
        {
            DeleteRelatedAMTransactionLines(this, ammTran, true);
            DeleteRelatedINTransacitonLines(this, ammTran);
            DeleteRelatedGLTransacitonLines(this, ammTran);
        }

        public static void DeleteRelatedAMTransactions(PXGraph graph, AMBatch deletedAMBatch, bool deleteRelatedInGl = false)
        {
            if (graph == null || deletedAMBatch == null)
            {
                return;
            }

            foreach (AMBatch childAMBatch in AMBatchReferences(graph, deletedAMBatch, false))
            {
                if (deleteRelatedInGl)
                {
                    DeleteRelatedINTransacitons(graph, childAMBatch);
                    DeleteRelatedGLTransacitons(graph, childAMBatch);
                }

                graph.Caches[typeof(AMBatch)].Delete(childAMBatch);
            }
        }

        public static void DeleteRelatedAMTransactionLines(PXGraph graph, AMMTran deletedAmmTran, bool deleteRelatedInGl = false)
        {
            if (graph == null || deletedAmmTran == null)
            {
                return;
            }

            foreach (AMMTran childAmmTran in AMLineReferences(graph, deletedAmmTran))
            {
                if (childAmmTran.Released ?? false)
                {
                    //Deleting a line with related transactions that are released
                    CreateProdEventDeletingReleasedRefBatch(graph, deletedAmmTran, childAmmTran);
                    continue;
                }

                if (deleteRelatedInGl)
                {
                    DeleteRelatedINTransacitonLines(graph, childAmmTran);
                    DeleteRelatedGLTransacitonLines(graph, childAmmTran);
                }
                graph.Caches[typeof(AMMTran)].Delete(childAmmTran);
            }
        }

        public static void DeleteRelatedINTransacitons(PXGraph graph, AMBatch deletedAMBatch)
        {
            if (graph == null || deletedAMBatch == null)
            {
                return;
            }

            foreach (INRegister inRegister in INBatchReferences(graph, deletedAMBatch, false))
            {
                graph.Caches[typeof(INRegister)].Delete(inRegister);
            }
        }

        public static void DeleteRelatedINTransacitonLines(PXGraph graph, AMMTran deletedAmmTran)
        {
            if (graph == null || deletedAmmTran == null)
            {
                return;
            }

            foreach (INTran inTran in INLineReferences(graph, deletedAmmTran))
            {
                if (inTran.Released ?? false)
                {
                    //Deleting a line with related transactions that are released
                    CreateProdEventDeletingReleasedRefBatch(graph, deletedAmmTran, inTran);
                    return;
                }

                graph.Caches[typeof(INTran)].Delete(inTran);
            }
        }

        public static void DeleteRelatedGLTransacitons(PXGraph graph, AMBatch deletedAMBatch)
        {
            if (graph == null || deletedAMBatch == null)
            {
                return;
            }

            foreach (Batch batch in GLBatchReferences(graph, deletedAMBatch))
            {
                graph.Caches[typeof(Batch)].Delete(batch);
            }
        }

        public static void DeleteRelatedGLTransacitonLines(PXGraph graph, AMMTran deletedAmmTran)
        {
            if (graph == null || deletedAmmTran == null)
            {
                return;
            }

            foreach (GLTran glTran in GLLineReferences(graph, deletedAmmTran))
            {
                if (glTran.Released ?? false)
                {
                    //Deleting a line with related transactions that are released
                    CreateProdEventDeletingReleasedRefBatch(graph, deletedAmmTran, glTran);
                    continue;
                }

                graph.Caches[typeof(GLTran)].Delete(glTran);
            }
        }

        /// <summary>
        /// Indicates if given AMMTran record contains related transactions that have been released
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="ammTran">Record to loopup related transactions</param>
        /// <returns>True if released records found</returns>
        public static bool HasReleasedReferenceDocs(PXGraph graph, AMMTran ammTran)
        {
            var amCheck = AMLineReferences(graph, ammTran, true);

            if (amCheck != null && amCheck.Count > 0)
            {
                return true;
            }

            var inCheck = INLineReferences(graph, ammTran, true);

            if (inCheck != null && inCheck.Count > 0)
            {
                return true;
            }

            var glCheck = GLLineReferences(graph, ammTran, true);

            if (glCheck != null && glCheck.Count > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Indicates if given AMMTran record contains related transactions that have been released
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="ammTran">Record to loopup related transactions</param>
        /// <returns>True if released records found</returns>
        public static bool HasReleasedReferenceDocs(PXGraph graph, AMBatch amBatch)
        {
            var amCheck = AMBatchReferences(graph, amBatch, true);

            if (amCheck != null && amCheck.Count > 0)
            {
                return true;
            }

            var inCheck = INBatchReferences(graph, amBatch, true);

            if (inCheck != null && inCheck.Count > 0)
            {
                return true;
            }

            var glCheck = GLBatchReferences(graph, amBatch, true);

            if (glCheck != null && glCheck.Count > 0)
            {
                return true;
            }

            return false;
        }


        public static PXResultset<Batch> GLBatchReferences(PXGraph graph, AMBatch amBatch, bool? released = null)
        {
            if (amBatch == null)
            {
                return null;
            }

            return GLBatchReferences(graph, amBatch.DocType, amBatch.BatNbr, released);
        }

        public static PXResultset<Batch> GLBatchReferences(PXGraph graph, AMMTran ammTran, bool? released = null)
        {
            if (ammTran == null)
            {
                return null;
            }

            return GLBatchReferences(graph, ammTran.DocType, ammTran.BatNbr, released);
        }

        public static PXResultset<Batch> GLBatchReferences(PXGraph graph, string amDocType, string amBatNbr, bool? released = null)
        {
            if (graph == null)
            {
                return null;
            }

            PXSelectBase<Batch> cmd = new PXSelect<Batch,
                 Where<BatchExt.aMDocType, Equal<Required<BatchExt.aMDocType>>,
                     And<BatchExt.aMBatNbr, Equal<Required<BatchExt.aMBatNbr>>>>>(graph);

            if (released != null)
            {
                if (released == true)
                {
                    cmd.WhereAnd<Where<Batch.released, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<Batch.released, Equal<False>>>();
                }
            }

            return cmd.Select(amDocType, amBatNbr);
        }

        public static PXResultset<GLTran> GLLineReferences(PXGraph graph, AMMTran ammTran, bool? released = null)
        {
            PXSelectBase<GLTran> cmd = new PXSelect<GLTran,
                Where<GLTran.module, Equal<BatchModule.moduleGL>,
                    And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
                        And<GLTran.origBatchNbr, Equal<Required<GLTran.origBatchNbr>>,
                            And<GLTran.origLineNbr, Equal<Required<GLTran.origLineNbr>>
                                >>>>>(graph);

            if (released != null)
            {
                if (released == true)
                {
                    cmd.WhereAnd<Where<GLTran.released, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<GLTran.released, Equal<False>>>();
                }
            }

            return cmd.Select(ammTran.GLBatNbr, ammTran.BatNbr, ammTran.LineNbr);
        }


        public static PXResultset<INRegister> INBatchReferences(PXGraph graph, AMBatch amBatch, bool? released = null)
        {
            if (amBatch == null)
            {
                return null;
            }

            return INBatchReferences(graph, amBatch.DocType, amBatch.BatNbr, released);
        }

        public static PXResultset<INRegister> INBatchReferences(PXGraph graph, AMMTran ammTran, bool? released = null)
        {
            if (ammTran == null)
            {
                return null;
            }

            return INBatchReferences(graph, ammTran.DocType, ammTran.BatNbr, released);
        }

        public static PXResultset<INRegister> INBatchReferences(PXGraph graph, string amDocType, string amBatNbr, bool? released = null)
        {
            if (graph == null)
            {
                return null;
            }

            PXSelectBase<INRegister> cmd = new PXSelect<INRegister,
                 Where<INRegisterExt.aMDocType, Equal<Required<INRegisterExt.aMDocType>>,
                     And<INRegisterExt.aMBatNbr, Equal<Required<INRegisterExt.aMBatNbr>>>>>(graph);

            if (released != null)
            {
                if (released == true)
                {
                    cmd.WhereAnd<Where<INRegister.released, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<INRegister.released, Equal<False>>>();
                }
            }

            return cmd.Select(amDocType, amBatNbr);
        }

        public static PXResultset<INTran> INLineReferences(PXGraph graph, AMMTran ammTran, bool? released = null)
        {
            PXSelectBase<INTran> cmd = new PXSelect<INTran,
                Where<INTran.docType, Equal<Required<INTran.docType>>,
                    And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                        And<INTran.lineNbr, Equal<Required<INTran.lineNbr>>>>>>(graph);

            if (released != null)
            {
                if (released == true)
                {
                    cmd.WhereAnd<Where<INTran.released, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<INTran.released, Equal<False>>>();
                }
            }

            return cmd.Select(ammTran.INDocType, ammTran.INBatNbr, ammTran.INLineNbr);
        }

        public static PXResultset<AMBatch> AMBatchReferences(PXGraph graph, AMBatch amBatch, bool? released = null)
        {
            if (graph == null || amBatch == null)
            {
                return null;
            }

            PXSelectBase<AMBatch> cmd = new PXSelect<AMBatch,
                Where<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>,
                    And<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>>>>(graph);

            if (released != null)
            {
                if (released == true)
                {
                    cmd.WhereAnd<Where<AMBatch.released, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<AMBatch.released, Equal<False>>>();
                }
            }

            return cmd.Select(amBatch.DocType, amBatch.BatNbr);
        }

        public static PXResultset<AMMTran> AMLineReferences(PXGraph graph, AMMTran ammTran, bool? released = null)
        {
            PXSelectBase<AMMTran> cmd = new PXSelect<AMMTran,
                Where<AMMTran.origDocType, Equal<Required<AMMTran.origDocType>>,
                    And<AMMTran.origBatNbr, Equal<Required<AMMTran.origBatNbr>>,
                        And<AMMTran.origLineNbr, Equal<Required<AMMTran.origLineNbr>>>>>>(graph);

            if (released != null)
            {
                if (released == true)
                {
                    cmd.WhereAnd<Where<AMMTran.released, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<AMMTran.released, Equal<False>>>();
                }
            }

            return cmd.Select(ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr);
        }

        #region Production Event History Creation
        protected static void CreateProdEventDeletingReleasedRefBatch(PXGraph graph, AMMTran deletedAmmTran, AMMTran releasedAmmTran)
        {
            if (deletedAmmTran == null || releasedAmmTran == null)
            {
                return;
            }

            CreateProdEventDeletingReleasedRefBatch(graph, deletedAmmTran.OrderType, deletedAmmTran.ProdOrdID, deletedAmmTran.DocType,
                deletedAmmTran.BatNbr, Common.ModuleAM, releasedAmmTran.DocType, releasedAmmTran.BatNbr);
        }

        protected static void CreateProdEventDeletingReleasedRefBatch(PXGraph graph, AMMTran deletedAmmTran, INTran releasedInTran)
        {
            if (deletedAmmTran == null || releasedInTran == null)
            {
                return;
            }

            CreateProdEventDeletingReleasedRefBatch(graph, deletedAmmTran.OrderType, deletedAmmTran.ProdOrdID, deletedAmmTran.DocType,
                deletedAmmTran.BatNbr, BatchModule.IN, releasedInTran.DocType, releasedInTran.RefNbr);
        }

        protected static void CreateProdEventDeletingReleasedRefBatch(PXGraph graph, AMMTran deletedAmmTran, GLTran releasedGlTran)
        {
            if (deletedAmmTran == null || releasedGlTran == null)
            {
                return;
            }

            CreateProdEventDeletingReleasedRefBatch(graph, deletedAmmTran.OrderType, deletedAmmTran.ProdOrdID, deletedAmmTran.DocType,
                deletedAmmTran.BatNbr, BatchModule.GL, BatchModule.GL, releasedGlTran.BatchNbr);
        }

        protected static void CreateProdEventDeletingReleasedRefBatch(PXGraph graph, string orderType, string prodOrdID,
            string deletedDocType, string deletedBatNbr, string releasedModule, string releasedDocType, string releasedBatNbr)
        {
            if (graph == null || string.IsNullOrWhiteSpace(prodOrdID) || string.IsNullOrWhiteSpace(orderType))
            {
                return;
            }

            string eventMessage = MakeEventMessage(deletedDocType, deletedBatNbr, releasedModule,
                releasedDocType, releasedBatNbr);

            if (string.IsNullOrWhiteSpace(eventMessage))
            {
                return;
            }

            ProductionEventHelper.InsertInformationEvent(graph, eventMessage, prodOrdID, orderType);
        }

        /// <summary>
        /// Build the event message string to be used in the Production Event
        /// </summary>
        protected static string MakeEventMessage(string deletedDocType, string deletedBatNbr, string releasedModule, string releasedDocType, string releasedBatNbr)
        {
            if (string.IsNullOrWhiteSpace(deletedBatNbr)
                || string.IsNullOrWhiteSpace(deletedBatNbr)
                || string.IsNullOrWhiteSpace(releasedDocType)
                || string.IsNullOrWhiteSpace(releasedBatNbr))
            {
                return string.Empty;
            }

            string releasedDoctypeDesc = releasedDocType;

            switch (releasedModule)
            {
                case Common.ModuleAM:
                    releasedDoctypeDesc = AMDocType.GetDocTypeDesc(releasedDocType);
                    break;
                case BatchModule.GL:
                    releasedDoctypeDesc = Messages.Journal;
                    break;
                case BatchModule.IN:
                    releasedDoctypeDesc = releasedDocType == INDocType.Receipt ? Messages.TranTypeReceipt : Messages.TranTypeIssue;
                    break;
            }

            return Messages.GetLocal(Messages.DeletedTransactionWithReference, AMDocType.GetDocTypeDesc(deletedDocType), deletedBatNbr, releasedModule, releasedDoctypeDesc, releasedBatNbr);
        }
        #endregion

    }
}
