using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class JointPayeeAmountsCalculationService : CalculationServiceBase
    {
        public JointPayeeAmountsCalculationService(PXGraph graph)
            : base(graph)
        {
        }

        public decimal GetTotalJointAmountOwed(string referenceNumber, int? lineNumber = 0)
        {
            return JointPayeeDataProvider.GetJointPayees(Graph, referenceNumber, lineNumber)
                .Where(jp => jp.BillLineNumber == lineNumber).Sum(x => x.JointAmountOwed.GetValueOrDefault());
        }

        public decimal? GetJointPreparedBalance(JointPayee jointPayee, string referenceNumber = null, int? lineNumber = null)
        {
	        return jointPayee.JointAmountOwed - GetJointAmountPaidWithNonReleased(jointPayee.SingleToListOrNull(), referenceNumber, lineNumber);
        }

        public decimal? GetJointAmountPaidWithNonReleased(List<JointPayee> jointPayees, string referenceNumber = null, int? lineNumber = null)
        {
	        var nonReleasedJointPayeePayments = GetUnreleasedJointPayeePayments(jointPayees, referenceNumber, lineNumber);

	        var sumOfJointAmountToPay = nonReleasedJointPayeePayments.Sum(jpp => jpp.JointAmountToPay);

			return jointPayees.Sum(jp=> jp.JointAmountPaid) + sumOfJointAmountToPay;
        }

        public IEnumerable<JointPayeePayment> GetUnreleasedJointPayeePayments(List<JointPayee> jointPayees,
	        bool skipInserted, string referenceNumber = null, int? lineNumber = null)
        {
	        var nonReleasedJointPayeePayments = JointPayeePaymentDataProvider
		        .GetNonReleasedJointPayeePayments(Graph, jointPayees, skipInserted);
	        return referenceNumber == null
		        ? nonReleasedJointPayeePayments
		        : nonReleasedJointPayeePayments.Where(jpp =>
			        jpp.InvoiceRefNbr == referenceNumber && jpp.BillLineNumber == lineNumber);
		}

		public IEnumerable<JointPayeePayment> GetUnreleasedJointPayeePayments(List<JointPayee> jointPayees,
            string referenceNumber = null, int? lineNumber = null)
		{
			return GetUnreleasedJointPayeePayments(jointPayees, false, referenceNumber, lineNumber);
		}
    }
}