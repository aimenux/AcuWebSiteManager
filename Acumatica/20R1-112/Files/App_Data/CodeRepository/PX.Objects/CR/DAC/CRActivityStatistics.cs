using System;
using PX.Data;
using PX.Objects.CS;

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

		#region InitialOutgoingActivityCompletedAtNoteID
		public abstract class initialOutgoingActivityCompletedAtNoteID : PX.Data.BQL.BqlGuid.Field<initialOutgoingActivityCompletedAtNoteID> { }
		[PXDBGuid]
		[PXDBDefault(typeof(CRActivity.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? InitialOutgoingActivityCompletedAtNoteID { get; set; }
		#endregion

		#region InitialOutgoingActivityCompletedAtDate
		public abstract class initialOutgoingActivityCompletedAtDate : PX.Data.BQL.BqlDateTime.Field<initialOutgoingActivityCompletedAtDate> { }
		[PXDBDate(PreserveTime = true, UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Initial Outgoing Activity", Enabled = false)]
		public virtual DateTime? InitialOutgoingActivityCompletedAtDate { get; set; }
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

		#region Activity Aging
		public enum LastActivityAgingEnum
		{
			None = 0,
			Last30days,
			Last3060days,
			Last6090days,
			Over90days
		}

		public class LastActivityAgingConst
		{
			public class none : PX.Data.BQL.BqlInt.Constant<none>
			{
				public none() : base((int)LastActivityAgingEnum.None) { }
			}
			public class last30days : Data.BQL.BqlInt.Constant<last30days>
			{
				public last30days() : base((int)LastActivityAgingEnum.Last30days) { }
			}
			public class last3060days : Data.BQL.BqlInt.Constant<last3060days>
			{
				public last3060days() : base((int)LastActivityAgingEnum.Last3060days) { }
			}
			public class last6090days : Data.BQL.BqlInt.Constant<last6090days>
			{
				public last6090days() : base((int)LastActivityAgingEnum.Last6090days) { }
			}
			public class over90days : Data.BQL.BqlInt.Constant<over90days>
			{
				public over90days() : base((int)LastActivityAgingEnum.Over90days) { }
			}
		}

		public abstract class lastActivityAging : PX.Data.BQL.BqlInt.Field<lastActivityAging> { }
		[PXUIField(DisplayName = "Last Activity Aging")]
		[PXInt]
		[PXIntList(typeof(LastActivityAgingEnum), 
			new[] {
				Messages.None,
				Messages.Last30days,
				Messages.Last3060days,
				Messages.Last6090days,
				Messages.Over90days
			})]
		[PXDBCalced(typeof(
				Switch<
						Case<
					Where<lastOutgoingActivityDate, IsNull>, LastActivityAgingConst.none,
						Case<
					Where<lastOutgoingActivityDate, GreaterEqual<Sub<CurrentValue<AccessInfo.businessDate>, int30>>>, LastActivityAgingConst.last30days,
						Case<
					Where<lastOutgoingActivityDate, GreaterEqual<Sub<CurrentValue<AccessInfo.businessDate>, int60>>>, LastActivityAgingConst.last3060days,
						Case<
					Where<lastOutgoingActivityDate, GreaterEqual<Sub<CurrentValue<AccessInfo.businessDate>, int90>>>, LastActivityAgingConst.last6090days>>>
					>,
					LastActivityAgingConst.over90days
					>)
			, typeof(int?))]
		public int? LastActivityAging { get; set; }
		#endregion

	}
}
