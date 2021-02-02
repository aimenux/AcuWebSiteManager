using System.Collections;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Linq;
using PX.Objects.AM.GraphExtensions;
using PX.Common;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Estimate Item Entry Graph
    /// </summary>    
    public class EstimateMaint : PXRevisionableGraph<EstimateMaint, AMEstimateItem, AMEstimateItem.estimateID, AMEstimateItem.revisionID>
    {
        //Primary view "Documents" comes from PXRevisionablegraph

        //Redeclare buttons as we need to change the save button to save-cancel but keep it in the correct order in the UI...
        public new PXSaveCancel<AMEstimateItem> Save;
        public new PXRevisionableCancel<EstimateMaint, AMEstimateItem, AMEstimateItem.estimateID, AMEstimateItem.revisionID> Cancel;
        public new PXRevisionableInsert<AMEstimateItem> Insert;
        public new PXDelete<AMEstimateItem> Delete;
        public new PXCopyPasteAction<AMEstimateItem> CopyPaste;
        public new PXRevisionableFirst<AMEstimateItem, AMEstimateItem.estimateID, AMEstimateItem.revisionID> First;
        public new PXRevisionablePrevious<AMEstimateItem, AMEstimateItem.estimateID, AMEstimateItem.revisionID> Previous;
        public new PXRevisionableNext<AMEstimateItem, AMEstimateItem.estimateID, AMEstimateItem.revisionID> Next;
        public new PXRevisionableLast<AMEstimateItem, AMEstimateItem.estimateID, AMEstimateItem.revisionID> Last;

        public PXSelect<AMEstimateItem, Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
            And<AMEstimateItem.revisionID, Equal<Current<AMEstimateItem.revisionID>>>>> EstimateRecordSelected;

        public PXSetup<AMEstimateSetup> EstimateSetup;

        [PXImport(typeof(AMEstimateItem))]
        public PXSelect<AMEstimateOper, Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
                And<AMEstimateOper.revisionID, Equal<Current<AMEstimateItem.revisionID>>>>,
            OrderBy<Asc<AMEstimateOper.operationCD>>> EstimateOperRecords;

        public PXSelect<AMEstimateReference,
            Where<AMEstimateReference.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
                And<AMEstimateReference.revisionID, Equal<Current<AMEstimateItem.revisionID>>>>> EstimateReferenceRecord;

        [PXCopyPasteHiddenView]
        public PXSelect<AMEstimateHistory, Where<AMEstimateHistory.estimateID, Equal<Current<AMEstimateItem.estimateID>>>,
            OrderBy<Desc<AMEstimateHistory.createdDateTime>>> EstimateHistoryRecords;

        public ToggleCurrency<AMEstimateItem> CurrencyView;

        [PXHidden]
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<AMEstimateItem.curyInfoID>>>> currencyinfo;

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
        public PXSelect<AMEstimateStep, Where<AMEstimateStep.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateStep.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                    And<AMEstimateStep.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> EstimateOperStepRecords;

        [PXHidden]
        public PXSelect<CROpportunity, Where<CROpportunity.quoteNoteID, Equal<Current<AMEstimateReference.opportunityQuoteID>>
        >> RelatedOpportunity;

        [PXHidden]
        public PXFilter<HistoryFilter> HistoryFilterRecord;

        [PXHidden]
        public PXFilter<AddToOrderFilter> Add2OrderFilter;

        [PXHidden]
        public PXFilter<CreateProdOrderFilter> CreateProductionOrderFilter;

        [PXHidden]
        public PXFilter<CreateBOMFilter> CreateBomItemFilter;

        public PXFilter<AMCopyEstimateFrom> CopyEstimateFromFilter;

        public PXSelect<Numbering, Where<Numbering.numberingID, Equal<Current<AMEstimateSetup.estimateNumberingID>>>> EstimateNumbering;

        [PXHidden]
        public PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID, Equal<Numbering.numberingID>>>,
            Where<AMOrderType.orderType, Equal<Current<AMOrderType.orderType>>>> ProductionNumbering;

        //Required for the BAccount selector on AMEstimateReference to work
        [PXHidden]
        public PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<AMEstimateReference.bAccountID>>>> CurrentBAccount;

        public EstimateMaint()
        {
            var estimateSetup = EstimateSetup.Current;
            if (estimateSetup == null)
            {
                throw new EstimatingSetupNotEnteredException();
            }

            ActionsMenu.AddMenuAction(MarkAsPrimary);
            ActionsMenu.AddMenuAction(CreateNewRevisionButton);
            ActionsMenu.AddMenuAction(CopyFrom);
            ActionsMenu.AddMenuAction(CreateBOM);
            ActionsMenu.AddMenuAction(CreateProdOrder);

            ActionsMenu.AddMenuAction(CreateInventory);
            ActionsMenu.AddMenuAction(Add2Order);

            ReportsMenu.AddMenuAction(ReportSummary);
            ReportsMenu.AddMenuAction(ReportQuote);

            EstimateHistoryRecords.AllowInsert = false;
            EstimateHistoryRecords.AllowDelete = false;
            EstimateHistoryRecords.AllowUpdate = false;

            PXUIFieldAttribute.SetVisible<AMEstimateItem.curyID>(Documents.Cache, null,
                PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

            //always enable the link command buttons...
            ViewOperation.SetEnabled(true);
        }

        /// <summary>
        /// Create an instance of the <see cref="EstimateMaint"/> graph and load it with the given estimate revision
        /// </summary>
        public static EstimateMaint Construct(string estimateId, string revisionId)
        {
            var graph = PXGraph.CreateInstance<EstimateMaint>();
            graph.Documents.Current =
                PXSelect<AMEstimateItem, Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                        And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>>>>
                    .Select(graph, estimateId, revisionId); 
            return graph;
        }

        /// <summary>
        /// Redirect to this graph for the given estimate/revision
        /// </summary>
        public static void Redirect(string estimateId, string revisionId)
        {
            var estimateGraph = CreateInstance<EstimateMaint>();
            AMEstimateItem estimateItem = PXSelect<
                AMEstimateItem,
                Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                    And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>>
            >>
                .Select(estimateGraph, estimateId, revisionId);
            if (estimateItem?.RevisionID == null)
            {
                PXTrace.WriteInformation($"Estimate '{estimateId}' revision '{revisionId}' not found");
                return;
            }

            // Work around for Case #081558
            var url = PXUrl.ToAbsoluteUrl("~/Pages/AM/AM303000.aspx")
                .AppendUrlParam(typeof(AMEstimateItem.estimateID).Name.ToCapitalized(), estimateId)
                .AppendUrlParam(typeof(AMEstimateItem.revisionID).Name.ToCapitalized(), revisionId);

            throw new PXRedirectRequiredException(url, estimateGraph, true, string.Empty);
        }

        public override bool CanClipboardCopyPaste()
        {
            //Use copy action - framework copy disabled
            return false;
        }

        public override bool CanCreateNewRevision(EstimateMaint fromGraph, EstimateMaint toGraph, string keyValue,
            string revisionValue, out string error)
        {
#if DEBUG
            AMDebug.TraceWriteMethodName($"key '{keyValue}' rev '{revisionValue}'");
#endif
            error = string.Empty;
            return true;
        }

        public override void CopyRevision(EstimateMaint fromGraph, EstimateMaint toGraph, string keyValue, string revisionValue)
        {
            if (toGraph?.Documents?.Current == null || fromGraph?.Documents?.Current == null)
            {
                return;
            }

            var estSetup = fromGraph.EstimateSetup.Current ?? fromGraph.EstimateSetup.Select();
            var newRevIsPrimary = estSetup?.NewRevisionIsPrimary == true || fromGraph.Documents.Current.QuoteSource == EstimateSource.SalesOrder ||
                                  fromGraph.Documents.Current.IsLockedByQuote.GetValueOrDefault() && fromGraph.Documents.Current.IsPrimary.GetValueOrDefault();

            toGraph.Documents.Cache.SetDefaultExt<AMEstimateItem.revisionDate>(toGraph.Documents.Current);
            toGraph.Documents.Current.PrimaryRevisionID = newRevIsPrimary ? revisionValue : toGraph.Documents.Current.PrimaryRevisionID;
            // Setting the fromGraph.PrimaryRevisionID also as CopyAsNewEstimateItem copies the SourceEstimate 
            fromGraph.Documents.Current.PrimaryRevisionID = toGraph.Documents.Current.PrimaryRevisionID;
            toGraph.Documents.Current = AMEstimateItem.ResetChildCalcValues(toGraph.Documents.Current);
            toGraph.Documents.Current.NoteID = null;

            foreach (AMEstimateReference insertedEstimateReference in EstimateReferenceRecord.Cache.Inserted)
            {
                EstimateReferenceRecord.Cache.Remove(insertedEstimateReference);
            }

            if (SkipAutoCreateNewRevision())
            {
                toGraph.Documents.Current.EstimateID = keyValue;
                toGraph.Documents.Current.RevisionID = revisionValue;
                EstimateCopy.CreateNewEstimateReferenceRevisionOnly(this, fromGraph.Documents.Current, toGraph.Documents.Current);
                return;
            }

            if (EstimateSetup.Current.CopyEstimateNotes.GetValueOrDefault() || EstimateSetup.Current.CopyEstimateFiles.GetValueOrDefault())
            {
                PXNoteAttribute.CopyNoteAndFiles(fromGraph.Documents.Cache, fromGraph.Documents.Current, toGraph.Documents.Cache, toGraph.Documents.Current,
                    EstimateSetup.Current.CopyEstimateNotes.GetValueOrDefault(), EstimateSetup.Current.CopyEstimateFiles.GetValueOrDefault());
            }

            CreateRevisionCopy(toGraph, fromGraph.Documents.Current, revisionValue, newRevIsPrimary);
        }

        protected virtual void CreateRevisionCopy(EstimateMaint toGraph, AMEstimateItem sourceEstimate,
            string newRevisionID, bool newRevIsPrimary)
        {
            var copyGraph = CreateInstance<EstimateCopy>();
            copyGraph.CreateNewRevision(sourceEstimate, newRevisionID);

            try
            {
                FieldUpdated.RemoveHandler<AMEstimateOper.workCenterID>(AMEstimateOper_WorkCenterID_FieldUpdated);

                TransferCache<AMEstimateReference>(copyGraph, toGraph);
                TransferOperCache(copyGraph, toGraph);
                TransferCache<AMEstimateMatl>(copyGraph, toGraph);
                TransferCache<AMEstimateTool>(copyGraph, toGraph);
                TransferCache<AMEstimateOvhd>(copyGraph, toGraph);

                // These are the aggregate fields which do not calc correctly so we manually copy/set here.
                toGraph.Documents.Current.FixedLaborCalcCost = copyGraph.Documents.Current.FixedLaborCalcCost.GetValueOrDefault();
                toGraph.Documents.Current.FixedLaborCost = copyGraph.Documents.Current.FixedLaborCost.GetValueOrDefault();
                toGraph.Documents.Current.VariableLaborCalcCost = copyGraph.Documents.Current.VariableLaborCalcCost.GetValueOrDefault();
                toGraph.Documents.Current.VariableLaborCost = copyGraph.Documents.Current.VariableLaborCost.GetValueOrDefault();
                toGraph.Documents.Current.MachineCalcCost = copyGraph.Documents.Current.MachineCalcCost.GetValueOrDefault();
                toGraph.Documents.Current.MachineCost = copyGraph.Documents.Current.MachineCost.GetValueOrDefault();
                toGraph.Documents.Current.MaterialCalcCost = copyGraph.Documents.Current.MaterialCalcCost.GetValueOrDefault();
                toGraph.Documents.Current.MaterialCost = copyGraph.Documents.Current.MaterialCost.GetValueOrDefault();
                toGraph.Documents.Current.ToolCalcCost = copyGraph.Documents.Current.ToolCalcCost.GetValueOrDefault();
                toGraph.Documents.Current.ToolCost = copyGraph.Documents.Current.ToolCost.GetValueOrDefault();
                toGraph.Documents.Current.FixedOverheadCalcCost = copyGraph.Documents.Current.FixedOverheadCalcCost.GetValueOrDefault();
                toGraph.Documents.Current.FixedOverheadCost = copyGraph.Documents.Current.FixedOverheadCost.GetValueOrDefault();
                toGraph.Documents.Current.VariableOverheadCalcCost = copyGraph.Documents.Current.VariableOverheadCalcCost.GetValueOrDefault();
                toGraph.Documents.Current.VariableOverheadCost = copyGraph.Documents.Current.VariableOverheadCost.GetValueOrDefault();
                toGraph.Documents.Current.ExtCost = copyGraph.Documents.Current.ExtCost.GetValueOrDefault();
                toGraph.Documents.Current.CuryExtCost = copyGraph.Documents.Current.CuryExtCost.GetValueOrDefault();
                toGraph.Documents.Current.UnitCost = copyGraph.Documents.Current.UnitCost.GetValueOrDefault();
                toGraph.Documents.Current.CuryUnitCost = copyGraph.Documents.Current.CuryUnitCost.GetValueOrDefault();
                toGraph.Documents.Current.UnitPrice = copyGraph.Documents.Current.UnitPrice.GetValueOrDefault();
                toGraph.Documents.Current.CuryUnitPrice = copyGraph.Documents.Current.CuryUnitPrice.GetValueOrDefault();

            }
            finally
            {
                FieldUpdated.AddHandler<AMEstimateOper.workCenterID>(AMEstimateOper_WorkCenterID_FieldUpdated);
            }

        }

        protected static void TransferOperCache(PXGraph fromGraph, PXGraph toGraph)
        {
            foreach (AMEstimateOper rowInserted in fromGraph.Caches[typeof(AMEstimateOper)].Inserted)
            {
                if (rowInserted == null)
                {
                    continue;
                }

                var rowInsertedCalcValuesCleared = AMEstimateOper.ClearCalcValues(PXCache<AMEstimateOper>.CreateCopy(rowInserted));
                if (rowInsertedCalcValuesCleared == null)
                {
                    continue;
                }

                toGraph.Caches[typeof(AMEstimateOper)].Insert(rowInsertedCalcValuesCleared);
            }

            foreach (AMEstimateOper rowUpdated in fromGraph.Caches[typeof(AMEstimateOper)].Updated)
            {
                toGraph.Caches[typeof(AMEstimateOper)].Update(rowUpdated);
            }

            foreach (AMEstimateOper rowDeleted in fromGraph.Caches[typeof(AMEstimateOper)].Deleted)
            {
                toGraph.Caches[typeof(AMEstimateOper)].Delete(rowDeleted);
            }
        }

        protected static void TransferCache<TCacheItem>(PXGraph fromGraph, PXGraph toGraph,
            bool clearSourceGraph = false) where TCacheItem : IBqlTable
        {
            foreach (TCacheItem rowInserted in fromGraph.Caches[typeof(TCacheItem)].Inserted)
            {
                toGraph.Caches[typeof(TCacheItem)].Insert(rowInserted);
            }

            foreach (TCacheItem rowUpdated in fromGraph.Caches[typeof(TCacheItem)].Updated)
            {
                toGraph.Caches[typeof(TCacheItem)].Update(rowUpdated);
            }

            foreach (TCacheItem rowDeleted in fromGraph.Caches[typeof(TCacheItem)].Deleted)
            {
                toGraph.Caches[typeof(TCacheItem)].Delete(rowDeleted);
            }

            if (clearSourceGraph)
            {
                fromGraph.Caches[typeof(TCacheItem)].Clear();
            }
        }

        /// <summary>
        /// Returns the related opportunity (if linked to an opportunity)
        /// </summary>
        public virtual CROpportunity SelectedOpportunity => RelatedOpportunity.Current ?? (RelatedOpportunity.Current = RelatedOpportunity.Select());

        protected virtual void InsertHistory()
        {
            if (Documents?.Current?.RevisionID == null)
            {
                return;
            }

            var currentDocCacheStatus = Documents.Cache.GetStatus(Documents.Current);
            if (currentDocCacheStatus == PXEntryStatus.Inserted)
            {
                var baseMsg = Documents.Current.IsPrimary.GetValueOrDefault()
                    ? Messages.EstimateCreatedPrimary
                    : Messages.EstimateCreated;

                EstimateHistoryRecords.Insert(new AMEstimateHistory()
                {
                    RevisionID = Documents.Current.RevisionID,
                    Description = Messages.GetLocal(baseMsg, Documents.Current.RevisionID.TrimIfNotNullEmpty())
                });

                return;
            }

            var unchangedDocument = (AMEstimateItem)Documents.Cache.GetOriginal(Documents.Current);
            if (unchangedDocument == null)
            {
                return;
            }

            if (unchangedDocument.EstimateStatus != null &&
                Documents.Current.EstimateStatus != null &&
                unchangedDocument.EstimateStatus != Documents.Current.EstimateStatus)
            {
                EstimateHistoryRecords.Insert(new AMEstimateHistory
                {
                    EstimateID = Documents.Current.EstimateID,
                    RevisionID = Documents.Current.RevisionID,
                    Description = Messages.GetLocal(Messages.EstimateStatusChangedFromTo,
                        EstimateStatus.GetDescription(unchangedDocument.EstimateStatus),
                        EstimateStatus.GetDescription(Documents.Current.EstimateStatus))
                });
            }

            if (!string.IsNullOrWhiteSpace(unchangedDocument.PrimaryRevisionID) &&
                !string.IsNullOrWhiteSpace(Documents.Current.PrimaryRevisionID) &&
                !unchangedDocument.PrimaryRevisionID.EqualsWithTrim(Documents.Current.PrimaryRevisionID))
            {
                EstimateHistoryRecords.Insert(new AMEstimateHistory
                {
                    EstimateID = Documents.Current.EstimateID,
                    RevisionID = Documents.Current.RevisionID,
                    Description = Messages.GetLocal(Messages.EstimatePrimaryRevChangedFromTo,
                        unchangedDocument.PrimaryRevisionID.TrimIfNotNullEmpty(),
                        Documents.Current.PrimaryRevisionID.TrimIfNotNullEmpty())
                });
            }
        }

        public static void InsertEstimateHistory(string estimateId, string revisionId, string desc)
        {
            if (string.IsNullOrWhiteSpace(estimateId) || string.IsNullOrWhiteSpace(revisionId) || string.IsNullOrWhiteSpace(desc))
            {
                return;
            }

            var estGraph = Construct(estimateId, revisionId);
            if(estGraph?.Documents?.Current?.EstimateID == null)
            {
                return;
            }

            estGraph.EstimateHistoryRecords.Insert(new AMEstimateHistory
            {
                Description = desc
            });

            estGraph.Actions.PressSave();
        }

        public virtual void PersistBase()
        {
            base.Persist();
        }

        public override void Persist()
        {
            InsertHistory();

            var estGraphHelper = new EstimateGraphHelper(this);
            if (estGraphHelper.PersistEstimateReference())
            {
                return;
            }

            try
            {
                PersistBase();
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw;
            }
        }

        //We get field name cannot be empty but no indication to which DAC, so we add this for improved error reporting
        public override int Persist(Type cacheType, PXDBOperation operation)
        {
            try
            {
                return base.Persist(cacheType, operation);
            }
            catch (Exception e)
            {
                PXTrace.WriteError($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#if DEBUG
                AMDebug.TraceWriteMethodName($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#endif
                throw;
            }
        }

        protected virtual void CopyFromEstimate(string estimateId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(estimateId))
            {
                throw new ArgumentNullException(nameof(estimateId));
            }

            if (string.IsNullOrWhiteSpace(revisionId))
            {
                throw new ArgumentNullException(nameof(revisionId));
            }

            var estimateItem = EstimateRecordSelected.Current;

            AMEstimateItem fromEstimateItem = PXSelect<
                AMEstimateItem,
                Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                    And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>>>>
                .Select(this, estimateId,
                revisionId);

            CopyOperations(fromEstimateItem, estimateItem);
            CopyMaterials(fromEstimateItem, estimateItem);
            CopyTools(fromEstimateItem, estimateItem);
            CopyOverheads(fromEstimateItem, estimateItem);
            CopySteps(fromEstimateItem, estimateItem);

            estimateItem.OrderQty = fromEstimateItem.OrderQty;
            estimateItem.BaseOrderQty = fromEstimateItem.BaseOrderQty;
            estimateItem.LaborMarkupPct = fromEstimateItem.LaborMarkupPct;
            estimateItem.MachineMarkupPct = fromEstimateItem.MachineMarkupPct;
            estimateItem.MaterialMarkupPct = fromEstimateItem.MaterialMarkupPct;
            estimateItem.SubcontractMarkupPct = fromEstimateItem.SubcontractMarkupPct;
            estimateItem.ToolMarkupPct = fromEstimateItem.ToolMarkupPct;
            estimateItem.OverheadMarkupPct = fromEstimateItem.OverheadMarkupPct;
            estimateItem.UOM = fromEstimateItem.UOM;
            estimateItem.MachineCost = fromEstimateItem.MachineCost;

            // Copy the Override Check Boxes
            estimateItem.FixedLaborOverride = fromEstimateItem.FixedLaborOverride;
            estimateItem.VariableLaborOverride = fromEstimateItem.VariableLaborOverride;
            estimateItem.MachineOverride = fromEstimateItem.MachineOverride;
            estimateItem.MaterialOverride = fromEstimateItem.MaterialOverride;
            estimateItem.ToolOverride = fromEstimateItem.ToolOverride;
            estimateItem.FixedOverheadOverride = fromEstimateItem.FixedOverheadOverride;
            estimateItem.VariableOverheadOverride = fromEstimateItem.VariableOverheadOverride;
            estimateItem.SubcontractOverride = fromEstimateItem.SubcontractOverride;

            if (fromEstimateItem.FixedLaborOverride == true)
            {
                estimateItem.FixedLaborCost = fromEstimateItem.FixedLaborCost;
            }

            if (fromEstimateItem.VariableLaborOverride == true)
            {
                estimateItem.VariableLaborCost = fromEstimateItem.VariableLaborCost;
            }

            if (fromEstimateItem.MachineOverride == true)
            {
                estimateItem.MachineCost = fromEstimateItem.MachineCost;
            }

            if (fromEstimateItem.MaterialOverride == true)
            {
                estimateItem.MaterialCost = fromEstimateItem.MaterialCost;
            }

            if (fromEstimateItem.ToolOverride == true)
            {
                estimateItem.ToolCost = fromEstimateItem.ToolCost;
            }

            if (fromEstimateItem.FixedOverheadOverride == true)
            {
                estimateItem.FixedOverheadCost = fromEstimateItem.FixedOverheadCost;
            }

            if (fromEstimateItem.VariableOverheadOverride == true)
            {
                estimateItem.VariableOverheadCost = fromEstimateItem.VariableOverheadCost;
            }

            if (fromEstimateItem.SubcontractOverride == true)
            {
                estimateItem.SubcontractCost = fromEstimateItem.SubcontractCost;
            }

            if (estimateItem.CuryID != fromEstimateItem.CuryID)
            {
                estimateItem.CuryID = fromEstimateItem.CuryID;
                estimateItem.CuryInfoID = null;
            }

            if (CopyEstimateFromFilter.Current.OverrideInventoryID == true)
            {
                estimateItem.InventoryID = fromEstimateItem.InventoryID;
                estimateItem.InventoryCD = fromEstimateItem.InventoryCD;
                estimateItem.ItemDesc = fromEstimateItem.ItemDesc;
                estimateItem.ItemClassID = fromEstimateItem.ItemClassID;
            }

            EstimateRecordSelected.Cache.Update(estimateItem);

            // Create Estimate History Event
            EstimateHistoryRecords.Insert(new AMEstimateHistory
            {
                RevisionID = Documents.Current.RevisionID,
                Description = Messages.GetLocal(Messages.EstimateDetailsUpdatedFromEstimate, estimateId, revisionId)
            });
        }

        protected virtual void CopyFromProductionOrder(string orderType, string prodOrdID)
        {
            AMProdItem prodItem = PXSelect<
                AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>
                    >>.Select(this, orderType, prodOrdID);
            CopyOperations(prodItem);
            CopyMaterials(prodItem);
            CopyTools(prodItem);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
            CopyOverheads(prodItem);
            CopySteps(prodItem);

            if (CopyEstimateFromFilter.Current.OverrideInventoryID == true)
            {
                AMEstimateItem estimateItem = EstimateRecordSelected.Current;
                estimateItem.InventoryID = prodItem.InventoryID;

                InventoryItem invItem = PXSelect<
                    InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>
                >>
                    .Select(this, prodItem.InventoryID);

                estimateItem.InventoryCD = invItem.InventoryCD;
                estimateItem.ItemDesc = invItem.Descr;
                estimateItem.UOM = invItem.BaseUnit;
                estimateItem.ItemClassID = invItem.ItemClassID;

                EstimateRecordSelected.Cache.Update(estimateItem);
            }

            // Create Estimate History Event
            EstimateHistoryRecords.Insert(new AMEstimateHistory
            {
                RevisionID = Documents.Current.RevisionID,
                Description = Messages.GetLocal(Messages.EstimateDetailsUpdatedFromProductionOrder, prodOrdID, orderType)
            });
        }

        protected virtual void CopyFromBOM(string bomId, string bomRevisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                throw new ArgumentNullException(nameof(bomId));
            }

            if (string.IsNullOrWhiteSpace(bomRevisionId))
            {
                throw new ArgumentNullException(nameof(bomRevisionId));
            }

            if (CopyEstimateFromFilter?.Current == null)
            {
                throw new PXArgumentException(nameof(CopyEstimateFromFilter));
            }

            AMBomItem bomItem = PXSelect<
                AMBomItem,
                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                    And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                .Select(this, bomId, bomRevisionId);

            if (bomItem == null)
            {
                throw new PXException(Messages.UnableToFindBomRev, bomId, bomRevisionId);
            }

            CopyOperations(bomItem);
            CopyMaterials(bomItem);
            CopyTools(bomItem);
            CopyOverheads(bomItem);
            CopySteps(bomItem);

            if (CopyEstimateFromFilter.Current.OverrideInventoryID == true)
            {
                var estimateItem = EstimateRecordSelected.Current;
                estimateItem.InventoryID = bomItem.InventoryID;

                InventoryItem invItem = PXSelect<
                    InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(this,
                    bomItem.InventoryID);

                estimateItem.InventoryCD = invItem.InventoryCD;
                estimateItem.ItemDesc = invItem.Descr;
                estimateItem.UOM = invItem.BaseUnit;
                estimateItem.ItemClassID = invItem.ItemClassID;

                EstimateRecordSelected.Cache.Update(estimateItem);
            }

            EstimateHistoryRecords.Insert(new AMEstimateHistory
            {
                RevisionID = Documents.Current.RevisionID,
                Description = Messages.GetLocal(Messages.EstimateDetailsUpdatedFromBOM, bomId, bomRevisionId)
            });
        }

        /// <summary>
        /// Copy the operations from the from estimate as operations of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        /// <returns>List of new/to estimate operations</returns>
        protected virtual void CopyOperations(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            foreach (AMEstimateOper fromEstimateOper in PXSelect<
                AMEstimateOper,
                Where<AMEstimateOper.estimateID, Equal<Required<AMEstimateOper.estimateID>>,
                    And<AMEstimateOper.revisionID, Equal<Required<AMEstimateOper.revisionID>>>>
            >
                .Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var newEstOper = EstimateOperRecords.Insert(new AMEstimateOper
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationCD = fromEstimateOper.OperationCD
                });

                newEstOper.BaseOrderQty = fromEstimateOper.BaseOrderQty;
                newEstOper.Description = fromEstimateOper.Description;
                newEstOper.WorkCenterID = fromEstimateOper.WorkCenterID;
                newEstOper.WorkCenterStdCost = fromEstimateOper.WorkCenterStdCost;
                newEstOper.SetupTime = fromEstimateOper.SetupTime;
                newEstOper.RunUnits = fromEstimateOper.RunUnits;
                newEstOper.RunUnitTime = fromEstimateOper.RunUnitTime;
                newEstOper.MachineUnits = fromEstimateOper.MachineUnits;
                newEstOper.MachineUnitTime = fromEstimateOper.MachineUnitTime;
                newEstOper.QueueTime = fromEstimateOper.QueueTime;
                newEstOper.BackFlushLabor = fromEstimateOper.BackFlushLabor;
                newEstOper.OutsideProcess = fromEstimateOper.OutsideProcess;
                newEstOper.DropShippedToVendor = fromEstimateOper.DropShippedToVendor;
                newEstOper.VendorID = fromEstimateOper.VendorID;
                newEstOper.VendorLocationID = fromEstimateOper.VendorLocationID;
                newEstOper.MachineStdCost = fromEstimateOper.MachineStdCost;
                newEstOper.MachineCost = fromEstimateOper.MachineCost;

                // Copy the Override Check Boxes
                newEstOper.FixedLaborOverride = fromEstimateOper.FixedLaborOverride;
                newEstOper.VariableLaborOverride = fromEstimateOper.VariableLaborOverride;
                newEstOper.MachineOverride = fromEstimateOper.MachineOverride;
                newEstOper.MaterialOverride = fromEstimateOper.MaterialOverride;
                newEstOper.ToolOverride = fromEstimateOper.ToolOverride;
                newEstOper.FixedOverheadOverride = fromEstimateOper.FixedOverheadOverride;
                newEstOper.VariableOverheadOverride = fromEstimateOper.VariableOverheadOverride;
                newEstOper.SubcontractOverride = fromEstimateOper.SubcontractOverride;

                if (fromEstimateOper.FixedLaborOverride == true)
                {
                    newEstOper.FixedLaborCost = fromEstimateOper.FixedLaborCost;
                }

                if (fromEstimateOper.VariableLaborOverride == true)
                {
                    newEstOper.VariableLaborCost = fromEstimateOper.VariableLaborCost;
                }

                if (fromEstimateOper.MachineOverride == true)
                {
                    newEstOper.MachineCost = fromEstimateOper.MachineCost;
                }

                if (fromEstimateOper.MaterialOverride == true)
                {
                    newEstOper.MaterialCost = fromEstimateOper.MaterialCost;
                }

                if (fromEstimateOper.ToolOverride == true)
                {
                    newEstOper.ToolCost = fromEstimateOper.ToolCost;
                }

                if (fromEstimateOper.FixedOverheadOverride == true)
                {
                    newEstOper.FixedOverheadCost = fromEstimateOper.FixedOverheadCost;
                }

                if (fromEstimateOper.VariableOverheadOverride == true)
                {
                    newEstOper.VariableOverheadCost = fromEstimateOper.VariableOverheadCost;
                }

                if (fromEstimateOper.SubcontractOverride == true)
                {
                    newEstOper.SubcontractCost = fromEstimateOper.SubcontractCost;
                }

                EstimateOperRecords.Update(newEstOper);
            }
        }

        /// <summary>
        /// Copy the operations from the Production Order as operations of the estimate
        /// </summary>
        /// <param name="prodItem">to/destination estimate</param>
        /// <returns>List of new/to estimate operations</returns>
        protected virtual void CopyOperations(AMProdItem prodItem)
        {
            foreach (PXResult<AMProdOper, AMWC> operResult in PXSelectJoin<
                AMProdOper,
                LeftJoin<AMWC,
                    On<AMProdOper.wcID, Equal<AMWC.wcID>>>,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>
                >>
                .Select(this, prodItem.OrderType, prodItem.ProdOrdID))
            {
                var prodOper = (AMProdOper)operResult;
                var prodWC = (AMWC)operResult;

                var newEstOper = EstimateOperRecords.Insert(new AMEstimateOper
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationCD = prodOper.OperationCD
                });

                newEstOper.BaseOrderQty = EstimateRecordSelected.Current.BaseOrderQty;
                newEstOper.Description = prodOper.Descr;
                newEstOper.WorkCenterID = prodOper.WcID;
                newEstOper.WorkCenterStdCost = prodWC.StdCost;
                newEstOper.SetupTime = prodOper.SetupTime;
                newEstOper.RunUnitTime = prodOper.RunUnitTime;
                newEstOper.RunUnits = prodOper.RunUnits;
                newEstOper.MachineUnitTime = prodOper.MachineUnitTime;
                newEstOper.MachineUnits = prodOper.MachineUnits;
                newEstOper.QueueTime = prodOper.QueueTime;
                newEstOper.BackFlushLabor = prodOper.BFlush;
                newEstOper.OutsideProcess = prodOper.OutsideProcess;
                newEstOper.DropShippedToVendor = prodOper.DropShippedToVendor;
                newEstOper.VendorID = prodOper.VendorID;
                newEstOper.VendorLocationID = prodOper.VendorLocationID;
                EstimateOperRecords.Update(newEstOper);
            }
        }

        /// <summary>
        /// Copy the operations from the BOM as operations of the estimate
        /// </summary>
        /// <param name="bomItem">to/destination estimate</param>
        protected virtual void CopyOperations(AMBomItem bomItem)
        {
            foreach (PXResult<AMBomOper, AMWC> operResult in PXSelectJoin<
                AMBomOper,
                LeftJoin<AMWC,
                    On<AMBomOper.wcID, Equal<AMWC.wcID>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>
            >
                .Select(this, bomItem.BOMID, bomItem.RevisionID))
            {
                var bomOper = (AMBomOper)operResult;
                var bomWC = (AMWC)operResult;

                var newEstOper = EstimateOperRecords.Insert(new AMEstimateOper
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationCD = bomOper.OperationCD
                });

                newEstOper.BaseOrderQty = EstimateRecordSelected.Current.BaseOrderQty;
                newEstOper.Description = bomOper.Descr;
                newEstOper.WorkCenterID = bomOper.WcID;
                newEstOper.WorkCenterStdCost = bomWC.StdCost;
                newEstOper.SetupTime = bomOper.SetupTime;
                newEstOper.RunUnitTime = bomOper.RunUnitTime;
                newEstOper.RunUnits = bomOper.RunUnits;
                newEstOper.MachineUnitTime = bomOper.MachineUnitTime;
                newEstOper.MachineUnits = bomOper.MachineUnits;
                newEstOper.QueueTime = bomOper.QueueTime;
                newEstOper.BackFlushLabor = bomOper.BFlush;
                newEstOper.OutsideProcess = bomOper.OutsideProcess;
                newEstOper.DropShippedToVendor = bomOper.DropShippedToVendor;
                newEstOper.VendorID = bomOper.VendorID;
                newEstOper.VendorLocationID = bomOper.VendorLocationID;
                EstimateOperRecords.Update(newEstOper);
            }
        }

        /// <summary>
        /// Copy the Materials from the from estimate as materials of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        protected virtual void CopyMaterials(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            foreach (PXResult<AMEstimateMatl, AMEstimateOper> result in PXSelectJoin<
                AMEstimateMatl, 
                InnerJoin<AMEstimateOper,
                    On<AMEstimateMatl.estimateID, Equal<AMEstimateOper.estimateID>,
                    And<AMEstimateMatl.revisionID, Equal<AMEstimateOper.revisionID>,
                    And<AMEstimateMatl.operationID, Equal<AMEstimateOper.operationID>>>>>,
                Where<AMEstimateMatl.estimateID, Equal<Required<AMEstimateMatl.estimateID>>,
                    And<AMEstimateMatl.revisionID, Equal<Required<AMEstimateMatl.revisionID>>>>
            >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var fromEstimateMatl = (AMEstimateMatl)result;
                var fromEstimateOper = (AMEstimateOper)result;

                var estimateOper = FindCachedEstimateOper(fromEstimateOper, toEstimateItem);
                if (TryCheckEstimateOperForCopy(estimateOper, fromEstimateOper, typeof(AMEstimateMatl), out var ex))
                {
                    throw ex;
                }

                var newEstMatl = new AMEstimateMatl
                {
                    EstimateID = toEstimateItem.EstimateID,
                    RevisionID = toEstimateItem.RevisionID,
                    OperationID = estimateOper.OperationID,
                    InventoryID = fromEstimateMatl.InventoryID,
                    InventoryCD = fromEstimateMatl.InventoryCD,
                    IsNonInventory = fromEstimateMatl.IsNonInventory,
                    ItemDesc = fromEstimateMatl.ItemDesc,
                    SubItemID = fromEstimateMatl.SubItemID,
                    ItemClassID = fromEstimateMatl.ItemClassID,
                    BackFlush = fromEstimateMatl.BackFlush,
                    QtyReq = fromEstimateMatl.QtyReq,
                    BaseQtyReq = fromEstimateMatl.BaseQtyReq,
                    UOM = fromEstimateMatl.UOM,
                    UnitCost = fromEstimateMatl.UnitCost,
                    MaterialType = fromEstimateMatl.MaterialType,
                    PhantomRouting = fromEstimateMatl.PhantomRouting,
                    SiteID = fromEstimateMatl.SiteID,
                    ScrapFactor = fromEstimateMatl.ScrapFactor,
                    LocationID = fromEstimateMatl.LocationID,
                    BatchSize = fromEstimateMatl.BatchSize,
                    QtyRoundUp = fromEstimateMatl.QtyRoundUp,
                    SubcontractSource = fromEstimateMatl.SubcontractSource
                };
                SetCurrentEstimateOperation(newEstMatl);
                EstimateOperMatlRecords.Insert(newEstMatl);
            }
        }

        protected virtual AMEstimateOper FindCachedEstimateOper(IOperationMaster sourceOperation, AMEstimateItem toEstimateItem) =>
            EstimateOperRecords.Cache.Cached.Cast<AMEstimateOper>()
                .Where(estimateOper => !string.IsNullOrWhiteSpace(estimateOper?.OperationCD))
                .FirstOrDefault(estimateOper => estimateOper.OperationCD == sourceOperation?.OperationCD 
                && estimateOper.EstimateID == toEstimateItem.EstimateID && estimateOper.RevisionID == toEstimateItem.RevisionID);

        protected virtual AMEstimateOper FindCachedEstimateOper(IOperationMaster sourceOperation) =>
            EstimateOperRecords.Cache.Cached.Cast<AMEstimateOper>()
                .Where(estimateOper => !string.IsNullOrWhiteSpace(estimateOper?.OperationCD))
                .FirstOrDefault(estimateOper => estimateOper.OperationCD == sourceOperation?.OperationCD);

        protected virtual bool TryCheckEstimateOperForCopy(AMEstimateOper estimateOper, IOperationMaster sourceOperation, Type copyType, out PXException ex)
        {
            ex = null;
            if (estimateOper?.OperationID == null)
            {
                var recMissing = Messages.GetLocal(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMEstimateOper)));
                ex = new PXException($"Error processing copy of {Common.Cache.GetCacheName(copyType)} for operation ID {sourceOperation?.OperationCD}. {recMissing}");
            }

            return ex != null;
        }

        /// <summary>
        /// Copy the Materials from the from the Production Order as materials of the estimate
        /// </summary>
        /// <param name="prodItem">from/source estimate</param>
        protected virtual void CopyMaterials(AMProdItem prodItem)
        {
            foreach (PXResult<AMProdMatl, AMProdOper, InventoryItem> result in PXSelectJoin<
                AMProdMatl,
                InnerJoin<AMProdOper,
                    On<AMProdMatl.orderType, Equal<AMProdOper.orderType>,
                    And<AMProdMatl.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMProdMatl.operationID, Equal<AMProdOper.operationID>>>>,
                LeftJoin<InventoryItem,
                    On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>>,
                OrderBy<
                    Asc<AMProdMatl.sortOrder,
                    Asc<AMProdMatl.lineID>>>
            >
                .Select(this, prodItem.OrderType, prodItem.ProdOrdID))
            {
                var prodMatl = (AMProdMatl) result;
                var prodOper = (AMProdOper) result;
                var inventoryItem = (InventoryItem) result;

                if (string.IsNullOrWhiteSpace(prodMatl?.ProdOrdID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(prodOper);
                if (TryCheckEstimateOperForCopy(estimateOper, prodOper, typeof(AMProdMatl), out var ex))
                {
                    throw ex;
                }

                var newEstMatl = new AMEstimateMatl
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    InventoryID = prodMatl.InventoryID,
                    InventoryCD = inventoryItem?.InventoryCD,
                    IsNonInventory = false,
                    ItemDesc = prodMatl.Descr,
                    SubItemID = prodMatl.SubItemID,
                    ItemClassID = inventoryItem?.ItemClassID,
                    BackFlush = prodMatl.BFlush,
                    QtyReq = prodMatl.QtyReq,
                    BaseQtyReq = prodMatl.BaseQty,
                    UOM = prodMatl.UOM,
                    UnitCost = prodMatl.UnitCost,
                    MaterialType = prodMatl.MaterialType,
                    PhantomRouting = prodMatl.PhantomRouting,
                    SiteID = prodMatl.SiteID,
                    ScrapFactor = prodMatl.ScrapFactor,
                    LocationID = prodMatl.LocationID,
                    BatchSize = prodMatl.BatchSize,
                    QtyRoundUp = prodMatl.QtyRoundUp,
                    SubcontractSource = prodMatl.SubcontractSource
                };
                SetCurrentEstimateOperation(newEstMatl);
                EstimateOperMatlRecords.Insert(newEstMatl);
            }
        }

        /// <summary>
        /// Copy the Materials from the from the BOM as materials of the estimate
        /// </summary>
        /// <param name="bomItem">Source BOM</param>
        protected virtual void CopyMaterials(AMBomItem bomItem)
        {
            foreach (PXResult<AMBomMatl, AMBomOper, InventoryItem> result in PXSelectJoin<
                AMBomMatl,
                InnerJoin<AMBomOper,
                    On<AMBomMatl.bOMID, Equal<AMBomOper.bOMID>,
                    And<AMBomMatl.revisionID, Equal<AMBomOper.revisionID>,
                    And<AMBomMatl.operationID, Equal<AMBomOper.operationID>>>>,
                LeftJoin<InventoryItem,
                    On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>>>,
                OrderBy<
                    Asc<AMBomMatl.sortOrder,
                    Asc<AMBomMatl.lineID>>>
            >
                .Select(this, bomItem.BOMID, bomItem.RevisionID))
            {
                var bomMatl = (AMBomMatl) result;
                var bomOper = (AMBomOper) result;
                var inventoryItem = (InventoryItem) result;

                if (string.IsNullOrWhiteSpace(bomMatl?.BOMID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(bomOper);
                if (TryCheckEstimateOperForCopy(estimateOper, bomOper, typeof(AMBomMatl), out var ex))
                {
                    throw ex;
                }

                var newEstMatl = new AMEstimateMatl
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    InventoryID = bomMatl.InventoryID,
                    InventoryCD = inventoryItem?.InventoryCD,
                    IsNonInventory = false,
                    ItemDesc = bomMatl.Descr,
                    SubItemID = bomMatl.SubItemID,
                    ItemClassID = inventoryItem?.ItemClassID,
                    BackFlush = bomMatl.BFlush,
                    QtyReq = bomMatl.QtyReq,
                    BaseQtyReq = bomMatl.BaseQty,
                    UOM = bomMatl.UOM,
                    UnitCost = bomMatl.UnitCost,
                    MaterialType = bomMatl.MaterialType,
                    PhantomRouting = bomMatl.PhantomRouting,
                    SiteID = bomMatl.SiteID,
                    ScrapFactor = bomMatl.ScrapFactor,
                    LocationID = bomMatl.LocationID,
                    BatchSize = bomMatl.BatchSize,
                    SubcontractSource = bomMatl.SubcontractSource
                };
                SetCurrentEstimateOperation(newEstMatl);
                EstimateOperMatlRecords.Insert(newEstMatl);
            }
        }

        /// <summary>
        /// Copy the tools from the from estimate as tools of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        /// <returns>List of new/to estimate operations</returns>
        protected virtual void CopyTools(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            foreach (PXResult<AMEstimateTool, AMEstimateOper> result in PXSelectJoin<
                AMEstimateTool,
                InnerJoin<AMEstimateOper,
                    On<AMEstimateTool.estimateID, Equal<AMEstimateOper.estimateID>,
                    And<AMEstimateTool.revisionID, Equal<AMEstimateOper.revisionID>,
                    And<AMEstimateTool.operationID, Equal<AMEstimateOper.operationID>>>>>,
                Where
                    <AMEstimateTool.estimateID, Equal<Required<AMEstimateTool.estimateID>>,
                    And<AMEstimateTool.revisionID, Equal<Required<AMEstimateTool.revisionID>>>>
                >
                .Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var fromEstimateTool = (AMEstimateTool)result;
                var fromEstimateOper = (AMEstimateOper)result;

                var estimateOper = FindCachedEstimateOper(fromEstimateOper, toEstimateItem);
                if (TryCheckEstimateOperForCopy(estimateOper, fromEstimateOper, typeof(AMEstimateTool), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateTool
                {
                    EstimateID = toEstimateItem.EstimateID,
                    RevisionID = toEstimateItem.RevisionID,
                    OperationID = estimateOper.OperationID,
                    Description = fromEstimateTool.Description,
                    QtyReq = fromEstimateTool.QtyReq,
                    ToolID = fromEstimateTool.ToolID,
                    UnitCost = fromEstimateTool.UnitCost
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperToolRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the Tools from the from the Production Order as tools of the estimate
        /// </summary>
        /// <param name="prodItem">Production Order to copy</param>
        protected virtual void CopyTools(AMProdItem prodItem)
        {
            foreach (PXResult<AMProdTool, AMProdOper> result in PXSelectJoin<
                AMProdTool,
                InnerJoin<AMProdOper,
                    On<AMProdTool.orderType, Equal<AMProdOper.orderType>,
                    And<AMProdTool.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMProdTool.operationID, Equal<AMProdOper.operationID>>>>>,
                Where<AMProdTool.orderType, Equal<Required<AMProdTool.orderType>>,
                    And<AMProdTool.prodOrdID, Equal<Required<AMProdTool.prodOrdID>>>
                >>
                .Select(this, prodItem.OrderType, prodItem.ProdOrdID))
            {
                var prodTool = (AMProdTool) result;
                var prodOper = (AMProdOper) result;

                if (string.IsNullOrWhiteSpace(prodTool?.ProdOrdID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(prodOper);
                if (TryCheckEstimateOperForCopy(estimateOper, prodOper, typeof(AMProdTool), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateTool
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    Description = prodTool.Descr,
                    QtyReq = prodTool.QtyReq,
                    ToolID = prodTool.ToolID,
                    UnitCost = prodTool.UnitCost
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperToolRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the Tools from the from the BOM as tools of the estimate
        /// </summary>
        /// <param name="bomItem">BOM to copy</param>
        protected virtual void CopyTools(AMBomItem bomItem)
        {
            foreach (PXResult<AMBomTool, AMBomOper> result in PXSelectJoin<
                AMBomTool,
                InnerJoin<AMBomOper,
                    On<AMBomTool.bOMID, Equal<AMBomOper.bOMID>,
                    And<AMBomTool.revisionID, Equal<AMBomOper.revisionID>,
                    And<AMBomTool.operationID, Equal<AMBomOper.operationID>>>>>,
                Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
                    And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>>>>
                .Select(this, bomItem.BOMID, bomItem.RevisionID))
            {
                var bomTool = (AMBomTool) result;
                var bomOper = (AMBomOper) result;

                if (string.IsNullOrWhiteSpace(bomTool?.BOMID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(bomOper);
                if (TryCheckEstimateOperForCopy(estimateOper, bomOper, typeof(AMBomTool), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateTool
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    Description = bomTool.Descr,
                    QtyReq = bomTool.QtyReq,
                    ToolID = bomTool.ToolID,
                    UnitCost = bomTool.UnitCost
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperToolRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the Overheads from the from estimate as overheads of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        protected virtual void CopyOverheads(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            foreach (PXResult<AMEstimateOvhd, AMEstimateOper> result in PXSelectJoin<
                AMEstimateOvhd,
                InnerJoin<AMEstimateOper,
                    On<AMEstimateOvhd.estimateID, Equal<AMEstimateOper.estimateID>,
                    And<AMEstimateOvhd.revisionID, Equal<AMEstimateOper.revisionID>,
                    And<AMEstimateOvhd.operationID, Equal<AMEstimateOper.operationID>>>>>,
                Where<AMEstimateOvhd.estimateID, Equal<Required<AMEstimateOvhd.estimateID>>,
                    And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOvhd.revisionID>>>>
                    >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var fromEstimateOvhd = (AMEstimateOvhd)result;
                var fromEstimateOper = (AMEstimateOper)result;

                var estimateOper = FindCachedEstimateOper(fromEstimateOper, toEstimateItem);
                if (TryCheckEstimateOperForCopy(estimateOper, fromEstimateOper, typeof(AMEstimateOvhd), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateOvhd
                {
                    EstimateID = toEstimateItem.EstimateID,
                    RevisionID = toEstimateItem.RevisionID,
                    OperationID = estimateOper.OperationID,
                    OvhdID = fromEstimateOvhd.OvhdID,
                    OFactor = fromEstimateOvhd.OFactor,
                    OvhdType = fromEstimateOvhd.OvhdType,
                    Description = fromEstimateOvhd.Description,
                    OverheadCostRate = fromEstimateOvhd.OverheadCostRate,
                    WCFlag = fromEstimateOvhd.WCFlag
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperOvhdRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the Overheads from the from the Production Order as Overheads of the estimate
        /// </summary>
        /// <param name="prodItem">Production Order to copy</param>
        protected virtual void CopyOverheads(AMProdItem prodItem)
        {
            foreach (PXResult<AMProdOvhd, AMProdOper, AMOverhead> result in PXSelectJoin<AMProdOvhd,
                InnerJoin<AMProdOper,
                    On<AMProdOvhd.orderType, Equal<AMProdOper.orderType>,
                        And<AMProdOvhd.prodOrdID, Equal<AMProdOper.prodOrdID>,
                            And<AMProdOvhd.operationID, Equal<AMProdOper.operationID>>>>,
                    InnerJoin<AMOverhead,
                        On<AMProdOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>>,
                Where<AMProdOvhd.orderType, Equal<Required<AMProdOvhd.orderType>>,
                    And<AMProdOvhd.prodOrdID, Equal<Required<AMProdOvhd.prodOrdID>>>
                >>.Select(this, prodItem.OrderType, prodItem.ProdOrdID))
            {
                var prodOvhd = (AMProdOvhd) result;
                var prodOper = (AMProdOper) result;
                var overhead = (AMOverhead)result;

                if (string.IsNullOrWhiteSpace(prodOvhd?.ProdOrdID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(prodOper);
                if (TryCheckEstimateOperForCopy(estimateOper, prodOper, typeof(AMProdOvhd), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateOvhd
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    OvhdID = prodOvhd.OvhdID,
                    OFactor = prodOvhd.OFactor,
                    OvhdType = overhead.OvhdType,
                    Description = overhead.Descr,
                    OverheadCostRate = overhead.CostRate
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperOvhdRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the Overheads from the from the BOM as Overheads of the estimate
        /// </summary>
        /// <param name="bomItem">BOM to copy</param>
        protected virtual void CopyOverheads(AMBomItem bomItem)
        {
            foreach (PXResult<AMBomOvhd, AMBomOper, AMOverhead> result in PXSelectJoin<
                AMBomOvhd,
                InnerJoin<AMBomOper,
                    On<AMBomOvhd.bOMID, Equal<AMBomOper.bOMID>,
                    And<AMBomOvhd.revisionID, Equal<AMBomOper.revisionID>,
                    And<AMBomOvhd.operationID, Equal<AMBomOper.operationID>>>>,
                InnerJoin<AMOverhead,
                    On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>>,
                Where<AMBomOvhd.bOMID, Equal<Required<AMBomOvhd.bOMID>>,
                    And<AMBomOvhd.revisionID, Equal<Required<AMBomOvhd.revisionID>>>>>
                .Select(this, bomItem.BOMID, bomItem.RevisionID))
            {
                var bomOvhd = (AMBomOvhd) result;
                var bomOper = (AMBomOper) result;
                var overhead = (AMOverhead) result;

                if (string.IsNullOrWhiteSpace(bomOvhd?.BOMID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(bomOper);
                if (TryCheckEstimateOperForCopy(estimateOper, bomOper, typeof(AMBomOvhd), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateOvhd
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    OvhdID = bomOvhd.OvhdID,
                    OFactor = bomOvhd.OFactor,
                    OvhdType = overhead.OvhdType,
                    Description = overhead.Descr,
                    OverheadCostRate = overhead.CostRate
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperOvhdRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the Steps from the from the BOM as steps of the estimate
        /// </summary>
        /// <param name="bomItem">BOM to copy</param>
        protected virtual void CopySteps(AMBomItem bomItem)
        {
            foreach (PXResult<AMBomStep, AMBomOper> result in PXSelectJoin<
                AMBomStep,
                InnerJoin<AMBomOper,
                    On<AMBomStep.bOMID, Equal<AMBomOper.bOMID>,
                    And<AMBomStep.revisionID, Equal<AMBomOper.revisionID>,
                    And<AMBomStep.operationID, Equal<AMBomOper.operationID>>>>>,
                Where<AMBomStep.bOMID, Equal<Required<AMBomStep.bOMID>>,
                    And<AMBomStep.revisionID, Equal<Required<AMBomStep.revisionID>>>>>
                .Select(this, bomItem.BOMID, bomItem.RevisionID))
            {
                var bomStep = (AMBomStep)result;
                var bomOper = (AMBomOper)result;

                if (string.IsNullOrWhiteSpace(bomStep?.BOMID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(bomOper);
                if (TryCheckEstimateOperForCopy(estimateOper, bomOper, typeof(AMBomStep), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateStep
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    Description = bomStep.Descr
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperStepRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the steps from the from estimate as steps of the to estimate
        /// </summary>
        /// <param name="fromEstimateItem">from/source estimate</param>
        /// <param name="toEstimateItem">to/destination estimate</param>
        /// <returns>List of new/to estimate operations</returns>
        protected virtual void CopySteps(AMEstimateItem fromEstimateItem, AMEstimateItem toEstimateItem)
        {
            foreach (PXResult<AMEstimateStep, AMEstimateOper> result in PXSelectJoin<
                AMEstimateStep,
                InnerJoin<AMEstimateOper,
                    On<AMEstimateStep.estimateID, Equal<AMEstimateOper.estimateID>,
                    And<AMEstimateStep.revisionID, Equal<AMEstimateOper.revisionID>,
                    And<AMEstimateStep.operationID, Equal<AMEstimateOper.operationID>>>>>,
                Where<AMEstimateStep.estimateID, Equal<Required<AMEstimateStep.estimateID>>,
                    And<AMEstimateStep.revisionID, Equal<Required<AMEstimateStep.revisionID>>>>
                    >.Select(this, fromEstimateItem.EstimateID, fromEstimateItem.RevisionID))
            {
                var fromEstimateStep = (AMEstimateStep)result;
                var fromEstimateOper = (AMEstimateOper)result;

                var estimateOper = FindCachedEstimateOper(fromEstimateOper, toEstimateItem);
                if (TryCheckEstimateOperForCopy(estimateOper, fromEstimateOper, typeof(AMEstimateStep), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateStep
                {
                    EstimateID = toEstimateItem.EstimateID,
                    RevisionID = toEstimateItem.RevisionID,
                    OperationID = estimateOper.OperationID,
                    Description = fromEstimateStep.Description
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperStepRecords.Insert(newEstRow);
            }
        }

        /// <summary>
        /// Copy the Steps from the from the Production Order as steps of the estimate
        /// </summary>
        /// <param name="prodItem">Production Order to copy</param>
        protected virtual void CopySteps(AMProdItem prodItem)
        {
            foreach (PXResult<AMProdStep, AMProdOper> result in PXSelectJoin<
                AMProdStep,
                InnerJoin<AMProdOper,
                    On<AMProdStep.orderType, Equal<AMProdOper.orderType>,
                    And<AMProdStep.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMProdStep.operationID, Equal<AMProdOper.operationID>>>>>,
                Where<AMProdStep.orderType, Equal<Required<AMProdStep.orderType>>,
                    And<AMProdStep.prodOrdID, Equal<Required<AMProdStep.prodOrdID>>>
                >>
                .Select(this, prodItem.OrderType, prodItem.ProdOrdID))
            {
                var prodStep = (AMProdStep)result;
                var prodOper = (AMProdOper)result;

                if (string.IsNullOrWhiteSpace(prodStep?.ProdOrdID))
                {
                    continue;
                }

                var estimateOper = FindCachedEstimateOper(prodOper);
                if (TryCheckEstimateOperForCopy(estimateOper, prodOper, typeof(AMProdStep), out var ex))
                {
                    throw ex;
                }

                var newEstRow = new AMEstimateStep
                {
                    EstimateID = EstimateRecordSelected.Current.EstimateID,
                    RevisionID = EstimateRecordSelected.Current.RevisionID,
                    OperationID = estimateOper.OperationID,
                    Description = prodStep.Descr
                };
                SetCurrentEstimateOperation(newEstRow);
                EstimateOperStepRecords.Insert(newEstRow);
            }
        }

        private void DeleteOperations()
        {
            // No need to delete the child records of Estimate Operations as the PXParent will handle the cascading deletes

            foreach (AMEstimateOper estimateOper in PXSelect<AMEstimateOper,
                Where<AMEstimateOper.estimateID, Equal<Required<AMEstimateOper.estimateID>>,
                    And<AMEstimateOper.revisionID, Equal<Required<AMEstimateOper.revisionID>>>>
            >.Select(this, Documents.Current.EstimateID, Documents.Current.RevisionID))
            {
                EstimateOperRecords.Delete(estimateOper);
            }
        }

        #region Menu Action Buttons

        public PXAction<AMEstimateItem> AddHistory;

        [PXUIField(DisplayName = Messages.AddHistory, MapEnableRights = PXCacheRights.Insert,
            MapViewRights = PXCacheRights.Insert)]
        [PXButton]
        public virtual IEnumerable addHistory(PXAdapter adapter)
        {
            HistoryFilterRecord.Current.EstimateID = Documents.Current.EstimateID;
            HistoryFilterRecord.Current.RevisionID = Documents.Current.RevisionID;

            bool estItemIsDirty = Documents.Cache.IsDirty;

            if (HistoryFilterRecord.AskExt() == WebDialogResult.OK &&
                !string.IsNullOrEmpty(HistoryFilterRecord.Current.Description))
            {
                EstimateHistoryRecords.Insert(new AMEstimateHistory()
                {
                    EstimateID = HistoryFilterRecord.Current.EstimateID,
                    RevisionID = HistoryFilterRecord.Current.RevisionID,
                    Description = HistoryFilterRecord.Current.Description
                });
            }

            HistoryFilterRecord.Cache.Clear();

            Documents.Cache.IsDirty = estItemIsDirty;

            return adapter.Get();
        }

        public PXAction<AMEstimateItem> CopyFrom;
        [PXUIField(DisplayName = Messages.CopyFrom, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable copyFrom(PXAdapter adapter)
        {
            if (Documents.Current == null)
            {
                return adapter.Get();
            }

            if (CopyEstimateFromFilter.AskExt() == WebDialogResult.OK)
            {
                DeleteOperations();
                ResetEstimateCalcValues();

                // Added to prevent WorkCenter Overheads from being added twice
                FieldUpdated.RemoveHandler<AMEstimateOper.workCenterID>(AMEstimateOper_WorkCenterID_FieldUpdated);

                switch (CopyEstimateFromFilter.Current.CopyFrom)
                {
                    case AMCopyEstimateFrom.CopyFromList.Estimate:
                        CopyFromEstimate(CopyEstimateFromFilter.Current.EstimateID,
                            CopyEstimateFromFilter.Current.RevisionID);
                        break;
                    case AMCopyEstimateFrom.CopyFromList.ProductionOrder:
                        CopyFromProductionOrder(CopyEstimateFromFilter.Current.OrderType, CopyEstimateFromFilter.Current.ProdOrdID);
                        break;
                    case AMCopyEstimateFrom.CopyFromList.BOM:
                        CopyFromBOM(CopyEstimateFromFilter.Current.BOMID, CopyEstimateFromFilter.Current.BOMRevisionID);
                        break;
                }

                FieldUpdated.AddHandler<AMEstimateOper.workCenterID>(AMEstimateOper_WorkCenterID_FieldUpdated);
            }
            this.Actions.PressSave();
            return adapter.Get();
        }

        protected virtual void ResetEstimateCalcValues()
        {
            var estimateItem = EstimateRecordSelected.Current;

            estimateItem.FixedLaborCost = 0m;
            estimateItem.FixedLaborCalcCost = 0m;
            estimateItem.FixedLaborOverride = false;
            estimateItem.VariableLaborCost = 0m;
            estimateItem.VariableLaborCalcCost = 0m;
            estimateItem.VariableLaborOverride = false;
            estimateItem.MachineCost = 0m;
            estimateItem.MachineCalcCost = 0m;
            estimateItem.MachineOverride = false;
            estimateItem.MaterialCost = 0m;
            estimateItem.MaterialCalcCost = 0m;
            estimateItem.MaterialOverride = false;
            estimateItem.ToolCost = 0m;
            estimateItem.ToolCalcCost = 0m;
            estimateItem.ToolOverride = false;
            estimateItem.FixedOverheadCost = 0m;
            estimateItem.FixedOverheadCalcCost = 0m;
            estimateItem.FixedOverheadOverride = false;
            estimateItem.VariableOverheadCost = 0m;
            estimateItem.VariableOverheadCalcCost = 0m;
            estimateItem.VariableOverheadOverride = false;

            EstimateRecordSelected.Cache.Update(estimateItem);
        }

        public PXAction<AMEstimateItem> CreateNewRevisionButton;
        [PXUIField(DisplayName = Messages.NewRevision, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable createNewRevisionButton(PXAdapter adapter)
        {
            if (Documents?.Current?.RevisionID == null)
            {
                return adapter.Get();
            }

            Actions.PressSave();
            var newRev = CreateNewRevision(Documents.Cache, this, Documents.Current.EstimateID, null);
            Documents.Current = newRev;
            adapter.Searches = new object[] { newRev.EstimateID, newRev.RevisionID  };

            return adapter.Get();
        }

        public PXAction<AMEstimateItem> CreateBOM;
        [PXUIField(DisplayName = Messages.CreateBOM, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable createBOM(PXAdapter adapter)
        {
            if (Documents.Current == null)
            {
                return adapter.Get();
            }

            if (IsDirty)
            {
                this.Actions.PressSave();
            }

            if (Documents.Current.SiteID != null && CreateBomItemFilter.Current.SiteID == null)
            {
                CreateBomItemFilter.Current.SiteID = Documents.Current.SiteID;
            }

            if (CreateBomItemFilter.AskExt() == WebDialogResult.OK)
            {
                PXLongOperation.StartOperation(this, delegate
                {
                    var bomMaintGraph = CreateInstance<BOMMaint>();
                    bomMaintGraph.Clear();
                    bomMaintGraph.IsImport = true;

                    bomMaintGraph.Documents.Current = CreateBomItem(Documents.Current, bomMaintGraph);

                    if (bomMaintGraph.Documents.Current != null)
                    {
                        bomMaintGraph.Caches<AMEstimateHistory>().Insert(new AMEstimateHistory
                        {
                            EstimateID = Documents.Current.EstimateID,
                            RevisionID = Documents.Current.RevisionID,
                            Description = Messages.GetLocal(Messages.EstimateCreatedBOM2, Documents.Current.RevisionID)
                        });
                    }

                    PXRedirectHelper.TryRedirect(bomMaintGraph, PXRedirectHelper.WindowMode.NewWindow);
                });
            }
            return adapter.Get();
        }

        /// <summary>
        /// Create a production order from the current estimate revision
        /// </summary>
        public PXAction<AMEstimateItem> CreateProdOrder;

        [PXUIField(DisplayName = Messages.CreateProdOrder, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable createProdOrder(PXAdapter adapter)
        {
            if (Documents.Current == null)
            {
                return adapter.Get();
            }

            if (IsDirty)
            {
                this.Actions.PressSave();
            }

            if (CreateProductionOrderFilter.AskExt() == WebDialogResult.OK)
            {
                PXLongOperation.StartOperation(this, delegate
                {
                    var prodGraph = CreateProductionOrder(Documents.Current);
                    var prodItem = prodGraph.ProdMaintRecords.Current;
                    if (prodItem == null)
                    {
                        return;
                    }

                    PXTrace.WriteInformation("Estimate '{0}' Revision '{1}' Created production order {2} {3}",
                        Documents.Current.EstimateID.TrimIfNotNullEmpty(),
                        Documents.Current.RevisionID.TrimIfNotNullEmpty(),
                        prodItem.OrderType.TrimIfNotNullEmpty(),
                        prodItem.ProdOrdID.TrimIfNotNullEmpty());

                    EstimateHistoryRecords.Insert(new AMEstimateHistory
                    {
                        Description = Messages.GetLocal(Messages.EstimateCreatedProdOrder,
                                prodItem.ProdOrdID.TrimIfNotNullEmpty(),
                                prodItem.OrderType,
                                Documents.Current.RevisionID.TrimIfNotNullEmpty())
                    });

                    Actions.PressSave();

                    PXRedirectHelper.TryRedirect(prodGraph, PXRedirectHelper.WindowMode.NewWindow);
                });
            }
            return adapter.Get();
        }

        /// <summary>
        /// Hyper-link on Operation CD to open the estimate operation page
        /// </summary>
        public PXAction<AMEstimateItem> ViewOperation;

        [PXButton]
        [PXUIField(DisplayName = "View Operation", Visible = false)]
        protected virtual void viewOperation()
        {
            if (Documents.Current == null)
            {
                return;
            }

            if (EstimateOperRecords.Current != null)
            {
                var operGraph = CreateInstance<EstimateOperMaint>();
                operGraph.EstimateOperationRecords.Current = this.EstimateOperRecords.Current;
                if (operGraph.EstimateOperationRecords.Current != null)
                {
                    throw new PXRedirectRequiredException(operGraph, true, string.Empty);
                }
            }
        }

        /// <summary>
        /// Hyper-link for reference Quote nbr
        /// </summary>
        public PXAction<AMEstimateItem> ViewQuote;
        [PXButton]
        [PXUIField(DisplayName = "View Quote", Visible = false)]
        protected virtual IEnumerable viewQuote(PXAdapter adapter)
        {
            if (Documents?.Current?.EstimateID == null || EstimateReferenceRecord?.Current == null ||
                Documents.Current.QuoteSource == EstimateSource.Estimate)
            {
                return adapter.Get();
            }

            if (Documents.Current.QuoteSource == EstimateSource.SalesOrder &&
                !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.QuoteType) &&
                !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.QuoteNbr))
            {
                var graphSO = CreateInstance<SOOrderEntry>();
                graphSO.Document.Current = graphSO.Document.Search<SOOrder.orderNbr>(EstimateReferenceRecord?.Current.QuoteNbr,
                    EstimateReferenceRecord?.Current.QuoteType);
                if (graphSO.Document.Current != null)
                {
                    throw new PXRedirectRequiredException(graphSO, true, string.Empty);
                }
            }

            if (Documents.Current.QuoteSource == EstimateSource.Opportunity &&
                    !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.OpportunityID) &&
                    !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.QuoteNbr))
            {
                var graphQuote = CreateInstance<QuoteMaint>();
                graphQuote.Quote.Current = graphQuote.Quote.Search<CRQuote.quoteNbr>(EstimateReferenceRecord?.Current.QuoteNbr,
                    EstimateReferenceRecord?.Current.OpportunityID);
                if (graphQuote.Quote.Current != null)
                {
                    throw new PXRedirectRequiredException(graphQuote, true, string.Empty);
                }
            }

            return adapter.Get();
        }

        /// <summary>
        /// Mark Current Revision as Primary
        /// </summary>
        public PXAction<AMEstimateItem> MarkAsPrimary;
        [PXButton]
        [PXUIField(DisplayName = "Mark As Primary", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        protected virtual IEnumerable markAsPrimary(PXAdapter adapter)
        {
            //TODO 2018R1 - Prompt user about change to quote when AMEstimateReference.HasQuoteReference()
            SetAsPrimaryRevision();
            return adapter.Get();
        }

        protected virtual void SetAsPrimaryRevision()
        {
            if (Documents?.Current?.PrimaryRevisionID == null
                || Documents.Current.IsPrimary.GetValueOrDefault()
                || Documents.Current.IsLockedByQuote.GetValueOrDefault())
            {
                return;
            }

            var currentEstimate = Documents.Current;
            var fromRevision = currentEstimate.PrimaryRevisionID;
            currentEstimate.PrimaryRevisionID = currentEstimate.RevisionID;
            currentEstimate = Documents.Update(currentEstimate);

            var primaryEstRef = EstimateReferenceRecord.Current ?? EstimateReferenceRecord.SelectSingle();
            var oldPrimaryEstRef = (AMEstimateReference)PXSelect<AMEstimateReference,
                    Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                        And<AMEstimateReference.revisionID, Equal<Required<AMEstimateItem.revisionID>>>>>
                .Select(this, currentEstimate.EstimateID, fromRevision);

            if (primaryEstRef == null || oldPrimaryEstRef == null)
            {
                return;
            }

            primaryEstRef = EstimateGraphHelper.AddEstimateReference(oldPrimaryEstRef, primaryEstRef);
            EstimateGraphHelper.ClearQuoteReferenceFields(ref oldPrimaryEstRef);

            EstimateReferenceRecord.Update(oldPrimaryEstRef);
            EstimateReferenceRecord.Current = EstimateReferenceRecord.Update(primaryEstRef);
        }

        /// <summary>
        /// Hyper-link to Create Inventory
        /// </summary>
        public PXAction<AMEstimateItem> CreateInventory;
        [PXButton]
        [PXUIField(DisplayName = Messages.CreateInventory, Visible = false)]
        protected virtual IEnumerable createInventory(PXAdapter adapter)
        {
            if (Documents.Current == null)
            {
                return adapter.Get();
            }

            var graph = CreateInstance<CreateInventoryItemProcess>();
            graph.Filter.Current.EstimateID = Documents.Current.EstimateID;
            graph.Filter.Current.RevisionID = Documents.Current.RevisionID;
            graph.Filter.Current.RevisionDate = Documents.Current.RevisionDate;
            graph.Filter.Current.Status = Documents.Current.EstimateStatus;
            if (graph.Filter.Current != null)
            {
                throw new PXRedirectRequiredException(graph, true, string.Empty);
            }

            return adapter.Get();
        }

        public PXAction<AMEstimateItem> Add2Order;

        [PXUIField(DisplayName = Messages.Add2Order, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable add2Order(PXAdapter adapter)
        {
            if (Documents.Current == null)
            {
                return adapter.Get();
            }

            //Warehouse is required if adding to an order
            if (Documents.Current.SiteID == null)
            {
                string msg = Messages.GetLocal(Messages.EstimateFieldRequiredForOrder,
                    PXUIFieldAttribute.GetDisplayName<AMEstimateItem.siteID>(Documents.Cache));

                Documents.Cache.RaiseExceptionHandling<AMEstimateItem.siteID>(Documents.Current,
                    Documents.Current.SiteID, new PXSetPropertyException(msg, PXErrorLevel.Warning));

                throw new PXException(msg);
            }


            if (Add2OrderFilter.AskExt() == WebDialogResult.OK)
            {
                // Saving the filter values before save as the OrderNbr was getting set to null and Order type set to default
                var orderType = Add2OrderFilter.Current.OrderType;
                var orderNbr = Add2OrderFilter.Current.OrderNbr;

                this.Actions.PressSave();

                SOOrderEntryAMExtension.AddEstimateToOrder(orderType,
                    orderNbr, Documents.Current);

                //refresh page
                Actions.PressCancel();
            }

            Add2OrderFilter.Cache.Clear();
            return adapter.Get();
        }

        #endregion

        protected virtual ProdMaint CreateProductionOrder(AMEstimateItem estimateItem)
        {
            if (estimateItem == null)
            {
                return null;
            }

            var prodMaintGraph = CreateInstance<ProdMaint>();
            prodMaintGraph.Clear();
            prodMaintGraph.IsImport = true;

            CreateProductionOrder(estimateItem, prodMaintGraph);

            return prodMaintGraph;
        }

        protected virtual void CreateProductionOrder(AMEstimateItem estimateItem, ProdMaint prodMaintGraph)
        {
            if (estimateItem.IsNonInventory.GetValueOrDefault() || estimateItem.InventoryID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.CannotCreateProductionOrderNonInventory,
                    estimateItem.InventoryCD.TrimIfNotNullEmpty()));
            }

            InventoryItem inventoryItem =
                PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(prodMaintGraph, estimateItem.InventoryID);

            if (inventoryItem == null)
            {
                throw new PXException(Messages.GetLocal(Messages.CouldNotFindItem));
            }

            if (!inventoryItem.StkItem.GetValueOrDefault())
            {
                throw new PXException(Messages.GetLocal(Messages.CannotCreateProdOrderForNonStock));
            }

            //Verify filter here...
            var sb = new System.Text.StringBuilder();
            if (CreateProductionOrderFilter.Current.SiteID.GetValueOrDefault() == 0)
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<CreateProdOrderFilter.siteID>(this.CreateProductionOrderFilter.Cache)));
            }

            if (CreateProductionOrderFilter.Current.LocationID.GetValueOrDefault() == 0)
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<CreateProdOrderFilter.locationID>(this.CreateProductionOrderFilter.Cache)));
            }

            if (CreateProductionOrderFilter.Current.OrderType == null)
            {
                sb.AppendLine(string.Format(PXMessages.Localize(ErrorMessages.FieldIsEmpty),
                    PXUIFieldAttribute.GetDisplayName<CreateProdOrderFilter.orderType>(this.CreateProductionOrderFilter.Cache)));
            }

            if (sb.Length != 0)
            {
                throw new PXException(sb.ToString());
            }

            prodMaintGraph.FieldDefaulting.AddHandler<AMProdItem.detailSource>((cache, e) =>
            {
                e.NewValue = ProductionDetailSource.Estimate;
            });
            prodMaintGraph.FieldUpdating.AddHandler<AMProdItem.detailSource>((cache, e) =>
            {
                e.NewValue = ProductionDetailSource.Estimate;
            });
            prodMaintGraph.FieldUpdating.AddHandler<AMProdItem.bOMID>((cache, e) =>
            {
                e.Cancel = true;
                e.NewValue = null;
            });
            prodMaintGraph.FieldUpdating.AddHandler<AMProdItem.bOMRevisionID>((cache, e) =>
            {
                e.Cancel = true;
                e.NewValue = null;
            });

            var amProdItem = new AMProdItem
            {
                OrderType = CreateProductionOrderFilter.Current.OrderType,
                DetailSource = ProductionDetailSource.Estimate
            };

            if (ProductionNumbering.Current.UserNumbering == true)
            {
                amProdItem.ProdOrdID = CreateProductionOrderFilter.Current.ProdOrdID;
            }

            amProdItem = PXCache<AMProdItem>.CreateCopy(prodMaintGraph.ProdMaintRecords.Insert(amProdItem));

            amProdItem.EstimateID = estimateItem.EstimateID;
            amProdItem.EstimateRevisionID = estimateItem.RevisionID;
            amProdItem.SiteID = CreateProductionOrderFilter.Current.SiteID;
            amProdItem.LocationID = CreateProductionOrderFilter.Current.LocationID;
            amProdItem.InventoryID = estimateItem.InventoryID;
            amProdItem.SubItemID = estimateItem.SubItemID;

            amProdItem = PXCache<AMProdItem>.CreateCopy(prodMaintGraph.ProdMaintRecords.Update(amProdItem));

            amProdItem.ConstDate = Accessinfo.BusinessDate;
            amProdItem.BOMID = null;
            amProdItem.QtytoProd = estimateItem.BaseOrderQty.GetValueOrDefault();
            amProdItem.UOM = inventoryItem.BaseUnit;
            amProdItem.BuildProductionBom = true;
            amProdItem.Reschedule = true;

            var refs = EstimateReferenceRecord.Current ?? PXSelect<
                               AMEstimateReference,
                               Where<AMEstimateReference.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                                   And<AMEstimateReference.revisionID, Equal<Required<AMEstimateItem.revisionID>>>>>
                           .Select(this, estimateItem.EstimateID, estimateItem.RevisionID);
            if (refs?.ProjectID != null)
            {
                amProdItem.ProjectID = refs.ProjectID;
                amProdItem.TaskID = refs.TaskID;
                amProdItem.CostCodeID = refs.CostCodeID;
            }

            prodMaintGraph.ProdMaintRecords.Update(amProdItem);
            prodMaintGraph.Actions.PressSave();
        }

        protected virtual void CreateBomDetails(AMEstimateItem estimateItem, BOMMaint bomMaintGraph)
        {
            foreach (AMEstimateOper estimateOper in PXSelect<AMEstimateOper,
                Where<AMEstimateOper.estimateID, Equal<Required<AMEstimateOper.estimateID>>,
                    And<AMEstimateOper.revisionID, Equal<Required<AMEstimateOper.revisionID>>>>
            >.Select(this, estimateItem.EstimateID, estimateItem.RevisionID))
            {
                var oper = bomMaintGraph.BomOperRecords.Insert(new AMBomOper
                {
                    OperationID = estimateOper.OperationID,
                    OperationCD = estimateOper.OperationCD,
                    WcID = estimateOper.WorkCenterID
                });

                oper.BFlush = estimateOper.BackFlushLabor;
                oper.Descr = estimateOper.Description;
                oper.MachineUnits = estimateOper.MachineUnits;
                oper.MachineUnitTime = estimateOper.MachineUnitTime;
                oper.SetupTime = estimateOper.SetupTime;
                oper.RunUnitTime = estimateOper.RunUnitTime;
                oper.RunUnits = estimateOper.RunUnits;
                oper.QueueTime = estimateOper.QueueTime;
                oper.OutsideProcess = estimateOper.OutsideProcess;
                oper.DropShippedToVendor = estimateOper.DropShippedToVendor;
                oper.VendorID = estimateOper.VendorID;
                oper.VendorLocationID = estimateOper.VendorLocationID;

                var insertedOper = bomMaintGraph.BomOperRecords.Update(oper);
                if (insertedOper == null)
                {
                    throw new PXException($"Unable to insert BOM operation from estimate {estimateOper.EstimateID} {estimateOper.RevisionID} operation {estimateOper.OperationCD}");
                }

                // Insert the material for the current operation
                foreach (AMEstimateMatl estimateMatl in PXSelect<AMEstimateMatl,
                    Where<AMEstimateMatl.estimateID, Equal<Required<AMEstimateMatl.estimateID>>,
                        And<AMEstimateMatl.revisionID, Equal<Required<AMEstimateMatl.revisionID>>,
                            And<AMEstimateMatl.operationID, Equal<Required<AMEstimateMatl.operationID>>>>>
                >.Select(this, estimateItem.EstimateID, estimateItem.RevisionID, estimateOper.OperationID))
                {
                    try
                    {
                        bomMaintGraph.BomMatlRecords.Insert(new AMBomMatl
                        {
                            OperationID = estimateOper.OperationID,
                            BFlush = estimateMatl.BackFlush,
                            Descr = estimateMatl.ItemDesc,
                            InventoryID = estimateMatl.InventoryID,
                            SubItemID = estimateMatl.SubItemID,
                            SiteID = estimateMatl.SiteID,
                            LocationID = estimateMatl.LocationID,
                            QtyReq = estimateMatl.QtyReq,
                            ScrapFactor = estimateMatl.ScrapFactor,
                            UnitCost = estimateMatl.UnitCost,
                            UOM = estimateMatl.UOM,
                            BatchSize = estimateMatl.BatchSize,
                            MaterialType = estimateMatl.MaterialType,
                            SubcontractSource = estimateMatl.SubcontractSource
                        });
                    }
                    catch (Exception e)
                    {
                        InventoryItem item = PXSelect<InventoryItem,
                            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this,
                            estimateMatl.InventoryID);

                        if (item == null)
                        {
                            throw;
                        }

                        throw new PXException(Messages.GetLocal(Messages.UnableToCreateBomFromEstimateMaterial),
                            estimateItem.EstimateID, estimateItem.RevisionID, item.InventoryCD, e.Message);
                    }
                }

                // Insert the tools for the current operation
                foreach (AMEstimateTool estimateTool in PXSelect<AMEstimateTool,
                    Where<AMEstimateTool.estimateID, Equal<Required<AMEstimateTool.estimateID>>,
                        And<AMEstimateTool.revisionID, Equal<Required<AMEstimateTool.revisionID>>,
                            And<AMEstimateTool.operationID, Equal<Required<AMEstimateTool.operationID>>>>>
                >.Select(this, estimateItem.EstimateID, estimateItem.RevisionID, estimateOper.OperationID))
                {
                    try
                    {
                        bomMaintGraph.BomToolRecords.Insert(new AMBomTool
                        {
                            OperationID = estimateOper.OperationID,
                            Descr = estimateTool.Description,
                            QtyReq = estimateTool.QtyReq,
                            UnitCost = estimateTool.UnitCost,
                            ToolID = estimateTool.ToolID
                        });
                    }
                    catch (Exception e)
                    {
                        if (e is PXOuterException)
                        {
                            PXTraceHelper.PxTraceOuterException((PXOuterException)e, PXTraceHelper.ErrorLevel.Error);
                        }

                        throw new PXException(Messages.GetLocal(Messages.UnableToCreateBomFromEstimateTool),
                            estimateItem.EstimateID, estimateItem.RevisionID, estimateTool.ToolID, e.Message);
                    }
                }

                // Insert the overheads for the current operation
                foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd,
                    Where<AMEstimateOvhd.estimateID, Equal<Required<AMEstimateOvhd.estimateID>>,
                        And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOvhd.revisionID>>,
                            And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOvhd.operationID>>>>>
                >.Select(this, estimateItem.EstimateID, estimateItem.RevisionID, estimateOper.OperationID))
                {
                    try
                    {
                        bomMaintGraph.BomOvhdRecords.Insert(new AMBomOvhd
                        {
                            OperationID = estimateOper.OperationID,
                            OFactor = estimateOvhd.OFactor,
                            OvhdID = estimateOvhd.OvhdID
                        });
                    }
                    catch (Exception e)
                    {
                        if (e is PXOuterException)
                        {
                            PXTraceHelper.PxTraceOuterException((PXOuterException)e, PXTraceHelper.ErrorLevel.Error);
                        }

                        throw new PXException(Messages.GetLocal(Messages.UnableToCreateBomFromEstimateOverhead),
                            estimateItem.EstimateID, estimateItem.RevisionID, estimateOvhd.OvhdID, e.Message);
                    }
                }
            }
        }

        protected virtual AMBomItem CreateBomItem(AMEstimateItem estimateItem, BOMMaint bomMaintGraph)
        {
            if (estimateItem.IsNonInventory.GetValueOrDefault() || estimateItem.InventoryID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.CannotCreateBOMNonInventory,
                    estimateItem.InventoryCD.TrimIfNotNullEmpty()));
            }

            InventoryItem inventoryItem = PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(bomMaintGraph,
                estimateItem.InventoryID);

            if (inventoryItem == null)
            {
                throw new PXException(Messages.GetLocal(Messages.CouldNotFindItem));
            }

            if (!inventoryItem.StkItem.GetValueOrDefault())
            {
                throw new PXException(Messages.GetLocal(Messages.CannotCreateBOMForNonStock));
            }

            if (estimateItem.SiteID == null && CreateBomItemFilter.Current.SiteID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.EstimateWarehouseRequiredForBOM));
            }

            var bomNumbering = (Numbering)PXSelectJoin<
                Numbering,
                InnerJoin<AMBSetup,
                    On<Numbering.numberingID, Equal<AMBSetup.bOMNumberingID>>>>
                .Select(this);

            //SetCurrentNumbering();
            var manualNumbering = bomNumbering?.UserNumbering ?? false;
            var newBomId = manualNumbering ? CreateBomItemFilter.Current.BOMID : null;

            if (manualNumbering && string.IsNullOrWhiteSpace(newBomId))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CreateBOMFilter.bOMID>(this.CreateBomItemFilter.Cache));
            }

            if (string.IsNullOrWhiteSpace(CreateBomItemFilter.Current.RevisionID))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CreateBOMFilter.revisionID>(this.CreateBomItemFilter.Cache));
            }

            var amBomItem = new AMBomItem
            {
                BOMID = newBomId,
                RevisionID = CreateBomItemFilter.Current.RevisionID,
                InventoryID = estimateItem.InventoryID,
                SubItemID = estimateItem.SubItemID,
                Descr = estimateItem.ItemDesc,
                SiteID = CreateBomItemFilter.Current.SiteID
            };

            bomMaintGraph.Documents.Insert(amBomItem);

            CreateBomDetails(Documents.Current, bomMaintGraph);

            return bomMaintGraph.Documents.Current;
        }

        protected virtual void AMEstimateItem_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            InitEstimateReferenceRow((AMEstimateItem)e.Row);
        }

        protected virtual void AMEstimateItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            InitEstimateReferenceRow((AMEstimateItem)e.Row);
        }

        protected virtual void InitEstimateReferenceRow(AMEstimateItem estimateItem)
        {
            if (estimateItem?.RevisionID == null || estimateItem.InventoryCD == null || estimateItem.EstimateClassID == null)
            {
                return;
            }

            var currentEstimateReference = EstimateReferenceRecord.Current;
            if (currentEstimateReference != null || EstimateReferenceRecord.Cache.Inserted.Any_())
            {
                return;
            }

            var isParentInserted = Documents.Cache.GetStatus(estimateItem) == PXEntryStatus.Inserted;
            if (!isParentInserted)
            {
                return;
            }

            EstimateReferenceRecord.Insert(new AMEstimateReference
            {
                RevisionID = estimateItem.RevisionID
            });
        }

        protected virtual void AMEstimateReference_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (AMEstimateReference)e.Row;
            var currentEstimateItem = Documents?.Current;
            if (row == null || currentEstimateItem == null)
            {
                return;
            }

            var enabled = currentEstimateItem.QuoteSource == EstimateSource.Estimate;

            PXUIFieldAttribute.SetEnabled<AMEstimateReference.quoteType>(EstimateReferenceRecord.Cache, row, enabled);
            PXUIFieldAttribute.SetEnabled<AMEstimateReference.quoteNbr>(EstimateReferenceRecord.Cache, row, enabled);
            PXUIFieldAttribute.SetEnabled<AMEstimateReference.orderType>(EstimateReferenceRecord.Cache, row, enabled);
            PXUIFieldAttribute.SetEnabled<AMEstimateReference.orderNbr>(EstimateReferenceRecord.Cache, row, enabled);
            PXUIFieldAttribute.SetEnabled<AMEstimateReference.bAccountID>(EstimateReferenceRecord.Cache, row, enabled);

            SetReferenceFieldsVisible(currentEstimateItem);

            if(!EstimateStatus.IsEditable(currentEstimateItem.EstimateStatus))
            {
                //disable the entire row as the status indicates the estimate is complete or canceled
                PXUIFieldAttribute.SetEnabled(cache, e.Row, false);
            }
        }

        protected virtual void SetReferenceFieldsVisible(AMEstimateItem estimateItem)
        {
            PXUIFieldAttribute.SetVisible<AMEstimateReference.opportunityID>(EstimateReferenceRecord.Cache, EstimateReferenceRecord.Current, estimateItem.QuoteSource == EstimateSource.Opportunity);
            PXUIFieldAttribute.SetVisible<AMEstimateReference.quoteType>(EstimateReferenceRecord.Cache, EstimateReferenceRecord.Current, estimateItem.QuoteSource != EstimateSource.Opportunity);
            var isSalesQuote = estimateItem.QuoteSource == EstimateSource.Opportunity &&
                               PXAccess.FeatureInstalled<FeaturesSet.salesQuotes>();
            PXUIFieldAttribute.SetVisible<AMEstimateReference.quoteNbr>(EstimateReferenceRecord.Cache, EstimateReferenceRecord.Current, estimateItem.QuoteSource == EstimateSource.Estimate);
            PXUIFieldAttribute.SetVisible<AMEstimateReference.quoteNbrLink>(EstimateReferenceRecord.Cache, EstimateReferenceRecord.Current, estimateItem.QuoteSource != EstimateSource.Estimate || isSalesQuote);
        }

        protected virtual void AMEstimateItem_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var amEstimateItem = (AMEstimateItem)e.Row;
            if (amEstimateItem == null || amEstimateItem.QuoteSource == EstimateSource.Estimate)
            {
                return;
            }

            var amEstimateReference = EstimateReferenceRecord.Current ?? EstimateReferenceRecord.Select();

            if (amEstimateReference == null)
            {
                return;
            }

            if (amEstimateItem.QuoteSource == EstimateSource.SalesOrder)
            {
                var salesMsg = Messages.GetLocal(Messages.UnableToDeleteEstimate,
                    Messages.GetLocal(Messages.SalesOrderRef), $"{EstimateReferenceRecord.Current.QuoteType} {EstimateReferenceRecord.Current.QuoteNbr}");

                sender.RaiseExceptionHandling<AMEstimateItem.estimateID>(amEstimateItem, amEstimateItem.EstimateID,
                    new PXSetPropertyException(salesMsg, PXErrorLevel.Error));
                e.Cancel = true;
                return;
            }

            var oppMsg = Messages.GetLocal(Messages.UnableToDeleteEstimate,
                Messages.GetLocal(Messages.OpportunityRef), EstimateReferenceRecord.Current.OpportunityID);
            if (!string.IsNullOrWhiteSpace(amEstimateReference.QuoteNbr) &&
                PXAccess.FeatureInstalled<FeaturesSet.salesQuotes>())
            {
                oppMsg = $"{oppMsg} {Messages.GetLocal(Messages.Quote)} {amEstimateReference.QuoteNbr}";
            }

            sender.RaiseExceptionHandling<AMEstimateItem.estimateID>(amEstimateItem, amEstimateItem.EstimateID,
                new PXSetPropertyException(oppMsg, PXErrorLevel.Error));
            e.Cancel = true;
        }

        protected virtual void AMEstimateItem_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Operation != PXDBOperation.Insert)
            {
                return;
            }

            //For this graph only during insert, lets remove the persisting check. The check and set of value will take place manually in EstimateItemProjectionAttribute
            PXDefaultAttribute.SetPersistingCheck<AMEstimateItem.primaryRevisionID>(cache, e.Row, PXPersistingCheck.Nothing);
        }

        protected virtual void AMEstimateItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (AMEstimateItem)e.Row;
            CreateProdOrder.SetEnabled(row != null);
            var estimateReference = EstimateReferenceRecord.Current ?? EstimateReferenceRecord.Select();
            if (row?.RevisionID == null)
            {
                return;
            }

            var isInserted = cache.GetStatus(row) == PXEntryStatus.Inserted;
            var isEditable = EstimateStatus.IsEditable(row.EstimateStatus);

            CopyFrom.SetEnabled(isEditable && !isInserted);
            CreateNewRevisionButton.SetEnabled(isEditable && !isInserted);
            CreateProdOrder.SetEnabled(!row.IsNonInventory.GetValueOrDefault() && !isInserted);
            CreateBOM.SetEnabled(!row.IsNonInventory.GetValueOrDefault() && !isInserted);
            MarkAsPrimary.SetEnabled(!isInserted && !row.IsPrimary.GetValueOrDefault() && isEditable && !row.IsLockedByQuote.GetValueOrDefault());
            CreateInventory.SetEnabled(!isInserted);
            Add2Order.SetEnabled(!row.IsNonInventory.GetValueOrDefault() &&
                                 !isInserted && estimateReference != null &&
                                 row.QuoteSource == EstimateSource.Estimate &&
                                 string.IsNullOrWhiteSpace(estimateReference.OrderNbr));

            PXUIFieldAttribute.SetEnabled<AMEstimateItem.curyID>(cache, row,
                row.QuoteSource == EstimateSource.Estimate);
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.itemClassID>(cache, row,
                row.IsNonInventory.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.fixedLaborCost>(cache, row,
                row.FixedLaborOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.variableLaborCost>(cache, row,
                row.VariableLaborOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.machineCost>(cache, row,
                row.MachineOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.materialCost>(cache, row,
                row.MaterialOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.subcontractCost>(cache, row,
                row.SubcontractOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.toolCost>(cache, row,
                row.ToolOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.fixedOverheadCost>(cache, row,
                row.FixedOverheadOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.variableOverheadCost>(cache, row,
                row.VariableOverheadOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.leadTime>(cache, row,
                row.LeadTimeOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.curyUnitPrice>(cache, row,
                row.PriceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.laborMarkupPct>(cache, row,
                !row.PriceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.machineMarkupPct>(cache, row,
                !row.PriceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.materialMarkupPct>(cache, row,
                !row.PriceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.subcontractMarkupPct>(cache, row,
                !row.PriceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.toolMarkupPct>(cache, row,
                !row.PriceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.overheadMarkupPct>(cache, row,
                !row.PriceOverride.GetValueOrDefault());

            EnableRecords(isEditable && (row.IsPrimary.GetValueOrDefault() || isInserted));
            EstimateOperRecords.AllowInsert = isEditable && row.IsPrimary.GetValueOrDefault();

            if (!Documents.AllowUpdate)
            {
                Documents.AllowUpdate = true;
                PXUIFieldAttribute.SetEnabled(cache, row, false);
                PXUIFieldAttribute.SetEnabled<AMEstimateItem.revisionID>(cache, row, true);
                PXUIFieldAttribute.SetEnabled<AMEstimateItem.estimateID>(cache, row, true);
            }
            PXUIFieldAttribute.SetEnabled<AMEstimateItem.estimateStatus>(cache, row, !row.IsLockedByQuote.GetValueOrDefault());

            // Enable or disable reports
            EnableReports();
            SetReferenceFieldsVisible(row);
        }

        protected virtual void EnableRecords(bool enableRecords)
        {
            //EstimateRecords.AllowInsert = enableRecords;
            Documents.AllowUpdate = enableRecords;
            Documents.AllowDelete = enableRecords;

            //Moved to Row selected to disable if Estimate record has not been saved
            //EstimateOperRecords.AllowInsert = enableRecords;
            EstimateOperRecords.AllowUpdate = enableRecords;
            EstimateOperRecords.AllowDelete = enableRecords;

            EstimateRecordSelected.AllowInsert = enableRecords;
            EstimateRecordSelected.AllowUpdate = enableRecords;
            EstimateRecordSelected.AllowDelete = enableRecords;

            EstimateReferenceRecord.AllowInsert =
                EstimateReferenceRecord.AllowUpdate =
                    EstimateReferenceRecord.AllowDelete = enableRecords;
        }

        protected virtual void EnableReports()
        {
            ReportSummary.SetEnabled(false);
            ReportQuote.SetEnabled(false);

            if (Documents.Current == null)
            {
                return;
            }

            var currentEstimateRowStatus = Documents.Cache.GetStatus(Documents.Current);

            ReportSummary.SetEnabled(currentEstimateRowStatus != PXEntryStatus.Inserted);

            if (EstimateReferenceRecord?.Current?.RevisionID == null)
            {
                return;
            }

            var salesQuoteValid = Documents.Current.QuoteSource == EstimateSource.SalesOrder &&
                                  !string.IsNullOrWhiteSpace(EstimateReferenceRecord.Current.QuoteType) &&
                                  !string.IsNullOrWhiteSpace(EstimateReferenceRecord.Current.QuoteNbr);

            var opportunityQuoteValid = Documents.Current.QuoteSource == EstimateSource.Opportunity &&
                                        !string.IsNullOrWhiteSpace(EstimateReferenceRecord.Current.OpportunityID) &&
                                        EstimateReferenceRecord.Current.OpportunityQuoteID != null &&
                                        !string.IsNullOrWhiteSpace(EstimateReferenceRecord.Current.QuoteNbr);

            ReportQuote.SetEnabled(salesQuoteValid || opportunityQuoteValid);
        }

        protected virtual void AMEstimateOper_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            #region Disable or Enable all cost fields

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.fixedLaborCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.FixedLaborOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.variableLaborCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.VariableLaborOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.machineCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.MachineOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.materialCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.MaterialOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.toolCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.ToolOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.fixedOverheadCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.FixedOverheadOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.variableOverheadCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.VariableOverheadOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.subcontractCost>(EstimateOperRecords.Cache, amEstimateOper,
                amEstimateOper.SubcontractOverride.GetValueOrDefault());
            #endregion
        }

        protected virtual void CreateProdOrderFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            ProductionNumbering.Current = PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID,
                    Equal<Numbering.numberingID>>>, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>
            >.Select(this, CreateProductionOrderFilter.Current.OrderType);

            if (ProductionNumbering.Current.UserNumbering != true)
            {
                CreateProductionOrderFilter.Current.ProdOrdID = ProductionNumbering.Current.NewSymbol;
                PXUIFieldAttribute.SetEnabled<CreateProdOrderFilter.prodOrdID>(CreateProductionOrderFilter.Cache,
                    CreateProductionOrderFilter.Current, false);
            }
        }

        protected virtual void CreateBOMFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var bomNumbering = (Numbering)PXSelectJoin<Numbering,
                LeftJoin<AMBSetup, On<AMBSetup.bOMNumberingID, Equal<Numbering.numberingID>>>,
                Where<Numbering.numberingID, Equal<AMBSetup.bOMNumberingID>>>.Select(this);
            if (bomNumbering?.NumberingID == null)
            {
                return;
            }

            if (!bomNumbering.UserNumbering.GetValueOrDefault())
            {
                CreateBomItemFilter.Current.BOMID = bomNumbering.NewSymbol;
            }

            PXUIFieldAttribute.SetEnabled<CreateBOMFilter.bOMID>(CreateBomItemFilter.Cache,
                CreateBomItemFilter.Current, bomNumbering.UserNumbering.GetValueOrDefault());
        }

        #region CACHE ATTACHED

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Operation Nbr")]
        [PXLineNbr(typeof(AMEstimateItem.lineCntrOper))]
        protected virtual void AMEstimateOper_OperationID_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Fixed Labor Override", Visible = false)]
        protected virtual void AMEstimateOper_FixedLaborOverride_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Variable Labor Override", Visible = false)]
        protected virtual void AMEstimateOper_VariableLaborOverride_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Machine Override", Visible = false)]
        protected virtual void AMEstimateOper_MachineOverride_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Material Override", Visible = false)]
        protected virtual void AMEstimateOper_MaterialOverride_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tool Override", Visible = false)]
        protected virtual void AMEstimateOper_ToolOverride_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Fixed Ovhd Override", Visible = false)]
        protected virtual void AMEstimateOper_FixedOverheadOverride_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Variable Ovhd Override", Visible = false)]
        protected virtual void AMEstimateOper_VariableOverheadOverride_CacheAttached(PXCache cache)
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Subcontract Override", Visible = false)]
        protected virtual void AMEstimateOper_SubcontractOverride_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBDefault(typeof(AMEstimateItem.curyInfoID), DefaultForInsert = true, DefaultForUpdate = false)]
        protected virtual void _(Events.CacheAttached<AMEstimateReference.curyInfoID> e) { }

        #endregion

        protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (Documents.Cache.Current != null)
            {
                e.NewValue = ((AMEstimateItem)Documents.Cache.Current).RevisionDate;
                e.Cancel = true;
            }
        }

        protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var info = (CurrencyInfo)e.Row;
            if (info == null)
            {
                return;
            }

            if (Documents?.Current?.QuoteSource != null && Documents.Current.QuoteSource != EstimateSource.Estimate)
            {
                EnableCurrency(sender, info, false);
                return;
            }

            var curyenabled = info.AllowUpdate(Documents?.Cache);

            if (EstimateReferenceRecord?.Current?.BAccountID == null)
            {
                return;
            }

            if (CurrentBAccount != null && (CurrentBAccount.Current?.BAccountID == null ||
                CurrentBAccount.Current.BAccountID != EstimateReferenceRecord.Current.BAccountID))
            {
                CurrentBAccount.Current = CurrentBAccount.Select();
            }

            if (CurrentBAccount?.Current?.BAccountID != null && CurrentBAccount.Current.IsCustomerOrCombined.GetValueOrDefault())
            {
                Customer customer =
                    PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, CurrentBAccount?.Current?.BAccountID);

                if (customer != null && !customer.AllowOverrideRate.GetValueOrDefault())
                {
                    curyenabled = false;
                }
            }

            EnableCurrency(sender, info, curyenabled);
        }

        protected virtual void EnableCurrency(PXCache sender, CurrencyInfo info, bool enabled)
        {
            // ~~~~~~
            // It is very important that we do not change the cury info for an estimate linked to an order
            //     potential issue is if the sales order changes the cury info detail and the estimate does not update accordingly...
            // ~~~~~~
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, enabled);
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, enabled);
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, enabled);
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, enabled);
        }

        protected virtual void AMEstimateItem_EstimateClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            AMEstimateItem amEstimateItem = (AMEstimateItem)e.Row;
            if (amEstimateItem == null || !amEstimateItem.IsNonInventory.GetValueOrDefault())
            {
                return;
            }

            AMEstimateClass estimateClass =
                (AMEstimateClass)
                PXSelectorAttribute.Select<AMEstimateItem.estimateClassID>(cache, amEstimateItem,
                    amEstimateItem.EstimateClassID);

            if (estimateClass == null)
            {
                return;
            }

            if (estimateClass.ItemClassID == null)
            {
                amEstimateItem.ItemClassID = estimateClass.ItemClassID;

                INItemClass itemClass = PXSelect<INItemClass,
                    Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>>>.Select(this,
                    amEstimateItem.ItemClassID);

                if (itemClass != null && !string.IsNullOrWhiteSpace(itemClass.BaseUnit))
                {
                    amEstimateItem.UOM = itemClass.BaseUnit;
                }
            }

            //put in the cost price defaults here?

            if (EstimateReferenceRecord.Current != null && !string.IsNullOrWhiteSpace(estimateClass.TaxCategoryID))
            {
                EstimateReferenceRecord.Current.TaxCategoryID = estimateClass.TaxCategoryID;
            }
        }


        protected virtual void AMEstimateItem_ItemClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            AMEstimateItem amEstimateItem = (AMEstimateItem)e.Row;
            if (amEstimateItem == null || !amEstimateItem.IsNonInventory.GetValueOrDefault())
            {
                return;
            }
            INItemClass itemClass = PXSelect<INItemClass,
                Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>>>.Select(this,
                amEstimateItem.ItemClassID);

            if (itemClass != null && !string.IsNullOrWhiteSpace(itemClass.BaseUnit))
            {
                amEstimateItem.UOM = itemClass.BaseUnit;
            }
        }



        /// <summary>
        /// Find a given estimate operations in the cache and if not there select from the DB
        /// </summary>
        /// <returns>Located estimate operation - otherwise null</returns>
        protected virtual AMEstimateOper FindEstimateOper(string estimateID, string revisionID, int? operationID)
        {
            if (string.IsNullOrWhiteSpace(estimateID)
                || string.IsNullOrWhiteSpace(revisionID)
                || operationID.GetValueOrDefault() == 0)
            {
                return null;
            }

            return FindEstimateOper(new AMEstimateOper()
            {
                EstimateID = estimateID,
                RevisionID = revisionID,
                OperationID = operationID
            });
        }

        /// <summary>
        /// Find a given estimate operations in the cache and if not there select from the DB
        /// </summary>
        /// <returns>Located estimate operation - otherwise null</returns>
        protected virtual AMEstimateOper FindEstimateOper(AMEstimateOper estimateOperLocate)
        {
            if (estimateOperLocate == null
                || string.IsNullOrWhiteSpace(estimateOperLocate.EstimateID)
                || string.IsNullOrWhiteSpace(estimateOperLocate.RevisionID)
                || estimateOperLocate.OperationID.GetValueOrDefault() == 0)
            {
                return null;
            }

            AMEstimateOper searchRow = (AMEstimateOper)EstimateOperRecords.Cache.Locate(estimateOperLocate);

            if (searchRow != null)
            {
                return searchRow;
            }

            return PXSelect<AMEstimateOper,
                Where<AMEstimateOper.estimateID, Equal<Required<AMEstimateOper.estimateID>>,
                    And<AMEstimateOper.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                        And<AMEstimateOper.operationID, Equal<Required<AMEstimateOper.operationID>>>>>
            >.Select(this, estimateOperLocate.EstimateID, estimateOperLocate.RevisionID, estimateOperLocate.OperationID);
        }

        /// <summary>
        /// Make sure the current estimate operation is set based on the given operation/child record
        /// </summary>
        /// <param name="estimateOper">An estimate operation related row</param>
        /// <returns>True if current is set</returns>
        protected virtual bool SetCurrentEstimateOperation(IEstimateOper estimateOper)
        {
            var found = false;

            if (EstimateOperRecords.Current != null
                && EstimateOperRecords.Current.EstimateID.EqualsWithTrim(estimateOper.EstimateID)
                && EstimateOperRecords.Current.RevisionID.EqualsWithTrim(estimateOper.RevisionID)
                && EstimateOperRecords.Current.OperationID == estimateOper.OperationID)
            {
                //Current is already a match...
                return true;
            }

            var locatedOper = FindEstimateOper(estimateOper.EstimateID, estimateOper.RevisionID, estimateOper.OperationID);

            if (locatedOper != null)
            {
                EstimateOperRecords.Current = locatedOper;
                found = true;
            }

            return found;
        }

        /// <summary>
        /// Insert overheads for estimate operation related to the work center
        /// </summary>
        /// <returns>True if overheads inserted</returns>
        protected virtual bool InsertEstiamteWorkcenterOverheads(PXCache cache, AMEstimateOper amEstimateOper)
        {
            if (amEstimateOper == null)
            {
                return false;
            }

            return InsertEstiamteWorkcenterOverheads(cache, amEstimateOper, amEstimateOper.WorkCenterID);
        }

        /// <summary>
        /// Insert overheads for estimate operation related to the work center
        /// </summary>
        /// <returns>True if overheads inserted</returns>
        protected virtual bool InsertEstiamteWorkcenterOverheads(PXCache cache, AMEstimateOper amEstimateOper, string workcenterID)
        {
            if (amEstimateOper == null
                || string.IsNullOrWhiteSpace(amEstimateOper.EstimateID)
                || cache == null
                || string.IsNullOrWhiteSpace(workcenterID))
            {
                return false;
            }

            SetCurrentEstimateOperation(amEstimateOper);
            bool inserted = false;
            foreach (PXResult<AMWCOvhd, AMOverhead> result in PXSelectJoin<AMWCOvhd,
                InnerJoin<AMOverhead, On<AMWCOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMWCOvhd.wcID, Equal<Required<AMWCOvhd.wcID>>>>.Select(this, workcenterID))
            {
                var wcOvhd = (AMWCOvhd)result;
                var amOverhead = (AMOverhead)result;

                var newEstimateOvhd = EstimateOperOvhdRecords.Insert(new AMEstimateOvhd()
                {
                    EstimateID = amEstimateOper.EstimateID,
                    RevisionID = amEstimateOper.RevisionID,
                    OperationID = amEstimateOper.OperationID,
                    OvhdID = wcOvhd.OvhdID,
                    OFactor = wcOvhd.OFactor,
                    OvhdType = amOverhead.OvhdType,
                    Description = amOverhead.Descr,
                    OverheadCostRate = amOverhead.CostRate,
                    WCFlag = true
                });
                
                // Subtract the Added overhead value from the Estimate fixed or Variable Overhead to prevent doubling up on EstimateItem
                if (newEstimateOvhd.OvhdType == OverheadType.FixedType)
                {
                    Documents.Current.FixedOverheadCalcCost -= newEstimateOvhd.FixedOvhdOperCost;
                }
                else
                {
                    Documents.Current.VariableOverheadCalcCost -= newEstimateOvhd.VariableOvhdOperCost;
                }
                inserted = true;
            }

             return inserted;
        }

        /// <summary>
        /// Deleted overheads from estimate operation related to the work center
        /// </summary>
        /// <returns>True if overheads deleted</returns>
        protected virtual bool DeleteEstimateWorkcenterOverheads(AMEstimateOper amEstimateOper)
        {
            if (amEstimateOper == null
                || string.IsNullOrWhiteSpace(amEstimateOper.EstimateID))
            {
                return false;
            }
            bool deleted = false;
            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd,
                Where<AMEstimateOvhd.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                    And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateItem.revisionID>>,
                    And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOvhd.operationID>>,
                    And<AMEstimateOvhd.wCFlag, Equal<boolTrue>>>>>
                    >.Select(this, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                EstimateOperOvhdRecords.Delete(estimateOvhd);

                if (Documents.Current == null)
                {
                    continue;
                }

                // Add the deleted overhead value from the Estimate fixed or Variable Overhead to prevent doubling up on EstimateItem
                if (estimateOvhd.OvhdType == OverheadType.FixedType)
                {
                    Documents.Current.FixedOverheadCalcCost += estimateOvhd.FixedOvhdOperCost;
                }
                else
                {
                    Documents.Current.VariableOverheadCalcCost += estimateOvhd.VariableOvhdOperCost;
                }
                deleted = true;
            }

            return deleted;
        }

        protected virtual decimal GetWCMachineCost(AMWC workcenter)
        {
            if (workcenter == null
                || string.IsNullOrWhiteSpace(workcenter.WcID))
            {
                throw new PXArgumentException("workcenter");
            }
            decimal machStdCost = 0m;
            foreach (PXResult<AMWCMach, AMMach> result in PXSelectJoin<AMWCMach,
                InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>>,
                Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>>>.Select(this, workcenter.WcID))
            {
                var wcMachine = (AMWCMach)result;
                var machine = (AMMach)result;

                if (string.IsNullOrWhiteSpace(wcMachine.MachID) || string.IsNullOrWhiteSpace(machine.MachID) ||
                    !machine.ActiveFlg.GetValueOrDefault())
                {
                    continue;
                }
                machStdCost += wcMachine.MachineOverride.GetValueOrDefault()
                    ? wcMachine.StdCost.GetValueOrDefault()
                    : machine.StdCost.GetValueOrDefault();
            }
            return machStdCost;
        }

        protected virtual void AMEstimateOper_WorkCenterID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            AMWC amwc = PXSelect<AMWC, Where<AMWC.wcID, Equal<Required<AMWC.wcID>>>>.Select(this,
                amEstimateOper.WorkCenterID);

            if (amwc == null)
            {
                return;
            }
            amEstimateOper.MachineStdCost = GetWCMachineCost(amwc);
            DeleteEstimateWorkcenterOverheads(amEstimateOper);
            InsertEstiamteWorkcenterOverheads(cache, amEstimateOper);
            cache.SetValueExt<AMEstimateOper.outsideProcess>(amEstimateOper, amwc.OutsideFlg.GetValueOrDefault());
        }

        protected virtual void AMCopyEstimateFrom_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var copyEstimateFrom = (AMCopyEstimateFrom)e.Row;
            if (copyEstimateFrom == null)
            {
                return;
            }

            PXUIFieldAttribute.SetVisible<AMCopyEstimateFrom.estimateID>(cache, copyEstimateFrom,
                copyEstimateFrom.CopyFrom == AMCopyEstimateFrom.CopyFromList.Estimate);
            PXUIFieldAttribute.SetVisible<AMCopyEstimateFrom.revisionID>(cache, copyEstimateFrom,
                copyEstimateFrom.CopyFrom == AMCopyEstimateFrom.CopyFromList.Estimate);
            PXUIFieldAttribute.SetVisible<AMCopyEstimateFrom.bOMID>(cache, copyEstimateFrom,
                copyEstimateFrom.CopyFrom == AMCopyEstimateFrom.CopyFromList.BOM);
            PXUIFieldAttribute.SetVisible<AMCopyEstimateFrom.bOMRevisionID>(cache, copyEstimateFrom,
                copyEstimateFrom.CopyFrom == AMCopyEstimateFrom.CopyFromList.BOM);
            PXUIFieldAttribute.SetVisible<AMCopyEstimateFrom.orderType>(cache, copyEstimateFrom,
                copyEstimateFrom.CopyFrom == AMCopyEstimateFrom.CopyFromList.ProductionOrder);
            PXUIFieldAttribute.SetVisible<AMCopyEstimateFrom.prodOrdID>(cache, copyEstimateFrom,
                copyEstimateFrom.CopyFrom == AMCopyEstimateFrom.CopyFromList.ProductionOrder);
        }

        protected virtual void AMCopyEstimateFrom_EstimateID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amCopyEstimateFrom = (AMCopyEstimateFrom)e.Row;
            if (amCopyEstimateFrom == null)
            {
                return;
            }

            amCopyEstimateFrom.RevisionID = null;
        }

        protected virtual void AMCopyEstimateFrom_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amCopyEstimateFrom = (AMCopyEstimateFrom)e.Row;
            if (amCopyEstimateFrom == null)
            {
                return;
            }

            amCopyEstimateFrom.ProdOrdID = null;
        }

        protected virtual void AMCopyEstimateFrom_BOMID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amCopyEstimateFrom = (AMCopyEstimateFrom)e.Row;
            if (amCopyEstimateFrom == null)
            {
                return;
            }

            amCopyEstimateFrom.BOMRevisionID = null;
        }

        protected virtual void CreateProdOrderFilter_ProdOrdID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var createProdOrderFilter = (CreateProdOrderFilter)e.Row;
            if (createProdOrderFilter == null)
            {
                return;
            }

            if (ProductionNumbering.Current.UserNumbering != true)
            {
                return;
            }

            AMProdItem amProdItem = PXSelect<
                AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>
            >>.Select(this, createProdOrderFilter.OrderType, createProdOrderFilter.ProdOrdID);
            if (amProdItem != null)
            {
                throw new PXSetPropertyException(Messages.GetLocal(Messages.ProductionOrderIDIsAlreadyUsed,
                        createProdOrderFilter.ProdOrdID, createProdOrderFilter.OrderType));
            }
        }

        protected virtual void CreateProdOrderFilter_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var createProdOrderFilter = (CreateProdOrderFilter)e.Row;
            if (createProdOrderFilter == null)
            {
                return;
            }

            int? loc = InventoryHelper.DfltLocation.GetDefault(this, InventoryHelper.DfltLocation.BinType.Receipt,
                EstimateRecordSelected.Current.InventoryID, createProdOrderFilter.SiteID, false);

            createProdOrderFilter.LocationID = loc;
        }

        #region Menus

        public PXAction<AMEstimateItem> ActionsMenu;

        [PXButton(CommitChanges = true, MenuAutoOpen = true)]
        [PXUIField(DisplayName = Messages.Actions)]
        protected virtual void actionsMenu()
        {
        }

        public PXAction<AMEstimateItem> ReportsMenu;

        [PXButton(CommitChanges = true, MenuAutoOpen = true)]
        [PXUIField(DisplayName = Messages.Reports)]
        protected virtual void reportsMenu()
        {
        }

        public PXAction<AMEstimateItem> ReportSummary;

        [PXUIField(DisplayName = Messages.Summary, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable reportSummary(PXAdapter adapter)
        {
            if (Documents.Current != null)
            {
                string userMessage = Messages.GetLocal(Messages.PrintMaterialConfirmation);
                var parameters =
                    Reports.EstimateSummaryReportParams.FromEstimateId(EstimateRecordSelected.Current.EstimateID,
                        EstimateRecordSelected.Current.RevisionID, true);
                WebDialogResult response = Documents.Ask(Messages.PrintMaterial, userMessage, MessageButtons.YesNo);
                if (response == WebDialogResult.No)
                {
                    parameters =
                        Reports.EstimateSummaryReportParams.FromEstimateId(EstimateRecordSelected.Current.EstimateID,
                            EstimateRecordSelected.Current.RevisionID, false);
                }
                throw new PXReportRequiredException(parameters, Reports.EstimateSummaryReportParams.ReportID,
                    PXBaseRedirectException.WindowMode.New, Reports.EstimateSummaryReportParams.ReportName);
            }
            return adapter.Get();
        }

        public PXAction<AMEstimateItem> ReportQuote;
        [PXUIField(DisplayName = Messages.Quote, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable reportQuote(PXAdapter adapter)
        {
            if (Documents?.Current?.EstimateID == null || EstimateReferenceRecord?.Current == null ||
                Documents.Current.QuoteSource == EstimateSource.Estimate)
            {
                return adapter.Get();
            }

            // Source is Sales Order
            if (Documents.Current.QuoteSource == EstimateSource.SalesOrder &&
                !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.QuoteType) &&
                !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.QuoteNbr))
            {
                var parameters = Reports.SOQuoteReportParams.FromEstimateId(EstimateReferenceRecord?.Current.QuoteType,
                    EstimateReferenceRecord?.Current.QuoteNbr);
                throw new PXReportRequiredException(parameters, Reports.SOQuoteReportParams.ReportID,
                    PXBaseRedirectException.WindowMode.New, Reports.SOQuoteReportParams.ReportName);
            }

            // Source is Opportunity
            if (Documents.Current.QuoteSource == EstimateSource.Opportunity &&
                !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.OpportunityID) &&
                !string.IsNullOrWhiteSpace(EstimateReferenceRecord?.Current.QuoteNbr))
            {
                var parameters = Reports.CRQuoteReportParams.FromEstimateId(EstimateReferenceRecord?.Current.OpportunityID,
                    EstimateReferenceRecord?.Current.QuoteNbr);
                throw new PXReportRequiredException(parameters, Reports.CRQuoteReportParams.ReportID,
                    PXBaseRedirectException.WindowMode.New, Reports.CRQuoteReportParams.ReportName);
            }

            return adapter.Get();
        }

        #endregion
    }

    [Serializable]
    [PXCacheName("Add to Order Filter")]
    public class AddToOrderFilter : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }


        protected String _OrderType;
        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXDefault(SOOrderTypeConstants.SalesOrder, typeof(SOSetup.defaultOrderType))]
        [PXSelector(typeof(Search5<SOOrderType.orderType, InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>, LeftJoin<SOSetupApproval, On<SOOrderType.orderType, Equal<SOSetupApproval.orderType>>>>, Where<SOOrderType.behavior, Equal<SOOrderTypeConstants.salesOrder>, And<SOOrderTypeExt.aMEstimateEntry, Equal<False>>>, Aggregate<GroupBy<SOOrderType.orderType>>>))]
        [PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }
        #endregion

        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

        protected String _OrderNbr;
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PX.Objects.SO.SO.RefNbr(typeof(Search2<SOOrder.orderNbr, LeftJoin<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>, And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>, Where<SOOrder.orderType, Equal<Current<AddToOrderFilter.orderType>>, And<SOOrder.completed, Equal<False>, And<SOOrder.cancelled, Equal<False>>>>, OrderBy<Desc<SOOrder.orderNbr>>>), Filterable = true)]
        public virtual String OrderNbr
        {
            get { return this._OrderNbr; }
            set { this._OrderNbr = value; }
        }
        #endregion
    }

    [Serializable]
    [PXCacheName("History Filter")]
    public class HistoryFilter : IBqlTable
    {
        #region EstimateID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }


        protected String _EstimateID;

        [PXString]
        [PXUIField(DisplayName = "EstimateID", Enabled = false)]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion

        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }


        protected String _RevisionID;

        [PXString]
        [PXUIField(DisplayName = "EstimateID", Enabled = false)]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }
        #endregion

        #region Description

        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }


        protected String _Description;

        [PXString]
        [PXUIField(DisplayName = "EstimateID")]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }

        #endregion
    }

    //SHARED WITH ESTIMATING AND CRITICAL MATERIALS!!!
    [Serializable]
    [PXCacheName("Create Production Order Filter")]
    public class CreateProdOrderFilter : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(Search<AMPSetup.defaultOrderType>))]
        [AMOrderTypeField]
        [PXRestrictor(typeof(Where<AMOrderType.function, Equal<OrderTypeFunction.regular>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion

        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr]
        public virtual String ProdOrdID
        {
            get { return this._ProdOrdID; }
            set { this._ProdOrdID = value; }
        }
        #endregion

        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }


        protected Int32? _SiteID;

        [Site(Required = true)]
        [PXDefault(typeof(Search<AMEstimateItem.siteID, Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
            And<AMEstimateItem.revisionID, Equal<Current<AMEstimateItem.revisionID>>>>>))]
        public virtual Int32? SiteID
        {
            get { return this._SiteID; }
            set { this._SiteID = value; }
        }
        #endregion

        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }


        protected Int32? _LocationID;

        [PXRestrictor(typeof(Where<INLocation.receiptsValid, Equal<True>>), PX.Objects.IN.Messages.LocationReceiptsInvalid, CacheGlobal = true)]
        [PXRestrictor(typeof(Where<INLocation.assemblyValid, Equal<True>>), Messages.LocationAssemblyInvalid, CacheGlobal = true)]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        [Location(typeof(CreateProdOrderFilter.siteID), Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [PXDefault]
        public virtual Int32? LocationID
        {
            get { return this._LocationID; }
            set { this._LocationID = value; }
        }
        #endregion
    }

    [Serializable]
    [PXCacheName("Create BOM Filter")]
    public class CreateBOMFilter : IBqlTable, IBomRevision
    {
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        [BomID(Required = true)]
        [PXDefault]
        public virtual string BOMID { get; set; }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        [RevisionIDField(Required = true)]
        [PXDefault(typeof(Search<AMBSetup.defaultRevisionID>))]
        public virtual string RevisionID { get; set; }
        #endregion 

        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }


        protected Int32? _SiteID;

        [Site(Required = true)]
        [PXDefault]
        public virtual Int32? SiteID
        {
            get { return this._SiteID; }
            set { this._SiteID = value; }
        }
        #endregion
    }
}