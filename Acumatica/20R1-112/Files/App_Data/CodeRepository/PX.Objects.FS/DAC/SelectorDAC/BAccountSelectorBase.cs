using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using System;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [CRCacheIndependentPrimaryGraphList(new Type[]{
        typeof(AR.CustomerMaint),
        typeof(AP.VendorMaint),
        typeof(EP.EmployeeMaint),
        typeof(CR.BusinessAccountMaint)},
        new Type[]{
            typeof(Select<AR.Customer, Where<Current<BAccount.bAccountID>, Less<Zero>,
                            Or<AR.Customer.bAccountID, Equal<Current<BAccount.bAccountID>>>>>),
            typeof(Select<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
            typeof(Select<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
            typeof(Select<CR.BAccount,
                Where2<Where<
                    CR.BAccount.type, Equal<BAccountType.prospectType>,
                    Or<CR.BAccount.type, Equal<BAccountType.customerType>,
                    Or<CR.BAccount.type, Equal<BAccountType.vendorType>,
                    Or<CR.BAccount.type, Equal<BAccountType.combinedType>>>>>,
                        And<Where<CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
                        Or<Current<BAccount.bAccountID>, Less<Zero>>>>>>)
        })]
    public class BAccountSelectorBase : BAccount
    {
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        #endregion
        #region AcctCD
        public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

        [PXDimensionSelector("BIZACCT", typeof(BAccount.acctCD), typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault()]
        [PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFieldDescription]
        public override string AcctCD { get; set; }
        #endregion
        #region AcctName
        public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

        [PXDBString(60, IsUnicode = true)]
        [PXDefault]
        [PXFieldDescription]
        [PXMassMergableField]
        [PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
        public override string AcctName { get; set; }
        #endregion
        #region Type
        public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [EmployeeType.List()]
        public override string Type { get; set; }
        #endregion
    }
}
