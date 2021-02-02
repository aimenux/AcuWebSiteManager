using System;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Objects.SO;
using System.Drawing;

public partial class Page_IN301020 : PX.Web.UI.PXPage
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