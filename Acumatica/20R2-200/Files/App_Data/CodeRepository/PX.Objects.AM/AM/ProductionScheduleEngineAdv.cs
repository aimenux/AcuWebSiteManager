using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Advanced Planning and Scheduling Engine
    /// </summary>
    public class ProductionScheduleEngineAdv : ProductionScheduleEngine
    {
#if DEBUG
        //public PXSelect<EPEmployee, Where<EPEmployeeExt.amProductionEmployee, Equal<True>>> ProductionEmployee;
        //public PXSelect<AMEmployeeWC> ProductionEmployeeWC;
        //public PXSelect<AMEmployeeSchd> EmployeeSchdRecs; 
#endif
        public virtual void ClearAll()
        {
            this.Clear(PXClearOption.ClearAll);
        }

        /// <summary>
        /// Adjust the list to order by a specific order for processing
        /// </summary>
        /// <param name="list">List of schedules to order</param>
        /// <returns>reordered list</returns>
        public virtual List<AMSchdItem> OrderScheduleBy(List<AMSchdItem> list)
        {
            return list?.OrderBy(x => x.SchPriority).ThenBy(x => x.EndDate).ToList();
        }

        /// <summary>
        /// Process the list of passed items
        /// </summary>
        /// <param name="schdItems"></param>
        public virtual void Process(List<AMSchdItem> schdItems)
        {
            foreach (var amSchdItem in schdItems)
            {
                ClearAll();
                Process(amSchdItem);
                Actions.PressSave();
            }
        }

        public DateTime NowDate => Common.Dates.Today;

        /// <summary>
        /// Round a give date to the next complete rounded block time
        /// </summary>
        /// <param name="date">Date time to round by block size</param>
        /// <param name="blockSize">schedule time block size</param>
        /// <param name="roundBackwards">indicates the process should round back (True), otherwise round forward (false)</param>
        /// <returns>Block rounded date time</returns>
        public static DateTime BlockRoundedDate(DateTime date, int blockSize, bool roundBackwards = false)
        {
            var remainder = Convert.ToInt32(date.TimeOfDay.TotalMinutes) % blockSize;
            var addMinutes = remainder == 0 ? 0 : (roundBackwards ? remainder * -1 : blockSize - remainder);
            return date.AddMinutes(addMinutes);
        }

        /// <summary>
        /// Make sure the AMProdItem row is in sync with some field that might allow edit at the schd item level
        /// </summary>
        /// <param name="schdItem"></param>
        public virtual void UpdateProdItem(AMSchdItem schdItem)
        {
            if (schdItem == null || string.IsNullOrWhiteSpace(schdItem.ProdOrdID))
            {
                return;
            }

            AMProdItem amProdItem = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(this, schdItem.OrderType, schdItem.ProdOrdID);

            if (amProdItem == null || ProductionStatus.IsClosedOrCanceled(amProdItem))
            {
                //possible for MRP order types
                return;
            }

            bool update = false;
            if (Common.Dates.Compare(amProdItem.ConstDate, schdItem.ConstDate) != 0
                && !Common.Dates.IsDefaultDate(schdItem.ConstDate))
            {
                update = true;
                amProdItem.ConstDate = schdItem.ConstDate;
            }

            if (!amProdItem.SchedulingMethod.EqualsWithTrim(schdItem.SchedulingMethod))
            {
                update = true;
                amProdItem.SchedulingMethod = schdItem.SchedulingMethod;
            }

            if (amProdItem.SchPriority.GetValueOrDefault() != schdItem.SchPriority.GetValueOrDefault())
            {
                update = true;
                amProdItem.SchPriority = schdItem.SchPriority.GetValueOrDefault();
            }

            if (amProdItem.FirmSchedule.GetValueOrDefault() != schdItem.FirmSchedule.GetValueOrDefault())
            {
                update = true;
                amProdItem.FirmSchedule = schdItem.FirmSchedule.GetValueOrDefault();
            }

            if (amProdItem.StartDate.GetValueOrDefault().CompareTo(schdItem.StartDate.GetValueOrDefault()) != 0
                || amProdItem.EndDate.GetValueOrDefault().CompareTo(schdItem.EndDate.GetValueOrDefault()) != 0)
            {
                update = true;
#if DEBUG
                AMDebug.TraceWriteMethodName(string.Format("Update order {4} dates: start from '{0}' to '{1}'; end from '{2}' to '{3}'"
                    , amProdItem.StartDate.GetValueOrDefault().ToShortDateString(), schdItem.StartDate.GetValueOrDefault().ToShortDateString()
                    , amProdItem.EndDate.GetValueOrDefault().ToShortDateString(), schdItem.EndDate.GetValueOrDefault().ToShortDateString()
                    , amProdItem.ProdOrdID.TrimIfNotNullEmpty()));
#endif
                amProdItem.StartDate = schdItem.StartDate;
                amProdItem.EndDate = schdItem.EndDate;
            }

            if (update)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName(string.Format("Updating production order '{0}' '{1}' from Schd Item",
                    schdItem.OrderType.TrimIfNotNullEmpty(), amProdItem.ProdOrdID.TrimIfNotNullEmpty()));
#endif
                ProdItems.Update(amProdItem);
            }
        }

        private void UpdateProdOper(AMSchdOper schdOper)
        {
            if (schdOper == null || string.IsNullOrWhiteSpace(schdOper.ProdOrdID))
            {
                return;
            }

            AMProdOper prodOper = PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                        And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>
                        >.Select(this, schdOper.OrderType, schdOper.ProdOrdID, schdOper.OperationID);

            UpdateProdOper(schdOper, prodOper);
        }

        private void UpdateProdOper(AMSchdOper schdOper, AMProdOper prodOper)
        {
            if (schdOper == null || string.IsNullOrWhiteSpace(schdOper.ProdOrdID)
                || prodOper == null || string.IsNullOrWhiteSpace(prodOper.ProdOrdID))
            {
                return;
            }

            prodOper.StartDate = schdOper.StartDate;
            prodOper.EndDate = schdOper.EndDate;
            ProdOpers.Update(prodOper);
        }

        public virtual void Process(AMSchdItem schdItem)
        {
            if (!Features.AdvancedPlanningEnabled())
            {
                throw new PXException(Messages.UnableToProcess, this.GetType().Name);
            }

            if (schdItem == null)
            {
                throw new PXArgumentException(nameof(schdItem));
            }

            var finiteDateWindow = NowDate.AddDays(TempBlockWindow);
            var planDate = schdItem.ConstDate ?? Common.Current.BusinessDate(this);
            if (Common.Dates.Compare(planDate, NowDate) < 0)
            {
                planDate = NowDate;
                if (!schdItem.SchedulingMethod.EqualsWithTrim(ScheduleMethod.StartOn))
                {
                    schdItem.SchedulingMethod = ScheduleMethod.StartOn;
                }
            }

            if (!schdItem.SchedulingMethod.EqualsWithTrim(ScheduleMethod.StartOn))
            {
                //handles both user dates and old ASAP values if found
                schdItem.SchedulingMethod = ScheduleMethod.FinishOn;
            }

            var scheduleBack = schdItem.SchedulingMethod.EqualsWithTrim(ScheduleMethod.FinishOn);

            PXSelectBase<AMSchdOper> cmd = new PXSelectJoin<AMSchdOper,
                InnerJoin<AMWC, On<AMSchdOper.wcID, Equal<AMWC.wcID>>,
                //Planned schedules might not have a prod oper
                LeftJoin<AMProdOper, On<AMSchdOper.orderType, Equal<AMProdOper.orderType>,
                    And<AMSchdOper.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMSchdOper.operationID, Equal<AMProdOper.operationID>>>>>>,
                Where<AMSchdOper.orderType, Equal<Required<AMSchdOper.orderType>>,
                    And<AMSchdOper.prodOrdID, Equal<Required<AMSchdOper.prodOrdID>>,
                    And<AMSchdOper.schdID, Equal<Required<AMSchdOper.schdID>>>>>,
                OrderBy<Asc<AMSchdOper.lineNbr, Asc<AMSchdOper.sortOrder>>>>(this);

            if (scheduleBack)
            {
                cmd.OrderByNew<OrderBy<Desc<AMSchdOper.lineNbr, Desc<AMSchdOper.sortOrder>>>>();
            }

            schdItem.StartDate = planDate;
            schdItem.EndDate = planDate;

            AMProdItem prodItem = PXSelect<
                AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>
                .Select(this, schdItem.OrderType, schdItem.ProdOrdID);

            if (prodItem != null)
            {
                ProdItems.Current = prodItem;
            }

            var resultset = cmd.Select(schdItem.OrderType, schdItem.ProdOrdID, schdItem.SchdID);

            AMSchdItem schdItem2 = null;
            //Try to schedule until a non expected exception is received
            var retryCount = 150; // need to figure out the best number or retry attempt logic
            for (var retry = 0; retry <= retryCount; retry++)
            {
                var originalDateTime = planDate;
                try
                {
                    // note that this assumes the plandate is the start date...
                    //      Start date less/equal finiteWindow will plan as finite but start date after is infinite
                    if (Common.Dates.Compare(planDate, finiteDateWindow) <= 0)
                    {
#if DEBUG
                        AMDebug.TraceWriteMethodName(string.Format("FINITE: Scheduling order '{0}' {2} with date '{1}' [Block Setup Size: {3}]", 
                            schdItem.ProdOrdID.TrimIfNotNullEmpty(), planDate.ToShortDateString(), scheduleBack ? "backward" : "forward", BlockSize));
                        //Here we need to retry finding the right dates to work with
                        //  There might be scenarios where we need to adjust the schedule process to start on (as it was previously finish on but the calculated dates brought the start before today)
                        //  If the planDate is >= to the finite window then we plan it as infinite... 
#endif
                        schdItem2 = ProcessSchdItem(schdItem, resultset, scheduleBack, ref planDate);
                    }
                    else
                    {
#if DEBUG
                        AMDebug.TraceWriteMethodName(
                            $"INFINITE: Scheduling order '{schdItem.ProdOrdID.TrimIfNotNullEmpty()}' with date '{planDate.ToShortDateString()}'");
#endif
                        //Schedule as infinite...
                    }

                    retry = retryCount;
                }
                catch (SchedulingOrderDatesException se)
                {
#if DEBUG
                    AMDebug.TraceWriteMethodName($"CAUGHT SchedulingOrderDatesException: {se.Message}");
                    if (retry >= retryCount)
                    {
                        AMDebug.TraceWriteLine("Max Retry Reached");
                        AMDebug.TraceException(se);
                        throw;
                    }
                    AMDebug.TraceWriteMethodName(
                        $"SchedulingOrderDatesException: Starting retry using date time '{se.NextAttemptDateTime.ToShortDateString()} {se.NextAttemptDateTime.ToLongTimeString()}' in place of previous date time '{originalDateTime.ToShortDateString()}  {originalDateTime.ToLongTimeString()}'");
                    
                    //Next attempt might be backward but most likely forward?
#endif
                    planDate = se.NextAttemptDateTime;

                    if (Common.Dates.Compare(planDate, NowDate) < 0)
                    {
                        planDate = NowDate;
                        if (schdItem.SchedulingMethod.EqualsWithTrim(ScheduleMethod.FinishOn))
                        {
                            schdItem.ConstDate = planDate;
                            schdItem.SchedulingMethod = ScheduleMethod.StartOn;
                        }
                    }
                    scheduleBack = schdItem.SchedulingMethod.EqualsWithTrim(ScheduleMethod.FinishOn);

                    //NEED TO CLEAR OUT THE CACHE TO START OVER...
                    if (IsDirty)
                    {
                        ClearAll();
                    }
                }
                catch(Exception e)
                {
                    AMDebug.TraceException(e);
                    throw;
                }
            }

            if(schdItem2 != null)
            {
                UpdateProdItem(SchdItems.Update(schdItem2));
            }
        }

        private AMSchdItem ProcessSchdItem(AMSchdItem schdItem, PXResultset<AMSchdOper> orderSchdResults, bool scheduleBack, ref DateTime currentSchedDate)
        {
            schdItem.StartDate = null;
            schdItem.EndDate = null;
            var checkMaterial = CurrentOrderType?.CheckSchdMatlAvailability ?? true;
            foreach (PXResult<AMSchdOper, AMWC, AMProdOper> result in orderSchdResults)
            {
                var schdOperResult = (AMSchdOper) result;
                var workCenter = (AMWC) result;
                var prodOper = (AMProdOper) result;

                if (schdOperResult?.OperationID == null || workCenter == null || string.IsNullOrWhiteSpace(workCenter.WcID))
                {
                    continue;
                }

                var schdOper = ProcessOperation(schdItem, schdOperResult, prodOper, workCenter, scheduleBack ? ReadDirection.Backward : ReadDirection.Forward, ref currentSchedDate);
                if (schdOper == null)
                {
                    continue;
                }

                if (schdOper.StartDate.GetValueOrDefault(Common.Dates.UtcToday.AddDays(-1)).Date.CompareTo(Common.Dates.UtcToday.Date) < 0)
                {
                    throw new SchedulingOrderDatesException(schdOper.StartDate.GetValueOrDefault(Common.Dates.UtcToday.AddDays(-1)).Date, Common.Dates.UtcToday.AddDays(-1), Messages.StartDateBeforeToday);
                }

                if (currentSchedDate.Date.CompareTo(Common.Dates.UtcToday.Date) < 0)
                {
                    throw new SchedulingOrderDatesException(currentSchedDate.Date, Common.Dates.UtcToday.AddDays(-1), Messages.StartDateBeforeToday);
                }

                if (checkMaterial)
                {
                    ProcessSchdOperMaterial(schdOper, prodOper, schdOper.StartDate.GetValueOrDefault());
                }
                
                if (schdItem.StartDate == null || schdOper.StartDate < schdItem.StartDate)
                {
                    schdItem.StartDate = schdOper.StartDate;
                }
                if (schdItem.EndDate == null || schdOper.EndDate > schdItem.EndDate)
                {
                    schdItem.EndDate = schdOper.EndDate;
                }
            }

            if (schdItem.StartDate == null) { schdItem.StartDate = currentSchedDate; }
            if (schdItem.EndDate == null) { schdItem.EndDate = currentSchedDate; }

            return schdItem;
        }

        private void ProcessSchdOperMaterial(AMSchdOper schdOper, AMProdOper prodOper, DateTime scheduleDate)
        {
            var materialAvailabilityList = MaterialAvailability(this, scheduleDate, schdOper.OrderType, schdOper.ProdOrdID, schdOper.OperationID);

            if (materialAvailabilityList == null || materialAvailabilityList.Count == 0)
            {
                //No material to schedule
                return;
            }

            var shortageList = new List<MaterialAvailabiltyDates>();
            var availableAtLaterDateList = new List<MaterialAvailabiltyDates>();

            foreach (var materialAvailability in materialAvailabilityList)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"[{materialAvailability.ProdMatl.OrderType}:{materialAvailability.ProdMatl.ProdOrdID.TrimIfNotNullEmpty()}:{prodOper?.OperationCD}({materialAvailability.ProdMatl.OperationID}):{materialAvailability.ProdMatl.LineNbr}] InventoryID = ({materialAvailability.ProdMatl.InventoryID}) {materialAvailability.MatlInventoryItem.InventoryCD.TrimIfNotNullEmpty()}; SiteID = {materialAvailability.ProdMatl.SiteID} TotalQtyRequired = {materialAvailability.ProdMatl.TotalQtyRequired}; AvailableDate = {materialAvailability.AvailableDate}; FirstLateAvailableDate = {materialAvailability.FirstLateAvailableDate}; AvailableBaseQtyShort = {materialAvailability.AvailableBaseQtyShort}; FirstLateAvailableBaseQtyShort = {materialAvailability.FirstLateAvailableBaseQtyShort}");
