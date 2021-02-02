using PX.Data;
using System;

namespace PX.Objects.IN.Attributes
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Property)]
	public class InventoryAllocationFieldAttribute : PXEventSubscriberAttribute
	{
		public bool IsAddition { get; set; }
		public string InclQtyFieldName { get; set; }
		public bool IsTotal { get; set; }
		public int SortOrder { get; set; }
	}
}
