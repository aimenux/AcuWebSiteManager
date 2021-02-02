<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS211000.aspx.cs" Inherits="Page_CS211000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.ReasonCodeMaint" PrimaryView="reasoncode">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="reasoncode" Caption="Reason Code" TemplateContainer="">
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edReasonCodeID" runat="server" DataField="ReasonCodeID" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXDropDown CommitChanges="True" ID="edUsage" runat="server" DataField="Usage" />
			<px:PXSegmentMask ID="edSubMask" runat="server" DataField="SubMaskInventory" />
			<px:PXTextEdit ID="PXSegmentMask2" runat="server" DataField="SubMask" />
			<px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="SubMaskFinance" />
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" />
			<px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
