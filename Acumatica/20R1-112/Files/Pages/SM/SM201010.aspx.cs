using System;
using PX.Common;
using PX.Data;
using PX.Web.UI;

public partial class Page_SM201010 : PX.Web.UI.PXPage
{
	protected override void OnInit(EventArgs e)
	{
		if (tab.Items["saleforceSyncStatus"] != null && !ds.DataGraph.Views.ContainsKey("SyncRecs"))
		{
			tab.Items.Remove(tab.Items["saleforceSyncStatus"]);
		}
		if (tab.Items["tabLocationTracking"] != null && !ds.DataGraph.Views.ContainsKey("LocationTracking"))
		{
			tab.Items.Remove(tab.Items["tabLocationTracking"]);
		}

		var mlRule = (PXLayoutRule)form.FindControl("lrMultiFactor");
		var mlOverride = (PXCheckBox)form.FindControl("edMultiFactorOverride");
		var mlType = (PXDropDown)form.FindControl("edMultiFactorType");

		if (mlRule != null && mlRule.Visible && mlType.Hidden && mlOverride.Hidden)
			mlRule.Parent.Controls.Remove(mlRule);

		base.OnInit(e);
	}

	protected void tab_DataBound(object sender, EventArgs e)
	{
		PXCache cache = ds.DataGraph.Caches[typeof(PX.SM.Users)];
		PX.SM.Users user = cache.Current as PX.SM.Users;
		if (user != null)
		{
			bool isFramework = !ds.DataGraph.Views["AllowedRoles"].Cache.AllowSelect;
			bool isFromActiveDirectory = user.Source == PX.SM.PXUsersSourceListAttribute.ActiveDirectory;
			var statisticsOriginalVisible = GetTabOriginalVisible("statistics");
			var rolesOriginalVisible = GetTabOriginalVisible("membership");
			var roles2OriginalVisible = GetTabOriginalVisible("roles");
			tab.Items["statistics"].Visible = !isFromActiveDirectory && statisticsOriginalVisible;
			tab.Items["membership"].Visible = !isFramework && rolesOriginalVisible;
			tab.Items["roles"].Visible = (!isFromActiveDirectory || user.OverrideADRoles == true) && isFramework && roles2OriginalVisible;
		}
	}

	private bool GetTabOriginalVisible(string tabName)
	{
		var context = System.Web.HttpContext.Current;
		var key = tabName + "_visible";
		bool? result = PXContext.Session.TabVisible[key];
		if (result == null)
		{
			result = tab.Items[tabName].Visible;
			PXContext.Session.TabVisible[key] = result;
		}
		return (bool)result;
	}
}
