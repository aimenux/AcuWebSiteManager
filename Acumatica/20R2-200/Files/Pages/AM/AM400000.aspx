<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM400000.aspx.cs" Inherits="Page_AM400000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Detailrecs" TypeName="PX.Objects.AM.MRPDisplay" Visible="True" BorderStyle="NotSet" >
        <CallbackCommands>
            <px:PXDSCallbackCommand Visible="false" Name="AMRPDetail$RefNoteID$Link" DependOnGrid="grid" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="AMRPDetail$ParentRefNoteID$Link" DependOnGrid="grid" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="AMRPDetail$ProductRefNoteID$Link" DependOnGrid="grid" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
<px:PXGrid ID="grid" runat="server" Width="100%" 
           AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" TabIndex="1500">
    <Levels>
        <px:PXGridLevel DataMember="Detailrecs" >
            <RowTemplate>
                <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                <px:PXTextEdit ID="edInventoryID_InventoryItem_descr" runat="server" DataField="InventoryID_InventoryItem_descr"/>
                <px:PXDateTimeEdit ID="edActionDate" runat="server" DataField="ActionDate"/>
                <px:PXNumberEdit ID="edActionLeadTime" runat="server" DataField="ActionLeadTime"/>
                <px:PXMaskEdit ID="edBOMID" runat="server" DataField="BOMID"/>
                <px:PXSelector ID="edBOMRevisionID" runat="server" DataField="BOMRevisionID" AllowEdit="True"/>
                <px:PXSegmentMask ID="edParentInventoryID" runat="server" DataField="ParentInventoryID" AllowEdit="True"/>
                <px:PXSegmentMask ID="edParentSubItemID" runat="server" DataField="ParentSubItemID" />
                <px:PXSegmentMask ID="edProductInventoryID" runat="server" DataField="ProductInventoryID" AllowEdit="True"/>
                <px:PXSegmentMask ID="edProductSubItemID" runat="server" DataField="ProductSubItemID" />
                <px:PXDateTimeEdit ID="edPromiseDate" runat="server" DataField="PromiseDate"/>
                <px:PXDropDown ID="edRefType" runat="server" DataField="RefType" />
                <px:PXDropDown ID="edSDFlag" runat="server" DataField="SDFlag"/>
                <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                <px:PXDropDown ID="edReplenishmentSource" runat="server" DataField="ReplenishmentSource"/>
                <px:PXNumberEdit ID="edBaseQty" runat="server" DataField="BaseQty"/>
                <px:PXSelector ID="edBaseUOM" runat="server" DataField="BaseUOM"/>
                <px:PXDropDown ID="edType" runat="server" DataField="Type" />
                <px:PXSelector ID="edProductManagerID" runat="server" DataField="ProductManagerID"/>
                <px:PXSegmentMask ID="edPreferredVendorID" runat="server" DataField="PreferredVendorID" AllowEdit="True" />
                <px:PXTextEdit ID="edPreferredVendorID_Vendor_AcctName" runat="server" DataField="PreferredVendorID_Vendor_AcctName"/>
                <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"/>
                <px:PXTextEdit ID="edProductManagerID_EPEmployee_AcctName" runat="server" DataField="ProductManagerID_EPEmployee_AcctName"/>
                <px:PXNumberEdit ID="edRecordID" runat="server" DataField="RecordID"/>
                <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" />
            </RowTemplate>
            <Columns>
                <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" Width="60px"/>
                <px:PXGridColumn DataField="InventoryID" Width="150px"/>
                <px:PXGridColumn DataField="InventoryID_InventoryItem_descr" Width="200px"/>
                <px:PXGridColumn DataField="SubItemID" Width="120px" />
                <px:PXGridColumn DataField="ReplenishmentSource" CommitChanges="True"/>
                <px:PXGridColumn DataField="SiteID" Width="120px"/>
                <px:PXGridColumn DataField="BaseQty" TextAlign="Right" Width="100px"/>
                <px:PXGridColumn DataField="PromiseDate" Width="90px"/>
                <px:PXGridColumn DataField="ActionDate" Width="90px"/>
                <px:PXGridColumn DataField="Type" Width="150px"/>
                <px:PXGridColumn DataField="ProductManagerID"/>
                <px:PXGridColumn DataField="ProductManagerID_EPEmployee_AcctName" Width="200px"/>
                <px:PXGridColumn DataField="PreferredVendorID" Width="120px"/>
                <px:PXGridColumn DataField="PreferredVendorID_Vendor_AcctName" Width="120px"/>
                <px:PXGridColumn DataField="ParentInventoryID"/>
                <px:PXGridColumn DataField="ParentSubItemID"/>
                <px:PXGridColumn DataField="ProductInventoryID"/>
                <px:PXGridColumn DataField="ProductSubItemID"/>
                <px:PXGridColumn DataField="SDFlag"/>
                <px:PXGridColumn DataField="RefType" Width="150px"/>
                <px:PXGridColumn DataField="ActionLeadTime" TextAlign="Right"/>
                <px:PXGridColumn DataField="CreatedDateTime" Width="90px"/>
                <px:PXGridColumn DataField="BOMID"/>
                <px:PXGridColumn DataField="BOMRevisionID"/>
                <px:PXGridColumn DataField="BaseUOM"/>
                <px:PXGridColumn DataField="RecordID" TextAlign="Right"/>
                <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="AMRPDetail$RefNoteID$Link" Width="175px" />
                <px:PXGridColumn DataField="ParentRefNoteID" RenderEditorText="True" LinkCommand="AMRPDetail$ParentRefNoteID$Link" Width="175px" />
                <px:PXGridColumn DataField="ProductRefNoteID" RenderEditorText="True" LinkCommand="AMRPDetail$ProductRefNoteID$Link" Width="175px" />
                <px:PXGridColumn DataField="ItemClassID"/>
            </Columns>
        </px:PXGridLevel>
    </Levels>
    <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    <ActionBar ActionsText="False">
    </ActionBar>
