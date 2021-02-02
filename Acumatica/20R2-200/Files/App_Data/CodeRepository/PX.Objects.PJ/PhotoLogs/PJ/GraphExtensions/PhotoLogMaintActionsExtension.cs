using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Objects.PJ.PhotoLogs.PJ.Services;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
    public class PhotoLogMaintActionsExtension : PhotoLogActionsExtensionBase<PhotoLogMaint, PhotoLogFilter>
    {
        public override void Initialize()
        {
            base.Initialize();
            PhotoLogZipServiceBase = new PhotoLogMaintZipService(Base);
        }
    }
}