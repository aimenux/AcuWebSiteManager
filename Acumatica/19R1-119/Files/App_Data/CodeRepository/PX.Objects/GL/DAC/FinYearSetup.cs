namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
    /// <summary>
    /// Represents the Financial Year setup record used to define the template for the actual <see cref="PX.Objects.GL.Obsolete.FinYear">Financial Years</see>.
    /// The system stores one record of this type per company. It is edited through the Financial Year (GL.10.10.00) form.
    /// </summary>
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(FiscalYearSetupMaint))]
    [PXCacheName(Messages.FiscalYearSetupMaint)]
	public partial class FinYearSetup : PX.Data.IBqlTable, IYearSetup
	{
		#region FirstFinYear
		public abstract class firstFinYear : PX.Data.BQL.BqlString.Field<firstFinYear> { }
		protected String _FirstFinYear;

        /// <summary>
        /// First financial year, for which data is stored in the system.
        /// </summary>
		[PXDBString(4, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "First Financial Year",Enabled = false)]
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
		public abstract class begFinYear : PX.Data.BQL.BqlDateTime.Field<begFinYear> { }
		protected DateTime? _BegFinYear;

        /// <summary>
        /// The start date of the financial year.
        /// </summary>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Financial Year Starts On")]
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
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods> { }
		protected Int16? _FinPeriods;

        /// <summary>
        /// The number of financial periods in the year.
        /// </summary>
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Number of Financial Periods ")]
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
		#region PeriodLength
		public abstract class periodLength : PX.Data.BQL.BqlShort.Field<periodLength> { }
		protected Int16? _PeriodLength;

        /// <summary>
        /// Obsolete field.
        /// The length of <see cref="PX.Objects.GL.Obsolete.FinPeriod">periods</see> of the year in days.
        /// </summary>
		[PXDBShort(MinValue=5, MaxValue=366)]
		[PXDefault(PersistingCheck =PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Length of Financial Period (days)",Visible= true, Enabled = false)]
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
		#region PeriodType
		public abstract class periodType : PX.Data.BQL.BqlString.Field<periodType> { }
		protected string _PeriodType;

        /// <summary>
        /// The type of financial periods that make up the year.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// "MO" - Month,
        /// "BM" - Two Months,
        /// "QR" - Quarter,
        /// "WK" - Week,
        /// "BW" - Two Weeks,
        /// "FW" - Four Weeks,
        /// "DC" - Decade,
        /// "FF" - 4-4-5 Weeks,
        /// "FI" - 4-5-4 Weeks,
        /// "IF" - 5-4-4 Weeks,
        /// "CL" - Customer Periods Length,
        /// "CN" - Custom Number of Periods.
        /// </value>
		[PXDBString(2,IsFixed = true)]
		[PXDefault(FinPeriodType.Month)]
		[PXUIField(DisplayName = "Period Type")]
		[FinPeriodType.List()]
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
		#region UserDefined
		public abstract class userDefined : PX.Data.BQL.BqlBool.Field<userDefined> { }
		protected Boolean? _UserDefined;

        /// <summary>
        /// The read-only field indicating whether the <see cref="PX.Objects.GL.Obsolete.FinPeriod">periods</see> of the year can be modified by user.
        /// </summary>
        /// <value>
        /// Depends on the value of the <see cref="PeriodType"/> field - returns <c>true</c> if
        /// the <see cref="PeriodType"/> is <c>"CN"</c> (Custom number of periods).
        /// </value>
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "User-Defined Periods")]
		public virtual Boolean? UserDefined
		{
			get
			{
				return (this.PeriodType == FinPeriodType.CustomPeriodsNumber);
			}
			set
			{
				//this._UserDefined = value;
			}
		}
		#endregion
		#region PeriodsStartDate
		public abstract class periodsStartDate : PX.Data.BQL.BqlDateTime.Field<periodsStartDate> { }
		protected DateTime? _PeriodsStartDate;

        /// <summary>
        /// The start date of the first period of the year.
        /// </summary>
		[PXDBDate()]
		[PXDefault(typeof(FinYearSetup.begFinYear))]
		[PXUIField(DisplayName = "First Period Start Date", Visible = true)]
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
		#region HasAdjustmentPeriod
		public abstract class hasAdjustmentPeriod : PX.Data.BQL.BqlBool.Field<hasAdjustmentPeriod> { }

        /// <summary>
        /// When set to <c>true</c>, indicates that the system generates an additional period for posting adjustments.
        /// The adjustment period has the same start and end date and is the last period of the year.
        /// No date in the year corresponds to the adjustment period, so it can be selected for a particular document or batch only manually.
        /// See also the <see cref="PX.Objects.GL.Obsolete.FinPeriod.IsAdjustment"/> field.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Has Adjustment Period")]
		public bool? HasAdjustmentPeriod
		{
			get { return this._HasAdjustmentPeriod; }
			set { if (value.HasValue) this._HasAdjustmentPeriod = value.Value; }
		}
		protected bool _HasAdjustmentPeriod = false;
		#endregion 
		#region EndYearCalcMethod
		public abstract class endYearCalcMethod : PX.Data.BQL.BqlString.Field<endYearCalcMethod> { }
		protected string _EndYearCalcMethod;

        /// <summary>
        /// The method used to determine the end date of a year with week-long periods.
        /// </summary>
        /// <value>
        /// Available values are:
        /// "CA" - Last day of the financial year,
        /// "LD" - Include last <see cref="YearLastDayOfWeek">selected day of week</see> of the year,
        /// "ND" - Include <see cref="YearLastDayOfWeek">selected day of week</see>, nearest tot the end of the year.
        /// For more information on these options see the documentation for the Financial Year (GL.10.10.00) page of the system.
        /// </value>
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
		public abstract class endYearDayOfWeek : PX.Data.BQL.BqlInt.Field<endYearDayOfWeek> { }
		protected int? _EndYearDayOfWeek;

        /// <summary>
        /// The day of the week when period starts.
        /// Relevant only for the <see cref="PeriodType">period types</see> based on weeks.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// 1 - "Sunday",
        /// 2 - "Monday",
        /// 3 - "Tuesday",
        /// 4 - "Wednesday",
        /// 5 - "Thursday",
        /// 6 - "Friday",
        /// 7 - "Saturday".
        /// </value>
		[PXDBInt()]
		[PXDefault(7)]
		[PXUIField(DisplayName = "Periods Start Day of Week", Enabled = true)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7 }, new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" })]
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
		#region YearLastDayOfWeek
		public abstract class yearLastDayOfWeek : PX.Data.BQL.BqlInt.Field<yearLastDayOfWeek> { }
		protected int? _YearLastDayOfWeek;

        /// <summary>
        /// The day of the week of the last day of the financial year.
        /// Relevant ony for the <see cref="PeriodType">period types</see> based on weeks and
        /// if the <see cref="EndYearCalcMethod"/> is <b>not</b> <c>"CA"</c> - Last day of the financial year.
        /// </summary>
		[PXInt()]
		[PXUIField(DisplayName = "Day of Week", Enabled = false)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7 }, new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" })]
		public virtual int? YearLastDayOfWeek
		{
			get
			{
				return _EndYearDayOfWeek - 1 > 0 ? _EndYearDayOfWeek - 1 : _EndYearDayOfWeek - 1 + 7;
			}
			set
			{
				this._YearLastDayOfWeek = value;
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
		//attention - during regeneration they are erased
		#region AdditionalFields 
		
		#region AdjustToPeriodStart
		public abstract class adjustToPeriodStart : PX.Data.BQL.BqlBool.Field<adjustToPeriodStart> { };

        /// <summary>
        /// When set to <c>true</c>, tells the system to align the start and end dates of the periods with 
        /// the start and end dates of the corresponding calendar periods.
        /// Relevant only for Month (<c>"MO"</c>), Two Months (<c>"BM"</c>) and Quarter (<c>"QR"</c>) <see cref="PeriodType">period types.</see>
        /// </summary>
		[PXUIField(DisplayName ="Adjust to Period Start")]
		[PXBool]
		public bool? AdjustToPeriodStart
		{
			get { return _AdjustToPeriodStart; }
			set { if (value.HasValue) this._AdjustToPeriodStart = value.Value; }
		}
		protected bool _AdjustToPeriodStart = false;
		#endregion

		#region BelongsToNextYear
		public abstract class belongsToNextYear : PX.Data.BQL.BqlBool.Field<belongsToNextYear> { };

        /// <summary>
        /// When set to <c>true</c>, indicates that the system must set the <see cref="FirstFinYear">financial year</see> to the one,
        /// following the year of the <see cref="BegFinYear">selected start date of the year</see>.
        /// </summary>
        /// <value>
        /// For example, if the <see cref="BegFinYear">selected start date</see> is 10/28/2014 and this option is set to <c>true</c>,
        /// the <see cref="FirstFinYear"/> will be <c>2015</c>.
        /// If this option is set to <c>false</c>, <see cref="FirstFinYear"/> for the same date will be <c>2014</c>.
        /// </value>
		[PXUIField(DisplayName ="Belongs to Next Year")]
		[PXBool()]
		public bool? BelongsToNextYear
		{
			get { return _BelongsToNextYear; }
			set { if (value.HasValue) this._BelongsToNextYear = value.Value; }
		}
		protected bool? _BelongsToNextYear=null;
		#endregion

		#endregion

		#region Methods

        /// <summary>
        /// When <c>true</c>, indicates that the length of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">periods</see> of the year in days is fixed.
        /// Read-only field.
        /// </summary>
        /// <value>
        /// The value of this field is determined by the <see cref="PeriodType"/> field.
        /// </value>
		public bool IsFixedLengthPeriod
		{
			get
			{
				return FiscalPeriodSetupCreator.IsFixedLengthPeriod(this.FPType);
			}
		}

        /// <summary>
        /// Internal type of <see cref="PX.Objects.GL.Obsolete.FinPeriod">periods</see> of the year.
        /// Read-only field.
        /// </summary>
        /// <value>
        /// The value of this field is determined by the <see cref="PeriodType"/> field.
        /// </value>
		public FiscalPeriodSetupCreator.FPType FPType
		{
			get
			{
				return FinPeriodType.GetFPType(this.PeriodType);
			}
		} 
		#endregion
	}

	public class FinPeriodType
	{
		public const string Month = "MO";
		public const string BiMonth = "BM";
		public const string Quarter = "QR";
		public const string Week = "WK";
		public const string BiWeek = "BW";
		public const string FourWeek = "FW";
		public const string Decade = "DC";
		public const string FourFourFive = "FF";
		public const string FourFiveFour = "FI";
		public const string FiveFourFour = "IF";
		public const string CustomPeriodLength = "CL";
		public const string CustomPeriodsNumber = "CN";
		
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Month , BiMonth, Quarter, Week, BiWeek, FourWeek, FourFourFive, FourFiveFour, FiveFourFour, CustomPeriodsNumber},
				new string[] { Messages.PT_Month, Messages.PT_BiMonth, Messages.PT_Quarter, Messages.PT_Week, Messages.PT_BiWeek, Messages.PT_FourWeeks, Messages.PT_FourFourFive, Messages.PT_FourFiveFour, Messages.PT_FiveFourFour, Messages.PT_CustomPeriodsNumber}
				) { }
		}

		public static FiscalPeriodSetupCreator.FPType GetFPType(string aFinPeriodType) 
		{			
			switch (aFinPeriodType) 
			{
				case Month : return FiscalPeriodSetupCreator.FPType.Month; 
				case BiMonth: return FiscalPeriodSetupCreator.FPType.BiMonth;
				case Quarter: return FiscalPeriodSetupCreator.FPType.Quarter;
				case Week: return FiscalPeriodSetupCreator.FPType.Week;
				case BiWeek: return FiscalPeriodSetupCreator.FPType.BiWeek;
				case FourWeek: return FiscalPeriodSetupCreator.FPType.FourWeek;
				case Decade: return FiscalPeriodSetupCreator.FPType.Decade;
				case FourFourFive: return FiscalPeriodSetupCreator.FPType.FourFourFive;
				case FourFiveFour: return FiscalPeriodSetupCreator.FPType.FourFiveFour;
				case FiveFourFour: return FiscalPeriodSetupCreator.FPType.FiveFourFour;
				case CustomPeriodsNumber: return FiscalPeriodSetupCreator.FPType.Custom;					
				default: 
					throw new PXException(Messages.UnknownFinPeriodType);
			}
		}
	}
	public class EndYearMethod
	{
		public const string Calendar = "CA";
		public const string LastDay = "LD";
		public const string NearestDay = "ND";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Calendar, LastDay, NearestDay },
				new string[] { Messages.EndYearCalculation_Default, Messages.EndYearCalculation_LastDay, Messages.EndYearCalculation_ClosestDay }
				) { }
		}
	}
}
