<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM401000.aspx.cs" Inherits="Page_AM401000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="ProdItemRecs" TypeName="PX.Objects.AM.CriticalMaterialsInq">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="Process" Visible="False" />
			<px:PXDSCallbackCommand Name="ProcessAll" Visible="False" />
			<px:PXDSCallbackCommand Name="Schedule" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="ProdItemRecs" DefaultControlID="edProdOrdID" Caption="Selection"> 
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" />
            <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AutoRefresh="True" DataSourceID="ds" CommitChanges="True" AllowEdit="True">
                <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXCheckBox CommitChanges="True" ID="chkShowAll" runat="server" DataField="ShowAll" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowAllocated" runat="server" DataField="ShowAllocated" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="200px" SkinID="Inquire" TabIndex="300">
		<Levels>
			<px:PXGridLevel DataMember="ProdMatlRecs" DataKeyNames="ProdOrdID,OperationID,LineID">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXTextEdit ID="edgOrderType" runat="server" DataField="OrderType" />
                    <px:PXTextEdit ID="edgProdOrdID" runat="server" DataField="ProdOrdID" />
                    <px:PXTextEdit ID="edOperationID" runat="server" DataField="OperationID" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                    <px:PXNumberEdit ID="edQtyRemaining" runat="server" DataField="QtyRemaining" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                    <px:PXNumberEdit ID="edQtyOnHand" runat="server" DataField="QtyOnHand" />
                    <px:PXNumberEdit ID="edQtyShort" runat="server" DataField="QtyShort" />
                    <px:PXDropDown ID="edReplenishmentSource" Size="m" runat="server" DataField="ReplenishmentSource"/>
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                    <px:PXCheckBox ID="edIsStockItem" runat="server" DataField="IsStockItem" />
                    <px:PXNumberEdit ID="edQtyAvail" runat="server" DataField="QtyAvail" />
                    <px:PXNumberEdit ID="edQtyHardAvail" runat="server" DataField="QtyHardAvail" />
                    <px:PXNumberEdit ID="edQtyProductionSupplyPrepared" runat="server" DataField="QtyProductionSupplyPrepared" />
                    <px:PXNumberEdit ID="edQtyProductionSupply" runat="server" DataField="QtyProductionSupply" />
                    <px:PXNumberEdit ID="edQtyProductionDemandPrepared" runat="server" DataField="QtyProductionDemandPrepared" />
                    <px:PXNumberEdit ID="edQtyProductionDemand" runat="server" DataField="QtyProductionDemand" />
                    <px:PXDateTimeEdit ID="edRequiredDate" runat="server" DataField="RequiredDate" DisplayFormat="d" />
                    <px:PXCheckBox ID="edIsByproduct2" runat="server" DataField="IsByproduct2" />
                    <px:PXNumberEdit ID="edTotalQtyRequired" runat="server" DataField="TotalQtyRequired" />
                    <px:PXNumberEdit ID="edBatchSize" runat="server" DataField="BatchSize" />
                    <px:PXSegmentMask ID="edPreferredVendorID" runat="server" DataField="PreferredVendorID" AllowEdit="True" />
                    <px:PXTextEdit ID="edPreferredVendorID_Vendor_AcctName" runat="server" DataField="PreferredVendorID_Vendor_AcctName"/>
                    <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" />
                    <px:PXCheckBox ID="edIsAllocated" runat="server" DataField="IsAllocated" />
                    <px:PXCheckBox ID="edPOCreate" runat="server" DataField="POCreate" />
                    <px:PXSelector ID="edPOOrderNbr" runat="server" DataField="POOrderNbr" AllowEdit="True" />
                    <px:PXCheckBox ID="edProdCreate" runat="server" DataField="ProdCreate" />
                    <px:PXSelector ID="edAMOrderType" runat="server" DataField="AMOrderType" />
                    <px:PXSelector ID="edAMProdOrdID" runat="server" DataField="AMProdOrdID" AllowEdit="True" />
                    <px:PXDropDown ID="edMatlMaterialType" runat="server" DataField="MaterialType" />
                    <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                    <px:PXGridColumn DataField="OrderType" Width="80px" />
                    <px:PXGridColumn DataField="ProdOrdID" AllowUpdate="False"  Width="130px" />
                    <px:PXGridColumn DataField="OperationID" MaxLength="10" Width="81px" />
                    <px:PXGridColumn DataField="LineID" Visible="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="SubItemID" Width="81px" />
                    <px:PXGridColumn DataField="Descr" Width="200px" />
                    <px:PXGridColumn DataField="QtyRemaining" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="UOM" Width="75px" />
                    <px:PXGridColumn DataField="QtyOnHand" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyShort" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="ReplenishmentSource" Width="120px"/>
                    <px:PXGridColumn DataField="SiteID" Width="120px"/>
                    <px:PXGridColumn DataField="IsStockItem" TextAlign="Center" Type="CheckBox" Width="60px"/>
                    <px:PXGridColumn DataField="QtyAvail" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyHardAvail" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyProductionSupplyPrepared" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyProductionSupply" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyProductionDemandPrepared" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyProductionDemand" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="RequiredDate" Width="90px" TextAlign="Right" />
                    <px:PXGridColumn DataField="IsByproduct2" TextAlign="Center" Type="CheckBox" Width="60px"/>
                    <px:PXGridColumn DataField="TotalQtyRequired" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="BatchSize" Width="108px" TextAlign="Right" />
                    <px:PXGridColumn DataField="PreferredVendorID" Width="120px"/>
                    <px:PXGridColumn DataField="PreferredVendorID_Vendor_AcctName" Width="120px"/>
                    <px:PXGridColumn DataField="ItemClassID" Width="80px" />
                    <px:PXGridColumn DataField="IsAllocated" TextAlign="Center" Type="CheckBox" Width="90px" />
                    <px:PXGridColumn DataField="POCreate" TextAlign="Center" Type="CheckBox" Width="90px" />
                    <px:PXGridColumn DataField="POOrderNbr" Width="108px" />
                    <px:PXGridColumn DataField="ProdCreate" TextAlign="Center" Type="CheckBox" Width="90px" />
                    <px:PXGridColumn DataField="AMOrderType" Width="80px" />
                    <px:PXGridColumn DataField="AMProdOrdID" Width="108px" />
                    <px:PXGridColumn DataField="MaterialType" />
                    <px:PXGridColumn DataField="SubcontractSource" Width="95px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
    <%-- Create Transfer Panel --%>
    <px:PXSmartPanel ID="panelCreateTransfer" runat="server" Caption="Create Transfer" CaptionVisible="true" LoadOnDemand="true" Key="TransferOrderFilter" CloseButtonDialogResult="Cancel"
        AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateTransfer" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        DesignView="Content" Height="205px" Width="385px" >
        <px:PXFormView ID="formCreateTransfer" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="TransferOrderFilter"
            SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                <px:PXSegmentMask CommitChanges="True" ID="edFromSiteID" runat="server" DataField="FromSiteID"/>
                <px:PXSegmentMask CommitChanges="True" ID="edToSiteID" runat="server" DataField="ToSiteID"/>
                <px:PXSelector CommitChanges="True" ID="edTransferOrderType" runat="server" DataField="OrderType" />
                <px:PXMaskEdit CommitChanges="True" ID="edTransferOrderNbr" runat="server" DataField="OrderNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
                <px:PXCheckBox CommitChanges="True" ID="edUseFullQtyTransferCheckBox" runat="server" DataField="UseFullQty" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="panelCreateTransferButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="createTransferButtonOK" runat="server" DialogResult="OK" Text="Create" />
            <px:PXButton ID="createTransferButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Create Production Orders Panel --%>
    <px:PXSmartPanel ID="CreateProdOrderPanel" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" CloseButtonDialogResult="Cancel"
        AutoCallBack-Target="FormCreateProdOrder" Caption="Create Production Order" CaptionVisible="True" Key="CreateProductionOrderFilter"
        DesignView="Content" Height="250px" Width="385px" LoadOnDemand="true" >
        <px:PXFormView ID="FormCreateProdOrder" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="CreateProductionOrderFilter" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                    <px:PXSelector CommitChanges="True" ID="edProdOrderType" runat="server" DataField="OrderType" AllowEdit="True"/>
                    <px:PXTextEdit CommitChanges="True" ID="edProdOrdID2" runat="server" DataField="ProdOrdID" />
                    <px:PXCheckBox CommitChanges="True" ID="edCreateLinkedOrdersCheckBox" runat="server" DataField="CreateLinkedOrders" />
                    <px:PXCheckBox CommitChanges="True" ID="edOverrideWarehouseCheckBox" runat="server" DataField="OverrideWarehouse" />
                    <px:PXSelector CommitChanges="True" ID="edSiteID2" runat="server" DataField="SiteID" />
                    <px:PXSelector CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" />
                    <px:PXCheckBox CommitChanges="True" ID="edUseFullQtyTransferCheckBox" runat="server" DataField="UseFullQty" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="CreateProdOrderButtonPanel" runat="server" SkinID="Buttons" >
            <px:PXButton ID="CreateProdOrderButton1" runat="server" DialogResult="OK" Text="Create" />
            <px:PXButton ID="CreateProdOrderPXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Create Purchase Orders Panel --%>
    <px:PXSmartPanel ID="CreatePurchaseOrderPanel" runat="server" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" 
        AutoCallBack-Target="FormCreatePurchaseOrder" Caption="Create Purchase Order" CaptionVisible="True" Key="CreatePurchaseOrderFilter"
        Height="190px" Width="385px" LoadOnDemand="true" CallBackMode-CommitChanges="True" >
        <px:PXFormView ID="FormCreatePurchaseOrder" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="CreatePurchaseOrderFilter" SkinID="Transparent" Width="100%">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                    <px:PXDropDown CommitChanges="True" ID="edPurchaseOrderType" runat="server" DataField="OrderType" />
                    <px:PXTextEdit CommitChanges="True" ID="edPurchaseOrderNbr" runat="server" DataField="OrderNbr" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons" >
            <px:PXButton ID="CreatePurchaseOrderButton1" runat="server" DialogResult="OK" Text="Create" />
            <px:PXButton ID="CreatePurchaseOrderButton2" runat="server" DialogResult="Abort" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
