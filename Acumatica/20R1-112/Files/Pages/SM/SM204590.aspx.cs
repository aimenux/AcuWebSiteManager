using System;
using System.Web;
using PX.Data;
using PX.Web.Customization;
using PX.Web.UI;

[Customization.CstDesignMode(Disabled = true)]
public partial class Pages_SM_SM204590 : PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		((IPXMasterPage)this.Master).CustomizationAvailable = false;
		this.Master.FindControl("usrCaption").Visible = false;
		ProjectBrowserMaint.InitSessionFromQueryString(HttpContext.Current);

		//string screenID = this.Request.QueryString["EditScreenID"];
		//if (!string.IsNullOrEmpty(screenID)) ProjectBrowserMaint.ContextScreenID = screenID;
	}

	protected void Page_Load(object sender, EventArgs e)
	{

	}

	public string GetScriptName(string rname)
	{
		string resource = "PX.Web.Customization.Controls.cseditor." + rname;
		string url = ClientScript.GetWebResourceUrl(typeof(Customization.WebsiteEntryPoints), resource);
		url = url.Replace(".axd?", ".axd?file=" + rname + "&");
		return HttpUtility.HtmlAttributeEncode(url);
		//			return VirtualPathUtility.GetFileName(url);

	}

	protected override void OnPreRenderComplete(EventArgs e)
	{
		string query = ProjectBrowserMaint.ContextScreenID;
		if (!string.IsNullOrEmpty(query))
		{
			this.ClientScript.RegisterStartupScript(this.GetType(), "query",
				string.Format("\nvar __queryString = '{0}={1}'; ", "EditScreenID", query), true);
		}
		base.OnPreRenderComplete(e);
	}
}
