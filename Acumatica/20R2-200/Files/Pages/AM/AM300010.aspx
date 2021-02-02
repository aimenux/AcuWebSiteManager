<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM300010.aspx.cs" Inherits="Page_AM300010" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="OpenOrders" TypeName="PX.Objects.AM.MatlWizard1">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="filter">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Options" />
            <px:PXCheckBox ID="edExcludeUnreleasedBatchQty" runat="server" DataField="ExcludeUnreleasedBatchQty" AlignLeft="True"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" TabIndex="300" TemporaryFilterCaption="Filter Applied" >
		<Levels>
			<px:PXGridLevel DataKeyNames="ProdOrdID" DataMember="OpenOrders" >
			    <RowTemplate>
			        <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True"/>
                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True"/>
			        <px:PXNumberEdit ID="edQtytoProd" runat="server" DataField="QtytoProd"/>
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM"/>
                    <px:PXNumberEdit ID="edQtyComplete" runat="server" DataField="QtyComplete"/>
			        <px:PXNumberEdit ID="edQtyRemaining" runat="server" DataField="QtyRemaining"/>
                    <px:PXDateTimeEdit ID="edProdDate" runat="server" DataField="ProdDate"/>
			        <px:PXDropDown ID="edStatusID" runat="server" DataField="StatusID"/>
			        <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate"/>
			        <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate"/>
			        <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID"/>
			        <px:PXTextEdit ID="edOrdTypeRef" runat="server" DataField="OrdTypeRef"/>
			        <px:PXSelector ID="edOrdNbr" runat="server" DataField="OrdNbr"/>
			        <px:PXSelector ID="edParentOrderType" runat="server" DataField="ParentOrderType" AllowEdit="True"/>
			        <px:PXSelector ID="edParentOrdID" runat="server" DataField="ParentOrdID" AllowEdit="True"/>
			        <px:PXSelector ID="edProductOrderType" runat="server" DataField="ProductOrderType" AllowEdit="True"/>
			        <px:PXSelector ID="edProductOrdID" runat="server" DataField="ProductOrdID" AllowEdit="True"/>
                    <px:PXTextEdit ID="edInventoryID_description" runat="server" DataField="InventoryID_description" Enabled="False"/>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="70px"/>
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="130px" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px"/>
                    <px:PXGridColumn DataField="SubItemID" Width="81px"/>
                    <px:PXGridColumn DataField="SiteID" Width="130px"/>
                    <px:PXGridColumn DataField="QtytoProd" CommitChanges="True" TextAlign="Right" Width="108px"/>
                    <px:PXGridColumn DataField="UOM"/>
			        <px:PXGridColumn DataField="QtyComplete" TextAlign="Right" Width="100px"/>
			        <px:PXGridColumn DataField="QtyRemaining" Width="100px" TextAlign="Right"/>
			        <px:PXGridColumn DataField="ProdDate" Width="90px"/>
			        <px:PXGridColumn DataField="StatusID"/>
			        <px:PXGridColumn DataField="StartDate" Width="90px"/>
			        <px:PXGridColumn DataField="EndDate" Width="90px"/>
			        <px:PXGridColumn DataField="CustomerID" Width="120px"/>
			        <px:PXGridColumn DataField="OrdTypeRef"/>
			        <px:PXGridColumn DataField="OrdNbr"/>
			        <px:PXGridColumn DataField="ParentOrderType"/>
                    <px:PXGridColumn DataField="ParentOrdID"/>
			        <px:PXGridColumn DataField="ProductOrderType"/>
			        <px:PXGridColumn DataField="ProductOrdID"/>
			        <px:PXGridColumn DataField="InventoryID_description" Width="200px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="True">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
