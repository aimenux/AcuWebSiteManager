using PX.Common;

namespace PX.Objects.PJ.PhotoLogs.Descriptor
{
    [PXLocalizable]
    public class PhotoLogMessages
    {
        public const string PhotoLogs = "photo logs";

        public const string AnotherPhotoHasAlreadyBeenMarkedAsTheMainPhoto =
            "Another photo has already been marked as the main photo in the photo log. Would you like to mark the current photo as the main photo instead?";

        public const string NoRecordsWereSelected = "Select one or multiple photo logs.";

        public const string NoPhotosAddedToPhotoLog = "At least one photo should be added to a photo log.";

        public const string NoPhotosInSelectedPhotoLogs = "There are no photos in the selected photo logs.";
    }
}
