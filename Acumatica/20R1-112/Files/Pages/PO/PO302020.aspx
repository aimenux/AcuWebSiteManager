<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO302020.aspx.cs"
    Inherits="Page_PO302020" Title="Receive and Put Away" %>

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
        }
        
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
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PO.ReceivePutAwayHost" PrimaryView="HeaderView">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Cancel" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ClearBtn" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanQty" Visible="true" CommitChanges="True" />

            <px:PXDSCallbackCommand Name="LSPOReceiptLine_generateLotSerial" Visible="False" />
            <px:PXDSCallbackCommand Name="LSPOReceiptLine_binLotSerial" Visible="False" />

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
            <px:PXDSCallbackCommand Name="ScanReleaseReceipt" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanCompletePOLines" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanReleaseTransfer" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ReviewReceive" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ReviewPutAway" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="UserSetupDialog" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grid2" Name="ViewOrder" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="grid2" Name="ViewTransferInfo" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
    <px:PXFormView ID="formHeader" runat="server" DataSourceID="ds" Height="120px" Width="100%" Visible="true" DataMember="HeaderView" DefaultControlID="edBarcode" FilesIndicator="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
            <px:PXTextEdit ID="edBarcode" runat="server" DataField="Barcode">
                <AutoCallBack Command="Scan" Target="ds">
                    <Behavior CommitChanges="True" />
                </AutoCallBack>
                <ClientEvents Initialize="Barcode_Initialize"/>
            </px:PXTextEdit>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" CommitChanges="true" AutoRefresh="true" AutoComplete="false" AllowEdit="true" />
            <px:PXSelector ID="edCartID" runat="server" DataField="CartID" CommitChanges="true" AutoRefresh="true" AutoComplete="false" />
            <px:PXSelector ID="edTransferRefNbr" runat="server" DataField="TransferRefNbr" CommitChanges="true" AutoRefresh="true" AutoComplete="false"  AllowEdit="true"/>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" ColumnWidth="M" />
            <px:PXTextEdit ID="edMessage" runat="server" DataField="Message" Width="800px" Style="font-size: 10pt; font-weight: bold;" SuppressLabel="true" TextMode="MultiLine" Height="55px" SkinID="Label" DisableSpellcheck="True" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="3" Merge="True" />
            <px:PXCheckBox ID="chkManualView" runat="server" DataField="ManualView" CommitChanges="true" AlignLeft="True" />
            <px:PXCheckBox ID="chkRemove" runat="server" DataField="Remove" AlignLeft="True" />
            <px:PXCheckBox ID="chkCartLoaded" runat="server" DataField="CartLoaded" AlignLeft="True" />
            <px:PXTextEdit ID="edMode" runat="server" DataField="Mode"/>
            <px:PXCheckBox ID="chkShowReceive" runat="server" DataField="ShowReceive" />
            <px:PXCheckBox ID="chkShowPutAway" runat="server" DataField="ShowPutAway" />
            <px:PXCheckBox ID="chkShowLog" runat="server" DataField="ShowLog" />
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
            <px:PXTabItem Text="Receive" VisibleExp="DataControls[&quot;chkShowReceive&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXGrid ID="gridReceived" runat="server" DataSourceID="ds" Style="height: 250px;" Width="100%" SkinID="Inquire" Height="372px" OnRowDataBound="ReceiveGrid_RowDataBound">
                        <Levels>
                            <px:PXGridLevel DataMember="Received">
                                <Columns>
                                    <px:PXGridColumn DataField="LineNbr" Width="50px" />
                                    <px:PXGridColumn DataField="POReceiptLine__POType" />
                                    <px:PXGridColumn DataField="POReceiptLine__PONbr" Width="100px" LinkCommand="ViewOrder"/>
                                    <px:PXGridColumn DataField="InventoryID" Width="180px" />
                                    <px:PXGridColumn DataField="POReceiptLine__TranDesc" Width="250px" />
                                    <px:PXGridColumn DataField="LotSerialNbr" Width="120px" />
                                    <px:PXGridColumn DataField="ExpireDate" Width="120px" />
                                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                                    <px:PXGridColumn DataField="LocationID" Width="120px" />
                                    <px:PXGridColumn DataField="ReceivedQty" Width="80px"/>
                                    <px:PXGridColumn DataField="Qty" Width="80px"/>
                                    <px:PXGridColumn DataField="RestQty" Width="80px"/>
                                    <px:PXGridColumn DataField="UOM" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edReceive_LineNbr" runat="server" DataField="LineNbr" Enabled="false"/>
                                    <px:PXTextEdit ID="edReceive_OrigOrderType" runat="server" DataField="POReceiptLine__POType" Enabled="False" />
                                    <px:PXSelector ID="edReceive_OrigOrderNbr" runat="server" DataField="POReceiptLine__PONbr" Enabled="False" />
                                    <px:PXSegmentMask ID="edReceive_InventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edReceive_SiteID" runat="server" DataField="SiteID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edReceive_LocationID" runat="server" DataField="LocationID" Enabled="True"/>
                                    <px:PXSelector ID="edReceive_LotSerialNbr" runat="server" DataField="LotSerialNbr" Enabled="False" />
                                    <px:PXNumberEdit ID="PXReceive_ReceiveQty" runat="server" DataField="ReceivedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXReceive_Qty" runat="server" DataField="Qty" Enabled="False"/>
                                    <px:PXSelector ID="edReceive_UOM" runat="server" DataField="UOM" Enabled="False" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Received Above Zero" BindingContext="formHeader" Visible="False">
                <Template>
                    <px:PXGrid ID="gridReceivedNotZero" runat="server" DataSourceID="ds" Style="height: 250px;" Width="100%" SkinID="Inquire" Height="372px">
                        <Levels>
                            <px:PXGridLevel DataMember="ReceivedNotZero">
                                <Columns>
                                    <px:PXGridColumn DataField="LineNbr" Width="50px" />
                                    <px:PXGridColumn DataField="POReceiptLine__POType" />
                                    <px:PXGridColumn DataField="POReceiptLine__PONbr" Width="100px" LinkCommand="ViewOrder"/>
                                    <px:PXGridColumn DataField="InventoryID" Width="180px" />
                                    <px:PXGridColumn DataField="POReceiptLine__TranDesc" Width="250px" />
                                    <px:PXGridColumn DataField="LotSerialNbr" Width="120px" />
                                    <px:PXGridColumn DataField="ExpireDate" Width="120px" />
                                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                                    <px:PXGridColumn DataField="LocationID" Width="120px" />
                                    <px:PXGridColumn DataField="ReceivedQty" Width="80px"/>
                                    <px:PXGridColumn DataField="Qty" Width="80px"/>
                                    <px:PXGridColumn DataField="RestQty" Width="80px"/>
                                    <px:PXGridColumn DataField="UOM" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edReceive_LineNbr1" runat="server" DataField="LineNbr" Enabled="false"/>
                                    <px:PXTextEdit ID="edReceive_OrigOrderType1" runat="server" DataField="POReceiptLine__POType" Enabled="False" />
                                    <px:PXSelector ID="edReceive_OrigOrderNbr1" runat="server" DataField="POReceiptLine__PONbr" Enabled="False" />
                                    <px:PXSegmentMask ID="edReceive_InventoryID1" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edReceive_SiteID1" runat="server" DataField="SiteID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edReceive_LocationID1" runat="server" DataField="LocationID" Enabled="True"/>
                                    <px:PXSelector ID="edReceive_LotSerialNbr1" runat="server" DataField="LotSerialNbr" Enabled="False" />
                                    <px:PXNumberEdit ID="PXReceive_ReceiveQty1" runat="server" DataField="ReceivedQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXReceive_Qty1" runat="server" DataField="Qty" Enabled="False"/>
                                    <px:PXSelector ID="edReceive_UOM1" runat="server" DataField="UOM" Enabled="False" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Put Away" VisibleExp="DataControls[&quot;chkShowPutAway&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Style="height: 250px;" Width="100%" SkinID="Inquire" Height="372px" OnRowDataBound="PutAwayGrid_RowDataBound">
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Transfer Allocations" CommandName="ViewTransferInfo" CommandSourceID="ds" DependOnGrid="grid2" />
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="PutAway">
                                <Columns>
                                    <px:PXGridColumn DataField="Fits" Width="50px" Type="CheckBox" />
                                    <px:PXGridColumn DataField="LineNbr" Width="50px" />
                                    <px:PXGridColumn DataField="POReceiptLine__POType" />
                                    <px:PXGridColumn DataField="POReceiptLine__PONbr" Width="100px" LinkCommand="ViewOrder"/>
                                    <px:PXGridColumn DataField="InventoryID" Width="180px" />
                                    <px:PXGridColumn DataField="POReceiptLine__TranDesc" Width="250px" />
                                    <px:PXGridColumn DataField="LotSerialNbr" Width="120px" />
                                    <px:PXGridColumn DataField="ExpireDate" Width="120px" />
                                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                                    <px:PXGridColumn DataField="LocationID" Width="120px" />
                                    <px:PXGridColumn DataField="ToLocationID" Width="120px"/>
                                    <px:PXGridColumn DataField="PutAwayQty" Width="80px"/>
                                    <px:PXGridColumn DataField="CartQty" Width="80px"/>
                                    <px:PXGridColumn DataField="OverallCartQty" Width="80px"/>
                                    <px:PXGridColumn DataField="Qty" Width="80px"/>
                                    <px:PXGridColumn DataField="UOM" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edPut_LineNbr" runat="server" DataField="LineNbr" Enabled="false"/>
                                    <px:PXTextEdit ID="edPut_OrigOrderType" runat="server" DataField="POReceiptLine__POType" Enabled="False" />
                                    <px:PXSelector ID="edPut_OrigOrderNbr" runat="server" DataField="POReceiptLine__PONbr" Enabled="False" />
                                    <px:PXSegmentMask ID="edPut_InventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edPut_SiteID" runat="server" DataField="SiteID" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask ID="edPut_LocationID" runat="server" DataField="LocationID" Enabled="False"/>
                                    <px:PXSelector ID="edPut_LotSerialNbr" runat="server" DataField="LotSerialNbr" Enabled="False" />
                                    <px:PXNumberEdit ID="PXPut_PutAwayQty" runat="server" DataField="PutAwayQty" Enabled="False"/>
                                    <px:PXNumberEdit ID="PXPut_Qty" runat="server" DataField="Qty" Enabled="False"/>
                                    <px:PXSelector ID="edPut_UOM" runat="server" DataField="UOM" Enabled="False" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Transfers" VisibleExp="DataControls[&quot;chkShowPutAway&quot;].Value == true" BindingContext="formHeader">
                <Template>
                    <px:PXGrid ID="formTransfers" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None">
                        <Levels>
                            <px:PXGridLevel DataMember="RelatedTransfers" >
                                <RowTemplate>
                                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" AllowEdit="True" edit="1" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TransferType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TranDate" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="FinPeriodID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="SiteID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TotalQty" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="BatchNbr" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                        </ActionBar>
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
                <px:PXLayoutRule ID="PXLayoutRule7" runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="General"/>
                <px:PXCheckBox ID="edDefaultLotSerialNumber" runat="server" DataField="DefaultLotSerialNumber" CommitChanges="true" />
                <px:PXCheckBox ID="edDefaultExpireDate" runat="server" DataField="DefaultExpireDate" CommitChanges="true" />
                <px:PXCheckBox ID="edSingleLocation" runat="server" DataField="SingleLocation" CommitChanges="true" />

                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="Printing"/>
                <px:PXCheckBox ID="edPrintInventoryLabelsAutomatically" runat="server" DataField="PrintInventoryLabelsAutomatically" CommitChanges="true" />
                <px:PXSelector ID="edInventoryLabelsReportID" runat="server" DataField="InventoryLabelsReportID" ValueField="ScreenID" />
                <px:PXCheckBox ID="edPrintPurchaseReceiptAutomatically" runat="server" DataField="PrintPurchaseReceiptAutomatically" CommitChanges="true" />

                <px:PXLayoutRule ID="PXLayoutRule4" runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="Scale"/>
                <px:PXCheckBox ID="edUseScale" runat="server" DataField="UseScale" CommitChanges="true" />
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" LabelsWidth="M" ControlSize="M" SuppressLabel="False"/>
                <px:PXSelector ID="edScaleID" runat="server" DataField="ScaleDeviceID" CommitChanges="true" AutoComplete="false" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="pbClose" runat="server" DialogResult="OK" Text="Save"/>
            <px:PXButton ID="pbCancel" runat="server" DialogResult="Abort" Text="Cancel"/>
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Transfer Allocations --%>
    <px:PXSmartPanel ID="PanelTransfers" runat="server" Width="764px" Caption="Transfer Allocations" DesignView="Hidden" CaptionVisible="True" Key="TransferSplitLinks" Height="500px" LoadOnDemand="True" AutoReload="True" AutoRepaint="True">
        <px:PXGrid ID="gridTransfers" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" SyncPosition="True">
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AutoInsert="False" InitNewRow="False" InplaceInsert="False" />
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="TransferSplitLinks">
                    <Columns>
                        <px:PXGridColumn DataField="TransferRefNbr" Width="108px" />
                        <px:PXGridColumn DataField="INTran__InventoryID" Width="108px" />
                        <px:PXGridColumn DataField="INTran__SubItemID" Width="108px" />
                        <px:PXGridColumn DataField="INTranSplit__LocationID" Width="108px" />
                        <px:PXGridColumn DataField="INTran__ToLocationID" Width="108px" />
                        <px:PXGridColumn DataField="INTranSplit__LotSerialNbr" Width="108px" />
                        <px:PXGridColumn DataField="Qty" Width="108px" TextAlign="Right" />
                        <px:PXGridColumn DataField="INTranSplit__UOM" Width="108px" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSelector ID="PXRefNbr" runat="server" DataField="TransferRefNbr" AutoRefresh="true" AllowEdit="true"/>
                        <px:PXSegmentMask ID="edPackedItemInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true"/>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="true"/>
                        <px:PXSegmentMask ID="edToLocationID2" runat="server" DataField="ToLocationID" AutoRefresh="true"/>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="true"/>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true"/>
                        <px:PXDateTimeEdit ID="edExpireDate2" runat="server" DataField="ExpireDate" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:content>