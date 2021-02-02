<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP700001.aspx.cs" Inherits="Pages_SP700001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="SP.Objects.IN.InventoryCardMaint" 
        PrimaryView="Document" PageLoadBehavior="GoFirstRecord" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="gridDocumentDetails" Name="DeleteRow" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="gridDocumentDetails" Name="ShowPicture" Visible="False" PopupCommand="Cancel" PopupCommandTarget="ds"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont33" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Document" Caption="Selection" Width="100%">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XS" LabelsWidth="SM" />
            <px:PXNumberEdit ID="edItemTotal" runat="server" DataField="ItemTotal" TextAlign="Right" Enabled="False"/>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" ControlSize="XS" LabelsWidth="XS"/>
            <px:PXNumberEdit ID="edTotalPrice" runat="server" DataField="AllTotalPrice" TextAlign="Right" Enabled="False"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" ControlSize="L" LabelsWidth="XS"/>
            <px:PXTextEdit ID="edCurrency" runat="server" DataField="CurrencyStatus" Size="XS" Enabled="False"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="gridDocumentDetails" runat="server" DataSourceID="ds"
        Width="100%" SkinID="Details" AdjustPageSize="Auto" AllowSearch="True"
        FilesIndicator="False" NoteIndicator="False" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="DocumentDetails">
                <RowTemplate>
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />      
                    <px:PXNumberEdit ID="edBaseDiscountPct" runat="server" DataField="BaseDiscountPct" />
                    <px:PXNumberEdit ID="edBaseDiscountAmt" runat="server" DataField="BaseDiscountAmt" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryIDDescription" Width="100px" LinkCommand="ShowPicture"/>
                    <px:PXGridColumn DataField="Descr" Width="160px" />
                    <px:PXGridColumn DataField="Qty" Width="60px" AutoCallback ="true" TextAlign="Right"/>
                    <px:PXGridColumn DataField="CuryUnitPrice" Width="100px" />
                    <px:PXGridColumn DataField="UOM" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="BaseDiscountPct" TextAlign="Right" Width="80px" />
                    <px:PXGridColumn AllowNull="False" DataField="BaseDiscountAmt" TextAlign="Right" Width="80px" />
                    <px:PXGridColumn DataField="TotalPrice" Width="100px" />
                    <px:PXGridColumn DataField="SiteID" Width="100px" />
                    <px:PXGridColumn DataField="CurrentWarehouse" Width="100px" />
                    <px:PXGridColumn DataField="TotalWarehouse" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <CallbackCommands>
            <Refresh CommitChanges="true" />
        </CallbackCommands>
        <Mode AllowAddNew="False" AutoInsert="False" />
        <ActionBar>
            <Actions>
                <ExportExcel Enabled="False" />
                <AddNew Enabled="False" />
                <FilterShow Enabled="False" />
                <FilterSet Enabled="False" />
                <NoteShow Enabled="False" />
                <Search Enabled="False" />
                <Delete Enabled="False" />
                <Refresh Enabled="False" />
                <AdjustColumns Enabled="False" />
            </Actions>
            <CustomItems>
                <px:PXToolBarButton CommandSourceID="ds" CommandName="DeleteRow" ImageKey="Remove">
                    <ActionBar GroupIndex="0"/>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>