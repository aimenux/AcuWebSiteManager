<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM202530.aspx.cs" Inherits="Page_SM202530"
	Title="Synchronization Processing" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="filter"
		TypeName="PX.SM.SynchronizationProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Operation"
		Width="100%" DataMember="filter">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
				ControlSize="M" />
			<px:PXDropDown CommitChanges="True" ID="edOperation" runat="server" AllowNull="False"
				DataField="Operation" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px;
		top: 0px; height: 283px;" Width="100%" SkinID="Details" Caption="Available Files">
		<Levels>
			<px:PXGridLevel DataMember="SelectedFiles">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector ID="edName" runat="server" DataField="Name" Enabled="False" AllowEdit="True" />
					<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" Enabled="False" />
					<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"
						DisplayFormat="g" Enabled="False" />
					<px:PXDropDown ID="edSourceType" runat="server" DataField="SourceType" Enabled="False" />
					<px:PXTextEdit ID="edSourceUri" runat="server" DataField="SourceUri" Enabled="False" /></RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center"
						Type="CheckBox" Width="60" />
					<px:PXGridColumn AllowUpdate="False" DataField="Name" Label="Name" Width="128px" />
					<px:PXGridColumn AllowUpdate="False" DataField="SourceType" Label="Synchronization Type"
						RenderEditorText="True" Width="110px" />
					<px:PXGridColumn AllowUpdate="False" DataField="SourceUri" Label="Uri" Width="300px" />
					<px:PXGridColumn AllowUpdate="False" DataField="SourceIsFolder" Label="Synchronize Folder"
						TextAlign="Center" Type="CheckBox" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="SourceLastImportDate" Label="Last Import Date"
						Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="SourceLastExportDate" Label="Last Export Date"
						Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID" Label="Created by" Width="128px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<Actions>
				<Delete MenuVisible="false" ToolBarVisible="false" />
				<AddNew MenuVisible="false" ToolBarVisible="false" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
