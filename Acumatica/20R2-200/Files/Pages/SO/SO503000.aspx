<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO503000.aspx.cs" Inherits="Page_SO503000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOInvoiceShipment" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="sM" ControlSize="M" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" DataField="Action" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXDateTimeEdit ID="edInvoiceDate" runat="server" DataField="InvoiceDate" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowPrinted" runat="server" DataField="ShowPrinted" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="sM" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edCarrierPlugin" runat="server" DataField="CarrierPluginID" />
            <px:PXSelector CommitChanges="True" ID="edShipVia" runat="server" DataField="ShipVia" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
            <px:PXDropDown CommitChanges="True" ID="edPackagingType" runat="server" DataField="PackagingType" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" AlignLeft="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" AlignLeft="true" />
            <px:PXSelector CommitChanges="True" ID="edPrinterID" runat="server" DataField="PrinterID" />
			<px:PXTextEdit CommitChanges="true" ID="edNumberOfCopies" runat="server" DataField="NumberOfCopies" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" AllowPaging="true" AdjustPageSize="Auto" 
        SkinID="PrimaryInquire" AllowSearch="true" BatchUpdate="true" Caption="Shipments" SyncPosition="True" FastFilterFields="ShipmentNbr,CustomerID,CustomerID_BAccountR_acctName,CustomerOrderNbr" RepaintColumns="true">
        <Levels>
            <px:PXGridLevel DataMember="Orders">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edShipmentNbr" runat="server" DataField="ShipmentNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                    <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" Enabled="False" />
                    <px:PXDateTimeEdit ID="edShipDate" runat="server" DataField="ShipDate" Enabled="False" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXNumberEdit ID="edShipmentQty" runat="server" AllowNull="False" DataField="ShipmentQty" Enabled="False" /></RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipmentNbr" LinkCommand="viewDocument" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Status" RenderEditorText="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerID" DisplayFormat="AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerID_BAccountR_acctName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID" DisplayFormat="&gt;AAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID_Location_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerOrderNbr" />
                    <px:PXGridColumn DataField="BillSeparately" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SiteID_INSite_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="WorkgroupID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OwnerID" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ShipmentQty" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipVia" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipVia_Carrier_description" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ShipmentWeight" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ShipmentVolume" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="LabelsPrinted" Type="CheckBox" TextAlign="Center" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar DefaultAction="viewDocument"/>
    </px:PXGrid>
</asp:Content>
