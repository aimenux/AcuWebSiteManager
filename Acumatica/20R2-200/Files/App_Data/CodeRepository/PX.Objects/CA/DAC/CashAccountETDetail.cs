using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(Messages.CashAccountETDetail)]
	public partial class CashAccountETDetail : IBqlTable
	{
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CashAccount.cashAccountID))]
		[PXUIField(DisplayName = "AccountID", Visible = false)]
        [PXParent(typeof(Select<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CashAccountETDetail.accountID>>>>))]
		public virtual int? AccountID
			{
			get;
			set;
		}
		#endregion
		#region EntryTypeID
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Entry Type ID")]
		[PXSelector(typeof(CAEntryType.entryTypeId))]
		public virtual string EntryTypeID
			{
			get;
			set;
		}
		#endregion
		#region OffsetAccountID
		public abstract class offsetAccountID : PX.Data.BQL.BqlInt.Field<offsetAccountID> { }

		[Account(DescriptionField = typeof(Account.description), DisplayName = "Offset Account Override", AvoidControlAccounts = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? OffsetAccountID
			{
			get;
			set;
		}
		#endregion
		#region OffsetSubID
		public abstract class offsetSubID : PX.Data.BQL.BqlInt.Field<offsetSubID> { }

		[SubAccount(typeof(CashAccountETDetail.offsetAccountID), DisplayName = "Offset Subaccount Override")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? OffsetSubID
		{
			get;
			set;
		}
		#endregion
        #region OffsetBranchID
		public abstract class offsetBranchID : PX.Data.BQL.BqlInt.Field<offsetBranchID> { }

		[Branch(DisplayName = "Offset Account Branch Override", PersistingCheck = PXPersistingCheck.Nothing, Enabled = false)]
		public virtual int? OffsetBranchID
        {
			get;
			set;
        }
        #endregion
        #region OffsetCashAccountID
		public abstract class offsetCashAccountID : PX.Data.BQL.BqlInt.Field<offsetCashAccountID> { }

        [PXRestrictor(typeof(Where<CashAccount.cashAccountID, NotEqual<Current<CashAccount.cashAccountID>>>), Messages.SetOffsetAccountInSameCurrency)]
        [PXRestrictor(typeof(Where<CashAccount.curyID, Equal<Current<CashAccount.curyID>>>), Messages.SetOffsetAccountInSameCurrency)]
        [CashAccountScalar(DisplayName = "Reclassification Account Override", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		[PXDBScalar(typeof(Search<CashAccount.cashAccountID,
				Where<CashAccount.accountID, Equal<CashAccountETDetail.offsetAccountID>,
				And<CashAccount.subID, Equal<CashAccountETDetail.offsetSubID>,
				And<CashAccount.branchID, Equal<CashAccountETDetail.offsetBranchID>>>>>))]
		public virtual int? OffsetCashAccountID
            {
			get;
			set;
        }
        #endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		public virtual string TaxZoneID
			{
			get;
			set;
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(TaxCalculationMode.taxSetting))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
			{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
			{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
			{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
