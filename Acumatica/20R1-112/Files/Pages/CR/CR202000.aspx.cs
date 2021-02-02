using System;
using PX.Objects.CR;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Page_CR202000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	}
	protected void edProjectTaskID_EditRecord(object sender, PX.Web.UI.PXNavigateEventArgs e)
	{
		var campaignMaint = this.ds.DataGraph as CampaignMaint;
		if (campaignMaint != null)
		{
			var currentCampaign = this.ds.DataGraph.Views[this.ds.DataGraph.PrimaryView].Cache.Current as CRCampaign;
			if (currentCampaign.ProjectID.HasValue && !currentCampaign.ProjectTaskID.HasValue)
			{
				{
					try
					{
						campaignMaint.addNewProjectTask.Press();
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
