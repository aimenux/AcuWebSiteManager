<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM310000.aspx.cs" Inherits="Page_AM310000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.VendorShipmentEntry" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Hold" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSAMShipLine_generateLotSerial" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSAMShipLine_binLotSerial" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Shipment Summary" NoteIndicator="True" 
        FilesIndicator="True" LinkIndicator="true" NotifyIndicator="true"  ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edShipmentNbr" >
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edShipmentNbr" runat="server" DataField="ShipmentNbr" AutoRefresh="true" />
            <px:PXDropDown ID="edShipmentType" runat="server" DataField="ShipmentType" CommitChanges="true" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="true">
            </px:PXCheckBox>
            <px:PXDateTimeEdit CommitChanges="True" ID="edShipDate" runat="server" DataField="ShipmentDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
            <px:PXSelector CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
			<px:PXSelector ID="edWorkgroupID" runat="server" CommitChanges="True" DataField="WorkgroupID" DataSourceID="ds"  />
            <px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXNumberEdit ID="edShipmentQty" runat="server" DataField="ShipmentQty" Enabled="False" />
            <px:PXNumberEdit CommitChanges="True" ID="edControlQty" runat="server" DataField="ControlQty" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="423px" Style="z-index: 100;" Width="100%" TabIndex="23">
        <Items>
            <px:PXTabItem Text="Document Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="height: 250px;" Width="100%" SkinID="DetailsInTab" StatusField="Availability" 
                        SyncPosition="true" Height="372px" TabIndex="-7372">
                        <Levels>
                            <px:PXGridLevel DataMember="Transactions" DataKeyNames="ShipmentNbr,LineNbr">
                                <Columns>
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LineType" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="OrderType" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ProdOrdID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="OperationID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubItemID" Width="81px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="MatlLineID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SiteID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LocationID" Width="130px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UOM" />
                                    <px:PXGridColumn DataField="LotSerialNbr" NullText="&lt;SPLIT&gt;" Width="130px" />
                                    <px:PXGridColumn DataField="ExpireDate" Width="90px" />
                                    <px:PXGridColumn DataField="InventoryID_description" Width="200px"/>
                                    <px:PXGridColumn DataField="TranDesc" Width="200px"/>
                                    <px:PXGridColumn DataField="POOrderNbr" />
                                    <px:PXGridColumn DataField="POLineNbr" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />  
                                    <px:PXDropDown ID="edLineType" runat="server" DataField="LineType" CommitChanges="true"/>
                                    <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" AutoRefresh="true" CommitChanges="True"/>
                                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AutoRefresh="True" CommitChanges="True" AllowEdit="True"/>
                                    <px:PXSelector ID="edOperationID" runat="server"  DataField="OperationID" AutoRefresh="True" CommitChanges="True"/>
                                    <px:PXSegmentMask  ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True" CommitChanges="True" AllowEdit="True"/> 
                                    <px:PXSegmentMask Size="xxs" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" CommitChanges="true" NullText="<SPLIT>">
					                    <Parameters>
						                    <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
							                    Type="String" />
					                    </Parameters>
				                    </px:PXSegmentMask>                                    
                                    <px:PXSelector ID="edMaterialLineID" runat="server" DataField="MatlLineID"  AutoRefresh="True" CommitChanges="true" />
                                    <px:PXSegmentMask  ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True" CommitChanges="True"/>
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" NullText="<SPLIT>">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.siteID" PropertyName="DataValues[&quot;SiteID&quot;]"
                                                Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                                Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
                                                Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXNumberEdit  ID="edShippedQty" runat="server" DataField="Qty" CommitChanges="True"/>  
                                    <px:PXSelector  ID="edUOM" runat="server" DataField="UOM" CommitChanges="True"/>
                                     <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" NullText="<SPLIT>" AutoRefresh="True">
						                <Parameters>
							                <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
								                Type="String" />
							                <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
								                Type="String" />
							                <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.locationID" PropertyName="DataValues[&quot;LocationID&quot;]"
								                Type="String" />
							                <px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
						                </Parameters>
					                </px:PXSelector>   
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" Enabled="False"/>
                                    <px:PXTextEdit ID="edInventoryID_description" runat="server" DataField="InventoryID_description" Enabled="False"/>
                                    <px:PXTextEdit ID="edPOOrderNbr" runat="server" DataField="POOrderNbr" />
                                    <px:PXNumberEdit ID="edPOLineNbr" runat="server" DataField="POLineNbr" />
                                </RowTemplate>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="True" AllowFormEdit="True" InitNewRow="True" />
                        <ActionBar ActionsText="False">
                            </ActionBar>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSAMShipLine_binLotSerial" CommandSourceID="ds" DependOnGrid="grid" />
