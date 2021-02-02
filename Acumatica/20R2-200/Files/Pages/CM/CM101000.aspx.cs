using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Web.UI;
using PX.Objects.CM;

public partial class Page_CM101000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.IsCallback)
		{
			PXLabel lbl = this.formSettings.FindControl("lblPeriodsNumberAfter") as PXLabel;
			if (lbl != null)
				lbl.Text = PXMessages.LocalizeNoPrefix(Messages.Periods);
		}
	}
}
