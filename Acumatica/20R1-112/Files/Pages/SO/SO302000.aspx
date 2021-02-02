<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO302000.aspx.cs" Inherits="Page_SO302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOShipmentEntry" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="SelectSO" Visible="False" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="AddSO" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddSOCancel" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Action" Visible="True" CommitChanges="true" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Report" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Inquiry" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Hold" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Flow" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="InventorySummary" Visible="false" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="RecalculatePackages" />
			<px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSSOShipLine_generateLotSerial" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSSOShipLine_binLotSerial" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalculatePackages" Visible="false" />
            <px:PXDSCallbackCommand Name="ShopRates" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="RefreshRates" Visible="false" CommitChanges="true" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Shipment Summary" NoteIndicator="True" FilesIndicator="True" LinkIndicator="true"
        NotifyIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edShipmentNbr" MarkRequired="Dynamic">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edShipmentNbr" runat="server" DataField="ShipmentNbr" AutoRefresh="true">
                <GridProperties FastFilterFields="CustomerID" />
            </px:PXSelector>
            <px:PXDropDown ID="edShipmentType" runat="server" DataField="ShipmentType" CommitChanges="true" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold">
                <AutoCallBack Command="Hold" Target="ds">
                    <Behavior CommitChanges="true" />
                </AutoCallBack>
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXDropDown ID="edOperation" runat="server" DataField="Operation" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edShipDate" runat="server" DataField="ShipDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" AutoRefresh="True" DataField="CustomerLocationID" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
			<px:PXSegmentMask ID="edDestinationSiteID" runat="server" CommitChanges="True" DataField="DestinationSiteID" DataSourceID="ds"  />
            <px:PXTreeSelector CommitChanges="True" ID="PXTreeSelector1" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds" PopulateOnDemand="true" InitialExpandLevel="0"
                ShowRootNode="false">
                <DataBindings>
                    <px:PXTreeItemBinding TextField="Description" ValueField="Description" />
                </DataBindings>
            </px:PXTreeSelector>
            <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" DisplayMode="Text" />
            <px:PXSelector ID="edWorksheet" runat="server" DataField="CurrentWorksheetNbr" AllowEdit="true"/>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXNumberEdit ID="edShipmentQty" runat="server" DataField="ShipmentQty" Enabled="False" />
            <px:PXNumberEdit CommitChanges="True" ID="edControlQty" runat="server" DataField="ControlQty" />
			<px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="ShipmentWeight" Enabled="False" />
			<px:PXNumberEdit ID="PXNumberEdit4" runat="server" DataField="ShipmentVolume" Enabled="False" />
			<px:PXNumberEdit ID="PXNumberEdit3" runat="server" DataField="PackageCount" Enabled="False" />
			<px:PXNumberEdit ID="PXNumberEdit2" runat="server" DataField="PackageWeight" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="423px" Style="z-index: 100;" Width="100%" TabIndex="23">
        <Items>
            <px:PXTabItem Text="Document Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="height: 250px;" Width="100%" SkinID="DetailsInTab" StatusField="Availability" SyncPosition="true" Height="372px" TabIndex="-7372">
                        <Levels>
                            <px:PXGridLevel DataMember="Transactions" DataKeyNames="ShipmentNbr,LineNbr">
                                <Columns>
                                    <px:PXGridColumn DataField="Availability" />
                                    <px:PXGridColumn DataField="ShipmentNbr" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrigOrderType" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrigOrderNbr" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrigLineNbr" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" NullText="<SPLIT>" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsFree" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" NullText="<SPLIT>" />
                                    <px:PXGridColumn DataField="UOM" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" AutoCallBack="True" DataField="ShippedQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="BaseShippedQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="OriginalShippedQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="OrigOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="OpenOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="UnassignedQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="PackedQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CompleteQtyMin" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LotSerialNbr" NullText="<SPLIT>" />
                                    <px:PXGridColumn DataField="ShipComplete" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                    <px:PXGridColumn DataField="ReasonCode" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXTextEdit ID="edOrigOrderType" runat="server" DataField="OrigOrderType" Enabled="False" />
                                    <px:PXSelector ID="edOrigOrderNbr" runat="server" DataField="OrigOrderNbr" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXCheckBox ID="chkIsFree" runat="server" DataField="IsFree" Enabled="False" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edShippedQty" runat="server" DataField="ShippedQty" />
                                    <px:PXNumberEdit ID="edOrigOrderQty" runat="server" DataField="OrigOrderQty" Enabled="False" />
                                    <px:PXLayoutRule runat="server" ColumnSpan="2" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXTextEdit ID="edShipmentNbr" runat="server" DataField="ShipmentNbr" />
                                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
                                    <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOShipLine.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXDropDown ID="edShipComplete" runat="server" AllowNull="False" DataField="ShipComplete" />
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                    <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode">
                                        <Parameters>
                                            <px:PXControlParam ControlID="form" Name="SOShipLine.orderType" PropertyName="NewDataKey[&quot;OrderType&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                </RowTemplate>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowFormEdit="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSSOShipLine_binLotSerial" CommandSourceID="ds" DependOnGrid="grid" />
                                <px:PXToolBarButton Text="Add Order">
                                    <AutoCallBack Command="SelectSO" Target="ds">
                                        <Behavior PostData="Page" CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Inventory Summary">
                                    <AutoCallBack Command="InventorySummary" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Orders">
                <Template>
                    <px:PXGrid ID="grid5" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details" BorderWidth="0px">
                        <Levels>
                            <px:PXGridLevel DataMember="OrderList" DataKeyNames="OrderType,OrderNbr,ShipmentType,ShipmentNbr">
                                <Columns>
                                    <px:PXGridColumn DataField="ShipmentNbr" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrderType" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrderNbr" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShipmentQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShipmentWeight" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShipmentVolume" TextAlign="Right" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvoiceType" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvoiceNbr" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvtDocType" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvtRefNbr" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXTextEdit ID="edOrderType3" runat="server" DataField="OrderType" Enabled="False" />
                                    <px:PXSelector ID="edOrderNbr3" runat="server" DataField="OrderNbr" AutoRefresh="True" AllowEdit="True" />
                                    <px:PXTextEdit ID="edShipmentNbr3" runat="server" DataField="ShipmentNbr" />
                                    <px:PXSelector SuppressLabel="True" ID="edInvoiceNbr3" runat="server" DataField="InvoiceNbr" AutoRefresh="True" AllowEdit="True" />
                                    <px:PXSelector SuppressLabel="True" ID="edInvtRefNbr3" runat="server" DataField="InvtRefNbr" AutoRefresh="True" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edShipmentQty3" runat="server" DataField="ShipmentQty" />
                                </RowTemplate>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar PagerGroup="3" PagerOrder="2" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Shipping Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXFormView ID="formD" runat="server" Caption="Ship-To Contact" DataMember="Shipping_Contact" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXCheckBox ID="chkOverrideContact" runat="server" CommitChanges="True" DataField="OverrideContact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None" />
                    </px:PXFormView>
                    <px:PXFormView ID="formB" runat="server" Caption="Ship-To Address" DataMember="Shipping_Address" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXCheckBox ID="chkOverrideAddress" runat="server" CommitChanges="True" DataField="OverrideAddress" />
                            <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" AutoRefresh="True" DataField="CountryID" DataSourceID="ds" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" DataSourceID="ds">
                                <CallBackMode PostData="Container" />
                                <Parameters>
                                    <px:PXControlParam ControlID="formB" Name="SOShippingAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value" Type="String" />
                                </Parameters>
                            </px:PXSelector>
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None" />
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXPanel RenderStyle="Fieldset" ID="PXPanel2" runat="server" Caption="Shipping Information">
                        <px:PXFormView ID="formF" runat="server" CaptionVisible="False" DataMember="CurrentDocument" DataSourceID="ds" AllowCollapse="false">
                            <Template>
                                <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                                <px:PXLayoutRule runat="server" Merge="True" />
                                <px:PXSelector ID="edShipVia" runat="server" CommitChanges="True" DataField="ShipVia" DataSourceID="ds" Size="s" />
                                <px:PXButton ID="shopRates" runat="server" Text="Shop For Rates" CommandName="ShopRates" CommandSourceID="ds" />
                                <px:PXLayoutRule runat="server" />
                                <px:PXCheckBox ID="edWillCall" runat="server" DataField="WillCall" Tooltip="The Will Call flag depends on the Common Carrier selection in the Ship Via field." Width="80px" />
								<px:PXDropDown runat="server" ID="edFreightClass" DataField="FreightClass" />
                                <px:PXSelector ID="edFOBPoint" runat="server" DataField="FOBPoint" DataSourceID="ds" />
                                <px:PXSelector ID="edShipTermsID" runat="server" CommitChanges="True" DataField="ShipTermsID" DataSourceID="ds" AutoRefresh="true" />
								<px:PXSelector ID="edShipZoneID" runat="server" CommitChanges="True" DataField="ShipZoneID" DataSourceID="ds" />
                                <px:PXCheckBox runat="server" ID="edSkipAddressVerification" DataField="SkipAddressVerification" />
                                <px:PXCheckBox ID="chkResedential" runat="server" DataField="Resedential" />
								<px:PXCheckBox ID="chkSaturdayDelivery" runat="server" DataField="SaturdayDelivery" />
								<px:PXCheckBox ID="chkUseCustomerAccount" runat="server" CommitChanges="True" DataField="UseCustomerAccount" />
								<px:PXCheckBox ID="chkInsurance" runat="server" DataField="Insurance" />
                                <px:PXCheckBox ID="chkGroundCollect" runat="server" DataField="GroundCollect" />
                                <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_SOShipment_CurrencyInfo_" DataMember="_Currency_" />
                                <px:PXNumberEdit ID="edCuryFreightCost" runat="server" DataField="CuryFreightCost" CommitChanges="true" />
								<px:PXCheckBox ID="chkOverrideFreightAmount" runat="server" DataField="OverrideFreightAmount" CommitChanges="True" />
								<px:PXDropDown ID="edFreightAmountSource" runat="server" DataField="FreightAmountSource" />
                                <px:PXNumberEdit ID="edCuryFreightAmt" runat="server" CommitChanges="True" DataField="CuryFreightAmt" />
                            </Template>
                            <ContentStyle BackColor="Transparent" BorderStyle="None" />
                        </px:PXFormView>
                    </px:PXPanel>
					<px:PXPanel runat="server" Caption="Service Management" RenderStyle="Fieldset">
						<px:PXFormView runat="server" ID="gridSM" SkinID="Transparent" DataSourceID="ds" DataMember="CurrentDocument" CaptionVisible="False">
							<Template>
								<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
								<px:PXCheckBox runat="server" ID="edInstalled" DataField="Installed" />
							</Template>
						</px:PXFormView>
					</px:PXPanel>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Packages">
                <Template>
	                <px:PXGrid ID="gridPackages" runat="server" DataSourceID="ds" RepaintColumns="True" AutoRepaint="True" MatrixMode="True" Style="z-index: 100; left: 0px; top: 0px; height: 372px;" Width="100%" SkinID="Details" BorderWidth="0px" SyncPosition="True">
	                    <ActionBar Position="TopAndBottom">
							<CustomItems>
								<px:PXToolBarButton Text="Recalculate Packages">
									<AutoCallBack Command="RecalculatePackages" Target="ds"/>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoCallBack Target="gridPackageDetailSplit" Command="Refresh">
							<Behavior CommitChanges="True" RepaintControlsIDs="gridPackageDetailSplit"/>
						</AutoCallBack>
	                    <Mode InitNewRow="True"/>
                        <Levels>
                            <px:PXGridLevel DataMember="Packages">
                                 <RowTemplate>
	                                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
	                                <px:PXMaskEdit ID="edShipmentNbr_Pkg" runat="server" DataField="ShipmentNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
	                                <px:PXSelector ID="edBoxID" runat="server" DataField="BoxID" />
	                                <px:PXNumberEdit ID="edLineNbr_Pkg" runat="server" DataField="LineNbr" />
                                    <px:PXNumberEdit ID="edWeight" runat="server" DataField="Weight" CommitChanges="True" />
                                    <px:PXNumberEdit ID="PXNumberEdit5" runat="server" DataField="COD" />
                                    <px:PXNumberEdit ID="PXNumberEdit6" runat="server" DataField="DeclaredValue" />
	                                <px:PXTextEdit ID="edTrackNumber" runat="server" DataField="TrackNumber" />
                                    <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="CustomRefNbr1" />
                                    <px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="CustomRefNbr2" />
                                    <px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="Description" />
                                    <px:PXDropDown runat="server" ID="edType" DataField="PackageType"></px:PXDropDown>
                                    <px:PXDropDown runat="server" ID="edStampsAddOns" DataField="StampsAddOns" AllowMultiSelect="True" />
								</RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowNull="False" DataField="Confirmed" Label="Confired" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="BoxID" DisplayFormat="&gt;aaaaaaaaaaaaaaa" Label="Box ID" />
									<px:PXGridColumn DataField="PackageType" Type="DropDownList" />
                                    <px:PXGridColumn AutoGenerateOption="NotSet" DataField="Description" MaxLength="30" />
									<px:PXGridColumn AllowNull="False" DataField="Weight" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="WeightUOM" />
                                    <px:PXGridColumn AllowNull="False" DataField="DeclaredValue" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="COD" Label="C.O.D. Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="StampsAddOns" />
                                    <px:PXGridColumn DataField="TrackNumber" />
                                    <px:PXGridColumn DataField="CustomRefNbr1" MaxLength="60" />
                                    <px:PXGridColumn DataField="CustomRefNbr2" MaxLength="60" />
                                    <px:PXGridColumn DataField="ContentType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ContentTypeDesc" Multiline="True" />
                                    <px:PXGridColumn DataField="CertificateNumber" />
                                    <px:PXGridColumn DataField="InvoiceNumber" />
                                    <px:PXGridColumn DataField="LicenseNumber" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
					<px:PXGrid ID="gridPackageDetailSplit" runat="server" SyncPosition="True" SkinID="DetailsInTab" Caption="Contents of Selected Package" Width="100%">
						<Mode InitNewRow="True"/>
						<AutoSize Enabled="True"/>
						<Levels>
							<px:PXGridLevel DataMember="PackageDetailSplit">
								<RowTemplate>
									<px:PXSegmentMask ID="edInventoryIDSplit" runat="server" DataField="InventoryID"/>
									<px:PXSegmentMask ID="edSubItemIDSplit" runat="server" DataField="SubItemID"/>
									<px:PXTextEdit ID="edLotSerialNbrSplit" runat="server" DataField="LotSerialNbr" Enabled="false"/>
									<px:PXNumberEdit ID="edQtySplit" runat="server" DataField="PackedQty"/>
									<px:PXTextEdit ID="edUOMSplit" runat="server" DataField="UOM"/>
									<px:PXSelector ID="edShipmentSplitLineNbr" runat="server" DataField="ShipmentSplitLineNbr" AutoRefresh="True"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ShipmentSplitLineNbr" CommitChanges="True"/>
									<px:PXGridColumn DataField="InventoryID" />
									<px:PXGridColumn DataField="SubItemID" />
									<px:PXGridColumn DataField="LotSerialNbr" />
									<px:PXGridColumn DataField="UOM" />
									<px:PXGridColumn DataField="PackedQty" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Parameters>
							<px:PXSyncGridParam ControlID="gridPackages"/>
						</Parameters>
					</px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <%-- Bin/Lot/Serial Numbers --%>
    <px:PXSmartPanel ID="PanelLS" runat="server" Style="z-index: 108; left: 252px; position: absolute; top: 531px; height: 500px;" Width="764px" Caption="Allocations" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSSOShipLine_lotseropts" DataSourceID="ds" SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSSOShipLine_generateLotSerial" CommandSourceID="ds"></px:PXButton>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" SyncPosition="true" SkinID="Details">
            <AutoSize Enabled="true" />
            <Mode InitNewRow="True" />
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="splits">
                    <Columns>
                        <px:PXGridColumn DataField="InventoryID" CommitChanges="true" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="LocationID" CommitChanges="true" />
                        <px:PXGridColumn DataField="LotSerialNbr" CommitChanges="true" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="ExpireDate" />
                        <px:PXGridColumn AllowUpdate="False" DataField="InventoryID_InventoryItem_descr" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true" />
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="SOShipLineSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOShipLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOShipLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="SOShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="SOShipLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOShipLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOShipLineSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXDateTimeEdit ID="edExpireDate2" runat="server" DataField="ExpireDate" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Add Sales Order --%>
    <px:PXSmartPanel ID="PanelAddSO" runat="server" Height="400px" Style="z-index: 108; left: 216px; position: absolute; top: 171px" Width="873px" CommandName="AddSO" CommandSourceID="ds" Caption="Add Sales Order"
        CaptionVisible="True" LoadOnDemand="true" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AutoRepaint="true" Key="addsofilter">
        <px:PXFormView ID="form4" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="addsofilter" CaptionVisible="False">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edOperation" runat="server" DataField="Operation" />
                <px:PXSelector CommitChanges="True" ID="edOrderType4" runat="server" DataField="OrderType" AutoRefresh="true" />
                <px:PXSelector ID="edOrderNbr4" runat="server" DataField="OrderNbr" AutoRefresh="true">
                    <AutoCallBack Target="grid4" Command="Refresh" />
                    <AutoCallBack Command="Save" Target="form4" />
                    <Parameters>
                        <px:PXControlParam ControlID="form4" Name="AddSOFilter.orderType" PropertyName="DataControls[&quot;edOrderType4&quot;].Value" Type="String" />
                    </Parameters>
                </px:PXSelector>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid4" runat="server" Height="240px" Width="100%" DataSourceID="ds" BatchUpdate="true" Style="border-width: 1px 0px" SkinID="Inquire" AutoAdjustColumns="True">
            <AutoSize Enabled="true" />
            <Parameters>
                <px:PXControlParam ControlID="form4" Name="AddSOFilter_orderType" PropertyName="DataControls[&quot;edOrderType4&quot;].Value" Type="String" />
                <px:PXControlParam ControlID="form4" Name="AddSOFilter_orderNbr" PropertyName="DataControls[&quot;edOrderNbr4&quot;].Value" Type="String" />
                <px:PXControlParam ControlID="form4" Name="AddSOFilter_operation" PropertyName="DataControls[&quot;edOperation&quot;].Value" Type="String" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="soshipmentplan">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" TextAlign="Center" />
                        <px:PXGridColumn DataField="SOLine__LineNbr" Visible="false" />
                        <px:PXGridColumn DataField="SOLine__InventoryID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SOLineSplit__SubItemID" DisplayFormat="&gt;AA-A-A" />
                        <px:PXGridColumn DataField="SOLineSplit__UOM" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn DataField="SOLineSplit__LotSerialNbr" />
                        <px:PXGridColumn DataField="PlanDate" Label="Plan Date" />
                        <px:PXGridColumn DataField="SOLineSplit__Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="SOLine__TranDesc" />
                    </Columns>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" CommandName="AddSO" CommandSourceID="ds" Text="Add" SyncVisible="false"/>
            <px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="Add &amp; Close" />
            <px:PXButton ID="PXButton3" runat="server" CommandName="AddSOCancel" CommandSourceID="ds" DialogResult="Cancel" Text="Cancel" SyncVisible="false" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Carrier Rates --%>
    <px:PXSmartPanel ID="PanelCarrierRates" Width="820" runat="server" Caption="Shop For Rates" CaptionVisible="True" LoadOnDemand="True" ShowAfterLoad="True" Key="CurrentDocument"
        AutoCallBack-Target="formCarrierRates" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="PXButtonRatesOK" AllowResize="False">
        <px:PXFormView ID="formCarrierRates" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="CurrentDocument"
            Caption="Services Settings" CaptionVisible="False" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXNumberEdit ID="edOrderWeight" runat="server" DataField="ShipmentWeight" Enabled="False" />
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="PackageWeight" Enabled="False" />
            </Template>
        </px:PXFormView>

        <px:PXGrid ID="gridRates" runat="server" Width="100%" DataSourceID="ds" Style="border-width: 1px 1px; left: 0px; top: 0px;"
            AutoAdjustColumns="true" Caption="Carrier Rates" Height="120px" AllowFilter="False" SkinID="Details" CaptionVisible="True" AllowPaging="False">
            <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" />
            <ActionBar Position="Top" PagerVisible="False" CustomItemsGroup="1" ActionsVisible="True">
                <CustomItems>
                    <px:PXToolBarButton Text="Get Rates">
                        <AutoCallBack Command="RefreshRates" Target="ds" />
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="CarrierRates">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Width="60px" Type="CheckBox" AutoCallBack="true" TextAlign="Center" />
                        <px:PXGridColumn DataField="Method" Label="Code" Width="140px" />
                        <px:PXGridColumn DataField="Description" Label="Description" Width="190px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="Amount" Width="60px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="DaysInTransit" Label="Days in Transit" Width="85px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="DeliveryDate" Label="Delivery Date" Width="80px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXGrid ID="gridPackagesForRates" runat="server" Width="100%" DataSourceID="ds" Style="border-width: 1px 1px; left: 0px; top: 0px;"
            Caption="Packages" SkinID="Details" Height="80px" CaptionVisible="True" AllowPaging="False">
            <ActionBar Position="TopAndBottom">
                <CustomItems>
                    <px:PXToolBarButton Text="Recalculate Packages">
                        <AutoCallBack Command="RecalculatePackages" Target="ds" />
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="PackagesForRates">
                    <Columns>
                        <px:PXGridColumn DataField="BoxID" Width="120px" CommitChanges="True" />
                        <px:PXGridColumn DataField="BoxDescription" Label="Description" Width="200px" />
                        <px:PXGridColumn DataField="WeightUOM" Width="55px" />
                        <px:PXGridColumn DataField="Weight" Width="60px" />
                        <px:PXGridColumn DataField="BoxWeight" Width="60px" />
                        <px:PXGridColumn DataField="NetWeight" Width="60px" />
                        <px:PXGridColumn DataField="DeclaredValue" Width="70px" />
                        <px:PXGridColumn DataField="COD" Width="55px" />
                        <px:PXGridColumn DataField="StampsAddOns" Width="200" Type="DropDownList" />
                    </Columns>
                    <RowTemplate>
                        <px:PXDropDown runat="server" ID="edStampsAddOns2" DataField="StampsAddOns" AllowMultiSelect="True" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel8" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonRatesOK" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
