<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN305010.aspx.cs"
    Inherits="Page_IN305010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INPICountEntry" PrimaryView="PIHeader" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="addLine" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="addLine2" PostData="Self" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Document Summary" DataMember="PIHeader"
        CaptionAlign="Justify" DefaultControlID="edPIID" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" ActivityIndicator="True"
        ActivityField="NoteActivity" TabIndex="100">
        <Searches>
            <px:PXControlParam ControlID="form" Name="PIID" PropertyName="NewDataKey[&quot;PIID&quot;]" Type="String" />
        </Searches>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector ID="edPIID" runat="server" DataField="PIID" DataKeyNames="PIID" DataSourceID="ds"
                MaxLength="15" />
            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" Enabled="False" DataSourceID="ds" />
            <px:PXFormView ID="PXFormView1" runat="server" DataMember="Filter" DataSourceID="ds" RenderStyle="Simple">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" DataSourceID="ds" />
                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" DataKeyNames="InventoryCD"
                        DataSourceID="ds"/>
                    <px:PXSegmentMask CommitChanges="True" ID="edSubItem" runat="server" DataField="SubItem" DataKeyNames="Value" DataSourceID="ds" />
                    <px:PXTextEdit CommitChanges="True" ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" />
                </Template>
            </px:PXFormView>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit ID="edCountDate" runat="server" DataField="CountDate" Enabled="False" />
            <px:PXNumberEdit ID="edLineCntr" runat="server" DataField="LineCntr" Enabled="False" />
            <px:PXFormView ID="formFilter" runat="server" DataMember="Filter" DataSourceID="ds" RenderStyle="Simple" 
                TabIndex="6100">
                <Template>
                    <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="M" />
                    <px:PXNumberEdit CommitChanges="True" ID="edStartLineNbr" runat="server" DataField="StartLineNbr" AllowNull="True" />
                    <px:PXNumberEdit CommitChanges="True" ID="edEndLineNbr" runat="server" DataField="EndLineNbr" AllowNull="True" />
                </Template>
            </px:PXFormView>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" Caption="Physical Inventory Details" SkinID="Inquire">
        <ActionBar PagerVisible="False">
            <CustomItems>
                <px:PXToolBarButton Text="Add Line" Key="cmdAddLine" CommandSourceID="ds" CommandName="AddLine">
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Levels>
            <px:PXGridLevel DataMember="PIDetail">
                <Columns>
                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                    <px:PXGridColumn DataField="LineNbr" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="TagNumber" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="InventoryID" />
                    <px:PXGridColumn DataField="InventoryID_InventoryItem_descr" />
                    <px:PXGridColumn DataField="SubItemID" />
                    <px:PXGridColumn DataField="LocationID" />
                    <px:PXGridColumn DataField="LotSerialNbr" />
                    <px:PXGridColumn DataField="BookQty" AllowShowHide="Server" AllowNull="False" TextAlign="Right" AllowUpdate="False" />
					<px:PXGridColumn DataField="InventoryItem__BaseUnit"/>
                    <px:PXGridColumn DataField="PhysicalQty" TextAlign="Right" />
                    <px:PXGridColumn DataField="VarQty" AllowShowHide="Server" TextAlign="Right" AllowUpdate="False" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
    <script type="text/javascript">
        function PanelAdd_Load() {
            px_callback.addHandler(_addLineHhandleCallback);
            var barcode = px_alls["edBarCodePnl"];
            var subitem = px_alls["edSubItemIDPnl"];
            var lotSerial = px_alls["edLotSerialNbrPnl"];
            var location = px_alls["edLocationIDPnl"];
            barcode.focus();
            barcode.events.addEventHandler("keyPress", _keypress);
            subitem.events.addEventHandler("keyPress", _keypress);
            lotSerial.events.addEventHandler("keyPress", _keypress);
            location.events.addEventHandler("keyPress", _keypress);
        }

        function _keypress(ctrl, ev) {
            var me = this, timeout = this._enterTimeoutID;
            if (timeout) clearTimeout(timeout);
            this._enterTimeoutID = setTimeout(function () {
                var autoAdd = px_alls["chkAutoAddLine"];
                if (autoAdd != null && autoAdd.getValue() == true)
                    ctrl.updateValue();
            }, 500);
        }

        function _addLineHhandleCallback(context, error) {
            var barcode = px_alls["edBarCodePnl"];
            if (context != null && context.info != null && context.info.name == "AddLine2" && barcode != null) {
                barcode.focus();
                return;
            }

            if (context == null || context.info == null || context.info.name != "Save" || !context.controlID.endsWith("_frmBarCode"))
                return;

            var item = px_alls["edInventoryIDPnl"];
            var subitem = px_alls["edSubItemIDPnl"];
            var lotSerial = px_alls["edLotSerialNbrPnl"];
            var location = px_alls["edLocationIDPnl"];
            var description = px_alls["edDescriptionPnl"];
            var expDate = px_alls["edExporeDatePnl"];

            if (description != null && description.getValue() != null)
                document.getElementById("audioDing").play();

            if (barcode != null && barcode.getValue() == null && barcode.getEnabled())
            { barcode.focus(); return; }

            if (item != null && item.getValue() == null && barcode != null)
            { barcode.elemText.select(); barcode.focus(); return; }

            if (subitem != null && subitem.getValue() == null && subitem.getEnabled())
            { subitem.focus(); return; }

            if (lotSerial != null && lotSerial.getValue() == null && lotSerial.getEnabled())
            { lotSerial.focus(); return; }

            if (location != null && location.getValue() == null && location.getEnabled())
            { location.focus(); return; }

            if (expDate != null && expDate.getValue() == null && expDate.getEnabled())
            { expDate.focus(); return; }
        }

    </script>
	<audio id="audioDing" src="../../Sounds/Ding.wav" preload="auto" style="visibility: hidden"></audio>
    <px:PXSmartPanel ID="PanelAddRL" runat="server" Style="z-index: 108;" Width="868px" Key="AddByBarCode" Caption="Add Line"
        CaptionVisible="True" LoadOnDemand="true" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="frmBarCode"
        DesignView="Content" ClientEvents-AfterLoad="PanelAdd_Load">
        <px:PXFormView ID="frmBarCode" runat="server" DataSourceID="ds" Height="173px" Style="z-index: 100" Width="100%" DataMember="AddByBarCode"
            SkinID="Transparent" CaptionVisible="False">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXTextEdit CommitChanges="True" ID="edBarCodePnl" runat="server" DataField="BarCode" />
                <px:PXSegmentMask CommitChanges="True" ID="edInventoryIDPnl" runat="server" DataField="InventoryID" />
                <px:PXSegmentMask CommitChanges="True" ID="edSubItemIDPnl" runat="server" DataField="SubItemID" />
                <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" InputMask="&gt;aaaaaa" Enabled="false" />
                <px:PXSegmentMask CommitChanges="True" ID="edLocationIDPnl" runat="server" DataField="LocationID" AutoRefresh="true" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXNumberEdit CommitChanges="True" ID="edReceiptQty" runat="server" DataField="Qty" />
                <px:PXTextEdit CommitChanges="True" ID="edLotSerialNbrPnl" runat="server" DataField="LotSerialNbr" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edExporeDatePnl" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                <px:PXCheckBox CommitChanges="True" ID="chkByOne" runat="server" Checked="True" DataField="ByOne" />
                <px:PXCheckBox CommitChanges="True" ID="chkAutoAddLine" runat="server" DataField="AutoAddLine" />
                <px:PXTextEdit ID="edDescriptionPnl" runat="server" DataField="Description" MaxLength="255" SkinID="Label" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton7" runat="server" Text="Add" CommandSourceID="ds" CommandName="AddLine2" SyncVisible="false"/>
            <px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="Add & Close" />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="No" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
