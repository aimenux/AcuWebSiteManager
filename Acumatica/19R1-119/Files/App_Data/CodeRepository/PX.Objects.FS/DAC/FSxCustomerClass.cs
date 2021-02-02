using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [PXTable(typeof(CustomerClass.customerClassID), IsOptional = true)]
     public class FSxCustomerClass : PXCacheExtension<CustomerClass>
     {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region DfltBillingCycle
        public abstract class dfltBillingCycleID : PX.Data.BQL.BqlInt.Field<dfltBillingCycleID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Billing Cycle")]
        [PXSelector(typeof(FSBillingCycle.billingCycleID), SubstituteKey = typeof(FSBillingCycle.billingCycleCD), DescriptionField = typeof(FSBillingCycle.descr))]
        public virtual int? DfltBillingCycleID { get; set; }
        #endregion
        #region SendInvoicesTo
        public abstract class sendInvoicesTo : ListField_Send_Invoices_To
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO, PersistingCheck = PXPersistingCheck.Nothing)]
        [sendInvoicesTo.ListAtrribute]
        [PXUIField(DisplayName = "Bill-To Address")]
        public virtual string SendInvoicesTo { get; set; }
        #endregion
        #region BillShipmentSource
        public abstract class billShipmentSource : ListField_Ship_To
        {
        }

        [PXDBString(2, IsFixed = true)]
        [billShipmentSource.ListAtrribute]
        [PXDefault(ID.Ship_To.SERVICE_ORDER_ADDRESS, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ship-To Address")]
        public virtual string BillShipmentSource { get; set; }
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

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSxCustomerClass.billCustomerID>>,
                            And<MatchWithBranch<Location.cBranchID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Billing Location")]
        [PXRestrictor(typeof(Where<Location.isActive, Equal<True>>), IN.Messages.InactiveLocation, typeof(Location.locationCD))]
        public virtual int? BillLocationID { get; set; }
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
