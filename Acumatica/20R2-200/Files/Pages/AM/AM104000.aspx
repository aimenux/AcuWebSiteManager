<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM104000.aspx.cs" Inherits="Page_AM104000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="ConfigSetupRecord" SuspendUnloading="False" TypeName="PX.Objects.AM.ConfigSetup">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ConfigSetupRecord" TabIndex="900">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="M" StartColumn="True"/>
		    <px:PXLayoutRule runat="server" GroupCaption="General" StartGroup="True"/>
		    <px:PXSelector ID="edConfigNumberingID" runat="server" DataField="ConfigNumberingID" AllowEdit="True"/>
		    <px:PXMaskEdit ID="edDfltRevisionNbr" runat="server" DataField="DfltRevisionNbr"/>
		    <px:PXDropDown ID="edConfigKeyFormat" runat="server" DataField="ConfigKeyFormat"/>
		    <px:PXSelector ID="edDefaultKeyNumberingID" runat="server" DataField="DefaultKeyNumberingID" AllowEdit="True" CommitChanges="True"/>
            <px:PXCheckBox ID="edIsCompletionRequired" runat="server" DataField="IsCompletionRequired"/>
		    <px:PXLayoutRule runat="server" GroupCaption="Price" StartGroup="True"/>
		    <px:PXCheckBox ID="edHidePriceDetails" runat="server" DataField="HidePriceDetails"/>
		    <px:PXDropDown ID="edRollup" runat="server" DataField="Rollup"/>
		    <px:PXCheckBox ID="edAllowRollupOverride" runat="server" DataField="AllowRollupOverride"/>
		    <px:PXDropDown ID="edCalculate" runat="server" DataField="Calculate"/>
		    <px:PXCheckBox ID="edAllowCalculateOverride" runat="server" DataField="AllowCalculateOverride"/>
		    <px:PXLayoutRule runat="server" GroupCaption="Order Fields" StartColumn="True" StartGroup="True" SuppressLabel="True"/>
		    <px:PXCheckBox ID="edEnableWarehouse" runat="server" DataField="EnableWarehouse"/>
		    <px:PXCheckBox ID="edEnableSubItem" runat="server" DataField="EnableSubItem"/>
		    <px:PXCheckBox ID="edEnableDiscount" runat="server" DataField="EnableDiscount"/>
		    <px:PXCheckBox ID="edEnablePrice" runat="server" DataField="EnablePrice"/>
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
