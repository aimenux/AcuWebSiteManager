using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
    public class JointPayeeDataProvider
    {
        public static IEnumerable<JointPayee> GetJointPayees(PXGraph graph, string referenceNumber,
            int? adjustmentLineNumber = 0)
        {
            var query = new PXSelectJoin<JointPayee,
                InnerJoin<APInvoice, On<JointPayee.billId, Equal<APInvoice.noteID>>>,
                Where<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>,
                    And<APInvoice.docType, Equal<APDocType.invoice>,
                    And<JointPayee.billLineNumber, Equal<Optional2<JointPayee.billLineNumber>>>>>>(graph);
            return query.Select(referenceNumber, adjustmentLineNumber).FirstTableItems;
        }

        public static JointPayee GetJointPayee(PXGraph graph, JointPayeePayment jointPayeePayment)
        {
            var query = new PXSelect<JointPayee,
                Where<JointPayee.jointPayeeId, Equal<Required<JointPayee.jointPayeeId>>>>(graph);
            return query.SelectSingle(jointPayeePayment.JointPayeeId);
        }

        public static IEnumerable<JointPayee> GetJointPayees(PXGraph graph,
            IEnumerable<JointPayeePayment> jointPayeePayments, int? lineNumber)
        {
            return GetJointPayees(graph, jointPayeePayments).Where(jp => jp.BillLineNumber == lineNumber);
        }

        public static IEnumerable<JointPayee> GetJointPayees(PXGraph graph,
            IEnumerable<JointPayeePayment> jointPayeePayments)
        {
            var jointPayeesIds = jointPayeePayments.Select(jpp => jpp.JointPayeeId).ToArray();
            return SelectFrom<JointPayee>
                .Where<JointPayee.jointPayeeId.IsIn<P.AsInt>>.View
                .Select(graph, jointPayeesIds).FirstTableItems;
        }
    }
}
