<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP202010.aspx.cs"
    Inherits="Page_AP202010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APPriceWorksheetMaint" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ReleasePriceWorksheet" Visible="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="AddItem" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="CopyPrices" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="CalculatePrices" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Worksheet Summary" DataMember="Document"
        DefaultControlID="edRefNbr">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="False" />
            <px:PXDropDown ID="ddStatus" runat="server" DataField="Status" />
            <px:PXCheckBox runat="server" ID="chkHold" DataField="Hold" CommitChanges="true" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Descr" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEffectiveDate" runat="server" DataField="EffectiveDate" Size="S" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsPromotional" runat="server" DataField="IsPromotional" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="False" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edExpirationDate" runat="server" DataField="ExpirationDate" Size="S" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOverwriteOverlapping" runat="server" DataField="OverwriteOverlapping" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" Caption="Sales Prices"
        SkinID="Details" FilterShortCuts="True" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Details">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSelector ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="true" />
                    <px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID"/>
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AutoRefresh="true">
                        <GridProperties>
                            <PagerSettings Mode="NextPrevFirstLast" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXTextEdit ID="teDescription" runat="server" DataField="Description" CommitChanges="true" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="true" />
                    <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXNumberEdit ID="edBreakQty" runat="server" DataField="BreakQty" />
                    <px:PXNumberEdit ID="edCurrentPrice" runat="server" DataField="CurrentPrice" />
                    <px:PXNumberEdit ID="edPendingPrice" runat="server" DataField="PendingPrice" AllowNull="true" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
                    <px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="VendorID" AutoCallBack="True"  />
                    <px:PXGridColumn DataField="AlternateID" CommitChanges="true"/>
                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" CommitChanges="true"/>
                    <px:PXGridColumn AllowUpdate="False" DataField="Description" CommitChanges="true" />
                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" CommitChanges="true" />
                    <px:PXGridColumn DataField="SiteID" AutoCallBack="True"  />
                    <px:PXGridColumn DataField="BreakQty" TextAlign="Right" CommitChanges="true" />
                    <px:PXGridColumn DataField="CurrentPrice" TextAlign="Right" />
                    <px:PXGridColumn DataField="PendingPrice" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" CommitChanges="true" />
                    <px:PXGridColumn DataField="TaxID" DisplayFormat="&gt;aaaaaaaaaa" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False">
            <PagerSettings Mode="NextPrevFirstLast" />
            <CustomItems>
                <px:PXToolBarButton Text="Add Item" Key="cmdADI">
                    <AutoCallBack Command="AddItem" Target="ds">
                        <Behavior CommitChanges="True" PostData="Page" />
                    </AutoCallBack>
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Copy Prices" Key="cmdCopy">
                    <AutoCallBack Command="CopyPrices" Target="ds">
                        <Behavior PostData="Page" />
                    </AutoCallBack>
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Calculate Pending Prices" Key="cmdCalculate">
                    <AutoCallBack Command="CalculatePrices" Target="ds">
                        <Behavior PostData="Page" />
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Mode AllowFormEdit="False" AllowUpload="True" InitNewRow="true" />
    </px:PXGrid>

    <%-- Inventory Lookup --%>
    <px:PXSmartPanel ID="PanelAddItem" runat="server" Key="addItemLookup" LoadOnDemand="true" Width="800px" Height="500px"
        Caption="Add Item to Worksheet" CaptionVisible="true" AutoRepaint="true" DesignView="Content">
        <px:PXFormView ID="formAddItem" runat="server" CaptionVisible="False" DataMember="addItemFilter" DataSourceID="ds"
            Width="100%" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXTextEdit ID="edInventory" runat="server" DataField="Inventory" />
                <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClass" />
                <px:PXSelector CommitChanges="True" ID="edPriceClassID" runat="server" DataField="PriceClassID" />
                <px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="True" />
                <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
                <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
                <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="False" />
                <px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
                <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
                <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
                <px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="False" />
                <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXFormView ID="PanelAddItemParametrs" runat="server" CaptionVisible="False" DataMember="addItemParameters" DataSourceID="ds"
                    Width="100%" SkinID="Transparent" RenderStyle="Simple" Style="margin: 0px">
                    <ContentStyle BackColor="Transparent" BorderStyle="None" />
                    <Template>
                        <px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" GroupCaption="Price Type to Add" />
                        <px:PXSelector ID="edVendorID" runat="server" DataField="VendorID" AutoRefresh="true" CommitChanges="true" />
                        <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
                        <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" CommitChanges="True" />
                    </Template>
                </px:PXFormView>
            </Template>
        </px:PXFormView>

        <px:PXGrid ID="gridAddItemLookup" runat="server" DataSourceID="ds" Style="border-width: 1px 0px; top: 0px; left: 0px; height: 189px;"
            AutoAdjustColumns="true" Width="100%" SkinID="Details" AdjustPageSize="Auto" AllowSearch="True" FastFilterID="edInventory"
            FastFilterFields="InventoryCD,Descr" >
            <CallbackCommands>
                <Refresh CommitChanges="true"></Refresh>
            </CallbackCommands>
            <ClientEvents AfterCellUpdate="UpdateItemSiteCell" />
            <ActionBar PagerVisible="False">
                <PagerSettings Mode="NextPrevFirstLast" />
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="addItemLookup">
                    <Mode AllowAddNew="false" AllowDelete="false" />
					<RowTemplate>
						<px:PXSegmentMask CommitChanges="True" ID="edItemClassCD" runat="server" DataField="ItemClassID" />
					</RowTemplate>
                    <Columns>
                        <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="40px" AutoCallBack="true"
                            AllowCheckAll="true" CommitChanges="true" />
                        <px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="ItemClassID" />
                        <px:PXGridColumn DataField="ItemClassDescription" />
                        <px:PXGridColumn DataField="Descr" />
                        <px:PXGridColumn DataField="PriceClassID" />
                        <px:PXGridColumn DataField="PriceClassDescription" />
                        <px:PXGridColumn DataField="ProductWorkgroupID" />
                        <px:PXGridColumn DataField="ProductManagerID" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" CommandName="AddSelItems" CommandSourceID="ds" Text="Add" SyncVisible="false" />
			<px:PXButton ID="PXButton1" runat="server" CommandName="AddAllItems" CommandSourceID="ds" Text="Add All" SyncVisible="false" />
            <px:PXButton ID="PXButton4" runat="server" Text="Add & Close" DialogResult="OK" />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelCopyPrices" runat="server" CommandSourceID="ds" Caption="Copy Prices" CaptionVisible="True" ShowAfterLoad="true" LoadOnDemand="true"
        DesignView="Content" Key="CopyPricesSettings" AutoCallBack-Enabled="true" AutoCallBack-Target="massCopyForm" AutoCallBack-Command="Refresh">
        <div style="padding: 5px">
            <px:PXFormView ID="massCopyForm" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="CopyPricesSettings">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Source" />
                    <px:PXSelector CommitChanges="True" ID="edSrcVendorID" runat="server" DataField="SourceVendorID" AutoRefresh="true" />
                    <px:PXSelector CommitChanges="True" ID="edSrcCuryID" runat="server" AutoRefresh="True" DataField="SourceCuryID" />
                    <px:PXSelector CommitChanges="True" ID="edSrcSiteID" runat="server" AutoRefresh="True" DataField="SourceSiteID" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
                    <px:PXCheckBox CommitChanges="True" ID="chkIsPromotional" runat="server" DataField="IsPromotional" />
                    <px:PXLayoutRule ID="PXLayoutRule16" runat="server" LabelsWidth="SM" ControlSize="XM" GroupCaption="Destination" />
                    <px:PXSelector CommitChanges="True" ID="edVendorID" runat="server" DataField="DestinationVendorID" AutoRefresh="true" />
                    <px:PXSelector CommitChanges="True" ID="edDestCuryID" runat="server" AutoRefresh="True" DataField="DestinationCuryID" />
                    <px:PXSelector CommitChanges="True" ID="esDestSiteID" runat="server" AutoRefresh="True" DataField="DestinationSiteID" />
                    <px:PXLayoutRule ID="PXLayoutRule17" runat="server" LabelsWidth="SM" ControlSize="XM" GroupCaption="Currency Conversion" />
                    <px:PXSelector CommitChanges="True" ID="edRateTypeID" runat="server" DataField="RateTypeID" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edCurrencyDate" runat="server" DataField="CurrencyDate" />
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnCopy" runat="server" Text="Copy" DialogResult="OK" />
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelCalculatePrices" runat="server" CommandSourceID="ds" Caption="Calculate Pending Prices" CaptionVisible="True" ShowAfterLoad="true" LoadOnDemand="true"
        DesignView="Content" Key="CalculatePricesSettings" AutoCallBack-Enabled="true" AutoCallBack-Target="massUpdateForm" AutoCallBack-Command="Refresh">
        <div style="padding: 5px">
            <px:PXFormView ID="massUpdateForm" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="CalculatePricesSettings">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule18" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Price Adjustment" />
                    <px:PXNumberEdit ID="edCorrectionPercent" runat="server" DataField="CorrectionPercent" />
                    <px:PXNumberEdit ID="edRounding" runat="server" DataField="Rounding" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkSkip" runat="server" Checked="True" DataField="UpdateOnZero" />
                    <px:PXGroupBox RenderStyle="Fieldset" ID="PriceBasisGroupBox" runat="server" DataField="PriceBasis" Caption="Price Basis">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule19" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXRadioButton ID="rdbLastCost" runat="server" Text="Last Cost + Markup %" Value="L" />
                            <px:PXRadioButton ID="rdbStdCost" runat="server" Text="Avg/Std. Cost + Markup %" Value="S" />
                            <px:PXRadioButton ID="rdbRecPrice" runat="server" Text="Recommended Price" Value="R" />
                            <px:PXRadioButton ID="rdbCurrentPrice" runat="server" Text="Source Price" Value="P" Checked="True" />
                            <px:PXRadioButton ID="rdbPendingPrice" runat="server" Text="Pending Price" Value="N" />
                        </Template>
                    </px:PXGroupBox>
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave2" runat="server" DialogResult="OK" Text="Update">
                <AutoCallBack Command="Save" Target="massUpdateForm"/>
            </px:PXButton>
            <px:PXButton ID="btnCancel2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
