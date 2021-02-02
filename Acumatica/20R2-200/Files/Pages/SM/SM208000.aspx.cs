using System;
using System.Linq;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Web.UI;
using PX.Web.Controls;
using PX.Data.Maintenance.GI;

public partial class Page_SM208000 : PX.Web.UI.PXPage
{
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		if (!this.IsCallback)
		{
			var renderer = JSManager.GetRenderer(this);
			JSManager.RegisterModule(renderer, JS.BaseKey, JS.NetType);
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupHeight = 560;
		this.Master.PopupWidth = 870;

		((PXLabel)this.form.FindControl("lblRecords")).Text = PX.SM.Messages.Records;
		((PXLabel)this.form.FindControl("lblColumns")).Text = PX.SM.Messages.Columns;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsCallback)
		{
			PXGrid grid = this.tab.FindControl("grdFilter") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridFilterID", "var grdFilterID=\"" + grid.ClientID + "\";", true);
			grid = this.tab.FindControl("grdResults") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridResultsID", "var grdResultsID=\"" + grid.ClientID + "\";", true);
			grid = this.tab.FindControl("grdSorts") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "gridSortsID", "var grdSortsID=\"" + grid.ClientID + "\";", true);
			grid = this.tab.FindControl("grdWheres") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "grdWheres", "var grdWheresID=\"" + grid.ClientID + "\";", true);
			grid = this.tab.FindControl("grdGroupBy") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "grdGroupBy", "var grdGroupByID=\"" + grid.ClientID + "\";", true);
			PXSplitContainer sp1 = this.tab.FindControl("sp1") as PXSplitContainer;
			grid = sp1.FindControl("grdJoins") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "grdJoins", "var grdJoinsID=\"" + grid.ClientID + "\";", true);
			grid = sp1.FindControl("grdOns") as PX.Web.UI.PXGrid;
			if (grid != null)
				this.Page.ClientScript.RegisterClientScriptBlock(GetType(), "grdOns", "var grdOnsID=\"" + grid.ClientID + "\";", true);

			this.ClientScript.RegisterArrayDeclaration("relativeDatesVariables", String.Join(",", RelativeDatesManager.AllVariables.Select(v => String.Concat("\"", v, "\""))));
		}
	}

	protected void edValue_RootFieldsNeeded(object sender, PXCallBackEventArgs e)
	{
		GenericInquiryDesigner graph = this.ds.DataGraph as GenericInquiryDesigner;
		if (graph != null)
		{
			String[] fields = graph.GetAllFields();
			String[] parameters = graph.GetAllParameters();
			
			e.Result = string.Join(";", parameters.Concat(fields));
		}
	}
	protected void edOns_RootFieldsNeeded(object sender, PXCallBackEventArgs e)
	{
		GenericInquiryDesigner graph = this.ds.DataGraph as GenericInquiryDesigner;
		if (graph != null)
		{
			String[] fields = graph.GetFieldsInRelation();
			String[] parameters = graph.GetAllParameters();

			e.Result = string.Join(";", parameters.Concat(fields));
		}
	}

	protected void edNavParam_RootFieldsNeeded(object sender, PXCallBackEventArgs e)
	{
		GenericInquiryDesigner graph = this.ds.DataGraph as GenericInquiryDesigner;
		if (graph != null)
		{
			String[] fields = graph.GetAllFields();
			var parameters = graph.GetAllParameters(false).Select(f => "@" + f);

			e.Result = string.Join(";", parameters.Concat(fields));
		}
	}

	protected void form_DataBound(object sender, EventArgs e)
	{
		GenericInquiryDesigner graph = this.ds.DataGraph as GenericInquiryDesigner;
		if (graph.IsSiteMapAltered) 
			this.ds.CallbackResultArg = "RefreshSitemap";
	}
	protected void grd_EditorsCreated_RelativeDates(object sender, EventArgs e)
	{
		PXGrid grid = sender as PXGrid;
		if (grid != null)
		{
			PXDateTimeEdit de = grid.PrimaryLevel.GetStandardEditor(GridStandardEditor.Date) as PXDateTimeEdit;
			if (de != null)
			{
				de.ShowRelativeDates = true;
			}
		}
	}
}
