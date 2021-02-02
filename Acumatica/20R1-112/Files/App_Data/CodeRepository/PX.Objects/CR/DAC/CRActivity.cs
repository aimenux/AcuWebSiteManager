using System;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using PX.Web.UI;

namespace PX.Objects.CR
{
	/// <exclude/>
	[PXBreakInheritance]
	[PXTable(typeof(CRActivity.noteID))]
	public partial class CRParentActivity : CRActivity
	{

	}

	/// <exclude/>
	[CRActivityPrimaryGraph]
	[Serializable]
	[PXCacheName(Messages.ActivityClassInfo)]
	[PXEMailSource]
	public partial class CRActivity : IBqlTable, IAssign, INotable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		
		[PXSequentialNote(new Type[0], SuppressActivitiesCount = true, IsKey = true)]
		[PXUIField(DisplayName = "ID")]
		[PXTimeTag(typeof(noteID))]
		[CRActivityStatisticFormulas]
		[PXSelector(typeof(noteID),
			new[] { typeof(noteID) })]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region ParentNoteID
		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Task")]
        [PXSelector(typeof(Search<CRParentActivity.noteID>),
                 typeof(CRParentActivity.subject),
                 typeof(CRParentActivity.priority),
                 typeof(CRParentActivity.startDate),
                 typeof(CRParentActivity.endDate))]
        public virtual Guid? ParentNoteID { get; set; }
		#endregion
		
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXDBGuid]
		[PXParent(typeof(Select<CRActivityStatistics, Where<CRActivityStatistics.noteID, Equal<Current<refNoteID>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXUIField(DisplayName = "References Nbr.")]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region DocumentNoteID
		public abstract class documentNoteID : PX.Data.BQL.BqlGuid.Field<documentNoteID> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Related Document")]
		public virtual Guid? DocumentNoteID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Related Entity Description", Enabled = false)]
		[PXFormula(typeof(EntityDescription<refNoteID>))]
		public virtual string Source { get; set; }
		#endregion
		
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }

		[PXDBInt]
		[CRActivityClass]
		[PXDefault(typeof(CRActivityClass.activity))]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual int? ClassID { get; set; }
		#endregion

		#region ClassIcon

		public abstract class classIcon : PX.Data.BQL.BqlString.Field<classIcon>
		{
			public class task : PX.Data.BQL.BqlString.Constant<task>
			{
				public task() : base(Sprite.Main.GetFullUrl(Sprite.Main.Task)) { }
			}
			public class events : PX.Data.BQL.BqlString.Constant<events>
			{
				public events() : base(Sprite.Main.GetFullUrl(Sprite.Main.Event)) { }
			}
			public class email : PX.Data.BQL.BqlString.Constant<email>
			{
				public email() : base(Sprite.Main.GetFullUrl(Sprite.Main.MailSend)) { }
			}
			public class emailResponse : PX.Data.BQL.BqlString.Constant<emailResponse>
			{
				public emailResponse() : base(Sprite.Main.GetFullUrl(Sprite.Main.MailReceive)) { }
			}
		}

		[PXUIField(DisplayName = "Class Icon", IsReadOnly = true)]
		[PXImage]
		[PXFormula(typeof(Switch<Case<Where<classID, Equal<CRActivityClass.task>>, CRActivity.classIcon.task,
			Case<Where<classID, Equal<CRActivityClass.events>>, CRActivity.classIcon.events,
			Case<Where<classID, Equal<CRActivityClass.email>, And<incoming, Equal<True>>>, CRActivity.classIcon.email,
			Case<Where<classID, Equal<CRActivityClass.email>, And<outgoing, Equal<True>>>, CRActivity.classIcon.emailResponse
			>>>>,
			Selector<Current2<type>, EPActivityType.imageUrl>>))]
		public virtual string ClassIcon { get; set; }
		#endregion

		#region ClassInfo
		public abstract class classInfo : PX.Data.BQL.BqlString.Field<classInfo> { }

		[PXString]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<Current2<noteID>, IsNull>, StringEmpty,
			Case<Where<classID, Equal<CRActivityClass.activity>, And<type, IsNotNull>>, Selector<type, EPActivityType.description>>>,
			String<classID>>))]
		public virtual string ClassInfo { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXDBString(5, IsFixed = true, IsUnicode = false)]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
		[PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), Messages.InactiveActivityType, typeof(EPActivityType.type))]
		[PXRestrictor(typeof(Where<EPActivityType.isInternal, Equal<True>, Or<EPActivityType.isSystem, Equal<True>>>), Messages.ExternalActivityType, typeof(EPActivityType.type))]
		[PXDefault(typeof(Search<EPActivityType.type,
			Where<Current<classID>, Equal<EPActivityType.classID>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Type { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		
		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(subject))]
		public virtual string Subject { get; set; }
		#endregion

		#region Location
		public abstract class location : PX.Data.BQL.BqlString.Field<location> { }

		[PXDBString(255, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Location")]
		public virtual string Location { get; set; }
		#endregion

		#region Body
		public abstract class body : PX.Data.BQL.BqlString.Field<body> { }

		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Activity Details")]
		public virtual string Body { get; set; }
		#endregion

		#region Priority
		public abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Priority")]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXIntList(
			new [] { 0, 1, 2 },
			new [] { Messages.Low, Messages.Normal, Messages.High })]
		public virtual int? Priority { get; set; }
		#endregion

		#region PriorityIcon
		public abstract class priorityIcon : PX.Data.BQL.BqlString.Field<priorityIcon>
		{
			public class low : PX.Data.BQL.BqlString.Constant<low>
			{
				public low() : base(Sprite.Control.GetFullUrl(Sprite.Control.PriorityLow)) { }
			}
			public class high : PX.Data.BQL.BqlString.Constant<high>
			{
				public high() : base(Sprite.Control.GetFullUrl(Sprite.Control.PriorityHigh)) { }
			}
		}

		[PXUIField(DisplayName = "Priority Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.PriorityHead))]
		[PXFormula(typeof(Switch<Case<Where<priority, Equal<int0>>, CRActivity.priorityIcon.low,
			Case<Where<priority, Equal<int2>>, CRActivity.priorityIcon.high>>>))]
		public virtual string PriorityIcon { get; set; }
		#endregion

		#region UIStatus
		public abstract class uistatus : PX.Data.BQL.BqlString.Field<uistatus> { }

		[PXDBString(2, IsFixed = true)]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string UIStatus { get; set; }
		#endregion

		#region IsOverdue
		public abstract class isOverdue : PX.Data.BQL.BqlBool.Field<isOverdue> { }

		[PXBool]
		public virtual bool? IsOverdue
		{
			[PXDependsOnFields(typeof(uistatus), typeof(endDate))]
			get
			{
				return
					UIStatus != ActivityStatusAttribute.Completed &&
					UIStatus != ActivityStatusAttribute.Canceled &&
					EndDate != null &&
					EndDate < PX.Common.PXTimeZoneInfo.Now;
			}
		}
		#endregion

		#region IsCompleteIcon
		public abstract class isCompleteIcon : PX.Data.BQL.BqlString.Field<isCompleteIcon>
		{
			public class completed : PX.Data.BQL.BqlString.Constant<completed>
			{
				public completed() : base(Sprite.Control.GetFullUrl(Sprite.Control.Complete)) { }
			}
		}

		[PXUIField(DisplayName = "Complete Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.CompleteHead))]
		[PXFormula(typeof(Switch<Case<Where<uistatus, Equal<ActivityStatusListAttribute.completed>>, CRActivity.isCompleteIcon.completed>>))]
		public virtual String IsCompleteIcon { get; set; }
		#endregion

		#region CategoryID
		public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }

		[PXDBInt]
		[PXSelector(typeof(EPEventCategory.categoryID), SubstituteKey = typeof(EPEventCategory.description))]
		[PXUIField(DisplayName = "Category")]
		public virtual int? CategoryID { get; set; }
		#endregion

		#region AllDay
		public abstract class allDay : PX.Data.BQL.BqlBool.Field<allDay> { }

		[EPAllDay(typeof(startDate), typeof(endDate))]
		[PXUIField(DisplayName = "All Day")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Switch<Case<Where<classID, Equal<CRActivityClass.task>>, True>, False>))]
        public virtual bool? AllDay { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		
		[EPStartDate(AllDayField = typeof(allDay), DisplayName = "Start Date", DisplayNameDate = "Date", DisplayNameTime = "Start Time")]
		[PXFormula(typeof(TimeZoneNow))]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		[EPEndDate(typeof(classID), typeof(startDate), AllDayField = typeof(allDay))]
		[PXUIField(DisplayName = "End Time")]
		public virtual DateTime? EndDate { get; set; }
		#endregion

		#region CompletedDate
		public abstract class completedDate : PX.Data.BQL.BqlDateTime.Field<completedDate> { }

		[PXDBDate(InputMask = "g", PreserveTime = true)]
		[PXUIField(DisplayName = "Completed At", Enabled = false)]
		[PXFormula(typeof(
			Switch<
				Case<Where<uistatus, Equal<ActivityStatusAttribute.completed>>, PXDateAndTimeAttribute.now>,
			completedDate>))]
		public virtual DateTime? CompletedDate { get; set; }
		#endregion

		#region DayOfWeek
		public abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }

		[PXInt]
		[PXUIField(DisplayName = PX.Objects.EP.Messages.DayOfWeek)]
		[DayOfWeek]
		public virtual int? DayOfWeek
		{
			[PXDependsOnFields(typeof(startDate))]
			get
			{
				var date = StartDate;
				return date != null ? (int?)((int)date.Value.DayOfWeek) : null;
			}
		}
		#endregion

		#region PercentCompletion
		public abstract class percentCompletion : PX.Data.BQL.BqlInt.Field<percentCompletion> { }

		[PXDBInt(MinValue = 0, MaxValue = 100)]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Completion (%)")]
		public virtual int? PercentCompletion { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXDBGuid]
		[PXChildUpdatable(AutoRefresh = true)]
		[PXOwnerSelector(typeof(workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		[PXDefault(typeof(Coalesce<
			Search<EPCompanyTreeMember.userID,
				Where<EPCompanyTreeMember.workGroupID, Equal<Current<workgroupID>>,
					And<EPCompanyTreeMember.userID, Equal<Current<AccessInfo.userID>>>>>,
			Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<workgroupID>, IsNull>>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<workgroupID>))]
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXDBInt]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		public virtual int? WorkgroupID { get; set; }
		#endregion
		
		#region IsExternal
		public abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }

		[PXDBBool]
		[PXUIField(Visible = false)]
		public virtual bool? IsExternal { get; set; }
		#endregion

		#region IsPrivate
		public abstract class isPrivate : PX.Data.BQL.BqlBool.Field<isPrivate> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Internal")]
		[PXFormula(typeof(IsNull<Selector<type, EPActivityType.privateByDefault>, False>))]
		public virtual bool? IsPrivate { get; set; }
		#endregion

		#region Incoming
		public abstract class incoming : PX.Data.BQL.BqlBool.Field<incoming> { }

		[PXDBBool]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.incoming>>, False>))]
		[PXUIField(DisplayName = "Incoming")]
		public virtual bool? Incoming { get; set; }
		#endregion

		#region Outgoing
		public abstract class outgoing : PX.Data.BQL.BqlBool.Field<outgoing> { }

		[PXDBBool]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.outgoing>>, False>))]
		[PXUIField(DisplayName = "Outgoing")]
		public virtual bool? Outgoing { get; set; }
		#endregion
		
		#region Synchronize
		public abstract class synchronize : PX.Data.BQL.BqlBool.Field<synchronize> { }

		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Synchronize")]
		public virtual bool? Synchronize { get; set; }
		#endregion
		
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Business Account")]
		[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD))]
		public virtual int? BAccountID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]
		public virtual int? ContactID { get; set; }
		#endregion
		
		#region EntityDescription

		public abstract class entityDescription : PX.Data.BQL.BqlString.Field<entityDescription> { }

		[PXString(InputMask = "")]
		[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual string EntityDescription { get; set; }
		#endregion

		#region ShowAsID
		public abstract class showAsID : PX.Data.BQL.BqlInt.Field<showAsID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Show As")]
		[PXSelector(typeof(EPEventShowAs.showAsID), DescriptionField = typeof(EPEventShowAs.description))]
		public virtual int? ShowAsID { get; set; }
		#endregion

		#region IsLocked
		public abstract class isLocked : PX.Data.BQL.BqlBool.Field<isLocked> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Locked")]
		public virtual bool? IsLocked { get; set; }
		#endregion



		#region DeletedDatabaseRecord
		public abstract class deletedDatabaseRecord : PX.Data.BQL.BqlBool.Field<deletedDatabaseRecord> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? DeletedDatabaseRecord { get; set; }
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
		
		[PXDBTimestamp(RecordComesFirst = true)]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}

	/// <exclude/>
	[PXProjection(typeof(Select<CRActivity>))]
	[PXHidden]
	public partial class CRActivityLink : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuid(IsKey = true, BqlField = typeof(CRActivity.noteID))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region ParentNoteID
		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		[PXDBGuid(BqlField = typeof(CRActivity.parentNoteID))]
		public virtual Guid? ParentNoteID { get; set; }
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXDBGuid(BqlField = typeof(CRActivity.refNoteID))]
		public virtual Guid? RefNoteID { get; set; }
		#endregion
	}

	/// <exclude/>
	[Serializable]
	[PXProjection(typeof(Select2<CRActivity,
		LeftJoin<SMEmail, 
			On<SMEmail.refNoteID, Equal<CRActivity.noteID>>,
		LeftJoin<PMTimeActivity,
			On<PMTimeActivity.refNoteID, Equal<CRActivity.noteID>>>>>), Persistent = true)]
	public partial class CRPMSMEmail : CRActivity
	{
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXSequentialNote(new Type[0], SuppressActivitiesCount = true, IsKey = true, BqlField = typeof(CRActivity.noteID))]
		[PXTimeTag(typeof(noteID))]
		[CRPMTimeActivityStatisticFormulas]
		public override Guid? NoteID { get; set; }
		#endregion

		#region TimeActivityNoteID
		public abstract class timeActivityNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityNoteID> { }

		[PXDBSequentialGuid(BqlField = typeof(PMTimeActivity.noteID))]
		[PXExtraKey]
		public virtual Guid? TimeActivityNoteID { get; set; }
		#endregion

		#region EmailNoteID
		public abstract class emailNoteID : PX.Data.BQL.BqlGuid.Field<emailNoteID> { }

		[PXDBSequentialGuid(BqlField = typeof(SMEmail.noteID))]
		[PXExtraKey]
		public virtual Guid? EmailNoteID { get; set; }
		#endregion

		#region RefNoteID
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXDBGuid(BqlField = typeof(CRActivity.refNoteID))]
		public override Guid? RefNoteID { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.isBillable))]
		public virtual bool? IsBillable { get; set; }
		#endregion

		#region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, BqlField = typeof(PMTimeActivity.timeCardCD))]
		public virtual string TimeCardCD { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		
		[EPProject(typeof(ownerID), FieldClass = ProjectAttribute.DimensionName, BqlField = typeof(PMTimeActivity.projectID))]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		
		[EPTimecardProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task", BqlField = typeof(PMTimeActivity.projectTaskID))]		
		public virtual int? ProjectTaskID { get; set; }
		#endregion

		#region MPStatus
		public abstract class mpstatus : PX.Data.BQL.BqlString.Field<mpstatus> { }

		[PXDBString(2, IsFixed = true, IsUnicode = false, BqlField = typeof(SMEmail.mpstatus))]
		public virtual string MPStatus { get; set; }
		#endregion

		#region IsArchived
		public abstract class isArchived : PX.Data.BQL.BqlBool.Field<isArchived> { }

		[PXDBBool(BqlField = typeof(SMEmail.isArchived))]
		[PXDefault(false)]
		public virtual bool? IsArchived { get; set; }
		#endregion

		#region ID
		public abstract class id : PX.Data.BQL.BqlInt.Field<id> { }

		[PXDBIdentity(BqlField = typeof(SMEmail.id))]
		public virtual int? ID { get; set; }
		#endregion
	}

	/// <exclude/>
	[Serializable]
	[CRActivityPrimaryGraph]
	[PXCacheName(Messages.CRActivity)]
	[PXEMailSource]
	[Obsolete]
	[PXProjection(typeof(Select2<CRActivity,
		LeftJoin<SMEmail,
			On<SMEmail.refNoteID, Equal<CRActivity.noteID>>,
		LeftJoin<PMTimeActivity,
			On<PMTimeActivity.refNoteID, Equal<CRActivity.noteID>>>>>), Persistent = true)]
	public partial class EPActivity : IAssign, IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlGuid.Field<taskID> { }


		[PXGuid]
		[PXFormula(typeof(noteID))]
		public virtual Guid? TaskID { get; set; }
		#endregion

		#region CRActivity

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXSequentialNote(new Type[0], SuppressActivitiesCount = true, IsKey = true, BqlField = typeof(CRActivity.noteID))]
		[PXTimeTag(typeof(noteID))]
		[CRSMEmailStatisticFormulas]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region ParentNoteID
		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		[PXUIField(DisplayName = "Task")]
		[PXDBGuid(BqlField = typeof(CRActivity.parentNoteID))]
		[PXRestrictor(typeof(Where<classID, Equal<CRActivityClass.task>, Or<classID, Equal<CRActivityClass.events>>>), null)]
		public virtual Guid? ParentNoteID { get; set; }
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXDBGuid(BqlField = typeof(CRActivity.refNoteID))]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false)]
		[PXParent(typeof(Select<CRActivityStatistics, Where<CRActivityStatistics.noteID, Equal<Current<refNoteID>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXUIField(DisplayName = "References Nbr.")]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Related Entity Description", Enabled = false)]
		[PXFormula(typeof(EntityDescription<refNoteID>))]
		public virtual string Source { get; set; }
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }

		[PXDBInt(BqlField = typeof(CRActivity.classID))]
		[CRActivityClass]
		[PXDefault(typeof(CRActivityClass.activity))]
		public virtual int? ClassID { get; set; }
		#endregion

		#region ClassIcon
		public abstract class classIcon : PX.Data.BQL.BqlString.Field<classIcon> { }

		[PXUIField(DisplayName = "Class Icon", IsReadOnly = true)]
		[PXImage]
		[PXFormula(typeof(Switch<Case<Where<classID, Equal<CRActivityClass.task>>, CRActivity.classIcon.task,
			Case<Where<classID, Equal<CRActivityClass.events>>, CRActivity.classIcon.events,
			Case<Where<classID, Equal<CRActivityClass.email>, And<incoming, Equal<True>>>, CRActivity.classIcon.email,
			Case<Where<classID, Equal<CRActivityClass.email>, And<outgoing, Equal<True>>>, CRActivity.classIcon.emailResponse
			>>>>,
			Selector<Current2<type>, EPActivityType.imageUrl>>))]
		public virtual string ClassIcon { get; set; }
		#endregion

		#region ClassInfo
		public abstract class classInfo : PX.Data.BQL.BqlString.Field<classInfo>
		{
			public class emailResponse : PX.Data.BQL.BqlString.Constant<emailResponse>
			{
				public emailResponse() : base(Messages.EmailResponseClassInfo) { }
			}
		}

		[PXString]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<Current2<noteID>, IsNull>, StringEmpty,
			Case<Where<classID, Equal<CRActivityClass.activity>, And<type, IsNotNull>>, Selector<type, EPActivityType.description>,
			Case<Where<classID, Equal<CRActivityClass.email>, And<isIncome, Equal<True>>>, classInfo.emailResponse>>>,
			String<classID>>))]
		public virtual string ClassInfo { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXDBString(5, IsFixed = true, IsUnicode = false, BqlField = typeof(CRActivity.type))]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
		[PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), Messages.InactiveActivityType, typeof(EPActivityType.type))]
		[PXRestrictor(typeof(Where<EPActivityType.isInternal, Equal<True>, Or<EPActivityType.isSystem, Equal<True>>>), Messages.ExternalActivityType, typeof(EPActivityType.type))]
		[PXDefault(typeof(Search<EPActivityType.type,
			Where<Current<classID>, Equal<EPActivityType.classID>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Type { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true, BqlField = typeof(CRActivity.subject))]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(subject))]
		public virtual string Subject { get; set; }
		#endregion

		#region Location
		public abstract class location : PX.Data.BQL.BqlString.Field<location> { }

		[PXDBString(255, IsUnicode = true, InputMask = "", BqlField = typeof(CRActivity.location))]
		[PXUIField(DisplayName = "Location")]
		public virtual string Location { get; set; }
		#endregion

		#region Body
		//Empty for Email
		#endregion

		#region Priority
		public abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }

		[PXDBInt(BqlField = typeof(CRActivity.priority))]
		[PXUIField(DisplayName = "Priority")]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXIntList(
			new[] { 0, 1, 2 },
			new[] { Messages.Low, Messages.Normal, Messages.High })]
		public virtual int? Priority { get; set; }
		#endregion

		#region PriorityIcon
		public abstract class priorityIcon : PX.Data.BQL.BqlString.Field<priorityIcon> { }

		[PXUIField(DisplayName = "Priority Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.PriorityHead))]
		[PXFormula(typeof(Switch<
			Case<Where<priority, Equal<int0>>, CRActivity.priorityIcon.low,
			Case<Where<priority, Equal<int2>>, CRActivity.priorityIcon.high>>>))]
		public virtual string PriorityIcon { get; set; }
		#endregion

		#region UIStatus
		public abstract class uistatus : PX.Data.BQL.BqlString.Field<uistatus> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(CRActivity.uistatus))]
		[PXFormula(typeof(Switch<
			Case<Where<type, IsNull>, ActivityStatusAttribute.open,
			Case<Where<mpstatus, Equal<DoubleSpace>>, ActivityStatusAttribute.completed,
			Case<Where<mpstatus, Equal<MailStatusListAttribute.processed>>, ActivityStatusAttribute.completed,
			Case<Where<mpstatus, Equal<MailStatusListAttribute.deleted>,
							Or<mpstatus, Equal<MailStatusListAttribute.failed>,
							Or<mpstatus, Equal<MailStatusListAttribute.canceled>>>>, ActivityStatusAttribute.canceled>>>>,
			ActivityStatusAttribute.open>))]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string UIStatus { get; set; }
		#endregion

		#region IsOverdue
		public abstract class isOverdue : PX.Data.BQL.BqlBool.Field<isOverdue> { }

		[PXBool]
		public virtual bool? IsOverdue
		{
			[PXDependsOnFields(typeof(uistatus), typeof(endDate))]
			get
			{
				return
					UIStatus != ActivityStatusAttribute.Completed &&
					UIStatus != ActivityStatusAttribute.Canceled &&
					EndDate != null &&
					EndDate < PX.Common.PXTimeZoneInfo.Now;
			}
		}
		#endregion

		#region IsCompleteIcon
		public abstract class isCompleteIcon : PX.Data.BQL.BqlString.Field<isCompleteIcon> { }

		[PXUIField(DisplayName = "Complete Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.CompleteHead))]
		[PXFormula(typeof(Switch<Case<Where<uistatus, Equal<ActivityStatusListAttribute.completed>>, CRActivity.isCompleteIcon.completed>>))]
		public virtual String IsCompleteIcon { get; set; }
		#endregion

		#region CategoryID
		public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }

		[PXDBInt(BqlField = typeof(CRActivity.categoryID))]
		[PXSelector(typeof(EPEventCategory.categoryID), SubstituteKey = typeof(EPEventCategory.description))]
		[PXUIField(DisplayName = "Category")]
		public virtual int? CategoryID { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		[EPAllDaySupportDateTime(BqlField = typeof(CRActivity.startDate))]
		[PXFormula(typeof(TimeZoneNow))]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		[EPAllDaySupportDateTime(BqlField = typeof(CRActivity.endDate))]
		[PXUIField(DisplayName = "End Time")]
		public virtual DateTime? EndDate { get; set; }
		#endregion

		#region CompletedDate
		public abstract class completedDate : PX.Data.BQL.BqlDateTime.Field<completedDate> { }

		[PXDBDate(InputMask = "g", PreserveTime = true, BqlField = typeof(CRActivity.completedDate))]
		[PXUIField(DisplayName = "Completed At", Enabled = false)]
		public virtual DateTime? CompletedDate { get; set; }
		#endregion

		#region AllDay
		public abstract class allDay : PX.Data.BQL.BqlBool.Field<allDay> { }

		[EPAllDay(typeof(startDate), typeof(endDate), BqlField = typeof(CRActivity.allDay))]
		[PXUIField(DisplayName = "All Day")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? AllDay { get; set; }
		#endregion

		#region DayOfWeek
		public abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }

		[PXInt]
		[PXUIField(DisplayName = PX.Objects.EP.Messages.DayOfWeek)]
		[DayOfWeek]
		public virtual int? DayOfWeek
		{
			[PXDependsOnFields(typeof(startDate))]
			get
			{
				var date = StartDate;
				return date != null ? (int?)((int)date.Value.DayOfWeek) : null;
			}
		}
		#endregion

		#region PercentCompletion
		public abstract class percentCompletion : PX.Data.BQL.BqlInt.Field<percentCompletion> { }

		[PXDBInt(MinValue = 0, MaxValue = 100, BqlField = typeof(CRActivity.percentCompletion))]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Completion (%)")]
		public virtual int? PercentCompletion { get; set; }
		#endregion

		#region OwnerID
		public abstract class owner : PX.Data.BQL.BqlGuid.Field<owner> { }

		[PXDBGuid(BqlField = typeof(CRActivity.ownerID))]
		[PXChildUpdatable(AutoRefresh = true)]
		[PXOwnerSelector(typeof(workgroup))]
		[PXUIField(DisplayName = "Owner")]
		[PXDefault(typeof(Coalesce<
			Search<EPCompanyTreeMember.userID,
				Where<EPCompanyTreeMember.workGroupID, Equal<Current<workgroup>>,
					And<EPCompanyTreeMember.userID, Equal<Current<AccessInfo.userID>>>>>,
			Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<workgroup>, IsNull>>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<workgroup>))]
		public virtual Guid? Owner { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroup : PX.Data.BQL.BqlInt.Field<workgroup> { }

		[PXDBInt(BqlField = typeof(CRActivity.workgroupID))]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSubordinateGroupSelector]
		public virtual int? Workgroup { get; set; }
		#endregion

		#region IsExternal
		public abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }

		[PXDBBool(BqlField = typeof(CRActivity.isExternal))]
		[PXUIField(Visible = false)]
		public virtual bool? IsExternal { get; set; }
		#endregion

		#region IsPrivate
		public abstract class isPrivate : PX.Data.BQL.BqlBool.Field<isPrivate> { }

		[PXDBBool(BqlField = typeof(CRActivity.isPrivate))]
		[PXUIField(DisplayName = "Internal")]
		[PXFormula(typeof(IsNull<Selector<type, EPActivityType.privateByDefault>, False>))]
		public virtual bool? IsPrivate { get; set; }
		#endregion

		#region Incoming
		public abstract class incoming : PX.Data.BQL.BqlBool.Field<incoming> { }

		[PXDBBool(BqlField = typeof(CRActivity.incoming))]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.incoming>>, False>))]
		[PXUIField(DisplayName = "Incoming")]
		public virtual bool? Incoming
		{
			[PXDependsOnFields(typeof(isIncome))]
			get
			{
				return IsIncome;
			}
		}
		#endregion

		#region Outgoing
		public abstract class outgoing : PX.Data.BQL.BqlBool.Field<outgoing> { }

		[PXDBBool(BqlField = typeof(CRActivity.outgoing))]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.outgoing>>, False>))]
		[PXUIField(DisplayName = "Outgoing")]
		public virtual bool? Outgoing
		{
			[PXDependsOnFields(typeof(isIncome))]
			get
			{
				return !IsIncome;
			}
		}
		#endregion

		#region Synchronize
		public abstract class synchronize : PX.Data.BQL.BqlBool.Field<synchronize> { }

		[PXDBBool(BqlField = typeof(CRActivity.synchronize))]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Synchronize")]
		public virtual bool? Synchronize { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXDBInt(BqlField = typeof(CRActivity.bAccountID))]
		[PXUIField(DisplayName = "Task")]
		public virtual int? BAccountID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(BqlField = typeof(CRActivity.contactID))]
		[PXUIField(DisplayName = "Task")]
		public virtual int? ContactID { get; set; }
		#endregion

		#region EntityDescription

		public abstract class entityDescription : PX.Data.BQL.BqlString.Field<entityDescription> { }

		[PXString(InputMask = "")]
		[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual string EntityDescription { get; set; }
		#endregion

		#region ShowAsID
		public abstract class showAsID : PX.Data.BQL.BqlInt.Field<showAsID> { }

		[PXDBInt(BqlField = typeof(CRActivity.showAsID))]
		[PXUIField(DisplayName = "Show As")]
		[PXSelector(typeof(EPEventShowAs.showAsID), DescriptionField = typeof(EPEventShowAs.description))]
		public virtual int? ShowAsID { get; set; }
		#endregion



		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID(DontOverrideValue = true, BqlField = typeof(CRActivity.createdByID))]
		[PXUIField(Enabled = false)]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(CRActivity.createdByScreenID))]
		public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTime(BqlField = typeof(CRActivity.createdDateTime))]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(CRActivity.lastModifiedByID))]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(CRActivity.lastModifiedByScreenID))]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(CRActivity.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion



		public int? WorkgroupID
		{
			get { return Workgroup; }
			set { Workgroup = value; }
		}

		public Guid? OwnerID
		{
			get { return Owner; }
			set { Owner = value; }
		}

		#endregion

		#region SMEmail

		#region EmailNoteID
		public abstract class emailNoteID : PX.Data.BQL.BqlGuid.Field<emailNoteID> { }

		[PXDBSequentialGuid(BqlField = typeof(SMEmail.noteID))]
		[PXExtraKey]
		public virtual Guid? EmailNoteID { get; set; }
		#endregion

		#region EmailRefNoteID
		public abstract class emailRefNoteID : PX.Data.BQL.BqlGuid.Field<emailRefNoteID> { }

		[PXDBGuid(BqlField = typeof(SMEmail.refNoteID))]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false)]
		public virtual Guid? EmailRefNoteID
		{
			[PXDependsOnFields(typeof(noteID))]
			get
			{
				return NoteID;
			}
		}
		#endregion

		#region EmailSubject
		public abstract class emailSubject : PX.Data.BQL.BqlString.Field<emailSubject> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true, BqlField = typeof(SMEmail.subject))]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof(subject))]
		public virtual string EmailSubject
		{
			[PXDependsOnFields(typeof(subject))]
			get
			{
				return Subject;
			}
		}

		#endregion

		#region Body
		public abstract class body : PX.Data.BQL.BqlString.Field<body> { }

		[PXDBText(IsUnicode = true, BqlField = typeof(SMEmail.body))]
		[PXUIField(DisplayName = "Activity Details")]
		public virtual string Body { get; set; }
		#endregion

		#region MPStatus
		public abstract class mpstatus : PX.Data.BQL.BqlString.Field<mpstatus> { }

		[PXDBString(2, IsFixed = true, IsUnicode = false, BqlField = typeof(SMEmail.mpstatus))]
		[MailStatusList]
		[PXDefault(ActivityStatusAttribute.Draft)]
		[PXUIField(DisplayName = "Mail Status", Enabled = false)]
		public virtual string MPStatus { get; set; }
		#endregion

		#region IsArchived
		public abstract class isArchived : PX.Data.BQL.BqlBool.Field<isArchived> { }

		[PXDBBool(BqlField = typeof(SMEmail.isArchived))]
		[PXUIField(DisplayName = EP.Messages.EmailArchived, Enabled = false)]
		public virtual bool? IsArchived { get; set; }
		#endregion

		#region ImcUID
		public abstract class imcUID : PX.Data.BQL.BqlGuid.Field<imcUID> { }

		[PXDBGuid(true, BqlField = typeof(SMEmail.imcUID))]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual Guid? ImcUID
		{
			get { return _ImcUID ?? (_ImcUID = Guid.NewGuid()); }
			set { _ImcUID = value; }
		}
		protected Guid? _ImcUID;
		#endregion

		#region Pop3UID
		public abstract class pop3UID : PX.Data.BQL.BqlString.Field<pop3UID> { }

		[PXDBString(150, BqlField = typeof(SMEmail.pop3UID))]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual string Pop3UID { get; set; }
		#endregion

		#region ImapUID
		public abstract class imapUID : PX.Data.BQL.BqlInt.Field<imapUID> { }

		[PXDBInt(BqlField = typeof(SMEmail.imapUID))]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual int? ImapUID { get; set; }
		#endregion

		#region MailAccountID
		public abstract class mailAccountID : PX.Data.BQL.BqlInt.Field<mailAccountID> { }

		[PXDBInt(BqlField = typeof(SMEmail.mailAccountID))]
		[PXUIField(DisplayName = "From")]
		public int? MailAccountID { get; set; }

		#endregion

		#region MailFrom
		public abstract class mailFrom : PX.Data.BQL.BqlString.Field<mailFrom> { }

		[PXDBString(500, IsUnicode = true, BqlField = typeof(SMEmail.mailFrom))]
		[PXUIField(DisplayName = "From")]
		public virtual string MailFrom { get; set; }
		#endregion

		#region MailReply
		public abstract class mailReply : PX.Data.BQL.BqlString.Field<mailReply> { }

		[PXDBString(500, IsUnicode = true, BqlField = typeof(SMEmail.mailReply))]
		[PXUIField(DisplayName = "Reply")]
		public virtual string MailReply { get; set; }
		#endregion

		#region MailTo
		public abstract class mailTo : PX.Data.BQL.BqlString.Field<mailTo> { }

		[PXDBString(1000, IsUnicode = true, BqlField = typeof(SMEmail.mailTo))]
		[PXUIField(DisplayName = "To")]
		public virtual string MailTo { get; set; }
		#endregion

		#region MailCc
		public abstract class mailCc : PX.Data.BQL.BqlString.Field<mailCc> { }

		[PXDBString(1000, IsUnicode = true, BqlField = typeof(SMEmail.mailCc))]
		[PXUIField(DisplayName = "CC")]
		public virtual string MailCc { get; set; }
		#endregion

		#region MailBcc
		public abstract class mailBcc : PX.Data.BQL.BqlString.Field<mailBcc> { }

		[PXDBString(1000, IsUnicode = true, BqlField = typeof(SMEmail.mailBcc))]
		[PXUIField(DisplayName = "BCC")]
		public virtual string MailBcc { get; set; }
		#endregion

		#region RetryCount
		public abstract class retryCount : PX.Data.BQL.BqlInt.Field<retryCount> { }

		[PXDBInt(BqlField = typeof(SMEmail.retryCount))]
		[PXUIField(Visible = false)]
		[PXDefault(0)]
		public virtual int? RetryCount { get; set; }
		#endregion

		#region MessageId
		public abstract class messageId : PX.Data.BQL.BqlString.Field<messageId> { }

		[PXDBString(255, BqlField = typeof(SMEmail.messageId))] // TODO: need review length
		[PXUIField(Visible = false)]
		public virtual string MessageId { get; set; }
		#endregion

		#region MessageReference
		public abstract class messageReference : PX.Data.BQL.BqlString.Field<messageReference> { }

		[PXDBString(BqlField = typeof(SMEmail.messageReference))]
		[PXUIField(Visible = false)]
		public virtual string MessageReference { get; set; }
		#endregion

		#region Exception
		public abstract class exception : PX.Data.BQL.BqlString.Field<exception> { }

		[PXDBString(IsUnicode = true, BqlField = typeof(SMEmail.exception))]
		[PXUIField(DisplayName = "Error Message")]
		public virtual string Exception { get; set; }
		#endregion

		#region Format
		public abstract class format : PX.Data.BQL.BqlString.Field<format> { }

		[PXDefault(EmailFormatListAttribute.Html, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(255, BqlField = typeof(SMEmail.format))]
		[PXUIField(DisplayName = "Format")]
		[EmailFormatList]
		public virtual string Format { get; set; }
		#endregion

		#region ReportFormat
		public abstract class reportFormat : PX.Data.BQL.BqlString.Field<reportFormat> { }

		[PXDBString(10, BqlField = typeof(SMEmail.reportFormat))]
		[PXUIField(DisplayName = "Format")]
		[PXStringList(
			new[] { "PDF", "HTML", "Excel" },
			new[] { "PDF", "HTML", "Excel" })]
		public virtual string ReportFormat { get; set; }
		#endregion

		#region ID
		public abstract class emailID : PX.Data.BQL.BqlInt.Field<emailID> { }

		[PXDBInt(BqlField = typeof(SMEmail.id))]
		[PXUIField(Visible = false)]
		public virtual int? EmailID { get; set; }
		#endregion

		#region Ticket
		public abstract class ticket : PX.Data.BQL.BqlInt.Field<ticket> { }

		[PXDBInt(BqlField = typeof(SMEmail.ticket))]
		[PXUIField(Visible = false)]
		public virtual int? Ticket { get; set; }
		#endregion

		#region IsIncome
		public abstract class isIncome : PX.Data.BQL.BqlBool.Field<isIncome> { }

		[PXDBBool(BqlField = typeof(SMEmail.isIncome))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Is Income")]
		public virtual bool? IsIncome { get; set; }
		#endregion



		#region EmailCreatedByID
		public abstract class emailCreatedByID : PX.Data.BQL.BqlGuid.Field<emailCreatedByID> { }

		[PXDBCreatedByID(DontOverrideValue = true, BqlField = typeof(SMEmail.createdByID))]
		[PXUIField(Enabled = false)]
		public virtual Guid? EmailCreatedByID { get; set; }
		#endregion

		#region EmailCreatedByScreenID
		public abstract class emailCreatedByScreenID : PX.Data.BQL.BqlString.Field<emailCreatedByScreenID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(SMEmail.createdByScreenID))]
		public virtual string EmailCreatedByScreenID { get; set; }
		#endregion

		#region EmailCreatedDateTime
		public abstract class emailCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<emailCreatedDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTime(BqlField = typeof(SMEmail.createdDateTime))]
		public virtual DateTime? EmailCreatedDateTime { get; set; }
		#endregion

		#region EmailLastModifiedByID
		public abstract class emailLastModifiedByID : PX.Data.BQL.BqlGuid.Field<emailLastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(SMEmail.lastModifiedByID))]
		public virtual Guid? EmailLastModifiedByID { get; set; }
		#endregion

		#region EmailLastModifiedByScreenID
		public abstract class emailLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<emailLastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(SMEmail.lastModifiedByScreenID))]
		public virtual string EmailLastModifiedByScreenID { get; set; }
		#endregion

		#region EmailLastModifiedDateTime
		public abstract class emailLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<emailLastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(SMEmail.lastModifiedDateTime))]
		public virtual DateTime? EmailLastModifiedDateTime { get; set; }
		#endregion

		#endregion

		#region PMTimeActivity

		#region TimeActivityNoteID
		public abstract class timeActivityNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityNoteID> { }

		[PXDBSequentialGuid(BqlField = typeof(PMTimeActivity.noteID))]
		[PXExtraKey]
		public virtual Guid? TimeActivityNoteID { get; set; }
		#endregion

		#region TimeActivityRefNoteID
		public abstract class timeActivityRefNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityRefNoteID> { }

		[PXDBGuid(BqlField = typeof(PMTimeActivity.refNoteID))]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false)]
		public virtual Guid? TimeActivityRefNoteID
		{
			[PXDependsOnFields(typeof(noteID))]
			get
			{
				return NoteID;
			}
		}
		#endregion

		#region TrackTime
		public abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.trackTime))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Track Time")]
		[PXFormula(typeof(IIf<FeatureInstalled<FeaturesSet.timeReportingModule>, IsNull<Selector<type, EPActivityType.requireTimeByDefault>, False>, False>))]
		public virtual bool? TrackTime { get; set; }
		#endregion

		#region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, BqlField = typeof(PMTimeActivity.timeCardCD))]
		[PXUIField(Visible = false)]
		public virtual string TimeCardCD { get; set; }
		#endregion

		#region TimeSheetCD
		public abstract class timeSheetCD : PX.Data.BQL.BqlString.Field<timeSheetCD> { }

		[PXDBString(15, BqlField = typeof(PMTimeActivity.timeSheetCD))]
		[PXUIField(Visible = false)]
		public virtual string TimeSheetCD { get; set; }
		#endregion

		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }

		[PXDBString(255, InputMask = "", IsUnicode = true, BqlField = typeof(PMTimeActivity.summary))]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof(summary))]
		public virtual string Summary
		{
			[PXDependsOnFields(typeof(subject))]
			get
			{
				return Subject;
			}
		}
		#endregion

		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		[PXDBDate(BqlField = typeof(PMTimeActivity.date))]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? Date { get; set; }
		#endregion

		#region TimeActivityOwner
		public abstract class timeActivityOwner : PX.Data.BQL.BqlGuid.Field<timeActivityOwner> { }

		[PXDBGuid(BqlField = typeof(PMTimeActivity.ownerID))]
		[PXChildUpdatable(AutoRefresh = true)]
		[PXOwnerSelector(typeof(workgroup))]
		[PXUIField(DisplayName = "Owner")]
		[PXFormula(typeof(owner))]
		public virtual Guid? TimeActivityOwner { get; set; }
		#endregion
		
		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.approverID))]
		[PXEPEmployeeSelector]
		[PXFormula(typeof(
			Switch<
				Case<Where<Current2<projectID>, Equal<NonProject>>, Null,
				Case<Where<Current2<projectTaskID>, IsNull>, Null>>,
				Selector<projectTaskID, PMTask.approverID>>
			))]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? ApproverID { get; set; }
		#endregion

		#region ApprovalStatus
		public abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(PMTimeActivity.approvalStatus))]
		[ActivityStatusList]
		[PXUIField(DisplayName = "Approval Status", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<uistatus, Equal<ActivityStatusAttribute.canceled>>, ActivityStatusAttribute.canceled,
			Case<Where<released, Equal<True>>, ActivityStatusAttribute.released,
			Case<Where<uistatus, Equal<ActivityStatusAttribute.rejected>>, ActivityStatusAttribute.rejected,
			Case<Where<approverID, IsNotNull, And<uistatus, NotEqual<ActivityStatusAttribute.approved>>>, ActivityStatusAttribute.pendingApproval>>>>,
			ActivityStatusAttribute.completed>))]
		public virtual string ApprovalStatus { get; set; }

		#endregion

		#region ApprovedDate
		public abstract class approvedDate : PX.Data.BQL.BqlDateTime.Field<approvedDate> { }

		[PXDBDate(DisplayMask = "d", PreserveTime = true, BqlField = typeof(PMTimeActivity.approvedDate))]
		[PXUIField(DisplayName = "Approved Date")]
		public virtual DateTime? ApprovedDate { get; set; }
		#endregion

		#region EarningTypeID
		public abstract class earningTypeID : PX.Data.BQL.BqlString.Field<earningTypeID> { }

		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL", BqlField = typeof(PMTimeActivity.earningTypeID))]
		[PXDefault("RG", typeof(Search<EPSetup.regularHoursType>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(trackTime))]
		[PXRestrictor(typeof(Where<EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EPEarningType.typeCD))]
		[PXSelector(typeof(EPEarningType.typeCD), DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningTypeID { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.isBillable))]
		[PXUIField(DisplayName = "Billable", FieldClass = "BILLABLE")]
		[PXFormula(typeof(Switch<
			Case<Where<classID, Equal<CRActivityClass.task>, Or<classID, Equal<CRActivityClass.events>>>, False,
			Case<Where2<FeatureInstalled<FeaturesSet.timeReportingModule>, And<trackTime, Equal<True>>>,
				IsNull<Selector<earningTypeID, EPEarningType.isbillable>, False>>>,
			False>))]
		public virtual bool? IsBillable { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[EPActivityProjectDefault(typeof(isBillable))]
		[EPProject(typeof(owner), FieldClass = ProjectAttribute.DimensionName, BqlField = typeof(PMTimeActivity.projectID))]
		[PXFormula(typeof(Switch<
			Case<Where<Not<FeatureInstalled<FeaturesSet.projectModule>>>, DefaultValue<projectID>,
			Case<Where<parentNoteID, IsNotNull,
					And<Selector<parentNoteID, Selector<projectID, PMProject.contractCD>>, IsNotNull>>,
				Selector<parentNoteID, Selector<projectID, PMProject.contractCD>>,
			Case<Where<isBillable, Equal<True>, And<Current2<projectID>, Equal<NonProject>>>, Null,
			Case<Where<isBillable, Equal<False>, And<Current2<projectID>, IsNull>>, DefaultValue<projectID>>>>>,
			projectID>))]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		
		[EPTimecardProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task", BqlField = typeof(PMTimeActivity.projectTaskID))]
		[PXFormula(typeof(Switch<
			Case<Where<Current2<projectID>, Equal<NonProject>>, Null,
			Case<Where<parentNoteID, IsNotNull>,
				Selector<parentNoteID, Selector<projectTaskID, PMTask.taskCD>>>>,
			projectTaskID>))]
		public virtual int? ProjectTaskID { get; set; }
		#endregion

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.contractID))]
		[PXUIField(DisplayName = "Contract", Visible = false)]
		[PXSelector(typeof(Search2<Contract.contractID,
				LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>>,
			OrderBy<Desc<Contract.contractCD>>>),
			DescriptionField = typeof(Contract.description),
			SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
		[PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>>), Messages.ContractIsNotActive)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]
		public virtual int? ContractID { get; set; }
		#endregion

		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.timeSpent))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Time Spent")]
		[PXFormula(typeof(Switch<Case<Where<trackTime, NotEqual<True>>, int0>, timeSpent>))]
		public virtual int? TimeSpent { get; set; }
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.overtimeSpent))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<Selector<earningTypeID, EPEarningType.isOvertime>, Equal<True>>, timeSpent>, int0>))]
		[PXUIField(DisplayName = "Overtime", Enabled = false)]
		public virtual int? OvertimeSpent { get; set; }
		#endregion

		#region TimeBillable
		public abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.timeBillable))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(isBillable))]
		[PXFormula(typeof(
			Switch<Case<Where<isBillable, Equal<True>>, timeSpent,
				Case<Where<isBillable, Equal<False>>, int0>>,
			timeBillable>))]
		[PXUIField(DisplayName = "Billable Time", FieldClass = "BILLABLE")]
		[PXUIVerify(typeof(Where<timeSpent, IsNull,
			Or<timeBillable, IsNull,
			Or<timeSpent, GreaterEqual<timeBillable>>>>), PXErrorLevel.Error, Messages.BillableTimeCannotBeGreaterThanTimeSpent)]
		[PXUIVerify(typeof(Where<isBillable, NotEqual<True>,
			Or<timeBillable, NotEqual<int0>>>), PXErrorLevel.Error, Messages.BillableTimeMustBeOtherThanZero)]
		public virtual int? TimeBillable { get; set; }
		#endregion

		#region OvertimeBillable
		public abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.overtimeBillable))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVerify(typeof(Where<overtimeSpent, IsNull,
			Or<overtimeBillable, IsNull,
				Or<overtimeSpent, GreaterEqual<overtimeBillable>>>>), PXErrorLevel.Error, Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent)]
		[PXFormula(typeof(
			Switch<Case<Where<isBillable, Equal<True>, And<overtimeSpent, GreaterEqual<timeBillable>>>, timeBillable,
				Case<Where<isBillable, Equal<True>, And<overtimeSpent, GreaterEqual<Zero>>>, overtimeBillable,
				Case<Where<isBillable, Equal<False>>, int0>>>,
			overtimeBillable>))]
		[PXUIField(DisplayName = "Billable Overtime", FieldClass = "BILLABLE")]
		public virtual int? OvertimeBillable { get; set; }
		#endregion

		#region Billed
		public abstract class billed : PX.Data.BQL.BqlBool.Field<billed> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.billed))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Billed", FieldClass = "BILLABLE")]
		public virtual bool? Billed { get; set; }
		#endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.released))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released", Enabled = false, Visible = false, FieldClass = "BILLABLE")]
		public virtual bool? Released { get; set; }
		#endregion

		#region IsCorrected
		public abstract class isCorrected : PX.Data.BQL.BqlBool.Field<isCorrected> { }

		/// <summary>
		/// If true this Activity has been corrected in the Timecard and is no longer valid. Please hide this activity in all lists displayed in the UI since there is another valid activity.
		/// The valid activity has a refence back to the corrected activity via OrigTaskID field. 
		/// </summary>
		[PXDBBool(BqlField = typeof(PMTimeActivity.isCorrected))]
		[PXDefault(false)]
		public virtual bool? IsCorrected { get; set; }
		#endregion

		#region OrigNoteID
		public abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }

		/// <summary>
		/// Use for correction. Stores the reference to the original activity.
		/// </summary>
		[PXDBGuid(BqlField = typeof(PMTimeActivity.origNoteID))]
		public virtual Guid? OrigNoteID { get; set; }
		#endregion

		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }

		[PXDBLong(BqlField = typeof(PMTimeActivity.tranID))]
		public virtual long? TranID { get; set; }
		#endregion

		#region WeekID
		public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.weekID))]
		[PXUIField(DisplayName = "Time Card Week", Enabled = false)]
		[PXWeekSelector2()]
		[PXFormula(typeof(Default<date, trackTime>))]
		public virtual int? WeekID { get; set; }
		#endregion

		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.labourItemID))]
		[PXUIField(Visible = false)]
		public virtual int? LabourItemID { get; set; }
		#endregion

		#region OvertimeItemID
		public abstract class overtimeItemID : PX.Data.BQL.BqlInt.Field<overtimeItemID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.overtimeItemID))]
		[PXUIField(Visible = false)]
		public virtual int? OvertimeItemID { get; set; }
		#endregion

		#region JobID
		public abstract class jobID : PX.Data.BQL.BqlInt.Field<jobID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.jobID))]
		public virtual int? JobID { get; set; }
		#endregion

		#region ShiftID
		public abstract class shiftID : PX.Data.BQL.BqlInt.Field<shiftID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.shiftID))]
		public virtual int? ShiftID { get; set; }
		#endregion

		#region EmployeeRate
		public abstract class employeeRate : PX.Data.BQL.BqlDecimal.Field<employeeRate> { }

		/// <summary>
		/// Stores Employee's Hourly rate at the time the activity was released to PM
		/// </summary>
		[IN.PXDBPriceCost(BqlField = typeof(PMTimeActivity.employeeRate))]
		[PXUIField(Visible = false)]
		public virtual decimal? EmployeeRate { get; set; }
		#endregion

		#region SummaryLineNbr
		public abstract class summaryLineNbr : PX.Data.BQL.BqlInt.Field<summaryLineNbr> { }

		/// <summary>
		/// This is a adjusting activity for the summary line in the Timecard.
		/// </summary>
		[PXDBInt(BqlField = typeof(PMTimeActivity.summaryLineNbr))]
		public virtual int? SummaryLineNbr { get; set; }
		#endregion

		#region ARDocType
		public abstract class arDocType : PX.Data.BQL.BqlString.Field<arDocType> { }
		[AR.ARDocType.List()]
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String ARDocType { get; set; }
		#endregion

		#region ARDocType
		public abstract class arRefNbr : PX.Data.BQL.BqlString.Field<arRefNbr> { }
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AR.ARRegister.refNbr, Where<AR.ARRegister.docType, Equal<Current<arDocType>>>>), DescriptionField = typeof(AR.ARRegister.docType))]
		public virtual string ARRefNbr { get; set; }
		#endregion



		#region TimeActivityCreatedByID
		public abstract class timeActivityCreatedByID : PX.Data.BQL.BqlGuid.Field<timeActivityCreatedByID> { }

		[PXDBCreatedByID(DontOverrideValue = true, BqlField = typeof(PMTimeActivity.createdByID))]
		[PXUIField(Enabled = false)]
		public virtual Guid? TimeActivityCreatedByID { get; set; }
		#endregion

		#region TimeActivityCreatedByScreenID
		public abstract class timeActivityCreatedByScreenID : PX.Data.BQL.BqlString.Field<timeActivityCreatedByScreenID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(PMTimeActivity.createdByScreenID))]
		public virtual string TimeActivityCreatedByScreenID { get; set; }
		#endregion

		#region TimeActivityCreatedDateTime
		public abstract class timeActivityCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<timeActivityCreatedDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTime(BqlField = typeof(PMTimeActivity.createdDateTime))]
		public virtual DateTime? TimeActivityCreatedDateTime { get; set; }
		#endregion

		#region TimeActivityLastModifiedByID
		public abstract class timeActivityLastModifiedByID : PX.Data.BQL.BqlGuid.Field<timeActivityLastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(PMTimeActivity.lastModifiedByID))]
		public virtual Guid? TimeActivityLastModifiedByID { get; set; }
		#endregion

		#region TimeActivityLastModifiedByScreenID
		public abstract class timeActivityLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<timeActivityLastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(PMTimeActivity.lastModifiedByScreenID))]
		public virtual string TimeActivityLastModifiedByScreenID { get; set; }
		#endregion

		#region TimeActivityLastModifiedDateTime
		public abstract class timeActivityLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<timeActivityLastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(PMTimeActivity.lastModifiedDateTime))]
		public virtual DateTime? TimeActivityLastModifiedDateTime { get; set; }
		#endregion

		#endregion
	}

}
