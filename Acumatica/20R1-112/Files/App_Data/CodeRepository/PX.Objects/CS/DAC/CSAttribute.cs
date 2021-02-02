namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	using System.Diagnostics;
    using System.Xml.Serialization;
	using PX.Data.BQL;

	[DebuggerDisplay("[{AttributeID}]: {Description}")]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(CSAttributeMaint)},
		new Type[] { typeof(Select<CSAttribute, 
			Where<CSAttribute.attributeID, Equal<Current<CSAttribute.attributeID>>>>)
		})]
	[PXCacheName(Messages.Attribute)]
	public partial class CSAttribute : PX.Data.IBqlTable
	{
		public const int Text = 1;
		public const int Combo = 2;
		public const int CheckBox = 4;
		public const int Datetime = 5;
		public const int MultiSelectCombo = 6;
		public const int GISelector = 7;
		public abstract class AttrType
		{
			public class giSelector : BqlInt.Constant<giSelector>
			{
				public giSelector() : base(GISelector)
				{

				}
			}
		}

		#region AttributeID
		public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
		protected String _AttributeID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Attribute ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CSAttribute.attributeID))]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region ControlType
		public abstract class controlType : PX.Data.BQL.BqlInt.Field<controlType> { }
		protected Int32? _ControlType;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Control Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXIntList(new int[] { Text, Combo, MultiSelectCombo, CheckBox, Datetime, GISelector }, new string[] { "Text", "Combo", "Multi Select Combo", "Checkbox", "Datetime", "Selector" })]
		public virtual Int32? ControlType
		{
			get
			{
				return this._ControlType;
			}
			set
			{
				this._ControlType = value;
			}
		}
		#endregion
		#region EntryMask
		public abstract class entryMask : PX.Data.BQL.BqlString.Field<entryMask> { }
		protected String _EntryMask;
		[PXDBString(60)]
		[PXUIField(DisplayName = "Entry Mask")]
		[PXUIVisible(typeof(Where<controlType, NotEqual<AttrType.giSelector>>))]
		public virtual String EntryMask
		{
			get
			{
				return this._EntryMask;
			}
			set
			{
				this._EntryMask = value;
			}
		}
		#endregion
		#region RegExp
		public abstract class regExp : PX.Data.BQL.BqlString.Field<regExp> { }
		protected String _RegExp;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Reg. Exp.")]
		[PXUIVisible(typeof(Where<controlType, NotEqual<AttrType.giSelector>>))]
		public virtual String RegExp
		{
			get
			{
				return this._RegExp;
			}
			set
			{
				this._RegExp = value;
			}
		}
		#endregion
		#region List
		public abstract class list : PX.Data.BQL.BqlString.Field<list> { }
		protected String _List;
        [XmlIgnore]
        [PXDBLocalizableString(IsUnicode = true)]
		public virtual String List
		{
			get
			{
				return this._List;
			}
			set
			{
				this._List = value;
			}
		}
		#endregion
        #region IsInternal

        public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }
        protected Boolean? _IsInternal;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Boolean? IsInternal
        {
            get
            {
                return this._IsInternal;
            }
            set
            {
                this._IsInternal = value;
            }
        }

		#endregion
		
		#region ContainsPersonalData
		public abstract class containsPersonalData : PX.Data.BQL.BqlBool.Field<containsPersonalData> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Contains Personal Data", FieldClass = FeaturesSet.gDPRCompliance.FieldClass)]
		public virtual bool? ContainsPersonalData { get; set; }
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

		#region SchemaObject
		public abstract class objectName : PX.Data.BQL.BqlString.Field<objectName> { }
		protected String _ObjectName;
		[PXDBString(512, InputMask = "", IsUnicode = true)]
		//[PXDefault()]
		[PXUIField(DisplayName = "Schema Object", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIVisible(typeof(Where<controlType, Equal<AttrType.giSelector>>))]
		//[PXSelector(typeof(CSAttribute.objectName))]
		[Data.Maintenance.GI.PXTablesSelector]
		public virtual String ObjectName
		{
			get
			{
				return this._ObjectName;
			}
			set
			{
				this._ObjectName = value;
			}
		}
		#endregion
		#region SchemaField
		public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }
		protected String _FieldName;
		[PXDBString(512, InputMask = "", IsUnicode = true)]
		//[PXDefault()]
		[PXUIField(DisplayName = "Schema Field")]
		[PXUIVisible(typeof(Where<controlType, Equal<AttrType.giSelector>>))]
		[PXStringList(new string[] { null }, new string[] { "" }, ExclusiveValues = false)]
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

	}
}
