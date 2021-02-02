using System;
using PX.Objects.CR;


public partial class Page_CR306000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupHeight = 700;
		this.Master.PopupWidth = 900;
	}

    protected void edContactID_EditRecord(object sender, PX.Web.UI.PXNavigateEventArgs e)
    {
        CRCaseMaint casemaint = this.ds.DataGraph as CRCaseMaint;
        if (casemaint != null)
        {
            CRCase currentcase = this.ds.DataGraph.Views[this.ds.DataGraph.PrimaryView].Cache.Current as CRCase;
            if (currentcase.ContactID == null && currentcase.CustomerID != null)
            {
                {
                    try
                    {
                        casemaint.addNewContact.Press();
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
