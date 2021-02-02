using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INCategory)]
	public class INCategory : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INCategory>.By<categoryID>
		{
			public static INCategory Find(PXGraph graph, int? categoryID) => FindBy(graph, categoryID);
		}
		public static class FK
		{
			public class Parent : INCategory.PK.ForeignKeyOf<INCategory>.By<parentID> { }
		}
		#endregion
		#region CategoryID
		public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
		protected int? _CategoryID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Category ID", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual int? CategoryID
		{
			get
			{
				return this._CategoryID;
			}
			set
			{
				this._CategoryID = value;
			}
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBLocalizableString(Constants.TranDescLength, InputMask = "", IsUnicode = true)]
		[PXDefault]
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

		#region ParentID
		public abstract class parentID : PX.Data.BQL.BqlInt.Field<parentID> { }
		protected int? _ParentID;
		[PXDBInt]
		[PXDefault(0)]
        [PXUIField(DisplayName = "Parent Category")]
		public virtual int? ParentID
		{
			get
			{
				return this._ParentID;
			}
			set
			{
				this._ParentID = value ?? 0;
			}
		}
		#endregion


        #region TempChildID
        public abstract class tempChildID : PX.Data.BQL.BqlInt.Field<tempChildID> { }
        protected int? _TempChildID;
        [PXInt]
        public virtual int? TempChildID
        {
            get
            {
                return this._TempChildID;
            }
            set
            {
                this._TempChildID = value;
            }
        }
        #endregion

        #region TempparentID
        public abstract class tempParentID : PX.Data.BQL.BqlInt.Field<tempParentID> { }
        protected int? _TempParentID;
        [PXInt]
        public virtual int? TempParentID
        {
            get
            {
                return this._TempParentID;
            }
            set
            {
                this._TempParentID = value;
            }
        }
        #endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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

		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXDefault(0)]
		[PXDBInt]
		public virtual Int32? SortOrder
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
