using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing builder of inventory transactions for both stock and non stock items
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InventoryTranBuilder
    {
        public enum TransactionType
        {
            None,
            IssueReturn,
            Receipt,
            Adjustment
        }

        #region Properties

        public bool SummaryPostGl;
        public DateTime? TranDate;
        public string PostPeriod;
        public string TransactionDescription;
        public INTran CurrentInTran { get; private set; }
        public bool HasInTransactions => InTransactionCount > 0;

        /// <summary>
        /// Reference back to the source document
        /// </summary>
        internal Guid? RefDocNoteId;

        /// <summary>
        /// Reference back to the source type
        /// </summary>
        internal string RefDocEntityType;

        /// <summary>
        /// To use as a reference doc (hyper link) a note record must exist.
        /// This indicates if in the process of creating inventory transaction the RefDocNoteId referenced was used and indicates (true) a note record is needed
        /// </summary>
        internal bool RefDocNoteRecordRequired;

        protected int InTransactionCount
        {
            get
            {
                if (_transactionType == TransactionType.IssueReturn)
                {
                    return ((INIssueEntry)_inGraph)?.transactions?.Select()?.Count ?? 0;
                }
                if (_transactionType == TransactionType.Receipt)
                {
                    return ((INReceiptEntry)_inGraph)?.transactions?.Select()?.Count ?? 0;
                }
                return ((INAdjustmentEntry)_inGraph)?.transactions?.Select()?.Count ?? 0;
            }
        }

        public bool HasGlTransactions => GlTransactionCount > 0;
        protected int GlTransactionCount => _glGraph?.GLTranModuleBatNbr?.Select()?.Count ?? 0;

        public INRegister CurrentInDocument
        {
            get
            {
                if (_transactionType == TransactionType.IssueReturn)
                {
                    return ((INIssueEntry) _inGraph).issue.Current;
                }
                if (_transactionType == TransactionType.Receipt)
                {
                    return ((INReceiptEntry)_inGraph).receipt.Current;
                }
                return ((INAdjustmentEntry)_inGraph).adjustment.Current;
            }
        }

        public Batch CurrentGlDocument => _glGraph?.BatchModule?.Current;

        private PXGraph _inGraph;
        private JournalEntry _glGraph;

        public JournalEntry GlGraph
        {
            get
            {
                if (_glGraph != null)
                {
                    return _glGraph;
                }
                return ConstructGlGraph();
            }
            set
            {
                _glGraph = value;
                if (_glGraph?.glsetup?.Current?.RequireControlTotal != null)
                {
                    _glGraph.glsetup.Current.RequireControlTotal = false;
                }
            }
        }

        /// <summary>
        /// Graph contains a journal entry batch
        /// </summary>
        public bool GlGraphLoaded => _glGraph?.glsetup?.Current != null;

        /// <summary>
        /// Creates the instance for GlGraph
        /// </summary>
        /// <returns></returns>
        public JournalEntry ConstructGlGraph()
        {
            return _glGraph = ConstructJournalEntry();
        }

        public static JournalEntry ConstructJournalEntry()
        {
            var graph = PXGraph.CreateInstance<JournalEntry>();
            graph.Clear();
            JournalEntryAMExtension.SetIsInternalCall(graph, true);
            graph.glsetup.Current.RequireControlTotal = false;
            return graph;
        }

        private TransactionType _transactionType;
        private string DebuggerDisplay => $"Builder: {Enum.GetName(typeof(TransactionType), _transactionType)}, InTransactions = {InTransactionCount}, GLTransactions = {GlTransactionCount}";

        #endregion

        #region CTOR

        public InventoryTranBuilder(TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.IssueReturn:
                    INIssueEntry issueEntryGraph = PXGraph.CreateInstance<INIssueEntry>();
                    issueEntryGraph.Clear();
                    InitEntryGraph(issueEntryGraph);
                    break;

                case TransactionType.Receipt:
                    INReceiptEntry receiptEntryGraph = PXGraph.CreateInstance<INReceiptEntry>();
                    receiptEntryGraph.Clear();
                    InitEntryGraph(receiptEntryGraph);
                    break;

                case TransactionType.Adjustment:
                    INAdjustmentEntry adjustmentEntry = PXGraph.CreateInstance<INAdjustmentEntry>();
                    adjustmentEntry.Clear();
                    InitEntryGraph(adjustmentEntry);
                    break;

                default:
                    throw new PXException(Messages.InvalidTranTypeForBuilding);
            }
        }

        public InventoryTranBuilder(INIssueEntry entryGraph)
        {
            InitEntryGraph(entryGraph);
        }

        public InventoryTranBuilder(INReceiptEntry entryGraph)
        {
            InitEntryGraph(entryGraph);
        }

        public InventoryTranBuilder(INAdjustmentEntry entryGraph)
        {
            InitEntryGraph(entryGraph);
        }

        #endregion

        protected void InitEntryGraph(INIssueEntry inIssueEntryGraph)
        {
            CheckGraph(inIssueEntryGraph);

            inIssueEntryGraph.insetup.Current.RequireControlTotal = false;
            CurrentInTran = inIssueEntryGraph.transactions.Current;

            _inGraph = inIssueEntryGraph;
            _transactionType = TransactionType.IssueReturn;

            Init();
        }

        protected void InitEntryGraph(INReceiptEntry inReceiptEntryGraph)
        {
            CheckGraph(inReceiptEntryGraph);

            inReceiptEntryGraph.insetup.Current.RequireControlTotal = false;
            CurrentInTran = inReceiptEntryGraph.transactions.Current;

            _inGraph = inReceiptEntryGraph;
            _transactionType = TransactionType.Receipt;

            Init();
        }

        protected void InitEntryGraph(INAdjustmentEntry inAdjustmentEntryGraph)
        {
            CheckGraph(inAdjustmentEntryGraph);

            inAdjustmentEntryGraph.insetup.Current.RequireControlTotal = false;
            CurrentInTran = inAdjustmentEntryGraph.transactions.Current;

            _inGraph = inAdjustmentEntryGraph;
            _transactionType = TransactionType.Adjustment;

            Init();
        }

        protected void Init()
        {
            // Cancel Filed Verifying on Reason Code to allow scrap reason codes to be persisted
            _inGraph.FieldVerifying.AddHandler<INTran.reasonCode>((sender, e) => { e.Cancel = true; });

            // Required to ignore the location restrictor. Location restriction already taken place on the production transaction. Ex: scrap IN Receipt needs to allow locations not allow for receipt.
            _inGraph.FieldVerifying.AddHandler<INTran.locationID>((sender, e) => { e.Cancel = true; });
            _inGraph.FieldVerifying.AddHandler<INTranSplit.locationID>((sender, e) => { e.Cancel = true; });

            var ampSetup = (AMPSetup)PXSelect<AMPSetup>.Select(_inGraph);

            SummaryPostGl = ampSetup.SummPost.GetValueOrDefault();

            CurrentInTran = new INTran();
            TransactionDescription = Messages.ProdGLEntry_ProdTranGeneric;

            TranDate = Common.Current.BusinessDate(_inGraph);
            PostPeriod = ProductionTransactionHelper.PeriodFromDate(_inGraph, TranDate);
        }

        protected virtual void CheckGraph(PXGraph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }
        }

        protected bool IsGLReleased
        {
            get { return GlGraph.GLTranModuleBatNbr.Current != null && GlGraph.GLTranModuleBatNbr.Current.Released.GetValueOrDefault(); }
        }

        protected bool IsINRefReleased
        {
            get
            {
                if (_transactionType == TransactionType.IssueReturn)
                {
                    return ((INIssueEntry) _inGraph).issue.Current != null &&
                           ((INIssueEntry) _inGraph).issue.Current.Released.GetValueOrDefault();
                }

                if (_transactionType == TransactionType.Receipt)
                {
                    return ((INReceiptEntry) _inGraph).receipt.Current != null &&
                           ((INReceiptEntry) _inGraph).receipt.Current.Released.GetValueOrDefault();
                }

                if (_transactionType == TransactionType.Adjustment)
                {
                    return ((INAdjustmentEntry)_inGraph).adjustment.Current != null &&
                           ((INAdjustmentEntry)_inGraph).adjustment.Current.Released.GetValueOrDefault();
                }

                return false;
            }
        }

        public virtual void UpdateInRegister()
        {
            if (_transactionType == TransactionType.IssueReturn && ((INIssueEntry)_inGraph).issue.Current != null)
            {
                INRegister register = ((INIssueEntry) _inGraph).issue.Current;

                if (register.Status != BatchStatus.Balanced && register.Status != BatchStatus.Hold)
                {
                    return;
                }

                register.OrigModule = Common.ModuleAM;
                register.TranDate = TranDate;
                register.FinPeriodID = PostPeriod;
                register.Hold = true;

                ((INIssueEntry)_inGraph).issue.Update(register);
                return;
            }

            if (_transactionType == TransactionType.Receipt && ((INReceiptEntry)_inGraph).receipt.Current != null)
            {
                INRegister register = ((INReceiptEntry)_inGraph).receipt.Current;

                if (register.Status != BatchStatus.Balanced && register.Status != BatchStatus.Hold)
                {
                    return;
                }

                register.OrigModule = Common.ModuleAM;
                register.TranDate = TranDate;
                register.FinPeriodID = PostPeriod;
                register.Hold = true;

                ((INReceiptEntry)_inGraph).receipt.Update(register);
                return;
            }

            if (_transactionType == TransactionType.Adjustment && ((INAdjustmentEntry)_inGraph).adjustment.Current != null)
            {
                INRegister register = ((INAdjustmentEntry)_inGraph).adjustment.Current;

                if (register.Status != BatchStatus.Balanced && register.Status != BatchStatus.Hold)
                {
                    return;
                }

                register.OrigModule = Common.ModuleAM;
                register.TranDate = TranDate;
                register.FinPeriodID = PostPeriod;
                register.Hold = true;

                ((INAdjustmentEntry)_inGraph).adjustment.Update(register);
            }
        }

        protected virtual bool CreateInRegister(string docType, string batNbr)
        {           
            //Create only on first line

            if (_transactionType == TransactionType.IssueReturn && ((INIssueEntry)_inGraph).issue.Current == null)
            {
                INRegister register = new INRegister();
                register = PXCache<INRegister>.CreateCopy(((INIssueEntry) _inGraph).issue.Insert(register));
                register.Hold = true;
                register.Status = BatchStatus.Balanced;
                register.OrigModule = Common.ModuleAM;
                register.TranDesc = TransactionDescription;
                register.TranDate = TranDate;
                register.FinPeriodID = PostPeriod;

                if (!string.IsNullOrWhiteSpace(docType) && !string.IsNullOrWhiteSpace(batNbr))
                {
                    var extension = PXCache<INRegister>.GetExtension<INRegisterExt>(register);
                    if (extension != null)
                    {
                        extension.AMDocType = docType;
                        extension.AMBatNbr = batNbr;
                    }
                }

                ((INIssueEntry) _inGraph).issue.Update(register);
                ((INIssueEntry) _inGraph).Persist();
                return true;
            }

            if (_transactionType == TransactionType.Receipt && ((INReceiptEntry)_inGraph).receipt.Current == null)
            {
                INRegister register = new INRegister();
                register = PXCache<INRegister>.CreateCopy(((INReceiptEntry)_inGraph).receipt.Insert(register));
                register.Hold = true;
                register.Status = BatchStatus.Balanced;
                register.OrigModule = Common.ModuleAM;
                register.TranDesc = TransactionDescription;
                register.TranDate = TranDate;
                register.FinPeriodID = PostPeriod;

                if (!string.IsNullOrWhiteSpace(docType) && !string.IsNullOrWhiteSpace(batNbr))
                {
                    var extension = PXCache<INRegister>.GetExtension<INRegisterExt>(register);
                    if (extension != null)
                    {
                        extension.AMDocType = docType;
                        extension.AMBatNbr = batNbr;
                    }
                }

                ((INReceiptEntry)_inGraph).receipt.Update(register);
                ((INReceiptEntry)_inGraph).Persist();
                return true;
            }

            if (_transactionType == TransactionType.Adjustment && ((INAdjustmentEntry)_inGraph).adjustment.Current == null)
            {
                INRegister register = new INRegister();
                register = PXCache<INRegister>.CreateCopy(((INAdjustmentEntry)_inGraph).adjustment.Insert(register));
                register.Hold = true;
                register.Status = BatchStatus.Balanced;
                register.OrigModule = Common.ModuleAM;
                register.TranDesc = TransactionDescription;
                register.TranDate = TranDate;
                register.FinPeriodID = PostPeriod;

                if (!string.IsNullOrWhiteSpace(docType) && !string.IsNullOrWhiteSpace(batNbr))
                {
                    var extension = PXCache<INRegister>.GetExtension<INRegisterExt>(register);
                    if (extension != null)
                    {
                        extension.AMDocType = docType;
                        extension.AMBatNbr = batNbr;
                    }
                }

                ((INAdjustmentEntry)_inGraph).adjustment.Update(register);
                ((INAdjustmentEntry)_inGraph).Persist();
                return true;
            }

            return false;
        }

        public void SyncBatch(AMBatch doc)
        {
            SyncINRegister(doc);
            SyncGLBatch(doc);
        }

        private void SyncINRegister(AMBatch doc)
        {
            if (CurrentInDocument == null)
            {
                return;
            }

            var inRegister = (INRegister)_inGraph.Caches[typeof(INRegister)].CreateCopy(CurrentInDocument);
            inRegister.TranDate = doc.TranDate;
            inRegister.FinPeriodID = doc.FinPeriodID;
            inRegister.TranDesc = doc.TranDesc;
            _inGraph.Caches[typeof(INRegister)].Update(inRegister);
        }

        private void SyncGLBatch(AMBatch doc)
        {
            if (CurrentGlDocument == null)
            {
                return;
            }

            var glbatch = (Batch)GlGraph.BatchModule.Cache.CreateCopy(CurrentGlDocument);
            glbatch.DateEntered = doc.TranDate;
            glbatch.FinPeriodID = doc.FinPeriodID;
            glbatch.Description = doc.TranDesc;
            GlGraph.BatchModule.Update(glbatch);
        }

        public virtual void Save()
        {
            if (HasInTransactions && _inGraph.IsDirty && !IsINRefReleased)
            {
                try
                {
                    _inGraph.Actions.PressSave();
                }
                catch (Exception e)
                {
                    PXTrace.WriteError(Messages.GetLocal(Messages.ErrorSavingTransaction, _inGraph.GetType().Name, e.Message));
                    throw;
                }
            }

            if (HasGlTransactions && GlGraph.IsDirty && !IsGLReleased)
            {
                try
                {
                    GlGraph.Actions.PressSave();
                }
                catch (Exception e)
                {
                    PXTrace.WriteError(Messages.GetLocal(Messages.ErrorSavingTransaction, GlGraph.GetType().Name, e.Message));
                    throw;
                }
            }
        }

        protected virtual bool IsStockItem(int? inventoryID)
        {
            InventoryItem inventoryItem = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(_inGraph, inventoryID );

            if (inventoryItem == null)
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                        Common.Cache.GetCacheName(typeof(InventoryItem))));
            }

            return inventoryItem.StkItem.GetValueOrDefault();
        }

        private INTranSplit InsertSplit(INTranSplit inTranSplit)
        {
            if (_transactionType == TransactionType.IssueReturn)
            {
                return PXCache<INTranSplit>.CreateCopy(((INIssueEntry)_inGraph).splits.Insert(inTranSplit));
            }
            if (_transactionType == TransactionType.Receipt)
            {
                return PXCache<INTranSplit>.CreateCopy(((INReceiptEntry)_inGraph).splits.Insert(inTranSplit));
            }
            //
            //  Adjustments do not allow for multiple splits. Set the lines lot/serial number field and we should be ok to not worry about the splits
            //

            return null;
        }

        private INTranSplit UpdateSplit(INTranSplit inTranSplit)
        {
            if (_transactionType == TransactionType.IssueReturn)
            {
                return PXCache<INTranSplit>.CreateCopy(((INIssueEntry)_inGraph).splits.Update(inTranSplit));
            }
            if (_transactionType == TransactionType.Receipt)
            {
                return PXCache<INTranSplit>.CreateCopy(((INReceiptEntry)_inGraph).splits.Update(inTranSplit));
            }
            //
            //  Adjustments do not allow for multiple splits. Set the lines lot/serial number field and we should be ok to not worry about the splits
            //

            return null;
        }

        private INTran InsertLine(INTran inTran)
        {
            if (inTran == null)
            {
                return null;
            }
            if (_transactionType == TransactionType.IssueReturn)
            {
                return PXCache<INTran>.CreateCopy(((INIssueEntry)_inGraph).transactions.Insert(inTran));
            }
            if (_transactionType == TransactionType.Receipt)
            {
                return PXCache<INTran>.CreateCopy(((INReceiptEntry)_inGraph).transactions.Insert(inTran));
            }
            if (_transactionType == TransactionType.Adjustment)
            {
                return PXCache<INTran>.CreateCopy(((INAdjustmentEntry)_inGraph).transactions.Insert(inTran));
            }

            return null;
        }

        protected virtual void SetINReference(ref AMMTran ammTran, ref INTran inTran)
        {
            if (ammTran == null || inTran == null)
            {
                return;
            }

            ammTran.INDocType = inTran.DocType;
            ammTran.INBatNbr = inTran.RefNbr;
            ammTran.INLineNbr = inTran.LineNbr;

            var inTranExtension = inTran.GetExtension<INTranExt>();

            if (inTranExtension == null)
            {
                return;
            }

            inTranExtension.AMDocType = ammTran.DocType;
            inTranExtension.AMBatNbr = ammTran.BatNbr;
            inTranExtension.AMLineNbr = ammTran.LineNbr;
            inTranExtension.AMOrderType = ammTran.OrderType;
            inTranExtension.AMProdOrdID = ammTran.ProdOrdID;
        }

        /// <summary>
        /// Delete/remove Inventory related reference for current AMMTran
        /// </summary>
        /// <param name="ammTran"></param>
        /// <returns>True if IN Record marked for deletion</returns>
        protected virtual bool RemoveINReference(ref AMMTran ammTran)
        {
            if (ammTran == null)
            {
                return false;
            }

            var deleted = DeleteINReferences(ammTran);

            ammTran.INDocType = null;
            ammTran.INBatNbr = null;
            ammTran.INLineNbr = null;

            return deleted;
        }

        protected virtual AMMTran RemoveINReferenceLegacy(AMMTran ammTran)
        {
            if (ammTran == null)
            {
                return null;
            }

            DeleteINReferencesLegacy(ammTran);

            ammTran.INDocType = null;
            ammTran.INBatNbr = null;
            ammTran.INLineNbr = null;
            return ammTran;
        }

        /// <summary>
        /// Delete the INTran records related to the passed AMMTran record.
        /// This uses the non AEF method for finding the records to delete (Match per INTran values)
        /// </summary>
        /// <param name="ammTran"></param>
        /// <returns>True when INTran rows marked for delete</returns>
        protected virtual bool DeleteINReferencesLegacy(AMMTran ammTran)
        {
            bool boolReturn = false;
            if (ammTran == null)
            {
                return boolReturn;
            }

            if (!string.IsNullOrEmpty(ammTran.INDocType) && !string.IsNullOrEmpty(ammTran.INBatNbr) &&
                ammTran.INLineNbr != null)
            {
                //will this automatically delete the splits?

                INTran inTran = PXSelect<INTran,
                Where<INTran.docType, Equal<Required<INTran.docType>>,
                    And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                    And<INTran.lineNbr, Equal<Required<INTran.lineNbr>>,
                        And<INTran.released, Equal<False>>>>>
                    >.Select(_inGraph, ammTran.INDocType, ammTran.INBatNbr, ammTran.INLineNbr);

                if (inTran != null)
                {
                    _inGraph.Caches[typeof(INTran)].Delete(inTran);
                    boolReturn = true;
                }
            }
            return boolReturn;
        }

        /// <summary>
        /// Delete the INTran records related to the passed AMMTran record.
        /// This uses the AEF references which is a better lookup as this will find multiple INTrans per single AMMTran which could 
        ///  occur in adjustment or similar transactions (many INTran to single AMMTran).
        /// </summary>
        /// <param name="ammTran"></param>
        /// <returns>True when INTran rows marked for delete</returns>
        protected virtual bool DeleteINReferences(AMMTran ammTran)
        {
            var boolReturn = false;
            if (ammTran == null ||
                string.IsNullOrEmpty(ammTran.INDocType) || 
                string.IsNullOrEmpty(ammTran.INBatNbr) ||
                ammTran.INLineNbr == null)
            {
                return boolReturn;
            }

            foreach (var inTran in PXSelect<INTran,
                Where<INTran.docType, Equal<Required<INTran.docType>>,
                    And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                        And<INTranExt.aMDocType, Equal<Required<INTranExt.aMDocType>>,
                            And<INTranExt.aMBatNbr, Equal<Required<INTranExt.aMBatNbr>>,
                                And<INTranExt.aMLineNbr, Equal<Required<INTranExt.aMLineNbr>>,
                                    And<INTran.released, Equal<False>>>>>>>
            >.Select(_inGraph, ammTran.INDocType, ammTran.INBatNbr, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr))
            {
                _inGraph.Caches[typeof(INTran)].Delete(inTran);
                boolReturn = true;
            }
            return boolReturn;
        }

        public virtual void DeleteINBatchOnly()
        {
            if (CurrentInDocument != null)
            {
                _inGraph.Caches[typeof(INRegister)].Delete(CurrentInDocument);
            }
        }

        public virtual void DeleteGLBatchOnly()
        {
            if (CurrentGlDocument != null)
            {
                GlGraph.BatchModule.Delete(CurrentGlDocument);
            }
        }

        /// <summary>
        /// Does the current journal entry contain released transactions
        /// </summary>
        /// <returns></returns>
        protected virtual bool ContainsReleasedGlTrans()
        {
            return (GlGraph?.GLTranModuleBatNbr?.Any() ?? false) && GlGraph.GLTranModuleBatNbr.Select().Cast<GLTran>().Any(result => result.Released.GetValueOrDefault());
        }

        /// <summary>
        /// Delete all GLTran records from the current transaction
        /// </summary>
        internal virtual void DeleteAllGlTrans()
        {
            if ((GlGraph?.GLTranModuleBatNbr?.Any() ?? false) == false)
            {
                return;
            }

            if (GlGraph?.BatchModule?.Current.Released != null &&
                GlGraph.BatchModule.Current.Released.GetValueOrDefault())
            {
                throw new PXException(Messages.GetLocal(Messages.JournalEntryIsReleased, GlGraph.BatchModule.Current.BatchNbr));
            }

            var rowsToDelete = new List<GLTran>();
            foreach (GLTran row in GlGraph.GLTranModuleBatNbr.Select())
            {
                if (row.Released.GetValueOrDefault())
                {
                    throw new PXException(Messages.GetLocal(Messages.JournalEntryIsReleased, row.BatchNbr));
                }

                if (GlGraph.GLTranModuleBatNbr.Cache.GetStatus(row) == PXEntryStatus.Deleted)
                {
                    continue;
                }

                rowsToDelete.Add(row);
            }

            foreach (var glTran in rowsToDelete)
            {
                GlGraph.GLTranModuleBatNbr.Delete(glTran);
            }
        }

        private INTran GetINTran(AMMTran ammTran)
        {
            return GetINTran(_transactionType, ammTran);
        }

        private INTran GetINTran(TransactionType transactionType, AMMTran ammTran)
        {
            if (ammTran == null 
                || string.IsNullOrEmpty(ammTran.INDocType) 
                || string.IsNullOrEmpty(ammTran.INBatNbr) 
                || ammTran.INLineNbr == null)
            {
                return null;
            }

            if (!ammTran.INDocType.EqualsWithTrim(ConvertToINDocType(transactionType)))
            {
                return null;
            }

            INTran inTran = PXSelect<INTran,
                Where<INTran.docType, Equal<Required<INTran.docType>>,
                    And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                    And<INTran.lineNbr, Equal<Required<INTran.lineNbr>>>>>
                    >.Select(_inGraph, ammTran.INDocType, ammTran.INBatNbr, ammTran.INLineNbr);

            if (inTran != null && inTran.InventoryID != ammTran.InventoryID)
            {
                return null;
            }

            return inTran;
        }

        private INTran UpdateLine(INTran inTran)
        {
            if (inTran == null)
            {
                return null;
            }
            if (_transactionType == TransactionType.IssueReturn)
            {
                return PXCache<INTran>.CreateCopy(((INIssueEntry)_inGraph).transactions.Update(inTran));
            }
            if (_transactionType == TransactionType.Receipt)
            {
                return PXCache<INTran>.CreateCopy(((INReceiptEntry)_inGraph).transactions.Update(inTran));
            }
            if (_transactionType == TransactionType.Adjustment)
            {
                return PXCache<INTran>.CreateCopy(((INAdjustmentEntry)_inGraph).transactions.Update(inTran));
            }

            return null;
        }

        /// <summary>
        /// Process non inventory transaction as a GL entry representing a scrapping of the item.
        /// </summary>
        /// <param name="ammTran">material transaction being scrapped</param>
        /// <param name="wipIsDebit">Is the WIP a debit transaction (true) or credit (false)</param>
        /// <returns></returns>
        public virtual AMMTran CreateScrapWriteOffLine(AMMTran ammTran, bool wipIsDebit)
        {
            if (ammTran == null || ammTran.TranAmt.GetValueOrDefault() == 0 || 
                IsINRefReleased || !ammTran.IsScrap.GetValueOrDefault())
            {
                return ammTran;
            }

            InventoryItem inventoryItem = PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(_inGraph, ammTran.InventoryID);

            if (inventoryItem == null)
            {
                return ammTran;
            }

            return CreateGLInventoryTranLine(ammTran, ammTran.AcctID, ammTran.SubID,
                Math.Abs(ammTran.TranAmt.GetValueOrDefault()),
                wipIsDebit,
                $"{Messages.ProdEntry_ScrapWriteOff} {inventoryItem.InventoryCD.Trim()}"
            );
        }

        public virtual AMMTran CreateTranLine(AMMTran ammTran, InventoryItem inventoryItem, List<AMMTranSplit> splits)
        {
            if (ammTran == null || ammTran.Qty.GetValueOrDefault() == 0 || IsINRefReleased)
            {
                return ammTran;
            }

            if (inventoryItem?.InventoryID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing,
                    Common.Cache.GetCacheName(typeof(InventoryItem))));
            }

            if (inventoryItem.StkItem.GetValueOrDefault())
            {
                return CreateStockItemTranLine(ammTran, splits);
            }

            return CreateNonStockTranLine(ammTran, inventoryItem,
                ammTran.Qty.GetValueOrDefault() > 0m && ammTran.TranType == AMTranType.Issue
                || ammTran.Qty.GetValueOrDefault() < 0m && ammTran.TranType == AMTranType.Adjustment);
        }

        public virtual AMMTran CreateNonStockTranLine(AMMTran ammTran, bool wipIsDebit)
        {
            if (ammTran == null)
            {
                throw new ArgumentNullException(nameof(ammTran));
            }

            return CreateNonStockTranLine(ammTran,
                PXSelect<InventoryItem,
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(_inGraph, ammTran.InventoryID),
                wipIsDebit);
        }

        protected virtual AMMTran CreateNonStockTranLine(AMMTran ammTran, InventoryItem inventoryItem, bool wipIsDebit)
        {
            if (ammTran == null)
            {
                throw new ArgumentNullException(nameof(ammTran));
            }

            if (inventoryItem == null)
            {
                return ammTran;
            }

            if (inventoryItem.StkItem != false)
            {
                //  A stock item was passed to this method for processing.
                //  Stop with an exception - only non stock items should be processed here
                throw new ArgumentException(nameof(inventoryItem));
            }

            if (inventoryItem.InvtAcctID == null || inventoryItem.InvtSubID == null)
            {
                //  The non stock item must have the expense accrual account setup (however its optional).
                //  If not entered the transaction should stop with an exception the same way Acumatica does during the release of a kit assembly for non stock item with no account.
                throw new Exception(Messages.GetLocal(Messages.NonStkNoExpAccrualAcct, inventoryItem.InventoryCD));
            }

            var stdCost = inventoryItem.StdCost;

            if (ammTran.SiteID.GetValueOrDefault() != 0)
            {
                INItemSite itemSite = PXSelect<INItemSite,
                    Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>
                    >>.Select(_inGraph, ammTran.InventoryID, ammTran.SiteID);

                if (itemSite != null)
                {
                    if (itemSite.TranUnitCost.GetValueOrDefault() != 0m)
                    {
                        //Non stock items at this time are only standard cost items, but just in case in the future things change...
                        stdCost = itemSite.TranUnitCost;
                    }
                }
            }

            if (stdCost.GetValueOrDefault() == 0m)
            {
                //No cost = no reason for GL tran
                return ammTran;
            }

            return CreateGLInventoryTranLine(ammTran, inventoryItem.InvtAcctID, inventoryItem.InvtSubID, 
                Math.Abs(ammTran.BaseQty.GetValueOrDefault()) * stdCost.GetValueOrDefault(),
                wipIsDebit,
                $"{Messages.ProdGLEntry_NonStockItem} {inventoryItem.InventoryCD.Trim()}"
                );
        }

        /// <summary>
        /// Create a GL transaction for the given inventory information
        /// </summary>
        /// <param name="ammTran"></param>
        /// <param name="accountId">GL Account to go against the WIP account</param>
        /// <param name="subAccountId">GL Sub-account to go against the WIP account</param>
        /// <param name="tranAmount">transaction amount</param>
        /// <param name="wipIsDebit">Is the WIP a debit transaction (true) or credit (false)</param>
        /// <param name="tranDesc">GL transaction description</param>
        /// <returns></returns>
        protected virtual AMMTran CreateGLInventoryTranLine(AMMTran ammTran, int? accountId, int? subAccountId, decimal tranAmount, bool wipIsDebit, string tranDesc)
        {
            if (ammTran == null)
            {
                throw new ArgumentNullException(nameof(ammTran));
            }

            if (tranAmount == 0)
            {
                return ammTran;
            }

            try
            {
                var WIPacct = ammTran.WIPAcctID;
                var WIPsub = ammTran.WIPSubID;

                var wipMsg = new System.Text.StringBuilder();
                if (WIPacct.GetValueOrDefault() == 0)
                {
                    wipMsg.AppendLine(Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.wIPAcctID>(_inGraph.Caches[typeof(AMMTran)])));
                }
                if (WIPsub.GetValueOrDefault() == 0)
                {
                    wipMsg.AppendLine(Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.wIPSubID>(_inGraph.Caches[typeof(AMMTran)])));
                }

                if (wipMsg.Length > 0)
                {
                    throw new PXException(wipMsg.ToString());
                }

                if (!string.IsNullOrWhiteSpace(ammTran.FinPeriodID) && string.IsNullOrWhiteSpace(PostPeriod))
                {
                    PostPeriod = ammTran.FinPeriodID;
                }

                if (string.IsNullOrWhiteSpace(PostPeriod))
                {
                    throw new Exception(Messages.InvalidPeriodToPost);
                }

                // On first line create batch header
                if (GlGraph.BatchModule.Current == null)
                {
                    var glBatch = PXCache<Batch>.CreateCopy(GlGraph.BatchModule.Insert(new Batch()));
                    glBatch.Hold = true;
                    glBatch.OrigModule = Common.ModuleAM;
                    glBatch.FinPeriodID = PostPeriod;
                    glBatch.DateEntered = TranDate;
                    glBatch.Description = Messages.GetLocal(Messages.ProductionTransaction, AMDocType.GetDocTypeDesc(ammTran.DocType));

                    if (!string.IsNullOrEmpty(ammTran.DocType) && !string.IsNullOrWhiteSpace(ammTran.BatNbr))
                    {
                        BatchExt extension = PXCache<Batch>.GetExtension<BatchExt>(glBatch);
                        extension.AMDocType = ammTran.DocType;
                        extension.AMBatNbr = ammTran.BatNbr;
                    }
                    GlGraph.BatchModule.Update(glBatch);
                    GlGraph.Persist();
                }


                var gltranDEBIT = GlGraph.GLTranModuleBatNbr.Insert();
                gltranDEBIT.AccountID = wipIsDebit ? WIPacct : accountId;
                gltranDEBIT.SubID = wipIsDebit ? WIPsub : subAccountId;
                gltranDEBIT.RefNbr = ammTran.ProdOrdID;
                gltranDEBIT.CreditAmt = 0m;
                gltranDEBIT.CuryCreditAmt = 0m;
                gltranDEBIT.DebitAmt = tranAmount;
                gltranDEBIT.CuryDebitAmt = tranAmount;
                gltranDEBIT.TranDate = ammTran.TranDate;
                gltranDEBIT.OrigModule = Common.ModuleAM;
                gltranDEBIT.OrigBatchNbr = ammTran.BatNbr;
                gltranDEBIT.OrigLineNbr = ammTran.LineNbr;
                gltranDEBIT.SummPost = SummaryPostGl;
                gltranDEBIT.TranDesc = tranDesc;

                gltranDEBIT = GlGraph.GLTranModuleBatNbr.Update(gltranDEBIT);

                if (!wipIsDebit)
                {
                    gltranDEBIT.Qty = Math.Abs(ammTran.Qty.GetValueOrDefault());
                    gltranDEBIT.UOM = ammTran.UOM;
                    gltranDEBIT.InventoryID = ammTran.InventoryID;

                    ammTran.GLBatNbr = gltranDEBIT.BatchNbr;
                    ammTran.GLLineNbr = gltranDEBIT.LineNbr;

                    gltranDEBIT.ProjectID = PM.ProjectDefaultAttribute.NonProject();
                }
                else
                {
                    gltranDEBIT.ProjectID = ammTran.ProjectID;
                    gltranDEBIT.TaskID = ammTran.TaskID;
                    gltranDEBIT.CostCodeID = ammTran.CostCodeID;
                }

                GlGraph.GLTranModuleBatNbr.Update(gltranDEBIT);


                var gltranCREDIT = GlGraph.GLTranModuleBatNbr.Insert();
                gltranCREDIT.AccountID = wipIsDebit ? accountId : WIPacct;
                gltranCREDIT.SubID = wipIsDebit ? subAccountId : WIPsub;
                gltranCREDIT.RefNbr = ammTran.ProdOrdID;
                gltranCREDIT.CreditAmt = tranAmount;
                gltranCREDIT.CuryCreditAmt = tranAmount;
                gltranCREDIT.DebitAmt = 0m;
                gltranCREDIT.CuryDebitAmt = 0m;
                gltranCREDIT.TranDate = ammTran.TranDate;
                gltranCREDIT.OrigModule = Common.ModuleAM;
                gltranCREDIT.OrigBatchNbr = ammTran.BatNbr;
                gltranCREDIT.OrigLineNbr = ammTran.LineNbr;
                gltranCREDIT.SummPost = SummaryPostGl;
                gltranCREDIT.TranDesc = tranDesc;

                gltranCREDIT = GlGraph.GLTranModuleBatNbr.Update(gltranCREDIT);

                if (wipIsDebit)
                {
                    gltranCREDIT.Qty = Math.Abs(ammTran.Qty.GetValueOrDefault());
                    gltranCREDIT.UOM = ammTran.UOM;
                    gltranCREDIT.InventoryID = ammTran.InventoryID;

                    ammTran.GLBatNbr = gltranCREDIT.BatchNbr;
                    ammTran.GLLineNbr = gltranCREDIT.LineNbr;

                    gltranCREDIT.ProjectID = PM.ProjectDefaultAttribute.NonProject();
                }
                else
                {
                    gltranCREDIT.ProjectID = ammTran.ProjectID;
                    gltranCREDIT.TaskID = ammTran.TaskID;
                    gltranCREDIT.CostCodeID = ammTran.CostCodeID;
                }

                GlGraph.GLTranModuleBatNbr.Update(gltranCREDIT);
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw new PXException(Messages.ErrorCreatingGLInventoryEntry);
            }

            return ammTran;
        }

        protected virtual AMMTran CreateStockItemTranLine(AMMTran ammTran, List<AMMTranSplit> splits)
        {
            if (ammTran == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            //Create a new INRegister
            if (!CreateInRegister(ammTran.DocType, ammTran.BatNbr))
            {
                //If a new one wasn't created then it exists so update
                UpdateInRegister();   
            }

            if (_transactionType == TransactionType.Adjustment)
            {
                return CreateStockItemINAdjustmentLines(ammTran, splits);
            }

            return CreateStockItemINLine(ammTran, splits);
        }

        public virtual AMMTran CreateScrapTranLine(AMMTran ammTran, decimal inTranAmount)
        {
            if (ammTran == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            //Create a new INRegister
            if (!CreateInRegister(ammTran.DocType, ammTran.BatNbr))
            {
                //If a new one wasn't created then it exists so update
                UpdateInRegister();
            }

            RemoveINReference(ref ammTran);

            if (ammTran.QtyScrapped.GetValueOrDefault() == 0)
            {
                return ammTran;
            }

            var scrapSplits = PXSelect<AMMTranSplit,
                Where<AMMTranSplit.docType, Equal<Required<AMMTranSplit.docType>>,
                    And<AMMTranSplit.batNbr, Equal<Required<AMMTranSplit.batNbr>>,
                        And<AMMTranSplit.lineNbr, Equal<Required<AMMTranSplit.lineNbr>>>>>
            >.Select(_inGraph, ammTran.OrigDocType, ammTran.OrigBatNbr, ammTran.OrigLineNbr).ToFirstTableList();

            if (_transactionType == TransactionType.Adjustment)
            {
                CreateScrapINAdjustmentLines(ammTran, inTranAmount, scrapSplits);
                return ammTran;
            }

            var scrapTran = (AMMTran) PXSelect<AMMTran,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>,
                        And<AMMTran.lineNbr, Equal<Required<AMMTran.lineNbr>>>>>
            >.Select(_inGraph, ammTran.OrigDocType, ammTran.OrigBatNbr, ammTran.OrigLineNbr);

            if (scrapTran?.BatNbr == null)
            {
                return ammTran;
            }

            CreateScrapINLines(scrapTran, scrapSplits, ammTran, scrapTran.QtyScrapped.GetValueOrDefault(), inTranAmount);

            return ammTran;
        }

        [Obsolete(InternalMessages.ClassIsObsoleteAndWillBeRemoved2020R2)]
        protected virtual void CreateScrapINAdjustmentLines(AMMTran ammTran, decimal inTranAmount)
        {
            CreateScrapINAdjustmentLines(ammTran, inTranAmount, null);
        }

        protected virtual void CreateScrapINAdjustmentLines(AMMTran ammTran, decimal inTranAmount, List<AMMTranSplit> scrapSplits)
        {
            if (ammTran == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            if (inTranAmount == 0 || ammTran.QtyScrapped.GetValueOrDefault() == 0)
            {
                return;
            }

            var unitCost = inTranAmount / ammTran.QtyScrapped.GetValueOrDefault();

            var containsSplits = (scrapSplits?.Count ?? 0) > 0;
            if (!containsSplits)
            {
                var scrapInTran = CreateStockItemINAdjustmentLine(ammTran, ammTran.QtyScrapped.GetValueOrDefault(), unitCost);
                scrapInTran.AcctID = ammTran.AcctID; //Use reason code account
                scrapInTran.SubID = ammTran.SubID;
                CurrentInTran = UpdateLine(scrapInTran);
                return;
            }

            foreach (var ammTranSplit in scrapSplits)
            {
                var scrapInTran = CreateStockItemINAdjustmentLine(ammTran, ammTranSplit, ammTranSplit.Qty.GetValueOrDefault(), unitCost);
                scrapInTran.AcctID = ammTran.AcctID; //Use reason code account
                scrapInTran.SubID = ammTran.SubID;
                CurrentInTran = UpdateLine(scrapInTran);
            }
        }

        protected virtual decimal GetUomUnitCost(string sourceUom, INTran inTran, decimal unitCost)
        {
            if (string.IsNullOrWhiteSpace(sourceUom))
            {
                throw new PXArgumentException(nameof(sourceUom));
            }

            if (inTran?.UOM == null)
            {
                throw new PXArgumentException(nameof(inTran));
            }

            if (sourceUom.EqualsWithTrim(inTran.UOM))
            {
                return unitCost;
            }

            if (UomHelper.TryConvertFromToCost<INTran.inventoryID>(_inGraph.Caches[typeof(INTran)], inTran, sourceUom, inTran.UOM,
                unitCost, out var amUnitCost))
            {
                return amUnitCost ?? unitCost;
            }
            return unitCost;
        }

        //do not run for adjustment
        protected virtual void CreateScrapINLines(AMMTran scrapTran, List<AMMTranSplit> scrapSplits, AMMTran wipTran, decimal qty, decimal inTranAmount)
        {
            if (wipTran == null)
            {
                throw new PXArgumentException(nameof(wipTran));
            }

            var hasSplits = (scrapSplits?.Count ?? 0) > 0;

            if (_transactionType == TransactionType.Adjustment || qty == 0)
            {
                return;
            }

            var scrapQty = Math.Abs(qty);
            var unitCost = inTranAmount / scrapQty;
            var tranUnitCost = unitCost.NotLessZero();
            
            var line = InsertLine(new INTran());
            line.ReasonCode = wipTran.ReasonCodeID;
            line.UnitCost = tranUnitCost;
            line.AcctID = wipTran.AcctID;
            line.SubID = wipTran.SubID;
            line.TranDate = scrapTran.TranDate;
            line.InventoryID = scrapTran.InventoryID;
            line.SubItemID = scrapTran.SubItemID;
            line.SiteID = scrapTran.SiteID;
            line.LocationID = scrapTran.LocationID;
            line.Qty = scrapQty;
            line.UOM = scrapTran.UOM;

            line = UpdateLine(line);

            line.LotSerialNbr = string.IsNullOrWhiteSpace(scrapTran.LotSerialNbr) ? null : scrapTran.LotSerialNbr;
            line.ExpireDate = scrapTran.ExpireDate;
            line.OrigRefNbr = scrapTran.ReceiptNbr;

            if (ProjectHelper.IsProjectFeatureEnabled())
            {
                line.ProjectID = scrapTran.ProjectID;
                line.TaskID = scrapTran.TaskID;
                line.CostCodeID = scrapTran.CostCodeID;
            }

            SetINReference(ref wipTran, ref line);

            //When zero cost... we need to re-update as Acumatica will replace the zero with last cost...
            if (tranUnitCost == 0 && line.UnitCost.GetValueOrDefault() != 0)
            {
                line.UnitCost = tranUnitCost;
            }

            line = UpdateLine(line);

            if (hasSplits)
            {
                //Only delete the INSplits when we know we have the records in our transaction splits to re-build the records.
                DeleteINTranSplits(line);
                var inSplits = CreateINTranSplits(line, scrapSplits);
                if (inSplits != null)
                {
                    foreach (var inSplit in inSplits)
                    {
                        InsertSplit(inSplit);
                    }
                }
            }

            // Make sure we at least have a matching split line for the given transaction line...
            if (!HasINTranSplits(line))
            {
                var inTranSplit = CreateInsertINTranSplit(line);
                inTranSplit.Qty = scrapQty;
                inTranSplit.LotSerialNbr = string.IsNullOrWhiteSpace(scrapTran.LotSerialNbr) ? null : scrapTran.LotSerialNbr;
                if (_transactionType == TransactionType.Receipt)
                {
                    inTranSplit.ExpireDate = scrapTran.ExpireDate;
                }
                UpdateSplit(inTranSplit);
            }
        }

        protected virtual INTran CreateStockItemINAdjustmentLine(AMMTran ammTran)
        {
            return CreateStockItemINAdjustmentLine(ammTran,
                ammTran.Qty.GetValueOrDefault(),
                ammTran.UnitCost.GetValueOrDefault() < 0 ? 0 : ammTran.UnitCost.GetValueOrDefault());
        }

        protected virtual INTran CreateStockItemINAdjustmentLine(AMMTran ammTran, decimal tranQty, decimal unitCost)
        {
            if (ammTran == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            var line = InsertLine(new INTran { TranType = INTranType.Adjustment });
            line.ReasonCode = ammTran.ReasonCodeID;
            line.UnitCost = unitCost;
            line.AcctID = ammTran.WIPAcctID;
            line.SubID = ammTran.WIPSubID;
            line.TranDate = ammTran.TranDate;
            line.InventoryID = ammTran.InventoryID;
            line.SubItemID = ammTran.SubItemID;
            line.SiteID = ammTran.SiteID;
            line.LocationID = ammTran.LocationID;
            line.Qty = Math.Abs(tranQty) * ammTran.InvtMult.GetValueOrDefault();
            line.UOM = ammTran.UOM;

            line = UpdateLine(line);

            line.LotSerialNbr = string.IsNullOrWhiteSpace(ammTran.LotSerialNbr) ? null : ammTran.LotSerialNbr;
            line.ExpireDate = ammTran.ExpireDate;
            line.OrigRefNbr = ammTran.ReceiptNbr;

            if (ProjectHelper.IsProjectFeatureEnabled())
            {
                line.ProjectID = ammTran.ProjectID;
                line.TaskID = ammTran.TaskID;
                line.CostCodeID = ammTran.CostCodeID;
            }

            SetINReference(ref ammTran, ref line);

            return line;
        }

        protected virtual INTran CreateStockItemINAdjustmentLine(AMMTran ammTran, AMMTranSplit ammTranSplit)
        {
            return CreateStockItemINAdjustmentLine(ammTran, ammTranSplit,
                ammTranSplit.Qty.GetValueOrDefault(),
                ammTran.UnitCost.GetValueOrDefault() < 0 ? 0 : ammTran.UnitCost.GetValueOrDefault());
        }

        protected virtual INTran CreateStockItemINAdjustmentLine(AMMTran ammTran, AMMTranSplit ammTranSplit, decimal tranQty, decimal unitCost)
        {
            var line = CreateStockItemINAdjustmentLine(ammTran);

            if (ammTranSplit == null)
            {
                return line;
            }

            if (line == null)
            {
                throw new PXArgumentException(nameof(line));
            }

            line.InventoryID = ammTranSplit.InventoryID;
            line.SubItemID = ammTranSplit.SubItemID;
            line.SiteID = ammTranSplit.SiteID;
            line.LocationID = ammTranSplit.LocationID;
            line.LotSerialNbr = ammTranSplit.LotSerialNbr;
            line.UOM = ammTranSplit.UOM;
            line.UnitCost = GetUomUnitCost(ammTran.UOM, line, unitCost);
            line.Qty = Math.Abs(tranQty) * ammTran.InvtMult.GetValueOrDefault();
            line.ExpireDate = ammTranSplit.ExpireDate;

            return line;
        }

        /// <summary>
        /// Creates IN Adjustment lines based on the AMTran line.
        /// Most likely negative move transactions
        /// </summary>
        /// <param name="ammTran">Manufacturing transaction line as the source of the IN Adjustment line(s) to create</param>
        /// <param name="splits">split lines for ammTran record</param>
        /// <returns>passed in ammtran record updated as it relates to the INAdjustment lines</returns>
        protected virtual AMMTran CreateStockItemINAdjustmentLines(AMMTran ammTran, List<AMMTranSplit> splits)
        {
            if (ammTran == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            if (_transactionType != TransactionType.Adjustment)
            {
                throw new PXException(Messages.UnknownTranType);
            }

            RemoveINReference(ref ammTran);

            if (splits != null && splits.Count > 0)
            {
                foreach (var split in splits)
                {
                    CurrentInTran = UpdateLine(CreateStockItemINAdjustmentLine(ammTran, split));
                }

                return ammTran;
            }

            CurrentInTran = UpdateLine(CreateStockItemINAdjustmentLine(ammTran));

            return ammTran;
        }

        protected virtual AMMTran CreateStockItemINLine(AMMTran ammTran, List<AMMTranSplit> splits)
        {
            return CreateStockItemINLine(ammTran, splits, null, null);
        }

        //DO NOT CALL FOR ADJUSTEMENTS
        protected virtual AMMTran CreateStockItemINLine(AMMTran ammTran, List<AMMTranSplit> splits, decimal? qty, decimal? unitCost)
        {
            if (ammTran == null)
            {
                throw new PXArgumentException(nameof(ammTran));
            }

            var line = GetINTran(ammTran);

            if (line == null)
            {
                //Remove reference if exists
                ammTran = RemoveINReferenceLegacy(ammTran);

                line = InsertLine(new INTran());
            }

            var tranQty = qty == null ? ammTran.Qty.GetValueOrDefault() : qty.GetValueOrDefault();
            var tranUnitCost = unitCost == null ? ammTran.UnitCost.GetValueOrDefault() : unitCost.GetValueOrDefault();

            line.ReasonCode = null;
            if (_transactionType == TransactionType.IssueReturn)
            {
                line.TranType = INTranType.Issue;
                line.COGSAcctID = ammTran.WIPAcctID;
                line.COGSSubID = ammTran.WIPSubID;
                line.Qty = Math.Abs(tranQty);

                if (tranQty < 0 && ammTran.TranType == AMTranType.Return)
                {
                    line.TranType = INTranType.Return;
                    line.UnitCost = tranUnitCost.NotLessZero();
                }

                if (ProjectHelper.IsProjectAcctWithProjectEnabled(_inGraph, line.COGSAcctID))
                {
                    line.ProjectID = ammTran.ProjectID;
                    line.TaskID = ammTran.TaskID;
                    line.CostCodeID = ammTran.CostCodeID;
                }
            }
            else if (_transactionType == TransactionType.Receipt)
            {
                line.TranType = INTranType.Receipt;
                line.UnitCost = tranUnitCost.NotLessZero();
                line.AcctID = ammTran.WIPAcctID;
                line.SubID = ammTran.WIPSubID;
                line.Qty = Math.Abs(tranQty);

                //fill in project no matter what and let Acumatica handle the GL transactions
                if (ProjectHelper.IsProjectFeatureEnabled())
                {
                    line.ProjectID = ammTran.ProjectID;
                    line.TaskID = ammTran.TaskID;
                    line.CostCodeID = ammTran.CostCodeID;
                }
            }
            else
            {
                throw new PXException(Messages.UnknownTranType);
            }

            line.TranDate = ammTran.TranDate;
            line.InventoryID = ammTran.InventoryID;
            line.SubItemID = ammTran.SubItemID;
            line.SiteID = ammTran.SiteID;
            line.LocationID = ammTran.LocationID;
            line.UOM = ammTran.UOM;

            SetINReference(ref ammTran, ref line);

            line = UpdateLine(line);

            //When zero cost... we need to re-update as Acumatica will replace the zero with last cost...
            if (tranUnitCost == 0 && line.UnitCost.GetValueOrDefault() != 0)
            {
                line.UnitCost = tranUnitCost;
                line = UpdateLine(line);
            }

            if (splits != null && splits.Count > 0)
            {
                var demandPlans = GetDemandPlans(ammTran);

                //Only delete the INSplits when we know we have the records in our transaction splits to re-build the records.
                DeleteINTranSplits(line);

                var inSplits = CreateINTranSplits(line, splits);
                var mergedResults = MergeINTranSplits(inSplits, demandPlans, CurrentCachedDemandPlanList());

                foreach (PXResult<INTranSplit, INItemPlan> mergedResult in mergedResults)
                {
                    var calcPlanType = (INItemPlan) mergedResult;
                    var insertedSplit = InsertSplit(mergedResult);

                    if (calcPlanType?.DemandPlanID == null)
                    {
                        continue;
                    }
                    var inItemPlan = GetINItemPlan(insertedSplit);
                    if (inItemPlan?.PlanID != null)
                    {
                        //inItemPlan.DemandPlanID = calcPlanType.DemandPlanID;
                        inItemPlan.PlanType = INPlanConstants.Plan77;
                        inItemPlan = (INItemPlan)_inGraph.Caches<INItemPlan>().Update(inItemPlan);

                        RefDocNoteRecordRequired |= RefDocNoteId != null;
                        var refNoteID = RefDocNoteId;
                        var refEntityType = RefDocEntityType;
                        if (refNoteID == null)
                        {
                            refNoteID = calcPlanType.RefNoteID;
                            refEntityType = calcPlanType.RefEntityType;
                        }

                        var preAllocatedPlan = new INItemPlan
                        {
                            InventoryID = line.InventoryID,
                            SiteID = line.SiteID,
                            SubItemID = line.SubItemID,
                            LotSerialNbr = calcPlanType.LotSerialNbr,
                            PlanQty = insertedSplit.BaseQty,
                            DemandPlanID = calcPlanType.DemandPlanID,
                            SupplyPlanID = inItemPlan.PlanID,
                            RefNoteID = refNoteID,
                            RefEntityType = refEntityType,
                            PlanDate = calcPlanType.PlanDate,
                            PlanType = INPlanConstants.Plan64,
                            FixedSource = insertedSplit.SiteID != calcPlanType.SiteID
                                ? INReplenishmentSource.Transfer
                                : INReplenishmentSource.None,
                            SourceSiteID = insertedSplit.SiteID,
                            Hold = false
                        };
                        _inGraph.Caches<INItemPlan>().Insert(preAllocatedPlan);
                    }
                }
            }

            // Make sure we at least have a matching split line for the given transaction line...
            if(!HasINTranSplits(line))
            {
                var inTranSplit = CreateInsertINTranSplit(line);
                inTranSplit.Qty = Math.Abs(tranQty);
                inTranSplit.LotSerialNbr = string.IsNullOrWhiteSpace(ammTran.LotSerialNbr) ? null : ammTran.LotSerialNbr;
                if (_transactionType == TransactionType.Receipt)
                {
                    inTranSplit.ExpireDate = ammTran.ExpireDate;
                }
                UpdateSplit(inTranSplit);
            }

            CurrentInTran = line;
            return ammTran;
        }

        public static List<PXResult<INTranSplit, INItemPlan>> MergeINTranSplits(List<INTranSplit> inTranSplits, List<PXResult<INItemPlan, INPlanType>> demandPlans, List<INItemPlan> cachededPlans)
        {
            if (inTranSplits == null)
            {
                return null;
            }

            var listResult = new List<PXResult<INTranSplit, INItemPlan>>();
            var cachedPlans2 = cachededPlans == null ? new List<INItemPlan>() : cachededPlans.ToList();

            foreach (var inTranSplit in inTranSplits)
            {
                if (demandPlans == null || demandPlans.Count == 0)
                {
                    listResult.Add(new PXResult<INTranSplit, INItemPlan>(inTranSplit, null));
                    continue;
                }

                var currentAdded = false;

                INItemPlan dummySplitPlan = new INItemPlan { PlanQty = inTranSplit.BaseQty.GetValueOrDefault() };
                foreach (PXResult<INItemPlan, INPlanType> demandPlanResult in demandPlans)
                {
                    var demandPlan = (INItemPlan)demandPlanResult;
                    var isSales = demandPlan.PlanType == INPlanConstants.PlanM8 ||
                                  demandPlan.PlanType == INPlanConstants.Plan60;
                    if (demandPlan?.PlanQty == null || !isSales)
                    {
                        //Ignoring any catch non sales order linked demand
                        continue;
                    }

                    var cachedQty = GetDemandPlanQtyFromList(demandPlan, cachedPlans2);
                    var remainQty = demandPlan.PlanQty.GetValueOrDefault() - cachedQty;
                    if (remainQty <= 0)
                    {
                        continue;
                    }

                    if (remainQty >= inTranSplit.BaseQty.GetValueOrDefault())
                    {
                        dummySplitPlan.PlanDate = demandPlan.PlanDate;
                        dummySplitPlan.SiteID = inTranSplit.SiteID;
                        dummySplitPlan.LotSerialNbr = inTranSplit.LotSerialNbr;
                        dummySplitPlan.RefNoteID = demandPlan.RefNoteID;
                        dummySplitPlan.DemandPlanID = demandPlan.PlanID;
                        dummySplitPlan.PlanQty = inTranSplit.BaseQty.GetValueOrDefault();
                        cachedPlans2.Add(dummySplitPlan);
                        break;
                    }

                    var conversion = inTranSplit.BaseQty.GetValueOrDefault() == 0 ? 
                        1 : 
                        inTranSplit.Qty.GetValueOrDefault() / inTranSplit.BaseQty.GetValueOrDefault(); // qty = 1 box, baseqty = 4 so conversion = 0.25

                    var newSplitBaseQty = inTranSplit.BaseQty.GetValueOrDefault() - remainQty;

                    //remainQty < inTranSplit.BaseQty
                    // Need current split = to remainQty
                    inTranSplit.Qty = Math.Round(remainQty * conversion, 6, MidpointRounding.AwayFromZero);
                    inTranSplit.BaseQty = remainQty;
                    dummySplitPlan.PlanDate = demandPlan.PlanDate;
                    dummySplitPlan.SiteID = inTranSplit.SiteID;
                    dummySplitPlan.LotSerialNbr = inTranSplit.LotSerialNbr;
                    dummySplitPlan.RefNoteID = demandPlan.RefNoteID;
                    dummySplitPlan.DemandPlanID = demandPlan.PlanID;
                    dummySplitPlan.PlanQty = inTranSplit.BaseQty.GetValueOrDefault();
                    cachedPlans2.Add(dummySplitPlan);
                    currentAdded = true;
                    listResult.Add(new PXResult<INTranSplit, INItemPlan>(inTranSplit, dummySplitPlan));
                    
                    var newSplit = new INTranSplit
                    {
                        DocType = inTranSplit.DocType,
                        TranType = inTranSplit.TranType,
                        TranDate = inTranSplit.TranDate,
                        InventoryID = inTranSplit.InventoryID,
                        SubItemID = inTranSplit.SubItemID,
                        SiteID = inTranSplit.SiteID,
                        LocationID = inTranSplit.LocationID,
                        LotSerialNbr = inTranSplit.LotSerialNbr,
                        Qty = Math.Round(newSplitBaseQty * conversion, 6, MidpointRounding.AwayFromZero),
                        BaseQty = newSplitBaseQty
                    };
                    listResult.Add(new PXResult<INTranSplit, INItemPlan>(newSplit, new INItemPlan{PlanQty = newSplitBaseQty}));
                    break;
                }

                if (!currentAdded)
                {
                    listResult.Add(new PXResult<INTranSplit, INItemPlan>(inTranSplit, dummySplitPlan));
                }
            }
            return listResult;
        }

        protected virtual List<INTranSplit> CreateINTranSplits(INTran inTran, List<AMMTranSplit> ammTranSplits)
        {
            var list = new List<INTranSplit>();
            if (inTran?.LineNbr == null || ammTranSplits == null)
            {
                return list;
            }

            foreach (var ammTranSplit in ammTranSplits)
            {
                var inTranSplit = new INTranSplit
                {
                    DocType = inTran.DocType,
                    TranType = inTran.TranType,
                    TranDate = inTran.TranDate,
                    InventoryID = inTran.InventoryID,
                    SubItemID = inTran.SubItemID,
                    SiteID = inTran.SiteID,
                    LocationID = inTran.LocationID,
                    ExpireDate = ammTranSplit.ExpireDate,
                    Qty = Math.Abs(ammTranSplit.Qty.GetValueOrDefault()),
                    LotSerialNbr = string.IsNullOrWhiteSpace(ammTranSplit.LotSerialNbr)
                        ? null
                        : ammTranSplit.LotSerialNbr
                };

                if (ammTranSplit.SiteID != null)
                {
                    inTranSplit.SiteID = ammTranSplit.SiteID;
                    inTranSplit.LocationID = ammTranSplit.LocationID ?? inTran.LocationID;
                }

                list.Add(inTranSplit);
            }

            return list;
        }

        /// <summary>
        /// How much plan qty is already linked/cached in the given INtransaction?
        /// </summary>
        /// <returns></returns>
        protected virtual decimal CurrentCachedDemandPlanQty(INItemPlan demandPlan)
        {
            var cachedBaseQty = 0m;
            if (demandPlan?.PlanID == null)
            {
                return cachedBaseQty;
            }

            return GetDemandPlanQtyFromList(demandPlan, CurrentCachedDemandPlanList());
        }

        protected virtual List<INItemPlan> CurrentCachedDemandPlanList()
        {
            var list = new List<INItemPlan>();
            foreach (INItemPlan itemPlan in _inGraph.Caches<INItemPlan>().Cached) /* OR INSERTED? */
            {
                list.Add(itemPlan);
            }

            return list;
        }

        public static decimal GetDemandPlanQtyFromList(INItemPlan demandPlan, List<INItemPlan> itemPlanCache)
        {
            var cachedBaseQty = 0m;
            if (demandPlan?.PlanID == null)
            {
                return cachedBaseQty;
            }
            foreach (INItemPlan itemPlan in itemPlanCache)
            {
                if (itemPlan.DemandPlanID == demandPlan.PlanID)
                {
                    cachedBaseQty += itemPlan.PlanQty.GetValueOrDefault();
                }
            }

            return cachedBaseQty;
        }

        protected virtual List<PXResult<INItemPlan, INPlanType>> GetDemandPlans(AMMTran line)
        {
            if (line?.LineNbr == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            var list = new List<PXResult<INItemPlan, INPlanType>>();

            if (!AMDocType.IsDocTypeMove(line.DocType))
            {
                return list;
            }

            foreach (AMProdItemSplit prodItemSplit in PXSelect<AMProdItemSplit, 
                Where<AMProdItemSplit.orderType, Equal<Current<AMMTran.orderType>>, 
                    And<AMProdItemSplit.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>.SelectMultiBound(_inGraph, new object[]{ line }))
            {
                var rowList = GetDemandPlans(line, prodItemSplit);
                if (rowList == null)
                {
                    continue;
                }
                list.AddRange(rowList);
            }
            return list;
        }

        /// <summary>
        /// Get the demand items linked to the production split
        /// </summary>
        /// <param name="line"></param>
        /// <param name="prodItemSplit"></param>
        protected virtual List<PXResult<INItemPlan, INPlanType>> GetDemandPlans(AMMTran line, AMProdItemSplit prodItemSplit)
        {
            if (line?.LineNbr == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (string.IsNullOrWhiteSpace(prodItemSplit?.ProdOrdID))
            {
                return null;
            }

            return PXSelectJoin<INItemPlan,
                    InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>>,
                    Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>,
                        And<INPlanType.isDemand, Equal<True>,
                            And<INPlanType.isFixed, Equal<True>>>>>.Select(_inGraph, prodItemSplit.PlanID)
                .ToList<INItemPlan, INPlanType>();
        }

        protected virtual INItemPlan GetINItemPlan(INTranSplit inTranSplit)
        {
            return PXSelect<INItemPlan, 
                    Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
                .Select(_inGraph, inTranSplit?.PlanID);
        }

        protected virtual INTranSplit CreateInsertINTranSplit(INTran inTran)
        {
            if (inTran == null)
            {
                throw new PXArgumentException(nameof(inTran));
            }

            var inTranSplit = InsertSplit(new INTranSplit());
            inTranSplit.DocType = inTran.DocType;
            inTranSplit.TranType = inTran.TranType;
            inTranSplit.TranDate = inTran.TranDate;
            inTranSplit.InventoryID = inTran.InventoryID;
            inTranSplit.SubItemID = inTran.SubItemID;
            inTranSplit.SiteID = inTran.SiteID;
            inTranSplit.LocationID = inTran.LocationID;
            return inTranSplit;
        }

        /// <summary>
        /// Deletes the related transaction split records to the IN transaction if found
        /// </summary>
        protected virtual void DeleteINTranSplits(INTran inTran)
        {
            PXResultset<INTranSplit> resultSet = GetINTranSplits(inTran);
            if (resultSet != null)
            {
                foreach (INTranSplit isplit in resultSet)
                {
                    if (_transactionType == TransactionType.IssueReturn)
                    {
                        ((INIssueEntry)_inGraph).splits.Delete(isplit);
                        continue;
                    }
                    if (_transactionType == TransactionType.Receipt)
                    {
                        ((INReceiptEntry)_inGraph).splits.Delete(isplit);
                    }
                }
            }
        }

        /// <summary>
        /// Does the given IN Transaction line currently contain any split records in the cache/graph?
        /// </summary>
        /// <returns>True if records found</returns>
        protected bool HasINTranSplits(INTran inTran)
        {
            var splits = GetINTranSplits(inTran);
            return splits != null && splits.Count > 0;
        }

        /// <summary>
        /// Gets the related transaction split records to the IN transaction
        /// </summary>
        protected PXResultset<INTranSplit> GetINTranSplits(INTran inTran)
        {
            if (_transactionType == TransactionType.IssueReturn)
            {
                return ((INIssueEntry)_inGraph).splits.Select(inTran.LineNbr);
            }

            if (_transactionType == TransactionType.Receipt)
            {
                return ((INReceiptEntry)_inGraph).splits.Select(inTran.LineNbr);
            }
            //
            //  Adjustments do not allow for multiple splits. Set the lines lot/serial number field and we should be ok to not worry about the splits
            //

            return null;
        }

        public static string ConvertToINDocType(TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.IssueReturn:
                    return INDocType.Issue;
                case TransactionType.Receipt:
                    return INDocType.Receipt;
                case TransactionType.Adjustment:
                    return INDocType.Adjustment;
            }

            return INDocType.Undefined;
        }
    }
}
