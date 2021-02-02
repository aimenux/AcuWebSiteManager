using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CS
{
	public class FOBPointMaint : PXGraph<FOBPointMaint>
	{
        public PXSavePerRow<FOBPoint> Save;
		public PXCancel<FOBPoint> Cancel;
		[PXImport(typeof(FOBPoint))]
		public PXSelect<FOBPoint> FOBPoint;

		protected virtual void FOBPoint_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			FOBPoint fob = PXSelect<FOBPoint, Where<FOBPoint.fOBPointID, Equal<Required<FOBPoint.fOBPointID>>>>.SelectWindowed(this, 0, 1, ((FOBPoint)e.Row).FOBPointID);
			if (fob != null)
			{
				cache.RaiseExceptionHandling<FOBPoint.fOBPointID>(e.Row, ((FOBPoint)e.Row).FOBPointID, new PXException(Messages.RecordAlreadyExists));
				e.Cancel = true;
			}
		}
	}
}
