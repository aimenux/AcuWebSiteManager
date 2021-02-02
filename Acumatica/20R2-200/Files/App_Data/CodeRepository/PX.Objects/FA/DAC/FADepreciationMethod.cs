using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FA
{
	#region FAAveragingConvention Attribute
	public class FAAveragingConvention
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new[] { null, FullPeriod, HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter, FullYear, HalfYear, FullDay },
				new[] { string.Empty, Messages.FullPeriod, Messages.HalfPeriod, Messages.NextPeriod, Messages.ModifiedPeriod, Messages.ModifiedPeriod2, Messages.FullQuarter, Messages.HalfQuarter, Messages.FullYear, Messages.HalfYear, Messages.FullDay })
			{ }
		}

		public static Dictionary<object, string[]> DeprMethodDisabledValues = new Dictionary<object, string[]>
		{
			{FADepreciationMethod.depreciationMethod.DecliningBalance, new[] {HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, FullYear}},
			{FADepreciationMethod.depreciationMethod.SumOfTheYearsDigits, new[] {HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter, FullYear, HalfYear}},
			{FADepreciationMethod.depreciationMethod.Dutch1, new[] {HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter, FullYear, HalfYear, FullDay}},
			{FADepreciationMethod.depreciationMethod.Dutch2, new[] {HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter, FullYear, HalfYear, FullDay}},
			{FADepreciationMethod.depreciationMethod.RemainingValueByPeriodLength, new[] {HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter, FullYear, HalfYear, FullPeriod }},
			{FADepreciationMethod.depreciationMethod.DecliningBalanceByPeriodLength, new[] {HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter, FullYear, HalfYear, FullPeriod }}

		};

		public static Dictionary<object, string[]> FixedLengthPeriodDisabledValues = new Dictionary<object, string[]>
		{
			{true, new[] {ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter}}
		};

		public static Dictionary<object, string[]> RecordTypeDisabledValues = new Dictionary<object, string[]>
		{
			{FARecordType.BothType, new[] {FullPeriod, HalfPeriod, NextPeriod, ModifiedPeriod, ModifiedPeriod2, FullQuarter, HalfQuarter, FullDay}}
		};

		public static void SetAveragingConventionsList<AveragingConventionField>(PXCache sender, object row, params KeyValuePair<object, Dictionary<object, string[]>>[] pars)
			where AveragingConventionField : IBqlField
		{
			Dictionary<string, string> fullDict = new ListAttribute().ValueLabelDic;
			foreach (KeyValuePair<object, Dictionary<object, string[]>> pair in pars)
			{
				object parValue = pair.Key;
				Dictionary<object, string[]> matrix = pair.Value;

				string[] matrixArray;
				if (parValue != null && matrix.TryGetValue(parValue, out matrixArray))
				{
					foreach (string val in matrixArray)
					{
						fullDict.Remove(val);
					}
				}
			}

			PXStringListAttribute.SetList<AveragingConventionField>(sender, row, fullDict.Keys.ToArray(), fullDict.Values.ToArray());
		}

		public const string FullPeriod = "FP";
		public const string HalfPeriod = "HP";
		public const string NextPeriod = "NP";
		public const string FullQuarter = "FQ";
		public const string HalfQuarter = "HQ";
		public const string FullYear = "FY";
		public const string HalfYear = "HY";
		public const string FullDay = "FD";
		public const string ModifiedPeriod = "MP";
		public const string ModifiedPeriod2 = "M2";


		public class fullPeriod : PX.Data.BQL.BqlString.Constant<fullPeriod>
		{
			public fullPeriod() : base(FullPeriod) {; }
		}
		public class halfPeriod : PX.Data.BQL.BqlString.Constant<halfPeriod>
		{
			public halfPeriod() : base(HalfPeriod) {; }
		}
		public class nextPeriod : PX.Data.BQL.BqlString.Constant<nextPeriod>
		{
			public nextPeriod() : base(NextPeriod) {; }
		}
		public class modifiedPeriod : PX.Data.BQL.BqlString.Constant<modifiedPeriod>
		{
			public modifiedPeriod() : base(ModifiedPeriod) {; }
		}
		public class modifiedPeriod2 : PX.Data.BQL.BqlString.Constant<modifiedPeriod2>
		{
			public modifiedPeriod2() : base(ModifiedPeriod2) {; }
		}
		public class fullQuarter : PX.Data.BQL.BqlString.Constant<fullQuarter>
		{
			public fullQuarter() : base(FullQuarter) {; }
		}
		public class halfQuarter : PX.Data.BQL.BqlString.Constant<halfQuarter>
		{
			public halfQuarter() : base(HalfQuarter) {; }
		}
		public class fullYear : PX.Data.BQL.BqlString.Constant<fullYear>
		{
			public fullYear() : base(FullYear) {; }
		}
		public class halfYear : PX.Data.BQL.BqlString.Constant<halfYear>
		{
			public halfYear() : base(HalfYear) {; }
		}
		public class fullDay : PX.Data.BQL.BqlString.Constant<fullDay>
		{
			public fullDay() : base(FullDay) {; }
		}
	}
	#endregion

	/// <exclude/>
	[Serializable]
	[PXPrimaryGraph(new Type[] {
		typeof(DepreciationMethodMaint),
		typeof(DepreciationTableMethodMaint)
	},
		new Type[] {
			typeof(Where<FADepreciationMethod.isTableMethod, Equal<False>>),
			typeof(Where<FADepreciationMethod.isTableMethod, Equal<True>>)
		})]
	[PXCacheName(Messages.DepreciationMethod)]
	public partial class FADepreciationMethod : IBqlTable
	{
		#region MethodID
		public abstract class methodID : PX.Data.BQL.BqlInt.Field<methodID> { }
		protected Int32? _MethodID;
		[PXDBIdentity()]
		public virtual Int32? MethodID
		{
			get
			{
				return this._MethodID;
			}
			set
			{
				this._MethodID = value;
			}
		}
		#endregion
		#region MethodCD
		public abstract class methodCD : PX.Data.BQL.BqlString.Field<methodCD> { }
		protected String _MethodCD;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<FADepreciationMethod.methodCD>), DescriptionField = typeof(FADepreciationMethod.description))]
		[PXUIField(DisplayName = "Depreciation Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PX.Data.EP.PXFieldDescription]
		public virtual String MethodCD
		{
			get
			{
				return this._MethodCD;
			}
			set
			{
				this._MethodCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
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
		#region ParentMethodID
		public abstract class parentMethodID : PX.Data.BQL.BqlInt.Field<parentMethodID> { }
		protected Int32? _ParentMethodID;
		[PXDBInt]
		[PXSelector(typeof(Search<FADepreciationMethod.methodID,
							Where<FADepreciationMethod.recordType, Equal<FARecordType.classType>>>),
					DescriptionField = typeof(FADepreciationMethod.description),
					SubstituteKey = typeof(FADepreciationMethod.methodCD))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Class Method", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual Int32? ParentMethodID
		{
			get
			{
				return this._ParentMethodID;
			}
			set
			{
				this._ParentMethodID = value;
			}
		}
		#endregion
		#region DepreciationMethod
		public abstract class depreciationMethod : PX.Data.BQL.BqlString.Field<depreciationMethod>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] 
					{
						StraightLine,
						DecliningBalance,
						SumOfTheYearsDigits,
						RemainingValue,
						Dutch1,
						Dutch2,
						RemainingValueByPeriodLength,
						DecliningBalanceByPeriodLength,
						AustralianPrimeCost,
						AustralianDiminishingValue,
						NewZealandStraightLine,
						NewZealandDiminishingValue,
						NewZealandStraightLineEvenly,
						NewZealandDiminishingValueEvenly,
					},
					new string[] 
					{
						Messages.StraightLine,
						Messages.DecliningBalance,
						Messages.SumOfTheYearsDigits,
						Messages.RemainingValue,
						Messages.Dutch1,
						Messages.Dutch2,
						Messages.RemainingValueByPeriodLength,
						Messages.DecliningBalanceByPeriodLength,
						Messages.AustralianPrimeCost,
						Messages.AustralianDiminishingValue,
						Messages.NewZealandStraightLine,
						Messages.NewZealandDiminishingValue,
						Messages.NewZealandStraightLineEvenly,
						Messages.NewZealandDiminishingValueEvenly,
					})
				{ }
			}

			public const string StraightLine = "SL";
			public const string DecliningBalance = "DB";
			public const string SumOfTheYearsDigits = "YD";
			public const string RemainingValue = "RV";
			public const string Dutch1 = "N1";
			public const string Dutch2 = "N2";
			public const string RemainingValueByPeriodLength = "RD";
			public const string DecliningBalanceByPeriodLength = "DP";
			public const string AustralianPrimeCost = "PC";
			public const string AustralianDiminishingValue = "DV";
			public const string NewZealandStraightLine = "ZL";
			public const string NewZealandDiminishingValue = "ZD";
			public const string NewZealandStraightLineEvenly = "LE";
			public const string NewZealandDiminishingValueEvenly = "DE";

			public class straightLine : PX.Data.BQL.BqlString.Constant<straightLine>
			{
				public straightLine() : base(StraightLine) { }
			}
			public class decliningBalance : PX.Data.BQL.BqlString.Constant<decliningBalance>
			{
				public decliningBalance() : base(DecliningBalance) { }
			}
			public class sumOfTheYearsDigits : PX.Data.BQL.BqlString.Constant<sumOfTheYearsDigits>
			{
				public sumOfTheYearsDigits() : base(SumOfTheYearsDigits) { }
			}
			public class remainingValue : PX.Data.BQL.BqlString.Constant<remainingValue>
			{
				public remainingValue() : base(RemainingValue) { }
			}
			public class dutch1 : PX.Data.BQL.BqlString.Constant<dutch1>
			{
				public dutch1() : base(Dutch1) { }
			}
			public class dutch2 : PX.Data.BQL.BqlString.Constant<dutch2>
			{
				public dutch2() : base(Dutch2) { }
			}
			public class remainingValueByPeriodLength : PX.Data.BQL.BqlString.Constant<remainingValueByPeriodLength>
			{
				public remainingValueByPeriodLength() : base(RemainingValueByPeriodLength) { }
			}
			public class decliningBalanceByPeriodLength : PX.Data.BQL.BqlString.Constant<decliningBalanceByPeriodLength>
			{
				public decliningBalanceByPeriodLength() : base(DecliningBalanceByPeriodLength) { }
			}
			public class australianPrimeCost : PX.Data.BQL.BqlString.Constant<australianPrimeCost>
			{
				public australianPrimeCost() : base(AustralianPrimeCost) { }
			}
			public class australianDiminishingValue : PX.Data.BQL.BqlString.Constant<australianDiminishingValue>
			{
				public australianDiminishingValue() : base(AustralianDiminishingValue) { }
			}
			public class newZealandStraightLine : PX.Data.BQL.BqlString.Constant<newZealandStraightLine>
			{
				public newZealandStraightLine() : base(NewZealandStraightLine) { }
			}
			public class newZealandDiminishingValue : PX.Data.BQL.BqlString.Constant<newZealandDiminishingValue>
			{
				public newZealandDiminishingValue() : base(NewZealandDiminishingValue) { }
			}
			public class newZealandStraightLineEvenly : PX.Data.BQL.BqlString.Constant<newZealandStraightLineEvenly>
			{
				public newZealandStraightLineEvenly() : base(NewZealandStraightLineEvenly) { }
			}
			public class newZealandDiminishingValueEvenly : PX.Data.BQL.BqlString.Constant<newZealandDiminishingValueEvenly>
			{
				public newZealandDiminishingValueEvenly() : base(NewZealandDiminishingValueEvenly) { }
			}
			#endregion
		}
		protected String _DepreciationMethod;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Calculation Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(depreciationMethod.StraightLine)]
		[depreciationMethod.List]
		public virtual String DepreciationMethod
		{
			get
			{
				return this._DepreciationMethod;
			}
			set
			{
				this._DepreciationMethod = value;
			}
		}
		#endregion
		#region AveragingConvention
		public abstract class averagingConvention : PX.Data.BQL.BqlString.Field<averagingConvention> { }
		protected String _AveragingConvention;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(FAAveragingConvention.FullPeriod, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Averaging Convention", Visibility = PXUIVisibility.SelectorVisible)]
		[FAAveragingConvention.List()]
		[PXFormula(typeof(Switch<
			Case2<
				Where<depreciationMethod.IsEqual<depreciationMethod.australianPrimeCost>
					.Or<depreciationMethod.IsEqual<depreciationMethod.australianDiminishingValue>>>,
					FAAveragingConvention.fullDay,
			Case2<Where<depreciationMethod.IsEqual<depreciationMethod.newZealandStraightLine>
					.Or<depreciationMethod.IsEqual<depreciationMethod.newZealandDiminishingValue>
					.Or<depreciationMethod.IsEqual<depreciationMethod.newZealandStraightLineEvenly>
					.Or<depreciationMethod.IsEqual<depreciationMethod.newZealandDiminishingValueEvenly>>>>>,
					FAAveragingConvention.fullPeriod>>,
					averagingConvention >))]
		[PXUIEnabled(typeof(Where<depreciationMethod.IsNotEqual<depreciationMethod.australianPrimeCost>
					.And<depreciationMethod.IsNotEqual<depreciationMethod.australianDiminishingValue>>
					.And<depreciationMethod.IsNotEqual<depreciationMethod.newZealandStraightLine>>
					.And<depreciationMethod.IsNotEqual<depreciationMethod.newZealandDiminishingValue>>
					.And<depreciationMethod.IsNotEqual<depreciationMethod.newZealandStraightLineEvenly>>
					.And<depreciationMethod.IsNotEqual<depreciationMethod.newZealandDiminishingValueEvenly>>>))]
		public virtual String AveragingConvention
		{
			get
			{
				return this._AveragingConvention;
			}
			set
			{
				this._AveragingConvention = value;
			}
		}
		#endregion
		#region RecoveryPeriod
		public abstract class recoveryPeriod : PX.Data.BQL.BqlInt.Field<recoveryPeriod> { }
		protected int? _RecoveryPeriod;
		[PXDBInt]
		[PXUIField(DisplayName = "Recovery Period", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual int? RecoveryPeriod
		{
			get
			{
				return this._RecoveryPeriod;
			}
			set
			{
				this._RecoveryPeriod = value;
			}
		}
		#endregion
		#region DisplayTotalPercents
		public abstract class displayTotalPercents : PX.Data.BQL.BqlDecimal.Field<displayTotalPercents> { }
		[PXDecimal(3)]
		[PXFormula(typeof(Mult<totalPercents, decimal100>))]
		[PXUIField(DisplayName = "Total Percent", Required = true, Enabled = false)]
		public virtual decimal? DisplayTotalPercents { get; set; }
		#endregion
		#region TotalPercents
		public abstract class totalPercents : PX.Data.BQL.BqlDecimal.Field<totalPercents> { }
		protected Decimal? _TotalPercents;
		[PXDBDecimal(5)]
		public virtual Decimal? TotalPercents
		{
			get
			{
				return this._TotalPercents;
			}
			set
			{
				this._TotalPercents = value;
			}
		}
		#endregion
		#region AveragingConvPeriod
		public abstract class averagingConvPeriod : PX.Data.BQL.BqlShort.Field<averagingConvPeriod> { }
		protected Int16? _AveragingConvPeriod;
		[PXDBShort]
		[PXDefault((short)1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Convention Period", Required = true)]
		public virtual Int16? AveragingConvPeriod
		{
			get
			{
				return this._AveragingConvPeriod;
			}
			set
			{
				this._AveragingConvPeriod = value;
			}
		}
		#endregion
		#region DepreciationPeriodsInYear
		public abstract class depreciationPeriodsInYear : PX.Data.BQL.BqlShort.Field<depreciationPeriodsInYear> { }
		protected Int16? _DepreciationPeriodsInYear;
		[PXUIField(DisplayName = "Depreciation periods in Year")]
		[PXShort(MinValue = 1, MaxValue = 366)]
		public virtual Int16? DepreciationPeriodsInYear
		{
			get
			{
				return this._DepreciationPeriodsInYear;
			}
			set
			{
				this._DepreciationPeriodsInYear = value;
			}
		}
		#endregion
		#region DepreciationStartDate
		public abstract class depreciationStartDate : PX.Data.BQL.BqlDateTime.Field<depreciationStartDate> { }
		protected DateTime? _DepreciationStartDate;
		[PXUIField(DisplayName = "Depreciation Start Date")]
		[PXDate()]
		public virtual DateTime? DepreciationStartDate
		{
			get
			{
				return this._DepreciationStartDate;
			}
			set
			{
				this._DepreciationStartDate = value;
			}
		}
		#endregion
		#region DepreciationStopDate
		public abstract class depreciationStopDate : PX.Data.BQL.BqlDateTime.Field<depreciationStopDate> { }
		protected DateTime? _DepreciationStopDate;
		[PXUIField(DisplayName = "Depreciation Stop Date")]
		[PXDate()]
		public virtual DateTime? DepreciationStopDate
		{
			get
			{
				return this._DepreciationStopDate;
			}
			set
			{
				this._DepreciationStopDate = value;
			}
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXInt()]
		[PXSelector(typeof(FABook.bookID), SubstituteKey = typeof(FABook.bookCode), DescriptionField = typeof(FABook.description))]
		[PXUIField(DisplayName = "Book")]
		public virtual Int32? BookID
		{
			get
			{
				return this._BookID;
			}
			set
			{
				this._BookID = value;
			}
		}
		#endregion
		#region RecordType
		public abstract class recordType : PX.Data.BQL.BqlString.Field<recordType> { }
		protected String _RecordType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Record Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(FARecordType.AssetType, PersistingCheck = PXPersistingCheck.Nothing)]
		[FARecordType.MethodList]
		public virtual String RecordType
		{
			get
			{
				return this._RecordType;
			}
			set
			{
				this._RecordType = value;
			}
		}
		#endregion
		#region UsefulLife
		public abstract class usefulLife : PX.Data.BQL.BqlDecimal.Field<usefulLife> { }
		protected Decimal? _UsefulLife;
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Useful Life, Years", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? UsefulLife
		{
			get
			{
				return this._UsefulLife;
			}
			set
			{
				this._UsefulLife = value;
			}
		}
		#endregion
		#region IsTableMethod
		public abstract class isTableMethod : PX.Data.BQL.BqlBool.Field<isTableMethod> { }
		protected Boolean? _IsTableMethod;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Is Table Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Boolean? IsTableMethod
		{
			get
			{
				return this._IsTableMethod;
			}
			set
			{
				this._IsTableMethod = value;
			}
		}
		#endregion
		#region IsPredefined
		public abstract class isPredefined : PX.Data.BQL.BqlBool.Field<isPredefined> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? IsPredefined { get; set; }
		#endregion
		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Predefined, Custom },
					new string[] { Messages.Predefined, Messages.Custom })
				{ }
			}

			public const string Predefined = "P";
			public const string Custom = "C";

			public class predefined : PX.Data.BQL.BqlString.Constant<predefined>
			{
				public predefined() : base(Predefined) { }
			}
			public class custom : PX.Data.BQL.BqlString.Constant<custom>
			{
				public custom() : base(Custom) { }
			}
		}

		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		[source.List]
		[PXFormula(typeof(Switch<Case<Where<isPredefined, Equal<True>>, source.predefined>, source.custom>))]
		public virtual string Source { get; set; }

		#endregion

		#region YearlyAccountancy
		public abstract class yearlyAccountancy : PX.Data.BQL.BqlBool.Field<yearlyAccountancy> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXFormula(typeof(Switch<
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.sumOfTheYearsDigits>>, True,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.australianDiminishingValue>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.dutch1>>, False>>>>>>>>,
			FADepreciationMethod.yearlyAccountancy>))]
		[PXUIEnabled(typeof(Where<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.sumOfTheYearsDigits>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.dutch1>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.australianDiminishingValue>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>
			>>>>>>>>))]
		[PXUIField(DisplayName = "Yearly Accountancy", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? YearlyAccountancy { get; set; }
		#endregion
		#region DBMultiPlier
		public abstract class dBMultiPlier : PX.Data.BQL.BqlDecimal.Field<dBMultiPlier> { }
		protected Decimal? _DBMultiPlier;
		[PercentDBDecimal]
		[PXUIField(DisplayName = "DB Multiplier")]
		[PXFormula(typeof(Switch<
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>>, decimal0,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.australianDiminishingValue>>, decimal0,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>>, decimal0,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>>, decimal0,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>>, decimal0,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>, decimal0>>>>>>,
			FADepreciationMethod.dBMultiPlier>))]
		[PXUIEnabled(typeof(Where<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.australianDiminishingValue>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>
			>>>>>>))]
		public virtual Decimal? DBMultiPlier
		{
			get
			{
				return this._DBMultiPlier;
			}
			set
			{
				this._DBMultiPlier = value;
			}
		}
		#endregion
		#region SwitchToSL
		public abstract class switchToSL : PX.Data.BQL.BqlBool.Field<switchToSL> { }
		protected Boolean? _SwitchToSL;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Switch to SL", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Switch<
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.australianDiminishingValue>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>>, False,
			Case<Where<FADepreciationMethod.depreciationMethod, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>, False>>>>>>,
			FADepreciationMethod.switchToSL>))]
		[PXUIEnabled(typeof(Where<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.australianDiminishingValue>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
			And<FADepreciationMethod.depreciationMethod, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>
			>>>>>>))]
		public virtual Boolean? SwitchToSL
		{
			get
			{
				return this._SwitchToSL;
			}
			set
			{
				this._SwitchToSL = value;
			}
		}
		#endregion
		#region PercentPerYear
		public abstract class percentPerYear : PX.Data.BQL.BqlDecimal.Field<percentPerYear> { }
		[PXDBDecimal(4, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Percent per Year")]
		public virtual decimal? PercentPerYear { get; set; }
		#endregion


		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(FADepreciationMethod.methodCD))]
		public virtual Guid? NoteID { get; set; }

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

		public bool IsPureStraightLine => IsTableMethod != true && (DepreciationMethod == depreciationMethod.StraightLine);
		public bool IsYearlyAccountancyTableMethod => IsTableMethod == true && YearlyAccountancy == true;
		public bool IsNewZealandMethod => (DepreciationMethod == depreciationMethod.NewZealandStraightLine
											|| DepreciationMethod == depreciationMethod.NewZealandStraightLineEvenly
											|| DepreciationMethod == depreciationMethod.NewZealandDiminishingValue
											|| DepreciationMethod == depreciationMethod.NewZealandDiminishingValueEvenly);
	}
}
