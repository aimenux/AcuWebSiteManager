using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;

public partial class Page_CT301000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		Master.PopupWidth = 950;
		Master.PopupHeight = 600;
	}

    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!Page.IsCallback)
        //{
        //    PXGroupBox gb = (PXGroupBox)this.tab.FindControl("BillingGroupBox");
        //    ((PXRadioButton)gb.FindControl("rdbPerCase")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.CT.Messages.PerCase);
        //    ((PXRadioButton)gb.FindControl("rdbPerItem")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.CT.Messages.PerItem);
        //}
	}
}
