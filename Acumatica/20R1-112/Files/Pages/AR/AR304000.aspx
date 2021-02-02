<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR304000.aspx.cs" Inherits="Page_AR304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:pxdatasource EnableAttributes="true" id="ds" runat="server" visible="True" width="100%" typename="PX.Objects.AR.ARCashSaleEntry" primaryview="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="VoidCheck" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ReclassifyBatch" />
			<px:PXDSCallbackCommand Name="Action" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewSchedule" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument"/>
			<px:PXDSCallbackCommand Visible="false" Name="ViewVoucherBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewWorkBook" />
            <px:PXDSCallbackCommand Visible="False" CommitChanges="true" Name="ViewExternalTransaction" DependOnGrid="grdCCProcTran" />
			<px:PXDSCallbackCommand Visible="False" Name="CustomerDocuments" />
			<px:PXDSCallbackCommand Visible="False" Name="SendARInvoiceMemo" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="CaptureCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="AuthorizeCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="VoidCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="CreditCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="RecordCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="CaptureOnlyCCPayment" />
			<px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="ValidateCCPayment" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:pxformview id="form" runat="server" style="z-index: 100" width="100%" datamember="Document" caption="Invoice Summary" noteindicator="True"
		filesindicator="True" activityindicator="True" activityfield="NoteActivity" linkindicator="True" notifyindicator="True"
		defaultcontrolid="edDocType" tabindex="100" markrequired="Dynamic">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>

		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" SelectedIndex="-1" />
				<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True">
					 <GridProperties FastFilterFields="ExtRefNbr, CustomerID, CustomerID_Customer_acctName" />
				</px:PXSelector>
				<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
				<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
				<px:PXDateTimeEdit CommitChanges="True" Size="s" ID="edAdjDate" runat="server" DataField="AdjDate" />
				<px:PXSelector CommitChanges="True" ID="edAdjFinPeriodID" runat="server" DataField="AdjFinPeriodID" AutoRefresh="True"/>
				<px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" CommitChanges="True" />
				<px:PXDateTimeEdit ID="edDepositAfter" runat="server" DataField="DepositAfter" />

			<px:PXLayoutRule runat="server" ColumnSpan="2" />
				<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowAddNew="True" AllowEdit="True" />
				<px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" AutoRefresh="True" DataField="CustomerLocationID" />
				<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AutoRefresh="True" />
				<px:PXSelector CommitChanges="True" ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr" AutoRefresh="True" />
				<px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="CCPaymentStateDescr" Enabled="False" />	
				<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
				<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_ARCashSale_CurrencyInfo_" DataMember="_Currency_"/>
			    <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryRoundDiff" runat="server" DataField="CuryRoundDiff" Enabled="False" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDiscAmt" runat="server" DataField="CuryOrigDiscAmt" />
				<px:PXCheckBox ID="chkIsCCPayment" runat="server" DataField="IsCCPayment" />
				<px:PXNumberEdit ID="edCuryChargeAmt" runat="server" DataField="CuryChargeAmt" Enabled="False" />
				<px:PXNumberEdit ID="edCuryConsolidateChargeTotal" runat="server" DataField="CuryConsolidateChargeTotal" Enabled="False" />
				<px:PXSelector ID="edRefTranExtNbr" runat="server" DataField="RefTranExtNbr" DataSourceID="ds" />
		</Template>
	</px:pxformview>

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

	<px:pxgrid id="docsTemplate" runat="server" visible="false">
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
	</px:pxgrid>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:pxtab id="tab" runat="server" height="300px" style="z-index: 100;" width="100%" tabindex="200" datamember="CurrentDocument">
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="300" DataSourceID="ds" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="Transactions">
								<Columns>
									<px:PXGridColumn DataField="BranchID" AutoCallBack="True" RenderEditorText="True"/>
									<px:PXGridColumn DataField="InventoryID" AutoCallBack="True" RenderEditorText="True" />
									<px:PXGridColumn DataField="TranDesc" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="UOM" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="ManualPrice" TextAlign="Center" AllowNull="False" Type="CheckBox" CommitChanges="True"/>
									<px:PXGridColumn DataField="CuryExtPrice" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscPct" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="ManualDisc" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="SubID" />
									<px:PXGridColumn DataField="TaskID" Label="Task" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
									<px:PXGridColumn DataField="SalesPersonID" RenderEditorText="True" />
									<px:PXGridColumn DataField="DefScheduleID" AutoCallBack="True" />
									<px:PXGridColumn DataField="DeferredCode" />
									<px:PXGridColumn DataField="DRTermStartDate" CommitChanges="true" />
									<px:PXGridColumn DataField="DRTermEndDate" CommitChanges="true" />
									<px:PXGridColumn DataField="TaxCategoryID" />
									<px:PXGridColumn DataField="Commissionable" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="LineNbr" Label="Line Nbr." TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="CuryUnitPriceDR" AllowShowHide="Server" />
									<px:PXGridColumn DataField="DiscPctDR" AllowShowHide="Server" />
								</Columns>

								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
										<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
										<px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" />
										<px:PXNumberEdit CommitChanges="True" ID="edQty" runat="server" DataField="Qty" />
									<px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" CommitChanges="true" />
									<px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
									<px:PXNumberEdit ID="edCuryExtPrice" runat="server" DataField="CuryExtPrice" CommitChanges="true" />
										<px:PXNumberEdit ID="edDiscPct" runat="server" DataField="DiscPct" />
										<px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" />
										<px:PXCheckBox ID="chkManualDisc" runat="server" DataField="ManualDisc" />
										<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" Enabled="False" />

									<px:PXLayoutRule runat="server" ColumnSpan="2" />
										<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />

									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
										<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
										<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
										<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" />
										<px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
										<px:PXCheckBox CommitChanges="True" ID="chkCommissionable" runat="server" Checked="True" DataField="Commissionable" />
										<px:PXSelector CommitChanges="True" ID="edDefScheduleID" runat="server" DataField="DefScheduleID" AutoRefresh="True" />
										<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" />
										<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
										<px:PXSegmentMask ID="edTaskID" runat="server" AutoRefresh="True" DataField="TaskID" />
                                        <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="CostCodeID" AutoRefresh="true" />
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>

						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" />

						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="View Schedule" Key="cmdViewSchedule">
									<AutoCallBack Command="ViewSchedule" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
					<px:PXLayoutRule runat="server" GroupCaption="Link to GL" StartGroup="True" />
						<px:PXSelector ID="edBatchNbr" runat="server" AllowEdit="True" DataField="BatchNbr" DataSourceID="ds" Enabled="False" />
						<px:PXSegmentMask ID="edBranchID" runat="server" CommitChanges="True" DataField="BranchID" DataSourceID="ds" />
						<px:PXSegmentMask ID="edARAccountID" runat="server" CommitChanges="True" DataField="ARAccountID" DataSourceID="ds" />
						<px:PXSegmentMask ID="edARSubID" runat="server" AutoRefresh="True" DataField="ARSubID" DataSourceID="ds" />
						<px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True" >
							 <LinkCommand Target="ds" Command="ViewOriginalDocument"/>
						</px:PXTextEdit>

					<px:PXLayoutRule runat="server" GroupCaption="Payment Info" StartGroup="True" />
						<px:PXSelector ID="edTermsID" runat="server" CommitChanges="True" DataField="TermsID" DataSourceID="ds" />
						<px:PXCheckBox ID="edCleared" runat="server" CommitChanges="True" DataField="Cleared" />
						<px:PXDateTimeEdit ID="edClearDate" runat="server" CommitChanges="True" DataField="ClearDate" />
						<px:PXCheckBox ID="chkDepositAsBatch" runat="server" CommitChanges="True" DataField="DepositAsBatch" />
						<px:PXCheckBox ID="chkDeposited" runat="server" DataField="Deposited" />
						<px:PXDateTimeEdit ID="edDepositDate" runat="server" DataField="DepositDate" Enabled="False" />
						<px:PXTextEdit ID="edDepositNbr" runat="server" DataField="DepositNbr" />

					<px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Tax Info" LabelsWidth="SM" StartColumn="True" StartGroup="True" />
						<px:PXSelector ID="edTaxZoneID" runat="server" CommitChanges="True" DataField="TaxZoneID" />
					<px:PXDropDown ID="exTaxCalcMode" runat="server" CommitChanges="True" DataField="TaxCalcMode" />
						<px:PXDropDown ID="edAvalaraCustomerUsageTypeID" runat="server" CommitChanges="True" DataField="AvalaraCustomerUsageType" />

					<px:PXLayoutRule runat="server" GroupCaption="Assigned to" StartGroup="True" />
						<px:PXTreeSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_"
							 TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False">
							<DataBindings>
								<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
							</DataBindings>
						</px:PXTreeSelector>
						<px:PXSelector ID="edOwnerID" runat="server" AutoGenerateColumn="false" AutoRefresh="True" CommitChanges="True" DataField="OwnerID" DataSourceID="ds"/>

					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Print and Email Options" StartGroup="True" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
					<px:PXCheckBox ID="chkPrinted" runat="server" DataField="Printed" Enabled="False" Size="SM" AlignLeft="true" />
					<px:PXCheckBox ID="chkDontPrint" runat="server" DataField="DontPrint" Size="SM" AlignLeft="true" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
					<px:PXCheckBox ID="chkEmailed" runat="server" DataField="Emailed" Enabled="False" Size="SM" AlignLeft="true" />
					<px:PXCheckBox ID="chkDontEmail" runat="server" DataField="DontEmail" Size="SM" AlignLeft="true" />

					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" GroupCaption="Voucher Details" />
						<px:PXFormView ID="VoucherDetails" runat="server" RenderStyle="Simple" DataMember="Voucher" DataSourceID="ds" TabIndex="1100">
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
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
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
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
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
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="350px" SkinID="DetailsInTab" Style="z-index: 100" TabIndex="600" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<RowTemplate>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
									<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" SuppressLabel="True" CommitChanges="true" AutoRefresh="true"/>
									<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" SuppressLabel="True" />
									<px:PXNumberEdit ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" SuppressLabel="True" />
									<px:PXNumberEdit ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" SuppressLabel="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="TaxID" CommitChanges="true"/>
									<px:PXGridColumn AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryExemptedAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxUOM" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxableQty" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="Tax__TaxType" />
									<px:PXGridColumn DataField="Tax__PendingTax" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Tax__ReverseTax" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Tax__ExemptTax" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Tax__StatisticalTax" TextAlign="Center" Type="CheckBox" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
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
					<px:PXGrid ID="grdSalesPerTrans" runat="server" BorderWidth="0px" Height="200px" SkinID="Details" Width="100%" TabIndex="400" DataSourceID="ds">
						<Levels>
							<px:PXGridLevel DataMember="salesPerTrans">
								<RowTemplate>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
									<px:PXNumberEdit ID="edCommnPct" runat="server" DataField="CommnPct" />
									<px:PXNumberEdit ID="edCommnAmt" runat="server" DataField="CommnAmt" />
									<px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" />
									<px:PXNumberEdit ID="edCommnblAmt" runat="server" DataField="CommnblAmt" />
									<px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" />
									<px:PXSegmentMask ID="edSalesPersonID_1" runat="server" AutoRefresh="True" DataField="SalespersonID" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AutoCallBack="True" DataField="SalespersonID" />
									<px:PXGridColumn DataField="CommnPct" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="CuryCommnAmt" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="CuryCommnblAmt" TextAlign="Right" />
									<px:PXGridColumn AllowShowHide="False" DataField="AdjdDocType" Visible="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Finance Charges" RepaintOnDemand="false" >
				<Template>
					<px:PXGrid ID="detgrid3" runat="server" Height="300px" SkinID="DetailsInTab" Style="z-index: 100;" TabIndex="30500" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="PaymentCharges" DataKeyNames="DocType,RefNbr,LineNbr">
								<RowTemplate>
									<px:PXSelector ID="edEntryTypeID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="EntryTypeID" />
									<px:PXSegmentMask ID="edChargeAccountID" runat="server" DataField="AccountID" Enabled="False" AllowEdit="False" />
									<px:PXSegmentMask ID="edChargeSubID" runat="server" DataField="SubID" Enabled="False" AllowEdit="False" />
									<px:PXNumberEdit ID="edChargeCuryTranAmt" runat="server" CommitChanges="true" DataField="CuryTranAmt" />
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
			<px:PXTabItem Text="Credit Card Processing Info" BindingContext="form" VisibleExp="DataControls[&quot;chkIsCCPayment&quot;].Value = 1" RepaintOnDemand = "false">
				<Template>
					<px:PXGrid ID="grdCCProcTran" runat="server" Height="120px" Width="100%" SkinID="DetailsInTab" TabIndex="700" BorderWidth="0px" DataSourceID="ds">
						<Levels>
							<px:PXGridLevel DataMember="ccProcTran" DataKeyNames="TranNbr">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXNumberEdit ID="edTranNbr" runat="server" DataField="TranNbr" />
									<px:PXDropDown ID="edProcStatus" runat="server" DataField="ProcStatus" />
									<px:PXTextEdit ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" />
									<px:PXDropDown ID="edCVVVerificationStatus" runat="server" DataField="CVVVerificationStatus" />
									<px:PXDropDown ID="edCCTranType" runat="server" DataField="TranType" />
									<px:PXDropDown ID="edTranStatus" runat="server" DataField="TranStatus" />
									<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
									<px:PXNumberEdit ID="edRefTranNbr" runat="server" DataField="RefTranNbr" />
									<px:PXTextEdit ID="edPCTranNumber" runat="server" DataField="PCTranNumber" />
									<px:PXTextEdit ID="edAuthNumber" runat="server" DataField="AuthNumber" />
									<px:PXTextEdit ID="edPCResponseReasonText" runat="server" DataField="PCResponseReasonText" />
									<px:PXDateTimeEdit ID="edStartTime" runat="server" DataField="StartTime" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TranNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="ProcessingCenterID" />
									<px:PXGridColumn DataField="TranType" />
									<px:PXGridColumn DataField="TranStatus" />
									<px:PXGridColumn DataField="Amount" TextAlign="Right" />
									<px:PXGridColumn DataField="RefTranNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="PCTranNumber" />
									<px:PXGridColumn DataField="AuthNumber" />
									<px:PXGridColumn DataField="PCResponseReasonText" />
									<px:PXGridColumn DataField="StartTime" />
									<px:PXGridColumn DataField="ProcStatus" />
									<px:PXGridColumn DataField="CVVVerificationStatus" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="50" MinWidth="50" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:pxtab>
	<!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
