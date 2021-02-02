using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public class PhotoConfirmationService : IPhotoConfirmationService
    {
        public bool IsMarkConfirmed(PXSelectBase<Photo> view)
        {
            return view.Ask(SharedMessages.Warning, PhotoLogMessages.AnotherPhotoHasAlreadyBeenMarkedAsTheMainPhoto,
                MessageButtons.YesNo).IsPositive();
        }
    }
}
