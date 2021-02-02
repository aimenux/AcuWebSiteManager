using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSCreateServiceOrderFilter : PX.Data.IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type", Required = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [FSSelectorActiveSrvOrdType]
        public virtual string SrvOrdType { get; set; }
        #endregion

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXInt]
        [PXDefault(typeof(
            Search<FSxUserPreferences.dfltBranchLocationID,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<PX.SM.UserPreferences.defBranchID, Equal<Current<FSServiceOrder.branchID>>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
                            Where<FSBranchLocation.branchID, Equal<Current<AccessInfo.branchID>>>>),
                            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
                            DescriptionField = typeof(FSBranchLocation.descr))]
        [PXUIField(DisplayName = "Branch Location", Required = true)]
        public virtual int? BranchLocationID { get; set; }
        #endregion

        #region AssignedEmpID
        public abstract class assignedEmpID : PX.Data.BQL.BqlInt.Field<assignedEmpID> { }

        [PXInt]
        [FSSelector_StaffMember_All]
        [PXUIField(DisplayName = "Assigned To")]
        public virtual int? AssignedEmpID { get; set; }
        #endregion

        #region SLAETA
        public abstract class sLAETA : PX.Data.BQL.BqlDateTime.Field<sLAETA> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Deadline - SLA")]
        public virtual DateTime? SLAETA { get; set; }
        #endregion
    }
}