</px:PXGrid>
<%-- Create Purchase Orders Panel --%>
<px:PXSmartPanel ID="PanelPurchaseOrder" runat="server" Caption="Create Purchase Order" CaptionVisible="True"
                 LoadOnDemand="True" Key="PlanPurchaseOrderFilter" AutoCallBack-Target="formPurchaseOrder" 
                 AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" 
                Width="800px" Height="450px" TabIndex="2000" AutoCallBack-Enabled="True" >
    <px:PXFormView ID="formPurchaseOrder" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="PlanPurchaseOrderFilter" SkinID="Transparent" Width="100%" SyncPosition="True">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown CommitChanges="True" ID="edPurchaseOrderType" runat="server" DataField="OrderType" />
            <px:PXTextEdit CommitChanges="True" ID="edPurchaseOrderNbr" runat="server" DataField="OrderNbr" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
        </Template>
    </px:PXFormView>
    <px:PXGrid ID="gridPurchaseDisplay" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire">
        <Levels>
            <px:PXGridLevel DataMember="ProcessRecords">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXSegmentMask ID="edPurchaseInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXSegmentMask ID="edPurchaseSubitem" runat="server" DataField="SubItemID" />
                    <px:PXDropDown ID="edPurchaseSource" runat="server" DataField="Source" />
                    <px:PXSegmentMask ID="edPurchaseSiteID" runat="server" DataField="SiteID" />
                    <px:PXNumberEdit ID="edPurchaseQty" runat="server" DataField="Qty" CommitChanges="True" />
                    <px:PXTextEdit ID="edPurchaseUOM" runat="server" DataField="UOM"/>
                    <px:PXDateTimeEdit ID="edPurchasePlanDate" runat="server" DataField="PlanDate"/>
                    <px:PXNumberEdit ID="edPurchaseGroupNumber" runat="server" DataField="GroupNumber" CommitChanges="True" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="SubItemID" Width="70px" />
                    <px:PXGridColumn DataField="Source" Width="120px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="Qty" Width="130px" TextAlign="Right" CommitChanges="True" />
                    <px:PXGridColumn DataField="UOM" Width="60px"  />
                    <px:PXGridColumn DataField="PlanDate" Width="80px" />
                    <px:PXGridColumn DataField="GroupNumber" Width="65px" TextAlign="Right" CommitChanges="True" />
                </Columns>
                <Mode AllowAddNew="false" AllowUpdate="true" AllowDelete="false" />
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Parent" Enabled="True"/>
        <ActionBar ActionsText="False">
        </ActionBar>
    </px:PXGrid>
    <px:PXPanel ID="PXPanelPurchaseBtn" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButtonPurchaseOk" runat="server" DialogResult="OK" Text="Create"></px:PXButton>
        <px:PXButton ID="PXButtonPurchaseCancel" runat="server" DialogResult="Abort" Text="Cancel"></px:PXButton>
    </px:PXPanel>
