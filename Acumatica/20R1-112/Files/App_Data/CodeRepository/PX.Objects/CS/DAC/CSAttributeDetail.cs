namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.AttributeDetail)]
	public partial class CSAttributeDetail : PX.Data.IBqlTable
	{
		public const int ParameterIdLength = 10;

		#region Keys
		public class PK : PrimaryKeyOf<CSAttributeDetail>.By<attributeID, valueID>
		{
			public static CSAttributeDetail Find(PXGraph graph, string attributeID, string valueID) => FindBy(graph, attributeID, valueID);
		}
		#endregion

		#region AttributeID
		public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
		protected String _AttributeID;
		[PXDBString(ParameterIdLength, IsKey = true)]
		[PXDBLiteDefault(typeof(CSAttribute.attributeID))]
		[PXUIField(DisplayName = "Attribute ID")]
		[PXParent(typeof(Select<CSAttribute, Where<CSAttribute.attributeID, Equal<Current<CSAttributeDetail.attributeID>>>>))]
		public virtual String AttributeID
		{
			get
			{
				return this._AttributeID;
			}
			set
			{
				this._AttributeID = value;
			}
		}
		#endregion
		#region ValueID
		public abstract class valueID : PX.Data.BQL.BqlString.Field<valueID> { }
		protected String _ValueID;
		[PXDBString(10, InputMask="", IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Value ID")]
		public virtual String ValueID
		{
			get
			{
				return this._ValueID;
			}
			set
			{
				this._ValueID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBLocalizableString(60, IsUnicode = true)]
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlShort.Field<sortOrder> { }
		protected Int16? _SortOrder;
		[PXDBShort()]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual Int16? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
        #region Disabled

        public abstract class disabled : PX.Data.BQL.BqlBool.Field<disabled> { }
        protected Boolean? _Disabled;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Disabled", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Boolean? Disabled
        {
            get
            {
                return this._Disabled;
            }
            set
            {
                this._Disabled = value;
            }
        }

		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
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
		#endregion
	}
}
