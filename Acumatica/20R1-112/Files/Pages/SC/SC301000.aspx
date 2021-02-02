<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="True" ValidateRequest="False"
    CodeFile="SC301000.aspx.cs" Inherits="Page_SC301000" Title="Subcontracts" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.CN.Subcontracts.SC.Graphs.SubcontractEntry"
        PrimaryView="Document" Width="100%">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" />
            <px:PXDSCallbackCommand Visible="false" Name="Hold" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="False" Name="NewVendor" />
            <px:PXDSCallbackCommand Visible="False" Name="EditVendor" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOOrder" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOOrderLine" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="CreatePOReceipt" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Name="AddInvBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvSelBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ViewDemand" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True"
                PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False"
                CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text=""
                Visible="False" />
            <px:PXDSCallbackCommand Name="CreatePrepayment" Visible="False" CommitChanges="True" PopupCommand="Cancel"
                PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ViewChangeOrder" Visible="False" DependOnGrid="ChangeOrdersGrid" />
            <px:PXDSCallbackCommand Name="ViewOrigChangeOrder" Visible="False" DependOnGrid="ChangeOrdersGrid" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$PurchaseOrder$Link"
                DependOnGrid="grid" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$Subcontract$Link"
                DependOnGrid="grid" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false"
                DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false"
                DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false"
                DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false"
                DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false"
                DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
    <px:PXSmartPanel ID="PanelAddPOLine" runat="server" Height="370px"
        Style="z-index: 108; position: absolute; left: 660px;top: 99px" Width="960px" Key="poLinesSelection"
        Caption="Add Purchase Order Line" CaptionVisible="True" LoadOnDemand="true" AutoRepaint="true"
        ShowAfterLoad="true">
        <px:PXFormView ID="frmPOFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
            DataMember="filter" Caption="PO Selection" CaptionVisible="false" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr"
                    AutoRefresh="True" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridOL" runat="server" Height="240px" Width="100%" DataSourceID="ds"
            Style="border-width: 1px 0px" AutoAdjustColumns="true">
            <Levels>
                <px:PXGridLevel DataMember="poLinesSelection">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" />
                        <px:PXGridColumn AllowNull="False" DataField="LineType" />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn DataField="OrderQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="LeftToReceiveQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="TranDesc" />
                        <px:PXGridColumn AllowNull="False" DataField="RcptQtyMin" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="RcptQtyMax" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="RcptQtyAction" Type="DropDownList"
                            Width="100px" />
                    </Columns>
                    <Mode AllowAddNew="false" AllowUpdate="false" AllowDelete="false" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelAddPO" runat="server" Height="415px"
        Style="z-index: 108; left: 486px; position: absolute; top: 99px"
        Width="960px" Caption="Add Purchase Order" CaptionVisible="True" LoadOnDemand="true" Key="openOrders"
        ShowAfterLoad="true" AutoCallBack-Enabled="True" AutoCallBack-Target="grdOpenOrders"
        AutoCallBack-Command="Refresh">
        <px:PXGrid ID="grdOpenOrders" runat="server" Height="340px" Width="100%" DataSourceID="ds"
            Style="border-width: 1px 0px; left: 0px; top: 2px;" AutoAdjustColumns="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="openOrders">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AutoCallBack="True" DataField="Selected"
                            TextAlign="Center" Type="CheckBox" Width="60px" />
                        <px:PXGridColumn DataField="OrderNbr" />
                        <px:PXGridColumn DataField="OrderDate" />
                        <px:PXGridColumn DataField="ExpirationDate" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" />
                        <px:PXGridColumn DataField="CuryID" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrderTotal"
                            TextAlign="Right" />
                        <px:PXGridColumn DataField="VendorRefNbr" />
                        <px:PXGridColumn DataField="TermsID" />
                        <px:PXGridColumn DataField="OrderDesc" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="LeftToReceiveQty"
                            TextAlign="Right" Width="100px" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryLeftToReceiveCost"
                            TextAlign="Right" Width="100px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelFixedDemand" runat="server" Height="415px"
        Style="z-index: 108; left: 486px; position: absolute; top: 99px" Width="960px" Caption="Demand"
        CaptionVisible="True" LoadOnDemand="true" Key="FixedDemand" ShowAfterLoad="true"
        AutoCallBack-Enabled="True" AutoCallBack-Target="gridFixedDemand" AutoCallBack-Command="Refresh">
        <px:PXGrid ID="gridFixedDemand" runat="server" Height="340px" Width="100%" DataSourceID="ds"
            Style="border-width: 1px 0px; left: 0px; top: 2px;" AutoAdjustColumns="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="FixedDemand">
                    <Columns>
                        <px:PXGridColumn DataField="OrderNbr" />
                        <px:PXGridColumn DataField="RequestDate" />
                        <px:PXGridColumn DataField="CustomerID" />
                        <px:PXGridColumn DataField="SiteID" />
                        <px:PXGridColumn AllowUpdate="False" DataField="UOM" DisplayFormat="&gt;aaaaaa"
                            Label="Orig. UOM" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="OrderQty"
                            Label="Orig. Quantity" TextAlign="Right" Width="100px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="POUOM" DisplayFormat="&gt;aaaaaa" Label="UOM" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="POUOMOrderQty"
                            Label="Quantity" TextAlign="Right" Width="100px" />
                        <px:PXGridColumn DataField="INItemPlan__Active" AllowNull="False"
                            TextAlign="Center" Type="CheckBox" />
                    </Columns>
                    <RowTemplate>
                        <px:PXSelector ID="PXSelector1" DataField="OrderNbr" runat="server" AllowEdit="True" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelReplenishment" runat="server" Height="415px"
        Style="z-index: 108; left: 486px; position: absolute; top: 99px" Width="960px" Caption="Demand"
        CaptionVisible="True" LoadOnDemand="True" Key="ReplenishmentLines" ShowAfterLoad="True"
        AutoCallBack-Target="gridReplenishmentLines" AutoCallBack-Command="Refresh">
        <px:PXGrid ID="gridReplenishmentLines" runat="server" Height="340px" Width="100%" DataSourceID="ds"
            Style="border-width: 1px 0px; left: 0px; top: 2px;" AutoAdjustColumns="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="ReplenishmentLines">
                    <Columns>
                        <px:PXGridColumn DataField="RefNbr" />
                        <px:PXGridColumn DataField="OrderDate" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Qty" TextAlign="Right"
                            Width="100px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton9" runat="server" DialogResult="Cancel" Text="Close"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document"
        Caption="Document Summary" NoteIndicator="True" FilesIndicator="True" LinkIndicator="true"
        ActivityIndicator="true" ActivityField="NoteActivity"
        emailinggraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edOrderNbr"
        NotifyIndicator="True">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="true">
                <GridProperties FastFilterFields="VendorRefNbr,VendorID,VendorID_Vendor_acctName">
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSelector>
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold">
                <AutoCallBack Command="Hold" Target="ds">
                </AutoCallBack>
            </px:PXCheckBox>
            <px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False" />
            <px:PXDropDown ID="edBehavior" runat="server" AllowNull="False" DataField="Behavior"/>
            <px:PXCheckBox ID="chkRequestApproval" runat="server" DataField="RequestApproval" />
            <px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edOrderDate" runat="server" DataField="OrderDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edExpectedDate" runat="server" DataField="ExpectedDate" />
            <px:PXDateTimeEdit ID="edExpirationDate" runat="server" DataField="ExpirationDate" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edOrderDesc" runat="server" DataField="OrderDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID"
                AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True"
                DataField="VendorLocationID" />
            <px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" />
            <px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" AllowEdit="True" CommitChanges="True" />
            <pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" DataSourceID="ds"
                RateTypeView="_POOrder_CurrencyInfo_" DataMember="_Currency_" />
            <px:PXTextEdit CommitChanges="True" ID="edVendorRefNbr" runat="server" DataField="VendorRefNbr" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot" />
            <px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal"
                Enabled="False" />
            <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryRetainageTotal" runat="server" DataField="CuryRetainageTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryOrderTotal" runat="server" DataField="CuryOrderTotal" Enabled="False" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryControlTotal" runat="server" DataField="CuryControlTotal" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
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
<px:PXTab ID="tab" runat="server" Height="504px" Style="z-index: 100;" Width="100%" DataSourceID="ds"
    DataMember="CurrentDocument">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
