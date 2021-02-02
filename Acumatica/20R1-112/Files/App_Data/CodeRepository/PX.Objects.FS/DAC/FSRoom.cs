using System;
using PX.Data;
﻿
namespace PX.Objects.FS
{
	[System.SerializableAttribute]
    [PXCacheName(TX.TableName.ROOM)]
    [PXPrimaryGraph(typeof(RoomMaint))]
	public class FSRoom : PX.Data.IBqlTable
	{
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
        [PXDBIdentity(IsKey = true)]
        public virtual Int32? RecordID { get; set; }
        #endregion
		#region BranchLocationID
		public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Branch Location ID")]
        [PXParent(typeof(Select<FSBranchLocation, Where<FSBranchLocation.branchLocationID, Equal<Current<FSRoom.branchLocationID>>>>))]
        [PXDBLiteDefault(typeof(FSBranchLocation.branchLocationID))]
        public virtual int? BranchLocationID { get; set; }
		#endregion
		#region RoomID
		public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }
        [PXDefault]
		[PXDBString(10, IsUnicode = true, InputMask=">AAAAAAAAAA")]
		[PXUIField(DisplayName = "Room ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSRoom.roomID, Where<FSRoom.branchLocationID,Equal<Current<FSRoom.branchLocationID>>>>))]
        public virtual string RoomID { get; set; }
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		[PXDBLocalizableString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
		#endregion
		#region FloorNbr
		public abstract class floorNbr : PX.Data.BQL.BqlInt.Field<floorNbr> { }
		[PXDBInt(MinValue=0)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Floor Nbr.")]
        public virtual int? FloorNbr { get; set; }
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
        #region DispatchBoardHelper
        #region CustomRoomID
        public abstract class customRoomID : PX.Data.BQL.BqlString.Field<customRoomID> { }
        [PXString]
        public virtual string CustomRoomID { get; set; }
        #endregion
        #endregion
    }
}
