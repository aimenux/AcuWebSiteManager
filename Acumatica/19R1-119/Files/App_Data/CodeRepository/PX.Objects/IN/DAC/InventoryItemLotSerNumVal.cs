using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.IN
{
	/// <summary>
	/// Represents a Auto-Incremental Value of a Stock Item.
	/// Auto-Incremental Value of a Stock Item are available only if the <see cref="FeaturesSet.LotSerialTracking">Lot/Serial Tracking</see> feature is enabled.
	/// The records of this type are created through the Stock Items (IN.20.25.00) screen
	/// (corresponds to the <see cref="InventoryItemMaint"/> graph)
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(InventoryItemMaint))]
	[PXCacheName(Messages.StockItemAutoIncrementalValue)]
	public class InventoryItemLotSerNumVal : IBqlTable, ILotSerNumVal
	{
		#region Keys
		public class PK : PrimaryKeyOf<InventoryItemLotSerNumVal>.By<inventoryID>
		{
			public static InventoryItemLotSerNumVal Find(PXGraph graph, int? inventoryID) => FindBy(graph, inventoryID);
			public static InventoryItemLotSerNumVal FindDirty(PXGraph graph, int? inventoryID)
				=> (InventoryItemLotSerNumVal)PXSelect<InventoryItemLotSerNumVal,
					Where<inventoryID, Equal<Required<inventoryID>>>>.SelectWindowed(graph, 0, 1, inventoryID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<InventoryItemLotSerNumVal>.By<inventoryID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		protected int? _InventoryID;

		/// <summary>
		/// The unique identifier of the Inventory Item.
		/// </summary>
		/// <summary>
		/// The <see cref="InventoryItem">inventory item</see>, to which the item is assigned.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="InventoryItem.inventoryID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(FK.InventoryItem))]
		public virtual int? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region LotSerNumVal
		public abstract class lotSerNumVal : BqlString.Field<lotSerNumVal> { }
		protected string _LotSerNumVal;

		/// <summary>
		/// The start value for the auto-incremented numbering segment.
		/// This field is relevant only if the <see cref="FeaturesSet.LotSerialTracking">Lot/Serial Tracking</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INLotSerClass.LotSerNumVal"/> of the corresponding <see cref="LotSerClassID">Lot/Serial Class</see>
		/// if <see cref="INLotSerClass.LotSerNumShared"/> is set to <c>true</c>.
		/// </value>
		[PXDBString(30, IsUnicode = true, InputMask = "999999999999999999999999999999")]
		[PXUIField(DisplayName = "Auto-Incremental Value")]
		public virtual string LotSerNumVal
		{
			get
			{
				return this._LotSerNumVal;
			}
			set
			{
				this._LotSerNumVal = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
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
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
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
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
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
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
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
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
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
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
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
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		protected byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual byte[] tstamp
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
