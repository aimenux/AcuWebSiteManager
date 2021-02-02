<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="TX103000.aspx.cs" Inherits="Page_TX103000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TXSetupRecord" TypeName="PX.Objects.TX.TXSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="TXSetupRecord" Caption="Tax Preferences" AllowCollapse="False" DefaultControlID="edTaxRoundingGainAcctID" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" StartColumn="True" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Rounding Settings" StartColumn="True" />
			<px:PXSelector ID="edTaxRoundingGainAcctID" runat="server" DataField="TaxRoundingGainAcctID" CommitChanges="true" />
			<px:PXSegmentMask ID="edTaxRoundingGainSubID" runat="server" DataField="TaxRoundingGainSubID" CommitChanges="true" />
			<px:PXSelector ID="edTaxRoundingLossAcctID" runat="server" DataField="TaxRoundingLossAcctID" CommitChanges="true" />
			<px:PXSegmentMask ID="edTaxRoundingLossSubID" runat="server" DataField="TaxRoundingLossSubID" CommitChanges="true" />
		</Template>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXFormView>
</asp:Content>
