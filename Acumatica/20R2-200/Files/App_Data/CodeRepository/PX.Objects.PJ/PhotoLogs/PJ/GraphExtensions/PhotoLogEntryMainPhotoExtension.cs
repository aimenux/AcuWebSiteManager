using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Data;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
    public class PhotoLogEntryMainPhotoExtension : PhotoLogMainPhotoExtensionBase<PhotoLogEntry>
    {
        public override void _(Events.FieldUpdated<Photo, Photo.isMainPhoto> args)
        {
            base._(args);
            Base.Photos.View.RequestRefresh();
        }
    }
}