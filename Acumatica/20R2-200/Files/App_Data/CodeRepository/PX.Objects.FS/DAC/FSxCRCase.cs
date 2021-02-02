using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [PXTable(typeof(CRCase.caseCD), IsOptional = true)]
    public class FSxCRCase : PXCacheExtension<CRCase>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
        }

        #region AssignedEmpID
        public abstract class assignedEmpID : PX.Data.BQL.BqlInt.Field<assignedEmpID> { }

        [PXDBInt]
        [FSSelector_StaffMember_All]
        [PXUIField(DisplayName = "Assigned To")]
        public virtual int? AssignedEmpID { get; set; }
        #endregion

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type", Enabled = false, Required = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActiveSrvOrdType]
        public virtual string SrvOrdType { get; set; }
        #endregion

        #region ProblemID
        public abstract class problemID : PX.Data.BQL.BqlInt.Field<problemID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Problem", Enabled = false)]
        [PXSelector(typeof(Search2<FSProblem.problemID,
                            InnerJoin<FSSrvOrdTypeProblem,
                                On<FSSrvOrdTypeProblem.problemID, Equal<FSProblem.problemID>>>,
                            Where<FSSrvOrdTypeProblem.srvOrdType, Equal<Current<FSxCRCase.srvOrdType>>>>), 
                    SubstituteKey = typeof(FSProblem.problemCD),
                    DescriptionField = typeof(FSProblem.descr))]
        public virtual int? ProblemID { get; set; }
        #endregion

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location", Required = true)]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
                            Where<FSBranchLocation.branchID, Equal<Current<AccessInfo.branchID>>>>),
                            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
                            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion

        #region SDEnabled
        public abstract class sDEnabled : PX.Data.BQL.BqlBool.Field<sDEnabled> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Create Service Order")]
        public virtual bool? SDEnabled { get; set; }
        #endregion

        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
        [PXSelector(typeof(FSServiceOrder.sOID), SubstituteKey = typeof(FSServiceOrder.refNbr))]
        public virtual int? SOID { get; set; }
        #endregion
    }
}