#endif
                if (materialAvailability.NoAvailableMaterial)
                {
                    //Nothing available for the material item
                    shortageList.Add(materialAvailability);
                    continue;
                }

                if (materialAvailability.ShortByScheduleDate)
                {
                    availableAtLaterDateList.Add(materialAvailability);
                    continue;
                }

                if (shortageList.Count == 0 && availableAtLaterDateList.Count == 0)
                {
                    UpdateMaterialSplit(materialAvailability.ProdMatl, materialAvailability.AllProdMatlSplits, scheduleDate);
                    AllocateMaterialQty(materialAvailability.ProdMatl, materialAvailability.AllocatingProdMatlSplits);
                }
            }

            var exceptionIsFullShortage = shortageList.Count > 0;

            if (!exceptionIsFullShortage && availableAtLaterDateList.Count == 0)
            {
                //Material checks out - good to schedule for date
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var materialShortage in shortageList)
            {
                sb.AppendLine(
                    Messages.GetLocal(Messages.ScheduleMaterialShortage,
                    materialShortage.MatlInventoryItem.InventoryCD.TrimIfNotNullEmpty(),
                        materialShortage.ProdMatl.OrderType,
                        materialShortage.ProdMatl.ProdOrdID,
                        prodOper?.OperationCD,
                        scheduleDate));
            }

            DateTime? bestAvailableDate = null;
            foreach (var materialavailableAtLaterDate in availableAtLaterDateList)
            {
                bestAvailableDate =
                    bestAvailableDate.GreaterDateTime(materialavailableAtLaterDate.FirstLateAvailableDate);

                sb.AppendLine(
                    Messages.GetLocal(Messages.ScheduleMaterialShortUntilDate,
                        materialavailableAtLaterDate.MatlInventoryItem.InventoryCD.TrimIfNotNullEmpty(),
                        materialavailableAtLaterDate.ProdMatl.OrderType,
                        materialavailableAtLaterDate.ProdMatl.ProdOrdID,
                        prodOper?.OperationCD,
                        scheduleDate,
                        materialavailableAtLaterDate.FirstLateAvailableDate));
            }

            if (exceptionIsFullShortage || bestAvailableDate == null)
            {
                throw new MaterialScheduleShortageException(sb.ToString());
            }
            throw new SchedulingOrderDatesException(scheduleDate, bestAvailableDate.GetValueOrDefault(), sb.ToString());
        }

        private void AllocateMaterialQty(AMProdMatl prodMatl, List<AMProdMatlSplit> splits)
        {
            if (splits == null || splits.Count == 0)
            {
                return;
            }

            var locatedProdMatl = ProdMatls.Cache.LocateElse(prodMatl);
            if (locatedProdMatl == null)
            {
                return;
            }
            ProdMatls.Current = locatedProdMatl;
            foreach (var split in splits)
            {
                var locatedProdMatlSplit = ProdMatlSplits.Cache.LocateElse(split);
                if (locatedProdMatlSplit == null || locatedProdMatlSplit.IsAllocated.GetValueOrDefault())
                {
                    continue;
                }
                ProdMatlSplits.Current = locatedProdMatlSplit;

#if DEBUG
                AMDebug.TraceWriteMethodName($"[{split.OrderType}:{split.ProdOrdID.TrimIfNotNullEmpty()}:({split.OperationID}):{split.LineID}:{split.SplitLineNbr}] InventoryID = {split.InventoryID}; SiteID = {split.SiteID}");
#endif
                locatedProdMatlSplit.IsAllocated = true;
                ProdMatlSplits.Update(locatedProdMatlSplit);
            }
        }

        private void UpdateMaterialSplit(AMProdMatl prodMatl, List<AMProdMatlSplit> splits, DateTime scheduleDate)
        {
            if (splits == null || splits.Count == 0)
            {
                return;
            }

            var locatedProdMatl = ProdMatls.Cache.LocateElse(prodMatl);
            if (locatedProdMatl == null)
            {
                return;
            }

            locatedProdMatl.TranDate = scheduleDate;
            ProdMatls.Current = ProdMatls.Update(locatedProdMatl) ?? locatedProdMatl;
            foreach (var split in splits)
            {
                var locatedProdMatlSplit = ProdMatlSplits.Cache.LocateElse(split);
                if (locatedProdMatlSplit == null)
                {
                    continue;
                }
                ProdMatlSplits.Current = locatedProdMatlSplit;
                locatedProdMatlSplit.TranDate = scheduleDate;
                ProdMatlSplits.Update(locatedProdMatlSplit);
            }
        }

//        [Obsolete]
//        private AMSchdOper ProcessOperation(AMSchdItem schdItem, AMSchdOper schdOper, AMProdOper prodOper, 
//            WorkCenterCalendarHelper workCenterCalendar, ReadDirection scheduleDirection,
//            ref DateTime currentSchedDateTime)
//        {
//            //make sure some of the params align
//            if (string.IsNullOrWhiteSpace(schdItem?.ProdOrdID))
//            {
//                throw new PXArgumentException("schdItem missing from process operation");
//            }

//            if (string.IsNullOrWhiteSpace(schdOper?.ProdOrdID))
//            {
//                throw new PXArgumentException("schdOper missing from process operation");
//            }

//            if (workCenterCalendar?.WorkCenter == null || string.IsNullOrWhiteSpace(workCenterCalendar.WorkCenter.WcID))
//            {
//                throw new PXArgumentException("workCenterCalendar missing from process operation");
//            }

//#if DEBUG
//            AMDebug.TraceWriteLine($"----- START Operation {schdOper.OperationID} -------------------------------------------------------------");
//#endif

//            //DELETE CURRENT SCHEDULE IF ONE EXISTS TO "RE-SCHEDULE"
//            DeleteSchdOperationDetail(schdOper);

//            schdOper.SchedulingMethod = schdItem.SchedulingMethod;
//            schdOper.ConstDate = currentSchedDateTime.Date;
//            schdOper.StartDate = null;
//            schdOper.EndDate = null;

//            if (prodOper?.OperationID != null)
//            {
//                var totalTime = CalcTotalTime(workCenterCalendar?.WorkCenter, prodOper, schdOper.QtytoProd.GetValueOrDefault());
//#if DEBUG
//                //TODO: what is the best way to include/exclude setup time when calculation partial remaining time?
//#endif
//                totalTime += prodOper.SetupTime.GetValueOrDefault();

//                if (totalTime < 0)
//                {
//                    totalTime = 0;
//                }

//                if (schdOper.QtytoProd.GetValueOrDefault() != 0)
//                {
//                    var rateWithSetup = totalTime / schdOper.QtytoProd.GetValueOrDefault();
//                    totalTime = Convert.ToInt32(rateWithSetup * schdOper.QtyRemaining.GetValueOrDefault());
//                }

//                schdOper.TotalPlanTime = totalTime;
//            }

//            var remainingOperationSchdTime = schdOper.TotalPlanTime.GetValueOrDefault();
//#if DEBUG
//            AMDebug.TraceWriteMethodName(
//                $"Order '{schdOper.ProdOrdID.TrimIfNotNullEmpty()}' Operation '{schdOper.OperationID}' at work center '{schdOper.WcID.TrimIfNotNullEmpty()}' contains '{remainingOperationSchdTime}' plan schd time [Schd Blocks: {MinutesToBlocks(remainingOperationSchdTime, true)}]");
//#endif

//            //Handle operation with no time.
//            if (remainingOperationSchdTime <= 0)
//            {
//                schdOper.StartDate = currentSchedDateTime.Date;
//                schdOper.EndDate = currentSchedDateTime.Date;
//                return schdOper;
//            }

//            var cntr = 0;
//            while (remainingOperationSchdTime > 0)
//            {
//                cntr++;
//#if DEBUG
//                if (cntr != 0 && cntr % 20 == 0)
//                {
//                    AMDebug.TraceWriteMethodName($"loop {cntr}");
//                }
//#endif

//                // get the next work center work day
//                var nextWorkCenterWorkDay = workCenterCalendar.GetNextWorkDay(currentSchedDateTime, scheduleDirection, true);
//                if (nextWorkCenterWorkDay == null)
//                {
//                    throw new PXException(Messages.NoWorkDaysForCalendarWorkCenter, workCenterCalendar.WorkCenter.WcID);
//                }
//                if(nextWorkCenterWorkDay.GetValueOrDefault().Date.CompareTo(currentSchedDateTime.Date) != 0)
//                {
//                    currentSchedDateTime = nextWorkCenterWorkDay.GetValueOrDefault();
//                }

//                var wcDateInfo = workCenterCalendar.GetWorkingHours(nextWorkCenterWorkDay.GetValueOrDefault());

//#if DEBUG
//                AMDebug.TraceWriteMethodName($"Current date time: {currentSchedDateTime}");
//#endif
//                //Loop shifts (in the correct order)
//                foreach (var amShift in workCenterCalendar.GetShifts(scheduleDirection))
//                {
//                    ProcessOperationShift(schdOper, amShift, wcDateInfo.ShiftDateInfos[amShift.ShiftID],
//                        workCenterCalendar, scheduleDirection, ref currentSchedDateTime, ref remainingOperationSchdTime);
//                }

//                if (schdOper.StartDate == null || currentSchedDateTime.Date < schdOper.StartDate)
//                {
//                    schdOper.StartDate = currentSchedDateTime.Date;
//                }
//                if (schdOper.EndDate == null || currentSchedDateTime.Date > schdOper.EndDate)
//                {
//                    schdOper.EndDate = currentSchedDateTime.Date;
//                }

//                // right here we need to check the start/end of the combined wc date with the current datetime and see if we need to move to the "next date"
//                if(IsCurrentDateUsedUp(wcDateInfo.WorkCenterDateInfo, currentSchedDateTime, scheduleDirection))
//                {
//                    currentSchedDateTime = currentSchedDateTime.AddDays(AddDays(scheduleDirection)).Date;
//                }
                
//                if (cntr >= 1234)
//                {
//#if DEBUG
//                    AMDebug.TraceWriteMethodName("**************************************");
//                    AMDebug.TraceWriteMethodName($"loop is maxed out {cntr}");
//                    AMDebug.TraceWriteMethodName("**************************************");
//#endif
//                    break;
//                }
//            }

//#if DEBUG
//            AMDebug.TraceWriteLine(
//                $"----- END Operation {schdOper.OperationID} -------------------------------------------------------------");
//#endif
//            if (string.IsNullOrWhiteSpace(prodOper?.ProdOrdID))
//            {
//                UpdateProdOper(schdOper);
//            }
//            else
//            {
//                UpdateProdOper(schdOper, prodOper);
//            }
            
