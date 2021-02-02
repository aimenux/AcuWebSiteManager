using System.Collections;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.SM.Services;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.DailyFieldReports.SM.GraphExtensions
{
    public class UploadFileInqExt : PXGraphExtension<UploadFileInq>
    {
        public PXAction<FilesFilter> DeleteFile;

        [InjectDependency]
        public IFilesDataProvider FilesDataProvider
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        [PXUIField(DisplayName = "Delete File", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable deleteFile(PXAdapter adapter)
        {
            var fileHasRelatedHistoryRevision =
                FilesDataProvider.DoesFileHaveRelatedHistoryRevision(Base.Files.Current.FileID);
            if (fileHasRelatedHistoryRevision)
            {
                Base.Files.Ask(DailyFieldReportMessages.TheFileIsReferredToTheDailyFieldReport, MessageButtons.OK);
                return adapter.Get();
            }
            return Base.DeleteFile.Press(adapter);
        }
    }
}
