using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Employee Expenses")]
    public class DailyFieldReportEmployeeExpense : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportEmployeeExpenseId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBString(15, IsUnicode = true)]
        [EmployeeExpenseSelector]
        [PXParent(typeof(SelectFrom<EPExpenseClaimDetails>
            .Where<EPExpenseClaimDetails.claimDetailCD.IsEqual<employeeExpenseId>>))]
        [PXUIField(DisplayName = "Reference Number")]
        public virtual string EmployeeExpenseId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportEmployeeExpensesId : BqlGuid.Field<dailyFieldReportEmployeeExpensesId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class employeeExpenseId : BqlString.Field<employeeExpenseId>
        {
        }
    }
}
