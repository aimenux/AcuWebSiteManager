using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Web.Controls;
using PX.SM;
using PX.Web.UI;
using PX.Olap.Maintenance;

public partial class Page_Pivot : PX.Web.UI.PXPage
{
	private PXPivotDataSource DataSource
	{
		get { return (PXPivotDataSource)this.DefaultDataSource; }
	}

	protected override void OnPreInit(EventArgs e)
	{
		Master.ScreenID = null;
		Master.ScreenTitle = null;
		Master.DataViewBarVisible = false;
		PXPageCache.RegisterRequiredReloadPage(this, true);
		base.OnPreInit(e);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (this.IsCallback)
		{
			var cm = PXCallbackManager.GetInstance();
			cm.PostGetCallbackResult += this.handle_getCallbackResult;
		}
		else
		{
			var tlbTools = this.Master.FindControl("usrCaption").FindControl("tlbTools") as PXToolBar;
			if (tlbTools != null)
			{
				var saveAs = tlbTools.Items["btnSavePivotAs"] as PXToolBarButton;
				if (saveAs != null) // can be null if user doesn't have access to Pivot Maint screen
					saveAs.PopupCommand.Handler = "handle_SaveAs";
			}
		}
	}

	void handle_getCallbackResult(PXCallbackManager sender, XmlWriter writer)
	{
		if (sender.ActiveCommand.Name == "pivotSaveAs")
		{
			string screenID = this.Request.QueryString[typeof(PivotTable.screenID).Name];
			var list = new List<string>();
			foreach (PivotTable table in PXPivotTableGraph.PivotTables.Where(t => String.Equals(t.ScreenID, screenID)))
				list.Add(table.Name + "|" + table.PivotTableID.Value.ToString());

			writer.WriteStartElement("PivotTables");
			writer.WriteAttributeString("Tables", string.Join(";", list.ToArray()));
			writer.WriteEndElement();
		}
	}

	protected override void OnInitComplete(EventArgs e)
	{
		base.OnInitComplete(e);
		Master.AddTitleModule(new PX.Web.Controls.TitleModules.PivotTableTitleModule(pnlSavePivotAs, txtPivotName));
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
			this.Page.ClientScript.RegisterHiddenField(_pivotIdField, string.Empty);
		else
		{
			string pivotIDstr = this.Request.Form[_pivotIdField];
			int pivotID;
			if (!string.IsNullOrEmpty(pivotIDstr) && int.TryParse(pivotIDstr, out pivotID)) 
			{
				DataSource.PivotTableID = pivotID;
			}
		}

		Title = DataSource.PivotTitle;
		tlbPivot.Style[HtmlTextWriterStyle.Display] = "none";

		// show tabs only if there is special parameter in the URL
		if (!String.Equals(this.Request.QueryString[PXPivotTableGraph.ShowTabsParam], true.ToString(), StringComparison.OrdinalIgnoreCase))
			return;

		string screenID = this.Request.QueryString[typeof(PivotTable.screenID).Name];
		string pivotTableID = this.Request.QueryString[typeof(PivotTable.pivotTableID).Name];
		bool isFirst = true;
		if (!string.IsNullOrEmpty(screenID) && !this.Page.IsCallback)
		{
			foreach (PivotTable table in PXPivotTableGraph.PivotTables
				.Where(t => String.Equals(t.ScreenID, screenID, StringComparison.OrdinalIgnoreCase) 
				&& !String.IsNullOrEmpty(t.Name)))
			{
				var btn = new PXToolBarButton
				{
					Text = table.Name,
					Key = table.PivotTableID.Value.ToString(),
					ToggleMode = true,
					ToggleGroup = "1"
				};

				btn.Pushed = (pivotTableID == btn.Key);
				if (isFirst)
				{
					btn.Attributes["first-tab"] = "1";
					isFirst = false;
				}
				tlbPivot.Items.Add(btn);
			}
			if (tlbPivot.Items.Count > 1)
			{
				var lbl = new PXToolBarLabel() { Width = Unit.Percentage(100) };
				tlbPivot.Items.Add(lbl);
				tlbPivot.Style[HtmlTextWriterStyle.Display] = "";
			}
		}
	}

	private const string _pivotIdField = "__pivotTableID";
}
