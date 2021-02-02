<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR202000.aspx.cs"
    Inherits="Page_AR202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARSalesPriceMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="CreateWorksheet" CommitChanges="true" Visible="true" />
		</CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="ddPriceType" AllowCollapse="false">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDropDown runat="server" ID="ddPriceType" DataField="PriceType" AllowNull="false" CommitChanges="true" />
			<px:PXSelector CommitChanges="True" ID="edPriceCode" runat="server" DataField="PriceCode" AutoRefresh="true" DisplayMode="Hint">
				<GridProperties  FastFilterFields="PriceCode,Description" />
			</px:PXSelector>
			<px:PXDateTimeEdit runat="server" CommitChanges="true" ID="edEffectiveAsOfDate" DataField="EffectiveAsOfDate" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassCD" />
			<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" />
			<px:PXSelector CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edInventoryPriceClassID" runat="server" DataField="InventoryPriceClassID"/>
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edPriceManagerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyUser" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="False" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" Caption="Sales Prices"
        SkinID="Details" FilterShortCuts="True" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="true" MarkRequired="Dynamic">
        <Levels>
            <px:PXGridLevel DataMember="Records">
				<RowTemplate>
					<px:PXSelector runat="server" ID="edPriceCode" DataField="PriceCode" AutoRefresh="true" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
					<px:PXNumberEdit runat="server" ID="edBreakQty" DataField="BreakQty" />
					<px:PXNumberEdit runat="server" ID="edSalesPrice" DataField="SalesPrice" />
				</RowTemplate>
                <Columns>
					<px:PXGridColumn DataField="PriceType" Type="DropDownList" CommitChanges="true"  />
					<px:PXGridColumn DataField="PriceCode" CommitChanges="true"   />
                    <px:PXGridColumn DataField="AlternateID" CommitChanges="true"/>
                    <px:PXGridColumn DataField="InventoryID" CommitChanges="true"/>
					<px:PXGridColumn DataField="Description" CommitChanges="true"/>
                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" CommitChanges="true" />
					<px:PXGridColumn DataField="SiteID" CommitChanges="true" />
					<px:PXGridColumn DataField="IsPromotionalPrice" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
					<px:PXGridColumn DataField="IsFairValue" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
					<px:PXGridColumn DataField="IsProrated" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
					<px:PXGridColumn DataField="BreakQty" TextAlign="Right" />
                    <px:PXGridColumn DataField="SalesPrice" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryID" />
                    <px:PXGridColumn DataField="TaxID" DisplayFormat="&gt;aaaaaaaaaa" />
                    <px:PXGridColumn DataField="EffectiveDate" CommitChanges="true" />
					<px:PXGridColumn DataField="ExpirationDate"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="True" />
    </px:PXGrid>
</asp:Content>

