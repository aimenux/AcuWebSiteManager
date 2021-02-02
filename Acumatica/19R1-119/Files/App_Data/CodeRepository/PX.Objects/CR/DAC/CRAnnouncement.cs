using System;
using PX.Common;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	[PXPrimaryGraph(typeof(CRAnnouncementMaint))]
    [Serializable]
	[PXHidden]
	public class CRAnnouncement : PX.Data.IBqlTable
	{
		#region AnnouncementsID
		public abstract class announcementsID : PX.Data.BQL.BqlInt.Field<announcementsID> { }
		protected Int32? _AnnouncementsID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Announcement", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(announcementsID), DescriptionField = typeof(subject))]
		public virtual Int32? AnnouncementsID
		{
			get
			{
				return this._AnnouncementsID;
			}
			set
			{
				this._AnnouncementsID = value;
			}
		}
		#endregion
		
		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		protected String _Subject;
		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Subject
		{
			get
			{
				return _Subject;
			}
			set
			{
				this._Subject = value;
			}
		}
		#endregion

		#region Body
		public abstract class body : PX.Data.BQL.BqlString.Field<body> { }
		protected String _Body;
		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Body")]
		public virtual String Body
		{
			get
			{
				return this._Body;
			}
			set
			{
				this._Body = value;
			}
		}
		#endregion

		#region Order
		public abstract class order : PX.Data.BQL.BqlInt.Field<order> { }
		protected Int32? _Order;
		[PXInt]
		[PXUIField(DisplayName = "Order")]
		public virtual Int32? Order
		{
			get { return this._Order; }
			set { this._Order = value; }
		}
		#endregion

		#region SmallBody
		public abstract class smallbody : PX.Data.BQL.BqlString.Field<smallbody> { }
		protected String _Smallbody;
		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Body")]
		public virtual String Smallbody
		{
			get
			{
				return _Smallbody;
			}
			set
			{
				this._Smallbody = value;
			}
		}
		#endregion

		#region MajorStatus
		public abstract class majorStatus : PX.Data.BQL.BqlInt.Field<majorStatus> { }
		[PXDBInt]
		[AnnouncementMajorStatuses]
		[PXDefault(AnnouncementMajorStatusesAttribute._DRAFT, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Int32? MajorStatus { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate(InputMask = "g", PreserveTime = true)]
		[PXUIField(DisplayName = "Publication Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(new Type[0])]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected string _Status;
		[PXDefault(NotificationStatusAttribute.Draft)]
		[PXStringList(new string[] { NotificationStatusAttribute.Draft, NotificationStatusAttribute.Published, NotificationStatusAttribute.Archived },
			new string[] { "Draft", "Published", "Archived" })]
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status")]
		public virtual string Status { get; set; }
		#endregion

		#region IsPortalVisible
		public abstract class isPortalVisible : PX.Data.BQL.BqlBool.Field<isPortalVisible> { }
		protected bool? _IsPortalVisible;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Visible on Portal")]
		public virtual bool? IsPortalVisible
		{
			get
			{
				return _IsPortalVisible;
			}
			set
			{
				_IsPortalVisible = value;
			}
		}
		#endregion

		#region PublishedDateTime
		public abstract class publishedDateTime : PX.Data.BQL.BqlDateTime.Field<publishedDateTime> { }
		protected DateTime? _PublishedDateTime;
		[PXDBDate(InputMask = "g", PreserveTime = true, UseTimeZone = true)]
		[PXUIField(DisplayName = "Published Date")]
		public virtual DateTime? PublishedDateTime { get; set; }
		#endregion

		#region Category
		public abstract class category : PX.Data.BQL.BqlString.Field<category> { }
		protected String _Category;
		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXSelector(typeof(Search4<category, Where<category, IsNotNull> ,Aggregate<GroupBy<category>>>))]
		[PXUIField(DisplayName = "Category", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Category
		{
			get
			{
				return this._Category;
			}
			set
			{
				this._Category = value;
			}
		}

		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		#endregion
	}
}
