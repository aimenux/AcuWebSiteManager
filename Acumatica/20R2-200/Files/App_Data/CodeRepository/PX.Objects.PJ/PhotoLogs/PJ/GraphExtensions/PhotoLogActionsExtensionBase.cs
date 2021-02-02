using System.Collections;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Data;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.PM;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
    public abstract class PhotoLogActionsExtensionBase<TGraph, TPrimaryView> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
        where TPrimaryView : class, IBqlTable, new()
    {
        public PXAction<TPrimaryView> ActionsFolder;
        public PXAction<TPrimaryView> DownloadZip;
        public PXAction<TPrimaryView> EmailPhotoLog;

        protected PhotoLogZipServiceBase PhotoLogZipServiceBase;

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        public override void Initialize()
        {
            ActionsFolder.AddMenuAction(DownloadZip);
            ActionsFolder.AddMenuAction(EmailPhotoLog);
        }

        [PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder, MenuAutoOpen = true)]
        public void actionsFolder()
        {
        }

        [PXUIField(DisplayName = "Download Zip",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable downloadZip(PXAdapter adapter)
        {
            PhotoLogZipServiceBase.DownloadPhotoLogZip();
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Email",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable emailPhotoLog(PXAdapter adapter)
        {
            Base.Persist();
            var photoLogZip = PhotoLogZipServiceBase.GetPhotoLogZip();
            var photoLogEmailActivityService = new PhotoLogEmailActivityService(Base, photoLogZip);
            var graph = photoLogEmailActivityService.GetEmailActivityGraph();
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }
    }
}