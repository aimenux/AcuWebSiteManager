<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN508000.aspx.cs"
    Inherits="Page_IN508000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.IN.INReplenishmentCreate"
                     BorderStyle="NotSet" PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edSiteID" EmailingGraph="" TabIndex="900">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask ID="edSiteID" runat="server" CommitChanges="True" DataField="ReplenishmentSiteID" DataSourceID="ds" />
            <px:PXDateTimeEdit ID="edPurchaseDate" runat="server" CommitChanges="True" DataField="PurchaseDate" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector ID="edOwnerID" runat="server" CommitChanges="True" DataField="OwnerID" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" AlignLeft="True" />
            <px:PXLayoutRule runat="server" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector ID="edWorkGroupID" runat="server" CommitChanges="True" DataField="WorkGroupID" DataSourceID="ds" />
            <px:PXCheckBox ID="chkMyWorkGroup" runat="server" AlignLeft="True" CommitChanges="True" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOnlySuggested" runat="server" Checked="True" 
                DataField="OnlySuggested" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="PreferredVendorID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edItemClassCD" runat="server" DataField="ItemClassCD" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemCD" runat="server" DataField="SubItemCD" DataSourceID="ds" />
            <px:PXTextEdit CommitChanges="True" ID="edDescr" runat="server" DataField="Descr" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" AllowPaging="true" Style="z-index: 100; left: 0px; top: 0px;"
        Width="100%" SkinID="PrimaryInquire" Caption="Items Requiring Replenishment" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edPreferredVendorID" runat="server" DataField="PreferredVendorID" />
                    <px:PXSegmentMask ID="edReplenishmentSourceSiteID" runat="server" DataField="ReplenishmentSourceSiteID" Enabled="False" />
                    <px:PXSegmentMask ID="edPreferredVendorLocationID" runat="server" DataField="PreferredVendorLocationID" />
                    <px:PXTextEdit ID="edPreferredVendorName" runat="server" DataField="PreferredVendorName" Enabled="False" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" /></RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
                    <px:PXGridColumn DataField="SiteID" AllowUpdate="False" DisplayFormat="&gt;CCCCCCCCCC" />
                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC" AllowUpdate="False" LinkCommand="viewInventoryItem" />
                    <px:PXGridColumn DataField="Descr" AllowUpdate="False" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SubItemID" DisplayFormat="&gt;C" />
                    <px:PXGridColumn DataField="QtyProcess" AllowUpdate="False" AllowNull="False" TextAlign="Right" AutoCallBack="true" />
                    <px:PXGridColumn DataField="QtyINReplaned" AllowUpdate="False" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyOnHand" AllowUpdate="False" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyNotAvail" AllowUpdate="False" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyReplenishment" AllowUpdate="False" TextAlign="Right" />
					<px:PXGridColumn DataField="QtyDemand" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="QtyHardDemand" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="SafetyStock" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="MinQty" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="MaxQty" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="ReplenishmentSource" AllowUpdate="False" AutoCallBack="true" />
                    <px:PXGridColumn DataField="ReplenishmentSourceSiteID" AllowUpdate="False" DisplayFormat="&gt;AAAAAAAAAA" Label="Source Warehouse" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PreferredVendorID" DisplayFormat="&gt;AAAAAA" AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PreferredVendorLocationID" DisplayFormat="&gt;CCCCCCCCCC" AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PreferredVendorName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ItemClassID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorClassID" DisplayFormat="&gt;aaaaaaaaaa" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyPOPrepared" Label="Qty. PO Prepared" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyPOOrders" Label="Qty. PO Orders" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyPOReceipts" Label="Qty. PO Receipts" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyInTransit" Label="Qty. IN Transit" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyINReceipts" Label="Qty. IN Receipts" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyINAssemblySupply" Label="Qty. IN Assembly Supply" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtySOBackOrdered" Label="Qty. SO Back Ordered" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtySOPrepared" Label="Qty. SO Prepared" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtySOBooked" Label="Qty. SO Booked" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtySOShipped" Label="Qty. SO Shipped" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyFSSrvOrdPrepared" Label="Qty. FSSO Prepared" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyFSSrvOrdBooked" Label="Qty. FSSO Booked" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyFSSrvOrdAllocated" Label="Qty. FSSO Allocated" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="QtySOShipping" Label="Qty. SO Shipping" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="QtyINIssues" Label="Qty. IN Issues" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="QtyINAssemblyDemand" Label="Qty. IN Assembly Demand" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
