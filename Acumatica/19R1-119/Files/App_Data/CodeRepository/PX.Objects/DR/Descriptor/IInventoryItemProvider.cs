using System.Collections.Generic;

using PX.Objects.IN;

namespace PX.Objects.DR.Descriptor
{
	public struct InventoryItemComponentInfo
	{
		public InventoryItem Item { get; set; }
		public INComponent Component { get; set; }
		public DRDeferredCode DeferralCode { get; set; }
	}

	public interface IInventoryItemProvider
	{
		/// <summary>
		/// Given an inventory item ID and the required component allocation method, 
		/// retrieves all inventory item components matching this method, along
		/// with the corresponding deferral codes in the form of 
		/// <see cref="InventoryItemComponentInfo"/>. If the allocation method does
		/// not support deferral codes, them <see cref="InventoryItemComponentInfo.DeferralCode"/>
		/// will be <c>null</c>.
		/// </summary>
		IEnumerable<InventoryItemComponentInfo> GetInventoryItemComponents(int? inventoryItemID, string allocationMethod);

		/// <summary>
		/// Given an inventory item component, returns the corresponding 
		/// substitute natural key - component name that as specified by
		/// <see cref="InventoryItem.InventoryCD"/>.
		/// </summary>
		string GetComponentName(INComponent component);
	}
}