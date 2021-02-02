using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [PXTable(typeof(CROpportunity.opportunityID), IsOptional = true)]
    public class FSxCROpportunity : PXCacheExtension<CROpportunity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
        }


        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<CROpportunity.branchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActiveSrvOrdType]
        [PXFormula(typeof(Default<FSxCROpportunity.sDEnabled>))]
        public virtual string SrvOrdType { get; set; }
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
        [PXSelector(typeof(
            Search<FSServiceOrder.sOID,
            Where<
                FSServiceOrder.sOID, Equal<Current<FSxCROpportunity.sOID>>>>),
            SubstituteKey = typeof(FSServiceOrder.refNbr))]
        public virtual int? SOID { get; set; }
        #endregion
        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }

        [PXBool]
        [PXUIField(Visible = false)]
        public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}

namespace PX.Objects.FS.Opportunity
{
    ///This cache extension is needed because screen OP000373 is working with PX.Objects.CR.CROpportunity what is a PXProjection
    ///and not with PX.Objects.CR.Standalone wich is the table in the Database. Without this, fields are not saved in the DB.
    [PXTable(typeof(CR.Standalone.CROpportunity.opportunityID), IsOptional = true)]
    public class FSxCROpportunity : PXCacheExtension<CR.Standalone.CROpportunity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
        }


        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<CROpportunity.branchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActiveSrvOrdType]
        [PXFormula(typeof(Default<FSxCROpportunity.sDEnabled>))]
        public virtual string SrvOrdType { get; set; }
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
        [PXSelector(typeof(
            Search<FSServiceOrder.sOID,
            Where<
                FSServiceOrder.sOID, Equal<Current<FSxCROpportunity.sOID>>>>),
            SubstituteKey = typeof(FSServiceOrder.refNbr))]
        public virtual int? SOID { get; set; }
        #endregion
        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }

        [PXBool]
        [PXUIField(Visible = false)]
        public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}