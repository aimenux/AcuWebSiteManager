using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Customization;
using PX.Web.UI;


//namespace PX.Web.Customization
//{
public partial class Customization_CustomizationDialogs : System.Web.UI.UserControl//, IDlgCallback
{

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        var page = this.Page as PXPage;
        if (page !=null && page.DefaultDataSource != null && page.DefaultDataSource.EnableAttributes)
        {
            var menuItem = new PXMenuItem("Manage User-Defined Fields");
            menuItem.AutoCallBack.Command = "ManageAttributes";
            menuItem.AutoCallBack.Target = "DsControlProps";
            (PXToolBar1.Items[0] as PXToolBarButton).MenuItems.Insert(2, menuItem);
        }
    }

    protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		//Page p = Page;
		//CustomizationContextMenu.Visible =
		//	SpanContent.Visible =
			//PanelContent.Visible =
			//WebsiteEntryPoints.IsCstVisible(p);
		//ModelPage.IsArtifical(p) || CstSession.Current.IsDesignMode(p.Request);

		//HandleEvents();
		//InitVSUrl();
	}

	protected void PXSmartPanel1_LoadContent(object sender, EventArgs e)
	{
		PXSmartPanel1.Caption = PX.Web.Controls.Messages.SelectForAction;
		PXSmartPanel1.DefaultMsgText = PX.Web.Controls.Messages.SelectForAction;
	}

	//public static void DisableCustomizeChildControls(AttributeCollection attr)
	//{
	//	attr["DesignerDisableCustomizeChildControls"] = "true";
	//}

	//private void HandleEvents()
	//{
	//	//preserve designers from customization
	//	foreach (object control in Controls)
	//	{
	//		WebControl w = control as WebControl;
	//		if (w == null) continue;
	//		DisableCustomizeChildControls(w.Attributes);

	//	}

	//	StringBuilder script = new StringBuilder();
	//	AppendVariable(script, ResizeHandler);
	//	AppendVariable(script, WizardControlProps);


	//	AppendVariable(script, CustomizationContextMenu);



	//	AppendVariable(script, FormDataField.FindControl("edDacAttrEdit"));



	//	Page.ClientScript.RegisterClientScriptBlock(GetType(), "PanelClientId", script.ToString(), true);
	//	//AttachColumnEditorContextMenu();

	//	if (!Page.IsCallback)
	//	{
	//		MenuItems();





	//	}
	//}

	//private void MenuItems()
	//{
	//	PXToolBarButton btn = (PXToolBarButton) PXToolBar1.Items[0];
	//	bool hasWorkingProject = WebsiteEntryPoints.HasWorkingProject;
	//	Page p = Page;
	//	bool isDesignMode = hasWorkingProject && WebsiteEntryPoints.IsCstVisible(p);
	//	//(ModelPage.IsArtifical(p) || CstSession.Current.IsDesignMode(p.Request));
	//	//	btn.MenuItems[0].Enabled = hasWorkingProject;

	//	//btn.MenuItems[1].Enabled = !isDesignMode;
	//	//btn.MenuItems[3].Enabled = isDesignMode;

	//	foreach (PXMenuItem itm in btn.MenuItems)
	//	{
	//		if (itm.CommandArgument == "SaveToDatabase")
	//		{
	//			itm.Enabled = hasWorkingProject;
	//			if (hasWorkingProject)
	//				itm.Text += String.Format("({0})", WebsiteEntryPoints.GetProjectName());

	//		}

	//		if (itm.CommandArgument == "ReloadFromDatabase")
	//		{
	//			itm.Enabled = hasWorkingProject;

	//		}

	//		//if (itm.CommandArgument == "CodeEditor")
	//		//{
	//		//    //var contextMenuItm = CustomizationContextMenu.Items
	//		//    //    .Cast<PXMenuItem>()
	//		//    //    .First(_ => _.NavigateUrl == "~/Controls/Edit.aspx");

	//		//    //contextMenuItm.Enabled = hasWorkingProject;
	//		//    itm.Enabled = hasWorkingProject;

	//		//    //if(hasWorkingProject
	//		//    //    && CstDesignModeAttribute.CanEnterDesignMode(this.Page))
	//		//    //{
	//		//    //    var ds = ControlContext.CurrentPage.DataSource;
	//		//    //    if(ds != null)
	//		//    //    {
	//		//    //        var t = PXBuildManager.GetType(ds.AspxElement.GetAttribute("TypeName"), false);
	//		//    //        if(t!= null)
	//		//    //        {

	//		//    //            t = PXGraphEx.GetTypeNotCustomized(t);
	//		//    //            itm.NavigateUrl += String.Format("?project={0}", CstSession.Current.WorkingProjectId);
	//		//    //            if(isDesignMode)
	//		//    //                itm.NavigateUrl += String.Format("&default={0}", t.FullName);
	//		//    //            contextMenuItm.NavigateUrl = itm.NavigateUrl;

	//		//    //        }


	//		//    //    }


	//		//    //}


	//		//}

	//		if (itm.CommandArgument == "PublishedDoc")
	//		{
	//			itm.Enabled = CstWebsiteStorage.IsCustomizationPublished;

	//		}
	//		if (itm.CommandArgument == "CloseWorkingProject")
	//		{
	//			itm.Enabled = hasWorkingProject;

	//		}
	//		if (itm.CommandArgument == "Validate")
	//		{
	//			itm.Enabled = hasWorkingProject;

	//		}

	//		//if (itm.CommandArgument == "AspxEditor")
	//		//{
	//		//    itm.Enabled = hasWorkingProject;

	//		//}


	//		if (itm.CommandArgument == "UndoPublish")
	//		{
	//			bool isPublished = CstWebsiteStorage.IsCustomizationPublished;
	//			itm.Enabled = isPublished;

	//			if (isPublished)
	//			{
	//				string n = CstWebsiteStorage.PublishedProjectList;
	//				itm.Text += "(" + n + ")";



	//			}

	//		}
	//		//if (itm.CommandArgument == "SelectWorkingProject")
	//		//{
	//		//    itm.Enabled = hasWorkingProject;


	//		//}

	//		if (itm.CommandArgument == "OpenManager")
	//		{

	//			if (hasWorkingProject)
	//			{
	//				itm.NavigateUrl += "?projectId=" + WebsiteEntryPoints.WorkingProjectId();
	//				//itm.NavigateParams.Add("projectId", CustomizationSession.Current.WorkingProjectId);



	//			}

	//		}

	//		if (itm.CommandArgument == "EnterDesignMode")
	//		{
	//			itm.Enabled = hasWorkingProject
	//						  && !isDesignMode
	//						  && CstDesignModeAttribute.CanEnterDesignMode(this.Page);

	//			//if (hasWorkingProject && !isDesignMode)
	//			//    itm.Enabled = ControlContext.CurrentPage.IsCustomizationAllowed();
	//			//CstManager.SubscribeCustomizationAllowed(
	//			//    delegate { itm.Enabled = true; });
	//		}
	//		if (itm.CommandArgument == "ExitDesignMode")
	//		{
	//			itm.Enabled = isDesignMode;
	//		}
	//		if (itm.CommandArgument == "CompilePage")
	//		{
	//			itm.Enabled = isDesignMode;

	//		}
	//		if (itm.CommandArgument == "SessionDoc")
	//		{
	//			itm.Enabled = hasWorkingProject;

	//		}
	//		if (itm.CommandArgument == "SaveRevision")
	//		{
	//			itm.Enabled = hasWorkingProject;

	//		}
	//		if (itm.CommandArgument == "VisualStudio")
	//		{
	//			itm.Enabled = hasWorkingProject;

	//		}
	//		//if (itm.Text == "Delete All...")
	//		//{
	//		//    itm.Enabled = hasWorkingProject;

	//		//}
	//		//if (itm.Text == "Reset this page...")
	//		//{
	//		//    itm.Enabled = hasWorkingProject;
	//		//    string url = "~/Controls/CustomizationExport.aspx?action=resetPage&key=" + CustomizedPage.Builder.GetKey(this.Page);
	//		//    itm.NavigateUrl = url;
	//		//}
	//	}
	//}


	//private static void AppendVariable(StringBuilder script, Control c)
	//{
	//	StringBuilder n = new StringBuilder(c.ID + "Id");
	//	n[0] = char.ToLower(n[0]);
	//	script.AppendFormat("var {0}='{1}';", n, c.ClientID);
	//}

	//protected void ResizeHanler_CallBack(object sender, PXCallBackEventArgs e)
	//{
	//	//CustomizationManager.
	//	string[] pair = e.Argument.Split('>');
	//	string cmd = pair[0];
	//	string props = pair[1];
	//	WebsiteEntryPoints.ParseCallback(cmd, props);
	//}



	//protected void WCPGrid_RowDataBound(object sender, PXGridRowEventArgs e)
	//{
	//	//CustomizationManager.PropertyGridRowDataBound(sender, e);
	//	RowControlProperty prop = (RowControlProperty) e.Row.DataItem;
	//	prop.OnDataBound(e.Row);
	//}

	//protected void PXToolBar1_CallBack(object sender, PXCallBackEventArgs e)
	//{
	//	WebsiteEntryPoints.ProcessMenuCommand(e.Argument, Page);

	//	//throw new Exception("Redirect:" + ResolveUrl/**/(this.Page.AppRelativeVirtualPath));
	//}




	//protected void CustomizationContextMenu_CallBack(object sender, PXCallBackEventArgs e)
	//{
	//	string item = e.Argument;
	//	if (item.StartsWith("Add"))
	//	{
	//		string controlType = item.Remove(0, "Add".Length);
	//		WebsiteEntryPoints.AddContainer(controlType);
	//	}
	//}

	//void InitVSUrl()
	//{
	//	var g =(CustomizationGraph) DsControlProps.DataGraph;

	//	var btn =(PXButton) ControlHelper.FindControl(this.PanelElemInfo, "ButtonScreenActions");
	//	if(btn == null)
	//		return;
	//	btn.MenuItems[2].NavigateUrl = g.ViewElemInfo.Current.VSLayout;
	//	btn.MenuItems[2].Target = "_blank";

	//	btn =(PXButton) ControlHelper.FindControl(this.FormElemInfo, "ButtonDacActions");
	//	btn.MenuItems[2].NavigateUrl = g.ViewElemInfo.Current.VSLayout;
	//	btn.MenuItems[2].Target = "_blank";

	//	btn =(PXButton) ControlHelper.FindControl(this.FormElemInfo, "ButtonGraphActions");
	//	btn.MenuItems[2].NavigateUrl = g.ViewElemInfo.Current.VSLayout;
	//	btn.MenuItems[2].Target = "_blank";


	//}


	//public void TreePageControlsSelectValue(string v)
	//{
	//	PXTreeNode node = TreePageControls.EnumNodes().FirstOrDefault(n => n.Value == v);
	//	this.TreePageControls.SelectedNode = node;
	//	//		TreePageControls.SelectedNodeID = TreePageControls.SelectedNodeID;
	//}

	//public string GetScriptName(string rname)
	//{
	//	string resource = "PX.Web.Customization.Controls.cseditor." + rname;
	//	var url = Page.ClientScript.GetWebResourceUrl(typeof (WebsiteEntryPoints), resource);
	//	url = url.Replace(".axd?", ".axd?file=" + rname + "&");
	//	return System.Web.HttpUtility.HtmlAttributeEncode(url);
	//	//			return VirtualPathUtility.GetFileName(url);

	//}
	//protected void ButtonScreenBinding(object sender, EventArgs e)
	//{
	//	var g = (CustomizationGraph)DsControlProps.DataGraph;
	//	var btn = (PXButton) sender;
	//	btn.MenuItems[2].NavigateUrl = g.ViewElemInfo.Current.VSLayout;
	//	btn.MenuItems[2].Target = "_blank";
	//}
}

//}
