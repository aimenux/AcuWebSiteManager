<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204560.aspx.cs" Inherits="Page_FormTab" Title="Addon Management" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Projects"
		TypeName="PX.SM.DatabaseMaintenance" PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Cancel" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Pack" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="actionSiteMap" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="actionReports" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="actionGenInq" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Projects" Caption="Working Project">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
				ControlSize="M" />
			<px:PXTextEdit runat="server" DataField="Name" ID="edName" ReadOnly="True" />
			<px:PXTextEdit runat="server" DataField="Description" ID="edDescription" ReadOnly="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%">
		<AutoSize Enabled="true" Container="Window" />
		<Items>
			<px:PXTabItem Text="Database Tables">
				<Template>
					<px:PXGrid ID="TablesGrid" runat="server" Height="100%" Width="100%" DataSourceID="ds"
						SkinID="Details" Style="left: 0px; top: 0px">
						<Levels>
							<px:PXGridLevel DataMember="Tables">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edTableName" runat="server" DataField="TableName" AutoRefresh="true" />
									<px:PXCheckBox ID="chkExportData" runat="server" DataField="ExportData" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TableName" Label="Table Name" Width="300px" />
									<px:PXGridColumn DataField="CreateSchema" Type="CheckBox" Width="110px" />
									<px:PXGridColumn DataField="CustomScript" Width="300px" Multiline="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="Save" Text="Save" />
							    <px:PXToolBarButton CommandSourceID="ds" CommandName="Cancel" Text="Cancel" />
							    <px:PXToolBarButton Text="Script Tables" CommandSourceID="ds" CommandName="Pack" />
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Database Reports">
				<Template>
					<px:PXGrid ID="PXGrid1" runat="server" SkinID="Details" Width="100%" Height="100%"
						DataSourceID="ds">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="ViewReports">
								<Columns>
									<px:PXGridColumn DataField="IsInProject" Type="CheckBox" Width="100px" />
									<px:PXGridColumn DataField="Name" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Checkin Reports" CommandSourceID="ds" CommandName="actionReports" />
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Site Map">
				<Template>
					<px:PXGrid ID="PXGrid2" runat="server" SkinID="Details" Width="100%" Height="100%"
						DataSourceID="ds">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="ViewSelectSiteMap">
								<Columns>
									<px:PXGridColumn DataField="IsInProject" Width="100px" Type="CheckBox" />
									<px:PXGridColumn DataField="Name" Width="300px" />
									<px:PXGridColumn DataField="Url" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Checkin Sitemap" CommandSourceID="ds" CommandName="actionSiteMap" />
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
</asp:Content>