//            return SchdOpers.Update(schdOper);
//        }

        private AMSchdOper ProcessOperation(AMSchdItem schdItem, AMSchdOper schdOper, AMProdOper prodOper,
            AMWC workCenter, ReadDirection scheduleDirection,
            ref DateTime currentSchedDateTime)
        {
            //make sure some of the params align
            if (string.IsNullOrWhiteSpace(schdItem?.ProdOrdID))
            {
                throw new PXArgumentException("schdItem missing from process operation");
            }

            if (string.IsNullOrWhiteSpace(schdOper?.ProdOrdID))
            {
                throw new PXArgumentException("schdOper missing from process operation");
            }

            if (workCenter?.WcID == null)
            {
                throw new PXArgumentException("workCenter missing from process operation");
            }

#if DEBUG
            AMDebug.TraceWriteLine($"----- START Operation {schdOper.OperationID} -------------------------------------------------------------");
#endif

            //DELETE CURRENT SCHEDULE IF ONE EXISTS TO "RE-SCHEDULE"
            DeleteSchdOperationDetail(schdOper);

            schdOper.SchedulingMethod = schdItem.SchedulingMethod;
            schdOper.ConstDate = currentSchedDateTime.Date;
            schdOper.StartDate = null;
            schdOper.EndDate = null;

            var isMachineScheduleOper = IsMachineScheduling(workCenter);

            var workCenterCalendar = new WorkCenterCalendarHelper(this, workCenter, true);

            if (prodOper?.OperationID != null)
            {
                var totalTime = CalcTotalTime(workCenter, prodOper, schdOper.QtytoProd.GetValueOrDefault());
#if DEBUG
                //TODO: what is the best way to include/exclude setup time when calculation partial remaining time?
#endif
                totalTime += prodOper.SetupTime.GetValueOrDefault();

                if (totalTime < 0)
                {
                    totalTime = 0;
                }

                if (schdOper.QtytoProd.GetValueOrDefault() != 0)
                {
                    var rateWithSetup = totalTime / schdOper.QtytoProd.GetValueOrDefault();
                    totalTime = Convert.ToInt32(rateWithSetup * schdOper.QtyRemaining.GetValueOrDefault());
                }

                schdOper.TotalPlanTime = totalTime;
            }

            var remainingOperationSchdTime = schdOper.TotalPlanTime.GetValueOrDefault();
#if DEBUG
            AMDebug.TraceWriteMethodName(
                $"Order '{schdOper.ProdOrdID.TrimIfNotNullEmpty()}' Operation '{schdOper.OperationID}' at work center '{schdOper.WcID.TrimIfNotNullEmpty()}' contains '{remainingOperationSchdTime}' plan schd time [Schd Blocks: {MinutesToBlocks(remainingOperationSchdTime, true)}]");
#endif

            //Handle operation with no time.
            if (remainingOperationSchdTime <= 0)
            {
                schdOper.StartDate = currentSchedDateTime.Date;
                schdOper.EndDate = currentSchedDateTime.Date;
                return schdOper;
            }

            List<MachineCalendarHelper> machineCalendars = null;
            var runAsMachineSchedule = isMachineScheduleOper;
            if (runAsMachineSchedule)
            {
                machineCalendars = GetActiveMachineCalendars(workCenter, workCenterCalendar.GetFirstShift(), scheduleDirection).ToList();
                runAsMachineSchedule = machineCalendars.Count > 0;
            }

            var cntr = 0;
            while (remainingOperationSchdTime > 0)
            {
                cntr++;
#if DEBUG
                if (cntr != 0 && cntr % 20 == 0)
                {
                    AMDebug.TraceWriteMethodName($"loop {cntr}");
                }
#endif
                if (runAsMachineSchedule)
                {
                    ProcessOperationMachines(machineCalendars, scheduleDirection, ref schdOper, ref currentSchedDateTime, ref remainingOperationSchdTime);
                }
                else
                {
                    ProcessWorkCenterSchedule(workCenterCalendar, scheduleDirection, ref schdOper, ref currentSchedDateTime, ref remainingOperationSchdTime);
                }

                //Safety net in case nothing scheduled
                if (cntr >= 1234)
                {
#if DEBUG
                    AMDebug.TraceWriteMethodName("**************************************");
                    AMDebug.TraceWriteMethodName($"loop is maxed out {cntr}");
                    AMDebug.TraceWriteMethodName("**************************************");
#endif
                    break;
                }
            }

#if DEBUG
            AMDebug.TraceWriteLine(
                $"----- END Operation {schdOper.OperationID} -------------------------------------------------------------");
#endif
            if (string.IsNullOrWhiteSpace(prodOper?.ProdOrdID))
            {
                UpdateProdOper(schdOper);
            }
            else
            {
                UpdateProdOper(schdOper, prodOper);
            }

            return SchdOpers.Update(schdOper);
        }

        private void ProcessWorkCenterSchedule(WorkCenterCalendarHelper workCenterCalendar, ReadDirection scheduleDirection, ref AMSchdOper schdOper, ref DateTime currentSchedDateTime, ref int remainingSchdTime)
        {
            if (workCenterCalendar == null)
            {
                return;
            }

            // get the next work center work day
            var nextWorkCenterWorkDay = workCenterCalendar.GetNextWorkDay(currentSchedDateTime, scheduleDirection, true);
            if (nextWorkCenterWorkDay == null)
            {
                throw new PXException(Messages.NoWorkDaysForCalendarWorkCenter, workCenterCalendar.WorkCenter.WcID);
            }
            if (nextWorkCenterWorkDay.GetValueOrDefault().Date.CompareTo(currentSchedDateTime.Date) != 0)
            {
                currentSchedDateTime = nextWorkCenterWorkDay.GetValueOrDefault();
            }

            var wcDateInfo = workCenterCalendar.GetWorkingHours(nextWorkCenterWorkDay.GetValueOrDefault());

#if DEBUG
            AMDebug.TraceWriteMethodName($"Current date time: {currentSchedDateTime}");
#endif
            //Loop shifts (in the correct order)
            foreach (var amShift in workCenterCalendar.GetShifts(scheduleDirection))
            {
                ProcessOperationShift(schdOper, amShift, wcDateInfo.ShiftDateInfos[amShift.ShiftID],
                    workCenterCalendar, scheduleDirection, ref currentSchedDateTime, ref remainingSchdTime);
            }

            if (schdOper.StartDate == null || currentSchedDateTime.Date < schdOper.StartDate)
            {
                schdOper.StartDate = currentSchedDateTime.Date;
            }

            if (schdOper.EndDate == null || currentSchedDateTime.Date > schdOper.EndDate)
            {
                schdOper.EndDate = currentSchedDateTime.Date;
            }

            // right here we need to check the start/end of the combined wc date with the current datetime and see if we need to move to the "next date"
            if (IsCurrentDateUsedUp(wcDateInfo.WorkCenterDateInfo, currentSchedDateTime, scheduleDirection))
            {
                currentSchedDateTime = currentSchedDateTime.AddDays(AddDays(scheduleDirection)).Date;
            }
        }

        private bool IsCurrentDateUsedUp(AMDateInfo di, DateTime dateTime, ReadDirection direction)
        {
            var diStart = di.Date + di.StartTime;
            var diEnd = di.Date.AddDays(di.OvernightTime ? 1 : 0).Date + di.EndTime;

            if (direction == ReadDirection.Forward)
            {
                return dateTime.CompareTo(diStart) < 0 || dateTime.CompareTo(diEnd) >= 0;
            }

            return dateTime.CompareTo(diStart) <= 0 || dateTime.CompareTo(diEnd) > 0;
        }

        private void ProcessOperationMachines(List<MachineCalendarHelper> machineCalendars, ReadDirection scheduleDirection, ref AMSchdOper schdOper, ref DateTime currentSchedDateTime, ref int remainingSchdTime)
        {
            if (schdOper?.ProdOrdID == null)
            {
                throw new ArgumentNullException(nameof(schdOper));
            }

            if (machineCalendars == null || machineCalendars.Count == 0 || remainingSchdTime <= 0)
            {
                return;
            }

            var workCenter = machineCalendars[0].WorkCenter;
            if (workCenter?.WcID == null)
            {
                throw new PXArgumentException($"Missing work center record while processing order {schdOper.OrderType}-{schdOper.ProdOrdID}-{schdOper.OperationID}");
            }

            var shift = machineCalendars[0].Shift;
            if (shift?.ShiftID == null)
            {
                throw new PXArgumentException($"Missing shift record while processing order {schdOper.OrderType}-{schdOper.ProdOrdID}-{schdOper.OperationID}");
            }

#if DEBUG
            /*
             *For machines we will try each day to consume at least 1 machine so there are no gaps just like there are no gaps in the schedule by workcenter only
             *
             */
#endif
            var nextMachineWorkDay = MachineCalendarHelper.GetNextWorkDay(machineCalendars, currentSchedDateTime, true);
            if (nextMachineWorkDay == null)
            {
                throw new PXException($"Unable to get next work day for '{workCenter.WcID}' machines");
            }

            currentSchedDateTime = nextMachineWorkDay.GetValueOrDefault();

            var workCenterDateInfo = MachineCalendarHelper.AddWorkingDateInfo(machineCalendars, currentSchedDateTime, out var totalMachineWorkingMinutes);
            var workCenterSchd = GetWorkCenterSchd(workCenter.WcID, shift.ShiftID, workCenter.SiteID, workCenterDateInfo, totalMachineWorkingMinutes);

            if (workCenterSchd == null)
            {
                throw new PXException($"missing work center '{workCenter.WcID}' schedule date");
            }

            workCenterSchd = UpdateWCSchdFromMachSchds(GetMachineSchds(machineCalendars, currentSchedDateTime).ToList(), workCenterSchd);
            if (workCenterSchd != null)
            {
                workCenterSchd = WorkCenterSchdRecs.Update(workCenterSchd);
            }

            if (workCenterSchd == null || workCenterSchd.AvailableBlocks.GetValueOrDefault() == 0)
            {
                throw new SchedulingOrderDatesException(workCenterDateInfo.Date, workCenterDateInfo.Date.AddDays(AddDays(scheduleDirection)),
                    $"Could not find available time for work center '{workCenter.WcID}' machine '{string.Join("','", MachineCalendarHelper.ToMachineIds(machineCalendars))}' on requested date time '{workCenterDateInfo.Date}'");
            }

            currentSchedDateTime = AdjusteDateToEarliestDateTime(workCenterDateInfo, currentSchedDateTime, scheduleDirection);

#if DEBUG
            //Need to watch out for break times... and what if the break times are different for mult machines scheduling at once?
            // This will be different for exception dates as these are times not available - break times do not count as a split/gap in schedule time.
#endif
            // We want to pass in the full working time to create full blocks because the efficiency might need more time
            ScheduleWorkCenter(workCenterSchd, workCenterDateInfo, workCenter, shift, scheduleDirection, workCenterDateInfo.WorkingMinutes, currentSchedDateTime, out var wcSchdDetails);

            if (wcSchdDetails == null)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"Could not find available time for work center '{workCenter.WcID}' on requested date time '{currentSchedDateTime}'");
