<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM305500.aspx.cs" Inherits="Page_AM305500" Title="Unreleased Material Allocations" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.AM.UnreleasedMaterialAllocations" 
        PrimaryView="AMUnrelMaterialAllocationsFilterRecs">
		<CallbackCommands>
                <px:PXDSCallbackCommand Name="ViewDetail" Visible="False" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="AMUnrelMaterialAllocationsFilterRecs" DefaultControlID="edInventoryID" >
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" />
            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" CommitChanges="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AutoRefresh="true" />
            <px:PXSelector CommitChanges="True" ID="edProductionOrderNbr" runat="server" DataField="ProductionOrderNbr" AutoRefresh="true" /> 
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
            <px:PXSelector CommitChanges="True" ID="edPONbr" runat="server" DataField="PONbr" />
            <px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" />
            <px:PXSelector CommitChanges="True" ID="edSubAssyOrderType" runat="server" DataField="SubAssyOrderType" />
            <px:PXSelector CommitChanges="True" ID="edSubAssyProdOrdID" runat="server" DataField="SubAssyProdOrdID" /> 
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%">
		<Levels>
			<px:PXGridLevel DataKeyNames="OrderType, ProdOrdID, OperationID, LineID" DataMember="UnrelMaterialAllocationsDetailRecs">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty"  />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate"  />
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="true" />
                    <px:PXTextEdit ID="edOrderType" runat="server" DataField="OrderType" />
                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AllowEdit="true" />
                    <px:PXTextEdit ID="edOperationID" runat="server" DataField="OperationID" AllowEdit="true" />
                    <px:PXDropDown ID="edStatusID" runat="server" DataField="AMProdItem__StatusID" />
                    <px:PXSelector ID="edPOOrderNbr" runat="server" DataField="POOrderNbr" AllowEdit="true" />
                    <px:PXSelector ID="edPOReceiptNbr" runat="server" DataField="POReceiptNbr" AllowEdit="true" />
                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="true" />
                    <px:PXDropDown ID="edAMDocType" runat="server" DataField="AMDocType" />
                    <px:PXSelector ID="edAMBatNbr" runat="server" DataField="AMBatNbr" AllowEdit="true" />
                    <px:PXTextEdit ID="edAMOrderType" runat="server" DataField="AMOrderType" AllowEdit="true" />
                    <px:PXSelector ID="edAMProdOrdID" runat="server" DataField="AMProdOrdID" AllowEdit="true" /> 
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="True" Width="90px" />
                    <px:PXGridColumn DataField="InventoryID" Width="120px" />
                    <px:PXGridColumn DataField="SubItemID" Width="80px" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="110px" />
                    <px:PXGridColumn DataField="UOM" Width="80px" />
                    <px:PXGridColumn DataField="TranDate" Width="90px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="OrderType" Width="60px" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="120px" />
                    <px:PXGridColumn DataField="OperationID" Width="120px" LinkCommand="ViewDetail" />
                    <px:PXGridColumn DataField="AMProdItem__StatusID" RenderEditorText="True" Width="100px" />
                    <px:PXGridColumn DataField="POOrderNbr" Width="120px" />
                    <px:PXGridColumn DataField="POReceiptNbr" Width="120px" />
                    <px:PXGridColumn DataField="VendorID" Width="120px" />
                    <px:PXGridColumn DataField="AMDocType" Width="80px" />
                    <px:PXGridColumn DataField="AMBatNbr" Width="120px" />
                    <px:PXGridColumn DataField="AMOrderType" Width="80px" />
                    <px:PXGridColumn DataField="AMProdOrdID" Width="130px" />
                  </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
