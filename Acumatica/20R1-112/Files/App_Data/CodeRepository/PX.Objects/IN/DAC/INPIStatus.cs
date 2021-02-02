using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.INPIStatus)]
	[PXProjection(typeof(Select2<INPIStatusItem, InnerJoin<INPIStatusLoc, On<INPIStatusLoc.pIID, Equal<INPIStatusItem.pIID>>>>), Persistent = false)]
	public partial class INPIStatus : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPIStatus>.By<recordID>
		{
			public static INPIStatus Find(PXGraph graph, int? recordID) => FindBy(graph, recordID);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INPIStatus>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INPIStatus>.By<locationID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INPIStatus>.By<inventoryID> { }
			public class PIHeader : INPIHeader.PK.ForeignKeyOf<INPIStatus>.By<pIID> { }
		}
        #endregion
        #region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBInt(IsKey = true, BqlField = typeof(INPIStatusItem.recordID))]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region LocRecordID
		public abstract class locRecordID : PX.Data.BQL.BqlInt.Field<locRecordID> { }
		protected Int32? _LocRecordID;
		[PXDBInt(IsKey = true, BqlField = typeof(INPIStatusLoc.recordID))]
		public virtual Int32? LocRecordID
		{
			get
			{
				return this._LocRecordID;
			}
			set
			{
				this._LocRecordID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(BqlField = typeof(INPIStatusLoc.siteID))]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INPIStatus.siteID), BqlField = typeof(INPIStatusLoc.locationID))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion  
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(BqlField = typeof(INPIStatusItem.inventoryID))]		
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion						
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;
		[PXDBBool(BqlField = typeof(INPIStatusItem.active))]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
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
		#region PIID		
		public abstract class pIID : PX.Data.BQL.BqlString.Field<pIID> { }
		protected String _PIID;		
		[PXDBString(15, IsUnicode = true, BqlField = typeof(INPIStatusItem.pIID))]
		[PXUIField(DisplayName="Physical Count ID", Visibility=PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String PIID
		{
			get
			{
				return this._PIID;
			}
			set
			{
				this._PIID = value;
			}
		}
		#endregion		
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField = typeof(INPIStatusItem.createdByID))]
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
		[PXDBCreatedByScreenID(BqlField = typeof(INPIStatusItem.createdByScreenID))]
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
		[PXDBCreatedDateTime(BqlField = typeof(INPIStatusItem.createdDateTime))]
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
		[PXDBLastModifiedByID(BqlField = typeof(INPIStatusItem.lastModifiedByID))]
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
		[PXDBLastModifiedByScreenID(BqlField = typeof(INPIStatusItem.lastModifiedByScreenID))]
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
		[PXDBLastModifiedDateTime(BqlField = typeof(INPIStatusItem.lastModifiedDateTime))]
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
		[PXDBTimestamp(BqlField = typeof(INPIStatusItem.Tstamp))]
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

}

