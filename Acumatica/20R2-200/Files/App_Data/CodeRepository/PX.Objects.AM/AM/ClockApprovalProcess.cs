using System;
using System.Collections;
using PX.Data;
using System.Collections.Generic;
using PX.Common;
using PX.Objects.AM.Attributes;
using System.Linq;

namespace PX.Objects.AM
{
    /// <summary>
    /// Close production orders process graph
    /// </summary>
    public class ClockApprovalProcess : PXGraph<ClockApprovalProcess>
    {
        public PXCancel<AMClockTran> Cancel;
        public PXSave<AMClockTran> Save;
        [PXFilterable]
        public PXProcessing<AMClockTran,
            Where2<Where<AMClockTran.employeeID, Equal<Current<ClockTranFilter.employeeID>>, Or<Current<ClockTranFilter.employeeID>, IsNull>>,
                And2<Where<AMClockTran.orderType, Equal<Current<ClockTranFilter.orderType>>, Or<Current<ClockTranFilter.orderType>, IsNull>>,
                    And2<Where<AMClockTran.prodOrdID, Equal<Current<ClockTranFilter.prodOrdID>>, Or<Current<ClockTranFilter.prodOrdID>, IsNull>>,
                        And<Where<AMClockTran.released, Equal<True>, And<AMClockTran.closeflg, Equal<False>>>>>>>> UnapprovedTrans;
        public PXSelect<AMClockTranSplit, Where<AMClockTranSplit.employeeID, Equal<Current<AMClockTran.employeeID>>,
            And<AMClockTranSplit.lineNbr, Equal<Current<AMClockTran.lineNbr>>>>> splits;
        public PXFilter<ClockTranFilter> Filter;
        public PXSetup<AMPSetup> ampsetup;
        public LSAMClockTran lsselect;

        public ClockApprovalProcess()
        {
            EnableFields(UnapprovedTrans.Cache);
            ClockTranFilter filter = Filter.Current;
            UnapprovedTrans.SetProcessDelegate(delegate (List<AMClockTran> list)
            {
                // Acuminator disable once PX1088 InvalidViewUsageInProcessingDelegate [Using new instance of process]
                CreateLaborBatch(list, true, filter);
            });

        }

        public PXAction<AMClockTran> delete;
        [PXUIField(DisplayName = "Delete")]
        [PXButton]
        public virtual IEnumerable Delete(PXAdapter adapter)
        {
            if (UnapprovedTrans.Current == null)
                return adapter.Get();

            UnapprovedTrans.Cache.Delete(UnapprovedTrans.Current);
            return adapter.Get();
        }

        protected virtual void EnableFields(PXCache cache)
        {
            PXUIFieldAttribute.SetEnabled(cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.laborTime>(cache, null, false);
        }

        public static void CreateLaborBatch(List<AMClockTran> list, bool isMassProcess, ClockTranFilter filter)
        {
            var failed = false;
            var graph = CreateInstance<LaborEntry>();
            graph.ampsetup.Current.RequireControlTotal = false;
            var batch = graph.batch.Insert();
            if (batch == null)
            {
                throw new PXException(Messages.UnableToCreateRelatedTransaction);
            }

            batch.TranDesc = Messages.GetLocal(Messages.ClockLine);
            batch.OrigDocType = AMDocType.Clock;
            batch.Hold = false;

            for (var i = 0; i < list.Count; i++)
            {
                var clock = list[i];
                try
                {
                    var newTran = graph.transactions.Insert();
                    if (newTran == null)
                    {
                        PXProcessing<AMClockTran>.SetError(i, Messages.UnableToCreateRelatedTransaction);
                        continue;
                    }
                    newTran.EmployeeID = clock.EmployeeID;
                    newTran.OrderType = clock.OrderType;
                    newTran.ProdOrdID = clock.ProdOrdID;
                    newTran.OperationID = clock.OperationID;
                    newTran.ShiftID = clock.ShiftID;
                    newTran.Qty = clock.Qty;
                    newTran.UOM = clock.UOM;
                    newTran.StartTime = clock.StartTime;
                    newTran.EndTime = clock.EndTime;
                    newTran.OrigDocType = AMDocType.Clock;
                    newTran.OrigBatNbr = clock.EmployeeID.ToString();
                    newTran.OrigLineNbr = clock.LineNbr;
                    newTran.TranDate = clock.TranDate;
                    graph.transactions.Update(newTran);
                    if (clock.LastOper == true)
                    {
                        var insertedSplits = graph.splits.Cache.Inserted.ToArray<AMMTranSplit>();
                        var splitList = PXSelect<AMClockTranSplit, Where<AMClockTranSplit.employeeID, Equal<Required<AMClockTranSplit.employeeID>>,
                            And<AMClockTranSplit.lineNbr, Equal<Required<AMClockTranSplit.lineNbr>>>>>.Select(graph, clock.EmployeeID, clock.LineNbr);
                        for (var j = 0; j < splitList.Count; j++)
                        {
                            var split = (AMClockTranSplit)splitList[j];
                            var newSplit = j < insertedSplits.Length ? insertedSplits[j] : graph.splits.Insert();
                            newSplit.LocationID = split.LocationID;
                            newSplit.LotSerialNbr = split.LotSerialNbr;
                            newSplit.ExpireDate = split.ExpireDate;
                            newSplit.Qty = split.Qty;
                            graph.splits.Update(newSplit);
                        }
                    }

                    PXProcessing<AMClockTran>.SetInfo(i, ActionsMessages.RecordProcessed);
                }
                catch (Exception ex)
                {
                    PXTrace.WriteError($"[BAccount ID = {clock.EmployeeID}; Line Nbr = {clock.LineNbr}] {ex.Message}");
                    PXProcessing<AMClockTran>.SetError(i, ex);
                    failed = true;
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }

            try
            {
                if (graph.transactions.Cache.Inserted.Count() > 0)
                {
                    graph.Persist();
                }

                AMDocumentRelease.ReleaseDoc(new List<AMBatch> { graph.batch.Current }, false);
            }
            catch (Exception e)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    PXProcessing<AMClockTran>.SetError(i, e);
                }
            }
        }

