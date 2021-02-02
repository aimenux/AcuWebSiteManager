using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [PXTable(IsOptional = true)]
    public class FSxARPayment : PXCacheExtension<ARPayment>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Service Contract ID")]
        [FSSelectorContractRefNbrAttribute(
                                   typeof(ListField_RecordType_ContractSchedule.ServiceContract), 
                                   typeof(Where<FSServiceContract.status, Equal<FSServiceContract.status.Active>,
                                            And<FSServiceContract.billingType, Equal<ListField_Contract_BillingType.StandardizedBillings>>>))]
        public virtual int? ServiceContractID { get; set; }
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