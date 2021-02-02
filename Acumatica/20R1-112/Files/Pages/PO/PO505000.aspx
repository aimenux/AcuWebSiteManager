<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO505000.aspx.cs"
    Inherits="Page_PO505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PO.POCreate" PrimaryView="Filter"/>
    
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" DataMember="Filter" Width="100%" Caption="Selection"
        DefaultControlID="edPurchDate" EmailingGraph="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="PurchDate" ID="edPurchDate" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXSegmentMask CommitChanges="True" ID="edItemClassCD" runat="server" DataField="ItemClassCD" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID">
                <GridProperties>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="SiteID" ID="edSiteID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="RequestedOnDate" ID="edRequestedOnDate" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="VendorID" ID="edVendorID" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="true" />
            <px:PXSelector CommitChanges="True" ID="edSrvOrdType" runat="server" DataField="SrvOrdType" />
            <px:PXSelector CommitChanges="True" ID="edserviceOrderRefNbr" runat="server" DataField="serviceOrderRefNbr" AutoRefresh="true" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXNumberEdit ID="edOrderTotal" runat="server" DataField="OrderTotal" Enabled="False" />
            <px:PXNumberEdit ID="edOrderWeight" runat="server" DataField="OrderWeight" Enabled="False" />
            <px:PXNumberEdit ID="edOrderVolume" runat="server" DataField="OrderVolume" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" SyncPosition="True"
        FastFilterFields="InventoryID,InventoryID_InventoryItem_descr,VendorID,VendorID_Vendor_acctName,SOOrder__OrderNbr,SOOrder__CustomerID,SOOrder__CustomerID_BAccountR_acctName,FSRefNbr" Caption="Details"
		AllowPaging="True" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="FixedDemand">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edPOSiteID" runat="server" DataField="POSiteID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edSourceSiteID" runat="server" DataField="SourceSiteID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXDateTimeEdit ID="edPlanDate" runat="server" DataField="PlanDate" Enabled="False" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" Enabled="False" AutoRefresh="true" AllowEdit="true">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="SOOrder__CustomerID" AllowEdit="true" />
                    <px:PXSelector ID="edSOOrderNbr" runat="server" DataField="SOOrder__OrderNbr" AllowEdit="true" />
                    <px:PXSelector ID="edFSRefNbr" runat="server" DataField="FSRefNbr" AllowEdit="true" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanType_INPlanType_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID" DisplayFormat="&gt;AAA-&gt;CCC-&gt;AA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID_InventoryItem_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SubItemID" DisplayFormat="&gt;AA-A" />
                    <px:PXGridColumn AllowUpdate="False" DataField="POSiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="POSiteID_description" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SourceSiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SourceSiteDescr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="UOM" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderQty" TextAlign="Right" AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID_Vendor_acctName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorLocationID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Location__vLeadTime" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AddLeadTimeDays" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Vendor__TermsID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Location__vCarrierID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="effPrice" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ExtCost" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Vendor__CuryID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID_BAccountR_acctName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerLocationID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOLine__UnitPrice" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOLine__UOM" />
                    <px:PXGridColumn DataField="SOOrder__OrderNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" LinkCommand="viewDocument" />
                    <px:PXGridColumn DataField="FSRefNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" LinkCommand="viewServiceOrderDocument"/>
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ExtWeight" Label="Weight" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ExtVolume" Label="Volume" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar DefaultAction="viewDocument"/>
    </px:PXGrid>
</asp:Content>
