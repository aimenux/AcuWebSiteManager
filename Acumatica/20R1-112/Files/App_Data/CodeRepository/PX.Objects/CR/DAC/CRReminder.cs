using System;
using PX.Data;
using PX.Data.EP;
using PX.SM;
using PX.TM;
using PX.Web.UI;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.Reminder)]
	public class CRReminder : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region IsReminderOn
		public abstract class isReminderOn : PX.Data.BQL.BqlBool.Field<isReminderOn> { }

		[PXBool]
		[PXFormula(typeof(
			Switch<
				Case<Where<reminderDate, IsNotNull>, True>
			, False>))]
		[PXUIField(DisplayName = "Reminder")]
		public virtual bool? IsReminderOn { get; set; }
		#endregion

		#region ReminderIcon
		public abstract class reminderIcon : PX.Data.BQL.BqlString.Field<reminderIcon>
		{
			public class reminder : PX.Data.BQL.BqlString.Constant<reminder>
			{
				public reminder() : base(Sprite.Control.GetFullUrl(Sprite.Control.Reminder)) { }
			}
		}

		[PXUIField(DisplayName = "Reminder Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.ReminderHead))]
		[PXFormula(typeof(Switch<Case<Where<reminderDate, IsNotNull>, CRReminder.reminderIcon.reminder>>))]
		public virtual String ReminderIcon { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXSequentialNote(new Type[0], SuppressActivitiesCount = true, IsKey = true)]
		[PXTimeTag(typeof(noteID))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXDBGuid]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false)]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region ReminderDate
		public abstract class reminderDate : PX.Data.BQL.BqlDateTime.Field<reminderDate> { }

		[PXDBDateAndTime(InputMask = "g", PreserveTime = true, UseTimeZone = true)]
		//[PXRemindDate(typeof(isReminderOn), typeof(startDate), InputMask = "g", PreserveTime = true)]
		[PXUIField(DisplayName = "Remind at")]
		public virtual DateTime? ReminderDate { get; set; }
		#endregion
		
		#region Owner
		public abstract class owner : PX.Data.BQL.BqlGuid.Field<owner> { }

		[PXDBGuid]
		[PXChildUpdatable(AutoRefresh = true)]
		//[PXOwnerSelector(typeof(groupID))]
		// cutted done
		[PXSubordinateOwnerSelector]
		[PXUIField(DisplayName = "Owner")]
		public virtual Guid? Owner { get; set; }
		#endregion

		#region Dismiss
		public abstract class dismiss : PX.Data.BQL.BqlBool.Field<dismiss> { }
		
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Dismiss { get; set; }
		#endregion


		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID(DontOverrideValue = true)]
		[PXUIField(Enabled = false)]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
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

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}
}