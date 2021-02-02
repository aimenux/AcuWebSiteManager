using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CA
{
	public class CAClosingProcess : FinPeriodClosingProcessBase<CAClosingProcess, FinPeriod.cAClosed>
	{
		protected static BqlCommand OpenDocumentsQuery { get; } =
			PXSelectJoin<CATran,
				LeftJoin<CashAccount, 
					On<CATran.cashAccountID, Equal<CashAccount.cashAccountID>>,
				LeftJoin<CADeposit, 
					On<CATran.origRefNbr, Equal<CADeposit.refNbr>,
					And<CATran.origTranType, Equal<CADeposit.tranType>>>,
				LeftJoin<CAAdj, 
					On<CATran.origRefNbr, Equal<CAAdj.adjRefNbr>,
					And<CATran.origTranType, Equal<CAAdj.adjTranType>>>,
				InnerJoin<Branch, 
					On<CATran.branchID, Equal<Branch.branchID>>,
				LeftJoin<CAExpense, 
					On<CATran.tranID, Equal<CAExpense.cashTranID>>,
				LeftJoin<CAExpenseBranch,
					On<CAExpense.branchID, Equal<CAExpenseBranch.branchID>>,
				LeftJoin<CASplit, 
					On<CATran.origTranType, Equal<CASplit.adjTranType>,
					And<CATran.origRefNbr, Equal<CASplit.adjRefNbr>>>,
				LeftJoin<CASplitBranch,
					On<CASplit.branchID, Equal<CASplitBranch.branchID>>>>>>>>>>,
				Where2<
					Where2<WhereFinPeriodInRange<CATran.finPeriodID, Branch.organizationID>,
						Or2<WhereFinPeriodInRange<CATran.finPeriodID, CAExpenseBranch.organizationID>,
						Or<WhereFinPeriodInRange<CATran.finPeriodID, CASplitBranch.organizationID>>>>,
					And<CATran.origModule, Equal<BatchModule.moduleCA>,
					And<CATran.released, NotEqual<True>>>>,
				OrderBy<
					Asc<CATran.finPeriodID, // sorting, must be redundant relative to the grouping and precede it
					Asc<CATran.origModule, // grouping
					Asc<CATran.origTranType, // grouping
					Asc<CATran.origRefNbr>>>>>> // grouping
				.GetCommand();

		protected static BqlCommand OpenVouchersQuery { get; } = 
			PXSelectJoin<GLTranDoc, 
				InnerJoin<Branch,
					On<GLTranDoc.branchID, Equal<Branch.branchID>>,
				InnerJoin<CAAdj,
					On<GLTranDoc.refNbr, Equal<CAAdj.adjRefNbr>,
					And<GLTranDoc.tranType, Equal<CAAdj.adjTranType>>>,
				InnerJoin<CashAccount,
					On<IIf<Where<GLTranDoc.debitCashAccountID, IsNull>, GLTranDoc.creditCashAccountID, GLTranDoc.debitCashAccountID>, Equal<CashAccount.cashAccountID>>>>>,
				Where2<WhereFinPeriodInRange<GLTranDoc.finPeriodID, Branch.organizationID>,
					And<GLTranDoc.tranModule, Equal<BatchModule.moduleCA>,
						And<GLTranDoc.released, Equal<False>>>>,
				OrderBy<
					Asc<GLTranDoc.finPeriodID,
					Asc<GLTranDoc.tranType, 
					Asc<GLTranDoc.refNbr,
					Asc<GLTranDoc.lineNbr>>>>>>
			.GetCommand();

		protected override UnprocessedObjectsCheckingRule[] CheckingRules { get; } = new UnprocessedObjectsCheckingRule[]
		{
			new UnprocessedObjectsCheckingRule
			{
				ReportID = "CA656100",
				ErrorMessage = AP.Messages.PeriodHasUnreleasedDocs,
				CheckCommand = OpenDocumentsQuery
			},
			new UnprocessedObjectsCheckingRule
			{
				ReportID = "CA656150",
				ErrorMessage = AP.Messages.PeriodHasUnreleasedDocs,
				CheckCommand = OpenVouchersQuery
			}
		};
	}
}
