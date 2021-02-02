using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CA
{
	[Serializable]
	[PXProjection(typeof(Select5<CARecon,
		InnerJoin<CashAccount, 
			On<CashAccount.cashAccountID, Equal<CARecon.cashAccountID>>,
		InnerJoin<Branch,
			On<Branch.branchID, Equal<CashAccount.branchID>>,
		InnerJoin<OrganizationFinPeriod,
			On<OrganizationFinPeriod.endDate, Greater<CARecon.reconDate>,
			And<OrganizationFinPeriod.organizationID, Equal<Branch.organizationID>,
			And<CARecon.reconciled, Equal<boolTrue>, 
			And<CARecon.voided, Equal<boolFalse>>>>>>>>,
		Aggregate<GroupBy<CARecon.cashAccountID,
			Max<CARecon.reconDate,
			GroupBy<OrganizationFinPeriod.finPeriodID
		>>>>>))]
	[PXCacheName(Messages.CAReconByPeriod)]
	public partial class CAReconByPeriod : IBqlTable
	{
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[CashAccount(IsKey = true, BqlField = typeof(CARecon.cashAccountID))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region LastReconDate
		public abstract class lastReconDate : PX.Data.BQL.BqlDateTime.Field<lastReconDate> { }
		[PXDBDate(BqlField = typeof(CARecon.reconDate))]
		[PXUIField(DisplayName = "Last Reconciliation Date")]
		public virtual DateTime? LastReconDate
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[FinPeriodID(IsKey = true, BqlField = typeof(OrganizationFinPeriod.finPeriodID))]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
	}
}
