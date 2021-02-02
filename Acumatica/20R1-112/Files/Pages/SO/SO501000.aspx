<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO501000.aspx.cs" Inherits="Page_SO501000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOCreateShipment" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edAction">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" DataField="Action" />
            <px:PXDropDown CommitChanges="True" ID="edDateSel" runat="server" AllowNull="False" DataField="DateSel" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit ID="edShipmentDate" runat="server" DataField="ShipmentDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edCarrierPlugin" runat="server" DataField="CarrierPluginID" />
            <px:PXSelector CommitChanges="True" ID="edShipVia" runat="server" DataField="ShipVia" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" /></Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" 
        AllowSearch="true" BatchUpdate="true" Caption="Orders" FastFilterFields="OrderNbr,CustomerID,CustomerID_BAccountR_acctName,OrderDesc,CustomerOrderNbr" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Orders">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector Size="xxs" ID="edOrderType" runat="server" AllowNull="False" DataField="OrderType" Enabled="False" />
                    <px:PXLayoutRule runat="server" Merge="False" />
                    <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                    <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" Enabled="False" />
                    <px:PXDateTimeEdit ID="edOrderDate" runat="server" DataField="OrderDate" Enabled="False" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXNumberEdit ID="edCuryOrderTotal" runat="server" AllowNull="False" DataField="CuryOrderTotal" Enabled="False" /></RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="OrderType" DisplayFormat="&gt;LL" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderNbr" LinkCommand="ViewDocument" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderDesc" Label="Description" Width="117px" />
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
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar DefaultAction="ViewDocument"/>
    </px:PXGrid>
</asp:Content>
