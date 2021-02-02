using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.GL
{
	public class GLHistoryPrimaryGraphAttribute : PXPrimaryGraphAttribute
	{
		public GLHistoryPrimaryGraphAttribute()
			: base(typeof(AccountByPeriodEnq))
		{
			Filter = typeof(AccountByPeriodFilter);
		}

		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			if (row is GLHistoryByPeriod history && history.LedgerID != null)
			{
				Ledger ledger = PXSelect<Ledger, 
					Where<Ledger.ledgerID, Equal<Current<GLHistoryByPeriod.ledgerID>>>>
					.SelectSingleBound(cache.Graph, new object[] {history});					
				if (ledger?.BalanceType == LedgerBalanceType.Budget)
				{
					Filter = typeof(BudgetFilter);
					return typeof(GLBudgetEntry);
				}
			}			
			return base.GetGraphType(cache, ref row, checkRights, preferedType);
		}
	}
}
