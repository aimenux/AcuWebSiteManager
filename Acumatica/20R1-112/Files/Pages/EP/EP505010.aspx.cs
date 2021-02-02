using System;

public partial class Page_EP505010 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		/*Style escalated = new Style();
		escalated.ForeColor = System.Drawing.Color.Red;
		this.Page.Header.StyleSheet.CreateStyleRule(escalated, this, ".CssEscalated");*/
	}

	protected void Page_Init(object sender, EventArgs e)
	{
	}

	protected void grid_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		/*EmployeeClaimsApproval.EPExpenseClaimOwned item = e.Row.DataItem as EmployeeClaimsApproval.EPExpenseClaimOwned;
		if (item == null) return;
		if (item.Escalated == true)
			e.Row.Style.CssClass = "CssEscalated";*/
	}
}
