using System;
using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;

namespace PX.Objects.GL
{
	public static class AccountRules
	{
		public static bool IsCreditBalance(string accountType)
		{
			if (IsGIRLSAccount(accountType)) return true;
			if (IsDEALAccount(accountType)) return false;
			throw new PXException(Messages.UnknownAccountTypeDetected, accountType);
		}

		/// <summary>
		/// Returns <c>true</c> if the provided Account Type has Credit balance,
		/// that is belongs to the Gains, Income, Revenues, Liabilities and Stock-holder equity group.
		/// In Acumatica, Gains, Revenues and Stock-holder equity account types are not implemented.
		/// </summary>
		/// <param name="accountType"><see cref="Account.Type"/></param>
		public static bool IsGIRLSAccount(string accountType)
		{
			return accountType == AccountType.Income || accountType == AccountType.Liability;
		}

		/// <summary>
		/// Returns <c>true</c> if the provided Account Type has Debit balance,
		/// that is belongs to the Dividends, Expenses, Assets and Losses group.
		/// In Acumatica, Dividends and Losses account types are not implemented.
		/// </summary>
		/// <param name="accountType"><see cref="Account.Type"/></param>
		public static bool IsDEALAccount(string accountType)
		{
			return accountType == AccountType.Expense || accountType == AccountType.Asset;
		}

		public static decimal CalcSaldo(string aAcctType, decimal aDebitAmt, decimal aCreditAmt)
		{
			return (IsCreditBalance(aAcctType) ? (aCreditAmt - aDebitAmt) : (aDebitAmt - aCreditAmt));
		}
	}

	public class GLUtility
	{
		public static bool IsAccountHistoryExist(PXGraph graph, int? accountID)
		{
			PXSelectBase select = new PXSelect<GLHistory, Where<GLHistory.accountID, Equal<Required<GLHistory.accountID>>>>(graph);
			Object result = select.View.SelectSingle(accountID);
			return (result != null);
		}

		public static bool IsLedgerHistoryExist(PXGraph graph, int? ledgerID)
		{
			PXSelectBase select = new PXSelect<GLHistory, Where<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>>>(graph);
			Object result = select.View.SelectSingle(ledgerID);
			return (result != null);
		}

		public static bool RelatedGLHistoryExists(PXGraph graph, int? ledgerID, int? organizationID)
		{
			GLHistory history = PXSelectReadonly2<GLHistory,
												InnerJoin<Branch,
													On<GLHistory.branchID, Equal<Branch.branchID>>>,
												Where<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>,
													And<Branch.organizationID, Equal<Required<Branch.organizationID>>>>>
												.SelectSingleBound(graph, null, ledgerID, organizationID);

			return history != null;
		}

		public static bool RelatedForOrganizationGLHistoryExists(PXGraph graph, int? organizationID)
		{
			GLHistory history = PXSelectReadonly2<GLHistory,
										InnerJoin<Branch,
											On<GLHistory.branchID, Equal<Branch.branchID>>>,
										Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
										.SelectSingleBound(graph, null, organizationID);

			return history != null;
		}

		public static GLHistory GetRelatedToBranchGLHistory(PXGraph graph, int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty()) return null;
			return PXSelectReadonly<GLHistory,
									Where<GLHistory.branchID, In<Required<GLHistory.branchID>>>>
									.SelectSingleBound(graph, null, branchIDs);
		}

		public static bool RelatedForBranchReleasedTransactionExists(PXGraph graph, int? branchID)
		{
			GLTran tran = PXSelectReadonly<GLTran,
											Where<GLTran.branchID, Equal<Required<GLTran.branchID>>,
													And<GLTran.released, Equal<True>>>>
											.SelectSingleBound(graph, null, branchID);

			return tran != null;
		}
	}
}
