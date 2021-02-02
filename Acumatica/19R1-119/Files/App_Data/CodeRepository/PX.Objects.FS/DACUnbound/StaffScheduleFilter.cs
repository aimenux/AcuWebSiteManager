using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class StaffScheduleFilter : IBqlTable
    {
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        [PXInt]
        [PXUIField(DisplayName = "Employee Name")]
        [FSSelector_Employee_All]
        public virtual int? BAccountID { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXInt]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXInt]
        [PXDefault(typeof(Search<FSBranchLocation.branchLocationID,
                        Where<FSBranchLocation.branchID, Equal<Current<ServiceContractFilter.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [FSSelectorBranchLocation]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region ToDate
        public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Generate Up To", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ToDate { get; set; }
        #endregion
        #region ScheduleID
        //Not shown on screen: Needed to filter one schedule when StaffScheduleProcess is launched from StaffContractScheduleEntry
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXInt]
        public virtual int? ScheduleID { get; set; }
        #endregion
    }
}