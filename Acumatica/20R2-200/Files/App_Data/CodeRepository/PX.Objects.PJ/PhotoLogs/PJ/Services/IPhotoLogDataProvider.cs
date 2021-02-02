using System.Collections.Generic;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public interface IPhotoLogDataProvider
    {
        PhotoLog GetPhotoLog(int? photoLogId);

        IEnumerable<PhotoLog> GetPhotoLogs(int? statusId);

        PhotoLogStatus GetDefaultStatus();

        Photo GetMainPhoto(int? photoLogId);

        Photo GetMainPhoto(string photoLogCd);
    }
}