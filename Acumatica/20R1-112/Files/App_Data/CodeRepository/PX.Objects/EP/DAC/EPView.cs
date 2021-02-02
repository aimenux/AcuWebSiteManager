using System;
using PX.Data;

namespace PX.Objects.EP
{
    [Serializable]
	[PXCacheName(Messages.ActivityViewStatus)]
	public partial class EPView : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		protected Guid? _NoteID;
		[PXDBGuid(IsKey = true)]
		public virtual Guid? NoteID
		{
			get { return _NoteID; }
			set { _NoteID = value; }
		}
		#endregion

		#region UserID
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

		protected Guid? _UserID;
		[PXDBGuid(IsKey = true)]
		public virtual Guid? UserID
		{
			get { return _UserID; }
			set { _UserID = value; }
		}
		#endregion

		#region Status

		public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }

		protected Int32? _Status;

		[PXDBInt]
		[PXDefault(EPViewStatusAttribute.NOTVIEWED)]
		[EPViewStatusAttribute]
		[PXUIField(DisplayName = "Status")]
		public virtual Int32? Status
		{
			get { return _Status; }
			set { _Status = value; }
		}

		#endregion

		#region Read

		public abstract class read : PX.Data.BQL.BqlBool.Field<read> { }

		protected bool? _Read;
		[PXBool]
		[PXDefault(false)]
		[PXDBCalced(typeof(Switch<Case<Where<EPView.status, Equal<EPViewStatusAttribute.Viewed>>, True>, False>), typeof(bool))]
		[PXUIField(DisplayName = "Read")]
		public virtual bool? Read
		{
			get { return _Read; }
			set { _Read = value; }
		}

		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
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
		[PXDBCreatedByScreenID]
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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedByID]
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
		[PXDBLastModifiedByScreenID]
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
		[PXDBLastModifiedDateTime]
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
