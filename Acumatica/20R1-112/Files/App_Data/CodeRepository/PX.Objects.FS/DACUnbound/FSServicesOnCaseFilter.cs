using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSCreateServiceOrderOnCaseFilter : PX.Data.IBqlTable
    {
        #region AssignedEmpID
        public abstract class assignedEmpID : PX.Data.BQL.BqlInt.Field<assignedEmpID> { }

        [PXInt]
        [FSSelector_StaffMember_All]
        [PXUIField(DisplayName = "Assigned To")]
        public virtual int? AssignedEmpID { get; set; }
        #endregion

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type", Required = true)]
        [PXDefault(typeof(Coalesce<
            Search<FSxUserPreferences.dfltSrvOrdType,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>,
            Search<FSSetup.dfltSrvOrdType>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [FSSelectorActiveSrvOrdType]
        public virtual string SrvOrdType { get; set; }
        #endregion

        #region ProblemID
        public abstract class problemID : PX.Data.BQL.BqlInt.Field<problemID> { }

        [PXInt]
        [PXUIField(DisplayName = "Problem")]
        [PXSelector(typeof(Search2<FSProblem.problemID,
                            InnerJoin<FSSrvOrdTypeProblem,
                                On<FSSrvOrdTypeProblem.problemID, Equal<FSProblem.problemID>>>,
                            Where<FSSrvOrdTypeProblem.srvOrdType, Equal<Current<FSCreateServiceOrderOnCaseFilter.srvOrdType>>>>),
                    SubstituteKey = typeof(FSProblem.problemCD),
                    DescriptionField = typeof(FSProblem.descr))]
        public virtual int? ProblemID { get; set; }
        #endregion

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXInt]
        [PXUIField(DisplayName = "Branch Location", Required = true)]
        [PXDefault(typeof(
            Search<FSxUserPreferences.dfltBranchLocationID,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<PX.SM.UserPreferences.defBranchID, Equal<Current<FSServiceOrder.branchID>>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
                            Where<FSBranchLocation.branchID, Equal<Current<AccessInfo.branchID>>>>),
                            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
                            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
    }
}
