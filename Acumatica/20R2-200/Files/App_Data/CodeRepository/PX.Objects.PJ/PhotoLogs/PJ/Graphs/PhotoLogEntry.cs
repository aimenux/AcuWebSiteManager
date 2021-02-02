using System.Collections;
using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Common.Services;
using PX.Objects.CS;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
    public class PhotoLogEntry : ProjectManagementBaseMaint<PhotoLogEntry, PhotoLog>
    {
        [PXCopyPasteHiddenFields(typeof(PhotoLog.date))]
        [PXViewName("Photo Log")]
        public SelectFrom<PhotoLog>.View PhotoLog;

        [PXCopyPasteHiddenView]
        [PXFilterable(typeof(Photo))]
        public SelectFrom<Photo>
            .Where<Photo.photoLogId.IsEqual<PhotoLog.photoLogId.FromCurrent>>.View Photos;

        [PXHidden]
        public SelectFrom<Photo>.View PhotoImage;

        public PXSetup<PhotoLogSetup> PhotoLogSetup;

        public PXAction<PhotoLog> CreatePhoto;

        public PXAction<PhotoLog> ViewPhoto;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<Photo.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<Photo.photoClassId>>>> PhotoAttributes;

        public SelectFrom<CSAnswers>
            .RightJoin<Photo>.On<CSAnswers.refNoteID.IsEqual<Photo.noteID>>.View Answers;

        public PhotoLogEntry()
        {
	        PhotoLogSetup setup = PhotoLogSetup.Current;

            var commonAttributeColumnCreator = new CommonAttributeColumnCreator(this, PhotoAttributes);
            commonAttributeColumnCreator.GenerateColumns(Photos.Cache, nameof(Photos), nameof(Answers), false);
        }

        public virtual IEnumerable photoImage()
        {
            return SelectFrom<Photo>
                .Where<Photo.photoCd.IsEqual<P.AsString>
                    .And<Photo.photoLogId.IsEqual<P.AsInt>>>.View
                .Select(this, Photos.Current?.PhotoCd, Photos.Current?.PhotoLogId);
        }

        [PXButton(CommitChanges = true, OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField]
        public void createPhoto()
        {
            var photoEntry = CreateInstance<PhotoEntry>();
            if (PhotoLog.Cache.GetStatus(PhotoLog.Current) != PXEntryStatus.Inserted)
            {
                var photo = photoEntry.Photos.Insert();
                photo.PhotoLogId = PhotoLog.Current.PhotoLogId;
            }
            PXRedirectHelper.TryRedirect(photoEntry, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton]
        [PXUIField]
        public void viewPhoto()
        {
            var photoEntry = CreateInstance<PhotoEntry>();
            photoEntry.Photos.Current =
                photoEntry.Photos.Search<Photo.photoId>(Photos.Current.PhotoId);
            PXRedirectHelper.TryRedirect(photoEntry, PXRedirectHelper.WindowMode.NewWindow);
        }

        public virtual void _(Events.RowSelected<Photo> args)
        {
            PXUIFieldAttribute.SetEnabled(args.Cache, args.Row, false);
            PXUIFieldAttribute.SetEnabled<Photo.isMainPhoto>(args.Cache, args.Row);
        }

        public override void _(Events.RowSelected<PhotoLog> args)
        {
            if (args.Row is PhotoLog photoLog)
            {
                base._(args);
                CreatePhoto.SetEnabled(PhotoLog.Cache.GetStatus(photoLog) != PXEntryStatus.Inserted);
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
    }
}