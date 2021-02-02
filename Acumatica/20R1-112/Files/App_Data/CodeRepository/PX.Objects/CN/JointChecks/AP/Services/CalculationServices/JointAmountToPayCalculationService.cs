using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class JointAmountToPayCalculationService : CalculationServiceBase
    {
        public JointAmountToPayCalculationService(PXGraph graph)
            : base(graph)
        {
        }

        public decimal? GetTotalJointAmountToPay(APAdjust apAdjust)
        {
            return JointPayeePaymentDataProvider.GetJointPayeePayments(Graph, apAdjust)
                .Sum(x => x.JointAmountToPay);
        }
    }
}