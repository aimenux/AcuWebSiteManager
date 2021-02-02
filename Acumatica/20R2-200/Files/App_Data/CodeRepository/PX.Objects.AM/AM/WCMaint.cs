using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Work Center Maintenance
    /// </summary>
    public class WCMaint : PXGraph<WCMaint, AMWC>
    {
        public PXSelect<AMWC> WCRecords;
        public PXSelect<AMWC, Where<AMWC.wcID, Equal<Current<AMWC.wcID>>>> WCRecordsSelected;

        public PXSelectJoin<AMShift, InnerJoin<AMShiftMst, On<AMShift.shiftID, Equal<AMShiftMst.shiftID>>>,
            Where<AMShift.wcID, Equal<Current<AMWC.wcID>>>> WCShifts;

        public PXSelectJoin<AMWCOvhd, InnerJoin<AMOverhead, On<AMWCOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
            Where<AMWCOvhd.wcID, Equal<Current<AMWC.wcID>>>> WCOverheads;

        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<
            AMBomOper, 
            InnerJoin<AMBomItem, 
                On<AMBomOper.bOMID, Equal<AMBomItem.bOMID>,
                And<AMBomOper.revisionID, Equal<AMBomItem.revisionID>>>>,
            Where<AMBomOper.wcID, Equal<Current<AMWC.wcID>>>> WCWhereUsed;

        public PXSelect<AMWCMach, Where<AMWCMach.wcID, Equal<Current<AMWC.wcID>>>> WCMachines;

        public PXFilter<CalendarWeek> CalendarInquiryFilter;

        public PXFilter<WorkCenterUpdateFilter> MassUpdateFilter;

        public PXSelect<AMWCSubstitute, Where<AMWCSubstitute.wcID, Equal<Current<AMWC.wcID>>>> SubstituteWorkCenters;

        public PXSetup<AMBSetup> Setup;

        public WCMaint()    
        {
            var bomSetup = Setup.Current;
            if (string.IsNullOrWhiteSpace(bomSetup?.BOMNumberingID))
            {
                throw new BOMSetupNotEnteredException();
            }
            
            InquiresMenu.AddMenuAction(CalendarInquiry);
            InquiresMenu.AddMenuAction(WcDispatch);
            InquiresMenu.AddMenuAction(ScheduleInquiry);
            InquiresMenu.AddMenuAction(CapacityInquiry);
            // Inquiry cache...
            WCWhereUsed.AllowDelete = false;
            WCWhereUsed.AllowInsert = false;
            PXUIFieldAttribute.SetVisible<AMBomOper.bOMID>(WCWhereUsed.Cache, null);

            SubstituteWorkCenters.AllowSelect = InventoryHelper.MultiWarehousesFeatureEnabled;
        }

        public override void Persist()
        {
            var wc = WCRecords.Current;
            var status = WCRecords.Cache.GetStatus(wc);
            if (wc != null && (status == PXEntryStatus.Updated || status == PXEntryStatus.Inserted))
            {
                var origRow = (AMWC)WCRecords.Cache.GetOriginal(wc);
                if (origRow?.SiteID != null && !WCRecords.Cache.ObjectsEqual<AMWC.siteID>(wc, origRow))
                {
                    DeleteSubstitue(wc.WcID, wc.SiteID);
                }
            }

            base.Persist();
        }

        protected virtual void AMWC_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            InsertDefaultShiftRow((AMWC) e.Row);
        }

        protected virtual void InsertDefaultShiftRow(AMWC row)
        {
            if (string.IsNullOrWhiteSpace(row?.WcID) || IsImport || IsContractBasedAPI || IsCopyPasteContext || WCRecords.Cache.GetStatus(row) != PXEntryStatus.Inserted || WCShifts.Cache.Inserted.Any_())
            {
                return;
            }

            var firstShift = (AMShiftMst)PXSelect<AMShiftMst>.SelectWindowed(this, 0, 1);

            if (string.IsNullOrWhiteSpace(firstShift?.ShiftID))
            {
                return;
            }

            PXResultset<AMLaborCode> laborCodeResult = PXSelect<AMLaborCode, Where<AMLaborCode.laborType, Equal<AMLaborType.direct>>>.SelectWindowed(this, 0, 2);
            string laborCodeId = null;
            if (laborCodeResult != null && laborCodeResult.Count == 1)
            {
                laborCodeId = ((AMLaborCode) laborCodeResult[0]).LaborCodeID;
            }

            WCShifts.Insert(new AMShift
            {
                WcID = row.WcID,
                ShiftID = firstShift.ShiftID,
                LaborCodeID = laborCodeId
            });
        }

        protected virtual void AMWC_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var allow = ((AMWC)e.Row)?.WcID != null;

            WCShifts.AllowInsert = allow;
            WCOverheads.AllowInsert = allow;
            WCMachines.AllowInsert = allow;
            SubstituteWorkCenters.AllowInsert = allow;
            CalendarInquiry.SetEnabled(allow);
            ScheduleInquiry.SetEnabled(allow);
            CapacityInquiry.SetEnabled(allow);

            var isEccRestricted = Setup?.Current?.ForceECR == true && Features.ECCEnabled();

            MassUpdate.SetEnabled(allow && !isEccRestricted);
        }

        protected virtual void AMWC_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var amwc = (AMWC)e.Row;
            if (WCShifts.Select().Count == 0)
            {
                sender.RaiseExceptionHandling<AMWC.wcID>(amwc, amwc?.WcID,
                    new PXSetPropertyException(Messages.WorkCenterMustHaveOneShift, PXErrorLevel.Error, amwc?.WcID));
            }
        }

        protected virtual void _(Events.RowDeleted<AMWC> e)
        {
            if (e.Row?.WcID == null || !InventoryHelper.MultiWarehousesFeatureEnabled)
            {
                return;
            }

            // Make sure if this deleting WC is ref as a sub we remove that too
            foreach (AMWCSubstitute sub in PXSelect<
                AMWCSubstitute, 
                Where<AMWCSubstitute.substituteWcID, Equal<Required<AMWCSubstitute.substituteWcID>>>>
                .Select(this, e.Row.WcID))
            {
                SubstituteWorkCenters.Delete(sub);
            }
        }

        protected virtual void _(Events.FieldUpdating<AMWC, AMWC.siteID> e)
        {
            if (e.Row?.SiteID == null || !InventoryHelper.MultiWarehousesFeatureEnabled || IsImport || IsContractBasedAPI)
            {
                return;
            }

            // Unfortunatly the new value is not the ID but the string of the warehouse
            var site = GetSite(e.NewValue);
            if(site?.SiteID == null)
            {
                return;
            }

            foreach (AMWCSubstitute sub in SubstituteWorkCenters.Select())
            {
                if (sub?.SiteID == null || sub.SiteID != site.SiteID)
                {
                    continue;
                }

                if (WCRecords.Ask(Messages.ChangingWarehouseWillDeleteSubstitue, MessageButtons.OKCancel) != WebDialogResult.OK)
                {
                    e.NewValue = e.Row.SiteID;
                    e.Cancel = true;
                }
            }
        }

        protected INSite GetSite(object siteCd)
        {
            if (siteCd == null)
            {
                return null;
            }

            INSite site = PXSelect<INSite, Where<INSite.siteCD, Equal<Required<INSite.siteCD>>>>.Select(this, siteCd);

            if(site?.SiteID != null)
            {
                return site;
            }

            return PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, siteCd);
        }

        protected virtual void DeleteSubstitue(string wcId, int? siteId)
        {
            AMWCSubstitute sub = PXSelect<
                AMWCSubstitute,
                Where<AMWCSubstitute.wcID, Equal<Required<AMWC.wcID>>,
                    And<AMWCSubstitute.siteID, Equal<Required<AMWCSubstitute.siteID>>>>>
                .Select(this, wcId, siteId);

            if (sub != null)
            {
                SubstituteWorkCenters.Delete(sub);
            }
        }

        protected virtual void AMWC_WCID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var newValueString = Convert.ToString(e.NewValue);
            if (string.IsNullOrWhiteSpace(newValueString))
            {
                return;
            }
            // Prevent silly users from entering leading spaces...
            e.NewValue = newValueString.TrimStart();
        }

        protected virtual void AMWCMach_MachID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var workCenterMachine = (AMWCMach)e.Row;
            AMMach machineMaster =
                PXSelect<AMMach, Where<AMMach.machID, Equal<Required<AMMach.machID>>>>.Select(this,
                    workCenterMachine.MachID);

            workCenterMachine.StdCost = 0;
            workCenterMachine.MachAcctID = null;
            workCenterMachine.MachSubID = null;

            if (machineMaster != null && !string.IsNullOrEmpty(workCenterMachine.MachID))
            {
                workCenterMachine.StdCost = machineMaster.StdCost;
                workCenterMachine.MachAcctID = machineMaster.MachAcctID;
                workCenterMachine.MachSubID = machineMaster.MachSubID;
            }
        }

        protected virtual void AMWCMach_MachineOverride_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var workCenterMachine = (AMWCMach)e.Row;
            if (workCenterMachine == null || workCenterMachine.MachineOverride.GetValueOrDefault())
            {
                return;
            }

            AMMach machineMaster = PXSelect<AMMach, Where<AMMach.machID, Equal<Required<AMMach.machID>>>>
                .Select(this, workCenterMachine.MachID);

            if (machineMaster != null && !string.IsNullOrEmpty(workCenterMachine.MachID))
            {
                workCenterMachine.StdCost = machineMaster.StdCost;
                workCenterMachine.MachAcctID = machineMaster.MachAcctID;
                workCenterMachine.MachSubID = machineMaster.MachSubID;
            }
        }

        protected virtual void AMWCMach_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMWCMach)e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMWCMach.stdCost>(sender, e.Row, row.MachineOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMWCMach.machAcctID>(sender, e.Row, row.MachineOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMWCMach.machSubID>(sender, e.Row, row.MachineOverride.GetValueOrDefault());
        }

        protected virtual void AMBomOper_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled(cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<AMBomOper.selected>(cache, e.Row, true);
        }

        public PXAction<AMWC> InquiresMenu;

        [PXUIField(DisplayName = Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected IEnumerable inquiresMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMWC> CalendarInquiry;

        [PXUIField(DisplayName = Messages.WcCalendarInquiry, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable calendarInquiry(PXAdapter adapter)
        {
            if (this.WCRecords.Current != null)
            {
                CalendarInquiryFilter.Current = BuildWorkCenterCalendarWeek(this, this.WCRecords.Current);
                CalendarInquiryFilter.AskExt();
            }
            return adapter.Get();
        }

        public PXAction<AMWC> ScheduleInquiry;

        [PXUIField(DisplayName = Messages.Schedule, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable scheduleInquiry(PXAdapter adapter)
        {
            if (this.WCRecords.Current != null)
            {
                var gi = new GIWorkCenterSchedule();
                gi.SetParameter(GIWorkCenterSchedule.Parameters.WorkCenter, WCRecords.Current.WcID);
                gi.CallGenericInquiry(PXBaseRedirectException.WindowMode.New);
            }
            return adapter.Get();
        }

        public PXAction<AMWC> WcDispatch;

        [PXUIField(DisplayName = Messages.Dispatch, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable wcDispatch(PXAdapter adapter)
        {
            if (this.WCRecords.Current != null)
            {
                var gi = new GIWorkCenterDispatch();
                gi.SetParameter(GIWorkCenterSchedule.Parameters.WorkCenter, WCRecords.Current.WcID);
                gi.CallGenericInquiry(PXBaseRedirectException.WindowMode.New);
            }
            return adapter.Get();
        }

        public PXAction<AMWC> CapacityInquiry;

        [PXUIField(DisplayName = Messages.Capacity, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable capacityInquiry(PXAdapter adapter)
        {
            if (this.WCRecords.Current != null)
            {
                WorkCenterCapacityInq graph = PXGraph.CreateInstance<WorkCenterCapacityInq>();
                graph.CapacityFilter.Current.WcID = WCRecords.Current.WcID;
                throw new PXRedirectRequiredException(graph, true, Messages.Capacity);
            }
            return adapter.Get();
        }

        protected static CalendarWeek BuildWorkCenterCalendarWeek(PXGraph graph, AMWC workCenter)
        {
            DateTime today = graph.Accessinfo.BusinessDate.GetValueOrDefault(Common.Dates.Today);
            var calendarWeek = new CalendarWeek();
            var wcCalendarHelper = new WorkCenterCalendarHelper(graph, workCenter, true);
            for (int day = 0; day < 7; day++)
            {
                UpdateCalendarWeek(day + 1, wcCalendarHelper.GetWorkingHours(today.AddDays(day)).WorkCenterDateInfo,
                    ref calendarWeek);
            }

            return calendarWeek;
        }

        protected static void UpdateCalendarWeek(int dayNumber, AMDateInfo wcDateInfo, ref CalendarWeek calendarWeek)
        {
            switch (dayNumber)
            {
                case 1:
                    calendarWeek.Day1Date = wcDateInfo.Date;
                    calendarWeek.Day1DayOfWeek = CalendarHelper.DayOfWeekFullName(wcDateInfo.Date.DayOfWeek);
                    calendarWeek.Day1WorkDay = wcDateInfo.IsWorkingDay;
                    if (wcDateInfo.IsWorkingDay)
                    {
                        calendarWeek.Day1StartTime = new DateTime(1900, 1, 1) + wcDateInfo.StartTime;
                        calendarWeek.Day1EndTime = new DateTime(1900, 1, 1) + wcDateInfo.EndTime;
                        calendarWeek.Day1WorkTime = wcDateInfo.WorkingMinutes;
                        calendarWeek.Day1BreakTime = wcDateInfo.BreakMinutes;
                    }
                    calendarWeek.Day1ExceptionDate = wcDateInfo.IsExceptionDate;
                    return;
                case 2:
                    calendarWeek.Day2Date = wcDateInfo.Date;
                    calendarWeek.Day2DayOfWeek = CalendarHelper.DayOfWeekFullName(wcDateInfo.Date.DayOfWeek);
                    calendarWeek.Day2WorkDay = wcDateInfo.IsWorkingDay;
                    if (wcDateInfo.IsWorkingDay)
                    {
                        calendarWeek.Day2StartTime = new DateTime(1900, 1, 1) + wcDateInfo.StartTime;
                        calendarWeek.Day2EndTime = new DateTime(1900, 1, 1) + wcDateInfo.EndTime;
                        calendarWeek.Day2WorkTime = wcDateInfo.WorkingMinutes;
                        calendarWeek.Day2BreakTime = wcDateInfo.BreakMinutes;
                    }
                    calendarWeek.Day2ExceptionDate = wcDateInfo.IsExceptionDate;
                    return;

                case 3:
                    calendarWeek.Day3Date = wcDateInfo.Date;
                    calendarWeek.Day3DayOfWeek = CalendarHelper.DayOfWeekFullName(wcDateInfo.Date.DayOfWeek);
                    calendarWeek.Day3WorkDay = wcDateInfo.IsWorkingDay;
                    if (wcDateInfo.IsWorkingDay)
                    {
                        calendarWeek.Day3StartTime = new DateTime(1900, 1, 1) + wcDateInfo.StartTime;
                        calendarWeek.Day3EndTime = new DateTime(1900, 1, 1) + wcDateInfo.EndTime;
                        calendarWeek.Day3WorkTime = wcDateInfo.WorkingMinutes;
                        calendarWeek.Day3BreakTime = wcDateInfo.BreakMinutes;
                    }
                    calendarWeek.Day3ExceptionDate = wcDateInfo.IsExceptionDate;
                    return;

                case 4:
                    calendarWeek.Day4Date = wcDateInfo.Date;
                    calendarWeek.Day4DayOfWeek = CalendarHelper.DayOfWeekFullName(wcDateInfo.Date.DayOfWeek);
                    calendarWeek.Day4WorkDay = wcDateInfo.IsWorkingDay;
                    if (wcDateInfo.IsWorkingDay)
                    {
                        calendarWeek.Day4StartTime = new DateTime(1900, 1, 1) + wcDateInfo.StartTime;
                        calendarWeek.Day4EndTime = new DateTime(1900, 1, 1) + wcDateInfo.EndTime;
                        calendarWeek.Day4WorkTime = wcDateInfo.WorkingMinutes;
                        calendarWeek.Day4BreakTime = wcDateInfo.BreakMinutes;
                    }
                    calendarWeek.Day4ExceptionDate = wcDateInfo.IsExceptionDate;
                    return;

                case 5:
                    calendarWeek.Day5Date = wcDateInfo.Date;
                    calendarWeek.Day5DayOfWeek = CalendarHelper.DayOfWeekFullName(wcDateInfo.Date.DayOfWeek);
                    calendarWeek.Day5WorkDay = wcDateInfo.IsWorkingDay;
                    if (wcDateInfo.IsWorkingDay)
                    {
                        calendarWeek.Day5StartTime = new DateTime(1900, 1, 1) + wcDateInfo.StartTime;
                        calendarWeek.Day5EndTime = new DateTime(1900, 1, 1) + wcDateInfo.EndTime;
                        calendarWeek.Day5WorkTime = wcDateInfo.WorkingMinutes;
                        calendarWeek.Day5BreakTime = wcDateInfo.BreakMinutes;
                    }
                    calendarWeek.Day5ExceptionDate = wcDateInfo.IsExceptionDate;
                    return;

                case 6:
                    calendarWeek.Day6Date = wcDateInfo.Date;
                    calendarWeek.Day6DayOfWeek = CalendarHelper.DayOfWeekFullName(wcDateInfo.Date.DayOfWeek);
                    calendarWeek.Day6WorkDay = wcDateInfo.IsWorkingDay;
                    if (wcDateInfo.IsWorkingDay)
                    {
                        calendarWeek.Day6StartTime = new DateTime(1900, 1, 1) + wcDateInfo.StartTime;
                        calendarWeek.Day6EndTime = new DateTime(1900, 1, 1) + wcDateInfo.EndTime;
                        calendarWeek.Day6WorkTime = wcDateInfo.WorkingMinutes;
                        calendarWeek.Day6BreakTime = wcDateInfo.BreakMinutes;
                    }
                    calendarWeek.Day6ExceptionDate = wcDateInfo.IsExceptionDate;
                    return;

                case 7:
                    calendarWeek.Day7Date = wcDateInfo.Date;
                    calendarWeek.Day7DayOfWeek = CalendarHelper.DayOfWeekFullName(wcDateInfo.Date.DayOfWeek);
                    calendarWeek.Day7WorkDay = wcDateInfo.IsWorkingDay;
                    if (wcDateInfo.IsWorkingDay)
                    {
                        calendarWeek.Day7StartTime = new DateTime(1900, 1, 1) + wcDateInfo.StartTime;
                        calendarWeek.Day7EndTime = new DateTime(1900, 1, 1) + wcDateInfo.EndTime;
                        calendarWeek.Day7WorkTime = wcDateInfo.WorkingMinutes;
                        calendarWeek.Day7BreakTime = wcDateInfo.BreakMinutes;
                    }
                    calendarWeek.Day7ExceptionDate = wcDateInfo.IsExceptionDate;
                    return;

                default:
                    throw new ArgumentOutOfRangeException("dayNumber");
            }
        }

        public PXAction<AMWC> MassUpdate;
        [PXUIField(DisplayName = Messages.MassUpdate, MapEnableRights = PXCacheRights.Update,
             MapViewRights = PXCacheRights.Update), PXProcessButton]
        protected virtual IEnumerable massUpdate(PXAdapter adapter)
        {
            var updateOptions = MassUpdateFilter.Current;

            if (MassUpdateFilter.AskExt() == WebDialogResult.OK)
            {
                if (updateOptions.BFlush != true && updateOptions.ScrapAction != true && updateOptions.OperDescription != true && updateOptions.OutsideProcess != true)
                {
                    return adapter.Get();
                }

                PXLongOperation.StartOperation(this, () =>
                {
                    ProcessMassUpdate(WCWhereUsed.Cache.Cached.Cast<AMBomOper>().Where(row => row.Selected == true).ToList());
                });

                PXLongOperation.WaitCompletion(UID);
                WCWhereUsed.Cache.Clear();
            }

            return adapter.Get();
        }

        protected virtual void ProcessMassUpdate(List<AMBomOper> list)
        {
            var updateGraph = CreateInstance<WCMaint>();
            updateGraph.Clear();
            foreach (var oper in list)
            {
                if (!oper.Selected.GetValueOrDefault())
                {
                    continue;
                }

                var operrec = WCWhereUsed.Locate(oper) ?? PXSelect<AMBomOper,
                                        Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                                            And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
                                                And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>
                                            >>>.Select(this, oper.BOMID, oper.RevisionID, oper.OperationID);
                if (operrec == null)
                {
                    continue;
                }

                var hasUpdate = false;
                if (MassUpdateFilter.Current.BFlush.GetValueOrDefault()
                    && operrec.BFlush.GetValueOrDefault() != WCRecordsSelected.Current.BflushLbr.GetValueOrDefault())
                {
                    hasUpdate = true;
                    operrec.BFlush = WCRecordsSelected.Current.BflushLbr;
                }

                if (MassUpdateFilter.Current.ScrapAction.GetValueOrDefault()
                    && operrec.ScrapAction != WCRecordsSelected.Current.ScrapAction)
                {
                    hasUpdate = true;
                    operrec.ScrapAction = WCRecordsSelected.Current.ScrapAction;
                }

                if (MassUpdateFilter.Current.OperDescription.GetValueOrDefault()
                    && !operrec.Descr.EqualsWithTrim(WCRecordsSelected.Current.Descr))
                {
                    hasUpdate = true;
                    operrec.Descr = WCRecordsSelected.Current.Descr;
                }

                if (MassUpdateFilter.Current.OutsideProcess.GetValueOrDefault()
                    && operrec.OutsideProcess.GetValueOrDefault() != WCRecordsSelected.Current.OutsideFlg.GetValueOrDefault())
                {
                    hasUpdate = true;
                    operrec.OutsideProcess = WCRecordsSelected.Current.OutsideFlg;
                }

                if (hasUpdate)
                {
                    updateGraph.WCWhereUsed.Update(operrec);
                }
            }

            if (updateGraph.IsDirty)
            {
                PXTrace.WriteInformation($"{Messages.MassUpdate} - {Common.Cache.GetCacheName(typeof(AMBomOper))} # {updateGraph.WCWhereUsed.Cache.Updated.Count()}");
                updateGraph.Actions.PressSave();
            }
        }

        public PXAction<AMWC> ViewBOM;
        [PXLookupButton]
        [PXUIField(DisplayName = "View BOM")]
        protected virtual void viewBOM()
        {
            if (WCWhereUsed.Current == null)
            {
                return;
            }

            var graphBOM = PXGraph.CreateInstance<BOMMaint>();

            AMBomItem bomItem = PXSelect<AMBomItem,
                Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                    And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>
                    >>>.Select(graphBOM, WCWhereUsed.Current.BOMID, WCWhereUsed.Current.RevisionID);

            if (bomItem != null)
            {
                graphBOM.Documents.Current = bomItem;
            }

            if (graphBOM.Documents.Current != null)
            {
                throw new PXRedirectRequiredException(graphBOM, true, string.Empty);
            }
        }

        [Serializable]
        [PXHidden]
        public class CalendarWeek : IBqlTable
        {
            #region DAY 1

            public abstract class day1Date : PX.Data.BQL.BqlDateTime.Field<day1Date> { }
            protected DateTime? _Day1Date;

            [PXDBDate]
            [PXUIField(DisplayName = "Day 1 Date", Enabled = false)]
            public virtual DateTime? Day1Date
            {
                get { return this._Day1Date; }
                set { this._Day1Date = value; }
            }

            public abstract class day1WorkDay : PX.Data.BQL.BqlBool.Field<day1WorkDay> { }
            protected bool? _Day1WorkDay;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Day 1 Work Day", Enabled = false)]
            public virtual bool? Day1WorkDay
            {
                get { return this._Day1WorkDay; }
                set { this._Day1WorkDay = value; }
            }

            public abstract class day1StartTime : PX.Data.BQL.BqlDateTime.Field<day1StartTime> { }
            protected DateTime? _Day1StartTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 1 Start Time", Enabled = false)]
            public virtual DateTime? Day1StartTime
            {
                get { return this._Day1StartTime; }
                set { this._Day1StartTime = value; }
            }

            public abstract class day1EndTime : PX.Data.BQL.BqlString.Field<day1EndTime> { }
            protected DateTime? _Day1EndTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 1 End Time", Enabled = false)]
            public virtual DateTime? Day1EndTime
            {
                get { return this._Day1EndTime; }
                set { this._Day1EndTime = value; }
            }

            public abstract class day1BreakTime : PX.Data.BQL.BqlString.Field<day1BreakTime> { }
            protected int? _Day1BreakTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 1 Break Time", Enabled = false)]
            public virtual int? Day1BreakTime
            {
                get { return this._Day1BreakTime; }
                set { this._Day1BreakTime = value; }
            }

            public abstract class day1WorkTime : PX.Data.BQL.BqlString.Field<day1WorkTime> { }
            protected int? _Day1WorkTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 1 Work Time", Enabled = false)]
            public virtual int? Day1WorkTime
            {
                get { return this._Day1WorkTime; }
                set { this._Day1WorkTime = value; }
            }

            public abstract class day1ExceptionDate : PX.Data.BQL.BqlDateTime.Field<day1ExceptionDate> { }
            protected bool? _Day1ExceptionDate;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "", Enabled = false)]
            public virtual bool? Day1ExceptionDate
            {
                get { return this._Day1ExceptionDate; }
                set { this._Day1ExceptionDate = value; }
            }

            public abstract class day1DayOfWeek : PX.Data.BQL.BqlString.Field<day1DayOfWeek> { }
            protected String _Day1DayOfWeek;

            [PXString]
            [PXUIField(DisplayName = "Day 1 Day of Week", Enabled = false)]
            public virtual String Day1DayOfWeek
            {
                get { return this._Day1DayOfWeek; }
                set { this._Day1DayOfWeek = value; }
            }

            #endregion

            #region DAY 2

            public abstract class day2Date : PX.Data.BQL.BqlDateTime.Field<day2Date> { }
            protected DateTime? _Day2Date;

            [PXDBDate]
            [PXUIField(DisplayName = "Day 2 Date", Enabled = false)]
            public virtual DateTime? Day2Date
            {
                get { return this._Day2Date; }
                set { this._Day2Date = value; }
            }

            public abstract class day2WorkDay : PX.Data.BQL.BqlBool.Field<day2WorkDay> { }
            protected bool? _Day2WorkDay;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Day 2 Work Day", Enabled = false)]
            public virtual bool? Day2WorkDay
            {
                get { return this._Day2WorkDay; }
                set { this._Day2WorkDay = value; }
            }

            public abstract class day2StartTime : PX.Data.BQL.BqlDateTime.Field<day2StartTime> { }
            protected DateTime? _Day2StartTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 2 Start Time", Enabled = false)]
            public virtual DateTime? Day2StartTime
            {
                get { return this._Day2StartTime; }
                set { this._Day2StartTime = value; }
            }

            public abstract class day2EndTime : PX.Data.BQL.BqlString.Field<day2EndTime> { }
            protected DateTime? _Day2EndTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 2 End Time", Enabled = false)]
            public virtual DateTime? Day2EndTime
            {
                get { return this._Day2EndTime; }
                set { this._Day2EndTime = value; }
            }

            public abstract class day2BreakTime : PX.Data.BQL.BqlString.Field<day2BreakTime> { }
            protected int? _Day2BreakTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 2 Break Time", Enabled = false)]
            public virtual int? Day2BreakTime
            {
                get { return this._Day2BreakTime; }
                set { this._Day2BreakTime = value; }
            }

            public abstract class day2WorkTime : PX.Data.BQL.BqlString.Field<day2WorkTime> { }
            protected int? _Day2WorkTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 2 Work Time", Enabled = false)]
            public virtual int? Day2WorkTime
            {
                get { return this._Day2WorkTime; }
                set { this._Day2WorkTime = value; }
            }

            public abstract class day2ExceptionDate : PX.Data.BQL.BqlDateTime.Field<day2ExceptionDate> { }
            protected bool? _Day2ExceptionDate;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "", Enabled = false)]
            public virtual bool? Day2ExceptionDate
            {
                get { return this._Day2ExceptionDate; }
                set { this._Day2ExceptionDate = value; }
            }

            public abstract class day2DayOfWeek : PX.Data.BQL.BqlString.Field<day2DayOfWeek> { }
            protected String _Day2DayOfWeek;

            [PXString]
            [PXUIField(DisplayName = "Day 2 Day of Week", Enabled = false)]
            public virtual String Day2DayOfWeek
            {
                get { return this._Day2DayOfWeek; }
                set { this._Day2DayOfWeek = value; }
            }

            #endregion

            #region DAY 3

            public abstract class day3Date : PX.Data.BQL.BqlDateTime.Field<day3Date> { }
            protected DateTime? _Day3Date;

            [PXDBDate]
            [PXUIField(DisplayName = "Day 3 Date", Enabled = false)]
            public virtual DateTime? Day3Date
            {
                get { return this._Day3Date; }
                set { this._Day3Date = value; }
            }

            public abstract class day3WorkDay : PX.Data.BQL.BqlBool.Field<day3WorkDay> { }
            protected bool? _Day3WorkDay;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Day 3 Work Day", Enabled = false)]
            public virtual bool? Day3WorkDay
            {
                get { return this._Day3WorkDay; }
                set { this._Day3WorkDay = value; }
            }

            public abstract class day3StartTime : PX.Data.BQL.BqlDateTime.Field<day3StartTime> { }
            protected DateTime? _Day3StartTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 3 Start Time", Enabled = false)]
            public virtual DateTime? Day3StartTime
            {
                get { return this._Day3StartTime; }
                set { this._Day3StartTime = value; }
            }

            public abstract class day3EndTime : PX.Data.BQL.BqlString.Field<day3EndTime> { }
            protected DateTime? _Day3EndTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 3 End Time", Enabled = false)]
            public virtual DateTime? Day3EndTime
            {
                get { return this._Day3EndTime; }
                set { this._Day3EndTime = value; }
            }

            public abstract class day3BreakTime : PX.Data.BQL.BqlString.Field<day3BreakTime> { }
            protected int? _Day3BreakTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 3 Break Time", Enabled = false)]
            public virtual int? Day3BreakTime
            {
                get { return this._Day3BreakTime; }
                set { this._Day3BreakTime = value; }
            }

            public abstract class day3WorkTime : PX.Data.BQL.BqlString.Field<day3WorkTime> { }
            protected int? _Day3WorkTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 3 Work Time", Enabled = false)]
            public virtual int? Day3WorkTime
            {
                get { return this._Day3WorkTime; }
                set { this._Day3WorkTime = value; }
            }

            public abstract class day3ExceptionDate : PX.Data.BQL.BqlDateTime.Field<day3ExceptionDate> { }
            protected bool? _Day3ExceptionDate;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "", Enabled = false)]
            public virtual bool? Day3ExceptionDate
            {
                get { return this._Day3ExceptionDate; }
                set { this._Day3ExceptionDate = value; }
            }

            public abstract class day3DayOfWeek : PX.Data.BQL.BqlString.Field<day3DayOfWeek> { }
            protected String _Day3DayOfWeek;

            [PXString]
            [PXUIField(DisplayName = "Day 3 Day of Week", Enabled = false)]
            public virtual String Day3DayOfWeek
            {
                get { return this._Day3DayOfWeek; }
                set { this._Day3DayOfWeek = value; }
            }

            #endregion

            #region DAY 4

            public abstract class day4Date : PX.Data.BQL.BqlDateTime.Field<day4Date> { }
            protected DateTime? _Day4Date;

            [PXDBDate]
            [PXUIField(DisplayName = "Day 4 Date", Enabled = false)]
            public virtual DateTime? Day4Date
            {
                get { return this._Day4Date; }
                set { this._Day4Date = value; }
            }

            public abstract class day4WorkDay : PX.Data.BQL.BqlBool.Field<day4WorkDay> { }
            protected bool? _Day4WorkDay;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Day 4 Work Day", Enabled = false)]
            public virtual bool? Day4WorkDay
            {
                get { return this._Day4WorkDay; }
                set { this._Day4WorkDay = value; }
            }

            public abstract class day4StartTime : PX.Data.BQL.BqlDateTime.Field<day4StartTime> { }
            protected DateTime? _Day4StartTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 4 Start Time", Enabled = false)]
            public virtual DateTime? Day4StartTime
            {
                get { return this._Day4StartTime; }
                set { this._Day4StartTime = value; }
            }

            public abstract class day4EndTime : PX.Data.BQL.BqlString.Field<day4EndTime> { }
            protected DateTime? _Day4EndTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 4 End Time", Enabled = false)]
            public virtual DateTime? Day4EndTime
            {
                get { return this._Day4EndTime; }
                set { this._Day4EndTime = value; }
            }

            public abstract class day4BreakTime : PX.Data.BQL.BqlString.Field<day4BreakTime> { }
            protected int? _Day4BreakTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 4 Break Time", Enabled = false)]
            public virtual int? Day4BreakTime
            {
                get { return this._Day4BreakTime; }
                set { this._Day4BreakTime = value; }
            }

            public abstract class day4WorkTime : PX.Data.BQL.BqlString.Field<day4WorkTime> { }
            protected int? _Day4WorkTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 4 Work Time", Enabled = false)]
            public virtual int? Day4WorkTime
            {
                get { return this._Day4WorkTime; }
                set { this._Day4WorkTime = value; }
            }

            public abstract class day4ExceptionDate : PX.Data.BQL.BqlDateTime.Field<day4ExceptionDate> { }
            protected bool? _Day4ExceptionDate;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "", Enabled = false)]
            public virtual bool? Day4ExceptionDate
            {
                get { return this._Day4ExceptionDate; }
                set { this._Day4ExceptionDate = value; }
            }

            public abstract class day4DayOfWeek : PX.Data.BQL.BqlString.Field<day4DayOfWeek> { }
            protected String _Day4DayOfWeek;

            [PXString]
            [PXUIField(DisplayName = "Day 4 Day of Week", Enabled = false)]
            public virtual String Day4DayOfWeek
            {
                get { return this._Day4DayOfWeek; }
                set { this._Day4DayOfWeek = value; }
            }

            #endregion

            #region DAY 5

            public abstract class day5Date : PX.Data.BQL.BqlDateTime.Field<day5Date> { }
            protected DateTime? _Day5Date;

            [PXDBDate]
            [PXUIField(DisplayName = "Day 5 Date", Enabled = false)]
            public virtual DateTime? Day5Date
            {
                get { return this._Day5Date; }
                set { this._Day5Date = value; }
            }

            public abstract class day5WorkDay : PX.Data.BQL.BqlBool.Field<day5WorkDay> { }
            protected bool? _Day5WorkDay;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Day 5 Work Day", Enabled = false)]
            public virtual bool? Day5WorkDay
            {
                get { return this._Day5WorkDay; }
                set { this._Day5WorkDay = value; }
            }

            public abstract class day5StartTime : PX.Data.BQL.BqlDateTime.Field<day5StartTime> { }
            protected DateTime? _Day5StartTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 5 Start Time", Enabled = false)]
            public virtual DateTime? Day5StartTime
            {
                get { return this._Day5StartTime; }
                set { this._Day5StartTime = value; }
            }

            public abstract class day5EndTime : PX.Data.BQL.BqlString.Field<day5EndTime> { }
            protected DateTime? _Day5EndTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 5 End Time", Enabled = false)]
            public virtual DateTime? Day5EndTime
            {
                get { return this._Day5EndTime; }
                set { this._Day5EndTime = value; }
            }

            public abstract class day5BreakTime : PX.Data.BQL.BqlString.Field<day5BreakTime> { }
            protected int? _Day5BreakTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 5 Break Time", Enabled = false)]
            public virtual int? Day5BreakTime
            {
                get { return this._Day5BreakTime; }
                set { this._Day5BreakTime = value; }
            }

            public abstract class day5WorkTime : PX.Data.BQL.BqlString.Field<day5WorkTime> { }
            protected int? _Day5WorkTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 5 Work Time", Enabled = false)]
            public virtual int? Day5WorkTime
            {
                get { return this._Day5WorkTime; }
                set { this._Day5WorkTime = value; }
            }

            public abstract class day5ExceptionDate : PX.Data.BQL.BqlDateTime.Field<day5ExceptionDate> { }
            protected bool? _Day5ExceptionDate;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "", Enabled = false)]
            public virtual bool? Day5ExceptionDate
            {
                get { return this._Day5ExceptionDate; }
                set { this._Day5ExceptionDate = value; }
            }

            public abstract class day5DayOfWeek : PX.Data.BQL.BqlString.Field<day5DayOfWeek> { }
            protected String _Day5DayOfWeek;

            [PXString]
            [PXUIField(DisplayName = "Day 5 Day of Week", Enabled = false)]
            public virtual String Day5DayOfWeek
            {
                get { return this._Day5DayOfWeek; }
                set { this._Day5DayOfWeek = value; }
            }

            #endregion

            #region DAY 6

            public abstract class day6Date : PX.Data.BQL.BqlDateTime.Field<day6Date> { }
            protected DateTime? _Day6Date;

            [PXDBDate]
            [PXUIField(DisplayName = "Day 6 Date", Enabled = false)]
            public virtual DateTime? Day6Date
            {
                get { return this._Day6Date; }
                set { this._Day6Date = value; }
            }

            public abstract class day6WorkDay : PX.Data.BQL.BqlBool.Field<day6WorkDay> { }
            protected bool? _Day6WorkDay;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Day 6 Work Day", Enabled = false)]
            public virtual bool? Day6WorkDay
            {
                get { return this._Day6WorkDay; }
                set { this._Day6WorkDay = value; }
            }

            public abstract class day6StartTime : PX.Data.BQL.BqlDateTime.Field<day6StartTime> { }
            protected DateTime? _Day6StartTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 6 Start Time", Enabled = false)]
            public virtual DateTime? Day6StartTime
            {
                get { return this._Day6StartTime; }
                set { this._Day6StartTime = value; }
            }

            public abstract class day6EndTime : PX.Data.BQL.BqlString.Field<day6EndTime> { }
            protected DateTime? _Day6EndTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 6 End Time", Enabled = false)]
            public virtual DateTime? Day6EndTime
            {
                get { return this._Day6EndTime; }
                set { this._Day6EndTime = value; }
            }

            public abstract class day6BreakTime : PX.Data.BQL.BqlString.Field<day6BreakTime> { }
            protected int? _Day6BreakTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 6 Break Time", Enabled = false)]
            public virtual int? Day6BreakTime
            {
                get { return this._Day6BreakTime; }
                set { this._Day6BreakTime = value; }
            }

            public abstract class day6WorkTime : PX.Data.BQL.BqlString.Field<day6WorkTime> { }
            protected int? _Day6WorkTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 6 Work Time", Enabled = false)]
            public virtual int? Day6WorkTime
            {
                get { return this._Day6WorkTime; }
                set { this._Day6WorkTime = value; }
            }

            public abstract class day6ExceptionDate : PX.Data.BQL.BqlDateTime.Field<day6ExceptionDate> { }
            protected bool? _Day6ExceptionDate;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "", Enabled = false)]
            public virtual bool? Day6ExceptionDate
            {
                get { return this._Day6ExceptionDate; }
                set { this._Day6ExceptionDate = value; }
            }

            public abstract class day6DayOfWeek : PX.Data.BQL.BqlString.Field<day6DayOfWeek> { }
            protected String _Day6DayOfWeek;

            [PXString]
            [PXUIField(DisplayName = "Day 6 Day of Week", Enabled = false)]
            public virtual String Day6DayOfWeek
            {
                get { return this._Day6DayOfWeek; }
                set { this._Day6DayOfWeek = value; }
            }

            #endregion

            #region DAY 7

            public abstract class day7Date : PX.Data.BQL.BqlDateTime.Field<day7Date> { }
            protected DateTime? _Day7Date;

            [PXDBDate]
            [PXUIField(DisplayName = "Day 7 Date", Enabled = false)]
            public virtual DateTime? Day7Date
            {
                get { return this._Day7Date; }
                set { this._Day7Date = value; }
            }

            public abstract class day7WorkDay : PX.Data.BQL.BqlBool.Field<day7WorkDay> { }
            protected bool? _Day7WorkDay;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Day 7 Work Day", Enabled = false)]
            public virtual bool? Day7WorkDay
            {
                get { return this._Day7WorkDay; }
                set { this._Day7WorkDay = value; }
            }

            public abstract class day7StartTime : PX.Data.BQL.BqlDateTime.Field<day7StartTime> { }
            protected DateTime? _Day7StartTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 7 Start Time", Enabled = false)]
            public virtual DateTime? Day7StartTime
            {
                get { return this._Day7StartTime; }
                set { this._Day7StartTime = value; }
            }

            public abstract class day7EndTime : PX.Data.BQL.BqlString.Field<day7EndTime> { }
            protected DateTime? _Day7EndTime;

            [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
            [PXUIField(DisplayName = "Day 7 End Time", Enabled = false)]
            public virtual DateTime? Day7EndTime
            {
                get { return this._Day7EndTime; }
                set { this._Day7EndTime = value; }
            }

            public abstract class day7BreakTime : PX.Data.BQL.BqlString.Field<day7BreakTime> { }
            protected int? _Day7BreakTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 7 Break Time", Enabled = false)]
            public virtual int? Day7BreakTime
            {
                get { return this._Day7BreakTime; }
                set { this._Day7BreakTime = value; }
            }

            public abstract class day7WorkTime : PX.Data.BQL.BqlString.Field<day7WorkTime> { }
            protected int? _Day7WorkTime;

            [PXDBInt]
            [PXUIField(DisplayName = "Day 7 Work Time", Enabled = false)]
            public virtual int? Day7WorkTime
            {
                get { return this._Day7WorkTime; }
                set { this._Day7WorkTime = value; }
            }

            public abstract class day7ExceptionDate : PX.Data.BQL.BqlDateTime.Field<day7ExceptionDate> { }
            protected bool? _Day7ExceptionDate;

            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "", Enabled = false)]
            public virtual bool? Day7ExceptionDate
            {
                get { return this._Day7ExceptionDate; }
                set { this._Day7ExceptionDate = value; }
            }

            public abstract class day7DayOfWeek : PX.Data.BQL.BqlString.Field<day7DayOfWeek> { }
            protected String _Day7DayOfWeek;

            [PXString]
            [PXUIField(DisplayName = "Day 7 Day of Week", Enabled = false)]
            public virtual String Day7DayOfWeek
            {
                get { return this._Day7DayOfWeek; }
                set { this._Day7DayOfWeek = value; }
            }

            #endregion
        }
    }

    #region Work Center Update DAC Fields
    [Serializable]
    [PXCacheName("WorkCenter Update Filter")]
    public class WorkCenterUpdateFilter : IBqlTable
    {
        #region BFlush
        public abstract class bFlush : PX.Data.BQL.BqlBool.Field<bFlush> { }

        protected Boolean? _BFlush;
        [PXBool]
        [PXUIField(DisplayName = "Backflush Labor")]
        public virtual Boolean? BFlush
        {
            get { return this._BFlush; }
            set { this._BFlush = value; }
        }
        #endregion

        #region ScrapAction
        public abstract class scrapAction : PX.Data.BQL.BqlBool.Field<scrapAction> { }

        protected Boolean? _ScrapAction;
        [PXBool]
        [PXUIField(DisplayName = "Scrap Action")]
        public virtual Boolean? ScrapAction
        {
            get { return this._ScrapAction; }
            set { this._ScrapAction = value; }
        }
        #endregion

        #region OperDescription
        public abstract class operDescription : PX.Data.BQL.BqlBool.Field<operDescription> { }

        protected Boolean? _OperDescription;
        [PXBool]
        [PXUIField(DisplayName = "Oper Description")]
        public virtual Boolean? OperDescription
        {
            get { return this._OperDescription; }
            set { this._OperDescription = value; }
        }
        #endregion

        #region OutsideProcess
        public abstract class outsideProcess : PX.Data.BQL.BqlBool.Field<outsideProcess> { }

        protected Boolean? _OutsideProcess;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Outside Process")]
        public virtual Boolean? OutsideProcess
        {
            get
            {
                return this._OutsideProcess;
            }
            set
            {
                this._OutsideProcess = value;
            }
        }
        #endregion
    }
    #endregion
}