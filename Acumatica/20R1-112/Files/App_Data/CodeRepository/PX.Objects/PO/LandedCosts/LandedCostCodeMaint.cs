using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.PO
{
	public class LandedCostCodeMaint : PXGraph<LandedCostCodeMaint, LandedCostCode>
	{
		public PXSelect<LandedCostCode> LandedCostCode;						
		
		public LandedCostCodeMaint()
		{

		}

		protected virtual void LandedCostCode_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) 
		{
			LandedCostCode row = (LandedCostCode)e.Row;
			sender.SetDefaultExt<LandedCostCode.vendorLocationID>(e.Row);
			sender.SetDefaultExt<LandedCostCode.termsID>(e.Row);
			doCancel = true;
		}

		protected virtual void LandedCostCode_VendorLocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if(doCancel)
			{
				e.NewValue = ((LandedCostCode)e.Row).VendorLocationID;
				e.Cancel = true;
				doCancel = false;
			}
			
		}

		protected virtual void LandedCostCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			LandedCostCode row = (LandedCostCode)e.Row;
			if (row != null)
			{
				bool hasVendor = row.VendorID.HasValue;
				PXDefaultAttribute.SetPersistingCheck<LandedCostCode.vendorLocationID>(sender, e.Row, hasVendor ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<LandedCostCode.vendorLocationID>(sender, hasVendor);
				PXUIFieldAttribute.SetEnabled<LandedCostCode.vendorLocationID>(sender,e.Row,hasVendor);
				sender.RaiseExceptionHandling<LandedCostCode.vendorID>(row, row.VendorID, null);
				if (hasVendor) 
				{
					Vendor vnd = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(this, row.VendorID);
					if (vnd != null && vnd.LandedCostVendor == false)
					{
						sender.RaiseExceptionHandling<LandedCostCode.vendorID>(row, row.VendorID, new PXSetPropertyException(Messages.LCCodeUsesNonLCVendor, PXErrorLevel.Warning));
					}
				}
			}
		}

		protected virtual void LandedCostCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			//new LC entities should be handled here
		}



		private bool doCancel = false;

	}

}
