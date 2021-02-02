using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Data;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
    public class PhotoEntryMainPhotoExtension : PhotoLogMainPhotoExtensionBase<PhotoEntry>
    {
        public virtual void _(Events.FieldUpdating<Photo, Photo.photoLogId> args)
        {
            if (args.NewValue != null && args.Row.IsMainPhoto == true)
            {
                var newPhotoLogCd = args.NewValue as string;
                var mainPhoto = PhotoLogDataProvider.GetMainPhoto(newPhotoLogCd);
                if (mainPhoto != null)
                {
                    UnmarkPhoto(mainPhoto, args.Row);
                }
            }
        }
    }
}