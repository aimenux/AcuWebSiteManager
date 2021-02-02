using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	[Serializable]
    [PXHidden]
	[Obsolete("Will be removed in 7.0 version")]
	public partial class CRShortcut : IBqlTable
	{
		#region Command

		public abstract class command : PX.Data.BQL.BqlString.Field<command> { }

		[PXDBString(IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Command")]
		public virtual String Command { get; set; }

		#endregion

		#region Ctrl

		public abstract class ctrl : PX.Data.BQL.BqlBool.Field<ctrl> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Ctrl")]
		public virtual Boolean? Ctrl { get; set; }

		#endregion

		#region Alt

		public abstract class alt : PX.Data.BQL.BqlBool.Field<alt> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Alt")]
		public virtual Boolean? Alt { get; set; }

		#endregion

		#region Shift

		public abstract class shift : PX.Data.BQL.BqlBool.Field<shift> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Shift")]
		public virtual Boolean? Shift { get; set; }

		#endregion

		#region Shortcut

		public abstract class shortcut : PX.Data.BQL.BqlString.Field<shortcut> { }

		[PXDBString(10)]
		[PXUIField(DisplayName = "Char Codes")]
		public virtual String CharCodes { get; set; }

		#endregion


		#region KeysDescription

		public abstract class keysDescription : PX.Data.BQL.BqlString.Field<keysDescription> { }

		[PXString]
		[PXUIField(DisplayName = "Shortcuts")]
		public virtual String KeysDescription { get; set; }

		#endregion

		#region DisplayName

		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

		[PXString]
		[PXUIField(DisplayName = "Command", IsReadOnly = true)]
		public virtual string DisplayName { get; set; }

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
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}
}
