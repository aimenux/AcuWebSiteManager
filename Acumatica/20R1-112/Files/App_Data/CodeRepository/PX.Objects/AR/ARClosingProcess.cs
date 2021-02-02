using PX.Data;
using PX.Common;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AR
{
	public class ARClosingProcess : FinPeriodClosingProcessBase<ARClosingProcess, FinPeriod.aRClosed>
	{
		public bool ExcludePendingProcessingDocs { get; set; }

		protected static BqlCommand OpenDocumentsQuery { get; } =
			PXSelectJoin<ARRegister,
				LeftJoin<ARTran,
					On<ARRegister.docType, Equal<ARTran.tranType>,
					And<ARRegister.refNbr, Equal<ARTran.refNbr>>>,
				LeftJoin<ARAdjust,
					On<ARAdjust.adjgDocType, Equal<ARRegister.docType>,
					And<ARAdjust.adjgRefNbr, Equal<ARRegister.refNbr>>>,
				LeftJoin<Branch,
					On<ARRegister.branchID, Equal<Branch.branchID>>,
				LeftJoin<TranBranch,
					On<ARTran.branchID, Equal<TranBranch.branchID>>,
				LeftJoin<AdjustingBranch,
					On<ARAdjust.adjgBranchID, Equal<AdjustingBranch.branchID>>,
				LeftJoin<AdjustedBranch,
					On<ARAdjust.adjdBranchID, Equal<AdjustedBranch.branchID>>,
				LeftJoin<ARInvoice,
					On<ARRegister.docType, Equal<ARInvoice.docType>,
					And<ARRegister.refNbr, Equal<ARInvoice.refNbr>>>,
				LeftJoin<ARPayment,
					On<ARRegister.docType, Equal<ARPayment.docType>,
					And<ARRegister.refNbr, Equal<ARPayment.refNbr>>>,
				LeftJoin<GLTranDoc,
					On<ARRegister.docType, Equal<GLTranDoc.tranType>,
					And<ARRegister.refNbr, Equal<GLTranDoc.refNbr>,
					And<GLTranDoc.tranModule, Equal<BatchModule.moduleAR>>>>,
				LeftJoin<BAccountR,
					On<ARRegister.customerID, Equal<BAccountR.bAccountID>>,
				LeftJoin<CashAccount,
					On<ARPayment.cashAccountID, Equal<CashAccount.cashAccountID>>,
				LeftJoin<CashAccountBranch,
					On<CashAccount.branchID, Equal<CashAccountBranch.branchID>>>>>>>>>>>>>>,
			Where<ARRegister.voided, NotEqual<True>,
				And<ARRegister.scheduled, NotEqual<True>,
				And<ARRegister.rejected, NotEqual<True>,
				And2<Where<ARRegister.released, NotEqual<True>, Or<ARAdjust.released, Equal<False>>>,
				And<Where<
					ARAdjust.adjgFinPeriodID, IsNull,
					And<ARRegister.released, NotEqual<True>,
					And2<Where2<WhereFinPeriodInRange<ARRegister.finPeriodID, Branch.organizationID>,
						Or2<WhereFinPeriodInRange<ARTran.finPeriodID, TranBranch.organizationID>,
						Or<WhereFinPeriodInRange<ARRegister.finPeriodID, CashAccountBranch.organizationID>>>>,
					Or<ARAdjust.released, Equal<False>, // explicit comparison with a False, a null value means no application, 
						And<Where2<WhereFinPeriodInRange<ARAdjust.adjgFinPeriodID, AdjustingBranch.organizationID>,
								Or<WhereFinPeriodInRange<ARAdjust.adjgFinPeriodID, AdjustedBranch.organizationID>>>>>>>>>>>>>,
			OrderBy<
				Asc<ARRegister.finPeriodID, // sorting, must be redundant relative to the grouping and precede it
				Asc<GLTranDoc.tranPeriodID, // sorting, must be redundant relative to the grouping and precede it
				Asc<ARRegister.docType, // grouping
				Asc<ARRegister.refNbr, // grouping
				Asc<GLTranDoc.lineNbr>>>>>>> // grouping
				.GetCommand();

		public override void ClosePeriod(FinPeriod finPeriod)
		{
			ExcludePendingProcessingDocs = true;
			base.ClosePeriod(finPeriod);
		}

		protected override void _(Events.RowSelected<FinPeriodClosingProcessParameters> e)
		{
			base._(e);
			ExcludePendingProcessingDocs = e.Row.Action == FinPeriodStatusProcess.FinPeriodStatusProcessParameters.action.Close ? true : false;
		}

		protected override UnprocessedObjectsCheckingRule[] CheckingRules
		{
			get
			{
				var query = ExcludePendingProcessingDocs
					? OpenDocumentsQuery.WhereAnd<Where<ARRegister.pendingProcessing, NotEqual<True>>>() : OpenDocumentsQuery;
				var rule = new UnprocessedObjectsCheckingRule() {
					ReportID = "AR656100",
					ErrorMessage = AP.Messages.PeriodHasUnreleasedDocs,
					CheckCommand = query
				};
				return new UnprocessedObjectsCheckingRule[] { rule };
			}
		}

		public override List<(string ReportID, IPXResultset ReportData)> GetReportsData(int? organizationID, string fromPeriodID, string toPeriodID) =>
			CheckingRules
				.Select(checker =>
				(
					ReportID: checker.ReportID,
					ReportData: GetResultset(
						checker.CheckCommand,
						organizationID,
						fromPeriodID,
						toPeriodID)
				))
				.Where(tuple => (tuple.ReportData?.GetRowCount() ?? 0) > 0)
				.ToList();
	}
}