        /// <summary>
        /// Calculate the time between the user entered start/end times
        /// </summary>
        /// <param name="cache">cache object</param>
        /// <param name="tran">ammtran row</param>
        /// <returns>decimal of hours</returns>
        protected virtual int GetStartEndLaborTime(PXCache cache, AMClockTran tran)
        {
            if (tran == null || tran.EndTime == null || tran.StartTime == null)
            {
                return 0;
            }
            TimeSpan? timeSpan;
            if (Common.Dates.StartBeforeEnd(tran.StartTime, tran.EndTime))
                timeSpan = tran.EndTime - tran.StartTime;
            else
                timeSpan = tran.EndTime.Value.AddDays(1) - tran.StartTime;

            return timeSpan == null ? 0 : Convert.ToInt32(timeSpan.Value.TotalMinutes);
        }

        /// <summary>
        /// Sets the Labor Hours field with the calculated start/end labor hours value
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="tran"></param>
        protected virtual void CalcLaborTime(PXCache cache, AMClockTran tran)
        {
            if (tran == null)
            {
                return;
            }

            var newLaborHours = GetStartEndLaborTime(cache, tran);
            cache.SetValueExt<AMClockTran.laborTime>(tran, newLaborHours);

        }

        protected virtual void ClockTranFilter_OrderType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ClockTranFilter)e.Row;
            cache.SetValueExt<ClockTranFilter.prodOrdID>(row, null);
        }


        protected virtual void AMClockTran_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            AMClockTran row = (AMClockTran)e.Row;
            if (row == null)
                return;

            CalcLaborTime(cache, row);
        }

        protected virtual void AMClockTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMClockTran)e.Row;

            if (string.IsNullOrWhiteSpace(row?.ProdOrdID) || e.Operation == PXDBOperation.Delete)
            {
                return;
            }

            if (row.LaborTime.GetValueOrDefault() == 0 && row.Qty.GetValueOrDefault() == 0 )
            {
                sender.RaiseExceptionHandling<AMClockTran.qty>(
                    row,
                    row.Qty,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMClockTran.qty>(sender)),
                        PXErrorLevel.Error));
                sender.RaiseExceptionHandling<AMClockTran.laborTime>(
                    row,
                    row.LaborTime,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMClockTran.laborTime>(sender)),
                        PXErrorLevel.Error));
            }

        }

        public override void Persist()
        {
            var trans = UnapprovedTrans.Cache;
            var transplits = splits.Cache;

            using (var ts = new PXTransactionScope())
            {
                try
                {
                    base.Persist();
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                    throw;
                }

                ts.Complete();
            }
        }
    }

    [Serializable]
    [PXCacheName("Clock Filter")]
    public class ClockTranFilter : IBqlTable
    {
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        protected Int32? _EmployeeID;
        [PXInt]
        [ProductionEmployeeSelector]
        [PXUIField(DisplayName = "Employee ID")]
        public virtual Int32? EmployeeID
        {
            get
            {
                return this._EmployeeID;
            }
            set
            {
                this._EmployeeID = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
        [PXString(2, IsFixed = true, InputMask = ">aa")]
        [PXUIField(DisplayName = "Order Type")]
        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
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
        [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Production Nbr", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [ProductionOrderSelector(typeof(orderType), true)]
        [PXFormula(typeof(Validate<orderType>))]
        [PXRestrictor(typeof(Where<AMProdItem.hold, NotEqual<True>,
            And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>),
            Messages.ProdStatusInvalidForProcess, typeof(AMProdItem.orderType), typeof(AMProdItem.prodOrdID), typeof(AMProdItem.statusID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
    }
}