using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FADepreciationMethodLines)]
	public partial class FADepreciationMethodLines : IBqlTable
	{
		#region MethodID
		public abstract class methodID : PX.Data.BQL.BqlInt.Field<methodID> { }
		protected Int32? _MethodID;
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(FADepreciationMethod.methodID))]
		[PXParent(typeof(Select<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Current<FADepreciationMethodLines.methodID>>>>), UseCurrent = true, LeaveChildren = false)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
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
		#region Year
		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }
		protected Int32? _Year;
		[PXDBInt(IsKey = true, MaxValue = 500, MinValue = 0)]
		[PXUIField(DisplayName = "Recovery Year", Enabled = false)]
		public virtual Int32? Year
		{
			get
			{
				return this._Year;
			}
			set
			{
				this._Year = value;
			}
		}
		#endregion
		#region DisplayRatioPerYear
		public abstract class displayRatioPerYear : PX.Data.BQL.BqlDecimal.Field<displayRatioPerYear> { }
		[PXDecimal(3, MinValue = 0, MaxValue = 100)]
		[PXFormula(typeof(Mult<Current<ratioPerYear>, decimal100>))]
		[PXUIField(DisplayName = "Percent per Year")]
		public virtual decimal? DisplayRatioPerYear { get; set; }
		#endregion
		#region RatioPerYear
		public abstract class ratioPerYear : PX.Data.BQL.BqlDecimal.Field<ratioPerYear> { }
		protected Decimal? _RatioPerYear;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(5)]
        [PXFormula(typeof(Div<displayRatioPerYear, decimal100>), typeof(SumCalc<FADepreciationMethod.totalPercents>))]
		public virtual Decimal? RatioPerYear
		{
			get
			{
				return this._RatioPerYear;
			}
			set
			{
				this._RatioPerYear = value;
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
	}
}
