using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.IN
{

	public class INPriceClassMaint : PXGraph<INPriceClassMaint>
	{
		public PXSelect<INPriceClass> Records;
        public PXSavePerRow<INPriceClass> Save;
		public PXCancel<INPriceClass> Cancel;

		protected virtual void INPriceClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			INPriceClass row = e.Row as INPriceClass;
			if (row != null)
			{
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof (DiscountInventoryPriceClass.inventoryPriceClassID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(InventoryItem.priceClassID));
			}
		}
	}
}
