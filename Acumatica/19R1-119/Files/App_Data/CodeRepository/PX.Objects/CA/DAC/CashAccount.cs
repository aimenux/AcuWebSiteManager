using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CA
{
	[PXCacheName(Messages.CashAccount)]
	[Serializable]
	[PXPrimaryGraph(
		new Type[] { typeof(CashAccountMaint) },
		new Type[] { typeof(Select<CashAccount,
			Where<CashAccount.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>>)
		})]
	public partial class CashAccount : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Active
		{
			get;
			set;
		}
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDBIdentity]
		[PXUIField(Enabled = false)]
		[PXReferentialIntegrityCheck]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountCD
		public abstract class cashAccountCD : PX.Data.BQL.BqlString.Field<cashAccountCD> { }

		[CashAccountRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual string CashAccountCD
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[Account(Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[SubAccount(typeof(CashAccount.accountID), DisplayName = "Subaccount", DescriptionField = typeof(Sub.description),
			Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = true)]
		[PXSelector(typeof(CM.Currency.curyID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }

		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CM.CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Curr. Rate Type ")]
		public virtual string CuryRateTypeID
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region Reconcile
		public abstract class reconcile : PX.Data.BQL.BqlBool.Field<reconcile> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Requires Reconciliation")]
		public virtual bool? Reconcile
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Vendor(DescriptionField = typeof(Vendor.acctName), DisplayName = "Bank ID")]
		[PXUIField(DisplayName = "Bank ID")]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region ReconNumberingID
		public abstract class reconNumberingID : PX.Data.BQL.BqlString.Field<reconNumberingID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID),
					 DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Reconciliation Numbering Sequence", Required = false)]
		public virtual string ReconNumberingID
		{
			get;
			set;
		}
		#endregion
		#region ClearingAccount
		public abstract class clearingAccount : PX.Data.BQL.BqlBool.Field<clearingAccount> { }
		
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Clearing Account")]
		public virtual bool? ClearingAccount
		{
			get;
			set;
		}
		#endregion
		#region Signature
		public abstract class signature : PX.Data.BQL.BqlString.Field<signature> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Signature")]
		public virtual string Signature
		{
			get;
			set;
		}
		#endregion
		#region SignatureDescr
		public abstract class signatureDescr : PX.Data.BQL.BqlString.Field<signatureDescr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual string SignatureDescr
		{
			get;
			set;
		}
		#endregion
		#region StatementImportTypeName
		public abstract class statementImportTypeName : PX.Data.BQL.BqlString.Field<statementImportTypeName> { }

		[PXDBString(255)]
		[PXUIField(DisplayName = "Statement Import Service")]
		[PXProviderTypeSelector(typeof(IStatementReader))]
		public virtual string StatementImportTypeName
		{
			get;
			set;
		}
		#endregion
		#region RestrictVisibilityWithBranch
		public abstract class restrictVisibilityWithBranch : PX.Data.BQL.BqlBool.Field<restrictVisibilityWithBranch> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Restrict Visibility with Branch")]
		public virtual bool? RestrictVisibilityWithBranch
		{
			get;
			set;
		}
		#endregion
		#region PTInstanceAllowed
		public abstract class pTInstancesAllowed : PX.Data.BQL.BqlBool.Field<pTInstancesAllowed> { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cards Allowed", Visible = false, Enabled = false)]
		public virtual bool? PTInstancesAllowed
		{
			get;
			set;
		}
		#endregion
		#region AcctSettingsAllowed
		public abstract class acctSettingsAllowed : PX.Data.BQL.BqlBool.Field<acctSettingsAllowed> { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Account Settings Allowed", Visible = false, Enabled = false)]
		public virtual bool? AcctSettingsAllowed
		{
			get;
			set;
		}
		#endregion
		#region MatchToBatch
		public abstract class matchToBatch : PX.Data.BQL.BqlBool.Field<matchToBatch> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Match Bank Transactions to Batch Payments")]
		public virtual bool? MatchToBatch
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(CashAccount.cashAccountCD))]
		public virtual Guid? NoteID
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
