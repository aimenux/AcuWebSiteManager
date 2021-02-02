using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;

namespace PX.Objects.IN
{
	public class INMovementClassMaint : PXGraph<INMovementClassMaint>
	{
		public PXSelect<INMovementClass> MovementClasses;
        public PXSavePerRow<INMovementClass> Save;
        public PXCancel<INMovementClass> Cancel;
        		
		protected virtual void INMovementClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            //decimal total;
		}

		public virtual void INMovementClass_CountsPerYear_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if (e.NewValue != null)
            {
                if ( (((short)e.NewValue) < 0) || (((short)e.NewValue) > 365) )
                {
					throw new PXSetPropertyException(Messages.ThisValueShouldBeBetweenP0AndP1, PXErrorLevel.Error, 0, 365);
				}
            }
        }
	}


}
