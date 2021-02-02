using System.Collections.Generic;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public interface IRedirectionService
    {
        void RedirectToEntity(List<DrawingLog> drawingLogs);

        void RedirectToEntity(DrawingLog drawingLog);
    }
}