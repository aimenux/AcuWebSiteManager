using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(StaffContractScheduleEntry))]
    public class FSStaffSchedule : FSSchedule
	{
        #region CustomerID
        [PXDBInt]
        public override int? CustomerID { get; set; }
        #endregion
        
        #region SrvOrdType
        [PXDBString(4, IsFixed = true)]
        public override string SrvOrdType { get; set; }
        #endregion

        #region BranchID
        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public override int? BranchID { get; set; }
        #endregion

        #region BranchLocationID
        [PXDBInt]
        [PXUIField(DisplayName = "Branch Location ID")]
        [FSSelectorBranchLocationByFSSchedule]
        public override int? BranchLocationID { get; set; }
        #endregion

        #region StaffScheduleDescription
        public abstract class staffScheduleDescription : PX.Data.BQL.BqlString.Field<staffScheduleDescription> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string StaffScheduleDescription { get; set; }
        #endregion

        #region EmployeeID
        [PXDBInt]
        [PXDefault]
        [FSSelector_Employee_All]
        [PXUIField(DisplayName = "Staff Member")]
        public override int? EmployeeID { get; set; }
        #endregion

        #region StartDate
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Start Date", DisplayNameTime = "Start Time")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible)]
        public override DateTime? StartDate { get; set; }
        #endregion

        #region EndDate
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "End Date", DisplayNameTime = "End Time")]
        [PXDefault]
        [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
        public override DateTime? EndDate { get; set; }
        #endregion

        #region RefNbr
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(Enabled = true, DisplayName = "Staff Schedule Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [AutoNumber(typeof(Search<FSSetup.empSchdlNumberingID>), typeof(AccessInfo.businessDate))]

        //The lookup columns have to be handled explicitly because the SelectorVisible attribute was duplicating them
        [PXSelector(typeof(Search<FSStaffSchedule.refNbr, Where<FSStaffSchedule.entityType, Equal<entityType.Employee>>,
                    OrderBy<Desc<FSStaffSchedule.refNbr>>>), 
                    new Type[] { 
                                typeof(FSStaffSchedule.refNbr),
                                typeof(FSStaffSchedule.employeeID),
                                typeof(FSStaffSchedule.branchID),
                                typeof(FSStaffSchedule.scheduleType),
                                typeof(FSStaffSchedule.startDate),
                                typeof(FSStaffSchedule.endDate),
                                typeof(FSStaffSchedule.mem_StartTime_Time),
                                typeof(FSStaffSchedule.mem_EndTime_Time) })]
        public override string RefNbr { get; set; }
        #endregion

        #region EntityType
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Schedule_EntityType.EMPLOYEE)]
        [entityType.ListAtrribute]
        [PXUIField(DisplayName = "Entity Type", Enabled = false)]
        public override string EntityType { get; set; }
        #endregion

        #region ScheduleType
        [scheduleType.ListAtrribute]
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Schedule Type")]
        [PXDefault(ID.ScheduleType.AVAILABILITY)]
        public override string ScheduleType { get; set; }
        #endregion

        #region StartTime
        public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameTime = "Start Time", InputMask = "t")]
        [PXUIField(DisplayName = "Start Time")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? StartTime { get; set; }
        #endregion

        #region EndTime
        public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameTime = "End Time", InputMask = "t")]
        [PXUIField(DisplayName = "End Time")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? EndTime { get; set; }
        #endregion

        #region MemoryFields
        #region Mem_StartTime_Time
        public abstract class mem_StartTime_Time : PX.Data.BQL.BqlString.Field<mem_StartTime_Time> { }

        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Start Time")]
        public virtual string Mem_StartTime_Time
        {
            get
            {
                return SharedFunctions.GetTimeStringFromDate(this.StartTime);
            }
        }
        #endregion

        #region Mem_EndTime_Time
        public abstract class mem_EndTime_Time : PX.Data.BQL.BqlString.Field<mem_EndTime_Time> { }

        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "End Time")]
        public virtual string Mem_EndTime_Time
        {
            get
            {
                return SharedFunctions.GetTimeStringFromDate(this.EndTime);
            }
        }
        #endregion
        #endregion
    }
}