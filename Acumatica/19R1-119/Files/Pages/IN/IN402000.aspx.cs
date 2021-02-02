using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Web.UI;
using PX.Data;
using PX.Objects.IN;

public partial class Page_IN402000 : PX.Web.UI.PXPage
{
	const string BoldTextStyle = "BoldText";

	protected void Page_Init(object sender, EventArgs e)
	{
		Style style = new Style();
		style.Font.Bold = true;
		this.Page.Header.StyleSheet.CreateStyleRule(style, this, "." + BoldTextStyle);

		this.Master.PopupWidth = 960;
		this.Master.PopupHeight = 600;
	}

	protected void AdditionDeductionGrids_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		var item = e.Row.DataItem as PX.Objects.IN.InventoryQtyByPlanType;
		if (item != null && item.IsTotal.GetValueOrDefault())
		{
			e.Row.Style.CssClass = BoldTextStyle;
		}
	}
}