</px:PXSmartPanel>
<%-- Create Production orders Panel --%>
<px:PXSmartPanel ID="PanelProductionOrder" runat="server" Caption="Create Manufacture Orders" CaptionVisible="True"
                 DesignView="Content" LoadOnDemand="True" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" AutoReload="True" CloseButtonDialogResult="Abort"
                 Key="PlanManufactureOrderFilter" AutoCallBack-Target="gridManufactureDisplay" CallBackMode-PostData="Page" Width="1000px" Height="450px" TabIndex="2000" >
    <px:PXGrid ID="gridManufactureDisplay" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataMember="ProcessRecords" DataKeyNames="LineNbr" >
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXSegmentMask ID="edManufactureInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXSegmentMask ID="edManufactureSubitem" runat="server" DataField="SubItemID" />
                    <px:PXDropDown ID="edManufactureSource" runat="server" DataField="Source" />
                    <px:PXSegmentMask ID="edManufactureSiteID" runat="server" DataField="SiteID" />
                    <px:PXNumberEdit ID="edmanufactureQty" runat="server" DataField="Qty" />
                    <px:PXTextEdit ID="edManufactureUOM" runat="server" DataField="UOM"/>
                    <px:PXDateTimeEdit ID="edManufacturePlanDate" runat="server" DataField="PlanDate"/>
                    <px:PXSelector ID="edManufactureOrderType" runat="server" DataField="OrderType" />
                    <px:PXTextEdit ID="edProductionNbr" runat="server" DataField="ProdOrdID" />
                    <px:PXNumberEdit ID="edmanufactureGroupNumber" runat="server" DataField="GroupNumber" CommitChanges="True" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="SubItemID" Width="70px" />
                    <px:PXGridColumn DataField="Source" Width="120px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="Qty" Width="130px" TextAlign="Right" CommitChanges="True" />
                    <px:PXGridColumn DataField="UOM" Width="60px"  />
                    <px:PXGridColumn DataField="PlanDate" Width="80px" />
                    <px:PXGridColumn DataField="OrderType" Width="65px" CommitChanges="True"  />
                    <px:PXGridColumn DataField="ProdOrdID" Width="135px" CommitChanges="True" />
                    <px:PXGridColumn DataField="GroupNumber" Width="65px" TextAlign="Right" CommitChanges="True" />
                </Columns>
                <Mode AllowAddNew="false" AllowUpdate="true" AllowDelete="false" />
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Parent" Enabled="True"/>
        <ActionBar ActionsText="False">
        </ActionBar>
    </px:PXGrid>
    <px:PXPanel ID="PXPanelmanufactureBtn" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButtonManufactureOk" runat="server" DialogResult="OK" Text="Create"></px:PXButton>
        <px:PXButton ID="PXButtonManufactureCancel" runat="server" DialogResult="Abort" Text="Cancel"></px:PXButton>
    </px:PXPanel>
</px:PXSmartPanel>
<%-- Create Transfer Panel --%>
<px:PXSmartPanel ID="PanelTransferOrder" runat="server" Caption="Create Transfer Order" CaptionVisible="True"
                 DesignView="Content" LoadOnDemand="True" Key="PlanTransferOrderFilter" AutoCallBack-Target="PlanTransferOrderFilter" 
                 AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" AutoReload="True" CloseButtonDialogResult="Abort"
                 CallBackMode-PostData="Page" Width="800px" Height="450px" TabIndex="2000"  >
    <px:PXFormView ID="formTransferOrder" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="PlanTransferOrderFilter" SkinID="Transparent" Width="100%" SyncPosition="True">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edTransferOrderType" runat="server" DataField="OrderType" />
            <px:PXMaskEdit CommitChanges="True" ID="edTransferOrderNbr" runat="server" DataField="OrderNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
            <px:PXSegmentMask CommitChanges="True" ID="edFromSiteID" runat="server" DataField="FromSiteID"/>
        </Template>
    </px:PXFormView>
    <px:PXGrid ID="gridTransferDisplay" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire">
        <Levels>
            <px:PXGridLevel DataMember="ProcessRecords">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXSegmentMask ID="edTransferInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXSegmentMask ID="edTransferSubitem" runat="server" DataField="SubItemID" />
                    <px:PXDropDown ID="edTransferSource" runat="server" DataField="Source" />
                    <px:PXSegmentMask ID="edTransferSiteID" runat="server" DataField="SiteID" />
                    <px:PXNumberEdit ID="edTransferQty" runat="server" DataField="Qty" CommitChanges="True" />
                    <px:PXTextEdit ID="edTransferUOM" runat="server" DataField="UOM"/>
                    <px:PXDateTimeEdit ID="edTransferPlanDate" runat="server" DataField="PlanDate"/>
                    <px:PXNumberEdit ID="edTransferGroupNumber" runat="server" DataField="GroupNumber" CommitChanges="True"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="SubItemID" Width="70px" />
                    <px:PXGridColumn DataField="Source" Width="120px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="Qty" Width="130px" TextAlign="Right" CommitChanges="True" />
                    <px:PXGridColumn DataField="UOM" Width="60px"  />
                    <px:PXGridColumn DataField="PlanDate" Width="80px" />
                    <px:PXGridColumn DataField="GroupNumber" Width="65px" TextAlign="Right" CommitChanges="True" />
                </Columns>
                <Mode AllowAddNew="false" AllowUpdate="true" AllowDelete="false" />
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Parent" Enabled="True"/>
        <ActionBar ActionsText="False">
        </ActionBar>
    </px:PXGrid>
    <px:PXPanel ID="PXPanelTransferBtn" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButtonTransferOk" runat="server" DialogResult="OK" Text="Create"></px:PXButton>
        <px:PXButton ID="PXButtonTransferCancel" runat="server" DialogResult="Abort" Text="Cancel"></px:PXButton>
    </px:PXPanel>
</px:PXSmartPanel>
</asp:Content>
