<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="DR101000.aspx.cs" Inherits="Page_DR101000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="DRSetupRecord" TypeName="PX.Objects.DR.DRSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="DRSetupRecord" AllowCollapse="False" DefaultControlID="edScheduleNumberingID" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" />
			<px:PXSelector DataField="ScheduleNumberingID" ID="edScheduleNumberingID" runat="server"  AllowEdit="True" />
			<px:PXCheckBox DataField="UseFairValuePricesInBaseCurrency" ID="chkUseFairValuePricesInBaseCurrency" runat="server" SuppressLabel="True" AlignLeft="True" Size="Empty"/>
		</Template>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXFormView>
</asp:Content>
