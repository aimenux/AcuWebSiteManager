using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Frames_Outlook_default : System.Web.UI.Page
{
	private const string fontAwesomeHref = "~/Content/font-awesome.css";
	protected void Page_Load(object sender, EventArgs e)
	{

	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
			var fa = new HtmlLink() { Href = fontAwesomeHref };
			fa.Attributes["type"] = "text/css";
			fa.Attributes["rel"] = "stylesheet";
			this.Page.Header.Controls.AddAt(0, fa);
		}
	}
}