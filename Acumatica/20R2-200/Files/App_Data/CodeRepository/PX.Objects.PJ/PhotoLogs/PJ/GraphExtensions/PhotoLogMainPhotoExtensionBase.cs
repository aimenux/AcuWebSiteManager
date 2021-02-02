using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
    public abstract class PhotoLogMainPhotoExtensionBase<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
    {
        public SelectFrom<Photo>.View MainPhotoPhotos;

        [InjectDependency]
        public IPhotoLogDataProvider PhotoLogDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public IPhotoConfirmationService PhotoConfirmationService
        {
            get;
            set;
        }

        public virtual void _(Events.FieldUpdated<Photo, Photo.isMainPhoto> args)
        {
            if ((bool) args.NewValue)
            {
                var mainPhoto = PhotoLogDataProvider.GetMainPhoto(args.Row.PhotoLogId);
                if (mainPhoto != null)
                {
                    UnmarkPhoto(mainPhoto, args.Row);
                }
            }
        }

        protected void UnmarkPhoto(Photo mainPhoto, Photo currentPhoto)
        {
            var photo = PhotoConfirmationService.IsMarkConfirmed(MainPhotoPhotos)
                ? mainPhoto
                : currentPhoto;
            photo.IsMainPhoto = false;
            MainPhotoPhotos.Cache.Update(photo);
        }
    }
}