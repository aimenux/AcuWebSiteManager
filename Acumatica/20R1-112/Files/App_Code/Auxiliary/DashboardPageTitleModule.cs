using System;
using PX.Web.UI;
using PX.Web.UI.TitleModules;

public class DashboardPageTitleModule : TitleModule
{
	public override void Initialize(ITitleModuleController controller)
	{
		if (controller == null) throw new ArgumentNullException("controller");

		if (CanDesign(controller))
		{
			AppendButton(controller);
			AppendPanels(controller);

			var tlb = ControlHelper.FindControl("tlbTools", controller.Page) as PXToolBar;
			if (tlb != null) tlb.CallbackUpdatable = true;

			controller.Page.LoadComplete += delegate (object sender, EventArgs e)
			{
				var dashSet = ControlHelper.FindControl(_dashContID, controller.Page) as PXDashboardContainer;
				if (dashSet != null) dashSet.CallBackMode.RepaintControlsIDs = "tlbTools";

				var btnW = tlb.Items["addWidget"] as PXToolBarButton;
				var btnD = tlb.Items["design"] as PXToolBarButton;
				var btnL = tlb.Items["editLayout"] as PXToolBarButton;
				var btnR = tlb.Items["resetToDefault"] as PXToolBarButton;
				var btnC = tlb.Items["createUserCopy"] as PXToolBarButton;
				var btnU = tlb.Items["updateAll"] as PXToolBarButton;
				if (dashSet != null)
				{
					//if (btnW != null) btnW.Visible = dashSet.InDesignMode;
					if (btnL != null) btnL.Visible = dashSet.InDesignMode;
					if (btnD != null) btnD.Pushed = dashSet.InDesignMode;
					if (btnR != null) btnR.Visible = !dashSet.InDesignMode;
					if (btnU != null) btnU.Visible = !dashSet.InDesignMode;
				}
			};
		}
	}

	public override bool GetDefaultVisibility()
	{
		return true;
	}

	#region Methods

	private bool CanDesign(ITitleModuleController controller)
	{
		return IsDashboardPage(controller);
	}

	private bool IsDashboardPage(ITitleModuleController controller)
	{
		var path = controller.Page.Request.Path.ToLower();
		return path.EndsWith("default.aspx") || path.EndsWith("dashboardlauncher.aspx");
	}

	private void AppendButton(ITitleModuleController controller)
	{
		var btn = new PXToolBarButton	{
			Key = "design",	Text = "Design", ToggleMode = true,
			CommandSourceID = _dashContID,	CommandName = PXDashboardContainer.CommandDesignMode
		};
		controller.AppendToolbarItem(btn);
		SetVisibility(btn);

		btn = new PXToolBarButton {
			Key = "updateAll", Text = "Refresh All",
			CommandSourceID = _dashContID, CommandName = PXDashboardContainer.CommandRefresh
		};
		controller.AppendToolbarItem(btn);
		SetVisibility(btn);

		btn = new PXToolBarButton
		{
			Key = "createUserCopy",
			Text = "Create User Copy",
			CommandSourceID = _dashContID,
			CommandName = PXDashboardContainer.CommandCreateUserCopy
		};
		controller.AppendToolbarItem(btn);
		SetVisibility(btn);

		btn = new PXToolBarButton	{
			Key = "editLayout",	Text = PX.Dashboards.Messages.EditLayout, PopupPanel = _layoutPanelID, Visible = false
		};
		controller.AppendToolbarItem(btn);
		SetVisibility(btn);

		btn = new PXToolBarButton
		{
			Key = "editDashboard",
			CommandName = "editDashboard",
			CommandSourceID = _dataSourceID,
		};
		controller.AppendToolbarItem(btn);
		SetVisibility(btn);

		btn = new PXToolBarButton
		{
			Key = "resetToDefault",
			CommandName = "resetToDefault",
			CommandSourceID = _dataSourceID,
		};
		controller.AppendToolbarItem(btn);
		SetVisibility(btn);

		btn = new PXToolBarButton {
			Key = "addWidget", Text = "Add Widget", PopupPanel = _wizardID, Visible = false	};
		controller.AppendToolbarItem(btn);
		SetVisibility(btn);
}

	private void AppendPanels(ITitleModuleController controller)
	{
	}
	#endregion

	#region Fields

	private const string _wizardID = "pnlWidget";
	private const string _layoutPanelID = "pnlDashLayout";
	private const string _dashContID = "dashSet";
	private const string _dataSourceID = "ds";

	#endregion
}
