using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;


namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.BookCalendar)]
	public partial class FABookYearSetup : IBqlTable, IYearSetup
	{
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected int? _BookID;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXSelector(typeof(Search<FABook.bookID, Where<FABook.updateGL, Equal<False>>>),
					SubstituteKey = typeof(FABook.bookCode),
					DescriptionField = typeof(FABook.description))]
		[PXUIField(DisplayName = "Book")]
		[PXFieldDescription]
		[PXParent(typeof(Select<FABook, Where<FABook.bookID, Equal<Current<FABookYearSetup.bookID>>>>))]
		public virtual int? BookID
		{
			get
			{
				return _BookID;
			}
			set
			{
				_BookID = value;
			}
		}
		#endregion
		#region FirstFinYear
		public abstract class firstFinYear : PX.Data.BQL.BqlString.Field<firstFinYear> { }
		protected String _FirstFinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "First Year", Enabled = false)]
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
		public abstract class begFinYear : PX.Data.BQL.BqlDateTime.Field<begFinYear> { }
		protected DateTime? _BegFinYear;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
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
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods> { }
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
		public abstract class userDefined : PX.Data.BQL.BqlBool.Field<userDefined> { }
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

		[PXNote(DescriptionField = typeof(FABookYearSetup.bookID))]
		public virtual Guid? NoteID { get; set; }

		#endregion
		#region PeriodType
		public abstract class periodType : PX.Data.BQL.BqlString.Field<periodType> { }
		protected string _PeriodType;
		[PXDBString(2, IsFixed = true)]
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
		#region PeriodLength
		public abstract class periodLength : PX.Data.BQL.BqlShort.Field<periodLength> { }
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
		public abstract class periodsStartDate : PX.Data.BQL.BqlDateTime.Field<periodsStartDate> { }
		protected DateTime? _PeriodsStartDate;
		[PXDBDate()]
		[PXDefault(typeof(FABookYearSetup.begFinYear))]
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
		#region HasAdjustmentPeriod
		public abstract class hasAdjustmentPeriod : PX.Data.BQL.BqlBool.Field<hasAdjustmentPeriod> { }
		protected bool _HasAdjustmentPeriod = false;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Has Adjustment Period")]
		public bool? HasAdjustmentPeriod
		{
			get { return this._HasAdjustmentPeriod; }
			set { if (value.HasValue) this._HasAdjustmentPeriod = value.Value; }
		}

		#endregion
		#region EndYearCalcMethod
		public abstract class endYearCalcMethod : PX.Data.BQL.BqlString.Field<endYearCalcMethod> { }
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
		public abstract class endYearDayOfWeek : PX.Data.BQL.BqlInt.Field<endYearDayOfWeek> { }
		protected int? _EndYearDayOfWeek;
		[PXDBInt()]
		[PXDefault(7)]
		[PXUIField(DisplayName = "Day Of Week", Enabled = true)]
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
		#region BelongsToNextYear
		public abstract class belongsToNextYear : PX.Data.BQL.BqlBool.Field<belongsToNextYear> { };
		[PXUIField(DisplayName = "Belongs To Next Year")]
		[PXBool()]
		public bool? BelongsToNextYear
		{
			get { return _BelongsToNextYear; }
			set { if (value.HasValue) this._BelongsToNextYear = value.Value; }
		}
		protected bool? _BelongsToNextYear = null;
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
		public abstract class fPType : PX.Data.IBqlField { }

		public FiscalPeriodSetupCreator.FPType FPType
		{
			[PXDependsOnFields(typeof(periodType))]
			get
			{
				return FinPeriodType.GetFPType(this.PeriodType);
			}
		}

		#endregion
	}
}