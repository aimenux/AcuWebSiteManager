using System;
using PX.Data;
using PX.Objects.IN;
	
namespace PX.Objects.FA
{
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(ServiceScheduleMaint))]
	[PXCacheName(Messages.FAServiceSchedule)]
	public partial class FAServiceSchedule : PX.Data.IBqlTable
	{
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
		protected Int32? _ScheduleID;
		[PXDBIdentity()]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region ScheduleCD
		public abstract class scheduleCD : PX.Data.BQL.BqlString.Field<scheduleCD> { }
		protected String _ScheduleCD;
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		public virtual String ScheduleCD
		{
			get
			{
				return this._ScheduleCD;
			}
			set
			{
				this._ScheduleCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
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
		#region ServiceEveryValue
		public abstract class serviceEveryValue : PX.Data.BQL.BqlInt.Field<serviceEveryValue> { }
		protected Int32? _ServiceEveryValue;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Service Every", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 2)]
		public virtual Int32? ServiceEveryValue
		{
			get
			{
				return this._ServiceEveryValue;
			}
			set
			{
				this._ServiceEveryValue = value;
			}
		}
		#endregion
		#region ServiceEveryPeriod
		public abstract class serviceEveryPeriod : PX.Data.BQL.BqlString.Field<serviceEveryPeriod> { }
		protected String _ServiceEveryPeriod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FAUsageSchedule.readUsageEveryPeriod.Month)]
		[PXUIField(DisplayName = "", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 3)]
		[FAUsageSchedule.readUsageEveryPeriod.List()]
		public virtual String ServiceEveryPeriod
		{
			get
			{
				return this._ServiceEveryPeriod;
			}
			set
			{
				this._ServiceEveryPeriod = value;
			}
		}
		#endregion
		#region ServiceAfterUsageValue
		public abstract class serviceAfterUsageValue : PX.Data.BQL.BqlDecimal.Field<serviceAfterUsageValue> { }
		protected Decimal? _ServiceAfterUsageValue;
		[PXDBDecimal(4)]
		[PXDefault()]
		[PXUIField(DisplayName = "Service after Usage", TabOrder = 4)]
		public virtual Decimal? ServiceAfterUsageValue
		{
			get
			{
				return this._ServiceAfterUsageValue;
			}
			set
			{
				this._ServiceAfterUsageValue = value;
			}
		}
		#endregion
		#region ServiceAfterUsageUOM
		public abstract class serviceAfterUsageUOM : PX.Data.BQL.BqlString.Field<serviceAfterUsageUOM> { }
		protected String _ServiceAfterUsageUOM;
		[INUnit(typeof(INTran.inventoryID), TabOrder = 5)]
		public virtual String ServiceAfterUsageUOM
		{
			get
			{
				return this._ServiceAfterUsageUOM;
			}
			set
			{
				this._ServiceAfterUsageUOM = value;
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
