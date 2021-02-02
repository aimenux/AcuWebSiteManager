using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public class PhotoLogEmailActivityService : EmailActivityService<PhotoLog>
    {
        private readonly FileInfo photoLogZip;

        public PhotoLogEmailActivityService(PXGraph graph, FileInfo photoLogZip)
            : base(graph, graph.Accessinfo.ContactID)
        {
            Entity = graph.Caches<PhotoLog>().Current as PhotoLog;
            this.photoLogZip = photoLogZip;
        }

        public PXGraph GetEmailActivityGraph()
        {
            var emailActivityGraph = CreateEmailActivityGraph();
            var cache = emailActivityGraph.Caches<CRSMEmail>();
            AttachPhotoLogZip(cache);
            return emailActivityGraph;
        }

        protected override string GetSubject()
        {
            return photoLogZip.Name.Replace(".zip", string.Empty);
        }

        private void AttachPhotoLogZip(PXCache cache)
        {
            var uploadFileMaintenance = PXGraph.CreateInstance<UploadFileMaintenance>();
            SetZipFileFullName(cache);
            uploadFileMaintenance.SaveFile(photoLogZip);
            PXNoteAttribute.SetFileNotes(cache, cache.Current, photoLogZip.UID?.SingleToArray());
        }

        private void SetZipFileFullName(PXCache cache)
        {
            var email = cache.Current as CRSMEmail;
            photoLogZip.FullName = $"{email?.NoteID}\\{photoLogZip.Name}";
        }
    }
}