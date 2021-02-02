using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Data;
using PX.SM;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public class PhotoLogEntryZipService : PhotoLogZipServiceBase
    {
        public PhotoLogEntryZipService(PXGraph graph)
            : base(graph)
        {
        }

        protected override string NoAttachedFilesInDocumentMessage => PhotoLogMessages.NoPhotosAddedToPhotoLog;

        public override FileInfo GetPhotoLogZip()
        {
            return GetZipFile(CurrentPhotoLog);
        }

        public override void DownloadPhotoLogZip()
        {
            DownloadZipFile(CurrentPhotoLog);
        }
    }
}