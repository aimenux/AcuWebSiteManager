<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR301000.aspx.cs" Inherits="Page_AR301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARInvoiceEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Action" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="ReverseInvoice" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="ReverseInvoiceAndApplyToMemo" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="WriteOff" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="PayInvoice" />
			<px:PXDSCallbackCommand Visible="False" Name="ReclassifyBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="CustomerRefund" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="CreateSchedule" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewSchedule" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewCorrectionDocument" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewRetainageDocument"/>
			<px:PXDSCallbackCommand Visible="False" Name="NewCustomer" />
			<px:PXDSCallbackCommand Visible="False" Name="SendARInvoiceMemo" />
			<px:PXDSCallbackCommand Visible="False" Name="EditCustomer" />
			<px:PXDSCallbackCommand Visible="False" Name="CustomerDocuments" />
			<px:PXDSCallbackCommand Visible="false" Name="AutoApply" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="LoadDocuments" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewItem" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ViewPayment" DependOnGrid="detgrid" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewInvoice" DependOnGrid="detgrid2" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="ViewInvoice2" DependOnGrid="detgrid3" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewVoucherBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewWorkBook" />
			<px:PXDSCallbackCommand Visible="False" Name="SOInvoice" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
			<px:PXDSCallbackCommand Visible="false" Name="ReleaseRetainage" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$PurchaseOrder$Link" Visible="false" DependOnGrid="grid" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="CreditHold" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="PutOnCreditHold" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ComplianceDocument$Subcontract$Link" Visible="false" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="Document" Caption="Invoice Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" LinkIndicator="True"
		NotifyIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edDocType" TabIndex="100" MarkRequired="Dynamic">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" SelectedIndex="-1" />
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True">
				<GridProperties FastFilterFields="ARInvoice__InvoiceNbr, CustomerID, CustomerID_Customer_AcctName" />
			</px:PXSelector>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="True"/>
			<px:PXTextEdit ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />

			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" AutoRefresh="True" DataField="CustomerLocationID" />
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_ARInvoice_CurrencyInfo_" DataMember="_Currency_"></pxa:PXCurrencyRate>
			<px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID"  AutoRefresh ="true" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" />
				<px:PXCheckBox runat="server" ID="chkIsRetainageDocument" DataField="IsRetainageDocument" AlignLeft="true" Enabled="false" />
				<px:PXCheckBox runat="server" ID="chkRetainageApply" DataField="RetainageApply" CommitChanges="true" AlignLeft="true" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXDateTimeEdit ID="edDiscDate" runat="server" DataField="DiscDate" />
				<px:PXCheckBox CommitChanges="True" ID="chkPaymentsByLinesAllowed" runat="server" DataField="PaymentsByLinesAllowed" />
			<px:PXLayoutRule runat="server" />
			
			<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot" CommitChanges = "True" />
			<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryInitDocBal" runat="server" DataField="CuryInitDocBal" CommitChanges="True" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryRoundDiff" runat="server" DataField="CuryRoundDiff" Enabled="False" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDiscAmt" runat="server" DataField="CuryOrigDiscAmt" />
			<px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRUTROT" AlignLeft="true" />
		</Template>
	</px:PXFormView>

	<style type="text/css">
		.leftDocTemplateCol {
			width: 50%;
			float: left;
			min-width: 90px;
		}

		.rightDocTemplateCol {
			min-width: 90px;
		}
	</style>

	<px:PXGrid ID="docsTemplate" runat="server" Visible="false">
		<Levels>
			<px:PXGridLevel>
				<Columns>
					<px:PXGridColumn Key="Template">
						<CellTemplate>
							<div id="ParentDiv1" class="leftDocTemplateCol">
								<div id="div11" class="Field0"><%# ((PXGridCellContainer)Container).Text("refNbr") %></div>
								<div id="div12" class="Field1"><%# ((PXGridCellContainer)Container).Text("docDate") %></div>
							</div>
							<div id="ParentDiv2" class="rightDocTemplateCol">
								<span id="span21" class="Field1"><%# ((PXGridCellContainer)Container).Text("curyOrigDocAmt") %></span>
								<span id="span22" class="Field1"><%# ((PXGridCellContainer)Container).Text("curyID") %></span>
								<div id="div21" class="Field1"><%# ((PXGridCellContainer)Container).Text("status") %></div>
							</div>
							<div id="div3" class="Field1"><%# ((PXGridCellContainer)Container).Text("customerID_Customer_acctName") %></div>
						</CellTemplate>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" TabIndex="200" DataMember="CurrentDocument">
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" NoteIndicator="True" FilesIndicator="True" Width="100%" SyncPosition="True" SkinID="DetailsInTab"
						TabIndex="300" Height="300px" FilesField="NoteFiles">
						<Levels>
							<px:PXGridLevel DataMember="Transactions">
								<Columns>
									<px:PXGridColumn DataField="BranchID" AutoCallBack="True" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
									<px:PXGridColumn DataField="InventoryID" AutoCallBack="True" LinkCommand="ViewItem"/>
									<px:PXGridColumn DataField="SubItemID" AutoCallBack="True" />
									<px:PXGridColumn DataField="SiteID" CommitChanges="true" />
									<px:PXGridColumn DataField="AppointmentDate" />
									<px:PXGridColumn DataField="AppointmentID" />
									<px:PXGridColumn DataField="SOID" />
									<px:PXGridColumn DataField="TranDesc" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="BaseQty" TextAlign="Right" />
									<px:PXGridColumn DataField="UOM" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="ManualPrice" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryExtPrice" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="TranCost" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscPct" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="ManualDisc" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscountID" RenderEditorText="True" TextAlign="Left" AutoCallBack="True" />
									<px:PXGridColumn DataField="DiscountSequenceID" TextAlign="Left" />
									<px:PXGridColumn DataField="RetainagePct" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryRetainageAmt" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryRetainageBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryRetainedTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryCashDiscBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTranBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryOrigTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID_Account_description" SyncVisibility="false" />
									<px:PXGridColumn DataField="SubID" />
									<px:PXGridColumn DataField="ExpenseAccrualAccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="ExpenseAccrualAccountID_Account_description" />
									<px:PXGridColumn DataField="ExpenseAccrualSubID" />
									<px:PXGridColumn DataField="ExpenseAccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="ExpenseAccountID_Account_description" />
									<px:PXGridColumn DataField="ExpenseSubID" />
									<px:PXGridColumn DataField="CostBasis" />
									<px:PXGridColumn DataField="CuryAccruedCost" />
									<px:PXGridColumn DataField="TaskID" CommitChanges="true" />
									<px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
									<px:PXGridColumn DataField="SalesPersonID" />
									<px:PXGridColumn DataField="DefScheduleID" TextAlign="Right" />
									<px:PXGridColumn DataField="DeferredCode" CommitChanges="true" />
									<px:PXGridColumn DataField="DRTermStartDate" CommitChanges="true" />
									<px:PXGridColumn DataField="DRTermEndDate" CommitChanges="true" />
									<px:PXGridColumn DataField="TaxCategoryID" />
									<px:PXGridColumn DataField="AvalaraCustomerUsageType" />
									<px:PXGridColumn DataField="Date" />
									<px:PXGridColumn DataField="Commissionable" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="IsRUTROTDeductible" Type="Checkbox" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="RUTROTItemType" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="RUTROTWorkTypeID" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryRUTROTAvailableAmt" />
									<px:PXGridColumn DataField="CaseCD" />
									<px:PXGridColumn DataField="CuryUnitPriceDR" AllowShowHide="Server" />
									<px:PXGridColumn DataField="DiscPctDR" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="HasExpiredComplianceDocuments" />
								</Columns>

								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AllowAddNew="True" AutoRefresh="True" />
									<px:PXSelector runat="server" ID="edAppointmentID" DataField="AppointmentID" AllowEdit="True" />
									<px:PXSelector runat="server" ID="edSOID" DataField="SOID" AllowEdit="True" />
									<px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" CommitChanges="True" />
									<px:PXSelector runat="server" ID="edSiteID" DataField="SiteID" AllowEdit="True" />
									<px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" />
									<px:PXNumberEdit CommitChanges="True" ID="edQty" runat="server" DataField="Qty" />
									<px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" CommitChanges="true" />
									<px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice" CommitChanges="true" />
									<px:PXNumberEdit ID="edCuryExtPrice" runat="server" DataField="CuryExtPrice" CommitChanges="true" />
									<px:PXNumberEdit ID="edTranCost" runat="server" DataField="TranCost" CommitChanges="true" />
									<px:PXSelector ID="edDiscountCode" runat="server" DataField="DiscountID" CommitChanges="True" AllowEdit="True" />
									<px:PXNumberEdit ID="edDiscPct" runat="server" DataField="DiscPct" />
									<px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" />
									<px:PXCheckBox ID="chkManualDisc" runat="server" DataField="ManualDisc" CommitChanges="true" />
									<px:PXNumberEdit ID="edRetainagePct" runat="server" DataField="RetainagePct" />
									<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" Enabled="False" />
									<px:PXNumberEdit ID="edCuryTranBal" runat="server" DataField="CuryTranBal" Enabled="False" />
									<px:PXNumberEdit ID="edCuryOrigTaxAmt" runat="server" DataField="CuryOrigTaxAmt" Enabled="False" />
									<px:PXLabel runat="server" />
									<px:PXLayoutRule runat="server" ColumnSpan="2" />
									<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />

									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
									<px:PXCheckBox CommitChanges="True" ID="chkCommissionable" runat="server" DataField="Commissionable" />
									<px:PXSelector ID="edDefScheduleID" runat="server" DataField="DefScheduleID" AutoRefresh="True" AllowEdit="true" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" CommitChanges="true" />
									<px:PXDateTimeEdit ID="edDRTermStartDate" runat="server" DataField="DRTermStartDate" CommitChanges="true" />
									<px:PXDateTimeEdit ID="edDRTermEndDate" runat="server" DataField="DRTermEndDate" CommitChanges="true" />
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
									<px:PXDropDown ID="edAvalaraCustomerUsageTypeID1" runat="server" DataField="AvalaraCustomerUsageType" />
									<px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edCostCode" runat="server" DataField="CostCodeID" AutoRefresh="True" AllowAddNew="true" />
									<px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRRDeductibleTran" />
									<px:PXDropDown runat="server" DataField="RUTROTItemType" CommitChanges="True" ID="cmbRRItemType" />
									<px:PXSelector runat="server" DataField="RUTROTWorkTypeID" CommitChanges="True" ID="cmbRRWorkType" AutoRefresh="true" />
									<px:PXNumberEdit runat="server" DataField="CuryRUTROTAvailableAmt" ID="edRRAvailable" />
									<px:PXSelector runat="server" DataField="CaseCD" ID="edCaseCD" AllowEdit="true" />
									
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edExpenseAccrualAccountID" runat="server" DataField="ExpenseAccrualAccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edExpenseAccrualSubID" runat="server" DataField="ExpenseAccrualSubID" AutoRefresh="True" />
									<px:PXSegmentMask CommitChanges="True" ID="edExpenseAccountID" runat="server" DataField="ExpenseAccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
									<px:PXDropDown runat="server" DataField="CostBasis" CommitChanges="True" ID="cmbCostBasis" />
									<px:PXNumberEdit ID="edCuryAccruedCost" runat="server" DataField="CuryAccruedCost" CommitChanges="true" />
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="View Schedule" Key="cmdViewSchedule">
									<AutoCallBack Command="ViewSchedule" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="View Item" Key="ViewItem">
									<AutoCallBack Command="ViewItem" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" StartColumn="True" GroupCaption="Link to GL" />
					<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
					<px:PXNumberEdit ID="edDisplayCuryInitDocBal" runat="server" DataField="DisplayCuryInitDocBal" Enabled="False" />
					<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXSegmentMask ID="edARAccountID" runat="server" DataField="ARAccountID" CommitChanges="True" />
					<px:PXSegmentMask ID="edARSubID" runat="server" DataField="ARSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edRetainageAcctID" runat="server" DataField="RetainageAcctID" DataSourceID="ds" CommitChanges="True" />
					<px:PXSegmentMask ID="edRetainageSubID" runat="server" DataField="RetainageSubID" DataSourceID="ds" />
					<px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True">
						<LinkCommand Target="ds" Command="ViewOriginalDocument" />
					</px:PXTextEdit>
					<px:PXTextEdit ID="edCorrectionRefNbr" runat="server" DataField="CorrectionRefNbr" Enabled="False" AllowEdit="True">
						<LinkCommand Target="ds" Command="ViewCorrectionDocument" />
					</px:PXTextEdit>
					<px:PXLabel runat="server" ID="space1" />

					<px:PXLayoutRule runat="server" GroupCaption="Default Payment Info" />
					<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" />
					<px:PXSelector CommitChanges="True" ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr"
						AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" AutoRefresh="True"/>
					<px:PXCheckBox ID="chk" runat="server" DataField="ApplyOverdueCharge" />

					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" GroupCaption="Tax Info" StartGroup="True" />
					<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
					<px:PXDropDown runat="server" ID="edTaxCalcMode" DataField="TaxCalcMode" CommitChanges="true" />
					<px:PXDropDown ID="edAvalaraCustomerUsageTypeID" runat="server" CommitChanges="True" DataField="AvalaraCustomerUsageType" />

					<px:PXLayoutRule runat="server" GroupCaption="Assigned To" StartGroup="True" />
					<px:PXTreeSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_"
						TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False">
						<DataBindings>
							<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
						</DataBindings>
					</px:PXTreeSelector>
					<px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" AutoRefresh="True" DataField="OwnerID" />

					<px:PXLayoutRule runat="server" GroupCaption="Print and Email Options" StartGroup="True" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkPrinted" runat="server" DataField="Printed" Enabled="False" Size="SM" AlignLeft="true" />
					<px:PXCheckBox ID="chkDontPrint" runat="server" DataField="DontPrint" CommitChanges="true" Size="SM" AlignLeft="true" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkEmailed" runat="server" DataField="Emailed" Enabled="False" Size="SM" AlignLeft="true" />
					<px:PXCheckBox ID="chkDontEmail" runat="server" DataField="DontEmail" CommitChanges="true" Size="SM" AlignLeft="true" />

					<px:PXLayoutRule runat="server" GroupCaption="Dunning Info" ControlSize="M" LabelsWidth="SM" />
					<px:PXFormView ID="DunningForm" runat="server" DataMember="dunningLetterDetail" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartGroup="True" ControlSize="M" LabelsWidth="SM" />
							<px:PXDateTimeEdit ID="edDunningDate" runat="server" Size="M" DataField="ARDunningLetter__DunningLetterDate" />
							<px:PXNumberEdit ID="edDunningLevel" runat="server" Size="M" DataField="DunningLetterLevel" />
						</Template>
					</px:PXFormView>
					<px:PXCheckBox ID="edRevoked" runat="server" DataField="Revoked" />

					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" GroupCaption="Cash Discount Info" StartGroup="True" />
					<px:PXNumberEdit runat="server" DataField="CuryDiscountedDocTotal" ID="edCuryDiscountedDocTotal" Enabled="false" />
					<px:PXNumberEdit runat="server" DataField="CuryDiscountedTaxableTotal" ID="edCuryDiscountedTaxableTotal" Enabled="false" />
					<px:PXNumberEdit runat="server" DataField="CuryDiscountedPrice" ID="edCuryDiscountedPrice" Enabled="false" />

					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" GroupCaption="Voucher Details" />
					<px:PXFormView ID="VoucherDetails" runat="server" RenderStyle="Simple"
						DataMember="Voucher" DataSourceID="ds" TabIndex="1100">
						<Template>
							<px:PXTextEdit ID="linkGLVoucherBatch" runat="server" DataField="VoucherBatchNbr" Enabled="false">
								<LinkCommand Target="ds" Command="ViewVoucherBatch"></LinkCommand>
							</px:PXTextEdit>
							<px:PXTextEdit ID="linkGLWorkBook" runat="server" DataField="WorkBookID" Enabled="false">
								<LinkCommand Target="ds" Command="ViewWorkBook"></LinkCommand>
							</px:PXTextEdit>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Address Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="Billing_Contact" runat="server" Caption="Bill-To Contact" DataMember="Billing_Contact" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommandSourceID="ds" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="Billing_Address" runat="server" Caption="Bill-To Address" DataMember="Billing_Address" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
							<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" />
							<px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="Shipping_Contact" runat="server" Caption="Ship-To Contact" DataMember="Shipping_Contact" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommandSourceID="ds" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="Shipping_Address" runat="server" Caption="Ship-To Address" DataMember="Shipping_Address" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
							<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" />
							<px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="500">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" CommitChanges="true" AutoRefresh="true"/>
									<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
									<px:PXNumberEdit ID="edTaxableAmt" runat="server" DataField="TaxableAmt" />
									<px:PXNumberEdit ID="edTaxAmt" runat="server" DataField="TaxAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" CommitChanges="true"/>
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryExemptedAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxUOM" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxableQty" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryRetainedTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryRetainedTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="Tax__TaxType" />
									<px:PXGridColumn DataField="Tax__PendingTax" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Tax__ReverseTax" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Tax__ExemptTax" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Tax__StatisticalTax" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="CuryDiscountedTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscountedPrice" TextAlign="Right" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Salesperson Commission" RepaintOnDemand="false">
				<Template>
					<px:PXFormView ID="Commission" runat="server" DataMember="CurrentDocument" RenderStyle="Simple" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXSegmentMask CommitChanges="True" ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" Enabled="False" />
							<px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" Enabled="False" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridSalesPerTran" runat="server" Width="100%" SkinID="DetailsInTab" TabIndex="600">
						<Levels>
							<px:PXGridLevel DataMember="SalesPerTrans" DataKeyNames="DocType,RefNbr,SalespersonID,AdjNbr,AdjdDocType,AdjdRefNbr">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSegmentMask CommitChanges="True" ID="edSalespersonID" runat="server" DataField="SalespersonID" Enabled="False" />
									<px:PXNumberEdit ID="edCommnPct" runat="server" DataField="CommnPct" />
									<px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" Enabled="False" />
									<px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SalespersonID" AutoCallBack="True" />
									<px:PXGridColumn DataField="CommnPct" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryCommnAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryCommnblAmt" TextAlign="Right" />
									<px:PXGridColumn AllowShowHide="False" DataField="AdjdDocType" Visible="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Approval Details" BindingContext="form" LoadOnDemand="false">
				<Template>
					<px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" Style="left: 0; top: 0;">
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
									<px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false"/>
									<px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Discount Details" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="ARDiscountDetails" DataKeyNames="DiscountID,DiscountSequenceID,Type">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkSkipDiscount" runat="server" DataField="SkipDiscount" />
									<px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID"
										AllowEdit="True" edit="1" />
									<px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
									<px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
									<px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" AutoRefresh="true" AllowEdit="true" />
									<px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
									<px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
									<px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" CommitChanges="true" />
									<px:PXNumberEdit ID="edCuryRetainedDiscountAmt" runat="server" DataField="CuryRetainedDiscountAmt" />
									<px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" CommitChanges="true" />
									<px:PXSegmentMask ID="edFreeItemID" runat="server" DataField="FreeItemID" AllowEdit="True" />
									<px:PXNumberEdit ID="edFreeItemQty" runat="server" DataField="FreeItemQty" />
									<px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" Enabled="False" AllowEdit="True" />
									<px:PXTextEdit ID="edExtDiscCode" runat="server" DataField="ExtDiscCode" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SkipDiscount" Type="CheckBox" TextAlign="Center" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscountID" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscountSequenceID" CommitChanges="true" />
									<px:PXGridColumn DataField="Type" RenderEditorText="True" />
									<px:PXGridColumn DataField="IsManual" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryRetainedDiscountAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="DiscountPct" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="FreeItemID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
									<px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" />
									<px:PXGridColumn DataField="OrderType" />
									<px:PXGridColumn DataField="OrderNbr" />
									<px:PXGridColumn DataField="ExtDiscCode"/>
									<px:PXGridColumn DataField="Description"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Retainage" RepaintOnDemand="false" VisibleExp="DataControls[&quot;chkRetainageApply&quot;].Value = 1" BindingContext="form">
				<Template>
					<px:PXFormView ID="formRetainage" runat="server" Style="z-index: 100" Width="100%" DataMember="CurrentDocument" SkinID="Transparent" DataSourceID="ds" >
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edDefRetainagePct" DataField="DefRetainagePct" />
							<px:PXNumberEdit runat="server" ID="edCuryOrigDocAmtWithRetainageTotal" DataField="CuryOrigDocAmtWithRetainageTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainageTotal" DataField="CuryRetainageTotal" Enabled="false" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainageUnreleasedAmt" DataField="CuryRetainageUnreleasedAmt" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainageReleasedAmt" DataField="CuryRetainageReleased" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainageUnpaidTotal" DataField="CuryRetainageUnpaidTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainagePaidTotal" DataField="CuryRetainagePaidTotal" Enabled="false" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainedTaxTotal" DataField="CuryRetainedTaxTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainedDiscTotal" DataField="CuryRetainedDiscTotal" Enabled="false" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="detgrid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SkinID="DetailsInTab" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="RetainageDocuments">
								<Columns>
									<px:PXGridColumn DataField="DocType" Type="DropDownList" />
									<px:PXGridColumn DataField="RefNbr" AutoCallBack="True" LinkCommand="ViewRetainageDocument" />
									<px:PXGridColumn DataField="DocDate" />
									<px:PXGridColumn DataField="FinPeriodID" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="ARInvoice__PaymentMethodID" />
									<px:PXGridColumn DataField="ARInvoice__InvoiceNbr" />
									<px:PXGridColumn DataField="DocDesc" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Applications" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="detgrid" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="700" FilesIndicator="True"
						NoteIndicator="True" SyncPosition="True" AllowPaging="true" PageSize="100" >
						<Levels>
							<px:PXGridLevel DataMember="Adjustments">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSelector ID="edAdjgBranchID" runat="server" DataField="AdjgBranchID" Enabled="False" />
									<px:PXDropDown ID="edAdjgDocType" runat="server" DataField="AdjgDocType" Enabled="False" />
									<px:PXSelector CommitChanges="True" ID="edAdjgRefNbr" runat="server" DataField="AdjgRefNbr" Enabled="False">
										<Parameters>
											<px:PXControlParam ControlID="form" Name="ARInvoice.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
											<px:PXControlParam ControlID="detgrid" Name="ARAdjust.adjgDocType" PropertyName="DataValues[&quot;AdjgDocType&quot;]" />
										</Parameters>
									</px:PXSelector>
									<px:PXNumberEdit CommitChanges="True" ID="edCuryAdjdAmt" runat="server" DataField="CuryAdjdAmt" />
									<px:PXDateTimeEdit ID="edARPayment__DocDate" runat="server" DataField="ARPayment__DocDate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
									<px:PXSelector ID="edARPayment__CuryID" runat="server" DataField="ARPayment__CuryID" />
									<px:PXSelector ID="edARPayment__FinPeriodID" runat="server" DataField="ARPayment__FinPeriodID" Enabled="False" />
									<px:PXTextEdit ID="edARPayment__ExtRefNbr" runat="server" DataField="ARPayment__ExtRefNbr" />
									<px:PXDropDown ID="edARPayment__Status" runat="server" DataField="ARPayment__Status" Enabled="False" />
									<px:PXTextEdit ID="edARPayment__DocDesc" runat="server" DataField="ARPayment__DocDesc" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="True" AllowFilter="True" AllowMove="False" AutoCallBack="True" />
									<px:PXGridColumn DataField="AdjgBranchID" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjgDocType" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjgRefNbr" AutoCallBack="True" LinkCommand="ViewPayment" />
									<px:PXGridColumn DataField="CustomerID" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryAdjdAmt" AutoCallBack="True" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryAdjdPPDAmt" AutoCallBack="True" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryAdjdWOAmt" AutoCallBack="True" TextAlign="Right" />
									<px:PXGridColumn DataField="WriteOffReasonCode" TextAlign="Right" />
									<px:PXGridColumn DataField="ARPayment__DocDate" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="ARPayment__DocDesc" />
									<px:PXGridColumn DataField="ARPayment__CuryID" />
									<px:PXGridColumn DataField="ARPayment__FinPeriodID" />
									<px:PXGridColumn DataField="ARPayment__ExtRefNbr" />
									<px:PXGridColumn DataField="AdjdDocType" />
									<px:PXGridColumn DataField="AdjdRefNbr" />
									<px:PXGridColumn DataField="ARPayment__Status" Label="Status" Type="DropDownList" />
									<px:PXGridColumn DataField="PendingPPD" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="PPDCrMemoRefNbr" LinkCommand="ViewPPDCrMemo" />
                                    <px:PXGridColumn DataField="HasExpiredComplianceDocuments" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar DefaultAction="ViewPayment">
							<CustomItems>
								<px:PXToolBarButton Text="Load Documents">
									<AutoCallBack Command="LoadDocuments" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Auto Apply">
									<AutoCallBack Command="AutoApply" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode AllowFormEdit="False" AllowAddNew="False" />
					</px:PXGrid>
					<px:PXGrid ID="detgrid2" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="700" FilesIndicator="True"
						NoteIndicator="True" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="Adjustments_1">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXDropDown ID="edAdjdDocType3" runat="server" DataField="AdjgDocType" Enabled="False" />
									<px:PXSelector CommitChanges="True" ID="edAdjdRefNbr3" runat="server" DataField="AdjdRefNbr" AutoRefresh="true">
										<Parameters>
											<px:PXControlParam ControlID="form" Name="ARInvoice.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
											<px:PXControlParam ControlID="detgrid2" Name="ARAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" />
										</Parameters>
									</px:PXSelector>
									<px:PXNumberEdit CommitChanges="True" ID="edCuryAdjgAmt3" runat="server" DataField="CuryAdjgAmt" />
									<px:PXDateTimeEdit ID="edARInvoice__DocDate3" runat="server" DataField="ARInvoice__DocDate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryDocBal3" runat="server" DataField="CuryDocBal" Enabled="False" />
									<px:PXSelector ID="edARInvoice__CuryID3" runat="server" DataField="ARInvoice__CuryID" />
									<px:PXSelector ID="edARInvoice__FinPeriodID3" runat="server" DataField="ARInvoice__FinPeriodID" Enabled="False" />
									<px:PXTextEdit ID="edARInvoice__InvoiceNbr3" runat="server" DataField="ARInvoice__InvoiceNbr" />
									<px:PXDropDown ID="edARInvoice__Status3" runat="server" DataField="ARInvoice__Status" Enabled="False" />
									<px:PXTextEdit ID="edARInvoice__DocDesc3" runat="server" DataField="ARInvoice__DocDesc" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjgBranchID" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjdDocType" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjdRefNbr" AutoCallBack="True" LinkCommand="ViewInvoice" />
									<px:PXGridColumn DataField="AdjdCustomerID" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryAdjgAmt" AutoCallBack="True" TextAlign="Right" />
									<px:PXGridColumn DataField="ARInvoice__DocDate" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="ARInvoice__DocDesc" />
									<px:PXGridColumn DataField="ARInvoice__CuryID" />
									<px:PXGridColumn DataField="ARInvoice__FinPeriodID" />
									<px:PXGridColumn DataField="ARInvoice__InvoiceNbr" />
									<px:PXGridColumn DataField="ARInvoice__Status" Label="Status" Type="DropDownList" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode AllowFormEdit="False" />
					</px:PXGrid>
					<px:PXGrid ID="detgrid3" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="700" FilesIndicator="True"
						NoteIndicator="True" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="Adjustments_2">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXDropDown ID="edAdjdDocType2" runat="server" DataField="DisplayDocType" Enabled="False" />
									<px:PXSelector CommitChanges="True" ID="edAdjdRefNbr2" runat="server" DataField="DisplayRefNbr" AutoRefresh="true">
										<Parameters>
											<px:PXControlParam ControlID="form" Name="ARInvoice.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
											<px:PXControlParam ControlID="detgrid3" Name="ARAdjust.adjdDocType" PropertyName="DataValues[&quot;DisplayDocType&quot;]" />
										</Parameters>
									</px:PXSelector>
									<px:PXNumberEdit CommitChanges="True" ID="edCuryAdjgAmt2" runat="server" DataField="DisplayCuryAmt" />
									<px:PXDateTimeEdit ID="edARInvoice__DocDate" runat="server" DataField="DisplayDocDate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryDocBal2" runat="server" DataField="CuryDocBal" Enabled="False" />
									<px:PXSelector ID="edARInvoice__CuryID" runat="server" DataField="DisplayCuryID" />
									<px:PXSelector ID="edARInvoice__FinPeriodID" runat="server" DataField="DisplayFinPeriodID" Enabled="False" />
									<px:PXTextEdit ID="edARInvoice__InvoiceNbr" runat="server" DataField="ARInvoice__InvoiceNbr" />
									<px:PXDropDown ID="edARInvoice__Status" runat="server" DataField="DisplayStatus" Enabled="False" />
									<px:PXTextEdit ID="edARInvoice__DocDesc" runat="server" DataField="DisplayDocDesc" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjgBranchID" RenderEditorText="True" />
									<px:PXGridColumn DataField="DisplayDocType" RenderEditorText="True" />
									<px:PXGridColumn DataField="DisplayRefNbr" AutoCallBack="True" LinkCommand="ViewInvoice2" />
									<px:PXGridColumn DataField="DisplayCustomerID" TextAlign="Right" />
									<px:PXGridColumn DataField="DisplayCuryAmt" AutoCallBack="True" TextAlign="Right" />
									<px:PXGridColumn DataField="DisplayDocDate" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="DisplayDocDesc" />
									<px:PXGridColumn DataField="DisplayCuryID" />
									<px:PXGridColumn DataField="DisplayFinPeriodID" />
									<px:PXGridColumn DataField="ARInvoice__InvoiceNbr" />
									<px:PXGridColumn DataField="DisplayStatus" Label="Status" Type="DropDownList" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar DefaultAction="ViewInvoice2"  />
						<Mode AllowFormEdit="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="ROT/RUT Details" VisibleExp="DataControls[&quot;chkRUTROT&quot;].Value == 1" BindingContext="form">
				<Template>
					<px:PXFormView runat="server" SkinID="Transparent" ID="RUTROTForm" DataSourceID="ds" DataMember="Rutrots" MarkRequired="Dynamic">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" GroupCaption="RUT and ROT Settings" />
							<px:PXCheckBox runat="server" DataField="AutoDistribution" CommitChanges="True" ID="chkRRAutoDistribution" AlignLeft="true" />
							<px:PXGroupBox runat="server" DataField="RUTROTType" CommitChanges="True" RenderStyle="Simple" ID="gbRRType">
								<ContentLayout Layout="Stack" Orientation="Vertical" />
								<Template>
									<px:PXRadioButton runat="server" Value="U" ID="gbRRType_opU" GroupName="gbRRType" Text="RUT" />
									<px:PXRadioButton runat="server" Value="O" ID="gbRRType_opO" GroupName="gbRRType" Text="ROT" />
									<px:PXTextEdit runat="server" DataField="ROTAppartment" ID="edRAppartment" />
									<px:PXTextEdit runat="server" DataField="ROTEstate" ID="edRREstate" />
									<px:PXTextEdit runat="server" DataField="ROTOrganizationNbr" ID="edRROrganizationNbr" CommitChanges="true" />
								</Template>
							</px:PXGroupBox>
							<px:PXLayoutRule runat="server" GroupCaption="RUT and ROT Distribution" />
							<px:PXGrid runat="server" DataSourceID="ds" Width="350px" AllowFilter="false" AllowSearch="false" Height="200px" SkinID="ShortList" ID="gridDistribution"
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
							<px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="L" GroupCaption="RUT and ROT Balancing Documents" />
							<px:PXTextEdit ID="edBalancingCreditMemoRefNbr" runat="server" DataField="BalancingCreditMemoRefNbr" Enabled="False" AllowEdit="True">
								<LinkCommand Target="ds" Command="viewCreditMemo" />
							</px:PXTextEdit>
							<px:PXTextEdit ID="edBalancingDebitMemoRefNbr" runat="server" DataField="BalancingDebitMemoRefNbr" Enabled="False" AllowEdit="True">
								<LinkCommand Target="ds" Command="viewDebitMemo" />
							</px:PXTextEdit>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" ID="grdComplianceDocuments" Height="300px" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" AutoGenerateColumns="Append" KeepPosition="True" SyncPosition="True" AllowPaging="True" PageSize="12">
                        <Levels>
                            <px:PXGridLevel DataMember="ComplianceDocuments">
                                <Columns>
                                    <px:PXGridColumn DataField="ExpirationDate" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CreationDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Required" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Received" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="ReceivedDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="IsProcessed" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsVoided" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsCreatedAutomatically" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="SentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True" LinkCommand="ComplianceDocuments_Project_ViewDetails" />
                                    <px:PXGridColumn DataField="CostTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RevenueTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CostCodeID" TextAlign="Left" LinkCommand="ComplianceDocuments_CostCode_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CustomerID" LinkCommand="ComplianceDocuments_Customer_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CustomerName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="VendorID" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="VendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="BillID" LinkCommand="ComplianceDocument$BillID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ApCheckID" LinkCommand="ComplianceDocument$ApCheckID$Link" DisplayMode="Text" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentID" LinkCommand="ComplianceDocument$ArPaymentID$Link" DisplayMode="Text" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn DataField="InvoiceID" LinkCommand="ComplianceDocument$InvoiceID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" CommitChanges="True" />
                                    <px:PXGridColumn DataField="IsExpired" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsRequiredJointCheck" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="JointRelease" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointReleaseReceived" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointVendorInternalId" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="LastModifiedByID" />
                                    <px:PXGridColumn DataField="LienWaiverAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Limit" TextAlign="Right" />
                                    <px:PXGridColumn DataField="MethodSent" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentMethodID" />
                                    <px:PXGridColumn DataField="ApPaymentMethodID" />
                                    <px:PXGridColumn DataField="Policy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectTransactionID" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" CommitChanges="True" DisplayMode="Text" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PurchaseOrder" TextAlign="Left" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ReceiptDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceivedBy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SecondaryVendorID" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SecondaryVendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SourceType" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SponsorOrganization" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask runat="server" DataField="CostCodeID" AutoRefresh="True" ID="edCostCode2" />
                                    <px:PXSelector runat="server" DataField="DocumentTypeValue" AutoRefresh="True" ID="edDocumentTypeValue" />
                                    <px:PXSelector runat="server" DataField="BillID" FilterByAllFields="True" AutoRefresh="True" ID="edBillID" />
                                    <px:PXSelector runat="server" DataField="InvoiceID" FilterByAllFields="True" AutoRefresh="True" ID="edInvoiceID" />
                                    <px:PXSelector runat="server" DataField="ApCheckID" FilterByAllFields="True" AutoRefresh="True" ID="edApCheckID" />
                                    <px:PXSelector runat="server" DataField="ArPaymentID" FilterByAllFields="True" AutoRefresh="True" ID="edArPaymentID" />
                                    <px:PXSelector runat="server" DataField="ProjectTransactionID" FilterByAllFields="True" AutoRefresh="True" ID="edProjectTransactionID" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrder" FilterByAllFields="True" AutoRefresh="True" ID="edPurchaseOrder" CommitChanges="True" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrderLineItem" AutoRefresh="True" ID="edPurchaseOrderLineItem" />
                                    <px:PXSelector runat="server" DataField="Subcontract" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edSubcontract" />
                                    <px:PXSelector runat="server" DataField="SubcontractLineItem" AutoRefresh="True" ID="edSubcontractLineItem" />
                                    <px:PXSelector runat="server" DataField="ChangeOrderNumber" AutoRefresh="True" ID="edChangeOrderNumber" />
                                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                                    <px:PXSelector runat="server" ID="edProjectID" DataField="ProjectID" FilterByAllFields="True" AutoRefresh="True" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" />
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
	<px:PXSmartPanel ID="spRetainageOptions" runat="server" Style="z-index: 108;" Caption="Release Retainage" CaptionVisible="True" Key="ReleaseRetainageOptions" AutoReload="true" LoadOnDemand="true">
		<px:PXFormView ID="frmRetainageOptions" runat="server" Style="z-index: 100;" DataMember="ReleaseRetainageOptions" CaptionVisible="False" SkinID="Transparent">
			<ContentStyle BorderWidth="0px">
			</ContentStyle>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
				<px:PXNumberEdit CommitChanges="True" ID="edRetainagePct" runat="server" AllowNull="False" DataField="RetainagePct" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryRetainageAmt" runat="server" AllowNull="False" DataField="CuryRetainageAmt" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryRetainageUnreleasedAmt" runat="server" AllowNull="False" DataField="CuryRetainageUnreleasedAmt" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel9" runat="server" SkinID="Buttons">
					<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
					<px:PXButton ID="btnRelease" runat="server" DialogResult="OK" Text="Release" />
				</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="panelDuplicate" runat="server" Caption="Duplicate Reference Nbr." CaptionVisible="true" LoadOnDemand="true" Key="duplicatefilter"
		AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
		<div style="padding: 5px">
			<px:PXFormView ID="formCopyTo" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="duplicatefilter">
				<ContentStyle BackColor="Transparent" BorderStyle="None" />
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
					<px:PXLabel Size="xl" ID="lblMessage" runat="server">Record already exists. Please enter new Reference Nbr.</px:PXLabel>
					<px:PXMaskEdit CommitChanges="True" ID="edRefNbr" runat="server" DataField="RefNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
				</Template>
			</px:PXFormView>
		</div>
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton9" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds" />
			<px:PXButton ID="PXButton1" runat="server" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>

	<%-- Recalculate Prices and Discounts --%>
	<px:PXSmartPanel ID="PanelRecalcDiscounts" runat="server" Caption="Recalculate Prices" CaptionVisible="true" LoadOnDemand="true" Key="recalcdiscountsfilter"
		AutoCallBack-Enabled="true" AutoCallBack-Target="formRecalcDiscounts" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
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
		<px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton10" runat="server" DialogResult="OK" Text="OK" CommandName="RecalcOk" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
