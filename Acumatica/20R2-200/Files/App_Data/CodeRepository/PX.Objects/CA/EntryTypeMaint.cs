using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class EntryTypeMaint : PXGraph<EntryTypeMaint, CAEntryType>
	{
		public PXSelect<CAEntryType> EntryType;

		public PXSetup<CASetup> CASetup;

		public EntryTypeMaint()
		{
			CASetup setup = CASetup.Current;
		}
		#region Events
		protected virtual void CAEntryType_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			CAEntryType row = (CAEntryType)e.Row;
			if (string.IsNullOrEmpty(row.EntryTypeId))
			{
				e.Cancel = true;
			}
			else
			{
				CAEntryType eType = PXSelectReadonly<CAEntryType,
					Where<CAEntryType.entryTypeId, Equal<Optional<CAEntryType.entryTypeId>>>>.
					SelectWindowed(this, 0, 1, row.EntryTypeId);
				if (eType != null && eType != row)
				{
					cache.RaiseExceptionHandling<CAEntryType.entryTypeId>(e.Row, row.EntryTypeId, new PXException(Messages.DuplicatedKeyForRow));
					e.Cancel = true;
				}
			}
		}

		protected virtual void CAEntryType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CAEntryType row = (CAEntryType)e.Row;
			if (row != null)
			{
				if (row.Module == GL.BatchModule.CA)
				{
					PXUIFieldAttribute.SetEnabled<CAEntryType.referenceID>(this.EntryType.Cache, row, false);
					PXUIFieldAttribute.SetEnabled<CAEntryType.accountID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<CAEntryType.subID>(sender, row, true);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<CAEntryType.referenceID>(this.EntryType.Cache, row, true);
					PXUIFieldAttribute.SetEnabled<CAEntryType.accountID>(sender, row, false);
					PXUIFieldAttribute.SetEnabled<CAEntryType.subID>(sender, row, false);
				}

				PXUIFieldAttribute.SetEnabled<CAEntryType.useToReclassifyPayments>(sender, row, row.Module == GL.BatchModule.CA);

				bool isInserted = sender.GetStatus(e.Row) == PXEntryStatus.Inserted;
				PXUIFieldAttribute.SetEnabled<CAEntryType.entryTypeId>(sender, row, isInserted);
				bool usesCashAccount = false;
				GL.Account acct = null;
				CashAccount cashAcct = null;
				if (row.AccountID != null && row.SubID != null)
				{
					cashAcct = PXSelectReadonly<CashAccount, 
						Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
						And<CashAccount.subID, Equal<Required<CashAccount.subID>>>>>.Select(this, row.AccountID, row.SubID);
					acct = PXSelectReadonly<GL.Account, 
						Where<GL.Account.accountID, Equal<Required<GL.Account.accountID>>>>.Select(this, row.AccountID);
					usesCashAccount = (cashAcct != null);
				}
				PXUIFieldAttribute.SetEnabled<CAEntryType.branchID>(sender, row, row.UseToReclassifyPayments != true && usesCashAccount);
				PXUIFieldAttribute.SetEnabled<CAEntryType.cashAccountID>(sender, row, row.UseToReclassifyPayments == true);
				if (row.UseToReclassifyPayments == true && acct != null && acct.AccountID.HasValue)
				{
					if (!usesCashAccount)
					{
						sender.RaiseExceptionHandling<CAEntryType.accountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount, PXErrorLevel.Error));
					}
					else
					{
						PaymentMethodAccount pmAccount = PXSelectReadonly2<PaymentMethodAccount, 
							InnerJoin<PaymentMethod, 
								On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>,
								And<PaymentMethod.isActive, Equal<True>>>>,
							Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
							And<Where<PaymentMethodAccount.useForAP, Equal<True>,
								Or<PaymentMethodAccount.useForAR, Equal<True>>>>>>.Select(this, row.CashAccountID);

						if (pmAccount == null || pmAccount.CashAccountID.HasValue == false)
						{
							sender.RaiseExceptionHandling<CAEntryType.cashAccountID>(e.Row, cashAcct.CashAccountCD, new PXSetPropertyException(Messages.EntryTypeCashAccountIsNotConfiguredToUseWithAnyPaymentMethod, PXErrorLevel.Warning));
						}
					}
				}
				else
				{
					row.CashAccountID = null;
				}
			}
		}

		protected virtual void CAEntryType_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CAEntryType row = e.NewRow as CAEntryType;
			if (row.AccountID != null)
			{
				Account currentAcc = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.AccountID);
				if (currentAcc.IsCashAccount == true)
				{
					CashAccount cashAccount = PXSelect<CashAccount, 
						Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
						And<CashAccount.subID, Equal<Required<CashAccount.subID>>,
						And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>>>>>.Select(this, row.AccountID, row.SubID, row.BranchID);
					if (cashAccount == null)
					{
						string branchCD = (string)PXSelectorAttribute.GetField(sender, row, typeof(CAEntryType.branchID).Name, row.BranchID, typeof(Branch.branchCD).Name);
						sender.RaiseExceptionHandling<CAEntryType.branchID>(row, branchCD, new PXSetPropertyException(Messages.NoCashAccountForBranchAndSub, PXErrorLevel.Error));
						string subCD = (string)PXSelectorAttribute.GetField(sender, row, typeof(CAEntryType.subID).Name, row.SubID, typeof(Sub.subCD).Name);
						sender.RaiseExceptionHandling<CAEntryType.subID>(row, subCD, new PXSetPropertyException(Messages.NoCashAccountForBranchAndSub, PXErrorLevel.Error));
						e.Cancel = true;
						return;
					}
				}
			}
		}

		protected virtual void CAEntryType_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CAEntryType row = e.Row as CAEntryType;
			PXDefaultAttribute.SetPersistingCheck<CAEntryType.referenceID>(sender, e.Row, PXPersistingCheck.Nothing);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				CAAdj foundCAAdj = PXSelectReadonly<CAAdj,
					Where<CAAdj.entryTypeID, Equal<Required<CAAdj.entryTypeID>>>>.
					Select(this, row.EntryTypeId);

				if (foundCAAdj != null)
				{
					CAEntryType foundCAEntryType = PXSelectReadonly<CAEntryType,
						Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.
						Select(this, row.EntryTypeId);

					if (foundCAEntryType != null)
					{
						if (row.DrCr != foundCAEntryType.DrCr)
						{
							if (sender.RaiseExceptionHandling<CAEntryType.drCr>(e.Row, row.DrCr, new PXSetPropertyException(Messages.CantEditDisbReceipt, PXErrorLevel.RowError, typeof(CAEntryType.drCr).Name)))
							{
								throw new PXRowPersistingException(typeof(CAEntryType.drCr).Name, row.DrCr, Messages.CantEditDisbReceipt, typeof(CAEntryType.drCr).Name);
							}
						}
						if (row.Module != foundCAEntryType.Module)
						{
							if (sender.RaiseExceptionHandling<CAEntryType.module>(e.Row, row.Module, new PXSetPropertyException(Messages.CantEditModule, PXErrorLevel.RowError, typeof(CAEntryType.module).Name)))
							{
								throw new PXRowPersistingException(typeof(CAEntryType.module).Name, row.Module, Messages.CantEditModule, typeof(CAEntryType.module).Name);
							}
						}
					}
				}
			}
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				if (row.UseToReclassifyPayments == true && row.AccountID.HasValue)
				{
					CashAccount cashAcct = PXSelectReadonly<CashAccount, 
						Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
						And<CashAccount.subID, Equal<Required<CashAccount.subID>>>>>.Select(this, row.AccountID, row.SubID);
					GL.Account acct = PXSelectReadonly<Account, 
						Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.AccountID);
					if (cashAcct == null || !cashAcct.AccountID.HasValue)
					{
						if (sender.RaiseExceptionHandling<CAEntryType.accountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount, PXErrorLevel.Error)))
						{
							throw new PXRowPersistingException(typeof(CAEntryType.accountID).Name, acct.AccountCD, Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount);
						}
					}
				}
			}
		}

		private bool moduleChanged = false;

		protected virtual void CAEntryType_Module_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			moduleChanged = true;

			CAEntryType eType = (CAEntryType)e.Row;
			if (eType.Module == GL.BatchModule.CA)
			{
				eType.ReferenceID = null;
			}
			else
			{
				eType.AccountID = null;
				eType.SubID = null;
				eType.ReferenceID = null;
			}
			cache.SetDefaultExt<CAEntryType.useToReclassifyPayments>(e.Row);

		}

		protected virtual void CAEntryType_ReferenceID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			CAEntryType eType = (CAEntryType)e.Row;
			if (eType.Module == GL.BatchModule.CA || moduleChanged)
			{
				e.NewValue = null;
			}
		}

		protected virtual void CAEntryType_AccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CAEntryType row = e.Row as CAEntryType;
			if (row == null) return;
			Account glAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.AccountID);
			if (glAccount != null && glAccount.IsCashAccount == true)
			{
				CashAccount cashAcct = PXSelect<CashAccount,
					Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>>>.Select(this, glAccount.AccountID);
				cache.SetValueExt<CAEntryType.branchID>(row, cashAcct.BranchID);
				cache.SetValueExt<CAEntryType.subID>(row, cashAcct.SubID);
			}
			else
			{
				cache.SetValueExt<CAEntryType.branchID>(row, null);
			}
		}

		protected virtual void CAEntryType_AccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			cache.RaiseExceptionHandling<CAEntryType.accountID>(e.Row, null, null);
		}

		protected virtual void CAEntryType_CashAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CAEntryType row = e.Row as CAEntryType;
			if (row == null) return;
			if (row.CashAccountID != null)
            {
				cache.SetValuePending<CAEntryType.accountID>(e.Row, PXCache.NotSetValue);
                cache.SetDefaultExt<CAEntryType.accountID>(e.Row);
                cache.SetDefaultExt<CAEntryType.subID>(e.Row);
                cache.SetDefaultExt<CAEntryType.branchID>(e.Row);
            }
        }

		protected virtual void CAEntryType_AccountID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CAEntryType row = (CAEntryType)e.Row;
			if (row != null && row.CashAccountID != null)
			{
				CashAccount acct = PXSelect<CashAccount, 
					Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.CashAccountID);
				if (acct != null)
				{
					e.NewValue = acct.AccountID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CAEntryType_SubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CAEntryType row = (CAEntryType)e.Row;
			if (row != null && row.CashAccountID != null)
			{
				CashAccount acct = PXSelect<CashAccount, 
					Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.CashAccountID);
				if (acct != null)
				{
					e.NewValue = acct.SubID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CAEntryType_BranchID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CAEntryType row = (CAEntryType)e.Row;
			if (row == null) return;
			if (row.CashAccountID != null)
			{
				CashAccount acct = PXSelect<CashAccount, 
					Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.CashAccountID);
				if (acct != null)
				{
					e.NewValue = acct.BranchID;
					e.Cancel = true;
				}
			}
			else
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void CAEntryType_UseToReclassifyPayments_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				cache.SetValueExt<CAEntryType.cashAccountID>(e.Row, null);
			}
		}
		#endregion
	}
}
