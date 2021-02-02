using PX.Commerce.Core;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using System;
using InventoryItem = PX.Commerce.Objects.Availability.InventoryItem;

namespace PX.Commerce.Objects.Substitutes
{
	[Serializable]
	[PXHidden]
	public class BCChildrenInventoryItem : InventoryItem
	{
		#region InventoryID
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID")]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : IBqlField { }
		#endregion
		#region InventoryCD
		[PXDBString]
		[PXUIField(DisplayName = "Inventory CD")]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : IBqlField { }
		#endregion
		#region NoteID
		public abstract class noteID : IBqlField { }
		[PXDBGuid()]
		[PXUIField(DisplayName = "NoteID")]
		public virtual Guid? NoteID
		{
			get; set;
		}
		#endregion
	}
	[Serializable]
	[PXHidden]
	public class BCChildrenRelatedInventory : INRelatedInventory
	{
		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID")]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : IBqlField { }
		#endregion
		#region InventoryCD
		[PXDBInt]
		[PXUIField(DisplayName = "Related Inventory ID")]
		public virtual int? RelatedInventoryID { get; set; }
		public abstract class relatedInventoryID : IBqlField { }
		#endregion
		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Is Active")]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : IBqlField { }
		#endregion
	}
	[Serializable]
	[PXHidden]
	public class BCParentInventoryItem : InventoryItem
	{
		#region InventoryID
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID")]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : IBqlField { }
		#endregion
		#region InventoryCD
		[PXDBString]
		[PXUIField(DisplayName = "Inventory CD")]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : IBqlField { }
		#endregion
		#region TemplateItemID
		public abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		public virtual int? TemplateItemID { set; get; }
		#endregion
		#region NoteID
		public abstract class noteID : IBqlField { }
		[PXDBGuid()]
		[PXUIField(DisplayName = "NoteID")]
		public virtual Guid? NoteID
		{
			get; set;
		}
		#endregion
	}
	[Serializable]
	[PXHidden]
	public class ChildSyncStatus : BCSyncStatus
	{
		#region SyncID
		public abstract class syncID : PX.Data.BQL.BqlInt.Field<BCSyncStatus.syncID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual int? SyncID { get; set; }
		#endregion
		#region ConnectorType
		[PXDBString()]
		public virtual string ConnectorType { get; set; }
		public abstract class connectorType : PX.Data.BQL.BqlString.Field<connectorType> { }
		#endregion
		#region BindingID
		[PXDBInt()]
		public virtual int? BindingID { get; set; }
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion+
		#region EntityType
		public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		[PXDBString()]
		public virtual string EntityType { get; set; }
		#endregion

		#region LocalID
		public abstract class localID : PX.Data.BQL.BqlGuid.Field<localID> { }
		[PXDBGuid()]
		public virtual Guid? LocalID { get; set; }
		#endregion

		#region ExternID
		public abstract class externID : PX.Data.BQL.BqlString.Field<externID> { }
		[PXDBString(64, IsUnicode = true)]
		public virtual string ExternID { get; set; }
		#endregion
	}
}
