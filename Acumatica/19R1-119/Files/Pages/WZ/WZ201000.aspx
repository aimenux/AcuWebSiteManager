<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="WZ201000.aspx.cs" Inherits="Page_WZ201000" 
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Scenarios" TypeName="PX.Objects.WZ.WZScenarioEntry" Visible="True">
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
        <DataTrees>
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID"  />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" SyncPosition="True" AutoAdjustColumns="True">
		<Levels>
			<px:PXGridLevel DataMember="Scenarios">
                <Columns>
                    <px:PXGridColumn DataField="Name" Width="300px" CommitChanges="True" LinkCommand="ViewScenarioDetails" />
                    <px:PXGridColumn DataField="Status" Width="100px" />
                    <px:PXGridColumn DataField="ExecutionDate" Width="200px" />
                    <px:PXGridColumn DataField="ExecutionPeriodID" Width="200px" />
                    <px:PXGridColumn DataField="Rolename" Width="200px" />
                    <px:PXGridColumn DataField="NodeID" TextField="SiteMap__Title" Width="200px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="ScenarioOrder" Width="100px" AllowShowHide="true" Visible="false" SyncVisible="false"/>
                </Columns>
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXTextEdit ID="edName" runat="server" DataField="Name" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="false"/>
                    <px:PXTextEdit ID="edExecutionDate" runat="server" DataField="ExecutionDate" Enabled="false"/>
                    <px:PXSelector ID="edRolename" runat="server" DataField="Rolename"/>

                    <px:PXTreeSelector CommitChanges="True" ID="edNodeID" runat="server" DataField="NodeID"
				        MinDropWidth="413" PopulateOnDemand="True" ShowRootNode="False" TreeDataMember="SiteMap"
				        TreeDataSourceID="ds" Size="M" AppendSelectedValue="True" AutoRefresh="True" >
				        <DataBindings>
					        <px:PXTreeItemBinding DataMember="SiteMap" ImageUrlField="Icon" TextField="Title"  ValueField="NodeID" />
				        </DataBindings>
				    </px:PXTreeSelector> 
                    <px:PXTextEdit ID="edOrder" runat="server" DataField="Order" />
                </RowTemplate>
			</px:PXGridLevel> 
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>