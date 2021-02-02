<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN304020.aspx.cs"
    Inherits="Page_IN304020" Title="Scan and Transfer" %>

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
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INScanTransferHost" PrimaryView="HeaderView">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Cancel" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ClearBtn" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanQty" Visible="true" CommitChanges="True" />
            
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
			<px:PXDSCallbackCommand Name="ScanRelease" Visible="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Review" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="UserSetupDialog" CommitChanges="True" />

			<px:PXDSCallbackCommand Name="LSINTran_generateLotSerial" Visible="false" />
			<px:PXDSCallbackCommand Name="LSINTran_binLotSerial" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
    <px:PXFormView ID="formHeader" runat="server" DataSourceID="ds" Height="120px" Width="100%" Visible="true" DataMember="HeaderView" DefaultControlID="edBarcode">
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
            <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" CommitChanges="true" AutoRefresh="true" AutoComplete="false" Enabled="false" />
            <px:PXSelector ID="edToSiteID" runat="server" DataField="ToSiteID" CommitChanges="true" AutoRefresh="true" AutoComplete="false" Enabled="false" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" ColumnWidth="M" />
            <px:PXTextEdit ID="edMessage" runat="server" DataField="Message" Width="800px" Style="font-size: 10pt; font-weight: bold;" SuppressLabel="true" TextMode="MultiLine" Height="55px" SkinID="Label" DisableSpellcheck="True" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" Merge="True" />
			<px:PXCheckBox ID="chkCartLoaded" runat="server" DataField="CartLoaded" AlignLeft="True" />
            <px:PXTextEdit ID="edMode" runat="server" DataField="Mode"/>
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
            <px:PXTabItem Text="Transfer">
                <Template>
                    <px:PXGrid ID="gridPicked" runat="server" DataSourceID="ds" SyncPosition="true" Width="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="transactions">
                                <Columns>
									<px:PXGridColumn DataField="InventoryID" Width="120px" />
									<px:PXGridColumn DataField="TranDesc" Width="150px" />
									<px:PXGridColumn DataField="LocationID" Width="108px" />
									<px:PXGridColumn DataField="ToLocationID" Width="108px" />
									<px:PXGridColumn DataField="LotSerialNbr" Width="95px" />
									<px:PXGridColumn DataField="ExpireDate" Width="70px" />
									<px:PXGridColumn DataField="ReasonCode" Width="90px" />
									<px:PXGridColumn DataField="CartQty" Width="80px"/>
									<px:PXGridColumn DataField="Qty" Width="65px" TextAlign="Right" />
									<px:PXGridColumn DataField="UOM" Width="65px" />
                                </Columns>
                                <RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edToLocationID" runat="server" DataField="ToLocationID" AutoRefresh="True" />
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
									<px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" />
									<px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" AutoRefresh="True" />
									<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" />
								</RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                         <AutoSize Enabled="True" />
                        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Scan Log">
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
	<px:PXSmartPanel ID="PanelSettings" runat="server" Caption="Settings" CaptionVisible="True" ShowAfterLoad="true"
        Key="UserSetupView" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="frmSettings" CloseButtonDialogResult="Abort">
        <px:PXFormView ID="frmSettings" runat="server" DataSourceID="ds" DataMember="UserSetupView" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="General"/>
                <px:PXCheckBox ID="edDefaultWarehouse" runat="server" DataField="DefaultWarehouse" CommitChanges="true" />

                <px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" StartGroup="True" SuppressLabel="True" GroupCaption="Scale"/>
                <px:PXCheckBox ID="edUseScale" runat="server" DataField="UseScale" CommitChanges="true" />
                <px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" SuppressLabel="False"/>
                <px:PXSelector ID="edScaleID" runat="server" DataField="ScaleDeviceID" CommitChanges="true" AutoComplete="false" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="pbClose" runat="server" DialogResult="OK" Text="Save"/>
            <px:PXButton ID="pbCancel" runat="server" DialogResult="Abort" Text="Cancel"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:content>