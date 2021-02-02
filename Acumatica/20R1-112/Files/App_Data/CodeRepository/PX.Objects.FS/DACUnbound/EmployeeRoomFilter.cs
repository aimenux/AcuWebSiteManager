/*
TODO - SD-7798
using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class EmployeeRoomFilter : IBqlTable
    {
        #region ServiceID
        public abstract class serviceID : PX.Data.BQL.BqlInt.Field<serviceID> { }

        [PXInt]
        [PXDefault]
        [ServiceInventory]
        public virtual int? ServiceID { get; set; }
        #endregion
        #region BranchLocation
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXInt]
        [PXDefault] //TODO SD-5257
        [PXUIField(DisplayName = "Branch Location ID")]
        [PXSelector(typeof(FSBranchLocation.branchLocationID), SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Staff Member ID")]
        [FSSelector_StaffMember_All]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }

        [PXString(10)]
        [PXUIField(DisplayName = "Room ID")]
        [PXSelector(typeof(Search<FSRoom.roomID, Where<FSRoom.branchLocationID, Equal<Current<EmployeeRoomFilter.branchLocationID>>>>))]
        public virtual string RoomID { get; set; }
        #endregion
        #region FromDate
        public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? FromDate { get; set; }
        #endregion
        #region ToDate
        public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXDefault]
        [PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ToDate { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorBusinessAccount_CU_PR_VC]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [PXInt]
        [PXUIField(DisplayName = "Customer Location ID")]
        [FSSelectorBusinessAccountLocationFilter]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region RoomAvailability
        public abstract class roomAvailability : PX.Data.BQL.BqlBool.Field<roomAvailability> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Consider Availability per Room")]
        public virtual bool? RoomAvailability { get; set; }
        #endregion
        #region ConsiderLicenses
        public abstract class considerLicenses : PX.Data.BQL.BqlBool.Field<considerLicenses> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Consider the Employee's licenses")]
        public virtual bool? ConsiderLicenses { get; set; }
        #endregion
        #region ConsiderSkills
        public abstract class considerSkills : PX.Data.BQL.BqlBool.Field<considerSkills> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Consider the Employee's skills")]
        public virtual bool? ConsiderSkills { get; set; }
        #endregion
    }
}*/