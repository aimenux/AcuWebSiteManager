using System;
using PX.Data;

namespace PX.Objects.CR
{
    [Serializable]
    [PXHidden]
	[PXCacheName(Messages.ActivityRelation)]
	public partial class CRActivityRelation : IBqlTable
	{
		#region RecordID

		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual int? RecordID { get; set; }

		#endregion

		#region NoteID

		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		[PXDBGuid]
		[PXUIField(Visible = false)]
		[PXDBDefault(typeof(CRActivity.noteID))]
		public virtual Guid? ParentNoteID { get; set; }

		#endregion

		#region RefNoteID

		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Task ID")]
        [PXDBDefault(typeof(CRActivity.noteID))]
		[RefTaskSelector(typeof(CRActivity.noteID))]
		public virtual Guid? RefNoteID { get; set; }

		#endregion

		#region Subject

		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXString]
		[PXUIField(DisplayName = "Subject", Enabled = false)]
		public virtual string Subject { get; set; }

		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		[PXDate(InputMask = "g", DisplayMask = "g")]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		[PXDate(InputMask = "g", DisplayMask = "g")]
		[PXUIField(DisplayName = "Due Date", Enabled = false)]
		public virtual DateTime? EndDate { get; set; }
		#endregion

		#region CompletedDateTime
		public abstract class completedDateTime : PX.Data.BQL.BqlDateTime.Field<completedDateTime> { }

		[PXDate(InputMask = "g", DisplayMask = "g")]
		[PXUIField(DisplayName = "Completed At", Enabled = false)]
		public virtual DateTime? CompletedDateTime { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXString]
		[ActivityStatusList]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual string Status { get; set; }
		#endregion


		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
	}
}
