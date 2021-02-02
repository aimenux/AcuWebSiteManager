using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.Common.CacheExtensions
{
    public sealed class UploadFileExt : PXCacheExtension<UploadFile>
    {
        [PXString]
        [PXUIField(DisplayName = "File Name")]
        public string FileName => FileInfo.GetShortName(Base.Name);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class fileName : IBqlField
        {
        }
    }
}