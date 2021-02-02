namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(DimensionMaint)},
		new Type[] { typeof(Select<Segment, 
			Where<Segment.dimensionID, Equal<Current<SegmentValue.dimensionID>>, And<Segment.segmentID, Equal<Current<SegmentValue.segmentID>>>>>)
		})]
    [PXCacheName(Messages.SegmentValue)]
    public partial class SegmentValue : PX.Data.IBqlTable, PX.SM.IIncludable
	{
		#region DimensionID
		public abstract class dimensionID : PX.Data.BQL.BqlString.Field<dimensionID> { }
		protected String _DimensionID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Segment.dimensionID))]
		[PXUIField(DisplayName = "Segmented Key ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual String DimensionID
		{
			get
			{
				return this._DimensionID;
			}
			set
			{
				this._DimensionID = value;
			}
		}
		#endregion
		#region SegmentID
		public abstract class segmentID : PX.Data.BQL.BqlShort.Field<segmentID> { }
		protected Int16? _SegmentID;
		[PXDBShort(IsKey = true)]
		[PXDefault(typeof(Segment.segmentID))]
		[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
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
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		protected String _Value;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible)]
		[PXParent(typeof(Select<Segment, Where<Segment.dimensionID, Equal<Current<SegmentValue.dimensionID>>, And<Segment.segmentID, Equal<Current<SegmentValue.segmentID>>>>>))]
		public virtual String Value
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
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible )]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault((bool)true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region IsConsolidatedValue
		public abstract class isConsolidatedValue : PX.Data.BQL.BqlBool.Field<isConsolidatedValue> { }
		protected Boolean? _IsConsolidatedValue;
		[PXDBBool()]
		[PXDefault((bool)false)]
		[PXUIField(DisplayName = "Aggregation")]
		public virtual Boolean? IsConsolidatedValue
		{
			get
			{
				return this._IsConsolidatedValue;
			}
			set
			{
				this._IsConsolidatedValue = value;
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
		#region MappedSegValue
		public abstract class mappedSegValue : PX.Data.BQL.BqlString.Field<mappedSegValue> { }
		protected String _MappedSegValue;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Mapped Value", Visibility = PXUIVisibility.Visible)]
		public virtual String MappedSegValue
		{
			get
			{
				return this._MappedSegValue;
			}
			set
			{
				this._MappedSegValue = value;
			}
		}
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		public virtual bool? Included
		{
			get
			{
				return this._Included;
			}
			set
			{
				this._Included = value;
			}
		}
		#endregion
	}
}
