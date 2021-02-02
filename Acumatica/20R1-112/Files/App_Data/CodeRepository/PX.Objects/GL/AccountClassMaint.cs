using System;
using System.Collections;

using PX.CS;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class AccountClassMaint : PXGraph<AccountClassMaint>, PXImportAttribute.IPXPrepareItems
	{
        public PXSavePerRow<AccountClass> Save;
		public PXCancel<AccountClass> Cancel;
		[PXImport(typeof(AccountClass))]
		public PXSelect<AccountClass> AccountClassRecords;

		public AccountClassMaint()
		{
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}
		public PXSetup<Branch> Company;

		protected virtual void AccountClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			AccountClass row = (AccountClass)e.Row;
			if (row == null) return;

			string usedIn;
			if (AccountClassInUse(this, row.AccountClassID, out usedIn))
			{
				throw new PXSetPropertyException(Messages.AccountClassIsUsedIn, row.AccountClassID, usedIn);
			}
		}

		protected virtual void AccountClass_AccountClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			AccountClass row = (AccountClass)e.Row;
			if (row == null) return;

			string usedIn;
			if (AccountClassInUse(this, row.AccountClassID, out usedIn))
			{
				throw new PXSetPropertyException(Messages.AccountClassIsUsedIn, row.AccountClassID, usedIn);
			}
		}

		public static bool AccountClassInUse(PXGraph graph, string accountClassID, out string usedIn)
		{
			usedIn = null;
			Account account = (Account)PXSelect<Account, Where<Account.accountClassID, Equal<Required<AccountClass.accountClassID>>>>.SelectSingleBound(graph, null, accountClassID);
			if (account != null)
			{
				usedIn = Messages.Account + account.AccountCD;
			}
			else
			{
				RMDataSource rmds = PXSelect<RMDataSource, Where<RMDataSourceGL.accountClassID, Equal<Required<AccountClass.accountClassID>>>>.SelectSingleBound(graph, null, accountClassID);
				if (rmds != null)
				{
					usedIn = Messages.RMDataSource;
				}
			}

			return usedIn != null;
		}

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			return !string.IsNullOrWhiteSpace(keys[nameof(AccountClass.AccountClassID)].ToString());
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		#endregion
	}
}