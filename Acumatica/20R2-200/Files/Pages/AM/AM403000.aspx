<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM403000.aspx.cs" Inherits="Page_AM403000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="ExceptRecs" TypeName="PX.Objects.AM.MRPExcept" Visible="True" BorderStyle="NotSet" >
		<CallbackCommands>
		    <px:PXDSCallbackCommand Visible="false" Name="AMRPExceptions$RefNoteID$Link" DependOnGrid="grid" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" TabIndex="100">
		<Levels>
			<px:PXGridLevel DataMember="ExceptRecs">
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" Width="120px"/>
                    <px:PXGridColumn DataField="SubItemID" Width="120px"/>
                    <px:PXGridColumn DataField="InventoryID_InventoryItem_descr" Width="200px"/>
                    <px:PXGridColumn DataField="Type" Width="125px"/>
                    <px:PXGridColumn DataField="RefType" Width="125px"/>
                    <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="AMRPExceptions$RefNoteID$Link" Width="175px" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="RequiredDate" Width="90px"/>
                    <px:PXGridColumn DataField="PromiseDate" Width="90px"/>
                    <px:PXGridColumn DataField="SiteID" Width="120px"/>
                    <px:PXGridColumn DataField="SupplyQty" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="SupplySiteID" Width="120px"/>
                    <px:PXGridColumn DataField="RecordID" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ProductManagerID"/>
                    <px:PXGridColumn DataField="ProductManagerID_EPEmployee_AcctName" Width="200px"/>
                    <px:PXGridColumn DataField="ItemClassID"/>
                </Columns>
                <RowTemplate>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXTextEdit ID="edInventoryID_InventoryItem_descr" runat="server" DataField="InventoryID_InventoryItem_descr"/>
                    <px:PXDropDown ID="edType" runat="server" DataField="Type"/>
                    <px:PXDropDown ID="edRefType" runat="server" DataField="RefType"/>
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty"/>
                    <px:PXDateTimeEdit ID="edPromiseDate" runat="server" DataField="PromiseDate"/>
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edSupplyQty" runat="server" DataField="SupplyQty"/>
                    <px:PXSegmentMask ID="edSupplySiteID" runat="server" DataField="SupplySiteID" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edRecordID" runat="server" DataField="RecordID"/>
                    <px:PXDateTimeEdit ID="edRequiredDate" runat="server" DataField="RequiredDate"/>
                    <px:PXTextEdit ID="edProductManagerID_EPEmployee_AcctName" runat="server" DataField="ProductManagerID_EPEmployee_AcctName"/>
                    <px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="ItemClassID" />
                </RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
