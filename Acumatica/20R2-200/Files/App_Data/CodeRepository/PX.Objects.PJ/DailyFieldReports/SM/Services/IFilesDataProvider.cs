using System;

namespace PX.Objects.PJ.DailyFieldReports.SM.Services
{
    public interface IFilesDataProvider
    {
        bool DoesFileHaveRelatedHistoryRevision(Guid? fileId);
    }
}
