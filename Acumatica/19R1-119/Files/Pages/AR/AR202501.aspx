<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR202501.aspx.cs"
    Inherits="Pages_AR202501" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARSalesPriceMaint">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="ddPriceType">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDropDown runat="server" ID="ddPriceType" DataField="PriceType" AllowNull="false" CommitChanges="true" />
			<px:PXSelector CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True" />
            <px:PXSelector CommitChanges="True" ID="edCustPriceClassID" runat="server" DataField="CustPriceClassID" AllowEdit="True" />
			<px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
			<px:PXDateTimeEdit runat="server" CommitChanges="true" ID="edMinDate" DataField="MinDate" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edItemClassCD" runat="server" DataField="ItemClassCD" />
            <px:PXSelector CommitChanges="True" ID="edInventoryPriceClassID" runat="server" DataField="InventoryPriceClassID"/>
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
<asp:Content ID="Content1" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" Caption="Sales Prices"
        SkinID="Details" FilterShortCuts="True" AdjustPageSize="Auto" AllowPaging="True" >
        <Levels>
            <px:PXGridLevel DataMember="Records">
				<RowTemplate>
					<px:PXNumberEdit runat="server" ID="edBreakQty" DataField="BreakQty" />
					<px:PXNumberEdit runat="server" ID="edSalesPrice" DataField="SalesPrice" />
				</RowTemplate>
                <Columns>
					<px:PXGridColumn DataField="PriceType" Width="150px" Type="DropDownList" CommitChanges="true"  />
					<px:PXGridColumn DataField="CustPriceClassID" Width="108px"  />
					<px:PXGridColumn DataField="CustomerID" Width="108px"  CommitChanges="true" />
                    <px:PXGridColumn DataField="InventoryID" Width="108px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="InventoryItem__Descr" Width="130px" />
                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
					<px:PXGridColumn DataField="IsPromotionalPrice" Width="90px" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
					<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right" />
                    <px:PXGridColumn DataField="SalesPrice" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryID" Width="90px" />
                    <px:PXGridColumn DataField="TaxID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" />
                    <px:PXGridColumn DataField="EffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="ExpirationDate" Width="90px" CommitChanges="true"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="True" />
    </px:PXGrid>
</asp:Content>
