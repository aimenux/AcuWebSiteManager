using System;
using PX.SM;
using PX.Web.UI;

public partial class Pages_SM_SM204530 : PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.ds.ToolBarSkin = "ModulesMenu";
	}
    protected void Page_Load(object sender, EventArgs e)
    {
	   //// this.ds.ToolBarSkin = "ModulesMenu";
	   // var panel = (PXSmartPanel) this.sp1.FindControl("PanelEditor");
	   // var g = (CustProjectMaint) this.ds.DataGraph;
	   // panel.InnerPageUrl = String.Format("~/Pages/SM/SM204520.aspx?ItemKey={0}&ItemContext={1}",
	   //									g.CurrentEditor.Current.ItemKey, 
	   //									g.CurrentEditor.Current.ItemContext);
    }
}