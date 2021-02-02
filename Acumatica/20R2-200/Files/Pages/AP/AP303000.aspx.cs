using System;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Web.UI;

public partial class Page_TabView : PX.Web.UI.PXPage
{
    protected void Page_Init(object sender, EventArgs e)
    {
    }

	protected void edPrmCntID_EditRecord(object sender, PX.Web.UI.PXNavigateEventArgs e)
	{
		var vendorMaint = this.ds.DataGraph as VendorMaint;
		if (vendorMaint != null)
		{
			var ext = vendorMaint.GetExtension<VendorMaint.PrimaryContactGraphExt>();

			BAccount bAccount = vendorMaint.BAccount.Cache.Current as BAccount;
			if (ext != null && bAccount != null && (bAccount.PrimaryContactID == null || bAccount.PrimaryContactID < 0))
			{
				{
					try
					{
						ext.AddNewPrimaryContact.Press();
					}
					catch (PX.Data.PXRedirectRequiredException e1)
					{
						PX.Web.UI.PXBaseDataSource ds = this.ds as PX.Web.UI.PXBaseDataSource;
						PX.Web.UI.PXBaseDataSource.RedirectHelper helper = new PX.Web.UI.PXBaseDataSource.RedirectHelper(ds);
						helper.TryRedirect(e1);
					}
				}
			}
		}
	}
}
