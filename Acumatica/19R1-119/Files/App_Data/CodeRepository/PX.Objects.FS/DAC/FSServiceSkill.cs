﻿using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.FS
{	
	[System.SerializableAttribute]
	public class FSServiceSkill : PX.Data.IBqlTable
	{
		#region ServiceID
		public abstract class serviceID : PX.Data.BQL.BqlInt.Field<serviceID> { }		
		[PXDBInt(IsKey = true)]
        [PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
		[PXUIField(DisplayName = "Service ID")]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<FSServiceSkill.serviceID>>>>))]
		public virtual int? ServiceID { get; set;}
		#endregion
        #region SkillID
        public abstract class skillID : PX.Data.BQL.BqlInt.Field<skillID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Skill ID")]
        [PXSelector(typeof(FSSkill.skillID), SubstituteKey = typeof(FSSkill.skillCD))]
        public virtual int? SkillID { get; set; }
        #endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }		
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set;}
        #endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }		
		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "Created By ScreenID")]
		public virtual string CreatedByScreenID { get; set;}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }		
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set;}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }		
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set;}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }		
		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "Last Modified By ScreenID")]
		public virtual string LastModifiedByScreenID { get; set;}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }		
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set;}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }		
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set;}
		#endregion
	}
}
