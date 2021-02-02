using System.Collections;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class EmployeeExpenseSelectorAttribute : RelatedEntitiesBaseSelectorAttribute
    {
        public EmployeeExpenseSelectorAttribute()
            : base(typeof(EPExpenseClaimDetails.claimDetailCD),
                typeof(EPExpenseClaimDetails.claimDetailCD),
                typeof(EPExpenseClaimDetails.expenseDate),
                typeof(EPExpenseClaimDetails.expenseRefNbr),
                typeof(EPExpenseClaimDetails.employeeID),
                typeof(EPExpenseClaimDetails.branchID),
                typeof(EPExpenseClaimDetails.tranDesc),
                typeof(EPExpenseClaimDetails.curyTranAmtWithTaxes),
                typeof(EPExpenseClaimDetails.curyID),
                typeof(EPExpenseClaimDetails.status))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedEmployeeExpensesNumbers = _Graph.GetExtension<DailyFieldReportEntryEmployeeExpensesExtension>()
                .EmployeeExpenses.SelectMain().Select(ee => ee.EmployeeExpenseId);
            return GetRelatedEntities<EPExpenseClaimDetails, EPExpenseClaimDetails.contractID>()
                .Where(eс => eс.ClaimDetailCD.IsNotIn(linkedEmployeeExpensesNumbers));
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            RelatedEntitiesIds = GetRelatedEntities<EPExpenseClaimDetails>().Select(ec => ec.ClaimDetailCD);
            base.FieldVerifying(cache, args);
        }
    }
}
