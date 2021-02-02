using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INLotSerSegment)]
	public partial class INLotSerSegment : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INLotSerSegment>.By<lotSerClassID, segmentID>
		{
			public static INLotSerSegment Find(PXGraph graph, string lotSerClassID, long? segmentID) => FindBy(graph, lotSerClassID, segmentID);
		}
		public static class FK
		{
			public class LotSerClass : INLotSerClass.PK.ForeignKeyOf<INLotSerSegment>.By<lotSerClassID> { }
		}
		#endregion

		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
		protected String _LotSerClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(INLotSerClass.lotSerClassID))]
		[PXParent(typeof(FK.LotSerClass))]
		public virtual String LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region SegmentID
		public abstract class segmentID : PX.Data.BQL.BqlShort.Field<segmentID> { }
		protected Int16? _SegmentID;
		[PXDBShort(IsKey = true)]
		[PXUIField(DisplayName="Segment Number", Enabled=false)]
		[PXLineNbr(typeof(INLotSerClass))]
		[PXDefault()]
		public virtual Int16? SegmentID
		{
			get
			{
				return this._SegmentID;
			}
			set
			{
				this._SegmentID = value;
			}
		}
		#endregion
		#region SegmentType
		public abstract class segmentType : PX.Data.BQL.BqlString.Field<segmentType> { }
		protected String _SegmentType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INLotSerSegmentType.FixedConst)]
		[INLotSerSegmentType.List()]
		[PXUIField(DisplayName="Type")]
		public virtual String SegmentType
		{
			get
			{
				return this._SegmentType;
			}
			set
			{
				this._SegmentType = value;
			}
		}
		#endregion
		#region SegmentValue
		public abstract class segmentValue : PX.Data.BQL.BqlString.Field<segmentValue> { }
		protected String _SegmentValue;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName="Value")]
		public virtual String SegmentValue
		{
			get
			{
				return this._SegmentValue;
			}
			set
			{
				this._SegmentValue = value;
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
	}

	public class INLotSerSegmentType
	{ 
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(NumericVal, Messages.NumericVal),
					Pair(FixedConst, Messages.FixedConst),
					Pair(DayConst, Messages.DayConst),
					Pair(MonthConst, Messages.MonthConst),
					Pair(MonthLongConst, Messages.MonthLongConst),
					Pair(YearConst, Messages.YearConst),
					Pair(YearLongConst, Messages.YearLongConst),
					Pair(DateConst, Messages.DateConst),
				}) {}
		}

		public const string NumericVal = "N";
		public const string FixedConst = "C";
		public const string DateConst  = "D";
		public const string DayConst = "U";
		public const string MonthConst = "M";
		public const string MonthLongConst = "A";
		public const string YearConst = "Y";
		public const string YearLongConst = "L";
	
	}
}
