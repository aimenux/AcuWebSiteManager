using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class ServiceContractFilter : IBqlTable
    {
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorCustomer]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Service Contract ID")]
        [FSSelectorContractRefNbrAttribute(typeof(ListField_RecordType_ContractSchedule.ServiceContract))]
        public virtual string RefNbr { get; set; }
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
        [PXDefault(
            typeof(Search<FSBranchLocation.branchLocationID,
                   Where<
                        FSBranchLocation.branchID, Equal<Current<ServiceContractFilter.branchID>>>>), 
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID, Where<FSBranchLocation.branchID, Equal<Current<ServiceContractFilter.branchID>>>>),
                        SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<ServiceContractFilter.customerID>>>),
            DescriptionField = typeof(Location.descr), DisplayName = "Location")]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region ToDate
        public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

        [PXDateAndTime(UseTimeZone = false)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Generate Up To", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ToDate { get; set; }
        #endregion
        #region ScheduleID
        //Not shown on screen: Needed to filter one schedule when ServiceContractInq is launched from ServiceContractScheduleEntry
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXInt]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region ActionType
        public abstract class actionType : ListField_ActionType_ProcessServiceContracts
        {
        }

        [PXString]
        [PXDefault(ID.ActionType_ProcessServiceContracts.STATUS)]
        [actionType.ListAtrribute]
        [PXUIField(DisplayName = "Action")]
        public virtual string ActionType { get; set; }
        #endregion

    }
}