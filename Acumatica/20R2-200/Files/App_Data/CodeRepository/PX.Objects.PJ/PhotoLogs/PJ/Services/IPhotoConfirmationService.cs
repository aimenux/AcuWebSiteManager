using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public interface IPhotoConfirmationService
    {
        bool IsMarkConfirmed(PXSelectBase<Photo> view);
    }
}