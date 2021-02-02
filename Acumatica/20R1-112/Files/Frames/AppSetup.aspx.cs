using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Pages_Ledger : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		this.Master.ScreenID = "11.00.00.00";
		this.Master.ScreenTitle = "#AS000000";
	}

	/// <summary>
	/// Sets the navigation links images.
	/// </summary>
	protected void siteLinks_ItemDataBound(object sender, DataListItemEventArgs e)
	{
		INavigateUIData dataItem = e.Item.DataItem as INavigateUIData;
		if (dataItem != null && !string.IsNullOrEmpty(dataItem.Description))
		{
			string descr = dataItem.Description;
			Image image = (Image)e.Item.FindControl("linkIm");
			image.ImageUrl = this.ResolveUrl/**/(descr.Split('|')[0]);
		}
	}
}
