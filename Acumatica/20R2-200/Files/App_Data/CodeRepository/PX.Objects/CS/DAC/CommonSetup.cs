namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	
	[Serializable]
	[PXCacheName(Messages.CommonSetup)]
	public partial class CommonSetup : IBqlTable
	{
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.BQL.BqlShort.Field<decPlPrcCst>
		{
			public const short Default = 4;
		}
		protected short? _DecPlPrcCst;
		[PXDBShort(MinValue = 0, MaxValue = 6)]
		[PXUIField(DisplayName = "Price/Cost Decimal Places")]
		[PXDefault(decPlPrcCst.Default)]
		public virtual short? DecPlPrcCst
		{
			get
			{
				return this._DecPlPrcCst;
			}
			set
			{
				this._DecPlPrcCst = value;
			}
		}
		#endregion
		#region DecPlQty
		public abstract class decPlQty : PX.Data.BQL.BqlShort.Field<decPlQty>
		{
			public const short Default = 2;
		}
		protected short? _DecPlQty;
		[PXDBShort(MinValue = 0, MaxValue = 6)]
		[PXUIField(DisplayName = "Quantity Decimal Places")]
		[PXDefault(decPlQty.Default)]
		public virtual short? DecPlQty
		{
			get
			{
				return this._DecPlQty;
			}
			set
			{
				this._DecPlQty = value;
			}
		}
		#endregion
		#region WeightUOM
		public abstract class weightUOM : PX.Data.BQL.BqlString.Field<weightUOM> { }
		protected String _WeightUOM;
		[IN.INUnit(DisplayName = "Weight UOM")]
		[PXDefault()]
		public virtual String WeightUOM
		{
			get
			{
				return this._WeightUOM;
			}
			set
			{
				this._WeightUOM = value;
			}
		}
		#endregion
		#region LinearUOM
		public abstract class linearUOM : PX.Data.BQL.BqlString.Field<linearUOM> { }
		[IN.INUnit(DisplayName = "Linear UOM")]
		public virtual String LinearUOM
		{
			get;
			set;
		}
		#endregion
		#region VolumeUOM
		public abstract class volumeUOM : PX.Data.BQL.BqlString.Field<volumeUOM> { }
		protected String _VolumeUOM;
		[PXDefault()]
		[IN.INUnit(DisplayName = "Volume UOM")]
		public virtual String VolumeUOM
		{
			get
			{
				return this._VolumeUOM;
			}
			set
			{
				this._VolumeUOM = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual byte[] tstamp
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
		protected string _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID
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
		protected string _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID
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
