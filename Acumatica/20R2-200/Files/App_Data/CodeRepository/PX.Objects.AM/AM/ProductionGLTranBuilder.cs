using System;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Objects.PM;
using System.Collections.Generic;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    public class ProductionGLTranBuilder
    {
        private JournalEntry _journalEntryGraph;
        private ProductionCostEntry _productionCostEntryGraph;
        private bool _summaryPostGl;

        public ProductionGLTranBuilder()
        {
            //initialize GL graph
            JournalEntry _journalEntryGraph = PXGraph.CreateInstance<JournalEntry>();
            _journalEntryGraph.Clear();

            //Used for Manufacturing GL batches that are stored in AMMtran
            ProductionCostEntry _amGlTranGraph = PXGraph.CreateInstance<ProductionCostEntry>();
            _amGlTranGraph.Clear();

            SetGraphs(_journalEntryGraph, _amGlTranGraph);
        }

        public ProductionGLTranBuilder(JournalEntry _journalEntryGraph)
        {
            //Used for Manufacturing GL batches that are stored in AMMtran
            ProductionCostEntry _amGlTranGraph = PXGraph.CreateInstance<ProductionCostEntry>();
            _amGlTranGraph.Clear();

            SetGraphs(_journalEntryGraph, _amGlTranGraph);
        }

        public ProductionGLTranBuilder(JournalEntry journalEntryGraph, ProductionCostEntry amGlTranGraph)
        {
            SetGraphs(journalEntryGraph, amGlTranGraph);
        }

        protected void SetGraphs(JournalEntry journalEntryGraph, ProductionCostEntry amGlTranGraph)
        {
            var ampSetup = (AMPSetup) PXSelect<AMPSetup>.Select(amGlTranGraph);

            _summaryPostGl = ampSetup.SummPost ?? false;

            CheckGraph(journalEntryGraph);
            CheckGraph(amGlTranGraph);

            journalEntryGraph.glsetup.Current.RequireControlTotal = false;
            amGlTranGraph.ampsetup.Current.RequireControlTotal = false;

            _journalEntryGraph = journalEntryGraph;
            _productionCostEntryGraph = amGlTranGraph;
        }
        
        protected virtual void CheckGraph(PXGraph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }
        }

        public virtual void Save()
        {
            var amBatchStatus = _productionCostEntryGraph.batch.Cache.GetStatus(_productionCostEntryGraph.batch.Current);
            if (_productionCostEntryGraph.IsDirty)
            {
                _productionCostEntryGraph.Persist();
            }

            if (!_journalEntryGraph.IsDirty)
            {
                return;
            }

            if(amBatchStatus == PXEntryStatus.Inserted || amBatchStatus == PXEntryStatus.Updated)
            {
                SetRefernces();
            }

            _journalEntryGraph.Persist();
        }

        protected virtual void SetRefernces()
        {
            var amBatch = _productionCostEntryGraph.batch?.Current;
            var batch = _journalEntryGraph?.BatchModule?.Current;
            var extension = batch?.GetExtension<BatchExt>();
            if (amBatch?.BatNbr == null || extension == null)
            {
                return;
            }

            extension.AMDocType = amBatch.DocType;
            extension.AMBatNbr = amBatch.BatNbr;
            _journalEntryGraph.BatchModule.Update(batch);

            foreach (GLTran row in _journalEntryGraph.GLTranModuleBatNbr.Cache.Inserted)
            {
                if (row.OrigBatchNbr.Equals(amBatch.BatNbr))
                {
                    continue;
                }

                row.OrigBatchNbr = amBatch.BatNbr;
                _journalEntryGraph.GLTranModuleBatNbr.Update(row);
            }
        }

        public AMBatch CurrentAmDocument
        {
            get { return _productionCostEntryGraph.batch.Current; }
        }

        public Batch CurrentGlDocument
        {
            get { return _journalEntryGraph.BatchModule.Current; }
        }

        public bool HasAmTransactions
        {
            get
            {
                return _productionCostEntryGraph.transactions.Select().Count > 0;
            }
        }

        public bool HasGlTransactions
        {
            get { return _journalEntryGraph.GLTranModuleBatNbr.Select().Count > 0; }
        }

        /// <summary>
        /// Delete the production cost batch and related journal entry batch
        /// </summary>
        public virtual void DeleteBatch()
        {
            DeleteJournalEntryBatchOnly();

            if (_productionCostEntryGraph != null && _productionCostEntryGraph.batch.Current != null)
            {
                _productionCostEntryGraph.batch.Delete(_productionCostEntryGraph.batch.Current);
            }
        }

        /// <summary>
        /// Delete journal entry transaction only
        /// </summary>
        public virtual void DeleteJournalEntryBatchOnly()
        {
            if (_journalEntryGraph != null && _journalEntryGraph.BatchModule.Current != null)
            {
                _journalEntryGraph.BatchModule.Delete(_journalEntryGraph.BatchModule.Current);
            }
        }

        #region Insert AM & GL lines ( InsertAMGL )

        /// <summary>
        /// Creating a GL and AM transaction for all production offset entries (Overhead, Tool, Machine, BF Labor).
        /// WIP account is a debit when the transaction amount is a positive number (credit when negative).
        /// The other side of the GL entry are the passed Offset accounts/sub fields.
        /// </summary>
        /// <param name="amProdItem">AMProdItem DAC related to transaction</param>
        /// <param name="ammTran">Root ammtran record generating the gl entry</param>
        /// <param name="tranAmount">GL Transaction amount</param>
        /// <param name="offsetAccount">Expense offset account</param>
        /// <param name="offsetSub">Expense offset sub account</param>
        /// <param name="amTranType">AM Transaction type (Pass a selection from AMMTran.AMTranType)</param>
        /// <param name="costOperationId">Operation ID that should be referenced as the cost operation</param>
        /// <param name="transactionDescription">Transaction description if different than tran type description</param>
        public virtual void CreateAmGlLine(
            AMProdItem amProdItem,
            AMMTran ammTran,
            decimal? tranAmount,
            int? offsetAccount,
            int? offsetSub,
            string amTranType,
            int? costOperationId,
            string transactionDescription)
        {
            CreateAmGlLine(amProdItem, ammTran, tranAmount, offsetAccount, offsetSub, amTranType, costOperationId, transactionDescription, null);
        }

        /// <summary>
        /// Creating a GL and AM transaction for all production offset entries (Overhead, Tool, Machine, BF Labor).
        /// WIP account is a debit when the transaction amount is a positive number (credit when negative).
        /// The other side of the GL entry are the passed Offset accounts/sub fields.
        /// </summary>
        /// <param name="amProdItem">AMProdItem DAC related to transaction</param>
        /// <param name="ammTran">Root ammtran record generating the gl entry</param>
        /// <param name="tranAmount">GL Transaction amount</param>
        /// <param name="offsetAccount">Expense offset account</param>
        /// <param name="offsetSub">Expense offset sub account</param>
        /// <param name="amTranType">AM Transaction type (Pass a selection from AMMTran.AMTranType)</param>
        /// <param name="costOperationId">Operation ID that should be referenced as the cost operation</param>
        /// <param name="transactionDescription">Transaction description if different than tran type description</param>
        /// <param name="laborTime">Labor Time related to the transaction</param>
        public virtual void CreateAmGlLine(
            AMProdItem amProdItem,
            AMMTran ammTran,
            decimal? tranAmount,
            int? offsetAccount,
            int? offsetSub,
            string amTranType,
            int? costOperationId,
            string transactionDescription,
            int? laborTime)
        {
            CreateAmGlLine(amProdItem, ammTran, tranAmount, offsetAccount, offsetSub, amTranType, costOperationId, transactionDescription, laborTime, null);
        }

        /// <summary>
        /// Creating a GL and AM transaction for all production offset entries (Overhead, Tool, Machine, BF Labor).
        /// WIP account is a debit when the transaction amount is a positive number (credit when negative).
        /// The other side of the GL entry are the passed Offset accounts/sub fields.
        /// </summary>
        /// <param name="amProdItem">AMProdItem DAC related to transaction</param>
        /// <param name="ammTran">Root ammtran record generating the gl entry</param>
        /// <param name="tranAmount">GL Transaction amount</param>
        /// <param name="offsetAccount">Expense offset account</param>
        /// <param name="offsetSub">Expense offset sub account</param>
        /// <param name="amTranType">AM Transaction type (Pass a selection from AMMTran.AMTranType)</param>
        /// <param name="costOperationId">Operation ID that should be referenced as the cost operation</param>
        /// <param name="transactionDescription">Transaction description if different than tran type description</param>
        /// <param name="laborTime">Labor Time related to the transaction</param>
        /// <param name="referenceCostId">the cost id reference such as tool id, overhead id, etc.</param>
        public virtual void CreateAmGlLine(
            AMProdItem amProdItem,
            AMMTran ammTran,
            decimal? tranAmount,
            int? offsetAccount,
            int? offsetSub,
            string amTranType,
            int? costOperationId,
            string transactionDescription,
            int? laborTime,
            string referenceCostId)
        {
            if (costOperationId == null)
            {
                costOperationId = ammTran.OperationID;
            }

            if (string.IsNullOrWhiteSpace(transactionDescription))
            {
                transactionDescription = AMTranType.GetTranDescription(amTranType);
            }

            try
            {
                if (amProdItem == null)
                {
                    throw new Exception(Messages.GetLocal(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMProdItem))));
                }

                int? WIPacct = amProdItem.WIPAcctID;
                int? WIPsub = amProdItem.WIPSubID;

                var sb = new StringBuilder();
                if (WIPacct.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof (ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.wIPAcctID>(
                            _productionCostEntryGraph.Caches[typeof (AMMTran)])));
                }
                if (WIPsub.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.wIPSubID>(
                            _productionCostEntryGraph.Caches[typeof(AMMTran)])));
                }
                if (offsetAccount.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.acctID>(
                            _productionCostEntryGraph.Caches[typeof(AMMTran)])));
                }
                if (offsetSub.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.subID>(
                            _productionCostEntryGraph.Caches[typeof(AMMTran)])));
                }
                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    throw new PXException(sb.ToString());
                }

                if (string.IsNullOrEmpty(ammTran.FinPeriodID))
                {
                    //Must pass the period to post
                    throw new Exception(Messages.InvalidPeriodToPost);
                }

                if (tranAmount.GetValueOrDefault() == 0m && laborTime.GetValueOrDefault() == 0)
                {
                    return;
                }

                // Allow at least an entry with Labor Time for correct labor time totals when no cost backflush time

                CreateBatchRecord(ammTran);

                var ammTranGL = _productionCostEntryGraph.transactions.Insert(new AMMTran
                {
                    OrderType = amProdItem.OrderType,
                    ProdOrdID = amProdItem.ProdOrdID,
                    Qty = 0m,
                    QtyScrapped = 0m,
                    UOM = amProdItem.UOM,
                    TranType = amTranType,
                    LaborTime = laborTime,
                    LocationID = amProdItem.LocationID,
                    OperationID = costOperationId,
                    InventoryID = amProdItem.InventoryID,
                    SubItemID = amProdItem.SubItemID,
                    SiteID = amProdItem.SiteID,
                    TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                    TranDesc = transactionDescription,
                    TranAmt = tranAmount,
                    LaborCodeID = ammTran.LaborCodeID,
                    AcctID = offsetAccount,
                    SubID = offsetSub,
                    WIPAcctID = WIPacct,
                    WIPSubID = WIPsub,
                    OrigDocType = ammTran.DocType,
                    OrigBatNbr = ammTran.BatNbr,
                    OrigLineNbr = ammTran.LineNbr,
                    ReferenceCostID = referenceCostId.TrimIfNotNull()
                });

                if (tranAmount.GetValueOrDefault() == 0)
                {
                    return;
                }
                
                //Wip is a debit when transaction amount > 0 (increasing wip); else wip is a credit (decreasing wip)
                var wiPisDebit = tranAmount.GetValueOrDefault() > 0;

                #region GL Batch - if missing, create

                if (_journalEntryGraph.BatchModule.Current == null)
                {
                    var glbatch = _journalEntryGraph.BatchModule.Insert(new Batch());                                        
                    glbatch.Hold = true;
                    glbatch.OrigModule = Common.ModuleAM;
                    glbatch.DateEntered = ammTran.TranDate;
                    glbatch.FinPeriodID = ammTran.FinPeriodID;
                    glbatch.Description = Messages.ProdGLEntry_ProdTranGeneric;

                    if (_productionCostEntryGraph.batch.Current != null)
                    {
                        //Set extension values for batch reference
                        BatchExt extension = glbatch.GetExtension<BatchExt>();
                        extension.AMDocType = _productionCostEntryGraph.batch.Current.DocType;
                        extension.AMBatNbr = _productionCostEntryGraph.batch.Current.BatNbr;
                    }

                    glbatch = _journalEntryGraph.BatchModule.Update(glbatch);
                    _journalEntryGraph.Persist();
                }
                #endregion

                #region Debit Transaction

                var gltranDEBIT = new GLTran
                {
                    AccountID = wiPisDebit ? WIPacct : offsetAccount,
                    SubID = wiPisDebit ? WIPsub : offsetSub,
                    RefNbr = amProdItem.ProdOrdID,
                    CreditAmt = 0m,
                    CuryCreditAmt = 0m,
                    DebitAmt = Math.Abs(tranAmount.GetValueOrDefault()),
                    CuryDebitAmt = Math.Abs(tranAmount.GetValueOrDefault()),
                    TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                    FinPeriodID = ammTran.FinPeriodID,
                    OrigModule = Common.ModuleAM,
                    TranDesc = transactionDescription
                };

                if(ProjectHelper.IsProjectAcctWithProjectEnabled(_journalEntryGraph, gltranDEBIT.AccountID))
                {
                    gltranDEBIT.ProjectID = wiPisDebit ? ammTran.ProjectID : ProjectDefaultAttribute.NonProject();
                    gltranDEBIT.TaskID = wiPisDebit ? ammTran.TaskID : null;
                    gltranDEBIT.CostCodeID = wiPisDebit ? ammTran.CostCodeID : null;
                }
                gltranDEBIT.Qty = wiPisDebit && ammTran.DocType == AMDocType.Labor ? ammTran.LaborTime.ToHours() : gltranDEBIT.Qty.GetValueOrDefault();

                if (!string.IsNullOrWhiteSpace(ammTranGL.DocType) &&
                    !string.IsNullOrWhiteSpace(ammTranGL.BatNbr) &&
                    ammTranGL.LineNbr != null)
                {
                    gltranDEBIT.TranType = ammTranGL.DocType;
                    gltranDEBIT.OrigBatchNbr = ammTranGL.BatNbr;
                    gltranDEBIT.OrigLineNbr = ammTranGL.LineNbr;
                }
                    
                gltranDEBIT.SummPost = _summaryPostGl;

                gltranDEBIT = _journalEntryGraph.GLTranModuleBatNbr.Insert(gltranDEBIT);

                if (gltranDEBIT == null)
                {
                    var batNbr = _journalEntryGraph?.BatchModule?.Current?.BatchNbr ?? string.Empty;
                    throw new PXException(Messages.GLBatchDebitLineMissing, batNbr, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr);
                }

                gltranDEBIT = _journalEntryGraph.GLTranModuleBatNbr.Update(gltranDEBIT);
                #endregion

                #region Credit Transaction

                var gltranCREDIT = new GLTran
                {
                    AccountID = wiPisDebit ? offsetAccount : WIPacct,
                    SubID = wiPisDebit ? offsetSub : WIPsub,
                    RefNbr = amProdItem.ProdOrdID,
                    CreditAmt = Math.Abs(tranAmount.GetValueOrDefault()),
                    CuryCreditAmt = Math.Abs(tranAmount.GetValueOrDefault()),
                    DebitAmt = 0m,
                    CuryDebitAmt = 0m,
                    TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                    FinPeriodID = ammTran.FinPeriodID,
                    OrigModule = Common.ModuleAM,
                    TranDesc = transactionDescription
                };                

                if (ProjectHelper.IsProjectAcctWithProjectEnabled(_journalEntryGraph, gltranCREDIT.AccountID))
                {
                    gltranCREDIT.ProjectID = !wiPisDebit ? ammTran.ProjectID : ProjectDefaultAttribute.NonProject();
                    gltranCREDIT.TaskID = !wiPisDebit ? ammTran.TaskID : null;
                    gltranCREDIT.CostCodeID = !wiPisDebit ? ammTran.CostCodeID : null;
                }
                gltranCREDIT.Qty = !wiPisDebit && ammTran.DocType == AMDocType.Labor ? ammTran.LaborTime.ToHours() : gltranCREDIT.Qty.GetValueOrDefault();

                if (!string.IsNullOrWhiteSpace(ammTranGL.DocType) &&
                    !string.IsNullOrWhiteSpace(ammTranGL.BatNbr) &&
                    ammTranGL.LineNbr != null)
                {
                    gltranCREDIT.TranType = ammTranGL.DocType;
                    gltranCREDIT.OrigBatchNbr = ammTranGL.BatNbr;
                    gltranCREDIT.OrigLineNbr = ammTranGL.LineNbr;
                }

                gltranCREDIT.SummPost = _summaryPostGl;

                gltranCREDIT = _journalEntryGraph.GLTranModuleBatNbr.Insert(gltranCREDIT);

                if (gltranCREDIT == null)
                {
                    var batNbr = _journalEntryGraph?.BatchModule?.Current?.BatchNbr ?? string.Empty;
                    throw new PXException(Messages.GLBatchCreditLineMissing, batNbr, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr);
                }

                //Reference back into AMMTran
                ammTranGL.GLBatNbr = wiPisDebit ? gltranCREDIT.BatchNbr : gltranDEBIT.BatchNbr;
                ammTranGL.GLLineNbr = wiPisDebit ?  gltranCREDIT.LineNbr : gltranDEBIT.LineNbr;

                _journalEntryGraph.GLTranModuleBatNbr.Update(gltranCREDIT);
                #endregion

                _productionCostEntryGraph.transactions.Update(ammTranGL);
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw new PXException(Messages.ErrorInAMGLentry, e.Message);
            }
        }


        public void SyncJournalEntryBatch(AMBatch doc)
        {
            if (_journalEntryGraph.BatchModule.Current == null)
            {
                return;
            }

            var glbatch = (Batch)_journalEntryGraph.BatchModule.Cache.CreateCopy(_journalEntryGraph.BatchModule.Current);
            glbatch.DateEntered = doc.TranDate;
            glbatch.FinPeriodID = doc.FinPeriodID;
            glbatch.Description = doc.TranDesc;
            _journalEntryGraph.BatchModule.Update(glbatch);
        }

        /// <summary>
        /// Creating a GL and AM transaction for indirect labor production offset entries.
        /// Overhead Account is a debit when the transaction amount is a positive number (credit when negative).
        /// The otherside of the GL entry are the passed Labor accounts/sub fields.
        /// </summary>
        /// <param name="amLaborCode">Root AMLaborCode record use to get the accounts for the gl entry</param>
        /// <param name="ammTran">Root ammtran record generating the gl entry</param>
        /// <param name="tranAmount">GL Transaction amount</param>
        /// <param name="amTranType">AM Transaction type (Pass a selection from AMMTran.AMTranType)</param>
        public void CreateAmGlLine(
            AMLaborCode amLaborCode,
            AMMTran ammTran,
            decimal? tranAmount,
            string amTranType,
            string transactionDescription = ""
            )
        {

            if (string.IsNullOrWhiteSpace(transactionDescription))
            {
                transactionDescription = AMTranType.GetTranDescription(amTranType);
            }

            try
            {
                bool isDebit;                //Used later for control if Overhead is the debit (true) or credit (false)
                
                #region Period to Post Check
                    if (string.IsNullOrEmpty(ammTran.FinPeriodID))
                    {
                        //Must pass the period to post
                        throw new Exception(Messages.InvalidPeriodToPost);
                    }
                    #endregion
            
                //  Only create AM & GL tran lines if an actual tran amount is supplied.
                if (tranAmount.GetValueOrDefault() != 0m)
                {
                    //  ------------------------------------------------------------------------------------------------
                    //  All params at this point should contain all the necessary information to complete the GL Entry.
                    //  ------------------------------------------------------------------------------------------------

                    CreateBatchRecord(ammTran);

                    #region GL Batch - if missing, create

                    if (_journalEntryGraph.BatchModule.Current == null)
                    {
                        Batch glbatch = new Batch();
                        glbatch = PXCache<Batch>.CreateCopy(_journalEntryGraph.BatchModule.Insert(glbatch));
                        glbatch.Hold = false; //Not on hold
                        glbatch.Status = BatchStatus.Balanced;
                        glbatch.OrigModule = Common.ModuleAM;
                        glbatch.FinPeriodID = ammTran.FinPeriodID;
                        glbatch.Description = Messages.ProdGLEntry_ProdTranGeneric;

                        if (_productionCostEntryGraph.batch.Current != null)
                        {
                            //Set extension values for batch reference
                            BatchExt extension = PXCache<Batch>.GetExtension<BatchExt>(glbatch);
                            extension.AMDocType = _productionCostEntryGraph.batch.Current.DocType;
                            extension.AMBatNbr = _productionCostEntryGraph.batch.Current.BatNbr;
                        }

                        glbatch = _journalEntryGraph.BatchModule.Update(glbatch);
                        _journalEntryGraph.Persist();
                    }

                    #endregion

                    //Wip is a debit when transaction amount > 0 (increasing wip); else wip is a credit (decreasing wip)
                    isDebit = tranAmount.GetValueOrDefault() > 0;

                    #region Start AMMTranGL line
                    
                    var ammTranGL = new AMMTran
                    {
                        ProdOrdID = null,
                        Qty = 0m,
                        QtyScrapped = 0m,
                        TranType = amTranType,
                        TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                        TranDesc = transactionDescription,
                        TranAmt = tranAmount,
                        LaborCodeID = amLaborCode.LaborCodeID,
                        AcctID = amLaborCode.LaborAccountID,
                        SubID = amLaborCode.LaborSubID,
                        WIPAcctID = amLaborCode.OverheadAccountID,
                        WIPSubID = amLaborCode.OverheadSubID,
                        OrigDocType = ammTran.DocType,
                        OrigBatNbr = ammTran.BatNbr,
                        OrigLineNbr = ammTran.LineNbr,
                        LaborType = ammTran.LaborType
                    };

                    ammTranGL = _productionCostEntryGraph.transactions.Insert(ammTranGL);

                    #endregion

                    #region Debit Transaction

                    var gltranDEBIT = new GLTran
                    {
                        AccountID = isDebit ? amLaborCode.OverheadAccountID : amLaborCode.LaborAccountID,
                        SubID = isDebit ? amLaborCode.OverheadSubID : amLaborCode.LaborSubID,
                        RefNbr = amLaborCode.LaborCodeID,
                        CreditAmt = 0m,
                        CuryCreditAmt = 0m,
                        DebitAmt = tranAmount,
                        CuryDebitAmt = tranAmount,
                        TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                        FinPeriodID = ammTran.FinPeriodID,
                        OrigModule = Common.ModuleAM,
                        TranDesc = ammTranGL.TranDesc
                    };

                    if (!string.IsNullOrWhiteSpace(ammTranGL.DocType) &&
                        !string.IsNullOrWhiteSpace(ammTranGL.BatNbr) &&
                        ammTranGL.LineNbr != null)
                    {
                        gltranDEBIT.TranType = ammTranGL.DocType;
                        gltranDEBIT.OrigBatchNbr = ammTranGL.BatNbr;
                        gltranDEBIT.OrigLineNbr = ammTranGL.LineNbr;
                    }

                    gltranDEBIT.SummPost = _summaryPostGl;

                    gltranDEBIT = _journalEntryGraph.GLTranModuleBatNbr.Insert(gltranDEBIT);
                    if (gltranDEBIT == null)
                    {
                        var batNbr = _journalEntryGraph?.BatchModule?.Current?.BatchNbr ?? string.Empty;
                        throw new PXException(Messages.GLBatchDebitLineMissing, batNbr, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr);
                    }

                    gltranDEBIT.ProjectID = ProjectDefaultAttribute.NonProject();
                    gltranDEBIT = _journalEntryGraph.GLTranModuleBatNbr.Update(gltranDEBIT);

                    #endregion

                    #region Credit Transaction

                    var gltranCREDIT = new GLTran
                    {
                        AccountID = isDebit ? amLaborCode.LaborAccountID : amLaborCode.OverheadAccountID,
                        SubID = isDebit ? amLaborCode.LaborSubID : amLaborCode.OverheadSubID,
                        RefNbr = amLaborCode.LaborCodeID,
                        CreditAmt = tranAmount,
                        CuryCreditAmt = tranAmount,
                        DebitAmt = 0m,
                        CuryDebitAmt = 0m,
                        TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                        FinPeriodID = ammTran.FinPeriodID,
                        OrigModule = Common.ModuleAM,
                        TranDesc = ammTranGL.TranDesc
                    };

                    if (!string.IsNullOrWhiteSpace(ammTranGL.DocType) &&
                        !string.IsNullOrWhiteSpace(ammTranGL.BatNbr) &&
                        ammTranGL.LineNbr != null)
                    {
                        gltranCREDIT.TranType = ammTranGL.DocType;
                        gltranCREDIT.OrigBatchNbr = ammTranGL.BatNbr;
                        gltranCREDIT.OrigLineNbr = ammTranGL.LineNbr;
                    }

                    gltranCREDIT.SummPost = _summaryPostGl;

                    gltranCREDIT = _journalEntryGraph.GLTranModuleBatNbr.Insert(gltranCREDIT);

                    if (gltranCREDIT == null)
                    {
                        var batNbr = _journalEntryGraph?.BatchModule?.Current?.BatchNbr ?? string.Empty;
                        throw new PXException(Messages.GLBatchCreditLineMissing, batNbr, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr);
                    }

                    //Reference back into AMMTran
                    ammTranGL.GLBatNbr = isDebit ? gltranCREDIT.BatchNbr : gltranDEBIT.BatchNbr;
                    ammTranGL.GLLineNbr = isDebit ? gltranCREDIT.LineNbr : gltranDEBIT.LineNbr;

                    gltranCREDIT.ProjectID = ProjectDefaultAttribute.NonProject();
                    _journalEntryGraph.GLTranModuleBatNbr.Update(gltranCREDIT);

                    #endregion

                    _productionCostEntryGraph.transactions.Update(ammTranGL);
                }

            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw new PXException(Messages.ErrorInAMGLentry, e.Message);
            }
        }

        #endregion

        protected static bool WipIsDebit(AMMTran tran)
        {
            var posAmount = tran.TranAmt.GetValueOrDefault() >= 0;
            var scrapTran = AMTranType.IsScrapTranType(tran.TranType);

            // When WIP (an Asset) is debit, it increases the account balance, else as credit this decreases the account balance

            return posAmount & !scrapTran || !posAmount && scrapTran;
        }

        /// <summary>
        /// Create a GL line from an AM line
        /// </summary>
        /// <param name="ammTran">Transaction to base the GL entry off of</param>
        public AMMTran CreateGlLine(AMMTran ammTran)
        {
            if (ammTran == null || ammTran.TranAmt.GetValueOrDefault() == 0)
            {
                return ammTran;
            }

            try
            {
                var wipAcctId = ammTran.WIPAcctID;
                var wipSubId = ammTran.WIPSubID;
                var offsetAcctId = ammTran.AcctID;
                var offsetSubId = ammTran.SubID;

                var sb = new StringBuilder();
                if (wipAcctId.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.wIPAcctID>(
                            _productionCostEntryGraph.Caches[typeof(AMMTran)])));
                }
                if (wipSubId.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.wIPSubID>(
                            _productionCostEntryGraph.Caches[typeof(AMMTran)])));
                }
                if (offsetAcctId.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.acctID>(
                            _productionCostEntryGraph.Caches[typeof(AMMTran)])));
                }
                if (offsetSubId.GetValueOrDefault() == 0)
                {
                    sb.AppendLine(string.Format(
                        Messages.GetLocal(ErrorMessages.FieldIsEmpty, typeof(ErrorMessages)),
                        PXUIFieldAttribute.GetDisplayName<AMMTran.subID>(
                            _productionCostEntryGraph.Caches[typeof(AMMTran)])));
                }
                if (string.IsNullOrEmpty(ammTran.FinPeriodID))
                {
                    //Must pass the period to post
                    sb.AppendLine(Messages.InvalidPeriodToPost);
                }
                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    throw new PXException(sb.ToString());
                }

                if (_journalEntryGraph.BatchModule.Current == null)
                {
                    var glbatch = new Batch();
                    glbatch = PXCache<Batch>.CreateCopy(_journalEntryGraph.BatchModule.Insert(glbatch));
                    glbatch.Hold = false; //Not on hold
                    glbatch.Status = BatchStatus.Balanced;
                    glbatch.OrigModule = Common.ModuleAM;
                    glbatch.FinPeriodID = ammTran.FinPeriodID;
                    glbatch.Description = $"{Messages.ProdGLEntry_ProdTranGeneric} - {AMDocType.GetDocTypeDesc(ammTran.DocType)}";

                    //Set extension values for batch reference
                    var extension = glbatch.GetExtension<BatchExt>();
                    extension.AMDocType = ammTran.DocType;
                    extension.AMBatNbr = ammTran.BatNbr;

                    glbatch = _journalEntryGraph.BatchModule.Update(glbatch);
                    _journalEntryGraph.Persist();
                }

                var wipIsDebit = WipIsDebit(ammTran);

                var gltranDEBIT = new GLTran
                {
                    AccountID = wipIsDebit ? wipAcctId : offsetAcctId,
                    SubID = wipIsDebit ? wipSubId : offsetSubId,
                    RefNbr = ammTran.ProdOrdID,
                    CreditAmt = 0m,
                    CuryCreditAmt = 0m,
                    DebitAmt = Math.Abs(ammTran.TranAmt.GetValueOrDefault()),
                    CuryDebitAmt = Math.Abs(ammTran.TranAmt.GetValueOrDefault()),
                    TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                    FinPeriodID = ammTran.FinPeriodID,
                    OrigModule = Common.ModuleAM,
                    TranDesc = ammTran.TranDesc,
                    TranType = ammTran.DocType,
                    OrigBatchNbr = ammTran.BatNbr,
                    OrigLineNbr = ammTran.LineNbr,
                    SummPost = _summaryPostGl
                };

                if (ProjectHelper.IsProjectAcctWithProjectEnabled(_journalEntryGraph, gltranDEBIT.AccountID))
                {
                    gltranDEBIT.ProjectID = wipIsDebit ? ammTran.ProjectID : ProjectDefaultAttribute.NonProject();
                    gltranDEBIT.TaskID = wipIsDebit ? ammTran.TaskID : null;
                    gltranDEBIT.CostCodeID = wipIsDebit ? ammTran.CostCodeID : null;
                }
                gltranDEBIT.Qty = wipIsDebit && ammTran.DocType == AMDocType.Labor ? ammTran.LaborTime.ToHours() : gltranDEBIT.Qty.GetValueOrDefault();

                gltranDEBIT = _journalEntryGraph.GLTranModuleBatNbr.Insert(gltranDEBIT);
                if (gltranDEBIT == null)
                {
                    var batNbr = _journalEntryGraph?.BatchModule?.Current?.BatchNbr ?? string.Empty;
                    throw new PXException(Messages.GLBatchDebitLineMissing, batNbr, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr);
                }

                gltranDEBIT = _journalEntryGraph.GLTranModuleBatNbr.Update(gltranDEBIT);

                var gltranCREDIT = new GLTran
                {
                    AccountID = wipIsDebit ? offsetAcctId : wipAcctId,
                    SubID = wipIsDebit ? offsetSubId : wipSubId,
                    RefNbr = ammTran.ProdOrdID,
                    CreditAmt = Math.Abs(ammTran.TranAmt.GetValueOrDefault()),
                    CuryCreditAmt = Math.Abs(ammTran.TranAmt.GetValueOrDefault()),
                    DebitAmt = 0m,
                    CuryDebitAmt = 0m,
                    TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate,
                    FinPeriodID = ammTran.FinPeriodID,
                    OrigModule = Common.ModuleAM,
                    TranDesc = ammTran.TranDesc,
                    TranType = ammTran.DocType,
                    OrigBatchNbr = ammTran.BatNbr,
                    OrigLineNbr = ammTran.LineNbr,
                    SummPost = _summaryPostGl
                };

                if (ProjectHelper.IsProjectAcctWithProjectEnabled(_journalEntryGraph, gltranCREDIT.AccountID))
                {
                    gltranCREDIT.ProjectID = !wipIsDebit ? ammTran.ProjectID : ProjectDefaultAttribute.NonProject();
                    gltranCREDIT.TaskID = !wipIsDebit ? ammTran.TaskID : null;
                    gltranCREDIT.CostCodeID = !wipIsDebit ? ammTran.CostCodeID : null;
                }
                gltranCREDIT.Qty = !wipIsDebit && ammTran.DocType == AMDocType.Labor ? ammTran.LaborTime.ToHours() : gltranCREDIT.Qty.GetValueOrDefault();

                gltranCREDIT = _journalEntryGraph.GLTranModuleBatNbr.Insert(gltranCREDIT);
                if (gltranCREDIT == null)
                {
                    var batNbr = _journalEntryGraph?.BatchModule?.Current?.BatchNbr ?? string.Empty;
                    throw new PXException(Messages.GLBatchCreditLineMissing, batNbr, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr);
                }

                //Reference back into AMMTran
                ammTran.GLBatNbr = wipIsDebit ? gltranCREDIT.BatchNbr : gltranDEBIT.BatchNbr;
                ammTran.GLLineNbr = wipIsDebit ? gltranCREDIT.LineNbr : gltranDEBIT.LineNbr;

                _journalEntryGraph.GLTranModuleBatNbr.Update(gltranCREDIT);
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw new PXException(Messages.GetLocal(Messages.ErrorInCreateGLLine), AMDocType.GetDocTypeDesc(ammTran.DocType), ammTran.BatNbr, ammTran.LineNbr);
            }

            return ammTran;
        }

        /// <summary>
        /// Create the AMBatch header record if missing
        /// </summary>
        private AMBatch CreateBatchRecord(AMMTran sourceTran)
        {
            var batch = _productionCostEntryGraph.batch.Current;

            if (batch == null)
            {
                var amBatch = PXCache<AMBatch>.CreateCopy(_productionCostEntryGraph.batch.Insert(new AMBatch()));
                amBatch.DocType = AMDocType.ProdCost;
                amBatch.Hold = false;
                amBatch.Status = DocStatus.Balanced;
                amBatch.TranDate = sourceTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate;
                amBatch.FinPeriodID = sourceTran.FinPeriodID;
                amBatch.TranDesc = Messages.ProdGLEntry_ProdTranGeneric;
                amBatch.OrigBatNbr = sourceTran.BatNbr;
                amBatch.OrigDocType = sourceTran.DocType;
                _productionCostEntryGraph.batch.Update(amBatch);
                //not ideal based on using up a transaction but having a few issues with using Persist(PXDBOperation.Insert) with Persist(false)
                _productionCostEntryGraph.Persist();
                batch = _productionCostEntryGraph.batch.Current;
            }
            return batch;
        }

        /// <summary>
        /// Adding cost batch entires that do not impact the GL (Ex: WIP Complete by operation numbers)
        /// </summary>
        /// <returns></returns>
        public AMMTran CreateNonGLCostBatchEntry(
            AMMTran moveTran,
            int? costOperationId,
            decimal? tranAmount,
            decimal? operationMoveQty,
            string amTranType,
            string transactionDescription)
        {
            if (tranAmount.GetValueOrDefault() == 0)
            {
                return null;
            }

            if (moveTran == null)
            {
                throw new PXArgumentException(nameof(moveTran));
            }

            if (string.IsNullOrWhiteSpace(amTranType))
            {
                throw new PXArgumentException(nameof(amTranType));
            }

            if (costOperationId == null)
            {
                throw new PXArgumentException(nameof(costOperationId));
            }

            CreateBatchRecord(moveTran);

            var ammTran = _productionCostEntryGraph.transactions.Insert(new AMMTran
            {
                TranType = amTranType,
                OrderType = moveTran.OrderType,
                ProdOrdID = moveTran.ProdOrdID,
                OperationID = costOperationId
            });

            ammTran.Qty = operationMoveQty.GetValueOrDefault();
            ammTran.QtyScrapped = 0m;
            ammTran.UOM = moveTran.UOM;
            ammTran.TranType = amTranType;
            ammTran.LocationID = moveTran.LocationID;
            ammTran.InventoryID = moveTran.InventoryID;
            ammTran.SubItemID = moveTran.SubItemID;
            ammTran.SiteID = moveTran.SiteID;
            ammTran.TranDate = ammTran.TranDate ?? _journalEntryGraph.Accessinfo.BusinessDate;
            ammTran.TranDesc = transactionDescription;
            ammTran.TranAmt = tranAmount;
            ammTran.OrigDocType = moveTran.DocType;
            ammTran.OrigBatNbr = moveTran.BatNbr;
            ammTran.OrigLineNbr = moveTran.LineNbr;

            return _productionCostEntryGraph.transactions.Update(ammTran);
        }

        protected virtual Dictionary<int, AMMTran> FindOperationWipCompleteRecords(AMMTran sourceTran)
        {
            var dic = new Dictionary<int, AMMTran>();
            foreach (AMMTran wipComplete in _productionCostEntryGraph.transactions.Select())
            {
                if (wipComplete.OperationID == null)
                {
                    continue;
                }

                if (wipComplete.TranType == AMTranType.OperWIPComplete
                    && wipComplete.OrigDocType.Equals(sourceTran.DocType)
                    && wipComplete.OrigBatNbr.Equals(sourceTran.BatNbr)
                    && wipComplete.OrigLineNbr.Equals(sourceTran.LineNbr))
                {
                    dic.Add(wipComplete.OperationID.GetValueOrDefault(), wipComplete);
                }
            }
            return dic;
        }

        public virtual void SetOperationWipCompleteRecords(AMMTran sourceTran, ProductionOperationCostResults costResults)
        {
            if (sourceTran?.OperationID == null)
            {
                throw new PXArgumentException(nameof(sourceTran));
            }

            if (costResults == null)
            {
                throw new PXArgumentException(nameof(costResults));
            }

            var existingWipCompleteRecordsDictionary = FindOperationWipCompleteRecords(sourceTran);

            var runningTotal = 0m;
            foreach (var costResult in costResults.OperationResults)
            {
                var tranCostAmount = costResult.UnitAmount * sourceTran.Qty.GetValueOrDefault();
                var noWipComp = tranCostAmount == 0;

                if (existingWipCompleteRecordsDictionary.TryGetValue(costResult.ProdOper.OperationID.GetValueOrDefault(), out var wipCompRow))
                {
                    if (noWipComp)
                    {
                        _productionCostEntryGraph.transactions.Delete(wipCompRow);
                        continue;
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"[{wipCompRow.DocType}-{wipCompRow.BatNbr}-{wipCompRow.LineNbr}] Updating WIP Comp Record for order {wipCompRow.OrderType} {wipCompRow.ProdOrdID} {wipCompRow.OperationID}. TranAmt from {wipCompRow.TranAmt} to {tranCostAmount}");

#endif

                    //UPDATE...
                    wipCompRow.OrderType = costResult.ProdOper.OrderType;
                    wipCompRow.ProdOrdID = costResult.ProdOper.ProdOrdID;
                    wipCompRow.OperationID = costResult.ProdOper.OperationID;
                    wipCompRow.InventoryID = sourceTran.InventoryID;
                    wipCompRow.UOM = sourceTran.UOM;
                    wipCompRow.TranAmt = tranCostAmount;
                    wipCompRow.Qty = sourceTran.Qty.GetValueOrDefault();
                    runningTotal += costResult.TotalAmount;
                    _productionCostEntryGraph.transactions.Update(wipCompRow);
                    continue;
                }

                if (noWipComp)
                {
                    continue;
                }

                runningTotal += tranCostAmount;

                CreateNonGLCostBatchEntry(
                    sourceTran,
                    costResult.ProdOper.OperationID,
                    tranCostAmount,
                    sourceTran.Qty.GetValueOrDefault(),
                    AMTranType.OperWIPComplete,
                    ProductionTransactionHelper.BuildTransactionDescription(AMTranType.OperWIPComplete, costResult.ProdOper.OperationCD));
            }

            if (UomHelper.PriceCostRound(runningTotal) != UomHelper.PriceCostRound(costResults.TotalAmount))
            {
                PXTraceHelper.WriteInformation($"[{sourceTran.DocType}-{sourceTran.BatNbr}-{sourceTran.LineNbr}] Building Oper WIP Complete Transactions: RT value {runningTotal} not equal to calculated total {costResults.TotalAmount}");
            }
        }
    }
}
