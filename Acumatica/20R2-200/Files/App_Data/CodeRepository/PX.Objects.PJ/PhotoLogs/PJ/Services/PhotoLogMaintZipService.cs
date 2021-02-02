using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.SM;
using Constants = PX.Objects.PJ.Common.Descriptor.Constants;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public class PhotoLogMaintZipService : PhotoLogZipServiceBase
    {
        private readonly List<PhotoLog> selectedPhotoLogs;

        public PhotoLogMaintZipService(PXGraph graph)
            : base(graph)
        {
            selectedPhotoLogs = Cache.Cached.Cast<PhotoLog>().Where(pl => pl.Selected == true).ToList();
        }

        protected override string NoAttachedFilesInDocumentMessage => PhotoLogMessages.NoPhotosInSelectedPhotoLogs;

        public override FileInfo GetPhotoLogZip()
        {
            var fileIds = GetDocumentFileIds(selectedPhotoLogs);
            return GetZipFile(fileIds, ZipFileName);
        }

        public override void DownloadPhotoLogZip()
        {
            var fileIds = GetDocumentFileIds(selectedPhotoLogs);
            PXLongOperation.StartOperation(Graph, () =>
            {
                DownloadZipFile(fileIds);
                Graph.Persist();
            });
        }

        protected override string GetPhotoLogZipFileName(PhotoLog photoLog)
        {
            if (selectedPhotoLogs.HasAtLeastTwoItems())
            {
                var date = Graph.Accessinfo.BusinessDate.GetValueOrDefault()
                    .ToString(Constants.FilesDateFormat);
                return $"Photo logs {date}.zip";
            }
            return base.GetPhotoLogZipFileName(selectedPhotoLogs.SingleOrDefault());
        }
    }
}