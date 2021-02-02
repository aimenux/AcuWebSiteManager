<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM206000.aspx.cs" Inherits="Page_AM206000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="EstimateClassRecords" TypeName="PX.Objects.AM.EstimateClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="EstimateClassRecords" DataKeyNames="EstimateClassID" 
        DefaultControlID="edEstimateClassID">
        <AutoSize Enabled="True" MinHeight="200" Container="Window" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edEstimateClassID" runat="server" DataField="EstimateClassID" CommitChanges="True" MaxLength="10" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" />
            <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" />
            <px:PXSelector ID="edEngineerID" runat="server" DataField="EngineerID" />
            <px:PXNumberEdit ID="edLeadTime" runat="server" DataField="LeadTime" />
            <px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" />
            <px:PXNumberEdit ID="edLaborMarkupPct" runat="server" DataField="LaborMarkupPct" />
            <px:PXNumberEdit ID="edMachineMarkupPct" runat="server" DataField="MachineMarkupPct" />
            <px:PXNumberEdit ID="edMaterialMarkupPct" runat="server" DataField="MaterialMarkupPct" />
            <px:PXNumberEdit ID="edToolMarkupPct" runat="server" DataField="ToolMarkupPct" />
            <px:PXNumberEdit ID="edOverheadMarkupPct" runat="server" DataField="OverheadMarkupPct" /> 
            <px:PXNumberEdit ID="edSubcontractMarkupPct" runat="server" DataField="SubcontractMarkupPct" />
        </Template>
	</px:PXFormView>
</asp:Content>

