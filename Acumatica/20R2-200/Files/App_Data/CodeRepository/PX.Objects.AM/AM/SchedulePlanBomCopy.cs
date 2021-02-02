using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Calculate the bom operations and the required schedule for a plan item (not an actual order compared to ProductionBomCopy)
    /// </summary>
    public class SchedulePlanBomCopy : ProductionBomCopyBase
    {
        public PXSelect<AMRPAuditTable> MrpAudit;

        public bool MachineScheduling => CurrentSetup?.MachineScheduling == true;
        public Guid? RefNoteID;

        protected DateUsedTime LastDateUsedTime;

        /// <summary>
        /// Scheduling block size (in minutes) for APS
        /// </summary>
        public int BlockSize => CurrentSetup?.SchdBlockSize ?? 0;

        public override void Persist()
        {
            var insertedMrpAudit = new List<AMRPAuditTable>();
            foreach (AMRPAuditTable row in MrpAudit.Cache.Inserted)
            {
                insertedMrpAudit.Add(row);
            }

            //We are only using MrpAudit for cache storage to pull the values from the parent graph. We dont want them to save here.
            ClearMrpAudit();

            base.Persist();

            foreach (var row in insertedMrpAudit)
            {
                MrpAudit.Insert(row);
            }
        }

        public void ClearMrpAudit()
        {
            MrpAudit.Cache.Clear();
            MrpAudit.Cache.ClearQueryCache();
        }

        protected DateTime CurrentPlanDate
        {
            get
            {
                if (CurrentSchdOper == null)
                {
                    if (Order.Current == null)
                    {
                        return BusinessDate;
                    }

                    return Order.Current.PlanDate.GetValueOrDefault(BusinessDate);
                }

                DateTime d = CurrentSchdOper.StartDate.GetValueOrDefault(BusinessDate);

                if (Common.Dates.IsDefaultDate(d))
                {
                    return BusinessDate;
                }

                return d;
            }
        }

        public override void Clear()
        {
            base.Clear();

            Reset();
        }

        protected virtual void Reset()
        {
            //Reset schedule stuff
            LastDateUsedTime = null;

            CurrentSchdOper = null;
            CurrentSchdItem = null;
            CurrentWCSchd = null;
            CurrentWCSchdDetail = null;
            CurrentSchdOperDetail = null;
        }

        public virtual AMSchdItem MakeSchdItem(AMOrder order)
        {
            return SetSchdItem(order, new AMSchdItem
            {
                OrderType = order.OrderType,
                ProdOrdID = order.OrderNbr
            });
        }

        private AMSchdItem SetSchdItem(AMOrder order, AMSchdItem schdItem)
        {
            schdItem.ConstDate = order.PlanDate;
            schdItem.IsPlan = true;
            schdItem.IsMRP = true;
            schdItem.InventoryID = order.InventoryID;
            schdItem.StartDate = order.PlanDate;
            schdItem.EndDate = order.PlanDate;
            schdItem.QtytoProd = order.OrderQty.GetValueOrDefault();
            schdItem.SchedulingMethod = ScheduleMethod.FinishOn;
            schdItem.SiteID = order.SiteID;
            schdItem.RefNoteID = RefNoteID;

            return schdItem;
        }

        public virtual void CreateSchedule(AMOrder order)
        {
            CreateSchedule(order, (AMSchdItem)null);
        }

        public virtual void CreateSchedule(AMOrder order, AMSchdItem existingSchdItem)
        {
            CreateSchedule(order, existingSchdItem, null);
        }

        public virtual void CreateSchedule(AMOrder order, bool useFixLeadTime)
        {
            var lt = useFixLeadTime ? (int?)InventoryHelper.GetFixMfgLeadTime(ProcessingGraph, order.InventoryID, order.SiteID) : null;
            CreateSchedule(order, lt);
        }

        public virtual void CreateSchedule(AMOrder order, int? fixLeadTime)
        {
            CreateSchedule(order, null, fixLeadTime);
        }

        public virtual void CreateSchedule(AMOrder order, AMSchdItem existingSchdItem, int? fixLeadTime)
        {
            if (order == null)
            {
                return;
            }

            CreateOperationDetail(order);

            if (Order.Current == null)
            {
                throw new PXException(nameof(order));
            }

            var reuseSchedule = existingSchdItem != null;
            var schdItem = reuseSchedule 
                ? SetSchdItem(order, PXCache<AMSchdItem>.CreateCopy(existingSchdItem)) 
                : MakeSchdItem(order);

            CurrentSchdItem = schdItem;

            if (fixLeadTime != null)
            {
                if (reuseSchedule && ProcessingGraph.Caches<AMSchdItem>().GetStatus(schdItem) == PXEntryStatus.Notchanged)
                {
                    ProcessingGraph.Caches<AMSchdItem>().SetStatus(schdItem, PXEntryStatus.Updated);
                }

                //Fixed lead times still needs the operation detail to build to determine which material is used (phantoms, etc.) for planning
                CreateFixLeadTimeSchedule(order, fixLeadTime.GetValueOrDefault());
                return;
            }

            //Process calculated operation numbers in descending order (schedule method always finish on)
            foreach (var operationDetail in _operationDetails.OrderedList.OrderByDescending(x => x.ProdOperationCD))
            {
                var schdOper = ScheduleOperation(operationDetail);

                if (schdOper == null)
                {
                    continue;
                }

                if (schdOper.StartDate != null && Common.Dates.Compare(schdItem.StartDate ?? Common.Dates.EndOfTimeDate, schdOper.StartDate) > 0)
                {
                    schdItem.StartDate = schdOper.StartDate;
                }
                if (schdOper.EndDate != null && Common.Dates.Compare(schdItem.EndDate ?? Common.Dates.BeginOfTimeDate, schdOper.EndDate) < 0)
                {
                    schdItem.EndDate = schdOper.EndDate;
                }
            }

            if (reuseSchedule)
            {
                CurrentSchdItem = (AMSchdItem)ProcessingGraph.Caches<AMSchdItem>().Update(schdItem);
                return;
            }

            CurrentSchdItem = (AMSchdItem)ProcessingGraph.Caches<AMSchdItem>().Insert(schdItem);
        }

        protected virtual AMSchdOper ScheduleOperation(OperationDetail operationDetail)
        {
            if (operationDetail == null || !operationDetail.IncludeOper)
            {
                return null;
            }

            var amBomOper = operationDetail.BomOper ?? PXSelect<AMBomOper,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
                    And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID);

            if (amBomOper == null)
            {
                return null;
            }

            var amSchdOper = new AMSchdOper
            {
                OrderType = Order.Current.OrderType,
                ProdOrdID = Order.Current.OrderNbr,
                OperationID = operationDetail.SortOrder,
                SortOrder = operationDetail.SortOrder,
                SiteID = Order.Current.SiteID,
                LineNbr = 0,
                IsPlan = true,
                IsMRP = true,
                QueueTime = amBomOper.QueueTime,
                WcID = amBomOper.WcID,
                MoveTime = CurrentSetup?.DefaultMoveTime ?? 0,
                SchedulingMethod = ScheduleMethod.FinishOn,
                ConstDate = CurrentPlanDate,
                QtytoProd = Order.Current.OrderQty
            };

            decimal priorLevelQty = operationDetail.BomQtyReq <= 0 ? 1 : operationDetail.BomQtyReq;

            var amSchdOper2 = (AMSchdOper)ProcessingGraph.Caches<AMSchdOper>().Insert(CreateScheduleDetail(amSchdOper, amBomOper, priorLevelQty));
            CurrentSchdOper = amSchdOper2;

            return amSchdOper2;
        }


        /// <summary>
        /// Creates the Daily schedule detail per operation
        /// </summary>
        /// <param name="amSchdOper">New plan operation to schedule</param>
        /// <param name="sourceAmBomOper">Source bom operation record</param>
        /// <returns>The sourced schedule operation with updated details</returns>
        protected virtual AMSchdOper CreateScheduleDetail(AMSchdOper amSchdOper, AMBomOper sourceAmBomOper, decimal phtmpriorlevelqty)
        {
            if (amSchdOper == null)
            {
                throw new ArgumentNullException(nameof(amSchdOper));
            }

            if (sourceAmBomOper == null)
            {
                throw new ArgumentNullException(nameof(sourceAmBomOper));
            }

            AMWC amWorkCenter = PXSelect<AMWC, Where<AMWC.wcID, Equal<Required<AMWC.wcID>>>>.Select(ProcessingGraph, amSchdOper.WcID);

            if (amWorkCenter == null)
            {
                throw new PXException(Messages.RecordMissingWithID, Common.Cache.GetCacheName(typeof(AMWC)), amSchdOper.WcID);
            }

            var fgQty = amSchdOper.QtytoProd.GetValueOrDefault() * phtmpriorlevelqty;
            var totalTime = ProductionScheduleEngine.CalcTotalTime(amWorkCenter, sourceAmBomOper, fgQty);
            totalTime += sourceAmBomOper.SetupTime.GetValueOrDefault();
            if (totalTime < 0)
            {
                totalTime = 0;
            }
            amSchdOper.TotalPlanTime = totalTime;
            
            var schdBackwards = amSchdOper.SchedulingMethod == ScheduleMethod.FinishOn;
            var currentDateTime = amSchdOper.ConstDate.GetValueOrDefault(BusinessDate);

            AMShift amShift = PXSelect<AMShift, Where<AMShift.wcID, Equal<Required<AMShift.wcID>>>>.SelectWindowed(ProcessingGraph, 0, 1, amSchdOper.WcID);

            if (amShift?.CalendarID == null)
            {
                throw new PXException(Messages.RecordMissingWithID, Common.Cache.GetCacheName(typeof(AMShift)), amSchdOper.WcID);
            }

            var calendarHelper = new CalendarHelper(ProcessingGraph, amShift.CalendarID)
            {
                CalendarReadDirection = schdBackwards
                    ? ReadDirection.Backward
                    : ReadDirection.Forward
            };

            if (totalTime <= 0)
            {
                DateTime d = calendarHelper.GetNextWorkDay(currentDateTime, true) ?? currentDateTime;

                //If no hours found
                if (amSchdOper.StartDate == null)
                {
                    amSchdOper.StartDate = d;
                }
                if (amSchdOper.EndDate == null)
                {
                    amSchdOper.EndDate = d;
                }
            }

            while (totalTime > 0)
            {
                currentDateTime = calendarHelper.GetNextWorkDay(currentDateTime, true) ?? currentDateTime;
                AMDateInfo dateInfo = calendarHelper.GetDateInfo(currentDateTime);

                if (Common.Dates.IsDefaultDate(currentDateTime))
                {
                    break;
                }

                var usedTime = LastDateUsedTime != null && DateTime.Compare(LastDateUsedTime.Date, dateInfo.Date) == 0 ? LastDateUsedTime.UsedTime : 0;
                var availTime = dateInfo.WorkingMinutes - usedTime;

                var schdMinutes = 0;
                if (availTime >= totalTime)
                {
                    schdMinutes = totalTime;
                }
                else
                {
                    if (availTime > 0)
                    {
                        schdMinutes = availTime;
                    }
                    else
                    {
                        availTime = 0;
                    }
                }

                if (schdMinutes > 0)
                {
                    var schdOperDetail = ProductionScheduleEngine.CreateSchdOperDetail(amSchdOper, currentDateTime);
                    GetWorkCenterSchd(amSchdOper.WcID, amShift.ShiftID, amSchdOper.SiteID, dateInfo);
                    var wcSchdDetail = ProductionScheduleEngine.CreateWCSchdDetail(amSchdOper, currentDateTime, amShift.ShiftID);

                    if (schdOperDetail != null && wcSchdDetail != null)
                    {
                        wcSchdDetail.SchdKey = Guid.NewGuid();
                        wcSchdDetail.SchdTime = schdMinutes;
                        wcSchdDetail.SchdEfficiencyTime = schdMinutes;
                        wcSchdDetail.PlanBlocks =
                            ProductionScheduleEngine.MinutesToBlocks(wcSchdDetail.SchdTime.GetValueOrDefault(), BlockSize, true);
                        wcSchdDetail.SchdBlocks = 0; // only finite sets schd blocks

                        schdOperDetail.SchdKey = wcSchdDetail.SchdKey;
                        schdOperDetail.SchdTime = wcSchdDetail.SchdTime.GetValueOrDefault();

                        ProcessingGraph.Caches<AMWCSchdDetail>().Insert(wcSchdDetail);
                        ProcessingGraph.Caches<AMSchdOperDetail>().Insert(schdOperDetail);
                    }

                    //Init dates here
                    if (amSchdOper.StartDate == null)
                    {
                        amSchdOper.StartDate = currentDateTime;
                    }
                    if (amSchdOper.EndDate == null)
                    {
                        amSchdOper.EndDate = currentDateTime;
                    }

                    //Keep track of schd oper start/end dates here
                    if (schdBackwards && currentDateTime < (amSchdOper.StartDate ?? currentDateTime))
                    {
                        amSchdOper.StartDate = currentDateTime;
                    }
                    if (!schdBackwards && currentDateTime > (amSchdOper.EndDate ?? currentDateTime))
                    {
                        amSchdOper.EndDate = currentDateTime;
                    }
                }

                var sameDayUsedTime = LastDateUsedTime != null &&
                                           DateTime.Compare(LastDateUsedTime.Date, currentDateTime) == 0
                    ? LastDateUsedTime.UsedTime
                    : 0;

                LastDateUsedTime = new DateUsedTime(currentDateTime, schdMinutes + sameDayUsedTime);

                if (availTime <= totalTime)
                {
                    totalTime -= availTime;
                    currentDateTime = calendarHelper.NextDateTime(currentDateTime);
                    //usedHours = 0m;
                }
                else
                {
                    totalTime = 0;
                }
            }

            return amSchdOper;
        }

        protected override AMBomItem GetOrderBomItem(string bomId, string revisionId)
        {
            //For MRP schedule we really do not care if there is a bom record (avoid trip to DB). The check after this for pulling operations will come back empty and that is enough
            if (BomItem?.Current?.RevisionID != null &&
                BomItem.Current.BOMID.EqualsWithTrim(bomId) &&
                BomItem.Current.RevisionID.EqualsWithTrim(revisionId))
            {
                return BomItem?.Current;
            }

            return new AMBomItem { BOMID = bomId, RevisionID = revisionId };
        }

        protected virtual AMWCSchd GetWorkCenterSchd(string workCenterID, string shiftID, int? siteID, AMDateInfo dateInfo)
        {
            if (string.IsNullOrWhiteSpace(workCenterID))
            {
                throw new ArgumentNullException(nameof(workCenterID));
            }

            if (string.IsNullOrWhiteSpace(shiftID))
            {
                throw new ArgumentNullException(nameof(shiftID));
            }

            if (siteID == null)
            {
                throw new ArgumentNullException(nameof(siteID));
            }

            if (Common.Dates.IsDefaultDate(dateInfo.Date))
            {
                throw new ArgumentException(nameof(dateInfo));
            }

            AMWCSchd workCenterSchd = PXSelect<AMWCSchd,
                    Where<AMWCSchd.wcID, Equal<Required<AMWCSchd.wcID>>,
                    And<AMWCSchd.shiftID, Equal<Required<AMWCSchd.shiftID>>,
                    And<AMWCSchd.schdDate, Equal<Required<AMWCSchd.schdDate>>>>>
                    >.Select(ProcessingGraph, workCenterID, shiftID, dateInfo.Date);

#if DEBUG
            //TODO: this should be total start/end time blocks and then schedule by removing minutes for break time. For now we will make it working time (which already excludes break times) 
#endif
            var totalBlocks = ProductionScheduleEngine.MinutesToBlocks(dateInfo.WorkingMinutes, BlockSize, false);
            if (workCenterSchd == null)
            {
                workCenterSchd = new AMWCSchd
                {
                    WcID = workCenterID,
                    ShiftID = shiftID,
                    SchdDate = dateInfo.Date,
                    SiteID = siteID,
                    TotalBlocks = totalBlocks
                };
                workCenterSchd = (AMWCSchd)ProcessingGraph.Caches<AMWCSchd>().Insert(workCenterSchd);
            }

            //in case the total Blocks changed, update the available blocks.
            if (workCenterSchd.TotalBlocks.GetValueOrDefault() != totalBlocks)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName(
                    $"Adjusting the total blocks for wc '{workCenterID.TrimIfNotNullEmpty()}' shift '{shiftID.TrimIfNotNullEmpty()}' by '{totalBlocks - workCenterSchd.TotalBlocks.GetValueOrDefault()}'");
#endif
                workCenterSchd.TotalBlocks = totalBlocks;
            }

            workCenterSchd.WorkTime = dateInfo.WorkingMinutes;
            workCenterSchd.StartTime = new DateTime(1900, 1, 1) + dateInfo.StartTime;
            workCenterSchd.EndTime = new DateTime(1900, 1, 1) + dateInfo.EndTime;
            workCenterSchd.ExceptionDate = dateInfo.IsExceptionDate;

            CurrentWCSchd = workCenterSchd;
            return workCenterSchd;
        }

        protected virtual string GetMachID(AMSchdOper amSchdOper)
        {
            if (amSchdOper == null || string.IsNullOrWhiteSpace(amSchdOper.WcID))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(amSchdOper.MachID))
            {
                return amSchdOper.MachID;
            }

            AMWCMach amwcMach = PXSelectJoin<AMWCMach
                , InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>>
                , Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>
                , And<AMMach.activeFlg, Equal<True>
                , And<AMMach.downFlg, Equal<False>>>>
                    >.Select(ProcessingGraph, amSchdOper.WcID);

            if (amwcMach != null && !string.IsNullOrWhiteSpace(amwcMach.MachID))
            {
                return amwcMach.MachID ?? string.Empty;
            }

            return string.Empty;
        }

        protected virtual void CreateFixLeadTimeSchedule(AMOrder order)
        {
            CreateFixLeadTimeSchedule(order , InventoryHelper.GetFixMfgLeadTime(ProcessingGraph, order.InventoryID, order.SiteID));
        }

        protected virtual void CreateFixLeadTimeSchedule(AMOrder order, int fixLeadTime)
        {
            if (order == null)
            {
                return;
            }

            if (fixLeadTime < 0)
            {
                fixLeadTime = 0;
            }

            if (CurrentSetup?.FixMfgCalendarID == null)
            {
                throw new PXException(Messages.MissingFixMfgCalendar);
            }

            var planDate = order.PlanDate.GetValueOrDefault();
            var schdConst = ScheduleMethod.FinishOn;
            var scheduleBack = schdConst == ScheduleMethod.FinishOn;
            var calendar = CalendarHelper.SelectCalendar(ProcessingGraph, CurrentSetup?.FixMfgCalendarID);
            var calendarHelper = new CalendarHelper(ProcessingGraph, calendar)
            {
                CalendarReadDirection = scheduleBack
                    ? ReadDirection.Backward
                    : ReadDirection.Forward
            };

            var firstDate = calendarHelper.GetNextWorkDay(planDate, true);
            var lastDate = firstDate ?? planDate;
            var leadTimeUnits = CurrentSetup?.FMLTimeUnits ?? TimeUnits.Days;
            decimal usedUnits = 0;
            if (firstDate != null && usedUnits != fixLeadTime)
            {
                int cntr = 0;
                while (cntr < 999)
                {
                    var nextDateTime = calendarHelper.GetNextWorkDay(lastDate, true);

                    if (nextDateTime == null)
                    {
                        break;
                    }

                    lastDate = nextDateTime.GetValueOrDefault();

                    AMDateInfo dateInfo = calendarHelper.GetDateInfo(lastDate);
                    usedUnits += leadTimeUnits == TimeUnits.Hours ? dateInfo.WorkingHoursDecimal : 1;

                    if (usedUnits >= fixLeadTime)
                    {
                        break;
                    }

                    lastDate = calendarHelper.NextDateTime(lastDate);
                    cntr++;
                }
            }

            if (firstDate == null)
            {
                firstDate = lastDate;
            }

            if (CurrentSchdItem != null)
            {
                var schdItem = CurrentSchdItem;
                schdItem.StartDate = calendarHelper.CalendarReadDirection == ReadDirection.Forward ? firstDate : lastDate;
                schdItem.EndDate = calendarHelper.CalendarReadDirection == ReadDirection.Forward ? lastDate : firstDate;

                var schdItemStatus = ProcessingGraph.Caches<AMSchdItem>().GetStatus(schdItem);
                if (schdItemStatus == PXEntryStatus.Updated || schdItemStatus == PXEntryStatus.Inserted)
                {
                    CurrentSchdItem = (AMSchdItem)ProcessingGraph.Caches<AMSchdItem>().Update(schdItem);
                    return;
                }

                if (schdItemStatus != PXEntryStatus.Notchanged)
                {
                    return;
                }
                
                CurrentSchdItem = (AMSchdItem) ProcessingGraph.Caches<AMSchdItem>().Insert(schdItem);
            }
        }

        public virtual List<AMPlanMaterial> GetFixLeadTimeOrderMaterial()
        {
            return GetOrderMaterial(CurrentSchdItem?.StartDate);
        }

        /// <summary>
        /// Gets the material used in calculating the routing
        /// (Not for use when using Fixed Manufacturing Lead Times)
        /// </summary>
        /// <returns></returns>
        public virtual List<AMPlanMaterial> GetOrderMaterial()
        {
            return GetOrderMaterial(null);
        }

        protected virtual List<AMPlanMaterial> GetOrderMaterial(DateTime? materialDateTime)
        {
            var planMaterialList = new List<AMPlanMaterial>();

            if (_operationDetails?.OrderedList == null)
            {
                return planMaterialList;
            }

            // for fixed lead time and for large processing of records we can get all material for a bom at once vs trying to query by operation (improve performance)

            var uniqueBomRevisions = GetUniqueOperationDetailBomRevisions();
            if (uniqueBomRevisions == null)
            {
                return planMaterialList;
            }

            var bomMatlList = new List<AMBomMatl>();
            var bomItemList = new List<AMBomItem>();

            foreach (var bomRevision in uniqueBomRevisions)
            {
                var firstBomItem = true;
                foreach (PXResult<AMBomMatl, AMBomItem> result in PXSelectJoin<
                        AMBomMatl,
                        InnerJoin<AMBomItem,
                            On<AMBomMatl.bOMID, Equal<AMBomItem.bOMID>,
                                And<AMBomMatl.revisionID, Equal<AMBomItem.revisionID>>>>,
                        Where<AMBomMatl.bOMID, Equal<Required<AMBomOper.bOMID>>,
                            And<AMBomMatl.revisionID, Equal<Required<AMBomOper.revisionID>>>>>
                    .Select(ProcessingGraph, bomRevision.Item1, bomRevision.Item2))
                {
                    var bomMatl = (AMBomMatl)result;
                    var bomItem = (AMBomItem)result;

                    if (bomMatl?.RevisionID == null ||
                        bomItem?.RevisionID == null)
                    {
                        continue;
                    }

                    if (firstBomItem)
                    {
                        bomItemList.Add(bomItem);
                        firstBomItem = false;
                    }

                    bomMatlList.Add(bomMatl);
                }
            }

            // Then we need to see if saving in a property is ok for now and then in 2020R2 update to just pass in the queried material list
            // (to avoid breaking anyone with a customization)

            foreach (var operationDetail in _operationDetails.OrderedList)
            {
                var materialPlanDate = materialDateTime ?? BusinessDate;
                if (materialDateTime == null)
                {
                    AMSchdOper amSchdOper = PXSelect<AMSchdOper,
                        Where<AMSchdOper.orderType, Equal<Required<AMSchdOper.orderType>>,
                            And<AMSchdOper.prodOrdID, Equal<Required<AMSchdOper.prodOrdID>>,
                                And<AMSchdOper.sortOrder, Equal<Required<AMSchdOper.sortOrder>>>>>
                    >.Select(ProcessingGraph, Order.Current.OrderType, Order.Current.OrderNbr, operationDetail.SortOrder);

                    if (amSchdOper != null)
                    {
                        materialPlanDate = amSchdOper.StartDate ?? BusinessDate;
                    }
                }

                var operationMaterial = bomMatlList
                    .Where(r => r.BOMID == operationDetail.BomID &&
                                r.RevisionID == operationDetail.BomRevisionID &&
                                r.OperationID == operationDetail.BomOperationID)
                    .ToList();

                if (operationMaterial.Count == 0)
                {
                    continue;
                }

                var operationBom = bomItemList
                    .FirstOrDefault(r => r.BOMID == operationDetail.BomID &&
                                r.RevisionID == operationDetail.BomRevisionID);

                if (operationBom?.BOMID == null)
                {
                    continue;
                }

                foreach (var bomMatl in operationMaterial)
                {
                    var planMatl = CreatePlanMaterial(bomMatl, operationBom, operationDetail, materialPlanDate);
                    if (planMatl == null)
                    {
                        continue;
                    }
                    planMaterialList.Add(planMatl);
                }
            }

            return planMaterialList;
        }

        protected virtual AMPlanMaterial CreatePlanMaterial(AMBomMatl bomMatl, AMBomItem bomItem, OperationDetail operationDetail, DateTime materialPlanDate)
        {
            if (bomMatl?.MaterialType == null || bomMatl.MaterialType != AMMaterialType.Regular ||
                bomItem?.RevisionID == null || operationDetail == null)
            {
                return null;
            }

            //Exclude expired/not yet effective material
            if (bomMatl.EffDate > CurrentPlanDate
                && !Common.Dates.DatesEqual(bomMatl.EffDate, Common.Dates.EndOfTimeDate)
                || bomMatl.ExpDate <= CurrentPlanDate
                && !Common.Dates.DatesEqual(bomMatl.ExpDate, Common.Dates.BeginOfTimeDate))
            {
                return null;
            }

            var planMaterial = new AMPlanMaterial
            {
                ProductInventoryID = Order.Current.InventoryID,
                ProductSubItemID = Order.Current.SubItemID,
                ParentInventoryID = bomItem.InventoryID,
                ParentSubItemID = bomItem.SubItemID,
                BFlush = bomMatl.BFlush,
                CompBOMID = bomMatl.CompBOMID,
                CompBOMRevisionID = bomMatl.CompBOMRevisionID,
                Descr = bomMatl.Descr,
                InventoryID = bomMatl.InventoryID,
                SubItemID = bomMatl.SubItemID,
                MaterialType = bomMatl.MaterialType,
                PhantomRouting = bomMatl.PhantomRouting,
                ScrapFactor = bomMatl.ScrapFactor,
                QtyReq = bomMatl.QtyReq,
                BaseQty = bomMatl.BaseQty,
                UOM = bomMatl.UOM,
                SiteID = bomMatl.SiteID ?? bomItem.SiteID,
                LocationID = bomMatl.SiteID == null ? null : bomMatl.LocationID,
                ProdOrdID = Order.Current.OrderNbr,
                PlanDate = materialPlanDate,
                BatchSize = bomMatl.BatchSize
            };

            if (!operationDetail.IsProdBom)
            {
                planMaterial.QtyReq = planMaterial.QtyReq * operationDetail.BomQtyReq;
                planMaterial.BaseQty = planMaterial.BaseQty * operationDetail.BomQtyReq;

                ProductionBomCopyMap.SetPhtmBomReferences(ref planMaterial, bomMatl, bomMatl.LineID);
                SetPhtmMatlReferences(ref planMaterial, operationDetail);
            }

            return planMaterial;
        }

        protected override int BuildOperationByBom(AMBomMatl parentAmBomMatl, int level, int currentOpIndex, decimal? parentQtyRequired)
        {
            try
            {
                return base.BuildOperationByBom(parentAmBomMatl, level, currentOpIndex, parentQtyRequired);
            }
            catch (InvalidBOMException ibe)
            {
                PXTrace.WriteWarning(ibe.Message);
                MrpAudit.Insert(new AMRPAuditTable {MsgText = ibe.Message});
            }

            return currentOpIndex;
        }

        /// <summary>
        /// <see cref="AMProdMatl"/> with additional reference fields
        /// </summary>
        [Serializable]
        [PXCacheName(Messages.PlanProductionMatl)]
        public class AMPlanMaterial : AMProdMatl
        {
            #region ParentInventoryID
            public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

            [PXDBInt]
            public virtual Int32? ParentInventoryID { get; set; }
            #endregion
            #region ParentSubItemID
            public abstract class parentSubItemID : PX.Data.BQL.BqlInt.Field<parentSubItemID> { }

            [PXDBInt]
            public virtual Int32? ParentSubItemID { get; set; }
            #endregion
            #region ProductInventoryID
            public abstract class productInventoryID : PX.Data.BQL.BqlInt.Field<productInventoryID> { }

            [PXDBInt]
            public virtual Int32? ProductInventoryID { get; set; }
            #endregion
            #region ProductSubItemID
            public abstract class productsubItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

            [PXDBInt]
            public virtual Int32? ProductSubItemID { get; set; }
            #endregion
            #region PlanDate
            public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

            /// <summary>
            /// The date the material is needed
            /// </summary>
            [PXDBDate]
            public virtual DateTime? PlanDate { get; set; }
            #endregion
        }

        protected class DateUsedTime
        {
            public readonly DateTime Date;
            public readonly int UsedTime;

            public DateUsedTime(DateTime date, int usedTime)
            {
                Date = date;
                UsedTime = usedTime;
            }
        }

    }
}