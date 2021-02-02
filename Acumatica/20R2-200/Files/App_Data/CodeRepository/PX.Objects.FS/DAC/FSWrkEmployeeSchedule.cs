using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.FS
{
	[System.SerializableAttribute]
	public class FSWrkEmployeeSchedule : PX.Data.IBqlTable
	{
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch ID")]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD))]
        public virtual int? BranchID { get; set; }
        #endregion
		#region BranchLocationID
		public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Branch Location ID")]
        public virtual int? BranchLocationID { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Employee ID")]
        public virtual int? EmployeeID { get; set; }
		#endregion
		#region TimeStart
		public abstract class timeStart : PX.Data.BQL.BqlDateTime.Field<timeStart> { }

        protected DateTime? _TimeStart;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, IsKey = true)]
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
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Record ID")]
        public virtual int? RecordID { get; set; }
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

        #region TimeDiff
        public abstract class timeDiff : PX.Data.BQL.BqlDecimal.Field<timeDiff> { }

        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Time Difference", Enabled = false)]
        public virtual decimal? TimeDiff { get; set; }
        #endregion

        #region CalendarHelpers
        #region CustomID
        public abstract class customID : PX.Data.BQL.BqlString.Field<customID> { }

        [PXString]        
        public virtual string CustomID { get; set; }
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
        #region ScheduleRefNbr
        public abstract class scheduleRefNbr : PX.Data.BQL.BqlString.Field<scheduleRefNbr> { }

        [PXString]
        public virtual string ScheduleRefNbr { get; set; }
        #endregion
        #endregion

        #region UTC Fields
        #region TimeStartUTC
        public abstract class timeStartUTC : PX.Data.BQL.BqlDateTime.Field<timeStartUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, IsKey = true)]
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