using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.EP.CacheExtensions;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryEmployeeExpensesExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.EmployeeExpenses)]
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportEmployeeExpense>
            .LeftJoin<EPExpenseClaimDetails>
                .On<DailyFieldReportEmployeeExpense.employeeExpenseId.IsEqual<EPExpenseClaimDetails.claimDetailCD>>
            .Where<DailyFieldReportEmployeeExpense.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View EmployeeExpenses;

        [PXHidden]
        public SelectFrom<EPExpenseClaimDetails>.View ExpenseClaimDetails;

        public PXAction<DailyFieldReport> CreateExpenseReceipt;

        public PXAction<DailyFieldReport> ViewExpenseReceipt;

        public PXAction<DailyFieldReport> ViewExpenseClaim;

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.EmployeeExpense, ViewNames.EmployeeExpenses);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.expenseManagement>();
        }

        [PXUIField(DisplayName = "Last Modification Date")]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        public virtual void _(Events.CacheAttached<EPExpenseClaimDetails.lastModifiedDateTime> args)
        {
        }

        [PXUIField(DisplayName = "Project Task")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void _(Events.CacheAttached<EPExpenseClaimDetails.taskID> args)
        {
        }

        [PXUIField(DisplayName = "Claimed By")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void _(Events.CacheAttached<EPExpenseClaimDetails.employeeID> args)
        {
        }

        [PXButton]
        [PXUIField]
        public virtual void viewExpenseReceipt()
        {
            var expenseClaimDetailEntry = PXGraph.CreateInstance<ExpenseClaimDetailEntry>();
            expenseClaimDetailEntry.CurrentClaimDetails.Current = GetExpenseClaimDetail();
            PXRedirectHelper.TryRedirect(expenseClaimDetailEntry, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton]
        [PXUIField]
        public virtual void viewExpenseClaim()
        {
            var expenseClaim = GetExpenseClaim();
            if (expenseClaim == null)
            {
                return;
            }
            var expenseClaimEntry = PXGraph.CreateInstance<ExpenseClaimEntry>();
            expenseClaimEntry.ExpenseClaim.Current = expenseClaim;
            PXRedirectHelper.TryRedirect(expenseClaimEntry, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField(DisplayName = "Create New Expense Receipt")]
        public virtual void createExpenseReceipt()
        {
            Base.Actions.PressSave();
            var graph = PXGraph.CreateInstance<ExpenseClaimDetailEntry>();
            var expenseDetail = graph.ClaimDetails.Insert();
            var dailyFieldReport = Base.DailyFieldReport.Current;
            expenseDetail.ContractID = dailyFieldReport.ProjectId;
            expenseDetail.ExpenseDate = dailyFieldReport.Date;
            graph.ClaimDetails.Cache.SetValueExt<EpExpenseClaimDetailsExt.dailyFieldReportId>(expenseDetail,
                dailyFieldReport.DailyFieldReportId);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        public override void _(Events.RowSelected<DailyFieldReport> args)
        {
            base._(args);
            if (args.Row is DailyFieldReport dailyFieldReport)
            {
                var isActionAvailable = IsCreationActionAvailable(dailyFieldReport);
                CreateExpenseReceipt.SetEnabled(isActionAvailable);
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportEmployeeExpense))
            {
                RelationNumber = typeof(DailyFieldReportEmployeeExpense.employeeExpenseId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
           return new PXSelectExtension<DailyFieldReportRelation>(EmployeeExpenses);
        }

        private EPExpenseClaim GetExpenseClaim()
        {
            var expenseClaimDetail = GetExpenseClaimDetail();
            return SelectFrom<EPExpenseClaim>
                .Where<EPExpenseClaim.refNbr.IsEqual<P.AsString>>.View.Select(Base, expenseClaimDetail?.RefNbr);
        }

        private EPExpenseClaimDetails GetExpenseClaimDetail()
        {
            return SelectFrom<EPExpenseClaimDetails>
                .Where<EPExpenseClaimDetails.claimDetailCD.IsEqual<P.AsString>>.View
                .Select(Base, EmployeeExpenses.Current?.EmployeeExpenseId);
        }
    }
}
