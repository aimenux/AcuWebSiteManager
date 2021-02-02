using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Objects.AM;
using PX.Objects.AM.Attributes;
using PX.Data;

public partial class Page_AM215000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
	}

    const string BoldTextStyle = "BoldText";

    protected void Page_Init(object sender, EventArgs e)
    {
        Style style = new Style();
        style.Font.Bold = true;
        this.Page.Header.StyleSheet.CreateStyleRule(style, this, "." + BoldTextStyle);

        this.Master.PopupWidth = 960;
        this.Master.PopupHeight = 600;
    }

    protected void AMBomOper_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
    {
        var item = e.Row.DataItem as AMBomOper;
        if (item != null && item.RowStatus != AMRowStatus.Unchanged)
        {
            e.Row.Style.CssClass = BoldTextStyle;
        }
    }

    protected void AMBomMatl_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
    {
        var item = e.Row.DataItem as AMBomMatl;
        if (item != null && item.RowStatus != AMRowStatus.Unchanged)
        {
            e.Row.Style.CssClass = BoldTextStyle;
        }
    }

    protected void AMBomStep_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
    {
        var item = e.Row.DataItem as AMBomStep;
        if (item != null && item.RowStatus != AMRowStatus.Unchanged)
        {
            e.Row.Style.CssClass = BoldTextStyle;
        }
    }

    protected void AMBomTool_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
    {
        var result = e.Row.DataItem as PXResult;
        if (result == null) return;
        AMBomTool item = (AMBomTool)result[typeof(AMBomTool)];
        if (item != null && item.RowStatus != AMRowStatus.Unchanged)
        {
            e.Row.Style.CssClass = BoldTextStyle;
        }
    }
    protected void AMBomOvhd_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
    {
        var result = e.Row.DataItem as PXResult;
        if (result == null) return;
        AMBomOvhd item = (AMBomOvhd)result[typeof(AMBomOvhd)];
        if (item != null && item.RowStatus != AMRowStatus.Unchanged)
        {
            e.Row.Style.CssClass = BoldTextStyle;
        }
    }

    protected void AMBomRef_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
    {
        AMBomRef item = e.Row.DataItem as AMBomRef;
        if (item != null && item.RowStatus != AMRowStatus.Unchanged)
        {
            e.Row.Style.CssClass = BoldTextStyle;
        }
    }

    protected void AMBomAttribute_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
    {
        AMBomAttribute item = e.Row.DataItem as AMBomAttribute;
        if (item != null && item.RowStatus != AMRowStatus.Unchanged)
        {
            e.Row.Style.CssClass = BoldTextStyle;
        }
    }
}
