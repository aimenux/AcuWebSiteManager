using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CS
{
	public class ShippingZoneMaint : PXGraph<ShippingZoneMaint>
	{
        public PXSavePerRow<ShippingZone> Save;
		public PXCancel<ShippingZone> Cancel;
		[PXImport(typeof(ShippingZone))]
		public PXSelect<ShippingZone> ShippingZones;

		protected virtual void ShippingZone_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			ShippingZone zone = PXSelect<ShippingZone, Where<ShippingZone.zoneID, Equal<Required<ShippingZone.zoneID>>>>.SelectWindowed(this, 0, 1, ((ShippingZone)e.Row).ZoneID);
			if (zone != null)
			{
				cache.RaiseExceptionHandling<ShippingZone.zoneID>(e.Row, ((ShippingZone)e.Row).ZoneID, new PXException(ErrorMessages.RecordExists));
				e.Cancel = true;
			}
		}
	}
}
