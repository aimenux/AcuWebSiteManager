<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM209000.aspx.cs"
    Inherits="Page_PM209000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ExternalCommitmentEntry" PrimaryView="Commitments">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Commitments" Caption="Committment"
        EmailingGraph="">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
			<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
            <px:PXSelector ID="edAccountGroupID" runat="server" DataField="AccountGroupID"/>
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
            <px:PXSegmentMask ID="edTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="true"/>
            <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID"/>
            <px:PXSegmentMask CommitChanges="True" ID="edCostCodeID" runat="server" DataField="CostCodeID"/>
            <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
            <px:PXSelector ID="edCuryID" runat="server" DataField="ProjectCuryID" />
            <px:PXNumberEdit ID="edOrigAmount" runat="server" DataField="OrigAmount" />
            <px:PXNumberEdit ID="edOrigQty" runat="server" DataField="OrigQty" />
            <px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
            <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
            <px:PXNumberEdit ID="edOpenAmount" runat="server" DataField="OpenAmount" />
            <px:PXNumberEdit ID="edOpenQty" runat="server" DataField="OpenQty" />
            <px:PXNumberEdit ID="edReceivedQty" runat="server" DataField="ReceivedQty" />
            <px:PXNumberEdit ID="edInvoicedAmount" runat="server" DataField="InvoicedAmount" />
            <px:PXNumberEdit ID="edInvoicedQty" runat="server" DataField="InvoicedQty" />
        </Template>
    </px:PXFormView>
</asp:Content>
