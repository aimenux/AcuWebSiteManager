<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO509000.aspx.cs"
    Inherits="Page_SO509000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOCreate" PrimaryView="Filter"/>
    
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" DataMember="Filter" Width="100%" Caption="Selection"
        DefaultControlID="edPurchDate">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
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
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True">
                <GridProperties>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edSourceSiteID" runat="server" DataField="SourceSiteID" />
			<px:PXSegmentMask CommitChanges="True" runat="server" DataField="SiteID" ID="edSiteID" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="true" />
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXNumberEdit ID="edOrderWeight" runat="server" DataField="OrderWeight" Enabled="False" />
            <px:PXNumberEdit ID="edOrderVolume" runat="server" DataField="OrderVolume" Enabled="False" />

        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Details"
        SyncPosition="True" FastFilterFields="InventoryID,InventoryID_InventoryItem_descr,SOOrder__OrderNbr">
        <Levels>
            <px:PXGridLevel DataMember="FixedDemand">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edDemandSiteID" runat="server" DataField="DemandSiteID" />
                    <px:PXSegmentMask ID="edSourceSiteID" runat="server" DataField="SourceSiteID" />
                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID">
                        <Parameters>
                            <px:PXControlParam ControlID="grid" Name="SOFixedDemand.vendorID" PropertyName="DataValues[&quot;VendorID&quot;]" Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXDateTimeEdit ID="edPlanDate" runat="server" DataField="PlanDate" Enabled="False" />
                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" Enabled="False" />
					<px:PXNumberEdit ID="edExtVolume" runat="server" DataField="ExtVolume" Enabled="false" />
					<px:PXNumberEdit ID="edExtWeight" runat="server" DataField="ExtWeight" Enabled="false" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanType_INPlanType_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID" DisplayFormat="&gt;AAA-&gt;CCC-&gt;AA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID_InventoryItem_descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SubItemID" DisplayFormat="&gt;AA-A" />
					<px:PXGridColumn AllowUpdate="False" DataField="SourceSiteID" DisplayFormat="&gt;AAAAAAAAAA" Label="Replenishment Warehouse" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DemandSiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="UOM" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderQty" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanDate" />
					<px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID" />
					<px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID_BAccountR_acctName" />
					<px:PXGridColumn AllowUpdate="False" DataField="SOOrder__OrderNbr" />
					<px:PXGridColumn AllowUpdate="False" DataField="ExtWeight" /> 
					<px:PXGridColumn AllowUpdate="False" DataField="ExtVolume" /> 
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <ActionBar DefaultAction="viewDocument"/>
           
        
    </px:PXGrid>
</asp:Content>
