<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM510000.aspx.cs"
    Inherits="Page_AM510000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.CreateProductionOrdersProcess" PrimaryView="Filter" >
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="InventorySummary" Visible="False" />
            <px:PXDSCallbackCommand Name="InventoryAllocationDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="viewDocument" Visible="False" DependOnGrid="grid" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" DataMember="Filter" Width="100%" Caption="Selection"
        DefaultControlID="edCreationDate" EmailingGraph="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edCreationOrderType" runat="server" DataField="CreationOrderType" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="CreationDate" ID="edCreationDate" />
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
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="RequestedOnStartDate" ID="edRequestedOnStartDate" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="RequestedOnEndDate" ID="edRequestedOnEndDate" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edSOOrderType" runat="server" DataField="SOOrderType" />
            <px:PXSelector CommitChanges="True" ID="edSOOrderNbr" runat="server" DataField="SOOrderNbr" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXSelector CommitChanges="True" ID="edProdOrdID" runat="server" DataField="ProdOrdID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" AllowPaging="True" 
               AllowSearch="true" BatchUpdate="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="FixedDemand">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edDemandInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edDemandSiteId" runat="server" DataField="SiteId" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edDemandPlanDate" runat="server" DataField="PlanDate" Enabled="False" />
                    <px:PXSegmentMask ID="edDemandCustomerID" runat="server" DataField="SOOrder__CustomerID" AllowEdit="true" />
                    <px:PXSelector ID="edDemandSOOrderType" runat="server" DataField="SOOrderType" AllowEdit="true" />
                    <px:PXSelector ID="edDemandSOOrderNbr" runat="server" DataField="SOOrderNbr" AllowEdit="true" />
                    <px:PXSelector ID="edDemandAMOrderType" runat="server" DataField="AMOrderType" AllowEdit="true" />
                    <px:PXSelector ID="edDemandAMProdOrdID" runat="server" DataField="AMProdOrdID" AllowEdit="true" />
                    <px:PXTextEdit ID="edDemandAMOperationID" runat="server" DataField="AMOperationID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" Width="30px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanType_INPlanType_descr" Width="165px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID" Width="115px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID_InventoryItem_descr" Width="125px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SubItemID" Width="45px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SiteID" Width="100px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="UOM" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderQty" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID" Width="115px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID_BAccountR_acctName" Width="135px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerLocationID" Width="80px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrderType" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrderNbr" LinkCommand="viewDocument" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AMOrderType" Width="70" />
	                <px:PXGridColumn AllowUpdate="False" DataField="AMProdOrdID" Width="120" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AMOperationID" Width="90" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>