<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO301000.aspx.cs"
    Inherits="Page_SO301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOOrderEntry" 
			PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Visible="False" Name="LSSOLine_generateLotSerial" CommitChanges="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSSOLine_binLotSerial" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="false" Name="SOLineSplit$RefNoteID$Link" DependOnGrid="grid2" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="POSupplyOK" Visible="False" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="Hold" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CreditHold" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Flow" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Bill" Visible="True" CommitChanges="true" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Action" Visible="True" CommitChanges="true" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Inquiry" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Report" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvoice" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvoiceOK" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CheckCopyParams" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
            <px:PXDSCallbackCommand Name="InventorySummary" Visible="false" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="false" Name="CalculateFreight" />
            <px:PXDSCallbackCommand Visible="false" Name="RecalculatePackages" />
            <px:PXDSCallbackCommand Visible="false" Name="ShopRates" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="RefreshRates" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="CreatePayment" CommitChanges="true" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Visible="false" Name="CreatePrepayment" CommitChanges="true" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewPayment" DependOnGrid="detgrid" />
            <px:PXDSCallbackCommand Visible="false" Name="AuthorizeCCPayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="VoidCCPayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="CaptureCCPayment" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ValidateCCPayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="CreditCCPayment" />
            <px:PXDSCallbackCommand Visible="false" Name="RecalcExternalTax" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvSelBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CreateCCPaymentMethodHF" PopupCommand="SyncCCPaymentMethods" PopupCommandTarget="ds" DependOnGrid="grdPMInstanceDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="SyncCCPaymentMethods" CommitChanges="true" Visible="False" />
            <px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
            <px:PXDSCallbackCommand Name="QuickProcessOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
            <px:PXDSCallbackCommand Name="QuickProcess" ShowProcessInfo="true" />
            <px:PXDSCallbackCommand Name="OpenAppointmentBoard" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="CreateServiceOrder" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewServiceOrder" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" Name="recalculateDiscountsFromImport" />
			<px:PXDSCallbackCommand Name="ShowMatrixPanel" CommitChanges="true" Visible="False" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
    <%-- Add Invoice Details --%>
    <px:PXSmartPanel ID="PanelAddInvoice" runat="server" Width="873px" Key="invoicesplits" Caption="Add Invoice Details" CaptionVisible="True"
        LoadOnDemand="True" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AutoCallBack-Command="Refresh" AutoCallBack-Target="form4" Height="400px">
        <px:PXFormView ID="form4" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="addinvoicefilter"
            CaptionVisible="False">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="S" StartColumn="True" />
                <px:PXDropDown ID="edDocType" runat="server" AllowNull="False" DataField="DocType" CommitChanges="true">
                </px:PXDropDown>
                <px:PXSelector ID="edRefNbr4" runat="server" AutoRefresh="True" DataField="RefNbr" DataSourceID="ds" CommitChanges="true">
                    <GridProperties>
                        <Layout ColumnsMenu="False" />
                    </GridProperties>
                </px:PXSelector>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="S" StartColumn="True" />
                 <px:PXCheckBox ID="chkExpand" runat="server" DataField="Expand" CommitChanges="true" />
            </Template>
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
        </px:PXFormView>
        <px:PXGrid ID="grid4" runat="server" Width="100%" DataSourceID="ds" BatchUpdate="True" Style="height: 250px;"
            AutoAdjustColumns="True" SkinID="Inquire" FilesIndicator="false" NoteIndicator="false">
            <Parameters>
                <px:PXControlParam ControlID="form4" Name="AddInvoiceFilter.refNbr" PropertyName="DataControls[&quot;edRefNbr4&quot;].Value"
                    Type="String" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="invoicesplits">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="SiteID" />
                        <px:PXGridColumn DataField="LocationID" AllowShowHide="Server" />
                        <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="Qty" DataType="Decimal" TextAlign="Right" DefValueText="0.0" />
                        <px:PXGridColumn DataField="TranDesc" />
                    </Columns>
                    <Layout ColumnsMenu="True" FormViewHeight="" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" />
            <Mode AllowAddNew="False" AllowDelete="False" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" CommandName="AddInvoiceOK" CommandSourceID="ds" Text="Add" SyncVisible="false" />
            <px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="Add &amp; Close" />
            <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Order Summary"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects"
        ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edOrderType" NotifyIndicator="True"
        TabIndex="14900">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edOrderType" aurelia="true" runat="server" DataField="OrderType" AutoRefresh="True" DataSourceID="ds">
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                </GridProperties>
                <AutoCallBack Command="Cancel" Target="ds" />
            </px:PXSelector>
            <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="True" DataSourceID="ds">
                <GridProperties FastFilterFields="CustomerOrderNbr,CustomerID,CustomerID_Customer_acctName">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
                <AutoCallBack Command="Cancel" Target="ds" />
            </px:PXSelector>
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold">
                <AutoCallBack Command="Hold" Target="ds">
                    <Behavior CommitChanges="True" />
                </AutoCallBack>
            </px:PXCheckBox>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXCheckBox ID="chkDontApprove" runat="server" DataField="DontApprove" Enabled="False" />
            <px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" CommitChanges="True" Enabled="False" />
            <px:PXDateTimeEdit ID="edOrderDate" runat="server" DataField="OrderDate">
                <AutoCallBack Command="Save" Target="form" />
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit CommitChanges="True" ID="edRequestDate" runat="server" DataField="RequestDate" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXTextEdit Size="s" ID="edCustomerOrderNbr" runat="server" DataField="CustomerOrderNbr" CommitChanges="True" />
            <px:PXLayoutRule runat="server" />
            <px:PXTextEdit ID="edCustomerRefNbr" runat="server" DataField="CustomerRefNbr" MaxLength="40" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowAddNew="True"
                AllowEdit="True" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" AutoRefresh="True"
                DataField="CustomerLocationID" DataSourceID="ds" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_SOOrder_CurrencyInfo_"
                DataMember="_Currency_"></pxa:PXCurrencyRate>
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edDestinationSiteID" runat="server" DataField="DestinationSiteID" AutoRefresh="True" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True"
                DataSourceID="ds" AllowAddNew="True" AllowEdit="True"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edOrderDesc" runat="server" DataField="OrderDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" StartGroup="True" />
            <px:PXPanel ID="XX" runat="server" RenderStyle="Simple">
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" Enabled="False" />
                <px:PXNumberEdit ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot" CommitChanges="true" />
                <px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
                <px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
                <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
                <px:PXNumberEdit ID="edCuryOrderTotal" runat="server" DataField="CuryOrderTotal" Enabled="False" />
                <px:PXNumberEdit ID="edCuryControlTotal" runat="server" CommitChanges="True" DataField="CuryControlTotal" />
            </px:PXPanel>
            
            <px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRUTROT" AlignLeft="true" />
            <px:PXCheckBox runat="server" DataField="IsFSIntegrated"     SuppressLabel="True" ID="chkIsFSIntegrated" Enabled="False" Visible="False" />
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
    <px:PXTab ID="tab" runat="server" Height="540px" Style="z-index: 100;" Width="100%">
        <Items>
            <px:PXTabItem Text="Document Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%"
                        TabIndex="100" SkinID="DetailsInTab" StatusField="Availability" SyncPosition="True" Height="473px">
                        <Levels>
                            <px:PXGridLevel DataMember="Transactions">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
									<px:PXSelector runat="server" ID="edAppointmentID" DataField="AppointmentID" AllowEdit="True" />
									<px:PXSelector runat="server" ID="edSOID" DataField="SOID" AllowEdit="True" />
									<px:PXCheckBox runat="server" ID="edSDSelected" DataField="SDSelected" Text="SDSelected" />
									<px:PXDropDown runat="server" ID="edEquipmentAction" DataField="EquipmentAction" CommitChanges="True" />
									<px:PXTextEdit runat="server" ID="edSMComment" DataField="Comment" />
									<px:PXSelector runat="server" ID="edSMEquipmentID" DataField="SMEquipmentID" CommitChanges="True" AutoRefresh="True" />
									<px:PXSelector runat="server" ID="edNewTargetEquipmentLineNbr" DataField="NewTargetEquipmentLineNbr" CommitChanges="True" AutoRefresh="True" />
									<px:PXSelector runat="server" ID="edSMComponentID" DataField="ComponentID" CommitChanges="True" AutoRefresh="True" />
									<px:PXSelector runat="server" ID="edEquipmentLineRef" DataField="EquipmentLineRef" CommitChanges="True" AutoRefresh="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXCheckBox ID="chkIsFree" runat="server" DataField="IsFree" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOLine.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" />
                                    <px:PXNumberEdit CommitChanges="True" ID="edOrderQty" runat="server" DataField="OrderQty" />
                                    <px:PXNumberEdit ID="edShippedQty" runat="server" DataField="ShippedQty" Enabled="False" />
                                    <px:PXNumberEdit ID="edOpenQty" runat="server" DataField="OpenQty" Enabled="False" />
                                    <px:PXNumberEdit ID="edUnitPrice" runat="server" DataField="CuryUnitPrice"  CommitChanges="True"/>
                                    <px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
                                    <px:PXSelector ID="edManualDiscountID" runat="server" DataField="DiscountID" AllowEdit="True" edit="1" />
                                    <px:PXNumberEdit ID="edDiscPct" runat="server" DataField="DiscPct" />
                                    <px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" />
                                    <px:PXCheckBox ID="chkManualDisc" runat="server" DataField="ManualDisc" CommitChanges="True" />
                                    <px:PXLayoutRule runat="server" ColumnSpan="3" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                                    <px:PXCheckBox ID="chkCompleted" runat="server" DataField="Completed" />
                                    <px:PXNumberEdit ID="edCuryExtPrice" runat="server" DataField="CuryExtPrice" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edCuryLineAmt" runat="server" DataField="CuryLineAmt" />
                                    <px:PXNumberEdit ID="edCuryUnbilledAmt" runat="server" DataField="CuryUnbilledAmt" Enabled="False" />
                                    <px:PXDateTimeEdit ID="edRequestDate1" runat="server" DataField="RequestDate" />
                                    <px:PXDateTimeEdit ID="edShipDate" runat="server" DataField="ShipDate" />
                                    <px:PXDropDown ID="edShipComplete" runat="server" AllowNull="False" DataField="ShipComplete" SelectedIndex="2" />
                                    <px:PXNumberEdit ID="edCompleteQtyMin" runat="server" DataField="CompleteQtyMin" />
                                    <px:PXNumberEdit ID="edCompleteQtyMax" runat="server" DataField="CompleteQtyMax" />
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                    <px:PXDateTimeEdit ID="edDRTermStartDate" runat="server" DataField="DRTermStartDate" CommitChanges="true" />
                                    <px:PXDateTimeEdit ID="edDRTermEndDate" runat="server" DataField="DRTermEndDate" CommitChanges="true" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
                                    <px:PXCheckBox CommitChanges="True" ID="chkPOCreate" runat="server" DataField="POCreate" />
                                    <px:PXDropDown ID="edPOSource" runat="server" DataField="POSource" />
                                    <px:PXSelector ID="edReasonCode" runat="server" DataField="ReasonCode" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="grid" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
                                    <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
									<px:PXDropDown ID="edAvalaraCustomerUsageTypeID1" runat="server" DataField="AvalaraCustomerUsageType" />
                                    <px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID" />
                                    <px:PXLayoutRule runat="server" Merge="True" />
                                    <px:PXSelector Size="xm" ID="edLotSerialNbr" runat="server" AllowNull="False" DataField="LotSerialNbr" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="SOLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOLine.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="SOLine.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXCheckBox CommitChanges="True" ID="chkCommissionable" runat="server" Checked="True" DataField="Commissionable" />
                                    <px:PXLayoutRule runat="server" />
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edCostCode" runat="server" DataField="CostCodeID" AutoRefresh="True" />
                                    <px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRRDeductibleTran" />
                                    <px:PXDropDown runat="server" DataField="RUTROTItemType" CommitChanges="True" ID="cmbRRItemType" />
                                    <px:PXSelector runat="server" DataField="RUTROTWorkTypeID" CommitChanges="True" ID="cmbRRWorkType" AutoRefresh="true" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Availability" />
                                    <px:PXGridColumn DataField="BranchID" RenderEditorText="True" CommitChanges="True" />
                                    <px:PXGridColumn DataField="OrderType" />
                                    <px:PXGridColumn DataField="OrderNbr" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LineType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="InvoiceNbr" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="Operation" AllowShowHide="Server" Label="Operation" RenderEditorText="True" CommitChanges="True" />
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" AllowDragDrop="true" />
									<px:PXGridColumn DataField="AppointmentDate" />
                                    <px:PXGridColumn DataField="EquipmentAction" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Comment" />
                                    <px:PXGridColumn DataField="SMEquipmentID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="NewTargetEquipmentLineNbr" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ComponentID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="EquipmentLineRef" CommitChanges="True" />
									<px:PXGridColumn DataField="CustomerLocationID" />
									<px:PXGridColumn DataField="AppointmentID" />
									<px:PXGridColumn DataField="SOID" />
									<px:PXGridColumn DataField="SDSelected" CommitChanges="True" Type="CheckBox" />
                                    <px:PXGridColumn DataField="SubItemID" NullText="<SPLIT>" CommitChanges="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="AutoCreateIssueLine" TextAlign="Center" Type="CheckBox" AllowShowHide="Server" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsFree" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="SiteID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="LocationID" AllowShowHide="Server" NullText="<SPLIT>" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="UOM" CommitChanges="True" AllowDragDrop="true"/>
                                    <px:PXGridColumn AllowNull="False" DataField="OrderQty" TextAlign="Right" CommitChanges="True" AllowDragDrop="true"/>
                                    <px:PXGridColumn DataField="BaseOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShippedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="OpenQty" TextAlign="Right" AllowNull="False" />
                                    <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryUnitPrice" TextAlign="Right" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="ManualPrice" TextAlign="Center" AllowNull="False" Type="CheckBox" CommitChanges="True"/>
                                    <px:PXGridColumn AllowNull="False" DataField="CuryExtPrice" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="DiscPct" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" AllowNull="False" />
                                    <px:PXGridColumn DataField="DiscountID" TextAlign="Left" CommitChanges="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DiscountSequenceID" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ManualDisc" TextAlign="Center" AllowNull="False" CommitChanges="True" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryDiscPrice" NullText="0.0" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AvgCost" TextAlign="Right" NullText="0.0" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryLineAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="DRTermStartDate" CommitChanges="true" />
                                    <px:PXGridColumn DataField="DRTermEndDate" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryUnbilledAmt" AllowNull="False" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RequestDate" />
                                    <px:PXGridColumn DataField="ShipDate" />
                                    <px:PXGridColumn DataField="ShipComplete" RenderEditorText="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="CompleteQtyMin" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CompleteQtyMax" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="Completed" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="POCreate" AllowNull="False" CommitChanges="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="POSource" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" NullText="&lt;SPLIT&gt;" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ExpireDate" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="ReasonCode" CommitChanges ="true" />
                                    <px:PXGridColumn DataField="SalesPersonID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TaxCategoryID" />
                                    <px:PXGridColumn DataField="AvalaraCustomerUsageType" />
                                    <px:PXGridColumn DataField="Commissionable" AllowNull="False" CommitChanges="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AlternateID" />
                                    <px:PXGridColumn DataField="SalesAcctID" CommitChanges ="True" />
                                    <px:PXGridColumn DataField="SalesSubID" />
                                    <px:PXGridColumn DataField="TaskID" Label="Task" CommitChanges ="True" />
                                    <px:PXGridColumn DataField="CostCodeID" Label="Task" />
                                    <px:PXGridColumn DataField="CuryUnitPriceDR" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="DiscPctDR" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="IsRUTROTDeductible" Type="Checkbox" AutoCallBack="True" CommitChanges="true" />
                                    <px:PXGridColumn DataField="RUTROTItemType" AutoCallBack="True" CommitChanges="true" />
                                    <px:PXGridColumn DataField="RUTROTWorkTypeID" AutoCallBack="True" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryRUTROTAvailableAmt" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Allocations" Key="cmdLS" CommandName="LSSOLine_binLotSerial" CommandSourceID="ds" DependOnGrid="grid">
                                    <AutoCallBack>
                                        <Behavior CommitChanges="True" PostData="Page" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Add Invoice" CommandSourceID="ds" CommandName="AddInvoice" />
                                <px:PXToolBarButton Text="Add Item" Key="cmdASI">
                                    <AutoCallBack Command="AddInvBySite" Target="ds">
                                        <Behavior CommitChanges="True" PostData="Page" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Add Matrix Item" CommandSourceID="ds" CommandName="ShowMatrixPanel" />
                                <px:PXToolBarButton Text="PO Link" DependOnGrid="grid" StateColumn="POCreate">
                                    <AutoCallBack Command="POSupplyOK" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Inventory Summary" DependOnGrid="grid" StateColumn="IsStockItem">
                                    <AutoCallBack Command="InventorySummary" Target="ds" />
                                </px:PXToolBarButton>

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
                        <CallbackCommands PasteCommand="PasteLine">
                            <Save PostData="Container" />
                        </CallbackCommands>
                        <Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" AllowDragRows="true" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tax Details" VisibleExp="DataControls[&quot;edOrderType&quot;].Value!=TR" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" TabIndex="200" Width="100%" BorderWidth="0px"
                        SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" CommitChanges="true" AutoRefresh="true"/>
                                    <px:PXNumberEdit SuppressLabel="True" ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" AllowUpdate="False" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryExemptedAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="TaxUOM" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxableQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar PagerGroup="3" PagerOrder="2">
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Commissions" VisibleExp="DataControls[&quot;edOrderType&quot;].Value!=TR" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXFormView ID="Commission" runat="server" DataMember="CurrentDocument" RenderStyle="Simple" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXSegmentMask CommitChanges="True" ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="gridSalesPerTran" runat="server" Height="200px" Width="100%" DataSourceID="ds" BorderWidth="0px" SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="SalesPerTran">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edCommnPct" runat="server" DataField="CommnPct" AllowNull="True" />
                                    <px:PXNumberEdit ID="edCommnAmt" runat="server" DataField="CommnAmt" />
                                    <px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" />
                                    <px:PXNumberEdit ID="edCommnblAmt" runat="server" DataField="CommnblAmt" />
                                    <px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" AllowNull="True" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edSalesPersonID_1" runat="server" DataField="SalespersonID" AutoRefresh="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SalespersonID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CommnPct" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryCommnAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CuryCommnblAmt" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinWidth="100" />
                        <Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXFormView ID="formC" runat="server" Caption="Bill-To Contact" DataMember="Billing_Contact" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXCheckBox ID="chkOverrideContact" runat="server" CommitChanges="True" DataField="OverrideContact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXFormView ID="formA" DataMember="Billing_Address" runat="server" DataSourceID="ds" Caption="Bill-To Address" RenderStyle="Fieldset" SyncPosition="True">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
                            <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" DataSourceID="ds" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" DataSourceID="ds">
                                <CallBackMode PostData="Container" />
                                <Parameters>
                                    <px:PXControlParam ControlID="formA" Name="SOBillingAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
                                        Type="String" />
                                </Parameters>
                            </px:PXSelector>
                            <px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" />
                    <px:PXPanel ID="PXPanel1" runat="server" Caption="Financial Information" RenderStyle="Fieldset" DataMember="">
                        <px:PXFormView ID="formE" runat="server" CaptionVisible="False" DataMember="CurrentDocument" DataSourceID="ds" RenderStyle="Simple"  MarkRequired="Dynamic">
                            <Template>
                                <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                                <px:PXSegmentMask ID="edBranchID" runat="server" CommitChanges="True" DataField="BranchID" DataSourceID="ds" />
                                <px:PXCheckBox ID="chkOverrideTaxZone" runat="server" CommitChanges="True" DataField="OverrideTaxZone" />
                                <px:PXSelector ID="edTaxZoneID" runat="server" CommitChanges="True" DataField="TaxZoneID" DataSourceID="ds" />
                                <px:PXDropDown ID="edTaxCalcMode" runat="server" CommitChanges="True" DataField="TaxCalcMode" />
                                <px:PXDropDown ID="edAvalaraCustomerUsageTypeID" runat="server" CommitChanges="True" DataField="AvalaraCustomerUsageType" />
                                <px:PXCheckBox ID="chkBillSeparately" runat="server" CommitChanges="True" DataField="BillSeparately" />
                                <px:PXTextEdit ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
                                <px:PXDateTimeEdit ID="edInvoiceDate" runat="server" CommitChanges="True" DataField="InvoiceDate" />
                                <px:PXSelector ID="edTermsID" runat="server" CommitChanges="True" DataField="TermsID" DataSourceID="ds" />
                                <px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" />
                                <px:PXDateTimeEdit ID="edDiscDate" runat="server" DataField="DiscDate" />
                                <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
                                <px:PXTextEdit ID="edOrigOrderType" runat="server" DataField="OrigOrderType" Enabled="False" />
                                <px:PXSelector ID="edOrigOrderNbr" runat="server" DataField="OrigOrderNbr" Enabled="False" AllowEdit="true" />
                                <px:PXCheckBox ID="chkEmailed" runat="server" DataField="Emailed" Height="18px" />
                                <px:PXSelector ID="edWorkgroupID" runat="server" AutoRefresh="True" DataField="WorkgroupID" DataSourceID="ds" CommitChanges="true" />
                                <px:PXSelector ID="edOwnerID" runat="server" AutoRefresh="True" DataField="OwnerID" DataSourceID="ds" CommitChanges="true" />
                            </Template>
                            <ContentStyle BackColor="Transparent" BorderStyle="None">
                            </ContentStyle>
                        </px:PXFormView>
                    </px:PXPanel>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Payment Settings" VisibleExp="DataControls[&quot;edOrderType&quot;].Value!=TR" BindingContext="form">
                <Template>
                    <px:PXFormView ID="formN" runat="server" DataSourceID="ds" DataMember="CurrentDocument" CaptionVisible="False" SkinID="Transparent" MarkRequired="Dynamic">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
                            <px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AutoRefresh="True"
                                DataSourceID="ds" />
                            <px:PXSelector CommitChanges="True" ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr" AutoRefresh="True"
                                AutoGenerateColumns="True" DataSourceID="ds" />
                            <px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" DataSourceID="ds" />
                            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
                            <px:PXTextEdit CommitChanges="True" ID="edCCCardNumber" runat="server" DataField="CCCardNumber" />
                            <px:PXCheckBox CommitChanges="True" ID="chkCreatePMInstance" runat="server" DataField="CreatePMInstance" />
                            <px:PXFormView ID="frmDefPMInstance" runat="server" CaptionVisible="False" DataMember="DefPaymentMethodInstance"
                                DataSourceID="ds" RenderStyle="Fieldset" Caption="Card Info">
                                <Template>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                                    <px:PXSelector CommitChanges="True" ID="edCCProcessingCenterID" runat="server" DataField="CCProcessingCenterID" AutoRefresh="True"
                                        DataSourceID="ds" />
                                    <px:PXTextEdit ID="edDescr" runat="server" AllowNull="False" DataField="Descr" />
                                </Template>
                                <ContentStyle BorderStyle="None" />
                            </px:PXFormView>
                            <px:PXFormView ID="rbtnFrom" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="InputModeFilter">
                                <Template>
                                    <px:PXGroupBox runat="server" DataField="InputMode" RenderStyle="Simple" ID="gbInputMode" CommitChanges="True" RenderSimple="True">
                                        <ContentLayout Layout="Stack" Orientation="Horizontal" />
                                        <Template>
                                            <px:PXRadioButton runat="server" Value="T" ID="gbInputMode_opT" GroupName="gbInputMode" />
                                            <px:PXRadioButton runat="server" Value="D" ID="gbInputMode_opD" GroupName="gbInputMode" />
                                        </Template>
                                    </px:PXGroupBox>
                                </Template>
                            </px:PXFormView>
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXGrid ID="grdPMInstanceDetails" runat="server" DataSourceID="ds" SkinID="Attributes" Width="360px" Height="121px">
                                <ActionBar Position="Top" ActionsText="False">
                                    <Actions>
                                        <Save Enabled="False" />
                                        <Search Enabled="False" />
                                        <NoteShow Enabled="False" />
                                        <Refresh GroupIndex="2" Order="1" />
                                        <AddNew Enabled="False" />
                                        <Delete Enabled="False" />
                                        <AdjustColumns Enabled="False" />
                                        <ExportExcel Enabled="False" />
                                        <Upload Enabled="False" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton Text="Create CC Payment Method HF" CommandSourceID="ds" CommandName="CreateCCPaymentMethodHF">
                                            <PopupCommand Target="ds" Command="SyncCCPaymentMethods" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="DefPaymentMethodInstanceDetails">
                                        <Columns>
                                            <px:PXGridColumn AllowUpdate="False" DataField="DetailID" />
                                            <px:PXGridColumn DataField="Value" RenderEditorText="True" CommitChanges="True" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edCCPaymentStateDescr" runat="server" DataField="CCPaymentStateDescr" Enabled="False" />
                            <px:PXTextEdit ID="edPCResponseReasonText" runat="server" DataField="PCResponseReasonText" Enabled="False" />
                            <px:PXTextEdit CommitChanges="True" ID="edPreAuthTranNumber" runat="server" DataField="PreAuthTranNumber" />
                            <px:PXDateTimeEdit ID="edCCAuthExpirationDate" runat="server" DataField="CCAuthExpirationDate" />
                            <px:PXNumberEdit ID="edCuryCCPreAuthAmount" runat="server" DataField="CuryCCPreAuthAmount" />
                            <px:PXNumberEdit ID="edCuryPaymentTotal1" runat="server" Enabled="False" DataField="CuryPaymentTotal" />
                            <px:PXNumberEdit ID="edCuryUnpaidBalance" runat="server" DataField="CuryUnpaidBalance" Enabled="False" />
                            <px:PXTextEdit CommitChanges="True" ID="edCaptureTranNumber" runat="server" DataField="CaptureTranNumber" Enabled="False" />
                            <px:PXSelector ID="edRefTranExtNbr" runat="server" DataField="RefTranExtNbr" AutoRefresh="True" DataSourceID="ds" CommitChanges="True" />
                            <px:PXButton ID="pbAutorizeCCPayment" runat="server" Text="Authorize Payment" CommandName="AuthorizeCCPayment" CommandSourceID="ds" />
                            <px:PXButton ID="pbCaptureCCPayment" runat="server" Text="Capture Payment" CommandName="CaptureCCPayment" CommandSourceID="ds" />
                            <px:PXButton ID="pbVoidCCPayment" runat="server" Text="Void Authorization" CommandName="VoidCCPayment" CommandSourceID="ds" />
                            <px:PXButton ID="pbCreditCCPayment" runat="server" Text="Refund Payment" CommandName="CreditCCPayment" CommandSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Shipping Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" />
                    <px:PXFormView ID="formD" runat="server" Caption="Ship-To Contact" DataMember="Shipping_Contact" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXFormView ID="formB" DataMember="Shipping_Address" runat="server" DataSourceID="ds" Caption="Ship-To Address" RenderStyle="Fieldset" SyncPosition="True">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
                            <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" DataSourceID="ds" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" DataSourceID="ds" CommitChanges="True">
                                <CallBackMode PostData="Container" />
                                <Parameters>
                                    <px:PXControlParam ControlID="formB" Name="SOShippingAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
                                        Type="String" />
                                </Parameters>
                            </px:PXSelector>
                            <px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Shipping Information" />
                    <px:PXFormView ID="formF" runat="server" DataSourceID="ds" DataMember="CurrentDocument" CaptionVisible="False" AllowCollapse="False">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXDateTimeEdit CommitChanges="True" ID="edShipDate" runat="server" DataField="ShipDate" />
                            <px:PXCheckBox ID="chkShipSeparately" runat="server" DataField="ShipSeparately" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXDropDown ID="edShipComplete" runat="server" AllowNull="False" DataField="ShipComplete" SelectedIndex="2" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXDateTimeEdit ID="edCancelDate" runat="server" DataField="CancelDate" />
                            <px:PXCheckBox Height="20px" ID="chkCancelled" runat="server"
                                DataField="Cancelled">
                                <AutoCallBack Command="Cancelled" Target="ds">
                                </AutoCallBack>
                            </px:PXCheckBox>
                            <px:PXLayoutRule runat="server" />
                            <px:PXSegmentMask CommitChanges="True" ID="edDefaultSiteID" runat="server" DataField="DefaultSiteID" DataSourceID="ds" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXSelector CommitChanges="True" Size="s" ID="edShipVia" runat="server" DataField="ShipVia" DataSourceID="ds" />
                            <px:PXButton ID="shopRates" runat="server" Text="Shop For Rates" CommandName="ShopRates" CommandSourceID="ds" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXCheckBox ID="edWillCall" runat="server" DataField="WillCall" Tooltip="The Will Call flag depends on the Common Carrier selection in the Ship Via field." Width="80px" />
                            <px:PXDropDown runat="server" ID="edFreightClass" DataField="FreightClass" />
                            <px:PXSelector ID="edFOBPoint" runat="server" DataField="FOBPoint" DataSourceID="ds" />
                            <px:PXNumberEdit ID="edPriority" runat="server" DataField="Priority" />
                            <px:PXSelector CommitChanges="True" ID="edShipTermsID" runat="server" DataField="ShipTermsID" DataSourceID="ds" AutoRefresh="True" />
                            <px:PXSelector CommitChanges="True" ID="edShipZoneID" runat="server" DataField="ShipZoneID" DataSourceID="ds" />
                            <px:PXCheckBox ID="chkResedential" runat="server" DataField="Resedential" />
                            <px:PXCheckBox ID="chkSaturdayDelivery" runat="server" DataField="SaturdayDelivery" />
                            <px:PXCheckBox ID="chkInsurance" runat="server" DataField="Insurance" />
                            <px:PXCheckBox CommitChanges="True" ID="chkUseCustomerAccount" runat="server" DataField="UseCustomerAccount" />
                            <px:PXCheckBox CommitChanges="True" ID="chkGroundCollect" runat="server" DataField="GroundCollect" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Discount Details" VisibleExp="DataControls[&quot;edOrderType&quot;].Value!=TR" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXFormView ID="DiscountParameters" runat="server" DataMember="CurrentDocument" RenderStyle="Simple" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox ID="chkDisableAutomaticDiscountCalculation" runat="server" DataField="DisableAutomaticDiscountCalculation" AlignLeft="true" CommitChanges="true" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="DiscountDetails">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXCheckBox ID="chkSkipDiscount" runat="server" DataField="SkipDiscount" />
                                    <px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID"
                                        AllowEdit="True" edit="1" />
                                    <px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" AllowEdit="True" AutoRefresh="True" edit="1" />
                                    <px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
                                    <px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
                                    <px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
                                    <px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
                                    <px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" CommitChanges="true" />
                                    <px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" CommitChanges="true" />
                                    <px:PXSegmentMask ID="edFreeItemID" runat="server" DataField="FreeItemID" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edFreeItemQty" runat="server" DataField="FreeItemQty" />
                                    <px:PXTextEdit ID="edExtDiscCode" runat="server" DataField="ExtDiscCode" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SkipDiscount" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="LineNbr" />
                                    <px:PXGridColumn DataField="DiscountID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DiscountSequenceID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="IsManual" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="DiscountPct" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="FreeItemID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
                                    <px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ExtDiscCode" />
                                    <px:PXGridColumn DataField="Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Shipments">
                <Template>
                    <px:PXGrid ID="grid5" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details"
                        BorderWidth="0px" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="ShipmentList">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXTextEdit ID="edOrderType3" runat="server" DataField="OrderType" Enabled="False" />
                                    <px:PXTextEdit ID="edOrderNbr3" runat="server" DataField="OrderNbr" Enabled="False" />
                                    <px:PXSelector SuppressLabel="True" Size="s" ID="edInvoiceNbr3" runat="server"
                                        DataField="InvoiceNbr" AutoRefresh="True"
                                        AllowEdit="True" edit="1" />
                                    <px:PXSelector SuppressLabel="True" Size="s" ID="edInvtRefNbr3" runat="server"
                                        DataField="InvtRefNbr" AutoRefresh="True"
                                        AllowEdit="True" edit="1">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="grid5" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXLayoutRule runat="server" />
                                    <px:PXNumberEdit ID="edShipmentQty3" runat="server" DataField="ShipmentQty" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ShipmentType" />
                                    <px:PXGridColumn DataField="ShipmentNbr" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="DisplayShippingRefNoteID" RenderEditorText="True"
                                        LinkCommand="SOOrderShipment~DisplayShippingRefNoteID~Link" />
                                    <px:PXGridColumn DataField="SOShipment__StatusIsNull" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Operation" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrderType" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OrderNbr" />
                                    <px:PXGridColumn DataField="ShipDate" Label="Ship Date" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShipmentQty" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShipmentWeight" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="ShipmentVolume" TextAlign="Right" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvoiceType" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvoiceNbr" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvtDocType" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InvtRefNbr" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar PagerGroup="3" PagerOrder="2">
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Payments" VisibleExp="DataControls[&quot;edOrderType&quot;].Value!=TR" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="detgrid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 332px;" Width="100%"
                        BorderWidth="0px" SkinID="Details" Height="332px" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Adjustments">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXTextEdit ID="edAdjdOrderType" runat="server" DataField="AdjdOrderType" />
                                    <px:PXDropDown ID="edARPayment__DocType" runat="server" DataField="ARPayment__DocType" />
                                    <px:PXSelector ID="edARPayment__PaymentMethodID" runat="server" DataField="ARPayment__PaymentMethodID" Enabled="False" />
                                    <px:PXTextEdit ID="edAdjdOrderNbr" runat="server" DataField="AdjdOrderNbr" />
                                    <px:PXSelector ID="edARPayment__RefNbr" runat="server" DataField="ARPayment__RefNbr" AllowEdit="True" edit="1" />
                                    <px:PXDropDown ID="edAdjgDocType" runat="server" DataField="AdjgDocType" />
                                    <px:PXDropDown ID="edARPayment__Status" runat="server" AllowNull="False" DataField="ARPayment__Status" Enabled="False" />
                                    <px:PXSelector ID="edAdjgRefNbr" runat="server" AutoRefresh="True" DataField="AdjgRefNbr">
                                        <Parameters>
                                            <px:PXControlParam ControlID="detgrid" Name="SOAdjust.adjgDocType" PropertyName="DataValues[&quot;AdjgDocType&quot;]" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edARPayment__CashAccountID" runat="server" DataField="ARPayment__CashAccountID" />
                                    <px:PXTextEdit ID="edARPayment__ExtRefNbr" runat="server" DataField="ARPayment__ExtRefNbr" />
                                    <px:PXNumberEdit ID="edCustomerID" runat="server" DataField="CustomerID" />
                                    <px:PXNumberEdit ID="edCuryAdjdAmt" runat="server" DataField="CuryAdjdAmt" />
                                    <px:PXNumberEdit ID="edCuryAdjdBilledAmt" runat="server" DataField="CuryAdjdBilledAmt" />
                                    <px:PXNumberEdit ID="edAdjAmt" runat="server" DataField="AdjAmt" />
                                    <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AdjgDocType" Label="ARPayment-Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjgRefNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" RenderEditorText="True" Label="Reference Nbr." LinkCommand="ViewPayment" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CuryAdjdAmt" Label="Applied To Order" AllowNull="False" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryAdjdBilledAmt" Label="Transferred to Invoice" AllowNull="False" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDocBal" Label="Balance" AllowNull="False" AllowUpdate="False" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARPayment__Status" Label="Status" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="ARPayment__ExtRefNbr" Label="Payment Ref." />
                                    <px:PXGridColumn AllowUpdate="False" DataField="ARPayment__PaymentMethodID" DisplayFormat="&gt;aaaaaaaaaa" Label="ARPayment-Payment Method" />
                                    <px:PXGridColumn DataField="ARPayment__CashAccountID" DisplayFormat="&gt;######" Label="Cash Account" />
                                    <px:PXGridColumn DataField="ARPayment__CuryOrigDocAmt" Label="Orig. Amount" />
                                    <px:PXGridColumn DataField="ARPayment__CuryID" Label="Currency ID" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Create Payment">
                                    <AutoCallBack Command="CreatePayment" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                    <PopupCommand Target="detgrid" Command="Refresh">
                                    </PopupCommand>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Create Prepayment">
                                    <AutoCallBack Command="CreatePrepayment" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                    <PopupCommand Target="detgrid" Command="Refresh">
                                    </PopupCommand>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="View Payment" CommandName="ViewPayment" CommandSourceID="ds">
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Totals">
                <Template>
                    <px:PXFormView ID="formG" runat="server" DataSourceID="ds" Style="z-index: 100; left: 18px; top: 36px;" Width="100%" DataMember="CurrentDocument"
                        CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" GroupCaption="Order Totals" />
                            <px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryMiscTot" runat="server" Enabled="False" DataField="CuryMiscTot" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" Size="Empty" />
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="SM" GroupCaption="Freight Info" />
                            <px:PXNumberEdit ID="edOrderWeight" runat="server" DataField="OrderWeight" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edOrderVolume" runat="server" DataField="OrderVolume" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit CommitChanges="True" ID="edPackageWeight" runat="server" DataField="PackageWeight" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryFreightCost" runat="server" DataField="CuryFreightCost" CommitChanges="true" Size="Empty" />
                            <px:PXButton ID="checkFreightRate" runat="server" Text="Check Freight Rate" CommandName="CalculateFreight" CommandSourceID="ds" />
                            <px:PXCheckBox ID="chkFreightCostIsValid" runat="server" DataField="FreightCostIsValid" />
							<px:PXCheckBox ID="chkOverrideFreightAmount" runat="server" DataField="OverrideFreightAmount" CommitChanges="True" />
							<px:PXDropDown ID="edFreightAmountSource" runat="server" DataField="FreightAmountSource" />
							<px:PXNumberEdit CommitChanges="True" ID="edCuryFreightAmt" runat="server" DataField="CuryFreightAmt" Size="Empty" />
                            <px:PXNumberEdit CommitChanges="True" ID="edCuryPremiumFreightAmt" runat="server" DataField="CuryPremiumFreightAmt" Size="Empty" />
                            <px:PXSelector CommitChanges="True" ID="edFreightTaxCategoryID" runat="server" DataField="FreightTaxCategoryID" DataSourceID="ds" Size="SMM"/>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Calculated Amounts and Quantities" />
                            <px:PXNumberEdit ID="edOpenOrderQty" runat="server" Enabled="False" DataField="OpenOrderQty" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryOpenOrderTotal" runat="server" Enabled="False" DataField="CuryOpenOrderTotal" Size="Empty" />
                            <px:PXNumberEdit ID="edUnbilledOrderQty" runat="server" Enabled="False" DataField="UnbilledOrderQty" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryUnbilledOrderTotal" runat="server" Enabled="False" DataField="CuryUnbilledOrderTotal" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryPaymentTotal" runat="server" Enabled="False" DataField="CuryPaymentTotal" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryCCPreAuthAmount1" runat="server" DataField="CuryCCPreAuthAmount" Enabled="False" Size="Empty" />
                            <px:PXNumberEdit ID="edCuryUnpaidBalance1" runat="server" DataField="CuryUnpaidBalance" Enabled="False" Size="Empty" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" Style="left: 0px; top: 0px;">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="Approval">
                                <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                    <px:PXGridColumn DataField="ApproveDate" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="ROT/RUT Details" VisibleExp="DataControls[&quot;chkRUTROT&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXFormView runat="server" SkinID="Transparent" ID="RUTROTForm" DataSourceID="ds" DataMember="Rutrots">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" GroupCaption="RUT and ROT Settings" />
                            <px:PXCheckBox runat="server" DataField="AutoDistribution" CommitChanges="True" ID="chkRRAutoDistribution" AlignLeft="true" />
                            <px:PXGroupBox runat="server" DataField="RUTROTType" CommitChanges="True" RenderStyle="Simple" ID="gbRRType">
                                <ContentLayout Layout="Stack" Orientation="Horizontal" />
                                <Template>
                                    <px:PXRadioButton runat="server" Value="O" ID="gbRRType_opO" GroupName="gbRRType" Text="ROT" />
                                    <px:PXRadioButton runat="server" Value="U" ID="gbRRType_opU" GroupName="gbRRType" Text="RUT" />
                                </Template>
                            </px:PXGroupBox>
                            <px:PXTextEdit runat="server" DataField="ROTAppartment" ID="edRAppartment" />
                            <px:PXTextEdit runat="server" DataField="ROTEstate" ID="edRREstate" />
                            <px:PXTextEdit runat="server" DataField="ROTOrganizationNbr" ID="edRROrganizationNbr" CommitChanges="true" />
                            <px:PXLayoutRule runat="server" GroupCaption="RUT and ROT Distribution" />
                            <px:PXGrid runat="server" DataSourceID="ds" Width="350px" AllowFilter="false" AllowSearch="false" Height="200px" SkinID="DetailsInTab" ID="gridDistribution"
                                Caption="RUT and ROT Distribution" CaptionVisible="false">
                                <Mode InitNewRow="true" />
                                <Levels>
                                    <px:PXGridLevel DataMember="RRDistribution">
                                        <RowTemplate>
                                            <px:PXTextEdit runat="server" DataField="PersonalID" ID="edPersonalID" />
                                            <px:PXNumberEdit runat="server" DataField="CuryAmount" ID="edAmount" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="PersonalID" />
                                            <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" />
                                            <px:PXGridColumn DataField="Extra" Type="CheckBox" />
                                        </Columns>

                                    </px:PXGridLevel>
                                </Levels>
                                <ActionBar>
                                    <Actions>
                                        <ExportExcel Enabled="false" />
                                        <AddNew Enabled="true" />
                                        <Delete Enabled="true" />
                                        <AdjustColumns Enabled="true" />
                                    </Actions>
                                </ActionBar>
                            </px:PXGrid>
                            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" GroupCaption="RUT and ROT Totals" />
                            <px:PXNumberEdit runat="server" DataField="DeductionPct" CommitChanges="True" ID="edRRDeduction" />
                            <px:PXNumberEdit runat="server" DataField="CuryTotalAmt" ID="edRRTotalAmt" Enabled="false" />
                            <px:PXNumberEdit runat="server" DataField="CuryOtherCost" ID="edRUTROTOtherCost" Enabled="false" />
                            <px:PXNumberEdit runat="server" DataField="CuryMaterialCost" ID="edRUTROTMaterialCost" Enabled="false" />
                            <px:PXNumberEdit runat="server" DataField="CuryWorkPrice" ID="edRUTROTWorkPrice" Enabled="false" />
                            <px:PXNumberEdit runat="server" DataField="CuryDistributedAmt" ID="edRRAvailAmt" Enabled="false" />
                            <px:PXNumberEdit runat="server" DataField="CuryUndistributedAmt" ID="edRRUndsitributedAmt" Enabled="false" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
            <Search CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Enabled="True" Container="Window" />
    </px:PXTab>
    <%-- PanelPOSupply --%>
    <px:PXSmartPanel ID="PanelPOSupply" runat="server" Width="960px" Height="360px" Caption="Purchasing Details" CaptionVisible="True"
        LoadOnDemand="True" ShowAfterLoad="True" AutoCallBack-Target="formCurrentPOSupply" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" Key="currentposupply" TabIndex="3100">
        <px:PXFormView ID="formCurrentPOSupply" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="currentposupply"
            Caption="Purchasing Settings" CaptionVisible="False" SkinID="Transparent">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edPOSource" runat="server" DataField="POSource" />
                <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
                <px:PXSegmentMask CommitChanges="True" ID="edPOSiteID" runat="server" DataField="POSiteID" AutoRefresh="True" />
                <%--                <px:PXDropDown ID="edPOType" runat="server" DataField="POType" Enabled="False" />
                <px:PXSelector ID="edPONbr" runat="server" DataField="PONbr" AutoRefresh="True" AllowEdit="True" />--%>
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridPOSupply" runat="server" Height="360px" Width="100%" DataSourceID="ds" AutoAdjustColumns="true">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataMember="posupply">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Type="CheckBox" TextAlign="Center" />
                        <px:PXGridColumn DataField="OrderType" />
                        <px:PXGridColumn DataField="OrderNbr" />
                        <px:PXGridColumn DataField="VendorRefNbr" />
                        <px:PXGridColumn AllowNull="False" DataField="LineType" />
                        <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                        <px:PXGridColumn DataField="VendorID" />
                        <px:PXGridColumn DataField="VendorID_Vendor_AcctName" />
                        <px:PXGridColumn DataField="PromisedDate" />
                        <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn DataField="OrderQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="OpenQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="TranDesc" />
                    </Columns>
                    <RowTemplate>
                        <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" AllowEdit="True" />
                    </RowTemplate>
                    <Layout ColumnsMenu="False" />
                    <Mode AllowAddNew="false" AllowDelete="false" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton7" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Bin/Lot/Serial Numbers --%>
    <px:PXSmartPanel ID="PanelLS" runat="server" Width="90%" Height="500px" Caption="Allocations" CaptionVisible="True" Key="lsselect"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="optform" DesignView="Content" TabIndex="3200">
        <px:PXFormView ID="optform" runat="server" CaptionVisible="False" DataMember="LSSOLine_lotseropts" DataSourceID="ds" SkinID="Transparent"
            TabIndex="-3236" Width="100%">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
                <px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False" />
                <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
                    <AutoCallBack>
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXNumberEdit>
                <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
                <px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal" />
                <px:PXButton ID="btnGenerate" runat="server" CommandName="LSSOLine_generateLotSerial" CommandSourceID="ds" Height="20px"
                    Text="Generate" />
            </Template>
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
        </px:PXFormView>
        <px:PXGrid ID="grid2" runat="server" AutoAdjustColumns="True" DataSourceID="ds" TabIndex="-3036" Width="100%" AllowFilter="true" SkinID="Details" SyncPosition="true">
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
            <Levels>
                <px:PXGridLevel DataKeyNames="OrderType,OrderNbr,LineNbr,SplitLineNbr" DataMember="splits">
                    <RowTemplate>
                        <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                        <px:PXSegmentMask ID="edSubItemID2" runat="server" AutoRefresh="True" DataField="SubItemID" />
                        <px:PXSegmentMask ID="edSiteID2" runat="server" AutoRefresh="True" DataField="SiteID" />
                        <px:PXSegmentMask ID="edLocationID2" runat="server" AutoRefresh="True" DataField="LocationID">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="SOLineSplit.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSegmentMask>
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty" />
                        <px:PXSelector ID="edUOM2" runat="server" AutoRefresh="True" DataField="UOM">
                            <Parameters>
                                <px:PXControlParam ControlID="grid" Name="SOLine.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXSelector ID="edLotSerialNbr2" runat="server" AutoRefresh="True" DataField="LotSerialNbr">
                            <Parameters>
                                <px:PXControlParam ControlID="grid2" Name="SOLineSplit.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOLineSplit.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                <px:PXControlParam ControlID="grid2" Name="SOLineSplit.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                            </Parameters>
                        </px:PXSelector>
                        <px:PXDateTimeEdit ID="edExpireDate2" runat="server" DataField="ExpireDate" />
                    </RowTemplate>
                    <Columns>
                        <px:PXGridColumn DataField="SplitLineNbr" TextAlign="Right" />
                        <px:PXGridColumn DataField="ParentSplitLineNbr" TextAlign="Right" />
                        <px:PXGridColumn DataField="InventoryID" />
                        <px:PXGridColumn DataField="SubItemID" />
                        <px:PXGridColumn DataField="ShipDate" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" AutoCallBack="True" DataField="IsAllocated" TextAlign="Center"
                            Type="CheckBox" />
                        <px:PXGridColumn DataField="SiteID" AutoCallBack="True" />
                        <px:PXGridColumn AllowNull="False" AllowShowHide="Server" DataField="Completed" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn AllowShowHide="Server" DataField="LocationID" />
                        <px:PXGridColumn DataField="LotSerialNbr" AutoCallBack="True" />
                        <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                        <px:PXGridColumn DataField="ShippedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="ReceivedQty" TextAlign="Right" />
                        <px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="ExpireDate" />
                        <px:PXGridColumn AllowNull="False" DataField="POCreate" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="SOLineSplit$RefNoteID$Link" />
                    </Columns>
                    <Layout FormViewHeight="" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" />
            <Mode InitNewRow="True" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Specify Shipment Parameters --%>
    <px:PXSmartPanel ID="pnlCreateShipment" runat="server" Caption="Specify Shipment Parameters"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="soparamfilter" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formCreateShipment" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
        <px:PXFormView ID="formCreateShipment" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="soparamfilter">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule44" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDateTimeEdit ID="edShipDate" runat="server" DataField="ShipDate" />
                <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="true" ValueField="INSite__SiteCD" HintField="INSite__descr">
                    <GridProperties FastFilterFields="Descr">
                    </GridProperties>
                </px:PXSelector>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
                <AutoCallBack Target="formCreateShipment" Command="Save" />
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Specify Parameters for Quick Process--%>
    <px:PXSmartPanel ID="PXSmartPanel1" runat="server" Caption="Process Order" ShowAfterLoad="true"
        CaptionVisible="true" DesignView="Hidden" Key="QuickProcessParameters"
        AutoCallBack-Enabled="true" AutoCallBack-Target="fromQuickProcess" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
        <px:PXFormView ID="fromQuickProcess" runat="server" DataSourceID="ds" Style="z-index: 100" Width="600px" CaptionVisible="False" DataMember="QuickProcessParameters" AllowCollapse="False">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
	            <px:PXLayoutRule runat="server" StartGroup="True" ColumnSpan="2" LabelsWidth="100px" />
                <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="true" ValueField="INSite__SiteCD" HintField="INSite__descr" CommitChanges="True" Style="margin-bottom: 20px">
                    <GridProperties FastFilterFields="Descr">
                    </GridProperties>
                </px:PXSelector>
	            <px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" ColumnWidth="260"/>
                <px:PXGroupBox ID="gbShipDate" runat="server" Caption="Shipment Date" DataField="ShipDateMode" CommitChanges="True" RenderStyle="Fieldset">
                    <Template>
                        <px:PXRadioButton ID="gbShipDate_Today" runat="server" Text="Today" Value="0" GroupName="gbShipDate"/>
                        <px:PXRadioButton ID="gbShipDate_Tommorow" runat="server" Text="Tomorrow" Value="1" GroupName="gbShipDate" />
                        <px:PXLayoutRule runat="server" StartGroup="False" StartColumn="False" SuppressLabel="False" Merge="True" />
                        <px:PXRadioButton ID="gbShipDate_Custom" runat="server" Text="Custom" Value="2" GroupName="gbShipDate" />
                        <px:PXDateTimeEdit ID="edShipDate" runat="server" DataField="ShipDate" CommitChanges="True" SuppressLabel="True" />
                    </Template>
                    <ContentLayout Layout="Stack" />
                </px:PXGroupBox>
	            <px:PXLayoutRule runat="server" GroupCaption="Printing settings" StartColumn="True" LabelsWidth="XS" ControlSize="SM" ColumnWidth="260" StartGroup="True" />
	            <px:PXCheckBox ID="edPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" CommitChanges="True" AlignLeft="true"/>
	            <px:PXCheckBox ID="PXDefinePrinterAutomatically" runat="server" DataField="DefinePrinterManually" CommitChanges="True" AlignLeft="true"/>
	            <px:PXSelector ID="edPrinterID" runat="server" DataField="PrinterID" CommitChanges="True"/>
				<px:PXTextEdit CommitChanges="true" ID="edNumberOfCopies" runat="server" DataField="NumberOfCopies" />

	            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Availability" ColumnSpan="2" StartRow="True" />
	            <px:PXLayoutRule runat="server" Merge="True"/>
	            <px:PXCheckBox ID="edGreenStatus" runat="server" DataField="GreenStatus" RenderStyle="Button" AlignLeft="True" Enabled="False">
		            <UncheckImages Normal="main@Success" />
		            <CheckImages Normal="main@Success" />
	            </px:PXCheckBox>
	            <px:PXCheckBox ID="edYellowStatus" runat="server" DataField="YellowStatus" RenderStyle="Button" AlignLeft="True" Enabled="False">
		            <UncheckImages Normal="control@Warning" />
		            <CheckImages Normal="control@Warning" />
	            </px:PXCheckBox>
	            <px:PXCheckBox ID="edRedStatus" runat="server" DataField="RedStatus" RenderStyle="Button" AlignLeft="True" Enabled="False">
		            <UncheckImages Normal="main@Fail" />
		            <CheckImages Normal="main@Fail" />
	            </px:PXCheckBox>
	            <px:PXTextEdit ID="edAvailMsg" runat="server" DataField="AvailabilityMessage" TextMode="MultiLine" SuppressLabel="True" Height="60" Width="500" TextAlign="Justify" Style="border-width:0; resize:none"/>
	            <px:PXLayoutRule runat="server" EndGroup="True" />
	            <px:PXLayoutRule runat="server" EndGroup="True" />

                <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping" StartRow="True" SuppressLabel="True" ColumnWidth="260" />
                <px:PXCheckBox ID="edCreateShipment" runat="server" DataField="CreateShipment" CommitChanges="True" />
                <px:PXCheckBox ID="edPrintPickList" runat="server" DataField="PrintPickList" CommitChanges="True"/>
                <px:PXCheckBox ID="edConfirmShipment" runat="server" DataField="ConfirmShipment" CommitChanges="True"/>
                <px:PXCheckBox ID="edPrintLabels" runat="server" DataField="PrintLabels" CommitChanges="True"/>
                <px:PXCheckBox ID="edPrintShipmentConfirmation" runat="server" DataField="PrintConfirmation" CommitChanges="True"/>
                <px:PXCheckBox ID="edUpdateIN" runat="server" DataField="UpdateIN" CommitChanges="True"/>
	            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Invoicing" StartColumn="True" SuppressLabel="True" ColumnWidth="260" />
	            <px:PXCheckBox ID="edPrepareInvoiceFromShipment" runat="server" DataField="PrepareInvoiceFromShipment" CommitChanges="True"/>
	            <px:PXCheckBox ID="edPrepareInvoice" runat="server" DataField="PrepareInvoice" CommitChanges="True"/>
	            <px:PXCheckBox ID="edPrintInvoice" runat="server" DataField="PrintInvoice" CommitChanges="True"/>
	            <px:PXCheckBox ID="edEmailInvoice" runat="server" DataField="EmailInvoice" CommitChanges="True"/>
	            <px:PXCheckBox ID="edReleaseInvoice" runat="server" DataField="ReleaseInvoice" CommitChanges="True"/>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel9" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXProcess" runat="server" DialogResult="OK" Text="OK" CommandName="QuickProcessOk" CommandSourceID="ds" SyncVisible="False"/>
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Inventory Lookup --%>
    <px:PXSmartPanel ID="PanelAddSiteStatus" runat="server" Key="sitestatus" LoadOnDemand="true" Width="1100px" Height="500px"
        Caption="Inventory Lookup" CaptionVisible="true" AutoRepaint="true" DesignView="Content" ShowAfterLoad="true">
        <px:PXFormView ID="formSitesStatus" runat="server" CaptionVisible="False" DataMember="sitestatusfilter" DataSourceID="ds"
            Width="100%" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXTextEdit ID="edInventory" runat="server" DataField="Inventory" IsClientControl="False"/>
                <px:PXTextEdit CommitChanges="True" ID="edBarCode" runat="server" DataField="BarCode" />
                <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="true" />
                <px:PXSegmentMask CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClass" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True" ID="edSubItem" runat="server" DataField="SubItem" AutoRefresh="true" />
                <px:PXGroupBox CommitChanges="True" RenderStyle="RoundBorder" ID="gpMode" runat="server" Caption="Selected Mode"
                    DataField="Mode" Width="300px">
                    <Template>
                        <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXRadioButton runat="server" ID="rModeSite" Value="0" Text="By Site State" />
                        <px:PXRadioButton runat="server" ID="rModeCustomer" Value="1" Text="By Last Sale" />
                    </Template>
                </px:PXGroupBox>
                <px:PXCheckBox CommitChanges="True" ID="chkOnlyAvailable" AlignLeft="true" runat="server" Checked="True" DataField="OnlyAvailable" />
				<px:PXCheckBox CommitChanges="True" ID="chkDropShipSales" AlignLeft="true" runat="server" Checked="True" DataField="DropShipSales" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edHistoryDate" runat="server" DataField="HistoryDate" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gripSiteStatus" runat="server" DataSourceID="ds" Style="height: 189px;"
            AutoAdjustColumns="true" Width="100%" SkinID="Details" AdjustPageSize="Auto" AllowSearch="True" FastFilterID="edInventory"
            FastFilterFields="InventoryCD,Descr,AlternateID" BatchUpdate="true">
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
                        <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true"
                            AllowCheckAll="true" />
                        <px:PXGridColumn AllowNull="False" DataField="QtySelected" TextAlign="Right" />
                        <px:PXGridColumn DataField="SiteID" />
                        <px:PXGridColumn DataField="SiteCD" 
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
                        <px:PXGridColumn DataField="ItemClassID" />
                        <px:PXGridColumn DataField="ItemClassDescription" />
                        <px:PXGridColumn DataField="PriceClassID" />
                        <px:PXGridColumn DataField="PriceClassDescription" />
                        <px:PXGridColumn DataField="PreferredVendorID" />
                        <px:PXGridColumn DataField="PreferredVendorDescription" />
                        <px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;AAAAAAAAAA" />
                        <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
						<px:PXGridColumn DataField="SubItemCD"
							AllowNull="False" SyncNullable ="false" 
							Visible="False" SyncVisible="false" 
							AllowShowHide ="False" SyncVisibility="false" />
                        <px:PXGridColumn DataField="Descr" />
                        <px:PXGridColumn DataField="SalesUnit" DisplayFormat="&gt;aaaaaa" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyAvailSale" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyOnHandSale" TextAlign="Right" />
						<px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                        <px:PXGridColumn AllowNull="False" DataField="QtyLastSale" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="CuryUnitPrice" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="LastSalesDate" TextAlign="Right" />
						<px:PXGridColumn AllowNull="False" DataField="DropShipLastQty" TextAlign="Right" />
						<px:PXGridColumn AllowNull="False" DataField="DropShipCuryUnitPrice" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="DropShipLastDate" TextAlign="Right" />
                        <px:PXGridColumn AllowNull="False" DataField="AlternateID" />
                        <px:PXGridColumn AllowNull="False" DataField="AlternateType" />
                        <px:PXGridColumn AllowNull="False" DataField="AlternateDescr" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton5" runat="server" CommandName="AddInvSelBySite" CommandSourceID="ds" Text="Add" SyncVisible="false" />
            <px:PXButton ID="PXButton4" runat="server" Text="Add & Close" DialogResult="OK" />
            <px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Copy To --%>
    <px:PXSmartPanel ID="panelCopyTo" runat="server" Caption="Copy To" CaptionVisible="true" LoadOnDemand="true" Key="copyparamfilter"
        AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyTo" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page">
        <div style="padding: 5px">
            <px:PXFormView ID="formCopyTo" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="copyparamfilter">
                <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType"
                        Text="SO" DataSourceID="ds" />
                    <px:PXMaskEdit CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server" DataField="RecalcUnitPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server" DataField="OverrideManualPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcDiscounts" runat="server" DataField="RecalcDiscounts" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" Style="margin-left: 25px" />
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton9" runat="server" DialogResult="OK" Text="OK" CommandName="CheckCopyParams" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Carrier Rates --%>
    <px:PXSmartPanel ID="PanelCarrierRates" Width="820" runat="server" Caption="Shop For Rates" CaptionVisible="True" LoadOnDemand="True" ShowAfterLoad="True" Key="DocumentProperties"
        AutoCallBack-Target="formCarrierRates" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="PXButtonRatesOK" AllowResize="False">
        <px:PXFormView ID="formCarrierRates" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="DocumentProperties"
            Caption="Services Settings" CaptionVisible="False" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXNumberEdit ID="edOrderWeight" runat="server" DataField="OrderWeight" Enabled="False" />
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="PackageWeight" Enabled="False" />

            </Template>
        </px:PXFormView>

        <px:PXGrid ID="gridRates" runat="server" Width="100%" DataSourceID="ds" Style="border-width: 1px 1px; left: 0px; top: 0px;"
            AutoAdjustColumns="true" Caption="Carrier Rates" Height="120px" AllowFilter="False" SkinID="Details" CaptionVisible="True" AllowPaging="False">
            <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" />
            <ActionBar Position="Top" PagerVisible="False" CustomItemsGroup="1" ActionsVisible="True">
                <CustomItems>
                    <px:PXToolBarButton Text="Get Rates">
                        <AutoCallBack Command="RefreshRates" Target="ds" />
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="CarrierRates">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Type="CheckBox" AutoCallBack="true" TextAlign="Center" />
                        <px:PXGridColumn DataField="Method" Label="Code" />
                        <px:PXGridColumn DataField="Description" Label="Description" />
                        <px:PXGridColumn AllowUpdate="False" DataField="Amount" />
                        <px:PXGridColumn AllowUpdate="False" DataField="DaysInTransit" Label="Days in Transit" />
                        <px:PXGridColumn AllowUpdate="False" DataField="DeliveryDate" Label="Delivery Date" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="DocumentProperties"
            Caption="Services Settings" CaptionVisible="False" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXCheckBox ID="edIsManualPackage" runat="server" DataField="IsManualPackage" AlignLeft="true" CommitChanges="true" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridPackages" runat="server" Width="100%" DataSourceID="ds" Style="border-width: 1px 1px; left: 0px; top: 0px;"
            Caption="Packages" SkinID="Details" Height="80px" CaptionVisible="True" AllowPaging="False">
            <ActionBar Position="TopAndBottom">
                <CustomItems>
                    <px:PXToolBarButton Text="Recalculate Packages">
                        <AutoCallBack Command="RecalculatePackages" Target="ds" />
                    </px:PXToolBarButton>
                </CustomItems>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="Packages">
                    <Columns>
                        <px:PXGridColumn DataField="BoxID" CommitChanges="True" />
                        <px:PXGridColumn DataField="Description" Label="Description" />
                        <px:PXGridColumn DataField="SiteID" />
                        <px:PXGridColumn DataField="WeightUOM" />
                        <px:PXGridColumn DataField="Weight" />
                        <px:PXGridColumn DataField="BoxWeight" />
                        <px:PXGridColumn DataField="GrossWeight" />
                        <px:PXGridColumn DataField="DeclaredValue" />
                        <px:PXGridColumn DataField="COD" Type="CheckBox" />
                        <px:PXGridColumn DataField="StampsAddOns" Type="DropDownList" />
                    </Columns>
                    <RowTemplate>
                        <px:PXDropDown runat="server" ID="edStampsAddOns" DataField="StampsAddOns" AllowMultiSelect="True" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel8" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonRatesOK" runat="server" DialogResult="OK" Text="OK" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <%-- Recalculate Prices and Discounts --%>
    <px:PXSmartPanel ID="PanelRecalcDiscounts" runat="server" Caption="Recalculate Prices" CaptionVisible="True" LoadOnDemand="True" Key="recalcdiscountsfilter" AutoCallBack-Target="formRecalcDiscounts" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page" TabIndex="5500">
            <px:PXFormView ID="formRecalcDiscounts" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="recalcdiscountsfilter">
                <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                    <px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" CommitChanges="true" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server" DataField="RecalcUnitPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server" DataField="OverrideManualPrices" Style="margin-left: 25px" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcDiscounts" runat="server" DataField="RecalcDiscounts" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" Style="margin-left: 25px" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" Style="margin-left: 25px" />
                </Template>
            </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton10" runat="server" DialogResult="OK" Text="OK" CommandName="RecalcOk" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="panelCreateServiceOrder" runat="server" Style="z-index: 108; left: 27px; position: absolute; top: 99px;" Caption="Create Service Order" Width="450px" Height="150px" AutoRepaint="true"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateServiceOrderFilter" AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXFormView ID="formCreateServiceOrder" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="CreateServiceOrderFilter">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SL" />
                <px:PXSelector ID="edSrvOrdType" runat="server" AllowNull="False" DataField="SrvOrdType" DisplayMode="Text" CommitChanges="True" AutoRefresh="True" AllowEdit="True" />
                <px:PXSelector ID="edAssignedEmpID" runat="server" AllowNull="False" DataField="AssignedEmpID" DisplayMode="Text" CommitChanges="True" AutoRefresh="True" AllowEdit="True"/>
                <px:PXLayoutRule runat="server" Merge="True">
                </px:PXLayoutRule>
                <px:PXDateTimeEdit ID="edSLAETA_Date" runat="server" DataField="SLAETA_Date">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edSLAETA_Time" runat="server" DataField="SLAETA_Time"
                    TimeMode="True" SuppressLabel = "True">
                </px:PXDateTimeEdit>
            </Template>
        </px:PXFormView>

        <div style="padding: 5px; text-align: right;">
                <px:PXButton ID="btnSave2" runat="server" CommitChanges="True" DialogResult="OK" Text="OK" Height="20"/>
                <px:PXButton ID="btnCancel2" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
        </div>
    </px:PXSmartPanel>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
	<!--#include file="~\Pages\Includes\InventoryMatrixEntrySmartPanel.inc"-->
</asp:Content>
