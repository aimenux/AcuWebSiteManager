<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN305000.aspx.cs"
    Inherits="Page_IN305000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="PIHeader" TypeName="PX.Objects.IN.INPIReview"
		PageLoadBehavior="GoLastRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="FinishCounting" CommitChanges="True" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="CompletePI" CommitChanges="True" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="addLine" PostData="Self" Visible="false" />
			<px:PXDSCallbackCommand Name="addLine2" PostData="Self" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="Document Summary" DataMember="PIHeader" DataSourceID="ds" NoteIndicator="True"
        FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edPIID" TabIndex="1900">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edPIID" runat="server" DataField="PIID" AutoRefresh="True" DataSourceID="ds" />
            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
            <px:PXDateTimeEdit ID="edCountDate" runat="server" DataField="CountDate" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXFormView runat="server" ID="formDetail" DataSourceID="ds" DataMember="PIHeaderInfo" TabIndex="100" RenderStyle="Simple">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXNumberEdit ID="edTotalPhysicalQty" runat="server" DataField="TotalPhysicalQty" Enabled="False" />
                    <px:PXNumberEdit ID="edTotalVarQty" runat="server" DataField="TotalVarQty" Enabled="False" />
                    <px:PXNumberEdit ID="edTotalVarCost" runat="server" DataField="TotalVarCost" Enabled="False" />
                </Template>
                
            </px:PXFormView>
        </Template>
    </px:PXFormView>
    <px:PXSmartPanel ID="insPanel" runat="server" Width="620px" Caption="Generate Physical Count" DesignView="Content" CaptionVisible="True"
        Key="GeneratorSettings" AutoCallBack-Command="Refresh" AutoCallBack-Target="generateform">
        <px:PXFormView runat="server" Width="100%" ID="generateform" CaptionVisible="False" DataMember="GeneratorSettings" DataSourceID="ds"
            SkinID="Transparent" TabIndex="2500">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXSelector CommitChanges="True" ID="edPIClassID" runat="server" DataField="PIClassID" DataSourceID="ds" />
                <px:PXTextEdit ID="edPIDescr" runat="server" DataField="Descr" />
                <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" DataSourceID="ds">
                    <AutoCallBack Command="Save" Target="generateform" />
                </px:PXSegmentMask>
                <px:PXDropDown ID="edMethod" runat="server" AllowNull="False" DataField="Method" Enabled="False" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton4" runat="server" DialogResult="OK" Text="Ok"/>
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel"/>
        </px:PXPanel>
    </px:PXSmartPanel>
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
        CaptionVisible="True" LoadOnDemand="True" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmBarCode" DesignView="Content" ClientEvents-AfterLoad="PanelAdd_Load">
        <px:PXFormView ID="frmBarCode" runat="server" DataSourceID="ds" Height="173px" Style="z-index: 100" Width="100%" DataMember="AddByBarCode"
            SkinID="Transparent" CaptionVisible="False" TabIndex="2900">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXTextEdit ID="edBarCodePnl" runat="server" CommitChanges="True" DataField="BarCode" />
                <px:PXSegmentMask ID="edInventoryIDPnl" runat="server" CommitChanges="True" DataField="InventoryID" />
                <px:PXSegmentMask ID="edSubItemIDPnl" runat="server" CommitChanges="True" DataField="SubItemID" AutoRefresh="true" />
                <px:PXSelector ID="edUOM" runat="server" CommitChanges="True" DataField="UOM" Enabled="False" InputMask="&gt;aaaaaa" />
                <px:PXSegmentMask ID="edLocationIDPnl" runat="server" AutoRefresh="True" CommitChanges="True" DataField="LocationID" />
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXNumberEdit ID="edReceiptQty" runat="server" CommitChanges="True" DataField="Qty" />
                <px:PXTextEdit ID="edLotSerialNbrPnl" runat="server" CommitChanges="True" DataField="LotSerialNbr" />
                <px:PXDateTimeEdit ID="edExporeDatePnl" runat="server" CommitChanges="True" DataField="ExpireDate" DisplayFormat="d" />
                <px:PXCheckBox ID="chkByOne" runat="server" Checked="True" CommitChanges="True" DataField="ByOne" />
                <px:PXCheckBox ID="chkAutoAddLine" runat="server" CommitChanges="True" DataField="AutoAddLine" />
                <px:PXTextEdit ID="edDescriptionPnl" runat="server" DataField="Description" SkinID="Label" SuppressLabel="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton7" runat="server" CommandName="AddLine2" CommandSourceID="ds" Text="Add" />
            <px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="Add &amp; Close" />
            <px:PXButton ID="PXButton1" runat="server" DialogResult="No" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="270px" Style="z-index: 100;" Width="100%">
        <Items>
            <px:PXTabItem Text="Physical Inventory Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" BorderWidth="0px" SkinID="DetailsInTab" Style="height: 240px;"
                        AllowPaging="True" AdjustPageSize="Auto" SyncPosition="true">
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Line" Key="cmdAddLine" CommandSourceID="ds" CommandName="AddLine">
                                    <%--<Images Normal="PX.Web.UI.Images.Data.addNew.gif" Disabled="PX.Web.UI.Images.Data.addNewD.gif" />--%>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="PIDetail">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowAddNew="True" AllowEdit="True">
                                        <GridProperties>
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INPIDetail.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INPIHeader.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode" AutoRefresh="true"/>
								</RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
                                    <px:PXGridColumn DataField="TagNumber" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="InventoryID_InventoryItem_descr" />
                                    <px:PXGridColumn DataField="SubItemID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LocationID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LotSerialNbr" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ExpireDate" Label="Expiry Date" />
                                    <px:PXGridColumn DataField="BookQty" TextAlign="Right" />
									<px:PXGridColumn DataField="InventoryItem__BaseUnit"/>
                                    <px:PXGridColumn DataField="PhysicalQty" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="VarQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UnitCost" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="ExtVarCost" TextAlign="Right" />
                                    <px:PXGridColumn DataField="FinalExtVarCost" TextAlign="Right" />
									<px:PXGridColumn DataField="ManualCost" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ReasonCode" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpload="True" InitNewRow="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Adjustment Info">
                <Template>
                    <px:PXFormView ID="form2" runat="server" DataSourceID="ds" Width="100%" DataMember="PIHeaderInfo" RenderStyle="Normal" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXSelector ID="edPIAdjRefNbr" runat="server" DataField="PIAdjRefNbr" Enabled="False" AllowEdit="true" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="240" />
    </px:PXTab>
</asp:Content>
