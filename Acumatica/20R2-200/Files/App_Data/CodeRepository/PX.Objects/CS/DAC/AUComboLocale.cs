using System;
using PX.Data;

namespace PX.Objects.CS
{
	[PXHidden]
	[SerializableAttribute]
	public class AUComboLocale : IBqlTable
	{
		#region TypeName
		public abstract class typeName : IBqlField
		{
		}
		protected String _TypeName;
		[PXDBString(128, IsKey = true, IsUnicode = true)]
		[PXDefault]
		public virtual String TypeName
		{
			get
			{
				return this._TypeName;
			}
			set
			{
				this._TypeName = value;
			}
		}
		#endregion
		#region FieldName
		public abstract class fieldName : IBqlField
		{
		}
		protected String _FieldName;
		[PXDBString(128, IsKey = true)]
		[PXDefault]
		public virtual String FieldName
		{
			get
			{
				return this._FieldName;
			}
			set
			{
				this._FieldName = value;
			}
		}
		#endregion
		#region LocaleName
		public abstract class localeName : IBqlField
		{
		}
		protected String _LocaleName;
		[PXDBString(5, IsKey = true, IsFixed = true, InputMask = "aa->aa")]
		[PXDefault]
		[PXUIField(DisplayName = "Language Name")]
		public virtual String LocaleName
		{
			get
			{
				return this._LocaleName;
			}
			set
			{
				this._LocaleName = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : IBqlField
		{
		}
		protected String _Value;
		[PXDBString(10, IsKey = true)]
		[PXDefault]
		public virtual String Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this._Value = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : IBqlField
		{
		}
		protected String _Description;
		[PXDBString(128, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description")]
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
		#region CreatedByID
		public abstract class createdByID : IBqlField
		{
		}
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
		public abstract class createdByScreenID : IBqlField
		{
		}
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
		public abstract class createdDateTime : IBqlField
		{
		}
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
		public abstract class lastModifiedByID : IBqlField
		{
		}
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
		public abstract class lastModifiedByScreenID : IBqlField
		{
		}
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
		public abstract class lastModifiedDateTime : IBqlField
		{
		}
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
		#region TStamp
		public abstract class tStamp : IBqlField
		{
		}
		protected Byte[] _TStamp;
		[PXDBTimestamp]
		public virtual Byte[] TStamp
		{
			get
			{
				return this._TStamp;
			}
			set
			{
				this._TStamp = value;
			}
		}
		#endregion
	}
}
