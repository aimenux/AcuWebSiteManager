using System;
using PX.Objects.CR;

public partial class Page_CR303000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		Master.PopupHeight = 600;
		Master.PopupWidth = 800;
	}

	protected void edPrmCntID_EditRecord(object sender, PX.Web.UI.PXNavigateEventArgs e)
	{
		var baMaint = this.ds.DataGraph as BusinessAccountMaint;
		if (baMaint != null)
		{
			var ext = baMaint.GetExtension<BusinessAccountMaint.PrimaryContactGraphExt>();

			BAccount bAccount = baMaint.BAccount.Cache.Current as BAccount;
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
