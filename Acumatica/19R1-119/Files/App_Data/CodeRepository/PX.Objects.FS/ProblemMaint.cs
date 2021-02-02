using PX.Data;

namespace PX.Objects.FS
{
    public class ProblemMaint : PXGraph<ProblemMaint, FSProblem>
    {
        [PXImport(typeof(FSProblem))]
        public PXSelect<FSProblem> ProblemRecords;           
    }
}