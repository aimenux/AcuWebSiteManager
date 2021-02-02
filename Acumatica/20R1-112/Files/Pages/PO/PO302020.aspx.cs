using System;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.Objects.PO;
using System.Drawing;

public partial class Page_PO302020 : PX.Web.UI.PXPage
{
	private static class PickCss
	{
		public const string Complete = "CssComplete";
		public const string Partial = "CssPartial";
		public const string Overpick = "CssOverpick";
	}

	protected void Page_Init(object sender, EventArgs e) { }

	protected void Page_Load(object sender, EventArgs e)
	{
		RegisterStyle(PickCss.Complete, null, Color.Green, true);
		RegisterStyle(PickCss.Partial, null, Color.Black, true);
		RegisterStyle(PickCss.Overpick, null, Color.OrangeRed, true);
	}

	private void RegisterStyle(string name, Color? backColor, Color? foreColor, bool bold)
	{
		Style style = new Style();
		if (backColor.HasValue) style.BackColor = backColor.Value;
		if (foreColor.HasValue) style.ForeColor = foreColor.Value;
		if (bold) style.Font.Bold = true;
		this.Page.Header.StyleSheet.CreateStyleRule(style, this, "." + name);
	}

	protected void ReceiveGrid_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		var receiptLine = (POReceiptLineSplit)PXResult.UnwrapMain(e.Row.DataItem);
		if (receiptLine == null) return;

		var grid = (PX.Web.UI.PXGrid)sender;
		var reGraph = (ReceivePutAwayHost)grid.DataGraph;

		if (reGraph.Document.Current.With(r => r.WMSSingleOrder == true))
		{
			decimal? restQty = ReceivePutAway.RestQty.GetValue(reGraph, receiptLine);

			if (restQty == 0)
			{
				e.Row.Style.CssClass = PickCss.Complete;
			}
			else if (receiptLine.ReceivedQty > 0)
			{
				e.Row.Style.CssClass = PickCss.Partial;
			}
		}
		else
		{
			if (receiptLine.ReceivedQty > receiptLine.Qty)
			{
				e.Row.Style.CssClass = PickCss.Overpick;
			}
			else if (receiptLine.ReceivedQty == receiptLine.Qty)
			{
				e.Row.Style.CssClass = PickCss.Complete;
			}
			else if (receiptLine.ReceivedQty > 0)
			{
				e.Row.Style.CssClass = PickCss.Partial;
			}
		}
	}

	protected void PutAwayGrid_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		var shipLine = (POReceiptLineSplit)PXResult.UnwrapMain(e.Row.DataItem);
		if (shipLine == null) return;

		if (shipLine.PutAwayQty > shipLine.Qty)
		{
			e.Row.Style.CssClass = PickCss.Overpick;
		}
		else if (shipLine.PutAwayQty == shipLine.Qty)
		{
			e.Row.Style.CssClass = PickCss.Complete;
		}
		else if (shipLine.PutAwayQty > 0)
		{
			e.Row.Style.CssClass = PickCss.Partial;
		}
	}

	protected void LogGrid_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		var log = (PX.Objects.IN.WMSScanLog)PXResult.UnwrapMain(e.Row.DataItem);
		if (log == null) return;

		if (log.MessageType == PX.Objects.IN.WMSMessageTypes.Error)
		{
			e.Row.Style.CssClass = PickCss.Overpick;
		}
		else if (log.MessageType == PX.Objects.IN.WMSMessageTypes.Warning)
		{
			e.Row.Style.CssClass = PickCss.Partial;
		}
	}
}