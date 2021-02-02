using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.DrawingLogs.PJ.CacheExtensions
{
	public sealed class UploadFileRevisionExt : PXCacheExtension<UploadFileRevision>
    {
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Current")]
        public bool? IsDrawingLogCurrentFile
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class isDrawingLogCurrentFile : BqlBool.Field<isDrawingLogCurrentFile>
        {
        }
    }
}