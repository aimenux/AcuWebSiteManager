using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSContractPeriodFilter : PX.Data.IBqlTable
    {
        #region Actions
        public abstract class actions : ListField_ContractPeriod_Actions
        {
        }

        [PXString(3, IsFixed = true)]
        [actions.ListAtrribute]
        [PXUIField(DisplayName = "Actions")]
        public virtual string Actions { get; set; }
        #endregion    
        #region ContractPeriodID
        public abstract class contractPeriodID : PX.Data.BQL.BqlInt.Field<contractPeriodID> { }

        [PXInt]
        [PXUIField(DisplayName = "Billing Period")]
        [FSSelectorContractBillingPeriod]
        [PXDefault(typeof(Search<FSContractPeriod.contractPeriodID,
                            Where2<
                                Where<Current<FSContractPeriodFilter.actions>, Equal<FSContractPeriodFilter.actions.ModifyBillingPeriod>,
                                    And<FSContractPeriod.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>,
                                    And<FSContractPeriod.status, Equal<FSContractPeriod.status.Inactive>>>>,
                                Or<Where<Current<FSContractPeriodFilter.actions>, Equal<FSContractPeriodFilter.actions.SearchBillingPeriod>,
                                        And<FSContractPeriod.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>,
                                        And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>>>,
                            OrderBy<Desc<FSContractPeriod.startPeriodDate>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        
        public virtual int? ContractPeriodID { get; set; }
        #endregion
        #region StandardizedBillingTotal
        public abstract class standardizedBillingTotal : PX.Data.BQL.BqlDecimal.Field<standardizedBillingTotal> { }

        [PXBaseCury]
        [PXUIField(DisplayName = "Standardized Billing Total", IsReadOnly = true)]
        public virtual decimal? StandardizedBillingTotal { get; set; }
        #endregion
        #region PostDocRefNbr
        public abstract class postDocRefNbr : PX.Data.BQL.BqlString.Field<postDocRefNbr> { }

        [PXString]
        [PXUIField(DisplayName = "Reference Nbr.", IsReadOnly = true)]
        public virtual string PostDocRefNbr { get; set; }
        #endregion
    }
}
