<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO502000.aspx.cs"
    Inherits="Page_SO502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.SO.SOOrderProcess" PrimaryView="Filter" BorderStyle="NotSet" Width="100%" PageLoadBehavior="PopulateSavedValues">
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
        EmailingGraph="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXSegmentMask CommitChanges="True" ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" DataField="Status" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" AlignLeft="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" AlignLeft="true" />
            <px:PXSelector CommitChanges="True" ID="edPrinterID" runat="server" DataField="PrinterID" />
			<px:PXTextEdit CommitChanges="true" ID="edNumberOfCopies" runat="server" DataField="NumberOfCopies" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        Caption="Documents" SkinID="PrimaryInquire" AllowPaging="true" AdjustPageSize="Auto" SyncPosition="true" FastFilterFields="OrderNbr,OrderDesc,CustomerOrderNbr,CustomerID,CustomerID_BAccountR_acctName">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edOrderType" runat="server" AllowNull="False" DataField="OrderType" Enabled="False" Text="SO" />
                    <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                    <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" Enabled="False" />
                    <px:PXDateTimeEdit ID="edOrderDate" runat="server" DataField="OrderDate" Enabled="False" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXNumberEdit ID="edCuryOrderTotal" runat="server" DataField="CuryOrderTotal" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False"
                        AllowMove="False" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="OrderType" DisplayFormat="&gt;LL" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderNbr" LinkCommand="ViewDocument" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderDesc" Label="Description" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerOrderNbr" Label="Customer Order" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Status" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RequestDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerID" DisplayFormat="AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerID_BAccountR_acctName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID" DisplayFormat="&gt;AAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID_Location_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DefaultSiteID" DisplayFormat="&gt;AAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DefaultSiteID_INSite_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipVia" DisplayFormat="AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipVia_Carrier_description" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ShipZoneID" DisplayFormat="AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="WorkgroupID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OwnerID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderWeight" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderVolume" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderQty" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryOrderTotal" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="Emailed" Type="CheckBox" AllowUpdate="False" TextAlign="Center"  />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
