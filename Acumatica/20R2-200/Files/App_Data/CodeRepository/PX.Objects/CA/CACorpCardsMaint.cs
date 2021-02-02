using PX.Data;
using PX.Objects.EP.DAC;

namespace PX.Objects.CA
{
	public class CACorpCardsMaint : PXGraph<CACorpCardsMaint, CACorpCard>
	{
		#region Repo

		public static CashAccount GetCardCashAccount(PXGraph graph, int? corpCardID)
		{
			return PXSelectJoin<CashAccount,
						InnerJoin<CACorpCard,
							On<CashAccount.cashAccountID, Equal<CACorpCard.cashAccountID>>>,
						Where<CACorpCard.corpCardID, Equal<Required<CACorpCard.corpCardID>>>>
						.Select(graph, corpCardID);
		}

		#endregion

		public PXSelect<CACorpCard> CreditCards;
		public PXSelect<EPEmployeeCorpCardLink, Where<EPEmployeeCorpCardLink.corpCardID, Equal<Current<CACorpCard.corpCardID>>>> EmployeeLinks;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(CACorpCard.corpCardID))]
		protected virtual void _(Events.CacheAttached<EPEmployeeCorpCardLink.corpCardID> e)
		{
		}
	}
}