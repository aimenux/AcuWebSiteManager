namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.KitSpecification, PXDacType.Config)]
	public partial class INKitSpecHdr : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INKitSpecHdr>.By<kitInventoryID, revisionID>
		{
			public static INKitSpecHdr Find(PXGraph graph, int? kitInventoryID, string revisionID) => FindBy(graph, kitInventoryID, revisionID);
		}
		public static class FK
		{
			public class KitInventoryItem : InventoryItem.PK.ForeignKeyOf<INKitSpecHdr>.By<kitInventoryID> { }
			public class KitSubItem : INSubItem.PK.ForeignKeyOf<INKitSpecHdr>.By<kitSubItemID> { }
		}
		#endregion
		#region KitInventoryID
		public abstract class kitInventoryID : PX.Data.BQL.BqlInt.Field<kitInventoryID> { }
		protected Int32? _KitInventoryID;
        [Inventory(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Kit Inventory ID")]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, Equal<boolTrue>>), Messages.InventoryItemIsNotaKit)]
		[PXDefault()]
		[PX.Data.EP.PXFieldDescription]
        [PXParent(typeof(FK.KitInventoryItem))]
        public virtual Int32? KitInventoryID
		{
			get
			{
				return this._KitInventoryID;
			}
			set
			{
				this._KitInventoryID = value;
			}
		}
		#endregion
        #region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
		protected String _RevisionID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INKitSpecHdr.revisionID,
			Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>>))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region KitSubItemID
		public abstract class kitSubItemID : PX.Data.BQL.BqlInt.Field<kitSubItemID> { }
		protected Int32? _KitSubItemID;
		[IN.SubItem(typeof(INKitSpecHdr.kitInventoryID))]
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<INKitSpecHdr.kitInventoryID>))]
		public virtual Int32? KitSubItemID
		{
			get
			{
				return this._KitSubItemID;
			}
			set
			{
				this._KitSubItemID = value;
			}
		}
		#endregion
        #region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region AllowCompAddition
		public abstract class allowCompAddition : PX.Data.BQL.BqlBool.Field<allowCompAddition> { }
		protected Boolean? _AllowCompAddition;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Component Addition")]
		public virtual Boolean? AllowCompAddition
		{
			get
			{
				return this._AllowCompAddition;
			}
			set
			{
				this._AllowCompAddition = value;
			}
		}
		#endregion
		#region IsNonStock
		public abstract class isNonStock : PX.Data.BQL.BqlBool.Field<isNonStock> { }
		protected Boolean? _IsNonStock;
		[PXBool()]
		[PXUIField(DisplayName = "Non-Stock", Enabled = false)]
        [PXDependsOnFields(typeof(kitInventoryID))]
		public virtual Boolean? IsNonStock
		{
			get
			{
				return this._IsNonStock;
			}
			set
			{
				this._IsNonStock = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(INKitSpecHdr.revisionID),
			Selector = typeof(Search<INKitSpecHdr.revisionID>), 
			FieldList = new [] { typeof(INKitSpecHdr.kitInventoryID), typeof(INKitSpecHdr.revisionID), typeof(INKitSpecHdr.descr) })]
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
