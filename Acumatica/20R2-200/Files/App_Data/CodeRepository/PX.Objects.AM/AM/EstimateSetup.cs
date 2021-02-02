using PX.Data;

namespace PX.Objects.AM
{
    public class EstimateSetup : PXGraph<EstimateSetup, AMEstimateSetup>
    {
        public PXSelect<AMEstimateSetup> AMEstimateSetupRecord;
    }
}