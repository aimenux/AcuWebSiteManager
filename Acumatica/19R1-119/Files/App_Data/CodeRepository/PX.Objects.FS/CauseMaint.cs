using PX.Data;

namespace PX.Objects.FS
{
    public class CauseMaint : PXGraph<CauseMaint, FSCause>
    {
        [PXImport(typeof(FSCause))]
        public PXSelect<FSCause> CauseRecords;            
    }
}