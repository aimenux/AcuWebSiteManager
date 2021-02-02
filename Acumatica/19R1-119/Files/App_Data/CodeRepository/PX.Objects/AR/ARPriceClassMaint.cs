using System;
using System.Collections.Generic;
using System.Text;

using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.AR
{
	public class ARPriceClassMaint : PXGraph<ARPriceClassMaint>
	{
        public PXSavePerRow<ARPriceClass> Save;
		public PXCancel<ARPriceClass> Cancel;
		public PXSelect<ARPriceClass> Records;


		protected virtual void ARPriceClass_CustPriceClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARPriceClass row = e.Row as ARPriceClass;
			if (row != null)
			{
				if ( ARPriceClass.EmptyPriceClass == e.NewValue.ToString())
				{
					e.Cancel = true;
					if (sender.RaiseExceptionHandling<ARPriceClass.priceClassID>(e.Row, null, new PXSetPropertyException(Messages.ReservedWord, ARPriceClass.EmptyPriceClass)))
					{
						throw new PXSetPropertyException(typeof(ARPriceClass.priceClassID).Name, null, Messages.ReservedWord, ARPriceClass.EmptyPriceClass);
					}
				}
			}
		}


		protected virtual void ARPriceClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ARPriceClass row = e.Row as ARPriceClass;
			if (row != null)
			{
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(DiscountCustomerPriceClass.customerPriceClassID));

				/* TODO: add customer(location) ref. */
			}
		}
	}
}