<Items>
<px:PXTabItem Text="Document Details">
    <Template>
        <px:PXGrid ID="grid" runat="server" DataSourceID="ds"
            Style="z-index: 100; left: 0px; top: 0px; height: 384px;" Width="100%" BorderWidth="0px"
            SkinID="Details" SyncPosition="True" Height="384px" TabIndex="2500">
            <Levels>
                <px:PXGridLevel DataMember="Transactions">
                    <Columns>
                        <px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAAAAAA"
                            RenderEditorText="True" AllowShowHide="Server" CommitChanges="True" />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA"
                            CommitChanges="True" AllowDragDrop="true"/>
                        <px:PXGridColumn DataField="ProjectID" Label="Project" CommitChanges="True" />
                        <px:PXGridColumn DataField="TaskID" DisplayFormat="&gt;AAAAAAAAAA" Label="Task"
                            CommitChanges="True" />
                        <px:PXGridColumn DataField="CostCodeID" CommitChanges="True"/>
                        <px:PXGridColumn DataField="TranDesc" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" CommitChanges="True"
                            AllowDragDrop="true"/>
                        <px:PXGridColumn AllowNull="False" DataField="OrderQty" TextAlign="Right"
                            CommitChanges="True" AllowDragDrop="true"/>
                        <px:PXGridColumn AllowNull="False" DataField="CuryUnitCost" TextAlign="Right"
                            CommitChanges="true" />
                        <px:PXGridColumn DataField="ManualPrice" TextAlign="Center" AllowNull="False" Type="CheckBox"
                            CommitChanges="True"/>
                        <px:PXGridColumn AllowNull="False" DataField="CuryLineAmt" TextAlign="Right"
                            CommitChanges="true" />
                        <px:PXGridColumn DataField="DiscPct" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryDiscCost" TextAlign="Right" />
                        <px:PXGridColumn DataField="ManualDisc" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="DiscountID" RenderEditorText="True" TextAlign="Left"
                            AllowShowHide="Server" Width="90px" CommitChanges="True" />
                        <px:PXGridColumn DataField="DiscountSequenceID" TextAlign="Left" />
                        <px:PXGridColumn DataField="DisplayReqPrepaidQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryReqPrepaidAmt" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="RetainagePct" TextAlign="Right"
                            CommitChanges="true" />
                        <px:PXGridColumn AllowNull="False" DataField="CuryRetainageAmt" TextAlign="Right"
                            CommitChanges="true" />
                        <px:PXGridColumn AllowNull="False" DataField="CuryExtCost" TextAlign="Right" />
                        <px:PXGridColumn DataField="AlternateID" />
                        <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" />
                        <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" />
                        <px:PXGridColumn DataField="ExpenseAcctID" DisplayFormat="&gt;######" CommitChanges="True" />
                        <px:PXGridColumn DataField="ExpenseAcctID_Account_description" />
                        <px:PXGridColumn DataField="ExpenseSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" />
                        <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Visible="False" />
                        <px:PXGridColumn DataField="RequestedDate" />
                        <px:PXGridColumn DataField="PromisedDate" />
                        <px:PXGridColumn DataField="ReceivedQty" />
                        <px:PXGridColumn DataField="CompletePOLine" Visible="False" />
                        <px:PXGridColumn AllowNull="False" DataField="Completed" TextAlign="Center" Type="CheckBox"
                            Width="60px" />
                        <px:PXGridColumn AllowNull="False" DataField="Cancelled" TextAlign="Center" Type="CheckBox"
                            Width="60px" />
                        <px:PXGridColumn DataField="Closed" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="BilledQty" />
                        <px:PXGridColumn DataField="CuryBilledAmt" />
                        <px:PXGridColumn DataField="UnbilledQty" />
                        <px:PXGridColumn DataField="CuryUnbilledAmt" />
                        <px:PXGridColumn AllowNull="False" DataField="BaseOrderQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                    </Columns>
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                        <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID"
                            AllowEdit="True" AutoRefresh="True">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="POLine.lineType"
                                    PropertyName="DataValues[&quot;LineType&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="POLine.inventoryID"
                                    PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID"
                            AutoRefresh="True" />
                        <px:PXDropDown CommitChanges="True" ID="edLineType" runat="server" AllowNull="False"
                            DataField="LineType" />
                        <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM"
                            AutoRefresh="True">
                            <Parameters>
                                <px:PXSyncGridParam ControlID="grid" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" />
                        <px:PXNumberEdit ID="edReceivedQty" runat="server" DataField="ReceivedQty" Enabled="False" />
                        <px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost"
                            CommitChanges="true" />
                        <px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice"
                            CommitChanges="True" />
                        <px:PXSelector ID="edDiscountCode" runat="server" DataField="DiscountID" CommitChanges="True"
                            AllowEdit="True" edit="1" />
                        <px:PXNumberEdit ID="edDiscPct" runat="server" DataField="DiscPct" />
                        <px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" />
                        <px:PXCheckBox ID="chkManualDisc" runat="server" DataField="ManualDisc" CommitChanges="True" />
                        <px:PXNumberEdit ID="edCuryLineAmt" runat="server" DataField="CuryLineAmt"
                            CommitChanges="true" />
                        <px:PXNumberEdit ID="edCuryExtCost" runat="server" DataField="CuryExtCost" />
                        <px:PXLayoutRule runat="server" ColumnSpan="2" />
                        <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />

                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                        <px:PXNumberEdit ID="edRcptQtyMin" runat="server" DataField="RcptQtyMin" />
                        <px:PXNumberEdit ID="edRcptQtyMax" runat="server" DataField="RcptQtyMax" />
                        <px:PXNumberEdit ID="edRcptQtyThreshold" runat="server" DataField="RcptQtyThreshold" />
                        <px:PXDropDown ID="edRcptQtyAction" runat="server" DataField="RcptQtyAction" />
                        <px:PXCheckBox ID="chkCompleted" runat="server" DataField="Completed" />
                        <px:PXCheckBox ID="chkCancelled" runat="server" DataField="Cancelled" />
                        <px:PXCheckBox ID="chkClosed" runat="server" DataField="Closed" />
                        <px:PXNumberEdit ID="edBilledQty" runat="server" DataField="BilledQty" />
                        <px:PXNumberEdit ID="edCuryBilledAmt" runat="server" DataField="CuryBilledAmt" />
                        <px:PXNumberEdit ID="edUnbilledQty" runat="server" DataField="UnbilledQty" />
                        <px:PXNumberEdit ID="edCuryUnbilledAmt" runat="server" DataField="CuryUnbilledAmt" />
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                        <px:PXSegmentMask CommitChanges="True" Height="19px" ID="edBranchID" runat="server"
                            DataField="BranchID" />
                        <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server"
                            DataField="ExpenseAcctID" AutoRefresh="True" />
                        <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID"
                            AutoRefresh="True">
                            <Parameters>
                                <px:PXSyncGridParam ControlID="grid" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXDateTimeEdit ID="edRequestedDate" runat="server" DataField="RequestedDate" />
                        <px:PXDateTimeEdit ID="edPromisedDate" runat="server" DataField="PromisedDate" />
                        <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID"
                            AutoRefresh="True"/>
                        <px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID" />
                        <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
                        <px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID"
                            AutoRefresh="True">
                            <Parameters>
                                <px:PXSyncGridParam ControlID="grid" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edCostCode" runat="server" DataField="CostCodeID" AutoRefresh="True"
                            AllowAddNew="true" />
                        <px:PXDropDown ID="edPOType" runat="server" DataField="POType" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
            <CallbackCommands PasteCommand="PasteLine">
                <Save PostData="Container" />
            </CallbackCommands>
            <Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" AllowDragRows="true"/>
            <ActionBar>
                <CustomItems>
                    <px:PXToolBarButton Text="Insert Row" SyncText="false" ImageSet="main" ImageKey="AddNew">
                        <AutoCallBack Target="grid" Command="AddNew" Argument="1"></AutoCallBack>
                        <ActionBar ToolBarVisible="External" MenuVisible="true" />
                    </px:PXToolBarButton>
                    <px:PXToolBarButton Text="Cut Row" SyncText="false" ImageSet="main" ImageKey="Copy">
                        <AutoCallBack Target="grid" Command="Copy"></AutoCallBack>
                        <ActionBar ToolBarVisible="External" MenuVisible="true" />
                    </px:PXToolBarButton>
                    <px:PXToolBarButton Text="Insert Cut Row" SyncText="false" ImageSet="main" ImageKey="Paste">
                        <AutoCallBack Target="grid" Command="Paste"></AutoCallBack>
                        <ActionBar ToolBarVisible="External" MenuVisible="true" />
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Tax Details">
    <Template>
        <px:PXGrid ID="gridTaxes" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%"
            ActionsPosition="Top" BorderWidth="0px" SkinID="Details">
            <AutoSize Enabled="True" MinHeight="150" />
            <ActionBar>
                <Actions>
                    <Search Enabled="False" />
                    <Save Enabled="False" />
                    <EditRecord Enabled="False" />
                </Actions>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="Taxes">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" />
                        <px:PXNumberEdit SuppressLabel="True" ID="edTaxRate" runat="server" DataField="TaxRate"
                            Enabled="False" />
                        <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxableAmt" runat="server"
                            DataField="CuryTaxableAmt" />
                        <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="TaxID" AllowUpdate="False" />
                        <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right"
                            Width="81px" />
                        <px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="CuryRetainedTaxableAmt" TextAlign="Right"
                            Width="81px" />
                        <px:PXGridColumn AllowNull="False" DataField="CuryRetainedTaxAmt" TextAlign="Right"
                            Width="81px" />
                        <px:PXGridColumn AllowNull="False" DataField="Tax__TaxType" Label="Tax Type"
                            RenderEditorText="True" />
                        <px:PXGridColumn AllowNull="False" DataField="Tax__PendingTax" Label="Pending VAT"
                            TextAlign="Center" Type="CheckBox" Width="60px" />
                        <px:PXGridColumn AllowNull="False" DataField="Tax__ReverseTax" Label="Reverse VAT"
                            TextAlign="Center" Type="CheckBox" Width="60px" />
                        <px:PXGridColumn AllowNull="False" DataField="Tax__ExemptTax" Label="Exempt From VAT"
                            TextAlign="Center" Type="CheckBox" Width="60px" />
                        <px:PXGridColumn AllowNull="False" DataField="Tax__StatisticalTax" Label="Statistical VAT"
                            TextAlign="Center" Type="CheckBox" Width="60px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Vendor Info">
    <Template>
        <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
        <px:PXLayoutRule runat="server" StartGroup="True" />
        <px:PXFormView ID="formVC" runat="server" Caption="Vendor Contact" DataMember="Remit_Contact" DataSourceID="ds"
            RenderStyle="Fieldset">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="PXCheckBox1" runat="server"
                    DataField="OverrideContact" />
                <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                <px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" />
                <px:PXTextEdit ID="edPhone1" runat="server" DataField="Phone1" />
                <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True"/>
            </Template>
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
        </px:PXFormView>
        <px:PXFormView ID="formVA" DataMember="Remit_Address" runat="server" Caption="Vendor Address" DataSourceID="ds"
            SyncPosition="True" RenderStyle="Fieldset">
            <Template>
                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                <px:PXLayoutRule runat="server" Merge="True" />
                <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOverrideAddress" runat="server"
                    DataField="OverrideAddress" />
                <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False"/>
                <px:PXLayoutRule runat="server" />
                <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True"
                    DataSourceID="ds" CommitChanges="true" />
                <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" DataSourceID="ds">
                    <CallBackMode PostData="Container" />
                    <Parameters>
                        <px:PXControlParam ControlID="formVA" Name="PORemitAddress.countryID"
                            PropertyName="DataControls[&quot;edCountryID&quot;].Value" Type="String" />
                    </Parameters>
                </px:PXSelector>
                <px:PXMaskEdit ID="PXMaskEdit1" runat="server" DataField="PostalCode" CommitChanges="true" />
            </Template>
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
        </px:PXFormView>
        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
        <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Info" />
        <px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" />
        <px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" Text="ZONE1" />
        <px:PXSegmentMask CommitChanges="True" ID="edPayToVendorID" runat="server" DataField="PayToVendorID"
            AllowEdit="True" AutoRefresh="True" />
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Approval Details" BindingContext="form"
    VisibleExp="DataControls[&quot;chkRequestApproval&quot;].Value = 1">
    <Template>
        <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab"
            NoteIndicator="True" Style="left: 0px; top: 0px;">
            <AutoSize Enabled="True" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
            <Levels>
                <px:PXGridLevel DataMember="Approval">
                    <Columns>
                        <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                        <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                        <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                        <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                        <px:PXGridColumn DataField="ApproveDate" />
                        <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False"
                            RenderEditorText="True" />
                        <px:PXGridColumn DataField="WorkgroupID" />
                        <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false"
                            Width="160px"/>
                        <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                        <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                        <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false"
                            Width="100px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Discount Details">
    <Template>
        <px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details"
            BorderStyle="None">
            <Levels>
                <px:PXGridLevel DataMember="DiscountDetails">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXCheckBox ID="chkSkipDiscount" runat="server" DataField="SkipDiscount" />
                        <px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID" 
                                       AllowEdit="True" edit="1" />
                        <px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
                        <px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
                        <px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID"
                            AllowEdit="True" AutoRefresh="True" edit="1" />
                        <px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
                        <px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
                        <px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" />
                        <px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" />
                        <px:PXTextEdit ID="edExtDiscCode" runat="server" DataField="ExtDiscCode"/>
                        <px:PXTextEdit ID="edDescription" runat="server" DataField="Description"/>
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="SkipDiscount" Type="CheckBox" TextAlign="Center" />
                        <px:PXGridColumn DataField="DiscountID" CommitChanges="True" />
                        <px:PXGridColumn DataField="DiscountSequenceID" CommitChanges="True" />
                        <px:PXGridColumn DataField="Type" RenderEditorText="True" />
                        <px:PXGridColumn DataField="IsManual" Type="CheckBox" TextAlign="Center" />
                        <px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" />
                        <px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryRetainedDiscountAmt" TextAlign="Right" />
                        <px:PXGridColumn DataField="DiscountPct" TextAlign="Right" />
                        <px:PXGridColumn DataField="ExtDiscCode"/>
                        <px:PXGridColumn DataField="Description"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="SC History" RepaintOnDemand="False">
    <Template>
        <px:PXGrid ID="formAPDocs" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" BorderStyle="None" StatusField="StatusText" AdjustPageSize="Auto">
            <Levels>
                <px:PXGridLevel DataMember="APDocs">
                    <RowTemplate>
                        <px:PXSelector SuppressLabel="True" Size="s" ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" AllowEdit="True" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="DocType" />
                        <px:PXGridColumn DataField="RefNbr" />
                        <px:PXGridColumn DataField="DocDate" />
                        <px:PXGridColumn DataField="Status" />
                        <px:PXGridColumn DataField="TotalQty" />
                        <px:PXGridColumn DataField="TotalAmt" />
                        <px:PXGridColumn DataField="TotalPPVAmt" />
                        <px:PXGridColumn DataField="CuryID" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Prepayments" LoadOnDemand="true">
    <Template>
        <px:PXGrid ID="PrepaymentGrid" runat="server" Height="350px" Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds"
                   SkinID="DetailsInTab" AllowFilter="true" StatusField="StatusText">
            <Levels>
                <px:PXGridLevel DataMember="PrepaymentDocuments">
                    <RowTemplate>
                        <px:PXSelector ID="edPrepaymentRefNbr" runat="server" DataField="APRefNbr" AllowEdit="True" />
                        <px:PXSelector ID="edPaymentRefNbr" runat="server" DataField="PayRefNbr" AllowEdit="True" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="APDocType" />
                        <px:PXGridColumn DataField="APRefNbr" />
                        <px:PXGridColumn DataField="APRegister__DocDate" />
                        <px:PXGridColumn DataField="CuryAppliedAmt" />
                        <px:PXGridColumn DataField="APRegister__CuryDocBal" />
                        <px:PXGridColumn DataField="APRegister__Status" />
                        <px:PXGridColumn DataField="APRegister__CuryID" />
                        <px:PXGridColumn DataField="PayRefNbr" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Change Orders" LoadOnDemand="true">
    <Template>
        <px:PXGrid ID="ChangeOrdersGrid" runat="server" Height="350px" Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds"
            SkinID="Inquire" AllowFilter="true">
            <Levels>
                <px:PXGridLevel DataMember="ChangeOrders">
                    <Columns>
                        <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewChangeOrder"/>
                        <px:PXGridColumn DataField="PMChangeOrder__ClassID"/>
                        <px:PXGridColumn DataField="PMChangeOrder__ProjectNbr"/>
                        <px:PXGridColumn DataField="PMChangeOrder__Status"/>
                        <px:PXGridColumn DataField="PMChangeOrder__Description"/>
                        <px:PXGridColumn DataField="PMChangeOrder__Date"/>
                        <px:PXGridColumn DataField="PMChangeOrder__CompletionDate"/>
                        <px:PXGridColumn DataField="PMChangeOrder__DelayDays"/>
                        <px:PXGridColumn DataField="PMChangeOrder__ReverseStatus"/>
                        <px:PXGridColumn DataField="PMChangeOrder__OrigRefNbr" LinkCommand="ViewOrigChangeOrder"/>
                        <px:PXGridColumn DataField="PMChangeOrder__ExtRefNbr"/>
                        <px:PXGridColumn DataField="ProjectID"/>
                        <px:PXGridColumn DataField="TaskID"/>
                        <px:PXGridColumn DataField="InventoryID"/>
                        <px:PXGridColumn DataField="CostCodeID"/>
                        <px:PXGridColumn DataField="Description"/>
                        <px:PXGridColumn DataField="Qty" TextAlign="Right"/>
                        <px:PXGridColumn DataField="UOM"/>
                        <px:PXGridColumn DataField="UnitCost" TextAlign="Right"/>
                        <px:PXGridColumn DataField="Amount" TextAlign="Right"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True"/>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/>
        </px:PXGrid>
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Other Information">
    <Template>
        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
        <px:PXSegmentMask CommitChanges="True" ID="PXSegmentMask2" runat="server" DataField="BranchID" />
        <px:PXSelector ID="edRQReqNbr" runat="server" AllowEdit="True" DataField="RQReqNbr" Enabled="False" />
        <px:PXSelector ID="edOwnerWorkgroupID" runat="server" DataField="OwnerWorkgroupID" />
        <px:PXLayoutRule runat="server" Merge="True" />
        <px:PXCheckBox ID="chkDontPrint" runat="server" Checked="True" DataField="DontPrint" Size="SM" />
        <px:PXCheckBox ID="chkPrinted" runat="server" DataField="Printed" Enabled="False" Size="SM" />
        <px:PXLayoutRule runat="server" />
        <px:PXLayoutRule runat="server" Merge="True" />
        <px:PXCheckBox ID="chkDontEmail" runat="server" Checked="True" DataField="DontEmail" Size="SM" />
        <px:PXCheckBox ID="chkEmailed" runat="server" DataField="Emailed" Enabled="False" Size="SM" />
        <px:PXLayoutRule runat="server" />
        <px:PXCheckBox ID="chkOrderBasedAPBill" runat="server" DataField="OrderBasedAPBill" />
        <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
        <px:PXCheckBox ID="chkRetainageApply" runat="server" DataField="RetainageApply" CommitChanges="true" />
        <px:PXNumberEdit ID="edDefRetainagePct" runat="server" DataField="DefRetainagePct" />
        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
        <px:PXNumberEdit ID="edUnbilledOrderQty" runat="server" DataField="UnbilledOrderQty" />
        <px:PXNumberEdit ID="edCuryUnbilledOrderTotal" runat="server" DataField="CuryUnbilledOrderTotal" />
        <px:PXNumberEdit ID="edCuryPrepaidTotal" runat="server" DataField="CuryPrepaidTotal" Enabled="false" />
        <px:PXNumberEdit ID="edCuryUnprepaidTotal" runat="server" DataField="CuryUnprepaidTotal" Enabled="false" />
    </Template>
