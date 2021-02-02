<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" 
	CodeFile="AP202000.aspx.cs" Inherits="Pages_AP_AP202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AP.APVendorPriceMaint">
		<CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edCustPriceClassID" AllowCollapse="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edItemClassCD" runat="server" DataField="ItemClassCD" />
			<px:PXDateTimeEdit runat="server" CommitChanges="true" ID="edEffectiveAsOfDate" DataField="EffectiveAsOfDate" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
			<px:PXSelector CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edPriceManagerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyUser" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="False" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" Caption="Sales Prices"
        SkinID="Details" FilterShortCuts="True" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Records">
				<RowTemplate>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
					<px:PXNumberEdit runat="server" DataField="BreakQty" ID="edBreakQty" />
					<px:PXNumberEdit runat="server" DataField="SalesPrice" ID="edSalesPrice" />
				</RowTemplate>
                <Columns>
					<px:PXGridColumn DataField="VendorID" Width="90px" CommitChanges="true" />
					<px:PXGridColumn DataField="VendorID_Vendor_AcctName" Width="90px" />
					<px:PXGridColumn DataField="AlternateID" Width="130px" CommitChanges="true"/>
					<px:PXGridColumn DataField="InventoryID" Width="108px" CommitChanges="true"/>
					<px:PXGridColumn DataField="InventoryID_InventoryItem_Descr" Width="130px" />
					<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
					<px:PXGridColumn DataField="SiteID" Width="90px" CommitChanges="true" />
					<px:PXGridColumn DataField="IsPromotionalPrice" Width="90px" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
					<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right"/>
					<px:PXGridColumn DataField="SalesPrice" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryID" Width="81px" />
					<px:PXGridColumn AutoCallBack="true" DataField="EffectiveDate" Width="90px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="ExpirationDate" Width="90px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False">
            <PagerSettings Mode="NextPrevFirstLast" />
        </ActionBar>
		<Mode InitNewRow="true" />
	</px:PXGrid>
</asp:Content>