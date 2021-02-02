using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Commerce.Objects.Availability
{
	[PXHidden] //TODO, Remove after merge
	public class InventoryItem : IBqlTable
	{
		#region InventoryID
		[PXDBInt(IsKey = true)]
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
		#region TemplateItemID
		public abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		[PXDBInt()]
		[PXUIField(DisplayName = "Template Item ID")]
		public virtual int? TemplateItemID
		{
			get;
			set;
		}
		#endregion

		#region Availability
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "Availability")]
		[BCItemAvailabilities.ListDef]
		public virtual string Availability { get; set; }
		public abstract class availability : IBqlField { }
		#endregion
	}
}