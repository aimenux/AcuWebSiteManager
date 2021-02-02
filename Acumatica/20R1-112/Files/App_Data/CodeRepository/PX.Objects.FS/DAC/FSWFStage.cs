using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.ORDER_STAGE)]
    [PXPrimaryGraph(typeof(WFStageMaint))]
	public class FSWFStage : PX.Data.IBqlTable
    {
        #region WFID
        public abstract class wFID : PX.Data.BQL.BqlInt.Field<wFID> { }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Workflow ID")]
        public virtual int? WFID { get; set; }
		#endregion
        #region ParentWFStageID
        public abstract class parentWFStageID : PX.Data.BQL.BqlInt.Field<parentWFStageID> { }

        [PXDBInt(IsKey = true)]
        [PXDBLiteDefault(typeof(FSWFStage.wFStageID))]
        [PXUIField(DisplayName = "Parent Workflow Stage ID")]
        public virtual int? ParentWFStageID { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXDBIdentity]
        [PXUIField(DisplayName = "Workflow Stage ID")]
		public virtual int? WFStageID { get; set; }
		#endregion
        #region WFStageCD
        public abstract class wFStageCD : PX.Data.BQL.BqlString.Field<wFStageCD> { }

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Workflow Stage ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string WFStageCD { get; set; }
		#endregion
		#region AllowCancel
		public abstract class allowCancel : PX.Data.BQL.BqlBool.Field<allowCancel> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allow Cancel")]
		public virtual bool? AllowCancel { get; set; }
		#endregion
		#region AllowDelete
		public abstract class allowDelete : PX.Data.BQL.BqlBool.Field<allowDelete> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allow Delete")]
		public virtual bool? AllowDelete { get; set; }
		#endregion
		#region AllowModify
		public abstract class allowModify : PX.Data.BQL.BqlBool.Field<allowModify> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allow Update")]
		public virtual bool? AllowModify { get; set; }
		#endregion
		#region AllowPost
		public abstract class allowPost : PX.Data.BQL.BqlBool.Field<allowPost> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allow Post")]
		public virtual bool? AllowPost { get; set; }
		#endregion
        #region AllowComplete
        public abstract class allowComplete : PX.Data.BQL.BqlBool.Field<allowComplete> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Allow Complete")]
        public virtual bool? AllowComplete { get; set; }
        #endregion
        #region AllowReopen
        public abstract class allowReopen : PX.Data.BQL.BqlBool.Field<allowReopen> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Allow Reopen")]
        public virtual bool? AllowReopen { get; set; }
        #endregion
        #region AllowClose
        public abstract class allowClose : PX.Data.BQL.BqlBool.Field<allowClose> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Allow Close")]
        public virtual bool? AllowClose { get; set; }
        #endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Descr { get; set; }
		#endregion
		#region RequireReason
		public abstract class requireReason : PX.Data.BQL.BqlBool.Field<requireReason> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Reason")]
		public virtual bool? RequireReason { get; set; }
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual int? SortOrder { get; set; }
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
		[PXUIField(DisplayName = "CreatedByID")]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "CreatedByScreenID")]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "CreatedDateTime")]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		[PXUIField(DisplayName = "LastModifiedByID")]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "LastModifiedByScreenID")]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "LastModifiedDateTime")]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		[PXUIField(DisplayName = "tstamp")]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}
}
