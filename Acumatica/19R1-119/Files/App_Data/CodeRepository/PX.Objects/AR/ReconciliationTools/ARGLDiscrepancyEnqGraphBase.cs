using System;

using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.CM;

namespace ReconciliationTools
{
	#region Internal Types

	[Serializable]
	public partial class ARGLDiscrepancyEnqFilter : DiscrepancyEnqFilter
	{
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[Account(null, typeof(Search5<Account.accountID,
			InnerJoin<ARHistory, On<Account.accountID, Equal<ARHistory.accountID>>>,
			Where<Match<Current<AccessInfo.userName>>>,
			Aggregate<GroupBy<Account.accountID>>>),
			DisplayName = "Account", DescriptionField = typeof(Account.description))]
		public override int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region TotalXXAmount
		public new abstract class totalXXAmount : PX.Data.BQL.BqlDecimal.Field<totalXXAmount> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total AR Amount", Enabled = false)]
		public override decimal? TotalXXAmount
		{
			get;
			set;
		}
		#endregion
	}

	#endregion

	[TableAndChartDashboardType]
	public class ARGLDiscrepancyEnqGraphBase<TGraph, TEnqFilter, TEnqResult> : DiscrepancyEnqGraphBase<TGraph, TEnqFilter, TEnqResult>
		where TGraph : PXGraph
		where TEnqFilter : DiscrepancyEnqFilter, new()
		where TEnqResult : class, IBqlTable, IDiscrepancyEnqResult, new()
	{
		protected override decimal CalcGLTurnover(GLTran tran)
		{
			return (tran.DebitAmt ?? 0m) - (tran.CreditAmt ?? 0m);
		}
	}
}