#endif
                PXTrace.WriteInformation($"Could not find available time for work center '{workCenter.WcID}' on requested date time '{currentSchedDateTime}'");
                return;
            }

            var insertedWorkCenterSchdDetails = new List<AMWCSchdDetail>();
            foreach (var wcSchdDetail in wcSchdDetails)
            {
                var machSchdDetails = GetIdealMachineSchdDetail(machineCalendars, wcSchdDetail, scheduleDirection,
                    currentSchedDateTime, remainingSchdTime, 0).GetHasSchdTime().ToList();

                var insertedWcSchdDetail = InsertMachineSchdDetail(machSchdDetails, wcSchdDetail, schdOper, false, out var totalMachSchdTime);
                if (insertedWcSchdDetail != null)
                {
                    insertedWorkCenterSchdDetails.Add(insertedWcSchdDetail);
                }
                remainingSchdTime -= totalMachSchdTime;
            }

            if (ToolScheduling)
            {
                ScheduleTool(insertedWorkCenterSchdDetails, schdOper, workCenterDateInfo, scheduleDirection, ref currentSchedDateTime);
            }

            CalculateUsedTime(insertedWorkCenterSchdDetails, scheduleDirection, currentSchedDateTime, out _, out currentSchedDateTime);

            if (schdOper.StartDate == null || currentSchedDateTime.Date < schdOper.StartDate)
            {
                schdOper.StartDate = currentSchedDateTime.Date;
            }

            if (schdOper.EndDate == null || currentSchedDateTime.Date > schdOper.EndDate)
            {
                schdOper.EndDate = currentSchedDateTime.Date;
            }

            // right here we need to check the start/end of the combined wc date with the current datetime and see if we need to move to the "next date"
            if (IsCurrentDateUsedUp(workCenterDateInfo, currentSchedDateTime, scheduleDirection))
            {
                currentSchedDateTime = currentSchedDateTime.AddDays(AddDays(scheduleDirection)).Date;
            }

            if (remainingSchdTime < 0)
            {
                remainingSchdTime = 0;
            }
        }

        private DateTime AdjusteDateToEarliestDateTime(AMDateInfo dateInfo, DateTime currentSchedDateTime, ReadDirection scheduleDirection)
        {
            if (scheduleDirection == ReadDirection.Forward &&
                dateInfo.StartTime.CompareTo(currentSchedDateTime.TimeOfDay) > 0)
            {
                return currentSchedDateTime.Date + dateInfo.StartTime;
            }
            if (scheduleDirection == ReadDirection.Backward &&
                dateInfo.EndTime.CompareTo(currentSchedDateTime.TimeOfDay) > 0)
            {
                return currentSchedDateTime.Date + dateInfo.EndTime;
            }

            return currentSchedDateTime;
        }

        private void ProcessOperationShift(AMSchdOper schdOper, AMShift wcShift, AMDateInfo shiftDateInfo, WorkCenterCalendarHelper workCenterCalendar, ReadDirection scheduleDirection, ref DateTime currentSchedDateTime, ref int remainingSchdTime)
        {
            if (schdOper == null)
            {
                throw new ArgumentNullException(nameof(schdOper));
            }

            if (workCenterCalendar == null)
            {
                throw new ArgumentNullException(nameof(workCenterCalendar));
            }

            if (wcShift?.ShiftID == null)
            {
                throw new ArgumentNullException(nameof(wcShift));
            }

            if (remainingSchdTime <= 0 || !shiftDateInfo.IsWorkingDay || shiftDateInfo.WorkingMinutes == 0)
            {
                return;
            }

            var addDays = scheduleDirection == ReadDirection.Backward ? -1 : 1;

            currentSchedDateTime = AdjusteDateToEarliestDateTime(shiftDateInfo, currentSchedDateTime, scheduleDirection);

            // then confirm its availability (current schedule is available)
            var checkWorkCenterSchd = GetWorkCenterSchd(wcShift.WcID, wcShift.ShiftID, workCenterCalendar.WorkCenter.SiteID, shiftDateInfo);

            if (checkWorkCenterSchd == null)
            {
                throw new PXException($"missing work center '{schdOper.WcID}' schedule date");
            }

            if (checkWorkCenterSchd.AvailableBlocks.GetValueOrDefault() == 0)
            {
                throw new SchedulingOrderDatesException(shiftDateInfo.Date, shiftDateInfo.Date.AddDays(addDays),
                    $"Could not find available time for work center '{wcShift.WcID.TrimIfNotNullEmpty()}' shift '{wcShift.ShiftID.TrimIfNotNullEmpty()}' on requested date time '{shiftDateInfo.Date}'");
            }

            // AT THIS POINT THE WC/Shift should have available time... Lets build an object that represents the current blocks in the day.

            ScheduleWorkCenter(checkWorkCenterSchd, shiftDateInfo, workCenterCalendar.WorkCenter, wcShift, scheduleDirection, schdOper, ref remainingSchdTime, ref currentSchedDateTime);
        }

        private void ScheduleWorkCenter(AMWCSchd workCenterSChd, AMDateInfo dateInfo,
            AMWC workCenter, AMShift wcShift, ReadDirection scheduleDirection, AMSchdOper schdOper,
            ref int remainingSchdTime, ref DateTime currentSchedDateTime)
        {
            ScheduleWorkCenter(workCenterSChd, dateInfo, workCenter, wcShift, scheduleDirection, remainingSchdTime, currentSchedDateTime, out var newWorkCenterSchdDetails);

            if (newWorkCenterSchdDetails == null || newWorkCenterSchdDetails.Count == 0)
            {
                currentSchedDateTime = NextAttemptDateTime(currentSchedDateTime, scheduleDirection, dateInfo);
                return;
            }

            var insertedList = new List<AMWCSchdDetail>();
            foreach (var schdDetail in newWorkCenterSchdDetails)
            {
                var inserted = InsertAMWCSchdDetail(schdDetail, schdOper);
                if (inserted == null)
                {
                    continue;
                }
                insertedList.Add(inserted);
            }

            if (ToolScheduling)
            {
                ScheduleTool(insertedList, schdOper, dateInfo, scheduleDirection, ref currentSchedDateTime);
            }

            CalculateUsedTime(insertedList, scheduleDirection, currentSchedDateTime, out var usedTime, out currentSchedDateTime);
            remainingSchdTime -= usedTime;
        }

        private void ScheduleWorkCenter(AMWCSchd workCenterSChd, AMDateInfo dateInfo, AMWC workCenter, AMShift wcShift, 
            ReadDirection scheduleDirection, int remainingSchdTime, DateTime currentSchedDateTime, out List<AMWCSchdDetail> wcSchdDetailList)
        {
            wcSchdDetailList = new List<AMWCSchdDetail>();

            var schdDateDetail = WorkCenterSchdDateDetail(workCenterSChd, dateInfo)
                .GetAvailableBlocks(remainingSchdTime, scheduleDirection, currentSchedDateTime.TimeOfDay);

            if (schdDateDetail.Results.Count == 0)
            {
                return;
            }

            var usedSchdTime = 0;
            foreach (var amwcSchdDetail in schdDateDetail.Results)
            {
                //consume these...
                amwcSchdDetail.WcID = workCenter.WcID;
                amwcSchdDetail.ShiftID = wcShift.ShiftID;
                amwcSchdDetail.SiteID = workCenter.SiteID;
                amwcSchdDetail.PlanBlocks = amwcSchdDetail.SchdBlocks;

                var maxedOut = usedSchdTime + amwcSchdDetail.SchdTime.GetValueOrDefault() > remainingSchdTime;
                if (maxedOut)
                {
                    //process part of a full calculated available time slot...

                    int partialTime = remainingSchdTime - usedSchdTime;
                    if (partialTime <= 0)
                    {
                        break;
                    }
                    usedSchdTime += partialTime;
                    amwcSchdDetail.SchdTime = partialTime;
                    amwcSchdDetail.SchdEfficiencyTime = partialTime;
                    amwcSchdDetail.SchdBlocks = MinutesToBlocks(partialTime, true);
                    amwcSchdDetail.PlanBlocks = amwcSchdDetail.SchdBlocks;
                    if (scheduleDirection == ReadDirection.Forward)
                    {
                        //adjust end time short...
                        amwcSchdDetail.EndTime = AddBlocks(amwcSchdDetail.StartTime.GetValueOrDefault(),
                            amwcSchdDetail.SchdBlocks.GetValueOrDefault());
                    }
                    if (scheduleDirection == ReadDirection.Backward)
                    {
                        //adjust start time short...
                        amwcSchdDetail.StartTime = AddBlocks(amwcSchdDetail.EndTime.GetValueOrDefault(),
                            amwcSchdDetail.SchdBlocks.GetValueOrDefault() * -1);
                    }
                }

                if (amwcSchdDetail.SchdKey == null)
                {
                    amwcSchdDetail.SchdKey = Guid.NewGuid();
                }

                wcSchdDetailList.Add(amwcSchdDetail);
                usedSchdTime += amwcSchdDetail.SchdTime.GetValueOrDefault();

                if (maxedOut)
                {
                    break;
                }
            }
        }

        protected virtual void ScheduleTool(List<AMWCSchdDetail> wcSchdDetails, AMSchdOper schdOper, AMDateInfo dateInfo, ReadDirection scheduleDirection, ref DateTime currentSchedDateTime)
        {
            //Schedule Tools
            if (!ToolScheduling || wcSchdDetails.Count <= 0)
            {
                return;
            }

            var ts = new ToolScheduleEngine(this, BlockSize);
            var prodTools = ts.GetProductionTools(schdOper);
            if (prodTools == null || prodTools.Count <= 0)
            {
                return;
            }

            if (ts.TrySchedule(schdOper, scheduleDirection, dateInfo,
                wcSchdDetails, currentSchedDateTime, prodTools, out var nextBestDateTime,
                out var newToolSchdDetails))
            {
                if (newToolSchdDetails == null)
                {
                    return;
                }

                var toolHash = new HashSet<string>();
                foreach (var toolSchdDetail in newToolSchdDetails)
                {
                    var i = ToolSchdDetailRecs.Insert(toolSchdDetail);
                    if (i?.ToolID != null)
                    {
                        toolHash.Add(i.ToolID.Trim());
                    }
                }

                // Set the WC Schd Detail description to show the tools used
                if (toolHash.Count <= 0)
                {
                    return;
                }

                var toolMsg = string.Join(",", toolHash.ToArray());
                foreach (var wcSchdDetail in wcSchdDetails)
                {
                    if (wcSchdDetail.IsBreak == true)
                    {
                        continue;
                    }

                    wcSchdDetail.Description = string.IsNullOrWhiteSpace(wcSchdDetail.Description)
                        ? toolMsg
                        : string.Join(";", wcSchdDetail.Description, toolMsg);
                    WorkCenterSchdDetailRecs.Update(wcSchdDetail);
                }

                return;
            }

            var msg =
                $"Unable to schedule tools '{string.Join(",", prodTools.FirstTableItems.Select(x => x.ToolID.TrimIfNotNullEmpty()).ToArray())}' for work center '{schdOper.WcID}' in production order '{schdOper.OrderType}:{schdOper.ProdOrdID}:{schdOper.OperationID}'  on date {currentSchedDateTime.ToShortDateString()} {currentSchedDateTime.ToLongTimeString()}";
#if DEBUG
            AMDebug.TraceWriteMethodName(msg);
#endif
            throw new SchedulingOrderDatesException(currentSchedDateTime,
                nextBestDateTime ?? NextAttemptDateTime(currentSchedDateTime, scheduleDirection, dateInfo),
                msg);
        }

        private void CalculateUsedTime(List<AMWCSchdDetail> wcSchdDetailList, ReadDirection scheduleDirection, DateTime currentSchedDateTime, out int usedTime, out DateTime schedDateTime)
        {
            usedTime = 0;
            schedDateTime = currentSchedDateTime;

            if (wcSchdDetailList == null)
            {
                return;
            }

            DateTime? start = null;
            DateTime? end = null;

            foreach (var schdDetail in wcSchdDetailList)
            {
                usedTime += schdDetail.SchdTime.GetValueOrDefault();

                if (start == null || schdDetail.StartTime.GetValueOrDefault() < start.GetValueOrDefault())
                {
                    start = schdDetail.StartTime.GetValueOrDefault();
                }
                if (end == null || schdDetail.EndTime.GetValueOrDefault() > end.GetValueOrDefault())
                {
                    end = schdDetail.EndTime.GetValueOrDefault();
                }
            }

            schedDateTime = currentSchedDateTime.Date +
                                   (scheduleDirection == ReadDirection.Forward
                                       ? end.GetValueOrDefault().TimeOfDay
                                       : start.GetValueOrDefault().TimeOfDay);
        }

        private DateTime NextAttemptDateTime(DateTime currentSchedDateTime, ReadDirection scheduleDirection, AMDateInfo dateInfo)
        {
            var nextSchdDateTime = currentSchedDateTime.Date +
                                   (scheduleDirection == ReadDirection.Forward
                                       ? dateInfo.EndTime
                                       : dateInfo.StartTime);

            if (nextSchdDateTime.CompareTo(currentSchedDateTime) == 0)
            {
                //lets just try to increase by a block..
                nextSchdDateTime = nextSchdDateTime.AddMinutes(BlockSize * AddDays(scheduleDirection));
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"Nothing available @ '{currentSchedDateTime}'; lets try '{nextSchdDateTime}'");
#endif
            return nextSchdDateTime;
        }

        protected List<AMWCSchdDetail> GetWCSchdDetail(AMWCSchd workCenterSchd)
        {
            return PXSelect<AMWCSchdDetail,
                Where<AMWCSchdDetail.wcID, Equal<Required<AMWCSchdDetail.wcID>>,
                And<AMWCSchdDetail.shiftID, Equal<Required<AMWCSchdDetail.shiftID>>,
                And<AMWCSchdDetail.schdDate, Equal<Required<AMWCSchdDetail.schdDate>>>>>
                >.Select(this, workCenterSchd.WcID, workCenterSchd.ShiftID, workCenterSchd.SchdDate).ToFirstTableList();
        }

        /// <summary>
        /// Updates the list of <see cref="AMWCSchdDetail"/> to provide the correct/updated break time setup. Will handle any change or missing break times.
        /// </summary>
        protected List<AMWCSchdDetail> UpdateWCSchdDetailBreakTimes(AMWCSchd workCenterSchd, List<AMWCSchdDetail> schdDetail, AMDateInfo dateInfo)
        {
            if (workCenterSchd == null || schdDetail == null || (dateInfo.BreakTimes?.Count ?? 0) == 0)
            {
                return schdDetail;
            }

            var all = schdDetail.Where(x => x.IsBreak != true).ToList();
            var calendarBreakTimes = dateInfo.BreakTimes;
            foreach (var breakSchdDetail in schdDetail.Where(x => x.IsBreak == true))
            {
                var isMatch = false;
                var unmatchedBreakTimes = new List<AMCalendarBreakTime>();
                foreach (var calendarBreakTime in calendarBreakTimes)
                {
                    if(!isMatch && IsMatchingBreakTime(breakSchdDetail, calendarBreakTime))
                    {
                        isMatch = true;
                        continue;
                    }
                    unmatchedBreakTimes.Add(calendarBreakTime);
                }

                calendarBreakTimes = unmatchedBreakTimes;

                if (isMatch)
                {
                    all.Add(breakSchdDetail);
                    continue;
                }

                WorkCenterSchdDetailRecs.Cache.Delete(breakSchdDetail);
            }

            //any calendarbreaktimes left in the list needs inserted
            all.AddRange(CreateBreakTimeDetail(workCenterSchd, calendarBreakTimes));

            return all;
        }

        protected bool IsMatchingBreakTime(AMWCSchdDetail schdDetail, AMCalendarBreakTime calendarBreakTime)
        {
            return schdDetail != null && calendarBreakTime != null && 
                   schdDetail.StartTime.GetValueOrDefault().TimeOfDay.Equals(calendarBreakTime.StartTime.GetValueOrDefault().TimeOfDay) &&
                   schdDetail.EndTime.GetValueOrDefault().TimeOfDay.Equals(calendarBreakTime.EndTime.GetValueOrDefault().TimeOfDay);
        }

        private SchdDateDetail<AMWCSchdDetail> WorkCenterSchdDateDetail(AMWCSchd workCenterSchd, AMDateInfo wcShiftDateInfo)
        {
            if (workCenterSchd == null || string.IsNullOrWhiteSpace(workCenterSchd.WcID) || Common.Dates.IsDefaultDate(workCenterSchd.SchdDate))
            {
                throw new PXArgumentException(nameof(workCenterSchd));
            }

            if (wcShiftDateInfo.Date.CompareTo(workCenterSchd.SchdDate.GetValueOrDefault()) != 0)
            {
                throw new PXArgumentException(nameof(wcShiftDateInfo));
            }

            return new SchdDateDetail<AMWCSchdDetail>(BlockSize, wcShiftDateInfo, UpdateWCSchdDetailBreakTimes(workCenterSchd, GetWCSchdDetail(workCenterSchd), wcShiftDateInfo));
        }

