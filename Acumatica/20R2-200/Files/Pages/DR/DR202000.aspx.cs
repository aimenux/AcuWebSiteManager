using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Objects.DR;
using PX.Web.UI;

public partial class Page_DR202000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.IsCallback)
		{
			PXGroupBox box = this.form.FindControl("gbPeriodically") as PXGroupBox;
			if (box != null)
			{
				PXLabel lbl = box.FindControl("PXLabel1") as PXLabel;
				if (lbl != null)
					lbl.Text = Messages.DocumentDateSelection;
			}
		}
	}
}
