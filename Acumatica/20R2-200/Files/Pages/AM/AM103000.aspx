<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM103000.aspx.cs" Inherits="Page_AM103000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.AM.EstimateSetup" BorderStyle="NotSet" PrimaryView="AMEstimateSetupRecord">
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
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="AMEstimateSetupRecord" DefaultControlID="edEstimateNumberingID" TabIndex="30500">
		<AutoSize Enabled="True" MinHeight="200" Container="Window" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  StartGroup="True" GroupCaption="General Settings" LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edEstimateNumberingID" runat="server" DataField="EstimateNumberingID" AllowEdit="True"/>
            <px:PXMaskEdit ID="edDefaultRevisionID" runat="server" DataField="DefaultRevisionID" />
            <px:PXCheckBox ID="edAutoNumberRevisionID" runat="server" DataField="AutoNumberRevisionID"/>
            <px:PXSelector ID="edDefaultEstimateClassID" runat="server" DataField="DefaultEstimateClassID" AllowEdit="True"/>
            <px:PXSelector ID="edDefaultWorkCenterID" runat="server" DataField="DefaultWorkCenterID" AllowEdit="True"/>
            <px:PXSelector ID="edDefaultOrderType" runat="server" DataField="DefaultOrderType" AllowEdit="True"/>
            <px:PXCheckBox ID="edNewRevisionIsPrimary" runat="server" DataField="NewRevisionIsPrimary"/>
            <px:PXLayoutRule runat="server" StartColumn="False"  StartGroup="True" GroupCaption="Create Inventory Items" LabelsWidth="M" ControlSize="XM" />
            <px:PXCheckBox ID="edUpdateAllRevisions" runat="server" DataField="UpdateAllRevisions"/>
            <px:PXCheckBox ID="edUpdatePriceInfo" runat="server" DataField="UpdatePriceInfo"/>
        </Template>
	</px:PXFormView>
</asp:Content>
