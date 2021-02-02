using System;
using System.Collections.Generic;

using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.Common;
using PX.Objects.GL;

namespace PX.Objects.DR
{
	public class DRScheduleOption : ILabelProvider
	{
		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ ScheduleOptionStart, Messages.StartOfFinancialPeriod },
			{ ScheduleOptionEnd, Messages.EndOfFinancialPeriod },
			{ ScheduleOptionFixedDate, Messages.FixedDayOfThePeriod },
		};

		public const string ScheduleOptionStart = "S";
		public const string ScheduleOptionEnd = "E";
		public const string ScheduleOptionFixedDate = "D";

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
	}

	/// <summary>
	/// Represents a deferral code, which determines the method 
	/// and schedule by which deferred revenue or expense is recognized.
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(DeferredCodeMaint))]
	[PXCacheName(Messages.DeferredCode)]
	public class DRDeferredCode : PX.Data.IBqlTable
	{
		#region DeferredCodeID
		public abstract class deferredCodeID : PX.Data.BQL.BqlString.Field<deferredCodeID> { }
		protected String _DeferredCodeID;
		/// <summary>
		/// The unique identifier of the deferral code.
		/// This field is the key field.
		/// </summary>
		[PXDefault]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
		[PXFieldDescription]
		[PXReferentialIntegrityCheck]
		public virtual String DeferredCodeID
		{
			get
			{
				return this._DeferredCodeID;
			}
			set
			{
				this._DeferredCodeID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>
		/// A user-friendly description of the deferral code.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region AccountType
		public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }
		protected string _AccountType;
		/// <summary>
		/// The type of the deferral code.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined in the
		/// <see cref="DeferredAccountType"/> class.
		/// </value>
		[PXDBString(1)]
		[PXDefault(DeferredAccountType.Income)]
		[LabelList(typeof(DeferredAccountType))]
		[PXUIField(DisplayName = "Code Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string AccountType
		{
			get
			{
				return this._AccountType;
			}
			set
			{
				this._AccountType = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		/// <summary>
		/// The identifier of the deferral <see cref="Account"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DescriptionField = typeof(Account.description), Visibility = PXUIVisibility.SelectorVisible, DisplayName="Deferral Account", ControlAccountForModule = ControlAccountModule.DR)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
        #endregion
        #region Active
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
        public virtual bool? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		/// <summary>
		/// The identifier of the deferral <see cref="Sub">subaccount</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(DRDeferredCode.accountID), DescriptionField = typeof(Sub.description), Visibility = PXUIVisibility.SelectorVisible, DisplayName="Deferral Sub.")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region ReconNowPct
		public abstract class reconNowPct : PX.Data.BQL.BqlDecimal.Field<reconNowPct> { }
		protected Decimal? _ReconNowPct;
		/// <summary>
		/// The percentage of deferred revenue or expense
		/// that should be recognized immediately upon the release of
		/// the <see cref="AP.APTran">AP</see> or <see cref="AR.ARTran"/>
		/// AR</see> document line containing the deferral code.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Recognize Now %", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReconNowPct
		{
			get
			{
				return this._ReconNowPct;
			}
			set
			{
				this._ReconNowPct = value;
			}
		}
		#endregion
		#region StartOffset
		public abstract class startOffset : PX.Data.BQL.BqlShort.Field<startOffset> { }
		protected Int16? _StartOffset;
		/// <summary>
		/// The number of <see cref="OrganizationFinPeriod"/>financial periods</see> 
		/// that should be skipped prior to the first recognition transaction.
		/// </summary>
		/// <remarks>
		/// This field is not applicable in case of a flexible
		/// deferral code. For details, see <see cref="DeferredMethodType"/>.
		/// </remarks>
		[PXDefault((short)0)]
		[PXDBShort]
		[PXUIField(DisplayName = "Start Offset")]
		public virtual Int16? StartOffset
		{
			get
			{
				return this._StartOffset;
			}
			set
			{
				this._StartOffset = value;
			}
		}
		#endregion
		#region Occurrences
		public abstract class occurrences : PX.Data.BQL.BqlShort.Field<occurrences> { }
		protected Int16? _Occurrences;
		/// <summary>
		/// The total number of required deferred revenue 
		/// recognition transactions.
		/// </summary>
		/// <remarks>
		/// This field is not applicable in case of a flexible
		/// deferral code. For details, see <see cref="DeferredMethodType"/>.
		/// </remarks>
		[PXDBShort(MinValue = 0)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Occurrences")]
		public virtual Int16? Occurrences
		{
			get
			{
				return this._Occurrences;
			}
			set
			{
				this._Occurrences = value;
			}
		}
		#endregion
		#region Frequency
		public abstract class frequency : PX.Data.BQL.BqlShort.Field<frequency> { }
		protected Int16? _Frequency;
		/// <summary>
		/// The number of <see cref="OrganizationFinPeriod">financial periods</see>
		/// that should pass between revenue or expense recognitions.
		/// </summary>
		/// <remarks>
		/// This field is not applicable in case of a flexible
		/// deferral code. For details, see <see cref="DeferredMethodType"/>.
		/// </remarks>
		[PXDBShort(MinValue = 1)]
		[PXUIField(DisplayName = "Every", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? Frequency
		{
			get
			{
				return this._Frequency;
			}
			set
			{
				this._Frequency = value;
			}
		}
		#endregion
		#region ScheduleOption
		public abstract class scheduleOption : PX.Data.BQL.BqlString.Field<scheduleOption> { }
		/// <summary>
		/// The position of the revenue or expense recognition date 
		/// within the financial period.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined by the
		/// <see cref="DRScheduleOption"/> class.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Schedule Options", Visibility = PXUIVisibility.Visible)]
		[PXDefault(DRScheduleOption.ScheduleOptionStart)]
		[LabelList(typeof(DRScheduleOption))]
		public virtual String ScheduleOption
		{
			get;
			set;
		}
		#endregion
		#region FixedDay
		public abstract class fixedDay : PX.Data.BQL.BqlShort.Field<fixedDay> { }
		protected Int16? _FixedDay;
		/// <summary>
		/// If <see cref="ScheduleOption"/> is set to 
		/// <see cref="DRScheduleOption.ScheduleOptionFixedDate"/>,
		/// defines the number of day within the financial period
		/// on which revenue or expense recognition should occur.
		/// </summary>
		[PXDBShort(MinValue = 1, MaxValue = 31)]
		[PXUIField(DisplayName = "Fixed Day of the Period", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? FixedDay
		{
			get
			{
				return this._FixedDay;
			}
			set
			{
				this._FixedDay = value;
			}
		}
		#endregion
		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }
		/// <summary>
		/// The revenue recognition method associated with 
		/// the deferral code. 
		/// </summary>
		/// <value>
		/// This field can have one of the values defined by
		/// the <see cref="DeferredMethodType"/> class.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Recognition Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(DeferredMethodType.EvenPeriods)]
		[LabelList(typeof(DeferredMethodType))]
		public virtual string Method
		{
			get;
			set;
		}
		#endregion
		#region IsMDA
		public abstract class multiDeliverableArrangement : PX.Data.BQL.BqlBool.Field<multiDeliverableArrangement> { }
		protected bool? _MultiDeliverableArrangement;
		/// <summary>
		/// When set to <c>true</c> indicates that this deferral code is used for items
		/// that represent multiple deliverable arrangements and
		/// for which the revenue should be split into multiple components.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Multiple-Deliverable Arrangement")]
		public virtual bool? MultiDeliverableArrangement
		{
			get { return _MultiDeliverableArrangement; }
			set { _MultiDeliverableArrangement = value; }
		}
		#endregion
		#region AccountSource
		public abstract class accountSource : PX.Data.BQL.BqlString.Field<accountSource> { }
		protected string _AccountSource;
		/// <summary>
		/// Determines the source from which the deferral account
		/// should be taken during deferral schedule creation.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined by
		/// <see cref="DeferralAccountSource.ListAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(DeferralAccountSource.DeferralCode)]
		[DeferralAccountSource.List]
		[PXUIField(DisplayName = "Use Deferral Account from")]
		public virtual string AccountSource
		{
			get { return _AccountSource; }
			set { _AccountSource = value; }
		}
		#endregion
		#region CopySubFromSourceTran
		public abstract class copySubFromSourceTran : PX.Data.BQL.BqlBool.Field<copySubFromSourceTran> { }
		protected bool? _CopySubFromSourceTran;
		/// <summary>
		/// If set to <c>true</c>, indicates that the deferral
		/// subaccount should be copied from the income or expense
		/// subaccount of the document line that contains the 
		/// deferral code.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Sub. from Sales/Expense Sub.")]
		public virtual bool? CopySubFromSourceTran
		{
			get { return _CopySubFromSourceTran; }
			set { _CopySubFromSourceTran = value; }
		}
		#endregion
		#region DeferralSubMaskAR
		public abstract class deferralSubMaskAR : PX.Data.BQL.BqlString.Field<deferralSubMaskAR> { }

		protected string _DeferralSubMaskAR;
		/// <summary>
		/// When <see cref="CopySubFromSourceTran"/> is set to <c>false</c>,
		/// specifies the subaccount mask that defines the structure
		/// of the deferral subaccount for the Accounts Receivable module.
		/// </summary>
		[PXDefault]
		[SubAccountMaskAR(DisplayName = "Combine Deferral Sub. from")]
		public virtual string DeferralSubMaskAR
		{
			get { return _DeferralSubMaskAR; }
			set { _DeferralSubMaskAR = value; }
		}
		#endregion
		#region DeferralSubMaskAP
		public abstract class deferralSubMaskAP : PX.Data.BQL.BqlString.Field<deferralSubMaskAP> { }
		protected string _DeferralSubMaskAP;
		/// <summary>
		/// When <see cref="CopySubFromSourceTran"/> is set to <c>false</c>,
		/// specifies the subaccount mask that defines the structure
		/// of the deferral subaccount for the Accounts Payable module.
		/// </summary>
		[PXDefault]
		[SubAccountMaskAP(DisplayName = "Combine Deferral Sub. from")]
		public virtual string DeferralSubMaskAP
		{
			get { return _DeferralSubMaskAP; }
			set { _DeferralSubMaskAP = value; }
		}
		#endregion
		#region Periods
		public abstract class periods : PX.Data.BQL.BqlString.Field<periods> { }

		/// <summary>
		/// The field is used for UI rendering purpose only.
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Period(s) ")]
		public virtual string Periods { get; set; }
		#endregion
		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(DRDeferredCode.deferredCodeID))]
		public virtual Guid? NoteID { get; set; }

		#endregion
		#region RecognizeInPastPeriods
		public abstract class recognizeInPastPeriods : PX.Data.BQL.BqlBool.Field<recognizeInPastPeriods> { }

		/// <summary>
		/// For flexible recognition methods, specifies if the system should
		/// allow recognition in periods that are earlier than the document date.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Recognition in Previous Periods")]
		public virtual bool? RecognizeInPastPeriods
		{
			get;
			set;
		}
		#endregion
		#region System Columns
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
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}

	public class DeferredAccountType : ILabelProvider
	{
		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ Income, Messages.Income },
			{ Expense, Messages.Expense },
		};

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public const string Income = GL.AccountType.Income;
		public const string Expense = GL.AccountType.Expense;

		public class income : PX.Data.BQL.BqlString.Constant<income>
		{
			public income() : base(Income) { ;}
		}

		public class expense : PX.Data.BQL.BqlString.Constant<expense>
		{
			public expense() : base(Expense) { ;}
		}

	}

	public class DeferredMethodType : ILabelProvider
	{
		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ EvenPeriods, Messages.EvenPeriods },
			{ ProrateDays, Messages.ProrateDays },
			{ ExactDays, Messages.ExactDays },
			{ FlexibleProrateDays, Messages.FlexibleProrateDays },
			{ FlexibleExactDays, Messages.FlexibleExactDays },
			{ CashReceipt, Messages.CashReceipt },
		};

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public const string EvenPeriods = "E";
		public const string ProrateDays = "P";
		public const string ExactDays = "D";
		public const string FlexibleProrateDays = "F";
		public const string FlexibleExactDays = "L";
		public const string CashReceipt = "C";

		public class EvenPeriodMethod : PX.Data.BQL.BqlString.Constant<EvenPeriodMethod>
		{
			public EvenPeriodMethod() : base(EvenPeriods) { ;}
		}
		public class ProrateDaysMethod : PX.Data.BQL.BqlString.Constant<ProrateDaysMethod>
		{
			public ProrateDaysMethod() : base(ProrateDays) { ;}
		}
		public class ExactDaysMethod : PX.Data.BQL.BqlString.Constant<ExactDaysMethod>
		{
			public ExactDaysMethod() : base(ExactDays) { ;}
		}

		public class cashReceipt : PX.Data.BQL.BqlString.Constant<cashReceipt>
		{
			public cashReceipt() : base(CashReceipt) { ;}
		}

		public class flexibleProrateDays : PX.Data.BQL.BqlString.Constant<flexibleProrateDays>
		{
			public flexibleProrateDays() : base(FlexibleProrateDays) { }
		}

		public class flexibleExactDays : PX.Data.BQL.BqlString.Constant<flexibleExactDays>
		{
			public flexibleExactDays() : base(FlexibleExactDays) { }
		}

		public static bool RequiresTerms(string method)
		{
			return method == FlexibleExactDays || method == FlexibleProrateDays;
		}

		public static bool RequiresTerms(DRDeferredCode code)
		{
			return code != null && RequiresTerms(code.Method);
		}
	}
}
