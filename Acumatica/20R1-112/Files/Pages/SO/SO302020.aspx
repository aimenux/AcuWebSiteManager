<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO302020.aspx.cs"
    Inherits="Page_SO302020" Title="Pick, Pack, and Ship" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:content id="cont1" contentplaceholderid="phDS" runat="Server">
    <script language="javascript" type="text/javascript">
        function Barcode_Initialize(ctrl) {
            ctrl.element.addEventListener('keydown', function (e) {
                if (e.keyCode === 13) { //Enter key 
                    e.preventDefault();
                    e.stopPropagation();
                }
            });
        };
        
        function ActionCallback(callbackContext) {
            var baseUrl = (location.href.indexOf("HideScript") > 0) ? "../../Sounds/" : "../../../Sounds/";
            var edInfoMessageSoundFile = px_alls["edInfoMessageSoundFile"];

            if ((callbackContext.info.name.toLowerCase().startsWith("scan") || callbackContext.info.name == "ElapsedTime") && callbackContext.control.longRunInProcess == null && edInfoMessageSoundFile != null) {
                var soundFile = edInfoMessageSoundFile.getValue();
                if (soundFile != null && soundFile != "") {
                    var audio = new Audio(baseUrl + soundFile + '.wav');
                    audio.play();
                }
            }
        };

        window.addEventListener('load', function () { px_callback.addHandler(ActionCallback); });
    </script>
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.PickPackShipHost" PrimaryView="HeaderView">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Cancel" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ClearBtn" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanQty" Visible="true" CommitChanges="True" />
            
            <px:PXDSCallbackCommand Name="LSSOShipLine_generateLotSerial" Visible="False" />
            <px:PXDSCallbackCommand Name="LSSOShipLine_binLotSerial" Visible="False"/>

            <px:PXDSCallbackCommand Name="Scan" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModePick" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModePack" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeShip" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeReceive" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModePutAway" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeItemLookup" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeStorageLookup" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeIssue" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeInReceive" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeInTransfer" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModePhysicalCount" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanConfirm" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanRemove" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanConfirmShipment" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanConfirmShipmentAll" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanConfirmPickList" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ReviewPickWS" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ReviewPick" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ReviewPack" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="gridPicked" Name="ViewOrder" Visible="False"/>
            <px:PXDSCallbackCommand Name="GetReturnLabels" CommitChanges="True" Visible="False"  />
            <px:PXDSCallbackCommand Name="RefreshRates" CommitChanges="True" Visible="False"  />
            <px:PXDSCallbackCommand Name="UserSetupDialog" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
	<style>
		.ProcessingStatusIcon .main-icon-img {
			font-size: 90px;
			margin: -10px;
		}
		.ProcessingStatusIcon .main-icon {
			height: 100px;
			width: 100px;
		}
		.ProcessingStatusIcon div.checkBox {
			height: 100px;
			width: 50px;
		}
	</style>