<%--                                <px:PXToolBarButton Text="Add Order">
                                    <AutoCallBack Command="SelectSO" Target="ds">
                                        <Behavior PostData="Page" CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Inventory Summary">
                                    <AutoCallBack Command="InventorySummary" Target="ds" />
                                </px:PXToolBarButton>--%>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Shipping Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXFormView ID="formD" runat="server" Caption="Ship-To Contact" DataMember="ShippingContact" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
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
                    <px:PXFormView ID="formB" runat="server" Caption="Ship-To Address" DataMember="ShippingAddress" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
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
                                    <px:PXControlParam ControlID="formB" Name="AMVendorShipmentAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value" Type="String" />
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
                               <%-- <px:PXButton ID="shopRates" runat="server" Text="Shop For Rates" CommandName="ShopRates" CommandSourceID="ds" />--%>
                                <px:PXLayoutRule runat="server" />
								<%--<px:PXDropDown runat="server" ID="edFreightClass" DataField="FreightClass" />--%>
                                <px:PXSelector ID="edFOBPoint" runat="server" DataField="FOBPoint" DataSourceID="ds" />
                                <px:PXSelector ID="edShipTermsID" runat="server" CommitChanges="True" DataField="ShipTermsID" DataSourceID="ds" AutoRefresh="true" />
								<px:PXSelector ID="edShipZoneID" runat="server" CommitChanges="True" DataField="ShipZoneID" DataSourceID="ds" />
                                <%--<px:PXCheckBox runat="server" ID="edSkipAddressVerification" DataField="SkipAddressVerification" />--%>
                                <px:PXCheckBox ID="chkResedential" runat="server" DataField="Residential" />
								<px:PXCheckBox ID="chkSaturdayDelivery" runat="server" DataField="SaturdayDelivery" />
								<px:PXCheckBox ID="chkInsurance" runat="server" DataField="Insurance" />
                                <px:PXCheckBox ID="chkGroundCollect" runat="server" DataField="GroundCollect" />
                                <%--<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_SOShipment_CurrencyInfo_" DataMember="_Currency_" />--%>
                                <px:PXNumberEdit ID="edCuryFreightCost" runat="server" DataField="CuryFreightCost" CommitChanges="true" />
								<px:PXCheckBox ID="chkOverrideFreightAmount" runat="server" DataField="OverrideFreightAmount" CommitChanges="True" />
								<px:PXDropDown ID="edFreightAmountSource" runat="server" DataField="FreightAmountSource" />
                                <px:PXNumberEdit ID="edCuryFreightAmt" runat="server" CommitChanges="True" DataField="CuryFreightAmt" />
                            </Template>
                            <ContentStyle BackColor="Transparent" BorderStyle="None" />
                        </px:PXFormView>
                    </px:PXPanel>
<%--					<px:PXPanel runat="server" Caption="Service Management" RenderStyle="Fieldset">
						<px:PXFormView runat="server" ID="gridSM" SkinID="Transparent" DataSourceID="ds" DataMember="CurrentDocument" CaptionVisible="False">
							<Template>
								<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
								<px:PXCheckBox runat="server" ID="edInstalled" DataField="Installed" />
							</Template>
						</px:PXFormView>
					</px:PXPanel>--%>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <%-- Bin/Lot/Serial Numbers --%>
    <px:PXSmartPanel ID="PanelLS" runat="server" Style="z-index: 108; left: 252px; position: absolute; top: 531px; height: 500px;" Width="764px" Caption="Allocations" CaptionVisible="True"
        Key="lsselect" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="optform">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSAMShipLine_lotseropts" DataSourceID="ds" SkinID="Transparent">
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
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSAMShipLine_generateLotSerial" CommandSourceID="ds"></px:PXButton>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" SyncPosition="true" SkinID="Details">
            <AutoSize Enabled="true" />
            <Mode InitNewRow="True" />
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="Splits">
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
                                <px:PXControlParam ControlID="grid2" Name="AMVendorShipLineSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMVendorShipLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMVendorShipLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="AMVendorShipLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="AMVendorShipLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMVendorShipLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="AMVendorShipLineSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
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
</asp:Content>