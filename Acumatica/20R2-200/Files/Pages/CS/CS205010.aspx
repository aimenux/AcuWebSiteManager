<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS205010.aspx.cs" Inherits="Page_CS205010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="filter" TypeName="PX.Objects.CS.BuildingMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Delete" Visible="False" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" Visible="False" />
			<px:PXDSCallbackCommand Name="Previous" Visible="False" />
			<px:PXDSCallbackCommand Name="Next" Visible="False" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  Style="z-index: 100" Width="100%" Caption="Branch" DataMember="filter" TemplateContainer="" DataSourceID="ds" TabIndex="27500">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="m" LabelsWidth="sm" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchCD" runat="server" DataField="BranchID" DataSourceID="ds"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server"  Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Buildings">
		<Levels>
			<px:PXGridLevel DataMember="building">
				<Columns>
					<px:PXGridColumn DataField="BuildingCD" Width="100px" />
					<px:PXGridColumn DataField="Description" Width="250px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
