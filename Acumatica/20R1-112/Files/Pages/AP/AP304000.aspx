<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP304000.aspx.cs" Inherits="Page_AP304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:pxdatasource EnableAttributes="true" id="ds" runat="server" visible="True" width="100%" typename="PX.Objects.AP.APQuickCheckEntry" primaryview="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Prebook" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewBatch" />
			<px:PXDSCallbackCommand Name="Action" CommitChanges="True" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="False" Name="PrintCheck" />
			<px:PXDSCallbackCommand Visible="False" Name="ReclassifyBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewVoucherBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewWorkBook" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewSchedule" Visible="False">
			</px:PXDSCallbackCommand>
		    <px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument"/>
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
		</CallbackCommands>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:pxformview id="form" runat="server" style="z-index: 100" width="100%" datamember="Document" caption="Document Summary" noteindicator="True" filesindicator="True" activityindicator="True" activityfield="NoteActivity"
		linkindicator="True" notifyindicator="True" defaultcontrolid="edDocType" markrequired="Dynamic">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" >
				<GridProperties FastFilterFields="ExtRefNbr, VendorID, VendorID_Vendor_acctName" />
			</px:PXSelector>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" Size="Empty" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edAdjDate" runat="server" DataField="AdjDate" />
			<px:PXSelector CommitChanges="True" ID="edAdjFinPeriodID" runat="server" DataField="AdjFinPeriodID" AutoRefresh="True"/>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowAddNew="True" AllowEdit="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
			<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_APQuickCheck_CurrencyInfo_" DataMember="_Currency_" />
			<px:PXTextEdit CommitChanges="True" ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryOrigWhTaxAmt" runat="server" DataField="CuryOrigWhTaxAmt" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryRoundDiff" runat="server" DataField="CuryRoundDiff" Enabled="False" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
			<px:PXNumberEdit ID="edCuryTaxAmt" runat="server" CommitChanges="True" DataField="CuryTaxAmt" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDiscAmt" runat="server" DataField="CuryOrigDiscAmt" />
			<px:PXNumberEdit ID="edCuryChargeAmt" runat="server" DataField="CuryChargeAmt" Enabled="False" />
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
							<div id="div3" class="Field1"><%# ((PXGridCellContainer)Container).Text("vendorID_Vendor_acctName") %></div>
						</CellTemplate>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:pxgrid>

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:pxtab id="tab" runat="server" height="300px" style="z-index: 100;" width="100%" datamember="CurrentDocument" datasourceid="ds">
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" SyncPosition="true" Style="z-index: 100;" Height="300px" Width="100%" SkinID="DetailsInTab" TabIndex="-26036" DataSourceID="ds">
						<Levels>
							<px:PXGridLevel DataMember="Transactions" DataKeyNames="TranType,RefNbr,LineNbr">
								<Columns>
									<px:PXGridColumn DataField="BranchID" AutoCallBack="True" RenderEditorText="True"/>
									<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
									<px:PXGridColumn DataField="TranDesc" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="UOM" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="ManualPrice" TextAlign="Center" AllowNull="False" Type="CheckBox" CommitChanges="True"/> 
									<px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID_Account_description" />
									<px:PXGridColumn DataField="SubID" />
									<px:PXGridColumn AutoCallBack="True" DataField="ProjectID" Label="Project" />
									<px:PXGridColumn AutoCallBack="True" DataField="TaskID" Label="Task" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
									<px:PXGridColumn DataField="NonBillable" Label="Non Billable" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Box1099" RenderEditorText="True"/>
									<px:PXGridColumn DataField="DefScheduleID" AutoCallBack="True" />
									<px:PXGridColumn DataField="DeferredCode" />
									<px:PXGridColumn DataField="TaxCategoryID" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
									<px:PXSelector CommitChanges="True" ID="edUOM" AutoRefresh="true" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
									<px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" CommitChanges="true" />
									<px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
									<px:PXNumberEdit ID="edCuryLineAmt" runat="server" DataField="CuryLineAmt" CommitChanges="true" />
									<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" />
									<px:PXDropDown ID="edBox1099" runat="server" DataField="Box1099" />
									<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" />
									<px:PXSelector ID="edDefScheduleID" runat="server" DataField="DefScheduleID" AutoRefresh="True" />
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
									<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
									<px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="CostCodeID" AutoRefresh="true" />
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="View Schedule" CommandName="ViewSchedule" CommandSourceID="ds" />
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="GL Link" StartGroup="True" />
					<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
					<px:PXSelector ID="edPrebookBatchNbr" runat="server" DataField="PrebookBatchNbr" Enabled="False" AllowEdit="True" />
					<px:PXSelector ID="edVoidBatchNbr" runat="server" DataField="VoidBatchNbr" Enabled="False" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXSegmentMask ID="edAPAccountID" runat="server" DataField="APAccountID" CommitChanges="True" />
					<px:PXSegmentMask ID="edAPSubID" runat="server" DataField="APSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edPrebookAcctID" runat="server" DataField="PrebookAcctID" CommitChanges="True" DataSourceID="ds" />
					<px:PXSegmentMask ID="edPrebookSubID" runat="server" DataField="PrebookSubID" AutoRefresh="True" DataSourceID="ds" />
					<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
					<px:PXSelector CommitChanges="True" Size="S" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkCleared" runat="server" DataField="Cleared" />
					<px:PXDateTimeEdit CommitChanges="True" ID="edClearDate" runat="server" DataField="ClearDate" />
				    <px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True">
				        <LinkCommand Target="ds" Command="ViewOriginalDocument"/>
				    </px:PXTextEdit>

					<px:PXLayoutRule runat="server" ControlSize="XM" GroupCaption="Tax and Terms" LabelsWidth="SM" StartColumn="True" StartGroup="True" />
					<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
					<px:PXDropDown runat="server" ID="edTaxCalcMode" DataField="TaxCalcMode" CommitChanges="true" />
					<px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" />
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
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" Style="z-index: 100;" Width="100%" SkinID="DetailsInTab" Height="300px" DataSourceID="ds" TabIndex="3700">
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<Search Enabled="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<Columns>
									<px:PXGridColumn DataField="TaxID" CommitChanges="true"/>
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxUOM" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxableQty" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryExpenseAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="Tax__TaxType" />
									<px:PXGridColumn DataField="Tax__PendingTax" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Tax__ReverseTax" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Tax__ExemptTax" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Tax__StatisticalTax" TextAlign="Center" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
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
					<px:PXFormView ID="Remittance_Contact" runat="server" Caption="Remittance Contact" DataMember="Remittance_Contact" RenderStyle="Fieldset" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="chkOverrideContact" runat="server" CommitChanges="True" DataField="OverrideContact" SuppressLabel="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" CommandSourceID="ds" DataField="Email" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="Remittance_Address" runat="server" Caption="Remittance Address" DataMember="Remittance_Address" RenderStyle="Fieldset" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="chkOverrideAddress" runat="server" CommitChanges="True" DataField="OverrideAddress" SuppressLabel="True" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AutoRefresh="True" DataField="CountryID" CommitChanges="True" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" GroupCaption="Print Options" StartGroup="True" StartColumn="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkPrintCheck" runat="server" DataField="PrintCheck" Size="SM" AlignLeft="true" />
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
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:pxtab>
	<!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
