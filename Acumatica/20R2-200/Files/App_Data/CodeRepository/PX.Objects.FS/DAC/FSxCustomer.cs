using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    [PXTable(typeof(Customer.bAccountID), IsOptional = true)]
     public class FSxCustomer : PXCacheExtension<Customer>
     {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }
        [PXDBInt]
        [PXDefault(typeof(Search<FSxCustomerClass.dfltBillingCycleID,
                        Where<
                            CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>)
                , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Cycle")]
        [PXSelector(typeof(FSBillingCycle.billingCycleID), SubstituteKey = typeof(FSBillingCycle.billingCycleCD), DescriptionField = typeof(FSBillingCycle.descr))]
        public virtual int? BillingCycleID { get; set; }
        #endregion
        #region SendInvoicesTo
        public abstract class sendInvoicesTo : ListField_Send_Invoices_To
        {
        }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO)]
        [sendInvoicesTo.ListAtrribute]
        [PXUIField(DisplayName = "Bill-To Address")]
        public virtual string SendInvoicesTo { get; set; }
        #endregion
        #region BillShipmentSource
        public abstract class billShipmentSource : ListField_Ship_To
        {
        }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Ship_To.SERVICE_ORDER_ADDRESS)]
        [billShipmentSource.ListAtrribute]
        [PXUIField(DisplayName = "Ship-To Address")]
        public virtual string BillShipmentSource { get; set; }
        #endregion
        #region BillingOptionsChanged
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? BillingOptionsChanged { get; set; }
        #endregion
        #region DefaultBillingCustomerSource
        public abstract class defaultBillingCustomerSource : ListField_Default_Billing_Customer_Source
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Default_Billing_Customer_Source.SERVICE_ORDER_CUSTOMER)]
        [defaultBillingCustomerSource.ListAtrribute]
        [PXUIField(DisplayName = "Default Billing Customer")]
        public virtual string DefaultBillingCustomerSource { get; set; }
        #endregion
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Customer")]
        [FSSelectorCustomer]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSxCustomer.billCustomerID>>,
                            And<MatchWithBranch<Location.cBranchID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Billing Location")]
        [PXFormula(typeof(Default<billCustomerID>))]
        [PXDefault(typeof(Coalesce<
            Search2<BAccountR.defLocationID,
            InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<FSxCustomer.billCustomerID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.cBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<FSxCustomer.billCustomerID>>,
                And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXRestrictor(typeof(Where<Location.isActive, Equal<True>>), IN.Messages.InactiveLocation, typeof(Location.locationCD))]
        public virtual int? BillLocationID { get; set; }
        #endregion
        #region RequireCustomerSignature
        public abstract class requireCustomerSignature : PX.Data.BQL.BqlBool.Field<requireCustomerSignature> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Require Customer Signature on Mobile App")]
        public virtual bool? RequireCustomerSignature { get; set; }
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
