using System;
using System.Web;
using PX.Data;
using PX.Web.Customization;
using PX.Web.UI;

[Customization.CstDesignMode(Disabled = true)]
public partial class Page_AU220010 : PXPage
{
    protected override void OnInit(EventArgs e)
    {

        this.Master.FindControl("usrCaption").Visible = false;
        ProjectBrowserMaint.InitSessionFromQueryString(HttpContext.Current);


        ((IPXMasterPage)this.Master).CustomizationAvailable = false;
        base.OnInit(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// The page PreRenderComplete event handler.
    /// </summary>
    protected override void OnPreRenderComplete(EventArgs e)
    {
        string query = ProjectBrowserMaint.ContextCodeFile;
        if (!string.IsNullOrEmpty(query))
        {
            this.ClientScript.RegisterStartupScript(this.GetType(), "query",
                string.Format("\nvar __queryString = '{0}={1}'; ", "EditMobileScreenID", query.Replace('#', '*')), true);
        }
        base.OnPreRenderComplete(e);
    }

    public string GetScriptName(string rname)
    {
        string resource = "PX.Web.Customization.Controls.cseditor." + rname;
        string url = ClientScript.GetWebResourceUrl(typeof(Customization.WebsiteEntryPoints), resource);
        url = url.Replace(".axd?", ".axd?file=" + rname + "&");
        return HttpUtility.HtmlAttributeEncode(url);
    }
}