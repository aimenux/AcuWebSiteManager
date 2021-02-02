using System;
using Customization;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class Upgrade2018R1Ver29 : UpgradeProcessVersionBase
    {
        public Upgrade2018R1Ver29(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public Upgrade2018R1Ver29(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new Upgrade2018R1Ver29(upgradeGraph, plugin);
            upgrade._upgradeFromVersion = upgradeFrom;

            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();
                return true;
            }
            return false;
        }

        private int _upgradeFromVersion;
        public override int Version => UpgradeVersions.Version2018R1Ver29;

        private static int PreviousVersion => UpgradeVersions.Version2017R2Ver50;
        private bool UpgradeRanInPreviousVersion => _upgradeFromVersion.BetweenInclusive(PreviousVersion, UpgradeVersions.MaxVersionNumbers.Version2017R2);

        public override void ProcessTables()
        {
            if (UpgradeRanInPreviousVersion)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"Process ran in previous version {PreviousVersion}; Upgrading from {_upgradeFromVersion}");
#endif
                return;
            }

            UpdateProductionOrderWipTotal();

            // Result of bug 1991 - we need to delete orphaned batches that the user cannot delete and would cause a hang on production order close (or GL and IN close)
            DeleteOrphanAMBatches(); //Run before GL as cost batch delete will leave GL batches
            DeleteOrphanINBatches();
            DeleteOrphanGLBatches();
        }

        /// <summary>
        /// Related to bug 2042 - Wip Total numbers not updated correctly when wip adjustments exist. Re-calculate the wip total using the correct formula.
        /// </summary>
        private void UpdateProductionOrderWipTotal()
        {
#if DEBUG
            /*
                UPDATE [AMProdItem]
                SET [AMProdItem].[WIPTotal] = [AMProdTotal].[ActualLabor]
                                              + ([AMProdTotal].[ActualMachine]
                                                 + ([AMProdTotal].[ActualMaterial]
                                                    + ([AMProdTotal].[ActualTool]
                                                       + ([AMProdTotal].[ActualFixedOverhead]
                                                          + ([AMProdTotal].[ActualVariableOverhead]
                                                             + ([AMProdTotal].[WIPAdjustment] - [AMProdTotal].[ScrapAmount]))))))
                FROM [AMProdItem] [AMProdItem]
                    INNER JOIN [AMProdTotal] [AMProdTotal]
                        ON [AMProdTotal].CompanyID = 2
                           AND [AMProdItem].[OrderType] = [AMProdTotal].[OrderType]
                           AND [AMProdItem].[ProdOrdID] = [AMProdTotal].[ProdOrdID]
                WHERE [AMProdItem].CompanyID = 2
                      AND [AMProdTotal].[WIPAdjustment] <> .0
                      AND [AMProdItem].[WIPTotal] <> (([AMProdTotal].[ActualLabor]
                                                       + ([AMProdTotal].[ActualMachine]
                                                          + ([AMProdTotal].[ActualMaterial]
                                                             + ([AMProdTotal].[ActualTool]
                                                                + ([AMProdTotal].[ActualFixedOverhead]
                                                                   + ([AMProdTotal].[ActualVariableOverhead]
                                                                      + ([AMProdTotal].[WIPAdjustment] - [AMProdTotal].[ScrapAmount]))))))));
             */
#endif
            PXUpdateJoin<
                Set<AMProdItem.wIPTotal, Standalone.AMProdTotal.wIPTotal>,
                AMProdItem,
                InnerJoin<Standalone.AMProdTotal, 
                    On<AMProdItem.orderType, Equal<Standalone.AMProdTotal.orderType>,
                        And<AMProdItem.prodOrdID, Equal<Standalone.AMProdTotal.prodOrdID>>>>,
                Where<Standalone.AMProdTotal.wIPAdjustment, NotEqual<decimal0>,
                    And<AMProdItem.wIPTotal, NotEqual<Standalone.AMProdTotal.wIPTotal>>>>
                .Update(_upgradeGraph);
        }

        private void DeleteOrphanGLBatches()
        {
            var journalEntry = PXGraph.CreateInstance<JournalEntry>();
            
            var batchDeletedCntr = 0;
            var batchErrorCntr = 0;
            var sbErrors = new System.Text.StringBuilder();

            foreach (Batch doc in PXSelectJoin<
                    Batch,
                    LeftJoin<AMBatchOrigDoc,
                        On<AMBatchOrigDoc.docType, Equal<BatchExt.aMDocType>,
                            And<AMBatchOrigDoc.batNbr, Equal<BatchExt.aMBatNbr>>>>,
                    Where<Batch.released, Equal<False>,
                        And<BatchExt.aMBatNbr, IsNotNull,
                            And<AMBatchOrigDoc.batNbr, IsNull>>>>
                .Select(_upgradeGraph))
            {
                var docExt = doc?.GetExtension<BatchExt>();
                if (docExt?.AMBatNbr == null || doc?.Released == true)
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"Unreleased GL batch {doc.BatchNbr} related to missing {AMDocType.GetDocTypeDesc(docExt.AMDocType)} batch {docExt.AMBatNbr}");
#endif

                if (TryDeleteDocument(journalEntry, doc, out var ex))
                {
                    batchDeletedCntr++;
                    continue;
                }

                if (ex == null)
                {
                    continue;
                }

                batchErrorCntr++;
                sbErrors.AppendLine($"{doc.BatchNbr}: {ex.Message}.");
            }

            if (batchDeletedCntr > 0)
            {
                WriteInfo($"Deleted {batchDeletedCntr} orphaned unreleased GL transactions.");
            }

            if (batchErrorCntr > 0)
            {
                WriteInfo($"Unable to delete {batchErrorCntr} GL transactions: {sbErrors}");
            }
        }

        private void DeleteOrphanINBatches()
        {
            INIssueEntry issueEntry = null;
            INReceiptEntry receiptEntry = null;
            INAdjustmentEntry adjustmentEntry = null;

            var batchDeletedCntr = 0;
            var batchErrorCntr = 0;
            var sbErrors = new System.Text.StringBuilder();

            foreach (INRegister doc in PXSelectJoin<
                    INRegister,
                    LeftJoin<AMBatchOrigDoc,
                        On<AMBatchOrigDoc.docType, Equal<INRegisterExt.aMDocType>,
                            And<AMBatchOrigDoc.batNbr, Equal<INRegisterExt.aMBatNbr>>>>,
                    Where<INRegister.released, Equal<False>,
                        And<INRegisterExt.aMBatNbr, IsNotNull,
                            And<AMBatchOrigDoc.batNbr, IsNull>>>>
                .Select(_upgradeGraph))
            {
                var docExt = doc?.GetExtension<INRegisterExt>();
                if (docExt?.AMBatNbr == null || doc?.Released == true)
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"Unreleased inventory ({doc.DocType}) batch {doc.RefNbr} related to missing {AMDocType.GetDocTypeDesc(docExt.AMDocType)} batch {docExt.AMBatNbr}");
#endif
                PXGraph graph = null;
                switch (doc.DocType)
                {
                    case INDocType.Issue:
                        if (issueEntry == null)
                        {
                            issueEntry = PXGraph.CreateInstance<INIssueEntry>();
                        }

                        graph = issueEntry;
                        break;
                    case INDocType.Receipt:
                        if (receiptEntry == null)
                        {
                            receiptEntry = PXGraph.CreateInstance<INReceiptEntry>();
                        }

                        graph = receiptEntry;
                        break;
                    case INDocType.Adjustment:
                        if (adjustmentEntry == null)
                        {
                            adjustmentEntry = PXGraph.CreateInstance<INAdjustmentEntry>();
                        }

                        graph = adjustmentEntry;
                        break;
                }

                if (graph == null)
                {
                    continue;
                }

                if (TryDeleteDocument(graph, doc, out var ex))
                {
                    batchDeletedCntr++;
                    continue;
                }

                if (ex == null)
                {
                    continue;
                }

                batchErrorCntr++;
                sbErrors.AppendLine($"{doc.DocType} - {doc.RefNbr}: {ex.Message}.");
            }

            if (batchDeletedCntr > 0)
            {
                WriteInfo($"Deleted {batchDeletedCntr} orphaned unreleased inventory transactions.");
            }

            if (batchErrorCntr > 0)
            {
                WriteInfo($"Unable to delete {batchErrorCntr} inventory transactions: {sbErrors}");
            }
        }

        private void DeleteOrphanAMBatches()
        {
            /*
                // List the batches we need to delete...
                SELECT c.DocType,
                       c.BatNbr,
                       c.CreatedDateTime,
                       c.OrigDocType,
                       c.OrigBatNbr,
                       o.DocType AS OrigDocTypeBatch,
                       o.BatNbr AS OrigBatNbrBatch
                FROM dbo.AMBatch c
                    LEFT JOIN dbo.AMBatch o
                        ON o.CompanyID = c.CompanyID
                           AND o.DocType = c.OrigDocType
                           AND o.BatNbr = c.OrigBatNbr
                WHERE c.Released = 0
                      AND c.OrigBatNbr IS NOT NULL
                      AND o.BatNbr IS NULL;
             */

            var batchDeletedCntr = 0;
            var batchErrorCntr = 0;
            var sbErrors = new System.Text.StringBuilder();

            MaterialEntry materialEntry = null;
            ProductionCostEntry productionCostEntry = null;
            WIPAdjustmentEntry wipAdjustmentEntry = null;

            foreach (AMBatch doc in PXSelectJoin<
                AMBatch,
                LeftJoin<AMBatchOrigDoc,
                    On<AMBatchOrigDoc.docType, Equal<AMBatch.origDocType>,
                    And<AMBatchOrigDoc.batNbr, Equal<AMBatch.origBatNbr>>>>,
                Where<AMBatch.released, Equal<False>,
                    And<AMBatch.origBatNbr, IsNotNull,
                    And<AMBatchOrigDoc.batNbr, IsNull>>>>
                .Select(_upgradeGraph))
            {
                if (string.IsNullOrWhiteSpace(doc?.BatNbr))
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName($"Unreleased {AMDocType.GetDocTypeDesc(doc.DocType)} batch {doc.BatNbr} related to missing {AMDocType.GetDocTypeDesc(doc.OrigDocType)} batch {doc.OrigBatNbr}");
#endif
                PXGraph graph = null;
                switch (doc.DocType)
                {
                    case AMDocType.Material:
                    {
                        if (materialEntry == null)
                        {
                            materialEntry = PXGraph.CreateInstance<MaterialEntry>();
                        }

                        graph = materialEntry;
                        break;
                    }
                    case AMDocType.ProdCost:
                    {
                        if (productionCostEntry == null)
                        {
                            productionCostEntry = PXGraph.CreateInstance<ProductionCostEntry>();
                        }
                        graph = productionCostEntry;
                        break;
                    }
                    case AMDocType.WipAdjust:
                    {
                        if (wipAdjustmentEntry == null)
                        {
                            wipAdjustmentEntry = PXGraph.CreateInstance<WIPAdjustmentEntry>();
                        }

                        graph = wipAdjustmentEntry;
                        break;
                    }
                }

                if (graph == null)
                {
                    continue;
                }

                if (TryDeleteDocument(graph, doc, out var ex))
                {
                    batchDeletedCntr++;
                    continue;
                }

                if (ex == null)
                {
                    continue;
                }

                batchErrorCntr++;
                sbErrors.AppendLine($"{AMDocType.GetDocTypeDesc(doc.DocType)} - {doc.BatNbr}: {ex.Message}.");
            }

            if (batchDeletedCntr > 0)
            {
                WriteInfo($"Deleted {batchDeletedCntr} orphaned unreleased production transactions.");
            }

            if (batchErrorCntr > 0)
            {
                WriteInfo($"Unable to delete {batchErrorCntr} production transactions: {sbErrors}");
            }
        }
    }

    [Serializable]
    [PXHidden]
    public class AMBatchOrigDoc : AMBatch
    {
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        public new abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }
    }
}