</asp:content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
    <px:PXFormView ID="formHeader" runat="server" DataSourceID="ds" Height="120px" Width="100%" Visible="true" DataMember="HeaderView" DefaultControlID="edBarcode" FilesIndicator="True" >
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
            <px:PXTextEdit ID="edBarcode" runat="server" DataField="Barcode">
                <AutoCallBack Command="Scan" Target="ds">
                    <Behavior CommitChanges="True" />
                </AutoCallBack>
                <ClientEvents Initialize="Barcode_Initialize"/>
            </px:PXTextEdit>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" CommitChanges="true" AutoRefresh="true" AutoComplete="false" AllowEdit="true" />
            <px:PXSelector ID="edWSNbr" runat="server" DataField="WorksheetNbr" CommitChanges="true" AutoRefresh="true" AutoComplete="false" AllowEdit="true" />
            <px:PXSelector ID="edCartID" runat="server" DataField="CartID" CommitChanges="true" AutoRefresh="true" AutoComplete="false" />
            <px:PXLayoutRule runat="server" StartColumn="true" ControlSize="XS"/>
            <px:PXCheckBox ID="chkStatusIcon" runat="server" DataField="ProcessingStatusIcon" RenderStyle="Button" CommitChanges="true" AlignLeft="True" CssClass="ProcessingStatusIcon">
                <CheckImages Normal="main@Success" />
                <UncheckImages Normal="main@Fail" />
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" ColumnWidth="M" />
            <px:PXTextEdit ID="edMessage" runat="server" DataField="Message" Width="800px" Style="font-size: 10pt; font-weight: bold;" SuppressLabel="true" TextMode="MultiLine" Height="55px" SkinID="Label" DisableSpellcheck="True" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="3" Merge="True" />
            <px:PXCheckBox ID="chkManualView" runat="server" DataField="ManualView" CommitChanges="true" AlignLeft="True" />
            <px:PXCheckBox ID="chkRemove" runat="server" DataField="Remove" AlignLeft="True" />
            <px:PXCheckBox ID="chkCartLoaded" runat="server" DataField="CartLoaded" AlignLeft="True" />
            <px:PXTextEdit ID="edMode" runat="server" DataField="Mode"/>
            <px:PXCheckBox ID="chkShowPickWS" runat="server" DataField="ShowPickWS" />
            <px:PXCheckBox ID="chkShowPick" runat="server" DataField="ShowPick" />
            <px:PXCheckBox ID="chkShowPack" runat="server" DataField="ShowPack" />
            <px:PXCheckBox ID="chkShowShip" runat="server" DataField="ShowShip" />
            <px:PXCheckBox ID="chkShowLog"  runat="server" DataField="ShowLog" />
        </Template>
    </px:PXFormView>
    <px:PXFormView ID="formInfo" runat="server" DataSourceID="ds" DataMember="Info">
        <Template>
            <px:PXTextEdit ID="edInfoMode" runat="server" DataField="Mode"/>
            <px:PXTextEdit ID="edInfoMessage" runat="server" DataField="Message"/>
            <px:PXTextEdit ID="edInfoMessageSoundFile" runat="server" DataField="MessageSoundFile"/>
            <px:PXTextEdit ID="edInfoPrompt" runat="server" DataField="Prompt"/>
        </Template>
    </px:PXFormView>
