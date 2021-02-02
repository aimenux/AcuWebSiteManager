using System;
using PX.Data;
using PX.Objects.IN;
﻿
namespace PX.Objects.FS
{
	[System.SerializableAttribute]
    [PXCacheName(TX.TableName.SERVICE_LICENSE_TYPE)]
	public class FSServiceLicenseType : PX.Data.IBqlTable
	{
		#region ServiceID
		public abstract class serviceID : PX.Data.BQL.BqlInt.Field<serviceID> { }
		[PXDBInt(IsKey = true)]
        [PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<FSServiceLicenseType.serviceID>>>>))]
		public virtual int? ServiceID  { get; set; }
		#endregion
		#region LicenseTypeID
		public abstract class licenseTypeID : PX.Data.BQL.BqlInt.Field<licenseTypeID> { }
		[PXDBInt(IsKey = true)]
        [PXDefault]
        [PXSelector(typeof(FSLicenseType.licenseTypeID), SubstituteKey = typeof(FSLicenseType.licenseTypeCD))]
		[PXUIField(DisplayName = "License Type ID")]
		public virtual int? LicenseTypeID  { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		[PXUIField(DisplayName = "CreatedByID")]
		public virtual Guid? CreatedByID  { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "CreatedByScreenID")]
		public virtual string CreatedByScreenID  { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "CreatedDateTime")]
		public virtual DateTime? CreatedDateTime  { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		[PXUIField(DisplayName = "LastModifiedByID")]
		public virtual Guid? LastModifiedByID  { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "LastModifiedByScreenID")]
		public virtual string LastModifiedByScreenID  { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "LastModifiedDateTime")]
		public virtual DateTime? LastModifiedDateTime  { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp  { get; set; }
		#endregion
	}
}