</px:PXTabItem>
<px:PXTabItem Text="Attributes">
    <Template>
        <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%"
                   Height="200px" MatrixMode="True">
            <Levels>
                <px:PXGridLevel DataMember="Answers">
                    <Columns>
                        <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False"
                                         TextField="AttributeID_description" />
                        <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="Value" AllowShowHide="False" AllowSort="False" />
                    </Columns>
                    <Layout FormViewHeight="" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="200" />
            <ActionBar>
                <Actions>
                    <Search Enabled="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
        </px:PXGrid>
    </Template>
</px:PXTabItem>
    <px:PXTabItem Text="Compliance">
        <Template>
            <px:PXGrid runat="server" SyncPosition="True" KeepPosition="True" Height="300px" SkinID="DetailsInTab" Width="100%" ID="grdComplianceDocuments" AutoGenerateColumns="Append" DataSourceID="ds" AllowPaging="True" PageSize="12">
                <AutoSize Enabled="True" MinHeight="150" />
                <Mode InitNewRow="True" />
                <Levels>
                    <px:PXGridLevel DataMember="ComplianceDocuments">
                        <RowTemplate>
                            <px:PXSelector runat="server" DataField="DocumentTypeValue" AutoRefresh="True" ID="edDocumentTypeValue" />
                            <px:PXSelector runat="server" DataField="BillID" FilterByAllFields="True" AutoRefresh="True" ID="edBillID" />
                            <px:PXSelector runat="server" DataField="InvoiceID" FilterByAllFields="True" AutoRefresh="True" ID="edInvoiceID" />
                            <px:PXSelector runat="server" DataField="ApCheckID" FilterByAllFields="True" AutoRefresh="True" ID="edApCheckID" />
                            <px:PXSelector runat="server" DataField="ArPaymentID" FilterByAllFields="True" AutoRefresh="True" ID="edArPaymentID" />
                            <px:PXSelector runat="server" DataField="ProjectTransactionID" FilterByAllFields="True" AutoRefresh="True" ID="edProjectTransactionID" />
                            <px:PXSelector runat="server" DataField="PurchaseOrder" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edPurchaseOrder" />
                            <px:PXSelector runat="server" DataField="PurchaseOrderLineItem" AutoRefresh="True" ID="edPurchaseOrderLineItem" />
                            <px:PXSelector runat="server" DataField="Subcontract" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edSubcontract" />
                            <px:PXSelector runat="server" DataField="SubcontractLineItem" AutoRefresh="True" ID="edSubcontractLineItem" />
                            <px:PXSelector runat="server" DataField="ChangeOrderNumber" AutoRefresh="True" ID="edChangeOrderNumber" />
                            <px:PXSelector runat="server" DataField="DocumentType" AutoRefresh="True" ID="edDocumentType" />
                            <px:PXSelector runat="server" DataField="Status" AutoRefresh="True" ID="edStatus" />
                            <px:PXSelector runat="server" DataField="VendorID" AutoRefresh="True" ID="edVendorID" />
                            <px:PXSelector runat="server" DataField="AccountID" AutoRefresh="True" ID="edAccountID" />
                            <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                            <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                            <px:PXSelector runat="server" DataField="ProjectID" FilterByAllFields="True" AutoRefresh="True" ID="edComplianceProjectID" />
                            <px:PXSegmentMask runat="server" DataField="CostCodeID" AutoRefresh="True" ID="edCostCode2" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn TextAlign="Left" DataField="ExpirationDate" CommitChanges="True" />
                            <px:PXGridColumn DataField="DocumentType" CommitChanges="True" />
                            <px:PXGridColumn TextAlign="Left" DataField="CreationDate" />
                            <px:PXGridColumn DataField="Status" CommitChanges="True" />
                            <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="Required" />
                            <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="Received" />
                            <px:PXGridColumn TextAlign="Left" DataField="ReceivedDate" />
                            <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="IsProcessed" />
                            <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="IsVoided" />
                            <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="IsCreatedAutomatically" />
                            <px:PXGridColumn TextAlign="Left" DataField="SentDate" />
                            <px:PXGridColumn DataField="ProjectID" CommitChanges="True" LinkCommand="ComplianceDocuments_Project_ViewDetails" />
                            <px:PXGridColumn TextAlign="Left" DataField="CostTaskID" CommitChanges="True" LinkCommand="ComplianceDocuments_Task_ViewDetails" />
                            <px:PXGridColumn TextAlign="Left" DataField="RevenueTaskID" CommitChanges="True" LinkCommand="ComplianceDocuments_Task_ViewDetails" />
                            <px:PXGridColumn TextAlign="Left" DataField="CostCodeID" CommitChanges="True" LinkCommand="ComplianceDocuments_CostCode_ViewDetails" />
                            <px:PXGridColumn DataField="VendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                            <px:PXGridColumn TextAlign="Left" DataField="VendorName" />
                            <px:PXGridColumn DisplayMode="Text" DataField="PurchaseOrder" CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
                            <px:PXGridColumn TextAlign="Left" DataField="PurchaseOrderLineItem" CommitChanges="True" />
                            <px:PXGridColumn DisplayMode="Text" DataField="Subcontract" CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                            <px:PXGridColumn TextAlign="Left" DataField="SubcontractLineItem" />
                            <px:PXGridColumn DisplayMode="Text" DataField="ChangeOrderNumber" CommitChanges="True" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" />
                            <px:PXGridColumn TextAlign="Left" DataField="AccountID" CommitChanges="True" />
                            <px:PXGridColumn DisplayMode="Text" TextAlign="Left" DataField="ApCheckID" CommitChanges="True" LinkCommand="ComplianceDocument$ApCheckID$Link" />
                            <px:PXGridColumn TextAlign="Left" DataField="CheckNumber" />
                            <px:PXGridColumn DisplayMode="Text" TextAlign="Left" DataField="ArPaymentID" CommitChanges="True" LinkCommand="ComplianceDocument$ArPaymentID$Link" />
                            <px:PXGridColumn TextAlign="Right" DataField="BillAmount" />
                            <px:PXGridColumn DisplayMode="Text" DataField="BillID" CommitChanges="True" LinkCommand="ComplianceDocument$BillID$Link" />
                            <px:PXGridColumn TextAlign="Left" DataField="CertificateNumber" />
                            <px:PXGridColumn DataField="CreatedByID" />
                            <px:PXGridColumn DataField="CustomerID" CommitChanges="True" LinkCommand="ComplianceDocuments_Customer_ViewDetails" />
                            <px:PXGridColumn TextAlign="Left" DataField="CustomerName" />
                            <px:PXGridColumn TextAlign="Left" DataField="DateIssued" />
                            <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                            <px:PXGridColumn TextAlign="Left" DataField="EffectiveDate" />
                            <px:PXGridColumn TextAlign="Left" DataField="InsuranceCompany" />
                            <px:PXGridColumn TextAlign="Right" DataField="InvoiceAmount" />
                            <px:PXGridColumn DisplayMode="Text" DataField="InvoiceID" CommitChanges="True" LinkCommand="ComplianceDocument$InvoiceID$Link" />
                            <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="IsExpired" />
                            <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="IsRequiredJointCheck" />
                            <px:PXGridColumn TextAlign="Right" DataField="JointAmount" />
                            <px:PXGridColumn TextAlign="Left" DataField="JointRelease" />
                            <px:PXGridColumn TextAlign="Center" DataField="JointReleaseReceived" />
                            <px:PXGridColumn TextAlign="Left" DataField="JointVendorInternalId" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                            <px:PXGridColumn TextAlign="Left" DataField="JointVendorExternalName" />
                            <px:PXGridColumn DataField="LastModifiedByID" />
                            <px:PXGridColumn TextAlign="Right" DataField="LienWaiverAmount" />
                            <px:PXGridColumn TextAlign="Right" DataField="Limit" />
                            <px:PXGridColumn TextAlign="Left" DataField="MethodSent" />
                            <px:PXGridColumn TextAlign="Left" DataField="PaymentDate" />
                            <px:PXGridColumn DataField="ArPaymentMethodID" />
                            <px:PXGridColumn DataField="ApPaymentMethodID" />
                            <px:PXGridColumn TextAlign="Left" DataField="Policy" />
                            <px:PXGridColumn DisplayMode="Text" TextAlign="Left" DataField="ProjectTransactionID" CommitChanges="True" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" />
                            <px:PXGridColumn TextAlign="Left" DataField="ReceiptDate" />
                            <px:PXGridColumn TextAlign="Left" DataField="ReceiveDate" />
                            <px:PXGridColumn TextAlign="Left" DataField="ReceivedBy" />
                            <px:PXGridColumn DataField="SecondaryVendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                            <px:PXGridColumn TextAlign="Left" DataField="SecondaryVendorName" />
                            <px:PXGridColumn TextAlign="Left" DataField="SponsorOrganization" />
                            <px:PXGridColumn TextAlign="Left" DataField="ThroughDate" />
                            <px:PXGridColumn TextAlign="Left" DataField="SourceType" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
            </px:PXGrid>
        </Template>
    </px:PXTabItem>
