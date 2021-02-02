namespace PX.Objects.CT
{
	using System;
	using PX.Data;
    using PX.Objects.CR;

	[PXPrimaryGraph(typeof(ContractMaint))]
	[System.SerializableAttribute()]
    [PXHidden]
	public partial class ContractWatcher : PX.Data.IBqlTable
	{
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		protected Int32? _ContractID;
		
        [PXDBLiteDefault(typeof(Contract.contractID))]
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Contract ID")]
        [PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<ContractWatcher.contractID>>>>))]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
        protected Int32? _ContactID;
        [PXDBInt()]
        [PXUIField(DisplayName = "Contact")]
		//[PXSelector(typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypes.person>,
		//    And<Where<Contact.eMail, Equal<Optional<ContractWatcher.eMail>>,
		//   Or<Current<ContractWatcher.eMail>, IsNull>>>>>))]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypesAttribute.person>>>), Filterable = true)]
		public virtual Int32? ContactID
        {
            get
            {
                return this._ContactID;
            }
            set
            {
                this._ContactID = value;
            }
        }
        #endregion
        #region WatchTypeID
        public abstract class watchTypeID : PX.Data.BQL.BqlString.Field<watchTypeID> { }
        protected String _WatchTypeID;
        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Watch Type")]
        [WatchType.List()]
        public virtual String WatchTypeID
        {
            get
            {
                return this._WatchTypeID;
            }
            set
            {
                this._WatchTypeID = value;
            }
        }
        #endregion
		#region EMail
		public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }
		protected String _EMail;
		[PXDBEmail(IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault()]
		public virtual String EMail
		{
			get
			{
				return this._EMail;
			}
			set
			{
				this._EMail = value;
			}
		}
		#endregion
	}

    public static class WatchType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
				new string[] { All, ContractRenewed, ContractExpired },
				new string[] { Messages.All, Messages.ContractRenewed, Messages.ContractExpired }) { ; }
        }
        public const string All = "A";
		public const string ContractRenewed = "R";
        public const string ContractExpired = "X";

        public class WatchAll : PX.Data.BQL.BqlString.Constant<WatchAll>
		{
            public WatchAll() : base(WatchType.All) { ;}
        }

        public class WatchContractRenewed : PX.Data.BQL.BqlString.Constant<WatchContractRenewed>
		{
			public WatchContractRenewed() : base(WatchType.ContractRenewed) { ;}
        }

        public class WatchContractExpired : PX.Data.BQL.BqlString.Constant<WatchContractExpired>
		{
			public WatchContractExpired() : base(WatchType.ContractExpired) { ;}
        }
    }
}
