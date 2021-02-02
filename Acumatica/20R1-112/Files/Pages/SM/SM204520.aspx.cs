using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Customization;
using PX.Web.Customization;
using PX.Web.UI;

[Customization.CstDesignMode(Disabled = true)]
public partial class Pages_SM_SM204520 :PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		((IPXMasterPage)this.Master).CustomizationAvailable = false;
		this.Master.FindControl("usrCaption").Visible = false;
		ProjectBrowserMaint.InitSessionFromQueryString(HttpContext.Current);

		//string screenID = this.Request.QueryString["EditScreenID"];
		//if (!string.IsNullOrEmpty(screenID)) ProjectBrowserMaint.ContextScreenID = screenID;
	}

    protected override void OnInitComplete(EventArgs e)
    {
        base.OnInitComplete(e);


        // Dirty hack to search screen not only by screen id, but and title.
        // There is dynamic selector in grid, so we can't set aspx's properties for it
        var grid = ControlHelper.FindControl("StatePropGrid", this.Master.Controls);
        if (grid != null)
        {
            var selector = ControlHelper.FindControl("es", grid.Controls) as PXSelector;
            if (selector != null)
            {
                selector.HintField = "Title";
            }
        }
    }

    protected override void OnLoad(EventArgs e)
	{
		this.SuppressPageValidation = true;
		
		base.OnLoad(e);

		string[] buttons = new string[] { "btnAddForm", "btnAddRow", "btnAddColumn", "btnAddGroup", 
			"btnAddMerge", "btnAddTab", "btnAddTabItem", "btnAddGrid", "btnAddPopup", "btnAddPanel", 
			"btnAddLabel", "btnAddButton", "btnAddGroupBox", "btnAddRule", "btnAddRadioButton", "btnAddScript" };
		var tab = (PXTab)this.SplitContainer.FindControl("tab");
		foreach (string id in buttons)
		{
			var btn = tab.FindControl(id) as PXButton;
			if (btn != null)
			{
                btn.Attributes["draggable"] = "true";
                btn.Style[HtmlTextWriterStyle.MarginLeft] = Unit.Pixel(30).ToString();
				btn.CallBack += this.OnCreateControl;
			}
		}

		//if (ControlHelper.IsReloadPage(this))
		{
			this.LoadComplete += delegate(object sender, EventArgs ea)
			{
				var tree = SplitContainer.FindControl("TreePageControls") as PXTreeView;
				if (tree.ToolBar != null) tree.ToolBar.CallbackUpdatable = true;
			};
		}
	}

	protected void PropGridFilter_DataBound(object sender, EventArgs e)
	{
		//if (!ControlHelper.IsReloadPage(this)) return;

		//var tree = SplitContainer.FindControl("TreePageControls") as PXTreeView;
		//if (tree != null)
		//{
		//	var btn = (PXToolBarButton)tree.ToolBar.Items["Filter"];
		//	var chk = PropGridFilter.DataControls["ShowAllControls"] as PXCheckBox;
		//	btn.Pushed = !chk.Checked && !string.IsNullOrEmpty(ProjectBrowserMaint.ContextControlID);
		//}
	}

	/// <summary>
	/// The page PreRenderComplete event handler.
	/// </summary>
	protected override void OnPreRenderComplete(EventArgs e)
	{
		string query = ProjectBrowserMaint.ContextScreenID;
		if (!string.IsNullOrEmpty(query))
		{
			this.ClientScript.RegisterStartupScript(this.GetType(), "query",
				string.Format("\nvar __queryString = '{0}={1}'; ", "EditScreenID", query), true);
		}
		base.OnPreRenderComplete(e);
	}

	/// <summary>
	/// The create new control event handler.
	/// </summary>
	protected void OnCreateControl(object sender, PXCallBackEventArgs arg)
	{
		string[] ar = arg.Argument.Split('|');
		string parent = ar[0], before = (ar.Length > 1) ? ar[1] : null, after = (ar.Length > 2) ? ar[2] : null;
		string controlType = ((PXButton)sender).Key;

		switch (controlType)
		{
			case "Row":
				LayoutEditor.CreateControl("<PXLayoutRule StartRow='True' />", parent, before, after);
				break;
			case "Column":
				LayoutEditor.CreateControl("<PXLayoutRule StartColumn='True' />", parent, before, after);
				break;
			case "Group":
				LayoutEditor.CreateControl("<PXLayoutRule StartGroup='True' />", parent, before, after);
				break;
			case "Merge":
				LayoutEditor.CreateControl("<PXLayoutRule Merge='True' />", parent, before, after);
				break;
			case "Grid":
				LayoutEditor.CreateControl("<PXGrid><Levels><PXGridLevel></PXGridLevel></Levels></PXGrid>", parent, before, after);
				break;
			case "Tab":
				LayoutEditor.CreateControl("<PXTab><Items><PXTabItem Text='TabItem1'></PXTabItem></Items></PXTab>", parent, before, after);
				break;

			case "Button":
				if (!string.IsNullOrEmpty(before))
				{
					var type = LayoutEditor.GetPageNodeTypeByUID(before);
					if (typeof(PXToolBarItem).IsAssignableFrom(type)) controlType = "PXToolBarButton";
					if (typeof(PXDSCallbackCommand).IsAssignableFrom(type)) controlType = "PXDSCallbackCommand";
				}
				else if (!string.IsNullOrEmpty(parent))
				{
					var type = parent.Contains(_customItemsName) ? typeof(PXToolBar) : LayoutEditor.GetPageNodeTypeByUID(parent);
					if (typeof(PXDataSource).IsAssignableFrom(type)) controlType = "PXDSCallbackCommand";
					if (typeof(PXToolBar).IsAssignableFrom(type) || typeof(PXGrid).IsAssignableFrom(type)) 
						controlType = "PXToolBarButton";
				}

                if (controlType == "PXDSCallbackCommand")
                {
                    break;
                }

				if(controlType != "Button")
					LayoutEditor.CreateControl(string.Format("<{0}></{0}>", controlType) , parent, before, after);
				else 
					LayoutEditor.CreateControl(controlType, parent, before, after);
				break;

			default:
				LayoutEditor.CreateControl(controlType, parent, before, after);
				break;
		}
	}

	LayoutEditorMaint LayoutEditor { get { return (LayoutEditorMaint) this.DefaultDataSource.DataGraph; }}

	/// <summary>
	/// Called after tree node has been moved.
	/// </summary>
	protected void TreePageControls_NodeMove(object sender, PXTreeMoveEventArgs e)
	{
		string[] node = e.NodeKey, parent = e.ParentKey, before = e.BeforeKey;
		var result = LayoutEditor.MoveControl(node[0], parent[0], before==null?null : before[0]);
        if (!result)
        {
            e.Cancel = true;
        }
	}

	public static void DisableCustomizeChildControls(AttributeCollection attr)
	{
		attr["DesignerDisableCustomizeChildControls"] = "true";
	}

	public void TreePageControlsSelectValue(string v)
	{
		var tree = (PXTreeView)this.SplitContainer.FindControl("TreePageControls");
		PXTreeNode node = tree.EnumNodes().FirstOrDefault(n => n.Value == v);
		tree.SelectedNode = node;
	}

	public string GetScriptName(string rname)
	{
		string resource = "PX.Web.Customization.Controls.cseditor." + rname;
		var url = Page.ClientScript.GetWebResourceUrl(typeof(WebsiteEntryPoints), resource);
		url = url.Replace(".axd?", ".axd?file=" + rname + "&");
		return System.Web.HttpUtility.HtmlAttributeEncode(url);
	}

	//private static void AppendVariable(StringBuilder script, Control c)
	//{
	//	StringBuilder n = new StringBuilder(c.ID + "Id");
	//	n[0] = char.ToLower(n[0]);
	//	script.AppendFormat("var {0}='{1}';", n, c.ClientID);
	//}

	protected void ResizeHanler_CallBack(object sender, PXCallBackEventArgs e)
	{
		//string[] pair = e.Argument.Split('>');
		//string cmd = pair[0];
		//string props = pair[1];
		//WebsiteEntryPoints.ParseCallback(cmd, props);
	}

	protected void PropGrid_RowDataBound(object sender, PXGridRowEventArgs e)
	{
		//CustomizationManager.PropertyGridRowDataBound(sender, e);
		RowControlProperty prop = (RowControlProperty)e.Row.DataItem;
		prop.OnDataBound(e.Row);

		if (prop.IsGroupHeader )
			e.Row.Style.CssClass += prop.Offset < 0 ? "PropHeader" : "PropHeader" + prop.Offset;

		if (prop.IsHidden) e.Row.Visible = false;
	
		if (prop.Offset > 0) e.Row.Cells["Name"].Style.CssClass = "PropOffset" + prop.Offset;
	}

	protected void TreePageControls_NodeDataBound(object sender, PXTreeNodeEventArgs e)
	{
		RowControlTree item = e.Node.DataItem as RowControlTree;
		Type type = item.RuntimeType;
		if (type == null && item.Name == _customItemsName) type = typeof(PXToolBar);

		if (e.Node.Level == 0) e.Node.SuppressDragDrop = true;
		if (type == null) e.Node.SuppressDragDrop = e.Node.SuppressDropInside = true;
		
		e.Node.SuppressDropInside = 
			!typeof(PXLayoutRule).IsAssignableFrom(type) && !typeof(PXTabItem).IsAssignableFrom(type) &&
			!typeof(PXPanel).IsAssignableFrom(type) && !typeof(PXSmartPanel).IsAssignableFrom(type) &&
			!typeof(PXGroupBox).IsAssignableFrom(type) && !typeof(PXBoundPanel).IsAssignableFrom(type) &&
			!typeof(PXGrid).IsAssignableFrom(type) && !typeof(PXGridLevel).IsAssignableFrom(type) &&
			!typeof(PXToolBar).IsAssignableFrom(type) && !typeof(PXDataSource).IsAssignableFrom(type)
			&& item.Name != "Dialogs";


		if (type != null)
		{
			string typeName = type.Name;
			if (typeof(PXLayoutRule).IsAssignableFrom(type) && !item.IsContainerRule)
			{
				typeName += "0"; e.Node.SuppressDropInside = true;
			}
			e.Node.Attributes.Add("data-type", typeName);
		}
		else if (item.IsContainerRule)
		{
			e.Node.SuppressDropInside = false;
			e.Node.Attributes.Add("data-type", "PXLayoutRule");
			e.Node.Attributes.Add("data-fakerule", "1");
		}



		if (item.Name == "Dialogs")
		{
			e.Node.Attributes["data-type"] =  "Dialogs";
			e.Node.SuppressDropInside = false;

		}

		if (ControlHelper.IsReloadPage(this))
		{
			if (e.Node.Value == ProjectBrowserMaint.ContextControlID)
				((PXTreeView)sender).SelectedNode = e.Node;
		}
	}

	protected void PXToolBar1_CallBack(object sender, PXCallBackEventArgs e)
	{
	//	WebsiteEntryPoints.ProcessMenuCommand(e.Argument, Page);
		//throw new Exception("Redirect:" + ResolveUrl/**/(this.Page.AppRelativeVirtualPath));
	}
	
	protected void CustomizationContextMenu_CallBack(object sender, PXCallBackEventArgs e)
	{
		//string item = e.Argument;
		//if (item.StartsWith("Add"))
		//{
		//	string controlType = item.Remove(0, "Add".Length);
		//	WebsiteEntryPoints.AddContainer(controlType);
		//}
	}

	protected void TreePageControls_DataBound(object sender, EventArgs e)
	{

	}

	private const string _customItemsName = "ActionBar-CustomItems";
}