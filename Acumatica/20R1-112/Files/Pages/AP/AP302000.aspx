<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP302000.aspx.cs" Inherits="Page_AP302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APPaymentEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="VoidCheck" CommitChanges="True" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="NewVendor" />
			<px:PXDSCallbackCommand Visible="False" Name="EditVendor" />
			<px:PXDSCallbackCommand Visible="False" Name="VendorDocuments" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument"/>
			<px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewVoucherBatch" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewPPDDebitAdj" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewWorkBook" />
			<px:PXDSCallbackCommand Visible="False" CommitChanges="True" Name="LoadInvoices" />
			<px:PXDSCallbackCommand Visible="False" CommitChanges="True" Name="LoadPOOrders" />
			<px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="ReverseApplication" DependOnGrid="detgrid2" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewApplicationDocument" DependOnGrid="detgrid2" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewDocumentToApply" DependOnGrid="detgrid" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewCurrentBatch" DependOnGrid="detgrid2" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="False" Name="PrintCheck" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ComplianceDocument$PurchaseOrder$Link" Visible="false" DependOnGrid="grid" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ComplianceDocument$Subcontract$Link" Visible="false" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Payment Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
		LinkIndicator="True" NotifyIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edDocType" TabIndex="26100" MarkRequired="Dynamic">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True">
				<GridProperties FastFilterFields="APPayment__ExtRefNbr, VendorID, VendorID_Vendor_acctName" />
			</px:PXSelector>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edAdjDate" runat="server" DataField="AdjDate" />
			<px:PXSelector CommitChanges="True" ID="edAdjFinPeriodID" runat="server" DataField="AdjFinPeriodID" AutoRefresh="True"/>
			<px:PXTextEdit CommitChanges="True" ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowAddNew="True" AllowEdit="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
			<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" AutoRefresh="True" />
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_APPayment_CurrencyInfo_" DataMember="_Currency_" />
			<px:PXDateTimeEdit ID="edDepositAfter" runat="server" DataField="DepositAfter" />
            <px:PXCheckBox runat="server" ID="chkIsJointCheck" DataField="IsJointCheck" CommitChanges="True" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
			<px:PXNumberEdit ID="edCuryPOApplAmt" runat="server" DataField="CuryPOApplAmt" Enabled="False" />
			<px:PXNumberEdit ID="edCuryUnappliedBal" runat="server" DataField="CuryUnappliedBal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryInitDocBal" runat="server" DataField="CuryInitDocBal" CommitChanges="True" />
			<px:PXNumberEdit ID="edCuryApplAmt" runat="server" DataField="CuryApplAmt" Enabled="False" />
			<px:PXNumberEdit ID="edCuryChargeAmt" runat="server" DataField="CuryChargeAmt" Enabled="False" />
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="M" />
			<px:PXNumberEdit runat="server" ID="edVendorPaymentAmount" DataField="VendorPaymentAmount" />
			<px:PXNumberEdit runat="server" ID="edJointPaymentAmount" DataField="JointPaymentAmount" />
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
							<div id="div3" class="Field1"><%# ((PXGridCellContainer)Container).Text("vendorID_Vendor_acctName") %></div>
						</CellTemplate>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" DataMember="CurrentDocument">
		<Items>
			<px:PXTabItem Text="Documents to Apply" RepaintOnDemand="false" >
				<Template>
					<px:PXGrid ID="detgrid" runat="server" Height="300px" SkinID="DetailsInTab" Style="z-index: 100;" TabIndex="30500" Width="100%" 
                               PageSize="100" AllowPaging="True">
						<Levels>
							<px:PXGridLevel DataMember="Adjustments">
								<RowTemplate>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
									<px:PXDropDown ID="edAdjdDocType" runat="server" DataField="AdjdDocType" CommitChanges="True" />
									<px:PXSelector ID="edAdjdRefNbr" runat="server" AutoRefresh="True" CommitChanges="True" DataField="AdjdRefNbr">
										<Parameters>
											<px:PXControlParam ControlID="form" Name="APPayment.vendorID" PropertyName="DataControls[&quot;edVendorID&quot;].Value" />
											<px:PXControlParam ControlID="detgrid" Name="APAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" />
										</Parameters>
									</px:PXSelector>
									<px:PXSelector ID="edAdjdLineNbr" runat="server" AutoRefresh="True" CommitChanges="True" DataField="AdjdLineNbr" >
										<Parameters>
											<px:PXControlParam ControlID="detgrid" Name="APAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" Type="String" />
											<px:PXControlParam ControlID="detgrid" Name="APAdjust.adjdRefNbr" PropertyName="DataValues[&quot;AdjdRefNbr&quot;]" Type="String" DefaultValue=" " />
										</Parameters>
									</px:PXSelector>
									<px:PXNumberEdit ID="edCuryAdjgAmt" runat="server" CommitChanges="True" DataField="CuryAdjgAmt" />
									<px:PXNumberEdit ID="edCuryAdjgPPDAmt" runat="server" AllowNull="False" DataField="CuryAdjgPPDAmt" />
									<px:PXNumberEdit ID="edCuryAdjgWhTaxAmt" runat="server" CommitChanges="True" DataField="CuryAdjgWhTaxAmt" />
									<px:PXDateTimeEdit ID="edAdjdDocDate" runat="server" DataField="AdjdDocDate" Enabled="False" />
									<px:PXDateTimeEdit ID="edAPInvoice__DueDate" runat="server" DataField="APInvoice__DueDate" />
									<px:PXDateTimeEdit ID="edAPInvoice__DiscDate" runat="server" DataField="APInvoice__DiscDate" />
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
									<px:PXNumberEdit ID="edAdjdCuryRate" runat="server" CommitChanges="True" DataField="AdjdCuryRate" />
									<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
									<px:PXNumberEdit ID="edCuryDiscBal" runat="server" DataField="CuryDiscBal" Enabled="False" />
									<px:PXNumberEdit ID="edCuryWhTaxBal" runat="server" DataField="CuryWhTaxBal" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjdBranchID" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjdDocType" Type="DropDownList" AutoCallBack="True" />
									<px:PXGridColumn AutoCallBack="True" DataField="AdjdRefNbr" LinkCommand="ViewDocumentToApply" />
									<px:PXGridColumn AutoCallBack="True" DataField="AdjdLineNbr" />

									<px:PXGridColumn DataField="APTran__InventoryID" />
									<px:PXGridColumn DataField="APTran__ProjectID" />
									<px:PXGridColumn DataField="APTran__TaskID" />
									<px:PXGridColumn DataField="APTran__CostCodeID" />
									<px:PXGridColumn DataField="APTran__AccountID" />

									<px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgAmt" TextAlign="Right" />
									<px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgPPDAmt" TextAlign="Right" />
									<px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgWhTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="AdjdDocDate" />
									<px:PXGridColumn DataField="APInvoice__DueDate" />
									<px:PXGridColumn DataField="APInvoice__DiscDate" />
									<px:PXGridColumn AutoCallBack="True" DataField="AdjdCuryRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryWhTaxBal" TextAlign="Right" />
									<px:PXGridColumn DataField="APInvoice__DocDesc" />
									<px:PXGridColumn DataField="AdjdCuryID" />
									<px:PXGridColumn DataField="AdjdFinPeriodID" />
									<px:PXGridColumn DataField="APInvoice__InvoiceNbr" />
									<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" DisplayMode="Hint"/>
                                    <px:PXGridColumn DataField="HasExpiredComplianceDocuments" Type="CheckBox" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Load Documents">
									<AutoCallBack Command="LoadInvoices" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Application History" RepaintOnDemand="false" >
				<Template>
					<px:PXGrid ID="detgrid2" runat="server" Height="300px" Style="z-index: 100" Width="100%" SkinID="DetailsInTab"
                               AdjustPageSize="Auto" AllowPaging="True">
						<Levels>
							<px:PXGridLevel DataMember="Adjustments_History" DataKeyNames="AdjgDocType,AdjgRefNbr,AdjdDocType,AdjdRefNbr,AdjdLineNbr,AdjNbr">
								<Mode AllowAddNew="False" AllowDelete="False" />
								<Columns>
									<px:PXGridColumn DataField="AdjdBranchID" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjBatchNbr" LinkCommand="ViewCurrentBatch" />
									<px:PXGridColumn DataField="DisplayDocType" Type="DropDownList" />
									<px:PXGridColumn DataField="DisplayRefNbr" LinkCommand="ViewApplicationDocument" />
									<px:PXGridColumn DataField="AdjdLineNbr" />

								    <px:PXGridColumn DataField="APTran__InventoryID" />
								    <px:PXGridColumn DataField="APTran__ProjectID" />
								    <px:PXGridColumn DataField="APTran__TaskID" />
								    <px:PXGridColumn DataField="APTran__CostCodeID" />
								    <px:PXGridColumn DataField="APTran__AccountID" />

									<px:PXGridColumn DataField="DisplayCuryAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="DisplayCuryPPDAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="DisplayCuryWhTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="AdjgFinPeriodID" />
									<px:PXGridColumn DataField="AdjdDocDate" />
									<px:PXGridColumn DataField="APInvoice__DueDate" />
									<px:PXGridColumn DataField="APInvoice__DiscDate" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" />
									<px:PXGridColumn DataField="DisplayDocDesc" />
									<px:PXGridColumn DataField="DisplayCuryID" />
									<px:PXGridColumn DataField="AdjdFinPeriodID" />
									<px:PXGridColumn DataField="APInvoice__InvoiceNbr" />
									<px:PXGridColumn DataField="TaxInvoiceNbr" />
									<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" DisplayMode="Hint"/>
									<px:PXGridColumn DataField="PendingPPD" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="PPDDebitAdjRefNbr" LinkCommand="ViewPPDDebitAdj"  />
                                    <px:PXGridColumn DataField="HasExpiredComplianceDocuments" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<Search Enabled="False" />
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Reverse Application">
									<AutoCallBack Command="ReverseApplication" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="GL Link" StartGroup="True" />
						<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
						<px:PXNumberEdit ID="edDisplayCuryInitDocBal" runat="server" DataField="DisplayCuryInitDocBal" Enabled="False"/>
						<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
						<px:PXSegmentMask ID="edAPAccountID" runat="server" DataField="APAccountID" CommitChanges="True" />
						<px:PXSegmentMask ID="edAPSubID" runat="server" DataField="APSubID" AutoRefresh="True" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
						<px:PXSelector CommitChanges="True" Size="S" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
						<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkCleared" runat="server" DataField="Cleared" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edClearDate" runat="server" DataField="ClearDate" />
						<px:PXCheckBox CommitChanges="True" ID="chkDepositAsBatch" runat="server" DataField="DepositAsBatch" />
						<px:PXCheckBox ID="chkDeposited" runat="server" DataField="Deposited" />
						<px:PXDateTimeEdit ID="edDepositDate" runat="server" DataField="DepositDate" Enabled="False" />
						<px:PXTextEdit ID="edDepositNbr" runat="server" DataField="DepositNbr" Enabled="False" />
						<px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True">
							<LinkCommand Target="ds" Command="ViewOriginalDocument"/>
						</px:PXTextEdit>

					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" GroupCaption="Voucher Details" />
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
			<px:PXTabItem Text="Orders to Apply" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="detgrid4" runat="server" Height="300px" SkinID="DetailsInTab" Style="z-index: 100;" TabIndex="30500" Width="100%" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="POAdjustments">
								<RowTemplate>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                                    <px:PXDropDown CommitChanges="True" ID="edPOAdjdOrderType" runat="server" AllowNull="False" DataField="OrderType" />
                                    <px:PXSelector CommitChanges="True" ID="edPOAdjdOrderNbr" runat="server" DataField="AdjdOrderNbr" AutoRefresh="true" AllowEdit="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="detgrid4" Name="POAdjust.adjdOrderType" PropertyName="DataValues[&quot;AdjdOrderType&quot;]" />
                                        </Parameters>
                                    </px:PXSelector>
									<px:PXSelector CommitChanges="True" ID="edPOAdjustAdjdRefNbr" runat="server" DataField="AdjdRefNbr" AutoRefresh="true" AllowEdit="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="detgrid4" Name="POAdjust.AdjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" />
                                        </Parameters>
									</px:PXSelector>
                                    <px:PXTextEdit ID="edPOOrder__Status" runat="server" DataField="POOrder__Status" Enabled="False" />
                                    <px:PXNumberEdit ID="edPOCuryAdjgAmt" runat="server" DataField="CuryAdjgAmt" />
                                    <px:PXNumberEdit ID="edPOCuryAdjgBilledAmt" runat="server" DataField="CuryAdjgBilledAmt" />
                                    <px:PXDateTimeEdit ID="edPOAdjdOrderDate" runat="server" DataField="POOrder__OrderDate" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjdOrderType" CommitChanges="true" />
									<px:PXGridColumn DataField="AdjdOrderNbr" CommitChanges="true" />
									<px:PXGridColumn DataField="AdjdRefNbr" CommitChanges="true" />
									<px:PXGridColumn DataField="POOrder__Status" />
									<px:PXGridColumn DataField="CuryAdjgAmt" />
									<px:PXGridColumn DataField="CuryAdjgBilledAmt" />
									<px:PXGridColumn DataField="POOrder__OrderDate" />
									<px:PXGridColumn DataField="POOrder__CuryUnprepaidTotal" />
									<px:PXGridColumn DataField="POOrder__CuryLineTotal" />
									<px:PXGridColumn DataField="POOrder__CuryID" />
									<px:PXGridColumn DataField="Released" Type="CheckBox" />
									<px:PXGridColumn DataField="IsRequest" Type="CheckBox" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Load Orders" Tooltip="Load Orders">
									<AutoCallBack Command="LoadPOOrders" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode InitNewRow="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Approval Details" BindingContext="form" RepaintOnDemand="false">
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
									<px:PXGridColumn DataField="Reason" AllowUpdate="False" />
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
			<px:PXTabItem Text="Remittance Information">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
					<px:PXFormView ID="Remittance_Contact" runat="server" Caption="Remittance Contact" DataMember="Remittance_Contact" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="chkOverrideContact" runat="server" CommitChanges="True" DataField="OverrideContact" SuppressLabel="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" CommandSourceID="ds" DataField="Email" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="Remittance_Address" runat="server" Caption="Remittance Address" DataMember="Remittance_Address" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="chkOverrideAddress" runat="server" CommitChanges="True" DataField="OverrideAddress" SuppressLabel="True" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AutoRefresh="True" DataField="CountryID" CommitChanges="true" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Print Options" StartGroup="True" StartColumn="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkPrintCheck" runat="server" DataField="PrintCheck" Size="SM" AlignLeft="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Finance Charges" RepaintOnDemand="false" >
				<Template>
					<px:PXGrid ID="detgrid3" runat="server" Height="300px" SkinID="DetailsInTab" Style="z-index: 100;" TabIndex="30500" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="PaymentCharges" DataKeyNames="DocType,RefNbr,LineNbr">
								<RowTemplate>
									<px:PXSelector ID="edEntryTypeID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="EntryTypeID" />
									<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" Enabled="False" AllowEdit="False" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" Enabled="False" AllowEdit="False" />
									<px:PXNumberEdit ID="edCuryTranAmt" runat="server" CommitChanges="true" DataField="CuryTranAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AutoCallBack="True" DataField="EntryTypeID" />
									<px:PXGridColumn DataField="TranDesc" />
									<px:PXGridColumn DataField="AccountID" CommitChanges="true" />
									<px:PXGridColumn DataField="SubID" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" AutoCallBack="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Joint Payees" BindingContext="Form" VisibleExp="DataControls[&quot;chkIsJointCheck&quot;].Value = 1">
                <Template>
                    <px:PXGrid runat="server" SyncPosition="True" KeepPosition="True" Height="300px" SkinID="DetailsInTab" Width="100%" ID="grdJointPayees" DataSourceID="ds" AllowPaging="True" PageSize="12" NoteIndicator="False" FilesIndicator="False">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <CallbackCommands>
                            <InitRow CommitChanges="True" />
                        </CallbackCommands>
                        <Mode InitNewRow="True" AllowAddNew="False" AllowDelete="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="JointPayeePayments">
                                <Columns>
                                    <px:PXGridColumn DataField="JointPayee__JointPayeeInternalId" CommitChanges="True" DisplayMode="Hint" />
                                    <px:PXGridColumn DataField="JointPayee__JointPayeeExternalName" CommitChanges="True" />
                                    <px:PXGridColumn DataField="JointAmountToPay" Required="true" />
                                    <px:PXGridColumn DataField="JointPayee__JointBalance" CommitChanges="True" />
                                    <px:PXGridColumn DataField="JointPayee__JointAmountPaid" CommitChanges="True" />
                                    <px:PXGridColumn DataField="JointPayee__JointAmountOwed" Required="false" CommitChanges="True" />
                                    <px:PXGridColumn DataField="InvoiceRefNbr" LinkCommand="ViewApInvoice" />
                                    <px:PXGridColumn DataField="JointPayee__BillLineNumber" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXNumberEdit runat="server" ID="PXNumberEdit6" DataField="JointAmountToPay" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                                <ExportExcel Enabled="False" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" ID="grdComplianceDocuments" Width="100%" Height="300px" AutoGenerateColumns="Append" DataSourceID="ds" KeepPosition="True" SyncPosition="True" SkinID="DetailsInTab" AllowPaging="True" PageSize="12">
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
                                    <px:PXGridColumn DataField="VendorName" LinkCommand="" TextAlign="Left" />
                                    <px:PXGridColumn DataField="BillID" LinkCommand="ComplianceDocument$BillID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" CommitChanges="" />
                                    <px:PXGridColumn DataField="ArPaymentMethodID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ApPaymentMethodID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ArPaymentID" LinkCommand="ComplianceDocument$ArPaymentID$Link" DisplayMode="Text" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InvoiceID" LinkCommand="ComplianceDocument$InvoiceID$Link" CommitChanges="True" DisplayMode="Text" />
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
                                    <px:PXGridColumn DataField="ApCheckID" LinkCommand="ComplianceDocument$ApCheckID$Link" DisplayMode="Text" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Policy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectTransactionID" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" CommitChanges="True" DisplayMode="Text" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="PurchaseOrder" TextAlign="Left" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
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
                        <CallbackCommands>
                            <InitRow CommitChanges="True" />
                        </CallbackCommands>
                    </px:PXGrid>
                </Template>
                <AutoCallBack>
                    <Behavior CommitChanges="True" />
                </AutoCallBack>
            </px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
    <px:PXSmartPanel ID="PanelLoadPOOrders" runat="server" Caption="Load Orders" CaptionVisible="true" LoadOnDemand="true" Key="LoadOrders"
		AutoCallBack-Enabled="true" AutoCallBack-Target="PXFormLoadPOOrders" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
		<px:PXFormView ID="PXFormLoadPOOrders" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="LoadOrders">
			<ContentStyle BackColor="Transparent" BorderStyle="None" Width="100%" Height="100%"  CssClass="" /> 
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="X" ControlSize="S" />
				<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" />
				<px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" />
				<px:PXSelector ID="edStartOrderNbr" runat="server" DataField="StartOrderNbr" />
				<px:PXNumberEdit ID="edMaxNumberOfDocuments" runat="server" DataField="MaxNumberOfDocuments" />

                <px:PXGroupBox RenderStyle="RoundBorder" ID="gpOrder" runat="server" Caption="Order by"
                    DataField="OrderBy" Width="300px">
                    <Template>
                        <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="M" ControlSize="XM" />
                        <px:PXRadioButton runat="server" ID="rModeOrderDate" Value="1" Text="Order Date, Order Nbr." />
                        <px:PXRadioButton runat="server" ID="rModeOrderNbr" Value="2" Text="Order Nbr." />
                    </Template>
                </px:PXGroupBox>

				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="X" ControlSize="S" />
				<px:PXDateTimeEdit ID="edToDate" runat="server" DataField="ToDate" />
				<px:PXSelector ID="edEndOrderNbr" runat="server" DataField="EndOrderNbr" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanelReasonButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnReasonOk" runat="server" Text="OK" DialogResult="Yes" CommandSourceID="ds" />
			<px:PXButton ID="btnReasonCancel" runat="server" Text="Cancel" DialogResult="No" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>	
</asp:Content>
