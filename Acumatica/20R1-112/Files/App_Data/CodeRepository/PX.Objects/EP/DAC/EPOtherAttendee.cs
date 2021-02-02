using System;
using System.Diagnostics;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[Serializable]
	[DebuggerDisplay("EventNoteID = {EventNoteID}, Email = {Email}")]
    [PXHidden]
	public partial class EPOtherAttendee : IBqlTable
	{
		#region EventNoteID
		public abstract class eventNoteID : PX.Data.BQL.BqlGuid.Field<eventNoteID> { }
		
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRActivity.noteID))]
		public virtual Guid? EventNoteID { get; set; }
		#endregion

		#region AttendeeID
		public abstract class attendeeID : PX.Data.BQL.BqlInt.Field<attendeeID> { }
		
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual int? AttendeeID { get; set; }
		#endregion

		#region Email
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		
		[PXDBEmail]
        [PXDefault]
		public virtual string Email { get; set; }
		#endregion

		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual string Name { get; set; }
		#endregion

		#region Comment
		public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
		
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Comment")]
		public virtual string Comment { get; set; }
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
