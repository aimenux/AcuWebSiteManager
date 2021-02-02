using PX.Data;
using PX.Objects.AR;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public partial class CreateInvoiceFilter : IBqlTable
    {
        #region PostTo
        public abstract class postTo : PX.Data.BQL.BqlString.Field<postTo>
        {
        }

        [PXString(2, IsFixed = true)]
        [FSPostTo.List]
        [PXDefault(ID.Batch_PostTo.AR_AP)]
        [PXUIField(DisplayName = "Generated Billing Documents")]
        public virtual string PostTo { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXInt]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch")]
        public virtual int? BranchID { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Billing Customer")]
        [FSSelectorCustomer]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region UpToDate

        public abstract class upToDate : PX.Data.BQL.BqlDateTime.Field<upToDate> { }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Up to Date")]
        public virtual DateTime? UpToDate { get; set; }
        #endregion
        #region InvoiceDate
        public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }

        [PXDate]
        [PXFormula(typeof(CreateInvoiceFilter.upToDate))]
        [PXUIField(DisplayName = "Billing Date")]
        public virtual DateTime? InvoiceDate { get; set; }
        #endregion
        #region InvoiceFinPeriodID
        public abstract class invoiceFinPeriodID : PX.Data.BQL.BqlString.Field<invoiceFinPeriodID> { }

        [SMOpenPeriod(typeof(CreateInvoiceFilter.invoiceDate), typeof(CreateInvoiceFilter.postTo), typeof(branchID))]
        [PXUIField(DisplayName = "Billing Period", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string InvoiceFinPeriodID { get; set; }
        #endregion
        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }

        [PXInt]
        [PXUIField(DisplayName = "Billing Cycle")]
        [PXSelector(typeof(FSBillingCycle.billingCycleID),
                    SubstituteKey = typeof(FSBillingCycle.billingCycleCD),
                    DescriptionField = typeof(FSBillingCycle.descr))]
        public virtual int? BillingCycleID { get; set; }
        #endregion
        #region IgnoreBillingCycles
        public abstract class ignoreBillingCycles : PX.Data.BQL.BqlBool.Field<ignoreBillingCycles> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ignore the Time Frame")]
        public virtual bool? IgnoreBillingCycles { get; set; }
        #endregion
        #region LoadData
        public abstract class loadData : PX.Data.BQL.BqlBool.Field<loadData> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Filter Manually", Visible = false)]
        public virtual bool? LoadData { get; set; }
        #endregion
        #region ReleaseInvoice
        public abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<prepareInvoice, Equal<True>>))]
        [PXFormula(typeof(Default<prepareInvoice>))]
        [PXUIField(DisplayName = "Release Invoice")]
        public virtual bool? ReleaseInvoice { get; set; }
        #endregion
        #region EmailInvoice
        public abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Email Invoice")]
        public virtual bool? EmailInvoice { get; set; }
        #endregion
        #region ReleaseBill
        public abstract class releaseBill : PX.Data.BQL.BqlBool.Field<releaseBill> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Release Bill")]
        public virtual bool? ReleaseBill { get; set; }
        #endregion
        #region PayBill
        public abstract class payBill : PX.Data.BQL.BqlBool.Field<payBill> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Pay Bill")]
        public virtual bool? PayBill { get; set; }
        #endregion
        #region PrepareInvoice
        public abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Prepare Invoice")]
        [PXUIEnabled(typeof(Where<sOQuickProcess, Equal<False>>))]
        public virtual bool? PrepareInvoice { get; set; }
        #endregion
        #region EmailSalesOrder
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Email Sales Order/Quote")]
        public virtual bool? EmailSalesOrder { get; set; }
        #endregion
        #region SOQuickProcess
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Use Sales Order Quick Processing")]
        [PXUIEnabled(typeof(Where<prepareInvoice, Equal<False>>))]
        public virtual bool? SOQuickProcess { get; set; }
        #endregion
        #region GenerateInvoiceScreen
        public abstract class isGenerateInvoiceScreen : PX.Data.IBqlField
        {
        }

        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual bool? IsGenerateInvoiceScreen { get; set; }
        #endregion
    }
}
