<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM404000.aspx.cs" Inherits="Page_AM404000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.MRPDetail" PrimaryView="invlookup" BorderStyle="NotSet" >
		<CallbackCommands>
            <px:PXDSCallbackCommand Visible="false" Name="AMRPPlan$RefNoteID$Link" DependOnGrid="grid" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="invlookup">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" DisplayMode="Hint"/>
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True"/>
            <px:PXSelector CommitChanges="True"  AutoRefresh="true" ID="edSiteID"  runat="server" DataField="SiteID" DisplayMode="Hint" AllowEdit="True" />
            <px:PXNumberEdit ID="edQtyOnHand" runat="server" DataField="QtyOnHand"/>
            <px:PXTextEdit ID="UOM" runat="server" DataField="UOM"/>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />
            <px:PXNumberEdit ID="edSafetyStock" runat="server" DataField="SafetyStock" Width="200px" />
            <px:PXNumberEdit ID="edMinOrderQty" runat="server" DataField="MinOrderQty" Width="200px" />
            <px:PXNumberEdit ID="edMaxOrderQty" runat="server" DataField="MaxOrderQty" Width="200px" />
            <px:PXNumberEdit ID="edLotQty" runat="server" DataField="LotQty" Width="200px" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" SyncPosition="True" TabIndex="300" >
		<Levels>
			<px:PXGridLevel DataMember="MRPRecs" >
                <RowTemplate>
                    <px:PXDropDown ID="edType" runat="server" DataField="Type"/>
                    <px:PXDateTimeEdit ID="edPromiseDate" runat="server" DataField="PromiseDate" />
                    <px:PXNumberEdit ID="edBaseQty" runat="server" DataField="BaseQty" />
                    <px:PXNumberEdit ID="edRTQtyOnHand" runat="server" DataField="QtyOnHand" />
                    <px:PXSegmentMask ID="edParentInventoryID" runat="server" DataField="ParentInventoryID"/>
                    <px:PXSegmentMask ID="edParentSubItemID" runat="server" DataField="ParentSubItemID" />
                    <px:PXSegmentMask ID="edProductInventoryID" runat="server" DataField="ProductInventoryID"/>
                    <px:PXSegmentMask ID="edProductSubItemID" runat="server" DataField="ProductSubItemID" />
                    <px:PXDropDown ID="edRefType" runat="server" DataField="RefType"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Type" Width="125px" />
                    <px:PXGridColumn DataField="PromiseDate" Width="90px" />
                    <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="AMRPPlan$RefNoteID$Link" Width="175px" />
                    <px:PXGridColumn DataField="BaseQty" TextAlign="Right"  Width="125px" />
                    <px:PXGridColumn DataField="QtyOnHand" TextAlign="Right" Width="125px" />
                    <px:PXGridColumn DataField="ParentInventoryID" Width="120px"/>
                    <px:PXGridColumn DataField="ParentSubItemID" Width="120px"/>
                    <px:PXGridColumn DataField="ProductInventoryID" Width="120px"/>
                    <px:PXGridColumn DataField="ProductSubItemID" Width="120px"/>
                    <px:PXGridColumn DataField="RefType" Width="125px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="True">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