</asp:content>
<asp:content id="cont3" contentplaceholderid="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="540px" Style="z-index: 100;" Width="100%">
        <Items>
            <px:PXTabItem Text="Pick" VisibleExp="DataControls[&quot;chkShowPickWS&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXGrid ID="gridPickedWS" runat="server" DataSourceID="ds" SyncPosition="true" Width="100%" SkinID="Inquire" OnRowDataBound="PickWSGrid_RowDataBound">
                        <Levels>
                            <px:PXGridLevel DataMember="PickListOfPicker">
                                <Columns>
                                    <px:PXGridColumn DataField="FitsWS" Type="CheckBox" />
                                    <px:PXGridColumn DataField="SiteID" />
                                    <px:PXGridColumn DataField="LocationID" />
                                    <px:PXGridColumn DataField="InventoryID" />
                                    <px:PXGridColumn DataField="LotSerialNbr" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                    <px:PXGridColumn DataField="PickedQty" />
                                    <px:PXGridColumn DataField="Qty" />
                                    <px:PXGridColumn DataField="UOM" />
                                    <px:PXGridColumn DataField="ShipmentNbr" />
                                    <px:PXGridColumn DataField="INTote__ToteCD" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edWSEntryNbr" runat="server" DataField="EntryNbr" Enabled="false"/>
                                    <px:PXSegmentMask ID="edWSInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edWSSiteID" runat="server" DataField="SiteID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edWSLocationID" runat="server" DataField="LocationID" Enabled="False"/>
                                    <px:PXTextEdit ID="edWSLotSerialNbr" runat="server" DataField="LotSerialNbr" Enabled="False" />
                                    <px:PXDateTimeEdit ID="edWSExpireDate" runat="server" DataField="ExpireDate" Enabled="False" />
                                    <px:PXNumberEdit ID="PXWSPickedQty" runat="server" DataField="PickedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXWSPackedQty" runat="server" DataField="PackedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXWSQty" runat="server" DataField="Qty" Enabled="False"/>
                                    <px:PXSelector ID="edWSUOM" runat="server" DataField="UOM" Enabled="False" />
                                 </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Pick" VisibleExp="DataControls[&quot;chkShowPick&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXGrid ID="gridPicked" runat="server" DataSourceID="ds" SyncPosition="true" Width="100%" SkinID="Inquire" OnRowDataBound="PickGrid_RowDataBound">
                        <Levels>
                            <px:PXGridLevel DataMember="Picked">
                                <Columns>
                                    <px:PXGridColumn DataField="Fits" Width="50px" Type="CheckBox" />
                                    <px:PXGridColumn DataField="LineNbr" Width="50px" />
                                    <px:PXGridColumn DataField="SOShipLine__OrigOrderType" />
                                    <px:PXGridColumn DataField="SOShipLine__OrigOrderNbr" Width="100px" LinkCommand="ViewOrder"/>
                                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                                    <px:PXGridColumn DataField="LocationID" Width="120px" />
                                    <px:PXGridColumn DataField="InventoryID" Width="180px" />
                                    <px:PXGridColumn DataField="SOShipLine__TranDesc" Width="250px" />
                                    <px:PXGridColumn DataField="LotSerialNbr" Width="120px" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                    <px:PXGridColumn DataField="CartQty" />
                                    <px:PXGridColumn DataField="OverAllCartQty" />
                                    <px:PXGridColumn DataField="PickedQty" />
                                    <px:PXGridColumn DataField="PackedQty" />
                                    <px:PXGridColumn DataField="Qty" />
                                    <px:PXGridColumn DataField="UOM" />
                                    <px:PXGridColumn DataField="SOShipLine__IsFree" Type="CheckBox" Width="50px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edPickLineNbr" runat="server" DataField="LineNbr" Enabled="false"/>
                                    <px:PXTextEdit ID="edPickOrigOrderType" runat="server" DataField="SOShipLine__OrigOrderType" Enabled="False" />
                                    <px:PXSelector ID="edPickOrigOrderNbr" runat="server" DataField="SOShipLine__OrigOrderNbr" Enabled="False"/>
                                    <px:PXSegmentMask ID="edPickInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edPickSiteID" runat="server" DataField="SiteID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edPickLocationID" runat="server" DataField="LocationID" Enabled="False"/>
                                    <px:PXSelector ID="edPickLotSerialNbr" runat="server" DataField="LotSerialNbr" Enabled="False" />
                                    <px:PXDateTimeEdit ID="edPickExpireDate" runat="server" DataField="ExpireDate" Enabled="False" />
                                    <px:PXNumberEdit ID="PXPickPickedQty" runat="server" DataField="PickedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXPickPackedQty" runat="server" DataField="PackedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXPickQty" runat="server" DataField="Qty" Enabled="False"/>
                                    <px:PXSelector ID="edPickUOM" runat="server" DataField="UOM" Enabled="False" />
                                    <px:PXCheckBox ID="chkPickIsFree" runat="server" DataField="SOShipLine__IsFree" Enabled="False" />
                                 </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Pack" VisibleExp="DataControls[&quot;chkShowPack&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXGrid ID="gridPacked" runat="server" DataSourceID="ds" SyncPosition="true" Width="100%" SkinID="Inquire" Height="200px"  OnRowDataBound="PackGrid_RowDataBound">
                        <Levels>
                            <px:PXGridLevel DataMember="PickedForPack">
                                <Columns>
                                    <px:PXGridColumn DataField="Fits" Width="50px" Type="CheckBox" />
                                    <px:PXGridColumn DataField="LineNbr" Width="50px" />
                                    <px:PXGridColumn DataField="SOShipLine__OrigOrderType" />
                                    <px:PXGridColumn DataField="SOShipLine__OrigOrderNbr" Width="100px" LinkCommand="ViewOrder"/>
                                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                                    <px:PXGridColumn DataField="LocationID" Width="120px" />
                                    <px:PXGridColumn DataField="InventoryID" Width="180px" />
                                    <px:PXGridColumn DataField="SOShipLine__TranDesc" Width="250px" />
                                    <px:PXGridColumn DataField="LotSerialNbr" Width="120px" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                    <px:PXGridColumn DataField="CartQty" />
                                    <px:PXGridColumn DataField="OverAllCartQty" />
                                    <px:PXGridColumn DataField="PickedQty" />
                                    <px:PXGridColumn DataField="PackedQty" />
                                    <px:PXGridColumn DataField="Qty" />
                                    <px:PXGridColumn DataField="UOM" />
                                    <px:PXGridColumn DataField="SOShipLine__IsFree" Type="CheckBox" Width="50px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edPackLineNbr" runat="server" DataField="LineNbr" Enabled="false"/>
                                    <px:PXTextEdit ID="edPackOrigOrderType" runat="server" DataField="SOShipLine__OrigOrderType" Enabled="False" />
                                    <px:PXSelector ID="edPackOrigOrderNbr" runat="server" DataField="SOShipLine__OrigOrderNbr" Enabled="False" />
                                    <px:PXSegmentMask ID="edPackInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edPackSiteID" runat="server" DataField="SiteID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edPackLocationID" runat="server" DataField="LocationID" Enabled="False"/>
                                    <px:PXTextEdit ID="edPackLotSerialNbr" runat="server" DataField="LotSerialNbr" Enabled="False" />
                                    <px:PXDateTimeEdit ID="edPackExpireDate" runat="server" DataField="ExpireDate" Enabled="False" />
                                    <px:PXNumberEdit ID="PXPackPickedQty" runat="server" DataField="PickedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXPackPackedQty" runat="server" DataField="PackedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXPackQty" runat="server" DataField="Qty" Enabled="False"/>
                                    <px:PXSelector ID="edPackUOM" runat="server" DataField="UOM" Enabled="False" />
                                    <px:PXCheckBox ID="chkPackIsFree" runat="server" DataField="SOShipLine__IsFree" Enabled="False" />
                                 </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                    <px:PXFormView ID="formBoxPackage" runat="server" DataSourceID="ds" DataMember="HeaderView" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                            <px:PXSelector ID="edPackage" runat="server" DataField="PackageLineNbrUI" CommitChanges="True" AutoRefresh="true"/>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
                            <px:PXFormView ID="formBoxInfo" runat="server" DataSourceID="ds" DataMember="ShownPackage" RenderStyle="Simple">
                                <Template>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
                                    <px:PXNumberEdit ID="edPackageWeight" runat="server" DataField="Weight" Enabled="false"/>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                                    <px:PXNumberEdit ID="edBoxMaxWeight" runat="server" DataField="MaxWeight" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XXS" ControlSize="XS" />
                                    <px:PXTextEdit ID="edWeightUOM" runat="server" DataField="WeightUOM"/>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="XS" />
                                    <px:PXCheckBox ID="chPackageConfirmed" runat="server" DataField="Confirmed" AlignLeft="True" Enabled="false"/>
                                </Template>
                            </px:PXFormView>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="gridPackedItems" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" Height="200px" Caption="Package Content" CaptionVisible="true" AllowPaging="False" >
                        <Levels>
                            <px:PXGridLevel DataMember="Packed">
                                <Columns>
                                    <px:PXGridColumn DataField="LineNbr" Width="50px" />
                                    <px:PXGridColumn DataField="InventoryID" Width="180px" />
                                    <px:PXGridColumn DataField="SOShipLine__TranDesc" Width="250px" />
                                    <px:PXGridColumn DataField="LotSerialNbr" Width="120px" />
                                    <px:PXGridColumn DataField="PackedQtyPerBox" />
                                    <px:PXGridColumn DataField="Qty" />
                                    <px:PXGridColumn DataField="UOM" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edPackedItemInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                                 </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Ship" VisibleExp="DataControls[&quot;chkShowShip&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXFormView ID="formShipAddress" runat="server" DataMember="Shipping_Address" DataSourceID="ds" AllowCollapse="true" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1"  Enabled="False"/>
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2"  Enabled="False"/>
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City"  Enabled="False"/>
                            <px:PXLayoutRule runat="server" StartColumn="True" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true"  Enabled="False"/>
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" Enabled="False"/>
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true"  Enabled="False"/>
                            <px:PXLayoutRule runat="server" StartColumn="True" />
                            <px:PXFormView ID="formShipInfo" runat="server" DataMember="CurrentDocument" DataSourceID="ds" CaptionVisible="False" RenderStyle="Simple">
                                <Template>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                                    <px:PXNumberEdit ID="edShipmentQty" runat="server" DataField="ShipmentQty" Enabled="False" />
                                    <px:PXNumberEdit ID="edShipmentWeight" runat="server" DataField="ShipmentWeight" Enabled="False" />
                                    <px:PXNumberEdit ID="edShipmentVolume" runat="server" DataField="ShipmentVolume" Enabled="False" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" />
                                    <px:PXNumberEdit ID="edPackageCount" runat="server" DataField="PackageCount" Enabled="False" />
                                    <px:PXNumberEdit ID="edPackageWeight" runat="server" DataField="PackageWeight" Enabled="False" />
                                </Template>
                                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                            </px:PXFormView>
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None" />
                    </px:PXFormView>
                    <px:PXGrid ID="gridRates" runat="server" Width="100%" DataSourceID="ds" Caption="Carrier Rates" SkinID="Details" Height="90px" CaptionVisible="True" AllowPaging="False" AllowFilter="False" AutoAdjustColumns="False" >
                        <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" />
                        <ActionBar Position="Top" PagerVisible="False" CustomItemsGroup="1" ActionsVisible="True">
                            <CustomItems>
                                <px:PXToolBarButton Text="Get Rates">
                                    <AutoCallBack Command="RefreshRates" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton CommandName="GetReturnLabels" CommandSourceID="ds"/>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="CarrierRates">
                                <Columns>
                                    <px:PXGridColumn DataField="Selected" Width="55px" Type="CheckBox" AutoCallBack="true" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Method" Label="Code" Width="85px" />
                                    <px:PXGridColumn DataField="Description" Label="Description" Width="120px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Amount" Width="85px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="DaysInTransit" Label="Days in Transit" Width="85px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="DeliveryDate" Label="Delivery Date" Width="85px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="50" />
                    </px:PXGrid>
                    <px:PXGrid ID="gridPackages" runat="server" Width="100%" DataSourceID="ds" Caption="Packages" SkinID="Details" Height="90px" CaptionVisible="True" AllowPaging="False">
                        <Levels>
                            <px:PXGridLevel DataMember="Packages">
                                <Columns>
                                    <px:PXGridColumn DataField="BoxID" Width="120px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
                                    <px:PXGridColumn DataField="WeightUOM" Width="55px" />
                                    <px:PXGridColumn DataField="Weight" Width="60px" />
                                    <px:PXGridColumn DataField="BoxWeight" Width="60px" />
                                    <px:PXGridColumn DataField="NetWeight" Width="60px" />
                                    <px:PXGridColumn DataField="MaxWeight" Width="60px" />
                                    <px:PXGridColumn DataField="DeclaredValue" Width="70px" />
                                    <px:PXGridColumn DataField="COD" Width="55px" />
                                    <px:PXGridColumn DataField="TrackNumber" Width="120px" />
                                    <px:PXGridColumn DataField="StampsAddOns" Width="200" Type="DropDownList" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXDropDown runat="server" ID="edStampsAddOns" DataField="StampsAddOns" AllowMultiSelect="True" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="50" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Scan Log" VisibleExp="DataControls[&quot;chkShowLog&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXGrid ID="grid4" runat="server" DataSourceID="ds" Style="height: 250px;" Width="100%" SkinID="Inquire" Height="372px" TabIndex="-7375" OnRowDataBound="LogGrid_RowDataBound">
                        <Levels>
                            <px:PXGridLevel DataMember="Logs">
                                <Columns>
                                    <px:PXGridColumn DataField="ScanTime" Width="160px" />
                                    <px:PXGridColumn DataField="Mode" Width="200px" />
                                    <px:PXGridColumn DataField="Prompt" Width="400px" />
                                    <px:PXGridColumn DataField="Scan" Width="400px" />
                                    <px:PXGridColumn DataField="Message" Width="400px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" Container="Window" />
    </px:PXTab>
    <%-- Settings --%>
    <px:PXSmartPanel ID="PanelSettings" runat="server" Caption="Settings" CaptionVisible="True" ShowAfterLoad="true"
        Key="UserSetupView" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="frmSettings" CloseButtonDialogResult="Abort">
        <px:PXFormView ID="frmSettings" runat="server" DataSourceID="ds" DataMember="UserSetupView" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="General"/>
                <px:PXCheckBox ID="edDefaultLocation" runat="server" DataField="DefaultLocationFromShipment" CommitChanges="true" />
                <px:PXCheckBox ID="edDefaultLotSerial" runat="server" DataField="DefaultLotSerialFromShipment" CommitChanges="true" />

                <px:PXLayoutRule ID="PXLayoutRule7" runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="Printing"/>
                <px:PXCheckBox ID="edPrintShipmentConfirmation" runat="server" DataField="PrintShipmentConfirmation" CommitChanges="true" />
                <px:PXCheckBox ID="edPrintShipmentLabels" runat="server" DataField="PrintShipmentLabels" CommitChanges="true" />

                <px:PXLayoutRule ID="PXLayoutRule4" runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="Scale"/>
                <px:PXCheckBox ID="edUseScale" runat="server" DataField="UseScale" CommitChanges="true" />
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" LabelsWidth="M" ControlSize="M" SuppressLabel="False"/>
                <px:PXSelector ID="edScaleID" runat="server" DataField="ScaleDeviceID" CommitChanges="true" AutoComplete="false" />

                <px:PXLayoutRule ID="PXLayoutRule8" runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True"/>
                <px:PXCheckBox ID="edEnterSizeForPackages" runat="server" DataField="EnterSizeForPackages" CommitChanges="true" />

            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="pbClose" runat="server" DialogResult="OK" Text="Save"/>
            <px:PXButton ID="pbCancel" runat="server" DialogResult="Abort" Text="Cancel"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:content>