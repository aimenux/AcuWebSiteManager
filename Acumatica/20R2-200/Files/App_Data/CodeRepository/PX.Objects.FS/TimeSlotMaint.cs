using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class TimeSlotMaint : PXGraph<TimeSlotMaint, FSTimeSlot>
    {
        public PXSelect<FSTimeSlot> TimeSlotRecords;

        #region Virtual Methods
        /// <summary>
        /// Converts a FSTimeSlot to a generic class Slot.
        /// </summary>
        public virtual Scheduler.Slot ConvertFSTimeSlotToSlot(FSTimeSlot fsTimeSlotRow)
        {
            Scheduler.Slot returnSlot = new Scheduler.Slot();
            returnSlot.SlotType = fsTimeSlotRow.ScheduleType;
            returnSlot.DateTimeBegin = (DateTime)fsTimeSlotRow.TimeStart;
            returnSlot.DateTimeEnd = (DateTime)fsTimeSlotRow.TimeEnd;

            return returnSlot;
        }

        /// <summary>
        /// Converts a generic Slot in a FSTimeSlot based in [fsTimeSlotRow] and [slotLevel].
        /// </summary>
        public virtual FSTimeSlot ConvertSlotToFSTimeSlot(Scheduler.Slot slot, FSTimeSlot fsTimeSlotRow, int slotLevel)
        {
            FSTimeSlot fsTimeSlotRow_Return = new FSTimeSlot();

            TimeSpan duration = (DateTime)slot.DateTimeEnd - (DateTime)slot.DateTimeBegin;

            fsTimeSlotRow_Return.BranchID = fsTimeSlotRow.BranchID;
            fsTimeSlotRow_Return.BranchLocationID = fsTimeSlotRow.BranchLocationID;
            fsTimeSlotRow_Return.EmployeeID = fsTimeSlotRow.EmployeeID;
            fsTimeSlotRow_Return.TimeStart = slot.DateTimeBegin;
            fsTimeSlotRow_Return.TimeEnd = slot.DateTimeEnd;
            fsTimeSlotRow_Return.ScheduleType = slot.SlotType;
            fsTimeSlotRow_Return.TimeDiff = (decimal?)duration.TotalMinutes;
            fsTimeSlotRow_Return.RecordCount = 1;
            fsTimeSlotRow_Return.SlotLevel = slotLevel;

            return fsTimeSlotRow_Return;
        }

        /// <summary>
        /// Compress a List of Slots in unique slots without overlapping. (The [slotList] must be ordered by TimeBegin and the Slots in [slotList] must be part of same type of Availability).
        /// </summary>
        public virtual List<Scheduler.Slot> CompressListOfSlots(List<Scheduler.Slot> slotList, string availability)
        {
            if (slotList.Count == 0)
            {
                return slotList;
            }

            List<Scheduler.Slot> resultSlots = new List<Scheduler.Slot>();

            DateTime? beginPivot = null;
            DateTime? endPivot = null;

            foreach (Scheduler.Slot slotRow in slotList)
            {
                if (beginPivot == null && endPivot == null)
                {
                    beginPivot = slotRow.DateTimeBegin;
                    endPivot = slotRow.DateTimeEnd;
                }
                else
                {
                    if (slotRow.DateTimeBegin > endPivot)
                    {
                        resultSlots.Add(new Scheduler.Slot
                        {
                            DateTimeBegin = (DateTime)beginPivot,
                            DateTimeEnd = (DateTime)endPivot,
                            SlotType = availability
                        });

                        beginPivot = slotRow.DateTimeBegin;
                        endPivot = slotRow.DateTimeEnd;
                    }
                    else
                    {
                        if (slotRow.DateTimeEnd > endPivot)
                        {
                            endPivot = slotRow.DateTimeEnd;
                        }
                    }
                }
            }

            resultSlots.Add(new Scheduler.Slot
            {
                DateTimeBegin = (DateTime)beginPivot,
                DateTimeEnd = (DateTime)endPivot,
                SlotType = availability
            });

            return resultSlots;
        }

        /// <summary>
        /// Compress the Availability and Unavailability lists and then proceed to intersect them. This function returns the intersection and the Unavailability records compressed as a List. 
        /// </summary>
        public virtual List<Scheduler.Slot> CompressAndIntersectSlots(List<Scheduler.Slot> slotList)
        {
            List<Scheduler.Slot> unavailabilitySlots = slotList.Select(x => x).Where(y => y.SlotType == ID.ScheduleType.UNAVAILABILITY).OrderBy(z => z.DateTimeBegin).ToList();
            List<Scheduler.Slot> availabilitySlots = slotList.Select(x => x).Where(y => y.SlotType == ID.ScheduleType.AVAILABILITY).OrderBy(z => z.DateTimeBegin).ToList();

            List<Scheduler.Slot> resultSlotsUnavailability = new List<Scheduler.Slot>();
            List<Scheduler.Slot> resultSlotsAvailability = new List<Scheduler.Slot>();

            List<Scheduler.Slot> resultCompressedSlots = new List<Scheduler.Slot>();

            SharedFunctions.SlotIsContained isContained = SharedFunctions.SlotIsContained.NotContained;
            bool timeCanceled = false;

            resultSlotsUnavailability = CompressListOfSlots(unavailabilitySlots, ID.ScheduleType.UNAVAILABILITY);
            resultSlotsAvailability = CompressListOfSlots(availabilitySlots, ID.ScheduleType.AVAILABILITY);
            DateTime slotBegin;
            DateTime slotEnd;

            foreach (Scheduler.Slot slotRow in resultSlotsAvailability)
            {
                slotBegin = slotRow.DateTimeBegin;
                slotEnd = slotRow.DateTimeEnd;

                //iterations on the appointments for the day
                foreach (Scheduler.Slot slotUnavailabilityRow in unavailabilitySlots)
                {
                    isContained = SharedFunctions.SlotIsContainedInSlot(slotBegin, slotEnd, slotUnavailabilityRow.DateTimeBegin, slotUnavailabilityRow.DateTimeEnd);
                    if (isContained == SharedFunctions.SlotIsContained.Contained)
                    {
                        if (slotBegin != slotUnavailabilityRow.DateTimeBegin)
                        {
                            resultCompressedSlots.Add(new Scheduler.Slot
                            {
                                DateTimeBegin = (DateTime)slotBegin,
                                DateTimeEnd = (DateTime)slotUnavailabilityRow.DateTimeBegin,
                                SlotType = ID.ScheduleType.AVAILABILITY
                            });
                        }

                        slotBegin = slotUnavailabilityRow.DateTimeEnd;
                    }

                    if (isContained == SharedFunctions.SlotIsContained.PartiallyContained && slotUnavailabilityRow.DateTimeBegin < slotBegin)
                    {
                        slotBegin = slotUnavailabilityRow.DateTimeEnd;
                    }

                    if (isContained == SharedFunctions.SlotIsContained.PartiallyContained && slotUnavailabilityRow.DateTimeBegin < slotEnd)
                    {
                        if (slotBegin < slotUnavailabilityRow.DateTimeBegin)
                        {
                            slotEnd = slotUnavailabilityRow.DateTimeBegin;
                        }
                    }

                    if (isContained == SharedFunctions.SlotIsContained.ExceedsContainment)
                    {
                        timeCanceled = true;
                        break;
                    }
                }

                //last iteration or slots without appointments
                if (timeCanceled == false && slotBegin != slotEnd)
                {
                    resultCompressedSlots.Add(new Scheduler.Slot
                    {
                        DateTimeBegin = (DateTime)slotBegin,
                        DateTimeEnd = (DateTime)slotEnd,
                        SlotType = ID.ScheduleType.AVAILABILITY
                    });
                }

                timeCanceled = false;
            }

            resultCompressedSlots = resultCompressedSlots.Concat(resultSlotsUnavailability).ToList().OrderBy(x => x.DateTimeBegin).ToList();

            return resultCompressedSlots;
        }

        /// <summary>
        /// Creates the compressed Time Slots that applies for the [fsTimeSlotRow].
        /// </summary>
        public virtual void CreateAndCompressedTimeSlots(FSTimeSlot fsTimeSlotRow, TimeSlotMaint timeSlotMaintGraph)
        {
            List<object> cmdArg = new List<object>();
            List<FSTimeSlot> fsTimeSlotListToCompress = new List<FSTimeSlot>();

            List<Scheduler.Slot> slotListToCompress = new List<Scheduler.Slot>();

            var fsBranchLocationSet = PXSelect<FSBranchLocation, 
                                      Where<
                                            FSBranchLocation.branchID, Equal<Required<FSBranchLocation.branchID>>>,
                                      OrderBy<
                                            Asc<FSBranchLocation.branchLocationID>>>.Select(timeSlotMaintGraph, fsTimeSlotRow.BranchID);

            DateHandler dateHandler = new DateHandler(SharedFunctions.TryParseHandlingDateTime(timeSlotMaintGraph.TimeSlotRecords.Cache, fsTimeSlotRow.TimeStart));

            PXSelectBase<FSTimeSlot> fsTimeSlotsBaseToCompress = new PXSelect<FSTimeSlot,
                                                                     Where<
                                                                         FSTimeSlot.branchID, Equal<Required<FSTimeSlot.branchID>>,
                                                                         And<FSTimeSlot.employeeID, Equal<Required<FSTimeSlot.employeeID>>,
                                                                         And<FSTimeSlot.slotLevel, Equal<Required<FSTimeSlot.slotLevel>>,
                                                                         And<FSTimeSlot.timeStart, GreaterEqual<Required<FSTimeSlot.timeStart>>,
                                                                         And<FSTimeSlot.timeStart, LessEqual<Required<FSTimeSlot.timeStart>>>>>>>>
                                                                     (timeSlotMaintGraph);
            cmdArg.Add(fsTimeSlotRow.BranchID);
            cmdArg.Add(fsTimeSlotRow.EmployeeID);
            cmdArg.Add(ID.EmployeeTimeSlotLevel.BASE);
            cmdArg.Add(dateHandler.StartOfDay());
            cmdArg.Add(dateHandler.BeginOfNextDay());

            if (fsTimeSlotRow.BranchLocationID != null)
            {
                fsTimeSlotsBaseToCompress.WhereAnd<Where<FSTimeSlot.branchLocationID, Equal<Required<FSTimeSlot.branchLocationID>>>>();
                cmdArg.Add(fsTimeSlotRow.BranchLocationID);
            }

            var fsTimeSlotSet = fsTimeSlotsBaseToCompress.Select(cmdArg.ToArray());
            foreach (FSTimeSlot fsTimeSlotRow_ToCompress in fsTimeSlotSet)
            {
                fsTimeSlotListToCompress.Add(fsTimeSlotRow_ToCompress);
            }

            var groupedSlotList = fsTimeSlotListToCompress
                .GroupBy(u => u.BranchLocationID == null ? -1 : u.BranchLocationID)
                .Select(group => new
                        {
                            BranchLocationID = group.Key,
                            FSTimeSlots = group.ToList()
                        })
                .OrderBy(group => group.BranchLocationID)
                .ToList();
            List<Scheduler.Slot> compressedSlots = new List<Scheduler.Slot>();

            foreach (FSBranchLocation fsBranchLocationRow in fsBranchLocationSet)
            {
                var existingBranchLocationList = groupedSlotList
                                                 .Select(bl => new
                                                         {
                                                             BranchLocationID = bl.BranchLocationID,
                                                             FSTimeSlot = bl.FSTimeSlots.ToList()
                                                         })
                                                         .Where(S => S.BranchLocationID == fsBranchLocationRow.BranchLocationID).ToList();
            
                fsTimeSlotListToCompress.Clear();
                slotListToCompress.Clear();

                if (existingBranchLocationList.Count > 0)
                {
                    fsTimeSlotListToCompress = existingBranchLocationList[0].FSTimeSlot;
                }

                if (groupedSlotList.Count > 0 && groupedSlotList[0].BranchLocationID == -1)
                {
                    fsTimeSlotListToCompress = fsTimeSlotListToCompress.Concat(groupedSlotList[0].FSTimeSlots).ToList();
                }
                
                foreach (FSTimeSlot fsTimeSlotRow_ToCompress in fsTimeSlotListToCompress)
                {
                    slotListToCompress.Add(ConvertFSTimeSlotToSlot(fsTimeSlotRow_ToCompress));
                }

                if (slotListToCompress.Count > 0)
                {
                    fsTimeSlotRow.BranchLocationID = fsBranchLocationRow.BranchLocationID;
                    CreateCompressedSlots(CompressAndIntersectSlots(slotListToCompress).ToList(), fsTimeSlotRow);
                }
            }
        }

        /// <summary>
        /// Create FSTimeSlot records based in the List [compressedSlots] and the FSTimeSLot record [fsTimeSlotRow].
        /// </summary>
        public virtual void CreateCompressedSlots(List<Scheduler.Slot> compressedSlots, FSTimeSlot fsTimeSlotRow)
        {
            TimeSlotMaint timeSlotMaintGraph = PXGraph.CreateInstance<TimeSlotMaint>();

            foreach (Scheduler.Slot slot in compressedSlots)
            {
                timeSlotMaintGraph.TimeSlotRecords.Insert(ConvertSlotToFSTimeSlot(slot, fsTimeSlotRow, ID.EmployeeTimeSlotLevel.COMPRESS));
            }

            timeSlotMaintGraph.Save.Press();
        }

        /// <summary>
        /// Delete the TimeSlots based in [slotLevel] that applies for the [fsTimeSlotRow].
        /// </summary>
        public virtual void DeleteTimeSlotsByLevel(FSTimeSlot fsTimeSlotRow, int slotLevel)
        {
            using (var ts = new PXTransactionScope())
            {
                List<object> cmdArg = new List<object>();

                TimeSlotMaint timeSlotMaintGraph = PXGraph.CreateInstance<TimeSlotMaint>();

                DateHandler dateHandler = new DateHandler(Convert.ToDateTime(fsTimeSlotRow.TimeStart));

                PXSelectBase<FSTimeSlot> fsTimeSlotsBaseToDelete = new PXSelect<FSTimeSlot,
                                                                       Where<
                                                                           FSTimeSlot.branchID, Equal<Required<FSTimeSlot.branchID>>,
                                                                           And<FSTimeSlot.employeeID, Equal<Required<FSTimeSlot.employeeID>>,
                                                                           And<FSTimeSlot.slotLevel, Equal<Required<FSTimeSlot.slotLevel>>,
                                                                           And<FSTimeSlot.timeStart, GreaterEqual<Required<FSTimeSlot.timeStart>>,
                                                                           And<FSTimeSlot.timeStart, LessEqual<Required<FSTimeSlot.timeStart>>>>>>>>
                                                                           (timeSlotMaintGraph);

                cmdArg.Add(fsTimeSlotRow.BranchID);
                cmdArg.Add(fsTimeSlotRow.EmployeeID);
                cmdArg.Add(slotLevel);
                cmdArg.Add(dateHandler.StartOfDay());
                cmdArg.Add(dateHandler.BeginOfNextDay());

                if (fsTimeSlotRow.BranchLocationID != null)
                {
                    fsTimeSlotsBaseToDelete.WhereAnd<Where<FSTimeSlot.branchLocationID, Equal<Required<FSTimeSlot.branchLocationID>>>>();
                    cmdArg.Add(fsTimeSlotRow.BranchLocationID);
                }

                var fsTimeSlotSet = fsTimeSlotsBaseToDelete.Select(cmdArg.ToArray());

                foreach (FSTimeSlot fsTimeSlotRow_ToDelete in fsTimeSlotSet)
                {
                    timeSlotMaintGraph.TimeSlotRecords.Delete(fsTimeSlotRow_ToDelete);
                }

                timeSlotMaintGraph.Save.Press();
                ts.Complete();
            }
        }

        #endregion

        #region Events

        #region FSTimeSlot

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSTimeSlot> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSTimeSlot> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSTimeSlot fsTimeSlotRow = (FSTimeSlot)e.Row;

            // Checking if is necessary compress the rules for an Employee for a specific day
            if (fsTimeSlotRow.SlotLevel == ID.EmployeeTimeSlotLevel.BASE && e.TranStatus == PXTranStatus.Completed)
            {
                DeleteTimeSlotsByLevel(fsTimeSlotRow, ID.EmployeeTimeSlotLevel.COMPRESS);
                CreateAndCompressedTimeSlots(fsTimeSlotRow, this);
            }
        }

        #endregion

        #endregion
    }
}