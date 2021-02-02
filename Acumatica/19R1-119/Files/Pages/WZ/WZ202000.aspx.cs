using System;
using PX.Data;
using PX.Objects.WZ;
using PX.SM;
using PX.Web.UI;

public partial class Page_WZ202000 : PXPage
{
    protected void Page_Init(object sender, EventArgs e)
    {
        
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    protected void taskFeatures_DataBound(object sender, PXGridRowEventArgs e)
    {
        WZTaskFeature feature = (WZTaskFeature)e.Row.DataItem;

        if (feature.Offset != null && feature.Offset > 0)
            e.Row.Cells["DisplayName"].Style.CssClass = "PropOffset" + feature.Offset;
    }
}