#if DEBUG
        ///// <summary>
        ///// Find the available employees for a given work center
        ///// (Question: does this return all available or the first found available based on required number of employees?)
        ///// </summary>
        ///// <returns></returns>
        //private EmployeeSchedulingAvailability FindAvailableWorkCenterEmployees(WorkCenterCalendarHelper workCenterCalendar, AMSchdOper schdOper, 
        //    PXResultset<EPEmployee> employeeResultset, ReadDirection scheduleDirection, DateTime currentSchedDate, HashSet<int> excludedEmployeeIDs)
        //{
        //    var employeeSchedulingResults = new EmployeeSchedulingAvailability(scheduleDirection);
        //    employeeSchedulingResults.SchdDateTime = currentSchedDate;

        //    if (employeeResultset == null || workCenterCalendar == null)
        //    {
        //        throw new PXException("No Dice");
        //    }

        ////    //GOAL OF THIS METHOD:
        ////    //  + how many employees do we need... (let the calling code define the number of employees to use)
        ////    //  + find the employees as available...
        ////    //  + return once found

        ////    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ////    //  Currently the shift defines the number of required employees?????
        ////    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ////    //int minNbrOfEmployees = 1;


        //    for (int e = 0; e < employeeResultset.Count; e++)
        //    {
        //        EPEmployee employeeResult = employeeResultset[e];
                
        //        if (employeeResult == null ||
        //            employeeResult.BAccountID <= 0 ||
        //            string.IsNullOrWhiteSpace(employeeResult.CalendarID))
        //        {
        //            continue;
        //        }

        //        //excluded employee
        //        //if (excludedEmployees != null && excludedEmployees.Any(employee => employeeResult.BAccountID == employee.BAccountID))
        //        if(excludedEmployeeIDs != null && excludedEmployeeIDs.Contains(employeeResult.BAccountID.GetValueOrDefault()))
        //        {
        //            continue;
        //        }

        //        EPEmployeeExt extension = PXCache<EPEmployee>.GetExtension<EPEmployeeExt>(employeeResult);
        //        if (extension == null || !extension.AMProductionEmployee.GetValueOrDefault())
        //        {
        //            continue;
        //        }

        //        CalendarHelper calendarHelper;
        //        try
        //        {
        //            calendarHelper = new CalendarHelper(this, employeeResult.CalendarID)
        //            {
        //                CalendarReadDirection = scheduleDirection
        //            };
        //        }
        //        catch (InvalidWorkCalendarException)
        //        {
        //            PXTrace.WriteWarning(string.Format(
        //                "Employee '{0};{1}' has an invalid work calendar '{2}'"
        //                , employeeResult.AcctCD.TrimIfNotNullEmpty(), employeeResult.AcctName.TrimIfNotNull(), employeeResult.CalendarID.TrimIfNotNull())
        //                );
        //            employeeSchedulingResults.UnavailableEmployees.Add(employeeResult);
        //            continue;
        //        }
                
        //        var dateInfo = calendarHelper.GetDateInfo(currentSchedDate);
        //        int employeeBlocks = MinutesToBlocks(dateInfo.WorkingMinutes, BlockSize, true);
        //        var employeeSchd = GetEmployeeSchd(employeeResult.BAccountID, dateInfo.Date, schdOper.SiteID, dateInfo.WorkingMinutes);

        //        bool unavailabEmployee = !dateInfo.IsWorkingDay ||
        //                                 employeeSchd.AvailableBlocks.GetValueOrDefault() < employeeBlocks;

        //        var nextAvailEmployeeSchd = NextEmployeeAvailableSchd(employeeResult, dateInfo.Date, schdOper.SiteID, calendarHelper);

        //        if (nextAvailEmployeeSchd != null)
        //        {
        //            employeeSchedulingResults.AddBestAvailableEmployeeSchd(nextAvailEmployeeSchd);

        //            AMDebug.TraceWriteMethodName(
        //                    string.Format(
        //                        "Employee ID '{0}' {3} for date {1}. Best Available Date {2}",
        //                        employeeResult.BAccountID, dateInfo.Date.ToShortDateString(),
        //                        nextAvailEmployeeSchd.SchdDate.GetValueOrDefault(NowDate).ToShortDateString(), unavailabEmployee ? "does not have enough available time" : "is available"));
        //        }

        //        if (unavailabEmployee)
        //        {
        //            employeeSchedulingResults.UnavailableEmployees.Add(employeeResult);
        //            continue;
        //        }

        //        employeeSchedulingResults.AvailableEmployees.Add(employeeResult);
        //    }
            
        //    return employeeSchedulingResults;
        //}


        //private int EmployeeShiftMinutes(AMDateInfo employeeDateInfo, AMDateInfo shiftDateInfo)
        //{
        //    return AMDateInfo.GetDateMinutes(AMDateInfo.LaterStartTimeAsDate(employeeDateInfo, shiftDateInfo), AMDateInfo.EarlierEndTimeAsDate(employeeDateInfo, shiftDateInfo));
        //}

        ///// <summary>
        ///// Schedule all employees required for an operation date
        ///// (multiple employees consume the same time bucket as this is side by side required work)
        ///// </summary>
        //private EmployeeSchdConsumed ScheduleWorkCenterEmployees(WorkCenterCalendarHelper workCenterCalendar, List<EPEmployee> employees, AMSchdOper schdOper, AMDateInfo shiftDateInfo, int requiredBlocks)
        //{
        //    var employeeSchdUpdates = new List<AMEmployeeSchd>();
        //    int totalUsedblocks = 0;

        //    foreach (var employee in employees)
        //    {
        //        var calendarHelper = new CalendarHelper(this, employee.CalendarID);
        //        var dateInfo = calendarHelper.GetDateInfo(shiftDateInfo.Date);

        //        var employeeShiftBlocks = MinutesToBlocks(EmployeeShiftMinutes(dateInfo, shiftDateInfo), BlockSize, false);

        //        int employeeBlocksCap = requiredBlocks > employeeShiftBlocks ? employeeShiftBlocks : requiredBlocks;

        //        if (!dateInfo.IsWorkingDay)
        //        {
        //            //not a working day for the employee
        //            throw new PXException("Not a working day for the employee");
        //        }

        //        var employeeSchd = GetEmployeeSchd(employee.BAccountID, dateInfo.Date, schdOper.SiteID, dateInfo.WorkingMinutes);

        //        if (employeeSchd == null || employeeSchd.BAccountID.GetValueOrDefault() <= 0)
        //        {
        //            throw new PXException("Employee Schedule Record Missing");
        //        }

        //        if (employeeSchd.AvailableBlocks < employeeBlocksCap)
        //        {
        //            AMDebug.TraceWriteMethodName(string.Format("Employee ID '{0}' does not have enough available time for date {1}.", employee.BAccountID, dateInfo.Date.ToShortDateString()));

        //            //nothing to schedule. 
        //            //  we are making an assumption that if the returned list count is less than the number of employees passed in that there was a scheduling issue.
        //            continue;
        //         }

        //        int usedBlocks = employeeBlocksCap;

        //        if (usedBlocks > employeeSchd.AvailableBlocks.GetValueOrDefault())
        //        {
        //            usedBlocks = employeeSchd.AvailableBlocks.GetValueOrDefault();
        //        }

        //        if (usedBlocks > 0)
        //        {
        //            AMDebug.TraceWriteMethodName(
        //                string.Format(
        //                    "Using '{0}' blocks on '{1}' for Employee '{2}-{6}' with order '{3}-{4}' operation '{5}' work center '{7}' [Employee Available before '{8}']",
        //                    usedBlocks, dateInfo.Date.ToShortDateString(), employeeSchd.BAccountID, schdOper.OrderType,
        //                    schdOper.ProdOrdID.TrimIfNotNullEmpty(), schdOper.OperNbr.TrimIfNotNullEmpty(), employee.AcctName.TrimIfNotNullEmpty(), schdOper.WcID.TrimIfNotNullEmpty(), employeeSchd.AvailableBlocks.GetValueOrDefault()));

        //            //we can use this employee as he has blocks available
        //            totalUsedblocks += usedBlocks;
        //            employeeSchd.AvailableBlocks = employeeSchd.AvailableBlocks.GetValueOrDefault() - usedBlocks;
        //            employeeSchd.SchdBlocks = employeeSchd.SchdBlocks.GetValueOrDefault() + usedBlocks;
        //            employeeSchdUpdates.Add(employeeSchd);

                    
        //        }
        //    }

        //    return new EmployeeSchdConsumed(totalUsedblocks, employeeSchdUpdates);
        //}

        //private AMEmployeeSchd NextEmployeeAvailableSchd(EPEmployee employee, DateTime date, int? siteID, CalendarHelper calendarHelper)
        //{
        //    var schdDate = NextEmployeeAvailableSchdDate(employee, date, siteID, calendarHelper.CalendarReadDirection);
        //    if (Common.Dates.IsDateNull(schdDate)
        //        || Common.Dates.IsDefaultDate(schdDate)
        //        || Common.Dates.IsMinMaxDate(schdDate.GetValueOrDefault()))
        //    {
        //        return null;
        //    }
        //    var schdDateInfo = calendarHelper.GetDateInfo(schdDate.GetValueOrDefault());

        //    if (!schdDateInfo.IsWorkingDay)
        //    {
        //        // Handles scenario where the returned available date is not a working date. Could be due to setup changes but we cannot count this as next available.
        //        var nextWorkingDate = calendarHelper.GetNextWorkDay(schdDate.GetValueOrDefault());
        //        if (Common.Dates.IsDefaultDate(nextWorkingDate))
        //        {
        //            //couldn't find the next working date
        //            return null;
        //        }
        //        return NextEmployeeAvailableSchd(employee, nextWorkingDate.GetValueOrDefault(), siteID, calendarHelper);
        //    }

        //    return GetEmployeeSchd(employee.BAccountID.GetValueOrDefault(), schdDate, siteID, schdDateInfo.WorkingMinutes);
        //}

        //private DateTime? NextEmployeeAvailableSchdDate(EPEmployee employee, DateTime date, int? siteID, ReadDirection readDirection)
        //{
        //    if (employee == null || employee.BAccountID.GetValueOrDefault() <= 0 || siteID.GetValueOrDefault() <= 0)
        //    {
        //        return null;
        //    }

        //    AMEmployeeSchd s = null;
        //    if (readDirection == ReadDirection.Backward)
        //    {
        //        s = PXSelectGroupBy<AMEmployeeSchd,
        //            Where<AMEmployeeSchd.siteID, Equal<Required<AMEmployeeSchd.siteID>>,
        //                And<AMEmployeeSchd.bAccountID, Equal<Required<AMEmployeeSchd.bAccountID>>,
        //                    And<AMEmployeeSchd.availableBlocks, Greater<Zero>,
        //                        And<AMEmployeeSchd.schdDate, Less<Required<AMEmployeeSchd.schdDate>>>>>>,
        //            Aggregate<Max<AMEmployeeSchd.schdDate>>>.Select(this, siteID, employee.BAccountID, date);

        //        return s.SchdDate;
        //    }
            
        //    s = PXSelectGroupBy<AMEmployeeSchd,
        //            Where<AMEmployeeSchd.siteID, Equal<Required<AMEmployeeSchd.siteID>>,
        //                And<AMEmployeeSchd.bAccountID, Equal<Required<AMEmployeeSchd.bAccountID>>,
        //                    And<AMEmployeeSchd.availableBlocks, Greater<Zero>,
        //                        And<AMEmployeeSchd.schdDate, Greater<Required<AMEmployeeSchd.schdDate>>>>>>,
        //            Aggregate<Min<AMEmployeeSchd.schdDate>>>.Select(this, siteID, employee.BAccountID, date);

        //    //what happens if there is no dates to a point that do not return any records?
        //    //   we might need to look at the last non avail date and then add one day...?

        //    return s.SchdDate;
        //} 
#endif

        private AMWCSchd NextWorkCenterAvailableSchd(WorkCenterCalendarHelper workCenterCalendarHelper, DateTime date, ReadDirection scheduleReadDirection)
        {
            var schdDate = NextWorkCenterAvailableSchdDate(workCenterCalendarHelper.WorkCenter, date, scheduleReadDirection);
            if (Common.Dates.IsDateNull(schdDate)
                || Common.Dates.IsDefaultDate(schdDate)
                || Common.Dates.IsMinMaxDate(schdDate.GetValueOrDefault()))
            {
                return null;
            }
            var schdDateInfo = workCenterCalendarHelper.GetWorkingHours(schdDate.GetValueOrDefault());

            if (!schdDateInfo.WorkCenterDateInfo.IsWorkingDay)
            {
                // Handles scenario where the returned available date is not a working date. Could be due to setup changes but we cannot count this as next available.
                var nextWorkingDate = workCenterCalendarHelper.GetNextWorkDay(schdDate.GetValueOrDefault(), scheduleReadDirection);
                if (Common.Dates.IsDefaultDate(nextWorkingDate))
                {
                    //couldn't find the next working date
                    return null;
                }
                return NextWorkCenterAvailableSchd(workCenterCalendarHelper, nextWorkingDate.GetValueOrDefault(), scheduleReadDirection);
            }

            return GetWorkCenterSchd(workCenterCalendarHelper.WorkCenter.WcID,"", workCenterCalendarHelper.WorkCenter.SiteID, schdDateInfo.WorkCenterDateInfo);
        }

        private DateTime? NextWorkCenterAvailableSchdDate(AMWC workCenter, DateTime date, ReadDirection readDirection)
        {
            if (workCenter == null || string.IsNullOrWhiteSpace(workCenter.WcID) || workCenter.SiteID.GetValueOrDefault() <= 0)
            {
                return null;
            }

            AMWCSchd s = null;
            if (readDirection == ReadDirection.Backward)
            {
                s = PXSelectGroupBy<AMWCSchd,
                    Where<AMWCSchd.siteID, Equal<Required<AMWCSchd.siteID>>,
                        And<AMWCSchd.wcID, Equal<Required<AMWCSchd.wcID>>,
                            And<AMWCSchd.availableBlocks, Greater<Zero>,
                                And<AMWCSchd.schdDate, Less<Required<AMWCSchd.schdDate>>>>>>,
                    Aggregate<Max<AMWCSchd.schdDate>>>.Select(this, workCenter.SiteID, workCenter.WcID, date);

                return s.SchdDate;
            }

            s = PXSelectGroupBy<AMWCSchd,
                    Where<AMWCSchd.siteID, Equal<Required<AMWCSchd.siteID>>,
                        And<AMWCSchd.wcID, Equal<Required<AMWCSchd.wcID>>,
                            And<AMWCSchd.availableBlocks, Greater<Zero>,
                                And<AMWCSchd.schdDate, Greater<Required<AMWCSchd.schdDate>>>>>>,
                    Aggregate<Min<AMWCSchd.schdDate>>>.Select(this, workCenter.SiteID, workCenter.WcID, date);

            //what happens if there is no dates to a point that do not return any records?
            //   we might need to look at the last non avail date and then add one day...?

            return s.SchdDate;
        }

