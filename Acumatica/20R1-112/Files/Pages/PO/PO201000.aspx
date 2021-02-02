<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO201000.aspx.cs"
    Inherits="Page_PO201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="BAccount" TypeName="PX.Objects.PO.POVendorCatalogueMaint"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Visible="false" Name="ShowVendorPrices" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXSmartPanel ID="pnlUpdatePrice" runat="server" Key="VendorCatalogue" CaptionVisible="true" DesignView="Content" Caption="Update Effective Vendor Prices"
        AllowResize="false">
        <px:PXFormView ID="formEffectiveDate" runat="server" DataSourceID="ds" CaptionVisible="false" DataMember="VendorInventory$UpdatePrice"
            Width="280px" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXDateTimeEdit ID="edPendingDate" runat="server" DataField="PendingDate" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Update" />
            <px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="BAccount" Caption="Vendor Inventory Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edBAccountID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="true" />
            <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true" />
            <px:PXSegmentMask ID="edVSiteID" runat="server" DataField="VSiteID" Enabled="False" />
            <px:PXNumberEdit ID="edVLeadTime" runat="server" DataField="VLeadTime" Enabled="False" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" AllowPaging="true" Style="z-index: 100" Width="100%" SkinID="Details" Caption="Vendor Inventory Details" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="VendorCatalogue">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXLabel Size="xs" ID="lblVendorUOM" runat="server">Vendor UOM :</px:PXLabel>
                    <px:PXMaskEdit Size="xm" ID="edVendorInventoryID" runat="server" DataField="VendorInventoryID" />
                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="true" AllowEdit="True" />
                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" Merge="False" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowAddNew="True" AllowEdit="True">
                        <GridProperties>
                            <PagerSettings Mode="NextPrevFirstLast" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="grid" Name="POVendorCatalogue.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Active" Width="60px" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="IsDefault" Width="60px" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="AllLocations" Width="80px" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="InventoryID" Width="104px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="InventoryItem__Descr" Width="200px" />
                    <px:PXGridColumn DataField="SubItemID" Width="85px" />
                    <px:PXGridColumn DataField="PurchaseUnit" Width="85px" />
                    <px:PXGridColumn DataField="VendorInventoryID" Width="120px" />
                    <px:PXGridColumn AllowNull="False" DataField="OverrideSettings" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="true" />
                    <px:PXGridColumn AllowNull="False" DataField="AddLeadTimeDays" TextAlign="Right" />
                    <px:PXGridColumn DataField="MinOrdFreq" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="MinOrdQty" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="MaxOrdQty" Width="100px" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="LotSize" Width="100px" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="ERQ" Width="100px" AllowNull="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" Label="Currency ID" MaxLength="5" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="LastPrice" TextAlign="Right" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Mode InitNewRow="True" AllowUpload="true" />
        <AutoSize Enabled="True" MinHeight="150" Container="Window" />
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton Key="cmdShowVendorPrices" CommandSourceID="ds" CommandName="ShowVendorPrices" />
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
