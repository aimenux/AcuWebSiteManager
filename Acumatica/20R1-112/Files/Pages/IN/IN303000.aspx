<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN303000.aspx.cs"
    Inherits="Page_IN303000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INAdjustmentEntry" PrimaryView="adjustment">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSINAdjustmentTran_generateLotSerial" PopupCommand="" PopupCommandTarget=""
                Visible="False" />
            <px:PXDSCallbackCommand Name="LSINAdjustmentTran_binLotSerial" PopupCommand="" PopupCommandTarget="" Visible="False" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="true" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="False" Name="INEdit" />
            <px:PXDSCallbackCommand Visible="False" Name="INRegisterDetails" />
            <px:PXDSCallbackCommand Name="InventorySummary" Visible="false" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="AddInvBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvSelBySite" Visible="False" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="adjustment" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edRefNbr">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
			<px:PXSelector ID="pIID" runat="server" DataField="PIID" DataSourceID="ds" Enabled="false" AllowEdit="True" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXPanel ID="PXPanel1" runat="server" ContentLayout-Layout="Stack" RenderSimple="True" RenderStyle="Simple">
                <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty" Enabled="False" />
                <px:PXNumberEdit CommitChanges="True" ID="edControlQty" runat="server" DataField="ControlQty" />
                <px:PXNumberEdit ID="edTotalCost" runat="server" DataField="TotalCost" Enabled="False" />
                <px:PXNumberEdit CommitChanges="True" ID="edControlCost" runat="server" DataField="ControlCost" />
            </px:PXPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="216px" Width="100%">
        <Items>
            <px:PXTabItem Text="Transaction Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="250px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        BorderWidth="0px" SkinID="Details" StatusField="Availability" SyncPosition="true">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Item" Key="cmdASI">
                                    <AutoCallBack Command="AddInvBySite" Target="ds">
                                        <Behavior PostData="Page" CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Inventory Summary">
                                    <AutoCallBack Command="InventorySummary" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="transactions">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask Size="xs" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true">
                                        <GridProperties>
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                    </px:PXSegmentMask>
                                    <px:PXSelector Size="s" ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" AutoRefresh="true" ValueField="ReceiptNbr">
                                        <GridProperties>
											<Columns>
												<px:PXGridColumn DataField="ReceiptNbr">
													<Header Text="Receipt Nbr"/>
												</px:PXGridColumn>
												<px:PXGridColumn DataField="ReceiptDate" DataType="DateTime">
													<Header Text="Receipt Date"/>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyOnHand" DataType="Decimal" >
													<Header Text="Quantity On Hand"/>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TotalCost" DataType="Decimal" >
													<Header Text="Total Cost"/>
												</px:PXGridColumn>
											</Columns>
											<Layout ColumnsMenu="False" />
										 <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSelector Size="s" ID="edReasonCode" runat="server" DataField="ReasonCode">
                                        <Parameters>
                                            <px:PXControlParam ControlID="form" Name="INTran.docType" PropertyName="NewDataKey[&quot;DocType&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="true" NullText="<SPLIT>">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true" NullText="<SPLIT>">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" />
                                    <px:PXNumberEdit ID="edTranCost" runat="server" DataField="TranCost" />
                                    <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" NullText="<SPLIT>" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                                            <px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" />
                                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXSelector ID="edSOShipmentNbr" runat="server" AllowEdit="True" DataField="SOShipmentNbr" />
                                    <px:PXSelector ID="edPOReceiptNbr" runat="server" DataField="POReceiptNbr" AllowEdit="True" />
                                    <px:PXSelector ID="edSOOrderType" runat="server" DataField="SOOrderType" />
                                    <px:PXSelector ID="edSOOrderNbr" runat="server" DataField="SOOrderNbr" AllowEdit="True" />
                                    <px:PXSegmentMask CommitChanges="True" Height="19px" ID="edBranchID" runat="server" DataField="BranchID" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="PILineNbr" Width="67px" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubItemID" NullText="<SPLIT>" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="SiteID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LocationID" NullText="<SPLIT>" AutoCallBack="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                                    <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="TranCost" TextAlign="Right" />
									<px:PXGridColumn DataField="ManualCost" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="LotSerialNbr" NullText="<SPLIT>" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                    <px:PXGridColumn DataField="OrigRefNbr" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ReasonCode" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="SOOrderType" />
                                    <px:PXGridColumn DataField="SOOrderNbr" />
                                    <px:PXGridColumn DataField="SOShipmentNbr" />
                                    <px:PXGridColumn DataField="POReceiptNbr" Visible="False" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXFormView ID="form2" runat="server" DataSourceID="ds" Width="100%" DataMember="CurrentDocument"
                        CaptionVisible="False">
                        <ContentStyle BorderWidth="0px">
                        </ContentStyle>
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                            <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
                            <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                        </Template>
                        <AutoSize Enabled="True" />
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <script type="text/javascript">
        function UpdateItemSiteCell(n, c) {
            var activeRow = c.cell.row;
            var sCell = activeRow.getCell("Selected");
            var qCell = activeRow.getCell("QtySelected");
            if (sCell == c.cell) {
                if (sCell.getValue() == true)
                    qCell.setValue("1");
                else
                    qCell.setValue("0");
            }
            if (qCell == c.cell) {
                if (qCell.getValue() == "0")
                    sCell.setValue(false);
                else
                    sCell.setValue(true);
            }
        }
    </script>
    <px:PXSmartPanel ID="PanelAddSiteStatus" runat="server" Key="sitestatus" LoadOnDemand="true" Width="900px" Height="500px"
        Caption="Inventory Lookup" CaptionVisible="true" AutoCallBack-Command='Refresh' AutoCallBack-Enabled="True" AutoCallBack-Target="formSitesStatus"
        DesignView="Content">
        <px:PXFormView ID="formSitesStatus" runat="server" CaptionVisible="False" DataMember="sitestatusfilter" DataSourceID="ds"
            Width="100%" SkinID="Transparent">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXTextEdit ID="edInventory" runat="server" DataField="Inventory" />
                <px:PXTextEdit CommitChanges="True" ID="edBarCode" runat="server" DataField="BarCode" />
                <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClass" />
                <px:PXCheckBox CommitChanges="True" ID="chkOnlyAvailable" runat="server" Checked="True" DataField="OnlyAvailable" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True" ID="edSubItem" runat="server" DataField="SubItem" AutoRefresh="true" />
                <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="true" />
                <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true" /></Template>
        </px:PXFormView>
        <px:PXGrid ID="gripSiteStatus" runat="server" DataSourceID="ds" Style="border-width: 1px 0px; top: 0px; left: 0px; height: 189px;"
            AutoAdjustColumns="true" Width="100%" SkinID="Details" AdjustPageSize="Auto" AllowSearch="True" FastFilterID="edInventory"
            FastFilterFields="InventoryCD,Descr" BatchUpdate="true">
            <ClientEvents AfterCellUpdate="UpdateItemSiteCell" />
            <ActionBar PagerVisible="False">
                <PagerSettings Mode="NextPrevFirstLast" />
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="siteStatus">
                    <Mode AllowAddNew="false" AllowDelete="false" />
                    <RowTemplate>
                        <px:PXSegmentMask ID="editemClass" runat="server" DataField="ItemClassID" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true"
                            AllowCheckAll="true" />
                        <px:PXGridColumn AllowNull="False" DataField="QtySelected" TextAlign="Right" />
                        <px:PXGridColumn DataField="SiteID" />
						<px:PXGridColumn DataField="SiteCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
                        <px:PXGridColumn DataField="LocationID" />
						<px:PXGridColumn DataField="LocationCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
                        <px:PXGridColumn DataField="ItemClassID" />
                        <px:PXGridColumn DataField="ItemClassDescription" />
                        <px:PXGridColumn DataField="PriceClassID" />
                        <px:PXGridColumn DataField="PriceClassDescription" />
                        <px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
						<px:PXGridColumn DataField="SubItemCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
                        <px:PXGridColumn DataField="Descr" />
                        <px:PXGridColumn DataField="BaseUnit" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyAvail" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyOnHand" TextAlign="Right" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" CommandName="AddInvSelBySite" CommandSourceID="ds" Text="Add" SyncVisible="false"/>
            <px:PXButton ID="PXButton4" runat="server" Text="Add & Close" DialogResult="OK" />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