#if DEBUG

        ///// <summary>
        ///// Find the employee schedule record for the given employee and date.
        ///// Constructs and inserts into cache a new record if one does not exist already returning a valid Employee Schedule record
        ///// </summary>
        ///// <param name="employeeID">Employee ID (Key)</param>
        ///// <param name="date">Employee schedule date (Key)</param>
        ///// <param name="siteID">Employee working site</param>
        ///// <param name="employeeWorkingMinutes">Employee working minutes for the given date</param>
        ///// <returns>New or Found Employee Schedule record</returns>
        //private AMEmployeeSchd GetEmployeeSchd(int? employeeID, DateTime? date, int? siteID, int employeeWorkingMinutes)
        //{
        //    if (employeeID.GetValueOrDefault() == 0
        //        || Common.Dates.IsDateNull(date)
        //        || siteID.GetValueOrDefault() == 0)
        //    {
        //        throw new PXException("stuff is missing");
        //    }

        //    AMEmployeeSchd employeeSchd = PXSelect<AMEmployeeSchd,
        //            Where<AMEmployeeSchd.bAccountID, Equal<Required<AMEmployeeSchd.bAccountID>>,
        //            And<AMEmployeeSchd.schdDate, Equal<Required<AMEmployeeSchd.schdDate>>>>
        //            >.Select(this, employeeID, date);

        //    if (employeeSchd == null)
        //    {
        //        int totalBlocks = MinutesToBlocks(employeeWorkingMinutes, BlockSize, false);
        //        employeeSchd = new AMEmployeeSchd()
        //        {
        //            BAccountID = employeeID,
        //            SchdDate = date,
        //            SiteID = siteID,
        //            TotalBlocks = totalBlocks,
        //            AvailableBlocks = totalBlocks,
        //        };
        //        employeeSchd = this.EmployeeSchdRecs.Insert(employeeSchd);
        //    }

        //    employeeSchd.WorkTime = employeeWorkingMinutes;

        //    return employeeSchd;
        //}

        //public struct EmployeeSchdConsumed
        //{
        //    public List<AMEmployeeSchd> EmployeeSchds { get;}
        //    public int TotalBlocksUsed { get;}

        //    public EmployeeSchdConsumed(int totalblocksUsed, List<AMEmployeeSchd> employeeSchds)
        //    {
        //        TotalBlocksUsed = totalblocksUsed;

        //        if (employeeSchds == null)
        //        {
        //            throw new PXArgumentException("employeeSchds");
        //        }

        //        EmployeeSchds = employeeSchds;
        //    }
        //}

        

        //public sealed class EmployeeSchedulingAvailability
        //{
        //    public HashSet<EPEmployee> AvailableEmployees;

        //    /// <summary>
        //    /// List of employees unavailable based on
        //    /// </summary>
        //    public HashSet<EPEmployee> UnavailableEmployees;

        //    /// <summary>
        //    /// Scheduling date direction
        //    /// </summary>
        //    public ReadDirection SchedulingDirection { get; }

        //    /// <summary>
        //    /// SCheduling Date
        //    /// </summary>
        //    public DateTime? SchdDateTime;

        //    /// <summary>
        //    /// Best available employee schedule record from all employees (not to be the same as the schedule date).
        //    /// </summary>
        //    public AMEmployeeSchd BestAvailableEmployeeSchd { get; private set; }

        //    /// <summary>
        //    /// Each employee and their best available schedule record.
        //    /// </summary>
        //    public Dictionary<int, AMEmployeeSchd> BestAvailableEmployeeSchds { get; private set; }

        //    public EmployeeSchedulingAvailability(ReadDirection schedulingDirection)
        //    {
        //        AvailableEmployees = new HashSet<EPEmployee>();
        //        UnavailableEmployees = new HashSet<EPEmployee>();
        //        BestAvailableEmployeeSchds = new Dictionary<int, AMEmployeeSchd>();
        //        SchedulingDirection = schedulingDirection;
        //    }

        //    public void AddBestAvailableEmployeeSchd(AMEmployeeSchd employeeSchd)
        //    {
        //        if (employeeSchd == null || employeeSchd.BAccountID.GetValueOrDefault() <= 0)
        //        {
        //            return;
        //        }

        //        RemoveBestAvailableEmployeeSchd(employeeSchd);
        //        BestAvailableEmployeeSchds.Add(employeeSchd.BAccountID.GetValueOrDefault(), employeeSchd);
        //        SetBestAvailableEmployeeSchd(employeeSchd);
        //    }

        //    public bool RemoveBestAvailableEmployeeSchd(AMEmployeeSchd employeeSchd)
        //    {

        //        //
        //        //  What if the current BestAvailableEmployeeSchd row is being removed????
        //        //

        //        if (employeeSchd == null || employeeSchd.BAccountID.GetValueOrDefault() <= 0)
        //        {
        //            return false;
        //        }

        //        if (BestAvailableEmployeeSchds.ContainsKey(employeeSchd.BAccountID.GetValueOrDefault()))
        //        {
        //            BestAvailableEmployeeSchds.Remove(employeeSchd.BAccountID.GetValueOrDefault());
        //            return true;
        //        }
        //        return false;
        //    }

        //    private void SetBestAvailableEmployeeSchd(AMEmployeeSchd employeeSchd)
        //    {
        //        if (BestAvailableEmployeeSchd == null)
        //        {
        //            AMDebug.TraceWriteMethodName(
        //                string.Format("Setting best Available using Employee ID '{0}' Schedule date '{1}'"
        //                , employeeSchd.BAccountID.GetValueOrDefault(), employeeSchd.SchdDate.GetValueOrDefault().ToShortDateString()));

        //            BestAvailableEmployeeSchd = employeeSchd;
        //            return;
        //        }
                
        //        if (Common.Dates.Compare(BestAvailableEmployeeSchd.SchdDate, employeeSchd.SchdDate) == (SchedulingDirection == ReadDirection.Backward ? -1 : 1))
        //        {
        //            AMDebug.TraceWriteMethodName(
        //                string.Format("Setting best Available using Employee ID '{0}' Schedule date '{1}' [Replacing ID '{2}' Date '{3}']"
        //                , employeeSchd.BAccountID.GetValueOrDefault(), employeeSchd.SchdDate.GetValueOrDefault().ToShortDateString()
        //                , BestAvailableEmployeeSchd.BAccountID.GetValueOrDefault(), BestAvailableEmployeeSchd.SchdDate.GetValueOrDefault().ToShortDateString()));

        //            BestAvailableEmployeeSchd = employeeSchd;
        //        }
        //    }
        //} 
