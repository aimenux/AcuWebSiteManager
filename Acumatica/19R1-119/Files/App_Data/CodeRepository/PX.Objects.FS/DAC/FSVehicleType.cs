using System;
using PX.Data;
﻿
namespace PX.Objects.FS
{
	[System.SerializableAttribute]
    [PXCacheName(TX.TableName.VEHICLE_TYPE)]
    [PXPrimaryGraph(typeof(VehicleTypeMaint))]
	public class FSVehicleType : PX.Data.IBqlTable
	{
		#region VehicleTypeID
		public abstract class vehicleTypeID : PX.Data.BQL.BqlInt.Field<vehicleTypeID> { }
		[PXDBIdentity]
        public virtual int? VehicleTypeID { get; set; }
		#endregion
		#region VehicleTypeCD
		public abstract class vehicleTypeCD : PX.Data.BQL.BqlString.Field<vehicleTypeCD> { }               
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [PXDefault] 
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Vehicle Type ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSVehicleType.vehicleTypeCD>))]
        public virtual string VehicleTypeCD { get; set; }
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		[PXDBString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
		#endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion

        #region Memory Helper
        #region VehicleTypeCD
        public abstract class vehicleTypeGICD : PX.Data.BQL.BqlString.Field<vehicleTypeGICD>
		{
        }
        [PXString]
        [PXUIField(DisplayName = "Vehicle Type")]
        public virtual string VehicleTypeGICD { get; set; }
        #endregion
        #endregion
    }

}