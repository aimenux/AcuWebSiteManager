<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="WZ201000.aspx.cs" Inherits="Page_WZ201000" 
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script language="javascript" type="text/javascript">
		function commandResult(ds, context)
		{
			if (context.command == "Save" || context.command == "Delete")
			{
				var ds = px_all[context.id];
				var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
				if (isSitemapAltered) __refreshMainMenu();
			}
		}
	</script>
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Scenarios" TypeName="PX.Objects.WZ.WZScenarioEntry" Visible="True">
	    <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Insert" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Delete" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Next" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Previous" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Last" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="First" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewScenarioDetails" DependOnGrid="grid" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" OnDataBound="grid_OnDataBound"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" SyncPosition="True" AutoAdjustColumns="True">
		<Levels>
			<px:PXGridLevel DataMember="Scenarios">
                <Columns>
                    <px:PXGridColumn DataField="Name" Width="300px" CommitChanges="True" LinkCommand="ViewScenarioDetails" />
                    <px:PXGridColumn DataField="Status" Width="100px" />
                    <px:PXGridColumn DataField="ExecutionDate" Width="200px" />
                    <px:PXGridColumn DataField="ExecutionPeriodID" Width="200px" />
                    <px:PXGridColumn DataField="Rolename" Width="200px" />
                    <px:PXGridColumn DataField="WorkspaceID" Width="200px" Label="Workspace" />
                    <px:PXGridColumn DataField="SubcategoryID" Width="200px" Label="Category" />
                    <px:PXGridColumn DataField="ScenarioOrder" Width="100px" AllowShowHide="true" Visible="false" SyncVisible="false"/>
                </Columns>
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXTextEdit ID="edName" runat="server" DataField="Name" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="false"/>
                    <px:PXTextEdit ID="edExecutionDate" runat="server" DataField="ExecutionDate" Enabled="false"/>
                    <px:PXSelector ID="edRolename" runat="server" DataField="Rolename"/>
                    
		            <px:PXSelector runat="server" DataField="WorkspaceID" ID="edWorkspaceID" />
		            <px:PXSelector runat="server" DataField="SubcategoryID" ID="edSubcategoryID" />
                    <px:PXTextEdit ID="edOrder" runat="server" DataField="Order" />
                </RowTemplate>
			</px:PXGridLevel> 
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>