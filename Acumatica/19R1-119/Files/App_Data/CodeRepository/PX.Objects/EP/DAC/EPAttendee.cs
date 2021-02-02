using System;
using System.Diagnostics;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[Serializable]
	[DebuggerDisplay("EventNoteID = {EventNoteID}, UserID = {UserID}")]
	[PXCacheName(Messages.Attendee)]
	public partial class EPAttendee : IBqlTable
	{
		#region EventNoteID
		public abstract class eventNoteID : PX.Data.BQL.BqlGuid.Field<eventNoteID> { }
		
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRActivity.noteID))]
		public virtual Guid? EventNoteID { get; set; }
		#endregion

		#region UserID
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		
		[PXDBGuid(IsKey = true)]
		public virtual Guid? UserID { get; set; }
		#endregion

		#region Invitation
		public abstract class invitation : PX.Data.BQL.BqlInt.Field<invitation> { }
		
		[PXDBInt]
		[PXUIField(DisplayName = "Invitation", Enabled = false)]
		[PXDefault(PXInvitationStatusAttribute.NOTINVITED)]
		[PXInvitationStatus]
		public virtual int? Invitation { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { } 

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
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
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
