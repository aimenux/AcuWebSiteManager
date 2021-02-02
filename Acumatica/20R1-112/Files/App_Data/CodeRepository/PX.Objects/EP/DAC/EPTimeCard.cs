using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.TM;

namespace PX.Objects.EP
{
	[PXPrimaryGraph(typeof(TimeCardMaint))]
	[PXCacheName(Messages.TimeCard)]
	[Serializable]
	[PXEMailSource]
	public partial class EPTimeCard : IBqlTable, IAssign
	{

		#region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(EPSetup.timeCardNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search2<EPTimeCard.timeCardCD,
			InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPTimeCard.employeeID>>>,
			Where<EPTimeCard.createdByID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
						 Or<EPTimeCard.noteID, Approver<Current<AccessInfo.userID>>,
						 Or<EPTimeCard.employeeID, WingmanUser<Current<AccessInfo.userID>>>>>>>>),
			typeof(EPTimeCard.timeCardCD),
			typeof(EPTimeCard.employeeID),
			typeof(EPTimeCard.employeeID_CREmployee_acctName),
			typeof(EPTimeCard.weekDescription),
			typeof(EPTimeCard.status))]
		[PXFieldDescription]
		public virtual String TimeCardCD { get; set; }
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

		[PXDBInt]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
		[PXUIField(DisplayName = "Employee")]
		[PXSubordinateAndWingmenSelector]
		[PXFieldDescription]
		public virtual Int32? EmployeeID { get; set; }
		#endregion
		#region EmployeeID_CREmployee_acctName
		public abstract class employeeID_CREmployee_acctName : PX.Data.BQL.BqlString.Field<employeeID_CREmployee_acctName> { }
		#endregion
		#region Status

		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }


		[PXDBString(1)]
		[PXDefault("H")]
		[EPTimeCardStatus]
		[PXUIField(DisplayName = "Status", Enabled=false)]
		public virtual String Status { get; set; }

		#endregion
		#region WeekID

		public abstract class weekId : PX.Data.BQL.BqlInt.Field<weekId> { }

		protected Int32? _WeekID;
		[PXDBInt]
		[PXUIField(DisplayName = "Week")]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		public virtual Int32? WeekID
		{
			get
			{
				return this._WeekID;
			}
			set
			{
				this._WeekID = value;
			}
		}

		#endregion
		#region OrigTimeCardCD
		public abstract class origTimeCardCD : PX.Data.BQL.BqlString.Field<origTimeCardCD> { }
		[PXSelector(typeof(Search<EPTimeCard.timeCardCD>))]
		[PXUIField(DisplayName = "Orig. Ref. Nbr.", Enabled = false)]
		[PXDBString(10, IsUnicode = true)]
		public virtual String OrigTimeCardCD { get; set; }
		#endregion
		#region IsApproved

		public abstract class isApproved : PX.Data.BQL.BqlBool.Field<isApproved> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsApproved { get; set; }

		#endregion
		#region IsRejected

		public abstract class isRejected : PX.Data.BQL.BqlBool.Field<isRejected> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsRejected { get; set; }

		#endregion
		#region IsHold

		public abstract class isHold : PX.Data.BQL.BqlBool.Field<isHold> { }

		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsHold { get; set; }

		#endregion
		#region IsReleased

		public abstract class isReleased : PX.Data.BQL.BqlBool.Field<isReleased> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsReleased { get; set; }

		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXInt]
		[PXUIField(DisplayName = "Workgroup ID", Visible = false)]
		[PXSelector(typeof(EPCompanyTreeOwner.workGroupID), SubstituteKey = typeof(EPCompanyTreeOwner.description))]
		public virtual int? WorkgroupID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXGuid]
		[PXUIField(Visible = false)]
		public virtual Guid? OwnerID { get; set; }
		#endregion
		#region SummaryLineCntr
		public abstract class summaryLineCntr : PX.Data.BQL.BqlInt.Field<summaryLineCntr> { }
		protected int? _SummaryLineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? SummaryLineCntr
		{
			get
			{
				return this._SummaryLineCntr;
			}
			set
			{
				this._SummaryLineCntr = value;
			}
		}
		#endregion
		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual int? TimeSpent { get; set; }
		#endregion
		#region OvertimeSpent
		public abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }

		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Overtime", Enabled = false)]
		public virtual int? OvertimeSpent { get; set; }
		#endregion
		#region TimeBillable
		public abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }

		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Time Billable", Enabled = false)]
		public virtual int? TimeBillable { get; set; }
		#endregion
		#region OvertimeBillable
		public abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }

		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Billable Overtime", Enabled = false)]
		public virtual int? OvertimeBillable { get; set; }
		#endregion
		#region TotalTimeSpent
		public abstract class totalTimeSpent : PX.Data.BQL.BqlInt.Field<totalTimeSpent> { }

		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Time Spent", Enabled = false)]
		public virtual int? TotalTimeSpent
		{
			[PXDependsOnFields(typeof(timeSpent), typeof(overtimeSpent))]
			get
			{
				return TimeSpent + OvertimeSpent;
			}
		}
		#endregion
		#region TotalTimeBillable
		public abstract class totalTimeBillable : PX.Data.BQL.BqlInt.Field<totalTimeBillable> { }

		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Time Billable", Enabled = false)]
		public virtual int? TotalTimeBillable
		{
			[PXDependsOnFields(typeof(timeBillable), typeof(overtimeBillable))]
			get
			{
				return TimeBillable + OvertimeBillable;
			}
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(typeof(EPTimeCard),
			DescriptionField = typeof(EPTimeCard.timeCardCD),
			Selector = typeof(EPTimeCard.timeCardCD)
			)]
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion


		#region Unbound Fields (Calculated in the TimecardMaint graph)

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region WeekStartDate (Used in Report)

		public abstract class weekStartDate : PX.Data.BQL.BqlDateTime.Field<weekStartDate> { }

		protected DateTime? _WeekStartDate;
		[TimecardWeekStartDate(typeof(weekId))]
		[PXUIField(DisplayName = "Week Start Date")]
		public virtual DateTime? WeekStartDate {
			get { return _WeekStartDate; }
			set { _WeekStartDate = value; }
		}

		#endregion

		public abstract class weekDescription : PX.Data.BQL.BqlString.Field<weekDescription> { }
		[PXString]
		[PXUIField(DisplayName = "Week")]
		[PXFormula(typeof(Selector<EPTimeCard.weekId, EPWeekRaw.description>))]
		public virtual String WeekDescription { get; set; }

		public abstract class weekShortDescription : PX.Data.BQL.BqlString.Field<weekShortDescription> { }
		[PXString]
		[PXUIField(DisplayName = "Week")]
		[PXFieldDescription]
		[PXFormula(typeof(Selector<EPTimeCard.weekId, EPWeekRaw.shortDescription>))]
		public virtual String WeekShortDescription { get; set; }

		public abstract class timeSpentCalc : PX.Data.BQL.BqlInt.Field<timeSpentCalc> { }
		[PXInt]
        [PXTimeList(30, 335, ExclusiveValues = false)]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual Int32? TimeSpentCalc { get; set; }

		public abstract class overtimeSpentCalc : PX.Data.BQL.BqlInt.Field<overtimeSpentCalc> { }
		[PXInt]
		[PXTimeList]
		[PXUIField(DisplayName = "Overtime Spent", Enabled = false)]
		public virtual Int32? OvertimeSpentCalc { get; set; }

		public abstract class totalSpentCalc : PX.Data.BQL.BqlInt.Field<totalSpentCalc> { }
		[PXInt]
		[PXTimeList(30, 335, ExclusiveValues = false)]
		[PXUIField(DisplayName = "Total Time Spent", Enabled = false)]
		public virtual Int32? TotalSpentCalc { get; set; }

		public abstract class timeBillableCalc : PX.Data.BQL.BqlInt.Field<timeBillableCalc> { }
		[PXInt]
		[PXTimeList]
		[PXUIField(DisplayName = "Billable", Enabled = false)]
		public virtual Int32? TimeBillableCalc { get; set; }

		public abstract class overtimeBillableCalc : PX.Data.BQL.BqlInt.Field<overtimeBillableCalc> { }
		[PXInt]
		[PXTimeList]
		[PXUIField(DisplayName = "Billable Overtime", Enabled = false)]
		public virtual Int32? OvertimeBillableCalc { get; set; }

		public abstract class totalBillableCalc : PX.Data.BQL.BqlInt.Field<totalBillableCalc> { }
		[PXInt]
		[PXTimeList]
		[PXUIField(DisplayName = "Total Billable", Enabled = false)]
		public virtual Int32? TotalBillableCalc { get; set; }

		public abstract class timecardType : PX.Data.BQL.BqlString.Field<timecardType> {}		

		[EPTimecardType]
		[PXStringList(new string[] { EPTimecardTypeAttribute.Normal, EPTimecardTypeAttribute.Correction, EPTimecardTypeAttribute.NormalCorrected }, new string[] {Messages.TimecardTypeNormal, Messages.TimecardTypeCorrection, Messages.TimecardTypeNormalCorrected })]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		public virtual string TimecardType
		{
			get;
			set;

		}

		public abstract class billingRateCalc : PX.Data.BQL.BqlInt.Field<billingRateCalc> { }
		[PXInt]
		[PXUIField(DisplayName = "Billing Ratio", Enabled = false)]
		public virtual Int32? BillingRateCalc
		{
			get
			{
				if (TotalSpentCalc != 0)
				{
					return TotalBillableCalc * 100 / TotalSpentCalc;
				}
				else
				{
					return 0;
				}
			}
		}

		#endregion
	}



	#region Projections

	[PXProjection(typeof(Select5<PMTimeActivity,
		InnerJoin<EPEarningType, 
			On<EPEarningType.typeCD, Equal<PMTimeActivity.earningTypeID>>>,
		Where<PMTimeActivity.timeCardCD, IsNull, And<EPEarningType.isOvertime, Equal<False>,
			And<PMTimeActivity.trackTime, Equal<True>>>>,
		Aggregate<
			GroupBy<PMTimeActivity.weekID,
			GroupBy<PMTimeActivity.ownerID,
			Sum<PMTimeActivity.timeSpent,
			Sum<PMTimeActivity.timeBillable>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class TimecardRegularCurrentTotals : IBqlTable
	{
		#region WeekID
		public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }
		
		[PXDBInt(BqlField = typeof(PMTimeActivity.weekID))]
		public virtual int? WeekID { get; set; }
		#endregion

		#region Owner
		public abstract class owner : PX.Data.BQL.BqlGuid.Field<owner> { }
		
		[PXDBGuid(BqlField = typeof(PMTimeActivity.ownerID))]
		public virtual Guid? Owner { get; set; }
		#endregion

		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.timeSpent))]
		public virtual int? TimeSpent { get; set; }
		#endregion

		#region TimeBillable
		public abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }
		
		[PXDBInt(BqlField = typeof(PMTimeActivity.timeBillable))]
		public virtual int? TimeBillable { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select5<PMTimeActivity,
		InnerJoin<EPEarningType, 
			On<EPEarningType.typeCD, Equal<PMTimeActivity.earningTypeID>>>,
		Where<PMTimeActivity.timeCardCD, IsNull, And<EPEarningType.isOvertime, Equal<True>,
			And<PMTimeActivity.trackTime, Equal<True>>>>,
		Aggregate<
			GroupBy<PMTimeActivity.weekID,
			GroupBy<PMTimeActivity.ownerID,
			Sum<PMTimeActivity.timeSpent,
			Sum<PMTimeActivity.timeBillable>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class TimecardOvertimeCurrentTotals : IBqlTable
	{
		#region WeekID
		public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }
		
		[PXDBInt(BqlField = typeof(PMTimeActivity.weekID))]
		public virtual int? WeekID { get; set; }
		#endregion

		#region Owner
		public abstract class owner : PX.Data.BQL.BqlGuid.Field<owner> { }
		
		[PXDBGuid(BqlField = typeof(PMTimeActivity.ownerID))]
		public virtual Guid? Owner { get; set; }
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.timeSpent))]
		public virtual int? OvertimeSpent { get; set; }
		#endregion

		#region OvertimeBillable
		public abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }
		
		[PXDBInt(BqlField = typeof(PMTimeActivity.timeBillable))]
		public virtual int? OvertimeBillable { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select2<EPTimeCard,
					InnerJoin<EPEmployeeEx, On<EPEmployeeEx.bAccountID, Equal<EPTimeCard.employeeID>>,
					LeftJoin<TimecardRegularCurrentTotals, On<TimecardRegularCurrentTotals.weekID, Equal<EPTimeCard.weekId>, And<EPTimeCard.isHold, Equal<True>, And<EPEmployeeEx.userID, Equal<TimecardRegularCurrentTotals.owner>>>>,
					LeftJoin<TimecardOvertimeCurrentTotals, On<TimecardOvertimeCurrentTotals.weekID, Equal<EPTimeCard.weekId>, And<EPTimeCard.isHold, Equal<True>, And<EPEmployeeEx.userID, Equal<TimecardOvertimeCurrentTotals.owner>>>>>>>>))]
	[Serializable]
	public partial class TimecardWithTotals : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC", BqlField = typeof(EPTimeCard.timeCardCD))]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search2<EPTimeCard.timeCardCD,
			InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPTimeCard.employeeID>>>,
			Where<EPTimeCard.createdByID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
						 Or<EPTimeCard.noteID, Approver<Current<AccessInfo.userID>>>>>>>),
			typeof(EPTimeCard.timeCardCD),
			typeof(EPTimeCard.employeeID),
			typeof(EPTimeCard.weekDescription),
			typeof(EPTimeCard.status))]
		[PXFieldDescription]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual String TimeCardCD { get; set; }
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

		[PXDBInt(BqlField = typeof(EPTimeCard.employeeID))]
		[PXUIField(DisplayName = "Employee")]
		[PXSubordinateAndWingmenSelector]
		[PXFieldDescription]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? EmployeeID { get; set; }
		#endregion
		#region Status

		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, BqlField = typeof(EPTimeCard.status))]
		[EPTimeCardStatus]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual String Status { get; set; }

		#endregion
		#region WeekID

		public abstract class weekId : PX.Data.BQL.BqlInt.Field<weekId> { }

		protected Int32? _WeekID;
		[PXDBInt(BqlField = typeof(EPTimeCard.weekId))]
		[PXUIField(DisplayName = "Week")]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? WeekID
		{
			get
			{
				return this._WeekID;
			}
			set
			{
				this._WeekID = value;
			}
		}

		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(typeof(EPTimeCard), BqlField = typeof(EPTimeCard.noteID),
			DescriptionField = typeof(EPTimeCard.timeCardCD),
			Selector = typeof(EPTimeCard.timeCardCD)
			)]
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
		#region IsApproved

		public abstract class isApproved : PX.Data.BQL.BqlBool.Field<isApproved> { }

		[PXDBBool(BqlField = typeof(EPTimeCard.isApproved))]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsApproved { get; set; }

		#endregion
		#region IsRejected

		public abstract class isRejected : PX.Data.BQL.BqlBool.Field<isRejected> { }

		[PXDBBool(BqlField = typeof(EPTimeCard.isRejected))]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsRejected { get; set; }

		#endregion
		#region IsHold

		public abstract class isHold : PX.Data.BQL.BqlBool.Field<isHold> { }

		[PXDBBool(BqlField = typeof(EPTimeCard.isHold))]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsHold { get; set; }

		#endregion
		#region IsReleased

		public abstract class isReleased : PX.Data.BQL.BqlBool.Field<isReleased> { }

		[PXDBBool(BqlField = typeof(EPTimeCard.isReleased))]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsReleased { get; set; }

		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField = typeof(EPTimeCard.createdByID))]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion

		#region TimeSpentFinal
		public abstract class timeSpentFinal : PX.Data.BQL.BqlInt.Field<timeSpentFinal> { }
		[PXDBInt(BqlField = typeof(EPTimeCard.timeSpent))]
		public virtual Int32? TimeSpentFinal
		{
			get;
			set;
		}
		#endregion
		#region TimeBillableFinal
		public abstract class timeBillableFinal : PX.Data.BQL.BqlInt.Field<timeBillableFinal> { }
		[PXDBInt(BqlField = typeof(EPTimeCard.timeBillable))]
		public virtual Int32? TimeBillableFinal
		{
			get;
			set;
		}
		#endregion
		#region OvertimeSpentFinal
		public abstract class overtimeSpentFinal : PX.Data.BQL.BqlInt.Field<overtimeSpentFinal> { }
		[PXDBInt(BqlField = typeof(EPTimeCard.overtimeSpent))]
		public virtual Int32? OvertimeSpentFinal
		{
			get;
			set;
		}
		#endregion
		#region OvertimeBillableFinal
		public abstract class overtimeBillableFinal : PX.Data.BQL.BqlInt.Field<overtimeBillableFinal> { }
		[PXDBInt(BqlField = typeof(EPTimeCard.overtimeBillable))]
		public virtual Int32? OvertimeBillableFinal
		{
			get;
			set;
		}
		#endregion
		#region TimeSpentCurrent
		public abstract class timeSpentCurrent : PX.Data.BQL.BqlInt.Field<timeSpentCurrent> { }
		[PXDBInt(BqlField = typeof(TimecardRegularCurrentTotals.timeSpent))]
		public virtual Int32? TimeSpentCurrent
		{
			get;
			set;
		}
		#endregion
		#region TimeBillableCurrent
		public abstract class timeBillableCurrent : PX.Data.BQL.BqlInt.Field<timeBillableCurrent> { }
		[PXDBInt(BqlField = typeof(TimecardRegularCurrentTotals.timeBillable))]
		public virtual Int32? TimeBillableCurrent
		{
			get;
			set;
		}
		#endregion
		#region OvertimeSpentCurrent
		public abstract class overtimeSpentCurrent : PX.Data.BQL.BqlInt.Field<overtimeSpentCurrent> { }
		[PXDBInt(BqlField = typeof(TimecardOvertimeCurrentTotals.overtimeSpent))]
		public virtual Int32? OvertimeSpentCurrent
		{
			get;
			set;
		}
		#endregion
		#region OvertimeBillableCurrent
		public abstract class overtimeBillableCurrent : PX.Data.BQL.BqlInt.Field<overtimeBillableCurrent> { }
		[PXDBInt(BqlField = typeof(TimecardOvertimeCurrentTotals.overtimeBillable))]
		public virtual Int32? OvertimeBillableCurrent
		{
			get;
			set;
		}
		#endregion

		#region EPEmployee.UserID
        /// <summary>
        /// userID
        /// </summary>
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		/// <summary>
		/// Gets sets Employee UserID
		/// </summary>
		[PXDBGuid(BqlField = typeof(EPEmployeeEx.userID))]
		[PXUIField(DisplayName = "Employee Login", Visibility = PXUIVisibility.Visible)]
		public virtual Guid? UserID { get; set; }
		#endregion

		public abstract class timeSpentCalc : PX.Data.BQL.BqlInt.Field<timeSpentCalc> { }
		[PXInt]
		[PXTimeList]
		[PXDependsOnFields(typeof(timeSpentFinal), typeof(timeSpentCurrent))]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? TimeSpentCalc
		{
			get { return TimeSpentFinal.GetValueOrDefault() + TimeSpentCurrent.GetValueOrDefault(); }
		}

		public abstract class overtimeSpentCalc : PX.Data.BQL.BqlInt.Field<overtimeSpentCalc> { }

		[PXInt]
		[PXTimeList]
		[PXDependsOnFields(typeof(overtimeSpentFinal), typeof(overtimeSpentCurrent))]
		[PXUIField(DisplayName = "Overtime Spent", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? OvertimeSpentCalc
		{
			get
			{
				int val = OvertimeSpentFinal.GetValueOrDefault() + OvertimeSpentCurrent.GetValueOrDefault();

				if (val == 0)
					return null;
				else
					return val;
			}
		}

		public abstract class totalSpentCalc : PX.Data.BQL.BqlInt.Field<totalSpentCalc> { }

		[PXInt]
		[PXTimeList]
		[PXDependsOnFields(typeof(timeSpentCalc), typeof(overtimeSpentCalc))]
		[PXUIField(DisplayName = "Total Time Spent", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? TotalSpentCalc
		{
			get { return TimeSpentCalc + OvertimeSpentCalc.GetValueOrDefault(); }
		}

		public abstract class timeBillableCalc : PX.Data.BQL.BqlInt.Field<timeBillableCalc> { }

		[PXInt]
		[PXTimeList]
		[PXDependsOnFields(typeof(timeBillableFinal), typeof(timeBillableCurrent))]
		[PXUIField(DisplayName = "Billable", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? TimeBillableCalc
		{
			get
			{
				int val = TimeBillableFinal.GetValueOrDefault() + TimeBillableCurrent.GetValueOrDefault();

				if (val == 0)
					return null;
				else
					return val;
			}
		}

		public abstract class overtimeBillableCalc : PX.Data.BQL.BqlInt.Field<overtimeBillableCalc> { }
		[PXInt]
		[PXTimeList]
		[PXDependsOnFields(typeof(overtimeBillableFinal), typeof(overtimeBillableCurrent))]
		[PXUIField(DisplayName = "Billable Overtime", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? OvertimeBillableCalc
		{
			get
			{
				int val = OvertimeBillableFinal.GetValueOrDefault() + OvertimeBillableCurrent.GetValueOrDefault();
				if (val == 0)
					return null;
				else
					return val;
			}
		}

		public abstract class totalBillableCalc : PX.Data.BQL.BqlInt.Field<totalBillableCalc> { }
		[PXInt]
		[PXTimeList]
		[PXDependsOnFields(typeof(timeBillableCalc), typeof(overtimeBillableCalc))]
		[PXUIField(DisplayName = "Total Billable", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? TotalBillableCalc
		{
			get
			{
				int val = TimeBillableCalc.GetValueOrDefault() + OvertimeBillableCalc.GetValueOrDefault();
				if (val == 0)
					return null;
				else
					return val;
			}
		}


		public abstract class billingRateCalc : PX.Data.BQL.BqlInt.Field<billingRateCalc> { }
		[PXInt]
		[PXDependsOnFields(typeof(totalBillableCalc), typeof(totalSpentCalc))]
		[PXUIField(DisplayName = "Billing Ratio", Enabled = false)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		public virtual Int32? BillingRateCalc
		{
			get
			{
				if (TotalSpentCalc != 0)
				{
					return TotalBillableCalc * 100 / TotalSpentCalc;
				}
				else
				{
					return 0;
				}
			}
		}

		#region WeekStartDate (Used in Report)

		public abstract class weekStartDate : PX.Data.BQL.BqlDateTime.Field<weekStartDate> { }

		protected DateTime? _WeekStartDate;
		[TimecardWeekStartDate(typeof(weekId))]
		[PXUIField(DisplayName = "Week Start Date", Visible = false)]
		public virtual DateTime? WeekStartDate
		{
			get;
			set;
		}

		#endregion
	}

	public class EPEmployeeEx : EPEmployee
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
	}

	/// <summary>
	/// Timecard (Lite version)
	/// This is a projection DAC
	/// </summary>
	[PXHidden]
	[PXProjection(typeof(Select<EPTimeCard>), Persistent = false)]
	public class EPTimecardLite : IBqlTable
	{
        #region TimeCardCD
        /// <summary>
        /// timeCardCD Bql field
        /// </summary>
        public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		/// <summary>
		/// Gets sets Timecard Number
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC", BqlField = typeof(EPTimeCard.timeCardCD))]
		public virtual String TimeCardCD { get; set; }
        #endregion
        #region EmployeeID
        /// <summary>
        /// employeeID Bql field
        /// </summary>
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

		/// <summary>
		/// Gets sets Employee
		/// </summary>
		[PXDBInt(BqlField = typeof(EPTimeCard.employeeID))]
		public virtual Int32? EmployeeID { get; set; }
        #endregion
        #region WeekID
        /// <summary>
        /// weekId Bql field
        /// </summary>
        public abstract class weekId : PX.Data.BQL.BqlInt.Field<weekId> { }
		/// <summary>
		/// Gets sets Week Number
		/// </summary>
		[PXDBInt(BqlField = typeof(EPTimeCard.weekId))]
		public virtual Int32? WeekID
		{
			get;
			set;
		}

		#endregion
		
	}

	#endregion

	public class EPTimeCardStatusAttribute : PXStringListAttribute
	{
		public const string ApprovedStatus = "A";
		public const string HoldStatus = "H";
		public const string ReleasedStatus = "R";
		public const string OpenStatus = "O";
		public const string RejectedStatus = "C";

		public EPTimeCardStatusAttribute()
			: base(
				new[] { HoldStatus, OpenStatus, ApprovedStatus, RejectedStatus, ReleasedStatus },
				new[] { "On Hold", "Pending Approval", "Approved", "Rejected", "Released" }) { }
	}
}
