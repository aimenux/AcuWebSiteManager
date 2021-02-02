using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FAUsage)]
	public partial class FAUsage : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FAUsage.assetID>>>>))]
		[PXParent(typeof(Select<FADetails, Where<FADetails.assetID, Equal<Current<FAUsage.assetID>>>>))]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
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
		#region Number
		public abstract class number : PX.Data.BQL.BqlInt.Field<number> { }
		protected Int32? _Number;
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXLineNbr(typeof(FixedAsset))]
		public virtual Int32? Number
		{
			get
			{
				return _Number;
			}
			set
			{
				_Number = value;
			}
		}
		#endregion

		#region MeasurementDate
		public abstract class measurementDate : PX.Data.BQL.BqlDateTime.Field<measurementDate> { }
		protected DateTime? _MeasurementDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Measurement Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[PXFormula(null, typeof(MaxCalc<FADetails.lastMeasurementUsageDate>))]
		public virtual DateTime? MeasurementDate
		{
			get
			{
				return _MeasurementDate;
			}
			set
			{
				_MeasurementDate = value;
			}
		}
		#endregion
		#region ScheduledDate
		public abstract class scheduledDate : PX.Data.BQL.BqlDateTime.Field<scheduledDate> { }
		protected DateTime? _ScheduledDate;
		[PXDBDate]
		[PXDefault(typeof(FADetails.nextMeasurementUsageDate))]
		[PXUIField(DisplayName = "Scheduled Date", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 3)]
		public virtual DateTime? ScheduledDate
		{
			get
			{
				return _ScheduledDate;
			}
			set
			{
				_ScheduledDate = value;
			}
		}
		#endregion
		#region MeasuredBy
		public abstract class measuredBy : PX.Data.BQL.BqlGuid.Field<measuredBy> { }
		protected Guid? _MeasuredBy;
		[PXDBField]
		[PXSelector(typeof(EPEmployee.userID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Measured By")]
		[PXDefault]
		public virtual Guid? MeasuredBy
		{
			get
			{
				return _MeasuredBy;
			}
			set
			{
				_MeasuredBy = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlDecimal.Field<value> { }
		protected Decimal? _Value;
		[PXDBDecimal(4)]
		[PXDefault]
		[PXUIField(DisplayName = "Value", Required = true)]
		public virtual Decimal? Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this._Value = value;
			}
		}
		#endregion
		#region Difference
		public abstract class difference : PX.Data.BQL.BqlDecimal.Field<difference> { }
		protected Decimal? _Difference;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Difference with Previos Measurement Value")]
		public virtual Decimal? Difference
		{
			get
			{
				return this._Difference;
			}
			set
			{
				this._Difference = value;
			}
		}
		#endregion
		#region DepreciationPercent
		public abstract class depreciationPercent : PX.Data.BQL.BqlDecimal.Field<depreciationPercent> { }
		protected Decimal? _DepreciationPercent;
		[PXDBDecimal(4)]
		[PXDefault]
		[PXUIField(DisplayName = "Depreciation Rate", Enabled = false)]
		[PXFormula(typeof(DepreciatedPartTotalUsage<value, assetID>))]
		public virtual Decimal? DepreciationPercent
		{
			get
			{
				return _DepreciationPercent;
			}
			set
			{
				_DepreciationPercent = value;
			}
		}
		#endregion
		#region UsageUOM
		public abstract class usageUOM : PX.Data.BQL.BqlString.Field<usageUOM> { }
		protected String _UsageUOM;
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		[PXUIField(DisplayName = "UOM", Enabled = false)]
		[PXDefault(typeof(Search2<FAUsageSchedule.usageUOM, LeftJoin<FixedAsset, On<FixedAsset.usageScheduleID, Equal<FAUsageSchedule.scheduleID>>>,
			Where<FixedAsset.assetID, Equal<Current<assetID>>>>))]
		public virtual String UsageUOM
		{
			get
			{
				return _UsageUOM;
			}
			set
			{
				_UsageUOM = value;
			}
		}
		#endregion
		#region Depreciated
		public abstract class depreciated : PX.Data.BQL.BqlBool.Field<depreciated> { }
		protected Boolean? _Depreciated;
		[PXDBBool]
		[PXUIField(DisplayName = "Depreciated", Enabled = false)]
		[PXDefault(false)]
		public virtual Boolean? Depreciated
		{
			get
			{
				return _Depreciated;
			}
			set
			{
				_Depreciated = value;
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