</Items>
<AutoSize Container="Window" Enabled="True" MinHeight="180" />
</px:PXTab>
<px:PXSmartPanel ID="PanelAddSiteStatus" runat="server" Key="sitestatus" LoadOnDemand="true" Width="1100px"
    Height="500px" Caption="Inventory Lookup" CaptionVisible="true" AutoCallBack-Command='Refresh'
    AutoCallBack-Enabled="True" AutoCallBack-Target="formSitesStatus" DesignView="Hidden">
    <px:PXFormView ID="formSitesStatus" runat="server" CaptionVisible="False" DataMember="sitestatusfilter"
        DataSourceID="ds" Width="100%" SkinID="Transparent">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXTextEdit CommitChanges="True" ID="edInventory" runat="server" DataField="Inventory" />
            <px:PXTextEdit CommitChanges="True" ID="edBarCode" runat="server" DataField="BarCode" />
            <px:PXCheckBox CommitChanges="True" ID="chkOnlyAvailable" runat="server" Checked="True"
                DataField="OnlyAvailable" AutoCallBack="true"  />
            <px:PXLayoutRule  runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="PXSegmentMask3" runat="server" DataField="SiteID" />
            <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClass" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubItem" runat="server" DataField="SubItem"
                AutoRefresh="true" />
        </Template>
    </px:PXFormView>
    <px:PXGrid ID="gripSiteStatus" runat="server" DataSourceID="ds" Style="border-width: 1px 0px; top: 0px; left: 0px;"
        AutoAdjustColumns="true" Width="100%" SkinID="Details" AdjustPageSize="Auto" Height="135px" AllowSearch="True"
        BatchUpdate="true" FastFilterID="edInventory" FastFilterFields="InventoryCD,Descr,AlternateID">
        <CallbackCommands>
            <Refresh CommitChanges="true"></Refresh>
        </CallbackCommands>
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
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox"
                        Width="80px" AutoCallBack="true"
                                     AllowCheckAll="true" />
                    <px:PXGridColumn AllowNull="False" DataField="QtySelected" TextAlign="Right" />
                    <px:PXGridColumn DataField="SiteID" />
                    <px:PXGridColumn DataField="ItemClassID" />
                    <px:PXGridColumn DataField="ItemClassDescription" />
                    <px:PXGridColumn DataField="PriceClassID" />
                    <px:PXGridColumn DataField="PriceClassDescription" />
                    <px:PXGridColumn DataField="PreferredVendorID" />
                    <px:PXGridColumn DataField="PreferredVendorDescription" />
                    <px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                    <px:PXGridColumn DataField="Descr" />
                    <px:PXGridColumn DataField="PurchaseUnit" DisplayFormat="&gt;aaaaaa" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyAvailExt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyOnHandExt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOOrdersExt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyPOReceiptsExt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="AlternateID" />
                    <px:PXGridColumn AllowNull="False" DataField="AlternateType" />
                    <px:PXGridColumn AllowNull="False" DataField="AlternateDescr" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="true" />
    </px:PXGrid>
    <px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButton6" runat="server" CommandName="AddInvSelBySite" CommandSourceID="ds" Text="Add"
            SyncVisible="false"/>
        <px:PXButton ID="PXButton7" runat="server" Text="Add & Close" DialogResult="OK"/>
        <px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" />
    </px:PXPanel>
