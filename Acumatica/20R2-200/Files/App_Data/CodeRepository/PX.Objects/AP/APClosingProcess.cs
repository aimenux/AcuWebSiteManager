using PX.Data;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.AP
{
	public class APClosingProcess : FinPeriodClosingProcessBase<APClosingProcess, FinPeriod.aPClosed>
	{
		protected static BqlCommand OpenDocumentsQuery { get; } =
			PXSelectJoin<
				APRegister,
					LeftJoin<APTran,
						On<APRegister.docType, Equal<APTran.tranType>,
						And<APRegister.refNbr, Equal<APTran.refNbr>>>,
					LeftJoin<APAdjust,
						On<APAdjust.adjgDocType, Equal<APRegister.docType>,
						And<APAdjust.adjgRefNbr, Equal<APRegister.refNbr>>>,
					LeftJoin<Branch,
						On<APRegister.branchID, Equal<Branch.branchID>>,
					LeftJoin<TranBranch,
						On<APTran.branchID, Equal<TranBranch.branchID>>,
					LeftJoin<AdjustingBranch,
						On<APAdjust.adjgBranchID, Equal<AdjustingBranch.branchID>>,
					LeftJoin<AdjustedBranch,
						On<APAdjust.adjdBranchID, Equal<AdjustedBranch.branchID>>,
					LeftJoin<APInvoice,
						On<APRegister.docType, Equal<APInvoice.docType>,
						And<APRegister.refNbr, Equal<APInvoice.refNbr>>>,
					LeftJoin<APPayment,
						On<APRegister.docType, Equal<APPayment.docType>,
						And<APRegister.refNbr, Equal<APPayment.refNbr>>>,
					LeftJoin<GLTranDoc,
						On<APRegister.docType, Equal<GLTranDoc.tranType>,
						And<APRegister.refNbr, Equal<GLTranDoc.refNbr>,
						And<GLTranDoc.tranModule, Equal<BatchModule.moduleAP>>>>,
					LeftJoin<BAccountR,
						On<APRegister.vendorID, Equal<BAccountR.bAccountID>>,
					LeftJoin<CashAccount, 
						On<APPayment.cashAccountID, Equal<CashAccount.cashAccountID>>,
					LeftJoin<CashAccountBranch, 
						On<CashAccount.branchID, Equal<CashAccountBranch.branchID>>>>>>>>>>>>>>,
				Where2<Where<APRegister.voided, NotEqual<True>,
						And<APRegister.scheduled, NotEqual<True>,
						And<APRegister.rejected, NotEqual<True>,
						And<Where<
							APAdjust.adjgFinPeriodID, IsNull,
							And<APRegister.released, NotEqual<True>,
							And2<Where2<WhereFinPeriodInRange<APRegister.finPeriodID, Branch.organizationID>,
								Or2<WhereFinPeriodInRange<APTran.finPeriodID, TranBranch.organizationID>,
								Or<WhereFinPeriodInRange<APRegister.finPeriodID, CashAccountBranch.organizationID>>>>,
							Or<APAdjust.released, Equal<False>, // explicit comparison with a False, a null value means no application, 
								And<Where2<WhereFinPeriodInRange<APAdjust.adjgFinPeriodID, AdjustingBranch.organizationID>,
										Or2<WhereFinPeriodInRange<APAdjust.adjgFinPeriodID, AdjustedBranch.organizationID>,
										Or<Where2<WhereFinPeriodInRange<APRegister.finPeriodID, CashAccountBranch.organizationID>,
											And<APRegister.released, NotEqual<True>>>>>>>>>>>>>>>, // Unreleased documents and applications
					Or<Where<APRegister.voided, NotEqual<True>,
						And<APRegister.prebooked, Equal<True>,
						And<APRegister.released, NotEqual<True>,
						And<APRegister.rejected, NotEqual<True>,
						And<Where2<WhereFinPeriodInRange<APRegister.finPeriodID, Branch.organizationID>,
								Or2<WhereFinPeriodInRange<APTran.finPeriodID, TranBranch.organizationID>,
								Or<WhereFinPeriodInRange<APRegister.finPeriodID, CashAccountBranch.organizationID>>>>>>>>>>>, // Prebooked documents
			OrderBy<
				Asc<APRegister.finPeriodID, // sorting, must be redundant relative to the grouping and precede it
				Asc<GLTranDoc.tranPeriodID, // sorting, must be redundant relative to the grouping and precede it
				Asc<APRegister.docType, // grouping
				Asc<APRegister.refNbr, // grouping
				Asc<GLTranDoc.lineNbr>>>>>>> // grouping
				.GetCommand();

		protected override UnprocessedObjectsCheckingRule[] CheckingRules { get; } = new UnprocessedObjectsCheckingRule[]
		{
			new UnprocessedObjectsCheckingRule
			{
				ReportID = "AP656100",
				ErrorMessage = Messages.PeriodHasUnreleasedDocs,
				CheckCommand = OpenDocumentsQuery
			},
		};

	   
	}
}
