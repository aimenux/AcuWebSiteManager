using PX.Objects.PJ.Common.CacheExtensions;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions
{
    public sealed class NoteDocRequestForInformationExt : PXCacheExtension<NoteDocExt, NoteDoc>
    {
        [PXString]
        [PXUIField(DisplayName = "File Source", Enabled = false)]
        public string FileSource
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class fileSource : IBqlField
        {
        }
    }
}