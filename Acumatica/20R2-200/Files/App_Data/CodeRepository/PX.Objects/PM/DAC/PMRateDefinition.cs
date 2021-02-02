namespace PX.Objects.PM
{
	using System;
	using PX.Data;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMRateDefinition)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMRateDefinition : PX.Data.IBqlTable
	{
		#region RateDefinitionID
		public abstract class rateDefinitionID : PX.Data.BQL.BqlInt.Field<rateDefinitionID> { }
		protected int? _RateDefinitionID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? RateDefinitionID
		{
			get
			{
				return this._RateDefinitionID;
			}
			set
			{
				this._RateDefinitionID = value;
			}
		}
		#endregion
		#region RateTableID
		public abstract class rateTableID : PX.Data.BQL.BqlString.Field<rateTableID> { }
		protected String _RateTableID;
		[PXDefault]
		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
		public virtual String RateTableID
		{
			get
			{
				return this._RateTableID;
			}
			set
			{
				this._RateTableID = value;
			}
		}
		#endregion
		#region RateTypeID
		public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		protected String _RateTypeID;
		[PXDefault]
		[PXDBString(PMRateType.rateTypeID.Length, IsUnicode = true)]
		public virtual String RateTypeID
		{
			get
			{
				return this._RateTypeID;
			}
			set
			{
				this._RateTypeID = value;
			}
		}
		#endregion
		#region Sequence
		public abstract class sequence : PX.Data.BQL.BqlShort.Field<sequence> { }
		protected Int16? _Sequence;
        [PXDefault(TypeCode.Int16, "1")]
		[PXDBShort]
		[PXUIField(DisplayName = "Sequence")]
		public virtual Int16? Sequence
		{
			get
			{
				return this._Sequence;
			}
			set
			{
				this._Sequence = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
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
		#region Project
		public abstract class project : PX.Data.BQL.BqlBool.Field<project> { }
		protected Boolean? _Project;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Project")]
		public virtual Boolean? Project
		{
			get
			{
				return this._Project;
			}
			set
			{
				this._Project = value;
			}
		}
		#endregion
		#region Task
		public abstract class task : PX.Data.BQL.BqlBool.Field<task> { }
		protected Boolean? _Task;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Project Task")]
		public virtual Boolean? Task
		{
			get
			{
				return this._Task;
			}
			set
			{
				this._Task = value;
			}
		}
		#endregion
		#region AccountGroup
		public abstract class accountGroup : PX.Data.BQL.BqlBool.Field<accountGroup> { }
		protected Boolean? _AccountGroup;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Account Group")]
		public virtual Boolean? AccountGroup
		{
			get
			{
				return this._AccountGroup;
			}
			set
			{
				this._AccountGroup = value;
			}
		}
		#endregion
		#region RateItem
		public abstract class rateItem : PX.Data.BQL.BqlBool.Field<rateItem> { }
		protected Boolean? _RateItem;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Inventory")]
		public virtual Boolean? RateItem
		{
			get
			{
				return this._RateItem;
			}
			set
			{
				this._RateItem = value;
			}
		}
		#endregion
		#region Employee
		public abstract class employee : PX.Data.BQL.BqlBool.Field<employee> { }
		protected Boolean? _Employee;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Employee")]
		public virtual Boolean? Employee
		{
			get
			{
				return this._Employee;
			}
			set
			{
				this._Employee = value;
			}
		}
		#endregion
		
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMTask.taskCD))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#endregion
	}
}
