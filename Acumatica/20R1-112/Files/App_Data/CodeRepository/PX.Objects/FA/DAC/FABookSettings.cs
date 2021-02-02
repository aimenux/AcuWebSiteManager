using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FABookSettings)]
	public partial class FABookSettings : IBqlTable
	{
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<assetID>>, And<FixedAsset.recordType, Equal<FARecordType.classType>>>>), UseCurrent = true, LeaveChildren = false)]
		[PXUIField(DisplayName = "Asset Class", Visibility = PXUIVisibility.Invisible, Visible = false)]
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
		#region Depreciate
		public abstract class depreciate : PX.Data.BQL.BqlBool.Field<depreciate> { }
		protected Boolean? _Depreciate;
		[PXDBBool]
		[PXDefault(typeof(Search<FixedAsset.depreciable, Where<FixedAsset.assetID, Equal<Current<FABookSettings.assetID>>>>))]
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
		#region UpdateGL
		public abstract class updateGL : PX.Data.BQL.BqlBool.Field<updateGL> { }
		protected Boolean? _UpdateGL;
		[PXBool]
		[PXDBScalar(typeof(Search<FABook.updateGL, Where<FABook.bookID, Equal<FABookSettings.bookID>>>))]
		[PXDefault(false, typeof(Search<FABook.updateGL, Where<FABook.bookID, Equal<Current<FABookSettings.bookID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region DepreciationMethodID
		public abstract class depreciationMethodID : PX.Data.BQL.BqlInt.Field<depreciationMethodID> { }
		protected Int32? _DepreciationMethodID;
		[PXDBInt]
		[PXSelector(typeof(Search<FADepreciationMethod.methodID,
			Where2<Where<FADepreciationMethod.usefulLife, Equal<Current<usefulLife>>,
				Or<FADepreciationMethod.usefulLife, IsNull,
				Or<FADepreciationMethod.usefulLife, Equal<decimal0>>>>,
			And<Where<FADepreciationMethod.recordType, NotEqual<FARecordType.assetType>>>>>),
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
		[PXUIField(DisplayName = "Class Method", Visibility = PXUIVisibility.Visible)]
		[PXDefault]
		[PXUIRequired(typeof(FABookSettings.depreciate))]
		[PXUIEnabled(typeof(FABookSettings.depreciate))]
		public virtual Int32? DepreciationMethodID
		{
			get
			{
				return this._DepreciationMethodID;
			}
			set
			{
				this._DepreciationMethodID = value;
			}
		}
		#endregion
		#region MidMonthType
		public abstract class midMonthType : PX.Data.BQL.BqlString.Field<midMonthType>
		{
			public class IsRequired<depreciateField, averagingConventionField> : IIf<Where<depreciateField, Equal<True>,
				And<Where<averagingConventionField, Equal<FAAveragingConvention.modifiedPeriod>,
					Or<averagingConventionField, Equal<FAAveragingConvention.modifiedPeriod2>>>>>, True, False>
				where depreciateField : IBqlField
				where averagingConventionField : PX.Data.IBqlField { }
		}
		protected string _MidMonthType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Mid-Period Type")]
		[PXFormula(typeof(Selector<FABookSettings.bookID, FABook.midMonthType>))]
		[FABook.midMonthType.List]
		[PXDefault]
		[PXUIRequired(typeof(midMonthType.IsRequired<FABookSettings.depreciate, FABookSettings.averagingConvention>))]
		[PXUIEnabled(typeof(midMonthType.IsRequired<FABookSettings.depreciate, FABookSettings.averagingConvention>))]
		public virtual string MidMonthType
		{
			get
			{
				return _MidMonthType;
			}
			set
			{
				_MidMonthType = value;
			}
		}
		#endregion
		#region MidMonthDay
		public abstract class midMonthDay : PX.Data.BQL.BqlShort.Field<midMonthDay> { }
		protected short? _MidMonthDay;
		[PXDBShort]
		[PXUIField(DisplayName = "Mid-Period Day")]
		[PXFormula(typeof(Selector<FABookSettings.bookID, FABook.midMonthDay>))]
		[PXDefault]
		[PXUIRequired(typeof(midMonthType.IsRequired<FABookSettings.depreciate, FABookSettings.averagingConvention>))]
		[PXUIEnabled(typeof(midMonthType.IsRequired<FABookSettings.depreciate, FABookSettings.averagingConvention>))]
		public virtual short? MidMonthDay
		{
			get
			{
				return _MidMonthDay;
			}
			set
			{
				_MidMonthDay = value;
			}
		}
		#endregion
		#region Bonus
		public abstract class bonus : PX.Data.BQL.BqlBool.Field<bonus> { }
		protected bool? _Bonus;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bonus")]
		[PXUIEnabled(typeof(FABookSettings.depreciate))]
		public virtual bool? Bonus
		{
			get
			{
				return this._Bonus;
			}
			set
			{
				this._Bonus = value;
			}
		}
		#endregion
		#region Sect179
		public abstract class sect179 : PX.Data.BQL.BqlBool.Field<sect179> { }
		protected bool? _Sect179;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Sect. 179")]
		[PXUIEnabled(typeof(FABookSettings.depreciate))]
		public virtual bool? Sect179
		{
			get
			{
				return this._Sect179;
			}
			set
			{
				this._Sect179 = value;
			}
		}
		#endregion
		#region AveragingConvention
		public abstract class averagingConvention : PX.Data.BQL.BqlString.Field<averagingConvention> { }
		protected string _AveragingConvention;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Averaging Convention")]
		[PXDefault(typeof(Search<FADepreciationMethod.averagingConvention, Where<FADepreciationMethod.methodID, Equal<Current<depreciationMethodID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[FAAveragingConvention.List]
		[PXUIRequired(typeof(FABookSettings.depreciate))]
		[PXUIEnabled(typeof(Switch<Case2<
			Where<depreciate, Equal<True>,
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
		public virtual string AveragingConvention
		{
			get
			{
				return _AveragingConvention;
			}
			set
			{
				_AveragingConvention = value;
			}
		}
		#endregion
		#region PercentPerYear
		public abstract class percentPerYear : PX.Data.BQL.BqlDecimal.Field<percentPerYear> { }
		[PXDBDecimal(4, MinValue = 0, MaxValue = 100)]
		[PXDefault(typeof(Search<FADepreciationMethod.percentPerYear, Where<FADepreciationMethod.methodID, Equal<Current<depreciationMethodID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Percent per Year")]
		[PXUIEnabled(typeof(Switch<Case2<
			Where<depreciate, Equal<True>,
			And<Where<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.australianDiminishingValue>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValue>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>,
					Or<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, Equal<FADepreciationMethod.depreciationMethod.newZealandDiminishingValueEvenly>>>>>>>>>, True>, False>))]
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
		[PXDefault(typeof(FixedAsset.usefulLife))]
		[PXUIField(DisplayName = "Useful Life, Years")]
		[PXUIRequired(typeof(depreciate))]
		[PXUIEnabled(typeof(Switch<Case2<
			Where<depreciate, Equal<True>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.australianPrimeCost>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLine>,
			And<Selector<depreciationMethodID, FADepreciationMethod.depreciationMethod>, NotEqual<FADepreciationMethod.depreciationMethod.newZealandStraightLineEvenly>>>>>, True>, False >))]
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
}