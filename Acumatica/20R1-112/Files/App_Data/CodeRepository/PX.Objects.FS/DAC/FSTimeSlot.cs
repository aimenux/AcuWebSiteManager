using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.FS
{
	[System.SerializableAttribute]
	public class FSTimeSlot : PX.Data.IBqlTable
	{
        #region TimeSlotID
        public abstract class timeSlotID : PX.Data.BQL.BqlInt.Field<timeSlotID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? TimeSlotID { get; set; }

        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Branch ID")]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD))]
        public virtual int? BranchID { get; set; }
        #endregion
		#region BranchLocationID
		public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Branch Location ID")]
        public virtual int? BranchLocationID { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Employee ID")]
        public virtual int? EmployeeID { get; set; }
		#endregion
		#region TimeStart
		public abstract class timeStart : PX.Data.BQL.BqlDateTime.Field<timeStart> { }

        protected DateTime? _TimeStart;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true)]
        [PXUIField(DisplayName = "Time Start")]
        public virtual DateTime? TimeStart
        {
            get
            {
                return this._TimeStart;
            }

            set
            {
                this.TimeStartUTC = value;
                this._TimeStart = value;
            }
        }
        #endregion
        #region TimeEnd
        public abstract class timeEnd : PX.Data.BQL.BqlDateTime.Field<timeEnd> { }

        protected DateTime? _TimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true)]
        [PXUIField(DisplayName = "Time End")]
        public virtual DateTime? TimeEnd
        {
            get
            {
                return this._TimeEnd;
            }

            set
            {
                this.TimeEndUTC = value;
                this._TimeEnd = value;
            }
        }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Schedule ID")]
        public virtual int? ScheduleID { get; set; }
		#endregion
        #region RecordCount
		public abstract class recordCount : PX.Data.BQL.BqlInt.Field<recordCount> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Record Count")]
        public virtual int? RecordCount { get; set; }
		#endregion
        #region ScheduleType
        public abstract class scheduleType : ListField_ScheduleType
        {
        }

        [PXDBString(1)]
        [scheduleType.ListAtrribute]
        public virtual string ScheduleType { get; set; }
        #endregion
        #region GenerationID
        public abstract class generationID : PX.Data.BQL.BqlInt.Field<generationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Generation ID")]
        public virtual int? GenerationID { get; set; }
        #endregion
        #region TimeDiff
        public abstract class timeDiff : PX.Data.BQL.BqlDecimal.Field<timeDiff> { }

        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Time Difference", Enabled = false)]
        public virtual decimal? TimeDiff { get; set; }
        #endregion
        #region SlotLevel
        public abstract class slotLevel : PX.Data.BQL.BqlInt.Field<slotLevel> { }

        [PXDBInt]
        [PXDefault(ID.EmployeeTimeSlotLevel.BASE)]
        [PXUIField(DisplayName = "Slot Level")]
        public virtual int? SlotLevel { get; set; }
        #endregion
        #region CalendarHelpers
        #region CustomID
        public abstract class customID : PX.Data.BQL.BqlString.Field<customID> { }

        [PXString]        
        public virtual string CustomID { get; set; }
        #endregion
        #region CustomDateID
        public abstract class customDateID : PX.Data.BQL.BqlString.Field<customID> { }

        [PXString]
        public virtual string CustomDateID { get; set; }
        #endregion
        #region WrkEmployeeScheduleID
        public abstract class wrkEmployeeScheduleID : PX.Data.BQL.BqlString.Field<wrkEmployeeScheduleID> { }

        [PXString]
        public virtual string WrkEmployeeScheduleID { get; set; }
        #endregion
        #region BranchLocationDesc
        public abstract class branchLocationDesc : PX.Data.BQL.BqlString.Field<branchLocationDesc> { }

        [PXString]
        public virtual string BranchLocationDesc { get; set; }
        #endregion
        #region BranchLocationCD
        public abstract class branchLocationCD : PX.Data.BQL.BqlString.Field<branchLocationCD> { }

        [PXString]
        public virtual string BranchLocationCD { get; set; }
        #endregion
        #region CustomDateTimeStart
        public abstract class customDateTimeStart : PX.Data.BQL.BqlString.Field<customDateTimeStart> { }

        [PXString]
        public virtual string CustomDateTimeStart 
        {
            get 
            { 
                //Value cannot be calculated with PXFormula attribute
                if (this.TimeStart != null)
                {
                    return this.TimeStart.ToString(); 
                }

                return string.Empty;
            }
        }
        #endregion
        #region CustomDateTimeEnd
        public abstract class customDateTimeEnd : PX.Data.BQL.BqlString.Field<customDateTimeEnd> { }

        [PXString]
        public virtual string CustomDateTimeEnd 
        { 
            get 
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.TimeEnd != null)
                {
                    return this.TimeEnd.ToString();
                }

                return string.Empty;
            } 
        }
        #endregion
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }

        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }

        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }

        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }

        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }

        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }

		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region UTC Fields

		#region TimeStartUTC
		public abstract class timeStartUTC : PX.Data.BQL.BqlDateTime.Field<timeStartUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true)]
        [PXUIField(DisplayName = "Time Start")]
        public virtual DateTime? TimeStartUTC { get; set; }
        #endregion
        #region TimeEndUTC
        public abstract class timeEndUTC : PX.Data.BQL.BqlDateTime.Field<timeEndUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true)]
        [PXUIField(DisplayName = "Time End")]
        public virtual DateTime? TimeEndUTC { get; set; }
        #endregion
        #endregion
    }
}