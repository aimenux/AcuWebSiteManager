using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Web.UI;
using PX.Web.UI.Controls;
using PX.Data.Process;

public partial class Frames_Trace : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		String key = this.Request.QueryString["key"];
		if(!String.IsNullOrEmpty(key))
		{
			AuditInfo info = PX.Common.PXContext.SessionTyped<PXSessionStatePXData>().AuditInfo[key] as AuditInfo;
			if (info != null)
			{
				DrawCaption(caption, info);
				DrawPanel(panelHolder, info);
				DrawTable(placeholder, info);

				auditTitle.InnerText = String.Concat(PX.Data.ActionsMessages.AuditHistory, ": ", info.Table);
			}

			this.Session.Remove(key);
		}

		lblVersion.Text = PX.Data.PXVersionInfo.Version;
		String cust = Customization.CstWebsiteStorage.PublishedProjectList;
		lblCustomization.Text = String.IsNullOrEmpty(cust) ? "None" : cust;
	}
	private void DrawCaption(HtmlGenericControl place, AuditInfo info)
	{
		PXPanel panel = new PXPanel();
		panel.ContentLayout.Layout = LayoutType.Stack;
		panel.ContentLayout.Orientation = PX.Web.UI.Orientation.Horizontal;
		panel.RenderStyle = FieldsetStyle.Simple;
		panel.ContentLayout.InnerSpacing = true;
		panel.ContentLayout.SpacingSize = SpacingSize.Medium;
		panel.ContentLayout.OuterSpacing = SpacingDirection.Around;

		IParserAccessor pa = (IParserAccessor)panel;
		foreach (AuditValue value in info.Keys)
		{
			DrawItem(pa, value.DisplayName + ":", value.NewValue.ToString());
		}
		place.Controls.Add(panel);
	}
	private void DrawPanel(HtmlGenericControl place, AuditInfo info)
	{
		if (info.Panel == null) return;

		place.Visible = true;
		edCreatedByID.Text = info.Panel.CreatedByID;
		edCreatedByScreenID.Text = info.Panel.CreatedByScreenID;
		edCreatedDateTime.Text = info.Panel.CreatedDateTime.ToString();
		edLastModifiedByID.Text = info.Panel.LastModifiedByID;		
		edLastModifiedByScreenID.Text = info.Panel.LastModifiedByScreenID;
		edLastModifiedDateTime.Text = info.Panel.LastModifiedDateTime.ToString();
	}
	private void DrawTable(HtmlGenericControl place, AuditInfo info)
	{
		foreach (AuditBatch batch in info)
		{
			Control control = Page.LoadControl("~/Controls/AuditItem.ascx");
			control.GetType().InvokeMember("Initialise", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
				null, control, new Object[] { batch });

			place.Controls.Add(control);
		}
	}

	private void DrawItem(IParserAccessor pa, String caption, String value)
	{
		PXLabel lblTitle = new PXLabel(caption);
		lblTitle.Font.Bold = true;
		lblTitle.Font.Size = FontUnit.Small;
		pa.AddParsedSubObject(lblTitle);

		if (!String.IsNullOrWhiteSpace(value))
		{
			PXLabel lblValue = new PXLabel(value.ToString());
			lblValue.Font.Size = FontUnit.Small;
			pa.AddParsedSubObject(lblValue);
		}
	}
}
