using System.Linq;
using System.Web;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.SM;
using Constants = PX.Objects.PJ.PhotoLogs.Descriptor.Constants;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
    public class PhotoEntry : PXGraph<PhotoEntry, Photo>
    {
        [PXCopyPasteHiddenFields(typeof(Photo.isMainPhoto))]
        public SelectFrom<Photo>.View Photos;

        public CRAttributeList<Photo> Attributes;

        public PXSetup<PhotoLogSetup> PhotoLogSetup;

        public PXAction<Photo> UploadPhoto;

        public PhotoEntry()
        {
	        PhotoLogSetup setup = PhotoLogSetup.Current;
        }

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Upload Photo")]
        public virtual void uploadPhoto()
        {
            if (Photos.AskExt() == WebDialogResult.OK)
            {
                DeleteAttachedPhoto();
                var fileInfo = PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[Constants.UploadFileKey];
                HttpContext.Current.Session.Remove(Constants.UploadFileKey);
                AttachPhoto(fileInfo);
                FillPhotoFields(fileInfo);
            }
        }

        public virtual void _(Events.RowSelected<Photo> args)
        {
            var photo = args.Row;
            if (photo == null)
            {
                return;
            }
            EnableFieldsInCopyPastContext(args.Cache, photo);
            if (IsCopyPasteContext && PXNoteAttribute.GetFileNotes(args.Cache, photo).IsEmpty() && photo.FileId != null)
            {
                PXNoteAttribute.SetFileNotes(args.Cache, photo, photo.FileId.Value);
            }
        }

        public virtual void _(Events.RowSelecting<Photo> args)
        {
            var photo = args.Row;
            if (photo != null)
            {
                photo.ImageUrl = photo.ImageUrl ?? string.Concat(PXUrl.SiteUrlWithPath(), Constants.FileUrl, photo.FileId);
            }
        }

        private void AttachPhoto(FileInfo fileInfo)
        {
            var graph = CreateInstance<UploadFileMaintenance>();
            var photoName = $"{Photos.Current.NoteID}\\{fileInfo.Name}";
            var photo = new FileInfo(photoName, null, fileInfo.BinData);
            graph.SaveFile(photo);
            PXNoteAttribute.SetFileNotes(Photos.Cache, Photos.Current, photo.UID.GetValueOrDefault());
        }

        private void FillPhotoFields(FileInfo fileInfo)
        {
            Photos.Current.Name = fileInfo.Name;
            Photos.Current.FileId = fileInfo.UID;
            Photos.Current.ImageUrl = ComposeImageUrl(fileInfo);
            Photos.Current.UploadedDate = Accessinfo.BusinessDate;
            Photos.Current.UploadedById = Accessinfo.UserID;
        }

        private static string ComposeImageUrl(FileInfo fileInfo)
        {
            return string.Concat(PXUrl.SiteUrlWithPath(), Constants.FileUrl, fileInfo.UID.ToString());
        }

        private void DeleteAttachedPhoto()
        {
            var fileNote = PXNoteAttribute.GetFileNotes(Photos.Cache, Photos.Current).SingleOrDefault();
            UploadFileMaintenance.DeleteFile(fileNote);
        }

        private void EnableFieldsInCopyPastContext(PXCache cache, Photo photo)
        {
            Enable<Photo.name>(cache, photo);
            Enable<Photo.uploadedById>(cache, photo);
            Enable<Photo.uploadedDate>(cache, photo);
        }

        private void Enable<TField>(PXCache cache, Photo photo)
            where TField : IBqlField
        {
            PXUIFieldAttribute.SetEnabled<TField>(cache, photo, IsCopyPasteContext);
        }
    }
}
