using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	/// <summary>
	/// A CA transaction detail helper class. Contains common logic for classes which implement <see cref="ICATranDetail"/>.
	/// </summary>
	internal static class CATranDetailHelper
	{
		/// <summary>
		/// Handles the CA transaction detail <see cref="ICATranDetail.CashAccountId"/> field verifying event.
		/// </summary>
		/// <param name="tranDetailsCache">The transaction details cache.</param>
		/// <param name="e">Field verifying event arguments.</param>
		/// <param name="curCashAccountID">Identifier for the current cash account.</param>
		public static void OnCashAccountIdFieldVerifyingEvent(PXCache tranDetailsCache, PXFieldVerifyingEventArgs e, int? curCashAccountID)
		{
			ICATranDetail tranDetail = e.Row as ICATranDetail;

			if (tranDetail == null)
				return;

			CashAccount offsetCashAccount = PXSelect<CashAccount,
											   Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
                                            .Select(tranDetailsCache.Graph, e.NewValue);

			if (offsetCashAccount != null && offsetCashAccount.AccountID == curCashAccountID)
			{
				e.NewValue = offsetCashAccount.CashAccountCD;
				throw new PXSetPropertyException(Messages.OffsetAccountMayNotBeTheSameAsCurrentAccount, PXErrorLevel.Error);
			}
		}

		/// <summary>
		/// Handles the CA transaction detail <see cref="ICATranDetail.AccountID"/> field updated event.
		/// </summary>
		/// <param name="tranDetailsCache">The transaction details cache.</param>
		/// <param name="e">Field updated event arguments.</param>
		public static void OnAccountIdFieldUpdatedEvent(PXCache tranDetailsCache, PXFieldUpdatedEventArgs e)
		{
			ICATranDetail tranDetail = e.Row as ICATranDetail;

			if (tranDetail == null)
				return;

			Account glAccount = PXSelect<Account,
								   Where<Account.accountID, Equal<Required<Account.accountID>>>>
                                .Select(tranDetailsCache.Graph, tranDetail.AccountID);

			if (glAccount?.IsCashAccount != true)
				return;

			CashAccount cashAccount = GetCashAccount(tranDetailsCache.Graph, glAccount.AccountID, tranDetail.SubID, tranDetail.BranchID, doSearchWithSubsetsOfArgs: true);

			if (cashAccount != null)
			{
				tranDetailsCache.SetValueExt(tranDetail, nameof(ICATranDetail.BranchID), cashAccount.BranchID);
				tranDetailsCache.SetValueExt(tranDetail, nameof(ICATranDetail.SubID), cashAccount.SubID);
			}
		}

        /// <summary>
		/// Handles the CA transaction detail <see cref="ICATranDetail.CashAccountID"/> field updated event.
		/// </summary>
		/// <param name="tranDetailsCache">The transaction details cache.</param>
		/// <param name="e">Field updated event arguments.</param>
        public static void OnCashAccountIdFieldUpdatedEvent(PXCache tranDetailsCache, PXFieldUpdatedEventArgs e)
        {
            ICATranDetail tranDetail = e.Row as ICATranDetail;

            if (tranDetail == null || tranDetail.CashAccountID == (int?)e.OldValue)
                return;

            CashAccount offsetCashAcct = null;

            if (tranDetail.CashAccountID != null)
            {
                offsetCashAcct = PXSelectReadonly<CashAccount,
                                            Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
                                .Select(tranDetailsCache.Graph, tranDetail.CashAccountID);
            }

            tranDetailsCache.SetValue(tranDetail, nameof(ICATranDetail.AccountID), offsetCashAcct?.AccountID);
            tranDetailsCache.SetValue(tranDetail, nameof(ICATranDetail.SubID), offsetCashAcct?.SubID);
            tranDetailsCache.SetValue(tranDetail, nameof(ICATranDetail.BranchID), offsetCashAcct?.BranchID);
        }

        /// <summary>
        /// Handles the CA transaction detail <see cref="ICATranDetail.CashAccountID"/> field defaulting event.
        /// </summary>
        /// <param name="tranDetailsCache">The transaction details cache.</param>
        /// <param name="e">Field defaulting event arguments.</param>
        public static void OnCashAccountIdFieldDefaultingEvent(PXCache tranDetailsCache, PXFieldDefaultingEventArgs e)
        {
            ICATranDetail tranDetail = e.Row as ICATranDetail;

            if (tranDetail == null || tranDetail.CashAccountID != null)
                return;

            CashAccount cashAccount = PXSelect<CashAccount,
                                         Where<CashAccount.accountID, Equal<Required<CABankTranDetail.accountID>>,
                                           And<CashAccount.subID, Equal<Required<CABankTranDetail.subID>>,
                                           And<CashAccount.branchID, Equal<Required<CABankTranDetail.branchID>>,
											And<CashAccount.active, Equal<True>>>>>>
                                     .Select(tranDetailsCache.Graph, tranDetail.AccountID, tranDetail.SubID, tranDetail.BranchID);

            if (cashAccount != null)
            {
                e.NewValue = cashAccount.CashAccountCD;
            }
        }

        /// <summary>
        /// Handles the CA transaction detail row updating event.
        /// </summary>
        /// <param name="tranDetailsCache">The transaction details cache.</param>
        /// <param name="e">Row updating event arguments.</param>
        public static void OnCATranDetailRowUpdatingEvent(PXCache tranDetailsCache, PXRowUpdatingEventArgs e)
		{
			ICATranDetail oldTranDetail = e.Row as ICATranDetail;
			ICATranDetail newTranDetail = e.NewRow as ICATranDetail;

			if (newTranDetail == null || tranDetailsCache == null)
				return;

			UpdateNewTranDetailCuryTranAmtOrCuryUnitPrice(tranDetailsCache, oldTranDetail, newTranDetail);

			if (newTranDetail.AccountID == null)
				return;

			Account currentAcc = PXSelect<Account,
									Where<Account.accountID, Equal<Required<Account.accountID>>>>.
								 Select(tranDetailsCache.Graph, newTranDetail.AccountID);

			if (currentAcc?.IsCashAccount != true)
				return;

			CashAccount cashAccount = GetCashAccount(tranDetailsCache.Graph, 
														newTranDetail.AccountID, 
														newTranDetail.SubID,
														newTranDetail.BranchID, 
														doSearchWithSubsetsOfArgs: false);

			if (cashAccount == null)
			{
				PXSetPropertyException exception = new PXSetPropertyException(Messages.NoCashAccountForBranchAndSub, PXErrorLevel.Error);
				string branchCD = (string)PXSelectorAttribute.GetField(tranDetailsCache, 
																		newTranDetail, 
																		nameof(ICATranDetail.BranchID),
																		newTranDetail.BranchID, 
																		typeof(Branch.branchCD).Name);
				string subCD = (string)PXSelectorAttribute.GetField(tranDetailsCache, 
																	newTranDetail,
																	nameof(ICATranDetail.SubID),
																	newTranDetail.SubID, 
																	typeof(Sub.subCD).Name);

				tranDetailsCache.RaiseExceptionHandling(nameof(ICATranDetail.BranchID), newTranDetail, branchCD, exception);
				tranDetailsCache.RaiseExceptionHandling(nameof(ICATranDetail.SubID), newTranDetail, subCD, exception);
				e.Cancel = true;
			}
		}

        /// <summary>
        /// Gets the new CA transaction detail <see cref="ICATranDetail"/> with <see cref="ICATranDetail.AccountID"/>, <see cref="ICATranDetail.SubID"/>
        /// and <see cref="ICATranDetail.BranchID"/> fields filled with default values.
        /// </summary>
        /// <typeparam name="TTransactionDetail">Type of transaction detail</typeparam>
        /// <param name="graph">The sender graph.</param>
        /// <param name="cashAccountID">The cash account ID</param>
        /// <param name="entryTypeID">The entry type ID</param>
        /// <returns/>
        public static TTransactionDetail CreateCATransactionDetailWithDefaultAccountValues<TTransactionDetail>(PXGraph graph, int? cashAccountID, string entryTypeID)
        where TTransactionDetail : ICATranDetail, new()
        {
            CashAccountETDetail acctSettings = PXSelect<CashAccountETDetail,
                                                  Where<CashAccountETDetail.accountID, Equal<Required<CashAccountETDetail.accountID>>,
                                                    And<CashAccountETDetail.entryTypeID, Equal<Required<CashAccountETDetail.entryTypeID>>>>>
                                               .Select(graph, cashAccountID, entryTypeID);

            bool allAccountValuesSet = acctSettings?.OffsetAccountID != null &&
                                            acctSettings.OffsetSubID != null &&
                                         acctSettings.OffsetBranchID != null;

            TTransactionDetail result = new TTransactionDetail
            {
                AccountID = acctSettings?.OffsetAccountID,
                SubID = acctSettings?.OffsetSubID,
                BranchID = acctSettings?.OffsetBranchID
            };

            if (allAccountValuesSet)
            {
                return result;
            }

            CAEntryType entryType = PXSelect<CAEntryType,
                                       Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>
                                   .Select(graph, entryTypeID);

            if (entryType != null)
            {
                result.AccountID = result.AccountID ?? entryType.AccountID;
                result.SubID = result.SubID ?? entryType.SubID;
                result.BranchID = result.BranchID ?? entryType.BranchID;
            }

            return result;
        }

        /// <summary>
        /// Updates the new transaction detail CuryTranAmt or CuryUnitPrice.
        /// </summary>
        /// <param name="tranDetailsCache">The transaction details cache.</param>
        /// <param name="oldTranDetail">The old transaction detail.</param>
        /// <param name="newTranDetail">The new transaction detail.</param>
        public static void UpdateNewTranDetailCuryTranAmtOrCuryUnitPrice(PXCache tranDetailsCache, ICATranDetail oldTranDetail, ICATranDetail newTranDetail)
		{
			if (tranDetailsCache == null || newTranDetail == null)
				return;

			bool priceChanged = (oldTranDetail?.CuryUnitPrice ?? 0m) != (newTranDetail.CuryUnitPrice ?? 0m);
			bool amtChanged = (oldTranDetail?.CuryTranAmt ?? 0m) != (newTranDetail.CuryTranAmt ?? 0m);
			bool qtyChanged = (oldTranDetail?.Qty ?? 0m) != (newTranDetail.Qty ?? 0m);

			if (amtChanged)
			{
				if (newTranDetail.Qty != null && newTranDetail.Qty != 0m)
				{
					decimal curyUnitPriceToRound = (newTranDetail.CuryTranAmt ?? 0m) / newTranDetail.Qty.Value;
					newTranDetail.CuryUnitPrice = PXCurrencyAttribute.RoundCury(tranDetailsCache, newTranDetail, curyUnitPriceToRound);
				}
				else
				{
					newTranDetail.CuryUnitPrice = newTranDetail.CuryTranAmt;
					newTranDetail.Qty = 1.0m;
				}
			}
			else if (priceChanged || qtyChanged)
			{
				newTranDetail.CuryTranAmt = newTranDetail.Qty * newTranDetail.CuryUnitPrice;
			}
		}

		/// <summary>
		/// Gets cash account.
		/// </summary>
		/// <param name="glAccountID">Identifier for the GL account.</param>
		/// <param name="subID">Identifier for the sub account.</param>
		/// <param name="branchID">Identifier for the branch.</param>
		/// <param name="doSearchWithSubsetsOfArgs">True to do search with subsets of search arguments.</param>
		/// <returns/>
		public static CashAccount GetCashAccount(PXGraph graph, int? glAccountID, int? subID, int? branchID, bool doSearchWithSubsetsOfArgs)
		{
			CashAccount cashAccount = null;

			if (subID != null && branchID != null)
			{
				cashAccount = PXSelect<CashAccount,
								 Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
								   And<CashAccount.subID, Equal<Required<CashAccount.subID>>,
								   And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>,
									And<CashAccount.active, Equal<True>>>>>>
                              .Select(graph, glAccountID, subID, branchID);
			}

			if (cashAccount != null || !doSearchWithSubsetsOfArgs)
			{
				return cashAccount;
			}

			if (branchID != null)
			{
				cashAccount = PXSelect<CashAccount,
								 Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
								   And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>,
									And<CashAccount.active, Equal<True>>>>>
                              .Select(graph, glAccountID, branchID);
			}
			else if (subID != null)
			{
				cashAccount = PXSelect<CashAccount,
								 Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
								   And<CashAccount.subID, Equal<Required<CashAccount.subID>>,
									And<CashAccount.active, Equal<True>>>>>
                              .Select(graph, glAccountID, subID);
			}

			if (cashAccount == null)
			{
				cashAccount = PXSelect<CashAccount,
								 Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
									And<CashAccount.active, Equal<True>>>>
                              .Select(graph, glAccountID);
			}

			return cashAccount;
		}
	}
}
