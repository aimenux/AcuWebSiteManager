<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN407000.aspx.cs"
    Inherits="Page_IN407000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="PopulateSavedValues" PrimaryView="Filter"
        TypeName="PX.Objects.IN.InventoryLotSerInq">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewSummary" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewAllocDet" DependOnGrid="grid" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify"
        DataMember="Filter" DefaultControlID="edLotSerialNbr">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXTextEdit CommitChanges="True" ID="edLotSerialNbr" runat="server" AllowNull="False" DataField="LotSerialNbr" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
            <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" AutoRefresh="True" DataField="LocationID" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" AllowEdit="true" >
                <GridProperties>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemCD" runat="server" DataField="SubItemCD" AutoRefresh="true" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" DisplayFormat="d" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" DisplayFormat="d" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkShowAdjUnitCost" runat="server" DataField="ShowAdjUnitCost" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" AdjustPageSize="Auto"
        AllowPaging="True" AllowSearch="True" BatchUpdate="True" Caption="Transaction Details" SkinID="Inquire" FastFilterFields="VendorID,VendorName" RestrictFields="True" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edVendor__AcctCD" runat="server" DataField="Vendor__AcctCD" AllowEdit="true" />
                    <px:PXSegmentMask ID="edCustomer__AcctCD" runat="server" DataField="Customer__AcctCD" AllowEdit="true" />
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="true" />
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="true" />
                    <px:PXSelector ID="edPOReceiptLine__ReceiptNbr" runat="server" DataField="POReceiptLine__ReceiptNbr" AllowEdit="true" />
                    <px:PXDropDown ID="edPOReceiptType" runat="server" DataField="POReceiptType" />
                    <px:PXTextEdit ID="edSOOrderType" runat="server" DataField="SOOrderType" />
                    <px:PXSelector ID="edSOOrderNbr" runat="server" DataField="SOOrderNbr" AllowEdit="true" />
                    <px:PXTextEdit ID="edTotalQty" DataField="TotalQty" runat="server"/>
                    <px:PXTextEdit ID="edTotalCost" DataField="TotalCost" runat="server"/>
                    <px:PXTextEdit ID="edAdditionalCost" DataField="AdditionalCost" runat="server" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="TranDate" Width="90px" />
                    <px:PXGridColumn DataField="TranType" />
                    <px:PXGridColumn DataField="DocType" Visible="false" />
                    <px:PXGridColumn DataField="RefNbr">
                        <NavigateParams>
                            <px:PXControlParam ControlID="grid" Name="DocType" PropertyName="DataValues[&quot;DocType&quot;]" Type="String" />
                            <px:PXControlParam ControlID="grid" Name="RefNbr" PropertyName="DataValues[&quot;RefNbr&quot;]" Type="String" />
                        </NavigateParams>
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" />
                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="LocationID" AllowShowHide="Server" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="LotSerialNbr" Width="120px" />
                    <px:PXGridColumn DataField="ExpireDate" DisplayFormat="d" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="UOM" DisplayFormat="&gt;aaaaaa" Label="UOM" />
                    <px:PXGridColumn DataField="InvQty" Label="Quantity" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="TranUnitCost" Label="Unit Cost" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="SOOrderType" DisplayFormat="&gt;aa" />
                    <px:PXGridColumn DataField="SOOrderNbr" />
                    <px:PXGridColumn DataField="Customer__AcctCD" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="Customer__AcctName" Width="200px" />
                    <px:PXGridColumn DataField="POReceiptType" RenderEditorText="True" />
                    <px:PXGridColumn DataField="POReceiptLine__ReceiptNbr" />
                    <px:PXGridColumn DataField="Vendor__AcctCD" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="Vendor__AcctName" Width="200px" />
                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="InventoryID_InventoryItem_Descr" Visible="false" Width="120px" />
                    <px:PXGridColumn DataField="TotalQty" Visible="false" Width="0px" AllowShowHide="Server" />
                    <px:PXGridColumn DataField="TotalCost" Visible="false" Width="0px" AllowShowHide="Server" />
                    <px:PXGridColumn DataField="AdditionalCost" Visible="false" Width="0px" AllowShowHide="Server" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="allocDet" PagerVisible="False">
            <CustomItems>
                <px:PXToolBarButton Key="summary" Text="Summary">
                    <AutoCallBack Target="ds" Command="ViewSummary" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="allocDet" Text="Allocation Details">
                    <AutoCallBack Target="ds" Command="ViewAllocDet" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
