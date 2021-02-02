using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXPrimaryGraph(typeof(AccountMaintBridge))]
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
        #endregion
    }
}
