using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    public class LaborEntry : MoveEntryBase<Where<AMBatch.docType, Equal<AMDocType.labor>>>
    {
        [PXHidden]
        public PXSelect<AMClockTran> clockTrans;

        public LaborEntry()
        {
            PXVerifySelectorAttribute.SetVerifyField<AMMTran.receiptNbr>(transactions.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMMTran.expireDate>(transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<AMMTran.lotSerialNbr>(transactions.Cache, null, false);
        }

        protected override void AMMTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null || sender.GetStatus(row) == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            //Only prompt when a non referenced batch
            if (batch.Current != null 
                && string.IsNullOrWhiteSpace(batch.Current.OrigBatNbr)
                && row.DocType == batch.Current.DocType && row.BatNbr == batch.Current.BatNbr
                && !_skipReleasedReferenceDocsCheck 
                && ReferenceDeleteGraph.HasReleasedReferenceDocs(this, row))
            {
                //Have the user confirm the delete when released references exist
                if (transactions.Ask(Messages.ConfirmDeleteTitle, Messages.GetLocal(Messages.ConfirmReleasedTransactionsExist), MessageButtons.YesNo) != WebDialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                PXTrace.WriteInformation($"AMMTran:{row.DocType}:{row.BatNbr}:{row.LineNbr} - {Messages.GetLocal(Messages.ConfirmReleasedTransactionsExist)} - Answered Yes");
            }
        }

        protected virtual void AMMTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
                return;

            //If labor came from Clock Entry, reopen the tran
            if(row.OrigDocType == AMDocType.Clock)
            {
                AMClockTran origTran = PXSelect<AMClockTran, Where<AMClockTran.lineNbr, Equal<Required<AMClockTran.lineNbr>>>>.Select(this, row.OrigLineNbr);
                if(origTran != null)
                {
                    origTran = (AMClockTran)clockTrans.Cache.CreateCopy(origTran);
                    origTran.Closeflg = false;
                    clockTrans.Cache.Update(origTran);
                }
            }
        }

        protected override void AMBatch_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Labor;
        }

        protected override void AMMTran_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Labor;
        }

        protected override void AMMTranSplit_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Labor;
        }


        protected override void AMMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.AMMTran_RowSelected(sender, e);
            LaborAMMTranRowSelected(sender, e);
        }

        protected override void AMMTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            LaborAMMTranRowPersisting(sender, e);
            base.AMMTran_RowPersisting(sender, e);
        }

        protected override void AMMTran_ProdOrdID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
            {
                return;
            }

            base.AMMTran_ProdOrdID_FieldUpdated(sender, e);
            SetDirectLaborCode((AMMTran)e.Row);

            AMProdItem amProdItem = PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(this, row.OrderType, row.ProdOrdID);
            if (amProdItem != null && amProdItem.Function == OrderTypeFunction.Disassemble)
            {
                sender.SetValue<AMMTran.qty>(row, 0m);
                sender.SetValue<AMMTran.qtyScrapped>(row, 0m);
            }
        }

        protected override void AMMTranAttribute_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMMTranAttribute)e.Row;
            if (row == null)
            {
                return;
            }
            
            var ammTran = GetParent(sender, row) ?? transactions.Current;
            if (ammTran == null || ammTran.Qty.GetValueOrDefault() == 0 && ammTran.QtyScrapped.GetValueOrDefault() == 0)
            {
                return;
            }

            base.AMMTranAttribute_RowPersisting(sender, e);
        }

        #region Cache Attached
      
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(TimeCardStatus.Unprocessed, PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void AMMTran_TimeCardStatus_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(0)]
        protected virtual void AMMTran_LaborTime_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [ProductionEmployeeSelector]
        [PXDefault(typeof(Search<EPEmployee.bAccountID,
                    Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
                    And<EPEmployeeExt.amProductionEmployee, Equal<True>,
                    And<Current<AMPSetup.defaultEmployee>, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        protected virtual void AMMTran_EmployeeID_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(4)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Shift")]
        [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
        protected virtual void AMMTran_ShiftID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(Search<AMProdOper.operationID,
            Where<AMProdOper.orderType, Equal<Current<AMMTran.orderType>>, 
                And<AMProdOper.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>,
            OrderBy<Asc<AMProdOper.operationCD>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void AMMTran_OperationID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(AMPSetup.defaultOrderType), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void AMMTran_OrderType_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void AMMTran_ProdOrdID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(Search<AMProdItem.inventoryID,
            Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Inventory(Enabled = false)]
        protected virtual void AMMTran_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(Search<AMProdItem.siteID,
            Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SiteAvail(typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID))]
        protected virtual void AMMTran_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(Search<AMProdItem.uOM, Where<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(AMMTran.inventoryID))]
        protected virtual void AMMTran_UOM_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "WIP Account")]
        protected virtual void AMMTran_WIPAcctID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "WIP Subaccount")]
        protected virtual void AMMTran_WIPSubID_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(15, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Labor Code", Enabled = false)] //default at first as false, graph changes later based on labor type
        [IndirectLabor]
        protected virtual void AMMTran_LaborCodeID_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(1, IsFixed = true)]
        [AMLaborType.List]
        [PXDefault(AMLaborType.Direct)]
        [PXUIField(DisplayName = "Labor Type")]
        protected virtual void AMMTran_LaborType_CacheAttached(PXCache sender)
        {
        }

        #endregion

        protected virtual void AMMTran_LaborCodeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;
            if (ammTran == null || string.IsNullOrWhiteSpace(ammTran.LaborType))
            {
                return;
            }

            //labor code select is based on indirect but direct entries auto fill in the labor code from the shift - no need to validate those
            if (ammTran.LaborType == AMLaborType.Direct)
            {
                e.Cancel = true;
            }
        }

        protected virtual void LaborAMMTranRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;

            if (ammTran == null)
            {
                return;
            }

            //Need the validation here for field required due to labor entry allowing both direct and indirect labor.

            if (ammTran.LaborTime.GetValueOrDefault() == 0)
            {
                sender.RaiseExceptionHandling<AMMTran.laborTime>(
                    ammTran,
                    ammTran.LaborTime,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMMTran.laborTime>(sender)),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.LaborCodeID))
            {
                sender.RaiseExceptionHandling<AMMTran.laborCodeID>(
                    ammTran,
                    ammTran.LaborCodeID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.laborCodeID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.LaborType == AMLaborType.Indirect)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ammTran.OrderType))
            {
                sender.RaiseExceptionHandling<AMMTran.orderType>(
                    ammTran,
                    ammTran.OrderType,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.orderType>(sender),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.ProdOrdID))
            {
                sender.RaiseExceptionHandling<AMMTran.prodOrdID>(
                    ammTran,
                    ammTran.ProdOrdID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.prodOrdID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.OperationID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.operationID>(
                    ammTran,
                    ammTran.OperationID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.operationID>(sender),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.ShiftID))
            {
                sender.RaiseExceptionHandling<AMMTran.shiftID>(
                    ammTran,
                    ammTran.ShiftID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.shiftID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.SiteID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.siteID>(
                    ammTran,
                    ammTran.SiteID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.siteID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.InventoryID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.inventoryID>(
                    ammTran,
                    ammTran.InventoryID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.inventoryID>(sender),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.UOM))
            {
                sender.RaiseExceptionHandling<AMMTran.uOM>(
                    ammTran,
                    ammTran.UOM,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.uOM>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.WIPAcctID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.wIPAcctID>(
                    ammTran,
                    ammTran.WIPAcctID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.wIPAcctID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.WIPSubID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.wIPSubID>(
                    ammTran,
                    ammTran.WIPSubID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.wIPSubID>(sender),
                        PXErrorLevel.Error));
            }
        }

        protected virtual void LaborAMMTranRowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;
            if (ammTran == null)
            {
                return;
            }

            if (ammTran.HasReference.GetValueOrDefault())
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMMTran.laborTime>(sender, e.Row, ammTran.StartTime == null && ammTran.EndTime == null);
        }

        #region Labor start/end time


        protected virtual void AMMTran_StartTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CalcLaborTime(cache, (AMMTran)e.Row);
        }

        /// <summary>
        /// Calculate the time between the user entered start/end times
        /// </summary>
        /// <param name="cache">cache object</param>
        /// <param name="ammTran">ammtran row</param>
        /// <returns>decimal of hours</returns>
        protected virtual int GetStartEndLaborTime(PXCache cache, AMMTran ammTran)
        {
            if (ammTran == null || ammTran.EndTime == null || ammTran.StartTime == null)
            {
                return 0;
            }
            TimeSpan? timeSpan;
            if (Common.Dates.StartBeforeEnd(ammTran.StartTime, ammTran.EndTime))
                timeSpan = ammTran.EndTime - ammTran.StartTime;
            else
                timeSpan = ammTran.EndTime.Value.AddDays(1) - ammTran.StartTime;

            return timeSpan == null ? 0 : Convert.ToInt32(timeSpan.Value.TotalMinutes);
        }

        /// <summary>
        /// Sets the Labor Hours field with the calculated start/end labor hours value
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="ammTran"></param>
        protected virtual void CalcLaborTime(PXCache cache, AMMTran ammTran)
        {
            if (ammTran == null)
            {
                return;
            }

            var newLaborHours = GetStartEndLaborTime(cache, ammTran);
            cache.SetValueExt<AMMTran.laborTime>(ammTran, newLaborHours);
        }

        protected virtual void AMMTran_EndTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CalcLaborTime(cache, (AMMTran)e.Row);
        }

        #endregion

        protected override void AMMTran_OperationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            base.AMMTran_OperationID_FieldUpdated(sender, e);
            SetDefaultShift(sender, (AMMTran) e.Row);
            SetLaborAmount(sender, (AMMTran)e.Row);
        }

        protected virtual void SetDefaultShift(PXCache sender, AMMTran row)
        {
            if (row?.OperationID == null || row.ProdOrdID == null || IsCopyPasteContext || IsImport || IsContractBasedAPI)
            {
                return;
            }

            PXResultset<AMShift> result = PXSelectJoin<AMShift,
                    InnerJoin<AMProdOper, On<AMShift.wcID, Equal<AMProdOper.wcID>>>,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                .Select(this, row.OrderType, row.ProdOrdID, row.OperationID);

            if (result == null || result.Count != 1)
            {
                return;
            }

            sender.SetValueExt<AMMTran.shiftID>(row, ((AMShift)result[0])?.ShiftID);
        }

        protected virtual void AMMTran_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetLaborAmount(cache, (AMMTran)e.Row);
        }

        protected virtual void AMMTran_ShiftID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetDirectLaborCode((AMMTran)e.Row);
            SetLaborAmount(cache, (AMMTran)e.Row);
        }
        
        public virtual void SetDirectLaborCode(AMMTran ammTran)
        {
            if (string.IsNullOrWhiteSpace(ammTran?.ProdOrdID)
                || ammTran.OperationID == null
                || string.IsNullOrWhiteSpace(ammTran.ShiftID)
                || ammTran.LaborType != AMLaborType.Direct)
            {
                return;
            }

            AMProdOper amProdOper = PXSelect<
                    AMProdOper,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                .Select(this, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID);

            AMShift amShift = PXSelect<
                AMShift, 
                Where<AMShift.wcID, Equal<Required<AMShift.wcID>>,
                    And<AMShift.shiftID, Equal<Required<AMShift.shiftID>>
                    >>>
                .Select(this, amProdOper.WcID, ammTran.ShiftID);

            if (amShift != null)
            {
                ammTran.LaborCodeID = amShift.LaborCodeID;
            }

            if (string.IsNullOrWhiteSpace(ammTran.LaborCodeID))
            {
                throw new PXException(Messages.NoLaborCodeForShift);
            }

        }

        protected virtual void AMMTran_OrderType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            AMMTran tran = (AMMTran)e.Row;
            if (tran == null)
            {
                return;
            }

            if (tran.LaborType == AMLaborType.Indirect)
            {
                e.Cancel = true;
            }
        }

        protected virtual void AMMTran_LaborType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var tran = (AMMTran)e.Row;
            if (tran == null)
            {
                return;
            }

            if (tran.LaborType == AMLaborType.Indirect)
            {               
                cache.SetValueExt<AMMTran.orderType>(e.Row, null);
                cache.SetValueExt<AMMTran.operationID>(e.Row, null);
                cache.SetValueExt<AMMTran.prodOrdID>(e.Row, null);
                cache.SetValueExt<AMMTran.subItemID>(e.Row, null);
                tran.Qty = (decimal)0.0;
                tran.QtyScrapped = (decimal)0.0;
                cache.SetValueExt<AMMTran.uOM>(e.Row, null);
                cache.SetValueExt<AMMTran.siteID>(e.Row, null);
                cache.SetValueExt<AMMTran.locationID>(e.Row, null);
                cache.SetValueExt<AMMTran.laborCodeID>(e.Row, null);
                cache.SetValueExt<AMMTran.inventoryID>(e.Row, null);
                return;
            }

            cache.SetDefaultExt<AMMTran.orderType>(e.Row);
            cache.SetValueExt<AMMTran.laborCodeID>(e.Row, null);
        }

       protected virtual void AMMTran_LaborTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetLaborAmount(cache, (AMMTran)e.Row);
        }

        /// <summary>
        /// Sets the transaction related labor amount fields
        /// </summary>
        protected virtual void SetLaborAmount(PXCache sender, AMMTran tran)
        {
            if (tran?.DocType == null)
            {
                return;
            }

            var laborRate = GetLaborRate(tran);
            if (laborRate == null)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"[{tran.BatNbr}:{tran.LineNbr}] Current LaborRate = {tran.LaborRate}; Calculated LaborRate = {laborRate}");
