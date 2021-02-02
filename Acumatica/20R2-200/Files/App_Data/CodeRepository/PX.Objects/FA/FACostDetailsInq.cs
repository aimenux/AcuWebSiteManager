using System;
using System.Collections;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;

namespace PX.Objects.FA
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class FACostDetailsInq : PXGraph<FACostDetailsInq>
	{
		public PXCancel<AccountFilter> Cancel;
		public PXFilter<AccountFilter> Filter;
		public PXSelect<Transact> Transactions;

		public FACostDetailsInq()
		{
			Transactions.Cache.AllowInsert = false;
			Transactions.Cache.AllowUpdate = false;
			Transactions.Cache.AllowDelete = false;
		}

		public virtual IEnumerable transactions(PXAdapter adapter)
		{
			AccountFilter filter = Filter.Current;
			if (filter == null) yield break;

			PXSelectBase<Transact> select = new PXSelectJoin<Transact,
				LeftJoin<FABook, On<Transact.bookID, Equal<FABook.bookID>>>,
				Where<Transact.assetID, Equal<Current<AccountFilter.assetID>>,
					And<Transact.released, Equal<True>>>>(this);

			if (!string.IsNullOrEmpty(filter.StartPeriodID))
			{
				select.WhereAnd<Where<Transact.finPeriodID, GreaterEqual<Current<AccountFilter.startPeriodID>>>>();
			}

			if (!string.IsNullOrEmpty(filter.EndPeriodID))
			{
				select.WhereAnd<Where<Transact.finPeriodID, LessEqual<Current<AccountFilter.endPeriodID>>>>();
			}

			if (filter.BookID != null)
			{
				select.WhereAnd<Where<Transact.bookID, Equal<Current<AccountFilter.bookID>>>>();
			}

			if (filter.AccountID != null)
			{
				select.WhereAnd<Where<Transact.debitAccountID, Equal<Current<AccountFilter.accountID>>, Or<Transact.creditAccountID, Equal<Current<AccountFilter.accountID>>>>>();
			}

			if (filter.SubID != null)
			{
				select.WhereAnd<Where<Transact.debitSubID, Equal<Current<AccountFilter.subID>>, Or<Transact.creditSubID, Equal<Current<AccountFilter.subID>>>>>();
			}

			foreach (Transact tran in select.Select())
			{
				if ((filter.AccountID == null || filter.AccountID == tran.CreditAccountID) && (filter.SubID == null || filter.SubID == tran.CreditSubID))
				{
					Transact tranCredit = (Transact) Transactions.Cache.CreateCopy(tran);
					tranCredit.AccountID = tran.CreditAccountID;
					tranCredit.SubID = tran.CreditSubID;
					tranCredit.CreditAmt = tran.TranAmt;
					tranCredit.DebitAmt = 0m;
					yield return tranCredit;
				}

				if ((filter.AccountID == null || filter.AccountID == tran.DebitAccountID) && (filter.SubID == null || filter.SubID == tran.DebitSubID))
				{
					Transact tranDebit = (Transact) Transactions.Cache.CreateCopy(tran);
					tranDebit.AccountID = tran.DebitAccountID;
					tranDebit.SubID = tran.DebitSubID;
					tranDebit.CreditAmt = 0m;
					tranDebit.DebitAmt = tran.TranAmt;
					yield return tranDebit;
				}
			}
		}
	}

	[Serializable]
	public partial class AccountFilter : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected int? _AssetID;
		[PXDBInt]
		[PXSelector(typeof(Search2<FixedAsset.assetID, InnerJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>>, Where<FixedAsset.recordType, Equal<FARecordType.assetType>>>),
			SubstituteKey = typeof(FixedAsset.assetCD),
			DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Asset ID")]
		[PXDefault]
		public virtual int? AssetID
		{
			get
			{
				return _AssetID;
			}
			set
			{
				_AssetID = value;
			}
		}
		#endregion
		#region StartPeriodID
		public abstract class startPeriodID : PX.Data.BQL.BqlString.Field<startPeriodID> { }
		protected string _StartPeriodID;
		[FABookPeriodExistingInGLSelector(
			dateType: typeof(AccessInfo.businessDate),
			assetSourceType: typeof(AccountFilter.assetID),
			bookSourceType: typeof(AccountFilter.bookID),
			isBookRequired: false)]
		[PXUIField(DisplayName = "Period From", Visibility = PXUIVisibility.Visible)]
		public virtual string StartPeriodID
		{
			get
			{
				return this._StartPeriodID;
			}
			set
			{
				this._StartPeriodID = value;
			}
		}
		#endregion
		#region EndPeriodID
		public abstract class endPeriodID : PX.Data.BQL.BqlString.Field<endPeriodID> { }
		protected string _EndPeriodID;
		[FABookPeriodExistingInGLSelector(
			dateType: typeof(AccessInfo.businessDate),
			assetSourceType: typeof(AccountFilter.assetID),
			bookSourceType: typeof(AccountFilter.bookID),
			isBookRequired: false)]
		[PXUIField(DisplayName = "Period To", Visibility = PXUIVisibility.Visible)]
		public virtual string EndPeriodID
		{
			get
			{
				return this._EndPeriodID;
			}
			set
			{
				this._EndPeriodID = value;
			}
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected int? _BookID;
		[PXDBInt]
		[PXSelector(typeof(FABook.bookID),
					SubstituteKey = typeof(FABook.bookCode),
					DescriptionField = typeof(FABook.description))]
		[PXDefault(typeof(Search<FABook.bookID, Where<FABook.updateGL, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Book")]
		public virtual int? BookID
		{
			get
			{
				return _BookID;
			}
			set
			{
				_BookID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(DisplayName = "Account", DescriptionField = typeof(Account.description))]
		public virtual Int32? AccountID
		{
			get
			{
				return _AccountID;
			}
			set
			{
				_AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(accountID), DisplayName = "Subaccount")]
		public virtual Int32? SubID
		{
			get
			{
				return _SubID;
			}
			set
			{
				_SubID = value;
			}
		}
		#endregion
	}

	[Serializable]
	public partial class Transact : FATran
	{
		#region DebitAmt
		public abstract class debitAmt : PX.Data.BQL.BqlDecimal.Field<debitAmt> { }
		protected decimal? _DebitAmt;
		[PXBaseCury]
		[PXUIField(DisplayName = "Debit")]
		[PXDefault]
		public virtual decimal? DebitAmt
		{
			get
			{
				return _DebitAmt;
			}
			set
			{
				_DebitAmt = value;
			}
		}
		#endregion
		#region CreditAmt
		public abstract class creditAmt : PX.Data.BQL.BqlDecimal.Field<creditAmt> { }
		protected decimal? _CreditAmt;
		[PXBaseCury]
		[PXUIField(DisplayName = "Credit")]
		[PXDefault]
		public virtual decimal? CreditAmt
		{
			get
			{
				return _CreditAmt;
			}
			set
			{
				_CreditAmt = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected int? _AccountID;
		[Account(DisplayName = "Account", DescriptionField = typeof(Account.description), IsDBField = false)]
		[PXDefault]
		public virtual int? AccountID
		{
			get
			{
				return _AccountID;
			}
			set
			{
				_AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected int? _SubID;
		[SubAccount(DisplayName = "Subaccount", IsDBField = false)]
		[PXDefault]
		public virtual int? SubID
		{
			get
			{
				return _SubID;
			}
			set
			{
				_SubID = value;
			}
		}
		#endregion
	}
}