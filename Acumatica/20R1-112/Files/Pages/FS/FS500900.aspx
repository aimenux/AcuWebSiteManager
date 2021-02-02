<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS500900.aspx.cs" 
    Inherits="Page_FS500900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FS.ConvertItemsToEquipmentProcess"
    SuspendUnloading="False">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="gridInventoryItems" Name="OpenInvoice" Visible="false"></px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="80px" DataMember="Filter" TabIndex="3700">
        <Template>
            <px:PXLayoutRule runat="Server" StartRow="True" LabelsWidth="SM" ControlSize="M"></px:PXLayoutRule>
            <px:PXSelector ID="edFilterItemClassID" runat="server" DataField="ItemClassID" CommitChanges="True">
            </px:PXSelector>
            <px:PXDateTimeEdit ID="edFilterDate" runat="Server" DataField="Date" CommitChanges="True"></px:PXDateTimeEdit>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="gridInventoryItems" runat="server" AllowPaging="True" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px"
        SkinID="Inquire" TabIndex="500" SyncPosition="True" BatchUpdate="True" KeepPosition="True" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="InventoryItems">
			    <RowTemplate>
                    <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" Text="Selected">
                    </px:PXCheckBox>
                    <px:PXSegmentMask ID="edInventoryCD" runat="server" DataField="InventoryCD" AllowEdit="True">
                    </px:PXSegmentMask>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"></px:PXTextEdit>
                    <px:PXTextEdit ID="edLotSerialNumber" runat="server" DataField="LotSerialNumber"></px:PXTextEdit>
                    <px:PXSelector ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXSelector ID="edInvoiceRefNbr" runat="server" DataField="InvoiceRefNbr" AllowEdit="True">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate"></px:PXDateTimeEdit>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AllowEdit="True">
                    </px:PXSegmentMask>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="InventoryCD">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Descr">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="LotSerialNumber">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ItemClassID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="InvoiceRefNbr" LinkCommand="OpenInvoice">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DocDate">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerLocationID">
                    </px:PXGridColumn>
                </Columns>
		    </px:PXGridLevel>
        </Levels>
        <Mode AllowAddNew="False" AllowDelete="False" />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>