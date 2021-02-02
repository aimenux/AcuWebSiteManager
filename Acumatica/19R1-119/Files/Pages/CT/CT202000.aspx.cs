using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;

public partial class Page_CT202000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
        //PXGroupBox gb = (PXGroupBox)this.tab.FindControl("BillingGroupBox");
        //if (!this.IsCallback)
        //{
        //    ((PXRadioButton)gb.FindControl("rdbPerCase")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.CT.Messages.PerCase);
        //    ((PXRadioButton)gb.FindControl("rdbPerItem")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.CT.Messages.PerItem);
        //}
	}
}
