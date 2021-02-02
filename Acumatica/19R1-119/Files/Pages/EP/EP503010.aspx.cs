using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Objects.EP;

public partial class Page_EP503010 : PX.Web.UI.PXPage
{	
	protected void Page_Load(object sender, EventArgs e)
	{
		Style escalated = new Style();
		escalated.ForeColor = System.Drawing.Color.Red;
		this.Page.Header.StyleSheet.CreateStyleRule(escalated, this, ".CssEscalated");
	}

	protected void Page_Init(object sender, EventArgs e)
	{
        Master.PopupHeight = 460;
        Master.PopupWidth = 800;
	}
		
	protected void grid_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		EPApprovalProcess.EPOwned item = e.Row.DataItem as EPApprovalProcess.EPOwned;
		if (item == null) return;
		if (item.Escalated ==  true)		
			e.Row.Style.CssClass = "CssEscalated";		
	}
}
