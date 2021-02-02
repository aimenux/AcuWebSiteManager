using PX.Data;
using PX.Objects.Common.Attributes;
using System;

namespace PX.Objects.SO.Attributes
{
	public class CopiedShipmentNoteIDAttribute : CopiedNoteIDAttribute
	{
		public CopiedShipmentNoteIDAttribute(params Type[] searches)
			: base(null, searches)
		{
		}

		protected override string GetEntityType(PXCache cache, Guid? noteId)
		{
			var orderShipment = new PXSelect<SOOrderShipment,
				Where<SOOrderShipment.shippingRefNoteID, Equal<Required<Note.noteID>>>>(cache.Graph).Select(noteId);
			if (orderShipment != null)
			{
				ShippingRefNoteAttribute.GetTargetTypeAndKeys(orderShipment, out Type targetType, out object[] targetKeys);
				if (targetType != null)
					return targetType.FullName;
			}
			return cache.GetItemType().FullName;
		}
	}
}