#endif

        public new static ProductionScheduleEngineAdv Construct()
        {
            return CreateInstance<ProductionScheduleEngineAdv>();
        }

        /// <summary>
        /// Used to pre-build work schedule days and adjust for exceptions
        /// </summary>
        public static void CreateWorkSchdDays(AMWC workCenter, DateTime? fromDate, DateTime? toDate)
        {
            var schdEngine = Construct();

            if (workCenter == null || fromDate == null || toDate == null)
            {
                return;
            }

            PXTrace.WriteInformation($"Creating schedule work days for work center {workCenter.WcID} between {fromDate.GetValueOrDefault().ToShortDateString()} and {toDate.GetValueOrDefault().ToShortDateString()}");

            var wcCalHelper = new WorkCenterCalendarHelper(schdEngine, workCenter, true);

            var runAsMachineSchedule = schdEngine.IsMachineScheduling(workCenter);
            if (runAsMachineSchedule)
            {
                var machineCalendars = schdEngine.GetActiveMachineCalendars(workCenter, wcCalHelper.GetFirstShift(), ReadDirection.Forward).ToList();

                runAsMachineSchedule = machineCalendars.Count > 0;

                if (runAsMachineSchedule)
                {
                    CreateWorkSchdDaysByMachines(schdEngine, machineCalendars, workCenter, wcCalHelper.GetFirstShift(), fromDate, toDate);
                }
            }
            
            if(!runAsMachineSchedule)
            {
                CreateWorkSchdDaysByWorkCenter(schdEngine, wcCalHelper, fromDate, toDate);
            }

            schdEngine.Actions.PressSave();
        }

        private static void CreateWorkSchdDaysByMachines(ProductionScheduleEngineAdv schdEngine, List<MachineCalendarHelper> machineCalendars, AMWC workCenter, AMShift firstShift, DateTime? fromDate, DateTime? toDate)
        {
            var date = fromDate.GetValueOrDefault(Common.Current.BusinessDate(schdEngine));

            if (date.CompareTo(toDate.GetValueOrDefault(Common.Current.BusinessDate(schdEngine))) > 0)
            {
                return;
            }

            while (date.CompareTo(toDate.GetValueOrDefault(Common.Current.BusinessDate(schdEngine))) <= 0)
            {
                var nextWorkDate = MachineCalendarHelper.GetNextWorkDay(machineCalendars, date, true);

                if (nextWorkDate == null)
                {
                    return;
                }

                date = nextWorkDate.GetValueOrDefault();
                var workCenterDateInfo = MachineCalendarHelper.AddWorkingDateInfo(machineCalendars, date, out var totalMachineWorkingMinutes);
                var wcSchd = schdEngine.GetWorkCenterSchd(workCenter.WcID, firstShift.ShiftID, workCenter.SiteID, workCenterDateInfo, totalMachineWorkingMinutes);
                var machineSchds = schdEngine.GetMachineSchds(machineCalendars, date).ToList();

                if (wcSchd != null)
                {
                    wcSchd = schdEngine.UpdateWCSchdFromMachSchds(machineSchds, wcSchd);
                    schdEngine.WorkCenterSchdRecs.Update(wcSchd);
                }

                foreach (var machineSchd in machineSchds)
                {
                    if (machineSchd != null)
                    {
                        var rowStatus = schdEngine.MachineSchdRecs.Cache.GetStatus(machineSchd);
                        if (rowStatus == PXEntryStatus.Notchanged)
                        {
                            schdEngine.MachineSchdRecs.Update(machineSchd);
                        }
                    }
                }

                date = date.AddDays(1);
            }
        }

        private static void CreateWorkSchdDaysByWorkCenter(ProductionScheduleEngineAdv schdEngine, WorkCenterCalendarHelper wcCalHelper, DateTime? fromDate, DateTime? toDate)
        {
            var date = fromDate.GetValueOrDefault(Common.Current.BusinessDate(schdEngine));

            if (date.CompareTo(toDate.GetValueOrDefault(Common.Current.BusinessDate(schdEngine))) > 0)
            {
                return;
            }

            while (date.CompareTo(toDate.GetValueOrDefault(Common.Current.BusinessDate(schdEngine))) <= 0)
            {
                var wcWorkDate = wcCalHelper.GetNextWorkDay(date, ReadDirection.Forward, true);

                if (wcWorkDate == null)
                {
                    date = date.AddDays(1);
                    continue;
                }
                date = wcWorkDate.GetValueOrDefault();

                var wcDateInfo = wcCalHelper.GetWorkingHours(date);

                foreach (var amShift in wcCalHelper.GetShifts(ReadDirection.Forward))
                {
                    var wcSchd = schdEngine.GetWorkCenterSchd(wcCalHelper.WorkCenter.WcID, amShift.ShiftID, wcCalHelper.WorkCenter.SiteID, wcDateInfo.ShiftDateInfos[amShift.ShiftID]);
                    if (wcSchd != null)
                    {
                        var rowStatus = schdEngine.WorkCenterSchdRecs.Cache.GetStatus(wcSchd);
                        if (rowStatus == PXEntryStatus.Notchanged)
                        {
                            schdEngine.WorkCenterSchdRecs.Update(wcSchd);
                        }
                    }
                }

                date = date.AddDays(1);
            }
        }

        private void CreateBreakTimeDetail(AMWCSchd workCenterSchd, AMDateInfo dateInfo)
        {
            if (!dateInfo.IsWorkingDay)
            {
                return;
            }

            CreateBreakTimeDetail(workCenterSchd, dateInfo.BreakTimes);
        }

        private List<AMWCSchdDetail> CreateBreakTimeDetail(AMWCSchd workCenterSchd, List<AMCalendarBreakTime> breakTimes)
        {
            var newBreakTimes = new List<AMWCSchdDetail>();
            if (breakTimes == null || breakTimes.Count == 0)
            {
                return newBreakTimes;
            }

            foreach (var calendarBreakTime in breakTimes)
            {
                var startTime = calendarBreakTime.StartTime;
                var endTime = BlockRoundedDate(calendarBreakTime.EndTime.GetValueOrDefault(), BlockSize, true);
                var schdTime = AMDateInfo.GetDateMinutes(startTime.GetValueOrDefault(), endTime);

                newBreakTimes.Add(WorkCenterSchdDetailRecs.Insert(new AMWCSchdDetail
                {
                    WcID = workCenterSchd.WcID,
                    ShiftID = workCenterSchd.ShiftID,
                    SchdDate = workCenterSchd.SchdDate,
                    SiteID = workCenterSchd.SiteID,
                    StartTime = startTime,
                    EndTime = endTime,
                    Description = calendarBreakTime.Description,
                    IsBreak = true,
                    SchdTime = schdTime,
                    SchdEfficiencyTime = schdTime,
                    OrderByDate = workCenterSchd.SchdDate.GetValueOrDefault().Date + startTime.GetValueOrDefault().TimeOfDay
                }));
            }

            return newBreakTimes;
        }

        private void CreateBreakTimeDetail(AMMachSchd machineSchd, AMDateInfo dateInfo)
        {
            if (dateInfo.BreakTimes == null || dateInfo.BreakTimes.Count == 0 || !dateInfo.IsWorkingDay)
            {
                return;
            }

            foreach (var calendarBreakTime in dateInfo.BreakTimes)
            {
                var startTime = calendarBreakTime.StartTime;
                var endTime = BlockRoundedDate(calendarBreakTime.EndTime.GetValueOrDefault(), BlockSize, true);
                var schdTime = AMDateInfo.GetDateMinutes(startTime.GetValueOrDefault(), endTime);

                MachineSchdDetailRecs.Insert(new AMMachSchdDetail
                {
                    MachID = machineSchd.MachID,
                    SchdDate = machineSchd.SchdDate,
                    SiteID = machineSchd.SiteID,
                    StartTime = startTime,
                    EndTime = endTime,
                    Description = calendarBreakTime.Description,
                    IsBreak = true,
                    SchdTime = schdTime,
                    SchdEfficiencyTime = schdTime,
                    OrderByDate = machineSchd.SchdDate.GetValueOrDefault().Date + startTime.GetValueOrDefault().TimeOfDay
                });
            }
        }

        protected override AMWCSchd GetWorkCenterSchd(string workCenterID, string shiftID, int? siteID, AMDateInfo dateInfo, int workingMinutes)
        {
#if DEBUG
            // HERE WE NEED TO FIND OUT IF THE ROW WAS INSERTED THAT IS RETURNED OR NOT.
            //  IF INSERTED THEN WE WILL CREATE THE BREAK TIME DETAIL. (In the future we might do this differently as we might need to rebuild the breaks if the setup changed.) 
#endif
            var wcSchd =  base.GetWorkCenterSchd(workCenterID, shiftID, siteID, dateInfo, workingMinutes);
            if (wcSchd != null && WorkCenterSchdRecs.Cache.GetStatus(wcSchd) == PXEntryStatus.Inserted)
            {
                CreateBreakTimeDetail(wcSchd, dateInfo);
            }

            return wcSchd;
        }

        protected override AMMachSchd GetMachineSchd(string machineID, int? siteID, AMDateInfo dateInfo)
        {
            var schd = base.GetMachineSchd(machineID, siteID, dateInfo);
            if (schd != null && MachineSchdRecs.Cache.GetStatus(schd) == PXEntryStatus.Inserted)
            {
                CreateBreakTimeDetail(schd, dateInfo);
            }

            return schd;
        }

        internal static List<MaterialAvailabiltyDates> MaterialAvailability(PXGraph graph, DateTime planDate, string orderType, string prodOrdId, int? operationId)
        {
            var materialList = new List<MaterialAvailabiltyDates>();

            foreach (PXResult<AMProdMatl, InventoryItem, AMProdOper> pxResult in PXSelectJoin<
                AMProdMatl, 
                InnerJoin<InventoryItem, 
                    On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<AMProdOper, On<AMProdMatl.orderType, Equal<AMProdOper.orderType>,
                    And<AMProdMatl.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMProdMatl.operationID, Equal<AMProdOper.operationID>>>>>>,
                Where<AMProdMatl.orderType, Equal<Required<AMProdOper.orderType>>, 
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>, 
                    And<AMProdMatl.operationID, Equal<Required<AMProdOper.operationID>>,
                    And<InventoryItem.stkItem, Equal<True>>>>>>
                .Select(graph, orderType, prodOrdId, operationId))
            {
                var material = (AMProdMatl) pxResult;
                var inventoryItem = (InventoryItem) pxResult;
                if (material.IsByproduct.GetValueOrDefault() || material.TotalQtyRequired.GetValueOrDefault() == 0 || inventoryItem.InventoryID == null)
                {
                    continue;
                }

                var itemExt = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);
                if (itemExt != null && itemExt.AMCheckSchdMatlAvailability == false)
                {
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Skip check of availability of item {inventoryItem.InventoryCD} based on InventoryItemExt.AMCheckSchdMatlAvailability");
#endif
                    continue;
                }

                var result = MaterialAvailability(graph, planDate, material, (InventoryItem)pxResult, (AMProdOper)pxResult);
                if (result == null)
                {
                    continue;
                }
                materialList.Add(result);
            }

            return materialList;
        }

        internal static MaterialAvailabiltyDates MaterialAvailability(PXGraph graph, DateTime planDate,
            AMProdMatl prodMatl, InventoryItem inventoryItem, AMProdOper prodOper)
        {
            if (string.IsNullOrWhiteSpace(prodMatl?.ProdOrdID))
            {
                throw new ArgumentNullException(nameof(prodMatl));
            }

            if (inventoryItem?.InventoryID == null)
            {
                throw new ArgumentNullException(nameof(inventoryItem));
            }

            if (prodOper?.OperationID == null)
            {
                throw new ArgumentNullException(nameof(prodOper));
            }

            // Itemstatus contains qty on hand, hard allocated qty, etc.
            var itemStatus = GetItemSiteStatus(graph, prodMatl?.SiteID, prodMatl?.InventoryID, prodMatl?.SubItemID);

            var startingQtyAvail = itemStatus?.QtyOnHand ?? 0m;
            startingQtyAvail -= itemStatus?.QtySOShipping ?? 0m + itemStatus?.QtyProductionAllocated ?? 0m;

#if DEBUG
            AMDebug.TraceWriteMethodName($"[{prodMatl.OrderType}:{prodMatl.ProdOrdID}:{prodOper.OperationCD}:{prodMatl.LineNbr}] startingQtyAvail = {startingQtyAvail}; InventoryID = ({itemStatus?.InventoryID}) {inventoryItem.InventoryCD.TrimIfNotNullEmpty()}; SiteID = {itemStatus?.SiteID}; QtyOnHand = {itemStatus?.QtyOnHand}; QtyAvail = {itemStatus?.QtyAvail}; QtyHardAvail = {itemStatus?.QtyHardAvail}");
#endif
            var prodMatlSplits = GetSplits(graph, prodMatl);
            if (prodMatlSplits == null || prodMatlSplits.Count == 0)
            {
                return null;
            }

            var allProdMatlSplitsList = GetSplits(prodMatlSplits);
            
            var itemPlans = GetItemPlans(graph, prodMatl?.SiteID, prodMatl?.InventoryID, prodMatl?.SubItemID);
            if (itemPlans == null)
            {
                return null;
            }

            //Find full linked supply to prodmatlsplits
            var linkedMaterialSupply = GetSupplyLinkedToDemand(graph, itemPlans, prodMatlSplits);

            var totalLinkedQty = 0m;
            DateTime? dateAvailable = null;
            if (linkedMaterialSupply != null)
            {
                foreach (PXResult<INItemPlanWithLinkQty, INItemPlan> result in linkedMaterialSupply)
                {
                    var supply = (INItemPlanWithLinkQty) result;
                    var demand = (INItemPlan) result;

                    var linkQty = 0m;
                    if (IsAllocatedPlantype(demand?.PlanType))
                    {
                        linkQty = demand?.PlanQty ?? 0m;
                        dateAvailable = dateAvailable == null
                            ? demand?.PlanDate.GetValueOrDefault()
                            : dateAvailable.GreaterDateTime(demand?.PlanDate);
                    }
                    else
                    {
                        linkQty = Math.Min(supply?.PlanQty ?? 0m, demand?.PlanQty ?? 0m);
                        dateAvailable = dateAvailable == null
                            ? supply?.PlanDate.GetValueOrDefault()
                            : dateAvailable.GreaterDateTime(supply?.PlanDate);
                    }
                    totalLinkedQty += linkQty;
                }
            }

            var remainingQty = prodMatl.BaseTotalQtyRequired.GetValueOrDefault() - totalLinkedQty;

            if (remainingQty <= 0)
            {
                if (dateAvailable == null || dateAvailable.GreaterThan(planDate))
                {
                    return new MaterialAvailabiltyDates(prodMatl, prodOper?.OperationCD, inventoryItem, planDate, allProdMatlSplitsList)
                    {
                        AvailableBaseQtyShort = 0m,
                        FirstLateAvailableDate = dateAvailable,
                        FirstLateAvailableBaseQtyShort = 0m
                    };
                }

                return new MaterialAvailabiltyDates(prodMatl, prodOper?.OperationCD, inventoryItem, planDate, allProdMatlSplitsList)
                {
                    AvailableDate = dateAvailable,
                    AvailableBaseQtyShort = 0m
                };
            }

            if (startingQtyAvail > 0 && remainingQty <= startingQtyAvail)
            {
                var allocatingMaterialList = new List<AMProdMatlSplit>();
                foreach (PXResult<AMProdMatlSplit, INItemPlan, INItemPlanAMExtension> value in prodMatlSplits.Values)
                {
                    var prodMatlSplit = (AMProdMatlSplit) value;
                    if (string.IsNullOrWhiteSpace(prodMatlSplit?.ProdOrdID) ||
                        prodMatlSplit.IsAllocated.GetValueOrDefault())
                    {
                        continue;
                    }
                    allocatingMaterialList.Add(prodMatlSplit);
                }
                
                //we have enough on hand to supply what we need (but how do we reduce the overall soft allocated qty from available qty?)
                dateAvailable = graph.Accessinfo.BusinessDate;
                return new MaterialAvailabiltyDates(prodMatl, prodOper?.OperationCD, inventoryItem, planDate, allProdMatlSplitsList)
                {
                    AvailableDate = dateAvailable,
                    AvailableBaseQtyShort = 0m,
                    UsedStockInventory = true,
                    AllocatingProdMatlSplits = allocatingMaterialList
                };
            }

            //any left over look for unlinked supply...

            var itemPlansByDate = GetItemPlansLessEqualDate(graph, itemPlans, planDate)?.OrderByDescending(x => x.PlanDate).ToList();
            if (itemPlansByDate != null)
            {
                var linkedSupply = FindUnlinkedSupplyPlans(remainingQty, itemPlansByDate);
                if (linkedSupply != null)
                {
                    foreach (var result in linkedSupply)
                    {
                        //Tuple<INItemPlanWithLinkQty, decimal>
                        var supply = result.Item1;
                        var suppliedQty = result.Item2;

                        if (suppliedQty <= 0)
                        {
                            continue;
                        }
#if DEBUG
                        //TODO: QtyAllocation - need to save/return the supply we matched to that was not already linked to the material. This way we can persist the soft supply link later or at least in cache so the next loop on the same material will only see the remaining supply qty
#endif
                        dateAvailable = dateAvailable == null
                            ? supply.PlanDate.GetValueOrDefault()
                            : dateAvailable.GreaterDateTime(supply.PlanDate);
                        remainingQty -= suppliedQty;
                        if (remainingQty <= 0)
                        {
                            break;
                        }
                    }
                }
            }

            if (remainingQty <= 0)
            {
                if (dateAvailable == null || dateAvailable.GreaterThan(planDate))
                {
                    return new MaterialAvailabiltyDates(prodMatl, prodOper?.OperationCD, inventoryItem, planDate, allProdMatlSplitsList)
                    {
                        AvailableBaseQtyShort = 0m,
                        FirstLateAvailableDate = dateAvailable,
                        FirstLateAvailableBaseQtyShort = 0m
                    };
                }

                return new MaterialAvailabiltyDates(prodMatl, prodOper?.OperationCD, inventoryItem, planDate, allProdMatlSplitsList)
                {
                    AvailableDate = dateAvailable,
                    AvailableBaseQtyShort = 0m
                };
            }

            var availableBaseQty = remainingQty * 1;
            DateTime? firstAvailableAfterPlanDate = dateAvailable;
            dateAvailable = null;

            itemPlansByDate = GetItemPlansGreaterThanDate(graph, itemPlans, planDate)?.OrderBy(x => x.PlanDate).ToList();
            if (itemPlansByDate != null)
            {
                var linkedSupply = FindUnlinkedSupplyPlans(remainingQty, itemPlansByDate);
                if (linkedSupply != null)
                {
                    foreach (var result in linkedSupply)
                    {
                        //Tuple<INItemPlanWithLinkQty, decimal>
                        var supply = result.Item1;
                        var suppliedQty = result.Item2;

                        if (suppliedQty <= 0)
                        {
                            continue;
                        }
#if DEBUG
                        //TODO: QtyAllocation - need to save/return the supply we matched to that was not already linked to the material. This way we can persist the soft supply link later or at least in cache so the next loop on the same material will only see the remaining supply qty                
#endif
                        dateAvailable = dateAvailable == null
                            ? supply.PlanDate.GetValueOrDefault()
                            : dateAvailable.GreaterDateTime(supply.PlanDate);
                        remainingQty -= suppliedQty;
                        if (remainingQty <= 0)
                        {
                            break;
                        }
                    }
                }
            }

            if (remainingQty > 0)
            {
                firstAvailableAfterPlanDate = null;
            }

            return new MaterialAvailabiltyDates(prodMatl, prodOper?.OperationCD, inventoryItem, planDate, allProdMatlSplitsList)
            {
                AvailableBaseQtyShort = availableBaseQty,
                FirstLateAvailableDate = firstAvailableAfterPlanDate,
                FirstLateAvailableBaseQtyShort = remainingQty
            };
        }

        public static List<PXResult<INItemPlanWithLinkQty, INItemPlan>> GetSupplyLinkedToDemand(PXGraph graph, IEnumerable<INItemPlanWithLinkQty> itemPlans, Dictionary<long, PXResult<AMProdMatlSplit, INItemPlan, INItemPlanAMExtension>> demandPlans)
        {
            var list = new List<PXResult<INItemPlanWithLinkQty, INItemPlan>>();
            if (itemPlans == null || demandPlans == null || demandPlans.Count == 0)
            {
                return list;
            }

            var demandMarkedWithSupply = new Dictionary<long, INItemPlan>();
            foreach (var demandPlan in demandPlans)
            {
                var inItemPlan = (INItemPlan) demandPlan.Value;
                if (IsAllocatedPlantype(inItemPlan?.PlanType))
                {
                    //Add the allocated entry to the list to show as "supply"
                    list.Add(new PXResult<INItemPlanWithLinkQty, INItemPlan>(null, inItemPlan));
                    continue;
                }

                if (inItemPlan?.SupplyPlanID != null)
                {
                    demandMarkedWithSupply[inItemPlan.SupplyPlanID.GetValueOrDefault()] = inItemPlan;
                    continue;
                }

                var inItemPlanExt = (INItemPlanAMExtension) demandPlan.Value;
                if (inItemPlanExt?.PlanID == null)
                {
                    continue;
                }

                demandMarkedWithSupply[inItemPlanExt.AMSoftSupplyPlanID.GetValueOrDefault()] = inItemPlan;
            }

            if (demandMarkedWithSupply.Count == 0)
            {
                return list;
            }

            foreach (var itemPlan in itemPlans)
            {
                if (itemPlan?.PlanID == null || 
                    !itemPlan.IsSupply.GetValueOrDefault() || 
                    !demandMarkedWithSupply.TryGetValue(itemPlan.PlanID.GetValueOrDefault(), out var demandItemPlan))
                {
                    continue;
                }

                list.Add(new PXResult<INItemPlanWithLinkQty, INItemPlan>(itemPlan, demandItemPlan));
            }

            return list;
        }

        private static List<Tuple<INItemPlanWithLinkQty, decimal>> FindUnlinkedSupplyPlans(decimal requiredSupplyQty, IEnumerable<INItemPlanWithLinkQty> itemPlansByDate)
        {
            var list = new List<Tuple<INItemPlanWithLinkQty, decimal>>();
            var remainingQty = requiredSupplyQty * 1m;
            foreach (var itemPlan in itemPlansByDate)
            {
                if (!itemPlan.IsSupply.GetValueOrDefault())
                {
                    continue;
                }

                var supplyRemainingQty = itemPlan.PlanQty.GetValueOrDefault() -
                                         itemPlan.SupplyHardPlanQty.GetValueOrDefault() -
                                         itemPlan.SupplySoftPlanQty.GetValueOrDefault();

                if (supplyRemainingQty <= 0)
                {
                    continue;
                }

                if (supplyRemainingQty > remainingQty)
                {
                    supplyRemainingQty = remainingQty;
                }

                remainingQty -= supplyRemainingQty;
                list.Add(new Tuple<INItemPlanWithLinkQty, decimal>(itemPlan, supplyRemainingQty));

                if (remainingQty <= 0)
                {
                    break;
                }    
            }

            return list;
        }

        private static bool IsAllocatedPlantype(string planType)
        {
            return planType != null && INPlanTypeHelper.IsAllocated(planType);
        }

        private static List<AMProdMatlSplit> GetSplits(Dictionary<long, PXResult<AMProdMatlSplit, INItemPlan, INItemPlanAMExtension>> prodSplits)
        {
            var list = new List<AMProdMatlSplit>();
            if (prodSplits == null)
            {
                return list;
            }

            foreach (PXResult<AMProdMatlSplit, INItemPlan, INItemPlanAMExtension> result in prodSplits.Values)
            {
                var split = (AMProdMatlSplit) result;
                if (split?.SplitLineNbr == null)
                {
                    continue;
                }
                list.Add(split);
            }
            return list;
        }

        private static Dictionary<long, PXResult<AMProdMatlSplit, INItemPlan, INItemPlanAMExtension>> GetSplits(PXGraph graph, AMProdMatl prodMatl)
        {
            var dic = new Dictionary<long, PXResult<AMProdMatlSplit, INItemPlan, INItemPlanAMExtension>> ();
            var hash = new HashSet<long>();
            if (string.IsNullOrWhiteSpace(prodMatl.ProdOrdID))
            {
                return dic;
            }

            foreach (PXResult<AMProdMatlSplit, INItemPlan, INItemPlanAMExtension> result in PXSelectJoin<AMProdMatlSplit,
                LeftJoin<INItemPlan, On<AMProdMatlSplit.planID, Equal<INItemPlan.planID>>,
                LeftJoin<INItemPlanAMExtension, On<AMProdMatlSplit.planID, Equal<INItemPlanAMExtension.planID>>>>,
                Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
                    And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
                        And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
                            And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>>.SelectMultiBound(graph, new object[] { prodMatl }))
            {
                var split = (AMProdMatlSplit) result;
                if (split?.PlanID == null)
                {
                    continue;
                }

                var planId = split.PlanID.GetValueOrDefault();
                if (hash.Add(planId))
                {
                    dic.Add(planId, result);
                }
            }

            return dic;
        }

        public static INSiteStatus GetItemSiteStatus(PXGraph graph, int? siteId, int? inventoryId, int? subItemId)
        {
            if (subItemId != null && InventoryHelper.SubItemFeatureEnabled)
            {
                return PXSelect<
                    INSiteStatus,
                        Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                        And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>,
                            And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>>>>>
                    .SelectWindowed(graph, 0, 1, inventoryId, siteId, subItemId);
            }

            return PXSelectGroupBy<
                    INSiteStatus,
                    Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                        And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>,
                    Aggregate<
                        Sum<INSiteStatus.qtyOnHand,
                            Sum<INSiteStatus.qtyAvail,
                                Sum<INSiteStatus.qtyNotAvail>>>>>
                .Select(graph, inventoryId, siteId);
        }

        private static List<INItemPlanWithLinkQty> GetItemPlansLessEqualDate(PXGraph graph, List<INItemPlanWithLinkQty> itemPlans, DateTime date)
        {
            if (itemPlans == null || itemPlans.Count == 0)
            {
                return itemPlans;
            }

            return itemPlans.Where(x => x.PlanDate != null && x.PlanDate != Common.Dates.BeginOfTimeDate && x.PlanDate.LessThanOrEqualTo(date)).ToList();
        }

        private static List<INItemPlanWithLinkQty> GetItemPlansGreaterThanDate(PXGraph graph, List<INItemPlanWithLinkQty> itemPlans, DateTime date)
        {
            if (itemPlans == null || itemPlans.Count == 0)
            {
                return itemPlans;
            }

            return itemPlans.Where(x => x.PlanDate != null && x.PlanDate.GreaterThan(date)).ToList();
        }

        public static List<INItemPlanWithLinkQty> GetItemPlans(PXGraph graph, int? siteId, int? inventoryId, int? subItemId)
        {
            PXSelectBase<INItemPlanWithLinkQty> cmd = new PXSelect<INItemPlanWithLinkQty,
                Where<INItemPlanWithLinkQty.siteID, Equal<Required<INItemPlanWithLinkQty.siteID>>,
                    And<INItemPlanWithLinkQty.inventoryID, Equal<Required<INItemPlanWithLinkQty.inventoryID>>>>>(graph);

            var paramList = new List<object> { siteId, inventoryId };

            if (InventoryHelper.SubItemFeatureEnabled && subItemId != null)
            {
                //Note: there is no index on subitemid
                paramList.Add(subItemId);
                cmd.WhereAnd<Where<INItemPlanWithLinkQty.subItemID, Equal<Required<INItemPlanWithLinkQty.subItemID>>>>();
            }

            return cmd.Select(paramList.ToArray()).ToFirstTableList();
        }
    }

    /// <summary>
    /// Aggregate of demand items by Hard linked Supply
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select5<INItemPlan,
        InnerJoin<INPlanType, On<INItemPlan.planType, Equal<INPlanType.planType>>>,
        Where<INPlanType.isDemand, Equal<True>,
            And<INItemPlan.supplyPlanID, IsNotNull>>,
        Aggregate<
            GroupBy<INItemPlan.inventoryID,
                GroupBy<INItemPlan.supplyPlanID,
                    Sum<INItemPlan.planQty>>>>>))]
    public class INItemPlanHardSupplyPlanAggregate : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [AnyInventory(IsKey = true, BqlField = typeof(INItemPlan.inventoryID))]
        [PXDefault]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SupplyPlanID
        public abstract class supplyPlanID : PX.Data.BQL.BqlLong.Field<supplyPlanID> { }

        protected Int64? _SupplyPlanID;
        [PXDBLong(BqlField = typeof(INItemPlan.supplyPlanID))]
        //[PXSelector(typeof(Search<INItemPlan.planID>), DirtyRead = true)]
        public virtual Int64? SupplyPlanID
        {
            get
            {
                return this._SupplyPlanID;
            }
            set
            {
                this._SupplyPlanID = value;
            }
        }
        #endregion
        #region PlanDate
        public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

        protected DateTime? _PlanDate;
        [PXDBDate(BqlField = typeof(INItemPlan.planDate))]
        [PXDefault]
        [PXUIField(DisplayName = "Planned On")]
        public virtual DateTime? PlanDate
        {
            get
            {
                return this._PlanDate;
            }
            set
            {
                this._PlanDate = value;
            }
        }
        #endregion
        #region HardPlanQty
        public abstract class hardPlanQty : PX.Data.BQL.BqlDecimal.Field<hardPlanQty> { }

        [PXDBQuantity(BqlField = typeof(INItemPlan.planQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Hard Planned Qty.")]
        public virtual Decimal? HardPlanQty { get; set; }
        #endregion
    }

    /// <summary>
    /// Aggregate of demand items by Soft (no hard link) Supply
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select5<INItemPlan,
        InnerJoin<INPlanType, On<INItemPlan.planType, Equal<INPlanType.planType>>,
        InnerJoin<INItemPlanAMExtension, On<INItemPlan.planID, Equal<INItemPlanAMExtension.planID>>>>,
        Where<INPlanType.isDemand, Equal<True>,
            And<INItemPlanAMExtension.aMSoftSupplyPlanID, IsNotNull>>,
        Aggregate<
            GroupBy<INItemPlan.inventoryID,
                GroupBy<INItemPlanAMExtension.aMSoftSupplyPlanID,
                    Sum<INItemPlan.planQty>>>>>))]
    public class INItemPlanSoftSupplyPlanAggregate : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [AnyInventory(IsKey = true, BqlField = typeof(INItemPlan.inventoryID))]
        [PXDefault]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SoftSupplyPlanID
        public abstract class softSupplyPlanID : PX.Data.BQL.BqlLong.Field<softSupplyPlanID> { }

        protected Int64? _SoftSupplyPlanID;
        [PXDBLong(BqlField = typeof(INItemPlanAMExtension.aMSoftSupplyPlanID))]
        //[PXSelector(typeof(Search<INItemPlan.planID>), DirtyRead = true)]
        public virtual Int64? SoftSupplyPlanID
        {
            get
            {
                return this._SoftSupplyPlanID;
            }
            set
            {
                this._SoftSupplyPlanID = value;
            }
        }
        #endregion
        #region PlanDate
        public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

        protected DateTime? _PlanDate;
        [PXDBDate(BqlField = typeof(INItemPlan.planDate))]
        [PXDefault]
        [PXUIField(DisplayName = "Planned On")]
        public virtual DateTime? PlanDate
        {
            get
            {
                return this._PlanDate;
            }
            set
            {
                this._PlanDate = value;
            }
        }
        #endregion
        #region SoftPlanQty
        public abstract class softPlanQty : PX.Data.BQL.BqlDecimal.Field<softPlanQty> { }

        [PXDBQuantity(BqlField = typeof(INItemPlan.planQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Soft Planned Qty.")]
        public virtual Decimal? SoftPlanQty { get; set; }
        #endregion
    }

    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select2<INItemPlan,
        InnerJoin<INPlanType, On<INItemPlan.planType, Equal<INPlanType.planType>>,
        LeftJoin<INItemPlanHardSupplyPlanAggregate, On<INItemPlan.planID, Equal<INItemPlanHardSupplyPlanAggregate.supplyPlanID>>,
            LeftJoin<INItemPlanSoftSupplyPlanAggregate, On<INItemPlan.planID, Equal<INItemPlanSoftSupplyPlanAggregate.softSupplyPlanID>>>>>>),
        /* INItemPlan is only persistent */
            new[] { typeof(INItemPlan) })]
    public class INItemPlanWithLinkQty : PX.Objects.IN.INItemPlan
    {
        /* IsKey / Db Key */
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        /* Db Key only */
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }


        /* IsKey / Db Key */
        public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        #region IsSupply (INPlanType.isSupply)
        public abstract class isSupply : PX.Data.BQL.BqlBool.Field<isSupply> { }

        protected Boolean? _IsSupply;
        [PXDBBool(BqlField = typeof(INPlanType.isSupply))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Supply", Enabled = false)]
        public virtual Boolean? IsSupply
        {
            get
            {
                return this._IsSupply;
            }
            set
            {
                this._IsSupply = value;
            }
        }
        #endregion
        #region IsDemand (INPlanType.isDemand)
        public abstract class isDemand : PX.Data.BQL.BqlBool.Field<isDemand> { }

        protected Boolean? _IsDemand;
        [PXDBBool(BqlField = typeof(INPlanType.isDemand))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Demand", Enabled = false)]
        public virtual Boolean? IsDemand
        {
            get
            {
                return this._IsDemand;
            }
            set
            {
                this._IsDemand = value;
            }
        }
        #endregion
        #region SupplyHardPlanQty (INItemPlanHardSupplyPlanAggregate.planQty)
        public abstract class supplyHardPlanQty : PX.Data.BQL.BqlDecimal.Field<supplyHardPlanQty> { }

        [PXDBQuantity(BqlField = typeof(INItemPlanHardSupplyPlanAggregate.hardPlanQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Supply Hard Planned Qty.")]
        public virtual Decimal? SupplyHardPlanQty { get; set; }
        #endregion
        #region SupplySoftPlanQty (INItemPlanSoftSupplyPlanAggregate.planQty)
        public abstract class supplySoftPlanQty : PX.Data.BQL.BqlDecimal.Field<supplySoftPlanQty> { }

        [PXDBQuantity(BqlField = typeof(INItemPlanSoftSupplyPlanAggregate.softPlanQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Supply Soft Planned Qty.")]
        public virtual Decimal? SupplySoftPlanQty { get; set; }
        #endregion
    }

    internal sealed class MaterialAvailabiltyDates
    {
        public readonly AMProdMatl ProdMatl;
        public readonly string MaterialOperationCD;
        public readonly InventoryItem MatlInventoryItem;
        /// <summary>
        /// All related split records
        /// </summary>
        public List<AMProdMatlSplit> AllProdMatlSplits;
        /// <summary>
        /// Material records being marked as allocated
        /// </summary>
        public List<AMProdMatlSplit> AllocatingProdMatlSplits;
        /// <summary>
        /// The schedule date used for the material calculation
        /// </summary>
        public readonly DateTime ScheduleDate;
        /// <summary>
        /// Material is fully available on this day on or before the schedule date
        /// </summary>
        public DateTime? AvailableDate;
        /// <summary>
        /// Quantity short for scheduling on the first late available date
        /// </summary>
        public decimal? AvailableBaseQtyShort;
        /// <summary>
        /// Material is due in after the schedule date and first fully available on this day
        /// </summary>
        public DateTime? FirstLateAvailableDate;
        /// <summary>
        /// Quantity short for scheduling on the first late available date
        /// </summary>
        public decimal? FirstLateAvailableBaseQtyShort;
        /// <summary>
        /// Stock inventory quantity used to supply material
        /// </summary>
        public bool UsedStockInventory;
        /// <summary>
        /// The material is short to be supplied by the schedule date
        /// </summary>
        public bool ShortByScheduleDate => AvailableDate == null;
        /// <summary>
        /// The maerial is short to be supplied for any date
        /// </summary>
        public bool NoAvailableMaterial => AvailableDate == null && FirstLateAvailableDate == null;

        public MaterialAvailabiltyDates(AMProdMatl prodMatl, string operationCD, InventoryItem inventoryItem, DateTime scheduleDate, List<AMProdMatlSplit> allProdMatlSplits)
        {
            if (string.IsNullOrWhiteSpace(prodMatl?.ProdOrdID))
            {
                throw new ArgumentNullException(nameof(prodMatl));
            }

            ProdMatl = prodMatl;
            MatlInventoryItem = inventoryItem ?? throw new ArgumentNullException(nameof(inventoryItem));
            ScheduleDate = scheduleDate;
            UsedStockInventory = false;
            AllocatingProdMatlSplits = new List<AMProdMatlSplit>();
            AllProdMatlSplits = allProdMatlSplits ?? new List<AMProdMatlSplit>();
            MaterialOperationCD = operationCD;
        }
    }
}