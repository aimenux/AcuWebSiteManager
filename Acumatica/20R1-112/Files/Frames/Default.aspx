<%@ Page Language="C#" MasterPageFile="~/MasterPages/Workspace.master" AutoEventWireup="true"
  CodeFile="Default.aspx.cs" Inherits="Frames_Default" Title="Acumatica" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/Workspace.master" %>

<asp:Content ID="c1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
        function form_AfterRepaint()
		{
            px_alls['dashSet'].refresh();
		}
	</script>   
	<% if (ShowWelcomePage)		{ %>
	<style>
		#page-caption{
			display:none;
		}
		html,body,.phDS,form{
			height:100%;
		}

		#welcomePage{
			box-sizing:border-box;
			height:100%;
			width:100%;
			border:0;
		}
	</style>
	<iframe id="welcomePage">
	</iframe>	
	<script>
		(function ()
		{
			document.body.classList.add('spinner')	
			var welcomePage = '<%:this.HomePage%>';
			var wikiUrl = '<%:this.WikiUrl%>';
			var navigationHappened = false;

			function loadSuccess()
			{
				document.body.classList.remove('spinner')
				console.log(this.responseText);
				var frame = document.getElementById('welcomePage');
				frame.src = welcomePage;

			}
			function loadError()
			{
				if (!navigationHappened)
				{
					window.location = wikiUrl;
				}
			}
			if (!welcomePage)
			{
				window.location = wikiUrl;
			}
			else
			{
				var oReq = new XMLHttpRequest();
				window.addEventListener('beforeunload', function () { navigationHappened = true; }, { once:true })
				oReq.addEventListener("load", loadSuccess);
				oReq.addEventListener("error", loadError);
				oReq.open("GET", welcomePage);
				oReq.send();
			}
		})();
	</script>


		<% } else { %>
	<px:PXDashboardDataSource runat="server" ID="ds" TypeName="PX.Dashboards.LayoutMaint" PrimaryView="Filter" PageLoadBehavior="SearchSavedKeys" FormID="form">
	    <CallbackCommands>
	        <px:PXDSCallbackCommand Name="designMode" />
	        <px:PXDSCallbackCommand Name="resetToDefault" SelectControlsIDs="dashSet" BlockPage="True" />
	        <px:PXDSCallbackCommand Name="editDashboard" SelectControlsIDs="dashSet" BlockPage="True" />
	    </CallbackCommands>
	</px:PXDashboardDataSource>

    <px:PXDashboardFormView id="form" runat="server" DataSourceID="ds" DataMember="Filter" Style="z-index: 100;" Width="100%" CaptionVisible="false">
        <ClientEvents AfterRepaint="form_AfterRepaint"/>
    </px:PXDashboardFormView>

	<px:PXDashboardContainer runat="server" ID="dashSet" DataSourceID="ds" DataMember="Widgets" 
		WizardWidgetViewName="Widget" DashboardViewName="Dashboard" WizardControlID="pnlWidget"
        CallbackUpdatable="True">
	    <CallBackMode RepaintControls="Unbound" />
		<%--<AutoSize Enabled="true" Container="Window" />--%>
		<Fields DashboardID="DashboardID" WidgetID="WidgetID" Type="Type" Settings="Settings" 
			Height="Height" Width="Width" Column="Column" Row="Row" Caption="Caption" Workspace="Workspace" />
	</px:PXDashboardContainer>

	<px:PXDashboardWizard runat="server" ID="pnlWidget" GraphName="PX.Dashboards.LayoutMaint" ViewName="Dashboard" 
		WidgetViewName="Widgets" WizardWidgetViewName="Widget">
		<Fields DashboardID="DashboardID" WidgetID="WidgetID" Type="Type" Settings="Settings" 
			Height="Height" Width="Width" Column="Column" Row="Row" Caption="Caption" Workspace="Workspace" />
	</px:PXDashboardWizard>
	<%	}  %>
</asp:Content>