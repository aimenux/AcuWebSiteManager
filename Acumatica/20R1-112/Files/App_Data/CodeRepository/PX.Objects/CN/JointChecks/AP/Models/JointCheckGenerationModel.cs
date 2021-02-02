using System.Collections.Generic;
using System.Linq;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Models
{
    public class JointCheckGenerationModel
    {
        private JointCheckGenerationModel()
        {
        }

        public decimal? JointAmountToPay => InvoiceJointPayeePayments.Sum(jpp => jpp.JointAmountToPay);

        public IEnumerable<int?> BillLineNumbers
        {
            get;
            set;
        }

        public APPayment CreatedPayment
        {
            get;
            set;
        }

        public IEnumerable<JointPayeePayment> InvoiceJointPayeePayments
        {
            get;
            set;
        }

        public static JointCheckGenerationModel Create(
            IEnumerable<JointPayeePayment> jointPayeePayments, IEnumerable<JointPayee> jointPayees)
        {
            var invoiceJointPayeePayments = jointPayees.Select(jp =>
                jointPayeePayments.Single(jjp => jjp.JointPayeeId == jp.JointPayeeId)).ToList();
            return new JointCheckGenerationModel
            {
                InvoiceJointPayeePayments = invoiceJointPayeePayments,
                BillLineNumbers = invoiceJointPayeePayments.Select(jpp => jpp.BillLineNumber)
            };
        }
    }
}