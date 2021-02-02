<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN203000.aspx.cs" Inherits="Page_IN203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.Matrix.Graphs.TemplateInventoryItemMaint" PrimaryView="Item">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Inquiry" />
            <px:PXDSCallbackCommand Name="AddWarehouseDetail" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="UpdateReplenishment" Visible="false" CommitChanges="true" DependOnGrid="repGrid" />
            <px:PXDSCallbackCommand Name="GenerateSubitems" Visible="false" CommitChanges="true" DependOnGrid="repGrid" />
            <px:PXDSCallbackCommand Name="ViewGroupDetails" Visible="False" DependOnGrid="grid3" />
            <px:PXDSCallbackCommand Name="syncSalesforce" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="PXGridIdGenerationRules" Name="IdRowUp" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="PXGridIdGenerationRules" Name="IdRowDown" Visible="False" />	
            <px:PXDSCallbackCommand DependOnGrid="PXGridDescriptionGenerationRules" Name="DescriptionRowUp" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="PXGridDescriptionGenerationRules" Name="DescriptionRowDown" Visible="False" />	
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="DeleteItems" DependOnGrid="grdMatrixItems" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="false" Name="ViewMatrixItem" DependOnGrid="grdMatrixItems" />
            <px:PXDSCallbackCommand Name="CreateMatrixItems" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CreateUpdate" CommitChanges="true" Visible="false" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
            <px:PXTreeDataMember TreeView="EntityItems" TreeKeys="Key" />
            <px:PXTreeDataMember TreeKeys="CategoryID" TreeView="Categories" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
        <px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="ChangeIDDialog">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
                <AutoCallBack Target="formChangeID" Command="Save" />
            </px:PXButton>
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Item" Caption="Stock Item Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
        ActivityField="NoteActivity" DefaultControlID="edInventoryCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edInventoryCD" runat="server" DataField="InventoryCD" DataSourceID="ds" AutoRefresh="true" >
                <GridProperties FastFilterFields="InventoryCD,Descr" />
			</px:PXSegmentMask>

			<px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXCheckBox ID="edStkItem" runat="server" DataField="StkItem" CommitChanges="true" Size="XXL" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="606px" DataSourceID="ds" DataMember="ItemSettings" FilesIndicator="False" NoteIndicator="False">
        <AutoSize Enabled="True" Container="Window" MinHeight="150" />
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Item Defaults" />
					<px:PXDropDown ID="edItemStatus" runat="server" DataField="ItemStatus" />
                    <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" AutoRefresh="true" />
                    <px:PXDropDown ID="edItemType" runat="server" DataField="ItemType" />
                    <px:PXDropDown CommitChanges="True" ID="edValMethod" runat="server" DataField="ValMethod" />
                    <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" CommitChanges="True" AutoRefresh="True" />
                    <px:PXSelector CommitChanges="True" ID="edPostClassID" runat="server" DataField="PostClassID" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edLotSerClassID" runat="server" DataField="LotSerClassID" AllowEdit="True" />
                    <px:PXSelector runat="server" ID="edCountryOfOrigin" DataField="CountryOfOrigin" />
                    <px:PXCheckBox ID="chkNonStockReceipt" runat="server" Checked="True" DataField="NonStockReceipt" CommitChanges="true" />
                    <px:PXCheckBox ID="chkNonStockShip" runat="server" Checked="True" DataField="NonStockShip" />
                    <px:PXDropDown ID="edCompletePOLine" runat="server" DataField="CompletePOLine" />

                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Field Service Defaults" />
                    <px:PXMaskEdit runat="server" ID="edEstimatedDuration" DataField="EstimatedDuration" />
                    <px:PXCheckBox runat="server" ID="edRouteService" DataField="ItemClass.Mem_RouteService" Enabled="False" />

                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Warehouse Defaults" />
                    <px:PXSegmentMask CommitChanges="True" ID="edDfltSiteID" runat="server" DataField="DfltSiteID" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edDfltShipLocationID" runat="server" DataField="DfltShipLocationID" AutoRefresh="True" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edDfltReceiptLocationID" runat="server" DataField="DfltReceiptLocationID" AutoRefresh="True" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSegmentMask Size="s" ID="edDefaultSubItemID" runat="server" DataField="DefaultSubItemID" AutoRefresh="True" />
                    <px:PXCheckBox ID="chkDefaultSubItemOnEntry" runat="server" DataField="DefaultSubItemOnEntry" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" GroupCaption="Unit of Measure" StartGroup="True" />
					<px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXSelector ID="edBaseUnit" Size="s" runat="server" AllowEdit="True" CommitChanges="True" DataField="BaseUnit" Style="margin-right:30px"/>
					<px:PXCheckBox ID="chkDecimalBaseUnit" runat="server" DataField="DecimalBaseUnit" CommitChanges="True"/>
					<px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXSelector ID="edSalesUnit" Size="s" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="SalesUnit" Style="margin-right:30px"/>
					<px:PXCheckBox ID="chkDecimalSalesUnit" runat="server" DataField="DecimalSalesUnit" CommitChanges="True" />
					<px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXSelector ID="edPurchaseUnit" Size="s" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="PurchaseUnit" Style="margin-right:30px"/>
					<px:PXCheckBox ID="chkDecimalPurchaseUnit" runat="server" DataField="DecimalPurchaseUnit" CommitChanges="True" />
					<px:PXLayoutRule runat="server"/>
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" SuppressLabel="True" />
                    <px:PXGrid ID="gridUnits" runat="server" DataSourceID="ds" SkinID="ShortList" Width="400px" Height="114px" Caption="Conversions" CaptionVisible="false">
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="itemunits">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edItemClassID2" runat="server" DataField="ItemClassID" />
                                    <px:PXNumberEdit ID="edInventoryID" runat="server" DataField="InventoryID" />
                                    <px:PXMaskEdit ID="edFromUnit" runat="server" DataField="FromUnit" />
                                    <px:PXMaskEdit ID="edSampleToUnit" runat="server" DataField="SampleToUnit" />
                                    <px:PXNumberEdit ID="edUnitRate" runat="server" DataField="UnitRate" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="UnitType" Type="DropDownList" Width="99px" Visible="False" />
                                    <px:PXGridColumn DataField="ItemClassID" Width="36px" Visible="False" />
                                    <px:PXGridColumn DataField="InventoryID" Visible="False" TextAlign="Right" Width="54px" />
                                    <px:PXGridColumn DataField="FromUnit" Width="72px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="UnitMultDiv" Type="DropDownList" Width="90px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="UnitRate" TextAlign="Right" Width="108px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SampleToUnit" Width="72px" />
                                    <px:PXGridColumn DataField="PriceAdjustmentMultiplier" TextAlign="Right" Width="108px" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Layout ColumnsMenu="False" />
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Physical Inventory" />
                    <px:PXSelector CommitChanges="True" ID="edPICycleID" runat="server" DataField="CycleID" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edABCCodeID" runat="server" DataField="ABCCodeID" AllowEdit="True" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkABCCodeIsFixed" runat="server" DataField="ABCCodeIsFixed" />
                    <px:PXSelector CommitChanges="True" ID="edMovementClassID" runat="server" DataField="MovementClassID" AllowEdit="True" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkMovementClassIsFixed" runat="server" DataField="MovementClassIsFixed" />
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Fulfillment">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Dimensions" />
                    <px:PXNumberEdit ID="edBaseItemWeight" runat="server" DataField="BaseItemWeight" />
                    <px:PXSelector ID="edWeightUOM" runat="server" DataField="WeightUOM" Size="S" AutoRefresh="true" />
                    <px:PXNumberEdit ID="edBaseItemVolume" runat="server" DataField="BaseItemVolume" />
                    <px:PXSelector ID="edVolumeUOM" runat="server" DataField="VolumeUOM" Size="S" AutoRefresh="true" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="International Shipping" />
                    <px:PXTextEdit runat="server" ID="edHSTariffCode" DataField="HSTariffCode" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping Thresholds" />
					<px:PXNumberEdit ID="edUndershipThreshold" runat="server" DataField="UndershipThreshold" />
					<px:PXNumberEdit ID="edOvershipThreshold" runat="server" DataField="OvershipThreshold" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Sales Categories" />
					<px:PXGrid ID="PXGridCategory" runat="server" DataSourceID="ds" Height="220px" Width="250px"
                        SkinID="ShortList" MatrixMode="False">
                        <Levels>
                            <px:PXGridLevel DataMember="Category">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXTreeSelector ID="edParent" runat="server" DataField="CategoryID" PopulateOnDemand="True"
                                        ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="Categories" CommitChanges="true">
                                        <DataBindings>
                                            <px:PXTreeItemBinding TextField="Description" ValueField="CategoryID" />
                                        </DataBindings>
                                    </px:PXTreeSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="CategoryID" Width="220px" TextField="INCategory__Description" AllowResize="False"/>
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Automatic Packaging" />
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" />
                    <px:PXDropDown ID="edPackageOption" runat="server" DataField="PackageOption" CommitChanges="true" AllowNull="False" />
                    <px:PXCheckBox ID="edPackSeparately" DataField="PackSeparately" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="False" />
                    <px:PXGrid ID="PXGridBoxes" runat="server" Caption="Boxes" DataSourceID="ds" Height="130px" Width="420px" SkinID="ShortList" FilesIndicator="False" NoteIndicator="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Boxes">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                                    <px:PXSelector ID="edBoxID" runat="server" DataField="BoxID" />
                                    <px:PXSelector ID="edUOM_box" runat="server" DataField="UOM" />
                                    <px:PXNumberEdit ID="edQty_box" runat="server" DataField="Qty" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="BoxID" Width="91px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Description" Width="91px" />
                                    <px:PXGridColumn DataField="UOM" Width="54px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="54px" />
                                    <px:PXGridColumn DataField="MaxWeight" Width="54px" />
                                    <px:PXGridColumn DataField="MaxVolume" Width="54px" />
                                    <px:PXGridColumn DataField="MaxQty" TextAlign="Right" Width="54px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Price/Cost Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" ControlSize="XM" GroupCaption="Price Management" />
                    <px:PXSelector ID="edPriceClassID" runat="server" DataField="PriceClassID" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edPriceWorkgroupID" runat="server" DataField="PriceWorkgroupID" ShowRootNode="False" />
                    <px:PXSelector ID="edPriceManagerID" runat="server" DataField="PriceManagerID" AutoRefresh="True" CommitChanges="True"/>
                    <px:PXCheckBox SuppressLabel="True" ID="chkCommisionable" runat="server" DataField="Commisionable" />
                    <px:PXNumberEdit ID="edMinGrossProfitPct" runat="server" DataField="MinGrossProfitPct" />
                    <px:PXNumberEdit ID="edMarkupPct" runat="server" DataField="MarkupPct" />
                    <px:PXNumberEdit ID="edRecPrice" runat="server" DataField="RecPrice" />
                    <px:PXNumberEdit ID="edBasePrice" runat="server" DataField="BasePrice" Enabled="True" />
                    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" StartGroup="True" GroupCaption="Standard Cost" />
                    <px:PXNumberEdit ID="edPendingStdCost" runat="server" DataField="PendingStdCost" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edPendingStdCostDate" runat="server" DataField="PendingStdCostDate" />
                    <px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost" Enabled="False" />
                    <px:PXDateTimeEdit ID="edStdCostDate" runat="server" DataField="StdCostDate" Enabled="False" />
                    <px:PXNumberEdit ID="edLastStdCost" runat="server" DataField="LastStdCost" Enabled="False" />
					<px:PXLayoutRule runat="server" ID="PXLayoutRuleA1" StartGroup="true" GroupCaption="Cost Accrual" ControlSize="XM" />
					<px:PXCheckBox ID="chkAccrueCost" runat="server" DataField="AccrueCost" CommitChanges="True" />
                    <px:PXDropDown ID="edCostBasis" runat="server" DataField="CostBasis" CommitChanges="true" />
					<px:PXNumberEdit ID="edPercentOfSalesPrice" runat="server" DataField="PercentOfSalesPrice" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Field Service Defaults" />
                    <px:PXSelector runat="server" ID="edDfltEarningType" DataField="DfltEarningType" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Vendor Details" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="PXGridVendorItems" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab" SyncPosition="true">
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="VendorItems" DataKeyNames="RecordID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="True" />
                                    <px:PXSegmentMask Size="xxs" ID="vp_edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edLocation__VSiteID" runat="server" DataField="Location__VSiteID" AllowEdit="true" />
                                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="True" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edAddLeadTimeDays" runat="server" DataField="AddLeadTimeDays" />
                                    <px:PXCheckBox ID="vp_chkActive" runat="server" Checked="True" DataField="Active" />
                                    <px:PXNumberEdit ID="edMinOrdFreq" runat="server" DataField="MinOrdFreq" />
                                    <px:PXNumberEdit ID="edMinOrdQty" runat="server" DataField="MinOrdQty" />
                                    <px:PXNumberEdit ID="edMaxOrdQty" runat="server" DataField="MaxOrdQty" />
                                    <px:PXNumberEdit ID="edLotSize" runat="server" DataField="LotSize" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edERQ" runat="server" DataField="ERQ" />
                                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
                                    <px:PXNumberEdit ID="edLastPrice" runat="server" DataField="LastPrice" Enabled="False" />
                                    <px:PXCheckBox ID="chkIsDefault" runat="server" DataField="IsDefault" />
                                    <px:PXTextEdit ID="edVendor__AcctName" runat="server" DataField="Vendor__AcctName" />
                                    <px:PXNumberEdit ID="edLocation__VLeadTime" runat="server" DataField="Location__VLeadTime" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="45px" />
                                    <px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" Width="45px" />
                                    <px:PXGridColumn DataField="VendorID" Width="81px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Vendor__AcctName" Width="210px" />
                                    <px:PXGridColumn DataField="VendorLocationID" Width="54px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Location__VSiteID" Width="81px" />
                                    <px:PXGridColumn DataField="SubItemID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="PurchaseUnit" Width="63px" />
                                    <px:PXGridColumn DataField="Location__VLeadTime" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OverrideSettings" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="AddLeadTimeDays" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="MinOrdFreq" TextAlign="Right" Width="84px" />
                                    <px:PXGridColumn DataField="MinOrdQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="MaxOrdQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="LotSize" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="ERQ" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryID" Width="54px" />
                                    <px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="99px" />
                                    <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" AllowNull="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" CommitChanges="true" AutoRefresh="true" />
					<px:PXSegmentMask ID="edNonStockInvtAcctID" runat="server" DataField="ExpenseAccrualAcctID" CommitChanges="true" AutoRefresh="true" />
                    <px:PXSegmentMask ID="edInvtSubID" runat="server" DataField="InvtSubID" AutoRefresh="True" CommitChanges="True" />
					<px:PXSegmentMask ID="edNonStockInvtSubID" runat="server" DataField="ExpenseAccrualSubID" AutoRefresh="True" CommitChanges="True" />
                    <px:PXSegmentMask ID="edReasonCodeSubID" runat="server" DataField="ReasonCodeSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edCOGSAcctID" runat="server" DataField="COGSAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edNonStockCOGSAcctID" runat="server" DataField="ExpenseAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edCOGSSubID" runat="server" DataField="COGSSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edNonStockCOGSSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edStdCstVarAcctID" runat="server" DataField="StdCstVarAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edStdCstVarSubID" runat="server" DataField="StdCstVarSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edStdCstRevAcctID" runat="server" DataField="StdCstRevAcctID" AutoRefresh="True" CommitChanges="true" />
                    <px:PXSegmentMask ID="edStdCstRevSubID" runat="server" DataField="StdCstRevSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edPPVAcctID" runat="server" DataField="PPVAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPPVSubID" runat="server" DataField="PPVSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edLCVarianceAcctID" runat="server" DataField="LCVarianceAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edLCVarianceSubID" runat="server" DataField="LCVarianceSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edDeferralAcctID" runat="server" DataField="DeferralAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edDeferralSubID" runat="server" DataField="DeferralSubID" AutoRefresh="True" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Description" LoadOnDemand="true" >
                <Template>
                    <px:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Style="border-width: 0px; border-top-width: 1px; width: 100%;"
                        AllowAttached="true" AllowSearch="true" AllowLoadTemplate="false" AllowSourceMode="true">
                        <AutoSize Enabled="True" MinHeight="216" />
                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
                    </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Attribute Configuration">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XM" GroupCaption="Attributes" />
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Height="150px" MatrixMode="True" Width="420px" SkinID="Attributes">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AttributeID,EntityType,EntityID" DataMember="Answers">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                                    <px:PXTextEdit ID="edParameterID" runat="server" DataField="AttributeID" Enabled="False" />
                                    <px:PXTextEdit ID="edAnswerValue" runat="server" DataField="Value" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowShowHide="False" DataField="AttributeID" TextField="AttributeID_description" TextAlign="Left" Width="135px" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="80px" />
									<px:PXGridColumn DataField="AttributeCategory" Type="DropDownList" />
                                    <px:PXGridColumn DataField="Value" Width="185px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
					
					<px:PXSelector ID="edDefaultColumnMatrixAttributeID" runat="server" DataField="DefaultColumnMatrixAttributeID" AllowEdit="True" CommitChanges="True" AutoRefresh="true" />
					<px:PXSelector ID="edDefaultRowMatrixAttributeID" runat="server" DataField="DefaultRowMatrixAttributeID" AllowEdit="True" CommitChanges="True" AutoRefresh="true" />

					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XM" GroupCaption="Inventory ID Segment Settings" />
                    <px:PXGrid ID="PXGridIdGenerationRules" runat="server" DataSourceID="ds" Height="150px" Width="750px" SkinID="Details" StatusField="Sample">
						<ActionBar>
                            <CustomItems>                               
								<px:PXToolBarButton CommandName="IdRowUp" CommandSourceID="ds">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="IdRowDown" CommandSourceID="ds">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AttributeID" DataMember="IdGenerationRules">
                                <RowTemplate>
                                </RowTemplate>
                                <Columns>
									<px:PXGridColumn DataField="SegmentType" Type="DropDownList" CommitChanges="true" />
									<px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="135px" CommitChanges="true" />
									<px:PXGridColumn DataField="Constant" Width="100px" CommitChanges="true" />
									<px:PXGridColumn DataField="NumberingID" CommitChanges="true" />
									<px:PXGridColumn DataField="NumberOfCharacters" CommitChanges="true" />
                                    <px:PXGridColumn DataField="UseSpaceAsSeparator" TextAlign="Center" Type="CheckBox" Width="80px" CommitChanges="true" />
									<px:PXGridColumn DataField="Separator" Width="80px" CommitChanges="true" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>

					<px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XM" GroupCaption="Description Segment Settings" />
                    <px:PXGrid ID="PXGridDescriptionGenerationRules" runat="server" DataSourceID="ds" Height="150px" Width="750px" SkinID="Details" StatusField="Sample">
						<ActionBar>
                            <CustomItems>                               
								<px:PXToolBarButton CommandName="DescriptionRowUp" CommandSourceID="ds">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="DescriptionRowDown" CommandSourceID="ds">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AttributeID" DataMember="DescriptionGenerationRules">
                                <RowTemplate>
                                </RowTemplate>
                                <Columns>
									<px:PXGridColumn DataField="SegmentType" Type="DropDownList" CommitChanges="true" />
									<px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="135px" CommitChanges="true" />
									<px:PXGridColumn DataField="Constant" Width="100px" CommitChanges="true" />
									<px:PXGridColumn DataField="NumberingID" CommitChanges="true" />
									<px:PXGridColumn DataField="NumberOfCharacters" CommitChanges="true" />
                                    <px:PXGridColumn DataField="UseSpaceAsSeparator" TextAlign="Center" Type="CheckBox" Width="80px" CommitChanges="true" />
									<px:PXGridColumn DataField="Separator" Width="80px" CommitChanges="true" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>

                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Create Matrix Items" LoadOnDemand="true">
				<Template>
					<px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Header">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True">
							</px:PXLayoutRule>
							<px:PXSelector runat="server" DataField="ColAttributeID" Size="M" CommitChanges="True" ID="edColAttributeID" AutoRefresh="True">
							</px:PXSelector>
							<px:PXSelector runat="server" DataField="RowAttributeID" Size="M" CommitChanges="True" ID="edRowAttributeID" AutoRefresh="True">
							</px:PXSelector>
						</Template>
					</px:PXFormView>
					<!--#include file="~\Pages\Includes\InventoryMatrixCreateItems.inc"-->
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Matrix Items" LoadOnDemand="true">
                <Template>
					<px:PXGrid ID="grdMatrixItems" runat="server" DataSourceID="ds" SkinID="DetailsInTab" SyncPosition="True" RepaintColumns="True"
						Width="100%" Height="100%" OnAfterSyncState="MatrixItems_AfterSyncState">
						<Levels>
							<px:PXGridLevel DataMember="MatrixItems">
								<RowTemplate>
									<px:PXSegmentMask ID="matrixItemsInventoryID" runat="server" DataField="InventoryID" />
									<px:PXSegmentMask ID="matrixItemsDfltSiteID" runat="server" DataField="DfltSiteID" AllowEdit="True" />
									<px:PXSegmentMask ID="matrixItemsItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" />
									<px:PXSegmentMask ID="matrixItemsTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" />
                                </RowTemplate>
                                <Columns>
									<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="true" />
									<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" LinkCommand="ViewMatrixItem" />
									<px:PXGridColumn DataField="Descr" />
									<px:PXGridColumn DataField="DfltSiteID" />
									<px:PXGridColumn DataField="AttributeValue0" />
									<px:PXGridColumn DataField="ItemClassID" />
									<px:PXGridColumn DataField="TaxCategoryID" />
									<px:PXGridColumn DataField="RecPrice" />
									<px:PXGridColumn DataField="LastStdCost" />
									<px:PXGridColumn DataField="BasePrice" />
									<px:PXGridColumn DataField="StkItem" Type="CheckBox" />
                                </Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" />
						<AutoSize Enabled="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Delete" Key="cmdDelete" CommandName="DeleteItems" CommandSourceID="ds" DependOnGrid="grdMatrixItems">
                                    <AutoCallBack>
                                        <Behavior CommitChanges="True" PostData="Page" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" MinHeight="150" />
    </px:PXTab>
    <px:PXSmartPanel ID="pnlUpdatePrice" runat="server" Key="VendorItems" CaptionVisible="true" DesignView="Content" Caption="Update Effective Vendor Prices" AllowResize="false">
        <px:PXFormView ID="formEffectiveDate" runat="server" DataSourceID="ds" CaptionVisible="false" DataMember="VendorInventory$UpdatePrice" Width="280px" Height="50px" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXDateTimeEdit ID="edPendingDate" runat="server" DataField="PendingDate" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Update" />
            <px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
