<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO503050.aspx.cs" Inherits="Page_SO503050"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOPickingWorksheetProcess" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>			
            <px:PXDSCallbackCommand Visible="false" Name="SelectItems" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="SelectLocations" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="sM" ControlSize="M" />
            <px:PXDropDown ID="edAction" runat="server" DataField="Action" CommitChanges="True" />
            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="sM" ControlSize="M" />
            <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" CommitChanges="True" />
            <px:PXSelector ID="edCarrierPlugin" runat="server" DataField="CarrierPluginID" CommitChanges="True" />
            <px:PXSelector ID="edShipVia" runat="server" DataField="ShipVia" CommitChanges="True" />
            <px:PXDropDown ID="edPackagingType" runat="server" DataField="PackagingType" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="M" ControlSize="S" />
            <px:PXSegmentMask ID="edInventoryItem" runat="server" DataField="InventoryID" CommitChanges="True" />
            <px:PXButton runat="server" ID="btnItemList" Text="List" CommandName="SelectItems" CommandSourceID="ds"/>
            <px:PXLayoutRule runat="server" EndGroup="True" />
            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="M" ControlSize="S" />
            <px:PXSegmentMask ID="edLocation" runat="server" DataField="LocationID" CommitChanges="True" />
            <px:PXButton runat="server" ID="btnLocationList" Text="List" CommandName="SelectLocations" CommandSourceID="ds"/>
            <px:PXLayoutRule runat="server" EndGroup="True" />
            <px:PXNumberEdit ID="edMaxNumberOfLinesInShipment" runat="server" DataField="MaxNumberOfLinesInShipment" CommitChanges="True" />
            <px:PXNumberEdit ID="edMaxQtyInLines" runat="server" DataField="MaxQtyInLines" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="SM" GroupCaption="Process Parameters" />
            <px:PXNumberEdit ID="edNumberOfPickers" runat="server" DataField="NumberOfPickers" CommitChanges="True" />
            <px:PXNumberEdit ID="edNumberOfTotesPerPicker" runat="server" DataField="NumberOfTotesPerPicker" CommitChanges="True" AllowNull="True"/>
            <px:PXCheckBox ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" CommitChanges="True" AlignLeft="true" />
            <px:PXCheckBox ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" CommitChanges="True" AlignLeft="true" />
            <px:PXSelector ID="edPrinterName" runat="server" DataField="PrinterID" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" AllowPaging="false" AllowSearch="true" 
        SkinID="PrimaryInquire" BatchUpdate="true" Caption="Shipments" SyncPosition="True" FastFilterFields="ShipmentNbr,CustomerID,CustomerID_BAccountR_acctName,CustomerOrderNbr" RepaintColumns="true">
        <Levels>
            <px:PXGridLevel DataMember="Shipments">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edShipmentNbr" runat="server" DataField="ShipmentNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSelector ID="edWorksheetNbr" runat="server" DataField="CurrentWorksheetNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                    <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" Enabled="False" />
                    <px:PXDateTimeEdit ID="edShipDate" runat="server" DataField="ShipDate" Enabled="False" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXNumberEdit ID="edShipmentQty" runat="server" DataField="ShipmentQty" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowNull="False" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="ShipmentNbr" AllowUpdate="False" LinkCommand="viewDocument" />
                    <px:PXGridColumn DataField="CurrentWorksheetNbr" AllowUpdate="False" />
                    <px:PXGridColumn DataField="Status" AllowUpdate="False" RenderEditorText="True" />
                    <px:PXGridColumn DataField="ShipDate" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CustomerID" AllowUpdate="False" DisplayFormat="AAAAAAAAAA" />
                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CustomerLocationID" AllowUpdate="False" DisplayFormat="&gt;AAAAAAA" />
                    <px:PXGridColumn DataField="CustomerLocationID_Location_descr" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CustomerOrderNbr" AllowUpdate="False" />
                    <px:PXGridColumn DataField="BillSeparately" Type="CheckBox" />
                    <px:PXGridColumn DataField="SiteID" AllowUpdate="False" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="SiteID_INSite_descr" AllowUpdate="False" />
                    <px:PXGridColumn DataField="WorkgroupID" AllowUpdate="False" />
                    <px:PXGridColumn DataField="OwnerID" AllowUpdate="False" />
                    <px:PXGridColumn DataField="ShipmentQty" AllowNull="False" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="ShipVia" AllowUpdate="False" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
                    <px:PXGridColumn DataField="ShipVia_Carrier_description" AllowUpdate="False" />
                    <px:PXGridColumn DataField="ShipmentWeight" AllowNull="False" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="ShipmentVolume" AllowNull="False" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="LabelsPrinted" AllowUpdate="False" Type="CheckBox" />
                    <px:PXGridColumn DataField="Picked" AllowUpdate="False" Type="CheckBox" />
                    <px:PXGridColumn DataField="PickListPrinted" AllowUpdate="False" Type="CheckBox" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar DefaultAction="viewDocument"/>
    </px:PXGrid>
    <%-- Inventory Item List Dialog --%>
    <px:PXSmartPanel ID="InventoryItemListDialog" runat="server" Caption="Inventory Item List" CaptionVisible="True" Key="selectedItems" LoadOnDemand="True" AutoReload="True" AutoRepaint="True">
        <px:PXGrid ID="gridItemList" runat="server" DataSourceID="ds" SkinID="Details" SyncPosition="True">
            <Levels>
                <px:PXGridLevel DataMember="selectedItems">
                    <Columns>
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="Descr" />
                    </Columns>
                    <RowTemplate>
                        <px:PXSegmentMask ID="iidInventoryID" runat="server" DataField="InventoryID" CommitChanges="True" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="300" />
            <Mode AllowAddNew="True" AllowDelete="True" AllowUpdate="True" AllowUpload="True" AllowFormEdit="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
    <%-- Location List Dialog --%>
    <px:PXSmartPanel ID="LocationListDialog" runat="server" Caption="Location List" CaptionVisible="True" Key="selectedLocations" LoadOnDemand="True" AutoReload="True" AutoRepaint="True">
        <px:PXGrid ID="gridLocationList" runat="server" DataSourceID="ds" SkinID="Details" SyncPosition="True">
            <Levels>
                <px:PXGridLevel DataMember="selectedLocations">
                    <Columns>
                        <px:PXGridColumn DataField="LocationID" />
                        <px:PXGridColumn DataField="Descr" />
                    </Columns>
                    <RowTemplate>
                        <px:PXSegmentMask ID="ldLocationID" runat="server" DataField="LocationID" CommitChanges="True" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="300" />
            <Mode AllowAddNew="True" AllowDelete="True" AllowUpdate="True" AllowUpload="True" AllowFormEdit="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
</asp:Content>