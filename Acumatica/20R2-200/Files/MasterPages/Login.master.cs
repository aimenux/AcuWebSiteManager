using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Web.UI;
using PX.Common;

public partial class MasterPages_Login : System.Web.UI.MasterPage
{
	/// <summary>
	/// Gets or sets the message text.
	/// </summary>
	public string Message
	{
		get { return this.lblMsg.Text; }
		set { this.lblMsg.Text = value; }
	}

	private const string fontAwesomeHref = "~/Content/font-awesome.css";

	/// <summary>
	/// 
	/// </summary>
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!PX.Translation.ResourceCollectingManager.IsStringCollecting)
		{
			this.Response.TryAddHeader("cache-control", "no-store, private");
		}

		if (!this.Page.IsCallback)
		{
			var fa = new HtmlLink() { Href = fontAwesomeHref };
			fa.Attributes["type"] = "text/css";
			fa.Attributes["rel"] = "stylesheet";
			this.Page.Header.Controls.AddAt(0, fa);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	protected void Page_Load(object sender, EventArgs e)
	{
		if (this.Page.Request.UrlReferrer != null && this.Page.Request.UrlReferrer.AbsolutePath.IndexOf("/Frames/Outlook/") >= 0
			|| __isOutlook.Value == "1")
			__isOutlook.Value = "1";

		string copyR = PXVersionInfo.Copyright;
		String version = PXMessages.LocalizeFormatNoPrefix(PX.AscxControlsMessages.PageTitle.Version, PXVersionInfo.Version);

		if (!PX.Data.Update.PXUpdateHelper.VersionsEquals()) version = "<b style=\"color:red\">" + version + "</b>";
		if (!PX.Data.Update.PXUpdateHelper.ChectUpdateStatus())
		{
			version = "<b style=\"color:red\">" + version + "</b>";
		}
		lblCopy.Text = PXMessages.LocalizeNoPrefix(copyR) + "<br>Acumatica Cloud ERP " + PXVersionInfo.ProductVersion + "<br>" + version;

		string cstProjects = Customization.CstWebsiteStorage.PublishedProjectList;
		if (!string.IsNullOrEmpty(cstProjects))
		{
			this.lblCstProjects.Visible = true;
			this.lblCstProjects.Text = PXMessages.LocalizeFormatNoPrefix(Msg.CustomizedLabel, cstProjects.Replace(",", ", "));
		}
		this.CorrectCssUrl();
	}

	/// <summary>
	/// The page PreRender event handler.
	/// </summary>
	protected override void OnPreRender(EventArgs e)
	{
		string fileName = null;
		if (!this.IsPostBack || string.IsNullOrEmpty(txtLoginBgIndex.Value))
		{
			var path = Path.Combine(this.Request.PhysicalApplicationPath, "Icons");
			string[] files = Directory.GetFiles(path, "login_bg*.*");
			if (files.Length > 0)
			{
				var r = new Random();
				int index = Math.Min(r.Next(0, files.Length), files.Length - 1);
				txtLoginBgIndex.Value = fileName = Path.GetFileName(files[index]);
			}
		}
		else fileName = ControlHelper.PreventXSSAttacks(txtLoginBgIndex.Value);

		if (!string.IsNullOrEmpty(fileName))
		{
			string url = this.ResolveUrl(string.Format("../Icons/{0}", fileName));
			this.Page.ClientScript.RegisterClientScriptBlock(
				this.GetType(), "LoginImage", string.Format("var __loginBg = '{0}';", url), true);
		}
		base.OnPreRender(e);
	}

	/// <summary>
	/// Append timestamp to CSS file url.
	/// </summary>
	private void CorrectCssUrl()
	{
		foreach (Control ctrl in this.Page.Header.Controls)
		{
			var link = ctrl as System.Web.UI.HtmlControls.HtmlLink;
			if (link != null && !string.IsNullOrEmpty(link.Href) && !link.Href.Contains("timestamp"))
			{
				string filePath = link.Href.Replace("~/", this.Request.PhysicalApplicationPath).Replace('/', '\\');
				bool exists = File.Exists(filePath);
				if (exists) link.Href += string.Format("?timestamp={0}", File.GetLastWriteTimeUtc(filePath).Ticks.ToString());
			}
		}
	}
}
