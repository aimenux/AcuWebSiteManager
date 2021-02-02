using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Linq;
using PX.SM;
using PX.Data;

public partial class Frames_Trace : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
        IEnumerable<TraceItem> errors = TraceMaint.GetTrace().Reverse();

		DrawTable(placeholder, errors);

		lblVersion.Text = PX.Data.PXVersionInfo.Version;
		String cust = Customization.CstWebsiteStorage.PublishedProjectList;
		lblCustomization.Text = String.IsNullOrEmpty(cust) ? PXLocalizer.Localize(Messages.None, typeof(Messages).FullName) : cust;
	}
	private void DrawTable(HtmlGenericControl place, IEnumerable<TraceItem> errros)
	{
		foreach (TraceItem item in errros)
		{
			Control control = Page.LoadControl("~/Controls/TraceItem.ascx");
			control.GetType().InvokeMember("Initialise",	System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
				null, control, new Object[] { item });

			place.Controls.Add(control);
		}
	}
}
