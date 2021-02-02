using PX.Data;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class CalculationServiceBase
    {
        protected readonly PXGraph Graph;

        public CalculationServiceBase(PXGraph graph)
        {
            Graph = graph;
        }
    }
}
