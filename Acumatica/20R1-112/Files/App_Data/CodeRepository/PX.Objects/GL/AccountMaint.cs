using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CA;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class AccountMaint : PXGraph<AccountMaint>
	{
		public PXSavePerRow<Account, Account.accountID> Save;
		public PXCancel<Account> Cancel;
		[PXImport(typeof(Account))]
		[PXFilterable]
		public PXSelect<Account,Where<Match<Current<AccessInfo.userName>>>, OrderBy<Asc<Account.accountCD>>> AccountRecords;

		public PXSelectReadonly<GLSetup> GLSetup;
		public GLSetup GLSETUP
		{
			get
			{
				GLSetup setup = GLSetup.Select();
				if (setup == null)
				{
					setup = new GLSetup();
					setup.COAOrder = (short)0;
				}
				return setup;
			}
		}
		public PXSetup<Company> Company;
		public CMSetupSelect cmsetup;

		protected bool? IsCOAOrderVisible = null;

		public AccountMaint()
		{
			if (string.IsNullOrEmpty(Company.Current.BaseCuryID))
			{
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
			if (IsCOAOrderVisible == null)
			{
				IsCOAOrderVisible = (GLSetup.Current.COAOrder > 3);
				PXUIFieldAttribute.SetVisible<Account.cOAOrder>(AccountRecords.Cache, null, (bool) IsCOAOrderVisible);
				PXUIFieldAttribute.SetEnabled<Account.cOAOrder>(AccountRecords.Cache, null, (bool)IsCOAOrderVisible);
			}

			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<Account.curyID>(AccountRecords.Cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetEnabled<Account.curyID>(AccountRecords.Cache, null, mcFeatureInstalled);

			PXUIFieldAttribute.SetVisible<Account.revalCuryRateTypeId>(AccountRecords.Cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetEnabled<Account.revalCuryRateTypeId>(AccountRecords.Cache, null, mcFeatureInstalled);
		}

		#region Repository methods

		public static Account FindAccountByCD(PXGraph graph, string accountCD)
		{
			Account account = PXSelect<Account,
				Where<Account.accountCD, Equal<Required<Account.accountCD>>>>.
				Select(graph, accountCD);

			return account;
		}

		protected bool PostedTransInOtherCuryExists(Account account, string curyID)
		{
			return PXSelectJoin<GLTran,
						InnerJoin<CurrencyInfo,
							On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>,
						InnerJoin<Ledger,
							On<GLTran.ledgerID, Equal<Ledger.ledgerID>>>>,
						Where<GLTran.accountID, Equal<Current<Account.accountID>>,
							And<CurrencyInfo.curyID, NotEqual<Required<CurrencyInfo.curyID>>,
							And<GLTran.posted, Equal<True>,
							And<Ledger.balanceType, NotEqual<LedgerBalanceType.report>>>>>>
						.SelectSingleBound(this, new object[] { account }, curyID)
						.Count > 0;
		}

		#endregion

		protected virtual void Account_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Account row = (Account)e.Row;
			if (row == null) return;

			Exception rateTypeWarning = (row != null && row.CuryID != null && row.CuryID != Company.Current.BaseCuryID && row.RevalCuryRateTypeId == null) ?
				new PXSetPropertyException(Messages.RevaluationRateTypeIsNotDefined, PXErrorLevel.Warning) :
				null;

			sender.RaiseExceptionHandling<Account.revalCuryRateTypeId>(row, row.RevalCuryRateTypeId, rateTypeWarning);

			PXUIFieldAttribute.SetEnabled<Account.curyID>(sender, row, row.IsCashAccount != true);
            PXUIFieldAttribute.SetEnabled<Account.postOption>(sender, row, row.IsCashAccount != true);

			PXUIFieldAttribute.SetEnabled<Account.controlAccountModule>(sender, row, row.IsCashAccount != true);
			PXUIFieldAttribute.SetEnabled<Account.allowManualEntry>(sender, row, row.IsCashAccount != true && row.ControlAccountModule != null);
		}

		protected virtual void Account_COAOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (IsCOAOrderVisible == false && e.Row != null && string.IsNullOrEmpty(((Account)e.Row).Type) == false)
			{
				e.NewValue = Convert.ToInt16(AccountType.COAOrderOptions[(int)GLSetup.Current.COAOrder].Substring(AccountType.Ordinal(((Account)e.Row).Type), 1));
				e.Cancel = true;
			}
		}

		protected virtual void Account_COAOrder_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (IsCOAOrderVisible == false && e.Row != null && string.IsNullOrEmpty(((Account)e.Row).Type) == false)
			{
				e.NewValue = Convert.ToInt16(AccountType.COAOrderOptions[(int)GLSetup.Current.COAOrder].Substring(AccountType.Ordinal(((Account)e.Row).Type), 1));
			}
		}

		protected virtual void Account_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (IsCOAOrderVisible == false)
			{
				sender.SetDefaultExt<Account.cOAOrder>(e.Row);
			}
		}

		protected virtual void Account_ControlAccountModule_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			var acc = (Account)e.Row;
			if (acc.ControlAccountModule == null)
				acc.AllowManualEntry = false;
		}

		protected virtual void Account_AllowManualEntry_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || (bool?)e.NewValue != true) return;
			var acc = (Account)e.Row;
			if (acc.ControlAccountModule == null)
				e.NewValue = false;
		}

		protected virtual void Account_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (e.NewRow == null) return;
			var account = (Account)e.NewRow;
			try
			{
				ValidateAccountGroupID(sender, account);
			}
			catch (PXSetPropertyException ex)
			{
				if (ex.ErrorLevel == PXErrorLevel.Error)
				{
					PM.PMAccountGroup item = (PM.PMAccountGroup)PXSelectorAttribute.Select<Account.accountGroupID>(sender, account);
					sender.RaiseExceptionHandling<Account.accountGroupID>(account, item.GroupCD, ex);
				}
				else
				{
					sender.RaiseExceptionHandling<Account.accountGroupID>(account, account.AccountGroupID, ex);
				}
			}
		}

		private void ValidateAccountGroupID(PXCache sender, Account account)
		{
			if (account.AccountGroupID == null) return;

			if (account.IsCashAccount == true)
			{
				throw new PXSetPropertyException(Messages.CashAccountIsNotForProjectPurposes, PXErrorLevel.Warning, account.AccountCD);
			}
			else
			{
				AccountAttribute.VerifyAccountIsNotControl(account);
			}
		}

		protected virtual void Account_Type_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = e.Row as Account;
			if (acct.Active != null && acct.Type != (string)e.NewValue && acct.AccountID != null)
			{
				bool hasHistory = GLUtility.IsAccountHistoryExist(this, acct.AccountID);
				if (hasHistory)
				{
					throw new PXSetPropertyException(Messages.AccountExistsType);
				}

				if (acct.AccountID == GLSetup.Current?.YtdNetIncAccountID && (string)e.NewValue != AccountType.Liability)
				{
					throw new PXSetPropertyException(Messages.AccountTypeCannotBeChangedGLYTD, acct.AccountCD);
				}

				if (acct.AccountGroupID != null)
                {
                    var group = (PM.PMAccountGroup)PXSelectorAttribute.Select<Account.accountGroupID>(cache, acct);

                    throw new PXSetPropertyException(Messages.AccountHasGroup, group.GroupCD);
                }
            }	

		}

		protected virtual void Account_CuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Account acct = e.Row as Account;

			if ((acct.CuryID != null) && (acct.CuryID != Company.Current.BaseCuryID))
			{
				acct.RevalCuryRateTypeId = cmsetup.Current.GLRateTypeReval;
			}
			else
			{
				acct.RevalCuryRateTypeId = null;
			}
		}

		protected virtual void Account_RevalCuryRateTypeId_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = e.Row as Account;

			if (((string)e.NewValue != null) && ((acct.CuryID == null) || (acct.CuryID == Company.Current.BaseCuryID)))
			{
			  throw new PXSetPropertyException(Messages.AccountRevalRateTypefailed);
			}

		}

		protected virtual void Account_GLConsolAccountCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account account = e.Row as Account;

			if (account == null)
				return;

			if (account.AccountID == GLSETUP.YtdNetIncAccountID
			    && e.NewValue != null)
			{
				throw new PXSetPropertyException(Messages.ConsolidationAccountCannotBeSpecified);
			}
		}

		protected virtual void Account_CuryID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = cache.Locate(e.Row) as Account;
			if (acct == null)
			{
				return;
			}
			string newCuryID = (string)e.NewValue;
			if (string.IsNullOrEmpty(acct.CuryID) && !string.IsNullOrEmpty(newCuryID))
			{
				if(PXSelect<BranchAcctMap,
				Where<BranchAcctMap.mapAccountID, Equal<Required<Account.accountID>>>>.Select(this, acct.AccountID).Any())
				{
					throw new PXSetPropertyException(Messages.CannotSetCurrencyToMappingAccount);
				}
				if (!PostedTransInOtherCuryExists(acct, newCuryID))
				{
					return;
				}
			}

			if (acct.CuryID != newCuryID)
			{
				if (newCuryID != null || acct.IsCashAccount == true)
				{
					bool hasHistory = GLUtility.IsAccountHistoryExist(this, acct.AccountID);
					if (hasHistory)
					{
						throw new PXSetPropertyException(Messages.CannotChangeAccountCurrencyTransactionsExist);
					}
				}
			}	

			if (acct.IsCashAccount == true && string.IsNullOrEmpty(newCuryID))
			{
				throw new PXSetPropertyException(Messages.CannotClearCurrencyInCashAccount);
			}
			
		}

		protected virtual void Account_AccountClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<Account.type>(e.Row);
		}

		protected virtual void Account_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (string.IsNullOrEmpty(((Account)e.Row).CuryID))
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void Account_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Account row = (Account)e.Row;

			try
			{
				ValidateAccountGroupID(sender, row);
			}
			catch (PXSetPropertyException ex)
			{
				if (ex.ErrorLevel == PXErrorLevel.Error)
				{
					PM.PMAccountGroup item = (PM.PMAccountGroup)PXSelectorAttribute.Select<Account.accountGroupID>(sender, row);
					sender.RaiseExceptionHandling<Account.accountGroupID>(row, item.GroupCD, ex);
					throw ex;
				}
			}

			if (!string.IsNullOrEmpty(row.CuryID))
			{
				CASetup casetup = PXSelect<CASetup>.Select(this);
				if (casetup != null && casetup.TransitAcctId != null && casetup.TransitAcctId == row.AccountID)
				{
					PXException exception = new PXException(CA.Messages.CashInTransitAccountCanNotBeDenominated);
					sender.RaiseExceptionHandling<Account.curyID>(row, row.CuryID, exception);
					throw exception;
				}

				string newCuryid;
				if (e.Operation == PXDBOperation.Update)
				{
					newCuryid = row.CuryID;
					byte[] timestamp = PXDatabase.SelectTimeStamp();

					PXDatabase.Update<GLHistory>(new PXDataFieldAssign("CuryID", newCuryid),
							new PXDataFieldRestrict("AccountID", ((Account)e.Row).AccountID),
							new PXDataFieldRestrict("CuryID", PXDbType.VarChar, 5, null, PXComp.ISNULL),
							new PXDataFieldRestrict("tstamp", PXDbType.Timestamp, 8, timestamp, PXComp.LE));
				}
			}
		}

		public PXAction<Account> viewRestrictionGroups;
		[PXUIField(DisplayName = Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (AccountRecords.Current != null)
			{
				GLAccessByAccount graph = CreateInstance<GLAccessByAccount>();
				graph.Account.Current = graph.Account.Search<Account.accountCD>(AccountRecords.Current.AccountCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		public PXAction<Account> accountByPeriodEnq;
		[PXUIField(DisplayName = Messages.ViewAccountByPeriod, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable AccountByPeriodEnq(PXAdapter adapter)
		{
			if (AccountRecords.Current != null)
			{
				AccountHistoryByYearEnq graph = CreateInstance<AccountHistoryByYearEnq>();
				graph.Filter.Current.AccountID = AccountRecords.Current.AccountID;
				throw new PXRedirectRequiredException(graph, false, Messages.ViewAccountByPeriod);
			}
			return adapter.Get();
		}

		public static Account GetAccountByCD(PXGraph graph, string accountCD)
		{
			Account account = FindAccountByCD(graph, accountCD);

			if (account == null)
			{
				throw new PXException(Messages.CannotFindAccount, accountCD);
			}

			return account;
		}

	}
}
