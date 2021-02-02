using PX.Data;

namespace PX.Objects.FS
{
    public class ResolutionMaint : PXGraph<ResolutionMaint, FSResolution>
    {
        [PXImport(typeof(FSResolution))]
        public PXSelect<FSResolution> ResolutionRecords;
    }
}