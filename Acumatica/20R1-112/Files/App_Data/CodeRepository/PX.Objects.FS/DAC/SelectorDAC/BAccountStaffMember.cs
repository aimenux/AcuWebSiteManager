using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class BAccountStaffMember : BAccountSelectorBase
    {
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        #endregion
        #region AcctCD
        [PXDimensionSelector(BAccountAttribute.DimensionName, typeof(BAccount.acctCD), typeof(BAccount.acctCD))]
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Staff Member ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFieldDescription]
        public override string AcctCD { get; set; }
        #endregion
        #region AcctName
        [PXDBString(60, IsUnicode = true)]
        [PXDefault]
        [PXFieldDescription]
        [PXMassMergableField]
        [PXUIField(DisplayName = "Staff Member Name", Visibility = PXUIVisibility.SelectorVisible)]
        public override string AcctName { get; set; }
        #endregion       
        #region ParentBAccountID
        [PXDBInt]
        [PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDimensionSelector(BAccountAttribute.DimensionName,
            typeof( 
                Search2<BAccountR.bAccountID,
                LeftJoin<Contact, 
                    On<Contact.bAccountID, Equal<BAccountR.bAccountID>, 
                    And<Contact.contactID, Equal<BAccountR.defContactID>>>,
                LeftJoin<Address, 
                    On<Address.bAccountID, Equal<BAccountR.bAccountID>, 
                    And<Address.addressID, Equal<BAccountR.defAddressID>>>>>,
                Where<BAccountR.type, Equal<BAccountType.customerType>,
                    Or<BAccountR.type, Equal<BAccountType.prospectType>,
                    Or<BAccountR.type, Equal<BAccountType.combinedType>,
                    Or<BAccountR.type, Equal<BAccountType.vendorType>>>>>>),
            typeof(BAccountR.acctCD),
            typeof(BAccountR.acctCD), 
            typeof(BAccountR.acctName), 
            typeof(BAccountR.type), 
            typeof(BAccountR.classID),
            typeof(BAccountR.status), 
            typeof(Contact.phone1), 
            typeof(Address.city), 
            typeof(Address.countryID), 
            typeof(Contact.eMail),
            DescriptionField = typeof(BAccountR.acctName))]
        public override int? ParentBAccountID { get; set; }
        #endregion
        #region Selected
        public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public override bool? Selected { get; set; }
        #endregion
    }
}
