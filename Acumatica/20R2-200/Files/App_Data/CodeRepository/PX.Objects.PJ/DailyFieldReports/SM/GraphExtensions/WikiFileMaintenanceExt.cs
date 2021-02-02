using PX.Objects.PJ.DailyFieldReports.SM.Services;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.DailyFieldReports.SM.GraphExtensions
{
    public class WikiFileMaintenanceExt : PXGraphExtension<WikiFileMaintenance>
    {
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

        public virtual void _(Events.RowSelected<UploadFileWithIDSelector> args)
        {
            if (args.Row is UploadFileWithIDSelector uploadFileWithIdSelector)
            {
                var fileHasRelatedHistoryRevision =
                    FilesDataProvider.DoesFileHaveRelatedHistoryRevision(uploadFileWithIdSelector.FileID);
                Base.Delete.SetEnabled(!fileHasRelatedHistoryRevision);
                Base.UploadNewVersion.SetEnabled(!fileHasRelatedHistoryRevision);
                Base.Revisions.AllowDelete = !fileHasRelatedHistoryRevision;
            }
        }
    }
}
