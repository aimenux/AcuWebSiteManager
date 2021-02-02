using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class EmployeeRoomHelper : IBqlTable
    {
        #region Distance
        public abstract class distance : PX.Data.BQL.BqlInt.Field<distance> { }

        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "Distance")]
        public virtual int? Distance { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXInt(IsKey = true)]

        //The SubstituteKey parameter as AcctName is used to correct an issue in Employee-Room screen (SD-5617)
        [FSSelector_StaffMember_All(parmSubstituteKey: typeof(BAccountSelectorBase.acctName))]
        [PXUIField(DisplayName = "Staff Member ID")]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXInt]
        [PXUIField(DisplayName = "Branch Location ID")]
        [PXSelector(typeof(FSBranchLocation.branchLocationID), SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region DateStart
        public abstract class dateStart : PX.Data.BQL.BqlDateTime.Field<dateStart> { }

        [PXDate(IsKey = true)]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? DateStart { get; set; }
        #endregion
        #region TimeStart
        public abstract class timeStart : PX.Data.BQL.BqlDateTime.Field<timeStart> { }

        [PXDateAndTime(UseTimeZone = true, DisplayMask = "t", InputMask = "t", IsKey = true)]
        [PXUIField(DisplayName = "Start Time")]
        public virtual DateTime? TimeStart { get; set; }
        #endregion                
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "Record ID")]
        public virtual int? RecordID { get; set; }
        #endregion        
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXString(60)]
        [PXUIField(DisplayName = "Room Description")]
        public virtual string Descr { get; set; }
        #endregion       
        #region Specific Use
        public abstract class specificUse : PX.Data.BQL.BqlBool.Field<specificUse> { }

        [PXBool]
        [PXUIField(DisplayName = "Exclusive")]
        public virtual bool? SpecificUse { get; set; }
        #endregion 
        #region EmployeeName
        public abstract class employeeName : PX.Data.BQL.BqlString.Field<employeeName> { }

        [PXString]
        [PXUIField(DisplayName = "Employee Name")]
        public virtual string EmployeeName { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }

        [PXString(10)]
        [PXUIField(DisplayName = "Room")]
        [PXSelector(typeof(FSRoom.roomID))]
        public virtual string RoomID { get; set; }
        #endregion        
        #region TimeEnd
        public abstract class timeEnd : PX.Data.BQL.BqlDateTime.Field<timeEnd> { }

        [PXDateAndTime(UseTimeZone = true, DisplayMask = "t", InputMask = "t")]
        [PXUIField(DisplayName = "End Time")]
        public virtual DateTime? TimeEnd { get; set; }
        #endregion        
        #region ValidOnFriday
        public abstract class validOnFriday : PX.Data.BQL.BqlBool.Field<validOnFriday> { }

        [PXBool]
        [PXUIField(DisplayName = "Fri")]
        public virtual bool? ValidOnFriday { get; set; }
        #endregion
        #region ValidOnMonday
        public abstract class validOnMonday : PX.Data.BQL.BqlBool.Field<validOnMonday> { }

        [PXBool]
        [PXUIField(DisplayName = "Mon")]
        public virtual bool? ValidOnMonday { get; set; }
        #endregion
        #region ValidOnSaturday
        public abstract class validOnSaturday : PX.Data.BQL.BqlBool.Field<validOnSaturday> { }

        [PXBool]
        [PXUIField(DisplayName = "Sat")]
        public virtual bool? ValidOnSaturday { get; set; }
        #endregion
        #region ValidOnSunday
        public abstract class validOnSunday : PX.Data.BQL.BqlBool.Field<validOnSunday> { }

        [PXBool]
        [PXUIField(DisplayName = "Sun")]
        public virtual bool? ValidOnSunday { get; set; }
        #endregion
        #region ValidOnThursday
        public abstract class validOnThursday : PX.Data.BQL.BqlBool.Field<validOnThursday> { }

        [PXBool]
        [PXUIField(DisplayName = "Thr")]
        public virtual bool? ValidOnThursday { get; set; }
        #endregion
        #region ValidOnTuesday
        public abstract class validOnTuesday : PX.Data.BQL.BqlBool.Field<validOnTuesday> { }

        [PXBool]
        [PXUIField(DisplayName = "Tue")]
        public virtual bool? ValidOnTuesday { get; set; }
        #endregion
        #region ValidOnWednesday
        public abstract class validOnWednesday : PX.Data.BQL.BqlBool.Field<validOnWednesday> { }

        [PXBool]
        [PXUIField(DisplayName = "Wed")]
        public virtual bool? ValidOnWednesday { get; set; }
        #endregion
        #region ServiceEstimatedDuration
        public abstract class serviceEstimatedDuration : PX.Data.BQL.BqlInt.Field<serviceEstimatedDuration> { }

        [PXInt]
        public virtual int? ServiceEstimatedDuration { get; set; }
        #endregion
        #region DistanceInMiles
        public abstract class distanceInMiles : PX.Data.BQL.BqlString.Field<distanceInMiles> { }

        [PXString]
        [PXUIField(DisplayName = "Distance")]
        public virtual string DistanceInMiles { get; set; }
        #endregion
    }
}