</px:PXSmartPanel>
<%-- Recalculate Prices and Discounts --%>
<px:PXSmartPanel ID="PanelRecalcDiscounts" runat="server" Caption="Recalculate Prices" CaptionVisible="true"
    LoadOnDemand="true" Key="recalcdiscountsfilter" AutoCallBack-Enabled="true"
    AutoCallBack-Target="formRecalcDiscounts" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
    CallBackMode-PostData="Page">
    <div style="padding: 5px">
        <px:PXFormView ID="formRecalcDiscounts" runat="server" DataSourceID="ds" CaptionVisible="False"
            DataMember="recalcdiscountsfilter">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S"
                    ControlSize="SM" />
                <px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" CommitChanges ="true" />
                <px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server"
                    DataField="RecalcUnitPrices" />
                <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server"
                    DataField="OverrideManualPrices" />
                <px:PXCheckBox CommitChanges="True" ID="chkRecalcDiscounts" runat="server"
                    DataField="RecalcDiscounts" />
                <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDiscounts" runat="server"
                    DataField="OverrideManualDiscounts" />
            </Template>
        </px:PXFormView>
    </div>
    <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButton10" runat="server" DialogResult="OK" Text="OK" CommandName="RecalcOk"
            CommandSourceID="ds" />
    </px:PXPanel>
</px:PXSmartPanel>
</asp:Content>
