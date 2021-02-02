<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN304000.aspx.cs"
    Inherits="Page_IN304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INTransferEntry" PrimaryView="transfer">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="true" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
            <px:PXDSCallbackCommand Visible="False" Name="INEdit" />
            <px:PXDSCallbackCommand Visible="False" Name="INRegisterDetails" />
            <px:PXDSCallbackCommand Name="InventorySummary" Visible="false" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINTran_generateLotSerial" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSINTran_binLotSerial" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="AddInvBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvSelBySite" Visible="False" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="transfer" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edRefNbr" TabIndex="100">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
            </px:PXSelector>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDropDown CommitChanges="True" ID="edTransferType" runat="server" AllowNull="False" DataField="TransferType" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" AutoRefresh="True"/>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" DataSourceID="ds" AutoRefresh="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edToSiteID" runat="server" DataField="ToSiteID" AutoRefresh="True" 
                DataSourceID="ds" />
            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXNumberEdit CommitChanges="True" ID="edControlQty" runat="server" DataField="ControlQty" />
            <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty" Enabled="False" />
            </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <script type="text/javascript">
        function UpdateItemSiteCell(n, c) {
            var activeRow = c.cell.row;
            var sCell = activeRow.getCell("Selected");
            var qCell = activeRow.getCell("QtySelected");
            var q = activeRow.getCell("QtyOnHand");
            if (sCell == c.cell) {
                if (sCell.getValue() == true)
                    qCell.setValue(q.getValue());
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
    <px:PXTab ID="tab" runat="server" Height="216px" Style="z-index: 100;" Width="100%" TabIndex="23">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Transaction Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        ActionsPosition="top" SkinID="DetailsInTab" StatusField="Availability" SyncPosition="true">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSINTran_binLotSerial" CommandSourceID="ds" DependOnGrid="grid" />
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
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true">
                                        <GridProperties>
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode">
                                        <Parameters>
                                            <px:PXControlParam ControlID="form" Name="INTran.docType" PropertyName="NewDataKey[&quot;DocType&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="true" NullText="<SPLIT>">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true" NullText="<SPLIT>">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edToLocationID" runat="server" DataField="ToLocationID" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.toSiteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
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
                                    <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" NullText="<SPLIT>" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="INTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                                            <px:PXSyncGridParam ControlID="grid" Name="SyncGrid" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXNumberEdit ID="edReceiptedQty" runat="server" DataField="ReceiptedQty" />
                                    <px:PXNumberEdit ID="edINTransitQty" runat="server" DataField="INTransitQty" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Availability" />
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" NullText="<SPLIT>" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" NullText="<SPLIT>" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ToLocationID" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" AutoCallBack="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ReceiptedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="INTransitQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LotSerialNbr" NullText="<SPLIT>" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ExpireDate" />
                                    <px:PXGridColumn DataField="ReasonCode" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="LineNbr"/>

                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXFormView ID="form2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="CurrentDocument"
                        CaptionVisible="False">
                        <ContentStyle BorderWidth="0px">
                        </ContentStyle>
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                            <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
                            <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                            <px:PXSelector ID="edReceiptNbr" runat="server" DataField="POReceiptNbr" Enabled="False" AllowEdit="True" />
                        </Template>
                        <AutoSize Enabled="True" />
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <px:PXSmartPanel ID="PanelLS" runat="server" Style="z-index: 108; left: 252px; position: absolute; top: 531px; height: 500px;"
        Width="764px" Caption="Allocations" DesignView="Hidden" CaptionVisible="True" Key="lsselect" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="optform">
        <px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LSINTran_lotseropts" DataSourceID="ds"
            SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px" CommandName="LSINTran_generateLotSerial" CommandSourceID="ds">
                </px:PXButton>
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
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="LocationID" />
                        <px:PXGridColumn DataField="LotSerialNbr" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="INTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr" AutoRefresh="true">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="INTranSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXDateTimeEdit ID="edExpireDate2" runat="server" DataField="ExpireDate" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK"/>
        </px:PXPanel>
    </px:PXSmartPanel>
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
                <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" />
                <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClass" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True" ID="edSubItem" runat="server" DataField="SubItem" AutoRefresh="true" />
                <px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" />
                <px:PXCheckBox CommitChanges="True" ID="chkOnlyAvailable" runat="server" Checked="True" DataField="OnlyAvailable" />
            </Template>
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
                        <px:PXGridColumn DataField="LocationID" />
                        <px:PXGridColumn DataField="ItemClassID" />
                        <px:PXGridColumn DataField="ItemClassDescription" />
                        <px:PXGridColumn DataField="PriceClassID" />
                        <px:PXGridColumn DataField="PriceClassDescription" />
                        <px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                        <px:PXGridColumn DataField="Descr" />
                        <px:PXGridColumn DataField="BaseUnit" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyAvail" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyOnHand" TextAlign="Right" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" CommandName="AddInvSelBySite" CommandSourceID="ds" Text="Add" SyncVisible="false"/>
            <px:PXButton ID="PXButton4" runat="server" Text="Add & Close" DialogResult="OK" />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
