using System;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FABonusDetails)]
	public partial class FABonusDetails:IBqlTable
	{
		#region BonusID
		public abstract class bonusID : PX.Data.BQL.BqlInt.Field<bonusID> { }
		protected Int32? _BonusID;
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<FABonus, Where<FABonus.bonusID, Equal<Current<bonusID>>>>))]
		[PXDBLiteDefault(typeof(FABonus.bonusID))]
		public virtual Int32? BonusID
		{
			get
			{
				return _BonusID;
			}
			set
			{
				_BonusID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXLineNbr(typeof(FABonus))]
		public virtual Int32? LineNbr
		{
			get
			{
				return _LineNbr;
			}
			set
			{
				_LineNbr = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return _StartDate;
			}
			set
			{
				_StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return _EndDate;
			}
			set
			{
				_EndDate = value;
			}
		}
		#endregion
		#region BonusPercent
		public abstract class bonusPercent : PX.Data.BQL.BqlDecimal.Field<bonusPercent> { }
		protected decimal? _BonusPercent;
		[PXDBDecimal(6, MinValue = 0.0, MaxValue = 100.0)]
		[PXUIField(DisplayName = "Bonus, %", Required = true)]
		[PXDefault]
		public virtual decimal? BonusPercent
		{
			get
			{
				return _BonusPercent;
			}
			set
			{
				_BonusPercent = value;
			}
		}
		#endregion
		#region BonusMax
		public abstract class bonusMax : PX.Data.BQL.BqlDecimal.Field<bonusMax> { }
		protected decimal? _BonusMax;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Max. Bonus")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? BonusMax
		{
			get
			{
				return _BonusMax;
			}
			set
			{
				_BonusMax = value;
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
