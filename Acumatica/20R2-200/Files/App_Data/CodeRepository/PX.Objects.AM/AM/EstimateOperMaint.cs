using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Estimate Operation Entry Graph
    /// </summary>
    public class EstimateOperMaint : PXGraph<EstimateOperMaint>
    {
        public PXSave<AMEstimateOper> Save;
        public PXCancel<AMEstimateOper> Cancel;
        public PXFirst<AMEstimateOper> First;
        public PXPrevious<AMEstimateOper> Prev;
        public PXNext<AMEstimateOper> Next;
        public PXLast<AMEstimateOper> Last;

        public PXSelect<AMEstimateOper> EstimateOperationRecords;

        public PXSelect<AMEstimateOper, Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateOper.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateOper.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> EstimateOperRecordSelected;

        public PXSelect<AMEstimateItem, Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
            And<AMEstimateItem.revisionID, Equal<Current<AMEstimateOper.revisionID>>>>> EstimateRecordSelected;

        public PXSetup<AMEstimateSetup> EstimateOperSetup;

        [PXHidden]
        public PXSelect<AMEstimateReference,
            Where<AMEstimateReference.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateReference.revisionID, Equal<Current<AMEstimateOper.revisionID>>>>> EstimateReferenceRecord;

        [PXImport(typeof(AMEstimateOper))]
        public AMOrderedMatlSelect<AMEstimateOper, AMEstimateMatl, Where<AMEstimateMatl.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateMatl.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateMatl.operationID, Equal<Current<AMEstimateOper.operationID>>>>>,
                OrderBy<Asc<AMEstimateMatl.sortOrder, Asc<AMEstimateMatl.lineID>>>> EstimateOperMatlRecords;

        [PXImport(typeof(AMEstimateOper))]
        public PXSelect<AMEstimateTool, Where<AMEstimateTool.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateTool.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateTool.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> EstimateOperToolRecords;

        [PXImport(typeof(AMEstimateOper))]
        public PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateOvhd.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateOvhd.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> EstimateOperOvhdRecords;

        [PXHidden]
        public PXSelect<AMEstimateHistory, Where<AMEstimateHistory.estimateID, Equal<Current<AMEstimateItem.estimateID>>>,
            OrderBy<Desc<AMEstimateHistory.createdDateTime>>> EstimateHistoryRecords;

        [PXImport(typeof(AMEstimateOper))]
        public PXSelect<AMEstimateStep, Where<AMEstimateStep.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateStep.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateStep.operationID, Equal<Current<AMEstimateOper.operationID>>>>>,
            OrderBy<Asc<AMEstimateStep.sortOrder, Asc<AMEstimateStep.lineID>>>> EstimateOperStepRecords;

        [PXHidden]
        public PXSelect<AMEstimateOper, Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateOper.revisionID, Equal<Current<AMEstimateOper.revisionID>>,
                And<AMEstimateOper.operationID, Equal<Current<AMEstimateOper.operationID>>>>>> OutsideProcessingOperationSelected;

        public EstimateOperMaint()
        {
            var estimateSetup = EstimateOperSetup.Current;
            if (estimateSetup == null)
            {
                throw new EstimatingSetupNotEnteredException();
            }
        }

        public override bool CanClipboardCopyPaste()
        {
            return false;
        }

        protected virtual void AMEstimateOper_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            var amEstimateItem = EstimateRecordSelected.Current ?? EstimateRecordSelected.Select();
            if (amEstimateOper == null || amEstimateItem == null)
            {
                return;
            }

            var isEditable = EstimateStatus.IsEditable(amEstimateItem.EstimateStatus) && amEstimateItem.IsPrimary.GetValueOrDefault();
            EnableRecords(isEditable);

            #region Disable or Enable all cost fields
            PXUIFieldAttribute.SetEnabled<AMEstimateOper.fixedLaborCost>(EstimateOperationRecords.Cache, null,
                    amEstimateOper.FixedLaborOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.variableLaborCost>(EstimateOperationRecords.Cache, null,
                    amEstimateOper.VariableLaborOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.machineCost>(EstimateOperationRecords.Cache, null,
                     amEstimateOper.MachineOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.materialCost>(EstimateOperationRecords.Cache, null,
                    amEstimateOper.MaterialOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.toolCost>(EstimateOperationRecords.Cache, null,
                    amEstimateOper.ToolOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.fixedOverheadCost>(EstimateOperationRecords.Cache, null,
                    amEstimateOper.FixedOverheadOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.variableOverheadCost>(EstimateOperationRecords.Cache, null,
                    amEstimateOper.VariableOverheadOverride.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMEstimateOper.subcontractCost>(EstimateOperationRecords.Cache, null,
                    amEstimateOper.SubcontractOverride.GetValueOrDefault());

            #endregion

            var isOutsideProcess = amEstimateOper.OutsideProcess == true;
            PXUIFieldAttribute.SetEnabled<AMBomOper.dropShippedToVendor>(cache, e.Row, isOutsideProcess && isEditable);
            PXUIFieldAttribute.SetEnabled<AMBomOper.vendorID>(cache, e.Row, isOutsideProcess);
            PXUIFieldAttribute.SetEnabled<AMBomOper.vendorLocationID>(cache, e.Row, isOutsideProcess);

            if (EstimateOperationRecords.AllowUpdate)
            {
                return;
            }

            EstimateOperationRecords.AllowUpdate = true;
            PXUIFieldAttribute.SetEnabled(cache, amEstimateOper, false);
            PXUIFieldAttribute.SetEnabled<AMEstimateOper.estimateID>(cache, amEstimateOper, true);
            PXUIFieldAttribute.SetEnabled<AMEstimateOper.revisionID>(cache, amEstimateOper, true);
            PXUIFieldAttribute.SetEnabled<AMEstimateOper.operationID>(cache, amEstimateOper, true);
        }

        public virtual void EnableRecords(bool enableRecords)
        {
            EstimateOperationRecords.AllowUpdate = enableRecords;
            EstimateOperationRecords.AllowDelete = enableRecords;

            EstimateOperMatlRecords.AllowInsert = enableRecords;
            EstimateOperMatlRecords.AllowUpdate = enableRecords;
            EstimateOperMatlRecords.AllowDelete = enableRecords;

            EstimateOperToolRecords.AllowInsert = enableRecords;
            EstimateOperToolRecords.AllowUpdate = enableRecords;
            EstimateOperToolRecords.AllowDelete = enableRecords;

            EstimateOperOvhdRecords.AllowInsert = enableRecords;
            EstimateOperOvhdRecords.AllowUpdate = enableRecords;
            EstimateOperOvhdRecords.AllowDelete = enableRecords;
        }

        protected virtual void InsertHistory()
        {
            if (EstimateOperationRecords.Current == null)
            {
                return;
            }

            if (EstimateOperRecordSelected.Cache.IsDirty && EstimateOperRecordSelected.Current != null)
            {
                // Create Estimate Operation Level Modified History Event
                EstimateHistoryRecords.Insert(new AMEstimateHistory()
                {
                    EstimateID = EstimateOperRecordSelected.Current.EstimateID,
                    RevisionID = EstimateOperRecordSelected.Current.RevisionID,
                    Description = Messages.GetLocal(Messages.EstimateOperationLevelModified,
                        EstimateOperRecordSelected.Current.RevisionID.TrimIfNotNullEmpty())
                });
            }

            if (EstimateOperRecordSelected.Current != null
                && (EstimateOperMatlRecords.Cache.IsDirty || EstimateOperToolRecords.Cache.IsDirty || EstimateOperOvhdRecords.Cache.IsDirty))
            {
                // Create Estimate Operation Level Modified History Event
                EstimateHistoryRecords.Insert(new AMEstimateHistory()
                {
                    EstimateID = EstimateOperRecordSelected.Current.EstimateID,
                    RevisionID = EstimateOperRecordSelected.Current.RevisionID,
                    Description = Messages.GetLocal(Messages.EstimateDetailLevelModified,
                        EstimateOperRecordSelected.Current.RevisionID.TrimIfNotNullEmpty())
                });
            }
        }

        protected virtual void AMEstimateMatl_ItemClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateMatl = (AMEstimateMatl) e.Row;
            if (amEstimateMatl == null || !amEstimateMatl.IsNonInventory.GetValueOrDefault())
            {
                return;
            }

            INItemClass itemClass = PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>
                   >>.Select(this, amEstimateMatl.ItemClassID);

            if (itemClass != null)
            {
                amEstimateMatl.UOM = itemClass.BaseUnit;
            }

        }

        public virtual void PersistBase()
        {
            base.Persist();
        }

        private void PersistBaseWithLockRetry()
        {
            var retryCount = 1;
            for (var retry = 0; retry <= retryCount; retry++)
            {
                try
                {
                    PersistBase();
                    retry = retryCount;
                }
                catch (PX.Data.PXLockViolationException lve)
                {
                    var currOper = EstimateOperationRecords?.Current;

                    // Only retry on estimate history lock
                    if (currOper?.EstimateID == null || retry >= retryCount || lve.Table != typeof(AMEstimateHistory))
                    {
                        throw;
                    }

                    PXTrace.WriteWarning(Messages.GetLocal(Messages.CorrectingHistoryLineCountersForEstimate, currOper.EstimateID));
                    if (!EstimateGraphHelper.TryCorrectHistoryLineCounters(EstimateRecordSelected.Cache, EstimateHistoryRecords.Cache))
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                    throw;
                }
            }
        }

        public override void Persist()
        {
            InsertHistory();

            var estGraphHelper = new EstimateGraphHelper(this);
            if (estGraphHelper.PersistEstimateReference())
            {
                return;
            }

            PersistBaseWithLockRetry();
        }

        /// <summary>
        /// Get the current estimates reference record
        /// </summary>
        public virtual AMEstimateReference CurrentReference
        {
            get
            {
                return EstimateReferenceRecord.Current ??
                       (EstimateReferenceRecord.Current = EstimateReferenceRecord.Select());
            }
        }

        /// <summary>
        /// Get the current estimates reference only if it relates to the current estimate by revision
        /// </summary>
        public virtual AMEstimateReference CurrentRelatedEstimateReference
        {
            get
            {
                var relatedReference = CurrentReference;
                if (relatedReference != null
                    && EstimateRecordSelected.Current != null
                    && relatedReference.EstimateID.EqualsWithTrim(EstimateRecordSelected.Current.EstimateID)
                    && relatedReference.RevisionID.EqualsWithTrim(EstimateRecordSelected.Current.RevisionID))
                {
                    return relatedReference;
                }

                return null;
            }
        }

        protected void AMEstimateMatl_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var amEstimateMatl = (AMEstimateMatl) e.Row;
            if (amEstimateMatl == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMEstimateMatl.itemClassID>(EstimateOperMatlRecords.Cache, amEstimateMatl,
                    amEstimateMatl.IsNonInventory.GetValueOrDefault());

            PXUIFieldAttribute.SetEnabled<AMProdMatl.subcontractSource>(cache, e.Row, amEstimateMatl.MaterialType == AMMaterialType.Subcontract);
        }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Operation Nbr")]
        [PXSelector(typeof(Search<AMEstimateOper.operationID,
            Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
                And<AMEstimateOper.revisionID, Equal<Current<AMEstimateOper.revisionID>>>>>),
            typeof(AMEstimateOper.operationCD),
            typeof(AMEstimateOper.description),
            typeof(AMEstimateOper.workCenterID),
            DescriptionField = typeof(AMEstimateOper.description),
            SubstituteKey = typeof(AMEstimateOper.operationCD))]
        protected virtual void AMEstimateOper_OperationID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void AMEstimateOper_WorkCenterID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            AMWC amwc = PXSelect<AMWC, Where<AMWC.wcID, Equal<Required<AMWC.wcID>>>>.Select(this, amEstimateOper.WorkCenterID);

            if (amwc == null)
            {
                return;
            }

            cache.SetValueExt<AMEstimateOper.outsideProcess>(amEstimateOper, amwc.OutsideFlg.GetValueOrDefault());

            var machStdCost = 0m;

            foreach (PXResult<AMWCMach, AMMach> result in PXSelectJoin<AMWCMach,
                InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>>,
                Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>>>.Select(this, amwc.WcID))
            {
                var wcMachine = (AMWCMach)result;
                var machine = (AMMach)result;

                if (string.IsNullOrWhiteSpace(wcMachine.MachID) || string.IsNullOrWhiteSpace(machine.MachID) || !machine.ActiveFlg.GetValueOrDefault())
                {
                    continue;
                }
                machStdCost += wcMachine.MachineOverride.GetValueOrDefault() ? wcMachine.StdCost.GetValueOrDefault() : machine.StdCost.GetValueOrDefault();
            }
            amEstimateOper.MachineStdCost = machStdCost;

            AMEstimateItem amEstimateItem = PXSelect<AMEstimateItem,
                Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                    And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>
                    >>>.Select(this, amEstimateOper.EstimateID, amEstimateOper.RevisionID);

            // Delete the Overheads from old Work Center
            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, 
                Where<AMEstimateOvhd.estimateID, Equal<Required<AMEstimateItem.estimateID>>, 
                    And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateItem.revisionID>>, 
                    And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOvhd.operationID>>, 
                    And<AMEstimateOvhd.wCFlag, Equal<True>>>>
                    >>.Select(this, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                EstimateOperOvhdRecords.Delete(estimateOvhd);

                if (amEstimateItem == null)
                {
                    continue;
                }

                // Add the deleted overhead value from the Estimate fixed or Variable Overhead to prevent doubling up on EstimateItem
                if (estimateOvhd.OvhdType == OverheadType.FixedType)
                {
                    amEstimateItem.FixedOverheadCalcCost += estimateOvhd.FixedOvhdOperCost;
                }
                else
                {
                    amEstimateItem.VariableOverheadCalcCost += estimateOvhd.VariableOvhdOperCost;
                }
            }
            
            // Insert the Overheads from new Work Center
            foreach (PXResult<AMWCOvhd, AMOverhead> result in PXSelectJoin<AMWCOvhd,
                InnerJoin<AMOverhead, On<AMWCOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMWCOvhd.wcID, Equal<Required<AMWCOvhd.wcID>>>>.Select(this, amwc.WcID))
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

                if (amEstimateItem == null)
                {
                    continue;
                }

                // Subtract the Added overhead value from the Estimate fixed or Variable Overhead to prevent doubling up on EstimateItem
                if (newEstimateOvhd.OvhdType == OverheadType.FixedType)
                {
                    amEstimateItem.FixedOverheadCalcCost -= newEstimateOvhd.FixedOvhdOperCost;
                }
                else
                {
                    amEstimateItem.VariableOverheadCalcCost -= newEstimateOvhd.VariableOvhdOperCost;
                }
            }
        }

        protected virtual void AMEstimateMatl_UnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var amEstimateMatl = (AMEstimateMatl)e.Row;
            if (amEstimateMatl == null || amEstimateMatl.IsNonInventory != true)
            {
                return;
            }

            //Keeping unit cost when non inventory
            e.NewValue = amEstimateMatl.UnitCost;
            e.Cancel = true;
        }

        protected virtual void AMEstimateMatl_MaterialType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var estMatl = (AMEstimateMatl)e.Row;

            if (estMatl?.MaterialType == null)
            {
                return;
            }

            if (estMatl.MaterialType == AMMaterialType.Regular)
            {
                estMatl.SubcontractSource = AMSubcontractSource.None;
            }
            else
            {
                estMatl.SubcontractSource = AMSubcontractSource.Purchase;
            }
        }

        protected virtual void AMEstimateMatl_SubcontractSource_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var estMatl = (AMEstimateMatl)e.Row;
            var newContractSource = (int)e.NewValue;
            if (estMatl?.MaterialType == null)
            {
                return;
            }

            if (estMatl.MaterialType == AMMaterialType.Regular && newContractSource != AMSubcontractSource.None)
            {
                e.NewValue = AMSubcontractSource.None;
                e.Cancel = true;
            }
            else if (estMatl.MaterialType == AMMaterialType.Subcontract && newContractSource == AMSubcontractSource.None)
            {
                e.NewValue = estMatl.SubcontractSource;
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.FieldUpdated<AMEstimateOper, AMEstimateOper.outsideProcess> e)
        {
            var oldValue = Convert.ToBoolean(e.OldValue ?? false);
            if (!oldValue || e.Row?.OutsideProcess == true)
            {
                return;
            }

            e.Cache.SetValueExt<AMEstimateOper.dropShippedToVendor>(e.Row, false);
            e.Cache.SetValueExt<AMEstimateOper.vendorID>(e.Row, null);
            e.Cache.SetValueExt<AMEstimateOper.vendorLocationID>(e.Row, null);


            foreach (AMEstimateMatl matl in EstimateOperMatlRecords.Select())
            {
                EstimateOperMatlRecords.Cache.SetValueExt<AMEstimateMatl.materialType>(matl, AMMaterialType.Regular);
            }
        }
    }
}