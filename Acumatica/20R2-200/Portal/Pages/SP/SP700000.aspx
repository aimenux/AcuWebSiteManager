<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP700000.aspx.cs" Inherits="Pages_SP700000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%"
        PrimaryView="Filter" TypeName="SP.Objects.IN.InventoryLineMaint" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="gridInventoryItem" Name="AddToCart" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="gridInventoryItem" Name="OpenCart" />
            <px:PXDSCallbackCommand DependOnGrid="gridInventoryItem" Name="RefreshHeader" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="gridInventoryItem" Name="ShowPicture" Visible="False" PopupCommand="RefreshHeader" PopupCommandTarget="ds" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeKeys="CategoryID" TreeView="Categories" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="ItemsInfo" Width="100%">
        <Template>
            <div style="padding: 5px; position: static; border-style: none;">
                <table id="Table1" runat="server" style="position: static; margin-left: 9px; width: 100%">
                    <tr>
                        <td>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XXL" />
                            <px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="Info" SuppressLabel="True" Enabled="False" Size="XXL" />
                        </td>
                    </tr>
                </table>
            </div>
        </Template>
    </px:PXFormView>

    <px:PXFormView ID="form1" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="L" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
            <px:PXTextEdit ID="edFindItem" runat="server" DataField="FindItem" CommitChanges="true" />
            <px:PXNumberEdit ID="edSelectionTotal" runat="server" DataField="SelectionTotal" Size="XS" TextAlign="Right" Enabled="False" />
            <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="CurrencyStatus" Size="XS" Enabled="False" SuppressLabel="true" />
            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="False" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
            <px:PXTreeSelector ID="edParent" runat="server" DataField="CategoryID" PopulateOnDemand="True"
                ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="Categories" CommitChanges="true">
                <DataBindings>
                    <px:PXTreeItemBinding TextField="Description" ValueField="CategoryID" />
                </DataBindings>
            </px:PXTreeSelector>
            <px:PXCheckBox ID="edIsShowAvailableItem" runat="server" DataField="IsShowAvailableItem" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="gridInventoryItem" runat="server" DataSourceID="ds"
        Width="100%" SkinID="Details" AdjustPageSize="Auto"
        FilesIndicator="False" NoteIndicator="False" FastFilterID="edFindItem"
        FastFilterFields="InventoryCD,Descr" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="FilteredItems">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                    <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True">
                        <Parameters>
                            <px:PXControlParam ControlID="gridInventoryItem" Name="InventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXNumberEdit ID="edUnitPrice" runat="server" DataField="UnitPrice" />
                    <px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" />
                    <px:PXNumberEdit ID="edBaseDiscountPct" runat="server" DataField="BaseDiscountPct" />
                    <px:PXNumberEdit ID="edBaseDiscountAmt" runat="server" DataField="BaseDiscountAmt" />
                    <px:PXNumberEdit ID="edTotalPrice" runat="server" DataField="TotalPrice" />
                    <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True" SelectOnly="True" />
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" Width="45px" Type="CheckBox" TextAlign="Center" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Qty" Width="45px" CommitChanges="True" TextAlign="Right" />
                    <px:PXGridColumn DataField="InventoryCD" Width="120px" LinkCommand="ShowPicture" />
                    <px:PXGridColumn DataField="Descr" Width="250px" />
                    <px:PXGridColumn AllowNull="False" DataField="CuryUnitPrice" TextAlign="Right" Width="70px" />
                    <px:PXGridColumn DataField="UOM" Width="50px" AutoCallBack="True" />
                    <px:PXGridColumn AllowNull="False" DataField="BaseDiscountPct" TextAlign="Right" Width="80px" />
                    <px:PXGridColumn AllowNull="False" DataField="BaseDiscountAmt" TextAlign="Right" Width="80px" />
                    <px:PXGridColumn DataField="TotalPrice" Width="80px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" CommitChanges="True" />
                    <px:PXGridColumn AllowNull="False" DataField="CurrentWarehouse" Width="95px" />
                    <px:PXGridColumn AllowNull="False" DataField="TotalWarehouse" Width="95px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" Container="Window" />
        <Mode AllowAddNew="False" AllowDelete="False" AutoInsert="False" />

        <ActionBar DefaultAction="cmdItemDetails">
            <Actions>
                <ExportExcel Enabled="False" />
                <AddNew Enabled="False" />
                <FilterShow Enabled="False" />
                <FilterSet Enabled="False" />
                <Delete Enabled="False" />
                <NoteShow Enabled="False" />
                <Search Enabled="False" />
                <AdjustColumns Enabled="False" />
            </Actions>
            <CustomItems>
                <px:PXToolBarButton>
                    <AutoCallBack Command="AddToCart" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="cmdItemDetails" Visible="false">
                    <AutoCallBack Command="ShowPicture" Target="ds" />

                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>

