using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;


namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRPayGroupCalendar)]
	public partial class PRPayGroupYearSetup : IBqlTable, IYearSetup
	{
		#region PayGroupID
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		protected string _PayGroupID;
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXSelector(typeof(Search<PRPayGroup.payGroupID>), DescriptionField = typeof(PRPayGroup.description))]
		[PXUIField(DisplayName = "Pay Group")]
		[PXFieldDescription]
		[PXParent(typeof(Select<PRPayGroup, Where<PRPayGroup.payGroupID, Equal<Current<PRPayGroupYearSetup.payGroupID>>>>))]
		public virtual string PayGroupID
		{
			get
			{
				return _PayGroupID;
			}
			set
			{
				_PayGroupID = value;
			}
		}
		#endregion
		#region FirstFinYear
		public abstract class firstFinYear : PX.Data.BQL.BqlString.Field<firstFinYear>
		{
		}
		protected String _FirstFinYear;
		[PXDBString(4, IsFixed = true, InputMask = "####")]
		[PXDefault]
		[PXUIField(DisplayName = "First Year", Enabled = true)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String FirstFinYear
		{
			get
			{
				return this._FirstFinYear;
			}
			set
			{
				this._FirstFinYear = value;
			}
		}
		#endregion
		#region BegFinYear
		public abstract class begFinYear : PX.Data.BQL.BqlDateTime.Field<begFinYear>
		{
		}
		protected DateTime? _BegFinYear;
		[PXDBDate()]
		[PXDefault]
		[PXUIField(DisplayName = "Year Starts On")]
		public virtual DateTime? BegFinYear
		{
			get
			{
				return this._BegFinYear;
			}
			set
			{
				this._BegFinYear = value;
			}
		}
		#endregion
		#region FinPeriods
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods>
		{
		}
		protected Int16? _FinPeriods;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Number of Periods ")]
		public virtual Int16? FinPeriods
		{
			get
			{
				return this._FinPeriods;
			}
			set
			{
				this._FinPeriods = value;
			}
		}
		#endregion
		#region UserDefined
		public abstract class userDefined : PX.Data.BQL.BqlBool.Field<userDefined>
		{
		}
		protected Boolean? _UserDefined;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "User-Defined Periods")]
		public virtual Boolean? UserDefined
		{
			[PXDependsOnFields(typeof(periodType))]
			get
			{
				return this.PeriodType == FinPeriodType.CustomPeriodsNumber;
			}
			set
			{
				//this._UserDefined = value;
			}
		}
		#endregion
		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(PRPayGroupYearSetup.payGroupID))]
		public virtual Guid? NoteID { get; set; }

		#endregion
		#region PeriodType
		public abstract class periodType : PX.Data.BQL.BqlString.Field<periodType>
		{
		}
		protected string _PeriodType;

		[PXDBString(2, IsFixed = true)]
		[PXDefault(FinPeriodType.Month)]
		[PXUIField(DisplayName = "Period Type")]
		[PayPeriodType.List()]
		public virtual string PeriodType
		{
			get
			{
				return this._PeriodType;
			}
			set
			{
				this._PeriodType = value;
			}
		}
		#endregion
		#region IsWeekOrBiWeek
		public abstract class isWeeklyOrBiWeeklyPeriod : PX.Data.BQL.BqlBool.Field<isWeeklyOrBiWeeklyPeriod> { }
		[PXBool]
		[PXFormula(typeof(True.When<periodType.IsEqual<PayPeriodType.week>.Or<periodType.IsEqual<PayPeriodType.biWeek>>>.Else<False>))]
		[PXUIField(DisplayName = "Is Weekly or Biweekly Period", Visible = false)]
		public bool? IsWeeklyOrBiWeeklyPeriod { get; set; }
		#endregion
		#region PeriodLength
		public abstract class periodLength : PX.Data.BQL.BqlShort.Field<periodLength>
		{
		}
		protected Int16? _PeriodLength;
		[PXDBShort(MinValue = 5, MaxValue = 366)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Length of Period(days)")]
		public virtual Int16? PeriodLength
		{
			get
			{
				return this._PeriodLength;
			}
			set
			{
				this._PeriodLength = value;
			}
		}
		#endregion
		#region PeriodsStartDate
		public abstract class periodsStartDate : PX.Data.BQL.BqlDateTime.Field<periodsStartDate>
		{
		}
		protected DateTime? _PeriodsStartDate;
		[PXDBDate()]
		[PXDefault]
		[PXUIField(DisplayName = "First Period Starts On")]
		public virtual DateTime? PeriodsStartDate
		{
			get
			{
				return this._PeriodsStartDate;
			}
			set
			{
				this._PeriodsStartDate = value;
			}
		}
		#endregion
		#region TransactionsStartDate
		public abstract class transactionsStartDate : PX.Data.BQL.BqlDateTime.Field<transactionsStartDate>
		{
		}
		protected DateTime? _TransactionsStartDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "First Transaction Date", Enabled = false)]
		[PXFormula(typeof(Validate<PRPayGroupYearSetup.finPeriods>))]
		public virtual DateTime? TransactionsStartDate
		{
			get
			{
				return this._TransactionsStartDate;
			}
			set
			{
				this._TransactionsStartDate = value;
			}
		}
		#endregion
		#region SecondPeriodsStartDate
		public abstract class secondPeriodsStartDate : PX.Data.BQL.BqlDateTime.Field<secondPeriodsStartDate>
		{
		}
		protected DateTime? _SecondPeriodsStartDate;
		[PXDBDate()]
		[PXDefault]
		[PXUIField(DisplayName = "2nd Period Starts On")]
		[PXFormula(typeof(Validate<PRPayGroupYearSetup.periodsStartDate>))]
		public virtual DateTime? SecondPeriodsStartDate
		{
			get
			{
				return this._SecondPeriodsStartDate;
			}
			set
			{
				this._SecondPeriodsStartDate = value;
			}
		}
		#endregion
		#region SecondTransactionsStartDate
		public abstract class secondTransactionsStartDate : PX.Data.BQL.BqlDateTime.Field<secondTransactionsStartDate>
		{
		}
		protected DateTime? _SecondTransactionsStartDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "2nd Transaction Date", Enabled = false)]
		[PXFormula(typeof(Validate<PRPayGroupYearSetup.transactionsStartDate>))]
		public virtual DateTime? SecondTransactionsStartDate
		{
			get
			{
				return this._SecondTransactionsStartDate;
			}
			set
			{
				this._SecondTransactionsStartDate = value;
			}
		}
		#endregion
		#region EndYearCalcMethod
		public abstract class endYearCalcMethod : PX.Data.BQL.BqlString.Field<endYearCalcMethod>
		{
		}
		protected string _EndYearCalcMethod;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(EndYearMethod.Calendar)]
		[PXUIField(DisplayName = "Year End Calculation Method")]
		[EndYearMethod.List()]
		public virtual string EndYearCalcMethod
		{
			get
			{
				return this._EndYearCalcMethod;
			}
			set
			{
				this._EndYearCalcMethod = value;
			}
		}
		#endregion
		#region EndYearDayOfWeek
		public abstract class endYearDayOfWeek : PX.Data.BQL.BqlInt.Field<endYearDayOfWeek>
		{
		}
		protected int? _EndYearDayOfWeek;
		[PXDBInt()]
		[PXDefault(typeof(OneBasedDayOfWeek.saturday))]
		[PXUIField(DisplayName = "Week Starts On", Enabled = true)]
		[OneBasedDayOfWeek.List]
		public virtual int? EndYearDayOfWeek
		{
			get
			{
				return this._EndYearDayOfWeek;
			}
			set
			{
				this._EndYearDayOfWeek = value;
			}
		}
		#endregion
		#region TranDayOfWeek
		public abstract class tranDayOfWeek : PX.Data.BQL.BqlInt.Field<tranDayOfWeek>
		{
		}
		protected int? _TranDayOfWeek;
		[PXDBInt()]
		[PXDefault(5)]
		[PXUIField(DisplayName = "Paid On", Enabled = true)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7 }, new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" })]
		public virtual int? TranDayOfWeek
		{
			get
			{
				return this._TranDayOfWeek;
			}
			set
			{
				this._TranDayOfWeek = value;
			}
		}
		#endregion
		#region TranWeekDiff
		public abstract class tranWeekDiff : PX.Data.BQL.BqlInt.Field<tranWeekDiff>
		{
		}
		protected int? _TranWeekDiff;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Is Paid ", Enabled = true)]
		[PXIntList(new int[] { -4, -3, -2, -1, 0, 1, 2, 3, 4 }, new string[] { "4 weeks before period", "3 weeks before period", "2 weeks before period", "1 week before period", "Same week as the period", "1 week after period ", "2 weeks after period", "3 weeks after period", "4 weeks after period" })]
		public virtual int? TranWeekDiff
		{
			get
			{
				return this._TranWeekDiff;
			}
			set
			{
				this._TranWeekDiff = value;
			}
		}
		#endregion
		#region IsSecondWeekOfYear
		public abstract class isSecondWeekOfYear : PX.Data.BQL.BqlBool.Field<isSecondWeekOfYear>
		{
		}

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Occurs On Second Week Of Year")]
		public bool? IsSecondWeekOfYear
		{
			get { return _IsSecondWeekOfYear; }
			set { if (value.HasValue) this._IsSecondWeekOfYear = value.Value; }
		}
		protected bool _IsSecondWeekOfYear = false;
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
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
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
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
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
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
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
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

		//attention - during regeneration they are erased
		#region Additional Fields

		#region AdjustToPeriodStart
		[PXUIField(DisplayName = "Adjust To Period Start")]
		[PXBool]
		public bool? AdjustToPeriodStart
		{
			get { return _AdjustToPeriodStart; }
			set { if (value.HasValue) this._AdjustToPeriodStart = value.Value; }
		}
		protected bool _AdjustToPeriodStart = false;
		#endregion
		#region HasAdjustmentPeriod
		public abstract class hasAdjustmentPeriod : PX.Data.BQL.BqlBool.Field<hasAdjustmentPeriod>
		{
		}
		[PXBool()]
		public bool? HasAdjustmentPeriod
		{
			get { return false; }
			set { }
		}

		#endregion
		#region BelongsToNextYear
		public abstract class belongsToNextYear : PX.Data.BQL.BqlBool.Field<belongsToNextYear> { };
		[PXUIField(DisplayName = "Belongs To Next Year", Enabled = false)]
		[PXBool()]
		public bool? BelongsToNextYear
		{
			[PXDependsOnFields(typeof(PRPayGroupYearSetup.firstFinYear), typeof(PRPayGroupYearSetup.begFinYear))]
			get
			{
				var yearNbr = PRPayGroupYearSetupMaint.GetYearNbr(this.FirstFinYear);
				if (!yearNbr.HasValue || !this.BegFinYear.HasValue)
					return null;

				return yearNbr.Value != this.BegFinYear.Value.Year;
			}
			set { }
		}
		#endregion

		#endregion

		#region Methods

		public bool IsFixedLengthPeriod
		{
			[PXDependsOnFields(typeof(fPType))]
			get
			{
				return FiscalPeriodSetupCreator.IsFixedLengthPeriod(this.FPType);
			}
		}
		public abstract class fPType : IBqlField { }

		public FiscalPeriodSetupCreator.FPType FPType
		{
			[PXDependsOnFields(typeof(periodType))]
			get
			{
				return PayPeriodType.GetFPType(this.PeriodType);
			}
		}

		#endregion
	}
}