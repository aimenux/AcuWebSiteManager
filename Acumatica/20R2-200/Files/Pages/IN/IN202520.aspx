<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN202520.aspx.cs"
    Inherits="Page_IN202520" Title="Item Lookup" %>

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
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.InventoryItemLookupHost" PrimaryView="HeaderView">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" Visible="false" />
            <px:PXDSCallbackCommand Name="Refresh" Visible="false" />
            <px:PXDSCallbackCommand Name="Scan" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModePick" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModePack" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeShip" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModeReceive" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ScanModePutAway" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ReviewAvailability" Visible="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ScanModeItemLookup" Visible="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ScanModeStorageLookup" Visible="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ScanModeIssue" Visible="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ScanModeInReceive" Visible="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ScanModeInTransfer" Visible="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ScanModePhysicalCount" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Cancel" Visible="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ClearBtn" Visible="false" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
    <px:PXFormView ID="formHeader" runat="server" DataSourceID="ds" Height="120px" Width="100%" Visible="true" DataMember="HeaderView" DefaultControlID="edBarcode" FilesIndicator="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
            <px:PXTextEdit ID="edBarcode" runat="server" DataField="Barcode" CommitChanges="true">
                <AutoCallBack Command="Scan" Target="ds">
                    <Behavior CommitChanges="True" />
                </AutoCallBack>
                <ClientEvents Initialize="Barcode_Initialize"/>
            </px:PXTextEdit>
            <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="true" />
            <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" ColumnWidth="M" />
            <px:PXTextEdit ID="edMessage" runat="server" DataField="Message" Width="800px" Style="font-size: 10pt; font-weight: bold;" SuppressLabel="true" TextMode="MultiLine" Height="55px" SkinID="Label" DisableSpellcheck="True" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" Merge="True" />
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
            <px:PXTabItem Text="Item">
                <Template>
                    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="InventoryItem">
                        <Template>
                            
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" Enabled="false" />
                            <px:PXDropDown ID="edItemType" runat="server" DataField="ItemType" Enabled="false" />
                            <px:PXCheckBox SuppressLabel="True" ID="chkKitItem" runat="server" DataField="KitItem" Enabled="false" />
                            <px:PXDropDown CommitChanges="True" ID="edValMethod" runat="server" DataField="ValMethod" Enabled="false" />
                            <px:PXSelector CommitChanges="True" ID="edLotSerClassID" runat="server" DataField="LotSerClassID" Enabled="false" />
                            
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector CommitChanges="True" ID="edProductWorkgroupID" runat="server" DataField="ProductWorkgroupID" Enabled="false" />
                            <px:PXSelector ID="edProductManagerID" runat="server" DataField="ProductManagerID" AutoRefresh="True" Enabled="false" />
                            <px:PXSelector ID="edBaseUnit" Size="s" runat="server" CommitChanges="True" DataField="BaseUnit" Enabled="false" />
                            <px:PXSelector ID="edSalesUnit" Size="s" runat="server" AutoRefresh="True" CommitChanges="True" DataField="SalesUnit" Enabled="false" />
                            <px:PXSelector ID="edPurchaseUnit" Size="s" runat="server" AutoRefresh="True" CommitChanges="True" DataField="PurchaseUnit" Enabled="false" />

                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" Caption="Inventory Summary" BatchUpdate="True" SkinID="PrimaryInquire"
                        TabIndex="8" RestrictFields="True" SyncPosition="true" FastFilterFields="InventoryID,SiteID">
                        <Levels>
                            <px:PXGridLevel DataMember="ISERecords">
                                <Columns>
                                    <px:PXGridColumn DataField="LocationID" AllowShowHide="Server" DisplayFormat="&gt;AAAAAAAAAA" AllowNull="True" NullText="<UNASSIGNED>" Width="150px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="QtyAvail" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="QtyHardAvail" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="QtyInTransit" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="QtyExpired" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="QtyOnHand" DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="BaseUnit" DisplayFormat="&gt;aaaaaa" />
                                    <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="120px" />
                                    <px:PXGridColumn DataField="ExpireDate" DataType="DateTime" Width="90px" AllowShowHide="Server" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar DefaultAction="ViewAllocDet"/>
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
</asp:content>