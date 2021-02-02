namespace PX.Objects.EP
{
	using System;
	using PX.Data;
	using PX.Objects.IN;
	using PX.Objects.PM;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.EPEquipmentRate)]
	public partial class EPEquipmentRate : PX.Data.IBqlTable
	{
		#region EquipmentID
		public abstract class equipmentID : PX.Data.BQL.BqlInt.Field<equipmentID> { }
		protected int? _EquipmentID;
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(EPEquipment.equipmentID))]
		[PXParent(typeof(Select<EPEquipment, Where<EPEquipment.equipmentID, Equal<Current<EPEquipmentRate.equipmentID>>>>))]
		public virtual int? EquipmentID
		{
			get
			{
				return this._EquipmentID;
			}
			set
			{
				this._EquipmentID = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDefault]
		[Project(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region RunRate
		public abstract class runRate : PX.Data.BQL.BqlDecimal.Field<runRate> { }
		protected decimal? _RunRate;
		[PXDefault(typeof(Search<EPEquipment.runRate, Where<EPEquipment.equipmentID, Equal<Current<EPEquipmentRate.equipmentID>>>>))]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Run Rate")]
		public virtual decimal? RunRate
		{
			get
			{
				return this._RunRate;
			}
			set
			{
				this._RunRate = value;
			}
		}
		#endregion
		#region SetupRate
		public abstract class setupRate : PX.Data.BQL.BqlDecimal.Field<setupRate> { }
		protected decimal? _SetupRate;
		[PXDefault(typeof(Search<EPEquipment.setupRate, Where<EPEquipment.equipmentID, Equal<Current<EPEquipmentRate.equipmentID>>>>))]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Setup Rate")]
		public virtual decimal? SetupRate
		{
			get
			{
				return this._SetupRate;
			}
			set
			{
				this._SetupRate = value;
			}
		}
		#endregion
		#region SuspendRate
		public abstract class suspendRate : PX.Data.BQL.BqlDecimal.Field<suspendRate> { }
		protected decimal? _SuspendRate;
		[PXDefault(typeof(Search<EPEquipment.suspendRate, Where<EPEquipment.equipmentID, Equal<Current<EPEquipmentRate.equipmentID>>>>))]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Suspend Rate")]
		public virtual decimal? SuspendRate
		{
			get
			{
				return this._SuspendRate;
			}
			set
			{
				this._SuspendRate = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
        [PXNote]
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
		#endregion
	}
}
