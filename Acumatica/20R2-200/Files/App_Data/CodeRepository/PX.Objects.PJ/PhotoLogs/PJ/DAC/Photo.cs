using System;
using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.Attributes;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PJ.PhotoLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName("Photo")]
    [PXPrimaryGraph(typeof(PhotoEntry))]
    public class Photo : IBqlTable
    {
        [PXDBInt]
        [PXDefault]
        [PXParent(typeof(SelectFrom<PhotoLog>
            .Where<PhotoLog.photoLogId.IsEqual<photoLogId>>))]
        [PXSelector(typeof(SearchFor<PhotoLog.photoLogId>), SubstituteKey =
            typeof(PhotoLog.photoLogCd))]
        [PXUIField(DisplayName = "Photo Log ID")]
        public int? PhotoLogId
        {
            get;
            set;
        }

        [PXDBIdentity]
        public int? PhotoId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Photo ID", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [AutoNumber(typeof(PhotoLogSetup.photoNumberingId), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(SearchFor<photoCd>
            .Where<photoLogId.IsEqual<photoLogId.FromCurrent>
                .Or<photoLogId.FromCurrent.IsNull>>))]
        public string PhotoCd
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public string Name
        {
            get;
            set;
        }

        [PXDBText(IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public string Description
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Uploaded On", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public DateTime? UploadedDate
        {
            get;
            set;
        }

        [PXDBGuid]
        [PXUIField(DisplayName = "Uploaded By", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(PXDBCreatedByIDAttribute.Creator.pKID),
            SubstituteKey = typeof(PXDBCreatedByIDAttribute.Creator.username),
            DescriptionField = typeof(PXDBCreatedByIDAttribute.Creator.displayName))]
        public Guid? UploadedById
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Main Photo")]
        public bool? IsMainPhoto
        {
            get;
            set;
        }

        [PXDBGuid]
        [PXUIField(Visible = false)]
        public Guid? FileId
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Image", Visibility = PXUIVisibility.SelectorVisible)]
        public string ImageUrl
        {
            get;
            set;
        }

        [PhotoNote]
        public Guid? NoteID
        {
            get;
            set;
        }

        [PXDBCreatedByID(Visible = false)]
        public Guid? CreatedById
        {
            get;
            set;
        }

        [PXBool]
        [UiInformationField]
        public bool? Selected
        {
            get;
            set;
        }

        [CRAttributesField(typeof(usrPhotoClassId), typeof(noteID))]
        public string[] Attributes
        {
            get;
            set;
        }

        [PXDBTimestamp]
        public byte[] Tstamp
        {
            get;
            set;
        }

        [PXDBCreatedByScreenID]
        public string CreatedByScreenId
        {
            get;
            set;
        }

        [PXDBCreatedDateTime]
        public DateTime? CreatedDateTime
        {
            get;
            set;
        }

        [PXDBLastModifiedByID(Visibility = PXUIVisibility.Invisible)]
        public Guid? LastModifiedById
        {
            get;
            set;
        }

        [PXDBLastModifiedByScreenID]
        public string LastModifiedByScreenId
        {
            get;
            set;
        }

        [PXDBLastModifiedDateTime]
        public DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        [PXString(20)]
        public string UsrPhotoClassId => Constants.PhotoClassId;

        public abstract class selected : BqlBool.Field<selected>
        {
        }

        public abstract class photoId : BqlInt.Field<photoId>
        {
        }

        public abstract class photoCd : BqlString.Field<photoCd>
        {
        }

        public abstract class photoLogId : BqlInt.Field<photoLogId>
        {
        }

        public abstract class name : BqlString.Field<name>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class uploadedDate : BqlDateTime.Field<uploadedDate>
        {
        }

        public abstract class uploadedById : BqlGuid.Field<uploadedById>
        {
        }

        public abstract class isMainPhoto : BqlBool.Field<isMainPhoto>
        {
        }

        public abstract class fileId : BqlGuid.Field<fileId>
        {
        }

        public abstract class imageUrl : BqlString.Field<imageUrl>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class createdById : BqlGuid.Field<createdById>
        {
        }

        public abstract class usrPhotoClassId : BqlString.Field<usrPhotoClassId>
        {
        }

        public abstract class attributes : BqlAttributes.Field<attributes>
        {
        }

        public class photoClassId : BqlString.Constant<photoClassId>
        {
            public photoClassId()
                : base(Constants.PhotoClassId)
            {
            }
        }

        public class typeName : BqlString.Constant<typeName>
        {
            public typeName()
                : base(typeof(Photo).FullName)
            {
            }
        }
    }
}
