<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM203590.aspx.cs" Inherits="Page_SM203590" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
		TypeName="PX.Data.Update.SpeedChecker" PrimaryView="Check">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="Measure" PostData="Self" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds"
		Style="z-index: 100" Width="100%" DataMember="Check">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
		    <px:PXTextEdit ID="Screen" runat="server" DataField="Screen" />
		    <px:PXTextEdit ID="Action" runat="server" DataField="Action" />
		    <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
		    <px:PXNumberEdit ID="Interval" runat="server" DataField="Interval" Enabled="False" Size="M" />
		    <px:PXNumberEdit ID="UsersCount" runat="server" DataField="UsersCount" Size="M" />
		</Template>
		<AutoSize Enabled="True" MinHeight="150" Container="Window" />
	</px:PXFormView>
</asp:Content>
