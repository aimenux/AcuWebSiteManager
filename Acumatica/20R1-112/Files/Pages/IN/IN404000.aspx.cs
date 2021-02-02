using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Objects.IN;
using PX.Web.UI;

public partial class Page_IN404000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.IsCallback)
		{
			PXLabel lbl = this.form.FindControl("lblNote1") as PXLabel;
			if (lbl != null)
				lbl.Text = Messages.EstimatedCosts;
		}
	}
}
