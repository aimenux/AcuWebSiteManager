using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;

namespace PX.Objects.IN
{
	public class INPostClassMaint : PXGraph<INPostClassMaint, INPostClass>
	{
		public PXSelect<INPostClass> postclass;
		public PXSelect<INPostClass, Where<INPostClass.postClassID,Equal<Current<INPostClass.postClassID>>>> postclassaccounts;
		public PXSetup<INSetup> insetup;

		public INPostClassMaint()
		{
			INSetup record = insetup.Select();

			PXUIFieldAttribute.SetVisible<INPostClass.pPVAcctID>(postclass.Cache, null, true);
			PXUIFieldAttribute.SetVisible<INPostClass.pPVSubID>(postclass.Cache, null, true);
			PXUIFieldAttribute.SetVisible<INPostClass.pPVAcctDefault>(postclass.Cache, null, true);
			PXUIFieldAttribute.SetVisible<INPostClass.pPVSubMask>(postclass.Cache, null, true);
		}

		protected virtual void INPostClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<INPostClass.cOGSSubMask>(sender, e.Row, (e.Row != null && ((INPostClass)e.Row).COGSSubFromSales == false));

			INAcctSubDefault.Required(sender, e);
		}


		protected virtual void INPostClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
            INAcctSubDefault.Required(sender, e);
		}

		protected virtual void INPostClass_COGSSubFromSales_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateCOGSSubMask(sender, e);
		}

		protected virtual void INPostClass_SalesSubMask_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateCOGSSubMask(sender, e);
		}

		public virtual void UpdateCOGSSubMask(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INPostClass row = e.Row as INPostClass;
			if (row != null && row.COGSSubFromSales == true)
			{
				sender.SetValueExt<INPostClass.cOGSSubMask>(row, row.SalesSubMask);
			}
		}
	}
}
