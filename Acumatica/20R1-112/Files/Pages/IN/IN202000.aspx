<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN202000.aspx.cs" Inherits="Page_IN202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" TypeName="PX.Objects.IN.NonStockItemMaint" PrimaryView="Item">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Inquiry" />
            <px:PXDSCallbackCommand Name="ViewOnServicesScreen" Visible="False" />
            <px:PXDSCallbackCommand Name="ConvertToService" Visible="False" />
            <px:PXDSCallbackCommand Name="syncSalesforce" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewRelatedItem" Visible="false" DependOnGrid="relatedItemsGrid" CommitChanges="true" />
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
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Item" Caption="Non-Stock Item Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
        ActivityField="NoteActivity" DefaultControlID="edInventoryCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edInventoryCD" runat="server" DataField="InventoryCD" DataSourceID="ds" AutoRefresh="true"> 
                <GridProperties FastFilterFields="InventoryCD,Descr" />
		</px:PXSegmentMask>
            <px:PXDropDown ID="edItemStatus" runat="server" DataField="ItemStatus" Size="S" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edProductWorkgroupID" runat="server" DataField="ProductWorkgroupID">
            </px:PXSelector>
            <px:PXSelector ID="edProductManagerID" runat="server" DataField="ProductManagerID" AutoRefresh="True" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" ControlSize="M" StartGroup="True" GroupCaption="Service Management" LabelsWidth="SM" StartRow="True" />
            <px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="487px" DataSourceID="ds" DataMember="ItemSettings" MarkRequired="Dynamic">
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSelector ID="edTemplateItemID" runat="server" DataField="TemplateItemID" AllowEdit="True" Enabled="false" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Item Defaults" />
                    <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" AutoRefresh="True" />
                    <px:PXDropDown ID="edItemType" runat="server" DataField="ItemType" CommitChanges="True" />
                    <px:PXSelector ID="edPostClassID" runat="server" DataField="PostClassID" AllowEdit="True" AutoRefresh="True" CommitChanges="True" />
                    <px:PXCheckBox ID="chkKitItem" runat="server" DataField="KitItem" CommitChanges="true" />
                    <px:PXCheckBox runat="server" ID="edIsTravelItem" DataField="IsTravelItem" CommitChanges="True" />
                    <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" AutoRefresh="True" />
					<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" />
                    <px:PXSegmentMask ID="edDfltSiteID" runat="server" DataField="DfltSiteID" />
                    <px:PXCheckBox ID="chkNonStockReceipt" runat="server" Checked="True" DataField="NonStockReceipt" CommitChanges="true" />
                    <px:PXCheckBox ID="chkNonStockShip" runat="server" Checked="True" DataField="NonStockShip" />
                    <px:PXDropDown ID="edCompletePOLine" runat="server" DataField="CompletePOLine" />

                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Field Service Defaults" />
                    <px:PXMaskEdit runat="server" ID="edEstimatedDuration" DataField="EstimatedDuration" />
                    <px:PXCheckBox runat="server" ID="edRouteService" DataField="ItemClass.Mem_RouteService" Enabled="False" />
                    
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Unit of Measure" />
					<px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXSelector CommitChanges="True" ID="edBaseUnit" runat="server" DataField="BaseUnit" Size="S" AllowEdit="true" Style="margin-right:30px"/>
					<px:PXCheckBox ID="chkDecimalBaseUnit" runat="server" DataField="DecimalBaseUnit" CommitChanges="True" />
					<px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXSelector CommitChanges="True" ID="edSalesUnit" runat="server" DataField="SalesUnit" AutoRefresh="True" Size="S" AllowEdit="true" Style="margin-right:30px"/>
					<px:PXCheckBox ID="chkDecimalSalesUnit" runat="server" DataField="DecimalSalesUnit" CommitChanges="True" />
					<px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXSelector CommitChanges="True" ID="edPurchaseUnit" runat="server" DataField="PurchaseUnit" AutoRefresh="True" Size="S" AllowEdit="true" Style="margin-right:30px"/>
					<px:PXCheckBox ID="chkDecimalPurchaseUnit" runat="server" DataField="DecimalPurchaseUnit" CommitChanges="True" />
					<px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XM" LabelsWidth="SM" />
                    <px:PXGrid ID="gridUnits" runat="server" DataSourceID="ds" Height="140px" Width="400px" SkinID="ShortList" Caption="Conversions" CaptionVisible="false">
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
                                    <px:PXGridColumn DataField="UnitMultDiv" Type="DropDownList" Width="90px" />
                                    <px:PXGridColumn DataField="UnitRate" TextAlign="Right" Width="108px" />
                                    <px:PXGridColumn DataField="SampleToUnit" Width="72px" />
                                    <px:PXGridColumn DataField="PriceAdjustmentMultiplier" TextAlign="Right" Width="108px" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Layout ColumnsMenu="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Price/Cost Information">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="true" ControlSize="XM" GroupCaption="Price Management" />
                    <px:PXSelector ID="edPriceClassID" runat="server" DataField="PriceClassID" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edPriceWorkgroupID" runat="server" DataField="PriceWorkgroupID" />
                    <px:PXSelector ID="edPriceManagerID" runat="server" DataField="PriceManagerID" AutoRefresh="True" />
                    <px:PXCheckBox ID="chkCommisionable" runat="server" DataField="Commisionable" />
                    <px:PXNumberEdit ID="edMinGrossProfitPct" runat="server" DataField="MinGrossProfitPct" />
                    <px:PXNumberEdit ID="edMarkupPct" runat="server" DataField="MarkupPct" />
                    <px:PXNumberEdit ID="edRecPrice" runat="server" DataField="RecPrice" />
                    <px:PXNumberEdit ID="edBasePrice" runat="server" DataField="BasePrice" Enabled="true" />
                   
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="true" StartGroup="True" GroupCaption="Standard Cost" />
                    <px:PXNumberEdit ID="edPendingStdCost" runat="server" DataField="PendingStdCost" />
                    <px:PXDateTimeEdit ID="edPendingStdCostDate" runat="server" DataField="PendingStdCostDate" />
                    <px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost" Enabled="False" />
                    <px:PXDateTimeEdit ID="edStdCostDate" runat="server" DataField="StdCostDate" Enabled="False" />
                    <px:PXNumberEdit ID="edLastStdCost" runat="server" DataField="LastStdCost" Enabled="False" />
					<px:PXLayoutRule runat="server" ID="PXLayoutRuleA1" StartGroup="true" GroupCaption="Cost Accrual" ControlSize="XM" />
					<px:PXCheckBox ID="chkAccrueCost" runat="server" DataField="AccrueCost" CommitChanges="True" />
                    <px:PXDropDown ID="edCostBasis" runat="server" DataField="CostBasis" CommitChanges="true" />
					<px:PXNumberEdit ID="edPercentOfSalesPrice" runat="server" DataField="PercentOfSalesPrice" />
                     <px:PXLayoutRule runat="server" ID="PXLayoutRuleC1" StartGroup="true" GroupCaption="RUT and RUT Settings" ControlSize="XM" />
                    <px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" ID="chkIsRUTROTDeductible" AlignLeft="True" CommitChanges="true" />
                    <px:PXGroupBox runat="server" DataField="RUTROTType" CommitChanges="True" RenderStyle="Simple" ID="gbRRType">
				    <ContentLayout Layout="Stack" Orientation="Horizontal" />
				    <Template>
					    <px:PXRadioButton runat="server" Value="O" ID="gbRRType_opO" GroupName="gbRRType" Text="ROT"  />
					    <px:PXRadioButton runat="server" Value="U" ID="gbRRType_opU" GroupName="gbRRType" Text="RUT" />
				    </Template>
                    </px:PXGroupBox>
                    <px:PXDropDown runat="server" DataField="RUTROTItemType" ID="cmbRUTROTItemType" CommitChanges="true" />
                    <px:PXSelector runat="server" DataField="RUTROTWorkTypeID" ID="cmbRUTROTWorkType" CommitChanges="true" AutoRefresh="true" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Field Service Defaults" />
                    <px:PXSelector runat="server" ID="edDfltEarningType" DataField="DfltEarningType" />
                    <px:PXDropDown runat="server" ID="edBillingRule" DataField="BillingRule" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Vendor Details" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="PXGridVendorItems" runat="server" DataSourceID="ds" Height="100%" Width="100%" BorderWidth="0px" SkinID="Details" SyncPosition="true">
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="VendorItems">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXCheckBox ID="vp_chkActive" runat="server" Checked="True" DataField="Active" />
                                    <px:PXCheckBox ID="IsDefault" runat="server" DataField="IsDefault" Text="Default" />
                                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" CommitChanges="True" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="True" />
                                    <px:PXMaskEdit ID="edVendorInventoryID" runat="server" DataField="VendorInventoryID" />
                                    <px:PXNumberEdit ID="edLastPrice" runat="server" DataField="LastPrice" Enabled="False" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="45px" />
                                    <px:PXGridColumn DataField="IsDefault" Width="60px" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="VendorID" Width="81px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Vendor__AcctName" Width="210px" />
                                    <px:PXGridColumn DataField="VendorLocationID" Width="54px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="PurchaseUnit" Width="63px" />
                                    <px:PXGridColumn DataField="VendorInventoryID" Width="90px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CuryID" Width="54px" />
                                    <px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="99px" />
                                    <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" AllowNull="True" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
             <px:PXTabItem Text="Cross-Reference">
                <Template>
                    <px:PXGrid ID="crossgrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="itemxrefrecords" DataKeyNames="InventoryID,SubItemID,AlternateType,BAccountID,AlternateID">
                                <Columns>
                                    <px:PXGridColumn DataField="AlternateType" Type="DropDownList" Width="135px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="BAccountID" Width="135px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="AlternateID" Width="180px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="UOM" Width="70px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="Descr" Width="351px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edBAccountID" runat="server" DataField="BAccountID" AutoRefresh="True" AllowEdit="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="crossgrid" Name="INItemXRef.alternateType" PropertyName="DataValues[&quot;AlternateType&quot;]" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edxUOM" runat="server" Size="s" DataField="UOM" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXSegmentMask SuppressLabel="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" />
                                    <px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID"/>
                                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                                </RowTemplate>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="true" />
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

			<!--#include file="~\Pages\Includes\RelatedItemsTab.inc"-->

            <px:PXTabItem Text="Packaging">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Dimensions" />
                    <px:PXNumberEdit ID="edBaseItemWeight" runat="server" DataField="BaseItemWeight" />
                    <px:PXSelector ID="edWeightUOM" runat="server" DataField="WeightUOM" />
                    <px:PXNumberEdit ID="edBaseItemVolume" runat="server" DataField="BaseItemVolume" />
                    <px:PXSelector ID="edVolumeUOM" runat="server" DataField="VolumeUOM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping Thresholds" />
					<px:PXNumberEdit ID="edUndershipThreshold" runat="server" DataField="UndershipThreshold" />
					<px:PXNumberEdit ID="edOvershipThreshold" runat="server" DataField="OvershipThreshold" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Deferral Settings">
                <Template>
                    <px:PXFormView ID="formDR" runat="server" Width="100%" DataMember="ItemSettings" DataSourceID="ds" Caption="Rules" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector CommitChanges="True" ID="edDeferredCode1" runat="server" DataField="DeferredCode" AllowEdit="True" DataSourceID="ds" />
							<px:PXLayoutRule runat="server" Merge="true" />
							<px:PXNumberEdit runat="server" ID="edDefaultTerm" DataField="DefaultTerm" CommitChanges="true" />
							<px:PXDropDown runat="server" ID="edDefaultTermUOM" DataField="DefaultTermUOM" CommitChanges="true" Width="134px" SuppressLabel="true" />
							<px:PXLayoutRule runat="server" />
                            <px:PXCheckBox ID="chkUseParentSubID" runat="server" DataField="UseParentSubID" />
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXNumberEdit ID="edTotalPercentage" runat="server" DataField="TotalPercentage" Enabled="false" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="PXGridComponents" runat="server" DataSourceID="ds" AllowFilter="False" Height="200px" Width="100%" Caption="Revenue Components" SkinID="DetailsWithFilter" SyncPosition="true">
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Components">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXDropDown ID="edPriceOption" runat="server" DataField="AmtOption" CommitChanges="true" />
                                    <px:PXSegmentMask ID="edComponentID" runat="server" DataField="ComponentID" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edFixedAmt" runat="server" DataField="FixedAmt" />
                                    <px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" CommitChanges="true" AllowEdit="True" />
									<px:PXNumberEdit runat="server" ID="edDefaultTerm" DataField="DefaultTerm" CommitChanges="true" />
                                    <px:PXNumberEdit ID="edPercentage" runat="server" DataField="Percentage" />
                                    <px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" CommitChanges="true" />
                                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="true" />
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ComponentID" Width="99px" />
                                    <px:PXGridColumn DataField="SalesAcctID" Width="99px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="SalesSubID" Width="99px" />
                                    <px:PXGridColumn DataField="UOM" Width="99px" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="99px" />
                                    <px:PXGridColumn DataField="DeferredCode" Width="99px" CommitChanges="true" />
									<px:PXGridColumn DataField="DefaultTerm" />
									<px:PXGridColumn DataField="DefaultTermUOM" />
                                    <px:PXGridColumn DataField="AmtOption" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="FixedAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="Percentage" TextAlign="Right" Width="99px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" CommitChanges="True" />
                    <px:PXSegmentMask ID="edInvtSubID" runat="server" DataField="InvtSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edReasonCodeSubID" runat="server" DataField="ReasonCodeSubID" AutoRefresh="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edExpenseAccountID" runat="server" DataField="COGSAcctID" />
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="COGSSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" AutoRefresh="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" />
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edPPVAcctID" runat="server" DataField="PPVAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPPVSubID" runat="server" DataField="PPVSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edDeferralAcctID" runat="server" DataField="DeferralAcctID" CommitChanges="true" />		   
                    <px:PXSegmentMask ID="edDeferralSubID" runat="server" DataField="DeferralSubID" AutoRefresh="True" />
					<px:PXLayoutRule runat="server" GroupCaption="Payroll Settings" />
					<px:PXSegmentMask ID="edEarningsAcctID" runat="server" DataField="EarningsAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edEarningsSubID" runat="server" DataField="EarningsSubID" AutoRefresh="true" />
					<px:PXSegmentMask ID="edBenefitExpenseAcctID" runat="server" DataField="BenefitExpenseAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edBenefitExpenseSubID" runat="server" DataField="BenefitExpenseSubID" AutoRefresh="true" />
					<px:PXSegmentMask ID="edTaxExpenseAcctID" runat="server" DataField="TaxExpenseAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edTaxExpenseSubID" runat="server" DataField="TaxExpenseSubID" AutoRefresh="true" />
                </Template>
            </px:PXTabItem>


            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXGrid ID="PXGridAnswers" runat="server" Caption="Attributes" DataSourceID="ds" Height="150px" MatrixMode="True" Width="420px" SkinID="Attributes">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AttributeID,EntityType,EntityID" DataMember="Answers">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
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
                    <px:PXGrid ID="PXGridCategory" runat="server" Caption="Sales Categories" DataSourceID="ds" Height="220px" Width="250px"
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
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXImageUploader Height="150px" Width="420px" ID="imgUploader" runat="server" DataField="ImageUrl" DataMember="ItemSettings" AllowUpload="true" SuppressLabel="True" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Description">
                <Template>
                    <px:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Style="border-width: 0px; border-top-width: 1px; width: 100%;"
                        DatafieldPreviewGraph="PX.Objects.IN.InventoryItemMaint" DatafieldPreviewView="Item" 
						AllowAttached="true" AllowSearch="true" AllowLoadTemplate="false" AllowSourceMode="true">
                        <AutoSize Enabled="True" MinHeight="216" />
                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
                    </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Service Skills" LoadOnDemand="True" RepaintOnDemand="False" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridServiceSkills" AutoGenerateColumns="None" SkinID="DetailsInTab" Style='height:120px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceSkills">
                                <Columns>
                                    <px:PXGridColumn DataField="SkillID" Width="120px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="FSSkill__Descr" Width="200px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edSkillID" DataField="SkillID" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXTextEdit runat="server" ID="edFSSkill__Descr" DataField="FSSkill__Descr" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Service License Types" LoadOnDemand="True" RepaintOnDemand="False" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridServiceLicenseTypes" AutoGenerateColumns="None" SkinID="DetailsInTab" Style='height:120px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceLicenseTypes">
                                <Columns>
                                    <px:PXGridColumn DataField="LicenseTypeID" Width="120px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="FSLicenseType__Descr" Width="200px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edLicenseTypeID" DataField="LicenseTypeID" AllowEdit="True" />
                                    <px:PXTextEdit runat="server" ID="edFSLicenseType__Descr" DataField="FSLicenseType__Descr" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Resource Equipment Types" LoadOnDemand="True" RepaintOnDemand="False" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid runat="server" ID="gridServiceEquipmentTypes" SkinID="DetailsInTab" AutoGenerateColumns="None" Style='height:120px;width:100%;'>
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceEquipmentTypes">
                                <Columns>
                                    <px:PXGridColumn DataField="EquipmentTypeID" Width="150px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="FSEquipmentType__Descr" Width="400px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector runat="server" ID="edEquipmentTypeID" DataField="EquipmentTypeID" AllowEdit="True" CommitChanges="True" />
                                    <px:PXTextEdit runat="server" ID="edFSEquipmentType__Descr" DataField="FSEquipmentType__Descr" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Pickup/Delivery Item" LoadOnDemand="True" RepaintOnDemand="False" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXPanel runat="server" ID="CstPanel43">
                        <px:PXLayoutRule runat="server" ID="CstPXLayoutRule47" StartRow="True" ControlSize="SM" LabelsWidth="SM" />
                        <px:PXDropDown runat="server" ID="CstPXDropDown48" DataField="ActionType" CommitChanges="True" AllowEdit="True" /></px:PXPanel>
                    <px:PXGrid runat="server" ID="gridPickDeliver" SkinID="DetailsInTab" Width="100%" AllowPaging="True" AdjustPageSize="Auto" FilesIndicator="False" NoteIndicator="False" Height="200px" TabIndex="11300" DataSourceID="ds">
                        <Levels>
                            <px:PXGridLevel DataMember="ServiceInventoryItems" DataKeyNames="ServiceID,InventoryID">
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" Width="160px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="InventoryItem__Descr" Width="400" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask runat="server" ID="edSMInventoryID" DataField="InventoryID" CommitChanges="True" AllowEdit="True" />
                                    <px:PXTextEdit runat="server" ID="edInventoryItem__Descr" DataField="InventoryItem__Descr" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Sync Status">
                <Template>
                    <px:PXGrid ID="syncGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Inquire" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="SyncRecs" DataKeyNames="SyncRecordID">
                                <Columns>
                                    <px:PXGridColumn DataField="SYProvider__Name" Width="200px" />                                    
                                    <px:PXGridColumn DataField="RemoteID" Width="200px" CommitChanges="True" LinkCommand="GoToSalesforce" />
                                    <px:PXGridColumn DataField="Status" Width="120px" />
                                    <px:PXGridColumn DataField="Operation" Width="100px" />      
                                    <px:PXGridColumn DataField="LastErrorMessage" Width="230" />
                                    <px:PXGridColumn DataField="LastAttemptTS" Width="120px" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="AttemptCount" Width="120px" />
                                    <px:PXGridColumn DataField="SFEntitySetup__ImportScenario" Width="150" />
                                    <px:PXGridColumn DataField="SFEntitySetup__ExportScenario" Width="150" />
                                </Columns>                               
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>                        
                            <CustomItems>
                                <px:PXToolBarButton Key="SyncSalesforce">
                                    <AutoCallBack Command="SyncSalesforce" Target="ds"/>
                                </px:PXToolBarButton>
                             </CustomItems>
                         </ActionBar>
                        <Mode InitNewRow="true" />
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
    <px:PXSmartPanel ID="pnlUpdatePrice" runat="server" Key="VendorItems" CaptionVisible="True" Caption="Update Effective Vendor Prices" AllowResize="False">
        <px:PXFormView ID="formEffectiveDate" runat="server" DataSourceID="ds" CaptionVisible="false" DataMember="VendorInventory$UpdatePrice" SkinID="Transparent">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXDateTimeEdit ID="edPendingDate" runat="server" DataField="PendingDate" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton9" runat="server" DialogResult="OK" Text="Update" />
            <px:PXButton ID="PXButton10" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
