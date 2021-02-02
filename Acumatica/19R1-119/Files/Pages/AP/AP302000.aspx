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
			<px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="ReverseApplication" DependOnGrid="detgrid2" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewApplicationDocument" DependOnGrid="detgrid2" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewDocumentToApply" DependOnGrid="detgrid" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewCurrentBatch" DependOnGrid="detgrid2" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="False" Name="PrintCheck" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="false" CommitChanges="True" />
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
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
			<px:PXNumberEdit ID="edCuryUnappliedBal" runat="server" DataField="CuryUnappliedBal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryInitDocBal" runat="server" DataField="CuryInitDocBal" CommitChanges="True" />
			<px:PXNumberEdit ID="edCuryApplAmt" runat="server" DataField="CuryApplAmt" Enabled="False" />
			<px:PXNumberEdit ID="edCuryChargeAmt" runat="server" DataField="CuryChargeAmt" Enabled="False" />
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
					<px:PXGrid ID="detgrid" runat="server" Height="300px" SkinID="DetailsInTab" Style="z-index: 100;" TabIndex="30500" Width="100%" AdjustPageSize="Auto" AllowPaging="True">
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
											<px:PXControlParam ControlID="detgrid" Name="APAdjust.adjdRefNbr" PropertyName="DataValues[&quot;AdjdRefNbr&quot;]" Type="String" />
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
									<px:PXGridColumn DataField="AdjdBranchID" Width="81px" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjdDocType" Width="100px" Type="DropDownList" AutoCallBack="True" />
									<px:PXGridColumn AutoCallBack="True" DataField="AdjdRefNbr" Width="100px" LinkCommand="ViewDocumentToApply" />
									<px:PXGridColumn AutoCallBack="True" DataField="AdjdLineNbr" />

									<px:PXGridColumn DataField="APTran__InventoryID" />
									<px:PXGridColumn DataField="APTran__ProjectID" />
									<px:PXGridColumn DataField="APTran__TaskID" />
									<px:PXGridColumn DataField="APTran__CostCodeID" />
									<px:PXGridColumn DataField="APTran__AccountID" />

									<px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgPPDAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgWhTaxAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="AdjdDocDate" Width="100px" />
									<px:PXGridColumn DataField="APInvoice__DueDate" Width="100px" />
									<px:PXGridColumn DataField="APInvoice__DiscDate" Width="100px" />
									<px:PXGridColumn AutoCallBack="True" DataField="AdjdCuryRate" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryWhTaxBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="APInvoice__DocDesc" Width="200px" />
									<px:PXGridColumn DataField="AdjdCuryID" Width="50px" />
									<px:PXGridColumn DataField="AdjdFinPeriodID" />
									<px:PXGridColumn DataField="APInvoice__InvoiceNbr" Width="90px" />
									<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" Width="220px" DisplayMode="Hint"/>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Load Documents" Tooltip="Load Documents">
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
					<px:PXGrid ID="detgrid2" runat="server" Height="300px" Style="z-index: 100" Width="100%" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Adjustments_History" DataKeyNames="AdjgDocType,AdjgRefNbr,AdjdDocType,AdjdRefNbr,AdjdLineNbr,AdjNbr">
								<Mode AllowAddNew="False" AllowDelete="False" />
								<Columns>
									<px:PXGridColumn DataField="AdjdBranchID" Width="81px" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjBatchNbr" Width="100px" LinkCommand="ViewCurrentBatch" />
									<px:PXGridColumn DataField="DisplayDocType" Width="100px" Type="DropDownList" />
									<px:PXGridColumn DataField="DisplayRefNbr" Width="100px" LinkCommand="ViewApplicationDocument" />
									<px:PXGridColumn DataField="AdjdLineNbr" />

								    <px:PXGridColumn DataField="APTran__InventoryID" />
								    <px:PXGridColumn DataField="APTran__ProjectID" />
								    <px:PXGridColumn DataField="APTran__TaskID" />
								    <px:PXGridColumn DataField="APTran__CostCodeID" />
								    <px:PXGridColumn DataField="APTran__AccountID" />

									<px:PXGridColumn DataField="DisplayCuryAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="DisplayCuryPPDAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="DisplayCuryWhTaxAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="AdjgFinPeriodID" />
									<px:PXGridColumn DataField="AdjdDocDate" Width="100px" />
									<px:PXGridColumn DataField="APInvoice__DueDate" Width="100px" />
									<px:PXGridColumn DataField="APInvoice__DiscDate" Width="100px" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="DisplayDocDesc" Width="200px" />
									<px:PXGridColumn DataField="DisplayCuryID" Width="50px" />
									<px:PXGridColumn DataField="AdjdFinPeriodID" />
									<px:PXGridColumn DataField="APInvoice__InvoiceNbr" Width="100px" />
									<px:PXGridColumn DataField="TaxInvoiceNbr" Width="100px" />
									<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" Width="220px" DisplayMode="Hint"/>
									<px:PXGridColumn DataField="PendingPPD" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="PPDDebitAdjRefNbr" LinkCommand="ViewPPDDebitAdj"  Width="108px" />
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
			<px:PXTabItem Text="Approval Details" BindingContext="form" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" Style="left: 0; top: 0;">
						<AutoSize Enabled="True" />
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<Levels>
							<px:PXGridLevel DataMember="Approval">
								<Columns>
									<px:PXGridColumn DataField="ApproverEmployee__AcctCD" Width="160px" />
									<px:PXGridColumn DataField="ApproverEmployee__AcctName" Width="160px" />
									<px:PXGridColumn DataField="WorkgroupID" Width="150px" />
									<px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" Width="100px" />
									<px:PXGridColumn DataField="ApprovedByEmployee__AcctName" Width="160px" />
									<px:PXGridColumn DataField="ApproveDate" Width="90px" />
									<px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
									<px:PXGridColumn DataField="Reason" AllowUpdate="False" Width="160px" />
									<px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false" Width="160px"/>
									<px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" Width="160px" />
									<px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" Width="160px" />
									<px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" Width="100px" />
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
									<px:PXGridColumn AutoCallBack="True" DataField="EntryTypeID" Width="100px" />
									<px:PXGridColumn DataField="TranDesc" Width="160px" />
									<px:PXGridColumn DataField="AccountID" Width="115px" />
									<px:PXGridColumn DataField="SubID" Width="130px" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" AutoCallBack="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
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
	<!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
