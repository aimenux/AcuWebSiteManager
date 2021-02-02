using System;
using System.Diagnostics;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[Serializable]
	[DebuggerDisplay("EventNoteID = {EventNoteID}, UserID = {UserID}, Type = {Type}")]
	[PXCacheName(Messages.AttendeeMessage)]
	public partial class EPAttendeeMessage : IBqlTable
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
		[PXUIField(Visible = false)]
		public virtual Guid? UserID { get; set; }
		#endregion

		#region MessageID
		public abstract class messageID : PX.Data.BQL.BqlGuid.Field<messageID> { }
		
		[PXDBGuid(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual Guid? MessageID { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlBool.Field<type> { }
		
		[EPMessageType.EPMessageTypeList]
		[PXUIField(DisplayName = "Type")]
		public virtual bool? Type { get; set; }
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
