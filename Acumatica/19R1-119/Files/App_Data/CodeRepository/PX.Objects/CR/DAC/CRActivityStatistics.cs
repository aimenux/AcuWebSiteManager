using System;
using PX.Data;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.ActivityStatistics)]
	public class CRActivityStatistics : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid(IsKey = true)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region LastIncomingActivityNoteID
		public abstract class lastIncomingActivityNoteID : PX.Data.BQL.BqlGuid.Field<lastIncomingActivityNoteID> { }
		[PXDBGuid]
		[PXDBDefault(typeof(CRActivity.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? LastIncomingActivityNoteID { get; set; }
		#endregion

		#region LastOutgoingActivityNoteID
		public abstract class lastOutgoingActivityNoteID : PX.Data.BQL.BqlGuid.Field<lastOutgoingActivityNoteID> { }
		[PXDBGuid]
		[PXDBDefault(typeof(CRActivity.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? LastOutgoingActivityNoteID { get; set; }
		#endregion

		#region LastIncomingActivityDate
		public abstract class lastIncomingActivityDate : PX.Data.BQL.BqlDateTime.Field<lastIncomingActivityDate> { }
		[PXDBDate(PreserveTime = true, UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Last Incoming Activity", Enabled = false)]
		public virtual DateTime? LastIncomingActivityDate { get; set; }
		#endregion

		#region LastOutgoingActivityDate
		public abstract class lastOutgoingActivityDate : PX.Data.BQL.BqlDateTime.Field<lastOutgoingActivityDate> { }
        [PXDBDate(PreserveTime = true, UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Last Outgoing Activity", Enabled = false)]
		public virtual DateTime? LastOutgoingActivityDate { get; set; }
		#endregion

		#region LastActivityDate
		public abstract class lastActivityDate : PX.Data.BQL.BqlDateTime.Field<lastActivityDate> { }
		[PXDBCalced(typeof(Switch<
				Case<Where<lastIncomingActivityDate, IsNotNull, And<lastOutgoingActivityDate, IsNull>>, lastIncomingActivityDate,
				Case<Where<lastOutgoingActivityDate, IsNotNull, And<lastIncomingActivityDate, IsNull>>, lastOutgoingActivityDate,
				Case<Where<lastIncomingActivityDate, Greater<lastOutgoingActivityDate>>, lastIncomingActivityDate>>>, 
			lastOutgoingActivityDate>), 
			typeof(DateTime))]
		[PXUIField(DisplayName = "Last Activity Date", Enabled = false)]
		[PXDate]
		public virtual DateTime? LastActivityDate { get; set; }
		#endregion
	}
}
