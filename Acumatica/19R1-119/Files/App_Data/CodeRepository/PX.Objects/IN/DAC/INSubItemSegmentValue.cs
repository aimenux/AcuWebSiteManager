using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INSubItemSegmentValue)]
	public class INSubItemSegmentValue : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSubItemSegmentValue>.By<inventoryID, segmentID>
		{
			public static INSubItemSegmentValue Find(PXGraph graph, int? inventoryID, long? segmentID) => FindBy(graph, inventoryID, segmentID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INSubItemSegmentValue>.By<inventoryID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true, DirtyRead = true, DisplayName = "Inventory ID", Visible = false)]
		[PXParent(typeof(FK.InventoryItem))]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		public virtual Int32? InventoryID
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
		#region SegmentID
		public abstract class segmentID : PX.Data.BQL.BqlShort.Field<segmentID> { }
		protected Int16? _SegmentID;
		[PXDBShort(IsKey = true)]
		[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? SegmentID
		{
			get
			{
				return this._SegmentID;
			}
			set
			{
				this._SegmentID = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		protected String _Value;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible)]
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
	}
}
