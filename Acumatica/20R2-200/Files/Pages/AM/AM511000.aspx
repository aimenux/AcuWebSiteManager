<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM511000.aspx.cs"
    Inherits="Page_AM511000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.ProductionOrderPrintProcess" PrimaryView="Filter" >
        <CallbackCommands>
            
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" DataMember="Filter" Width="100%" Caption="Selection"
        DefaultControlID="edCreationDate" EmailingGraph="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
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
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="StartDate" ID="edStartDate" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="EndDate" ID="edEndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edSOOrderType" runat="server" DataField="SOOrderType" />
            <px:PXSelector CommitChanges="True" ID="edSOOrderNbr" runat="server" DataField="SOOrderNbr" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXSelector CommitChanges="True" ID="edProdOrdID" runat="server" DataField="ProdOrdID" />
            <px:PXCheckBox CommitChanges="True" ID="edReprint" runat="server" DataField="Reprint" Checked="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" SyncPosition="True"
        FastFilterFields="InventoryID,InventoryID_InventoryItem_descr">
        <Levels>
            <px:PXGridLevel DataMember="ProductionOrders" DataKeyNames="OrderType, ProdOrdID">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" />
                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataKeyNames="InventoryCD" 
                        DataSourceID="ds" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID"/>
                    <px:PXSegmentMask ID="edSiteId" runat="server" DataField="SiteId" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXNumberEdit ID="edQtytoProd" runat="server" DataField="QtytoProd"  />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM"/>
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" AllowNull="False" DataField="StartDate" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate"/>
                    <px:PXDateTimeEdit ID="edProdDate" runat="server" DataField="ProdDate"/>
                    <px:PXTextEdit ID="edOrdTypeRef" runat="server" DataField="OrdTypeRef"  />
                    <px:PXSelector ID="edOrdNbr" runat="server" DataField="OrdNbr" AllowEdit="True" edit="1"/>
                    <px:PXTextEdit ID="edInventoryID_InventoryItem_descr" runat="server" DataField="InventoryID_InventoryItem_descr"/>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True"/>
                    <px:PXTextEdit ID="edCustomerID_Customer_acctName" runat="server" DataField="CustomerID_Customer_acctName"/>
                    <px:PXDropDown ID="edDetailSource" runat="server" DataField="DetailSource" />
                    <px:PXDropDown ID="edStatusID" runat="server" DataField="StatusID" />
                    <px:PXTextEdit ID="edProductionReportID" runat="server" DataField="ProductionReportID" ValueField="ScreenID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="125px"/>
                    <px:PXGridColumn DataField="InventoryID" Width="125px" />
                    <px:PXGridColumn DataField="SubItemID" Width="120px"  />
                    <px:PXGridColumn DataField="SiteId" Width="125px" />
                    <px:PXGridColumn DataField="QtytoProd" Width="117px"/>
                    <px:PXGridColumn DataField="UOM" />
                    <px:PXGridColumn DataField="StartDate" Width="90px"/>
                    <px:PXGridColumn DataField="EndDate" Width="90px"/>
                    <px:PXGridColumn DataField="ProdDate" Width="90px"/>
                    <px:PXGridColumn DataField="OrdTypeRef"/>
                    <px:PXGridColumn DataField="OrdNbr"/>
                    <px:PXGridColumn DataField="InventoryID_InventoryItem_descr" Width="200px"/>
                    <px:PXGridColumn DataField="CustomerID" Width="120px"/>
                    <px:PXGridColumn DataField="CustomerID_Customer_acctName" Width="200px"/>
                    <px:PXGridColumn DataField="DetailSource" TextAlign="Left"/>
                    <px:PXGridColumn DataField="StatusID" TextAlign="Left"/>
                    <px:PXGridColumn DataField="ProjectID" Width="90px"/>
                    <px:PXGridColumn DataField="TaskID" Width="90px"/>
                    <px:PXGridColumn DataField="CostCodeID" Width="90px"/>
                    <px:PXGridColumn DataField="ProductionReportID" />
                </Columns>
			</px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
