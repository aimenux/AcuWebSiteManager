using System;

using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.Common;
using PX.Objects.EP;

namespace PX.Objects.AR
{
	[Serializable]
	[PXCacheName(Messages.StatementCycle)]
	[PXPrimaryGraph(typeof(ARStatementMaint))]
	public partial class ARStatementCycle : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		/// <summary>
		/// A non-DB field indicating whether the current statement 
		/// cycle has been selected for processing by a user.
		/// </summary>
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		} = false;
		#endregion
		#region NextStatementDate
		public abstract class nextStmtDate : PX.Data.BQL.BqlDateTime.Field<nextStmtDate> { }
		/// <summary>
		/// A non-DB calculated field indicating the next date
		/// on which the customer statements will be generated.
		/// </summary>
		[PXDate]
		[PXUIField(DisplayName = "Next Statement Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? NextStmtDate
		{
			get;
			set;
		}
		#endregion
		#region StatementCycleId
		public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
		/// <summary>
		/// Key field. A human-readable unique string identifier 
		/// of the statement cycle.
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Cycle ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(ARStatementCycle.statementCycleId))]
		[PXReferentialIntegrityCheck]
		[PXFieldDescription]
		public virtual string StatementCycleId
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		/// <summary>
		/// The statement cycle description.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = " Description",Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXFieldDescription]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region UseFinPeriodForAging
		public abstract class useFinPeriodForAging : PX.Data.BQL.BqlBool.Field<useFinPeriodForAging> { }
		/// <summary>
		/// A boolean value indicating whether financial periods should be used instead 
		/// of user-defined aging periods. If <c>true</c>, the fields <see cref="AgeDays00"/>, 
		/// <see cref="AgeDays01"/>, and <see cref="AgeDays02"/> will not be used for aging. 
		/// Instead, the current (corresponding to the statement/aging date) and the three 
		/// preceding financial periods will be used.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = Messages.UseFinPeriodForAging)]
		[PXDefault(false)]
		public virtual bool? UseFinPeriodForAging
		{
			get;
			set;
		}
		#endregion
		#region AgeDays00
		public abstract class ageDays00 : PX.Data.BQL.BqlShort.Field<ageDays00> { }
		/// <summary>
		/// An integer value indicating the upper inclusive bound, in days, of the first 
		/// aging period. For example, if <see cref="AgeDays00"/> is equal to 7, then the first 
		/// aging period will correspond to documents that are 1-7 days past due.
		/// </summary>
		[PXDBShort(MinValue = 0)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Age Days 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual short? AgeDays00
		{
			get;
			set;
		}
		#endregion
		#region AgeDays01
		public abstract class ageDays01 : PX.Data.BQL.BqlShort.Field<ageDays01> { }
		/// <summary>
		/// An integer value indicating the upper inclusive bound, in days, of the second
		/// aging period. For example, if <see cref="AgeDays00"/> is equal to 7, and 
		/// <see cref="AgeDays01"/> is equal to 14, then the second aging period will
		/// correspond to documents that are 8-14 days past due.
		/// </summary>
		[PXDBShort(MinValue = 0)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Age Days 2", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual short? AgeDays01
		{
			get;
			set;
		}
		#endregion
		#region AgeDays02
		public abstract class ageDays02 : PX.Data.BQL.BqlShort.Field<ageDays02> { }
		/// <summary>
		/// An integer value indicating the upper inclusive bound, in days, of the third
		/// non-current aging period. For example, if <see cref="AgeDays01"/> is equal to 14,
		/// and <see cref="AgeDays02"/> is equal to 21, then the third aging period will
		/// correspond to documents that are 15-21 days past due.
		/// </summary>
		[PXDBShort(MinValue = 0)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Age Days 3", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual short? AgeDays02
		{
			get;
			set;
		}
		#endregion
		#region Bucket01LowerInclusiveBound
		public abstract class bucket01LowerInclusiveBound : PX.Data.BQL.BqlInt.Field<bucket01LowerInclusiveBound> { }
		/// <summary>
		/// A display-only integer field indicating the lower inclusive bound, in days, 
		/// of the first aging period.
		/// </summary>
		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Enabled = false)]
		[PXFormula(typeof(CS.int1))]
		public virtual int? Bucket01LowerInclusiveBound
		{
			get;
			set;
		}		
		#endregion
		#region Bucket02LowerInclusiveBound
		public abstract class bucket02LowerInclusiveBound : PX.Data.BQL.BqlInt.Field<bucket02LowerInclusiveBound> { }
		/// <summary>
		/// A display-only integer field indicating the lower inclusive bound, in days, 
		/// of the second aging period.
		/// </summary>
		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Enabled = false)]
		[PXFormula(typeof(Add<ARStatementCycle.ageDays00, CS.int1>))]
		public virtual int? Bucket02LowerInclusiveBound
		{
			get;
			set;
		}
		#endregion
		#region Bucket03LowerInclusiveBound
		public abstract class bucket03LowerInclusiveBound : PX.Data.BQL.BqlInt.Field<bucket03LowerInclusiveBound> { }
		/// <summary>
		/// A display-only integer field indicating the lower inclusive bound, in days, 
		/// of the third aging period.
		/// </summary>
		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Enabled = false)]
		[PXFormula(typeof(Add<ARStatementCycle.ageDays01, CS.int1>))]
		public virtual int? Bucket03LowerInclusiveBound
		{
			get;
			set;
		}
		#endregion
		#region Bucket04LowerExclusiveBound
		public abstract class bucket04LowerExclusiveBound : PX.Data.BQL.BqlInt.Field<bucket04LowerExclusiveBound> { }
		/// <summary>
		/// A display-only integer field indicating the lower exclusive bound, in days, 
		/// of the last aging period (the "over" period).
		/// </summary>
		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Enabled = false)]
		[PXFormula(typeof(Add<ARStatementCycle.ageDays02, CS.int0>))]
		public virtual int? Bucket04LowerExclusiveBound
		{
			get;
			set;
		}
		#endregion
		#region AgeMsgCurrent
		public abstract class ageMsgCurrent : PX.Data.BQL.BqlString.Field<ageMsgCurrent> { }
		/// <summary>
		/// The description of the zeroth (current) aging period, which incorporates
		/// documents that are not overdue.
		/// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Age Message 0", Visibility = PXUIVisibility.Visible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual string AgeMsgCurrent
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg00
		public abstract class ageMsg00 : PX.Data.BQL.BqlString.Field<ageMsg00> { }
		/// <summary>
		/// The description of the first aging period, which incorporates documents
		/// that are from 1 to <see cref="AgeDays00"/> days past due.
		/// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 1", Visibility = PXUIVisibility.Visible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual string AgeMsg00
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg01
		public abstract class ageMsg01 : PX.Data.BQL.BqlString.Field<ageMsg01> { }
		/// <summary>
		/// The description of the second aging period, which incorporates documents
		/// that are from <see cref="AgeDays00"/> + 1 to <see cref="AgeDays01"/> 
		/// days past due.
		/// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 2", Visibility = PXUIVisibility.Visible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual string AgeMsg01
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg02
		public abstract class ageMsg02 : PX.Data.BQL.BqlString.Field<ageMsg02> { }
		/// <summary>
		/// The description of the third aging period that incorporates documents
		/// that are from <see cref="AgeDays01"/> + 1 to <see cref="AgeDays02"/>
		/// days past due.
		/// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 3", Visibility = PXUIVisibility.Visible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual string AgeMsg02
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg03
		public abstract class ageMsg03 : PX.Data.BQL.BqlString.Field<ageMsg03> { }
		/// <summary>
		/// The description of the last aging period that incorporates documents
		/// that are over <see cref="AgeDays02"/> days past due.
		/// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 4", Visibility = PXUIVisibility.Visible)]
		[PXUIEnabled(typeof(Where<ARStatementCycle.useFinPeriodForAging, Equal<False>>))]
		public virtual string AgeMsg03
		{
			get;
			set;
		}
		#endregion
		#region LastAgeDate
		[Obsolete("This field is not used anymore and will be removed in Acumatica 7.0")]
		public abstract class lastAgeDate : PX.Data.BQL.BqlDateTime.Field<lastAgeDate> { }
		/// <summary>
		/// Obsolete field.
		/// </summary>
		[PXDBDate]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 7.0")]
		public virtual DateTime? LastAgeDate
		{
			get;
			set;
		}
		#endregion
		#region LastFinChrgDate
		public abstract class lastFinChrgDate : PX.Data.BQL.BqlDateTime.Field<lastFinChrgDate> { }
		/// <summary>
		/// Indicates the date on which financial charges were last generated for 
		/// the current statement cycle.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Last Finance Charge Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? LastFinChrgDate
		{
			get;
			set;
		}
		#endregion
		#region LastStmtDate
		public abstract class lastStmtDate : PX.Data.BQL.BqlDateTime.Field<lastStmtDate> { }
		/// <summary>
		/// Indicates the date on which customer statements were last generated for
		/// the current statement cycle.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Last Statement Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? LastStmtDate
		{
			get;
			set;
		}
		#endregion
		#region PrepareOn
		public abstract class prepareOn : PX.Data.BQL.BqlString.Field<prepareOn> { }
		/// <summary>
		/// Indicates the type of schedule, according to which customer statements
		/// are generated within the current statement cycle. See <see cref="ARStatementScheduleType"/>.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[LabelList(typeof(ARStatementScheduleType))]
		[PXDefault(ARStatementScheduleType.EndOfMonth)]
		[PXUIField(DisplayName = Messages.ScheduleType)]
		public virtual string PrepareOn
		{
			get;
			set;
		}
		#endregion
		#region Day00
		public abstract class day00 : PX.Data.BQL.BqlShort.Field<day00> { }
		/// <summary>
		/// For <see cref="ARStatementScheduleType.TwiceAMonth"/> and
		/// <see cref="ARStatementScheduleType.FixedDayOfMonth"/> schedule types,
		/// indicates the first (or the only, correspondingly) day of month, on
		/// which customer statements are generated.
		/// </summary>
		[PXDBShort]
		[PXUIField(DisplayName = Messages.DayOfMonth1)]
		[PXUIVisible(typeof(Where<
			ARStatementCycle.prepareOn, Equal<ARStatementScheduleType.fixedDayOfMonth>,
			Or<ARStatementCycle.prepareOn, Equal<ARStatementScheduleType.twiceAMonth>>>))]
		public virtual short? Day00
		{
			get;
			set;
		}
		#endregion
		#region Day01
		public abstract class day01 : PX.Data.BQL.BqlShort.Field<day01> { }
		/// <summary>
		/// For <see cref="ARStatementScheduleType.TwiceAMonth"/> schedule type,
		/// indicates the second day of month, on which bi-monthly customer 
		/// statements are generated.
		/// </summary>
		[PXDBShort]
		[PXUIField(DisplayName = Messages.DayOfMonth2)]
		[PXUIVisible(typeof(Where<ARStatementCycle.prepareOn, Equal<ARStatementScheduleType.twiceAMonth>>))]
		public virtual short? Day01
		{
			get;
			set;
		}
		#endregion
		#region DayOfWeek
		public abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }
		/// <summary>
		/// For <see cref="ARStatementScheduleType.Weekly"/> schedule type,
		/// indicates the day of the week, on which weekly customer statements
		/// are generated.
		/// </summary>
		[PXDBInt]
		[DayOfWeek]
		[PXDefault((int)System.DayOfWeek.Sunday)]
		[PXUIField(DisplayName = Messages.DayOfWeek)]
		[PXUIVisible(typeof(Where<ARStatementCycle.prepareOn, Equal<ARStatementScheduleType.weekly>>))]
		public virtual int? DayOfWeek
		{
			get;
			set;
		}
		#endregion
		#region FinChargeApply
		public abstract class finChargeApply : PX.Data.BQL.BqlBool.Field<finChargeApply> { }
		/// <summary>
		/// A boolean value indicating whether financial charges should be applied
		/// to customers belonging to the current statement cycle.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(Search<CustomerClass.finChargeApply, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
		[PXUIField(DisplayName = "Apply Overdue Charges")]
		public virtual bool? FinChargeApply
		{
			get;
			set;
		}
		#endregion
		#region FinChargeID
		public abstract class finChargeID : PX.Data.BQL.BqlString.Field<finChargeID> { }
		/// <summary>
		/// The reference to the overdue charge that should be calculated 
		/// for customers belonging to the current statement cycle.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Overdue Charge ID")]
		[PXSelector(typeof(ARFinCharge.finChargeID), DescriptionField = typeof(ARFinCharge.finChargeDesc))]
		public virtual string FinChargeID
		{
			get;
			set;
		}
		#endregion
		#region RequirePaymentApplication
		public abstract class requirePaymentApplication : PX.Data.BQL.BqlBool.Field<requirePaymentApplication> { }
		/// <summary>
		/// A boolean value indicating whether the system should require
		/// all open customer payments to be applied in full before generating
		/// customer statements.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.RequirePaymentApplicationBeforeStatement)]
		public virtual bool? RequirePaymentApplication
		{
			get;
			set;
		}
		#endregion
		#region RequireFinChargeProcessing
		public abstract class requireFinChargeProcessing : PX.Data.BQL.BqlBool.Field<requireFinChargeProcessing> { }
		/// <summary>
		/// A boolean value indicating whether the system should require
		/// the overdue charges calculation before generating customer statements.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Overdue Charges Calculation Before Statement")]
		public virtual bool? RequireFinChargeProcessing
		{
			get;
			set;
		}
		#endregion
		#region AgeBasedOn
		public abstract class ageBasedOn : PX.Data.BQL.BqlString.Field<ageBasedOn> { }
		/// <summary>
		/// Indicates whether documents of customers belonging to the current
		/// statement cycle should be aged based on their document date, 
		/// or their due date.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[LabelList(typeof(AgeBasedOnType))]
		[PXDefault(AgeBasedOnType.DueDate)]
		[PXUIField(DisplayName = Messages.AgeBasedOn)]
		public virtual string AgeBasedOn
		{
			get;
			set;
		}
		#endregion
		#region PrintEmptyStatements
		public abstract class printEmptyStatements : PX.Data.BQL.BqlBool.Field<printEmptyStatements> { }
		/// <summary>
		/// If <c>true</c>, the system will generate but not print or email
		/// Open Item statements without open documents, and Balance Brought
		/// Forward statements if there was no activity in the period and
		/// the balance brought from the previous statement is zero.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.PrintEmptyStatements)]
		public virtual bool? PrintEmptyStatements
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		/// <summary>
		/// The unique identifier of the note associated with the current
		/// statement cycle.
		/// </summary>
		[PXNote(DescriptionField = typeof(ARStatementCycle.statementCycleId))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}