#endif
            if (tran.LaborRate != laborRate)
            {
                sender.SetValueExt<AMMTran.laborRate>(tran, laborRate);
            }

            var shift = (AMShiftMst)PXSelectorAttribute.Select<AMMTran.shiftID>(sender, tran);
            var shiftAddn = shift?.DiffType != null ? ShiftDiffType.GetShiftDifferentialCost(tran.LaborRate, shift.ShftDiff, shift.DiffType) : tran.LaborRate;
            var extCost = UomHelper.PriceCostRound(tran.LaborTime.GetValueOrDefault() * shiftAddn.GetValueOrDefault() / 60.0m);
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{tran.BatNbr}:{tran.LineNbr}] Current ExtCost = {tran.ExtCost}; Calculated ExtCost = {extCost}");
#endif
            if (tran.ExtCost != extCost)
            {
                sender.SetValueExt<AMMTran.extCost>(tran, extCost);
            }
        }

        /// <summary>
        /// Determine the correct labor rate for the transactions
        /// </summary>
        protected virtual decimal? GetLaborRate(AMMTran tran)
        {
            if (tran?.EmployeeID == null || tran.TranDate == null || tran.ShiftID == null)
            {
                return 0m;
            }

            if (ampsetup.Current.DfltLbrRate == LaborRateType.Standard && tran.LaborType == AMLaborType.Direct)
            {
                var amwc = (AMWC)PXSelectJoin<AMWC,
                    InnerJoin<AMProdOper, On<AMWC.wcID, Equal<AMProdOper.wcID>>>,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>
                >.SelectWindowed(this, 0, 1, tran.OrderType, tran.ProdOrdID, tran.OperationID);
                return amwc?.StdCost ?? 0m;
            }

            try
            {
                var empCostEngine = new AMEmployeeCostEngine(this);
                return empCostEngine.GetEmployeeHourlyRate(tran.ProjectID, tran.TaskID, tran.EmployeeID, tran.TranDate).GetValueOrDefault();
            }
            catch (Exception exception)
            {
                PXTraceHelper.PxTraceException(exception);
            }

            return null;
        }
    }
}