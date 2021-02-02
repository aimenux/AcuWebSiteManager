<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204550.aspx.cs" Inherits="Page_FormTab" Title="Addon Management" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Projects"
		TypeName="PX.SM.PackageMaintenance" PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="actionCheckoutWebsite" />
			<px:PXDSCallbackCommand Name="actionGetSolution" />
			<px:PXDSCallbackCommand Name="actionCheckinWebsite" Visible="false" CommitChanges="True"
				BlockPage="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Projects" Caption="Working Project" TemplateContainer="" >
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXTextEdit runat="server" DataField="Name" ID="edName" ReadOnly="True" Size="M" />
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
		    <px:PXTextEdit runat="server" DataField="Description" ID="edDescription" ReadOnly="True" Size="M" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
</asp:Content>
