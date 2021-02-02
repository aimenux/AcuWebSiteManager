using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
    public class PhotoLogMaint : PXGraph<PhotoLogMaint>
    {
        public PXFilter<PhotoLogFilter> Filter;

        public PXSetup<PhotoLogSetup> PhotoLogSetup;

        [PXFilterable(typeof(PhotoLog))]
        [PXViewName("Photo Logs")]
        public SelectFrom<PhotoLog>
            .Where<Brackets<PhotoLog.projectId.IsEqual<PhotoLogFilter.projectId.FromCurrent>
                    .Or<PhotoLogFilter.projectId.FromCurrent.IsNull>>
                .And<PhotoLog.projectTaskId.IsEqual<PhotoLogFilter.projectTaskId.FromCurrent>
                    .Or<PhotoLogFilter.projectTaskId.FromCurrent.IsNull>>
                .And<PhotoLog.date.IsGreaterEqual<PhotoLogFilter.dateFrom.FromCurrent>
                    .Or<PhotoLogFilter.dateFrom.FromCurrent.IsNull>>
                .And<PhotoLog.date.IsLessEqual<PhotoLogFilter.dateTo.FromCurrent>
                    .Or<PhotoLogFilter.dateTo.FromCurrent.IsNull>>>
            .OrderBy<Desc<PhotoLog.photoLogCd>>.View PhotoLogs;

        [PXHidden]
        public SelectFrom<Photo>
            .InnerJoin<PhotoLog>
                .On<Photo.photoLogId.IsEqual<PhotoLog.photoLogId>>
            .Where<PhotoLog.photoLogCd.IsEqual<PhotoLog.photoLogCd.FromCurrent>
                .And<Photo.isMainPhoto.IsEqual<True>>>.View MainPhoto;

        public PXInsert<PhotoLogFilter> InsertPhotoLog;
        public PXAction<PhotoLogFilter> EditPhotoLog;

        public PhotoLogMaint()
        {
	        PhotoLogSetup setup = PhotoLogSetup.Current;
        }

        [PXInsertButton]
        [PXUIField(DisplayName = "")]
        public virtual void insertPhotoLog()
        {
            var graph = CreateInstance<PhotoLogEntry>();
            graph.PhotoLog.Insert();
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

        [PXEditDetailButton]
        [PXUIField(DisplayName = "")]
        public virtual void editPhotoLog()
        {
            this.RedirectToEntity(PhotoLogs.Current, PXRedirectHelper.WindowMode.InlineWindow);
        }

        public virtual void _(Events.RowSelecting<Photo> args)
        {
            var photo = args.Row;
            if (photo != null)
            {
                photo.ImageUrl = photo.ImageUrl ?? string.Concat(PXUrl.SiteUrlWithPath(), Constants.FileUrl, photo.FileId);
            }
        }
    }
}