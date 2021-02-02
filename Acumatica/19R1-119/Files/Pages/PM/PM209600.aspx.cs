using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Objects.PM;

public partial class Page_PM209600 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		Style styleSummaryRecord = new Style();
		styleSummaryRecord.BackColor = System.Drawing.ColorTranslator.FromHtml("#B5D7EF");
		this.Page.Header.StyleSheet.CreateStyleRule(styleSummaryRecord, this, "." + "SummaryRecord");

		Style styleTotalRecord = new Style();
		styleTotalRecord.Font.Bold = true;
		this.Page.Header.StyleSheet.CreateStyleRule(styleTotalRecord, this, "." + "TotalRecord");

		Style styleDiffRecord = new Style();
		styleDiffRecord.Font.Bold = true;
		this.Page.Header.StyleSheet.CreateStyleRule(styleDiffRecord, this, "." + "DiffRecord");

		Style styleTotalErrorCell = new Style();
		styleTotalErrorCell.Font.Bold = true;
		styleTotalErrorCell.ForeColor = System.Drawing.ColorTranslator.FromHtml("#f00");
		this.Page.Header.StyleSheet.CreateStyleRule(styleTotalErrorCell, this, "." + "TotalErrorCell");


		this.Master.PopupWidth = 1000;
		this.Master.PopupHeight = 700;
	}

	
	protected void Grid_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		PMForecastRecord item = e.Row.DataItem as PMForecastRecord;
		if (item == null) return;

		if (item.IsSummary)
		{
			e.Row.Style.CssClass = "SummaryRecord";
		}
		else if ( item.IsTotal)
		{
			e.Row.Style.CssClass = "TotalRecord";
		}
		else if (item.IsTotal || item.IsDifference)
		{
			e.Row.Style.CssClass = "DiffRecord";
			if (item.Qty != 0)
			{
				e.Row.Cells["Qty"].Style.CssClass = "TotalErrorCell";
			}
			if (item.CuryAmount != 0)
			{
				e.Row.Cells["CuryAmount"].Style.CssClass = "TotalErrorCell";
			}
			if (item.RevisedQty != 0)
			{
				e.Row.Cells["RevisedQty"].Style.CssClass = "TotalErrorCell";
			}
			if (item.CuryRevisedAmount != 0)
			{
				e.Row.Cells["CuryRevisedAmount"].Style.CssClass = "TotalErrorCell";
			}
			if (item.ChangeOrderQty != 0)
			{
				e.Row.Cells["ChangeOrderQty"].Style.CssClass = "TotalErrorCell";
			}
			if (item.CuryChangeOrderAmount != 0)
			{
				e.Row.Cells["CuryChangeOrderAmount"].Style.CssClass = "TotalErrorCell";
			}
			if (item.ActualQty != 0)
			{
				e.Row.Cells["ActualQty"].Style.CssClass = "TotalErrorCell";
			}
			if (item.CuryActualAmount != 0)
			{
				e.Row.Cells["CuryActualAmount"].Style.CssClass = "TotalErrorCell";
			}
		}
	}
}
