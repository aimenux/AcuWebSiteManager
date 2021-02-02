using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.Common;
using PX.Data.BQL.Fluent;

namespace PX.Objects.FA
{
	[Serializable]
	[PXProjection(typeof(Select2<FABookBalance,
		LeftJoin<FABookHistory, On<FABookHistory.assetID, Equal<FABookBalance.assetID>,
		And<FABookHistory.bookID, Equal<FABookBalance.bookID>,
		And<FABookHistory.finPeriodID, Equal<IsNull<FABookBalance.currDeprPeriod, FABookBalance.lastPeriod>>>>>,
		InnerJoin<FABook, On<FABook.bookID, Equal<FABookBalance.bookID>>>>>), new Type[] { typeof(FABookBalance) })]
	[PXCacheName(Messages.FABookBalance)]
	public partial class FABookBalance : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
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
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FABookBalance.assetID>>>>))]
		[PXSelector(typeof(Search<FixedAsset.assetID>),
			SubstituteKey = typeof(FixedAsset.assetCD), DirtyRead = true, DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Fixed Asset", Enabled = false)]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		protected Int32? _ClassID;
		[PXInt()]
		[PXDBScalar(typeof(Search<FixedAsset.classID, Where<FixedAsset.assetID, Equal<FABookBalance.assetID>>>))]
		[PXDefault(typeof(Search<FixedAsset.classID, Where<FixedAsset.assetID, Equal<Current<FABookBalance.assetID>>>>))]
		[PXSelector(typeof(Search<FixedAsset.assetID>), SubstituteKey = typeof(FixedAsset.assetCD), CacheGlobal = true, DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Asset Class", Enabled = false)]
		public virtual Int32? ClassID
		{
			get
			{
				return this._ClassID;
			}
			set
			{
				this._ClassID = value;
			}
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXSelector(typeof(Search2<FABook.bookID,
								InnerJoin<FABookSettings, On<FABookSettings.bookID, Equal<FABook.bookID>>>,
								Where<FABookSettings.assetID, Equal<Current<FixedAsset.classID>>>>),
					SubstituteKey = typeof(FABook.bookCode),
					DescriptionField = typeof(FABook.description), CacheGlobal = true)]
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
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[FixedAssetStatus.List()]
		[PXDefault(FixedAssetStatus.Active)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region UpdateGL
		public abstract class updateGL : PX.Data.BQL.BqlBool.Field<updateGL> { }
		protected Boolean? _UpdateGL;
		[PXDBBool(BqlField = typeof(FABook.updateGL))]
		[PXDefault(false, typeof(Search<FABook.updateGL, Where<FABook.bookID, Equal<Current<FABookBalance.bookID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Posting Book", Enabled = false)]
		public virtual Boolean? UpdateGL
		{
			get
			{
				return this._UpdateGL;
			}
			set
			{
				this._UpdateGL = value;
			}
		}
		#endregion
		#region Depreciate
		public abstract class depreciate : PX.Data.BQL.BqlBool.Field<depreciate> { }
		protected Boolean? _Depreciate;
		[PXDBBool]
		[PXDefault(typeof(Search<FixedAsset.depreciable, Where<FixedAsset.assetID, Equal<Current<FABookBalance.assetID>>>>))]
		[PXUIField(DisplayName = "Depreciate", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Boolean? Depreciate
		{
			get
			{
				return this._Depreciate;
			}
			set
			{
				this._Depreciate = value;
			}
		}
		#endregion
		#region AveragingConvention
		public abstract class averagingConvention : PX.Data.BQL.BqlString.Field<averagingConvention> { }
		protected String _AveragingConvention;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Averaging Convention")]
		[FAAveragingConvention.List()]
		[PXDefault(typeof(Search<FABookSettings.averagingConvention,
								Where<FABookSettings.bookID, Equal<Current<FABookBalance.bookID>>,
								  And<FABookSettings.assetID, Equal<Current<FABookBalance.classID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(FABookBalance.depreciate))]
		[PXUIEnabled(typeof(Switch<Case2<
			Where<FABookBalance.depreciate, Equal<True>, 
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.australianDiminishingValue>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>>>>>>>, True>, False>))]
		[PXFormula(typeof(Switch<
			Case2<
				Where<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianDiminishingValue>>>,
					FAAveragingConvention.fullDay,
			Case2<
				Where<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>>>>,
					FAAveragingConvention.fullPeriod>>,
					averagingConvention>))]
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
		#region PercentPerYear
		public abstract class percentPerYear : PX.Data.BQL.BqlDecimal.Field<percentPerYear> { }
		[PXDBDecimal(4, MinValue = 0, MaxValue = 100)]
		[PXDefault(typeof(Coalesce<
			Search<FABookSettings.percentPerYear, 
				Where<FABookSettings.bookID, Equal<Current<FABookBalance.bookID>>, 
					And<FABookSettings.assetID, Equal<Current<FABookBalance.classID>>>>>,
			SearchFor<FADepreciationMethod.percentPerYear>
				.In<SelectFrom<FADepreciationMethod>
					.Where<FADepreciationMethod.methodID.IsEqual<FABookBalance.depreciationMethodID.FromCurrent>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Percent per Year")]
		[PXUIEnabled(typeof(Switch<Case2<
			Where<FABookBalance.depreciate, Equal<True>, 
			And<Where<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianDiminishingValue>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>>>>>>>>, True>, False>))]
		[UndefaultFormula(typeof(Switch<
			Case2<Where<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
				Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianDiminishingValue>,
				Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
				Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
				Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
				Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>>>>>>,
			DefaultValue<percentPerYear>>>))]
		public virtual decimal? PercentPerYear
		{
			get;
			set;
		}
		#endregion
		#region UsefulLife
		public abstract class usefulLife : PX.Data.BQL.BqlDecimal.Field<usefulLife> { }
		protected Decimal? _UsefulLife;
		[PXDBDecimal(4)]
		[PXDefault(typeof(Search<FABookSettings.usefulLife, Where<FABookSettings.bookID, Equal<Current<FABookBalance.bookID>>, And<FABookSettings.assetID, Equal<Current<FABookBalance.classID>>>>>))]
		[PXUIField(DisplayName = "Useful Life, Years")]
		[PXUIRequired(typeof(FABookBalance.depreciate))]
		[PXUIEnabled(typeof(Switch<Case2<
			Where<FABookBalance.depreciate, Equal<True>, 
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>>>>>, True>, False>))]
		[PXFormula(typeof(Switch<
			Case2<Where<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>>>>,
				Div<decimal100, percentPerYear>>, 
				usefulLife>))]
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
		#region MidMonthType
		public abstract class midMonthType : PX.Data.BQL.BqlString.Field<midMonthType> { }
		protected string _MidMonthType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Mid-Period Type", Enabled = false)]
		[PXDefault(FABook.midMonthType.FixedDay, typeof(Search<FABookSettings.midMonthType,
			Where<FABookSettings.bookID, Equal<Current<FABookBalance.bookID>>,
				 And<FABookSettings.assetID, Equal<Current<FABookBalance.classID>>>>>))]
		[FABook.midMonthType.List]
		[PXUIRequired(typeof(FABookSettings.midMonthType.IsRequired<FABookBalance.depreciate, FABookBalance.averagingConvention>))]
		[PXUIEnabled(typeof(FABookSettings.midMonthType.IsRequired<FABookBalance.depreciate, FABookBalance.averagingConvention>))]
		public virtual string MidMonthType
		{
			get
			{
				return this._MidMonthType;
			}
			set
			{
				this._MidMonthType = value;
			}
		}
		#endregion
		#region MidMonthDay
		public abstract class midMonthDay : PX.Data.BQL.BqlShort.Field<midMonthDay> { }
		protected short? _MidMonthDay;
		[PXDBShort]
		[PXDefault(typeof(Search<FABookSettings.midMonthDay,
			Where<FABookSettings.bookID, Equal<Current<FABookBalance.bookID>>,
				 And<FABookSettings.assetID, Equal<Current<FABookBalance.classID>>>>>))]
		[PXUIField(DisplayName = "Mid-Period Day")]
		[PXUIRequired(typeof(FABookSettings.midMonthType.IsRequired<FABookBalance.depreciate, FABookBalance.averagingConvention>))]
		[PXUIEnabled(typeof(FABookSettings.midMonthType.IsRequired<FABookBalance.depreciate, FABookBalance.averagingConvention>))]
		public virtual short? MidMonthDay
		{
			get
			{
				return this._MidMonthDay;
			}
			set
			{
				this._MidMonthDay = value;
			}
		}
		#endregion
		#region ADSLife
		public abstract class aDSLife : PX.Data.BQL.BqlDecimal.Field<aDSLife> { }
		protected Decimal? _ADSLife;
		[PXDBDecimal(2)]
		[PXFormula(typeof(usefulLife))]
		[PXUIField(DisplayName = "ADS Life, Years")]
		public virtual Decimal? ADSLife
		{
			get
			{
				return this._ADSLife;
			}
			set
			{
				this._ADSLife = value;
			}
		}
		#endregion
		#region AcquisitionCost
		public abstract class acquisitionCost : PX.Data.BQL.BqlDecimal.Field<acquisitionCost> { }
		protected Decimal? _AcquisitionCost;
		[PXDBBaseCury()]
		[PXDefault(typeof(Search<FADetails.acquisitionCost, Where<FADetails.assetID, Equal<Current<FABookBalance.assetID>>>>))]
		[PXUIField(DisplayName = "Orig. Acquisition Cost")]
		public virtual Decimal? AcquisitionCost
		{
			get
			{
				return this._AcquisitionCost;
			}
			set
			{
				this._AcquisitionCost = value;
			}
		}
		#endregion
		#region SalvageAmount
		public abstract class salvageAmount : PX.Data.BQL.BqlDecimal.Field<salvageAmount> { }
		protected Decimal? _SalvageAmount;
		[PXDBBaseCury()]
		[PXDefault(typeof(Search<FADetails.salvageAmount, Where<FADetails.assetID, Equal<Current<FABookBalance.assetID>>>>))]
		[PXUIField(DisplayName = "Salvage Amount")]
		public virtual Decimal? SalvageAmount
		{
			get
			{
				return this._SalvageAmount;
			}
			set
			{
				this._SalvageAmount = value;
			}
		}
		#endregion
		#region BusinessUse
		public abstract class businessUse : PX.Data.BQL.BqlDecimal.Field<businessUse> { }
		protected Decimal? _BusinessUse;
		[PXDBDecimal(4, MinValue = 0.0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Business Use, %")]
		public virtual Decimal? BusinessUse
		{
			get
			{
				return this._BusinessUse;
			}
			set
			{
				this._BusinessUse = value;
			}
		}
		#endregion
		#region BonusID
		public abstract class bonusID : PX.Data.BQL.BqlInt.Field<bonusID> { }
		protected Int32? _BonusID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Bonus")]
		[PXSelector(typeof(Search2<FABonus.bonusID,
									CrossJoin<FABookSettings>,
									Where<FABookSettings.bookID, Equal<Current<FABookBalance.bookID>>,
									  And<FABookSettings.assetID, Equal<Current<FABookBalance.classID>>,
									  And<FABookSettings.bonus, Equal<True>>>>>),
					typeof(FABonus.bonusCD),
					SubstituteKey = typeof(FABonus.bonusCD),
					DescriptionField = typeof(FABonus.description))]
		public virtual Int32? BonusID
		{
			get
			{
				return this._BonusID;
			}
			set
			{
				this._BonusID = value;
			}
		}
		#endregion
		#region BonusRate
		public abstract class bonusRate : PX.Data.BQL.BqlDecimal.Field<bonusRate> { }
		protected Decimal? _BonusRate;
		[PXDBDecimal(6, MinValue = 0.0, MaxValue = 100.0)]
		[PXUIField(DisplayName = "Bonus Rate")]
		[PXFormula(typeof(GetBonusRate<deprFromDate, bonusID>))]
		public virtual Decimal? BonusRate
		{
			get
			{
				return _BonusRate;
			}
			set
			{
				_BonusRate = value;
			}
		}
		#endregion
		#region BonusAmount
		public abstract class bonusAmount : PX.Data.BQL.BqlDecimal.Field<bonusAmount> { }
		protected Decimal? _BonusAmount;
		[PXDBBaseCury()]
		[PXFormula(typeof(Mult<Sub<Mult<acquisitionCost, Div<businessUse, decimal100>>, tax179Amount>, Div<bonusRate, decimal100>>))]
		[PXUIField(DisplayName = "Bonus Amount")]
		public virtual Decimal? BonusAmount
		{
			get
			{
				return this._BonusAmount;
			}
			set
			{
				this._BonusAmount = value;
			}
		}
		#endregion
		#region Tax179Amount
		public abstract class tax179Amount : PX.Data.BQL.BqlDecimal.Field<tax179Amount> { }
		protected Decimal? _Tax179Amount;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax 179 Amount")]
		public virtual Decimal? Tax179Amount
		{
			get
			{
				return this._Tax179Amount;
			}
			set
			{
				this._Tax179Amount = value;
			}
		}
		#endregion
		#region DeprFromDate
		public abstract class deprFromDate : PX.Data.BQL.BqlDateTime.Field<deprFromDate> { }
		protected DateTime? _DeprFromDate;
		[PXDBDate]
		[PXUIField(DisplayName = Messages.DeprFromDate)]
		[PXDefault(typeof(Search<FADetails.depreciateFromDate, Where<FADetails.assetID, Equal<Current<assetID>>>>))]
		public virtual DateTime? DeprFromDate
		{
			get
			{
				return this._DeprFromDate;
			}
			set
			{
				this._DeprFromDate = value;
			}
		}
		#endregion
		#region DeprFromPeriod
		public abstract class deprFromPeriod : PX.Data.BQL.BqlString.Field<deprFromPeriod> { }
		protected String _DeprFromPeriod;
		[PXUIField(DisplayName = Messages.DeprFromPeriod, Enabled = false)]
		[FABookPeriodSelector(
			assetSourceType: typeof(FABookBalance.assetID),
			bookSourceType: typeof(FABookBalance.bookID))]
		[PXFormula(typeof(RecoveryStartPeriod<deprFromDate, bookID, depreciationMethodID, averagingConvention, midMonthType, midMonthDay>))]
		public virtual String DeprFromPeriod
		{
			get
			{
				return this._DeprFromPeriod;
			}
			set
			{
				this._DeprFromPeriod = value;
			}
		}
		#endregion
		#region DeprFromYear
		public abstract class deprFromYear : PX.Data.BQL.BqlString.Field<deprFromYear> { }
		protected String _DeprFromYear;
		[PXString(4, IsFixed = true)]
		[PXFormula(typeof(Substring<FABookBalance.deprFromPeriod, int0, int4>))]
		public virtual String DeprFromYear
		{
			get
			{
				return _DeprFromYear;
			}
			set
			{
				_DeprFromYear = value;
			}
		}
		#endregion
		#region DepreciationMethodID
		public abstract class depreciationMethodID : PX.Data.BQL.BqlInt.Field<depreciationMethodID> { }
		protected Int32? _DepreciationMethodID;
		[PXDBInt]
		[PXSelector(typeof(Search<FADepreciationMethod.methodID,
			Where2<Where<Current2<usefulLife>, IsNull,
				Or<FADepreciationMethod.usefulLife, IsNull,
				Or<FADepreciationMethod.usefulLife, Equal<Current2<usefulLife>>,
				Or<FADepreciationMethod.usefulLife, Equal<decimal0>>>>>,
			And<Where<FADepreciationMethod.recordType, NotEqual<FARecordType.classType>>>>>),
					typeof(FADepreciationMethod.methodCD),
					typeof(FADepreciationMethod.depreciationMethod),
					typeof(FADepreciationMethod.usefulLife),
					typeof(FADepreciationMethod.recoveryPeriod),
					typeof(FADepreciationMethod.averagingConvention),
					typeof(FADepreciationMethod.recordType),
					typeof(FADepreciationMethod.description),
					SubstituteKey = typeof(FADepreciationMethod.methodCD),
					DescriptionField = typeof(FADepreciationMethod.description)
			)]
		[PXFormula(typeof(SelectDepreciationMethod<deprFromDate, classID, bookID, assetID>))]
		[PXUIField(DisplayName = "Depreciation Method")]
		[PXDefault]
		[PXUIRequired(typeof(FABookBalance.depreciate))]
		[PXUIEnabled(typeof(FABookBalance.depreciate))]
		public virtual Int32? DepreciationMethodID
		{
			get
			{
				return _DepreciationMethodID;
			}
			set
			{
				_DepreciationMethodID = value;
			}
		}
		#endregion
		#region RecoveryPeriod
		public abstract class recoveryPeriod : PX.Data.BQL.BqlInt.Field<recoveryPeriod> { }
		protected Int32? _RecoveryPeriod;
		[PXDBInt(MinValue = 1)]
		[PXUIField(DisplayName = "Recovery Periods", Visible = false)]
		public virtual Int32? RecoveryPeriod
		{
			get
			{
				return _RecoveryPeriod;
			}
			set
			{
				_RecoveryPeriod = value;
			}
		}
		#endregion
		#region DeprToDate
		public abstract class deprToDate : PX.Data.BQL.BqlDateTime.Field<deprToDate> { }
		protected DateTime? _DeprToDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Depr. to")]
		public virtual DateTime? DeprToDate
		{
			get
			{
				return this._DeprToDate;
			}
			set
			{
				this._DeprToDate = value;
			}
		}
		#endregion
		#region DeprToPeriod
		public abstract class deprToPeriod : PX.Data.BQL.BqlString.Field<deprToPeriod> { }
		protected String _DeprToPeriod;
		[PXUIField(DisplayName = "Depr. to Period", Enabled = false)]
		[FABookPeriodSelector(
			assetSourceType: typeof(assetID),
			bookSourceType: typeof(bookID))]
		[PXFormula(typeof(Switch<Case<Where<depreciate, Equal<True>>, OffsetBookDateToPeriod<deprFromDate, bookID, assetID, depreciationMethodID, averagingConvention, usefulLife>>>))]
		public virtual String DeprToPeriod
		{
			get
			{
				return _DeprToPeriod;
			}
			set
			{
				_DeprToPeriod = value;
			}
		}
		#endregion
		#region DeprToYear
		public abstract class deprToYear : PX.Data.BQL.BqlString.Field<deprToYear> { }
		protected String _DeprToYear;
		[PXString(4, IsFixed = true)]
		[PXFormula(typeof(Substring<FABookBalance.deprToPeriod, int0, int4>))]
		public virtual String DeprToYear
		{
			get
			{
				return _DeprToYear;
			}
			set
			{
				_DeprToYear = value;
			}
		}
		#endregion
		#region LastDeprPeriod
		public abstract class lastDeprPeriod : PX.Data.BQL.BqlString.Field<lastDeprPeriod> { }
		protected String _LastDeprPeriod;
		[PXUIField(DisplayName = "Last Depr. Period", Enabled = false)]
		[FABookPeriodSelector(
			assetSourceType: typeof(FABookBalance.assetID),
			bookSourceType: typeof(FABookBalance.bookID))]
		public virtual String LastDeprPeriod
		{
			get
			{
				return this._LastDeprPeriod;
			}
			set
			{
				this._LastDeprPeriod = value;
			}
		}
		#endregion
		#region CurrDeprPeriod
		public abstract class currDeprPeriod : PX.Data.BQL.BqlString.Field<currDeprPeriod> { }
		protected String _CurrDeprPeriod;
		[PXDBString(6, IsFixed = true)]
		[PXUIField(DisplayName = "Current Period", Enabled = false)]
		[GL.FinPeriodIDFormatting()]
		public virtual String CurrDeprPeriod
		{
			get
			{
				return this._CurrDeprPeriod;
			}
			set
			{
				this._CurrDeprPeriod = value;
			}
		}
		#endregion
		#region InitPeriod
		public abstract class initPeriod : PX.Data.BQL.BqlString.Field<initPeriod> { }
		protected String _InitPeriod;
		[PXDBString(6, IsFixed = true)]
		[GL.FinPeriodIDFormatting()]
		public virtual String InitPeriod
		{
			get
			{
				return this._InitPeriod;
			}
			set
			{
				this._InitPeriod = value;
			}
		}
		#endregion
		#region LastPeriod
		public abstract class lastPeriod : PX.Data.BQL.BqlString.Field<lastPeriod> { }
		protected String _LastPeriod;
		[PXDBString(6, IsFixed = true)]
		[GL.FinPeriodIDFormatting()]
		public virtual String LastPeriod
		{
			get
			{
				return _LastPeriod;
			}
			set
			{
				_LastPeriod = value;
			}
		}
		#endregion
		#region HistPeriod
		public abstract class histPeriod : PX.Data.BQL.BqlString.Field<histPeriod> { }
		protected String _HistPeriod;
		[PXString(6, IsFixed = true)]
		[PXDBCalced(typeof(Switch<
			Case<Where<FABookBalance.currDeprPeriod, IsNull, And<FABookBalance.deprToPeriod, IsNull>>, FABookBalance.deprFromPeriod,
			Case<Where<FABookBalance.currDeprPeriod, IsNull>, FABookBalance.deprToPeriod>>, FABookBalance.currDeprPeriod>), typeof(string))]
		public virtual String HistPeriod
		{
			get
			{
				return this._HistPeriod;
			}
			set
			{
				this._HistPeriod = value;
			}
		}
		#endregion
		#region DisposalPeriodID
		public abstract class disposalPeriodID : PX.Data.BQL.BqlString.Field<disposalPeriodID> { }
		protected string _DisposalPeriodID;
		[PXDBString(6, IsFixed = true)]
		[GL.FinPeriodIDFormatting]
		public virtual string DisposalPeriodID
		{
			get
			{
				return _DisposalPeriodID;
			}
			set
			{
				_DisposalPeriodID = value;
			}
		}
		#endregion
		#region YtdDeprBase
		public abstract class ytdDeprBase : PX.Data.BQL.BqlDecimal.Field<ytdDeprBase> { }
		protected Decimal? _YtdDeprBase;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdDeprBase))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Basis", Enabled = false)]
		public virtual Decimal? YtdDeprBase
		{
			get
			{
				return this._YtdDeprBase;
			}
			set
			{
				this._YtdDeprBase = value;
			}
		}
		#endregion
		#region YtdDepreciated
		public abstract class ytdDepreciated : PX.Data.BQL.BqlDecimal.Field<ytdDepreciated> { }
		protected Decimal? _YtdDepreciated;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdDepreciated))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Accum. Depr.", Enabled = false)]
		public virtual Decimal? YtdDepreciated
		{
			get
			{
				return this._YtdDepreciated;
			}
			set
			{
				this._YtdDepreciated = value;
			}
		}
		#endregion
		#region YtdBal
		public abstract class ytdBal : PX.Data.BQL.BqlDecimal.Field<ytdBal> { }
		protected Decimal? _YtdBal;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdBal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Net Value", Enabled = false)]
		public virtual Decimal? YtdBal
		{
			get
			{
				return this._YtdBal;
			}
			set
			{
				this._YtdBal = value;
			}
		}
		#endregion
		#region YtdAcquired
		public abstract class ytdAcquired : PX.Data.BQL.BqlDecimal.Field<ytdAcquired> { }
		protected Decimal? _YtdAcquired;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdAcquired))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		public virtual Decimal? YtdAcquired
		{
			get
			{
				return this._YtdAcquired;
			}
			set
			{
				this._YtdAcquired = value;
			}
		}
		#endregion
		#region YtdTax179Recap
		public abstract class ytdTax179Recap : PX.Data.BQL.BqlDecimal.Field<ytdTax179Recap> { }
		protected Decimal? _YtdTax179Recap;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdTax179Recap))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax 179 Recapture", Enabled = false)]
		public virtual Decimal? YtdTax179Recap
		{
			get
			{
				return this._YtdTax179Recap;
			}
			set
			{
				this._YtdTax179Recap = value;
			}
		}
		#endregion
		#region YtdBonusRecap
		public abstract class ytdBonusRecap : PX.Data.BQL.BqlDecimal.Field<ytdBonusRecap> { }
		protected Decimal? _YtdBonusRecap;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdBonusRecap))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Bonus Recapture", Enabled = false)]
		public virtual Decimal? YtdBonusRecap
		{
			get
			{
				return this._YtdBonusRecap;
			}
			set
			{
				this._YtdBonusRecap = value;
			}
		}
		#endregion
		#region YtdRGOL
		public abstract class ytdRGOL : PX.Data.BQL.BqlDecimal.Field<ytdRGOL> { }
		protected Decimal? _YtdRGOL;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdRGOL))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gain/Loss Amount", Enabled = false)]
		public virtual Decimal? YtdRGOL
		{
			get
			{
				return this._YtdRGOL;
			}
			set
			{
				this._YtdRGOL = value;
			}
		}
		#endregion
		#region PtdDeprDisposed
		public abstract class ptdDeprDisposed : PX.Data.BQL.BqlDecimal.Field<ptdDeprDisposed> { }
		protected Decimal? _PtdDeprDisposed;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ptdDeprDisposed))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? PtdDeprDisposed
		{
			get
			{
				return this._PtdDeprDisposed;
			}
			set
			{
				this._PtdDeprDisposed = value;
			}
		}
		#endregion
		#region YtdSuspended
		public abstract class ytdSuspended : PX.Data.BQL.BqlInt.Field<ytdSuspended> { }
		protected Int32? _YtdSuspended;
		[PXDBInt(BqlField = typeof(FABookHistory.ytdSuspended))]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? YtdSuspended
		{
			get
			{
				return this._YtdSuspended;
			}
			set
			{
				this._YtdSuspended = value;
			}
		}
		#endregion
		#region YtdReconciled
		public abstract class ytdReconciled : PX.Data.BQL.BqlDecimal.Field<ytdReconciled> { }
		protected Decimal? _YtdReconciled;
		[PXDBBaseCury(BqlField = typeof(FABookHistory.ytdReconciled))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? YtdReconciled
		{
			get
			{
				return this._YtdReconciled;
			}
			set
			{
				this._YtdReconciled = value;
			}
		}
		#endregion
		#region DisposalAmount
		public abstract class disposalAmount : PX.Data.BQL.BqlDecimal.Field<disposalAmount> { }
		protected Decimal? _DisposalAmount;
		[PXBaseCury()]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Disposal Amount")]
		public virtual Decimal? DisposalAmount
		{
			get
			{
				return this._DisposalAmount;
			}
			set
			{
				this._DisposalAmount = value;
			}
		}
		#endregion
		#region OrigDeprToDate
		public abstract class origDeprToDate : PX.Data.BQL.BqlDateTime.Field<origDeprToDate> { }
		protected DateTime? _OrigDeprToDate;
		[PXDBDate]
		public virtual DateTime? OrigDeprToDate
		{
			get
			{
				return this._OrigDeprToDate;
			}
			set
			{
				this._OrigDeprToDate = value;
			}
		}
		#endregion
		#region AllowChangeDeprFromPeriod
		public abstract class allowChangeDeprFromPeriod : PX.Data.BQL.BqlBool.Field<allowChangeDeprFromPeriod> { }
		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? AllowChangeDeprFromPeriod { get; set; }
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
	}

	[PXProjection(typeof(Select5<
		Standalone.FABookBalance,
		InnerJoin<FixedAsset, 
			On<FixedAsset.assetID, Equal<Standalone.FABookBalance.assetID>>,
		InnerJoin<Branch, 
			On<Branch.branchID, Equal<FixedAsset.branchID>>,
		InnerJoin<FABook,
			On<Standalone.FABookBalance.bookID, Equal<FABook.bookID>>,
		InnerJoin<FABookPeriod,
			On<FABookPeriod.bookID, Equal<Standalone.FABookBalance.bookID>,
			And<FABookPeriod.organizationID, Equal<IIf<Where<FABook.updateGL, Equal<True>>, Branch.organizationID, FinPeriod.organizationID.masterValue>>>>,
		LeftJoin<OrganizationFinPeriod, 
			On<OrganizationFinPeriod.finPeriodID, Equal<FABookPeriod.finPeriodID>,
			And<OrganizationFinPeriod.organizationID, Equal<Branch.organizationID>>>>>>>>,
		Where<FABookPeriod.finPeriodID, Greater<Standalone.FABookBalance.lastDeprPeriod>, 
			And2<Where<FABook.updateGL, NotEqual<True>, 
				Or<OrganizationFinPeriod.fAClosed, NotEqual<True>>>,
			And<Standalone.FABookBalance.assetID,Equal<CurrentValue<FixedAsset.assetID>>>>>, // CurrentValue inserts AssetID of Current FixedAsset. This is necessary to improve performance MySQL
		Aggregate<
			GroupBy<Standalone.FABookBalance.assetID, 
			GroupBy<Standalone.FABookBalance.bookID, 
			Min<FABookPeriod.finPeriodID>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class FABookBalanceNext : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.assetID))]
		[PXDefault()]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.bookID))]
		[PXDefault()]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FABookPeriodID(
			assetSourceType: typeof(FABookBalanceNext.assetID),
			bookSourceType: typeof(FABookBalanceNext.bookID),
			BqlField = typeof(FABookPeriod.finPeriodID))]

		[PXDefault()]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}


	[PXProjection(
		typeof(Select2<Standalone.FABookBalance, 
			LeftJoin<FABook,
				On<FABook.bookID, Equal<Standalone.FABookBalance.bookID>>>,
			Where<FABook.updateGL, Equal<True>>>))]
	[Serializable]
	[PXHidden]
	public class FABookBalanceUpdateGL: IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.assetID))]
		public virtual int? AssetID { get; set; }
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.bookID))]
		public virtual int? BookID { get; set; }
		#endregion
	}

	[PXProjection(
		typeof(Select5<Standalone.FABookBalance,
			LeftJoin<FABook,
				On<FABook.bookID, Equal<Standalone.FABookBalance.bookID>>>,
			Where<FABook.updateGL, NotEqual<True>>, 
			Aggregate<GroupBy<Standalone.FABookBalance.assetID, Min<Standalone.FABookBalance.bookID>>>>))]
	[Serializable]
	[PXHidden]
	public class FABookBalanceNonUpdateGL : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.assetID))]
		public virtual int? AssetID { get; set; }
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.bookID))]
		public virtual int? BookID { get; set; }
		#endregion
	}

	[PXProjection(
		typeof(Select2<Standalone.FABookBalance,
			LeftJoin<FABookBalanceNext, 
				On<FABookBalanceNext.assetID, Equal<Standalone.FABookBalance.assetID>, 
					And<FABookBalanceNext.bookID, Equal<Standalone.FABookBalance.bookID>>>,
			LeftJoin<FABook, 
				On<FABook.bookID, Equal<Standalone.FABookBalance.bookID>>>>,
			Where<Standalone.FABookBalance.currDeprPeriod, IsNotNull, 
				Or<Standalone.FABookBalance.lastDeprPeriod, IsNotNull>>>))]
	[Serializable]
	[PXHidden]
	public class FABookBalanceTransfer : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.assetID))]
		[PXDefault()]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXDBInt(IsKey = true, BqlField = typeof(Standalone.FABookBalance.bookID))]
		[PXDefault()]
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
		#region UpdateGL
		public abstract class updateGL : PX.Data.BQL.BqlBool.Field<updateGL> { }
		protected Boolean? _UpdateGL;
		[PXDBBool(BqlField = typeof(FABook.updateGL))]
		public virtual Boolean? UpdateGL
		{
			get
			{
				return this._UpdateGL;
			}
			set
			{
				this._UpdateGL = value;
			}
		}
		#endregion
		#region TransferPeriodID
		public abstract class transferPeriodID : PX.Data.BQL.BqlString.Field<transferPeriodID> { }
		[PXDBCalced(typeof(IsNull<FABookBalance.currDeprPeriod, FABookBalanceNext.finPeriodID>), typeof(string))]
		[GL.FinPeriodIDFormatting]
		public virtual string TransferPeriodID
		{
			get;
			set;
		}
		#endregion
	}
}