using System.Collections.Generic;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public class PhotoLogDataProvider : IPhotoLogDataProvider
    {
        private readonly PXGraph graph;

        public PhotoLogDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public PhotoLog GetPhotoLog(int? photoLogId)
        {
            return SelectFrom<PhotoLog>
                .Where<PhotoLog.photoLogId.IsEqual<P.AsInt>>.View.Select(graph, photoLogId);
        }

        public IEnumerable<PhotoLog> GetPhotoLogs(int? statusId)
        {
            return SelectFrom<PhotoLog>
                .Where<PhotoLog.statusId.IsEqual<P.AsInt>>.View.Select(graph, statusId).FirstTableItems;
        }

        public PhotoLogStatus GetDefaultStatus()
        {
            return SelectFrom<PhotoLogStatus>
                .Where<PhotoLogStatus.isDefault.IsEqual<True>>.View.Select(graph);
        }

        public Photo GetMainPhoto(int? photoLogId)
        {
            return SelectFrom<Photo>
                .Where<Photo.photoLogId.IsEqual<P.AsInt>
                    .And<Photo.photoId.IsNotEqual<Photo.photoId.FromCurrent>>
                    .And<Photo.isMainPhoto.IsEqual<True>>>.View.Select(graph, photoLogId);
        }

        public Photo GetMainPhoto(string photoLogCd)
        {
            return SelectFrom<Photo>
                .LeftJoin<PhotoLog>
                    .On<PhotoLog.photoLogId.IsEqual<Photo.photoLogId>>
                .Where<PhotoLog.photoLogCd.IsEqual<P.AsString>
                    .And<Photo.photoId.IsNotEqual<Photo.photoId.FromCurrent>>
                    .And<Photo.isMainPhoto.IsEqual<True>>>.View.Select(graph, photoLogCd);
        }
    }
}