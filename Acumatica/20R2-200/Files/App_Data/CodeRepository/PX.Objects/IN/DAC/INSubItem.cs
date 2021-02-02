using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// Represents a Subitem (or subitem code).
	/// Subitems allow to track different variations of a product under one <see cref="InventoryItem"/>.
	/// Subitems are available only if the <see cref="FeaturesSet.SubItem">Inventory Subitems</see> feature is enabled.
	/// The records of this type are created and edited through the Subitems (IN.20.50.00) screen
	/// (corresponds to the <see cref="INSubItemMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
    [PXCacheName(Messages.INSubItem, PXDacType.Catalogue, CacheGlobal = true)]
    public partial class INSubItem : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSubItem>.By<subItemID>
		{
			public static INSubItem Find(PXGraph graph, int? subItemID) => FindBy(graph, subItemID);
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;

		/// <summary>
		/// Database identity.
		/// Unique identifier of the Subitem.
		/// </summary>
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Subitem ID", Enabled = false, Visible = false)]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;

		/// <summary>
		/// Key field.
		/// Unique user-friendly identifier of the Subitem.
		/// </summary>
		/// <value>
		/// The structure of the identifier is defined by the <i>INSUBITEM</i> <see cref="CS.Dimension">Segmented Key</see>
		/// and usually reflects the important properties associated with the <see cref="InventoryItem">item</see>.
		/// </value>
		[IN.SubItemRaw(IsKey=true)]
		[PXDefault()]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The description of the Subitem.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
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
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;

		/// <summary>
		/// The group mask showing which <see cref="PX.SM.RelationGroup">restriction groups</see> the Subitem belongs to.
		/// </summary>
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
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

		public class Zero : PX.Data.BQL.BqlString.Constant<Zero>
		{
			public Zero():base("0")
			{
			}
		}
	}
}
